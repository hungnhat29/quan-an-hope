using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sau sprint 10 (yeu cau 23/11/2026, spec specs/sprint-10b/spec.md, constitution 2.1): 8 ve moi ngay (C-03), ti le 6 bac khi nau
    /// + Huyen thoai ngan rieng (C-05), bep mua them (C-06), mon ngon nhat cho chu no (C-09), relic Huyen thoai (C-05).
    /// </summary>
    public class Sprint11Tests
    {
        static Balance B { get { return TestData.Balance; } }
        static RecipeBook R { get { return TestData.Recipes; } }

        static GameSession NewGame(int seed = 1)
        {
            return new GameSession(B, R, new SeededRandom(seed), GameSession.LastDayOfGame(B));
        }

        // ---------- C-03: 8 ve moi ngay, ngay 1 van 5 ----------

        [Test]
        public void C03_MoiNgay8Ve_Ngay1Van5Ve()
        {
            var w = new TicketWallet(B);
            w.BeginDay(1);
            Assert.AreEqual(5, w.Count, "ngay 1: 5 ve");
            for (int day = 2; day <= 22; day++)
            {
                w.TrySpend(w.Count);
                w.BeginDay(day, RelicFx.None);
                Assert.AreEqual(8, w.Count, "ngay " + day + ": tieu het hom qua thi con dung 8 ve");
            }
        }

        // ---------- C-05: ti le 6 bac (bang constitution 2.1 muc 5.3) ----------

        static DishFormula F(params string[] relics)
        {
            var f = new DishFormula(B);
            if (relics.Length > 0)
            {
                var inv = new RelicInventory(B);
                foreach (string id in relics) inv.Add(B.relics.First(r => r.id == id));
                f.Relics = inv;
            }
            return f;
        }

        static double Pct(TierChance[] bar, int tier) { return bar.Where(c => c.tier == tier).Sum(c => c.chance) * 100.0; }

        static readonly object[] Table =
        {
            new object[] { 0, 0, new[] { 18.53, 62.61, 11.16, 7.32, 0.38, 0.001 } },
            new object[] { 0, 1, new[] { 12.81, 45.26, 28.98, 11.29, 1.66, 0.002 } },
            new object[] { 1, 1, new[] { 4.17, 14.36, 62.61, 11.16, 7.71, 0.005 } },
            new object[] { 2, 1, new[] { 0.90, 8.68, 33.89, 41.13, 15.38, 0.012 } },
            new object[] { 2, 2, new[] { 0.21, 3.96, 14.36, 62.60, 18.86, 0.021 } },
            new object[] { 3, 3, new[] { 0.01, 0.20, 3.96, 14.34, 81.40, 0.096 } },
            new object[] { 4, 2, new[] { 0.007, 0.13, 2.57, 13.12, 84.04, 0.13 } },
            new object[] { 4, 4, new[] { 0.001, 0.01, 0.20, 3.94, 95.41, 0.44 } },
            new object[] { 5, 4, new[] { 0.001, 0.002, 0.042, 0.85, 98.02, 1.09 } },
            new object[] { 5, 5, new[] { 0.001, 0.001, 0.01, 0.19, 97.80, 2.00 } },
        };

        [TestCaseSource(nameof(Table))]
        public void C05_ThanhTiLe_KhopBangConstitution(int main, int side, double[] want)
        {
            TierChance[] bar = F().Bar(main, side);
            Assert.AreEqual(6, bar.Length, "du 6 bac");
            Assert.AreEqual(1.0, bar.Sum(c => c.chance), 1e-9, "luoi 100.000 ve cong dung 100%");
            for (int t = 0; t < 6; t++)
                Assert.AreEqual(want[t], Pct(bar, t), System.Math.Max(0.011, want[t] * 0.02), "bac " + t);
        }

        [Test]
        public void C05_MoiBacToiThieu1Ve_LuoiNguyen()
        {
            var f = F();
            for (int m = 0; m <= 5; m++)
                for (int s = 0; s <= 5; s++)
                {
                    int[] t = f.Tickets(m, s);
                    Assert.AreEqual(100000, t.Sum());
                    Assert.IsTrue(t.All(x => x >= 1), "chinh " + m + " phu " + s + ": bac nao cung con ra duoc");
                }
        }

        [Test]
        public void C05_NguyenLieuTotHon_KhongLamGiamXacSuatRaBacCaoHon()
        {
            var f = F();
            for (int m1 = 0; m1 <= 5; m1++)
                for (int s1 = 0; s1 <= 5; s1++)
                    for (int m2 = m1; m2 <= 5; m2++)
                        for (int s2 = s1; s2 <= 5; s2++)
                        {
                            double[] a = f.Chances(m1, s1), b = f.Chances(m2, s2);
                            for (int k = 1; k < 6; k++)
                                Assert.GreaterOrEqual(b.Skip(k).Sum() + 1e-9, a.Skip(k).Sum(), m1 + "/" + s1 + " -> " + m2 + "/" + s2 + " bac >= " + k);
                        }
        }

        [Test]
        public void C05_HuyenThoai_ToiDa2PhanTramKhiNau_RelicCongThem_ToiDa5()
        {
            Assert.AreEqual(0.00001, F().LegendChance(0, 0), 1e-9);
            Assert.AreEqual(0.02, F().LegendChance(5, 5), 1e-9);
            Assert.AreEqual(0.30, Pct(F("F11").Bar(1, 1), 5), 0.011, "Sao bang trong noi +0,3%");
            Assert.AreEqual(1.74, Pct(F("F11", "F12").Bar(4, 4), 5), 0.02, "ca 2 relic +1,3%");
            Assert.AreEqual(94.17, Pct(F("F11", "F12").Bar(4, 4), 4), 0.05);
            Assert.AreEqual(3.3, Pct(F("F11", "F12").Bar(5, 5), 5), 0.02, "2% + 1,3%");
            var cap = F("F11", "F12");
            Assert.LessOrEqual(cap.LegendChance(5, 5), B.dish.odds.legendCap + 1e-12);
        }

        [Test]
        public void C05_NgayMatDien_ChaoGang_KhopBang()
        {
            var dark = F();
            dark.Blackout = true;
            TierChance[] bar = dark.Bar(1, 1);
            double[] want = { 8.03, 28.28, 47.26, 12.45, 3.97, 0.005 };
            for (int t = 0; t < 6; t++) Assert.AreEqual(want[t], Pct(bar, t), System.Math.Max(0.011, want[t] * 0.02), "mat dien bac " + t);
            TierChance[] pan = F("F5").Bar(0, 0);
            Assert.AreEqual(0.0, Pct(pan, 0), 1e-9, "Chao gang chan han Lac vi");
            Assert.AreEqual(81.14, Pct(pan, 1), 0.02);
        }

        [Test]
        public void C05_QuayBacMon_TanSuatKhopThanhTiLe_Dieu3()
        {
            var f = F();
            var rng = new SeededRandom(20261123);
            var n = new int[6];
            const int N = 200000;
            for (int i = 0; i < N; i++) n[f.Roll(1, 1, rng)]++;
            TierChance[] bar = f.Bar(1, 1);
            for (int t = 0; t < 6; t++) Assert.AreEqual(Pct(bar, t), n[t] * 100.0 / N, 0.35, "bac " + t);
            // nguyen lieu xin nhat: Huyen thoai ra duoc (2%)
            int legends = 0;
            for (int i = 0; i < N; i++) if (f.Roll(5, 5, rng) == 5) legends++;
            Assert.AreEqual(2.0, legends * 100.0 / N, 0.15);
        }

        [Test]
        public void C05_DinhDangPhanTram()
        {
            Assert.AreEqual("62,61%", DishFormula.FormatPercent(0.6261));
            Assert.AreEqual("1,00%", DishFormula.FormatPercent(0.01));
            Assert.AreEqual("0,005%", DishFormula.FormatPercent(0.00005));
            Assert.AreEqual("0,001%", DishFormula.FormatPercent(0.00001));
            Assert.AreEqual("0,437%", DishFormula.FormatPercent(0.00437));
            Assert.AreEqual("0,001%", DishFormula.FormatPercent(0.000001), "khong bao gio hien 0% cho bac con ra duoc");
            Assert.AreEqual("0%", DishFormula.FormatPercent(0.0));
        }

        [Test]
        public void C05_MonHuyenThoai_GiuTenMon_SotKhongLamTut()
        {
            var d = new Dish { recipeId = "trung__ca_chua", name = "Trứng chiên cà chua", tier = Dish.LegendTier };
            Assert.IsTrue(d.IsLegend);
            Assert.AreEqual("Trứng chiên cà chua", d.Title, "2.1: Huyen thoai la bac cua moi mon");
            Assert.AreEqual(5, ItemRolls.ApplySauce(SauceResult.Up1, 5, B), "+1 khong keo Huyen thoai ve Tuyet hao");
            Assert.AreEqual(5, ItemRolls.ApplySauce(SauceResult.Up2, 5, B));
            Assert.AreEqual(0, ItemRolls.ApplySauce(SauceResult.Fail, 5, B));
            Assert.AreEqual(5, ItemRolls.ApplyMachine(MachineResult.Up1, 5, B));
            Assert.AreEqual(4, ItemRolls.ApplySauce(SauceResult.Up1, 3, B));
        }

        [Test]
        public void C05_HaiRelicMoi_CoTrongDuLieu()
        {
            RelicDef a = B.relics.First(r => r.id == "F11"), b = B.relics.First(r => r.id == "F12");
            Assert.AreEqual("Sao băng trong nồi", a.name);
            Assert.AreEqual(0.003, a.effects.First(e => e.key == RelicKeys.DishLegend).value, 1e-6);
            Assert.AreEqual(0.01, b.effects.First(e => e.key == RelicKeys.DishLegend).value, 1e-6);
        }

        // ---------- C-06: bep mua them ----------

        [Test]
        public void C06_MuaBep_500G_ToiDa2_LuuTai()
        {
            GameSession s = NewGame();
            Assert.AreEqual(1, s.Stoves.Count);
            s.DebugJumpToDay(4);
            s.DebugAddMoney(2000 - s.Money);
            Assert.AreEqual(500, s.Items.StovePrice);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyStove());
            Assert.AreEqual(1500, s.Money);
            Assert.AreEqual(2, s.Stoves.Count);
            Assert.AreEqual(ItemBuyResult.AtLimit, s.BuyStove(), "toi da 2 bep");
            Assert.AreEqual(800, B.stove.price + B.stove.step, "lan mua thu hai (neu mo rong) 800G");

            SaveData data = s.Snapshot();
            GameSession back = GameSession.FromSave(B, R, data);
            Assert.AreEqual(2, back.Stoves.Count, "so bep luu trong ban luu");
        }

        [Test]
        public void C06_MuaBep_TruocNgay4_DongCua()
        {
            GameSession s = NewGame();
            s.DebugAddMoney(5000);
            Assert.AreEqual(ItemBuyResult.Closed, s.BuyStove());
        }

        [Test]
        public void C06_HaiBep_NauDocLap()
        {
            GameSession s = NewGame(3);
            s.DebugJumpToDay(4);
            s.DebugAddMoney(1000);
            Assert.AreEqual(ItemBuyResult.Bought, s.BuyStove());
            Recipe r = R.recipes.First(x => x.id == "trung__ca_chua");
            Assert.IsTrue(s.Stoves[1].SelectTool(r.tool));
            Assert.AreEqual(StoveState.Empty, s.Stove.State, "bep 1 khong bi dong vao");
            Assert.IsNull(s.Stove.Tool);
            Assert.AreEqual(r.tool, s.Stoves[1].Tool);
            Assert.AreEqual(s.Stoves[1].BurnEnabled, s.Stove.BurnEnabled, "luat qua lua ap cho moi bep");
        }

        // ---------- C-09: mon ngon nhat cho chu no ----------

        [Test]
        public void C09_ChuNo_TrungChienCaChuaHuyenThoai_ThuongX1_5()
        {
            Assert.AreEqual("trung__ca_chua", B.bossBest.recipe);
            GameSession s = NewGame();
            var best = new Dish { recipeId = "trung__ca_chua", name = "Trứng chiên cà chua", tier = 5 };
            var top = new Dish { recipeId = "trung__ca_chua", name = "Trứng chiên cà chua", tier = 4 };
            var other = new Dish { recipeId = R.recipes.First(x => x.id != "trung__ca_chua" && x.group == "chinh").id, name = "x", tier = 5 };
            Assert.IsTrue(s.IsBossBest(best));
            Assert.IsFalse(s.IsBossBest(top), "Tuyet hao: thuong thuong");
            Assert.IsFalse(s.IsBossBest(other), "mon khac Huyen thoai: thuong thuong");
            Assert.AreEqual(17250, (int)System.Math.Round(B.bossBonus * B.bossBest.bonusMult));
        }
    }
}
