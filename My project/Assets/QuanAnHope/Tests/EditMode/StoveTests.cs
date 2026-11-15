using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-003 AC-4 va phan logic cua AC-5, AC-6. Nguon: constitution muc 5.1, 5.8.</summary>
    public class StoveTests
    {
        Counter counter;

        Stove NewStove()
        {
            counter = new Counter(TestData.Balance);
            return new Stove(new RecipeLookup(TestData.Recipes), new DishFormula(TestData.Balance));
        }

        /// <summary>Trung chien ca chua dung cong thuc: Dau an -> Ca chua -> Trung ga -> Nuoc mam, Chao, 12 giay.</summary>
        static void FillTrungChienCaChua(Stove stove)
        {
            Assert.IsTrue(stove.SelectTool("Chảo"));
            Assert.IsTrue(stove.AddPantry("Dầu ăn"));
            Assert.IsTrue(stove.AddIngredient(TestData.Side("ca_chua", 1)));
            Assert.IsTrue(stove.AddIngredient(TestData.Main("trung", 1)));
            Assert.IsTrue(stove.AddPantry("Nước mắm"));
        }

        static Dish CookAndPlate(Stove stove, Counter counter, double roll)
        {
            Assert.IsTrue(stove.Start());
            stove.Tick(stove.CookSeconds);
            Dish dish;
            Assert.IsTrue(stove.TryPlate(counter, new FixedRandom(roll), out dish));
            return dish;
        }

        // ---------- AC-4 ----------

        [Test]
        public void AC4_DungThuTu_RaMonTheoCongThucBac()
        {
            Stove stove = NewStove();
            FillTrungChienCaChua(stove);

            Dish dish = CookAndPlate(stove, counter, 0.5);

            Assert.AreEqual("trung__ca_chua", dish.recipeId);
            Assert.AreEqual("Trứng chiên cà chua", dish.name);
            Assert.AreEqual("Cơm", dish.baseName);
            Assert.AreEqual(2, dish.tier, "chinh 1 + phu 1, so ngau nhien 0,5 < 0,8 -> Binh thuong");
            Assert.AreEqual(1, counter.Dishes.Count);
            Assert.AreEqual(StoveState.Empty, stove.State);
        }

        /// <summary>F-013 AC-6: bep biet ngay tu luc vat num la mon se Lac vi, de hinh trong chao chay xem.</summary>
        [Test]
        public void F013_AC6_BietSaiThuTuTuLucBatDauNau()
        {
            Stove ok = NewStove();
            FillTrungChienCaChua(ok);
            Assert.IsFalse(ok.FollowsRecipe, "chua nau thi chua biet");
            Assert.IsTrue(ok.Start());
            Assert.IsTrue(ok.FollowsRecipe);

            Stove wrong = NewStove();
            wrong.SelectTool("Chảo");
            wrong.AddIngredient(TestData.Side("ca_chua", 1));   // thieu dau an truoc
            wrong.AddPantry("Dầu ăn");
            wrong.AddIngredient(TestData.Main("trung", 1));
            wrong.AddPantry("Nước mắm");
            Assert.IsTrue(wrong.Start());
            Assert.IsFalse(wrong.FollowsRecipe);
            wrong.Tick(wrong.CookSeconds);
            Assert.AreEqual(StoveState.Ready, wrong.State);
            Assert.IsFalse(wrong.FollowsRecipe, "chin roi van biet de ve mon chay");
        }

        [Test]
        public void AC4_SaiThuTu_NguyenLieu_RaLacVi()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            stove.AddPantry("Dầu ăn");
            stove.AddIngredient(TestData.Main("trung", 3));   // dung ra phai cho ca chua truoc
            stove.AddIngredient(TestData.Side("ca_chua", 3));
            stove.AddPantry("Nước mắm");

            Dish dish = CookAndPlate(stove, counter, 0.5);

            Assert.AreEqual(0, dish.tier);
            Assert.AreEqual("Trứng chiên cà chua", dish.name);
        }

        [Test]
        public void AC4_SaiThuTu_DoKho_RaLacVi()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            stove.AddIngredient(TestData.Side("ca_chua", 3));
            stove.AddPantry("Dầu ăn");                        // dau an phai vao dau tien
            stove.AddIngredient(TestData.Main("trung", 3));
            stove.AddPantry("Nước mắm");

            Assert.AreEqual(0, CookAndPlate(stove, counter, 0.5).tier);
        }

        [Test]
        public void AC4_ThieuDoKho_RaLacVi()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            stove.AddPantry("Dầu ăn");
            stove.AddIngredient(TestData.Side("ca_chua", 3));
            stove.AddIngredient(TestData.Main("trung", 3));

            Assert.AreEqual(0, CookAndPlate(stove, counter, 0.5).tier);
        }

        [Test]
        public void AC4_ThuaDoKho_RaLacVi()
        {
            Stove stove = NewStove();
            FillTrungChienCaChua(stove);
            stove.AddPantry("Tỏi");

            Assert.AreEqual(0, CookAndPlate(stove, counter, 0.5).tier);
        }

        [Test]
        public void AC4_SaiDungCu_RaLacVi()
        {
            // Mac dinh cho cau hoi Q-11: sai dung cu coi nhu sai cong thuc.
            Stove stove = NewStove();
            stove.SelectTool("Nồi");
            stove.AddPantry("Dầu ăn");
            stove.AddIngredient(TestData.Side("ca_chua", 3));
            stove.AddIngredient(TestData.Main("trung", 3));
            stove.AddPantry("Nước mắm");

            Assert.AreEqual(0, CookAndPlate(stove, counter, 0.5).tier);
        }

        [Test]
        public void AC4_HeoXao_DoPhuVaoSauCung()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            stove.AddPantry("Dầu ăn");
            stove.AddPantry("Tỏi");
            stove.AddIngredient(TestData.Main("heo", 2));
            stove.AddIngredient(TestData.Side("rau_muong", 2));

            Dish dish = CookAndPlate(stove, counter, 0.5);

            Assert.AreEqual("Heo xào rau muống", dish.name);
            Assert.AreEqual(3, dish.tier, "chinh 2 + phu 2: b = 2,8; 0,5 < 0,8 -> Ngon");
        }

        // ---------- AC-5 (logic): thanh ti le co ngay khi du 2 nguyen lieu ----------

        [Test]
        public void AC5_ThanhTiLe_CoKhiDuNguyenLieuChinhVaPhu()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            stove.AddPantry("Dầu ăn");
            Assert.IsNull(stove.Bar());
            stove.AddIngredient(TestData.Side("ca_chua", 1));
            Assert.IsNull(stove.Bar());
            stove.AddIngredient(TestData.Main("trung", 1));

            TierChance[] bar = stove.Bar();

            Assert.AreEqual(2, bar.Length);
            Assert.AreEqual(1, bar[0].tier);
            Assert.AreEqual(0.2, bar[0].chance, 1e-6);
            Assert.AreEqual(2, bar[1].tier);
            Assert.AreEqual(0.8, bar[1].chance, 1e-6);
        }

        // ---------- AC-6 (logic): trinh tu thao tac ----------

        [Test]
        public void AC6_PhaiChonDungCuTruoc()
        {
            Stove stove = NewStove();
            Assert.IsFalse(stove.AddPantry("Dầu ăn"));
            Assert.IsFalse(stove.AddIngredient(TestData.Side("ca_chua", 1)));
        }

        [Test]
        public void AC6_NguyenLieuChuaSoChe_KhongChoVaoDuoc()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            Assert.IsFalse(stove.AddIngredient(TestData.Side("ca_chua", 1, prepared: false)));
        }

        [Test]
        public void AC6_MoiMonChiMotChinh_MotPhu()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            Assert.IsTrue(stove.AddIngredient(TestData.Side("ca_chua", 1)));
            Assert.IsFalse(stove.AddIngredient(TestData.Side("hanh_tay", 1)));
            Assert.IsTrue(stove.AddIngredient(TestData.Main("trung", 1)));
            Assert.IsFalse(stove.AddIngredient(TestData.Main("heo", 1)));
        }

        [Test]
        public void AC6_ChuaDuNguyenLieu_KhongNauDuoc()
        {
            Stove stove = NewStove();
            stove.SelectTool("Chảo");
            stove.AddPantry("Dầu ăn");
            stove.AddIngredient(TestData.Side("ca_chua", 1));

            Assert.IsFalse(stove.Start());
            Assert.AreEqual(StoveState.Filling, stove.State);
        }

        [Test]
        public void AC6_PhaiChoDuThoiGianNau_MoiMucDuoc()
        {
            Stove stove = NewStove();
            FillTrungChienCaChua(stove);
            Assert.IsTrue(stove.Start());
            Assert.AreEqual(12f, stove.CookSeconds, 1e-6f, "trung chien 12 giay (constitution 1.4 muc 5.9)");
            Assert.AreEqual(StoveState.Cooking, stove.State);

            stove.Tick(11.9f);
            Dish dish;
            Assert.AreEqual(StoveState.Cooking, stove.State);
            Assert.IsFalse(stove.TryPlate(counter, new FixedRandom(0.5), out dish));

            stove.Tick(0.2f);
            Assert.AreEqual(StoveState.Ready, stove.State);
            Assert.IsTrue(stove.TryPlate(counter, new FixedRandom(0.5), out dish));
        }

        [Test]
        public void AC6_DangNau_KhongChoThemDuoc()
        {
            Stove stove = NewStove();
            FillTrungChienCaChua(stove);
            stove.Start();

            Assert.IsFalse(stove.AddPantry("Tỏi"));
            Assert.IsFalse(stove.SelectTool("Nồi"));
        }

        // ---------- AC-7 (phia bep): quay cho day thi khong muc duoc ----------

        [Test]
        public void AC7_QuayChoDay_KhongMucDuoc_MonVanTrenBep()
        {
            Stove stove = NewStove();
            for (int i = 0; i < 3; i++)
            {
                FillTrungChienCaChua(stove);
                CookAndPlate(stove, counter, 0.5);
            }
            Assert.IsTrue(counter.IsFull);

            FillTrungChienCaChua(stove);
            stove.Start();
            stove.Tick(stove.CookSeconds);
            Dish dish;

            Assert.IsFalse(stove.TryPlate(counter, new FixedRandom(0.5), out dish));
            Assert.IsNull(dish);
            Assert.AreEqual(StoveState.Ready, stove.State);

            counter.TakeAt(0);
            Assert.IsTrue(stove.TryPlate(counter, new FixedRandom(0.5), out dish));
        }
    }
}
