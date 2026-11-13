using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-002 AC-1, AC-2 (phan ve). Nguon: constitution muc 3, 4.2.</summary>
    public class TicketWalletTests
    {
        [Test]
        public void AC1_Ngay1_Co5Ve()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            Assert.AreEqual(5, wallet.Count);
        }

        [Test]
        public void AC1_MoiNgaySau_Cong3Ve_VeThuaCongDon()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            Assert.IsTrue(wallet.TrySpend());
            Assert.IsTrue(wallet.TrySpend());

            wallet.BeginDay(2);
            Assert.AreEqual(6, wallet.Count, "con 3 ve thua + 3 ve moi");

            wallet.BeginDay(3);
            Assert.AreEqual(9, wallet.Count);
        }

        [Test]
        public void AC2_HetVe_KhongTieuDuoc()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            for (int i = 0; i < 5; i++) Assert.IsTrue(wallet.TrySpend());

            Assert.IsFalse(wallet.TrySpend());
            Assert.AreEqual(0, wallet.Count);
        }
    }
}
