using System;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-021 AC-1..3 va F-022 AC-1, 2, 4. Nguon: constitution 1.4 muc 4.4, 7.1-7.3, Dieu 3; cach gop va thu tu ap
    /// theo balance/sim_balance.py (P.fx, pull, cook, pay); specs/sprint-3/F-021-hieu-ung-relic/spec.md.
    /// </summary>
    public class RelicEffectTests
    {
        const int Items = 100000;
        const double Tol = 1.0; // Dieu 3: +-1 diem phan tram
        const int Day1 = 1;

        static RelicFx Fx(params string[] ids)
        {
            return RelicFx.Combine(ids.Select(RelicShopTests.Def), TestData.Balance.setBonuses);
        }

        static IngredientGacha Gacha(int seed = 20261116)
        {
            return new IngredientGacha(TestData.Balance, TestData.Recipes, new SeededRandom(seed));
        }

        static double[] R1()
        {
            return TestData.Balance.OddsForDay(Day1).percents.Select(p => p / 100.0).ToArray();
        }

        /// <summary>Phan bo bac (%) cua it nhat Items mon thuoc loai nay, quay o ngay 1.</summary>
        static double[] Distribution(RelicFx fx, IngredientKind kind, int seed = 20261116)
        {
            IngredientGacha g = Gacha(seed);
            var counts = new int[6];
            int n = 0;
            while (n < Items)
            {
                PullResult p = g.Pull(Day1, fx);
                foreach (Ingredient i in (kind == IngredientKind.Side ? p.side : p.main).items) { counts[i.tier]++; n++; }
            }
            return counts.Select(c => 100.0 * c / n).ToArray();
        }

        static void AssertClose(double[] expectedShare, double[] actualPct, string what)
        {
            for (int t = 0; t < 6; t++)
                Assert.That(actualPct[t], Is.InRange(expectedShare[t] * 100 - Tol, expectedShare[t] * 100 + Tol),
                    what + " bac " + t + ": " + actualPct[t].ToString("0.00") + "% (bang " + (expectedShare[t] * 100).ToString("0.00") + "%)");
        }

        static double[] Up(double[] p, double u)
        {
            var e = new double[6];
            for (int t = 0; t < 6; t++)
            {
                e[t] += p[t] * (1 - u);
                e[Math.Min(5, t + 1)] += p[t] * u;
            }
            return e;
        }

        static double[] Reroll0(double[] p)
        {
            return p.Select((x, t) => t == 0 ? p[0] * p[0] : x + p[0] * x).ToArray();
        }

        static double[] Jump4(double[] p, double j)
        {
            var e = new double[6];
            for (int t = 0; t < 6; t++)
            {
                if (t < 4) { e[t] += p[t] * (1 - j); e[4] += p[t] * j; }
                else e[t] += p[t];
            }
            return e;
        }

        // ---------- gop hieu ung + thuong bo (AC-3) ----------

        [Test]
        public void AC3_VuonNha_Nguong2_3_4_DungSoSetBonus()
        {
            Assert.AreEqual(0.15f, Fx("V1").Get("phu_up"), 1e-6);
            RelicFx two = Fx("V1", "V2");
            Assert.AreEqual(0.15f + 0.20f, two.Get("phu_up"), 1e-6);
            Assert.AreEqual(0f, two.Get("phu_min"));
            RelicFx three = Fx("V1", "V2", "V3");
            Assert.AreEqual(0.15f + 0.20f + 0.35f, three.Get("phu_up"), 1e-6);
            Assert.AreEqual(2f, three.Get("phu_min"));
            Assert.AreEqual(0f, three.Get("phu_plus"));
            RelicFx four = Fx("V1", "V2", "V3", "V4");
            Assert.AreEqual(1f, four.Get("phu_plus"));
            Assert.AreEqual(0.06f, four.Get("phu_jump4"), 1e-6);
            Assert.AreEqual(0f, four.Get("chinh_up"), "bo Vuon Nha khong dong vao nguyen lieu chinh");
        }

        [Test]
        public void AC3_ChoSom_Nguong2_3_4_DungSoSetBonus()
        {
            Assert.AreEqual(0.15f + 0.20f, Fx("C1", "C2").Get("chinh_up"), 1e-6);
            RelicFx three = Fx("C1", "C2", "C3");
            Assert.AreEqual(0.15f + 0.20f + 0.25f, three.Get("chinh_up"), 1e-6);
            Assert.AreEqual(2f, three.Get("chinh_min"));
            Assert.AreEqual(1f, three.Get("chinh_size"));
            Assert.AreEqual(1f, Fx("C1", "C2", "C3", "C4").Get("chinh_plus"));
            Assert.AreEqual(0f, Fx("C1", "V1").Get("chinh_min"), "khac bo khong tinh chung");
            Assert.AreEqual(0.15f, Fx("C1", "V1").Get("chinh_up"), 1e-6, "2 relic khac bo: khong co thuong bo");
        }

        [Test]
        public void AC3_SonHao_Nguong2_3_4_DungSoSetBonus()
        {
            Assert.AreEqual(0.10f + 0.08f + 0.15f, Fx("S1", "S2").Get("bt_chance"), 1e-6);
            Assert.AreEqual(1f, Fx("S1", "S2", "S3").Get("bt_items"));
            RelicFx four = Fx("S1", "S2", "S3", "S4");
            Assert.AreEqual(0.10f + 0.08f + 0.12f + 0.15f + 0.15f + 0.20f, four.Get("bt_chance"), 1e-5);
            Assert.AreEqual(1f, four.Get("bt_t5"), 1e-6, "4 mon: moi mon dot pha bac 5");
            Assert.AreEqual(-1f, four.Get("phu_size"));
        }

        // ---------- tung relic (AC-1), ti le thong ke (AC-2) ----------

        [Test]
        public void V1_BinhTuoiGi_DoPhu15PhanTramNangBac_ThongKe()
        {
            AssertClose(Up(R1(), 0.15), Distribution(Fx("V1"), IngredientKind.Side), "do phu");
            AssertClose(R1(), Distribution(Fx("V1"), IngredientKind.Main), "chinh khong doi");
        }

        [Test]
        public void C1_DaoPhayCu_Chinh15PhanTramNangBac_ThongKe()
        {
            AssertClose(Up(R1(), 0.15), Distribution(Fx("C1"), IngredientKind.Main), "chinh");
        }

        [Test]
        public void AC2_NangBac_ToiDaBac5()
        {
            // bang chi co bac 5, nang bac 100%: van la bac 5
            var fx = Fx("V1", "V2", "V3"); // phu_up 0,7, toi thieu 2
            double[] d = Distribution(fx, IngredientKind.Side);
            Assert.AreEqual(0.0, d[0] + d[1], 1e-9, "thuong bo 3: do phu toi thieu bac 2");
            Assert.AreEqual(100.0, d.Sum(), 1e-6);
        }

        [Test]
        public void V2_RoTre_ThungPhu4Mon_S3_ThuyenThue_2Mon_CaHai_3Mon()
        {
            Assert.AreEqual(4, Gacha().Pull(Day1, Fx("V2")).side.items.Count);
            Assert.AreEqual(2, Gacha().Pull(Day1, Fx("S3")).side.items.Count);
            Assert.AreEqual(3, Gacha().Pull(Day1, Fx("V2", "S3")).side.items.Count);
            Assert.AreEqual(2, Gacha().Pull(Day1, Fx("V2")).main.items.Count);
            // ngay 3 (sau mua) 2 + 1: Thuyen thue con 1 mon (toi thieu 1)
            Assert.AreEqual(1, Gacha().Pull(3, Fx("S3")).side.items.Count);
        }

        [Test]
        public void C3_MoiQuenChoCa_ThungChinh3Mon()
        {
            PullResult p = Gacha().Pull(Day1, Fx("C3"));
            Assert.AreEqual(3, p.main.items.Count);
            Assert.AreEqual(3, p.side.items.Count);
        }

        [Test]
        public void V3_PhanBon_DoPhuBac0QuayLai_ThongKe()
        {
            AssertClose(Reroll0(R1()), Distribution(Fx("V3"), IngredientKind.Side), "do phu");
        }

        [Test]
        public void C2_ThungDa_ChinhBac0QuayLai_ThongKe()
        {
            AssertClose(Reroll0(R1()), Distribution(Fx("C2"), IngredientKind.Main), "chinh");
        }

        [Test]
        public void V4_HatGiongVang_DoPhu6PhanTramLenBac4_ThongKe()
        {
            AssertClose(Jump4(R1(), 0.06), Distribution(Fx("V4"), IngredientKind.Side), "do phu");
        }

        [Test]
        public void C4_CanVang_Chinh8PhanTramLenBac4_ThongKe()
        {
            AssertClose(Jump4(R1(), 0.08), Distribution(Fx("C4"), IngredientKind.Main), "chinh");
        }

        [Test]
        public void ThuongBo4_VuonNha_MoiDoPhuCong1Bac_ToiThieuBac3()
        {
            double[] d = Distribution(Fx("V1", "V2", "V3", "V4"), IngredientKind.Side);
            Assert.AreEqual(0.0, d[0] + d[1] + d[2], 1e-9, "toi thieu 2 roi +1 bac");
            Assert.Greater(d[5], 0.0);
        }

        [Test]
        public void ThuongBo2_ChoSom_NangBac35PhanTram_ThongKe()
        {
            AssertClose(Up(Reroll0(R1()), 0.35), Distribution(Fx("C1", "C2"), IngredientKind.Main), "chinh");
        }

        [Test]
        public void F4_MuoiHong_DiemMonCong015()
        {
            var f = new DishFormula(TestData.Balance) { Relics = Inventory("F4") };
            Assert.AreEqual(0.6 + 0.4 + 0.8 + 0.15, f.Score(1, 1), 1e-9);
            TierChance[] bar = f.Bar(1, 1);
            Assert.AreEqual(2, bar.Length);
            Assert.AreEqual(0.95, bar[1].chance, 1e-9);
        }

        [Test]
        public void F8_OtMa_DiemCong06_Va10PhanTramLacVi_ThanhTiLeCoLacVi()
        {
            var f = new DishFormula(TestData.Balance) { Relics = Inventory("F8") };
            Assert.AreEqual(0.6 + 0.4 + 0.8 + 0.6, f.Score(1, 1), 1e-9);
            // b = 2,4: 60% bac 2, 40% bac 3; sau Ot ma: 10% Lac vi
            TierChance[] bar = f.Bar(1, 1);
            Assert.AreEqual(0.10, bar.Single(c => c.tier == 0).chance, 1e-9);
            Assert.AreEqual(0.54, bar.Single(c => c.tier == 2).chance, 1e-9);
            Assert.AreEqual(0.36, bar.Single(c => c.tier == 3).chance, 1e-9);
            Assert.AreEqual(3, f.Roll(1, 1, new FixedRandom(0.1, 0.5)));
            Assert.AreEqual(0, f.Roll(1, 1, new FixedRandom(0.1, 0.05)));
            var rng = new SeededRandom(3);
            int ruined = 0;
            for (int i = 0; i < Items; i++) if (f.Roll(1, 1, rng) == 0) ruined++;
            Assert.That(100.0 * ruined / Items, Is.InRange(10 - Tol, 10 + Tol));
        }

        [Test]
        public void F5_ChaoGang_MonKhongDuoiDo_KeCaOtMa_VaSaiCongThuc()
        {
            var f = new DishFormula(TestData.Balance) { Relics = Inventory("F5", "F8") };
            Assert.AreEqual(1, f.Roll(0, 0, new FixedRandom(0.0, 0.0)), "Ot ma ra Lac vi, Chao gang keo len Do");
            Assert.IsFalse(f.Bar(0, 0).Any(c => c.tier == 0));
            Assert.AreEqual(1, f.ApplyFloor(0));

            // mon sai cong thuc (Q-29): van la Do
            var lookup = new RecipeLookup(TestData.Recipes);
            var counter = new Counter(TestData.Balance);
            var stove = new Stove(lookup, f);
            Assert.IsTrue(stove.SelectTool("Nồi")); // trung chien dung chao: sai dung cu
            Assert.IsTrue(stove.AddIngredient(TestData.Main("trung", 1)));
            Assert.IsTrue(stove.AddIngredient(TestData.Side("ca_chua", 1)));
            Assert.IsTrue(stove.Start());
            stove.Tick(stove.CookSeconds);
            Dish dish;
            Assert.IsTrue(stove.TryPlate(counter, new FixedRandom(0.5, 0.5), out dish));
            Assert.AreEqual(1, dish.tier);
        }

        [Test]
        public void F2_ChuongVang_Tip15PhanTram_F6_TuongMeo_TienMon8PhanTram()
        {
            Balance b = TestData.Balance;
            PaymentResult plain = Payment.Settle(b, 1, 40, 3);
            PaymentResult bell = Payment.Settle(b, 1, 40, 3, Fx("F2"));
            PaymentResult cat = Payment.Settle(b, 1, 40, 3, Fx("F6"));
            double rawPrice = b.dish.priceMult[3] * b.BasePrice(1);
            double rawTip = rawPrice * 0.6 * (b.payment.tipBase + b.payment.tipPerTier * plain.diff);
            Assert.AreEqual(plain.price, bell.price);
            Assert.AreEqual(Math.Round(rawTip * 1.15, MidpointRounding.AwayFromZero), bell.tip);
            Assert.AreEqual(Math.Round(rawPrice * 1.08, MidpointRounding.AwayFromZero), cat.price);
            Assert.AreEqual(Math.Round(rawTip * 1.08, MidpointRounding.AwayFromZero), cat.tip);
            Assert.Greater(bell.amount, plain.amount);
            // khach that vong: khong co thuong relic (sim_balance.py pay)
            Assert.AreEqual(Payment.Settle(b, 1, 95, 0).amount, Payment.Settle(b, 1, 95, 0, Fx("F2", "F6")).amount);
        }

        [Test]
        public void F10_VeSoCao_Cong1VeNgayChiaHet3()
        {
            var w = new TicketWallet(TestData.Balance);
            RelicFx fx = Fx("F10");
            w.BeginDay(1, fx);
            Assert.AreEqual(8, w.Count);
            w.BeginDay(2, fx);
            Assert.AreEqual(14, w.Count);
            w.BeginDay(3, fx);
            Assert.AreEqual(21, w.Count);
        }

        [Test]
        public void F10_TrongVanChoi_Ngay3CoThem1Ve()
        {
            GameSession s = RelicShopTests.NewSession(2);
            s.DebugGiveRelic("F10");
            RelicShopTests.NextDay(s);
            int day2 = s.Wallet.Count;
            RelicShopTests.NextDay(s);
            Assert.AreEqual(day2 + 6 + 1, s.Wallet.Count);
        }

        [Test]
        public void TrongVanChoi_RelicApVaoGacha_BepVaTien()
        {
            GameSession s = RelicShopTests.NewSession(4);
            Assert.IsTrue(s.DebugGiveRelic("V2"));
            Assert.IsTrue(s.DebugGiveRelic("F4"));
            PullResult p;
            Assert.IsTrue(s.PullCrates(out p));
            Assert.AreEqual(4, p.side.items.Count, "Ro tre trong van choi");
            Assert.AreEqual(0.6 + 0.4 + 0.8 + 0.15, s.Formula.Score(1, 1), 1e-9, "Muoi hong trong bep");
        }

        // ---------- F-022 thung dot pha ----------

        [Test]
        public void F022_AC1_KhongCoRelic_100000Luot_KhongRaThungDotPha()
        {
            IngredientGacha g = Gacha();
            for (int i = 0; i < Items; i++) Assert.IsNull(g.Pull(1 + i % 3).breakthrough);
            // co relic khong phai Son Hao cung vay
            for (int i = 0; i < 20000; i++) Assert.IsNull(g.Pull(1, Fx("V1", "C1", "F2")).breakthrough);
        }

        [Test]
        public void F022_AC2_BanDo_TiLeThung10PhanTram_1Mon_70Bac4_30Bac5()
        {
            IngredientGacha g = Gacha(9);
            RelicFx fx = Fx("S1");
            int crates = 0, t4 = 0, t5 = 0;
            for (int i = 0; i < Items; i++)
            {
                Crate c = g.Pull(1, fx).breakthrough;
                if (c == null) continue;
                crates++;
                Assert.AreEqual(1, c.items.Count);
                Assert.IsTrue(c.isBreakthrough);
                Assert.AreEqual(IngredientKind.Main, c.kind);
                Assert.AreEqual("dotpha", TestData.Recipes.mains.Single(m => m.id == c.typeId).group);
                foreach (Ingredient it in c.items) { if (it.tier == 4) t4++; else if (it.tier == 5) t5++; else Assert.Fail("bac " + it.tier); }
            }
            Assert.That(100.0 * crates / Items, Is.InRange(10 - Tol, 10 + Tol));
            // ~10.000 mon: kiem them bang 100.000 mon o test duoi
            Assert.That(100.0 * t5 / (t4 + t5), Is.InRange(30 - 1.5, 30 + 1.5));
        }

        [Test]
        public void F022_AC2_BacMonDotPha_100000Mon_70_30()
        {
            IngredientGacha g = Gacha(10);
            RelicFx fx = Fx("S1", "S3", "S4"); // 3 mon bo Son Hao: thung 2 mon
            Assert.AreEqual(0.10 + 0.12 + 0.15 + 0.15, g.BreakthroughChance(fx), 1e-5);
            Assert.AreEqual(2, g.BreakthroughItems(fx));
            int t4 = 0, t5 = 0, crates = 0, pulls = 0;
            while (t4 + t5 < Items)
            {
                pulls++;
                Crate c = g.Pull(1, fx).breakthrough;
                if (c == null) continue;
                crates++;
                Assert.AreEqual(2, c.items.Count, "thuong bo 3: thung 2 mon");
                foreach (Ingredient it in c.items) if (it.tier == 5) t5++; else t4++;
            }
            Assert.That(100.0 * t5 / (t4 + t5), Is.InRange(30 - Tol, 30 + Tol));
            Assert.That(100.0 * crates / pulls, Is.InRange(52 - Tol, 52 + Tol));
        }

        [Test]
        public void S2_LuoiThep_Bac5Len50PhanTram_ThongKe()
        {
            IngredientGacha g = Gacha(12);
            RelicFx fx = Fx("S2");
            Assert.AreEqual(0.08, g.BreakthroughChance(fx), 1e-6);
            Assert.AreEqual(0.5, g.BreakthroughTier5(fx), 1e-6);
            int t4 = 0, t5 = 0;
            while (t4 + t5 < 20000)
            {
                Crate c = g.Pull(1, fx).breakthrough;
                if (c != null) foreach (Ingredient it in c.items) if (it.tier == 5) t5++; else t4++;
            }
            Assert.That(100.0 * t5 / (t4 + t5), Is.InRange(50 - 1.5, 50 + 1.5));
        }

        [Test]
        public void S3_S4_TiLeThung_Va_ThuongBo4_MoiMonBac5()
        {
            Assert.AreEqual(0.12, Gacha().BreakthroughChance(Fx("S3")), 1e-6);
            Assert.AreEqual(0.15, Gacha().BreakthroughChance(Fx("S4")), 1e-6);
            IngredientGacha g = Gacha(13);
            RelicFx four = Fx("S1", "S2", "S3", "S4");
            int seen = 0;
            for (int i = 0; i < 5000; i++)
            {
                Crate c = g.Pull(1, four).breakthrough;
                if (c == null) continue;
                Assert.IsTrue(c.items.All(it => it.tier == 5));
                seen += c.items.Count;
            }
            Assert.Greater(seen, 1000);
        }

        [Test]
        public void F022_AC3_AC4_MonDotPha_KhachGoiDuoc_NauRaMonDotPha()
        {
            // Tim van co thung dot pha ngay luot dau (relic Son Hao 4 mon: 80%/luot).
            for (int seed = 0; seed < 50; seed++)
            {
                GameSession s = RelicShopTests.NewSession(seed);
                foreach (string id in new[] { "S1", "S2", "S3", "S4" }) s.DebugGiveRelic(id);
                PullResult p;
                Assert.IsTrue(s.PullCrates(out p));
                if (p.breakthrough == null) continue;

                Assert.AreSame(p.breakthrough, s.LastBreakthrough);
                Ingredient bt = p.breakthrough.items[0];
                Assert.IsTrue(s.Stock.Items.Contains(bt));
                Recipe dish = s.Lookup.Find(bt.typeId, p.side.typeId);
                Assert.IsNotNull(dish, "co cong thuc dot pha");
                Assert.AreEqual("dotpha", dish.group);
                Assert.IsTrue(s.MakeableRecipes().Any(r => r.id == dish.id), "khach goi duoc mon dot pha (muc 6.7)");

                Assert.IsTrue(NaiveBot.CookFor(s, dish.id));
                Assert.AreEqual(dish.id, s.Counter.Dishes.Last().recipeId);
                return;
            }
            Assert.Fail("50 van khong co thung dot pha o luot dau");
        }

        // ---------- AC-5: bang ti le Goc 2 hien tac dung relic ----------

        [Test]
        public void AC5_TiLeHienTrenBang_KhopTinhTay_VaKhopQuayThat()
        {
            int[] r1 = TestData.Balance.OddsForDay(Day1).percents;
            double[] none = IngredientGacha.EffectiveOdds(r1, RelicFx.None, IngredientKind.Side);
            for (int t = 0; t < 6; t++) Assert.AreEqual(R1()[t], none[t], 1e-9);
            double[] v1 = IngredientGacha.EffectiveOdds(r1, Fx("V1"), IngredientKind.Side);
            double[] up = Up(R1(), 0.15);
            for (int t = 0; t < 6; t++) Assert.AreEqual(up[t], v1[t], 1e-6); // 0,15f la float

            // to hop nhieu hieu ung: tinh tren bang = quay that (+-1 diem)
            foreach (string[] ids in new[] { new[] { "V1", "V2", "V3", "V4" }, new[] { "C1", "C2", "C4" }, new[] { "C1", "C2", "C3" } })
            {
                RelicFx fx = Fx(ids);
                IngredientKind kind = ids[0].StartsWith("V") ? IngredientKind.Side : IngredientKind.Main;
                AssertClose(IngredientGacha.EffectiveOdds(r1, fx, kind), Distribution(fx, kind), string.Join("+", ids));
            }
            Assert.AreEqual(4, IngredientGacha.CrateCount(3, Fx("V2"), IngredientKind.Side));
        }

        static RelicInventory Inventory(params string[] ids)
        {
            var inv = new RelicInventory(TestData.Balance);
            foreach (string id in ids) Assert.IsTrue(inv.Add(RelicShopTests.Def(id)));
            return inv;
        }
    }
}
