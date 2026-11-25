using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Truoc sprint 10 (spec specs/sprint-10/spec.md, constitution 2.0): gia niem yet tung mon (B-12), bot 30% cong thuc (B-14),
    /// thot mua them (B-15), khong tu het ngay (B-18), khach het thanh cho thi bo di (B-19).
    /// </summary>
    public class Sprint10Tests
    {
        static Balance B { get { return TestData.Balance; } }
        static RecipeBook R { get { return TestData.Recipes; } }

        static GameSession NewGame(int seed = 1)
        {
            return new GameSession(B, R, new SeededRandom(seed), GameSession.LastDayOfGame(B));
        }

        // ---------- B-14: 84 mon, moi nguyen lieu con mon ----------

        [Test]
        public void B14_ConLai84Mon_56Thuong_28DotPha()
        {
            Assert.AreEqual(84, R.recipes.Length);
            Assert.AreEqual(56, R.recipes.Count(r => r.group == "chinh"));
            Assert.AreEqual(28, R.recipes.Count(r => r.group == "dotpha"));
        }

        [Test]
        public void B14_MoiVong_GiuKhoang70PhanTramCap_MoiNguyenLieuCoMon()
        {
            for (int round = 1; round <= 4; round++)
            {
                var sides = R.sides.Where(x => x.unlockRound <= round).ToList();
                var mains = R.mains.Where(x => x.group == "chinh" && x.unlockRound <= round).ToList();
                int pairs = R.recipes.Count(r => r.group == "chinh" && r.unlockRound <= round);
                double share = pairs / (double)(sides.Count * mains.Count);
                Assert.That(share, Is.InRange(0.65, 0.75), "vong " + round);
                foreach (SideIngredient p in sides)
                    Assert.IsTrue(mains.Any(m => new RecipeLookup(R).Find(m.id, p.id) != null), p.name + " vong " + round + " khong co mon");
                foreach (MainIngredient m in mains)
                    Assert.IsTrue(sides.Any(p => new RecipeLookup(R).Find(m.id, p.id) != null), m.name + " vong " + round + " khong co mon");
            }
            // mon dot pha: moi loai co mon voi do phu vong 1
            foreach (MainIngredient m in R.mains.Where(x => x.group == "dotpha"))
                Assert.IsTrue(R.sides.Where(x => x.unlockRound == 1).Any(p => new RecipeLookup(R).Find(m.id, p.id) != null), m.name);
        }

        [Test]
        public void B14_BepKhongNhanCapKhongCoMon()
        {
            var lookup = new RecipeLookup(R);
            Assert.IsNull(lookup.Find("trung", "rau_muong"), "trung chien rau muong da bo");
            Assert.IsFalse(lookup.Pairs("trung", "rau_muong"));
            Assert.IsTrue(lookup.Pairs("trung", "ca_chua"));

            var stove = new Stove(lookup, new DishFormula(B));
            Assert.IsTrue(stove.SelectTool("Chảo"));
            Assert.IsTrue(stove.AddIngredient(TestData.Main("trung", 1)));
            Assert.IsFalse(stove.AddIngredient(TestData.Side("rau_muong", 1)), "cap khong co mon");
            Assert.IsTrue(stove.AddIngredient(TestData.Side("ca_chua", 1)));

            var stove2 = new Stove(lookup, new DishFormula(B));
            stove2.SelectTool("Chảo");
            Assert.IsTrue(stove2.AddIngredient(TestData.Side("rau_muong", 1)));
            Assert.IsFalse(stove2.AddIngredient(TestData.Main("trung", 1)), "nguoc thu tu cung tu choi");
        }

        [Test]
        public void B14_MoiLuotQuay_ThungChinhNauDuocVoiThungPhu()
        {
            var lookup = new RecipeLookup(R);
            foreach (int day in new[] { 1, 3, 4, 9, 15 })
            {
                var seen = new HashSet<string>();
                var gacha = new IngredientGacha(B, R, new SeededRandom(day * 7));
                for (int i = 0; i < 1500; i++)
                {
                    PullResult p = gacha.Pull(day);
                    Assert.IsTrue(lookup.Pairs(p.main.typeId, p.side.typeId), "ngay " + day + ": " + p.main.typeId + " + " + p.side.typeId);
                    seen.Add(p.main.typeId);
                }
                int round = B.RoundOfDay(day);
                Assert.AreEqual(R.mains.Count(m => m.group == "chinh" && m.unlockRound <= round), seen.Count, "moi loai chinh van ra (ngay " + day + ")");
            }
        }

        [Test]
        public void B14_ThucDonCoMonDotPha()
        {
            GameSession s = NewGame();
            Assert.AreEqual(28, s.BreakthroughRecipes().Count);
            Assert.AreEqual(6, s.BookRecipes().Count, "vong 1 con 6 mon");
        }

        // ---------- B-12: gia niem yet ----------

        [Test]
        public void B12_MoiMonCoGia_TrungBinhVongBangGiaGocCu()
        {
            Assert.IsTrue(R.recipes.All(r => r.price > 0));
            // muc tieu: gia goc cu theo vong x 0,95 (2.1; 2.0: x 1,05; chon bang bot C#, make_next.py PRICE_X); lam tron 5G nen lech <= 2G
            int[] old = { 60, 70, 85, 100 };
            for (int round = 1; round <= 4; round++)
            {
                double avg = R.recipes.Where(r => r.group == "chinh" && r.unlockRound <= round).Average(r => r.price);
                Assert.AreEqual(old[round - 1] * 0.95, avg, 2.0, "vong " + round);
            }
        }

        [Test]
        public void B12_GiaTheoBac_TiLeThuanGiaNiemYet()
        {
            Recipe r = R.recipes.First(x => x.id == "dau_phu__ca_chua");
            Assert.AreEqual(r.price, Payment.Price(B, r.price, 2));
            Assert.AreEqual((int)System.Math.Round(r.price * 1.8, System.MidpointRounding.AwayFromZero), Payment.Price(B, r.price, 3));
            Assert.AreEqual((int)System.Math.Round(r.price * 0.5, System.MidpointRounding.AwayFromZero), Payment.Price(B, r.price, 1));
        }

        [Test]
        public void B12_PhucVu_TraTheoGiaMon_SaiMonDenTheoGiaMonGoi()
        {
            GameSession s = TestData.SessionAt(4, 0, 3);
            Assert.IsTrue(s.OpenShop());
            Customer c = s.CurrentCustomer;
            int seat = s.SeatOf(c);
            Recipe ordered = s.Lookup.ById(c.orderId);
            Dish right = TestData.DishOf(ordered.id, 3);
            Assert.IsTrue(s.Counter.TryPlace(right));
            int expected = Payment.Settle(B, ordered.price, s.EasinessNow(c), 3, s.Relics.Effects).amount;
            int before = s.Money;
            PaymentResult paid = s.ServeSeat(seat, right);
            Assert.AreEqual(expected, paid.amount);
            Assert.AreEqual(before + expected, s.Money);

            s.Tick(30f);
            Customer c2 = null;
            foreach (Customer x in s.Seats) if (x != null) { c2 = x; break; }
            Assert.IsNotNull(c2);
            Recipe other = R.recipes.First(x => x.group == "chinh" && x.id != c2.orderId && x.unlockRound <= 2);
            Dish wrong = TestData.DishOf(other.id, 4);
            Assert.IsTrue(s.Counter.TryPlace(wrong));
            PaymentResult bad = s.ServeSeat(s.SeatOf(c2), wrong);
            Assert.IsTrue(bad.wrongOrder);
            int penalty = (int)System.Math.Round(B.payment.penaltyBaseShare * s.PriceOf(c2.orderId), System.MidpointRounding.AwayFromZero);
            Assert.AreEqual(-penalty, bad.amount);
        }

        // ---------- B-18: chi dong quan moi het ngay ----------

        [Test]
        public void B18_Qua21Gio_HetKhach_VanMo_ChoBamDongQuan()
        {
            GameSession s = NewGame();
            Assert.IsTrue(s.OpenShop());
            // khach vong 1 ngoi doi mai; dong ho chay qua 21:00 roi khach van ngoi
            s.Tick(B.DaySeconds() + 5f);
            Assert.IsTrue(s.IsClosedForNewCustomers);
            Assert.AreEqual(GamePhase.Open, s.Phase);
            Customer c = s.CurrentCustomer;
            Assert.IsNotNull(c);
            Dish d = TestData.DishOf(c.orderId, 2);
            s.Counter.TryPlace(d);
            s.ServeSeat(s.SeatOf(c), d);
            s.Tick(TestData.NextSeat + 30f);
            Assert.AreEqual(GamePhase.Open, s.Phase, "khong tu het ngay");
            Assert.IsNull(s.CurrentCustomer);
            Assert.IsFalse(s.AnyoneEating);
            Assert.IsTrue(s.ReadyToClose);
            Assert.IsTrue(s.CloseEarly());
            Assert.AreEqual(GamePhase.DayEnd, s.Phase);
        }

        [Test]
        public void B18_TruocGioDong_ChuaSanSangDong()
        {
            GameSession s = NewGame();
            s.OpenShop();
            Assert.IsFalse(s.ReadyToClose);
        }

        // ---------- B-19: khach het thanh cho ----------

        static float FullWait(GameSession s)
        {
            return s.SafeSeconds + B.patience.maxDrop * B.patience.dropEverySeconds;
        }

        [Test]
        public void B19_DuLieu()
        {
            Assert.AreEqual(new[] { 0, 20, 25, 30 }, B.patience.leavePenaltyByRound);
            Assert.AreEqual(0, Patience.LeavePenalty(B, 1));
            Assert.AreEqual(20, Patience.LeavePenalty(B, 4));
            Assert.AreEqual(30, Patience.LeavePenalty(B, 15));
        }

        [Test]
        public void B19_HetThanhCho_KhachBoDi_TruTien()
        {
            GameSession s = TestData.SessionAt(4, 500, 2);
            s.OpenShop();
            Customer first = s.CurrentCustomer;
            int seat = s.SeatOf(first);
            float full = FullWait(s);
            s.Tick(full - 1f);
            Assert.AreSame(first, s.Seats[seat], "chua het thanh: con ngoi");
            int before = s.Money;
            s.Tick(1.5f);
            Assert.IsFalse(ReferenceEquals(first, s.Seats[seat]), "het thanh: bo di");
            Assert.GreaterOrEqual(s.LeftToday, 1);
            Assert.IsNotNull(s.LastLeave);
            Assert.AreSame(first, s.LastLeave.customer);
            Assert.AreEqual(seat, s.LastLeave.seat);
            Assert.AreEqual(20, s.LastLeave.penalty);
            Assert.LessOrEqual(s.Money, before - 20);
        }

        [Test]
        public void B19_Vong1_KhongBoDi()
        {
            GameSession s = NewGame();
            s.OpenShop();
            Customer c = s.CurrentCustomer;
            s.Tick(400f);
            Assert.AreSame(c, s.CurrentCustomer);
            Assert.AreEqual(0, s.LeftToday);
        }

        [Test]
        public void B19_ChuNo_KhongBoDi()
        {
            GameSession s = TestData.SessionAt(21, 0, 4, 21);
            s.OpenShop();
            float toBoss = (B.day.bossHour - B.day.openHour) * B.day.secondsPerHour;
            // phuc vu het khach thuong lien tuc de chu no co ghe
            for (float t = 0f; t < toBoss + 400f; t += 2f)
            {
                s.Tick(2f);
                foreach (Customer c in s.Seats.ToList())
                    if (c != null && !c.isBoss)
                    {
                        Dish d = TestData.DishOf(c.orderId, 4);
                        s.Counter.TryPlace(d);
                        s.ServeSeat(s.SeatOf(c), d);
                    }
            }
            Assert.IsNotNull(s.Boss, "chu no van ngoi sau 400 giay");
        }

        [Test]
        public void B19_ThanhCho_VeKhongKhiBoDi()
        {
            GameSession s = TestData.SessionAt(4, 0, 2);
            s.OpenShop();
            Customer c = s.CurrentCustomer;
            c.waitedSeconds = FullWait(s) - 0.01f;
            Assert.IsFalse(Patience.GivesUp(B, 4, c, s.Relics.Effects));
            c.waitedSeconds = FullWait(s);
            Assert.IsTrue(Patience.GivesUp(B, 4, c, s.Relics.Effects));
        }

        // ---------- B-15: thot ----------

        [Test]
        public void B15_DuLieuThot()
        {
            Assert.AreEqual(1, B.board.start);
            Assert.AreEqual(3, B.board.max);
            Assert.AreEqual(300, B.board.price);
            Assert.AreEqual(200, B.board.step);
        }

        [Test]
        public void B15_MuaThot_GiaTang_ToiDa3()
        {
            GameSession s = NewGame();
            Assert.AreEqual(1, s.Boards.Count);
            Assert.AreEqual(ItemBuyResult.Closed, s.BuyBoard(), "ngay 1 cua hang chua mo");

            s = TestData.SessionAt(4, 2000);
            Assert.AreEqual(300, s.Items.BoardPrice);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyBoard());
            Assert.AreEqual(1700, s.Money);
            Assert.AreEqual(2, s.Boards.Count);
            Assert.AreEqual(500, s.Items.BoardPrice);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyBoard());
            Assert.AreEqual(1200, s.Money);
            Assert.AreEqual(3, s.Boards.Count);
            Assert.AreEqual(ItemBuyResult.AtLimit, s.BuyBoard());
            Assert.AreEqual(1200, s.Money);
        }

        [Test]
        public void B15_HaiThot_SoCheSongSong()
        {
            GameSession s = TestData.SessionAt(4, 2000, 7);
            s.BuyBoard();
            PullResult p;
            Assert.IsTrue(s.PullCrates(out p));
            Ingredient a = s.Stock.Items.First(i => i.kind == IngredientKind.Side);
            Ingredient b = s.Stock.Items.First(i => i.kind == IngredientKind.Main);
            Assert.IsTrue(s.BeginPrep(a, 0));
            Assert.IsTrue(s.BeginPrep(b, 1));
            Assert.IsFalse(s.BeginPrep(b, 0), "mot nguyen lieu khong len hai thot");
            s.HoldPrep(a.prepSeconds + 0.1f, 0);
            s.HoldPrep(b.prepSeconds + 0.1f, 1);
            Assert.IsTrue(a.prepared);
            Assert.IsTrue(b.prepared);
        }

        [Test]
        public void B15_LuuTai_GiuSoThot()
        {
            GameSession s = TestData.SessionAt(4, 2000);
            s.BuyBoard();
            s.OpenShop();
            s.CloseEarly();
            s.StartNextDay();
            GameSession t = GameSession.FromSave(B, R, s.Snapshot());
            Assert.AreEqual(2, t.Boards.Count);
            Assert.AreEqual(500, t.Items.BoardPrice);
        }
    }
}
