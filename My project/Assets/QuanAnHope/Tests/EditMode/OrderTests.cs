using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-009: khach goi mot mon cu the. Nguon: constitution v1.3 muc 6.7 (tin nhan nguoi dung 14/11:
    /// "khach hang phai order 1 mon an cu the"; tra loi: goi mon nau duoc, sai mon thi coi nhu gian).
    /// </summary>
    public class OrderTests
    {
        const int LastDayOfRound1 = 3;

        static GameSession NewSession(int seed)
        {
            return new GameSession(TestData.Balance, TestData.Recipes, new SeededRandom(seed), LastDayOfRound1);
        }

        static List<string> StockPairs(GameSession s)
        {
            var mains = s.Stock.Items.Where(i => i.kind == IngredientKind.Main).Select(i => i.typeId).Distinct();
            var sides = s.Stock.Items.Where(i => i.kind == IngredientKind.Side).Select(i => i.typeId).Distinct().ToList();
            var book = new HashSet<string>(TestData.Recipes.recipes.Select(r => r.id));
            return mains.SelectMany(m => sides.Select(p => m + "__" + p)).Where(book.Contains).ToList();
        }

        // ---------- AC-1: goi mon lam duoc tu kho ----------

        [Test]
        public void AC1_MotThungChinhMotThungPhu_KhachGoiDungMonGhepDuoc()
        {
            GameSession s = NewSession(11);
            PullResult pull;
            Assert.IsTrue(s.PullCrates(out pull));
            Assert.IsTrue(s.OpenShop());

            Customer c = s.CurrentCustomer;
            string expected = pull.main.typeId + "__" + pull.side.typeId;
            Assert.AreEqual(expected, c.orderId);
            Assert.AreEqual(s.Lookup.Find(pull.main.typeId, pull.side.typeId).name, c.orderName);
        }

        [Test]
        public void AC1_NhieuThung_MonGoiLuonGhepDuocTuKho()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                GameSession s = NewSession(seed);
                PullResult pull;
                while (s.PullCrates(out pull)) { }
                List<string> pairs = StockPairs(s);
                s.OpenShop();
                CollectionAssert.Contains(pairs, s.CurrentCustomer.orderId, "seed " + seed);
            }
        }

        // ---------- AC-2: mon tren quay cho cung tinh la lam duoc ----------

        [Test]
        public void AC2_KhoTrong_ConDiaTrenQuayCho_KhachGoiMonTrenQuay()
        {
            GameSession s = NewSession(12);
            Assert.IsTrue(NaiveBot.CookOne(s));
            foreach (Ingredient i in s.Stock.Items.ToList()) s.Stock.Remove(i);
            Assert.AreEqual(0, s.Stock.Count);

            s.OpenShop();
            Assert.AreEqual(s.Counter.Dishes[0].recipeId, s.CurrentCustomer.orderId);
        }

        // ---------- AC-3: khong lam duoc mon nao thi goi mon trong sach ----------

        [Test]
        public void AC3_KhongCoGiLamDuoc_GoiMonDaMoKhoaTrongSach()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                GameSession s = NewSession(seed);
                s.OpenShop();
                Recipe r = TestData.Recipes.recipes.FirstOrDefault(x => x.id == s.CurrentCustomer.orderId);
                Assert.IsNotNull(r, "mon goi phai co cong thuc");
                Assert.AreEqual("chinh", r.group);
                Assert.That(r.unlockRound, Is.InRange(1, s.Round), "chi goi mon da mo khoa o vong hien tai");
            }
        }

        // ---------- AC-4: chon deu giua cac mon lam duoc (Dieu 3: moi xac suat co test) ----------

        [Test]
        public void AC4_ChonDeuGiuaCacMonLamDuoc()
        {
            GameSession s = NewSession(13);
            PullResult pull;
            while (s.PullCrates(out pull)) { }
            List<string> makeable = s.MakeableRecipes().Select(r => r.id).ToList();
            CollectionAssert.AreEquivalent(StockPairs(s), makeable);
            Assert.Greater(makeable.Count, 1, "seed nay can nhieu mon de kiem");

            const int N = 30000;
            var count = makeable.ToDictionary(id => id, id => 0);
            for (int i = 0; i < N; i++) count[s.ChooseOrder().id]++;
            double expected = (double)N / makeable.Count;
            foreach (KeyValuePair<string, int> kv in count)
                Assert.AreEqual(expected, kv.Value, expected * 0.06, kv.Key);
        }

        // ---------- AC-5: dung mon tra tien nhu cu; sai mon coi nhu khach gian ----------

        [Test]
        public void AC5_DungMon_TraTheoBangTinhTien()
        {
            GameSession s = NewSession(14);
            Assert.IsTrue(NaiveBot.CookOne(s));
            s.OpenShop();
            Assert.AreEqual(s.Counter.Dishes[0].recipeId, s.CurrentCustomer.orderId);
            int expected = Payment.Settle(TestData.Balance, s.PriceOf(s.Counter.Dishes[0].recipeId), s.CurrentCustomer.easiness, s.Counter.Dishes[0].tier).amount;

            PaymentResult paid = s.Serve(0);

            Assert.IsFalse(paid.wrongOrder);
            Assert.AreEqual(expected, paid.amount);
        }

        [Test]
        public void AC5_SaiMon_KhachNhanNhungGian_QuanDen30PhanTramGiaGoc()
        {
            GameSession s = NewSession(15);
            Assert.IsTrue(NaiveBot.CookOne(s));
            s.OpenShop();
            Customer first = s.CurrentCustomer;
            Dish dish = s.Counter.Dishes[0];
            dish.recipeId = dish.recipeId == "trung__ca_chua" ? "heo__hanh_tay" : "trung__ca_chua";   // dia khac mon khach goi
            int money = s.Money;

            PaymentResult paid = s.Serve(0);

            Assert.IsTrue(paid.wrongOrder);
            Assert.AreEqual(PaymentOutcome.Angry, paid.outcome);
            int penalty = (int)System.Math.Round(0.3 * s.PriceOf(first.orderId), System.MidpointRounding.AwayFromZero);
            Assert.AreEqual(-penalty, paid.amount, "30% gia niem yet mon khach goi (constitution 2.0)");
            Assert.AreEqual(money - penalty, s.Money);
            Assert.AreEqual(0, s.Counter.Dishes.Count, "khach nhan dia");
            Assert.AreEqual(1, s.ServedToday);
            s.Tick(TestData.NextSeat);
            Assert.IsNotNull(s.CurrentCustomer, "khach truoc an xong, khach sau vao sau 3 giay (constitution 1.5 muc 6.2)");
            Assert.AreNotSame(first, s.CurrentCustomer);
        }

        // ---------- AC-6: khach moi goi lai theo kho luc vao ----------

        [Test]
        public void AC6_MoiKhachMoiGoiMon()
        {
            GameSession s = NewSession(16);
            PullResult pull;
            while (s.PullCrates(out pull)) { }
            s.OpenShop();
            for (int i = 0; i < 5 && s.Phase == GamePhase.Open; i++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(s.CurrentCustomer.orderId));
                CollectionAssert.Contains(s.MakeableRecipes().Select(r => r.id).ToList(), s.CurrentCustomer.orderId);
                Assert.IsTrue(NaiveBot.CookFor(s, s.CurrentCustomer.orderId), "bot nau duoc mon khach goi");
                int index = s.Counter.Dishes.ToList().FindIndex(d => d.recipeId == s.CurrentCustomer.orderId);
                Assert.IsFalse(s.Serve(index).wrongOrder);
                s.Tick(TestData.NextSeat);
            }
        }
    }
}
