using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-003 AC-6 (thot), AC-7 (quay cho), AC-8 (tra cong thuc). Nguon: constitution muc 4.5, 5.1, 5.8.</summary>
    public class KitchenTests
    {
        // ---------- AC-6: thot ----------

        [Test]
        public void AC6_Thot_GiuDuThoiGian_MoiSoCheXong()
        {
            var board = new PrepStation();
            Ingredient tomato = TestData.Side("ca_chua", 1, prepared: false); // 4,5 giay (constitution 1.4 muc 4.5)

            Assert.IsTrue(board.Begin(tomato));
            Assert.IsFalse(board.Hold(3.0f));
            Assert.IsFalse(tomato.prepared);
            Assert.AreEqual(3.0 / 4.5, board.Progress, 1e-4);

            Assert.IsTrue(board.Hold(1.5f));
            Assert.IsTrue(tomato.prepared);
            Assert.IsNull(board.Current);
        }

        [Test]
        public void AC6_Thot_MoiNguyenLieuCoThoiGianRieng()
        {
            var board = new PrepStation();
            Ingredient egg = TestData.Main("trung", 1, prepared: false); // 1,5 giay
            board.Begin(egg);
            Assert.IsFalse(board.Hold(1.4f));
            Assert.IsTrue(board.Hold(0.1f));
            Assert.IsTrue(egg.prepared);
        }

        [Test]
        public void AC6_Thot_KhongNhanDoDaSoChe_VaKhongNhanHaiThuCungLuc()
        {
            var board = new PrepStation();
            Assert.IsFalse(board.Begin(TestData.Side("ca_chua", 1, prepared: true)));

            Assert.IsTrue(board.Begin(TestData.Side("ca_chua", 1, prepared: false)));
            Assert.IsFalse(board.Begin(TestData.Side("hanh_tay", 1, prepared: false)));
        }

        // ---------- AC-7: quay cho ----------

        [Test]
        public void AC7_QuayCho_GiuToiDa3Dia()
        {
            var counter = new Counter(TestData.Balance);
            Assert.AreEqual(3, counter.Capacity);

            for (int i = 0; i < 3; i++) Assert.IsTrue(counter.TryPlace(new Dish { name = "Mon " + i, tier = i }));
            Assert.IsTrue(counter.IsFull);
            Assert.IsFalse(counter.TryPlace(new Dish { name = "Mon thu tu", tier = 4 }));
            Assert.AreEqual(3, counter.Dishes.Count);
        }

        [Test]
        public void AC7_QuayCho_KhongGiamBac_LayRaDungDia()
        {
            var counter = new Counter(TestData.Balance);
            counter.TryPlace(new Dish { name = "A", tier = 2 });
            counter.TryPlace(new Dish { name = "B", tier = 4 });

            Dish taken = counter.TakeAt(1);

            Assert.AreEqual("B", taken.name);
            Assert.AreEqual(4, taken.tier);
            Assert.AreEqual(1, counter.Dishes.Count);
            Assert.AreEqual("A", counter.Dishes[0].name);
            Assert.AreEqual(2, counter.Dishes[0].tier);
        }

        [Test]
        public void AC7_QuayCho_BoHetCuoiNgay()
        {
            var counter = new Counter(TestData.Balance);
            counter.TryPlace(new Dish { name = "A", tier = 2 });
            counter.Clear();
            Assert.AreEqual(0, counter.Dishes.Count);
        }

        // ---------- AC-8: tra cong thuc ----------

        [TestCase("trung", "ca_chua", "Trứng chiên cà chua", "Chảo", 12f)]
        [TestCase("trung", "hanh_tay", "Trứng chiên hành tây", "Chảo", 12f)]
        [TestCase("trung", "rau_muong", "Trứng chiên rau muống", "Chảo", 12f)]
        [TestCase("heo", "ca_chua", "Heo xào cà chua", "Chảo", 15f)]
        [TestCase("heo", "hanh_tay", "Heo xào hành tây", "Chảo", 15f)]
        [TestCase("heo", "rau_muong", "Heo xào rau muống", "Chảo", 15f)]
        [TestCase("dau_phu", "ca_chua", "Đậu phụ sốt cà chua", "Nồi", 18f)]
        [TestCase("dau_phu", "hanh_tay", "Đậu phụ sốt hành tây", "Nồi", 18f)]
        [TestCase("dau_phu", "rau_muong", "Đậu phụ sốt rau muống", "Nồi", 18f)]
        public void AC8_ChinMonVong1(string mainId, string sideId, string name, string tool, float cookSeconds)
        {
            Recipe r = new RecipeLookup(TestData.Recipes).Find(mainId, sideId);

            Assert.IsNotNull(r);
            Assert.AreEqual(name, r.name);
            Assert.AreEqual(tool, r.tool);
            Assert.AreEqual("Cơm", r.baseName);
            Assert.AreEqual(cookSeconds, r.cookSeconds, 1e-6f);
            Assert.AreEqual(4, r.steps.Length);
        }

        [Test]
        public void AC8_CapKhongCoCongThuc_TraVeNull()
        {
            Assert.IsNull(new RecipeLookup(TestData.Recipes).Find("trung", "khong_co"));
        }

        [Test]
        public void AC8_MoiCapChinhPhuDeuCoCongThuc()
        {
            var lookup = new RecipeLookup(TestData.Recipes);
            foreach (MainIngredient m in TestData.Recipes.mains)
                foreach (SideIngredient s in TestData.Recipes.sides)
                    Assert.IsNotNull(lookup.Find(m.id, s.id), m.id + " + " + s.id);
            Assert.AreEqual(TestData.Recipes.mains.Length * TestData.Recipes.sides.Length, TestData.Recipes.recipes.Length);
        }
    }
}
