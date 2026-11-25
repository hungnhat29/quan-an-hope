using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Tranh huong dan (yeu cau 22/11/2026, "Phong cach huong dan bang hinh.md"): moi the mot hanh dong + ket qua, mot diem
    /// nhan, chu chu thich <= 4 tu duoi tranh. Icon nguyen lieu / dung cu / mon chup tu model trong phong (IconBook) nen trung
    /// voi vat that nguoi choi thay. Khuon A (buoc lam), khuon C (dung / sai / thuong) cho khai niem.
    /// </summary>
    public static class GuidePics
    {
        // id tranh buoc lam (khuon A)
        public const string Kiosk = "nhet_ve", Take = "lay_mon", Prep = "so_che", Tool = "dung_cu", Menu = "thuc_don", Order = "thu_tu",
            Knob = "vat_num", Plate = "muc_dia", Open = "mo_quan", Serve = "phuc_vu", Close = "dong_quan", Light = "bat_den",
            FreeRelic = "relic_free", TabRelic = "tab_relic", TabShop = "tab_shop", Fridge = "tu_lanh", Drop = "tha_do", Boss = "chu_no", Dark = "mat_dien",
            // sau sprint 10 (yeu cau 23/11/2026): mua relic bang ve (bai relic doi sang dau ngay 3), cam lo sot nho len dia
            BuyRelic = "relic_ve", Sauce = "nuoc_sot";

        // id tranh khai niem
        public const string Tiers = "bac_nl", DishTier = "bac_mon", Price = "gia", Easy = "de_tinh", Patience = "cho_lau", WrongRight = "dung_sai",
            Overheat = "qua_lua", Pull = "ti_le",
            // sau sprint 10: nuoc sot co the hong / ra Huyen thoai; goi y mon cuoi cho chu no (Trung chien ca chua bac Huyen thoai)
            SauceLuck = "sot_may_rui", BossBest = "mon_ngon_nhat";

        /// <summary>Moi tranh co trong the "Cach choi" (Tab), theo thu tu doc.</summary>
        public static readonly string[] Sheet = { Kiosk, Prep, Order, Serve, Close, Tiers, DishTier, Price, Easy, WrongRight, Patience, Overheat };

        /// <summary>Chu chu thich (<= 4 tu) cua tranh.</summary>
        public static string Caption(string id)
        {
            switch (id)
            {
                case Kiosk: return "Nhét vé";
                case Take: return "Lấy 1 món";
                case Prep: return "Giữ chuột: sơ chế";
                case Tool: return "Chọn dụng cụ";
                case Menu: return "Tab: xem thực đơn";
                case Order: return "Cho đúng thứ tự";
                case Knob: return "Vặn núm";
                case Plate: return "Múc ra đĩa";
                case Open: return "Mở quán";
                case Serve: return "Đặt trước mặt khách";
                case Close: return "Đóng quán";
                case Light: return "Bật đèn";
                case FreeRelic: return "Chọn 1 relic";
                case TabRelic: return "Tab: relic";
                case TabShop: return "Tab: cửa hàng";
                case Fridge: return "Cất tủ lạnh";
                case Drop: return "Chuột phải: thả";
                case Boss: return "Món Tuyệt hảo";
                case Dark: return "Mất điện";
                case Tiers: return "Màu đế = bậc";
                case DishTier: return "Đồ tốt → món ngon";
                case Price: return "Món ngon: đắt hơn";
                case Easy: return "Khách khó: cần ngon";
                case Patience: return "Chờ lâu: bỏ đi";
                case WrongRight: return "Đúng món mới trả";
                case Overheat: return "Khói: múc ngay";
                case Pull: return "Vòng sau: đồ tốt hơn";
                case BuyRelic: return "Trả vé: mua relic";
                case Sauce: return "Cầm lọ, nhỏ sốt";
                case SauceLuck: return "Thường lên bậc";
                case BossBest: return "Món ngon nhất?";
                default: return "";
            }
        }

        /// <summary>Dung the tranh <paramref name="id"/> (khung + hinh + chu) trong parent. Tra ve khung the.</summary>
        public static RectTransform Card(GameController g, Transform parent, string id, Vector2 anchor, Vector2 pos, Vector2 size, int number = 0)
        {
            Color bg, dot;
            Theme(id, out bg, out dot);
            RectTransform pic = PicKit.Frame(parent, "Tranh_" + id, anchor, pos, size, number, Caption(id), null, bg, dot);
            Draw(g, pic, id);
            return (RectTransform)pic.parent;
        }

        static Vector2 P(RectTransform pic, float fx, float fy)
        {
            Rect r = pic.rect;
            return new Vector2((fx - 0.5f) * r.width, (fy - 0.5f) * r.height);
        }

        static float S(RectTransform pic, float f) { return Mathf.Min(pic.rect.width, pic.rect.height) * f; }

        static readonly Color Wood = UiKit.Hex("#D69B68"), Skin = UiKit.Hex("#F6D1B1"), Paper = UiKit.Hex("#FBF0DA");

        /// <summary>Cap mon mau: mon dau tien lam duoc tu kho, khong co thi trung chien ca chua.</summary>
        static Recipe Sample(GameController g)
        {
            GameSession s = g != null ? g.Session : null;
            if (s == null) return null;
            List<Recipe> can = s.MakeableRecipes();
            if (can.Count > 0) return can[0];
            Recipe r = s.Lookup.ById("trung__ca_chua");
            return r ?? (s.Recipes.recipes.Length > 0 ? s.Recipes.recipes[0] : null);
        }

        static Texture2D IconOf(GameController g, string step, Recipe r)
        {
            if (r == null) return null;
            foreach (SideIngredient x in g.Session.Recipes.sides) if (x.name == step) return IconBook.Ingredient(x.id, true);
            foreach (MainIngredient x in g.Session.Recipes.mains) if (x.name == step) return IconBook.Ingredient(x.id, true);
            return IconBook.Jar(step, JarColor(g, step));
        }

        static Color JarColor(GameController g, string item)
        {
            Color c;
            return g.Room != null && g.Room.JarColor(item, out c) ? c : UiKit.Cream;
        }

        // ---------------- paper-ui-pickit.md: nhom mau nen cua so, luoi 3 cot ----------------

        /// <summary>Tam cot (18% / 50% / 82%) va tam hang (2 hang: 72% / 26% tinh tu duoi) cua cua so hinh.</summary>
        static readonly float[] ColX = { 0.18f, 0.5f, 0.82f };
        static readonly float[] RowY = { 0.72f, 0.26f };

        /// <summary>Mau nen cua so / cham tuong theo nhom the (muc 5).</summary>
        public static void Theme(string id, out Color bg, out Color dot)
        {
            switch (id)
            {
                case Prep: case DishTier: case Take:
                    bg = UiKit.Hex("#E6EFD9"); dot = UiKit.Hex("#D3E3C1"); return;
                case Serve: case Patience: case Open: case Boss: case BossBest:
                    bg = UiKit.Hex("#F7E3D6"); dot = UiKit.Hex("#EECBB8"); return;
                case Close: case Easy: case Light: case Dark:
                    bg = UiKit.Hex("#EAE2F5"); dot = UiKit.Hex("#D8CBEE"); return;
                case Overheat: case Tool: case Knob: case Plate: case Sauce: case SauceLuck: case Fridge:
                    bg = UiKit.Hex("#E3ECF0"); dot = UiKit.Hex("#CCDDE5"); return;
                default:
                    bg = UiKit.Hex("#F3DFB8"); dot = UiKit.Hex("#E9CE9E"); return;
            }
        }

        /// <summary>Dong xu "G" mu tat (gia, tien).</summary>
        static Image Coin(Transform parent, Vector2 pos, float size)
        {
            Image c = PicKit.Dot(parent, pos, size, UiKit.Mustard);
            TMPro.TextMeshProUGUI t = UiKit.Text(c.transform, "G", Mathf.Max(10f, size * 0.5f), TMPro.TextAlignmentOptions.Center, 0f);
            t.enableAutoSizing = false;
            return c;
        }

        static void Draw(GameController g, RectTransform pic, string id)
        {
            Recipe r = Sample(g);
            float u = S(pic, 1f);
            // vien thuoc rong co dinh theo cot (muc 3): khong rong hon 30% cua so (3 cot) de khong cham cot ben
            float pillW = Mathf.Min(u * 0.58f, pic.rect.width * 0.3f), pillH = u * 0.15f, font = Mathf.Min(u * 0.078f, pillW * 0.16f);
            switch (id)
            {
                case Kiosk:
                {
                    Image ticket = PicKit.Block(pic, P(pic, 0.15f, 0.52f), new Vector2(u * 0.4f, u * 0.22f), UiKit.Mustard, "VÉ", u * 0.12f);
                    PaperKit.AddShadow(ticket, u / 160f);
                    PicKit.Arrow(pic, P(pic, 0.31f, 0.52f), P(pic, 0.44f, 0.52f), u * 0.06f);
                    Image m = PicKit.Dot(pic, P(pic, 0.58f, 0.52f), u * 0.46f, UiKit.Coral);
                    PaperKit.AddShadow(m, u / 160f);
                    PicKit.Block(m.transform, new Vector2(0f, -u * 0.05f), new Vector2(u * 0.22f, u * 0.05f), UiKit.Ink);
                    PicKit.Hand(pic, P(pic, 0.6f, 0.45f), u * 0.4f, GhostHand.Gesture.Tap);
                    PicKit.Chevron(pic, P(pic, 0.75f, 0.52f), u * 0.18f);
                    string side = r != null ? r.sideId : "ca_chua", main = r != null ? r.mainId : "trung";
                    Image c1 = PicKit.Block(pic, P(pic, 0.88f, 0.66f), new Vector2(u * 0.34f, u * 0.17f), Wood);
                    PicKit.Icon(c1.transform, IconBook.Ingredient(side), new Vector2(0f, u * 0.1f), u * 0.26f);
                    Image c2 = PicKit.Block(pic, P(pic, 0.88f, 0.26f), new Vector2(u * 0.34f, u * 0.17f), UiKit.Hex("#B97F52"));
                    PicKit.Icon(c2.transform, IconBook.Ingredient(main), new Vector2(0f, u * 0.1f), u * 0.26f);
                    break;
                }
                case Take:
                {
                    string side = r != null ? r.sideId : "ca_chua";
                    Image crate = PicKit.Block(pic, P(pic, 0.27f, 0.45f), new Vector2(u * 0.62f, u * 0.36f), Wood);
                    PaperKit.AddShadow(crate, u / 160f);
                    for (int i = 0; i < 2; i++) PicKit.Icon(crate.transform, IconBook.Ingredient(side), new Vector2((i - 0.5f) * u * 0.22f, u * 0.1f), u * 0.24f);
                    PicKit.Hand(pic, P(pic, 0.3f, 0.5f), u * 0.44f, GhostHand.Gesture.Tap);
                    PicKit.Arrow(pic, P(pic, 0.52f, 0.5f), P(pic, 0.66f, 0.5f), u * 0.06f);
                    PicKit.Icon(pic, IconBook.Ingredient(side), P(pic, 0.82f, 0.5f), u * 0.56f);
                    break;
                }
                case Prep:
                {
                    string side = r != null ? r.sideId : "ca_chua";
                    PicKit.Icon(pic, IconBook.Board(), P(pic, 0.27f, 0.42f), u * 0.7f);
                    PicKit.Icon(pic, IconBook.Ingredient(side), P(pic, 0.27f, 0.56f), u * 0.34f);
                    PicKit.Hand(pic, P(pic, 0.31f, 0.5f), u * 0.4f, GhostHand.Gesture.Hold);
                    PicKit.Arrow(pic, P(pic, 0.56f, 0.5f), P(pic, 0.69f, 0.5f), u * 0.06f);
                    PicKit.Icon(pic, IconBook.Ingredient(side, true), P(pic, 0.84f, 0.5f), u * 0.44f);
                    break;
                }
                case Tool:
                {
                    string want = r != null ? r.tool : RoomView.Tools[0];
                    for (int i = 0; i < RoomView.Tools.Length && i < 3; i++)
                    {
                        Vector2 at = P(pic, ColX[i], 0.58f);
                        if (RoomView.Tools[i] == want) PicKit.Dot(pic, at + new Vector2(0f, u * 0.06f), u * 0.5f, UiKit.Mustard);
                        PicKit.Column(pic, at, IconBook.Tool(RoomView.Tools[i]), u * 0.4f, RoomView.Tools[i], new Vector2(pillW * 0.85f, pillH), font, UiKit.Cream);
                        if (RoomView.Tools[i] == want) PicKit.Hand(pic, at + new Vector2(u * 0.05f, 0f), u * 0.34f, GhostHand.Gesture.Tap);
                    }
                    break;
                }
                case Menu:
                {
                    PicKit.Block(pic, P(pic, 0.18f, 0.52f), new Vector2(u * 0.36f, u * 0.22f), UiKit.Cream, "Tab", u * 0.12f);
                    PicKit.Arrow(pic, P(pic, 0.35f, 0.52f), P(pic, 0.48f, 0.52f), u * 0.06f);
                    Image page = PicKit.Block(pic, P(pic, 0.76f, 0.5f), new Vector2(u * 0.68f, u * 0.86f), Paper);
                    PaperKit.AddShadow(page, u / 160f);
                    if (r != null) PicKit.Icon(page.transform, IconBook.Dish(r, g.Session.Recipes), new Vector2(0f, u * 0.1f), u * 0.46f);
                    if (r != null) PicKit.Tag(page.transform, new Vector2(0f, -u * 0.26f), new Vector2(pillW * 0.8f, pillH), r.price + "G", font, UiKit.Mustard);
                    break;
                }
                case Order:
                {
                    // muc 1 the 3: so buoc thang tren tam tung hinh, dau › giua cac hinh, mot mui ten duy nhat vao chao
                    if (r == null) break;
                    int n = r.steps.Length;
                    float left = 0.09f, right = 0.7f;
                    float step = (right - left) / Mathf.Max(1, n - 1);
                    float icon = u * Mathf.Min(0.3f, 1.6f / Mathf.Max(4, n + 1));
                    for (int i = 0; i < n; i++)
                    {
                        Vector2 at = P(pic, left + step * i, 0.42f);
                        PicKit.Icon(pic, IconOf(g, r.steps[i], r), at, icon);
                        PicKit.Badge(pic, at + new Vector2(0f, icon * 0.5f + u * 0.12f), (i + 1).ToString(), UiKit.Mustard, u * 0.15f);
                        if (i < n - 1) PicKit.Chevron(pic, P(pic, left + step * (i + 0.5f), 0.42f), u * 0.14f);
                    }
                    PicKit.Arrow(pic, P(pic, 0.76f, 0.42f), P(pic, 0.84f, 0.42f), u * 0.05f);
                    PicKit.Icon(pic, IconBook.Tool(r.tool), P(pic, 0.92f, 0.42f), u * 0.3f);
                    break;
                }
                case Knob:
                {
                    Image k = PicKit.Dot(pic, P(pic, 0.3f, 0.5f), u * 0.5f, UiKit.Coral);
                    PaperKit.AddShadow(k, u / 160f);
                    PicKit.Block(k.transform, Vector2.zero, new Vector2(u * 0.06f, u * 0.36f), UiKit.Cream);
                    PicKit.Hand(pic, P(pic, 0.33f, 0.46f), u * 0.42f, GhostHand.Gesture.Turn);
                    PicKit.Arrow(pic, P(pic, 0.54f, 0.5f), P(pic, 0.67f, 0.5f), u * 0.06f);
                    if (r != null) PicKit.Column(pic, P(pic, 0.84f, 0.52f), IconBook.Tool(r.tool), u * 0.44f, Mathf.RoundToInt(r.cookSeconds) + "s",
                        new Vector2(pillW * 0.6f, pillH), font, UiKit.Mustard);
                    break;
                }
                case Plate:
                {
                    PicKit.Icon(pic, IconBook.Plate(), P(pic, 0.15f, 0.5f), u * 0.42f);
                    PicKit.Arrow(pic, P(pic, 0.28f, 0.5f), P(pic, 0.38f, 0.5f), u * 0.05f);
                    if (r != null) PicKit.Icon(pic, IconBook.Tool(r.tool), P(pic, 0.5f, 0.5f), u * 0.42f);
                    PicKit.Hand(pic, P(pic, 0.5f, 0.44f), u * 0.36f, GhostHand.Gesture.Tap);
                    PicKit.Chevron(pic, P(pic, 0.66f, 0.5f), u * 0.18f);
                    if (r != null) PicKit.Icon(pic, IconBook.Dish(r, g.Session.Recipes), P(pic, 0.83f, 0.5f), u * 0.48f);
                    break;
                }
                case Open:
                case Close:
                {
                    bool open = id == Open;
                    Image sign = PicKit.Dot(pic, P(pic, 0.27f, 0.5f), u * 0.62f, UiKit.Mustard);
                    PaperKit.AddShadow(sign, u / 160f);
                    Image inner = PicKit.Dot(sign.transform, Vector2.zero, u * 0.44f, UiKit.Coral);
                    PicKit.Block(inner.transform, Vector2.zero, new Vector2(u * 0.36f, u * 0.16f), UiKit.Cream, open ? "MỞ" : "ĐÓNG", u * 0.08f);
                    PicKit.Hand(pic, P(pic, 0.3f, 0.42f), u * 0.4f, GhostHand.Gesture.Tap);
                    PicKit.Arrow(pic, P(pic, 0.52f, 0.5f), P(pic, 0.65f, 0.5f), u * 0.06f);
                    if (open) PicKit.Face(pic, FaceArt.Customer(FaceArt.MoodHappy), P(pic, 0.82f, 0.5f), u * 0.44f, Skin);
                    else
                    {
                        // lich: dai do tren + "+1"
                        Image cal = PicKit.Block(pic, P(pic, 0.82f, 0.48f), new Vector2(u * 0.42f, u * 0.44f), UiKit.Cream, "+1", u * 0.16f);
                        PaperKit.AddShadow(cal, u / 160f);
                        PicKit.Block(cal.transform, new Vector2(0f, u * 0.17f), new Vector2(u * 0.42f, u * 0.1f), UiKit.Coral);
                    }
                    break;
                }
                case Serve:
                {
                    Texture2D dish = r != null ? IconBook.Dish(r, g.Session.Recipes) : null;
                    PicKit.Block(pic, P(pic, 0.5f, 0.2f), new Vector2(u * 1.7f, u * 0.14f), Wood);
                    PicKit.Icon(pic, dish, P(pic, 0.14f, 0.42f), u * 0.4f);
                    PicKit.Arrow(pic, P(pic, 0.28f, 0.42f), P(pic, 0.44f, 0.42f), u * 0.06f);
                    PicKit.Ghost(pic, P(pic, 0.6f, 0.38f), u * 0.34f);
                    PicKit.Face(pic, FaceArt.Customer(FaceArt.MoodWait), P(pic, 0.84f, 0.62f), u * 0.42f, Skin);
                    PicKit.Hand(pic, P(pic, 0.6f, 0.34f), u * 0.38f, GhostHand.Gesture.Tap);
                    break;
                }
                case Light:
                {
                    Image sw = PicKit.Block(pic, P(pic, 0.27f, 0.5f), new Vector2(u * 0.3f, u * 0.42f), UiKit.Cream);
                    PaperKit.AddShadow(sw, u / 160f);
                    PicKit.Block(sw.transform, new Vector2(0f, u * 0.05f), new Vector2(u * 0.09f, u * 0.16f), UiKit.Coral);
                    PicKit.Hand(pic, P(pic, 0.3f, 0.46f), u * 0.38f, GhostHand.Gesture.Tap);
                    PicKit.Arrow(pic, P(pic, 0.48f, 0.5f), P(pic, 0.62f, 0.5f), u * 0.06f);
                    PicKit.Dot(pic, P(pic, 0.8f, 0.5f), u * 0.46f, UiKit.Mustard);
                    break;
                }
                case FreeRelic:
                case BuyRelic:
                {
                    RelicDef sample = FirstRelic(g);
                    bool free = id == FreeRelic;
                    if (!free)
                    {
                        Image ticket = PicKit.Block(pic, P(pic, ColX[0], 0.52f), new Vector2(u * 0.36f, u * 0.2f), UiKit.Mustard, "VÉ", u * 0.11f);
                        PaperKit.AddShadow(ticket, u / 160f);
                        PicKit.Arrow(pic, P(pic, 0.31f, 0.52f), P(pic, 0.42f, 0.52f), u * 0.05f);
                    }
                    if (sample != null)
                        PicKit.Column(pic, P(pic, ColX[free ? 1 : 1] + (free ? 0f : 0.08f), 0.55f), IconBook.Relic(sample), u * 0.48f,
                            free ? "0 vé · lấy 1" : sample.price + " vé", new Vector2(pillW, pillH), font, free ? UiKit.Mint : UiKit.Mustard);
                    PicKit.Hand(pic, P(pic, free ? 0.5f : 0.58f, 0.55f), u * 0.36f, GhostHand.Gesture.Tap);
                    break;
                }
                case TabRelic:
                case TabShop:
                {
                    PicKit.Block(pic, P(pic, 0.18f, 0.52f), new Vector2(u * 0.36f, u * 0.22f), UiKit.Cream, "Tab", u * 0.12f);
                    PicKit.Arrow(pic, P(pic, 0.35f, 0.52f), P(pic, 0.48f, 0.52f), u * 0.06f);
                    Image page = PicKit.Block(pic, P(pic, 0.76f, 0.5f), new Vector2(u * 0.68f, u * 0.86f), Paper);
                    PaperKit.AddShadow(page, u / 160f);
                    if (id == TabRelic) { RelicDef sample = FirstRelic(g); if (sample != null) PicKit.Icon(page.transform, IconBook.Relic(sample), Vector2.zero, u * 0.5f); }
                    else
                    {
                        PicKit.Tag(page.transform, new Vector2(0f, u * 0.14f), new Vector2(pillW * 0.85f, pillH), "Mua", font, UiKit.Mustard);
                        PicKit.Tag(page.transform, new Vector2(0f, -u * 0.12f), new Vector2(pillW * 0.85f, pillH), "Mua", font, UiKit.Mustard);
                    }
                    break;
                }
                case Fridge:
                {
                    if (r != null) PicKit.Icon(pic, IconBook.Dish(r, g.Session.Recipes, 4), P(pic, 0.15f, 0.5f), u * 0.44f);
                    PicKit.Arrow(pic, P(pic, 0.3f, 0.5f), P(pic, 0.42f, 0.5f), u * 0.05f);
                    Image f = PicKit.Block(pic, P(pic, 0.56f, 0.5f), new Vector2(u * 0.32f, u * 0.62f), UiKit.Hex("#9CC7D8"));
                    PaperKit.AddShadow(f, u / 160f);
                    PicKit.Block(f.transform, new Vector2(u * 0.09f, 0f), new Vector2(u * 0.04f, u * 0.18f), UiKit.Cream);
                    PicKit.Chevron(pic, P(pic, 0.72f, 0.5f), u * 0.16f);
                    PicKit.Tag(pic, P(pic, 0.86f, 0.5f), new Vector2(pillW * 0.7f, pillH * 1.4f), "Mai", font, UiKit.Lilac);
                    break;
                }
                case Drop:
                {
                    string side = r != null ? r.sideId : "ca_chua";
                    PicKit.Icon(pic, IconBook.Ingredient(side), P(pic, 0.3f, 0.76f), u * 0.34f);
                    PicKit.Arrow(pic, P(pic, 0.3f, 0.6f), P(pic, 0.3f, 0.32f), u * 0.05f);
                    PicKit.Ghost(pic, P(pic, 0.3f, 0.2f), u * 0.3f);
                    PicKit.Tag(pic, P(pic, 0.72f, 0.5f), new Vector2(pillW * 1.1f, pillH), "Chuột phải", font, UiKit.Cream);
                    break;
                }
                case Boss:
                {
                    PicKit.Face(pic, FaceArt.Boss(false), P(pic, 0.16f, 0.52f), u * 0.42f, Skin);
                    PicKit.Arrow(pic, P(pic, 0.31f, 0.52f), P(pic, 0.43f, 0.52f), u * 0.05f);
                    if (r != null) PicKit.Column(pic, P(pic, 0.6f, 0.55f), IconBook.Dish(r, g.Session.Recipes, 4), u * 0.44f, UiKit.DishTierName[4],
                        new Vector2(pillW, pillH), font, UiKit.DishTier[4]);
                    PicKit.Mark(pic, P(pic, 0.74f, 0.82f), u * 0.16f, true);
                    PicKit.Tag(pic, P(pic, 0.88f, 0.52f), new Vector2(pillW * 0.62f, pillH * 1.2f), "+" + g.Session.Balance.bossBonus / 1000 + "k", font, UiKit.Mustard);
                    break;
                }
                case Sauce:
                {
                    PicKit.Icon(pic, IconBook.Jar("Nước sốt", UiKit.Coral), P(pic, 0.18f, 0.52f), u * 0.46f);
                    PicKit.Hand(pic, P(pic, 0.2f, 0.46f), u * 0.36f, GhostHand.Gesture.Tap);
                    PicKit.Arrow(pic, P(pic, 0.36f, 0.52f), P(pic, 0.5f, 0.52f), u * 0.06f);
                    if (r != null) PicKit.Icon(pic, IconBook.Dish(r, g.Session.Recipes, 2), P(pic, 0.74f, 0.46f), u * 0.5f);
                    for (int i = 0; i < 3; i++) PicKit.Dot(pic, P(pic, 0.68f + 0.06f * i, 0.82f - 0.04f * (i % 2)), u * 0.07f, UiKit.Coral);
                    break;
                }
                case SauceLuck:
                {
                    // ba cot: len bac (thuong) ✓ · Lac vi (it) ✗ · Huyen thoai (hiem, tu ngay 9)
                    int[] tiers = { 3, 0, 5 };
                    for (int i = 0; i < 3; i++)
                    {
                        if (r != null) PicKit.Column(pic, P(pic, ColX[i], 0.5f), IconBook.Dish(r, g.Session.Recipes, tiers[i]), u * 0.36f, UiKit.DishTierName[tiers[i]],
                            new Vector2(pillW, pillH), font, UiKit.DishTier[tiers[i]]);
                        if (i < 2) PicKit.Mark(pic, P(pic, ColX[i] + 0.1f, 0.84f), u * 0.14f, i == 0);
                    }
                    for (int i = 0; i < SauceFx.Rainbow7.Length; i++)
                        PicKit.Dot(pic, P(pic, ColX[2] - 0.075f + 0.025f * i, 0.88f), u * 0.05f, SauceFx.Rainbow7[i]);
                    break;
                }
                case BossBest:
                {
                    // goi y mon cuoi (yeu cau 23/11/2026): chu no -> Trung chien ca chua bac Huyen thoai, vong 7 mau cau vong
                    Recipe best = g.Session.Lookup.ById(g.Session.Balance.bossBest != null ? g.Session.Balance.bossBest.recipe : null) ?? r;
                    PicKit.Face(pic, FaceArt.Boss(false), P(pic, 0.15f, 0.56f), u * 0.36f, Skin);
                    PicKit.Tag(pic, P(pic, 0.15f, 0.2f), new Vector2(pillW * 0.4f, pillH), "?", font, UiKit.Cream);
                    PicKit.Arrow(pic, P(pic, 0.3f, 0.56f), P(pic, 0.42f, 0.56f), u * 0.05f);
                    Vector2 c = P(pic, 0.68f, 0.6f);
                    for (int i = 0; i < SauceFx.Rainbow7.Length; i++)
                    {
                        float a = i * Mathf.PI * 2f / SauceFx.Rainbow7.Length + Mathf.PI * 0.5f;
                        PicKit.Dot(pic, c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * u * 0.32f, u * 0.08f, SauceFx.Rainbow7[i]);
                    }
                    PicKit.Dot(pic, c, u * 0.5f, UiKit.Cream);
                    if (best != null) PicKit.Icon(pic, IconBook.Dish(best, g.Session.Recipes, Dish.LegendTier), c, u * 0.62f);
                    PicKit.Tag(pic, P(pic, 0.68f, 0.12f), new Vector2(pillW, pillH), UiKit.DishTierName[Dish.LegendTier], font, UiKit.Mustard);
                    break;
                }
                case Dark:
                {
                    PicKit.Block(pic, P(pic, 0.5f, 0.5f), new Vector2(u * 1.5f, u * 0.8f), UiKit.Hex("#4A3F3A"));
                    PicKit.Dot(pic, P(pic, 0.32f, 0.5f), u * 0.18f, UiKit.Mustard);
                    PicKit.Column(pic, P(pic, 0.66f, 0.55f), IconBook.Ingredient(r != null ? r.sideId : "ca_chua", false, 1), u * 0.34f, "?",
                        new Vector2(pillW * 0.4f, pillH), font, UiKit.Cream);
                    break;
                }
                case Tiers:
                {
                    // muc 1 the 6: luoi 3 x 2, moi ca chua co nhan ngay duoi, can giua theo tam; doc trai -> phai, tren -> duoi
                    for (int t = 0; t < 6; t++)
                        PicKit.Column(pic, P(pic, ColX[t % 3], RowY[t / 3]), IconBook.Ingredient("ca_chua", false, t), u * 0.33f, UiKit.IngredientTierName[t],
                            new Vector2(pillW, pillH * 0.9f), font, UiKit.IngredientTier[t], -u * 0.04f);
                    break;
                }
                case DishTier:
                {
                    // the 7: nguyen lieu de vang (Hiem) + -> dia phat sang, "Ngon" ngay duoi dia
                    string main = r != null ? r.mainId : "trung", side = r != null ? r.sideId : "ca_chua";
                    PicKit.Icon(pic, IconBook.Ingredient(main, false, 5), P(pic, 0.12f, 0.55f), u * 0.42f);
                    PicKit.Tag(pic, P(pic, 0.24f, 0.55f), new Vector2(u * 0.1f, u * 0.14f), "+", u * 0.12f);
                    PicKit.Icon(pic, IconBook.Ingredient(side, false, 5), P(pic, 0.36f, 0.55f), u * 0.42f);
                    PicKit.Arrow(pic, P(pic, 0.5f, 0.55f), P(pic, 0.62f, 0.55f), u * 0.06f);
                    RectTransform gl = UiKit.Box(pic, "QuangSang", new Vector2(0.5f, 0.5f), P(pic, 0.81f, 0.62f), new Vector2(u * 0.8f, u * 0.8f));
                    Image glow = gl.gameObject.AddComponent<Image>();
                    glow.sprite = PaperKit.Glow;
                    glow.color = new Color(UiKit.Mustard.r, UiKit.Mustard.g, UiKit.Mustard.b, 0.8f);
                    glow.raycastTarget = false;
                    if (r != null) PicKit.Column(pic, P(pic, 0.81f, 0.5f), IconBook.Dish(r, g.Session.Recipes, 3), u * 0.52f, UiKit.DishTierName[3],
                        new Vector2(pillW * 0.85f, pillH), font, UiKit.DishTier[3]);
                    break;
                }
                case Price:
                {
                    int[] tiers = { 1, 2, 3 };
                    for (int i = 0; i < tiers.Length; i++)
                    {
                        int t = tiers[i];
                        Vector2 at = P(pic, ColX[i], 0.62f);
                        if (r != null) PicKit.Column(pic, at, IconBook.Dish(r, g.Session.Recipes, t), u * 0.38f, UiKit.DishTierName[t],
                            new Vector2(pillW, pillH), font, UiKit.DishTier[t]);
                        // xu + gia ngay duoi vien thuoc, cung tam cot
                        string coins = r == null ? "" : Payment.Price(g.Session.Balance, r.price, t) + "G";
                        Vector2 money = P(pic, ColX[i], 0.12f);
                        Coin(pic, money + new Vector2(-u * 0.14f, 0f), u * 0.12f);
                        PicKit.Tag(pic, money + new Vector2(u * 0.06f, 0f), new Vector2(pillW * 0.55f, pillH), coins, font);
                    }
                    break;
                }
                case Easy:
                {
                    // ba dong: khach de (vui) - vua - kho (gian) -> bac mon toi thieu (vien thuoc rong co dinh)
                    string[] faces = { FaceArt.MoodHappy, FaceArt.MoodWait, FaceArt.MoodAngry };
                    int[] easy = { 85, 50, 20 };
                    for (int i = 0; i < 3; i++)
                    {
                        float y = 0.82f - 0.31f * i;
                        PicKit.Face(pic, FaceArt.Customer(faces[i]), P(pic, 0.12f, y), u * 0.24f, Skin);
                        PicKit.Tag(pic, P(pic, 0.3f, y), new Vector2(pillW * 0.62f, pillH), easy[i] + "%", font, UiKit.Cream);
                        PicKit.Arrow(pic, P(pic, 0.45f, y), P(pic, 0.56f, y), u * 0.04f);
                        int need = g.Session.Balance.RequiredTierFor(easy[i]);
                        PicKit.Tag(pic, P(pic, 0.78f, y), new Vector2(pillW, pillH), UiKit.DishTierName[need], font, UiKit.DishTier[need]);
                    }
                    break;
                }
                case WrongRight:
                {
                    // khuon C: DUNG ben trai, SAI ben phai; "-tien" kem dong xu (rut chu, khong thu nho font)
                    Image left = PicKit.Block(pic, P(pic, 0.26f, 0.5f), new Vector2(u * 0.78f, u * 0.92f), UiKit.Hex("#E9F5EE"));
                    Image right = PicKit.Block(pic, P(pic, 0.74f, 0.5f), new Vector2(u * 0.78f, u * 0.92f), UiKit.Hex("#F6E1DC"));
                    PaperKit.AddShadow(left, u / 160f);
                    PaperKit.AddShadow(right, u / 160f);
                    foreach (bool ok in new[] { true, false })
                    {
                        Transform side = ok ? left.transform : right.transform;
                        PicKit.Face(side, FaceArt.Customer(ok ? FaceArt.MoodHappy : FaceArt.MoodAngry), new Vector2(0f, u * 0.12f), u * 0.32f, Skin);
                        PicKit.Mark(side, new Vector2(-u * 0.26f, u * 0.34f), u * 0.16f, ok);
                        Coin(side, new Vector2(-u * 0.16f, -u * 0.22f), u * 0.12f);
                        PicKit.Tag(side, new Vector2(u * 0.06f, -u * 0.22f), new Vector2(pillW * 0.6f, pillH), ok ? "+tiền" : "−tiền", font);
                    }
                    break;
                }
                case Patience:
                {
                    // the 11: 3 mat khach va 3 thanh cho cung cot, "-20G" dan canh mat khach cuoi
                    string[] faces = { FaceArt.MoodWait, FaceArt.MoodSweat, FaceArt.MoodAngry };
                    float[] fill = { 0.9f, 0.45f, 0.08f };
                    Color[] col = { UiKit.Mint, UiKit.Mustard, UiKit.Coral };
                    for (int i = 0; i < 3; i++)
                    {
                        float x = 0.14f + 0.3f * i;
                        PicKit.Face(pic, FaceArt.Customer(faces[i]), P(pic, x, 0.62f), u * 0.34f, Skin);
                        PicKit.Bar(pic, P(pic, x, 0.24f), new Vector2(u * 0.4f, u * 0.1f), fill[i], col[i]);
                        if (i < 2) PicKit.Chevron(pic, P(pic, x + 0.15f, 0.62f), u * 0.14f);
                    }
                    int pen = Core.Patience.LeavePenalty(g.Session.Balance, System.Math.Max(g.Session.Day, g.Session.Balance.itemShopFromDay));
                    PicKit.Tag(pic, P(pic, 0.88f, 0.78f), new Vector2(pillW * 0.55f, pillH), "−" + pen + "G", font, UiKit.Cream);
                    break;
                }
                case Overheat:
                {
                    // the 12: dau canh bao sat cot khoi cua chao, ✓ canh dia
                    if (r != null) PicKit.Icon(pic, IconBook.Tool(r.tool), P(pic, 0.24f, 0.36f), u * 0.46f);
                    for (int i = 0; i < 3; i++) PicKit.Dot(pic, P(pic, 0.22f + 0.03f * (i % 2), 0.58f + 0.12f * i), u * (0.13f - 0.02f * i), UiKit.Hex("#C9C1B6"));
                    PicKit.Warn(pic, P(pic, 0.36f, 0.8f), u * 0.2f);
                    PicKit.Hand(pic, P(pic, 0.26f, 0.32f), u * 0.36f, GhostHand.Gesture.Tap);
                    PicKit.Arrow(pic, P(pic, 0.48f, 0.4f), P(pic, 0.6f, 0.4f), u * 0.06f);
                    if (r != null) PicKit.Icon(pic, IconBook.Dish(r, g.Session.Recipes), P(pic, 0.78f, 0.4f), u * 0.46f);
                    PicKit.Mark(pic, P(pic, 0.92f, 0.66f), u * 0.16f, true);
                    break;
                }
                case Pull:
                {
                    var odds = g.Session.Balance.OddsForDay(g.Session.Day).percents;
                    for (int t = 0; t < odds.Length && t < 6; t++)
                        PicKit.Column(pic, P(pic, ColX[t % 3], RowY[t / 3]), IconBook.Ingredient("ca_chua", false, t), u * 0.2f, odds[t] + "%",
                            new Vector2(pillW * 0.6f, pillH), font, UiKit.IngredientTier[t], u * 0.06f);
                    break;
                }
            }
        }

        static RelicDef FirstRelic(GameController g)
        {
            GameSession s = g.Session;
            foreach (RelicDef r in s.Shop.Offers) if (r != null) return r;
            return s.Balance.relics.Length > 0 ? s.Balance.relics[0] : null;
        }
    }
}
