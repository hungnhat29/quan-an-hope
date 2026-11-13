using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-005 AC-1..5 va F-004 AC-9. Nguon: constitution muc 2, 3, 4.2, 5.1; tin nhan nguoi dung 14/11 (nut Mo quan).
    /// </summary>
    public class GameSessionTests
    {
        const int LastDayOfRound1 = 3;

        static GameSession NewSession(int seed = 1)
        {
            return new GameSession(TestData.Balance, TestData.Recipes, new SeededRandom(seed), LastDayOfRound1);
        }

        /// <summary>Nau va phuc vu het khach cua ngay hien tai theo kieu bot; tra ve pha sau do.</summary>
        static void PlayOneDay(GameSession s)
        {
            NaiveBot.PlayDay(s);
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
        }

        // ---------- F-005 AC-1 ----------

        [Test]
        public void F005_AC1_BatDau_100G_Ngay1_5Ve()
        {
            GameSession s = NewSession();
            Assert.AreEqual(1, s.Day);
            Assert.AreEqual(100, s.Money);
            Assert.AreEqual(5, s.Wallet.Count);
            Assert.AreEqual(GamePhase.Prep, s.Phase);
            Assert.AreEqual(4, s.NextDebt().day);
            Assert.AreEqual(400, s.NextDebt().amount);
        }

        // ---------- F-004 AC-9: nut Mo quan ----------

        [Test]
        public void F004_AC9_ChuaMoQuan_ChuaCoKhach()
        {
            GameSession s = NewSession();
            Assert.IsNull(s.CurrentCustomer);
            Assert.IsTrue(NaiveBot.CookOne(s));
            Assert.IsNull(s.Serve(0), "chua mo quan thi khong phuc vu duoc");
            Assert.AreEqual(1, s.Counter.Dishes.Count);
        }

        [Test]
        public void F004_AC9_MoQuan_KhachDenLanLuot()
        {
            GameSession s = NewSession();
            PullResult pull;
            Assert.IsTrue(s.PullCrates(out pull));
            Assert.IsTrue(NaiveBot.CookOne(s));
            Assert.IsTrue(NaiveBot.CookOne(s));

            Assert.IsTrue(s.OpenShop());
            Assert.AreEqual(GamePhase.Open, s.Phase);
            Assert.AreEqual(6, s.CustomersToday);
            Customer first = s.CurrentCustomer;
            Assert.IsNotNull(first);
            Assert.IsFalse(s.OpenShop(), "da mo roi thi khong mo lai");

            Assert.AreSame(first, s.CurrentCustomer, "khach sau chi vao khi khach truoc da duoc phuc vu");
            Assert.IsNotNull(s.Serve(0));
            Assert.AreEqual(1, s.ServedToday);
            Assert.IsNotNull(s.CurrentCustomer);
            Assert.AreNotSame(first, s.CurrentCustomer);
        }

        [Test]
        public void F004_PhucVu_CongTienDungBangTinhTien()
        {
            GameSession s = NewSession(5);
            Assert.IsTrue(NaiveBot.CookOne(s));
            s.OpenShop();
            int easiness = s.CurrentCustomer.easiness;
            int tier = s.Counter.Dishes[0].tier;
            int expected = Payment.Settle(TestData.Balance, 1, easiness, tier).amount;

            PaymentResult paid = s.Serve(0);

            Assert.AreEqual(expected, paid.amount);
            Assert.AreEqual(100 + expected, s.Money);
            Assert.AreEqual(expected, s.IncomeToday);
            Assert.AreEqual(0, s.Counter.Dishes.Count);
        }

        // ---------- F-005 AC-2 ----------

        [Test]
        public void F005_AC2_PhucVuXongKhachCuoi_KetThucNgay_BoNguyenLieuVaDia()
        {
            GameSession s = NewSession(2);
            PullResult pull;
            while (s.PullCrates(out pull)) { }           // dung het 5 ve: 25 do phu + 15 chinh
            s.OpenShop();
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(GamePhase.Open, s.Phase);
                Assert.IsTrue(NaiveBot.CookOne(s));
                if (i == 5) Assert.IsTrue(NaiveBot.CookOne(s)); // nau du 1 dia de kiem viec bo dia cuoi ngay
                Assert.IsNotNull(s.Serve(0));
            }

            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
            Assert.AreEqual(6, s.ServedToday);
            Assert.IsNull(s.CurrentCustomer);
            Assert.AreEqual(0, s.Stock.Count, "nguyen lieu thua bi bo");
            Assert.AreEqual(0, s.Counter.Dishes.Count, "dia tren quay cho bi bo");
        }

        [Test]
        public void F005_AC2_SangNgayMoi_CongVe_VeThuaCongDon()
        {
            GameSession s = NewSession(3);
            PullResult pull;
            s.PullCrates(out pull);
            s.PullCrates(out pull);                       // con 3 ve
            s.OpenShop();
            s.CloseEarly();

            Assert.IsTrue(s.StartNextDay());

            Assert.AreEqual(2, s.Day);
            Assert.AreEqual(GamePhase.Prep, s.Phase);
            Assert.AreEqual(6, s.Wallet.Count);
            Assert.AreEqual(0, s.ServedToday);
            Assert.AreEqual(0, s.IncomeToday);
            Assert.IsNull(s.CurrentCustomer);
        }

        [Test]
        public void F005_AC2_DongQuanSom_KetThucNgay_KhachConLaiKhongTra()
        {
            GameSession s = NewSession(4);
            Assert.IsFalse(s.CloseEarly(), "chua mo quan thi khong dong duoc");
            s.OpenShop();
            Assert.IsTrue(s.CloseEarly());
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
            Assert.AreEqual(100, s.Money);
        }

        [Test]
        public void F005_AC2_HetNgay_KhongQuayGachaDuoc()
        {
            GameSession s = NewSession(4);
            s.OpenShop();
            s.CloseEarly();
            PullResult pull;
            Assert.IsFalse(s.PullCrates(out pull));
            Assert.AreEqual(5, s.Wallet.Count);
        }

        // ---------- F-005 AC-3 ----------

        [Test]
        public void F005_AC3_DauNgay4_KhongDu400G_Thua()
        {
            GameSession s = NewSession(6);
            for (int day = 1; day <= 3; day++)
            {
                s.OpenShop();
                s.CloseEarly();                           // khong ban gi: van 100G
                Assert.IsTrue(s.StartNextDay());
            }

            Assert.AreEqual(GamePhase.Lost, s.Phase);
            Assert.AreEqual(4, s.Day);
            Assert.AreEqual(100, s.Money, "khong du thi khong tru tien");
            Assert.IsFalse(s.StartNextDay());
        }

        [Test]
        public void F005_AC3_AC4_DauNgay4_Du400G_TruNo_QuaVong1()
        {
            GameSession s = NewSession(7);
            for (int day = 1; day <= 3; day++)
            {
                PlayOneDay(s);
                if (day < 3) Assert.IsTrue(s.StartNextDay());
            }
            int before = s.Money;
            Assert.GreaterOrEqual(before, 400, "seed nay bot phai kiem du tien");

            Assert.IsTrue(s.StartNextDay());

            Assert.AreEqual(GamePhase.Cleared, s.Phase);
            Assert.AreEqual(4, s.Day);
            Assert.AreEqual(400, s.LastDebtPaid);
            Assert.AreEqual(before - 400, s.Money);
        }

        [Test]
        public void F005_AC3_KhongThuNoVaoNgayKhongDenHan()
        {
            GameSession s = NewSession(8);
            s.OpenShop();
            s.CloseEarly();
            s.StartNextDay();
            Assert.AreEqual(100, s.Money);
            Assert.AreEqual(0, s.LastDebtPaid);
        }

        // ---------- F-005 AC-5: bot "khong doc chi so" ----------

        [Test]
        public void F005_AC5_BotKhongDocChiSo_2000Van_QuaLanTraNo1()
        {
            const int Games = 2000;
            int cleared = 0;
            long moneyAtDebt = 0;
            for (int g = 0; g < Games; g++)
            {
                GameSession s = NewSession(10000 + g);
                GamePhase end = NaiveBot.PlayToEnd(s);
                if (end == GamePhase.Cleared) { cleared++; moneyAtDebt += s.Money + s.LastDebtPaid; }
            }

            double passRate = 100.0 * cleared / Games;
            UnityEngine.Debug.Log("NaiveBot: qua lan tra no 1 = " + passRate.ToString("0.00") + "% (" + cleared + "/" + Games
                + "), tien trung binh truoc khi tra no = " + (cleared > 0 ? moneyAtDebt / cleared : 0) + "G");
            // Mo phong (constitution Dieu 1): 100%. Cho phep lech 1 diem nhu Dieu 3.
            Assert.GreaterOrEqual(passRate, 99.0);
        }
    }
}
