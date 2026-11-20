using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Tranh phac cot truyen (F-051; impl plan sprint 6 muc 4: "nhan vat khoi + phong mau", tranh dep o sprint 9-10).
    /// Moi khung ve bang hinh tron / chu nhat bo goc cua UiKit (vien muc, mau phang Art Bible muc 3), mat dung chung FaceArt.
    /// Chu tren tranh (bien quan, lich no) la TextMeshPro (constitution Dieu 6).
    /// </summary>
    public static class StoryArt
    {
        static readonly Color HopeShirt = UiKit.Hex("#9CC7D8");
        static readonly Color Skin = UiKit.Hex("#FBE0C3");
        static readonly Color Vest = UiKit.Hex("#4A3F3A");
        static readonly Color Gold = UiKit.Hex("#F2C14E");
        static readonly Color Pink = UiKit.Hex("#E9A0A0");
        static readonly Color Wood = UiKit.Hex("#B97F52");
        static readonly Color WoodLight = UiKit.Hex("#D69B68");
        static readonly Color Paper = UiKit.Hex("#E3CFA6");
        static readonly Color Grey = UiKit.Hex("#C9C1B6");
        static readonly Color Leaf = UiKit.Hex("#7DB87A");

        /// <summary>Mau phong cua tung canh.</summary>
        public static Color Backdrop(StoryScene scene)
        {
            switch (scene)
            {
                case StoryScene.Office: return UiKit.Hex("#C9D6DC");
                case StoryScene.Home: return UiKit.Hex("#E8C9A0");
                case StoryScene.Begging: return UiKit.Hex("#D9B38C");
                case StoryScene.Keys: return UiKit.Hex("#E8C9A0");
                case StoryScene.Schedule: return UiKit.Hex("#F2E3C6");
                case StoryScene.Shop: return UiKit.Hex("#F5D9A8");
                case StoryScene.Wedding: return UiKit.Hex("#F6D6D6");
                default: return UiKit.Hex("#8C8279");
            }
        }

        /// <summary>Ve canh vao khung <paramref name="area"/> (he toa do 1000 x 500, goc duoi trai la 0,0).</summary>
        public static void Draw(RectTransform area, StoryScene scene, Balance balance)
        {
            UiKit.Clear(area);
            Image bg = Rect(area, "Phong", new Vector2(500f, 250f), new Vector2(1000f, 500f), Backdrop(scene));
            bg.raycastTarget = false;
            Rect(area, "San", new Vector2(500f, 45f), new Vector2(1000f, 90f), Shade(Backdrop(scene), 0.82f));

            switch (scene)
            {
                case StoryScene.Office:
                    for (int i = 0; i < 3; i++)
                    {
                        Image b = Rect(area, "ToaNha_" + i, new Vector2(140f + i * 170f, 250f + i * 20f), new Vector2(150f, 360f + i * 40f), Grey);
                        for (int w = 0; w < 6; w++)
                            Rect(b.transform, "Cua_" + w, new Vector2(40f + (w % 2) * 70f, 80f + (w / 2) * 90f), new Vector2(40f, 50f), UiKit.Hex("#9CC7D8"), false);
                    }
                    Hope(area, new Vector2(720f, 90f), 1f, 0);
                    Rect(area, "ThungCacTong", new Vector2(720f, 180f), new Vector2(150f, 90f), WoodLight);
                    Rect(area, "ButTrongThung", new Vector2(690f, 235f), new Vector2(14f, 50f), UiKit.Coral, false);
                    Circle(area, "CayTrongThung", new Vector2(750f, 240f), new Vector2(40f, 40f), Leaf);
                    break;

                case StoryScene.Home:
                    Rect(area, "Nha", new Vector2(300f, 230f), new Vector2(360f, 300f), UiKit.Cream);
                    Image roof = Rect(area, "Mai", new Vector2(300f, 400f), new Vector2(260f, 260f), UiKit.Coral);
                    roof.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);
                    roof.transform.SetSiblingIndex(2);
                    Rect(area, "Cua", new Vector2(300f, 160f), new Vector2(110f, 170f), Wood);
                    Boss(area, new Vector2(470f, 90f), 1f, false);
                    Rect(area, "SoNo", new Vector2(540f, 230f), new Vector2(60f, 80f), Vest);
                    Hope(area, new Vector2(800f, 90f), 0.9f, 0);
                    Rect(area, "ThungCacTong", new Vector2(800f, 165f), new Vector2(130f, 80f), WoodLight);
                    break;

                case StoryScene.Begging:
                    Boss(area, new Vector2(330f, 90f), 1.1f, false);
                    Hope(area, new Vector2(650f, 70f), 0.75f, 0);   // quy goi: thap xuong
                    for (int i = 0; i < 3; i++) Circle(area, "GiotNuocMat_" + i, new Vector2(600f - i * 25f, 260f - i * 30f), new Vector2(18f, 24f), UiKit.Hex("#8EC3E6"));
                    break;

                case StoryScene.Keys:
                    Boss(area, new Vector2(300f, 90f), 1f, false);
                    Hope(area, new Vector2(720f, 90f), 1f, 3);
                    Circle(area, "VongChiaKhoa", new Vector2(520f, 330f), new Vector2(70f, 70f), Gold);
                    Circle(area, "LoVong", new Vector2(520f, 330f), new Vector2(38f, 38f), Backdrop(scene));
                    for (int i = 0; i < 3; i++)
                    {
                        Image k = Rect(area, "ChiaKhoa_" + i, new Vector2(520f + (i - 1) * 34f, 270f), new Vector2(16f, 80f), Gold);
                        k.rectTransform.localEulerAngles = new Vector3(0f, 0f, (i - 1) * 20f);
                    }
                    break;

                case StoryScene.Schedule:
                    Boss(area, new Vector2(140f, 90f), 0.85f, false);
                    DebtEntry[] debts = balance != null ? balance.debts : new DebtEntry[0];
                    for (int i = 0; i < debts.Length; i++)
                    {
                        Image card = Rect(area, "LichNo_" + i, new Vector2(370f + i * 170f, 280f), new Vector2(150f, 190f), UiKit.Cream);
                        Rect(card.transform, "Dau", new Vector2(75f, 170f), new Vector2(150f, 40f), UiKit.Coral, false);
                        TextMeshProUGUI t = UiKit.Label(card.transform, "Chu", new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(140f, 140f),
                            "Ngày " + debts[i].day + "\n<size=30>" + StoryScript.Money(debts[i].amount) + "</size>", 24f);
                        t.lineSpacing = -10f;
                    }
                    break;

                case StoryScene.Shop:
                    Rect(area, "TuongQuan", new Vector2(500f, 260f), new Vector2(620f, 340f), UiKit.Cream);
                    Image sign = Rect(area, "BienQuan", new Vector2(500f, 405f), new Vector2(420f, 70f), Wood);
                    TextMeshProUGUI st = UiKit.Text(sign.transform, RoomView.ShopName, 34f);
                    st.color = UiKit.Cream;
                    for (int i = 0; i < 4; i++)
                        Rect(area, "Rem_" + i, new Vector2(335f + i * 110f, 330f), new Vector2(100f, 70f), i % 2 == 0 ? UiKit.Coral : UiKit.Hex("#D9564A"));
                    Rect(area, "Cua", new Vector2(500f, 190f), new Vector2(200f, 200f), Wood);
                    Hope(area, new Vector2(760f, 90f), 1f, 3);
                    break;

                case StoryScene.Wedding:
                    Hope(area, new Vector2(400f, 90f), 1f, 3);
                    Daughter(area, new Vector2(600f, 90f), 0.95f);
                    Boss(area, new Vector2(850f, 90f), 0.85f, true);
                    for (int i = 0; i < 7; i++)
                        Circle(area, "TraiTim_" + i, new Vector2(200f + i * 110f, 420f + (i % 2) * 35f), new Vector2(34f, 34f), i % 2 == 0 ? UiKit.Coral : Pink);
                    break;

                case StoryScene.Closed:
                    Rect(area, "TuongQuan", new Vector2(420f, 260f), new Vector2(560f, 340f), Shade(UiKit.Cream, 0.8f));
                    Rect(area, "Cua", new Vector2(420f, 190f), new Vector2(200f, 200f), Shade(Wood, 0.8f));
                    Image tag = Rect(area, "BienDong", new Vector2(420f, 260f), new Vector2(220f, 70f), UiKit.Coral);
                    tag.rectTransform.localEulerAngles = new Vector3(0f, 0f, -8f);
                    TextMeshProUGUI ct = UiKit.Text(tag.transform, "ĐÓNG CỬA", 34f);
                    ct.color = UiKit.Cream;
                    Boss(area, new Vector2(640f, 90f), 1f, false);
                    Hope(area, new Vector2(850f, 70f), 0.8f, 0);
                    break;
            }
        }

        // ---------------- nhan vat ----------------

        /// <summary>Hope: ao xanh, dau tron, mat theo bac (0 = buon, 3 = vui).</summary>
        static void Hope(RectTransform area, Vector2 feet, float k, int mood)
        {
            Rect(area, "Hope_Than", feet + new Vector2(0f, 85f) * k, new Vector2(150f, 170f) * k, HopeShirt);
            Head(area, "Hope", feet + new Vector2(0f, 230f) * k, 150f * k, FaceArt.Ingredient(mood));
            Circle(area, "Hope_Toc", feet + new Vector2(0f, 300f) * k, new Vector2(120f, 46f) * k, UiKit.Hex("#5A4636"));
        }

        static void Boss(RectTransform area, Vector2 feet, float k, bool surprised)
        {
            Rect(area, "ChuNo_Than", feet + new Vector2(0f, 95f) * k, new Vector2(170f, 190f) * k, Vest);
            for (int i = 0; i < 5; i++)
                Circle(area, "ChuNo_DayVang_" + i, feet + new Vector2((i - 2) * 22f, 165f - Mathf.Abs(i - 2) * 6f) * k, new Vector2(14f, 14f) * k, Gold);
            Head(area, "ChuNo", feet + new Vector2(0f, 250f) * k, 150f * k, FaceArt.Boss(surprised));
            Rect(area, "ChuNo_VanhMu", feet + new Vector2(0f, 318f) * k, new Vector2(150f, 16f) * k, Vest);
            Rect(area, "ChuNo_Mu", feet + new Vector2(0f, 360f) * k, new Vector2(100f, 76f) * k, Vest);
            Rect(area, "ChuNo_BangMu", feet + new Vector2(0f, 334f) * k, new Vector2(102f, 14f) * k, Gold, false);
        }

        static void Daughter(RectTransform area, Vector2 feet, float k)
        {
            Rect(area, "ConGai_Than", feet + new Vector2(0f, 85f) * k, new Vector2(160f, 170f) * k, Pink);
            Circle(area, "ConGai_BuiToc", feet + new Vector2(0f, 320f) * k, new Vector2(70f, 70f) * k, UiKit.Hex("#5A4636"));
            Head(area, "ConGai", feet + new Vector2(0f, 230f) * k, 150f * k, FaceArt.Ingredient(4));
            Circle(area, "ConGai_Hoa", feet + new Vector2(55f, 290f) * k, new Vector2(30f, 30f) * k, Gold);
        }

        static void Head(RectTransform area, string who, Vector2 center, float size, Texture2D face)
        {
            Circle(area, who + "_Dau", center, new Vector2(size, size * 0.92f), Skin);
            if (face == null) return;
            RectTransform rt = UiKit.Box(area, who + "_Mat", Vector2.zero, center, new Vector2(size * 0.8f, size * 0.5f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = center;
            var img = rt.gameObject.AddComponent<RawImage>();
            img.texture = face;
            img.raycastTarget = false;
        }

        // ---------------- hinh ----------------

        static Image Rect(Transform parent, string name, Vector2 center, Vector2 size, Color color, bool outline = true)
        {
            RectTransform rt = UiKit.Box(parent, name, Vector2.zero, center, size);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = center;
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiKit.Rounded;
            img.type = Image.Type.Sliced;
            img.color = outline ? color : color;
            if (!outline) img.sprite = null;
            img.raycastTarget = false;
            return img;
        }

        static Image Circle(Transform parent, string name, Vector2 center, Vector2 size, Color color)
        {
            RectTransform rt = UiKit.Box(parent, name, Vector2.zero, center, size);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = center;
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiKit.Circle;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static Color Shade(Color c, float k)
        {
            return new Color(c.r * k, c.g * k, c.b * k, c.a);
        }
    }
}
