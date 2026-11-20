using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Troi ngoai cua doi mau theo gio trong game (9:00 sang -> trua -> chieu vang -> hoang hon -> toi sau 21:00).
    /// Pho (BuildStreet) nhuom theo anh sang troi; cua so, long den, bong den duong sang len khi tran toi.
    /// Dung MaterialPropertyBlock nen vat lieu dung chung trong phong khong bi doi mau.
    /// Mau la quy cach hinh (Art Bible §3), khong phai so can bang (Dieu 2).
    /// </summary>
    public class SkyCycle : MonoBehaviour
    {
        struct Key
        {
            public float hour;
            public Color top, bottom, ambient, cloud;
            public Key(float h, string t, string b, string a, string c)
            {
                hour = h; top = UiKit.Hex(t); bottom = UiKit.Hex(b); ambient = UiKit.Hex(a); cloud = UiKit.Hex(c);
            }
        }

        static readonly Key[] Keys =
        {
            new Key(9f,    "#CFE4EA", "#F7E3BE", "#FFFFFF", "#FFF8EC"), // sang: dung mau dung canh
            new Key(12f,   "#B5DCEB", "#EAF2E6", "#FFFFFF", "#FFFFFF"), // trua: xanh trong
            new Key(15f,   "#BFDCE6", "#F7E0B8", "#FFF6EA", "#FFF4E4"),
            new Key(17.5f, "#F6CFA0", "#F4A77E", "#FFE6CC", "#FFE2C4"), // chieu vang
            new Key(19f,   "#B88AB8", "#F08A66", "#E6B4B4", "#F6B9A0"), // hoang hon
            new Key(20.5f, "#3F4475", "#8C6A9A", "#8C8CC0", "#9C94C0"), // chang vang
            new Key(22f,   "#262A4E", "#4A3F6A", "#6C70A8", "#5C5E8A"), // toi
        };

        /// <summary>Den bat dan tu 18:30, sang han luc 20:00.</summary>
        public const float LightsOnFrom = 18.5f, LightsFullAt = 20f;
        static readonly Color WindowGlow = UiKit.Hex("#FFD98A");
        static readonly Color BulbGlow = UiKit.Hex("#FFE9B0");
        const float FarHaze = 0.35f;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        enum Kind { SkyTop, SkyBottom, Cloud, Window, Lantern, Bulb, Near, Far }

        class Part
        {
            public Renderer r;
            public Kind kind;
            public Color baseColor;
        }

        readonly List<Part> parts = new List<Part>();
        MaterialPropertyBlock block;
        Func<float> hourSource;
        float shownHour = float.NaN;

        /// <summary>Gio dang ve (sau lan cap nhat gan nhat).</summary>
        public float ShownHour { get { return shownHour; } }
        public Color SkyTop { get; private set; }
        public Color SkyBottom { get; private set; }
        /// <summary>0 ban ngay .. 1 den sang han.</summary>
        public float Lights { get; private set; }

        public void Init(Func<float> hour)
        {
            hourSource = hour;
            block = new MaterialPropertyBlock();
            parts.Clear();
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                Material m = r.sharedMaterial;
                if (m == null || !m.HasProperty(ColorId)) continue;
                parts.Add(new Part { r = r, kind = Classify(r.transform), baseColor = m.GetColor(ColorId) });
            }
            Apply(Hour());
        }

        static Kind Classify(Transform t)
        {
            string n = t.name;
            if (n == "TroiTren") return Kind.SkyTop;
            if (n == "TroiDuoi") return Kind.SkyBottom;
            if (n.StartsWith("May_", StringComparison.Ordinal)) return Kind.Cloud;
            if (n.Contains("_CuaSo")) return Kind.Window;
            if (n.StartsWith("LongPho_", StringComparison.Ordinal)) return Kind.Lantern;
            if (n == "BongDenDuong") return Kind.Bulb;
            for (Transform p = t.parent; p != null; p = p.parent)
                if (p.name == "Xa") return Kind.Far;
            return Kind.Near;
        }

        float Hour()
        {
            float h = hourSource != null ? hourSource() : 9f;
            return float.IsNaN(h) ? 9f : h;
        }

        void Update()
        {
            float h = Hour();
            // ~1 phut game moi lan ve lai la du muot, khong dat lai vai tram renderer moi khung
            if (float.IsNaN(shownHour) || Mathf.Abs(h - shownHour) > 0.015f) Apply(h);
        }

        /// <summary>Mau troi o mot gio (dung cho test va menu).</summary>
        public static void Sample(float hour, out Color top, out Color bottom, out Color ambient, out Color cloud)
        {
            hour = Mathf.Clamp(hour, Keys[0].hour, Keys[Keys.Length - 1].hour);
            int i = 0;
            while (i < Keys.Length - 2 && hour > Keys[i + 1].hour) i++;
            Key a = Keys[i], b = Keys[i + 1];
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(a.hour, b.hour, hour));
            top = Color.Lerp(a.top, b.top, t);
            bottom = Color.Lerp(a.bottom, b.bottom, t);
            ambient = Color.Lerp(a.ambient, b.ambient, t);
            cloud = Color.Lerp(a.cloud, b.cloud, t);
        }

        public static float LightsAt(float hour)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(LightsOnFrom, LightsFullAt, hour));
        }

        public void Apply(float hour)
        {
            if (block == null) block = new MaterialPropertyBlock();
            shownHour = hour;
            Color top, bottom, ambient, cloud;
            Sample(hour, out top, out bottom, out ambient, out cloud);
            SkyTop = top;
            SkyBottom = bottom;
            Lights = LightsAt(hour);
            foreach (Part p in parts)
            {
                if (p.r == null) continue;
                Color lit = Mul(p.baseColor, ambient);
                Color c;
                switch (p.kind)
                {
                    case Kind.SkyTop: c = top; break;
                    case Kind.SkyBottom: c = bottom; break;
                    case Kind.Cloud: c = cloud; break;
                    case Kind.Window: c = Color.Lerp(Color.Lerp(lit, bottom, FarHaze * FarFactor(p)), WindowGlow, Lights); break;
                    case Kind.Lantern: c = Color.Lerp(lit, Brighten(p.baseColor), Lights); break;
                    case Kind.Bulb: c = Color.Lerp(lit, BulbGlow, Lights); break;
                    case Kind.Far: c = Color.Lerp(lit, bottom, FarHaze); break;
                    default: c = lit; break;
                }
                c.a = p.baseColor.a;
                p.r.GetPropertyBlock(block);
                block.SetColor(ColorId, c);
                p.r.SetPropertyBlock(block);
            }
        }

        static float FarFactor(Part p)
        {
            for (Transform t = p.r.transform.parent; t != null; t = t.parent)
                if (t.name == "Xa") return 1f;
            return 0f;
        }

        static Color Mul(Color a, Color b) { return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a); }

        static Color Brighten(Color c) { return Color.Lerp(c, Color.white, 0.25f); }

        /// <summary>Mau dang dat tren mot renderer cua pho (cho test).</summary>
        public Color ColorOf(string objectName)
        {
            foreach (Part p in parts)
                if (p.r != null && p.r.name == objectName)
                {
                    p.r.GetPropertyBlock(block);
                    Color c = block.GetColor(ColorId);
                    return c;
                }
            return Color.clear;
        }
    }
}
