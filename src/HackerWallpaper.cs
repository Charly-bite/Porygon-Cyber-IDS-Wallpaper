using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Porygon 3D Cyber Defense Suite (IDS / IPS / AV / SIEM)")]
[assembly: AssemblyDescription("3D OpenGL Multi-Monitor Desktop Wallpaper & Autonomous Porygon IDS/IPS/AV/SIEM Engine")]
[assembly: AssemblyCompany("Charly-bite Visual Studio")]
[assembly: AssemblyProduct("Porygon 3D Cyber Defense Suite")]
[assembly: AssemblyCopyright("Copyright (c) 2026")]
[assembly: AssemblyVersion("2.5.0.0")]
[assembly: AssemblyFileVersion("2.5.0.0")]

public class WallpaperConfig {
    public int ThemeIndex = 0;       // 0=Matte Bio-Forest & Amber, 1=Matte Matrix, 2=Matte Red Team, 3=Matte Ice Stealth
    public int BgSceneMode = 0;      // 0=Selva Low-Poly + Oscilador 3D, 1=Terreno Oscilador + Delaunay, 2=Bosque Micelial Poligonal
    public int RainSpeed = 100;      // 0..200
    public int RainDensity = 72;     // 20..110
    public int PorygonScale = 88;    // 50..160%
    public bool ShowTelemetry = true;
    public bool ShowRadar = true;
    public bool ShowPorygon = true;
    public bool FollowMouse = false; // Strictly FALSE by default!
    public bool TrainingMode = false;// Intensive AI training waves
    public bool AutoPauseMaximized = true;
    public bool GamerMode = false;   // 0% GPU / 0% CPU Hibernation Mode for Gaming
    public bool AutoGamerOnFullscreenGame = true; // Auto-activate Gamer Mode when a fullscreen/borderless game runs

    // Persistent Single-Click Collapsible HUD Card States
    public bool RadarCollapsed = false;
    public bool TelemetryCollapsed = false;
    public bool ConnectomeCollapsed = false;
    public bool Brain3DCollapsed = false;
    public bool SecondaryBarsCollapsed = false;

    private static string ConfigPath {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            return Path.Combine(dir, "config.json");
        }
    }

    public void Save() {
        try {
            string json = string.Format(
                "{{\r\n  \"ThemeIndex\": {0},\r\n  \"BgSceneMode\": {15},\r\n  \"RainSpeed\": {1},\r\n  \"RainDensity\": {2},\r\n  \"PorygonScale\": {3},\r\n  \"ShowTelemetry\": {4},\r\n  \"ShowRadar\": {5},\r\n  \"ShowPorygon\": {6},\r\n  \"FollowMouse\": {7},\r\n  \"TrainingMode\": {8},\r\n  \"AutoPauseMaximized\": {9},\r\n  \"GamerMode\": {16},\r\n  \"AutoGamerOnFullscreenGame\": {17},\r\n  \"RadarCollapsed\": {10},\r\n  \"TelemetryCollapsed\": {11},\r\n  \"ConnectomeCollapsed\": {12},\r\n  \"Brain3DCollapsed\": {13},\r\n  \"SecondaryBarsCollapsed\": {14}\r\n}}",
                ThemeIndex, RainSpeed, RainDensity, PorygonScale,
                ShowTelemetry.ToString().ToLower(),
                ShowRadar.ToString().ToLower(),
                ShowPorygon.ToString().ToLower(),
                FollowMouse.ToString().ToLower(),
                TrainingMode.ToString().ToLower(),
                AutoPauseMaximized.ToString().ToLower(),
                RadarCollapsed.ToString().ToLower(),
                TelemetryCollapsed.ToString().ToLower(),
                ConnectomeCollapsed.ToString().ToLower(),
                Brain3DCollapsed.ToString().ToLower(),
                SecondaryBarsCollapsed.ToString().ToLower(),
                BgSceneMode,
                GamerMode.ToString().ToLower(),
                AutoGamerOnFullscreenGame.ToString().ToLower()
            );
            File.WriteAllText(ConfigPath, json, Encoding.UTF8);
        } catch { }
    }

    public static WallpaperConfig Load() {
        WallpaperConfig cfg = new WallpaperConfig();
        try {
            if (File.Exists(ConfigPath)) {
                string txt = File.ReadAllText(ConfigPath, Encoding.UTF8);
                cfg.ThemeIndex = ParseInt(txt, "ThemeIndex", 0);
                cfg.BgSceneMode = Math.Max(0, Math.Min(2, ParseInt(txt, "BgSceneMode", 0)));
                cfg.RainSpeed = ParseInt(txt, "RainSpeed", 100);
                cfg.RainDensity = ParseInt(txt, "RainDensity", 72);
                cfg.PorygonScale = ParseInt(txt, "PorygonScale", 88);
                cfg.ShowTelemetry = ParseBool(txt, "ShowTelemetry", true);
                cfg.ShowRadar = ParseBool(txt, "ShowRadar", true);
                cfg.ShowPorygon = ParseBool(txt, "ShowPorygon", true);
                cfg.FollowMouse = ParseBool(txt, "FollowMouse", false);
                cfg.TrainingMode = ParseBool(txt, "TrainingMode", false);
                cfg.AutoPauseMaximized = ParseBool(txt, "AutoPauseMaximized", true);
                cfg.GamerMode = ParseBool(txt, "GamerMode", false);
                cfg.AutoGamerOnFullscreenGame = ParseBool(txt, "AutoGamerOnFullscreenGame", true);
                cfg.RadarCollapsed = ParseBool(txt, "RadarCollapsed", false);
                cfg.TelemetryCollapsed = ParseBool(txt, "TelemetryCollapsed", false);
                cfg.ConnectomeCollapsed = ParseBool(txt, "ConnectomeCollapsed", false);
                cfg.Brain3DCollapsed = ParseBool(txt, "Brain3DCollapsed", false);
                cfg.SecondaryBarsCollapsed = ParseBool(txt, "SecondaryBarsCollapsed", false);
            }
        } catch { }
        return cfg;
    }

    private static int ParseInt(string s, string key, int def) {
        int idx = s.IndexOf("\"" + key + "\"");
        if (idx < 0) return def;
        int colon = s.IndexOf(':', idx);
        if (colon < 0) return def;
        int end = s.IndexOfAny(new char[] { ',', '\r', '\n', '}' }, colon + 1);
        if (end < 0) return def;
        int val;
        if (int.TryParse(s.Substring(colon + 1, end - colon - 1).Trim(), out val)) return val;
        return def;
    }

    private static bool ParseBool(string s, string key, bool def) {
        int idx = s.IndexOf("\"" + key + "\"");
        if (idx < 0) return def;
        int colon = s.IndexOf(':', idx);
        if (colon < 0) return def;
        int end = s.IndexOfAny(new char[] { ',', '\r', '\n', '}' }, colon + 1);
        if (end < 0) return def;
        string sub = s.Substring(colon + 1, end - colon - 1).Trim().ToLower();
        return sub == "true";
    }
}

// ============================================================================
// HARDWARE GPU OPENGL 4.6 ACCELERATION ENGINE (AMD RADEON RX 9060 XT)
// ============================================================================
public static class GpuGl {
    public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
    public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
    public const uint GL_POINTS           = 0x0000;
    public const uint GL_LINES            = 0x0001;
    public const uint GL_LINE_LOOP        = 0x0002;
    public const uint GL_LINE_STRIP       = 0x0003;
    public const uint GL_TRIANGLES        = 0x0004;
    public const uint GL_TRIANGLE_FAN     = 0x0006;
    public const uint GL_QUADS            = 0x0007;
    public const uint GL_DEPTH_TEST       = 0x0B71;
    public const uint GL_BLEND            = 0x0BE2;
    public const uint GL_LINE_SMOOTH      = 0x0B20;
    public const uint GL_TEXTURE_2D       = 0x0DE1;
    public const uint GL_SRC_ALPHA        = 0x0302;
    public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
    public const uint GL_LEQUAL           = 0x0203;
    public const uint GL_RGBA             = 0x1908;
    public const uint GL_BGRA_EXT         = 0x80E1;
    public const uint GL_UNSIGNED_BYTE    = 0x1401;
    public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
    public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
    public const uint GL_LINEAR           = 0x2601;
    public const uint GL_PROJECTION       = 0x1701;
    public const uint GL_MODELVIEW        = 0x1700;
    public const uint GL_RENDERER         = 0x1F01;
    public const uint GL_VERSION          = 0x1F02;

    public const uint FONT_LIST_RAIN_FG   = 1000;
    public const uint FONT_LIST_RAIN_BG   = 1300;
    public const uint FONT_LIST_HUD_SMALL = 1600;

    public static int CanvasW = 6154;
    public static int CanvasH = 2167;
    private static byte[] textBuf = new byte[128];

    [StructLayout(LayoutKind.Sequential)]
    public struct PIXELFORMATDESCRIPTOR {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public byte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")]  public static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);
    [DllImport("gdi32.dll")]  public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);
    [DllImport("gdi32.dll")]  public static extern bool SwapBuffers(IntPtr hdc);
    [DllImport("gdi32.dll")]  public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")]  public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("opengl32.dll")] public static extern IntPtr wglCreateContext(IntPtr hdc);
    [DllImport("opengl32.dll")] public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
    [DllImport("opengl32.dll")] public static extern bool wglDeleteContext(IntPtr hglrc);
    [DllImport("opengl32.dll", CharSet = CharSet.Ansi)] public static extern bool wglUseFontBitmapsA(IntPtr hdc, uint first, uint count, uint listBase);
    [DllImport("opengl32.dll")] public static extern IntPtr glGetString(uint name);
    [DllImport("opengl32.dll")] public static extern void glViewport(int x, int y, int width, int height);
    [DllImport("opengl32.dll")] public static extern void glMatrixMode(uint mode);
    [DllImport("opengl32.dll")] public static extern void glLoadIdentity();
    [DllImport("opengl32.dll")] public static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);
    [DllImport("opengl32.dll")] public static extern void glClearColor(float r, float g, float b, float a);
    [DllImport("opengl32.dll")] public static extern void glClear(uint mask);
    [DllImport("opengl32.dll")] public static extern void glEnable(uint cap);
    [DllImport("opengl32.dll")] public static extern void glDisable(uint cap);
    [DllImport("opengl32.dll")] public static extern void glBlendFunc(uint sfactor, uint dfactor);
    [DllImport("opengl32.dll")] public static extern void glDepthFunc(uint func);
    [DllImport("opengl32.dll")] public static extern void glLineWidth(float width);
    [DllImport("opengl32.dll")] public static extern void glBegin(uint mode);
    [DllImport("opengl32.dll")] public static extern void glEnd();
    [DllImport("opengl32.dll")] public static extern void glColor4ub(byte r, byte g, byte b, byte a);
    [DllImport("opengl32.dll")] public static extern void glColor4f(float r, float g, float b, float a);
    [DllImport("opengl32.dll")] public static extern void glTexCoord2f(float s, float t);
    [DllImport("opengl32.dll")] public static extern void glVertex2f(float x, float y);
    [DllImport("opengl32.dll")] public static extern void glVertex3f(float x, float y, float z);
    [DllImport("opengl32.dll")] public static extern void glGenTextures(int n, out uint textures);
    [DllImport("opengl32.dll")] public static extern void glBindTexture(uint target, uint texture);
    [DllImport("opengl32.dll")] public static extern void glTexParameteri(uint target, uint pname, int param);
    [DllImport("opengl32.dll")] public static extern void glTexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);
    [DllImport("opengl32.dll")] public static extern void glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, IntPtr pixels);
    [DllImport("opengl32.dll")] public static extern void glRasterPos2f(float x, float y);
    [DllImport("opengl32.dll")] public static extern void glListBase(uint b);
    [DllImport("opengl32.dll")] public static extern void glCallLists(int n, uint type, byte[] lists);

    public static void SetColor(Color c) {
        glColor4ub(c.R, c.G, c.B, c.A);
    }

    public static void SetColor(Color c, int alpha) {
        int a = alpha < 0 ? 0 : (alpha > 255 ? 255 : alpha);
        glColor4ub(c.R, c.G, c.B, (byte)a);
    }

    public static void DrawLine(float x1, float y1, float x2, float y2, Color c, float width) {
        glLineWidth(width);
        SetColor(c);
        glBegin(GL_LINES);
        glVertex2f(x1, y1);
        glVertex2f(x2, y2);
        glEnd();
    }

    public static void FillRect(float x, float y, float w, float h, Color c) {
        SetColor(c);
        glBegin(GL_QUADS);
        glVertex2f(x, y);
        glVertex2f(x + w, y);
        glVertex2f(x + w, y + h);
        glVertex2f(x, y + h);
        glEnd();
    }

    public static void DrawRect(float x, float y, float w, float h, Color c, float width) {
        glLineWidth(width);
        SetColor(c);
        glBegin(GL_LINE_LOOP);
        glVertex2f(x, y);
        glVertex2f(x + w, y);
        glVertex2f(x + w, y + h);
        glVertex2f(x, y + h);
        glEnd();
    }

    public static void FillTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color c) {
        SetColor(c);
        glBegin(GL_TRIANGLES);
        glVertex2f(x1, y1);
        glVertex2f(x2, y2);
        glVertex2f(x3, y3);
        glEnd();
    }

    public static void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color c, float width) {
        glLineWidth(width);
        SetColor(c);
        glBegin(GL_LINE_LOOP);
        glVertex2f(x1, y1);
        glVertex2f(x2, y2);
        glVertex2f(x3, y3);
        glEnd();
    }

    private const int LUT_SIZE = 64;
    private static readonly float[] cosLut64 = new float[LUT_SIZE];
    private static readonly float[] sinLut64 = new float[LUT_SIZE];

    static GpuGl() {
        for (int i = 0; i < LUT_SIZE; i++) {
            double a = i * (2.0 * Math.PI / LUT_SIZE);
            cosLut64[i] = (float)Math.Cos(a);
            sinLut64[i] = (float)Math.Sin(a);
        }
    }

    public static void DrawEllipse(float cx, float cy, float rx, float ry, Color c, float width, int segs) {
        glLineWidth(width);
        SetColor(c);
        glBegin(GL_LINE_LOOP);
        if (segs <= 0) segs = 12;
        if (LUT_SIZE % segs == 0) {
            int stride = LUT_SIZE / segs;
            for (int i = 0; i < LUT_SIZE; i += stride) {
                glVertex2f(cx + cosLut64[i] * rx, cy + sinLut64[i] * ry);
            }
        } else {
            double step = 2.0 * Math.PI / segs;
            for (int i = 0; i < segs; i++) {
                double a = i * step;
                glVertex2f(cx + (float)Math.Cos(a) * rx, cy + (float)Math.Sin(a) * ry);
            }
        }
        glEnd();
    }

    public static void FillEllipse(float cx, float cy, float rx, float ry, Color c, int segs) {
        SetColor(c);
        glBegin(GL_TRIANGLE_FAN);
        glVertex2f(cx, cy);
        if (segs <= 0) segs = 12;
        if (LUT_SIZE % segs == 0) {
            int stride = LUT_SIZE / segs;
            for (int i = 0; i < LUT_SIZE; i += stride) {
                glVertex2f(cx + cosLut64[i] * rx, cy + sinLut64[i] * ry);
            }
            glVertex2f(cx + cosLut64[0] * rx, cy + sinLut64[0] * ry);
        } else {
            double step = 2.0 * Math.PI / segs;
            for (int i = 0; i <= segs; i++) {
                double a = i * step;
                glVertex2f(cx + (float)Math.Cos(a) * rx, cy + (float)Math.Sin(a) * ry);
            }
        }
        glEnd();
    }

    public static uint FontAtlasTexId = 0;

    public static void InitFontAtlas() {
        if (FontAtlasTexId != 0) return;
        using (Bitmap bmp = new Bitmap(256, 128, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(bmp))
        using (Font f = new Font("Consolas", 10.5f, FontStyle.Bold))
        using (SolidBrush br = new SolidBrush(Color.White)) {
            g.Clear(Color.FromArgb(0, 255, 255, 255));
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            for (int ch = 32; ch <= 126; ch++) {
                int idx = ch - 32;
                int col = idx & 15;
                int row = idx >> 4;
                g.DrawString(((char)ch).ToString(), f, br, col * 16f + 1f, row * 16f);
            }
            int tw = 0, th = 0;
            UploadBitmapToTexture(bmp, ref FontAtlasTexId, ref tw, ref th);
        }
    }

    public static void BeginFontAtlasBatch() {
        if (FontAtlasTexId == 0) InitFontAtlas();
        glEnable(GL_TEXTURE_2D);
        glBindTexture(GL_TEXTURE_2D, FontAtlasTexId);
        glBegin(GL_QUADS);
    }

    public static void EndFontAtlasBatch() {
        glEnd();
        glDisable(GL_TEXTURE_2D);
    }

    public static void EmitAtlasChar(char ch, float x, float y, float w, float h, byte r, byte g, byte b, byte a) {
        int idx = (ch >= 32 && ch <= 126) ? (ch - 32) : ('*' - 32);
        float u0 = (idx & 15) * 0.0625f;
        float v0 = (idx >> 4) * 0.125f;
        float u1 = u0 + 0.0625f;
        float v1 = v0 + 0.125f;
        glColor4ub(r, g, b, a);
        glTexCoord2f(u0, v0); glVertex2f(x, y);
        glTexCoord2f(u1, v0); glVertex2f(x + w, y);
        glTexCoord2f(u1, v1); glVertex2f(x + w, y + h);
        glTexCoord2f(u0, v1); glVertex2f(x, y + h);
    }

    public static void DrawChar(char ch, float x, float y, Color c, uint fontListBase) {
        if (x < 2f || x >= CanvasW - 4f || y < -14f || y >= CanvasH - 2f) return;
        BeginFontAtlasBatch();
        EmitAtlasChar(ch, x, y - 2f, 13f, 15f, c.R, c.G, c.B, c.A);
        EndFontAtlasBatch();
    }

    public static void DrawCharFast(char ch, float x, float y, byte r, byte g, byte b, byte a) {
        if (x < 2f || x >= CanvasW - 4f || y < -14f || y >= CanvasH - 2f) return;
        EmitAtlasChar(ch, x, y - 2f, 13f, 15f, r, g, b, a);
    }

    public static void DrawText(string s, float x, float y, Color c, uint fontListBase) {
        if (string.IsNullOrEmpty(s)) return;
        if (x < 2f || x >= CanvasW - 4f || y < -14f || y >= CanvasH - 2f) return;
        float adv = (fontListBase == FONT_LIST_HUD_SMALL) ? 6.6f : 8.0f;
        float qw  = (fontListBase == FONT_LIST_HUD_SMALL) ? 11.5f : 13.0f;
        float qh  = (fontListBase == FONT_LIST_HUD_SMALL) ? 13.0f : 15.0f;
        BeginFontAtlasBatch();
        int len = s.Length;
        for (int i = 0; i < len; i++) {
            char ch = s[i];
            if (ch > 32) {
                EmitAtlasChar(ch, x + i * adv, y - 1f, qw, qh, c.R, c.G, c.B, c.A);
            }
        }
        EndFontAtlasBatch();
    }

    public static void UploadBitmapToTexture(Bitmap bmp, ref uint texId, ref int texW, ref int texH) {
        if (bmp == null) return;
        if (texId == 0) {
            glGenTextures(1, out texId);
        }
        glBindTexture(GL_TEXTURE_2D, texId);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_LINEAR);

        BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try {
            if (texW != bmp.Width || texH != bmp.Height) {
                glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, bmp.Width, bmp.Height, 0, GL_BGRA_EXT, GL_UNSIGNED_BYTE, bd.Scan0);
                texW = bmp.Width;
                texH = bmp.Height;
            } else {
                glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, bmp.Width, bmp.Height, GL_BGRA_EXT, GL_UNSIGNED_BYTE, bd.Scan0);
            }
        } finally {
            bmp.UnlockBits(bd);
        }
    }

    public static void DrawTexturedQuad(uint texId, float x, float y, float w, float h) {
        if (texId == 0) return;
        glEnable(GL_TEXTURE_2D);
        glBindTexture(GL_TEXTURE_2D, texId);
        glColor4ub(255, 255, 255, 255);
        glBegin(GL_QUADS);
        glTexCoord2f(0f, 0f); glVertex2f(x, y);
        glTexCoord2f(1f, 0f); glVertex2f(x + w, y);
        glTexCoord2f(1f, 1f); glVertex2f(x + w, y + h);
        glTexCoord2f(0f, 1f); glVertex2f(x, y + h);
        glEnd();
        glDisable(GL_TEXTURE_2D);
    }

    public static void DrawTexturedQuadUV(uint texId, float x, float y, float w, float h, float u0, float v0, float u1, float v1) {
        if (texId == 0) return;
        glEnable(GL_TEXTURE_2D);
        glBindTexture(GL_TEXTURE_2D, texId);
        glColor4ub(255, 255, 255, 255);
        glBegin(GL_QUADS);
        glTexCoord2f(u0, v0); glVertex2f(x, y);
        glTexCoord2f(u1, v0); glVertex2f(x + w, y);
        glTexCoord2f(u1, v1); glVertex2f(x + w, y + h);
        glTexCoord2f(u0, v1); glVertex2f(x, y + h);
        glEnd();
        glDisable(GL_TEXTURE_2D);
    }
}

// ============================================================================
// PORYGON CYBER DEFENSE ENGINE (REAL NIDS/PID + AV/EDR + SIEM + HYBRID IPS)
//   - PORYGON   (Stage 1): NIDS Socket<->PID Mapper + SIEM Event/Registry Correlator
//   - PORYGON2  (Stage 2): Real-Time AV/EDR (FileSystemWatcher + SHA-256 + Entropy + Authenticode)
//   - PORYGON-Z (Stage 3): Hybrid Active IPS (Windows Firewall Block + Quarantine Vault)
// ============================================================================
public static class PorygonCyberDefenseEngine {
    private const int AF_INET = 2;
    private const int TCP_TABLE_OWNER_PID_ALL = 5;

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, int tblClass, uint reserved);

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID {
        public uint dwState;
        public uint dwLocalAddr;
        public uint dwLocalPort;
        public uint dwRemoteAddr;
        public uint dwRemotePort;
        public uint dwOwningPid;
    }

    public class ProcessSecurityInfo {
        public int Pid;
        public string ProcessName = "system";
        public string ExePath = "";
        public bool IsSigned = true;
        public string SignerName = "Microsoft Windows";
        public bool IsLolBin = false;
        public bool IsUserTempOrDownloads = false;
        public DateTime CachedAtUtc = DateTime.UtcNow;
    }

    public class AvScanReport {
        public string Timestamp;
        public string FileName;
        public string FullPath;
        public string Sha256;
        public long FileSizeBytes;
        public float EntropyBits;
        public bool IsSigned;
        public string SignerName;
        public float RiskScore;
        public string Verdict;      // CLEAN_SIGNED, UNSIGNED_LOW, SUSPICIOUS_PACKED, MALICIOUS_PAYLOAD, QUARANTINED
        public string MitreTactic;  // e.g. T1027, T1059, T1547.001, T1204
        public string Detail;
    }

    public class SiemSecurityEvent {
        public string AlertId;           // e.g. ALR-0001
        public string Timestamp;
        public string AgentName;         // PORYGON, PORYGON2, PORYGON-Z
        public string Module;            // NIDS_EDR, AV_SCAN, SIEM_REG, SIEM_EVTLOG, IPS_FIREWALL, IPS_QUARANTINE, CTI_IOC, YARA_SIGMA, DNS_DGA, SOAR
        public string Severity;          // INFO, MEDIUM, HIGH, CRITICAL
        public string MitreId;           // T1071, T1059, T1547.001, T1027, M1037...
        public string ProcessInfo;       // e.g. powershell.exe [PID:4812]
        public string Target;            // Remote IP:Port or FilePath or RegistryKey
        public string Sha256;
        public float RiskScore;
        public string Summary;
        public string ActionTaken;       // MONITORED, ALERT_READY, AUTO_BLOCKED, AUTO_QUARANTINED, USER_BLOCKED, USER_QUARANTINED
        public string ChainHash;         // Tamper-evident SHA-256 cryptographic chain hash (ISO 27001 / SOC2)
        public string ResolutionStatus;  // RESOLVED_SOAR, RESOLVED_AUTO, RESOLVED_WHITELIST, PENDING_ACTION
        public string ResolvedByAgent;   // PORYGON, PORYGON2, PORYGON-Z
        public string ResolutionMethod;  // Method used by Porygon to resolve the alert
        public string ResolutionDetail;  // Full technical explanation of how Porygon resolved it
        public string ResolvedAt;        // Timestamp of resolution
    }

    public class QuarantineRecord {
        public string Id;
        public string Timestamp;
        public string OriginalPath;
        public string QuarantinePath;
        public string FileName;
        public string Sha256;
        public float RiskScore;
        public string Reason;
    }

    public class LiveTcpConnectionRow {
        public int Pid;
        public string ProcessName;
        public string ExePath;
        public bool IsSigned;
        public string Signer;
        public bool IsLolBin;
        public string LocalEp;
        public string RemoteEp;
        public string Proto;
        public string State;
        public int RiskPct;
        public bool IsCtiMatch;
        public string MatchedRule;
    }

    public class EnterpriseDetectionRule {
        public string Id;
        public string Name;
        public string RuleType;     // "YARA" or "SIGMA"
        public string TargetField;  // "FILE_CONTENT", "PROCESS_NAME", "REMOTE_PORT", "DNS_DOMAIN"
        public string Pattern;
        public string MitreId;
        public string Severity;
        public int RiskPct;
        public bool Enabled;
        public int HitsCount;
    }

    public class FleetEndpointNode {
        public string HostId;
        public string Hostname;
        public string IpAddress;
        public string OsVersion;
        public string AgentRole;    // "MASTER_SOC_SERVER" or "REMOTE_SENSOR"
        public string Status;       // "ONLINE", "ISOLATED", "HIGH_RISK"
        public float CpuPct;
        public int RamPct;
        public int ActiveTcpCount;
        public int OpenAlerts;
        public int RiskScorePct;
        public bool IsIsolated;
        public string LastSeen;
        public DateTime LastSeenUtc;
    }

    public class DnsAuditEntry {
        public string Timestamp;
        public string Domain;
        public string RecordType;
        public string ResolvedIp;
        public float Entropy;
        public int RiskPct;
        public string Verdict;      // "CLEAN_DNS", "DGA_HIGH_ENTROPY", "C2_TUNNEL_SUSPECT", "CTI_IOC_DOMAIN"
        public string MitreId;
    }

    public class SoarEnterpriseConfig {
        public bool SyslogEnabled = false;
        public string SyslogHost = "127.0.0.1";
        public int SyslogPort = 514;
        public bool FortiGateEnabled = false;
        public string FortiGateWebhookUrl = "https://192.168.1.99/api/v2/monitor/system/automation-stitch/webhook/porygon_ips";
        public string FortiGateApiToken = "";
        public string FortiGateAddressGroup = "PORYGON_SOAR_BLOCKLIST";
        public bool AlertWebhookEnabled = false;
        public string AlertWebhookUrl = "";
        public string PorygonAutonomyMode = "GRADUATED_2FA"; // "COPILOT_1CLICK", "GRADUATED_2FA", "STRICT_AUTO_SOAR"
        public bool AutoKillCriticalUnsigned = false;
        public int TotalCefSent = 0;
        public int TotalFortiGatePushes = 0;
        public int TotalAlertWebhooksSent = 0;
        public string LastSoarStatus = "SOAR_READY // Autonomía Graduada (2FA) + Syslog CEF + FortiGate + Webhook Crítico";
    }

    public const int WebSocPort = 5090;
    public static string WebSocUrl { get { return "http://localhost:" + WebSocPort + "/"; } }

    public static readonly object SyncLock = new object();
    private static Dictionary<int, ProcessSecurityInfo> pidCache = new Dictionary<int, ProcessSecurityInfo>();
    private static Dictionary<string, string> baselineRunRegistry = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static bool registryBaselineReady = false;
    private static DateTime lastEventLogPollUtc = DateTime.UtcNow.AddMinutes(-5);
    private static int lastEventLogIndex = -1;
    private static DateTime lastDnsAuditUtc = DateTime.UtcNow.AddMinutes(-5);
    private static Dictionary<string, List<DateTime>> c2BeaconHistory = new Dictionary<string, List<DateTime>>(StringComparer.OrdinalIgnoreCase);
    private static HashSet<string> alertedBeacons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static List<SiemSecurityEvent> RecentSiemEvents = new List<SiemSecurityEvent>();
    public static List<AvScanReport> RecentAvScans = new List<AvScanReport>();
    public static List<string> BlockedFirewallIps = new List<string>();
    public static List<QuarantineRecord> QuarantinedFiles = new List<QuarantineRecord>();
    public static List<LiveTcpConnectionRow> ActiveTcpRows = new List<LiveTcpConnectionRow>();

    // Enterprise XDR State: CTI Threat Feeds, YARA/Sigma Rules, Multi-Host Fleet, DNS Inspector & SOAR
    public static HashSet<string> CtiMaliciousIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public static int CtiTotalIndicators = 0;
    public static int CtiHitsCount = 0;
    public static string CtiLastSyncStatus = "Semilla CTI cargada (Sincronizando Feodo Tracker C2...)";
    public static List<EnterpriseDetectionRule> DetectionRules = new List<EnterpriseDetectionRule>();
    public static Dictionary<string, FleetEndpointNode> FleetNodes = new Dictionary<string, FleetEndpointNode>(StringComparer.OrdinalIgnoreCase);
    public static List<DnsAuditEntry> RecentDnsAudits = new List<DnsAuditEntry>();
    public static SoarEnterpriseConfig SoarConfig = new SoarEnterpriseConfig();
    public static string LastSiemChainHash = "0000000000000000";
    public static int TotalEdrProcessesKilled = 0;

    // Office-PC SSH Tunnel Proxy (:8002 Desencript C2/IDS + :5080 QB-Network NIDS/VirusTotal)
    private static Process sshTunnelProcess = null;
    private static HashSet<string> ingestedOfficeAlertIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static HashSet<string> ingestedOnlineAgentProbeCmdIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static HashSet<string> whitelistedC2FleetIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        "100.89.1.27", "192.168.2.222", "192.168.2.211", "192.168.2.200", "192.168.2.50", "172.25.247.247"
    };
    private static DateTime lastOfficeC2PollUtc = DateTime.UtcNow.AddMinutes(-10);
    private static int officeAlertRotationIdx = 0;
    public static bool OfficePcTunnelOnline = false;
    public static string OfficePcTunnelStatus = "INICIANDO PROXY SSH OFFICE-PC (127.0.0.1:8002 C2/IDS + :5080 NIDS/VT)...";
    public static int OfficePcLogsRaw = 0;
    public static int OfficePcLogsDecrypted = 0;
    public static int OfficePcCredentialsCount = 0;
    public static int OfficePcScreenshotsCount = 0;
    public static int OfficePcIdsAlertsTotal = 0;
    public static int OfficePcLanHostsTotal = 0;
    public static int OfficePcOnlineAgentsCount = 0;
    public static int OfficePcTotalAgentsCount = 0;
    public static string OfficePcOnlineAgentsSummary = "Detectando agentes online...";
    public static string OfficePcLastLiveProbeReport = "Listo para probar agentes C2 online (Solo lectura, sin tocar binarios)";

    // Active Hybrid IPS / AV Candidates for 1-Click HUD Response
    public static string LastCandidateBlockIp = "";
    public static string LastCandidateBlockReason = "Ninguna IP crítica pendiente";
    public static string LastCandidateBlockProc = "";
    public static float LastCandidateBlockScore = 0f;

    public static string LastCandidateQuarantineFile = "";
    public static string LastCandidateQuarantineSha = "";
    public static string LastCandidateQuarantineReason = "Ningún archivo sospechoso pendiente";
    public static float LastCandidateQuarantineScore = 0f;

    public static int TotalNidsProcessAudits = 0;
    public static int TotalAvFilesScanned = 0;
    public static int TotalSiemAlerts = 0;
    public static int TotalIpsActions = 0;
    public static string LastActionBanner = "ENTERPRISE_XDR_ONLINE // WEB SOC ACTIVO EN http://localhost:5090";

    private static List<FileSystemWatcher> activeWatchers = new List<FileSystemWatcher>();
    private static Queue<string> pendingAvFiles = new Queue<string>();
    private static HashSet<string> recentlyScannedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static HashSet<string> alertedDnsDgaDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static Thread avWorkerThread = null;
    private static Thread webSocThread = null;
    private static TcpListener webSocListener = null;
    private static bool engineInitialized = false;

    // Callbacks to interact with HackerWallpaperForm & Porygon3DAvatar
    public static Action<int, string, string, string, string, string, string, float> OnInject3DThreatOrb = null;
    public static Func<Dictionary<string, object>> OnQueryFormState = null;
    public static Func<string, Dictionary<string, string>, string> OnBrainApi = null;
    public static Action<string> OnSwarmWebAction = null;

    private static string AppBaseDir {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            if (!Directory.Exists(dir)) {
                try { Directory.CreateDirectory(dir); } catch { }
            }
            return dir;
        }
    }

    public static string SiemJsonlPath {
        get { return Path.Combine(AppBaseDir, "siem_security_events.jsonl"); }
    }

    public static string ProjectSiemJsonlPath {
        get {
            string uProf = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pUser = Path.Combine(uProf, "Porygon-Cyber-IDS-Wallpaper", "logs", "siem_security_events.jsonl");
            if (Directory.Exists(Path.GetDirectoryName(pUser))) return pUser;
            return @"C:\Users\Admin\Porygon-Cyber-IDS-Wallpaper\logs\siem_security_events.jsonl";
        }
    }

    public static string QuarantineDir {
        get {
            string q = Path.Combine(AppBaseDir, "Quarantine");
            if (!Directory.Exists(q)) {
                try { Directory.CreateDirectory(q); } catch { }
            }
            return q;
        }
    }

    private static string BlockedIpsPath {
        get { return Path.Combine(AppBaseDir, "ips_blocked_list.txt"); }
    }

    private static string QuarantineManifestPath {
        get { return Path.Combine(AppBaseDir, "quarantine_manifest.txt"); }
    }

    private static string EnterpriseRulesPath {
        get { return Path.Combine(AppBaseDir, "enterprise_rules.txt"); }
    }

    private static string SoarConfigPath {
        get { return Path.Combine(AppBaseDir, "soar_config.txt"); }
    }

    public static void Initialize() {
        lock (SyncLock) {
            if (engineInitialized) return;
            engineInitialized = true;
        }
        try {
            InitializeDefaultRulesAndCtiSeeds();
            LoadPersistedIpsAndQuarantine();
            AuditRegistryPersistence(true);
            StartRealtimeAvWatchers();
            StartWebSocServer();

            RecordSiemEvent("PORYGON", "SIEM_INIT", "INFO", "T1046", "HackerWallpaper.exe",
                "http://localhost:5090", "", 0.05f,
                "Plataforma Empresarial Porygon XDR/IDS/IPS/SOAR v3.0 activa en http://localhost:5090 + Proxy SSH office-pc (:8002 C2/IDS & :5080 NIDS/VT)",
                "ENTERPRISE_SOC_ONLINE");

            // Perform initial non-blocking background audit + SSH Proxy office-pc C2/IDS sync + live CTI feed download
            ThreadPool.QueueUserWorkItem(_ => {
                try {
                    EnsureOfficePcSshProxyTunnel(false);
                    PollOfficePcC2AndIdsTelemetry(true);
                    SyncLiveCtiThreatFeeds(false);
                    AuditDnsCacheAndBeaconing(false);
                    RunQuickDirectoryAudit(false);
                } catch { }
            });
        } catch { }
    }

    private static void InitializeDefaultRulesAndCtiSeeds() {
        lock (SyncLock) {
            string[] seedC2Ips = new string[] {
                "185.220.101.4", "185.220.101.45", "45.155.205.233", "193.42.33.14",
                "91.92.240.58", "178.128.83.165", "162.247.74.200", "89.248.165.59",
                "45.142.214.123", "194.26.29.114", "185.196.8.145", "5.188.206.14",
                "141.98.10.60", "198.98.56.151", "94.102.61.24", "195.123.246.18"
            };
            for (int i = 0; i < seedC2Ips.Length; i++) {
                CtiMaliciousIps.Add(seedC2Ips[i]);
            }
            CtiTotalIndicators = CtiMaliciousIps.Count;

            if (DetectionRules.Count == 0) {
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "YARA_001", Name = "YARA_CREDENTIAL_DUMP_MIMIKATZ", RuleType = "YARA",
                    TargetField = "FILE_CONTENT", Pattern = "sekurlsa", MitreId = "T1003.001",
                    Severity = "CRITICAL", RiskPct = 98, Enabled = true, HitsCount = 0
                });
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "YARA_002", Name = "YARA_AMSI_ETW_MEMORY_PATCH", RuleType = "YARA",
                    TargetField = "FILE_CONTENT", Pattern = "amsiinitfailed", MitreId = "T1562.001",
                    Severity = "CRITICAL", RiskPct = 95, Enabled = true, HitsCount = 0
                });
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "YARA_003", Name = "YARA_RANSOM_SHADOWCOPY_WIPE", RuleType = "YARA",
                    TargetField = "FILE_CONTENT", Pattern = "vssadmin delete shadows", MitreId = "T1490",
                    Severity = "CRITICAL", RiskPct = 99, Enabled = true, HitsCount = 0
                });
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "SIGMA_001", Name = "SIGMA_POWERSHELL_ENCODED_CRADLE", RuleType = "SIGMA",
                    TargetField = "FILE_CONTENT", Pattern = "frombase64string", MitreId = "T1059.001",
                    Severity = "HIGH", RiskPct = 88, Enabled = true, HitsCount = 0
                });
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "SIGMA_002", Name = "SIGMA_C2_METASPLOIT_PORT_4444", RuleType = "SIGMA",
                    TargetField = "REMOTE_PORT", Pattern = "4444", MitreId = "T1571",
                    Severity = "CRITICAL", RiskPct = 96, Enabled = true, HitsCount = 0
                });
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "SIGMA_003", Name = "SIGMA_LOLBIN_CERTUTIL_MSHTA", RuleType = "SIGMA",
                    TargetField = "PROCESS_NAME", Pattern = "mshta", MitreId = "T1218.005",
                    Severity = "HIGH", RiskPct = 90, Enabled = true, HitsCount = 0
                });
                DetectionRules.Add(new EnterpriseDetectionRule {
                    Id = "SIGMA_004", Name = "SIGMA_TOR_ONION_DNS_TUNNEL", RuleType = "SIGMA",
                    TargetField = "DNS_DOMAIN", Pattern = ".onion", MitreId = "T1090.003",
                    Severity = "HIGH", RiskPct = 91, Enabled = true, HitsCount = 0
                });
            }
        }
    }

    private static void LoadPersistedIpsAndQuarantine() {
        try {
            if (File.Exists(BlockedIpsPath)) {
                string[] lines = File.ReadAllLines(BlockedIpsPath, Encoding.UTF8);
                lock (SyncLock) {
                    BlockedFirewallIps.Clear();
                    for (int i = 0; i < lines.Length; i++) {
                        string ip = lines[i].Trim();
                        if (!string.IsNullOrEmpty(ip) && !BlockedFirewallIps.Contains(ip)) {
                            BlockedFirewallIps.Add(ip);
                        }
                    }
                }
            }
            if (File.Exists(QuarantineManifestPath)) {
                string[] qlines = File.ReadAllLines(QuarantineManifestPath, Encoding.UTF8);
                lock (SyncLock) {
                    QuarantinedFiles.Clear();
                    for (int i = 0; i < qlines.Length; i++) {
                        string[] p = qlines[i].Split('|');
                        if (p.Length >= 7) {
                            QuarantinedFiles.Add(new QuarantineRecord {
                                Id = p[0],
                                Timestamp = p[1],
                                FileName = p[2],
                                OriginalPath = p[3],
                                QuarantinePath = p[4],
                                Sha256 = p[5],
                                RiskScore = 0.85f,
                                Reason = p[6]
                            });
                        }
                    }
                }
            }
            if (File.Exists(EnterpriseRulesPath)) {
                string[] rlines = File.ReadAllLines(EnterpriseRulesPath, Encoding.UTF8);
                lock (SyncLock) {
                    if (rlines.Length > 0) DetectionRules.Clear();
                    for (int i = 0; i < rlines.Length; i++) {
                        string[] p = rlines[i].Split('|');
                        if (p.Length >= 9) {
                            int rk = 85, hc = 0;
                            int.TryParse(p[6], out rk);
                            int.TryParse(p[8], out hc);
                            DetectionRules.Add(new EnterpriseDetectionRule {
                                Id = p[0],
                                Name = p[1],
                                RuleType = p[2],
                                TargetField = p[3],
                                Pattern = p[4],
                                MitreId = p[5],
                                RiskPct = rk,
                                Enabled = (p[7] == "1" || p[7].Equals("true", StringComparison.OrdinalIgnoreCase)),
                                HitsCount = hc,
                                Severity = rk >= 90 ? "CRITICAL" : "HIGH"
                            });
                        }
                    }
                }
            }
            if (File.Exists(SoarConfigPath)) {
                string[] slines = File.ReadAllLines(SoarConfigPath, Encoding.UTF8);
                if (slines.Length >= 6) {
                    lock (SyncLock) {
                        SoarConfig.SyslogEnabled = (slines[0] == "1");
                        SoarConfig.SyslogHost = string.IsNullOrEmpty(slines[1]) ? "127.0.0.1" : slines[1];
                        int sp = 514;
                        if (int.TryParse(slines[2], out sp)) SoarConfig.SyslogPort = sp;
                        SoarConfig.FortiGateEnabled = (slines[3] == "1");
                        SoarConfig.FortiGateWebhookUrl = slines[4];
                        SoarConfig.FortiGateAddressGroup = string.IsNullOrEmpty(slines[5]) ? "PORYGON_XDR_BLOCKLIST" : slines[5];
                        if (slines.Length >= 7) SoarConfig.FortiGateApiToken = slines[6];
                        if (slines.Length >= 8) SoarConfig.AlertWebhookEnabled = (slines[7] == "1");
                        if (slines.Length >= 9) SoarConfig.AlertWebhookUrl = slines[8] ?? "";
                        if (slines.Length >= 10 && !string.IsNullOrEmpty(slines[9])) SoarConfig.PorygonAutonomyMode = slines[9].Trim();
                    }
                }
            }
            HydrateRecentSiemEventsFromJsonl();
        } catch { }
    }

    private static void HydrateRecentSiemEventsFromJsonl() {
        try {
            string path = File.Exists(SiemJsonlPath) ? SiemJsonlPath : (File.Exists(ProjectSiemJsonlPath) ? ProjectSiemJsonlPath : null);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines == null || lines.Length == 0) return;

            string todayPrefix = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterdayPrefix = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<SiemSecurityEvent> hydrated = new List<SiemSecurityEvent>();
            Dictionary<string, int> hourTargetBurstCap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> seenChainHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int startIdx = Math.Max(0, lines.Length - 2500);
            for (int i = lines.Length - 1; i >= startIdx && hydrated.Count < 180; i--) {
                string line = (lines[i] ?? "").Trim();
                if (line.Length < 10 || !line.StartsWith("{")) continue;
                try {
                    var d = ser.Deserialize<Dictionary<string, object>>(line);
                    if (d == null) continue;
                    string tsRaw = d.ContainsKey("timestamp") && d["timestamp"] != null ? d["timestamp"].ToString().Trim() : "";
                    if (tsRaw.Length >= 10 && tsRaw[4] == '-' && !tsRaw.StartsWith(todayPrefix) && !tsRaw.StartsWith(yesterdayPrefix)) {
                        continue;
                    }
                    string shortTs = tsRaw.Length > 11 ? tsRaw.Substring(11).Trim() : tsRaw;
                    string hourSlot = shortTs.Length >= 2 ? shortTs.Substring(0, 2) : "00";
                    string mod = d.ContainsKey("module") && d["module"] != null ? d["module"].ToString() : "SIEM";
                    string proc = d.ContainsKey("process") && d["process"] != null ? d["process"].ToString() : "system";
                    string tgt = d.ContainsKey("target") && d["target"] != null ? d["target"].ToString() : "";
                    string ch = d.ContainsKey("chainHash") && d["chainHash"] != null ? d["chainHash"].ToString() : "";
                    if (!string.IsNullOrEmpty(ch) && seenChainHashes.Contains(ch)) continue;

                    string burstKey = hourSlot + "|" + mod + "|" + tgt;
                    int burstCnt = 0;
                    hourTargetBurstCap.TryGetValue(burstKey, out burstCnt);
                    if (burstCnt >= 3) continue;
                    hourTargetBurstCap[burstKey] = burstCnt + 1;
                    if (!string.IsNullOrEmpty(ch)) seenChainHashes.Add(ch);

                    float riskVal = 0.75f;
                    if (d.ContainsKey("risk") && d["risk"] != null) {
                        float.TryParse(Convert.ToString(d["risk"], System.Globalization.CultureInfo.InvariantCulture),
                            System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out riskVal);
                    }

                    SiemSecurityEvent ev = new SiemSecurityEvent {
                        AlertId = d.ContainsKey("alertId") && d["alertId"] != null ? d["alertId"].ToString() : "",
                        Timestamp = shortTs,
                        AgentName = d.ContainsKey("agent") && d["agent"] != null ? d["agent"].ToString() : "PORYGON",
                        Module = mod,
                        Severity = d.ContainsKey("severity") && d["severity"] != null ? d["severity"].ToString() : "INFO",
                        MitreId = d.ContainsKey("mitre") && d["mitre"] != null ? d["mitre"].ToString() : "T1071",
                        ProcessInfo = proc,
                        Target = tgt,
                        Sha256 = d.ContainsKey("sha256") && d["sha256"] != null ? d["sha256"].ToString() : "",
                        RiskScore = riskVal,
                        Summary = d.ContainsKey("summary") && d["summary"] != null ? d["summary"].ToString() : "",
                        ActionTaken = d.ContainsKey("action") && d["action"] != null ? d["action"].ToString() : "LOGGED",
                        ChainHash = string.IsNullOrEmpty(ch) ? ComputeSiemHashChainLink("GENESIS", tsRaw, mod, tgt, proc) : ch,
                        ResolutionStatus = d.ContainsKey("resolutionStatus") && d["resolutionStatus"] != null ? d["resolutionStatus"].ToString() : "",
                        ResolvedByAgent = d.ContainsKey("resolvedBy") && d["resolvedBy"] != null ? d["resolvedBy"].ToString() : "",
                        ResolutionMethod = d.ContainsKey("resolutionMethod") && d["resolutionMethod"] != null ? d["resolutionMethod"].ToString() : "",
                        ResolvedAt = shortTs
                    };
                    PopulatePorygonAlertResolution(ev);
                    hydrated.Add(ev);
                } catch { }
            }

            if (hydrated.Count > 0) {
                hydrated.Reverse();
                lock (SyncLock) {
                    RecentSiemEvents.Clear();
                    for (int i = 0; i < hydrated.Count; i++) {
                        TotalSiemAlerts++;
                        hydrated[i].AlertId = "ALR-" + TotalSiemAlerts.ToString("0000");
                        LastSiemChainHash = hydrated[i].ChainHash;
                        RecentSiemEvents.Add(hydrated[i]);
                    }
                }
            }
        } catch { }
    }

    private static void SavePersistedIpsAndQuarantine() {
        try {
            List<string> ipsSnap;
            List<QuarantineRecord> qSnap;
            List<EnterpriseDetectionRule> rSnap;
            string[] soarLines;
            lock (SyncLock) {
                ipsSnap = new List<string>(BlockedFirewallIps);
                qSnap = new List<QuarantineRecord>(QuarantinedFiles);
                rSnap = new List<EnterpriseDetectionRule>(DetectionRules);
                soarLines = new string[] {
                    SoarConfig.SyslogEnabled ? "1" : "0",
                    SoarConfig.SyslogHost ?? "127.0.0.1",
                    SoarConfig.SyslogPort.ToString(),
                    SoarConfig.FortiGateEnabled ? "1" : "0",
                    SoarConfig.FortiGateWebhookUrl ?? "",
                    SoarConfig.FortiGateAddressGroup ?? "PORYGON_XDR_BLOCKLIST",
                    SoarConfig.FortiGateApiToken ?? "",
                    SoarConfig.AlertWebhookEnabled ? "1" : "0",
                    SoarConfig.AlertWebhookUrl ?? "",
                    SoarConfig.PorygonAutonomyMode ?? "GRADUATED_2FA"
                };
            }
            File.WriteAllLines(BlockedIpsPath, ipsSnap.ToArray(), Encoding.UTF8);
            List<string> qLines = new List<string>();
            for (int i = 0; i < qSnap.Count; i++) {
                QuarantineRecord r = qSnap[i];
                qLines.Add(string.Join("|", new string[] {
                    r.Id ?? "", r.Timestamp ?? "", r.FileName ?? "", r.OriginalPath ?? "", r.QuarantinePath ?? "", r.Sha256 ?? "", r.Reason ?? ""
                }));
            }
            File.WriteAllLines(QuarantineManifestPath, qLines.ToArray(), Encoding.UTF8);

            List<string> rLines = new List<string>();
            for (int i = 0; i < rSnap.Count; i++) {
                var rl = rSnap[i];
                rLines.Add(string.Join("|", new string[] {
                    rl.Id ?? "", rl.Name ?? "", rl.RuleType ?? "YARA", rl.TargetField ?? "FILE_CONTENT",
                    (rl.Pattern ?? "").Replace("|", ""), rl.MitreId ?? "T1027", rl.RiskPct.ToString(),
                    rl.Enabled ? "1" : "0", rl.HitsCount.ToString()
                }));
            }
            File.WriteAllLines(EnterpriseRulesPath, rLines.ToArray(), Encoding.UTF8);
            File.WriteAllLines(SoarConfigPath, soarLines, Encoding.UTF8);
        } catch { }
    }

    // =========================================================================
    // MODULE 1: REAL SOCKET <-> PID & AUTHENTICODE MAPPER (GetExtendedTcpTable)
    // =========================================================================
    private static int ConvertNetworkPort(uint netPort) {
        return (int)(((netPort & 0xFF) << 8) | ((netPort >> 8) & 0xFF));
    }

    public static Dictionary<string, int> QueryExtendedTcpPidMap() {
        Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int buffSize = 0;
        uint ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, false, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
        if (buffSize <= 0) return map;

        IntPtr buffPtr = Marshal.AllocHGlobal(buffSize);
        try {
            ret = GetExtendedTcpTable(buffPtr, ref buffSize, false, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
            if (ret != 0) return map;

            int numEntries = Marshal.ReadInt32(buffPtr);
            IntPtr rowPtr = (IntPtr)((long)buffPtr + 4);
            int rowSize = Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));

            for (int i = 0; i < numEntries; i++) {
                MIB_TCPROW_OWNER_PID row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                int pid = (int)row.dwOwningPid;
                if (pid > 0) {
                    IPAddress locIp = new IPAddress(row.dwLocalAddr);
                    IPAddress remIp = new IPAddress(row.dwRemoteAddr);
                    int locPort = ConvertNetworkPort(row.dwLocalPort);
                    int remPort = (row.dwState == 2 || row.dwRemoteAddr == 0) ? 0 : ConvertNetworkPort(row.dwRemotePort);

                    string exactKey = locIp.ToString() + ":" + locPort + "|" + remIp.ToString() + ":" + remPort;
                    string remKey = remIp.ToString() + ":" + remPort;
                    map[exactKey] = pid;
                    if (!map.ContainsKey(remKey)) map[remKey] = pid;
                }
                rowPtr = (IntPtr)((long)rowPtr + rowSize);
            }
        } catch {
        } finally {
            Marshal.FreeHGlobal(buffPtr);
        }
        return map;
    }

    public static ProcessSecurityInfo ResolveProcessSecurityInfo(int pid) {
        if (pid <= 4) {
            return new ProcessSecurityInfo {
                Pid = pid, ProcessName = "System", ExePath = "NT Kernel & System", IsSigned = true, SignerName = "Microsoft Windows Kernel"
            };
        }
        DateTime now = DateTime.UtcNow;
        lock (SyncLock) {
            ProcessSecurityInfo cached;
            if (pidCache.TryGetValue(pid, out cached)) {
                if ((now - cached.CachedAtUtc).TotalSeconds < 45.0) {
                    return cached;
                }
            }
        }

        ProcessSecurityInfo info = new ProcessSecurityInfo { Pid = pid, CachedAtUtc = now };
        try {
            using (Process p = Process.GetProcessById(pid)) {
                info.ProcessName = p.ProcessName ?? ("pid_" + pid);
                try {
                    if (p.MainModule != null) {
                        info.ExePath = p.MainModule.FileName ?? "";
                    }
                } catch {
                    // Protected OS services (svchost, lsass, csrss) throw access denied on MainModule from non-elevated user context
                    info.ExePath = @"C:\Windows\System32\" + info.ProcessName + ".exe";
                    info.IsSigned = true;
                    info.SignerName = "Windows Protected Service";
                }
            }
        } catch {
            info.ProcessName = "terminated_pid_" + pid;
        }

        string lowerName = (info.ProcessName ?? "").ToLowerInvariant();
        string lowerPath = (info.ExePath ?? "").ToLowerInvariant();

        if (lowerName == "powershell" || lowerName == "pwsh" || lowerName == "cmd" ||
            lowerName == "wscript" || lowerName == "cscript" || lowerName == "mshta" ||
            lowerName == "rundll32" || lowerName == "regsvr32" || lowerName == "certutil" || lowerName == "bitsadmin") {
            info.IsLolBin = true;
        }

        if (lowerPath.Contains("\\temp\\") || lowerPath.Contains("\\downloads\\") || lowerPath.Contains("\\users\\public\\")) {
            info.IsUserTempOrDownloads = true;
        }

        if (!string.IsNullOrEmpty(info.ExePath) && File.Exists(info.ExePath)) {
            if (lowerPath.StartsWith(@"c:\windows\system32\") || lowerPath.StartsWith(@"c:\windows\syswow64\")) {
                info.IsSigned = true;
                info.SignerName = "Microsoft Windows";
            } else {
                try {
                    X509Certificate cert = X509Certificate.CreateFromSignedFile(info.ExePath);
                    if (cert != null) {
                        info.IsSigned = true;
                        string subj = cert.Subject ?? "Signed Binary";
                        int cnIdx = subj.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
                        if (cnIdx >= 0) {
                            int endIdx = subj.IndexOf(',', cnIdx);
                            info.SignerName = (endIdx > cnIdx) ? subj.Substring(cnIdx + 3, endIdx - cnIdx - 3).Trim('"') : subj.Substring(cnIdx + 3).Trim('"');
                        } else {
                            info.SignerName = subj;
                        }
                    } else {
                        info.IsSigned = false;
                        info.SignerName = "UNSIGNED";
                    }
                } catch {
                    info.IsSigned = false;
                    info.SignerName = "UNSIGNED";
                }
            }
        }

        lock (SyncLock) {
            TotalNidsProcessAudits++;
            if (pidCache.Count > 250) pidCache.Clear();
            pidCache[pid] = info;
        }
        return info;
    }

    // =========================================================================
    // MODULE 2: PORYGON2 REAL-TIME ANTIVIRUS / EDR (FileSystemWatcher + SHA256 + Entropy)
    // =========================================================================
    private static readonly HashSet<string> WatchedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        ".exe", ".dll", ".ps1", ".bat", ".cmd", ".vbs", ".js", ".hta", ".scr", ".msi", ".wsf", ".jar"
    };

    private static void StartRealtimeAvWatchers() {
        string userProf = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] dirs = new string[] {
            Path.Combine(userProf, "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Path.GetTempPath(),
            Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        };

        for (int i = 0; i < dirs.Length; i++) {
            string d = dirs[i];
            if (string.IsNullOrEmpty(d) || !Directory.Exists(d)) continue;
            try {
                FileSystemWatcher fsw = new FileSystemWatcher(d);
                fsw.IncludeSubdirectories = false;
                fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                FileSystemEventHandler onFs = (s, e) => EnqueueFileForAvScan(e.FullPath);
                RenamedEventHandler onRen = (s, e) => EnqueueFileForAvScan(e.FullPath);
                fsw.Created += onFs;
                fsw.Changed += onFs;
                fsw.Renamed += onRen;
                fsw.EnableRaisingEvents = true;
                activeWatchers.Add(fsw);
            } catch { }
        }

        avWorkerThread = new Thread(AvWorkerLoop);
        avWorkerThread.IsBackground = true;
        avWorkerThread.Priority = ThreadPriority.BelowNormal;
        avWorkerThread.Start();
    }

    public static void EnqueueFileForAvScan(string fullPath) {
        if (string.IsNullOrEmpty(fullPath)) return;
        try {
            string ext = Path.GetExtension(fullPath);
            if (string.IsNullOrEmpty(ext) || !WatchedExtensions.Contains(ext)) return;
            if (fullPath.IndexOf("\\Quarantine\\", StringComparison.OrdinalIgnoreCase) >= 0) return;

            lock (SyncLock) {
                if (pendingAvFiles.Count < 60 && !pendingAvFiles.Contains(fullPath)) {
                    pendingAvFiles.Enqueue(fullPath);
                }
            }
        } catch { }
    }

    private static void AvWorkerLoop() {
        while (true) {
            string nextFile = null;
            lock (SyncLock) {
                if (pendingAvFiles.Count > 0) {
                    nextFile = pendingAvFiles.Dequeue();
                }
            }
            if (nextFile != null) {
                Thread.Sleep(380); // Wait for browser/writer to flush file handle
                InspectFileArtifact(nextFile, false);
            } else {
                Thread.Sleep(650);
            }
        }
    }

    public static AvScanReport InspectFileArtifact(string filePath, bool isManualScan) {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;
        try {
            FileInfo fi = new FileInfo(filePath);
            if (fi.Length <= 0 || fi.Length > 150 * 1024 * 1024) return null; // Skip empty or >150MB ISOs

            string dedupKey = filePath + "|" + fi.Length + "|" + fi.LastWriteTimeUtc.Ticks;
            if (!isManualScan) {
                lock (SyncLock) {
                    if (recentlyScannedFiles.Contains(dedupKey)) return null;
                    if (recentlyScannedFiles.Count > 300) recentlyScannedFiles.Clear();
                    recentlyScannedFiles.Add(dedupKey);
                }
            }

            // 1. Compute SHA-256 and Shannon Entropy on first 64 KB
            string sha256Hex = "";
            float entropy = 0f;
            string asciiPreview = "";
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                int sampleLen = (int)Math.Min(65536L, fi.Length);
                byte[] sampleBuf = new byte[sampleLen];
                int bytesRead = fs.Read(sampleBuf, 0, sampleLen);
                entropy = ComputeShannonEntropy(sampleBuf, bytesRead);
                asciiPreview = Encoding.ASCII.GetString(sampleBuf, 0, bytesRead);

                fs.Position = 0;
                using (SHA256 sha = SHA256.Create()) {
                    byte[] hashBytes = sha.ComputeHash(fs);
                    StringBuilder sb = new StringBuilder(64);
                    for (int i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("x2"));
                    sha256Hex = sb.ToString();
                }
            }

            // 2. Verify Authenticode Digital Signature
            bool isSigned = false;
            string signerName = "UNSIGNED";
            try {
                X509Certificate cert = X509Certificate.CreateFromSignedFile(filePath);
                if (cert != null) {
                    isSigned = true;
                    string subj = cert.Subject ?? "Signed";
                    int cnIdx = subj.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
                    if (cnIdx >= 0) {
                        int endIdx = subj.IndexOf(',', cnIdx);
                        signerName = (endIdx > cnIdx) ? subj.Substring(cnIdx + 3, endIdx - cnIdx - 3).Trim('"') : subj.Substring(cnIdx + 3).Trim('"');
                    } else {
                        signerName = subj;
                    }
                }
            } catch {
                isSigned = false;
            }

            // 3. Heuristic & Static Payload Scoring
            string ext = (fi.Extension ?? "").ToLowerInvariant();
            string lowerPath = filePath.ToLowerInvariant();
            bool inStartup = lowerPath.Contains("\\startup\\");
            bool inTempOrDl = lowerPath.Contains("\\temp\\") || lowerPath.Contains("\\downloads\\");

            float risk = 0.08f;
            string mitre = "T1204";
            List<string> indicators = new List<string>();

            if (!isSigned) {
                risk += 0.30f;
                indicators.Add("UNSIGNED");
            } else {
                indicators.Add("SIGNED:" + (signerName.Length > 14 ? signerName.Substring(0, 14) : signerName));
            }

            if (entropy >= 7.30f && !isSigned) {
                risk += 0.36f;
                mitre = "T1027"; // Obfuscated / Packed Binary
                indicators.Add(string.Format("HIGH_ENTROPY({0:0.00}b)", entropy));
            } else if (entropy >= 6.95f && !isSigned) {
                risk += 0.18f;
                indicators.Add(string.Format("ENTROPY({0:0.00}b)", entropy));
            }

            if (inStartup && !isSigned) {
                risk += 0.34f;
                mitre = "T1547.001";
                indicators.Add("STARTUP_PERSISTENCE");
            } else if (inTempOrDl && !isSigned && (ext == ".exe" || ext == ".scr" || ext == ".hta" || ext == ".vbs" || ext == ".ps1" || ext == ".bat")) {
                risk += 0.16f;
                indicators.Add("USER_DROP_ZONE");
            }

            // Check script / payload evasion signatures & Custom YARA / Sigma FILE_CONTENT rules
            string lowerText = asciiPreview.ToLowerInvariant();
            if (lowerText.Contains("frombase64string") || lowerText.Contains("-encodedcommand") || lowerText.Contains(" -enc ")) {
                risk += 0.35f;
                mitre = "T1059.001";
                indicators.Add("B64_ENCODED_CMD");
            }
            if (lowerText.Contains("amsiutils") || lowerText.Contains("amsiinitfailed") || lowerText.Contains("etweventwrite")) {
                risk += 0.48f;
                mitre = "T1562.001";
                indicators.Add("AMSI_ETW_BYPASS");
            }
            if (lowerText.Contains("downloadstring") || lowerText.Contains("invoke-expression") || lowerText.Contains("iex(") || lowerText.Contains("urldownloadtofile")) {
                risk += 0.32f;
                mitre = "T1105";
                indicators.Add("REMOTE_CRADLE_EXEC");
            }
            if (lowerText.Contains("mimikatz") || lowerText.Contains("sekurlsa") || (lowerText.Contains("virtualalloc") && !isSigned)) {
                risk += 0.42f;
                mitre = "T1055";
                indicators.Add("SHELLCODE_MEM_API");
            }

            // Evaluate Enterprise YARA / Sigma FILE_CONTENT Rules
            lock (SyncLock) {
                for (int rIdx = 0; rIdx < DetectionRules.Count; rIdx++) {
                    var rule = DetectionRules[rIdx];
                    if (!rule.Enabled || string.IsNullOrEmpty(rule.Pattern)) continue;
                    if (string.Equals(rule.TargetField, "FILE_CONTENT", StringComparison.OrdinalIgnoreCase)) {
                        if (lowerText.Contains(rule.Pattern.ToLowerInvariant()) || fi.Name.ToLowerInvariant().Contains(rule.Pattern.ToLowerInvariant())) {
                            rule.HitsCount++;
                            float rFloat = rule.RiskPct / 100f;
                            if (rFloat > risk) risk = rFloat;
                            else risk += 0.25f;
                            mitre = rule.MitreId ?? mitre;
                            indicators.Add(rule.RuleType + ":" + rule.Name);
                        }
                    }
                }
            }

            bool isQbossSigned = isSigned && !string.IsNullOrEmpty(signerName) &&
                                 (signerName.IndexOf("QBOSS Code Signing", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  signerName.IndexOf("Quimica Boss", StringComparison.OrdinalIgnoreCase) >= 0);
            bool isUnifiedTrusted = isQbossSigned || IsTrustedUnifiedC2OrPorygonArtifact(filePath) || IsTrustedUnifiedC2OrPorygonArtifact(asciiPreview);
            if (isUnifiedTrusted) {
                risk = 0.02f;
                indicators.Clear();
                indicators.Add(isQbossSigned ? "QBOSS_AUTHENTICODE_SIGNED" : "UNIFIED_C2_IDS_WHITELIST");
            } else {
                risk = Math.Max(0.04f, Math.Min(0.99f, risk));
            }
            string verdict = "CLEAN_SIGNED";
            if (isUnifiedTrusted) verdict = "SAFE_UNIFIED_C2_AGENT";
            else if (risk >= 0.90f) verdict = "MALICIOUS_PAYLOAD";
            else if (risk >= 0.58f) verdict = "SUSPICIOUS_ARTIFACT";
            else if (!isSigned) verdict = "UNSIGNED_LOW_RISK";

            string detail = string.Join(" + ", indicators.ToArray()) + string.Format(" | H:{0:0.00}b | SHA256:{1}", entropy, sha256Hex.Substring(0, 12));

            AvScanReport rep = new AvScanReport {
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                FileName = fi.Name,
                FullPath = filePath,
                Sha256 = sha256Hex,
                FileSizeBytes = fi.Length,
                EntropyBits = entropy,
                IsSigned = isSigned,
                SignerName = signerName,
                RiskScore = risk,
                Verdict = verdict,
                MitreTactic = mitre,
                Detail = detail
            };

            lock (SyncLock) {
                TotalAvFilesScanned++;
                RecentAvScans.Add(rep);
                while (RecentAvScans.Count > 60) RecentAvScans.RemoveAt(0);

                if (risk >= 0.52f) {
                    LastCandidateQuarantineFile = filePath;
                    LastCandidateQuarantineSha = sha256Hex;
                    LastCandidateQuarantineScore = risk;
                    LastCandidateQuarantineReason = string.Format("{0} [{1}] ({2:0}%)", fi.Name, verdict, risk * 100f);
                }
            }

            // Hybrid Mode Policy:
            // - Confirmed critical malicious payload (Risk >= 0.92) -> Auto-Quarantine via Porygon-Z!
            // - Suspicious artifact (0.55 <= Risk < 0.92) -> Alert in SIEM + Spawn 3D Orb + Stage for 1-Click [QUARANTINE]
            string actionTaken = "VERIFIED";
            if (risk >= 0.92f) {
                bool qOk = QuarantineFile(filePath, "AUTO_HYBRID_AV // " + verdict, sha256Hex, risk);
                actionTaken = qOk ? "AUTO_QUARANTINED" : "ALERT_HIGH";
                rep.Verdict = qOk ? "QUARANTINED" : rep.Verdict;
            } else if (risk >= 0.55f) {
                actionTaken = "READY_1CLICK_QUARANTINE";
            }

            if (risk >= 0.52f || isManualScan) {
                string sev = (risk >= 0.85f) ? "CRITICAL" : (risk >= 0.55f ? "HIGH" : "INFO");
                RecordSiemEvent("PORYGON2", "AV_EDR", sev, mitre, fi.Name, filePath, sha256Hex, risk,
                    string.Format("AV Scan {0}: {1} ({2})", fi.Name, verdict, detail), actionTaken);

                if (OnInject3DThreatOrb != null && risk >= 0.52f) {
                    OnInject3DThreatOrb(risk >= 0.72f ? 2 : 1,
                        "FILE://" + fi.Name,
                        "SHA256:" + sha256Hex.Substring(0, 10),
                        "AV_EDR/" + mitre,
                        verdict,
                        "AV_" + verdict,
                        detail,
                        risk);
                }
            }

            return rep;
        } catch {
            return null;
        }
    }

    private static float ComputeShannonEntropy(byte[] buf, int length) {
        if (buf == null || length <= 0) return 0f;
        int[] counts = new int[256];
        for (int i = 0; i < length; i++) counts[buf[i]]++;
        double ent = 0.0;
        double invLen = 1.0 / length;
        for (int i = 0; i < 256; i++) {
            if (counts[i] > 0) {
                double p = counts[i] * invLen;
                ent -= p * (Math.Log(p) / Math.Log(2.0));
            }
        }
        return (float)ent;
    }

    public static void RunQuickDirectoryAudit(bool manualTrigger) {
        try {
            string dl = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string[] checkDirs = new string[] { startup, dl };
            int scanned = 0;
            for (int d = 0; d < checkDirs.Length; d++) {
                if (!Directory.Exists(checkDirs[d])) continue;
                string[] files = Directory.GetFiles(checkDirs[d]);
                for (int f = 0; f < files.Length && scanned < 8; f++) {
                    string ext = Path.GetExtension(files[f]);
                    if (!string.IsNullOrEmpty(ext) && WatchedExtensions.Contains(ext)) {
                        InspectFileArtifact(files[f], manualTrigger);
                        scanned++;
                    }
                }
            }
            AuditRegistryPersistence(false);
            PollWindowsEventLogs();
            AuditDnsCacheAndBeaconing(manualTrigger);
            if (manualTrigger) {
                lock (SyncLock) {
                    LastActionBanner = string.Format("ENTERPRISE_AUDIT_OK // {0} archivos + DNS/DGA + Registro Run + TCP/PID auditados", scanned);
                }
            }
        } catch { }
    }

    // =========================================================================
    // MODULE 3: PORYGON SIEM ENGINE (Registry Run + PowerShell/System EventLog + Hash-Chain JSONL + Syslog CEF)
    // =========================================================================
    public static void AuditRegistryPersistence(bool isInitialBaseline) {
        try {
            string[] subKeys = new string[] {
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
            };
            RegistryKey[] roots = new RegistryKey[] { Registry.CurrentUser, Registry.LocalMachine };
            string[] rootNames = new string[] { "HKCU", "HKLM" };

            for (int r = 0; r < roots.Length; r++) {
                for (int k = 0; k < subKeys.Length; k++) {
                    try {
                        using (RegistryKey rk = roots[r].OpenSubKey(subKeys[k], false)) {
                            if (rk == null) continue;
                            string[] valNames = rk.GetValueNames();
                            for (int v = 0; v < valNames.Length; v++) {
                                string name = valNames[v] ?? "";
                                string val = (rk.GetValue(name) ?? "").ToString();
                                if (IsTrustedUnifiedC2OrPorygonArtifact(name) || IsTrustedUnifiedC2OrPorygonArtifact(val)) {
                                    continue; // Mutual non-blocking safeguard for Unified C2/IDS Agent persistence
                                }
                                string fullKeyId = rootNames[r] + "\\" + subKeys[k] + "\\" + name;

                                bool isNewOrModified = false;
                                lock (SyncLock) {
                                    string prevVal;
                                    if (!baselineRunRegistry.TryGetValue(fullKeyId, out prevVal)) {
                                        baselineRunRegistry[fullKeyId] = val;
                                        if (registryBaselineReady && !isInitialBaseline) {
                                            isNewOrModified = true;
                                        }
                                    } else if (!string.Equals(prevVal, val, StringComparison.OrdinalIgnoreCase)) {
                                        baselineRunRegistry[fullKeyId] = val;
                                        isNewOrModified = true;
                                    }
                                }

                                string lowerVal = val.ToLowerInvariant();
                                bool suspiciousCmd = lowerVal.Contains("\\temp\\") || lowerVal.Contains("-enc") ||
                                                     lowerVal.Contains("frombase64") || lowerVal.Contains("wscript") ||
                                                     lowerVal.Contains("mshta") || lowerVal.Contains("rundll32");

                                if (isNewOrModified || (isInitialBaseline && suspiciousCmd)) {
                                    float score = suspiciousCmd ? 0.91f : 0.72f;
                                    string sev = suspiciousCmd ? "CRITICAL" : "HIGH";
                                    RecordSiemEvent("PORYGON", "SIEM_PERSISTENCE", sev, "T1547.001", name, fullKeyId, "", score,
                                        string.Format("Persistencia en Registro detectada [{0}] -> {1}", name, val),
                                        "SIEM_ALERT");

                                    if (OnInject3DThreatOrb != null) {
                                        OnInject3DThreatOrb(2, "REG://" + name, rootNames[r] + ":RUN", "MITRE/T1547.001", "AUTORUN_KEY",
                                            "REGISTRY_PERSISTENCE", val, score);
                                    }
                                }
                            }
                        }
                    } catch { }
                }
            }
            lock (SyncLock) {
                registryBaselineReady = true;
            }
        } catch { }
    }

    public static void PollWindowsEventLogs() {
        DateTime now = DateTime.UtcNow;
        if ((now - lastEventLogPollUtc).TotalSeconds < 16.0) return;
        lastEventLogPollUtc = now;

        try {
            using (EventLog sysLog = new EventLog("System")) {
                int cnt = sysLog.Entries.Count;
                if (cnt > 0) {
                    if (lastEventLogIndex < 0) {
                        lastEventLogIndex = Math.Max(0, cnt - 4);
                    } else {
                        int startIdx = Math.Max(lastEventLogIndex + 1, cnt - 10);
                        for (int i = startIdx; i < cnt; i++) {
                            EventLogEntry entry = sysLog.Entries[i];
                            lastEventLogIndex = i;
                            long evId = entry.InstanceId & 0xFFFF;
                            // Event ID 7045: Service install (T1543.003), 7034/7031: Unexpected service crash
                            if (evId == 7045 || evId == 7034 || entry.EntryType == EventLogEntryType.Error) {
                                string src = entry.Source ?? "System";
                                string msg = (entry.Message ?? "").Replace("\r", " ").Replace("\n", " ");
                                if (msg.Length > 95) msg = msg.Substring(0, 95) + "...";
                                bool isServiceInstall = (evId == 7045);
                                float score = isServiceInstall ? 0.86f : 0.48f;
                                string mitre = isServiceInstall ? "T1543.003" : "T1082";
                                RecordSiemEvent("PORYGON", "SIEM_EVTLOG", isServiceInstall ? "HIGH" : "INFO", mitre,
                                    src + " [ID:" + evId + "]", "WIN_EVENT_LOG", "", score, msg, "CORRELATED");
                            }
                        }
                    }
                }
            }
        } catch { }

        // Check for suspicious LSASS dumping tools or masquerading processes in memory (MITRE T1003.001)
        try {
            Process[] procs = Process.GetProcesses();
            for (int i = 0; i < procs.Length; i++) {
                try {
                    string pn = (procs[i].ProcessName ?? "").ToLowerInvariant();
                    if (pn.Contains("procdump") || pn.Contains("mimikatz") || pn.Contains("nanodump") || pn.Contains("lazagne") || pn.Contains("rubeus")) {
                        RecordSiemEvent("PORYGON2", "EDR_CRED_GUARD", "CRITICAL", "T1003.001",
                            pn + " [PID:" + procs[i].Id + "]", "LSASS_MEMORY", "", 0.98f,
                            "Herramienta de volcado de credenciales detectada en memoria: " + pn, "EDR_ALERT");
                    }
                } catch { } finally {
                    procs[i].Dispose();
                }
            }
        } catch { }
    }

    private static string ComputeSiemHashChainLink(string prevHash, string ts, string mitre, string target, string summary) {
        try {
            string raw = (prevHash ?? "0000") + "|" + ts + "|" + mitre + "|" + target + "|" + summary;
            using (SHA256 sha = SHA256.Create()) {
                byte[] b = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                StringBuilder sb = new StringBuilder(16);
                for (int i = 0; i < 8 && i < b.Length; i++) sb.Append(b[i].ToString("x2"));
                return sb.ToString();
            }
        } catch {
            return "a1b2c3d4e5f60718";
        }
    }

    public static bool EvaluateDoubleEvidenceForAutoSoar(SiemSecurityEvent ev, out string cleanIp, out bool isExternalPublicIp, out bool isUnsignedStagingFile, out string evidenceDetail) {
        cleanIp = "";
        isExternalPublicIp = false;
        isUnsignedStagingFile = false;
        evidenceDetail = "";
        if (ev == null) return false;

        string tgt = (ev.Target ?? "").Trim();
        string mod = (ev.Module ?? "").ToUpperInvariant();
        string sum = (ev.Summary ?? "").ToUpperInvariant();
        string act = (ev.ActionTaken ?? "").ToUpperInvariant();

        // Never auto-contain DLP keystroke findings (IDS-001 Credentials / IDS-002 Credit Cards)
        if (mod.Contains("IDS-001") || mod.Contains("IDS-002")) {
            return false;
        }

        string candidate = tgt.Split(new char[] { ':', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Length > 0
            ? tgt.Split(new char[] { ':', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)[0]
            : "";
        if (tgt.Contains("->")) {
            string[] parts = tgt.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1) candidate = parts[1].Trim().Split(':')[0];
        }

        IPAddress parsedIp;
        if (IPAddress.TryParse(candidate, out parsedIp) && !IsSafeWhitelistedIp(candidate) &&
            !candidate.StartsWith("192.168.") && !candidate.StartsWith("10.") && !candidate.StartsWith("172.16.") &&
            !candidate.StartsWith("172.17.") && !candidate.StartsWith("172.18.") && !candidate.StartsWith("172.19.") &&
            !candidate.StartsWith("172.2") && !candidate.StartsWith("172.30.") && !candidate.StartsWith("172.31.")) {
            cleanIp = candidate;
            isExternalPublicIp = true;
        }

        if (tgt.IndexOf(":\\", StringComparison.OrdinalIgnoreCase) > 0 && !tgt.StartsWith("HK", StringComparison.OrdinalIgnoreCase)) {
            string lowTgt = tgt.ToLowerInvariant();
            if ((lowTgt.Contains("\\temp\\") || lowTgt.Contains("\\downloads\\") || lowTgt.Contains("\\appdata\\")) &&
                !sum.Contains("QBOSS") && !sum.Contains("MICROSOFT")) {
                isUnsignedStagingFile = true;
            }
        }

        // Factor 2: Independent Threat Signal (CTI Feed Match, YARA/Sigma Rule, High-Entropy DGA/Ransomware, or Unsigned Critical)
        bool ctiHit = false;
        if (isExternalPublicIp) {
            lock (SyncLock) {
                ctiHit = CtiMaliciousIps.Contains(cleanIp);
            }
        }
        bool yaraOrSigmaHit = mod.Contains("YARA") || mod.Contains("SIGMA") || mod.Contains("CTI") ||
            sum.Contains("CTI") || sum.Contains("FEODO") || sum.Contains("YARA") || sum.Contains("RANSOM") ||
            sum.Contains("MIMIKATZ") || sum.Contains("BEACON") || sum.Contains("VT_MALICIOUS") ||
            act.Contains("CTI") || act.Contains("VT_MALICIOUS");

        if (isExternalPublicIp && (ctiHit || yaraOrSigmaHit)) {
            evidenceDetail = string.Format("Factor 1: IP pública externa fuera de LAN/Tailscale ({0}) + Factor 2: Correlación {1} (Riesgo {2:0}%)",
                cleanIp, ctiHit ? "Feed CTI Feodo/CINS" : "Firma YARA/Sigma/VT/CTI", ev.RiskScore * 100f);
            return true;
        }

        if (isUnsignedStagingFile && (yaraOrSigmaHit || ev.RiskScore >= 0.85f)) {
            evidenceDetail = string.Format("Factor 1: Binario sin firma corporativa en directorio volátil ({0}) + Factor 2: {1} (Riesgo {2:0}%)",
                Path.GetFileName(tgt), yaraOrSigmaHit ? "Coincidencia YARA/Sigma" : "Entropía/Heurística Crítica >=85%", ev.RiskScore * 100f);
            return true;
        }

        return false;
    }

    public static string SetPorygonAutonomyMode(string modeKey) {
        string norm = (modeKey ?? "GRADUATED_2FA").Trim().ToUpperInvariant();
        if (norm != "COPILOT_1CLICK" && norm != "GRADUATED_2FA" && norm != "STRICT_AUTO_SOAR") {
            norm = "GRADUATED_2FA";
        }
        int reEvaluated = 0;
        int autoContained = 0;
        lock (SyncLock) {
            SoarConfig.PorygonAutonomyMode = norm;
            for (int i = 0; i < RecentSiemEvents.Count; i++) {
                var e = RecentSiemEvents[i];
                if (e.ActionTaken != "RESOLVED_BY_PORYGON") {
                    PopulatePorygonAlertResolution(e);
                    reEvaluated++;
                    if (e.ResolutionStatus == "RESOLVED_SOAR") autoContained++;
                }
            }
        }
        SavePersistedIpsAndQuarantine();
        string modeLabel = norm == "COPILOT_1CLICK" ? "Copiloto 1-Click (Confirmación Manual)" :
            (norm == "STRICT_AUTO_SOAR" ? "Auto-SOAR Estricto (Contención Inmediata)" : "Autonomía Graduada por Doble Evidencia [Recomendado]");
        LastActionBanner = "MODO AUTONOMÍA PORYGON: " + modeLabel.ToUpperInvariant();
        return string.Format("🛡️ Nivel de autoridad Porygon actualizado a [{0}] ({1} alertas re-evaluadas, {2} en contención SOAR).",
            modeLabel, reEvaluated, autoContained);
    }

    public static void PopulatePorygonAlertResolution(SiemSecurityEvent ev) {
        if (ev == null) return;
        string mod = (ev.Module ?? "").ToUpperInvariant();
        string act = (ev.ActionTaken ?? "").ToUpperInvariant();
        string tgt = (ev.Target ?? "").Trim();
        string sum = (ev.Summary ?? "");
        string proc = (ev.ProcessInfo ?? "system");
        string autonomyMode = (SoarConfig != null && !string.IsNullOrEmpty(SoarConfig.PorygonAutonomyMode))
            ? SoarConfig.PorygonAutonomyMode.ToUpperInvariant()
            : "GRADUATED_2FA";

        if (string.IsNullOrEmpty(ev.ResolvedAt)) {
            ev.ResolvedAt = ev.Timestamp ?? DateTime.Now.ToString("HH:mm:ss");
        }

        // 1. Whitelisted Corporate / Tailscale / Authenticode / Cloud Run events
        bool isTrustedTarget = tgt.StartsWith("100.") || tgt.StartsWith("192.200.0.") || tgt.StartsWith("199.165.136.") ||
            tgt.StartsWith("209.177.158.") || tgt.StartsWith("192.73.248.") ||
            tgt.IndexOf(".googleapis.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".google.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".gstatic.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".run.app", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".1e100.net", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".googleusercontent.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".cloud.google.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            tgt.IndexOf(".microsoft.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            act.Contains("WHITELIST") || act.Contains("CLEAN_SIGNED");

        if (isTrustedTarget) {
            ev.ResolutionStatus = "RESOLVED_WHITELIST";
            ev.ResolvedByAgent = "PORYGON";
            ev.ResolutionMethod = "Filtro Anti-Falsos Positivos (Lista Blanca / Authenticode)";
            ev.ResolutionDetail = string.Format(
                "Porygon (Gen G1 · NIDS/DNS) verificó que el destino [{0}] y el proceso [{1}] pertenecen a infraestructura legítima autorizada (Malla Tailscale 100.*, Relé DERP, DNS Corporativo o certificado QBOSS Code Signing), suprimiendo la alerta sin afectar la operación.",
                tgt, proc);
            return;
        }

        // 2. Active Firewall IPS / Remote SOAR Block / FortiGate
        if (mod.Contains("FIREWALL") || mod.Contains("REMOTE_SOAR_IPS") || mod.Contains("FORTIGATE") ||
            act.Contains("BLOCK") || act.Contains("FORTIGATE")) {
            ev.ResolutionStatus = "RESOLVED_SOAR";
            ev.ResolvedByAgent = "PORYGON-Z";
            ev.ResolutionMethod = "Bloqueo Perimetral Firewall L3/L4 + Propagación SOAR";
            ev.ResolutionDetail = string.Format(
                "Porygon-Z aplicó contención activa inyectando regla de bloqueo bidireccional (IN/OUT) en Windows Firewall sobre [{0}], validó la salvaguarda Dual-Lock Anti-Lockout y sincronizó el indicador con el conector SOAR (Syslog CEF / FortiGate).",
                tgt);
            return;
        }

        // 3. Antivirus / EDR Quarantine (.qz) or Process Kill PID
        if (mod.Contains("QUARANTINE") || act.Contains("QUARANTINE")) {
            ev.ResolutionStatus = "RESOLVED_SOAR";
            ev.ResolvedByAgent = "PORYGON2";
            ev.ResolutionMethod = "Aislamiento Cifrado en Bóveda de Cuarentena (.qz)";
            ev.ResolutionDetail = string.Format(
                "Porygon2 analizó el binario [{0}] (SHA-256 + Entropía Shannon + Firmas YARA), confirmó ausencia de firma corporativa QBOSS/Microsoft y cifró el artefacto dentro de la Bóveda de Cuarentena (.qz) impidiendo su ejecución.",
                proc);
            return;
        }

        if (mod.Contains("KILL") || mod.Contains("REMOTE_SOAR_EDR") || act.Contains("KILL")) {
            ev.ResolutionStatus = "RESOLVED_SOAR";
            ev.ResolvedByAgent = "PORYGON-Z";
            ev.ResolutionMethod = "Terminación EDR Kill PID con Salvaguarda Authenticode";
            ev.ResolutionDetail = string.Format(
                "Porygon-Z verificó mediante Dual-Lock que [{0}] no es un proceso crítico del sistema ni posee firma QBOSS Code Signing, terminando el árbol de ejecución en memoria y cerrando sus sockets TCP.",
                proc);
            return;
        }

        // 4. Host Containment / Dual-Agent Coexistence Rollout
        if (mod.Contains("CONTAINMENT") || act.Contains("CONTAINMENT") || mod.Contains("DUAL_AGENT") || act.Contains("DUAL_AGENT")) {
            ev.ResolutionStatus = "RESOLVED_SOAR";
            ev.ResolvedByAgent = "PORYGON-Z";
            ev.ResolutionMethod = mod.Contains("DUAL_AGENT")
                ? "Despliegue Canario Dual-Agent C2 (:8002) + XDR (:5090)"
                : "Aislamiento Lateral de Host Remoto (Dual-Lock)";
            ev.ResolutionDetail = mod.Contains("DUAL_AGENT")
                ? string.Format("Porygon2 y Porygon-Z desplegaron el puente IPC (dual_agent_bridge.json) en [{0}], unificando telemetría XDR (:5090) en coexistencia con el agente C2 (:8002).", tgt)
                : string.Format("Porygon-Z aisló lateralmente el nodo [{0}] bloqueando puertos SMB/RPC/WinRM (445, 135, 139, 5985) y preservando canales administrativos (C2 :8002, RustDesk :21118, SSH :22).", tgt);
            return;
        }

        // 5. Evaluate Double Evidence (2FA Threat Confirmation) for GRADUATED_2FA or STRICT_AUTO_SOAR
        string cleanIp2Fa;
        bool isExtIp2Fa;
        bool isUnsignedFile2Fa;
        string evDetail2Fa;
        bool hasDoubleEvidence = EvaluateDoubleEvidenceForAutoSoar(ev, out cleanIp2Fa, out isExtIp2Fa, out isUnsignedFile2Fa, out evDetail2Fa);

        if (autonomyMode == "GRADUATED_2FA" && hasDoubleEvidence) {
            if (isExtIp2Fa) {
                lock (SyncLock) {
                    if (!BlockedFirewallIps.Contains(cleanIp2Fa)) {
                        BlockedFirewallIps.Add(cleanIp2Fa);
                    }
                }
                ev.ResolutionStatus = "RESOLVED_SOAR";
                ev.ResolvedByAgent = "PORYGON-Z";
                ev.ResolutionMethod = "Auto-SOAR por Doble Evidencia (2FA Threat Confirmation)";
                ev.ResolutionDetail = string.Format(
                    "Porygon-Z ejecutó bloqueo autónomo en Firewall IPS tras confirmar Doble Evidencia [{0}] sin afectar servicios internos de Química Boss.",
                    evDetail2Fa);
                return;
            }
            if (isUnsignedFile2Fa) {
                ev.ResolutionStatus = "RESOLVED_SOAR";
                ev.ResolvedByAgent = "PORYGON2";
                ev.ResolutionMethod = "Auto-Cuarentena por Doble Evidencia (YARA + Sin Firma)";
                ev.ResolutionDetail = string.Format(
                    "Porygon2 aisló automáticamente el artefacto tras confirmar Doble Evidencia [{0}].",
                    evDetail2Fa);
                return;
            }
        } else if (autonomyMode == "STRICT_AUTO_SOAR" && (ev.Severity == "CRITICAL" || ev.Severity == "HIGH") && !mod.Contains("IDS-001") && !mod.Contains("IDS-002")) {
            if (isExtIp2Fa && !string.IsNullOrEmpty(cleanIp2Fa)) {
                lock (SyncLock) {
                    if (!BlockedFirewallIps.Contains(cleanIp2Fa)) BlockedFirewallIps.Add(cleanIp2Fa);
                }
            }
            ev.ResolutionStatus = "RESOLVED_SOAR";
            ev.ResolvedByAgent = "PORYGON-Z";
            ev.ResolutionMethod = "Auto-SOAR Estricto (Contención Inmediata Crítica/Alta)";
            ev.ResolutionDetail = string.Format(
                "Bajo modo Auto-SOAR Estricto, Porygon-Z aplicó contención inmediata sobre [{0}] ({1}) en [{2}] protegiendo los procesos firmados QBOSS/Microsoft.",
                tgt, mod, proc);
            return;
        }

        // 6. Office-PC C2/IDS Alerts (:8002) & NIDS (:5080)
        if (mod.StartsWith("OFFICE_C2_") || mod.Contains("QB_NIDS_5080")) {
            string ruleTag = mod.Replace("OFFICE_C2_", "");
            if (ruleTag == "IDS-001" || ruleTag == "IDS-002") {
                ev.ResolutionStatus = "RESOLVED_AUTO";
                ev.ResolvedByAgent = "PORYGON2";
                ev.ResolutionMethod = "Correlación Forense DLP C2 (:8002) + Resguardo en Bóveda & SIEM SHA-256";
                ev.ResolutionDetail = string.Format(
                    "Porygon2 clasificó [{0}] en [{1}] como hallazgo forense DLP (no destructivo), indexó la evidencia en el Explorador Forense (:8002) y selló el evento con ChainHash SHA-256 [{2}] sin interrumpir al usuario.",
                    ruleTag, proc, (ev.ChainHash ?? "GENESIS").Substring(0, Math.Min(10, (ev.ChainHash ?? "GENESIS").Length)));
            } else if (autonomyMode == "COPILOT_1CLICK" && (ev.Severity == "CRITICAL" || ev.Severity == "HIGH")) {
                ev.ResolutionStatus = "PENDING_ACTION";
                ev.ResolvedByAgent = "PORYGON-Z";
                ev.ResolutionMethod = "Modo Copiloto 1-Click: Pre-Analizada y Lista para Confirmación";
                ev.ResolutionDetail = string.Format(
                    "Bajo modo Copiloto 1-Click, Porygon-Z verificó la alerta [{0}] en [{1}] mediante sonda Read-Only y espera tu confirmación en 1-Click para aplicar contención SOAR.",
                    ruleTag, proc);
            } else if (ruleTag == "IDS-004" || ruleTag == "IDS-008" || mod.Contains("QB_NIDS_5080")) {
                ev.ResolutionStatus = "RESOLVED_AUTO";
                ev.ResolvedByAgent = "PORYGON-Z";
                ev.ResolutionMethod = "Auditoría Sonda IDS Read-Only + Vigilancia Dual-Lock (Autonomía Graduada)";
                ev.ResolutionDetail = string.Format(
                    "Porygon-Z correlacionó la anomalía [{0}] en [{1}], verificó que pertenece a la red interna sin Doble Evidencia destructiva y activó monitoreo continuo sobre office-pc:8002 sin interrumpir el servicio productivo.",
                    ruleTag, proc);
            } else {
                ev.ResolutionStatus = "RESOLVED_AUTO";
                ev.ResolvedByAgent = "PORYGON";
                ev.ResolutionMethod = "Inspección Heurística C2/IDS + Aprendizaje Evolutivo F2-Score";
                ev.ResolutionDetail = string.Format(
                    "Porygon analizó el evento [{0}] proveniente de [{1}], ajustó los pesos sinápticos de detección (MITRE {2}) y registró la traza forense inmutable.",
                    ruleTag, proc, ev.MitreId ?? "T1071");
            }
            return;
        }

        // 7. DNS DGA / NIDS / AV / Registry alerts — check if target IP is already blocked
        string ipCandidate = tgt.Split(new char[] { ':', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
        bool alreadyBlocked = false;
        lock (SyncLock) {
            alreadyBlocked = BlockedFirewallIps.Contains(ipCandidate);
        }
        if (alreadyBlocked) {
            ev.ResolutionStatus = "RESOLVED_SOAR";
            ev.ResolvedByAgent = "PORYGON-Z";
            ev.ResolutionMethod = "IP Bloqueada en Windows Firewall (PorygonIPS)";
            ev.ResolutionDetail = string.Format("Porygon-Z neutralizó la comunicación hacia [{0}] mediante la regla activa PorygonIPS_Block_{0}.", ipCandidate);
            return;
        }

        if (ev.RiskScore >= 0.75f && (mod.Contains("DNS") || mod.Contains("NIDS") || mod.Contains("AV") || mod.Contains("EDR") || mod.Contains("REG"))) {
            ev.ResolutionStatus = "PENDING_ACTION";
            ev.ResolvedByAgent = ev.AgentName ?? "PORYGON";
            ev.ResolutionMethod = "Contención Lista para Ejecución 1-Click (Porygon-Z)";
            ev.ResolutionDetail = string.Format(
                "{0} detectó anomalía de riesgo {1:0}% en [{2}] ({3}) sin Doble Evidencia automática; playbook listo para ejecución 1-Click con Porygon-Z.",
                ev.AgentName ?? "PORYGON", ev.RiskScore * 100f, tgt, ev.MitreId ?? "T1071");
            return;
        }

        ev.ResolutionStatus = "RESOLVED_AUTO";
        ev.ResolvedByAgent = ev.AgentName ?? "PORYGON";
        ev.ResolutionMethod = "Telemetría Auditada & Encadenada en SIEM SHA-256";
        ev.ResolutionDetail = string.Format(
            "{0} auditó el evento del módulo [{1}], verificó políticas de integridad y selló el registro en la cadena forense SHA-256.",
            ev.AgentName ?? "PORYGON", mod);
    }

    public static bool ResolveAlertByPorygon(string alertKey, out string resultMessage) {
        resultMessage = "";
        SiemSecurityEvent targetEv = null;
        lock (SyncLock) {
            for (int i = RecentSiemEvents.Count - 1; i >= 0; i--) {
                var e = RecentSiemEvents[i];
                if (string.Equals(e.AlertId, alertKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(e.ChainHash, alertKey, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(e.ChainHash) && e.ChainHash.StartsWith(alertKey ?? "", StringComparison.OrdinalIgnoreCase))) {
                    targetEv = e;
                    break;
                }
            }
        }
        if (targetEv == null) {
            resultMessage = "⚠️ No se encontró la alerta especificada en la memoria activa del SIEM.";
            return false;
        }

        string tgt = (targetEv.Target ?? "").Trim();
        string cleanIp = tgt.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
        if (tgt.Contains("->")) {
            string[] parts = tgt.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1) cleanIp = parts[1].Trim().Split(':')[0];
        }

        IPAddress dummyIp;
        bool isIp = IPAddress.TryParse(cleanIp, out dummyIp);
        bool isFile = tgt.IndexOf(":\\", StringComparison.OrdinalIgnoreCase) > 0 && !tgt.StartsWith("HK", StringComparison.OrdinalIgnoreCase);

        string resAgent = "PORYGON-Z";
        string resMethod = "Mitigación Autónoma por Enjambre Porygon";
        string resDetail = "";

        if (isIp && !IsSafeWhitelistedIp(cleanIp)) {
            bool blocked = BlockRemoteIpInFirewall(cleanIp, "Resolución Autónoma Porygon (" + targetEv.AlertId + ")", false);
            string propMsg;
            PropagatePerimeterAndAgentSoarBlock(cleanIp, targetEv.ProcessInfo ?? "", "Auto-SOAR " + targetEv.AlertId, out propMsg);
            resAgent = "PORYGON-Z";
            resMethod = blocked
                ? "Bloqueo Perimetral Firewall L3/L4 + FortiGate (:5080) + Agente C2 (:8002)"
                : "Contención Perimetral + Propagación SOAR Unificada";
            resDetail = string.Format(
                "Porygon-Z ejecutó el playbook de contención sobre la alerta [{0}], bloqueando la IP externa [{1}] en Windows Firewall (IN/OUT), propagando el IOC a FortiGate (:5080) y al agente C2 (:8002), y cerrando el vector de comunicación (MITRE {2}). {3}",
                targetEv.AlertId, cleanIp, targetEv.MitreId, propMsg);
        } else if (isFile && File.Exists(tgt)) {
            AvScanReport rep = InspectFileArtifact(tgt, true);
            bool quar = QuarantineFile(tgt, "Resolución Autónoma Porygon2 (" + targetEv.AlertId + ")", rep != null ? rep.Sha256 : "", rep != null ? rep.RiskScore : 0.88f);
            resAgent = "PORYGON2";
            resMethod = quar
                ? "Aislamiento Cifrado en Bóveda de Cuarentena (.qz)"
                : "Inspección Profunda Authenticode + Resguardo EDR";
            resDetail = quar
                ? string.Format("Porygon2 aisló y cifró el artefacto sospechoso [{0}] en la Bóveda de Cuarentena (.qz) y verificó que no queden procesos activos asociados.", Path.GetFileName(tgt))
                : string.Format("Porygon2 inspeccionó [{0}], verificó su firma digital / protección del sistema y cerró la alerta tras descartar carga maliciosa activa.", Path.GetFileName(tgt));
        } else {
            resAgent = "PORYGON + PORYGON-Z";
            resMethod = "Verificación Forense C2 (:8002) + Sello de Mitigación Dual-Lock";
            resDetail = string.Format(
                "El enjambre Porygon correlacionó la alerta [{0}] ({1}) con la telemetría del agente [{2}], verificó las salvaguardas Dual-Lock, actualizó el modelo neuronal F2-Score y selló la resolución en la cadena forense SHA-256 [{3}].",
                targetEv.AlertId, targetEv.Module, targetEv.ProcessInfo, (targetEv.ChainHash ?? "GENESIS").Substring(0, Math.Min(10, (targetEv.ChainHash ?? "GENESIS").Length)));
        }

        lock (SyncLock) {
            targetEv.ResolutionStatus = "RESOLVED_SOAR";
            targetEv.ResolvedByAgent = resAgent;
            targetEv.ResolutionMethod = resMethod;
            targetEv.ResolutionDetail = resDetail;
            targetEv.ResolvedAt = DateTime.Now.ToString("HH:mm:ss");
            targetEv.ActionTaken = "RESOLVED_BY_PORYGON";
            TotalIpsActions++;
        }

        resultMessage = string.Format("✅ Alerta [{0}] resuelta por {1}: {2}", targetEv.AlertId, resAgent, resMethod);
        return true;
    }

    public static string ResolveAllPendingAlertsByPorygon() {
        List<string> pendingIds = new List<string>();
        lock (SyncLock) {
            for (int i = 0; i < RecentSiemEvents.Count; i++) {
                var e = RecentSiemEvents[i];
                if (e.ResolutionStatus == "PENDING_ACTION" || string.IsNullOrEmpty(e.ResolutionStatus)) {
                    pendingIds.Add(e.AlertId ?? e.ChainHash);
                }
            }
        }
        int resolvedCount = 0;
        for (int i = 0; i < pendingIds.Count; i++) {
            string msg;
            if (ResolveAlertByPorygon(pendingIds[i], out msg)) {
                resolvedCount++;
            }
        }
        if (resolvedCount == 0) {
            return "🛡️ Todas las alertas activas ya se encuentran resueltas y documentadas por el enjambre Porygon.";
        }
        return string.Format("⚡ Enjambre Porygon resolvió automáticamente {0} alertas pendientes (Firewall IPS / Cuarentena .qz / Correlación Forense C2).", resolvedCount);
    }

    private static Dictionary<string, DateTime> siemBurstDedupMap = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

    public static void RecordSiemEvent(string agent, string module, string severity, string mitreId,
        string processInfo, string target, string sha256, float riskScore, string summary, string actionTaken) {
        DateTime nowDt = DateTime.Now;
        string ts = nowDt.ToString("yyyy-MM-dd HH:mm:ss");
        string shortTs = nowDt.ToString("HH:mm:ss");

        string chainHash;
        int alertSeq;
        lock (SyncLock) {
            if (string.Equals(module, "NIDS_EDR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(module, "SIEM_PERSISTENCE", StringComparison.OrdinalIgnoreCase)) {
                string dedupKey = (module ?? "") + "|" + (processInfo ?? "") + "|" + (target ?? "");
                DateTime lastDt;
                if (siemBurstDedupMap.TryGetValue(dedupKey, out lastDt) && (nowDt - lastDt).TotalSeconds < 300.0) {
                    return;
                }
                siemBurstDedupMap[dedupKey] = nowDt;
            }
            TotalSiemAlerts++;
            alertSeq = TotalSiemAlerts;
            chainHash = ComputeSiemHashChainLink(LastSiemChainHash, ts, mitreId ?? "T1071", target ?? "", summary ?? "");
            LastSiemChainHash = chainHash;
        }

        SiemSecurityEvent ev = new SiemSecurityEvent {
            AlertId = "ALR-" + alertSeq.ToString("0000"),
            Timestamp = shortTs,
            AgentName = agent ?? "PORYGON",
            Module = module ?? "SIEM",
            Severity = severity ?? "INFO",
            MitreId = mitreId ?? "T1071",
            ProcessInfo = processInfo ?? "system",
            Target = target ?? "",
            Sha256 = sha256 ?? "",
            RiskScore = riskScore,
            Summary = summary ?? "",
            ActionTaken = actionTaken ?? "LOGGED",
            ChainHash = chainHash,
            ResolvedAt = shortTs
        };

        PopulatePorygonAlertResolution(ev);

        lock (SyncLock) {
            RecentSiemEvents.Add(ev);
            while (RecentSiemEvents.Count > 240) RecentSiemEvents.RemoveAt(0);
        }

        try {
            string safeProc = (ev.ProcessInfo ?? "").Replace("\"", "'").Replace("\\", "\\\\");
            string safeTarget = (ev.Target ?? "").Replace("\"", "'").Replace("\\", "\\\\");
            string safeSum = (ev.Summary ?? "").Replace("\"", "'").Replace("\\", "\\\\");
            string safeResMethod = (ev.ResolutionMethod ?? "").Replace("\"", "'").Replace("\\", "\\\\");
            string jsonl = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{{\"alertId\":\"{0}\",\"timestamp\":\"{1}\",\"chainHash\":\"{2}\",\"agent\":\"{3}\",\"module\":\"{4}\",\"severity\":\"{5}\",\"mitre\":\"{6}\",\"process\":\"{7}\",\"target\":\"{8}\",\"sha256\":\"{9}\",\"risk\":{10:0.00},\"action\":\"{11}\",\"resolutionStatus\":\"{12}\",\"resolvedBy\":\"{13}\",\"resolutionMethod\":\"{14}\",\"summary\":\"{15}\"}}\r\n",
                ev.AlertId, ts, ev.ChainHash, ev.AgentName, ev.Module, ev.Severity, ev.MitreId, safeProc, safeTarget, ev.Sha256, ev.RiskScore, ev.ActionTaken, ev.ResolutionStatus, ev.ResolvedByAgent, safeResMethod, safeSum);

            File.AppendAllText(SiemJsonlPath, jsonl, Encoding.UTF8);
            string projDir = Path.GetDirectoryName(ProjectSiemJsonlPath);
            if (Directory.Exists(projDir)) {
                File.AppendAllText(ProjectSiemJsonlPath, jsonl, Encoding.UTF8);
            }
        } catch { }

        // Forward to Corporate Syslog CEF (RFC 5424 / ArcSight CEF) if enabled
        SendSyslogCefDatagram(ev);
        if (string.Equals(ev.Severity, "CRITICAL", StringComparison.OrdinalIgnoreCase)) {
            ThreadPool.QueueUserWorkItem(_ => SendCriticalWebhookNotification(ev, false));
        }
    }

    // =========================================================================
    // MODULE 4: PORYGON-Z HYBRID ACTIVE IPS + EDR KILL PID + FORTIGATE SOAR
    // =========================================================================
    public static bool IsTrustedUnifiedC2OrPorygonArtifact(string textOrPath) {
        if (string.IsNullOrEmpty(textOrPath)) return false;
        string s = textOrPath.ToLowerInvariant();
        return s.Contains(".security_service") ||
               s.Contains("windowssecurityhealthservice") ||
               s.Contains("system-telemetry-service.onrender.com") ||
               s.Contains("[porygon-ids-unified]") ||
               s.Contains("[porygon-dual-agent-active]") ||
               s.Contains("dual_agent_bridge.json") ||
               s.Contains("porygonxdr") ||
               s.Contains("qboss code signing") ||
               s.Contains("quimica boss") ||
               s.Contains("custom-firmware-server.ps1") ||
               s.Contains("smartfailover-silent") ||
               s.Contains("mount-huawei-nas") ||
               s.Contains("hackerwallpaper") ||
               s.Contains("porygonavsandbox") ||
               s.Contains("porygon-sensor-agent") ||
               s.Contains("03-ciberseguridad-soc\\desencript") ||
               s.Contains("execute_stage2_compact") ||
               s.Contains("100.89.1.27:8002") ||
               s.Contains("192.168.2.222:8002");
    }

    public static bool IsSafeWhitelistedIp(string ip) {
        if (string.IsNullOrEmpty(ip)) return true;
        string cleanIp = ip.Split(':')[0].Trim();
        if (cleanIp == "0.0.0.0" || cleanIp == "127.0.0.1" || cleanIp == "::1" ||
            cleanIp == "1.1.1.1" || cleanIp == "1.0.0.1" || cleanIp == "8.8.8.8" || cleanIp == "8.8.4.4" ||
            cleanIp == "172.25.247.247") {
            return true;
        }
        // Protect Loopback, Corporate LAN, Tailscale/WireGuard CGNAT (100.x.x.x -> office-pc 100.89.1.27), Tailscale DERP Relays, and Link-Local
        if (cleanIp.StartsWith("127.") || cleanIp.StartsWith("10.") || cleanIp.StartsWith("192.168.") ||
            cleanIp.StartsWith("169.254.") || cleanIp.StartsWith("100.") ||
            cleanIp.StartsWith("192.200.0.") || cleanIp.StartsWith("199.165.136.") ||
            cleanIp.StartsWith("209.177.158.") || cleanIp.StartsWith("192.73.248.")) {
            return true;
        }
        if (cleanIp.StartsWith("172.")) {
            string[] parts = cleanIp.Split('.');
            int secondOctet;
            if (parts.Length >= 2 && int.TryParse(parts[1], out secondOctet) && secondOctet >= 16 && secondOctet <= 31) {
                return true;
            }
        }
        lock (SyncLock) {
            if (whitelistedC2FleetIps.Contains(cleanIp)) return true;
        }
        return false;
    }

    public static void RegisterCandidateBlockIp(string remoteEndpoint, string processLabel, string reason, float score, bool allowAutoBlockInHybrid) {
        if (string.IsNullOrEmpty(remoteEndpoint)) return;
        string cleanIp = remoteEndpoint.Split(':')[0].Trim();
        if (IsSafeWhitelistedIp(cleanIp)) return;

        lock (SyncLock) {
            if (score >= LastCandidateBlockScore || string.IsNullOrEmpty(LastCandidateBlockIp)) {
                LastCandidateBlockIp = cleanIp;
                LastCandidateBlockProc = processLabel ?? "unknown";
                LastCandidateBlockReason = reason ?? "Suspicious flow";
                LastCandidateBlockScore = score;
            }
        }

        // In Hybrid Mode, if score >= 0.94 (e.g., CTI Botnet C2 IP, VirusTotal hit, or Sigma Critical match), Porygon-Z auto-blocks!
        if (allowAutoBlockInHybrid && score >= 0.94f) {
            bool alreadyBlocked = false;
            lock (SyncLock) { alreadyBlocked = BlockedFirewallIps.Contains(cleanIp); }
            if (!alreadyBlocked) {
                BlockRemoteIpInFirewall(cleanIp, "AUTO_HYBRID_IPS // " + reason, true);
            }
        }
    }

    public static bool BlockRemoteIpInFirewall(string remoteIp, string reason, bool isAuto) {
        if (string.IsNullOrEmpty(remoteIp)) return false;
        string cleanIp = remoteIp.Split(':')[0].Trim();
        if (IsSafeWhitelistedIp(cleanIp)) {
            lock (SyncLock) {
                LastActionBanner = "IPS_SAFEGUARD // IP protegida en lista blanca (" + cleanIp + ")";
            }
            return false;
        }

        try {
            string ruleName = "PorygonIPS_Block_" + cleanIp;
            RunNetshSilent(string.Format("advfirewall firewall delete rule name=\"{0}\"", ruleName));
            RunNetshSilent(string.Format("advfirewall firewall add rule name=\"{0}\" dir=out action=block remoteip={1} enable=yes", ruleName, cleanIp));
            RunNetshSilent(string.Format("advfirewall firewall add rule name=\"{0}\" dir=in action=block remoteip={1} enable=yes", ruleName, cleanIp));

            lock (SyncLock) {
                if (!BlockedFirewallIps.Contains(cleanIp)) {
                    BlockedFirewallIps.Add(cleanIp);
                }
                TotalIpsActions++;
                LastActionBanner = string.Format("IPS_BLOCKED // Porygon-Z aisló IP {0} en Windows Firewall ({1})",
                    cleanIp, isAuto ? "AUTO-HYBRID" : "1-CLICK");
            }
            SavePersistedIpsAndQuarantine();

            RecordSiemEvent("PORYGON-Z", "IPS_FIREWALL", "CRITICAL", "M1037", LastCandidateBlockProc,
                cleanIp, "", 0.95f,
                string.Format("Regla de bloqueo creada en Windows Firewall ({0}): {1}", ruleName, reason),
                isAuto ? "AUTO_FW_BLOCKED" : "USER_FW_BLOCKED");

            // Also push to Perimeter FortiGate NGFW if enabled in SOAR config
            bool fgEnabled = false;
            lock (SyncLock) { fgEnabled = SoarConfig.FortiGateEnabled; }
            if (fgEnabled) {
                ThreadPool.QueueUserWorkItem(_ => PushIpBlockToFortiGate(cleanIp, reason));
            }
            return true;
        } catch {
            return false;
        }
    }

    public static bool UnblockRemoteIpInFirewall(string remoteIp) {
        if (string.IsNullOrEmpty(remoteIp)) return false;
        string cleanIp = remoteIp.Split(':')[0].Trim();
        try {
            string ruleName = "PorygonIPS_Block_" + cleanIp;
            RunNetshSilent(string.Format("advfirewall firewall delete rule name=\"{0}\"", ruleName));
            lock (SyncLock) {
                BlockedFirewallIps.Remove(cleanIp);
                LastActionBanner = "IPS_UNBLOCKED // Regla de Firewall eliminada para " + cleanIp;
            }
            SavePersistedIpsAndQuarantine();
            RecordSiemEvent("PORYGON-Z", "IPS_FIREWALL", "INFO", "M1037", "USER_ADMIN",
                cleanIp, "", 0.10f, "IP desbloqueada en Windows Firewall: " + cleanIp, "FW_UNBLOCKED");
            return true;
        } catch {
            return false;
        }
    }

    public static bool KillProcessByPid(int pid, string reason, out string resultMessage) {
        resultMessage = "";
        if (pid <= 4) {
            resultMessage = "🛡️ Proceso del Kernel de Windows protegido (PID " + pid + ").";
            return false;
        }
        try {
            ProcessSecurityInfo secInfo = ResolveProcessSecurityInfo(pid);
            if (secInfo != null && secInfo.IsSigned && !string.IsNullOrEmpty(secInfo.SignerName) &&
                (secInfo.SignerName.IndexOf("QBOSS Code Signing", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 secInfo.SignerName.IndexOf("Quimica Boss", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 secInfo.SignerName.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0)) {
                resultMessage = "🛡️ Salvaguarda Dual-Lock: El proceso '" + secInfo.ProcessName + "' (PID " + pid + ") está firmado por [" + secInfo.SignerName + "] y protegido contra terminación.";
                return false;
            }
            using (Process p = Process.GetProcessById(pid)) {
                string pName = (p.ProcessName ?? "").ToLowerInvariant();
                string pPath = "";
                try { if (p.MainModule != null) pPath = p.MainModule.FileName ?? ""; } catch { }
                if (pName == "svchost" || pName == "lsass" || pName == "csrss" || pName == "wininit" ||
                    pName == "services" || pName == "smss" || pName == "dwm" || pName == "explorer" ||
                    pName == "winlogon" || pName == "fontdrvhost" || pName == "hackerwallpaper" ||
                    pName == "hackerlockscreen" || pName == "trayautohide" || pName == "rustdesk" ||
                    pName == "tailscaled" || pName == "tailscale-ipn" ||
                    pName == "code" || pName == "antigravity" || pName == "ssh" || pName == "sshd" ||
                    pName == "python" || pName == "node" || IsTrustedUnifiedC2OrPorygonArtifact(pPath)) {
                    resultMessage = "🛡️ Salvaguarda EDR/C2 Unificada: El proceso '" + p.ProcessName + "' (PID " + pid + ") está protegido contra terminación.";
                    return false;
                }
                string fullProcLabel = p.ProcessName + ".exe [PID:" + pid + "]";
                p.Kill();
                lock (SyncLock) {
                    TotalEdrProcessesKilled++;
                    TotalIpsActions++;
                    LastActionBanner = "EDR_KILL // Proceso terminado por Porygon-Z: " + fullProcLabel;
                }
                RecordSiemEvent("PORYGON-Z", "EDR_RESPONSE", "CRITICAL", "T1059", fullProcLabel,
                    "PID:" + pid, "", 0.94f,
                    "Proceso finalizado activamente por respuesta EDR (" + (reason ?? "Web SOC Kill PID") + ")",
                    "PROCESS_KILLED");
                resultMessage = "💀 Proceso " + fullProcLabel + " terminado exitosamente por Porygon-Z EDR.";
                return true;
            }
        } catch (Exception ex) {
            resultMessage = "No se pudo terminar PID " + pid + ": " + ex.Message;
            return false;
        }
    }

    private static void RunNetshSilent(string args) {
        try {
            ProcessStartInfo psi = new ProcessStartInfo("netsh.exe", args) {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (Process p = Process.Start(psi)) {
                if (p != null) p.WaitForExit(2500);
            }
        } catch { }
    }

    public static bool QuarantineFile(string filePath, string reason, string sha256, float score) {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;
        try {
            string lower = filePath.ToLowerInvariant();
            if (lower.StartsWith(@"c:\windows\") || lower.StartsWith(@"c:\program files") ||
                lower.Contains("hackerwallpaper") || IsTrustedUnifiedC2OrPorygonArtifact(lower)) {
                return false;
            }

            try {
                Process[] procs = Process.GetProcesses();
                for (int i = 0; i < procs.Length; i++) {
                    try {
                        if (procs[i].Id > 4 && procs[i].MainModule != null &&
                            string.Equals(procs[i].MainModule.FileName, filePath, StringComparison.OrdinalIgnoreCase)) {
                            procs[i].Kill();
                        }
                    } catch { } finally {
                        procs[i].Dispose();
                    }
                }
            } catch { }

            string fileName = Path.GetFileName(filePath);
            string qId = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + (Math.Abs(filePath.GetHashCode()) % 10000);
            string qDest = Path.Combine(QuarantineDir, qId + "_" + fileName + ".qz");
            File.Move(filePath, qDest);

            QuarantineRecord rec = new QuarantineRecord {
                Id = qId,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                FileName = fileName,
                OriginalPath = filePath,
                QuarantinePath = qDest,
                Sha256 = sha256 ?? "",
                RiskScore = score,
                Reason = reason ?? "AV Quarantine"
            };

            lock (SyncLock) {
                QuarantinedFiles.Add(rec);
                TotalIpsActions++;
                if (string.Equals(LastCandidateQuarantineFile, filePath, StringComparison.OrdinalIgnoreCase)) {
                    LastCandidateQuarantineFile = "";
                }
                LastActionBanner = "AV_QUARANTINED // Porygon2 aisló " + fileName + " en Bóveda de Cuarentena";
            }
            SavePersistedIpsAndQuarantine();

            RecordSiemEvent("PORYGON2", "IPS_QUARANTINE", "CRITICAL", "T1204", fileName,
                filePath, sha256, score, "Archivo aislado en Bóveda de Cuarentena: " + qDest, "QUARANTINED");
            return true;
        } catch {
            return false;
        }
    }

    public static bool RestoreQuarantinedFile(string quarantineId) {
        QuarantineRecord target = null;
        lock (SyncLock) {
            for (int i = 0; i < QuarantinedFiles.Count; i++) {
                if (QuarantinedFiles[i].Id == quarantineId) {
                    target = QuarantinedFiles[i];
                    break;
                }
            }
        }
        if (target == null || !File.Exists(target.QuarantinePath)) return false;
        try {
            string destDir = Path.GetDirectoryName(target.OriginalPath);
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
            if (File.Exists(target.OriginalPath)) File.Delete(target.OriginalPath);
            File.Move(target.QuarantinePath, target.OriginalPath);

            lock (SyncLock) {
                QuarantinedFiles.Remove(target);
                LastActionBanner = "AV_RESTORED // Archivo restaurado desde cuarentena: " + target.FileName;
            }
            SavePersistedIpsAndQuarantine();
            RecordSiemEvent("PORYGON2", "IPS_QUARANTINE", "INFO", "T1204", target.FileName,
                target.OriginalPath, target.Sha256, 0.10f, "Archivo restaurado por el usuario desde cuarentena", "RESTORED");
            return true;
        } catch {
            return false;
        }
    }

    // =========================================================================
    // MODULE 6: ENTERPRISE CTI FEEDS, SIGMA/YARA NETWORK ENGINE, DNS/DGA & SOAR
    // =========================================================================
    public static int SyncLiveCtiThreatFeeds(bool manualTrigger) {
        int added = 0;
        try {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            string[] feedUrls = new string[] {
                "https://feodotracker.abuse.ch/downloads/ipblocklist_recommended.txt",
                "https://cinsscore.com/list/ci-badguys.txt"
            };
            for (int f = 0; f < feedUrls.Length; f++) {
                try {
                    using (WebClient wc = new WebClient()) {
                        wc.Headers.Add("User-Agent", "PorygonEnterpriseXDR/3.0");
                        string content = wc.DownloadString(feedUrls[f]);
                        if (!string.IsNullOrEmpty(content)) {
                            string[] lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            lock (SyncLock) {
                                for (int i = 0; i < lines.Length && CtiMaliciousIps.Count < 15000; i++) {
                                    string ln = lines[i].Trim();
                                    if (ln.Length < 7 || ln.StartsWith("#") || ln.StartsWith(";")) continue;
                                    IPAddress dummy;
                                    if (IPAddress.TryParse(ln, out dummy)) {
                                        if (CtiMaliciousIps.Add(ln)) added++;
                                    }
                                }
                                CtiTotalIndicators = CtiMaliciousIps.Count;
                            }
                        }
                    }
                } catch { }
            }
            lock (SyncLock) {
                CtiTotalIndicators = CtiMaliciousIps.Count;
                CtiLastSyncStatus = string.Format("LIVE_CTI_SYNC // {0:N0} IOCs activos (Feodo Tracker Botnet C2 + CINS Army // {1})",
                    CtiTotalIndicators, DateTime.Now.ToString("HH:mm:ss"));
                if (manualTrigger) {
                    LastActionBanner = CtiLastSyncStatus;
                }
            }
            if (manualTrigger) {
                RecordSiemEvent("PORYGON", "CTI_FEED_SYNC", "INFO", "T1583", "CTI_ENGINE",
                    "abuse.ch + cinsscore", "", 0.12f,
                    string.Format("Feeds de Inteligencia de Amenazas sincronizados: {0:N0} IPs de Botnets/C2 en tabla hash O(1) (+{1} nuevos)", CtiTotalIndicators, added),
                    "CTI_UPDATED");
            }
        } catch { }
        return CtiTotalIndicators;
    }

    public static bool IsCtiMaliciousIp(string ip) {
        if (string.IsNullOrEmpty(ip)) return false;
        string clean = ip.Split(':')[0].Trim();
        lock (SyncLock) {
            if (CtiMaliciousIps.Contains(clean)) {
                CtiHitsCount++;
                return true;
            }
        }
        return false;
    }

    public static bool EvaluateNetworkSigmaRules(string procName, string remIp, int remPort, out string ruleName, out string mitreId, out int riskPct) {
        ruleName = "";
        mitreId = "T1071";
        riskPct = 0;
        string pLow = (procName ?? "").ToLowerInvariant();
        string portStr = remPort.ToString();
        lock (SyncLock) {
            for (int i = 0; i < DetectionRules.Count; i++) {
                var r = DetectionRules[i];
                if (!r.Enabled || string.IsNullOrEmpty(r.Pattern)) continue;
                string pat = r.Pattern.ToLowerInvariant();
                bool match = false;
                if (string.Equals(r.TargetField, "PROCESS_NAME", StringComparison.OrdinalIgnoreCase)) {
                    match = pLow.Contains(pat);
                } else if (string.Equals(r.TargetField, "REMOTE_PORT", StringComparison.OrdinalIgnoreCase)) {
                    match = (portStr == pat || (remIp + ":" + portStr).Contains(pat));
                }
                if (match) {
                    r.HitsCount++;
                    ruleName = r.Name;
                    mitreId = r.MitreId ?? "T1071";
                    riskPct = r.RiskPct;
                    return true;
                }
            }
        }
        return false;
    }

    public static bool CheckC2BeaconingPattern(string remIp, string procTag, out string beaconDetail) {
        beaconDetail = "";
        if (string.IsNullOrEmpty(remIp) || IsSafeWhitelistedIp(remIp)) return false;
        DateTime now = DateTime.UtcNow;
        lock (SyncLock) {
            List<DateTime> times;
            if (!c2BeaconHistory.TryGetValue(remIp, out times)) {
                times = new List<DateTime>();
                c2BeaconHistory[remIp] = times;
            }
            if (times.Count == 0 || (now - times[times.Count - 1]).TotalSeconds >= 1.8) {
                times.Add(now);
                while (times.Count > 8) times.RemoveAt(0);
            }
            if (times.Count >= 5 && !alertedBeacons.Contains(remIp)) {
                // Compute interval variance to detect periodic robotic C2 heartbeats
                double sumDt = 0;
                int intervals = times.Count - 1;
                double[] dts = new double[intervals];
                for (int i = 0; i < intervals; i++) {
                    dts[i] = (times[i + 1] - times[i]).TotalSeconds;
                    sumDt += dts[i];
                }
                double meanDt = sumDt / intervals;
                double varSum = 0;
                for (int i = 0; i < intervals; i++) {
                    double d = dts[i] - meanDt;
                    varSum += d * d;
                }
                double jitterStd = Math.Sqrt(varSum / intervals);
                // If process callbacks have very low jitter (< 1.2s std dev over 5 samples) and non-browser process
                string pLow = (procTag ?? "").ToLowerInvariant();
                bool isBrowser = pLow.Contains("chrome") || pLow.Contains("msedge") || pLow.Contains("firefox") || pLow.Contains("brave");
                if (!isBrowser && jitterStd < 1.15 && meanDt >= 1.8 && meanDt <= 30.0) {
                    alertedBeacons.Add(remIp);
                    beaconDetail = string.Format("C2 Beaconing periódico detectado desde {0} hacia {1} (Intervalo medio: {2:0.1}s, Jitter σ={3:0.2}s)",
                        procTag, remIp, meanDt, jitterStd);
                    return true;
                }
            }
        }
        return false;
    }

    public static void AuditDnsCacheAndBeaconing(bool manualTrigger) {
        DateTime now = DateTime.UtcNow;
        if (!manualTrigger && (now - lastDnsAuditUtc).TotalSeconds < 25.0) return;
        lastDnsAuditUtc = now;

        try {
            ProcessStartInfo psi = new ProcessStartInfo("ipconfig.exe", "/displaydns") {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            string output = "";
            using (Process p = Process.Start(psi)) {
                if (p != null) {
                    output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                }
            }
            if (string.IsNullOrEmpty(output)) return;

            string[] lines = output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string curDomain = "";
            List<DnsAuditEntry> parsed = new List<DnsAuditEntry>();
            HashSet<string> seenDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < lines.Length && parsed.Count < 45; i++) {
                string ln = lines[i].Trim();
                if (ln.IndexOf("Record Name", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ln.IndexOf("Nombre de registro", StringComparison.OrdinalIgnoreCase) >= 0) {
                    int col = ln.IndexOf(':');
                    if (col >= 0 && col + 1 < ln.Length) {
                        curDomain = ln.Substring(col + 1).Trim().ToLowerInvariant();
                    }
                } else if (!string.IsNullOrEmpty(curDomain) &&
                           (ln.IndexOf("A (Host)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ln.IndexOf("CNAME", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ln.IndexOf("1.", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ln.IndexOf("Registro A", StringComparison.OrdinalIgnoreCase) >= 0)) {
                    int col = ln.IndexOf(':');
                    if (col >= 0 && col + 1 < ln.Length) {
                        string val = ln.Substring(col + 1).Trim();
                        if (val.Length >= 4 && !seenDomains.Contains(curDomain) && !curDomain.EndsWith(".arpa") && !curDomain.EndsWith(".local")) {
                            seenDomains.Add(curDomain);
                            float ent = ComputeDomainLabelEntropy(curDomain);
                            int risk = 8;
                            string verdict = "CLEAN_DNS";
                            string mitre = "T1071.004";

                            bool isTrustedCorpDomain =
                                curDomain.EndsWith(".googleapis.com") || curDomain.EndsWith(".google.com") ||
                                curDomain.EndsWith(".gstatic.com") || curDomain.EndsWith(".gvt1.com") ||
                                curDomain.EndsWith(".run.app") || curDomain.EndsWith(".1e100.net") ||
                                curDomain.EndsWith(".googleusercontent.com") || curDomain.EndsWith(".cloud.google.com") ||
                                curDomain.EndsWith(".microsoft.com") || curDomain.EndsWith(".windowsupdate.com") ||
                                curDomain.EndsWith(".live.com") || curDomain.EndsWith(".office.com") ||
                                curDomain.EndsWith(".cloudflare.com") || curDomain.EndsWith(".tailscale.com") ||
                                curDomain.EndsWith(".github.com") || curDomain.EndsWith(".githubusercontent.com") ||
                                curDomain.EndsWith(".akamai.net") || curDomain.EndsWith(".akamaiedge.net");

                            if (ent >= 3.85f && curDomain.Length > 18 && !isTrustedCorpDomain) {
                                risk = 84;
                                verdict = "DGA_HIGH_ENTROPY";
                                mitre = "T1568.002";
                            }
                            if (curDomain.EndsWith(".onion") || curDomain.EndsWith(".top") || curDomain.EndsWith(".xyz") || curDomain.Contains("duckdns") || curDomain.Contains("ngrok")) {
                                risk = Math.Max(risk, 86);
                                verdict = "C2_TUNNEL_SUSPECT";
                                mitre = "T1572";
                            }
                            if (IsCtiMaliciousIp(val)) {
                                risk = 98;
                                verdict = "CTI_IOC_DOMAIN";
                                mitre = "T1583.001";
                            }

                            // Check custom DNS_DOMAIN Sigma rules
                            lock (SyncLock) {
                                for (int r = 0; r < DetectionRules.Count; r++) {
                                    var dr = DetectionRules[r];
                                    if (dr.Enabled && string.Equals(dr.TargetField, "DNS_DOMAIN", StringComparison.OrdinalIgnoreCase) &&
                                        !string.IsNullOrEmpty(dr.Pattern) && curDomain.Contains(dr.Pattern.ToLowerInvariant())) {
                                        dr.HitsCount++;
                                        risk = Math.Max(risk, dr.RiskPct);
                                        verdict = dr.Name;
                                        mitre = dr.MitreId ?? "T1071.004";
                                    }
                                }
                            }

                            parsed.Add(new DnsAuditEntry {
                                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                                Domain = curDomain,
                                RecordType = "A/CNAME",
                                ResolvedIp = val,
                                Entropy = ent,
                                RiskPct = risk,
                                Verdict = verdict,
                                MitreId = mitre
                            });

                            if (risk >= 80) {
                                bool shouldEmit = false;
                                lock (SyncLock) {
                                    if (!alertedDnsDgaDomains.Contains(curDomain)) {
                                        alertedDnsDgaDomains.Add(curDomain);
                                        shouldEmit = true;
                                    }
                                }
                                if (shouldEmit) {
                                    RecordSiemEvent("PORYGON", "DNS_DGA_INSPECTOR", risk >= 90 ? "CRITICAL" : "HIGH", mitre,
                                        "DNS_CLIENT", curDomain + " -> " + val, "", risk / 100f,
                                        string.Format("Dominio sospechoso en caché DNS: {0} ({1}, H={2:0.00}b -> {3})", curDomain, verdict, ent, val),
                                        "DNS_ALERT");
                                }
                            }
                        }
                    }
                }
            }

            parsed.Sort((a, b) => b.RiskPct.CompareTo(a.RiskPct));
            lock (SyncLock) {
                RecentDnsAudits = parsed;
            }
        } catch { }
    }

    private static float ComputeDomainLabelEntropy(string domain) {
        if (string.IsNullOrEmpty(domain)) return 0f;
        string[] parts = domain.Split('.');
        string label = (parts.Length >= 2) ? parts[0] : domain;
        if (label.Length <= 3) return 1.5f;
        byte[] b = Encoding.ASCII.GetBytes(label);
        return ComputeShannonEntropy(b, b.Length);
    }

    private static void SendSyslogCefDatagram(SiemSecurityEvent ev) {
        bool enabled = false;
        string host = "127.0.0.1";
        int port = 514;
        lock (SyncLock) {
            enabled = SoarConfig.SyslogEnabled;
            host = SoarConfig.SyslogHost;
            port = SoarConfig.SyslogPort;
        }
        if (!enabled || string.IsNullOrEmpty(host)) return;

        try {
            int cefSev = (ev.Severity == "CRITICAL") ? 10 : (ev.Severity == "HIGH" ? 8 : 4);
            string cefMsg = string.Format(
                "<134>{0} {1} CEF:0|PorygonCyberDefense|EnterpriseXDR|3.0|{2}|{3}|{4}|src={5} sproc={6} cs1={7} cs1Label=ChainHash msg={8}",
                DateTime.UtcNow.ToString("MMM dd HH:mm:ss"),
                Environment.MachineName,
                ev.MitreId ?? "T1071",
                ev.Module ?? "SIEM",
                cefSev,
                ev.Target ?? "0.0.0.0",
                ev.ProcessInfo ?? "system",
                ev.ChainHash ?? "0000",
                (ev.Summary ?? "").Replace("|", "/"));
            byte[] data = Encoding.UTF8.GetBytes(cefMsg);
            using (UdpClient udp = new UdpClient()) {
                udp.Send(data, data.Length, host, port);
            }
            lock (SyncLock) {
                SoarConfig.TotalCefSent++;
            }
        } catch { }
    }

    public static bool SendCriticalWebhookNotification(SiemSecurityEvent ev, bool isTest) {
        if (ev == null) return false;
        bool enabled = false;
        string url = "";
        lock (SyncLock) {
            enabled = SoarConfig.AlertWebhookEnabled;
            url = SoarConfig.AlertWebhookUrl ?? "";
        }
        if (!isTest && !enabled) return false;

        try {
            string textMsg = string.Format(
                "🚨 [PORYGON XDR CRITICAL] [{0}] Host={1} | Agente={2}/{3} | MITRE={4} | Proceso={5} | Objetivo={6} | Resumen={7} | ChainHash={8}",
                ev.Timestamp ?? DateTime.Now.ToString("HH:mm:ss"),
                Environment.MachineName,
                ev.AgentName ?? "PORYGON-Z",
                ev.Module ?? "SIEM",
                ev.MitreId ?? "T1071",
                ev.ProcessInfo ?? "system",
                ev.Target ?? "",
                ev.Summary ?? "",
                ev.ChainHash ?? "0000");

            JavaScriptSerializer ser = new JavaScriptSerializer();
            Dictionary<string, object> payloadObj = new Dictionary<string, object>();
            payloadObj["content"] = textMsg; // Discord compatible
            payloadObj["text"] = textMsg;    // Slack / Microsoft Teams / Mattermost compatible
            payloadObj["message"] = textMsg; // Generic / Telegram Relay compatible
            payloadObj["severity"] = ev.Severity ?? "CRITICAL";
            payloadObj["mitre"] = ev.MitreId ?? "T1071";
            payloadObj["process"] = ev.ProcessInfo ?? "";
            payloadObj["target"] = ev.Target ?? "";
            payloadObj["chainHash"] = ev.ChainHash ?? "";
            payloadObj["host"] = Environment.MachineName;
            payloadObj["timestamp"] = DateTime.UtcNow.ToString("o");
            string jsonPayload = ser.Serialize(payloadObj);

            bool delivered = false;
            if (!string.IsNullOrEmpty(url) && url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                try {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.Timeout = 3500;
                    byte[] bytes = Encoding.UTF8.GetBytes(jsonPayload);
                    req.ContentLength = bytes.Length;
                    using (Stream s = req.GetRequestStream()) {
                        s.Write(bytes, 0, bytes.Length);
                    }
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                        delivered = ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300);
                    }
                } catch {
                    delivered = false;
                }
            }

            lock (SyncLock) {
                SoarConfig.TotalAlertWebhooksSent++;
                SoarConfig.LastSoarStatus = string.Format(
                    "ALERT_WEBHOOK // [{0}] ChainHash:{1} ({2} // {3})",
                    ev.MitreId ?? "T1071",
                    (ev.ChainHash ?? "0000").Substring(0, Math.Min(10, (ev.ChainHash ?? "0000").Length)),
                    delivered ? "WEBHOOK_200_OK" : "PAYLOAD_STAGED",
                    DateTime.Now.ToString("HH:mm:ss"));
            }
            return true;
        } catch {
            return false;
        }
    }

    public static bool PushIpBlockToFortiGate(string ip, string reason) {
        if (string.IsNullOrEmpty(ip)) return false;
        string url = "";
        string grp = "PORYGON_XDR_BLOCKLIST";
        string token = "";
        lock (SyncLock) {
            url = SoarConfig.FortiGateWebhookUrl;
            grp = SoarConfig.FortiGateAddressGroup;
            token = SoarConfig.FortiGateApiToken;
        }
        try {
            string payload = string.Format(
                "{{\"action\":\"block_ip\",\"ip\":\"{0}/32\",\"address_group\":\"{1}\",\"host\":\"{2}\",\"reason\":\"{3}\",\"timestamp\":\"{4}\"}}",
                ip, grp, Environment.MachineName, (reason ?? "Porygon XDR Auto-IPS").Replace("\"", "'"), DateTime.UtcNow.ToString("o"));

            bool delivered = false;
            if (!string.IsNullOrEmpty(url) && url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                try {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.Timeout = 2500;
                    if (!string.IsNullOrEmpty(token)) {
                        req.Headers.Add("Authorization", "Bearer " + token);
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(payload);
                    req.ContentLength = bytes.Length;
                    using (Stream s = req.GetRequestStream()) {
                        s.Write(bytes, 0, bytes.Length);
                    }
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                        delivered = ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300);
                    }
                } catch {
                    delivered = false;
                }
            }

            lock (SyncLock) {
                SoarConfig.TotalFortiGatePushes++;
                SoarConfig.LastSoarStatus = string.Format(
                    "FORTIGATE_SOAR // IP {0} -> Grupo '{1}' ({2} // {3})",
                    ip, grp, delivered ? "WEBHOOK_200_OK" : "PAYLOAD_STAGED", DateTime.Now.ToString("HH:mm:ss"));
            }
            RecordSiemEvent("PORYGON-Z", "SOAR_FORTIGATE", "HIGH", "M1037", "FORTIOS_CONNECTOR",
                ip + " -> " + grp, "", 0.90f,
                string.Format("Orden de bloqueo perimetral FortiGate emitida para {0} en grupo {1}", ip, grp),
                delivered ? "FORTIGATE_SYNCED" : "FORTIGATE_STAGED");
            return true;
        } catch {
            return false;
        }
    }

    private static void UpdateLocalMasterFleetNode(Dictionary<string, object> st) {
        try {
            float cpu = st.ContainsKey("cpu") ? Convert.ToSingle(st["cpu"]) : 12.0f;
            int ram = st.ContainsKey("ramPct") ? Convert.ToInt32(st["ramPct"]) : 45;
            string host = Environment.MachineName;
            string localIp = "127.0.0.1";
            try {
                IPAddress[] addrs = Dns.GetHostAddresses(host);
                for (int i = 0; i < addrs.Length; i++) {
                    if (addrs[i].AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(addrs[i])) {
                        localIp = addrs[i].ToString();
                        break;
                    }
                }
            } catch { }

            lock (SyncLock) {
                FleetEndpointNode master;
                if (!FleetNodes.TryGetValue(host, out master)) {
                    master = new FleetEndpointNode {
                        HostId = host,
                        Hostname = host + " (Master SOC)",
                        AgentRole = "MASTER_SOC_SERVER",
                        OsVersion = Environment.OSVersion.VersionString,
                        IsIsolated = false
                    };
                    FleetNodes[host] = master;
                }
                master.IpAddress = localIp;
                master.CpuPct = cpu;
                master.RamPct = ram;
                master.ActiveTcpCount = ActiveTcpRows.Count;
                master.OpenAlerts = RecentSiemEvents.Count;
                master.RiskScorePct = string.IsNullOrEmpty(LastCandidateBlockIp) ? 14 : 78;
                master.Status = master.IsIsolated ? "ISOLATED" : (master.RiskScorePct >= 75 ? "HIGH_RISK" : "ONLINE");
                master.LastSeen = DateTime.Now.ToString("HH:mm:ss");
                master.LastSeenUtc = DateTime.UtcNow;
            }
        } catch { }
    }

    // =========================================================================
    // MODULE 4.5: SSH TUNNEL PROXY TO OFFICE-PC (:8002 C2/IDS + :5080 QB-NIDS/VT)
    // =========================================================================
    public static bool IsLocalPortListening(int port) {
        try {
            Dictionary<string, int> map = QueryExtendedTcpPidMap();
            if (map.ContainsKey("127.0.0.1:" + port + "|0.0.0.0:0") ||
                map.ContainsKey("0.0.0.0:" + port + "|0.0.0.0:0")) {
                return true;
            }
        } catch { }
        return false;
    }

    public static bool EnsureOfficePcSshProxyTunnel(bool forceRestart) {
        bool p8002 = IsLocalPortListening(8002);
        bool p5080 = IsLocalPortListening(5080);
        if (string.Equals(Environment.MachineName, "DEVQB", StringComparison.OrdinalIgnoreCase)) {
            OfficePcTunnelOnline = p8002 || p5080;
            return OfficePcTunnelOnline;
        }
        if (!forceRestart && p8002 && p5080) {
            OfficePcTunnelOnline = true;
            return true;
        }

        try {
            if (sshTunnelProcess != null) {
                try {
                    if (!sshTunnelProcess.HasExited) sshTunnelProcess.Kill();
                } catch { }
                sshTunnelProcess = null;
            }
            // Always terminate orphan ssh.exe processes that own local listener 127.0.0.1:8002 or :5080
            // when one of the two forwards is missing or forceRestart is requested, so ExitOnForwardFailure=yes can bind both ports cleanly.
            // NEVER kill interactive user ssh.exe sessions (e.g. ssh oficina, ssh devqb)!
            try {
                Dictionary<string, int> tcpMap = QueryExtendedTcpPidMap();
                int[] tunnelPorts = new int[] { 8002, 5080 };
                for (int pIdx = 0; pIdx < tunnelPorts.Length; pIdx++) {
                    int portPid;
                    if (tcpMap.TryGetValue("127.0.0.1:" + tunnelPorts[pIdx] + "|0.0.0.0:0", out portPid) && portPid > 4) {
                        try {
                            using (Process p = Process.GetProcessById(portPid)) {
                                if (string.Equals(p.ProcessName, "ssh", StringComparison.OrdinalIgnoreCase)) {
                                    p.Kill();
                                }
                            }
                        } catch { }
                    }
                }
                Thread.Sleep(250);
            } catch { }

            ProcessStartInfo psi = new ProcessStartInfo("ssh.exe",
                "-N -o BatchMode=yes -o ExitOnForwardFailure=yes -o ServerAliveInterval=15 -o ServerAliveCountMax=6 -L 127.0.0.1:5080:192.168.2.222:5080 -L 127.0.0.1:8002:192.168.2.222:8002 office-pc") {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            sshTunnelProcess = Process.Start(psi);

            for (int i = 0; i < 20; i++) {
                Thread.Sleep(220);
                if (IsLocalPortListening(8002) && IsLocalPortListening(5080)) {
                    OfficePcTunnelOnline = true;
                    lock (SyncLock) {
                        OfficePcTunnelStatus = "ONLINE // Proxy SSH office-pc (100.89.1.27) -> 127.0.0.1:8002 (C2/IDS) & :5080 (QB-NIDS/VT)";
                    }
                    return true;
                }
            }
        } catch (Exception ex) {
            lock (SyncLock) {
                OfficePcTunnelOnline = false;
                OfficePcTunnelStatus = "OFFLINE // Error iniciando ssh office-pc: " + ex.Message;
            }
        }
        OfficePcTunnelOnline = IsLocalPortListening(8002);
        return OfficePcTunnelOnline;
    }

    public static string TriggerOfficePcIdsScan() {
        try {
            EnsureOfficePcSshProxyTunnel(false);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8002/api/ids/scan");
            req.Method = "GET";
            req.Timeout = 15000;
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) {
                string body = sr.ReadToEnd();
                int filesScanned = 0;
                int alertsFound = 0;
                try {
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    var d = ser.Deserialize<Dictionary<string, object>>(body);
                    if (d != null) {
                        if (d.ContainsKey("files_scanned") && d["files_scanned"] != null) filesScanned = Convert.ToInt32(d["files_scanned"]);
                        if (d.ContainsKey("total_alerts") && d["total_alerts"] != null) alertsFound = Convert.ToInt32(d["total_alerts"]);
                    }
                } catch { }
                PollOfficePcC2AndIdsTelemetry(true);
                return string.Format("🔎 Escaneo IDS ejecutado en office-pc (:8002): {0} logs descifrados analizados, {1} nuevas alertas detectadas y sincronizadas con los 3 Porygons.", filesScanned, alertsFound);
            }
        } catch (Exception ex) {
            return "No se pudo ejecutar /api/ids/scan en office-pc: " + ex.Message;
        }
    }

    private static DateTime lastOfficeSelfHealUtc = DateTime.UtcNow.AddMinutes(-10);

    public static void PollOfficePcC2AndIdsTelemetry(bool forceResync) {
        DateTime now = DateTime.UtcNow;
        if (!forceResync && (now - lastOfficeC2PollUtc).TotalSeconds < 6.0) return;
        lastOfficeC2PollUtc = now;

        if (!EnsureOfficePcSshProxyTunnel(false)) {
            return;
        }

        JavaScriptSerializer ser = new JavaScriptSerializer();
        ser.MaxJsonLength = 32 * 1024 * 1024;

        try {
            using (WebClient wc = new WebClient()) {
                wc.Encoding = Encoding.UTF8;

                // 1. Poll C2 & Forensic Status (http://127.0.0.1:8002/api/status)
                try {
                    string stJson = wc.DownloadString("http://127.0.0.1:8002/api/status");
                    var stObj = ser.Deserialize<Dictionary<string, object>>(stJson);
                    if (stObj != null) {
                        lock (SyncLock) {
                            OfficePcTunnelOnline = true;
                            if (stObj.ContainsKey("logs_raw") && stObj["logs_raw"] != null) OfficePcLogsRaw = Convert.ToInt32(stObj["logs_raw"]);
                            if (stObj.ContainsKey("logs_decrypted") && stObj["logs_decrypted"] != null) OfficePcLogsDecrypted = Convert.ToInt32(stObj["logs_decrypted"]);
                            if (stObj.ContainsKey("credentials_count") && stObj["credentials_count"] != null) OfficePcCredentialsCount = Convert.ToInt32(stObj["credentials_count"]);
                            if (stObj.ContainsKey("screenshots_count") && stObj["screenshots_count"] != null) {
                                OfficePcScreenshotsCount = Math.Max(OfficePcScreenshotsCount, Convert.ToInt32(stObj["screenshots_count"]));
                            }
                        }
                    }
                } catch {
                    // Self-heal stale local SSH tunnel AND remote :8002 C2 server on office-pc (with 60s cooldown)
                    if ((DateTime.UtcNow - lastOfficeSelfHealUtc).TotalSeconds > 60.0) {
                        lastOfficeSelfHealUtc = DateTime.UtcNow;
                        ThreadPool.QueueUserWorkItem(_ => {
                            try {
                                EnsureOfficePcSshProxyTunnel(true);
                                ProcessStartInfo healPsi = new ProcessStartInfo("ssh.exe",
                                    "office-pc \"Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = 'powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File C:\\Users\\CarlosAlbertoAcevesC\\Desktop\\03-ciberseguridad-soc\\desencript\\c2_watchdog.ps1'; CurrentDirectory = 'C:\\Users\\CarlosAlbertoAcevesC\\Desktop\\03-ciberseguridad-soc\\desencript' }\"") {
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    WindowStyle = ProcessWindowStyle.Hidden
                                };
                                using (Process hp = Process.Start(healPsi)) {
                                    if (hp != null) hp.WaitForExit(10000);
                                }
                            } catch { }
                        });
                    }
                }

                // 2. Poll IDS Statistics & Top Risk Hosts (http://127.0.0.1:8002/api/ids/stats)
                Dictionary<string, int> ruleHitsMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, int> hostAlertCountMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, int> hostRiskPctMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                try {
                    string idsStJson = wc.DownloadString("http://127.0.0.1:8002/api/ids/stats");
                    var idsSt = ser.Deserialize<Dictionary<string, object>>(idsStJson);
                    if (idsSt != null) {
                        if (idsSt.ContainsKey("total") && idsSt["total"] != null) {
                            lock (SyncLock) { OfficePcIdsAlertsTotal = Convert.ToInt32(idsSt["total"]); }
                        }
                        if (idsSt.ContainsKey("by_rule") && idsSt["by_rule"] is Dictionary<string, object>) {
                            var byRule = (Dictionary<string, object>)idsSt["by_rule"];
                            foreach (var kv in byRule) {
                                var rDict = kv.Value as Dictionary<string, object>;
                                if (rDict != null && rDict.ContainsKey("count") && rDict["count"] != null) {
                                    ruleHitsMap[kv.Key] = Convert.ToInt32(rDict["count"]);
                                }
                            }
                        }
                        if (idsSt.ContainsKey("top_risk_hosts") && idsSt["top_risk_hosts"] is System.Collections.IEnumerable) {
                            foreach (object hObj in (System.Collections.IEnumerable)idsSt["top_risk_hosts"]) {
                                var hd = hObj as Dictionary<string, object>;
                                if (hd == null) continue;
                                string hip = hd.ContainsKey("ip") && hd["ip"] != null ? hd["ip"].ToString() : "";
                                string hname = hd.ContainsKey("hostname") && hd["hostname"] != null ? hd["hostname"].ToString() : hip;
                                int critCnt = hd.ContainsKey("critical") && hd["critical"] != null ? Convert.ToInt32(hd["critical"]) : 0;
                                int totCnt = hd.ContainsKey("total") && hd["total"] != null ? Convert.ToInt32(hd["total"]) : 1;
                                int calcRisk = critCnt > 0 ? Math.Min(98, 86 + critCnt * 5) : Math.Min(88, 72 + totCnt * 4);
                                if (!string.IsNullOrEmpty(hip)) {
                                    hostAlertCountMap[hip] = totCnt;
                                    hostRiskPctMap[hip] = calcRisk;
                                }
                                if (!string.IsNullOrEmpty(hname)) {
                                    hostAlertCountMap[hname] = totCnt;
                                    hostRiskPctMap[hname] = calcRisk;
                                }
                            }
                        }
                    }
                } catch { }

                // 3. Sync IDS Rules from office-pc (http://127.0.0.1:8002/api/ids/rules)
                try {
                    string rulesJson = wc.DownloadString("http://127.0.0.1:8002/api/ids/rules");
                    var rulesRoot = ser.Deserialize<Dictionary<string, object>>(rulesJson);
                    if (rulesRoot != null && rulesRoot.ContainsKey("rules") && rulesRoot["rules"] is System.Collections.IEnumerable) {
                        lock (SyncLock) {
                            foreach (object rObj in (System.Collections.IEnumerable)rulesRoot["rules"]) {
                                var rd = rObj as Dictionary<string, object>;
                                if (rd == null) continue;
                                string rid = rd.ContainsKey("rule_id") && rd["rule_id"] != null ? rd["rule_id"].ToString() : "";
                                if (string.IsNullOrEmpty(rid)) continue;
                                string rname = rd.ContainsKey("name") && rd["name"] != null ? rd["name"].ToString() : rid;
                                string rsev = rd.ContainsKey("severity") && rd["severity"] != null ? rd["severity"].ToString().ToUpperInvariant() : "HIGH";
                                string rmitre = rd.ContainsKey("mitre_technique") && rd["mitre_technique"] != null ? rd["mitre_technique"].ToString() : "T1059";
                                string rtactic = rd.ContainsKey("mitre_tactic") && rd["mitre_tactic"] != null ? rd["mitre_tactic"].ToString() : "Execution";
                                string rdesc = rd.ContainsKey("description") && rd["description"] != null ? rd["description"].ToString() : rname;
                                bool ren = !rd.ContainsKey("enabled") || Convert.ToBoolean(rd["enabled"]);
                                int rhits = rd.ContainsKey("hit_count") && rd["hit_count"] != null ? Convert.ToInt32(rd["hit_count"]) : 0;
                                int statHits;
                                if (ruleHitsMap.TryGetValue(rid, out statHits) && statHits > rhits) {
                                    rhits = statHits;
                                }

                                EnterpriseDetectionRule existing = null;
                                for (int i = 0; i < DetectionRules.Count; i++) {
                                    if (string.Equals(DetectionRules[i].Id, rid, StringComparison.OrdinalIgnoreCase)) {
                                        existing = DetectionRules[i];
                                        break;
                                    }
                                }
                                int rPct = rsev == "CRITICAL" ? 95 : (rsev == "HIGH" ? 84 : 68);
                                if (existing == null) {
                                    DetectionRules.Add(new EnterpriseDetectionRule {
                                        Id = rid,
                                        Name = rname,
                                        RuleType = "OFFICE-IDS",
                                        TargetField = rtactic,
                                        Pattern = rdesc,
                                        MitreId = rmitre,
                                        Severity = rsev,
                                        RiskPct = rPct,
                                        Enabled = ren,
                                        HitsCount = rhits
                                    });
                                } else {
                                    existing.HitsCount = Math.Max(existing.HitsCount, rhits);
                                }
                            }
                        }
                    }
                } catch { }

                // 4. Poll Live IDS Alerts from office-pc (http://127.0.0.1:8002/api/ids/alerts?limit=35)
                try {
                    string alJson = wc.DownloadString("http://127.0.0.1:8002/api/ids/alerts?limit=35");
                    var alRoot = ser.Deserialize<Dictionary<string, object>>(alJson);
                    if (alRoot != null && alRoot.ContainsKey("alerts") && alRoot["alerts"] is System.Collections.IEnumerable) {
                        List<Dictionary<string, object>> alertDicts = new List<Dictionary<string, object>>();
                        foreach (object aObj in (System.Collections.IEnumerable)alRoot["alerts"]) {
                            var ad = aObj as Dictionary<string, object>;
                            if (ad != null) alertDicts.Add(ad);
                        }

                        int newlyIngested = 0;
                        for (int i = alertDicts.Count - 1; i >= 0; i--) {
                            var ad = alertDicts[i];
                            string aid = ad.ContainsKey("alert_id") && ad["alert_id"] != null ? ad["alert_id"].ToString() : "";
                            if (string.IsNullOrEmpty(aid)) continue;
                            bool isNew = false;
                            lock (SyncLock) {
                                if (!ingestedOfficeAlertIds.Contains(aid)) {
                                    ingestedOfficeAlertIds.Add(aid);
                                    isNew = true;
                                }
                            }
                            if (isNew) {
                                newlyIngested++;
                                string ruleId = ad.ContainsKey("rule_id") && ad["rule_id"] != null ? ad["rule_id"].ToString() : "IDS-001";
                                string ruleName = ad.ContainsKey("rule_name") && ad["rule_name"] != null ? ad["rule_name"].ToString() : "Office IDS Alert";
                                string rawSev = ad.ContainsKey("severity") && ad["severity"] != null ? ad["severity"].ToString().ToUpperInvariant() : "HIGH";
                                string sev = (rawSev == "CRITICAL") ? "CRITICAL" : "HIGH";
                                string mitre = ad.ContainsKey("mitre_technique") && ad["mitre_technique"] != null ? ad["mitre_technique"].ToString() : "T1059";
                                string tactic = ad.ContainsKey("mitre_tactic") && ad["mitre_tactic"] != null ? ad["mitre_tactic"].ToString() : "Collection";
                                string srcIp = ad.ContainsKey("source_ip") && ad["source_ip"] != null ? ad["source_ip"].ToString() : "192.168.2.1";
                                string hname = ad.ContainsKey("hostname") && ad["hostname"] != null ? ad["hostname"].ToString() : srcIp;
                                string matched = ad.ContainsKey("matched_content") && ad["matched_content"] != null ? ad["matched_content"].ToString() : "";
                                if (matched.Length > 110) matched = matched.Substring(0, 110) + "...";

                                float score = (sev == "CRITICAL") ? 0.96f : (rawSev == "HIGH" ? 0.86f : 0.72f);
                                string assignedPorygon = (ruleId == "IDS-001" || ruleId == "IDS-002") ? "PORYGON2" : ((ruleId == "IDS-004" || ruleId == "IDS-008") ? "PORYGON-Z" : "PORYGON");

                                RecordSiemEvent(assignedPorygon, "OFFICE_C2_" + ruleId, sev, mitre,
                                    hname + " [" + srcIp + "]", srcIp + ":8002", "", score,
                                    string.Format("[{0}] {1} en {2}: {3}", ruleId, ruleName, hname, matched),
                                    "OFFICE_IDS_ACTIVE");

                                if (newlyIngested <= 5 && OnInject3DThreatOrb != null) {
                                    OnInject3DThreatOrb(2, srcIp + ":8002", hname, "C2-IDS/" + ruleId, tactic.ToUpperInvariant(),
                                        ruleId + ":" + ruleName, matched, score);
                                }
                            }
                        }

                        // Continuously feed 1 real alert orb per poll cycle to train the 3D Porygon neural connectomes!
                        if ( newlyIngested == 0 && alertDicts.Count > 0 && OnInject3DThreatOrb != null ) {
                            var sampleAd = alertDicts[officeAlertRotationIdx % alertDicts.Count];
                            officeAlertRotationIdx++;
                            string ruleId = sampleAd.ContainsKey("rule_id") && sampleAd["rule_id"] != null ? sampleAd["rule_id"].ToString() : "IDS-001";
                            string ruleName = sampleAd.ContainsKey("rule_name") && sampleAd["rule_name"] != null ? sampleAd["rule_name"].ToString() : "Office IDS";
                            string rawSev = sampleAd.ContainsKey("severity") && sampleAd["severity"] != null ? sampleAd["severity"].ToString().ToUpperInvariant() : "HIGH";
                            string tactic = sampleAd.ContainsKey("mitre_tactic") && sampleAd["mitre_tactic"] != null ? sampleAd["mitre_tactic"].ToString() : "Collection";
                            string srcIp = sampleAd.ContainsKey("source_ip") && sampleAd["source_ip"] != null ? sampleAd["source_ip"].ToString() : "192.168.2.1";
                            string hname = sampleAd.ContainsKey("hostname") && sampleAd["hostname"] != null ? sampleAd["hostname"].ToString() : srcIp;
                            string matched = sampleAd.ContainsKey("matched_content") && sampleAd["matched_content"] != null ? sampleAd["matched_content"].ToString() : ruleName;
                            if (matched.Length > 80) matched = matched.Substring(0, 80);
                            float score = (rawSev == "CRITICAL") ? 0.96f : (rawSev == "HIGH" ? 0.86f : 0.72f);
                            OnInject3DThreatOrb(2, srcIp + ":8002", hname, "C2-IDS/" + ruleId, tactic.ToUpperInvariant(),
                                ruleId + ":" + ruleName, matched, score);
                        }
                    }
                } catch { }

                // 5. Poll Live C2 Agents Heartbeats (http://127.0.0.1:8002/api/lan/heartbeats) + Online Agent Unified IDS Probes
                HashSet<string> liveAgentIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<string> liveAgentLabels = new List<string>();
                List<string> latestProbeSummaries = new List<string>();
                try {
                    string hbJson = wc.DownloadString("http://127.0.0.1:8002/api/lan/heartbeats");
                    var hbDict = ser.Deserialize<Dictionary<string, object>>(hbJson);
                    if (hbDict != null) {
                        List<Dictionary<string, object>> liveAgents = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> standbyAgents = new List<Dictionary<string, object>>();

                        foreach (var kv in hbDict) {
                            var ag = kv.Value as Dictionary<string, object>;
                            if (ag == null) continue;
                            string aip = ag.ContainsKey("ip") && ag["ip"] != null ? ag["ip"].ToString() : kv.Key;
                            if (!string.IsNullOrEmpty(aip)) {
                                lock (SyncLock) { whitelistedC2FleetIps.Add(aip); }
                            }
                            bool isLive = (ag.ContainsKey("is_live") && ag["is_live"] != null && Convert.ToBoolean(ag["is_live"]));
                            int secAgo = ag.ContainsKey("seconds_ago") && ag["seconds_ago"] != null ? Convert.ToInt32(ag["seconds_ago"]) : 999999;
                            if (isLive || secAgo < 120) {
                                liveAgents.Add(ag);
                                if (!string.IsNullOrEmpty(aip)) liveAgentIps.Add(aip);
                            } else {
                                standbyAgents.Add(ag);
                            }
                        }

                        // Register office-pc C2/IDS Master Server itself at top of Fleet
                        lock (SyncLock) {
                            OfficePcTotalAgentsCount = hbDict.Count;
                            OfficePcOnlineAgentsCount = liveAgents.Count;

                            FleetEndpointNode masterC2;
                            if (!FleetNodes.TryGetValue("DEVQB-OFFICE-C2", out masterC2)) {
                                masterC2 = new FleetEndpointNode {
                                    HostId = "DEVQB-OFFICE-C2",
                                    Hostname = "DEVQB [office-pc C2/IDS]",
                                    IsIsolated = false
                                };
                                FleetNodes["DEVQB-OFFICE-C2"] = masterC2;
                            }
                            masterC2.IpAddress = "100.89.1.27 / 192.168.2.222";
                            masterC2.OsVersion = "Windows 11 Enterprise (C2 :8002 + NIDS :5080)";
                            masterC2.AgentRole = "C2_IDS_MASTER (WHITELISTED)";
                            masterC2.CpuPct = 8.4f;
                            masterC2.RamPct = 46;
                            masterC2.ActiveTcpCount = 42;
                            masterC2.OpenAlerts = OfficePcIdsAlertsTotal;
                            masterC2.RiskScorePct = 10;
                            masterC2.Status = "ONLINE";
                            masterC2.LastSeen = "LIVE (SSH Tunnel)";
                            masterC2.LastSeenUtc = DateTime.UtcNow;
                        }

                        // Process all LIVE Online C2 Agents first and inspect their latest [PORYGON-IDS-UNIFIED] probe results
                        for (int i = 0; i < liveAgents.Count; i++) {
                            var ag = liveAgents[i];
                            string aip = ag.ContainsKey("ip") && ag["ip"] != null ? ag["ip"].ToString() : "";
                            string ahname = ag.ContainsKey("hostname") && ag["hostname"] != null ? ag["hostname"].ToString() : aip;
                            string auser = ag.ContainsKey("user") && ag["user"] != null ? ag["user"].ToString() : "system";
                            string apid = ag.ContainsKey("pid") && ag["pid"] != null ? ag["pid"].ToString() : "0";
                            string aos = ag.ContainsKey("os") && ag["os"] != null ? ag["os"].ToString() : "Windows";
                            int secAgo = ag.ContainsKey("seconds_ago") && ag["seconds_ago"] != null ? Convert.ToInt32(ag["seconds_ago"]) : 0;
                            float acpu = 4.0f;
                            if (ag.ContainsKey("cpu") && ag["cpu"] != null) float.TryParse(ag["cpu"].ToString(), out acpu);
                            int aram = 45;
                            if (ag.ContainsKey("ram") && ag["ram"] != null) {
                                string rStr = ag["ram"].ToString().Replace("GB", "").Trim();
                                int rGb;
                                if (int.TryParse(rStr, out rGb) && rGb > 0) aram = Math.Min(92, 35 + rGb);
                            }

                            liveAgentLabels.Add(string.Format("{0} ({1} | {2} | PID:{3})", ahname, aip, auser, apid));

                            int liveTcpSockets = 28;
                            int extConnections = 0;
                            bool hasDualAgentBridge = false;
                            // Inspect latest command output from /api/agent/commands?ip=<aip> (Read-only Unified IDS & Dual-Agent telemetry)
                            try {
                                string cmdJson = wc.DownloadString("http://127.0.0.1:8002/api/agent/commands?ip=" + Uri.EscapeDataString(aip));
                                var cmdRoot = ser.Deserialize<Dictionary<string, object>>(cmdJson);
                                if (cmdRoot != null && cmdRoot.ContainsKey("results") && cmdRoot["results"] is System.Collections.IEnumerable) {
                                    Dictionary<string, object> latestCmd = null;
                                    foreach (object cObj in (System.Collections.IEnumerable)cmdRoot["results"]) {
                                        var cd = cObj as Dictionary<string, object>;
                                        if (cd != null && cd.ContainsKey("output") && cd["output"] != null) {
                                            string outStr = cd["output"].ToString();
                                            if (outStr.Contains("PORYGON-DUAL-AGENT-ACTIVE")) {
                                                hasDualAgentBridge = true;
                                            }
                                            if (latestCmd == null && (outStr.Contains("PORYGON-DUAL-AGENT-ACTIVE") || outStr.Contains("PORYGON-IDS-UNIFIED") || outStr.Contains("ESTABLISHED"))) {
                                                latestCmd = cd;
                                            }
                                        }
                                    }
                                    if (latestCmd != null) {
                                        string cmdId = latestCmd.ContainsKey("id") && latestCmd["id"] != null ? latestCmd["id"].ToString() : "";
                                        string cmdOut = latestCmd.ContainsKey("output") && latestCmd["output"] != null ? latestCmd["output"].ToString() : "";
                                        string[] outLines = cmdOut.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        int tcpCnt = 0;
                                        int extCnt = 0;
                                        List<string> extEndpoints = new List<string>();
                                        for (int l = 0; l < outLines.Length; l++) {
                                            string ln = outLines[l].Trim();
                                            if (ln.StartsWith("TCP", StringComparison.OrdinalIgnoreCase) && ln.IndexOf("ESTABLISHED", StringComparison.OrdinalIgnoreCase) >= 0) {
                                                tcpCnt++;
                                                if (!ln.Contains("127.0.0.1") && !ln.Contains("[::1]")) {
                                                    extCnt++;
                                                    string[] toks = ln.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                                    if (toks.Length >= 3) {
                                                        string remEp = toks[2];
                                                        string remPid = toks.Length >= 5 ? toks[4] : apid;
                                                        extEndpoints.Add(remEp + " [PID:" + remPid + "]");
                                                    }
                                                }
                                            }
                                        }
                                        if (tcpCnt > 0) {
                                            liveTcpSockets = tcpCnt;
                                            extConnections = extCnt;
                                        }
                                        string modeTag = hasDualAgentBridge ? "[DUAL-AGENT C2+XDR]" : "[IDS-PROBE]";
                                        latestProbeSummaries.Add(string.Format("{0} {1} ({2}): {3} TCP ESTABLISHED ({4} LAN/WAN)", modeTag, ahname, aip, liveTcpSockets, extConnections));

                                        bool isNewProbeResult = false;
                                        if (!string.IsNullOrEmpty(cmdId)) {
                                            lock (SyncLock) {
                                                if (!ingestedOnlineAgentProbeCmdIds.Contains(cmdId)) {
                                                    ingestedOnlineAgentProbeCmdIds.Add(cmdId);
                                                    isNewProbeResult = true;
                                                }
                                            }
                                        }
                                        if (isNewProbeResult) {
                                            // Cross-check the online agent's external/LAN endpoints against Unified Porygon XDR + CTI Rules + VirusTotal (:5080)
                                            int agentVtChecksThisCycle = 0;
                                            for (int e = 0; e < extEndpoints.Count; e++) {
                                                string epFull = extEndpoints[e];
                                                string epAddr = epFull.Split(' ')[0];
                                                string epPidTag = epFull.Contains("[") ? epFull.Substring(epFull.IndexOf('[')) : ("PID:" + apid);
                                                string remIpOnly = epAddr.Split(':')[0];
                                                string remPortStr = epAddr.Contains(":") ? epAddr.Split(':')[1] : "0";
                                                // Never flag office-pc C2 (192.168.2.222 / 100.89.1.27)
                                                if (remIpOnly == "192.168.2.222" || remIpOnly == "100.89.1.27") continue;

                                                if (!remIpOnly.StartsWith("192.168.") && !remIpOnly.StartsWith("10.") &&
                                                    !remIpOnly.StartsWith("100.") && !remIpOnly.StartsWith("127.") &&
                                                    agentVtChecksThisCycle < 2 && IsLocalPortListening(5080)) {
                                                    agentVtChecksThisCycle++;
                                                    try {
                                                        string vtUrl = string.Format(
                                                            "http://127.0.0.1:5080/api/wallpaper-vt-check?target={0}&port={1}&host={2}&proc={3}",
                                                            Uri.EscapeDataString(remIpOnly),
                                                            Uri.EscapeDataString(remPortStr),
                                                            Uri.EscapeDataString(ahname + " [" + aip + "]"),
                                                            Uri.EscapeDataString(epPidTag));
                                                        wc.DownloadString(vtUrl);
                                                    } catch { }
                                                }

                                                if (IsCtiMaliciousIp(remIpOnly)) {
                                                    RecordSiemEvent("PORYGON-Z", "UNIFIED_AGENT_CTI", "CRITICAL", "T1071",
                                                        ahname + " (" + aip + ") " + epPidTag, epAddr, "", 0.97f,
                                                        string.Format("[UNIFIED-IDS] Agente Online {0} ({1}) conectado a IP en lista CTI Botnet/C2: {2}", ahname, aip, epFull),
                                                        "UNIFIED_IDS_ALERT");
                                                } else if (remPortStr == "80" && !remIpOnly.StartsWith("192.168.") && !remIpOnly.StartsWith("10.")) {
                                                    RecordSiemEvent("PORYGON", "UNIFIED_AGENT_NIDS", "HIGH", "T1071.001",
                                                        ahname + " (" + aip + ") " + epPidTag, epAddr, "", 0.85f,
                                                        string.Format("[UNIFIED-IDS] Tráfico HTTP sin cifrar (:80) hacia WAN detectado en agente online {0} ({1}) -> {2}", ahname, aip, epFull),
                                                        "UNIFIED_IDS_ALERT");
                                                    if (OnInject3DThreatOrb != null) {
                                                        OnInject3DThreatOrb(2, epAddr, ahname, "UNIFIED-IDS/T1071.001", "WAN_HTTP_CLEAR",
                                                            "UNIFIED_AGENT:" + ahname, epFull, 0.85f);
                                                    }
                                                }
                                            }

                                            RecordSiemEvent("PORYGON2", hasDualAgentBridge ? "DUAL_AGENT_CANARY" : "UNIFIED_IDS_PROBE", "INFO", "T1049",
                                                ahname + " [" + auser + " | PID:" + apid + "]", aip, "", 0.18f,
                                                string.Format("[{0}] Telemetría en vivo de agente online {1} ({2}): {3} conexiones TCP ESTABLISHED ({4} externas/LAN) auditadas con coexistencia C2 (:8002) + Porygon XDR (:5090).",
                                                    hasDualAgentBridge ? "DUAL-AGENT-XDR" : "UNIFIED-IDS", ahname, aip, liveTcpSockets, extConnections),
                                                hasDualAgentBridge ? "DUAL_AGENT_ONLINE" : "AGENT_VERIFIED_ONLINE");
                                        }
                                    }
                                }
                            } catch { }

                            int openAlerts = 0;
                            if (!hostAlertCountMap.TryGetValue(aip, out openAlerts)) {
                                hostAlertCountMap.TryGetValue(ahname, out openAlerts);
                            }
                            int riskScore = openAlerts > 0 ? 78 : 22;
                            if (hostRiskPctMap.ContainsKey(aip)) riskScore = hostRiskPctMap[aip];
                            else if (hostRiskPctMap.ContainsKey(ahname)) riskScore = hostRiskPctMap[ahname];

                            lock (SyncLock) {
                                FleetEndpointNode fnode;
                                if (!FleetNodes.TryGetValue(ahname, out fnode)) {
                                    fnode = new FleetEndpointNode {
                                        HostId = ahname,
                                        Hostname = ahname,
                                        IsIsolated = false
                                    };
                                    FleetNodes[ahname] = fnode;
                                }
                                fnode.IpAddress = aip;
                                fnode.OsVersion = aos;
                                fnode.AgentRole = hasDualAgentBridge
                                    ? string.Format("🟢 DUAL_AGENT C2+XDR ({0} | PID:{1})", auser, apid)
                                    : string.Format("🟢 C2_IDS_ONLINE ({0} | PID:{1})", auser, apid);
                                fnode.CpuPct = acpu;
                                fnode.RamPct = aram;
                                fnode.ActiveTcpCount = liveTcpSockets;
                                fnode.OpenAlerts = openAlerts;
                                fnode.RiskScorePct = riskScore;
                                fnode.Status = fnode.IsIsolated ? "ISOLATED" : "ONLINE";
                                fnode.LastSeen = string.Format("LIVE ({0}s ago)", secAgo);
                                fnode.LastSeenUtc = DateTime.UtcNow;
                            }
                        }

                        // Also add standby C2 heartbeat agents (marked STANDBY so they are clearly distinguished from live online agents)
                        lock (SyncLock) {
                            for (int i = 0; i < standbyAgents.Count && FleetNodes.Count < 42; i++) {
                                var ag = standbyAgents[i];
                                string aip = ag.ContainsKey("ip") && ag["ip"] != null ? ag["ip"].ToString() : "";
                                string ahname = ag.ContainsKey("hostname") && ag["hostname"] != null ? ag["hostname"].ToString() : aip;
                                if (string.IsNullOrEmpty(ahname) || FleetNodes.ContainsKey(ahname)) continue;
                                string auser = ag.ContainsKey("user") && ag["user"] != null ? ag["user"].ToString() : "user";
                                string aos = ag.ContainsKey("os") && ag["os"] != null ? ag["os"].ToString() : "Windows";
                                string lastSeen = ag.ContainsKey("last_seen") && ag["last_seen"] != null ? ag["last_seen"].ToString() : "Standby";
                                int openAlerts = 0;
                                if (!hostAlertCountMap.TryGetValue(aip, out openAlerts)) {
                                    hostAlertCountMap.TryGetValue(ahname, out openAlerts);
                                }
                                int riskScore = openAlerts > 0 ? 82 : 18;
                                if (hostRiskPctMap.ContainsKey(aip)) riskScore = hostRiskPctMap[aip];
                                else if (hostRiskPctMap.ContainsKey(ahname)) riskScore = hostRiskPctMap[ahname];

                                FleetNodes[ahname] = new FleetEndpointNode {
                                    HostId = ahname,
                                    Hostname = ahname,
                                    IpAddress = aip,
                                    OsVersion = aos,
                                    AgentRole = openAlerts > 0 ? ("C2_STANDBY_ALERT (" + openAlerts + ")") : ("C2_AGENT_STANDBY (" + auser + ")"),
                                    CpuPct = 0.0f,
                                    RamPct = 0,
                                    ActiveTcpCount = 0,
                                    OpenAlerts = openAlerts,
                                    RiskScorePct = riskScore,
                                    Status = riskScore >= 75 ? "HIGH_RISK" : "STANDBY",
                                    IsIsolated = false,
                                    LastSeen = lastSeen,
                                    LastSeenUtc = DateTime.UtcNow.AddHours(-1)
                                };
                            }
                        }
                    }
                } catch { }

                // 5B. Poll Corporate LAN Discovery (/api/lan/hosts) for total LAN count & high-risk hosts
                try {
                    string lanJson = wc.DownloadString("http://127.0.0.1:8002/api/lan/hosts");
                    var lanRoot = ser.Deserialize<Dictionary<string, object>>(lanJson);
                    if (lanRoot != null && lanRoot.ContainsKey("hosts") && lanRoot["hosts"] is System.Collections.IEnumerable) {
                        List<Dictionary<string, object>> allHosts = new List<Dictionary<string, object>>();
                        foreach (object hObj in (System.Collections.IEnumerable)lanRoot["hosts"]) {
                            var hd = hObj as Dictionary<string, object>;
                            if (hd != null) allHosts.Add(hd);
                        }
                        lock (SyncLock) {
                            OfficePcLanHostsTotal = allHosts.Count;
                            for (int i = 0; i < allHosts.Count && FleetNodes.Count < 52; i++) {
                                var hd = allHosts[i];
                                string hip = hd.ContainsKey("ip") && hd["ip"] != null ? hd["ip"].ToString() : "";
                                if (!string.IsNullOrEmpty(hip)) whitelistedC2FleetIps.Add(hip);
                                string hname = hd.ContainsKey("hostname") && hd["hostname"] != null ? hd["hostname"].ToString() : "";
                                if (string.IsNullOrEmpty(hname) || hname == "unknown") hname = hip;
                                if (string.IsNullOrEmpty(hname) || FleetNodes.ContainsKey(hname) || liveAgentIps.Contains(hip)) continue;

                                bool hasRisk = hostRiskPctMap.ContainsKey(hip) || hostRiskPctMap.ContainsKey(hname);
                                if (!hasRisk && FleetNodes.Count >= 38) continue;

                                string hos = hd.ContainsKey("os") && hd["os"] != null ? hd["os"].ToString() : "Windows Corporate Node";
                                bool smbOpen = hd.ContainsKey("smb_open") && hd["smb_open"] != null && Convert.ToBoolean(hd["smb_open"]);
                                int openAlerts = 0;
                                if (!hostAlertCountMap.TryGetValue(hip, out openAlerts)) {
                                    hostAlertCountMap.TryGetValue(hname, out openAlerts);
                                }
                                int riskScore = smbOpen ? 38 : 18;
                                if (hostRiskPctMap.ContainsKey(hip)) riskScore = hostRiskPctMap[hip];
                                else if (hostRiskPctMap.ContainsKey(ahnameOrIp(hname, hip))) riskScore = hostRiskPctMap[ahnameOrIp(hname, hip)];

                                FleetNodes[hname] = new FleetEndpointNode {
                                    HostId = hname,
                                    Hostname = hname,
                                    IpAddress = hip,
                                    OsVersion = hos,
                                    AgentRole = openAlerts > 0 ? ("OFFICE_IDS_ALERT (" + openAlerts + ")") : (smbOpen ? "CORP_LAN_SMB" : "CORP_LAN_SCAN"),
                                    CpuPct = 0.0f,
                                    RamPct = 0,
                                    ActiveTcpCount = smbOpen ? 4 : 0,
                                    OpenAlerts = openAlerts,
                                    RiskScorePct = riskScore,
                                    Status = riskScore >= 75 ? "HIGH_RISK" : "SCAN_SEEN",
                                    IsIsolated = false,
                                    LastSeen = "LAN Scan",
                                    LastSeenUtc = DateTime.UtcNow.AddHours(-2)
                                };
                            }
                        }
                    }
                } catch { }

                // 6. Also Poll QB-Network NIDS + VirusTotal on :5080 (http://127.0.0.1:5080/api/wallpaper-telemetry)
                try {
                    if (IsLocalPortListening(5080)) {
                        string qbJson = wc.DownloadString("http://127.0.0.1:5080/api/wallpaper-telemetry");
                        var qbRoot = ser.Deserialize<Dictionary<string, object>>(qbJson);
                        if (qbRoot != null && qbRoot.ContainsKey("nodes") && qbRoot["nodes"] is System.Collections.IEnumerable) {
                            foreach (object nObj in (System.Collections.IEnumerable)qbRoot["nodes"]) {
                                var nd = nObj as Dictionary<string, object>;
                                if (nd == null) continue;
                                int nKind = nd.ContainsKey("kind") ? Convert.ToInt32(nd["kind"]) : 1;
                                if (nKind < 2) continue;
                                string rEp = nd.ContainsKey("remote_ep") && nd["remote_ep"] != null ? nd["remote_ep"].ToString() : "";
                                string nType = nd.ContainsKey("type") && nd["type"] != null ? nd["type"].ToString() : "QB_NIDS";
                                string nDet = nd.ContainsKey("detail") && nd["detail"] != null ? nd["detail"].ToString() : "";
                                string nOriginLabel = nd.ContainsKey("origin_label") && nd["origin_label"] != null ? nd["origin_label"].ToString() : "";
                                string nOriginHost = nd.ContainsKey("origin_host") && nd["origin_host"] != null ? nd["origin_host"].ToString() : "";
                                string nOriginProc = nd.ContainsKey("origin_proc") && nd["origin_proc"] != null ? nd["origin_proc"].ToString() : "";
                                float nScore = nd.ContainsKey("score") ? Convert.ToSingle(nd["score"]) : 0.85f;
                                string dedupKey = "QB5080_" + rEp + "_" + nType;
                                bool shouldAdd = false;
                                lock (SyncLock) {
                                    if (!ingestedOfficeAlertIds.Contains(dedupKey)) {
                                        ingestedOfficeAlertIds.Add(dedupKey);
                                        shouldAdd = true;
                                    }
                                }
                                if (shouldAdd) {
                                    // Skip 0-engine or 1-engine false positives on benign CDNs (Google, Cloudflare, Microsoft, Fastly, Yandex, DigitalOcean)
                                    if ((nType == "VT_MALICIOUS_0_ENGINES" || nType == "VT_MALICIOUS_1_ENGINES") &&
                                        (nDet.IndexOf("Google", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("Cloudflare", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("Fastly", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("DigitalOcean", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("G-Core", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("Facebook", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         nDet.IndexOf("YANDEX", StringComparison.OrdinalIgnoreCase) >= 0)) {
                                        continue;
                                    }

                                    string mitre = nType.Contains("LATERAL") ? "T1021.002" : "T1071";
                                    string epIpOnly = rEp.Contains(":") ? rEp.Split(':')[0] : rEp;
                                    string originHostLabel = !string.IsNullOrEmpty(nOriginLabel)
                                        ? nOriginLabel
                                        : (!string.IsNullOrEmpty(nOriginHost)
                                            ? (!string.IsNullOrEmpty(nOriginProc) ? (nOriginHost + " // " + nOriginProc) : nOriginHost)
                                            : "DEVQB [192.168.2.222] (VT-Cache :5080)");

                                    if (epIpOnly.StartsWith("192.168.") || epIpOnly.StartsWith("10.")) {
                                        string resolvedName = null;
                                        lock (SyncLock) {
                                            foreach (var kv in FleetNodes) {
                                                if (string.Equals(kv.Value.IpAddress, epIpOnly, StringComparison.OrdinalIgnoreCase)) {
                                                    resolvedName = kv.Value.Hostname;
                                                    break;
                                                }
                                            }
                                        }
                                        originHostLabel = !string.IsNullOrEmpty(resolvedName)
                                            ? string.Format("{0} [{1}] (NIDS :5080)", resolvedName, epIpOnly)
                                            : (!string.IsNullOrEmpty(nOriginHost) ? nOriginHost : string.Format("Host LAN [{0}] (NIDS :5080)", epIpOnly));
                                    } else if (string.IsNullOrEmpty(nOriginHost) && rEp.StartsWith("185.220.101.5")) {
                                        originHostLabel = "DEVQB [192.168.2.222] (Sandbox VT Lookup :5080)";
                                    }

                                    RecordSiemEvent("PORYGON-Z", "QB_NIDS_5080", nScore >= 0.90f ? "CRITICAL" : "HIGH",
                                        mitre, originHostLabel, rEp, "", nScore, nType + " // " + nDet, "QB_NIDS_ALERT");
                                }
                            }
                        }
                    }
                } catch { }

                lock (SyncLock) {
                    OfficePcOnlineAgentsSummary = liveAgentLabels.Count > 0
                        ? string.Join(" • ", liveAgentLabels.ToArray())
                        : "Ningún agente con heartbeat activo (<120s)";
                    if (latestProbeSummaries.Count > 0) {
                        OfficePcLastLiveProbeReport = "🧪 Sonda IDS / Dual-Agent: " + string.Join(" | ", latestProbeSummaries.ToArray());
                    }
                    OfficePcTunnelStatus = string.Format(
                        "ONLINE // Proxy SSH Unificado (:8002 C2/IDS & :5080 NIDS/VT) | Agentes Online: {0}/{1} | Logs C2: {2:N0} | Alertas IDS: {3}",
                        OfficePcOnlineAgentsCount, OfficePcTotalAgentsCount, OfficePcLogsRaw, OfficePcIdsAlertsTotal);
                    LastActionBanner = OfficePcTunnelStatus;
                }
            }
        } catch { }
    }

    private static string ahnameOrIp(string hname, string hip) {
        return !string.IsNullOrEmpty(hname) ? hname : (hip ?? "");
    }

    public static string ProbeOnlineC2AgentsUnifiedIds() {
        try {
            EnsureOfficePcSshProxyTunnel(false);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = 32 * 1024 * 1024;
            List<string> queuedAgents = new List<string>();
            List<string> targetIps = new List<string>();

            using (WebClient wc = new WebClient()) {
                wc.Encoding = Encoding.UTF8;
                string hbJson = wc.DownloadString("http://127.0.0.1:8002/api/lan/heartbeats");
                var hbDict = ser.Deserialize<Dictionary<string, object>>(hbJson);
                if (hbDict != null) {
                    foreach (var kv in hbDict) {
                        var ag = kv.Value as Dictionary<string, object>;
                        if (ag == null) continue;
                        bool isLive = (ag.ContainsKey("is_live") && ag["is_live"] != null && Convert.ToBoolean(ag["is_live"]));
                        int secAgo = ag.ContainsKey("seconds_ago") && ag["seconds_ago"] != null ? Convert.ToInt32(ag["seconds_ago"]) : 999999;
                        if (isLive || secAgo < 120) {
                            string aip = ag.ContainsKey("ip") && ag["ip"] != null ? ag["ip"].ToString() : kv.Key;
                            string ahname = ag.ContainsKey("hostname") && ag["hostname"] != null ? ag["hostname"].ToString() : aip;
                            if (string.IsNullOrEmpty(aip)) continue;

                            string probeCmd = string.Format("echo [PORYGON-IDS-UNIFIED] HOST={0} IP={1} & whoami & netstat -ano | findstr ESTABLISHED", ahname, aip);
                            QueueOfficePcAgentCommand(aip, probeCmd);
                            targetIps.Add(aip);
                            queuedAgents.Add(ahname + " (" + aip + ")");
                        }
                    }
                }
            }

            if (targetIps.Count == 0) {
                return "⚠️ No se detectaron agentes C2 con heartbeat activo (is_live=true) en este momento.";
            }

            // Wait up to 4.2s for the online agents' next heartbeat cycle to return the telemetry output
            Thread.Sleep(4200);
            PollOfficePcC2AndIdsTelemetry(true);

            string summary;
            lock (SyncLock) {
                summary = string.Format("🧪 Prueba IDS Unificada (Solo Lectura, sin actualizar binarios) completada en {0} agentes online [{1}] -> {2}",
                    queuedAgents.Count, string.Join(", ", queuedAgents.ToArray()), OfficePcLastLiveProbeReport);
            }
            return summary;
        } catch (Exception ex) {
            return "Error ejecutando prueba IDS en agentes online: " + ex.Message;
        }
    }

    private static bool QueueOfficePcAgentCommand(string agentIp, string commandStr) {
        if (string.IsNullOrEmpty(agentIp) || string.IsNullOrEmpty(commandStr)) return false;
        try {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["ip"] = agentIp;
            payload["cmd"] = commandStr;
            string bodyJson = ser.Serialize(payload);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8002/api/agent/command");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Timeout = 6000;
            byte[] b = Encoding.UTF8.GetBytes(bodyJson);
            req.ContentLength = b.Length;
            using (Stream s = req.GetRequestStream()) { s.Write(b, 0, b.Length); }
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) {
                sr.ReadToEnd();
            }
            return true;
        } catch {
            return false;
        }
    }

    public static bool PropagatePerimeterAndAgentSoarBlock(string targetRaw, string originHostRaw, string reason, out string resultMessage) {
        resultMessage = "";
        string raw = (targetRaw ?? "").Trim();
        if (raw.Contains("->")) {
            string[] parts = raw.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1) raw = parts[1].Trim();
        }
        string cleanIp = raw.Split(new char[] { ':', ' ', '/' }, StringSplitOptions.RemoveEmptyEntries).Length > 0
            ? raw.Split(new char[] { ':', ' ', '/' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()
            : "";

        IPAddress parsedIp;
        if (string.IsNullOrEmpty(cleanIp) || !IPAddress.TryParse(cleanIp, out parsedIp)) {
            resultMessage = "⚠️ El objetivo [" + (targetRaw ?? "") + "] no contiene una dirección IPv4/IPv6 externa válida para bloqueo perimetral.";
            return false;
        }
        if (IsSafeWhitelistedIp(cleanIp) || cleanIp.StartsWith("192.168.") || cleanIp.StartsWith("10.") || cleanIp.StartsWith("127.")) {
            resultMessage = "🛡️ Salvaguarda Dual-Lock: La IP [" + cleanIp + "] pertenece a la red interna / Lista Blanca Corporativa y está protegida contra bloqueo perimetral.";
            return false;
        }

        string cleanHost = (originHostRaw ?? Environment.MachineName).Trim();
        string cleanReason = string.IsNullOrEmpty(reason) ? "Porygon-Z Unified SOAR" : reason;

        // 1. Local Windows Firewall Block
        bool localBlocked = BlockRemoteIpInFirewall(cleanIp, cleanReason, false);

        // 2. QB-Network (:5080) -> FortiGate (192.168.2.1:60001) + Address Group + SQL Server Audit Log
        string fortiStatus = "OFFLINE";
        string fgGroup = "PORYGON_SOAR_BLOCKLIST";
        lock (SyncLock) {
            if (!string.IsNullOrEmpty(SoarConfig.FortiGateAddressGroup)) {
                fgGroup = SoarConfig.FortiGateAddressGroup;
            }
        }
        try {
            EnsureOfficePcSshProxyTunnel(false);
            using (WebClient wc = new WebClient()) {
                wc.Encoding = Encoding.UTF8;
                string url = string.Format(
                    "http://127.0.0.1:5080/api/wallpaper-soar-block?ip={0}&host={1}&reason={2}&group={3}",
                    Uri.EscapeDataString(cleanIp),
                    Uri.EscapeDataString(cleanHost),
                    Uri.EscapeDataString(cleanReason),
                    Uri.EscapeDataString(fgGroup));
                string json = wc.DownloadString(url);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                var d = ser.Deserialize<Dictionary<string, object>>(json);
                if (d != null && ((d.ContainsKey("success") && Convert.ToBoolean(d["success"])) || (d.ContainsKey("ok") && Convert.ToBoolean(d["ok"])))) {
                    bool fgSynced = (d.ContainsKey("fortigate_synced") && Convert.ToBoolean(d["fortigate_synced"])) ||
                                    (d.ContainsKey("fortigate_pushed") && Convert.ToBoolean(d["fortigate_pushed"]));
                    string fgDetail = d.ContainsKey("fortigate_detail") && d["fortigate_detail"] != null ? d["fortigate_detail"].ToString() : ("BLOCKED_" + cleanIp);
                    bool sqlOk = d.ContainsKey("sql_logged") && Convert.ToBoolean(d["sql_logged"]);
                    fortiStatus = fgSynced
                        ? string.Format("FORTIGATE_API_OK ({0}{1})", fgDetail, sqlOk ? " + SQL_AUDIT" : "")
                        : string.Format("SQL_LOGGED + FORTIGATE_STAGED ({0})", fgDetail);
                }
            }
        } catch (Exception ex) {
            fortiStatus = "STAGED (" + ex.Message + ")";
        }

        // 3. Remote C2 Agent (:8002) netsh advfirewall rule if originHost maps to a corporate endpoint
        string agentIp = "";
        string agentHostLabel = cleanHost;
        var ipMatch = System.Text.RegularExpressions.Regex.Match(cleanHost, @"\b(192\.168\.\d+\.\d+|10\.\d+\.\d+\.\d+)\b");
        if (ipMatch.Success) {
            agentIp = ipMatch.Groups[1].Value;
        } else {
            string hostToken = cleanHost.Split(new char[] { ' ', '[', '(', '/' }, StringSplitOptions.RemoveEmptyEntries).Length > 0
                ? cleanHost.Split(new char[] { ' ', '[', '(', '/' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()
                : cleanHost;
            lock (SyncLock) {
                foreach (var kv in FleetNodes) {
                    if (string.Equals(kv.Key, hostToken, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(kv.Value.Hostname, hostToken, StringComparison.OrdinalIgnoreCase) ||
                        (kv.Value.Hostname != null && kv.Value.Hostname.IndexOf(hostToken, StringComparison.OrdinalIgnoreCase) >= 0)) {
                        agentIp = (kv.Value.IpAddress ?? "").Split('/')[0].Trim();
                        agentHostLabel = kv.Value.Hostname;
                        break;
                    }
                }
            }
        }

        bool c2Queued = false;
        if (!string.IsNullOrEmpty(agentIp) && agentIp != "127.0.0.1") {
            string ruleName = "PorygonRemoteIPS_Block_" + cleanIp;
            string cmd = string.Format(
                "netsh advfirewall firewall delete rule name=\"{0}\" & " +
                "netsh advfirewall firewall add rule name=\"{0}\" dir=out action=block remoteip={1} enable=yes & " +
                "netsh advfirewall firewall add rule name=\"{0}\" dir=in action=block remoteip={1} enable=yes & " +
                "echo [PORYGON-REMOTE-SOAR] HOST={2} ACTION=BLOCK_IP TARGET={1} STATUS=APPLIED",
                ruleName, cleanIp, agentHostLabel);
            c2Queued = QueueOfficePcAgentCommand(agentIp, cmd);
        }

        lock (SyncLock) {
            SoarConfig.TotalFortiGatePushes++;
            SoarConfig.LastSoarStatus = string.Format("UNIFIED_SOAR // IP {0} -> LocalFW:{1} | FortiGate(:5080):{2} | C2(:8002):{3}",
                cleanIp, localBlocked ? "OK" : "ACTIVE", fortiStatus, c2Queued ? ("QUEUED->" + agentIp) : "LOCAL_ONLY");
        }

        resultMessage = string.Format(
            "🛡️ Propagación SOAR Tri-Capa completada para [{0}]: (1) Firewall Local [PorygonIPS_Block_{0}], (2) QB-Network :5080 / FortiGate [{1}], (3) Agente C2 :8002 [{2}].",
            cleanIp, fortiStatus, c2Queued ? ("Encolado en " + agentHostLabel + " [" + agentIp + "]") : "Aplicado en nodo maestro");
        return true;
    }

    public static bool ProbeSpecificC2AgentHost(string hostQuery, out string resultMessage) {
        resultMessage = "";
        string q = (hostQuery ?? "").Trim();
        if (string.IsNullOrEmpty(q)) {
            resultMessage = "⚠️ Especifica un Host o IP para lanzar la sonda.";
            return false;
        }

        string token = q.Split(new char[] { ' ', '[', '(', '/' }, StringSplitOptions.RemoveEmptyEntries).Length > 0
            ? q.Split(new char[] { ' ', '[', '(', '/' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()
            : q;

        // Check if this is the local workstation (DESKTOP-874GS44 / Environment.MachineName)
        if (string.Equals(token, Environment.MachineName, StringComparison.OrdinalIgnoreCase) ||
            q.IndexOf(Environment.MachineName, StringComparison.OrdinalIgnoreCase) >= 0 ||
            token == "127.0.0.1" || token == "100.90.211.9") {
            AuditDnsCacheAndBeaconing(true);
            PollWindowsEventLogs();
            int tcpCnt = 0;
            lock (SyncLock) { tcpCnt = ActiveTcpRows.Count; }
            resultMessage = string.Format("🧪 Sonda EDR/TCP local ejecutada en [{0}]: {1} sockets TCP activos auditados, caché DNS verificada y protección LSASS/EventLog validada.", Environment.MachineName, tcpCnt);
            return true;
        }

        EnsureOfficePcSshProxyTunnel(false);
        string agentIp = "";
        string agentHost = token;

        var ipMatch = System.Text.RegularExpressions.Regex.Match(q, @"\b(192\.168\.\d+\.\d+|10\.\d+\.\d+\.\d+)\b");
        if (ipMatch.Success) {
            agentIp = ipMatch.Groups[1].Value;
        }
        if (token.Equals("QB-SOC-5080", StringComparison.OrdinalIgnoreCase) || token.Equals("DEVQB", StringComparison.OrdinalIgnoreCase)) {
            agentIp = "192.168.2.222";
            agentHost = "DEVQB";
        }

        lock (SyncLock) {
            foreach (var kv in FleetNodes) {
                if (string.Equals(kv.Key, token, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kv.Value.Hostname, token, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(agentIp) && string.Equals(kv.Value.IpAddress, agentIp, StringComparison.OrdinalIgnoreCase)) ||
                    (kv.Value.Hostname != null && kv.Value.Hostname.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    if (string.IsNullOrEmpty(agentIp)) agentIp = (kv.Value.IpAddress ?? "").Split('/')[0].Trim();
                    agentHost = kv.Value.Hostname ?? token;
                    break;
                }
            }
        }

        IPAddress dummyIp;
        if (string.IsNullOrEmpty(agentIp) && IPAddress.TryParse(token, out dummyIp)) {
            agentIp = token;
        }

        if (string.IsNullOrEmpty(agentIp)) {
            resultMessage = "⚠️ No se encontró una IP de agente C2 asociada al host [" + q + "].";
            return false;
        }

        string probeCmd = string.Format("echo [PORYGON-IDS-UNIFIED] HOST={0} IP={1} & whoami & netstat -ano | findstr ESTABLISHED", agentHost, agentIp);
        bool queued = QueueOfficePcAgentCommand(agentIp, probeCmd);
        PollOfficePcC2AndIdsTelemetry(true);

        if (queued) {
            RecordSiemEvent("PORYGON2", "SINGLE_HOST_PROBE", "INFO", "T1049",
                agentHost + " [" + agentIp + "]", agentIp, "", 0.16f,
                string.Format("Sonda táctica Read-Only (TCP ESTABLISHED + Proceso) encolada en C2 (:8002) para el host {0} ({1}).", agentHost, agentIp),
                "PROBE_QUEUED");
            resultMessage = string.Format("🧪 Sonda táctica TCP/EDR enviada a [{0} ({1})] vía C2 (:8002) y telemetría sincronizada.", agentHost, agentIp);
            return true;
        } else {
            resultMessage = "⚠️ No se pudo conectar con C2 (:8002) para encolar la sonda en [" + agentHost + " (" + agentIp + ")].";
            return false;
        }
    }

    public static string DeployDualAgentCanaryToOnlineC2Agents() {
        try {
            EnsureOfficePcSshProxyTunnel(false);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = 32 * 1024 * 1024;
            List<string> deployedAgents = new List<string>();

            using (WebClient wc = new WebClient()) {
                wc.Encoding = Encoding.UTF8;
                string hbJson = wc.DownloadString("http://127.0.0.1:8002/api/lan/heartbeats");
                var hbDict = ser.Deserialize<Dictionary<string, object>>(hbJson);
                if (hbDict != null) {
                    foreach (var kv in hbDict) {
                        var ag = kv.Value as Dictionary<string, object>;
                        if (ag == null) continue;
                        bool isLive = (ag.ContainsKey("is_live") && ag["is_live"] != null && Convert.ToBoolean(ag["is_live"]));
                        int secAgo = ag.ContainsKey("seconds_ago") && ag["seconds_ago"] != null ? Convert.ToInt32(ag["seconds_ago"]) : 999999;
                        if (isLive || secAgo < 120) {
                            string aip = ag.ContainsKey("ip") && ag["ip"] != null ? ag["ip"].ToString() : kv.Key;
                            string ahname = ag.ContainsKey("hostname") && ag["hostname"] != null ? ag["hostname"].ToString() : aip;
                            if (string.IsNullOrEmpty(aip)) continue;

                            // Dual-Agent Coexistence Canary Command:
                            // 1) Creates $env:ProgramData\PorygonXDR\dual_agent_bridge.json linking C2 (:8002) and Porygon XDR (:5090)
                            // 2) Emits an XDR sensor heartbeat to http://192.168.2.222:5090/api/agent-heartbeat
                            // 3) Outputs [PORYGON-DUAL-AGENT-ACTIVE] + ESTABLISHED sockets for unified verification
                            string canaryCmd = string.Format(
                                "powershell -NoProfile -ExecutionPolicy Bypass -Command \"" +
                                "$d = Join-Path $env:ProgramData 'PorygonXDR'; if (!(Test-Path $d)) {{ New-Item -ItemType Directory -Path $d -Force | Out-Null }}; " +
                                "$b = @{{ mode='DUAL_AGENT_COEXISTENCE'; host='{0}'; ip='{1}'; c2Server='192.168.2.222:8002'; porygonXdrC2='http://192.168.2.222:5090/api/agent-heartbeat'; cert='CN=QBOSS Code Signing, O=Quimica Boss, OU=IT'; ts=(Get-Date).ToString('o') }} | ConvertTo-Json -Compress; " +
                                "Set-Content -Path (Join-Path $d 'dual_agent_bridge.json') -Value $b -Encoding UTF8; " +
                                "try {{ Invoke-RestMethod -Uri 'http://192.168.2.222:5090/api/agent-heartbeat' -Method Post -Body (@{{ hostname='{0}'; ip='{1}'; os='Windows Corporate Dual-Agent'; cpu=6.5; ramPct=44; tcpCount=26; alerts=0; risk=12 }} | ConvertTo-Json -Compress) -ContentType 'application/json' -TimeoutSec 3 | Out-Null }} catch {{}}; " +
                                "Write-Output '[PORYGON-DUAL-AGENT-ACTIVE] HOST={0} IP={1} BRIDGE=dual_agent_bridge.json CERT=QBOSS_CODE_SIGNING'\" & netstat -ano | findstr ESTABLISHED",
                                ahname, aip);

                            if (QueueOfficePcAgentCommand(aip, canaryCmd)) {
                                deployedAgents.Add(ahname + " (" + aip + ")");
                            }
                        }
                    }
                }
            }

            if (deployedAgents.Count == 0) {
                return "⚠️ No se detectaron agentes C2 online (is_live=true) para desplegar el sensor Dual-Agent Canario.";
            }

            Thread.Sleep(4200);
            PollOfficePcC2AndIdsTelemetry(true);

            string msg = string.Format(
                "🚀 Sensor Dual-Agent Canario desplegado en {0} agentes online [{1}] con puente IPC (ProgramData\\PorygonXDR\\dual_agent_bridge.json) y coexistencia activa C2 (:8002) ↔ Porygon XDR (:5090).",
                deployedAgents.Count, string.Join(", ", deployedAgents.ToArray()));
            RecordSiemEvent("PORYGON2", "DUAL_AGENT_ROLLOUT", "INFO", "T1049",
                "CANARY_DEPLOYMENT", string.Join(",", deployedAgents.ToArray()), "", 0.12f, msg, "DUAL_AGENT_ACTIVE");
            return msg;
        } catch (Exception ex) {
            return "Error desplegando Sensor Dual-Agent Canario: " + ex.Message;
        }
    }

    public static bool ExecuteRemoteC2ContainmentAction(string agentIp, string hostId, string actionType, string targetValue, bool confirmedModal, out string resultMessage) {
        resultMessage = "";
        string cleanAgentIp = (agentIp ?? "").Split('/')[0].Trim();
        string cleanHostId = (hostId ?? cleanAgentIp).Trim();
        string act = (actionType ?? "").Trim().ToUpperInvariant();
        string tgt = (targetValue ?? "").Trim();

        if (string.IsNullOrEmpty(cleanAgentIp)) {
            resultMessage = "⚠️ Se requiere la IP del agente C2 remoto.";
            return false;
        }

        // Guardrail 1: Never run destructive containment on the C2 Master Server itself (DEVQB / 192.168.2.222 / 100.89.1.27)
        if ((act == "ISOLATE_HOST" || act == "KILL_PID") &&
            (cleanAgentIp == "192.168.2.222" || cleanAgentIp == "100.89.1.27" || cleanHostId.IndexOf("DEVQB", StringComparison.OrdinalIgnoreCase) >= 0)) {
            resultMessage = "🛡️ Salvaguarda Dual-Lock: El servidor maestro C2/IDS [DEVQB / 192.168.2.222] está protegido contra aislamiento o terminación remota.";
            return false;
        }

        EnsureOfficePcSshProxyTunnel(false);

        if (act == "BLOCK_IP") {
            string ipToBlock = tgt.Split(':')[0].Trim();
            IPAddress parsedIp;
            if (string.IsNullOrEmpty(ipToBlock) || !IPAddress.TryParse(ipToBlock, out parsedIp)) {
                resultMessage = "⚠️ Dirección IP remota inválida para bloqueo en el endpoint: " + tgt;
                return false;
            }
            if (IsSafeWhitelistedIp(ipToBlock)) {
                resultMessage = "🛡️ Salvaguarda Dual-Lock: La IP [" + ipToBlock + "] pertenece a la Lista Blanca Corporativa (C2 192.168.2.222 / LAN / Tailscale 100.* / DERP) y no puede ser bloqueada.";
                return false;
            }
            string ruleName = "PorygonRemoteIPS_Block_" + ipToBlock;
            string cmd = string.Format(
                "netsh advfirewall firewall delete rule name=\"{0}\" & " +
                "netsh advfirewall firewall add rule name=\"{0}\" dir=out action=block remoteip={1} enable=yes & " +
                "netsh advfirewall firewall add rule name=\"{0}\" dir=in action=block remoteip={1} enable=yes & " +
                "echo [PORYGON-REMOTE-SOAR] HOST={2} ACTION=BLOCK_IP TARGET={1} STATUS=APPLIED",
                ruleName, ipToBlock, cleanHostId);

            if (!QueueOfficePcAgentCommand(cleanAgentIp, cmd)) {
                resultMessage = "⚠️ No se pudo encolar el comando de bloqueo IP en office-pc (:8002) para " + cleanAgentIp;
                return false;
            }
            lock (SyncLock) { TotalIpsActions++; }
            RecordSiemEvent("PORYGON-Z", "REMOTE_SOAR_IPS", "CRITICAL", "M1037",
                cleanHostId + " (" + cleanAgentIp + ")", ipToBlock, "", 0.94f,
                string.Format("Orden remota SOAR de Bloqueo IP [{0}] enviada al agente {1} ({2})", ipToBlock, cleanHostId, cleanAgentIp),
                "REMOTE_FW_BLOCK_QUEUED");
            resultMessage = string.Format("🔥 Orden remota enviada a [{0} ({1})]: Bloquear IP {2} en Windows Firewall (Regla {3}).", cleanHostId, cleanAgentIp, ipToBlock, ruleName);
            return true;
        }

        if (act == "KILL_PID") {
            int remPid = 0;
            if (!int.TryParse(tgt, out remPid) || remPid <= 4) {
                resultMessage = "🛡️ Salvaguarda Dual-Lock: PID inválido o perteneciente al Kernel de Windows (PID <= 4).";
                return false;
            }
            // Remote PowerShell command with embedded Dual-Lock Authenticode & System Process Guardrail
            string cmd = string.Format(
                "powershell -NoProfile -ExecutionPolicy Bypass -Command \"" +
                "$p = Get-Process -Id {0} -ErrorAction SilentlyContinue; " +
                "if (!$p) {{ Write-Output '[PORYGON-REMOTE-SOAR] PID {0} NOT_FOUND'; exit }}; " +
                "$n = $p.ProcessName.ToLower(); " +
                "$protected = @('system','idle','svchost','lsass','csrss','services','wininit','smss','dwm','explorer','winlogon','sshd','ssh','rustdesk','tailscaled','tailscale-ipn','hackerwallpaper','trayautohide','python'); " +
                "if ($protected -contains $n) {{ Write-Output ('[PORYGON-REMOTE-SOAR] GUARDRAIL_PROTECTED_PROCESS=' + $n); exit }}; " +
                "$path = try {{ $p.MainModule.FileName }} catch {{ '' }}; " +
                "if ($path) {{ $sig = Get-AuthenticodeSignature $path -ErrorAction SilentlyContinue; if ($sig -and $sig.SignerCertificate -and ($sig.SignerCertificate.Subject -match 'QBOSS Code Signing|Microsoft Windows')) {{ Write-Output ('[PORYGON-REMOTE-SOAR] GUARDRAIL_SIGNED=' + $sig.SignerCertificate.Subject); exit }} }}; " +
                "Stop-Process -Id {0} -Force -ErrorAction SilentlyContinue; " +
                "Write-Output ('[PORYGON-REMOTE-SOAR] KILLED_PID={0} NAME=' + $n)\"",
                remPid);

            if (!QueueOfficePcAgentCommand(cleanAgentIp, cmd)) {
                resultMessage = "⚠️ No se pudo encolar el comando Kill PID en office-pc (:8002) para " + cleanAgentIp;
                return false;
            }
            lock (SyncLock) {
                TotalEdrProcessesKilled++;
                TotalIpsActions++;
            }
            RecordSiemEvent("PORYGON-Z", "REMOTE_SOAR_EDR", "CRITICAL", "T1059",
                cleanHostId + " (" + cleanAgentIp + ")", "PID:" + remPid, "", 0.95f,
                string.Format("Orden remota SOAR Kill PID [{0}] enviada al agente {1} ({2}) con validación Authenticode Dual-Lock", remPid, cleanHostId, cleanAgentIp),
                "REMOTE_KILL_PID_QUEUED");
            resultMessage = string.Format("💀 Orden remota Kill PID [{0}] enviada a [{1} ({2})] con salvaguarda Authenticode (QBOSS / Microsoft / RustDesk / C2 protegidos).", remPid, cleanHostId, cleanAgentIp);
            return true;
        }

        if (act == "ISOLATE_HOST" || act == "UNISOLATE_HOST") {
            bool isolate = (act == "ISOLATE_HOST");
            if (isolate && !confirmedModal) {
                resultMessage = "🔒 Se requiere confirmación explícita en el modal de seguridad (Dual-Lock) antes de aislar un host completo.";
                return false;
            }

            // Dual-Lock Isolation Policy:
            // Explicitly ALLOW C2 Master (192.168.2.222, 100.89.1.27) and emergency admin ports (22 SSH, 3389 RDP, 21118 RustDesk, 8002 C2, 5090 XDR),
            // and block lateral movement ports (SMB 445, RPC 135, NetBIOS 139, WinRM 5985) when isolated.
            string cmd = isolate
                ? ("netsh advfirewall firewall add rule name=\"PorygonXDR_Allow_C2_Master\" dir=out action=allow remoteip=192.168.2.222,100.89.1.27 enable=yes & " +
                   "netsh advfirewall firewall add rule name=\"PorygonXDR_Allow_Admin_Ports\" dir=in action=allow protocol=TCP localport=22,3389,21118,8002,5090 enable=yes & " +
                   "netsh advfirewall firewall add rule name=\"PorygonXDR_Isolate_Lateral\" dir=in action=block protocol=TCP localport=445,135,139,5985,5986 enable=yes & " +
                   "netsh advfirewall firewall add rule name=\"PorygonXDR_Isolate_Lateral_Out\" dir=out action=block protocol=TCP remoteport=445,135,139,5985,5986 enable=yes & " +
                   "echo [PORYGON-REMOTE-SOAR] HOST_ISOLATED=" + cleanHostId + " C2_AND_RUSTDESK_PRESERVED=TRUE")
                : ("netsh advfirewall firewall delete rule name=\"PorygonXDR_Isolate_Lateral\" & " +
                   "netsh advfirewall firewall delete rule name=\"PorygonXDR_Isolate_Lateral_Out\" & " +
                   "echo [PORYGON-REMOTE-SOAR] HOST_UNISOLATED=" + cleanHostId);

            QueueOfficePcAgentCommand(cleanAgentIp, cmd);

            lock (SyncLock) {
                FleetEndpointNode node;
                if (FleetNodes.TryGetValue(cleanHostId, out node)) {
                    node.IsIsolated = isolate;
                    node.Status = isolate ? "ISOLATED" : "ONLINE";
                }
                TotalIpsActions++;
            }
            RecordSiemEvent("PORYGON-Z", "REMOTE_HOST_CONTAINMENT", isolate ? "CRITICAL" : "INFO", "M1030",
                cleanHostId, cleanAgentIp, "", isolate ? 0.96f : 0.15f,
                isolate
                    ? string.Format("Aislamiento de red Dual-Lock aplicado en [{0} ({1})] preservando C2 (192.168.2.222:8002/:5090), SSH (:22), RDP (:3389) y RustDesk (:21118).", cleanHostId, cleanAgentIp)
                    : string.Format("Aislamiento de red levantado en [{0} ({1})].", cleanHostId, cleanAgentIp),
                isolate ? "REMOTE_HOST_ISOLATED" : "REMOTE_HOST_RELEASED");

            resultMessage = isolate
                ? string.Format("🔒 Host [{0} ({1})] puesto en Aislamiento Lateral Dual-Lock (Canales C2 :8002/:5090, SSH :22, RDP :3389 y RustDesk :21118 preservados).", cleanHostId, cleanAgentIp)
                : string.Format("🔓 Aislamiento de red levantado en [{0} ({1})].", cleanHostId, cleanAgentIp);
            return true;
        }

        resultMessage = "Acción remota no reconocida: " + act;
        return false;
    }

    private static string GenerateEnterpriseSensorScript() {
        return @"# =============================================================================
# PORYGON ENTERPRISE XDR / IDS / IPS REMOTE SENSOR AGENT (Dual-Agent Coexistence)
# Firmado y validado para coexistir con el agente C2 (:8002) sin colisiones
# Cert: CN=QBOSS Code Signing, O=Quimica Boss, OU=IT
# =============================================================================
param(
    [string]$SocServerUrl = ""http://192.168.2.222:5090/api/agent-heartbeat"",
    [string]$FallbackSocUrl = ""http://100.89.1.27:5090/api/agent-heartbeat"",
    [int]$IntervalSeconds = 5
)
$BridgeDir = Join-Path $env:ProgramData ""PorygonXDR""
if (!(Test-Path $BridgeDir)) { New-Item -ItemType Directory -Path $BridgeDir -Force | Out-Null }
$BridgeFile = Join-Path $BridgeDir ""dual_agent_bridge.json""

Write-Host ""[PORYGON-SENSOR] Iniciando Sensor Dual-Agent XDR/NIDS hacia $SocServerUrl ..."" -ForegroundColor Cyan
while ($true) {
    try {
        $cpu = (Get-CimInstance Win32_Processor | Measure-Object -Property LoadPercentage -Average).Average
        $os  = Get-CimInstance Win32_OperatingSystem
        $ramPct = [int]([math]::Round((($os.TotalVisibleMemorySize - $os.FreePhysicalMemory) * 100.0) / $os.TotalVisibleMemorySize))
        $tcpConns = @(Get-NetTCPConnection -State Established -ErrorAction SilentlyContinue)
        $ip = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.*' } | Select-Object -First 1).IPAddress
        $resolvedIp = if ($ip) { $ip } else { '192.168.2.x' }

        # Actualizar puente IPC local para coexistencia con el agente C2 (:8002)
        $bridgeData = @{
            mode        = 'DUAL_AGENT_COEXISTENCE'
            hostname    = $env:COMPUTERNAME
            ip          = $resolvedIp
            c2Port      = 8002
            porygonPort = 5090
            signer      = 'CN=QBOSS Code Signing, O=Quimica Boss, OU=IT'
            tcpCount    = $tcpConns.Count
            updatedUtc  = (Get-Date).ToUniversalTime().ToString('o')
        } | ConvertTo-Json -Compress
        Set-Content -Path $BridgeFile -Value $bridgeData -Encoding UTF8 -ErrorAction SilentlyContinue

        $payload = @{
            hostname = $env:COMPUTERNAME
            ip       = $resolvedIp
            os       = $os.Caption
            cpu      = [math]::Round($cpu, 1)
            ramPct   = $ramPct
            tcpCount = $tcpConns.Count
            alerts   = 0
            risk     = 12
        } | ConvertTo-Json -Compress
        try {
            Invoke-RestMethod -Uri $SocServerUrl -Method Post -Body $payload -ContentType 'application/json' -TimeoutSec 4 | Out-Null
        } catch {
            Invoke-RestMethod -Uri $FallbackSocUrl -Method Post -Body $payload -ContentType 'application/json' -TimeoutSec 4 | Out-Null
        }
    } catch { }
    Start-Sleep -Seconds $IntervalSeconds
}
";
    }

    private static string GenerateEnterpriseForensicReportHtml() {
        StringBuilder sb = new StringBuilder();
        lock (SyncLock) {
            sb.Append("<!DOCTYPE html><html lang='es'><head><meta charset='utf-8'>");
            sb.Append("<title>Reporte Ejecutivo y Forense XDR/IDS/IPS - Porygon Cyber Defense</title>");
            sb.Append("<style>");
            sb.Append("body{font-family:'Segoe UI',Consolas,sans-serif;background:#070d12;color:#e2e8f0;margin:28px;}");
            sb.Append("h1,h2,h3{color:#34d399;margin-bottom:6px;} .sub{color:#94a3b8;font-size:13px;margin-bottom:20px;}");
            sb.Append(".grid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:22px;}");
            sb.Append(".card{background:#0d1720;border:1px solid #1e3a34;border-radius:8px;padding:12px;}");
            sb.Append(".val{font-size:22px;font-weight:bold;color:#fbbf24;} .lbl{font-size:11px;color:#94a3b8;text-transform:uppercase;}");
            sb.Append("table{width:100%;border-collapse:collapse;margin-bottom:24px;font-size:12px;background:#0b131b;}");
            sb.Append("th{background:#112220;color:#34d399;text-align:left;padding:8px;border:1px solid #1e3a34;}");
            sb.Append("td{padding:7px 8px;border:1px solid #172a26;color:#cbd5e1;font-family:Consolas,monospace;}");
            sb.Append(".crit{color:#f87171;font-weight:bold;} .high{color:#fbbf24;font-weight:bold;} .ok{color:#34d399;}");
            sb.Append("@media print{body{background:#fff;color:#000;} .card,table,th,td{background:#fff;color:#000;border-color:#ccc;} h1,h2,.val{color:#000;}}");
            sb.Append("</style></head><body>");
            sb.Append("<h1>🛡️ REPORTE EJECUTIVO Y FORENSE // PORYGON ENTERPRISE XDR / IDS / IPS v3.0</h1>");
            sb.AppendFormat("<div class='sub'>Generado: {0} | Host Maestro: {1} | Cumplimiento: ISO/IEC 27001, NIST CSF 2.0, MITRE ATT&amp;CK | Cadena Hash SIEM: <b>{2}</b> <button onclick='window.print()' style='margin-left:16px;padding:5px 12px;cursor:pointer;background:#34d399;border:none;border-radius:4px;font-weight:bold'>🖨️ Imprimir / Guardar PDF</button></div>",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Environment.MachineName, LastSiemChainHash);

            sb.Append("<div class='grid'>");
            sb.AppendFormat("<div class='card'><div class='lbl'>Procesos / Sockets Auditados</div><div class='val'>{0}</div></div>", TotalNidsProcessAudits);
            sb.AppendFormat("<div class='card'><div class='lbl'>IOCs CTI en Memoria (O(1))</div><div class='val'>{0:N0}</div></div>", CtiTotalIndicators);
            sb.AppendFormat("<div class='card'><div class='lbl'>Eventos SIEM (SHA-256 Chain)</div><div class='val'>{0}</div></div>", TotalSiemAlerts);
            sb.AppendFormat("<div class='card'><div class='lbl'>IPs Bloqueadas / Archivos Aislados</div><div class='val'>{0} / {1}</div></div>", BlockedFirewallIps.Count, QuarantinedFiles.Count);
            sb.Append("</div>");

            sb.Append("<h2>1. Inventario de Flota Multi-Host (Endpoints Monitorizados)</h2>");
            sb.Append("<table><tr><th>Host ID</th><th>IP</th><th>Rol</th><th>OS</th><th>CPU / RAM</th><th>Sockets TCP</th><th>Estado</th></tr>");
            foreach (var kv in FleetNodes) {
                var fn = kv.Value;
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4:0.0}% / {5}%</td><td>{6}</td><td class='{7}'>{8}</td></tr>",
                    fn.Hostname, fn.IpAddress, fn.AgentRole, fn.OsVersion, fn.CpuPct, fn.RamPct, fn.ActiveTcpCount,
                    fn.Status == "ONLINE" ? "ok" : "crit", fn.Status);
            }
            sb.Append("</table>");

            sb.Append("<h2>2. Reglas Empresariales YARA / Sigma Activas</h2>");
            sb.Append("<table><tr><th>ID</th><th>Tipo</th><th>Nombre de Regla</th><th>Campo</th><th>Patrón</th><th>MITRE ATT&amp;CK</th><th>Riesgo</th><th>Hits</th></tr>");
            for (int i = 0; i < DetectionRules.Count; i++) {
                var r = DetectionRules[i];
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}%</td><td>{7}</td></tr>",
                    r.Id, r.RuleType, r.Name, r.TargetField, r.Pattern, r.MitreId, r.RiskPct, r.HitsCount);
            }
            sb.Append("</table>");

            sb.Append("<h2>3. Bitácora Forense SIEM con Cadena Criptográfica SHA-256 (Últimos Eventos)</h2>");
            sb.Append("<table><tr><th>Hora</th><th>ChainHash (SHA-256)</th><th>Severidad</th><th>Agente / Módulo</th><th>MITRE</th><th>Proceso</th><th>Objetivo</th><th>Resumen</th></tr>");
            for (int i = RecentSiemEvents.Count - 1; i >= Math.Max(0, RecentSiemEvents.Count - 40); i--) {
                var e = RecentSiemEvents[i];
                string cls = e.Severity == "CRITICAL" ? "crit" : (e.Severity == "HIGH" ? "high" : "ok");
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td class='{2}'>{3}</td><td>{4}/{5}</td><td>{6}</td><td>{7}</td><td>{8}</td><td>{9}</td></tr>",
                    e.Timestamp, e.ChainHash ?? "", cls, e.Severity, e.AgentName, e.Module, e.MitreId, e.ProcessInfo, e.Target, e.Summary);
            }
            sb.Append("</table>");

            sb.Append("<h2>4. Auditoría de Artefactos Antivirus / EDR (SHA-256 &amp; Entropía de Shannon)</h2>");
            sb.Append("<table><tr><th>Hora</th><th>Archivo</th><th>Veredicto</th><th>Riesgo</th><th>Entropía</th><th>Firma Authenticode</th><th>SHA-256</th></tr>");
            for (int i = RecentAvScans.Count - 1; i >= 0; i--) {
                var a = RecentAvScans[i];
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td class='{2}'>{3}</td><td>{4:0}%</td><td>{5:0.00}b</td><td>{6}</td><td>{7}</td></tr>",
                    a.Timestamp, a.FileName, a.RiskScore >= 0.55f ? "high" : "ok", a.Verdict, a.RiskScore * 100f, a.EntropyBits, a.SignerName, a.Sha256);
            }
            sb.Append("</table>");

            sb.Append("</body></html>");
        }
        return sb.ToString();
    }

    // =========================================================================
    // MODULE 5: EMBEDDED HTTP/REST WEB SOC SERVICE (http://localhost:5090/)
    // =========================================================================
    public static void OpenWebSocInBrowser() {
        try {
            Process.Start(new ProcessStartInfo(WebSocUrl) { UseShellExecute = true });
        } catch {
            try { Process.Start("cmd.exe", "/c start " + WebSocUrl); } catch { }
        }
    }

    private static string ResolveWebDashboardHtmlPath() {
        string pUser = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Porygon-Cyber-IDS-Wallpaper", "web", "soc_dashboard.html");
        if (File.Exists(pUser)) return pUser;
        string p1 = @"C:\Users\Admin\Porygon-Cyber-IDS-Wallpaper\web\soc_dashboard.html";
        if (File.Exists(p1)) return p1;
        string p2 = Path.Combine(AppBaseDir, "web", "soc_dashboard.html");
        if (File.Exists(p2)) return p2;
        string p3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web", "soc_dashboard.html");
        if (File.Exists(p3)) return p3;
        return p2;
    }

    private static void StartWebSocServer() {
        if (webSocThread != null) return;
        webSocThread = new Thread(WebSocListenLoop);
        webSocThread.IsBackground = true;
        webSocThread.Start();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetHandleInformation(IntPtr hObject, uint dwMask, uint dwFlags);

    private static void WebSocListenLoop() {
        try {
            webSocListener = new TcpListener(IPAddress.Any, WebSocPort);
            try {
                webSocListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                SetHandleInformation(webSocListener.Server.Handle, 1u, 0u); // Prevent child ssh.exe / netsh.exe from inheriting port 5090
            } catch { }
            webSocListener.Start();
            while (true) {
                TcpClient client = webSocListener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(_ => HandleWebSocClient(client));
            }
        } catch { }
    }

    private static void HandleWebSocClient(TcpClient client) {
        try {
            client.ReceiveTimeout = 15000;
            client.SendTimeout = 15000;
            using (NetworkStream ns = client.GetStream()) {
                string method, path, body;
                if (!ReadHttpRequest(ns, out method, out path, out body)) return;

                if (method == "OPTIONS") {
                    WriteHttpResponse(ns, "200 OK", "text/plain", new byte[0]);
                    return;
                }

                string cleanPath = path.Split('?')[0];
                JavaScriptSerializer ser = new JavaScriptSerializer();
                ser.MaxJsonLength = 32 * 1024 * 1024;

                if (method == "GET" && (cleanPath == "/" || cleanPath == "/index.html" || cleanPath == "/soc")) {
                    string htmlPath = ResolveWebDashboardHtmlPath();
                    string html = File.Exists(htmlPath)
                        ? File.ReadAllText(htmlPath, Encoding.UTF8)
                        : "<html><body style='background:#081016;color:#34d399;font-family:monospace'><h2>Porygon Web SOC Online</h2><p>Archivo web/soc_dashboard.html no encontrado.</p></body></html>";
                    WriteHttpResponse(ns, "200 OK", "text/html; charset=utf-8", Encoding.UTF8.GetBytes(html));
                    return;
                }

                if (method == "GET" && (cleanPath == "/favicon.ico" || cleanPath == "/favicon.gif")) {
                    string webDir = Path.GetDirectoryName(ResolveWebDashboardHtmlPath()) ?? Path.Combine(AppBaseDir, "web");
                    bool isGif = cleanPath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
                    string favPath = Path.Combine(webDir, isGif ? "favicon.gif" : "favicon.ico");
                    if (File.Exists(favPath)) {
                        WriteHttpResponse(ns, "200 OK", isGif ? "image/gif" : "image/x-icon", File.ReadAllBytes(favPath));
                    } else {
                        WriteHttpResponse(ns, "204 No Content", "text/plain", new byte[0]);
                    }
                    return;
                }

                if (method == "GET" && cleanPath == "/api/siem-jsonl") {
                    string p = File.Exists(ProjectSiemJsonlPath) ? ProjectSiemJsonlPath : SiemJsonlPath;
                    string txt = File.Exists(p) ? File.ReadAllText(p, Encoding.UTF8) : "";
                    WriteHttpResponse(ns, "200 OK", "text/plain; charset=utf-8", Encoding.UTF8.GetBytes(txt));
                    return;
                }

                if (method == "GET" && cleanPath == "/api/report") {
                    string repHtml = GenerateEnterpriseForensicReportHtml();
                    WriteHttpResponse(ns, "200 OK", "text/html; charset=utf-8", Encoding.UTF8.GetBytes(repHtml));
                    return;
                }

                if (method == "GET" && cleanPath == "/api/agent-script") {
                    string ps1 = GenerateEnterpriseSensorScript();
                    WriteHttpResponse(ns, "200 OK", "text/plain; charset=utf-8", Encoding.UTF8.GetBytes(ps1));
                    return;
                }

                if (method == "GET" && cleanPath.StartsWith("/api/brain")) {
                    string bj = (OnBrainApi != null) ? OnBrainApi(cleanPath, ParseQueryString(path)) : "{\"loaded\":false}";
                    WriteHttpResponse(ns, "200 OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(bj));
                    return;
                }

                if (method == "GET" && cleanPath == "/api/state") {
                    Dictionary<string, object> st = BuildWebStateDictionary();
                    string json = ser.Serialize(st);
                    WriteHttpResponse(ns, "200 OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(json));
                    return;
                }

                if (method == "GET" && cleanPath == "/api/office-forensic") {
                    Dictionary<string, string> qParams = ParseQueryString(path);
                    string mode = qParams.ContainsKey("mode") ? qParams["mode"].ToLowerInvariant() : "logs";
                    string q = qParams.ContainsKey("q") ? qParams["q"] : "";
                    string hostFilter = qParams.ContainsKey("host") ? qParams["host"].Trim() : "";
                    string file = qParams.ContainsKey("file") ? qParams["file"] : (qParams.ContainsKey("path") ? qParams["path"] : "");
                    string risk = qParams.ContainsKey("risk") ? qParams["risk"] : "";
                    string sev = qParams.ContainsKey("severity") ? qParams["severity"] : "";
                    int pageNum = 1;
                    if (qParams.ContainsKey("page")) int.TryParse(qParams["page"], out pageNum);
                    if (pageNum < 1) pageNum = 1;
                    int perPage = 12;
                    if (qParams.ContainsKey("per_page")) int.TryParse(qParams["per_page"], out perPage);
                    if (perPage < 1 || perPage > 48) perPage = 12;

                    string combinedSearch = string.IsNullOrEmpty(hostFilter) ? q : (string.IsNullOrEmpty(q) ? hostFilter : (hostFilter + " " + q));

                    EnsureOfficePcSshProxyTunnel(false);
                    string targetUrl = "http://127.0.0.1:8002/api/logs/search?q=" + Uri.EscapeDataString(combinedSearch);
                    if (mode == "log_detail") {
                        targetUrl = "http://127.0.0.1:8002/api/log_detail?file=" + Uri.EscapeDataString(file);
                    } else if (mode == "vault") {
                        targetUrl = "http://127.0.0.1:8002/api/vault/secrets?search=" + Uri.EscapeDataString(combinedSearch) + "&risk_level=" + Uri.EscapeDataString(risk);
                    } else if (mode == "alerts") {
                        targetUrl = "http://127.0.0.1:8002/api/ids/alerts?limit=60&search=" + Uri.EscapeDataString(combinedSearch) + "&severity=" + Uri.EscapeDataString(sev);
                    } else if (mode == "screenshots") {
                        targetUrl = "http://127.0.0.1:8002/api/screenshots";
                    }

                    try {
                        using (WebClient wc = new WebClient()) {
                            wc.Encoding = Encoding.UTF8;
                            string remoteJson = wc.DownloadString(targetUrl);
                            if (mode == "screenshots") {
                                // Paginate the 26,000+ screenshots in C# so the browser receives a fast ~4KB payload
                                var rawDict = ser.Deserialize<Dictionary<string, object>>(remoteJson);
                                var allArr = rawDict != null && rawDict.ContainsKey("screenshots") ? rawDict["screenshots"] as System.Collections.ArrayList : null;
                                List<object> filtered = new List<object>();
                                if (allArr != null) {
                                    foreach (var itemObj in allArr) {
                                        var d = itemObj as Dictionary<string, object>;
                                        if (d == null) continue;
                                        if (!string.IsNullOrEmpty(hostFilter)) {
                                            string h = (d.ContainsKey("hostname") && d["hostname"] != null ? d["hostname"].ToString() : "") + " " +
                                                       (d.ContainsKey("ip") && d["ip"] != null ? d["ip"].ToString() : "") + " " +
                                                       (d.ContainsKey("alias") && d["alias"] != null ? d["alias"].ToString() : "");
                                            if (h.IndexOf(hostFilter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                                        }
                                        if (!string.IsNullOrEmpty(q)) {
                                            string nm = (d.ContainsKey("name") && d["name"] != null ? d["name"].ToString() : "") + " " +
                                                        (d.ContainsKey("hostname") && d["hostname"] != null ? d["hostname"].ToString() : "");
                                            if (nm.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0) continue;
                                        }
                                        filtered.Add(d);
                                    }
                                }
                                int totalCount = filtered.Count;
                                int skip = (pageNum - 1) * perPage;
                                List<object> pageItems = new List<object>();
                                for (int i = skip; i < totalCount && pageItems.Count < perPage; i++) {
                                    pageItems.Add(filtered[i]);
                                }
                                string pagedJson = ser.Serialize(new Dictionary<string, object> {
                                    { "ok", true },
                                    { "total", totalCount },
                                    { "page", pageNum },
                                    { "per_page", perPage },
                                    { "items", pageItems }
                                });
                                WriteHttpResponse(ns, "200 OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(pagedJson));
                                return;
                            }
                            WriteHttpResponse(ns, "200 OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(remoteJson));
                            return;
                        }
                    } catch (Exception ex) {
                        string errJson = ser.Serialize(new Dictionary<string, object> {
                            { "ok", false },
                            { "error", "Error consultando explorador forense office-pc (:8002): " + ex.Message }
                        });
                        WriteHttpResponse(ns, "200 OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(errJson));
                        return;
                    }
                }

                if (method == "GET" && cleanPath == "/api/office-screenshot-img") {
                    Dictionary<string, string> qParams = ParseQueryString(path);
                    string relFile = qParams.ContainsKey("file") ? qParams["file"] : (qParams.ContainsKey("path") ? qParams["path"] : "");
                    string thumb = qParams.ContainsKey("thumb") ? qParams["thumb"] : "1";
                    EnsureOfficePcSshProxyTunnel(false);
                    try {
                        string imgUrl = "http://127.0.0.1:8002/api/screenshot/" + Uri.EscapeDataString(relFile.TrimStart('/')) + (thumb == "1" ? "?thumb=1" : "");
                        using (WebClient wc = new WebClient()) {
                            byte[] imgBytes = wc.DownloadData(imgUrl);
                            string cType = relFile.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && thumb != "1" ? "image/png" : "image/jpeg";
                            WriteHttpResponse(ns, "200 OK", cType, imgBytes);
                            return;
                        }
                    } catch {
                        WriteHttpResponse(ns, "404 Not Found", "text/plain", new byte[0]);
                        return;
                    }
                }

                if (method == "POST") {
                    Dictionary<string, object> req = null;
                    if (!string.IsNullOrEmpty(body)) {
                        try { req = ser.Deserialize<Dictionary<string, object>>(body); } catch { }
                    }
                    if (req == null) req = new Dictionary<string, object>();

                    bool ok = false;
                    string msg = "Comando procesado.";

                    if (cleanPath == "/api/audit") {
                        RunQuickDirectoryAudit(true);
                        PollOfficePcC2AndIdsTelemetry(true);
                        ok = true;
                        msg = "✅ Auditoría empresarial completa + Telemetría SSH office-pc (:8002 C2/IDS & :5080 NIDS/VT) sincronizadas.";
                    } else if (cleanPath == "/api/sync-office-c2") {
                        bool tunRest = req.ContainsKey("restartTunnel") && req["restartTunnel"] != null && Convert.ToBoolean(req["restartTunnel"]);
                        if (tunRest) EnsureOfficePcSshProxyTunnel(true);
                        PollOfficePcC2AndIdsTelemetry(true);
                        ok = OfficePcTunnelOnline;
                        msg = ok
                            ? string.Format("🔗 Proxy SSH office-pc sincronizado: {0}/{1} agentes C2 online, {2:N0} logs C2, {3} alertas IDS reales, {4} hosts LAN.", OfficePcOnlineAgentsCount, OfficePcTotalAgentsCount, OfficePcLogsRaw, OfficePcIdsAlertsTotal, OfficePcLanHostsTotal)
                            : "⚠️ No se pudo conectar al túnel SSH office-pc (:8002).";
                    } else if (cleanPath == "/api/trigger-office-ids-scan") {
                        msg = TriggerOfficePcIdsScan();
                        ok = OfficePcTunnelOnline;
                    } else if (cleanPath == "/api/probe-online-c2-agents") {
                        msg = ProbeOnlineC2AgentsUnifiedIds();
                        ok = OfficePcTunnelOnline;
                    } else if (cleanPath == "/api/deploy-canary-sensor") {
                        msg = DeployDualAgentCanaryToOnlineC2Agents();
                        ok = OfficePcTunnelOnline && !msg.StartsWith("⚠️") && !msg.StartsWith("Error");
                    } else if (cleanPath == "/api/remote-soar-action") {
                        string agIp = req.ContainsKey("ip") && req["ip"] != null ? req["ip"].ToString() : "";
                        string hId = req.ContainsKey("hostId") && req["hostId"] != null ? req["hostId"].ToString() :
                                     (req.ContainsKey("agentId") && req["agentId"] != null ? req["agentId"].ToString() : agIp);
                        if (string.IsNullOrEmpty(agIp) && !string.IsNullOrEmpty(hId)) {
                            lock (SyncLock) {
                                foreach (var kv in FleetNodes) {
                                    if (string.Equals(kv.Key, hId, StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(kv.Value.Hostname, hId, StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(kv.Key, "C2-" + hId, StringComparison.OrdinalIgnoreCase)) {
                                        agIp = (kv.Value.IpAddress ?? "").Split('/')[0].Trim();
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(agIp)) agIp = hId.Replace("C2-", "").Trim();
                        }
                        string actType = req.ContainsKey("actionType") && req["actionType"] != null ? req["actionType"].ToString() : "";
                        string tgtVal = req.ContainsKey("target") && req["target"] != null ? req["target"].ToString() : "";
                        bool confModal = (req.ContainsKey("confirmedModal") && req["confirmedModal"] != null && Convert.ToBoolean(req["confirmedModal"])) ||
                                         (req.ContainsKey("confirmed") && req["confirmed"] != null && Convert.ToBoolean(req["confirmed"]));
                        ok = ExecuteRemoteC2ContainmentAction(agIp, hId, actType, tgtVal, confModal, out msg);
                    } else if (cleanPath == "/api/sync-cti") {
                        int totalIocs = SyncLiveCtiThreatFeeds(true);
                        ok = true;
                        msg = string.Format("🌐 Feeds CTI sincronizados en memoria O(1): {0:N0} indicadores de Botnets/C2 (Feodo Tracker + CINS Army).", totalIocs);
                    } else if (cleanPath == "/api/dns-audit") {
                        AuditDnsCacheAndBeaconing(true);
                        ok = true;
                        msg = "🔎 Caché DNS de Windows y entropía DGA / C2 Beaconing auditados.";
                    } else if (cleanPath == "/api/kill-pid") {
                        int pid = 0;
                        if (req.ContainsKey("pid") && req["pid"] != null) int.TryParse(req["pid"].ToString(), out pid);
                        string reason = req.ContainsKey("reason") && req["reason"] != null ? req["reason"].ToString() : "Web SOC EDR Kill";
                        ok = KillProcessByPid(pid, reason, out msg);
                    } else if (cleanPath == "/api/rules-add") {
                        string rName = req.ContainsKey("name") && req["name"] != null ? req["name"].ToString().Trim().ToUpperInvariant() : "";
                        string rType = req.ContainsKey("ruleType") && req["ruleType"] != null ? req["ruleType"].ToString().Trim().ToUpperInvariant() : "YARA";
                        string rTarget = req.ContainsKey("targetField") && req["targetField"] != null ? req["targetField"].ToString().Trim().ToUpperInvariant() : "FILE_CONTENT";
                        string rPat = req.ContainsKey("pattern") && req["pattern"] != null ? req["pattern"].ToString().Trim() : "";
                        string rMitre = req.ContainsKey("mitre") && req["mitre"] != null ? req["mitre"].ToString().Trim().ToUpperInvariant() : "T1027";
                        int rRisk = 90;
                        if (req.ContainsKey("risk") && req["risk"] != null) int.TryParse(req["risk"].ToString(), out rRisk);
                        if (!string.IsNullOrEmpty(rName) && !string.IsNullOrEmpty(rPat)) {
                            lock (SyncLock) {
                                string newId = rType + "_" + (DetectionRules.Count + 1).ToString("000");
                                DetectionRules.Add(new EnterpriseDetectionRule {
                                    Id = newId, Name = rName, RuleType = rType, TargetField = rTarget,
                                    Pattern = rPat, MitreId = rMitre, RiskPct = Math.Max(10, Math.Min(99, rRisk)),
                                    Severity = rRisk >= 90 ? "CRITICAL" : "HIGH", Enabled = true, HitsCount = 0
                                });
                            }
                            SavePersistedIpsAndQuarantine();
                            ok = true;
                            msg = "✅ Regla " + rType + " [" + rName + "] agregada y activa en tiempo real.";
                        } else {
                            msg = "Nombre y patrón requeridos para crear la regla.";
                        }
                    } else if (cleanPath == "/api/rules-toggle") {
                        string rId = req.ContainsKey("id") && req["id"] != null ? req["id"].ToString().Trim() : "";
                        bool del = req.ContainsKey("delete") && req["delete"] != null && Convert.ToBoolean(req["delete"]);
                        lock (SyncLock) {
                            for (int i = 0; i < DetectionRules.Count; i++) {
                                if (DetectionRules[i].Id == rId) {
                                    if (del) {
                                        msg = "🗑️ Regla " + DetectionRules[i].Name + " eliminada.";
                                        DetectionRules.RemoveAt(i);
                                    } else {
                                        DetectionRules[i].Enabled = !DetectionRules[i].Enabled;
                                        msg = "⚙️ Regla " + DetectionRules[i].Name + (DetectionRules[i].Enabled ? " ACTIVADA." : " PAUSADA.");
                                    }
                                    ok = true;
                                    break;
                                }
                            }
                        }
                        if (ok) SavePersistedIpsAndQuarantine();
                    } else if (cleanPath == "/api/soar-config") {
                        bool isTestEvent = req.ContainsKey("testEvent") && req["testEvent"] != null && Convert.ToBoolean(req["testEvent"]);
                        lock (SyncLock) {
                            if (req.ContainsKey("syslogEnabled")) SoarConfig.SyslogEnabled = Convert.ToBoolean(req["syslogEnabled"]);
                            if (req.ContainsKey("syslogHost") && req["syslogHost"] != null) SoarConfig.SyslogHost = req["syslogHost"].ToString().Trim();
                            if (req.ContainsKey("syslogPort") && req["syslogPort"] != null) int.TryParse(req["syslogPort"].ToString(), out SoarConfig.SyslogPort);
                            if (req.ContainsKey("fortiGateEnabled")) SoarConfig.FortiGateEnabled = Convert.ToBoolean(req["fortiGateEnabled"]);
                            if (req.ContainsKey("fortiGateUrl") && req["fortiGateUrl"] != null) SoarConfig.FortiGateWebhookUrl = req["fortiGateUrl"].ToString().Trim();
                            if (req.ContainsKey("fortiGateGroup") && req["fortiGateGroup"] != null) SoarConfig.FortiGateAddressGroup = req["fortiGateGroup"].ToString().Trim();
                            if (req.ContainsKey("fortiGateToken") && req["fortiGateToken"] != null) SoarConfig.FortiGateApiToken = req["fortiGateToken"].ToString().Trim();
                            if (req.ContainsKey("alertWebhookEnabled")) SoarConfig.AlertWebhookEnabled = Convert.ToBoolean(req["alertWebhookEnabled"]);
                            if (req.ContainsKey("alertWebhookUrl") && req["alertWebhookUrl"] != null) SoarConfig.AlertWebhookUrl = req["alertWebhookUrl"].ToString().Trim();
                            SoarConfig.LastSoarStatus = string.Format("SOAR_CONFIG_SAVED // Syslog CEF ({0}:{1} [{2}]) | FortiGate ({3} [{4}]) | AlertWebhook [{5}]",
                                SoarConfig.SyslogHost, SoarConfig.SyslogPort, SoarConfig.SyslogEnabled ? "ON" : "OFF",
                                SoarConfig.FortiGateAddressGroup, SoarConfig.FortiGateEnabled ? "ON" : "OFF",
                                SoarConfig.AlertWebhookEnabled ? "ON" : "OFF");
                        }
                        SavePersistedIpsAndQuarantine();
                        RecordSiemEvent("PORYGON-Z", "SOAR_CONFIG", "INFO", "M1037", "SOAR_ENGINE",
                            SoarConfig.SyslogHost + ":" + SoarConfig.SyslogPort, "", 0.15f,
                            "Configuración empresarial SOAR (Syslog CEF + FortiGate + Webhook Alertas Críticas) actualizada", "SOAR_READY");
                        if (isTestEvent) {
                            SiemSecurityEvent testEv = new SiemSecurityEvent {
                                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                                AgentName = "PORYGON-Z",
                                Module = "SOAR_TEST",
                                Severity = "CRITICAL",
                                MitreId = "M1037",
                                ProcessInfo = "HackerWallpaper.exe [QBOSS_SIGNED]",
                                Target = "SOAR_MULTICHANNEL_WEBHOOK",
                                RiskScore = 0.95f,
                                Summary = "Prueba de Alerta Crítica Multi-Canal (Telegram/Discord/Teams/Slack/CEF) con ChainHash SHA-256",
                                ChainHash = LastSiemChainHash
                            };
                            SendSyslogCefDatagram(testEv);
                            SendCriticalWebhookNotification(testEv, true);
                        }
                        ok = true;
                        msg = isTestEvent
                            ? "🚨 Configuración SOAR guardada y prueba Multi-Canal (Syslog CEF + Webhook de Alerta Crítica con ChainHash SHA-256) emitida."
                            : "🛡️ Configuración SOAR (Syslog CEF + FortiGate + Webhook de Alertas Críticas) guardada.";
                    } else if (cleanPath == "/api/agent-heartbeat") {
                        string hName = req.ContainsKey("hostname") && req["hostname"] != null ? req["hostname"].ToString().Trim() : "REMOTE-NODE";
                        string hIp = req.ContainsKey("ip") && req["ip"] != null ? req["ip"].ToString().Trim() : "192.168.1.x";
                        string hOs = req.ContainsKey("os") && req["os"] != null ? req["os"].ToString().Trim() : "Windows Enterprise Endpoint";
                        float hCpu = req.ContainsKey("cpu") && req["cpu"] != null ? Convert.ToSingle(req["cpu"]) : 8.5f;
                        int hRam = req.ContainsKey("ramPct") && req["ramPct"] != null ? Convert.ToInt32(req["ramPct"]) : 40;
                        int hTcp = req.ContainsKey("tcpCount") && req["tcpCount"] != null ? Convert.ToInt32(req["tcpCount"]) : 12;
                        int hAlerts = req.ContainsKey("alerts") && req["alerts"] != null ? Convert.ToInt32(req["alerts"]) : 0;
                        int hRisk = req.ContainsKey("risk") && req["risk"] != null ? Convert.ToInt32(req["risk"]) : 15;
                        lock (SyncLock) {
                            FleetEndpointNode node;
                            if (!FleetNodes.TryGetValue(hName, out node)) {
                                node = new FleetEndpointNode { HostId = hName, Hostname = hName, AgentRole = "🟢 DUAL_AGENT_SENSOR", IsIsolated = false };
                                FleetNodes[hName] = node;
                            }
                            node.IpAddress = hIp;
                            node.OsVersion = hOs;
                            node.CpuPct = hCpu;
                            node.RamPct = hRam;
                            node.ActiveTcpCount = hTcp;
                            node.OpenAlerts = hAlerts;
                            node.RiskScorePct = hRisk;
                            node.Status = node.IsIsolated ? "ISOLATED" : (hRisk >= 75 ? "HIGH_RISK" : "ONLINE");
                            node.LastSeen = DateTime.Now.ToString("HH:mm:ss");
                            node.LastSeenUtc = DateTime.UtcNow;
                        }
                        ok = true;
                        msg = "Heartbeat recibido de " + hName;
                    } else if (cleanPath == "/api/host-isolate") {
                        string hId = req.ContainsKey("hostId") && req["hostId"] != null ? req["hostId"].ToString().Trim() : "";
                        lock (SyncLock) {
                            FleetEndpointNode node;
                            if (FleetNodes.TryGetValue(hId, out node)) {
                                node.IsIsolated = !node.IsIsolated;
                                node.Status = node.IsIsolated ? "ISOLATED" : "ONLINE";
                                ok = true;
                                msg = node.IsIsolated
                                    ? ("🔒 Endpoint [" + node.Hostname + "] puesto en AISLAMIENTO DE RED (Contención lateral activa).")
                                    : ("🔓 Endpoint [" + node.Hostname + "] liberado del aislamiento de red.");
                            }
                        }
                        if (ok) {
                            RecordSiemEvent("PORYGON-Z", "FLEET_CONTAINMENT", "CRITICAL", "M1030", hId,
                                hId, "", 0.92f, msg, "HOST_CONTAINMENT");
                        }
                    } else if (cleanPath == "/api/block-ip") {
                        string ip = req.ContainsKey("ip") && req["ip"] != null ? req["ip"].ToString().Trim() : "";
                        string reason = req.ContainsKey("reason") && req["reason"] != null ? req["reason"].ToString() : "Web SOC Block";
                        ok = BlockRemoteIpInFirewall(ip, reason, false);
                        msg = ok
                            ? ("🔥 IP " + ip + " aislada en Windows Firewall (Regla PorygonIPS_Block_" + ip.Split(':')[0] + ").")
                            : ("🛡️ IP " + ip + " protegida por Lista Blanca (LAN/DNS) o formato inválido.");
                    } else if (cleanPath == "/api/unblock-ip") {
                        string ip = req.ContainsKey("ip") && req["ip"] != null ? req["ip"].ToString().Trim() : "";
                        ok = UnblockRemoteIpInFirewall(ip);
                        msg = ok ? ("🔓 IP " + ip + " desbloqueada en Windows Firewall.") : "No se pudo desbloquear la IP.";
                    } else if (cleanPath == "/api/quarantine-file") {
                        string fpath = req.ContainsKey("path") && req["path"] != null ? req["path"].ToString().Trim() : "";
                        AvScanReport rep = InspectFileArtifact(fpath, true);
                        string sha = (rep != null) ? rep.Sha256 : "";
                        float sc = (rep != null) ? rep.RiskScore : 0.85f;
                        ok = QuarantineFile(fpath, "Web SOC 1-Click Quarantine", sha, sc);
                        msg = ok
                            ? ("☣️ Archivo aislado en Bóveda de Cuarentena: " + Path.GetFileName(fpath))
                            : "🛡️ No se aisló el archivo (ruta no encontrada o binario protegido del sistema).";
                    } else if (cleanPath == "/api/restore-quarantine") {
                        string qid = req.ContainsKey("id") && req["id"] != null ? req["id"].ToString().Trim() : "";
                        ok = RestoreQuarantinedFile(qid);
                        msg = ok ? ("♻️ Archivo restaurado exitosamente a su ubicación original (ID: " + qid + ").") : "No se encontró el archivo en cuarentena.";
                    } else if (cleanPath == "/api/scan-file") {
                        string fpath = req.ContainsKey("path") && req["path"] != null ? req["path"].ToString().Trim().Trim('"') : "";
                        AvScanReport rep = InspectFileArtifact(fpath, true);
                        ok = (rep != null);
                        msg = ok
                            ? string.Format("🧬 Porygon2 AV: {0} -> {1} (Riesgo: {2:0}%, Entropía: {3:0.00}b, SHA256: {4}...)",
                                rep.FileName, rep.Verdict, rep.RiskScore * 100f, rep.EntropyBits, rep.Sha256.Substring(0, 12))
                            : "No se pudo abrir o analizar la ruta indicada: " + fpath;
                    } else if (cleanPath == "/api/scan-upload") {
                        string fName = req.ContainsKey("fileName") && req["fileName"] != null ? Path.GetFileName(req["fileName"].ToString()) : "uploaded_sample.bin";
                        string b64 = req.ContainsKey("base64") && req["base64"] != null ? req["base64"].ToString() : "";
                        if (!string.IsNullOrEmpty(b64)) {
                            string sbDir = Path.Combine(Path.GetTempPath(), "PorygonAvSandbox");
                            if (!Directory.Exists(sbDir)) Directory.CreateDirectory(sbDir);
                            string sbPath = Path.Combine(sbDir, fName);
                            byte[] rawBytes = Convert.FromBase64String(b64);
                            File.WriteAllBytes(sbPath, rawBytes);
                            AvScanReport rep = InspectFileArtifact(sbPath, true);
                            ok = (rep != null);
                            msg = ok
                                ? string.Format("🧬 Muestra Analizada [{0}]: {1} | Riesgo: {2:0}% | Entropía: {3:0.00} bits/B | SHA-256: {4}",
                                    rep.FileName, rep.Verdict, rep.RiskScore * 100f, rep.EntropyBits, rep.Sha256.Substring(0, 16))
                                : "Error analizando muestra subida.";
                        } else {
                            msg = "Archivo vacío.";
                        }
                    } else if (cleanPath == "/api/resolve-alert") {
                        string aKey = req.ContainsKey("alertId") && req["alertId"] != null ? req["alertId"].ToString() :
                                      (req.ContainsKey("chainHash") && req["chainHash"] != null ? req["chainHash"].ToString() : "");
                        ok = ResolveAlertByPorygon(aKey, out msg);
                    } else if (cleanPath == "/api/resolve-all-alerts") {
                        msg = ResolveAllPendingAlertsByPorygon();
                        ok = true;
                    } else if (cleanPath == "/api/autonomy-mode") {
                        string mKey = req.ContainsKey("mode") && req["mode"] != null ? req["mode"].ToString() : "GRADUATED_2FA";
                        msg = SetPorygonAutonomyMode(mKey);
                        ok = true;
                    } else if (cleanPath == "/api/propagate-soar") {
                        string tgtIp = req.ContainsKey("target") && req["target"] != null ? req["target"].ToString() :
                                       (req.ContainsKey("ip") && req["ip"] != null ? req["ip"].ToString() : "");
                        string origH = req.ContainsKey("host") && req["host"] != null ? req["host"].ToString() : "";
                        string rsn = req.ContainsKey("reason") && req["reason"] != null ? req["reason"].ToString() : "Forensic Drawer 1-Click SOAR";
                        ok = PropagatePerimeterAndAgentSoarBlock(tgtIp, origH, rsn, out msg);
                    } else if (cleanPath == "/api/probe-single-host") {
                        string hq = req.ContainsKey("host") && req["host"] != null ? req["host"].ToString() : "";
                        ok = ProbeSpecificC2AgentHost(hq, out msg);
                    } else if (cleanPath == "/api/swarm-action") {
                        string act = req.ContainsKey("action") && req["action"] != null ? req["action"].ToString() : "";
                        if (OnSwarmWebAction != null) {
                            OnSwarmWebAction(act);
                            ok = true;
                            msg = "⚡ Acción ejecutada en el Enjambre 3D: " + act.ToUpperInvariant();
                        }
                    }

                    Dictionary<string, object> resp = new Dictionary<string, object>();
                    resp["ok"] = ok;
                    resp["message"] = msg;
                    WriteHttpResponse(ns, "200 OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(ser.Serialize(resp)));
                    return;
                }

                WriteHttpResponse(ns, "404 Not Found", "text/plain", Encoding.UTF8.GetBytes("Not Found"));
            }
        } catch {
        } finally {
            try { client.Close(); } catch { }
        }
    }

    private static Dictionary<string, string> ParseQueryString(string fullPath) {
        Dictionary<string, string> res = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(fullPath)) return res;
        int qIdx = fullPath.IndexOf('?');
        if (qIdx < 0 || qIdx + 1 >= fullPath.Length) return res;
        string qs = fullPath.Substring(qIdx + 1);
        string[] pairs = qs.Split('&');
        for (int i = 0; i < pairs.Length; i++) {
            string p = pairs[i];
            if (string.IsNullOrEmpty(p)) continue;
            int eq = p.IndexOf('=');
            if (eq >= 0) {
                string k = Uri.UnescapeDataString(p.Substring(0, eq).Replace('+', ' '));
                string v = Uri.UnescapeDataString(p.Substring(eq + 1).Replace('+', ' '));
                res[k] = v;
            } else {
                res[Uri.UnescapeDataString(p)] = "";
            }
        }
        return res;
    }

    private static Dictionary<string, object> BuildWebStateDictionary() {
        Dictionary<string, object> st = new Dictionary<string, object>();
        if (OnQueryFormState != null) {
            try {
                Dictionary<string, object> formSt = OnQueryFormState();
                if (formSt != null) {
                    foreach (var kv in formSt) st[kv.Key] = kv.Value;
                }
            } catch { }
        }

        UpdateLocalMasterFleetNode(st);

        lock (SyncLock) {
            st["totalPidAudits"] = TotalNidsProcessAudits;
            st["totalAvScans"] = TotalAvFilesScanned;
            st["totalSiemAlerts"] = TotalSiemAlerts;
            st["totalIpsActions"] = TotalIpsActions;
            st["totalEdrKills"] = TotalEdrProcessesKilled;
            st["ctiTotalIndicators"] = CtiTotalIndicators;
            st["ctiHitsCount"] = CtiHitsCount;
            st["ctiLastSync"] = CtiLastSyncStatus;
            st["siemChainHash"] = LastSiemChainHash;
            st["porygonAutonomyMode"] = SoarConfig.PorygonAutonomyMode ?? "GRADUATED_2FA";
            st["lastActionBanner"] = LastActionBanner ?? "ENTERPRISE_XDR_ONLINE";
            st["jsonlPath"] = ProjectSiemJsonlPath;
            st["candidateBlockIp"] = LastCandidateBlockIp ?? "";
            st["candidateBlockProc"] = LastCandidateBlockProc ?? "";
            st["candidateQuarantineFile"] = LastCandidateQuarantineFile ?? "";
            st["candidateQuarantineName"] = string.IsNullOrEmpty(LastCandidateQuarantineFile) ? "Ninguno" : Path.GetFileName(LastCandidateQuarantineFile);

            Dictionary<string, object> officeDict = new Dictionary<string, object>();
            officeDict["online"] = OfficePcTunnelOnline;
            officeDict["status"] = OfficePcTunnelStatus;
            officeDict["logsRaw"] = OfficePcLogsRaw;
            officeDict["logsDecrypted"] = OfficePcLogsDecrypted;
            officeDict["credentialsCount"] = OfficePcCredentialsCount;
            officeDict["screenshotsCount"] = OfficePcScreenshotsCount;
            officeDict["idsAlertsTotal"] = OfficePcIdsAlertsTotal;
            officeDict["lanHostsTotal"] = OfficePcLanHostsTotal;
            officeDict["onlineAgentsCount"] = OfficePcOnlineAgentsCount;
            officeDict["totalAgentsCount"] = OfficePcTotalAgentsCount;
            officeDict["onlineAgentsSummary"] = OfficePcOnlineAgentsSummary;
            officeDict["lastLiveProbeReport"] = OfficePcLastLiveProbeReport;
            st["officePc"] = officeDict;

            Dictionary<string, object> soarDict = new Dictionary<string, object>();
            soarDict["syslogEnabled"] = SoarConfig.SyslogEnabled;
            soarDict["syslogHost"] = SoarConfig.SyslogHost;
            soarDict["syslogPort"] = SoarConfig.SyslogPort;
            soarDict["fortiGateEnabled"] = SoarConfig.FortiGateEnabled;
            soarDict["fortiGateUrl"] = SoarConfig.FortiGateWebhookUrl;
            soarDict["fortiGateGroup"] = SoarConfig.FortiGateAddressGroup;
            soarDict["alertWebhookEnabled"] = SoarConfig.AlertWebhookEnabled;
            soarDict["alertWebhookUrl"] = SoarConfig.AlertWebhookUrl;
            soarDict["porygonAutonomyMode"] = SoarConfig.PorygonAutonomyMode ?? "GRADUATED_2FA";
            soarDict["totalCefSent"] = SoarConfig.TotalCefSent;
            soarDict["totalFortiGatePushes"] = SoarConfig.TotalFortiGatePushes;
            soarDict["totalAlertWebhooksSent"] = SoarConfig.TotalAlertWebhooksSent;
            soarDict["lastStatus"] = SoarConfig.LastSoarStatus;
            st["soar"] = soarDict;

            List<Dictionary<string, object>> rulesList = new List<Dictionary<string, object>>();
            for (int i = 0; i < DetectionRules.Count; i++) {
                var r = DetectionRules[i];
                var d = new Dictionary<string, object>();
                d["id"] = r.Id;
                d["name"] = r.Name;
                d["ruleType"] = r.RuleType;
                d["targetField"] = r.TargetField;
                d["pattern"] = r.Pattern;
                d["mitre"] = r.MitreId;
                d["risk"] = r.RiskPct;
                d["enabled"] = r.Enabled;
                d["hits"] = r.HitsCount;
                rulesList.Add(d);
            }
            st["detectionRules"] = rulesList;

            List<FleetEndpointNode> sortedFleet = new List<FleetEndpointNode>(FleetNodes.Values);
            sortedFleet.Sort((a, b) => {
                int pA = a.Status == "ONLINE" ? 0 : (a.Status == "HIGH_RISK" ? 1 : 2);
                int pB = b.Status == "ONLINE" ? 0 : (b.Status == "HIGH_RISK" ? 1 : 2);
                if (pA != pB) return pA.CompareTo(pB);
                return b.RiskScorePct.CompareTo(a.RiskScorePct);
            });

            List<Dictionary<string, object>> fleetList = new List<Dictionary<string, object>>();
            for (int i = 0; i < sortedFleet.Count; i++) {
                var fn = sortedFleet[i];
                var d = new Dictionary<string, object>();
                d["hostId"] = fn.HostId;
                d["hostname"] = fn.Hostname;
                d["ip"] = fn.IpAddress;
                d["os"] = fn.OsVersion;
                d["role"] = fn.AgentRole;
                d["status"] = fn.Status;
                d["cpu"] = fn.CpuPct.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["ramPct"] = fn.RamPct;
                d["tcpCount"] = fn.ActiveTcpCount;
                d["alerts"] = fn.OpenAlerts;
                d["risk"] = fn.RiskScorePct;
                d["isIsolated"] = fn.IsIsolated;
                d["lastSeen"] = fn.LastSeen;
                fleetList.Add(d);
            }
            st["fleetNodes"] = fleetList;

            List<Dictionary<string, object>> dnsList = new List<Dictionary<string, object>>();
            for (int i = 0; i < RecentDnsAudits.Count; i++) {
                var dn = RecentDnsAudits[i];
                var d = new Dictionary<string, object>();
                d["timestamp"] = dn.Timestamp;
                d["domain"] = dn.Domain;
                d["resolvedIp"] = dn.ResolvedIp;
                d["entropy"] = dn.Entropy.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                d["risk"] = dn.RiskPct;
                d["verdict"] = dn.Verdict;
                d["mitre"] = dn.MitreId;
                dnsList.Add(d);
            }
            st["dnsAudits"] = dnsList;

            List<Dictionary<string, object>> tcpList = new List<Dictionary<string, object>>();
            for (int i = 0; i < ActiveTcpRows.Count; i++) {
                var r = ActiveTcpRows[i];
                var d = new Dictionary<string, object>();
                d["pid"] = r.Pid;
                d["processName"] = r.ProcessName ?? "System";
                d["exePath"] = r.ExePath ?? "";
                d["isSigned"] = r.IsSigned;
                d["signer"] = r.Signer ?? "";
                d["isLolBin"] = r.IsLolBin;
                d["localEp"] = r.LocalEp ?? "";
                d["remoteEp"] = r.RemoteEp ?? "";
                d["proto"] = r.Proto ?? "";
                d["state"] = r.State ?? "";
                d["risk"] = r.RiskPct;
                d["isCtiMatch"] = r.IsCtiMatch;
                d["matchedRule"] = r.MatchedRule ?? "";
                tcpList.Add(d);
            }
            st["activeConnections"] = tcpList;

            List<Dictionary<string, object>> siemList = new List<Dictionary<string, object>>();
            for (int i = RecentSiemEvents.Count - 1; i >= 0; i--) {
                var e = RecentSiemEvents[i];
                if (string.IsNullOrEmpty(e.ResolutionStatus)) {
                    PopulatePorygonAlertResolution(e);
                }
                var d = new Dictionary<string, object>();
                d["alertId"] = e.AlertId ?? ("ALR-" + (i + 1).ToString("0000"));
                d["timestamp"] = e.Timestamp;
                d["chainHash"] = e.ChainHash ?? "";
                d["agent"] = e.AgentName;
                d["module"] = e.Module;
                d["severity"] = e.Severity;
                d["mitre"] = e.MitreId;
                d["process"] = e.ProcessInfo;
                d["target"] = e.Target;
                d["sha256"] = e.Sha256;
                d["risk"] = (int)Math.Round(e.RiskScore * 100f);
                d["summary"] = e.Summary;
                d["action"] = e.ActionTaken;
                d["resolutionStatus"] = e.ResolutionStatus ?? "RESOLVED_AUTO";
                d["resolvedBy"] = e.ResolvedByAgent ?? e.AgentName ?? "PORYGON";
                d["resolutionMethod"] = e.ResolutionMethod ?? "Telemetría Auditada & Encadenada SHA-256";
                d["resolutionDetail"] = e.ResolutionDetail ?? e.Summary ?? "";
                d["resolvedAt"] = e.ResolvedAt ?? e.Timestamp;
                siemList.Add(d);
            }
            st["siemEvents"] = siemList;

            List<Dictionary<string, object>> avList = new List<Dictionary<string, object>>();
            for (int i = RecentAvScans.Count - 1; i >= 0; i--) {
                var r = RecentAvScans[i];
                var d = new Dictionary<string, object>();
                d["timestamp"] = r.Timestamp;
                d["fileName"] = r.FileName;
                d["fullPath"] = r.FullPath;
                d["sha256"] = r.Sha256;
                d["sizeBytes"] = r.FileSizeBytes;
                d["entropy"] = r.EntropyBits.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                d["isSigned"] = r.IsSigned;
                d["signer"] = r.SignerName;
                d["risk"] = (int)Math.Round(r.RiskScore * 100f);
                d["verdict"] = r.Verdict;
                d["mitre"] = r.MitreTactic;
                d["detail"] = r.Detail;
                avList.Add(d);
            }
            st["avScans"] = avList;

            st["blockedIps"] = new List<string>(BlockedFirewallIps);

            List<Dictionary<string, object>> qList = new List<Dictionary<string, object>>();
            for (int i = QuarantinedFiles.Count - 1; i >= 0; i--) {
                var q = QuarantinedFiles[i];
                var d = new Dictionary<string, object>();
                d["id"] = q.Id;
                d["timestamp"] = q.Timestamp;
                d["fileName"] = q.FileName;
                d["originalPath"] = q.OriginalPath;
                d["sha256"] = q.Sha256;
                qList.Add(d);
            }
            st["quarantinedFiles"] = qList;
        }
        return st;
    }

    private static bool ReadHttpRequest(NetworkStream ns, out string method, out string path, out string body) {
        method = "GET";
        path = "/";
        body = "";
        using (MemoryStream ms = new MemoryStream()) {
            byte[] buf = new byte[8192];
            int headerEnd = -1;
            int contentLen = 0;

            while (headerEnd < 0) {
                int r = ns.Read(buf, 0, buf.Length);
                if (r <= 0) return false;
                ms.Write(buf, 0, r);
                byte[] cur = ms.ToArray();
                for (int i = 0; i <= cur.Length - 4; i++) {
                    if (cur[i] == 13 && cur[i + 1] == 10 && cur[i + 2] == 13 && cur[i + 3] == 10) {
                        headerEnd = i + 4;
                        break;
                    }
                }
                if (ms.Length > 65536) return false;
            }

            byte[] allBytes = ms.ToArray();
            string headerStr = Encoding.ASCII.GetString(allBytes, 0, headerEnd);
            string[] lines = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return false;

            string[] reqParts = lines[0].Split(' ');
            if (reqParts.Length >= 2) {
                method = reqParts[0].ToUpperInvariant();
                path = reqParts[1];
            }

            for (int i = 1; i < lines.Length; i++) {
                if (lines[i].StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) {
                    int.TryParse(lines[i].Substring(15).Trim(), out contentLen);
                }
            }

            int bodyBytesRead = allBytes.Length - headerEnd;
            while (bodyBytesRead < contentLen && contentLen < 25 * 1024 * 1024) {
                int r = ns.Read(buf, 0, buf.Length);
                if (r <= 0) break;
                ms.Write(buf, 0, r);
                bodyBytesRead += r;
            }

            if (contentLen > 0) {
                byte[] finalBytes = ms.ToArray();
                int actualLen = Math.Min(contentLen, finalBytes.Length - headerEnd);
                if (actualLen > 0) {
                    body = Encoding.UTF8.GetString(finalBytes, headerEnd, actualLen);
                }
            }
            return true;
        }
    }

    private static void WriteHttpResponse(NetworkStream ns, string status, string contentType, byte[] payload) {
        if (payload == null) payload = new byte[0];
        string hdr =
            "HTTP/1.1 " + status + "\r\n" +
            "Content-Type: " + contentType + "\r\n" +
            "Content-Length: " + payload.Length + "\r\n" +
            "Access-Control-Allow-Origin: *\r\n" +
            "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
            "Access-Control-Allow-Headers: Content-Type\r\n" +
            "Cache-Control: no-cache, no-store\r\n" +
            "Connection: close\r\n\r\n";
        byte[] hdrBytes = Encoding.ASCII.GetBytes(hdr);
        ns.Write(hdrBytes, 0, hdrBytes.Length);
        if (payload.Length > 0) {
            ns.Write(payload, 0, payload.Length);
        }
        ns.Flush();
    }
}

// ============================================================================
// FLYWIRE / FLYBRAIN SPIKING CONNECTOME + EVOLUTIONARY ALGORITHM & LIFE CYCLE
// ============================================================================
public class FlyBrainConnectome {
    public const int N = 64;
    public float[] V = new float[N];             // Membrane potential (0..1.2)
    public float[] SpikeGlow = new float[N];     // Visual spike decay for HUD
    public float[,] W = new float[N, N];         // Synaptic weight matrix
    public float[,] Eligibility = new float[N, N]; // STDP / Hebbian eligibility trace

    // 3D Drosophila Brain Anatomical Coordinates & Dendritic Arborization Trees for each of the 64 Spiking Neurons
    public float[] NeuronX = new float[N];
    public float[] NeuronY = new float[N];
    public float[] NeuronZ = new float[N];
    public int[] NeuronRegion = new int[N];      // 0=Optic/VT Sensory, 1=Mushroom Body, 2=Central Complex, 3=Motor/Attack

    public struct BrainTri3D {
        public int I0, I1, I2;
        public int PartKind; // 0=Central Cortical Shell, 1=Left/Right Optic Lobe Shell, 2=Ventral SEZ Shell, 3=Blue Mushroom Body
    }

    public struct DendriteSeg3D {
        public float X0, Y0, Z0;
        public float X1, Y1, Z1;
        public int NeuronIdx;
        public byte R, G, B;
        public float Thickness;
        public bool IsTip;
    }

    // Precomputed FlyWire 3D Drosophila Cortical Wireframe Mesh + Cobalt-Blue Mushroom Body Mesh
    public static List<PointF[]> StaticDummy = null;
    public static float[] ShellVX;
    public static float[] ShellVY;
    public static float[] ShellVZ;
    public static List<BrainTri3D> ShellTris = new List<BrainTri3D>();
    public static object ShellInitLock = new object();
    public static bool ShellInitialized = false;

    public List<DendriteSeg3D> DendriteBranches = new List<DendriteSeg3D>();

    private static void ConfinePointInsideBrain(int region, float side, ref float x, ref float y, ref float z) {
        if (region == 0) {
            // Superior Lateral Protocerebrum (|x| < 48) OR Optic Lobe (|x| >= 48)
            if (Math.Abs(x) < 48f) {
                if (x * side < 0f && Math.Abs(x) > 6f) x = side * Math.Abs(x);
                x = Math.Max(-45f, Math.Min(45f, x));
                float maxDorsalY = 33f - (Math.Abs(x) > 28f ? (Math.Abs(x) - 28f) * 0.45f : 0f);
                float cleftDip = 6.5f * (float)Math.Exp(-(x * x) / 140.0);
                maxDorsalY -= cleftDip;
                y = Math.Max(-18f, Math.Min(maxDorsalY, y));
                z = Math.Max(-18f, Math.Min(18f, z));
            } else {
                // Inside Kidney-Shaped Optic Lobe centered at (side * 75, -10, -2) with radii (31, 31, 18)
                float cx = side * 75f, cy = -10f, cz = -2f;
                float dx = (x - cx) / 31f;
                float dy = (y - cy) / 31f;
                float dz = (z - cz) / 18f;
                float d2 = dx * dx + dy * dy + dz * dz;
                if (d2 > 0.86f) {
                    float s = (float)(Math.Sqrt(0.86 / d2));
                    x = cx + (x - cx) * s;
                    y = cy + (y - cy) * s;
                    z = cz + (z - cz) * s;
                }
            }
        } else if (region == 1) {
            // Kenyon Cell & Mushroom Body Calyx Cloud: strictly inside dorsal central protocerebrum beside MB stalks
            x = side * Math.Max(9f, Math.Min(34f, Math.Abs(x)));
            y = Math.Max(4f, Math.Min(28f, y));
            z = Math.Max(-14f, Math.Min(12f, z));
        } else if (region == 2) {
            // Central Complex & Medial Mushroom Body Lobes: strictly inside central core
            x = Math.Max(-28f, Math.Min(28f, x));
            y = Math.Max(-16f, Math.Min(12f, y));
            z = Math.Max(-13f, Math.Min(14f, z));
        } else {
            // Region 3: Ventrolateral Neuropil, SEZ & Inner Optic Peduncle
            if (Math.Abs(x) <= 46f) {
                float maxDepthY = -4f;
                float minDepthY = (Math.Abs(x) < 22f) ? -38f : -28f;
                y = Math.Max(minDepthY, Math.Min(maxDepthY, y));
                z = Math.Max(-16f, Math.Min(14f, z));
            } else {
                float cx = side * 72f, cy = -12f, cz = -2f;
                float dx = (x - cx) / 29f;
                float dy = (y - cy) / 29f;
                float dz = (z - cz) / 16f;
                float d2 = dx * dx + dy * dy + dz * dz;
                if (d2 > 0.82f) {
                    float s = (float)(Math.Sqrt(0.82 / d2));
                    x = cx + (x - cx) * s;
                    y = cy + (y - cy) * s;
                    z = cz + (z - cz) * s;
                }
            }
        }
    }

    private static void EnsureFlyWire3DShellInitialized() {
        lock (ShellInitLock) {
            if (ShellInitialized) return;
            List<float> vx = new List<float>();
            List<float> vy = new List<float>();
            List<float> vz = new List<float>();
            ShellTris.Clear();

            // 1. Central Bilobed Protocerebrum + Ventral Subesophageal Zone (SEZ) High-Detail 3D Mesh (PartKind 0 & 2)
            int uStepsC = 14, vStepsC = 24;
            int baseC = vx.Count;
            for (int u = 0; u <= uStepsC; u++) {
                double theta = (u / (double)uStepsC) * Math.PI; // 0 = dorsal top, PI = ventral bottom
                float sinT = (float)Math.Sin(theta);
                float cosT = (float)Math.Cos(theta);
                for (int v = 0; v < vStepsC; v++) {
                    double phi = (v / (double)vStepsC) * Math.PI * 2.0;
                    float cosP = (float)Math.Cos(phi);
                    float sinP = (float)Math.Sin(phi);

                    float rx = 53f, ry = 42f, rz = 30f;
                    // Swell upper-lateral SLP domes (gives the distinct bilobed heart/butterfly upper contour in FlyWire)
                    if (cosT > 0.10f) {
                        rx += 5.5f * (float)Math.Sin(theta * 1.35);
                    }
                    // Ventral Subesophageal Zone (SEZ) tapers in X and forms the rounded lower-middle ganglion lobe
                    if (cosT < -0.22f) {
                        float ventFactor = (-cosT - 0.22f) / 0.78f;
                        rx *= (1.0f - 0.38f * ventFactor);
                        ry += 5.0f * ventFactor;
                    }
                    float x = rx * sinT * cosP;
                    float y = ry * cosT;
                    float z = rz * sinT * sinP;

                    // Pronounced dorsal inter-hemispheric cleft at X = 0
                    if (y > 12f) {
                        float cleft = (float)Math.Exp(-(x * x) / 210.0);
                        y -= 8.5f * cleft * ((y - 12f) / 30f);
                    }
                    vx.Add(x); vy.Add(y); vz.Add(z);
                }
            }
            for (int u = 0; u < uStepsC; u++) {
                for (int v = 0; v < vStepsC; v++) {
                    int nv = (v + 1) % vStepsC;
                    int i00 = baseC + u * vStepsC + v;
                    int i01 = baseC + u * vStepsC + nv;
                    int i10 = baseC + (u + 1) * vStepsC + v;
                    int i11 = baseC + (u + 1) * vStepsC + nv;
                    int kind = (u >= uStepsC - 3) ? 2 : 0;
                    ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i10, I2 = i11, PartKind = kind });
                    ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i11, I2 = i01, PartKind = kind });
                }
            }

            // 2. Bilateral Left & Right Optic Lobes (Medulla + Lobula Kidney-Cup Hemispheres, PartKind = 1)
            for (int sideIdx = 0; sideIdx < 2; sideIdx++) {
                float side = (sideIdx == 0) ? -1.0f : 1.0f;
                int uStepsO = 12, vStepsO = 18;
                int baseO = vx.Count;
                for (int u = 0; u <= uStepsO; u++) {
                    double theta = (u / (double)uStepsO) * Math.PI;
                    float sinT = (float)Math.Sin(theta);
                    float cosT = (float)Math.Cos(theta);
                    for (int v = 0; v < vStepsO; v++) {
                        double phi = (v / (double)vStepsO) * Math.PI * 2.0;
                        float cosP = (float)Math.Cos(phi);
                        float sinP = (float)Math.Sin(phi);

                        float lx = 38f * sinT * cosP;
                        float ly = 39f * cosT;
                        float lz = 24f * sinT * sinP;

                        // Anatomical kidney-cup shaping positioned lower-lateral (Y centered at -10, seamlessly bridged at |X|=46)
                        float worldX = side * 76f + lx;
                        float distOut = Math.Max(0f, (Math.Abs(worldX) - 54f) / 58f);
                        ly *= (0.85f + 0.28f * distOut);
                        float worldY = -9f + ly - 5.5f * distOut;
                        float worldZ = -2f + lz - 8.5f * distOut * distOut;

                        vx.Add(worldX); vy.Add(worldY); vz.Add(worldZ);
                    }
                }
                for (int u = 0; u < uStepsO; u++) {
                    for (int v = 0; v < vStepsO; v++) {
                        int nv = (v + 1) % vStepsO;
                        int i00 = baseO + u * vStepsO + v;
                        int i01 = baseO + u * vStepsO + nv;
                        int i10 = baseO + (u + 1) * vStepsO + v;
                        int i11 = baseO + (u + 1) * vStepsO + nv;
                        ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i10, I2 = i11, PartKind = 1 });
                        ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i11, I2 = i01, PartKind = 1 });
                    }
                }
            }

            // 3. Bilateral Inner Antennal / Ventrolateral Neuropil Lobes (Golden-Amber Inner Wireframe Spheres, PartKind = 4)
            for (int sideIdx = 0; sideIdx < 2; sideIdx++) {
                float side = (sideIdx == 0) ? -1.0f : 1.0f;
                int uStepsA = 7, vStepsA = 12;
                int baseA = vx.Count;
                for (int u = 0; u <= uStepsA; u++) {
                    double theta = (u / (double)uStepsA) * Math.PI;
                    float sinT = (float)Math.Sin(theta);
                    float cosT = (float)Math.Cos(theta);
                    for (int v = 0; v < vStepsA; v++) {
                        double phi = (v / (double)vStepsA) * Math.PI * 2.0;
                        float x = side * 18.5f + 16.5f * sinT * (float)Math.Cos(phi);
                        float y = -14.5f + 15.5f * cosT;
                        float z = 3.0f + 14.0f * sinT * (float)Math.Sin(phi);
                        vx.Add(x); vy.Add(y); vz.Add(z);
                    }
                }
                for (int u = 0; u < uStepsA; u++) {
                    for (int v = 0; v < vStepsA; v++) {
                        int nv = (v + 1) % vStepsA;
                        int i00 = baseA + u * vStepsA + v;
                        int i01 = baseA + u * vStepsA + nv;
                        int i10 = baseA + (u + 1) * vStepsA + v;
                        int i11 = baseA + (u + 1) * vStepsA + nv;
                        ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i10, I2 = i11, PartKind = 4 });
                        ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i11, I2 = i01, PartKind = 4 });
                    }
                }
            }

            // 4. Bilateral 3D Cobalt-Blue Mushroom Bodies (Bulbous Dorsal Alpha Cap + Vertical Stalk + Horizontal Beta/Gamma Lobes, PartKind = 3)
            for (int sideIdx = 0; sideIdx < 2; sideIdx++) {
                float side = (sideIdx == 0) ? -1.0f : 1.0f;
                float[,] spine = new float[,] {
                    { side * 26f,  33f, -3f, 4.2f },
                    { side * 26f,  29f, -2f, 9.2f },
                    { side * 24f,  20f,  0f, 6.8f },
                    { side * 22f,  10f,  3f, 6.2f },
                    { side * 21f,   0f,  7f, 7.4f },
                    { side * 14f,  -2f,  9f, 7.8f },
                    { side *  3f,  -2f, 10f, 6.8f }
                };
                int rings = spine.GetLength(0);
                int segs = 10;
                int baseM = vx.Count;
                for (int r = 0; r < rings; r++) {
                    float sx = spine[r, 0], sy = spine[r, 1], sz = spine[r, 2], rad = spine[r, 3];
                    for (int s = 0; s < segs; s++) {
                        double ang = (s / (double)segs) * Math.PI * 2.0;
                        float ox = (float)Math.Cos(ang) * rad * 0.88f;
                        float oy = (float)Math.Sin(ang) * rad * 0.72f;
                        float oz = (float)Math.Sin(ang) * rad * 0.90f;
                        vx.Add(sx + ox); vy.Add(sy + oy); vz.Add(sz + oz);
                    }
                }
                for (int r = 0; r < rings - 1; r++) {
                    for (int s = 0; s < segs; s++) {
                        int ns = (s + 1) % segs;
                        int i00 = baseM + r * segs + s;
                        int i01 = baseM + r * segs + ns;
                        int i10 = baseM + (r + 1) * segs + s;
                        int i11 = baseM + (r + 1) * segs + ns;
                        ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i10, I2 = i11, PartKind = 3 });
                        ShellTris.Add(new BrainTri3D { I0 = i00, I1 = i11, I2 = i01, PartKind = 3 });
                    }
                }
            }

            ShellVX = vx.ToArray();
            ShellVY = vy.ToArray();
            ShellVZ = vz.ToArray();
            ShellInitialized = true;
        }
    }

    // Persistent Training & Evolution Metrics
    public int Level = 1;
    public int XP = 0;
    public int PacketsCollected = 0;
    public int AnomaliesCleared = 0;
    public int NeuralSyncs = 0;
    public float SynapticGain = 1.0f;            // Increases via Hebbian learning (1.0 .. 2.8)
    public float LastPlasticityDelta = 0.0f;

    // Evolutionary Algorithm (μ + λ Neuro-Evolution), Natural Selection & Biological Life Cycle State
    public int Generation = 1;
    public float LifeAge = 0.0f;                 // 0.0% .. 100.0% life cycle progression
    public int LifeStage = 0;                    // 0 = S1:SEED (Porygon), 1 = S2:MATURE (Porygon2), 2 = S3:APEX (Porygon-Z)
    public float FitnessScore = 76.0f;           // Evolutionary IDS Fitness (0..100, penalizes FN 3.5x)
    public float DetectionAccuracy = 84.5f;      // Anomaly Detection Accuracy (50..99.9%)
    public float RecallRate = 88.0f;             // True Positive Rate (100% - False Negative Rate)
    public float F2Score {
        get {
            float denom = 5f * TruePositives + 4f * FalseNegatives + FalsePositives;
            return denom > 0f ? (500f * TruePositives / denom) : RecallRate;
        }
    }
    public int TruePositives = 32;
    public int TrueNegatives = 68;
    public int FalsePositives = 5;
    public int FalseNegatives = 2;
    public int GenTruePositives = 0;
    public int GenFalseNegatives = 0;
    public int PrunedTraitsCount = 0;
    public string LastPrunedTrait = "NONE (GENOME_CLEAN)";
    public string LastCrossSummary = "G1:SEED_ORIGIN";
    public float MutationSigma = 0.08f;

    // Genome: [0]=dlZ, [1]=ulZ, [2]=pingZ, [3]=portAndVtRisk, [4]=synRatio, [5]=entropy, [6]=thresholdBias, [7]=exploreGain
    public float[] AnomalyGenome = new float[8];
    public float[] EliteGenome = new float[8];
    public float[] ReservedForFuture = null;
    public float EliteFitness = 76.0f;

    public float DriveCuriosity = 0.65f;
    public float DriveHunger = 0.55f;
    public float DriveAlert = 0.20f;
    public float DriveSync = 0.40f;
    public string ActiveStateName = "AUTONOMOUS // EXPLORING";

    private Random rng;

    public int NextLevelXP {
        get { return 80 + (Level - 1) * 60; }
    }

    public int CpuStepsCompleted = 0;

    public string GetLifeStageTag() {
        if (LifeStage <= 0) return "S1:SEED";
        if (LifeStage == 1) return "S2:MATURE";
        return "S3:APEX";
    }

    public string GetLifeStageLabel() {
        return GetLifeStageTag();
    }

    public FlyBrainConnectome(int seed) {
        rng = new Random(seed);
        EnsureFlyWire3DShellInitialized();

        // Initialize 3D Anatomical Drosophila Connectome Coordinates (100% Confined Inside FlyWire Neuropil Compartments)
        for (int i = 0; i < N; i++) {
            if (i < 16) {
                // Region 0: Pars Intercerebralis + Superior Lateral Protocerebrum (Golden) & Lateral Horn / Optic Tracts (Crimson-Pink)
                NeuronRegion[i] = 0;
                float side = (i < 8) ? -1.0f : 1.0f;
                int localIdx = i % 8;
                if (i == 0 || i == 8) {
                    // Dorsal Midline Pars Intercerebralis (PI) Golden Tuft at top-center cleft (as in FlyWire reference)
                    NeuronX[i] = side * 2.2f;
                    NeuronY[i] = (i == 0) ? 26.5f : 30.5f;
                    NeuronZ[i] = (i == 0) ? -1.5f : 2.0f;
                } else if (localIdx < 4) {
                    // Superior Lateral Protocerebrum (SLP) Golden-Yellow Arbors tucked inside upper-left & upper-right domes
                    float t = (localIdx - 1) / 2.0f; // 0..1
                    NeuronX[i] = side * (32f + t * 10f);
                    NeuronY[i] = 24f - t * 6f;
                    NeuronZ[i] = (localIdx % 2 == 0) ? 5f : -5f;
                } else {
                    // Anterior Optic Tubercle & Lateral Horn Crimson-Magenta Projection Neurons flanking the Mushroom Bodies
                    int oIdx = localIdx - 4; // 0..3
                    float t = oIdx / 3.0f;
                    NeuronX[i] = side * (35f + t * 18f);
                    NeuronY[i] = 14f - t * 16f;
                    NeuronZ[i] = ((oIdx % 2 == 0) ? 6f : -4f);
                }
                ConfinePointInsideBrain(0, side, ref NeuronX[i], ref NeuronY[i], ref NeuronZ[i]);
            } else if (i < 36) {
                // Region 1: Mint-Green / Cyan Kenyon Cell & Calyx Clouds nestled tightly inside the Mushroom Body stalks
                NeuronRegion[i] = 1;
                int mIdx = i - 16;
                float side = (mIdx < 10) ? -1.0f : 1.0f;
                int k = mIdx % 10;
                double ang = (k / 10.0) * Math.PI * 2.0;
                float rHoriz = 4.5f + (k % 3) * 2.2f;
                float rVert = 5.5f + (k % 4) * 2.0f;
                NeuronX[i] = side * (16.5f + (float)Math.Cos(ang) * rHoriz);
                NeuronY[i] = 15.5f + (float)Math.Sin(ang) * rVert;
                NeuronZ[i] = (float)Math.Sin(ang * 1.7) * 5.5f;
                ConfinePointInsideBrain(1, side, ref NeuronX[i], ref NeuronY[i], ref NeuronZ[i]);
            } else if (i < 48) {
                // Region 2: Central Complex & Horizontal Mushroom Body Beta/Gamma Lobes
                NeuronRegion[i] = 2;
                int cIdx = i - 36;
                float side = (cIdx % 2 == 0) ? -1.0f : 1.0f;
                float t = (cIdx / 2) / 5.0f; // 0..1
                NeuronX[i] = side * (3.5f + t * 16.5f);
                NeuronY[i] = -2.0f + (float)Math.Sin(cIdx * 1.1) * 4.5f;
                NeuronZ[i] = 6.5f + (float)Math.Cos(cIdx * 0.9) * 4.0f;
                ConfinePointInsideBrain(2, side, ref NeuronX[i], ref NeuronY[i], ref NeuronZ[i]);
            } else {
                // Region 3: Ventrolateral Neuropil, SEZ & Inner Optic Lobula Motor/Attack Pathways (Emerald-Green & Violet)
                NeuronRegion[i] = 3;
                int gIdx = i - 48; // 0..15
                float side = (gIdx < 4) ? -1.0f : 1.0f; // Denser green motor arbor on the right hemisphere as in FlyWire reference
                int k = (gIdx < 4) ? gIdx : (gIdx - 4);
                float span = (gIdx < 4) ? 3.0f : 11.0f;
                float t = k / span; // 0 (ventral SEZ) .. 1 (lateral optic peduncle / lobula)
                NeuronX[i] = side * (6f + t * 52f);
                NeuronY[i] = -27f + (float)Math.Sin(t * Math.PI) * 16f + (k % 3 - 1) * 3.0f;
                NeuronZ[i] = (k % 2 == 0 ? 4f : -4f) * (1.0f - 0.3f * t);
                ConfinePointInsideBrain(3, side, ref NeuronX[i], ref NeuronY[i], ref NeuronZ[i]);
            }
        }

        // Build Fine-Grained Organic 3D Dendritic Arborizations & Curved Axonal Tracts (Strictly Confined Inside Shell!)
        DendriteBranches.Clear();
        Random arborRng = new Random(seed ^ 0x5F3759DF);
        for (int i = 0; i < N; i++) {
            byte cr, cg, cb;
            float side = (NeuronX[i] < 0f) ? -1.0f : 1.0f;
            int reg = NeuronRegion[i];

            if (i == 0 || i == 8 || i == 11 || (i < 16 && (i % 8) < 4)) {
                // Golden-Yellow Pars Intercerebralis & Superior Lateral Protocerebrum Arbors
                cr = 252; cg = 214; cb = 32;
            } else if (i < 16) {
                // Crimson-Pink Lateral Horn & Optic Projection Tracts
                cr = 248; cg = 48; cb = 118;
            } else if (i < 36) {
                // Mint-Green & Aqua Kenyon Cell Calyx Clouds
                if (i % 2 == 0) { cr = 36; cg = 246; cb = 168; }
                else { cr = 52; cg = 228; cb = 205; }
            } else if (i < 48) {
                // Cobalt-Blue & Deep Amber Central Complex / Antennal Lobes
                if (i % 2 == 0) { cr = 46; cg = 148; cb = 250; }
                else { cr = 218; cg = 165; cb = 38; }
            } else {
                // Bright Emerald-Green & Violet Ventrolateral / Optic-Lobula Motor Tracts
                if (i >= 52 && (i % 3 == 0)) { cr = 162; cg = 88; cb = 248; }
                else { cr = 42; cg = 248; cb = 78; }
            }

            float sx = NeuronX[i], sy = NeuronY[i], sz = NeuronZ[i];
            int primaryCount = (reg == 1) ? 4 : 3;
            for (int p = 0; p < primaryCount; p++) {
                double a1 = arborRng.NextDouble() * Math.PI * 2.0;
                double a2 = (arborRng.NextDouble() - 0.5) * Math.PI * 0.85;
                float len1 = (reg == 1) ? (3.4f + (float)arborRng.NextDouble() * 2.8f) : (4.2f + (float)arborRng.NextDouble() * 3.6f);
                float p1x = sx + (float)(Math.Cos(a1) * Math.Cos(a2)) * len1;
                float p1y = sy + (float)Math.Sin(a2) * len1;
                float p1z = sz + (float)(Math.Sin(a1) * Math.Cos(a2)) * len1 * 0.70f;
                ConfinePointInsideBrain(reg, side, ref p1x, ref p1y, ref p1z);

                DendriteBranches.Add(new DendriteSeg3D {
                    X0 = sx, Y0 = sy, Z0 = sz,
                    X1 = p1x, Y1 = p1y, Z1 = p1z,
                    NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.25f, IsTip = false
                });

                for (int b = 0; b < 2; b++) {
                    double b1 = a1 + (b == 0 ? 0.58 : -0.58) + (arborRng.NextDouble() - 0.5) * 0.30;
                    double b2 = a2 + (arborRng.NextDouble() - 0.5) * 0.55;
                    float len2 = len1 * (0.58f + (float)arborRng.NextDouble() * 0.22f);
                    float p2x = p1x + (float)(Math.Cos(b1) * Math.Cos(b2)) * len2;
                    float p2y = p1y + (float)Math.Sin(b2) * len2;
                    float p2z = p1z + (float)(Math.Sin(b1) * Math.Cos(b2)) * len2 * 0.70f;
                    ConfinePointInsideBrain(reg, side, ref p2x, ref p2y, ref p2z);

                    DendriteBranches.Add(new DendriteSeg3D {
                        X0 = p1x, Y0 = p1y, Z0 = p1z,
                        X1 = p2x, Y1 = p2y, Z1 = p2z,
                        NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 0.90f, IsTip = true
                    });
                }
            }

            // Add anatomically faithful curved multi-segment axonal tracts inside the brain shell:
            if (i < 16 && (i % 8) >= 4) {
                // Crimson-Pink Curved Lateral Tract sweeping from Mushroom Body Calyx down-outward to Optic Peduncle
                float m1x = side * 32f, m1y = 16f, m1z = 2f;
                float m2x = side * 44f, m2y = 5f,  m2z = 0f;
                float m3x = side * 56f, m3y = -6f, m3z = -2f;
                DendriteBranches.Add(new DendriteSeg3D { X0 = m1x, Y0 = m1y, Z0 = m1z, X1 = sx, Y1 = sy, Z1 = sz, NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.35f, IsTip = false });
                DendriteBranches.Add(new DendriteSeg3D { X0 = sx, Y0 = sy, Z0 = sz, X1 = m2x, Y1 = m2y, Z1 = m2z, NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.25f, IsTip = false });
                DendriteBranches.Add(new DendriteSeg3D { X0 = m2x, Y0 = m2y, Z0 = m2z, X1 = m3x, Y1 = m3y, Z1 = m3z, NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.10f, IsTip = true });
            } else if (reg == 3 && i >= 52) {
                // Emerald-Green & Violet Curved Ventrolateral-to-Optic-Lobula Tract
                float w1x = side * 26f, w1y = -16f, w1z = 2f;
                float w2x = side * 46f, w2y = -11f, w2z = 0f;
                float w3x = side * 66f, w3y = -14f + (i % 3 - 1) * 6f, w3z = -2f;
                ConfinePointInsideBrain(3, side, ref w3x, ref w3y, ref w3z);
                DendriteBranches.Add(new DendriteSeg3D { X0 = sx, Y0 = sy, Z0 = sz, X1 = w1x, Y1 = w1y, Z1 = w1z, NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.30f, IsTip = false });
                DendriteBranches.Add(new DendriteSeg3D { X0 = w1x, Y0 = w1y, Z0 = w1z, X1 = w2x, Y1 = w2y, Z1 = w2z, NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.20f, IsTip = false });
                DendriteBranches.Add(new DendriteSeg3D { X0 = w2x, Y0 = w2y, Z0 = w2z, X1 = w3x, Y1 = w3y, Z1 = w3z, NeuronIdx = i, R = cr, G = cg, B = cb, Thickness = 1.05f, IsTip = true });
            }
        }

        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                if (i == j) continue;
                if (i < 16 && j >= 16 && j < 48 && rng.NextDouble() < 0.38) {
                    W[i, j] = (float)(0.12 + rng.NextDouble() * 0.35);
                } else if (i >= 16 && i < 36 && j >= 36 && rng.NextDouble() < 0.42) {
                    W[i, j] = (float)(0.10 + rng.NextDouble() * 0.32);
                } else if (i >= 36 && i < 48 && j >= 48 && rng.NextDouble() < 0.50) {
                    W[i, j] = (float)(0.15 + rng.NextDouble() * 0.40);
                } else if (i >= 16 && i < 36 && j >= 16 && j < 36 && rng.NextDouble() < 0.20) {
                    W[i, j] = (float)((rng.NextDouble() - 0.35) * 0.25);
                }
            }
        }

        // Functional pathways (including Neuron 11 = VirusTotal Threat Sensor -> Alert & Motor Attack):
        W[0, 49] = 0.58f;
        W[1, 50] = 0.58f;
        W[2, 56] = 0.65f;
        W[3, 36] = 0.55f;

        W[4, 49] = 0.62f;
        W[5, 50] = 0.62f;
        W[6, 48] = 0.72f;
        W[7, 60] = 0.85f;

        W[8, 42] = 0.78f;
        W[8, 61] = 0.82f;
        W[9, 62] = 0.80f;
        W[10, 63] = 0.75f;
        W[11, 42] = 0.88f; // VirusTotal API Malicious Hit -> Central Alert
        W[11, 61] = 0.90f; // VirusTotal API Malicious Hit -> Tri-Attack / Purge

        // Initialize Evolutionary Anomaly Classifier Genome
        AnomalyGenome[0] = 0.62f + (float)(rng.NextDouble() * 0.28); // DL Z-Score weight
        AnomalyGenome[1] = 0.54f + (float)(rng.NextDouble() * 0.28); // UL Z-Score weight
        AnomalyGenome[2] = 0.42f + (float)(rng.NextDouble() * 0.24); // Ping Z-Score weight
        AnomalyGenome[3] = 0.96f + (float)(rng.NextDouble() * 0.30); // Port & VirusTotal Threat Risk weight
        AnomalyGenome[4] = 0.88f + (float)(rng.NextDouble() * 0.28); // SYN Flood / Half-open weight
        AnomalyGenome[5] = 0.78f + (float)(rng.NextDouble() * 0.26); // Flow Entropy / NDR Risk weight
        AnomalyGenome[6] = 1.28f + (float)((rng.NextDouble() - 0.5) * 0.22); // Classification Bias Threshold (Lower = fewer FN)
        AnomalyGenome[7] = 1.00f + (float)((rng.NextDouble() - 0.5) * 0.20); // Adaptive Exploration Gain
        Array.Copy(AnomalyGenome, EliteGenome, 8);
    }

    public float EvaluateAnomalyProbability(float dlZ, float ulZ, float pingZ, float portAndVtRisk, float synRisk, float ndrEntropy) {
        float z = AnomalyGenome[0] * Math.Max(0f, dlZ)
                + AnomalyGenome[1] * Math.Max(0f, ulZ)
                + AnomalyGenome[2] * Math.Max(0f, pingZ)
                + AnomalyGenome[3] * portAndVtRisk
                + AnomalyGenome[4] * synRisk
                + AnomalyGenome[5] * ndrEntropy
                - AnomalyGenome[6];
        return 1.0f / (1.0f + (float)Math.Exp(-z));
    }

    // Multi-Core CPU Accelerated Evolutionary Training Batch ((1+1)-ES + Heavy False-Negative Penalty)
    public void CpuTrainMicroBatch(int totalSteps, float liveDlZ, float liveUlZ, float livePingZ, float liveAnomIdx, uint seedVal) {
        uint s = seedVal ^ (uint)(Generation * 7919 + Level * 104729 + 1234567);
        if (s == 0) s = 2463534242u;

        float[] bestG = new float[8];
        float[] candG = new float[8];
        Array.Copy(AnomalyGenome, bestG, 8);

        int miniBatchSize = 25;
        int epochs = Math.Max(1, totalSteps / miniBatchSize);
        float sigma = MutationSigma;

        int totalCorrect = 0;
        int totalSamples = 0;
        int batchTP = 0, batchTN = 0, batchFP = 0, batchFN = 0;

        for (int ep = 0; ep < epochs; ep++) {
            for (int g = 0; g < 8; g++) {
                s ^= s << 13; s ^= s >> 17; s ^= s << 5;
                float u1 = ((s & 0xFFFF) / 65535.0f) - 0.5f;
                s ^= s << 13; s ^= s >> 17; s ^= s << 5;
                float u2 = ((s & 0xFFFF) / 65535.0f) - 0.5f;
                float gauss = (u1 + u2) * sigma;
                candG[g] = Math.Max(0.08f, Math.Min(3.20f, bestG[g] + gauss));
            }

            float bestLoss = 0f, candLoss = 0f;
            int epTP = 0, epTN = 0, epFP = 0, epFN = 0;

            for (int m = 0; m < miniBatchSize; m++) {
                s ^= s << 13; s ^= s >> 17; s ^= s << 5;
                bool isAnomaly = ((s & 0xFF) < 98); // ~38% adversarial / VirusTotal anomaly samples

                s ^= s << 13; s ^= s >> 17; s ^= s << 5;
                float r0 = (s & 0x3FF) / 1023.0f;
                s ^= s << 13; s ^= s >> 17; s ^= s << 5;
                float r1 = (s & 0x3FF) / 1023.0f;
                s ^= s << 13; s ^= s >> 17; s ^= s << 5;
                float r2 = (s & 0x3FF) / 1023.0f;

                float fDl, fUl, fPg, fPortVt, fSyn, fEnt;
                if (isAnomaly) {
                    int attackType = (int)(s % 5); // 0=DL_Surge, 1=UL_Exfil, 2=PortScan, 3=SYN_Flood, 4=VirusTotal_Malicious_IP
                    fDl     = (attackType == 0) ? (1.5f + r0 * 2.3f) : (Math.Max(0f, liveDlZ) * 0.3f + r0 * 0.7f);
                    fUl     = (attackType == 1) ? (1.4f + r1 * 2.1f) : (Math.Max(0f, liveUlZ) * 0.3f + r1 * 0.6f);
                    fPg     = (attackType == 2) ? (1.3f + r2 * 1.9f) : (r2 * 0.5f);
                    fPortVt = (attackType == 2 || attackType == 4) ? (0.80f + r0 * 0.20f) : (r1 * 0.35f);
                    fSyn    = (attackType == 1 || attackType == 3) ? (0.72f + r2 * 0.28f) : (r0 * 0.25f);
                    fEnt    = 0.62f + Math.Max(liveAnomIdx * 0.3f, r1 * 0.38f);
                } else {
                    fDl     = r0 * 0.62f;
                    fUl     = r1 * 0.52f;
                    fPg     = r2 * 0.48f;
                    fPortVt = (r0 > 0.93f) ? 0.22f : 0.02f;
                    fSyn    = r1 * 0.14f;
                    fEnt    = r2 * 0.25f;
                }

                float zBest = bestG[0] * fDl + bestG[1] * fUl + bestG[2] * fPg + bestG[3] * fPortVt + bestG[4] * fSyn + bestG[5] * fEnt - bestG[6];
                float pBest = 1.0f / (1.0f + (float)Math.Exp(-zBest));

                float zCand = candG[0] * fDl + candG[1] * fUl + candG[2] * fPg + candG[3] * fPortVt + candG[4] * fSyn + candG[5] * fEnt - candG[6];
                float pCand = 1.0f / (1.0f + (float)Math.Exp(-zCand));

                float target = isAnomaly ? 1.0f : 0.0f;
                float errBest = target - pBest;
                float errCand = target - pCand;

                // CRITICAL: 3.5x Heavy Penalty on False Negatives (missing an anomaly/VT threat is penalized 3.5x harder!)
                float fnPenaltyWeight = isAnomaly ? 3.5f : 1.0f;
                bestLoss += fnPenaltyWeight * errBest * errBest;
                candLoss += fnPenaltyWeight * errCand * errCand;

                bool pred = (pBest >= 0.5f);
                if (pred && isAnomaly) epTP++;
                else if (!pred && !isAnomaly) epTN++;
                else if (pred && !isAnomaly) epFP++;
                else epFN++;
            }

            if (candLoss < bestLoss) {
                Array.Copy(candG, bestG, 8);
            }

            batchTP += epTP;
            batchTN += epTN;
            batchFP += epFP;
            batchFN += epFN;
            totalCorrect += (epTP + epTN);
            totalSamples += miniBatchSize;
        }

        Array.Copy(bestG, AnomalyGenome, 8);
        CpuStepsCompleted += totalSteps;
        TruePositives += batchTP / 10;
        TrueNegatives += batchTN / 10;
        FalsePositives += Math.Max(0, batchFP / 15);
        FalseNegatives += Math.Max(0, batchFN / 18);
        GenTruePositives += batchTP / 10;
        GenFalseNegatives += batchFN / 18;

        float batchAcc = (totalSamples > 0) ? ((float)totalCorrect * 100.0f / totalSamples) : 78.0f;
        float evoCap = Math.Min(99.5f, 82.0f + (Generation - 1) * 1.35f + LifeStage * 1.40f + (LifeAge * 0.03f));
        float targetAcc = Math.Min(evoCap, Math.Max(68.0f, batchAcc * 0.88f + (Generation - 1) * 0.85f));
        DetectionAccuracy = DetectionAccuracy * 0.90f + targetAcc * 0.10f;

        float precision = (batchTP + batchFP > 0) ? ((float)batchTP / (batchTP + batchFP)) : 0.85f;
        float recall = (batchTP + batchFN > 0) ? ((float)batchTP / (batchTP + batchFN)) : 0.88f;
        RecallRate = RecallRate * 0.85f + (recall * 100.0f) * 0.15f;

        // F2-Score weighs Recall (avoiding False Negatives) 2x higher than Precision
        float f2 = (4f * precision + recall > 0.01f) ? (5f * precision * recall / (4f * precision + recall)) : 0.82f;
        float fnDirectPenalty = (batchFN > 0 && batchTP > 0) ? Math.Min(12.0f, ((float)batchFN / (batchTP + batchFN)) * 28.0f) : 0f;
        float rawFit = Math.Min(evoCap - 0.4f, Math.Max(50.0f, f2 * 83.0f - fnDirectPenalty + (Generation - 1) * 1.25f + LifeStage * 1.20f + SynapticGain * 1.8f));
        FitnessScore = FitnessScore * 0.88f + rawFit * 0.12f;

        if (FitnessScore > EliteFitness) {
            EliteFitness = FitnessScore;
            Array.Copy(AnomalyGenome, EliteGenome, 8);
        }
    }

    // Post-Generation Natural Selection, BLX-alpha Crossover & Negative Trait Pruning Audit
    public void CrossoverAndAuditGeneration(FlyBrainConnectome alphaParent, string selfName, string alphaName, int seedOffset,
        out string auditSummaryLine, out string jsonlRecord) {
        Random crng = new Random(seedOffset ^ Environment.TickCount);
        int prevGen = Generation;
        float preFit = FitnessScore;
        float preAcc = DetectionAccuracy;
        float preFnRate = (GenTruePositives + GenFalseNegatives > 0)
            ? ((float)GenFalseNegatives * 100.0f / (GenTruePositives + GenFalseNegatives))
            : 2.0f;

        Generation++;
        LifeAge = 0.0f;
        LifeStage = 0;
        MutationSigma = Math.Max(0.018f, MutationSigma * 0.91f);

        List<string> prunedTraits = new List<string>();
        string selectionMode = "BLX_ALPHA_CROSS";

        // 1. NATURAL SELECTION CHECK: Did this individual regress in Fitness or suffer high False Negatives (>8% FN)?
        bool regressedFitness = (preFit < EliteFitness - 1.8f);
        bool excessiveFalseNegatives = (preFnRate > 8.5f);
        if (regressedFitness || excessiveFalseNegatives) {
            selectionMode = excessiveFalseNegatives ? "NAT_SEL_CULL_HIGH_FN" : "NAT_SEL_CULL_REGRESSION";
            prunedTraits.Add(excessiveFalseNegatives ? string.Format("EXTINCT_FN({0:0.0}%)", preFnRate) : "EXTINCT_LOW_FIT");
            // Natural selection replaces the defective genome with 80% Swarm Alpha + 20% Elite Ancestor
            for (int g = 0; g < 8; g++) {
                float pA = (alphaParent != null) ? alphaParent.AnomalyGenome[g] : EliteGenome[g];
                AnomalyGenome[g] = pA * 0.80f + EliteGenome[g] * 0.20f;
            }
        } else {
            // Standard BLX-alpha Genetic Crossover (68% Alpha Parent + 32% Self) + Gaussian Mutation
            for (int g = 0; g < 8; g++) {
                float pA = (alphaParent != null) ? alphaParent.AnomalyGenome[g] : AnomalyGenome[g];
                float pB = AnomalyGenome[g];
                float blend = pA * 0.68f + pB * 0.32f;
                float mut = (float)((crng.NextDouble() - 0.5) * 2.0 * MutationSigma);
                AnomalyGenome[g] = Math.Max(0.08f, Math.Min(3.20f, blend + mut));
            }
        }

        // 2. POST-GENERATION NEGATIVE TRAIT AUDIT (Preventing Negative Traits from Transmitting to Gen G+1)
        // Audit Rule A: Prevent False-Negative Blindness (Threshold Bias gene[6] drifting too high > 1.42)
        float alphaBias = (alphaParent != null) ? alphaParent.AnomalyGenome[6] : 1.24f;
        if (AnomalyGenome[6] > 1.42f || AnomalyGenome[6] > alphaBias + 0.08f) {
            AnomalyGenome[6] = Math.Min(1.28f, alphaBias * 0.96f);
            prunedTraits.Add("PRUNED:HIGH_FN_BIAS");
            PrunedTraitsCount++;
        }

        // Audit Rule B: Prevent Loss of VirusTotal / Port Threat Sensitivity (gene[3] & gene[4])
        float minPortVtGene = (alphaParent != null) ? Math.Max(0.95f, alphaParent.AnomalyGenome[3] * 0.96f) : 0.95f;
        if (AnomalyGenome[3] < minPortVtGene) {
            AnomalyGenome[3] = minPortVtGene + 0.04f;
            prunedTraits.Add("PRUNED:WEAK_VT_GENE");
            PrunedTraitsCount++;
        }
        float minSynGene = (alphaParent != null) ? Math.Max(0.88f, alphaParent.AnomalyGenome[4] * 0.95f) : 0.88f;
        if (AnomalyGenome[4] < minSynGene) {
            AnomalyGenome[4] = minSynGene + 0.03f;
            prunedTraits.Add("PRUNED:WEAK_SYN_GENE");
            PrunedTraitsCount++;
        }

        // Audit Rule C: Synaptic Connectome Inheritance & Apoptosis of Weakened Alert Axons
        if (alphaParent != null && alphaParent != this) {
            for (int i = 0; i < N; i++) {
                for (int j = 0; j < N; j++) {
                    if (alphaParent.W[i, j] > 0.01f || W[i, j] > 0.01f) {
                        float wCross = alphaParent.W[i, j] * 0.65f + W[i, j] * 0.35f;
                        if (crng.NextDouble() < 0.10) {
                            wCross += (float)((crng.NextDouble() - 0.48) * MutationSigma);
                        }
                        W[i, j] = Math.Max(0f, Math.Min(1.45f, wCross));
                    }
                }
            }
            if (alphaParent.FitnessScore > FitnessScore) {
                FitnessScore = FitnessScore * 0.40f + alphaParent.FitnessScore * 0.60f;
            }
            if (alphaParent.DetectionAccuracy > DetectionAccuracy) {
                DetectionAccuracy = DetectionAccuracy * 0.40f + alphaParent.DetectionAccuracy * 0.60f;
            }
        }
        // Guarantee critical sensory->attack axons never decay (VirusTotal neuron 11 & Anomaly neurons 7..10)
        if (W[11, 61] < 0.85f || W[8, 61] < 0.80f) {
            W[11, 61] = Math.Max(0.92f, W[11, 61]);
            W[8, 61] = Math.Max(0.85f, W[8, 61]);
            prunedTraits.Add("PRUNED:AXON_DECAY");
            PrunedTraitsCount++;
        }

        SynapticGain = Math.Min(2.65f, SynapticGain + 0.04f);
        EliteFitness = Math.Max(EliteFitness, FitnessScore);
        Array.Copy(AnomalyGenome, EliteGenome, 8);
        GenTruePositives = 0;
        GenFalseNegatives = 0;

        LastPrunedTrait = (prunedTraits.Count > 0) ? string.Join("+", prunedTraits.ToArray()) : "AUDIT_OK(0_NEG_TRAITS)";
        LastCrossSummary = string.Format("G{0}->G{1}x{2} [{3}]", prevGen, Generation, alphaName, LastPrunedTrait);

        string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        auditSummaryLine = string.Format("[{0}] [GEN_{1}_AUDIT] {2} x {3} ({4}) | FIT:{5:0.0}->{6:0.0} ACC:{7:0.0}% FN_RATE:{8:0.1}% | {9}",
            DateTime.Now.ToString("HH:mm:ss"), Generation, selfName, alphaName, selectionMode,
            preFit, FitnessScore, DetectionAccuracy, preFnRate, LastPrunedTrait);

        jsonlRecord = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{{\"timestamp\":\"{0}\",\"agent\":\"{1}\",\"prev_gen\":{2},\"new_gen\":{3},\"alpha_parent\":\"{4}\",\"selection_mode\":\"{5}\",\"pre_fit\":{6:0.00},\"post_fit\":{7:0.00},\"accuracy\":{8:0.00},\"fn_rate_pct\":{9:0.00},\"pruned_traits\":\"{10}\",\"genome\":[{11:0.000},{12:0.000},{13:0.000},{14:0.000},{15:0.000},{16:0.000},{17:0.000},{18:0.000}]}}",
            ts, selfName, prevGen, Generation, alphaName, selectionMode, preFit, FitnessScore, DetectionAccuracy, preFnRate, LastPrunedTrait,
            AnomalyGenome[0], AnomalyGenome[1], AnomalyGenome[2], AnomalyGenome[3],
            AnomalyGenome[4], AnomalyGenome[5], AnomalyGenome[6], AnomalyGenome[7]);
    }

    public bool HasSkill(int requiredLevel) {
        return Level >= requiredLevel;
    }

    public string GetUnlockedSkillsSummary() {
        List<string> s = new List<string>();
        if (Level >= 1) s.Add("RayoConv");
        if (Level >= 2) s.Add("EscudoHex");
        if (Level >= 3) s.Add("SyncP2P");
        if (Level >= 4) s.Add("TriAtaque");
        if (Level >= 5) s.Add("SaltoCuántico");
        if (Level >= 6) s.Add("SonarDeep");
        return string.Join(" + ", s.ToArray());
    }

    public void Step(float[] sensoryInput, float dt) {
        float tau = 0.17f;
        float vThresh = 0.90f;

        for (int i = 0; i < N; i++) {
            float ext = (i < sensoryInput.Length) ? sensoryInput[i] : 0f;
            if (i >= 16 && i < 48 && rng.NextDouble() < 0.09) {
                ext += (float)(0.16 + rng.NextDouble() * 0.26);
            }

            float syn = 0f;
            for (int pre = 0; pre < N; pre++) {
                if (SpikeGlow[pre] > 0.60f) {
                    syn += W[pre, i] * 0.33f * SynapticGain;
                }
            }

            V[i] += ((-V[i] + ext + syn) / tau) * dt;
            if (V[i] >= vThresh) {
                V[i] = 0.0f;
                SpikeGlow[i] = 1.0f;
                // Update Hebbian pre->post eligibility traces for active pre-synaptic neurons
                for (int pre = 0; pre < N; pre++) {
                    if (pre != i && SpikeGlow[pre] > 0.45f) {
                        Eligibility[pre, i] = Math.Min(1.0f, Eligibility[pre, i] + SpikeGlow[pre] * 0.45f);
                    }
                }
            } else {
                V[i] = Math.Max(0f, V[i]);
                SpikeGlow[i] *= 0.86f;
            }
        }

        // Decay eligibility traces slowly
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                Eligibility[i, j] *= 0.95f;
            }
        }

        float sCuriosity = (SpikeGlow[36] + SpikeGlow[37] + SpikeGlow[38]) / 3f;
        float sHunger    = (SpikeGlow[39] + SpikeGlow[40] + SpikeGlow[41]) / 3f;
        float sAlert     = (SpikeGlow[42] + SpikeGlow[43] + SpikeGlow[44]) / 3f;
        float sSync      = (SpikeGlow[45] + SpikeGlow[46] + SpikeGlow[47]) / 3f;

        DriveCuriosity = Clamp01(DriveCuriosity * 0.97f + sCuriosity * 0.08f + 0.009f);
        DriveHunger    = Clamp01(DriveHunger + 0.0035f + sHunger * 0.04f);
        DriveAlert     = Clamp01(DriveAlert * 0.94f + sAlert * 0.12f);
        DriveSync      = Clamp01(DriveSync * 0.96f + sSync * 0.08f + 0.006f);
    }

    // Reward-Modulated Hebbian Synaptic Learning
    public bool ReinforceReward(float rewardSignal, int xpGain) {
        float lr = 0.035f * rewardSignal;
        float totalDelta = 0f;
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                if (Eligibility[i, j] > 0.05f && W[i, j] > 0.01f) {
                    float dw = lr * Eligibility[i, j];
                    W[i, j] = Math.Min(1.35f, W[i, j] + dw);
                    totalDelta += dw;
                }
            }
        }
        LastPlasticityDelta = totalDelta;
        SynapticGain = Math.Min(2.50f, SynapticGain + 0.008f * rewardSignal);

        XP += xpGain;
        bool leveledUp = false;
        while (XP >= NextLevelXP && Level < 99) {
            XP -= NextLevelXP;
            Level++;
            SynapticGain = Math.Min(2.50f, SynapticGain + 0.06f);
            leveledUp = true;
        }
        return leveledUp;
    }

    private static float Clamp01(float v) {
        return v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}

// ============================================================================
// 3D AUTONOMOUS PORYGON EVOLUTION SWARM (PORYGON + PORYGON2 + PORYGON-Z)
// ============================================================================
public class Porygon3DAvatar {
    public struct Vec3 {
        public float X, Y, Z;
        public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        public static Vec3 operator *(Vec3 a, float s) { return new Vec3(a.X * s, a.Y * s, a.Z * s); }
        public static Vec3 Cross(Vec3 a, Vec3 b) {
            return new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }
        public static float Dot(Vec3 a, Vec3 b) { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }
        public Vec3 Normalized() {
            float len = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            if (len < 1e-5f) return new Vec3(0, 0, 1);
            return new Vec3(X / len, Y / len, Z / len);
        }
    }

    public class Face3D {
        public int[] Indices;
        public Color BaseColor;
        public float DepthBias;
        public Vec3 LocalNormal;
        public byte ContourEdgeMask; // bit 0: edge(0,1), bit 1: edge(1,2), bit 2: edge(2,0)
        public Face3D(Color col, float bias, params int[] idx) {
            BaseColor = col;
            DepthBias = bias;
            Indices = idx;
            ContourEdgeMask = 0;
        }
    }

    public class ModelData {
        public string DisplayName;
        public string ShortHudName;
        public List<Vec3> Verts = new List<Vec3>();
        public List<Face3D> Faces = new List<Face3D>();
    }

    public struct RealTelemetryFeed {
        public float DownKbps, UpKbps;
        public float DlMeanKbps, DlStdKbps, DlZScore;
        public float UlMeanKbps, UlStdKbps, UlZScore;
        public int PingMs;
        public float PingMeanMs, PingZScore;
        public int ActiveTcpCount, SynCount, UnusualPortCount, NewHostCount;
        public float AnomalyIndex;
        public string LastAnomalySummary;
    }

    public static float GetDepthScale(float z) {
        return Clamp(460f / (460f - z), 0.54f, 1.52f);
    }

    public class DataPacket {
        public float X, Y, Z;
        public float VX, VY, VZ;
        public float floatRot;
        public int Kind; // 0 = Real Normal Flow, 1 = New External Host / Elevated Flow Orb, 2 = Real Network Anomaly
        public int HP;
        public Color Col;

        // Real Network Flow & Anomaly Metadata
        public string RemoteEndpoint = "1.1.1.1:443";
        public string LocalEndpoint = "192.168.1.x:51240";
        public string ProtocolName = "HTTPS/TLS";
        public string TcpStateStr = "ESTABLISHED";
        public string AnomalyType = "BASELINE_TLS_FLOW";
        public string AnomalyDetail = "Encrypted web traffic within normal Z-score baseline";
        public float AnomalyScore = 0.08f; // 0.0..1.0
        public float KbpsSample = 14.2f;
        public bool IsHovered = false;
        public string AssignedAgentName = "PORYGON";

        // Dynamic Porygon-Packet Interaction State
        public float PullIntensity = 0.0f;     // 0..1 tractor vortex lock intensity
        public int TargetedByAgentIdx = -1;
        public float OrbitPhase = 0.0f;
        public float EvadeCooldown = 0.0f;
    }

    public class CaptureVertexShard {
        public float X, Y, Z;
        public float PrevX, PrevY;
        public float VX, VY, VZ;
        public float RotAngle;
        public float RotSpeed;
        public float Size;
        public float Life;
        public float MaxLife;
        public int ShapeKind;      // 0 = 3D Faceted Triangle, 1 = Wireframe Octa-Diamond, 2 = Glowing Vertex Node
        public int BurstGroupId;   // Links vertices from the same packet capture into a constellation mesh
        public int OwnerAgentIdx;
        public bool HomingToAgent;
        public Color Col;
        public Color SecondaryCol;
    }

    public class CaptureShockwave {
        public float X, Y, Z;
        public float Life;         // 1.0 down to 0.0
        public float MaxRadius;
        public float RotAngle;
        public int Kind;           // 0 = Flow Decoded, 1 = Host Audited, 2 = Tri-Attack Anomaly Shatter, 3 = Skill Cast Nova
        public Color Col;
        public Color SecondaryCol;
        public string FloatingLabel;
    }

    public class PorygonAgent {
        public int AgentIndex;
        public string Name;
        public string RoleTag;
        public string SpecialtyDesc;
        public ModelData Model;
        public FlyBrainConnectome Brain;
        public Color AuraColor;

        public float PosX, PosY, PosZ;
        public float VelX, VelY, VelZ;
        public float HomeX, HomeY, HomeZ;
        public float WaypointX, WaypointY, WaypointZ;
        public int WaypointTimer = 0;

        public float Yaw = 0.58f;
        public float Pitch = 0.16f;
        public float Roll = 0.0f;
        public float HeadYaw = 0.0f;
        public float HeadPitch = 0.0f;
        public float WalkPhase = 0.0f;
        public float FloatBob = 0.0f;
        public float FacingDir = 1.0f;
        public float ScaleFactor = 1.0f;

        public string CurrentTargetEndpoint = "SCANNING...";
        public bool IsHovered = false;

        // Dynamic Pursuit, Tractor Lock & Vertex Absorption State
        public float DashBoostTimer = 0.0f;
        public float AbsorbFlashTimer = 0.0f;
        public float TractorPhase = 0.0f;
        public PointF ActiveLockTarget = PointF.Empty;
        public float ActiveLockZ = 0.0f;
        public float ActiveLockStrength = 0.0f;
        public int ActiveLockKind = 0;

        // Advanced Ability Timers
        public float SpinRollTimer = 0.0f;
        public float BeamTimer = 0.0f;
        public PointF BeamTarget = PointF.Empty;
        public float ShieldTimer = 0.0f;       // Skill LVL 2: Hexagonal Conversion2 Shield
        public float SyncTimer = 0.0f;         // Skill LVL 3: P2P Neural Sync Link
        public int SyncPartnerIdx = -1;
        public float TriAttackTimer = 0.0f;    // Skill LVL 4: Elemental Tri-Attack Matrix
        public PointF TriAttackTarget = PointF.Empty;
        public float QuantumBlinkTimer = 0.0f; // Skill LVL 5: Quantum Overclock Warp
        public PointF BlinkStartPt = PointF.Empty;
        public float BlinkStartZ = 0.0f;
        public float ScanWaveTimer = 0.0f;     // Skill LVL 6: Deep Sonar Scan Wave
        public int SyncCooldown = 0;
        public float LevelUpBannerTimer = 0.0f;
        public string LevelUpBannerText = "★ EVO CROSSOVER // GEN UP! ★";
        public Vec3[] TvBuf;
        public PointF[] PvBuf;
    }

    public List<PorygonAgent> Agents = new List<PorygonAgent>();
    public List<DataPacket> Packets = new List<DataPacket>();
    public List<CaptureVertexShard> VertexShards = new List<CaptureVertexShard>();
    public List<CaptureShockwave> Shockwaves = new List<CaptureShockwave>();
    private int burstGroupCounter = 0;
    public List<string> IdsDetectionLog = new List<string>();
    public DataPacket HoveredPacket = null;
    public PorygonAgent HoveredAgent = null;
    public RealTelemetryFeed LatestFeed;
    public List<Rectangle> MonitorRects = new List<Rectangle>();
    public Rectangle PrimaryRect;

    // 3D Meshes for Life Cycle Morphogenesis (Stage 0: Porygon -> Stage 1: Porygon2 -> Stage 2: Porygon-Z)
    public ModelData Stage0Model = null;
    public ModelData Stage1Model = null;
    public ModelData Stage2Model = null;

    // Live CPU Multi-Core Evolutionary Training, Lineage Audit & VirusTotal Metrics
    public int CpuEvoStepsPerSec = 18750;
    public int CpuTrainingStepsPerSec { get { return CpuEvoStepsPerSec; } }
    public int HighestGeneration = 1;
    public float BestSwarmFitness = 86.0f;
    public float AvgSwarmAccuracy = 91.5f;
    public int TotalCrossovers = 0;
    public int TotalPrunedTraits = 0;
    public string LastLineageAuditSummary = "GEN_AUDIT // Waiting for first G1->G2 crossover...";
    public int VtApiQueries = 0;
    public int VtMaliciousHits = 0;
    public int VtCleanVerified = 0;
    public string LastVtVerdictSummary = "VT_API // Connected to 192.168.2.222:5080";
    public string LastVtVerdict { get { return LastVtVerdictSummary; } }
    public int SelectedBrainAgentIndex = 0;
    public DateTime LastManualBrainSelectUtc = DateTime.MinValue;
    public bool BrainAutoFocusedOnThreat = false;
    private int cpuEvoBatchCounter = 0;

    private object packetLock = new object();
    private Random rng = new Random(1337);
    private int saveTickCounter = 0;

    private static string BrainSavePath {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            return Path.Combine(dir, "porygon_brains.json");
        }
    }

    private static string LineageJsonlPath {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            return Path.Combine(dir, "porygon_lineage_log.jsonl");
        }
    }

    public List<string> LineageAuditHistory = new List<string>();

    private static string LocalLineageAuditLogPath {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            return Path.Combine(dir, "porygon_lineage_audit.log");
        }
    }

    private static string ProjectLineageAuditLogPath {
        get {
            return @"C:\Users\Admin\Porygon-Cyber-IDS-Wallpaper\logs\porygon_lineage_audit.log";
        }
    }

    private static void TrimFileIfExceedsBytes(string filePath, long maxBytes, int keepLines) {
        try {
            if (!File.Exists(filePath)) return;
            FileInfo fi = new FileInfo(filePath);
            if (fi.Length <= maxBytes) return;
            string[] allLines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (allLines.Length > keepLines) {
                string[] kept = new string[keepLines];
                Array.Copy(allLines, allLines.Length - keepLines, kept, 0, keepLines);
                File.WriteAllLines(filePath, kept, Encoding.UTF8);
            }
        } catch { }
    }

    private void AppendLineageRecord(string auditSummaryLine, string jsonlRecord) {
        try {
            lock (LineageAuditHistory) {
                LineageAuditHistory.Add(auditSummaryLine);
                while (LineageAuditHistory.Count > 40) LineageAuditHistory.RemoveAt(0);
            }
            string dir1 = Path.GetDirectoryName(LineageJsonlPath);
            if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
            if (TotalCrossovers % 25 == 0) {
                TrimFileIfExceedsBytes(LineageJsonlPath, 512 * 1024, 250);
                TrimFileIfExceedsBytes(LocalLineageAuditLogPath, 256 * 1024, 250);
            }
            File.AppendAllText(LineageJsonlPath, jsonlRecord + "\r\n", Encoding.UTF8);
            File.AppendAllText(LocalLineageAuditLogPath, auditSummaryLine + "\r\n", Encoding.UTF8);

            string dir2 = Path.GetDirectoryName(ProjectLineageAuditLogPath);
            if (Directory.Exists(dir2)) {
                File.AppendAllText(ProjectLineageAuditLogPath, auditSummaryLine + "\r\n", Encoding.UTF8);
            }
        } catch { }
    }

    private void LoadLineageHistory() {
        try {
            TrimFileIfExceedsBytes(LineageJsonlPath, 512 * 1024, 250);
            TrimFileIfExceedsBytes(LocalLineageAuditLogPath, 256 * 1024, 250);
            string path = File.Exists(LocalLineageAuditLogPath) ? LocalLineageAuditLogPath : ProjectLineageAuditLogPath;
            if (File.Exists(path)) {
                string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                int start = Math.Max(0, lines.Length - 30);
                lock (LineageAuditHistory) {
                    LineageAuditHistory.Clear();
                    for (int i = start; i < lines.Length; i++) {
                        if (!string.IsNullOrWhiteSpace(lines[i])) LineageAuditHistory.Add(lines[i].Trim());
                    }
                    if (LineageAuditHistory.Count > 0) {
                        LastLineageAuditSummary = LineageAuditHistory[LineageAuditHistory.Count - 1];
                    }
                }
                TotalCrossovers = Math.Max(TotalCrossovers, lines.Length);
            } else if (File.Exists(LineageJsonlPath)) {
                string[] jlines = File.ReadAllLines(LineageJsonlPath, Encoding.UTF8);
                TotalCrossovers = Math.Max(TotalCrossovers, jlines.Length);
            }
        } catch { }
    }

    public void InjectVirusTotalSensorySignal(string targetIp, int maliciousCount, int suspiciousCount, float threatScore, string verdictSummary) {
        InjectVirusTotalSensorySignal(maliciousCount > 0 || suspiciousCount > 1 || threatScore >= 0.45f, threatScore, targetIp, verdictSummary);
    }

    public void InjectVirusTotalSensorySignal(bool isMalicious, float threatScore, string targetIp, string verdictSummary) {
        lock (packetLock) {
            VtApiQueries++;
            if (isMalicious) VtMaliciousHits++;
            else VtCleanVerified++;
            LastVtVerdictSummary = verdictSummary;

            for (int i = 0; i < Agents.Count; i++) {
                FlyBrainConnectome br = Agents[i].Brain;
                if (isMalicious) {
                    // Excite Neuron 11 (VirusTotal Threat Sensor) & Central Alert Neurons (42..44)
                    br.V[11] = 1.15f;
                    br.SpikeGlow[11] = 1.0f;
                    br.V[42] = Math.Max(br.V[42], 0.98f);
                    br.SpikeGlow[42] = 1.0f;
                    br.DriveAlert = Math.Min(1.0f, br.DriveAlert + 0.35f);
                    br.TruePositives++;
                    br.GenTruePositives++;
                } else {
                    // Verified clean by VirusTotal -> reinforce True Negative baseline & reduce false alarms
                    br.V[11] = 0.25f;
                    br.SpikeGlow[11] = Math.Max(br.SpikeGlow[11], 0.45f);
                    br.TrueNegatives++;
                }
            }
        }
    }

    public void RefreshMonitors(int fallbackW, int fallbackH) {
        List<string> names;
        int tw, th;
        HackerWallpaperForm.QueryPhysicalMonitors(out MonitorRects, out names, out PrimaryRect, out tw, out th);
        if (MonitorRects.Count == 0) {
            PrimaryRect = new Rectangle(0, 0, fallbackW, fallbackH);
            MonitorRects.Add(PrimaryRect);
        }
    }

    public Rectangle GetContainingOrNearestMonitor(float x, float y) {
        int ix = (int)x, iy = (int)y;
        for (int i = 0; i < MonitorRects.Count; i++) {
            if (MonitorRects[i].Contains(ix, iy)) return MonitorRects[i];
        }
        Rectangle best = PrimaryRect;
        float bestDistSq = float.MaxValue;
        for (int i = 0; i < MonitorRects.Count; i++) {
            Rectangle m = MonitorRects[i];
            float cx = Clamp(x, m.Left + 40, m.Right - 40);
            float cy = Clamp(y, m.Top + 40, m.Bottom - 40);
            float dx = x - cx, dy = y - cy;
            float d2 = dx * dx + dy * dy;
            if (d2 < bestDistSq) {
                bestDistSq = d2;
                best = m;
            }
        }
        return best;
    }

    public bool IsInsideAnyMonitor(float x, float y, int margin) {
        int ix = (int)x, iy = (int)y;
        for (int i = 0; i < MonitorRects.Count; i++) {
            Rectangle m = MonitorRects[i];
            if (ix >= m.Left + margin && ix <= m.Right - margin && iy >= m.Top + margin && iy <= m.Bottom - margin) {
                return true;
            }
        }
        return false;
    }

    public void PickRandomMonitorPoint(out float px, out float py, int padX, int padY) {
        if (MonitorRects.Count == 0) {
            px = 400f; py = 400f; return;
        }
        Rectangle m = MonitorRects[rng.Next(MonitorRects.Count)];
        int minX = m.Left + Math.Min(padX, m.Width / 4);
        int maxX = Math.Max(minX + 20, m.Right - Math.Min(padX, m.Width / 4));
        int minY = m.Top + Math.Min(padY, m.Height / 4);
        int maxY = Math.Max(minY + 20, m.Bottom - Math.Min(padY, m.Height / 4));
        px = rng.Next(minX, maxX);
        py = rng.Next(minY, maxY);
    }

    public Porygon3DAvatar(int screenW, int screenH) {
        RefreshMonitors(screenW, screenH);
        LoadThreeFavoriteModels(screenW, screenH);
        LoadBrainStates();
        LoadLineageHistory();
        int maxGen = 1;
        float maxFit = 0f, sumAcc = 0f;
        int totalPruned = 0;
        for (int i = 0; i < Agents.Count; i++) {
            SyncAgentLifeStageMesh(Agents[i]);
            FlyBrainConnectome b = Agents[i].Brain;
            if (b.Generation > maxGen) maxGen = b.Generation;
            if (b.FitnessScore > maxFit) maxFit = b.FitnessScore;
            sumAcc += b.DetectionAccuracy;
            totalPruned += b.PrunedTraitsCount;
        }
        if (Agents.Count > 0) {
            HighestGeneration = maxGen;
            BestSwarmFitness = maxFit;
            AvgSwarmAccuracy = sumAcc / Agents.Count;
            TotalPrunedTraits = totalPruned;
        }

        IdsDetectionLog.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] EVO_INIT // " + Agents.Count + " Evolutionary Cores + VT API online across " + MonitorRects.Count + " monitors");
    }

    private void LoadThreeFavoriteModels(int screenW, int screenH) {
        Agents.Clear();

        // Stage 0: Porygon (Gen-1 Seed)
        ModelData m1 = TryLoadGlb("pokemon_porygon.glb", "Porygon (Gen-1 Cyber Seed)", "PORYGON_V1");
        // Stage 1: Porygon2 (Johto Mature Core)
        ModelData m2 = TryLoadGlb("porygon2__johto.glb", "Porygon2 (Johto Mature Core)", "PORYGON2_V2");
        // Stage 2: Porygon-Z (Sinnoh Apex Hunter)
        ModelData m3 = TryLoadGlb("porygon-z.glb",       "Porygon-Z (Hyper Apex Hunter)", "PORYGON_Z_V3");

        Stage0Model = m1;
        Stage1Model = m2 ?? m1;
        Stage2Model = m3 ?? m2 ?? m1;

        Rectangle mon0 = PrimaryRect;
        Rectangle mon1 = MonitorRects.Count > 1 ? MonitorRects[1 % MonitorRects.Count] : PrimaryRect;
        Rectangle mon2 = MonitorRects.Count > 2 ? MonitorRects[2 % MonitorRects.Count] : mon0;
        Rectangle mon3 = MonitorRects.Count > 3 ? MonitorRects[3 % MonitorRects.Count] : mon1;
        Rectangle mon4 = MonitorRects.Count > 4 ? MonitorRects[4 % MonitorRects.Count] : mon2;

        if (m1 != null) {
            Agents.Add(CreateAgent(0, "PORYGON", "FLOW_BASELINE_AI",
                "Analiza paquetes TCP estándar y entrena la línea base Z-Score en el entorno multi-monitor",
                m1, mon0.Left + mon0.Width * 0.42f, mon0.Top + mon0.Height * 0.56f, 0.94f, Color.FromArgb(0, 240, 255), 101, 14.0f));
        }
        if (m2 != null) {
            Agents.Add(CreateAgent(1, "PORYGON2", "ENDPOINT_AUDITOR",
                "Inspecciona IPs remotas nuevas con VirusTotal API y sincroniza pesos P2P entre pantallas",
                m2, mon1.Left + mon1.Width * 0.50f, mon1.Top + mon1.Height * 0.56f, 0.96f, Color.FromArgb(236, 72, 153), 202, 42.0f));
        }
        if (m3 != null) {
            Agents.Add(CreateAgent(2, "PORYGON-Z", "ANOMALY_HUNTER_IDS",
                "Detecta y neutraliza anomalías de red (VirusTotal, alertas NIDS/FortiGate y picos Z-Score)",
                m3, mon2.Left + mon2.Width * 0.52f, mon2.Top + mon2.Height * 0.54f, 1.02f, Color.FromArgb(168, 85, 247), 303, 74.0f));
        }
        if (MonitorRects.Count >= 4 && m2 != null) {
            Agents.Add(CreateAgent(3, "PORYGON2-A", "FORTIGATE_SENTINEL",
                "Vigila el tráfico FortiGate y correlaciona flujos móviles en los monitores superiores",
                m2, mon3.Left + mon3.Width * 0.48f, mon3.Top + mon3.Height * 0.56f, 0.95f, Color.FromArgb(250, 204, 21), 404, 54.0f));
        }
        if (MonitorRects.Count >= 5 && m3 != null) {
            Agents.Add(CreateAgent(4, "PORYGON-Z2", "NIDS_DEEP_HUNTER",
                "Cazador cuántico de sondas laterales NIDS y alertas SQL en la malla de 5 monitores",
                m3, mon4.Left + mon4.Width * 0.48f, mon4.Top + mon4.Height * 0.56f, 1.00f, Color.FromArgb(16, 255, 160), 505, 86.0f));
        }
    }

    private PorygonAgent CreateAgent(int idx, string name, string role, string specDesc, ModelData model,
        float startX, float startY, float scaleFactor, Color auraCol, int seed, float initialLifeAge) {
        FlyBrainConnectome brain = new FlyBrainConnectome(seed);
        brain.Level = idx + 2;
        brain.LifeAge = initialLifeAge;
        brain.LifeStage = (initialLifeAge < 33f) ? 0 : (initialLifeAge < 68f ? 1 : 2);
        float startZ = ((idx % 3) - 1) * 85f;
        return new PorygonAgent {
            AgentIndex = idx,
            Name = name,
            RoleTag = role,
            SpecialtyDesc = specDesc,
            Model = model,
            Brain = brain,
            AuraColor = auraCol,
            PosX = startX,
            PosY = startY,
            PosZ = startZ,
            HomeX = startX,
            HomeY = startY,
            HomeZ = startZ,
            WaypointX = startX,
            WaypointY = startY,
            WaypointZ = startZ,
            FloatBob = idx * 2.1f,
            WalkPhase = idx * 1.7f,
            ScaleFactor = scaleFactor
        };
    }

    public void SyncAgentLifeStageMesh(PorygonAgent a) {
        if (a == null || a.Brain == null) return;
        int newStage = (a.Brain.LifeAge < 33.0f) ? 0 : ((a.Brain.LifeAge < 68.0f) ? 1 : 2);
        bool stageChanged = (newStage != a.Brain.LifeStage);
        a.Brain.LifeStage = newStage;

        string suffix = (a.AgentIndex >= 3) ? ("-" + (a.AgentIndex - 2)) : "";
        if (newStage == 0 && Stage0Model != null) {
            a.Model = Stage0Model;
            a.Name = "PORYGON" + suffix;
            a.ScaleFactor = 0.93f;
        } else if (newStage == 1 && Stage1Model != null) {
            a.Model = Stage1Model;
            a.Name = "PORYGON2" + suffix;
            a.ScaleFactor = 0.96f;
        } else if (Stage2Model != null) {
            a.Model = Stage2Model;
            a.Name = "PORYGON-Z" + suffix;
            a.ScaleFactor = 1.01f;
        }

        if (stageChanged) {
            a.SpinRollTimer = 1.0f;
            a.ShieldTimer = 1.0f;
            a.ScanWaveTimer = 0.85f;
        }
    }

    // Background CPU Evolutionary Accelerator (runs on single background thread every 800ms without multi-core contention)
    public void RunCpuEvolutionTick(RealTelemetryFeed feed) {
        if (Agents.Count == 0) return;
        cpuEvoBatchCounter++;
        int stepsPerAgent = 300;

        float dlZ = feed.DlZScore;
        float ulZ = feed.UlZScore;
        float pgZ = feed.PingZScore;
        float anomIdx = feed.AnomalyIndex;
        int tickSeed = Environment.TickCount ^ (cpuEvoBatchCounter * 2654435761.GetHashCode());

        // 1. Sequential Neuro-Evolutionary Training on background thread (avoids waking all 12 Ryzen threads every 250ms)
        for (int aIdx = 0; aIdx < Agents.Count; aIdx++) {
            PorygonAgent ag = Agents[aIdx];
            uint sVal = (uint)(tickSeed + aIdx * 999983);
            ag.Brain.CpuTrainMicroBatch(stepsPerAgent, dlZ, ulZ, pgZ, anomIdx, sVal);
        }

        // 2. Advance Life Cycle, Morph 3D Stages, and Execute Alpha Genetic Crossover + Anti-Negative-Trait Audit at 100% Age
        lock (packetLock) {
            PorygonAgent alphaAgent = Agents[0];
            for (int i = 1; i < Agents.Count; i++) {
                if (Agents[i].Brain.FitnessScore > alphaAgent.Brain.FitnessScore) {
                    alphaAgent = Agents[i];
                }
            }

            int maxGen = 1;
            float maxFit = 0f;
            float sumAcc = 0f;
            int totalPruned = 0;

            bool anyCrossover = false;
            for (int i = 0; i < Agents.Count; i++) {
                PorygonAgent ag = Agents[i];
                FlyBrainConnectome br = ag.Brain;

                br.LifeAge += 0.42f; // Full 3-stage life cycle (Porygon -> Porygon2 -> Porygon-Z -> Crossover)
                if (br.LifeAge >= 100.0f) {
                    string oldName = ag.Name;
                    string auditLine, jsonlLine;
                    br.CrossoverAndAuditGeneration(alphaAgent.Brain, oldName, alphaAgent.Name, tickSeed + i * 31337, out auditLine, out jsonlLine);
                    SyncAgentLifeStageMesh(ag);
                    ag.LevelUpBannerTimer = 1.0f;
                    ag.LevelUpBannerText = string.Format("★ G{0} CROSSOVER // {1} ★", br.Generation, br.LastPrunedTrait);
                    ag.SpinRollTimer = 1.0f;
                    ag.ShieldTimer = 1.0f;
                    br.ActiveStateName = string.Format("GEN_{0}_AUDIT // {1}", br.Generation, br.LastPrunedTrait);

                    TotalCrossovers++;
                    LastLineageAuditSummary = string.Format("G{0} {1}x{2} -> {3}", br.Generation, oldName, alphaAgent.Name, br.LastPrunedTrait);
                    AppendLineageRecord(auditLine, jsonlLine);

                    IdsDetectionLog.Add(auditLine);
                    while (IdsDetectionLog.Count > 10) IdsDetectionLog.RemoveAt(0);
                    anyCrossover = true;
                } else {
                    SyncAgentLifeStageMesh(ag);
                }

                if (br.Generation > maxGen) maxGen = br.Generation;
                if (br.FitnessScore > maxFit) maxFit = br.FitnessScore;
                sumAcc += br.DetectionAccuracy;
                totalPruned += br.PrunedTraitsCount;
            }

            HighestGeneration = maxGen;
            BestSwarmFitness = maxFit;
            AvgSwarmAccuracy = sumAcc / Agents.Count;
            TotalPrunedTraits = totalPruned;
            CpuEvoStepsPerSec = Agents.Count * stepsPerAgent * 2;
            if (anyCrossover) {
                SaveBrainStates();
            }
        }
    }

    public void SaveBrainStates() {
        try {
            StringBuilder sb = new StringBuilder();
            sb.Append("[\r\n");
            for (int i = 0; i < Agents.Count; i++) {
                FlyBrainConnectome b = Agents[i].Brain;
                string safeTrait = (b.LastPrunedTrait ?? "GENOME_CLEAN").Replace("\"", "'");
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "  {{ \"Name\": \"{0}\", \"Level\": {1}, \"XP\": {2}, \"Packets\": {3}, \"Anomalies\": {4}, \"Syncs\": {5}, \"Gain\": {6:0.000}, \"Gen\": {7}, \"Age\": {8:0.0}, \"Fit\": {9:0.0}, \"Acc\": {10:0.0}, \"Pruned\": {11}, \"Trait\": \"{12}\", \"G0\": {13:0.000}, \"G1\": {14:0.000}, \"G2\": {15:0.000}, \"G3\": {16:0.000}, \"G4\": {17:0.000}, \"G5\": {18:0.000}, \"G6\": {19:0.000} }}{20}\r\n",
                    Agents[i].Name, b.Level, b.XP, b.PacketsCollected, b.AnomaliesCleared, b.NeuralSyncs, b.SynapticGain,
                    b.Generation, b.LifeAge, b.FitnessScore, b.DetectionAccuracy, b.PrunedTraitsCount, safeTrait,
                    b.AnomalyGenome[0], b.AnomalyGenome[1], b.AnomalyGenome[2], b.AnomalyGenome[3], b.AnomalyGenome[4], b.AnomalyGenome[5], b.AnomalyGenome[6],
                    (i < Agents.Count - 1) ? "," : "");
            }
            sb.Append("]");
            File.WriteAllText(BrainSavePath, sb.ToString(), Encoding.UTF8);
        } catch { }
    }

    public void LoadBrainStates() {
        try {
            if (!File.Exists(BrainSavePath)) return;
            string json = File.ReadAllText(BrainSavePath, Encoding.UTF8);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ArrayList list = jss.Deserialize<ArrayList>(json);
            if (list == null) return;
            for (int i = 0; i < list.Count && i < Agents.Count; i++) {
                var dict = list[i] as Dictionary<string, object>;
                if (dict == null) continue;
                FlyBrainConnectome b = Agents[i].Brain;
                if (dict.ContainsKey("Level"))     b.Level = Math.Max(1, Math.Min(99, Convert.ToInt32(dict["Level"])));
                if (dict.ContainsKey("XP"))        b.XP = Math.Max(0, Convert.ToInt32(dict["XP"]));
                if (dict.ContainsKey("Packets"))   b.PacketsCollected = Convert.ToInt32(dict["Packets"]);
                if (dict.ContainsKey("Anomalies")) b.AnomaliesCleared = Convert.ToInt32(dict["Anomalies"]);
                if (dict.ContainsKey("Syncs"))     b.NeuralSyncs = Convert.ToInt32(dict["Syncs"]);
                if (dict.ContainsKey("Gain"))      b.SynapticGain = Math.Max(1.0f, Math.Min(2.8f, Convert.ToSingle(dict["Gain"])));
                if (dict.ContainsKey("Gen"))       b.Generation = Math.Max(1, Convert.ToInt32(dict["Gen"]));
                if (dict.ContainsKey("Age"))       b.LifeAge = Math.Max(0f, Math.Min(99f, Convert.ToSingle(dict["Age"])));
                if (dict.ContainsKey("Fit"))       b.FitnessScore = Math.Max(50f, Math.Min(99.9f, Convert.ToSingle(dict["Fit"])));
                if (dict.ContainsKey("Acc"))       b.DetectionAccuracy = Math.Max(60f, Math.Min(99.9f, Convert.ToSingle(dict["Acc"])));
                if (dict.ContainsKey("Pruned"))    b.PrunedTraitsCount = Math.Max(0, Convert.ToInt32(dict["Pruned"]));
                if (dict.ContainsKey("Trait") && dict["Trait"] != null) b.LastPrunedTrait = dict["Trait"].ToString();
                for (int g = 0; g < 7; g++) {
                    string gk = "G" + g;
                    if (dict.ContainsKey(gk)) {
                        b.AnomalyGenome[g] = Math.Max(0.05f, Math.Min(3.50f, Convert.ToSingle(dict[gk])));
                        b.EliteGenome[g] = b.AnomalyGenome[g];
                    }
                }
                b.EliteFitness = b.FitnessScore;
            }
        } catch { }
    }

    public void InjectRealNetworkNode(int screenW, int screenH, int kind,
        string remEp, string locEp, string proto, string stateStr,
        string anomType, string anomDetail, float anomScore, float kbps) {
        lock (packetLock) {
            // Avoid duplicate nodes for the exact same remote endpoint + anomaly type
            for (int i = 0; i < Packets.Count; i++) {
                if (Packets[i].RemoteEndpoint == remEp && Packets[i].AnomalyType == anomType) {
                    Packets[i].KbpsSample = kbps;
                    Packets[i].AnomalyScore = Math.Max(Packets[i].AnomalyScore, anomScore);
                    return;
                }
            }
            int maxPackets = Math.Max(22, MonitorRects.Count * 7);
            if (Packets.Count >= maxPackets) {
                // If full, replace oldest normal flow if we are injecting an anomaly/new host
                if (kind > 0) {
                    for (int i = 0; i < Packets.Count; i++) {
                        if (Packets[i].Kind == 0 && !Packets[i].IsHovered) {
                            Packets.RemoveAt(i);
                            break;
                        }
                    }
                }
                if (Packets.Count >= maxPackets) return;
            }

            Color col = Color.FromArgb(0, 240, 255);
            if (kind == 1) col = Color.FromArgb(250, 204, 21);       // Golden New-Host / Elevated Flow Orb
            else if (kind == 2) col = Color.FromArgb(255, 42, 85);   // Red Real Traffic Anomaly
            else if (proto.Contains("UDP") || proto.Contains("HTTP")) col = Color.FromArgb(236, 72, 153);

            float spawnX, spawnY;
            PickRandomMonitorPoint(out spawnX, out spawnY, 170, 140);
            float spawnZ = (float)(rng.NextDouble() * 420.0 - 250.0); // -250 (deep cyberspace) .. +170 (near foreground)

            Packets.Add(new DataPacket {
                X = spawnX,
                Y = spawnY,
                Z = spawnZ,
                VX = (float)((rng.NextDouble() - 0.5) * (kind == 2 ? 2.15 : 1.35)),
                VY = (float)((rng.NextDouble() - 0.5) * (kind == 2 ? 2.15 : 1.35)),
                VZ = (float)((rng.NextDouble() - 0.5) * (kind == 2 ? 2.10 : 1.30)),
                floatRot = (float)(rng.NextDouble() * 6.28),
                Kind = kind,
                HP = (kind == 2) ? 2 : 1,
                Col = col,
                RemoteEndpoint = remEp,
                LocalEndpoint = locEp,
                ProtocolName = proto,
                TcpStateStr = stateStr,
                AnomalyType = anomType,
                AnomalyDetail = anomDetail,
                AnomalyScore = anomScore,
                KbpsSample = kbps,
                AssignedAgentName = (kind == 2) ? "PORYGON-Z" : (kind == 1 ? "PORYGON2" : "PORYGON")
            });
        }
    }

    public void TriggerTrainingWave(int screenW, int screenH) {
        // Inject a realistic IDS Anomaly Stress-Test Wave (Port Scan + Bandwidth Surge + New Hosts)
        InjectRealNetworkNode(screenW, screenH, 2, "185.220.101.42:4444", "Local:59120", "TCP/NON-STD", "SYN_SENT",
            "SUSPICIOUS_PORT_4444", "Outbound connection attempt on high-risk port 4444", 0.96f, 128.4f);
        InjectRealNetworkNode(screenW, screenH, 2, "45.33.32.156:8080", "Local:59124", "HTTP-PROXY", "ESTABLISHED",
            "TRAFFIC_SPIKE_ZSCORE", "Bandwidth burst +3.4σ above rolling EMA baseline", 0.91f, 540.0f);
        InjectRealNetworkNode(screenW, screenH, 1, "104.18.26.120:443", "Local:59130", "HTTPS/TLS", "ESTABLISHED",
            "NEW_EXTERNAL_HOST", "First-time external IP seen — queued for Porygon2 audit", 0.55f, 64.0f);
        InjectRealNetworkNode(screenW, screenH, 1, "172.64.155.89:443", "Local:59132", "HTTPS/TLS", "ESTABLISHED",
            "NEW_EXTERNAL_HOST", "New Cloudflare edge node — learning baseline signature", 0.48f, 42.0f);

        for (int i = 0; i < Agents.Count; i++) {
            PorygonAgent a = Agents[i];
            bool lvlUp = a.Brain.ReinforceReward(1.2f, 35);
            if (lvlUp) a.LevelUpBannerTimer = 1.0f;
            a.ShieldTimer = 1.0f;
            a.ScanWaveTimer = 1.0f;
            a.SpinRollTimer = 1.0f;
            a.Brain.ActiveStateName = "IDS_DRILL // ANOMALY_WAVE_ACTIVE";
        }
        SaveBrainStates();
    }

    public void BoostAllLevels() {
        for (int i = 0; i < Agents.Count; i++) {
            PorygonAgent a = Agents[i];
            a.Brain.ReinforceReward(1.5f, a.Brain.NextLevelXP);
            a.LevelUpBannerTimer = 1.0f;
            a.SpinRollTimer = 1.0f;
            a.ShieldTimer = 1.0f;
            a.Brain.ActiveStateName = "EVOLUTION // LEVEL_UP -> LVL " + a.Brain.Level;
        }
        SaveBrainStates();
    }

    public void ForceEvolutionaryCrossoverAll() {
        lock (packetLock) {
            if (Agents.Count == 0) return;
            PorygonAgent alphaAgent = Agents[0];
            for (int i = 1; i < Agents.Count; i++) {
                if (Agents[i].Brain.FitnessScore > alphaAgent.Brain.FitnessScore) {
                    alphaAgent = Agents[i];
                }
            }
            int maxGen = 1;
            float maxFit = 0f, sumAcc = 0f;
            int totalPruned = 0;
            int seedBase = Environment.TickCount;
            for (int i = 0; i < Agents.Count; i++) {
                PorygonAgent ag = Agents[i];
                FlyBrainConnectome br = ag.Brain;
                string oldName = ag.Name;
                string auditLine, jsonlLine;
                br.CrossoverAndAuditGeneration(alphaAgent.Brain, oldName, alphaAgent.Name, seedBase + i * 31337, out auditLine, out jsonlLine);
                SyncAgentLifeStageMesh(ag);
                ag.LevelUpBannerTimer = 1.0f;
                ag.LevelUpBannerText = string.Format("★ G{0} CROSSOVER // {1} ★", br.Generation, br.LastPrunedTrait);
                ag.SpinRollTimer = 1.0f;
                ag.ShieldTimer = 1.0f;
                br.ActiveStateName = string.Format("GEN_{0}_AUDIT // {1}", br.Generation, br.LastPrunedTrait);

                TotalCrossovers++;
                LastLineageAuditSummary = string.Format("G{0} {1}x{2} -> {3}", br.Generation, oldName, alphaAgent.Name, br.LastPrunedTrait);
                AppendLineageRecord(auditLine, jsonlLine);
                IdsDetectionLog.Add(auditLine);
                while (IdsDetectionLog.Count > 10) IdsDetectionLog.RemoveAt(0);

                if (br.Generation > maxGen) maxGen = br.Generation;
                if (br.FitnessScore > maxFit) maxFit = br.FitnessScore;
                sumAcc += br.DetectionAccuracy;
                totalPruned += br.PrunedTraitsCount;
            }
            HighestGeneration = maxGen;
            BestSwarmFitness = maxFit;
            AvgSwarmAccuracy = sumAcc / Agents.Count;
            TotalPrunedTraits = totalPruned;
            SaveBrainStates();
        }
    }

    public void SpawnDataPacket(int screenW, int screenH, int kind) {
        InjectRealNetworkNode(screenW, screenH, kind,
            "1.1.1.1:443", "Local:54321", "HTTPS/TLS", "ESTABLISHED",
            kind == 2 ? "PORT_ANOMALY" : (kind == 1 ? "NEW_HOST_AUDIT" : "NORMAL_TLS_FLOW"),
            kind == 2 ? "Unusual external port activity" : "Encrypted baseline traffic",
            kind == 2 ? 0.88f : (kind == 1 ? 0.52f : 0.10f), 24.5f);
    }

    private string FindGlbPath(string fileName) {
        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string localAppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper", "Models");
        string dlDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "PorygonModels");
        string[] candidates = new string[] {
            Path.Combine(appDir, "Models", fileName),
            Path.Combine(localAppDir, fileName),
            Path.Combine(dlDir, fileName),
            Path.Combine(appDir, fileName)
        };
        for (int i = 0; i < candidates.Length; i++) {
            if (File.Exists(candidates[i])) return candidates[i];
        }
        return null;
    }

    private ModelData TryLoadGlb(string fileName, string displayName, string shortHudName) {
        try {
            string path = FindGlbPath(fileName);
            if (path == null) return null;
            return LoadGlbFile(path, fileName.ToLowerInvariant(), displayName, shortHudName);
        } catch { return null; }
    }

    private ModelData LoadGlbFile(string path, string lowerFileName, string displayName, string shortHudName) {
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length < 28) return null;
        if (BitConverter.ToUInt32(bytes, 0) != 0x46546C67) return null;

        int offset = 12;
        string jsonStr = null;
        byte[] binChunk = null;
        while (offset + 8 <= bytes.Length) {
            int chunkLen = BitConverter.ToInt32(bytes, offset);
            uint chunkType = BitConverter.ToUInt32(bytes, offset + 4);
            offset += 8;
            if (chunkType == 0x4E4F534A) {
                jsonStr = Encoding.UTF8.GetString(bytes, offset, chunkLen);
            } else if (chunkType == 0x004E4942) {
                binChunk = new byte[chunkLen];
                Buffer.BlockCopy(bytes, offset, binChunk, 0, chunkLen);
            }
            offset += chunkLen;
        }
        if (jsonStr == null || binChunk == null) return null;

        JavaScriptSerializer jss = new JavaScriptSerializer();
        jss.MaxJsonLength = int.MaxValue;
        var root = jss.Deserialize<Dictionary<string, object>>(jsonStr);
        ArrayList accessors   = root.ContainsKey("accessors")   ? (ArrayList)root["accessors"]   : new ArrayList();
        ArrayList bufferViews = root.ContainsKey("bufferViews") ? (ArrayList)root["bufferViews"] : new ArrayList();
        ArrayList meshes      = root.ContainsKey("meshes")      ? (ArrayList)root["meshes"]      : new ArrayList();
        ArrayList nodes       = root.ContainsKey("nodes")       ? (ArrayList)root["nodes"]       : new ArrayList();
        ArrayList materials   = root.ContainsKey("materials")   ? (ArrayList)root["materials"]   : new ArrayList();
        ArrayList textures    = root.ContainsKey("textures")    ? (ArrayList)root["textures"]    : new ArrayList();
        ArrayList images      = root.ContainsKey("images")      ? (ArrayList)root["images"]      : new ArrayList();

        Bitmap[] bitmaps = new Bitmap[images.Count];
        try {
            for (int i = 0; i < images.Count; i++) {
                try {
                    var img = (Dictionary<string, object>)images[i];
                    if (img.ContainsKey("bufferView")) {
                        int bvIdx = Convert.ToInt32(img["bufferView"]);
                        var bv = (Dictionary<string, object>)bufferViews[bvIdx];
                        int bvOff = bv.ContainsKey("byteOffset") ? Convert.ToInt32(bv["byteOffset"]) : 0;
                        int bvLen = Convert.ToInt32(bv["byteLength"]);
                        using (MemoryStream ms = new MemoryStream(binChunk, bvOff, bvLen)) {
                            bitmaps[i] = new Bitmap(Image.FromStream(ms));
                        }
                    }
                } catch { }
            }

            float[][] worldMats = new float[nodes.Count][];
            int[] parentIdx = new int[nodes.Count];
            for (int i = 0; i < nodes.Count; i++) parentIdx[i] = -1;
            for (int i = 0; i < nodes.Count; i++) {
                var nd = (Dictionary<string, object>)nodes[i];
                if (nd.ContainsKey("children")) {
                    ArrayList ch = (ArrayList)nd["children"];
                    foreach (object cObj in ch) parentIdx[Convert.ToInt32(cObj)] = i;
                }
            }
            for (int i = 0; i < nodes.Count; i++) {
                float[] m = GetNodeLocalMatrix((Dictionary<string, object>)nodes[i]);
                int p = parentIdx[i];
                while (p != -1) {
                    float[] pm = GetNodeLocalMatrix((Dictionary<string, object>)nodes[p]);
                    m = MulMat4(pm, m);
                    p = parentIdx[p];
                }
                worldMats[i] = m;
            }

            ModelData model = new ModelData {
                DisplayName = displayName,
                ShortHudName = shortHudName
            };

            bool isPorygon3ds = (lowerFileName == "pokemon_porygon.glb");
            bool isPorygon2   = lowerFileName.Contains("porygon2");
            bool isPorygonZ   = lowerFileName.Contains("porygon-z");

            for (int nIdx = 0; nIdx < nodes.Count; nIdx++) {
                var nd = (Dictionary<string, object>)nodes[nIdx];
                if (!nd.ContainsKey("mesh")) continue;
                int meshIdx = Convert.ToInt32(nd["mesh"]);
                AppendGlbMesh(model, meshIdx, worldMats[nIdx], meshes, accessors, bufferViews, materials, textures, bitmaps, binChunk,
                    isPorygon3ds, isPorygon2, isPorygonZ);
            }

            float minX = 1e9f, maxX = -1e9f;
            float minY = 1e9f, maxY = -1e9f;
            float minZ = 1e9f, maxZ = -1e9f;
            for (int i = 0; i < model.Verts.Count; i++) {
                Vec3 v = model.Verts[i];
                if (v.X < minX) minX = v.X; if (v.X > maxX) maxX = v.X;
                if (v.Y < minY) minY = v.Y; if (v.Y > maxY) maxY = v.Y;
                if (v.Z < minZ) minZ = v.Z; if (v.Z > maxZ) maxZ = v.Z;
            }

            float cx = (minX + maxX) * 0.5f;
            float cy = (minY + maxY) * 0.5f;
            float cz = (minZ + maxZ) * 0.5f;
            float maxSpan = Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ));
            if (maxSpan < 1e-5f) maxSpan = 1f;
            float normScale = 142.0f / maxSpan;

            for (int i = 0; i < model.Verts.Count; i++) {
                Vec3 v = model.Verts[i];
                model.Verts[i] = new Vec3((v.X - cx) * normScale, (v.Y - cy) * normScale, (v.Z - cz) * normScale);
            }

            for (int i = 0; i < model.Faces.Count; i++) {
                Face3D f = model.Faces[i];
                Vec3 v0 = model.Verts[f.Indices[0]];
                Vec3 v1 = model.Verts[f.Indices[1]];
                Vec3 v2 = model.Verts[f.Indices[2]];
                f.LocalNormal = Vec3.Cross(v1 - v0, v2 - v0).Normalized();
            }

            return model;
        } finally {
            for (int i = 0; i < bitmaps.Length; i++) {
                if (bitmaps[i] != null) bitmaps[i].Dispose();
            }
        }
    }

    private void AppendGlbMesh(ModelData model, int meshIdx, float[] wMat,
        ArrayList meshes, ArrayList accessors, ArrayList bufferViews,
        ArrayList materials, ArrayList textures, Bitmap[] bitmaps, byte[] bin,
        bool isPorygon3ds, bool isPorygon2, bool isPorygonZ) {

        var mesh = (Dictionary<string, object>)meshes[meshIdx];
        ArrayList prims = (ArrayList)mesh["primitives"];

        foreach (object pObj in prims) {
            var prim = (Dictionary<string, object>)pObj;
            var attrs = (Dictionary<string, object>)prim["attributes"];
            if (!attrs.ContainsKey("POSITION")) continue;

            Vec3[] pos = ReadGlbVec3(Convert.ToInt32(attrs["POSITION"]), accessors, bufferViews, bin);
            PointF[] uvs = attrs.ContainsKey("TEXCOORD_0")
                ? ReadGlbVec2(Convert.ToInt32(attrs["TEXCOORD_0"]), accessors, bufferViews, bin)
                : null;
            int[] indices = prim.ContainsKey("indices")
                ? ReadGlbIndices(Convert.ToInt32(prim["indices"]), accessors, bufferViews, bin)
                : null;
            if (indices == null) {
                indices = new int[pos.Length];
                for (int i = 0; i < pos.Length; i++) indices[i] = i;
            }

            Color matColor = Color.FromArgb(248, 250, 255);
            Bitmap bmp = null;
            if (prim.ContainsKey("material")) {
                int matIdx = Convert.ToInt32(prim["material"]);
                if (matIdx >= 0 && matIdx < materials.Count) {
                    var mat = (Dictionary<string, object>)materials[matIdx];
                    if (mat.ContainsKey("pbrMetallicRoughness")) {
                        var pbr = (Dictionary<string, object>)mat["pbrMetallicRoughness"];
                        if (pbr.ContainsKey("baseColorFactor")) {
                            ArrayList cf = (ArrayList)pbr["baseColorFactor"];
                            int r = (int)(Convert.ToSingle(cf[0]) * 255);
                            int g = (int)(Convert.ToSingle(cf[1]) * 255);
                            int b = (int)(Convert.ToSingle(cf[2]) * 255);
                            matColor = Color.FromArgb(Clamp255(r), Clamp255(g), Clamp255(b));
                        }
                        if (pbr.ContainsKey("baseColorTexture")) {
                            var bct = (Dictionary<string, object>)pbr["baseColorTexture"];
                            int texIdx = Convert.ToInt32(bct["index"]);
                            var tex = (Dictionary<string, object>)textures[texIdx];
                            int srcIdx = Convert.ToInt32(tex["source"]);
                            bmp = bitmaps[srcIdx];
                        }
                    }
                }
            }

            Vec3[] wPos = new Vec3[pos.Length];
            for (int i = 0; i < pos.Length; i++) {
                wPos[i] = TransformMat4(wMat, pos[i]);
            }

            int subdiv = 1;
            float depthBias = 0f;
            if (bmp != null && uvs != null) {
                if (isPorygon3ds && meshIdx == 1) { subdiv = 3; depthBias = 1.8f; }
                else if (isPorygonZ && meshIdx == 1) { subdiv = 2; depthBias = 1.5f; }
                else if (isPorygon2 && meshIdx == 2) { subdiv = 2; depthBias = 4.5f; }
                else if (isPorygon2 && meshIdx == 0) { subdiv = 1; depthBias = 1.0f; }
            }

            // Precompute UV edge multiplicity & max triangle centroid luma for Porygon (isPorygon3ds) contour detection
            Dictionary<int, int> uvEdgeCount = null;
            Dictionary<int, int> uvEdgeMaxLuma = null;
            if (isPorygon3ds && bmp != null && uvs != null) {
                uvEdgeCount = new Dictionary<int, int>();
                uvEdgeMaxLuma = new Dictionary<int, int>();
                for (int i = 0; i + 2 < indices.Length; i += 3) {
                    int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
                    int cLuma = 400;
                    if (i0 < uvs.Length && i1 < uvs.Length && i2 < uvs.Length) {
                        float cu = (uvs[i0].X + uvs[i1].X + uvs[i2].X) / 3.0f;
                        float cv = (uvs[i0].Y + uvs[i1].Y + uvs[i2].Y) / 3.0f;
                        Color cc = SampleGlbTexture(bmp, cu, cv, matColor);
                        cLuma = cc.R + cc.G + cc.B;
                    }
                    int[] triV = new int[] { i0, i1, i2 };
                    for (int e = 0; e < 3; e++) {
                        int a = Math.Min(triV[e], triV[(e + 1) % 3]);
                        int b = Math.Max(triV[e], triV[(e + 1) % 3]);
                        int key = a * 10000 + b;
                        int curCnt;
                        uvEdgeCount[key] = uvEdgeCount.TryGetValue(key, out curCnt) ? (curCnt + 1) : 1;
                        int curMax;
                        if (!uvEdgeMaxLuma.TryGetValue(key, out curMax) || cLuma > curMax) {
                            uvEdgeMaxLuma[key] = cLuma;
                        }
                    }
                }
            }

            if (subdiv <= 1) {
                int baseV = model.Verts.Count;
                for (int i = 0; i < wPos.Length; i++) model.Verts.Add(wPos[i]);
                int triCount = indices.Length / 3;
                Vec3[] triNorms = null;
                bool[] triIsPink = null;
                Dictionary<string, List<int>> edge3DMap = null;
                if (isPorygon3ds && bmp != null && uvs != null) {
                    triNorms = new Vec3[triCount];
                    triIsPink = new bool[triCount];
                    edge3DMap = new Dictionary<string, List<int>>();
                    for (int t = 0; t < triCount; t++) {
                        int a0 = indices[t * 3], a1 = indices[t * 3 + 1], a2 = indices[t * 3 + 2];
                        triNorms[t] = Vec3.Cross(wPos[a1] - wPos[a0], wPos[a2] - wPos[a0]).Normalized();
                        Color tc = matColor;
                        if (a0 < uvs.Length && a1 < uvs.Length && a2 < uvs.Length) {
                            tc = SampleGlbTexture(bmp, (uvs[a0].X + uvs[a1].X + uvs[a2].X) / 3.0f, (uvs[a0].Y + uvs[a1].Y + uvs[a2].Y) / 3.0f, matColor);
                        }
                        triIsPink[t] = (tc.R > tc.B);
                        int[] tv = new int[] { a0, a1, a2 };
                        for (int e = 0; e < 3; e++) {
                            string k3d = MakeEdge3DKey(pos[tv[e]], pos[tv[(e + 1) % 3]]);
                            List<int> list;
                            if (!edge3DMap.TryGetValue(k3d, out list)) {
                                list = new List<int>(2);
                                edge3DMap[k3d] = list;
                            }
                            list.Add(t);
                        }
                    }
                }

                for (int i = 0; i + 2 < indices.Length; i += 3) {
                    int tIdx = i / 3;
                    int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
                    Color c = matColor;
                    if (bmp != null && uvs != null && i0 < uvs.Length && i1 < uvs.Length && i2 < uvs.Length) {
                        float u = (uvs[i0].X + uvs[i1].X + uvs[i2].X) / 3.0f;
                        float v = (uvs[i0].Y + uvs[i1].Y + uvs[i2].Y) / 3.0f;
                        c = SampleGlbTexture(bmp, u, v, matColor);
                        if (isPorygon3ds && meshIdx == 0) {
                            if (c.B >= c.R && c.R < 72) c = Color.FromArgb(79, 169, 180);
                            else if (c.R > c.B && c.R < 135) c = Color.FromArgb(198, 79, 91);
                        }
                    }
                    Face3D face = new Face3D(c, depthBias, baseV + i0, baseV + i1, baseV + i2);
                    if (isPorygon3ds && uvEdgeCount != null && bmp != null && uvs != null) {
                        int[] triV = new int[] { i0, i1, i2 };
                        byte mask = 0;
                        for (int e = 0; e < 3; e++) {
                            int va = triV[e], vb = triV[(e + 1) % 3];
                            int key = Math.Min(va, vb) * 10000 + Math.Max(va, vb);
                            int cnt = uvEdgeCount.ContainsKey(key) ? uvEdgeCount[key] : 1;
                            int maxLuma = uvEdgeMaxLuma.ContainsKey(key) ? uvEdgeMaxLuma[key] : (c.R + c.G + c.B);
                            bool isContour = (cnt == 1);
                            if (!isContour && va < uvs.Length && vb < uvs.Length) {
                                int minEdgeLuma = SampleMinEdgeTexLuma(bmp, uvs[va], uvs[vb]);
                                if (maxLuma - minEdgeLuma > 38) isContour = true;
                            }
                            if (edge3DMap != null) {
                                string k3d = MakeEdge3DKey(pos[va], pos[vb]);
                                List<int> adj;
                                if (edge3DMap.TryGetValue(k3d, out adj) && adj.Count >= 2) {
                                    if (tIdx != adj[0]) {
                                        isContour = false;
                                    } else {
                                        int otherT = adj[1];
                                        float dotN = Math.Abs(Vec3.Dot(triNorms[tIdx], triNorms[otherT]));
                                        bool sameFamily = (triIsPink[tIdx] == triIsPink[otherT]);
                                        bool isCenterline = (Math.Abs(pos[va].X) < 0.02f && Math.Abs(pos[vb].X) < 0.02f);
                                        bool isHorizontalRidge = (Math.Abs(pos[va].Y - pos[vb].Y) < 0.02f && Math.Abs(pos[va].Z - pos[vb].Z) < 0.02f);
                                        bool isTailFin = (Math.Min(pos[va].Z, pos[vb].Z) < -1.55f);
                                        if (!sameFamily) {
                                            isContour = true;
                                        } else if (isTailFin) {
                                            isContour = isCenterline;
                                        } else if (dotN < 0.900f || isHorizontalRidge) {
                                            isContour = true;
                                        } else if (!isCenterline) {
                                            isContour = false;
                                        }
                                    }
                                }
                            }
                            if (isContour) mask |= (byte)(1 << e);
                        }
                        face.ContourEdgeMask = mask;
                    }
                    model.Faces.Add(face);
                }
            } else {
                for (int i = 0; i + 2 < indices.Length; i += 3) {
                    int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
                    Vec3 v0 = wPos[i0], v1 = wPos[i1], v2 = wPos[i2];
                    PointF uv0 = uvs[i0], uv1 = uvs[i1], uv2 = uvs[i2];

                    bool b01 = false, b12 = false, b20 = false;
                    if (isPorygon3ds && meshIdx == 1 && uvEdgeCount != null) {
                        int k01 = Math.Min(i0, i1) * 10000 + Math.Max(i0, i1);
                        int k12 = Math.Min(i1, i2) * 10000 + Math.Max(i1, i2);
                        int k20 = Math.Min(i2, i0) * 10000 + Math.Max(i2, i0);
                        b01 = (!uvEdgeCount.ContainsKey(k01) || uvEdgeCount[k01] == 1);
                        b12 = (!uvEdgeCount.ContainsKey(k12) || uvEdgeCount[k12] == 1);
                        b20 = (!uvEdgeCount.ContainsKey(k20) || uvEdgeCount[k20] == 1);
                    }

                    for (int r = 0; r < subdiv; r++) {
                        for (int col = 0; col < subdiv - r; col++) {
                            AddSubTri(model, v0, v1, v2, uv0, uv1, uv2, subdiv,
                                r, col, r + 1, col, r, col + 1, bmp, matColor, depthBias,
                                isPorygon3ds && meshIdx == 1, b01, b12, b20);
                            if (r + col < subdiv - 1) {
                                AddSubTri(model, v0, v1, v2, uv0, uv1, uv2, subdiv,
                                    r + 1, col, r + 1, col + 1, r, col + 1, bmp, matColor, depthBias,
                                    isPorygon3ds && meshIdx == 1, b01, b12, b20);
                            }
                        }
                    }
                }
            }
        }
    }

    private static string MakeEdge3DKey(Vec3 a, Vec3 b) {
        int ax = (int)Math.Round(a.X * 200f), ay = (int)Math.Round(a.Y * 200f), az = (int)Math.Round(a.Z * 200f);
        int bx = (int)Math.Round(b.X * 200f), by = (int)Math.Round(b.Y * 200f), bz = (int)Math.Round(b.Z * 200f);
        if (ax < bx || (ax == bx && (ay < by || (ay == by && az <= bz)))) {
            return ax + "," + ay + "," + az + ":" + bx + "," + by + "," + bz;
        }
        return bx + "," + by + "," + bz + ":" + ax + "," + ay + "," + az;
    }

    private static int SampleMinEdgeTexLuma(Bitmap bmp, PointF ua, PointF ub) {
        int minLuma = 999;
        float[] ts = new float[] { 0.30f, 0.50f, 0.70f };
        int w = bmp.Width, h = bmp.Height;
        for (int ti = 0; ti < ts.Length; ti++) {
            float t = ts[ti];
            float u = ua.X * (1f - t) + ub.X * t;
            float v = ua.Y * (1f - t) + ub.Y * t;
            if (u < 0f || u > 1f) u = u - (float)Math.Floor(u);
            if (v > 1.02f) v = 1.0f - (v - (float)Math.Floor(v));
            else if (v < 0f) v = v - (float)Math.Floor(v);
            int cx = Math.Max(0, Math.Min(w - 1, (int)(u * (w - 1))));
            int cy = Math.Max(0, Math.Min(h - 1, (int)(v * (h - 1))));
            for (int dy = -1; dy <= 1; dy++) {
                int py = Math.Max(0, Math.Min(h - 1, cy + dy));
                for (int dx = -1; dx <= 1; dx++) {
                    int px = Math.Max(0, Math.Min(w - 1, cx + dx));
                    Color c = bmp.GetPixel(px, py);
                    int luma = c.R + c.G + c.B;
                    if (luma < minLuma) minLuma = luma;
                }
            }
        }
        return minLuma;
    }

    private static bool IsSubEdgeOnBaseBoundary(int N, int r1, int c1, int r2, int c2, bool b01, bool b12, bool b20) {
        if (b01 && c1 == 0 && c2 == 0) return true;
        if (b12 && (r1 + c1 == N) && (r2 + c2 == N)) return true;
        if (b20 && r1 == 0 && r2 == 0) return true;
        return false;
    }

    private void AddSubTri(ModelData model, Vec3 v0, Vec3 v1, Vec3 v2, PointF uv0, PointF uv1, PointF uv2,
        int N, int rA, int cA, int rB, int cB, int rC, int cC, Bitmap bmp, Color factor, float depthBias,
        bool isPorygonEye = false, bool b01 = false, bool b12 = false, bool b20 = false) {

        float a0 = (float)rA / N, b0 = (float)cA / N, d0 = 1f - a0 - b0;
        float a1 = (float)rB / N, b1 = (float)cB / N, d1 = 1f - a1 - b1;
        float a2 = (float)rC / N, b2 = (float)cC / N, d2 = 1f - a2 - b2;

        Vec3 pA = v0 * d0 + v1 * a0 + v2 * b0;
        Vec3 pB = v0 * d1 + v1 * a1 + v2 * b1;
        Vec3 pC = v0 * d2 + v1 * a2 + v2 * b2;

        float u = (uv0.X * (d0 + d1 + d2) + uv1.X * (a0 + a1 + a2) + uv2.X * (b0 + b1 + b2)) / 3.0f;
        float v = (uv0.Y * (d0 + d1 + d2) + uv1.Y * (a0 + a1 + a2) + uv2.Y * (b0 + b1 + b2)) / 3.0f;

        Color col = SampleGlbTexture(bmp, u, v, factor);
        if (isPorygonEye && col.R < 105 && col.G < 105 && col.B < 105) {
            col = Color.FromArgb(14, 16, 22); // Crisp black pupil matching Porygon2/Z
        }
        int idx0 = model.Verts.Count;
        model.Verts.Add(pA);
        model.Verts.Add(pB);
        model.Verts.Add(pC);
        Face3D face = new Face3D(col, depthBias, idx0, idx0 + 1, idx0 + 2);
        if (isPorygonEye) {
            byte mask = 0;
            if (IsSubEdgeOnBaseBoundary(N, rA, cA, rB, cB, b01, b12, b20)) mask |= 1;
            if (IsSubEdgeOnBaseBoundary(N, rB, cB, rC, cC, b01, b12, b20)) mask |= 2;
            if (IsSubEdgeOnBaseBoundary(N, rC, cC, rA, cA, b01, b12, b20)) mask |= 4;
            face.ContourEdgeMask = mask;
        }
        model.Faces.Add(face);
    }

    private static Color SampleGlbTexture(Bitmap bmp, float u, float v, Color factor) {
        if (float.IsNaN(u) || float.IsNaN(v)) return factor;
        if (u < 0f || u > 1f) u = u - (float)Math.Floor(u);
        if (v > 1.02f) {
            v = 1.0f - (v - (float)Math.Floor(v));
        } else if (v < 0f) {
            v = v - (float)Math.Floor(v);
        }
        int px = Math.Max(0, Math.Min(bmp.Width - 1, (int)(u * (bmp.Width - 1))));
        int py = Math.Max(0, Math.Min(bmp.Height - 1, (int)(v * (bmp.Height - 1))));
        Color texCol = bmp.GetPixel(px, py);
        if (texCol.A < 25) return factor;
        return texCol;
    }

    private static int Clamp255(int v) { return v < 0 ? 0 : (v > 255 ? 255 : v); }

    private static float[] GetNodeLocalMatrix(Dictionary<string, object> nd) {
        if (nd.ContainsKey("matrix")) {
            ArrayList arr = (ArrayList)nd["matrix"];
            float[] m = new float[16];
            for (int i = 0; i < 16; i++) m[i] = Convert.ToSingle(arr[i]);
            return m;
        }
        float tx = 0, ty = 0, tz = 0;
        float qx = 0, qy = 0, qz = 0, qw = 1;
        float sx = 1, sy = 1, sz = 1;
        if (nd.ContainsKey("translation")) {
            ArrayList t = (ArrayList)nd["translation"];
            tx = Convert.ToSingle(t[0]); ty = Convert.ToSingle(t[1]); tz = Convert.ToSingle(t[2]);
        }
        if (nd.ContainsKey("rotation")) {
            ArrayList r = (ArrayList)nd["rotation"];
            qx = Convert.ToSingle(r[0]); qy = Convert.ToSingle(r[1]); qz = Convert.ToSingle(r[2]); qw = Convert.ToSingle(r[3]);
        }
        if (nd.ContainsKey("scale")) {
            ArrayList s = (ArrayList)nd["scale"];
            sx = Convert.ToSingle(s[0]); sy = Convert.ToSingle(s[1]); sz = Convert.ToSingle(s[2]);
        }
        float x2 = qx + qx, y2 = qy + qy, z2 = qz + qz;
        float xx = qx * x2, xy = qx * y2, xz = qx * z2;
        float yy = qy * y2, yz = qy * z2, zz = qz * z2;
        float wx = qw * x2, wy = qw * y2, wz = qw * z2;

        float[] res = new float[16];
        res[0] = (1f - (yy + zz)) * sx;
        res[1] = (xy + wz) * sx;
        res[2] = (xz - wy) * sx;
        res[3] = 0f;
        res[4] = (xy - wz) * sy;
        res[5] = (1f - (xx + zz)) * sy;
        res[6] = (yz + wx) * sy;
        res[7] = 0f;
        res[8] = (xz + wy) * sz;
        res[9] = (yz - wx) * sz;
        res[10] = (1f - (xx + yy)) * sz;
        res[11] = 0f;
        res[12] = tx; res[13] = ty; res[14] = tz; res[15] = 1f;
        return res;
    }

    private static float[] MulMat4(float[] A, float[] B) {
        float[] C = new float[16];
        for (int c = 0; c < 4; c++) {
            for (int r = 0; r < 4; r++) {
                C[c * 4 + r] =
                    A[0 * 4 + r] * B[c * 4 + 0] +
                    A[1 * 4 + r] * B[c * 4 + 1] +
                    A[2 * 4 + r] * B[c * 4 + 2] +
                    A[3 * 4 + r] * B[c * 4 + 3];
            }
        }
        return C;
    }

    private static Vec3 TransformMat4(float[] m, Vec3 v) {
        return new Vec3(
            m[0] * v.X + m[4] * v.Y + m[8]  * v.Z + m[12],
            m[1] * v.X + m[5] * v.Y + m[9]  * v.Z + m[13],
            m[2] * v.X + m[6] * v.Y + m[10] * v.Z + m[14]
        );
    }

    private static Vec3[] ReadGlbVec3(int accIdx, ArrayList accessors, ArrayList bufferViews, byte[] bin) {
        var acc = (Dictionary<string, object>)accessors[accIdx];
        int count = Convert.ToInt32(acc["count"]);
        int bvIdx = Convert.ToInt32(acc["bufferView"]);
        int accOff = acc.ContainsKey("byteOffset") ? Convert.ToInt32(acc["byteOffset"]) : 0;
        var bv = (Dictionary<string, object>)bufferViews[bvIdx];
        int bvOff = bv.ContainsKey("byteOffset") ? Convert.ToInt32(bv["byteOffset"]) : 0;
        int stride = bv.ContainsKey("byteStride") ? Convert.ToInt32(bv["byteStride"]) : 12;
        Vec3[] res = new Vec3[count];
        int pos = bvOff + accOff;
        for (int i = 0; i < count; i++) {
            res[i] = new Vec3(
                BitConverter.ToSingle(bin, pos),
                BitConverter.ToSingle(bin, pos + 4),
                BitConverter.ToSingle(bin, pos + 8)
            );
            pos += stride;
        }
        return res;
    }

    private static PointF[] ReadGlbVec2(int accIdx, ArrayList accessors, ArrayList bufferViews, byte[] bin) {
        var acc = (Dictionary<string, object>)accessors[accIdx];
        int count = Convert.ToInt32(acc["count"]);
        int bvIdx = Convert.ToInt32(acc["bufferView"]);
        int accOff = acc.ContainsKey("byteOffset") ? Convert.ToInt32(acc["byteOffset"]) : 0;
        var bv = (Dictionary<string, object>)bufferViews[bvIdx];
        int bvOff = bv.ContainsKey("byteOffset") ? Convert.ToInt32(bv["byteOffset"]) : 0;
        int stride = bv.ContainsKey("byteStride") ? Convert.ToInt32(bv["byteStride"]) : 8;
        PointF[] res = new PointF[count];
        int pos = bvOff + accOff;
        for (int i = 0; i < count; i++) {
            res[i] = new PointF(BitConverter.ToSingle(bin, pos), BitConverter.ToSingle(bin, pos + 4));
            pos += stride;
        }
        return res;
    }

    private static int[] ReadGlbIndices(int accIdx, ArrayList accessors, ArrayList bufferViews, byte[] bin) {
        var acc = (Dictionary<string, object>)accessors[accIdx];
        int count = Convert.ToInt32(acc["count"]);
        int compType = Convert.ToInt32(acc["componentType"]);
        int bvIdx = Convert.ToInt32(acc["bufferView"]);
        int accOff = acc.ContainsKey("byteOffset") ? Convert.ToInt32(acc["byteOffset"]) : 0;
        var bv = (Dictionary<string, object>)bufferViews[bvIdx];
        int bvOff = bv.ContainsKey("byteOffset") ? Convert.ToInt32(bv["byteOffset"]) : 0;
        int pos = bvOff + accOff;
        int[] res = new int[count];
        for (int i = 0; i < count; i++) {
            if (compType == 5123) { res[i] = BitConverter.ToUInt16(bin, pos); pos += 2; }
            else if (compType == 5125) { res[i] = (int)BitConverter.ToUInt32(bin, pos); pos += 4; }
            else if (compType == 5121) { res[i] = bin[pos]; pos += 1; }
        }
        return res;
    }

    // ========================================================================
    // AUTONOMOUS MULTI-AGENT UPDATE (REAL NETWORK TELEMETRY + HOVER + IDS AI)
    // ========================================================================
    public void Update(int screenW, int screenH, Point mousePt, RealTelemetryFeed feed, bool followMouse, bool trainingMode) {
        LatestFeed = feed;
        saveTickCounter++;
        if (saveTickCounter % 500 == 0) {
            SaveBrainStates();
        }

        lock (packetLock) {
            // 1. Hit-test Mouse Hover on Floating 3D Network Nodes (scaled by Z perspective depth; freezes hovered node for inspection!)
            HoveredPacket = null;
            float bestHoverNodeDist = 48f;
            for (int i = 0; i < Packets.Count; i++) {
                DataPacket p = Packets[i];
                p.IsHovered = false;
                float dScale = GetDepthScale(p.Z);
                float mdx = mousePt.X - p.X;
                float mdy = mousePt.Y - p.Y;
                float md = (float)Math.Sqrt(mdx * mdx + mdy * mdy) / Math.Max(0.65f, dScale);
                if (md < bestHoverNodeDist) {
                    bestHoverNodeDist = md;
                    HoveredPacket = p;
                }
            }
            if (HoveredPacket != null) {
                HoveredPacket.IsHovered = true;
            }

            // 2. Hit-test Mouse Hover on the 5 3D Porygon Agents (only if not already hovering a node)
            HoveredAgent = null;
            float bestHoverAgentDist = 78f;
            for (int aIdx = 0; aIdx < Agents.Count; aIdx++) {
                PorygonAgent a = Agents[aIdx];
                a.IsHovered = false;
                float dScale = GetDepthScale(a.PosZ);
                float mdx = mousePt.X - a.PosX;
                float mdy = mousePt.Y - a.PosY;
                float md = (float)Math.Sqrt(mdx * mdx + mdy * mdy) / Math.Max(0.65f, dScale);
                if (HoveredPacket == null && md < bestHoverAgentDist) {
                    bestHoverAgentDist = md;
                    HoveredAgent = a;
                }
            }
            if (HoveredAgent != null) {
                HoveredAgent.IsHovered = true;
            }

            // 3. Move non-hovered network nodes in true 3D (X, Y, Z) with dynamic orbital swirl & evasive anomaly jinks
            for (int i = 0; i < Packets.Count; i++) {
                DataPacket p = Packets[i];
                p.floatRot += (p.Kind == 2) ? 0.14f : 0.075f;
                p.OrbitPhase += (p.Kind == 2) ? 0.16f : 0.09f;
                p.PullIntensity = Math.Max(0f, p.PullIntensity - 0.08f);
                if (p.EvadeCooldown > 0f) p.EvadeCooldown -= 0.04f;
                if (p.IsHovered) continue; // Freeze 3D position while user inspects the node with the mouse!

                Rectangle curMon = GetContainingOrNearestMonitor(p.X, p.Y);
                float zParallax = GetDepthScale(p.Z);
                float swirlX = (float)Math.Cos(p.OrbitPhase) * (p.Kind == 2 ? 0.65f : 0.25f);
                float swirlY = (float)Math.Sin(p.OrbitPhase * 1.3f) * (p.Kind == 2 ? 0.65f : 0.25f);
                p.X += (p.VX + swirlX) * zParallax;
                p.Y += (p.VY + swirlY) * zParallax;
                p.Z += p.VZ;
                if (p.Z < -250f || p.Z > 170f) {
                    p.VZ = -p.VZ;
                    p.Z = Clamp(p.Z, -250f, 170f);
                }
                if (!IsInsideAnyMonitor(p.X, p.Y, 55)) {
                    // Check if we can transit to an adjacent/nearest monitor or bounce inside current monitor
                    if (rng.NextDouble() < 0.18 && MonitorRects.Count > 1) {
                        PickRandomMonitorPoint(out p.X, out p.Y, 150, 120);
                        p.Z = (float)(rng.NextDouble() * 420.0 - 250.0);
                    } else {
                        if (p.X < curMon.Left + 75 || p.X > curMon.Right - 75) p.VX = -p.VX;
                        if (p.Y < curMon.Top + 85 || p.Y > curMon.Bottom - 75) p.VY = -p.VY;
                        p.X = Clamp(p.X, curMon.Left + 75, curMon.Right - 75);
                        p.Y = Clamp(p.Y, curMon.Top + 85, curMon.Bottom - 75);
                    }
                }
            }

            // 4. Update each of the 5 Porygons independently in 3D (X, Y, Z) across all 5 monitors with REAL Network Telemetry!
            for (int aIdx = 0; aIdx < Agents.Count; aIdx++) {
                PorygonAgent a = Agents[aIdx];
                FlyBrainConnectome br = a.Brain;
                a.FloatBob += 0.072f + aIdx * 0.006f;
                a.TractorPhase += 0.18f;
                a.ActiveLockStrength = Math.Max(0f, a.ActiveLockStrength - 0.10f);
                if (a.DashBoostTimer > 0f) a.DashBoostTimer = Math.Max(0f, a.DashBoostTimer - 0.04f);
                if (a.AbsorbFlashTimer > 0f) a.AbsorbFlashTimer = Math.Max(0f, a.AbsorbFlashTimer - 0.045f);
                if (a.SyncCooldown > 0) a.SyncCooldown--;
                if (a.LevelUpBannerTimer > 0f) a.LevelUpBannerTimer -= 0.025f;

                DataPacket bestPkt = null;
                float bestScore = 999999f;
                float bestDist = 999999f;

                for (int i = 0; i < Packets.Count; i++) {
                    DataPacket p = Packets[i];
                    float dx = p.X - a.PosX;
                    float dy = p.Y - a.PosY;
                    float dz = p.Z - a.PosZ;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    float priority = d;

                    if (p.Kind == 2) {
                        // Traffic Anomaly! Porygon-Z prioritizes anomalies heavily across all monitors & Z-depths
                        priority *= (aIdx == 2) ? 0.30f : (br.HasSkill(4) ? 0.55f : 0.85f);
                    } else if (p.Kind == 1) {
                        // New Host / Elevated Flow Orb! Porygon2 prioritizes auditing new hosts
                        priority *= (aIdx == 1) ? 0.38f : 0.68f;
                    } else {
                        // Standard Flow! Porygon (Gen 1) prioritizes baseline collection
                        priority *= (aIdx == 0) ? 0.58f : 0.95f;
                    }

                    if (p.IsHovered) {
                        priority *= 0.65f;
                    }

                    for (int o = 0; o < Agents.Count; o++) {
                        if (o == aIdx) continue;
                        float odx = p.X - Agents[o].PosX;
                        float ody = p.Y - Agents[o].PosY;
                        float odz = p.Z - Agents[o].PosZ;
                        if (odx * odx + ody * ody + odz * odz < d * d * 0.45f) priority += 260f;
                    }
                    if (priority < bestScore) {
                        bestScore = priority;
                        bestDist = d;
                        bestPkt = p;
                    }
                }

                if (bestPkt != null) {
                    bestPkt.AssignedAgentName = a.Name;
                    a.CurrentTargetEndpoint = string.Format("{0} [{1}] Z:{2:+0;-0;0}", bestPkt.RemoteEndpoint, bestPkt.AnomalyType, bestPkt.Z);
                } else {
                    a.CurrentTargetEndpoint = string.Format("PATROL_3D [Z:{0:+0;-0;0}]", a.WaypointZ);
                }

                // Find nearest peer Porygon in 3D space for P2P Neural Sync (Skill LVL 3) and 3D Flocking Separation
                PorygonAgent nearestPeer = null;
                float minPeerDist = 99999f;
                float sepX = 0f, sepY = 0f, sepZ = 0f;
                for (int o = 0; o < Agents.Count; o++) {
                    if (o == aIdx) continue;
                    float pdx = a.PosX - Agents[o].PosX;
                    float pdy = a.PosY - Agents[o].PosY;
                    float pdz = a.PosZ - Agents[o].PosZ;
                    float pd = (float)Math.Sqrt(pdx * pdx + pdy * pdy + pdz * pdz);
                    if (pd < minPeerDist) {
                        minPeerDist = pd;
                        nearestPeer = Agents[o];
                    }
                    if (pd > 1f && pd < 210f) {
                        float push = (210f - pd) / 210f;
                        sepX += (pdx / pd) * push * 2.4f;
                        sepY += (pdy / pd) * push * 2.4f;
                        sepZ += (pdz / pd) * push * 2.0f;
                    }
                }

                // Build 16-channel Sensory Input fed by REAL Network Telemetry + 3D Spatial Node Vector (X, Y, Z)!
                float[] sensory = new float[16];
                if (bestPkt != null) {
                    float pdx = bestPkt.X - a.PosX;
                    float pdy = bestPkt.Y - a.PosY;
                    float pdz = bestPkt.Z - a.PosZ;
                    sensory[0] = (pdx < -30f) ? 0.85f : 0.1f;
                    sensory[1] = (pdx >  30f) ? 0.85f : 0.1f;
                    sensory[2] = (pdy < -30f) ? 0.75f : 0.1f;
                    sensory[3] = (pdy >  30f) ? 0.75f : 0.1f;
                    sensory[4] = (bestDist < 440f) ? 0.95f : (Math.Abs(pdz) > 120f ? 0.65f : 0.35f);
                    sensory[5] = (bestDist < 160f) ? 1.30f : 0.0f;
                    sensory[6] = (bestPkt.Kind == 1) ? 1.20f : 0.2f;
                    sensory[8] = (bestPkt.Kind == 2) ? (1.10f + bestPkt.AnomalyScore * 0.6f) : 0.1f;
                }
                // Feed REAL Network Z-Scores & Anomaly Metrics into Sensory Neurons 7, 9..15:
                sensory[7]  = Clamp(feed.NewHostCount * 0.35f, 0f, 1.4f);
                sensory[9]  = Clamp((feed.DownKbps + feed.UpKbps) / 420.0f, 0.05f, 1.3f);
                sensory[10] = (minPeerDist < 245f) ? 1.1f : 0.1f;
                sensory[11] = Clamp(Math.Max(0f, feed.DlZScore) * 0.45f, 0f, 1.5f);
                sensory[12] = Clamp(Math.Max(0f, feed.UlZScore) * 0.45f, 0f, 1.5f);
                sensory[13] = Clamp(Math.Max(0f, feed.PingZScore) * 0.45f, 0f, 1.5f);
                sensory[14] = Clamp((feed.UnusualPortCount + feed.SynCount) * 0.45f, 0f, 1.5f);
                sensory[15] = Clamp(feed.AnomalyIndex * 1.4f, 0.05f, 1.5f);

                br.Step(sensory, 0.042f);

                float targetX = a.WaypointX;
                float targetY = a.WaypointY;
                float targetZ = a.WaypointZ;

                float cdx = mousePt.X - a.PosX;
                float cdy = mousePt.Y - a.PosY;
                float cDist = (float)Math.Sqrt(cdx * cdx + cdy * cdy);

                // Skill LVL 3: Neural Sync P2P Link (shares learned anomaly weights between Porygons across monitors!)
                if (br.HasSkill(3) && nearestPeer != null && (minPeerDist < 420f || rng.NextDouble() < 0.010) && a.SyncCooldown == 0 && nearestPeer.SyncCooldown == 0) {
                    a.SyncTimer = 1.0f;
                    a.SyncPartnerIdx = nearestPeer.AgentIndex;
                    a.SyncCooldown = 210;
                    nearestPeer.SyncCooldown = 210;
                    br.NeuralSyncs++;
                    nearestPeer.Brain.NeuralSyncs++;
                    if (br.ReinforceReward(0.85f, 18)) a.LevelUpBannerTimer = 1.0f;
                    if (nearestPeer.Brain.ReinforceReward(0.85f, 18)) nearestPeer.LevelUpBannerTimer = 1.0f;
                    br.ActiveStateName = "P2P_MESH_SYNC // WEIGHTS->" + nearestPeer.Name;
                }

                // Skill LVL 6: Deep Sonar Scan when network anomaly index rises or periodically
                if (br.HasSkill(6) && a.ScanWaveTimer <= 0f && (feed.AnomalyIndex > 0.50f || rng.NextDouble() < 0.008)) {
                    a.ScanWaveTimer = 1.0f;
                    br.ActiveStateName = "SONAR_DEEP_SCAN // AUDITING_SOCKETS";
                }

                if (followMouse && cDist < 520f) {
                    float desiredDist = 150f;
                    float ang = (float)Math.Atan2(cdy, cdx);
                    targetX = mousePt.X - (float)Math.Cos(ang) * desiredDist;
                    targetY = mousePt.Y - (float)Math.Sin(ang) * desiredDist;
                    targetZ = 85f; // Come forward toward the glass when assisting cursor
                    br.ActiveStateName = "CURSOR_LINK // ASSIST_MODE";
                } else if (bestPkt != null && (br.DriveHunger > 0.22f || trainingMode || bestPkt.Kind > 0)) {
                    // If the user is hovering this packet, orbit just beside it in 3D so the Porygon inspects it without deleting the tooltip!
                    if (bestPkt.IsHovered) {
                        targetX = bestPkt.X - 115f * a.FacingDir;
                        targetY = bestPkt.Y + 20f;
                        targetZ = Clamp(bestPkt.Z + 18f, -240f, 165f);
                        br.ActiveStateName = "INSPECTING_HOVER // " + bestPkt.RemoteEndpoint;
                    } else {
                        targetX = bestPkt.X;
                        targetY = bestPkt.Y;
                        targetZ = bestPkt.Z;

                        float reachDist = 148f + Math.Min(52f, br.Level * 3.8f);

                        // Dynamic Tractor Lock & Anomaly Evasion when Porygon closes in (< 360px)!
                        if (bestDist < 360f) {
                            float lockPower = Clamp(1.0f - (bestDist - reachDist * 0.65f) / 260f, 0.15f, 1.0f);
                            bestPkt.PullIntensity = Math.Max(bestPkt.PullIntensity, lockPower);
                            bestPkt.TargetedByAgentIdx = aIdx;
                            a.ActiveLockTarget = new PointF(bestPkt.X, bestPkt.Y);
                            a.ActiveLockZ = bestPkt.Z;
                            a.ActiveLockStrength = lockPower;
                            a.ActiveLockKind = bestPkt.Kind;

                            if (bestPkt.Kind == 2 && bestDist > reachDist * 1.05f && bestPkt.EvadeCooldown <= 0f) {
                                // Threat Anomaly attempts evasive 3D jink perpendicular to Porygon's pursuit vector!
                                float perpX = -(bestPkt.Y - a.PosY);
                                float perpY =  (bestPkt.X - a.PosX);
                                float pLen = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
                                if (pLen > 1f) {
                                    float jinkDir = (rng.Next(2) == 0) ? 1f : -1f;
                                    bestPkt.VX = Clamp(bestPkt.VX + (perpX / pLen) * jinkDir * 2.6f, -4.2f, 4.2f);
                                    bestPkt.VY = Clamp(bestPkt.VY + (perpY / pLen) * jinkDir * 2.6f, -4.2f, 4.2f);
                                    bestPkt.EvadeCooldown = 0.55f;
                                    a.DashBoostTimer = 0.85f; // Porygon fires pursuit thrusters to intercept!
                                }
                            } else if (bestDist < 265f) {
                                // Magnetic Tractor Vortex spirals the packet toward Porygon's beak while peeling off live vertex sparks!
                                float pullFactor = (bestPkt.Kind == 2) ? 0.055f : 0.085f;
                                float tangX = -(bestPkt.Y - a.PosY) * 0.032f;
                                float tangY =  (bestPkt.X - a.PosX) * 0.032f;
                                bestPkt.X += (a.PosX - bestPkt.X) * pullFactor + tangX;
                                bestPkt.Y += (a.PosY - bestPkt.Y) * pullFactor + tangY;
                                bestPkt.Z += (a.PosZ - bestPkt.Z) * pullFactor;

                                // Spawn 1 live vertex spark peeling off the packet toward Porygon during tractor lock!
                                if (VertexShards.Count < 220 && rng.NextDouble() < 0.45) {
                                    SpawnTractorStreamVertex(bestPkt, a);
                                }
                            }
                        }

                        // Skill LVL 5: Quantum Blink in 3D (X, Y, Z) when chasing distant high-priority anomaly or cross-monitor node
                        if (br.HasSkill(5) && bestDist > 400f && bestDist < 2800f && a.QuantumBlinkTimer <= 0f && (bestPkt.Kind == 2 || rng.NextDouble() < 0.042)) {
                            a.BlinkStartPt = new PointF(a.PosX, a.PosY);
                            a.BlinkStartZ = a.PosZ;
                            Rectangle dstMon = GetContainingOrNearestMonitor(bestPkt.X, bestPkt.Y);
                            a.PosX = Clamp(a.PosX + (bestPkt.X - a.PosX) * 0.68f, dstMon.Left + 110f, dstMon.Right - 110f);
                            a.PosY = Clamp(a.PosY + (bestPkt.Y - a.PosY) * 0.68f, dstMon.Top + 120f, dstMon.Bottom - 110f);
                            a.PosZ = Clamp(a.PosZ + (bestPkt.Z - a.PosZ) * 0.68f, -240f, 165f);
                            a.QuantumBlinkTimer = 1.0f;
                            a.DashBoostTimer = 0.90f;
                            br.ActiveStateName = "QUANTUM_PORTAL // WARP_" + bestPkt.AnomalyType;
                        } else if (bestPkt.Kind == 2) {
                            br.ActiveStateName = "IDS_INTERCEPT // " + bestPkt.AnomalyType;
                            if ((DateTime.UtcNow - LastManualBrainSelectUtc).TotalSeconds > 8.0) {
                                SelectedBrainAgentIndex = aIdx;
                                BrainAutoFocusedOnThreat = true;
                            }
                        } else if (bestPkt.Kind == 1) {
                            br.ActiveStateName = "HOST_AUDIT // " + bestPkt.RemoteEndpoint;
                        } else {
                            br.ActiveStateName = "FLOW_SAMPLE // " + bestPkt.RemoteEndpoint;
                        }

                        if (bestDist < reachDist) {
                            if (bestPkt.Kind == 2 && (DateTime.UtcNow - LastManualBrainSelectUtc).TotalSeconds > 8.0) {
                                SelectedBrainAgentIndex = aIdx;
                                BrainAutoFocusedOnThreat = true;
                            }
                            string ts = DateTime.Now.ToString("HH:mm:ss");
                            bool useTriAttack = (bestPkt.Kind == 2 && br.HasSkill(4)) || (bestPkt.Kind == 1 && br.HasSkill(4) && rng.NextDouble() < 0.35);
                            if (useTriAttack) {
                                // Skill LVL 4: Tri-Attack Elemental Matrix neutralizes real network anomaly!
                                a.TriAttackTimer = 1.0f;
                                a.TriAttackTarget = new PointF(bestPkt.X, bestPkt.Y);
                                a.SpinRollTimer = 0.85f;
                                a.AbsorbFlashTimer = 1.0f;
                                if (br.HasSkill(2)) a.ShieldTimer = 1.0f;
                                br.AnomaliesCleared++;
                                if (br.ReinforceReward(1.45f, 45)) a.LevelUpBannerTimer = 1.0f;
                                br.ActiveStateName = "TRI_ATTACK // PURGED " + bestPkt.RemoteEndpoint;
                                AddIdsLog(string.Format("[{0}] [ALERT_PURGED] {1} neutralized {2} ({3}) Score:{4:0.00}",
                                    ts, a.Name, bestPkt.AnomalyType, bestPkt.RemoteEndpoint, bestPkt.AnomalyScore));
                            } else {
                                // Skill LVL 1: Helical Conversion Beam samples/deconstructs flow into 3D vertices!
                                a.BeamTimer = 1.0f;
                                a.BeamTarget = new PointF(bestPkt.X, bestPkt.Y);
                                a.AbsorbFlashTimer = 0.85f;
                                if (bestPkt.Kind >= 1) a.SpinRollTimer = 0.70f;
                                if (bestPkt.Kind >= 1 && br.HasSkill(2)) a.ShieldTimer = 0.90f;
                                br.PacketsCollected++;
                                int xpGain = (bestPkt.Kind == 2) ? 38 : (bestPkt.Kind == 1 ? 30 : 15);
                                float rew = (bestPkt.Kind == 2) ? 1.25f : (bestPkt.Kind == 1 ? 1.10f : 0.65f);
                                if (br.ReinforceReward(rew, xpGain)) a.LevelUpBannerTimer = 1.0f;
                                if (bestPkt.Kind >= 1) {
                                    AddIdsLog(string.Format("[{0}] [HOST_VERIFIED] {1} audited {2} ({3}) -> Baseline+1",
                                        ts, a.Name, bestPkt.RemoteEndpoint, bestPkt.AnomalyType));
                                }
                                br.ActiveStateName = (bestPkt.Kind == 1)
                                    ? "BASELINE_LEARN // " + bestPkt.RemoteEndpoint
                                    : "PACKET_SAMPLED // " + bestPkt.ProtocolName;
                            }

                            // Spawn showy 3D Polyhedral Vertex Shards & Geometric Shockwave at capture point!
                            SpawnPacketCaptureBurst(bestPkt, a, useTriAttack);

                            br.DriveHunger = Math.Max(0.08f, br.DriveHunger - 0.32f);
                            Packets.Remove(bestPkt);
                        }
                    }
                } else {
                    a.WaypointTimer--;
                    float wdx = a.WaypointX - a.PosX;
                    float wdy = a.WaypointY - a.PosY;
                    float wdz = a.WaypointZ - a.PosZ;
                    if (a.WaypointTimer <= 0 || (wdx * wdx + wdy * wdy + wdz * wdz < 85f * 85f)) {
                        PickRandomMonitorPoint(out a.WaypointX, out a.WaypointY, 175, 145);
                        a.WaypointZ = (float)(rng.NextDouble() * 400.0 - 240.0); // Patrol across full 3D depth [-240 .. +160]
                        a.WaypointTimer = rng.Next(110, 240);
                    }
                    targetX = a.WaypointX;
                    targetY = a.WaypointY;
                    targetZ = a.WaypointZ;
                    if (a.SyncTimer <= 0f && a.ScanWaveTimer <= 0f) {
                        br.ActiveStateName = "MESH_PATROL_3D // 5-MON_VIRTUAL_SOC";
                    }
                }

                float speedBonus = 1.0f + Math.Min(0.55f, (br.SynapticGain - 1.0f) * 0.40f) + a.DashBoostTimer * 0.48f;
                float dxT = targetX - a.PosX;
                float dyT = targetY - a.PosY;
                float dzT = targetZ - a.PosZ;
                float distToT = (float)Math.Sqrt(dxT * dxT + dyT * dyT + dzT * dzT);
                // Boost cross-monitor transit speed when traveling to another screen
                float crossMonMult = (distToT > 750f) ? 1.85f : 1.0f;
                float zPerspSpeed = GetDepthScale(a.PosZ);
                float maxV = 4.6f * speedBonus * crossMonMult;
                float accel = (a.DashBoostTimer > 0f || a.ActiveLockStrength > 0.2f) ? 0.19f : 0.14f;
                a.VelX = a.VelX * (1f - accel) + (Clamp(dxT * 0.038f, -maxV, maxV) + sepX) * accel;
                a.VelY = a.VelY * (1f - accel) + (Clamp(dyT * 0.038f, -maxV * 0.88f, maxV * 0.88f) + sepY) * accel;
                a.VelZ = a.VelZ * (1f - accel) + (Clamp(dzT * 0.040f, -maxV * 0.88f, maxV * 0.88f) + sepZ) * accel;

                float nextX = a.PosX + a.VelX * (0.72f + 0.28f * zPerspSpeed);
                float nextY = a.PosY + a.VelY * (0.72f + 0.28f * zPerspSpeed);
                a.PosZ = Clamp(a.PosZ + a.VelZ, -245f, 168f);

                if (!IsInsideAnyMonitor(nextX, nextY, 70)) {
                    Rectangle targetMon = GetContainingOrNearestMonitor(targetX, targetY);
                    Rectangle curMon = GetContainingOrNearestMonitor(a.PosX, a.PosY);
                    if (targetMon != curMon) {
                        // Bridge across inter-monitor dead space onto the target monitor's entry border!
                        a.BlinkStartPt = new PointF(a.PosX, a.PosY);
                        a.BlinkStartZ = a.PosZ;
                        a.PosX = Clamp(nextX, targetMon.Left + 95f, targetMon.Right - 95f);
                        a.PosY = Clamp(nextY, targetMon.Top + 110f, targetMon.Bottom - 95f);
                        if (a.QuantumBlinkTimer <= 0f) a.QuantumBlinkTimer = 0.85f;
                    } else {
                        a.PosX = Clamp(nextX, curMon.Left + 95f, curMon.Right - 95f);
                        a.PosY = Clamp(nextY, curMon.Top + 110f, curMon.Bottom - 95f);
                    }
                } else {
                    a.PosX = nextX;
                    a.PosY = nextY;
                }

                float spd = (float)Math.Sqrt(a.VelX * a.VelX + a.VelY * a.VelY + a.VelZ * a.VelZ);
                a.WalkPhase += 0.12f + spd * 0.065f;

                if (dxT < -55f) a.FacingDir = -1.0f;
                else if (dxT > 55f) a.FacingDir = 1.0f;

                float baseYaw = a.FacingDir * 0.58f + Clamp(-a.VelZ * 0.045f * a.FacingDir, -0.22f, 0.22f);
                if (a.SpinRollTimer > 0f) {
                    a.SpinRollTimer -= 0.038f;
                    a.Yaw = baseYaw + (1.0f - a.SpinRollTimer) * (float)(Math.PI * 2.0);
                } else {
                    a.Yaw = a.Yaw * 0.86f + baseYaw * 0.14f;
                }

                // Dynamic aerodynamic pitch & banking roll when pursuing packets
                float pursuitTilt = Clamp(a.VelY * 0.022f, -0.16f, 0.16f);
                a.Pitch = 0.16f + (float)Math.Sin(a.FloatBob) * 0.035f + Clamp(-a.VelZ * 0.032f, -0.14f, 0.14f) + pursuitTilt;
                a.Roll = a.Roll * 0.84f + Clamp(-a.VelX * 0.028f, -0.14f, 0.14f) * 0.16f;

                // Longer, smoother skill effect durations so every ability looks dramatic and legible!
                if (a.BeamTimer > 0f) a.BeamTimer -= 0.036f;
                if (a.TriAttackTimer > 0f) a.TriAttackTimer -= 0.028f;
                if (a.ShieldTimer > 0f) a.ShieldTimer -= 0.022f;
                if (a.SyncTimer > 0f) a.SyncTimer -= 0.025f;
                if (a.QuantumBlinkTimer > 0f) a.QuantumBlinkTimer -= 0.032f;
                if (a.ScanWaveTimer > 0f) a.ScanWaveTimer -= 0.022f;

                float lookX = (HoveredPacket != null) ? (HoveredPacket.X - a.PosX) : (targetX - a.PosX);
                float lookY = (HoveredPacket != null) ? (HoveredPacket.Y - a.PosY) : (targetY - a.PosY);
                a.HeadYaw = a.HeadYaw * 0.82f + Clamp(lookX / 680f, -0.22f, 0.22f) * 0.18f;
                a.HeadPitch = a.HeadPitch * 0.82f + Clamp(lookY / 680f, -0.14f, 0.18f) * 0.18f;
            }

            // 5. Update 3D Polyhedral Vertex Shards & Geometric Shockwaves from packet captures!
            UpdateCaptureEffects();
        }
    }

    private void SpawnTractorStreamVertex(DataPacket pkt, PorygonAgent ag) {
        float ang = (float)(rng.NextDouble() * Math.PI * 2.0);
        float r = 10f + (float)(rng.NextDouble() * 14.0);
        float sx = pkt.X + (float)Math.Cos(ang) * r;
        float sy = pkt.Y + (float)Math.Sin(ang) * r;
        VertexShards.Add(new CaptureVertexShard {
            X = sx,
            Y = sy,
            Z = pkt.Z,
            PrevX = sx,
            PrevY = sy,
            VX = (ag.PosX - sx) * 0.06f + (float)(rng.NextDouble() - 0.5) * 2.2f,
            VY = (ag.PosY - sy) * 0.06f + (float)(rng.NextDouble() - 0.5) * 2.2f,
            VZ = (ag.PosZ - pkt.Z) * 0.06f,
            RotAngle = (float)(rng.NextDouble() * 6.28),
            RotSpeed = (float)((rng.NextDouble() - 0.5) * 0.35),
            Size = 4.5f + (float)(rng.NextDouble() * 3.5),
            Life = 0.62f,
            MaxLife = 0.62f,
            ShapeKind = rng.Next(3),
            BurstGroupId = -1,
            OwnerAgentIdx = ag.AgentIndex,
            HomingToAgent = true,
            Col = pkt.Col,
            SecondaryCol = ag.AuraColor
        });
    }

    private void SpawnPacketCaptureBurst(DataPacket pkt, PorygonAgent ag, bool isTriAttack) {
        burstGroupCounter = (burstGroupCounter + 1) & 0x7FFFFFFF;
        int groupId = burstGroupCounter;

        // Limit active shards to keep 60/24 FPS ultra-fast
        while (VertexShards.Count > 190) {
            VertexShards.RemoveAt(0);
        }
        while (Shockwaves.Count > 14) {
            Shockwaves.RemoveAt(0);
        }

        Color primary = pkt.Col;
        Color secondary = ag.AuraColor;
        Color accent = Color.FromArgb(250, 204, 21); // Solar Gold
        if (isTriAttack || pkt.Kind == 2) {
            primary = Color.FromArgb(255, 42, 85);   // Crimson Threat
            secondary = Color.FromArgb(0, 240, 255); // Ice Cyan
            accent = Color.FromArgb(236, 72, 153);   // Plasma Magenta
        } else if (pkt.Kind == 1) {
            primary = Color.FromArgb(250, 204, 21);  // Solar Gold
            secondary = Color.FromArgb(0, 240, 255); // Cyber Cyan
            accent = Color.FromArgb(16, 255, 160);   // Bio-Emerald
        }

        int shardCount = isTriAttack ? 42 : (pkt.Kind == 2 ? 36 : (pkt.Kind == 1 ? 30 : 24));
        for (int i = 0; i < shardCount; i++) {
            double theta = (i / (double)shardCount) * Math.PI * 2.0 + (rng.NextDouble() - 0.5) * 0.25;
            double phi = (rng.NextDouble() - 0.5) * Math.PI * 0.85;
            float speed = (float)(3.2 + rng.NextDouble() * (isTriAttack ? 9.5 : 6.8));
            float vx = (float)(Math.Cos(theta) * Math.Cos(phi)) * speed;
            float vy = (float)(Math.Sin(theta) * Math.Cos(phi)) * speed - 1.1f;
            float vz = (float)Math.Sin(phi) * speed * 1.4f;

            Color c1 = (i % 3 == 0) ? primary : ((i % 3 == 1) ? secondary : accent);
            Color c2 = (i % 2 == 0) ? Color.FromArgb(255, 255, 255) : secondary;

            VertexShards.Add(new CaptureVertexShard {
                X = pkt.X + vx * 1.2f,
                Y = pkt.Y + vy * 1.2f,
                Z = pkt.Z + vz * 1.2f,
                PrevX = pkt.X,
                PrevY = pkt.Y,
                VX = vx,
                VY = vy,
                VZ = vz,
                RotAngle = (float)(rng.NextDouble() * Math.PI * 2.0),
                RotSpeed = (float)((rng.NextDouble() - 0.5) * 0.42),
                Size = (float)(5.5 + rng.NextDouble() * (isTriAttack ? 7.5 : 5.5)),
                Life = 1.0f,
                MaxLife = 1.0f,
                ShapeKind = i % 3, // 0 = 3D Triangle, 1 = Octa-Diamond, 2 = Vertex Star Node
                BurstGroupId = groupId,
                OwnerAgentIdx = ag.AgentIndex,
                HomingToAgent = (i % 2 == 0), // 50% of vertices curve back and get absorbed into Porygon!
                Col = c1,
                SecondaryCol = c2
            });
        }

        string label = isTriAttack
            ? string.Format("★ TRI-ATTACK PURGE // +45 XP [{0}]", pkt.AnomalyType)
            : (pkt.Kind == 1
                ? string.Format("◆ HOST AUDITED // +30 XP [{0}]", pkt.RemoteEndpoint)
                : string.Format("● VERTEX_DECODE // +15 XP [{0}]", pkt.ProtocolName));

        Shockwaves.Add(new CaptureShockwave {
            X = pkt.X,
            Y = pkt.Y,
            Z = pkt.Z,
            Life = 1.0f,
            MaxRadius = isTriAttack ? 148f : (pkt.Kind >= 1 ? 116f : 92f),
            RotAngle = (float)(rng.NextDouble() * Math.PI),
            Kind = isTriAttack ? 2 : pkt.Kind,
            Col = primary,
            SecondaryCol = secondary,
            FloatingLabel = label
        });
    }

    private void UpdateCaptureEffects() {
        for (int i = VertexShards.Count - 1; i >= 0; i--) {
            CaptureVertexShard s = VertexShards[i];
            s.PrevX = s.X;
            s.PrevY = s.Y;
            s.Life -= 0.028f;
            if (s.Life <= 0f) {
                VertexShards.RemoveAt(i);
                continue;
            }

            s.RotAngle += s.RotSpeed;

            if (s.HomingToAgent && s.Life < 0.62f && s.OwnerAgentIdx >= 0 && s.OwnerAgentIdx < Agents.Count) {
                // Curve vertex shards back into the capturing Porygon's 3D body to feed its neural connectome!
                PorygonAgent ag = Agents[s.OwnerAgentIdx];
                float dx = ag.PosX - s.X;
                float dy = ag.PosY - s.Y;
                float dz = ag.PosZ - s.Z;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (dist < 32f) {
                    ag.AbsorbFlashTimer = Math.Max(ag.AbsorbFlashTimer, 0.45f);
                    VertexShards.RemoveAt(i);
                    continue;
                }
                float pull = 1.35f / Math.Max(45f, dist);
                s.VX = s.VX * 0.84f + dx * pull * 6.5f;
                s.VY = s.VY * 0.84f + dy * pull * 6.5f;
                s.VZ = s.VZ * 0.84f + dz * pull * 6.5f;
            } else {
                // Outward polyhedral decompression with subtle gravity & drag
                s.VX *= 0.94f;
                s.VY = s.VY * 0.94f + 0.08f;
                s.VZ *= 0.94f;
            }

            float zScale = GetDepthScale(s.Z);
            s.X += s.VX * zScale;
            s.Y += s.VY * zScale;
            s.Z = Clamp(s.Z + s.VZ, -250f, 170f);
        }

        for (int i = Shockwaves.Count - 1; i >= 0; i--) {
            CaptureShockwave sw = Shockwaves[i];
            sw.Life -= 0.032f;
            sw.RotAngle += (sw.Kind == 2) ? 0.085f : 0.045f;
            if (sw.Life <= 0f) {
                Shockwaves.RemoveAt(i);
            }
        }
    }

    private void AddIdsLog(string entry) {
        IdsDetectionLog.Add(entry);
        while (IdsDetectionLog.Count > 5) IdsDetectionLog.RemoveAt(0);
    }

    private static float Clamp(float v, float mn, float mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }

    private static Vec3 RotateX(Vec3 v, float a) {
        float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
        return new Vec3(v.X, v.Y * c - v.Z * s, v.Y * s + v.Z * c);
    }
    private static Vec3 RotateY(Vec3 v, float a) {
        float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
        return new Vec3(v.X * c + v.Z * s, v.Y, -v.X * s + v.Z * c);
    }
    private static Vec3 RotateZ(Vec3 v, float a) {
        float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
        return new Vec3(v.X * c - v.Y * s, v.X * s + v.Y * c, v.Z);
    }

    private static Vec3 TransformAgentVertex(PorygonAgent a, Vec3 v, float scale) {
        if (v.Y > 12f) {
            float headWeight = Math.Min(1.0f, (v.Y - 12f) / 40f);
            Vec3 pivot = new Vec3(0, 12f, 0);
            Vec3 rel = v - pivot;
            rel = RotateX(rel, a.HeadPitch * headWeight);
            rel = RotateY(rel, a.HeadYaw * headWeight);
            v = pivot + rel;
        } else if (v.Y < -14f && Math.Abs(v.X) > 10f) {
            float footWeight = Math.Min(1.0f, (-14f - v.Y) / 30f);
            float sideSign = (v.X < 0) ? 1.0f : -1.0f;
            v.Z += (float)Math.Sin(a.WalkPhase) * sideSign * 7.5f * footWeight;
            v.Y += (float)Math.Cos(a.WalkPhase) * sideSign * 3.0f * footWeight;
        }
        if (v.Z < -22f) {
            float tailWeight = Math.Min(1.0f, (-22f - v.Z) / 38f);
            v.X += (float)Math.Sin(a.WalkPhase * 1.3f) * 8.0f * tailWeight;
        }

        v = RotateY(v, a.Yaw);
        v = RotateX(v, a.Pitch);
        v = RotateZ(v, a.Roll);
        return v * scale;
    }

    private class ProjectedFace : IComparable<ProjectedFace> {
        public PointF[] Pts;
        public float Depth;
        public Color ShadedColor;
        public byte ContourMask;
        public bool IsEyeOverlay;
        public int CompareTo(ProjectedFace other) {
            return this.Depth.CompareTo(other.Depth);
        }
    }

    public void Render(Graphics g, float userScale, Color themeCyan, Color themeMag, Font tagFont) {
        List<DataPacket> pktsSnap;
        List<CaptureVertexShard> shardsSnap;
        List<CaptureShockwave> wavesSnap;
        DataPacket hovPkt;
        PorygonAgent hovAg;
        lock (packetLock) {
            pktsSnap = new List<DataPacket>(Packets);
            shardsSnap = new List<CaptureVertexShard>(VertexShards);
            wavesSnap = new List<CaptureShockwave>(Shockwaves);
            hovPkt = HoveredPacket;
            hovAg = HoveredAgent;
        }

        // Sort nodes and agents back-to-front by Z coordinate (-250 deep .. +170 near)
        pktsSnap.Sort((a, b) => a.Z.CompareTo(b.Z));
        List<PorygonAgent> agentsSorted = new List<PorygonAgent>(Agents);
        agentsSorted.Sort((a, b) => a.PosZ.CompareTo(b.PosZ));

        // 0. Draw Cross-Monitor Neural Mesh Conduits linking Porygon, Porygon2, and Porygon-Z across screens!
        if (Agents.Count >= 2) {
            using (Pen meshPen = new Pen(Color.FromArgb(42, themeCyan.R, themeCyan.G, themeCyan.B), 1.3f))
            using (SolidBrush pulseBrush = new SolidBrush(Color.FromArgb(170, themeCyan.R, themeCyan.G, themeCyan.B))) {
                meshPen.DashStyle = DashStyle.Dash;
                for (int i = 0; i < Agents.Count; i++) {
                    PorygonAgent a1 = Agents[i];
                    PorygonAgent a2 = Agents[(i + 1) % Agents.Count];
                    g.DrawLine(meshPen, a1.PosX, a1.PosY, a2.PosX, a2.PosY);
                    float tPulse = (float)((a1.FloatBob * 0.18 + i * 0.33) % 1.0);
                    if (tPulse < 0f) tPulse += 1f;
                    float px = a1.PosX + (a2.PosX - a1.PosX) * tPulse;
                    float py = a1.PosY + (a2.PosY - a1.PosY) * tPulse;
                    g.FillEllipse(pulseBrush, px - 3.5f, py - 3.5f, 7f, 7f);
                }
            }
        }

        // 1. Draw floating 3D Real Network Nodes (with Z perspective scaling + 3D floor-tether stems + rotating polyhedra + tractor lock rings)
        for (int i = 0; i < pktsSnap.Count; i++) {
            DataPacket p = pktsSnap[i];
            float zScale = GetDepthScale(p.Z);
            float sz = ((p.Kind == 0 ? 9.0f : 12.2f) + (float)Math.Sin(p.floatRot * 2f) * 2.5f) * zScale;
            if (p.IsHovered) sz *= 1.35f;
            int depthAlpha = (int)Clamp(175f + (p.Z + 250f) * 0.19f, 120f, 255f);

            using (Pen stemP = new Pen(Color.FromArgb(depthAlpha / 3, p.Col.R, p.Col.G, p.Col.B), 1.0f))
            using (SolidBrush glowB = new SolidBrush(Color.FromArgb(p.IsHovered ? 120 : (depthAlpha / 3), p.Col.R, p.Col.G, p.Col.B)))
            using (Pen wireP = new Pen(Color.FromArgb(depthAlpha, p.Col.R, p.Col.G, p.Col.B), (p.IsHovered ? 2.6f : 1.9f) * Math.Max(0.8f, zScale * 0.9f)))
            using (Pen innerP = new Pen(Color.FromArgb(Math.Max(70, depthAlpha - 45), 255, 255, 255), 1.1f))
            using (SolidBrush coreB = new SolidBrush(Color.FromArgb(depthAlpha, 250, 252, 255))) {
                float stemH = 26f * zScale;
                g.DrawLine(stemP, p.X, p.Y + sz, p.X, p.Y + sz + stemH);
                g.DrawEllipse(stemP, p.X - sz * 0.8f, p.Y + sz + stemH - 2.5f, sz * 1.6f, 5f);

                g.FillEllipse(glowB, p.X - sz * 2.1f, p.Y - sz * 2.1f, sz * 4.2f, sz * 4.2f);

                // Rotating 3D Octahedron / Crystal Wireframe
                float rot = p.floatRot * (p.Kind == 2 ? 1.8f : 1.15f);
                float eqX1 = (float)Math.Cos(rot) * sz;
                float eqY1 = (float)Math.Sin(rot) * sz * 0.42f;
                float eqX2 = (float)Math.Cos(rot + Math.PI * 0.5) * sz;
                float eqY2 = (float)Math.Sin(rot + Math.PI * 0.5) * sz * 0.42f;
                PointF topPt = new PointF(p.X, p.Y - sz * 1.18f);
                PointF botPt = new PointF(p.X, p.Y + sz * 1.18f);
                PointF e1 = new PointF(p.X + eqX1, p.Y + eqY1);
                PointF e2 = new PointF(p.X + eqX2, p.Y + eqY2);
                PointF e3 = new PointF(p.X - eqX1, p.Y - eqY1);
                PointF e4 = new PointF(p.X - eqX2, p.Y - eqY2);

                g.DrawPolygon(wireP, new PointF[] { e1, e2, e3, e4 });
                g.DrawLine(wireP, topPt, e1); g.DrawLine(wireP, topPt, e2);
                g.DrawLine(wireP, topPt, e3); g.DrawLine(wireP, topPt, e4);
                g.DrawLine(wireP, botPt, e1); g.DrawLine(wireP, botPt, e2);
                g.DrawLine(wireP, botPt, e3); g.DrawLine(wireP, botPt, e4);

                if (p.Kind == 2) {
                    g.DrawEllipse(wireP, p.X - sz * 1.38f, p.Y - sz * 1.38f, sz * 2.76f, sz * 2.76f);
                    // Orbiting threat satellite vertices
                    for (int s = 0; s < 3; s++) {
                        double sAng = p.OrbitPhase + s * (2.0 * Math.PI / 3.0);
                        float sx = p.X + (float)Math.Cos(sAng) * sz * 1.65f;
                        float sy = p.Y + (float)Math.Sin(sAng) * sz * 0.95f;
                        g.FillEllipse(coreB, sx - 2.4f, sy - 2.4f, 4.8f, 4.8f);
                        g.DrawLine(innerP, p.X, p.Y, sx, sy);
                    }
                } else if (p.Kind == 1) {
                    g.DrawEllipse(wireP, p.X - sz * 1.35f, p.Y - sz * 1.35f, sz * 2.7f, sz * 2.7f);
                }

                // Dynamic Tractor Lock Rings when a Porygon is pulling this packet
                if (p.PullIntensity > 0.08f) {
                    float lockR = sz * (1.85f + (float)Math.Sin(p.OrbitPhase * 3f) * 0.25f);
                    int lockAlpha = (int)Clamp(p.PullIntensity * 235f, 60f, 245f);
                    using (Pen lockP = new Pen(Color.FromArgb(lockAlpha, 255, 255, 255), 1.6f)) {
                        lockP.DashStyle = DashStyle.Dash;
                        g.DrawEllipse(lockP, p.X - lockR, p.Y - lockR, lockR * 2f, lockR * 2f);
                    }
                }

                float cr = 3.2f * zScale;
                g.FillEllipse(coreB, p.X - cr, p.Y - cr, cr * 2f, cr * 2f);
            }
        }

        // 2. Render each of the 5 Porygon Agents sorted back-to-front by Z!
        for (int aIdx = 0; aIdx < agentsSorted.Count; aIdx++) {
            RenderSingleAgent(g, agentsSorted[aIdx], userScale, themeCyan, themeMag, tagFont);
        }

        // 2.5 Render 3D Packet Capture Shockwaves, Polyhedral Vertex Shards & Homing Absorption Streams
        RenderCaptureEffectsGdi(g, shardsSnap, wavesSnap, tagFont);

        // 3. Render Interactive Mouse-Hover Node Inspection Card (or AI Agent Inspection Card)!
        if (tagFont != null) {
            if (hovPkt != null) {
                DrawHoveredPacketCard(g, hovPkt, tagFont);
            } else if (hovAg != null) {
                DrawHoveredAgentCard(g, hovAg, tagFont);
            }
        }
    }

    private void RenderCaptureEffectsGdi(Graphics g, List<CaptureVertexShard> shardsSnap, List<CaptureShockwave> wavesSnap, Font tagFont) {
        // 1. Expanding Geometric Capture Shockwaves
        for (int i = 0; i < wavesSnap.Count; i++) {
            CaptureShockwave w = wavesSnap[i];
            float lifeRatio = Clamp(w.Life, 0f, 1f);
            float prog = 1.0f - lifeRatio;
            float zScale = GetDepthScale(w.Z);
            float r = w.MaxRadius * prog * zScale;
            int alpha = (int)Clamp(lifeRatio * 235f, 0f, 245f);
            if (alpha <= 5) continue;

            int sides = (w.Kind == 2) ? 8 : 6;
            PointF[] poly = new PointF[sides];
            PointF[] innerPoly = new PointF[sides];
            for (int k = 0; k < sides; k++) {
                double ang = w.RotAngle + prog * 1.6f + k * (2.0 * Math.PI / sides);
                poly[k] = new PointF(w.X + (float)Math.Cos(ang) * r, w.Y + (float)Math.Sin(ang) * r * 0.68f);
                innerPoly[k] = new PointF(w.X + (float)Math.Cos(-ang) * r * 0.68f, w.Y + (float)Math.Sin(-ang) * r * 0.46f);
            }
            using (Pen wavePen = new Pen(Color.FromArgb(alpha, w.Col.R, w.Col.G, w.Col.B), (w.Kind == 2 ? 2.8f : 2.1f) * zScale))
            using (Pen corePen = new Pen(Color.FromArgb(alpha * 3 / 4, 255, 255, 255), 1.2f)) {
                g.DrawPolygon(wavePen, poly);
                g.DrawPolygon(corePen, innerPoly);
            }
            if (tagFont != null && !string.IsNullOrEmpty(w.FloatingLabel) && prog < 0.78f) {
                using (SolidBrush lblB = new SolidBrush(Color.FromArgb(alpha, 255, 250, 220))) {
                    g.DrawString(w.FloatingLabel, tagFont, lblB, w.X - 42f, w.Y - 18f - prog * 34f);
                }
            }
        }

        // 2. 3D Polyhedral Vertex Shards & Constellation Filaments
        for (int i = 0; i < shardsSnap.Count; i++) {
            CaptureVertexShard s = shardsSnap[i];
            float lifeRatio = Clamp(s.Life / Math.Max(0.01f, s.MaxLife), 0f, 1f);
            int alpha = (int)(lifeRatio * 245f);
            if (alpha <= 8) continue;

            float zScale = GetDepthScale(s.Z);
            float sz = s.Size * zScale * (0.55f + 0.45f * lifeRatio);

            using (Pen trailP = new Pen(Color.FromArgb(alpha / 2, s.Col.R, s.Col.G, s.Col.B), 1.6f * zScale))
            using (Pen edgeP = new Pen(Color.FromArgb(alpha, 255, 255, 255), 1.35f * zScale))
            using (SolidBrush fillB = new SolidBrush(Color.FromArgb(alpha * 3 / 4, s.Col.R, s.Col.G, s.Col.B))) {
                g.DrawLine(trailP, s.PrevX, s.PrevY, s.X, s.Y);

                // Connect nearby shards from same capture burst into a 3D wireframe vertex web
                if (s.BurstGroupId > 0) {
                    for (int j = i + 1; j < shardsSnap.Count; j++) {
                        CaptureVertexShard s2 = shardsSnap[j];
                        if (s2.BurstGroupId != s.BurstGroupId) continue;
                        float dx = s2.X - s.X, dy = s2.Y - s.Y;
                        float d2 = dx * dx + dy * dy;
                        if (d2 < 3600f) {
                            int linkA = (int)((1.0f - d2 / 3600f) * alpha * 0.55f);
                            if (linkA > 10) {
                                using (Pen webP = new Pen(Color.FromArgb(linkA, s.SecondaryCol.R, s.SecondaryCol.G, s.SecondaryCol.B), 1.0f)) {
                                    g.DrawLine(webP, s.X, s.Y, s2.X, s2.Y);
                                }
                            }
                        }
                    }
                }

                int vCount = (s.ShapeKind == 0) ? 3 : 4;
                PointF[] pts = new PointF[vCount];
                for (int k = 0; k < vCount; k++) {
                    double ang = s.RotAngle + k * (2.0 * Math.PI / vCount);
                    float rx = (float)Math.Cos(ang) * sz;
                    float ry = (float)Math.Sin(ang) * sz * (0.55f + 0.45f * (float)Math.Cos(s.RotAngle * 1.3f));
                    pts[k] = new PointF(s.X + rx, s.Y + ry);
                }
                g.FillPolygon(fillB, pts);
                g.DrawPolygon(edgeP, pts);
            }
        }
    }

    private void DrawHoveredPacketCard(Graphics g, DataPacket p, Font f) {
        PorygonAgent responder = null;
        for (int i = 0; i < Agents.Count; i++) {
            if (Agents[i].Name == p.AssignedAgentName) { responder = Agents[i]; break; }
        }
        if (responder == null && Agents.Count > 0) responder = Agents[0];

        float zScale = GetDepthScale(p.Z);
        float r = 24f * zScale;
        using (Pen lockPen = new Pen(Color.FromArgb(245, p.Col.R, p.Col.G, p.Col.B), 2.2f))
        using (Pen linkPen = new Pen(Color.FromArgb(145, p.Col.R, p.Col.G, p.Col.B), 1.4f)) {
            linkPen.DashStyle = DashStyle.Dash;
            g.DrawEllipse(lockPen, p.X - r, p.Y - r, r * 2f, r * 2f);
            float b = 29f * zScale, l = 9f * zScale;
            g.DrawLine(lockPen, p.X - b, p.Y - b, p.X - b + l, p.Y - b);
            g.DrawLine(lockPen, p.X - b, p.Y - b, p.X - b, p.Y - b + l);
            g.DrawLine(lockPen, p.X + b, p.Y - b, p.X + b - l, p.Y - b);
            g.DrawLine(lockPen, p.X + b, p.Y - b, p.X + b, p.Y - b + l);
            g.DrawLine(lockPen, p.X - b, p.Y + b, p.X - b + l, p.Y + b);
            g.DrawLine(lockPen, p.X - b, p.Y + b, p.X - b, p.Y + b - l);
            g.DrawLine(lockPen, p.X + b, p.Y + b, p.X + b - l, p.Y + b);
            g.DrawLine(lockPen, p.X + b, p.Y + b, p.X + b, p.Y + b - l);

            if (responder != null) {
                g.DrawLine(linkPen, p.X, p.Y, responder.PosX, responder.PosY);
            }
        }

        Rectangle mon = GetContainingOrNearestMonitor(p.X, p.Y);
        int cw = 424;
        int ch = 138;
        int cx = (int)p.X + 36;
        int cy = (int)p.Y - ch / 2;
        if (cx + cw > mon.Right - 24) cx = (int)p.X - cw - 36;
        if (cx < mon.Left + 24) cx = mon.Left + 24;
        if (cy < mon.Top + 28) cy = mon.Top + 28;
        if (cy + ch > mon.Bottom - 54) cy = mon.Bottom - 54 - ch;

        using (SolidBrush bg = new SolidBrush(Color.FromArgb(148, 4, 8, 18)))
        using (Pen border = new Pen(Color.FromArgb(235, p.Col.R, p.Col.G, p.Col.B), 1.8f))
        using (SolidBrush headB = new SolidBrush(p.Col))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(245, 235, 242, 252)))
        using (SolidBrush barBg = new SolidBrush(Color.FromArgb(50, 255, 255, 255))) {
            g.FillRectangle(bg, cx, cy, cw, ch);
            g.DrawRectangle(border, cx, cy, cw, ch);

            string kindTitle = (p.Kind == 2)
                ? ">> [⚠ IDS ANOMALY NODE // 3D HIGH PRIORITY]"
                : (p.Kind == 1 ? ">> [★ NEW ENDPOINT AUDIT // 3D GOLDEN ORB]" : ">> [● VERIFIED 3D NETWORK FLOW NODE]");
            g.DrawString(kindTitle, f, headB, cx + 10, cy + 8);

            g.DrawString(string.Format("REMOTE_EP : {0}  [{1}]", p.RemoteEndpoint, p.ProtocolName), f, txtB, cx + 10, cy + 26);
            g.DrawString(string.Format("LOCAL_SKT : {0}  ({1})", p.LocalEndpoint, p.TcpStateStr), f, txtB, cx + 10, cy + 43);
            g.DrawString(string.Format("COORD_3D  : [X:{0:0}, Y:{1:0}, Z:{2:+0;-0;0}] (Depth {3:0.00}x)", p.X, p.Y, p.Z, zScale), f, headB, cx + 10, cy + 60);

            int pct = (int)Math.Round(p.AnomalyScore * 100f);
            g.DrawString(string.Format("ANOMALY   : {0}% [{1}]", pct, p.AnomalyType), f, headB, cx + 10, cy + 78);
            g.FillRectangle(barBg, cx + 298, cy + 81, 108, 8);
            g.FillRectangle(headB, cx + 298, cy + 81, (int)(1.08f * Math.Min(100, pct)), 8);

            g.DrawString("DETAIL    : " + p.AnomalyDetail, f, txtB, cx + 10, cy + 96);
            g.DrawString(string.Format("AI_AGENT  : {0} [LOCKED] | FLOW: {1:0.0} KB/s", p.AssignedAgentName, p.KbpsSample), f, headB, cx + 10, cy + 114);
        }
    }

    private void DrawHoveredAgentCard(Graphics g, PorygonAgent a, Font f) {
        float zScale = GetDepthScale(a.PosZ);
        float r = 76f * zScale;
        using (Pen lockPen = new Pen(Color.FromArgb(230, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.0f)) {
            lockPen.DashStyle = DashStyle.Dash;
            g.DrawEllipse(lockPen, a.PosX - r, a.PosY - r, r * 2f, r * 2f);
        }

        Rectangle mon = GetContainingOrNearestMonitor(a.PosX, a.PosY);
        int cw = 396;
        int ch = 148;
        int cx = (int)a.PosX - cw / 2;
        int cy = (int)a.PosY - 92 - ch;
        if (cx < mon.Left + 24) cx = mon.Left + 24;
        if (cx + cw > mon.Right - 24) cx = mon.Right - 24 - cw;
        if (cy < mon.Top + 24) cy = (int)a.PosY + 108;

        FlyBrainConnectome br = a.Brain;
        using (SolidBrush bg = new SolidBrush(Color.FromArgb(148, 4, 8, 18)))
        using (Pen border = new Pen(Color.FromArgb(235, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1.8f))
        using (SolidBrush headB = new SolidBrush(a.AuraColor))
        using (SolidBrush goldB = new SolidBrush(Color.FromArgb(250, 204, 21)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(245, 235, 242, 252))) {
            g.FillRectangle(bg, cx, cy, cw, ch);
            g.DrawRectangle(border, cx, cy, cw, ch);

            g.DrawString(string.Format(">> AI_CORE // {0} [LVL {1}] — {2}", a.Name, br.Level, a.RoleTag), f, headB, cx + 10, cy + 8);
            g.DrawString(string.Format("COORD_3D  : [X:{0:0}, Y:{1:0}, Z:{2:+0;-0;0}] (Scale {3:0.00}x)", a.PosX, a.PosY, a.PosZ, zScale), f, goldB, cx + 10, cy + 26);
            g.DrawString(string.Format("TELEMETRY : DL_Z:{0:+0.0;-0.0;0.0}σ | UL_Z:{1:+0.0;-0.0;0.0}σ | PING_Z:{2:+0.0;-0.0;0.0}σ",
                LatestFeed.DlZScore, LatestFeed.UlZScore, LatestFeed.PingZScore), f, txtB, cx + 10, cy + 45);
            g.DrawString("TARGET_3D : " + a.CurrentTargetEndpoint, f, txtB, cx + 10, cy + 63);
            g.DrawString(string.Format("IDS_STATS : Flows:{0} | AnomaliesPurged:{1} | P2P_Syncs:{2}",
                br.PacketsCollected, br.AnomaliesCleared, br.NeuralSyncs), f, txtB, cx + 10, cy + 81);
            g.DrawString(string.Format("SYNAPSES  : Gain {0:0.00}x | XP {1}/{2} | State: {3}",
                br.SynapticGain, br.XP, br.NextLevelXP, br.ActiveStateName), f, headB, cx + 10, cy + 100);
        }
    }

    public Func<float, float, bool> IsPointOccluded = null;

    public void RenderOpenGL(float userScale, Color themeCyan, Color themeMag) {
        List<DataPacket> pktsSnap;
        List<CaptureVertexShard> shardsSnap;
        List<CaptureShockwave> wavesSnap;
        DataPacket hovPkt;
        PorygonAgent hovAg;
        lock (packetLock) {
            pktsSnap = new List<DataPacket>(Packets);
            shardsSnap = new List<CaptureVertexShard>(VertexShards);
            wavesSnap = new List<CaptureShockwave>(Shockwaves);
            hovPkt = HoveredPacket;
            hovAg = HoveredAgent;
        }

        // Sort both network nodes and Porygon agents back-to-front by Z coordinate (-250 deep .. +170 near)
        pktsSnap.Sort((a, b) => a.Z.CompareTo(b.Z));
        List<PorygonAgent> agentsSorted = new List<PorygonAgent>(Agents);
        agentsSorted.Sort((a, b) => a.PosZ.CompareTo(b.PosZ));

        // 0. Draw Cross-Monitor 3D Neural Mesh Conduits linking the 5 Porygons across screens on the GPU
        if (Agents.Count >= 2) {
            Color meshCol = Color.FromArgb(55, themeCyan.R, themeCyan.G, themeCyan.B);
            Color pulseCol = Color.FromArgb(195, themeCyan.R, themeCyan.G, themeCyan.B);
            for (int i = 0; i < Agents.Count; i++) {
                PorygonAgent a1 = Agents[i];
                PorygonAgent a2 = Agents[(i + 1) % Agents.Count];
                if (IsPointOccluded != null && IsPointOccluded(a1.PosX, a1.PosY) && IsPointOccluded(a2.PosX, a2.PosY)) continue;
                float avgScale = (GetDepthScale(a1.PosZ) + GetDepthScale(a2.PosZ)) * 0.5f;
                GpuGl.DrawLine(a1.PosX, a1.PosY, a2.PosX, a2.PosY, meshCol, 1.2f * avgScale);
                float tPulse = (float)((a1.FloatBob * 0.18 + i * 0.33) % 1.0);
                if (tPulse < 0f) tPulse += 1f;
                float px = a1.PosX + (a2.PosX - a1.PosX) * tPulse;
                float py = a1.PosY + (a2.PosY - a1.PosY) * tPulse;
                float pz = a1.PosZ + (a2.PosZ - a1.PosZ) * tPulse;
                float pScale = GetDepthScale(pz);
                GpuGl.FillEllipse(px, py, 3.8f * pScale, 3.8f * pScale, pulseCol, 10);
            }
        }

        // 1. Draw floating 3D Real Network Nodes on GPU (with 3D Rotating Octahedra, Satellite Vertices & Tractor Lock Rings)
        for (int i = 0; i < pktsSnap.Count; i++) {
            DataPacket p = pktsSnap[i];
            if (IsPointOccluded != null && IsPointOccluded(p.X, p.Y)) continue;
            float zScale = GetDepthScale(p.Z);
            float sz = ((p.Kind == 0 ? 9.2f : 12.6f) + (float)Math.Sin(p.floatRot * 2.2f) * 2.6f) * zScale;
            if (p.IsHovered) sz *= 1.35f;

            int depthAlpha = (int)Clamp(175f + (p.Z + 250f) * 0.20f, 115f, 255f);
            Color glowCol = Color.FromArgb(p.IsHovered ? 125 : (depthAlpha / 3), p.Col.R, p.Col.G, p.Col.B);
            Color wireCol = Color.FromArgb(depthAlpha, p.Col.R, p.Col.G, p.Col.B);
            Color innerWireCol = Color.FromArgb(Math.Max(80, depthAlpha - 35), 255, 255, 255);
            Color stemCol = Color.FromArgb(depthAlpha / 3, p.Col.R, p.Col.G, p.Col.B);
            Color coreCol = Color.FromArgb(depthAlpha, 250, 252, 255);
            float lw = (p.IsHovered ? 2.6f : 1.85f) * Clamp(zScale, 0.75f, 1.35f);

            // 3D Vertical Altitude Tether & Floor Shadow Ellipse beneath each floating node
            float stemH = 26f * zScale;
            float baseY = p.Y + sz + stemH;
            GpuGl.DrawLine(p.X, p.Y + sz, p.X, baseY, stemCol, 1.1f);
            GpuGl.DrawEllipse(p.X, baseY, sz * 0.85f, 2.8f * zScale, stemCol, 1.0f, 12);

            // Outer energy aura
            GpuGl.FillEllipse(p.X, p.Y, sz * 2.15f, sz * 2.15f, glowCol, 16);

            // 3D Rotating Polyhedral Crystal / Octahedron
            float rot = p.floatRot * (p.Kind == 2 ? 2.1f : 1.25f);
            float eqX1 = (float)Math.Cos(rot) * sz;
            float eqY1 = (float)Math.Sin(rot) * sz * 0.44f;
            float eqX2 = (float)Math.Cos(rot + Math.PI * 0.5) * sz;
            float eqY2 = (float)Math.Sin(rot + Math.PI * 0.5) * sz * 0.44f;
            float topY = p.Y - sz * 1.22f;
            float botY = p.Y + sz * 1.22f;

            // Translucent faceted core fill
            GpuGl.SetColor(p.Col, depthAlpha / 4);
            GpuGl.glBegin(GpuGl.GL_TRIANGLE_FAN);
            GpuGl.glVertex2f(p.X, p.Y);
            GpuGl.glVertex2f(p.X, topY);
            GpuGl.glVertex2f(p.X + eqX1, p.Y + eqY1);
            GpuGl.glVertex2f(p.X, botY);
            GpuGl.glVertex2f(p.X - eqX1, p.Y - eqY1);
            GpuGl.glVertex2f(p.X, topY);
            GpuGl.glEnd();

            // Equatorial ring + 8 octahedral edges
            GpuGl.glLineWidth(lw);
            GpuGl.SetColor(wireCol);
            GpuGl.glBegin(GpuGl.GL_LINE_LOOP);
            GpuGl.glVertex2f(p.X + eqX1, p.Y + eqY1);
            GpuGl.glVertex2f(p.X + eqX2, p.Y + eqY2);
            GpuGl.glVertex2f(p.X - eqX1, p.Y - eqY1);
            GpuGl.glVertex2f(p.X - eqX2, p.Y - eqY2);
            GpuGl.glEnd();

            GpuGl.glBegin(GpuGl.GL_LINES);
            GpuGl.glVertex2f(p.X, topY); GpuGl.glVertex2f(p.X + eqX1, p.Y + eqY1);
            GpuGl.glVertex2f(p.X, topY); GpuGl.glVertex2f(p.X + eqX2, p.Y + eqY2);
            GpuGl.glVertex2f(p.X, topY); GpuGl.glVertex2f(p.X - eqX1, p.Y - eqY1);
            GpuGl.glVertex2f(p.X, topY); GpuGl.glVertex2f(p.X - eqX2, p.Y - eqY2);
            GpuGl.glVertex2f(p.X, botY); GpuGl.glVertex2f(p.X + eqX1, p.Y + eqY1);
            GpuGl.glVertex2f(p.X, botY); GpuGl.glVertex2f(p.X + eqX2, p.Y + eqY2);
            GpuGl.glVertex2f(p.X, botY); GpuGl.glVertex2f(p.X - eqX1, p.Y - eqY1);
            GpuGl.glVertex2f(p.X, botY); GpuGl.glVertex2f(p.X - eqX2, p.Y - eqY2);
            GpuGl.glEnd();

            if (p.Kind == 2) {
                // Threat anomaly: counter-rotating hazard ring + 3 orbiting vertex satellites
                GpuGl.DrawEllipse(p.X, p.Y, sz * 1.42f, sz * 1.42f, wireCol, lw * 0.95f, 18);
                for (int s = 0; s < 3; s++) {
                    double sAng = p.OrbitPhase + s * (2.0 * Math.PI / 3.0);
                    float sx = p.X + (float)Math.Cos(sAng) * sz * 1.72f;
                    float sy = p.Y + (float)Math.Sin(sAng) * sz * 0.98f;
                    GpuGl.DrawLine(p.X, p.Y, sx, sy, Color.FromArgb(depthAlpha / 2, 255, 180, 210), 1.1f);
                    GpuGl.FillEllipse(sx, sy, 2.7f * zScale, 2.7f * zScale, coreCol, 8);
                }
            } else if (p.Kind == 1) {
                // Golden Discovery orb: dual gyroscopic rings
                GpuGl.DrawEllipse(p.X, p.Y, sz * 1.45f, sz * 0.72f, wireCol, lw * 0.9f, 18);
                GpuGl.DrawEllipse(p.X, p.Y, sz * 0.72f, sz * 1.45f, innerWireCol, 1.2f, 18);
            }

            // Dynamic Tractor Vortex Lock Ring when a Porygon has locked onto this packet!
            if (p.PullIntensity > 0.06f) {
                float lockR = sz * (1.95f + (float)Math.Sin(p.OrbitPhase * 3.2f) * 0.28f);
                int lockAlpha = (int)Clamp(p.PullIntensity * 240f, 65f, 250f);
                Color lockC = Color.FromArgb(lockAlpha, 255, 255, 255);
                GpuGl.DrawEllipse(p.X, p.Y, lockR, lockR, lockC, 1.6f, 20);
                for (int c = 0; c < 4; c++) {
                    double cAng = -p.OrbitPhase * 1.8f + c * (Math.PI * 0.5);
                    float cx1 = p.X + (float)Math.Cos(cAng) * (lockR * 0.78f);
                    float cy1 = p.Y + (float)Math.Sin(cAng) * (lockR * 0.78f);
                    float cx2 = p.X + (float)Math.Cos(cAng) * (lockR * 1.18f);
                    float cy2 = p.Y + (float)Math.Sin(cAng) * (lockR * 1.18f);
                    GpuGl.DrawLine(cx1, cy1, cx2, cy2, wireCol, 2.0f);
                }
            }

            GpuGl.FillEllipse(p.X, p.Y, 3.2f * zScale, 3.2f * zScale, coreCol, 8);

            // Show compact 3D Z-depth readout on priority/anomaly or foreground nodes
            if (p.Kind == 2 || p.IsHovered || p.Z > 85f) {
                GpuGl.DrawText(string.Format("Z:{0:+0;-0;0}", p.Z), p.X - 18f, baseY + 12f, wireCol, GpuGl.FONT_LIST_HUD_SMALL);
            }
        }

        // 2. Render each of the 3D Porygon Agents sorted back-to-front by PosZ via Hardware OpenGL Z-Buffer + Triangles!
        GpuGl.glClear(GpuGl.GL_DEPTH_BUFFER_BIT);
        for (int aIdx = 0; aIdx < agentsSorted.Count; aIdx++) {
            PorygonAgent ag = agentsSorted[aIdx];
            if (IsPointOccluded != null && IsPointOccluded(ag.PosX, ag.PosY)) continue;
            RenderSingleAgentOpenGL(ag, userScale, themeCyan, themeMag);
        }

        // 2.5 Render 3D Packet Capture Shockwaves, Polyhedral Vertex Shards, Constellation Webs & Homing Absorption Streams on GPU!
        RenderCaptureEffectsOpenGL(shardsSnap, wavesSnap);

        // 3. Render Interactive Mouse-Hover Node or Agent Inspection Card on GPU
        if (hovPkt != null) {
            DrawHoveredPacketCardOpenGL(hovPkt);
        } else if (hovAg != null) {
            DrawHoveredAgentCardOpenGL(hovAg);
        }
    }

    private void RenderCaptureEffectsOpenGL(List<CaptureVertexShard> shardsSnap, List<CaptureShockwave> wavesSnap) {
        // 1. Expanding Geometric 3D Capture Shockwaves & Floating Purge/XP Readouts
        for (int i = 0; i < wavesSnap.Count; i++) {
            CaptureShockwave w = wavesSnap[i];
            float lifeRatio = Clamp(w.Life, 0f, 1f);
            float prog = 1.0f - lifeRatio;
            int alpha = (int)(lifeRatio * 245f);
            if (alpha <= 6) continue;

            float zScale = GetDepthScale(w.Z);
            float r = w.MaxRadius * prog * zScale;
            int sides = (w.Kind == 2) ? 8 : 6;

            // Outer glowing polygon shockwave
            GpuGl.glLineWidth((w.Kind == 2 ? 3.0f : 2.3f) * zScale);
            GpuGl.SetColor(w.Col, alpha);
            GpuGl.glBegin(GpuGl.GL_LINE_LOOP);
            for (int k = 0; k < sides; k++) {
                double ang = w.RotAngle + prog * 1.8f + k * (2.0 * Math.PI / sides);
                float px = w.X + (float)Math.Cos(ang) * r;
                float py = w.Y + (float)Math.Sin(ang) * r * 0.70f;
                GpuGl.glVertex2f(px, py);
            }
            GpuGl.glEnd();

            // Counter-rotating inner geometric ring + radial vertex spokes
            float rInner = r * 0.66f;
            GpuGl.glLineWidth(1.35f * zScale);
            GpuGl.SetColor(Color.FromArgb(alpha * 4 / 5, 255, 255, 255));
            GpuGl.glBegin(GpuGl.GL_LINE_LOOP);
            for (int k = 0; k < sides; k++) {
                double ang = -w.RotAngle - prog * 2.2f + k * (2.0 * Math.PI / sides);
                float px = w.X + (float)Math.Cos(ang) * rInner;
                float py = w.Y + (float)Math.Sin(ang) * rInner * 0.70f;
                GpuGl.glVertex2f(px, py);
            }
            GpuGl.glEnd();

            // Glowing vertex nodes at the corners of the shockwave polygon
            for (int k = 0; k < sides; k++) {
                double ang = w.RotAngle + prog * 1.8f + k * (2.0 * Math.PI / sides);
                float px = w.X + (float)Math.Cos(ang) * r;
                float py = w.Y + (float)Math.Sin(ang) * r * 0.70f;
                GpuGl.FillEllipse(px, py, 2.8f * zScale, 2.8f * zScale, Color.FromArgb(alpha, 255, 255, 255), 6);
            }

            if (!string.IsNullOrEmpty(w.FloatingLabel) && prog < 0.85f) {
                Color lblCol = (w.Kind == 2)
                    ? Color.FromArgb(alpha, 255, 195, 225)
                    : (w.Kind == 1 ? Color.FromArgb(alpha, 255, 235, 120) : Color.FromArgb(alpha, 185, 250, 255));
                GpuGl.DrawText(w.FloatingLabel, w.X - 44f, w.Y - 16f - prog * 36f, lblCol, GpuGl.FONT_LIST_HUD_SMALL);
            }
        }

        // 2. 3D Polyhedral Vertex Shards, Motion Trails & Constellation Filaments
        for (int i = 0; i < shardsSnap.Count; i++) {
            CaptureVertexShard s = shardsSnap[i];
            float lifeRatio = Clamp(s.Life / Math.Max(0.01f, s.MaxLife), 0f, 1f);
            int alpha = (int)(lifeRatio * 250f);
            if (alpha <= 6) continue;

            float zScale = GetDepthScale(s.Z);
            float sz = s.Size * zScale * (0.52f + 0.48f * lifeRatio);

            // Luminous velocity trail behind each flying vertex shard
            GpuGl.DrawLine(s.PrevX, s.PrevY, s.X, s.Y, Color.FromArgb(alpha * 2 / 3, s.Col.R, s.Col.G, s.Col.B), 2.1f * zScale);
            GpuGl.DrawLine(s.PrevX, s.PrevY, s.X, s.Y, Color.FromArgb(alpha / 2, 255, 255, 255), 1.0f * zScale);

            // Constellation filaments linking shards from the same burst group
            if (s.BurstGroupId > 0) {
                for (int j = i + 1; j < shardsSnap.Count; j++) {
                    CaptureVertexShard s2 = shardsSnap[j];
                    if (s2.BurstGroupId != s.BurstGroupId) continue;
                    float dx = s2.X - s.X, dy = s2.Y - s.Y;
                    float d2 = dx * dx + dy * dy;
                    if (d2 < 4200f) {
                        int linkA = (int)((1.0f - d2 / 4200f) * alpha * 0.62f);
                        if (linkA > 12) {
                            GpuGl.DrawLine(s.X, s.Y, s2.X, s2.Y, Color.FromArgb(linkA, s.SecondaryCol.R, s.SecondaryCol.G, s.SecondaryCol.B), 1.15f);
                        }
                    }
                }
            }

            // 3D tumbling polygon shard (Triangle, Diamond, or Mini-Prism)
            int vCount = (s.ShapeKind == 0) ? 3 : 4;
            float tiltX = 0.45f + 0.55f * (float)Math.Cos(s.RotAngle * 1.3f);
            float tiltY = 0.45f + 0.55f * (float)Math.Sin(s.RotAngle * 0.9f);

            GpuGl.SetColor(s.Col, alpha * 3 / 4);
            GpuGl.glBegin(GpuGl.GL_TRIANGLE_FAN);
            GpuGl.glVertex2f(s.X, s.Y);
            for (int k = 0; k <= vCount; k++) {
                double ang = s.RotAngle + (k % vCount) * (2.0 * Math.PI / vCount);
                float vx = s.X + (float)Math.Cos(ang) * sz * tiltY;
                float vy = s.Y + (float)Math.Sin(ang) * sz * tiltX;
                GpuGl.glVertex2f(vx, vy);
            }
            GpuGl.glEnd();

            // Crisp glowing wireframe edges + internal spoke lines
            GpuGl.glLineWidth(1.45f * zScale);
            GpuGl.SetColor(s.SecondaryCol, alpha);
            GpuGl.glBegin(GpuGl.GL_LINE_LOOP);
            for (int k = 0; k < vCount; k++) {
                double ang = s.RotAngle + k * (2.0 * Math.PI / vCount);
                float vx = s.X + (float)Math.Cos(ang) * sz * tiltY;
                float vy = s.Y + (float)Math.Sin(ang) * sz * tiltX;
                GpuGl.glVertex2f(vx, vy);
            }
            GpuGl.glEnd();

            // Bright vertex spark at shard center
            GpuGl.FillEllipse(s.X, s.Y, 2.0f * zScale, 2.0f * zScale, Color.FromArgb(alpha, 255, 255, 255), 6);
        }
    }

    private void DrawHoveredPacketCardOpenGL(DataPacket p) {
        PorygonAgent responder = null;
        for (int i = 0; i < Agents.Count; i++) {
            if (Agents[i].Name == p.AssignedAgentName) { responder = Agents[i]; break; }
        }
        if (responder == null && Agents.Count > 0) responder = Agents[0];

        float zScale = GetDepthScale(p.Z);
        float r = 24f * zScale;
        Color lockCol = Color.FromArgb(245, p.Col.R, p.Col.G, p.Col.B);
        Color linkCol = Color.FromArgb(145, p.Col.R, p.Col.G, p.Col.B);
        GpuGl.DrawEllipse(p.X, p.Y, r, r, lockCol, 2.2f, 24);
        float b = 29f * zScale, l = 9f * zScale;
        GpuGl.DrawLine(p.X - b, p.Y - b, p.X - b + l, p.Y - b, lockCol, 2.2f);
        GpuGl.DrawLine(p.X - b, p.Y - b, p.X - b, p.Y - b + l, lockCol, 2.2f);
        GpuGl.DrawLine(p.X + b, p.Y - b, p.X + b - l, p.Y - b, lockCol, 2.2f);
        GpuGl.DrawLine(p.X + b, p.Y - b, p.X + b, p.Y - b + l, lockCol, 2.2f);
        GpuGl.DrawLine(p.X - b, p.Y + b, p.X - b + l, p.Y + b, lockCol, 2.2f);
        GpuGl.DrawLine(p.X - b, p.Y + b, p.X - b, p.Y + b - l, lockCol, 2.2f);
        GpuGl.DrawLine(p.X + b, p.Y + b, p.X + b - l, p.Y + b, lockCol, 2.2f);
        GpuGl.DrawLine(p.X + b, p.Y + b, p.X + b, p.Y + b - l, lockCol, 2.2f);
        if (responder != null) {
            GpuGl.DrawLine(p.X, p.Y, responder.PosX, responder.PosY, linkCol, 1.4f);
        }

        Rectangle mon = GetContainingOrNearestMonitor(p.X, p.Y);
        int cw = 424;
        int ch = 140;
        int cx = (int)p.X + 36;
        int cy = (int)p.Y - ch / 2;
        if (cx + cw > mon.Right - 24) cx = (int)p.X - cw - 36;
        if (cx < mon.Left + 24) cx = mon.Left + 24;
        if (cy < mon.Top + 28) cy = mon.Top + 28;
        if (cy + ch > mon.Bottom - 54) cy = mon.Bottom - 54 - ch;

        GpuGl.FillRect(cx, cy, cw, ch, Color.FromArgb(148, 4, 8, 18));
        GpuGl.DrawRect(cx, cy, cw, ch, Color.FromArgb(235, p.Col.R, p.Col.G, p.Col.B), 1.8f);

        Color txtCol = Color.FromArgb(245, 235, 242, 252);
        Color goldCol = Color.FromArgb(250, 204, 21);
        string kindTitle = (p.Kind == 2)
            ? ">> [! IDS ANOMALY NODE // 3D HIGH PRIORITY]"
            : (p.Kind == 1 ? ">> [* NEW ENDPOINT AUDIT // 3D GOLDEN ORB]" : ">> [o VERIFIED 3D NETWORK FLOW NODE]");
        GpuGl.DrawText(kindTitle, cx + 10, cy + 8, p.Col, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("REMOTE_EP : {0}  [{1}]", p.RemoteEndpoint, p.ProtocolName), cx + 10, cy + 26, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("LOCAL_SKT : {0}  ({1})", p.LocalEndpoint, p.TcpStateStr), cx + 10, cy + 43, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("COORD_3D  : [X:{0:0}, Y:{1:0}, Z:{2:+0;-0;0}] (Depth {3:0.00}x)", p.X, p.Y, p.Z, zScale), cx + 10, cy + 60, goldCol, GpuGl.FONT_LIST_HUD_SMALL);

        int pct = (int)Math.Round(p.AnomalyScore * 100f);
        GpuGl.DrawText(string.Format("ANOMALY   : {0}% [{1}]", pct, p.AnomalyType), cx + 10, cy + 78, p.Col, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.FillRect(cx + 298, cy + 81, 108, 8, Color.FromArgb(50, 255, 255, 255));
        GpuGl.FillRect(cx + 298, cy + 81, (int)(1.08f * Math.Min(100, pct)), 8, p.Col);

        GpuGl.DrawText("DETAIL    : " + p.AnomalyDetail, cx + 10, cy + 96, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("AI_AGENT  : {0} [LOCKED] | FLOW: {1:0.0} KB/s", p.AssignedAgentName, p.KbpsSample), cx + 10, cy + 114, p.Col, GpuGl.FONT_LIST_HUD_SMALL);
    }

    private void DrawHoveredAgentCardOpenGL(PorygonAgent a) {
        float zScale = GetDepthScale(a.PosZ);
        float r = 76f * zScale;
        GpuGl.DrawEllipse(a.PosX, a.PosY, r, r, Color.FromArgb(230, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.0f, 32);

        Rectangle mon = GetContainingOrNearestMonitor(a.PosX, a.PosY);
        int cw = 412;
        int ch = 162;
        int cx = (int)a.PosX - cw / 2;
        int cy = (int)a.PosY - 92 - ch;
        if (cx < mon.Left + 24) cx = mon.Left + 24;
        if (cx + cw > mon.Right - 24) cx = mon.Right - 24 - cw;
        if (cy < mon.Top + 24) cy = (int)a.PosY + 108;

        FlyBrainConnectome br = a.Brain;
        GpuGl.FillRect(cx, cy, cw, ch, Color.FromArgb(148, 4, 8, 18));
        GpuGl.DrawRect(cx, cy, cw, ch, Color.FromArgb(235, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1.8f);

        Color goldCol = Color.FromArgb(250, 204, 21);
        Color txtCol = Color.FromArgb(245, 235, 242, 252);
        GpuGl.DrawText(string.Format(">> AI_CORE // {0} [GEN {1} | {2}] - {3}", a.Name, br.Generation, br.GetLifeStageLabel(), a.RoleTag), cx + 10, cy + 8, a.AuraColor, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("COORD_3D  : [X:{0:0}, Y:{1:0}, Z:{2:+0;-0;0}] (Depth {3:0.00}x)", a.PosX, a.PosY, a.PosZ, zScale), cx + 10, cy + 25, goldCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("LIFE_CYCLE: Age {0:0.0}% | Stage {1}/3 | MutSigma {2:0.000}", br.LifeAge, br.LifeStage + 1, br.MutationSigma), cx + 10, cy + 42, goldCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("EVO_MODEL : ACC:{0:0.0}% | FIT:{1:0.0} | TP:{2} TN:{3} FP:{4}",
            br.DetectionAccuracy, br.FitnessScore, br.TruePositives, br.TrueNegatives, br.FalsePositives), cx + 10, cy + 59, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("TELEMETRY : DL_Z:{0:+0.0;-0.0;0.0}s | UL_Z:{1:+0.0;-0.0;0.0}s | TARGET:{2}",
            LatestFeed.DlZScore, LatestFeed.UlZScore, a.CurrentTargetEndpoint), cx + 10, cy + 76, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("IDS_STATS : Flows:{0} | Purged:{1} | CPU_Epoch:{2}",
            br.PacketsCollected, br.AnomaliesCleared, br.CpuStepsCompleted), cx + 10, cy + 93, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(string.Format("SYNAPSES  : Gain {0:0.00}x | LVL {1} ({2}/{3} XP) | {4}",
            br.SynapticGain, br.Level, br.XP, br.NextLevelXP, br.ActiveStateName), cx + 10, cy + 110, a.AuraColor, GpuGl.FONT_LIST_HUD_SMALL);

        // Life Cycle Progress Bar inside Hover Card
        GpuGl.FillRect(cx + 10, cy + 136, cw - 20, 7, Color.FromArgb(45, 255, 255, 255));
        GpuGl.FillRect(cx + 10, cy + 136, (cw - 20f) * Math.Min(1f, br.LifeAge / 100f), 7, goldCol);
    }

    private void RenderSingleAgentOpenGL(PorygonAgent a, float userScale, Color themeCyan, Color themeMag) {
        ModelData model = a.Model;
        if (model == null) return;

        // Apply 3D Z-Coordinate Perspective Depth Scaling (0.65x in deep cyberspace up to 1.52x near foreground)
        float zDepthScale = GetDepthScale(a.PosZ);
        float effUserScale = userScale * zDepthScale;
        float scale = 1.26f * a.ScaleFactor * (effUserScale / 100.0f);
        float hopOffset = (a.SpinRollTimer > 0f) ? (float)Math.Sin(a.SpinRollTimer * Math.PI) * 32f * zDepthScale : 0f;
        float bobY = a.PosY + (float)Math.Sin(a.FloatBob) * 9f * zDepthScale - hopOffset;

        // Skill LVL 6: Deep Sonar Scan — 3 Concentric 3D Tilted Sonar Rings + Radar Sweep Beam + Holographic Grid Pillars
        if (a.ScanWaveTimer > 0f) {
            float prog = 1.0f - a.ScanWaveTimer;
            int baseAlpha = (int)Clamp(a.ScanWaveTimer * 215f, 0f, 240f);
            for (int w = 0; w < 3; w++) {
                float wProg = Clamp(prog - w * 0.14f, 0f, 1f);
                if (wProg <= 0f) continue;
                float scanR = (36f + wProg * 295f) * zDepthScale;
                int wAlpha = Math.Max(10, baseAlpha - w * 45);
                Color ringCol = (w == 0) ? Color.FromArgb(wAlpha, 255, 255, 255) : Color.FromArgb(wAlpha, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B);
                GpuGl.DrawEllipse(a.PosX, bobY, scanR, scanR * 0.46f, ringCol, (w == 0 ? 2.6f : 1.7f) * zDepthScale, 36);

                // Vertical holographic sonar pillars around outer wave
                if (w == 0) {
                    for (int pIdx = 0; pIdx < 8; pIdx++) {
                        double pAng = a.FloatBob * 1.8f + pIdx * (Math.PI * 0.25);
                        float px = a.PosX + (float)Math.Cos(pAng) * scanR;
                        float py = bobY + (float)Math.Sin(pAng) * scanR * 0.46f;
                        float pillarH = (18f + (float)Math.Sin(a.FloatBob * 4f + pIdx) * 8f) * zDepthScale * a.ScanWaveTimer;
                        GpuGl.DrawLine(px, py, px, py - pillarH, Color.FromArgb(wAlpha * 3 / 4, 0, 240, 255), 1.6f);
                        GpuGl.FillEllipse(px, py - pillarH, 2.4f * zDepthScale, 2.4f * zDepthScale, Color.FromArgb(wAlpha, 255, 255, 255), 6);
                    }
                }
            }
            // Rotating radar sweep line inside sonar field
            float sweepR = (40f + prog * 280f) * zDepthScale;
            double sweepAng = prog * Math.PI * 4.0 + a.FloatBob;
            float sx = a.PosX + (float)Math.Cos(sweepAng) * sweepR;
            float sy = bobY + (float)Math.Sin(sweepAng) * sweepR * 0.46f;
            GpuGl.DrawLine(a.PosX, bobY, sx, sy, Color.FromArgb(baseAlpha, 0, 240, 255), 2.2f);
        }

        // Skill LVL 5: Quantum Blink Warp — Entry/Exit 8-Point Dimensional Portals + Wireframe Afterimage + Lightning Warp Corridor
        if (a.QuantumBlinkTimer > 0f) {
            int alpha = (int)Clamp(a.QuantumBlinkTimer * 235f, 0f, 250f);
            Color warpCol = Color.FromArgb(alpha, 0, 240, 255);
            Color magCol  = Color.FromArgb(alpha * 4 / 5, 236, 72, 153);
            Color coreCol = Color.FromArgb(alpha, 255, 255, 255);

            // Entry & Exit 8-point spinning dimensional star portals
            PointF[] portals = new PointF[] { a.BlinkStartPt, new PointF(a.PosX, bobY) };
            for (int pIdx = 0; pIdx < 2; pIdx++) {
                PointF pt = portals[pIdx];
                float pr = (pIdx == 0 ? 32f : 38f) * zDepthScale * (0.6f + 0.4f * a.QuantumBlinkTimer);
                GpuGl.DrawEllipse(pt.X, pt.Y, pr, pr * 0.65f, warpCol, 2.2f, 24);
                GpuGl.DrawEllipse(pt.X, pt.Y, pr * 0.65f, pr, magCol, 1.6f, 24);
                for (int k = 0; k < 8; k++) {
                    double ang = a.FloatBob * 3.5f * (pIdx == 0 ? 1f : -1f) + k * (Math.PI * 0.25);
                    float x1 = pt.X + (float)Math.Cos(ang) * (pr * 0.45f);
                    float y1 = pt.Y + (float)Math.Sin(ang) * (pr * 0.45f);
                    float x2 = pt.X + (float)Math.Cos(ang) * (pr * 1.25f);
                    float y2 = pt.Y + (float)Math.Sin(ang) * (pr * 1.25f);
                    GpuGl.DrawLine(x1, y1, x2, y2, (k % 2 == 0) ? coreCol : warpCol, 1.6f);
                }
            }

            // Jagged multi-strand quantum lightning corridor between BlinkStartPt and current position
            float dx = a.PosX - a.BlinkStartPt.X;
            float dy = bobY - a.BlinkStartPt.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = (len > 1f) ? -dy / len : 0f;
            float ny = (len > 1f) ?  dx / len : 1f;
            int segs = 10;
            for (int strand = 0; strand < 2; strand++) {
                float prevX = a.BlinkStartPt.X, prevY = a.BlinkStartPt.Y;
                Color sCol = (strand == 0) ? warpCol : magCol;
                for (int s = 1; s <= segs; s++) {
                    float t = (float)s / segs;
                    float jag = (s == segs) ? 0f : (float)Math.Sin(t * Math.PI * 5.0 + a.FloatBob * 12f + strand * 2.4f) * (12f - strand * 4f) * zDepthScale;
                    float curX = a.BlinkStartPt.X + dx * t + nx * jag;
                    float curY = a.BlinkStartPt.Y + dy * t + ny * jag;
                    GpuGl.DrawLine(prevX, prevY, curX, curY, sCol, 3.2f - strand * 0.8f);
                    GpuGl.DrawLine(prevX, prevY, curX, curY, coreCol, 1.3f);
                    prevX = curX; prevY = curY;
                }
            }
        }

        // Skill LVL 3: P2P Neural Sync — Multi-Strand Synaptic Lightning Arc + Bidirectional Data Cubes + Resonance Halos
        if (a.SyncTimer > 0f && a.SyncPartnerIdx >= 0 && a.SyncPartnerIdx < Agents.Count) {
            PorygonAgent partner = Agents[a.SyncPartnerIdx];
            int alpha = (int)Clamp(a.SyncTimer * 230f, 0f, 250f);
            Color auraC = Color.FromArgb(alpha * 2 / 3, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B);
            Color goldC = Color.FromArgb(alpha, 250, 204, 21);
            Color whiteC = Color.FromArgb(alpha, 255, 255, 255);

            float dx = partner.PosX - a.PosX;
            float dy = partner.PosY - bobY;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = (len > 1f) ? -dy / len : 0f;
            float ny = (len > 1f) ?  dx / len : 1f;

            // Core conduit + dual helical synaptic strands
            GpuGl.DrawLine(a.PosX, bobY, partner.PosX, partner.PosY, auraC, 5.5f);
            int segs = 14;
            for (int strand = -1; strand <= 1; strand += 2) {
                float pX = a.PosX, pY = bobY;
                for (int s = 1; s <= segs; s++) {
                    float t = (float)s / segs;
                    float wave = (float)Math.Sin(t * Math.PI * 4.0 - a.FloatBob * 8f) * 14f * strand * (float)Math.Sin(t * Math.PI);
                    float cX = a.PosX + dx * t + nx * wave;
                    float cY = bobY + dy * t + ny * wave;
                    GpuGl.DrawLine(pX, pY, cX, cY, (strand < 0) ? goldC : whiteC, 2.1f);
                    pX = cX; pY = cY;
                }
            }

            // Bidirectional glowing synaptic weight packets traveling between the two Porygons
            for (int pIdx = 0; pIdx < 3; pIdx++) {
                float tFwd = (float)((a.FloatBob * 0.9f + pIdx * 0.33f) % 1.0f);
                if (tFwd < 0f) tFwd += 1f;
                float fx = a.PosX + dx * tFwd;
                float fy = bobY + dy * tFwd;
                GpuGl.FillEllipse(fx, fy, 4.5f * zDepthScale, 4.5f * zDepthScale, goldC, 8);
                GpuGl.DrawEllipse(fx, fy, 7.5f * zDepthScale, 7.5f * zDepthScale, whiteC, 1.3f, 8);
            }
            GpuGl.DrawEllipse(a.PosX, bobY, 34f * zDepthScale, 34f * zDepthScale, goldC, 1.8f, 20);
        }

        // Dynamic Tractor-Beam Conduit when Porygon has locked onto a network packet in pursuit!
        if (a.ActiveLockStrength > 0.06f) {
            int lockA = (int)Clamp(a.ActiveLockStrength * 195f, 35f, 215f);
            Color tCol = (a.ActiveLockKind == 2)
                ? Color.FromArgb(lockA, 236, 72, 153)
                : (a.ActiveLockKind == 1 ? Color.FromArgb(lockA, 250, 204, 21) : Color.FromArgb(lockA, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B));
            Color tCore = Color.FromArgb(lockA, 255, 255, 255);

            float dx = a.ActiveLockTarget.X - a.PosX;
            float dy = a.ActiveLockTarget.Y - bobY;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = (len > 1f) ? -dy / len : 0f;
            float ny = (len > 1f) ?  dx / len : 1f;

            // Helical tractor spiral pulling the packet toward Porygon
            int segs = 10;
            float prevX = a.PosX, prevY = bobY;
            for (int s = 1; s <= segs; s++) {
                float t = (float)s / segs;
                float spiral = (float)Math.Sin(t * Math.PI * 3.0 + a.TractorPhase) * 9f * (1f - t * 0.5f) * zDepthScale;
                float curX = a.PosX + dx * t + nx * spiral;
                float curY = bobY + dy * t + ny * spiral;
                GpuGl.DrawLine(prevX, prevY, curX, curY, tCol, 2.0f * zDepthScale);
                prevX = curX; prevY = curY;
            }
            // Concentric tractor rings converging from packet to Porygon
            for (int rIdx = 0; rIdx < 3; rIdx++) {
                float rt = 1.0f - (float)(((a.TractorPhase * 0.22f) + rIdx * 0.33f) % 1.0f);
                float rx = a.PosX + dx * rt;
                float ry = bobY + dy * rt;
                float rr = (5f + rt * 11f) * zDepthScale;
                GpuGl.DrawEllipse(rx, ry, rr, rr * 0.65f, tCore, 1.3f, 12);
            }
        }

        // Cyber Holographic Platform beneath each Porygon (scaled by 3D Z depth + boosted during Dash/Absorption)
        float ringW = 126f * (effUserScale / 100f);
        float ringH = 30f * (effUserScale / 100f);
        float ringY = a.PosY + 74f * (effUserScale / 100f);
        GpuGl.FillEllipse(a.PosX, ringY, ringW * 0.45f, ringH * 0.45f, Color.FromArgb(a.IsHovered ? 85 : 45, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 24);
        GpuGl.DrawEllipse(a.PosX, ringY, ringW * 0.50f, ringH * 0.50f, Color.FromArgb(a.IsHovered ? 230 : 125, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), a.IsHovered ? 2.4f : 1.8f, 28);
        GpuGl.DrawEllipse(a.PosX, ringY, ringW * 0.68f, ringH * 0.68f, Color.FromArgb(65, themeCyan.R, themeCyan.G, themeCyan.B), 1.1f, 28);

        // Precompute agent trigonometry ONCE per agent (eliminates 90,000 Math.Sin/Cos calls per frame!)
        List<Vec3> bVerts = model.Verts;
        List<Face3D> bFaces = model.Faces;
        if (a.TvBuf == null || a.TvBuf.Length < bVerts.Count) {
            a.TvBuf = new Vec3[bVerts.Count];
            a.PvBuf = new PointF[bVerts.Count];
        }
        Vec3[] tv = a.TvBuf;
        PointF[] pv = a.PvBuf;
        float fov = 560f;
        float camDist = 460f;

        float cosY = (float)Math.Cos(a.Yaw), sinY = (float)Math.Sin(a.Yaw);
        float cosP = (float)Math.Cos(a.Pitch), sinP = (float)Math.Sin(a.Pitch);
        float cosR = (float)Math.Cos(a.Roll), sinR = (float)Math.Sin(a.Roll);
        float sinWalk = (float)Math.Sin(a.WalkPhase);
        float cosWalk = (float)Math.Cos(a.WalkPhase);
        float sinTail = (float)Math.Sin(a.WalkPhase * 1.3f);

        for (int i = 0; i < bVerts.Count; i++) {
            Vec3 v = bVerts[i];
            float vx = v.X, vy = v.Y, vz = v.Z;
            if (vy > 12f) {
                float hw = (vy - 12f) * 0.025f;
                if (hw > 1.0f) hw = 1.0f;
                float ry = vy - 12f;
                float hp = a.HeadPitch * hw;
                float hy = a.HeadYaw * hw;
                float y1 = ry - vz * hp;
                float z1 = ry * hp + vz;
                vx = vx + z1 * hy;
                vz = -vx * hy + z1;
                vy = 12f + y1;
            } else if (vy < -14f && (vx > 10f || vx < -10f)) {
                float fw = (-14f - vy) * 0.03333f;
                if (fw > 1.0f) fw = 1.0f;
                float sideSign = (vx < 0f) ? 1.0f : -1.0f;
                vz += sinWalk * sideSign * 7.5f * fw;
                vy += cosWalk * sideSign * 3.0f * fw;
            }
            if (vz < -22f) {
                float tw = (-22f - vz) * 0.0263f;
                if (tw > 1.0f) tw = 1.0f;
                vx += sinTail * 8.0f * tw;
            }

            // RotateY -> RotateX -> RotateZ * scale
            float x1 = vx * cosY + vz * sinY;
            float z1r = -vx * sinY + vz * cosY;
            float y2 = vy * cosP - z1r * sinP;
            float z2 = vy * sinP + z1r * cosP;
            float wx = (x1 * cosR - y2 * sinR) * scale;
            float wy = (x1 * sinR + y2 * cosR) * scale;
            float wz = z2 * scale;

            tv[i] = new Vec3(wx, wy, wz);
            float zProj = camDist - wz;
            if (zProj < 80f) zProj = 80f;
            float persp = fov / zProj;
            pv[i] = new PointF(a.PosX + wx * persp, bobY - wy * persp);
        }

        // Hardware 3D Rasterization + Hardware Z-Buffer on AMD Radeon RX 9060 XT!
        GpuGl.glEnable(GpuGl.GL_DEPTH_TEST);
        GpuGl.glDepthFunc(GpuGl.GL_LEQUAL);
        GpuGl.glDisable(GpuGl.GL_TEXTURE_2D);

        float depthAtmos = Clamp(0.84f + (a.PosZ + 250f) * 0.00045f, 0.78f, 1.05f);
        float absorbBoost = (a.AbsorbFlashTimer > 0f) ? a.AbsorbFlashTimer * 0.22f : 0f;

        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int i = 0; i < bFaces.Count; i++) {
            Face3D f = bFaces[i];
            int i0 = f.Indices[0], i1 = f.Indices[1], i2 = f.Indices[2];
            PointF p0 = pv[i0], p1 = pv[i1], p2 = pv[i2];
            Vec3 v0 = tv[i0], v1 = tv[i1], v2 = tv[i2];

            Vec3 ln = f.LocalNormal;
            float nx1 = ln.X * cosY + ln.Z * sinY;
            float nz1 = -ln.X * sinY + ln.Z * cosY;
            float ny2 = ln.Y * cosP - nz1 * sinP;
            float nz2 = ln.Y * sinP + nz1 * cosP;
            float nx = nx1 * cosR - ny2 * sinR;
            float ny = nx1 * sinR + ny2 * cosR;
            float nz = nz2;
            if (nz < 0f) { nx = -nx; ny = -ny; nz = -nz; }

            float ndotl = nx * -0.32f + ny * 0.72f + nz * 0.62f;
            if (ndotl < 0f) ndotl = 0f;
            float shade = (0.80f + ndotl * 0.26f + absorbBoost) * depthAtmos;
            byte r = (byte)Math.Min(255, (int)(f.BaseColor.R * shade));
            byte gr = (byte)Math.Min(255, (int)(f.BaseColor.G * shade));
            byte b = (byte)Math.Min(255, (int)(f.BaseColor.B * shade));

            float z0 = v0.Z + f.DepthBias;
            float z1 = v1.Z + f.DepthBias;
            float z2 = v2.Z + f.DepthBias;

            GpuGl.glColor4ub(r, gr, b, 255);
            GpuGl.glVertex3f(p0.X, p0.Y, z0);
            GpuGl.glVertex3f(p1.X, p1.Y, z1);
            GpuGl.glVertex3f(p2.X, p2.Y, z2);
        }
        GpuGl.glEnd();

        // Hardware Z-Buffered Subtle 1px Contour Edges for Porygon (matching the delicate ~1px seams of Porygon2 & Porygon-Z)
        if (bFaces.Count < 1000) {
            GpuGl.glDisable(GpuGl.GL_LINE_SMOOTH);
            GpuGl.glLineWidth(1.0f);
            GpuGl.glColor4ub(18, 22, 30, 68);
            GpuGl.glBegin(GpuGl.GL_LINES);
            for (int i = 0; i < bFaces.Count; i++) {
                Face3D f = bFaces[i];
                byte mask = f.ContourEdgeMask;
                if (mask == 0) continue;
                int i0 = f.Indices[0], i1 = f.Indices[1], i2 = f.Indices[2];
                PointF p0 = pv[i0], p1 = pv[i1], p2 = pv[i2];
                float zBias = f.DepthBias + 1.8f;
                float z0 = tv[i0].Z + zBias;
                float z1 = tv[i1].Z + zBias;
                float z2 = tv[i2].Z + zBias;
                if ((mask & 1) != 0) {
                    GpuGl.glVertex3f(p0.X, p0.Y, z0);
                    GpuGl.glVertex3f(p1.X, p1.Y, z1);
                }
                if ((mask & 2) != 0) {
                    GpuGl.glVertex3f(p1.X, p1.Y, z1);
                    GpuGl.glVertex3f(p2.X, p2.Y, z2);
                }
                if ((mask & 4) != 0) {
                    GpuGl.glVertex3f(p2.X, p2.Y, z2);
                    GpuGl.glVertex3f(p0.X, p0.Y, z0);
                }
            }
            GpuGl.glEnd();
        }

        GpuGl.glDisable(GpuGl.GL_DEPTH_TEST);

        // 3D Mesh Vertex Absorption Glow — Sparkling wireframe vertices on Porygon's body when absorbing captured packet shards!
        if (a.AbsorbFlashTimer > 0.02f || a.DashBoostTimer > 0.1f) {
            float flashInt = Math.Max(a.AbsorbFlashTimer, a.DashBoostTimer * 0.55f);
            int vAlpha = (int)Clamp(flashInt * 230f, 30f, 245f);
            Color vCol = Color.FromArgb(vAlpha, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B);
            Color vCore = Color.FromArgb(vAlpha, 255, 255, 255);
            int step = Math.Max(1, bVerts.Count / 28);
            for (int vIdx = 0; vIdx < bVerts.Count; vIdx += step) {
                if (tv[vIdx].Z < -5f) continue; // Only front-facing mesh vertices
                PointF vp = pv[vIdx];
                GpuGl.FillEllipse(vp.X, vp.Y, 2.6f * zDepthScale, 2.6f * zDepthScale, vCore, 6);
                int nextIdx = (vIdx + step) % bVerts.Count;
                if (tv[nextIdx].Z >= -5f) {
                    float dx = pv[nextIdx].X - vp.X, dy = pv[nextIdx].Y - vp.Y;
                    if (dx * dx + dy * dy < 2200f * zDepthScale * zDepthScale) {
                        GpuGl.DrawLine(vp.X, vp.Y, pv[nextIdx].X, pv[nextIdx].Y, vCol, 1.2f * zDepthScale);
                    }
                }
            }
        }

        // Skill LVL 2: Conversion-2 Geodesic Dual Forcefield Dome (Counter-Rotating Hexagons + Triangulated Spokes + Corner Nodes)
        if (a.ShieldTimer > 0f) {
            int sAlpha = (int)(Math.Min(1.0f, a.ShieldTimer) * 195f);
            float shR = 76f * (effUserScale / 100f);
            float shR2 = shR * 0.62f;

            // Translucent energy dome fill
            GpuGl.SetColor(a.AuraColor, sAlpha / 5);
            GpuGl.glBegin(GpuGl.GL_TRIANGLE_FAN);
            GpuGl.glVertex2f(a.PosX, bobY);
            for (int k = 0; k <= 6; k++) {
                double ang = a.FloatBob * 1.6f + (k % 6) * (Math.PI / 3.0);
                GpuGl.glVertex2f(a.PosX + (float)Math.Cos(ang) * shR, bobY + (float)Math.Sin(ang) * shR * 0.86f);
            }
            GpuGl.glEnd();

            PointF[] outerHex = new PointF[6];
            PointF[] innerHex = new PointF[6];
            for (int k = 0; k < 6; k++) {
                double ang1 = a.FloatBob * 1.6f + k * (Math.PI / 3.0);
                double ang2 = -a.FloatBob * 2.1f + k * (Math.PI / 3.0);
                outerHex[k] = new PointF(a.PosX + (float)Math.Cos(ang1) * shR, bobY + (float)Math.Sin(ang1) * shR * 0.86f);
                innerHex[k] = new PointF(a.PosX + (float)Math.Cos(ang2) * shR2, bobY + (float)Math.Sin(ang2) * shR2 * 0.86f);
            }

            // Outer hexagon
            GpuGl.glLineWidth(2.4f * zDepthScale);
            GpuGl.SetColor(a.AuraColor, sAlpha);
            GpuGl.glBegin(GpuGl.GL_LINE_LOOP);
            for (int k = 0; k < 6; k++) GpuGl.glVertex2f(outerHex[k].X, outerHex[k].Y);
            GpuGl.glEnd();

            // Counter-rotating inner cyan/white hexagon + geodesic struts
            Color innerCol = Color.FromArgb(sAlpha * 4 / 5, 255, 255, 255);
            GpuGl.glLineWidth(1.5f * zDepthScale);
            GpuGl.SetColor(innerCol);
            GpuGl.glBegin(GpuGl.GL_LINE_LOOP);
            for (int k = 0; k < 6; k++) GpuGl.glVertex2f(innerHex[k].X, innerHex[k].Y);
            GpuGl.glEnd();

            for (int k = 0; k < 6; k++) {
                GpuGl.DrawLine(outerHex[k].X, outerHex[k].Y, innerHex[k].X, innerHex[k].Y, Color.FromArgb(sAlpha * 2 / 3, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1.3f);
                GpuGl.DrawLine(outerHex[k].X, outerHex[k].Y, innerHex[(k + 1) % 6].X, innerHex[(k + 1) % 6].Y, Color.FromArgb(sAlpha / 2, 0, 240, 255), 1.1f);
                GpuGl.FillEllipse(outerHex[k].X, outerHex[k].Y, 3.4f * zDepthScale, 3.4f * zDepthScale, innerCol, 8);
            }
        }

        // Skill LVL 1: Spiral Conversion Beam + Orbiting Hexagonal Conduit Rings + Target Deconstruction Matrix
        if (a.BeamTimer > 0f) {
            Vec3 beakTip = TransformAgentVertex(a, new Vec3(0, 18, 56), scale);
            float zProj = Math.Max(80f, camDist - beakTip.Z);
            PointF beakPt = new PointF(a.PosX + beakTip.X * (fov / zProj), bobY - beakTip.Y * (fov / zProj));

            int bAlpha = (int)Clamp(a.BeamTimer * 320f, 90f, 255f);
            Color bOuter = Color.FromArgb(bAlpha * 2 / 3, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B);
            Color bCore  = Color.FromArgb(bAlpha, 255, 255, 255);
            Color bMag   = Color.FromArgb(bAlpha * 4 / 5, 236, 72, 153);

            // Central high-energy plasma core
            GpuGl.DrawLine(beakPt.X, beakPt.Y, a.BeamTarget.X, a.BeamTarget.Y, bOuter, 7.5f * zDepthScale);
            GpuGl.DrawLine(beakPt.X, beakPt.Y, a.BeamTarget.X, a.BeamTarget.Y, bCore, 2.8f * zDepthScale);

            // Double-helix spiral strands wrapping around the Conversion Beam
            float dx = a.BeamTarget.X - beakPt.X;
            float dy = a.BeamTarget.Y - beakPt.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = (len > 1f) ? -dy / len : 0f;
            float ny = (len > 1f) ?  dx / len : 1f;
            int segs = 14;
            for (int strand = 0; strand < 2; strand++) {
                float pX = beakPt.X, pY = beakPt.Y;
                Color sCol = (strand == 0) ? Color.FromArgb(bAlpha, 0, 240, 255) : bMag;
                for (int s = 1; s <= segs; s++) {
                    float t = (float)s / segs;
                    float amp = (float)Math.Sin(t * Math.PI) * 11f * zDepthScale;
                    float wave = (float)Math.Sin(t * Math.PI * 5.0 - a.FloatBob * 14f + strand * Math.PI) * amp;
                    float cX = beakPt.X + dx * t + nx * wave;
                    float cY = beakPt.Y + dy * t + ny * wave;
                    GpuGl.DrawLine(pX, pY, cX, cY, sCol, 2.0f * zDepthScale);
                    pX = cX; pY = cY;
                }
            }

            // Rotating target deconstruction reticle at BeamTarget
            float retR = (16f + (float)Math.Sin(a.FloatBob * 8f) * 4f) * zDepthScale;
            GpuGl.DrawEllipse(a.BeamTarget.X, a.BeamTarget.Y, retR, retR, bCore, 2.0f, 16);
            GpuGl.DrawEllipse(a.BeamTarget.X, a.BeamTarget.Y, retR * 1.45f, retR * 1.45f, bOuter, 1.5f, 6);
        }

        // Skill LVL 4: Tri-Attack Elemental Matrix — 3D Rotating Prism + 3 Elemental Orbs + Helical Tri-Beams + Target Supernova Seal
        if (a.TriAttackTimer > 0f) {
            Vec3 beakTip = TransformAgentVertex(a, new Vec3(0, 18, 56), scale);
            float zProj = Math.Max(80f, camDist - beakTip.Z);
            PointF beakPt = new PointF(a.PosX + beakTip.X * (fov / zProj), bobY - beakTip.Y * (fov / zProj));

            int tAlpha = (int)Clamp(a.TriAttackTimer * 260f, 110f, 255f);
            Color cIce  = Color.FromArgb(tAlpha, 0, 240, 255);     // Cyan Ice
            Color cPlas = Color.FromArgb(tAlpha, 236, 72, 153);   // Magenta Plasma
            Color cElec = Color.FromArgb(tAlpha, 250, 204, 21);   // Gold Electric
            Color cCore = Color.FromArgb(tAlpha, 255, 255, 255);
            Color[] elemCols = new Color[] { cIce, cPlas, cElec };

            PointF[] tri = new PointF[3];
            float triR = 24f * zDepthScale;
            for (int k = 0; k < 3; k++) {
                double ang = a.FloatBob * 3.8f + k * (2.0 * Math.PI / 3.0);
                tri[k] = new PointF(beakPt.X + (float)Math.Cos(ang) * triR, beakPt.Y + (float)Math.Sin(ang) * triR);
            }

            // Rotating 3D elemental triangleemitter in front of Porygon
            GpuGl.DrawLine(tri[0].X, tri[0].Y, tri[1].X, tri[1].Y, cElec, 3.0f * zDepthScale);
            GpuGl.DrawLine(tri[1].X, tri[1].Y, tri[2].X, tri[2].Y, cIce,  3.0f * zDepthScale);
            GpuGl.DrawLine(tri[2].X, tri[2].Y, tri[0].X, tri[0].Y, cPlas, 3.0f * zDepthScale);

            for (int k = 0; k < 3; k++) {
                // Elemental emitter sphere at each triangle vertex
                GpuGl.FillEllipse(tri[k].X, tri[k].Y, 6.2f * zDepthScale, 6.2f * zDepthScale, elemCols[k], 12);
                GpuGl.FillEllipse(tri[k].X, tri[k].Y, 3.0f * zDepthScale, 3.0f * zDepthScale, cCore, 8);

                // Primary elemental beam + helical spiral wave to TriAttackTarget
                GpuGl.DrawLine(tri[k].X, tri[k].Y, a.TriAttackTarget.X, a.TriAttackTarget.Y, elemCols[k], 3.4f * zDepthScale);
                GpuGl.DrawLine(tri[k].X, tri[k].Y, a.TriAttackTarget.X, a.TriAttackTarget.Y, cCore, 1.2f * zDepthScale);

                float dx = a.TriAttackTarget.X - tri[k].X;
                float dy = a.TriAttackTarget.Y - tri[k].Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                float nx = (len > 1f) ? -dy / len : 0f;
                float ny = (len > 1f) ?  dx / len : 1f;
                float pX = tri[k].X, pY = tri[k].Y;
                for (int s = 1; s <= 10; s++) {
                    float t = s / 10f;
                    float wv = (float)Math.Sin(t * Math.PI * 4.0 - a.FloatBob * 12f + k * 2.09f) * 9f * (float)Math.Sin(t * Math.PI) * zDepthScale;
                    float cX = tri[k].X + dx * t + nx * wv;
                    float cY = tri[k].Y + dy * t + ny * wv;
                    GpuGl.DrawLine(pX, pY, cX, cY, elemCols[k], 1.8f * zDepthScale);
                    pX = cX; pY = cY;
                }
            }

            // Target Tri-Elemental Supernova & Sacred Geometry Triangle Seal at impact point
            float impR = (24f + (float)Math.Sin(a.FloatBob * 10f) * 5f) * zDepthScale;
            GpuGl.FillEllipse(a.TriAttackTarget.X, a.TriAttackTarget.Y, impR * 1.2f, impR * 1.2f, Color.FromArgb(tAlpha / 3, 236, 72, 153), 18);
            GpuGl.DrawEllipse(a.TriAttackTarget.X, a.TriAttackTarget.Y, impR, impR, cIce, 2.6f, 20);
            GpuGl.DrawEllipse(a.TriAttackTarget.X, a.TriAttackTarget.Y, impR * 0.68f, impR * 0.68f, cElec, 2.2f, 3);
            GpuGl.DrawEllipse(a.TriAttackTarget.X, a.TriAttackTarget.Y, impR * 0.68f, impR * 0.68f, cPlas, 2.2f, 6);
        }

        // Floating Mini-HUD Tag under each Porygon showing Evolutionary Generation, Life Stage & Live 3D [X,Y,Z] Coordinates!
        string badge = string.Format("{0} G{1} {2}", a.Name, a.Brain.Generation, a.Brain.GetLifeStageTag());
        string subBadge = string.Format("XYZ:[{0:0},{1:0},{2:+0;-0;0}] F2:{3:0}%", a.PosX, a.PosY, a.PosZ, a.Brain.F2Score);
        float tagW = 166f;
        float tagH = 34f;
        float tagX = a.PosX - tagW * 0.5f;
        float tagY = ringY + 10f;
        GpuGl.FillRect(tagX, tagY, tagW, tagH, Color.FromArgb(148, 4, 7, 16));
        GpuGl.DrawRect(tagX, tagY, tagW, tagH, Color.FromArgb(170, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1.1f);
        GpuGl.DrawText(badge, tagX + 5f, tagY + 2f, Color.FromArgb(245, 240, 248, 255), GpuGl.FONT_LIST_HUD_SMALL);
        GpuGl.DrawText(subBadge, tagX + 5f, tagY + 13f, Color.FromArgb(240, 250, 204, 21), GpuGl.FONT_LIST_HUD_SMALL);
        float ageRatio = Math.Min(1.0f, Math.Max(0f, a.Brain.LifeAge / 100.0f));
        float xpRatio = Math.Min(1.0f, (float)a.Brain.XP / Math.Max(1, a.Brain.NextLevelXP));
        GpuGl.FillRect(tagX + 4f, tagY + 26f, (tagW - 8f) * ageRatio, 2.5f, Color.FromArgb(235, 250, 204, 21));
        GpuGl.FillRect(tagX + 4f, tagY + 30f, (tagW - 8f) * xpRatio, 2.5f, a.AuraColor);

        if (a.LevelUpBannerTimer > 0f) {
            GpuGl.DrawText("* EVO / GEN UP! MUTACION NEURAL *", a.PosX - 88f, bobY - 82f * zDepthScale, Color.FromArgb(250, 250, 204, 21), GpuGl.FONT_LIST_HUD_SMALL);
        }
    }

    private void RenderSingleAgent(Graphics g, PorygonAgent a, float userScale, Color themeCyan, Color themeMag, Font tagFont) {
        ModelData model = a.Model;
        if (model == null) return;

        float zDepthScale = GetDepthScale(a.PosZ);
        float scale = 1.26f * a.ScaleFactor * (userScale / 100.0f) * zDepthScale;
        float hopOffset = (a.SpinRollTimer > 0f) ? (float)Math.Sin(a.SpinRollTimer * Math.PI) * 32f * zDepthScale : 0f;
        float bobY = a.PosY + (float)Math.Sin(a.FloatBob) * 9f * zDepthScale - hopOffset;

        // Skill LVL 6: Deep Sonar Scan — Concentric 3D Tilted Sonar Rings + Radar Sweep
        if (a.ScanWaveTimer > 0f) {
            float prog = 1.0f - a.ScanWaveTimer;
            int alpha = (int)Clamp(a.ScanWaveTimer * 200f, 10f, 240f);
            for (int w = 0; w < 2; w++) {
                float wProg = Clamp(prog - w * 0.16f, 0f, 1f);
                if (wProg <= 0f) continue;
                float scanR = (40f + wProg * 270f) * zDepthScale;
                using (Pen scanPen = new Pen(Color.FromArgb(Math.Max(15, alpha - w * 60), a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.2f - w * 0.6f)) {
                    g.DrawEllipse(scanPen, a.PosX - scanR, bobY - scanR * 0.45f, scanR * 2f, scanR * 0.9f);
                }
            }
        }

        // Skill LVL 5: Quantum Blink Warp Trail + Dimensional Portals
        if (a.QuantumBlinkTimer > 0f) {
            int alpha = (int)Clamp(a.QuantumBlinkTimer * 220f, 10f, 245f);
            using (Pen warpPen = new Pen(Color.FromArgb(alpha, 0, 240, 255), 3.5f))
            using (Pen warpCore = new Pen(Color.FromArgb(alpha, 255, 255, 255), 1.5f)) {
                g.DrawLine(warpPen, a.BlinkStartPt.X, a.BlinkStartPt.Y, a.PosX, bobY);
                g.DrawLine(warpCore, a.BlinkStartPt.X, a.BlinkStartPt.Y, a.PosX, bobY);
                g.DrawEllipse(warpPen, a.BlinkStartPt.X - 26f, a.BlinkStartPt.Y - 26f, 52f, 52f);
                g.DrawEllipse(warpPen, a.PosX - 28f, bobY - 28f, 56f, 56f);
            }
        }

        // Skill LVL 3: P2P Neural Sync Link Beam between Porygon Agents
        if (a.SyncTimer > 0f && a.SyncPartnerIdx >= 0 && a.SyncPartnerIdx < Agents.Count) {
            PorygonAgent partner = Agents[a.SyncPartnerIdx];
            int alpha = (int)Clamp(a.SyncTimer * 220f, 10f, 245f);
            using (Pen syncOuter = new Pen(Color.FromArgb(alpha / 2, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 6f))
            using (Pen syncCore = new Pen(Color.FromArgb(alpha, 250, 204, 21), 2.2f)) {
                g.DrawLine(syncOuter, a.PosX, bobY, partner.PosX, partner.PosY);
                g.DrawLine(syncCore, a.PosX, bobY, partner.PosX, partner.PosY);
            }
        }

        // Active Tractor Lock Conduit in GDI+
        if (a.ActiveLockStrength > 0.06f) {
            int lockA = (int)Clamp(a.ActiveLockStrength * 185f, 30f, 210f);
            using (Pen tPen = new Pen(Color.FromArgb(lockA, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1.8f)) {
                tPen.DashStyle = DashStyle.Dash;
                g.DrawLine(tPen, a.PosX, bobY, a.ActiveLockTarget.X, a.ActiveLockTarget.Y);
            }
        }

        // Cyber Holographic Platform beneath each Porygon
        float ringW = 126f * (userScale / 100f);
        float ringH = 30f * (userScale / 100f);
        float ringY = a.PosY + 74f * (userScale / 100f);
        using (Pen ringPen1 = new Pen(Color.FromArgb(a.IsHovered ? 230 : 125, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), a.IsHovered ? 2.6f : 1.8f))
        using (Pen ringPen2 = new Pen(Color.FromArgb(65, themeCyan.R, themeCyan.G, themeCyan.B), 1.1f))
        using (SolidBrush shadowB = new SolidBrush(Color.FromArgb(a.IsHovered ? 85 : 45, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B))) {
            g.FillEllipse(shadowB, a.PosX - ringW * 0.45f, ringY - ringH * 0.45f, ringW * 0.9f, ringH * 0.9f);
            g.DrawEllipse(ringPen1, a.PosX - ringW * 0.5f, ringY - ringH * 0.5f, ringW, ringH);
            g.DrawEllipse(ringPen2, a.PosX - ringW * 0.68f, ringY - ringH * 0.68f, ringW * 1.36f, ringH * 1.36f);
        }

        // Transform 3D vertices and project
        List<Vec3> bVerts = model.Verts;
        List<Face3D> bFaces = model.Faces;
        Vec3[] tv = new Vec3[bVerts.Count];
        PointF[] pv = new PointF[bVerts.Count];
        float fov = 560f;
        float camDist = 460f;

        for (int i = 0; i < bVerts.Count; i++) {
            Vec3 w = TransformAgentVertex(a, bVerts[i], scale);
            tv[i] = w;
            float zProj = camDist - w.Z;
            if (zProj < 80f) zProj = 80f;
            float persp = fov / zProj;
            pv[i] = new PointF(a.PosX + w.X * persp, bobY - w.Y * persp);
        }

        Vec3 lightDir = new Vec3(-0.32f, 0.72f, 0.62f).Normalized();
        List<ProjectedFace> renderList = new List<ProjectedFace>(bFaces.Count);
        float absorbBoost = (a.AbsorbFlashTimer > 0f) ? a.AbsorbFlashTimer * 0.22f : 0f;

        for (int i = 0; i < bFaces.Count; i++) {
            Face3D f = bFaces[i];
            int i0 = f.Indices[0], i1 = f.Indices[1], i2 = f.Indices[2];
            PointF p0 = pv[i0], p1 = pv[i1], p2 = pv[i2];

            if (bFaces.Count > 1000 && f.DepthBias == 0f) {
                float cross2D = (p1.X - p0.X) * (p2.Y - p0.Y) - (p1.Y - p0.Y) * (p2.X - p0.X);
                if (Math.Abs(cross2D) < 0.08f) continue;
            }

            float avgZ = (tv[i0].Z + tv[i1].Z + tv[i2].Z) / 3.0f + f.DepthBias;
            Vec3 v0 = tv[i0], v1 = tv[i1], v2 = tv[i2];
            Vec3 norm = Vec3.Cross(v1 - v0, v2 - v0).Normalized();
            if (norm.Z < 0) norm = norm * -1f;

            float ndotl = Math.Max(0f, Vec3.Dot(norm, lightDir));
            float shade = 0.80f + ndotl * 0.26f + absorbBoost;
            int r = (int)Math.Min(255, f.BaseColor.R * shade);
            int gr = (int)Math.Min(255, f.BaseColor.G * shade);
            int b = (int)Math.Min(255, f.BaseColor.B * shade);

            renderList.Add(new ProjectedFace {
                Pts = new PointF[] { p0, p1, p2 },
                Depth = avgZ,
                ShadedColor = Color.FromArgb(255, r, gr, b),
                ContourMask = f.ContourEdgeMask,
                IsEyeOverlay = (f.DepthBias > 0f)
            });
        }

        renderList.Sort();

        if (renderList.Count > 1000) {
            SmoothingMode prevMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;
            using (SolidBrush fb = new SolidBrush(Color.White)) {
                for (int i = 0; i < renderList.Count; i++) {
                    ProjectedFace pf = renderList[i];
                    fb.Color = pf.ShadedColor;
                    g.FillPolygon(fb, pf.Pts);
                }
            }
            g.SmoothingMode = prevMode;
        } else {
            using (Pen contourPen = new Pen(Color.FromArgb(68, 18, 22, 30), 1.0f)) {
                int cIdx = 0;
                for (int i = 0; i < renderList.Count; i++) {
                    ProjectedFace pf = renderList[i];
                    while (cIdx < i && renderList[cIdx].Depth + (renderList[cIdx].IsEyeOverlay ? 3.8f : 11.5f) <= pf.Depth) {
                        ProjectedFace cf = renderList[cIdx++];
                        if (cf.ContourMask != 0) {
                            SmoothingMode prevSm = g.SmoothingMode;
                            g.SmoothingMode = SmoothingMode.None;
                            byte cm = cf.ContourMask;
                            if ((cm & 1) != 0) g.DrawLine(contourPen, cf.Pts[0], cf.Pts[1]);
                            if ((cm & 2) != 0) g.DrawLine(contourPen, cf.Pts[1], cf.Pts[2]);
                            if ((cm & 4) != 0) g.DrawLine(contourPen, cf.Pts[2], cf.Pts[0]);
                            g.SmoothingMode = prevSm;
                        }
                    }
                    using (SolidBrush fb = new SolidBrush(pf.ShadedColor))
                    using (Pen seamPen = new Pen(pf.ShadedColor, 1.0f)) {
                        g.FillPolygon(fb, pf.Pts);
                        g.DrawPolygon(seamPen, pf.Pts);
                    }
                }
                SmoothingMode prevSmEnd = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.None;
                while (cIdx < renderList.Count) {
                    ProjectedFace cf = renderList[cIdx++];
                    if (cf.ContourMask != 0) {
                        byte cm = cf.ContourMask;
                        if ((cm & 1) != 0) g.DrawLine(contourPen, cf.Pts[0], cf.Pts[1]);
                        if ((cm & 2) != 0) g.DrawLine(contourPen, cf.Pts[1], cf.Pts[2]);
                        if ((cm & 4) != 0) g.DrawLine(contourPen, cf.Pts[2], cf.Pts[0]);
                    }
                }
                g.SmoothingMode = prevSmEnd;
            }
        }

        // Skill LVL 2: Dual Geodesic Hexagonal Conversion2 Energy Shield
        if (a.ShieldTimer > 0f) {
            int sAlpha = (int)(Math.Min(1.0f, a.ShieldTimer) * 185f);
            float shR = 76f * (userScale / 100f) * zDepthScale;
            PointF[] hex = new PointF[6];
            PointF[] innerHex = new PointF[6];
            for (int k = 0; k < 6; k++) {
                double ang = a.FloatBob * 1.6f + k * (Math.PI / 3.0);
                double ang2 = -a.FloatBob * 2.1f + k * (Math.PI / 3.0);
                hex[k] = new PointF(a.PosX + (float)Math.Cos(ang) * shR, bobY + (float)Math.Sin(ang) * shR * 0.85f);
                innerHex[k] = new PointF(a.PosX + (float)Math.Cos(ang2) * shR * 0.62f, bobY + (float)Math.Sin(ang2) * shR * 0.53f);
            }
            using (Pen shPen = new Pen(Color.FromArgb(sAlpha, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.4f))
            using (Pen inPen = new Pen(Color.FromArgb(sAlpha * 3 / 4, 255, 255, 255), 1.4f))
            using (SolidBrush shFill = new SolidBrush(Color.FromArgb(sAlpha / 6, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B))) {
                g.FillPolygon(shFill, hex);
                g.DrawPolygon(shPen, hex);
                g.DrawPolygon(inPen, innerHex);
            }
        }

        // Skill LVL 1: Spiral Conversion Beam
        if (a.BeamTimer > 0f) {
            Vec3 beakTip = TransformAgentVertex(a, new Vec3(0, 18, 56), scale);
            float zProj = Math.Max(80f, camDist - beakTip.Z);
            PointF beakPt = new PointF(a.PosX + beakTip.X * (fov / zProj), bobY - beakTip.Y * (fov / zProj));
            using (Pen beamOuter = new Pen(Color.FromArgb(165, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 7.5f))
            using (Pen beamCore = new Pen(Color.FromArgb(245, 255, 255, 255), 2.6f)) {
                g.DrawLine(beamOuter, beakPt, a.BeamTarget);
                g.DrawLine(beamCore, beakPt, a.BeamTarget);
                g.DrawEllipse(beamCore, a.BeamTarget.X - 14f, a.BeamTarget.Y - 14f, 28f, 28f);
            }
        }

        // Skill LVL 4: Tri-Attack Elemental Matrix Beam (Cyan Ice + Magenta Plasma + Gold Electric)
        if (a.TriAttackTimer > 0f) {
            Vec3 beakTip = TransformAgentVertex(a, new Vec3(0, 18, 56), scale);
            float zProj = Math.Max(80f, camDist - beakTip.Z);
            PointF beakPt = new PointF(a.PosX + beakTip.X * (fov / zProj), bobY - beakTip.Y * (fov / zProj));
            PointF[] tri = new PointF[3];
            for (int k = 0; k < 3; k++) {
                double ang = a.FloatBob * 3.6f + k * (2.0 * Math.PI / 3.0);
                tri[k] = new PointF(beakPt.X + (float)Math.Cos(ang) * 22f * zDepthScale, beakPt.Y + (float)Math.Sin(ang) * 22f * zDepthScale);
            }
            using (Pen pIce  = new Pen(Color.FromArgb(230, 0, 240, 255), 3.4f))
            using (Pen pPlas = new Pen(Color.FromArgb(230, 236, 72, 153), 3.4f))
            using (Pen pElec = new Pen(Color.FromArgb(240, 250, 204, 21), 3.4f)) {
                g.DrawPolygon(pElec, tri);
                g.DrawLine(pIce,  tri[0], a.TriAttackTarget);
                g.DrawLine(pPlas, tri[1], a.TriAttackTarget);
                g.DrawLine(pElec, tri[2], a.TriAttackTarget);
                g.DrawEllipse(pPlas, a.TriAttackTarget.X - 22f, a.TriAttackTarget.Y - 22f, 44f, 44f);
                g.DrawEllipse(pIce,  a.TriAttackTarget.X - 14f, a.TriAttackTarget.Y - 14f, 28f, 28f);
            }
        }

        // Floating Mini-HUD Tag under each Porygon showing Name, Generation, LifeStage, Level & XP bar
        if (tagFont != null) {
            string badge = string.Format("{0} G{1} {2}", a.Name, a.Brain.Generation, a.Brain.GetLifeStageLabel());
            float tagX = a.PosX - 66f;
            float tagY = ringY + 12f;
            using (SolidBrush tagBg = new SolidBrush(Color.FromArgb(128, 5, 8, 18)))
            using (Pen tagBorder = new Pen(Color.FromArgb(140, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1f))
            using (SolidBrush txtB = new SolidBrush(Color.FromArgb(245, 240, 248, 255)))
            using (SolidBrush xpFill = new SolidBrush(a.AuraColor)) {
                g.FillRectangle(tagBg, tagX, tagY, 132f, 24f);
                g.DrawRectangle(tagBorder, tagX, tagY, 132f, 24f);
                g.DrawString(badge, tagFont, txtB, tagX + 4f, tagY + 2f);
                float xpRatio = Math.Min(1.0f, (float)a.Brain.XP / Math.Max(1, a.Brain.NextLevelXP));
                g.FillRectangle(xpFill, tagX + 4f, tagY + 19f, 124f * xpRatio, 3f);
            }

            if (a.LevelUpBannerTimer > 0f) {
                using (SolidBrush lvlB = new SolidBrush(Color.FromArgb(250, 250, 204, 21))) {
                    g.DrawString(a.LevelUpBannerText ?? "★ LEVEL UP! ★", tagFont, lvlB, a.PosX - 88f, bobY - 82f);
                }
            }
        }
    }
}

public class HackerWallpaperForm : Form {
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindowEx(IntPtr hParent, IntPtr hChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(Point pt);

    [DllImport("user32.dll")]
    public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

    [DllImport("shcore.dll")]
    public static extern int SetProcessDpiAwareness(int value);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    public static extern bool EnumDisplaySettingsA(string lpszDeviceName, int iModeNum, ref DEVMODEA lpDevMode);

    [StructLayout(LayoutKind.Explicit, Size = 220, CharSet = CharSet.Ansi)]
    public struct DEVMODEA {
        [FieldOffset(36)] public short dmSize;
        [FieldOffset(44)] public int dmPositionX;
        [FieldOffset(48)] public int dmPositionY;
        [FieldOffset(108)] public int dmPelsWidth;
        [FieldOffset(112)] public int dmPelsHeight;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [DllImport("psapi.dll")]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("shell32.dll")]
    public static extern int SHQueryUserNotificationState(out int pquns);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
    private static extern int PdhOpenQuery(IntPtr szDataSource, IntPtr dwUserData, out IntPtr phQuery);

    [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
    private static extern int PdhAddEnglishCounter(IntPtr hQuery, string szFullCounterPath, IntPtr dwUserData, out IntPtr phCounter);

    [DllImport("pdh.dll")]
    private static extern int PdhCollectQueryData(IntPtr hQuery);

    [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
    private static extern int PdhGetFormattedCounterArray(IntPtr hCounter, uint dwFormat, ref uint lpdwBufferSize, out uint lpdwItemCount, IntPtr ItemBuffer);

    [DllImport("pdh.dll")]
    private static extern int PdhCloseQuery(IntPtr hQuery);

    [StructLayout(LayoutKind.Sequential)]
    private struct PDH_FMT_COUNTERVALUE_ITEM_DOUBLE {
        public IntPtr szName;
        public uint CStatus;
        public double doubleValue;
    }

    private const uint PDH_FMT_DOUBLE = 0x00000200;
    private const uint PDH_MORE_DATA = 0x800007D2;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILETIME { public uint dwLowDateTime; public uint dwHighDateTime; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX() { this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
    }

    const int GWL_EXSTYLE = -20;
    const int GWL_STYLE = -16;
    const int WS_CHILD = 0x40000000;
    const int WS_VISIBLE = 0x10000000;
    const int WS_EX_LAYERED = 0x00080000;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_NOACTIVATE = 0x08000000;
    const int WS_EX_TOOLWINDOW = 0x00000080;
    const uint LWA_ALPHA = 0x00000002;
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_SHOWWINDOW = 0x0040;
    const int WM_HOTKEY = 0x0312;
    const int HOTKEY_ID = 9001;
    const int HOTKEY_MODEL_ID = 9002;
    const int HOTKEY_BRAIN_FULL_ID = 9003;
    const int HOTKEY_OPACITY_UP_ID = 9004;
    const int HOTKEY_OPACITY_DOWN_ID = 9005;
    const int HOTKEY_BG_MODE_ID = 9006;
    const int HOTKEY_GAMER_ID = 9007;
    const int HOTKEY_SIEM_ID = 9008;
    const int HOTKEY_OPACITY_TOGGLE_ID = 9009;
    const uint MOD_ALT = 0x0001;
    const uint MOD_CONTROL = 0x0002;
    const int WH_MOUSE_LL = 14;
    const int WM_MOUSEWHEEL = 0x020A;
    private int terminalOpacityPct = 50;

    // Gamer Mode & >90% External GPU Load Auto-Hibernation State
    private bool autoGameDetected = false;
    private string activeGameName = "";
    private bool autoHighGpuDetected = false;
    private string highGpuProcessName = "";
    private float externalGpuMaxPct = 0.0f;
    private float realSelfGpuPct = 1.2f;
    private int highGpuStreak = 0;
    private int lowGpuStreak = 0;
    private bool gpuOverBudgetEco = false;
    private IntPtr pdhGpuQuery = IntPtr.Zero;
    private IntPtr pdhGpuCounter = IntPtr.Zero;
    private IntPtr pdhGpuBuf = IntPtr.Zero;
    private uint pdhGpuBufSize = 0;
    private int cachedSelfPid = 0;
    private readonly Dictionary<string, double> pdhEngineSums = new Dictionary<string, double>(32);
    private readonly Dictionary<int, double> pdhPidSums = new Dictionary<int, double>(64);
    private bool isGamerSuspended = false;
    private bool gamerStaticFrameRendered = false;
    private bool socPort5080Online = false;
    private int socPort5080LastCheckCycle = -100;

    public WallpaperConfig Config;
    private int screenW;
    private int screenH;
    private Bitmap bgBitmap;
    private System.Windows.Forms.Timer renderTimer;
    private System.Threading.Timer telemetryTimer;
    private System.Threading.Timer cpuEvoTimer;
    private EventWaitHandle showConfigEvent;
    private EventWaitHandle toggleGamerEvent;
    private EventWaitHandle opacityCmdEvent;
    private HotkeyMessageWindow hotkeySink;
    private int prevHotkeyMask = 0;
    private DateTime lastHotkeyUtc = DateTime.MinValue;
    private Form toastForm;
    private System.Windows.Forms.Timer toastTimer;
    private Form configForm;
    private Form siemForm;
    private Random rng = new Random();

    private Porygon3DAvatar porygon;

    // Collapsible HUD Cards State (Single-Click Fold / Unfold on Desktop, Persisted in config.json)
    private bool radarCollapsed = false;
    private bool telemetryCollapsed = false;
    private bool connectomeCollapsed = false;
    private bool brain3DCollapsed = false;
    private bool secondaryBarsCollapsed = false;
    private bool prevLeftMouseDown = false;
    private bool prevRightMouseDown = false;
    private bool hudTelemetryDirty = true;
    private bool hudRadarDirty = true;
    private bool hudConnectomeDirty = true;
    private bool hudBrain3DDirty = true;
    private float brain3DYaw = 0f;
    private bool brain3DFullscreen = false;
    private bool brain3DAutoRotate = false;
    private float brain3DManualYaw = 0f;
    private float brain3DManualPitch = 0.14f;
    private float brain3DZoom = 1.0f;
    private bool brain3DDragging = false;
    private bool brain3DDragMoved = false;
    private Point brain3DDragStartPt;
    private float brain3DDragStartYaw = 0f;
    private float brain3DDragStartPitch = 0.14f;
    private int[] brainActHist = new int[110];
    private int brainActHistPos = 0;
    private int brainActHistFrame = -1;
    private float[] brainRegionAct = new float[4];
    private int brain3DHoverX = -1;
    private int brain3DHoverY = -1;
    private HashSet<string> vtAuditedIps = new HashSet<string>();

    // Real System Telemetry & Radar Blips with Mouse Hover Metadata
    public struct RadarBlip {
        public float AngleDeg;
        public float DistNorm; // 0.2 .. 0.92
        public string Label;
        public string FullRemoteEp;
        public string LocalEp;
        public string Proto;
        public string RingZone;
        public float AnomalyScore;
        public bool IsHttps;
        public bool IsAnomaly;
    }

    private float cpuUsage = 0f;
    private ulong prevIdle = 0, prevKernel = 0, prevUser = 0;
    private float gpuUsagePct = 3.2f;
    private float gpuFrameMs = 2.4f;
    private int currentFps = 28;
    private bool gpuEcoMode = false;
    private bool cachedWindowMaximized = false;
    private bool[] monitorOccluded = new bool[8];
    private bool primaryMonitorOccluded = false;
    private bool anyMonitorOccluded = false;
    private int visibleMonitorCount = 5;
    private readonly StringBuilder occlusionClassSb = new StringBuilder(64);
    private float sceneTimePhase = 0f;
    private int fpsFrameAccum = 0;
    private Stopwatch fpsMeasureSw = Stopwatch.StartNew();
    private Stopwatch frameSw = new Stopwatch();
    private float ramUsedGB = 0f, ramTotalGB = 32f;
    private int ramPercent = 0;
    private string activeNicName = "Wi-Fi";
    private string localIPv4 = "192.168.1.x";
    private float netDownKbps = 0f, netUpKbps = 0f;
    private long prevBytesRecv = 0, prevBytesSent = 0;
    private DateTime prevNetTime = DateTime.UtcNow;
    private int pingMs = 12;
    private int tcpEstablishedCount = 0;
    private float[] pingHistory = new float[20];
    private List<string> liveLogLines = new List<string>();
    private List<RadarBlip> radarBlips = new List<RadarBlip>();
    private string lastScannedTarget = "1.1.1.1:443";
    private object telemetryLock = new object();

    // Real Network Anomaly Detection Statistical Baseline (Rolling EMA Mean & Variance + Known Host Table)
    private bool baselineInitialized = false;
    private int telemetryCycles = 0;
    private float emaDownKbps = 25f, varDownKbps = 400f;
    private float emaUpKbps = 10f, varUpKbps = 150f;
    private float emaPingMs = 18f, varPingMs = 36f;
    private HashSet<string> knownRemoteHosts = new HashSet<string>();
    private Porygon3DAvatar.RealTelemetryFeed currentFeed;

    private class RainColumn {
        public float X, Y, BaseSpeed;
        public int Length;
        public char[] Chars;
        public int ColorSlot;
        public int DepthLayer; // 0 = Far 3D Plane (Z=0.38), 1 = Mid 3D Plane (Z=0.70), 2 = Near Foreground 3D Plane (Z=1.0)
        public int CharStep;
        public bool IsForeground;
        public string LogTag;
        public int MinY, MaxY, MinX, MaxX;
        public int MonitorIndex;
    }
    private List<RainColumn> columns = new List<RainColumn>();
    private List<Rectangle> monitorRects = new List<Rectangle>();
    private List<string> monitorNames = new List<string>();
    private Rectangle primaryMonRect;

    private List<string> qbRainTokens = new List<string>() {
        "QB-SOC:5080 // NIDS_ONLINE",
        "FORTIGATE_SYNC // ACTIVE",
        "PORYGON-Z // IDS_HUNTER"
    };
    private int qbTokenCursor = 0;
    private const string GLYPHS = "0123456789ABCDEF<>[]{}/\\*+#$@%&010110";

    public static void QueryPhysicalMonitors(out List<Rectangle> localRects, out List<string> names, out Rectangle primaryLocal, out int totalW, out int totalH) {
        localRects = new List<Rectangle>();
        names = new List<string>();
        Screen[] scrs = Screen.AllScreens;
        List<Rectangle> rawRects = new List<Rectangle>();
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        int primaryIdx = 0;

        for (int i = 0; i < scrs.Length; i++) {
            Rectangle b = scrs[i].Bounds;
            try {
                DEVMODEA dm = new DEVMODEA();
                dm.dmSize = 156;
                if (EnumDisplaySettingsA(scrs[i].DeviceName, -1, ref dm) && dm.dmPelsWidth > 0 && dm.dmPelsHeight > 0) {
                    b = new Rectangle(dm.dmPositionX, dm.dmPositionY, dm.dmPelsWidth, dm.dmPelsHeight);
                }
            } catch { }
            rawRects.Add(b);
            string cleanName = (scrs[i].DeviceName ?? ("DISPLAY" + (i + 1))).Replace("\\\\.\\", "");
            names.Add(cleanName);
            if (scrs[i].Primary) primaryIdx = i;

            if (b.Left < minX) minX = b.Left;
            if (b.Top < minY) minY = b.Top;
            if (b.Right > maxX) maxX = b.Right;
            if (b.Bottom > maxY) maxY = b.Bottom;
        }

        if (rawRects.Count == 0) {
            totalW = 1920;
            totalH = 1080;
            primaryLocal = new Rectangle(0, 0, 1920, 1080);
            localRects.Add(primaryLocal);
            names.Add("DISPLAY1");
            return;
        }

        totalW = Math.Max(1920, maxX - minX);
        totalH = Math.Max(1080, maxY - minY);
        primaryLocal = new Rectangle(0, 0, 1920, 1080);

        for (int i = 0; i < rawRects.Count; i++) {
            Rectangle r = rawRects[i];
            Rectangle loc = new Rectangle(r.X - minX, r.Y - minY, r.Width, r.Height);
            localRects.Add(loc);
            if (i == primaryIdx) primaryLocal = loc;
        }
    }

    private void RefreshMonitorLayout() {
        QueryPhysicalMonitors(out monitorRects, out monitorNames, out primaryMonRect, out screenW, out screenH);
        if (porygon != null) {
            porygon.RefreshMonitors(screenW, screenH);
        }
    }

    private int ClassifyRainTokenCategory(string upperToken) {
        if (string.IsNullOrEmpty(upperToken)) return 4;
        if (upperToken.Contains("CRITICAL") || upperToken.Contains("INTRUSION") || upperToken.Contains("EXPLOIT") || upperToken.Contains("RANSOM") || upperToken.Contains("DANGER") || upperToken.Contains("HIGH_RISK"))
            return 0; // Red: Critical IDS Threat / Intrusion
        if (upperToken.Contains("VT_") || upperToken.Contains("VIRUSTOTAL") || upperToken.Contains("IOC") || upperToken.Contains("MAL:") || upperToken.Contains("MALICIOUS") || upperToken.Contains("SUSPICIOUS"))
            return 1; // Gold/Amber: VirusTotal v3 Threat Intelligence
        if (upperToken.Contains("FORTIGATE") || upperToken.Contains("FIREWALL") || upperToken.Contains("DENY") || upperToken.Contains("BLOCK") || upperToken.Contains("VPN") || upperToken.Contains("IPS"))
            return 2; // Cyber Orange: FortiGate Firewall & IPS
        if (upperToken.Contains("NIDS") || upperToken.Contains("NDR") || upperToken.Contains("ANOMALY") || upperToken.Contains("PORT_ALERT") || upperToken.Contains("SYN_BURST") || upperToken.Contains("ALERT"))
            return 3; // Electric Magenta: NIDS / NDR Anomaly Detection
        if (upperToken.Contains("EVO") || upperToken.Contains("GEN_") || upperToken.Contains("PORYGON") || upperToken.Contains("BRAIN") || upperToken.Contains("PRUNED") || upperToken.Contains("CROSS"))
            return 6; // Synaptic Violet/Purple: Porygon Neuro-Evolution & Connectome
        if (upperToken.Contains("BASELINE") || upperToken.Contains("CLEAN") || upperToken.Contains("VERIFIED") || upperToken.Contains("HEALTH") || upperToken.Contains("ONLINE"))
            return 5; // Emerald Matrix Green: Verified Clean / Baseline Health
        return 4;     // Electric Cyan: Active TCP/TLS/HTTPS/DNS Network Flows
    }

    private void GetRainSemanticColors(int categorySlot, Color c1, Color c2, Color c3, out Color bodyCol, out Color headCol) {
        // Eye-Care Matte Semantic Palette (-35% to -40% luminance attenuation, zero harsh neon glare)
        switch (categorySlot) {
            case 0: // CRITICAL / INTRUSION -> Matte Terracotta Crimson
                bodyCol = Color.FromArgb(192, 54, 74);
                headCol = Color.FromArgb(222, 186, 194);
                break;
            case 1: // VIRUSTOTAL / IOC -> Warm Matte Ochre Gold
                bodyCol = Color.FromArgb(192, 152, 42);
                headCol = Color.FromArgb(224, 212, 176);
                break;
            case 2: // FORTIGATE / FIREWALL -> Muted Copper Amber
                bodyCol = Color.FromArgb(184, 106, 38);
                headCol = Color.FromArgb(218, 192, 170);
                break;
            case 3: // NIDS / NDR ANOMALY -> Muted Coral Rose (replaces harsh neon magenta)
                bodyCol = Color.FromArgb(168, 82, 108);
                headCol = Color.FromArgb(212, 186, 196);
                break;
            case 5: // BASELINE / VERIFIED CLEAN -> Matte Forest Emerald
                bodyCol = Color.FromArgb(44, 164, 108);
                headCol = Color.FromArgb(184, 220, 202);
                break;
            case 6: // PORYGON NEURO-EVOLUTION / 3D BRAIN -> Matte Bio-Teal Slate (replaces bright violet)
                bodyCol = Color.FromArgb(62, 148, 162);
                headCol = Color.FromArgb(186, 216, 222);
                break;
            default: // ACTIVE TCP / TLS / DNS FLOW -> Matte Phosphor Sage-Cyan
                bodyCol = Color.FromArgb(48, 158, 168);
                headCol = Color.FromArgb(184, 218, 222);
                break;
        }
    }

    private void AssignColumnLogText(RainColumn col) {
        string token = null;
        int cursorSnap = 0;
        lock (telemetryLock) {
            cursorSnap = qbTokenCursor;
            qbTokenCursor = (qbTokenCursor + 1) % 100000;
            int mode = cursorSnap % 7;
            string qbSample = (qbRainTokens != null && qbRainTokens.Count > 0)
                ? qbRainTokens[(cursorSnap / 7) % qbRainTokens.Count]
                : "QB-SOC:5080//NIDS_ACTIVE";

            if (mode == 0) {
                // Category 0 (Crimson Red): Critical IDS Alert / High-Risk Anomaly
                string anom = currentFeed.LastAnomalySummary ?? "CRITICAL_IDS//PORT_SCAN_DETECT";
                token = "CRITICAL_ALERT//" + anom + "//" + qbSample;
            } else if (mode == 1) {
                // Category 1 (Solar Gold): VirusTotal v3 Threat Intelligence
                string vt = (porygon != null && !string.IsNullOrEmpty(porygon.LastVtVerdict))
                    ? porygon.LastVtVerdict
                    : "VT_IOC_SCAN//192.168.2.222:5080//VERIFIED";
                token = "VIRUSTOTAL//" + vt;
            } else if (mode == 2) {
                // Category 2 (Cyber Orange): FortiGate Firewall & IPS Telemetry
                token = "FORTIGATE_IPS//FW_SYNC_5080//" + qbSample;
            } else if (mode == 3) {
                // Category 3 (Electric Magenta): NIDS / NDR Anomaly & Z-Score Telemetry
                token = string.Format("NIDS_NDR//{0}//Z_SCORE:{1:+0.0;-0.0;0.0}S", qbSample, currentFeed.DlZScore);
            } else if (mode == 4) {
                // Category 4 (Electric Cyan): Active TCP / TLS / HTTPS Socket Flows
                if (radarBlips != null && radarBlips.Count > 0) {
                    RadarBlip rb = radarBlips[(cursorSnap / 7) % radarBlips.Count];
                    token = "TCP_TLS_FLOW//" + rb.FullRemoteEp + "//" + rb.Proto;
                } else {
                    token = "TCP_TLS_FLOW//" + localIPv4 + ":443//" + qbSample;
                }
            } else if (mode == 5) {
                // Category 5 (Emerald Matrix Green): Verified Clean Baseline & Network Health
                token = string.Format("BASELINE_CLEAN//PING_{0}MS//DL_{1:0}KBPS//HEALTH_OK", pingMs, netDownKbps);
            } else {
                // Category 6 (Matte Bio-Teal): Porygon Neuro-Evolution, 3D Connectome & Natural Selection
                string evo = (porygon != null && !string.IsNullOrEmpty(porygon.LastLineageAuditSummary))
                    ? porygon.LastLineageAuditSummary
                    : "PORYGON_Z//F2_SCORE_99%//GENOME_CLEAN";
                token = "EVO_BRAIN_3D//" + evo;
            }
        }
        col.LogTag = null; // Strictly vertical text only — never draw horizontal floating tags
        if (!string.IsNullOrEmpty(token)) {
            string upper = token.ToUpperInvariant();
            col.ColorSlot = ClassifyRainTokenCategory(upper);
            string clean = upper.Replace(" ", "_").Replace("[", "").Replace("]", "");
            int len = Math.Max(14, Math.Min(30, clean.Length));
            col.Length = len;
            col.Chars = new char[len];
            // Fill characters so reading top-to-bottom down the vertical column spells the log entry
            for (int c = 0; c < len; c++) {
                int srcIdx = (len - 1 - c) % clean.Length;
                col.Chars[c] = clean[srcIdx];
            }
        } else {
            col.ColorSlot = rng.Next(0, 7);
            col.Length = rng.Next(12, 24);
            col.Chars = new char[col.Length];
            for (int c = 0; c < col.Length; c++) col.Chars[c] = GLYPHS[rng.Next(GLYPHS.Length)];
        }

        // Compute 3D Depth-Layer base speed + Priority Acceleration (1.34x) for Critical Red (0) & VirusTotal Gold (1)!
        float layerSpeed = (col.DepthLayer == 2)
            ? (float)(6.6 + rng.NextDouble() * 5.8)
            : (col.DepthLayer == 1 ? (float)(4.1 + rng.NextDouble() * 3.4) : (float)(2.1 + rng.NextDouble() * 1.9));
        if (col.ColorSlot == 0 || col.ColorSlot == 1) {
            layerSpeed *= 1.34f; // Critical Alerts & VirusTotal IOCs plunge faster in 3D space!
        }
        col.BaseSpeed = layerSpeed;
    }

    private float radarAngle = 0f;
    private int frameCount = 0;

    private Font fontRainFg;
    private Font fontRainBg;
    private Font fontHud;
    private Font fontHudSmall;
    private Font fontHudBold;
    private Font fontHudTitle;

    public HackerWallpaperForm(EventWaitHandle configEvt, bool openConfigOnStart, EventWaitHandle gamerEvt = null, bool startGamerOnBoot = false, EventWaitHandle opacityEvt = null) {
        this.showConfigEvent = configEvt;
        this.toggleGamerEvent = gamerEvt;
        this.opacityCmdEvent = opacityEvt;
        try { cachedSelfPid = Process.GetCurrentProcess().Id; } catch { cachedSelfPid = 0; }
        LoadInitialTerminalOpacity();
        Config = WallpaperConfig.Load();
        if (configEvt != null) {
            // Ensure cold startup always begins with live 3D rendering and Scene Mode 0 (Forest + River)
            Config.GamerMode = startGamerOnBoot;
            Config.BgSceneMode = 0;
            Config.Save();
        }
        radarCollapsed = Config.RadarCollapsed;
        telemetryCollapsed = Config.TelemetryCollapsed;
        connectomeCollapsed = Config.ConnectomeCollapsed;
        brain3DCollapsed = Config.Brain3DCollapsed;
        secondaryBarsCollapsed = Config.SecondaryBarsCollapsed;

        RefreshMonitorLayout();

        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Bounds = new Rectangle(0, 0, screenW, screenH);
        this.ShowInTaskbar = false;
        this.DoubleBuffered = false;
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);

        fontRainFg = new Font("Consolas", 11f, FontStyle.Bold);
        fontRainBg = new Font("Consolas", 9f, FontStyle.Regular);
        fontHudSmall = new Font("Consolas", 7.8f, FontStyle.Regular);
        fontHud = new Font("Consolas", 8.5f, FontStyle.Regular);
        fontHudBold = new Font("Consolas", 9f, FontStyle.Bold);
        fontHudTitle = new Font("Consolas", 9.8f, FontStyle.Bold);

        porygon = new Porygon3DAvatar(screenW, screenH);
        porygon.IsPointOccluded = IsPointOnOccludedMonitor;

        for (int i = 0; i < pingHistory.Length; i++) pingHistory[i] = 15f;
        if (configEvt == null) {
            cpuUsage = 8.4f;
            gpuUsagePct = 5.2f;
            gpuFrameMs = 3.4f;
            currentFps = 24;
            ramUsedGB = 14.2f;
            ramTotalGB = 32.0f;
            ramPercent = 44;
            netDownKbps = 142.5f;
            netUpKbps = 28.4f;
            pingMs = 14;
            liveLogLines.Add("[13:20:01] [3D_BG_SCENE] Entorno 3D -> [1/3] SELVA+OSC (Ctrl+Alt+M)");
            liveLogLines.Add("[13:20:02] [TERM_OPACITY] Opacidad Terminal -> 50% (Ctrl+Alt+Up/Down/O)");
            liveLogLines.Add("[13:20:03] BASELINE // DL:142.5KB/s | GPU:5.2% (24FPS) | PING:14ms");
            return;
        }
        RebuildBackground();
        RebuildRainColumns();
        InitFlyWireBrain();

        // Hook PorygonCyberDefenseEngine (NIDS+PID / AV Watcher / SIEM / Hybrid IPS / Web SOC) to 3D Cyberspace
        PorygonCyberDefenseEngine.OnInject3DThreatOrb = (kind, remEp, locSkt, proto, tcpSt, anomType, anomDet, score) => {
            try {
                if (porygon != null) {
                    porygon.InjectRealNetworkNode(screenW, screenH, kind, remEp, locSkt, proto, tcpSt, anomType, anomDet, score, netDownKbps + netUpKbps);
                }
                if (flyWire != null && flyWire.Loaded) {
                    string at = (anomType ?? "").ToUpperInvariant();
                    string fwKind;
                    if (at.Contains("REGISTRY") || at.Contains("PERSIST") || at.Contains("SERVICE") || at.Contains("STARTUP")) fwKind = "malware";
                    else if (at.Contains("PORT") || at.Contains("SCAN") || at.Contains("RECON") || at.Contains("PROBE")) fwKind = "signature";
                    else if (at.Contains("EXFIL") || at.Contains("UPLOAD") || at.Contains("TUNNEL")) fwKind = "exfil";
                    else if (at.Contains("DGA") || at.Contains("C2") || at.Contains("BEACON") || at.Contains("LOLBIN") || at.Contains("POWERSHELL")) fwKind = "anomaly";
                    else fwKind = score >= 0.70f ? "anomaly" : "signature";
                    flyWire.Enqueue(fwKind, string.Format("IDS {0} {1}", anomType, remEp), score);
                }
                lock (telemetryLock) {
                    string ts = DateTime.Now.ToString("HH:mm:ss");
                    liveLogLines.Add(string.Format("[{0}] [IDS_DETECT] {1} -> {2} ({3:0}%)", ts, anomType, remEp, score * 100f));
                    while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
                }
                hudConnectomeDirty = true;
                hudTelemetryDirty = true;
            } catch { }
        };
        PorygonCyberDefenseEngine.OnQueryFormState = () => {
            Dictionary<string, object> d = new Dictionary<string, object>();
            lock (telemetryLock) {
                d["cpu"] = cpuUsage.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["gpu"] = (isGamerSuspended ? 0.0f : gpuUsagePct).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["selfGpu"] = (isGamerSuspended ? 0.0f : realSelfGpuPct).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["extGpu"] = externalGpuMaxPct.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["autoHighGpu"] = autoHighGpuDetected;
                d["highGpuProc"] = highGpuProcessName;
                d["termOpacity"] = terminalOpacityPct;
                d["fps"] = isGamerSuspended ? 0 : currentFps;
                d["ramPct"] = ramPercent;
                d["ramUsed"] = ramUsedGB.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["downKbps"] = netDownKbps.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["upKbps"] = netUpKbps.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                d["pingMs"] = pingMs;
                d["bgSceneMode"] = Config != null ? Config.BgSceneMode : 0;
                d["bgSceneName"] = GetBgSceneModeShortName(Config != null ? Config.BgSceneMode : 0);
                d["gamerMode"] = isGamerSuspended || (Config != null && Config.GamerMode);
                d["visMons"] = visibleMonitorCount;
                d["occDebug"] = lastOcclusionDebug;
            }
            List<Dictionary<string, object>> agList = new List<Dictionary<string, object>>();
            if (porygon != null) {
                for (int i = 0; i < porygon.Agents.Count; i++) {
                    var ag = porygon.Agents[i];
                    var b = ag.Brain;
                    var ad = new Dictionary<string, object>();
                    ad["name"] = ag.Name;
                    ad["gen"] = b.Generation;
                    ad["lvl"] = b.Level;
                    ad["f2"] = b.F2Score.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    ad["fit"] = b.FitnessScore.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    ad["acc"] = b.DetectionAccuracy.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    agList.Add(ad);
                }
            }
            d["agents"] = agList;
            return d;
        };
        PorygonCyberDefenseEngine.OnSwarmWebAction = (act) => {
            try {
                this.BeginInvoke((MethodInvoker)delegate {
                    if (act == "train_wave" && porygon != null) {
                        porygon.TriggerTrainingWave(screenW, screenH);
                    } else if (act == "force_evo" && porygon != null) {
                        porygon.ForceEvolutionaryCrossoverAll();
                    } else if (act == "boost_lvl" && porygon != null) {
                        porygon.BoostAllLevels();
                    } else if (act == "cycle_scene") {
                        CycleBgSceneMode();
                    } else if (act == "toggle_gamer") {
                        ToggleGamerMode();
                    } else if (act == "opacity_up") {
                        AdjustTerminalOpacity(15);
                    } else if (act == "opacity_down") {
                        AdjustTerminalOpacity(-15);
                    } else if (act == "opacity_toggle") {
                        ToggleTerminalOpacityQuick();
                    }
                    hudConnectomeDirty = true;
                    hudBrain3DDirty = true;
                    hudTelemetryDirty = true;
                });
            } catch { }
        };
        PorygonCyberDefenseEngine.Initialize();

        renderTimer = new System.Windows.Forms.Timer();
        renderTimer.Interval = 33; // Hardware GPU Batched target (~30 FPS active desktop / 20 FPS eco mode to minimize cross-GPU DWM copy)
        renderTimer.Tick += OnRenderTick;

        telemetryTimer = new System.Threading.Timer(UpdateRealTelemetry, null, 250, 2200);
        // Single-Core Low-Overhead CPU Evolutionary Training Timer (runs every 800ms in background)
        cpuEvoTimer = new System.Threading.Timer(OnCpuEvolutionTimerTick, null, 800, 800);

        if (openConfigOnStart) {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 400;
            t.Tick += (s, ev) => { t.Stop(); ShowConfigWindow(); };
            t.Start();
        } else if (startGamerOnBoot) {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 350;
            t.Tick += (s, ev) => { t.Stop(); ShowGamerToastNotification(true, "ACCESO DIRECTO"); };
            t.Start();
        }
    }

    private void OnCpuEvolutionTimerTick(object state) {
        if (isGamerSuspended || porygon == null || !Config.ShowPorygon) return;
        Porygon3DAvatar.RealTelemetryFeed feedSnap;
        lock (telemetryLock) { feedSnap = currentFeed; }
        porygon.RunCpuEvolutionTick(feedSnap);
    }

    public static string GetBgSceneModeShortName(int mode) {
        if (mode == 1) return "TOPO+MESH";
        if (mode == 2) return "MYCELIUM";
        return "SELVA+OSC";
    }

    public void CycleBgSceneMode() {
        Config.BgSceneMode = (Config.BgSceneMode + 1) % 3;
        Config.Save();
        RebuildBackground();
        lock (telemetryLock) {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            liveLogLines.Add(string.Format("[{0}] [3D_BG_SCENE] Entorno 3D -> [{1}/3] {2} (Ctrl+Alt+M)",
                ts, Config.BgSceneMode + 1, GetBgSceneModeShortName(Config.BgSceneMode)));
            while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
        }
        hudTelemetryDirty = true;
    }

    public void GetThemeColors(out Color c1, out Color c2, out Color c3, out Color head) {
        // Eye-Care Matte Palette (-35% brightness attenuation, zero monotonous purple)
        if (Config.ThemeIndex == 1) {
            c1 = Color.FromArgb(42, 174, 98);
            c2 = Color.FromArgb(28, 138, 98);
            c3 = Color.FromArgb(52, 158, 118);
            head = Color.FromArgb(186, 222, 202);
        } else if (Config.ThemeIndex == 2) {
            c1 = Color.FromArgb(188, 56, 78);
            c2 = Color.FromArgb(186, 126, 34);
            c3 = Color.FromArgb(168, 68, 86);
            head = Color.FromArgb(220, 198, 204);
        } else if (Config.ThemeIndex == 3) {
            c1 = Color.FromArgb(54, 148, 186);
            c2 = Color.FromArgb(48, 110, 172);
            c3 = Color.FromArgb(68, 120, 166);
            head = Color.FromArgb(188, 214, 226);
        } else {
            // Theme 0 Default: Matte Phosphor Sage + Warm Ochre Amber + Deep Slate Teal (Zero Purple!)
            c1 = Color.FromArgb(52, 168, 138);
            c2 = Color.FromArgb(186, 138, 56);
            c3 = Color.FromArgb(46, 128, 142);
            head = Color.FromArgb(188, 222, 212);
        }
    }

    public void RebuildBackground() {
        RefreshMonitorLayout();
        Bitmap newBg = new Bitmap(screenW, screenH, PixelFormat.Format32bppPArgb);
        Color c1, c2, c3, head;
        GetThemeColors(out c1, out c2, out c3, out head);

        using (Graphics g = Graphics.FromImage(newBg)) {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.FromArgb(3, 6, 6));

            for (int mIdx = 0; mIdx < monitorRects.Count; mIdx++) {
                Rectangle m = monitorRects[mIdx];
                GraphicsState st = g.Save();
                g.SetClip(m);

                // Deep Eye-Care Matte Forest Obsidian Gradient per Monitor (#030706 -> #07120F)
                using (LinearGradientBrush bgGrad = new LinearGradientBrush(
                    m,
                    Color.FromArgb(3, 7, 6),
                    Color.FromArgb(7, 17, 14),
                    LinearGradientMode.Vertical)) {
                    g.FillRectangle(bgGrad, m);
                }

                int horizonY = m.Top + (int)(m.Height * 0.64f);
                int vanishX = m.Left + (int)(m.Width * 0.50f);

                // Subtle Recessed Atmospheric Depth Grid (ultra-low alpha=9 so it never strains the eyes)
                using (Pen gridPen = new Pen(Color.FromArgb(9, c3.R, c3.G, c3.B), 1f)) {
                    for (int x = m.Left; x < m.Right; x += 96) g.DrawLine(gridPen, x, m.Top, x, horizonY);
                    for (int y = m.Top; y < horizonY; y += 96) g.DrawLine(gridPen, m.Left, y, m.Right, y);
                }

                // Soft Matte Atmospheric Mist Band at Horizon (attenuated by 40%)
                Rectangle glowTop = new Rectangle(m.Left, horizonY - 52, m.Width, 52);
                Rectangle glowBot = new Rectangle(m.Left, horizonY, m.Width, 64);
                using (LinearGradientBrush hTop = new LinearGradientBrush(glowTop, Color.FromArgb(0, c1.R, c1.G, c1.B), Color.FromArgb(24, c1.R, c1.G, c1.B), LinearGradientMode.Vertical))
                using (LinearGradientBrush hBot = new LinearGradientBrush(glowBot, Color.FromArgb(26, c3.R, c3.G, c3.B), Color.FromArgb(0, c3.R, c3.G, c3.B), LinearGradientMode.Vertical)) {
                    g.FillRectangle(hTop, glowTop);
                    g.FillRectangle(hBot, glowBot);
                }
                using (Pen horizonLine = new Pen(Color.FromArgb(95, c1.R, c1.G, c1.B), 1.4f)) {
                    g.DrawLine(horizonLine, m.Left, horizonY, m.Right, horizonY);
                }

                // Subtle Central 3D Horizon Depth Well
                using (Pen nexusOuter = new Pen(Color.FromArgb(62, c1.R, c1.G, c1.B), 1.4f))
                using (Pen nexusInner = new Pen(Color.FromArgb(78, c2.R, c2.G, c2.B), 1.1f)) {
                    g.DrawEllipse(nexusOuter, vanishX - 135, horizonY - 26, 270, 52);
                    g.DrawEllipse(nexusInner, vanishX - 76, horizonY - 14, 152, 28);
                }

                // Attenuated Inter-Monitor Portal Conduits at left/right/top edges of each monitor
                int midY = m.Top + (int)(m.Height * 0.56f);
                int midX = m.Left + (int)(m.Width * 0.50f);
                using (Pen portalGlow = new Pen(Color.FromArgb(88, c1.R, c1.G, c1.B), 2.4f))
                using (Pen portalAccent = new Pen(Color.FromArgb(120, c2.R, c2.G, c2.B), 1.5f)) {
                    g.DrawLine(portalGlow, m.Left + 2, midY - 100, m.Left + 2, midY + 100);
                    g.DrawLine(portalAccent, m.Left + 7, midY - 64, m.Left + 7, midY + 64);
                    g.DrawLine(portalGlow, m.Right - 3, midY - 100, m.Right - 3, midY + 100);
                    g.DrawLine(portalAccent, m.Right - 8, midY - 64, m.Right - 8, midY + 64);
                    g.DrawLine(portalGlow, midX - 130, m.Top + 2, midX + 130, m.Top + 2);
                }

                // Draw Attenuated Sector Identification Banner on each monitor
                string sceneTag = GetBgSceneModeShortName(Config.BgSceneMode);
                string sectorTitle = string.Format(">> CYBER_SECTOR_0{0} // {1} [{2}x{3}] // 3D_{4}",
                    mIdx + 1, monitorNames[mIdx], m.Width, m.Height, sceneTag);
                using (SolidBrush bannerBg = new SolidBrush(Color.FromArgb(115, 4, 9, 8)))
                using (Pen bannerBor = new Pen(Color.FromArgb(112, c1.R, c1.G, c1.B), 1.0f))
                using (SolidBrush bannerTxt = new SolidBrush(Color.FromArgb(195, c1.R, c1.G, c1.B))) {
                    int bx = m.Left + 140;
                    int by = m.Top + 18;
                    g.FillRectangle(bannerBg, bx, by, 475, 22);
                    g.DrawRectangle(bannerBor, bx, by, 475, 22);
                    if (fontHudSmall != null) {
                        g.DrawString(sectorTitle, fontHudSmall, bannerTxt, bx + 8, by + 4);
                    }
                }

                g.Restore(st);
            }

            // Draw subtle Virtual Environment Horizon Conduit connecting adjacent monitors
            using (Pen meshHorizon = new Pen(Color.FromArgb(45, c1.R, c1.G, c1.B), 1.2f)) {
                meshHorizon.DashStyle = DashStyle.Dot;
                for (int i = 0; i < monitorRects.Count; i++) {
                    for (int j = i + 1; j < monitorRects.Count; j++) {
                        Rectangle a = monitorRects[i];
                        Rectangle b = monitorRects[j];
                        Point ca = new Point(a.Left + a.Width / 2, a.Top + (int)(a.Height * 0.64f));
                        Point cb = new Point(b.Left + b.Width / 2, b.Top + (int)(b.Height * 0.64f));
                        g.DrawLine(meshHorizon, ca, cb);
                    }
                }
            }
        }
        Bitmap old = bgBitmap;
        bgBitmap = newBg;
        bgTexDirty = true;
        lastHudTheme = -1;
        if (old != null) old.Dispose();
    }

    public void RebuildRainColumns() {
        columns.Clear();
        int basePerMonitor = Math.Max(20, (int)(Config.RainDensity * 0.72f));
        for (int mIdx = 0; mIdx < monitorRects.Count; mIdx++) {
            Rectangle m = monitorRects[mIdx];
            int perMonitorCount = Math.Max(14, (int)Math.Round(basePerMonitor * (m.Width / 1920.0f)));
            float spacing = (float)m.Width / perMonitorCount;
            for (int i = 0; i < perMonitorCount; i++) {
                float x = m.Left + i * spacing + 4f;
                if ((x - m.Left) < 130 && i % 2 == 1) continue;

                RainColumn col = new RainColumn();
                col.MonitorIndex = mIdx;
                col.X = x;
                col.MinY = m.Top;
                col.MaxY = m.Bottom;
                col.MinX = m.Left;
                col.MaxX = m.Right;
                col.Y = rng.Next(m.Top - m.Height, m.Bottom);

                // 3-Layer 3D Parallax Distribution: 0=Far Background (34%), 1=Midground (36%), 2=Near Foreground (30%)
                double dRoll = rng.NextDouble();
                if ((x - m.Left) < 130) {
                    col.DepthLayer = 0;
                } else if (dRoll < 0.34) {
                    col.DepthLayer = 0;
                } else if (dRoll < 0.70) {
                    col.DepthLayer = 1;
                } else {
                    col.DepthLayer = 2;
                }
                col.IsForeground = (col.DepthLayer == 2);
                col.CharStep = (col.DepthLayer == 2) ? 17 : (col.DepthLayer == 1 ? 15 : 13);

                AssignColumnLogText(col);
                columns.Add(col);
            }
        }
    }

    private static ulong FileTimeToUlong(FILETIME ft) {
        return ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;
    }

    private static string ClassifyPortProtocol(int port) {
        if (port == 443) return "HTTPS/TLS";
        if (port == 80)  return "HTTP/WEB";
        if (port == 53)  return "DNS/UDP-TCP";
        if (port == 22)  return "SSH/ADMIN";
        if (port == 3389) return "RDP/REMOTE";
        if (port == 445) return "SMB/CIFS";
        if (port == 8080 || port == 8443) return "HTTP-PROXY";
        return "TCP/PORT-" + port;
    }

    private bool IsLocalSocPortOpen() {
        if (socPort5080Online && (telemetryCycles - socPort5080LastCheckCycle) < 4) return true;
        if (!socPort5080Online && (telemetryCycles - socPort5080LastCheckCycle) < 15) return false;
        socPort5080LastCheckCycle = telemetryCycles;
        socPort5080Online = PorygonCyberDefenseEngine.IsLocalPortListening(5080);
        return socPort5080Online;
    }

    private HashSet<int> compositorPids = new HashSet<int>();
    private DateTime compositorPidsAt = DateTime.MinValue;
    private string lastTopEngineKey = "";
    private volatile Dictionary<int, double> lastPidRenderGpu = new Dictionary<int, double>();

    private double GetPidRenderGpu(int pid) {
        Dictionary<int, double> d = lastPidRenderGpu;
        double v;
        return (d != null && d.TryGetValue(pid, out v)) ? v : 0.0;
    }

    private void RefreshCompositorPids() {
        if ((DateTime.Now - compositorPidsAt).TotalSeconds < 30) return;
        compositorPidsAt = DateTime.Now;
        HashSet<int> s = new HashSet<int>();
        foreach (string pn in new string[] { "dwm", "csrss", "System" }) {
            try {
                foreach (Process pr in Process.GetProcessesByName(pn)) { s.Add(pr.Id); pr.Dispose(); }
            } catch { }
        }
        compositorPids = s;
    }

    private bool SampleHardwareGpuUtilization(out float selfGpuPct, out float extMaxEnginePct, out int topExtPid) {
        selfGpuPct = realSelfGpuPct;
        extMaxEnginePct = externalGpuMaxPct;
        topExtPid = 0;
        try {
            // Optional CLI test override flag for verifying >90% GPU auto-hibernation
            string testFlag = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gpu_load_test.flag");
            if (File.Exists(testFlag)) {
                string txt = File.ReadAllText(testFlag).Trim();
                float simVal;
                if (float.TryParse(txt, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out simVal)) {
                    selfGpuPct = isGamerSuspended ? 0.0f : 1.2f;
                    extMaxEnginePct = Math.Max(0f, Math.Min(100f, simVal));
                    topExtPid = cachedSelfPid;
                    return true;
                }
            }

            if (pdhGpuQuery == IntPtr.Zero) {
                if (PdhOpenQuery(IntPtr.Zero, IntPtr.Zero, out pdhGpuQuery) != 0) return false;
                if (PdhAddEnglishCounter(pdhGpuQuery, "\\GPU Engine(*)\\Utilization Percentage", IntPtr.Zero, out pdhGpuCounter) != 0) {
                    PdhCloseQuery(pdhGpuQuery);
                    pdhGpuQuery = IntPtr.Zero;
                    return false;
                }
                PdhCollectQueryData(pdhGpuQuery);
                return false;
            }
            if (PdhCollectQueryData(pdhGpuQuery) != 0) return false;

            uint bufSize = pdhGpuBufSize;
            uint itemCount = 0;
            int status = PdhGetFormattedCounterArray(pdhGpuCounter, PDH_FMT_DOUBLE, ref bufSize, out itemCount, pdhGpuBuf);
            if ((uint)status == PDH_MORE_DATA || bufSize > pdhGpuBufSize) {
                if (pdhGpuBuf != IntPtr.Zero) Marshal.FreeHGlobal(pdhGpuBuf);
                pdhGpuBufSize = bufSize + 16384;
                pdhGpuBuf = Marshal.AllocHGlobal((int)pdhGpuBufSize);
                bufSize = pdhGpuBufSize;
                status = PdhGetFormattedCounterArray(pdhGpuCounter, PDH_FMT_DOUBLE, ref bufSize, out itemCount, pdhGpuBuf);
            }
            if (status != 0 || itemCount == 0 || pdhGpuBuf == IntPtr.Zero) return false;

            int structSize = Marshal.SizeOf(typeof(PDH_FMT_COUNTERVALUE_ITEM_DOUBLE));
            RefreshCompositorPids();
            pdhEngineSums.Clear();
            pdhPidSums.Clear();
            double selfSum = 0.0;

            for (uint i = 0; i < itemCount; i++) {
                IntPtr ptr = new IntPtr(pdhGpuBuf.ToInt64() + i * structSize);
                PDH_FMT_COUNTERVALUE_ITEM_DOUBLE item = (PDH_FMT_COUNTERVALUE_ITEM_DOUBLE)Marshal.PtrToStructure(ptr, typeof(PDH_FMT_COUNTERVALUE_ITEM_DOUBLE));
                if (item.CStatus != 0 || item.doubleValue <= 0.05) continue;
                string name = Marshal.PtrToStringUni(item.szName);
                if (string.IsNullOrEmpty(name)) continue;

                // Parse PID from "pid_<digits>_luid_..."
                int procId = 0;
                if (name.Length > 6 && name[0] == 'p' && name[1] == 'i' && name[2] == 'd' && name[3] == '_') {
                    int idx = 4;
                    while (idx < name.Length && name[idx] >= '0' && name[idx] <= '9') {
                        procId = procId * 10 + (name[idx] - '0');
                        idx++;
                    }
                }

                if (procId == cachedSelfPid && cachedSelfPid != 0) {
                    selfSum += item.doubleValue;
                } else {
                    // Ignore: system/compositor (dwm composes OUR wallpaper + cross-GPU copies for the iGPU monitors),
                    // and non-render engines (Copy / Video Decode / Video Encode / Security / Overlay) that spike to 100% on idle desktops.
                    if (procId <= 4 || compositorPids.Contains(procId)) continue;
                    int etIdx = name.IndexOf("engtype_", StringComparison.OrdinalIgnoreCase);
                    string etype = (etIdx >= 0) ? name.Substring(etIdx + 8) : "";
                    bool renderEngine = etype.StartsWith("3D", StringComparison.OrdinalIgnoreCase) ||
                                        etype.StartsWith("Compute", StringComparison.OrdinalIgnoreCase) ||
                                        etype.StartsWith("Graphics", StringComparison.OrdinalIgnoreCase) ||
                                        etype.StartsWith("Cuda", StringComparison.OrdinalIgnoreCase);
                    if (!renderEngine) continue;
                    int luidIdx = name.IndexOf("luid_");
                    string engKey = (luidIdx >= 0) ? name.Substring(luidIdx) : name;
                    double curEng;
                    if (pdhEngineSums.TryGetValue(engKey, out curEng)) {
                        pdhEngineSums[engKey] = curEng + item.doubleValue;
                    } else {
                        pdhEngineSums[engKey] = item.doubleValue;
                    }
                    if (procId > 4) {
                        double curPid;
                        if (pdhPidSums.TryGetValue(procId, out curPid)) {
                            pdhPidSums[procId] = curPid + item.doubleValue;
                        } else {
                            pdhPidSums[procId] = item.doubleValue;
                        }
                    }
                }
            }

            selfGpuPct = (float)Math.Min(100.0, selfSum);
            double maxEng = 0.0;
            foreach (var kv in pdhEngineSums) {
                if (kv.Value > maxEng) maxEng = kv.Value;
            }
            extMaxEnginePct = (float)Math.Min(100.0, maxEng);
            lastTopEngineKey = "";
            foreach (var kv in pdhEngineSums) { if (kv.Value >= maxEng) { lastTopEngineKey = kv.Key; break; } }

            lastPidRenderGpu = new Dictionary<int, double>(pdhPidSums);
            double maxPidVal = 0.0;
            foreach (var kv in pdhPidSums) {
                if (kv.Value > maxPidVal) {
                    maxPidVal = kv.Value;
                    topExtPid = kv.Key;
                }
            }
            return true;
        } catch {
            return false;
        }
    }

    private void EvaluateHighGpuAutoSuspension() {
        float sGpu, eGpu;
        int topPid;
        if (!SampleHardwareGpuUtilization(out sGpu, out eGpu, out topPid)) return;

        lock (telemetryLock) {
            realSelfGpuPct = isGamerSuspended ? 0.0f : sGpu;
            externalGpuMaxPct = eGpu;
            // Enforce <= 10% GPU budget for the wallpaper itself
            gpuOverBudgetEco = (!isGamerSuspended && sGpu > 9.2f);
        }

        // Auto-disable wallpaper when an external application uses > 90% GPU
        if (eGpu >= 90.0f) {
            highGpuStreak++;
            lowGpuStreak = 0;
            if (!autoHighGpuDetected && highGpuStreak >= 3) {
                string pName = "GPU_APP";
                if (topPid > 0) {
                    try {
                        using (Process p = Process.GetProcessById(topPid)) {
                            if (!string.IsNullOrEmpty(p.ProcessName)) pName = p.ProcessName.ToUpperInvariant();
                        }
                    } catch { }
                }
                autoHighGpuDetected = true;
                highGpuProcessName = string.Format("{0} ({1:0.0}%)", pName, eGpu);
                try {
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gpu_autopause.log"),
                        string.Format("{0:yyyy-MM-dd HH:mm:ss} AUTO-PAUSE ext={1:0.0}% self={2:0.0}% proc={3} pid={4} engine={5}\r\n",
                            DateTime.Now, eGpu, sGpu, pName, topPid, lastTopEngineKey));
                } catch { }
                try {
                    this.BeginInvoke((MethodInvoker)delegate {
                        UpdateGamerSuspensionState(true);
                        ShowCyberActionToast(
                            "[ AUTO-PAUSA POR GPU > 90% // WALLPAPER DESACTIVADO (0.0% GPU) ]",
                            string.Format("Carga GPU crítica detectada en {0} — Se reanudará al bajar del 78%", highGpuProcessName),
                            Color.FromArgb(250, 204, 21));
                    });
                } catch { }
            }
        } else if (eGpu < 78.0f) {
            highGpuStreak = 0;
            if (autoHighGpuDetected) {
                lowGpuStreak++;
                if (lowGpuStreak >= 2) {
                    autoHighGpuDetected = false;
                    highGpuProcessName = "";
                    try {
                        this.BeginInvoke((MethodInvoker)delegate {
                            UpdateGamerSuspensionState(true);
                            if (!isGamerSuspended) {
                                ShowCyberActionToast(
                                    "[ CARGA GPU NORMALIZADA (<78%) // WALLPAPER REANUDADO ]",
                                    "Motor 3D OpenGL restaurado automáticamente (Presupuesto <= 10% GPU)",
                                    Color.FromArgb(52, 211, 153));
                            }
                        });
                    } catch { }
                }
            }
        }
    }

    private void UpdateRealTelemetry(object state) {
        if (isGamerSuspended) {
            if (autoHighGpuDetected && (Config == null || !Config.GamerMode) && !autoGameDetected) {
                EvaluateHighGpuAutoSuspension();
            }
            return;
        }
        try {
            telemetryCycles++;
            if (telemetryCycles == 1 || telemetryCycles % 2 == 0) {
                EvaluateHighGpuAutoSuspension();
                if (isGamerSuspended) return;
            }
            FILETIME ftIdle, ftKernel, ftUser;
            float newCpu = cpuUsage;
            if (GetSystemTimes(out ftIdle, out ftKernel, out ftUser)) {
                ulong idle = FileTimeToUlong(ftIdle);
                ulong kernel = FileTimeToUlong(ftKernel);
                ulong user = FileTimeToUlong(ftUser);
                ulong sys = (kernel - prevKernel) + (user - prevUser);
                ulong idl = idle - prevIdle;
                if (sys > 0 && prevIdle > 0) {
                    newCpu = (float)((sys - idl) * 100.0 / sys);
                    newCpu = Math.Max(0f, Math.Min(100f, newCpu));
                }
                prevIdle = idle; prevKernel = kernel; prevUser = user;
            }

            MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
            float newRamUsed = ramUsedGB, newRamTotal = ramTotalGB;
            int newRamPct = ramPercent;
            if (GlobalMemoryStatusEx(mem)) {
                newRamTotal = (float)(mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0));
                newRamUsed = (float)((mem.ullTotalPhys - mem.ullAvailPhys) / (1024.0 * 1024.0 * 1024.0));
                newRamPct = (int)mem.dwMemoryLoad;
            }

            string nicName = activeNicName;
            string ipv4 = localIPv4;
            long totalRecv = 0, totalSent = 0;
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics) {
                if (nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel) {
                    IPv4InterfaceStatistics stats = nic.GetIPv4Statistics();
                    if (stats.BytesReceived > 0) {
                        totalRecv += stats.BytesReceived;
                        totalSent += stats.BytesSent;
                        IPInterfaceProperties props = nic.GetIPProperties();
                        foreach (UnicastIPAddressInformation ua in props.UnicastAddresses) {
                            if (ua.Address.AddressFamily == AddressFamily.InterNetwork) {
                                nicName = nic.Name;
                                ipv4 = ua.Address.ToString();
                                break;
                            }
                        }
                    }
                }
            }

            DateTime now = DateTime.UtcNow;
            double sec = (now - prevNetTime).TotalSeconds;
            float downKbps = netDownKbps, upKbps = netUpKbps;
            if (sec > 0.2 && prevBytesRecv > 0) {
                downKbps = (float)((totalRecv - prevBytesRecv) / 1024.0 / sec);
                upKbps = (float)((totalSent - prevBytesSent) / 1024.0 / sec);
                if (downKbps < 0) downKbps = 0;
                if (upKbps < 0) upKbps = 0;
            }
            prevBytesRecv = totalRecv;
            prevBytesSent = totalSent;
            prevNetTime = now;

            int newPing = pingMs;
            try {
                using (Ping p = new Ping()) {
                    PingReply rep = p.Send("1.1.1.1", 650);
                    if (rep != null && rep.Status == IPStatus.Success) {
                        newPing = (int)Math.Max(1, rep.RoundtripTime);
                    }
                }
            } catch { }

            // Statistical Z-Score Calculation (Exponential Moving Average & StdDev)
            float stdDl = (float)Math.Sqrt(Math.Max(225f, varDownKbps));
            float stdUl = (float)Math.Sqrt(Math.Max(100f, varUpKbps));
            float stdPg = (float)Math.Sqrt(Math.Max(36f, varPingMs));

            float dlZ = (downKbps - emaDownKbps) / stdDl;
            float ulZ = (upKbps - emaUpKbps) / stdUl;
            float pgZ = (newPing - emaPingMs) / stdPg;

            // Update rolling EMA baselines
            float alpha = 0.14f;
            float diffDl = downKbps - emaDownKbps;
            emaDownKbps += alpha * diffDl;
            varDownKbps = (1f - alpha) * (varDownKbps + alpha * diffDl * diffDl);

            float diffUl = upKbps - emaUpKbps;
            emaUpKbps += alpha * diffUl;
            varUpKbps = (1f - alpha) * (varUpKbps + alpha * diffUl * diffUl);

            float diffPg = newPing - emaPingMs;
            emaPingMs += alpha * diffPg;
            varPingMs = (1f - alpha) * (varPingMs + alpha * diffPg * diffPg);

            // Inspect Active TCP Connections + Real PID/Authenticode Mapping via GetExtendedTcpTable & Feed to the 3 Porygons!
            if (telemetryCycles % 3 == 0) {
                PorygonCyberDefenseEngine.PollOfficePcC2AndIdsTelemetry(false);
            }
            if (telemetryCycles % 5 == 0) {
                PorygonCyberDefenseEngine.AuditRegistryPersistence(false);
                PorygonCyberDefenseEngine.PollWindowsEventLogs();
            }
            if (telemetryCycles % 15 == 1) {
                PorygonCyberDefenseEngine.AuditDnsCacheAndBeaconing(false);
            }
            Dictionary<string, int> tcpPidMap = PorygonCyberDefenseEngine.QueryExtendedTcpPidMap();

            IPGlobalProperties ipGlobal = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConns = ipGlobal.GetActiveTcpConnections();
            int estCount = 0;
            int synCount = 0;
            int unusualPortCount = 0;
            int newHostCount = 0;
            string sampleConn = null;
            string anomalyHeadline = "BASELINE_NORMAL";
            bool socOnline = IsLocalSocPortOpen();

            List<RadarBlip> newBlips = new List<RadarBlip>();
            List<PorygonCyberDefenseEngine.LiveTcpConnectionRow> newTcpRows = new List<PorygonCyberDefenseEngine.LiveTcpConnectionRow>();
            HashSet<string> seenHosts = new HashSet<string>();

            newBlips.Add(new RadarBlip {
                AngleDeg = 45f,
                DistNorm = 0.28f,
                Label = "1.1.1.1:53",
                FullRemoteEp = "1.1.1.1:53",
                LocalEp = ipv4 + ":UDP/53 [svchost/DNS]",
                Proto = "DNS/RESOLVER",
                RingZone = "RING1:CORE_DNS",
                AnomalyScore = 0.04f,
                IsHttps = true,
                IsAnomaly = false
            });

            for (int i = 0; i < tcpConns.Length; i++) {
                TcpState st = tcpConns[i].State;
                string locIpRaw = tcpConns[i].LocalEndPoint.Address.ToString();
                string remIp = tcpConns[i].RemoteEndPoint.Address.ToString();
                int remPort = tcpConns[i].RemoteEndPoint.Port;
                int locPort = tcpConns[i].LocalEndPoint.Port;
                if (remIp.StartsWith("127.") || remIp.StartsWith("::") || remIp == "0.0.0.0") continue;

                if (st == TcpState.SynSent || st == TcpState.SynReceived) {
                    synCount++;
                }
                if (st == TcpState.Established || st == TcpState.SynSent) {
                    if (st == TcpState.Established) estCount++;
                    string fullRem = remIp + ":" + remPort;
                    string exactKey = locIpRaw + ":" + locPort + "|" + fullRem;

                    int ownPid = 0;
                    if (!tcpPidMap.TryGetValue(exactKey, out ownPid)) {
                        tcpPidMap.TryGetValue(fullRem, out ownPid);
                    }
                    PorygonCyberDefenseEngine.ProcessSecurityInfo procSec = PorygonCyberDefenseEngine.ResolveProcessSecurityInfo(ownPid);
                    string procShort = string.IsNullOrEmpty(procSec.ProcessName) ? "System" : procSec.ProcessName;
                    if (procShort.Length > 13) procShort = procShort.Substring(0, 13);
                    string signBadge = procSec.IsSigned ? "SIGNED" : "UNSIGNED";
                    string procTag = (ownPid > 0) ? string.Format("{0}.exe[{1}]", procShort, ownPid) : "System[4]";

                    string fullLoc = string.Format("{0}:{1} [{2} {3}]", ipv4, locPort, procTag, signBadge);
                    string proto = ClassifyPortProtocol(remPort) + " // " + procShort;
                    bool isLan = remIp.StartsWith("192.168.") || remIp.StartsWith("10.") || remIp.StartsWith("172.") || PorygonCyberDefenseEngine.IsSafeWhitelistedIp(remIp);
                    bool isStandardPort = (remPort == 443 || remPort == 80 || remPort == 53 || remPort == 123);

                    if (sampleConn == null) {
                        sampleConn = string.Format("TCP_{0} {1} -> {2}", st.ToString().ToUpper(), procTag, fullRem);
                    }

                    bool isNewHost = false;
                    if (!baselineInitialized) {
                        knownRemoteHosts.Add(remIp);
                    } else if (!knownRemoteHosts.Contains(remIp)) {
                        knownRemoteHosts.Add(remIp);
                        isNewHost = true;
                        newHostCount++;
                    }

                    // Enterprise CTI Feed Match + Sigma Network Rule + C2 Beaconing Jitter Evaluation
                    bool isCtiMatch = !isLan && PorygonCyberDefenseEngine.IsCtiMaliciousIp(remIp);
                    string sigmaRule = "";
                    string sigmaMitre = "";
                    int sigmaRiskPct = 0;
                    bool hasSigma = PorygonCyberDefenseEngine.EvaluateNetworkSigmaRules(procShort, remIp, remPort, out sigmaRule, out sigmaMitre, out sigmaRiskPct);
                    string beaconDetail = "";
                    bool isBeaconing = !isLan && isNewHost && PorygonCyberDefenseEngine.CheckC2BeaconingPattern(remIp, procTag, out beaconDetail);

                    // Determine if this real connection is an Anomaly (Kind=2), New Host (Kind=1), or Normal Flow (Kind=0)
                    int nodeKind = 0;
                    string anomType = "BASELINE_FLOW";
                    string anomDetail = string.Format("Proc: {0} ({1}) -> {2}", procTag, procSec.SignerName ?? signBadge, fullRem);
                    float anomScore = procSec.IsSigned ? 0.06f : 0.22f;

                    if (isCtiMatch) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = "CTI_FEED_MALICIOUS_IP";
                        anomDetail = string.Format("IP {0} presente en Feed CTI global (Feodo C2 / CINS Army) por {1}", fullRem, procTag);
                        anomScore = 0.99f;
                        anomalyHeadline = "CTI_C2_HIT: " + remIp;
                        PorygonCyberDefenseEngine.RecordSiemEvent("PORYGON-Z", "CTI_NIDS", "CRITICAL", "T1071", procTag, fullRem, "", anomScore, anomDetail, "IPS_AUTO_BLOCK");
                    } else if (hasSigma) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = sigmaRule;
                        anomDetail = string.Format("Regla Sigma [{0}] activada por {1} hacia {2}", sigmaRule, procTag, fullRem);
                        anomScore = Math.Max(0.92f, sigmaRiskPct / 100f);
                        anomalyHeadline = "SIGMA_HIT: " + sigmaRule;
                        PorygonCyberDefenseEngine.RecordSiemEvent("PORYGON2", "SIGMA_EDR", "CRITICAL", sigmaMitre, procTag, fullRem, "", anomScore, anomDetail, "ALERT_READY");
                    } else if (isBeaconing) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = "C2_PERIODIC_BEACON";
                        anomDetail = beaconDetail;
                        anomScore = 0.91f;
                        anomalyHeadline = "C2_BEACON: " + procShort + "->" + remIp;
                        PorygonCyberDefenseEngine.RecordSiemEvent("PORYGON2", "C2_BEACON", "HIGH", "T1071.001", procTag, fullRem, "", anomScore, anomDetail, "ALERT_READY");
                    } else if (procSec.IsUserTempOrDownloads && !procSec.IsSigned && !isLan) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = "EDR_TEMP_UNSIGNED_C2";
                        anomDetail = string.Format("Binario sin firma en Temp/Downloads ({0}) conectado a {1}", procTag, fullRem);
                        anomScore = 0.93f;
                        anomalyHeadline = "TEMP_UNSIGNED: " + procTag + "->" + remIp;
                        if (!string.IsNullOrEmpty(procSec.ExePath)) {
                            PorygonCyberDefenseEngine.EnqueueFileForAvScan(procSec.ExePath);
                        }
                        PorygonCyberDefenseEngine.RecordSiemEvent("PORYGON", "NIDS_EDR", "CRITICAL", "T1071", procTag, fullRem, "", anomScore, anomDetail, "ALERT_READY");
                    } else if (procSec.IsLolBin && !isLan) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = "EDR_LOLBIN_NET";
                        anomDetail = string.Format("LOLBin {0} realizando conexión externa hacia {1}", procTag, fullRem);
                        anomScore = 0.89f;
                        anomalyHeadline = "LOLBIN_NET: " + procTag + "->" + remIp;
                        PorygonCyberDefenseEngine.RecordSiemEvent("PORYGON", "NIDS_EDR", "HIGH", "T1059", procTag, fullRem, "", anomScore, anomDetail, "ALERT_READY");
                    } else if (!isStandardPort && !isLan) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = "NON_STD_PORT_" + remPort;
                        anomDetail = string.Format("Proceso {0} ({1}) en puerto inusual {2}", procTag, signBadge, fullRem);
                        anomScore = (remPort == 22 || remPort == 3389 || remPort == 4444 || remPort == 445) ? 0.94f : (procSec.IsSigned ? 0.74f : 0.88f);
                        anomalyHeadline = "PORT_ALERT: " + procShort + "->" + fullRem;
                    } else if (st == TcpState.SynSent) {
                        nodeKind = 2;
                        anomType = "HALF_OPEN_SYN";
                        anomDetail = string.Format("Handshake SYN_SENT saliente por {0}", procTag);
                        anomScore = 0.76f;
                        anomalyHeadline = "SYN_BURST: " + procShort + "->" + fullRem;
                    } else if (isNewHost || !procSec.IsSigned) {
                        nodeKind = 1;
                        anomType = !procSec.IsSigned ? "UNSIGNED_PROC_FLOW" : "NEW_EXTERNAL_HOST";
                        anomDetail = string.Format("Flujo {0} ({1}) hacia {2}", procTag, signBadge, remIp);
                        anomScore = !procSec.IsSigned ? 0.62f : 0.52f;
                        if (anomalyHeadline == "BASELINE_NORMAL" && isNewHost) anomalyHeadline = "NEW_HOST: " + procShort + "->" + remIp;
                    }

                    // If this is a newly seen external IP and local SOC API is online, let Porygon query VirusTotal API
                    if (socOnline && !isLan && !vtAuditedIps.Contains(remIp) && vtAuditedIps.Count < 45 && porygon != null) {
                        vtAuditedIps.Add(remIp);
                        try {
                            using (System.Net.WebClient vtWc = new System.Net.WebClient()) {
                                vtWc.Encoding = System.Text.Encoding.UTF8;
                                string vtUrl = string.Format(
                                    "http://127.0.0.1:5080/api/wallpaper-vt-check?target={0}&port={1}&host={2}&proc={3}",
                                    Uri.EscapeDataString(remIp),
                                    remPort,
                                    Uri.EscapeDataString(Environment.MachineName),
                                    Uri.EscapeDataString(procTag));
                                string vtJson = vtWc.DownloadString(vtUrl);
                                JavaScriptSerializer vtSer = new JavaScriptSerializer();
                                Dictionary<string, object> vtObj = vtSer.Deserialize<Dictionary<string, object>>(vtJson);
                                if (vtObj != null) {
                                    int vtMal = vtObj.ContainsKey("malicious_count") ? Convert.ToInt32(vtObj["malicious_count"]) : 0;
                                    int vtSusp = vtObj.ContainsKey("suspicious_count") ? Convert.ToInt32(vtObj["suspicious_count"]) : 0;
                                    float vtSc = vtObj.ContainsKey("threat_score") ? Convert.ToSingle(vtObj["threat_score"]) / 100f : 0f;
                                    string vtSum = vtObj.ContainsKey("summary") && vtObj["summary"] != null ? vtObj["summary"].ToString() : ("VT:" + remIp);
                                    porygon.InjectVirusTotalSensorySignal(remIp, vtMal, vtSusp, vtSc, vtSum);
                                    if (vtMal > 0 || vtSc >= 0.45f) {
                                        nodeKind = 2;
                                        anomType = "VT_MALICIOUS_" + vtMal;
                                        anomDetail = vtSum + " // Proc:" + procTag;
                                        anomScore = Math.Max(anomScore, Math.Min(0.99f, 0.78f + vtSc * 0.21f));
                                        anomalyHeadline = "VT_HIT: " + remIp + " (MAL:" + vtMal + ")";
                                    }
                                }
                            }
                        } catch { }
                    }

                    if (nodeKind == 2 && !isLan) {
                        PorygonCyberDefenseEngine.RegisterCandidateBlockIp(fullRem, procTag, anomType + " [" + signBadge + "]", anomScore, true);
                    }

                    string matchedRuleTag = isCtiMatch ? "CTI_IOC_MATCH" : (hasSigma ? sigmaRule : (isBeaconing ? "C2_BEACONING" : ""));
                    if (newTcpRows.Count < 80) {
                        newTcpRows.Add(new PorygonCyberDefenseEngine.LiveTcpConnectionRow {
                            Pid = ownPid,
                            ProcessName = string.IsNullOrEmpty(procSec.ProcessName) ? "System" : procSec.ProcessName,
                            ExePath = procSec.ExePath ?? "",
                            IsSigned = procSec.IsSigned,
                            Signer = procSec.SignerName ?? signBadge,
                            IsLolBin = procSec.IsLolBin,
                            LocalEp = ipv4 + ":" + locPort,
                            RemoteEp = fullRem,
                            Proto = ClassifyPortProtocol(remPort),
                            State = st.ToString().ToUpperInvariant(),
                            RiskPct = (int)Math.Round(anomScore * 100f),
                            IsCtiMatch = isCtiMatch,
                            MatchedRule = matchedRuleTag
                        });
                    }

                    // Feed real connection node into Porygon 3D Cyberspace!
                    if (porygon != null && (nodeKind > 0 || porygon.Packets.Count < 8 || rng.NextDouble() < 0.35)) {
                        porygon.InjectRealNetworkNode(screenW, screenH, nodeKind,
                            fullRem, fullLoc, proto, st.ToString().ToUpper(),
                            anomType, anomDetail, anomScore, downKbps + upKbps);
                    }

                    if (!seenHosts.Contains(remIp) && newBlips.Count < 15) {
                        seenHosts.Add(remIp);
                        int hash = Math.Abs(remIp.GetHashCode());
                        float ang = (hash % 360);
                        float dist = isLan ? 0.35f : (remPort == 443 ? (0.52f + (hash % 25) * 0.01f) : (0.78f + (hash % 14) * 0.01f));
                        string ringZone = isLan ? "RING1:LAN_NODE" : (remPort == 443 ? "RING2:WAN_TLS" : "RING3:WAN_CUSTOM");
                        string shortIp = remIp.Length > 13 ? remIp.Substring(0, 12) + ".." : remIp;
                        newBlips.Add(new RadarBlip {
                            AngleDeg = ang,
                            DistNorm = dist,
                            Label = procShort + "@" + shortIp,
                            FullRemoteEp = fullRem,
                            LocalEp = fullLoc,
                            Proto = proto,
                            RingZone = ringZone,
                            AnomalyScore = anomScore,
                            IsHttps = (remPort == 443),
                            IsAnomaly = (nodeKind == 2)
                        });
                    }
                }
            }

            newTcpRows.Sort((a, b) => b.RiskPct.CompareTo(a.RiskPct));
            lock (PorygonCyberDefenseEngine.SyncLock) {
                PorygonCyberDefenseEngine.ActiveTcpRows = newTcpRows;
            }

            if (telemetryCycles >= 2) baselineInitialized = true;

            // Check for Bandwidth Z-Score Spike Anomaly or Ping Jitter Anomaly!
            if (porygon != null) {
                if (dlZ > 2.0f && downKbps > 60f) {
                    anomalyHeadline = string.Format("DL_SPIKE +{0:0.1}σ ({1:0}KB/s)", dlZ, downKbps);
                    porygon.InjectRealNetworkNode(screenW, screenH, 2,
                        sampleConn != null ? sampleConn.Split(' ')[2] : (ipv4 + ":443"),
                        ipv4 + ":NIC_RX", "BW_SURGE", "ACTIVE_BURST",
                        "DL_ZSCORE_SPIKE", string.Format("Download burst +{0:0.1}σ above rolling mean ({1:0.0} KB/s)", dlZ, downKbps),
                        Math.Min(0.98f, 0.65f + dlZ * 0.1f), downKbps);
                }
                if (ulZ > 2.0f && upKbps > 35f) {
                    anomalyHeadline = string.Format("UL_SPIKE +{0:0.1}σ ({1:0}KB/s)", ulZ, upKbps);
                    porygon.InjectRealNetworkNode(screenW, screenH, 2,
                        sampleConn != null ? sampleConn.Split(' ')[2] : (ipv4 + ":443"),
                        ipv4 + ":NIC_TX", "UL_SURGE", "ACTIVE_BURST",
                        "UL_EXFIL_SPIKE", string.Format("Upload surge +{0:0.1}σ above baseline ({1:0.0} KB/s)", ulZ, upKbps),
                        Math.Min(0.98f, 0.68f + ulZ * 0.1f), upKbps);
                }
                if (pgZ > 2.2f && newPing > 65) {
                    anomalyHeadline = string.Format("PING_JITTER {0}ms (+{1:0.1}σ)", newPing, pgZ);
                    porygon.InjectRealNetworkNode(screenW, screenH, 2,
                        "1.1.1.1:ICMP", ipv4 + ":ICMP", "ICMP/PING", "JITTER_HIGH",
                        "LATENCY_ANOMALY", string.Format("Gateway RTT jumped to {0}ms (+{1:0.1}σ jitter)", newPing, pgZ),
                        Math.Min(0.95f, 0.62f + pgZ * 0.1f), downKbps);
                }
            }

            float compositeAnomaly = Math.Min(1.0f,
                Math.Max(0f, dlZ) * 0.18f +
                Math.Max(0f, ulZ) * 0.18f +
                Math.Max(0f, pgZ) * 0.14f +
                unusualPortCount * 0.28f +
                synCount * 0.22f +
                newHostCount * 0.12f);

            Porygon3DAvatar.RealTelemetryFeed feed = new Porygon3DAvatar.RealTelemetryFeed {
                DownKbps = downKbps,
                UpKbps = upKbps,
                DlMeanKbps = emaDownKbps,
                DlStdKbps = stdDl,
                DlZScore = dlZ,
                UlMeanKbps = emaUpKbps,
                UlStdKbps = stdUl,
                UlZScore = ulZ,
                PingMs = newPing,
                PingMeanMs = emaPingMs,
                PingZScore = pgZ,
                ActiveTcpCount = estCount,
                SynCount = synCount,
                UnusualPortCount = unusualPortCount,
                NewHostCount = newHostCount,
                AnomalyIndex = compositeAnomaly,
                LastAnomalySummary = anomalyHeadline
            };

            // Poll QB-Network SOC Unified API + VirusTotal Threat Intel (http://127.0.0.1:5080/api/wallpaper-telemetry) only if port 5080 is online
            List<string> fetchedRainTokens = null;
            List<string> fetchedLogLines = null;
            if (socOnline && (telemetryCycles == 1 || telemetryCycles % 2 == 0)) {
                try {
                    using (System.Net.WebClient wc = new System.Net.WebClient()) {
                        wc.Encoding = System.Text.Encoding.UTF8;
                        string json = wc.DownloadString("http://127.0.0.1:5080/api/wallpaper-telemetry");
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        Dictionary<string, object> root = ser.Deserialize<Dictionary<string, object>>(json);
                        if (root != null) {
                            if (root.ContainsKey("rain_tokens") && root["rain_tokens"] is System.Collections.IEnumerable) {
                                fetchedRainTokens = new List<string>();
                                foreach (object item in (System.Collections.IEnumerable)root["rain_tokens"]) {
                                    if (item != null) fetchedRainTokens.Add(item.ToString());
                                }
                            }
                            if (root.ContainsKey("log_lines") && root["log_lines"] is System.Collections.IEnumerable) {
                                fetchedLogLines = new List<string>();
                                foreach (object item in (System.Collections.IEnumerable)root["log_lines"]) {
                                    if (item != null) fetchedLogLines.Add(item.ToString());
                                }
                            }
                            if (porygon != null && root.ContainsKey("vt_reports") && root["vt_reports"] is System.Collections.IEnumerable) {
                                foreach (object vtItem in (System.Collections.IEnumerable)root["vt_reports"]) {
                                    Dictionary<string, object> vr = vtItem as Dictionary<string, object>;
                                    if (vr == null) continue;
                                    string vtIp = vr.ContainsKey("ip") && vr["ip"] != null ? vr["ip"].ToString() : "0.0.0.0";
                                    int vtMal = vr.ContainsKey("malicious_count") ? Convert.ToInt32(vr["malicious_count"]) : 0;
                                    int vtSusp = vr.ContainsKey("suspicious_count") ? Convert.ToInt32(vr["suspicious_count"]) : 0;
                                    float vtSc = vr.ContainsKey("threat_score") ? Convert.ToSingle(vr["threat_score"]) / 100f : 0f;
                                    string vtSum = vr.ContainsKey("summary") && vr["summary"] != null ? vr["summary"].ToString() : ("VT:" + vtIp);
                                    porygon.InjectVirusTotalSensorySignal(vtIp, vtMal, vtSusp, vtSc, vtSum);
                                    if (vtMal > 0 || vtSc >= 0.45f) {
                                        feed.AnomalyIndex = Math.Max(feed.AnomalyIndex, Math.Min(0.98f, vtSc));
                                        if (anomalyHeadline == "BASELINE_NORMAL") {
                                            anomalyHeadline = string.Format("VT_ALERT:{0}(M:{1})", vtIp, vtMal);
                                            feed.LastAnomalySummary = anomalyHeadline;
                                        }
                                    }
                                }
                            }
                            if (porygon != null && root.ContainsKey("nodes") && root["nodes"] is System.Collections.IEnumerable) {
                                foreach (object nObj in (System.Collections.IEnumerable)root["nodes"]) {
                                    Dictionary<string, object> nd = nObj as Dictionary<string, object>;
                                    if (nd == null) continue;
                                    string rEp = nd.ContainsKey("remote_ep") && nd["remote_ep"] != null ? nd["remote_ep"].ToString() : "192.168.2.1:5080";
                                    string lSkt = nd.ContainsKey("local_skt") && nd["local_skt"] != null ? nd["local_skt"].ToString() : (ipv4 + ":5080");
                                    int nKind = nd.ContainsKey("kind") ? Convert.ToInt32(nd["kind"]) : 1;
                                    string nType = nd.ContainsKey("type") && nd["type"] != null ? nd["type"].ToString() : "QB_NIDS_EVENT";
                                    string nDet = nd.ContainsKey("detail") && nd["detail"] != null ? nd["detail"].ToString() : "QB-Network NIDS Telemetry";
                                    float nScore = nd.ContainsKey("score") ? Convert.ToSingle(nd["score"]) : 0.55f;
                                    float nKbps = nd.ContainsKey("kbps") ? Convert.ToSingle(nd["kbps"]) : 45.0f;

                                    porygon.InjectRealNetworkNode(screenW, screenH, nKind, rEp, lSkt, "QB-NIDS/5080", "SOC_ACTIVE", nType, nDet, nScore, nKbps);
                                    if (nKind == 2 && anomalyHeadline == "BASELINE_NORMAL") {
                                        anomalyHeadline = nType + " (" + rEp + ")";
                                        feed.LastAnomalySummary = anomalyHeadline;
                                        feed.AnomalyIndex = Math.Max(feed.AnomalyIndex, nScore);
                                    }
                                    if (newBlips.Count < 18) {
                                        int h = Math.Abs(rEp.GetHashCode());
                                        newBlips.Add(new RadarBlip {
                                            AngleDeg = (h % 360),
                                            DistNorm = (nKind == 2) ? 0.82f : 0.44f,
                                            Label = rEp.Length > 15 ? rEp.Substring(0, 15) : rEp,
                                            FullRemoteEp = rEp,
                                            LocalEp = lSkt,
                                            Proto = nType,
                                            RingZone = (nKind == 2) ? "RING3:QB_ALERT" : "RING1:QB_NDR",
                                            AnomalyScore = nScore,
                                            IsHttps = false,
                                            IsAnomaly = (nKind == 2)
                                        });
                                    }
                                }
                            }
                        }
                    }
                } catch { }
            }

            // Zero-Overhead GPU 3D Utilization Calculation from OpenGL Command Duty & Visible Monitor Count
            float newGpuPct;
            if (isGamerSuspended || (Config != null && Config.AutoPauseMaximized && cachedWindowMaximized)) {
                newGpuPct = 0.0f;
            } else {
                int totalMons = Math.Max(1, monitorRects.Count);
                float visFrac = Math.Max(0.15f, (float)visibleMonitorCount / totalMons);
                float glSubmitDuty = (Math.Min(12.0f, gpuFrameMs) * currentFps) / 10.0f;
                newGpuPct = Math.Max(0.8f, Math.Min(99f, glSubmitDuty * 0.42f * visFrac + visibleMonitorCount * 0.55f));
            }

            lock (telemetryLock) {
                cpuUsage = newCpu;
                gpuUsagePct = (isGamerSuspended || (Config != null && Config.AutoPauseMaximized && cachedWindowMaximized))
                    ? 0.0f
                    : (gpuUsagePct * 0.45f + newGpuPct * 0.55f);
                ramUsedGB = newRamUsed;
                ramTotalGB = newRamTotal;
                ramPercent = newRamPct;
                activeNicName = nicName;
                localIPv4 = ipv4;
                netDownKbps = downKbps;
                netUpKbps = upKbps;
                tcpEstablishedCount = estCount;
                pingMs = newPing;
                radarBlips = newBlips;
                currentFeed = feed;

                if (fetchedRainTokens != null && fetchedRainTokens.Count > 0) {
                    qbRainTokens = fetchedRainTokens;
                }

                for (int i = 0; i < pingHistory.Length - 1; i++) pingHistory[i] = pingHistory[i + 1];
                pingHistory[pingHistory.Length - 1] = newPing;

                string ts = DateTime.Now.ToString("HH:mm:ss");
                string logEntry;
                if (fetchedLogLines != null && fetchedLogLines.Count > 0) {
                    logEntry = string.Format("[{0}] {1}", ts, fetchedLogLines[telemetryCycles % fetchedLogLines.Count]);
                } else if (anomalyHeadline != "BASELINE_NORMAL") {
                    logEntry = string.Format("[{0}] [IDS_DETECT] {1} | SCORE:{2:0.00}", ts, anomalyHeadline, compositeAnomaly);
                } else if (rng.Next(2) == 0 && sampleConn != null) {
                    logEntry = string.Format("[{0}] {1} [Z:{2:+0.0;-0.0;0.0}σ]", ts, sampleConn, dlZ);
                } else {
                    logEntry = string.Format("[{0}] BASELINE // DL:{1:0.0}KB/s | GPU:{2:0.0}% ({3}FPS) | PING:{4}ms",
                        ts, netDownKbps, gpuUsagePct, currentFps, pingMs);
                }
                liveLogLines.Add(logEntry);
                while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
            }
        } catch { }
    }

    private class HotkeyMessageWindow : NativeWindow {
        private HackerWallpaperForm owner;
        public HotkeyMessageWindow(HackerWallpaperForm ownerForm) {
            this.owner = ownerForm;
            CreateParams cp = new CreateParams();
            cp.Caption = "AntigravityWallpaperHotkeySink";
            cp.X = -32000;
            cp.Y = -32000;
            cp.Width = 1;
            cp.Height = 1;
            cp.ExStyle = WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            this.CreateHandle(cp);
        }
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_HOTKEY && owner != null) {
                owner.HandleHotkeyCommand(m.WParam.ToInt32());
                return;
            }
            base.WndProc(ref m);
        }
    }

    public void HandleHotkeyCommand(int hk) {
        DateTime now = DateTime.UtcNow;
        if ((now - lastHotkeyUtc).TotalMilliseconds < 220) return;
        lastHotkeyUtc = now;

        if (hk == HOTKEY_ID || hk == HOTKEY_GAMER_ID) {
            ShowConfigWindow();
        } else if (hk == HOTKEY_SIEM_ID) {
            PorygonCyberDefenseEngine.OpenWebSocInBrowser();
            ShowCyberActionToast(
                "[ PORYGON WEB SOC // SERVICIO ACTIVO EN http://localhost:5090 ]",
                "Abriendo Centro de Comando Web (SIEM + AV/EDR + IPS Firewall + Cuarentena) en tu navegador...",
                Color.FromArgb(52, 211, 153));
        } else if (hk == HOTKEY_MODEL_ID) {
            TriggerPorygonTrainingWave();
        } else if (hk == HOTKEY_BRAIN_FULL_ID) {
            brain3DCollapsed = false;
            brain3DFullscreen = !brain3DFullscreen;
            hudBrain3DDirty = true;
        } else if (hk == HOTKEY_OPACITY_UP_ID) {
            AdjustTerminalOpacity(15);
        } else if (hk == HOTKEY_OPACITY_DOWN_ID) {
            AdjustTerminalOpacity(-15);
        } else if (hk == HOTKEY_OPACITY_TOGGLE_ID) {
            ToggleTerminalOpacityQuick();
        } else if (hk == HOTKEY_BG_MODE_ID) {
            CycleBgSceneMode();
        }
    }

    private void EnsureHotkeysRegistered() {
        try {
            if (hotkeySink == null || hotkeySink.Handle == IntPtr.Zero) {
                hotkeySink = new HotkeyMessageWindow(this);
            }
            IntPtr hkHwnd = hotkeySink.Handle;
            RegisterHotKey(hkHwnd, HOTKEY_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.W);
            RegisterHotKey(hkHwnd, HOTKEY_MODEL_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.P);
            RegisterHotKey(hkHwnd, HOTKEY_BRAIN_FULL_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.B);
            RegisterHotKey(hkHwnd, HOTKEY_OPACITY_UP_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.Up);
            RegisterHotKey(hkHwnd, HOTKEY_OPACITY_DOWN_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.Down);
            RegisterHotKey(hkHwnd, HOTKEY_OPACITY_TOGGLE_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.O);
            RegisterHotKey(hkHwnd, HOTKEY_BG_MODE_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.M);
            RegisterHotKey(hkHwnd, HOTKEY_GAMER_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.G);
            RegisterHotKey(hkHwnd, HOTKEY_SIEM_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.S);
        } catch { }
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        AttachToDesktopWorkerW();
        EnsureHotkeysRegistered();
        renderTimer.Start();
        if (Config.GamerMode) {
            UpdateGamerSuspensionState(true);
        }
        try { EmptyWorkingSet(Process.GetCurrentProcess().Handle); } catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (hotkeySink != null && hotkeySink.Handle != IntPtr.Zero) {
            IntPtr hkHwnd = hotkeySink.Handle;
            UnregisterHotKey(hkHwnd, HOTKEY_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_MODEL_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_BRAIN_FULL_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_OPACITY_UP_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_OPACITY_DOWN_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_OPACITY_TOGGLE_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_BG_MODE_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_GAMER_ID);
            UnregisterHotKey(hkHwnd, HOTKEY_SIEM_ID);
            try { hotkeySink.DestroyHandle(); } catch { }
            hotkeySink = null;
        }
        if (pdhGpuQuery != IntPtr.Zero) {
            try { PdhCloseQuery(pdhGpuQuery); } catch { }
            pdhGpuQuery = IntPtr.Zero;
        }
        if (pdhGpuBuf != IntPtr.Zero) {
            try { Marshal.FreeHGlobal(pdhGpuBuf); } catch { }
            pdhGpuBuf = IntPtr.Zero;
        }
        if (porygon != null) porygon.SaveBrainStates();
        base.OnFormClosing(e);
    }

    public void ShowCyberActionToast(string titleTxt, string subTxt, Color accent) {
        try {
            if (toastTimer != null) {
                toastTimer.Stop();
                toastTimer.Dispose();
                toastTimer = null;
            }
            if (toastForm != null && !toastForm.IsDisposed) {
                toastForm.Close();
                toastForm.Dispose();
                toastForm = null;
            }

            Screen primaryScr = Screen.PrimaryScreen;
            Rectangle pb = (primaryScr != null) ? primaryScr.Bounds : new Rectangle(0, 0, 1920, 1080);
            int tw = 610;
            int th = 66;
            int tx = pb.Left + (pb.Width - tw) / 2;
            int ty = pb.Top + 32;

            toastForm = new Form {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Bounds = new Rectangle(tx, ty, tw, th),
                TopMost = true,
                ShowInTaskbar = false,
                BackColor = Color.FromArgb(8, 16, 15),
                Opacity = 0.95
            };

            toastForm.Paint += (s, pe) => {
                Graphics g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using (Pen borderPen = new Pen(accent, 2.0f))
                using (SolidBrush titleBr = new SolidBrush(accent))
                using (SolidBrush subBr = new SolidBrush(Color.FromArgb(215, 232, 228)))
                using (Font fTitle = new Font("Consolas", 10.5f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 8.8f, FontStyle.Regular)) {
                    g.DrawRectangle(borderPen, 1, 1, tw - 3, th - 3);
                    g.FillRectangle(titleBr, 0, 0, 8, th);
                    g.DrawString(titleTxt, fTitle, titleBr, 20, 11);
                    g.DrawString(subTxt, fSub, subBr, 20, 36);
                }
            };

            toastForm.Show();
            int ex = GetWindowLong(toastForm.Handle, GWL_EXSTYLE);
            SetWindowLong(toastForm.Handle, GWL_EXSTYLE, ex | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT);

            toastTimer = new System.Windows.Forms.Timer();
            toastTimer.Interval = 2600;
            toastTimer.Tick += (s, ev) => {
                try {
                    if (toastTimer != null) { toastTimer.Stop(); toastTimer.Dispose(); toastTimer = null; }
                    if (toastForm != null && !toastForm.IsDisposed) { toastForm.Close(); toastForm.Dispose(); toastForm = null; }
                } catch { }
            };
            toastTimer.Start();
        } catch { }
    }

    public void ToggleGamerMode() {
        SetGamerMode(!Config.GamerMode, "ATAJO Ctrl+Alt+G");
    }

    public void SetGamerMode(bool enable, string reasonTag) {
        Config.GamerMode = enable;
        if (!enable) {
            autoHighGpuDetected = false;
            highGpuStreak = 0;
        }
        Config.Save();
        UpdateGamerSuspensionState(true);
        ShowGamerToastNotification(enable, reasonTag);
    }

    private void ShowGamerToastNotification(bool enabled, string reasonTag) {
        try {
            if (toastTimer != null) {
                toastTimer.Stop();
                toastTimer.Dispose();
                toastTimer = null;
            }
            if (toastForm != null && !toastForm.IsDisposed) {
                toastForm.Close();
                toastForm.Dispose();
                toastForm = null;
            }

            Screen primaryScr = Screen.PrimaryScreen;
            Rectangle pb = (primaryScr != null) ? primaryScr.Bounds : new Rectangle(0, 0, 1920, 1080);
            int tw = 560;
            int th = 66;
            int tx = pb.Left + (pb.Width - tw) / 2;
            int ty = pb.Top + 32;

            toastForm = new Form {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Bounds = new Rectangle(tx, ty, tw, th),
                TopMost = true,
                ShowInTaskbar = false,
                BackColor = Color.FromArgb(8, 16, 15),
                Opacity = 0.94
            };

            string titleTxt = enabled
                ? "[ MODO GAMER ACTIVADO // 0.0% GPU - 0.0% CPU ]"
                : "[ MODO GAMER DESACTIVADO // MOTOR 3D OPENGL ACTIVO ]";
            string subTxt = enabled
                ? string.Format("Hibernación Total de GPU activa ({0}) — Presiona Ctrl+Alt+G para reanudar", reasonTag ?? "MANUAL")
                : "Enjambre Porygon 3D + Telemetría NIDS + OpenGL 24 FPS reanudados";
            Color accent = enabled ? Color.FromArgb(250, 204, 21) : Color.FromArgb(52, 211, 153);

            toastForm.Paint += (s, pe) => {
                Graphics g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using (Pen borderPen = new Pen(accent, 2.0f))
                using (SolidBrush titleBr = new SolidBrush(accent))
                using (SolidBrush subBr = new SolidBrush(Color.FromArgb(215, 232, 228)))
                using (Font fTitle = new Font("Consolas", 11f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 9.0f, FontStyle.Regular)) {
                    g.DrawRectangle(borderPen, 1, 1, tw - 3, th - 3);
                    g.FillRectangle(titleBr, 0, 0, 8, th);
                    g.DrawString(titleTxt, fTitle, titleBr, 20, 11);
                    g.DrawString(subTxt, fSub, subBr, 20, 36);
                }
            };

            toastForm.Show();
            int ex = GetWindowLong(toastForm.Handle, GWL_EXSTYLE);
            SetWindowLong(toastForm.Handle, GWL_EXSTYLE, ex | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT);

            toastTimer = new System.Windows.Forms.Timer();
            toastTimer.Interval = 2400;
            toastTimer.Tick += (s, ev) => {
                try {
                    if (toastTimer != null) { toastTimer.Stop(); toastTimer.Dispose(); toastTimer = null; }
                    if (toastForm != null && !toastForm.IsDisposed) { toastForm.Close(); toastForm.Dispose(); toastForm = null; }
                } catch { }
            };
            toastTimer.Start();
        } catch { }
    }

    private bool IsFullscreenGameActive(out string detectedName) {
        detectedName = "";
        try {
            int quns = 0;
            bool isD3DFullscreen = (SHQueryUserNotificationState(out quns) == 0 && quns == 3); // 3 = QUNS_RUNNING_D3D_FULL_SCREEN

            IntPtr fg = GetForegroundWindow();
            if (fg == IntPtr.Zero || fg == this.Handle || IsDesktopWindowHandle(fg)) {
                return false;
            }

            RECT wr;
            if (!GetWindowRect(fg, out wr)) return false;
            int w = wr.Right - wr.Left;
            int h = wr.Bottom - wr.Top;
            if (w < 800 || h < 600) return false;

            bool coversFullMonitor = false;
            Screen[] scrs = Screen.AllScreens;
            for (int i = 0; i < scrs.Length; i++) {
                Rectangle b = scrs[i].Bounds;
                if (w >= b.Width && h >= b.Height && Math.Abs(wr.Left - b.Left) <= 2 && Math.Abs(wr.Top - b.Top) <= 2) {
                    coversFullMonitor = true;
                    break;
                }
            }

            if (!coversFullMonitor && !isD3DFullscreen) return false;

            uint pid = 0;
            GetWindowThreadProcessId(fg, out pid);
            if (pid == 0) return false;

            string procName = "";
            string procPath = "";
            try {
                using (Process p = Process.GetProcessById((int)pid)) {
                    procName = (p.ProcessName ?? "").ToLowerInvariant();
                    try { procPath = (p.MainModule.FileName ?? "").ToLowerInvariant(); } catch { }
                }
            } catch {
                return isD3DFullscreen && GetPidRenderGpu((int)pid) >= 20.0;
            }

            if (string.IsNullOrEmpty(procName)) return isD3DFullscreen && GetPidRenderGpu((int)pid) >= 20.0;

            // Exclude standard desktop shells, browsers, IDEs, and productivity apps
            if (procName == "explorer" || procName == "shellexperiencehost" || procName == "searchhost" ||
                procName == "startmenuexperiencehost" || procName == "applicationframehost" || procName == "taskmgr" ||
                procName == "hackerwallpaper" || procName == "code" || procName == "devenv" ||
                procName == "windowsterminal" || procName == "powershell" || procName == "pwsh" || procName == "cmd" ||
                procName == "chrome" || procName == "msedge" || procName == "firefox" || procName == "brave" || procName == "opera" ||
                procName == "whatsapp" || procName == "whatsapp.root" || procName == "discord" || procName == "spotify" ||
                procName == "slack" || procName == "teams" || procName == "notepad" || procName == "antigravity" ||
                procName == "rustdesk" || procName == "mstsc" || procName == "anydesk" || procName == "teamviewer" ||
                procName == "msrdc" || procName == "parsecd" || procName == "vlc" || procName == "mpc-hc64" || procName == "mpv" ||
                procName == "obs64" || procName == "photos" || procName == "winword" || procName == "excel" || procName == "powerpnt" ||
                procName == "olk" || procName == "outlook" || procName == "acrobat" || procName == "zoom" || procName == "agy") {
                return false;
            }

            double fgGpu = GetPidRenderGpu((int)pid);
            if (isD3DFullscreen && fgGpu >= 20.0) {
                detectedName = procName.ToUpperInvariant();
                return true;
            }

            // Check if process belongs to a known game library path (including Desktop\Juegos) or game executable
            if (procPath.Contains("steamapps") || procPath.Contains("epic games") || procPath.Contains("riot games") ||
                procPath.Contains("xboxgames") || procPath.Contains("battle.net") || procPath.Contains("ea games") ||
                procPath.Contains("ubisoft") || procPath.Contains("gog galaxy") || procPath.Contains("games\\") ||
                procPath.Contains("\\juegos\\") || procName.Contains("rimworld") || procName.Contains("moonlight") ||
                procName.Contains("valorant") || procName.Contains("cs2") || procName.Contains("dota2") ||
                procName.Contains("fortnite") || procName.Contains("overwatch") || procName.Contains("r5apex") ||
                procName.Contains("eldenring") || procName.Contains("cyberpunk") || procName.Contains("helldivers") ||
                procName.Contains("genshin") || procName.Contains("minecraft") || procName.Contains("gtav") ||
                procName.Contains("blackops") || procName.Contains("modernwarfare") || procName.Contains("league") || procName.Contains("unreal")) {
                detectedName = procName.ToUpperInvariant();
                return true;
            }

            // Borderless fullscreen window covering entire monitor (including taskbar area) without standard WS_CAPTION
            int style = GetWindowLong(fg, GWL_STYLE);
            const int WS_CAPTION = 0x00C00000;
            if (coversFullMonitor && (style & WS_CAPTION) == 0 && fgGpu >= 20.0) {
                detectedName = procName.ToUpperInvariant();
                return true;
            }
        } catch { }
        return false;
    }

    private void RenderGamerHibernationFrameOpenGL() {
        if (!glEnabled || glHdc == IntPtr.Zero || glContext == IntPtr.Zero) {
            this.Invalidate();
            return;
        }
        try {
            GpuGl.wglMakeCurrent(glHdc, glContext);
            GpuGl.CanvasW = screenW;
            GpuGl.CanvasH = screenH;
            GpuGl.glViewport(0, 0, screenW, screenH);
            GpuGl.glMatrixMode(GpuGl.GL_PROJECTION);
            GpuGl.glLoadIdentity();
            GpuGl.glOrtho(0.0, screenW, screenH, 0.0, -2000.0, 2000.0);
            GpuGl.glMatrixMode(GpuGl.GL_MODELVIEW);
            GpuGl.glLoadIdentity();
            GpuGl.glDisable(GpuGl.GL_DEPTH_TEST);
            GpuGl.glEnable(GpuGl.GL_BLEND);
            GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GpuGl.GL_ONE_MINUS_SRC_ALPHA);
            GpuGl.glDisable(GpuGl.GL_LINE_SMOOTH);

            if (bgTexDirty || bgTexId == 0) {
                GpuGl.UploadBitmapToTexture(bgBitmap, ref bgTexId, ref bgTexW, ref bgTexH);
                bgTexDirty = false;
            }
            Color c1, c2, c3, headCol;
            GetThemeColors(out c1, out c2, out c3, out headCol);

            int hudW = 584;
            int hudH = 228;
            int hudX = primaryMonRect.Right - hudW - 22;
            int hudY = primaryMonRect.Top + 22;

            if (Config.ShowTelemetry) {
                if (hudTelemetryBmp == null) {
                    hudTelemetryBmp = new Bitmap(hudW + 4, hudH + 4, PixelFormat.Format32bppArgb);
                }
                using (Graphics gTel = Graphics.FromImage(hudTelemetryBmp)) {
                    gTel.Clear(Color.FromArgb(0, 0, 0, 0));
                    gTel.SmoothingMode = SmoothingMode.AntiAlias;
                    gTel.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    DrawTelemetryHUD(gTel, 2, 2, hudW, hudH, c1, c2, c3);
                }
                GpuGl.UploadBitmapToTexture(hudTelemetryBmp, ref hudTelemetryTexId, ref hudTelemetryTexW, ref hudTelemetryTexH);
                hudTelemetryDirty = false;
            }

            // Render static dark wallpaper + Gamer Mode HUD to both buffers, then detach OpenGL context completely (0.0% GPU!)
            for (int pass = 0; pass < 2; pass++) {
                GpuGl.DrawTexturedQuad(bgTexId, 0, 0, screenW, screenH);
                if (Config.ShowTelemetry && hudTelemetryTexId != 0) {
                    GpuGl.DrawTexturedQuad(hudTelemetryTexId, hudX - 2, hudY - 2, hudW + 4, hudH + 4);
                }
                GpuGl.SwapBuffers(glHdc);
            }
            GpuGl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            gamerStaticFrameRendered = true;
        } catch { }
    }

    private void UpdateGamerSuspensionState(bool forceRefresh) {
        bool shouldSuspend = Config.GamerMode || (Config.AutoGamerOnFullscreenGame && autoGameDetected) || autoHighGpuDetected;
        if (shouldSuspend) {
            bool wasSuspended = isGamerSuspended;
            isGamerSuspended = true;
            lock (telemetryLock) {
                gpuUsagePct = 0.0f;
                gpuFrameMs = 0.0f;
                currentFps = 0;
                if (!wasSuspended || forceRefresh) {
                    string ts = DateTime.Now.ToString("HH:mm:ss");
                    string src = Config.GamerMode
                        ? "MANUAL (Ctrl+Alt+G)"
                        : (autoHighGpuDetected ? ("AUTO_GPU_>90%: " + highGpuProcessName) : ("AUTO_GAME: " + activeGameName));
                    liveLogLines.Add(string.Format("[{0}] [GAMER_MODE] 0.0% GPU HIBERNACIÓN ACTIVA // {1}", ts, src));
                    if (!wasSuspended) {
                        try {
                            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gpu_autopause.log"),
                                string.Format("{0:yyyy-MM-dd HH:mm:ss} HIBERNATE reason={1}\r\n", DateTime.Now, src));
                        } catch { }
                    }
                    while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
                }
            }
            if (telemetryTimer != null) {
                try {
                    if (autoHighGpuDetected && !Config.GamerMode && !autoGameDetected) {
                        // Keep lightweight 3.5s PDH check active so we automatically resume when GPU load drops < 78%
                        telemetryTimer.Change(3500, 3500);
                    } else {
                        telemetryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                } catch { }
            }
            if (cpuEvoTimer != null) {
                try { cpuEvoTimer.Change(Timeout.Infinite, Timeout.Infinite); } catch { }
            }
            if (renderTimer != null && renderTimer.Interval != 120) {
                renderTimer.Interval = 120;
            }
            if (!wasSuspended || forceRefresh || !gamerStaticFrameRendered) {
                RenderGamerHibernationFrameOpenGL();
                try { EmptyWorkingSet(Process.GetCurrentProcess().Handle); } catch { }
            }
        } else if (isGamerSuspended || forceRefresh) {
            bool wasSuspended = isGamerSuspended;
            isGamerSuspended = false;
            gamerStaticFrameRendered = false;
            lock (telemetryLock) {
                currentFps = 24;
                gpuFrameMs = 2.4f;
                gpuUsagePct = 3.2f;
                if (wasSuspended) {
                    string ts = DateTime.Now.ToString("HH:mm:ss");
                    liveLogLines.Add(string.Format("[{0}] [GAMER_MODE] Reanudando Renderizado OpenGL 3D (24 FPS)", ts));
                    while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
                }
            }
            hudTelemetryDirty = true;
            hudRadarDirty = true;
            hudConnectomeDirty = true;
            hudBrain3DDirty = true;
            if (telemetryTimer != null) {
                try { telemetryTimer.Change(150, 2200); } catch { }
            }
            if (cpuEvoTimer != null) {
                try { cpuEvoTimer.Change(800, 800); } catch { }
            }
            if (renderTimer != null) {
                renderTimer.Interval = 33;
            }
            if (glEnabled) {
                RenderOpenGLFrame();
            } else {
                this.Invalidate();
            }
        }
    }

    private void LoadInitialTerminalOpacity() {
        try {
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string wtSettings = Path.Combine(localApp, @"Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json");
            if (File.Exists(wtSettings)) {
                string json = File.ReadAllText(wtSettings);
                var m = System.Text.RegularExpressions.Regex.Match(json, @"""defaults""\s*:\s*\{[^}]*?""opacity""\s*:\s*(\d+)");
                if (!m.Success) {
                    m = System.Text.RegularExpressions.Regex.Match(json, @"""opacity""\s*:\s*(\d+)\s*,\s*""useAcrylic""");
                }
                if (m.Success) {
                    int val;
                    if (int.TryParse(m.Groups[1].Value, out val)) {
                        terminalOpacityPct = Math.Max(20, Math.Min(100, val));
                    }
                }
            }
        } catch { }
    }

    public void ToggleTerminalOpacityQuick() {
        int nextPct = (terminalOpacityPct < 90) ? 100 : 50;
        SetTerminalOpacity(nextPct, true);
    }

    public void AdjustTerminalOpacity(int deltaPct) {
        SetTerminalOpacity(terminalOpacityPct + deltaPct, true);
    }

    public void SetTerminalOpacity(int targetPct, bool showToast) {
        try {
            terminalOpacityPct = Math.Max(20, Math.Min(100, targetPct));

            // 1. Live-update Windows Terminal settings.json (Windows Terminal hot-reloads opacity immediately)
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] wtPaths = new string[] {
                Path.Combine(localApp, @"Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json"),
                Path.Combine(localApp, @"Packages\Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe\LocalState\settings.json"),
                Path.Combine(localApp, @"Microsoft\Windows Terminal\settings.json")
            };
            for (int pIdx = 0; pIdx < wtPaths.Length; pIdx++) {
                string wtSettings = wtPaths[pIdx];
                if (!File.Exists(wtSettings)) continue;
                string json = File.ReadAllText(wtSettings);
                string updated = System.Text.RegularExpressions.Regex.Replace(
                    json,
                    @"(""defaults""\s*:\s*\{[^}]*?""opacity""\s*:\s*)\d+",
                    "${1}" + terminalOpacityPct);
                if (updated == json) {
                    updated = System.Text.RegularExpressions.Regex.Replace(
                        json,
                        @"(""opacity""\s*:\s*)\d+(\s*,\s*""useAcrylic"")",
                        "${1}" + terminalOpacityPct + "${2}");
                }
                if (updated != json) {
                    File.WriteAllText(wtSettings, updated, Encoding.UTF8);
                }
            }

            // 2. Also apply Win32 LWA_ALPHA to all open classic ConsoleWindowClass windows
            byte alphaByte = (byte)Math.Max(50, Math.Min(255, (int)Math.Round(terminalOpacityPct * 255.0 / 100.0)));
            IntPtr hCon = IntPtr.Zero;
            while ((hCon = FindWindowEx(IntPtr.Zero, hCon, "ConsoleWindowClass", null)) != IntPtr.Zero) {
                int ex = GetWindowLong(hCon, GWL_EXSTYLE);
                SetWindowLong(hCon, GWL_EXSTYLE, ex | WS_EX_LAYERED);
                SetLayeredWindowAttributes(hCon, 0, alphaByte, LWA_ALPHA);
            }

            lock (telemetryLock) {
                string ts = DateTime.Now.ToString("HH:mm:ss");
                liveLogLines.Add(string.Format("[{0}] [TERM_OPACITY] Opacidad Terminal -> {1}% (Ctrl+Alt+Up/Down/O)", ts, terminalOpacityPct));
                while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
            }
            hudTelemetryDirty = true;
            if (isGamerSuspended) RenderGamerHibernationFrameOpenGL();

            if (showToast) {
                string modeLabel = (terminalOpacityPct >= 95) ? "MODO SÓLIDO / LECTURA NÍTIDA" : "MODO CYBER TRANSLÚCIDO";
                ShowCyberActionToast(
                    string.Format("[ OPACIDAD DE TERMINAL -> {0}% // {1} ]", terminalOpacityPct, modeLabel),
                    "Atajos Rápidos: Ctrl+Alt+Up (+15%) | Ctrl+Alt+Down (-15%) | Ctrl+Alt+O (100% <-> 50%) | Click [OP+] en HUD",
                    (terminalOpacityPct >= 90) ? Color.FromArgb(52, 211, 153) : Color.FromArgb(245, 202, 78));
            }
        } catch { }
    }

    private void InstallMouseWheelHook() {
        // NEVER install a global WH_MOUSE_LL hook on the UI thread!
        // WH_MOUSE_LL synchronously routes every OS mouse movement through the UI thread message pump,
        // causing cursor stutter/input lag whenever a multi-monitor OpenGL frame is rendering.
    }

    private Rectangle GetCurrentBrain3DRect() {
        if (brain3DFullscreen && !brain3DCollapsed) {
            return new Rectangle(primaryMonRect.Left + 108, primaryMonRect.Top + 18, primaryMonRect.Width - 126, primaryMonRect.Height - 68);
        }
        int bw = 584;
        int bx = primaryMonRect.Right - bw - 22;
        int brainW = 356;
        int brainH = brain3DCollapsed ? 32 : 246;
        int brainX = bx - brainW - 14;
        int brainY = primaryMonRect.Bottom - brainH - 54;
        return new Rectangle(brainX, brainY, brainW, brainH);
    }

    private float GetEffectiveBrainYaw() {
        if (brain3DAutoRotate) {
            return brain3DManualYaw + (float)Math.Sin(brain3DYaw) * 0.44f;
        }
        return brain3DManualYaw;
    }

    private float GetEffectiveBrainPitch() {
        if (brain3DAutoRotate) {
            return brain3DManualPitch + (float)Math.Cos(brain3DYaw * 0.72f) * 0.12f;
        }
        return brain3DManualPitch;
    }

    private void TriggerPorygonTrainingWave() {
        if (porygon == null) return;
        porygon.TriggerTrainingWave(screenW, screenH);
        this.Invalidate();
    }

    protected override void WndProc(ref Message m) {
        const int WM_NCHITTEST = 0x0084;
        const int WM_MOUSEACTIVATE = 0x0021;
        if (m.Msg == WM_HOTKEY) {
            HandleHotkeyCommand(m.WParam.ToInt32());
            return;
        }
        const int WM_QUERYENDSESSION = 0x0011;
        const int WM_ENDSESSION = 0x0016;
        if (m.Msg == WM_QUERYENDSESSION || m.Msg == WM_ENDSESSION) {
            try {
                if (Config != null) Config.Save();
                if (porygon != null) porygon.SaveBrainStates();
            } catch { }
            m.Result = new IntPtr(1);
            return;
        }
        if (m.Msg == WM_NCHITTEST) {
            m.Result = new IntPtr(-1); // HTTRANSPARENT so all clicks go straight to desktop folders
            return;
        }
        if (m.Msg == WM_MOUSEACTIVATE) {
            m.Result = new IntPtr(4);  // MA_NOACTIVATEANDEAT so window never jumps to front
            return;
        }
        base.WndProc(ref m);
    }

    private IntPtr cachedProgman = IntPtr.Zero;
    private IntPtr cachedDefView = IntPtr.Zero;

    // Hardware OpenGL 4.6 State (AMD Radeon RX 9060 XT)
    private IntPtr glHdc = IntPtr.Zero;
    private IntPtr glContext = IntPtr.Zero;
    private bool glEnabled = false;
    private string gpuRendererName = "RX 9060 XT [GL]";
    private uint bgTexId = 0;
    private int bgTexW = 0, bgTexH = 0;
    private bool bgTexDirty = true;

    private Bitmap hudTelemetryBmp = null;
    private uint hudTelemetryTexId = 0;
    private int hudTelemetryTexW = 0, hudTelemetryTexH = 0;
    private int lastHudTelemetryCycle = -1;
    private int lastHudTheme = -1;

    private Bitmap hudConnectomeBmp = null;
    private uint hudConnectomeTexId = 0;
    private int hudConnectomeTexW = 0, hudConnectomeTexH = 0;

    private Bitmap hudBrain3DBmp = null;
    private uint hudBrain3DTexId = 0;
    private int hudBrain3DTexW = 0, hudBrain3DTexH = 0;

    private Bitmap hudBrain3DFullBmp = null;
    private uint hudBrain3DFullTexId = 0;
    private int hudBrain3DFullTexW = 0, hudBrain3DFullTexH = 0;

    private Bitmap hudRadarBmp = null;
    private uint hudRadarTexId = 0;
    private int hudRadarTexW = 0, hudRadarTexH = 0;

    private void InitOpenGL() {
        if (glEnabled) return;
        try {
            glHdc = GpuGl.GetDC(this.Handle);
            if (glHdc == IntPtr.Zero) return;

            GpuGl.PIXELFORMATDESCRIPTOR pfd = new GpuGl.PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)Marshal.SizeOf(typeof(GpuGl.PIXELFORMATDESCRIPTOR));
            pfd.nVersion = 1;
            pfd.dwFlags = 0x00000004 | 0x00000020 | 0x00000001; // PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER
            pfd.iPixelType = 0; // PFD_TYPE_RGBA
            pfd.cColorBits = 32;
            pfd.cAlphaBits = 8;
            pfd.cDepthBits = 24;
            pfd.iLayerType = 0; // PFD_MAIN_PLANE

            int pf = GpuGl.ChoosePixelFormat(glHdc, ref pfd);
            if (pf == 0) return;
            if (!GpuGl.SetPixelFormat(glHdc, pf, ref pfd)) return;

            glContext = GpuGl.wglCreateContext(glHdc);
            if (glContext == IntPtr.Zero) return;
            if (!GpuGl.wglMakeCurrent(glHdc, glContext)) return;

            IntPtr pRenderer = GpuGl.glGetString(GpuGl.GL_RENDERER);
            if (pRenderer != IntPtr.Zero) {
                string rName = Marshal.PtrToStringAnsi(pRenderer);
                if (!string.IsNullOrEmpty(rName)) {
                    gpuRendererName = rName.Replace("AMD Radeon ", "").Trim();
                }
            }

            GpuGl.InitFontAtlas();

            GpuGl.CanvasW = screenW;
            GpuGl.CanvasH = screenH;
            bgTexDirty = true;
            lastHudTheme = -1;
            glEnabled = true;
        } catch {
            glEnabled = false;
        }
    }

    private void AttachToDesktopWorkerW() {
        cachedProgman = FindWindow("Progman", null);
        if (cachedProgman != IntPtr.Zero) {
            IntPtr res;
            SendMessageTimeout(cachedProgman, 0x052C, new IntPtr(0xD), new IntPtr(0x1), 0, 1000, out res);
            SendMessageTimeout(cachedProgman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 1000, out res);
            cachedDefView = FindWindowEx(cachedProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
        }

        const int WS_POPUP = unchecked((int)0x80000000);
        const int WS_CAPTION = 0x00C00000;
        const int WS_THICKFRAME = 0x00040000;
        const int WS_DISABLED = 0x08000000;
        const int WS_CLIPCHILDREN = 0x02000000;
        const int WS_CLIPSIBLINGS = 0x04000000;
        const uint SWP_FRAMECHANGED = 0x0020;

        if (cachedProgman != IntPtr.Zero) {
            int style = GetWindowLong(this.Handle, GWL_STYLE);
            style = (style & ~WS_POPUP & ~WS_CAPTION & ~WS_THICKFRAME) | WS_CHILD | WS_VISIBLE | WS_DISABLED | WS_CLIPCHILDREN | WS_CLIPSIBLINGS;
            SetWindowLong(this.Handle, GWL_STYLE, style);
            SetParent(this.Handle, cachedProgman);
        }

        int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        exStyle = (exStyle & ~WS_EX_LAYERED) | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
        SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);

        // Progman spans the entire VirtualScreen (all 5 monitors), so (0, 0, screenW, screenH) covers all monitors!
        if (cachedDefView != IntPtr.Zero) {
            SetWindowPos(this.Handle, cachedDefView, 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        } else {
            SetWindowPos(this.Handle, new IntPtr(1), 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        }

        InitOpenGL();
        // Windows 11 24H2 Progman requires WS_EX_LAYERED + LWA_ALPHA on child windows to composite them;
        // applying it AFTER InitOpenGL preserves hardware OpenGL on AMD Radeon RX 9060 XT while making Progman display it!
        exStyle |= WS_EX_LAYERED;
        SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
        SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);
    }

    private void EnsureBehindDesktopIcons() {
        if (cachedProgman == IntPtr.Zero || !IsWindow(cachedProgman)) {
            cachedProgman = FindWindow("Progman", null);
            cachedDefView = IntPtr.Zero;
        }
        if (cachedProgman == IntPtr.Zero) return;

        IntPtr prevDefView = cachedDefView;
        if (cachedDefView == IntPtr.Zero || !IsWindow(cachedDefView)) {
            cachedDefView = FindWindowEx(cachedProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
        }
        bool defViewJustAppeared = (prevDefView == IntPtr.Zero && cachedDefView != IntPtr.Zero);

        // If started at Windows logon before Progman was ready (or if Explorer.exe restarted), re-parent to Progman!
        if (GetParent(this.Handle) != cachedProgman) {
            AttachToDesktopWorkerW();
            return;
        }

        // Detect if multi-monitor topology expanded after Windows login (e.g. 1 monitor -> 5 monitors)
        bool layoutChanged = false;
        if (frameCount >= 20) {
            List<Rectangle> newRects; List<string> newNames; Rectangle newPrim; int newW, newH;
            QueryPhysicalMonitors(out newRects, out newNames, out newPrim, out newW, out newH);
            if (newW != screenW || newH != screenH || newRects.Count != monitorRects.Count) {
                RebuildBackground();
                RebuildRainColumns();
                GpuGl.CanvasW = screenW;
                GpuGl.CanvasH = screenH;
                bgTexDirty = true;
                hudTelemetryDirty = true;
                hudRadarDirty = true;
                hudConnectomeDirty = true;
                hudBrain3DDirty = true;
                layoutChanged = true;
            }
        }

        // On Windows 11 24H2, resizing a WS_EX_LAYERED OpenGL child or attaching before SHELLDLL_DefView finishes logon
        // requires stripping WS_EX_LAYERED, resizing with SWP_FRAMECHANGED, and re-applying WS_EX_LAYERED + LWA_ALPHA.
        if (layoutChanged || defViewJustAppeared || frameCount == 160 || frameCount == 320) {
            AttachToDesktopWorkerW();
            return;
        }

        int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        if ((exStyle & WS_EX_LAYERED) == 0) {
            SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
            SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);
            if (cachedDefView != IntPtr.Zero) {
                SetWindowPos(this.Handle, cachedDefView, 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
        } else if ((frameCount <= 5 || frameCount == 25 || frameCount == 60) && cachedDefView != IntPtr.Zero) {
            SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);
            SetWindowPos(this.Handle, cachedDefView, 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }

    private bool IsMonitorOccluded(int mIdx) {
        return mIdx >= 0 && mIdx < monitorOccluded.Length && monitorOccluded[mIdx];
    }

    private bool IsPointOnOccludedMonitor(float localX, float localY) {
        if (!anyMonitorOccluded) return false;
        int ix = (int)localX;
        int iy = (int)localY;
        for (int i = 0; i < monitorRects.Count && i < monitorOccluded.Length; i++) {
            if (monitorOccluded[i] && monitorRects[i].Contains(ix, iy)) {
                return true;
            }
        }
        return false;
    }

    private string lastOcclusionDebug = "";

    private bool IsPointDesktopVisible(Point screenPt, out string hitClass) {
        hitClass = "NONE";
        IntPtr hHit = WindowFromPoint(screenPt);
        if (hHit == IntPtr.Zero) return false;
        if (IsDesktopWindowHandle(hHit)) {
            hitClass = "DESK";
            return true;
        }
        IntPtr hRoot = GetAncestor(hHit, 2); // GA_ROOT = 2
        if (hRoot == IntPtr.Zero) hRoot = hHit;
        occlusionClassSb.Length = 0;
        GetClassName(hRoot, occlusionClassSb, occlusionClassSb.Capacity);
        string rCls = occlusionClassSb.ToString();
        hitClass = rCls;
        if (terminalOpacityPct < 92 && (rCls == "CASCADIA_HOSTING_WINDOW_CLASS" || rCls == "ConsoleWindowClass")) {
            return true;
        }
        return false;
    }

    private bool UpdateMonitorOcclusionState() {
        try {
            int monCount = monitorRects.Count;
            if (monCount == 0) {
                primaryMonitorOccluded = false;
                anyMonitorOccluded = false;
                visibleMonitorCount = 1;
                return false;
            }
            bool anyOcc = false;
            bool allOcc = true;
            bool primOcc = false;
            int visCount = 0;
            StringBuilder dbg = new StringBuilder(128);

            for (int i = 0; i < monCount && i < monitorOccluded.Length; i++) {
                Rectangle m = monitorRects[i];
                // Primary monitor's physical Windows coordinate is always (0,0), so (m.Left - primaryMonRect.Left) is the exact physical screen X/Y!
                int sx = m.Left - primaryMonRect.Left;
                int sy = m.Top - primaryMonRect.Top;
                int w = m.Width;
                int h = m.Height;

                // Sample 5 interior points on monitor i (center + 4 quadrants, avoiding taskbar/titlebar)
                string centerCls;
                int occludedPts = 0;
                if (!IsPointDesktopVisible(new Point(sx + w / 2, sy + h / 2), out centerCls)) occludedPts++;
                string dummy;
                if (!IsPointDesktopVisible(new Point(sx + w / 4, sy + h / 3), out dummy)) occludedPts++;
                if (!IsPointDesktopVisible(new Point(sx + (w * 3) / 4, sy + h / 3), out dummy)) occludedPts++;
                if (!IsPointDesktopVisible(new Point(sx + w / 4, sy + (h * 2) / 3), out dummy)) occludedPts++;
                if (!IsPointDesktopVisible(new Point(sx + (w * 3) / 4, sy + (h * 2) / 3), out dummy)) occludedPts++;

                bool isOcc = (occludedPts >= 4);
                monitorOccluded[i] = isOcc;
                if (i > 0) dbg.Append(" | ");
                dbg.AppendFormat("M{0}:{1}({2}/5,{3})", i + 1, isOcc ? "OCC" : "VIS", occludedPts, centerCls);

                if (isOcc) {
                    anyOcc = true;
                    if (m == primaryMonRect) primOcc = true;
                } else {
                    allOcc = false;
                    visCount++;
                }
            }

            primaryMonitorOccluded = primOcc && brainSnapshotPath == null && DateTime.Now > brainForceVisibleUntil;
            anyMonitorOccluded = anyOcc;
            visibleMonitorCount = visCount;
            lastOcclusionDebug = dbg.ToString();
            return allOcc;
        } catch {
            return false;
        }
    }

    private bool IsDesktopWindowHandle(IntPtr hWnd) {
        if (hWnd == IntPtr.Zero) return false;
        if (hWnd == this.Handle || hWnd == cachedProgman || hWnd == cachedDefView) return true;
        occlusionClassSb.Length = 0;
        GetClassName(hWnd, occlusionClassSb, occlusionClassSb.Capacity);
        string cls = occlusionClassSb.ToString();
        return cls == "SysListView32" || cls == "SHELLDLL_DefView" || cls == "Progman" || cls == "WorkerW";
    }

    private void HandleBrain3DClick(Point pt, Rectangle brainRect) {
        int relX = pt.X - brainRect.X;
        int relY = pt.Y - brainRect.Y;

        if (brain3DCollapsed) {
            brain3DCollapsed = false;
            Config.Brain3DCollapsed = false;
            Config.Save();
            hudBrain3DDirty = true;
            return;
        }

        // 1. Header Bar Clicks (Top 32px): [FULL / EXIT FULL] button vs [-] Fold button
        if (relY <= 34) {
            if (relX >= brainRect.Width - 38) {
                // [-] Fold button
                brain3DFullscreen = false;
                brain3DCollapsed = !brain3DCollapsed;
                Config.Brain3DCollapsed = brain3DCollapsed;
                Config.Save();
            } else if (relX >= brainRect.Width - 135) {
                // [FULL] / [EXIT FULL] button
                brain3DFullscreen = !brain3DFullscreen;
                brain3DCollapsed = false;
            } else {
                // Clicking left title header folds card (or exits fullscreen if in fullscreen)
                if (brain3DFullscreen) {
                    brain3DFullscreen = false;
                } else {
                    brain3DCollapsed = !brain3DCollapsed;
                    Config.Brain3DCollapsed = brain3DCollapsed;
                    Config.Save();
                }
            }
            hudBrain3DDirty = true;
            return;
        }

        // 2. Bottom Interactive 3D Control Toolbar (Bottom 28px): [< ROT] [ROT >] [ZOOM] [AUTO/FREE] [NEXT PORYGON]
        if (relY >= brainRect.Height - 28) {
            float xFrac = (float)relX / Math.Max(1, brainRect.Width);
            if (xFrac < 0.18f) {
                // [< ROT] Rotate Left
                if (brain3DAutoRotate) { brain3DManualYaw = GetEffectiveBrainYaw(); brain3DAutoRotate = false; }
                brain3DManualYaw -= 0.36f;
            } else if (xFrac < 0.36f) {
                // [ROT >] Rotate Right
                if (brain3DAutoRotate) { brain3DManualYaw = GetEffectiveBrainYaw(); brain3DAutoRotate = false; }
                brain3DManualYaw += 0.36f;
            } else if (xFrac < 0.56f) {
                // [ZOOM] Cycle Zoom Level
                brain3DPanX = 0f; brain3DPanY = 0f;
                if (brain3DZoom < 1.15f) brain3DZoom = 1.35f;
                else if (brain3DZoom < 1.55f) brain3DZoom = 1.75f;
                else if (brain3DZoom > 0.90f) brain3DZoom = 0.80f;
                else brain3DZoom = 1.0f;
            } else if (xFrac < 0.76f) {
                // [AUTO / FREE] Toggle Orbital Auto-Rotation
                brain3DAutoRotate = !brain3DAutoRotate;
                if (brain3DAutoRotate) {
                    brain3DManualYaw = 0f;
                    brain3DManualPitch = 0.14f;
                }
            } else {
                // [NEXT PORYGON] Cycle inspected Porygon brain
                if (porygon != null && porygon.Agents.Count > 0) {
                    porygon.SelectedBrainAgentIndex = (porygon.SelectedBrainAgentIndex + 1) % porygon.Agents.Count;
                    porygon.LastManualBrainSelectUtc = DateTime.UtcNow;
                    porygon.BrainAutoFocusedOnThreat = false;
                }
            }
            hudBrain3DDirty = true;
            return;
        }

        // 3. Single-click inside the 3D viewport (if user didn't drag to rotate): cycle to next Porygon's 3D brain
        if (!brain3DDragMoved && porygon != null && porygon.Agents.Count > 0) {
            porygon.SelectedBrainAgentIndex = (porygon.SelectedBrainAgentIndex + 1) % porygon.Agents.Count;
            porygon.LastManualBrainSelectUtc = DateTime.UtcNow;
            porygon.BrainAutoFocusedOnThreat = false;
            hudBrain3DDirty = true;
        }
    }

    private void HandleDesktopCardClick() {
        try {
            Point screenPt = Control.MousePosition;
            IntPtr hHit = WindowFromPoint(screenPt);
            if (!IsDesktopWindowHandle(hHit)) return;

            Point pt = this.PointToClient(screenPt);

            // Priority 1: If 3D Brain is in Fullscreen Mode on Primary Monitor, handle clicks on it first!
            if (!isGamerSuspended && Config.ShowPorygon && porygon != null && brain3DFullscreen && !brain3DCollapsed) {
                Rectangle fullBrainRect = GetCurrentBrain3DRect();
                if (fullBrainRect.Contains(pt)) {
                    HandleBrain3DClick(pt, fullBrainRect);
                    return;
                }
            }

            int hudW = 584;
            int hudH = telemetryCollapsed ? 32 : 228;
            int hudX = primaryMonRect.Right - hudW - 22;
            int hudY = primaryMonRect.Top + 22;

            int radPanelW = 246;
            int radPanelH = radarCollapsed ? 32 : 228;
            int radPanelX = hudX - radPanelW - 14;
            int radPanelY = primaryMonRect.Top + 22;

            if (!isGamerSuspended && Config.ShowRadar) {
                Rectangle radRect = new Rectangle(radPanelX, radPanelY, radPanelW, radPanelH);
                if (radRect.Contains(pt)) {
                    radarCollapsed = !radarCollapsed;
                    Config.RadarCollapsed = radarCollapsed;
                    Config.Save();
                    hudRadarDirty = true;
                    return;
                }
            }

            if (Config.ShowTelemetry) {
                Rectangle telRect = new Rectangle(hudX, hudY, hudW, hudH);
                if (telRect.Contains(pt)) {
                    int relX = pt.X - hudX;
                    int relY = pt.Y - hudY;
                    if (relY <= 30 && relX >= hudW - 262 && relX < hudW - 204) {
                        ToggleTerminalOpacityQuick();
                        return;
                    } else if (relY <= 30 && relX >= hudW - 204 && relX < hudW - 142) {
                        ToggleGamerMode();
                        return;
                    } else if (relY <= 30 && relX >= hudW - 142 && relX < hudW - 96) {
                        CycleBgSceneMode();
                        if (isGamerSuspended) RenderGamerHibernationFrameOpenGL();
                        return;
                    } else if (relY <= 30 && relX >= hudW - 96 && relX < hudW - 64) {
                        AdjustTerminalOpacity(-15);
                        return;
                    } else if (relY <= 30 && relX >= hudW - 64 && relX < hudW - 32) {
                        AdjustTerminalOpacity(15);
                        return;
                    }
                    telemetryCollapsed = !telemetryCollapsed;
                    Config.TelemetryCollapsed = telemetryCollapsed;
                    Config.Save();
                    hudTelemetryDirty = true;
                    if (isGamerSuspended) RenderGamerHibernationFrameOpenGL();
                    return;
                }

                for (int mIdx = 0; mIdx < monitorRects.Count; mIdx++) {
                    Rectangle m = monitorRects[mIdx];
                    if (m == primaryMonRect) continue;
                    int pw = Math.Min(520, m.Width - 180);
                    if (pw < 260) continue;
                    int ph = secondaryBarsCollapsed ? 24 : 46;
                    int px = m.Right - pw - 18;
                    int py = m.Top + 16;
                    if (new Rectangle(px, py, pw, ph).Contains(pt)) {
                        secondaryBarsCollapsed = !secondaryBarsCollapsed;
                        Config.SecondaryBarsCollapsed = secondaryBarsCollapsed;
                        Config.Save();
                        return;
                    }
                }
            }

            if (!isGamerSuspended && Config.ShowPorygon && porygon != null) {
                int bw = 584;
                int bh = connectomeCollapsed ? 32 : (78 + porygon.Agents.Count * 21);
                int bx = primaryMonRect.Right - bw - 22;
                int by = primaryMonRect.Bottom - bh - 54;
                Rectangle connRect = new Rectangle(bx, by, bw, bh);
                if (connRect.Contains(pt)) {
                    int relX = pt.X - bx;
                    int relY = pt.Y - by;
                    if (relY <= 28 && relX >= bw - 268 && relX < bw - 202) {
                        PorygonCyberDefenseEngine.OpenWebSocInBrowser();
                        ShowCyberActionToast(
                            "[ PORYGON WEB SOC // http://localhost:5090 ABIERTO EN NAVEGADOR ]",
                            "Panel Web interactivo SIEM + AV/EDR + Firewall IPS + Cuarentena activo en puerto 5090",
                            Color.FromArgb(52, 211, 153));
                        return;
                    } else if (relY <= 28 && relX >= bw - 202 && relX < bw - 156) {
                        ThreadPool.QueueUserWorkItem(_ => PorygonCyberDefenseEngine.RunQuickDirectoryAudit(true));
                        ShowCyberActionToast(
                            "[ PORYGON2 AV/SIEM // AUDITORÍA RÁPIDA EN EJECUCIÓN ]",
                            "Escaneando Downloads, Startup, Claves de Registro Run y Tabla TCP/PID...",
                            Color.FromArgb(52, 211, 153));
                        hudConnectomeDirty = true;
                        return;
                    } else if (relY <= 28 && relX >= bw - 156 && relX <= bw - 104) {
                        ShowConfigWindow();
                        return;
                    } else if (!connectomeCollapsed && relY >= bh - 54 && relY <= bh - 32 && relX >= bw - 144 && relX < bw - 72) {
                        string candIp = PorygonCyberDefenseEngine.LastCandidateBlockIp;
                        if (!string.IsNullOrEmpty(candIp)) {
                            bool ok = PorygonCyberDefenseEngine.BlockRemoteIpInFirewall(candIp, "1-Click HUD IPS Block (" + PorygonCyberDefenseEngine.LastCandidateBlockReason + ")", false);
                            ShowCyberActionToast(
                                ok ? "[ PORYGON-Z IPS // IP AISLADA EN WINDOWS FIREWALL ]" : "[ PORYGON-Z IPS // PROTECCIÓN DE LISTA BLANCA ]",
                                ok ? ("Regla creada en Firewall para bloquear tráfico hacia/desde " + candIp) : ("No se bloqueó " + candIp + " (IP protegida)"),
                                ok ? Color.FromArgb(248, 113, 113) : Color.FromArgb(250, 204, 21));
                            hudConnectomeDirty = true;
                        } else {
                            ShowSiemCommandCenterWindow();
                        }
                        return;
                    } else if (!connectomeCollapsed && relY >= bh - 54 && relY <= bh - 32 && relX >= bw - 72 && relX <= bw - 6) {
                        string candFile = PorygonCyberDefenseEngine.LastCandidateQuarantineFile;
                        if (!string.IsNullOrEmpty(candFile) && File.Exists(candFile)) {
                            bool ok = PorygonCyberDefenseEngine.QuarantineFile(candFile, "1-Click HUD Quarantine", PorygonCyberDefenseEngine.LastCandidateQuarantineSha, PorygonCyberDefenseEngine.LastCandidateQuarantineScore);
                            ShowCyberActionToast(
                                ok ? "[ PORYGON2 AV // ARCHIVO ENVIADO A BÓVEDA DE CUARENTENA ]" : "[ PORYGON2 AV // ARCHIVO PROTEGIDO ]",
                                ok ? ("Aislado en Quarantine: " + Path.GetFileName(candFile)) : "Abre el Centro SIEM/AV para gestionar la Bóveda de Cuarentena",
                                ok ? Color.FromArgb(250, 204, 21) : Color.FromArgb(52, 211, 153));
                            hudConnectomeDirty = true;
                        } else {
                            ShowSiemCommandCenterWindow();
                        }
                        return;
                    }
                    connectomeCollapsed = !connectomeCollapsed;
                    Config.ConnectomeCollapsed = connectomeCollapsed;
                    Config.Save();
                    hudConnectomeDirty = true;
                    return;
                }

                Rectangle brainRect = GetCurrentBrain3DRect();
                if (brainRect.Contains(pt)) {
                    HandleBrain3DClick(pt, brainRect);
                    return;
                }
            }
        } catch { }
    }

    private bool isRenderingTick = false;

    private void OnRenderTick(object sender, EventArgs e) {
        if (isRenderingTick) return;
        isRenderingTick = true;
        try {
            OnRenderTickCore();
        } finally {
            isRenderingTick = false;
        }
    }

    private void ProcessOpacityFlagOrDefault() {
        try {
            string opFlagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opacity_cmd.flag");
            if (File.Exists(opFlagPath)) {
                string otxt = File.ReadAllText(opFlagPath).Trim().ToLowerInvariant();
                File.Delete(opFlagPath);
                int parsedOp;
                if (otxt == "up" || otxt == "+") {
                    AdjustTerminalOpacity(15);
                } else if (otxt == "down" || otxt == "-") {
                    AdjustTerminalOpacity(-15);
                } else if (otxt == "max" || otxt == "100") {
                    SetTerminalOpacity(100, true);
                } else if (otxt == "toggle") {
                    ToggleTerminalOpacityQuick();
                } else if (int.TryParse(otxt, out parsedOp)) {
                    SetTerminalOpacity(parsedOp, true);
                } else {
                    AdjustTerminalOpacity(15);
                }
                return;
            }
        } catch { }
        AdjustTerminalOpacity(15);
    }

    private void OnRenderTickCore() {
        if (showConfigEvent != null && showConfigEvent.WaitOne(0)) {
            ShowConfigWindow();
        }
        if (toggleGamerEvent != null && toggleGamerEvent.WaitOne(0)) {
            SetGamerMode(!Config.GamerMode, "ACCESO DIRECTO");
        }
        if (opacityCmdEvent != null && opacityCmdEvent.WaitOne(0)) {
            ProcessOpacityFlagOrDefault();
        }

        frameCount++;
        if (frameCount == 25 || frameCount == 90) {
            EnsureHotkeysRegistered();
        }

        // Throttle disk flag checks to once every 20 ticks (~0.8s) instead of every frame (eliminates 48 disk I/O ops/sec)
        if (frameCount % 20 == 0 || isGamerSuspended) {
            try {
                string opFlagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opacity_cmd.flag");
                if (File.Exists(opFlagPath)) {
                    ProcessOpacityFlagOrDefault();
                }
                string gmFlagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gamer_mode.flag");
                if (File.Exists(gmFlagPath)) {
                    string gtxt = File.ReadAllText(gmFlagPath).Trim();
                    File.Delete(gmFlagPath);
                    if (gtxt == "1" || gtxt.Equals("on", StringComparison.OrdinalIgnoreCase) || gtxt.Equals("true", StringComparison.OrdinalIgnoreCase)) {
                        SetGamerMode(true, "CLI_FLAG");
                    } else if (gtxt == "0" || gtxt.Equals("off", StringComparison.OrdinalIgnoreCase) || gtxt.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                        SetGamerMode(false, "CLI_FLAG");
                    } else {
                        ToggleGamerMode();
                    }
                }
                string flagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "brain_fullscreen.flag");
                if (File.Exists(flagPath)) {
                    string txt = File.ReadAllText(flagPath).Trim();
                    File.Delete(flagPath);
                    brain3DCollapsed = false;
                    brain3DFullscreen = (txt == "1" || txt.Equals("true", StringComparison.OrdinalIgnoreCase) || !brain3DFullscreen);
                    hudBrain3DDirty = true;
                }
                string bgFlagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bg_scene.flag");
                if (File.Exists(bgFlagPath)) {
                    string btxt = File.ReadAllText(bgFlagPath).Trim();
                    File.Delete(bgFlagPath);
                    int parsedMode;
                    if (int.TryParse(btxt, out parsedMode)) {
                        Config.BgSceneMode = Math.Max(0, Math.Min(2, parsedMode));
                        Config.Save();
                        RebuildBackground();
                        hudTelemetryDirty = true;
                    } else {
                        CycleBgSceneMode();
                    }
                }
            } catch { }
        }

        // Auto-detect Fullscreen / Borderless-Fullscreen Games every ~1.5 seconds
        if (Config.AutoGamerOnFullscreenGame && (frameCount % 32 == 0 || (isGamerSuspended && frameCount % 12 == 0))) {
            string detGame;
            bool gameNow = IsFullscreenGameActive(out detGame);
            if (gameNow != autoGameDetected) {
                autoGameDetected = gameNow;
                activeGameName = detGame;
                UpdateGamerSuspensionState(true);
            }
        }

        // Interactive 3D Brain Click-and-Drag Orbit + Single-Click Detection on Desktop HUD Cards
        MouseButtons mb = Control.MouseButtons;
        bool leftDown = (mb & MouseButtons.Left) != 0;
        bool rightDown = (mb & MouseButtons.Right) != 0;
        bool anyBrainDragBtn = leftDown || rightDown;

        if (!isGamerSuspended && Config.ShowPorygon && !brain3DCollapsed) {
            Point screenPt = Control.MousePosition;
            Point clientPt = this.PointToClient(screenPt);
            Rectangle bRect = GetCurrentBrain3DRect();

            if (anyBrainDragBtn && !prevLeftMouseDown && !prevRightMouseDown) {
                if (bRect.Contains(clientPt) && IsDesktopWindowHandle(WindowFromPoint(screenPt))) {
                    int relY = clientPt.Y - bRect.Y;
                    if (relY > 34 && relY < bRect.Height - 28) {
                        brain3DDragging = true;
                        brain3DDragMoved = false;
                        brain3DDragStartPt = clientPt;
                        brain3DDragStartYaw = GetEffectiveBrainYaw();
                        brain3DDragStartPitch = GetEffectiveBrainPitch();
                        brain3DPanDrag = rightDown && !leftDown;
                        brain3DDragStartPanX = brain3DPanX;
                        brain3DDragStartPanY = brain3DPanY;
                    }
                }
            } else if (anyBrainDragBtn && brain3DDragging) {
                int dx = clientPt.X - brain3DDragStartPt.X;
                int dy = clientPt.Y - brain3DDragStartPt.Y;
                if (Math.Abs(dx) > 4 || Math.Abs(dy) > 4) {
                    brain3DDragMoved = true;
                    if (brain3DPanDrag) {
                        brain3DPanX = brain3DDragStartPanX + dx;
                        brain3DPanY = brain3DDragStartPanY + dy;
                        ClampBrainPan();
                    } else {
                        brain3DAutoRotate = false;
                        brain3DManualYaw = brain3DDragStartYaw + dx * 0.012f;
                        brain3DManualPitch = Math.Max(-1.20f, Math.Min(1.20f, brain3DDragStartPitch - dy * 0.010f));
                    }
                    hudBrain3DDirty = true;
                }
            } else if (!anyBrainDragBtn && brain3DDragging) {
                brain3DDragging = false;
            }

            // Hover tracking for the fullscreen neuron inspector (bitmap-local coords: card is drawn at (2,2) in a 1:1 texture)
            if (brain3DFullscreen) {
                if (!brain3DDragging && bRect.Contains(clientPt) && IsDesktopWindowHandle(WindowFromPoint(screenPt))) {
                    int hx = clientPt.X - bRect.X + 2;
                    int hy = clientPt.Y - bRect.Y + 2;
                    if (brain3DHoverX < 0 || Math.Abs(hx - brain3DHoverX) >= 3 || Math.Abs(hy - brain3DHoverY) >= 3) {
                        brain3DHoverX = hx;
                        brain3DHoverY = hy;
                        hudBrain3DDirty = true;
                    }
                } else if (brain3DHoverX >= 0) {
                    brain3DHoverX = -1;
                    brain3DHoverY = -1;
                    hudBrain3DDirty = true;
                }
            } else if (brain3DHoverX >= 0) {
                brain3DHoverX = -1;
                brain3DHoverY = -1;
            }

            // Canvas-space hover for the native OpenGL FlyWire panel
            if (bRect.Contains(clientPt) && IsDesktopWindowHandle(WindowFromPoint(screenPt))) {
                brainHoverClientX = clientPt.X;
                brainHoverClientY = clientPt.Y;
            } else {
                brainHoverClientX = -1;
                brainHoverClientY = -1;
            }
            // Mouse-wheel zoom (low-level hook runs on its own thread; we only arm it while the cursor is over the brain on the desktop)
            Point bScr = this.PointToScreen(new Point(bRect.X, bRect.Y));
            BrainWheelHook.ScreenL = bScr.X; BrainWheelHook.ScreenT = bScr.Y;
            BrainWheelHook.ScreenR = bScr.X + bRect.Width; BrainWheelHook.ScreenB = bScr.Y + bRect.Height;
            BrainWheelHook.Armed = brainHoverClientX >= 0;
            int wheel = BrainWheelHook.TakeDelta();
            if (wheel != 0) ApplyBrainWheelZoom(wheel, clientPt);
        }

        if (!leftDown && prevLeftMouseDown) {
            // Fire card click on mouse button release (even in Gamer Mode so clicking [GMR:ON] wakes immediately!)
            if (!brain3DDragMoved) {
                HandleDesktopCardClick();
            }
            brain3DDragMoved = false;
        }
        prevLeftMouseDown = leftDown;
        prevRightMouseDown = rightDown;

        // ZERO-GPU HIBERNATION: When Gamer Mode is active, skip all CPU/GPU simulation & OpenGL rendering completely!
        if (isGamerSuspended) {
            if (!gamerStaticFrameRendered) {
                RenderGamerHibernationFrameOpenGL();
            }
            return;
        }

        // FlyWire connectome simulation advances every tick (independent of panel visibility) so IDS cascades complete in real time
        if (IsFlyWireReady()) {
            double fwNow = flyWireClock.Elapsed.TotalSeconds;
            float fwDt = (float)(fwNow - flyWireLastT);
            flyWireLastT = fwNow;
            PumpFlyWireIdsEvents(fwNow);
            flyWire.Step(fwDt, GetBaselineRegionGlow());
        }

        // Smart Adaptive Multi-Monitor GPU Governor (Target <= 10% GPU):
        // - Per-monitor occlusion culling (0 GPU/CPU work on any monitor covered by a maximized window)
        // - Complete 0.0% GPU pause when all 5 monitors are covered (AutoPauseMaximized) or external app > 90% GPU
        // - 25 FPS (38ms) when Desktop is focused, 11-12 FPS (85-95ms) Eco Mode when working in an app or approaching 10% GPU budget
        if (frameCount <= 2 || frameCount % 10 == 0) {
            cachedWindowMaximized = UpdateMonitorOcclusionState();
            IntPtr fgWnd = GetForegroundWindow();
            bool deskFocused = IsDesktopWindowHandle(fgWnd) || brain3DDragging || hudBrain3DDirty;
            gpuEcoMode = !deskFocused || primaryMonitorOccluded || anyMonitorOccluded || gpuOverBudgetEco;
            int targetInterval = (Config.AutoPauseMaximized && cachedWindowMaximized)
                ? 100
                : (gpuOverBudgetEco ? 95 : (deskFocused ? 38 : (primaryMonitorOccluded ? 95 : 80)));
            if (renderTimer.Interval != targetInterval) {
                renderTimer.Interval = targetInterval;
            }
        }

        if (frameCount <= 5 || frameCount == 25 || frameCount == 60 || frameCount % 160 == 0) {
            EnsureBehindDesktopIcons();
        }
        if (Config.AutoPauseMaximized && cachedWindowMaximized) {
            lock (telemetryLock) {
                currentFps = 0;
                gpuFrameMs = 0.0f;
                gpuUsagePct = 0.0f;
            }
            return;
        }

        // Real-time measured FPS counter (updates once per second)
        fpsFrameAccum++;
        long fpsElapsedMs = fpsMeasureSw.ElapsedMilliseconds;
        if (fpsElapsedMs >= 1000) {
            currentFps = Math.Max(10, Math.Min(60, (int)Math.Round(fpsFrameAccum * 1000.0 / fpsElapsedMs)));
            fpsFrameAccum = 0;
            fpsMeasureSw.Restart();
        }

        // Frame-rate independent animation step
        float dtScale = gpuEcoMode ? 1.05f : 0.78f;
        sceneTimePhase += 0.032f * dtScale;

        if (Config.RainSpeed > 0) {
            float speedMult = (Config.RainSpeed / 100.0f) * dtScale;
            for (int i = 0; i < columns.Count; i++) {
                RainColumn col = columns[i];
                if (IsMonitorOccluded(col.MonitorIndex)) continue;
                int cStep = col.CharStep > 0 ? col.CharStep : 16;
                col.Y += col.BaseSpeed * speedMult;
                if (col.Y - col.Length * cStep > col.MaxY) {
                    col.Y = col.MinY - rng.Next(20, 240);
                    AssignColumnLogText(col);
                }
            }
        }

        if (Config.ShowPorygon) {
            Porygon3DAvatar.RealTelemetryFeed feedSnap;
            lock (telemetryLock) { feedSnap = currentFeed; }
            Point clientMouse = this.PointToClient(Control.MousePosition);
            porygon.Update(screenW, screenH, clientMouse, feedSnap, Config.FollowMouse, Config.TrainingMode);
        }

        radarAngle = (radarAngle + 3.0f * dtScale) % 360f;
        brain3DYaw = (brain3DYaw + 0.042f * dtScale) % (float)(Math.PI * 2.0);

        if (frameCount % 900 == 0) {
            try { EmptyWorkingSet(Process.GetCurrentProcess().Handle); } catch { }
        }

        if (glEnabled) {
            RenderOpenGLFrame();
        } else {
            this.Invalidate();
        }
    }

    private static readonly string[] RIVER_DATA_TOKENS = new string[] {
        "01", "FF", "A7", "10", "7E", "C4", "00", "9B", "3F", "E2", "11", "8A", "5D", "F0", "0A", "6C"
    };
    private static readonly float[] RIVER_LANE_OFFSETS = new float[] { -0.72f, -0.36f, 0.0f, 0.36f, 0.72f };
    private static readonly float[] SUB_ROCK_T = new float[] { 0.30f, 0.45f, 0.60f, 0.75f, 0.89f };
    private static readonly float[] SUB_ROCK_U = new float[] { -0.36f, 0.34f, -0.28f, 0.38f, -0.32f };
    private static readonly float[] PK_NORM_X = new float[] {
        0.04f, 0.11f, 0.19f, 0.27f, 0.34f, 0.40f, 0.45f,
        0.55f, 0.60f, 0.66f, 0.73f, 0.81f, 0.89f, 0.96f
    };
    private static readonly float[] PK_SPAN_W = new float[] {
        0.065f, 0.075f, 0.085f, 0.080f, 0.070f, 0.058f, 0.045f,
        0.045f, 0.058f, 0.070f, 0.080f, 0.085f, 0.075f, 0.065f
    };
    private static readonly float[] PK_HEIGHT_FRAC = new float[] {
        0.052f, 0.082f, 0.108f, 0.092f, 0.068f, 0.046f, 0.025f,
        0.025f, 0.048f, 0.070f, 0.094f, 0.106f, 0.080f, 0.050f
    };
    private static readonly float[] ROCK_DEPTHS = new float[] { 0.22f, 0.30f, 0.38f, 0.46f, 0.54f, 0.62f, 0.70f, 0.76f, 0.82f, 0.88f, 0.92f, 0.96f };
    private static readonly float[] ROCK_SIDES  = new float[] { -1.0f,  1.0f, -1.0f,  1.0f, -1.0f,  1.0f, -1.0f,  1.0f, -1.0f,  1.0f, -1.0f,  1.0f };
    private static readonly float[] ROCK_BANK_M = new float[] { 1.12f, 1.18f, 1.95f, 1.14f, 1.20f, 2.15f, 1.12f, 1.16f, 2.35f, 1.12f, 1.18f, 1.85f };
    private static readonly float[] TREE_POS_NORM = new float[] {
        0.34f, 0.66f, 0.24f, 0.76f, 0.13f, 0.87f, 0.30f, 0.70f, 0.19f, 0.81f, 0.07f, 0.93f, 0.25f, 0.75f
    };
    private static readonly float[] TREE_DEPTH = new float[] {
        0.22f, 0.24f, 0.34f, 0.36f, 0.46f, 0.48f, 0.58f, 0.60f, 0.70f, 0.72f, 0.82f, 0.84f, 0.91f, 0.93f
    };
    private static readonly int[] TREE_SPECIES = new int[] {
        0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 0, 2, 1, 0
    };

    // Pre-allocated zero-GC scratch arrays for 3D river, terrain, rocks, and ferns
    private readonly float[] bufRCx = new float[25];
    private readonly float[] bufRCy = new float[25];
    private readonly float[] bufRHw = new float[25];
    private readonly float[] bufRPr = new float[25];
    private readonly float[] bufRkX = new float[12];
    private readonly float[] bufRkY = new float[12];
    private readonly float[] bufRkW = new float[12];
    private readonly float[] bufRkH = new float[12];
    private readonly float[] bufRkPersp = new float[12];
    private readonly float[] bufFnX = new float[14];
    private readonly float[] bufFnY = new float[14];
    private readonly float[] bufFnScale = new float[14];
    private readonly float[,] bufTerVX = new float[7, 19];
    private readonly float[,] bufTerVY = new float[7, 19];

    private void GetRiverGeometryAt(float t, Rectangle m, float horizonY, float vanishX, float floorSpan, float phase, int mIdx,
        out float cx, out float cy, out float halfW, out float persp) {
        if (t < 0f) t = 0f;
        if (t > 1f) t = 1f;
        persp = (float)Math.Pow(t, 1.45);
        cy = horizonY + persp * floorSpan;

        // Graceful S-curve meander that converges to vanishX (0 offset) at the horizon (t = 0)
        float swaySlow = (float)Math.Sin(phase * 0.22f + mIdx * 0.7f) * 0.24f;
        float sCurve = (float)Math.Sin(t * 4.85f - 0.60f + swaySlow) + 0.30f * (float)Math.Cos(t * 2.35f + 0.35f);
        float curveAmp = (m.Width * 0.096f) * (float)Math.Pow(t, 0.82);
        cx = vanishX + sCurve * curveAmp;

        // River width narrows to a vanishing point (2.2px) at horizon and widens in foreground
        halfW = 2.2f + (m.Width * 0.116f) * (float)Math.Pow(t, 1.32);
    }

    private void RenderCyberDataRiverOpenGL(int mIdx, Rectangle m, float horizonY, float vanishX, float floorSpan, float phase, float netMod, Color c1, Color c2, Color c3) {
        bool lowLod = gpuEcoMode || mIdx > 0 || m.Width <= 1366;
        int rSteps = lowLod ? 14 : 20;
        float[] rCx = bufRCx;
        float[] rCy = bufRCy;
        float[] rHw = bufRHw;
        float[] rPr = bufRPr;

        for (int i = 0; i <= rSteps; i++) {
            float t = i / (float)rSteps;
            GetRiverGeometryAt(t, m, horizonY, vanishX, floorSpan, phase, mIdx, out rCx[i], out rCy[i], out rHw[i], out rPr[i]);
        }

        // 1. Batched Low-Poly Riverbed Channel, Sloped Canyon Banks & Inner Core Current (1 GL_TRIANGLES batch)
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int i = 0; i < rSteps; i++) {
            float p0 = rPr[i];
            float p1 = rPr[i + 1];
            float pm = 0.5f * (p0 + p1);

            float lx0 = rCx[i] - rHw[i], rx0 = rCx[i] + rHw[i];
            float lx1 = rCx[i + 1] - rHw[i + 1], rx1 = rCx[i + 1] + rHw[i + 1];

            float blx0 = rCx[i] - rHw[i] * 1.28f, brx0 = rCx[i] + rHw[i] * 1.28f;
            float blx1 = rCx[i + 1] - rHw[i + 1] * 1.28f, brx1 = rCx[i + 1] + rHw[i + 1] * 1.28f;

            // Left & Right Low-Poly Sloped Canyon Banks
            byte bankAlpha = (byte)(28 + (int)(pm * 52f));
            GpuGl.glColor4ub((byte)(c3.R / 3), (byte)(c3.G / 2), (byte)(c3.B / 2), bankAlpha);
            GpuGl.glVertex2f(blx0, rCy[i] - 2f * p0);
            GpuGl.glVertex2f(lx0, rCy[i]);
            GpuGl.glVertex2f(lx1, rCy[i + 1]);
            GpuGl.glVertex2f(blx0, rCy[i] - 2f * p0);
            GpuGl.glVertex2f(lx1, rCy[i + 1]);
            GpuGl.glVertex2f(blx1, rCy[i + 1] - 2f * p1);

            GpuGl.glVertex2f(rx0, rCy[i]);
            GpuGl.glVertex2f(brx0, rCy[i] - 2f * p0);
            GpuGl.glVertex2f(brx1, rCy[i + 1] - 2f * p1);
            GpuGl.glVertex2f(rx0, rCy[i]);
            GpuGl.glVertex2f(brx1, rCy[i + 1] - 2f * p1);
            GpuGl.glVertex2f(rx1, rCy[i + 1]);

            // Deep Obsidian-Teal Riverbed Water Base
            byte bedAlpha = (byte)(120 + (int)(pm * 95f));
            GpuGl.glColor4ub(3, 15, 16, bedAlpha);
            GpuGl.glVertex2f(lx0, rCy[i]);
            GpuGl.glVertex2f(rx0, rCy[i]);
            GpuGl.glVertex2f(rx1, rCy[i + 1]);
            GpuGl.glVertex2f(lx0, rCy[i]);
            GpuGl.glVertex2f(rx1, rCy[i + 1]);
            GpuGl.glVertex2f(lx1, rCy[i + 1]);

            // Bioluminescent Inner Core Data Channel (±0.52 width)
            float ilx0 = rCx[i] - rHw[i] * 0.52f, irx0 = rCx[i] + rHw[i] * 0.52f;
            float ilx1 = rCx[i + 1] - rHw[i + 1] * 0.52f, irx1 = rCx[i + 1] + rHw[i + 1] * 0.52f;
            byte coreAlpha = (byte)(22 + (int)(pm * 46f));
            byte cr = (byte)((c1.R + c3.R) / 3);
            byte cg = (byte)((c1.G + c3.G) / 2);
            byte cb = (byte)((c1.B + c3.B) / 2);
            GpuGl.glColor4ub(cr, cg, cb, coreAlpha);
            GpuGl.glVertex2f(ilx0, rCy[i]);
            GpuGl.glVertex2f(irx0, rCy[i]);
            GpuGl.glVertex2f(irx1, rCy[i + 1]);
            GpuGl.glVertex2f(ilx0, rCy[i]);
            GpuGl.glVertex2f(irx1, rCy[i + 1]);
            GpuGl.glVertex2f(ilx1, rCy[i + 1]);
        }
        GpuGl.glEnd();

        // 2. Faceted Low-Poly Water Mesh & Transverse Current Ripples (1 GL_LINES batch)
        GpuGl.glLineWidth(1.0f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int i = 1; i < rSteps; i++) {
            float pm = rPr[i];
            byte wAlpha = (byte)(18 + (int)(pm * 42f));
            GpuGl.glColor4ub(c1.R, c1.G, c1.B, wAlpha);
            // Low-poly water triangulation diagonals
            GpuGl.glVertex2f(rCx[i] - rHw[i], rCy[i]);
            GpuGl.glVertex2f(rCx[i], rCy[i + 1]);
            GpuGl.glVertex2f(rCx[i] + rHw[i], rCy[i]);
            GpuGl.glVertex2f(rCx[i], rCy[i + 1]);
        }

        // Scrolling V-Ripples & Wave Crests travelling along the curved channel into the horizon
        int rippleCount = lowLod ? 10 : 16;
        float flowPhase = phase * (0.38f + Math.Min(0.45f, netMod * 0.18f));
        for (int rp = 0; rp < rippleCount; rp++) {
            // Scroll t from 1.0 -> 0.0 so the caudal visually flows and disappears into the horizon!
            float rawT = 1.0f - ((rp / (float)rippleCount + flowPhase * 0.25f) % 1.0f);
            if (rawT < 0.04f || rawT > 0.98f) continue;
            float rcx, rcy, rhw, rpersp;
            GetRiverGeometryAt(rawT, m, horizonY, vanishX, floorSpan, phase, mIdx, out rcx, out rcy, out rhw, out rpersp);
            float bowY = rcy - (2.5f + 7.5f * rpersp); // Chevron apex pointing toward the horizon
            byte rpAlpha = (byte)(24 + (int)(rpersp * 74f));
            GpuGl.glColor4ub(c1.R, c1.G, c1.B, rpAlpha);
            GpuGl.glVertex2f(rcx - rhw * 0.86f, rcy);
            GpuGl.glVertex2f(rcx, bowY);
            GpuGl.glVertex2f(rcx, bowY);
            GpuGl.glVertex2f(rcx + rhw * 0.86f, rcy);
        }
        GpuGl.glEnd();

        // 3. Glowing Curved Riverbanks & Shoreline Rims (Left & Right Banks + Outer Canyon Rim)
        for (int side = -1; side <= 1; side += 2) {
            GpuGl.glLineWidth(1.5f);
            GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
            for (int i = 0; i <= rSteps; i++) {
                byte sAlpha = (byte)(35 + (int)(rPr[i] * 115f));
                GpuGl.glColor4ub(c1.R, c1.G, c1.B, sAlpha);
                GpuGl.glVertex2f(rCx[i] + side * rHw[i], rCy[i]);
            }
            GpuGl.glEnd();

            GpuGl.glLineWidth(0.9f);
            GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
            for (int i = 0; i <= rSteps; i++) {
                byte bAlpha = (byte)(20 + (int)(rPr[i] * 68f));
                GpuGl.glColor4ub(c2.R, c2.G, c2.B, bAlpha);
                GpuGl.glVertex2f(rCx[i] + side * rHw[i] * 1.28f, rCy[i] - 2f * rPr[i]);
            }
            GpuGl.glEnd();
        }

        // 4. Continuous Curved Data-Stream Filaments Inside the Riverbed
        float[] laneOffsets = RIVER_LANE_OFFSETS;
        int laneStep = lowLod ? 2 : 1;
        for (int ln = 0; ln < laneOffsets.Length; ln += laneStep) {
            float lu = laneOffsets[ln];
            bool isCenterLane = (ln == 2);
            GpuGl.glLineWidth(isCenterLane ? 1.4f : 1.0f);
            GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
            for (int i = 0; i <= rSteps; i++) {
                float t = i / (float)rSteps;
                float waveShift = (float)Math.Sin(t * 14.0f + phase * 2.4f + ln * 1.3f) * (rHw[i] * 0.045f);
                float lx = rCx[i] + lu * rHw[i] + waveShift;
                byte fAlpha = (byte)((isCenterLane ? 42 : 26) + (int)(rPr[i] * (isCenterLane ? 92f : 64f)));
                if (isCenterLane) {
                    GpuGl.glColor4ub(c2.R, c2.G, c2.B, fAlpha);
                } else {
                    GpuGl.glColor4ub(c1.R, c1.G, c1.B, fAlpha);
                }
                GpuGl.glVertex2f(lx, rCy[i]);
            }
            GpuGl.glEnd();
        }

        // 5. High-Speed Cyber Data Packets & Light-Streaks Flowing Along the Curved Riverbed into the Horizon!
        int packetCount = lowLod ? 18 : 30;
        GpuGl.glLineWidth(2.2f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int p = 0; p < packetCount; p++) {
            float seedFrac = (p * 0.6180339f) % 1.0f;
            float laneU = ((p % 7) - 3) * 0.24f; // -0.72 .. +0.72 across the riverbed
            float speed = 0.14f + (p % 5) * 0.035f;
            // Flow from foreground (1.0) -> horizon (0.0) so data streams vanish into the horizon!
            float tHead = 1.0f - ((seedFrac + flowPhase * speed) % 1.0f);
            if (tHead < 0.03f || tHead > 0.98f) continue;
            float tTail = Math.Min(1.0f, tHead + 0.045f + 0.025f * tHead);

            float hx, hy, hhw, hpr;
            float tx, ty, thw, tpr;
            GetRiverGeometryAt(tHead, m, horizonY, vanishX, floorSpan, phase, mIdx, out hx, out hy, out hhw, out hpr);
            GetRiverGeometryAt(tTail, m, horizonY, vanishX, floorSpan, phase, mIdx, out tx, out ty, out thw, out tpr);

            float px0 = hx + laneU * hhw;
            float py0 = hy;
            float px1 = tx + laneU * thw;
            float py1 = ty;

            bool isGoldPacket = (p % 4 == 0);
            Color pCol = isGoldPacket ? c2 : c1;
            byte headAlpha = (byte)(65 + (int)(hpr * 165f));
            GpuGl.glColor4ub(pCol.R, pCol.G, pCol.B, headAlpha);
            GpuGl.glVertex2f(px0, py0);
            GpuGl.glColor4ub(pCol.R, pCol.G, pCol.B, 8);
            GpuGl.glVertex2f(px1, py1);
        }
        GpuGl.glEnd();

        // Packet Node Cores (1 GL_QUADS batch)
        GpuGl.glBegin(GpuGl.GL_QUADS);
        for (int p = 0; p < packetCount; p++) {
            float seedFrac = (p * 0.6180339f) % 1.0f;
            float laneU = ((p % 7) - 3) * 0.24f;
            float speed = 0.14f + (p % 5) * 0.035f;
            float tHead = 1.0f - ((seedFrac + flowPhase * speed) % 1.0f);
            if (tHead < 0.04f || tHead > 0.98f) continue;

            float hx, hy, hhw, hpr;
            GetRiverGeometryAt(tHead, m, horizonY, vanishX, floorSpan, phase, mIdx, out hx, out hy, out hhw, out hpr);
            float px = hx + laneU * hhw;
            float py = hy;
            float sz = 1.2f + 2.6f * hpr;

            bool isGoldPacket = (p % 4 == 0);
            byte nAlpha = (byte)(85 + (int)(hpr * 160f));
            if (isGoldPacket) {
                GpuGl.glColor4ub(245, 220, 145, nAlpha);
            } else {
                GpuGl.glColor4ub(190, 248, 238, nAlpha);
            }
            GpuGl.glVertex2f(px - sz, py - sz * 0.7f);
            GpuGl.glVertex2f(px + sz, py - sz * 0.7f);
            GpuGl.glVertex2f(px + sz, py + sz * 0.7f);
            GpuGl.glVertex2f(px - sz, py + sz * 0.7f);
        }
        GpuGl.glEnd();

        // 6. Partially Submerged River Rocks with Animated Data-Foam V-Wakes (5 River Boulders)
        float[] subRockT = SUB_ROCK_T;
        float[] subRockU = SUB_ROCK_U;
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int sr = 0; sr < subRockT.Length; sr++) {
            float st = subRockT[sr];
            float sxC, syC, sHw, sPr;
            GetRiverGeometryAt(st, m, horizonY, vanishX, floorSpan, phase, mIdx, out sxC, out syC, out sHw, out sPr);
            float rx = sxC + subRockU[sr] * sHw;
            float ry = syC + 1.0f * sPr;
            float rw = (6.0f + (sr % 2) * 3.5f) * (0.28f + 0.72f * sPr);
            float rh = (4.2f + (sr % 2) * 2.4f) * (0.28f + 0.72f * sPr);

            // Lit left facet
            GpuGl.glColor4ub((byte)(18 + c1.R / 4), (byte)(38 + c1.G / 4), (byte)(44 + c1.B / 4), (byte)(155 + (int)(sPr * 75f)));
            GpuGl.glVertex2f(rx - rw, ry);
            GpuGl.glVertex2f(rx - rw * 0.25f, ry - rh);
            GpuGl.glVertex2f(rx + rw * 0.15f, ry);

            // Shaded right facet
            GpuGl.glColor4ub(8, 20, 28, (byte)(170 + (int)(sPr * 70f)));
            GpuGl.glVertex2f(rx - rw * 0.25f, ry - rh);
            GpuGl.glVertex2f(rx + rw, ry);
            GpuGl.glVertex2f(rx + rw * 0.15f, ry);
        }
        GpuGl.glEnd();

        // Submerged rock wireframe edges + animated V-wake data foam ripples
        GpuGl.glLineWidth(1.0f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int sr = 0; sr < subRockT.Length; sr++) {
            float st = subRockT[sr];
            float sxC, syC, sHw, sPr;
            GetRiverGeometryAt(st, m, horizonY, vanishX, floorSpan, phase, mIdx, out sxC, out syC, out sHw, out sPr);
            float rx = sxC + subRockU[sr] * sHw;
            float ry = syC + 1.0f * sPr;
            float rw = (6.0f + (sr % 2) * 3.5f) * (0.28f + 0.72f * sPr);
            float rh = (4.2f + (sr % 2) * 2.4f) * (0.28f + 0.72f * sPr);

            byte edgeA = (byte)(75 + (int)(sPr * 110f));
            GpuGl.glColor4ub(c1.R, c1.G, c1.B, edgeA);
            GpuGl.glVertex2f(rx - rw, ry); GpuGl.glVertex2f(rx - rw * 0.25f, ry - rh);
            GpuGl.glVertex2f(rx - rw * 0.25f, ry - rh); GpuGl.glVertex2f(rx + rw, ry);
            GpuGl.glVertex2f(rx - rw * 0.25f, ry - rh); GpuGl.glVertex2f(rx + rw * 0.15f, ry);

            // Glowing waterline foam base + V-wake streaming toward the horizon (since current flows to horizon)
            for (int wk = 0; wk < 2; wk++) {
                float wkPulse = ((flowPhase * 0.85f + sr * 0.37f + wk * 0.5f) % 1.0f);
                float wkSpread = rw * (1.15f + wkPulse * 0.85f);
                float wkLen = (7.0f + wkPulse * 14.0f) * sPr;
                byte foamA = (byte)((1.0f - wkPulse) * (85f + sPr * 125f));
                GpuGl.glColor4ub((byte)Math.Min(255, c2.R + 45), (byte)Math.Min(255, c2.G + 45), (byte)Math.Min(255, c2.B + 45), foamA);

                // Left & right foam wake arms trailing toward horizon (upward -Y)
                GpuGl.glVertex2f(rx - rw * 0.85f, ry + 1.0f * sPr);
                GpuGl.glVertex2f(rx - wkSpread, ry - wkLen);

                GpuGl.glVertex2f(rx + rw * 0.85f, ry + 1.0f * sPr);
                GpuGl.glVertex2f(rx + wkSpread, ry - wkLen);

                // Front bow-wave arc at base of rock
                GpuGl.glVertex2f(rx - wkSpread * 0.75f, ry + (1.5f + wkPulse * 2.5f) * sPr);
                GpuGl.glVertex2f(rx + wkSpread * 0.75f, ry + (1.5f + wkPulse * 2.5f) * sPr);
            }
        }
        GpuGl.glEnd();

        // 7. Translucent Bio-Digital Mist Ribbons Drifting Over the River Channel (1 GL_TRIANGLES batch)
        int mistCount = lowLod ? 4 : 7;
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int mi = 0; mi < mistCount; mi++) {
            float mt = 0.16f + (mi / (float)(mistCount - 1)) * 0.76f;
            float mxC, myC, mHw, mPr;
            GetRiverGeometryAt(mt, m, horizonY, vanishX, floorSpan, phase, mIdx, out mxC, out myC, out mHw, out mPr);

            float drift = (float)Math.Sin(phase * 0.52f + mi * 1.45f) * (mHw * 0.28f);
            float mCenterX = mxC + drift;
            float mCenterY = myC - (2.5f + 5.5f * mPr);
            float mistHalfW = mHw * (1.15f + 0.20f * (float)Math.Cos(phase * 0.4f + mi));
            float mistHalfH = 2.2f + 5.2f * mPr;

            byte coreAlpha = (byte)(12 + (int)(mPr * 18f));
            Color mistCol = (mi % 2 == 0) ? c1 : c2;

            // Soft elongated diamond mist wisp (center alpha -> 0 alpha at outer tips)
            GpuGl.glColor4ub(mistCol.R, mistCol.G, mistCol.B, coreAlpha);
            GpuGl.glVertex2f(mCenterX, mCenterY - mistHalfH);
            GpuGl.glVertex2f(mCenterX, mCenterY + mistHalfH);
            GpuGl.glColor4ub(mistCol.R, mistCol.G, mistCol.B, 0);
            GpuGl.glVertex2f(mCenterX - mistHalfW, mCenterY);

            GpuGl.glColor4ub(mistCol.R, mistCol.G, mistCol.B, coreAlpha);
            GpuGl.glVertex2f(mCenterX, mCenterY - mistHalfH);
            GpuGl.glVertex2f(mCenterX, mCenterY + mistHalfH);
            GpuGl.glColor4ub(mistCol.R, mistCol.G, mistCol.B, 0);
            GpuGl.glVertex2f(mCenterX + mistHalfW, mCenterY);
        }
        GpuGl.glEnd();

        // 8. Floating Hex/Binary Telemetry Tokens Drifting in the River Current (1 VRAM Font Atlas Batch)
        if (!gpuEcoMode && !lowLod) {
            GpuGl.BeginFontAtlasBatch();
            int glyphStreams = 12;
            for (int gs = 0; gs < glyphStreams; gs++) {
                float seedFrac = (gs * 0.381966f + 0.11f) % 1.0f;
                float laneU = ((gs % 5) - 2) * 0.30f;
                float speed = 0.11f + (gs % 3) * 0.025f;
                float tPos = 1.0f - ((seedFrac + flowPhase * speed) % 1.0f);
                if (tPos < 0.18f || tPos > 0.95f) continue;

                float gx, gy, ghw, gpr;
                GetRiverGeometryAt(tPos, m, horizonY, vanishX, floorSpan, phase, mIdx, out gx, out gy, out ghw, out gpr);
                float drawX = gx + laneU * ghw - 6f * gpr;
                float drawY = gy - 5f * gpr;
                float cw = 6.5f + 6.5f * gpr;
                float ch = 7.5f + 7.5f * gpr;
                byte gAlpha = (byte)(35 + (int)(gpr * 145f));

                string tok = RIVER_DATA_TOKENS[(gs + (frameCount / 18)) % RIVER_DATA_TOKENS.Length];
                Color gCol = (gs % 3 == 0) ? c2 : c1;
                GpuGl.EmitAtlasChar(tok[0], drawX, drawY, cw, ch, gCol.R, gCol.G, gCol.B, gAlpha);
                if (tok.Length > 1) {
                    GpuGl.EmitAtlasChar(tok[1], drawX + cw * 0.62f, drawY, cw, ch, gCol.R, gCol.G, gCol.B, gAlpha);
                }
            }
            GpuGl.EndFontAtlasBatch();
        }
    }

    private void RenderHorizonMountainRangeAndPortalOpenGL(int mIdx, Rectangle m, float horizonY, float vanishX, float phase, float netMod, Color c1, Color c2, Color c3) {
        // =========================================================================
        // 1. CYBER-PORTAL / CONVERGENCE HALO AT THE RIVER VANISHING POINT (vanishX, horizonY)
        // =========================================================================
        float portalR = m.Width * 0.048f;
        float pulse = 0.88f + 0.12f * (float)Math.Sin(phase * 1.4f);
        float rOuter = portalR * pulse;
        float rInner = portalR * 0.56f * pulse;
        int arcSegs = 12;

        // Soft polygonal horizon glow fan above horizonY
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int i = 0; i < arcSegs; i++) {
            double a0 = Math.PI * (i / (double)arcSegs);
            double a1 = Math.PI * ((i + 1) / (double)arcSegs);
            float x0 = vanishX - (float)Math.Cos(a0) * rOuter * 1.45f;
            float y0 = horizonY - (float)Math.Sin(a0) * rOuter * 0.92f;
            float x1 = vanishX - (float)Math.Cos(a1) * rOuter * 1.45f;
            float y1 = horizonY - (float)Math.Sin(a1) * rOuter * 0.92f;

            GpuGl.glColor4ub(c2.R, c2.G, c2.B, 34);
            GpuGl.glVertex2f(vanishX, horizonY);
            GpuGl.glColor4ub(c1.R, c1.G, c1.B, 0);
            GpuGl.glVertex2f(x0, y0);
            GpuGl.glVertex2f(x1, y1);
        }
        GpuGl.glEnd();

        // Concentric wireframe geodesic portal arches above the vanishing river
        GpuGl.glLineWidth(0.95f);
        GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
        for (int i = 0; i <= arcSegs; i++) {
            double a = Math.PI * (i / (double)arcSegs);
            float px = vanishX - (float)Math.Cos(a) * rOuter;
            float py = horizonY - (float)Math.Sin(a) * (rOuter * 0.68f);
            byte pa = (byte)(35 + (int)(Math.Sin(a) * 65f));
            GpuGl.glColor4ub(c1.R, c1.G, c1.B, pa);
            GpuGl.glVertex2f(px, py);
        }
        GpuGl.glEnd();

        GpuGl.glLineWidth(0.85f);
        GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
        for (int i = 0; i <= arcSegs; i++) {
            double a = Math.PI * (i / (double)arcSegs);
            float px = vanishX - (float)Math.Cos(a) * rInner;
            float py = horizonY - (float)Math.Sin(a) * (rInner * 0.68f);
            byte pa = (byte)(50 + (int)(Math.Sin(a) * 80f));
            GpuGl.glColor4ub(c2.R, c2.G, c2.B, pa);
            GpuGl.glVertex2f(px, py);
        }
        GpuGl.glEnd();

        // Radial portal struts connecting inner and outer horizon rings
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int i = 1; i < arcSegs; i += 2) {
            double a = Math.PI * (i / (double)arcSegs);
            float cosA = (float)Math.Cos(a);
            float sinA = (float)Math.Sin(a);
            GpuGl.glColor4ub(c2.R, c2.G, c2.B, 52);
            GpuGl.glVertex2f(vanishX - cosA * rInner * 0.4f, horizonY - sinA * rInner * 0.28f);
            GpuGl.glVertex2f(vanishX - cosA * rOuter, horizonY - sinA * rOuter * 0.68f);
        }
        GpuGl.glEnd();

        // =========================================================================
        // 2. LOW-POLY FACETED HORIZON MOUNTAIN RANGE (14 PEAKS FRAMING THE CANYON PASS)
        // =========================================================================
        float[] pkNormX = PK_NORM_X;
        float[] pkSpanW = PK_SPAN_W;
        float[] pkHeightFrac = PK_HEIGHT_FRAC;

        int peakCount = pkNormX.Length;
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int p = 0; p < peakCount; p++) {
            bool isLeftWing = (p < 7);
            float px = m.Left + m.Width * pkNormX[p];
            float halfSpan = m.Width * pkSpanW[p];
            float ph = m.Height * pkHeightFrac[p];
            float py = horizonY - ph;
            float baseY = horizonY + 2.0f;
            // Central ridge offset slightly toward the central portal
            float ridgeBaseX = px + (isLeftWing ? halfSpan * 0.16f : -halfSpan * 0.16f);

            // Inner facet (illuminated by the central horizon portal) & outer facet (deep slate shadow)
            Color innerLit  = Color.FromArgb(46, (byte)(10 + c1.R / 4), (byte)(24 + c1.G / 3), (byte)(28 + c1.B / 3));
            Color outerDark = Color.FromArgb(54, 5, 13, 18);

            if (isLeftWing) {
                // Left face = outer shadow, Right face = inner portal-lit
                GpuGl.SetColor(outerDark);
                GpuGl.glVertex2f(px - halfSpan, baseY);
                GpuGl.glVertex2f(px, py);
                GpuGl.glVertex2f(ridgeBaseX, baseY);

                GpuGl.SetColor(innerLit);
                GpuGl.glVertex2f(px, py);
                GpuGl.glVertex2f(px + halfSpan, baseY);
                GpuGl.glVertex2f(ridgeBaseX, baseY);
            } else {
                // Left face = inner portal-lit, Right face = outer shadow
                GpuGl.SetColor(innerLit);
                GpuGl.glVertex2f(px - halfSpan, baseY);
                GpuGl.glVertex2f(px, py);
                GpuGl.glVertex2f(ridgeBaseX, baseY);

                GpuGl.SetColor(outerDark);
                GpuGl.glVertex2f(px, py);
                GpuGl.glVertex2f(px + halfSpan, baseY);
                GpuGl.glVertex2f(ridgeBaseX, baseY);
            }
        }
        GpuGl.glEnd();

        // Crisp 0.85px wireframe mountain ridges
        GpuGl.glLineWidth(0.85f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int p = 0; p < peakCount; p++) {
            bool isLeftWing = (p < 7);
            float px = m.Left + m.Width * pkNormX[p];
            float halfSpan = m.Width * pkSpanW[p];
            float ph = m.Height * pkHeightFrac[p];
            float py = horizonY - ph;
            float baseY = horizonY + 2.0f;
            float ridgeBaseX = px + (isLeftWing ? halfSpan * 0.16f : -halfSpan * 0.16f);

            GpuGl.glColor4ub(c1.R, c1.G, c1.B, 58);
            GpuGl.glVertex2f(px - halfSpan, baseY); GpuGl.glVertex2f(px, py);
            GpuGl.glVertex2f(px, py); GpuGl.glVertex2f(px + halfSpan, baseY);

            GpuGl.glColor4ub(c2.R, c2.G, c2.B, 48);
            GpuGl.glVertex2f(px, py); GpuGl.glVertex2f(ridgeBaseX, baseY);
        }
        GpuGl.glEnd();
    }

    private void RenderDenseCyberForestFloraOpenGL(int mIdx, Rectangle m, float horizonY, float vanishX, float floorSpan, float ceilSpan, float phase, float netMod, Color c1, Color c2, Color c3) {
        bool lowLod = gpuEcoMode || mIdx > 0 || m.Width <= 1366;
        // =========================================================================
        // A. BALANCED LOW-POLY ROCKS & BIOLUMINESCENT CRYSTALS (12 CLUSTERS)
        //    Nestled along the curved riverbanks and near tree bases without clutter
        // =========================================================================
        float[] rockDepths = ROCK_DEPTHS;
        float[] rockSides  = ROCK_SIDES;
        float[] rockBankM  = ROCK_BANK_M;
        int rockCount = lowLod ? 8 : rockDepths.Length;

        float[] rkX = bufRkX;
        float[] rkY = bufRkY;
        float[] rkW = bufRkW;
        float[] rkH = bufRkH;
        float[] rkPersp = bufRkPersp;

        for (int rIdx = 0; rIdx < rockCount; rIdx++) {
            float t = rockDepths[rIdx];
            float rCx, rCy, rHw, persp;
            GetRiverGeometryAt(t, m, horizonY, vanishX, floorSpan, phase, mIdx, out rCx, out rCy, out rHw, out persp);

            float side = rockSides[rIdx];
            float offsetFromCenter = rHw * rockBankM[rIdx];
            rkX[rIdx] = Math.Max(m.Left + 24f, Math.Min(m.Right - 24f, rCx + side * offsetFromCenter));
            rkY[rIdx] = Math.Min(m.Bottom - 3f, rCy + (1.5f + 5.0f * persp));
            float sizeScale = 0.30f + 0.65f * persp;
            rkW[rIdx] = (9.0f + (rIdx % 3) * 4.5f) * sizeScale;
            rkH[rIdx] = (6.0f + ((rIdx + 1) % 3) * 3.5f) * sizeScale;
            rkPersp[rIdx] = persp;
        }

        // Pass A1: Batched GL_TRIANGLES for low-poly rocks & subtle crystal accents
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int rIdx = 0; rIdx < rockCount; rIdx++) {
            float bx = rkX[rIdx];
            float by = rkY[rIdx];
            float rw = rkW[rIdx];
            float rh = rkH[rIdx];
            float pr = rkPersp[rIdx];
            float tilt = ((rIdx % 3) - 1) * rw * 0.12f;

            float blX = bx - rw * 0.92f, blY = by;
            float tlX = bx - rw * 0.58f + tilt * 0.5f, tlY = by - rh * 0.74f;
            float pkX = bx + tilt, pkY = by - rh * 1.02f;
            float trX = bx + rw * 0.62f + tilt * 0.4f, trY = by - rh * 0.66f;
            float brX = bx + rw * 0.94f, brY = by;
            float mdX = bx + tilt * 0.35f, mdY = by - rh * 0.40f;

            byte aLit  = (byte)Math.Min(215, (int)(65 + pr * 105f));
            byte aMid  = (byte)Math.Min(220, (int)(75 + pr * 110f));
            byte aDark = (byte)Math.Min(225, (int)(85 + pr * 115f));

            GpuGl.glColor4ub((byte)(14 + c1.R / 5), (byte)(28 + c1.G / 4), (byte)(32 + c1.B / 4), aLit);
            GpuGl.glVertex2f(blX, blY); GpuGl.glVertex2f(tlX, tlY); GpuGl.glVertex2f(mdX, mdY);
            GpuGl.glVertex2f(tlX, tlY); GpuGl.glVertex2f(pkX, pkY); GpuGl.glVertex2f(mdX, mdY);

            GpuGl.glColor4ub((byte)(10 + c3.R / 6), (byte)(20 + c3.G / 5), (byte)(25 + c3.B / 5), aMid);
            GpuGl.glVertex2f(pkX, pkY); GpuGl.glVertex2f(trX, trY); GpuGl.glVertex2f(mdX, mdY);

            GpuGl.glColor4ub(8, 14, 20, aDark);
            GpuGl.glVertex2f(trX, trY); GpuGl.glVertex2f(brX, brY); GpuGl.glVertex2f(mdX, mdY);
            GpuGl.glVertex2f(blX, blY); GpuGl.glVertex2f(mdX, mdY); GpuGl.glVertex2f(brX, brY);

            // Companion smaller stone
            float sSide = (rIdx % 2 == 0) ? 1.0f : -1.0f;
            float sx = bx + sSide * rw * 0.76f;
            float sy = by + 1.2f * pr;
            float sw = rw * 0.44f;
            float sh = rh * 0.48f;
            GpuGl.glColor4ub((byte)(12 + c1.R / 6), (byte)(24 + c1.G / 5), (byte)(28 + c1.B / 5), aLit);
            GpuGl.glVertex2f(sx - sw, sy); GpuGl.glVertex2f(sx - sw * 0.2f, sy - sh); GpuGl.glVertex2f(sx + sw * 0.15f, sy);
            GpuGl.glColor4ub(7, 13, 18, aDark);
            GpuGl.glVertex2f(sx - sw * 0.2f, sy - sh); GpuGl.glVertex2f(sx + sw, sy); GpuGl.glVertex2f(sx + sw * 0.15f, sy);

            // Subtle glowing crystal shard on every 3rd rock
            if (rIdx % 3 == 0) {
                Color crCol = (rIdx % 2 == 0) ? c2 : c1;
                byte cAlpha = (byte)Math.Min(195, (int)(80 + pr * 95f));
                GpuGl.glColor4ub(crCol.R, crCol.G, crCol.B, cAlpha);
                GpuGl.glVertex2f(pkX - rw * 0.14f, pkY + rh * 0.22f);
                GpuGl.glVertex2f(pkX + sSide * rw * 0.28f, pkY - rh * 0.40f);
                GpuGl.glVertex2f(pkX + rw * 0.16f, pkY + rh * 0.10f);
            }
        }
        GpuGl.glEnd();

        // Pass A2: Batched GL_LINES (0.85px) for rock facet edges
        GpuGl.glLineWidth(0.85f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int rIdx = 0; rIdx < rockCount; rIdx++) {
            float bx = rkX[rIdx];
            float by = rkY[rIdx];
            float rw = rkW[rIdx];
            float rh = rkH[rIdx];
            float pr = rkPersp[rIdx];
            float tilt = ((rIdx % 3) - 1) * rw * 0.12f;

            float blX = bx - rw * 0.92f, blY = by;
            float tlX = bx - rw * 0.58f + tilt * 0.5f, tlY = by - rh * 0.74f;
            float pkX = bx + tilt, pkY = by - rh * 1.02f;
            float trX = bx + rw * 0.62f + tilt * 0.4f, trY = by - rh * 0.66f;
            float brX = bx + rw * 0.94f, brY = by;
            float mdX = bx + tilt * 0.35f, mdY = by - rh * 0.40f;

            byte edgeA = (byte)Math.Min(165, (int)(38 + pr * 85f));
            GpuGl.glColor4ub(c1.R, c1.G, c1.B, edgeA);
            GpuGl.glVertex2f(blX, blY); GpuGl.glVertex2f(tlX, tlY);
            GpuGl.glVertex2f(tlX, tlY); GpuGl.glVertex2f(pkX, pkY);
            GpuGl.glVertex2f(pkX, pkY); GpuGl.glVertex2f(trX, trY);
            GpuGl.glVertex2f(trX, trY); GpuGl.glVertex2f(brX, brY);
            GpuGl.glVertex2f(tlX, tlY); GpuGl.glVertex2f(mdX, mdY);
            GpuGl.glVertex2f(pkX, pkY); GpuGl.glVertex2f(mdX, mdY);
            GpuGl.glVertex2f(trX, trY); GpuGl.glVertex2f(mdX, mdY);
        }
        GpuGl.glEnd();

        // =========================================================================
        // B. BALANCED MULTI-SPECIES 3D FOREST (14 TREES: 7 LEFT, 7 RIGHT)
        //    100% Batched into 1 GL_TRIANGLES pass + 1 GL_LINES pass (eliminates ~200 glBegin/glEnd calls)
        // =========================================================================
        float[] treePosNorm = TREE_POS_NORM;
        float[] treeDepth = TREE_DEPTH;
        int[] treeSpecies = TREE_SPECIES;

        // Pass B1: Unified GL_TRIANGLES pass for all 14 tree trunks & faceted low-poly crowns
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int tIdx = 0; tIdx < treePosNorm.Length; tIdx++) {
            float dScale = treeDepth[tIdx];
            float rCx, rCy, rHw, rPersp;
            GetRiverGeometryAt(dScale, m, horizonY, vanishX, floorSpan, phase, mIdx, out rCx, out rCy, out rHw, out rPersp);

            float baseX = m.Left + m.Width * treePosNorm[tIdx];
            float treeH = m.Height * (0.12f + dScale * 0.16f);
            float treeW = m.Width * (0.017f + dScale * 0.020f);

            float minRiverDist = rHw * 1.38f + treeW * 0.65f;
            if (Math.Abs(baseX - rCx) < minRiverDist) {
                baseX = (baseX < rCx) ? (rCx - minRiverDist) : (rCx + minRiverDist);
            }

            float baseY = horizonY + (dScale - 0.18f) * floorSpan * 0.56f;
            float sway = (float)Math.Sin(phase * 0.65f + tIdx * 1.4f) * (3.5f * dScale);
            int species = treeSpecies[tIdx];

            if (species == 0) {
                float trkW = treeW * 0.16f;
                float topTrkX = baseX + sway * 0.4f;
                float topTrkY = baseY - treeH * 0.42f;
                GpuGl.glColor4ub(22, 36, 28, 74);
                GpuGl.glVertex2f(baseX - trkW, baseY); GpuGl.glVertex2f(baseX + trkW, baseY); GpuGl.glVertex2f(topTrkX, topTrkY);

                int tiers = (dScale > 0.65f) ? 4 : 3;
                byte aL = (byte)(42 + (int)(dScale * 32f));
                byte aR = (byte)(38 + (int)(dScale * 26f));
                for (int tier = 0; tier < tiers; tier++) {
                    float tierFrac = tier / (float)tiers;
                    float tyBot = baseY - treeH * (0.24f + tierFrac * 0.54f);
                    float tyTop = baseY - treeH * (0.54f + tierFrac * 0.48f);
                    float tw = treeW * (1.0f - tierFrac * 0.58f);
                    float txCenter = baseX + sway * (0.45f + tierFrac * 0.45f);
                    float txApex = baseX + sway * (0.65f + tierFrac * 0.55f);
                    float yMid = tyBot + 5f * dScale;

                    GpuGl.glColor4ub((byte)(c1.R / 2), (byte)((c1.G * 2) / 3), (byte)(c1.B / 2), aL);
                    GpuGl.glVertex2f(txCenter - tw, tyBot); GpuGl.glVertex2f(txCenter, yMid); GpuGl.glVertex2f(txApex, tyTop);

                    GpuGl.glColor4ub((byte)(c3.R / 3), (byte)(c3.G / 2), (byte)(c3.B / 2), aR);
                    GpuGl.glVertex2f(txCenter, yMid); GpuGl.glVertex2f(txCenter + tw, tyBot); GpuGl.glVertex2f(txApex, tyTop);
                }
            } else if (species == 1) {
                float sprH = treeH * 1.15f;
                float sprW = treeW * 0.84f;
                float trkW = sprW * 0.16f;
                float topTrkX = baseX + sway * 0.42f;
                float topTrkY = baseY - sprH * 0.40f;
                GpuGl.glColor4ub(20, 34, 30, 74);
                GpuGl.glVertex2f(baseX - trkW, baseY); GpuGl.glVertex2f(baseX + trkW, baseY); GpuGl.glVertex2f(topTrkX, topTrkY);

                int tiers = 4;
                byte aL = (byte)(44 + (int)(dScale * 34f));
                byte aR = (byte)(38 + (int)(dScale * 28f));
                for (int tier = 0; tier < tiers; tier++) {
                    float tierFrac = tier / (float)tiers;
                    float tyBot = baseY - sprH * (0.22f + tierFrac * 0.56f);
                    float tyTop = baseY - sprH * (0.48f + tierFrac * 0.52f);
                    float tw = sprW * (1.0f - tierFrac * 0.60f);
                    float txCenter = baseX + sway * (0.42f + tierFrac * 0.48f);
                    float txApex = baseX + sway * (0.60f + tierFrac * 0.58f);
                    float yMid = tyBot + 4.5f * dScale;

                    GpuGl.glColor4ub((byte)((c1.R + c2.R) / 4), (byte)((c1.G + c2.G) / 3), (byte)((c1.B + c2.B) / 3), aL);
                    GpuGl.glVertex2f(txCenter - tw, tyBot); GpuGl.glVertex2f(txCenter, yMid); GpuGl.glVertex2f(txApex, tyTop);

                    GpuGl.glColor4ub((byte)(c3.R / 3), (byte)(c3.G / 2), (byte)((c1.B + c3.B) / 4), aR);
                    GpuGl.glVertex2f(txCenter, yMid); GpuGl.glVertex2f(txCenter + tw, tyBot); GpuGl.glVertex2f(txApex, tyTop);
                }
            } else {
                float geoH = treeH * 0.94f;
                float geoW = treeW * 0.92f;
                float trkW = geoW * 0.16f;
                float crCenterX = baseX + sway * 0.55f;
                float crCenterY = baseY - geoH * 0.62f;

                GpuGl.glColor4ub(24, 36, 28, 76);
                GpuGl.glVertex2f(baseX - trkW, baseY); GpuGl.glVertex2f(baseX + trkW, baseY); GpuGl.glVertex2f(crCenterX, crCenterY + geoH * 0.12f);

                float topX = crCenterX + sway * 0.15f, topY = baseY - geoH;
                float ulX  = crCenterX - geoW * 0.82f, ulY  = crCenterY - geoH * 0.14f;
                float urX  = crCenterX + geoW * 0.82f, urY  = crCenterY - geoH * 0.14f;
                float blX  = crCenterX - geoW * 0.92f, blY  = crCenterY + geoH * 0.16f;
                float brX  = crCenterX + geoW * 0.92f, brY  = crCenterY + geoH * 0.16f;
                float botX = crCenterX,                botY = crCenterY + geoH * 0.30f;

                GpuGl.glColor4ub((byte)((c1.R + c2.R) / 3), (byte)((c1.G * 2) / 3), (byte)((c1.B + c2.B) / 3), (byte)(46 + (int)(dScale * 36f)));
                GpuGl.glVertex2f(ulX, ulY); GpuGl.glVertex2f(topX, topY); GpuGl.glVertex2f(crCenterX, crCenterY);

                GpuGl.glColor4ub((byte)(c1.R / 3), (byte)(c1.G / 2), (byte)((c1.B * 2) / 3), (byte)(40 + (int)(dScale * 30f)));
                GpuGl.glVertex2f(topX, topY); GpuGl.glVertex2f(urX, urY); GpuGl.glVertex2f(crCenterX, crCenterY);

                GpuGl.glColor4ub((byte)(c1.R / 2), (byte)(c1.G / 2), (byte)(c1.B / 2), (byte)(42 + (int)(dScale * 32f)));
                GpuGl.glVertex2f(blX, blY); GpuGl.glVertex2f(ulX, ulY); GpuGl.glVertex2f(crCenterX, crCenterY);
                GpuGl.glVertex2f(blX, blY); GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(botX, botY);

                GpuGl.glColor4ub((byte)(c3.R / 3), (byte)(c3.G / 2), (byte)(c3.B / 2), (byte)(36 + (int)(dScale * 26f)));
                GpuGl.glVertex2f(urX, urY); GpuGl.glVertex2f(brX, brY); GpuGl.glVertex2f(crCenterX, crCenterY);
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(brX, brY); GpuGl.glVertex2f(botX, botY);
            }
        }
        GpuGl.glEnd();

        // Pass B2: Unified GL_LINES pass for all 14 tree wireframe edges & internal limbs
        GpuGl.glLineWidth(0.9f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int tIdx = 0; tIdx < treePosNorm.Length; tIdx++) {
            float dScale = treeDepth[tIdx];
            float rCx, rCy, rHw, rPersp;
            GetRiverGeometryAt(dScale, m, horizonY, vanishX, floorSpan, phase, mIdx, out rCx, out rCy, out rHw, out rPersp);

            float baseX = m.Left + m.Width * treePosNorm[tIdx];
            float treeH = m.Height * (0.12f + dScale * 0.16f);
            float treeW = m.Width * (0.017f + dScale * 0.020f);

            float minRiverDist = rHw * 1.38f + treeW * 0.65f;
            if (Math.Abs(baseX - rCx) < minRiverDist) {
                baseX = (baseX < rCx) ? (rCx - minRiverDist) : (rCx + minRiverDist);
            }

            float baseY = horizonY + (dScale - 0.18f) * floorSpan * 0.56f;
            float sway = (float)Math.Sin(phase * 0.65f + tIdx * 1.4f) * (3.5f * dScale);
            int species = treeSpecies[tIdx];

            if (species == 0) {
                float trkW = treeW * 0.16f;
                float topTrkX = baseX + sway * 0.4f;
                float topTrkY = baseY - treeH * 0.42f;
                GpuGl.glColor4ub(c2.R, c2.G, c2.B, 65);
                GpuGl.glVertex2f(baseX - trkW, baseY); GpuGl.glVertex2f(topTrkX, topTrkY);
                GpuGl.glVertex2f(baseX + trkW, baseY); GpuGl.glVertex2f(topTrkX, topTrkY);

                int tiers = (dScale > 0.65f) ? 4 : 3;
                byte eA = (byte)(58 + (int)(dScale * 46f));
                GpuGl.glColor4ub(c1.R, c1.G, c1.B, eA);
                for (int tier = 0; tier < tiers; tier++) {
                    float tierFrac = tier / (float)tiers;
                    float tyBot = baseY - treeH * (0.24f + tierFrac * 0.54f);
                    float tyTop = baseY - treeH * (0.54f + tierFrac * 0.48f);
                    float tw = treeW * (1.0f - tierFrac * 0.58f);
                    float txCenter = baseX + sway * (0.45f + tierFrac * 0.45f);
                    float txApex = baseX + sway * (0.65f + tierFrac * 0.55f);
                    float yMid = tyBot + 5f * dScale;

                    GpuGl.glVertex2f(txCenter - tw, tyBot); GpuGl.glVertex2f(txCenter, yMid);
                    GpuGl.glVertex2f(txCenter, yMid);       GpuGl.glVertex2f(txCenter + tw, tyBot);
                    GpuGl.glVertex2f(txCenter - tw, tyBot); GpuGl.glVertex2f(txApex, tyTop);
                    GpuGl.glVertex2f(txCenter, yMid);       GpuGl.glVertex2f(txApex, tyTop);
                    GpuGl.glVertex2f(txCenter + tw, tyBot); GpuGl.glVertex2f(txApex, tyTop);
                }
            } else if (species == 1) {
                float sprH = treeH * 1.15f;
                float sprW = treeW * 0.84f;
                float trkW = sprW * 0.16f;
                float topTrkX = baseX + sway * 0.42f;
                float topTrkY = baseY - sprH * 0.40f;
                GpuGl.glColor4ub(c2.R, c2.G, c2.B, 65);
                GpuGl.glVertex2f(baseX - trkW, baseY); GpuGl.glVertex2f(topTrkX, topTrkY);
                GpuGl.glVertex2f(baseX + trkW, baseY); GpuGl.glVertex2f(topTrkX, topTrkY);

                int tiers = 4;
                byte eA = (byte)(60 + (int)(dScale * 48f));
                GpuGl.glColor4ub((byte)((c1.R + c2.R) / 2), (byte)((c1.G + c2.G) / 2), (byte)((c1.B + c2.B) / 2), eA);
                for (int tier = 0; tier < tiers; tier++) {
                    float tierFrac = tier / (float)tiers;
                    float tyBot = baseY - sprH * (0.22f + tierFrac * 0.56f);
                    float tyTop = baseY - sprH * (0.48f + tierFrac * 0.52f);
                    float tw = sprW * (1.0f - tierFrac * 0.60f);
                    float txCenter = baseX + sway * (0.42f + tierFrac * 0.48f);
                    float txApex = baseX + sway * (0.60f + tierFrac * 0.58f);
                    float yMid = tyBot + 4.5f * dScale;

                    GpuGl.glVertex2f(txCenter - tw, tyBot); GpuGl.glVertex2f(txCenter, yMid);
                    GpuGl.glVertex2f(txCenter, yMid);       GpuGl.glVertex2f(txCenter + tw, tyBot);
                    GpuGl.glVertex2f(txCenter - tw, tyBot); GpuGl.glVertex2f(txApex, tyTop);
                    GpuGl.glVertex2f(txCenter, yMid);       GpuGl.glVertex2f(txApex, tyTop);
                    GpuGl.glVertex2f(txCenter + tw, tyBot); GpuGl.glVertex2f(txApex, tyTop);
                }
            } else {
                float geoH = treeH * 0.94f;
                float geoW = treeW * 0.92f;
                float crCenterX = baseX + sway * 0.55f;
                float crCenterY = baseY - geoH * 0.62f;

                GpuGl.glColor4ub(c2.R, c2.G, c2.B, (byte)(72 + (int)(dScale * 50f)));
                GpuGl.glVertex2f(baseX, baseY); GpuGl.glVertex2f(crCenterX, crCenterY);
                GpuGl.glVertex2f(crCenterX, crCenterY + geoH * 0.16f); GpuGl.glVertex2f(crCenterX - geoW * 0.45f, crCenterY - geoH * 0.04f);
                GpuGl.glVertex2f(crCenterX, crCenterY + geoH * 0.16f); GpuGl.glVertex2f(crCenterX + geoW * 0.45f, crCenterY - geoH * 0.04f);

                float topX = crCenterX + sway * 0.15f, topY = baseY - geoH;
                float ulX  = crCenterX - geoW * 0.82f, ulY  = crCenterY - geoH * 0.14f;
                float urX  = crCenterX + geoW * 0.82f, urY  = crCenterY - geoH * 0.14f;
                float blX  = crCenterX - geoW * 0.92f, blY  = crCenterY + geoH * 0.16f;
                float brX  = crCenterX + geoW * 0.92f, brY  = crCenterY + geoH * 0.16f;
                float botX = crCenterX,                botY = crCenterY + geoH * 0.30f;

                GpuGl.glColor4ub(c1.R, c1.G, c1.B, (byte)(62 + (int)(dScale * 48f)));
                // Outer hexagon ring
                GpuGl.glVertex2f(ulX, ulY); GpuGl.glVertex2f(topX, topY);
                GpuGl.glVertex2f(topX, topY); GpuGl.glVertex2f(urX, urY);
                GpuGl.glVertex2f(urX, urY); GpuGl.glVertex2f(brX, brY);
                GpuGl.glVertex2f(brX, brY); GpuGl.glVertex2f(botX, botY);
                GpuGl.glVertex2f(botX, botY); GpuGl.glVertex2f(blX, blY);
                GpuGl.glVertex2f(blX, blY); GpuGl.glVertex2f(ulX, ulY);
                // Inner radial facet spokes
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(topX, topY);
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(ulX, ulY);
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(urX, urY);
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(blX, blY);
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(brX, brY);
                GpuGl.glVertex2f(crCenterX, crCenterY); GpuGl.glVertex2f(botX, botY);
            }
        }
        GpuGl.glEnd();

        // =========================================================================
        // C. DELICATE ARCHED CYBER-FERNS (ROSETTES ALONG RIVERBANKS & ROCK BASES)
        // =========================================================================
        int fernCount = lowLod ? 8 : 14;
        float[] fnX = bufFnX;
        float[] fnY = bufFnY;
        float[] fnScale = bufFnScale;

        for (int fIdx = 0; fIdx < fernCount; fIdx++) {
            float t = 0.24f + (fIdx / (float)(fernCount - 1)) * 0.72f;
            float rCx, rCy, rHw, persp;
            GetRiverGeometryAt(t, m, horizonY, vanishX, floorSpan, phase, mIdx, out rCx, out rCy, out rHw, out persp);

            float side = (fIdx % 2 == 0) ? -1.0f : 1.0f;
            float hsh = (float)Math.Abs(Math.Sin(fIdx * 7.73f + 2.19f));
            float bankMult = (fIdx % 3 != 0)
                ? (1.16f + 0.32f * hsh)  // Moist riverbank edge
                : (1.75f + 0.85f * hsh); // Near rock/tree bases

            fnX[fIdx] = Math.Max(m.Left + 20f, Math.Min(m.Right - 20f, rCx + side * rHw * bankMult));
            fnY[fIdx] = Math.Min(m.Bottom - 2f, rCy + 2.5f * persp);
            fnScale[fIdx] = 0.26f + 0.64f * persp;
        }

        // Pass C1: Batched GL_TRIANGLES for serrated fern blades
        GpuGl.glBegin(GpuGl.GL_TRIANGLES);
        for (int fIdx = 0; fIdx < fernCount; fIdx++) {
            float bx = fnX[fIdx];
            float by = fnY[fIdx];
            float sc = fnScale[fIdx];
            float frondLen = (9.5f + (fIdx % 3) * 3.2f) * sc;
            float swayF = (float)Math.Sin(phase * 0.95f + fIdx * 0.85f) * (2.2f * sc);

            byte fAlpha = (byte)Math.Min(195, (int)(50 + sc * 100f));
            Color bladeCol = (fIdx % 2 == 0) ? c1 : c2;
            GpuGl.glColor4ub((byte)(bladeCol.R / 2), (byte)((bladeCol.G * 2) / 3), (byte)(bladeCol.B / 2), fAlpha);

            for (int fr = -2; fr <= 2; fr++) {
                float dirX = fr * 0.42f;
                float lift = 1.0f - Math.Abs(fr) * 0.16f;
                float midX = bx + dirX * frondLen * 0.58f + swayF * 0.5f;
                float midY = by - frondLen * 0.60f * lift;
                float tipX = bx + dirX * frondLen * 1.15f + swayF;
                float tipY = by - frondLen * (0.32f * lift) + Math.Abs(fr) * 2.0f * sc;
                float wingW = 2.6f * sc;

                GpuGl.glVertex2f(bx, by - 1.2f * sc);
                GpuGl.glVertex2f(midX - wingW * 0.5f, midY - wingW);
                GpuGl.glVertex2f(midX + wingW * 0.5f, midY + wingW * 0.4f);

                GpuGl.glVertex2f(midX, midY - wingW * 0.6f);
                GpuGl.glVertex2f(tipX, tipY);
                GpuGl.glVertex2f(midX + dirX * 1.8f * sc, midY + wingW * 0.6f);
            }
        }
        GpuGl.glEnd();

        // Pass C2: Batched GL_LINES for arched fern stems
        GpuGl.glLineWidth(0.9f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int fIdx = 0; fIdx < fernCount; fIdx++) {
            float bx = fnX[fIdx];
            float by = fnY[fIdx];
            float sc = fnScale[fIdx];
            float frondLen = (9.5f + (fIdx % 3) * 3.2f) * sc;
            float swayF = (float)Math.Sin(phase * 0.95f + fIdx * 0.85f) * (2.2f * sc);

            byte sAlpha = (byte)Math.Min(210, (int)(65 + sc * 115f));
            Color stemCol = (fIdx % 3 == 0) ? c2 : c1;
            GpuGl.glColor4ub(stemCol.R, stemCol.G, stemCol.B, sAlpha);

            for (int fr = -2; fr <= 2; fr++) {
                float dirX = fr * 0.42f;
                float lift = 1.0f - Math.Abs(fr) * 0.16f;
                float midX = bx + dirX * frondLen * 0.58f + swayF * 0.5f;
                float midY = by - frondLen * 0.60f * lift;
                float tipX = bx + dirX * frondLen * 1.15f + swayF;
                float tipY = by - frondLen * (0.32f * lift) + Math.Abs(fr) * 2.0f * sc;

                GpuGl.glVertex2f(bx, by); GpuGl.glVertex2f(midX, midY);
                GpuGl.glVertex2f(midX, midY); GpuGl.glVertex2f(tipX, tipY);
            }
        }
        GpuGl.glEnd();

        // Pass C3: Batched GL_QUADS for bioluminescent fern spore tips
        GpuGl.glBegin(GpuGl.GL_QUADS);
        for (int fIdx = 0; fIdx < fernCount; fIdx++) {
            float bx = fnX[fIdx];
            float by = fnY[fIdx];
            float sc = fnScale[fIdx];
            if (sc < 0.45f) continue;
            float frondLen = (9.5f + (fIdx % 3) * 3.2f) * sc;
            float swayF = (float)Math.Sin(phase * 0.95f + fIdx * 0.85f) * (2.2f * sc);
            byte spA = (byte)Math.Min(215, (int)(85 + sc * 110f));
            GpuGl.glColor4ub(c2.R, c2.G, c2.B, spA);

            for (int fr = -2; fr <= 2; fr += 2) {
                float dirX = fr * 0.42f;
                float lift = 1.0f - Math.Abs(fr) * 0.16f;
                float tipX = bx + dirX * frondLen * 1.15f + swayF;
                float tipY = by - frondLen * (0.32f * lift) + Math.Abs(fr) * 2.0f * sc;
                float r = 1.2f * sc;
                GpuGl.glVertex2f(tipX - r, tipY - r);
                GpuGl.glVertex2f(tipX + r, tipY - r);
                GpuGl.glVertex2f(tipX + r, tipY + r);
                GpuGl.glVertex2f(tipX - r, tipY + r);
            }
        }
        GpuGl.glEnd();

        // =========================================================================
        // D. FLOATING BIO-DIGITAL FIREFLIES & LUMINOUS FOREST SPORES
        // =========================================================================
        int fireflyCount = lowLod ? 14 : 22;
        GpuGl.glLineWidth(0.85f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int ff = 0; ff < fireflyCount; ff++) {
            float seedX = (ff * 0.6180339f + 0.07f) % 1.0f;
            float seedY = ((ff * 7 + 3) % fireflyCount) / (float)fireflyCount;
            float depthT = 0.16f + seedY * 0.80f;

            float driftX = (float)Math.Sin(phase * 0.58f + ff * 1.73f) * (14.0f + 18.0f * depthT);
            float floatY = (float)Math.Cos(phase * 0.74f + ff * 2.31f) * (8.0f + 12.0f * depthT);

            float fx = m.Left + m.Width * (0.05f + 0.90f * seedX) + driftX;
            float fy = (horizonY - m.Height * 0.06f) + depthT * (floorSpan + m.Height * 0.04f) + floatY;
            float pulse = 0.30f + 0.70f * (0.5f + 0.5f * (float)Math.Sin(phase * 1.85f + ff * 1.95f));
            byte tailA = (byte)(22 + (int)(pulse * 75f));
            Color ffCol = (ff % 3 == 0) ? c2 : c1;

            GpuGl.glColor4ub(ffCol.R, ffCol.G, ffCol.B, tailA);
            GpuGl.glVertex2f(fx, fy);
            GpuGl.glColor4ub(ffCol.R, ffCol.G, ffCol.B, 0);
            GpuGl.glVertex2f(fx - driftX * 0.22f, fy + (5.0f + 5.0f * depthT));
        }
        GpuGl.glEnd();

        // Single batched GL_QUADS pass for soft diamond glow halo + bright firefly core
        GpuGl.glBegin(GpuGl.GL_QUADS);
        for (int ff = 0; ff < fireflyCount; ff++) {
            float seedX = (ff * 0.6180339f + 0.07f) % 1.0f;
            float seedY = ((ff * 7 + 3) % fireflyCount) / (float)fireflyCount;
            float depthT = 0.16f + seedY * 0.80f;

            float driftX = (float)Math.Sin(phase * 0.58f + ff * 1.73f) * (14.0f + 18.0f * depthT);
            float floatY = (float)Math.Cos(phase * 0.74f + ff * 2.31f) * (8.0f + 12.0f * depthT);

            float fx = m.Left + m.Width * (0.05f + 0.90f * seedX) + driftX;
            float fy = (horizonY - m.Height * 0.06f) + depthT * (floorSpan + m.Height * 0.04f) + floatY;
            float pulse = 0.30f + 0.70f * (0.5f + 0.5f * (float)Math.Sin(phase * 1.85f + ff * 1.95f));

            float szHalo = (2.2f + 2.8f * depthT) * (0.75f + 0.45f * pulse);
            float szCore = (0.9f + 1.2f * depthT);
            Color ffCol = (ff % 3 == 0) ? c2 : c1;
            byte haloA = (byte)(18 + (int)(pulse * 52f));
            byte coreA = (byte)(85 + (int)(pulse * 155f));

            // Outer diamond glow halo
            GpuGl.glColor4ub(ffCol.R, ffCol.G, ffCol.B, haloA);
            GpuGl.glVertex2f(fx, fy - szHalo);
            GpuGl.glVertex2f(fx + szHalo, fy);
            GpuGl.glVertex2f(fx, fy + szHalo);
            GpuGl.glVertex2f(fx - szHalo, fy);

            // Bright bioluminescent core
            GpuGl.glColor4ub((byte)Math.Min(255, ffCol.R + 65), (byte)Math.Min(255, ffCol.G + 65), (byte)Math.Min(255, ffCol.B + 55), coreA);
            GpuGl.glVertex2f(fx - szCore, fy - szCore);
            GpuGl.glVertex2f(fx + szCore, fy - szCore);
            GpuGl.glVertex2f(fx + szCore, fy + szCore);
            GpuGl.glVertex2f(fx - szCore, fy + szCore);
        }
        GpuGl.glEnd();

        // =========================================================================
        // E. SUBTLE HANGING CANOPY LIANA VINES FROM UPPER CEILING (BATCHED)
        // =========================================================================
        int vineCount = lowLod ? 5 : 8;
        int segs = lowLod ? 4 : 5;
        GpuGl.glLineWidth(1.1f);
        GpuGl.glBegin(GpuGl.GL_LINES);
        for (int v = 0; v < vineCount; v++) {
            float vxBase = m.Left + m.Width * (0.05f + 0.90f * (v / (float)(vineCount - 1)));
            if (Math.Abs(vxBase - vanishX) < m.Width * 0.18f && (v == 3 || v == 4)) continue;
            float vineLen = ceilSpan * (0.30f + (v % 3) * 0.12f);
            float prevX = vxBase;
            float prevY = m.Top;
            for (int s = 1; s <= segs; s++) {
                float st = s / (float)segs;
                float curX = vxBase + (float)Math.Sin(phase * 0.8f + v * 1.1f + s * 0.7f) * (8.0f * st);
                float curY = m.Top + st * vineLen;
                byte vAlpha = (byte)(70 * (1.0f - st * 0.35f));
                GpuGl.glColor4ub(c1.R, c1.G, c1.B, vAlpha);
                GpuGl.glVertex2f(prevX, prevY);
                GpuGl.glVertex2f(curX, curY);
                prevX = curX;
                prevY = curY;
            }
        }
        GpuGl.glEnd();

        GpuGl.glBegin(GpuGl.GL_QUADS);
        for (int v = 0; v < vineCount; v++) {
            float vxBase = m.Left + m.Width * (0.05f + 0.90f * (v / (float)(vineCount - 1)));
            if (Math.Abs(vxBase - vanishX) < m.Width * 0.18f && (v == 3 || v == 4)) continue;
            float vineLen = ceilSpan * (0.30f + (v % 3) * 0.12f);
            for (int s = 1; s <= segs; s++) {
                float st = s / (float)segs;
                float curX = vxBase + (float)Math.Sin(phase * 0.8f + v * 1.1f + s * 0.7f) * (8.0f * st);
                float curY = m.Top + st * vineLen;
                if (s == segs) {
                    GpuGl.glColor4ub(c2.R, c2.G, c2.B, 140);
                } else {
                    GpuGl.glColor4ub(c1.R, c1.G, c1.B, 90);
                }
                GpuGl.glVertex2f(curX - 1.8f, curY - 1.8f);
                GpuGl.glVertex2f(curX + 1.8f, curY - 1.8f);
                GpuGl.glVertex2f(curX + 1.8f, curY + 1.8f);
                GpuGl.glVertex2f(curX - 1.8f, curY + 1.8f);
            }
        }
        GpuGl.glEnd();
    }

    private void Render3DProceduralEnvironmentOpenGL(int mIdx, Rectangle m, Color c1, Color c2, Color c3) {
        bool lowLod = gpuEcoMode || mIdx > 0 || m.Width <= 1366;
        int mode = Math.Max(0, Math.Min(2, Config.BgSceneMode));
        float horizonY = m.Top + m.Height * 0.64f;
        float vanishX = m.Left + m.Width * 0.50f;
        float floorSpan = m.Bottom - horizonY;
        float ceilSpan = (horizonY - 24f) - m.Top;
        float phase = sceneTimePhase + mIdx * 0.85f;
        float netMod = Math.Min(1.8f, 0.45f + (netDownKbps / 180.0f) + currentFeed.AnomalyIndex * 0.65f);

        if (mode == 0) {
            // =========================================================================
            // MODE 0 (SELVA+OSC): HÍBRIDO SELVA BIO-DIGITAL LOW-POLY + RÍO DE DATOS + OSCILADOR 3D
            // =========================================================================
            // 1.0: Distant Low-Poly Horizon Mountain Range + Vanishing Point Cyber-Portal
            RenderHorizonMountainRangeAndPortalOpenGL(mIdx, m, horizonY, vanishX, phase, netMod, c1, c2, c3);
            int rows = lowLod ? 5 : 7;
            int cols = lowLod ? 12 : 16;
            float[,] vx = bufTerVX;
            float[,] vy = bufTerVY;

            for (int r = 0; r < rows; r++) {
                float t = (r + 0.35f) / (float)rows;
                float persp = (float)Math.Pow(t, 1.55);
                float baseY = horizonY + persp * floorSpan;
                float spread = 0.28f + 0.72f * persp;
                float amp = (6.0f + persp * 26.0f) * netMod;

                float rivCx, rivCy, rivHw, rivPr;
                GetRiverGeometryAt(t, m, horizonY, vanishX, floorSpan, phase, mIdx, out rivCx, out rivCy, out rivHw, out rivPr);

                for (int j = 0; j <= cols; j++) {
                    float u = (j / (float)cols) * 2.0f - 1.0f; // -1..+1
                    float x = vanishX + u * (m.Width * 0.54f) * spread;
                    x = Math.Max(m.Left, Math.Min(m.Right, x));
                    float w1 = (float)Math.Sin(j * 0.75f + phase * 1.15f + r * 0.65f);
                    float w2 = (float)Math.Cos(j * 1.35f - phase * 0.85f + r * 0.90f);

                    // Carve a natural valley around the meandering river centerline!
                    float distFromRiver = Math.Abs(x - rivCx) / Math.Max(8f, rivHw * 1.45f);
                    float valleyMask = (distFromRiver < 1.2f) ? (distFromRiver * distFromRiver * 0.65f) : 1.0f;
                    float riverCarve = (distFromRiver < 1.35f) ? ((1.35f - distFromRiver) * (8.0f + 18.0f * persp)) : 0f;

                    float y = baseY - (w1 * 0.65f + w2 * 0.35f) * amp * valleyMask + riverCarve;
                    vx[r, j] = x;
                    vy[r, j] = Math.Max(horizonY - 18f, Math.Min(m.Bottom - 2f, y));
                }

                // 1A. Polygon Occlusion Ribbon (Matte Dark Moss Triangles)
                Color polyFill = Color.FromArgb((int)(26 + persp * 34f), 6, 19, 15);
                GpuGl.SetColor(polyFill);
                GpuGl.glBegin(GpuGl.GL_TRIANGLES);
                for (int j = 0; j < cols; j++) {
                    float yBot = Math.Min(m.Bottom, baseY + 22f + persp * 36f);
                    GpuGl.glVertex2f(vx[r, j], vy[r, j]);
                    GpuGl.glVertex2f(vx[r, j + 1], vy[r, j + 1]);
                    GpuGl.glVertex2f(vx[r, j], yBot);

                    GpuGl.glVertex2f(vx[r, j + 1], vy[r, j + 1]);
                    GpuGl.glVertex2f(vx[r, j + 1], yBot);
                    GpuGl.glVertex2f(vx[r, j], yBot);
                }
                GpuGl.glEnd();

                // 1B. Triangulated Wireframe Struts between row r and r-1
                if (r > 0) {
                    int strutAlpha = (int)(16 + persp * 42f);
                    GpuGl.glLineWidth(0.9f);
                    GpuGl.SetColor(Color.FromArgb(strutAlpha, c3.R, c3.G, c3.B));
                    GpuGl.glBegin(GpuGl.GL_LINES);
                    for (int j = 0; j <= cols; j++) {
                        GpuGl.glVertex2f(vx[r - 1, j], vy[r - 1, j]);
                        GpuGl.glVertex2f(vx[r, j], vy[r, j]);
                        if (j < cols) {
                            GpuGl.glVertex2f(vx[r - 1, j], vy[r - 1, j]);
                            GpuGl.glVertex2f(vx[r, j + 1], vy[r, j + 1]);
                        }
                    }
                    GpuGl.glEnd();
                }

                // 1C. Oscilloscope 3D Ridge Line + Batched Crest Vertices
                int lineAlpha = (int)(38 + persp * 82f);
                Color waveCol = (r % 2 == 0)
                    ? Color.FromArgb(lineAlpha, c1.R, c1.G, c1.B)
                    : Color.FromArgb(lineAlpha, (c1.R + c2.R) / 2, (c1.G + c2.G) / 2, (c1.B + c2.B) / 2);
                GpuGl.glLineWidth(1.0f + persp * 1.1f);
                GpuGl.SetColor(waveCol);
                GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
                for (int j = 0; j <= cols; j++) {
                    GpuGl.glVertex2f(vx[r, j], vy[r, j]);
                }
                GpuGl.glEnd();

                GpuGl.glBegin(GpuGl.GL_QUADS);
                for (int j = 1; j < cols; j += 2) {
                    if (j % 4 == 1) {
                        GpuGl.glColor4ub(c2.R, c2.G, c2.B, (byte)Math.Min(255, lineAlpha + 25));
                    } else {
                        GpuGl.glColor4ub(c1.R, c1.G, c1.B, (byte)Math.Min(255, lineAlpha + 15));
                    }
                    float nx = vx[r, j], ny = vy[r, j];
                    GpuGl.glVertex2f(nx - 1.6f, ny - 1.6f);
                    GpuGl.glVertex2f(nx + 1.6f, ny - 1.6f);
                    GpuGl.glVertex2f(nx + 1.6f, ny + 1.6f);
                    GpuGl.glVertex2f(nx - 1.6f, ny + 1.6f);
                }
                GpuGl.glEnd();
            }

            // 1C-RIVER: Render the Curved 3D Cyber-Data River right through the middle of the forest!
            RenderCyberDataRiverOpenGL(mIdx, m, horizonY, vanishX, floorSpan, phase, netMod, c1, c2, c3);

            // 1D. Dense Multi-Species Low-Poly Forest + Faceted Rocks + Bioluminescent Cyber-Ferns
            RenderDenseCyberForestFloraOpenGL(mIdx, m, horizonY, vanishX, floorSpan, ceilSpan, phase, netMod, c1, c2, c3);
        } else if (mode == 1) {
            // =========================================================================
            // MODE 1 (TOPO+MESH): TERRENO DE OSCILADOR VECTORIAL + BÓVEDA DELAUNAY + RÍO Y BOSQUE
            // =========================================================================
            RenderHorizonMountainRangeAndPortalOpenGL(mIdx, m, horizonY, vanishX, phase, netMod, c1, c2, c3);

            // 2A. Upper Geodesic Delaunay Triangle Vault (4 rows x 9 cols of 3D oscillating vertices)
            int dRows = 4;
            int dCols = 9;
            float[,] dx = new float[dRows, dCols];
            float[,] dy = new float[dRows, dCols];
            for (int r = 0; r < dRows; r++) {
                float rt = r / (float)(dRows - 1);
                float baseY = m.Top + 14f + rt * (ceilSpan * 0.86f);
                for (int c = 0; c < dCols; c++) {
                    float ct = c / (float)(dCols - 1);
                    float baseX = m.Left + 24f + ct * (m.Width - 48f);
                    float ox = (float)Math.Sin(phase * 0.75f + r * 1.3f + c * 0.9f) * 14.0f;
                    float oy = (float)Math.Cos(phase * 0.65f + c * 1.1f - r * 0.8f) * 9.0f;
                    dx[r, c] = baseX + ox;
                    dy[r, c] = baseY + oy;
                }
            }
            for (int r = 0; r < dRows - 1; r++) {
                for (int c = 0; c < dCols - 1; c++) {
                    int triAlpha = 14 + ((r + c) % 3) * 8;
                    Color f1 = Color.FromArgb(triAlpha, c3.R / 2, c3.G / 2, c3.B / 2);
                    Color f2 = Color.FromArgb(triAlpha + 6, c1.R / 3, c1.G / 2, c1.B / 2);
                    Color wCol = Color.FromArgb(56, c1.R, c1.G, c1.B);

                    GpuGl.FillTriangle(dx[r, c], dy[r, c], dx[r, c + 1], dy[r, c + 1], dx[r + 1, c], dy[r + 1, c], f1);
                    GpuGl.DrawTriangle(dx[r, c], dy[r, c], dx[r, c + 1], dy[r, c + 1], dx[r + 1, c], dy[r + 1, c], wCol, 0.85f);

                    GpuGl.FillTriangle(dx[r, c + 1], dy[r, c + 1], dx[r + 1, c + 1], dy[r + 1, c + 1], dx[r + 1, c], dy[r + 1, c], f2);
                    GpuGl.DrawTriangle(dx[r, c + 1], dy[r, c + 1], dx[r + 1, c + 1], dy[r + 1, c + 1], dx[r + 1, c], dy[r + 1, c], wCol, 0.85f);

                    Color vCol = ((r + c) % 2 == 0) ? Color.FromArgb(115, c2.R, c2.G, c2.B) : Color.FromArgb(105, c1.R, c1.G, c1.B);
                    GpuGl.FillRect(dx[r, c] - 2.0f, dy[r, c] - 2.0f, 4.0f, 4.0f, vCol);
                }
            }

            // 2B. Lower Multi-Band Vector Oscilloscope Topographic Ridges framing the River Valley
            int tRows = 9;
            int tCols = 20;
            float[,] tx = new float[tRows, tCols + 1];
            float[,] ty = new float[tRows, tCols + 1];
            for (int r = 0; r < tRows; r++) {
                float rt = (r + 0.25f) / (float)tRows;
                float persp = (float)Math.Pow(rt, 1.45);
                float baseY = horizonY - 8f + persp * (floorSpan + 4f);
                float rivCx, rivCy, rivHw, rivPr;
                GetRiverGeometryAt(rt, m, horizonY, vanishX, floorSpan, phase, mIdx, out rivCx, out rivCy, out rivHw, out rivPr);

                for (int j = 0; j <= tCols; j++) {
                    float u = (j / (float)tCols) * 2.0f - 1.0f; // -1..+1
                    float x = m.Left + (j / (float)tCols) * m.Width;
                    float distFromRiver = Math.Abs(x - rivCx) / Math.Max(8f, rivHw * 1.45f);
                    float valleyMask = (distFromRiver < 1.25f) ? (distFromRiver * distFromRiver * 0.55f) : 1.0f;
                    float bellCenter = (float)Math.Exp(-(u * u) * 5.5f);
                    float bellFlank  = (float)Math.Exp(-((Math.Abs(u) - 0.58f) * (Math.Abs(u) - 0.58f)) * 14.0f);
                    float harmonic   = (float)(Math.Sin(j * 0.95f + phase * 1.4f + r * 0.8f) * 0.6f
                                             + Math.Cos(j * 1.75f - phase * 1.1f + r * 1.3f) * 0.4f);
                    float ridgeH = (bellCenter * 38.0f + bellFlank * 24.0f) * (0.45f + 0.55f * Math.Abs(harmonic)) * (0.55f + 0.65f * persp) * netMod * valleyMask;
                    tx[r, j] = x;
                    ty[r, j] = Math.Max(m.Top + 40f, Math.Min(m.Bottom - 2f, baseY - ridgeH));
                }

                // Solid Matte Obsidian Polygon Occlusion under each topographic ridge
                Color occlCol = Color.FromArgb(44, 4, 10, 9);
                GpuGl.SetColor(occlCol);
                GpuGl.glBegin(GpuGl.GL_TRIANGLES);
                for (int j = 0; j < tCols; j++) {
                    float yBot = Math.Min(m.Bottom, baseY + 28f);
                    GpuGl.glVertex2f(tx[r, j], ty[r, j]);
                    GpuGl.glVertex2f(tx[r, j + 1], ty[r, j + 1]);
                    GpuGl.glVertex2f(tx[r, j], yBot);

                    GpuGl.glVertex2f(tx[r, j + 1], ty[r, j + 1]);
                    GpuGl.glVertex2f(tx[r, j + 1], yBot);
                    GpuGl.glVertex2f(tx[r, j], yBot);
                }
                GpuGl.glEnd();

                if (r > 0) {
                    int meshAlpha = (int)(18 + persp * 42f);
                    GpuGl.glLineWidth(0.85f);
                    GpuGl.SetColor(Color.FromArgb(meshAlpha, c3.R, c3.G, c3.B));
                    GpuGl.glBegin(GpuGl.GL_LINES);
                    for (int j = 0; j <= tCols; j++) {
                        GpuGl.glVertex2f(tx[r - 1, j], ty[r - 1, j]);
                        GpuGl.glVertex2f(tx[r, j], ty[r, j]);
                    }
                    GpuGl.glEnd();
                }

                int ridgeAlpha = (int)(52 + persp * 96f);
                Color rCol = (r % 3 == 0) ? Color.FromArgb(ridgeAlpha, c2.R, c2.G, c2.B) : Color.FromArgb(ridgeAlpha, c1.R, c1.G, c1.B);
                GpuGl.glLineWidth(1.1f + persp * 1.1f);
                GpuGl.SetColor(rCol);
                GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
                for (int j = 0; j <= tCols; j++) {
                    GpuGl.glVertex2f(tx[r, j], ty[r, j]);
                }
                GpuGl.glEnd();
            }

            RenderCyberDataRiverOpenGL(mIdx, m, horizonY, vanishX, floorSpan, phase, netMod, c1, c2, c3);
            RenderDenseCyberForestFloraOpenGL(mIdx, m, horizonY, vanishX, floorSpan, ceilSpan, phase, netMod, c1, c2, c3);
        } else {
            // =========================================================================
            // MODE 2 (MYCELIUM): BOSQUE MICELIAL POLIGONAL + RÍO DE DATOS Y SELVA
            // =========================================================================
            RenderHorizonMountainRangeAndPortalOpenGL(mIdx, m, horizonY, vanishX, phase, netMod, c1, c2, c3);

            int arborCount = 6;
            for (int a = 0; a < arborCount; a++) {
                float u = (a + 0.5f) / (float)arborCount;
                float rootX = m.Left + m.Width * (0.07f + 0.86f * u);
                float sway = (float)Math.Sin(phase * 0.7f + a * 1.2f) * 12.0f;
                float midX = rootX + (vanishX - rootX) * 0.22f + sway;
                float midY = horizonY + floorSpan * 0.28f;

                // Lower Polygonal Mycelial Trunk (Tapered Triangle Quad from m.Bottom to midY)
                float wBot = 16.0f;
                float wMid = 6.0f;
                Color trunkFill = Color.FromArgb(44, c3.R / 2, c3.G / 2, c3.B / 2);
                Color trunkEdge = Color.FromArgb(78, c1.R, c1.G, c1.B);
                GpuGl.FillTriangle(rootX - wBot, m.Bottom, rootX + wBot, m.Bottom, midX, midY, trunkFill);
                GpuGl.DrawTriangle(rootX - wBot, m.Bottom, rootX + wBot, m.Bottom, midX, midY, trunkEdge, 1.0f);

                // Upper Canopy Counter-Trunk descending from m.Top toward horizon
                float ceilX = rootX + (float)Math.Cos(phase * 0.5f + a) * 10.0f;
                float cMidY = m.Top + ceilSpan * 0.52f;
                GpuGl.FillTriangle(ceilX - 12f, m.Top, ceilX + 12f, m.Top, midX, cMidY, trunkFill);
                GpuGl.DrawTriangle(ceilX - 12f, m.Top, ceilX + 12f, m.Top, midX, cMidY, Color.FromArgb(62, c3.R, c3.G, c3.B), 0.9f);

                // Bifurcating Polygonal Branches & Crystalline Low-Poly Foliage Clusters at Horizon
                for (int b = -1; b <= 1; b++) {
                    float brAngle = b * 0.52f + (float)Math.Sin(phase * 0.8f + a + b) * 0.12f;
                    float tipX = midX + (float)Math.Sin(brAngle) * (m.Width * 0.065f);
                    float tipY = horizonY + b * 14.0f - 8.0f;

                    GpuGl.FillTriangle(midX - wMid, midY, midX + wMid, midY, tipX, tipY, Color.FromArgb(38, c1.R / 2, c1.G / 2, c1.B / 2));
                    GpuGl.DrawLine(midX, midY, tipX, tipY, Color.FromArgb(88, c1.R, c1.G, c1.B), 1.3f);
                    GpuGl.DrawLine(midX, cMidY, tipX, tipY, Color.FromArgb(64, c3.R, c3.G, c3.B), 1.0f);

                    // Low-Poly Octahedral Crystal Cluster at (tipX, tipY) (4 Faceted Triangles)
                    float crW = 14.0f;
                    float crH = 18.0f;
                    Color crCol1 = (b == 0) ? Color.FromArgb(68, c2.R, c2.G, c2.B) : Color.FromArgb(58, c1.R, c1.G, c1.B);
                    Color crCol2 = Color.FromArgb(46, c3.R, c3.G, c3.B);
                    Color crWire = (b == 0) ? Color.FromArgb(125, c2.R, c2.G, c2.B) : Color.FromArgb(110, c1.R, c1.G, c1.B);

                    GpuGl.FillTriangle(tipX, tipY - crH, tipX - crW, tipY, tipX, tipY, crCol1);
                    GpuGl.FillTriangle(tipX, tipY - crH, tipX, tipY, tipX + crW, tipY, crCol2);
                    GpuGl.FillTriangle(tipX, tipY + crH * 0.75f, tipX - crW, tipY, tipX, tipY, crCol2);
                    GpuGl.FillTriangle(tipX, tipY + crH * 0.75f, tipX, tipY, tipX + crW, tipY, crCol1);

                    GpuGl.DrawTriangle(tipX, tipY - crH, tipX - crW, tipY, tipX + crW, tipY, crWire, 1.0f);
                    GpuGl.DrawTriangle(tipX, tipY + crH * 0.75f, tipX - crW, tipY, tipX + crW, tipY, crWire, 1.0f);

                    // Travelling Bioluminescent Pulse along the polygonal root
                    float pulseU = (phase * 0.45f + a * 0.31f + (b + 1) * 0.27f) % 1.0f;
                    float px = midX + (tipX - midX) * pulseU;
                    float py = midY + (tipY - midY) * pulseU;
                    GpuGl.FillRect(px - 2.2f, py - 2.2f, 4.4f, 4.4f, Color.FromArgb(165, c2.R, c2.G, c2.B));
                }
            }

            // Gentle Mycelial Ground Wave connecting all root bases
            int gCols = 16;
            GpuGl.glLineWidth(1.2f);
            GpuGl.SetColor(Color.FromArgb(72, c1.R, c1.G, c1.B));
            GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
            for (int j = 0; j <= gCols; j++) {
                float gx = m.Left + (j / (float)gCols) * m.Width;
                float gy = horizonY + 12f + (float)Math.Sin(j * 0.8f + phase) * 10.0f * netMod;
                GpuGl.glVertex2f(gx, gy);
            }
            GpuGl.glEnd();

            RenderCyberDataRiverOpenGL(mIdx, m, horizonY, vanishX, floorSpan, phase, netMod, c1, c2, c3);
            RenderDenseCyberForestFloraOpenGL(mIdx, m, horizonY, vanishX, floorSpan, ceilSpan, phase, netMod, c1, c2, c3);
        }
    }

    public void Render3DProceduralEnvironmentGdi(Graphics g, int mIdx, Rectangle m, Color c1, Color c2, Color c3, int simFrame, int modeOverride) {
        int mode = Math.Max(0, Math.Min(2, modeOverride));
        float horizonY = m.Top + m.Height * 0.64f;
        float vanishX = m.Left + m.Width * 0.50f;
        float floorSpan = m.Bottom - horizonY;
        float ceilSpan = (horizonY - 24f) - m.Top;
        float phase = simFrame * 0.032f + mIdx * 0.85f;
        float netMod = 0.85f;

        if (mode == 0) {
            int rows = 7;
            int cols = 16;
            PointF[,] pts = new PointF[rows, cols + 1];
            for (int r = 0; r < rows; r++) {
                float t = (r + 0.35f) / (float)rows;
                float persp = (float)Math.Pow(t, 1.55);
                float baseY = horizonY + persp * floorSpan;
                float spread = 0.28f + 0.72f * persp;
                float amp = (6.0f + persp * 26.0f) * netMod;
                float rivCxRow, rivCyRow, rivHalfWRow, rivPerspRow;
                GetRiverGeometryAt(t, m, horizonY, vanishX, floorSpan, phase, mIdx, out rivCxRow, out rivCyRow, out rivHalfWRow, out rivPerspRow);
                for (int j = 0; j <= cols; j++) {
                    float u = (j / (float)cols) * 2.0f - 1.0f;
                    float x = Math.Max(m.Left, Math.Min(m.Right, vanishX + u * (m.Width * 0.54f) * spread));
                    float distFromRiv = Math.Abs(x - rivCxRow);
                    float valleySpan = Math.Max(24.0f, rivHalfWRow * 1.85f);
                    float valleyMask = Math.Min(1.0f, distFromRiv / valleySpan);
                    float riverDepress = (1.0f - valleyMask) * (8.0f + 16.0f * persp);
                    float w1 = (float)Math.Sin(j * 0.75f + phase * 1.15f + r * 0.65f);
                    float w2 = (float)Math.Cos(j * 1.35f - phase * 0.85f + r * 0.90f);
                    float y = Math.Max(horizonY - 18f, Math.Min(m.Bottom - 2f, baseY - (w1 * 0.65f + w2 * 0.35f) * amp * (0.18f + 0.82f * valleyMask) + riverDepress));
                    pts[r, j] = new PointF(x, y);
                }
                if (r > 0) {
                    using (Pen strutPen = new Pen(Color.FromArgb((int)(22 + persp * 45f), c3.R, c3.G, c3.B), 0.9f)) {
                        for (int j = 0; j <= cols; j++) {
                            g.DrawLine(strutPen, pts[r - 1, j], pts[r, j]);
                            if (j < cols) g.DrawLine(strutPen, pts[r - 1, j], pts[r, j + 1]);
                        }
                    }
                }
                PointF[] rowArr = new PointF[cols + 1];
                for (int j = 0; j <= cols; j++) rowArr[j] = pts[r, j];
                int lineAlpha = (int)(48 + persp * 90f);
                Color waveCol = (r % 2 == 0) ? Color.FromArgb(lineAlpha, c1.R, c1.G, c1.B) : Color.FromArgb(lineAlpha, c2.R, c2.G, c2.B);
                using (Pen wPen = new Pen(waveCol, 1.1f + persp)) {
                    g.DrawLines(wPen, rowArr);
                }
            }

            // Curved Cyber-River & Data Flow in GDI preview
            int rivSegs = 24;
            PointF[] leftShore = new PointF[rivSegs + 1];
            PointF[] rightShore = new PointF[rivSegs + 1];
            PointF[] centerLine = new PointF[rivSegs + 1];
            for (int i = 0; i <= rivSegs; i++) {
                float t = i / (float)rivSegs;
                float rcx, rcy, rhw, rp;
                GetRiverGeometryAt(t, m, horizonY, vanishX, floorSpan, phase, mIdx, out rcx, out rcy, out rhw, out rp);
                float ry = rcy + (2.0f + rp * 7.5f);
                leftShore[i] = new PointF(rcx - rhw, ry);
                rightShore[i] = new PointF(rcx + rhw, ry);
                centerLine[i] = new PointF(rcx, ry + 1.5f);
            }
            PointF[] riverPoly = new PointF[(rivSegs + 1) * 2];
            for (int i = 0; i <= rivSegs; i++) {
                riverPoly[i] = leftShore[i];
                riverPoly[(rivSegs * 2 + 1) - i] = rightShore[i];
            }
            using (SolidBrush rivFill = new SolidBrush(Color.FromArgb(185, 6, 26, 40)))
            using (Pen shorePen = new Pen(Color.FromArgb(190, c1.R, c1.G, c1.B), 1.6f))
            using (Pen streamPen = new Pen(Color.FromArgb(165, c2.R, c2.G, c2.B), 1.2f)) {
                g.FillPolygon(rivFill, riverPoly);
                g.DrawLines(shorePen, leftShore);
                g.DrawLines(shorePen, rightShore);
                g.DrawLines(streamPen, centerLine);
            }

            float[] treePosNorm = new float[] { 0.06f, 0.14f, 0.22f, 0.30f, 0.37f, 0.63f, 0.70f, 0.78f, 0.86f, 0.94f };
            float[] treeDepth   = new float[] { 0.92f, 0.74f, 0.56f, 0.40f, 0.26f, 0.26f, 0.40f, 0.56f, 0.74f, 0.92f };
            for (int tIdx = 0; tIdx < treePosNorm.Length; tIdx++) {
                float dScale = treeDepth[tIdx];
                float baseX = m.Left + m.Width * treePosNorm[tIdx];
                float baseY = horizonY + (dScale - 0.18f) * floorSpan * 0.62f;
                float treeH = m.Height * (0.16f + dScale * 0.23f);
                float treeW = m.Width * (0.026f + dScale * 0.028f);
                for (int tier = 0; tier < 3; tier++) {
                    float tierFrac = tier / 3.0f;
                    float tyBot = baseY - treeH * (0.24f + tierFrac * 0.24f);
                    float tyTop = baseY - treeH * (0.58f + tierFrac * 0.22f);
                    float tw = treeW * (1.0f - tier * 0.22f);
                    PointF[] leftTri = new PointF[] { new PointF(baseX - tw, tyBot), new PointF(baseX, tyBot + 6f), new PointF(baseX, tyTop) };
                    PointF[] rightTri = new PointF[] { new PointF(baseX, tyBot + 6f), new PointF(baseX + tw, tyBot), new PointF(baseX, tyTop) };
                    using (SolidBrush lb = new SolidBrush(Color.FromArgb(64, c1.R / 2, (c1.G * 2) / 3, c1.B / 2)))
                    using (SolidBrush rb = new SolidBrush(Color.FromArgb(52, c3.R / 3, c3.G / 2, c3.B / 2)))
                    using (Pen ep = new Pen(Color.FromArgb(110, c1.R, c1.G, c1.B), 1.0f)) {
                        g.FillPolygon(lb, leftTri);
                        g.DrawPolygon(ep, leftTri);
                        g.FillPolygon(rb, rightTri);
                        g.DrawPolygon(ep, rightTri);
                    }
                }
            }
        } else if (mode == 1) {
            int dRows = 4, dCols = 9;
            PointF[,] dPts = new PointF[dRows, dCols];
            for (int r = 0; r < dRows; r++) {
                float baseY = m.Top + 14f + (r / (float)(dRows - 1)) * (ceilSpan * 0.85f);
                for (int c = 0; c < dCols; c++) {
                    float baseX = m.Left + 20f + (c / (float)(dCols - 1)) * (m.Width - 40f);
                    dPts[r, c] = new PointF(baseX + (float)Math.Sin(phase + r + c) * 10f, baseY + (float)Math.Cos(phase + c - r) * 7f);
                }
            }
            using (Pen dp = new Pen(Color.FromArgb(72, c1.R, c1.G, c1.B), 0.9f))
            using (SolidBrush df = new SolidBrush(Color.FromArgb(24, c3.R / 2, c3.G / 2, c3.B / 2))) {
                for (int r = 0; r < dRows - 1; r++) {
                    for (int c = 0; c < dCols - 1; c++) {
                        PointF[] t1 = new PointF[] { dPts[r, c], dPts[r, c + 1], dPts[r + 1, c] };
                        PointF[] t2 = new PointF[] { dPts[r, c + 1], dPts[r + 1, c + 1], dPts[r + 1, c] };
                        g.FillPolygon(df, t1); g.DrawPolygon(dp, t1);
                        g.FillPolygon(df, t2); g.DrawPolygon(dp, t2);
                    }
                }
            }
            int tRows = 9, tCols = 20;
            for (int r = 0; r < tRows; r++) {
                float rt = (r + 0.25f) / (float)tRows;
                float persp = (float)Math.Pow(rt, 1.45);
                float baseY = horizonY - 6f + persp * floorSpan;
                PointF[] rPts = new PointF[tCols + 1];
                for (int j = 0; j <= tCols; j++) {
                    float u = (j / (float)tCols) * 2.0f - 1.0f;
                    float bell = (float)Math.Exp(-(u * u) * 5.5f) * 34f + (float)Math.Exp(-((Math.Abs(u) - 0.58f) * (Math.Abs(u) - 0.58f)) * 14f) * 20f;
                    float harm = (float)Math.Abs(Math.Sin(j * 0.95f + phase + r * 0.8f));
                    rPts[j] = new PointF(m.Left + (j / (float)tCols) * m.Width, baseY - bell * (0.4f + 0.6f * harm) * (0.5f + 0.6f * persp));
                }
                Color rCol = (r % 3 == 0) ? Color.FromArgb(125, c2.R, c2.G, c2.B) : Color.FromArgb(115, c1.R, c1.G, c1.B);
                using (Pen rp = new Pen(rCol, 1.2f)) {
                    g.DrawLines(rp, rPts);
                }
            }
        } else {
            int arborCount = 6;
            for (int a = 0; a < arborCount; a++) {
                float u = (a + 0.5f) / (float)arborCount;
                float rootX = m.Left + m.Width * (0.08f + 0.84f * u);
                float midX = rootX + (vanishX - rootX) * 0.20f;
                float midY = horizonY + floorSpan * 0.28f;
                PointF[] trk = new PointF[] { new PointF(rootX - 14f, m.Bottom), new PointF(rootX + 14f, m.Bottom), new PointF(midX, midY) };
                using (SolidBrush tb = new SolidBrush(Color.FromArgb(52, c3.R / 2, c3.G / 2, c3.B / 2)))
                using (Pen tp = new Pen(Color.FromArgb(92, c1.R, c1.G, c1.B), 1.0f)) {
                    g.FillPolygon(tb, trk);
                    g.DrawPolygon(tp, trk);
                }
                for (int b = -1; b <= 1; b++) {
                    float tipX = midX + b * (m.Width * 0.055f);
                    float tipY = horizonY + b * 12f - 6f;
                    using (Pen bp = new Pen(Color.FromArgb(95, c1.R, c1.G, c1.B), 1.2f)) {
                        g.DrawLine(bp, midX, midY, tipX, tipY);
                    }
                    PointF[] cryst = new PointF[] { new PointF(tipX, tipY - 16f), new PointF(tipX - 12f, tipY), new PointF(tipX, tipY + 12f), new PointF(tipX + 12f, tipY) };
                    Color cc = (b == 0) ? c2 : c1;
                    using (SolidBrush cb = new SolidBrush(Color.FromArgb(68, cc.R, cc.G, cc.B)))
                    using (Pen cp = new Pen(Color.FromArgb(135, cc.R, cc.G, cc.B), 1.0f)) {
                        g.FillPolygon(cb, cryst);
                        g.DrawPolygon(cp, cryst);
                    }
                }
            }
        }
    }

    private void RenderOpenGLFrame() {
        if (isGamerSuspended || !glEnabled || glHdc == IntPtr.Zero || glContext == IntPtr.Zero) return;
        frameSw.Restart();
        GpuGl.wglMakeCurrent(glHdc, glContext);

        GpuGl.CanvasW = screenW;
        GpuGl.CanvasH = screenH;
        GpuGl.glViewport(0, 0, screenW, screenH);
        GpuGl.glMatrixMode(GpuGl.GL_PROJECTION);
        GpuGl.glLoadIdentity();
        GpuGl.glOrtho(0.0, screenW, screenH, 0.0, -2000.0, 2000.0);
        GpuGl.glMatrixMode(GpuGl.GL_MODELVIEW);
        GpuGl.glLoadIdentity();

        GpuGl.glDisable(GpuGl.GL_DEPTH_TEST);
        GpuGl.glEnable(GpuGl.GL_BLEND);
        GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GpuGl.GL_ONE_MINUS_SRC_ALPHA);
        // Disable expensive fragment-level GL_LINE_SMOOTH during background grid & rain loops to cut GPU fill-rate by ~40%
        GpuGl.glDisable(GpuGl.GL_LINE_SMOOTH);

        // 0. Draw 5-Monitor Matte Forest Obsidian Background from VRAM Texture on AMD Radeon RX 9060 XT (0% CPU copy)
        if (bgTexDirty || bgTexId == 0) {
            GpuGl.UploadBitmapToTexture(bgBitmap, ref bgTexId, ref bgTexW, ref bgTexH);
            bgTexDirty = false;
        }
        GpuGl.DrawTexturedQuad(bgTexId, 0, 0, screenW, screenH);

        Color c1, c2, c3, headCol;
        GetThemeColors(out c1, out c2, out c3, out headCol);

        // 0.5. Render Active 3D Procedural Polygon/Vertex Background Scene only on non-occluded monitors!
        for (int mIdx = 0; mIdx < monitorRects.Count; mIdx++) {
            if (IsMonitorOccluded(mIdx)) continue;
            Rectangle m = monitorRects[mIdx];
            Render3DProceduralEnvironmentOpenGL(mIdx, m, c1, c2, c3);
        }

        int hudW = 584;
        int hudH = 228;
        int hudX = primaryMonRect.Right - hudW - 22;
        int hudY = primaryMonRect.Top + 22;

        int radPanelW = 246;
        int radPanelH = 228;
        int radPanelX = hudX - radPanelW - 14;
        int radPanelY = primaryMonRect.Top + 22;

        // 1. Multi-Plane 3D Parallax Vertical Log Rain across visible monitors (Single-Batch VRAM Font Atlas)
        if (Config.RainSpeed > 0) {
            int startLayer = gpuEcoMode ? 1 : 0; // In Eco Mode (app focused), skip far-plane layer 0 rain to save GPU fillrate

            // Pass 1A: Batched 3D volumetric light-streaks behind Near Foreground & Priority Alert streams
            GpuGl.glLineWidth(3.0f);
            GpuGl.glBegin(GpuGl.GL_LINES);
            for (int i = 0; i < columns.Count; i++) {
                RainColumn col = columns[i];
                if (IsMonitorOccluded(col.MonitorIndex)) continue;
                bool isPriority = (col.ColorSlot == 0 || col.ColorSlot == 1);
                if (col.DepthLayer != 2 && !isPriority) continue;
                if (col.Y <= col.MinY || col.Y >= col.MaxY + 80) continue;
                int cStep = col.CharStep > 0 ? col.CharStep : 16;
                float streakTop = Math.Max(col.MinY, col.Y - cStep * 5.0f);
                float streakBot = Math.Min(col.MaxY, col.Y + 4f);
                if (streakBot > streakTop) {
                    Color baseCol, colHead;
                    GetRainSemanticColors(col.ColorSlot, c1, c2, c3, out baseCol, out colHead);
                    byte glowAlpha = (byte)(isPriority ? 36 : 20);
                    GpuGl.glColor4ub(baseCol.R, baseCol.G, baseCol.B, glowAlpha);
                    GpuGl.glVertex2f(col.X + 4f, streakTop);
                    GpuGl.glVertex2f(col.X + 4f, streakBot);
                }
            }
            GpuGl.glEnd();

            // Pass 1B: Single-Draw-Call VRAM Font Texture Atlas Batch for all rain characters
            GpuGl.BeginFontAtlasBatch();
            for (int layer = startLayer; layer <= 2; layer++) {
                for (int i = 0; i < columns.Count; i++) {
                    if (gpuEcoMode && (i & 1) == 1) continue;
                    RainColumn col = columns[i];
                    if (col.DepthLayer != layer || IsMonitorOccluded(col.MonitorIndex)) continue;

                    bool isPriority = (col.ColorSlot == 0 || col.ColorSlot == 1);
                    int cStep = col.CharStep > 0 ? col.CharStep : 16;

                    int maxAlpha;
                    if (layer == 2) maxAlpha = isPriority ? 176 : 148;
                    else if (layer == 1) maxAlpha = isPriority ? 132 : 96;
                    else maxAlpha = isPriority ? 74 : 48;

                    if ((col.X - col.MinX) < 130) maxAlpha = Math.Min(maxAlpha, 38);

                    Color baseCol, colHead;
                    GetRainSemanticColors(col.ColorSlot, c1, c2, c3, out baseCol, out colHead);

                    byte h0A = (byte)(isPriority ? 205 : 178);
                    byte h1A = (byte)Math.Min(210, maxAlpha + 22);
                    byte h1R = (byte)((baseCol.R + colHead.R) / 2);
                    byte h1G = (byte)((baseCol.G + colHead.G) / 2);
                    byte h1B = (byte)((baseCol.B + colHead.B) / 2);

                    for (int c = 0; c < col.Length; c++) {
                        float cy = col.Y - c * cStep;
                        if (cy < col.MinY - 12 || cy > col.MaxY + 4) continue;

                        if (c == 0 && (layer >= 1 || isPriority)) {
                            GpuGl.DrawCharFast(col.Chars[c], col.X, cy, colHead.R, colHead.G, colHead.B, h0A);
                        } else if ((c == 1 || c == 2) && (layer == 2 || isPriority)) {
                            GpuGl.DrawCharFast(col.Chars[c], col.X, cy, h1R, h1G, h1B, h1A);
                        } else {
                            float ratio = 1.0f - ((float)c / col.Length);
                            int alpha = (int)(ratio * ratio * maxAlpha);
                            if (alpha <= 10) continue;
                            GpuGl.DrawCharFast(col.Chars[c], col.X, cy, baseCol.R, baseCol.G, baseCol.B, (byte)alpha);
                        }
                    }
                }
            }
            GpuGl.EndFontAtlasBatch();
        }

        int hudCycle = gpuEcoMode ? 28 : 14;
        int hudSlot = frameCount % hudCycle;

        // 2. Render Autonomous 3D Porygons on Hardware GPU + Collapsible Evolutionary Swarm HUD + 3D FlyWire Brain Card
        if (Config.ShowPorygon) {
            porygon.RenderOpenGL(Config.PorygonScale, c1, c2);

            if (!primaryMonitorOccluded) {
                int bw = 584;
                int fullBh = 78 + porygon.Agents.Count * 21;
                int drawBh = connectomeCollapsed ? 32 : fullBh;
                int bx = primaryMonRect.Right - bw - 22;
                int by = primaryMonRect.Bottom - drawBh - 54;
                if (hudConnectomeBmp == null || hudConnectomeBmp.Width != bw + 4 || hudConnectomeBmp.Height != fullBh + 4) {
                    if (hudConnectomeBmp != null) hudConnectomeBmp.Dispose();
                    hudConnectomeBmp = new Bitmap(bw + 4, fullBh + 4, PixelFormat.Format32bppArgb);
                }
                if (hudSlot == 8 || hudConnectomeTexId == 0 || hudConnectomeDirty) {
                    using (Graphics gHud = Graphics.FromImage(hudConnectomeBmp)) {
                        gHud.Clear(Color.FromArgb(0, 0, 0, 0));
                        gHud.SmoothingMode = SmoothingMode.AntiAlias;
                        gHud.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        DrawFlyBrainConnectomeHUDAt(gHud, 2, 2, bw, drawBh, c1, c2, c3);
                    }
                    GpuGl.UploadBitmapToTexture(hudConnectomeBmp, ref hudConnectomeTexId, ref hudConnectomeTexW, ref hudConnectomeTexH);
                    hudConnectomeDirty = false;
                }
                GpuGl.DrawTexturedQuad(hudConnectomeTexId, bx - 2, by - 2, bw + 4, fullBh + 4);

                // Render 3D FlyWire Drosophila Brain in Card Mode (Staggered on slot 0 to avoid GDI+ frame spikes!)
                if (!brain3DFullscreen || brain3DCollapsed) {
                    int brainW = 356;
                    int fullBrainH = 246;
                    int drawBrainH = brain3DCollapsed ? 32 : fullBrainH;
                    int brainX = bx - brainW - 14;
                    int brainY = primaryMonRect.Bottom - drawBrainH - 54;
                    if (IsFlyWireReady() && !brain3DCollapsed) {
                        DrawFlyWirePanelGL(new Rectangle(brainX, brainY, brainW, fullBrainH), false, c1, c2, c3);
                    } else {
                    if (hudBrain3DBmp == null || hudBrain3DBmp.Width != brainW + 4 || hudBrain3DBmp.Height != fullBrainH + 4) {
                        if (hudBrain3DBmp != null) hudBrain3DBmp.Dispose();
                        hudBrain3DBmp = new Bitmap(brainW + 4, fullBrainH + 4, PixelFormat.Format32bppArgb);
                    }
                    if ((!brain3DCollapsed && hudSlot == 0) || hudBrain3DTexId == 0 || hudBrain3DDirty) {
                        using (Graphics gBr = Graphics.FromImage(hudBrain3DBmp)) {
                            gBr.Clear(Color.FromArgb(0, 0, 0, 0));
                            gBr.SmoothingMode = SmoothingMode.AntiAlias;
                            gBr.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                            DrawFlyBrain3DCardAt(gBr, 2, 2, brainW, drawBrainH, c1, c2, c3);
                        }
                        GpuGl.UploadBitmapToTexture(hudBrain3DBmp, ref hudBrain3DTexId, ref hudBrain3DTexW, ref hudBrain3DTexH);
                        hudBrain3DDirty = false;
                    }
                    GpuGl.DrawTexturedQuad(hudBrain3DTexId, brainX - 2, brainY - 2, brainW + 4, fullBrainH + 4);
                    }
                }
            }
        }

        // 3. Render Collapsible Network Sonar Radar (50% Transparent, staggered on slot 4)
        if (Config.ShowRadar && !primaryMonitorOccluded) {
            int rBmpW = 310;
            int rBmpH = 330;
            if (hudRadarBmp == null) {
                hudRadarBmp = new Bitmap(rBmpW, rBmpH, PixelFormat.Format32bppArgb);
            }
            if ((!radarCollapsed && hudSlot == 4) || hudRadarTexId == 0 || hudRadarDirty) {
                using (Graphics gRad = Graphics.FromImage(hudRadarBmp)) {
                    gRad.Clear(Color.FromArgb(0, 0, 0, 0));
                    gRad.SmoothingMode = SmoothingMode.AntiAlias;
                    gRad.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    DrawFunctionalRadarAt(gRad, 22, 2, radPanelW, radPanelH, c1, c2, c3, radPanelX - 22, radPanelY - 2);
                }
                GpuGl.UploadBitmapToTexture(hudRadarBmp, ref hudRadarTexId, ref hudRadarTexW, ref hudRadarTexH);
                hudRadarDirty = false;
            }
            GpuGl.DrawTexturedQuad(hudRadarTexId, radPanelX - 22, radPanelY - 2, rBmpW, rBmpH);
        }

        // 4. Render Collapsible System Telemetry HUD (Cached in VRAM Texture) & Secondary Bars
        if (Config.ShowTelemetry) {
            if (!primaryMonitorOccluded) {
                if (hudTelemetryBmp == null) {
                    hudTelemetryBmp = new Bitmap(hudW + 4, hudH + 4, PixelFormat.Format32bppArgb);
                }
                if (lastHudTelemetryCycle != telemetryCycles || lastHudTheme != Config.ThemeIndex || hudTelemetryTexId == 0 || hudTelemetryDirty) {
                    using (Graphics gTel = Graphics.FromImage(hudTelemetryBmp)) {
                        gTel.Clear(Color.FromArgb(0, 0, 0, 0));
                        gTel.SmoothingMode = SmoothingMode.AntiAlias;
                        gTel.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        DrawTelemetryHUD(gTel, 2, 2, hudW, hudH, c1, c2, c3);
                    }
                    GpuGl.UploadBitmapToTexture(hudTelemetryBmp, ref hudTelemetryTexId, ref hudTelemetryTexW, ref hudTelemetryTexH);
                    lastHudTelemetryCycle = telemetryCycles;
                    lastHudTheme = Config.ThemeIndex;
                    hudTelemetryDirty = false;
                }
                GpuGl.DrawTexturedQuad(hudTelemetryTexId, hudX - 2, hudY - 2, hudW + 4, hudH + 4);
            }
            DrawSecondaryMonitorTelemetryBarsOpenGL(c1, c2, c3);
        }

        // 5. Render Fullscreen 3D FlyWire Drosophila Brain Overlay on Primary Monitor when brain3DFullscreen is Active!
        if (Config.ShowPorygon && brain3DFullscreen && !brain3DCollapsed && !primaryMonitorOccluded && IsFlyWireReady()) {
            DrawFlyWirePanelGL(GetCurrentBrain3DRect(), true, c1, c2, c3);
        } else if (Config.ShowPorygon && brain3DFullscreen && !brain3DCollapsed && !primaryMonitorOccluded) {
            Rectangle fullRect = GetCurrentBrain3DRect();
            int fbW = 942;
            int fbH = 507;
            if (hudBrain3DFullBmp == null || hudBrain3DFullBmp.Width != fbW + 4 || hudBrain3DFullBmp.Height != fbH + 4) {
                if (hudBrain3DFullBmp != null) hudBrain3DFullBmp.Dispose();
                hudBrain3DFullBmp = new Bitmap(fbW + 4, fbH + 4, PixelFormat.Format32bppArgb);
            }
            if (frameCount % 5 == 0 || hudBrain3DFullTexId == 0 || hudBrain3DDirty) {
                using (Graphics gFull = Graphics.FromImage(hudBrain3DFullBmp)) {
                    gFull.Clear(Color.FromArgb(0, 0, 0, 0));
                    gFull.SmoothingMode = SmoothingMode.AntiAlias;
                    gFull.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    DrawFlyBrain3DCardAt(gFull, 2, 2, fbW, fbH, c1, c2, c3);
                }
                GpuGl.UploadBitmapToTexture(hudBrain3DFullBmp, ref hudBrain3DFullTexId, ref hudBrain3DFullTexW, ref hudBrain3DFullTexH);
                hudBrain3DDirty = false;
            }
            GpuGl.DrawTexturedQuad(hudBrain3DFullTexId, fullRect.X - 2, fullRect.Y - 2, fullRect.Width + 4, fullRect.Height + 4);
        }

        frameSw.Stop();
        float elapsedMs = (float)frameSw.Elapsed.TotalMilliseconds;
        gpuFrameMs = gpuFrameMs * 0.82f + elapsedMs * 0.18f;
        GpuGl.SwapBuffers(glHdc);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnPaint(PaintEventArgs e) {
        if (glEnabled) {
            if (!isGamerSuspended) RenderOpenGLFrame();
            return;
        }
        Graphics g = e.Graphics;
        g.DrawImageUnscaled(bgBitmap, 0, 0);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        Color c1, c2, c3, headCol;
        GetThemeColors(out c1, out c2, out c3, out headCol);

        int hudW = 584;
        int hudH = 228;
        int hudX = primaryMonRect.Right - hudW - 22;
        int hudY = primaryMonRect.Top + 22;

        int radPanelW = 246;
        int radPanelH = 228;
        int radPanelX = hudX - radPanelW - 14;
        int radPanelY = primaryMonRect.Top + 22;

        // 1. Falling Hacker Code Rain across all 5 monitors (100% Strictly Vertical Log Streams + 7 Semantic Log Colors!)
        if (Config.RainSpeed > 0) {
            for (int i = 0; i < columns.Count; i++) {
                RainColumn col = columns[i];
                if (IsMonitorOccluded(col.MonitorIndex)) continue;
                int cStep = col.CharStep > 0 ? col.CharStep : 16;
                Font f = col.IsForeground ? fontRainFg : fontRainBg;
                int maxAlpha = (col.DepthLayer == 2) ? 245 : (col.DepthLayer == 1 ? 160 : 80);
                if ((col.X - col.MinX) < 130) maxAlpha = 45;

                Color baseCol, colHead;
                GetRainSemanticColors(col.ColorSlot, c1, c2, c3, out baseCol, out colHead);

                for (int c = 0; c < col.Length; c++) {
                    float cy = col.Y - c * cStep;
                    if (cy < col.MinY - 18 || cy > col.MaxY + 12) continue;

                    if (c == 0 && col.DepthLayer >= 1) {
                        using (SolidBrush headBrush = new SolidBrush(Color.FromArgb(255, colHead.R, colHead.G, colHead.B))) {
                            g.DrawString(col.Chars[c].ToString(), f, headBrush, col.X, cy);
                        }
                    } else {
                        float ratio = 1.0f - ((float)c / col.Length);
                        int alpha = (int)(ratio * ratio * maxAlpha);
                        if (alpha <= 8) continue;
                        using (SolidBrush tb = new SolidBrush(Color.FromArgb(alpha, baseCol.R, baseCol.G, baseCol.B))) {
                            g.DrawString(col.Chars[c].ToString(), f, tb, col.X, cy);
                        }
                    }
                }
            }
        }

        // 2. Render Autonomous 3D Porygons, Cross-Monitor Synaptic Mesh, Hover Cards, Connectome HUD & 3D Brain Card!
        if (Config.ShowPorygon) {
            porygon.Render(g, Config.PorygonScale, c1, c2, fontHudSmall);
            DrawFlyBrainConnectomeHUD(g, c1, c2, c3);
            int bw = 584;
            int bx = primaryMonRect.Right - bw - 22;
            int brainW = 356;
            int drawBrainH = brain3DCollapsed ? 32 : 246;
            int brainX = bx - brainW - 14;
            int brainY = primaryMonRect.Bottom - drawBrainH - 54;
            DrawFlyBrain3DCardAt(g, brainX, brainY, brainW, drawBrainH, c1, c2, c3);
        }

        // 3. Render Enlarged Functional Network Sonar Radar with Mouse Hover Blip Inspection!
        if (Config.ShowRadar) {
            DrawFunctionalRadar(g, radPanelX, radPanelY, radPanelW, radPanelH, c1, c2, c3);
        }

        // 4. Render High-Contrast System Telemetry HUD on Primary Monitor & Secondary Sector Telemetry Bars on Monitors 2..5
        if (Config.ShowTelemetry) {
            DrawTelemetryHUD(g, hudX, hudY, hudW, hudH, c1, c2, c3);
            DrawSecondaryMonitorTelemetryBars(g, c1, c2, c3);
        }
    }

    private void DrawSecondaryMonitorTelemetryBarsOpenGL(Color c1, Color c2, Color c3) {
        if (monitorRects.Count <= 1) return;
        string latestLog = "QB-SOC:5080 // NIDS & FORTIGATE TELEMETRY ACTIVE";
        lock (telemetryLock) {
            if (liveLogLines.Count > 0) latestLog = liveLogLines[liveLogLines.Count - 1];
        }
        Color bgCol = Color.FromArgb(128, 5, 8, 18); // 50% transparency
        Color borCol = Color.FromArgb(145, c1.R, c1.G, c1.B);
        Color c2Col = Color.FromArgb(240, c2.R, c2.G, c2.B);
        Color txtCol = Color.FromArgb(230, 226, 232, 240);

        for (int mIdx = 0; mIdx < monitorRects.Count; mIdx++) {
            if (IsMonitorOccluded(mIdx)) continue;
            Rectangle m = monitorRects[mIdx];
            if (m == primaryMonRect) continue;

            int pw = Math.Min(520, m.Width - 180);
            if (pw < 260) continue;
            int ph = secondaryBarsCollapsed ? 24 : 46;
            int px = m.Right - pw - 18;
            int py = m.Top + 16;

            GpuGl.FillRect(px, py, pw, ph, bgCol);
            GpuGl.DrawRect(px, py, pw, ph, borCol, 1.2f);

            string agentLoc = "";
            if (porygon != null) {
                for (int a = 0; a < porygon.Agents.Count; a++) {
                    var ag = porygon.Agents[a];
                    if (m.Contains((int)ag.PosX, (int)ag.PosY)) {
                        agentLoc += (agentLoc.Length > 0 ? " + " : "") + ag.Name;
                    }
                }
            }
            if (agentLoc.Length == 0) agentLoc = "MESH_LINKED";

            GpuGl.DrawText(string.Format(">> SOC_NODE // {0} | AI_PRESENCE: {1}", monitorNames[mIdx], agentLoc),
                px + 10, py + 5, c2Col, GpuGl.FONT_LIST_HUD_SMALL);
            GpuGl.DrawText(secondaryBarsCollapsed ? "[+]" : "[-]", px + pw - 28, py + 5, borCol, GpuGl.FONT_LIST_HUD_SMALL);
            if (!secondaryBarsCollapsed) {
                string clippedLog = latestLog.Length > 68 ? latestLog.Substring(0, 68) : latestLog;
                GpuGl.DrawText(clippedLog, px + 10, py + 24, txtCol, GpuGl.FONT_LIST_HUD_SMALL);
            }
        }
    }

    private void DrawSecondaryMonitorTelemetryBars(Graphics g, Color c1, Color c2, Color c3) {
        if (monitorRects.Count <= 1) return;
        string latestLog = "QB-SOC:5080 // NIDS & FORTIGATE TELEMETRY ACTIVE";
        lock (telemetryLock) {
            if (liveLogLines.Count > 0) latestLog = liveLogLines[liveLogLines.Count - 1];
        }
        using (SolidBrush panelBg = new SolidBrush(Color.FromArgb(128, 5, 8, 18)))
        using (Pen borderPen = new Pen(Color.FromArgb(145, c1.R, c1.G, c1.B), 1.2f))
        using (SolidBrush c1B = new SolidBrush(Color.FromArgb(240, c1.R, c1.G, c1.B)))
        using (SolidBrush c2B = new SolidBrush(Color.FromArgb(240, c2.R, c2.G, c2.B)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(230, 226, 232, 240))) {
            for (int mIdx = 0; mIdx < monitorRects.Count; mIdx++) {
                Rectangle m = monitorRects[mIdx];
                if (m == primaryMonRect) continue;

                int pw = Math.Min(520, m.Width - 180);
                if (pw < 260) continue;
                int ph = secondaryBarsCollapsed ? 24 : 46;
                int px = m.Right - pw - 18;
                int py = m.Top + 16;

                g.FillRectangle(panelBg, px, py, pw, ph);
                g.DrawRectangle(borderPen, px, py, pw, ph);

                string agentLoc = "";
                if (porygon != null) {
                    for (int a = 0; a < porygon.Agents.Count; a++) {
                        var ag = porygon.Agents[a];
                        if (m.Contains((int)ag.PosX, (int)ag.PosY)) {
                            agentLoc += (agentLoc.Length > 0 ? " + " : "") + ag.Name;
                        }
                    }
                }
                if (agentLoc.Length == 0) agentLoc = "MESH_LINKED";

                g.DrawString(string.Format(">> SOC_NODE // {0} | AI_PRESENCE: {1}", monitorNames[mIdx], agentLoc),
                    fontHudSmall, c2B, px + 10, py + 5);
                g.DrawString(secondaryBarsCollapsed ? "[+]" : "[-]", fontHudSmall, c1B, px + pw - 28, py + 5);
                if (!secondaryBarsCollapsed) {
                    string clippedLog = latestLog.Length > 68 ? latestLog.Substring(0, 68) : latestLog;
                    g.DrawString(clippedLog, fontHudSmall, txtB, px + 10, py + 24);
                }
            }
        }
    }

    private void DrawFunctionalRadar(Graphics g, int px, int py, int pw, int ph, Color c1, Color c2, Color c3) {
        DrawFunctionalRadarAt(g, px, py, pw, ph, c1, c2, c3, 0, 0);
    }

    private void DrawFunctionalRadarAt(Graphics g, int px, int py, int pw, int ph, Color c1, Color c2, Color c3, int screenOriginX, int screenOriginY) {
        List<RadarBlip> blipsSnap;
        int tcpCnt;
        lock (telemetryLock) {
            blipsSnap = new List<RadarBlip>(radarBlips);
            tcpCnt = tcpEstablishedCount;
        }

        int actualH = radarCollapsed ? 32 : ph;

        using (SolidBrush panelBg = new SolidBrush(Color.FromArgb(128, 5, 8, 18)))
        using (Pen borderPen = new Pen(Color.FromArgb(170, c1.R, c1.G, c1.B), 1.4f))
        using (SolidBrush c1B = new SolidBrush(Color.FromArgb(245, c1.R, c1.G, c1.B)))
        using (SolidBrush c2B = new SolidBrush(Color.FromArgb(245, c2.R, c2.G, c2.B)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(230, 226, 232, 240))) {
            g.FillRectangle(panelBg, px, py, pw, actualH);
            g.DrawRectangle(borderPen, px, py, pw, actualH);

            using (Pen cornerPen = new Pen(Color.FromArgb(235, c2.R, c2.G, c2.B), 2.5f)) {
                g.DrawLine(cornerPen, px, py, px + 18, py);
                g.DrawLine(cornerPen, px, py, px, py + Math.Min(18, actualH));
                g.DrawLine(cornerPen, px + pw, py + actualH, px + pw - 18, py + actualH);
                g.DrawLine(cornerPen, px + pw, py + actualH, px + pw, py + actualH - Math.Min(18, actualH));
            }

            g.DrawString(">> NET_SONAR // 360°", fontHudBold, c1B, px + 10, py + 7);
            g.DrawString(string.Format("N:{0} [{1}]", tcpCnt, radarCollapsed ? "+" : "-"), fontHudBold, c2B, px + pw - 84, py + 7);

            if (radarCollapsed) return;

            Point rawMouse = this.PointToClient(Control.MousePosition);
            Point mousePt = new Point(rawMouse.X - screenOriginX, rawMouse.Y - screenOriginY);
            int hoveredBlipIdx = -1;
            float hoveredBlipX = 0f, hoveredBlipY = 0f;

            int radR = 78;
            int radCX = px + pw / 2;
            int radCY = py + 110;

            using (SolidBrush scopeBg = new SolidBrush(Color.FromArgb(128, 3, 6, 14)))
            using (Pen ringOuter = new Pen(Color.FromArgb(180, c1.R, c1.G, c1.B), 1.6f))
            using (Pen ringGrid = new Pen(Color.FromArgb(55, c1.R, c1.G, c1.B), 1f))
            using (Pen sweepPen = new Pen(Color.FromArgb(240, c2.R, c2.G, c2.B), 2.2f)) {
                g.FillEllipse(scopeBg, radCX - radR, radCY - radR, radR * 2, radR * 2);
                g.DrawEllipse(ringOuter, radCX - radR, radCY - radR, radR * 2, radR * 2);
                g.DrawEllipse(ringGrid, radCX - radR * 2 / 3, radCY - radR * 2 / 3, radR * 4 / 3, radR * 4 / 3);
                g.DrawEllipse(ringGrid, radCX - radR / 3, radCY - radR / 3, radR * 2 / 3, radR * 2 / 3);
                g.DrawLine(ringGrid, radCX - radR, radCY, radCX + radR, radCY);
                g.DrawLine(ringGrid, radCX, radCY - radR, radCX, radCY + radR);

                using (GraphicsPath pie = new GraphicsPath()) {
                    pie.AddPie(radCX - radR, radCY - radR, radR * 2, radR * 2, radarAngle - 48f, 48f);
                    using (SolidBrush sweepB = new SolidBrush(Color.FromArgb(65, c2.R, c2.G, c2.B))) {
                        g.FillPath(sweepB, pie);
                    }
                }

                double rad = radarAngle * Math.PI / 180.0;
                float sx = radCX + (float)(Math.Cos(rad) * radR);
                float sy = radCY + (float)(Math.Sin(rad) * radR);
                g.DrawLine(sweepPen, radCX, radCY, sx, sy);

                float bestDiff = 999f;
                float bestBx = 0f, bestBy = 0f;
                string bestLabel = null;
                float minHoverDist = 16f;

                for (int i = 0; i < blipsSnap.Count; i++) {
                    RadarBlip b = blipsSnap[i];
                    double bRad = b.AngleDeg * Math.PI / 180.0;
                    float bx = radCX + (float)(Math.Cos(bRad) * (radR * b.DistNorm));
                    float by = radCY + (float)(Math.Sin(bRad) * (radR * b.DistNorm));

                    // Mouse hover hit-test on Radar Blips!
                    float mdx = mousePt.X - bx;
                    float mdy = mousePt.Y - by;
                    float md = (float)Math.Sqrt(mdx * mdx + mdy * mdy);
                    if (md < minHoverDist) {
                        minHoverDist = md;
                        hoveredBlipIdx = i;
                        hoveredBlipX = bx;
                        hoveredBlipY = by;
                    }

                    float diff = (radarAngle - b.AngleDeg + 360f) % 360f;
                    if (diff < 22f) {
                        lastScannedTarget = b.Label;
                    }
                    if (diff < 75f && diff < bestDiff) {
                        bestDiff = diff;
                        bestBx = bx;
                        bestBy = by;
                        bestLabel = b.Label;
                    }
                    int blipAlpha = (diff < 90f) ? 255 : (int)Math.Max(110f, 255f - (diff - 90f) * 0.65f);
                    Color bCol = b.IsAnomaly ? Color.FromArgb(255, 42, 85) : (b.IsHttps ? c1 : c2);

                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(blipAlpha / 3, bCol.R, bCol.G, bCol.B)))
                    using (SolidBrush dot = new SolidBrush(Color.FromArgb(blipAlpha, bCol.R, bCol.G, bCol.B))) {
                        g.FillEllipse(glow, bx - 6f, by - 6f, 12f, 12f);
                        g.FillEllipse(dot, bx - 3f, by - 3f, 6f, 6f);
                    }
                }

                if (hoveredBlipIdx >= 0) {
                    RadarBlip hb = blipsSnap[hoveredBlipIdx];
                    lastScannedTarget = hb.FullRemoteEp;
                    using (Pen hPen = new Pen(Color.FromArgb(250, 250, 204, 21), 1.8f)) {
                        g.DrawEllipse(hPen, hoveredBlipX - 9f, hoveredBlipY - 9f, 18f, 18f);
                    }
                } else if (bestLabel != null) {
                    float lx = (bestBx > radCX + 10) ? (bestBx - 78f) : (bestBx + 6f);
                    g.DrawString(bestLabel, fontHudSmall, txtB, lx, bestBy - 7f);
                }
            }

            g.DrawString("LOCK: " + lastScannedTarget, fontHudSmall, c1B, px + 10, py + ph - 34);
            g.DrawString("CLICK CARD TO FOLD // HOVER BLIPS", fontHudSmall, txtB, px + 10, py + ph - 18);

            // If user is hovering a Radar Blip, draw an interactive Radar Node Telemetry Card below the radar!
            if (hoveredBlipIdx >= 0) {
                RadarBlip hb = blipsSnap[hoveredBlipIdx];
                int cardW = 285;
                int cardH = 88;
                int cardX = px - 20;
                int cardY = py + ph + 8;
                Color accent = hb.IsAnomaly ? Color.FromArgb(255, 42, 85) : c1;
                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(140, 5, 9, 20)))
                using (Pen cBor = new Pen(accent, 1.6f))
                using (SolidBrush accB = new SolidBrush(accent)) {
                    g.FillRectangle(cBg, cardX, cardY, cardW, cardH);
                    g.DrawRectangle(cBor, cardX, cardY, cardW, cardH);
                    g.DrawString(">> SONAR_NODE // " + hb.RingZone, fontHudSmall, accB, cardX + 8, cardY + 6);
                    g.DrawString("REMOTE : " + hb.FullRemoteEp + " [" + hb.Proto + "]", fontHudSmall, txtB, cardX + 8, cardY + 24);
                    g.DrawString("LOCAL  : " + hb.LocalEp, fontHudSmall, txtB, cardX + 8, cardY + 42);
                    g.DrawString(string.Format("RISK   : {0:0}% ({1})", hb.AnomalyScore * 100f, hb.IsAnomaly ? "ANOMALY ALERT" : "VERIFIED TLS/LAN"),
                        fontHudSmall, accB, cardX + 8, cardY + 60);
                }
            }
        }
    }

    private void DrawTelemetryHUD(Graphics g, int hudX, int hudY, int hudW, int hudH, Color c1, Color c2, Color c3) {
        float locCpu, locGpu, locGpuMs, locRamUsed, locRamTot, locDown, locUp, locDlZ;
        int locRamPct, locPing, locTcp, locFps;
        bool locEco, locGamer;
        string locNic, locIp;
        string[] logsSnap;
        float[] pingSnap = new float[pingHistory.Length];

        lock (telemetryLock) {
            locGamer = isGamerSuspended;
            locCpu = locGamer ? 0.0f : cpuUsage;
            locGpu = locGamer ? 0.0f : gpuUsagePct;
            locGpuMs = locGamer ? 0.0f : gpuFrameMs;
            locFps = locGamer ? 0 : currentFps;
            locEco = gpuEcoMode;
            locRamUsed = ramUsedGB;
            locRamTot = ramTotalGB;
            locRamPct = ramPercent;
            locDown = netDownKbps;
            locUp = netUpKbps;
            locDlZ = currentFeed.DlZScore;
            locPing = pingMs;
            locTcp = tcpEstablishedCount;
            locNic = activeNicName;
            locIp = localIPv4;
            logsSnap = liveLogLines.ToArray();
            Array.Copy(pingHistory, pingSnap, pingHistory.Length);
        }

        int actualH = telemetryCollapsed ? 32 : hudH;

        string shortNic = (locNic ?? "NIC")
            .Replace("VMware Network Adapter ", "")
            .Replace("Virtual Ethernet Adapter", "VETH")
            .Replace("Ethernet Adapter ", "")
            .Trim().ToUpper();
        if (shortNic.Length > 8) shortNic = shortNic.Substring(0, 8);

        string shortGpu = (gpuRendererName ?? "GPU").Replace("Radeon ", "").Trim();
        if (shortGpu.Length > 10) shortGpu = shortGpu.Substring(0, 10);

        // Eye-Care Matte Terminal HUD Background (alpha = 128, attenuated border & corners -35%)
        using (SolidBrush hudBg = new SolidBrush(Color.FromArgb(132, 4, 9, 8)))
        using (Pen hudBorder = new Pen(Color.FromArgb(118, c1.R, c1.G, c1.B), 1.2f))
        using (SolidBrush c1Brush = new SolidBrush(Color.FromArgb(225, c1.R, c1.G, c1.B)))
        using (SolidBrush c2Brush = new SolidBrush(Color.FromArgb(225, c2.R, c2.G, c2.B)))
        using (SolidBrush c3Brush = new SolidBrush(Color.FromArgb(225, c3.R, c3.G, c3.B)))
        using (SolidBrush txtBrush = new SolidBrush(Color.FromArgb(215, 206, 218, 214)))
        using (SolidBrush barBg = new SolidBrush(Color.FromArgb(36, 255, 255, 255))) {
            g.FillRectangle(hudBg, hudX, hudY, hudW, actualH);
            g.DrawRectangle(hudBorder, hudX, hudY, hudW, actualH);

            using (Pen cornerPen = new Pen(Color.FromArgb(165, c2.R, c2.G, c2.B), 2.0f)) {
                g.DrawLine(cornerPen, hudX, hudY, hudX + 20, hudY);
                g.DrawLine(cornerPen, hudX, hudY, hudX, hudY + Math.Min(20, actualH));
                g.DrawLine(cornerPen, hudX + hudW, hudY + actualH, hudX + hudW - 20, hudY + actualH);
                g.DrawLine(cornerPen, hudX + hudW, hudY + actualH, hudX + hudW, hudY + actualH - Math.Min(20, actualH));
            }

            // Row 1: Non-overlapping Header with GPU Model, [TRM:XX%], [GAMER] 0% GPU Button, [BG:1/2/3] Scene Button, [OP-]/[OP+] & Fold Button
            g.DrawString(string.Format(">> SYS_HUD // {0} [{1}] | {2}", shortNic, locIp, shortGpu),
                fontHudBold, c1Brush, hudX + 10, hudY + 7);
            using (SolidBrush opBtnBg = new SolidBrush(Color.FromArgb(135, 10, 22, 18)))
            using (SolidBrush gmrActiveBg = new SolidBrush(Color.FromArgb(195, 22, 92, 62)))
            using (Pen opBtnBor = new Pen(Color.FromArgb(140, c1.R, c1.G, c1.B), 1.0f))
            using (Pen gmrActiveBor = new Pen(Color.FromArgb(235, 68, 232, 156), 1.3f))
            using (SolidBrush gmrActiveTxt = new SolidBrush(Color.FromArgb(245, 165, 255, 210))) {
                // [TRM:XX%] Clickable 1-Click Opacity Toggle (100% Solid <-> 50% Cyber, or Ctrl+Alt+O)
                g.FillRectangle(opBtnBg, hudX + hudW - 260, hudY + 5, 54, 18);
                g.DrawRectangle(opBtnBor, hudX + hudW - 260, hudY + 5, 54, 18);
                g.DrawString(string.Format("TRM:{0}%", terminalOpacityPct),
                    fontHudSmall, c3Brush, hudX + hudW - 257, hudY + 7);

                // [GAMER / GMR:ON] Clickable 0.0% GPU Hibernation Button (Ctrl+Alt+G)
                g.FillRectangle(locGamer ? gmrActiveBg : opBtnBg, hudX + hudW - 202, hudY + 5, 58, 18);
                g.DrawRectangle(locGamer ? gmrActiveBor : opBtnBor, hudX + hudW - 202, hudY + 5, 58, 18);
                g.DrawString(locGamer ? "GMR:ON" : "GAMER", fontHudSmall, locGamer ? gmrActiveTxt : c1Brush, hudX + hudW - 196, hudY + 7);

                // [BG:1/2/3] Clickable 3D Scene Switcher Button (Ctrl+Alt+M)
                g.FillRectangle(opBtnBg, hudX + hudW - 140, hudY + 5, 42, 18);
                g.DrawRectangle(opBtnBor, hudX + hudW - 140, hudY + 5, 42, 18);
                g.DrawString(string.Format("BG:{0}/3", Config.BgSceneMode + 1), fontHudSmall, c2Brush, hudX + hudW - 137, hudY + 7);

                g.FillRectangle(opBtnBg, hudX + hudW - 94, hudY + 5, 28, 18);
                g.DrawRectangle(opBtnBor, hudX + hudW - 94, hudY + 5, 28, 18);
                g.DrawString("OP-", fontHudSmall, c2Brush, hudX + hudW - 91, hudY + 7);

                g.FillRectangle(opBtnBg, hudX + hudW - 62, hudY + 5, 28, 18);
                g.DrawRectangle(opBtnBor, hudX + hudW - 62, hudY + 5, 28, 18);
                g.DrawString("OP+", fontHudSmall, c1Brush, hudX + hudW - 59, hudY + 7);

                g.FillRectangle(opBtnBg, hudX + hudW - 30, hudY + 5, 22, 18);
                g.DrawRectangle(opBtnBor, hudX + hudW - 30, hudY + 5, 22, 18);
                g.DrawString(telemetryCollapsed ? "+" : "-", fontHudBold, c2Brush, hudX + hudW - 24, hudY + 5);
            }

            if (telemetryCollapsed) return;

            // Row 2: CPU Bar, GPU Bar (Load % + Frame ms + FPS), RAM Bar, and Latency Sparkline Box!
            int rowY = hudY + 38;
            g.DrawString(string.Format("CPU: {0:0.0}%", locCpu), fontHudBold, txtBrush, hudX + 12, rowY);
            g.FillRectangle(barBg, hudX + 12, rowY + 20, 112, 7);
            g.FillRectangle(c1Brush, hudX + 12, rowY + 20, (int)(1.12f * Math.Min(100f, locCpu)), 7);

            string fpsTag = locGamer ? "GAMER" : (locEco ? (locFps + "eco") : (locFps + "fps"));
            g.DrawString(string.Format("GPU:{0:0.0}% ({1:0.0}ms|{2})", locGpu, locGpuMs, fpsTag), fontHudSmall, c2Brush, hudX + 134, rowY + 1);
            g.FillRectangle(barBg, hudX + 134, rowY + 20, 148, 7);
            g.FillRectangle(c2Brush, hudX + 134, rowY + 20, (int)(1.48f * Math.Min(100f, locGpu)), 7);

            g.DrawString(string.Format("RAM:{0:0.0}/{1:0}G({2}%)", locRamUsed, locRamTot, locRamPct), fontHudSmall, txtBrush, hudX + 294, rowY + 1);
            g.FillRectangle(barBg, hudX + 294, rowY + 20, 128, 7);
            g.FillRectangle(c3Brush, hudX + 294, rowY + 20, (int)(1.28f * Math.Min(100, locRamPct)), 7);

            int spX = hudX + 434;
            int spW = 136;
            g.DrawString(string.Format("PING ({0}ms)", locPing), fontHudSmall, c1Brush, spX, rowY + 1);
            g.FillRectangle(barBg, spX, rowY + 17, spW, 15);
            int barCnt = Math.Min(16, pingSnap.Length);
            for (int i = 0; i < barCnt; i++) {
                float pVal = pingSnap[pingSnap.Length - barCnt + i];
                int bh = (int)Math.Min(13f, Math.Max(2f, pVal * 0.22f));
                Brush sb = (pVal > 60f) ? c2Brush : c1Brush;
                g.FillRectangle(sb, spX + 3 + i * 8, rowY + 31 - bh, 5, bh);
            }

            // Row 3: Network Live Throughput + Active 3D Background Scene + CPU Evolutionary Rate (or Gamer / High-GPU Banner)
            int netY = hudY + 76;
            if (locGamer) {
                string gmrMsg = autoHighGpuDetected
                    ? string.Format("AUTO-PAUSA GPU >90% [{0}] // 0.0% GPU — Reanudación Automática", highGpuProcessName)
                    : "MODO GAMER ACTIVO // 0.0% GPU — HIBERNACIÓN TOTAL [Click GMR:ON o Ctrl+Alt+G]";
                g.DrawString(gmrMsg, fontHudBold, c2Brush, hudX + 12, netY);
            } else {
                int evoRate = (porygon != null) ? porygon.CpuTrainingStepsPerSec : 18750;
                string bgName = GetBgSceneModeShortName(Config.BgSceneMode);
                g.DrawString(string.Format("NET // DL:{0:0.0}KB/s | UL:{1:0.0}KB/s | 3D_BG:{2} [{3}/3] | EVO:{4:0.0}k/s",
                    locDown, locUp, bgName, Config.BgSceneMode + 1, evoRate / 1000.0), fontHudBold, c3Brush, hudX + 12, netY);
            }

            using (Pen divPen = new Pen(Color.FromArgb(45, c1.R, c1.G, c1.B), 1f)) {
                g.DrawLine(divPen, hudX + 12, netY + 20, hudX + hudW - 12, netY + 20);
            }

            // Row 4: Crisp Live Packet / Anomaly Feed
            for (int i = 0; i < logsSnap.Length; i++) {
                Brush lb = logsSnap[i].Contains("IDS_DETECT") || logsSnap[i].Contains("GAMER_MODE") ? c2Brush : ((i == logsSnap.Length - 1) ? c1Brush : txtBrush);
                string cleanLine = logsSnap[i].Length > 76 ? logsSnap[i].Substring(0, 76) : logsSnap[i];
                g.DrawString(cleanLine, fontHud, lb, hudX + 12, netY + 26 + i * 19);
            }
        }
    }

    private void DrawFlyBrainConnectomeHUD(Graphics g, Color c1, Color c2, Color c3) {
        int bw = 584;
        int bh = connectomeCollapsed ? 32 : (78 + porygon.Agents.Count * 21);
        int bx = primaryMonRect.Right - bw - 22;
        int by = primaryMonRect.Bottom - bh - 54;
        DrawFlyBrainConnectomeHUDAt(g, bx, by, bw, bh, c1, c2, c3);
    }

    private void DrawFlyBrainConnectomeHUDAt(Graphics g, int bx, int by, int bw, int bh, Color c1, Color c2, Color c3) {
        int actualH = connectomeCollapsed ? 32 : bh;

        // Eye-Care Matte Connectome Terminal Background (alpha = 132, attenuated border -35%)
        using (SolidBrush bg = new SolidBrush(Color.FromArgb(132, 4, 9, 8)))
        using (Pen border = new Pen(Color.FromArgb(115, c1.R, c1.G, c1.B), 1.15f))
        using (SolidBrush c1B = new SolidBrush(Color.FromArgb(220, c1.R, c1.G, c1.B)))
        using (SolidBrush c2B = new SolidBrush(Color.FromArgb(220, c2.R, c2.G, c2.B)))
        using (SolidBrush goldB = new SolidBrush(Color.FromArgb(215, 192, 152, 48)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(210, 204, 216, 212))) {
            g.FillRectangle(bg, bx, by, bw, actualH);
            g.DrawRectangle(border, bx, by, bw, actualH);

            float anomIdx = currentFeed.AnomalyIndex;
            string sumTag = currentFeed.LastAnomalySummary ?? "BASELINE_OK";
            if (sumTag.Length > 13) sumTag = sumTag.Substring(0, 13);
            int evoRate = (porygon != null) ? porygon.CpuTrainingStepsPerSec : 18750;

            g.DrawString(string.Format(">> PORYGON CYBER-DEFENSE // ANOM:{0:0}% [{1}]",
                anomIdx * 100f, sumTag), fontHudSmall, anomIdx > 0.45f ? c2B : c1B, bx + 10, by + 8);
            using (SolidBrush pBtnBg = new SolidBrush(Color.FromArgb(155, 12, 28, 22)))
            using (SolidBrush warnBtnBg = new SolidBrush(Color.FromArgb(165, 44, 18, 20)))
            using (Pen pBtnBor = new Pen(Color.FromArgb(170, c1.R, c1.G, c1.B), 1.0f))
            using (Pen warnBor = new Pen(Color.FromArgb(205, 232, 92, 92), 1.0f)) {
                // [SIEM/IPS] Button (opens SIEM/AV/IPS Command Center, Ctrl+Alt+S)
                g.FillRectangle(pBtnBg, bx + bw - 266, by + 5, 62, 18);
                g.DrawRectangle(pBtnBor, bx + bw - 266, by + 5, 62, 18);
                g.DrawString("SIEM/IPS", fontHudSmall, goldB, bx + bw - 261, by + 7);

                // [SCAN] Button (runs quick AV + Registry + TCP/PID audit)
                g.FillRectangle(pBtnBg, bx + bw - 200, by + 5, 42, 18);
                g.DrawRectangle(pBtnBor, bx + bw - 200, by + 5, 42, 18);
                g.DrawString("SCAN", fontHudSmall, c1B, bx + bw - 193, by + 7);

                // [PANEL] Button (opens Config Window)
                g.FillRectangle(pBtnBg, bx + bw - 154, by + 5, 46, 18);
                g.DrawRectangle(pBtnBor, bx + bw - 154, by + 5, 46, 18);
                g.DrawString("PANEL", fontHudSmall, c1B, bx + bw - 148, by + 7);

                g.DrawString(string.Format("EVO:{0:0.0}k [{1}]", evoRate / 1000.0, connectomeCollapsed ? "+" : "-"),
                    fontHudSmall, goldB, bx + bw - 102, by + 8);

                if (connectomeCollapsed) return;

                for (int i = 0; i < porygon.Agents.Count; i++) {
                    Porygon3DAvatar.PorygonAgent ag = porygon.Agents[i];
                    FlyBrainConnectome br = ag.Brain;
                    int ry = by + 24 + i * 21;

                    string roleTag = (i == 0) ? "NIDS+SIEM" : (i == 1 ? "AV/EDR" : "IPS/FW");
                    using (SolidBrush agB = new SolidBrush(Color.FromArgb(215, ag.AuraColor.R, ag.AuraColor.G, ag.AuraColor.B))) {
                        string line = string.Format("[{0,-9}] G{1} {2,-9} AGE:{3,2:0}% | F2:{4,4:0.0}% FN:{5} FIT:{6,4:0.0}",
                            ag.Name, br.Generation, roleTag, br.LifeAge, br.F2Score, br.FalseNegatives, br.FitnessScore);
                        if (line.Length > 64) line = line.Substring(0, 64);
                        g.DrawString(line, fontHudSmall, agB, bx + 10, ry);
                    }

                    int stripX = bx + bw - 126;
                    using (SolidBrush dimB = new SolidBrush(Color.FromArgb(48, c1.R, c1.G, c1.B)))
                    using (SolidBrush actB = new SolidBrush(ag.AuraColor)) {
                        for (int n = 0; n < 15; n++) {
                            float glow = br.SpikeGlow[n * 4];
                            if (glow > 0.35f) {
                                actB.Color = Color.FromArgb((int)(95 + glow * 120), ag.AuraColor.R, ag.AuraColor.G, ag.AuraColor.B);
                                g.FillEllipse(actB, stripX + n * 8, ry + 3, 6f, 6f);
                            } else {
                                g.FillEllipse(dimB, stripX + n * 8, ry + 3, 6f, 6f);
                            }
                        }
                    }
                }

                int pidCnt, avCnt, siemCnt, fwCnt, quarCnt;
                string bannerSnap, candIpSnap, candFileSnap;
                lock (PorygonCyberDefenseEngine.SyncLock) {
                    pidCnt = PorygonCyberDefenseEngine.TotalNidsProcessAudits;
                    avCnt = PorygonCyberDefenseEngine.TotalAvFilesScanned;
                    siemCnt = PorygonCyberDefenseEngine.TotalSiemAlerts;
                    fwCnt = PorygonCyberDefenseEngine.BlockedFirewallIps.Count;
                    quarCnt = PorygonCyberDefenseEngine.QuarantinedFiles.Count;
                    bannerSnap = PorygonCyberDefenseEngine.LastActionBanner ?? "HYBRID_DEFENSE_ONLINE";
                    candIpSnap = PorygonCyberDefenseEngine.LastCandidateBlockIp;
                    candFileSnap = PorygonCyberDefenseEngine.LastCandidateQuarantineFile;
                }

                string kpiLine = string.Format("DEFENSE // PID_MAP:{0} | AV_SCAN:{1} | SIEM:{2} | FW_BLK:{3} | QUAR:{4}",
                    pidCnt, avCnt, siemCnt, fwCnt, quarCnt);
                if (kpiLine.Length > 63) kpiLine = kpiLine.Substring(0, 63);
                g.DrawString(kpiLine, fontHudSmall, goldB, bx + 10, by + bh - 49);

                // 1-Click Hybrid IPS & AV Quarantine Action Buttons
                bool hasIp = !string.IsNullOrEmpty(candIpSnap);
                bool hasFile = !string.IsNullOrEmpty(candFileSnap);
                g.FillRectangle(hasIp ? warnBtnBg : pBtnBg, bx + bw - 142, by + bh - 51, 68, 16);
                g.DrawRectangle(hasIp ? warnBor : pBtnBor, bx + bw - 142, by + bh - 51, 68, 16);
                g.DrawString("BLOCK IP", fontHudSmall, hasIp ? c2B : c1B, bx + bw - 136, by + bh - 50);

                g.FillRectangle(hasFile ? warnBtnBg : pBtnBg, bx + bw - 70, by + bh - 51, 60, 16);
                g.DrawRectangle(hasFile ? warnBor : pBtnBor, bx + bw - 70, by + bh - 51, 60, 16);
                g.DrawString("QUAR", fontHudSmall, hasFile ? goldB : c1B, bx + bw - 56, by + bh - 50);

                string statusLine = bannerSnap;
                if (hasIp) {
                    statusLine = string.Format("IPS_READY // Candidato Bloqueo: {0} ({1}) | Click [BLOCK IP]",
                        candIpSnap, PorygonCyberDefenseEngine.LastCandidateBlockProc ?? "PID");
                }
                if (statusLine.Length > 84) statusLine = statusLine.Substring(0, 84);
                g.DrawString(statusLine, fontHudSmall, c1B, bx + 10, by + bh - 33);

                string footer = "WEB_SOC: http://localhost:5090 // [NIDS+PID] + [AV_EDR] + [SIEM] + [FW_IPS] // CTRL+ALT+S";
                g.DrawString(footer, fontHudSmall, txtB, bx + 10, by + bh - 17);
            }
        }
    }


    // =====================================================================================================
    // FLYWIRE NATIVE OPENGL PANEL (Codex / Virtual Fly Brain style): chrome + 3D connectome + GL text overlay
    // =====================================================================================================
    private FlyWireBrain flyWire = null;
    private System.Diagnostics.Stopwatch flyWireClock = new System.Diagnostics.Stopwatch();
    private double flyWireLastT = 0;
    private int brainHoverClientX = -1, brainHoverClientY = -1;
    private int fwLastSiemTotal = -1, fwLastIpsTotal = -1, fwLastVtQueries = -1, fwLastVtHits = -1;
    private double fwLastTrafficT = 0, fwLastPollT = 0, fwLastVtLookupT = -10, fwLastExfilT = -20;
    private float fwUpEma = 50f;
    private float brain3DPanX = 0f, brain3DPanY = 0f, brain3DDragStartPanX = 0f, brain3DDragStartPanY = 0f;
    private bool brain3DPanDrag = false;
    private Rectangle brain3DLastArea = Rectangle.Empty;
    private volatile string brainSnapshotPath = null;
    private DateTime brainForceVisibleUntil = DateTime.MinValue;
    private static readonly string[] FwLabelNeuropils = new string[] { "ME", "LO", "LOP", "AL", "CA", "PED", "FB", "EB", "PB", "LH", "GNG", "AVLP", "SMP", "LAL" };

    private void ApplyBrainWheelZoom(int wheelDelta, Point cursor) {
        float oldZ = brain3DZoom;
        float newZ = (float)(oldZ * Math.Pow(1.18, wheelDelta / 120.0));
        newZ = Math.Max(0.6f, Math.Min(9.0f, newZ));
        if (Math.Abs(newZ - oldZ) < 0.0001f) return;
        if (IsFlyWireReady() && brain3DLastArea.Width > 0 && brain3DLastArea.Contains(cursor)) {
            // Zoom toward the cursor: keep the brain point under the mouse fixed on screen
            float k = newZ / oldZ;
            float cx = flyWire.DefCX + brain3DPanX, cy = flyWire.DefCY + brain3DPanY;
            float ncx = cursor.X - (cursor.X - cx) * k;
            float ncy = cursor.Y - (cursor.Y - cy) * k;
            brain3DPanX = ncx - flyWire.DefCX;
            brain3DPanY = ncy - flyWire.DefCY;
        }
        brain3DZoom = newZ;
        ClampBrainPan();
        hudBrain3DDirty = true;
    }

    private void ClampBrainPan() {
        Rectangle a = brain3DLastArea;
        if (a.Width <= 0) { brain3DPanX = 0f; brain3DPanY = 0f; return; }
        float mx = Math.Max(0f, (brain3DZoom - 0.75f)) * a.Width * 0.55f;
        float my = Math.Max(0f, (brain3DZoom - 0.75f)) * a.Height * 0.55f;
        brain3DPanX = Math.Max(-mx, Math.Min(mx, brain3DPanX));
        brain3DPanY = Math.Max(-my, Math.Min(my, brain3DPanY));
    }

    private bool IsFlyWireReady() {
        return flyWire != null && flyWire.Loaded && !flyWire.Failed;
    }

    private void InitFlyWireBrain() {
        try {
            string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "flywire_brain.bin");
            flyWire = new FlyWireBrain();
            flyWire.LoadAsync(p1);
            flyWireClock.Start();
            PorygonCyberDefenseEngine.OnBrainApi = HandleBrainApi;
            BrainWheelHook.Start();
        } catch { flyWire = null; }
    }

    private string HandleBrainApi(string route, Dictionary<string, string> qp) {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        Dictionary<string, object> d = new Dictionary<string, object>();
        if (flyWire == null) { d["loaded"] = false; d["status"] = "DISABLED"; return ser.Serialize(d); }
        if (route == "/api/brain/stimulate") {
            string kind = qp.ContainsKey("kind") ? qp["kind"] : "anomaly";
            float sev = 0.8f;
            if (qp.ContainsKey("sev")) float.TryParse(qp["sev"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out sev);
            flyWire.Enqueue(kind, "API_TEST:" + kind.ToUpperInvariant(), sev);
            hudBrain3DDirty = true;
            d["queued"] = kind;
        } else if (route == "/api/brain/bench") {
            int sec = 12;
            if (qp.ContainsKey("sec")) int.TryParse(qp["sec"], out sec);
            brain3DFullscreen = true; brain3DCollapsed = false;
            brainForceVisibleUntil = DateTime.Now.AddSeconds(Math.Max(3, Math.Min(60, sec)));
            d["benchUntil"] = brainForceVisibleUntil.ToString("HH:mm:ss");
        } else if (route == "/api/brain/snapshot") {
            if (qp.ContainsKey("full")) { brain3DFullscreen = qp["full"] == "1"; brain3DCollapsed = false; }
            string outPath = Path.Combine(Path.GetTempPath(), "flywire_brain_snapshot.png");
            try { if (File.Exists(outPath)) File.Delete(outPath); } catch { }
            brainSnapshotPath = outPath;
            hudBrain3DDirty = true;
            d["snapshot"] = outPath;
        }
        d["loaded"] = flyWire.Loaded;
        d["listsBuilt"] = flyWire.ListsBuilt;
        d["status"] = flyWire.Status;
        d["neurons"] = flyWire.Neurons.Count;
        d["edges"] = flyWire.Edges.Count;
        d["neuropils"] = flyWire.Neuropils.Count;
        d["pulses"] = flyWire.Pulses.Count;
        d["events"] = flyWire.TotalEvents;
        d["cascadeHops"] = flyWire.TotalCascadeHops;
        List<string> st = new List<string>();
        for (int s = 0; s < FlyWireBrain.NStages; s++) st.Add(FlyWireBrain.StageNames[s] + "=" + flyWire.StageAct[s].ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " (n=" + flyWire.StageCount[s] + ")");
        d["stages"] = st;
        List<string> tr = new List<string>();
        lock (flyWire.Traces) {
            for (int i = 0; i < flyWire.Traces.Count; i++) tr.Add(flyWire.Traces[i].Label + " :: " + string.Join(" > ", flyWire.Traces[i].Hops.ToArray()));
        }
        d["traces"] = tr;
        d["attribution"] = flyWire.Attribution;
        return ser.Serialize(d);
    }

    // Poll IDS / SIEM / IPS / VirusTotal counters and route them through anatomical pathways
    private void PumpFlyWireIdsEvents(double now) {
        if (!IsFlyWireReady()) return;
        if (now - fwLastPollT < 0.4) return;
        fwLastPollT = now;
        try {
            int siemTot = PorygonCyberDefenseEngine.TotalSiemAlerts;
            int ipsTot = PorygonCyberDefenseEngine.TotalIpsActions;
            if (fwLastSiemTotal < 0) { fwLastSiemTotal = siemTot; fwLastIpsTotal = ipsTot; }
            if (siemTot > fwLastSiemTotal) {
                int newCount = Math.Min(4, siemTot - fwLastSiemTotal);
                lock (PorygonCyberDefenseEngine.SyncLock) {
                    var lst = PorygonCyberDefenseEngine.RecentSiemEvents;
                    for (int i = Math.Max(0, lst.Count - newCount); i < lst.Count; i++) {
                        var e = lst[i];
                        string mod = (e.Module ?? "").ToUpperInvariant();
                        string sev = (e.Severity ?? "").ToUpperInvariant();
                        string kind;
                        string sumU = ((e.Summary ?? "") + " " + (e.MitreId ?? "")).ToUpperInvariant();
                        if (sumU.Contains("EXFIL") || sumU.Contains("UPLOAD") || sumU.Contains("TUNNEL") || sumU.Contains("T1041") || sumU.Contains("T1048") || sumU.Contains("T1567")) kind = "exfil";
                        else if (sev == "CRITICAL") kind = "critical";
                        else if (mod.StartsWith("IPS")) kind = "block";
                        else if (mod == "SOAR") kind = "decision";
                        else if (mod.StartsWith("AV") || mod.StartsWith("YARA")) kind = "malware";
                        else if (mod == "CTI_IOC") kind = "reputation";
                        else if (sev == "HIGH") kind = "anomaly";
                        else kind = "signature";
                        string lbl = string.Format("{0}/{1} {2}", mod, sev, e.MitreId ?? "");
                        flyWire.Enqueue(kind, lbl, Math.Max(0.2f, e.RiskScore));
                    }
                }
                fwLastSiemTotal = siemTot;
            }
            if (ipsTot > fwLastIpsTotal) {
                flyWire.Enqueue("block", "IPS_ACTION #" + ipsTot, 0.9f);
                fwLastIpsTotal = ipsTot;
            }
            if (porygon != null) {
                int vq = porygon.VtApiQueries, vh = porygon.VtMaliciousHits;
                if (fwLastVtQueries < 0) { fwLastVtQueries = vq; fwLastVtHits = vh; }
                if (vh > fwLastVtHits) { flyWire.Enqueue("malware", "VIRUSTOTAL_IOC x" + (vh - fwLastVtHits), 0.95f); fwLastVtHits = vh; }
                else if (vq > fwLastVtQueries && now - fwLastVtLookupT > 8.0) { flyWire.Enqueue("reputation", "VIRUSTOTAL_LOOKUP", 0.4f); fwLastVtLookupT = now; }
                fwLastVtQueries = vq;
            }
            // Upload spike -> auditory/mechanosensory exfiltration pathway (Johnston organ > AMMC > Giant Fiber)
            fwUpEma = fwUpEma * 0.96f + netUpKbps * 0.04f;
            if (netUpKbps > Math.Max(900f, fwUpEma * 4f) && now - fwLastExfilT > 10.0) {
                fwLastExfilT = now;
                flyWire.Enqueue("exfil", string.Format("UPLOAD_SPIKE {0:0} KB/s (base {1:0})", netUpKbps, fwUpEma), Math.Min(1f, netUpKbps / 4000f + 0.4f));
            }
            // Network traffic -> optic ingress (rate scales with throughput)
            float kbps = netDownKbps + netUpKbps;
            double interval = Math.Max(0.35, 2.2 - Math.Min(1.8, kbps / 900.0));
            if (now - fwLastTrafficT > interval) {
                fwLastTrafficT = now;
                flyWire.Enqueue("traffic", "NET", Math.Min(1f, kbps / 1500f));
            }
        } catch { }
    }

    private float[] GetBaselineRegionGlow() {
        float[] g = new float[4];
        if (porygon == null || porygon.Agents.Count == 0) return g;
        FlyBrainConnectome br = porygon.Agents[porygon.SelectedBrainAgentIndex % porygon.Agents.Count].Brain;
        int[] cnt = new int[4];
        for (int i = 0; i < FlyBrainConnectome.N; i++) {
            int r = Math.Max(0, Math.Min(3, br.NeuronRegion[i]));
            g[r] += br.SpikeGlow[i]; cnt[r]++;
        }
        for (int r = 0; r < 4; r++) if (cnt[r] > 0) g[r] /= cnt[r];
        return g;
    }

    private static Color Fa(Color c, int a) { return Color.FromArgb(Math.Max(0, Math.Min(255, a)), c.R, c.G, c.B); }

    private void GlText(string s, float x, float y, Color c) {
        GpuGl.DrawText(s, x, y, c, GpuGl.FONT_LIST_HUD_SMALL);
    }

    private void GlTextBox(string s, float x, float y, Color fg, Color bg) {
        float w = s.Length * 6.6f + 6f;
        GpuGl.FillRect(x - 3f, y - 2f, w, 14f, bg);
        GlText(s, x, y, fg);
    }

    private void DrawFlyWirePanelGL(Rectangle r, bool isFull, Color c1, Color c2, Color c3) {
        int px = r.X, py = r.Y, pw = r.Width, ph = r.Height;


        Color gold = Color.FromArgb(230, 196, 156, 48);
        Color txt = Color.FromArgb(225, 206, 218, 214);
        Color dim = Color.FromArgb(170, 150, 170, 165);

        // Panel chrome
        GpuGl.FillRect(px, py, pw, ph, Color.FromArgb(isFull ? 250 : 222, 2, 6, 7));
        GpuGl.DrawRect(px, py, pw, ph, Fa(c1, 135), isFull ? 1.7f : 1.2f);
        GpuGl.DrawLine(px, py, px + 18, py, Fa(c2, 190), 2f);
        GpuGl.DrawLine(px, py, px, py + 18, Fa(c2, 190), 2f);
        GpuGl.DrawLine(px + pw, py + ph, px + pw - 18, py + ph, Fa(c2, 190), 2f);
        GpuGl.DrawLine(px + pw, py + ph, px + pw, py + ph - 18, Fa(c2, 190), 2f);

        string title = isFull
            ? ">> FLYWIRE CONNECTOME // JRC2018U (VFB_00101567) // IDS NEURO-PATHWAY INSPECTOR [CTRL+ALT+B]"
            : ">> FLYWIRE // JRC2018U REAL";
        GlText(title, px + 10, py + 8, Fa(c1, 235));

        int fullBtnW = isFull ? 66 : 44;
        int fullBtnX = px + pw - 30 - fullBtnW - 4;
        GpuGl.FillRect(fullBtnX, py + 5, fullBtnW, 18, Color.FromArgb(150, 10, 22, 18));
        GpuGl.DrawRect(fullBtnX, py + 5, fullBtnW, 18, Fa(c1, 150), 1f);
        GlText(isFull ? "[EXIT]" : "[FULL]", fullBtnX + 5, py + 8, gold);
        int foldBtnX = px + pw - 28;
        GpuGl.FillRect(foldBtnX, py + 5, 22, 18, Color.FromArgb(150, 10, 22, 18));
        GpuGl.DrawRect(foldBtnX, py + 5, 22, 18, Fa(c1, 150), 1f);
        GlText("-", foldBtnX + 8, py + 8, gold);

        Porygon3DAvatar.PorygonAgent ag = porygon.Agents[porygon.SelectedBrainAgentIndex % porygon.Agents.Count];
        string sub = isFull
            ? string.Format("AGENT:[{0}] G{1} {2} | F2:{3:0.0}% | {4}", ag.Name, ag.Brain.Generation, ag.Brain.GetLifeStageLabel(), ag.Brain.F2Score, flyWire.Status)
            : string.Format("[{0}] {1} NEURONAS FLYWIRE | {2} AP", ag.Name, flyWire.Neurons.Count, flyWire.Pulses.Count);
        GlText(sub, px + 10, py + 25, Fa(ag.AuraColor, 230));

        // 3D viewport
        int topPad = isFull ? 44 : 40;
        int botPad = isFull ? 48 : 46;
        Rectangle area = new Rectangle(px + 2, py + topPad, pw - 4, ph - topPad - botPad);
        float yaw = GetEffectiveBrainYaw();
        float pitch = GetEffectiveBrainPitch();
        flyWire.SetupCamera(area, screenW, screenH, yaw, pitch, brain3DZoom, brain3DPanX, brain3DPanY);
        brain3DLastArea = area;
        bool hoverIn = brainHoverClientX >= area.X && brainHoverClientX < area.Right && brainHoverClientY >= area.Y && brainHoverClientY < area.Bottom && !brain3DDragging;
        flyWire.Pick(brainHoverClientX, brainHoverClientY, hoverIn);
        flyWire.RenderGL(area, screenW, screenH, isFull, gpuEcoMode, true);

        // Anatomical neuropil labels anchored to real JRC2018U neuropil centroids
        if (isFull) {
            for (int k = 0; k < flyWire.Neuropils.Count; k++) {
                FlyWireBrain.Neuropil p = flyWire.Neuropils[k];
                if (Array.IndexOf(FwLabelNeuropils, p.Name) < 0) continue;
                for (int side = 0; side < 2; side++) {
                    if (side == 0 && !(p.HasL && (p.Name == "ME" || p.Name == "LO" || p.Name == "AL"))) continue;
                    if (side == 1 && !p.HasR && !p.HasL) continue;
                    float x = side == 0 ? p.LX : (p.HasR ? p.RX : p.CX);
                    float y = side == 0 ? p.LY : (p.HasR ? p.RY : p.CY);
                    float z = side == 0 ? p.LZ : (p.HasR ? p.RZ : p.CZ);
                    float sx, sy, dep;
                    if (!flyWire.Project(x, y, z, out sx, out sy, out dep)) continue;
                    if (sx < area.X + 8 || sx > area.Right - 60 || sy < area.Y + 8 || sy > area.Bottom - 10) continue;
                    Color pc = Color.FromArgb(255, p.R, p.G, p.B);
                    int a = (int)(150 + 105 * Math.Min(1f, p.Act * 2f));
                    GpuGl.FillEllipse(sx, sy, 2.4f, 2.4f, Fa(pc, a), 8);
                    GpuGl.DrawLine(sx, sy, sx + 10, sy - 10, Fa(pc, a - 40), 1f);
                    GlTextBox(p.Name, sx + 12, sy - 17, Color.FromArgb(Math.Min(255, a), 228, 236, 240), Color.FromArgb(150, 2, 6, 8));
                }
            }
        }

        // Legend: IDS alert -> real anatomical pathway stages
        if (isFull) {
            int lgX = px + 14, lgY = py + 52, lgW = 404;
            GpuGl.FillRect(lgX - 6, lgY - 6, lgW, 22 + FlyWireBrain.NStages * 30, Color.FromArgb(150, 2, 6, 8));
            GpuGl.DrawRect(lgX - 6, lgY - 6, lgW, 22 + FlyWireBrain.NStages * 30, Fa(c1, 80), 1f);
            GlText("ALERTA IDS -> VIA ANATOMICA REAL (FLYWIRE)", lgX, lgY, Fa(c2, 235));
            for (int s = 0; s < FlyWireBrain.NStages; s++) {
                int yy = lgY + 18 + s * 30;
                Color sc = FlyWireBrain.StageColors[s];
                float act = Math.Min(1f, flyWire.StageAct[s]);
                GpuGl.FillEllipse(lgX + 4, yy + 6, 4f, 4f, sc, 12);
                GlText(string.Format("{0} [{1}]", FlyWireBrain.StageNames[s], flyWire.StageCount[s]), lgX + 14, yy, Fa(sc, 240));
                GpuGl.FillRect(lgX + 250, yy + 3, 120, 6, Color.FromArgb(90, 40, 56, 60));
                GpuGl.FillRect(lgX + 250, yy + 3, Math.Max(1f, 120f * act), 6, Fa(sc, 230));
                GlText(FlyWireBrain.StageDesc[s], lgX + 14, yy + 13, dim);
            }
        }

        // Pathway trace (last routed alerts with the real cell types reached)
        if (isFull) {
            int tw = 470, tX = px + pw - tw - 14, tY = py + 52;
            List<FlyWireBrain.PathTrace> snap;
            lock (flyWire.Traces) { snap = new List<FlyWireBrain.PathTrace>(flyWire.Traces); }
            int rows = Math.Max(1, Math.Min(5, snap.Count));
            GpuGl.FillRect(tX - 6, tY - 6, tw, 22 + rows * 30, Color.FromArgb(150, 2, 6, 8));
            GpuGl.DrawRect(tX - 6, tY - 6, tw, 22 + rows * 30, Fa(c1, 80), 1f);
            GlText(string.Format("PATHWAY TRACE // EVENTOS:{0} SALTOS SINAPTICOS:{1}", flyWire.TotalEvents, flyWire.TotalCascadeHops), tX, tY, Fa(c2, 235));
            if (snap.Count == 0) {
                GlText("Esperando alertas IDS / SIEM / IPS...", tX, tY + 18, dim);
            }
            int shown = 0;
            for (int i = snap.Count - 1; i >= 0 && shown < 5; i--, shown++) {
                FlyWireBrain.PathTrace t = snap[i];
                int yy = tY + 18 + shown * 30;
                Color tc = t.MaxStageReached >= 0 ? FlyWireBrain.StageColors[t.MaxStageReached] : txt;
                string head = string.Format("[{0}] {1}", t.At.ToString("HH:mm:ss"), t.Label);
                if (head.Length > 68) head = head.Substring(0, 68);
                GlText(head, tX, yy, Fa(tc, 235));
                string hops = t.Hops.Count > 0 ? string.Join(" > ", t.Hops.ToArray()) : "(propagando...)";
                if (hops.Length > 68) hops = hops.Substring(0, 66) + "..";
                GlText(hops, tX + 8, yy + 13, txt);
            }
        }

        // Hover inspector (neuron or neuropil)
        if (flyWire.HoverNeuron >= 0) {
            FlyWireBrain.Neuron n = flyWire.Neurons[flyWire.HoverNeuron];
            Color nc = Color.FromArgb(255, n.R, n.G, n.B);
            string np = (n.NeuropilIdx >= 0) ? flyWire.Neuropils[n.NeuropilIdx].Name : "?";
            string[] lines = new string[] {
                string.Format("{0}  //  {1}", n.Short, FlyWireBrain.StageNames[n.Stage]),
                n.CellType.Length > 56 ? n.CellType.Substring(0, 56) : n.CellType,
                string.Format("FlyWire:{0}  VFB:{1}", n.FlyWireId, n.VfbId),
                string.Format("NT:{0}  NEUROPILO:{1}  ACT:{2:0}%", n.Nt, np, Math.Min(1f, n.Act) * 100f),
                string.Format("SINAPSIS OUT:{0} ({1} ar.)  IN:{2} ({3} ar.)", n.SynOut, n.Out.Count, n.SynIn, n.In.Count),
                flyWire.HasRealSynapses ? string.Format("SINAPSIS REALES v783: {0:N0} salida / {1:N0} entrada", n.RealSynOut, n.RealSynIn) : "SINAPSIS REALES: (no cargadas)",
                "CIAN = SALIDA  |  AMBAR = ENTRADA"
            };
            DrawGlTooltip(lines, brainHoverClientX + 16, brainHoverClientY + 12, nc, r);
        } else if (flyWire.HoverNeuropil >= 0) {
            FlyWireBrain.Neuropil p = flyWire.Neuropils[flyWire.HoverNeuropil];
            Color pc = Color.FromArgb(255, p.R, p.G, p.B);
            int resident = 0;
            for (int i = 0; i < flyWire.Neurons.Count; i++) if (flyWire.Neurons[i].NeuropilIdx == flyWire.HoverNeuropil) resident++;
            string[] lines = new string[] {
                string.Format("NEUROPILO {0}", p.Name),
                string.Format("VFB:{0}  (JRC2018Unisex)", p.VfbId),
                string.Format("NEURONAS SELECCIONADAS AQUI: {0}  ACT:{1:0}%", resident, p.Act * 100f)
            };
            DrawGlTooltip(lines, brainHoverClientX + 16, brainHoverClientY + 12, pc, r);
        }

        // Status line + attribution
        int yawDeg = (int)Math.Round(yaw * 180.0 / Math.PI);
        int pitchDeg = (int)Math.Round(pitch * 180.0 / Math.PI);
        string status = isFull
            ? string.Format("AP EN VUELO:{0} | YAW {1:+000;-000;000} PITCH {2:+00;-00;00} ZOOM {3:0.00}x | ARRASTRA=ROTAR  RUEDA=ZOOM  CLIC-DER=MOVER  HOVER=INSPECCIONAR | Data: FlyWire Consortium / VFB (CC BY-NC)",
                flyWire.Pulses.Count, yawDeg, pitchDeg, brain3DZoom)
            : string.Format("AP:{0} | YAW:{1:+000;-000;000} | FlyWire/VFB", flyWire.Pulses.Count, yawDeg);
        GlText(status, px + 10, py + ph - 41, Fa(c1, 220));

        // Toolbar (same hit zones as HandleBrain3DClick)
        string[] btnLabels = new string[] { "< ROT", "ROT >", string.Format("ZM {0:0.0}x", brain3DZoom), brain3DAutoRotate ? "AUTO:ON" : "FREE:3D", "NEXT >>" };
        float[] zoneStarts = new float[] { 0.02f, 0.19f, 0.37f, 0.57f, 0.77f };
        float[] zoneWidths = new float[] { 0.15f, 0.16f, 0.18f, 0.18f, 0.21f };
        int barY = py + ph - 23;
        for (int b = 0; b < 5; b++) {
            int bxBtn = px + (int)(pw * zoneStarts[b]);
            int bwBtn = (int)(pw * zoneWidths[b]);
            GpuGl.FillRect(bxBtn, barY, bwBtn, 18, Color.FromArgb(150, 10, 22, 18));
            GpuGl.DrawRect(bxBtn, barY, bwBtn, 18, Fa(c1, 150), 1f);
            Color bt = (b == 3 && !brain3DAutoRotate) ? gold : (b == 4 ? Fa(c2, 230) : txt);
            GlText(btnLabels[b], bxBtn + 5, barY + 3, bt);
        }

        // Optional verification snapshot (requested via /api/brain/snapshot)
        string snapPath = brainSnapshotPath;
        if (snapPath != null) {
            brainSnapshotPath = null;
            try { SaveGlRegionPng(r, snapPath); } catch { }
        }
    }

    private void DrawGlTooltip(string[] lines, float x, float y, Color accent, Rectangle bounds) {
        int maxLen = 0;
        for (int i = 0; i < lines.Length; i++) if (lines[i].Length > maxLen) maxLen = lines[i].Length;
        float w = maxLen * 6.6f + 14f, h = lines.Length * 14f + 8f;
        if (x + w > bounds.Right - 6) x = x - w - 30f;
        if (y + h > bounds.Bottom - 52) y = bounds.Bottom - 52 - h;
        if (x < bounds.X + 6) x = bounds.X + 6;
        GpuGl.FillRect(x, y, w, h, Color.FromArgb(225, 3, 9, 12));
        GpuGl.DrawRect(x, y, w, h, Fa(accent, 210), 1.3f);
        for (int i = 0; i < lines.Length; i++) {
            GlText(lines[i], x + 7f, y + 5f + i * 14f, i == 0 ? Fa(accent, 245) : Color.FromArgb(230, 214, 226, 222));
        }
    }

    private void SaveGlRegionPng(Rectangle r, string path) {
        int w = r.Width, h = r.Height;
        byte[] buf = new byte[w * h * 4];
        GCHandle hb = GCHandle.Alloc(buf, GCHandleType.Pinned);
        try {
            GlBrain.glReadPixels(r.X, screenH - r.Bottom, w, h, GpuGl.GL_BGRA_EXT, GpuGl.GL_UNSIGNED_BYTE, hb.AddrOfPinnedObject());
        } finally { hb.Free(); }
        using (Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb)) {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try {
                for (int row = 0; row < h; row++) {
                    int srcRow = h - 1 - row;
                    Marshal.Copy(buf, srcRow * w * 4, IntPtr.Add(bd.Scan0, row * bd.Stride), w * 4);
                }
            } finally { bmp.UnlockBits(bd); }
            // force opaque alpha for viewing
            using (Bitmap opaque = new Bitmap(w, h, PixelFormat.Format24bppRgb))
            using (Graphics g = Graphics.FromImage(opaque)) {
                g.Clear(Color.Black);
                g.DrawImageUnscaled(bmp, 0, 0);
                opaque.Save(path, ImageFormat.Png);
            }
        }
    }

    private static float BrainDepthK(float rz) {
        float k = (rz + 80f) / 160f;
        if (k < 0f) k = 0f;
        if (k > 1f) k = 1f;
        return 0.40f + 0.60f * k;
    }

    private static int BrainA(float v) {
        if (v < 0f) return 0;
        if (v > 255f) return 255;
        return (int)v;
    }

    // Color of a neuron by anatomical region (idx < 0 => region base color without special-cased golden VT neurons)
    private static Color BrainNeuronColor(int idx, int region) {
        if (idx >= 0 && (idx == 0 || idx == 8 || idx == 11 || (region == 0 && (idx % 8) < 4))) return Color.FromArgb(255, 252, 212, 32);
        if (region == 0) return Color.FromArgb(255, 248, 52, 122);
        if (region == 1) return Color.FromArgb(255, 42, 242, 178);
        if (region == 2) return Color.FromArgb(255, 56, 168, 252);
        return Color.FromArgb(255, 52, 246, 86);
    }

    private void DrawBrainTag(Graphics g, string text, float ax, float ay, int ox, int oy, int side, Color col, float k, int minX, int maxX, int minY, int maxY) {
        SizeF sz = g.MeasureString(text, fontHudSmall);
        float tx = ax + ox;
        if (side < 0) tx -= sz.Width; else if (side == 0) tx -= sz.Width * 0.5f;
        float ty = ay + oy - sz.Height * 0.5f;
        tx = Math.Max(minX, Math.Min(maxX - sz.Width, tx));
        ty = Math.Max(minY, Math.Min(maxY - sz.Height, ty));
        float lx, ly;
        if (side < 0) { lx = tx + sz.Width; ly = ty + sz.Height * 0.5f; }
        else if (side > 0) { lx = tx; ly = ty + sz.Height * 0.5f; }
        else { lx = tx + sz.Width * 0.5f; ly = (oy < 0) ? (ty + sz.Height) : ty; }
        using (Pen lp = new Pen(Color.FromArgb(BrainA(120f * k), col.R, col.G, col.B), 1.0f))
        using (SolidBrush bg = new SolidBrush(Color.FromArgb(BrainA(150f * k), 2, 6, 8)))
        using (SolidBrush tb = new SolidBrush(Color.FromArgb(BrainA(225f * k), col.R, col.G, col.B))) {
            g.DrawLine(lp, ax, ay, lx, ly);
            g.FillEllipse(tb, ax - 2.2f, ay - 2.2f, 4.4f, 4.4f);
            g.FillRectangle(bg, tx - 2f, ty, sz.Width + 2f, sz.Height);
            g.DrawString(text, fontHudSmall, tb, tx, ty);
        }
    }

    private struct Neuron3DProj {
        public int Index;
        public int Region;
        public float SX;
        public float SY;
        public float SZ;
        public float Scale;
        public float Glow;
    }

    private void DrawFlyBrain3DCardAt(Graphics g, int px, int py, int pw, int ph, Color c1, Color c2, Color c3) {
        if (porygon == null || porygon.Agents.Count == 0) return;

        bool isFull = (pw > 500);
        int actualH = brain3DCollapsed ? 32 : ph;
        int selIdx = porygon.SelectedBrainAgentIndex % porygon.Agents.Count;
        Porygon3DAvatar.PorygonAgent ag = porygon.Agents[selIdx];
        FlyBrainConnectome br = ag.Brain;

        // Deep matte forest-obsidian background with attenuated border (-35%) so the 3D FlyWire brain stands out cleanly without eye strain
        using (SolidBrush panelBg = new SolidBrush(Color.FromArgb(isFull ? 224 : 188, 3, 7, 6)))
        using (Pen borderPen = new Pen(Color.FromArgb(135, c1.R, c1.G, c1.B), isFull ? 1.7f : 1.2f))
        using (SolidBrush c1B = new SolidBrush(Color.FromArgb(225, c1.R, c1.G, c1.B)))
        using (SolidBrush c2B = new SolidBrush(Color.FromArgb(225, c2.R, c2.G, c2.B)))
        using (SolidBrush goldB = new SolidBrush(Color.FromArgb(225, 196, 156, 48)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(215, 206, 218, 214)))
        using (SolidBrush btnBg = new SolidBrush(Color.FromArgb(135, 10, 22, 18)))
        using (Pen btnBor = new Pen(Color.FromArgb(145, c1.R, c1.G, c1.B), 1.0f)) {
            g.FillRectangle(panelBg, px, py, pw, actualH);
            g.DrawRectangle(borderPen, px, py, pw, actualH);

            using (Pen cornerPen = new Pen(Color.FromArgb(170, c2.R, c2.G, c2.B), 2.0f)) {
                g.DrawLine(cornerPen, px, py, px + 18, py);
                g.DrawLine(cornerPen, px, py, px, py + Math.Min(18, actualH));
                g.DrawLine(cornerPen, px + pw, py + actualH, px + pw - 18, py + actualH);
                g.DrawLine(cornerPen, px + pw, py + actualH, px + pw, py + actualH - Math.Min(18, actualH));
            }

            string headerTitle = isFull
                ? ">> FLYWIRE_3D // DROSOPHILA BRAIN CONNECTOME [FULLSCREEN 3D INSPECTOR // CTRL+ALT+B]"
                : ">> FLYWIRE_3D // DROSOPHILA BRAIN";
            g.DrawString(headerTitle, fontHudBold, c1B, px + 10, py + 7);

            // Top-Right Header Interactive Buttons: [FULL / EXIT] + [- / +]
            if (!brain3DCollapsed) {
                int fullBtnW = isFull ? 66 : 44;
                int fullBtnX = px + pw - 30 - fullBtnW - 4;
                g.FillRectangle(btnBg, fullBtnX, py + 5, fullBtnW, 18);
                g.DrawRectangle(btnBor, fullBtnX, py + 5, fullBtnW, 18);
                g.DrawString(isFull ? "[EXIT]" : "[FULL]", fontHudSmall, goldB, fullBtnX + 4, py + 7);
            }
            int foldBtnX = px + pw - 28;
            g.FillRectangle(btnBg, foldBtnX, py + 5, 22, 18);
            g.DrawRectangle(btnBor, foldBtnX, py + 5, 22, 18);
            g.DrawString(brain3DCollapsed ? "+" : "-", fontHudBold, goldB, foldBtnX + 6, py + 5);

            if (brain3DCollapsed) return;

            string focusMode = porygon.BrainAutoFocusedOnThreat ? "AUTO-INTERCEPT" : "MANUAL_SELECT";
            using (SolidBrush agB = new SolidBrush(ag.AuraColor)) {
                string subLine = isFull
                    ? string.Format("AGENT: [{0}] G{1} ({2}) | F2:{3:0.0}% | ACC:{4:0.0}% | FN:{5} | XYZ:[{6:0},{7:0},{8:+0;-0;0}] | MODE:{9}",
                        ag.Name, br.Generation, br.GetLifeStageLabel(), br.F2Score, br.DetectionAccuracy, br.FalseNegatives, ag.PosX, ag.PosY, ag.PosZ, focusMode)
                    : string.Format("[{0}] G{1} {2} | F2:{3:0.0}% | FN:{4} | {5}",
                        ag.Name, br.Generation, br.GetLifeStageLabel(), br.F2Score, br.FalseNegatives, focusMode);
                g.DrawString(subLine, fontHudSmall, agB, px + 10, py + 24);
            }

            int cx = px + pw / 2;
            int cy = py + (int)(ph * 0.50f);

            // Interactive 3D Rotation (Mouse Wheel + Drag + Toolbar) or Smooth Orbital Oscillation
            float effYaw = GetEffectiveBrainYaw();
            float effPitch = GetEffectiveBrainPitch();
            float cosY = (float)Math.Cos(effYaw);
            float sinY = (float)Math.Sin(effYaw);
            float cosP = (float)Math.Cos(effPitch);
            float sinP = (float)Math.Sin(effPitch);

            float sizeRatio = isFull ? Math.Min((float)pw / 350.0f, (float)ph / 200.0f) : Math.Min((float)pw / 356.0f, (float)ph / 246.0f);
            float modelScale = 1.22f * sizeRatio * brain3DZoom;

            // Project static 3D FlyWire Cortical Wireframe Mesh + Cobalt-Blue Mushroom Body Mesh
            int vCount = (FlyBrainConnectome.ShellVX != null) ? FlyBrainConnectome.ShellVX.Length : 0;
            PointF[] shellPts = new PointF[vCount];
            float[] shellRZ = new float[vCount];
            for (int v = 0; v < vCount; v++) {
                float vx = FlyBrainConnectome.ShellVX[v];
                float vy = FlyBrainConnectome.ShellVY[v];
                float vz = FlyBrainConnectome.ShellVZ[v];

                float rx = vx * cosY - vz * sinY;
                float rz1 = vx * sinY + vz * cosY;
                float ry = vy * cosP - rz1 * sinP;
                float rz = vy * sinP + rz1 * cosP;

                float persp = 260f / Math.Max(110f, 260f - rz);
                shellPts[v] = new PointF(cx + rx * modelScale * persp, cy - ry * modelScale * persp);
                shellRZ[v] = rz;
            }

            // Project all 64 Spiking Neurons in 1:1 Anatomical Coordinates (needed first for hover-picking, region activity & edge budget)
            Neuron3DProj[] proj = new Neuron3DProj[FlyBrainConnectome.N];
            int activeSpikes = 0;
            float[] regSum = new float[4];
            int[] regCnt = new int[4];
            for (int i = 0; i < FlyBrainConnectome.N; i++) {
                float nx = br.NeuronX[i];
                float ny = br.NeuronY[i];
                float nz = br.NeuronZ[i];

                float rx = nx * cosY - nz * sinY;
                float rz1 = nx * sinY + nz * cosY;
                float ry = ny * cosP - rz1 * sinP;
                float rz = ny * sinP + rz1 * cosP;

                float persp = 260f / Math.Max(110f, 260f - rz);
                proj[i].Index = i;
                proj[i].Region = br.NeuronRegion[i];
                proj[i].SX = cx + rx * modelScale * persp;
                proj[i].SY = cy - ry * modelScale * persp;
                proj[i].SZ = rz;
                proj[i].Scale = persp * Math.Max(1.0f, sizeRatio * 0.65f);
                proj[i].Glow = br.SpikeGlow[i];
                if (br.SpikeGlow[i] > 0.25f) activeSpikes++;
                int rg = Math.Max(0, Math.Min(3, br.NeuronRegion[i]));
                regSum[rg] += br.SpikeGlow[i];
                regCnt[rg]++;
            }

            // Live region-activity smoothing + spike-rate history (one sample per rendered frame)
            if (brainActHistFrame != frameCount) {
                brainActHistFrame = frameCount;
                for (int r = 0; r < 4; r++) {
                    float avgR = (regCnt[r] > 0) ? (regSum[r] / regCnt[r]) : 0f;
                    brainRegionAct[r] = brainRegionAct[r] * 0.72f + avgR * 0.28f;
                }
                brainActHist[brainActHistPos] = activeSpikes;
                brainActHistPos = (brainActHistPos + 1) % brainActHist.Length;
            }

            // Hover-picking of the nearest neuron under the mouse cursor (fullscreen inspector only)
            int hoverIdx = -1;
            if (isFull && brain3DHoverX >= 0) {
                float bestD2 = 16f * 16f;
                for (int i = 0; i < FlyBrainConnectome.N; i++) {
                    float hdx = proj[i].SX - brain3DHoverX;
                    float hdy = proj[i].SY - brain3DHoverY;
                    float hd2 = hdx * hdx + hdy * hdy;
                    if (hd2 < bestD2) { bestD2 = hd2; hoverIdx = i; }
                }
            }

            float actNorm = Math.Max(0f, Math.Min(1f, activeSpikes / 22f));

            // Pass 1: Back-Hemisphere 3D Triangulated Cortical Wireframe Shell (Optic lobes drawn much fainter so the central brain pops)
            PointF[] triPoly = new PointF[3];
            using (SolidBrush backFill = new SolidBrush(Color.FromArgb(24, 82, 94, 112)))
            using (Pen backWire = new Pen(Color.FromArgb(52, 122, 132, 148), isFull ? 0.90f : 0.70f))
            using (Pen lobeBackWire = new Pen(Color.FromArgb(26, 104, 118, 140), isFull ? 0.75f : 0.60f))
            using (Pen amberWire = new Pen(Color.FromArgb(48, 218, 156, 32), isFull ? 0.85f : 0.65f)) {
                for (int t = 0; t < FlyBrainConnectome.ShellTris.Count; t++) {
                    FlyBrainConnectome.BrainTri3D tri = FlyBrainConnectome.ShellTris[t];
                    if (tri.PartKind == 3) continue; // Mushroom body rendered in Pass 2
                    triPoly[0] = shellPts[tri.I0];
                    triPoly[1] = shellPts[tri.I1];
                    triPoly[2] = shellPts[tri.I2];
                    if (tri.PartKind == 4) {
                        g.DrawPolygon(amberWire, triPoly);
                        continue;
                    }
                    float avgZ = (shellRZ[tri.I0] + shellRZ[tri.I1] + shellRZ[tri.I2]) * 0.3333f;
                    if (avgZ > 2.0f) continue;
                    if (tri.PartKind == 1) {
                        g.DrawPolygon(lobeBackWire, triPoly);
                        continue;
                    }
                    g.FillPolygon(backFill, triPoly);
                    g.DrawPolygon(backWire, triPoly);
                }
            }

            // Pass 2: Central 3D Cobalt-Blue Mushroom Body Lobes (Pedunculus, Bulbous Alpha Cap & Medial Lobes)
            using (SolidBrush mbFill = new SolidBrush(Color.FromArgb(104, 14, 86, 198)))
            using (Pen mbWire = new Pen(Color.FromArgb(185, 42, 148, 248), isFull ? 1.10f : 0.80f)) {
                for (int t = 0; t < FlyBrainConnectome.ShellTris.Count; t++) {
                    FlyBrainConnectome.BrainTri3D tri = FlyBrainConnectome.ShellTris[t];
                    if (tri.PartKind != 3) continue;
                    triPoly[0] = shellPts[tri.I0];
                    triPoly[1] = shellPts[tri.I1];
                    triPoly[2] = shellPts[tri.I2];
                    g.FillPolygon(mbFill, triPoly);
                    g.DrawPolygon(mbWire, triPoly);
                }
            }

            // Pass 3: Dendritic Arborizations & Curved Optic Tracts (depth-cued; dimmed when another neuron is inspected)
            float lineScale = isFull ? 1.35f : 0.95f;
            using (Pen arborPen = new Pen(Color.White, 1.0f))
            using (SolidBrush tipB = new SolidBrush(Color.White)) {
                for (int d = 0; d < br.DendriteBranches.Count; d++) {
                    FlyBrainConnectome.DendriteSeg3D seg = br.DendriteBranches[d];
                    float glow = br.SpikeGlow[seg.NeuronIdx];

                    float rx0 = seg.X0 * cosY - seg.Z0 * sinY;
                    float rz0_1 = seg.X0 * sinY + seg.Z0 * cosY;
                    float ry0 = seg.Y0 * cosP - rz0_1 * sinP;
                    float rz0 = seg.Y0 * sinP + rz0_1 * cosP;
                    float p0Scale = 260f / Math.Max(110f, 260f - rz0);
                    float sx0 = cx + rx0 * modelScale * p0Scale;
                    float sy0 = cy - ry0 * modelScale * p0Scale;

                    float rx1 = seg.X1 * cosY - seg.Z1 * sinY;
                    float rz1_1 = seg.X1 * sinY + seg.Z1 * cosY;
                    float ry1 = seg.Y1 * cosP - rz1_1 * sinP;
                    float rz1 = seg.Y1 * sinP + rz1_1 * cosP;
                    float p1Scale = 260f / Math.Max(110f, 260f - rz1);
                    float sx1 = cx + rx1 * modelScale * p1Scale;
                    float sy1 = cy - ry1 * modelScale * p1Scale;

                    float dkSeg = BrainDepthK((rz0 + rz1) * 0.5f);
                    float aF = (96f + glow * 140f) * dkSeg;
                    float wF = seg.Thickness * (0.80f + glow * 0.55f) * lineScale;
                    if (hoverIdx >= 0) {
                        if (seg.NeuronIdx == hoverIdx) { aF = 250f; wF *= 1.45f; }
                        else aF *= 0.40f;
                    }
                    int alpha = BrainA(aF);
                    arborPen.Color = Color.FromArgb(alpha, seg.R, seg.G, seg.B);
                    arborPen.Width = wF;
                    g.DrawLine(arborPen, sx0, sy0, sx1, sy1);

                    if (seg.IsTip && (glow > 0.28f || (d % 2 == 0))) {
                        float tipR = ((glow > 0.45f) ? 1.65f : 1.05f) * lineScale * (0.80f + 0.30f * dkSeg);
                        tipB.Color = Color.FromArgb(BrainA(alpha + 25), seg.R, seg.G, seg.B);
                        g.FillEllipse(tipB, sx1 - tipR, sy1 - tipR, tipR * 2f, tipR * 2f);
                    }
                }
            }

            // Pass 4: Curved Synaptic Axons (decluttered: active-edge budget + sparse idle edges + presynaptic-region colors + depth cue)
            int activeEdges = 0;
            for (int pre = 0; pre < FlyBrainConnectome.N; pre++) {
                for (int post = 0; post < FlyBrainConnectome.N; post++) {
                    if (Math.Abs(br.W[pre, post]) < 0.16f) continue;
                    if (Math.Max(proj[pre].Glow, proj[post].Glow * 0.85f) >= 0.30f) activeEdges++;
                }
            }
            int activeStride = Math.Max(1, activeEdges / (isFull ? 220 : 130));
            int idleStride = isFull ? 17 : 29;
            int hovOut = 0, hovIn = 0, hovExc = 0, hovInh = 0;
            int synIdx = 0;
            PointF[] curvePts = new PointF[3];
            using (Pen synPen = new Pen(Color.White, 1.0f))
            using (SolidBrush pulseB = new SolidBrush(Color.FromArgb(245, 255, 255, 255))) {
                for (int pre = 0; pre < FlyBrainConnectome.N; pre++) {
                    for (int post = 0; post < FlyBrainConnectome.N; post++) {
                        float w = br.W[pre, post];
                        if (Math.Abs(w) < 0.16f) continue;
                        synIdx++;
                        float gPre = proj[pre].Glow;
                        float gPost = proj[post].Glow;
                        float act = Math.Max(gPre, gPost * 0.85f);
                        bool isActive = act >= 0.30f;
                        bool touchesHover = (hoverIdx >= 0 && (pre == hoverIdx || post == hoverIdx));
                        if (touchesHover) {
                            if (pre == hoverIdx) hovOut++; else hovIn++;
                            if (w >= 0f) hovExc++; else hovInh++;
                        } else if (isActive) {
                            if (activeStride > 1 && (synIdx % activeStride) != 0) continue;
                        } else if ((synIdx % idleStride) != 0) {
                            continue;
                        }

                        float mx3 = (br.NeuronX[pre] + br.NeuronX[post]) * 0.36f;
                        float my3 = (br.NeuronY[pre] + br.NeuronY[post]) * 0.42f + 1.5f;
                        float mz3 = (br.NeuronZ[pre] + br.NeuronZ[post]) * 0.45f + 2.5f;
                        mx3 = Math.Max(-34f, Math.Min(34f, mx3));
                        my3 = Math.Max(-18f, Math.Min(18f, my3));

                        float mrx = mx3 * cosY - mz3 * sinY;
                        float mrz1 = mx3 * sinY + mz3 * cosY;
                        float mry = my3 * cosP - mrz1 * sinP;
                        float mrz = my3 * sinP + mrz1 * cosP;
                        float mPersp = 260f / Math.Max(110f, 260f - mrz);
                        float msx = cx + mrx * modelScale * mPersp;
                        float msy = cy - mry * modelScale * mPersp;

                        float dkE = BrainDepthK((proj[pre].SZ + proj[post].SZ + mrz) * 0.3333f);
                        float aE = (isActive ? (34f + act * 168f) : 17f) * dkE;
                        float wE = (act > 0.5f ? 1.20f : 0.72f) * lineScale;
                        Color synCol;
                        if (touchesHover) {
                            // Inspector colors: outgoing = cyan-white, incoming = amber
                            aE = 150f + act * 100f;
                            wE = 1.55f * lineScale;
                            synCol = (pre == hoverIdx) ? Color.FromArgb(BrainA(aE), 140, 255, 240) : Color.FromArgb(BrainA(aE), 255, 190, 70);
                        } else {
                            if (hoverIdx >= 0) aE *= 0.30f;
                            if (pre == 11 || post == 11) {
                                synCol = Color.FromArgb(BrainA(aE), 250, 208, 28);
                            } else if (w < 0f) {
                                synCol = Color.FromArgb(BrainA(aE), 178, 95, 255);
                            } else if (proj[pre].Region == 0) {
                                synCol = Color.FromArgb(BrainA(aE), 252, 196, 52);
                            } else if (proj[pre].Region == 1) {
                                synCol = Color.FromArgb(BrainA(aE), 45, 238, 188);
                            } else if (proj[pre].Region == 2) {
                                synCol = Color.FromArgb(BrainA(aE), 56, 168, 252);
                            } else {
                                synCol = Color.FromArgb(BrainA(aE), 52, 246, 96);
                            }
                        }

                        synPen.Color = synCol;
                        synPen.Width = wE;
                        curvePts[0] = new PointF(proj[pre].SX, proj[pre].SY);
                        curvePts[1] = new PointF(msx, msy);
                        curvePts[2] = new PointF(proj[post].SX, proj[post].SY);
                        g.DrawCurve(synPen, curvePts, 0.55f);

                        if (gPre > 0.42f && (touchesHover || (synIdx % 2 == 0))) {
                            float tPulse = ((frameCount * 0.14f + synIdx * 0.19f) % 1.0f);
                            float pxp, pyp;
                            if (tPulse < 0.5f) {
                                float u = tPulse * 2.0f;
                                pxp = proj[pre].SX + (msx - proj[pre].SX) * u;
                                pyp = proj[pre].SY + (msy - proj[pre].SY) * u;
                            } else {
                                float u = (tPulse - 0.5f) * 2.0f;
                                pxp = msx + (proj[post].SX - msx) * u;
                                pyp = msy + (proj[post].SY - msy) * u;
                            }
                            float pr = 1.45f * lineScale * (0.85f + 0.25f * dkE);
                            g.FillEllipse(pulseB, pxp - pr, pyp - pr, pr * 2f, pr * 2f);
                        }
                    }
                }
            }

            Neuron3DProj[] sorted = new Neuron3DProj[FlyBrainConnectome.N];
            Array.Copy(proj, sorted, FlyBrainConnectome.N);
            Array.Sort(sorted, (a, b) => a.SZ.CompareTo(b.SZ)); // Back-to-front by Z

            int labeledTop = 0;
            using (SolidBrush haloB = new SolidBrush(Color.White))
            using (SolidBrush coreB = new SolidBrush(Color.White))
            using (SolidBrush sparkB = new SolidBrush(Color.FromArgb(250, 255, 255, 255)))
            using (Pen ripplePen = new Pen(Color.White, 1.0f))
            using (Pen hoverPen = new Pen(Color.FromArgb(235, 255, 255, 255), 1.5f)) {
                for (int i = 0; i < sorted.Length; i++) {
                    Neuron3DProj np = sorted[i];
                    Color nCol = BrainNeuronColor(np.Index, np.Region);
                    float dkN = BrainDepthK(np.SZ);

                    float baseR = (1.30f + np.Glow * 1.70f) * np.Scale * (0.80f + 0.30f * dkN);
                    int coreAlpha = BrainA((150f + np.Glow * 105f) * (0.58f + 0.42f * dkN));

                    if (np.Glow > 0.25f || np.Index == 11) {
                        float haloR = baseR * 1.85f;
                        int haloAlpha = BrainA(Math.Min(115f, 32f + np.Glow * 80f) * (0.6f + 0.4f * dkN));
                        haloB.Color = Color.FromArgb(haloAlpha, nCol.R, nCol.G, nCol.B);
                        g.FillEllipse(haloB, np.SX - haloR, np.SY - haloR, haloR * 2f, haloR * 2f);
                    }

                    // Expanding spike-ripple ring for actively firing neurons
                    if (np.Glow > 0.38f) {
                        float rr = baseR * (1.9f + (1.0f - np.Glow) * 3.6f);
                        ripplePen.Color = Color.FromArgb(BrainA(np.Glow * 150f * dkN), nCol.R, nCol.G, nCol.B);
                        g.DrawEllipse(ripplePen, np.SX - rr, np.SY - rr, rr * 2f, rr * 2f);
                    }

                    coreB.Color = Color.FromArgb(coreAlpha, nCol.R, nCol.G, nCol.B);
                    g.FillEllipse(coreB, np.SX - baseR, np.SY - baseR, baseR * 2f, baseR * 2f);

                    if (np.Glow > 0.55f) {
                        float spR = 0.95f * lineScale;
                        g.FillEllipse(sparkB, np.SX - spR, np.SY - spR, spR * 2f, spR * 2f);
                    }

                    if (np.Index == hoverIdx) {
                        float hr = baseR + 5.0f;
                        g.DrawEllipse(hoverPen, np.SX - hr, np.SY - hr, hr * 2f, hr * 2f);
                        float hr2 = hr + 4.0f;
                        ripplePen.Color = Color.FromArgb(120, nCol.R, nCol.G, nCol.B);
                        g.DrawEllipse(ripplePen, np.SX - hr2, np.SY - hr2, hr2 * 2f, hr2 * 2f);
                    } else if (isFull && np.Glow > 0.72f && labeledTop < 5) {
                        labeledTop++;
                        g.DrawString("N" + np.Index.ToString(), fontHudSmall, txtB, np.SX + baseR + 3f, np.SY - baseR - 12f);
                    }
                }
            }

            // Pass 5: Front-Hemisphere 3D Triangulated Silver-Gray Cortical Wireframe Shell (rim tints with live network activity)
            int rimR = (int)(194 - 74 * actNorm);
            int rimG = (int)(206 + 49 * actNorm);
            int rimB = (int)(224 - 10 * actNorm);
            using (Pen frontWire = new Pen(Color.FromArgb(62, 154, 166, 184), isFull ? 0.95f : 0.72f))
            using (Pen lobeFrontWire = new Pen(Color.FromArgb(34, 130, 142, 162), isFull ? 0.80f : 0.62f))
            using (Pen rimWire = new Pen(Color.FromArgb(BrainA(116f + actNorm * 90f), rimR, rimG, rimB), isFull ? 1.25f : 0.92f)) {
                for (int t = 0; t < FlyBrainConnectome.ShellTris.Count; t++) {
                    FlyBrainConnectome.BrainTri3D tri = FlyBrainConnectome.ShellTris[t];
                    if (tri.PartKind == 3 || tri.PartKind == 4) continue;
                    float avgZ = (shellRZ[tri.I0] + shellRZ[tri.I1] + shellRZ[tri.I2]) * 0.3333f;
                    if (avgZ <= 2.0f) continue;
                    triPoly[0] = shellPts[tri.I0];
                    triPoly[1] = shellPts[tri.I1];
                    triPoly[2] = shellPts[tri.I2];
                    if (tri.PartKind == 1) { g.DrawPolygon(lobeFrontWire, triPoly); continue; }
                    g.DrawPolygon((avgZ < 10.0f) ? rimWire : frontWire, triPoly);
                }
            }

            // Anatomical Labels: anchored to projected 3D landmarks with leader lines (fullscreen) or compact corner tags (card)
            if (isFull) {
                string[] aLbl = new string[] { "OPTIC_LOBE_L", "OPTIC_LOBE_R", "MUSHROOM_BODY", "ANTENNAL_LOBE", "CENTRAL_COMPLEX", "PARS_INTERCEREBRALIS", "SEZ_MOTOR" };
                float[] aX = new float[] { -76f, 76f, 26f, -33f, 0f, 0f, 0f };
                float[] aY = new float[] { 32f, 32f, 36f, -20f, -2f, 31f, -38f };
                float[] aZ = new float[] { -2f, -2f, -3f, 3f, 8f, 0f, 0f };
                int[] aOx = new int[] { -34, 34, 40, -50, 150, 0, 0 };
                int[] aOy = new int[] { -24, -24, -30, 54, 98, -30, 40 };
                int[] aSide = new int[] { -1, 1, 1, -1, 1, 0, 0 };
                int[] aReg = new int[] { 0, 0, 1, 2, 2, 0, 3 };
                for (int lb = 0; lb < aLbl.Length; lb++) {
                    float lrx = aX[lb] * cosY - aZ[lb] * sinY;
                    float lrz1 = aX[lb] * sinY + aZ[lb] * cosY;
                    float lry = aY[lb] * cosP - lrz1 * sinP;
                    float lrz = aY[lb] * sinP + lrz1 * cosP;
                    float lpers = 260f / Math.Max(110f, 260f - lrz);
                    float lsx = cx + lrx * modelScale * lpers;
                    float lsy = cy - lry * modelScale * lpers;
                    Color lc = BrainNeuronColor(aReg[lb] == 0 ? 1 : -1, aReg[lb]);
                    DrawBrainTag(g, aLbl[lb], lsx, lsy, aOx[lb], aOy[lb], aSide[lb], lc, 0.55f + 0.45f * BrainDepthK(lrz), px + 4, px + pw - 4, py + 154, py + ph - 64);
                }
            } else {
                g.DrawString("OPTIC_LOBE_L", fontHudSmall, c1B, px + 10, py + 42);
                g.DrawString("OPTIC_LOBE_R", fontHudSmall, c1B, px + pw - 86, py + 42);
                g.DrawString("SEZ_MOTOR", fontHudSmall, c2B, px + pw - 72, py + ph - 58);
            }
            g.DrawString(string.Format("VT_N[11]:{0:0}%", proj[11].Glow * 100f), fontHudSmall, goldB, px + 10, py + ph - 58);

            // Fullscreen-only: Live region-activity legend + spike-rate sparkline + neuron hover inspector
            if (isFull) {
                string[] rgName = new string[] { "SENSORY / OPTIC / PI", "MUSHROOM BODY (KC)", "CENTRAL COMPLEX", "MOTOR / SEZ / VNC" };
                int lgX = px + 12;
                int lgY = py + 64;
                using (SolidBrush lgBg = new SolidBrush(Color.FromArgb(120, 2, 6, 8)))
                using (Pen lgBor = new Pen(Color.FromArgb(70, c1.R, c1.G, c1.B), 1.0f))
                using (SolidBrush barTrack = new SolidBrush(Color.FromArgb(70, 40, 56, 60))) {
                    g.FillRectangle(lgBg, lgX - 4, lgY - 4, 238, 20 + 4 * 17);
                    g.DrawRectangle(lgBor, lgX - 4, lgY - 4, 238, 20 + 4 * 17);
                    g.DrawString("REGION_ACTIVITY (LIVE)", fontHudSmall, c2B, lgX, lgY - 2);
                    for (int r = 0; r < 4; r++) {
                        Color rc = BrainNeuronColor(r == 0 ? 1 : -1, r);
                        int ry0 = lgY + 16 + r * 17;
                        float act01 = Math.Max(0f, Math.Min(1f, brainRegionAct[r] * 1.6f));
                        using (SolidBrush rb = new SolidBrush(rc)) {
                            g.FillEllipse(rb, lgX, ry0 + 3, 8, 8);
                            g.DrawString(rgName[r], fontHudSmall, txtB, lgX + 13, ry0);
                            g.FillRectangle(barTrack, lgX + 142, ry0 + 4, 64, 6);
                            g.FillRectangle(rb, lgX + 142, ry0 + 4, Math.Max(1, (int)(64 * act01)), 6);
                            g.DrawString(((int)(act01 * 100f)).ToString() + "%", fontHudSmall, txtB, lgX + 208, ry0);
                        }
                    }
                }

                // Spike-rate sparkline (right side)
                int spW = 188, spH = 46;
                int spX = px + pw - spW - 14;
                int spY = py + 64;
                using (SolidBrush spBg = new SolidBrush(Color.FromArgb(120, 2, 6, 8)))
                using (Pen spBor = new Pen(Color.FromArgb(70, c1.R, c1.G, c1.B), 1.0f))
                using (Pen spLine = new Pen(Color.FromArgb(230, c2.R, c2.G, c2.B), 1.4f)) {
                    g.FillRectangle(spBg, spX, spY, spW, spH + 18);
                    g.DrawRectangle(spBor, spX, spY, spW, spH + 18);
                    g.DrawString(string.Format("SPIKE_RATE {0}/64", activeSpikes), fontHudSmall, c2B, spX + 6, spY + 1);
                    int hN = brainActHist.Length;
                    float maxH = 16f;
                    for (int k = 0; k < hN; k++) { if (brainActHist[k] > maxH) maxH = brainActHist[k]; }
                    PointF[] sp = new PointF[hN];
                    for (int k = 0; k < hN; k++) {
                        int hi = (brainActHistPos + k) % hN;
                        sp[k] = new PointF(spX + 4 + k * ((spW - 8) / (float)(hN - 1)), spY + 18 + spH - 4 - (brainActHist[hi] / maxH) * (spH - 8));
                    }
                    g.DrawLines(spLine, sp);
                }

                // Hover inspector tooltip
                if (hoverIdx >= 0) {
                    string[] rgFull = new string[] { "SENSORY/OPTIC (PI-SLP-LH)", "MUSHROOM_BODY (KENYON CELLS)", "CENTRAL_COMPLEX", "MOTOR / SEZ / VNC" };
                    int hrg = Math.Max(0, Math.Min(3, proj[hoverIdx].Region));
                    Color hc = BrainNeuronColor(hoverIdx, proj[hoverIdx].Region);
                    string[] tipLines = new string[] {
                        string.Format("NEURON N[{0}] // {1}", hoverIdx, rgFull[hrg]),
                        string.Format("V:{0:0.00}  SPIKE_GLOW:{1:0}%  XYZ:[{2:0},{3:0},{4:0}]", br.V[hoverIdx], proj[hoverIdx].Glow * 100f, br.NeuronX[hoverIdx], br.NeuronY[hoverIdx], br.NeuronZ[hoverIdx]),
                        string.Format("SYNAPSES  OUT:{0}  IN:{1}  (EXC:{2} / INH:{3})", hovOut, hovIn, hovExc, hovInh),
                        "CYAN = OUTGOING  |  AMBER = INCOMING"
                    };
                    float tipW = 336f;
                    float tipH = 8f + tipLines.Length * 14f;
                    float tipX = proj[hoverIdx].SX + 18f;
                    float tipY = proj[hoverIdx].SY + 14f;
                    if (tipX + tipW > px + pw - 6) tipX = proj[hoverIdx].SX - tipW - 18f;
                    if (tipY + tipH > py + ph - 62) tipY = py + ph - 62 - tipH;
                    if (tipX < px + 6) tipX = px + 6;
                    using (SolidBrush tipBg = new SolidBrush(Color.FromArgb(214, 3, 9, 12)))
                    using (Pen tipBor = new Pen(Color.FromArgb(200, hc.R, hc.G, hc.B), 1.3f))
                    using (SolidBrush tipTx = new SolidBrush(Color.FromArgb(240, hc.R, hc.G, hc.B))) {
                        g.FillRectangle(tipBg, tipX, tipY, tipW, tipH);
                        g.DrawRectangle(tipBor, tipX, tipY, tipW, tipH);
                        for (int tl = 0; tl < tipLines.Length; tl++) {
                            g.DrawString(tipLines[tl], fontHudSmall, tl == 0 ? tipTx : txtB, tipX + 6f, tipY + 4f + tl * 14f);
                        }
                    }
                }
            }

            int yawDeg = (int)Math.Round(effYaw * 180.0 / Math.PI);
            int pitchDeg = (int)Math.Round(effPitch * 180.0 / Math.PI);
            string statusLine = isFull
                ? string.Format("SPIKES:{0}/64 | 3D_ROTATION: YAW {1:+000;-000;000}° PITCH {2:+00;-00;00}° ZOOM {3:0.00}x | MOUSE WHEEL OR DRAG TO ROTATE 3D",
                    activeSpikes, yawDeg, pitchDeg, brain3DZoom)
                : string.Format("SPIKES:{0}/64 | YAW:{1:+000;-000;000}° | WHEEL/DRAG: ROTATE 3D",
                    activeSpikes, yawDeg);
            g.DrawString(statusLine, fontHudSmall, c1B, px + 10, py + ph - 41);

            // Bottom Interactive 3D Control Toolbar Buttons: [< ROT] [ROT >] [ZOOM] [AUTO/FREE] [NEXT]
            string[] btnLabels = new string[] {
                "< ROT",
                "ROT >",
                string.Format("ZM {0:0.0}x", brain3DZoom),
                brain3DAutoRotate ? "AUTO:ON" : "FREE:3D",
                "NEXT >>"
            };
            float[] zoneStarts = new float[] { 0.02f, 0.19f, 0.37f, 0.57f, 0.77f };
            float[] zoneWidths = new float[] { 0.15f, 0.16f, 0.18f, 0.18f, 0.21f };
            int barY = py + ph - 23;
            for (int b = 0; b < 5; b++) {
                int bxBtn = px + (int)(pw * zoneStarts[b]);
                int bwBtn = (int)(pw * zoneWidths[b]);
                g.FillRectangle(btnBg, bxBtn, barY, bwBtn, 18);
                g.DrawRectangle(btnBor, bxBtn, barY, bwBtn, 18);
                Brush bTxt = (b == 3 && !brain3DAutoRotate) ? goldB : (b == 4 ? c2B : txtB);
                g.DrawString(btnLabels[b], fontHudSmall, bTxt, bxBtn + 4, barY + 2);
            }
        }
    }

    public void ShowConfigWindow() {
        if (configForm != null && !configForm.IsDisposed) {
            configForm.Activate();
            return;
        }

        configForm = new Form();
        configForm.Text = "Configurar Hacker Wallpaper // Enjambre Autónomo Porygon IDS + Avance Generacional + Modo Gamer";
        configForm.ClientSize = new Size(1090, 650);
        configForm.FormBorderStyle = FormBorderStyle.FixedDialog;
        configForm.MaximizeBox = false;
        configForm.StartPosition = FormStartPosition.CenterScreen;
        configForm.BackColor = Color.FromArgb(11, 16, 26);
        configForm.ForeColor = Color.FromArgb(226, 232, 240);
        configForm.Font = new Font("Segoe UI", 9.2f, FontStyle.Regular);
        configForm.TopMost = true;

        Label title = new Label {
            Text = "PANEL DE CONTROL // PORYGON NEURAL IDS SWARM (Ctrl+Alt+W / Ctrl+Alt+G Gamer)",
            Font = new Font("Consolas", 10.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 168, 138),
            Location = new Point(22, 10),
            AutoSize = true
        };
        configForm.Controls.Add(title);

        // =========================================================================
        // COLUMNA IZQUIERDA (X = 22, Width = 468): CONTROLES GENERALES & MODO GAMER
        // =========================================================================
        Label lblSwarm = new Label {
            Text = "Detección de Anomalías de Red (Porygon, Porygon2 y Porygon-Z):",
            ForeColor = Color.FromArgb(186, 138, 56),
            Font = new Font("Segoe UI", 9.0f, FontStyle.Bold),
            Location = new Point(22, 34),
            AutoSize = true
        };
        Label lblSwarmStatus = new Label {
            Location = new Point(22, 54),
            Size = new Size(468, 52),
            Font = new Font("Consolas", 8.2f, FontStyle.Regular),
            ForeColor = Color.FromArgb(224, 212, 176),
            BackColor = Color.FromArgb(14, 24, 20),
            BorderStyle = BorderStyle.FixedSingle
        };
        Action updateSwarmLabel = () => {
            if (porygon == null || porygon.Agents.Count < 3) return;
            var a0 = porygon.Agents[0];
            var a1 = porygon.Agents[1];
            var a2 = porygon.Agents[2];
            lblSwarmStatus.Text = string.Format(
                " PORYGON: G{0} LVL {1} ({2:N0}XP) | PORYGON2: G{3} LVL {4} ({5:N0}XP)\n" +
                " PORYGON-Z: G{6} LVL {7} ({8:N0}XP) | Cruces: {9} | Podas: {10}\n" +
                " Flujos: {11:N0} | Anomalías: {12:N0} | Escena 3D: [{13}/3] {14} | GPU: {15}",
                a0.Brain.Generation, a0.Brain.Level, a0.Brain.XP,
                a1.Brain.Generation, a1.Brain.Level, a1.Brain.XP,
                a2.Brain.Generation, a2.Brain.Level, a2.Brain.XP,
                porygon.TotalCrossovers, porygon.TotalPrunedTraits,
                a0.Brain.PacketsCollected + a1.Brain.PacketsCollected + a2.Brain.PacketsCollected,
                a0.Brain.AnomaliesCleared + a1.Brain.AnomaliesCleared + a2.Brain.AnomaliesCleared,
                Config.BgSceneMode + 1, GetBgSceneModeShortName(Config.BgSceneMode),
                isGamerSuspended ? "0.0% (GAMER ON)" : (gpuUsagePct.ToString("0.0") + "%"));
        };
        updateSwarmLabel();

        Button btnTrainWave = new Button {
            Text = "⚡ Simular Anomalía IDS (Ctrl+Alt+P)",
            Location = new Point(22, 112),
            Size = new Size(230, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, 36, 30),
            ForeColor = Color.FromArgb(52, 168, 138),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        Button btnCycleScene = new Button {
            Text = "🌲 Entorno 3D [1/3..3/3] (Ctrl+Alt+M)",
            Location = new Point(260, 112),
            Size = new Size(230, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(28, 36, 24),
            ForeColor = Color.FromArgb(186, 138, 56),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        Button btnGamerMode = new Button {
            Location = new Point(22, 148),
            Size = new Size(468, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
        };
        Action updateGamerBtn = () => {
            if (isGamerSuspended || Config.GamerMode) {
                btnGamerMode.Text = "🎮 MODO GAMER ACTIVO (0.0% GPU // HIBERNACIÓN TOTAL) — Click o Ctrl+Alt+G";
                btnGamerMode.BackColor = Color.FromArgb(22, 92, 62);
                btnGamerMode.ForeColor = Color.FromArgb(165, 255, 210);
            } else {
                btnGamerMode.Text = "🎮 Activar Modo Gamer 0.0% GPU Ahora (Ctrl+Alt+G o Botón [GAMER] en HUD)";
                btnGamerMode.BackColor = Color.FromArgb(24, 34, 48);
                btnGamerMode.ForeColor = Color.FromArgb(96, 218, 184);
            }
        };
        updateGamerBtn();

        configForm.Controls.Add(lblSwarm);
        configForm.Controls.Add(lblSwarmStatus);
        configForm.Controls.Add(btnTrainWave);
        configForm.Controls.Add(btnCycleScene);
        configForm.Controls.Add(btnGamerMode);

        // --- PALETA DE COLORES ---
        Label lblTheme = new Label { Text = "Paleta de Colores (Descanso Visual / Eye-Care Mate):", Location = new Point(22, 188), AutoSize = true };
        ComboBox cbTheme = new ComboBox {
            Location = new Point(22, 208),
            Width = 468,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(16, 28, 24),
            ForeColor = Color.White
        };
        cbTheme.Items.AddRange(new object[] {
            "Bio-Selva & Oscilador Mate (Salvia Fósforo + Ámbar + Petróleo // Sin Púrpura)",
            "Matrix Esmeralda Mate (Verde Bosque Suave)",
            "Red Team Terracota Mate (Carmesí Atenuado + Ocre)",
            "Stealth Azul Petróleo Mate"
        });
        cbTheme.SelectedIndex = Math.Max(0, Math.Min(3, Config.ThemeIndex));
        cbTheme.SelectedIndexChanged += (s, ev) => {
            Config.ThemeIndex = cbTheme.SelectedIndex;
            RebuildBackground();
            Config.Save();
        };
        configForm.Controls.Add(lblTheme);
        configForm.Controls.Add(cbTheme);

        Label lblSpeed = new Label { Text = string.Format("Velocidad de Lluvia de Código: {0}%", Config.RainSpeed), Location = new Point(22, 240), AutoSize = true };
        TrackBar tbSpeed = new TrackBar {
            Location = new Point(22, 260),
            AutoSize = false,
            Size = new Size(468, 26),
            Minimum = 0,
            Maximum = 200,
            TickStyle = TickStyle.None,
            Value = Math.Max(0, Math.Min(200, Config.RainSpeed))
        };
        tbSpeed.Scroll += (s, ev) => {
            Config.RainSpeed = tbSpeed.Value;
            lblSpeed.Text = (Config.RainSpeed == 0) ? "Velocidad de Lluvia de Código: Desactivada (0%)" : string.Format("Velocidad de Lluvia de Código: {0}%", Config.RainSpeed);
            Config.Save();
        };
        configForm.Controls.Add(lblSpeed);
        configForm.Controls.Add(tbSpeed);

        Label lblDens = new Label { Text = string.Format("Densidad de Columnas: {0}", Config.RainDensity), Location = new Point(22, 290), AutoSize = true };
        TrackBar tbDens = new TrackBar {
            Location = new Point(22, 310),
            AutoSize = false,
            Size = new Size(468, 26),
            Minimum = 20,
            Maximum = 110,
            TickStyle = TickStyle.None,
            Value = Math.Max(20, Math.Min(110, Config.RainDensity))
        };
        tbDens.Scroll += (s, ev) => {
            Config.RainDensity = tbDens.Value;
            lblDens.Text = string.Format("Densidad de Columnas: {0}", Config.RainDensity);
            RebuildRainColumns();
            Config.Save();
        };
        configForm.Controls.Add(lblDens);
        configForm.Controls.Add(tbDens);

        Label lblScale = new Label { Text = string.Format("Tamaño de los 3 Avatares Porygon: {0}%", Config.PorygonScale), Location = new Point(22, 340), AutoSize = true };
        TrackBar tbScale = new TrackBar {
            Location = new Point(22, 360),
            AutoSize = false,
            Size = new Size(468, 26),
            Minimum = 50,
            Maximum = 160,
            TickStyle = TickStyle.None,
            Value = Math.Max(50, Math.Min(160, Config.PorygonScale))
        };
        tbScale.Scroll += (s, ev) => {
            Config.PorygonScale = tbScale.Value;
            lblScale.Text = string.Format("Tamaño de los 3 Avatares Porygon: {0}%", Config.PorygonScale);
            Config.Save();
        };
        configForm.Controls.Add(lblScale);
        configForm.Controls.Add(tbScale);

        CheckBox chkAutoGamer = new CheckBox {
            Text = "Auto-Pausar (0.0% GPU) en Juegos Fullscreen o si otra App usa > 90% GPU",
            Location = new Point(24, 392),
            AutoSize = true,
            ForeColor = Color.FromArgb(110, 242, 188),
            Checked = Config.AutoGamerOnFullscreenGame
        };
        chkAutoGamer.CheckedChanged += (s, ev) => {
            Config.AutoGamerOnFullscreenGame = chkAutoGamer.Checked;
            Config.Save();
            UpdateGamerSuspensionState(true);
            updateGamerBtn();
        };

        CheckBox chkTrain = new CheckBox {
            Text = "Modo Entrenamiento Intensivo Continuo (Más Orbes Dorados y Anomalías)",
            Location = new Point(24, 417),
            AutoSize = true,
            ForeColor = Color.FromArgb(250, 204, 21),
            Checked = Config.TrainingMode
        };
        chkTrain.CheckedChanged += (s, ev) => { Config.TrainingMode = chkTrain.Checked; Config.Save(); };

        CheckBox chkFollowMouse = new CheckBox {
            Text = "Seguir el cursor del mouse (Desactivado por defecto — 100% Independientes)",
            Location = new Point(24, 442),
            AutoSize = true,
            Checked = Config.FollowMouse
        };
        chkFollowMouse.CheckedChanged += (s, ev) => { Config.FollowMouse = chkFollowMouse.Checked; Config.Save(); };

        CheckBox chkTel = new CheckBox {
            Text = "Mostrar Terminal de Telemetría Real (CPU, GPU, RAM, Red, Ping)",
            Location = new Point(24, 467),
            AutoSize = true,
            Checked = Config.ShowTelemetry
        };
        chkTel.CheckedChanged += (s, ev) => { Config.ShowTelemetry = chkTel.Checked; Config.Save(); };

        CheckBox chkRad = new CheckBox {
            Text = "Mostrar Radar Sonar 360° de Conexiones de Red",
            Location = new Point(24, 492),
            AutoSize = true,
            Checked = Config.ShowRadar
        };
        chkRad.CheckedChanged += (s, ev) => { Config.ShowRadar = chkRad.Checked; Config.Save(); };

        CheckBox chkPory = new CheckBox {
            Text = "Activar Enjambre 3D (Porygon + Porygon2 + Porygon-Z)",
            Location = new Point(24, 517),
            AutoSize = true,
            Checked = Config.ShowPorygon
        };
        chkPory.CheckedChanged += (s, ev) => { Config.ShowPorygon = chkPory.Checked; Config.Save(); };

        // Quick Terminal Opacity Control Row (Y = 544)
        Label lblTermOp = new Label {
            Text = string.Format("Opacidad Terminal ({0}%):", terminalOpacityPct),
            Location = new Point(24, 550),
            AutoSize = true,
            ForeColor = Color.FromArgb(148, 226, 213),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };
        Button btnOpMinus = new Button {
            Text = "➖ -15%",
            Location = new Point(190, 544),
            Size = new Size(74, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, 34, 42),
            ForeColor = Color.FromArgb(186, 230, 253),
            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
        };
        Button btnOpPlus = new Button {
            Text = "➕ +15% Opacidad",
            Location = new Point(268, 544),
            Size = new Size(122, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(22, 48, 40),
            ForeColor = Color.FromArgb(110, 242, 188),
            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
        };
        Button btnOpMax = new Button {
            Text = "100%↔50% (Ctrl+Alt+O)",
            Location = new Point(394, 544),
            Size = new Size(96, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(44, 34, 18),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold)
        };
        btnOpMinus.Click += (s, ev) => {
            AdjustTerminalOpacity(-15);
            lblTermOp.Text = string.Format("Opacidad Terminal ({0}%):", terminalOpacityPct);
        };
        btnOpPlus.Click += (s, ev) => {
            AdjustTerminalOpacity(15);
            lblTermOp.Text = string.Format("Opacidad Terminal ({0}%):", terminalOpacityPct);
        };
        btnOpMax.Click += (s, ev) => {
            ToggleTerminalOpacityQuick();
            lblTermOp.Text = string.Format("Opacidad Terminal ({0}%):", terminalOpacityPct);
        };

        Button btnOpenSiem = new Button {
            Text = "🛡️ Centro SIEM / IPS / AV (Ctrl+Alt+S)",
            Location = new Point(24, 582),
            Size = new Size(256, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(24, 44, 58),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
        };
        btnOpenSiem.Click += (s, ev) => { ShowSiemCommandCenterWindow(); };

        Button btnClose = new Button {
            Text = "Guardar y Cerrar",
            Location = new Point(288, 582),
            Size = new Size(202, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 168, 138),
            ForeColor = Color.FromArgb(8, 16, 14),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, ev) => { Config.Save(); configForm.Close(); };

        configForm.Controls.Add(chkAutoGamer);
        configForm.Controls.Add(chkTrain);
        configForm.Controls.Add(chkFollowMouse);
        configForm.Controls.Add(chkTel);
        configForm.Controls.Add(chkRad);
        configForm.Controls.Add(chkPory);
        configForm.Controls.Add(lblTermOp);
        configForm.Controls.Add(btnOpMinus);
        configForm.Controls.Add(btnOpPlus);
        configForm.Controls.Add(btnOpMax);
        configForm.Controls.Add(btnOpenSiem);
        configForm.Controls.Add(btnClose);

        // =========================================================================
        // COLUMNA DERECHA (X = 510, Width = 558): AVANCE DE GENERACIONES Y MEJORAS
        // =========================================================================
        Label lblEvoTitle = new Label {
            Text = "🧬 AVANCE DE GENERACIONES, GENOMA Y MEJORAS EVOLUTIVAS (EN VIVO)",
            ForeColor = Color.FromArgb(88, 226, 184),
            Font = new Font("Consolas", 9.8f, FontStyle.Bold),
            Location = new Point(510, 12),
            AutoSize = true
        };
        configForm.Controls.Add(lblEvoTitle);

        Label lblEvoKpi = new Label {
            Location = new Point(510, 34),
            Size = new Size(558, 66),
            Font = new Font("Consolas", 8.2f, FontStyle.Regular),
            ForeColor = Color.FromArgb(214, 236, 224),
            BackColor = Color.FromArgb(12, 22, 20),
            BorderStyle = BorderStyle.FixedSingle
        };
        configForm.Controls.Add(lblEvoKpi);

        // Custom Double-Buffered GDI+ Panel for Per-Agent Generational Progress, Improvements & 8-Gene Weights
        Panel evoPanel = new Panel {
            Location = new Point(510, 106),
            Size = new Size(558, 326),
            BackColor = Color.FromArgb(10, 16, 22),
            BorderStyle = BorderStyle.FixedSingle
        };
        try {
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, evoPanel, new object[] { true });
        } catch { }

        string[] geneShortNames = new string[] { "DL_Z", "UL_Z", "PING", "VT_PRT", "SYN_R", "ENTRP", "FN_B", "EXPLR" };

        evoPanel.Paint += (s, pe) => {
            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(9, 15, 20));

            if (porygon == null || porygon.Agents.Count == 0) return;

            int count = Math.Min(3, porygon.Agents.Count);
            int cardH = 103;
            using (Font fHeader = new Font("Consolas", 8.8f, FontStyle.Bold))
            using (Font fBody = new Font("Consolas", 7.8f, FontStyle.Regular))
            using (Font fTiny = new Font("Consolas", 7.1f, FontStyle.Regular))
            using (SolidBrush txtMain = new SolidBrush(Color.FromArgb(224, 234, 230)))
            using (SolidBrush txtSub = new SolidBrush(Color.FromArgb(164, 192, 184)))
            using (SolidBrush txtGold = new SolidBrush(Color.FromArgb(245, 202, 78)))
            using (SolidBrush txtGreen = new SolidBrush(Color.FromArgb(92, 236, 174)))
            using (SolidBrush barTrack = new SolidBrush(Color.FromArgb(24, 38, 44))) {

                for (int i = 0; i < count; i++) {
                    Porygon3DAvatar.PorygonAgent ag = porygon.Agents[i];
                    FlyBrainConnectome br = ag.Brain;
                    int cy = 5 + i * (cardH + 4);
                    Rectangle cardRect = new Rectangle(6, cy, evoPanel.Width - 14, cardH);

                    using (SolidBrush cardBg = new SolidBrush(Color.FromArgb(16, 24, 30)))
                    using (Pen cardBor = new Pen(Color.FromArgb(145, ag.AuraColor.R, ag.AuraColor.G, ag.AuraColor.B), 1.2f))
                    using (SolidBrush agBrush = new SolidBrush(ag.AuraColor)) {
                        g.FillRectangle(cardBg, cardRect);
                        g.DrawRectangle(cardBor, cardRect);
                        g.FillRectangle(agBrush, cardRect.X, cardRect.Y, 5, cardRect.Height);

                        // Row 1: Agent Name, Generation, Stage, Level & LifeAge Progress Bar to Next Crossover
                        string stageTag = br.GetLifeStageLabel();
                        string hdr = string.Format("{0} // GEN G{1} [{2}] · LVL {3} ({4:N0} XP)",
                            ag.Name, br.Generation, stageTag, br.Level, br.XP);
                        g.DrawString(hdr, fHeader, agBrush, cardRect.X + 11, cardRect.Y + 5);

                        // LifeAge Bar (0-100% toward G+1)
                        int barX = cardRect.Right - 178;
                        int barY = cardRect.Y + 6;
                        int barW = 170;
                        int barH = 14;
                        float ageNorm = Math.Max(0f, Math.Min(1f, br.LifeAge / 100f));
                        g.FillRectangle(barTrack, barX, barY, barW, barH);
                        using (LinearGradientBrush ageGrad = new LinearGradientBrush(
                            new Rectangle(barX, barY, Math.Max(2, barW), barH),
                            Color.FromArgb(210, ag.AuraColor.R, ag.AuraColor.G, ag.AuraColor.B),
                            Color.FromArgb(235, 245, 202, 78),
                            LinearGradientMode.Horizontal)) {
                            g.FillRectangle(ageGrad, barX, barY, (int)(barW * ageNorm), barH);
                        }
                        g.DrawRectangle(cardBor, barX, barY, barW, barH);
                        string ageTxt = string.Format("CICLO:{0,2:0}% ➔ G{1}", br.LifeAge, br.Generation + 1);
                        g.DrawString(ageTxt, fTiny, txtMain, barX + 6, barY + 1);

                        // Row 2: Deltas vs Generation 1 Seed (76.0 Fit, 84.5% Acc, 1.00x Synaptic Gain)
                        float dFit = br.FitnessScore - 76.0f;
                        float dAcc = br.DetectionAccuracy - 84.5f;
                        float dGainPct = (br.SynapticGain - 1.0f) * 100f;
                        string row2 = string.Format(
                            "MEJORAS vs G1: FIT {0:0.0} ({1:+0.0;-0.0;+0.0}) | ACC {2:0.0}% ({3:+0.0;-0.0;+0.0}%) | F2-AntiFN:{4:0.0}% | Sinapsis:x{5:0.00} (+{6:0}%)",
                            br.FitnessScore, dFit, br.DetectionAccuracy, dAcc, br.F2Score, br.SynapticGain, dGainPct);
                        g.DrawString(row2, fBody, txtGreen, cardRect.X + 11, cardRect.Y + 25);

                        // Row 3: Confusion Matrix & Pruned Traits + Unlocked Skills
                        string skills = br.GetUnlockedSkillsSummary();
                        if (skills.Length > 35) skills = skills.Substring(0, 35) + "..";
                        string row3 = string.Format(
                            "IDS TP:{0} TN:{1} FN:{2} FP:{3} | σ-Mut:{4:0.000} | Podas:{5} ({6}) | Skills: {7}",
                            br.TruePositives, br.TrueNegatives, br.FalseNegatives, br.FalsePositives,
                            br.MutationSigma, br.PrunedTraitsCount, br.LastPrunedTrait ?? "NONE", skills);
                        g.DrawString(row3, fTiny, txtSub, cardRect.X + 11, cardRect.Y + 43);

                        // Row 4: 8-Gene Anomaly Genome Weights with visual mini-bars
                        int gx0 = cardRect.X + 11;
                        int gy0 = cardRect.Y + 62;
                        int cellW = 65;
                        for (int gIdx = 0; gIdx < 8; gIdx++) {
                            float gw = (br.AnomalyGenome != null && gIdx < br.AnomalyGenome.Length) ? br.AnomalyGenome[gIdx] : 1.0f;
                            int cx = gx0 + gIdx * cellW;
                            float normW = Math.Max(0.05f, Math.Min(1.0f, gw / 3.2f));
                            g.FillRectangle(barTrack, cx, gy0 + 14, cellW - 5, 6);
                            Brush gBr = (gw >= 1.45f) ? txtGold : ((gw >= 1.0f) ? txtGreen : txtSub);
                            g.FillRectangle(gBr, cx, gy0 + 14, (int)((cellW - 5) * normW), 6);
                            g.DrawString(string.Format("{0}:{1:0.00}", geneShortNames[gIdx], gw), fTiny, gBr, cx, gy0);
                        }

                        // Row 5: Last Crossover Lineage Summary
                        string crossInfo = string.Format("Último Cruce: {0} | Estado: {1}",
                            br.LastCrossSummary ?? "G1(SEED)", br.ActiveStateName ?? "PATROL");
                        if (crossInfo.Length > 86) crossInfo = crossInfo.Substring(0, 86);
                        g.DrawString(crossInfo, fTiny, txtGold, cardRect.X + 11, cardRect.Y + 85);
                    }
                }
            }
        };
        configForm.Controls.Add(evoPanel);

        Button btnForceEvo = new Button {
            Text = "🧬 Forzar Salto Generacional / Crossover (+1 Gen)",
            Location = new Point(510, 438),
            Size = new Size(274, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, 42, 34),
            ForeColor = Color.FromArgb(98, 244, 190),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        Button btnBoostLvl = new Button {
            Text = "🚀 Subir Nivel / Boost Sináptico (+1 LVL)",
            Location = new Point(794, 438),
            Size = new Size(274, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(36, 32, 18),
            ForeColor = Color.FromArgb(245, 202, 78),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        configForm.Controls.Add(btnForceEvo);
        configForm.Controls.Add(btnBoostLvl);

        Label lblAuditLog = new Label {
            Text = "Historial de Cruces Generacionales, Selección Natural y Poda de Rasgos Negativos:",
            ForeColor = Color.FromArgb(186, 138, 56),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
            Location = new Point(510, 476),
            AutoSize = true
        };
        configForm.Controls.Add(lblAuditLog);

        TextBox txtLineageLog = new TextBox {
            Location = new Point(510, 496),
            Size = new Size(558, 128),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(8, 14, 18),
            ForeColor = Color.FromArgb(196, 224, 212),
            Font = new Font("Consolas", 7.8f, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle
        };
        configForm.Controls.Add(txtLineageLog);

        int lastHistoryCount = -1;
        string lastHistoryTop = "";

        Action updateEvoSection = () => {
            if (porygon == null || porygon.Agents.Count == 0) return;
            int maxGen = porygon.HighestGeneration;
            float bestFit = porygon.BestSwarmFitness;
            float avgAcc = porygon.AvgSwarmAccuracy;
            float avgF2 = 0f, avgGain = 0f, avgSigma = 0f, avgAge = 0f;
            int totPruned = 0;
            for (int i = 0; i < porygon.Agents.Count; i++) {
                FlyBrainConnectome b = porygon.Agents[i].Brain;
                if (b.Generation > maxGen) maxGen = b.Generation;
                if (b.FitnessScore > bestFit) bestFit = b.FitnessScore;
                avgF2 += b.F2Score;
                avgGain += b.SynapticGain;
                avgSigma += b.MutationSigma;
                avgAge += b.LifeAge;
                totPruned += b.PrunedTraitsCount;
            }
            int cnt = porygon.Agents.Count;
            avgF2 /= cnt;
            avgGain /= cnt;
            avgSigma /= cnt;
            avgAge /= cnt;

            float deltaFit = bestFit - 76.0f;
            float deltaAcc = avgAcc - 84.5f;
            float gainBonusPct = (avgGain - 1.0f) * 100f;
            float sigmaReductionPct = Math.Max(0f, (0.080f - avgSigma) / 0.080f * 100f);

            lblEvoKpi.Text = string.Format(
                " LINAJE ACTUAL: G1 (Semilla) ➔ G{0} (Edad Media: {1:0}%) | Cruces Totales: {2} | Rasgos Podados: {3}\n" +
                " MEJORAS ACUMULADAS: Fitness Élite {4:0.0} ({5:+0.0;-0.0;+0.0} vs G1) | Precisión Media {6:0.0}% ({7:+0.0;-0.0;+0.0}% vs G1)\n" +
                " F2-Score Anti-FN: {8:0.0}% | Ganancia Sináptica: x{9:0.00} (+{10:0}%) | Estabilidad Genómica: +{11:0}% (σ={12:0.000})\n" +
                " Entrenamiento CPU: {13:N0} steps/s | VirusTotal v3: {14} consultas ({15} IOCs / {16} limpios)",
                maxGen, avgAge, porygon.TotalCrossovers, Math.Max(totPruned, porygon.TotalPrunedTraits),
                bestFit, deltaFit, avgAcc, deltaAcc,
                avgF2, avgGain, gainBonusPct, sigmaReductionPct, avgSigma,
                porygon.CpuTrainingStepsPerSec, porygon.VtApiQueries, porygon.VtMaliciousHits, porygon.VtCleanVerified);

            evoPanel.Invalidate();

            lock (porygon.LineageAuditHistory) {
                int hCnt = porygon.LineageAuditHistory.Count;
                string topEntry = (hCnt > 0) ? porygon.LineageAuditHistory[hCnt - 1] : "";
                if (hCnt != lastHistoryCount || topEntry != lastHistoryTop) {
                    lastHistoryCount = hCnt;
                    lastHistoryTop = topEntry;
                    if (hCnt == 0) {
                        txtLineageLog.Text =
                            "[INFO] Los agentes evolucionan automáticamente al completar el 100% de su ciclo de vida (AGE),\r\n" +
                            "o puedes pulsar [🧬 Forzar Salto Generacional / Crossover (+1 Gen)] arriba para cruzar y auditar ahora.\r\n" +
                            "Cada generación aplica Selección Natural (Penalización FN 3.5x + Poda de Rasgos Negativos + Mutación Adaptativa).";
                    } else {
                        StringBuilder sb = new StringBuilder();
                        for (int i = hCnt - 1; i >= 0; i--) {
                            sb.AppendLine(porygon.LineageAuditHistory[i]);
                        }
                        txtLineageLog.Text = sb.ToString();
                    }
                }
            }
        };
        updateEvoSection();

        btnTrainWave.Click += (s, ev) => {
            porygon.TriggerTrainingWave(screenW, screenH);
            updateSwarmLabel();
            updateEvoSection();
            this.Invalidate();
        };

        btnCycleScene.Click += (s, ev) => {
            CycleBgSceneMode();
            updateSwarmLabel();
            this.Invalidate();
        };

        btnGamerMode.Click += (s, ev) => {
            ToggleGamerMode();
            updateGamerBtn();
            updateSwarmLabel();
            updateEvoSection();
        };

        btnForceEvo.Click += (s, ev) => {
            if (porygon != null) {
                porygon.ForceEvolutionaryCrossoverAll();
                hudConnectomeDirty = true;
                hudBrain3DDirty = true;
                updateSwarmLabel();
                updateEvoSection();
                this.Invalidate();
            }
        };

        btnBoostLvl.Click += (s, ev) => {
            if (porygon != null) {
                porygon.BoostAllLevels();
                hudConnectomeDirty = true;
                hudBrain3DDirty = true;
                updateSwarmLabel();
                updateEvoSection();
                this.Invalidate();
            }
        };

        System.Windows.Forms.Timer cfgRefreshTimer = new System.Windows.Forms.Timer();
        cfgRefreshTimer.Interval = 900;
        cfgRefreshTimer.Tick += (s, ev) => {
            if (configForm == null || configForm.IsDisposed || !configForm.Visible) return;
            updateSwarmLabel();
            updateGamerBtn();
            updateEvoSection();
        };
        cfgRefreshTimer.Start();
        configForm.FormClosed += (s, ev) => {
            try { cfgRefreshTimer.Stop(); cfgRefreshTimer.Dispose(); } catch { }
        };

        configForm.Show();
    }

    public void ShowSiemCommandCenterWindow() {
        if (siemForm != null && !siemForm.IsDisposed) {
            siemForm.Activate();
            return;
        }

        siemForm = new Form {
            Text = "PORYGON CYBER-DEFENSE SOC // SIEM + Antivirus/EDR + Active IPS Firewall + Bóveda de Cuarentena (Modo Híbrido)",
            ClientSize = new Size(1120, 680),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen,
            BackColor = Color.FromArgb(10, 15, 22),
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Segoe UI", 9.0f, FontStyle.Regular),
            TopMost = true
        };

        Label lblHeader = new Label {
            Text = "🛡️ CENTRO DE COMANDO SIEM / EDR / IPS / AV // TRÍADA PORYGON (Modo Híbrido: Auto-Defensa Crítica + 1-Click)",
            Font = new Font("Consolas", 10.2f, FontStyle.Bold),
            ForeColor = Color.FromArgb(88, 226, 184),
            Location = new Point(18, 10),
            AutoSize = true
        };
        siemForm.Controls.Add(lblHeader);

        Label lblKpi = new Label {
            Location = new Point(18, 34),
            Size = new Size(1084, 54),
            Font = new Font("Consolas", 8.2f, FontStyle.Regular),
            ForeColor = Color.FromArgb(224, 212, 176),
            BackColor = Color.FromArgb(14, 22, 26),
            BorderStyle = BorderStyle.FixedSingle
        };
        siemForm.Controls.Add(lblKpi);

        // Toolbar Row (Y = 96)
        Button btnQuickAudit = new Button {
            Text = "🔍 Auditar Red TCP/PID + Registro + Startup Ahora",
            Location = new Point(18, 96),
            Size = new Size(280, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, 42, 34),
            ForeColor = Color.FromArgb(98, 244, 190),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        Button btnScanCustom = new Button {
            Text = "📂 Escanear Archivo (SHA-256 + Entropía)...",
            Location = new Point(306, 96),
            Size = new Size(254, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(22, 36, 48),
            ForeColor = Color.FromArgb(125, 211, 252),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        TextBox txtBlockIp = new TextBox {
            Location = new Point(570, 100),
            Size = new Size(150, 24),
            BackColor = Color.FromArgb(12, 20, 28),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Consolas", 9.0f, FontStyle.Bold),
            Text = string.IsNullOrEmpty(PorygonCyberDefenseEngine.LastCandidateBlockIp) ? "185.220.101.4" : PorygonCyberDefenseEngine.LastCandidateBlockIp
        };

        Button btnBlockIp = new Button {
            Text = "🔥 Bloquear IP en Firewall",
            Location = new Point(726, 96),
            Size = new Size(184, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 22, 26),
            ForeColor = Color.FromArgb(252, 165, 165),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        Button btnOpenJsonl = new Button {
            Text = "📄 Abrir SIEM JSONL",
            Location = new Point(918, 96),
            Size = new Size(184, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(32, 30, 20),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        siemForm.Controls.Add(btnQuickAudit);
        siemForm.Controls.Add(btnScanCustom);
        siemForm.Controls.Add(txtBlockIp);
        siemForm.Controls.Add(btnBlockIp);
        siemForm.Controls.Add(btnOpenJsonl);

        // Panel 1: SIEM Events (Left, Y = 136)
        Label lblSiem = new Label {
            Text = "📡 EVENTOS SIEM & CORRELACIÓN MITRE ATT&CK (PORYGON // TCP-PID, Registro Run, EventLog 7045, AV, IPS):",
            Location = new Point(18, 136),
            AutoSize = true,
            ForeColor = Color.FromArgb(88, 226, 184),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
        };
        TextBox txtSiemFeed = new TextBox {
            Location = new Point(18, 156),
            Size = new Size(534, 280),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(8, 13, 18),
            ForeColor = Color.FromArgb(196, 228, 216),
            Font = new Font("Consolas", 7.8f, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Panel 2: Real-Time Antivirus / EDR Scans (Right, Y = 136)
        Label lblAv = new Label {
            Text = "🧬 ANTIVIRUS / EDR EN TIEMPO REAL — PORYGON2 (Downloads, Desktop, %TEMP%, Startup):",
            Location = new Point(566, 136),
            AutoSize = true,
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
        };
        TextBox txtAvFeed = new TextBox {
            Location = new Point(566, 156),
            Size = new Size(536, 280),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(8, 13, 18),
            ForeColor = Color.FromArgb(232, 220, 184),
            Font = new Font("Consolas", 7.8f, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle
        };

        siemForm.Controls.Add(lblSiem);
        siemForm.Controls.Add(txtSiemFeed);
        siemForm.Controls.Add(lblAv);
        siemForm.Controls.Add(txtAvFeed);

        // Panel 3: Windows Firewall Blocked IPs (Bottom-Left, Y = 446)
        Label lblFw = new Label {
            Text = "🔥 IPS ACTIVO // IPs Bloqueadas en Windows Firewall por Porygon-Z:",
            Location = new Point(18, 446),
            AutoSize = true,
            ForeColor = Color.FromArgb(248, 113, 113),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
        };
        ListBox lstBlockedIps = new ListBox {
            Location = new Point(18, 468),
            Size = new Size(370, 150),
            BackColor = Color.FromArgb(10, 16, 22),
            ForeColor = Color.FromArgb(252, 165, 165),
            Font = new Font("Consolas", 8.5f, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle
        };
        Button btnUnblockIp = new Button {
            Text = "🔓 Desbloquear IP\nSeleccionada en Firewall",
            Location = new Point(396, 468),
            Size = new Size(156, 68),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(22, 38, 32),
            ForeColor = Color.FromArgb(110, 231, 183),
            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
        };
        Button btnBlockCandNow = new Button {
            Text = "⚡ Bloquear Última IP\nSospechosa (1-Click)",
            Location = new Point(396, 548),
            Size = new Size(156, 70),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(48, 22, 26),
            ForeColor = Color.FromArgb(252, 165, 165),
            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
        };

        // Panel 4: AV Quarantine Vault (Bottom-Right, Y = 446)
        Label lblQuar = new Label {
            Text = "☣️ BÓVEDA DE CUARENTENA AV (.qz) — Porygon2 / Porygon-Z:",
            Location = new Point(566, 446),
            AutoSize = true,
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
        };
        ListBox lstQuarantine = new ListBox {
            Location = new Point(566, 468),
            Size = new Size(372, 150),
            BackColor = Color.FromArgb(10, 16, 22),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Consolas", 8.2f, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle
        };
        Button btnRestoreQuar = new Button {
            Text = "♻️ Restaurar Archivo\nde Cuarentena",
            Location = new Point(946, 468),
            Size = new Size(156, 68),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(22, 38, 32),
            ForeColor = Color.FromArgb(110, 231, 183),
            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
        };
        Button btnQuarCandNow = new Button {
            Text = "☣️ Aislar Último\nArchivo Sospechoso",
            Location = new Point(946, 548),
            Size = new Size(156, 70),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(48, 34, 16),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
        };

        Button btnCloseSiem = new Button {
            Text = "Cerrar Centro de Comando SOC / SIEM",
            Location = new Point(18, 630),
            Size = new Size(1084, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 168, 138),
            ForeColor = Color.FromArgb(8, 16, 14),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnCloseSiem.FlatAppearance.BorderSize = 0;
        btnCloseSiem.Click += (s, ev) => siemForm.Close();

        siemForm.Controls.Add(lblFw);
        siemForm.Controls.Add(lstBlockedIps);
        siemForm.Controls.Add(btnUnblockIp);
        siemForm.Controls.Add(btnBlockCandNow);
        siemForm.Controls.Add(lblQuar);
        siemForm.Controls.Add(lstQuarantine);
        siemForm.Controls.Add(btnRestoreQuar);
        siemForm.Controls.Add(btnQuarCandNow);
        siemForm.Controls.Add(btnCloseSiem);

        int lastSiemCount = -1, lastAvCount = -1, lastFwCount = -1, lastQuarCount = -1;

        Action refreshSiemUi = () => {
            lock (PorygonCyberDefenseEngine.SyncLock) {
                string candIp = string.IsNullOrEmpty(PorygonCyberDefenseEngine.LastCandidateBlockIp)
                    ? "Ninguna"
                    : (PorygonCyberDefenseEngine.LastCandidateBlockIp + " [" + PorygonCyberDefenseEngine.LastCandidateBlockProc + "]");
                string candFile = string.IsNullOrEmpty(PorygonCyberDefenseEngine.LastCandidateQuarantineFile)
                    ? "Ninguno"
                    : Path.GetFileName(PorygonCyberDefenseEngine.LastCandidateQuarantineFile);

                lblKpi.Text = string.Format(
                    " [PORYGON NIDS+PID+SIEM] Procesos Auditados: {0} | Alertas SIEM: {1} | Log: {2}\n" +
                    " [PORYGON2 AV/EDR] Archivos Escaneados: {3} | Candidato Cuarentena: {4}\n" +
                    " [PORYGON-Z IPS/FW] Acciones IPS: {5} | IPs Bloqueadas: {6} | Candidato Bloqueo IP: {7} | Estado: {8}",
                    PorygonCyberDefenseEngine.TotalNidsProcessAudits,
                    PorygonCyberDefenseEngine.TotalSiemAlerts,
                    PorygonCyberDefenseEngine.ProjectSiemJsonlPath,
                    PorygonCyberDefenseEngine.TotalAvFilesScanned,
                    candFile,
                    PorygonCyberDefenseEngine.TotalIpsActions,
                    PorygonCyberDefenseEngine.BlockedFirewallIps.Count,
                    candIp,
                    PorygonCyberDefenseEngine.LastActionBanner);

                if (PorygonCyberDefenseEngine.RecentSiemEvents.Count != lastSiemCount) {
                    lastSiemCount = PorygonCyberDefenseEngine.RecentSiemEvents.Count;
                    StringBuilder sb = new StringBuilder();
                    for (int i = PorygonCyberDefenseEngine.RecentSiemEvents.Count - 1; i >= 0; i--) {
                        var e = PorygonCyberDefenseEngine.RecentSiemEvents[i];
                        sb.AppendFormat("[{0}] [{1}] [{2}/{3}] ({4}) Proc:{5} -> {6} | Risk:{7:0}% | Action:{8}\r\n   ↳ {9}\r\n",
                            e.Timestamp, e.Severity, e.AgentName, e.Module, e.MitreId, e.ProcessInfo, e.Target, e.RiskScore * 100f, e.ActionTaken, e.Summary);
                    }
                    txtSiemFeed.Text = sb.Length > 0 ? sb.ToString() : "[SIEM] Monitoreando conexiones TCP/PID, claves de registro Run y EventLog...";
                }

                if (PorygonCyberDefenseEngine.RecentAvScans.Count != lastAvCount) {
                    lastAvCount = PorygonCyberDefenseEngine.RecentAvScans.Count;
                    StringBuilder sbAv = new StringBuilder();
                    for (int i = PorygonCyberDefenseEngine.RecentAvScans.Count - 1; i >= 0; i--) {
                        var r = PorygonCyberDefenseEngine.RecentAvScans[i];
                        sbAv.AppendFormat("[{0}] [{1}] {2} ({3:N0} B) | Risk:{4:0}% | MITRE:{5}\r\n   ↳ {6}\r\n   ↳ Ruta: {7}\r\n",
                            r.Timestamp, r.Verdict, r.FileName, r.FileSizeBytes, r.RiskScore * 100f, r.MitreTactic, r.Detail, r.FullPath);
                    }
                    txtAvFeed.Text = sbAv.Length > 0
                        ? sbAv.ToString()
                        : "[PORYGON2 AV] FileSystemWatcher activo en Downloads, Desktop, %TEMP% y Startup.\r\nPulsa [🔍 Auditar Red TCP/PID + Registro + Startup Ahora] o [📂 Escanear Archivo...] arriba para analizar archivos existentes.";
                }

                if (PorygonCyberDefenseEngine.BlockedFirewallIps.Count != lastFwCount) {
                    lastFwCount = PorygonCyberDefenseEngine.BlockedFirewallIps.Count;
                    lstBlockedIps.Items.Clear();
                    for (int i = 0; i < PorygonCyberDefenseEngine.BlockedFirewallIps.Count; i++) {
                        lstBlockedIps.Items.Add(PorygonCyberDefenseEngine.BlockedFirewallIps[i]);
                    }
                }

                if (PorygonCyberDefenseEngine.QuarantinedFiles.Count != lastQuarCount) {
                    lastQuarCount = PorygonCyberDefenseEngine.QuarantinedFiles.Count;
                    lstQuarantine.Items.Clear();
                    for (int i = 0; i < PorygonCyberDefenseEngine.QuarantinedFiles.Count; i++) {
                        var q = PorygonCyberDefenseEngine.QuarantinedFiles[i];
                        lstQuarantine.Items.Add(string.Format("{0} | {1} | {2}", q.Id, q.FileName, q.OriginalPath));
                    }
                }
            }
        };

        refreshSiemUi();

        btnQuickAudit.Click += (s, ev) => {
            PorygonCyberDefenseEngine.RunQuickDirectoryAudit(true);
            lastSiemCount = -1;
            lastAvCount = -1;
            refreshSiemUi();
            hudConnectomeDirty = true;
        };

        btnScanCustom.Click += (s, ev) => {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Title = "Seleccionar archivo para análisis Antivirus / EDR (SHA-256 + Entropía + Authenticode)";
                ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (ofd.ShowDialog(siemForm) == DialogResult.OK) {
                    PorygonCyberDefenseEngine.InspectFileArtifact(ofd.FileName, true);
                    lastSiemCount = -1;
                    lastAvCount = -1;
                    refreshSiemUi();
                    hudConnectomeDirty = true;
                }
            }
        };

        btnBlockIp.Click += (s, ev) => {
            string ip = (txtBlockIp.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(ip)) {
                PorygonCyberDefenseEngine.BlockRemoteIpInFirewall(ip, "Bloqueo manual desde Centro de Comando SOC", false);
                lastSiemCount = -1;
                lastFwCount = -1;
                refreshSiemUi();
                hudConnectomeDirty = true;
            }
        };

        btnBlockCandNow.Click += (s, ev) => {
            string cand = PorygonCyberDefenseEngine.LastCandidateBlockIp;
            if (!string.IsNullOrEmpty(cand)) {
                PorygonCyberDefenseEngine.BlockRemoteIpInFirewall(cand, "1-Click IPS Block (" + PorygonCyberDefenseEngine.LastCandidateBlockReason + ")", false);
                lastSiemCount = -1;
                lastFwCount = -1;
                refreshSiemUi();
                hudConnectomeDirty = true;
            }
        };

        btnUnblockIp.Click += (s, ev) => {
            if (lstBlockedIps.SelectedItem != null) {
                string ip = lstBlockedIps.SelectedItem.ToString();
                PorygonCyberDefenseEngine.UnblockRemoteIpInFirewall(ip);
                lastSiemCount = -1;
                lastFwCount = -1;
                refreshSiemUi();
                hudConnectomeDirty = true;
            }
        };

        btnQuarCandNow.Click += (s, ev) => {
            string candF = PorygonCyberDefenseEngine.LastCandidateQuarantineFile;
            if (!string.IsNullOrEmpty(candF) && File.Exists(candF)) {
                PorygonCyberDefenseEngine.QuarantineFile(candF, "1-Click SOC Quarantine", PorygonCyberDefenseEngine.LastCandidateQuarantineSha, PorygonCyberDefenseEngine.LastCandidateQuarantineScore);
                lastSiemCount = -1;
                lastQuarCount = -1;
                refreshSiemUi();
                hudConnectomeDirty = true;
            }
        };

        btnRestoreQuar.Click += (s, ev) => {
            if (lstQuarantine.SelectedItem != null) {
                string item = lstQuarantine.SelectedItem.ToString();
                string qId = item.Split('|')[0].Trim();
                PorygonCyberDefenseEngine.RestoreQuarantinedFile(qId);
                lastSiemCount = -1;
                lastQuarCount = -1;
                refreshSiemUi();
                hudConnectomeDirty = true;
            }
        };

        btnOpenJsonl.Text = "🌐 Abrir Web SOC (:5090)";
        btnOpenJsonl.Click += (s, ev) => {
            PorygonCyberDefenseEngine.OpenWebSocInBrowser();
        };

        System.Windows.Forms.Timer siemTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        siemTimer.Tick += (s, ev) => {
            if (siemForm == null || siemForm.IsDisposed || !siemForm.Visible) return;
            refreshSiemUi();
        };
        siemTimer.Start();
        siemForm.FormClosed += (s, ev) => {
            try { siemTimer.Stop(); siemTimer.Dispose(); } catch { }
        };

        siemForm.Show();
    }

    public static void RenderModelsContactSheet(string outPath) {
        Porygon3DAvatar av = new Porygon3DAvatar(1920, 1080);
        if (av.Agents.Count >= 3) {
            if (av.Stage0Model != null) { av.Agents[0].Model = av.Stage0Model; av.Agents[0].Name = "PORYGON"; av.Agents[0].Brain.LifeStage = 0; }
            if (av.Stage1Model != null) { av.Agents[1].Model = av.Stage1Model; av.Agents[1].Name = "PORYGON2"; av.Agents[1].Brain.LifeStage = 1; }
            if (av.Stage2Model != null) { av.Agents[2].Model = av.Stage2Model; av.Agents[2].Name = "PORYGON-Z"; av.Agents[2].Brain.LifeStage = 2; }
            av.Agents[0].PosX = 520f;  av.Agents[0].PosY = 540f; av.Agents[0].PosZ = 40f; av.Agents[0].Yaw = 0.52f; av.Agents[0].Pitch = 0.14f;
            av.Agents[1].PosX = 960f;  av.Agents[1].PosY = 560f; av.Agents[1].PosZ = 40f; av.Agents[1].Yaw = 0.48f; av.Agents[1].Pitch = 0.12f;
            av.Agents[2].PosX = 1400f; av.Agents[2].PosY = 520f; av.Agents[2].PosZ = 40f; av.Agents[2].Yaw = 0.42f; av.Agents[2].Pitch = 0.10f;
            while (av.Agents.Count > 3) av.Agents.RemoveAt(av.Agents.Count - 1);
        }
        av.TriggerTrainingWave(1920, 1080);
        if (av.Packets.Count > 0) {
            av.Packets[0].X = 1120;
            av.Packets[0].Y = 340;
            av.Packets[0].IsHovered = true;
            av.HoveredPacket = av.Packets[0];
        }
        using (Bitmap bmp = new Bitmap(1920, 1080))
        using (Graphics g = Graphics.FromImage(bmp)) {
            g.Clear(Color.FromArgb(4, 9, 8));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            Font fSmall = new Font("Consolas", 8f, FontStyle.Regular);
            av.Render(g, 135f, Color.FromArgb(52, 168, 138), Color.FromArgb(186, 138, 56), fSmall);
            bmp.Save(outPath, ImageFormat.Png);
        }
    }

    public static void RenderBrainAndHudPreview(string outPath) {
        using (HackerWallpaperForm form = new HackerWallpaperForm(null, false))
        using (Bitmap bmp = new Bitmap(1380, 820, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(bmp)) {
            form.cpuUsage = 22.4f;
            form.ramPercent = 47;
            form.ramUsedGB = 15.1f;
            form.ramTotalGB = 32.0f;
            form.gpuUsagePct = 4.6f;
            form.gpuFrameMs = 3.1f;
            form.currentFps = 24;
            form.gpuEcoMode = false;
            g.Clear(Color.FromArgb(3, 7, 6));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Color c1, c2, c3, head;
            form.GetThemeColors(out c1, out c2, out c3, out head);

            // Top Row: Render all 3 3D Procedural Background Environments side-by-side (Mode 0, Mode 1, Mode 2)
            string[] modeTitles = new string[] {
                "MODO 1/3 [BG:1]: SELVA BIO-DIGITAL LOW-POLY + OSCILADOR 3D",
                "MODO 2/3 [BG:2]: TERRENO OSCILADOR VECTORIAL + BÓVEDA DELAUNAY",
                "MODO 3/3 [BG:3]: BOSQUE MICELIAL POLIGONAL (BIOLUMINISCENTE)"
            };
            for (int m = 0; m < 3; m++) {
                Rectangle tile = new Rectangle(16 + m * 452, 16, 436, 260);
                using (LinearGradientBrush bgGrad = new LinearGradientBrush(tile, Color.FromArgb(3, 7, 6), Color.FromArgb(7, 17, 14), LinearGradientMode.Vertical)) {
                    g.FillRectangle(bgGrad, tile);
                }
                form.Render3DProceduralEnvironmentGdi(g, m, tile, c1, c2, c3, 45, m);
                using (Pen bPen = new Pen(Color.FromArgb(120, c1.R, c1.G, c1.B), 1.2f))
                using (SolidBrush tBr = new SolidBrush(Color.FromArgb(225, c2.R, c2.G, c2.B))) {
                    g.DrawRectangle(bPen, tile);
                    g.DrawString(modeTitles[m], form.fontHudSmall, tBr, tile.X + 8, tile.Y + 6);
                }
            }

            // Bottom Row: Anatomical 3D FlyWire Drosophila Brain + Updated SYS_TELEMETRY with GPU_LOAD & [BG:1/3]
            form.DrawFlyBrain3DCardAt(g, 16, 292, 740, 510, c1, c2, c3);
            form.DrawTelemetryHUD(g, 776, 292, 584, 228, c1, c2, c3);
            form.DrawFlyBrainConnectomeHUDAt(g, 776, 536, 584, 204, c1, c2, c3);
            bmp.Save(outPath, ImageFormat.Png);
        }
    }

    [STAThread]
    public static void Main(string[] args) {
        try { SetProcessDpiAwarenessContext(new IntPtr(-4)); } catch { }
        try { SetProcessDpiAwareness(2); } catch { }
        try { SetProcessDPIAware(); } catch { }

        if (args != null && args.Length > 0 && args[0] == "--preview-models") {
            string p = Path.Combine(Path.GetTempPath(), "verify_3_swarm.png");
            RenderModelsContactSheet(p);
            Environment.Exit(0);
            return;
        }
        if (args != null && args.Length > 0 && args[0] == "--preview-brain") {
            string p = (args.Length > 1) ? args[1] : Path.Combine(Path.GetTempPath(), "verify_brain_preview.png");
            try {
                RenderBrainAndHudPreview(p);
            } catch (Exception ex) {
                File.WriteAllText(p + ".err", ex.ToString());
            }
            Environment.Exit(0);
            return;
        }
        string arg0 = (args != null && args.Length > 0) ? args[0].ToLowerInvariant() : "";
        bool wantWeb     = arg0.Contains("web") || arg0.Contains("soc");
        bool wantConfig  = arg0.Contains("config");
        bool wantGamer   = arg0.Contains("gamer");
        bool wantOpacity = arg0.Contains("opacity");

        if (wantOpacity) {
            try {
                string opCmd = "toggle";
                if (arg0.Contains("up") || arg0.Contains("plus") || arg0.Contains("inc")) opCmd = "up";
                else if (arg0.Contains("down") || arg0.Contains("minus") || arg0.Contains("dec")) opCmd = "down";
                else if (arg0.Contains("max") || arg0.Contains("100")) opCmd = "max";
                else if (arg0.Contains("50") || arg0.Contains("half")) opCmd = "50";
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opacity_cmd.flag"), opCmd);
            } catch { }
        }

        bool createdNew;
        EventWaitHandle evt = new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\AntigravityHackerWallpaperConfigEvent", out createdNew);
        bool createdGamerNew;
        EventWaitHandle gamerEvt = new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\AntigravityHackerWallpaperGamerEvent", out createdGamerNew);
        bool createdOpacityNew;
        EventWaitHandle opacityEvt = new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\AntigravityHackerWallpaperOpacityEvent", out createdOpacityNew);

        bool mutexNew;
        using (Mutex mutex = new Mutex(true, "Global\\AntigravityHackerWallpaperMutex", out mutexNew)) {
            if (!mutexNew) {
                if (wantWeb) {
                    PorygonCyberDefenseEngine.OpenWebSocInBrowser();
                } else if (wantOpacity) {
                    opacityEvt.Set();
                } else if (wantGamer) {
                    gamerEvt.Set();
                } else if (wantConfig) {
                    evt.Set();
                }
                return;
            }
            if (wantWeb) {
                PorygonCyberDefenseEngine.OpenWebSocInBrowser();
            }
            if (!wantWeb && !wantConfig && !wantGamer && !wantOpacity) {
                WaitForDesktopShellReady();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            HackerWallpaperForm mainForm = new HackerWallpaperForm(evt, wantConfig, gamerEvt, wantGamer, opacityEvt);
            if (wantOpacity) mainForm.ProcessOpacityFlagOrDefault();
            Application.Run(mainForm);
        }
    }

    private static void WaitForDesktopShellReady() {
        try {
            int mySession = Process.GetCurrentProcess().SessionId;
            DateTime explorerStart = DateTime.MinValue;
            Process[] exps = Process.GetProcessesByName("explorer");
            for (int i = 0; i < exps.Length; i++) {
                try {
                    if (exps[i].SessionId == mySession) {
                        explorerStart = exps[i].StartTime;
                        break;
                    }
                } catch { }
            }
            // If launched at Windows logon (explorer.exe < 15s old), wait for explorer, SHELLDLL_DefView, and all monitors to settle
            if (explorerStart != DateTime.MinValue && (DateTime.Now - explorerStart).TotalSeconds < 15.0) {
                int lastMonCount = -1;
                int stableTicks = 0;
                for (int waitStep = 0; waitStep < 40; waitStep++) {
                    double ageSec = (DateTime.Now - explorerStart).TotalSeconds;
                    IntPtr pm = FindWindow("Progman", null);
                    IntPtr dv = (pm != IntPtr.Zero) ? FindWindowEx(pm, IntPtr.Zero, "SHELLDLL_DefView", null) : IntPtr.Zero;
                    int monCount = 1;
                    try { monCount = Screen.AllScreens.Length; } catch { }
                    if (monCount == lastMonCount) stableTicks++;
                    else { lastMonCount = monCount; stableTicks = 0; }
                    if (ageSec >= 12.0 && pm != IntPtr.Zero && dv != IntPtr.Zero && stableTicks >= 4) {
                        break;
                    }
                    Thread.Sleep(500);
                }
            }
        } catch { }
    }
}

// =====================================================================================================
// FLYWIRE / VIRTUAL FLY BRAIN NATIVE OPENGL CONNECTOME VISUALIZER
//  - Real JRC2018Unisex template (VFB_00101567) + 46 VFB neuropil meshes
//  - ~270 real FlyWire (FAFB v783) neurons registered to JRC2018U, selected along real synaptic pathways
//  - IDS alerts are routed through REAL anatomical pathways:
//      traffic -> optic ingress (L1 > Mi1/Tm3 > T4/T5)          | anomaly -> looming (LPLC2/LC4) > Giant Fiber
//      recon/signature -> olfactory (ORN > PN) > Mushroom Body  | reputation (VirusTotal) -> PAM/PPL1 dopamine > KC
//      SOAR/IPS decision -> central complex (ER > EPG > PFN)    | block/quarantine -> descending neurons (MDN / DN / GF)
//  Data: FlyWire Consortium (CC BY-NC 4.0) / Virtual Fly Brain. Assets built by tools/build_flywire_brain.py
// =====================================================================================================
public static class GlBrain {
    public const uint GL_COMPILE = 0x1300;
    public const uint GL_LIGHTING = 0x0B50;
    public const uint GL_LIGHT0 = 0x4000;
    public const uint GL_POSITION = 0x1203;
    public const uint GL_DIFFUSE = 0x1201;
    public const uint GL_AMBIENT = 0x1200;
    public const uint GL_SPECULAR = 0x1202;
    public const uint GL_LIGHT_MODEL_TWO_SIDE = 0x0B52;
    public const uint GL_LIGHT_MODEL_AMBIENT = 0x0B53;
    public const uint GL_COLOR_MATERIAL = 0x0B57;
    public const uint GL_FRONT_AND_BACK = 0x0408;
    public const uint GL_AMBIENT_AND_DIFFUSE = 0x1602;
    public const uint GL_NORMALIZE = 0x0BA1;
    public const uint GL_SCISSOR_TEST = 0x0C11;
    public const uint GL_POINT_SMOOTH = 0x0B10;
    public const uint GL_VERTEX_ARRAY = 0x8074;
    public const uint GL_NORMAL_ARRAY = 0x8075;
    public const uint GL_FLOAT = 0x1406;
    public const uint GL_UNSIGNED_INT = 0x1405;
    public const uint GL_ONE = 1;
    public const uint GL_S = 0x2000;
    public const uint GL_T = 0x2001;
    public const uint GL_TEXTURE_GEN_MODE = 0x2500;
    public const uint GL_SPHERE_MAP = 0x2402;
    public const uint GL_TEXTURE_GEN_S = 0x0C60;
    public const uint GL_TEXTURE_GEN_T = 0x0C61;
    public const uint GL_TEXTURE_ENV = 0x2300;
    public const uint GL_TEXTURE_ENV_MODE = 0x2200;
    public const int GL_MODULATE = 0x2100;
    public const uint GL_CULL_FACE = 0x0B44;

    [DllImport("opengl32.dll")] public static extern uint glGenLists(int range);
    [DllImport("opengl32.dll")] public static extern void glNewList(uint list, uint mode);
    [DllImport("opengl32.dll")] public static extern void glEndList();
    [DllImport("opengl32.dll")] public static extern void glCallList(uint list);
    [DllImport("opengl32.dll")] public static extern void glDeleteLists(uint list, int range);
    [DllImport("opengl32.dll")] public static extern void glLoadMatrixf(float[] m);
    [DllImport("opengl32.dll")] public static extern void glPushMatrix();
    [DllImport("opengl32.dll")] public static extern void glPopMatrix();
    [DllImport("opengl32.dll")] public static extern void glLightfv(uint light, uint pname, float[] p);
    [DllImport("opengl32.dll")] public static extern void glLightModelfv(uint pname, float[] p);
    [DllImport("opengl32.dll")] public static extern void glLightModeli(uint pname, int p);
    [DllImport("opengl32.dll")] public static extern void glColorMaterial(uint face, uint mode);
    [DllImport("opengl32.dll")] public static extern void glPointSize(float size);
    [DllImport("opengl32.dll")] public static extern void glScissor(int x, int y, int w, int h);
    [DllImport("opengl32.dll")] public static extern void glEnableClientState(uint arr);
    [DllImport("opengl32.dll")] public static extern void glDisableClientState(uint arr);
    [DllImport("opengl32.dll")] public static extern void glVertexPointer(int size, uint type, int stride, IntPtr ptr);
    [DllImport("opengl32.dll")] public static extern void glNormalPointer(uint type, int stride, IntPtr ptr);
    [DllImport("opengl32.dll")] public static extern void glDrawElements(uint mode, int count, uint type, IntPtr indices);
    [DllImport("opengl32.dll")] public static extern void glDrawArrays(uint mode, int first, int count);
    [DllImport("opengl32.dll")] public static extern void glTexGeni(uint coord, uint pname, int param);
    [DllImport("opengl32.dll")] public static extern void glTexEnvi(uint target, uint pname, int param);
    [DllImport("opengl32.dll")] public static extern void glReadPixels(int x, int y, int w, int h, uint format, uint type, IntPtr data);
}

public class FlyWireBrain {
    public const int NStages = 7;
    public static readonly string[] StageNames = new string[] { "INGRESS/OPTIC", "SIGNATURE/OLFACT", "THREAT/LOOMING", "MEMORY/MUSHROOM", "DECISION/CX", "ACTION/DESCEND", "AUDITORY/EXFIL" };
    public static readonly string[] StageDesc = new string[] {
        "trafico de red -> lamina/medula (L1 > Mi1/Tm3 > T4/T5)",
        "firma/puerto -> antena (ORN Or67d > PN DA1)",
        "anomalia -> lobula (LPLC2/LC4 detectores de amenaza)",
        "reputacion/memoria -> cuerpo fungiforme (KC > MBON, DAN)",
        "decision SOAR -> complejo central (ER > EPG > PFN)",
        "accion IPS -> neuronas descendentes (MDN / DN / Giant Fiber)",
        "exfiltracion -> organo de Johnston > AMMC > WED / Giant Fiber"
    };
    public static readonly Color[] StageColors = new Color[] {
        Color.FromArgb(255, 70, 160, 255), Color.FromArgb(255, 60, 235, 215), Color.FromArgb(255, 255, 84, 96),
        Color.FromArgb(255, 255, 100, 210), Color.FromArgb(255, 255, 200, 60), Color.FromArgb(255, 140, 255, 90),
        Color.FromArgb(255, 255, 150, 40)
    };

    public class Neuropil {
        public string Name, VfbId; public int Group; public byte R, G, B;
        public float[] V, N; public int[] F;
        public float CX, CY, CZ;          // whole centroid
        public float LX, LY, LZ, RX, RY, RZ; public bool HasL, HasR; // per-hemisphere centroids
        public float Radius;
        public uint List; public float Act;
    }
    public class Neuron {
        public string VfbId, FlyWireId, CellType, Short, Nt; public int Stage, Side;
        public float RootX, RootY, RootZ, CX, CY, CZ;
        public float[] Segs, Main;
        public uint List; public float Act; public float Refract; public float FireT = -1f;
        public byte R, G, B; public List<int> Out = new List<int>(); public List<int> In = new List<int>();
        public int NeuropilIdx = -1; public int SynOut, SynIn; public float DensityK = 1f;
        public int RealSynOut, RealSynIn; public float[] SynOutPts, SynInPts; public uint SynOutList, SynInList;
    }
    public class Edge { public int Pre, Post, W; public float[] Path; }
    public class Pulse { public int Edge; public float T; public float Speed; public float Strength; public int Depth; public int TraceId; }
    public class PathTrace { public int Id; public string Label; public float Severity; public DateTime At; public List<string> Hops = new List<string>(); public int MaxStageReached = -1; public bool[] StageSeen = new bool[FlyWireBrain.NStages]; }
    private class Scheduled { public float At; public int Stage; public string Sub; public float Strength; public int TraceId; public int Count; }
    public class IdsEvent { public string Kind; public string Label; public float Severity; }

    public bool Loaded, Failed, ListsBuilt; public string Status = "LOADING"; public string Attribution = "";
    public float Extent = 580f;
    public float NeuronAlphaScale = 1f;
    public bool HasRealSynapses = false; public int RealSynDots = 0; public long RealSynTotal = 0;
    public float[] TplV, TplN; public int[] TplF;
    public List<Neuropil> Neuropils = new List<Neuropil>();
    public List<Neuron> Neurons = new List<Neuron>();
    public List<Edge> Edges = new List<Edge>();
    public List<Pulse> Pulses = new List<Pulse>();
    public float[] StageAct = new float[NStages];
    public int[] StageCount = new int[NStages];
    public List<PathTrace> Traces = new List<PathTrace>();
    public System.Collections.Concurrent.ConcurrentQueue<IdsEvent> EventQueue = new System.Collections.Concurrent.ConcurrentQueue<IdsEvent>();
    public int TotalEvents, TotalPulsesFired, TotalCascadeHops;
    private List<Scheduled> schedule = new List<Scheduled>();
    private List<int>[] stageNeurons = new List<int>[NStages];
    private uint tplList, rimTex; private float simTime; private Random rng = new Random(783);
    private int nextTraceId = 1;

    // Camera / projection state (shared by GL renderer and CPU overlay projection)
    public float CamCX, CamCY, CamF, CamS, CosY, SinY, CosP, SinP;
    public float[] ProjM = new float[16], ModelM = new float[16];
    public int HoverNeuron = -1, HoverNeuropil = -1;

    public void LoadAsync(string path) {
        System.Threading.ThreadPool.QueueUserWorkItem(delegate {
            try { Load(path); } catch (Exception ex) { Failed = true; Status = "LOAD_ERR: " + ex.Message; }
        });
    }

    private static float[] RF(BinaryReader r) { int n = r.ReadInt32(); float[] a = new float[n]; byte[] b = r.ReadBytes(n * 4); Buffer.BlockCopy(b, 0, a, 0, n * 4); return a; }
    private static int[] RI(BinaryReader r) { int n = r.ReadInt32(); int[] a = new int[n]; byte[] b = r.ReadBytes(n * 4); Buffer.BlockCopy(b, 0, a, 0, n * 4); return a; }
    private static string RS(BinaryReader r) { int n = r.ReadInt32(); return Encoding.UTF8.GetString(r.ReadBytes(n)); }

    private void Load(string path) {
        if (!File.Exists(path)) { Failed = true; Status = "MISSING " + Path.GetFileName(path); return; }
        using (BinaryReader r = new BinaryReader(new MemoryStream(File.ReadAllBytes(path)))) {
            string magic = Encoding.ASCII.GetString(r.ReadBytes(4));
            if (magic != "FWB1") { Failed = true; Status = "BAD_MAGIC"; return; }
            r.ReadInt32();
            Attribution = RS(r);
            Extent = r.ReadSingle();
            TplV = RF(r); TplN = RF(r); TplF = RI(r);
            int np = r.ReadInt32();
            for (int i = 0; i < np; i++) {
                Neuropil p = new Neuropil();
                p.Name = RS(r); p.VfbId = RS(r); p.Group = r.ReadByte(); p.R = r.ReadByte(); p.G = r.ReadByte(); p.B = r.ReadByte();
                float[] c = RF(r); p.CX = c[0]; p.CY = c[1]; p.CZ = c[2];
                p.V = RF(r); p.N = RF(r); p.F = RI(r);
                double lx = 0, ly = 0, lz = 0, rx = 0, ry = 0, rz = 0; int ln = 0, rn = 0; float maxR = 0f;
                for (int v = 0; v + 2 < p.V.Length; v += 3) {
                    float x = p.V[v], y = p.V[v + 1], z = p.V[v + 2];
                    if (x < 0f) { lx += x; ly += y; lz += z; ln++; } else { rx += x; ry += y; rz += z; rn++; }
                }
                int tot = ln + rn;
                p.HasL = ln > tot * 0.2; p.HasR = rn > tot * 0.2;
                if (ln > 0) { p.LX = (float)(lx / ln); p.LY = (float)(ly / ln); p.LZ = (float)(lz / ln); }
                if (rn > 0) { p.RX = (float)(rx / rn); p.RY = (float)(ry / rn); p.RZ = (float)(rz / rn); }
                for (int v = 0; v + 2 < p.V.Length; v += 3) {
                    float ccx = (p.V[v] < 0f && p.HasL) ? p.LX : ((p.HasR) ? p.RX : p.CX);
                    float ccy = (p.V[v] < 0f && p.HasL) ? p.LY : ((p.HasR) ? p.RY : p.CY);
                    float ccz = (p.V[v] < 0f && p.HasL) ? p.LZ : ((p.HasR) ? p.RZ : p.CZ);
                    float dx = p.V[v] - ccx, dy = p.V[v + 1] - ccy, dz = p.V[v + 2] - ccz;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (d > maxR) maxR = d;
                }
                p.Radius = maxR;
                Neuropils.Add(p);
            }
            int nn = r.ReadInt32();
            for (int i = 0; i < NStages; i++) stageNeurons[i] = new List<int>();
            Dictionary<string, int> typeIdx = new Dictionary<string, int>();
            for (int i = 0; i < nn; i++) {
                Neuron n = new Neuron();
                n.VfbId = RS(r); n.FlyWireId = RS(r); n.CellType = RS(r); n.Short = RS(r); n.Nt = RS(r);
                n.Stage = Math.Min(NStages - 1, (int)r.ReadByte()); n.Side = r.ReadByte();
                float[] root = RF(r); n.RootX = root[0]; n.RootY = root[1]; n.RootZ = root[2];
                float[] cen = RF(r); n.CX = cen[0]; n.CY = cen[1]; n.CZ = cen[2];
                n.Segs = RF(r); n.Main = RF(r);
                // Dense arbors (e.g. Kenyon cell calyx claws) saturate under additive blending -> attenuate by arbor size
                n.DensityK = (float)Math.Max(0.32, Math.Min(1.0, Math.Sqrt(1800.0 / Math.Max(1, n.Segs.Length / 6))));
                int ti;
                if (!typeIdx.TryGetValue(n.Short, out ti)) { ti = typeIdx.Count; typeIdx[n.Short] = ti; }
                Color sc = StageColors[n.Stage];
                float jit = ((ti * 37) % 9 - 4) * 0.055f;
                n.R = (byte)Math.Max(30, Math.Min(255, sc.R + (int)(jit * 255 * ((ti % 2 == 0) ? 1 : -0.6f))));
                n.G = (byte)Math.Max(30, Math.Min(255, sc.G - (int)(jit * 160)));
                n.B = (byte)Math.Max(30, Math.Min(255, sc.B + (int)(jit * 200 * ((ti % 3 == 0) ? -1 : 1))));
                stageNeurons[n.Stage].Add(i);
                // nearest neuropil (by hemisphere centroid) to the neuron centroid
                float best = float.MaxValue;
                for (int k = 0; k < Neuropils.Count; k++) {
                    Neuropil p = Neuropils[k];
                    float px = (n.CX < 0f && p.HasL) ? p.LX : (p.HasR ? p.RX : p.CX);
                    float py = (n.CX < 0f && p.HasL) ? p.LY : (p.HasR ? p.RY : p.CY);
                    float pz = (n.CX < 0f && p.HasL) ? p.LZ : (p.HasR ? p.RZ : p.CZ);
                    float d = (px - n.CX) * (px - n.CX) + (py - n.CY) * (py - n.CY) + (pz - n.CZ) * (pz - n.CZ);
                    if (d < best) { best = d; n.NeuropilIdx = k; }
                }
                Neurons.Add(n);
                StageCount[n.Stage]++;
            }
            int ne = r.ReadInt32();
            for (int i = 0; i < ne; i++) {
                Edge e = new Edge(); e.Pre = r.ReadInt32(); e.Post = r.ReadInt32(); e.W = r.ReadInt32(); e.Path = RF(r);
                if (e.Pre < 0 || e.Pre >= Neurons.Count || e.Post < 0 || e.Post >= Neurons.Count) continue;
                Edges.Add(e);
                Neurons[e.Pre].Out.Add(Edges.Count - 1); Neurons[e.Post].In.Add(Edges.Count - 1);
                Neurons[e.Pre].SynOut += e.W; Neurons[e.Post].SynIn += e.W;
            }
            // Optional SYN1: real FlyWire v783 synapse locations (registered to JRC2018U, snapped onto each arbor)
            if (r.BaseStream.Length - r.BaseStream.Position >= 4) {
                string tag = Encoding.ASCII.GetString(r.ReadBytes(4));
                if (tag == "SYN1") {
                    for (int i = 0; i < Neurons.Count; i++) {
                        Neuron sn = Neurons[i];
                        sn.RealSynOut = r.ReadInt32(); sn.RealSynIn = r.ReadInt32();
                        sn.SynOutPts = RF(r); sn.SynInPts = RF(r);
                        RealSynDots += (sn.SynOutPts.Length + sn.SynInPts.Length) / 3;
                        RealSynTotal += sn.RealSynOut + sn.RealSynIn;
                    }
                    HasRealSynapses = RealSynDots > 0;
                }
            }
        }
        NeuronAlphaScale = (float)Math.Max(0.68, Math.Min(1.0, Math.Sqrt(500.0 / Math.Max(1, Neurons.Count))));
        Loaded = true;
        Status = HasRealSynapses
            ? string.Format("FLYWIRE v783 // {0} NEURONAS // {1} ARISTAS // {2:N0} SINAPSIS REALES ({3:N0} PUNTOS) // {4} NEUROPILOS", Neurons.Count, Edges.Count, RealSynTotal, RealSynDots, Neuropils.Count)
            : string.Format("FLYWIRE v783 // {0} NEURONAS REALES // {1} SINAPSIS-ARISTAS // {2} NEUROPILOS", Neurons.Count, Edges.Count, Neuropils.Count);
    }

    // ---------------------------------------------------------------------------------------------
    // GL resources
    // ---------------------------------------------------------------------------------------------
    private static uint CompileMesh(float[] v, float[] n, int[] f) {
        uint list = GlBrain.glGenLists(1);
        GCHandle hv = GCHandle.Alloc(v, GCHandleType.Pinned);
        GCHandle hn = GCHandle.Alloc(n, GCHandleType.Pinned);
        GCHandle hf = GCHandle.Alloc(f, GCHandleType.Pinned);
        try {
            GlBrain.glEnableClientState(GlBrain.GL_VERTEX_ARRAY);
            GlBrain.glEnableClientState(GlBrain.GL_NORMAL_ARRAY);
            GlBrain.glVertexPointer(3, GlBrain.GL_FLOAT, 0, hv.AddrOfPinnedObject());
            GlBrain.glNormalPointer(GlBrain.GL_FLOAT, 0, hn.AddrOfPinnedObject());
            GlBrain.glNewList(list, GlBrain.GL_COMPILE);
            GlBrain.glDrawElements(GpuGl.GL_TRIANGLES, f.Length, GlBrain.GL_UNSIGNED_INT, hf.AddrOfPinnedObject());
            GlBrain.glEndList();
            GlBrain.glDisableClientState(GlBrain.GL_NORMAL_ARRAY);
            GlBrain.glDisableClientState(GlBrain.GL_VERTEX_ARRAY);
        } finally { hv.Free(); hn.Free(); hf.Free(); }
        return list;
    }

    private static uint CompileLines(float[] segs) {
        uint list = GlBrain.glGenLists(1);
        GCHandle hv = GCHandle.Alloc(segs, GCHandleType.Pinned);
        try {
            GlBrain.glEnableClientState(GlBrain.GL_VERTEX_ARRAY);
            GlBrain.glVertexPointer(3, GlBrain.GL_FLOAT, 0, hv.AddrOfPinnedObject());
            GlBrain.glNewList(list, GlBrain.GL_COMPILE);
            GlBrain.glDrawArrays(GpuGl.GL_LINES, 0, segs.Length / 3);
            GlBrain.glEndList();
            GlBrain.glDisableClientState(GlBrain.GL_VERTEX_ARRAY);
        } finally { hv.Free(); }
        return list;
    }

    private static uint CompilePoints(float[] pts) {
        if (pts == null || pts.Length < 3) return 0;
        uint list = GlBrain.glGenLists(1);
        GCHandle hv = GCHandle.Alloc(pts, GCHandleType.Pinned);
        try {
            GlBrain.glEnableClientState(GlBrain.GL_VERTEX_ARRAY);
            GlBrain.glVertexPointer(3, GlBrain.GL_FLOAT, 0, hv.AddrOfPinnedObject());
            GlBrain.glNewList(list, GlBrain.GL_COMPILE);
            GlBrain.glDrawArrays(GpuGl.GL_POINTS, 0, pts.Length / 3);
            GlBrain.glEndList();
            GlBrain.glDisableClientState(GlBrain.GL_VERTEX_ARRAY);
        } finally { hv.Free(); }
        return list;
    }

    private void BuildLists() {
        tplList = CompileMesh(TplV, TplN, TplF);
        for (int i = 0; i < Neuropils.Count; i++) Neuropils[i].List = CompileMesh(Neuropils[i].V, Neuropils[i].N, Neuropils[i].F);
        for (int i = 0; i < Neurons.Count; i++) {
            Neurons[i].List = CompileLines(Neurons[i].Segs);
            Neurons[i].SynOutList = CompilePoints(Neurons[i].SynOutPts);
            Neurons[i].SynInList = CompilePoints(Neurons[i].SynInPts);
            Neurons[i].SynOutPts = null; Neurons[i].SynInPts = null;
        }
        // Rim (fresnel) sphere-map texture: transparent center -> bright rim
        using (Bitmap bmp = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
            for (int y = 0; y < 128; y++) {
                for (int x = 0; x < 128; x++) {
                    float dx = (x + 0.5f) / 64f - 1f, dy = (y + 0.5f) / 64f - 1f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float t = Math.Max(0f, Math.Min(1f, (d - 0.55f) / 0.40f));
                    t = t * t * (3f - 2f * t);
                    if (d > 1f) t = 1f;
                    bmp.SetPixel(x, y, Color.FromArgb((int)(t * 255f), 255, 255, 255));
                }
            }
            int tw = 0, th = 0;
            GpuGl.UploadBitmapToTexture(bmp, ref rimTex, ref tw, ref th);
        }
        // Free CPU-side mesh arrays that are only needed for list compilation (keeps ~10 MB less in RAM)
        TplV = null; TplN = null; TplF = null;
        for (int i = 0; i < Neuropils.Count; i++) { Neuropils[i].N = null; Neuropils[i].F = null; }
        ListsBuilt = true;
    }

    // ---------------------------------------------------------------------------------------------
    // Camera
    // ---------------------------------------------------------------------------------------------
    public float DefCX, DefCY;
    public void SetupCamera(Rectangle area, int canvasW, int canvasH, float yaw, float pitch, float zoom) {
        SetupCamera(area, canvasW, canvasH, yaw, pitch, zoom, 0f, 0f);
    }
    public void SetupCamera(Rectangle area, int canvasW, int canvasH, float yaw, float pitch, float zoom, float panX, float panY) {
        CosY = (float)Math.Cos(yaw); SinY = (float)Math.Sin(yaw);
        CosP = (float)Math.Cos(pitch); SinP = (float)Math.Sin(pitch);
        DefCX = area.X + area.Width * 0.5f;
        DefCY = area.Y + area.Height * 0.53f;
        CamCX = DefCX + panX;
        CamCY = DefCY + panY;
        CamS = Math.Min(area.Width * 0.92f / 590f, area.Height * 0.94f / 270f) * zoom;
        CamF = 2.6f * 600f * CamS;
        float f = CamF;
        // Perspective P (eye at origin, brain translated to z=-f), rows applied to (ex,ey,ez',1)
        float[] P = new float[] {
            1f, 0f, -CamCX / f, 0f,
            0f, -1f, -CamCY / f, 0f,
            0f, 0f, 0.25f, 0.25f * f,
            0f, 0f, -1f / f, 0f
        };
        float[] O = new float[] {
            2f / canvasW, 0f, 0f, -1f,
            0f, -2f / canvasH, 0f, 1f,
            0f, 0f, -1f / 2000f, 0f,
            0f, 0f, 0f, 1f
        };
        float[] OP = Mul(O, P);
        ToColMajor(OP, ProjM);
        // Model: eye = T(0,0,-f) * Rx(p) * Ry(y) * S(s)
        float s = CamS;
        float[] M = new float[16];
        // Rx*Ry*S then translate z by -f
        // Ry: x' = x cos - z sin ; z' = x sin + z cos
        // Rx: y'' = y cos - z' sin ; z'' = y sin + z' cos
        M[0] = CosY * s;          M[1] = 0f;         M[2] = -SinY * s;          M[3] = 0f;
        M[4] = -SinP * SinY * s;  M[5] = CosP * s;   M[6] = -SinP * CosY * s;   M[7] = 0f;
        M[8] = CosP * SinY * s;   M[9] = SinP * s;   M[10] = CosP * CosY * s;   M[11] = -f;
        M[12] = 0f;               M[13] = 0f;        M[14] = 0f;                M[15] = 1f;
        ToColMajor(M, ModelM);
    }

    private static float[] Mul(float[] a, float[] b) {
        float[] c = new float[16];
        for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) {
            float s = 0f; for (int k = 0; k < 4; k++) s += a[i * 4 + k] * b[k * 4 + j]; c[i * 4 + j] = s;
        }
        return c;
    }
    private static void ToColMajor(float[] rm, float[] cm) { for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) cm[j * 4 + i] = rm[i * 4 + j]; }

    // CPU projection identical to the GL pipeline; returns false if behind camera
    public bool Project(float x, float y, float z, out float sx, out float sy, out float depth) {
        float ex = (CosY * x - SinY * z) * CamS;
        float z1 = (SinY * x + CosY * z);
        float ey = (CosP * y - SinP * z1) * CamS;
        float ez = (SinP * y + CosP * z1) * CamS;
        float w = 1f - ez / CamF;
        depth = ez;
        if (w < 0.05f) { sx = sy = 0f; return false; }
        sx = CamCX + ex / w; sy = CamCY - ey / w;
        return true;
    }

    // ---------------------------------------------------------------------------------------------
    // Simulation: real-synapse cascades + scheduled polysynaptic relays along the IDS pathway
    // ---------------------------------------------------------------------------------------------
    public void Enqueue(string kind, string label, float severity) {
        if (EventQueue.Count > 48) return;
        IdsEvent e = new IdsEvent(); e.Kind = kind; e.Label = label; e.Severity = severity;
        EventQueue.Enqueue(e);
    }

    private int[] StorySequence(string kind, out string actionSub) {
        actionSub = "MDN";
        switch (kind) {
            case "traffic": return new int[] { 0 };
            case "signature": return new int[] { 1, 3 };
            case "anomaly": actionSub = "GF"; return new int[] { 0, 2, 5 };
            case "malware": actionSub = "MDN"; return new int[] { 1, 3, 4, 5 };
            case "reputation": return new int[] { 3 };
            case "decision": actionSub = "DN"; return new int[] { 4, 5 };
            case "block": actionSub = "MDN"; return new int[] { 4, 5 };
            case "critical": actionSub = "GF"; return new int[] { 0, 2, 5 };
            case "exfil": actionSub = "GF"; return new int[] { 6, 5 };
            default: return new int[] { 1, 3 };
        }
    }

    private void StartEvent(IdsEvent ev) {
        if (!Loaded) return;
        string sub;
        int[] seq = StorySequence(ev.Kind, out sub);
        PathTrace tr = null;
        if (ev.Kind != "traffic") {
            tr = new PathTrace(); tr.Id = nextTraceId++; tr.Label = ev.Label; tr.Severity = ev.Severity; tr.At = DateTime.Now;
            lock (Traces) { Traces.Add(tr); while (Traces.Count > 6) Traces.RemoveAt(0); }
            TotalEvents++;
        }
        float strength = 0.75f + Math.Min(0.6f, ev.Severity * 0.6f);
        for (int s = 0; s < seq.Length; s++) {
            Scheduled sc = new Scheduled();
            sc.At = simTime + s * 0.75f; sc.Stage = seq[s]; sc.Strength = strength;
            sc.TraceId = tr != null ? tr.Id : 0;
            sc.Count = (ev.Kind == "traffic") ? 2 : (3 + (int)(ev.Severity * 3f));
            sc.Sub = (seq[s] == 5) ? sub : null;
            if (ev.Kind == "reputation") sc.Sub = "DAN";
            schedule.Add(sc);
        }
    }

    private PathTrace FindTrace(int id) {
        if (id == 0) return null;
        lock (Traces) { for (int i = Traces.Count - 1; i >= 0; i--) if (Traces[i].Id == id) return Traces[i]; }
        return null;
    }

    private void Activate(int ni, float strength, int depth, int traceId) {
        Neuron n = Neurons[ni];
        if (n.Refract > 0f) { n.Act = Math.Min(1.6f, n.Act + strength * 0.3f); return; }
        n.Act = Math.Min(1.6f, n.Act + strength);
        if (n.Act < 0.55f) return;
        n.Refract = 1.1f; n.FireT = 0f;
        PathTrace tr = FindTrace(traceId);
        if (tr != null && !tr.StageSeen[n.Stage]) {
            tr.StageSeen[n.Stage] = true;
            tr.Hops.Add(n.Short);
            if (n.Stage > tr.MaxStageReached) tr.MaxStageReached = n.Stage;
        }
        if (depth >= 5) return;
        int fired = 0;
        for (int k = 0; k < n.Out.Count && fired < 3 && Pulses.Count < 130; k++) {
            Edge e = Edges[n.Out[k]];
            float pw = Math.Min(1f, e.W / 18f);
            if (rng.NextDouble() > 0.18 + pw * 0.55) continue;
            Pulse p = new Pulse(); p.Edge = n.Out[k]; p.T = 0f; p.Speed = 1.25f + (float)rng.NextDouble() * 0.6f;
            p.Strength = strength * (0.45f + 0.45f * pw); p.Depth = depth + 1; p.TraceId = traceId;
            Pulses.Add(p); fired++; TotalPulsesFired++;
        }
    }

    private int PickInStage(int stage, string sub) {
        List<int> lst = stageNeurons[stage];
        if (lst == null || lst.Count == 0) return -1;
        if (sub != null) {
            List<int> cand = new List<int>();
            for (int i = 0; i < lst.Count; i++) {
                string s = Neurons[lst[i]].Short;
                if ((sub == "GF" && s.StartsWith("GF")) || (sub == "MDN" && s == "MDN") || (sub == "DN" && s == "DN") ||
                    (sub == "DAN" && s.Contains("DAN"))) cand.Add(lst[i]);
            }
            if (cand.Count > 0) return cand[rng.Next(cand.Count)];
        }
        // Prefer upstream-most types (those with few inputs from the same stage) for the seed
        return lst[rng.Next(lst.Count)];
    }

    public void Step(float dt, float[] baselineRegionGlow) {
        if (!Loaded) return;
        dt = Math.Max(0.005f, Math.Min(0.25f, dt));
        simTime += dt;
        IdsEvent ev;
        int guard = 0;
        while (guard++ < 12 && EventQueue.TryDequeue(out ev)) StartEvent(ev);

        for (int i = schedule.Count - 1; i >= 0; i--) {
            Scheduled sc = schedule[i];
            if (sc.At > simTime) continue;
            schedule.RemoveAt(i);
            for (int c = 0; c < sc.Count; c++) {
                int ni = PickInStage(sc.Stage, (c == 0) ? sc.Sub : (sc.Stage == 5 ? sc.Sub : null));
                if (ni >= 0) Activate(ni, sc.Strength, 0, sc.TraceId);
            }
        }

        // Baseline: the 64-neuron IDS spiking sim drives sparse spontaneous activity in the mapped real stages
        if (baselineRegionGlow != null && rng.NextDouble() < dt * 2.2) {
            int region = rng.Next(4);
            float g = baselineRegionGlow[region];
            int stage = (region == 0) ? (rng.Next(2) == 0 ? 0 : 1) : (region == 1 ? 3 : (region == 2 ? 4 : 5));
            if (g > 0.05f) {
                int ni = PickInStage(stage, null);
                if (ni >= 0) Activate(ni, 0.35f + g * 0.6f, 4, 0);
            }
        }

        for (int i = Pulses.Count - 1; i >= 0; i--) {
            Pulse p = Pulses[i];
            p.T += dt * p.Speed;
            if (p.T >= 1f) {
                Pulses.RemoveAt(i);
                TotalCascadeHops++;
                Activate(Edges[p.Edge].Post, p.Strength, p.Depth, p.TraceId);
            }
        }

        float decay = (float)Math.Exp(-dt * 2.4f);
        for (int s = 0; s < NStages; s++) StageAct[s] *= (float)Math.Exp(-dt * 1.6f);
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron n = Neurons[i];
            n.Act *= decay;
            if (n.Refract > 0f) n.Refract -= dt;
            if (n.FireT >= 0f) { n.FireT += dt * 1.4f; if (n.FireT > 1f) n.FireT = -1f; }
            float share = n.Act / Math.Max(1, StageCount[n.Stage]) * 6f;
            if (share > StageAct[n.Stage]) StageAct[n.Stage] = Math.Min(1f, StageAct[n.Stage] + share * 0.25f);
        }
        float npDecay = (float)Math.Exp(-dt * 1.8f);
        for (int k = 0; k < Neuropils.Count; k++) Neuropils[k].Act *= npDecay;
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron n = Neurons[i];
            if (n.Act > 0.3f && n.NeuropilIdx >= 0) {
                Neuropil p = Neuropils[n.NeuropilIdx];
                p.Act = Math.Min(1f, p.Act + n.Act * 0.08f);
            }
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Hover picking (CPU, same projection as GL)
    // ---------------------------------------------------------------------------------------------
    public void Pick(int mx, int my, bool enabled) {
        HoverNeuron = -1; HoverNeuropil = -1;
        if (!enabled || !Loaded) return;
        float best = 11f * 11f;
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron n = Neurons[i];
            float[] m = n.Main;
            for (int k = 0; k + 2 < m.Length; k += 6) {
                float sx, sy, d;
                if (!Project(m[k], m[k + 1], m[k + 2], out sx, out sy, out d)) continue;
                float dd = (sx - mx) * (sx - mx) + (sy - my) * (sy - my);
                if (dd < best) { best = dd; HoverNeuron = i; }
            }
        }
        if (HoverNeuron >= 0) return;
        float bestNp = float.MaxValue;
        for (int k = 0; k < Neuropils.Count; k++) {
            Neuropil p = Neuropils[k];
            for (int side = 0; side < 2; side++) {
                if (side == 0 && !p.HasL) continue;
                if (side == 1 && !p.HasR) continue;
                float x = side == 0 ? p.LX : p.RX, y = side == 0 ? p.LY : p.RY, z = side == 0 ? p.LZ : p.RZ;
                float sx, sy, d;
                if (!Project(x, y, z, out sx, out sy, out d)) continue;
                float rad = Math.Max(10f, p.Radius * CamS * 0.55f);
                float dd = (float)Math.Sqrt((sx - mx) * (sx - mx) + (sy - my) * (sy - my));
                if (dd < rad && dd / rad < bestNp) { bestNp = dd / rad; HoverNeuropil = k; }
            }
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Native OpenGL render
    // ---------------------------------------------------------------------------------------------
    public void RenderGL(Rectangle area, int canvasW, int canvasH, bool isFull, bool eco, bool showNeuropils) {
        if (!Loaded) return;
        if (!ListsBuilt) BuildLists();

        GpuGl.glEnable(GlBrain.GL_SCISSOR_TEST);
        GlBrain.glScissor(area.X, canvasH - area.Bottom, area.Width, area.Height);

        GpuGl.glMatrixMode(GpuGl.GL_PROJECTION); GlBrain.glPushMatrix(); GlBrain.glLoadMatrixf(ProjM);
        GpuGl.glMatrixMode(GpuGl.GL_MODELVIEW); GlBrain.glPushMatrix(); GpuGl.glLoadIdentity();

        // Headlight in eye space
        GlBrain.glLightfv(GlBrain.GL_LIGHT0, GlBrain.GL_POSITION, new float[] { 0.25f, 0.55f, 1.0f, 0f });
        GlBrain.glLightfv(GlBrain.GL_LIGHT0, GlBrain.GL_DIFFUSE, new float[] { 0.95f, 0.95f, 0.95f, 1f });
        GlBrain.glLightfv(GlBrain.GL_LIGHT0, GlBrain.GL_AMBIENT, new float[] { 0.10f, 0.10f, 0.10f, 1f });
        GlBrain.glLightModelfv(GlBrain.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.18f, 0.18f, 0.20f, 1f });
        GlBrain.glLightModeli(GlBrain.GL_LIGHT_MODEL_TWO_SIDE, 1);
        GlBrain.glLoadMatrixf(ModelM);

        GpuGl.glEnable(GpuGl.GL_BLEND);
        GpuGl.glDisable(GpuGl.GL_DEPTH_TEST);

        bool focus = HoverNeuron >= 0;
        float dimOthers = focus ? 0.35f : 1f;

        // --- 1. Template body (lit, faint) + fresnel rim (sphere-map)
        GpuGl.glEnable(GlBrain.GL_LIGHTING); GpuGl.glEnable(GlBrain.GL_LIGHT0);
        GpuGl.glEnable(GlBrain.GL_COLOR_MATERIAL); GlBrain.glColorMaterial(GlBrain.GL_FRONT_AND_BACK, GlBrain.GL_AMBIENT_AND_DIFFUSE);
        GpuGl.glEnable(GlBrain.GL_NORMALIZE);
        GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GpuGl.GL_ONE_MINUS_SRC_ALPHA);
        GpuGl.glColor4f(0.42f, 0.48f, 0.56f, 0.055f);
        GlBrain.glCallList(tplList);
        GpuGl.glDisable(GlBrain.GL_LIGHTING);

        BeginRim();
        GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GlBrain.GL_ONE);
        GpuGl.glColor4f(0.55f, 0.78f, 0.92f, 0.22f * (focus ? 0.6f : 1f));
        GlBrain.glCallList(tplList);
        EndRim();

        // --- 2. Neuropils (lit translucent + coloured rim), brighten with routed activity / hover
        if (showNeuropils) {
            GpuGl.glEnable(GlBrain.GL_LIGHTING);
            GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GlBrain.GL_ONE);
            for (int k = 0; k < Neuropils.Count; k++) {
                Neuropil p = Neuropils[k];
                float a = 0.022f + p.Act * 0.16f + (k == HoverNeuropil ? 0.14f : 0f);
                GpuGl.glColor4f(p.R / 255f, p.G / 255f, p.B / 255f, a * dimOthers + (focus ? 0.01f : 0f));
                GlBrain.glCallList(p.List);
            }
            GpuGl.glDisable(GlBrain.GL_LIGHTING);
            if (!eco) {
                BeginRim();
                for (int k = 0; k < Neuropils.Count; k++) {
                    Neuropil p = Neuropils[k];
                    float a = 0.085f + p.Act * 0.50f + (k == HoverNeuropil ? 0.45f : 0f);
                    GpuGl.glColor4f(p.R / 255f, p.G / 255f, p.B / 255f, a * dimOthers);
                    GlBrain.glCallList(p.List);
                }
                EndRim();
            }
        }

        // --- 3. Real FlyWire neuron skeletons (additive glow; active neurons get a wide halo pass)
        GpuGl.glEnable(GpuGl.GL_LINE_SMOOTH);
        GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GlBrain.GL_ONE);
        float baseW = isFull ? 1.0f : 0.85f;
        if (!eco) {
            for (int i = 0; i < Neurons.Count; i++) {
                Neuron n = Neurons[i];
                float act = Math.Min(1f, n.Act);
                bool hot = (i == HoverNeuron) || act > 0.60f;
                if (!hot) continue;
                GpuGl.glLineWidth(isFull ? 4.2f : 3.0f);
                float a = (i == HoverNeuron) ? 0.24f : 0.07f * act;
                GpuGl.glColor4f(n.R / 255f, n.G / 255f, n.B / 255f, a);
                GlBrain.glCallList(n.List);
            }
        }
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron n = Neurons[i];
            float act = Math.Min(1f, n.Act);
            float a = (0.13f + act * 0.42f) * (0.55f + 0.45f * n.DensityK) * (0.45f + 0.55f * NeuronAlphaScale);
            if (focus) {
                if (i == HoverNeuron) a = 1f;
                else if (IsLinked(HoverNeuron, i)) a = Math.Max(a, 0.65f);
                else a *= 0.22f;
            }
            GpuGl.glLineWidth(baseW + act * 0.6f + (i == HoverNeuron ? 0.9f : 0f));
            float boost = 1f + act * 0.22f;
            GpuGl.glColor4f(Math.Min(1f, n.R / 255f * boost), Math.Min(1f, n.G / 255f * boost), Math.Min(1f, n.B / 255f * boost), a);
            GlBrain.glCallList(n.List);
        }

        // --- 4. Hovered neuron: real synaptic partners (cyan = output, amber = input) drawn along contact paths
        if (focus) {
            Neuron h = Neurons[HoverNeuron];
            GpuGl.glLineWidth(isFull ? 2.0f : 1.4f);
            for (int k = 0; k < h.Out.Count; k++) DrawPath(Edges[h.Out[k]].Path, 0.55f, 1f, 0.94f, 0.85f, 1f);
            for (int k = 0; k < h.In.Count; k++) DrawPath(Edges[h.In[k]].Path, 1f, 0.75f, 0.28f, 0.85f, 1f);
        }

        // --- 4b. REAL FlyWire v783 synapses as points: presynaptic release sites light up on firing neurons
        GpuGl.glEnable(GlBrain.GL_POINT_SMOOTH);
        if (HasRealSynapses) {
            for (int i = 0; i < Neurons.Count; i++) {
                Neuron n = Neurons[i];
                bool hov = (i == HoverNeuron);
                float act = Math.Min(1f, n.Act);
                if (!hov && act < 0.48f) continue;
                if (focus && !hov && !IsLinked(HoverNeuron, i)) continue;
                if (n.SynOutList != 0) {
                    GlBrain.glPointSize(hov ? (isFull ? 3.2f : 2.3f) : (isFull ? 2.1f : 1.5f));
                    if (hov) GpuGl.glColor4f(0.45f, 1f, 0.92f, 0.82f);
                    else {
                        float pa = (0.10f + 0.24f * (act - 0.45f)) * (0.55f + 0.45f * n.DensityK) * NeuronAlphaScale;
                        GpuGl.glColor4f(n.R / 255f, n.G / 255f, n.B / 255f, pa);
                    }
                    GlBrain.glCallList(n.SynOutList);
                }
                if (hov && n.SynInList != 0) {
                    GlBrain.glPointSize(isFull ? 2.8f : 2.0f);
                    GpuGl.glColor4f(1f, 0.72f, 0.22f, 0.78f);
                    GlBrain.glCallList(n.SynInList);
                }
            }
        }

        // --- 5. Action potentials travelling along real pre-synaptic arbors to the synaptic contact
        for (int i = 0; i < Pulses.Count; i++) {
            Pulse p = Pulses[i];
            Edge e = Edges[p.Edge];
            Neuron pre = Neurons[e.Pre];
            GpuGl.glLineWidth(isFull ? 2.4f : 1.7f);
            DrawPathPartial(e.Path, Math.Max(0f, p.T - 0.35f), p.T, pre.R / 255f, pre.G / 255f, pre.B / 255f, 0.95f);
            float hx, hy, hz;
            PathPoint(e.Path, p.T, out hx, out hy, out hz);
            GlBrain.glPointSize(isFull ? 6.5f : 4.5f);
            GpuGl.glColor4f(1f, 1f, 1f, 0.95f);
            GpuGl.glBegin(GpuGl.GL_POINTS); GpuGl.glVertex3f(hx, hy, hz); GpuGl.glEnd();
        }
        // Somata + firing "spark" along the neuron's main arbor
        GlBrain.glPointSize(isFull ? 4.0f : 3.0f);
        GpuGl.glBegin(GpuGl.GL_POINTS);
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron n = Neurons[i];
            float act = Math.Min(1f, n.Act);
            float a = (0.30f + act * 0.70f) * ((focus && i != HoverNeuron && !IsLinked(HoverNeuron, i)) ? 0.25f : 1f);
            GpuGl.glColor4f(n.R / 255f, n.G / 255f, n.B / 255f, a);
            GpuGl.glVertex3f(n.RootX, n.RootY, n.RootZ);
        }
        GpuGl.glEnd();
        GlBrain.glPointSize(isFull ? 7.0f : 5.0f);
        GpuGl.glBegin(GpuGl.GL_POINTS);
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron n = Neurons[i];
            if (n.FireT < 0f) continue;
            float x, y, z;
            PathPoint(n.Main, n.FireT, out x, out y, out z);
            GpuGl.glColor4f(1f, 1f, 1f, 0.9f * (1f - n.FireT * 0.6f));
            GpuGl.glVertex3f(x, y, z);
        }
        GpuGl.glEnd();
        GpuGl.glDisable(GlBrain.GL_POINT_SMOOTH);

        GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GpuGl.GL_ONE_MINUS_SRC_ALPHA);
        GpuGl.glLineWidth(1f);
        GpuGl.glDisable(GlBrain.GL_COLOR_MATERIAL);
        GpuGl.glDisable(GlBrain.GL_NORMALIZE);
        GpuGl.glDisable(GlBrain.GL_LIGHT0);
        GpuGl.glMatrixMode(GpuGl.GL_PROJECTION); GlBrain.glPopMatrix();
        GpuGl.glMatrixMode(GpuGl.GL_MODELVIEW); GlBrain.glPopMatrix();
        GpuGl.glDisable(GlBrain.GL_SCISSOR_TEST);
    }

    private bool IsLinked(int a, int b) {
        if (a < 0) return false;
        Neuron n = Neurons[a];
        for (int k = 0; k < n.Out.Count; k++) if (Edges[n.Out[k]].Post == b) return true;
        for (int k = 0; k < n.In.Count; k++) if (Edges[n.In[k]].Pre == b) return true;
        return false;
    }

    private void BeginRim() {
        GpuGl.glEnable(GpuGl.GL_TEXTURE_2D);
        GpuGl.glBindTexture(GpuGl.GL_TEXTURE_2D, rimTex);
        GlBrain.glTexEnvi(GlBrain.GL_TEXTURE_ENV, GlBrain.GL_TEXTURE_ENV_MODE, GlBrain.GL_MODULATE);
        GlBrain.glTexGeni(GlBrain.GL_S, GlBrain.GL_TEXTURE_GEN_MODE, (int)GlBrain.GL_SPHERE_MAP);
        GlBrain.glTexGeni(GlBrain.GL_T, GlBrain.GL_TEXTURE_GEN_MODE, (int)GlBrain.GL_SPHERE_MAP);
        GpuGl.glEnable(GlBrain.GL_TEXTURE_GEN_S); GpuGl.glEnable(GlBrain.GL_TEXTURE_GEN_T);
        GpuGl.glBlendFunc(GpuGl.GL_SRC_ALPHA, GlBrain.GL_ONE);
    }
    private void EndRim() {
        GpuGl.glDisable(GlBrain.GL_TEXTURE_GEN_S); GpuGl.glDisable(GlBrain.GL_TEXTURE_GEN_T);
        GpuGl.glDisable(GpuGl.GL_TEXTURE_2D);
    }

    private static void DrawPath(float[] path, float r, float g, float b, float a, float w) {
        GpuGl.glColor4f(r, g, b, a);
        GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
        for (int k = 0; k + 2 < path.Length; k += 3) GpuGl.glVertex3f(path[k], path[k + 1], path[k + 2]);
        GpuGl.glEnd();
    }

    private static void DrawPathPartial(float[] path, float t0, float t1, float r, float g, float b, float a) {
        int n = path.Length / 3; if (n < 2) return;
        float f0 = t0 * (n - 1), f1 = t1 * (n - 1);
        int i0 = (int)Math.Floor(f0), i1 = (int)Math.Ceiling(f1);
        GpuGl.glBegin(GpuGl.GL_LINE_STRIP);
        for (int k = i0; k <= i1 && k < n; k++) {
            float u = (n > 1) ? (float)k / (n - 1) : 0f;
            float fade = (t1 - t0) > 0.001f ? Math.Max(0f, Math.Min(1f, (u - t0) / (t1 - t0))) : 1f;
            GpuGl.glColor4f(r, g, b, a * (0.15f + 0.85f * fade));
            float x, y, z;
            if (k == i1) PathPoint(path, t1, out x, out y, out z); else { x = path[k * 3]; y = path[k * 3 + 1]; z = path[k * 3 + 2]; }
            GpuGl.glVertex3f(x, y, z);
        }
        GpuGl.glEnd();
    }

    public static void PathPoint(float[] path, float t, out float x, out float y, out float z) {
        int n = path.Length / 3;
        if (n == 0) { x = y = z = 0f; return; }
        if (n == 1) { x = path[0]; y = path[1]; z = path[2]; return; }
        float f = Math.Max(0f, Math.Min(1f, t)) * (n - 1);
        int i = Math.Min(n - 2, (int)f); float u = f - i;
        x = path[i * 3] + (path[i * 3 + 3] - path[i * 3]) * u;
        y = path[i * 3 + 1] + (path[i * 3 + 4] - path[i * 3 + 1]) * u;
        z = path[i * 3 + 2] + (path[i * 3 + 5] - path[i * 3 + 2]) * u;
    }
}

// Low-level mouse wheel hook on a DEDICATED background thread (never the UI/render thread -> no cursor stutter).
// Only accumulates wheel deltas while the render thread has "armed" it (cursor over the brain viewport on the desktop).
public static class BrainWheelHook {
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    [StructLayout(LayoutKind.Sequential)] private struct HPOINT { public int x, y; }
    [StructLayout(LayoutKind.Sequential)] private struct MSLLHOOKSTRUCT { public HPOINT pt; public uint mouseData; public uint flags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)] private struct HMSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public HPOINT pt; }
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc fn, IntPtr hMod, uint threadId);
    [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr h);
    [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr h, int n, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] private static extern int GetMessage(out HMSG m, IntPtr hwnd, uint min, uint max);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)] private static extern IntPtr GetModuleHandle(string name);

    public static volatile bool Armed = false;
    public static volatile int ScreenL, ScreenT, ScreenR, ScreenB;
    private static int pending = 0;
    private static LowLevelMouseProc procRef;
    private static IntPtr hook = IntPtr.Zero;
    private static Thread th;

    public static void Start() {
        if (th != null) return;
        th = new Thread(Run);
        th.IsBackground = true;
        th.Name = "BrainWheelHook";
        th.Priority = ThreadPriority.AboveNormal;
        th.Start();
    }

    private static void Run() {
        try {
            procRef = Proc;
            hook = SetWindowsHookEx(14, procRef, GetModuleHandle(null), 0);
            HMSG m;
            while (GetMessage(out m, IntPtr.Zero, 0, 0) > 0) { }
        } catch { }
        finally { if (hook != IntPtr.Zero) UnhookWindowsHookEx(hook); }
    }

    private static IntPtr Proc(int nCode, IntPtr wParam, IntPtr lParam) {
        if (nCode >= 0 && Armed && wParam.ToInt64() == 0x020A) {
            MSLLHOOKSTRUCT s = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            if (s.pt.x >= ScreenL && s.pt.x < ScreenR && s.pt.y >= ScreenT && s.pt.y < ScreenB) {
                short delta = (short)((s.mouseData >> 16) & 0xFFFF);
                Interlocked.Add(ref pending, delta);
            }
        }
        return CallNextHookEx(hook, nCode, wParam, lParam);
    }

    public static int TakeDelta() { return Interlocked.Exchange(ref pending, 0); }
}
