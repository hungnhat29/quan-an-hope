using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Gia lap nguoi choi sprint 6 (docs/sprint-6/impl-plan.md, constitution 1.5): menu -> cot truyen -> ngay 1 theo huong dan
    /// -> thoat -> choi tiep; cai dat; ket thang / thua; khach an; phong rong hon; khong con anh chu. Anh: docs/sprint-6/ui.
    /// Ban luu va cai dat cua test nam o thu muc / khoa rieng (khong dong vao ban luu that).
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint6 = "sprint-6";
        static readonly string TestSaveDir = Path.Combine(Path.GetTempPath(), "QuanAnHope_TestSave");

        /// <summary>Ban luu va cai dat rieng cho test; fresh = xoa sach (nhu may chua tung choi).</summary>
        void UseTestStorage(bool fresh = true)
        {
            SaveStore.Directory = TestSaveDir;
            GameSettings.Prefix = "Test.";
            if (fresh)
            {
                SaveStore.Delete();
                GameSettings.ClearSaved();
            }
            g.ReloadSettings();
        }

        [TearDown]
        public void RestoreStorage()
        {
            if (GameSettings.Prefix == "Test.") GameSettings.ClearSaved();
            SaveStore.Directory = TestSaveDir;
            SaveStore.Delete();
            GameSettings.Prefix = "";
        }

        /// <summary>Chup man hinh co ca menu / cot truyen (canvas Menu) va HUD.</summary>
        IEnumerator ShotAll(string fileName)
        {
            Canvas menu = g.Menu.Canvas.GetComponent<Canvas>();
            menu.renderMode = RenderMode.ScreenSpaceCamera;
            menu.worldCamera = P.Cam;
            menu.planeDistance = 0.1f;
            yield return Shot(fileName, null, Sprint6);
            menu.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        IEnumerator Frames(int n)
        {
            for (int i = 0; i < n; i++) yield return null;
        }

        // =====================================================================
        // F-050 + F-051 + F-052 + F-053: luong day du nhu nguoi moi choi
        // =====================================================================

        [UnityTest]
        public IEnumerator F050_F053_MenuTruyenHuongDan_ThoatRoiChoiTiep()
        {
            UseTestStorage();
            g.ShowMainMenu();
            yield return null;

            // AC-1: menu chinh, con tro hien, Choi tiep mo khi chua co ban luu
            Assert.AreEqual(GameMode.Menu, g.Mode);
            Assert.IsTrue(g.Menu.MainShown);
            Assert.IsFalse(g.Menu.ContinueButton.interactable, "chua co ban luu: Choi tiep mo");
            Assert.IsTrue(P.CameraOverride, "menu: camera nhin tu cua");
            Assert.IsFalse(g.Hud.Canvas.gameObject.activeSelf, "menu: HUD an");
            float clock = S.Clock.Elapsed;
            Time.timeScale = 1f;
            yield return ShotAll("menu_chinh.png");
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(clock, S.Clock.Elapsed, "menu: dong ho ngay khong chay");
            Time.timeScale = FastForward;

            // Choi moi -> cot truyen mo dau: Chuong 1 (5 khung, truyen tranh ve san CotTruyen)
            g.Menu.NewGameButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GameMode.Story, g.Mode);
            Assert.IsTrue(g.Menu.StoryShown);
            Assert.That(g.Menu.PanelCount, Is.InRange(4, 6));
            Assert.IsTrue(SaveStore.Exists, "choi moi: luu ngay 1 ngay");
            int panels = g.Menu.PanelCount;
            for (int i = 0; i < panels; i++)
            {
                Assert.AreEqual(i, g.Menu.PanelIndex);
                // truyen tranh ve san (20/11): mot trang 5 khung, khung hien lan luot
                Assert.IsTrue(g.Menu.UsingComicBook);
                Assert.AreEqual(1, g.Menu.ComicPage, "trang 1");
                Assert.AreEqual(i + 1, g.Menu.ComicPanelsOnPage, "so khung da hien");
                Assert.Greater(g.Menu.Book.CurrentLayerCount, 0, "khung co lop giay");
                g.Menu.Advance();                       // chu dang chay / khung dang roi: hien het
                Assert.IsTrue(g.Menu.CaptionComplete);
                Assert.IsTrue(g.Menu.ComicSettled);
                Time.timeScale = 1f;
                yield return new WaitForSeconds(0.4f);  // het mo dan
                yield return ShotAll("truyen_mo_dau_" + (i + 1) + ".png");
                Time.timeScale = FastForward;
                g.Menu.Advance();                       // sang khung sau
                yield return null;
            }
            Assert.AreEqual(GameMode.Playing, g.Mode, "het truyen thi vao ngay 1");
            Assert.AreEqual(1, S.Day);
            Assert.IsFalse(P.CameraOverride);
            Assert.IsTrue(g.Tutorial.Active, "van dau: co huong dan");
            yield return null;

            // F-053: theo huong dan tung buoc
            yield return FollowTutorial();

            // F-050 AC-2: Esc -> tam dung, dong ho dung
            g.Pause();
            yield return null;
            Assert.AreEqual(GameMode.Paused, g.Mode);
            Assert.IsTrue(g.Menu.PauseShown);
            clock = S.Clock.Elapsed;
            Time.timeScale = 1f;
            yield return ShotAll("tam_dung.png");
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(clock, S.Clock.Elapsed, "tam dung: dong ho ngay dung");
            Time.timeScale = FastForward;
            g.Menu.ResumeButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GameMode.Playing, g.Mode);

            // Ve menu giua ngay -> Choi tiep tu dau ngay 1 (F-052 AC-2)
            g.Pause();
            g.Menu.MainMenuButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GameMode.Menu, g.Mode);
            Assert.IsTrue(g.Menu.ContinueButton.interactable);
            StringAssert.Contains("ngày 1", g.Menu.ContinueButton.GetComponentInChildren<TextMeshProUGUI>().text);
            g.Menu.ContinueButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GameMode.Playing, g.Mode);
            Assert.AreEqual(1, S.Day);
            Assert.AreEqual(S.Balance.start.money, S.Money, "ve dung dau ngay 1");
            Assert.AreEqual(GamePhase.Prep, S.Phase);
            Assert.IsTrue(g.Tutorial.Active, "ngay 1 van dau: huong dan lai tu dau");

            // Het ngay -> sang ngay 2 tu luu; mo lai game (nap lai canh) -> Choi tiep vao dau ngay 2
            yield return Use("ShopSign");
            yield return WaitUntil(() => S.CurrentCustomer != null, 2f, "khach dau");
            yield return Use("ShopSign");                // dong quan som
            Assert.AreEqual(GamePhase.DayEnd, S.Phase);
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(2, S.Day);
            Assert.IsFalse(g.Tutorial.Active, "huong dan chi ngay 1");
            int money2 = S.Money, tickets2 = S.Wallet.Count;
            string offers2 = string.Join(",", S.Shop.Offers.Select(r => r == null ? "-" : r.id).ToArray());

            SceneManager.LoadScene(SceneName);
            yield return null;
            yield return null;
            g = Object.FindAnyObjectByType<GameController>();
            UseTestStorage(false);
            Assert.AreEqual(GameMode.Menu, g.Mode, "mo game: vao menu");
            g.Menu.ShowMain();
            Assert.IsTrue(g.Menu.ContinueButton.interactable, "co ban luu ngay 2");
            g.Menu.ContinueButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(2, S.Day);
            Assert.AreEqual(money2, S.Money);
            Assert.AreEqual(tickets2, S.Wallet.Count);
            Assert.AreEqual(offers2, string.Join(",", S.Shop.Offers.Select(r => r == null ? "-" : r.id).ToArray()), "cua hang relic bay y nhu truoc");
            Time.timeScale = 1f;
            yield return Shot("choi_tiep_ngay_2.png", null, Sprint6);
        }

        /// <summary>Lam theo tung buoc huong dan nhu nguoi moi: doc dong chu, nhin mui ten, bam dung do vat.</summary>
        IEnumerator FollowTutorial()
        {
            Tutorial t = g.Tutorial;
            yield return Frames(2);
            AssertStep(0, "Kiosk");
            Time.timeScale = 1f;
            yield return WalkTo(StandFor(g.Room.Station("Kiosk").WorldCenter));
            yield return Shot("huong_dan_1_may_nhet_ve.png", g.Room.Station("Kiosk").WorldCenter + new Vector3(0f, 0.25f, 0f), Sprint6);
            Time.timeScale = FastForward;
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Frames(2);
            Assert.AreEqual(1, t.Index, "quay xong: sang buoc lay nguyen lieu");

            Recipe r = S.MakeableRecipes()[0];
            Ingredient main = S.Stock.Items.First(i => i.kind == IngredientKind.Main && i.typeId == r.mainId);
            Ingredient side = S.Stock.Items.First(i => i.kind == IngredientKind.Side && i.typeId == r.sideId);
            Assert.IsNotNull(t.Arrow);
            Assert.IsTrue(t.Arrow.activeSelf, "mui ten tren thung");
            yield return TakeFromCrate(main);
            yield return Frames(2);
            Assert.AreEqual(2, t.Index, "cam nguyen lieu: sang buoc thot");
            AssertArrowOver("Board");

            yield return Use("Board");
            Ingredient onBoard = g.OnBoard;
            yield return HoldOn("Board", () => onBoard.prepared, onBoard.prepSeconds + 5f);
            yield return Frames(2);
            Assert.AreEqual(3, t.Index, "so che xong: sang buoc dung cu");
            AssertArrowOver("Tool_" + r.tool);
            yield return Use("Board");                  // cam len (da so che)
            g.PutDown();                                // dat lai vao thung de lam theo thu tu sach
            yield return Use("Tool_" + r.tool);
            yield return Frames(2);
            Assert.AreEqual(4, t.Index, "chon dung cu: sang buoc sach");

            yield return Approach(g.Room.Station("BookNext"));
            yield return Frames(2);
            Assert.AreEqual(5, t.Index, "nhin sach: sang buoc cho vao bep");
            foreach (string step in r.steps)
            {
                if (step == main.name) yield return PrepareAndAdd(S.Stock.Items.First(i => i.typeId == main.typeId));
                else if (step == side.name) yield return PrepareAndAdd(side);
                else yield return Use("Jar_" + step);
            }
            yield return Frames(2);
            Assert.AreEqual(6, t.Index, "da cho do vao bep: sang buoc vat num");
            AssertArrowOver("Knob");
            Time.timeScale = 1f;
            yield return Shot("huong_dan_7_vat_num.png", g.Room.Station("Knob").WorldCenter + new Vector3(0f, 0.3f, 0f), Sprint6);
            Time.timeScale = FastForward;
            yield return Use("Knob");
            yield return Frames(2);
            Assert.AreEqual(7, t.Index);
            yield return WaitUntil(() => S.Stove.State == StoveState.Ready, r.cookSeconds + 5f, "nau");
            yield return Use("PlateStack");
            yield return Use("Stove");
            yield return Frames(2);
            Assert.AreEqual(8, t.Index, "mon len quay: sang buoc mo quan");
            AssertArrowOver("ShopSign");
            yield return Use("ShopSign");
            yield return Frames(2);
            Assert.AreEqual(9, t.Index, "mo quan: sang buoc phuc vu");
            yield return ServeOne();
            Time.timeScale = 1f;                        // khach an 5 giay: ve toc do that ngay (L-022)
            yield return Frames(2);
            Assert.IsTrue(t.Finished, "phuc vu xong: het huong dan");
            StringAssert.Contains("Hướng dẫn xong", g.Hud.TutorialText ?? "");

            // khach vua phuc vu ngoi an (sua truoc sprint 6)
            Assert.IsNotNull(S.Eaters[0], "khach dang an");
            yield return Shot("huong_dan_xong_khach_an.png", g.Room.SeatViews[0].ServedSpot.position + new Vector3(0f, 0.5f, 0.3f), Sprint6);
            Time.timeScale = FastForward;
        }

        void AssertStep(int index, string stationId)
        {
            Assert.AreEqual(index, g.Tutorial.Index, "buoc huong dan");
            string line = g.Hud.TutorialText;
            Assert.IsNotNull(line, "HUD co dong huong dan");
            StringAssert.Contains(g.Tutorial.Current.text, line);
            AssertArrowOver(stationId);
        }

        void AssertArrowOver(string stationId)
        {
            Interactable target = g.Room.Station(stationId);
            Assert.IsNotNull(target, stationId);
            GameObject arrow = g.Tutorial.Arrow;
            Assert.IsNotNull(arrow, "co mui ten");
            Assert.IsTrue(arrow.activeSelf, "mui ten hien");
            Vector3 a = arrow.transform.position, c = target.WorldCenter;
            Assert.Less(new Vector2(a.x - c.x, a.z - c.z).magnitude, 0.05f, "mui ten ngay tren " + stationId);
            Assert.Greater(a.y, target.Box.bounds.max.y, "mui ten noi phia tren " + stationId);
        }

        // ---------------- F-053 AC-3: tat trong cai dat; van thu hai mac dinh tat ----------------

        [UnityTest]
        public IEnumerator F053_AC3_VanThuHai_MacDinhTatHuongDan()
        {
            UseTestStorage();
            g.ShowMainMenu();
            g.Menu.NewGameButton.onClick.Invoke();
            g.Menu.SkipStory();
            yield return null;
            Assert.IsTrue(g.Tutorial.Active, "van dau co huong dan");
            g.ShowMainMenu();
            g.Menu.NewGameButton.onClick.Invoke();
            g.Menu.SkipStory();
            yield return null;
            Assert.IsFalse(g.Tutorial.Active, "van thu hai mac dinh tat");

            // Bat lai trong Cai dat -> van sau co
            g.ShowMainMenu();
            g.Menu.SettingsButton.onClick.Invoke();
            g.Menu.TutorialButton.onClick.Invoke();     // Tu dong -> Tat
            g.Menu.TutorialButton.onClick.Invoke();     // Tat -> Bat
            Assert.IsTrue(g.Settings.tutorial && g.Settings.tutorialChosen);
            g.Menu.SettingsDoneButton.onClick.Invoke();
            g.Menu.NewGameButton.onClick.Invoke();
            g.Menu.SkipStory();
            yield return null;
            Assert.IsTrue(g.Tutorial.Active, "nguoi choi bat: co huong dan");
        }

        // ---------------- F-054 cai dat ----------------

        [UnityTest]
        public IEnumerator F054_CaiDat_DoiVaLuuQuaLanMo()
        {
            UseTestStorage();
            g.ShowMainMenu();
            g.Menu.SettingsButton.onClick.Invoke();
            yield return null;
            Assert.IsTrue(g.Menu.SettingsShown);
            Assert.AreEqual(GameSettings.DefaultSensitivity, g.Settings.sensitivity, 1e-4, "mac dinh 0,1 do / diem anh (Art Bible muc 9)");
            Assert.IsTrue(g.Settings.fullscreen, "mac dinh toan man hinh");

            g.Menu.MasterSlider.value = 35f;
            g.Menu.SensitivitySlider.value = 0.2f;
            g.Menu.InvertButton.onClick.Invoke();
            g.Menu.FullscreenButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(35, g.Settings.masterVolume);
            Assert.AreEqual(0.35f, AudioListener.volume, 1e-4);
            Assert.AreEqual(0.2f, P.LookDegreesPerPixel, 1e-4, "do nhay ap ngay vao nguoi choi");
            Assert.IsTrue(P.InvertY);
            Assert.IsFalse(g.Settings.fullscreen);
            Time.timeScale = 1f;
            yield return ShotAll("cai_dat.png");

            GameSettings reloaded = GameSettings.Load();
            Assert.AreEqual(35, reloaded.masterVolume, "luu qua lan mo");
            Assert.AreEqual(0.2f, reloaded.sensitivity, 1e-4);
            Assert.IsTrue(reloaded.invertY);
            Assert.IsFalse(reloaded.fullscreen);
            g.Menu.SettingsDoneButton.onClick.Invoke();
            Assert.IsTrue(g.Menu.MainShown, "Xong: ve menu chinh");
            AudioListener.volume = 1f;
        }

        // ---------------- F-051: ket thang / thua; F-052 AC-3 xoa ban luu ----------------

        IEnumerator StartAndJump(int day, int money)
        {
            UseTestStorage();
            g.ShowMainMenu();
            g.Menu.NewGameButton.onClick.Invoke();
            g.Menu.SkipStory();
            yield return null;
            Assert.IsTrue(g.DebugJumpToDay(day, 0));
            S.DebugAddMoney(money - S.Money);
            yield return Use("ShopSign");
            yield return WaitUntil(() => S.CurrentCustomer != null, 2f, "khach dau");
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.DayEnd, S.Phase);
            Assert.IsTrue(SaveStore.Exists);
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
        }

        [UnityTest]
        public IEnumerator F051_KetThua_GhiRoThieu_XoaBanLuu_NutChoiLai()
        {
            yield return StartAndJump(3, 500);
            Assert.AreEqual(GamePhase.Lost, S.Phase);
            Assert.AreEqual(GameMode.Story, g.Mode, "thua: tranh ket");
            StringAssert.Contains("thiếu " + StoryScript.Money(S.Balance.DebtDueOn(4) - 500), g.Menu.CaptionText);
            Assert.IsFalse(SaveStore.Exists, "thua: xoa ban luu");
            g.Menu.Advance();
            Time.timeScale = 1f;
            yield return new WaitForSeconds(0.4f);
            yield return ShotAll("ket_thua_tranh.png");
            g.Menu.Advance();
            yield return null;
            Assert.AreEqual(GameMode.Playing, g.Mode);
            StringAssert.Contains("Thua", g.Hud.OverlayTitle);
            StringAssert.Contains("thiếu", g.Hud.OverlayBody);
            Assert.IsTrue(g.Hud.OverlayMenuButton.gameObject.activeSelf, "co nut ve menu");
            yield return ShotAll("ket_thua_man.png");
            g.Hud.OverlayButton.onClick.Invoke();      // Choi lai
            yield return null;
            Assert.AreEqual(1, S.Day);
            Assert.AreEqual(GamePhase.Prep, S.Phase);
            Assert.IsTrue(SaveStore.Exists, "choi lai: luu ngay 1");
        }

        /// <summary>
        /// Yeu cau 20/11: cot truyen theo CotTruyen_BepNhanPham.zip. Ngay 2 chieu Chuong 2 roi ve phong;
        /// ca 6 trang dung du: lop giay, chu TextMeshPro (khong nam trong anh), so ngay theo du lieu.
        /// </summary>
        [UnityTest]
        public IEnumerator CotTruyen_ChuongGiuaVan_VaDuSauTrang()
        {
            g.PlayChapters = true;
            yield return EndDayEarly();                    // het ngay 1 -> sang ngay 2: Chuong 2 (bao)
            Assert.AreEqual(2, S.Day);
            Assert.AreEqual(GameMode.Story, g.Mode, "dau ngay 2: trang chuong");
            Assert.IsTrue(g.Menu.UsingComicBook);
            Assert.AreEqual(2, g.Menu.ComicPage);
            g.Menu.SkipStory();
            yield return null;
            Assert.AreEqual(GameMode.Playing, g.Mode, "het chuong: ve phong choi");

            string[][] expect =
            {
                new[] { "CẬU BỊ", "GIẤY NỢ", "Ngày " + S.Balance.debts[3].day },
                new[] { "ĐÙNG!!", "QUÁN CỦA HOPE" },
                new[] { "PHỤT!", "Ngày " + S.Balance.blackout.day + " · 19 giờ 02" },
                new[] { "LỘ RỒI!!", "Ngày " + (S.Balance.debts[1].day + 1) + "–" + (S.Balance.debts[2].day - 2) },
                new[] { "XÈO!", "Ngày " + S.Balance.day.bossDay + " · Hạn chót" },
                new[] { "XOẸT!!", "HẾT" },
            };
            int[] counts = { 5, 5, 5, 5, 5, 3 };
            for (int page = 1; page <= 6; page++)
            {
                var list = new List<StoryPanel>();
                for (int k = 0; k < counts[page - 1]; k++) list.Add(StoryPanel.Comic(page, k, "Trang " + page + " · khung " + (k + 1)));
                g.Menu.PlayStory(list, null);
                for (int k = 0; k < counts[page - 1]; k++)
                {
                    Assert.AreEqual(k + 1, g.Menu.ComicPanelsOnPage);
                    Assert.Greater(g.Menu.Book.CurrentLayerCount, 0);
                    g.Menu.Advance();                      // xong hoat anh
                    if (k < counts[page - 1] - 1) g.Menu.Advance();
                }
                Assert.AreEqual(page, g.Menu.ComicPage);
                List<string> texts = g.Menu.Book.PageTexts();
                foreach (string e in expect[page - 1]) Assert.That(texts, Has.Some.Contains(e), "trang " + page + ": chu TMP " + e);
                foreach (RawImage img in g.Menu.Canvas.GetComponentsInChildren<RawImage>())
                    if (img.name == "Anh") Assert.IsNotNull(img.texture, "anh lop " + img.transform.parent.name);
                Time.timeScale = 1f;
                yield return new WaitForSeconds(0.6f);
                yield return ShotAll("cot_truyen_trang_" + page + ".png");
                Time.timeScale = FastForward;
                g.Menu.SkipStory();
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator F051_KetThang_GaConGai_XoaBanLuu()
        {
            yield return StartAndJump(21, 40000);
            Assert.IsTrue(S.IsWon);
            Assert.AreEqual(GameMode.Story, g.Mode);
            Assert.IsFalse(SaveStore.Exists, "thang: xoa ban luu");
            int n = g.Menu.PanelCount;
            string all = "";
            for (int i = 0; i < n; i++)
            {
                g.Menu.Advance();
                all += g.Menu.CaptionText + " ";
                Time.timeScale = 1f;
                yield return new WaitForSeconds(0.4f);
                yield return ShotAll("ket_thang_" + (i + 1) + ".png");
                g.Menu.Advance();
                yield return null;
            }
            StringAssert.Contains("gả con gái", all);
            Assert.AreEqual(GameMode.Playing, g.Mode);
            StringAssert.Contains("trả hết nợ", g.Hud.OverlayTitle);
            g.Hud.OverlayMenuButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GameMode.Menu, g.Mode);
            Assert.IsFalse(g.Menu.ContinueButton.interactable);
        }

        [UnityTest]
        public IEnumerator F051_AC3_GiuDeBoQua()
        {
            UseTestStorage();
            g.ShowMainMenu();
            g.Menu.NewGameButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GameMode.Story, g.Mode);
            g.Menu.SkipStory();
            yield return null;
            Assert.AreEqual(GameMode.Playing, g.Mode, "giu de bo qua: vao ngay 1 luon");
        }

        // ---------------- F-055: khong con anh chu ----------------

        [UnityTest]
        public IEnumerator F055_KhongConAnhChu_BienTenQuanLaTextMeshPro()
        {
            yield return null;
            var bad = new System.Collections.Generic.List<string>();
            foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    foreach (string prop in new[] { "_MainTex", "_BaseMap" })
                    {
                        if (!m.HasProperty(prop)) continue;
                        Texture t = m.GetTexture(prop);
                        if (t != null && RoomView.TextImages.Contains(t.name)) bad.Add(r.name + ":" + t.name);
                    }
                }
            foreach (RawImage ri in Object.FindObjectsByType<RawImage>(FindObjectsInactive.Include))
                if (ri.texture != null && RoomView.TextImages.Contains(ri.texture.name)) bad.Add(ri.name + ":" + ri.texture.name);
            foreach (Image im in Object.FindObjectsByType<Image>(FindObjectsInactive.Include))
                if (im.sprite != null && RoomView.TextImages.Contains(im.sprite.texture.name)) bad.Add(im.name + ":" + im.sprite.texture.name);
            CollectionAssert.IsEmpty(bad, "con anh chu trong canh");
            Assert.AreEqual(RoomView.ShopName, g.Room.ShopNameText);

            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 1.6f));
            yield return Shot("bien_ten_quan.png", new Vector3(0f, 3.9f, 4.9f + BepNhanPham.RoomSize.Grow), Sprint6);
        }

        // ---------------- Sua truoc sprint 6: phong rong hon, khach an ----------------

        [UnityTest]
        public IEnumerator Phong_RongHon_TuongLuiRa()
        {
            yield return null;
            Transform room = GameObject.Find("NhaHang").transform;
            Renderer floor = room.GetComponentsInChildren<Renderer>(true).First(r => r.name == "Floor");
            Assert.AreEqual(BepNhanPham.RoomSize.Width, floor.bounds.size.x, 0.01f);
            Assert.Greater(BepNhanPham.RoomSize.Width, 8f, "rong hon 8 x 8 m cu");
            for (int c = 1; c <= 4; c++)
            {
                // mat trong cua tuong (Goc 1 la mat co lo cua, ba goc kia la khoi day 0,1 m) cach tam phong dung Half
                Transform goc = room.Find("Goc" + c);
                Renderer wall = goc.Find("Wall/WallBack").GetComponent<Renderer>();
                Vector3 lc = goc.InverseTransformPoint(wall.bounds.center);
                Vector3 le = goc.InverseTransformVector(wall.bounds.extents);
                Vector3 inner = goc.TransformPoint(new Vector3(0f, 2.5f, lc.z - Mathf.Abs(le.z)));
                Assert.AreEqual(BepNhanPham.RoomSize.Half, new Vector2(inner.x, inner.z).magnitude, 0.02f, "tuong goc " + c);
            }
            // di duoc toi vung moi rong ra ma khong ket
            yield return WalkTo(new Vector3(MinX, 0f, MinZ));
            yield return WalkTo(new Vector3(MaxX, 0f, MaxZ));
            Time.timeScale = 1f;
            P.Teleport(new Vector3(0f, 0f, -2.6f), 0f);
            yield return Shot("phong_rong.png", new Vector3(0f, 1.5f, 3f), Sprint6);
        }

        /// <summary>Yeu cau 18/11: them do sau ngoai cua. O cua khoet that, sau cua la pho nhieu lop xa dan.</summary>
        [UnityTest]
        public IEnumerator CuaRaPho_KhoetThat_PhoNhieuLopXaDan()
        {
            yield return null;
            Transform goc1 = GameObject.Find("NhaHang").transform.Find("Goc1");
            Assert.IsNull(goc1.Find("Entrance/DoorStreet"), "bo anh phang door_street");

            // Tuong Goc 1 khong co tam giac nao che tam o cua (0; 1,5 m) => nhin xuyen qua duoc
            Mesh wall = goc1.Find("Wall/WallBack").GetComponent<MeshFilter>().sharedMesh;
            Vector3[] v = wall.vertices;
            int[] t = wall.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = v[t[i]], b = v[t[i + 1]], c = v[t[i + 2]];
                float minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x)), maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
                float minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y)), maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));
                Assert.IsFalse(minX < 0f && maxX > 0f && minY < 1.5f && maxY > 1.5f, "tuong che o cua");
            }

            // Cac lop: gan < giua < xa < troi (theo khoang cach tu tuong)
            float prev = 4.1f;
            foreach (string layer in new[] { "Gan", "Giua", "Xa", "Troi" })
            {
                Transform l = goc1.Find("Street/" + layer);
                Assert.IsNotNull(l, "lop " + layer);
                Renderer[] rs = l.GetComponentsInChildren<Renderer>();
                Assert.Greater(rs.Length, 0);
                float z = rs.Max(r => goc1.InverseTransformPoint(r.bounds.center).z);
                Assert.Greater(z, prev, "lop " + layer + " xa hon lop truoc");
                prev = z;
            }
            Assert.IsNotNull(goc1.Find("Street/Giua").GetComponentInChildren<Sway>(), "long den qua duong dung dua");
            Assert.IsNotNull(goc1.Find("Street/Troi").GetComponentInChildren<Drift>(), "may troi");

            Time.timeScale = 1f;
            P.Teleport(new Vector3(0f, 0f, 0.4f), 0f);
            yield return Shot("cua_ra_pho.png", new Vector3(0f, 1.9f, 10f), Sprint6);
            yield return WalkTo(new Vector3(-1.6f, 0f, 1.6f + BepNhanPham.RoomSize.Grow * 0.5f));
            yield return Shot("cua_ra_pho_cheo.png", new Vector3(1.2f, 2.2f, 12f), Sprint6);
        }

        /// <summary>Yeu cau 19/11: troi ngoai cua doi mau theo gio trong game; toi thi cua so, long den sang.</summary>
        [UnityTest]
        public IEnumerator TroiDoiMauTheoGio_ToiThiDenSang()
        {
            yield return null;
            SkyCycle sky = g.Room.Sky;
            Assert.IsNotNull(sky, "co SkyCycle tren pho");

            // 9:00 giu dung mau canh dung san (anh sang)
            Color top, bottom, amb, cloud;
            SkyCycle.Sample(9f, out top, out bottom, out amb, out cloud);
            Assert.That(Vector4.Distance(top, UiKit.Hex("#CFE4EA")), Is.LessThan(0.01f));

            // dong ho choi chay -> troi theo gio that
            float h0 = S.Clock.HourOfDay;
            S.Clock.Advance(S.Balance.day.secondsPerHour * 3f);
            yield return null;
            yield return null;
            Assert.AreEqual(h0 + 3f, sky.ShownHour, 0.05f, "troi theo dong ho");

            Time.timeScale = 1f;
            P.Teleport(new Vector3(0f, 0f, 0.4f), 0f);
            float[] hours = { 12f, 17.5f, 19f, 21f };
            string[] files = { "troi_12h.png", "troi_17h30.png", "troi_19h.png", "troi_21h.png" };
            float dayLum = 0f;
            for (int i = 0; i < hours.Length; i++)
            {
                float hh = hours[i];
                sky.Init(() => hh);
                yield return null;
                float lum = sky.SkyTop.grayscale;
                if (i == 0) dayLum = lum;
                if (hh <= 17.5f) Assert.AreEqual(0f, sky.Lights, 1e-3f, "ban ngay den tat " + hh);
                yield return Shot(files[i], new Vector3(0f, 1.9f, 10f), Sprint6);
            }
            Assert.Less(sky.SkyTop.grayscale, dayLum - 0.3f, "21:00 troi toi han");
            Assert.AreEqual(1f, sky.Lights, 1e-3f, "21:00 den sang han");
            Assert.Greater(sky.ColorOf("Nha_0_CuaSo_T").grayscale, sky.ColorOf("Nha_0").grayscale + 0.2f, "cua so sang hon tuong luc toi");
            Assert.Greater(sky.ColorOf("BongDenDuong").grayscale, 0.8f, "den duong sang");

            // vat lieu dung chung trong phong khong bi doi (chi dung MaterialPropertyBlock)
            Renderer win = GameObject.Find("Nha_0_CuaSo_T").GetComponent<Renderer>();
            Assert.That(Vector4.Distance(win.sharedMaterial.GetColor("_Color"), UiKit.Hex("#FFF3DC")), Is.LessThan(0.01f));
        }

        /// <summary>
        /// Yeu cau 20/11: khach va nguyen lieu chuyen dong muot nhu cot truyen. Khach tho / dung dua lien tuc, nhay vao ghe,
        /// nhay cang khi vui; nguyen lieu nhun, nghieng. Muot = dao dong ro nhung giua hai khung hinh khong co buoc nhay lon.
        /// </summary>
        [UnityTest]
        public IEnumerator KhachVaNguyenLieu_ChuyenDongMuot()
        {
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Use("ShopSign");
            yield return WaitForCustomer();
            Time.timeScale = 1f;
            int seat = System.Array.IndexOf(S.Seats.ToArray(), S.CurrentCustomer);
            RoomView.SeatView v = g.Room.SeatViews[Mathf.Max(0, seat)];
            Lively l = v.Model.GetComponent<Lively>();
            Assert.IsNotNull(l, "khach co Lively");
            yield return new WaitForSeconds(Lively.ArriveSeconds + 0.1f);

            float min = 9f, max = -9f, jump = 0f, prev = l.ScaleY;
            float rotMin = 999f, rotMax = -999f;
            for (float t = 0f; t < 2.6f; t += Time.deltaTime)
            {
                yield return null;
                float y = l.ScaleY;
                min = Mathf.Min(min, y); max = Mathf.Max(max, y);
                jump = Mathf.Max(jump, Mathf.Abs(y - prev) / Mathf.Max(Time.deltaTime, 1e-3f) / 60f);
                prev = y;
                float z = Mathf.DeltaAngle(0f, v.Model.transform.localEulerAngles.z);
                rotMin = Mathf.Min(rotMin, z); rotMax = Mathf.Max(rotMax, z);
            }
            Assert.Greater(max - min, 0.03f, "khach tho (co gian than)");
            Assert.Greater(rotMax - rotMin, 2f, "khach dung dua");
            Assert.Less(jump, 0.01f, "khong giat: moi 1/60 giay doi < 1%");

            // nguyen lieu trong thung: co IngredientIdle, nhun lien tuc va muot
            ItemView[] items = Object.FindObjectsByType<ItemView>(FindObjectsSortMode.None);
            Assert.Greater(items.Length, 0);
            Assert.IsTrue(items.All(i => i.Idle != null), "moi nguyen lieu co chuyen dong nghi");
            Transform h = items[0].Idle.transform;
            float hy0 = 9f, hy1 = -9f, hJump = 0f, hPrev = h.localScale.y;
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                yield return null;
                float y = h.localScale.y;
                hy0 = Mathf.Min(hy0, y); hy1 = Mathf.Max(hy1, y);
                hJump = Mathf.Max(hJump, Mathf.Abs(y - hPrev) / Mathf.Max(Time.deltaTime, 1e-3f) / 60f);
                hPrev = y;
            }
            Assert.Greater(hy1 - hy0, 0.03f * h.localScale.y, "nguyen lieu tho");
            Assert.Less(hJump, 0.02f, "nguyen lieu khong giat");

            // duong cong nhay o toc do that (khach dang ngoi cho): len cao roi ve, muot
            float baseY = v.Model.transform.localPosition.y, peak = baseY;
            l.Hop();
            for (float t = 0f; t < Lively.HopSeconds + 0.05f; t += Time.deltaTime)
            {
                yield return null;
                peak = Mathf.Max(peak, v.Model.transform.localPosition.y);
            }
            Assert.Greater(peak - baseY, 0.1f, "nhay cang len cao: base " + baseY + " peak " + peak + " active " + v.Model.activeInHierarchy + " scaleY " + l.ScaleY + " ts " + Time.timeScale);
            Assert.AreEqual(baseY, v.Model.transform.localPosition.y, 0.02f, "nhay xong ve cho cu");
            // phuc vu: vui thi nhay cang, gian thi lac dau (dien ngay luc doi mat)
            int hops = l.HopCount, shakes = l.ShakeCount;
            yield return ServeOne();
            Time.timeScale = 1f;
            if (S.LastPayment != null && S.LastPayment.outcome == PaymentOutcome.Satisfied) Assert.Greater(l.HopCount, hops, "vui: nhay cang");
            if (S.LastPayment != null && S.LastPayment.outcome == PaymentOutcome.Angry) Assert.Greater(l.ShakeCount, shakes, "gian: lac dau");
            yield return Shot("khach_nguyen_lieu_muot.png", v.ServedSpot.position + new Vector3(0f, 0.6f, 0.3f), Sprint6);
            Time.timeScale = FastForward;
        }

        [UnityTest]
        public IEnumerator KhachAn_DuaGapMonVoiDan_AnXongRoiGhe()
        {
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Use("ShopSign");
            yield return WaitForCustomer();
            Customer c = S.CurrentCustomer;
            yield return ServeOne();
            Time.timeScale = 1f;                        // hieu ung ngan: xem o toc do that (L-022)
            Assert.AreSame(c, S.Eaters[0], "khach vua phuc vu dang an");
            RoomView.SeatView v = g.Room.SeatViews[0];
            Assert.IsNotNull(v.Eating, "co hoat anh an");
            Assert.IsTrue(v.Model.activeInHierarchy, "khach con ngoi");
            Assert.IsNotNull(v.Eating.Sticks, "co doi dua");

            P.LookAt(v.ServedSpot.position + new Vector3(0f, 0.5f, 0.3f));
            float stickLow = v.Eating.StickLift, stickHigh = stickLow;
            float left0 = v.Eating.FoodLeft;
            yield return Shot("khach_an_1.png", null, Sprint6);
            float t0 = Time.time;
            while (v.Eating != null && (v.Eating.FoodLeft >= left0 || Time.time - t0 < EatingFx.BitePeriod) && Time.time - t0 < S.Balance.day.eatSeconds)
            {
                stickLow = Mathf.Min(stickLow, v.Eating.StickLift);
                stickHigh = Mathf.Max(stickHigh, v.Eating.StickLift);
                yield return null;
            }
            Assert.IsNotNull(v.Eating, "van dang an");
            Assert.Less(v.Eating.FoodLeft, left0, "mon voi dan");
            Assert.Greater(stickHigh - stickLow, 0.5f, "dua gap len xuong");
            StringAssert.Contains("đang ăn", g.Room.Station("Customer_0").CurrentPrompt);
            yield return Shot("khach_an_2.png", null, Sprint6);
            Time.timeScale = FastForward;
            yield return WaitUntil(() => S.Eaters[0] == null, S.Balance.day.eatSeconds + 1f, "an xong");
            yield return WaitUntil(() => v.LeaveLeft <= 0f, 2f, "roi ghe");
            yield return Frames(2);
            if (S.Seats[0] == null) Assert.IsFalse(v.Model.activeInHierarchy, "an xong roi ghe: khoi khach an");
            Assert.IsNull(v.ServedDish, "dia duoc don");
        }
    }
}
