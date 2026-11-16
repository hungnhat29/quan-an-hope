using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-034 Cua hang vat pham. Nguon: constitution 1.4 muc 4.3 (ve den), 5.5, 5.6, 7.3 (Hu sot cu), 8, Dieu 3, Dieu 4;
    /// docs/sprint-4/impl-plan.md muc 2 (F-034 AC 1-4).
    /// </summary>
    public class ItemShopTests
    {
        const int Rolls = 100000;
        const double TolerancePoints = 1.0;
        static Balance B { get { return TestData.Balance; } }
        const string Recipe1 = "trung__ca_chua";

        // ---------- AC-1: gia, gioi han ----------

        [Test]
        public void AC1_TruocNgay4_CuaHangDong()
        {
            GameSession s = TestData.SessionAt(3, 100000, 1, 8);
            Assert.IsFalse(s.Items.IsOpen);
            Assert.AreEqual(ItemBuyResult.Closed, s.BuySauce());
            Assert.AreEqual(ItemBuyResult.Closed, s.BuyMachine());
            Assert.AreEqual(ItemBuyResult.Closed, s.BuyBlackTicket());
            Assert.AreEqual(100000, s.Money);
        }

        [Test]
        public void AC1_NuocSot_450_MoiLanSauCong45_GiuToiDa20()
        {
            GameSession s = TestData.SessionAt(4, 1000000);
            Assert.IsTrue(s.Items.IsOpen);
            Assert.AreEqual(450, s.Items.SaucePrice);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuySauce());
            Assert.AreEqual(1000000 - 450, s.Money);
            Assert.AreEqual(495, s.Items.SaucePrice);
            for (int i = 1; i < 20; i++) Assert.AreEqual(ItemBuyResult.Bought, s.BuySauce());
            Assert.AreEqual(20, s.Items.Sauces);
            Assert.AreEqual(ItemBuyResult.AtLimit, s.BuySauce());
            Assert.AreEqual(20, s.Items.Sauces);
        }

        [Test]
        public void AC1_MayGacha_800_MotLan()
        {
            GameSession s = TestData.SessionAt(4, 2000);
            Assert.IsFalse(s.Items.HasMachine);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyMachine());
            Assert.IsTrue(s.Items.HasMachine);
            Assert.AreEqual(1200, s.Money);
            Assert.AreEqual(ItemBuyResult.AtLimit, s.BuyMachine());
            Assert.AreEqual(1200, s.Money);
        }

        [Test]
        public void AC1_VeDen_700_MoiLanSauCong120_GiuToiDa5()
        {
            GameSession s = TestData.SessionAt(4, 100000);
            Assert.AreEqual(700, s.Items.BlackPrice);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyBlackTicket());
            Assert.AreEqual(820, s.Items.BlackPrice);
            for (int i = 1; i < 5; i++) s.BuyBlackTicket();
            Assert.AreEqual(5, s.Items.BlackTickets);
            Assert.AreEqual(ItemBuyResult.AtLimit, s.BuyBlackTicket());
            Assert.AreEqual(100000 - (700 + 820 + 940 + 1060 + 1180), s.Money);
        }

        [Test]
        public void AC1_ThieuTien_KhongMua()
        {
            GameSession s = TestData.SessionAt(4, 449);
            Assert.AreEqual(ItemBuyResult.NotEnoughMoney, s.BuySauce());
            Assert.AreEqual(ItemBuyResult.NotEnoughMoney, s.BuyMachine());
            Assert.AreEqual(ItemBuyResult.NotEnoughMoney, s.BuyBlackTicket());
            Assert.AreEqual(449, s.Money);
            Assert.AreEqual(0, s.Items.Sauces);
        }

        // ---------- AC-2: nuoc sot ----------

        [Test]
        public void AC2_NuocSot_Ngay4_Quay100000Lan_KhopBang()
        {
            SauceOdds o = B.sauce.oddsEarly;
            double[] expected = { o.fail, o.same, o.up1, o.up2, o.legend };
            CollectionAssert.AreEqual(new double[] { 12, 18, 50, 20, 0 }, expected, "bang muc 5.5 ngay 4-8");
            var rng = new SeededRandom(45);
            var counts = new int[5];
            for (int i = 0; i < Rolls; i++) counts[(int)ItemRolls.Sauce(B, 4, RelicFx.None, rng)]++;
            for (int k = 0; k < 5; k++)
                Assert.AreEqual(expected[k], 100.0 * counts[k] / Rolls, TolerancePoints, "ket qua " + (SauceResult)k);
            Assert.AreEqual(0, counts[(int)SauceResult.Legend], "ngay 4-8 khong ra Huyen thoai");
        }

        [Test]
        public void AC2_TiLeHienChoNguoiChoi_KhopBang_HuSotCuGiamHong8Diem()
        {
            double[] shown = ItemRolls.SauceOdds(B, 4, RelicFx.None);
            CollectionAssert.AreEqual(new double[] { 12, 18, 50, 20, 0 }, shown);
            var inv = new RelicInventory(B);
            inv.Add(B.relics.Single(r => r.id == "F9"));
            double[] safe = ItemRolls.SauceOdds(B, 4, inv.Effects);
            Assert.AreEqual(4, safe[0], 1e-4);
            Assert.AreEqual(26, safe[1], 1e-4);
            CollectionAssert.DoesNotContain(RelicKeys.NotYetActive, RelicKeys.SauceSafe);
        }

        [Test]
        public void AC2_DungNuocSotLenDia_DoiBac_TruMotChai()
        {
            Assert.AreEqual(0, ItemRolls.ApplySauce(SauceResult.Fail, 3, B));
            Assert.AreEqual(3, ItemRolls.ApplySauce(SauceResult.Same, 3, B));
            Assert.AreEqual(4, ItemRolls.ApplySauce(SauceResult.Up1, 3, B));
            Assert.AreEqual(4, ItemRolls.ApplySauce(SauceResult.Up2, 3, B), "toi da Tuyet hao (sim_balance sauce_roll)");
            Assert.AreEqual(5, ItemRolls.ApplySauce(SauceResult.Legend, 1, B));

            GameSession s = TestData.SessionAt(4, 1000);
            s.BuySauce();
            Dish d = TestData.DishOf(Recipe1, 2);
            SauceResult r;
            Assert.IsFalse(s.UseSauce(d, out r), "dia khong o quay cho / tu lanh");
            s.Counter.TryPlace(d);
            Assert.IsTrue(s.UseSauce(d, out r));
            Assert.AreEqual(ItemRolls.ApplySauce(r, 2, B), d.tier);
            Assert.AreEqual(0, s.Items.Sauces);
            Assert.IsFalse(s.UseSauce(d, out r), "het nuoc sot");
        }

        // ---------- AC-3: may gacha do an ----------

        [Test]
        public void AC3_MayGacha_Quay100000Lan_KhopBang()
        {
            MachineRules m = B.machine;
            double[] expected = { m.trash, m.down1, m.same, m.up1, m.up2, m.up3 };
            CollectionAssert.AreEqual(new double[] { 15, 20, 25, 28, 10, 2 }, expected, "bang muc 5.6");
            var rng = new SeededRandom(56);
            var counts = new int[6];
            for (int i = 0; i < Rolls; i++) counts[(int)ItemRolls.Machine(B, rng)]++;
            for (int k = 0; k < 6; k++)
                Assert.AreEqual(expected[k], 100.0 * counts[k] / Rolls, TolerancePoints, "ket qua " + (MachineResult)k);
        }

        [Test]
        public void AC3_MoiMonMotLan_3LanMoiNgay_HongThiMatMon()
        {
            Assert.AreEqual(-1, ItemRolls.ApplyMachine(MachineResult.Trash, 2, B));
            Assert.AreEqual(1, ItemRolls.ApplyMachine(MachineResult.Down1, 2, B));
            Assert.AreEqual(0, ItemRolls.ApplyMachine(MachineResult.Down1, 0, B));
            Assert.AreEqual(4, ItemRolls.ApplyMachine(MachineResult.Up3, 2, B));

            GameSession s = TestData.SessionAt(4, 1000, 9);
            SauceResult unused;
            MachineResult r;
            Dish first = TestData.DishOf(Recipe1, 1);
            s.Counter.TryPlace(first);
            Assert.IsFalse(s.UseMachine(first, out r), "chua mua may");
            s.BuyMachine();
            Assert.IsTrue(s.UseMachine(first, out r));
            if (r != MachineResult.Trash) Assert.IsFalse(s.UseMachine(first, out r), "moi mon chi 1 lan");
            Assert.AreEqual(s.Balance.machine.usesPerDay - 1, s.Items.MachineUsesLeft);
            int used = 1;
            while (used < 3)
            {
                Dish d = TestData.DishOf(Recipe1, 1);
                s.Counter.Clear();
                s.Counter.TryPlace(d);
                Assert.IsTrue(s.UseMachine(d, out r));
                if (r == MachineResult.Trash) Assert.AreEqual(0, s.Counter.Dishes.Count, "hong: mat mon");
                used++;
            }
            Dish extra = TestData.DishOf(Recipe1, 1);
            s.Counter.Clear();
            s.Counter.TryPlace(extra);
            Assert.IsFalse(s.UseMachine(extra, out r), "het 3 lan hom nay");
            Assert.IsFalse(s.UseSauce(extra, out unused));
        }

        [Test]
        public void AC3_LuotMay_HoiLaiMoiNgay()
        {
            GameSession s = TestData.SessionAt(4, 1000);
            s.BuyMachine();
            Assert.AreEqual(3, s.Items.MachineUsesLeft);
            s.OpenShop(); s.CloseEarly(); s.StartNextDay();
            Assert.AreEqual(3, s.Items.MachineUsesLeft);
        }

        // ---------- AC-4: ve den ----------

        [Test]
        public void AC4_VeDen_ThungTheoBangDen_100000Mon()
        {
            Assert.AreEqual(new[] { 0, 0, 0, 62, 30, 8 }, B.BlackOdds().percents);
            var gacha = new IngredientGacha(B, TestData.Recipes, new SeededRandom(62));
            var counts = new int[6];
            int total = 0;
            while (total < Rolls)
            {
                PullResult p = gacha.Pull(4, RelicFx.None, true);
                Assert.AreEqual(B.crateSize.phu, p.side.items.Count);
                Assert.AreEqual(B.crateSize.chinh, p.main.items.Count);
                foreach (Ingredient i in p.side.items.Concat(p.main.items)) { counts[i.tier]++; total++; }
            }
            int[] expected = B.BlackOdds().percents;
            for (int t = 0; t < 6; t++)
            {
                Assert.AreEqual(expected[t], 100.0 * counts[t] / total, TolerancePoints, "bac " + t);
                if (expected[t] == 0) Assert.AreEqual(0, counts[t]);
            }
        }

        [Test]
        public void AC4_NhetVeDen_TruVeDen_KhongTruVeTrang()
        {
            GameSession s = TestData.SessionAt(4, 1000);
            PullResult p;
            Assert.IsFalse(s.PullBlack(out p), "chua co ve den");
            s.BuyBlackTicket();
            int white = s.Wallet.Count;
            Assert.IsTrue(s.PullBlack(out p));
            Assert.AreEqual(0, s.Items.BlackTickets);
            Assert.AreEqual(white, s.Wallet.Count);
            Assert.AreEqual(p.side.items.Count + p.main.items.Count, s.Stock.Count);
            Assert.IsTrue(p.side.items.All(i => i.tier >= 3));
        }
    }
}
