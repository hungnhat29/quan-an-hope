using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-002 AC-1, AC-2 (phan ve); F-010 AC-2. Nguon: constitution 1.4 muc 3, 4.2.</summary>
    public class TicketWalletTests
    {
        [Test]
        public void AC1_Ngay1_Co8Ve()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            Assert.AreEqual(8, wallet.Count, "6 ve moi ngay + 2 ve thuong");
        }

        [Test]
        public void AC1_MoiNgaySau_Cong6Ve_VeThuaCongDon()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            Assert.IsTrue(wallet.TrySpend());
            Assert.IsTrue(wallet.TrySpend());

            wallet.BeginDay(2);
            Assert.AreEqual(12, wallet.Count, "con 6 ve thua + 6 ve moi");

            wallet.BeginDay(3);
            Assert.AreEqual(18, wallet.Count);
        }

        [Test]
        public void AC2_HetVe_KhongTieuDuoc()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            for (int i = 0; i < 8; i++) Assert.IsTrue(wallet.TrySpend());

            Assert.IsFalse(wallet.TrySpend());
            Assert.AreEqual(0, wallet.Count);
        }
    }
}
