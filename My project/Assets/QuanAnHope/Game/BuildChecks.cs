using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QuanAnHope.Core;
using QuanAnHope.Data;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Kiem tra trong ban build (sprint 10, docs/sprint-10/impl-plan.md):
    /// - <c>-botrun N</c> (F-091 AC-1): bot C# choi N van moi kieu nguoi choi tren luat that, dem ngoai le, ghi bot-build.txt roi thoat.
    /// - <c>-benchmark</c> (F-092): dung ngay 15 (vong 4, 3 khach, nhieu thung), lam lan luot nhet ve / khach vao / lat thuc don,
    ///   do khung hinh + khung khung > 50 ms + thoi gian mo game toi menu, ghi benchmark.txt roi thoat.
    /// - <c>-out &lt;thu muc&gt;</c>: noi ghi ket qua (mac dinh Application.persistentDataPath).
    /// Khong co tham so thi khong lam gi (nguoi choi binh thuong).
    /// </summary>
    public static class BuildChecks
    {
        public static string OutFolder { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            string[] args = Environment.GetCommandLineArgs();
            int bots = 0;
            bool bench = false;
            OutFolder = Application.persistentDataPath;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-botrun" && i + 1 < args.Length) int.TryParse(args[i + 1], out bots);
                if (args[i] == "-benchmark") bench = true;
                if (args[i] == "-out" && i + 1 < args.Length) OutFolder = args[i + 1];
            }
            if (bots <= 0 && !bench) return;
            Directory.CreateDirectory(OutFolder);
            var go = new GameObject("KiemTraBanBuild");
            UnityEngine.Object.DontDestroyOnLoad(go);
            if (bots > 0) go.AddComponent<BotRunner>().Games = bots;
            else go.AddComponent<BenchmarkRunner>();
        }
    }

    /// <summary>F-091 AC-1: bot choi tron van trong ban build, khong duoc co ngoai le nao.</summary>
    public class BotRunner : MonoBehaviour
    {
        public int Games = 200;
        readonly List<string> errors = new List<string>();

        void OnEnable() { Application.logMessageReceived += OnLog; }
        void OnDisable() { Application.logMessageReceived -= OnLog; }

        void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) errors.Add(type + ": " + message);
        }

        IEnumerator Start()
        {
            yield return null;
            var data = Resources.Load<BalanceData>("BalanceData");
            var sb = new StringBuilder();
            sb.AppendLine("bot-build " + DateTime.Now.ToString("s") + " · phien ban " + Application.version + " · " + Application.platform);
            int total = 0, exceptions = 0, full = 0;
            float t0 = Time.realtimeSinceStartup;
            foreach (BotStyle style in new[] { BotStyle.Naive, BotStyle.NoCombo, BotStyle.Smart })
            {
                int wins = 0, reached = 0;
                for (int i = 0; i < Games; i++)
                {
                    total++;
                    try
                    {
                        var s = new GameSession(data.balance, data.recipes, new SeededRandom(1000 + i), GameSession.LastDayOfGame(data.balance));
                        GamePhase end = StrategyBot.PlayToEnd(s, style);
                        if (end == GamePhase.Cleared && s.IsWon) wins++;
                        if (end == GamePhase.Cleared || end == GamePhase.Lost) full++;
                        if (s.Day >= GameSession.LastDayOfGame(data.balance)) reached++;
                    }
                    catch (Exception e)
                    {
                        exceptions++;
                        errors.Add("Exception (" + style + " van " + i + "): " + e);
                    }
                    if (i % 20 == 19) yield return null;
                }
                sb.AppendLine(style + ": " + Games + " van, thang " + wins + ", toi ngay 21 " + reached);
            }
            sb.AppendLine("tong " + total + " van, ket thuc tron van (thang / thua) " + full + ", ngoai le " + exceptions
                + ", loi trong log " + errors.Count + ", " + (Time.realtimeSinceStartup - t0).ToString("0.0") + " giay");
            foreach (string e in errors) sb.AppendLine("  " + e);
            File.WriteAllText(Path.Combine(BuildChecks.OutFolder, "bot-build.txt"), sb.ToString(), Encoding.UTF8);
            Application.Quit(errors.Count == 0 && exceptions == 0 ? 0 : 1);
        }
    }

    /// <summary>
    /// F-092: do khung hinh ngay dong nhat (vong 4, 3 khach, nhieu thung) va cac thao tac de khung (nhet ve, khach vao, lat thuc don).
    /// </summary>
    public class BenchmarkRunner : MonoBehaviour
    {
        const float Seconds = 30f;
        readonly List<float> frames = new List<float>();
        readonly List<string> spikes = new List<string>();
        string lastAction = "";
        float menuAt = -1f, warmAt = -1f;

        IEnumerator Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = true;   // cua so mat focus van chay (khong treo)
            Application.targetFrameRate = -1;
            GameController g = null;
            while (g == null || g.Mode != GameMode.Menu) { g = FindAnyObjectByType<GameController>(); yield return null; }
            menuAt = Time.realtimeSinceStartup;
            // cho lam nong icon va mat (nhu nguoi choi o menu vai giay)
            for (float w = 0f; w < 4f || (!g.IconsReady && w < 30f); w += Time.unscaledDeltaTime) yield return null;
            warmAt = Time.realtimeSinceStartup;

            g.NewGame(20261210);
            GameSession s = g.Session;
            s.DebugJumpToDay(15);
            s.DebugAddMoney(20000);
            s.DebugAddTickets(12);
            g.Tutorial.Stop();
            yield return null;
            for (int i = 0; i < 10; i++) { lastAction = "nhet ve"; Timed("nhet ve", () => g.Pull()); yield return Measure(0.3f); }
            lastAction = "mo quan";
            g.ToggleShop();
            float end = Time.realtimeSinceStartup + Seconds;
            float next = 0f;
            int step = 0;
            while (Time.realtimeSinceStartup < end)
            {
                // giu khach ngoi (do khung hinh luc 3 khach, khong de khach bo di)
                foreach (Customer c in s.Seats) if (c != null && !c.isBoss) c.waitedSeconds = 0f;
                if (Time.realtimeSinceStartup >= next)
                {
                    next = Time.realtimeSinceStartup + 2.5f;
                    switch (step++ % 4)
                    {
                        case 0: lastAction = "nhet ve"; Timed("nhet ve", () => g.Pull()); break;
                        case 1: lastAction = "mo Tab thuc don"; Timed("mo Tab", () => g.OpenTab(TabMenu.Page.Menu)); break;
                        case 2:
                            lastAction = "lat thuc don";
                            if (g.Tab != null && g.Tab.Shown && g.Tab.MenuButtons.Count > 0)
                                foreach (string id in g.Tab.MenuButtons.Keys) { if (id != g.Tab.MenuRecipe) { g.Tab.OpenRecipe(id); break; } }
                            break;
                        default: lastAction = "dong Tab"; g.CloseTab(); g.ClockScale = 3f; break;
                    }
                }
                yield return Measure(0f);
            }
            g.ClockScale = 1f;
            Write(g);
            Application.Quit(0);
        }

        readonly List<string> timed = new List<string>();

        void Timed(string what, Action act)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            act();
            float ms = (float)sw.Elapsed.TotalMilliseconds;
            if (ms > 10f) timed.Add(what + " " + ms.ToString("0") + " ms (dong bo)");
        }

        string Events(GameController g)
        {
            GameSession s = g.Session;
            string sig = s.CustomersToday + "/" + s.ServedToday + "/" + s.Stock.Count + "/" + s.LeftToday;
            int eating = 0;
            foreach (Customer c in s.Eaters) if (c != null) eating++;
            sig += "/" + eating;
            string ev = "";
            if (lastSig != null && sig != lastSig)
            {
                string[] a = lastSig.Split('/'), b = sig.Split('/');
                if (a[0] != b[0]) ev += " khach vao";
                if (a[2] != b[2]) ev += " thung";
                if (a[4] != b[4]) ev += " khach an / roi ghe";
            }
            lastSig = sig;
            return ev;
        }

        string lastSig;
        int lastGc;

        IEnumerator Measure(float seconds)
        {
            GameController g = FindAnyObjectByType<GameController>();
            float until = Time.realtimeSinceStartup + seconds;
            do
            {
                yield return null;
                float dt = Time.unscaledDeltaTime;
                frames.Add(dt);
                string ev = g != null ? Events(g) : "";
                int gc = GC.CollectionCount(0);
                if (gc != lastGc) { ev += " GC"; lastGc = gc; }
                ev += " t=" + (Time.realtimeSinceStartup - menuAt).ToString("0.0");
                if (dt > 0.05f) spikes.Add((dt * 1000f).ToString("0") + " ms sau \"" + lastAction + "\"" + ev);
                lastAction = "";
            } while (Time.realtimeSinceStartup < until);
        }

        void Write(GameController g)
        {
            frames.Sort();
            float avg = 0f;
            foreach (float f in frames) avg += f;
            avg /= Mathf.Max(1, frames.Count);
            float p99 = frames.Count > 0 ? frames[Mathf.Min(frames.Count - 1, (int)(frames.Count * 0.99f))] : 0f;
            var sb = new StringBuilder();
            sb.AppendLine("benchmark " + DateTime.Now.ToString("s") + " · phien ban " + Application.version);
            sb.AppendLine("man hinh " + Screen.width + "x" + Screen.height + " · " + Screen.fullScreenMode + " · GPU " + SystemInfo.graphicsDeviceName);
            sb.AppendLine("mo game toi menu: " + menuAt.ToString("0.00") + " giay · lam nong xong: " + warmAt.ToString("0.00") + " giay");
            sb.AppendLine("ngay " + g.Session.Day + " · khach dang ngoi " + CountSeated(g.Session) + " · thung " + g.Session.Stock.Count + " mon");
            sb.AppendLine("khung hinh: " + frames.Count + " · trung binh " + (1f / Mathf.Max(1e-4f, avg)).ToString("0") + " FPS · p99 "
                + (p99 * 1000f).ToString("0.0") + " ms · cham nhat " + (frames.Count > 0 ? frames[frames.Count - 1] * 1000f : 0f).ToString("0.0") + " ms");
            sb.AppendLine("khung > 50 ms: " + spikes.Count);
            foreach (string t in timed) sb.AppendLine("  [do] " + t);
            foreach (string sp in spikes) sb.AppendLine("  " + sp);
            File.WriteAllText(Path.Combine(BuildChecks.OutFolder, "benchmark.txt"), sb.ToString(), Encoding.UTF8);
        }

        static int CountSeated(GameSession s)
        {
            int n = 0;
            foreach (Customer c in s.Seats) if (c != null) n++;
            return n;
        }
    }
}
