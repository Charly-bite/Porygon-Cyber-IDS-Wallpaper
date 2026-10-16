using System;
using System.Diagnostics;
using System.Drawing;
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
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point Point);
    [DllImport("user32.dll")] private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

    [DllImport("gdi32.dll")] private static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);
    [DllImport("gdi32.dll")] private static extern int CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, int fnCombineMode);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")] private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left, Top, Right, Bottom;
    }

    private const int SW_HIDE = 0;
    private const int RGN_OR = 2;
    private const int VK_LBUTTON = 0x01;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const uint GW_OWNER = 4;

    private System.Windows.Forms.Timer pollTimer;
    private bool[,] cachedIconMask = null;
    private int cachedMaskW = 1920;
    private int cachedMaskH = 48;
    private int startSentinelX = -1;
    private int startSentinelY = 18;
    private int lastTaskbarWinSig = -1;
    private bool wasHoveringClock = false;
    private bool pendingMaskRefresh = false;
    private DateTime pendingRefreshTime = DateTime.MinValue;
    private DateTime lastRebuildTime = DateTime.MinValue;
    private int tickCount = 0;

    public TrayAutoHideContext() {
        StopConflictingTaskbarTools();
        ConfigureTaskbarRegistrySettings();
        EnforceSingleMonitorTaskbars();
        lastTaskbarWinSig = ComputeTaskbarWindowSignature();
        RebuildIconSilhouetteMask();

        pollTimer = new System.Windows.Forms.Timer();
        pollTimer.Interval = 45; // 22 Hz responsive hover & window-change detection
        pollTimer.Tick += OnPollTick;
        pollTimer.Start();
    }

    private static void StopConflictingTaskbarTools() {
        try {
            // Stop TranslucentTB if running so its maximized_window_appearance (e.g. #B8070913 red bar) never overrides Shell_TrayWnd
            Process[] ttb = Process.GetProcessesByName("TranslucentTB");
            for (int i = 0; i < ttb.Length; i++) {
                try { ttb[i].Kill(); } catch { }
            }
            int selfId = Process.GetCurrentProcess().Id;
            Process[] tah = Process.GetProcessesByName("TrayAutoHide");
            for (int i = 0; i < tah.Length; i++) {
                if (tah[i].Id != selfId) {
                    try { tah[i].Kill(); } catch { }
                }
            }
        } catch { }
    }

    private static void ConfigureTaskbarRegistrySettings() {
        try {
            // 1. Keep taskbar ONLY on Primary Monitor (disable multi-monitor taskbars)
            using (RegistryKey advKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true)) {
                if (advKey != null) {
                    object mmVal = advKey.GetValue("MMTaskbarEnabled");
                    if (mmVal == null || Convert.ToInt32(mmVal) != 0) {
                        advKey.SetValue("MMTaskbarEnabled", 0, RegistryValueKind.DWord);
                    }
                    advKey.SetValue("TaskbarDa", 0, RegistryValueKind.DWord);
                }
            }
            // 2. Ensure uniform dark background RGB(28,28,28) on Shell_TrayWnd (no accent color / transparency) so icon silhouette extraction is 100% crisp
            using (RegistryKey persKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true)) {
                if (persKey != null) {
                    persKey.SetValue("ColorPrevalence", 0, RegistryValueKind.DWord);
                    persKey.SetValue("EnableTransparency", 0, RegistryValueKind.DWord);
                    persKey.SetValue("SystemUsesLightTheme", 0, RegistryValueKind.DWord);
                }
            }
            IntPtr res;
            SendMessageTimeout(new IntPtr(0xFFFF), 0x001A, IntPtr.Zero, "TraySettings", 0x0002, 500, out res);
            SendMessageTimeout(new IntPtr(0xFFFF), 0x001A, IntPtr.Zero, "ImmersiveColorSet", 0x0002, 500, out res);
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

    private static int ComputeTaskbarWindowSignature() {
        int count = 0;
        int xorHash = 0;
        EnumWindows((hWnd, lParam) => {
            if (IsWindowVisible(hWnd)) {
                if (GetWindow(hWnd, GW_OWNER) == IntPtr.Zero) {
                    int ex = GetWindowLong(hWnd, GWL_EXSTYLE);
                    if ((ex & WS_EX_TOOLWINDOW) == 0) {
                        count++;
                        xorHash ^= (hWnd.ToInt32() * 31);
                    }
                }
            }
            return true;
        }, IntPtr.Zero);
        return (count << 20) ^ (xorHash & 0xFFFFF);
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

    private bool IsTaskbarOccludedByWindow(IntPtr tray, int trayW, int trayTop) {
        IntPtr fg = GetForegroundWindow();
        if (fg == IntPtr.Zero || fg == tray || IsChild(tray, fg)) return false;

        StringBuilder sb = new StringBuilder(64);
        GetClassName(fg, sb, sb.Capacity);
        string cls = sb.ToString();
        if (cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd" || cls == "Shell_SecondaryTrayWnd") {
            return false;
        }

        RECT r;
        if (GetWindowRect(fg, out r)) {
            // Normal maximized windows stop at trayTop (e.g. r.Bottom <= trayTop + 2).
            // Fullscreen apps or windows overlapping the taskbar have r.Bottom > trayTop + 8.
            if (r.Bottom > trayTop + 8 && r.Right > 260 && r.Left < trayW - 260) {
                return true;
            }
        }
        return false;
    }

    private bool IsStartButtonSentinelValid(IntPtr tray, int trayW, int trayTop) {
        if (startSentinelX <= 0) return true;
        if (IsTaskbarOccludedByWindow(tray, trayW, trayTop)) return true;

        IntPtr hdc = GetDC(IntPtr.Zero);
        if (hdc == IntPtr.Zero) return true;
        try {
            uint px = GetPixel(hdc, startSentinelX, trayTop + startSentinelY);
            if (px == 0xFFFFFFFF) return true;
            int r = (int)(px & 0xFF);
            int g = (int)((px >> 8) & 0xFF);
            int b = (int)((px >> 16) & 0xFF);
            // Windows 11 Start button top-left square is bright cyan-blue (B > 175, G > 105, R < 100)
            return (b > 175 && g > 105 && r < 100);
        } finally {
            ReleaseDC(IntPtr.Zero, hdc);
        }
    }

    private void RebuildIconSilhouetteMask() {
        try {
            IntPtr tray;
            int trayW, trayH, trayTop, notifyLeft;
            GetTrayGeometry(out tray, out trayW, out trayH, out trayTop, out notifyLeft);
            if (tray == IntPtr.Zero || trayW <= 0 || trayH <= 0) return;

            if (cachedIconMask != null && IsTaskbarOccludedByWindow(tray, trayW, trayTop)) {
                pendingMaskRefresh = true;
                pendingRefreshTime = DateTime.UtcNow.AddMilliseconds(600);
                return;
            }

            int scanLeft = 260;
            int scanRight = Math.Min(trayW - 40, notifyLeft - 14);
            if (scanRight <= scanLeft + 60) scanRight = trayW - 230;

            // Temporarily set region to the centered icon strip so Windows renders fresh unclipped icon pixels
            IntPtr stripRgn = CreateRectRgn(scanLeft, 1, scanRight, trayH);
            SetWindowRgn(tray, stripRgn, true);
            Thread.Sleep(70);

            // Verify that Shell_TrayWnd is actually visible at the center of the strip (not covered by a fullscreen app or flyout)
            IntPtr hMid = WindowFromPoint(new Point(trayW / 2, trayTop + 20));
            if (cachedIconMask != null && hMid != IntPtr.Zero && hMid != tray && !IsChild(tray, hMid)) {
                ApplyCurrentRegion(tray, wasHoveringClock, notifyLeft, trayW, trayH);
                pendingMaskRefresh = true;
                pendingRefreshTime = DateTime.UtcNow.AddMilliseconds(600);
                return;
            }

            using (Bitmap bmp = new Bitmap(trayW, trayH)) {
                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.CopyFromScreen(0, trayTop, 0, 0, new Size(trayW, trayH));
                }

                // Sample uniform taskbar background color inside the strip (or fallback to dark RGB 28,28,28)
                Color bg = Color.FromArgb(28, 28, 28);
                Color sampleL = bmp.GetPixel(Math.Min(trayW - 1, scanLeft + 4), Math.Min(trayH - 1, 8));
                Color sampleR = bmp.GetPixel(Math.Max(0, scanRight - 6), Math.Min(trayH - 1, 8));
                bool validBgL = (sampleL.R >= 14 && sampleL.R <= 46 && Math.Abs(sampleL.R - sampleL.G) <= 8 && Math.Abs(sampleL.G - sampleL.B) <= 8);
                bool validBgR = (sampleR.R >= 14 && sampleR.R <= 46 && Math.Abs(sampleR.R - sampleR.G) <= 8 && Math.Abs(sampleR.G - sampleR.B) <= 8);
                if (validBgL) {
                    bg = sampleL;
                }

                bool[,] raw = new bool[trayW, trayH];
                int foundStartX = -1;
                int activeIconPixels = 0;

                // 1. Scan icon body band (y = 12 .. 35), excluding active-button highlight box borders
                int iconYMin = 12;
                int iconYMax = Math.Min(trayH - 8, 36);
                for (int x = scanLeft; x < scanRight; x++) {
                    // Check if this column is a vertical border of an active-app highlight box (present at y=9)
                    Color topCheck = bmp.GetPixel(x, 9);
                    int topDiff = Math.Abs(topCheck.R - bg.R) + Math.Abs(topCheck.G - bg.G) + Math.Abs(topCheck.B - bg.B);
                    bool isHighlightBoxBorderCol = (topDiff > 24 && topCheck.R < 88 && Math.Abs(topCheck.R - topCheck.G) <= 6 && Math.Abs(topCheck.G - topCheck.B) <= 6);

                    for (int y = iconYMin; y < iconYMax; y++) {
                        Color c = bmp.GetPixel(x, y);
                        int diff = Math.Abs(c.R - bg.R) + Math.Abs(c.G - bg.G) + Math.Abs(c.B - bg.B);
                        bool isNeutralGray = (Math.Abs(c.R - c.G) <= 5 && Math.Abs(c.G - c.B) <= 5);

                        if (isHighlightBoxBorderCol && isNeutralGray && c.R < 90) {
                            continue;
                        }
                        // Colored icon pixels need diff > 48; neutral gray pixels need diff > 74 (to skip RGB 45,45,45 active button fill)
                        int thresh = isNeutralGray ? 74 : 48;
                        if (diff > thresh) {
                            raw[x, y] = true;
                            activeIconPixels++;
                            if (foundStartX == -1 && y >= 16 && y <= 20 && c.B > 205 && c.G > 130 && c.R < 75) {
                                foundStartX = x + 3;
                            }
                        }
                    }
                }

                int totalBandPixels = Math.Max(1, (scanRight - scanLeft) * (iconYMax - iconYMin));
                bool validCapture = validBgL && validBgR && (foundStartX != -1) && (activeIconPixels < (totalBandPixels * 38 / 100));
                if (!validCapture && cachedIconMask != null) {
                    // Reject contaminated capture (e.g. fullscreen overlay or accent flash) so a solid bar is never created
                    ApplyCurrentRegion(tray, wasHoveringClock, notifyLeft, trayW, trayH);
                    pendingMaskRefresh = true;
                    pendingRefreshTime = DateTime.UtcNow.AddMilliseconds(600);
                    return;
                }

                // 2. Scan bottom running-app indicator pills (y = 40 .. 44)
                int pillYMin = Math.Min(trayH - 4, 40);
                int pillYMax = Math.Min(trayH - 1, 45);
                for (int y = pillYMin; y < pillYMax; y++) {
                    for (int x = scanLeft; x < scanRight; x++) {
                        Color c = bmp.GetPixel(x, y);
                        int diff = Math.Abs(c.R - bg.R) + Math.Abs(c.G - bg.G) + Math.Abs(c.B - bg.B);
                        if (diff > 85) {
                            raw[x, y] = true;
                        }
                    }
                }

                // 3. 1px dilation for crisp anti-aliased icon borders (only inside icon/pill bands)
                bool[,] dilated = new bool[trayW, trayH];
                for (int y = iconYMin; y < pillYMax; y++) {
                    for (int x = scanLeft; x < scanRight; x++) {
                        if (raw[x, y]) {
                            for (int dy = -1; dy <= 1; dy++) {
                                for (int dx = -1; dx <= 1; dx++) {
                                    int nx = x + dx, ny = y + dy;
                                    if (nx >= scanLeft && nx < scanRight && ny >= 11 && ny < trayH - 1) {
                                        dilated[nx, ny] = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // 4. Bridge narrow internal gaps inside icons ONLY if the pixel is not dark background RGB(28,28,28)
                for (int x = scanLeft; x < scanRight; x++) {
                    int yFirst = -1, yLast = -1;
                    for (int y = iconYMin; y < iconYMax; y++) {
                        if (dilated[x, y]) {
                            if (yFirst == -1) yFirst = y;
                            yLast = y;
                        }
                    }
                    if (yFirst != -1 && yLast > yFirst && (yLast - yFirst) <= 23) {
                        for (int y = yFirst; y <= yLast; y++) {
                            Color c = bmp.GetPixel(x, y);
                            int diff = Math.Abs(c.R - bg.R) + Math.Abs(c.G - bg.G) + Math.Abs(c.B - bg.B);
                            // Keep interior of Windows Terminal (RGB ~51,51,51, diff ~69) and Search lens (RGB ~68,68,68, diff ~120),
                            // but never fill pure RGB(28,28,28) gaps!
                            if (diff > 32) {
                                dilated[x, y] = true;
                            }
                        }
                    }
                }

                if (foundStartX != -1) {
                    startSentinelX = foundStartX;
                    startSentinelY = 18;
                }

                cachedIconMask = dilated;
                cachedMaskW = trayW;
                cachedMaskH = trayH;
                lastRebuildTime = DateTime.UtcNow;
            }

            ApplyCurrentRegion(tray, wasHoveringClock, notifyLeft, trayW, trayH);
        } catch { }
    }

    private void ApplyCurrentRegion(IntPtr tray, bool includeClock, int notifyLeft, int trayW, int trayH) {
        if (tray == IntPtr.Zero || cachedIconMask == null) return;

        IntPtr masterRgn = CreateRectRgn(0, 0, 0, 0);
        int w = cachedMaskW;
        int h = cachedMaskH;

        for (int y = 10; y < h - 1; y++) {
            int x = 0;
            while (x < w) {
                while (x < w && !cachedIconMask[x, y]) x++;
                if (x >= w) break;
                int xStart = x;
                while (x < w && cachedIconMask[x, y]) x++;
                IntPtr span = CreateRectRgn(xStart, y, x, y + 1);
                CombineRgn(masterRgn, masterRgn, span, RGN_OR);
                DeleteObject(span);
            }
        }

        if (includeClock) {
            int pillLeft = Math.Max(1500, notifyLeft - 8);
            IntPtr clockPill = CreateRoundRectRgn(pillLeft, 4, trayW - 4, trayH - 4, 12, 12);
            CombineRgn(masterRgn, masterRgn, clockPill, RGN_OR);
            DeleteObject(clockPill);
        }

        if (SetWindowRgn(tray, masterRgn, true) == 0) {
            DeleteObject(masterRgn);
        }
    }

    private void OnPollTick(object sender, EventArgs e) {
        tickCount++;
        if (tickCount % 30 == 0) {
            EnforceSingleMonitorTaskbars();
        }

        IntPtr tray;
        int trayW, trayH, trayTop, notifyLeft;
        GetTrayGeometry(out tray, out trayW, out trayH, out trayTop, out notifyLeft);
        if (tray == IntPtr.Zero) return;

        bool occluded = IsTaskbarOccludedByWindow(tray, trayW, trayTop);
        Point cursor = Cursor.Position;
        bool mouseInTaskbarBand = !occluded && (cursor.Y >= trayTop - 6 && cursor.Y <= trayTop + trayH + 8 && cursor.X >= 0 && cursor.X <= trayW);

        // 1. Detect clicks on taskbar icons so we refresh the icon silhouette once the cursor moves away
        if (mouseInTaskbarBand && (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0) {
            pendingMaskRefresh = true;
            pendingRefreshTime = DateTime.UtcNow.AddMilliseconds(450);
        }

        // 2. Every ~225ms, check if any top-level taskbar window opened/closed (e.g. Snipping Tool, browser, dialog)
        //    OR if the Windows Start button sentinel pixel shifted horizontally due to icon re-centering!
        if (tickCount % 5 == 0) {
            int sig = ComputeTaskbarWindowSignature();
            if (sig != lastTaskbarWinSig) {
                lastTaskbarWinSig = sig;
                pendingMaskRefresh = true;
                // Wait 320ms for Windows 11 centered taskbar slide animation to finish
                pendingRefreshTime = DateTime.UtcNow.AddMilliseconds(320);
            } else if (!occluded && !pendingMaskRefresh && (DateTime.UtcNow - lastRebuildTime).TotalMilliseconds > 900) {
                if (!IsStartButtonSentinelValid(tray, trayW, trayTop)) {
                    pendingMaskRefresh = true;
                    pendingRefreshTime = DateTime.UtcNow.AddMilliseconds(220);
                }
            }
        }

        if (!occluded && pendingMaskRefresh && DateTime.UtcNow >= pendingRefreshTime) {
            pendingMaskRefresh = false;
            RebuildIconSilhouetteMask();
            return;
        }

        // 3. Hover over bottom-right clock & system tray zone reveals the clock
        int clockHoverLeft = Math.Max(1650, notifyLeft - 14);
        bool hoverClock = !occluded && (cursor.X >= clockHoverLeft && cursor.X <= trayW + 4 && cursor.Y >= trayTop - 8 && cursor.Y >= 0 && cursor.Y <= trayTop + trayH + 8);

        if (hoverClock != wasHoveringClock) {
            wasHoveringClock = hoverClock;
            ApplyCurrentRegion(tray, wasHoveringClock, notifyLeft, trayW, trayH);
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
