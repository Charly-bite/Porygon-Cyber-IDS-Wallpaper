using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public class HackerLockScreenForm : Form {
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetThreadDesktop(IntPtr hDesktop);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool CloseDesktop(IntPtr hDesktop);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, uint nLength, out uint lpnLengthNeeded);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("psapi.dll")]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

    const int GWL_EXSTYLE = -20;
    const int WS_EX_LAYERED = 0x00080000;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_NOACTIVATE = 0x08000000;
    const int WS_EX_TOOLWINDOW = 0x00000080;
    const int WS_EX_TOPMOST = 0x00000008;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_SHOWWINDOW = 0x0040;

    private int screenW, screenH;
    private System.Windows.Forms.Timer timer;
    private Random rng = new Random();
    private Font fontRainFg, fontRainBg, fontHud;
    private float radarAngle = 0f;
    private int tickCount = 0;

    private class RainCol {
        public float X, Y, Speed;
        public int Length;
        public char[] Chars;
        public int ColorType;
        public bool IsFg;
    }
    private List<RainCol> cols = new List<RainCol>();
    private const string GLYPHS = "0123456789ABCDEF<>[]{}/\\*+#$@%&";

    public HackerLockScreenForm() {
        Rectangle bounds = Screen.PrimaryScreen.Bounds;
        screenW = bounds.Width;
        screenH = bounds.Height;

        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Bounds = bounds;
        this.ShowInTaskbar = false;
        this.TopMost = true;
        this.BackColor = Color.FromArgb(1, 2, 5);
        this.TransparencyKey = Color.FromArgb(1, 2, 5);
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        fontRainFg = new Font("Consolas", 11.5f, FontStyle.Bold);
        fontRainBg = new Font("Consolas", 9.5f, FontStyle.Bold);
        fontHud = new Font("Consolas", 10f, FontStyle.Bold);

        int numCols = screenW / 24;
        for (int i = 0; i < numCols; i++) {
            float x = i * 24 + 8;
            // Leave center user selection / password box area (screenW/2 - 260 .. screenW/2 + 260) clear!
            if (Math.Abs(x - screenW / 2) < 250) continue;

            RainCol c = new RainCol();
            c.X = x;
            c.Y = rng.Next(-screenH, screenH);
            c.IsFg = rng.NextDouble() > 0.4;
            c.Speed = c.IsFg ? (float)(6.5 + rng.NextDouble() * 7.0) : (float)(3.5 + rng.NextDouble() * 4.0);
            c.Length = rng.Next(8, 20);
            c.Chars = new char[c.Length];
            for (int k = 0; k < c.Length; k++) c.Chars[k] = GLYPHS[rng.Next(GLYPHS.Length)];
            double p = rng.NextDouble();
            c.ColorType = (p < 0.55) ? 0 : (p < 0.82 ? 1 : 2);
            cols.Add(c);
        }

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 45;
        timer.Tick += OnTick;
    }

    protected override CreateParams CreateParams {
        get {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
            return cp;
        }
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        int ex = GetWindowLong(this.Handle, GWL_EXSTYLE);
        SetWindowLong(this.Handle, GWL_EXSTYLE, ex | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        timer.Start();
        try { EmptyWorkingSet(Process.GetCurrentProcess().Handle); } catch { }
    }

    private static bool IsWinlogonActive() {
        IntPtr hDesk = OpenInputDesktop(0, false, 0x0100);
        if (hDesk == IntPtr.Zero) return true; // Secure desktop active
        try {
            uint needed = 0;
            GetUserObjectInformation(hDesk, 2, IntPtr.Zero, 0, out needed);
            if (needed > 0) {
                IntPtr ptr = Marshal.AllocHGlobal((int)needed);
                try {
                    if (GetUserObjectInformation(hDesk, 2, ptr, needed, out needed)) {
                        string name = Marshal.PtrToStringAuto(ptr);
                        return string.Equals(name, "Winlogon", StringComparison.OrdinalIgnoreCase);
                    }
                } finally {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        } finally {
            CloseDesktop(hDesk);
        }
        return false;
    }

    private void OnTick(object sender, EventArgs e) {
        tickCount++;
        if (!IsWinlogonActive()) {
            // Sleep lightly when normal user desktop is active -> 0% CPU!
            return;
        }

        SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW);

        for (int i = 0; i < cols.Count; i++) {
            RainCol c = cols[i];
            c.Y += c.Speed;
            if (rng.NextDouble() < 0.25) c.Chars[rng.Next(c.Length)] = GLYPHS[rng.Next(GLYPHS.Length)];
            if (c.Y - c.Length * 16 > screenH) c.Y = rng.Next(-220, -20);
        }
        radarAngle = (radarAngle + 3.8f) % 360f;
        this.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) {
        Graphics g = e.Graphics;
        g.Clear(this.TransparencyKey);
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

        using (SolidBrush headBrush = new SolidBrush(Color.FromArgb(230, 252, 255)))
        using (SolidBrush cyanBrush = new SolidBrush(Color.FromArgb(0, 240, 255)))
        using (SolidBrush magBrush = new SolidBrush(Color.FromArgb(236, 72, 153)))
        using (SolidBrush vioBrush = new SolidBrush(Color.FromArgb(168, 85, 247))) {
            for (int i = 0; i < cols.Count; i++) {
                RainCol col = cols[i];
                Font f = col.IsFg ? fontRainFg : fontRainBg;
                Brush b = (col.ColorType == 0) ? cyanBrush : (col.ColorType == 1 ? magBrush : vioBrush);
                for (int c = 0; c < col.Length; c++) {
                    float cy = col.Y - c * 16;
                    if (cy < -20 || cy > screenH) continue;
                    if (c == 0 && col.IsFg) g.DrawString(col.Chars[c].ToString(), f, headBrush, col.X, cy);
                    else if (c < col.Length / 2 || c % 2 == 0) g.DrawString(col.Chars[c].ToString(), f, b, col.X, cy);
                }
            }

            // Draw HUD header & radar in top-left/top-right of Lock Screen
            g.DrawString(">> WINLOGON_GUARD // AUTH_GATEWAY_ONLINE [SESSION_LOCKED]", fontHud, cyanBrush, 36, 28);
            g.DrawString("ENCRYPTION: AES-256-GCM // ZERO_TRUST", fontHud, magBrush, screenW - 360, 28);
        }
    }

    [STAThread]
    public static void Main() {
        bool createdNew;
        using (Mutex m = new Mutex(true, "Global\\AntigravityHackerLockScreenMutex", out createdNew)) {
            if (!createdNew) return;
            IntPtr hWinlogon = OpenDesktop("Winlogon", 0, false, 0x01FF);
            if (hWinlogon != IntPtr.Zero) {
                SetThreadDesktop(hWinlogon);
            }
            Application.Run(new HackerLockScreenForm());
        }
    }
}
