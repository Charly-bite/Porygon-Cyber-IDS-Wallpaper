using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

class TrayAutoHide {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindowEx(IntPtr hParent, IntPtr hChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("psapi.dll")]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int X, Y;
    }

    const int SW_HIDE = 0;
    const int SW_SHOWNA = 8;
    const uint RDW_INVALIDATE = 0x0001;
    const uint RDW_ERASE = 0x0004;
    const uint RDW_ALLCHILDREN = 0x0080;
    const uint RDW_UPDATENOW = 0x0100;
    const uint RDW_FRAME = 0x0400;

    static void SetTrayVisibility(IntPtr hShellTray, IntPtr hTrayNotify, bool visible) {
        int cmd = visible ? SW_SHOWNA : SW_HIDE;
        ShowWindow(hTrayNotify, cmd);
        EnumChildWindows(hTrayNotify, (hChild, lParam) => {
            ShowWindow(hChild, cmd);
            return true;
        }, IntPtr.Zero);
        RedrawWindow(hShellTray, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
    }

    static void Main(string[] args) {
        bool createdNew;
        using (Mutex mutex = new Mutex(true, "Global\\AntigravityTrayAutoHideMutex", out createdNew)) {
            if (!createdNew) return;

            try {
                EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            } catch { }

            int screenW = GetSystemMetrics(0);
            int screenH = GetSystemMetrics(1);
            int triggerLeft = screenW - 280;
            int triggerTop = screenH - 48;

            bool currentlyHidden = false;
            DateTime lastHoverTime = DateTime.MinValue;

            while (true) {
                IntPtr hShellTray = FindWindow("Shell_TrayWnd", null);
                if (hShellTray != IntPtr.Zero) {
                    IntPtr hTrayNotify = FindWindowEx(hShellTray, IntPtr.Zero, "TrayNotifyWnd", null);
                    if (hTrayNotify != IntPtr.Zero) {
                        if (IsWindowVisible(hTrayNotify)) {
                            RECT r;
                            if (GetWindowRect(hTrayNotify, out r)) {
                                int w = r.Right - r.Left;
                                int h = r.Bottom - r.Top;
                                if (w >= 60 && w <= 600 && h >= 20 && r.Right > screenW / 2) {
                                    triggerLeft = r.Left - 20;
                                    triggerTop = r.Top - 8;
                                }
                            }
                        }

                        POINT pt;
                        if (GetCursorPos(out pt)) {
                            bool inHoverZone = (pt.X >= triggerLeft && pt.Y >= triggerTop);
                            bool inKeepAliveZone = (pt.X >= triggerLeft - 45 && pt.Y >= triggerTop - 35);
                            bool mousePressed = ((GetAsyncKeyState(0x01) & 0x8000) != 0) || ((GetAsyncKeyState(0x02) & 0x8000) != 0);

                            if (inHoverZone) {
                                lastHoverTime = DateTime.UtcNow;
                                if (currentlyHidden || !IsWindowVisible(hTrayNotify)) {
                                    SetTrayVisibility(hShellTray, hTrayNotify, true);
                                    currentlyHidden = false;
                                }
                            } else {
                                if (!currentlyHidden && inKeepAliveZone) {
                                    lastHoverTime = DateTime.UtcNow;
                                }
                                double elapsedMs = (DateTime.UtcNow - lastHoverTime).TotalMilliseconds;
                                if (!currentlyHidden && !inKeepAliveZone && !mousePressed && elapsedMs > 800) {
                                    SetTrayVisibility(hShellTray, hTrayNotify, false);
                                    currentlyHidden = true;
                                } else if (currentlyHidden && IsWindowVisible(hTrayNotify)) {
                                    // Keep it hidden if Explorer/StartAllBack re-showed it while cursor is away
                                    SetTrayVisibility(hShellTray, hTrayNotify, false);
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(75);
            }
        }
    }
}
