using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-011 Ngay theo gio, khach khong gioi han. Nguon: constitution 1.4 muc 2, 6.1, 6.2;
    /// docs/sprint-2/impl-plan.md muc 3 (F-011 AC 1-7).
    /// </summary>
    public class DayClockTests
    {
        const int LastDayOfRound1 = 3;

        static GameSession NewSession(int seed = 1)
        {
            return new GameSession(TestData.Balance, TestData.Recipes, new SeededRandom(seed), LastDayOfRound1);
        }

        static float Hour { get { return TestData.Balance.day.secondsPerHour; } }
        static float DaySeconds { get { return TestData.Balance.DaySeconds(); } }
        static float Gap { get { return TestData.Balance.day.nextCustomerSeconds; } }
        static float Eat { get { return TestData.Eat; } }

        /// <summary>Phuc vu khach dang ngoi bang dung mon khach goi (bot nau khong tinh gio).</summary>
        static PaymentResult ServeCurrent(GameSession s)
        {
            Assert.IsTrue(NaiveBot.CookFor(s, s.CurrentCustomer.orderId), "seed nay phai nau duoc mon goi");
            int index = s.Counter.Dishes.ToList().FindIndex(d => d.recipeId == s.CurrentCustomer.orderId);
            return s.Serve(index);
        }

        // ---------- AC-1: dong ho ----------

        [Test]
        public void GioThuc_ChoTroiDoiMau_KhongLamTron()
        {
            var clock = new DayClock(TestData.Balance.day);
            Assert.AreEqual(9f, clock.HourOfDay, 1e-5f);
            clock.Advance(Hour * 2.5f);
            Assert.AreEqual(11.5f, clock.HourOfDay, 1e-4f);
            Assert.AreEqual("11:30", clock.Text);
            clock.Advance(DaySeconds);
            Assert.AreEqual(23.5f, clock.HourOfDay, 1e-3f, "van chay qua gio dong cua");
        }

        [Test]
        public void AC1_DauNgay_9Gio_40GiayLa1Gio()
        {
            var clock = new DayClock(TestData.Balance.day);
            Assert.AreEqual("9:00", clock.Text);
            clock.Advance(Hour);
            Assert.AreEqual("10:00", clock.Text);
            clock.Advance(Hour * 3 + Hour / 3f);           // 13:20 nhu kich ban 10 giay
            Assert.AreEqual("13:20", clock.Text);
            Assert.AreEqual(13, clock.Hour);
            Assert.AreEqual(20, clock.Minute);
            Assert.IsFalse(clock.IsPastClose);
        }

        [Test]
        public void AC1_DongHo_HienGio2ChuSo_PhutLamTronXuong()
        {
            var clock = new DayClock(TestData.Balance.day);
            clock.Advance(Hour * 5 + Hour / 2f - 0.01f);    // 14:29,99
            Assert.AreEqual("14:29", clock.Text);
            clock.Advance(0.02f);
            Assert.AreEqual("14:30", clock.Text);
        }

        [Test]
        public void AC1_21Gio_SauDungMotNgay()
        {
            var clock = new DayClock(TestData.Balance.day);
            clock.Advance(DaySeconds - 0.01f);
            Assert.IsFalse(clock.IsPastClose);
            clock.Advance(0.01f);
            Assert.IsTrue(clock.IsPastClose);
            Assert.AreEqual("21:00", clock.Text);
        }

        [Test]
        public void AC1_PhienChoi_DongHoChayTheoTick()
        {
            GameSession s = NewSession();
            Assert.AreEqual("9:00", s.Clock.Text);
            s.Tick(Hour * 2);
            Assert.AreEqual("11:00", s.Clock.Text);
        }

        // ---------- AC-2: chua mo quan ----------

        [Test]
        public void AC2_ChuaMoQuan_GioVanTroi_KhongCoKhach()
        {
            GameSession s = NewSession();
            s.Tick(Hour * 4);
            Assert.AreEqual(GamePhase.Prep, s.Phase);
            Assert.AreEqual("13:00", s.Clock.Text);
            Assert.IsNull(s.CurrentCustomer);
            Assert.AreEqual(0, s.CustomersToday);
        }

        // ---------- AC-3: khach lien tuc, khong gioi han ----------

        [Test]
        public void AC3_PhucVuXong_KhachKeTiepVaoSau3Giay()
        {
            GameSession s = NewSession(5);
            Assert.IsTrue(s.OpenShop());
            Customer first = s.CurrentCustomer;
            Assert.IsNotNull(first, "mo quan thi khach dau tien vao ngay");

            Assert.IsNotNull(ServeCurrent(s));
            Assert.AreEqual(GamePhase.Open, s.Phase);
            Assert.IsNull(s.CurrentCustomer, "khach dang an: khong goi mon nua");
            Assert.AreSame(first, s.Eaters[0], "khach vua phuc vu ngoi an tai ghe (constitution 1.5 muc 6.2)");

            s.Tick(Eat);                                   // an xong, roi ghe
            Assert.IsNull(s.Eaters[0]);
            s.Tick(Gap - 0.1f);
            Assert.IsNull(s.CurrentCustomer);
            s.Tick(0.2f);
            Assert.IsNotNull(s.CurrentCustomer);
            Assert.AreNotSame(first, s.CurrentCustomer);
            Assert.IsFalse(string.IsNullOrEmpty(s.CurrentCustomer.orderId), "khach moi goi mon (muc 6.7)");
            Assert.AreEqual(2, s.CustomersToday);
        }

        [Test]
        public void AC3_KhongGioiHanSoKhach_Qua10Khach()
        {
            GameSession s = NewSession(11);
            PullResult pull;
            while (s.PullCrates(out pull)) { }           // 8 ve: 16 nguyen lieu chinh
            s.OpenShop();
            for (int i = 0; i < 12; i++)
            {
                Assert.AreEqual(GamePhase.Open, s.Phase, "khach thu " + (i + 1));
                Assert.IsNotNull(s.CurrentCustomer, "khach thu " + (i + 1));
                Assert.IsNotNull(ServeCurrent(s));
                s.Tick(Eat + Gap);
            }
            Assert.AreEqual(12, s.ServedToday);
            Assert.AreEqual(13, s.CustomersToday);
        }

        [Test]
        public void AC3_ChuaMoQuan_KhongPhucVuDuoc()
        {
            GameSession s = NewSession();
            Assert.IsNull(s.CurrentCustomer);
            Assert.IsTrue(NaiveBot.CookOne(s));
            Assert.IsNull(s.Serve(0));
            Assert.AreEqual(1, s.Counter.Dishes.Count);
        }

        // ---------- AC-4: 21:00 dang co khach ----------

        [Test]
        public void AC4_21Gio_CoKhachNgoi_VanPhucVu_XongThiHetNgay()
        {
            GameSession s = NewSession(5);
            s.OpenShop();
            s.Tick(DaySeconds + 1f);

            Assert.AreEqual(GamePhase.Open, s.Phase, "khach dang ngoi van cho mon");
            Assert.IsTrue(s.IsClosedForNewCustomers);
            Assert.IsNotNull(s.CurrentCustomer);

            Assert.IsNotNull(ServeCurrent(s));
            Assert.AreEqual(GamePhase.Open, s.Phase, "khach cuoi dang an: ngay chua het");
            s.Tick(Eat);

            Assert.AreEqual(GamePhase.DayEnd, s.Phase, "khach cuoi an xong thi het ngay");
            Assert.IsNull(s.CurrentCustomer);
            Assert.AreEqual(1, s.ServedToday);
            Assert.AreEqual(0, s.Stock.Count, "het ngay bo nguyen lieu (muc 4.2)");
            Assert.AreEqual(0, s.Counter.Dishes.Count, "het ngay bo dia tren quay cho");
        }

        [Test]
        public void AC4_KhachVaoTruoc21Gio_SauDoKhongCoKhachMoi()
        {
            GameSession s = NewSession(5);
            s.OpenShop();
            s.Tick(DaySeconds - 1f);                     // 20:58,5
            ServeCurrent(s);
            s.Tick(Eat + Gap);                           // an xong qua 21:00, khach ke tiep le ra vao sau do
            Assert.AreEqual(GamePhase.DayEnd, s.Phase, "21:00 trong luc cho khach ke tiep -> het ngay");
            Assert.AreEqual(1, s.CustomersToday);
        }

        // ---------- AC-5: 21:00 khong co khach ----------

        [Test]
        public void AC5_21Gio_ChuaMoQuan_HetNgay()
        {
            GameSession s = NewSession();
            s.Tick(DaySeconds);
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
            Assert.AreEqual(0, s.ServedToday);
        }

        [Test]
        public void AC5_21Gio_DangChoKhachKeTiep_HetNgayNgay()
        {
            GameSession s = NewSession(5);
            s.OpenShop();
            s.Tick(DaySeconds - 1f);
            ServeCurrent(s);                             // 20:58,5: khach an toi qua 21:00
            s.Tick(1f);                                  // dung 21:00
            Assert.AreEqual(GamePhase.Open, s.Phase, "khach con dang an");
            s.Tick(Eat - 1f);                            // an xong
            Assert.AreEqual(GamePhase.DayEnd, s.Phase, "an xong la het ngay, khong doi them 3 giay");
            var expect = new DayClock(TestData.Balance.day);
            expect.Advance(DaySeconds - 1f + Eat);
            Assert.AreEqual(expect.Text, s.ClosedAt, "gio dong = luc khach cuoi an xong");
        }

        [Test]
        public void AC5_HetNgay_DongHoDung_TickKhongLamGi()
        {
            GameSession s = NewSession();
            s.Tick(DaySeconds);
            string closed = s.Clock.Text;
            s.Tick(Hour);
            Assert.AreEqual(closed, s.Clock.Text);
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
        }

        // ---------- AC-6: dong quan som ----------

        [Test]
        public void AC6_BamBienLanNua_DongQuanSom()
        {
            GameSession s = NewSession(4);
            s.OpenShop();
            s.Tick(Hour);
            Assert.IsTrue(s.CloseEarly());
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
            Assert.AreEqual(100, s.Money, "khach con lai ra ve, khong tra, khong phat");
        }

        // ---------- AC-7: man ket thuc ngay ----------

        [Test]
        public void AC7_HetNgay_GhiSoKhach_TienTrongNgay_GioDongQuan()
        {
            GameSession s = NewSession(5);
            s.Tick(Hour / 2f);                           // 9:30 mo quan
            s.OpenShop();
            PaymentResult paid = ServeCurrent(s);        // bot nau khong tinh gio
            s.Tick(Hour * 2);                            // 11:30, khach thu 2 dang ngoi
            Assert.IsTrue(s.CloseEarly());

            Assert.AreEqual(1, s.ServedToday);
            Assert.AreEqual(paid.amount, s.IncomeToday);
            Assert.AreEqual("11:30", s.ClosedAt);
        }

        [Test]
        public void AC7_SangNgayMoi_DongHoVe9Gio()
        {
            GameSession s = NewSession();
            s.Tick(DaySeconds);
            Assert.IsTrue(s.StartNextDay());
            Assert.AreEqual("9:00", s.Clock.Text);
            Assert.AreEqual(0, s.CustomersToday);
            Assert.IsNull(s.ClosedAt);
        }
    }
}
