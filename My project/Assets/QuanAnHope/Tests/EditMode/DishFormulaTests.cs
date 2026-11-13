using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-003 AC-1, 2, 3. Nguon: constitution muc 5.3.</summary>
    public class DishFormulaTests
    {
        const int Lac = 0, Do = 1, BinhThuong = 2, Ngon = 3, TuyetHao = 4;

        static DishFormula Formula()
        {
            return new DishFormula(TestData.Balance);
        }

        static void AssertBar(TierChance[] bar, params double[] tierThenChance)
        {
            Assert.AreEqual(tierThenChance.Length / 2, bar.Length, "so o tren thanh ti le");
            for (int i = 0; i < bar.Length; i++)
            {
                Assert.AreEqual((int)tierThenChance[i * 2], bar[i].tier, "bac o o " + i);
                Assert.AreEqual(tierThenChance[i * 2 + 1], bar[i].chance, 1e-6, "ti le o o " + i);
            }
        }

        // ---------- AC-1 ----------

        [Test]
        public void AC1_B_Bang_06Chinh_04Phu_Cong08()
        {
            DishFormula f = Formula();
            Assert.AreEqual(1.8, f.Score(1, 1), 1e-6);
            Assert.AreEqual(2.6, f.Score(3, 0), 1e-6);
            Assert.AreEqual(2.0, f.Score(0, 3), 1e-6);
        }

        [Test]
        public void AC1_XacSuatLenBacTren_BangPhanLe()
        {
            DishFormula f = Formula();
            // b = 1,8 -> len Binh thuong khi so ngau nhien < 0,8
            Assert.AreEqual(BinhThuong, f.Roll(1, 1, new FixedRandom(0.79)));
            Assert.AreEqual(Do, f.Roll(1, 1, new FixedRandom(0.81)));
        }

        [Test]
        public void AC1_KepTrongLacViDenTuyetHao()
        {
            DishFormula f = Formula();
            // chinh 5 + phu 5: b = 5,8 -> van chi Tuyet hao
            AssertBar(f.Bar(5, 5), TuyetHao, 1.0);
            Assert.AreEqual(TuyetHao, f.Roll(5, 5, new FixedRandom(0.5)));
            // chinh 0 + phu 0: b = 0,8 -> 20% Lac vi, 80% Do
            AssertBar(f.Bar(0, 0), Lac, 0.2, Do, 0.8);
        }

        // ---------- AC-2: nam vi du trong bang muc 5.3 ----------

        [Test]
        public void AC2_Chinh1_Phu1_20Do_80BinhThuong()
        {
            AssertBar(Formula().Bar(1, 1), Do, 0.2, BinhThuong, 0.8);
        }

        [Test]
        public void AC2_Chinh0_Phu1_80Do_20BinhThuong()
        {
            AssertBar(Formula().Bar(0, 1), Do, 0.8, BinhThuong, 0.2);
        }

        [Test]
        public void AC2_Chinh2_Phu2_20BinhThuong_80Ngon()
        {
            AssertBar(Formula().Bar(2, 2), BinhThuong, 0.2, Ngon, 0.8);
        }

        [Test]
        public void AC2_Chinh3_Phu3_20Ngon_80TuyetHao()
        {
            AssertBar(Formula().Bar(3, 3), Ngon, 0.2, TuyetHao, 0.8);
        }

        [Test]
        public void AC2_Chinh4_Phu2_100TuyetHao()
        {
            AssertBar(Formula().Bar(4, 2), TuyetHao, 1.0);
        }

        // ---------- AC-3 ----------

        [Test]
        public void AC3_Quay100000Lan_KhopThanhTiLe()
        {
            const int N = 100000;
            DishFormula f = Formula();
            var rng = new SeededRandom(53);
            var counts = new int[6];
            for (int i = 0; i < N; i++) counts[f.Roll(1, 1, rng)]++;

            Assert.AreEqual(20.0, 100.0 * counts[Do] / N, 1.0);
            Assert.AreEqual(80.0, 100.0 * counts[BinhThuong] / N, 1.0);
            Assert.AreEqual(N, counts[Do] + counts[BinhThuong]);
        }
    }
}
