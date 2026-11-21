using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Cong dich chuyen xoan oc tren ban nhan hang (Art Bible muc 11: "thung roi qua cong dich chuyen 1,2 giay: xoan oc mo ->
    /// thung roi -> nay 2 lan -> lap lanh"; tu vung hinh: xoan oc = dich chuyen). Mo 0,25 giay, quay, dong lai khi thung cham ban.
    /// </summary>
    public class PortalFx : MonoBehaviour
    {
        public const float Seconds = 1.2f;
        const int Beads = 24;
        static readonly Color Lilac = UiKit.Hex("#C9B3F0");
        static readonly Color Gold = UiKit.Hex("#F2C14E");
        float t;
        public float Openness { get; private set; }

        public static PortalFx Play(Transform parent, Vector3 localPos)
        {
            var go = new GameObject("CongDichChuyen");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var p = go.AddComponent<PortalFx>();
            for (int i = 0; i < Beads; i++)
            {
                // xoan oc Archimedes 2 vong, hat to dan ra ngoai
                float u = i / (Beads - 1f);
                float a = u * Mathf.PI * 4f;
                float r = 0.04f + 0.26f * u;
                Toon.Prim(PrimitiveType.Sphere, go.transform, "Hat_" + i, new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r),
                    Vector3.one * (0.025f + 0.035f * u), i % 4 == 3 ? Gold : Lilac, 0.006f);
            }
            go.transform.localScale = Vector3.one * 0.01f;
            return p;
        }

        void Update()
        {
            t += Time.deltaTime / Seconds;
            // mo nhanh (easeOutBack), giu, dong lai o 60-80% (thung da roi qua)
            float open = t < 0.2f ? EaseOutBack(t / 0.2f) : t < 0.6f ? 1f : Mathf.Clamp01(1f - (t - 0.6f) / 0.2f);
            Openness = open;
            transform.localScale = Vector3.one * Mathf.Max(0.01f, open);
            transform.localRotation = Quaternion.Euler(0f, -t * 540f, 0f);
            if (t >= 0.8f) Destroy(gameObject);
        }

        static float EaseOutBack(float x)
        {
            float a = x - 1f;
            return 1f + 2.70158f * a * a * a + 1.70158f * a * a;
        }
    }

    /// <summary>Mot hat bay len roi mo (trai tim khach vui, khoi xam, xoay vang): FaceBillboard quay ve nguoi choi.</summary>
    public class RiseFx : MonoBehaviour
    {
        FaceBillboard[] parts;
        Vector3[] starts;
        float seconds, rise, spin, t;
        MaterialPropertyBlock block;

        public float Progress { get { return t; } }
        public string Kind { get; private set; }

        /// <summary>
        /// <paramref name="kind"/>: anh FaceArt.Fx ("tim", "tim_vang", "khoi", "lap_lanh"); <paramref name="ring"/> &gt; 0: cac hat xep
        /// vong tron ban kinh ring va quay quanh tam (xoay vang nuoc sot).
        /// </summary>
        public static RiseFx Play(Transform at, Vector3 localPos, string kind, int count, float seconds, float rise, float size, float ring = 0f, float spinDeg = 0f)
        {
            var go = new GameObject("HieuUng_" + kind);
            go.transform.SetParent(at, false);
            go.transform.localPosition = localPos;
            var fx = go.AddComponent<RiseFx>();
            fx.Kind = kind;
            fx.seconds = seconds;
            fx.rise = rise;
            fx.spin = spinDeg;
            fx.parts = new FaceBillboard[count];
            fx.starts = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count;
                fx.starts[i] = ring > 0f ? new Vector3(Mathf.Cos(a) * ring, 0.02f * i, Mathf.Sin(a) * ring)
                    : new Vector3((i - (count - 1) * 0.5f) * size * 0.8f, 0.03f * (i % 2), 0f);
                Transform spot = Toon.Empty(go.transform, "Goc_" + i, fx.starts[i]).transform;
                fx.parts[i] = FaceBillboard.Create(go.transform, spot, FaceArt.Fx(kind), size, 0f, "Hat_" + i);
            }
            return fx;
        }

        void Update()
        {
            t = Mathf.Min(1f, t + Time.deltaTime / Mathf.Max(0.05f, seconds));
            if (block == null) block = new MaterialPropertyBlock();
            Quaternion r = Quaternion.Euler(0f, spin * t, 0f);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null) continue;
                parts[i].Anchor.localPosition = r * starts[i] + Vector3.up * rise * Mathf.Sin(t * Mathf.PI * 0.5f);
                MeshRenderer mr = parts[i].Renderer;
                mr.GetPropertyBlock(block);
                block.SetColor("_Color", new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Sin(t * Mathf.PI) * 1.6f)));
                mr.SetPropertyBlock(block);
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }

    /// <summary>Dong xu bay tu khach ve o Vi tren HUD (Art Bible muc 11: "khach vui / tip 0,8 giay: trai tim + dong xu bay ve o Vi").</summary>
    public class CoinFly : MonoBehaviour
    {
        public const float Seconds = 0.8f;
        RectTransform rect, target;
        Vector2 from;
        float t;
        public bool Done { get { return t >= 1f; } }

        public static CoinFly Play(RectTransform canvas, RectTransform target, Vector2 fromScreen)
        {
            Image coin = UiKit.Panel(canvas, "DongXu", new Vector2(0f, 0f), Vector2.zero, new Vector2(46f, 46f), UiKit.Mustard, false);
            coin.sprite = UiKit.Circle;
            coin.raycastTarget = false;
            Image rim = UiKit.Panel(coin.transform, "Vien", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54f, 54f), UiKit.Ink, false);
            rim.sprite = UiKit.Circle;
            rim.raycastTarget = false;
            rim.transform.SetAsFirstSibling();
            var label = UiKit.Text(coin.transform, "G", 26f);
            label.color = UiKit.Ink;
            var f = coin.gameObject.AddComponent<CoinFly>();
            f.rect = coin.rectTransform;
            f.rect.anchorMin = f.rect.anchorMax = Vector2.zero;
            f.target = target;
            f.from = fromScreen;
            f.Place();
            return f;
        }

        Vector2 TargetScreen()
        {
            Vector3[] c = new Vector3[4];
            target.GetWorldCorners(c);
            return (c[0] + c[2]) * 0.5f;
        }

        void Place()
        {
            Vector2 to = TargetScreen();
            float e = 1f - (1f - t) * (1f - t);
            Vector2 p = Vector2.Lerp(from, to, e) + Vector2.up * Mathf.Sin(t * Mathf.PI) * 120f;
            Canvas cv = rect.GetComponentInParent<Canvas>();
            float k = cv != null ? cv.scaleFactor : 1f;
            rect.anchoredPosition = p / Mathf.Max(0.01f, k);
            rect.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(t * Mathf.PI));
        }

        void Update()
        {
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime / Seconds);
            Place();
            if (t >= 1f) Destroy(gameObject);
        }
    }

    /// <summary>
    /// Mau phu toan canh theo gio (Art Bible muc 4): 9:00-15:00 #FFF6E6 0%; 15:00-21:00 #FFD9B0 tang dan toi 20%;
    /// sau 21:00 #C9B3F0 20%. Mot lop anh trong suot tren canh, duoi HUD (thu tu canvas 5 &lt; HUD 10).
    /// </summary>
    public class ScreenTint : MonoBehaviour
    {
        static readonly Color Day = UiKit.Hex("#FFF6E6"), Afternoon = UiKit.Hex("#FFD9B0"), Night = UiKit.Hex("#C9B3F0");
        Image image;
        System.Func<float> hour;
        System.Func<bool> visible;

        public Color Current { get { return image != null ? image.color : Color.clear; } }

        public static ScreenTint Create(System.Func<float> hourSource, System.Func<bool> visibleNow)
        {
            var go = new GameObject("MauPhuTheoGio");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var st = go.AddComponent<ScreenTint>();
            var img = new GameObject("Phu").AddComponent<Image>();
            img.transform.SetParent(go.transform, false);
            img.raycastTarget = false;
            RectTransform r = img.rectTransform;
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            st.image = img;
            st.hour = hourSource;
            st.visible = visibleNow;
            st.Update();
            return st;
        }

        /// <summary>Mau va do phu (alpha) o mot gio trong ngay.</summary>
        public static Color TintFor(float h)
        {
            if (h < 15f) return new Color(Day.r, Day.g, Day.b, 0f);
            if (h < 21f) { Color c = Afternoon; c.a = 0.2f * Mathf.InverseLerp(15f, 21f, h); return c; }
            // 21:00 -> 21:15 chuyen tu chieu am sang tim nhat
            float k = Mathf.InverseLerp(21f, 21.25f, h);
            Color n = Color.Lerp(Afternoon, Night, k);
            n.a = 0.2f;
            return n;
        }

        void Update()
        {
            bool on = visible == null || visible();
            Color c = on && hour != null ? TintFor(hour()) : Color.clear;
            if (image.color != c) image.color = c;
            image.enabled = c.a > 0.001f;
        }
    }

    /// <summary>
    /// Mua ngay 2 (constitution muc 2 "ngay 2 mua lon"; Art Bible muc 4: #9CC7D8 25% ngoai cua, vet mua ve tay ngoai cua,
    /// trong phong giu am). Vet mua mong roi xien tren pho (Goc1/Street), khong vao trong phong.
    /// </summary>
    public class RainFx : MonoBehaviour
    {
        public const int Streaks = 140;
        static readonly Color StreakColor = UiKit.Hex("#DCEEF5");
        readonly List<Transform> streaks = new List<Transform>();
        readonly List<float> speeds = new List<float>();
        public bool Raining { get; private set; }
        public int Count { get { return streaks.Count; } }

        public static RainFx Create(Transform street)
        {
            var go = new GameObject("Mua");
            go.transform.SetParent(street, false);
            var fx = go.AddComponent<RainFx>();
            for (int i = 0; i < Streaks; i++)
            {
                // vung ngay sau cua: x -6..6, cao 0..5 m, sau 4,5..12 m (truoc nha ben kia duong)
                var p = new Vector3(Random.Range(-6f, 6f), Random.Range(0f, 5f), Random.Range(4.5f, 12f));
                Transform s = Toon.Prim(PrimitiveType.Cube, go.transform, "VetMua_" + i, p, new Vector3(0.012f, 0.34f, 0.012f), StreakColor, 0f,
                    new Vector3(0f, 0f, -12f)).transform;
                fx.streaks.Add(s);
                fx.speeds.Add(Random.Range(6f, 8.5f));
            }
            go.SetActive(false);
            return fx;
        }

        public void SetRaining(bool on)
        {
            Raining = on;
            if (gameObject.activeSelf != on) gameObject.SetActive(on);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector3 slant = Quaternion.Euler(0f, 0f, -12f) * Vector3.down;
            for (int i = 0; i < streaks.Count; i++)
            {
                Transform s = streaks[i];
                Vector3 p = s.localPosition + slant * speeds[i] * dt;
                if (p.y < 0f) p = new Vector3(Random.Range(-6f, 6f), 5f, Random.Range(4.5f, 12f));
                s.localPosition = p;
            }
        }
    }
}
