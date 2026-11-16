using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-020 AC-1..6 (Core). Nguon: constitution 1.4 muc 3, 7.1, Dieu 4; specs/sprint-3/F-020-cua-hang-relic/spec.md.
    /// </summary>
    public class RelicShopTests
    {
        const int LastDayOfRound1 = 3;

        internal static GameSession NewSession(int seed = 1, int lastDay = LastDayOfRound1)
        {
            return new GameSession(TestData.Balance, TestData.Recipes, new SeededRandom(seed), lastDay);
        }

        /// <summary>Het ngay hien tai ngay luc chua mo quan roi sang ngay sau.</summary>
        internal static void NextDay(GameSession s)
        {
            s.OpenShop();
            s.CloseEarly();
            Assert.IsTrue(s.StartNextDay());
        }

        internal static RelicDef Def(string id)
        {
            return TestData.Balance.relics.Single(r => r.id == id);
        }

        [Test]
        public void Data_22Relic_Gia8_3Be_ThuongBo9Nguong()
        {
            Balance b = TestData.Balance;
            Assert.AreEqual(22, b.relics.Length);
            Assert.AreEqual(8, b.relicCap);
            Assert.AreEqual(3, b.relicShopOffers);
            Assert.AreEqual(9, b.setBonuses.Length);
            Assert.AreEqual(0f, b.breakthrough.baseChance);
            Assert.AreEqual(1, b.breakthrough.items);
            Assert.AreEqual(0.3f, b.breakthrough.tier5Share, 1e-6);
            Assert.AreEqual(1, Def("C1").price);
            Assert.AreEqual("cho", Def("C1").set);
            Assert.AreEqual(0.15f, Def("C1").effects.Single(e => e.key == "chinh_up").value, 1e-6);
        }

        [Test]
        public void AC1_Ngay1_BaBeTrong_KhongMuaDuoc()
        {
            GameSession s = NewSession();
            Assert.IsFalse(s.Shop.IsOpen);
            Assert.IsTrue(s.Shop.Offers.All(o => o == null));
            Assert.AreEqual(RelicBuyResult.Closed, s.BuyRelic(0));
            Assert.AreEqual(8, s.Wallet.Count);
        }

        [Test]
        public void AC2_DauNgay2_3RelicKhacNhau_KhongTrungRelicDangCo()
        {
            for (int seed = 0; seed < 300; seed++)
            {
                GameSession s = NewSession(seed);
                // co san vai relic de kiem "khong trung relic dang co"
                foreach (string id in new[] { "V1", "C1", "S1", "F2", "F4" }) Assert.IsTrue(s.DebugGiveRelic(id));
                NextDay(s);
                Assert.IsTrue(s.Shop.IsOpen);
                List<RelicDef> offers = s.Shop.Offers.ToList();
                Assert.AreEqual(3, offers.Count);
                Assert.IsTrue(offers.All(o => o != null));
                Assert.AreEqual(3, offers.Select(o => o.id).Distinct().Count(), "3 relic khac nhau");
                Assert.IsFalse(offers.Any(o => s.Relics.Has(o.id)), "khong trung relic dang co");
            }
        }

        [Test]
        public void AC2_MoiRelicChuaCo_DeuCoTheRa_RelicDaBoQuayLai()
        {
            var seen = new HashSet<string>();
            for (int seed = 0; seed < 400; seed++)
            {
                GameSession s = NewSession(seed);
                s.DebugGiveRelic("V1");
                s.Relics.Remove("V1"); // da bo thi quay lai ro rut
                NextDay(s);
                foreach (RelicDef o in s.Shop.Offers) seen.Add(o.id);
            }
            Assert.AreEqual(22, seen.Count);
        }

        [Test]
        public void AC3_DuVe_MuaTruDungVe_LenGia_BeTrongToiHetNgay()
        {
            GameSession s = NewSession(5);
            NextDay(s);
            int before = s.Wallet.Count;
            RelicDef offer = s.Shop.Offers[1];
            Assert.AreEqual(RelicBuyResult.Bought, s.BuyRelic(1));
            Assert.AreEqual(before - offer.price, s.Wallet.Count);
            Assert.IsTrue(s.Relics.Has(offer.id));
            Assert.IsNull(s.Shop.Offers[1]);
            Assert.AreEqual(RelicBuyResult.Empty, s.BuyRelic(1));
            Assert.IsNotNull(s.Shop.Offers[0]);

            // hang moi vao dau ngay sau
            NextDay(s);
            Assert.IsTrue(s.Shop.Offers.All(o => o != null));
        }

        [Test]
        public void AC4_ThieuVe_KhongMua_BaoThieuMayVe()
        {
            GameSession s = NewSession(7);
            NextDay(s);
            int slot = Enumerable.Range(0, 3).OrderByDescending(i => s.Shop.Offers[i].price).First();
            RelicDef offer = s.Shop.Offers[slot];
            while (s.Wallet.Count > offer.price - 1) Assert.IsTrue(s.Wallet.TrySpend());
            int tickets = s.Wallet.Count;
            Assert.AreEqual(RelicBuyResult.NotEnoughTickets, s.BuyRelic(slot));
            Assert.AreEqual(tickets, s.Wallet.Count);
            Assert.AreEqual(offer.price - tickets, s.Shop.Missing(slot, s.Wallet));
            Assert.IsFalse(s.Relics.Has(offer.id));
        }

        [Test]
        public void AC5_Day8_PhaiChonRelicDeBo_HuyThiKhongMua()
        {
            GameSession s = NewSession(11);
            foreach (string id in new[] { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8" }) Assert.IsTrue(s.DebugGiveRelic(id));
            Assert.IsTrue(s.Relics.IsFull);
            NextDay(s);
            for (int i = 0; i < 20; i++) s.Wallet.BeginDay(2); // du ve
            int tickets = s.Wallet.Count;
            RelicDef offer = s.Shop.Offers[0];

            Assert.AreEqual(RelicBuyResult.NeedDiscard, s.BuyRelic(0));
            Assert.AreEqual(tickets, s.Wallet.Count, "huy: khong mat ve");
            Assert.AreEqual(8, s.Relics.Owned.Count);
            Assert.IsNotNull(s.Shop.Offers[0]);

            Assert.AreEqual(RelicBuyResult.NeedDiscard, s.BuyRelic(0, "khong_co"), "bo relic khong co tren gia");

            Assert.AreEqual(RelicBuyResult.Bought, s.BuyRelic(0, "F3"));
            Assert.AreEqual(8, s.Relics.Owned.Count);
            Assert.IsFalse(s.Relics.Has("F3"));
            Assert.IsTrue(s.Relics.Has(offer.id));
            Assert.AreEqual(tickets - offer.price, s.Wallet.Count);
        }

        [Test]
        public void AC6_MoiRelic_TheCoTenGiaDoHiemHieuUng()
        {
            foreach (RelicDef r in TestData.Balance.relics)
            {
                Assert.IsFalse(string.IsNullOrEmpty(r.name), r.id);
                Assert.Greater(r.price, 0, r.id);
                Assert.IsTrue(new[] { "Thường", "Hiếm", "Huyền thoại" }.Contains(RelicKeys.RarityName(r.rarity)), r.id);
                string fx = RelicText.Effect(r);
                Assert.IsFalse(string.IsNullOrEmpty(fx), r.id);
                Assert.IsFalse(fx.Contains("_"), r.id + ": hieu ung chua dich: " + fx);
            }
            Assert.AreEqual("Nguyên liệu chính: 15% nâng bậc", RelicText.Effect(Def("C1")));
        }

        [Test]
        public void F021_RelicTuLanhGioChoNuocSot_TuSprint4DaCoTacDung()
        {
            // Sprint 3 ghi "tac dung tu ngay 4"; sprint 4 (F-031 AC-4, F-032 AC-3, F-034) bat ca bon.
            foreach (string id in new[] { "F1", "F3", "F7", "F9" }) Assert.IsTrue(RelicText.IsActiveNow(Def(id)), id);
            foreach (string id in new[] { "V1", "C1", "S1", "F2", "F4", "F5", "F6", "F8", "F10" }) Assert.IsTrue(RelicText.IsActiveNow(Def(id)), id);
            GameSession s = NewSession();
            Assert.IsTrue(s.DebugGiveRelic("F1"));
            Assert.IsTrue(s.Relics.Has("F1"));
        }
    }
}
