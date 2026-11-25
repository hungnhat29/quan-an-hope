using System.Collections.Generic;
using System.Text;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Bang Tab (sprint 8 F-078; sprint 10 B-10, B-11): the Thuc don (thay quyen sach trong phong, khuon B "Phong cach huong dan
    /// bang hinh": thanh pham + hang icon x so + cac buoc + gia niem yet), Relic (hinh relic that), Cua hang (them Thot), Cach choi
    /// (the tranh). Man ngoai phong nhu menu tam dung (Dieu 9: duoc dung nut), dong ho dung.
    /// Quy cach Art Bible muc 13: the kem giua man, nen toi 72%, nut cao toi thieu 52 px, mau nut theo vai tro.
    /// </summary>
    public class TabMenu
    {
        public enum Page { Relic, Shop, Guide, Menu }

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f), Top = new Vector2(0.5f, 1f), TopLeft = new Vector2(0f, 1f);
        const int Rows = 4;
        const float CardW = 1180f, CardH = 660f, BodyW = 1120f, BodyH = 500f;

        readonly GameController g;
        readonly GameObject root;

        /// <summary>Goc bang Tab (test).</summary>
        public Transform Root { get { return root.transform; } }
        readonly RectTransform relicList, shopList, guideBox, menuBox;
        readonly TextMeshProUGUI header, note;
        readonly Button relicTab, shopTab, guideTab, menuTab;
        string confirmId;
        string menuRecipe;
        int guidePage;

        public Page Current { get; private set; }
        /// <summary>Da mo it nhat mot lan trong van (Tab mo lai the vua xem).</summary>
        public bool Opened { get; private set; }
        public Canvas Canvas { get { return root.GetComponent<Canvas>(); } }
        public bool Shown { get { return root.activeSelf; } }
        public string HeaderText { get { return header.text; } }
        public string NoteText { get { return note.text; } }
        public Button GuideTab { get { return guideTab; } }
        public Button MenuTab { get { return menuTab; } }

        public readonly Dictionary<string, Button> DiscardButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, Button> BuyButtons = new Dictionary<string, Button>();
        public readonly Dictionary<string, TextMeshProUGUI> ShopLines = new Dictionary<string, TextMeshProUGUI>();

        /// <summary>The thuc don: nut tung mon theo id cong thuc (test), mon dang mo, gia dang hien.</summary>
        public readonly Dictionary<string, Button> MenuButtons = new Dictionary<string, Button>();
        public string MenuRecipe { get { return menuRecipe; } }
        public TextMeshProUGUI MenuTitle { get; private set; }
        public TextMeshProUGUI MenuPrice { get; private set; }
        /// <summary>So buoc (khung) cua mon dang mo, so icon hang "CAN".</summary>
        public int MenuStepFrames { get; private set; }
        public int MenuNeedIcons { get; private set; }
        public bool MenuBreakthroughLocked { get; private set; }

        /// <summary>Hinh relic tung dong (test: khong con o bau duc).</summary>
        public readonly Dictionary<string, RawImage> RelicIcons = new Dictionary<string, RawImage>();
        /// <summary>The tranh dang hien o trang Cach choi.</summary>
        public readonly List<string> GuideCards = new List<string>();

        public TabMenu(GameController controller)
        {
            g = controller;
            var go = new GameObject("BangTab", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 28;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            root = go;

            RectTransform dim = UiKit.Stretch(go.transform, "NenToi");
            var di = dim.gameObject.AddComponent<Image>();
            di.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.72f);
            Image card = UiKit.Panel(dim, "The", Center, Vector2.zero, new Vector2(CardW, CardH), UiKit.Cream);
            menuTab = TabButton(card.transform, "Tab_ThucDon", 0, "Thực đơn", UiKit.Mint, Page.Menu);
            relicTab = TabButton(card.transform, "Tab_Relic", 1, "Relic", UiKit.Lilac, Page.Relic);
            shopTab = TabButton(card.transform, "Tab_CuaHang", 2, "Cửa hàng", UiKit.Mustard, Page.Shop);
            guideTab = TabButton(card.transform, "Tab_CachChoi", 3, "Cách chơi", UiKit.Coral, Page.Guide);
            UiKit.Button(card.transform, "Btn_Dong", new Vector2(1f, 1f), new Vector2(-28f, -20f), new Vector2(170f, 52f), "Đóng (Tab)", UiKit.Cream, () => g.CloseTab(), 20f)
                .transform.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            header = UiKit.Label(card.transform, "TieuDe", Top, new Vector2(0f, -92f), new Vector2(BodyW, 36f), "", 22f);
            relicList = UiKit.Box(card.transform, "DanhSachRelic", Top, new Vector2(0f, -128f), new Vector2(BodyW, BodyH));
            shopList = UiKit.Box(card.transform, "DanhSachCuaHang", Top, new Vector2(0f, -128f), new Vector2(BodyW, BodyH));
            guideBox = UiKit.Box(card.transform, "CachChoi", Top, new Vector2(0f, -88f), new Vector2(BodyW, BodyH + 36f));
            menuBox = UiKit.Box(card.transform, "ThucDon", Top, new Vector2(0f, -88f), new Vector2(BodyW, BodyH + 36f));
            foreach (RectTransform r in new[] { relicList, shopList, guideBox, menuBox }) r.pivot = Top;
            note = UiKit.Label(card.transform, "GhiChu", new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(BodyW, 34f), "", 16f);
            root.SetActive(false);
        }

        Button TabButton(Transform card, string name, int index, string label, Color color, Page page)
        {
            Button b = UiKit.Button(card, name, TopLeft, new Vector2(28f + 200f * index, -20f), new Vector2(188f, 52f), label, color, () => Show(page), 22f);
            ((RectTransform)b.transform).pivot = TopLeft;
            return b;
        }

        public void Show(Page page)
        {
            Current = page;
            Opened = true;
            confirmId = null;
            if (page == Page.Shop) g.NoteShopView();
            if (page == Page.Menu) g.NoteMenuView();
            root.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            confirmId = null;
            root.SetActive(false);
        }

        public void Refresh()
        {
            if (!root.activeSelf || g.Session == null) return;
            relicList.gameObject.SetActive(Current == Page.Relic);
            shopList.gameObject.SetActive(Current == Page.Shop);
            guideBox.gameObject.SetActive(Current == Page.Guide);
            menuBox.gameObject.SetActive(Current == Page.Menu);
            header.gameObject.SetActive(Current == Page.Relic || Current == Page.Shop);
            relicTab.interactable = Current != Page.Relic;
            shopTab.interactable = Current != Page.Shop;
            guideTab.interactable = Current != Page.Guide;
            menuTab.interactable = Current != Page.Menu;
            switch (Current)
            {
                case Page.Relic: BuildRelics(); break;
                case Page.Shop: BuildShop(); break;
                case Page.Guide: BuildGuide(); break;
                default: BuildMenu(); break;
            }
        }

        // ---------------- the Thuc don (khuon B) ----------------

        /// <summary>Mon trong thuc don: mon da mo khoa o vong nay; mon dot pha khi da co ruong dot pha (relic) hoac nguyen lieu.</summary>
        public List<Recipe> MenuRecipes(out bool breakthroughOpen)
        {
            GameSession s = g.Session;
            var list = new List<Recipe>(s.BookRecipes());
            breakthroughOpen = s.BreakthroughChance > 0;
            foreach (Ingredient i in s.Stock.Items)
                foreach (MainIngredient m in s.Recipes.mains)
                    if (m.id == i.typeId && m.group != "chinh") breakthroughOpen = true;
            if (breakthroughOpen) list.AddRange(s.BreakthroughRecipes());
            return list;
        }

        /// <summary>F-092 AC-2: dung san danh sach mon (luc o menu) de lan mo Tab dau khong khung.</summary>
        public void Prebuild()
        {
            if (g.Session == null) return;
            BuildMenu();
        }

        /// <summary>Mo trang mot mon (test / bam trong danh sach).</summary>
        public void OpenRecipe(string id)
        {
            menuRecipe = id;
            if (Current == Page.Menu && root.activeSelf) BuildMenu();
        }

        void BuildMenu()
        {
            GameSession s = g.Session;
            bool bt;
            List<Recipe> recipes = MenuRecipes(out bt);
            MenuBreakthroughLocked = !bt;
            if (recipes.Count == 0) return;
            Recipe cur = null;
            foreach (Recipe r in recipes) if (r.id == menuRecipe) cur = r;
            if (cur == null) cur = recipes[0];
            menuRecipe = cur.id;

            // F-092 AC-2: danh sach mon chi dung lai khi doi (vong moi, co mon dot pha); bam mon chi doi mau dong + trang phai
            string key = s.Round + ":" + bt + ":" + recipes.Count;
            if (key == menuListKey && menuList != null)
            {
                foreach (KeyValuePair<string, Button> kv in MenuButtons)
                {
                    Recipe r = s.Lookup.ById(kv.Key);
                    kv.Value.GetComponent<Image>().color = kv.Key == cur.id ? UiKit.Mustard : r != null && r.group != "chinh" ? UiKit.Hex("#E7DDF7") : UiKit.Hex("#FBF0DA");
                }
                if (menuPage != null) Object.Destroy(menuPage.gameObject);
                BuildMenuPage(s, cur, recipes.Count);
                return;
            }
            menuListKey = key;
            UiKit.Clear(menuBox);
            MenuButtons.Clear();

            // Trai: danh sach mon (icon + ten + gia), cuon duoc
            Transform list = UiKit.ScrollGrid(menuBox, "DanhSachMon", TopLeft, new Vector2(0f, 0f), new Vector2(380f, BodyH + 30f), 1, new Vector2(366f, 54f), 6f);
            menuList = list;
            ((RectTransform)list.parent).pivot = TopLeft;
            foreach (Recipe r in recipes)
            {
                Recipe rr = r;
                bool special = r.group != "chinh";
                Image row = UiKit.Chip(list, "Mon_" + r.id, "", r.id == cur.id ? UiKit.Mustard : special ? UiKit.Hex("#E7DDF7") : UiKit.Hex("#FBF0DA"), () => OpenRecipe(rr.id), 16f);
                Button b = row.GetComponent<Button>();
                MenuButtons[r.id] = b;
                TextMeshProUGUI t = row.GetComponentInChildren<TextMeshProUGUI>();
                t.alignment = TextAlignmentOptions.Left;
                t.rectTransform.offsetMin = new Vector2(58f, 2f);
                t.rectTransform.offsetMax = new Vector2(-70f, -2f);
                t.text = r.name;
                PicKit.Icon(row.transform, IconBook.Dish(r, s.Recipes), new Vector2(-366f * 0.5f + 28f, 0f), 50f);
                TextMeshProUGUI price = UiKit.Label(row.transform, "Gia", new Vector2(1f, 0.5f), new Vector2(-38f, 0f), new Vector2(68f, 40f), r.price + "G", 18f);
                price.alignment = TextAlignmentOptions.Right;
            }
            if (!bt)
            {
                Image locked = UiKit.Chip(list, "Mon_DotPha_Khoa", "Sơn hào hải vị · cần rương đột phá", UiKit.Disabled, null, 15f);
                locked.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            }

            BuildMenuPage(s, cur, recipes.Count);
        }

        string menuListKey;
        Transform menuList;
        RectTransform menuPage;

        void BuildMenuPage(GameSession s, Recipe cur, int count)
        {
            // Phai: trang mon (thanh pham o dau, gia niem yet, hang CAN, cac buoc)
            RectTransform page = UiKit.Box(menuBox, "TrangMon", new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(720f, BodyH + 30f));
            page.pivot = new Vector2(1f, 1f);
            menuPage = page;
            MenuTitle = UiKit.Label(page, "TenMon", TopLeft, new Vector2(14f, -4f), new Vector2(520f, 52f), cur.name, 30f, TextAlignmentOptions.Left);
            MenuTitle.rectTransform.parent.GetComponent<RectTransform>().pivot = TopLeft;
            Image chip = UiKit.Panel(page, "GiaNiemYet", new Vector2(1f, 1f), new Vector2(-10f, -6f), new Vector2(160f, 50f), UiKit.Mustard, false);
            ((RectTransform)chip.transform).pivot = new Vector2(1f, 1f);
            MenuPrice = UiKit.Text(chip.transform, cur.price + "G", 28f);

            // Thanh pham (diorama): dia mon to tren nen giay
            Image hero = UiKit.Panel(page, "ThanhPham", TopLeft, new Vector2(10f, -62f), new Vector2(250f, 230f), UiKit.Hex("#F6E6C6"), false);
            ((RectTransform)hero.transform).pivot = TopLeft;
            PicKit.Icon(hero.transform, IconBook.Dish(cur, s.Recipes), new Vector2(0f, 10f), 210f);
            UiKit.Label(hero.transform, "De", new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(230f, 30f), "Đế: " + cur.baseName, 17f);

            // Hang CAN: icon x so (nguyen lieu chinh, do phu, dung cu)
            RectTransform need = UiKit.Box(page, "Can", TopLeft, new Vector2(276f, -62f), new Vector2(430f, 110f));
            need.pivot = TopLeft;
            UiKit.Label(need, "Nhan", TopLeft, new Vector2(0f, 0f), new Vector2(90f, 30f), "CẦN", 20f, TextAlignmentOptions.Left).rectTransform.parent.GetComponent<RectTransform>().pivot = TopLeft;
            MenuNeedIcons = 0;
            NeedIcon(need, 0, IconBook.Ingredient(cur.mainId), "×1");
            NeedIcon(need, 1, IconBook.Ingredient(cur.sideId), "×1");
            NeedIcon(need, 2, IconBook.Tool(cur.tool), cur.tool);
            UiKit.Label(page, "ThoiGian", TopLeft, new Vector2(276f, -180f), new Vector2(430f, 34f),
                "Nấu " + Mathf.RoundToInt(cur.cookSeconds) + "s · bán " + cur.price + "G (bậc Bình thường)", 17f, TextAlignmentOptions.Left)
                .rectTransform.parent.GetComponent<RectTransform>().pivot = TopLeft;
            UiKit.Label(page, "NgonHon", TopLeft, new Vector2(276f, -212f), new Vector2(430f, 30f),
                "Món ngon hơn → đắt hơn · tệ hơn → rẻ hơn", 15f, TextAlignmentOptions.Left).rectTransform.parent.GetComponent<RectTransform>().pivot = TopLeft;

            // Cac buoc: (1) chon dung cu, (2..) cho vao theo thu tu, cuoi: vat num -> mon
            RectTransform steps = UiKit.Box(page, "CacBuoc", TopLeft, new Vector2(10f, -306f), new Vector2(700f, 220f));
            steps.pivot = TopLeft;
            int n = cur.steps.Length + 2;
            float w = Mathf.Min(118f, (700f - 16f * (n - 1)) / n);
            MenuStepFrames = 0;
            StepFrame(steps, 0, n, w, IconBook.Tool(cur.tool), "Chọn");
            for (int i = 0; i < cur.steps.Length; i++)
                StepFrame(steps, i + 1, n, w, StepIcon(cur.steps[i]), "Cho");
            StepFrame(steps, n - 1, n, w, IconBook.Dish(cur, s.Recipes), "Vặn núm");
            UiKit.Label(page, "ThuTu", TopLeft, new Vector2(10f, -478f), new Vector2(700f, 30f),
                "Sai thứ tự hoặc sai dụng cụ → Lạc vị", 15f, TextAlignmentOptions.Left).rectTransform.parent.GetComponent<RectTransform>().pivot = TopLeft;

            if (Current == Page.Menu) note.text = count + " món · giá niêm yết ở bậc Bình thường · khách trả thêm tip";
        }

        void NeedIcon(RectTransform need, int i, Texture tex, string label)
        {
            Image cell = UiKit.Panel(need, "O_" + i, TopLeft, new Vector2(92f + 112f * i, 0f), new Vector2(104f, 104f), UiKit.Hex("#FBF0DA"), false);
            ((RectTransform)cell.transform).pivot = TopLeft;
            PicKit.Icon(cell.transform, tex, new Vector2(0f, 10f), 76f);
            UiKit.Label(cell.transform, "So", new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(100f, 26f), label, 16f);
            MenuNeedIcons++;
        }

        void StepFrame(RectTransform steps, int i, int n, float w, Texture tex, string verb)
        {
            float x = i * (w + 16f);
            // goc trai tren cua khung (UiKit.Box: pivot = anchor)
            RectTransform pic = PicKit.Frame(steps, "Buoc_" + (i + 1), TopLeft, new Vector2(x, -10f), new Vector2(w, 150f), i + 1, verb);
            PicKit.Icon(pic, tex, Vector2.zero, Mathf.Min(w - 12f, 84f));
            // chevron giua hai khung (toa do so voi tam khung CacBuoc 700 x 220; tam khung o y = 110 - 85)
            if (i < n - 1) PicKit.Chevron(steps, new Vector2(x + w + 8f - 350f, 110f - 85f), 28f);
            MenuStepFrames++;
        }

        Texture2D StepIcon(string step)
        {
            GameSession s = g.Session;
            foreach (SideIngredient x in s.Recipes.sides) if (x.name == step) return IconBook.Ingredient(x.id, true);
            foreach (MainIngredient x in s.Recipes.mains) if (x.name == step) return IconBook.Ingredient(x.id, true);
            Color c;
            if (g.Room == null || !g.Room.JarColor(step, out c)) c = UiKit.Cream;
            return IconBook.Jar(step, c);
        }

        // ---------------- the Relic ----------------

        void BuildRelics()
        {
            GameSession s = g.Session;
            RelicInventory inv = s.Relics;
            UiKit.Clear(relicList);
            DiscardButtons.Clear();
            RelicIcons.Clear();
            bool pending = g.PendingRelicSlot >= 0;
            RelicDef want = !pending ? null : g.PendingIsBlack
                ? (g.PendingRelicSlot < s.BlackRelicOffers.Count ? s.BlackRelicOffers[g.PendingRelicSlot] : null)
                : (g.PendingRelicSlot < s.Shop.Offers.Count ? s.Shop.Offers[g.PendingRelicSlot] : null);
            header.text = "Relic đang có · " + inv.Owned.Count + "/" + inv.Capacity
                + (pending && want != null ? " · <color=#E8604F>đã đủ: chọn 1 relic để bỏ, lấy " + want.name + "</color>" : "");
            for (int i = 0; i < inv.Owned.Count; i++)
            {
                RelicDef r = inv.Owned[i];
                int col = i / Rows, row = i % Rows;
                Image box = UiKit.Panel(relicList, "Relic_" + r.id, TopLeft, new Vector2(col * 565f, -row * 118f), new Vector2(550f, 108f), UiKit.Hex("#FBF0DA"), false);
                ((RectTransform)box.transform).pivot = TopLeft;
                // B-10: hinh relic that (chup tu model) tren dia mau do hiem, khong con thanh bau duc
                Image disc = PicKit.Dot(box.transform, new Vector2(-550f * 0.5f + 56f, 0f), 92f, RelicArt.RarityColor(r.rarity));
                RelicIcons[r.id] = PicKit.Icon(box.transform, IconBook.Relic(r), new Vector2(-550f * 0.5f + 56f, 2f), 88f, "HinhRelic");
                disc.name = "DoHiem";
                TextMeshProUGUI t = UiKit.Label(box.transform, "Chu", new Vector2(0f, 0.5f), new Vector2(112f, 0f), new Vector2(310f, 100f),
                    "<b>" + r.name + "</b> <size=14>· " + RelicKeys.RarityName(r.rarity) + "</size>\n<size=15>" + RelicText.Effect(r) + "</size>", 18f, TextAlignmentOptions.Left);
                ((RectTransform)t.transform.parent).pivot = new Vector2(0f, 0.5f);
                string id = r.id;
                bool confirm = confirmId == id;
                Button b = UiKit.Button(box.transform, "Btn_Bo_" + id, new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(104f, 52f),
                    pending ? "Bỏ" : confirm ? "Chắc?" : "Bỏ", pending || confirm ? UiKit.Coral : UiKit.Cream, () => OnDiscard(id), 18f);
                ((RectTransform)b.transform).pivot = new Vector2(1f, 0.5f);
                DiscardButtons[id] = b;
            }
            if (inv.Owned.Count == 0)
                UiKit.Label(relicList, "Trong", Center, Vector2.zero, new Vector2(900f, 60f), "Chưa có relic. Mua ở 3 bệ relic (mở từ ngày " + s.Balance.start.relicShopFromDay + ").", 20f);
            note.text = pending ? "Bấm Bỏ ở relic muốn bỏ: relic mới vào thay chỗ." : "Bỏ relic không hoàn vé. Bấm Bỏ hai lần để chắc chắn.";
        }

        void OnDiscard(string id)
        {
            if (g.PendingRelicSlot < 0 && confirmId != id)
            {
                confirmId = id;
                BuildRelics();
                return;
            }
            confirmId = null;
            bool wasPending = g.PendingRelicSlot >= 0;
            g.DiscardRelic(id);
            if (wasPending) g.CloseTab();
            else Refresh();
        }

        // ---------------- the Cua hang ----------------

        void BuildShop()
        {
            GameSession s = g.Session;
            ItemShop it = s.Items;
            Balance b = s.Balance;
            UiKit.Clear(shopList);
            BuyButtons.Clear();
            ShopLines.Clear();
            header.text = "Cửa hàng · ví " + s.Money + "G" + (it.IsOpen ? "" : " · mở từ ngày " + b.itemShopFromDay);

            double[] so = ItemRolls.SauceOdds(b, s.Day, s.Relics.Effects);
            var sauce = new StringBuilder();
            sauce.Append("<b>Nước sốt thần thánh</b> · ").Append(it.SaucePrice).Append("G · có ").Append(it.Sauces).Append('/').Append(b.sauce.max)
                .Append("\n<size=15>Lọ ở bên phải bếp: cầm lọ, bấm đĩa vừa nấu. Hỏng ").Append(Pct(so[0])).Append(" · giữ ").Append(Pct(so[1]))
                .Append(" · +1 bậc ").Append(Pct(so[2])).Append(" · +2 bậc ").Append(Pct(so[3]));
            if (so[4] > 0) sauce.Append(" · Huyền thoại ").Append(Pct(so[4]));
            sauce.Append("</size>");
            Row(0, GameController.ItemSauce, IconBook.Jar("Nước sốt", UiKit.Coral), sauce.ToString(), it.IsOpen && it.Sauces < b.sauce.max);

            var black = new StringBuilder();
            black.Append("<b>Vé đen</b> · ").Append(it.BlackPrice).Append("G · có ").Append(it.BlackTickets).Append('/').Append(b.blackTicket.max)
                .Append("\n<size=15>Nhét vào máy vé: ");
            int[] bo = b.BlackOdds().percents;
            bool first = true;
            for (int tier = 0; tier < bo.Length; tier++)
                if (bo[tier] > 0) { black.Append(first ? "" : " · ").Append(UiKit.IngredientTierName[tier]).Append(' ').Append(bo[tier]).Append('%'); first = false; }
            black.Append(" · hoặc khe cạnh bệ relic: chọn relic Hiếm</size>");
            Row(1, GameController.ItemBlack, null, black.ToString(), it.IsOpen && it.BlackTickets < b.blackTicket.max);

            // Dieu 4: may gacha do an giau ti le
            string machine = "<b>Máy gacha đồ ăn</b> · " + (it.HasMachine ? "đã mua · còn " + it.MachineUsesLeft + "/" + b.machine.usesPerDay + " lượt" : b.machine.price + "G · mua 1 lần")
                + "\n<size=15>Đổi bậc một món đã nấu (lên hoặc xuống, tỉ lệ ẩn), mỗi món 1 lần.</size>";
            Row(2, GameController.ItemMachine, null, machine, it.IsOpen && !it.HasMachine);

            // B-15: thot mua them (gia tang moi lan, toi da board.max)
            bool fullBoards = it.Boards >= b.board.max;
            string board = "<b>Thớt</b> · " + (fullBoards ? "đủ " + b.board.max + " thớt" : it.BoardPrice + "G") + " · có " + it.Boards + "/" + b.board.max
                + "\n<size=15>Sơ chế nhiều món cùng lúc. Bàn sơ chế dài thêm.</size>";
            Row(3, GameController.ItemBoard, IconBook.Board(), board, it.IsOpen && !fullBoards);

            // sau sprint 10 (constitution 2.1 muc 8): bep mua them 500G, toi da stove.max
            bool fullStoves = it.Stoves >= b.stove.max;
            string stove = "<b>Bếp</b> · " + (fullStoves ? "đủ " + b.stove.max + " bếp" : it.StovePrice + "G") + " · có " + it.Stoves + "/" + b.stove.max
                + "\n<size=15>Nấu hai món cùng lúc. Bếp mới ở bên phải quầy đĩa.</size>";
            Row(4, GameController.ItemStove, IconBook.Tool("Nồi"), stove, it.IsOpen && !fullStoves);

            // 2.2 (yeu cau 24/11/2026): tay van nang - cam 2 do, mua 1 lan
            string glove = "<b>Tay vạn năng</b> · " + (it.HasGlove ? "đã mua" : b.glovePrice + "G · mua 1 lần")
                + "\n<size=15>Cầm 2 đồ cùng lúc. Phím 1 / 2 chọn ô; không chọn thì dùng / thả theo thứ tự nhặt.</size>";
            Row(5, GameController.ItemGlove, null, glove, it.IsOpen && !it.HasGlove);
            note.text = "Đồ mua xong hiện trong phòng. Đồng hồ đứng trong lúc mở bảng này.";
        }

        /// <summary>Khoang cach dong cua hang (5 dong tu khi co Bep).</summary>
        // bug 24/11/2026 (anh 1): 6 dong x 92 = 552 > BodyH 500 -> dong cuoi de len ghi chu; 6 x 82 = 492
        const float RowStep = 82f;

        void Row(int index, string item, Texture icon, string text, bool canBuy)
        {
            Image box = UiKit.Panel(shopList, "Hang_" + item, Top, new Vector2(0f, -index * RowStep), new Vector2(BodyW, RowStep - 8f), UiKit.Hex("#FBF0DA"), false);
            ((RectTransform)box.transform).pivot = Top;
            if (icon != null) PicKit.Icon(box.transform, icon, new Vector2(-BodyW * 0.5f + 56f, 0f), 68f);
            else PicKit.Block(box.transform, new Vector2(-BodyW * 0.5f + 56f, 0f), new Vector2(80f, 54f), item == GameController.ItemBlack ? UiKit.Ink : item == GameController.ItemGlove ? UiKit.Hex("#FFF8EA") : UiKit.Hex("#9CC7D8"), item == GameController.ItemGlove ? "1 · 2" : null, 22f);
            TextMeshProUGUI t = UiKit.Label(box.transform, "Chu", new Vector2(0f, 0.5f), new Vector2(112f, 0f), new Vector2(820f, 72f), text, 18f, TextAlignmentOptions.Left);
            ((RectTransform)t.transform.parent).pivot = new Vector2(0f, 0.5f);
            ShopLines[item] = t;
            string it = item;
            Button b = UiKit.Button(box.transform, "Btn_Mua_" + item, new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(150f, 56f), "Mua",
                canBuy ? UiKit.Mustard : UiKit.Disabled, () => { g.BuyItem(it); Refresh(); }, 22f);
            ((RectTransform)b.transform).pivot = new Vector2(1f, 0.5f);
            b.interactable = canBuy;
            BuyButtons[item] = b;
        }

        static string Pct(double v) { return System.Math.Round(v).ToString("0") + "%"; }

        // ---------------- the Cach choi (tranh) ----------------

        const int GuidePerPage = 6;

        /// <summary>Nut het trang: mo, khong bam duoc.</summary>
        static void SetFaded(Button b, bool faded)
        {
            b.interactable = !faded;
            var cg = b.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = faded ? 0.45f : 1f;
        }

        /// <summary>Trang Cach choi dang xem (test).</summary>
        public int GuidePage { get { return guidePage; } }

        public int GuidePages { get { return (GuidePics.Sheet.Length + GuidePerPage - 1) / GuidePerPage; } }

        public void TurnGuide(int delta)
        {
            // paper-ui-pickit.md: khong vong tron trang; nut het trang thi mo
            guidePage = Mathf.Clamp(guidePage + delta, 0, GuidePages - 1);
            if (Current == Page.Guide && root.activeSelf) BuildGuide();
        }

        void BuildGuide()
        {
            UiKit.Clear(guideBox);
            GuideCards.Clear();
            Vector2 size = new Vector2(352f, 214f);
            for (int k = 0; k < GuidePerPage; k++)
            {
                int i = guidePage * GuidePerPage + k;
                if (i >= GuidePics.Sheet.Length) break;
                int col = k % 3, row = k / 3;
                string id = GuidePics.Sheet[i];
                // anchor Top: vi tri la diem giua canh tren cua the (UiKit.Box: pivot = anchor)
                RectTransform card = GuidePics.Card(g, guideBox, id, Top, new Vector2((col - 1) * 372f, -4f - row * (size.y + 14f)), size, i + 1);
                // lat trang: the moi "dan" vao lan luot, moi the tre 2 hinh (12 hinh / giay)
                CardPop pop = card.GetComponent<CardPop>();
                if (pop != null) pop.Delay = k * 2f / PaperKit.StepFps;
                GuideCards.Add(id);
            }
            Button prev = UiKit.Button(guideBox, "Btn_TruocCachChoi", new Vector2(0f, 0f), new Vector2(70f, 10f), new Vector2(120f, 52f), "‹ Trước", UiKit.Cream, () => TurnGuide(-1), 20f);
            Button next = UiKit.Button(guideBox, "Btn_SauCachChoi", new Vector2(1f, 0f), new Vector2(-70f, 10f), new Vector2(120f, 52f), "Sau ›", UiKit.Cream, () => TurnGuide(1), 20f);
            SetFaded(prev, guidePage == 0);
            SetFaded(next, guidePage >= GuidePages - 1);
            // cham trang thay dong chu "Trang 1/2 · ..."
            for (int p = 0; p < GuidePages; p++)
            {
                Image d = PicKit.Dot(guideBox, Vector2.zero, 18f, p == guidePage ? UiKit.Coral : UiKit.Cream);
                RectTransform dr = (RectTransform)d.transform;
                dr.anchorMin = dr.anchorMax = new Vector2(0.5f, 0f);
                dr.anchoredPosition = new Vector2((p - (GuidePages - 1) * 0.5f) * 28f, 36f);
                d.name = "ChamTrang_" + p;
            }
            note.text = "";
        }
    }
}
