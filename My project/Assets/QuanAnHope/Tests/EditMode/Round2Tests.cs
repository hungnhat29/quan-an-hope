using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-035 Ngay 8 mat dien, F-036 Noi dung vong 2, muc tieu 1 + 7 (choi lien ngay 1 -> dau ngay 9, thu 2.900G).
    /// Nguon: constitution 1.4 muc 2, 4.3, 4.5, 5.2, 5.3, 5.8, 5.9, 6.6, Dieu 4; docs/sprint-4/impl-plan.md muc 1, 2.
    /// </summary>
    public class Round2Tests
    {
        static Balance B { get { return TestData.Balance; } }

        // ---------- F-035: mat dien ----------

        [Test]
        public void F035_Ngay8_MatDien_CacNgayKhacKhong()
        {
            Assert.IsTrue(TestData.SessionAt(8).IsBlackout);
            Assert.IsFalse(TestData.SessionAt(7).IsBlackout);
            Assert.IsFalse(TestData.SessionAt(4).IsBlackout);
            Assert.AreEqual(0.6f, B.blackout.dim, 1e-6, "phong toi 60% (impl plan F-035 AC-1)");
        }

        [Test]
        public void F035_AC2_DiemMonTru03()
        {
            GameSession day7 = TestData.SessionAt(7), day8 = TestData.SessionAt(8);
            double normal = day7.Formula.Score(2, 2), dark = day8.Formula.Score(2, 2);
            Assert.AreEqual(2.8, normal, 1e-4);
            Assert.AreEqual(2.5, dark, 1e-4, "ngay mat dien: b - 0,3 (muc 5.3)");
            TierChance[] bar = day8.Formula.Bar(2, 2);
            Assert.AreEqual(2, bar[0].tier);
            Assert.AreEqual(0.5, bar[0].chance, 1e-4);
        }

        [Test]
        public void F035_AC2_ThoiGianNau_Cong25PhanTram()
        {
            GameSession s = TestData.SessionAt(8);
            Recipe r = s.Lookup.Find("bo", "khoai_tay");
            Ingredient main = TestData.Main("bo", 2), side = TestData.Side("khoai_tay", 2);
            s.Stove.SelectTool(r.tool);
            foreach (string step in r.steps)
            {
                if (step == main.name) s.Stove.AddIngredient(main);
                else if (step == side.name) s.Stove.AddIngredient(side);
                else s.Stove.AddPantry(step);
            }
            Assert.IsTrue(s.Stove.Start());
            Assert.AreEqual(r.cookSeconds * 1.25f, s.Stove.CookSeconds, 1e-4);
            s.Stove.Tick(r.cookSeconds);
            Assert.AreEqual(StoveState.Cooking, s.Stove.State, "15 giay chua chin khi nau bang nen");
            s.Stove.Tick(r.cookSeconds * 0.25f);
            Assert.AreEqual(StoveState.Ready, s.Stove.State);
        }

        // ---------- F-036: noi dung vong 2 ----------

        [Test]
        public void F036_Vong2_Mo4NguyenLieuMoi_25MonTrongSach()
        {
            GameSession s = TestData.SessionAt(4);
            List<Recipe> book = s.BookRecipes();
            Assert.AreEqual(25, book.Count, "5 chinh x 5 phu");
            Assert.AreEqual(16, book.Count(r => r.unlockRound == 2), "16 mon moi");
            foreach (string id in new[] { "khoai_tay", "nam_rom" })
                Assert.AreEqual(2, TestData.Recipes.sides.Single(x => x.id == id).unlockRound);
            foreach (string id in new[] { "bo", "tom" })
                Assert.AreEqual(2, TestData.Recipes.mains.Single(x => x.id == id).unlockRound);
            CollectionAssert.IsSupersetOf(book.Select(r => r.baseName).Distinct().ToList(), new[] { "Mì Ý", "Bún" });
            Assert.AreEqual("Mì Ý bò sốt cà chua", s.Lookup.Find("bo", "ca_chua").name);
            Assert.AreEqual(70, B.BasePrice(2));
        }

        [Test]
        public void F036_Ngay4_ThungChiRaNguyenLieuVong2TroXuong_TheoBangVong2()
        {
            Assert.AreEqual("r2", B.OddsForDay(4).id);
            Assert.AreEqual("r2", B.OddsForDay(8).id);
            var allowedSide = new HashSet<string>(TestData.Recipes.sides.Where(x => x.unlockRound <= 2).Select(x => x.id));
            var allowedMain = new HashSet<string>(TestData.Recipes.mains.Where(x => x.group == "chinh" && x.unlockRound <= 2).Select(x => x.id));
            var seen = new HashSet<string>();
            var gacha = new IngredientGacha(B, TestData.Recipes, new SeededRandom(4));
            for (int i = 0; i < 400; i++)
            {
                PullResult p = gacha.Pull(4);
                Assert.IsTrue(allowedSide.Contains(p.side.typeId));
                Assert.IsTrue(allowedMain.Contains(p.main.typeId));
                seen.Add(p.side.typeId); seen.Add(p.main.typeId);
            }
            CollectionAssert.IsSupersetOf(seen, new[] { "khoai_tay", "nam_rom", "bo", "tom" });
        }

        // ---------- muc tieu 1, 7: choi lien ngay 1 -> dau ngay 9 ----------

        [Test]
        public void MucTieu_QuaVong1_SangNgay4_KhongDung()
        {
            GameSession s = TestData.SessionAt(3, 1000, 1, 8);
            s.OpenShop(); s.CloseEarly();
            Assert.IsTrue(s.StartNextDay());
            Assert.AreEqual(4, s.Day);
            Assert.AreEqual(GamePhase.Prep, s.Phase);
            Assert.AreEqual(750, s.LastDebtPaid);
            Assert.AreEqual(250, s.Money);
        }

        [Test]
        public void MucTieu_DauNgay9_Thu2900()
        {
            GameSession s = TestData.SessionAt(8, 3000, 1, 8);
            s.OpenShop(); s.CloseEarly();
            s.StartNextDay();
            Assert.AreEqual(9, s.Day);
            Assert.AreEqual(2900, s.LastDebtPaid);
            Assert.AreEqual(100, s.Money);
            Assert.AreEqual(GamePhase.Cleared, s.Phase);

            GameSession poor = TestData.SessionAt(8, 2899, 1, 8);
            poor.OpenShop(); poor.CloseEarly();
            poor.StartNextDay();
            Assert.AreEqual(GamePhase.Lost, poor.Phase);
        }

        [Test]
        public void MucTieu_BanChoiSprint4_DenHetVong2()
        {
            Assert.AreEqual(8, GameSession.LastPlayableDayFor(B, 2));
        }

        [Test]
        public void Bot_ChoiTuNgay1_ToiNgay9_KhongLoi()
        {
            int cleared = 0, lost = 0;
            for (int seed = 1; seed <= 20; seed++)
            {
                var s = new GameSession(B, TestData.Recipes, new SeededRandom(seed), 8);
                GamePhase end = NaiveBot.PlayToEnd(s, BotTiming.Naive);
                if (end == GamePhase.Cleared) { cleared++; Assert.AreEqual(9, s.Day); }
                else { lost++; Assert.That(s.Day, Is.EqualTo(4).Or.EqualTo(9)); }
            }
            Assert.AreEqual(20, cleared + lost);
        }
    }
}
