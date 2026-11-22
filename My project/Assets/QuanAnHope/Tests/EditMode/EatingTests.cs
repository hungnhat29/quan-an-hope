using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Khach an tai ghe (constitution 1.5 muc 6.2, truoc sprint 6 theo yeu cau 18/11/2026):
    /// phuc vu xong khach ngoi an <c>eat_s</c> giay roi moi roi ghe; tien tra luc phuc vu.
    /// </summary>
    public class EatingTests
    {
        static Balance B { get { return TestData.Balance; } }

        static GameSession OpenDay(int day, int seed = 1)
        {
            GameSession s = TestData.SessionAt(day, 0, seed);
            Assert.IsTrue(s.OpenShop());
            return s;
        }

        static PaymentResult ServeRight(GameSession s, int seat, int tier = 2)
        {
            Dish d = TestData.DishOf(s.Seats[seat].orderId ?? "trung__ca_chua", tier);
            Assert.IsTrue(s.Counter.TryPlace(d));
            return s.ServeSeat(seat, d);
        }

        [Test]
        public void SoGiayAn_LayTuDuLieu_5Giay()
        {
            Assert.AreEqual(5f, B.day.eatSeconds, 1e-6, "balance.json day.eat_s (constitution 1.5 muc 6.2)");
        }

        [Test]
        public void PhucVu_KhachNgoiAn_TienTraNgay_GheChuaTrong()
        {
            GameSession s = OpenDay(1);
            Customer c = s.Seats[0];
            int money = s.Money;
            PaymentResult paid = ServeRight(s, 0);
            Assert.IsNotNull(paid);
            Assert.AreEqual(money + paid.amount, s.Money, "tra tien luc phuc vu, khong doi an xong");
            Assert.IsNull(s.Seats[0], "khach dang an khong con goi mon");
            Assert.AreSame(c, s.Eaters[0]);
            Assert.IsNotNull(s.EatingDish(0));
            Assert.IsTrue(s.AnyoneEating);
        }

        [Test]
        public void TienDoAn_Tu0Toi1_AnXongThiRoiGhe()
        {
            GameSession s = OpenDay(1);
            ServeRight(s, 0);
            Assert.AreEqual(0f, s.EatProgress(0), 1e-6);
            s.Tick(B.day.eatSeconds * 0.5f);
            Assert.AreEqual(0.5f, s.EatProgress(0), 1e-4);
            Assert.IsNotNull(s.Eaters[0]);
            s.Tick(B.day.eatSeconds * 0.5f + 0.01f);
            Assert.IsNull(s.Eaters[0], "an xong roi ghe");
            Assert.IsNull(s.EatingDish(0));
            Assert.AreEqual(0f, s.EatProgress(0), 1e-6);
        }

        [Test]
        public void KhachMoi_VaoSauKhiAnXong_Cong3Giay()
        {
            GameSession s = OpenDay(1);
            ServeRight(s, 0);
            s.Tick(B.day.eatSeconds + B.day.nextCustomerSeconds - 0.05f);
            Assert.IsNull(s.Seats[0], "chua toi luc khach moi vao");
            s.Tick(0.1f);
            Assert.IsNotNull(s.Seats[0]);
            Assert.AreEqual(2, s.CustomersToday);
        }

        [Test]
        public void KhachDangAn_KhongNhanDiaThuHai()
        {
            GameSession s = OpenDay(1);
            ServeRight(s, 0);
            Dish second = TestData.DishOf("trung__ca_chua", 3);
            s.Counter.TryPlace(second);
            Assert.IsNull(s.ServeSeat(0, second), "ghe dang an khong phuc vu duoc");
            Assert.AreEqual(1, s.Counter.Dishes.Count, "dia van tren quay");
        }

        [Test]
        public void Vong2_MotKhachAn_HaiGheKiaVanPhucVuDuoc()
        {
            GameSession s = OpenDay(4);
            s.Tick(2f * s.ArrivalGap + 0.01f);   // 1.8: khach vao cach nhau ArrivalGap
            Assert.AreEqual(3, s.Seats.Count(c => c != null));
            ServeRight(s, 1);
            Assert.IsNotNull(s.Eaters[1]);
            Assert.IsNotNull(ServeRight(s, 0), "ghe khac van nhan mon trong luc ghe giua an");
            Assert.IsNotNull(s.Eaters[0]);
            Assert.IsNotNull(s.Seats[2]);
        }

        [Test]
        public void DongQuanSom_KhachDangAn_RaVe()
        {
            GameSession s = OpenDay(1);
            ServeRight(s, 0);
            Assert.IsTrue(s.CloseEarly());
            Assert.IsFalse(s.AnyoneEating);
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
        }

        [Test]
        public void Bot_ChoiDuocNgayCoKhachAn()
        {
            GameSession s = new GameSession(B, TestData.Recipes, new SeededRandom(3), 3);
            NaiveBot.PlayDay(s, BotTiming.Naive);
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
            Assert.Greater(s.ServedToday, 0);
        }
    }
}
