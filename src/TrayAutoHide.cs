using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

public class TrayAutoHideContext : ApplicationContext {
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")] private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

    [DllImport("gdi32.dll")] private static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left, Top, Right, Bottom;
    }

    private const int SW_HIDE = 0;

    private System.Windows.Forms.Timer pollTimer;
    private bool wasHoveringClock = false;
    private int lastNotifyLeft = -1;
    private int lastTrayW = -1;
    private int lastTrayH = -1;
    private int tickCount = 0;

    public TrayAutoHideContext() {
        StopOtherTrayAutoHideInstances();
        ConfigureTaskbarRegistrySettings();
        EnsureTranslucentTaskbarClear();
        EnforceSingleMonitorTaskbars();

        IntPtr tray;
        int trayW, trayH, trayTop, notifyLeft;
        GetTrayGeometry(out tray, out trayW, out trayH, out trayTop, out notifyLeft);
        if (tray != IntPtr.Zero) {
            ApplyClockAutoHideRegion(tray, false, notifyLeft, trayW, trayH);
        }

        pollTimer = new System.Windows.Forms.Timer();
        pollTimer.Interval = 45; // 22 Hz responsive clock-hover detection
        pollTimer.Tick += OnPollTick;
        pollTimer.Start();
    }

    private static void StopOtherTrayAutoHideInstances() {
        try {
            int selfId = Process.GetCurrentProcess().Id;
            Process[] tah = Process.GetProcessesByName("TrayAutoHide");
            for (int i = 0; i < tah.Length; i++) {
                if (tah[i].Id != selfId) {
                    try { tah[i].Kill(); } catch { }
                }
            }
        } catch { }
    }

    private static void EnsureTranslucentTaskbarClear() {
        try {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string ttbDir = Path.Combine(localAppData, "Programs", "TranslucentTB");
            string settingsPath = Path.Combine(ttbDir, "settings.json");
            string ttbExe = Path.Combine(ttbDir, "TranslucentTB.exe");

            if (Directory.Exists(ttbDir)) {
                string cleanJson = @"{
  ""$schema"": ""https://TranslucentTB.github.io/settings.schema.json"",
  ""desktop_appearance"": {
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0
  },
  ""visible_window_appearance"": {
    ""enabled"": false,
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0,
    ""rules"": { ""window_class"": {}, ""window_title"": {}, ""process_name"": {} }
  },
  ""maximized_window_appearance"": {
    ""enabled"": false,
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0,
    ""rules"": { ""window_class"": {}, ""window_title"": {}, ""process_name"": {} }
  },
  ""start_opened_appearance"": {
    ""enabled"": false,
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0
  },
  ""search_opened_appearance"": {
    ""enabled"": false,
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0
  },
  ""task_view_opened_appearance"": {
    ""enabled"": false,
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0
  },
  ""battery_saver_appearance"": {
    ""enabled"": false,
    ""accent"": ""clear"",
    ""color"": ""#00000000"",
    ""show_peek"": false,
    ""show_line"": false,
    ""blur_radius"": 9.0
  },
  ""ignored_windows"": { ""window_class"": [], ""window_title"": [], ""process_name"": [] },
  ""hide_tray"": true,
  ""disable_saving"": false,
  ""verbosity"": ""warn""
}";
                bool needsRestart = false;
                if (File.Exists(settingsPath)) {
                    string existing = File.ReadAllText(settingsPath, Encoding.UTF8);
                    if (existing.IndexOf("#B8070913", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        existing.IndexOf("\"acrylic\"", StringComparison.OrdinalIgnoreCase) >= 0) {
                        File.WriteAllText(settingsPath, cleanJson, Encoding.UTF8);
                        needsRestart = true;
                    }
                } else {
                    File.WriteAllText(settingsPath, cleanJson, Encoding.UTF8);
                    needsRestart = true;
                }

                if (File.Exists(ttbExe)) {
                    Process[] running = Process.GetProcessesByName("TranslucentTB");
                    if (needsRestart && running.Length > 0) {
                        for (int i = 0; i < running.Length; i++) {
                            try { running[i].Kill(); } catch { }
                        }
                        Thread.Sleep(200);
                        running = new Process[0];
                    }
                    if (running.Length == 0) {
                        ProcessStartInfo psi = new ProcessStartInfo(ttbExe);
                        psi.WorkingDirectory = ttbDir;
                        psi.UseShellExecute = true;
                        Process.Start(psi);
                    }
                }
            }
        } catch { }
    }

    private static void ConfigureTaskbarRegistrySettings() {
        try {
            using (RegistryKey advKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true)) {
                if (advKey != null) {
                    object mmVal = advKey.GetValue("MMTaskbarEnabled");
                    if (mmVal == null || Convert.ToInt32(mmVal) != 0) {
                        advKey.SetValue("MMTaskbarEnabled", 0, RegistryValueKind.DWord);
                    }
                    advKey.SetValue("TaskbarDa", 0, RegistryValueKind.DWord);
                }
            }
            using (RegistryKey persKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true)) {
                if (persKey != null) {
                    persKey.SetValue("ColorPrevalence", 0, RegistryValueKind.DWord);
                    persKey.SetValue("SystemUsesLightTheme", 0, RegistryValueKind.DWord);
                }
            }
            IntPtr res;
            SendMessageTimeout(new IntPtr(0xFFFF), 0x001A, IntPtr.Zero, "TraySettings", 0x0002, 500, out res);
        } catch { }
    }

    private static void EnforceSingleMonitorTaskbars() {
        IntPtr secTray = IntPtr.Zero;
        while ((secTray = FindWindowEx(IntPtr.Zero, secTray, "Shell_SecondaryTrayWnd", null)) != IntPtr.Zero) {
            if (IsWindowVisible(secTray)) {
                ShowWindow(secTray, SW_HIDE);
            }
        }
    }

    private void GetTrayGeometry(out IntPtr tray, out int trayW, out int trayH, out int trayTop, out int notifyLeft) {
        tray = FindWindow("Shell_TrayWnd", null);
        Rectangle pri = Screen.PrimaryScreen.Bounds;
        trayW = pri.Width;
        trayH = 48;
        trayTop = pri.Bottom - 48;
        notifyLeft = pri.Width - 220;

        if (tray != IntPtr.Zero) {
            RECT tr;
            if (GetWindowRect(tray, out tr) && tr.Right > tr.Left && tr.Bottom > tr.Top) {
                trayW = tr.Right - tr.Left;
                trayH = tr.Bottom - tr.Top;
                trayTop = tr.Top;
            }
            IntPtr notifyWnd = FindWindowEx(tray, IntPtr.Zero, "TrayNotifyWnd", null);
            if (notifyWnd != IntPtr.Zero) {
                RECT nr;
                if (GetWindowRect(notifyWnd, out nr) && nr.Left > 300) {
                    notifyLeft = nr.Left;
                }
            }
        }
    }

    private void ApplyClockAutoHideRegion(IntPtr tray, bool includeClock, int notifyLeft, int trayW, int trayH) {
        if (tray == IntPtr.Zero) return;

        lastNotifyLeft = notifyLeft;
        lastTrayW = trayW;
        lastTrayH = trayH;

        if (includeClock) {
            // Unclip entire taskbar when hovering bottom-right so clock and system tray icons render natively
            SetWindowRgn(tray, IntPtr.Zero, true);
        } else {
            // Keep 0 .. notifyLeft-8 completely unclipped from y=0..trayH+4 so centered taskbar icons NEVER get clipped,
            // while cleanly cutting out the right-hand system tray & clock zone until hovered
            int clipRight = Math.Max(400, notifyLeft - 8);
            IntPtr rgn = CreateRectRgn(0, 0, clipRight, trayH + 4);
            if (SetWindowRgn(tray, rgn, true) == 0) {
                DeleteObject(rgn);
            }
        }
    }

    private void OnPollTick(object sender, EventArgs e) {
        tickCount++;
        if (tickCount % 30 == 0) {
            EnforceSingleMonitorTaskbars();
        }
        if (tickCount % 200 == 0) {
            EnsureTranslucentTaskbarClear();
        }

        IntPtr tray;
        int trayW, trayH, trayTop, notifyLeft;
        GetTrayGeometry(out tray, out trayW, out trayH, out trayTop, out notifyLeft);
        if (tray == IntPtr.Zero) return;

        Point cursor = Cursor.Position;
        int clockHoverLeft = Math.Max(1650, notifyLeft - 14);
        bool hoverClock = (cursor.X >= clockHoverLeft && cursor.X <= trayW + 4 && cursor.Y >= trayTop - 8 && cursor.Y <= trayTop + trayH + 8);

        if (hoverClock != wasHoveringClock || notifyLeft != lastNotifyLeft || trayW != lastTrayW || trayH != lastTrayH) {
            wasHoveringClock = hoverClock;
            ApplyClockAutoHideRegion(tray, wasHoveringClock, notifyLeft, trayW, trayH);
        }
    }

    public static void Main() {
        try { SetProcessDpiAwarenessContext(new IntPtr(-4)); } catch { }
        bool createdNew;
        using (Mutex m = new Mutex(true, "Global\\PorygonCyberIDSTrayAutoHideMutex", out createdNew)) {
            if (!createdNew) return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayAutoHideContext());
        }
    }
}
