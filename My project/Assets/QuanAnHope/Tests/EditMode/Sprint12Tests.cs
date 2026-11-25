using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>Yeu cau 24/11/2026 (spec specs/sprint-10c/spec.md, constitution 2.2): tay van nang (D-06), relic ngay 2 (D-05).</summary>
    public class Sprint12Tests
    {
        static Balance B { get { return TestData.Balance; } }
        static RecipeBook R { get { return TestData.Recipes; } }

        static GameSession NewGame(int seed = 1)
        {
            return new GameSession(B, R, new SeededRandom(seed), GameSession.LastDayOfGame(B));
        }

        [Test]
        public void D06_TayVanNang_600G_MuaMotLan_LuuTai()
        {
            Assert.AreEqual(600, B.glovePrice);
            GameSession s = NewGame();
            s.DebugAddMoney(5000);
            Assert.AreEqual(ItemBuyResult.Closed, s.BuyGlove(), "mo cung cua hang (ngay 4)");
            s.DebugJumpToDay(4);
            int money = s.Money;
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyGlove());
            Assert.AreEqual(money - 600, s.Money);
            Assert.IsTrue(s.Items.HasGlove);
            Assert.AreEqual(ItemBuyResult.AtLimit, s.BuyGlove(), "chi mua 1 lan");
            GameSession back = GameSession.FromSave(B, R, s.Snapshot());
            Assert.IsTrue(back.Items.HasGlove, "luu trong ban luu");
        }

        [Test]
        public void D05_Ngay2_SauMienPhi_DoiHangVanDuoc()
        {
            GameSession s = NewGame(5);
            Assert.IsTrue(s.OpenShop());
            Assert.IsTrue(s.CloseEarly());
            Assert.IsTrue(s.StartNextDay());
            Assert.IsTrue(s.Shop.IsFreeDay);
            Assert.AreEqual(RelicBuyResult.Bought, s.BuyRelic(0));
            s.DebugAddMoney(500);
            Assert.AreEqual(RerollResult.Rerolled, s.RerollShop(), "het mien phi: cua hang nhu ngay thuong");
            for (int i = 0; i < s.Shop.Offers.Count; i++)
                if (s.Shop.Offers[i] != null) Assert.AreEqual(s.Shop.Offers[i].price, s.Shop.Price(i));
        }
    }
}
