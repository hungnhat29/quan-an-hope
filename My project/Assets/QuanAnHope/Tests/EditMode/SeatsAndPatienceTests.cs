using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-030 Ba khach cung ngoi, F-031 Gio cho. Nguon: constitution 1.4 muc 6.1, 6.2, 6.5, 6.6, 6.7, 7.3;
    /// docs/sprint-4/impl-plan.md muc 2 (F-030 AC 1-4, F-031 AC 1-4).
    /// </summary>
    public class SeatsAndPatienceTests
    {
        static Balance B { get { return TestData.Balance; } }
        static float Gap { get { return B.day.nextCustomerSeconds; } }

        static GameSession OpenDay(int day, int seed = 1)
        {
            GameSession s = TestData.SessionAt(day, 0, seed);
            Assert.IsTrue(s.OpenShop());
            return s;
        }

        static int Seated(GameSession s)
        {
            return s.Seats.Count(c => c != null);
        }

        // ---------- F-030 AC-1: toi da 3 ghe, ghe trong 3 giay sau co khach ----------

        [Test]
        public void F030_AC1_Vong1_MotGhe_Vong2_BaGhe()
        {
            Assert.AreEqual(1, TestData.SessionAt(1).SeatCount);
            Assert.AreEqual(1, TestData.SessionAt(3).SeatCount);
            Assert.AreEqual(3, TestData.SessionAt(4).SeatCount);
            Assert.AreEqual(3, TestData.SessionAt(8).SeatCount);
        }

        [Test]
        public void F030_AC1_MoQuan_KhachDauVaoNgay_HaiGheConLai3GiaySau()
        {
            GameSession s = OpenDay(4);
            Assert.AreEqual(1, Seated(s));
            s.Tick(Gap - 0.01f);
            Assert.AreEqual(1, Seated(s));
            s.Tick(0.02f);
            Assert.AreEqual(3, Seated(s), "ghe trong: 3 giay sau co khach moi (muc 6.2)");
            Assert.AreEqual(3, s.CustomersToday);
        }

        [Test]
        public void F030_AC1_GheVuaTrong_3GiaySauCoKhachMoi()
        {
            GameSession s = OpenDay(4);
            s.Tick(Gap + 0.01f);
            Customer middle = s.Seats[1];
            s.Counter.TryPlace(TestData.DishOf(middle.orderId, 2));
            Assert.IsNotNull(s.ServeSeat(1, s.Counter.Dishes[0]));
            Assert.IsNull(s.Seats[1]);
            Assert.AreEqual(2, Seated(s));
            s.Tick(Gap - 0.01f);
            Assert.IsNull(s.Seats[1]);
            s.Tick(0.02f);
            Assert.IsNotNull(s.Seats[1]);
            Assert.AreNotSame(middle, s.Seats[1]);
        }

        [Test]
        public void F030_AC1_Vong1_VanLanLuot()
        {
            GameSession s = OpenDay(1);
            s.Tick(Gap * 5);
            Assert.AreEqual(1, Seated(s), "vong 1 khach den lan luot (muc 6.1)");
        }

        // ---------- F-030 AC-2: moi khach mot bong bong ----------

        [Test]
        public void F030_AC2_MoiKhach_CoDoDeTinh_MonGoi_GioCho()
        {
            GameSession s = OpenDay(4);
            s.Tick(Gap + 0.01f);
            foreach (Customer c in s.Seats)
            {
                Assert.IsNotNull(c.orderId);
                Assert.IsNotNull(c.orderName);
                Assert.That(c.easiness, Is.InRange(10, 95));
                Assert.That(s.PatienceLeft(c), Is.InRange(0f, 1f));
            }
            Assert.AreEqual(1f, s.PatienceLeft(s.Seats[2]), 1e-3, "khach vua ngoi: thanh kien nhan gan day");
        }

        // ---------- F-030 AC-3: dung khach nhan dia ----------

        [Test]
        public void F030_AC3_PhucVuKhachGiua_DungMon_TraTienTheoKhachGiua()
        {
            GameSession s = OpenDay(4, 3);
            s.Tick(Gap + 0.01f);
            Customer left = s.Seats[0], middle = s.Seats[1], right = s.Seats[2];
            s.Counter.TryPlace(TestData.DishOf(middle.orderId, 3));
            PaymentResult expected = Payment.Settle(B, 2, s.EasinessNow(middle), 3, s.Relics.Effects);
            PaymentResult paid = s.ServeSeat(1, s.Counter.Dishes[0]);
            Assert.IsFalse(paid.wrongOrder);
            Assert.AreEqual(expected.amount, paid.amount);
            Assert.AreSame(left, s.Seats[0]);
            Assert.AreSame(right, s.Seats[2]);
            Assert.AreEqual(0, s.Counter.Dishes.Count);
        }

        [Test]
        public void F030_AC3_DiaKhacMonKhachDoGoi_LaSaiMon()
        {
            GameSession s = OpenDay(4, 5);
            s.Tick(Gap + 0.01f);
            Customer c = s.Seats[2];
            string other = s.BookRecipes().First(r => r.id != c.orderId).id;
            s.Counter.TryPlace(TestData.DishOf(other, 4));
            PaymentResult paid = s.ServeSeat(2, s.Counter.Dishes[0]);
            Assert.IsTrue(paid.wrongOrder);
            Assert.AreEqual(-Payment.WrongOrder(B, 2).amount, -paid.amount);
        }

        [Test]
        public void F030_AC3_GheTrong_KhongPhucVuDuoc()
        {
            GameSession s = OpenDay(4);
            s.Counter.TryPlace(TestData.DishOf(s.Seats[0].orderId, 2));
            Assert.IsNull(s.ServeSeat(2, s.Counter.Dishes[0]), "ghe 3 chua co khach");
            Assert.AreEqual(1, s.Counter.Dishes.Count);
        }

        // ---------- F-030 AC-4: 21:00 ----------

        [Test]
        public void F030_AC4_Sau21Gio_KhongThemKhach_PhucVuHetThiHetNgay()
        {
            GameSession s = OpenDay(4);
            s.Tick(Gap + 0.01f);
            s.Tick(B.DaySeconds());
            Assert.AreEqual(GamePhase.Open, s.Phase);
            Assert.IsTrue(s.IsClosedForNewCustomers);
            for (int seat = 0; seat < 3; seat++)
            {
                s.Counter.TryPlace(TestData.DishOf(s.Seats[seat].orderId, 2));
                s.ServeSeat(seat, s.Counter.Dishes[0]);
                s.Tick(Gap * 2);
                if (seat < 2) Assert.AreEqual(GamePhase.Open, s.Phase, "con khach ngoi: chua het ngay");
            }
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
            Assert.AreEqual(3, s.CustomersToday);
        }

        // ---------- F-031 AC-1..4: gio cho ----------

        [Test]
        public void F031_AC1_Vong2_An90Giay_Giam1Diem3Giay_ToiDa20()
        {
            Assert.AreEqual(90f, Patience.SafeSeconds(B, 4, RelicFx.None), 1e-4);
            Assert.AreEqual(0, Patience.Drop(B, 4, 90f, RelicFx.None));
            Assert.AreEqual(0, Patience.Drop(B, 4, 92.9f, RelicFx.None));
            Assert.AreEqual(1, Patience.Drop(B, 4, 93f, RelicFx.None));
            Assert.AreEqual(10, Patience.Drop(B, 4, 120f, RelicFx.None));
            Assert.AreEqual(20, Patience.Drop(B, 4, 150f, RelicFx.None));
            Assert.AreEqual(20, Patience.Drop(B, 4, 1000f, RelicFx.None));
        }

        [Test]
        public void F031_AC1_KhachNgoiLau_DoDeTinhLucNayGiam()
        {
            GameSession s = OpenDay(4);
            Customer c = s.Seats[0];
            s.Tick(120f);
            Assert.AreEqual(120f, c.waitedSeconds, 1e-3);
            Assert.AreEqual(System.Math.Max(1, c.easiness - 10), s.EasinessNow(c));
            Assert.Less(s.PatienceLeft(c), 0.5f);
        }

        [Test]
        public void F031_AC2_Vong1_KhongTinhGio_Ngay8An110Giay()
        {
            Assert.AreEqual(0, Patience.Drop(B, 1, 5000f, RelicFx.None));
            Assert.AreEqual(0, Patience.Drop(B, 3, 5000f, RelicFx.None));
            Assert.AreEqual(110f, Patience.SafeSeconds(B, 8, RelicFx.None), 1e-4);
            Assert.AreEqual(0, Patience.Drop(B, 8, 110f, RelicFx.None));
            Assert.AreEqual(1, Patience.Drop(B, 8, 113f, RelicFx.None));

            GameSession s = OpenDay(1);
            Customer c = s.CurrentCustomer;
            s.Tick(400f);
            Assert.AreEqual(c.easiness, s.EasinessNow(c));
            Assert.AreEqual(1f, s.PatienceLeft(c), 1e-6);
        }

        [Test]
        public void F031_AC3_TienVaBacToiThieu_TheoDoDeTinhLucPhucVu()
        {
            GameSession s = OpenDay(4, 7);
            Customer c = s.Seats[0];
            s.Tick(150f); // -20 diem
            int now = s.EasinessNow(c);
            Assert.AreEqual(System.Math.Max(1, c.easiness - 20), now);
            s.Counter.TryPlace(TestData.DishOf(c.orderId, 3));
            PaymentResult expected = Payment.Settle(B, 2, now, 3, s.Relics.Effects);
            PaymentResult paid = s.ServeSeat(0, s.Counter.Dishes[0]);
            Assert.AreEqual(expected.amount, paid.amount);
            Assert.AreEqual(expected.diff, paid.diff);
        }

        [Test]
        public void F031_AC4_DongHoCat40_MayPhaCaPhe60()
        {
            var inv = new RelicInventory(B);
            inv.Add(B.relics.Single(r => r.id == "F3"));
            Assert.AreEqual(126f, Patience.SafeSeconds(B, 4, inv.Effects), 1e-3);
            var inv2 = new RelicInventory(B);
            inv2.Add(B.relics.Single(r => r.id == "F7"));
            Assert.AreEqual(144f, Patience.SafeSeconds(B, 4, inv2.Effects), 1e-3);
            Assert.AreEqual(0, Patience.Drop(B, 4, 140f, inv2.Effects));
        }

        [Test]
        public void F031_AC4_RelicGioCho_DaCoTacDung()
        {
            CollectionAssert.DoesNotContain(RelicKeys.NotYetActive, RelicKeys.Patience);
        }
    }
}
