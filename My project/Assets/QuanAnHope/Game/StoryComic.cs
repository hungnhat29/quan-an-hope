using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Cot truyen kieu truyen tranh giay cat (yeu cau 19/11; bao cao reports/Phong cách giấy cắt lớp diorama.md):
    /// - Trang truyen la to giay kem nam tren mat ban go; moi trang 1-2 khung, khung hien lan luot theo thu tu doc trai -> phai.
    /// - Khung la tam bia giay: vien trang (lop cat), bong do lech mot huong anh sang, van giay, bang dinh.
    /// - Moi hinh trong tranh (StoryArt) la mot manh giay: vien trang + bong do -> nhin nhu cac lop dan chong (diorama).
    /// - Nhip stop-motion: khung roi vao, trang truot vao, chu tuong thanh bat ra deu chay "on twos" 12 hinh/giay;
    ///   nhan vat "rung" (boil) 6 lan/giay; rieng camera trong khung (day cham + thi sai lop phong) muot.
    /// </summary>
    public class StoryComic
    {
        public const float StepFps = 12f;      // hoat anh giat kieu stop motion (on twos tren nen 24)
        public const float BoilFps = 6f;       // giu moi tu the 2 hinh 12 fps
        const float EnterSeconds = 0.34f;
        const float PageSeconds = 0.42f;
        const float SfxAt = 0.34f;
        const float BoilPx = 1.6f, BoilDeg = 0.6f;
        const float KenBurnsZoom = 0.05f, KenBurnsSeconds = 7f;

        static readonly Vector2 PageSize = new Vector2(1210f, 600f);
        static readonly Vector2 PagePos = new Vector2(0f, 26f);
        const float PageMargin = 22f, Gutter = 20f, RimPx = 8f;

        static readonly Color Desk = UiKit.Hex("#A8714A");
        static readonly Color PagePaper = UiKit.Hex("#F7EBD3");
        static readonly Color Rim = UiKit.Hex("#FFFDF6");
        static readonly Color Tape = new Color(0.95f, 0.76f, 0.31f, 0.72f);

        class Group
        {
            public readonly List<RectTransform> parts = new List<RectTransform>();
            public readonly List<Vector2> pos = new List<Vector2>();
            public readonly List<float> rot = new List<float>();
            public float parallax = 1f;
            public int seed;
        }

        class PanelView
        {
            public RectTransform card, art;
            public TextMeshProUGUI caption, sfx;
            public float t, tilt, artScale;
            public Vector2 cardRest;
            public readonly List<Group> groups = new List<Group>();
            public int boilFrame = -1;
        }

        readonly RectTransform root;
        RectTransform page;
        float pageT, pageTilt;
        int pageIndex = -1;
        readonly List<RectTransform> slots = new List<RectTransform>();
        readonly List<PanelView> shown = new List<PanelView>();
        PanelView current;
        float clock;

        public StoryComic(RectTransform parent)
        {
            root = parent;
            var desk = root.gameObject.GetComponent<Image>();
            if (desk == null) desk = root.gameObject.AddComponent<Image>();
            desk.color = Desk;
            Grain(root, 0.16f, 3f);
            // but chi nam tren ban (do vat diorama, khong noi dung)
            Image pencil = Shape(root, "ButChi", new Vector2(1f, 0f), new Vector2(-150f, 52f), new Vector2(230f, 16f), UiKit.Mustard, UiKit.Rounded);
            pencil.rectTransform.localEulerAngles = new Vector3(0f, 0f, 11f);
            Shape(pencil.transform, "NgoiBut", new Vector2(0f, 0.5f), new Vector2(-10f, 0f), new Vector2(26f, 12f), UiKit.Ink, UiKit.Rounded);
            Shape(pencil.transform, "Tay", new Vector2(1f, 0.5f), new Vector2(8f, 0f), new Vector2(24f, 16f), UiKit.Coral, UiKit.Rounded);
        }

        /// <summary>Chu loi ke cua khung dang doc.</summary>
        public TextMeshProUGUI Caption { get { return current != null ? current.caption : null; } }
        public int PageIndex { get { return pageIndex; } }
        public int ShownOnPage { get { return shown.Count; } }
        /// <summary>Hoat anh vao (khung roi, trang truot, chu tuong thanh) da xong.</summary>
        public bool Settled { get { return current == null || (current.t >= Mathf.Max(EnterSeconds, SfxAt + 0.2f) && pageT >= PageSeconds); } }

        public static float Step(float t) { return Mathf.Floor(t * StepFps) / StepFps; }

        /// <summary>Hien khung <paramref name="index"/>: sang trang moi neu khung thuoc trang khac.</summary>
        public void Show(List<StoryPanel> all, int index, Balance balance)
        {
            StoryPanel p = all[index];
            if (p.page != pageIndex || page == null) BuildPage(all, p.page);
            int slot = 0;
            for (int i = 0; i < index; i++) if (all[i].page == p.page) slot++;
            slot = Mathf.Min(slot, slots.Count - 1);
            current = BuildPanel(slots[slot], p, balance, slots.Count);
            shown.Add(current);
        }

        /// <summary>Bam giua luc hoat anh: cho xong ngay.</summary>
        public void Finish()
        {
            pageT = PageSeconds;
            if (current != null) current.t = Mathf.Max(current.t, SfxAt + 0.25f);
            Tick(0f);
        }

        public void Clear()
        {
            if (page != null) Object.Destroy(page.gameObject);
            page = null;
            pageIndex = -1;
            slots.Clear();
            shown.Clear();
            current = null;
        }

        // ---------------------------------------------------------------- dung trang / khung

        void BuildPage(List<StoryPanel> all, int index)
        {
            if (page != null) Object.Destroy(page.gameObject);
            slots.Clear();
            shown.Clear();
            pageIndex = index;
            pageT = 0f;
            pageTilt = index % 2 == 0 ? -0.6f : 0.5f;

            page = UiKit.Box(root, "Trang_" + (index + 1), new Vector2(0.5f, 0.5f), PagePos, PageSize);
            var shadow = Shape(page, "BongTrang", new Vector2(0.5f, 0.5f), new Vector2(10f, -12f), PageSize, new Color(0f, 0f, 0f, 0.3f), UiKit.Rounded);
            shadow.raycastTarget = false;
            Shape(page, "Giay", new Vector2(0.5f, 0.5f), Vector2.zero, PageSize, PagePaper, UiKit.Rounded);
            Grain(page, 0.07f, 1f);
            var num = UiKit.Label(page, "SoTrang", new Vector2(1f, 0f), new Vector2(-28f, 4f), new Vector2(80f, 22f), (index + 1).ToString(), 15f, TextAlignmentOptions.Right);
            num.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.45f);

            int n = 0;
            foreach (StoryPanel p in all) if (p.page == index) n++;
            n = Mathf.Max(1, n);
            float w = (PageSize.x - PageMargin * 2f - Gutter * (n - 1)) / n;
            float h = PageSize.y - PageMargin * 2f;
            for (int i = 0; i < n; i++)
            {
                float x = -PageSize.x * 0.5f + PageMargin + w * 0.5f + i * (w + Gutter);
                RectTransform slot = UiKit.Box(page, "O_" + i, new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(w, h));
                // vet but chi cho o chua hien (bo cuc trang doc truoc duoc)
                Shape(slot, "VetBut", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h), new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.06f), UiKit.Rounded);
                slots.Add(slot);
            }
            ApplyPage();
        }

        PanelView BuildPanel(RectTransform slot, StoryPanel p, Balance balance, int perPage)
        {
            var v = new PanelView();
            Vector2 size = slot.sizeDelta;
            v.tilt = ((p.scene.GetHashCode() * 7 + slot.GetSiblingIndex() * 3) % 5 - 2) * 0.6f;
            v.card = UiKit.Box(slot, "Khung_" + p.scene, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            v.cardRest = Vector2.zero;

            // bia giay: bong do (anh sang tu tren trai) + vien trang
            Image cardImg = Shape(v.card, "VienGiay", new Vector2(0.5f, 0.5f), Vector2.zero, size, Rim, UiKit.Rounded);
            var sh = cardImg.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.38f);
            sh.effectDistance = new Vector2(6f, -8f);

            Vector2 inner = size - new Vector2(RimPx * 2f, RimPx * 2f);
            RectTransform win = UiKit.Box(v.card, "Tranh", new Vector2(0.5f, 0.5f), Vector2.zero, inner);
            win.gameObject.AddComponent<RectMask2D>();
            Color back = StoryArt.Backdrop(p.scene);
            Shape(win, "Phong", new Vector2(0.5f, 0.5f), Vector2.zero, inner, back, null);

            // hop loi ke kieu truyen tranh o dau khung
            float capH = perPage >= 2 ? 142f : 104f;
            float artH = inner.y - capH - 6f;
            float s = Mathf.Min(inner.x / 1000f, artH / 500f);
            Shape(win, "SanRong", new Vector2(0.5f, 0f), Vector2.zero, new Vector2(inner.x, 90f * s), Shade(back, 0.82f), null);

            v.art = UiKit.Box(win, "Lop", new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1000f, 500f));
            v.art.pivot = new Vector2(0.5f, 0f);
            v.art.anchoredPosition = Vector2.zero;
            v.artScale = s;
            v.art.localScale = Vector3.one * s;
            StoryArt.Draw(v.art, p.scene, balance);
            PaperCut(v);

            Grain(win, 0.09f, 1.6f);

            Image capBox = Shape(win, "LoiKe", new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(inner.x - 20f, capH - 12f), UiKit.Cream, UiKit.Rounded);
            var capShadow = capBox.gameObject.AddComponent<Shadow>();
            capShadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            capShadow.effectDistance = new Vector2(3f, -4f);
            v.caption = UiKit.Text(capBox.transform, p.text, perPage >= 2 ? 22f : 26f, TextAlignmentOptions.TopLeft, 18f);
            v.caption.fontSizeMin = 16f;
            v.caption.maxVisibleCharacters = 0;
            v.caption.ForceMeshUpdate();

            if (!string.IsNullOrEmpty(p.sfx))
            {
                v.sfx = UiKit.Label(win, "TuongThanh", new Vector2(1f, 0f), new Vector2(-24f - inner.x * 0.04f, artH * 0.62f), new Vector2(inner.x * 0.5f, 70f), p.sfx, perPage >= 2 ? 44f : 56f, TextAlignmentOptions.Right);
                v.sfx.color = UiKit.Coral;
                v.sfx.fontStyle = FontStyles.Bold;
                v.sfx.outlineColor = UiKit.Cream;
                v.sfx.outlineWidth = 0.22f;
                v.sfx.rectTransform.localEulerAngles = new Vector3(0f, 0f, 7f);
                v.sfx.rectTransform.localScale = Vector3.zero;
            }

            // bang dinh giu khung len trang
            Image tape = Shape(v.card, "BangDinh", new Vector2(0.5f, 1f), new Vector2(size.x * 0.18f, 6f), new Vector2(96f, 28f), Tape, null);
            tape.rectTransform.localEulerAngles = new Vector3(0f, 0f, v.tilt >= 0f ? -7f : 6f);

            Animate(v);
            return v;
        }

        /// <summary>Moi hinh cua StoryArt thanh manh giay cat: vien trang + bong do; gom theo nhan vat de rung cung nhau.</summary>
        static void PaperCut(PanelView v)
        {
            var byKey = new Dictionary<string, Group>();
            for (int i = 0; i < v.art.childCount; i++)
            {
                var rt = v.art.GetChild(i) as RectTransform;
                if (rt == null) continue;
                string n = rt.name;
                bool background = n == "Phong" || n == "San";
                bool face = n.EndsWith("_Mat");
                var g = rt.GetComponent<Graphic>();
                if (background)
                {
                    // khung da co phong + san rong het khung; tam phong 1000 x 500 cu se lo vien
                    if (g != null) g.enabled = false;
                    continue;
                }
                string key = GroupKey(n);
                bool scenery = System.Array.IndexOf(Scenery, key) >= 0;
                if (!face && g is Image)
                {
                    var o = rt.gameObject.AddComponent<Outline>();
                    o.effectColor = Rim;
                    o.effectDistance = new Vector2(2.6f, -2.6f);
                    var s = rt.gameObject.AddComponent<Shadow>();
                    s.effectColor = new Color(0.17f, 0.12f, 0.1f, 0.3f);
                    s.effectDistance = new Vector2(5f, -6f);
                }
                Group grp;
                if (!byKey.TryGetValue(key, out grp))
                {
                    // canh (nha, tuong quan) la lop sau: dung yen nhu phim truong, troi cham hon nhan vat
                    grp = new Group { parallax = scenery ? 0.45f : 1f, seed = key.GetHashCode() };
                    byKey[key] = grp;
                    v.groups.Add(grp);
                }
                grp.parts.Add(rt);
                grp.pos.Add(rt.anchoredPosition);
                grp.rot.Add(rt.localEulerAngles.z);
            }
        }

        static readonly string[] Scenery = { "ToaNha", "Nha", "TuongQuan" };

        /// <summary>Nhom cac manh di cung nhau: nhan vat + do nhan vat cam; nha + mai + cua.</summary>
        static string GroupKey(string n)
        {
            if (n.StartsWith("ThungCacTong") || n.StartsWith("ButTrongThung") || n.StartsWith("CayTrongThung") || n.StartsWith("GiotNuocMat")) return "Hope";
            if (n.StartsWith("SoNo")) return "ChuNo";
            if (n.StartsWith("VongChiaKhoa") || n.StartsWith("LoVong") || n.StartsWith("ChiaKhoa")) return "ChiaKhoa";
            if (n == "Mai" || n == "Cua" || n.StartsWith("Rem_") || n == "BienQuan" || n == "BienDong") return n == "Mai" ? "Nha" : "TuongQuan";
            int cut = n.IndexOf('_');
            return cut > 0 ? n.Substring(0, cut) : n;
        }

        // ---------------------------------------------------------------- hoat anh

        public void Tick(float dt)
        {
            clock += dt;
            if (page == null) return;
            pageT += dt;
            ApplyPage();
            foreach (PanelView v in shown)
            {
                v.t += dt;
                Animate(v);
            }
        }

        void ApplyPage()
        {
            if (page == null) return;
            float p = Mathf.Clamp01(Step(pageT) / PageSeconds);
            float e = 1f - (1f - p) * (1f - p);
            page.anchoredPosition = PagePos + new Vector2((1f - e) * 1350f, (1f - e) * -40f);
            page.localEulerAngles = new Vector3(0f, 0f, pageTilt + (1f - e) * 5f);
        }

        void Animate(PanelView v)
        {
            // khung roi xuong trang: giat 12 hinh/giay
            float p = Mathf.Clamp01(Step(v.t) / EnterSeconds);
            float e = 1f - Mathf.Pow(1f - p, 3f);
            v.card.anchoredPosition = v.cardRest + new Vector2(0f, (1f - e) * 46f);
            v.card.localEulerAngles = new Vector3(0f, 0f, v.tilt + (1f - e) * 7f);
            v.card.localScale = Vector3.one * (1.07f - 0.07f * e);

            // chu tuong thanh bat ra: 0 -> 1,25 -> 1 (ba nhip giat)
            if (v.sfx != null)
            {
                float st = Step(v.t) - SfxAt;
                float k = st < 0f ? 0f : st < 1f / StepFps ? 0.6f : st < 2f / StepFps ? 1.25f : 1f;
                v.sfx.rectTransform.localScale = Vector3.one * k;
            }

            // camera trong khung: day cham + truot ngang (muot), lop phong troi cham hon -> thi sai
            bool isCurrent = v == current;
            float kb = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(v.t / KenBurnsSeconds));
            if (!isCurrent) kb = Mathf.Min(kb, 1f);
            float zoom = 1f + KenBurnsZoom * kb;
            float pan = (v.tilt >= 0f ? -1f : 1f) * 14f * kb;
            v.art.localScale = Vector3.one * v.artScale * zoom;
            v.art.anchoredPosition = new Vector2(pan * v.artScale, 0f);

            // boil: moi nhom (nhan vat, do vat) lech nho, doi tu the 6 lan/giay
            int frame = Mathf.FloorToInt(clock * BoilFps);
            bool reroll = frame != v.boilFrame;
            v.boilFrame = frame;
            foreach (Group g in v.groups)
            {
                Vector2 off = Vector2.zero;
                float rot = 0f;
                if (g.parallax >= 1f)
                {
                    off = new Vector2(Hash(g.seed, frame, 1), Hash(g.seed, frame, 2)) * BoilPx;
                    rot = Hash(g.seed, frame, 3) * BoilDeg;
                }
                Vector2 para = new Vector2(-pan * (1f - g.parallax) * 1.6f, 0f);
                if (!reroll && g.parallax >= 1f) continue;
                for (int i = 0; i < g.parts.Count; i++)
                {
                    if (g.parts[i] == null) continue;
                    g.parts[i].anchoredPosition = g.pos[i] + off + para;
                    g.parts[i].localEulerAngles = new Vector3(0f, 0f, g.rot[i] + rot);
                }
            }
        }

        /// <summary>So gia ngau nhien on dinh trong [-1, 1] (khong dung Random de khong dong vao seed game).</summary>
        static float Hash(int seed, int frame, int axis)
        {
            unchecked
            {
                uint h = (uint)(seed * 73856093) ^ (uint)(frame * 19349663) ^ (uint)(axis * 83492791);
                h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
                return (h & 0xFFFF) / 32767.5f - 1f;
            }
        }

        // ---------------------------------------------------------------- giay

        static Texture2D grain;

        /// <summary>Van giay sinh bang Perlin + hat lam tam (khong can anh ve san).</summary>
        public static Texture2D GrainTexture
        {
            get
            {
                if (grain != null) return grain;
                const int N = 128;
                grain = new Texture2D(N, N, TextureFormat.RGBA32, false) { name = "VanGiay", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
                var px = new Color32[N * N];
                uint r = 2463534242u;
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                    {
                        // Perlin lap lai theo o N (tron 2 lop: soi giay dai + vet loang)
                        float u = x / (float)N, w = y / (float)N;
                        float fiber = Tile(u * 16f, w * 4f, 16f, 4f);
                        float blot = Tile(u * 4f, w * 4f, 4f, 4f);
                        r ^= r << 13; r ^= r >> 17; r ^= r << 5;
                        float speck = (r & 255) / 255f;
                        float a = Mathf.Clamp01(fiber * 0.55f + blot * 0.35f + (speck > 0.93f ? 0.5f : speck * 0.15f));
                        px[y * N + x] = new Color32(0, 0, 0, (byte)(a * 255f));
                    }
                grain.SetPixels32(px);
                grain.Apply();
                return grain;
            }
        }

        static float Tile(float x, float y, float px, float py)
        {
            // noi suy 4 goc de lap lai lien mach
            float fx = x / px, fy = y / py;
            float a = Mathf.PerlinNoise(x, y), b = Mathf.PerlinNoise(x - px, y);
            float c = Mathf.PerlinNoise(x, y - py), d = Mathf.PerlinNoise(x - px, y - py);
            return Mathf.Clamp01(Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy));
        }

        static void Grain(RectTransform parent, float alpha, float tiles)
        {
            RectTransform rt = UiKit.Stretch(parent, "VanGiay");
            var img = rt.gameObject.AddComponent<RawImage>();
            img.texture = GrainTexture;
            img.color = new Color(1f, 1f, 1f, alpha);
            Vector2 size = parent.rect.size;
            if (size.x <= 0f) size = new Vector2(1280f, 720f);
            img.uvRect = new Rect(0f, 0f, size.x / 128f / tiles * 0.5f, size.y / 128f / tiles * 0.5f);
            img.raycastTarget = false;
        }

        static Image Shape(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color, Sprite sprite)
        {
            RectTransform rt = UiKit.Box(parent, name, anchor, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            if (sprite != null) img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static Color Shade(Color c, float k) { return new Color(c.r * k, c.g * k, c.b * k, c.a); }
    }
}
