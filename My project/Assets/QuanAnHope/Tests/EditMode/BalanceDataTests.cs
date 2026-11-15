using System.IO;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Data;
using QuanAnHope.EditorTools;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-001 Du lieu can bang. Cac con so mong doi trong file nay chep tu specs/constitution.md v1.4 (F-010).
    /// </summary>
    public class BalanceDataTests
    {
        const string TempAssetPath = "Assets/QuanAnHope/Data/__BalanceData_Test.asset";

        Balance balance;
        RecipeBook recipes;

        [OneTimeSetUp]
        public void ReadJson()
        {
            balance = BalanceImporter.ParseBalance(File.ReadAllText(BalanceImporter.BalanceJsonPath), File.ReadAllText(BalanceImporter.RelicsJsonPath));
            recipes = BalanceImporter.ParseRecipes(File.ReadAllText(BalanceImporter.RecipesJsonPath));
        }

        [TearDown]
        public void DeleteTempAsset()
        {
            AssetDatabase.DeleteAsset(TempAssetPath);
        }

        static TierTable Odds(Balance b, string id)
        {
            return b.ingredientOdds.Single(t => t.id == id);
        }

        // ---------- AC-1: nhap JSON sinh ra asset voi moi truong sprint 1 dung ----------

        [Test]
        public void AC1_Import_TaoRaAssetDocLaiDuoc()
        {
            BalanceData made = BalanceImporter.ImportTo(TempAssetPath);

            Assert.IsNotNull(made, "ImportTo phai tra ve asset vua tao");
            var loaded = AssetDatabase.LoadAssetAtPath<BalanceData>(TempAssetPath);
            Assert.IsNotNull(loaded, "Asset phai nam o " + TempAssetPath);
            Assert.AreEqual(22, loaded.balance.days);
            Assert.AreEqual(120, loaded.recipes.recipes.Length);
        }

        [Test]
        public void AC1_TienTe_VaLichNo() // constitution muc 2, 3
        {
            Assert.AreEqual(22, balance.days);
            Assert.AreEqual(6, balance.ticketsPerDay);
            Assert.AreEqual(100, balance.start.money);
            Assert.AreEqual(2, balance.start.bonusTickets);
            Assert.AreEqual(2, balance.start.relicShopFromDay);
            Assert.AreEqual(11500, balance.bossBonus);
            CollectionAssert.AreEqual(new[] { 4, 9, 15, 22 }, balance.debts.Select(d => d.day).ToArray());
            CollectionAssert.AreEqual(new[] { 750, 2900, 10500, 32000 }, balance.debts.Select(d => d.amount).ToArray());
        }

        [Test]
        public void AC1_Vong_TheoNgay() // constitution muc 2
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, balance.rounds.Select(r => r.round).ToArray());
            CollectionAssert.AreEqual(new[] { 1, 4, 9, 15 }, balance.rounds.Select(r => r.firstDay).ToArray());
            CollectionAssert.AreEqual(new[] { 3, 8, 14, 21 }, balance.rounds.Select(r => r.lastDay).ToArray());
        }

        [Test]
        public void AC1_TiLeBac_VaKichThuocThung() // constitution muc 4.2, 4.3
        {
            CollectionAssert.AreEqual(new[] { 25, 57, 15, 3, 0, 0 }, Odds(balance, "r1").percents);
            CollectionAssert.AreEqual(new[] { 50, 42, 8, 0, 0, 0 }, Odds(balance, "storm").percents);
            CollectionAssert.AreEqual(new[] { 15, 40, 33, 10, 2, 0 }, Odds(balance, "r2").percents);
            CollectionAssert.AreEqual(new[] { 10, 28, 35, 20, 6, 1 }, Odds(balance, "r3").percents);
            CollectionAssert.AreEqual(new[] { 6, 18, 33, 28, 12, 3 }, Odds(balance, "r4").percents);
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 62, 30, 8 }, Odds(balance, "black").percents);
            Assert.AreEqual(3, balance.crateSize.phu);
            Assert.AreEqual(2, balance.crateSize.chinh);
            Assert.AreEqual(2, balance.crateSize.phuStorm);
            Assert.AreEqual(1, balance.crateSize.chinhStorm);
        }

        [Test]
        public void AC1_CongThucBacMon_VaGia() // constitution muc 5.2, 5.3
        {
            Assert.AreEqual(0.6f, balance.dish.weightChinh, 1e-6f);
            Assert.AreEqual(0.4f, balance.dish.weightPhu, 1e-6f);
            Assert.AreEqual(0.8f, balance.dish.offset, 1e-6f);
            Assert.AreEqual(0.3f, balance.dish.blackoutPenalty, 1e-6f);
            CollectionAssert.AreEqual(new[] { 0.2f, 0.5f, 1.0f, 1.8f, 3.2f, 6.0f }, balance.dish.priceMult);
            CollectionAssert.AreEqual(new[] { 60, 70, 85, 100 }, balance.dish.basePriceByRound);
        }

        [Test]
        public void AC1_SoBoSungNgay2Va3() // constitution muc 4.3, 5.1, 5.3, 6.5
        {
            Assert.AreEqual(3, balance.stormDay);
            Assert.AreEqual(3, balance.counterSlots);
            Assert.AreEqual(4, balance.dish.maxTier);
            Assert.AreEqual(0.3f, balance.payment.tipBase, 1e-6f);
            Assert.AreEqual(0.2f, balance.payment.tipPerTier, 1e-6f);
            Assert.AreEqual(0.5f, balance.payment.underPayShare, 1e-6f);
            Assert.AreEqual(0.3f, balance.payment.penaltyBaseShare, 1e-6f);
        }

        [Test]
        public void AC1_Khach() // constitution muc 6.3, 6.4 (1.4: khong con so khach moi ngay)
        {
            Assert.AreEqual(5, balance.customers.bossEasy);

            EasyBand[] round1 = balance.customers.easyBands.Single(b => b.round == 1).bands;
            CollectionAssert.AreEqual(new[] { 60, 35, 10 }, round1.Select(b => b.min).ToArray());
            CollectionAssert.AreEqual(new[] { 95, 59, 34 }, round1.Select(b => b.max).ToArray());
            CollectionAssert.AreEqual(new[] { 0.9f, 0.1f, 0f }, round1.Select(b => b.share).ToArray());

            CollectionAssert.AreEqual(new[] { 80, 60, 35, 0 }, balance.customers.requiredTier.Select(r => r.minEasy).ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, balance.customers.requiredTier.Select(r => r.tier).ToArray());
        }

        [Test]
        public void AC1_NguyenLieu_Vong1() // constitution muc 4.5
        {
            Assert.AreEqual(10, recipes.sides.Length);
            Assert.AreEqual(8, recipes.mains.Count(m => m.group == "chinh"));
            Assert.AreEqual(4, recipes.mains.Count(m => m.group == "dotpha"));

            CollectionAssert.AreEquivalent(
                new[] { "Cà chua", "Hành tây", "Rau muống" },
                recipes.sides.Where(s => s.unlockRound == 1).Select(s => s.name).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "Trứng gà", "Thịt heo", "Đậu phụ" },
                recipes.mains.Where(m => m.group == "chinh" && m.unlockRound == 1).Select(m => m.name).ToArray());

            Assert.AreEqual(4.5f, recipes.sides.Single(s => s.id == "ca_chua").prepSeconds, 1e-6f);
            Assert.AreEqual(4.5f, recipes.sides.Single(s => s.id == "hanh_tay").prepSeconds, 1e-6f);
            Assert.AreEqual(3.0f, recipes.sides.Single(s => s.id == "rau_muong").prepSeconds, 1e-6f);
            Assert.AreEqual(1.5f, recipes.mains.Single(m => m.id == "trung").prepSeconds, 1e-6f);
            Assert.AreEqual(4.5f, recipes.mains.Single(m => m.id == "heo").prepSeconds, 1e-6f);
            Assert.AreEqual(3.0f, recipes.mains.Single(m => m.id == "dau_phu").prepSeconds, 1e-6f);
        }

        [Test]
        public void AC1_CongThuc_Vong1() // constitution muc 5.8, 5.9
        {
            Assert.AreEqual(120, recipes.recipes.Length);
            Recipe[] round1 = recipes.recipes.Where(r => r.group == "chinh" && r.unlockRound == 1).ToArray();
            Assert.AreEqual(9, round1.Length);

            Recipe trung = recipes.recipes.Single(r => r.id == "trung__ca_chua");
            Assert.AreEqual("Trứng chiên cà chua", trung.name);
            Assert.AreEqual("trung", trung.mainId);
            Assert.AreEqual("ca_chua", trung.sideId);
            Assert.AreEqual("Chảo", trung.tool);
            Assert.AreEqual("Cơm", trung.baseName);
            CollectionAssert.AreEqual(new[] { "Dầu ăn", "Cà chua", "Trứng gà", "Nước mắm" }, trung.steps);
            Assert.AreEqual(12f, trung.cookSeconds, 1e-6f);

            Recipe heo = recipes.recipes.Single(r => r.id == "heo__rau_muong");
            CollectionAssert.AreEqual(new[] { "Dầu ăn", "Tỏi", "Thịt heo", "Rau muống" }, heo.steps);
            Assert.AreEqual(15f, heo.cookSeconds, 1e-6f);

            Recipe dau = recipes.recipes.Single(r => r.id == "dau_phu__hanh_tay");
            Assert.AreEqual("Nồi", dau.tool);
            Assert.AreEqual(18f, dau.cookSeconds, 1e-6f);

            Assert.AreEqual("Mì Ý bò sốt cà chua", recipes.recipes.Single(r => r.id == "bo__ca_chua").name);

            // F-013: cach nau quyet dinh hinh mon trong dung cu (constitution 1.4 muc 5.8, 5.9)
            Assert.AreEqual("chiên", recipes.mains.Single(m => m.id == "trung").method);
            Assert.AreEqual("xào", recipes.mains.Single(m => m.id == "heo").method);
            Assert.AreEqual("sốt", recipes.mains.Single(m => m.id == "dau_phu").method);
        }

        [Test]
        public void AC1_ThoiGianNau() // constitution muc 5.8 (chu thich *), 5.9
        {
            Assert.AreEqual(0.5f, recipes.timing.stepSeconds, 1e-6f);
            Assert.AreEqual(1.0f, recipes.timing.plateSeconds, 1e-6f);
            Assert.AreEqual(1.5f, recipes.timing.serveSeconds, 1e-6f);
            Assert.AreEqual(1.5f, recipes.timing.warnFactor, 1e-6f);
            Assert.AreEqual(2.0f, recipes.timing.burnFactor, 1e-6f);
            Assert.AreEqual(2, recipes.timing.burnFromRound);
            Assert.AreEqual(9, recipes.pantry.Length);
            CollectionAssert.AreEqual(new[] { "Cơm", "Mì Ý", "Bún", "Bánh mì" }, recipes.bases);
        }

        [Test]
        public void F010_NgayTheoGio() // constitution 1.4 muc 2, 6.2
        {
            Assert.AreEqual(9, balance.day.openHour);
            Assert.AreEqual(21, balance.day.closeHour);
            Assert.AreEqual(40f, balance.day.secondsPerHour, 1e-6f);
            Assert.AreEqual(20, balance.day.bossHour);
            Assert.AreEqual(3f, balance.day.nextCustomerSeconds, 1e-6f);
            Assert.AreEqual(480f, balance.DaySeconds(), 1e-6f, "9:00-21:00 = 12 gio x 40 giay = 8 phut");
        }

        // ---------- AC-2: gia tri trong asset bang gia tri trong JSON ----------

        [Test]
        public void AC2_AssetDaCommit_KhopVoiJson()
        {
            var asset = AssetDatabase.LoadAssetAtPath<BalanceData>(BalanceImporter.AssetPath);
            Assert.IsNotNull(asset, "Chua co " + BalanceImporter.AssetPath + ". Chay menu Quán Ăn Hope > Nhập balance.json.");

            Assert.AreEqual(JsonUtility.ToJson(balance, true), JsonUtility.ToJson(asset.balance, true),
                "balance.json da doi ma chua nhap lai asset");
            Assert.AreEqual(JsonUtility.ToJson(recipes, true), JsonUtility.ToJson(asset.recipes, true),
                "recipes.json da doi ma chua nhap lai asset");
        }

        // ---------- AC-3: moi bang ti le co tong 100 ----------

        [Test]
        public void AC3_MoiBangTiLeBac_CoTong100()
        {
            Assert.AreEqual(6, balance.ingredientOdds.Length, "Phai co du 6 bang: r1, storm, r2, r3, r4, black");
            foreach (TierTable table in balance.ingredientOdds)
            {
                Assert.AreEqual(6, table.percents.Length, "Bang " + table.id + " phai co 6 bac");
                Assert.AreEqual(100, table.percents.Sum(), "Bang " + table.id);
            }
        }

        [Test]
        public void AC3_TiLeKhachMoiVong_CoTong100Phantram()
        {
            Assert.AreEqual(4, balance.customers.easyBands.Length);
            foreach (RoundBands round in balance.customers.easyBands)
            {
                Assert.AreEqual(1f, round.bands.Sum(b => b.share), 1e-4f, "Vong " + round.round);
            }
        }
    }
}
