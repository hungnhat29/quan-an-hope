using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Tranh huong dan kieu hop diorama giay (docs/sprint-10/ui/paper-ui-pickit.md, yeu cau 24/11/2026): hoa tiet sinh bang code
    /// (cham tuong, hat giay, giay xe, bang dinh, bong trong cua so), sticker vien trang cho icon, rung vien giay va the bat ra
    /// theo nhip stop-motion 12 hinh / giay. Muc hieu ung theo Cai dat "Hieu ung giay" (<see cref="Motion"/>).
    /// </summary>
    public static class PaperKit
    {
        /// <summary>0 = Day du, 1 = Giam (tat rung vien, tay ma muot), 2 = Tat (khong bat the, khong rung).</summary>
        public static int Motion;
        public static readonly string[] MotionNames = { "Đầy đủ", "Giảm", "Tắt" };

        /// <summary>Nhip stop-motion: 12 hinh / giay (moi tu the giu 2 khung o 24 khung / giay).</summary>
        public const float StepFps = 12f;

        public static readonly Color Carton = UiKit.Hex("#C9A47E");
        public static readonly Color StickerWhite = UiKit.Hex("#FFFDF6");
        public static readonly Color ShadowInk = new Color(0x2B / 255f, 0x1A / 255f, 0x10 / 255f, 0.3f);
        public static readonly Color TornPaper = UiKit.Hex("#FBF5E6");
        public static readonly Color TapeColor = new Color(UiKit.Mustard.r, UiKit.Mustard.g, UiKit.Mustard.b, 0.55f);
        public static readonly Color PinGrey = UiKit.Hex("#9A928C");

        static Sprite dots, grain, torn, tape, shade;

        static Sprite Make(string name, int w, int h, System.Func<int, int, Color32> px, Vector4 border = default(Vector4), FilterMode filter = FilterMode.Bilinear,
            TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "Paper_" + name, wrapMode = wrap, filterMode = filter };
            var data = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) data[y * w + x] = px(x, y);
            tex.SetPixels32(data);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s.name = "Paper_" + name;
            return s;
        }

        /// <summary>Cham tuong (tile 48 px, mot cham tron giua), to mau bang Image.color.</summary>
        public static Sprite Dots
        {
            get
            {
                if (dots != null) return dots;
                const int n = 48;
                dots = Make("Dots", n, n, (x, y) =>
                {
                    float d = new Vector2(x + 0.5f - n * 0.5f, y + 0.5f - n * 0.5f).magnitude;
                    byte a = (byte)(255 * Mathf.Clamp01(6.5f - d));
                    return new Color32(255, 255, 255, a);
                }, default(Vector4), FilterMode.Bilinear, TextureWrapMode.Repeat);
                return dots;
            }
        }

        /// <summary>Hat giay (tile 256 px, nhieu xam).</summary>
        public static Sprite Grain
        {
            get
            {
                if (grain != null) return grain;
                var rnd = new System.Random(20261124);
                grain = Make("Grain", 256, 256, (x, y) =>
                {
                    int v = rnd.Next(0, 256);
                    return new Color32((byte)v, (byte)v, (byte)v, 255);
                }, default(Vector4), FilterMode.Point, TextureWrapMode.Repeat);
                return grain;
            }
        }

        /// <summary>Dai giay xe: mep tren / duoi rang cua nho, ruot trang (to mau bang Image.color).</summary>
        public static Sprite Torn
        {
            get
            {
                if (torn != null) return torn;
                const int w = 512, h = 72;
                var top = new float[w];
                var bottom = new float[w];
                var rnd = new System.Random(7);
                float a = 0f, b = 0f;
                for (int x = 0; x < w; x++)
                {
                    a = Mathf.Clamp(a + (float)(rnd.NextDouble() - 0.5) * 2.4f, 0f, 5f);
                    b = Mathf.Clamp(b + (float)(rnd.NextDouble() - 0.5) * 2.4f, 0f, 5f);
                    top[x] = h - 3f - a;
                    bottom[x] = 3f + b;
                }
                torn = Make("Torn", w, h, (x, y) =>
                {
                    float inside = Mathf.Min(y + 0.5f - bottom[x], top[x] - (y + 0.5f));
                    byte al = (byte)(255 * Mathf.Clamp01(inside + 0.5f));
                    return new Color32(255, 255, 255, al);
                }, new Vector4(24, 0, 24, 0));
                return torn;
            }
        }

        /// <summary>Mieng bang dinh: hinh chu nhat hai dau rang cua.</summary>
        public static Sprite Tape
        {
            get
            {
                if (tape != null) return tape;
                const int w = 96, h = 40;
                tape = Make("Tape", w, h, (x, y) =>
                {
                    float teeth = 3f + 2.5f * Mathf.Abs(Mathf.Sin(y * 0.9f));
                    float inside = Mathf.Min(x + 0.5f - teeth, w - teeth - (x + 0.5f));
                    return new Color32(255, 255, 255, (byte)(255 * Mathf.Clamp01(inside + 0.5f)));
                });
                return tape;
            }
        }

        static Sprite glow;

        /// <summary>Quang sang mem (tron, tat dan ra mep) - dia phat sang.</summary>
        public static Sprite Glow
        {
            get
            {
                if (glow != null) return glow;
                const int n = 128;
                glow = Make("Glow", n, n, (x, y) =>
                {
                    float d = new Vector2(x + 0.5f - n * 0.5f, y + 0.5f - n * 0.5f).magnitude / (n * 0.5f);
                    float a = Mathf.Clamp01(1f - d);
                    return new Color32(255, 255, 255, (byte)(255 * a * a));
                });
                return glow;
            }
        }

        /// <summary>Bong trong cua so: vien toi mo dan vao trong (9-slice 20 px), ruot trong suot.</summary>
        public static Sprite InnerShade
        {
            get
            {
                if (shade != null) return shade;
                const int n = 64, edge = 20;
                shade = Make("Shade", n, n, (x, y) =>
                {
                    float d = Mathf.Min(Mathf.Min(x + 0.5f, n - x - 0.5f), Mathf.Min(y + 0.5f, n - y - 0.5f));
                    float k = 1f - Mathf.Clamp01(d / edge);
                    return new Color32(0x2B, 0x1A, 0x10, (byte)(255 * 0.28f * k * k));
                }, new Vector4(edge + 2, edge + 2, edge + 2, edge + 2));
                return shade;
            }
        }

        // ---------------- sticker: vien trang quanh hinh icon ----------------

        static readonly Dictionary<Texture, Texture2D> stickers = new Dictionary<Texture, Texture2D>();

        /// <summary>
        /// Ban sticker cua icon: vien trang #FFFDF6 bao ngoai net muc (dan no alpha theo hang roi cot, ban kinh theo kich thuoc anh,
        /// ~3 px khi hien 80-100 px). Anh khong doc duoc thi tra ve chinh no. Co bo nho dem.
        /// </summary>
        public static Texture Sticker(Texture src)
        {
            var t = src as Texture2D;
            if (t == null) return src;
            Texture2D cached;
            if (stickers.TryGetValue(t, out cached) && cached != null) return cached;
            if (!t.isReadable) return src;
            int w = t.width, h = t.height, r = Mathf.Max(2, w / 40);
            Color32[] px = t.GetPixels32();
            var a = new byte[w * h];
            for (int i = 0; i < a.Length; i++) a[i] = px[i].a;
            var row = new byte[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    byte m = 0;
                    for (int k = Mathf.Max(0, x - r); k <= Mathf.Min(w - 1, x + r); k++) if (a[y * w + k] > m) m = a[y * w + k];
                    row[y * w + x] = m;
                }
            var dil = new byte[w * h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    byte m = 0;
                    for (int k = Mathf.Max(0, y - r); k <= Mathf.Min(h - 1, y + r); k++) if (row[k * w + x] > m) m = row[k * w + x];
                    dil[y * w + x] = m;
                }
            var outPx = new Color32[w * h];
            Color32 white = StickerWhite;
            for (int i = 0; i < outPx.Length; i++)
            {
                float fa = px[i].a / 255f, da = dil[i] / 255f;
                float outA = fa + da * (1f - fa);
                if (outA <= 0f) { outPx[i] = new Color32(0, 0, 0, 0); continue; }
                float wk = da * (1f - fa) / outA, ok = fa / outA;
                outPx[i] = new Color32((byte)(px[i].r * ok + white.r * wk), (byte)(px[i].g * ok + white.g * wk), (byte)(px[i].b * ok + white.b * wk), (byte)(outA * 255f));
            }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = t.name + "_Sticker", wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels32(outPx);
            tex.Apply(false, true);
            stickers[t] = tex;
            return tex;
        }

        /// <summary>Bong do vat: lech xuong phai, mau nau toi 22-40%.</summary>
        public static Shadow AddShadow(Graphic gfx, float scale, float depth = 1f)
        {
            var s = gfx.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(ShadowInk.r, ShadowInk.g, ShadowInk.b, Mathf.Lerp(0.22f, 0.4f, Mathf.Clamp01(depth * 0.5f)));
            s.effectDistance = new Vector2(3f, -5f) * Mathf.Max(0.6f, scale) * Mathf.Lerp(0.8f, 1.6f, Mathf.Clamp01(depth * 0.5f));
            s.useGraphicAlpha = true;
            return s;
        }
    }

    /// <summary>Rung vien giay: lech +-1 px, doi 8 lan / giay, luan phien 3 bo lech (chi o muc Day du).</summary>
    public class PaperJitter : MonoBehaviour
    {
        static readonly Vector2[] Offsets = { new Vector2(1f, 0f), new Vector2(0f, -1f), new Vector2(-1f, 1f) };
        Vector2 basePos;
        int phase;
        float t;
        RectTransform rt;

        void Awake()
        {
            rt = (RectTransform)transform;
            basePos = rt.anchoredPosition;
            phase = Random.Range(0, 3);
        }

        /// <summary>Doi vi tri goc (khi noi goi dat lai anchoredPosition).</summary>
        public void Rebase() { basePos = rt.anchoredPosition; }

        void Update()
        {
            if (PaperKit.Motion != 0) { rt.anchoredPosition = basePos; return; }
            t += Time.unscaledDeltaTime;
            if (t < 0.125f) return;
            t -= 0.125f;
            phase = (phase + 1) % Offsets.Length;
            rt.anchoredPosition = basePos + Offsets[phase];
        }
    }

    /// <summary>
    /// The mo ra (stop-motion): bat 120% -> 96% -> 100% trong 6 hinh (12 hinh / giay); cac lop hien lan luot tu sau ra truoc, moi lop
    /// tre 2 hinh. <see cref="Delay"/> de cac the trong trang Tab "dan" vao lan luot.
    /// </summary>
    public class CardPop : MonoBehaviour
    {
        static readonly float[] Scale = { 1.2f, 1.12f, 1.02f, 0.96f, 0.98f, 1f };
        public float Delay;
        readonly List<CanvasGroup> layers = new List<CanvasGroup>();
        float t;
        bool done;

        public void AddLayer(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
            layers.Add(cg);
        }

        public bool Done { get { return done; } }

        void Start()
        {
            if (PaperKit.Motion == 2) Finish();
            else Apply();
        }

        void Update()
        {
            if (done) return;
            t += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            Apply();
        }

        void Apply()
        {
            float local = t - Delay;
            int frame = Mathf.FloorToInt(local * PaperKit.StepFps);
            if (frame < 0)
            {
                transform.localScale = Vector3.one;
                foreach (CanvasGroup cg in layers) cg.alpha = 0f;
                return;
            }
            transform.localScale = Vector3.one * Scale[Mathf.Min(frame, Scale.Length - 1)];
            for (int i = 0; i < layers.Count; i++) layers[i].alpha = frame >= i * 2 ? 1f : 0f;
            if (frame >= Mathf.Max(Scale.Length, layers.Count * 2)) Finish();
        }

        void Finish()
        {
            done = true;
            transform.localScale = Vector3.one;
            foreach (CanvasGroup cg in layers) if (cg != null) cg.alpha = 1f;
        }
    }
}
