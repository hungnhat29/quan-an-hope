using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-004 AC-1, 2, 3. Nguon: constitution muc 6.2, 6.3, 6.4.</summary>
    public class CustomerTests
    {
        // ---------- AC-1 ----------

        [TestCase(1, 6)]
        [TestCase(2, 6)]
        [TestCase(3, 6)]
        [TestCase(4, 8)]
        public void AC1_SoKhachMoiNgay(int day, int expected)
        {
            Assert.AreEqual(expected, TestData.Balance.CustomersPerDay(day));
        }

        // ---------- AC-2 ----------

        [TestCase(1, 90.0, 10.0, 0.0)]
        [TestCase(2, 75.0, 22.0, 3.0)]
        [TestCase(3, 30.0, 50.0, 20.0)]
        [TestCase(4, 60.0, 32.0, 8.0)]
        public void AC2_DoDeTinh_Quay100000Lan(int round, double easyPct, double midPct, double hardPct)
        {
            const int N = 100000;
            var gen = new CustomerGenerator(TestData.Balance, new SeededRandom(600 + round));
            int easy = 0, mid = 0, hard = 0;

            for (int i = 0; i < N; i++)
            {
                int e = gen.Next(round).easiness;
                if (e >= 60 && e <= 95) easy++;
                else if (e >= 35 && e <= 59) mid++;
                else if (e >= 10 && e <= 34) hard++;
                else Assert.Fail("Do de tinh ngoai moi khoang: " + e);
            }

            Assert.AreEqual(easyPct, 100.0 * easy / N, 1.0, "khoang 60-95");
            Assert.AreEqual(midPct, 100.0 * mid / N, 1.0, "khoang 35-59");
            Assert.AreEqual(hardPct, 100.0 * hard / N, 1.0, "khoang 10-34");
        }

        [Test]
        public void AC2_Vong1_KhongCoKhachKho()
        {
            var gen = new CustomerGenerator(TestData.Balance, new SeededRandom(61));
            for (int i = 0; i < 20000; i++) Assert.GreaterOrEqual(gen.Next(1).easiness, 35);
        }

        [Test]
        public void AC2_GiaTriTraiDeuTrongKhoang_GomCaHaiDau()
        {
            // Khoang de tinh vong 1: 60..95. So ngau nhien thu nhat chon khoang, thu hai chon gia tri.
            Assert.AreEqual(60, new CustomerGenerator(TestData.Balance, new FixedRandom(0.0, 0.0)).Next(1).easiness);
            Assert.AreEqual(95, new CustomerGenerator(TestData.Balance, new FixedRandom(0.0, 0.9999)).Next(1).easiness);
            Assert.AreEqual(35, new CustomerGenerator(TestData.Balance, new FixedRandom(0.95, 0.0)).Next(1).easiness);
            Assert.AreEqual(59, new CustomerGenerator(TestData.Balance, new FixedRandom(0.95, 0.9999)).Next(1).easiness);
        }

        // ---------- AC-3 ----------

        [TestCase(95, 1)]
        [TestCase(80, 1)]
        [TestCase(79, 2)]
        [TestCase(60, 2)]
        [TestCase(59, 3)]
        [TestCase(35, 3)]
        [TestCase(34, 4)]
        [TestCase(10, 4)]
        [TestCase(5, 4)]
        public void AC3_MonToiThieu(int easiness, int expectedTier)
        {
            Assert.AreEqual(expectedTier, TestData.Balance.RequiredTierFor(easiness));
        }
    }
}
