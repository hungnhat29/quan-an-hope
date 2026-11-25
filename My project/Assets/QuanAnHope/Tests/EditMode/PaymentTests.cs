using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-004 AC-4, 5, 6, 7. Nguon: constitution muc 5.2, 6.5.
    /// Vong 1: gia goc 60G. He so bac: Lac vi 0,2 / Do 0,5 / Binh thuong 1,0 / Ngon 1,8 / Tuyet hao 3,2.
    /// Tien lam tron den 1G (Q-12).
    /// </summary>
    public class PaymentTests
    {
        static PaymentResult Pay(int easiness, int dishTier, int round = 1)
        {
            return Payment.Settle(TestData.Balance, TestData.Balance.BasePrice(round), easiness, dishTier);
        }

        // ---------- AC-7 ----------

        [TestCase(0, 12)]
        [TestCase(1, 30)]
        [TestCase(2, 60)]
        [TestCase(3, 108)]
        [TestCase(4, 192)]
        [TestCase(5, 360)]
        public void AC7_GiaVong1_Bang60NhanHeSoBac(int tier, int expectedPrice)
        {
            Assert.AreEqual(expectedPrice, Payment.Price(TestData.Balance, 60, tier));
        }

        [Test]
        public void AC7_GiaGocTheoVong()
        {
            Assert.AreEqual(70, Payment.Price(TestData.Balance, 70, 2));
            Assert.AreEqual(85, Payment.Price(TestData.Balance, 85, 2));
            Assert.AreEqual(100, Payment.Price(TestData.Balance, 100, 2));
        }

        // ---------- AC-4: chenh >= 0 -> gia + tip = gia x kho x (0,3 + 0,2 x chenh) ----------

        [Test]
        public void AC4_DungYeuCau_KhachDe_TipIt()
        {
            // De tinh 85% -> toi thieu Do. Mon Do: gia 30, kho 0,15, tip = 30 x 0,15 x 0,3 = 1,35 -> 1
            PaymentResult r = Pay(85, 1);
            Assert.AreEqual(PaymentOutcome.Satisfied, r.outcome);
            Assert.AreEqual(0, r.diff);
            Assert.AreEqual(30, r.price);
            Assert.AreEqual(1, r.tip);
            Assert.AreEqual(31, r.amount);
        }

        [Test]
        public void AC4_VuotYeuCau_TipTangTheoChenh()
        {
            // De tinh 85% -> toi thieu Do. Mon Ngon: chenh 2, gia 108, tip = 108 x 0,15 x 0,7 = 11,34 -> 11
            PaymentResult r = Pay(85, 3);
            Assert.AreEqual(2, r.diff);
            Assert.AreEqual(108, r.price);
            Assert.AreEqual(11, r.tip);
            Assert.AreEqual(119, r.amount);
        }

        [Test]
        public void AC4_KhachKhoTinhHon_TipNhieuHon()
        {
            // De tinh 50% -> toi thieu Ngon. Mon Ngon: gia 108, kho 0,5, tip = 108 x 0,5 x 0,3 = 16,2 -> 16
            PaymentResult ngon = Pay(50, 3);
            Assert.AreEqual(16, ngon.tip);
            Assert.AreEqual(124, ngon.amount);

            // Mon Tuyet hao: chenh 1, gia 192, tip = 192 x 0,5 x 0,5 = 48
            PaymentResult tuyetHao = Pay(50, 4);
            Assert.AreEqual(48, tuyetHao.tip);
            Assert.AreEqual(240, tuyetHao.amount);
        }

        // ---------- AC-5: chenh = -1 -> 50% gia, khong tip ----------

        [Test]
        public void AC5_ThapHon1Bac_TraNuaGia_KhongTip()
        {
            // De tinh 70% -> toi thieu Binh thuong. Mon Do: gia 30 -> tra 15
            PaymentResult r = Pay(70, 1);
            Assert.AreEqual(PaymentOutcome.Disappointed, r.outcome);
            Assert.AreEqual(-1, r.diff);
            Assert.AreEqual(0, r.tip);
            Assert.AreEqual(15, r.amount);
        }

        // ---------- AC-6: chenh <= -2 -> khong tra, bi tru 30% gia goc ----------

        [Test]
        public void AC6_ThapHon2Bac_BiTru30PhanTramGiaGoc()
        {
            // De tinh 50% -> toi thieu Ngon. Mon Do: chenh -2 -> -0,3 x 60 = -18
            PaymentResult r = Pay(50, 1);
            Assert.AreEqual(PaymentOutcome.Angry, r.outcome);
            Assert.AreEqual(-2, r.diff);
            Assert.AreEqual(0, r.tip);
            Assert.AreEqual(-18, r.amount);
        }

        [Test]
        public void AC6_ThapHon3Bac_VanTruNhuVay()
        {
            Assert.AreEqual(-18, Pay(50, 0).amount);
        }

        [Test]
        public void AC6_TienDenBuTinhTheoGiaGocCuaVong()
        {
            // Vong 2: gia goc 70 -> -21
            Assert.AreEqual(-21, Pay(50, 1, round: 2).amount);
        }
    }
}
