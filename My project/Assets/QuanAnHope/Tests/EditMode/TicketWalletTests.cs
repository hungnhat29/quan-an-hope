using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>F-002 AC-1, AC-2 (phan ve); F-010 AC-2. Nguon: constitution 1.4 muc 3, 4.2.</summary>
    public class TicketWalletTests
    {
        [Test]
        public void AC1_Ngay1_Co5Ve() // constitution 1.7 muc 3 (cu: 8 ve)
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            Assert.AreEqual(5, wallet.Count, "ngay 1 chi co 5 ve");
        }

        [Test]
        public void AC1_MoiNgaySau_Cong6Ve_VeThuaCongDon()
        {
            var wallet = new TicketWallet(TestData.Balance);
            wallet.BeginDay(1);
            Assert.IsTrue(wallet.TrySpend());
            Assert.IsTrue(wallet.TrySpend());

            wallet.BeginDay(2);
            Assert.AreEqual(3 + TestData.Balance.ticketsPerDay, wallet.Count, "con 3 ve thua + ve moi ngay");

            wallet.BeginDay(3);
            Assert.AreEqual(3 + 2 * TestData.Balance.ticketsPerDay, wallet.Count);
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
