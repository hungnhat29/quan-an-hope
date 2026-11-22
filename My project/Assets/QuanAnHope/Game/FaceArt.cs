using System;
using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Ve mat nguyen lieu, mat mon an va hat hieu ung luc chay (Art Bible muc 6, 7, 11).
    /// Mat: mat la vong tron rong vien dam, ho khe ~40 do o phia tren huong vao giua mat (muc 6).
    /// Nguyen lieu theo bac (anh 08): 0 mat X + ruoi, 1 mat phang, 2 cuoi + giot nuoc, 3 lap lanh, 4 vuong mien, 5 kinh den + hao quang.
    /// Mon theo bac (bang muc 7): Lac vi mat X, Do buon, Binh thuong mat phang, Ngon vui, Tuyet hao ngac nhien, Huyen thoai kinh den + vuong mien.
    /// Toa do ve trong khung uv [0,1], y huong len; than mon chiem vong tron ban kinh ~0,29 quanh tam.
    /// </summary>
    public static class FaceArt
    {
        const int Size = 256;

        static readonly Color Ink = UiKit.Ink;
        static readonly Color EyeWhite = UiKit.Hex("#FFFDF6");
        static readonly Color Gold = UiKit.Hex("#F2C14E");
        static readonly Color Drop = UiKit.Hex("#8EC3E6");
        static readonly Color Blush = new Color(0.95f, 0.63f, 0.65f, 0.85f);
        static readonly Color MouthRed = UiKit.Hex("#C94A3B");
        static readonly Color FlyGreen = UiKit.Hex("#7DB87A");
        static readonly Color Smoke = UiKit.Hex("#8C8279");
        const float Line = 0.022f;   // net mat
        const float Rim = 0.014f;    // vien muc quanh phu kien

        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static readonly Dictionary<Texture2D, Material> materials = new Dictionary<Texture2D, Material>();
        static Shader alpha;

        public static Texture2D Ingredient(int tier)
        {
            tier = Mathf.Clamp(tier, 0, 5);
            return Get("Mat_NguyenLieu_" + tier, p => DrawIngredient(p, tier));
        }

        public static Texture2D Dish(int tier)
        {
            tier = Mathf.Clamp(tier, 0, 5);
            return Get("Mat_Mon_" + tier, p => DrawDish(p, tier));
        }

        /// <summary>
        /// Mat chu no (Art Bible muc 6): "lanh lung" (mac dinh: mi mat sup, con nguoi nho, may chau xuong, mieng thang)
        /// hoac "ngac nhien" (ngay 21 khi duoc mon Tuyet hao / Huyen thoai). Ve de dan len tam mat 800 x 500 cua khach
        /// (FaceSwap): truc x bi keo dan 1,6 lan nen hinh tron ve thanh elip hep.
        /// </summary>
        public static Texture2D Boss(bool surprised)
        {
            return Get(surprised ? "Mat_ChuNo_NgacNhien" : "Mat_ChuNo_LanhLung", p => DrawBoss(p, surprised));
        }

        const float FaceAspect = 1.6f;

        static void DrawBoss(Painter p, bool surprised)
        {
            Vector2 l = new Vector2(0.38f, 0.56f), r = new Vector2(0.62f, 0.56f);
            float rx = 0.085f / FaceAspect, ry = 0.085f;
            foreach (Vector2 c in new[] { l, r })
            {
                float k = surprised ? 1.25f : 1f;
                p.Ellipse(c, rx * k, ry * k, EyeWhite);
                p.EllipseRing(c, rx * k, ry * k, Line * 1.2f, Ink, -1f, 0f);
                // con nguoi nho (Art Bible muc 6: chu no co con nguoi nho)
                p.Ellipse(c + new Vector2(0f, surprised ? 0f : -0.02f), 0.022f / FaceAspect, 0.022f, Ink);
                if (!surprised)
                {
                    // mi mat sup nua tren
                    p.Polygon(new[] { c + new Vector2(-rx * 1.1f, 0.005f), c + new Vector2(rx * 1.1f, 0.005f),
                        c + new Vector2(rx * 0.8f, ry * 1.05f), c + new Vector2(-rx * 0.8f, ry * 1.05f) }, Ink, 0f);
                }
            }
            if (surprised)
            {
                // may nhuong cao, mieng chu O
                p.Stroke(new[] { new Vector2(0.33f, 0.74f), new Vector2(0.38f, 0.77f), new Vector2(0.43f, 0.745f) }, 0.022f, Ink);
                p.Stroke(new[] { new Vector2(0.57f, 0.745f), new Vector2(0.62f, 0.77f), new Vector2(0.67f, 0.74f) }, 0.022f, Ink);
                p.Ellipse(new Vector2(0.5f, 0.33f), 0.05f / FaceAspect, 0.065f, Ink);
                p.Ellipse(new Vector2(0.5f, 0.33f), 0.03f / FaceAspect, 0.042f, MouthRed);
            }
            else
            {
                // may chau xuong giua, mieng thang mong, ria mep
                p.Stroke(new[] { new Vector2(0.32f, 0.71f), new Vector2(0.44f, 0.665f) }, 0.026f, Ink);
                p.Stroke(new[] { new Vector2(0.56f, 0.665f), new Vector2(0.68f, 0.71f) }, 0.026f, Ink);
                p.Stroke(new[] { new Vector2(0.44f, 0.37f), new Vector2(0.56f, 0.37f) }, 0.02f, Ink);
                p.Stroke(new[] { new Vector2(0.42f, 0.43f), new Vector2(0.47f, 0.415f), new Vector2(0.5f, 0.425f),
                    new Vector2(0.53f, 0.415f), new Vector2(0.58f, 0.43f) }, 0.026f, Ink);
            }
        }

        // =====================================================================
        // Mat khach (sprint 7 F-062 AC-2, Art Bible muc 6): 6 bieu cam + cho do mo hoi + ban chop mat
        // =====================================================================

        public const string MoodHappy = "vui", MoodWait = "cho", MoodSweat = "cho_mo_hoi", MoodAngry = "gian",
            MoodSurprised = "ngac_nhien", MoodSad = "buon", MoodEating = "dang_an";

        public static readonly string[] CustomerMoods = { MoodHappy, MoodWait, MoodSweat, MoodAngry, MoodSurprised, MoodSad, MoodEating };

        /// <summary>
        /// Mat khach ve theo net cua 3 anh goc (BepNhanPham/Textures/face_*.png, 800 x 500): mat vong rong vien dam ho khe
        /// 40 do o phia tren huong vao giua mat, ma hong, mieng chu "w". <paramref name="blink"/>: mi nham (chop mat 0,12 giay,
        /// Art Bible muc 12). Anh vuong 512, dan len tam mat bi keo ngang 1,6 lan nhu mat chu no.
        /// </summary>
        public static Texture2D Customer(string mood, bool blink = false, int face = 0)
        {
            if (mood == MoodEating) blink = false; // dang an: mat da nham sung suong
            face = ((face % CustomerFaces) + CustomerFaces) % CustomerFaces;
            FaceStyle st = Styles[face];
            return Get("Mat_Khach_" + mood + (blink ? "_chop" : "") + (face > 0 ? "_k" + face : ""), p => DrawCustomer(p, mood, blink, st), 512);
        }

        // ---------------- sprint 9 B-06: 8 kieu mat (yeu cau 22/11 "nhieu khuon mat khac nhau, dua vao art bible") ----------------
        // Moi kieu giu quy cach Art Bible muc 6 (mat vong tron rong vien dam ho khe 40 do phia tren huong vao giua mat, khong
        // trong den / dom sang tru "Cho"; mieng chu "w" khi vui). Khac nhau o: co va khoang cach mat, long may, ma (tron / dep /
        // tan nhang), long mi, rau mep, not ruoi, do rong mieng.

        public const int CustomerFaces = 8;

        public struct FaceStyle
        {
            public string name;
            public float eye, spread, tall, mouth;
            /// <summary>0 khong, 1 cung ngan, 2 thang day.</summary>
            public int brow;
            /// <summary>0 ma tron, 1 ma dep rong, 2 tan nhang, 3 khong ma.</summary>
            public int cheek;
            public bool lashes, mustache, mole;
        }

        public static readonly FaceStyle[] Styles =
        {
            new FaceStyle { name = "Tròn hiền", eye = 1f, spread = 0f, tall = 1f, mouth = 1f },
            new FaceStyle { name = "Mắt to mi cong", eye = 1.14f, spread = -0.03f, tall = 1.04f, mouth = 0.9f, cheek = 1, lashes = true },
            new FaceStyle { name = "Mắt nhỏ tàn nhang", eye = 0.84f, spread = 0.035f, tall = 0.95f, mouth = 1f, brow = 1, cheek = 2 },
            new FaceStyle { name = "Râu mép", eye = 0.95f, spread = 0.01f, tall = 1f, mouth = 1.1f, brow = 2, cheek = 3, mustache = true },
            new FaceStyle { name = "Nốt ruồi", eye = 0.92f, spread = 0.02f, tall = 1f, mouth = 1.25f, cheek = 0, mole = true },
            new FaceStyle { name = "Mắt dài", eye = 1.02f, spread = -0.01f, tall = 1.15f, mouth = 0.95f, brow = 1, cheek = 2, lashes = true },
            new FaceStyle { name = "Má hây", eye = 0.96f, spread = 0.025f, tall = 0.98f, mouth = 0.8f, brow = 1, cheek = 1 },
            new FaceStyle { name = "Ông chú", eye = 1.08f, spread = -0.02f, tall = 0.94f, mouth = 1.05f, brow = 2, cheek = 0, mustache = true },
        };

        static readonly Color Freckle = UiKit.Hex("#C98E5A");
        static readonly Color MustacheBrown = UiKit.Hex("#6B4A3A");

        /// <summary>
        /// Chon bieu cam theo bang Art Bible muc 6: vui (dat / vuot yeu cau), ngac nhien (mon Tuyet hao / Huyen thoai),
        /// buon (thap hon 1 bac), gian (thap hon tu 2 bac, sai mon, hoac cho het kien nhan), cho (do mo hoi khi qua khoang
        /// an toan), dang an (trong luc an, sau phan ung 0,8 giay).
        /// </summary>
        public static string CustomerMood(bool reacting, PaymentOutcome outcome, int dishTier, bool eating, bool impatient, bool exhausted)
        {
            if (reacting)
            {
                if (outcome == PaymentOutcome.Angry) return MoodAngry;
                if (outcome == PaymentOutcome.Disappointed) return MoodSad;
                return dishTier >= 4 ? MoodSurprised : MoodHappy;
            }
            if (eating) return MoodEating;
            if (exhausted) return MoodAngry;
            return impatient ? MoodSweat : MoodWait;
        }

        static readonly Vector2 CEyeL = new Vector2(0.30f, 0.57f), CEyeR = new Vector2(0.70f, 0.57f);
        const float CEyeRx = 0.119f, CEyeRy = 0.21f, CLine = 0.034f;
        static readonly Color Sweat = UiKit.Hex("#8FC3D6");
        static readonly Color AngerMark = UiKit.Hex("#E8604F");

        static void DrawCustomer(Painter p, string mood, bool blink, FaceStyle st)
        {
            bool closedHappy = mood == MoodEating;
            float k = mood == MoodSurprised ? 1.12f : 1f;
            if (mood != MoodAngry && st.cheek != 3)
            {
                float b = mood == MoodEating ? 1.25f : 1f;
                if (st.cheek == 2)
                {
                    // tan nhang: 3 cham nho moi ben
                    foreach (float sx in new[] { 0.12f, 0.88f })
                        for (int i = 0; i < 3; i++)
                            p.Disc(new Vector2(sx + (i - 1) * 0.035f, 0.28f + (i == 1 ? 0.02f : 0f)), 0.012f, Freckle);
                }
                else
                {
                    float wx = st.cheek == 1 ? 1.35f : 1f, wy = st.cheek == 1 ? 0.7f : 1f;
                    p.Ellipse(new Vector2(0.12f, 0.27f), 0.075f * b * wx, 0.07f * b * wy, Blush);
                    p.Ellipse(new Vector2(0.88f, 0.27f), 0.075f * b * wx, 0.07f * b * wy, Blush);
                }
            }

            foreach (Vector2 c0 in new[] { CEyeL, CEyeR })
            {
                bool left = c0.x < 0.5f;
                Vector2 c = c0 + new Vector2(left ? -st.spread : st.spread, 0f);
                float rx = CEyeRx * k * st.eye, ry = CEyeRy * k * st.eye * st.tall;
                if (closedHappy)
                {
                    // mat cong len (^ ^) sung suong khi an
                    var arc = new Vector2[9];
                    for (int i = 0; i < arc.Length; i++)
                    {
                        float a = Mathf.Lerp(200f, -20f, i / (arc.Length - 1f)) * Mathf.Deg2Rad;
                        arc[i] = c + new Vector2(Mathf.Cos(a) * rx * 0.8f, Mathf.Sin(a) * ry * 0.45f - 0.03f);
                    }
                    p.Stroke(arc, CLine, Ink);
                    continue;
                }
                if (blink)
                {
                    // mi nham: duong cong xuong nhe ngang tam mat
                    p.Stroke(new[] { c + new Vector2(-rx, 0.01f), c + new Vector2(-rx * 0.5f, -0.025f), c + new Vector2(0f, -0.035f),
                        c + new Vector2(rx * 0.5f, -0.025f), c + new Vector2(rx, 0.01f) }, CLine, Ink);
                    continue;
                }
                p.Ellipse(c, rx, ry, EyeWhite);
                p.EllipseRing(c, rx, ry, CLine, Ink, left ? 60f : 120f, 20f);
                if (st.lashes)
                {
                    // hai soi mi cong ra phia ngoai (khong cham khe ho phia tren)
                    float sgn = left ? -1f : 1f;
                    Vector2 o = c + new Vector2(sgn * rx * 0.92f, ry * 0.35f);
                    p.Stroke(new[] { o, o + new Vector2(sgn * 0.035f, 0.03f) }, 0.016f, Ink);
                    Vector2 o2 = c + new Vector2(sgn * rx * 0.75f, ry * 0.65f);
                    p.Stroke(new[] { o2, o2 + new Vector2(sgn * 0.03f, 0.04f) }, 0.016f, Ink);
                }
                Vector2 pupil = Vector2.zero; float pr = 0f;
                switch (mood)
                {
                    case MoodWait: case MoodSweat: pupil = new Vector2(0.0625f, -0.04f); pr = 1f; break;
                    case MoodSurprised: pupil = Vector2.zero; pr = 0.7f; break;
                    case MoodSad: pupil = new Vector2(left ? 0.02f : -0.02f, -0.08f); pr = 1f; break;
                }
                if (pr > 0f) p.Ellipse(c + pupil, 0.025f * pr, 0.04f * pr, Ink);
            }

            // long may rieng cua kieu mat (bieu cam co long may rieng thi khong ve)
            if (mood != MoodAngry && mood != MoodSad && mood != MoodSurprised && st.brow > 0)
                foreach (Vector2 c0 in new[] { CEyeL, CEyeR })
                {
                    bool left = c0.x < 0.5f;
                    Vector2 c = c0 + new Vector2(left ? -st.spread : st.spread, CEyeRy * st.eye * st.tall + 0.06f);
                    if (st.brow == 1)
                        p.Stroke(new[] { c + new Vector2(-0.06f, -0.01f), c + new Vector2(0f, 0.012f), c + new Vector2(0.06f, -0.01f) }, 0.026f, Ink);
                    else
                        p.Stroke(new[] { c + new Vector2(-0.075f, 0f), c + new Vector2(0.075f, 0f) }, 0.045f, Ink);
                }
            if (st.mustache)
            {
                p.Ellipse(new Vector2(0.462f, 0.415f), 0.045f, 0.022f, MustacheBrown);
                p.Ellipse(new Vector2(0.538f, 0.415f), 0.045f, 0.022f, MustacheBrown);
            }
            if (st.mole) p.Disc(new Vector2(0.64f, 0.26f), 0.012f, Ink);

            // long may
            switch (mood)
            {
                case MoodAngry:
                    p.Stroke(new[] { new Vector2(0.16f, 0.785f), new Vector2(0.41f, 0.66f) }, 0.04f, Ink);
                    p.Stroke(new[] { new Vector2(0.84f, 0.785f), new Vector2(0.59f, 0.66f) }, 0.04f, Ink);
                    // gan gian do (tam giac = nguy hiem, Art Bible muc 5)
                    p.Stroke(new[] { new Vector2(0.9f, 0.96f), new Vector2(0.905f, 0.92f), new Vector2(0.88f, 0.885f) }, 0.024f, AngerMark);
                    p.Stroke(new[] { new Vector2(0.97f, 0.89f), new Vector2(0.955f, 0.84f), new Vector2(0.92f, 0.82f) }, 0.024f, AngerMark);
                    break;
                case MoodSad:
                    p.Stroke(new[] { new Vector2(0.19f, 0.79f), new Vector2(0.40f, 0.86f) }, 0.032f, Ink);
                    p.Stroke(new[] { new Vector2(0.81f, 0.79f), new Vector2(0.60f, 0.86f) }, 0.032f, Ink);
                    break;
                case MoodSurprised:
                    p.Stroke(new[] { new Vector2(0.20f, 0.87f), new Vector2(0.30f, 0.915f), new Vector2(0.40f, 0.87f) }, 0.03f, Ink);
                    p.Stroke(new[] { new Vector2(0.60f, 0.87f), new Vector2(0.70f, 0.915f), new Vector2(0.80f, 0.87f) }, 0.03f, Ink);
                    break;
            }

            // mieng
            switch (mood)
            {
                case MoodWait: case MoodSweat:
                    p.Stroke(new[] { new Vector2(MX(0.435f, st), 0.31f), new Vector2(MX(0.565f, st), 0.31f) }, CLine, Ink);
                    break;
                case MoodAngry:
                    p.Stroke(new[] { new Vector2(MX(0.43f, st), 0.255f), new Vector2(MX(0.465f, st), 0.305f), new Vector2(MX(0.5f, st), 0.26f),
                        new Vector2(MX(0.535f, st), 0.305f), new Vector2(MX(0.57f, st), 0.255f) }, CLine, Ink);
                    break;
                case MoodSurprised:
                    p.Ellipse(new Vector2(MX(0.5f, st), 0.29f), 0.042f, 0.08f, Ink);
                    p.Ellipse(new Vector2(MX(0.5f, st), 0.285f), 0.026f, 0.052f, MouthRed);
                    break;
                case MoodSad:
                    p.Stroke(new[] { new Vector2(MX(0.44f, st), 0.265f), new Vector2(MX(0.47f, st), 0.295f), new Vector2(MX(0.5f, st), 0.305f),
                        new Vector2(MX(0.53f, st), 0.295f), new Vector2(MX(0.56f, st), 0.265f) }, CLine, Ink);
                    // giot nuoc mat nho duoi mat trai
                    WaterDrop(p, new Vector2(MX(0.235f, st), 0.33f), 0.6f);
                    break;
                case MoodEating:
                    // ma phong, mieng nhai (o tron nho)
                    p.Ellipse(new Vector2(MX(0.5f, st), 0.31f), 0.04f, 0.05f, Ink);
                    p.Ellipse(new Vector2(MX(0.5f, st), 0.305f), 0.024f, 0.03f, MouthRed);
                    break;
                default: // vui: chu w
                    p.Stroke(new[] { new Vector2(MX(0.435f, st), 0.37f), new Vector2(MX(0.465f, st), 0.33f), new Vector2(MX(0.5f, st), 0.37f),
                        new Vector2(MX(0.535f, st), 0.33f), new Vector2(MX(0.565f, st), 0.37f) }, 0.03f, Ink);
                    break;
            }
            if (mood == MoodSweat) WaterDrop(p, new Vector2(0.875f, 0.74f), 1f);
        }

        /// <summary>Co gian do rong mieng quanh giua mat theo kieu mat.</summary>
        static float MX(float x, FaceStyle st) { return 0.5f + (x - 0.5f) * st.mouth; }

        /// <summary>Giot nuoc (mo hoi / nuoc mat): xanh nhat, vien muc, dau nhon o tren.</summary>
        static void WaterDrop(Painter p, Vector2 c, float s)
        {
            var pts = new[] { c + new Vector2(0f, 0.075f * s), c + new Vector2(0.026f * s, 0.005f * s), c + new Vector2(0.022f * s, -0.03f * s),
                c + new Vector2(0f, -0.045f * s), c + new Vector2(-0.022f * s, -0.03f * s), c + new Vector2(-0.026f * s, 0.005f * s) };
            p.Polygon(pts, Ink, 0.012f);
            p.Polygon(pts, Sweat, 0f);
        }

        /// <summary>Hat hieu ung: "khoi" (khoi xam gap khuc), "hoi" (net hoi trang), "tim", "tim_vang", "lap_lanh".</summary>
        public static Texture2D Fx(string kind)
        {
            return Get("Fx_" + kind, p => DrawFx(p, kind));
        }

        /// <summary>Vat lieu trong suot khong chiu anh sang cho mot anh (dung chung).</summary>
        public static Material Mat(Texture2D tex)
        {
            Material m;
            if (materials.TryGetValue(tex, out m) && m != null) return m;
            if (alpha == null) alpha = Shader.Find("BepNhanPham/UnlitAlpha");
            m = new Material(alpha) { name = "M_" + tex.name };
            m.SetTexture("_MainTex", tex);
            m.SetColor("_Color", Color.white);
            materials[tex] = m;
            return m;
        }

        static Texture2D Get(string name, Action<Painter> draw, int size = Size)
        {
            Texture2D t;
            if (textures.TryGetValue(name, out t) && t != null) return t;
            var p = new Painter(size);
            draw(p);
            t = p.ToTexture(name);
            textures[name] = t;
            return t;
        }

        // =====================================================================
        // Nguyen lieu
        // =====================================================================

        static readonly Vector2 EyeL = new Vector2(0.405f, 0.53f), EyeR = new Vector2(0.595f, 0.53f);
        const float EyeRadius = 0.052f;

        static void DrawIngredient(Painter p, int tier)
        {
            switch (tier)
            {
                case 0:
                    XEyes(p);
                    Frown(p);
                    Flies(p);
                    break;
                case 1:
                    RingEyes(p);
                    FlatMouth(p);
                    break;
                case 2:
                    RingEyes(p);
                    CatMouth(p);
                    SweatDrop(p, new Vector2(0.665f, 0.6f), 1f);
                    break;
                case 3:
                    RingEyes(p);
                    CatMouth(p);
                    Sparkle(p, new Vector2(0.75f, 0.72f), 0.08f, Color.white);
                    break;
                case 4:
                    RingEyes(p);
                    CatMouth(p);
                    Crown(p, 0.5f, 0.76f);
                    break;
                default:
                    Halo(p, new Vector2(0.5f, 0.9f));
                    Sunglasses(p);
                    break;
            }
        }

        // =====================================================================
        // Mon an
        // =====================================================================

        static void DrawDish(Painter p, int tier)
        {
            switch (tier)
            {
                case 0: // Lac vi: mat X
                    XEyes(p);
                    Frown(p);
                    break;
                case 1: // Do: buon
                    RingEyes(p);
                    p.Stroke(new[] { new Vector2(0.35f, 0.615f), new Vector2(0.455f, 0.632f) }, 0.018f, Ink);
                    p.Stroke(new[] { new Vector2(0.545f, 0.632f), new Vector2(0.65f, 0.615f) }, 0.018f, Ink);
                    p.Stroke(new[] { new Vector2(0.455f, 0.425f), new Vector2(0.478f, 0.452f), new Vector2(0.5f, 0.432f),
                        new Vector2(0.522f, 0.452f), new Vector2(0.545f, 0.425f) }, 0.018f, Ink);
                    SweatDrop(p, new Vector2(0.37f, 0.45f), 0.8f);
                    break;
                case 2: // Binh thuong: mat phang
                    RingEyes(p);
                    FlatMouth(p);
                    break;
                case 3: // Ngon: vui
                    p.Ellipse(new Vector2(0.345f, 0.455f), 0.042f, 0.024f, Blush);
                    p.Ellipse(new Vector2(0.655f, 0.455f), 0.042f, 0.024f, Blush);
                    RingEyes(p);
                    CatMouth(p);
                    break;
                case 4: // Tuyet hao: ngac nhien
                    OvalEye(p, EyeL, 60f);
                    OvalEye(p, EyeR, 120f);
                    p.Ellipse(new Vector2(0.5f, 0.405f), 0.03f, 0.038f, Ink);
                    p.Ellipse(new Vector2(0.5f, 0.405f), 0.018f, 0.026f, MouthRed);
                    p.Stroke(new[] { new Vector2(0.31f, 0.66f), new Vector2(0.33f, 0.62f) }, 0.014f, Ink);
                    p.Stroke(new[] { new Vector2(0.69f, 0.66f), new Vector2(0.67f, 0.62f) }, 0.014f, Ink);
                    break;
                default: // Huyen thoai: kinh den + vuong mien
                    Crown(p, 0.5f, 0.76f);
                    Sunglasses(p);
                    CatMouth(p);
                    break;
            }
        }

        // =====================================================================
        // Hat hieu ung
        // =====================================================================

        static void DrawFx(Painter p, string kind)
        {
            switch (kind)
            {
                case "khoi":
                {
                    var pts = new[] { new Vector2(0.42f, 0.1f), new Vector2(0.6f, 0.3f), new Vector2(0.4f, 0.5f), new Vector2(0.6f, 0.7f), new Vector2(0.46f, 0.9f) };
                    p.Stroke(pts, 0.11f, Ink);
                    p.Stroke(pts, 0.07f, Smoke);
                    break;
                }
                case "hoi":
                {
                    var pts = new Vector2[16];
                    for (int i = 0; i < pts.Length; i++)
                    {
                        float t = i / (pts.Length - 1f);
                        pts[i] = new Vector2(0.5f + 0.13f * Mathf.Sin(t * Mathf.PI * 2.2f), 0.1f + 0.8f * t);
                    }
                    p.Stroke(pts, 0.1f, Ink);
                    p.Stroke(pts, 0.064f, Color.white);
                    break;
                }
                case "tim":
                    Heart(p, new Vector2(0.5f, 0.5f), 0.36f, Color.white);
                    break;
                case "tim_vang":
                    Heart(p, new Vector2(0.5f, 0.5f), 0.36f, Gold);
                    break;
                case "xoay_vang":
                {
                    // vong xoay vang (nuoc sot than thanh, Art Bible muc 11)
                    var pts = new Vector2[24];
                    for (int i = 0; i < pts.Length; i++)
                    {
                        float t = i / (pts.Length - 1f);
                        float a = t * Mathf.PI * 3f;
                        float r = 0.08f + 0.3f * t;
                        pts[i] = new Vector2(0.5f + Mathf.Cos(a) * r, 0.5f + Mathf.Sin(a) * r);
                    }
                    p.Stroke(pts, 0.11f, Ink);
                    p.Stroke(pts, 0.065f, Gold);
                    break;
                }
                default:
                    Sparkle(p, new Vector2(0.5f, 0.5f), 0.42f, Color.white);
                    break;
            }
        }

        // =====================================================================
        // Bo phan
        // =====================================================================

        static void RingEyes(Painter p)
        {
            Eye(p, EyeL, 60f);
            Eye(p, EyeR, 120f);
        }

        /// <summary>Mat vong tron trang, vien muc ho khe 40 do; gapDeg la huong khe (huong vao giua mat, phia tren).</summary>
        static void Eye(Painter p, Vector2 c, float gapDeg)
        {
            p.Disc(c, EyeRadius, EyeWhite);
            p.Ring(c, EyeRadius, Line, Ink, gapDeg, 20f);
        }

        static void OvalEye(Painter p, Vector2 c, float gapDeg)
        {
            p.Ellipse(c, EyeRadius * 1.05f, EyeRadius * 1.35f, EyeWhite);
            p.EllipseRing(c, EyeRadius * 1.05f, EyeRadius * 1.35f, Line, Ink, gapDeg, 20f);
        }

        static void XEyes(Painter p)
        {
            foreach (Vector2 c in new[] { EyeL, EyeR })
            {
                const float d = 0.036f;
                p.Stroke(new[] { c + new Vector2(-d, -d), c + new Vector2(d, d) }, Line, Ink);
                p.Stroke(new[] { c + new Vector2(-d, d), c + new Vector2(d, -d) }, Line, Ink);
            }
        }

        static void FlatMouth(Painter p)
        {
            p.Stroke(new[] { new Vector2(0.465f, 0.44f), new Vector2(0.535f, 0.44f) }, 0.02f, Ink);
        }

        static void CatMouth(Painter p)
        {
            p.Stroke(Arc(new Vector2(0.475f, 0.458f), 0.025f, 200f, 340f), 0.018f, Ink);
            p.Stroke(Arc(new Vector2(0.525f, 0.458f), 0.025f, 200f, 340f), 0.018f, Ink);
        }

        static void Frown(Painter p)
        {
            p.Stroke(Arc(new Vector2(0.5f, 0.39f), 0.065f, 45f, 135f), 0.02f, Ink);
        }

        static void Flies(Painter p)
        {
            p.Stroke(Arc(new Vector2(0.79f, 0.8f), 0.05f, 140f, 220f), 0.016f, FlyGreen);
            p.Stroke(Arc(new Vector2(0.85f, 0.83f), 0.05f, 140f, 220f), 0.016f, FlyGreen);
            p.Disc(new Vector2(0.81f, 0.9f), 0.012f, Ink);
        }

        static void SweatDrop(Painter p, Vector2 c, float scale)
        {
            Vector2[] shape = DropShape(c, 0.026f * scale);
            p.Polygon(shape, Ink, Rim);
            p.Polygon(shape, Drop, 0f);
        }

        static Vector2[] DropShape(Vector2 c, float r)
        {
            var pts = new List<Vector2> { c + new Vector2(0f, r * 2.3f) };
            for (int i = 0; i <= 12; i++)
            {
                float a = Mathf.Deg2Rad * (20f - 220f * i / 12f);
                pts.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r);
            }
            return pts.ToArray();
        }

        static void Sparkle(Painter p, Vector2 c, float r, Color fill)
        {
            var pts = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                float a = Mathf.PI * 0.5f + i * Mathf.PI / 4f;
                float rr = i % 2 == 0 ? r : r * 0.28f;
                pts[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rr;
            }
            p.Polygon(pts, Ink, Rim);
            p.Polygon(pts, fill, 0f);
        }

        static void Crown(Painter p, float cx, float baseY)
        {
            var pts = new[]
            {
                new Vector2(cx - 0.12f, baseY), new Vector2(cx + 0.12f, baseY), new Vector2(cx + 0.13f, baseY + 0.13f),
                new Vector2(cx + 0.065f, baseY + 0.06f), new Vector2(cx, baseY + 0.15f), new Vector2(cx - 0.065f, baseY + 0.06f),
                new Vector2(cx - 0.13f, baseY + 0.13f),
            };
            p.Polygon(pts, Ink, Rim);
            p.Polygon(pts, Gold, 0f);
        }

        static void Halo(Painter p, Vector2 c)
        {
            p.EllipseRing(c, 0.13f, 0.035f, 0.03f, Ink, -1f, 0f);
            p.EllipseRing(c, 0.13f, 0.035f, 0.016f, Gold, -1f, 0f);
        }

        static void Sunglasses(Painter p)
        {
            p.Stroke(new[] { new Vector2(0.34f, 0.565f), new Vector2(0.66f, 0.565f) }, 0.024f, Ink);
            p.Polygon(new[] { new Vector2(0.35f, 0.575f), new Vector2(0.48f, 0.575f), new Vector2(0.47f, 0.515f), new Vector2(0.375f, 0.505f) }, Ink, 0.006f);
            p.Polygon(new[] { new Vector2(0.52f, 0.575f), new Vector2(0.65f, 0.575f), new Vector2(0.625f, 0.505f), new Vector2(0.53f, 0.515f) }, Ink, 0.006f);
        }

        static void Heart(Painter p, Vector2 c, float r, Color fill)
        {
            var pts = new Vector2[48];
            for (int i = 0; i < pts.Length; i++)
            {
                float t = i / (float)pts.Length * Mathf.PI * 2f;
                float s = Mathf.Sin(t);
                float x = 16f * s * s * s;
                float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);
                pts[i] = c + new Vector2(x, y + 2f) / 17f * r;
            }
            p.Polygon(pts, Ink, 0.035f);
            p.Polygon(pts, fill, 0f);
        }

        static Vector2[] Arc(Vector2 c, float r, float fromDeg, float toDeg)
        {
            var pts = new Vector2[10];
            for (int i = 0; i < pts.Length; i++)
            {
                float a = Mathf.Deg2Rad * Mathf.Lerp(fromDeg, toDeg, i / (pts.Length - 1f));
                pts[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            return pts;
        }

        // =====================================================================
        // Ve bang ham khoang cach co dau (khu rang cua theo 1 diem anh)
        // =====================================================================

        sealed class Painter
        {
            readonly int n;
            readonly Color[] px;
            readonly float pixel;

            public Painter(int size)
            {
                n = size;
                px = new Color[size * size];
                pixel = 1f / size;
            }

            /// <summary>Phu mau theo ham khoang cach (am = ben trong), chi trong hop gioi han.</summary>
            void Fill(Func<Vector2, float> sdf, Color color, Vector2 min, Vector2 max)
            {
                int x0 = Mathf.Clamp(Mathf.FloorToInt(min.x * n) - 2, 0, n - 1), x1 = Mathf.Clamp(Mathf.CeilToInt(max.x * n) + 2, 0, n - 1);
                int y0 = Mathf.Clamp(Mathf.FloorToInt(min.y * n) - 2, 0, n - 1), y1 = Mathf.Clamp(Mathf.CeilToInt(max.y * n) + 2, 0, n - 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = sdf(new Vector2((x + 0.5f) * pixel, (y + 0.5f) * pixel));
                        float cover = Mathf.Clamp01(0.5f - d / pixel) * color.a;
                        if (cover <= 0f) continue;
                        int i = y * n + x;
                        Color dst = px[i];
                        float a = cover + dst.a * (1f - cover);
                        if (a <= 0f) continue;
                        Color rgb = (new Color(color.r, color.g, color.b) * cover + new Color(dst.r, dst.g, dst.b) * dst.a * (1f - cover)) / a;
                        px[i] = new Color(rgb.r, rgb.g, rgb.b, a);
                    }
            }

            public void Disc(Vector2 c, float r, Color color)
            {
                Fill(q => (q - c).magnitude - r, color, c - Vector2.one * r, c + Vector2.one * r);
            }

            public void Ellipse(Vector2 c, float rx, float ry, Color color)
            {
                Fill(q => EllipseDistance(q - c, rx, ry), color, c - new Vector2(rx, ry), c + new Vector2(rx, ry));
            }

            /// <summary>Vong tron vien, ho khe (gapDeg &lt; 0: khong ho).</summary>
            public void Ring(Vector2 c, float r, float width, Color color, float gapDeg, float gapHalfDeg)
            {
                EllipseRing(c, r, r, width, color, gapDeg, gapHalfDeg);
            }

            public void EllipseRing(Vector2 c, float rx, float ry, float width, Color color, float gapDeg, float gapHalfDeg)
            {
                Vector2 e1 = Vector2.zero, e2 = Vector2.zero;
                if (gapDeg >= 0f)
                {
                    float a1 = Mathf.Deg2Rad * (gapDeg - gapHalfDeg), a2 = Mathf.Deg2Rad * (gapDeg + gapHalfDeg);
                    e1 = c + new Vector2(Mathf.Cos(a1) * rx, Mathf.Sin(a1) * ry);
                    e2 = c + new Vector2(Mathf.Cos(a2) * rx, Mathf.Sin(a2) * ry);
                }
                Fill(q =>
                {
                    Vector2 d = q - c;
                    if (gapDeg >= 0f)
                    {
                        float ang = Mathf.Atan2(d.y / ry, d.x / rx) * Mathf.Rad2Deg;
                        if (Mathf.Abs(Mathf.DeltaAngle(ang, gapDeg)) < gapHalfDeg)
                            return Mathf.Min((q - e1).magnitude, (q - e2).magnitude) - width * 0.5f;
                    }
                    return Mathf.Abs(EllipseDistance(d, rx, ry)) - width * 0.5f;
                }, color, c - new Vector2(rx + width, ry + width), c + new Vector2(rx + width, ry + width));
            }

            /// <summary>Net ve dau tron qua cac diem.</summary>
            public void Stroke(Vector2[] pts, float width, Color color)
            {
                Vector2 min = pts[0], max = pts[0];
                foreach (Vector2 q in pts)
                {
                    min = Vector2.Min(min, q);
                    max = Vector2.Max(max, q);
                }
                float h = width * 0.5f;
                Fill(q =>
                {
                    float best = float.MaxValue;
                    for (int i = 0; i + 1 < pts.Length; i++) best = Mathf.Min(best, SegmentDistance(q, pts[i], pts[i + 1]));
                    return best - h;
                }, color, min - Vector2.one * h, max + Vector2.one * h);
            }

            /// <summary>Da giac to dac; grow &gt; 0 noi rong ra (dung ve vien muc).</summary>
            public void Polygon(Vector2[] pts, Color color, float grow)
            {
                Vector2 min = pts[0], max = pts[0];
                foreach (Vector2 q in pts)
                {
                    min = Vector2.Min(min, q);
                    max = Vector2.Max(max, q);
                }
                Fill(q =>
                {
                    float best = float.MaxValue;
                    bool inside = false;
                    for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
                    {
                        best = Mathf.Min(best, SegmentDistance(q, pts[j], pts[i]));
                        if ((pts[i].y > q.y) != (pts[j].y > q.y) &&
                            q.x < (pts[j].x - pts[i].x) * (q.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x)
                            inside = !inside;
                    }
                    return (inside ? -best : best) - grow;
                }, color, min - Vector2.one * grow, max + Vector2.one * grow);
            }

            static float SegmentDistance(Vector2 q, Vector2 a, Vector2 b)
            {
                Vector2 ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-8f));
                return (q - (a + ab * t)).magnitude;
            }

            static float EllipseDistance(Vector2 d, float rx, float ry)
            {
                float k = new Vector2(d.x / rx, d.y / ry).magnitude;
                return (k - 1f) * Mathf.Min(rx, ry);
            }

            public Texture2D ToTexture(string name)
            {
                var t = new Texture2D(n, n, TextureFormat.RGBA32, true) { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear };
                t.SetPixels(px);
                t.Apply(true, true);
                return t;
            }
        }
    }

    /// <summary>
    /// Tam anh mat / hat hieu ung luon quay ve nguoi choi, dat truoc than mon (phia nguoi nhin) mot khoang depth.
    /// </summary>
    public class FaceBillboard : MonoBehaviour
    {
        /// <summary>Camera cua nguoi choi; GameController gan khi khoi tao.</summary>
        public static Camera View;

        Transform anchor;
        Transform scaleRoot;
        float depth;
        float baseSize;
        float sizeFactor = 1f;
        MeshRenderer quad;
        float blinkIn = -1f, blink = 1f;
        /// <summary>So lan da chop (test).</summary>
        public int Blinks { get; private set; }
        /// <summary>Art Bible muc 12: chop mat 0,12 giay, ngau nhien moi 3-5 giay (impl plan sprint 7 F-062 AC-3).</summary>
        public const float BlinkSeconds = 0.12f;
        public const float BlinkMin = 3f, BlinkMax = 5f;

        /// <summary>Chop mat (nep mat theo chieu doc) - 20/11: nguyen lieu song nhu nhan vat cot truyen. Tat cho anh test on dinh.</summary>
        public static bool Blinking = true;

        public Texture2D Texture { get; private set; }
        public MeshRenderer Renderer { get { return quad; } }
        public Transform Anchor { get { return anchor; } }

        /// <summary>Tao tam mat con cua parent. size, depth tinh theo don vi cuc bo cua parent.</summary>
        public static FaceBillboard Create(Transform parent, Transform anchor, Texture2D tex, float size, float depth, string name = "Mat")
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = FaceArt.Mat(tex);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            var f = go.AddComponent<FaceBillboard>();
            f.anchor = anchor != null ? anchor : parent;
            f.scaleRoot = parent;
            f.depth = depth;
            f.baseSize = size;
            f.quad = r;
            f.Texture = tex;
            go.transform.localScale = Vector3.one * size;
            f.LateUpdate();
            return f;
        }

        public void SetSizeFactor(float factor)
        {
            sizeFactor = factor;
            transform.localScale = Vector3.one * baseSize * sizeFactor;
        }

        void LateUpdate()
        {
            Camera cam = View != null ? View : Camera.main;
            if (cam == null || anchor == null) return;
            Transform c = cam.transform;
            float worldDepth = depth * Mathf.Abs(scaleRoot.lossyScale.x);
            Vector3 toCam = c.position - anchor.position;
            Vector3 dir = toCam.sqrMagnitude > 1e-6f ? toCam.normalized : -c.forward;
            transform.position = anchor.position + dir * worldDepth;
            transform.rotation = c.rotation;

            // chop mat: moi 3-5 giay nep doc trong 0,12 giay (Art Bible muc 12)
            float y = 1f;
            if (Blinking)
            {
                if (blinkIn < 0f) blinkIn = UnityEngine.Random.Range(0.5f, BlinkMax);
                blinkIn -= Time.deltaTime;
                if (blinkIn <= 0f && blinkIn > -1f) { blink = 0f; Blinks++; blinkIn = UnityEngine.Random.Range(BlinkMin, BlinkMax); }
                if (blink < 1f)
                {
                    blink = Mathf.Min(1f, blink + Time.deltaTime / BlinkSeconds);
                    y = 1f - 0.8f * Mathf.Sin(blink * Mathf.PI);
                }
            }
            float k = baseSize * sizeFactor;
            transform.localScale = new Vector3(k, k * y, k);
        }
    }
}
