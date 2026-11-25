using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Sprint 11 (specs/sprint-11/spec.md): F-110 tieng Anh (yeu cau 25/11/2026) — doi trong Cai dat, choi ngay 1 bang tieng Anh
    /// khong con chu Viet tren man hinh, anh docs/sprint-11/ui/en_*.png; F-111 ten mon dai trong Thuc don (K-8).
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint11 = "sprint-11";

        [TearDown]
        public void RestoreLanguage()
        {
            Lang.Set(Lang.Vietnamese);
        }

        void UseLanguage(int lang)
        {
            g.Settings.language = lang;
            g.Settings.Save();
            g.ReloadSettings();
            Assert.AreEqual(lang, Lang.Current);
        }

        /// <summary>Chu dang hien that su (sau bo dich), khong phai .text goc.</summary>
        static string Shown(TMP_Text t)
        {
            t.ForceMeshUpdate(true, true);
            return t.GetParsedText();
        }

        /// <summary>Tieu de "TieuDe" trong the Cai dat (menu tam dung cung co "TieuDe").</summary>
        TMP_Text SettingsTitle()
        {
            Transform card = g.Menu.Canvas.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "CaiDat");
            return Named(card, "TieuDe");
        }

        static TMP_Text Named(Transform root, string name)
        {
            return root.GetComponentsInChildren<RectTransform>(true).Where(r => r.name == name)
                .Select(r => r.GetComponentInChildren<TMP_Text>(true)).First(t => t != null);
        }

        readonly List<string> enLeft = new List<string>();

        void ScanEnglish(string where)
        {
            foreach (TMP_Text t in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                if (!t.isActiveAndEnabled || string.IsNullOrEmpty(t.text)) continue;
                string shown = Shown(t);
                if (Translator.HasVietnamese(shown))
                {
                    string line = where + " | " + t.name + " | " + t.text.Replace("\n", "\\n") + "  ->  " + shown.Replace("\n", "\\n");
                    if (!enLeft.Contains(line)) enLeft.Add(line);
                }
            }
        }

        bool scanning;

        IEnumerator ScanLoop(string where)
        {
            while (scanning)
            {
                ScanEnglish(where + " ngay " + S.Day + " " + S.Phase);
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        // ---------------- AC-1 doi ngon ngu trong Cai dat ----------------

        [UnityTest]
        public IEnumerator F110_AC1_CaiDat_DoiNgonNgu_ChuDoiNgay()
        {
            UseTestStorage();
            UseLanguage(Lang.Vietnamese);
            g.ShowMainMenu();
            g.Menu.ShowSettings();
            yield return Frames(2);
            Button b = g.Menu.LanguageButton;
            Assert.IsNotNull(b, "Cai dat co dong Ngon ngu / Language");
            Assert.AreEqual("Tiếng Việt", Shown(b.GetComponentInChildren<TMP_Text>()));
            Assert.AreEqual("Cài đặt", Shown(SettingsTitle()));
            b.onClick.Invoke();
            yield return Frames(2);
            Assert.AreEqual(Lang.English, g.Settings.language);
            Assert.AreEqual(Lang.English, GameSettings.Load().language, "luu qua lan mo");
            Assert.AreEqual("English", Shown(b.GetComponentInChildren<TMP_Text>()));
            Assert.AreEqual("Settings", Shown(SettingsTitle()), "chu doi ngay, khong can mo lai");
            Assert.AreEqual("Cài đặt", SettingsTitle().text, ".text goc giu tieng Viet");
            Time.timeScale = 1f;
            yield return ShotAllTo("en_cai_dat.png", Sprint11);
            b.onClick.Invoke();
            yield return Frames(2);
            Assert.AreEqual(Lang.Vietnamese, g.Settings.language);
            Assert.AreEqual("Cài đặt", Shown(SettingsTitle()));
            g.Menu.CloseSettings();
        }

        // ---------------- AC-4, AC-5 choi bang tieng Anh ----------------

        [UnityTest]
        public IEnumerator F110_AC4_ChoiNgay1_BangTiengAnh_KhongSotChuViet()
        {
            UseTestStorage();
            UseLanguage(Lang.English);
            enLeft.Clear();
            Time.timeScale = 1f;

            g.ShowMainMenu();
            yield return Frames(3);
            ScanEnglish("menu chinh");
            yield return ShotAllTo("en_menu.png", Sprint11);

            g.Menu.ShowSettings();
            yield return Frames(3);
            ScanEnglish("cai dat");
            g.Menu.CloseSettings();

            g.Menu.PlayStory(StoryScript.Intro(S.Balance), () => { });
            yield return new WaitForSecondsRealtime(2.5f);
            ScanEnglish("truyen chuong 1");
            yield return ShotAllTo("en_truyen.png", Sprint11);
            for (int i = 0; i < 6; i++) { g.Menu.Advance(); yield return new WaitForSecondsRealtime(0.4f); ScanEnglish("truyen chuong 1"); }
            g.Menu.SkipStory();
            yield return Frames(2);

            g.NewGame(Seed);
            g.Tutorial.Begin(true);
            yield return Frames(3);
            ScanEnglish("huong dan");
            yield return WalkTo(StandFor(g.Room.Station("Kiosk").WorldCenter));
            yield return Shot("en_huong_dan.png", g.Room.Station("Kiosk").WorldCenter, Sprint11);
            g.ToggleTutorialPics();
            yield return Frames(2);
            yield return Shot("en_phong_bep.png", g.Room.Station("Kiosk").WorldCenter + new Vector3(0.8f, -0.3f, 0f), Sprint11);

            Time.timeScale = FastForward;
            scanning = true;
            Coroutine loop = g.StartCoroutine(ScanLoop("ngay 1"));
            yield return PlayDay(S.Wallet.Count, null);
            scanning = false;
            g.StopCoroutine(loop);
            Time.timeScale = 1f;
            yield return Frames(3);
            ScanEnglish("cuoi ngay");
            yield return Shot("en_cuoi_ngay.png", null, Sprint11);

            foreach (TabMenu.Page p in new[] { TabMenu.Page.Menu, TabMenu.Page.Relic, TabMenu.Page.Shop, TabMenu.Page.Guide })
            {
                Assert.IsTrue(g.OpenTab(p));
                yield return Frames(3);
                ScanEnglish("Tab " + p);
                yield return Shot("en_tab_" + p.ToString().ToLowerInvariant() + ".png", null, Sprint11);
                g.CloseTab();
                yield return Frames(1);
            }

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", Sprint11, "ui"));
            Directory.CreateDirectory(dir);
            File.WriteAllLines(Path.Combine(dir, "en_con_sot.txt"), enLeft.Concat(Lang.Table.Leftovers.Select(x => "bang | " + x.Replace("\n", "\\n"))));
            Assert.IsEmpty(enLeft, "chu Viet con sot tren man hinh (docs/sprint-11/ui/en_con_sot.txt):\n" + string.Join("\n", enLeft.Take(25)));
        }

        // ---------------- F-111 ten mon dai trong Thuc don (K-8) ----------------

        [UnityTest]
        public IEnumerator F111_ThucDon_TenMonDai_KhongChamCotGia()
        {
            yield return StartAt(17, 0, new string[0]);
            foreach (int lang in new[] { Lang.Vietnamese, Lang.English })
            {
                UseLanguage(lang);
                Assert.IsTrue(g.OpenTab(TabMenu.Page.Menu));
                yield return Frames(3);
                Assert.Greater(g.Tab.MenuButtons.Count, 30);
                foreach (KeyValuePair<string, Button> kv in g.Tab.MenuButtons)
                {
                    TMP_Text name = kv.Value.GetComponentsInChildren<TMP_Text>(true).First(t => t.transform.parent == kv.Value.transform);
                    TMP_Text price = kv.Value.transform.Find("Gia").GetComponentInChildren<TMP_Text>(true);
                    Shown(name);
                    Vector3[] n = new Vector3[4], pr = new Vector3[4];
                    name.rectTransform.GetWorldCorners(n);
                    price.rectTransform.GetWorldCorners(pr);
                    Assert.LessOrEqual(n[2].x, pr[0].x + 0.01f, kv.Key + ": o ten khong chui duoi cot gia (" + name.text + ")");
                    Assert.IsFalse(name.isTextOverflowing, kv.Key + ": ten tran o (" + Shown(name) + ")");
                    Assert.LessOrEqual(name.textInfo.lineCount, 2, kv.Key + ": toi da 2 dong (" + Shown(name) + ")");
                }
                Time.timeScale = 1f;
                yield return Shot(lang == Lang.English ? "en_thuc_don_ten_dai.png" : "f111_thuc_don_ten_dai.png", null, Sprint11);
                g.CloseTab();
                yield return Frames(1);
            }
        }
    }
}
