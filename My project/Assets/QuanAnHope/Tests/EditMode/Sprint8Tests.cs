using System.Collections.Generic;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sprint 8 (spec specs/sprint-8/spec.md, constitution 1.8): ve cong don qua luu / tai (B-02), cua hang relic doi hang va
    /// ngay 2 mien phi (F-077), nhip khach tu vong 2 (F-079), ket qua tra tien cua khach dang an (F-074).
    /// </summary>
    public class Sprint8Tests
    {
        static Balance B { get { return TestData.Balance; } }

        static GameSession NewGame(int seed = 1)
        {
            return new GameSession(B, TestData.Recipes, new SeededRandom(seed), GameSession.LastDayOfGame(B));
        }

        static void EndDay(GameSession s)
        {
            if (s.Phase == GamePhase.Prep) Assert.IsTrue(s.OpenShop());
            Assert.IsTrue(s.CloseEarly());
            Assert.IsTrue(s.StartNextDay());
        }

        // ---------- B-02: ve khong dung cong don sang ngay sau ----------

        [Test]
        public void B02_Ngay1Dung2Ve_Ngay2Co3Cong7()
        {
            GameSession s = NewGame();
            PullResult p;
            Assert.IsTrue(s.PullCrates(out p));
            Assert.IsTrue(s.PullCrates(out p));
            EndDay(s);
            Assert.AreEqual(3 + B.ticketsPerDay, s.Wallet.Count);
        }

        [Test]
        public void B02_LuuRoiTai_GiuVeCongDon()
        {
            GameSession s = NewGame(5);
            EndDay(s);                          // ngay 1 khong dung ve: 5 + 7
            SaveData save = s.Snapshot();
            Assert.IsNotNull(save);
            GameSession t = GameSession.FromSave(B, TestData.Recipes, save);
            Assert.AreEqual(B.start.firstDayTickets + B.ticketsPerDay, t.Wallet.Count);
            EndDay(t);
            Assert.AreEqual(B.start.firstDayTickets + 2 * B.ticketsPerDay, t.Wallet.Count, "ngay 3 van cong don");
        }

        [Test]
        public void B02_NhayNgayGoLoi_GiuVeCu()
        {
            GameSession s = NewGame();
            s.DebugJumpToDay(4);
            Assert.AreEqual(B.start.firstDayTickets + B.ticketsPerDay, s.Wallet.Count);
        }

        // ---------- F-077: ngay 2 mien phi ----------

        [Test]
        public void F077_DuLieu()
        {
            Assert.AreEqual(50, B.relicReroll.baseCost);
            Assert.AreEqual(25, B.relicReroll.step);
            Assert.AreEqual(2, B.freeRelic.day);
            Assert.AreEqual(RelicKeys.RarityCommon, B.freeRelic.rarity);
            Assert.AreEqual(1, B.freeRelic.picks);
        }

        [Test]
        public void F077_AC3_Ngay2_ChiRelicThuong_Gia0()
        {
            for (int seed = 1; seed <= 40; seed++)
            {
                GameSession s = NewGame(seed);
                EndDay(s);
                Assert.AreEqual(2, s.Day);
                Assert.IsTrue(s.Shop.IsFreeDay);
                for (int i = 0; i < s.Shop.Offers.Count; i++)
                {
                    Assert.IsNotNull(s.Shop.Offers[i]);
                    Assert.AreEqual(RelicKeys.RarityCommon, s.Shop.Offers[i].rarity, "seed " + seed);
                    Assert.AreEqual(0, s.Shop.Price(i));
                }
            }
        }

        [Test]
        public void F077_AC3_Ngay2_LayMotCai_ConLaiHienGiaVe_MuaTiepDuoc()
        {
            GameSession s = NewGame(3);
            EndDay(s);
            int tickets = s.Wallet.Count;
            Assert.AreEqual(RelicBuyResult.Bought, s.BuyRelic(1));
            Assert.AreEqual(tickets, s.Wallet.Count, "mien phi");
            Assert.AreEqual(1, s.Relics.Owned.Count);
            Assert.IsTrue(s.Shop.FreeDone);
            // 2.2 (yeu cau 24/11/2026): hai be con lai van tren ke, hien lai gia ve, mua bang ve
            Assert.IsNotNull(s.Shop.Offers[0]);
            Assert.AreEqual(s.Shop.Offers[0].price, s.Shop.Price(0), "hien lai gia");
            int price = s.Shop.Price(0);
            Assert.AreEqual(RelicBuyResult.Bought, s.BuyRelic(0));
            Assert.AreEqual(tickets - price, s.Wallet.Count, "relic thu hai tra bang ve");
            Assert.AreEqual(2, s.Relics.Owned.Count);
        }

        [Test]
        public void F077_AC3_Ngay2_DoiHang_VanMienPhi_VanChiThuong()
        {
            GameSession s = NewGame(4);
            EndDay(s);
            s.DebugAddMoney(500);
            Assert.AreEqual(RerollResult.Rerolled, s.RerollShop());
            for (int i = 0; i < s.Shop.Offers.Count; i++)
            {
                Assert.AreEqual(RelicKeys.RarityCommon, s.Shop.Offers[i].rarity);
                Assert.AreEqual(0, s.Shop.Price(i));
            }
        }

        [Test]
        public void F077_Ngay3_GiaVeNhuCu()
        {
            GameSession s = NewGame(2);
            EndDay(s);
            EndDay(s);
            Assert.AreEqual(3, s.Day);
            Assert.IsFalse(s.Shop.IsFreeDay);
            for (int i = 0; i < s.Shop.Offers.Count; i++) Assert.AreEqual(s.Shop.Offers[i].price, s.Shop.Price(i));
        }

        // ---------- F-077: doi hang ----------

        [Test]
        public void F077_AC1_DoiHang_BaRelicKhacHan_TruTien_GiaTang()
        {
            GameSession s = NewGame(7);
            EndDay(s); EndDay(s);               // ngay 3
            s.DebugAddMoney(1000);
            for (int k = 0; k < 3; k++)
            {
                var before = new List<RelicDef>(s.Shop.Offers);
                int money = s.Money;
                int cost = B.relicReroll.baseCost + B.relicReroll.step * k;
                Assert.AreEqual(cost, s.Shop.RerollCost);
                Assert.AreEqual(RerollResult.Rerolled, s.RerollShop());
                Assert.AreEqual(money - cost, s.Money);
                foreach (RelicDef r in s.Shop.Offers)
                {
                    Assert.IsNotNull(r);
                    CollectionAssert.DoesNotContain(before, r, "khac han ba relic dang bay");
                    Assert.IsFalse(s.Relics.Has(r.id));
                }
                Assert.AreEqual(3, new HashSet<RelicDef>(s.Shop.Offers).Count, "ba relic khac nhau");
            }
        }

        [Test]
        public void F077_AC1_DauNgay_GiaVe50()
        {
            GameSession s = NewGame(8);
            EndDay(s); EndDay(s);
            s.DebugAddMoney(1000);
            Assert.AreEqual(RerollResult.Rerolled, s.RerollShop());
            Assert.AreEqual(RerollResult.Rerolled, s.RerollShop());
            EndDay(s);
            Assert.AreEqual(B.relicReroll.baseCost, s.Shop.RerollCost);
        }

        [Test]
        public void F077_AC2_ThieuTien_Ngay1_KhongDoi()
        {
            GameSession s = NewGame(9);
            Assert.AreEqual(RerollResult.Closed, s.RerollShop(), "ngay 1 cua hang chua mo");
            EndDay(s); EndDay(s);
            s.DebugAddMoney(-s.Money + 49);
            var before = new List<RelicDef>(s.Shop.Offers);
            Assert.AreEqual(RerollResult.NotEnoughMoney, s.RerollShop());
            Assert.AreEqual(49, s.Money);
            CollectionAssert.AreEqual(before, new List<RelicDef>(s.Shop.Offers));
        }

        // ---------- F-079: nhip khach ----------

        static GameSession OpenOnDay(int day, int seed = 11)
        {
            GameSession s = NewGame(seed);
            s.DebugJumpToDay(day);
            Assert.IsTrue(s.OpenShop());
            return s;
        }

        static int Seated(GameSession s)
        {
            int n = 0;
            foreach (Customer c in s.Seats) if (c != null) n++;
            return n;
        }

        [Test]
        public void F079_KhoangCach_TheoNgay()
        {
            GameSession s = NewGame();
            s.DebugJumpToDay(1);
            Assert.AreEqual(0f, s.ArrivalGap, "vong 1 khong doi");
            s.DebugJumpToDay(4);
            Assert.AreEqual(System.Math.Max(B.day.arrivalMinGap, B.day.arrivalGapK / 4f), s.ArrivalGap, 1e-4);
            s.DebugJumpToDay(10);
            float d10 = s.ArrivalGap;
            s.DebugJumpToDay(20);
            Assert.LessOrEqual(s.ArrivalGap, d10, "ngay cang muon khach den cang day");
            Assert.GreaterOrEqual(s.ArrivalGap, B.day.arrivalMinGap);
        }

        [Test]
        public void F079_AC1_Ngay4_MoQuan_ChiMotKhachVaoNgay()
        {
            GameSession s = OpenOnDay(4);
            Assert.AreEqual(1, Seated(s));
            float gap = s.ArrivalGap;
            Assert.Greater(gap, B.day.nextCustomerSeconds + 1f);
            for (float t = 0f; t < gap - 0.5f; t += 0.25f) s.Tick(0.25f);
            Assert.AreEqual(1, Seated(s), "chua du khoang cach thi chua co khach thu hai");
            for (int i = 0; i < 8; i++) s.Tick(0.25f);
            Assert.AreEqual(2, Seated(s), "du khoang cach: khach thu hai vao");
            while (s.Clock.Elapsed < 2f * gap - 0.5f) s.Tick(0.25f);
            Assert.AreEqual(2, Seated(s));
            for (int i = 0; i < 4; i++) s.Tick(0.25f);
            Assert.AreEqual(3, Seated(s));
        }

        [Test]
        public void F079_AC1_BuocTickLon_VanGiuKhoangCach()
        {
            GameSession s = OpenOnDay(5);
            s.Tick(1f);
            Assert.AreEqual(1, Seated(s));
            s.Tick(s.ArrivalGap * 2.5f);
            Assert.AreEqual(3, Seated(s));
            float[] waited = new float[3];
            for (int i = 0; i < 3; i++) waited[i] = s.Seats[i].waitedSeconds;
            System.Array.Sort(waited);
            Assert.AreEqual(s.ArrivalGap, waited[2] - waited[1], 0.05, "khach thu 2 va 3 cach nhau dung khoang");
        }

        [Test]
        public void F079_AC2_Vong1_KhongDoi()
        {
            GameSession s = OpenOnDay(1);
            Assert.AreEqual(1, s.SeatCount);
            Assert.AreEqual(1, Seated(s));
        }

        [Test]
        public void F079_AC3_KhoangAnToan_NoiThem()
        {
            Assert.Greater(B.patience.safeSecondsByRound[1], 90f);
            Assert.Greater(B.patience.safeSecondsByRound[2], 70f);
            Assert.Greater(B.patience.safeSecondsByRound[3], 75f);
        }

        // ---------- F-074: ket qua tra tien cua khach dang an ----------

        [Test]
        public void F074_KhachDangAn_GiuKetQua_SaiMon()
        {
            GameSession s = OpenOnDay(1, 21);
            Customer c = s.Seats[0];
            Recipe other = null;
            foreach (Recipe r in s.BookRecipes()) if (r.id != c.orderId) { other = r; break; }
            var dish = new Dish { recipeId = other.id, name = other.name, baseName = other.baseName, tier = 4 };
            Assert.IsTrue(s.Fridge.TryStore(dish));
            PaymentResult pay = s.ServeSeat(0, dish);
            Assert.IsNotNull(pay);
            Assert.IsTrue(pay.wrongOrder);
            s.Tick(B.day.eatSeconds * 0.5f);
            Assert.IsNotNull(s.EatingPayment(0), "dang an van biet ket qua");
            Assert.IsTrue(s.EatingPayment(0).wrongOrder);
            Assert.AreEqual(PaymentOutcome.Angry, s.EatingPayment(0).outcome);
            s.Tick(B.day.eatSeconds);
            Assert.IsNull(s.EatingPayment(0), "an xong thi het");
        }
    }
}
