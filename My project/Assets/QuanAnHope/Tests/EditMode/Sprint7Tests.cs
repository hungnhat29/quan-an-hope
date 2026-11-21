using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sua truoc sprint 7 (yeu cau 21/11/2026, constitution 1.7): ngay 1 chi co 5 ve; dong ho ngay chi chay tu luc bam Mo quan.
    /// </summary>
    public class Sprint7Tests
    {
        static Balance B { get { return TestData.Balance; } }

        static GameSession NewGame(int seed = 1)
        {
            return new GameSession(B, TestData.Recipes, new SeededRandom(seed), GameSession.LastDayOfGame(B));
        }

        // ---------- "ngay 1 chi co 5 ve" ----------

        [Test]
        public void Truoc7_Ngay1_Co5Ve()
        {
            Assert.AreEqual(5, B.start.firstDayTickets, "so nam o du lieu (Dieu 2)");
            Assert.AreEqual(5, NewGame().Wallet.Count);
        }

        [Test]
        public void Truoc7_Ngay2_CongVeMoiNgay_VeConLaiCongDon()
        {
            GameSession s = NewGame();
            PullResult pull;
            Assert.IsTrue(s.PullCrates(out pull));
            Assert.IsTrue(s.OpenShop());
            Assert.IsTrue(s.CloseEarly());
            Assert.IsTrue(s.StartNextDay());
            Assert.AreEqual(4 + B.ticketsPerDay, s.Wallet.Count);
        }

        // ---------- "khi nao bat dau mo quan thi dong ho moi bat dau chay tu 9h" ----------

        [Test]
        public void Truoc7_DongHo_ChuaMoQuan_DungO9h()
        {
            GameSession s = NewGame();
            s.Tick(120f);
            Assert.AreEqual(0f, s.Clock.Elapsed, 1e-6, "chua mo quan: dong ho khong chay");
            Assert.AreEqual(B.day.openHour + ":00", s.Clock.Text);
            Assert.AreEqual(GamePhase.Prep, s.Phase, "chua mo quan thi khong het ngay");
        }

        [Test]
        public void Truoc7_DongHo_ChayTu9h_KhiMoQuan()
        {
            GameSession s = NewGame();
            s.Tick(200f);
            Assert.IsTrue(s.OpenShop());
            Assert.AreEqual(B.day.openHour + ":00", s.Clock.Text, "mo quan luc 9:00");
            s.Tick(B.day.secondsPerHour);
            Assert.AreEqual((B.day.openHour + 1) + ":00", s.Clock.Text, "mot gio game sau khi mo");
        }

        [Test]
        public void Truoc7_SoCheTruocKhiMo_KhongMatGio()
        {
            GameSession s = NewGame();
            PullResult pull;
            Assert.IsTrue(s.PullCrates(out pull));
            Ingredient side = null;
            foreach (Ingredient i in s.Stock.Items) if (i.kind == IngredientKind.Side) { side = i; break; }
            Assert.IsNotNull(side);
            Assert.IsTrue(s.BeginPrep(side));
            s.HoldPrep(side.prepSeconds + 1f);
            s.Tick(side.prepSeconds + 1f);
            Assert.AreEqual(0f, s.Clock.Elapsed, 1e-6);
        }

        [Test]
        public void Truoc7_ChuNo_DenLuc20h_TinhTuLucMoQuan()
        {
            GameSession s = TestData.SessionAt(B.day.bossDay, 0, 3, GameSession.LastDayOfGame(B));
            s.Tick(B.DaySeconds());
            Assert.IsFalse(s.BossArrived, "chua mo quan: chua toi 20:00");
            Assert.IsTrue(s.OpenShop());
            float to20 = (B.day.bossHour - B.day.openHour) * B.day.secondsPerHour;
            s.Tick(to20 - 1f);
            Assert.IsFalse(s.BossArrived);
            s.Tick(2f);
            Assert.IsTrue(s.BossArrived);
        }
    }
}
