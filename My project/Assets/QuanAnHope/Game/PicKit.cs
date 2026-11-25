using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Bo "ngu phap hinh" cho huong dan (yeu cau 22/11/2026, "Phong cach huong dan bang hinh.md"):
    /// - Khung kem #FFF3DC vien muc #2B2321, bo goc giong nhau o moi the; chu chu thich nam trong dai rieng ben duoi (<= 4 tu).
    /// - Huy hieu so tron mau mu tat o goc trai tren (trinh tu); mui ten day dau dac (hanh dong); chevron "›" giua khung (buoc sau).
    /// - DUNG: huy hieu tron mint + ✓ muc; SAI: hinh bat giac coral + ✗ muc; CANH BAO: tam giac mu tat + "!" (khac nhau ca hinh lan mau).
    /// - Ban tay ma: gang kem vien muc, moi cu chi mot tin hieu o dau ngon (cham: song lan; giu: vong day dan; keo: duong net dut).
    /// Moi chu va ky hieu mau #2B2321 (tuong phan 14:1 tren kem).
    /// </summary>
    public static class PicKit
    {
        public static readonly Color Paper = UiKit.Cream;
        public static readonly Color Ink = UiKit.Ink;
        static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);

        // ---------------- hinh co ban ----------------

        static Sprite triangle, ring, dashRing, octagon, glove;

        /// <summary>Tam giac deu dinh huong len, ruot trang (to mau bang Image.color).</summary>
        public static Sprite Triangle { get { return triangle != null ? triangle : (triangle = MakeSprite("Tri", 256, (x, y) => TriangleSdf(x, y))); } }

        /// <summary>Vong tron day (vong song cua cu chi cham, vong tien trinh cua cu chi giu).</summary>
        public static Sprite Ring { get { return ring != null ? ring : (ring = MakeSprite("Ring", 256, (x, y) => Mathf.Abs(new Vector2(x, y).magnitude - 0.8f) - 0.12f)); } }

        /// <summary>Vong net dut (bong mo vi tri dich).</summary>
        public static Sprite DashRing
        {
            get
            {
                if (dashRing != null) return dashRing;
                dashRing = MakeSprite("DashRing", 256, (x, y) =>
                {
                    float d = Mathf.Abs(new Vector2(x, y).magnitude - 0.85f) - 0.07f;
                    float a = Mathf.Atan2(y, x) / (2f * Mathf.PI) * 16f;
                    float gap = Mathf.Abs(a - Mathf.Round(a)) < 0.22f ? 1f : -1f;
                    return gap > 0 ? 1f : d;
                });
                return dashRing;
            }
        }

        /// <summary>Bat giac deu (huy hieu SAI, ISO 3864: do = cam).</summary>
        public static Sprite Octagon
        {
            get
            {
                if (octagon != null) return octagon;
                octagon = MakeSprite("Octagon", 256, (x, y) =>
                {
                    float d = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    float e = (Mathf.Abs(x) + Mathf.Abs(y)) / 1.4142f;
                    return Mathf.Max(d, e) - 0.92f;
                });
                return octagon;
            }
        }

        delegate float Sdf(float x, float y);

        static readonly Vector2[] Tri = { new Vector2(0f, 0.88f), new Vector2(-0.92f, -0.72f), new Vector2(0.92f, -0.72f) };

        /// <summary>Khoang cach co dau toi tam giac (dinh nguoc chieu kim dong ho): max cac nua mat phang.</summary>
        static float TriangleSdf(float x, float y)
        {
            var p = new Vector2(x, y);
            float d = float.MinValue;
            for (int i = 0; i < 3; i++)
            {
                Vector2 a = Tri[i], b = Tri[(i + 1) % 3], e = b - a;
                Vector2 n = new Vector2(e.y, -e.x).normalized;
                d = Mathf.Max(d, Vector2.Dot(n, p - a));
            }
            return d;
        }

        /// <summary>Sprite tu ham khoang cach co dau (am = ben trong), ruot trang co vien muc 10%.</summary>
        static Sprite MakeSprite(string name, int size, Sdf sdf)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Pic_" + name, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            float px1 = 2f / size;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    float d = sdf(u, v);
                    float alpha = Mathf.Clamp01(0.5f - d / px1);
                    float fill = Mathf.Clamp01(0.5f - (d + 0.1f) / px1);
                    Color c = Color.Lerp(Ink, Color.white, fill);
                    c.a = alpha;
                    px[y * size + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s.name = "Pic_" + name;
            return s;
        }

        static Image Shape(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size, Color color, float angle = 0f)
        {
            RectTransform rt = UiKit.Box(parent, name, Mid, pos, size);
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        // ---------------- khung ----------------

        /// <summary>
        /// The tranh kieu hop diorama giay (paper-ui-pickit.md muc 2), tu sau ra truoc: bia carton #C9A47E lech (4, 6); khung kem vien muc
        /// khoet cua so (nen nhat theo nhom + cham tuong + dai san + bong trong); hat giay 7%; dai chu giay xe + 2 mieng bang dinh mu tat 55%
        /// (chu 20 px); huy hieu so (dia mu tat ban kinh 17, dinh ghim). <paramref name="number"/> = 0: khong huy hieu; &lt; 0: huy hieu "?" tim
        /// (the khai niem). Tra ve vung hinh (con cua cua so). Kich thuoc chuan: the 360 px rong (ti le k = rong / 360).
        /// </summary>
        public static RectTransform Frame(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, int number, string caption,
            Color? tint = null, Color? windowBg = null, Color? dotColor = null)
        {
            float k = Mathf.Clamp(size.x / 360f, 0.55f, 2f);
            RectTransform root = UiKit.Box(parent, name, anchor, pos, size);
            var pop = root.gameObject.AddComponent<CardPop>();

            // 1 bia carton phia sau
            Image carton = UiKit.Panel(root, "BiaCarton", Mid, new Vector2(4f, -6f) * k, size, PaperKit.Carton, false);
            carton.raycastTarget = false;
            carton.gameObject.AddComponent<PaperJitter>();
            pop.AddLayer(carton.gameObject);

            // 3 khung kem khoet cua so (vien muc tu sprite bo goc)
            Image card = UiKit.Panel(root, "Khung", Mid, Vector2.zero, size, tint ?? Paper, false);
            card.raycastTarget = false;
            pop.AddLayer(card.gameObject);
            float inset = 10f * k;
            float captionH = string.IsNullOrEmpty(caption) ? 0f : 34f * k;
            float winH = size.y - inset * 2f - captionH * 0.55f;
            RectTransform win = UiKit.Box(card.transform, "CuaSo", new Vector2(0.5f, 1f), new Vector2(0f, -inset), new Vector2(size.x - inset * 2f, winH));
            win.pivot = new Vector2(0.5f, 1f);
            Image wbg = win.gameObject.AddComponent<Image>();
            wbg.sprite = UiKit.Rounded;
            wbg.type = Image.Type.Sliced;
            wbg.color = windowBg ?? UiKit.Hex("#F3DFB8");
            wbg.raycastTarget = false;
            // 24/11/2026 (anh 2): cham tuong / dai san / bong trong / hinh la khoi chu nhat -> goc vuong lo ra ngoai cua so bo tron;
            // mat na theo chinh hinh bo tron cua so
            win.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            // 0 nen cua so: cham tuong + dai san
            RectTransform dotsRt = UiKit.Stretch(win, "ChamTuong");
            dotsRt.offsetMin = new Vector2(5f, 5f);
            dotsRt.offsetMax = new Vector2(-5f, -5f);
            Image dots = dotsRt.gameObject.AddComponent<Image>();
            dots.sprite = PaperKit.Dots;
            dots.type = Image.Type.Tiled;
            dots.pixelsPerUnitMultiplier = 48f / Mathf.Max(8f, 22f * k);
            dots.color = dotColor ?? UiKit.Hex("#E9CE9E");
            dots.raycastTarget = false;
            RectTransform floor = UiKit.Box(win, "San", new Vector2(0.5f, 0f), new Vector2(0f, 5f), new Vector2(size.x - inset * 2f - 10f, winH * 0.24f));
            floor.pivot = new Vector2(0.5f, 0f);
            Image fl = floor.gameObject.AddComponent<Image>();
            Color fc = dotColor ?? UiKit.Hex("#E9CE9E");
            fl.color = new Color(fc.r, fc.g, fc.b, 0.55f);
            fl.raycastTarget = false;
            RectTransform shadeRt = UiKit.Stretch(win, "BongTrong");
            Image shade = shadeRt.gameObject.AddComponent<Image>();
            shade.sprite = PaperKit.InnerShade;
            shade.type = Image.Type.Sliced;
            shade.pixelsPerUnitMultiplier = 1f / Mathf.Max(0.5f, k);
            shade.raycastTarget = false;
            RectTransform pic = UiKit.Stretch(win, "Hinh");
            pic.offsetMin = new Vector2(4f, 4f);
            pic.offsetMax = new Vector2(-4f, -4f);

            // hat giay phu toan the (7%)
            RectTransform grainRt = UiKit.Stretch(root, "HatGiay");
            Image grain = grainRt.gameObject.AddComponent<Image>();
            grain.sprite = PaperKit.Grain;
            grain.type = Image.Type.Tiled;
            grain.color = new Color(0.17f, 0.12f, 0.08f, 0.07f);
            grain.raycastTarget = false;

            // 4 dai chu giay xe + bang dinh
            if (captionH > 0f)
            {
                float stripW = size.x * 0.76f;
                RectTransform strip = UiKit.Box(root, "DaiChu", new Vector2(0.5f, 0f), new Vector2(0f, inset * 0.6f), new Vector2(stripW, captionH));
                strip.pivot = new Vector2(0.5f, 0f);
                Image paper = strip.gameObject.AddComponent<Image>();
                paper.sprite = PaperKit.Torn;
                paper.type = Image.Type.Sliced;
                paper.color = PaperKit.TornPaper;
                paper.raycastTarget = false;
                PaperKit.AddShadow(paper, k * 0.6f, 0.5f);
                strip.gameObject.AddComponent<PaperJitter>();
                for (int s = -1; s <= 1; s += 2)
                {
                    RectTransform tp = UiKit.Box(strip, "BangDinh", new Vector2(0.5f, 0.5f), new Vector2(s * (stripW * 0.5f - 6f * k), captionH * 0.42f), new Vector2(30f * k, 13f * k));
                    tp.localRotation = Quaternion.Euler(0f, 0f, s * -14f);
                    Image ti = tp.gameObject.AddComponent<Image>();
                    ti.sprite = PaperKit.Tape;
                    ti.color = PaperKit.TapeColor;
                    ti.raycastTarget = false;
                }
                TextMeshProUGUI t = UiKit.Text(strip, caption, Mathf.Max(UiKit.MinFontSize, 20f * k), TextAlignmentOptions.Center, 6f);
                t.enableAutoSizing = false;
                t.textWrappingMode = TextWrappingModes.NoWrap;
                t.overflowMode = TextOverflowModes.Overflow;
                pop.AddLayer(strip.gameObject);
            }

            // huy hieu so (dia mu tat + dinh ghim) hoac "?" tim cho the khai niem
            if (number != 0)
            {
                float r = 17f * k;
                Image b = Badge(root, new Vector2(-size.x * 0.5f + r * 1.05f, size.y * 0.5f - r * 1.05f), number > 0 ? number.ToString() : "?",
                    number > 0 ? UiKit.Mustard : UiKit.Lilac, r * 2f);
                Image pin = Shape(b.transform, "DinhGhim", UiKit.Circle, new Vector2(0f, r * 0.95f), new Vector2(7f * k, 7f * k), PaperKit.PinGrey);
                PaperKit.AddShadow(b, k * 0.5f, 0.3f);
                pop.AddLayer(b.gameObject);
            }
            return pic;
        }

        /// <summary>Huy hieu so tron (mu tat) - trinh tu.</summary>
        public static Image Badge(Transform parent, Vector2 pos, string text, Color color, float size = 34f)
        {
            Image c = Shape(parent, "So_" + text, UiKit.Circle, pos, new Vector2(size, size), color);
            UiKit.Text(c.transform, text, size * 0.62f, TextAlignmentOptions.Center, 0f);
            return c;
        }

        // ---------------- noi dung ----------------

        /// <summary>Do vat dang sticker: vien trang 3 px quanh net muc + bong nau lech xuong (lop cang gan bong cang xa).</summary>
        public static RawImage Icon(Transform parent, Texture tex, Vector2 pos, float size, string name = "Icon")
        {
            RectTransform rt = UiKit.Box(parent, name, Mid, pos, new Vector2(size, size));
            var img = rt.gameObject.AddComponent<RawImage>();
            img.texture = tex != null ? PaperKit.Sticker(tex) : null;
            // anh chup model co vien trong rong: cat bot 11% moi canh de hinh to ra (paper-ui-pickit.md: hinh >= 60% cua so)
            if (img.texture is Texture2D) img.uvRect = new Rect(0.11f, 0.11f, 0.78f, 0.78f);
            img.raycastTarget = false;
            if (tex == null) img.color = new Color(0f, 0f, 0f, 0f);
            else PaperKit.AddShadow(img, size / 90f, 1f);
            return img;
        }

        /// <summary>
        /// Vien thuoc chu (nhan ngan, so: "×2", "30s", "60G"): rong co dinh theo cot, KHONG xuong dong, khong co chu; chu >= 14 px
        /// (paper-ui-pickit.md muc 3). Co nen thi them vien trang sticker + bong.
        /// </summary>
        public static TextMeshProUGUI Tag(Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, Color? bg = null)
        {
            float fs = Mathf.Max(UiKit.MinFontSize, fontSize);
            TextMeshProUGUI t;
            if (bg.HasValue)
            {
                Image rim = UiKit.Panel(parent, "NhanVien", Mid, pos, size + new Vector2(6f, 6f), PaperKit.StickerWhite, false);
                rim.raycastTarget = false;
                rim.sprite = UiKit.Rounded;
                PaperKit.AddShadow(rim, size.y / 40f, 0.2f);
                Image p = UiKit.Panel(parent, "Nhan", Mid, pos, size, bg.Value, false);
                p.raycastTarget = false;
                t = UiKit.Text(p.transform, text, fs, TextAlignmentOptions.Center, 2f);
            }
            else t = UiKit.Label(parent, "Nhan", Mid, pos, size, text, fs);
            t.enableAutoSizing = false;
            t.fontSize = fs;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        /// <summary>
        /// Mot cot "hinh + nhan" (paper-ui-pickit.md muc 3): nhan nam duoi hinh, can giua theo tam hinh, cach 16 px (theo ti le).
        /// Nhom ten "Cot" (test kiem lech tam &lt;= 2 px).
        /// </summary>
        public static RectTransform Column(Transform parent, Vector2 center, Texture icon, float iconSize, string label, Vector2 pillSize, float fontSize,
            Color? pillColor = null, float gap = 16f)
        {
            RectTransform col = UiKit.Box(parent, "Cot", Mid, center, Vector2.zero);
            if (icon != null) Icon(col, icon, new Vector2(0f, pillSize.y * 0.5f + gap * 0.5f), iconSize, "Hinh");
            Tag(col, new Vector2(0f, -iconSize * 0.5f - gap * 0.5f), pillSize, label, fontSize, pillColor ?? UiKit.Cream);
            return col;
        }

        /// <summary>Mui ten day dau dac vien muc (di chuyen / bo vao). Mau mu tat mac dinh.</summary>
        public static RectTransform Arrow(Transform parent, Vector2 from, Vector2 to, float thickness = 14f, Color? color = null)
        {
            Color c = color ?? UiKit.Mustard;
            Vector2 d = to - from;
            float len = d.magnitude, angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            RectTransform root = UiKit.Box(parent, "MuiTen", Mid, from, Vector2.zero);
            root.localRotation = Quaternion.Euler(0f, 0f, angle);
            float head = thickness * 2.4f;
            float shaft = Mathf.Max(0f, len - head * 0.8f);
            // vien muc: hinh muc to hon nam duoi
            Image so = Shape(root, "ThanVien", null, new Vector2(shaft * 0.5f, 0f), new Vector2(shaft + 4f, thickness + 6f), Ink);
            Image ho = Shape(root, "DauVien", Triangle, new Vector2(len - head * 0.5f, 0f), new Vector2(head + 8f, head + 8f), Ink, -90f);
            Image s = Shape(root, "Than", null, new Vector2(shaft * 0.5f, 0f), new Vector2(shaft, thickness), c);
            Image h = Shape(root, "Dau", Triangle, new Vector2(len - head * 0.5f, 0f), new Vector2(head, head), c, -90f);
            so.sprite = null; s.sprite = null;
            return root;
        }

        /// <summary>Chevron "›" giua hai khung (buoc tiep theo): mo hon mui ten hanh dong.</summary>
        public static TextMeshProUGUI Chevron(Transform parent, Vector2 pos, float size = 40f)
        {
            TextMeshProUGUI t = UiKit.Label(parent, "Chevron", Mid, pos, new Vector2(size, size * 1.4f), "›", size);
            t.color = new Color(Ink.r, Ink.g, Ink.b, 0.5f);
            return t;
        }

        /// <summary>Bong mo net dut tai dich ("bo vao day").</summary>
        public static Image Ghost(Transform parent, Vector2 pos, float size)
        {
            Image g = Shape(parent, "BongMo", DashRing, pos, new Vector2(size, size), new Color(1f, 1f, 1f, 0.9f));
            return g;
        }

        /// <summary>
        /// Dau DUNG (huy hieu tron mint, dau tick muc) hoac SAI (bat giac coral, dau nhan muc): khac nhau ca hinh lan mau, doc duoc
        /// o thang xam. Ve bang thanh (font Baloo 2 khong co glyph tick / nhan).
        /// </summary>
        public static Image Mark(Transform parent, Vector2 pos, float size, bool ok)
        {
            Image b = Shape(parent, ok ? "Dung" : "Sai", ok ? UiKit.Circle : Octagon, pos, new Vector2(size, size), ok ? UiKit.Mint : UiKit.Coral);
            float t = Mathf.Max(3f, size * 0.12f);
            if (ok)
            {
                Shape(b.transform, "Tick1", null, new Vector2(-size * 0.11f, -size * 0.06f), new Vector2(size * 0.26f, t), Ink, -45f);
                Shape(b.transform, "Tick2", null, new Vector2(size * 0.08f, size * 0.03f), new Vector2(size * 0.44f, t), Ink, 50f);
            }
            else
            {
                Shape(b.transform, "Nhan1", null, Vector2.zero, new Vector2(size * 0.5f, t), Ink, 45f);
                Shape(b.transform, "Nhan2", null, Vector2.zero, new Vector2(size * 0.5f, t), Ink, -45f);
            }
            return b;
        }

        /// <summary>Tam giac canh bao mu tat + "!".</summary>
        public static Image Warn(Transform parent, Vector2 pos, float size)
        {
            Image b = Shape(parent, "CanhBao", Triangle, pos, new Vector2(size, size), UiKit.Mustard);
            TextMeshProUGUI t = UiKit.Text(b.transform, "!", size * 0.5f, TextAlignmentOptions.Center, 0f);
            t.rectTransform.offsetMin = new Vector2(0f, -size * 0.12f);
            return b;
        }

        /// <summary>Hinh tron mau (thung, nut, khach...) co chu.</summary>
        public static Image Dot(Transform parent, Vector2 pos, float size, Color color, string text = null)
        {
            Image b = Shape(parent, "Cham", UiKit.Circle, pos, new Vector2(size, size), color);
            if (text != null) UiKit.Text(b.transform, text, size * 0.45f, TextAlignmentOptions.Center, 2f);
            return b;
        }

        /// <summary>O chu nhat bo goc (thung, bang, the) co chu.</summary>
        public static Image Block(Transform parent, Vector2 pos, Vector2 size, Color color, string text = null, float fontSize = 18f)
        {
            Image b = UiKit.Panel(parent, "Khoi", Mid, pos, size, color, false);
            b.raycastTarget = false;
            if (text != null) UiKit.Text(b.transform, text, fontSize, TextAlignmentOptions.Center, 3f);
            return b;
        }

        /// <summary>Mat khach / nguyen lieu 2D (FaceArt) trong vong tron.</summary>
        public static RawImage Face(Transform parent, Texture2D face, Vector2 pos, float size, Color skin)
        {
            Dot(parent, pos, size, skin);
            return Icon(parent, face, pos, size * 0.92f, "Mat");
        }

        /// <summary>Thanh ngang day mot phan (kien nhan, tien do).</summary>
        public static void Bar(Transform parent, Vector2 pos, Vector2 size, float fill, Color color)
        {
            Image back = UiKit.Panel(parent, "Thanh", Mid, pos, size, UiKit.Hex("#E3CFA6"), false);
            back.raycastTarget = false;
            RectTransform f = UiKit.Box(back.transform, "Day", new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2((size.x - 6f) * Mathf.Clamp01(fill), size.y - 6f));
            f.pivot = new Vector2(0f, 0.5f);
            var img = f.gameObject.AddComponent<Image>();
            img.sprite = UiKit.Rounded;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
        }

        // ---------------- ban tay ma ----------------

        /// <summary>Gang tay kem vien muc (Art Bible: tay ong cao su gang trang), ngon tro chi len; goc la dau ngon tro.</summary>
        public static GhostHand Hand(Transform parent, Vector2 tip, float size, GhostHand.Gesture gesture, Vector2? dragTo = null)
        {
            RectTransform rt = UiKit.Box(parent, "BanTayMa", Mid, tip, new Vector2(size, size));
            var h = rt.gameObject.AddComponent<GhostHand>();
            h.Build(size, gesture, tip, dragTo ?? tip);
            return h;
        }
    }

    /// <summary>
    /// Ban tay ma lap lai mot cu chi (nhip Material: hien 225 ms, dung 200 ms, cu chi, giu ket qua 400 ms, an 195 ms, nghi 450 ms).
    /// Dung thoi gian khong phu thuoc timeScale (bang Tab, tam dung).
    /// </summary>
    public class GhostHand : MonoBehaviour
    {
        public enum Gesture { Tap, Hold, Drag, Turn }

        Gesture gesture;
        Vector2 from, to;
        RectTransform glove;
        Image signal;
        CanvasGroup group;
        float t;
        float size;

        const float Appear = 0.225f, Pause = 0.2f, Result = 0.4f, Hide = 0.195f, Rest = 0.45f;

        float GestureSeconds
        {
            get
            {
                switch (gesture)
                {
                    case Gesture.Hold: return 0.9f;
                    case Gesture.Drag: return 0.7f;
                    case Gesture.Turn: return 0.6f;
                    default: return 0.4f;
                }
            }
        }

        public float Cycle { get { return Appear + Pause + GestureSeconds + Result + Hide + Rest; } }

        public void Build(float s, Gesture g, Vector2 a, Vector2 b)
        {
            size = s;
            gesture = g;
            from = a;
            to = b;
            group = gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var rt = (RectTransform)transform;

            // tin hieu o dau ngon
            RectTransform sig = UiKit.Box(rt, "TinHieu", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(s * 0.7f, s * 0.7f));
            signal = sig.gameObject.AddComponent<Image>();
            signal.sprite = PicKit.Ring;
            signal.color = new Color(1f, 1f, 1f, 0f);
            signal.raycastTarget = false;
            if (g == Gesture.Hold)
            {
                signal.type = Image.Type.Filled;
                signal.fillMethod = Image.FillMethod.Radial360;
                signal.fillOrigin = 2;
            }

            // gang: long ban tay tron + ngon tro dung + ngon cai; dau ngon tro o goc (0,0)
            glove = UiKit.Box(rt, "Gang", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(s, s));
            Color cream = UiKit.Hex("#FFF8EA");
            Part(glove, "Ngon", new Vector2(0f, -s * 0.22f), new Vector2(s * 0.24f, s * 0.5f), cream, 0f);
            Part(glove, "NgonCai", new Vector2(-s * 0.2f, -s * 0.52f), new Vector2(s * 0.2f, s * 0.34f), cream, 40f);
            Part(glove, "Long", new Vector2(s * 0.04f, -s * 0.62f), new Vector2(s * 0.46f, s * 0.42f), cream, 0f);
            Part(glove, "Co", new Vector2(s * 0.06f, -s * 0.88f), new Vector2(s * 0.34f, s * 0.14f), UiKit.Hex("#F6E6C6"), 0f);
            t = 0f;
            Apply();
        }

        static void Part(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color, float angle)
        {
            RectTransform rt = UiKit.Box(parent, name, new Vector2(0.5f, 0.5f), pos, size);
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiKit.Rounded;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 2.2f;
            img.color = color;
            img.raycastTarget = false;
        }

        void Update()
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            if (t > Cycle) t -= Cycle;
            Apply();
        }

        static float Decel(float x) { x = Mathf.Clamp01(x); return 1f - (1f - x) * (1f - x); }

        void Apply()
        {
            // paper-ui-pickit.md muc 4: stop-motion 12 hinh / giay (giu moi tu the 2 khung) o muc Day du; Giam / Tat thi muot
            float a = PaperKit.Motion == 0 ? Mathf.Floor(t * PaperKit.StepFps) / PaperKit.StepFps : t;
            float alpha = 0.92f, sig = 0f, press = 0f;
            Vector2 at = from;
            float g = GestureSeconds;
            if (a < Appear) { alpha *= Decel(a / Appear); }
            else if ((a -= Appear) < Pause) { }
            else if ((a -= Pause) < g)
            {
                float k = a / g;
                switch (gesture)
                {
                    case Gesture.Tap: press = k < 0.25f ? k / 0.25f : 1f - (k - 0.25f) / 0.75f; sig = k; break;
                    case Gesture.Hold: press = 1f; sig = k; break;
                    case Gesture.Drag: press = 1f; at = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k)); sig = 0f; break;
                    case Gesture.Turn: press = 1f; sig = k; break;
                }
            }
            else if ((a -= g) < Result) { at = gesture == Gesture.Drag ? to : from; sig = gesture == Gesture.Hold ? 1f : 0f; }
            else if ((a -= Result) < Hide) { at = gesture == Gesture.Drag ? to : from; alpha *= 1f - a / Hide; }
            else alpha = 0f;

            ((RectTransform)transform).anchoredPosition = at;
            glove.localScale = Vector3.one * (1f - 0.12f * press);
            glove.localRotation = Quaternion.Euler(0f, 0f, gesture == Gesture.Turn ? -60f * sig : 0f);
            group.alpha = alpha;
            switch (gesture)
            {
                case Gesture.Tap:
                    signal.rectTransform.localScale = Vector3.one * (0.4f + sig * 1.1f);
                    signal.color = new Color(UiKit.Mustard.r, UiKit.Mustard.g, UiKit.Mustard.b, sig > 0f ? 1f - sig : 0f);
                    break;
                case Gesture.Hold:
                    signal.fillAmount = sig;
                    signal.color = UiKit.Mint;
                    break;
                default:
                    signal.color = new Color(1f, 1f, 1f, 0f);
                    break;
            }
        }

        /// <summary>Test: tua toi thoi diem trong vong lap.</summary>
        public void Seek(float seconds)
        {
            t = Mathf.Repeat(seconds, Cycle);
            Apply();
        }
    }
}
