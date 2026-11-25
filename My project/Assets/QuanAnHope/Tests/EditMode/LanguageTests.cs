using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.EditorTools;
using QuanAnHope.Game;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sprint 11 (specs/sprint-11/spec.md): F-110 tieng Anh (yeu cau 25/11/2026), F-112 giong khach giai nen luc nap (K-12),
    /// F-114 so phien ban ban release candidate.
    /// </summary>
    public class LanguageTests
    {
        static Translator Small()
        {
            return new Translator(new Dictionary<string, string>
            {
                { "Chơi mới", "New game" },
                { "Cà chua", "Tomato" },
                { "cà chua", "tomato" },
                { "Trứng chiên cà chua", "Fried egg with tomato" },
                { "Đã mua {0} · còn {1} vé.", "Bought {0} · {1} tickets left." },
                { "Ví <size=26>{0}G</size>\n", "Wallet <size=26>{0}G</size>\n" },
                { "Nợ <size=26>{0}G</size> đầu ngày {1}", "Debt <size=26>{0}G</size> at start of day {1}" },
                { "Ngày {0}", "Day {0}" },
                { "Hết vé", "No tickets" },
                { "khách trái", "left customer" },
                { "Bơ", "Butter" },
                { "đã sơ chế", "prepped" },
            });
        }

        // ---------------- AC-2 bo dich ----------------

        [Test]
        public void AC2_NguyenVan_VaKhoangTrangDauCuoi()
        {
            Translator t = Small();
            Assert.AreEqual("New game", t.Translate("Chơi mới"));
            Assert.AreEqual("  New game ", t.Translate("  Chơi mới "), "giu khoang trang dau / cuoi");
            Assert.AreEqual("Left customer", t.Translate("Khách trái"), "chu dau viet hoa (char.ToUpper o RoomViewRound2) van tra duoc");
            Assert.AreEqual("NO TICKETS", t.Translate("HẾT VÉ"), "chu in hoa (ToUpper) van tra duoc");
        }

        [Test]
        public void AC2_MauCoLo_DichCaCau_ChuTrongLoDichTiep()
        {
            Translator t = Small();
            Assert.AreEqual("Bought Fried egg with tomato · 3 tickets left.", t.Translate("Đã mua Trứng chiên cà chua · còn 3 vé."));
            Assert.AreEqual("Day 4", t.Translate("Ngày 4"));
        }

        [Test]
        public void AC2_NhieuDong_DichTungDong_VaSoKieuAnh()
        {
            Translator t = Small();
            string vi = "Ví <size=26>32.000G</size>\nNợ <size=26>1.500G</size> đầu ngày 9";
            Assert.AreEqual("Wallet <size=26>32,000G</size>\nDebt <size=26>1,500G</size> at start of day 9", t.Translate(vi));
        }

        [Test]
        public void AC2_ThayManh_DungRanhGioiTu()
        {
            Translator t = Small();
            Assert.AreEqual("Tomato prepped", t.Translate("Cà chua đã sơ chế"), "manh con lai thay theo tu");
            Assert.AreEqual("Bơm", t.Translate("Bơm"), "\"Bơ\" khong thay ben trong tu \"Bơm\"");
        }

        /// <summary>Loi gap 25/11: mau "Tủ lạnh +{0}" nuot ca " · món không tụt bậc" vao lo -> "items không tụt tier".</summary>
        [Test]
        public void AC2_LoNuotDauNgan_ChonCachTachDoan()
        {
            var t = new Translator(new Dictionary<string, string>
            {
                { "Tủ lạnh +{0}", "Fridge +{0}" }, { "{0} ô", "{0} slots" }, { "{0} bậc", "{0} tier" }, { "món không tụt bậc", "dishes don't lose tiers" },
                { "Cửa hàng · ví {0}G{1}", "Shop · wallet {0}G{1}" }, { " · mở từ ngày {0}", " · opens on day {0}" },
            });
            Assert.AreEqual("Fridge +2 slots · dishes don't lose tiers", t.Translate("Tủ lạnh +2 ô · món không tụt bậc"));
            Assert.AreEqual("Shop · wallet 120G · opens on day 4", t.Translate("Cửa hàng · ví 120G · mở từ ngày 4"), "lo chua dau ngan hop le van theo mau");
        }

        /// <summary>Anh en_* 25/11: nut "Xong", "Mua", "Sau ›" khong co dau nen bo dich bo qua; cau co mau day du phai theo mau.</summary>
        [Test]
        public void AC2_ChuKhongDau_VaMauDayDu()
        {
            Translator t = Lang.Table;
            Assert.AreEqual("Done", t.Translate("Xong"));
            Assert.AreEqual("Buy", t.Translate("Mua"));
            Assert.AreEqual("Next ›", t.Translate("Sau ›"));
            Assert.AreEqual("Left mouse · Insert ticket · 5 tickets left", t.Translate("Chuột trái · Nhét vé · còn 5 vé"));
            Assert.AreEqual("Tab", t.Translate("Tab"), "chu tieng Anh san giu nguyen");
        }

        [Test]
        public void AC2_DoiSo_ChiDoiSoKieuViet()
        {
            Assert.AreEqual("32,000G", Translator.EnglishNumbers("32.000G"));
            Assert.AreEqual("1,250,000G", Translator.EnglishNumbers("1.250.000G"));
            Assert.AreEqual("0.3%", Translator.EnglishNumbers("0,3%"));
            Assert.AreEqual("Version 0.10.4", Translator.EnglishNumbers("Version 0.10.4"), "so phien ban giu nguyen");
            Assert.AreEqual("19:02", Translator.EnglishNumbers("19:02"));
            Assert.AreEqual("<size=80%>", Translator.EnglishNumbers("<size=80%>"));
        }

        [Test]
        public void AC2_ConSot_GhiVaoLeftovers()
        {
            Translator t = Small();
            Assert.IsTrue(Translator.HasVietnamese("Bếp"));
            Assert.IsFalse(Translator.HasVietnamese("Stove 2 · 9:00"));
            t.Translate("Một câu chưa có trong bảng");
            Assert.IsTrue(t.Leftovers.Contains("Một câu chưa có trong bảng"));
        }

        [Test]
        public void AC2_TiengViet_TraNguyenVan()
        {
            Lang.Set(Lang.Vietnamese);
            Assert.AreEqual("Chơi mới", Lang.T("Chơi mới"));
            Lang.Set(Lang.English);
            Assert.AreEqual("New game", Lang.T("Chơi mới"));
            Lang.Set(Lang.Vietnamese);
        }

        // ---------------- AC-3 bang dich phu moi chuoi ----------------

        [System.Serializable] class ExtractedItem { public string vi; }
        [System.Serializable] class Extracted { public ExtractedItem[] items; }

        static string Repo { get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..")); } }

        [Test]
        public void AC3_BangDich_PhuMoiChuoiGomDuoc()
        {
            string p = Path.Combine(Repo, "tools", "i18n", "strings_vi.json");
            Assert.IsTrue(File.Exists(p), "chay tools/i18n/extract.py");
            var all = JsonUtility.FromJson<Extracted>("{\"items\":" + File.ReadAllText(p) + "}");
            Translator t = Lang.Table;
            Assert.Greater(t.Count, 800);
            var missing = all.items.Select(x => x.vi).Where(s => !t.Has(s)).ToList();
            Assert.IsEmpty(missing, "thieu ban dich (chay tools/i18n/build.py): " + string.Join(" | ", missing.Take(20)));
        }

        [Test]
        public void AC3_Ten84Mon_VaNguyenLieu_DichHet()
        {
            Translator t = Lang.Table;
            RecipeBook r = TestData.Recipes;
            foreach (Recipe x in r.recipes)
                Assert.IsFalse(Translator.HasVietnamese(t.Translate(x.name)), x.name + " -> " + t.Translate(x.name));
            foreach (MainIngredient m in r.mains) Assert.IsFalse(Translator.HasVietnamese(t.Translate(m.name)), m.name);
            foreach (SideIngredient s in r.sides) Assert.IsFalse(Translator.HasVietnamese(t.Translate(s.name)), s.name);
            foreach (RelicDef d in TestData.Balance.relics) Assert.IsFalse(Translator.HasVietnamese(t.Translate(d.name)), d.name);
            Assert.AreEqual("Fried egg with tomato", t.Translate("Trứng chiên cà chua"));
        }

        [Test]
        public void AC3_ChuTuDong_RelicVaCotTruyen_DichHet()
        {
            Translator t = Lang.Table;
            foreach (RelicDef d in TestData.Balance.relics)
            {
                string fx = RelicText.Effect(d);
                Assert.IsFalse(Translator.HasVietnamese(t.Translate(fx)), d.id + ": " + fx + " -> " + t.Translate(fx));
            }
            foreach (StoryPanel s in StoryScript.Intro(TestData.Balance).Concat(StoryScript.Win(TestData.Balance, 1000)).Concat(StoryScript.Lose(9, 2900, 100)))
                Assert.IsFalse(Translator.HasVietnamese(t.Translate(s.text)), s.text + " -> " + t.Translate(s.text));
        }

        // ---------------- AC-1, AC-6 cai dat ----------------

        [Test]
        public void AC1_CaiDat_LuuNgonNgu()
        {
            GameSettings.Prefix = "Test.Lang.";
            GameSettings.ClearSaved();
            var s = new GameSettings { language = Lang.English };
            s.Save();
            Assert.AreEqual(Lang.English, GameSettings.Load().language);
            GameSettings.ClearSaved();
            GameSettings.Prefix = "";
        }

        [Test]
        public void AC6_MacDinh_TheoNgonNguMay_TestLaTiengViet()
        {
            Assert.AreEqual(Lang.Vietnamese, GameSettings.DefaultLanguage(true, false));
            Assert.AreEqual(Lang.English, GameSettings.DefaultLanguage(false, false));
            Assert.AreEqual(Lang.Vietnamese, GameSettings.DefaultLanguage(false, true), "batchmode / test: tieng Viet");
            GameSettings.Prefix = "Test.Lang2.";
            GameSettings.ClearSaved();
            Assert.AreEqual(Lang.Vietnamese, GameSettings.Load().language, "Editor/test chua luu: tieng Viet (khong de -1)");
            GameSettings.Prefix = "";
        }

        // ---------------- F-112 giong khach, F-114 phien ban ----------------

        [Test]
        public void F112_AC1_GiongKhach_GiaiNenLucNap()
        {
            string[] files = Directory.GetFiles(Path.Combine(Application.dataPath, "QuanAnHope", "Resources", "Audio"), "khach_*.mp3");
            Assert.GreaterOrEqual(files.Length, 3);
            foreach (string f in files)
            {
                string asset = "Assets" + f.Substring(Application.dataPath.Length).Replace('\\', '/');
                var imp = (AudioImporter)AssetImporter.GetAtPath(asset);
                Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, imp.defaultSampleSettings.loadType, asset);
                Assert.IsTrue(imp.defaultSampleSettings.preloadAudioData, asset + ": nap san khi mo game");
                Assert.IsTrue(imp.loadInBackground, asset + ": giai nen o luong nen, khong chan khung hinh");
            }
        }

        /// <summary>F-113: 16 nguyen lieu vong 2-4 + dot pha bang Blender: du ca 22 nguyen lieu (nguyen + so che), co Face, dung quy cach.</summary>
        [Test]
        public void F113_MoiNguyenLieu_CoModelBlender_NguyenVaSoChe()
        {
            List<ModelChecker.Result> all = ModelChecker.CheckAll();
            RecipeBook r = TestData.Recipes;
            var ids = r.sides.Select(x => x.id).Concat(r.mains.Select(x => x.id)).ToList();
            Assert.AreEqual(22, ids.Count);
            foreach (string id in ids)
                foreach (string n in new[] { "SM_NL_" + id, "SM_NL_" + id + "_SoChe" })
                {
                    ModelChecker.Result m = all.FirstOrDefault(x => x.group == "Ingredients" && x.name == n);
                    Assert.IsNotNull(m, n + " chua co (chay build_models.py)");
                    Assert.IsTrue(m.hasFace, n + ": thieu empty Face");
                    Assert.IsTrue(m.Ok, n + ": " + string.Join("; ", m.errors));
                }
        }

        [Test]
        public void F114_AC1_PhienBan_0_11_0()
        {
            Assert.AreEqual("0.11.0", Branding.Version);
        }
    }
}
