using System;
using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Cac man ngoai phong (constitution Dieu 5: menu, ket thuc ngay, cot truyen) cua sprint 6:
    /// menu chinh (F-050 AC-1), menu tam dung (AC-2), cai dat (F-054), trinh chieu cot truyen (F-051).
    /// Nut theo quy cach Art Bible muc 13 (UiKit.Button). Chu deu la TextMeshPro (Dieu 6).
    /// </summary>
    public class MenuView
    {
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        static readonly Vector2 Left = new Vector2(0f, 0.5f);
        static readonly Vector2 Top = new Vector2(0.5f, 1f);
        static readonly Vector2 Bottom = new Vector2(0.5f, 0f);

        public const string GameTitle = "Quán ăn của Hope";

        /// <summary>Dong "Phien ban x.y.z" goc duoi phai menu chinh (F-093 AC-4).</summary>
        public TextMeshProUGUI VersionLabel { get; private set; }

        readonly GameController g;
        public Transform Canvas { get; private set; }

        GameObject main, pause, settings, story;
        public Button NewGameButton { get; private set; }
        public Button ContinueButton { get; private set; }
        public Button SettingsButton { get; private set; }
        public Button QuitButton { get; private set; }
        public Button ResumeButton { get; private set; }
        public Button PauseSettingsButton { get; private set; }
        public Button MainMenuButton { get; private set; }
        public Button SettingsDoneButton { get; private set; }

        public bool MainShown { get { return main.activeSelf; } }
        public bool PauseShown { get { return pause.activeSelf; } }
        public bool SettingsShown { get { return settings.activeSelf; } }
        public bool StoryShown { get { return story.activeSelf; } }

        public MenuView(GameController controller)
        {
            g = controller;
            var go = new GameObject("Menu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            Canvas = go.transform;

            BuildMain();
            BuildPause();
            BuildSettings();
            BuildStory();
            HideAll();
        }

        public void HideAll()
        {
            main.SetActive(false);
            pause.SetActive(false);
            settings.SetActive(false);
            story.SetActive(false);
        }

        // =====================================================================
        // Menu chinh: can phong nhin tu cua, bon nut doc ben trai (kich ban 10 giay)
        // =====================================================================

        void BuildMain()
        {
            RectTransform root = UiKit.Stretch(Canvas, "MenuChinh");
            main = root.gameObject;
            // dai toi ben trai de chu doc duoc tren nen phong
            Image band = UiKit.Panel(root, "Dai", Left, new Vector2(0f, 0f), new Vector2(470f, 900f), new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.68f), false);
            band.sprite = null;
            band.raycastTarget = false;

            TextMeshProUGUI title = UiKit.Label(root, "TenGame", new Vector2(0f, 1f), new Vector2(48f, -60f), new Vector2(400f, 130f), GameTitle, 54f, TextAlignmentOptions.Left);
            title.color = UiKit.Cream;
            title.lineSpacing = -30f;
            TextMeshProUGUI sub = UiKit.Label(root, "PhuDe", new Vector2(0f, 1f), new Vector2(50f, -190f), new Vector2(400f, 40f), "22 ngày để trả hết nợ", 20f, TextAlignmentOptions.Left);
            // F-093 AC-4: so phien ban o menu (nguoi choi thu gui kem khi bao loi)
            VersionLabel = UiKit.Label(root, "PhienBan", new Vector2(1f, 0f), new Vector2(-16f, 10f), new Vector2(300f, 28f), "Phiên bản " + Application.version, 16f, TextAlignmentOptions.Right);
            VersionLabel.color = UiKit.Cream;
            sub.color = UiKit.Mustard;

            float y = 80f;
            NewGameButton = UiKit.Button(root, "Btn_ChoiMoi", Left, new Vector2(48f, y), new Vector2(340f, 60f), "Chơi mới", UiKit.Coral, () => g.StartNewGame(), 26f);
            ContinueButton = UiKit.Button(root, "Btn_ChoiTiep", Left, new Vector2(48f, y - 80f), new Vector2(340f, 60f), "Chơi tiếp", UiKit.Mustard, () => g.ContinueGame(), 24f);
            SettingsButton = UiKit.Button(root, "Btn_CaiDat", Left, new Vector2(48f, y - 160f), new Vector2(340f, 60f), "Cài đặt", UiKit.Cream, () => ShowSettings(), 24f);
            QuitButton = UiKit.Button(root, "Btn_Thoat", Left, new Vector2(48f, y - 240f), new Vector2(340f, 60f), "Thoát", UiKit.Cream, () => g.Quit(), 24f);
        }

        /// <summary>Hien menu chinh; "Choi tiep" mo khi khong co ban luu (F-050 AC-1).</summary>
        public void ShowMain()
        {
            HideAll();
            main.SetActive(true);
            SaveData save = SaveStore.Load();
            ContinueButton.interactable = save != null;
            UiKit.SetLabel(ContinueButton, save != null ? "Chơi tiếp · ngày " + save.day : "Chơi tiếp");
        }

        // =====================================================================
        // Tam dung (Esc trong phong)
        // =====================================================================

        void BuildPause()
        {
            RectTransform dim = UiKit.Stretch(Canvas, "TamDung");
            var img = dim.gameObject.AddComponent<Image>();
            img.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.72f);
            pause = dim.gameObject;
            Image card = UiKit.Panel(dim, "The", Center, Vector2.zero, new Vector2(460f, 400f), UiKit.Cream);
            UiKit.Label(card.transform, "TieuDe", Top, new Vector2(0f, -24f), new Vector2(420f, 56f), "Tạm dừng", 36f);
            ResumeButton = UiKit.Button(card.transform, "Btn_TiepTuc", Top, new Vector2(0f, -100f), new Vector2(340f, 60f), "Tiếp tục", UiKit.Coral, () => g.Resume(), 24f);
            PauseSettingsButton = UiKit.Button(card.transform, "Btn_CaiDat", Top, new Vector2(0f, -180f), new Vector2(340f, 60f), "Cài đặt", UiKit.Mustard, () => ShowSettings(), 24f);
            MainMenuButton = UiKit.Button(card.transform, "Btn_VeMenu", Top, new Vector2(0f, -260f), new Vector2(340f, 60f), "Về menu chính", UiKit.Cream, () => g.ShowMainMenu(), 24f);
            TextMeshProUGUI note = UiKit.Label(card.transform, "GhiChu", Bottom, new Vector2(0f, 14f), new Vector2(420f, 40f),
                "Game tự lưu đầu mỗi ngày: về menu thì chơi tiếp từ đầu ngày này.", 15f);
            note.color = UiKit.Ink;
        }

        public void ShowPause()
        {
            HideAll();
            pause.SetActive(true);
        }

        // =====================================================================
        // Cai dat (F-054)
        // =====================================================================

        Slider master, music, sfx, sensitivity;
        Button invert, fullscreen, resolution, tutorial, comicMotion, paperMotion;
        TextMeshProUGUI sensitivityValue;
        bool fromPause;

        public Slider MasterSlider { get { return master; } }
        public Slider SensitivitySlider { get { return sensitivity; } }
        public Button InvertButton { get { return invert; } }
        public Button FullscreenButton { get { return fullscreen; } }
        public Button TutorialButton { get { return tutorial; } }
        public Button ComicMotionButton { get { return comicMotion; } }
        public Button PaperMotionButton { get { return paperMotion; } }

        /// <summary>F-110 AC-1: nut Ngon ngu / Language.</summary>
        public Button LanguageButton { get { return language; } }
        Button language;

        void BuildSettings()
        {
            RectTransform dim = UiKit.Stretch(Canvas, "CaiDat");
            var img = dim.gameObject.AddComponent<Image>();
            img.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.72f);
            settings = dim.gameObject;
            // sau sprint 10 (yeu cau 23/11/2026): huong dan di chuyen chuyen tu HUD vao Cai dat -> the rong them cot Dieu khien
            Image card = UiKit.Panel(dim, "The", Center, Vector2.zero, new Vector2(SettingsW, SettingsH), UiKit.Cream);
            Transform cardT = card.transform;
            RectTransform leftCol = UiKit.Box(cardT, "CotCaiDat", new Vector2(0f, 1f), Vector2.zero, new Vector2(760f, SettingsH));
            Transform c = leftCol;
            UiKit.Label(c, "TieuDe", Top, new Vector2(0f, -20f), new Vector2(700f, 50f), "Cài đặt", 34f);
            BuildControls(cardT);

            float y = -84f;
            master = SliderRow(c, "AmLuongTong", "Âm lượng tổng", y, 0f, 100f, true, v => { g.Settings.masterVolume = (int)v; Changed(); });
            music = SliderRow(c, "AmLuongNhac", "Nhạc", y -= 48f, 0f, 100f, true, v => { g.Settings.musicVolume = (int)v; Changed(); });
            sfx = SliderRow(c, "AmLuongHieuUng", "Hiệu ứng", y -= 48f, 0f, 100f, true, v => { g.Settings.sfxVolume = (int)v; Changed(); });
            sensitivity = SliderRow(c, "DoNhay", "Độ nhạy chuột", y -= 48f, GameSettings.MinSensitivity, GameSettings.MaxSensitivity, false,
                v => { g.Settings.sensitivity = Mathf.Round(v * 100f) / 100f; Changed(); });
            sensitivityValue = sensitivity.transform.parent.Find("GiaTri").GetComponentInChildren<TextMeshProUGUI>();

            invert = ToggleRow(c, "DaoTruc", "Đảo trục dọc", y -= 56f, () => { g.Settings.invertY = !g.Settings.invertY; Changed(); });
            fullscreen = ToggleRow(c, "ToanManHinh", "Toàn màn hình", y -= 54f, () => { g.Settings.fullscreen = !g.Settings.fullscreen; Changed(true); });
            resolution = ToggleRow(c, "DoPhanGiai", "Độ phân giải", y -= 54f, () => { g.Settings.NextResolution(); Changed(true); });
            tutorial = ToggleRow(c, "HuongDan", "Hướng dẫn ngày 1", y -= 54f, () =>
            {
                g.Settings.tutorial = !g.Settings.tutorial;
                g.Settings.tutorialChosen = true;
                Changed();
            });
            comicMotion = ToggleRow(c, "HieuUngTruyen", "Hiệu ứng truyện", y -= 54f, () =>
            {
                g.Settings.comicMotion = (g.Settings.comicMotion + 1) % GameSettings.ComicMotionNames.Length;
                Changed();
            });
            // paper-ui-pickit.md muc 4: hieu ung giay cua tranh huong dan
            paperMotion = ToggleRow(c, "HieuUngGiay", "Hiệu ứng giấy", y -= 54f, () =>
            {
                g.Settings.paperMotion = (g.Settings.paperMotion + 1) % PaperKit.MotionNames.Length;
                Changed();
            });
            // F-110 AC-1 (yeu cau 25/11/2026): nhan hai thu tieng de nguoi khong doc tieng Viet van tim thay
            language = ToggleRow(c, "NgonNgu", "Ngôn ngữ / Language", y -= 54f, () =>
            {
                g.Settings.language = (Lang.Current + 1) % Lang.Names.Length;
                Changed();
            });
            SettingsDoneButton = UiKit.Button(cardT, "Btn_Xong", Bottom, new Vector2(0f, 18f), new Vector2(260f, 54f), "Xong", UiKit.Coral, () => CloseSettings(), 24f);
        }

        const float SettingsW = 1180f;
        const float SettingsH = 760f;   // F-110: them dong Ngon ngu (truoc 700)

        /// <summary>Phim dieu khien (truoc o dong chu trang goc duoi HUD).</summary>
        public static readonly string[][] Controls =
        {
            new[] { "W A S D", "Đi" },
            new[] { "Shift", "Chạy" },
            new[] { "Space", "Nhảy" },
            new[] { "Chuột", "Nhìn quanh" },
            new[] { "Chuột trái", "Bấm · giữ để sơ chế" },
            new[] { "Chuột phải", "Thả / đặt lại đồ" },
            new[] { "Tab", "Thực đơn, relic, cửa hàng" },
            new[] { "T", "Ẩn / hiện hướng dẫn" },
            new[] { "Esc", "Tạm dừng" },
        };

        /// <summary>Cac dong trong cot Dieu khien (test).</summary>
        public readonly System.Collections.Generic.List<string> ControlLines = new System.Collections.Generic.List<string>();

        void BuildControls(Transform card)
        {
            RectTransform col = UiKit.Box(card, "CotDieuKhien", new Vector2(1f, 1f), new Vector2(-24f, -20f), new Vector2(390f, 580f));
            Image bg = col.gameObject.AddComponent<Image>();
            bg.sprite = UiKit.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = UiKit.Hex("#FBF0DA");
            bg.raycastTarget = false;
            UiKit.Label(col, "TieuDe", Top, new Vector2(0f, -10f), new Vector2(360f, 44f), "Điều khiển", 28f);
            ControlLines.Clear();
            for (int i = 0; i < Controls.Length; i++)
            {
                float y = -66f - i * 56f;
                Image key = UiKit.Panel(col, "Phim_" + i, new Vector2(0f, 1f), new Vector2(16f, y), new Vector2(140f, 44f), UiKit.Mustard, false);
                key.raycastTarget = false;
                UiKit.Text(key.transform, Controls[i][0], 18f, TextAlignmentOptions.Center, 4f);
                UiKit.Label(col, "Viec_" + i, new Vector2(0f, 1f), new Vector2(168f, y - 2f), new Vector2(210f, 48f), Controls[i][1], 18f, TextAlignmentOptions.Left);
                ControlLines.Add(Controls[i][0] + ": " + Controls[i][1]);
            }
        }

        Slider SliderRow(Transform parent, string name, string label, float y, float min, float max, bool whole, Action<float> onChange)
        {
            RectTransform row = UiKit.Box(parent, name, Top, new Vector2(0f, y), new Vector2(700f, 48f));
            UiKit.Label(row, "Nhan", new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(240f, 44f), label, 22f, TextAlignmentOptions.Left);
            Slider s = MakeSlider(row, new Vector2(260f, 0f), new Vector2(330f, 26f));
            s.minValue = min;
            s.maxValue = max;
            s.wholeNumbers = whole;
            Image vbox = UiKit.Panel(row, "GiaTri", new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(90f, 44f), UiKit.Mustard, false);
            vbox.raycastTarget = false;
            TextMeshProUGUI vt = UiKit.Text(vbox.transform, "", 20f);
            s.onValueChanged.AddListener(v =>
            {
                vt.text = whole ? ((int)v).ToString() : v.ToString("0.00") + "°";
                if (!syncing) onChange(v);
            });
            return s;
        }

        /// <summary>Thanh truot uGUI dung tay (nen kem, ruot bac ha, nut vang) theo Art Bible muc 13.</summary>
        /// <summary>Duong kinh nut keo (hinh tron).</summary>
        public const float HandleSize = 38f;

        static Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size)
        {
            RectTransform rt = UiKit.Box(parent, "ThanhTruot", new Vector2(0f, 0.5f), pos, size);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = pos;
            Image bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UiKit.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = UiKit.Disabled;

            RectTransform fillArea = UiKit.Stretch(rt, "VungRuot");
            fillArea.offsetMin = new Vector2(4f, 4f);
            fillArea.offsetMax = new Vector2(-4f, -4f);
            RectTransform fill = UiKit.Stretch(fillArea, "Ruot");
            Image fi = fill.gameObject.AddComponent<Image>();
            fi.sprite = UiKit.Rounded;
            fi.type = Image.Type.Sliced;
            fi.color = UiKit.Mint;

            RectTransform handleArea = UiKit.Stretch(rt, "VungNut");
            handleArea.offsetMin = new Vector2(14f, 0f);
            handleArea.offsetMax = new Vector2(-14f, 0f);
            RectTransform handle = UiKit.Box(handleArea, "Nut", Center, Vector2.zero, new Vector2(30f, 38f));
            Image hi = handle.gameObject.AddComponent<Image>();
            hi.sprite = UiKit.Circle;
            hi.color = UiKit.Mustard;

            Slider s = rt.gameObject.AddComponent<Slider>();
            s.fillRect = fill;
            s.handleRect = handle;
            s.targetGraphic = hi;
            s.direction = Slider.Direction.LeftToRight;
            // B-09 (yeu cau 22/11/2026): Slider keo nut theo chieu cao thanh (anchor y 0..1) nen sizeDelta.y la phan CONG THEM;
            // de nut tron 38 x 38 px thi chieu cao cong them = 38 - chieu cao thanh. Giu ti le hinh tron.
            handle.sizeDelta = new Vector2(HandleSize, HandleSize - size.y);
            hi.preserveAspect = true;
            return s;
        }

        Button ToggleRow(Transform parent, string name, string label, float y, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform row = UiKit.Box(parent, name, Top, new Vector2(0f, y), new Vector2(700f, 54f));
            UiKit.Label(row, "Nhan", new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(240f, 44f), label, 22f, TextAlignmentOptions.Left);
            return UiKit.Button(row, "Btn_" + name, new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(330f, 52f), "", UiKit.Mint, onClick, 20f);
        }

        bool syncing;

        /// <summary>Dua gia tri cai dat len cac nut (khong goi lai onChange).</summary>
        void Sync()
        {
            GameSettings s = g.Settings;
            syncing = true;
            master.value = s.masterVolume;
            music.value = s.musicVolume;
            sfx.value = s.sfxVolume;
            sensitivity.value = s.sensitivity;
            master.onValueChanged.Invoke(master.value);
            music.onValueChanged.Invoke(music.value);
            sfx.onValueChanged.Invoke(sfx.value);
            sensitivity.onValueChanged.Invoke(sensitivity.value);
            syncing = false;
            UiKit.SetLabel(invert, s.invertY ? "Bật" : "Tắt");
            UiKit.SetLabel(fullscreen, s.fullscreen ? "Bật" : "Tắt (cửa sổ)");
            UiKit.SetLabel(resolution, s.ResolutionText);
            UiKit.SetLabel(tutorial, !s.tutorialChosen ? "Tự động (ván đầu)" : s.tutorial ? "Bật" : "Tắt");
            UiKit.SetLabel(comicMotion, GameSettings.ComicMotionNames[Mathf.Clamp(s.comicMotion, 0, GameSettings.ComicMotionNames.Length - 1)]);
            UiKit.SetLabel(paperMotion, PaperKit.MotionNames[Mathf.Clamp(s.paperMotion, 0, PaperKit.MotionNames.Length - 1)]);
            UiKit.SetLabel(language, Lang.Names[Lang.Current]);
        }

        void Changed(bool screen = false)
        {
            g.Settings.Save();
            g.Settings.Apply(g.Player, screen);
            Sync();
            g.OnSettingsChanged();
        }

        public string SensitivityText { get { return sensitivityValue != null ? sensitivityValue.text : ""; } }

        public void ShowSettings()
        {
            fromPause = pause.activeSelf;
            HideAll();
            settings.SetActive(true);
            Sync();
        }

        public void CloseSettings()
        {
            settings.SetActive(false);
            if (fromPause) pause.SetActive(true);
            else main.SetActive(g.Mode == GameMode.Menu);
        }

        // =====================================================================
        // Cot truyen (F-051): bam de qua khung, giu de bo qua, chu chay ben duoi
        // =====================================================================

        const float CharsPerSecond = 40f;
        const float ReadCharsPerSecond = 13f;   // toc do doc tieng Viet uoc luong 12-15 ky tu / giay (can do o nguoi that)
        public const float HoldToSkipSeconds = 1.2f;
        const float FadeSeconds = 0.35f;

        StoryComic comic;
        ComicBook book;
        bool usingBook;
        float autoWait;
        TextMeshProUGUI caption, storyHint;
        Image skipFill;
        CanvasGroup storyGroup;
        List<StoryPanel> panels = new List<StoryPanel>();
        int panelIndex;
        float shownChars, holdTime, fade;
        Action storyDone;

        public int PanelIndex { get { return panelIndex; } }
        public int PanelCount { get { return panels.Count; } }
        public string CaptionText { get { return caption != null ? caption.text : ""; } }
        public bool CaptionComplete { get { return caption == null || caption.maxVisibleCharacters >= caption.text.Length; } }
        /// <summary>Trang truyen tranh dang mo (0 = trang 1) va so khung da hien tren trang.</summary>
        /// <summary>Trang dang mo: truyen ve san thi la so trang 1..6, tranh khoi thi la 0-based (ket thua).</summary>
        public int ComicPage { get { return usingBook ? book.PageNumber : comic.PageIndex; } }
        public int ComicPanelsOnPage { get { return usingBook ? book.ShownOnPage : comic.ShownOnPage; } }
        public bool ComicSettled { get { return usingBook ? book.Settled : comic.Settled; } }
        public ComicBook Book { get { return book; } }
        public bool UsingComicBook { get { return usingBook; } }
        public StoryScene CurrentScene { get { return panels.Count > 0 ? panels[Mathf.Min(panelIndex, panels.Count - 1)].scene : StoryScene.Office; } }

        void BuildStory()
        {
            RectTransform root = UiKit.Stretch(Canvas, "CotTruyen");
            story = root.gameObject;
            root.gameObject.AddComponent<Image>();
            storyGroup = root.gameObject.AddComponent<CanvasGroup>();

            // Truyen tranh giay cat tren mat ban go (StoryComic); loi ke nam trong hop chu cua tung khung.
            comic = new StoryComic(root);
            book = new ComicBook(root);

            // bug 24/11/2026: goi y mau trang o day de len hop loi ke -> chuyen len goc phai tren, nen muc
            Image hintBg = UiKit.Panel(root, "GoiYNen", new Vector2(1f, 1f), new Vector2(-12f, -8f), new Vector2(300f, 30f), UiKit.Ink, false);
            hintBg.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.82f);
            hintBg.raycastTarget = false;
            storyHint = UiKit.Text(hintBg.transform, "Bấm để tiếp · Giữ để bỏ qua", 15f, TMPro.TextAlignmentOptions.Center, 6f);
            storyHint.color = UiKit.Cream;
            RectTransform bar = UiKit.Box(root, "BoQua", new Vector2(1f, 1f), new Vector2(-82f, -42f), new Vector2(160f, 10f));
            var barBg = bar.gameObject.AddComponent<Image>();
            barBg.color = new Color(1f, 1f, 1f, 0.15f);
            RectTransform fillRt = UiKit.Box(bar, "Ruot", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(160f, 12f));
            skipFill = fillRt.gameObject.AddComponent<Image>();
            skipFill.color = UiKit.Mustard;
            skipFill.type = Image.Type.Filled;
            skipFill.sprite = UiKit.Rounded;
            skipFill.fillMethod = Image.FillMethod.Horizontal;
            skipFill.fillAmount = 0f;
        }

        /// <summary>Chieu mot chuoi khung tranh; xong (het khung hoac bo qua) thi goi <paramref name="done"/>.</summary>
        public void PlayStory(List<StoryPanel> list, Action done)
        {
            HideAll();
            story.SetActive(true);
            panels = list ?? new List<StoryPanel>();
            storyDone = done;
            panelIndex = -1;
            holdTime = 0f;
            comic.Clear();
            book.Hide();
            usingBook = false;
            book.Motion = g.Settings != null ? g.Settings.comicMotion : 0;
            caption = null;
            fade = 0f;
            storyGroup.alpha = 0f;
            NextPanel();
        }

        void NextPanel()
        {
            panelIndex++;
            if (panelIndex >= panels.Count)
            {
                FinishStory();
                return;
            }
            StoryPanel p = panels[panelIndex];
            Balance balance = g.Session != null ? g.Session.Balance : null;
            usingBook = p.comicPage > 0;
            if (usingBook)
            {
                comic.Clear();
                book.Show(p, balance);
                caption = book.Caption;
            }
            else
            {
                book.Hide();
                comic.Show(panels, panelIndex, balance);
                caption = comic.Caption;
            }
            shownChars = 0f;
            autoWait = 0f;
        }

        void FinishStory()
        {
            story.SetActive(false);
            comic.Clear();
            book.Hide();
            usingBook = false;
            caption = null;
            Action done = storyDone;
            storyDone = null;
            if (done != null) done();
        }

        /// <summary>Bam: chu dang chay thi hien het; da hien het thi sang khung sau (F-051 AC-3).</summary>
        public void Advance()
        {
            if (!story.activeSelf) return;
            if (!CaptionComplete || !ComicSettled)
            {
                if (usingBook) book.Finish(); else comic.Finish();
                shownChars = caption.text.Length;
                caption.maxVisibleCharacters = caption.text.Length;
                return;
            }
            NextPanel();
        }

        /// <summary>Giu du lau: bo qua ca chuoi tranh (F-051 AC-3).</summary>
        public void SkipStory()
        {
            if (!story.activeSelf) return;
            panelIndex = panels.Count;
            FinishStory();
        }

        /// <summary>Moi khung hinh (thoi gian thuc).</summary>
        public void Tick(float dt)
        {
            if (!story.activeSelf) return;
            fade = Mathf.Min(1f, fade + dt / FadeSeconds);
            storyGroup.alpha = fade;
            comic.Tick(dt);
            book.Tick(dt);
            // chu chay sau khi khung da roi xuong trang (truyen ve san: chay ngay, khung bung song song)
            if (!CaptionComplete && (usingBook || comic.Settled))
            {
                shownChars += dt * CharsPerSecond;
                caption.maxVisibleCharacters = Mathf.Min(caption.text.Length, Mathf.FloorToInt(shownChars));
            }

            if (g.Player != null && g.Player.SimulateInput) return; // test dieu khien bang Advance / SkipStory
            // truyen ve san tu sang khung sau khi het nhip khung + thoi gian doc (Animation_CotTruyen 1.3); bam de qua som
            if (usingBook && CaptionComplete && book.Settled)
            {
                autoWait += dt;
                if (book.CurrentTime >= book.CurrentDuration && autoWait >= 1.2f + caption.text.Length / ReadCharsPerSecond)
                {
                    NextPanel();
                    return;
                }
            }
            bool down = false, held = false;
            Mouse m = Mouse.current;
            Keyboard k = Keyboard.current;
            if (m != null) { down |= m.leftButton.wasReleasedThisFrame; held |= m.leftButton.isPressed; }
            if (k != null)
            {
                down |= k.spaceKey.wasReleasedThisFrame || k.enterKey.wasReleasedThisFrame;
                held |= k.spaceKey.isPressed || k.enterKey.isPressed;
            }
            if (held) holdTime += dt;
            skipFill.fillAmount = Mathf.Clamp01(holdTime / HoldToSkipSeconds);
            if (holdTime >= HoldToSkipSeconds)
            {
                holdTime = 0f;
                skipFill.fillAmount = 0f;
                SkipStory();
                return;
            }
            if (down)
            {
                bool wasShortTap = holdTime < 0.4f;
                holdTime = 0f;
                if (wasShortTap) Advance();
            }
            if (!held && !down) holdTime = 0f;
        }
    }
}
