using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Bo UI theo Art Bible muc 4 va 13: khung bo goc, vien 4 px #2B2321, bong dac lech xuong 5 px,
    /// chu Baloo 2, mau nut theo vai tro, sau mau danh rieng cho bac. Day la quy cach hinh anh, khong phai so can bang.
    /// </summary>
    public static class UiKit
    {
        public static readonly Color Ink = Hex("#2B2321");
        public static readonly Color Cream = Hex("#FFF3DC");
        public static readonly Color Coral = Hex("#E8604F");
        public static readonly Color Mustard = Hex("#F2C14E");
        public static readonly Color Mint = Hex("#8CC9AE");
        public static readonly Color Lilac = Hex("#C9B3F0");
        public static readonly Color Disabled = Hex("#D9CFC0");

        /// <summary>Mau bac nguyen lieu 0..5 (Art Bible: Bac_NguyenLieu).</summary>
        public static readonly Color[] IngredientTier =
        {
            Hex("#A39064"), Hex("#FFFFFF"), Hex("#8CC9AE"), Hex("#9CC7D8"), Hex("#C9B3F0"), Hex("#F2C14E"),
        };

        /// <summary>Mau bac mon an 0..5 (Art Bible: Bac_MonAn).</summary>
        public static readonly Color[] DishTier =
        {
            Hex("#A39064"), Hex("#D9CFC0"), Hex("#FFFFFF"), Hex("#8CC9AE"), Hex("#C9B3F0"), Hex("#F2C14E"),
        };

        public static readonly string[] IngredientTierName = { "Ôi thiu", "Bình thường", "Tốt", "Tươi", "Xuất sắc", "Hiếm" };
        public static readonly string[] DishTierName = { "Lạc vị", "Dở", "Bình thường", "Ngon", "Tuyệt hảo", "Huyền thoại" };

        public const float MinFontSize = 14f;      // Art Bible muc 13: toi thieu 14 px o 1280x720
        public const float NumberFontSize = 26f;   // con so quan trong toi thieu 26 px
        public const float ButtonMinHeight = 52f;

        const string FontResource = "Fonts/Baloo2-Bold";
        const int SpriteSize = 64;
        const int SpriteRadius = 20;
        const int SpriteBorder = 4;
        const int SpriteSlice = 26;
        const float ShadowDrop = 5f;
        const float ChipLineSpacing = -30f;

        static TMP_FontAsset font;
        static Sprite rounded;

        public static TMP_FontAsset Font
        {
            get
            {
                if (font != null) return font;
                Font source = Resources.Load<Font>(FontResource);
                if (source != null)
                    font = TMP_FontAsset.CreateFontAsset(source, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                        1024, 1024, AtlasPopulationMode.Dynamic);
                if (font == null)
                {
                    Debug.LogWarning("Quán Ăn Hope: không tạo được font Baloo 2, dùng font mặc định của TextMeshPro.");
                    font = TMP_Settings.defaultFontAsset;
                }
                else font.name = "Baloo2-Bold (dynamic)";
                return font;
            }
        }

        /// <summary>Hinh chu nhat bo goc: ruot trang, vien muc; to mau bang Image.color.</summary>
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                var tex = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);
                tex.name = "UI_Rounded";
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                var pixels = new Color32[SpriteSize * SpriteSize];
                float half = SpriteSize * 0.5f;
                for (int y = 0; y < SpriteSize; y++)
                {
                    for (int x = 0; x < SpriteSize; x++)
                    {
                        // khoang cach co dau toi mep hinh chu nhat bo goc (am = ben trong)
                        float qx = Mathf.Abs(x + 0.5f - half) - (half - SpriteRadius);
                        float qy = Mathf.Abs(y + 0.5f - half) - (half - SpriteRadius);
                        float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                        float d = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - SpriteRadius;

                        float alpha = Mathf.Clamp01(0.5f - d);
                        float fill = Mathf.Clamp01(-d - SpriteBorder + 0.5f);
                        Color c = Color.Lerp(Ink, Color.white, fill);
                        c.a = alpha;
                        pixels[y * SpriteSize + x] = c;
                    }
                }
                tex.SetPixels32(pixels);
                tex.Apply();
                rounded = Sprite.Create(tex, new Rect(0, 0, SpriteSize, SpriteSize), new Vector2(0.5f, 0.5f), 100f, 0,
                    SpriteMeshType.FullRect, new Vector4(SpriteSlice, SpriteSlice, SpriteSlice, SpriteSlice));
                rounded.name = "UI_Rounded";
                return rounded;
            }
        }

        public static Color Hex(string hex)
        {
            Color c;
            return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
        }

        /// <summary>Khung co neo, tam va kich thuoc tinh theo he 1280x720.</summary>
        public static RectTransform Box(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color, bool shadow = true)
        {
            RectTransform rt = Box(parent, name, anchor, pos, size);
            return Decorate(rt, color, shadow);
        }

        static Image Decorate(RectTransform rt, Color color, bool shadow)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Rounded;
            img.type = Image.Type.Sliced;
            img.color = color;
            if (shadow)
            {
                var s = rt.gameObject.AddComponent<Shadow>();
                s.effectColor = Ink;
                s.effectDistance = new Vector2(0f, -ShadowDrop);
                s.useGraphicAlpha = true;
            }
            return img;
        }

        /// <summary>Chu phu kin khung cha (tru le).</summary>
        public static TextMeshProUGUI Text(Transform parent, string text, float size, TextAlignmentOptions align = TextAlignmentOptions.Center, float padding = 10f)
        {
            RectTransform rt = Stretch(parent, "Text");
            rt.offsetMin = new Vector2(padding, padding * 0.5f);
            rt.offsetMax = new Vector2(-padding, -padding * 0.5f);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.enableAutoSizing = true;
            t.fontSizeMax = size;
            t.fontSizeMin = Mathf.Min(size, MinFontSize);
            t.color = Ink;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Chu tu do (khong nen).</summary>
        public static TextMeshProUGUI Label(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, string text, float fontSize,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            RectTransform rt = Box(parent, name, anchor, pos, size);
            TextMeshProUGUI t = Text(rt, text, fontSize, align, 0f);
            return t;
        }

        public static Button Button(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, string label, Color color,
            UnityAction onClick, float fontSize = 22f)
        {
            Image img = Panel(parent, name, anchor, pos, new Vector2(size.x, Mathf.Max(size.y, ButtonMinHeight)), color);
            return MakeButton(img, label, color, onClick, fontSize);
        }

        /// <summary>O trong luoi (kich thuoc do layout group quyet dinh). onClick = null thi chi la nhan.</summary>
        public static Image Chip(Transform parent, string name, string label, Color color, UnityAction onClick, float fontSize = 16f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image img = Decorate((RectTransform)go.transform, color, false);
            if (onClick != null) MakeButton(img, label, color, onClick, fontSize);
            else
            {
                img.raycastTarget = false;
                Text(img.transform, label, fontSize, TextAlignmentOptions.Center, 6f);
            }
            // Baloo 2 co khoang dong rong; o hai dong can ep lai de chu khong tran ra ngoai o.
            img.GetComponentInChildren<TextMeshProUGUI>().lineSpacing = ChipLineSpacing;
            return img;
        }

        static Button MakeButton(Image img, string label, Color color, UnityAction onClick, float fontSize)
        {
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.disabledColor = new Color(0.72f, 0.72f, 0.72f, 0.75f);
            b.colors = cb;
            if (onClick != null) b.onClick.AddListener(onClick);
            TextMeshProUGUI t = Text(img.transform, label, fontSize, TextAlignmentOptions.Center, 8f);
            if (color == Coral) t.color = Cream;
            return b;
        }

        public static GridLayoutGroup Grid(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, int columns, Vector2 cell, float spacing = 6f)
        {
            RectTransform rt = Box(parent, name, anchor, pos, size);
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = columns;
            g.cellSize = cell;
            g.spacing = new Vector2(spacing, spacing);
            g.childAlignment = TextAnchor.UpperCenter;
            return g;
        }

        /// <summary>Luoi cuon doc duoc khi so o vuot qua khung. Tra ve noi chua cac o.</summary>
        public static Transform ScrollGrid(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, int columns, Vector2 cell, float spacing = 6f)
        {
            RectTransform view = Box(parent, name + "_Khung", anchor, pos, size);
            view.gameObject.AddComponent<RectMask2D>();
            var catcher = view.gameObject.AddComponent<Image>();   // trong suot, de bat thao tac keo / lan chuot
            catcher.color = new Color(0f, 0f, 0f, 0f);

            RectTransform content = Box(view, name, Top01, Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.sizeDelta = Vector2.zero;
            var g = content.gameObject.AddComponent<GridLayoutGroup>();
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = columns;
            g.cellSize = cell;
            g.spacing = new Vector2(spacing, spacing);
            g.childAlignment = TextAnchor.UpperCenter;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = view;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            return content;
        }

        static readonly Vector2 Top01 = new Vector2(0.5f, 1f);

        /// <summary>Xoa moi o trong mot luoi truoc khi dung lai.</summary>
        public static void Clear(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Transform child = container.GetChild(i);
                child.SetParent(null, false);
                Object.Destroy(child.gameObject);
            }
        }

        public static void SetLabel(Component buttonOrPanel, string text)
        {
            var t = buttonOrPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (t != null) t.text = text;
        }
    }

    /// <summary>Thanh tien do: nen kem, ruot mau, dai theo gia tri 0..1.</summary>
    public class ProgressBar : MonoBehaviour
    {
        RectTransform fill;

        public static ProgressBar Create(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            Image back = UiKit.Panel(parent, name, anchor, pos, size, UiKit.Cream, false);
            back.raycastTarget = false;
            var bar = back.gameObject.AddComponent<ProgressBar>();
            RectTransform f = UiKit.Stretch(back.transform, "Fill");
            f.offsetMin = new Vector2(4f, 4f);
            f.offsetMax = new Vector2(-4f, -4f);
            var img = f.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            bar.fill = f;
            bar.Set(0f);
            return bar;
        }

        public float Value { get; private set; }

        public void Set(float value01)
        {
            Value = Mathf.Clamp01(value01);
            fill.anchorMax = new Vector2(Value, 1f);
            fill.offsetMax = new Vector2(Value > 0f ? -4f : 0f, -4f);
            fill.gameObject.SetActive(Value > 0f);
        }
    }

    /// <summary>Nut giu: dang giu thi IsHeld = true (so che tren thot, constitution muc 5.1).</summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsHeld { get; private set; }

        public void Press() { IsHeld = true; }
        public void Release() { IsHeld = false; }

        public void OnPointerDown(PointerEventData eventData) { Press(); }
        public void OnPointerUp(PointerEventData eventData) { Release(); }
        public void OnPointerExit(PointerEventData eventData) { Release(); }
        void OnDisable() { Release(); }
    }
}
