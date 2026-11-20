using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sprint 6 (docs/sprint-6/impl-plan.md, constitution 1.5): F-052 luu / tai theo ngay, F-051 loi cot truyen.
    /// </summary>
    public class Sprint6Tests
    {
        static Balance B { get { return TestData.Balance; } }

        /// <summary>Choi bang bot toi dau ngay <paramref name="day"/> (pha Prep), co relic, tu lanh, vat pham neu bot mua.</summary>
        static GameSession PlayTo(int day, int seed)
        {
            var s = new GameSession(B, TestData.Recipes, new SeededRandom(seed), GameSession.LastDayOfGame(B));
            StrategyBot.PlayUntil(s, BotStyle.Smart, day);
            Assert.AreEqual(GamePhase.Prep, s.Phase, "bot phai toi duoc dau ngay " + day);
            return s;
        }

        static SaveData RoundTrip(SaveData d)
        {
            // Game luu bang JsonUtility (SaveStore): kiem dung duong do.
            return JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(d));
        }

        // ---------- F-052 AC-1, AC-4 ----------

        [Test]
        public void F052_AC1_LuuDauNgay_DuCacTruong()
        {
            GameSession s = PlayTo(10, 7);
            SaveData d = s.Snapshot();
            Assert.IsNotNull(d);
            Assert.AreEqual(10, d.day);
            Assert.AreEqual(s.Money, d.money);
            Assert.AreEqual(s.Wallet.Count, d.tickets);
            Assert.AreEqual(s.Items.BlackTickets, d.blackTickets);
            CollectionAssert.AreEqual(s.Relics.Owned.Select(r => r.id).ToArray(), d.relics);
            Assert.AreEqual(s.Fridge.Dishes.Count, d.fridge.Length);
            Assert.AreEqual(s.Items.Sauces, d.sauces);
            Assert.AreEqual(s.Items.HasMachine, d.hasMachine);
            Assert.AreEqual(s.Items.SaucesBought, d.saucesBought);
            Assert.AreEqual(s.Items.BlackBought, d.blackBought);
            Assert.AreEqual(7, d.seed);
            Assert.Greater(d.draws, 0);
        }

        [Test]
        public void F052_AC1_GiuaNgay_KhongLuu()
        {
            GameSession s = TestData.SessionAt(1);
            s.OpenShop();
            Assert.IsNull(s.Snapshot(), "chi luu dau ngay (pha Prep)");
        }

        [Test]
        public void F052_AC4_LuuRoiTai_GiongHetTungTruong([Values(1, 7, 42)] int seed)
        {
            GameSession a = PlayTo(9 + seed % 5, seed);
            SaveData d = RoundTrip(a.Snapshot());
            GameSession b = GameSession.FromSave(B, TestData.Recipes, d);

            Assert.AreEqual(a.Day, b.Day);
            Assert.AreEqual(a.Money, b.Money);
            Assert.AreEqual(a.Phase, b.Phase);
            Assert.AreEqual(a.LastPlayableDay, b.LastPlayableDay);
            Assert.AreEqual(a.Wallet.Count, b.Wallet.Count);
            CollectionAssert.AreEqual(a.Relics.Owned.Select(r => r.id), b.Relics.Owned.Select(r => r.id));
            CollectionAssert.AreEqual(a.Shop.Offers.Select(r => r == null ? null : r.id), b.Shop.Offers.Select(r => r == null ? null : r.id));
            Assert.AreEqual(a.Shop.IsOpen, b.Shop.IsOpen);
            Assert.AreEqual(a.Items.Sauces, b.Items.Sauces);
            Assert.AreEqual(a.Items.SaucesBought, b.Items.SaucesBought);
            Assert.AreEqual(a.Items.BlackTickets, b.Items.BlackTickets);
            Assert.AreEqual(a.Items.BlackBought, b.Items.BlackBought);
            Assert.AreEqual(a.Items.HasMachine, b.Items.HasMachine);
            Assert.AreEqual(a.Items.MachineUsesLeft, b.Items.MachineUsesLeft);
            Assert.AreEqual(a.Fridge.Dishes.Count, b.Fridge.Dishes.Count);
            for (int i = 0; i < a.Fridge.Dishes.Count; i++)
            {
                Dish x = a.Fridge.Dishes[i], y = b.Fridge.Dishes[i];
                Assert.AreEqual(x.recipeId, y.recipeId);
                Assert.AreEqual(x.tier, y.tier);
                Assert.AreEqual(x.daysInFridge, y.daysInFridge);
                Assert.AreEqual(x.decayed, y.decayed);
                Assert.AreEqual(x.machined, y.machined);
            }
            Assert.AreEqual(a.SeatCount, b.SeatCount);
            Assert.AreEqual(a.Relics.Effects.Get(RelicKeys.Fridge), b.Relics.Effects.Get(RelicKeys.Fridge));
            Assert.AreEqual(a.Clock.Elapsed, b.Clock.Elapsed);
        }

        [Test]
        public void F052_AC2_TaiLai_ChoiLaiNgay_RaY_NhuCu()
        {
            GameSession a = PlayTo(6, 11);
            SaveData d = RoundTrip(a.Snapshot());
            GameSession b = GameSession.FromSave(B, TestData.Recipes, d);
            GameSession c = GameSession.FromSave(B, TestData.Recipes, d);

            // Cung mot ngay, cung cach choi: tien cuoi ngay va cac thung gacha phai trung nhau (khong quay lai de doi ket qua).
            NaiveBot.PlayDay(a, BotTiming.Naive);
            NaiveBot.PlayDay(b, BotTiming.Naive);
            NaiveBot.PlayDay(c, BotTiming.Naive);
            Assert.AreEqual(a.Money, b.Money);
            Assert.AreEqual(a.ServedToday, b.ServedToday);
            Assert.AreEqual(b.Money, c.Money, "tai hai lan cung ra mot ket qua");
            Assert.AreEqual(b.IncomeToday, c.IncomeToday);
        }

        [Test]
        public void F052_TaiBanLuuSaiPhienBan_BaoLoi()
        {
            SaveData d = TestData.SessionAt(1).Snapshot();
            d.version = 99;
            Assert.Throws<System.ArgumentException>(() => GameSession.FromSave(B, TestData.Recipes, d));
        }

        [Test]
        public void F052_SeededRandom_AdvanceTo_TiepTucDungDay()
        {
            var a = new SeededRandom(5);
            for (int i = 0; i < 37; i++) a.NextDouble();
            var b = new SeededRandom(5);
            b.AdvanceTo(37);
            for (int i = 0; i < 10; i++) Assert.AreEqual(a.NextDouble(), b.NextDouble());
            Assert.Throws<System.ArgumentOutOfRangeException>(() => b.AdvanceTo(3));
        }

        // ---------- F-051 ----------

        [Test]
        public void F051_AC1_MoDau_KeDu3Y_VaLichNo()
        {
            var intro = StoryScript.Intro(B);
            Assert.That(intro.Count, Is.InRange(4, 6), "4-6 khung tranh (impl plan muc 1)");
            string all = string.Join(" ", intro.Select(p => p.text));
            StringAssert.Contains("đuổi việc", all);
            StringAssert.Contains("chủ nợ", all);
            StringAssert.Contains("quán ăn", all.ToLowerInvariant());
            foreach (DebtEntry debt in B.debts)
                StringAssert.Contains(StoryScript.Money(debt.amount), all, "lich doi no lay tu du lieu");
        }

        /// <summary>Truyen tranh ve san (20/11): mo dau Chuong 1, chuong 2-4 giua van, thang Chuong 5-6; khung theo thu tu doc.</summary>
        [Test]
        public void F051_TruyenTranh_ChuongVaThuTuKhung()
        {
            var intro = StoryScript.Intro(B);
            Assert.IsTrue(intro.All(p => p.comicPage == 1));
            CollectionAssert.AreEqual(Enumerable.Range(0, 5).ToArray(), intro.Select(p => p.comicPanel).ToArray());
            var win = StoryScript.Win(B, 100);
            CollectionAssert.AreEqual(new[] { 5, 5, 5, 5, 5, 6, 6, 6 }, win.Select(p => p.comicPage).ToArray());
            Assert.AreEqual(0, StoryScript.Lose(4, 750, 500)[0].comicPage, "ket thua: tranh khoi (chua co trang ve)");

            // chuong giua van: ngay 2 (bao), ngay mat dien, ngay chu no thu lan 3 (tin don -> danh tieng tro lai)
            Assert.AreEqual(2, StoryScript.ChapterPageFor(B, 2));
            Assert.AreEqual(3, StoryScript.ChapterPageFor(B, B.blackout.day));
            Assert.AreEqual(4, StoryScript.ChapterPageFor(B, B.debts[2].day));
            int chapters = 0;
            for (int d = 1; d <= 22; d++) if (StoryScript.ChapterPageFor(B, d) > 0) chapters++;
            Assert.AreEqual(3, chapters);
            foreach (int d in new[] { 2, B.blackout.day, B.debts[2].day })
            {
                var ch = StoryScript.Chapter(B, d);
                Assert.AreEqual(5, ch.Count);
                CollectionAssert.AreEqual(Enumerable.Range(0, 5).ToArray(), ch.Select(p => p.comicPanel).ToArray());
            }
            StringAssert.Contains("Ngày " + B.blackout.day, StoryScript.Chapter(B, B.blackout.day)[0].text, "so ngay tu du lieu");
        }

        /// <summary>So ngay ve tren tranh (lich no 4 / 9 / 15 / 22, chu no 21, mat dien 8) doi theo du lieu (Dieu 2).</summary>
        [Test]
        public void F051_TruyenTranh_SoNgayTheoDuLieu()
        {
            var m = StoryScript.ComicDayMap(B);
            Assert.AreEqual(B.debts[0].day, m[4]);
            Assert.AreEqual(B.debts[3].day, m[22]);
            Assert.AreEqual(B.debts[2].day - 1, m[14]);
            Assert.AreEqual(B.day.bossDay, m[21]);
            Assert.AreEqual(B.blackout.day, m[8]);
        }

        [Test]
        public void F051_AC2_KetThang_GaConGai()
        {
            string all = string.Join(" ", StoryScript.Win(B, 1234).Select(p => p.text));
            StringAssert.Contains("gả con gái", all);
            StringAssert.Contains("1.234G", all);
        }

        [Test]
        public void F051_AC2_KetThua_GhiRoThieuBaoNhieu()
        {
            string all = string.Join(" ", StoryScript.Lose(15, 10500, 9800).Select(p => p.text));
            StringAssert.Contains("ngày 15", all);
            StringAssert.Contains("10.500G", all);
            StringAssert.Contains("thiếu 700G", all);
        }

        [Test]
        public void F051_AC4_LoiTiengVietDuDau()
        {
            foreach (StoryPanel p in StoryScript.Intro(B).Concat(StoryScript.Win(B, 0)).Concat(StoryScript.Lose(4, 750, 0)))
                Assert.IsTrue(p.text.Any(ch => "ăâđêôơưáàảãạấầẩẫậắằẳẵặéèẻẽẹếềểễệíìỉĩịóòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵ".IndexOf(char.ToLowerInvariant(ch)) >= 0),
                    "loi phai co dau tieng Viet: " + p.text);
        }
    }
}
