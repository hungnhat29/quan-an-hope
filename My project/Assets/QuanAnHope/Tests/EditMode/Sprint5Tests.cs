using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sprint 5 (docs/sprint-5/impl-plan.md): F-040 noi dung vong 3-4, F-041 Mon Hy Vong, F-042 relic bang ve den,
    /// F-043 chu no, F-044 thang / thua / tra no. Nguon: constitution 1.4 muc 2, 4.3, 4.5, 5.2, 5.4, 5.5, 5.8, 6.2, 6.3, 6.7, 7.1, 8.
    /// </summary>
    public class Sprint5Tests
    {
        static Balance B { get { return TestData.Balance; } }
        const int FullGame = 21;
        const int Rolls = 100000;

        static GameSession Day(int day, int money = 0, int seed = 1)
        {
            return TestData.SessionAt(day, money, seed, FullGame);
        }

        // ================= F-040 noi dung vong 3-4 =================

        [Test]
        public void F040_AC1_BangBacTheoNgay_Vong3Vong4()
        {
            Assert.AreEqual("r3", B.OddsForDay(9).id);
            Assert.AreEqual("r3", B.OddsForDay(14).id);
            Assert.AreEqual("r4", B.OddsForDay(15).id);
            Assert.AreEqual("r4", B.OddsForDay(21).id);
            CollectionAssert.AreEqual(new[] { 10, 28, 35, 20, 6, 1 }, B.OddsForDay(9).percents);
            CollectionAssert.AreEqual(new[] { 6, 18, 33, 28, 12, 3 }, B.OddsForDay(15).percents);
        }

        [TestCase(9, "r3")]
        [TestCase(15, "r4")]
        public void F040_AC1_GachaThat_Quay100000Mon_KhopBang(int day, string table)
        {
            int[] expected = B.ingredientOdds.Single(t => t.id == table).percents;
            var gacha = new IngredientGacha(B, TestData.Recipes, new SeededRandom(5000 + day));
            var counts = new int[6];
            int n = 0;
            while (n < Rolls)
            {
                PullResult p = gacha.Pull(day);
                foreach (Ingredient i in p.side.items.Concat(p.main.items)) { counts[i.tier]++; n++; }
            }
            for (int t = 0; t < 6; t++)
                Assert.AreEqual(expected[t], 100.0 * counts[t] / n, 1.0, "ngay " + day + " bac " + t);
        }

        [Test]
        public void F040_AC1_KhachVong3_20PhanTramDuoi35()
        {
            var gen = new CustomerGenerator(B, new SeededRandom(93));
            int hard = 0;
            for (int i = 0; i < Rolls; i++) if (gen.Next(3).easiness < 35) hard++;
            Assert.AreEqual(20.0, 100.0 * hard / Rolls, 1.0);
            Assert.AreEqual(3, Day(9).Round);
            Assert.AreEqual(4, Day(15).Round);
            Assert.AreEqual(3, Day(9).SeatCount);
            Assert.AreEqual(B.patience.safeSecondsByRound[2], Day(9).SafeSeconds, 1e-4);   // 1.8: so trong du lieu
            Assert.AreEqual(B.patience.safeSecondsByRound[3], Day(15).SafeSeconds, 1e-4);
        }

        [Test]
        public void F040_AC2_NguyenLieuMoKhoaDungVong_SachTuThemTrang_GiaGoc()
        {
            CollectionAssert.AreEquivalent(new[] { "bong_cai", "ot_chuong" },
                TestData.Recipes.sides.Where(x => x.unlockRound == 3).Select(x => x.id).ToArray());
            CollectionAssert.AreEquivalent(new[] { "ca_hoi", "muc" },
                TestData.Recipes.mains.Where(x => x.group == "chinh" && x.unlockRound == 3).Select(x => x.id).ToArray());
            CollectionAssert.AreEquivalent(new[] { "bi_do", "mang_tay", "nam_dong_co" },
                TestData.Recipes.sides.Where(x => x.unlockRound == 4).Select(x => x.id).ToArray());
            CollectionAssert.AreEquivalent(new[] { "ga" },
                TestData.Recipes.mains.Where(x => x.group == "chinh" && x.unlockRound == 4).Select(x => x.id).ToArray());

            Assert.AreEqual(34, Day(9).BookRecipes().Count, "vong 3: 7 chinh x 7 phu, bo 30% (constitution 2.0)");
            Assert.AreEqual(56, Day(15).BookRecipes().Count, "vong 4: 8 chinh x 10 phu, bo 30% (constitution 2.0)");
            Assert.AreEqual(85, B.BasePrice(3));
            Assert.AreEqual(100, B.BasePrice(4));
            Assert.AreEqual("Vỉ nướng", Day(15).Lookup.Find("ga", "bi_do").tool);
            Assert.AreEqual("Bánh mì", Day(15).Lookup.Find("ga", "bi_do").baseName);
        }

        [Test]
        public void F040_AC2_ThungNgay9_ChiRaNguyenLieuDaMo_CoDu4LoaiMoi()
        {
            var seen = new HashSet<string>();
            var gacha = new IngredientGacha(B, TestData.Recipes, new SeededRandom(9));
            for (int i = 0; i < 600; i++)
            {
                PullResult p = gacha.Pull(9);
                Assert.LessOrEqual(TestData.Recipes.sides.Single(x => x.id == p.side.typeId).unlockRound, 3);
                Assert.LessOrEqual(TestData.Recipes.mains.Single(x => x.id == p.main.typeId).unlockRound, 3);
                seen.Add(p.side.typeId); seen.Add(p.main.typeId);
            }
            CollectionAssert.IsSupersetOf(seen, new[] { "bong_cai", "ot_chuong", "ca_hoi", "muc" });
            Assert.IsFalse(seen.Contains("ga"));
        }

        [Test]
        public void F040_AC4_GaNuong_NauBangViNuong_DungThuTu()
        {
            GameSession s = Day(15);
            Recipe r = s.Lookup.Find("ga", "mang_tay");
            Ingredient main = TestData.Main("ga", 3), side = TestData.Side("mang_tay", 3);
            Assert.IsTrue(s.Stove.SelectTool("Vỉ nướng"));
            foreach (string step in r.steps)
            {
                if (step == main.name) Assert.IsTrue(s.Stove.AddIngredient(main));
                else if (step == side.name) Assert.IsTrue(s.Stove.AddIngredient(side));
                else Assert.IsTrue(s.Stove.AddPantry(step));
            }
            Assert.IsTrue(s.Stove.Start());
            Assert.AreEqual(27f, s.Stove.CookSeconds, 1e-4);
            s.Stove.Tick(27f);
            Assert.AreEqual(StoveState.Ready, s.Stove.State);
            Assert.IsTrue(s.Stove.FollowsRecipe);
        }

        // ================= F-041 Mon Hy Vong =================

        [Test]
        public void F041_AC1_TruocNgay9_100000LanNuocSot_KhongRaHuyenThoai()
        {
            var rng = new SeededRandom(41);
            for (int day = 1; day <= 8; day++)
                for (int i = 0; i < Rolls / 8; i++)
                    Assert.AreNotEqual(SauceResult.Legend, ItemRolls.Sauce(B, day, RelicFx.None, rng), "ngay " + day);
        }

        [Test]
        public void F041_AC2_TuNgay9_TiLeDungBang_HienChoNguoiChoi()
        {
            double[] shown = ItemRolls.SauceOdds(B, 9, RelicFx.None);
            CollectionAssert.AreEqual(new double[] { 12, 16, 48, 20, 4 }, shown);
            CollectionAssert.AreEqual(shown, ItemRolls.SauceOdds(B, 21, RelicFx.None));
            var rng = new SeededRandom(9041);
            var counts = new int[5];
            for (int i = 0; i < Rolls; i++) counts[(int)ItemRolls.Sauce(B, 9, RelicFx.None, rng)]++;
            for (int k = 0; k < 5; k++) Assert.AreEqual(shown[k], 100.0 * counts[k] / Rolls, 1.0, ((SauceResult)k).ToString());
        }

        [Test]
        public void F041_AC3_MonHyVong_Bac5_GiaX6_DatYeuCauChuNo()
        {
            Dish d = TestData.DishOf("trung__ca_chua", 3);
            d.tier = ItemRolls.ApplySauce(SauceResult.Legend, d.tier, B);
            Assert.AreEqual(5, d.tier);
            Assert.IsTrue(d.IsLegend);
            Assert.AreEqual("Trứng chiên cà chua", d.Title, "2.1: Huyen thoai la bac cua moi mon, giu ten mon");
            Assert.AreEqual("Trứng chiên cà chua", TestData.DishOf("trung__ca_chua", 4).Title);
            Assert.AreEqual(510, Payment.Price(B, 85, 5), "85G x 6,0");
            PaymentResult p = Payment.Settle(B, 85, 20, 5);
            Assert.AreEqual(PaymentOutcome.Satisfied, p.outcome, "Huyen thoai cung dat yeu cau Tuyet hao (muc 6.4)");
            Assert.AreEqual(0, p.diff);
        }

        [Test]
        public void F041_DungNuocSotThat_Ngay9_CoTheRaMonHyVong()
        {
            bool legend = false;
            for (int seed = 0; seed < 400 && !legend; seed++)
            {
                GameSession s = Day(9, 1000, seed);
                Assert.AreEqual(ItemBuyResult.Bought, s.BuySauce());
                Dish d = TestData.DishOf("trung__ca_chua", 3);
                s.Counter.TryPlace(d);
                SauceResult r;
                Assert.IsTrue(s.UseSauce(d, out r));
                if (r == SauceResult.Legend) { legend = true; Assert.IsTrue(d.IsLegend); }
            }
            Assert.IsTrue(legend);
        }

        // ================= F-042 relic bang ve den =================

        static GameSession WithBlack(int day = 9, int seed = 1)
        {
            GameSession s = Day(day, 100000, seed);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyBlackTicket());
            return s;
        }

        static bool RareUp(RelicDef r) { return r.rarity == "hiem" || r.rarity == "huyenthoai"; }

        [Test]
        public void F042_AC1_DungVeDen_3RelicHiemTroLen_ItNhatMotThuocBoNhieuNhat()
        {
            for (int seed = 0; seed < 300; seed++)
            {
                GameSession s = WithBlack(9, seed);
                s.DebugGiveRelic("C1"); s.DebugGiveRelic("C2"); s.DebugGiveRelic("V1");
                Assert.IsTrue(s.UseBlackForRelic());
                Assert.AreEqual(0, s.Items.BlackTickets, "ton 1 ve den");
                IReadOnlyList<RelicDef> offers = s.BlackRelicOffers;
                Assert.AreEqual(3, offers.Count);
                Assert.AreEqual(3, offers.Select(o => o.id).Distinct().Count(), "3 relic khac nhau");
                Assert.IsTrue(offers.All(RareUp), "chi Hiem tro len");
                Assert.IsTrue(offers.All(o => !s.Relics.Has(o.id)), "khong ban relic dang co");
                Assert.IsTrue(offers.Any(o => o.set == "cho"), "it nhat mot cai bo Cho Som (dang co 2 mon)");
            }
        }

        [Test]
        public void F042_AC1_ChonMot_VaoGia_KhongTonVeTrang()
        {
            GameSession s = WithBlack();
            int white = s.Wallet.Count;
            Assert.IsTrue(s.UseBlackForRelic());
            Assert.IsFalse(s.UseBlackForRelic(), "dang chon thi khong dung them");
            RelicDef pick = s.BlackRelicOffers[1];
            Assert.AreEqual(RelicBuyResult.Bought, s.ChooseBlackRelic(1));
            Assert.IsTrue(s.Relics.Has(pick.id));
            Assert.AreEqual(0, s.BlackRelicOffers.Count);
            Assert.AreEqual(white, s.Wallet.Count);
        }

        [Test]
        public void F042_AC1_GiaDay_PhaiBoMotRelic()
        {
            GameSession s = WithBlack();
            foreach (string id in new[] { "V1", "V2", "C1", "C2", "S1", "F2", "F3", "F4" }) s.DebugGiveRelic(id);
            Assert.IsTrue(s.Relics.IsFull);
            Assert.IsTrue(s.UseBlackForRelic());
            Assert.AreEqual(RelicBuyResult.NeedDiscard, s.ChooseBlackRelic(0));
            Assert.AreEqual(3, s.BlackRelicOffers.Count, "chua chon thi van giu 3 lua chon");
            RelicDef pick = s.BlackRelicOffers[0];
            Assert.AreEqual(RelicBuyResult.Bought, s.ChooseBlackRelic(0, "F3"));
            Assert.IsTrue(s.Relics.Has(pick.id));
            Assert.IsFalse(s.Relics.Has("F3"));
        }

        [Test]
        public void F042_AC1_KhongCoVeDen_KhongDungDuoc()
        {
            GameSession s = Day(9, 100000);
            Assert.IsFalse(s.UseBlackForRelic());
            Assert.AreEqual(0, s.BlackRelicOffers.Count);
        }

        [Test]
        public void F042_AC2_ChuaCoRelicBo_3CaiBatKyHiemTroLen()
        {
            var seen = new HashSet<string>();
            for (int seed = 0; seed < 300; seed++)
            {
                GameSession s = WithBlack(9, seed);
                s.DebugGiveRelic("F2");
                Assert.IsTrue(s.UseBlackForRelic());
                Assert.AreEqual(3, s.BlackRelicOffers.Count);
                Assert.IsTrue(s.BlackRelicOffers.All(RareUp));
                foreach (RelicDef r in s.BlackRelicOffers) seen.Add(r.id);
            }
            CollectionAssert.AreEquivalent(B.relics.Where(RareUp).Select(r => r.id).ToArray(), seen.ToArray(),
                "moi relic Hiem tro len deu co the ra");
        }

        // ================= F-043 chu no =================

        static float BossAt { get { return (B.day.bossHour - B.day.openHour) * B.day.secondsPerHour; } }

        static GameSession Day21Open(int seed = 1)
        {
            GameSession s = Day(21, 0, seed);
            Assert.IsTrue(s.IsBossDay);
            Assert.IsTrue(s.OpenShop());
            return s;
        }

        [Test]
        public void F043_AC1_Ngay21_20Gio_ChuNoVao_DeTinh5_ChiemMotGhe()
        {
            GameSession s = Day21Open();
            TestData.TickKeepWaiting(s, BossAt - 1f);
            Assert.IsFalse(s.BossArrived, "truoc 20:00 chua toi");
            Assert.IsTrue(s.Seats.All(c => c == null || !c.isBoss));
            Assert.AreEqual(3, s.Seats.Count(c => c != null), "3 ghe da co khach thuong");

            s.Tick(1f);
            Assert.IsTrue(s.BossArrived);
            Assert.IsTrue(s.BossWaiting, "het ghe: cho ghe trong");

            // phuc vu ghe 1 (dia bat ky) -> khach an xong thi chu no ngoi vao ngay, khong doi 3 giay
            Dish d = TestData.DishOf(s.Seats[1].orderId ?? "trung__ca_chua", 2);
            s.Counter.TryPlace(d);
            Assert.IsNotNull(s.ServeSeat(1, d));
            Assert.IsNull(s.Boss, "khach ghe 1 con dang an");
            Assert.IsTrue(s.BossWaiting);
            s.Tick(TestData.Eat);
            Assert.IsNotNull(s.Boss);
            Assert.AreEqual(1, s.SeatOf(s.Boss));
            Assert.IsTrue(s.Boss.isBoss);
            Assert.AreEqual(5, s.Boss.easiness);
            Assert.IsFalse(s.BossWaiting);
        }

        [Test]
        public void F043_AC1_ChuNoNgoiQua21Gio_KhongMatDeTinh_NgayChuaHet()
        {
            GameSession s = Day21Open();
            s.Tick(BossAt);
            // phuc vu het khach thuong de chu no co ghe, sau do qua 21:00
            for (int guard = 0; guard < 20 && s.Boss == null; guard++)
            {
                int seat = Enumerable.Range(0, s.SeatCount).First(i => s.Seats[i] != null && !s.Seats[i].isBoss);
                Dish d = TestData.DishOf("trung__ca_chua", 2);
                s.Counter.TryPlace(d);
                s.ServeSeat(seat, d);
                s.Tick(TestData.Eat);
            }
            Assert.IsNotNull(s.Boss);
            TestData.TickKeepWaiting(s, B.DaySeconds());
            Assert.IsTrue(s.Clock.IsPastClose);
            Assert.AreEqual(GamePhase.Open, s.Phase, "chu no con ngoi thi ngay chua het");
            Assert.IsNotNull(s.Boss);
            Assert.AreEqual(5, s.EasinessNow(s.Boss), "do de tinh co dinh 5%");
            Assert.AreEqual(1f, s.PatienceLeft(s.Boss), 1e-6);

            // khach thuong phuc vu het -> chi con chu no
            for (int i = 0; i < s.SeatCount; i++)
                if (s.Seats[i] != null && !s.Seats[i].isBoss)
                {
                    Dish d = TestData.DishOf("trung__ca_chua", 2);
                    s.Counter.TryPlace(d);
                    s.ServeSeat(i, d);
                }
            Assert.AreEqual(GamePhase.Open, s.Phase);
            Dish top = TestData.DishOf("heo__hanh_tay", 4);
            s.Counter.TryPlace(top);
            s.ServeSeat(s.SeatOf(s.Boss), top);
            s.Tick(TestData.Eat);
            Assert.IsTrue(s.ReadyToClose, "phuc vu chu no (nguoi cuoi), an xong thi dong quan duoc");
            Assert.IsTrue(s.CloseEarly());
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
        }

        [Test]
        public void F043_AC2_ChuNoGoiMonNgonNhat_NhanMoiMon()
        {
            GameSession s = Day21Open();
            s.Tick(BossAt);
            FreeSeatForBoss(s);
            Assert.IsNull(s.Boss.orderId);
            Assert.AreEqual("món ngon nhất của quán", s.Boss.orderName);
            Dish d = TestData.DishOf("ga__bi_do", 4);
            s.Counter.TryPlace(d);
            PaymentResult p = s.ServeSeat(s.SeatOf(s.Boss), d);
            Assert.IsFalse(p.wrongOrder, "chu no nhan moi mon");
        }

        static void FreeSeatForBoss(GameSession s)
        {
            if (s.Boss != null) return;
            int seat = Enumerable.Range(0, s.SeatCount).First(i => s.Seats[i] != null);
            Dish d = TestData.DishOf("trung__ca_chua", 2);
            s.Counter.TryPlace(d);
            s.ServeSeat(seat, d);
            Assert.IsNotNull(s.Boss);
        }

        [TestCase(4)]
        [TestCase(5)]
        public void F043_AC3_TuyetHaoHoacHuyenThoai_Thuong11500(int tier)
        {
            GameSession s = Day21Open();
            s.Tick(BossAt);
            FreeSeatForBoss(s);
            int before = s.Money;
            Dish d = TestData.DishOf("ca_hoi__ot_chuong", tier);
            s.Counter.TryPlace(d);
            PaymentResult p = s.ServeSeat(s.SeatOf(s.Boss), d);
            PaymentResult plain = Payment.Settle(B, s.PriceOf("ca_hoi__ot_chuong"), 5, tier, s.Relics.Effects);
            Assert.AreEqual(11500, p.bossBonus);
            Assert.AreEqual(plain.amount + 11500, p.amount);
            Assert.AreEqual(before + plain.amount + 11500, s.Money);
            Assert.IsTrue(s.BossServed);
            Assert.IsNull(s.Boss);
        }

        [TestCase(3)]
        [TestCase(1)]
        public void F043_AC4_MonThapHon_TinhNhuKhachKhoTinh_KhongThuong(int tier)
        {
            GameSession s = Day21Open();
            s.Tick(BossAt);
            FreeSeatForBoss(s);
            Dish d = TestData.DishOf("muc__bong_cai", tier);
            s.Counter.TryPlace(d);
            PaymentResult p = s.ServeSeat(s.SeatOf(s.Boss), d);
            Assert.AreEqual(0, p.bossBonus);
            Assert.AreEqual(Payment.Settle(B, s.PriceOf("muc__bong_cai"), 5, tier).amount, p.amount);
            Assert.IsTrue(s.BossServed);
        }

        [Test]
        public void F043_AC5_QuanChuaMo_DongHoDung_ChuNoChuaToi() // constitution 1.7 muc 6.2 (cu: chu no doi ngoai cua)
        {
            GameSession s = Day(21);
            s.Tick(BossAt + 30f);
            Assert.AreEqual(GamePhase.Prep, s.Phase);
            Assert.IsFalse(s.BossArrived, "dong ho chua chay: chua toi 20:00");
            Assert.IsFalse(s.BossWaiting);
            Assert.IsTrue(s.OpenShop());
            Assert.IsNull(s.Boss, "mo quan luc 9:00: chu no chua toi");
        }

        [Test]
        public void F043_AC5_KhongMoQuan_HoacDongQuanSom_MatThuong()
        {
            GameSession never = Day(21);
            never.Tick(B.DaySeconds() + 1f);
            Assert.AreEqual(GamePhase.Prep, never.Phase, "khong mo quan thi ngay chua bat dau (1.7)");
            Assert.IsFalse(never.BossServed);

            GameSession early = Day21Open();
            early.Tick(BossAt);
            FreeSeatForBoss(early);
            Assert.IsTrue(early.CloseEarly());
            Assert.IsFalse(early.BossServed);
            Assert.AreEqual(GamePhase.DayEnd, early.Phase);
        }

        [Test]
        public void F043_NgayKhac_KhongCoChuNo()
        {
            GameSession s = Day(20);
            Assert.IsFalse(s.IsBossDay);
            s.OpenShop();
            s.Tick(BossAt + 10f);
            Assert.IsFalse(s.BossArrived);
            Assert.IsTrue(s.Seats.All(c => c == null || !c.isBoss));
            Assert.AreEqual(21, B.day.bossDay);
        }

        // ================= F-044 thang, thua, tra no =================

        [Test]
        public void F044_AC1_DauNgay15_Thu10500_ThieuThiThua()
        {
            GameSession ok = Day(14, 10600);
            ok.OpenShop(); ok.CloseEarly(); ok.StartNextDay();
            Assert.AreEqual(15, ok.Day);
            Assert.AreEqual(10500, ok.LastDebtPaid);
            Assert.AreEqual(100, ok.Money);
            Assert.AreEqual(GamePhase.Prep, ok.Phase);

            GameSession poor = Day(14, 10499);
            poor.OpenShop(); poor.CloseEarly(); poor.StartNextDay();
            Assert.AreEqual(GamePhase.Lost, poor.Phase);
            Assert.IsFalse(poor.IsWon);
        }

        [Test]
        public void F044_AC1_AC2_DauNgay22_ThuNo_DuThiThang() // no 22 doc tu du lieu (sprint 7: 36.000G)
        {
            int due = B.DebtDueOn(22);
            GameSession ok = Day(21, due);
            ok.OpenShop(); ok.CloseEarly(); ok.StartNextDay();
            Assert.AreEqual(22, ok.Day);
            Assert.AreEqual(due, ok.LastDebtPaid);
            Assert.AreEqual(GamePhase.Cleared, ok.Phase);
            Assert.IsTrue(ok.IsWon);

            GameSession poor = Day(21, due - 1);
            poor.OpenShop(); poor.CloseEarly(); poor.StartNextDay();
            Assert.AreEqual(GamePhase.Lost, poor.Phase);
            Assert.IsFalse(poor.IsWon);
        }

        [Test]
        public void F044_BanDayDu_ChoiToiNgay21()
        {
            Assert.AreEqual(21, GameSession.LastDayOfGame(B));
            Assert.AreEqual(21, GameSession.LastPlayableDayFor(B, 4));
            GameSession part = TestData.SessionAt(8, 3000, 1, 8);
            part.OpenShop(); part.CloseEarly(); part.StartNextDay();
            Assert.AreEqual(GamePhase.Cleared, part.Phase);
            Assert.IsFalse(part.IsWon, "ban choi mot phan (dung o ngay 9) khong phai thang");
        }

        [Test]
        public void F044_Ngay8Sang9_Ngay14Sang15_KhongDung()
        {
            GameSession s = Day(8, 3000);
            s.OpenShop(); s.CloseEarly(); s.StartNextDay();
            Assert.AreEqual(9, s.Day);
            Assert.AreEqual(GamePhase.Prep, s.Phase);
        }
    }
}
