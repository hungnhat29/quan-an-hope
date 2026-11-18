using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-045 Bot tren luat that (docs/sprint-5/impl-plan.md F-045, constitution Dieu 1, muc 9).
    /// Bao cao 4.000 van moi kieu: test Explicit <see cref="F045_BaoCao_4000VanMoiKieu"/>, chay bang balance/run_bots.ps1.
    /// </summary>
    public class Sprint5BotTests
    {
        static Balance B { get { return TestData.Balance; } }

        static GameSession NewGame(int seed)
        {
            return new GameSession(B, TestData.Recipes, new SeededRandom(seed), GameSession.LastDayOfGame(B));
        }

        [TestCase(BotStyle.Naive)]
        [TestCase(BotStyle.NoCombo)]
        [TestCase(BotStyle.Smart)]
        public void F045_AC1_MoiKieuBot_ChoiTron22Ngay_KetThucDungMoc(BotStyle style)
        {
            for (int seed = 1; seed <= 25; seed++)
            {
                GameSession s = NewGame(seed);
                GamePhase end = StrategyBot.PlayToEnd(s, style);
                if (end == GamePhase.Cleared)
                {
                    Assert.AreEqual(22, s.Day);
                    Assert.IsTrue(s.IsWon);
                }
                else
                {
                    Assert.AreEqual(GamePhase.Lost, end);
                    CollectionAssert.Contains(new[] { 4, 9, 15, 22 }, s.Day, "chi thua vao ngay thu no");
                }
            }
        }

        [Test]
        public void F045_BotGioi_PhucVuChuNo_NhanThuongKhiCoMonTuyetHao()
        {
            int reached = 0, bonus = 0;
            for (int seed = 1; seed <= 40; seed++)
            {
                GameSession s = NewGame(seed);
                StrategyBot.PlayToEnd(s, BotStyle.Smart);
                if (StrategyBot.LastRun.reachedBossDay) reached++;
                if (StrategyBot.LastRun.bossBonus > 0) bonus++;
            }
            Assert.Greater(reached, 0, "co van toi ngay 21");
            Assert.Greater(bonus, 0, "co van nhan thuong chu no");
        }

        [Test]
        public void F045_AC2_BaoCao_CungDangBangVoiSimBalance()
        {
            var stats = new BotStats(BotStyle.Smart);
            for (int seed = 1; seed <= 10; seed++)
            {
                GameSession s = NewGame(seed);
                StrategyBot.PlayToEnd(s, BotStyle.Smart);
                stats.Add(s, StrategyBot.LastRun);
            }
            string line = stats.Line();
            StringAssert.StartsWith("smart  win=", line);
            StringAssert.Contains("progress=", line);
            StringAssert.Contains("pass(d4,d9,d15,d22)=[", line);
            StringAssert.Contains("combo3+=", line);
            StringAssert.Contains("win|combo=", line);
            Assert.AreEqual(10, stats.Games);
        }

        /// <summary>F-045 AC-1, AC-2: 3 kieu x 4.000 van, ghi balance/bot_report.md. Chay: balance/run_bots.ps1.</summary>
        [Test, Explicit("4.000 van moi kieu, chay bang balance/run_bots.ps1")]
        public void F045_BaoCao_4000VanMoiKieu()
        {
            int games = 4000;
            string env = Environment.GetEnvironmentVariable("BOT_GAMES");
            int parsed;
            if (!string.IsNullOrEmpty(env) && int.TryParse(env, out parsed)) games = parsed;

            var watch = Stopwatch.StartNew();
            var all = new BotStats[3];
            BotStyle[] styles = { BotStyle.Naive, BotStyle.NoCombo, BotStyle.Smart };
            for (int k = 0; k < styles.Length; k++)
            {
                all[k] = new BotStats(styles[k]);
                for (int g = 0; g < games; g++)
                {
                    GameSession s = NewGame(g);
                    StrategyBot.PlayToEnd(s, styles[k]);
                    all[k].Add(s, StrategyBot.LastRun);
                }
            }
            watch.Stop();

            string dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "balance"));
            string simPath = Path.Combine(dir, "sim_report.txt");
            string sim = File.Exists(simPath) ? File.ReadAllText(simPath) : null;
            string report = BotStats.Report(all, games, watch.Elapsed.TotalSeconds, sim);
            string path = Path.Combine(dir, "bot_report.md");
            File.WriteAllText(path, report);
            UnityEngine.Debug.Log(report);
            Assert.Less(watch.Elapsed.TotalMinutes, 10.0, "F-045 AC-1: duoi 10 phut");
        }
    }
}
