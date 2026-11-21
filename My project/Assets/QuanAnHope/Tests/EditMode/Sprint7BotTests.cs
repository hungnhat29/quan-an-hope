using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.EditorTools;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-060 (docs/sprint-7/impl-plan.md): do va chinh can bang tren luat that bang bot C#.
    /// Test Explicit <see cref="F060_QuetPhuongAn"/>: mot lan mo Unity chay bot cho nhieu bo so (bien moi truong BOT_SWEEP),
    /// ghi balance/bot_sweep.md. Dung de chon so, khong phai luat game; so chon xong ghi vao balance/make_next.py (Dieu 2).
    /// BOT_SWEEP = "ten|d22=36000|d15=11000|price=1.1|tickets=6|first=5|boss=11500;ten2|..."
    /// </summary>
    public class Sprint7BotTests
    {
        static Balance Fresh()
        {
            return BalanceImporter.ParseBalance(File.ReadAllText(BalanceImporter.BalanceJsonPath), File.ReadAllText(BalanceImporter.RelicsJsonPath));
        }

        /// <summary>Ap mot phuong an len ban sao du lieu (chi doi so, khong doi luat).</summary>
        public static Balance Variant(string spec, out string name)
        {
            Balance b = Fresh();
            string[] parts = spec.Split('|');
            name = parts[0].Trim();
            for (int i = 1; i < parts.Length; i++)
            {
                string[] kv = parts[i].Split('=');
                if (kv.Length != 2) continue;
                string k = kv[0].Trim();
                double v = double.Parse(kv[1].Trim(), CultureInfo.InvariantCulture);
                if (k.StartsWith("d"))
                {
                    int day = int.Parse(k.Substring(1), CultureInfo.InvariantCulture);
                    foreach (DebtEntry d in b.debts) if (d.day == day) d.amount = (int)v;
                }
                else if (k == "price")
                    for (int r = 0; r < b.dish.basePriceByRound.Length; r++) b.dish.basePriceByRound[r] = (int)Math.Round(b.dish.basePriceByRound[r] * v);
                else if (k == "tickets") b.ticketsPerDay = (int)v;
                else if (k == "first") b.start.firstDayTickets = (int)v;
                else if (k == "boss") b.bossBonus = (int)v;
                else throw new ArgumentException("khong biet khoa " + k);
            }
            return b;
        }

        public class SweepRow
        {
            public string name;
            public BotStats naive, nocombo, smart;
            public readonly Dictionary<string, int[]> setWins = new Dictionary<string, int[]>(); // bo -> [van du 3 mon, thang]
        }

        public static SweepRow Run(Balance b, string name, int games)
        {
            var row = new SweepRow { name = name, naive = new BotStats(BotStyle.Naive), nocombo = new BotStats(BotStyle.NoCombo), smart = new BotStats(BotStyle.Smart) };
            RecipeBook recipes = TestData.Recipes;
            foreach (BotStats st in new[] { row.naive, row.nocombo, row.smart })
                for (int g = 0; g < games; g++)
                {
                    var s = new GameSession(b, recipes, new SeededRandom(g), GameSession.LastDayOfGame(b));
                    StrategyBot.PlayToEnd(s, st.Style);
                    BotRun r = StrategyBot.LastRun;
                    st.Add(s, r);
                    if (st.Style == BotStyle.Smart && r.maxSetCount >= 3 && r.topSet != null)
                    {
                        int[] w;
                        if (!row.setWins.TryGetValue(r.topSet, out w)) row.setWins[r.topSet] = w = new int[2];
                        w[0]++;
                        if (r.won) w[1]++;
                    }
                }
            return row;
        }

        static string P(double v) { return (100 * v).ToString("0", CultureInfo.InvariantCulture) + "%"; }

        public static string Table(List<SweepRow> rows, int games, double seconds)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Quet phuong an can bang (F-060) — " + games + " van moi kieu, " + seconds.ToString("0", CultureInfo.InvariantCulture) + " giay");
            sb.AppendLine();
            sb.AppendLine("Moc Dieu 1: khong doc chi so di >= 60% chang; gom >= 3 mon mot bo thang >= 70%; khong gom bo thap hon ro. AC-5: tung bo 70-97%.");
            sb.AppendLine();
            sb.AppendLine("| Phuong an | Naive chang | Naive qua d9 | Gom bo: thang (du 3 mon) | Smart: ti le du 3 mon | Smart thang | Khong gom bo thang | Tung bo (du 3 mon: thang) | Khach giam de tinh / ngay (smart) |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (SweepRow r in rows)
            {
                var sets = new List<string>();
                foreach (KeyValuePair<string, int[]> kv in r.setWins)
                    sets.Add(RelicKeys.SetName(kv.Key) + " " + kv.Value[0] + ": " + P((double)kv.Value[1] / Math.Max(1, kv.Value[0])));
                sb.AppendLine("| " + r.name + " | " + P(r.naive.Progress) + " | " + P(r.naive.Pass(1)) + " | " + P(r.smart.WinGivenCombo) + " | " + P(r.smart.Combo)
                    + " | " + P(r.smart.Win) + " | " + P(r.nocombo.Win) + " | " + string.Join(", ", sets.ToArray())
                    + " | " + r.smart.ImpatientPerDay.ToString("0.0", CultureInfo.InvariantCulture) + " |");
            }
            return sb.ToString();
        }

        [Test, Explicit("chay bang balance/run_sweep.ps1")]
        public void F060_QuetPhuongAn()
        {
            int games = 1000;
            int parsed;
            string env = Environment.GetEnvironmentVariable("BOT_GAMES");
            if (!string.IsNullOrEmpty(env) && int.TryParse(env, out parsed)) games = parsed;
            string sweep = Environment.GetEnvironmentVariable("BOT_SWEEP");
            if (string.IsNullOrEmpty(sweep)) sweep = "hien tai";
            var watch = Stopwatch.StartNew();
            var rows = new List<SweepRow>();
            foreach (string spec in sweep.Split(';'))
            {
                if (spec.Trim().Length == 0) continue;
                string name;
                Balance b = Variant(spec, out name);
                rows.Add(Run(b, name, games));
            }
            watch.Stop();
            string dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "balance"));
            string table = Table(rows, games, watch.Elapsed.TotalSeconds);
            File.WriteAllText(Path.Combine(dir, "bot_sweep.md"), table);
            UnityEngine.Debug.Log(table);
        }

        // ---------- test thuong (nhanh) ----------

        [Test]
        public void F060_AC6_DemKhachBiGiamDeTinh_VongCoTinhGio()
        {
            Balance b = Fresh();
            SweepRow r = Run(b, "nho", 20);
            Assert.Greater(r.smart.ImpatientPerDay, 0.0, "vong 2-4 co khach bi giam de tinh vi cho");
            Assert.Less(r.smart.ImpatientPerDay, 30.0);
        }

        [Test]
        public void F060_PhuongAn_ChiDoiSo_KhongDoiDuLieuGoc()
        {
            string name;
            Balance v = Variant("thu|d22=40000|price=1.5|tickets=7|first=4|boss=9000", out name);
            Assert.AreEqual("thu", name);
            Assert.AreEqual(40000, v.DebtDueOn(22));
            Assert.AreEqual(7, v.ticketsPerDay);
            Assert.AreEqual(4, v.start.firstDayTickets);
            Assert.AreEqual(9000, v.bossBonus);
            Assert.AreEqual((int)Math.Round(TestData.Balance.dish.basePriceByRound[0] * 1.5), v.dish.basePriceByRound[0]);
            Assert.AreNotEqual(40000, TestData.Balance.DebtDueOn(22), "du lieu dung chung cua test khong bi doi");
        }

        [Test]
        public void F060_AC2_BotDungGioDo_TuDuLieu()
        {
            Assert.Greater(TestData.Balance.botTiming.goodOverheadSeconds, 0f, "balance.json bot_timing (playtest_overhead.py)");
            BotTiming good = BotTiming.From(TestData.Balance, false), naive = BotTiming.From(TestData.Balance, true);
            Assert.AreEqual(TestData.Balance.botTiming.goodOverheadSeconds, good.overheadSeconds);
            Assert.AreEqual(TestData.Balance.botTiming.naiveOverheadSeconds, naive.overheadSeconds);
            Assert.AreEqual(TestData.Balance.botTiming.fridgeSeconds, good.fridgeSeconds);
        }
    }
}
