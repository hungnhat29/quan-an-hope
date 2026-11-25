using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-003 AC-1, 2, 3. Nguon: constitution muc 5.3. Tu 2.1 (yeu cau 23/11/2026): moi to hop ra du 6 bac theo luoi 100.000 ve
    /// (bang day du o Sprint11Tests.C05_*); test nay giu cac y cu con dung: diem b, quay theo thanh ti le, nguyen lieu tot day len.
    /// </summary>
    public class DishFormulaTests
    {
        const int Lac = 0, Do = 1, BinhThuong = 2, Ngon = 3, TuyetHao = 4, HuyenThoai = 5;

        static DishFormula Formula()
        {
            return new DishFormula(TestData.Balance);
        }

        static double P(TierChance[] bar, int tier) { return bar.Where(c => c.tier == tier).Sum(c => c.chance); }

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
        public void AC1_QuayTheoLuoiVe_SoNgauNhienNhoRaBacThap()
        {
            DishFormula f = Formula();
            int[] t = f.Tickets(1, 1);
            // ve 0..t0-1 la Lac vi, ke tiep la Do, ...; so ngau nhien sat 1 ra Huyen thoai
            Assert.AreEqual(Lac, f.Roll(1, 1, new FixedRandom(0.0)));
            Assert.AreEqual(Do, f.Roll(1, 1, new FixedRandom((t[0] + 1) / 100000.0)));
            Assert.AreEqual(BinhThuong, f.Roll(1, 1, new FixedRandom(0.5)));
            Assert.AreEqual(HuyenThoai, f.Roll(1, 1, new FixedRandom(0.999999)));
        }

        [Test]
        public void AC1_DuSauBac_TuLacViDenHuyenThoai()
        {
            DishFormula f = Formula();
            TierChance[] best = f.Bar(5, 5), worst = f.Bar(0, 0);
            Assert.AreEqual(6, best.Length);
            Assert.AreEqual(6, worst.Length);
            Assert.AreEqual(0.978, P(best, TuyetHao), 0.001, "nguyen lieu xin nhat: Tuyet hao ~97,8%");
            Assert.AreEqual(0.02, P(best, HuyenThoai), 1e-4, "toi da 2% Huyen thoai khi nau");
            Assert.AreEqual(0.6261, P(worst, Do), 0.0002, "nguyen lieu te nhat: Do 62,61%");
        }

        // ---------- AC-2: vi du trong bang muc 5.3 (2.1) ----------

        [Test]
        public void AC2_Chinh1_Phu1_MauChuan()
        {
            TierChance[] bar = Formula().Bar(1, 1);
            Assert.AreEqual(0.0417, P(bar, Lac), 0.0002);
            Assert.AreEqual(0.1436, P(bar, Do), 0.0002);
            Assert.AreEqual(0.6261, P(bar, BinhThuong), 0.0002);
            Assert.AreEqual(0.1116, P(bar, Ngon), 0.0002);
            Assert.AreEqual(0.0771, P(bar, TuyetHao), 0.0002);
        }

        [Test]
        public void AC2_Chinh2_Phu2_Ngon62()
        {
            Assert.AreEqual(0.6260, P(Formula().Bar(2, 2), Ngon), 0.0002);
        }

        [Test]
        public void AC2_Chinh3_Phu3_TuyetHao81()
        {
            Assert.AreEqual(0.8140, P(Formula().Bar(3, 3), TuyetHao), 0.0002);
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
            TierChance[] bar = f.Bar(1, 1);
            for (int t = 0; t < 6; t++) Assert.AreEqual(100.0 * P(bar, t), 100.0 * counts[t] / N, 0.5, "bac " + t);
            Assert.AreEqual(N, counts.Sum());
        }
    }
}
