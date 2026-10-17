using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Porygon Tray AutoHide Utility")]
[assembly: AssemblyDescription("Taskbar System Tray Auto-Hide and Desktop Wallpaper Supervisor")]
[assembly: AssemblyCompany("Charly-bite Visual Studio")]
[assembly: AssemblyProduct("Porygon 3D Wallpaper Suite")]
[assembly: AssemblyCopyright("Copyright (c) 2026")]
[assembly: AssemblyVersion("2.4.1.0")]
[assembly: AssemblyFileVersion("2.4.1.0")]

public class TrayAutoHideContext : ApplicationContext {
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")] private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
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
    private int tickCount = 0;

    public TrayAutoHideContext() {
        StopOtherTrayAutoHideInstances();
        ConfigureTaskbarRegistrySettings();
        EnsureTranslucentTaskbarClear();
        EnsureWallpaperRunning();
        EnforceSingleMonitorTaskbars();

        IntPtr tray, notifyWnd;
        int trayW, trayH, trayTop, notifyLeft;
        GetTrayGeometry(out tray, out notifyWnd, out trayW, out trayH, out trayTop, out notifyLeft);
        if (tray != IntPtr.Zero) {
            SetTrayVisibility(tray, notifyWnd, false);
        }

        pollTimer = new System.Windows.Forms.Timer();
        pollTimer.Interval = 45; // 22 Hz responsive clock-hover detection
        pollTimer.Tick += OnPollTick;
        pollTimer.Start();
    }

    private static void EnsureWallpaperRunning() {
        try {
            if (Process.GetProcessesByName("HackerWallpaper").Length == 0) {
                string wpExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HackerWallpaper.exe");
                if (File.Exists(wpExe)) {
                    ProcessStartInfo psi = new ProcessStartInfo(wpExe);
                    psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
        } catch { }
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

    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);
    private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

    private const int SW_SHOWNA = 8;
    private const uint RDW_INVALIDATE = 0x0001;
    private const uint RDW_ERASE = 0x0004;
    private const uint RDW_ALLCHILDREN = 0x0080;
    private const uint RDW_UPDATENOW = 0x0100;
    private const uint RDW_FRAME = 0x0400;

    private DateTime lastHoverTime = DateTime.MinValue;
    private int cachedNotifyLeft = 1640;

    private void GetTrayGeometry(out IntPtr tray, out IntPtr notifyWnd, out int trayW, out int trayH, out int trayTop, out int notifyLeft) {
        tray = FindWindow("Shell_TrayWnd", null);
        notifyWnd = IntPtr.Zero;
        Rectangle pri = Screen.PrimaryScreen.Bounds;
        trayW = pri.Width;
        trayH = 48;
        trayTop = pri.Bottom - 48;
        notifyLeft = cachedNotifyLeft;

        if (tray != IntPtr.Zero) {
            RECT tr;
            if (GetWindowRect(tray, out tr) && tr.Right > tr.Left && tr.Bottom > tr.Top) {
                trayW = tr.Right - tr.Left;
                trayH = tr.Bottom - tr.Top;
                trayTop = tr.Top;
            }
            notifyWnd = FindWindowEx(tray, IntPtr.Zero, "TrayNotifyWnd", null);
            if (notifyWnd != IntPtr.Zero) {
                RECT nr;
                if (GetWindowRect(notifyWnd, out nr)) {
                    int w = nr.Right - nr.Left;
                    if (nr.Left > 300 && w >= 40 && w <= 650) {
                        notifyLeft = nr.Left;
                        cachedNotifyLeft = nr.Left;
                    }
                }
            }
        }
    }

    private void SetTrayVisibility(IntPtr tray, IntPtr notifyWnd, bool visible) {
        if (tray == IntPtr.Zero) return;

        // Always ensure Shell_TrayWnd itself has no clipping region so centered app icons and tray render natively
        SetWindowRgn(tray, IntPtr.Zero, true);

        if (notifyWnd != IntPtr.Zero) {
            SetWindowRgn(notifyWnd, IntPtr.Zero, true);
            int cmd = visible ? SW_SHOWNA : SW_HIDE;
            ShowWindow(notifyWnd, cmd);
            EnumChildWindows(notifyWnd, (hChild, lParam) => {
                ShowWindow(hChild, cmd);
                return true;
            }, IntPtr.Zero);
            if (visible) {
                RedrawWindow(notifyWnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
            }
        }
        RedrawWindow(tray, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
    }

    private void OnPollTick(object sender, EventArgs e) {
        tickCount++;
        if (tickCount % 30 == 0) {
            EnforceSingleMonitorTaskbars();
        }
        if (tickCount == 15 || tickCount % 200 == 0) {
            EnsureWallpaperRunning();
            EnsureTranslucentTaskbarClear();
        }

        IntPtr tray, notifyWnd;
        int trayW, trayH, trayTop, notifyLeft;
        GetTrayGeometry(out tray, out notifyWnd, out trayW, out trayH, out trayTop, out notifyLeft);
        if (tray == IntPtr.Zero) return;

        Point cursor = Cursor.Position;
        int triggerLeft = Math.Max(1550, notifyLeft - 24);
        int triggerTop = trayTop - 10;

        bool inHoverZone = (cursor.X >= triggerLeft && cursor.X <= trayW + 8 && cursor.Y >= triggerTop && cursor.Y <= trayTop + trayH + 12);
        bool inKeepAliveZone = (cursor.X >= triggerLeft - 50 && cursor.X <= trayW + 12 && cursor.Y >= triggerTop - 40 && cursor.Y <= trayTop + trayH + 16);
        bool mousePressed = (Control.MouseButtons & (MouseButtons.Left | MouseButtons.Right)) != 0;

        if (inHoverZone) {
            lastHoverTime = DateTime.UtcNow;
            if (!wasHoveringClock || (notifyWnd != IntPtr.Zero && !IsWindowVisible(notifyWnd))) {
                wasHoveringClock = true;
                SetTrayVisibility(tray, notifyWnd, true);
            }
        } else {
            if (wasHoveringClock && inKeepAliveZone) {
                lastHoverTime = DateTime.UtcNow;
            }
            double elapsedMs = (DateTime.UtcNow - lastHoverTime).TotalMilliseconds;
            if (wasHoveringClock && !inKeepAliveZone && !mousePressed && elapsedMs > 800) {
                wasHoveringClock = false;
                SetTrayVisibility(tray, notifyWnd, false);
            } else if (!wasHoveringClock && notifyWnd != IntPtr.Zero && IsWindowVisible(notifyWnd)) {
                SetTrayVisibility(tray, notifyWnd, false);
            }
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
