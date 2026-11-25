using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    // ---------------------------------------------------------------- du lieu (Resources/CotTruyen/Trang_N.json, tools/cot-truyen/export.js)

    [Serializable]
    public class ComicImage
    {
        public string file;
        public float x, y, w, h;
        public int d;
        public int i;
        public ComicText[] texts;
    }

    [Serializable]
    public class ComicText
    {
        public string s;
        public float x, y, size, rot, sw, ls;
        public string anchor, fill, stroke;
        public int depth, panel;
        public bool pop;
    }

    [Serializable]
    public class ComicPanelData
    {
        public int i;
        public float[] poly;
        public float x, y, w, h;
        public ComicImage[] layers;
        public ComicImage vig;
        public ComicText[] texts;
    }

    [Serializable]
    public class ComicPageData
    {
        public int page;
        public string title, chapter;
        public int width, height;
        public ComicImage bg, border, top;
        public ComicPanelData[] panels;
        public ComicImage[] pops;
        public ComicText[] texts;

        public static ComicPageData Load(int page)
        {
            var json = Resources.Load<TextAsset>("CotTruyen/Trang_" + page);
            return json != null ? JsonUtility.FromJson<ComicPageData>(json.text) : null;
        }
    }

    /// <summary>
    /// Nhip may quay / hieu ung cua tung khung (docs/cot-truyen/Animation_CotTruyen.md muc 3). Toa do khung 1920 x 1080.
    /// pan: may quay truot ngang (px, tai lop nhan vat f = 1); panY: cau len (px); dolly: day vao (+) / lui ra (-) theo phan;
    /// punch / shake / flash / flicker: thoi diem (giay tu luc khung hien, &lt; 0 = khong); rumble: rung nhe lien tuc (px).
    /// </summary>
    public struct ComicCue
    {
        public float dur, pan, panY, dolly, punch, shake, shake2, flash, flicker, rumble, shakePx;

        static ComicCue C(float dur, float pan = 0f, float dolly = 0f, float punch = -1f, float shake = -1f, float flash = -1f,
            float flicker = -1f, float rumble = 0f, float panY = 0f, float shake2 = -1f, float shakePx = 6f)
        {
            return new ComicCue { dur = dur, pan = pan, panY = panY, dolly = dolly, punch = punch, shake = shake, shake2 = shake2, flash = flash,
                flicker = flicker, rumble = rumble, shakePx = shakePx };
        }

        public static ComicCue For(int page, int panel)
        {
            switch (page * 10 + panel)
            {
                // Trang 1 · Bi duoi viec
                case 10: return C(4f, pan: 20f, punch: 1.0f);
                case 11: return C(4f, pan: 120f);
                case 12: return C(3f, shake: 0.7f, shake2: 1.2f, shakePx: 3f);
                case 13: return C(4f, dolly: 0.03f);
                case 14: return C(5f, dolly: 0.08f, shake: 2.2f, punch: 2.2f);
                // Trang 2 · Con bao
                case 20: return C(4f, flash: 0.6f, shake: 0.85f, pan: 30f);
                case 21: return C(5f, punch: 2.2f);
                case 22: return C(3f, dolly: -0.06f);
                case 23: return C(3f, dolly: 0.05f);
                case 24: return C(4f, dolly: 0.04f, pan: 30f);
                // Trang 3 · Dem mat dien
                case 30: return C(2.5f, flicker: 0.4f, shake: 1.0f, shakePx: 3f);
                case 31: return C(3f);
                case 32: return C(3f, dolly: 0.05f);
                case 33: return C(7f, pan: 160f);
                case 34: return C(4f, dolly: 0.04f);
                // Trang 4 · Tin don
                case 40: return C(4f, pan: 40f);
                case 41: return C(4f);
                case 42: return C(4f, dolly: 0.05f);
                case 43: return C(6f, punch: 0.8f, shake: 0.8f);
                case 44: return C(4f, panY: 40f);
                // Trang 5 · Mon cuoi cung
                case 50: return C(5f, dolly: 0.10f);
                case 51: return C(5f, rumble: 2f);
                case 52: return C(5f, flash: 0.5f, punch: 0.6f);
                case 53: return C(4f, punch: 1.0f);
                case 54: return C(6f, dolly: 0.06f);
                // Trang 6 · Het no, co vo
                case 60: return C(4f, shake: 0.5f);
                case 61: return C(6f, pan: 120f);
                case 62: return C(12f, dolly: -0.25f, shake: 4.4f);
                default: return C(4f);
            }
        }
    }

    /// <summary>
    /// Truyen tranh giay ve san (CotTruyen_BepNhanPham.zip, 19/11): moi khung la hop san khau 6 lop giay pp0..pp5
    /// (Animation_CotTruyen.md muc 1.1). Khung truot vao theo huong duong chia khung (8 hinh, giat 12 hinh/giay),
    /// cac lop bung ra theo do sau tu nen den tien canh (moi lop tre 2 hinh, nhu sach pop-up); lop nhan vat tho (98-100%),
    /// lop pp1..pp5 rung vien (boil) 12 lan/giay; may quay (truot, day, thi sai f = 0,2 / 0,4 / 0,67 / 1 / 1,33 / 2) muot.
    /// Chu (bong thoai, chu thich, tuong thanh, bien hieu) la TextMeshPro dat dung cho chu trong tranh goc (Dieu 6).
    /// </summary>
    public class ComicBook
    {
        public const float StepFps = 12f;
        public static readonly float[] Parallax = { 0.20f, 0.40f, 0.67f, 1.00f, 1.33f, 2.00f };
        const float SlideSeconds = 8f / 24f;
        const float LayerDelay = 2f / 24f;
        const float PageSeconds = 0.42f;
        const float NarrationHeight = 64f;
        const float PageW = 1920f, PageH = 1080f;

        class Layer
        {
            public RectTransform holder;
            public Vector2 rest;
            public int depth;
            public readonly List<Graphic> shown = new List<Graphic>();
            public readonly List<RectTransform> sfx = new List<RectTransform>();
        }

        class PanelView
        {
            public int index;
            public ComicPanelData data;
            public ComicCue cue;
            public RectTransform root, cam;
            public Vector2 camRest;
            public readonly List<Layer> layers = new List<Layer>();
            public Image flash, dark;
            public Vector2 slideFrom;
            public float t;
            public bool active;
        }

        readonly RectTransform root, fit;
        RectTransform page;
        Image narrationBox;
        TextMeshProUGUI narration;
        ComicPageData data;
        int pageNumber = -1;
        float pageT;
        readonly List<PanelView> panels = new List<PanelView>();
        readonly List<GameObject> pops = new List<GameObject>();
        PanelView current;
        Dictionary<int, int> dayMap = new Dictionary<int, int>();
        float clock;
        float shakeLeft;
        float shakePx;

        /// <summary>0 day du, 1 giam (khong boil / rung / zoom giat, nhan vat on ones), 2 tat (chi hien khung, khong hoat anh).</summary>
        public int Motion;

        public ComicBook(RectTransform parent)
        {
            root = UiKit.Stretch(parent, "TruyenTranh");
            fit = UiKit.Box(root, "Khop", new Vector2(0.5f, 1f), Vector2.zero, new Vector2(PageW, PageH));
            fit.pivot = new Vector2(0.5f, 1f);

            narrationBox = UiKit.Panel(root, "LoiKe", new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(1180f, NarrationHeight - 10f), UiKit.Cream, false);
            narrationBox.raycastTarget = false;
            narration = UiKit.Text(narrationBox.transform, "", 22f, TextAlignmentOptions.Left, 18f);
            narration.fontSizeMin = 15f;
            root.gameObject.SetActive(false);
        }

        public bool Visible { get { return root.gameObject.activeSelf; } }
        public TextMeshProUGUI Caption { get { return narration; } }
        public int PageNumber { get { return pageNumber; } }
        public int ShownOnPage { get { int n = 0; foreach (PanelView p in panels) if (p.active) n++; return n; } }
        public bool Settled
        {
            get
            {
                if (current == null) return true;
                return pageT >= PageSeconds && current.t >= SlideSeconds + LayerDelay * 6f + 3f / StepFps;
            }
        }
        /// <summary>Do dai goi y cua khung dang doc (giay) theo bang nhip.</summary>
        public float CurrentDuration { get { return current != null ? current.cue.dur : 0f; } }
        public float CurrentTime { get { return current != null ? current.t : 0f; } }
        /// <summary>Lop giay dang hien o khung dang doc (test).</summary>
        public int CurrentLayerCount { get { return current != null ? current.layers.Count : 0; } }
        /// <summary>Chu TextMeshPro tren trang (test: chu khong nam trong anh).</summary>
        public List<string> PageTexts()
        {
            var list = new List<string>();
            if (page != null) foreach (TextMeshProUGUI t in page.GetComponentsInChildren<TextMeshProUGUI>(true)) list.Add(t.text);
            return list;
        }

        public static float Step(float t) { return Mathf.Floor(t * StepFps) / StepFps; }

        public void Show(StoryPanel p, Balance balance)
        {
            root.gameObject.SetActive(true);
            dayMap = balance != null ? StoryScript.ComicDayMap(balance) : new Dictionary<int, int>();
            if (p.comicPage != pageNumber || page == null) BuildPage(p.comicPage);
            narration.text = p.text;
            narration.maxVisibleCharacters = 0;
            narration.ForceMeshUpdate();
            int k = Mathf.Clamp(p.comicPanel, 0, panels.Count - 1);
            if (panels.Count == 0) { current = null; return; }
            current = panels[k];
            Reveal(current);
        }

        public void Hide()
        {
            Clear();
            root.gameObject.SetActive(false);
        }

        public void Clear()
        {
            if (page != null) UnityEngine.Object.Destroy(page.gameObject);
            page = null;
            panels.Clear();
            pops.Clear();
            current = null;
            pageNumber = -1;
            data = null;
        }

        /// <summary>Bam giua luc hoat anh: cho xong ngay (khung hien du lop).</summary>
        public void Finish()
        {
            pageT = Mathf.Max(pageT, PageSeconds);
            if (current != null) current.t = Mathf.Max(current.t, SlideSeconds + LayerDelay * 6f + 4f / StepFps);
            Tick(0f);
        }

        // ---------------------------------------------------------------- dung trang

        void BuildPage(int number)
        {
            Clear();
            pageNumber = number;
            pageT = Motion >= 2 ? PageSeconds : 0f;
            data = ComicPageData.Load(number);
            page = Box(fit, "Trang_" + number, 0f, 0f, PageW, PageH);
            if (data == null)
            {
                Debug.LogWarning("Quán Ăn Hope: thiếu Resources/CotTruyen/Trang_" + number + ".json (chạy tools/cot-truyen/export.js).");
                return;
            }
            string dir = "CotTruyen/Trang_" + number + "/";
            Img(page, dir, data.bg, "Nen");

            for (int i = 0; i < data.panels.Length; i++)
            {
                ComicPanelData pd = data.panels[i];
                var v = new PanelView { index = i, data = pd, cue = ComicCue.For(number, i) };
                v.root = Box(page, "Khung_" + (i + 1), 0f, 0f, PageW, PageH);
                v.root.gameObject.AddComponent<CanvasRenderer>();
                var mask = v.root.gameObject.AddComponent<PolygonGraphic>();
                mask.Points = pd.poly;
                mask.raycastTarget = false;
                v.root.gameObject.AddComponent<Mask>().showMaskGraphic = false;

                Vector2 pivot = new Vector2((pd.x + pd.w * 0.5f) / PageW, 1f - (pd.y + pd.h * 0.5f) / PageH);
                v.cam = Box(v.root, "MayQuay", 0f, 0f, PageW, PageH);
                SetPivotKeep(v.cam, pivot);
                v.camRest = v.cam.anchoredPosition;
                for (int d = 0; d <= 5; d++)
                {
                    ComicImage li = Find(pd.layers, d);
                    var layer = new Layer { depth = d, holder = Box(v.cam, "Lop" + d, 0f, 0f, PageW, PageH) };
                    SetPivotKeep(layer.holder, new Vector2(pivot.x, 1f - (pd.y + pd.h) / PageH));   // nhan vat "dung" o day khung
                    layer.rest = layer.holder.anchoredPosition;
                    if (li != null) layer.shown.Add(Img(layer.holder, dir, li, "Anh"));
                    v.layers.Add(layer);
                }
                if (pd.vig != null) Img(v.cam, dir, pd.vig, "Toi");
                if (pd.texts != null)
                    foreach (ComicText t in pd.texts)
                    {
                        Layer host = v.layers[Mathf.Clamp(t.depth < 0 ? 4 : t.depth, 0, 5)];
                        TextMeshProUGUI tmp = Text(host.holder, t);
                        host.shown.Add(tmp);
                        if (!string.IsNullOrEmpty(t.stroke)) host.sfx.Add(tmp.rectTransform);
                    }
                v.flash = Fill(v.cam, "Choi", Color.white);
                v.dark = Fill(v.cam, "Toi_Den", UiKit.Hex("#2A2445"));
                // huong truot vao: khung trai tu trai, khung phai tu phai, khung giua tu duoi (theo duong chia khung)
                float cx = pd.x + pd.w * 0.5f;
                v.slideFrom = cx < PageW * 0.34f ? new Vector2(-90f, 0f) : cx > PageW * 0.66f ? new Vector2(90f, 0f) : new Vector2(0f, -70f);
                v.root.gameObject.SetActive(false);
                panels.Add(v);
            }

            Img(page, dir, data.border, "VienKhung");
            if (data.pops != null)
                foreach (ComicImage pop in data.pops)
                {
                    RawImage img = Img(page, dir, pop, "Tran_" + (pop.i + 1));
                    if (pop.texts != null) foreach (ComicText t in pop.texts) Text(img.rectTransform, t, page);
                    img.gameObject.SetActive(false);
                    pops.Add(img.gameObject);
                }
            Img(page, dir, data.top, "Phu");
            // hat giay chung toan trang (Animation_CotTruyen 1.1); anh lop khong co hat de nhe
            RectTransform grain = UiKit.Stretch(page, "HatGiay");
            var g = grain.gameObject.AddComponent<RawImage>();
            g.texture = StoryComic.GrainTexture;
            g.color = new Color(1f, 1f, 1f, 0.10f);
            g.uvRect = new Rect(0f, 0f, PageW / 256f, PageH / 256f);
            g.raycastTarget = false;
            if (data.texts != null) foreach (ComicText t in data.texts) Text(page, t);
            ApplyPage();
        }

        void Reveal(PanelView v)
        {
            v.active = true;
            v.t = Motion >= 2 ? 10f : 0f;
            v.root.gameObject.SetActive(true);
            foreach (GameObject pop in pops)
                if (pop.name == "Tran_" + (v.index + 1)) pop.SetActive(true);
            Animate(v, 0f);
        }

        // ---------------------------------------------------------------- hoat anh

        public void Tick(float dt)
        {
            if (!Visible) return;
            clock += dt;
            pageT += dt;
            Fit();
            ApplyPage();
            foreach (PanelView v in panels)
            {
                if (!v.active) continue;
                float before = v.t;
                v.t += dt;
                if (v == current) Cues(v, before, v.t);
                Animate(v, dt);
            }
            shakeLeft = Mathf.Max(0f, shakeLeft - dt);
        }

        void Fit()
        {
            Vector2 size = root.rect.size;
            if (size.x <= 0f) return;
            float s = Mathf.Min(size.x / PageW, (size.y - NarrationHeight) / PageH);
            fit.localScale = Vector3.one * s;
            fit.anchoredPosition = new Vector2(0f, -Mathf.Max(0f, (size.y - NarrationHeight - PageH * s) * 0.5f));
            narrationBox.rectTransform.sizeDelta = new Vector2(Mathf.Min(PageW * s, size.x - 24f), NarrationHeight - 12f);
        }

        void ApplyPage()
        {
            if (page == null) return;
            float p = Mathf.Clamp01(Step(pageT) / PageSeconds);
            float e = 1f - (1f - p) * (1f - p);
            Vector2 shake = Vector2.zero;
            if (shakeLeft > 0f && Motion == 0)
            {
                int k = Mathf.FloorToInt(clock * StepFps);
                shake = new Vector2(Hash(k, 1), Hash(k, 2)) * shakePx;
            }
            page.anchoredPosition = new Vector2((1f - e) * 2100f, 0f) + shake;
            page.localEulerAngles = new Vector3(0f, 0f, (1f - e) * 4f);
        }

        void Cues(PanelView v, float before, float now)
        {
            if (Motion != 0) return;
            if (Crossed(v.cue.shake, before, now) || Crossed(v.cue.shake2, before, now)) { shakeLeft = 4f / StepFps; shakePx = v.cue.shakePx; }
        }

        static bool Crossed(float at, float before, float now) { return at >= 0f && before < at && now >= at; }

        void Animate(PanelView v, float dt)
        {
            float t = v.t;
            bool full = Motion == 0;
            // 1) khung truot vao (8 hinh, giat 12/s, ease-out)
            float ts = Motion >= 2 ? 10f : Step(t);
            float slide = Mathf.Clamp01(ts / SlideSeconds);
            float es = 1f - Mathf.Pow(1f - slide, 3f);
            v.root.anchoredPosition = v.slideFrom * (1f - es);

            // 2) may quay muot: truot / cau / day (chi khung dang doc chay tiep; khung cu giu tu the cuoi)
            float camT = Mathf.Max(0f, t - SlideSeconds);
            float prog = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(camT / Mathf.Max(0.1f, v.cue.dur)));
            float pan = v.cue.pan * (prog - 0.5f), panY = v.cue.panY * prog;
            float dolly = v.cue.dolly >= 0f ? v.cue.dolly * prog : -v.cue.dolly * (1f - prog);
            if (Motion >= 2) { pan = 0f; panY = 0f; dolly = 0f; }

            // zoom giat: nhay 115% (cat cung), giu 0,35 giay, tra ve trong 10 hinh
            float punch = 1f;
            if (full && v.cue.punch >= 0f && camT >= v.cue.punch)
            {
                float k = camT - v.cue.punch;
                punch = k < 0.35f ? 1.15f : Mathf.Lerp(1.15f, 1f, Mathf.SmoothStep(0f, 1f, (k - 0.35f) / (10f / 24f)));
            }
            Vector2 rumble = full && v.cue.rumble > 0f && v == current
                ? new Vector2(Hash(Mathf.FloorToInt(clock * StepFps), 7), Hash(Mathf.FloorToInt(clock * StepFps), 8)) * v.cue.rumble : Vector2.zero;
            v.cam.localScale = Vector3.one * punch;
            v.cam.anchoredPosition = v.camRest + rumble;

            // 3) lop bung theo do sau (pop-up), thi sai, tho, boil
            int frame = Mathf.FloorToInt(clock * StepFps);
            int order = 0;
            for (int i = 0; i < v.layers.Count; i++)
            {
                Layer l = v.layers[i];
                if (l.shown.Count == 0) continue;
                float lt = (Motion >= 2 ? 10f : Step(t)) - SlideSeconds - LayerDelay * order;
                order++;
                float pop = lt < 0f ? 0f : lt < 1f / StepFps ? 0.9f : lt < 2f / StepFps ? 1.04f : 1f;
                if (!full && lt >= 0f) pop = 1f;
                bool on = lt >= 0f;
                foreach (Graphic gr in l.shown) if (gr.enabled != on) gr.enabled = on;

                float f = Parallax[l.depth];
                Vector2 off = new Vector2(-pan * f, panY * f * 0.6f);
                if (full && l.depth >= 1)
                {
                    float amp = l.depth >= 3 ? 1.2f : 0.6f;
                    off += new Vector2(Hash(frame * 7 + l.depth, 3), Hash(frame * 7 + l.depth, 4)) * amp;
                }
                float breath = 1f;
                if (l.depth == 3)
                {
                    // idle_breath: 4 tu the giu 3 hinh (on twos -> ~1 giay mot nhip); muc Giam: muot (on ones)
                    float cyc = full ? Mathf.Floor(clock * 4f) % 4f : clock * 4f % 4f;
                    breath = 1f - 0.012f * Mathf.Abs(Mathf.Sin(cyc / 4f * Mathf.PI));
                }
                l.holder.anchoredPosition = l.rest + off;
                float sc = (1f + dolly * (0.4f + 0.6f * f)) * pop;
                l.holder.localScale = new Vector3(sc, sc * breath, 1f);

                // chu tuong thanh: bop 120% -> 95% -> 100% trong 6 hinh
                foreach (RectTransform sfx in l.sfx)
                {
                    float st = lt - 1f / StepFps;
                    float k = st < 0f ? 0f : st < 2f / 24f ? 1.2f : st < 4f / 24f ? 0.95f : 1f;
                    if (!full && st >= 0f) k = 1f;
                    sfx.localScale = Vector3.one * k;
                }
            }

            // 4) chop trang (2 hinh), den chop tat (3 lan, 2 hinh sang 2 hinh toi) roi phu toi
            float fa = 0f, da = 0f;
            if (Motion < 2)
            {
                if (v.cue.flash >= 0f && camT >= v.cue.flash && camT < v.cue.flash + 2f / 24f) fa = 0.9f;
                if (v.cue.flicker >= 0f && camT >= v.cue.flicker)
                {
                    int k = Mathf.FloorToInt((camT - v.cue.flicker) * 12f);
                    da = k >= 6 ? 0.55f : (k % 2 == 1 ? 0.55f : 0f);
                }
            }
            v.flash.color = new Color(1f, 1f, 1f, fa);
            v.dark.color = new Color(v.dark.color.r, v.dark.color.g, v.dark.color.b, da);
        }

        static float Hash(int frame, int axis)
        {
            unchecked
            {
                uint h = (uint)(frame * 19349663) ^ (uint)(axis * 83492791) ^ 0x9E3779B9u;
                h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
                return (h & 0xFFFF) / 32767.5f - 1f;
            }
        }

        // ---------------------------------------------------------------- dung hinh

        static ComicImage Find(ComicImage[] list, int d)
        {
            if (list == null) return null;
            foreach (ComicImage c in list) if (c.d == d) return c;
            return null;
        }

        /// <summary>Khung dat theo toa do trang (goc tren trai, y xuong duoi).</summary>
        static RectTransform Box(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, -y);
            return rt;
        }

        /// <summary>Doi pivot ma khung khong xe dich (de phong to / tho quanh tam khung).</summary>
        static void SetPivotKeep(RectTransform rt, Vector2 pivot)
        {
            Vector2 size = rt.sizeDelta;
            Vector2 delta = pivot - rt.pivot;
            rt.pivot = pivot;
            rt.anchoredPosition += new Vector2(delta.x * size.x, delta.y * size.y);
        }

        static RawImage Img(Transform parent, string dir, ComicImage c, string name)
        {
            if (c == null) return null;
            RectTransform rt = Box(parent, name, c.x, c.y, c.w, c.h);
            // holder co pivot khac (0,1): dat lai theo goc tren trai cua cha
            var img = rt.gameObject.AddComponent<RawImage>();
            img.texture = Resources.Load<Texture2D>(dir + c.file);
            img.raycastTarget = false;
            return img;
        }

        static Image Fill(RectTransform parent, string name, Color c)
        {
            RectTransform rt = Box(parent, name, 0f, 0f, PageW, PageH);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(c.r, c.g, c.b, 0f);
            img.raycastTarget = false;
            return img;
        }

        static readonly Regex DayRx = new Regex(@"Ngày (\d+)(–(\d+))?");

        string Days(string s)
        {
            return DayRx.Replace(s, m =>
            {
                string a = Map(m.Groups[1].Value);
                return m.Groups[3].Success ? "Ngày " + a + "–" + Map(m.Groups[3].Value) : "Ngày " + a;
            });
        }

        string Map(string n)
        {
            int v, to;
            return int.TryParse(n, out v) && dayMap.TryGetValue(v, out to) ? to.ToString() : n;
        }

        /// <summary>Chu TextMeshPro tai diem neo cua chu SVG (x, duong co so y), xoay, vien tuong thanh.</summary>
        TextMeshProUGUI Text(RectTransform parent, ComicText t, RectTransform pageSpace = null)
        {
            float ax = t.anchor == "middle" ? 0.5f : t.anchor == "end" ? 1f : 0f;
            float w = Mathf.Max(200f, t.size * (t.s.Length + 2) * 0.8f), h = t.size * 1.7f;
            // SVG: y la duong co so; tam dong chu ~ 0,34 co chu phia tren (cung cach tranh goc canh giua)
            RectTransform rt = Box(parent, "Chu", 0f, 0f, w, h);
            rt.pivot = new Vector2(ax, 0.5f);
            Vector2 pos = new Vector2(t.x, -(t.y - t.size * 0.34f));
            if (pageSpace != null) pos -= new Vector2(parent.anchoredPosition.x, parent.anchoredPosition.y);
            rt.anchoredPosition = pos;
            rt.localEulerAngles = new Vector3(0f, 0f, -t.rot);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            Lang.Attach(tmp);   // F-110
            tmp.font = UiKit.Font;
            tmp.text = Days(t.s);
            tmp.fontSize = t.size;
            tmp.enableAutoSizing = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = ax == 0.5f ? TextAlignmentOptions.Center : ax == 1f ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
            tmp.color = string.IsNullOrEmpty(t.fill) ? UiKit.Ink : UiKit.Hex(t.fill);
            tmp.characterSpacing = t.size > 0f ? t.ls / t.size * 100f : 0f;
            tmp.raycastTarget = false;
            if (!string.IsNullOrEmpty(t.stroke) && t.sw > 0f)
            {
                // vien SVG ve kieu paint-order: stroke -> lo ra ngoai nua be day; shader SDF can bat tu khoa OUTLINE_ON
                Material m = tmp.fontMaterial;
                m.EnableKeyword("OUTLINE_ON");
                tmp.outlineColor = UiKit.Hex(t.stroke);
                tmp.outlineWidth = Mathf.Clamp(t.sw / t.size * 2.4f, 0.08f, 0.32f);
                tmp.fontMaterial = m;
            }
            return tmp;
        }
    }

    /// <summary>Hinh tu giac cua khung truyen (diem 1920 x 1080, y xuong) - dung lam mat na stencil (Mask).</summary>
    public class PolygonGraphic : MaskableGraphic
    {
        float[] points;
        public float[] Points { get { return points; } set { points = value; SetVerticesDirty(); } }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (points == null || points.Length < 6) return;
            Rect r = rectTransform.rect;   // pivot (0,1): goc tren trai la (r.xMin, r.yMax)
            int n = points.Length / 2;
            for (int i = 0; i < n; i++)
                vh.AddVert(new Vector3(r.xMin + points[i * 2], r.yMax - points[i * 2 + 1]), color, Vector2.zero);
            for (int i = 1; i < n - 1; i++) vh.AddTriangle(0, i, i + 1);
        }
    }
}
