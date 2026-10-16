using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public class WallpaperConfig {
    public int ThemeIndex = 0;       // 0=Cyberpunk Neon, 1=Matrix Green, 2=Red Team, 3=Ice Stealth
    public int RainSpeed = 100;      // 0..200
    public int RainDensity = 72;     // 20..110
    public int PorygonScale = 88;    // 50..160%
    public bool ShowTelemetry = true;
    public bool ShowRadar = true;
    public bool ShowPorygon = true;
    public bool FollowMouse = false; // Strictly FALSE by default!
    public bool TrainingMode = false;// Intensive AI training waves
    public bool AutoPauseMaximized = true;

    private static string ConfigPath {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            return Path.Combine(dir, "config.json");
        }
    }

    public void Save() {
        try {
            string json = string.Format(
                "{{\r\n  \"ThemeIndex\": {0},\r\n  \"RainSpeed\": {1},\r\n  \"RainDensity\": {2},\r\n  \"PorygonScale\": {3},\r\n  \"ShowTelemetry\": {4},\r\n  \"ShowRadar\": {5},\r\n  \"ShowPorygon\": {6},\r\n  \"FollowMouse\": {7},\r\n  \"TrainingMode\": {8},\r\n  \"AutoPauseMaximized\": {9}\r\n}}",
                ThemeIndex, RainSpeed, RainDensity, PorygonScale,
                ShowTelemetry.ToString().ToLower(),
                ShowRadar.ToString().ToLower(),
                ShowPorygon.ToString().ToLower(),
                FollowMouse.ToString().ToLower(),
                TrainingMode.ToString().ToLower(),
                AutoPauseMaximized.ToString().ToLower()
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
                cfg.RainSpeed = ParseInt(txt, "RainSpeed", 100);
                cfg.RainDensity = ParseInt(txt, "RainDensity", 72);
                cfg.PorygonScale = ParseInt(txt, "PorygonScale", 88);
                cfg.ShowTelemetry = ParseBool(txt, "ShowTelemetry", true);
                cfg.ShowRadar = ParseBool(txt, "ShowRadar", true);
                cfg.ShowPorygon = ParseBool(txt, "ShowPorygon", true);
                cfg.FollowMouse = ParseBool(txt, "FollowMouse", false);
                cfg.TrainingMode = ParseBool(txt, "TrainingMode", false);
                cfg.AutoPauseMaximized = ParseBool(txt, "AutoPauseMaximized", true);
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
// FLYWIRE / FLYBRAIN SPIKING CONNECTOME WITH HEBBIAN SYNAPTIC PLASTICITY & RL
// ============================================================================
public class FlyBrainConnectome {
    public const int N = 64;
    public float[] V = new float[N];             // Membrane potential (0..1.2)
    public float[] SpikeGlow = new float[N];     // Visual spike decay for HUD
    public float[,] W = new float[N, N];         // Synaptic weight matrix
    public float[,] Eligibility = new float[N, N]; // STDP / Hebbian eligibility trace

    // Persistent Training & Evolution Metrics
    public int Level = 1;
    public int XP = 0;
    public int PacketsCollected = 0;
    public int AnomaliesCleared = 0;
    public int NeuralSyncs = 0;
    public float SynapticGain = 1.0f;            // Increases via Hebbian learning (1.0 .. 2.8)
    public float LastPlasticityDelta = 0.0f;

    public float DriveCuriosity = 0.65f;
    public float DriveHunger = 0.55f;
    public float DriveAlert = 0.20f;
    public float DriveSync = 0.40f;
    public string ActiveStateName = "AUTONOMOUS // EXPLORING";

    private Random rng;

    public int NextLevelXP {
        get { return 80 + (Level - 1) * 60; }
    }

    public FlyBrainConnectome(int seed) {
        rng = new Random(seed);
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

        // Functional pathways:
        // Sensory 0..3 (Target Node Left/Right/Near) -> Curiosity & Steering
        W[0, 49] = 0.58f;
        W[1, 50] = 0.58f;
        W[2, 56] = 0.65f;
        W[3, 36] = 0.55f;

        // Sensory 4..7 (Data Packet / Training Orb) -> Hunger & Thrust & Beam
        W[4, 49] = 0.62f;
        W[5, 50] = 0.62f;
        W[6, 48] = 0.72f;
        W[7, 60] = 0.85f;

        // Sensory 8..11 (Firewall Anomaly / Peer Sync) -> Alert & TriAttack / Sync / QuantumBlink
        W[8, 42] = 0.78f;
        W[8, 61] = 0.82f;
        W[9, 62] = 0.80f;
        W[10, 63] = 0.75f;
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
        public Face3D(Color col, float bias, params int[] idx) {
            BaseColor = col;
            DepthBias = bias;
            Indices = idx;
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

    public class DataPacket {
        public float X, Y;
        public float VX, VY;
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
    }

    public class PorygonAgent {
        public int AgentIndex;
        public string Name;
        public string RoleTag;
        public string SpecialtyDesc;
        public ModelData Model;
        public FlyBrainConnectome Brain;
        public Color AuraColor;

        public float PosX, PosY;
        public float VelX, VelY;
        public float HomeX, HomeY;
        public float WaypointX, WaypointY;
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
        public float ScanWaveTimer = 0.0f;     // Skill LVL 6: Deep Sonar Scan Wave
        public int SyncCooldown = 0;
        public float LevelUpBannerTimer = 0.0f;
    }

    public List<PorygonAgent> Agents = new List<PorygonAgent>();
    public List<DataPacket> Packets = new List<DataPacket>();
    public List<string> IdsDetectionLog = new List<string>();
    public DataPacket HoveredPacket = null;
    public PorygonAgent HoveredAgent = null;
    public RealTelemetryFeed LatestFeed;

    private object packetLock = new object();
    private Random rng = new Random(1337);
    private int saveTickCounter = 0;

    private static string BrainSavePath {
        get {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HackerWallpaper");
            return Path.Combine(dir, "porygon_brains.json");
        }
    }

    public Porygon3DAvatar(int screenW, int screenH) {
        LoadThreeFavoriteModels(screenW, screenH);
        LoadBrainStates();

        IdsDetectionLog.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] IDS_INIT // 3 Neural Cores online — learning network baseline");
    }

    private void LoadThreeFavoriteModels(int screenW, int screenH) {
        Agents.Clear();

        // Favorite 1 (was #2): Porygon (3DS / Switch HD - GLB)
        ModelData m1 = TryLoadGlb("pokemon_porygon.glb", "Porygon (Gen-1 Cyber Scout)", "PORYGON_V1");
        // Favorite 2 (was #3): Porygon2 (Johto Cyber HD - GLB)
        ModelData m2 = TryLoadGlb("porygon2__johto.glb", "Porygon2 (Johto Neural Core)", "PORYGON2_V2");
        // Favorite 3 (was #4): Porygon-Z (Sinnoh Glitch HD - GLB)
        ModelData m3 = TryLoadGlb("porygon-z.glb",       "Porygon-Z (Hyper Overclock AI)", "PORYGON_Z_V3");

        if (m1 != null) {
            Agents.Add(CreateAgent(0, "PORYGON", "FLOW_BASELINE_AI",
                "Analiza paquetes TCP estándar y entrena la línea base Z-Score de ancho de banda",
                m1, screenW * 0.34f, screenH * 0.56f, 0.92f, Color.FromArgb(0, 240, 255), 101));
        }
        if (m2 != null) {
            Agents.Add(CreateAgent(1, "PORYGON2", "ENDPOINT_AUDITOR",
                "Inspecciona IPs remotas nuevas, verifica certificados TLS y sincroniza pesos P2P",
                m2, screenW * 0.55f, screenH * 0.64f, 0.96f, Color.FromArgb(236, 72, 153), 202));
        }
        if (m3 != null) {
            Agents.Add(CreateAgent(2, "PORYGON-Z", "ANOMALY_HUNTER_IDS",
                "Detecta y neutraliza anomalías de red (Puertos inusuales, picos Z-Score, Jitter y SYN)",
                m3, screenW * 0.77f, screenH * 0.54f, 1.02f, Color.FromArgb(168, 85, 247), 303));
        }
    }

    private PorygonAgent CreateAgent(int idx, string name, string role, string specDesc, ModelData model,
        float startX, float startY, float scaleFactor, Color auraCol, int seed) {
        FlyBrainConnectome brain = new FlyBrainConnectome(seed);
        brain.Level = idx + 2; // Porygon=LVL 2, Porygon2=LVL 3, Porygon-Z=LVL 4 initially (if no save file yet)
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
            HomeX = startX,
            HomeY = startY,
            WaypointX = startX,
            WaypointY = startY,
            FloatBob = idx * 2.1f,
            WalkPhase = idx * 1.7f,
            ScaleFactor = scaleFactor
        };
    }

    public void SaveBrainStates() {
        try {
            StringBuilder sb = new StringBuilder();
            sb.Append("[\r\n");
            for (int i = 0; i < Agents.Count; i++) {
                FlyBrainConnectome b = Agents[i].Brain;
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "  {{ \"Name\": \"{0}\", \"Level\": {1}, \"XP\": {2}, \"Packets\": {3}, \"Anomalies\": {4}, \"Syncs\": {5}, \"Gain\": {6:0.000} }}{7}\r\n",
                    Agents[i].Name, b.Level, b.XP, b.PacketsCollected, b.AnomaliesCleared, b.NeuralSyncs, b.SynapticGain,
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
            if (Packets.Count >= 18) {
                // If full, replace oldest normal flow if we are injecting an anomaly/new host
                if (kind > 0) {
                    for (int i = 0; i < Packets.Count; i++) {
                        if (Packets[i].Kind == 0 && !Packets[i].IsHovered) {
                            Packets.RemoveAt(i);
                            break;
                        }
                    }
                }
                if (Packets.Count >= 18) return;
            }

            Color col = Color.FromArgb(0, 240, 255);
            if (kind == 1) col = Color.FromArgb(250, 204, 21);       // Golden New-Host / Elevated Flow Orb
            else if (kind == 2) col = Color.FromArgb(255, 42, 85);   // Red Real Traffic Anomaly
            else if (proto.Contains("UDP") || proto.Contains("HTTP")) col = Color.FromArgb(236, 72, 153);

            Packets.Add(new DataPacket {
                X = rng.Next(240, Math.Max(320, screenW - 210)),
                Y = rng.Next(265, Math.Max(340, screenH - 175)),
                VX = (float)((rng.NextDouble() - 0.5) * (kind == 2 ? 1.85 : 1.15)),
                VY = (float)((rng.NextDouble() - 0.5) * (kind == 2 ? 1.85 : 1.15)),
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
                if (isPorygon3ds && meshIdx == 1) subdiv = 6;
                else if (isPorygonZ && meshIdx == 1) { subdiv = 2; depthBias = 1.5f; }
                else if (isPorygon2 && meshIdx == 2) { subdiv = 3; depthBias = 4.5f; }
                else if (isPorygon2 && meshIdx == 0) { subdiv = 1; depthBias = 1.0f; }
            }

            if (subdiv <= 1) {
                int baseV = model.Verts.Count;
                for (int i = 0; i < wPos.Length; i++) model.Verts.Add(wPos[i]);
                for (int i = 0; i + 2 < indices.Length; i += 3) {
                    int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
                    Color c = matColor;
                    if (bmp != null && uvs != null && i0 < uvs.Length && i1 < uvs.Length && i2 < uvs.Length) {
                        float u = (uvs[i0].X + uvs[i1].X + uvs[i2].X) / 3.0f;
                        float v = (uvs[i0].Y + uvs[i1].Y + uvs[i2].Y) / 3.0f;
                        c = SampleGlbTexture(bmp, u, v, matColor);
                    }
                    model.Faces.Add(new Face3D(c, depthBias, baseV + i0, baseV + i1, baseV + i2));
                }
            } else {
                for (int i = 0; i + 2 < indices.Length; i += 3) {
                    int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
                    Vec3 v0 = wPos[i0], v1 = wPos[i1], v2 = wPos[i2];
                    PointF uv0 = uvs[i0], uv1 = uvs[i1], uv2 = uvs[i2];

                    for (int r = 0; r < subdiv; r++) {
                        for (int col = 0; col < subdiv - r; col++) {
                            AddSubTri(model, v0, v1, v2, uv0, uv1, uv2, subdiv,
                                r, col, r + 1, col, r, col + 1, bmp, matColor, depthBias);
                            if (r + col < subdiv - 1) {
                                AddSubTri(model, v0, v1, v2, uv0, uv1, uv2, subdiv,
                                    r + 1, col, r + 1, col + 1, r, col + 1, bmp, matColor, depthBias);
                            }
                        }
                    }
                }
            }
        }
    }

    private void AddSubTri(ModelData model, Vec3 v0, Vec3 v1, Vec3 v2, PointF uv0, PointF uv1, PointF uv2,
        int N, int rA, int cA, int rB, int cB, int rC, int cC, Bitmap bmp, Color factor, float depthBias) {

        float a0 = (float)rA / N, b0 = (float)cA / N, d0 = 1f - a0 - b0;
        float a1 = (float)rB / N, b1 = (float)cB / N, d1 = 1f - a1 - b1;
        float a2 = (float)rC / N, b2 = (float)cC / N, d2 = 1f - a2 - b2;

        Vec3 pA = v0 * d0 + v1 * a0 + v2 * b0;
        Vec3 pB = v0 * d1 + v1 * a1 + v2 * b1;
        Vec3 pC = v0 * d2 + v1 * a2 + v2 * b2;

        float u = (uv0.X * (d0 + d1 + d2) + uv1.X * (a0 + a1 + a2) + uv2.X * (b0 + b1 + b2)) / 3.0f;
        float v = (uv0.Y * (d0 + d1 + d2) + uv1.Y * (a0 + a1 + a2) + uv2.Y * (b0 + b1 + b2)) / 3.0f;

        Color col = SampleGlbTexture(bmp, u, v, factor);
        int idx0 = model.Verts.Count;
        model.Verts.Add(pA);
        model.Verts.Add(pB);
        model.Verts.Add(pC);
        model.Faces.Add(new Face3D(col, depthBias, idx0, idx0 + 1, idx0 + 2));
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
            // 1. Hit-test Mouse Hover on Floating Network Nodes (freezes hovered node for inspection!)
            HoveredPacket = null;
            float bestHoverNodeDist = 42f;
            for (int i = 0; i < Packets.Count; i++) {
                DataPacket p = Packets[i];
                p.IsHovered = false;
                float mdx = mousePt.X - p.X;
                float mdy = mousePt.Y - p.Y;
                float md = (float)Math.Sqrt(mdx * mdx + mdy * mdy);
                if (md < bestHoverNodeDist) {
                    bestHoverNodeDist = md;
                    HoveredPacket = p;
                }
            }
            if (HoveredPacket != null) {
                HoveredPacket.IsHovered = true;
            }

            // 2. Hit-test Mouse Hover on the 3 Porygon Agents (only if not already hovering a node)
            HoveredAgent = null;
            float bestHoverAgentDist = 74f;
            for (int aIdx = 0; aIdx < Agents.Count; aIdx++) {
                PorygonAgent a = Agents[aIdx];
                a.IsHovered = false;
                float mdx = mousePt.X - a.PosX;
                float mdy = mousePt.Y - a.PosY;
                float md = (float)Math.Sqrt(mdx * mdx + mdy * mdy);
                if (HoveredPacket == null && md < bestHoverAgentDist) {
                    bestHoverAgentDist = md;
                    HoveredAgent = a;
                }
            }
            if (HoveredAgent != null) {
                HoveredAgent.IsHovered = true;
            }

            // 3. Move non-hovered network nodes in cyberspace
            for (int i = 0; i < Packets.Count; i++) {
                DataPacket p = Packets[i];
                p.floatRot += (p.Kind == 2) ? 0.12f : 0.06f;
                if (p.IsHovered) continue; // Freeze position while user inspects the node with the mouse!
                p.X += p.VX;
                p.Y += p.VY;
                if (p.X < 210 || p.X > screenW - 140) p.VX = -p.VX;
                if (p.Y < 260 || p.Y > screenH - 140) p.VY = -p.VY;
            }

            // 4. Update each of the 3 Porygons independently with REAL Network Telemetry!
            for (int aIdx = 0; aIdx < Agents.Count; aIdx++) {
                PorygonAgent a = Agents[aIdx];
                FlyBrainConnectome br = a.Brain;
                a.FloatBob += 0.072f + aIdx * 0.006f;
                if (a.SyncCooldown > 0) a.SyncCooldown--;
                if (a.LevelUpBannerTimer > 0f) a.LevelUpBannerTimer -= 0.025f;

                // Specialization per evolution:
                // Porygon (aIdx=0): Baseline Flow Collector (Kind 0 & 1)
                // Porygon2 (aIdx=1): New External Host Auditor & TLS Verifier (Kind 1 & 2)
                // Porygon-Z (aIdx=2): Primary IDS Anomaly Hunter (Kind 2 priority!)
                DataPacket bestPkt = null;
                float bestScore = 999999f;
                float bestDist = 999999f;

                for (int i = 0; i < Packets.Count; i++) {
                    DataPacket p = Packets[i];
                    float dx = p.X - a.PosX;
                    float dy = p.Y - a.PosY;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float priority = d;

                    if (p.Kind == 2) {
                        // Traffic Anomaly! Porygon-Z prioritizes anomalies heavily
                        priority *= (aIdx == 2) ? 0.35f : (br.HasSkill(4) ? 0.58f : 0.85f);
                    } else if (p.Kind == 1) {
                        // New Host / Elevated Flow Orb! Porygon2 prioritizes auditing new hosts
                        priority *= (aIdx == 1) ? 0.42f : 0.68f;
                    } else {
                        // Standard Flow! Porygon (Gen 1) prioritizes baseline collection
                        priority *= (aIdx == 0) ? 0.62f : 0.95f;
                    }

                    // If user is hovering a node, boost priority slightly so the assigned Porygon looks at it,
                    // but don't destroy the node while the user is actively reading its hover card!
                    if (p.IsHovered) {
                        priority *= 0.65f;
                    }

                    for (int o = 0; o < Agents.Count; o++) {
                        if (o == aIdx) continue;
                        float odx = p.X - Agents[o].PosX;
                        float ody = p.Y - Agents[o].PosY;
                        if (odx * odx + ody * ody < d * d * 0.45f) priority += 260f;
                    }
                    if (priority < bestScore) {
                        bestScore = priority;
                        bestDist = d;
                        bestPkt = p;
                    }
                }

                if (bestPkt != null) {
                    bestPkt.AssignedAgentName = a.Name;
                    a.CurrentTargetEndpoint = bestPkt.RemoteEndpoint + " [" + bestPkt.AnomalyType + "]";
                } else {
                    a.CurrentTargetEndpoint = "PATROLLING_SECTOR";
                }

                // Find nearest peer Porygon for P2P Neural Sync (Skill LVL 3) and Flocking Separation
                PorygonAgent nearestPeer = null;
                float minPeerDist = 99999f;
                float sepX = 0f, sepY = 0f;
                for (int o = 0; o < Agents.Count; o++) {
                    if (o == aIdx) continue;
                    float pdx = a.PosX - Agents[o].PosX;
                    float pdy = a.PosY - Agents[o].PosY;
                    float pd = (float)Math.Sqrt(pdx * pdx + pdy * pdy);
                    if (pd < minPeerDist) {
                        minPeerDist = pd;
                        nearestPeer = Agents[o];
                    }
                    if (pd > 1f && pd < 195f) {
                        float push = (195f - pd) / 195f;
                        sepX += (pdx / pd) * push * 2.4f;
                        sepY += (pdy / pd) * push * 2.4f;
                    }
                }

                // Build 16-channel Sensory Input fed by REAL Network Telemetry + Spatial Node Vector!
                float[] sensory = new float[16];
                if (bestPkt != null) {
                    float pdx = bestPkt.X - a.PosX;
                    float pdy = bestPkt.Y - a.PosY;
                    sensory[0] = (pdx < -30f) ? 0.85f : 0.1f;
                    sensory[1] = (pdx >  30f) ? 0.85f : 0.1f;
                    sensory[2] = (pdy < -30f) ? 0.75f : 0.1f;
                    sensory[3] = (pdy >  30f) ? 0.75f : 0.1f;
                    sensory[4] = (bestDist < 420f) ? 0.95f : 0.35f;
                    sensory[5] = (bestDist < 145f) ? 1.30f : 0.0f;
                    sensory[6] = (bestPkt.Kind == 1) ? 1.20f : 0.2f;
                    sensory[8] = (bestPkt.Kind == 2) ? (1.10f + bestPkt.AnomalyScore * 0.6f) : 0.1f;
                }
                // Feed REAL Network Z-Scores & Anomaly Metrics into Sensory Neurons 7, 9..15:
                sensory[7]  = Clamp(feed.NewHostCount * 0.35f, 0f, 1.4f);
                sensory[9]  = Clamp((feed.DownKbps + feed.UpKbps) / 420.0f, 0.05f, 1.3f);
                sensory[10] = (minPeerDist < 230f) ? 1.1f : 0.1f;
                sensory[11] = Clamp(Math.Max(0f, feed.DlZScore) * 0.45f, 0f, 1.5f);
                sensory[12] = Clamp(Math.Max(0f, feed.UlZScore) * 0.45f, 0f, 1.5f);
                sensory[13] = Clamp(Math.Max(0f, feed.PingZScore) * 0.45f, 0f, 1.5f);
                sensory[14] = Clamp((feed.UnusualPortCount + feed.SynCount) * 0.45f, 0f, 1.5f);
                sensory[15] = Clamp(feed.AnomalyIndex * 1.4f, 0.05f, 1.5f);

                br.Step(sensory, 0.042f);

                float targetX = a.WaypointX;
                float targetY = a.WaypointY;

                float cdx = mousePt.X - a.PosX;
                float cdy = mousePt.Y - a.PosY;
                float cDist = (float)Math.Sqrt(cdx * cdx + cdy * cdy);

                // Skill LVL 3: Neural Sync P2P Link (shares learned anomaly weights between Porygons!)
                if (br.HasSkill(3) && nearestPeer != null && minPeerDist < 215f && a.SyncCooldown == 0 && nearestPeer.SyncCooldown == 0) {
                    a.SyncTimer = 1.0f;
                    a.SyncPartnerIdx = nearestPeer.AgentIndex;
                    a.SyncCooldown = 260;
                    nearestPeer.SyncCooldown = 260;
                    br.NeuralSyncs++;
                    nearestPeer.Brain.NeuralSyncs++;
                    if (br.ReinforceReward(0.85f, 18)) a.LevelUpBannerTimer = 1.0f;
                    if (nearestPeer.Brain.ReinforceReward(0.85f, 18)) nearestPeer.LevelUpBannerTimer = 1.0f;
                    br.ActiveStateName = "P2P_IDS_SYNC // WEIGHTS->" + nearestPeer.Name;
                }

                // Skill LVL 6: Deep Sonar Scan when network anomaly index rises or periodically
                if (br.HasSkill(6) && a.ScanWaveTimer <= 0f && (feed.AnomalyIndex > 0.55f || rng.NextDouble() < 0.005)) {
                    a.ScanWaveTimer = 1.0f;
                    br.ActiveStateName = "SONAR_DEEP_SCAN // AUDITING_SOCKETS";
                }

                if (followMouse && cDist < 360f) {
                    float desiredDist = 150f;
                    float ang = (float)Math.Atan2(cdy, cdx);
                    targetX = mousePt.X - (float)Math.Cos(ang) * desiredDist;
                    targetY = mousePt.Y - (float)Math.Sin(ang) * desiredDist;
                    br.ActiveStateName = "CURSOR_LINK // ASSIST_MODE";
                } else if (bestPkt != null && (br.DriveHunger > 0.26f || trainingMode || bestPkt.Kind > 0)) {
                    // If the user is hovering this packet, orbit just beside it so the Porygon inspects it without deleting the tooltip!
                    if (bestPkt.IsHovered) {
                        targetX = bestPkt.X - 115f * a.FacingDir;
                        targetY = bestPkt.Y + 20f;
                        br.ActiveStateName = "INSPECTING_HOVER // " + bestPkt.RemoteEndpoint;
                    } else {
                        targetX = bestPkt.X;
                        targetY = bestPkt.Y;

                        // Skill LVL 5: Quantum Blink when chasing distant high-priority anomaly or new host
                        if (br.HasSkill(5) && bestDist > 320f && bestDist < 680f && a.QuantumBlinkTimer <= 0f && (bestPkt.Kind == 2 || rng.NextDouble() < 0.025)) {
                            a.BlinkStartPt = new PointF(a.PosX, a.PosY);
                            a.PosX = a.PosX + (bestPkt.X - a.PosX) * 0.62f;
                            a.PosY = a.PosY + (bestPkt.Y - a.PosY) * 0.62f;
                            a.QuantumBlinkTimer = 1.0f;
                            br.ActiveStateName = "QUANTUM_BLINK // INTERCEPT_" + bestPkt.AnomalyType;
                        } else if (bestPkt.Kind == 2) {
                            br.ActiveStateName = "IDS_INTERCEPT // " + bestPkt.AnomalyType;
                        } else if (bestPkt.Kind == 1) {
                            br.ActiveStateName = "HOST_AUDIT // " + bestPkt.RemoteEndpoint;
                        } else {
                            br.ActiveStateName = "FLOW_SAMPLE // " + bestPkt.RemoteEndpoint;
                        }

                        float reachDist = 135f + Math.Min(50f, br.Level * 4f);
                        if (bestDist < reachDist) {
                            string ts = DateTime.Now.ToString("HH:mm:ss");
                            if (bestPkt.Kind == 2 && br.HasSkill(4)) {
                                // Skill LVL 4: Tri-Attack Elemental Matrix neutralizes real network anomaly!
                                a.TriAttackTimer = 1.0f;
                                a.TriAttackTarget = new PointF(bestPkt.X, bestPkt.Y);
                                if (br.HasSkill(2)) a.ShieldTimer = 1.0f;
                                br.AnomaliesCleared++;
                                if (br.ReinforceReward(1.45f, 45)) a.LevelUpBannerTimer = 1.0f;
                                br.ActiveStateName = "TRI_ATTACK // PURGED " + bestPkt.RemoteEndpoint;
                                AddIdsLog(string.Format("[{0}] [ALERT_PURGED] {1} neutralized {2} ({3}) Score:{4:0.00}",
                                    ts, a.Name, bestPkt.AnomalyType, bestPkt.RemoteEndpoint, bestPkt.AnomalyScore));
                            } else {
                                // Skill LVL 1: Conversion Beam samples/audits flow
                                a.BeamTimer = 1.0f;
                                a.BeamTarget = new PointF(bestPkt.X, bestPkt.Y);
                                if (bestPkt.Kind >= 1 && br.HasSkill(2)) a.ShieldTimer = 0.85f;
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
                            br.DriveHunger = Math.Max(0.08f, br.DriveHunger - 0.32f);
                            Packets.Remove(bestPkt);
                        }
                    }
                } else {
                    a.WaypointTimer--;
                    float wdx = a.WaypointX - a.PosX;
                    float wdy = a.WaypointY - a.PosY;
                    if (a.WaypointTimer <= 0 || (wdx * wdx + wdy * wdy < 60f * 60f)) {
                        a.WaypointX = Clamp(a.HomeX + rng.Next(-240, 240), 230f, screenW - 180f);
                        a.WaypointY = Clamp(a.HomeY + rng.Next(-160, 160), 270f, screenH - 160f);
                        a.WaypointTimer = rng.Next(90, 210);
                    }
                    targetX = a.WaypointX;
                    targetY = a.WaypointY;
                    if (a.SyncTimer <= 0f && a.ScanWaveTimer <= 0f) {
                        br.ActiveStateName = "IDS_PATROL // Z-SCORE_MONITORING";
                    }
                }

                float speedBonus = 1.0f + Math.Min(0.45f, (br.SynapticGain - 1.0f) * 0.35f);
                float dxT = targetX - a.PosX;
                float dyT = targetY - a.PosY;
                float maxV = 3.6f * speedBonus;
                a.VelX = a.VelX * 0.88f + (Clamp(dxT * 0.026f, -maxV, maxV) + sepX) * 0.12f;
                a.VelY = a.VelY * 0.88f + (Clamp(dyT * 0.026f, -maxV * 0.8f, maxV * 0.8f) + sepY) * 0.12f;
                a.PosX = Clamp(a.PosX + a.VelX, 225f, screenW - 165f);
                a.PosY = Clamp(a.PosY + a.VelY, 265f, screenH - 145f);

                float spd = (float)Math.Sqrt(a.VelX * a.VelX + a.VelY * a.VelY);
                a.WalkPhase += 0.12f + spd * 0.06f;

                if (dxT < -65f) a.FacingDir = -1.0f;
                else if (dxT > 65f) a.FacingDir = 1.0f;

                float baseYaw = a.FacingDir * 0.58f;
                if (a.SpinRollTimer > 0f) {
                    a.SpinRollTimer -= 0.045f;
                    a.Yaw = baseYaw + (1.0f - a.SpinRollTimer) * (float)(Math.PI * 2.0);
                } else {
                    a.Yaw = a.Yaw * 0.88f + baseYaw * 0.12f;
                }

                a.Pitch = 0.16f + (float)Math.Sin(a.FloatBob) * 0.035f;
                a.Roll = a.Roll * 0.88f + Clamp(-a.VelX * 0.018f, -0.06f, 0.06f) * 0.12f;

                if (a.BeamTimer > 0f) a.BeamTimer -= 0.08f;
                if (a.TriAttackTimer > 0f) a.TriAttackTimer -= 0.065f;
                if (a.ShieldTimer > 0f) a.ShieldTimer -= 0.025f;
                if (a.SyncTimer > 0f) a.SyncTimer -= 0.03f;
                if (a.QuantumBlinkTimer > 0f) a.QuantumBlinkTimer -= 0.06f;
                if (a.ScanWaveTimer > 0f) a.ScanWaveTimer -= 0.03f;

                float lookX = (HoveredPacket != null) ? (HoveredPacket.X - a.PosX) : (targetX - a.PosX);
                float lookY = (HoveredPacket != null) ? (HoveredPacket.Y - a.PosY) : (targetY - a.PosY);
                a.HeadYaw = a.HeadYaw * 0.85f + Clamp(lookX / 850f, -0.14f, 0.14f) * 0.15f;
                a.HeadPitch = a.HeadPitch * 0.85f + Clamp(lookY / 850f, -0.08f, 0.12f) * 0.15f;
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
        public int CompareTo(ProjectedFace other) {
            return this.Depth.CompareTo(other.Depth);
        }
    }

    public void Render(Graphics g, float userScale, Color themeCyan, Color themeMag, Font tagFont) {
        List<DataPacket> pktsSnap;
        DataPacket hovPkt;
        PorygonAgent hovAg;
        lock (packetLock) {
            pktsSnap = new List<DataPacket>(Packets);
            hovPkt = HoveredPacket;
            hovAg = HoveredAgent;
        }

        // 1. Draw floating Real Network Nodes (Kind 0 = Normal Flow, Kind 1 = New Host Orb, Kind 2 = Traffic Anomaly)
        for (int i = 0; i < pktsSnap.Count; i++) {
            DataPacket p = pktsSnap[i];
            float sz = (p.Kind == 0 ? 8.5f : 11.5f) + (float)Math.Sin(p.floatRot * 2f) * 2.5f;
            if (p.IsHovered) sz *= 1.35f;

            using (SolidBrush glowB = new SolidBrush(Color.FromArgb(p.IsHovered ? 110 : 55, p.Col.R, p.Col.G, p.Col.B)))
            using (Pen wireP = new Pen(Color.FromArgb(240, p.Col.R, p.Col.G, p.Col.B), p.IsHovered ? 2.6f : 2.0f))
            using (SolidBrush coreB = new SolidBrush(Color.FromArgb(240, 250, 252, 255))) {
                g.FillEllipse(glowB, p.X - sz * 2f, p.Y - sz * 2f, sz * 4f, sz * 4f);
                if (p.Kind == 2) {
                    // Spiky Red Firewall Anomaly Node
                    g.DrawEllipse(wireP, p.X - sz * 1.2f, p.Y - sz * 1.2f, sz * 2.4f, sz * 2.4f);
                    g.DrawLine(wireP, p.X - sz * 1.6f, p.Y, p.X + sz * 1.6f, p.Y);
                    g.DrawLine(wireP, p.X, p.Y - sz * 1.6f, p.X, p.Y + sz * 1.6f);
                } else {
                    PointF[] diamond = new PointF[] {
                        new PointF(p.X, p.Y - sz),
                        new PointF(p.X + sz, p.Y),
                        new PointF(p.X, p.Y + sz),
                        new PointF(p.X - sz, p.Y)
                    };
                    g.DrawPolygon(wireP, diamond);
                    if (p.Kind == 1) {
                        g.DrawEllipse(wireP, p.X - sz * 1.35f, p.Y - sz * 1.35f, sz * 2.7f, sz * 2.7f);
                    }
                }
                g.FillEllipse(coreB, p.X - 3f, p.Y - 3f, 6f, 6f);

                // Subtle mini endpoint label next to non-hovered Anomaly or New-Host nodes
                if (tagFont != null && p.Kind >= 1 && !p.IsHovered) {
                    using (SolidBrush lblB = new SolidBrush(Color.FromArgb(215, p.Col.R, p.Col.G, p.Col.B))) {
                        g.DrawString(p.RemoteEndpoint, tagFont, lblB, p.X + sz + 5f, p.Y - 6f);
                    }
                }
            }
        }

        // 2. Render each of the 3 Porygon Agents & their active Advanced Skills!
        for (int aIdx = 0; aIdx < Agents.Count; aIdx++) {
            RenderSingleAgent(g, Agents[aIdx], userScale, themeCyan, themeMag, tagFont);
        }

        // 3. Render Interactive Mouse-Hover Node Inspection Card (or AI Agent Inspection Card)!
        if (tagFont != null) {
            if (hovPkt != null) {
                DrawHoveredPacketCard(g, hovPkt, tagFont);
            } else if (hovAg != null) {
                DrawHoveredAgentCard(g, hovAg, tagFont);
            }
        }
    }

    private void DrawHoveredPacketCard(Graphics g, DataPacket p, Font f) {
        // Find assigned Porygon agent to draw a tactical vector link line
        PorygonAgent responder = null;
        for (int i = 0; i < Agents.Count; i++) {
            if (Agents[i].Name == p.AssignedAgentName) { responder = Agents[i]; break; }
        }
        if (responder == null && Agents.Count > 0) responder = Agents[0];

        // Rotating Cyber Target Lock Reticle around the hovered node
        float r = 24f;
        using (Pen lockPen = new Pen(Color.FromArgb(245, p.Col.R, p.Col.G, p.Col.B), 2.2f))
        using (Pen linkPen = new Pen(Color.FromArgb(145, p.Col.R, p.Col.G, p.Col.B), 1.4f)) {
            linkPen.DashStyle = DashStyle.Dash;
            g.DrawEllipse(lockPen, p.X - r, p.Y - r, r * 2f, r * 2f);
            float b = 29f, l = 9f;
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

        int cw = 408;
        int ch = 124;
        int cx = (int)p.X + 36;
        int cy = (int)p.Y - ch / 2;
        if (cx + cw > 1890) cx = (int)p.X - cw - 36;
        if (cy < 28) cy = 28;
        if (cy + ch > 1020) cy = 1020 - ch;

        using (SolidBrush bg = new SolidBrush(Color.FromArgb(246, 5, 9, 20)))
        using (Pen border = new Pen(Color.FromArgb(235, p.Col.R, p.Col.G, p.Col.B), 1.8f))
        using (SolidBrush headB = new SolidBrush(p.Col))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(245, 235, 242, 252)))
        using (SolidBrush barBg = new SolidBrush(Color.FromArgb(50, 255, 255, 255))) {
            g.FillRectangle(bg, cx, cy, cw, ch);
            g.DrawRectangle(border, cx, cy, cw, ch);

            string kindTitle = (p.Kind == 2)
                ? ">> [⚠ IDS ANOMALY NODE // HIGH PRIORITY]"
                : (p.Kind == 1 ? ">> [★ NEW ENDPOINT AUDIT // GOLDEN ORB]" : ">> [● VERIFIED NETWORK FLOW NODE]");
            g.DrawString(kindTitle, f, headB, cx + 10, cy + 8);

            g.DrawString(string.Format("REMOTE_EP : {0}  [{1}]", p.RemoteEndpoint, p.ProtocolName), f, txtB, cx + 10, cy + 28);
            g.DrawString(string.Format("LOCAL_SKT : {0}  ({1})", p.LocalEndpoint, p.TcpStateStr), f, txtB, cx + 10, cy + 45);

            // Anomaly Score Progress Bar with generous spacing
            int pct = (int)Math.Round(p.AnomalyScore * 100f);
            g.DrawString(string.Format("ANOMALY   : {0}% [{1}]", pct, p.AnomalyType), f, headB, cx + 10, cy + 63);
            g.FillRectangle(barBg, cx + 288, cy + 66, 108, 8);
            g.FillRectangle(headB, cx + 288, cy + 66, (int)(1.08f * Math.Min(100, pct)), 8);

            g.DrawString("DETAIL    : " + p.AnomalyDetail, f, txtB, cx + 10, cy + 82);
            g.DrawString(string.Format("AI_AGENT  : {0} [LOCKED] | FLOW: {1:0.0} KB/s", p.AssignedAgentName, p.KbpsSample), f, headB, cx + 10, cy + 101);
        }
    }

    private void DrawHoveredAgentCard(Graphics g, PorygonAgent a, Font f) {
        float r = 76f;
        using (Pen lockPen = new Pen(Color.FromArgb(230, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.0f)) {
            lockPen.DashStyle = DashStyle.Dash;
            g.DrawEllipse(lockPen, a.PosX - r, a.PosY - r, r * 2f, r * 2f);
        }

        int cw = 365;
        int ch = 132;
        int cx = (int)a.PosX - cw / 2;
        int cy = (int)a.PosY - 92 - ch;
        if (cx < 150) cx = 150;
        if (cx + cw > 1890) cx = 1890 - cw;
        if (cy < 24) cy = (int)a.PosY + 108;

        FlyBrainConnectome br = a.Brain;
        using (SolidBrush bg = new SolidBrush(Color.FromArgb(246, 5, 9, 20)))
        using (Pen border = new Pen(Color.FromArgb(235, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1.8f))
        using (SolidBrush headB = new SolidBrush(a.AuraColor))
        using (SolidBrush goldB = new SolidBrush(Color.FromArgb(250, 204, 21)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(245, 235, 242, 252))) {
            g.FillRectangle(bg, cx, cy, cw, ch);
            g.DrawRectangle(border, cx, cy, cw, ch);

            g.DrawString(string.Format(">> AI_CORE // {0} [LVL {1}] — {2}", a.Name, br.Level, a.RoleTag), f, headB, cx + 10, cy + 8);
            g.DrawString(a.SpecialtyDesc, f, goldB, cx + 10, cy + 26);
            g.DrawString(string.Format("TELEMETRY : DL_Z:{0:+0.0;-0.0;0.0}σ | UL_Z:{1:+0.0;-0.0;0.0}σ | PING_Z:{2:+0.0;-0.0;0.0}σ",
                LatestFeed.DlZScore, LatestFeed.UlZScore, LatestFeed.PingZScore), f, txtB, cx + 10, cy + 45);
            g.DrawString("TARGET    : " + a.CurrentTargetEndpoint, f, txtB, cx + 10, cy + 63);
            g.DrawString(string.Format("IDS_STATS : Flows:{0} | AnomaliesPurged:{1} | P2P_Syncs:{2}",
                br.PacketsCollected, br.AnomaliesCleared, br.NeuralSyncs), f, txtB, cx + 10, cy + 81);
            g.DrawString(string.Format("SYNAPSES  : Gain {0:0.00}x | XP {1}/{2} | State: {3}",
                br.SynapticGain, br.XP, br.NextLevelXP, br.ActiveStateName), f, headB, cx + 10, cy + 100);
        }
    }

    private void RenderSingleAgent(Graphics g, PorygonAgent a, float userScale, Color themeCyan, Color themeMag, Font tagFont) {
        ModelData model = a.Model;
        if (model == null) return;

        float scale = 1.26f * a.ScaleFactor * (userScale / 100.0f);
        float hopOffset = (a.SpinRollTimer > 0f) ? (float)Math.Sin(a.SpinRollTimer * Math.PI) * 32f : 0f;
        float bobY = a.PosY + (float)Math.Sin(a.FloatBob) * 9f - hopOffset;

        // Skill LVL 6: Deep Sonar Scan Expanding Ring
        if (a.ScanWaveTimer > 0f) {
            float prog = 1.0f - a.ScanWaveTimer;
            float scanR = 40f + prog * 260f;
            int alpha = (int)(a.ScanWaveTimer * 160f);
            using (Pen scanPen = new Pen(Color.FromArgb(alpha, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.2f)) {
                g.DrawEllipse(scanPen, a.PosX - scanR, bobY - scanR * 0.45f, scanR * 2f, scanR * 0.9f);
            }
        }

        // Skill LVL 5: Quantum Blink Warp Trail
        if (a.QuantumBlinkTimer > 0f) {
            int alpha = (int)(a.QuantumBlinkTimer * 190f);
            using (Pen warpPen = new Pen(Color.FromArgb(alpha, 0, 240, 255), 3.5f))
            using (Pen warpCore = new Pen(Color.FromArgb(alpha, 255, 255, 255), 1.5f)) {
                g.DrawLine(warpPen, a.BlinkStartPt.X, a.BlinkStartPt.Y, a.PosX, bobY);
                g.DrawLine(warpCore, a.BlinkStartPt.X, a.BlinkStartPt.Y, a.PosX, bobY);
                g.DrawEllipse(warpPen, a.BlinkStartPt.X - 24f, a.BlinkStartPt.Y - 24f, 48f, 48f);
            }
        }

        // Skill LVL 3: P2P Neural Sync Link Beam between Porygon Agents
        if (a.SyncTimer > 0f && a.SyncPartnerIdx >= 0 && a.SyncPartnerIdx < Agents.Count) {
            PorygonAgent partner = Agents[a.SyncPartnerIdx];
            int alpha = (int)(a.SyncTimer * 210f);
            using (Pen syncOuter = new Pen(Color.FromArgb(alpha / 2, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 6f))
            using (Pen syncCore = new Pen(Color.FromArgb(alpha, 250, 204, 21), 2.0f)) {
                g.DrawLine(syncOuter, a.PosX, bobY, partner.PosX, partner.PosY);
                g.DrawLine(syncCore, a.PosX, bobY, partner.PosX, partner.PosY);
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
            float shade = 0.80f + ndotl * 0.26f;
            int r = (int)Math.Min(255, f.BaseColor.R * shade);
            int gr = (int)Math.Min(255, f.BaseColor.G * shade);
            int b = (int)Math.Min(255, f.BaseColor.B * shade);

            renderList.Add(new ProjectedFace {
                Pts = new PointF[] { p0, p1, p2 },
                Depth = avgZ,
                ShadedColor = Color.FromArgb(255, r, gr, b)
            });
        }

        renderList.Sort();

        if (renderList.Count > 350) {
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
            for (int i = 0; i < renderList.Count; i++) {
                ProjectedFace pf = renderList[i];
                using (SolidBrush fb = new SolidBrush(pf.ShadedColor))
                using (Pen seamPen = new Pen(pf.ShadedColor, 1.1f)) {
                    g.FillPolygon(fb, pf.Pts);
                    g.DrawPolygon(seamPen, pf.Pts);
                }
            }
        }

        // Skill LVL 2: Hexagonal Conversion2 Energy Shield
        if (a.ShieldTimer > 0f) {
            int sAlpha = (int)(Math.Min(1.0f, a.ShieldTimer) * 165f);
            float shR = 72f * (userScale / 100f);
            PointF[] hex = new PointF[6];
            for (int k = 0; k < 6; k++) {
                double ang = a.FloatBob * 1.4f + k * (Math.PI / 3.0);
                hex[k] = new PointF(a.PosX + (float)Math.Cos(ang) * shR, bobY + (float)Math.Sin(ang) * shR * 0.85f);
            }
            using (Pen shPen = new Pen(Color.FromArgb(sAlpha, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 2.2f))
            using (SolidBrush shFill = new SolidBrush(Color.FromArgb(sAlpha / 6, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B))) {
                g.FillPolygon(shFill, hex);
                g.DrawPolygon(shPen, hex);
            }
        }

        // Skill LVL 1: Conversion Beam
        if (a.BeamTimer > 0f) {
            Vec3 beakTip = TransformAgentVertex(a, new Vec3(0, 18, 56), scale);
            float zProj = Math.Max(80f, camDist - beakTip.Z);
            PointF beakPt = new PointF(a.PosX + beakTip.X * (fov / zProj), bobY - beakTip.Y * (fov / zProj));
            using (Pen beamOuter = new Pen(Color.FromArgb(150, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 7f))
            using (Pen beamCore = new Pen(Color.FromArgb(245, 255, 255, 255), 2.5f)) {
                g.DrawLine(beamOuter, beakPt, a.BeamTarget);
                g.DrawLine(beamCore, beakPt, a.BeamTarget);
            }
        }

        // Skill LVL 4: Tri-Attack Elemental Matrix Beam (Cyan Ice + Magenta Plasma + Gold Electric)
        if (a.TriAttackTimer > 0f) {
            Vec3 beakTip = TransformAgentVertex(a, new Vec3(0, 18, 56), scale);
            float zProj = Math.Max(80f, camDist - beakTip.Z);
            PointF beakPt = new PointF(a.PosX + beakTip.X * (fov / zProj), bobY - beakTip.Y * (fov / zProj));
            PointF[] tri = new PointF[3];
            for (int k = 0; k < 3; k++) {
                double ang = a.FloatBob * 3.0f + k * (2.0 * Math.PI / 3.0);
                tri[k] = new PointF(beakPt.X + (float)Math.Cos(ang) * 18f, beakPt.Y + (float)Math.Sin(ang) * 18f);
            }
            using (Pen pIce  = new Pen(Color.FromArgb(220, 0, 240, 255), 3.2f))
            using (Pen pPlas = new Pen(Color.FromArgb(220, 236, 72, 153), 3.2f))
            using (Pen pElec = new Pen(Color.FromArgb(230, 250, 204, 21), 3.2f)) {
                g.DrawPolygon(pElec, tri);
                g.DrawLine(pIce,  tri[0], a.TriAttackTarget);
                g.DrawLine(pPlas, tri[1], a.TriAttackTarget);
                g.DrawLine(pElec, tri[2], a.TriAttackTarget);
                g.DrawEllipse(pPlas, a.TriAttackTarget.X - 18f, a.TriAttackTarget.Y - 18f, 36f, 36f);
            }
        }

        // Floating Mini-HUD Tag under each Porygon showing Name, Level & XP bar
        if (tagFont != null) {
            string badge = string.Format("{0} [LVL {1}]", a.Name, a.Brain.Level);
            float tagX = a.PosX - 54f;
            float tagY = ringY + 14f;
            using (SolidBrush tagBg = new SolidBrush(Color.FromArgb(195, 5, 8, 18)))
            using (Pen tagBorder = new Pen(Color.FromArgb(140, a.AuraColor.R, a.AuraColor.G, a.AuraColor.B), 1f))
            using (SolidBrush txtB = new SolidBrush(Color.FromArgb(245, 240, 248, 255)))
            using (SolidBrush xpFill = new SolidBrush(a.AuraColor)) {
                g.FillRectangle(tagBg, tagX, tagY, 108f, 22f);
                g.DrawRectangle(tagBorder, tagX, tagY, 108f, 22f);
                g.DrawString(badge, tagFont, txtB, tagX + 5f, tagY + 2f);
                float xpRatio = Math.Min(1.0f, (float)a.Brain.XP / Math.Max(1, a.Brain.NextLevelXP));
                g.FillRectangle(xpFill, tagX + 4f, tagY + 17f, 100f * xpRatio, 3f);
            }

            if (a.LevelUpBannerTimer > 0f) {
                using (SolidBrush lvlB = new SolidBrush(Color.FromArgb(250, 250, 204, 21))) {
                    g.DrawString("★ LEVEL UP! NUEVA HABILIDAD ★", tagFont, lvlB, a.PosX - 82f, bobY - 82f);
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
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [DllImport("psapi.dll")]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

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
    const uint MOD_ALT = 0x0001;
    const uint MOD_CONTROL = 0x0002;

    public WallpaperConfig Config;
    private int screenW;
    private int screenH;
    private Bitmap bgBitmap;
    private System.Windows.Forms.Timer renderTimer;
    private System.Threading.Timer telemetryTimer;
    private EventWaitHandle showConfigEvent;
    private Form configForm;
    private Random rng = new Random();

    private Porygon3DAvatar porygon;

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
        public bool IsForeground;
    }
    private List<RainColumn> columns = new List<RainColumn>();
    private const string GLYPHS = "0123456789ABCDEF<>[]{}/\\*+#$@%&010110";

    private float radarAngle = 0f;
    private int frameCount = 0;

    private Font fontRainFg;
    private Font fontRainBg;
    private Font fontHud;
    private Font fontHudSmall;
    private Font fontHudBold;
    private Font fontHudTitle;

    public HackerWallpaperForm(EventWaitHandle configEvt, bool openConfigOnStart) {
        this.showConfigEvent = configEvt;
        Config = WallpaperConfig.Load();

        Rectangle bounds = Screen.PrimaryScreen.Bounds;
        screenW = bounds.Width;
        screenH = bounds.Height;

        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Bounds = bounds;
        this.ShowInTaskbar = false;
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        fontRainFg = new Font("Consolas", 11f, FontStyle.Bold);
        fontRainBg = new Font("Consolas", 9f, FontStyle.Regular);
        fontHudSmall = new Font("Consolas", 7.8f, FontStyle.Regular);
        fontHud = new Font("Consolas", 8.5f, FontStyle.Regular);
        fontHudBold = new Font("Consolas", 9f, FontStyle.Bold);
        fontHudTitle = new Font("Consolas", 9.8f, FontStyle.Bold);

        porygon = new Porygon3DAvatar(screenW, screenH);

        for (int i = 0; i < pingHistory.Length; i++) pingHistory[i] = 15f;
        RebuildBackground();
        RebuildRainColumns();
        UpdateRealTelemetry(null);

        renderTimer = new System.Windows.Forms.Timer();
        renderTimer.Interval = 40; // 25 FPS smooth
        renderTimer.Tick += OnRenderTick;

        telemetryTimer = new System.Threading.Timer(UpdateRealTelemetry, null, 1200, 1800);

        if (openConfigOnStart) {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 400;
            t.Tick += (s, ev) => { t.Stop(); ShowConfigWindow(); };
            t.Start();
        }
    }

    public void GetThemeColors(out Color c1, out Color c2, out Color c3, out Color head) {
        if (Config.ThemeIndex == 1) {
            c1 = Color.FromArgb(0, 255, 102);
            c2 = Color.FromArgb(16, 185, 129);
            c3 = Color.FromArgb(52, 211, 153);
            head = Color.FromArgb(220, 255, 235);
        } else if (Config.ThemeIndex == 2) {
            c1 = Color.FromArgb(255, 42, 85);
            c2 = Color.FromArgb(245, 158, 11);
            c3 = Color.FromArgb(244, 63, 94);
            head = Color.FromArgb(255, 235, 240);
        } else if (Config.ThemeIndex == 3) {
            c1 = Color.FromArgb(56, 189, 248);
            c2 = Color.FromArgb(59, 130, 246);
            c3 = Color.FromArgb(99, 102, 241);
            head = Color.FromArgb(224, 242, 254);
        } else {
            c1 = Color.FromArgb(0, 240, 255);
            c2 = Color.FromArgb(236, 72, 153);
            c3 = Color.FromArgb(168, 85, 247);
            head = Color.FromArgb(230, 252, 255);
        }
    }

    public void RebuildBackground() {
        Bitmap newBg = new Bitmap(screenW, screenH, PixelFormat.Format32bppPArgb);
        Color c1, c2, c3, head;
        GetThemeColors(out c1, out c2, out c3, out head);

        using (Graphics g = Graphics.FromImage(newBg)) {
            g.SmoothingMode = SmoothingMode.HighQuality;
            using (LinearGradientBrush bgGrad = new LinearGradientBrush(
                new Rectangle(0, 0, screenW, screenH),
                Color.FromArgb(4, 6, 14),
                Color.FromArgb(8, 13, 28),
                LinearGradientMode.Vertical)) {
                g.FillRectangle(bgGrad, 0, 0, screenW, screenH);
            }

            using (Pen gridPen = new Pen(Color.FromArgb(14, c1.R, c1.G, c1.B), 1f)) {
                for (int x = 0; x < screenW; x += 64) g.DrawLine(gridPen, x, 0, x, screenH);
                for (int y = 0; y < screenH; y += 64) g.DrawLine(gridPen, 0, y, screenW, y);
            }

            int horizonY = (int)(screenH * 0.68f);
            using (Pen floorPen = new Pen(Color.FromArgb(22, c2.R, c2.G, c2.B), 1f)) {
                for (int i = 1; i <= 8; i++) {
                    int fy = horizonY + (int)(Math.Pow(i / 8.0, 1.7) * (screenH - horizonY));
                    g.DrawLine(floorPen, 0, fy, screenW, fy);
                }
                int vanishX = (int)(screenW * 0.56f);
                for (int x = -screenW; x <= screenW * 2; x += 140) {
                    g.DrawLine(floorPen, vanishX + (x - vanishX) / 5, horizonY, x, screenH);
                }
            }

            using (Pen scanPen = new Pen(Color.FromArgb(12, 0, 240, 255), 1f)) {
                for (int y = 0; y < screenH; y += 4) g.DrawLine(scanPen, 0, y, screenW, y);
            }
        }
        Bitmap old = bgBitmap;
        bgBitmap = newBg;
        if (old != null) old.Dispose();
    }

    public void RebuildRainColumns() {
        columns.Clear();
        int count = Math.Max(10, Config.RainDensity);
        float spacing = (float)screenW / count;
        for (int i = 0; i < count; i++) {
            float x = i * spacing + 6f;
            if (x < 135 && i % 2 == 1) continue;

            RainColumn col = new RainColumn();
            col.X = x;
            col.Y = rng.Next(-screenH, screenH);
            col.IsForeground = (rng.NextDouble() > 0.45) && (x > 135);
            col.BaseSpeed = col.IsForeground ? (float)(5.2 + rng.NextDouble() * 6.5) : (float)(2.8 + rng.NextDouble() * 3.8);
            col.Length = rng.Next(8, 20);
            col.Chars = new char[col.Length];
            for (int c = 0; c < col.Length; c++) col.Chars[c] = GLYPHS[rng.Next(GLYPHS.Length)];

            double p = rng.NextDouble();
            col.ColorSlot = (p < 0.58) ? 0 : (p < 0.82 ? 1 : 2);
            columns.Add(col);
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

    private void UpdateRealTelemetry(object state) {
        try {
            telemetryCycles++;
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
                    PingReply rep = p.Send("1.1.1.1", 900);
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

            // Inspect Active TCP Connections & Feed Real Nodes to the 3 Porygons!
            IPGlobalProperties ipGlobal = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConns = ipGlobal.GetActiveTcpConnections();
            int estCount = 0;
            int synCount = 0;
            int unusualPortCount = 0;
            int newHostCount = 0;
            string sampleConn = null;
            string anomalyHeadline = "BASELINE_NORMAL";

            List<RadarBlip> newBlips = new List<RadarBlip>();
            HashSet<string> seenHosts = new HashSet<string>();

            newBlips.Add(new RadarBlip {
                AngleDeg = 45f,
                DistNorm = 0.28f,
                Label = "1.1.1.1:53",
                FullRemoteEp = "1.1.1.1:53",
                LocalEp = ipv4 + ":UDP/53",
                Proto = "DNS/RESOLVER",
                RingZone = "RING1:CORE_DNS",
                AnomalyScore = 0.04f,
                IsHttps = true,
                IsAnomaly = false
            });

            for (int i = 0; i < tcpConns.Length; i++) {
                TcpState st = tcpConns[i].State;
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
                    string fullLoc = ipv4 + ":" + locPort;
                    string proto = ClassifyPortProtocol(remPort);
                    bool isLan = remIp.StartsWith("192.168.") || remIp.StartsWith("10.") || remIp.StartsWith("172.");
                    bool isStandardPort = (remPort == 443 || remPort == 80 || remPort == 53 || remPort == 123);

                    if (sampleConn == null) {
                        sampleConn = string.Format("TCP_{0} {1} -> {2}", st.ToString().ToUpper(), fullLoc, fullRem);
                    }

                    bool isNewHost = false;
                    if (!baselineInitialized) {
                        knownRemoteHosts.Add(remIp);
                    } else if (!knownRemoteHosts.Contains(remIp)) {
                        knownRemoteHosts.Add(remIp);
                        isNewHost = true;
                        newHostCount++;
                    }

                    // Determine if this real connection is an Anomaly (Kind=2), New Host (Kind=1), or Normal Flow (Kind=0)
                    int nodeKind = 0;
                    string anomType = "BASELINE_FLOW";
                    string anomDetail = "Encrypted/standard socket within baseline profile";
                    float anomScore = 0.08f;

                    if (!isStandardPort && !isLan) {
                        unusualPortCount++;
                        nodeKind = 2;
                        anomType = "NON_STD_PORT_" + remPort;
                        anomDetail = "External connection on non-standard TCP port " + remPort;
                        anomScore = (remPort == 22 || remPort == 3389 || remPort == 4444 || remPort == 445) ? 0.94f : 0.78f;
                        anomalyHeadline = "PORT_ALERT: " + fullRem;
                    } else if (st == TcpState.SynSent) {
                        nodeKind = 2;
                        anomType = "HALF_OPEN_SYN";
                        anomDetail = "Outbound SYN_SENT handshake pending response";
                        anomScore = 0.76f;
                        anomalyHeadline = "SYN_BURST: " + fullRem;
                    } else if (isNewHost) {
                        nodeKind = 1;
                        anomType = "NEW_EXTERNAL_HOST";
                        anomDetail = "First time seeing remote IP " + remIp + " in session";
                        anomScore = 0.54f;
                        if (anomalyHeadline == "BASELINE_NORMAL") anomalyHeadline = "NEW_HOST: " + remIp;
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
                            Label = shortIp + ":" + remPort,
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

            lock (telemetryLock) {
                cpuUsage = newCpu;
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

                for (int i = 0; i < pingHistory.Length - 1; i++) pingHistory[i] = pingHistory[i + 1];
                pingHistory[pingHistory.Length - 1] = newPing;

                string ts = DateTime.Now.ToString("HH:mm:ss");
                string logEntry;
                if (anomalyHeadline != "BASELINE_NORMAL") {
                    logEntry = string.Format("[{0}] [IDS_DETECT] {1} | SCORE:{2:0.00}", ts, anomalyHeadline, compositeAnomaly);
                } else if (rng.Next(2) == 0 && sampleConn != null) {
                    logEntry = string.Format("[{0}] {1} [Z:{2:+0.0;-0.0;0.0}σ]", ts, sampleConn, dlZ);
                } else {
                    logEntry = string.Format("[{0}] BASELINE // DL:{1:0.0}KB/s (μ={2:0.0}) | PING:{3}ms | HOSTS:{4}",
                        ts, netDownKbps, emaDownKbps, pingMs, knownRemoteHosts.Count);
                }
                liveLogLines.Add(logEntry);
                while (liveLogLines.Count > 5) liveLogLines.RemoveAt(0);
            }
        } catch { }
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        AttachToDesktopWorkerW();
        RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.W);
        RegisterHotKey(this.Handle, HOTKEY_MODEL_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.P);
        renderTimer.Start();
        try { EmptyWorkingSet(Process.GetCurrentProcess().Handle); } catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        UnregisterHotKey(this.Handle, HOTKEY_ID);
        UnregisterHotKey(this.Handle, HOTKEY_MODEL_ID);
        if (porygon != null) porygon.SaveBrainStates();
        base.OnFormClosing(e);
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
            int hk = m.WParam.ToInt32();
            if (hk == HOTKEY_ID) {
                ShowConfigWindow();
                return;
            } else if (hk == HOTKEY_MODEL_ID) {
                TriggerPorygonTrainingWave();
                return;
            }
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

    private void AttachToDesktopWorkerW() {
        cachedProgman = FindWindow("Progman", null);
        IntPtr res;
        SendMessageTimeout(cachedProgman, 0x052C, new IntPtr(0xD), new IntPtr(0x1), 0, 1000, out res);
        SendMessageTimeout(cachedProgman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 1000, out res);

        cachedDefView = FindWindowEx(cachedProgman, IntPtr.Zero, "SHELLDLL_DefView", null);

        const int WS_POPUP = unchecked((int)0x80000000);
        const int WS_CAPTION = 0x00C00000;
        const int WS_THICKFRAME = 0x00040000;
        const int WS_DISABLED = 0x08000000;
        const uint SWP_FRAMECHANGED = 0x0020;

        int style = GetWindowLong(this.Handle, GWL_STYLE);
        style = (style & ~WS_POPUP & ~WS_CAPTION & ~WS_THICKFRAME) | WS_CHILD | WS_VISIBLE | WS_DISABLED;
        SetWindowLong(this.Handle, GWL_STYLE, style);

        SetParent(this.Handle, cachedProgman);

        int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
        SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
        SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);

        if (cachedDefView != IntPtr.Zero) {
            SetWindowPos(this.Handle, cachedDefView, 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        } else {
            SetWindowPos(this.Handle, new IntPtr(1), 0, 0, screenW, screenH, SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        }
    }

    private void EnsureBehindDesktopIcons() {
        if (cachedProgman == IntPtr.Zero) cachedProgman = FindWindow("Progman", null);
        if (cachedDefView == IntPtr.Zero) cachedDefView = FindWindowEx(cachedProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
        if (cachedDefView != IntPtr.Zero) {
            SetWindowPos(this.Handle, cachedDefView, 0, 0, 0, 0, 0x0001 | 0x0002 | SWP_NOACTIVATE);
        }
    }

    private bool IsAnotherWindowMaximized() {
        IntPtr fg = GetForegroundWindow();
        if (fg == IntPtr.Zero || fg == this.Handle) return false;
        if (configForm != null && !configForm.IsDisposed && fg == configForm.Handle) return false;
        return IsZoomed(fg);
    }

    private void OnRenderTick(object sender, EventArgs e) {
        if (showConfigEvent != null && showConfigEvent.WaitOne(0)) {
            ShowConfigWindow();
        }

        frameCount++;
        if (frameCount <= 5 || frameCount % 30 == 0) {
            EnsureBehindDesktopIcons();
        }
        if (Config.AutoPauseMaximized && frameCount % 8 == 0 && IsAnotherWindowMaximized()) {
            return;
        }

        if (Config.RainSpeed > 0) {
            float speedMult = Config.RainSpeed / 100.0f;
            int charStep = 16;
            for (int i = 0; i < columns.Count; i++) {
                RainColumn col = columns[i];
                col.Y += col.BaseSpeed * speedMult;
                if (rng.NextDouble() < 0.22) {
                    col.Chars[rng.Next(col.Length)] = GLYPHS[rng.Next(GLYPHS.Length)];
                }
                if (col.Y - col.Length * charStep > screenH) {
                    col.Y = rng.Next(-260, -20);
                }
            }
        }

        if (Config.ShowPorygon) {
            Porygon3DAvatar.RealTelemetryFeed feedSnap;
            lock (telemetryLock) { feedSnap = currentFeed; }
            porygon.Update(screenW, screenH, Control.MousePosition, feedSnap, Config.FollowMouse, Config.TrainingMode);
        }

        radarAngle = (radarAngle + 3.0f) % 360f;

        if (frameCount % 600 == 0) {
            try { EmptyWorkingSet(Process.GetCurrentProcess().Handle); } catch { }
        }

        this.Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnPaint(PaintEventArgs e) {
        Graphics g = e.Graphics;
        g.DrawImageUnscaled(bgBitmap, 0, 0);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        Color c1, c2, c3, headCol;
        GetThemeColors(out c1, out c2, out c3, out headCol);

        int hudW = 584;
        int hudH = 228;
        int hudX = screenW - hudW - 22;
        int hudY = 22;

        int radPanelW = 246;
        int radPanelH = 228;
        int radPanelX = hudX - radPanelW - 14;
        int radPanelY = 22;

        // 1. Falling Hacker Code Rain
        if (Config.RainSpeed > 0) {
            int charStep = 16;
            using (SolidBrush headBrush = new SolidBrush(Color.FromArgb(240, headCol.R, headCol.G, headCol.B))) {
                for (int i = 0; i < columns.Count; i++) {
                    RainColumn col = columns[i];
                    Font f = col.IsForeground ? fontRainFg : fontRainBg;
                    int maxAlpha = col.IsForeground ? 190 : 80;
                    if (col.X < 135) maxAlpha = 42;

                    Color baseCol = (col.ColorSlot == 0) ? c1 : (col.ColorSlot == 1 ? c2 : c3);

                    for (int c = 0; c < col.Length; c++) {
                        float cy = col.Y - c * charStep;
                        if (cy < -20 || cy > screenH) continue;

                        if (c == 0 && col.IsForeground) {
                            g.DrawString(col.Chars[c].ToString(), f, headBrush, col.X, cy);
                        } else {
                            float ratio = 1.0f - ((float)c / col.Length);
                            int alpha = (int)(ratio * maxAlpha);
                            if (alpha <= 8) continue;
                            using (SolidBrush tb = new SolidBrush(Color.FromArgb(alpha, baseCol.R, baseCol.G, baseCol.B))) {
                                g.DrawString(col.Chars[c].ToString(), f, tb, col.X, cy);
                            }
                        }
                    }
                }
            }
        }

        // 2. Render 3 Autonomous 3D Porygon Evolutions, Hover Cards & Multi-Agent IDS Connectome HUD!
        if (Config.ShowPorygon) {
            porygon.Render(g, Config.PorygonScale, c1, c2, fontHudSmall);
            DrawFlyBrainConnectomeHUD(g, c1, c2, c3);
        }

        // 3. Render Enlarged Functional Network Sonar Radar with Mouse Hover Blip Inspection!
        if (Config.ShowRadar) {
            DrawFunctionalRadar(g, radPanelX, radPanelY, radPanelW, radPanelH, c1, c2, c3);
        }

        // 4. Render High-Contrast System Telemetry HUD
        if (Config.ShowTelemetry) {
            DrawTelemetryHUD(g, hudX, hudY, hudW, hudH, c1, c2, c3);
        }
    }

    private void DrawFunctionalRadar(Graphics g, int px, int py, int pw, int ph, Color c1, Color c2, Color c3) {
        List<RadarBlip> blipsSnap;
        int tcpCnt;
        lock (telemetryLock) {
            blipsSnap = new List<RadarBlip>(radarBlips);
            tcpCnt = tcpEstablishedCount;
        }

        Point mousePt = Control.MousePosition;
        int hoveredBlipIdx = -1;
        float hoveredBlipX = 0f, hoveredBlipY = 0f;

        using (SolidBrush panelBg = new SolidBrush(Color.FromArgb(244, 5, 8, 18)))
        using (Pen borderPen = new Pen(Color.FromArgb(170, c1.R, c1.G, c1.B), 1.4f))
        using (SolidBrush c1B = new SolidBrush(Color.FromArgb(245, c1.R, c1.G, c1.B)))
        using (SolidBrush c2B = new SolidBrush(Color.FromArgb(245, c2.R, c2.G, c2.B)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(230, 226, 232, 240))) {
            g.FillRectangle(panelBg, px, py, pw, ph);
            g.DrawRectangle(borderPen, px, py, pw, ph);

            using (Pen cornerPen = new Pen(Color.FromArgb(235, c2.R, c2.G, c2.B), 2.5f)) {
                g.DrawLine(cornerPen, px, py, px + 20, py);
                g.DrawLine(cornerPen, px, py, px, py + 20);
                g.DrawLine(cornerPen, px + pw, py + ph, px + pw - 20, py + ph);
                g.DrawLine(cornerPen, px + pw, py + ph, px + pw, py + ph - 20);
            }

            g.DrawString(">> NET_SONAR // 360°", fontHudBold, c1B, px + 10, py + 7);
            g.DrawString(string.Format("NODES:{0}", tcpCnt), fontHudBold, c2B, px + pw - 74, py + 7);

            int radR = 78;
            int radCX = px + pw / 2;
            int radCY = py + 110;

            using (SolidBrush scopeBg = new SolidBrush(Color.FromArgb(255, 3, 6, 14)))
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
            g.DrawString("HOVER BLIPS OR NODES TO INSPECT", fontHudSmall, txtB, px + 10, py + ph - 18);

            // If user is hovering a Radar Blip, draw an interactive Radar Node Telemetry Card below the radar!
            if (hoveredBlipIdx >= 0) {
                RadarBlip hb = blipsSnap[hoveredBlipIdx];
                int cardW = 285;
                int cardH = 88;
                int cardX = px - 20;
                int cardY = py + ph + 8;
                Color accent = hb.IsAnomaly ? Color.FromArgb(255, 42, 85) : c1;
                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(248, 5, 9, 20)))
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
        float locCpu, locRamUsed, locRamTot, locDown, locUp, locDlZ;
        int locRamPct, locPing, locTcp;
        string locNic, locIp;
        string[] logsSnap;
        float[] pingSnap = new float[pingHistory.Length];

        lock (telemetryLock) {
            locCpu = cpuUsage;
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

        using (SolidBrush hudBg = new SolidBrush(Color.FromArgb(244, 5, 8, 18)))
        using (Pen hudBorder = new Pen(Color.FromArgb(170, c1.R, c1.G, c1.B), 1.4f))
        using (SolidBrush c1Brush = new SolidBrush(Color.FromArgb(250, c1.R, c1.G, c1.B)))
        using (SolidBrush c2Brush = new SolidBrush(Color.FromArgb(250, c2.R, c2.G, c2.B)))
        using (SolidBrush c3Brush = new SolidBrush(Color.FromArgb(250, c3.R, c3.G, c3.B)))
        using (SolidBrush txtBrush = new SolidBrush(Color.FromArgb(245, 235, 242, 250)))
        using (SolidBrush barBg = new SolidBrush(Color.FromArgb(45, 255, 255, 255))) {
            g.FillRectangle(hudBg, hudX, hudY, hudW, hudH);
            g.DrawRectangle(hudBorder, hudX, hudY, hudW, hudH);

            using (Pen cornerPen = new Pen(Color.FromArgb(235, c2.R, c2.G, c2.B), 2.5f)) {
                g.DrawLine(cornerPen, hudX, hudY, hudX + 24, hudY);
                g.DrawLine(cornerPen, hudX, hudY, hudX, hudY + 24);
                g.DrawLine(cornerPen, hudX + hudW, hudY + hudH, hudX + hudW - 24, hudY + hudH);
                g.DrawLine(cornerPen, hudX + hudW, hudY + hudH, hudX + hudW, hudY + hudH - 24);
            }

            // Row 1: Header
            g.DrawString(">> SYS_TELEMETRY // " + locNic.ToUpper() + " [" + locIp + "]", fontHudTitle, c1Brush, hudX + 14, hudY + 10);
            g.DrawString(string.Format("PING: {0}ms  |  TCP: {1}", locPing, locTcp), fontHudBold, c2Brush, hudX + hudW - 195, hudY + 10);

            // Row 2: CPU Bar, RAM Bar, and NON-OVERLAPPING Latency Sparkline Box!
            int rowY = hudY + 38;
            g.DrawString(string.Format("CPU_LOAD : {0:0.0}%", locCpu), fontHudBold, txtBrush, hudX + 14, rowY);
            g.FillRectangle(barBg, hudX + 14, rowY + 20, 155, 7);
            g.FillRectangle(c1Brush, hudX + 14, rowY + 20, (int)(1.55f * Math.Min(100f, locCpu)), 7);

            g.DrawString(string.Format("MEM_USED : {0:0.0}/{1:0.0} GB ({2}%)", locRamUsed, locRamTot, locRamPct), fontHudBold, txtBrush, hudX + 186, rowY);
            g.FillRectangle(barBg, hudX + 186, rowY + 20, 195, 7);
            g.FillRectangle(c2Brush, hudX + 186, rowY + 20, (int)(1.95f * Math.Min(100, locRamPct)), 7);

            int spX = hudX + 402;
            int spW = 164;
            g.DrawString(string.Format("PING_GRAPH ({0}ms)", locPing), fontHudSmall, c3Brush, spX, rowY + 1);
            g.FillRectangle(barBg, spX, rowY + 17, spW, 15);
            for (int i = 0; i < pingSnap.Length; i++) {
                int bh = (int)Math.Min(13f, Math.Max(2f, pingSnap[i] * 0.22f));
                Brush sb = (pingSnap[i] > 60f) ? c2Brush : c1Brush;
                g.FillRectangle(sb, spX + 3 + i * 8, rowY + 31 - bh, 5, bh);
            }

            // Row 3: Network Live Throughput + Z-Score + Hotkey Hints
            int netY = hudY + 76;
            g.DrawString(string.Format("NET_IO // DL:{0:0.0}KB/s ({1:+0.0;-0.0;0.0}σ) | UL:{2:0.0}KB/s | CTRL+ALT+P: TEST IDS | CTRL+ALT+W: MENU",
                locDown, locDlZ, locUp), fontHudBold, c3Brush, hudX + 14, netY);

            using (Pen divPen = new Pen(Color.FromArgb(55, c1.R, c1.G, c1.B), 1f)) {
                g.DrawLine(divPen, hudX + 14, netY + 20, hudX + hudW - 14, netY + 20);
            }

            // Row 4: Crisp Live Packet / Anomaly Feed
            for (int i = 0; i < logsSnap.Length; i++) {
                Brush lb = logsSnap[i].Contains("IDS_DETECT") ? c2Brush : ((i == logsSnap.Length - 1) ? c1Brush : txtBrush);
                g.DrawString(logsSnap[i], fontHud, lb, hudX + 14, netY + 26 + i * 19);
            }
        }
    }

    private void DrawFlyBrainConnectomeHUD(Graphics g, Color c1, Color c2, Color c3) {
        int bw = 584;
        int bh = 128;
        int bx = screenW - bw - 22;
        int by = screenH - bh - 54;

        using (SolidBrush bg = new SolidBrush(Color.FromArgb(244, 5, 8, 18)))
        using (Pen border = new Pen(Color.FromArgb(165, c1.R, c1.G, c1.B), 1.3f))
        using (SolidBrush c1B = new SolidBrush(Color.FromArgb(245, c1.R, c1.G, c1.B)))
        using (SolidBrush c2B = new SolidBrush(Color.FromArgb(245, c2.R, c2.G, c2.B)))
        using (SolidBrush goldB = new SolidBrush(Color.FromArgb(250, 204, 21)))
        using (SolidBrush txtB = new SolidBrush(Color.FromArgb(238, 226, 232, 240))) {
            g.FillRectangle(bg, bx, by, bw, bh);
            g.DrawRectangle(border, bx, by, bw, bh);

            float anomIdx = currentFeed.AnomalyIndex;
            g.DrawString(string.Format(">> PORYGON NEURAL IDS SWARM // ANOMALY_INDEX: {0:0}% [{1}]",
                anomIdx * 100f, currentFeed.LastAnomalySummary ?? "BASELINE_OK"), fontHudSmall, anomIdx > 0.45f ? c2B : c1B, bx + 10, by + 6);
            g.DrawString("HOVER NODES FOR DETAILS", fontHudSmall, goldB, bx + bw - 162, by + 6);

            for (int i = 0; i < porygon.Agents.Count; i++) {
                Porygon3DAvatar.PorygonAgent ag = porygon.Agents[i];
                FlyBrainConnectome br = ag.Brain;
                int ry = by + 24 + i * 21;

                using (SolidBrush agB = new SolidBrush(ag.AuraColor)) {
                    string line = string.Format("[{0,-9}] LVL:{1} XP:{2,3}/{3,3} | PURGED:{4,2} | {5}",
                        ag.Name, br.Level, br.XP, br.NextLevelXP, br.AnomaliesCleared, br.ActiveStateName);
                    g.DrawString(line, fontHudSmall, agB, bx + 10, ry);
                }

                int stripX = bx + bw - 136;
                for (int n = 0; n < 16; n++) {
                    float glow = br.SpikeGlow[n * 4];
                    Color nc = (glow > 0.35f)
                        ? Color.FromArgb((int)(110 + glow * 145), ag.AuraColor.R, ag.AuraColor.G, ag.AuraColor.B)
                        : Color.FromArgb(60, c1.R, c1.G, c1.B);
                    using (SolidBrush nb = new SolidBrush(nc)) {
                        g.FillEllipse(nb, stripX + n * 8, ry + 3, 6f, 6f);
                    }
                }
            }

            string lastIdsLog = (porygon.IdsDetectionLog.Count > 0)
                ? porygon.IdsDetectionLog[porygon.IdsDetectionLog.Count - 1]
                : "IDS_LOG // Monitoring live TCP sockets & bandwidth Z-Score...";
            g.DrawString(lastIdsLog, fontHudSmall, goldB, bx + 10, by + bh - 34);

            string footer = "SKILLS: [L1]BEAM [L2]HEX-SHIELD [L3]P2P-SYNC [L4]TRI-ATTACK [L5]QUANTUM-BLINK [L6]DEEP-SONAR";
            g.DrawString(footer, fontHudSmall, txtB, bx + 10, by + bh - 17);
        }
    }

    public void ShowConfigWindow() {
        if (configForm != null && !configForm.IsDisposed) {
            configForm.Activate();
            return;
        }

        configForm = new Form();
        configForm.Text = "Configurar Hacker Wallpaper // Enjambre Autónomo Porygon IDS";
        configForm.ClientSize = new Size(495, 585);
        configForm.FormBorderStyle = FormBorderStyle.FixedDialog;
        configForm.MaximizeBox = false;
        configForm.StartPosition = FormStartPosition.CenterScreen;
        configForm.BackColor = Color.FromArgb(13, 18, 32);
        configForm.ForeColor = Color.FromArgb(226, 232, 240);
        configForm.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        configForm.TopMost = true;

        Label title = new Label {
            Text = "PANEL DE CONTROL // PORYGON NEURAL IDS SWARM",
            Font = new Font("Consolas", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 240, 255),
            Location = new Point(22, 12),
            AutoSize = true
        };
        configForm.Controls.Add(title);

        // --- SECCIÓN DE DETECCIÓN DE ANOMALÍAS DE RED Y ENTRENAMIENTO ---
        Label lblSwarm = new Label {
            Text = "Detección de Anomalías de Red (Porygon, Porygon2 y Porygon-Z):",
            ForeColor = Color.FromArgb(236, 72, 153),
            Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
            Location = new Point(22, 38),
            AutoSize = true
        };
        Label lblSwarmStatus = new Label {
            Location = new Point(22, 58),
            Size = new Size(450, 40),
            Font = new Font("Consolas", 8.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(250, 204, 21),
            BackColor = Color.FromArgb(18, 25, 44)
        };
        Action updateSwarmLabel = () => {
            if (porygon == null || porygon.Agents.Count < 3) return;
            var a0 = porygon.Agents[0];
            var a1 = porygon.Agents[1];
            var a2 = porygon.Agents[2];
            lblSwarmStatus.Text = string.Format(
                " PORYGON: LVL {0} ({1}XP) | PORYGON2: LVL {2} ({3}XP) | PORYGON-Z: LVL {4} ({5}XP)\n" +
                " Flujos Auditados: {6} | Anomalías Purgadas: {7} | Hosts Conocidos: {8}",
                a0.Brain.Level, a0.Brain.XP, a1.Brain.Level, a1.Brain.XP, a2.Brain.Level, a2.Brain.XP,
                a0.Brain.PacketsCollected + a1.Brain.PacketsCollected + a2.Brain.PacketsCollected,
                a0.Brain.AnomaliesCleared + a1.Brain.AnomaliesCleared + a2.Brain.AnomaliesCleared,
                knownRemoteHosts.Count);
        };
        updateSwarmLabel();

        Button btnTrainWave = new Button {
            Text = "⚡ Simular Anomalía IDS (Ctrl+Alt+P)",
            Location = new Point(22, 104),
            Size = new Size(242, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(30, 41, 68),
            ForeColor = Color.FromArgb(0, 240, 255),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
        };
        btnTrainWave.Click += (s, ev) => {
            porygon.TriggerTrainingWave(screenW, screenH);
            updateSwarmLabel();
            this.Invalidate();
        };

        Button btnBoostLvl = new Button {
            Text = "★ Subir +1 LVL / Habilidad",
            Location = new Point(272, 104),
            Size = new Size(200, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(48, 30, 68),
            ForeColor = Color.FromArgb(250, 204, 21),
            Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
        };
        btnBoostLvl.Click += (s, ev) => {
            porygon.BoostAllLevels();
            updateSwarmLabel();
            this.Invalidate();
        };

        configForm.Controls.Add(lblSwarm);
        configForm.Controls.Add(lblSwarmStatus);
        configForm.Controls.Add(btnTrainWave);
        configForm.Controls.Add(btnBoostLvl);

        // --- PALETA DE COLORES ---
        Label lblTheme = new Label { Text = "Paleta de Colores:", Location = new Point(22, 144), AutoSize = true };
        ComboBox cbTheme = new ComboBox {
            Location = new Point(22, 165),
            Width = 450,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(22, 30, 50),
            ForeColor = Color.White
        };
        cbTheme.Items.AddRange(new object[] {
            "Cyberpunk Neón (Cian Eléctrico + Magenta + Violeta)",
            "Hacker Clásico / Matrix (Verde Fosforescente #00FF66)",
            "Red Team / Alerta Cibernética (Rojo Carmesí + Ámbar)",
            "Stealth / Azul Hielo Minimalista"
        });
        cbTheme.SelectedIndex = Math.Max(0, Math.Min(3, Config.ThemeIndex));
        cbTheme.SelectedIndexChanged += (s, ev) => {
            Config.ThemeIndex = cbTheme.SelectedIndex;
            RebuildBackground();
            Config.Save();
        };
        configForm.Controls.Add(lblTheme);
        configForm.Controls.Add(cbTheme);

        Label lblSpeed = new Label { Text = string.Format("Velocidad de Lluvia de Código: {0}%", Config.RainSpeed), Location = new Point(22, 198), AutoSize = true };
        TrackBar tbSpeed = new TrackBar {
            Location = new Point(22, 218),
            AutoSize = false,
            Size = new Size(450, 30),
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

        Label lblDens = new Label { Text = string.Format("Densidad de Columnas: {0}", Config.RainDensity), Location = new Point(22, 252), AutoSize = true };
        TrackBar tbDens = new TrackBar {
            Location = new Point(22, 272),
            AutoSize = false,
            Size = new Size(450, 30),
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

        Label lblScale = new Label { Text = string.Format("Tamaño de los 3 Avatares Porygon: {0}%", Config.PorygonScale), Location = new Point(22, 306), AutoSize = true };
        TrackBar tbScale = new TrackBar {
            Location = new Point(22, 326),
            AutoSize = false,
            Size = new Size(450, 30),
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

        CheckBox chkTrain = new CheckBox {
            Text = "Modo Entrenamiento Intensivo Continuo (Más Orbes Dorados y Anomalías)",
            Location = new Point(24, 366),
            AutoSize = true,
            ForeColor = Color.FromArgb(250, 204, 21),
            Checked = Config.TrainingMode
        };
        chkTrain.CheckedChanged += (s, ev) => { Config.TrainingMode = chkTrain.Checked; Config.Save(); };

        CheckBox chkFollowMouse = new CheckBox {
            Text = "Seguir el cursor del mouse (Desactivado por defecto — 100% Independientes)",
            Location = new Point(24, 394),
            AutoSize = true,
            Checked = Config.FollowMouse
        };
        chkFollowMouse.CheckedChanged += (s, ev) => { Config.FollowMouse = chkFollowMouse.Checked; Config.Save(); };

        CheckBox chkTel = new CheckBox {
            Text = "Mostrar Terminal de Telemetría Real (CPU, RAM, Red, Ping)",
            Location = new Point(24, 422),
            AutoSize = true,
            Checked = Config.ShowTelemetry
        };
        chkTel.CheckedChanged += (s, ev) => { Config.ShowTelemetry = chkTel.Checked; Config.Save(); };

        CheckBox chkRad = new CheckBox {
            Text = "Mostrar Radar Sonar 360° de Conexiones de Red",
            Location = new Point(24, 450),
            AutoSize = true,
            Checked = Config.ShowRadar
        };
        chkRad.CheckedChanged += (s, ev) => { Config.ShowRadar = chkRad.Checked; Config.Save(); };

        CheckBox chkPory = new CheckBox {
            Text = "Activar Enjambre 3D (Porygon + Porygon2 + Porygon-Z)",
            Location = new Point(24, 478),
            AutoSize = true,
            Checked = Config.ShowPorygon
        };
        chkPory.CheckedChanged += (s, ev) => { Config.ShowPorygon = chkPory.Checked; Config.Save(); };

        Button btnClose = new Button {
            Text = "Guardar y Cerrar Panel",
            Location = new Point(24, 518),
            Size = new Size(448, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 180, 216),
            ForeColor = Color.FromArgb(10, 15, 28),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, ev) => { Config.Save(); configForm.Close(); };

        configForm.Controls.Add(chkTrain);
        configForm.Controls.Add(chkFollowMouse);
        configForm.Controls.Add(chkTel);
        configForm.Controls.Add(chkRad);
        configForm.Controls.Add(chkPory);
        configForm.Controls.Add(btnClose);

        configForm.Show();
    }

    public static void RenderModelsContactSheet(string outPath) {
        Porygon3DAvatar av = new Porygon3DAvatar(1920, 1080);
        av.TriggerTrainingWave(1920, 1080);
        if (av.Packets.Count > 0) {
            av.Packets[0].X = 1120;
            av.Packets[0].Y = 360;
            av.Packets[0].IsHovered = true;
            av.HoveredPacket = av.Packets[0];
        }
        using (Bitmap bmp = new Bitmap(1920, 1080))
        using (Graphics g = Graphics.FromImage(bmp)) {
            g.Clear(Color.FromArgb(7, 10, 22));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            Font fSmall = new Font("Consolas", 8f, FontStyle.Regular);
            av.Render(g, 110f, Color.FromArgb(0, 240, 255), Color.FromArgb(236, 72, 153), fSmall);
            bmp.Save(outPath, ImageFormat.Png);
        }
    }

    [STAThread]
    public static void Main(string[] args) {
        if (args != null && args.Length > 0 && args[0] == "--preview-models") {
            string p = Path.Combine(Path.GetTempPath(), "verify_3_swarm.png");
            RenderModelsContactSheet(p);
            return;
        }
        bool wantConfig = (args != null && args.Length > 0 && args[0].Contains("config"));
        bool createdNew;
        EventWaitHandle evt = new EventWaitHandle(false, EventResetMode.AutoReset, "Global\\AntigravityHackerWallpaperConfigEvent", out createdNew);

        bool mutexNew;
        using (Mutex mutex = new Mutex(true, "Global\\AntigravityHackerWallpaperMutex", out mutexNew)) {
            if (!mutexNew) {
                evt.Set();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HackerWallpaperForm(evt, wantConfig));
        }
    }
}

