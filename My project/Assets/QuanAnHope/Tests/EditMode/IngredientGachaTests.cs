using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-002 AC-2, 3, 4, 6, 8. Nguon: constitution muc 4.2, 4.3, 4.5.</summary>
    public class IngredientGachaTests
    {
        static readonly string[] Round1Sides = { "ca_chua", "hanh_tay", "rau_muong" };
        static readonly string[] Round1Mains = { "trung", "heo", "dau_phu" };

        static IngredientGacha Gacha(int seed)
        {
            return new IngredientGacha(TestData.Balance, TestData.Recipes, new SeededRandom(seed));
        }

        // ---------- AC-2 ----------

        [Test]
        public void AC2_MotLuot_Ton1Ve_Cho3DoPhu_2Chinh() // constitution 1.4 muc 4.2
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);

            PullResult pull;
            bool ok = Gacha(1).TryPull(wallet, 1, out pull);

            Assert.IsTrue(ok);
            Assert.AreEqual(4, wallet.Count, "ngay 1 co 5 ve, tieu 1");
            Assert.AreEqual(3, pull.side.items.Count);
            Assert.AreEqual(2, pull.main.items.Count);
            Assert.IsTrue(pull.side.items.All(i => i.kind == IngredientKind.Side));
            Assert.IsTrue(pull.main.items.All(i => i.kind == IngredientKind.Main));
        }

        [Test]
        public void AC2_HetVe_KhongQuayDuoc()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            IngredientGacha gacha = Gacha(2);
            PullResult pull;
            for (int i = 0; i < 5; i++) Assert.IsTrue(gacha.TryPull(wallet, 1, out pull));

            bool ok = gacha.TryPull(wallet, 1, out pull);

            Assert.IsFalse(ok);
            Assert.IsNull(pull);
            Assert.AreEqual(0, wallet.Count);
        }

        // ---------- AC-3 ----------

        [Test]
        public void AC3_Ngay3_Cho2DoPhu_1Chinh() // constitution 1.4 muc 4.3
        {
            PullResult pull = Gacha(3).Pull(3);
            Assert.AreEqual(2, pull.side.items.Count);
            Assert.AreEqual(1, pull.main.items.Count);
        }

        [Test]
        public void AC3_Ngay2_VanLaThungThuong()
        {
            PullResult pull = Gacha(4).Pull(2);
            Assert.AreEqual(3, pull.side.items.Count);
            Assert.AreEqual(2, pull.main.items.Count);
        }

        // ---------- AC-4 ----------

        [Test]
        public void AC4_Ngay1Va2_DungBangVong1_Ngay3_DungBangSauMua()
        {
            Balance b = TestData.Balance;
            Assert.AreEqual("r1", b.OddsForDay(1).id);
            Assert.AreEqual("r1", b.OddsForDay(2).id);
            Assert.AreEqual("storm", b.OddsForDay(3).id);
            Assert.AreEqual("r2", b.OddsForDay(4).id);
        }

        [Test]
        public void AC4_MoiMonQuayBacDocLap()
        {
            // 1 gia tri chon loai + 3 gia tri bac cho thung phu, roi 1 + 2 cho thung chinh (constitution 1.4 muc 4.2).
            // Bang vong 1: 25 / 57 / 15 / 3 -> nguong tich luy 0,25 / 0,82 / 0,97 / 1.
            var rng = new FixedRandom(
                0.0, 0.10, 0.50, 0.90,
                0.0, 0.98, 0.26);
            var gacha = new IngredientGacha(TestData.Balance, TestData.Recipes, rng);

            PullResult pull = gacha.Pull(1);

            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, pull.side.items.Select(i => i.tier).ToArray());
            CollectionAssert.AreEqual(new[] { 3, 1 }, pull.main.items.Select(i => i.tier).ToArray());
        }

        // ---------- AC-6 ----------

        [Test]
        public void AC6_NguyenLieuChuaDung_BiBoCuoiNgay()
        {
            var stock = new IngredientStock();
            PullResult pull = Gacha(5).Pull(1);
            stock.AddCrate(pull.side);
            stock.AddCrate(pull.main);
            Assert.AreEqual(5, stock.Count, "3 do phu + 2 chinh");

            Assert.IsTrue(stock.Remove(pull.side.items[0]));
            Assert.AreEqual(4, stock.Count);

            stock.Clear();
            Assert.AreEqual(0, stock.Count);
        }

        // ---------- AC-8 ----------

        [Test]
        public void AC8_MoiThungChiMotLoai()
        {
            IngredientGacha gacha = Gacha(6);
            for (int n = 0; n < 200; n++)
            {
                PullResult pull = gacha.Pull(1);
                Assert.AreEqual(1, pull.side.items.Select(i => i.typeId).Distinct().Count());
                Assert.AreEqual(1, pull.main.items.Select(i => i.typeId).Distinct().Count());
                Assert.AreEqual(pull.side.typeId, pull.side.items[0].typeId);
                Assert.AreEqual(pull.main.typeId, pull.main.items[0].typeId);
            }
        }

        [Test]
        public void AC8_Vong1_ChiRaLoaiDaMoKhoa_VaChonDeu()
        {
            const int N = 30000;
            IngredientGacha gacha = Gacha(7);
            var sides = new Dictionary<string, int>();
            var mains = new Dictionary<string, int>();

            for (int n = 0; n < N; n++)
            {
                PullResult pull = gacha.Pull(1);
                Count(sides, pull.side.typeId);
                Count(mains, pull.main.typeId);
            }

            CollectionAssert.AreEquivalent(Round1Sides, sides.Keys);
            CollectionAssert.AreEquivalent(Round1Mains, mains.Keys);
            foreach (int c in sides.Values)
                Assert.AreEqual(100.0 / 3, 100.0 * c / N, 1.0, "do phu chon deu: xap xi 33,3%");
            // constitution 2.0 muc 4.2: loai chinh chon deu trong cac loai nau duoc voi do phu vua ra (Dieu 3: lech <= 1 diem)
            var lookup = new RecipeLookup(TestData.Recipes);
            foreach (string m in Round1Mains)
            {
                double expected = 0.0;
                foreach (string p in Round1Sides)
                {
                    int fit = Round1Mains.Count(x => lookup.Pairs(x, p));
                    if (lookup.Pairs(m, p)) expected += 1.0 / Round1Sides.Length / fit;
                }
                Assert.AreEqual(100.0 * expected, 100.0 * mains[m] / N, 1.0, m);
            }
        }

        [Test]
        public void AC8_Vong2_MoThemLoai_KhongCoDotPha()
        {
            IngredientGacha gacha = Gacha(8);
            var sides = new HashSet<string>();
            var mains = new HashSet<string>();
            for (int n = 0; n < 2000; n++)
            {
                PullResult pull = gacha.Pull(4);
                sides.Add(pull.side.typeId);
                mains.Add(pull.main.typeId);
            }

            CollectionAssert.AreEquivalent(new[] { "ca_chua", "hanh_tay", "rau_muong", "khoai_tay", "nam_rom" }, sides);
            CollectionAssert.AreEquivalent(new[] { "trung", "heo", "dau_phu", "bo", "tom" }, mains);
        }

        [Test]
        public void AC8_NguyenLieuMangTenVaThoiGianSoCheTuDuLieu()
        {
            var rng = new FixedRandom(0.0, 0.5, 0.5, 0.5, 0.0, 0.5, 0.5);
            PullResult pull = new IngredientGacha(TestData.Balance, TestData.Recipes, rng).Pull(1);

            Assert.AreEqual("ca_chua", pull.side.typeId);
            Assert.AreEqual("Cà chua", pull.side.items[0].name);
            Assert.AreEqual(4.5f, pull.side.items[0].prepSeconds, 1e-6f);
            Assert.IsFalse(pull.side.items[0].prepared);
            Assert.AreEqual("trung", pull.main.typeId);
            Assert.AreEqual("Trứng gà", pull.main.items[0].name);
            Assert.AreEqual(1.5f, pull.main.items[0].prepSeconds, 1e-6f);
        }

        static void Count(Dictionary<string, int> d, string key)
        {
            int c;
            d.TryGetValue(key, out c);
            d[key] = c + 1;
        }
    }
}
