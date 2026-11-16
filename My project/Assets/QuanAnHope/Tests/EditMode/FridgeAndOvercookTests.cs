using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-032 Tu lanh, F-033 Qua lua. Nguon: constitution 1.4 muc 5.1, 5.7, 5.9, 6.7, 7.3;
    /// docs/sprint-4/impl-plan.md muc 2 (F-032 AC 1-4, F-033 AC 1-2).
    /// </summary>
    public class FridgeAndOvercookTests
    {
        static Balance B { get { return TestData.Balance; } }
        const string Recipe1 = "trung__ca_chua";
        const string Recipe2 = "heo__hanh_tay";
        const string Recipe3 = "dau_phu__rau_muong";

        static void EndDay(GameSession s)
        {
            if (s.Phase == GamePhase.Prep) s.OpenShop();
            s.CloseEarly();
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
        }

        // ---------- F-032 AC-1: tu 2 o ----------

        [Test]
        public void F032_AC1_Tu2O_CatTuQuayCho_DayThiKhongNhan()
        {
            GameSession s = TestData.SessionAt(4);
            Assert.AreEqual(2, s.Fridge.Capacity);
            Dish a = TestData.DishOf(Recipe1, 3), b = TestData.DishOf(Recipe2, 2), c = TestData.DishOf(Recipe3, 1);
            s.Counter.TryPlace(a); s.Counter.TryPlace(b); s.Counter.TryPlace(c);
            Assert.IsTrue(s.StoreInFridge(a));
            Assert.IsTrue(s.StoreInFridge(b));
            Assert.IsFalse(s.StoreInFridge(c), "tu day 2/2");
            CollectionAssert.AreEqual(new[] { a, b }, s.Fridge.Dishes.ToArray());
            CollectionAssert.AreEqual(new[] { c }, s.Counter.Dishes.ToArray());
        }

        [Test]
        public void F032_AC1_DiaKhongTrenQuay_KhongCatDuoc()
        {
            GameSession s = TestData.SessionAt(4);
            Assert.IsFalse(s.StoreInFridge(TestData.DishOf(Recipe1, 3)));
            Assert.AreEqual(0, s.Fridge.Dishes.Count);
        }

        // ---------- F-032 AC-2: cuoi ngay ----------

        [Test]
        public void F032_AC2_CuoiNgay_DiaTrenQuayVaoTuNeuConO_ConLaiBiBo()
        {
            GameSession s = TestData.SessionAt(4);
            Dish keep = TestData.DishOf(Recipe1, 4);
            s.Counter.TryPlace(keep);
            Assert.IsTrue(s.StoreInFridge(keep));
            Dish a = TestData.DishOf(Recipe2, 2), b = TestData.DishOf(Recipe3, 3);
            s.Counter.TryPlace(a); s.Counter.TryPlace(b);
            EndDay(s);
            Assert.AreEqual(2, s.Fridge.Dishes.Count);
            Assert.Contains(keep, s.Fridge.Dishes.ToList());
            Assert.Contains(a, s.Fridge.Dishes.ToList(), "o con lai nhan dia dau tien tren quay");
            Assert.AreEqual(0, s.Counter.Dishes.Count);
            s.StartNextDay();
            Assert.AreEqual(2, s.Fridge.Dishes.Count, "tu lanh giu qua ngay");
        }

        // ---------- F-032 AC-3: qua 3 ngay tut 1 bac ----------

        [Test]
        public void F032_AC3_QuaBaNgay_Tut1Bac_ChiMotLan()
        {
            GameSession s = TestData.SessionAt(4, 100000);
            Dish d = TestData.DishOf(Recipe1, 4);
            s.Counter.TryPlace(d);
            s.StoreInFridge(d);
            for (int i = 0; i < B.fridge.decayDays; i++)
            {
                EndDay(s);
                s.StartNextDay();
                Assert.AreEqual(4, d.tier, "chua qua " + B.fridge.decayDays + " ngay");
            }
            EndDay(s);
            s.StartNextDay();
            Assert.AreEqual(3, d.tier, "qua 3 ngay: tut 1 bac");
            Assert.AreEqual(GamePhase.Prep, s.Phase);
        }

        [Test]
        public void F032_AC3_TuLanh2Cua_4O_KhongTut()
        {
            GameSession s = TestData.SessionAt(4, 100000);
            Assert.IsTrue(s.DebugGiveRelic("F1"));
            Assert.AreEqual(4, s.Fridge.Capacity);
            Dish d = TestData.DishOf(Recipe1, 4);
            s.Counter.TryPlace(d);
            s.StoreInFridge(d);
            for (int i = 0; i < 4; i++) { EndDay(s); s.StartNextDay(); }
            Assert.AreEqual(4, d.tier);
            CollectionAssert.DoesNotContain(RelicKeys.NotYetActive, RelicKeys.Fridge);
            CollectionAssert.DoesNotContain(RelicKeys.NotYetActive, RelicKeys.NoDecay);
        }

        // ---------- F-032 AC-4: mon trong tu la "lam duoc" ----------

        [Test]
        public void F032_AC4_MonTrongTu_LaMonLamDuoc_VaPhucVuDuoc()
        {
            GameSession s = TestData.SessionAt(4, 0, 11);
            Dish d = TestData.DishOf("bo__khoai_tay", 3);
            s.Counter.TryPlace(d);
            s.StoreInFridge(d);
            CollectionAssert.AreEqual(new[] { "bo__khoai_tay" }, s.MakeableRecipes().Select(r => r.id).ToArray());
            s.OpenShop();
            Assert.AreEqual("bo__khoai_tay", s.Seats[0].orderId, "chi co mot mon lam duoc: khach goi mon trong tu");
            PaymentResult paid = s.ServeSeat(0, d);
            Assert.IsNotNull(paid);
            Assert.IsFalse(paid.wrongOrder);
            Assert.AreEqual(0, s.Fridge.Dishes.Count);
        }

        // ---------- F-033: qua lua ----------

        static Stove FilledStove(GameSession s, out Recipe recipe)
        {
            // chinh bac 4 + phu bac 2: b = 0,6x4 + 0,4x2 + 0,8 = 4,0 -> chac chan Tuyet hao
            Ingredient main = TestData.Main("heo", 4), side = TestData.Side("ca_chua", 2);
            recipe = s.Lookup.Find("heo", "ca_chua");
            Stove st = s.Stove;
            st.SelectTool(recipe.tool);
            foreach (string step in recipe.steps)
            {
                if (step == main.name) Assert.IsTrue(st.AddIngredient(main));
                else if (step == side.name) Assert.IsTrue(st.AddIngredient(side));
                else Assert.IsTrue(st.AddPantry(step));
            }
            Assert.IsTrue(st.Start());
            return st;
        }

        [Test]
        public void F033_AC1_Vong2_Qua15Lan_CanhBao()
        {
            GameSession s = TestData.SessionAt(4);
            Recipe r;
            Stove st = FilledStove(s, out r);
            Assert.AreEqual(r.cookSeconds, st.CookSeconds, 1e-4, "ngay thuong: khong nhan he so mat dien");
            st.Tick(r.cookSeconds);
            Assert.AreEqual(StoveState.Ready, st.State);
            Assert.IsFalse(st.IsOverheating);
            st.Tick(r.cookSeconds * (s.Recipes.timing.warnFactor - 1f) - 0.05f);
            Assert.IsFalse(st.IsOverheating);
            st.Tick(0.1f);
            Assert.IsTrue(st.IsOverheating, "qua 1,5 lan thoi gian nau: canh bao");
            Assert.IsFalse(st.IsBurnt);
        }

        [Test]
        public void F033_AC2_Qua2Lan_MonTru1Bac()
        {
            GameSession s = TestData.SessionAt(4);
            Recipe r;
            Stove st = FilledStove(s, out r);
            st.Tick(r.cookSeconds * s.Recipes.timing.burnFactor + 0.05f);
            Assert.IsTrue(st.IsBurnt);
            Dish d;
            Assert.IsTrue(s.PlateDish(out d));
            Assert.AreEqual(3, d.tier, "Tuyet hao qua lua -> Ngon");
        }

        [Test]
        public void F033_AC2_ChuaQua2Lan_GiuBac()
        {
            GameSession s = TestData.SessionAt(4);
            Recipe r;
            Stove st = FilledStove(s, out r);
            st.Tick(r.cookSeconds * s.Recipes.timing.burnFactor - 0.05f);
            Dish d;
            Assert.IsTrue(s.PlateDish(out d));
            Assert.AreEqual(4, d.tier);
        }

        [Test]
        public void F033_AC2_Vong1_KhongQuaLua()
        {
            GameSession s = TestData.SessionAt(2);
            Recipe r;
            Stove st = FilledStove(s, out r);
            st.Tick(r.cookSeconds * 5f);
            Assert.IsFalse(st.IsOverheating);
            Assert.IsFalse(st.IsBurnt);
            Dish d;
            Assert.IsTrue(s.PlateDish(out d));
            Assert.AreEqual(4, d.tier);
        }
    }
}
