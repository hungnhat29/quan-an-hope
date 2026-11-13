using System.Collections.Generic;
using System.Text;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Dung va ve lai toan bo UI choi that bang UGUI + TextMeshPro (constitution Dieu 6, Art Bible muc 9 va 13).
    /// Bo cuc theo khung 1280x720: HUD tren 12%, nut duoi 14%, hai ben 7% cho mui ten xoay.
    /// </summary>
    public class GameView
    {
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        static readonly Vector2 Top = new Vector2(0.5f, 1f);
        static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        static readonly Vector2 TopRight = new Vector2(1f, 1f);
        static readonly Vector2 Bottom = new Vector2(0.5f, 0f);

        static readonly string[] CornerTitles =
        {
            "Góc 1 · Giao món", "Góc 2 · Nhận nguyên liệu", "Góc 3 · Sách nấu ăn", "Góc 4 · Bếp nấu",
        };

        // Dung cu co trong cong thuc (constitution muc 5.1).
        static readonly string[] Tools = { "Chảo", "Nồi", "Vỉ nướng" };

        // Nhom mon thuong trong recipes.json; mon dot pha ("dotpha") chi mo qua relic.
        const string StandardGroup = "chinh";

        readonly GameController g;
        readonly Transform canvas;
        readonly RectTransform[] corners = new RectTransform[4];

        TextMeshProUGUI hudDay, hudMoney, hudTickets, title, toast;

        // Goc 1
        TextMeshProUGUI serveInfo, customerLine, customerEase;
        GameObject bubble;
        Transform serveDishes;
        Button btnOpen, btnBell, btnClose;

        // Goc 2
        TextMeshProUGUI oddsHeader, sideTitle, mainTitle, stockSummary;
        readonly TextMeshProUGUI[] oddsCells = new TextMeshProUGUI[6];
        Transform sideGrid, mainGrid;
        Button btnPull;

        // Goc 3
        TextMeshProUGUI bookTitle, bookBody;

        // Goc 4
        Transform rawGrid, preparedGrid, counterGrid, barRow;
        TextMeshProUGUI boardText, stepsText, counterTitle, stoveStatus;
        ProgressBar boardBar, cookBar;
        HoldButton holdButton;
        readonly Button[] toolButtons = new Button[Tools.Length];
        Button btnCook, btnPlate, btnDiscard;

        // Man phu
        GameObject overlay;
        TextMeshProUGUI overlayTitle, overlayBody;
        Button overlayButton;

        int shownCorner = -1;

        public GameView(GameController controller)
        {
            g = controller;
            canvas = FindOrCreateCanvas();
            EnsureEventSystem();

            for (int i = 0; i < 4; i++) corners[i] = FindOrCreateCorner(i);

            BuildHud();
            BuildServe(corners[0]);
            BuildGacha(corners[1]);
            BuildBook(corners[2]);
            BuildKitchen(corners[3]);
            BuildOverlay();
        }

        public HoldButton HoldButton { get { return holdButton; } }

        // =====================================================================
        // Khung chung
        // =====================================================================

        static Transform FindOrCreateCanvas()
        {
            GameObject existing = GameObject.Find("UI_NhaHang");
            if (existing != null && existing.GetComponent<Canvas>() != null) return existing.transform;

            var go = new GameObject("UI_NhaHang", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        RectTransform FindOrCreateCorner(int index)
        {
            Transform panel = canvas.Find("UI_Goc" + (index + 1));
            Transform parent = panel != null ? panel : canvas;
            RectTransform rt = UiKit.Stretch(parent, "Game_Goc" + (index + 1));
            return rt;
        }

        void BuildHud()
        {
            Image left = UiKit.Panel(canvas, "HUD_Trai", TopLeft, new Vector2(24f, -14f), new Vector2(400f, 72f), UiKit.Cream);
            left.raycastTarget = false;
            hudDay = UiKit.Label(left.transform, "Ngay", TopLeft, new Vector2(14f, -4f), new Vector2(120f, 64f), "", 30f, TextAlignmentOptions.Left);
            hudMoney = UiKit.Label(left.transform, "Tien", TopRight, new Vector2(-12f, -4f), new Vector2(262f, 64f), "", 16f, TextAlignmentOptions.Right);
            hudMoney.lineSpacing = -38f;

            Image right = UiKit.Panel(canvas, "HUD_Phai", TopRight, new Vector2(-24f, -14f), new Vector2(150f, 60f), UiKit.Mustard);
            right.raycastTarget = false;
            hudTickets = UiKit.Text(right.transform, "", UiKit.NumberFontSize);

            Image t = UiKit.Panel(canvas, "HUD_TieuDe", Top, new Vector2(0f, -14f), new Vector2(330f, 44f), UiKit.Ink, false);
            t.raycastTarget = false;
            title = UiKit.Text(t.transform, "", 20f);
            title.color = UiKit.Cream;

            Image toastPanel = UiKit.Panel(canvas, "Toast", Top, new Vector2(0f, -62f), new Vector2(420f, 36f), UiKit.Cream, false);
            toastPanel.raycastTarget = false;
            toast = UiKit.Text(toastPanel.transform, "", 17f);
        }

        // =====================================================================
        // Goc 1 - Giao mon
        // =====================================================================

        void BuildServe(Transform root)
        {
            serveInfo = UiKit.Label(root, "ThongTin", Center, new Vector2(0f, 140f), new Vector2(560f, 60f), "", 20f);
            Image infoBack = UiKit.Panel(root, "ThongTinNen", Center, new Vector2(0f, 140f), new Vector2(580f, 64f), UiKit.Cream);
            infoBack.raycastTarget = false;
            infoBack.transform.SetAsFirstSibling();

            Image b = UiKit.Panel(root, "BongThoai", Center, new Vector2(360f, 150f), new Vector2(290f, 130f), UiKit.Cream);
            b.raycastTarget = false;
            bubble = b.gameObject;
            customerLine = UiKit.Label(b.transform, "Khach", Top, new Vector2(0f, -8f), new Vector2(270f, 34f), "", 20f);
            customerEase = UiKit.Label(b.transform, "DeTinh", Bottom, new Vector2(0f, 10f), new Vector2(270f, 76f), "", 34f);

            Image shelf = UiKit.Panel(root, "QuayCho", Center, new Vector2(-360f, -40f), new Vector2(300f, 272f), UiKit.Cream);
            shelf.raycastTarget = false;
            UiKit.Label(shelf.transform, "TieuDe", Top, new Vector2(0f, -6f), new Vector2(284f, 34f), "Quầy chờ · chọn đĩa để phục vụ", 16f);
            serveDishes = UiKit.Grid(shelf.transform, "Dia", Top, new Vector2(0f, -44f), new Vector2(284f, 216f), 1, new Vector2(280f, 66f)).transform;

            btnOpen = UiKit.Button(root, "Btn_MoQuan", Bottom, new Vector2(0f, 24f), new Vector2(260f, 64f), "Mở quán", UiKit.Coral, () => g.OpenShop(), 26f);
            btnBell = UiKit.Button(root, "Btn_Chuong", Bottom, new Vector2(-125f, 24f), new Vector2(240f, 64f), "Bấm chuông", UiKit.Coral, () => g.RingBell(), 24f);
            btnClose = UiKit.Button(root, "Btn_DongQuan", Bottom, new Vector2(135f, 24f), new Vector2(240f, 64f), "Đóng quán sớm", UiKit.Cream, () => g.CloseEarly(), 20f);
        }

        void RefreshServe()
        {
            GameSession s = g.Session;
            bool prep = s.Phase == GamePhase.Prep;
            bool open = s.Phase == GamePhase.Open;

            btnOpen.gameObject.SetActive(prep);
            btnBell.gameObject.SetActive(open);
            btnClose.gameObject.SetActive(open);
            bubble.SetActive(open && s.CurrentCustomer != null);
            serveInfo.transform.parent.gameObject.SetActive(prep);
            serveInfo.transform.parent.parent.Find("ThongTinNen").gameObject.SetActive(prep);

            if (prep)
                serveInfo.text = "Hôm nay có " + s.Balance.CustomersPerDay(s.Day) + " khách, đến lần lượt.\nNấu trước rồi bấm Mở quán.";

            if (open && s.CurrentCustomer != null)
            {
                customerLine.text = "Khách " + (s.ServedToday + 1) + "/" + s.CustomersToday;
                customerEase.text = "Dễ tính " + s.CurrentCustomer.easiness + "%";
            }

            UiKit.Clear(serveDishes);
            IReadOnlyList<Dish> dishes = s.Counter.Dishes;
            int selected = Mathf.Clamp(g.SelectedDish, 0, Mathf.Max(0, dishes.Count - 1));
            for (int i = 0; i < dishes.Count; i++)
            {
                int index = i;
                Dish d = dishes[i];
                string label = (i == selected ? "<b>> </b>" : "") + d.name + "\n<size=15>" + UiKit.DishTierName[d.tier] + " · </size><size=26>" + Payment.Price(s.Balance, s.Round, d.tier) + "G</size>";
                UiKit.Chip(serveDishes, "Dish_" + i, label, UiKit.DishTier[d.tier], () => g.SelectDish(index), 17f);
            }
            if (dishes.Count == 0)
                UiKit.Chip(serveDishes, "Trong", "Chưa có đĩa nào. Sang Góc 4 để nấu.", UiKit.Cream, null, 15f);

            btnBell.interactable = open && dishes.Count > 0;
        }

        // =====================================================================
        // Goc 2 - Nhan nguyen lieu
        // =====================================================================

        void BuildGacha(Transform root)
        {
            Image board = UiKit.Panel(root, "BangTiLe", Center, new Vector2(0f, 190f), new Vector2(790f, 116f), UiKit.Cream);
            board.raycastTarget = false;
            oddsHeader = UiKit.Label(board.transform, "TieuDe", Top, new Vector2(0f, -6f), new Vector2(760f, 30f), "", 17f);
            // Art Bible muc 13: bang ti le nam ngang, tu kem nhat (trai) den xin nhat (phai), moi o dung mau bac.
            Transform cells = UiKit.Grid(board.transform, "O", Bottom, new Vector2(0f, 12f), new Vector2(762f, 62f), 6, new Vector2(120f, 62f), 8f).transform;
            for (int tier = 0; tier < 6; tier++)
            {
                Image cell = UiKit.Chip(cells, "Bac_" + tier, "", UiKit.IngredientTier[tier], null, UiKit.NumberFontSize);
                oddsCells[tier] = cell.GetComponentInChildren<TextMeshProUGUI>();
            }

            Image sideCard = UiKit.Panel(root, "ThungPhu", Center, new Vector2(-205f, 5f), new Vector2(390f, 210f), UiKit.Hex("#D69B68"));
            sideCard.raycastTarget = false;
            sideTitle = UiKit.Label(sideCard.transform, "TieuDe", Top, new Vector2(0f, -8f), new Vector2(370f, 34f), "", 20f);
            sideGrid = UiKit.Grid(sideCard.transform, "Mon", Top, new Vector2(0f, -48f), new Vector2(372f, 150f), 3, new Vector2(118f, 46f)).transform;

            Image mainCard = UiKit.Panel(root, "ThungChinh", Center, new Vector2(205f, 5f), new Vector2(390f, 210f), UiKit.Hex("#B97F52"));
            mainCard.raycastTarget = false;
            mainTitle = UiKit.Label(mainCard.transform, "TieuDe", Top, new Vector2(0f, -8f), new Vector2(370f, 34f), "", 20f);
            mainGrid = UiKit.Grid(mainCard.transform, "Mon", Top, new Vector2(0f, -48f), new Vector2(372f, 150f), 3, new Vector2(118f, 46f)).transform;

            Image sum = UiKit.Panel(root, "TrongKho", Center, new Vector2(0f, -140f), new Vector2(520f, 40f), UiKit.Cream, false);
            sum.raycastTarget = false;
            stockSummary = UiKit.Text(sum.transform, "", 17f);

            btnPull = UiKit.Button(root, "Btn_NhetVe", Bottom, new Vector2(0f, 24f), new Vector2(280f, 64f), "Nhét vé · 1 vé", UiKit.Mustard, () => g.Pull(), 24f);
        }

        void RefreshGacha()
        {
            GameSession s = g.Session;
            TierTable table = s.Balance.OddsForDay(s.Day);
            oddsHeader.text = "Tỉ lệ bậc nguyên liệu · " + (s.Balance.IsStormDay(s.Day) ? "Ngày " + s.Day + " (sau mưa)" : "Vòng " + s.Round)
                + " · mỗi vé: " + s.Balance.SideCrateSize(s.Day) + " đồ phụ + " + s.Balance.MainCrateSize(s.Day) + " nguyên liệu chính";
            for (int tier = 0; tier < oddsCells.Length; tier++)
            {
                int pct = tier < table.percents.Length ? table.percents[tier] : 0;
                oddsCells[tier].text = pct + "%\n<size=14>" + UiKit.IngredientTierName[tier] + "</size>";
            }

            FillCrate(sideGrid, sideTitle, g.LastPull != null ? g.LastPull.side : null, "Thùng đồ phụ");
            FillCrate(mainGrid, mainTitle, g.LastPull != null ? g.LastPull.main : null, "Thùng nguyên liệu chính");

            int sides = 0, mains = 0;
            foreach (Ingredient i in s.Stock.Items)
            {
                if (i.kind == IngredientKind.Side) sides++;
                else mains++;
            }
            stockSummary.text = "Trong kho hôm nay: " + sides + " đồ phụ, " + mains + " nguyên liệu chính";

            bool canPull = (s.Phase == GamePhase.Prep || s.Phase == GamePhase.Open) && s.Wallet.Count > 0;
            btnPull.interactable = canPull;
        }

        static void FillCrate(Transform grid, TextMeshProUGUI titleText, Crate crate, string emptyTitle)
        {
            UiKit.Clear(grid);
            if (crate == null)
            {
                titleText.text = emptyTitle + " · chưa có";
                return;
            }
            titleText.text = emptyTitle + ": " + crate.items.Count + " " + crate.name;
            for (int i = 0; i < crate.items.Count; i++)
            {
                Ingredient item = crate.items[i];
                UiKit.Chip(grid, "Mon_" + i, UiKit.IngredientTierName[item.tier], UiKit.IngredientTier[item.tier], null, 17f);
            }
        }

        // =====================================================================
        // Goc 3 - Sach nau an (cua hang relic lam o sprint 2)
        // =====================================================================

        void BuildBook(Transform root)
        {
            Image page = UiKit.Panel(root, "SachNauAn", Center, new Vector2(0f, 0f), new Vector2(960f, 470f), UiKit.Cream);
            page.raycastTarget = false;
            bookTitle = UiKit.Label(page.transform, "TieuDe", Top, new Vector2(0f, -10f), new Vector2(920f, 40f), "", 24f);
            bookBody = UiKit.Label(page.transform, "NoiDung", Top, new Vector2(0f, -58f), new Vector2(920f, 400f), "", 18f, TextAlignmentOptions.TopLeft);
            bookBody.fontSizeMin = UiKit.MinFontSize;
        }

        void RefreshBook()
        {
            GameSession s = g.Session;
            int round = s.Round;
            bookTitle.text = "Sách nấu ăn · các món đã mở ở vòng " + round;

            var sb = new StringBuilder();
            foreach (Recipe r in s.Recipes.recipes)
            {
                if (r.group != StandardGroup || r.unlockRound < 1 || r.unlockRound > round) continue;
                sb.Append("<b>").Append(r.name).Append("</b>  ·  ").Append(r.tool).Append("  ·  ");
                sb.Append(string.Join(" → ", r.steps));
                sb.Append("  ·  nấu ").Append(r.cookSeconds.ToString("0.#")).Append(" giây  ·  đế: ").Append(r.baseName).Append('\n');
            }
            sb.Append("\n<size=15>Cho sai thứ tự hoặc sai dụng cụ thì món ra Lạc vị. Cửa hàng relic sẽ mở ở bản sau.</size>");
            bookBody.text = sb.ToString();
        }

        // =====================================================================
        // Goc 4 - Bep nau
        // =====================================================================

        void BuildKitchen(Transform root)
        {
            // Trai: kho va thot
            Image stock = UiKit.Panel(root, "Kho", Center, new Vector2(-362f, 56f), new Vector2(300f, 396f), UiKit.Cream);
            stock.raycastTarget = false;
            UiKit.Label(stock.transform, "T1", Top, new Vector2(0f, -4f), new Vector2(284f, 28f), "Kho · bấm để đưa lên thớt", 15f);
            rawGrid = UiKit.ScrollGrid(stock.transform, "ChuaSoChe", Top, new Vector2(0f, -34f), new Vector2(288f, 236f), 2, new Vector2(140f, 52f), 5f);
            UiKit.Label(stock.transform, "T2", Top, new Vector2(0f, -274f), new Vector2(284f, 28f), "Đã sơ chế · bấm để cho vào bếp", 15f);
            preparedGrid = UiKit.ScrollGrid(stock.transform, "DaSoChe", Top, new Vector2(0f, -304f), new Vector2(288f, 84f), 2, new Vector2(140f, 52f), 5f);

            Image board = UiKit.Panel(root, "Thot", Center, new Vector2(-362f, -200f), new Vector2(300f, 104f), UiKit.Cream);
            board.raycastTarget = false;
            boardText = UiKit.Label(board.transform, "Ten", TopLeft, new Vector2(12f, -6f), new Vector2(168f, 44f), "", 16f, TextAlignmentOptions.Left);
            boardBar = ProgressBar.Create(board.transform, "TienDo", new Vector2(0f, 0f), new Vector2(12f, 12f), new Vector2(168f, 28f), UiKit.Mint);
            Button hold = UiKit.Button(board.transform, "Btn_SoChe", new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(100f, 76f), "Giữ để\nsơ chế", UiKit.Mint, null, 18f);
            holdButton = hold.gameObject.AddComponent<HoldButton>();

            // Giua: bep
            Image stove = UiKit.Panel(root, "Bep", Center, new Vector2(0f, 120f), new Vector2(390f, 268f), UiKit.Cream);
            stove.raycastTarget = false;
            Transform tools = UiKit.Grid(stove.transform, "DungCu", Top, new Vector2(0f, -10f), new Vector2(370f, 54f), 3, new Vector2(116f, 52f), 8f).transform;
            for (int i = 0; i < Tools.Length; i++)
            {
                string tool = Tools[i];
                Image chip = UiKit.Chip(tools, "Tool_" + tool, tool, UiKit.Cream, () => g.SelectTool(tool), 20f);
                toolButtons[i] = chip.GetComponent<Button>();
            }
            stepsText = UiKit.Label(stove.transform, "ThuTu", Top, new Vector2(0f, -70f), new Vector2(366f, 62f), "", 17f);
            // Thanh khoang bac mon (constitution Dieu 4, muc 5.3)
            barRow = UiKit.Grid(stove.transform, "ThanhTiLe", Top, new Vector2(0f, -138f), new Vector2(366f, 54f), 2, new Vector2(178f, 54f), 8f).transform;
            stoveStatus = UiKit.Label(stove.transform, "TrangThai", Bottom, new Vector2(0f, 38f), new Vector2(366f, 26f), "", 15f);
            cookBar = ProgressBar.Create(stove.transform, "DangNau", Bottom, new Vector2(0f, 10f), new Vector2(366f, 26f), UiKit.Coral);

            // Phai: do kho va quay cho
            Image pantry = UiKit.Panel(root, "DoKho", Center, new Vector2(362f, 120f), new Vector2(300f, 268f), UiKit.Cream);
            pantry.raycastTarget = false;
            UiKit.Label(pantry.transform, "T", Top, new Vector2(0f, -4f), new Vector2(284f, 28f), "Đồ kho · không giới hạn", 15f);
            Transform pantryGrid = UiKit.Grid(pantry.transform, "Mon", Top, new Vector2(0f, -36f), new Vector2(288f, 226f), 3, new Vector2(92f, 68f), 6f).transform;
            foreach (string item in g.Session.Recipes.pantry)
            {
                string name = item;
                UiKit.Chip(pantryGrid, "Pantry_" + name, name, UiKit.Hex("#F3DFB8"), () => g.AddPantry(name), 17f);
            }

            Image counter = UiKit.Panel(root, "QuayCho", Center, new Vector2(362f, -136f), new Vector2(300f, 204f), UiKit.Cream);
            counter.raycastTarget = false;
            counterTitle = UiKit.Label(counter.transform, "T", Top, new Vector2(0f, -4f), new Vector2(284f, 28f), "", 15f);
            counterGrid = UiKit.Grid(counter.transform, "Dia", Top, new Vector2(0f, -36f), new Vector2(288f, 160f), 1, new Vector2(284f, 48f), 5f).transform;

            btnCook = UiKit.Button(root, "Btn_Nau", Bottom, new Vector2(-175f, 24f), new Vector2(150f, 64f), "Nấu", UiKit.Coral, () => g.StartCooking(), 24f);
            btnPlate = UiKit.Button(root, "Btn_Muc", Bottom, new Vector2(0f, 24f), new Vector2(180f, 64f), "Múc ra đĩa", UiKit.Mint, () => g.Plate(), 22f);
            btnDiscard = UiKit.Button(root, "Btn_DoBo", Bottom, new Vector2(170f, 24f), new Vector2(140f, 64f), "Đổ bỏ", UiKit.Cream, () => g.DiscardStove(), 20f);
        }

        struct StockGroup
        {
            public Ingredient first;
            public int count;
        }

        void RefreshKitchen()
        {
            GameSession s = g.Session;
            Stove stove = s.Stove;

            // Kho: gop cac mon giong nhau (loai, bac, da so che) thanh mot o co so luong.
            var keys = new List<string>();
            var groups = new Dictionary<string, StockGroup>();
            foreach (Ingredient i in s.Stock.Items)
            {
                if (ReferenceEquals(i, s.Board.Current)) continue;
                string key = i.typeId + "|" + i.tier + "|" + (i.prepared ? "1" : "0");
                StockGroup group;
                if (!groups.TryGetValue(key, out group))
                {
                    keys.Add(key);
                    group = new StockGroup { first = i };
                }
                group.count++;
                groups[key] = group;
            }

            UiKit.Clear(rawGrid);
            UiKit.Clear(preparedGrid);
            int rawIndex = 0, preparedIndex = 0;
            foreach (string key in keys)
            {
                StockGroup group = groups[key];
                Ingredient ing = group.first;
                string label = ing.name + " ×" + group.count + "\n<size=14>" + UiKit.IngredientTierName[ing.tier] + "</size>";
                if (ing.prepared)
                    UiKit.Chip(preparedGrid, "Prepared_" + preparedIndex++, label, UiKit.IngredientTier[ing.tier], () => g.UseIngredient(ing), 15f);
                else
                    UiKit.Chip(rawGrid, "Stock_" + rawIndex++, label, UiKit.IngredientTier[ing.tier], () => g.UseIngredient(ing), 15f);
            }

            // Thot
            Ingredient onBoard = s.Board.Current;
            boardText.text = onBoard != null
                ? "Thớt: " + onBoard.name + "\n<size=14>giữ " + onBoard.prepSeconds.ToString("0.#") + " giây</size>"
                : "Thớt trống";
            holdButton.GetComponent<Button>().interactable = onBoard != null;

            // Bep
            for (int i = 0; i < Tools.Length; i++)
            {
                bool chosen = stove.Tool == Tools[i];
                toolButtons[i].GetComponent<Image>().color = chosen ? UiKit.Mustard : UiKit.Cream;
                toolButtons[i].interactable = stove.State == StoveState.Empty || stove.State == StoveState.Filling;
            }

            if (stove.Tool == null) stepsText.text = "1. Chọn dụng cụ   2. Cho từng thứ vào đúng thứ tự   3. Nấu";
            else if (stove.Added.Count == 0) stepsText.text = stove.Tool + ": chưa cho gì vào";
            else stepsText.text = stove.Tool + ": " + string.Join(" → ", stove.Added);

            UiKit.Clear(barRow);
            TierChance[] bar = stove.Bar();
            if (bar != null)
            {
                for (int i = 0; i < bar.Length; i++)
                {
                    string label = Mathf.RoundToInt((float)(bar[i].chance * 100.0)) + "%\n<size=15>" + UiKit.DishTierName[bar[i].tier] + "</size>";
                    UiKit.Chip(barRow, "Bar_" + i, label, UiKit.DishTier[bar[i].tier], null, UiKit.NumberFontSize);
                }
            }

            switch (stove.State)
            {
                case StoveState.Cooking: stoveStatus.text = "Đang nấu " + stove.Recipe.name; break;
                case StoveState.Ready: stoveStatus.text = stove.Recipe.name + " đã chín"; break;
                default: stoveStatus.text = bar != null ? "Khoảng bậc món nếu nấu đúng công thức" : ""; break;
            }

            btnCook.interactable = stove.State == StoveState.Filling && bar != null;
            btnPlate.interactable = stove.State == StoveState.Ready;
            btnDiscard.interactable = stove.State != StoveState.Empty || stove.Tool != null;

            // Quay cho
            counterTitle.text = "Quầy chờ " + s.Counter.Dishes.Count + "/" + s.Counter.Capacity;
            UiKit.Clear(counterGrid);
            for (int i = 0; i < s.Counter.Dishes.Count; i++)
            {
                Dish d = s.Counter.Dishes[i];
                UiKit.Chip(counterGrid, "Plate_" + i, d.name + " · " + UiKit.DishTierName[d.tier], UiKit.DishTier[d.tier], null, 16f);
            }
        }

        // =====================================================================
        // Man ket thuc ngay, qua vong, thua
        // =====================================================================

        void BuildOverlay()
        {
            RectTransform dim = UiKit.Stretch(canvas, "ManPhu");
            var img = dim.gameObject.AddComponent<Image>();
            img.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.72f);
            overlay = dim.gameObject;

            Image card = UiKit.Panel(dim, "The", Center, Vector2.zero, new Vector2(600f, 360f), UiKit.Cream);
            overlayTitle = UiKit.Label(card.transform, "TieuDe", Top, new Vector2(0f, -22f), new Vector2(560f, 56f), "", 36f);
            overlayBody = UiKit.Label(card.transform, "NoiDung", Center, new Vector2(0f, 6f), new Vector2(560f, 176f), "", UiKit.NumberFontSize);
            overlayBody.lineSpacing = -22f;
            overlayButton = UiKit.Button(card.transform, "Btn_ManPhu", Bottom, new Vector2(0f, 24f), new Vector2(280f, 64f), "", UiKit.Coral, OnOverlayButton, 24f);
            overlay.SetActive(false);
        }

        void OnOverlayButton()
        {
            if (g.Session.Phase == GamePhase.DayEnd) g.NextDay();
            else g.Restart();
        }

        void RefreshOverlay()
        {
            GameSession s = g.Session;
            bool show = s.Phase == GamePhase.DayEnd || s.Phase == GamePhase.Cleared || s.Phase == GamePhase.Lost;
            overlay.SetActive(show);
            if (!show) return;
            overlay.transform.SetAsLastSibling();

            if (s.Phase == GamePhase.DayEnd)
            {
                DebtEntry debt = s.NextDebt();
                overlayTitle.text = "Hết ngày " + s.Day;
                var sb = new StringBuilder();
                sb.Append("Đã phục vụ ").Append(s.ServedToday).Append('/').Append(s.CustomersToday).Append(" khách\n");
                sb.Append("Thu trong ngày: ").Append(Signed(s.IncomeToday)).Append("G\n");
                sb.Append("Tiền hiện có: ").Append(s.Money).Append("G\n");
                if (debt != null)
                {
                    if (debt.day == s.Day + 1) sb.Append("<b>Sáng mai chủ nợ thu ").Append(debt.amount).Append("G</b>");
                    else sb.Append("Nợ sắp tới: ").Append(debt.amount).Append("G vào đầu ngày ").Append(debt.day);
                }
                overlayBody.text = sb.ToString();
                UiKit.SetLabel(overlayButton, "Sang ngày " + (s.Day + 1));
            }
            else if (s.Phase == GamePhase.Cleared)
            {
                overlayTitle.text = "Qua vòng " + s.Balance.RoundOfDay(s.LastPlayableDay) + "!";
                overlayBody.text = "Đầu ngày " + s.Day + ", chủ nợ đã thu " + s.LastDebtPaid + "G.\nBạn còn " + s.Money
                    + "G.\n\nBản chơi thử này dừng ở đây.";
                UiKit.SetLabel(overlayButton, "Chơi lại");
            }
            else
            {
                int due = s.Balance.DebtDueOn(s.Day);
                overlayTitle.text = "Thua rồi";
                overlayBody.text = "Đầu ngày " + s.Day + ", chủ nợ đòi " + due + "G\nnhưng bạn chỉ có " + s.Money + "G.";
                UiKit.SetLabel(overlayButton, "Chơi lại");
            }
        }

        static string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }

        // =====================================================================
        // Ve lai
        // =====================================================================

        /// <summary>Ve lai moi thu theo trang thai hien tai. Goi sau moi thao tac.</summary>
        public void Refresh()
        {
            GameSession s = g.Session;
            DebtEntry debt = s.NextDebt();
            hudDay.text = "Ngày " + s.Day;
            hudMoney.text = "Ví <size=26>" + s.Money + "G</size>\n"
                + (debt != null ? "Nợ <size=26>" + debt.amount + "G</size> đầu ngày " + debt.day : "Hết nợ");
            hudTickets.text = s.Wallet.Count + " vé";

            bool hasToast = !string.IsNullOrEmpty(g.Toast);
            toast.transform.parent.gameObject.SetActive(hasToast);
            if (hasToast) toast.text = g.Toast;

            RefreshServe();
            RefreshGacha();
            RefreshBook();
            RefreshKitchen();
            RefreshOverlay();
            shownCorner = -1;
            Tick();
        }

        /// <summary>Moi khung hinh: tieu de goc, thanh tien do, nut giu.</summary>
        public void Tick()
        {
            int corner = g.Corner;
            if (corner != shownCorner)
            {
                shownCorner = corner;
                title.text = CornerTitles[Mathf.Clamp(corner, 0, CornerTitles.Length - 1)];
                // Neu canh khong co san khung tung goc thi tu bat/tat.
                for (int i = 0; i < corners.Length; i++)
                    if (corners[i].parent == canvas) corners[i].gameObject.SetActive(i == corner);
            }

            GameSession s = g.Session;
            g.Holding = holdButton.IsHeld;
            boardBar.Set((float)s.Board.Progress);

            Stove stove = s.Stove;
            if (stove.State == StoveState.Cooking && stove.CookSeconds > 0f) cookBar.Set(stove.Elapsed / stove.CookSeconds);
            else cookBar.Set(stove.State == StoveState.Ready ? 1f : 0f);
        }
    }
}
