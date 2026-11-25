using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>Sprint 10 (docs/sprint-10/impl-plan.md): F-091 AC-3 luu / tai giua van, F-090 AC-3 so constitution khop du lieu.</summary>
    public class Sprint10BuildTests
    {
        static Balance B { get { return TestData.Balance; } }

        static string Fingerprint(GameSession s)
        {
            return s.Day + "|" + s.Money + "|" + s.Wallet.Count + "|" + string.Join(",", s.Relics.Owned.Select(r => r.id)) + "|"
                + string.Join(",", s.Shop.Offers.Select(r => r != null ? r.id : "-")) + "|" + string.Join(",", s.Fridge.Dishes.Select(d => d.recipeId + ":" + d.tier + ":" + d.daysInFridge))
                + "|" + s.Items.Sauces + "/" + s.Items.SaucesBought + "|" + s.Items.BlackTickets + "/" + s.Items.BlackBought + "|" + s.Items.HasMachine + "|" + s.Boards.Count
                + "|" + ((SeededRandom)s.Rng).Draws;
        }

        /// <summary>F-091 AC-3: 50 lan luu roi tai (qua JSON nhu SaveStore) o cac ngay ngau nhien cua nhieu van: khong hong, choi tiep ra y nhu cu.</summary>
        [Test]
        public void F091_AC3_LuuTai50LanNgauNhien_KhongHongBanLuu()
        {
            var pick = new System.Random(2026);
            int checks = 0, games = 0;
            BotStyle[] styles = { BotStyle.Smart, BotStyle.NoCombo, BotStyle.Naive };
            while (checks < 50)
            {
                var s = new GameSession(B, TestData.Recipes, new SeededRandom(500 + games), GameSession.LastDayOfGame(B));
                BotStyle style = styles[games % styles.Length];
                games++;
                int day = 1;
                while (checks < 50 && s.Phase == GamePhase.Prep)
                {
                    day += 1 + pick.Next(0, 4);
                    StrategyBot.PlayUntil(s, style, day);
                    if (s.Phase != GamePhase.Prep) break;
                    if (s.Items.IsOpen && s.Money > s.Items.BoardPrice + 3000 && s.Boards.Count < B.board.max) s.BuyBoard();
                    SaveData save = s.Snapshot();
                    Assert.IsNotNull(save, "luu duoc o dau ngay " + s.Day);
                    string json = JsonUtility.ToJson(save, true);
                    GameSession t = GameSession.FromSave(B, TestData.Recipes, JsonUtility.FromJson<SaveData>(json));
                    Assert.AreEqual(Fingerprint(s), Fingerprint(t), "tai lai ngay " + s.Day + " van " + games);
                    // choi tiep ca hai ban mot ngay: ra y nhu nhau (constitution muc 2.1 "tai lai thi ngay do ra y nhu cu")
                    GameSession a = s, b = t;
                    StrategyBot.PlayUntil(a, style, a.Day + 1);
                    StrategyBot.PlayUntil(b, style, b.Day + 1);
                    Assert.AreEqual(Fingerprint(a), Fingerprint(b), "ngay sau khi tai lai khac ban goc (ngay " + s.Day + ")");
                    s = b;
                    day = s.Day;
                    checks++;
                }
            }
            Assert.AreEqual(50, checks);
        }
    }
}
