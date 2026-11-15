using System.Collections.Generic;
using System.Text;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Do vat choi duoc trong can phong 3D (constitution Dieu 5, Dieu 9; Art Bible muc 8).
    /// Moi thong tin nam tren chinh do vat: nguyen lieu trong thung (bac = mau de), bang ti le tren tuong,
    /// bang nho tren bep, trang sach, bong bong tren dau khach. Toa do viet theo khung cua tung goc
    /// (x doc tuong, y len tren, z huong vao tuong), giong BuildNhaHang.
    /// </summary>
    public class RoomView
    {
        // Dung cu co trong cong thuc (constitution muc 5.1).
        public static readonly string[] Tools = { "Chảo", "Nồi", "Vỉ nướng" };
        const string StandardGroup = "chinh";

        // Mau go cua thung (Art Bible muc 7) va mot so mau do vat.
        static readonly Color SideCrateColor = UiKit.Hex("#D69B68");
        static readonly Color MainCrateColor = UiKit.Hex("#B97F52");
        static readonly Color WoodDark = UiKit.Hex("#B97F52");
        static readonly Color Steel = UiKit.Hex("#5A5250");
        static readonly Color SteelLight = UiKit.Hex("#C9C1B6");
        static readonly Color PlateWhite = UiKit.Hex("#FFFFFF");

        /// <summary>Kich thuoc dai ti le bac mon tren bang bep (mm tren do vat).</summary>
        public static readonly Vector2 BarSize = new Vector2(1420f, 104f);

        readonly GameController g;
        readonly Transform root, goc1, goc2, goc3, goc4;
        readonly Dictionary<string, Interactable> stations = new Dictionary<string, Interactable>();

        // Goc 2
        TextMeshProUGUI oddsHeader;
        readonly TextMeshProUGUI[] oddsCells = new TextMeshProUGUI[6];
        Transform crateRoot;
        readonly Dictionary<string, CrateView> crates = new Dictionary<string, CrateView>();
        readonly Dictionary<Ingredient, ItemView> items = new Dictionary<Ingredient, ItemView>();

        // Goc 4
        Transform boardSpot;
        GameObject boardItem;
        Ingredient boardItemFor;
        ProgressBar boardBar;
        GameObject boardBarRoot;
        readonly Dictionary<string, GameObject> rackTools = new Dictionary<string, GameObject>();
        readonly Dictionary<string, GameObject> burnerTools = new Dictionary<string, GameObject>();
        GameObject potGroup, steam;
        Transform knob;
        TextMeshProUGUI stoveSteps, stoveStatus;
        GameObject stoveBarRoot;
        readonly TextMeshProUGUI[] barCells = new TextMeshProUGUI[6];
        ProgressBar cookBar;
        readonly Dictionary<string, GameObject> jars = new Dictionary<string, GameObject>();
        readonly Dictionary<string, Color> jarColors = new Dictionary<string, Color>();
        Transform panRoot;
        readonly List<PanItem> panItems = new List<PanItem>();
        readonly Transform[] shelfSlots = new Transform[3];
        readonly List<GameObject> shelfDishes = new List<GameObject>();

        readonly List<Dish> shownDishes = new List<Dish> { null, null, null, null, null };

        // Goc 1
        TextMeshProUGUI signText, bubbleText;
        GameObject bubble;
        Transform servedSpot;
        GameObject servedDish;

        // Goc 3
        TextMeshProUGUI pageTitle, pageInfo, pageFooter, rightTitle;
        Transform stepList;
        Image dishFood, dishBase;
        TextMeshProUGUI priceHeader;
        // Gia theo 5 bac Lac vi -> Tuyet hao; Huyen thoai chi co mot mon rieng (constitution muc 5.4) nen khong ghi o day.
        readonly TextMeshProUGUI[] priceCells = new TextMeshProUGUI[5];
        int page;

        // Tay
        Transform handAnchor;
        GameObject handModel;
        string handKey;

        public RoomView(GameController controller, Transform roomRoot)
        {
            g = controller;
            root = roomRoot;
            goc1 = root.Find("Goc1");
            goc2 = root.Find("Goc2");
            goc3 = root.Find("Goc3");
            goc4 = root.Find("Goc4");

            BuildColliders();
            BuildGacha();
            BuildKitchen();
            BuildServe();
            BuildBookAndRelics();
            BuildShadows();

            handAnchor = Toon.Empty(g.Player.Cam.transform, "Tay", new Vector3(0.3f, -0.26f, 0.55f)).transform;
        }

        /// <summary>Tram tuong tac theo ten (dung cho test va huong dan).</summary>
        public Interactable Station(string id)
        {
            Interactable i;
            return stations.TryGetValue(id, out i) ? i : null;
        }

        public IEnumerable<Interactable> Stations { get { return stations.Values; } }

        /// <summary>Do vat 3D cua mot mon nguyen lieu dang nam trong thung (null neu khong co).</summary>
        public ItemView ItemOf(Ingredient ingredient)
        {
            ItemView v;
            return items.TryGetValue(ingredient, out v) ? v : null;
        }

        public IEnumerable<CrateView> Crates { get { return crates.Values; } }
        public GameObject BoardItem { get { return boardItem; } }
        public GameObject HandModel { get { return handModel; } }
        public IReadOnlyList<GameObject> ShelfDishes { get { return shelfDishes; } }

        /// <summary>Do da cho vao dung cu tren bep (nguyen lieu va giot gia vi), theo thu tu.</summary>
        public IReadOnlyList<PanItem> PanItems { get { return panItems; } }
        public Transform PanRoot { get { return panRoot; } }
        public GameObject Jar(string item)
        {
            GameObject j;
            return jars.TryGetValue(item, out j) ? j : null;
        }

        Interactable Register(Interactable i)
        {
            stations[i.Id] = i;
            return i;
        }

        // =====================================================================
        // Va cham: tuong va do dat tren san chan duong di
        // =====================================================================

        void BuildColliders()
        {
            var blockers = new List<Bounds>();
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.gameObject.name == "Outline") continue;
                Bounds b = r.bounds;
                if (b.min.y > 0.15f || b.size.y < 0.4f) continue;           // chi do dung tren san
                if (r.GetComponentInParent<BepNhanPham.FaceSwap>() != null) continue; // khach dung sau quay
                blockers.Add(b);
            }

            var go = new GameObject("VaCham");
            go.transform.SetParent(root, false);

            // San phong 8 x 8 m (Art Bible muc 8): khoi san cua canh khong co collider.
            var floor = go.AddComponent<BoxCollider>();
            floor.center = new Vector3(0f, -0.1f, 0f);
            floor.size = new Vector3(8f, 0.2f, 8f);
            foreach (Bounds b in blockers)
            {
                var c = go.AddComponent<BoxCollider>();
                c.center = go.transform.InverseTransformPoint(b.center);
                c.size = new Vector3(Mathf.Max(b.size.x, 0.05f), Mathf.Max(b.size.y, 2f), Mathf.Max(b.size.z, 0.05f));
            }
            Physics.SyncTransforms();
        }

        // =====================================================================
        // Goc 2 - may nhet ve, bang ti le, thung nguyen lieu
        // =====================================================================

        void BuildGacha()
        {
            // Bang ti le treo tuong (constitution Dieu 4 + Dieu 9; Art Bible muc 13: nam ngang, kem nhat ben trai).
            Canvas board = UiKit.WorldCanvas(goc2, "BangTiLe", new Vector3(0.55f, 3.3f, 3.96f), Quaternion.identity, new Vector2(2700f, 780f));
            Image back = UiKit.Panel(board.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2700f, 780f), UiKit.Hex("#2E4A3F"));
            back.raycastTarget = false;
            oddsHeader = UiKit.Label(back.transform, "TieuDe", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(2560f, 150f), "", 90f);
            oddsHeader.color = UiKit.Cream;
            Transform cells = UiKit.Grid(back.transform, "O", new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(2580f, 450f), 6,
                new Vector2(410f, 450f), 24f).transform;
            for (int tier = 0; tier < 6; tier++)
            {
                Image cell = UiKit.Chip(cells, "Bac_" + tier, "", UiKit.IngredientTier[tier], null, 150f);
                oddsCells[tier] = cell.GetComponentInChildren<TextMeshProUGUI>();
                oddsCells[tier].lineSpacing = -20f;
            }

            // May nhet ve
            GameObject kiosk = Toon.Empty(goc2, "MayNhetVe", new Vector3(-0.1f, 1.4f, 2.8f));
            Register(Interactable.Add(kiosk, "Kiosk", Vector3.zero, new Vector3(0.75f, 0.7f, 0.6f),
                () => g.Session.Wallet.Count > 0 ? "Nhét vé · còn " + g.Session.Wallet.Count + " vé" : "Hết vé",
                () => g.Pull()));
            Canvas kioskSign = UiKit.WorldCanvas(goc2, "BienNhetVe", new Vector3(-0.1f, 1.88f, 2.6f), Quaternion.identity, new Vector2(520f, 150f));
            Image ks = UiKit.Panel(kioskSign.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 150f), UiKit.Mustard);
            UiKit.Text(ks.transform, "Nhét vé", 90f);

            // Ke bac thang tren ban nhan hang: hang truoc do phu, hang sau (cao hon) nguyen lieu chinh.
            crateRoot = Toon.Empty(goc2, "ThungNguyenLieu", Vector3.zero).transform;
            Toon.Prim(PrimitiveType.Cube, crateRoot, "BacKe", new Vector3(-1.45f, 0.97f, 3.42f), new Vector3(1.95f, 0.12f, 0.6f), WoodDark, Toon.OutlineSmall);

            // May gacha do an: mo tu vong 2 (constitution muc 8). Anh cu trong cua so may co chu ve san (trai Dieu 6) nen an di.
            Transform oldIcon = goc2.Find("UpgradeMachine/DishInside");
            if (oldIcon != null) oldIcon.gameObject.SetActive(false);
            Canvas machine = UiKit.WorldCanvas(goc2, "BienMayGacha", new Vector3(1.6f, 0.62f, 2.66f), Quaternion.identity, new Vector2(900f, 260f));
            Image ms = UiKit.Panel(machine.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 260f), UiKit.Cream);
            UiKit.Text(ms.transform, "Máy gacha đồ ăn\n<size=60>sắp mở</size>", 80f).lineSpacing = -30f;
        }

        /// <summary>Vi tri thung thu index trong hang cua loai nguyen lieu.</summary>
        static Vector3 CrateSlot(IngredientKind kind, int index)
        {
            float x = -0.8f - 0.6f * (index % 4);
            float layer = 0.28f * (index / 4);
            return kind == IngredientKind.Side
                ? new Vector3(x, 0.95f + layer, 2.82f)
                : new Vector3(x, 1.12f + layer, 3.42f);
        }

        void RefreshCrates()
        {
            // Bo do vat cua nguyen lieu da dung (vao bep) hoac het ngay.
            var gone = new List<Ingredient>();
            foreach (Ingredient i in items.Keys) if (!g.InStock(i)) gone.Add(i);
            foreach (Ingredient i in gone)
            {
                if (items[i] != null) Object.Destroy(items[i].gameObject);
                items.Remove(i);
            }

            // Moi loai nguyen lieu trong kho co mot thung (constitution muc 4.2).
            var byType = new Dictionary<string, List<Ingredient>>();
            var order = new List<string>();
            foreach (Ingredient i in g.Session.Stock.Items)
            {
                List<Ingredient> list;
                if (!byType.TryGetValue(i.typeId, out list))
                {
                    list = new List<Ingredient>();
                    byType[i.typeId] = list;
                    order.Add(i.typeId);
                }
                list.Add(i);
            }

            var emptied = new List<string>();
            foreach (KeyValuePair<string, CrateView> kv in crates) if (!byType.ContainsKey(kv.Key)) emptied.Add(kv.Key);
            foreach (string id in emptied)
            {
                Object.Destroy(crates[id].gameObject);
                crates.Remove(id);
            }

            int sideIndex = 0, mainIndex = 0;
            foreach (string typeId in order)
            {
                List<Ingredient> list = byType[typeId];
                IngredientKind kind = list[0].kind;
                int slot = kind == IngredientKind.Side ? sideIndex++ : mainIndex++;
                CrateView crate;
                if (!crates.TryGetValue(typeId, out crate))
                {
                    crate = CrateView.Create(crateRoot, list[0], kind == IngredientKind.Side ? SideCrateColor : MainCrateColor);
                    crates[typeId] = crate;
                }
                crate.MoveTo(CrateSlot(kind, slot));

                for (int n = 0; n < list.Count; n++)
                {
                    Ingredient ing = list[n];
                    ItemView view;
                    if (!items.TryGetValue(ing, out view))
                    {
                        view = ItemView.Create(crate.ItemRoot, ing, CrateView.ItemScale, true);
                        Ingredient captured = ing;
                        Interactable.Add(view.gameObject, "Item_" + captured.typeId, new Vector3(0f, 0.04f, 0f),
                            new Vector3(0.085f, 0.1f, 0.08f),
                            () => "Lấy " + captured.name + (captured.prepared ? " đã sơ chế" : "") + " · " + UiKit.IngredientTierName[captured.tier],
                            () => g.TakeIngredient(captured));
                        items[ing] = view;
                    }
                    view.transform.SetParent(crate.ItemRoot, false);
                    view.transform.localPosition = CrateView.ItemPosition(n);
                    bool visible = !ReferenceEquals(ing, g.HeldIngredient) && !ReferenceEquals(ing, g.OnBoard);
                    view.gameObject.SetActive(visible);
                    view.SetPrepared(ing.prepared);
                }
            }
        }

        void RefreshOdds()
        {
            GameSession s = g.Session;
            TierTable table = s.Balance.OddsForDay(s.Day);
            oddsHeader.text = "Tỉ lệ bậc nguyên liệu · " + (s.Balance.IsStormDay(s.Day) ? "Ngày " + s.Day + " (sau mưa)" : "Vòng " + s.Round)
                + " · 1 vé: " + s.Balance.SideCrateSize(s.Day) + " đồ phụ + " + s.Balance.MainCrateSize(s.Day) + " nguyên liệu chính";
            for (int tier = 0; tier < oddsCells.Length; tier++)
            {
                int pct = tier < table.percents.Length ? table.percents[tier] : 0;
                oddsCells[tier].text = pct + "%\n<size=70>" + UiKit.IngredientTierName[tier] + "</size>";
            }
        }

        // =====================================================================
        // Goc 4 - bep
        // =====================================================================

        void BuildKitchen()
        {
            // Thot (trai)
            boardSpot = Toon.Empty(goc4, "ViTriThot", new Vector3(-2.75f, 1.17f, 3.1f)).transform;
            Register(Interactable.Add(boardSpot.gameObject, "Board", new Vector3(0f, 0.1f, 0f), new Vector3(1f, 0.3f, 0.6f),
                BoardPrompt, () => g.UseBoard(), dt => g.HoldBoard(dt)));
            Canvas bar = UiKit.WorldCanvas(goc4, "TienDoSoChe", new Vector3(-2.75f, 1.62f, 3.0f), Quaternion.Euler(20f, 0f, 0f), new Vector2(700f, 110f));
            boardBarRoot = bar.gameObject;
            boardBar = ProgressBar.Create(bar.transform, "Thanh", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 110f), UiKit.Mint);

            // Ke gia vi tren tuong phia tren quay trai: 9 lo do kho (constitution muc 4.5).
            string[] pantry = g.Session.Recipes.pantry;
            for (int shelf = 0; shelf < 2; shelf++)
                Toon.Prim(PrimitiveType.Cube, goc4, "KeGiaVi" + shelf, new Vector3(-2.75f, 1.86f + 0.55f * shelf, 3.84f), new Vector3(2.0f, 0.06f, 0.3f),
                    WoodDark, Toon.OutlineSmall);
            Color[] jarColorList = { UiKit.Mustard, UiKit.Cream, UiKit.Hex("#EFE7DA"), UiKit.Hex("#A5644A"), UiKit.Hex("#5A5250"),
                PlateWhite, UiKit.Hex("#F6D06A"), UiKit.Hex("#F08A6A"), UiKit.Hex("#7DB87A") };
            for (int i = 0; i < pantry.Length; i++)
            {
                int shelf = i < 5 ? 0 : 1;
                int col = shelf == 0 ? i : i - 5;
                float x = shelf == 0 ? -3.55f + 0.4f * col : -3.35f + 0.4f * col;
                float y = 1.89f + 0.55f * shelf;
                string item = pantry[i];
                GameObject jar = Toon.Prim(PrimitiveType.Cylinder, goc4, "Lo_" + item, new Vector3(x, y + 0.15f, 3.82f), new Vector3(0.2f, 0.3f, 0.2f),
                    jarColorList[i % jarColorList.Length], Toon.OutlineSmall);
                // Nap la con cua lo de nghieng cung lo khi rot.
                Toon.Prim(PrimitiveType.Cylinder, jar.transform, "Nap_" + item, new Vector3(0f, 0.17f / 0.15f, 0f), new Vector3(1.1f, 0.05f / 0.15f, 1.1f),
                    WoodDark, Toon.OutlineSmall * 3f);
                jar.AddComponent<Wobble>();
                jars[item] = jar;
                jarColors[item] = jarColorList[i % jarColorList.Length];
                Register(Interactable.Add(jar, "Jar_" + item, Vector3.zero, new Vector3(1.6f, 1.3f, 1.6f),
                    () => g.Session.Stove.Tool == null ? item + " · chọn dụng cụ trước" : "Cho " + item + " vào " + g.Session.Stove.Tool.ToLowerInvariant(),
                    () => g.AddPantry(item)));
                Canvas label = UiKit.WorldCanvas(goc4, "NhanLo_" + item, new Vector3(x, y - 0.05f, 3.68f), Quaternion.identity, new Vector2(380f, 90f));
                Image lb = UiKit.Panel(label.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 90f), UiKit.Cream, false);
                UiKit.Text(lb.transform, item, 56f, TextAlignmentOptions.Center, 6f);
            }

            // Gia dung cu phia tren quay phai.
            Toon.Prim(PrimitiveType.Cube, goc4, "GiaDungCu", new Vector3(2.75f, 1.96f, 3.82f), new Vector3(2.0f, 0.06f, 0.34f), WoodDark, Toon.OutlineSmall);
            for (int i = 0; i < Tools.Length; i++)
            {
                string tool = Tools[i];
                Vector3 spot = new Vector3(2.1f + 0.65f * i, 1.99f, 3.8f);
                GameObject onRack = BuildTool(goc4, tool, spot, "Gia_");
                rackTools[tool] = onRack;
                GameObject hit = Toon.Empty(goc4, "ChonDungCu_" + tool, spot + new Vector3(0f, 0.12f, 0f));
                Register(Interactable.Add(hit, "Tool_" + tool, Vector3.zero, new Vector3(0.55f, 0.3f, 0.4f),
                    () => "Dùng " + tool.ToLowerInvariant(), () => g.SelectTool(tool)));
                Canvas label = UiKit.WorldCanvas(goc4, "NhanDungCu_" + tool, new Vector3(spot.x, 1.88f, 3.64f), Quaternion.identity, new Vector2(500f, 90f));
                Image lb = UiKit.Panel(label.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 90f), UiKit.Cream, false);
                UiKit.Text(lb.transform, tool, 60f, TextAlignmentOptions.Center, 6f);
            }

            // Bep: dung cu dang chon hien tren bep. Noi co mat cua canh la "Noi".
            Transform pot = goc4.Find("Pot");
            potGroup = pot != null ? pot.gameObject : null;
            if (pot != null)
            {
                var steamParts = new List<GameObject>();
                foreach (Transform t in pot) if (t.name.StartsWith("SteamHeart")) steamParts.Add(t.gameObject);
                steam = Toon.Empty(goc4, "HoiNuoc", Vector3.zero);
                foreach (GameObject s in steamParts) s.transform.SetParent(steam.transform, true);
            }
            Toon.Prim(PrimitiveType.Cylinder, goc4, "MiengBep", new Vector3(0f, 1.11f, 3.3f), new Vector3(0.95f, 0.02f, 0.95f), UiKit.Ink, 0f);
            burnerTools["Chảo"] = BuildTool(goc4, "Chảo", new Vector3(0f, 1.12f, 3.3f), "Bep_", 1.6f);
            burnerTools["Vỉ nướng"] = BuildTool(goc4, "Vỉ nướng", new Vector3(0f, 1.12f, 3.3f), "Bep_", 1.6f);

            GameObject stoveHit = Toon.Empty(goc4, "BepNau", new Vector3(0f, 1.45f, 3.3f));
            Register(Interactable.Add(stoveHit, "Stove", Vector3.zero, new Vector3(1.4f, 0.8f, 1.0f), StovePrompt, () => g.UseStove()));

            // Bang nho tren bep: dung cu + thu tu da cho, khoang bac mon, tien do nau (constitution muc 5.3, Dieu 9).
            Canvas panel = UiKit.WorldCanvas(goc4, "BangBep", new Vector3(0f, 2.24f, 2.95f), Quaternion.Euler(-15f, 0f, 0f), new Vector2(1500f, 520f));
            Image pb = UiKit.Panel(panel.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 520f), UiKit.Cream);
            pb.raycastTarget = false;
            stoveSteps = UiKit.Label(pb.transform, "ThuTu", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1420f, 190f), "", 66f);

            // Thanh ti le bac mon: mot dai chu nhat dai, thap; du 6 bac ke ca bac 0% (tin nhan nguoi dung 14/11).
            Image strip = UiKit.Panel(pb.transform, "ThanhTiLe", new Vector2(0.5f, 1f), new Vector2(0f, -222f), new Vector2(BarSize.x, BarSize.y), UiKit.Ink, false);
            strip.raycastTarget = false;
            stoveBarRoot = strip.gameObject;
            float segment = (BarSize.x - 8f) / barCells.Length;
            for (int tier = 0; tier < barCells.Length; tier++)
            {
                RectTransform cell = UiKit.Box(strip.transform, "Bar_" + tier, new Vector2(0f, 0.5f), new Vector2(4f + segment * tier + 2f, 0f),
                    new Vector2(segment - 4f, BarSize.y - 8f));
                var img = cell.gameObject.AddComponent<Image>();
                img.color = UiKit.DishTier[tier];
                img.raycastTarget = false;
                barCells[tier] = UiKit.Text(cell, "", 40f, TextAlignmentOptions.Center, 4f);
                barCells[tier].lineSpacing = -28f;
            }
            stoveStatus = UiKit.Label(pb.transform, "TrangThai", new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(1420f, 90f), "", 54f);
            cookBar = ProgressBar.Create(pb.transform, "DangNau", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1420f, 60f), UiKit.Coral);

            // Num bep (do): vang de nau.
            Transform k = goc4.Find("Stove/Knob0");
            knob = k;
            GameObject knobHit = Toon.Empty(goc4, "NumBep", new Vector3(-1f, 0.86f, 2.74f));
            Register(Interactable.Add(knobHit, "Knob", Vector3.zero, new Vector3(0.3f, 0.3f, 0.2f),
                () => g.Session.Stove.State == StoveState.Cooking ? "Đang nấu" : "Vặn núm: nấu", () => g.StartCooking()));
            Canvas knobLabel = UiKit.WorldCanvas(goc4, "NhanNum", new Vector3(-1f, 0.63f, 2.76f), Quaternion.identity, new Vector2(380f, 90f));
            Image kl = UiKit.Panel(knobLabel.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 90f), UiKit.Coral, false);
            UiKit.Text(kl.transform, "Nấu", 60f).color = UiKit.Cream;

            // Thung rac giua quay trai va bep.
            GameObject bin = Toon.Prim(PrimitiveType.Cylinder, goc4, "ThungRac", new Vector3(-1.52f, 0.3f, 3.35f), new Vector3(0.4f, 0.6f, 0.4f),
                SteelLight, Toon.OutlineBig);
            Toon.Prim(PrimitiveType.Cylinder, goc4, "NapThungRac", new Vector3(-1.52f, 0.62f, 3.35f), new Vector3(0.44f, 0.05f, 0.44f), Steel, Toon.OutlineSmall);
            Register(Interactable.Add(bin, "Trash", Vector3.zero, new Vector3(1.2f, 1.3f, 1.2f), () => "Đổ bỏ đồ trên bếp", () => g.DiscardStove()));

            // Chong dia trong va quay cho 3 o tren quay phai (constitution muc 5.1).
            GameObject stack = Toon.Empty(goc4, "ChongDia", new Vector3(3.45f, 1.1f, 3.15f));
            for (int i = 0; i < 5; i++)
                Toon.Prim(PrimitiveType.Cylinder, stack.transform, "Dia" + i, new Vector3(0f, 0.015f + 0.032f * i, 0f), new Vector3(0.38f, 0.025f, 0.38f),
                    PlateWhite, Toon.OutlineSmall);
            Register(Interactable.Add(stack, "PlateStack", new Vector3(0f, 0.08f, 0f), new Vector3(0.45f, 0.25f, 0.45f),
                () => "Lấy đĩa trống", () => g.TakePlate()));

            for (int i = 0; i < shelfSlots.Length; i++)
            {
                shelfSlots[i] = Toon.Empty(goc4, "QuayCho_" + i, new Vector3(1.95f + 0.5f * i, 1.1f, 3.05f)).transform;
                Toon.Prim(PrimitiveType.Cylinder, shelfSlots[i], "Lot", Vector3.zero, new Vector3(0.44f, 0.01f, 0.44f), UiKit.Hex("#E9CE9E"), 0f);
            }
            Canvas shelfLabel = UiKit.WorldCanvas(goc4, "NhanQuayCho", new Vector3(2.45f, 0.86f, 2.77f), Quaternion.identity, new Vector2(1500f, 120f));
            Image sl = UiKit.Panel(shelfLabel.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 120f), UiKit.Cream, false);
            UiKit.Text(sl.transform, "Quầy chờ", 70f);
        }

        static bool SameDishes(List<Dish> a, List<Dish> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) if (!ReferenceEquals(a[i], b[i])) return false;
            return true;
        }

        /// <summary>Mo hinh dung cu: chao (dia + can), noi nho, vi nuong (khung + nan).</summary>
        static GameObject BuildTool(Transform parent, string tool, Vector3 spot, string prefix, float scale = 1f)
        {
            GameObject t = Toon.Empty(parent, prefix + tool, spot);
            t.transform.localScale = Vector3.one * scale;
            if (tool == "Chảo")
            {
                Toon.Prim(PrimitiveType.Cylinder, t.transform, "Long", new Vector3(0f, 0.03f, 0f), new Vector3(0.36f, 0.06f, 0.36f), Steel, Toon.OutlineSmall);
                Toon.Prim(PrimitiveType.Cube, t.transform, "Can", new Vector3(0.3f, 0.05f, 0f), new Vector3(0.26f, 0.035f, 0.05f), WoodDark, Toon.OutlineSmall);
            }
            else if (tool == "Nồi")
            {
                Toon.Prim(PrimitiveType.Cylinder, t.transform, "Than", new Vector3(0f, 0.11f, 0f), new Vector3(0.32f, 0.22f, 0.32f), UiKit.Coral, Toon.OutlineSmall);
                Toon.Prim(PrimitiveType.Cylinder, t.transform, "Vanh", new Vector3(0f, 0.23f, 0f), new Vector3(0.35f, 0.03f, 0.35f), UiKit.Coral, Toon.OutlineSmall);
            }
            else
            {
                Toon.Prim(PrimitiveType.Cube, t.transform, "Khung", new Vector3(0f, 0.015f, 0f), new Vector3(0.42f, 0.03f, 0.3f), Steel, Toon.OutlineSmall);
                for (int i = 0; i < 4; i++)
                    Toon.Prim(PrimitiveType.Cube, t.transform, "Nan" + i, new Vector3(-0.15f + 0.1f * i, 0.04f, 0f), new Vector3(0.025f, 0.02f, 0.3f),
                        SteelLight, 0f);
            }
            return t;
        }

        // ---- Do trong dung cu tren bep (hinh anh + animation khi cho vao) ----

        /// <summary>Tam mat dung cu, ban kinh xep do va ti le do theo dung cu dang dat tren bep.</summary>
        static void PanSpec(string tool, out Vector3 center, out float radius, out float scale)
        {
            switch (tool)
            {
                // F-013: theo hinh dung cu that (BuildTool x 1,6). Noi: vanh o 1,51 m, mat canh ngay duoi vanh;
                // chao: long 0,36 m x 1,6 -> ban kinh trong ~0,25 m; vi: khung 0,42 x 0,3 m x 1,6.
                case "Nồi": center = new Vector3(0f, 1.47f, 3.3f); radius = 0.22f; scale = 2.2f; return;
                case "Vỉ nướng": center = new Vector3(0f, 1.2f, 3.3f); radius = 0.22f; scale = 2f; return;
                default: center = new Vector3(0f, 1.215f, 3.3f); radius = 0.25f; scale = 2f; return;
            }
        }

        /// <summary>Do cao mat bep (noi dat dung cu, Goc 4) de ve lua ben duoi.</summary>
        const float BurnerY = 1.12f;

        void PlacePanRoot(string tool)
        {
            Vector3 center;
            float radius, scale;
            PanSpec(tool, out center, out radius, out scale);
            if (panRoot == null)
            {
                panRoot = Toon.Empty(goc4, "DoTrongBep", center).transform;
                panView = panRoot.gameObject.AddComponent<PanView>();
            }
            panRoot.localPosition = center;
            panView.Setup(tool, radius, scale, BurnerY - center.y);
            // F-013: moi lan cho vao la mot nhom (lop day / cac mieng rai deu) nam o tam dung cu.
            for (int i = 0; i < panItems.Count; i++)
                if (panItems[i] != null) panItems[i].Retarget(Vector3.zero);
        }

        PanView panView;

        /// <summary>Hinh do trong dung cu (F-013).</summary>
        public PanView Pan { get { return panView; } }

        /// <summary>
        /// Goi ngay khi bep nhan them mot thu (truoc khi ve lai tay): nguyen lieu bay tu tay vao dung cu,
        /// gia vi thi lo nghieng va mot giot mau lo bay vao. Do nam lai trong dung cu toi khi muc / do bo.
        /// </summary>
        public void OnAddedToStove(Ingredient ingredient, string pantryItem)
        {
            Stove stove = g.Session.Stove;
            PlacePanRoot(stove.Tool);
            Vector3 center;
            float radius, scale;
            PanSpec(stove.Tool, out center, out radius, out scale);
            Vector3 slot = Vector3.zero;

            GameObject go;
            Vector3 from;
            if (ingredient != null)
            {
                // F-013 AC-2: roi vao thanh cac mieng da cat rai deu trong long dung cu
                go = panView.AddIngredient(ingredient);
                from = handModel != null ? handModel.transform.position : handAnchor.position;
            }
            else
            {
                // F-013 AC-1: dau / bo / nuoc dung loang thanh lop o day dung cu
                Color c;
                if (!jarColors.TryGetValue(pantryItem, out c)) c = UiKit.Cream;
                go = panView.AddPantry(pantryItem, c);
                GameObject jar = Jar(pantryItem);
                from = jar != null ? jar.transform.position + Vector3.down * 0.05f : panRoot.position + Vector3.up * 0.5f;
                if (jar != null) jar.GetComponent<Wobble>().Play();
            }
            panItems.Add(PanItem.Launch(go, panRoot, from, slot));
        }

        void RefreshPan(Stove stove)
        {
            if (stove.Added.Count == 0 || stove.Added.Count < panItems.Count)
            {
                foreach (PanItem p in panItems) if (p != null) Object.Destroy(p.gameObject);
                panItems.Clear();
                if (panView != null) panView.Clear();
            }
            if (stove.Tool != null && panRoot != null) PlacePanRoot(stove.Tool);
        }

        string BoardPrompt()
        {
            if (g.OnBoard == null)
                return g.Hand == HandKind.Ingredient && !g.HeldIngredient.prepared ? "Đặt " + g.HeldIngredient.name + " lên thớt" : "Thớt";
            return g.OnBoard.prepared ? "Cầm " + g.OnBoard.name + " đã sơ chế" : "Giữ chuột trái: sơ chế " + g.OnBoard.name;
        }

        string StovePrompt()
        {
            Stove stove = g.Session.Stove;
            if (g.Hand == HandKind.Ingredient)
                return g.HeldIngredient.prepared ? "Cho " + g.HeldIngredient.name + " vào bếp" : "Sơ chế " + g.HeldIngredient.name + " trước";
            if (stove.State == StoveState.Ready) return g.Hand == HandKind.Plate ? "Múc " + stove.Recipe.name + " ra đĩa" : "Lấy đĩa trống để múc";
            if (stove.State == StoveState.Cooking) return "Đang nấu " + stove.Recipe.name;
            return stove.Tool == null ? "Bếp · chọn dụng cụ trên giá" : "Bếp · " + stove.Tool;
        }

        void RefreshKitchen()
        {
            GameSession s = g.Session;
            Stove stove = s.Stove;

            // Thot
            if (!ReferenceEquals(boardItemFor, g.OnBoard) || (boardItem != null && g.OnBoard != null))
            {
                if (boardItem != null) Object.Destroy(boardItem);
                boardItem = null;
                boardItemFor = g.OnBoard;
                if (g.OnBoard != null)
                {
                    ItemView v = ItemView.Create(boardSpot, g.OnBoard, 2.2f, false);
                    v.SetPrepared(g.OnBoard.prepared);
                    boardItem = v.gameObject;
                }
            }
            boardBarRoot.SetActive(g.OnBoard != null);

            // Dung cu
            foreach (string tool in Tools)
            {
                bool onStove = stove.Tool == tool;
                rackTools[tool].SetActive(!onStove);
                GameObject burner;
                if (burnerTools.TryGetValue(tool, out burner)) burner.SetActive(onStove);
            }
            if (potGroup != null) potGroup.SetActive(stove.Tool == "Nồi");
            if (steam != null) steam.SetActive(stove.State == StoveState.Cooking);

            // Bang tren bep
            if (stove.Tool == null) stoveSteps.text = "Chọn dụng cụ trên giá → cho từng thứ vào đúng thứ tự → vặn núm";
            else if (stove.Added.Count == 0) stoveSteps.text = stove.Tool + ": chưa cho gì vào";
            else stoveSteps.text = stove.Tool + ": " + string.Join(" → ", stove.Added);

            TierChance[] bar = stove.Bar();
            stoveBarRoot.SetActive(bar != null);
            if (bar != null)
            {
                var pct = new int[barCells.Length];
                foreach (TierChance c in bar)
                    if (c.tier >= 0 && c.tier < pct.Length) pct[c.tier] = Mathf.RoundToInt((float)(c.chance * 100.0));
                for (int tier = 0; tier < barCells.Length; tier++)
                {
                    barCells[tier].text = "<size=28>" + UiKit.DishTierName[tier] + "</size>\n" + pct[tier] + "%";
                    barCells[tier].alpha = pct[tier] > 0 ? 1f : 0.45f;
                }
            }

            RefreshPan(stove);

            switch (stove.State)
            {
                case StoveState.Cooking: stoveStatus.text = "Đang nấu " + stove.Recipe.name; break;
                case StoveState.Ready: stoveStatus.text = stove.Recipe.name + " đã chín"; break;
                default: stoveStatus.text = bar != null ? "Khoảng bậc món nếu nấu đúng công thức" : ""; break;
            }

            // Quay cho: chi dung lai khi danh sach dia hoac dia dang cam doi.
            var signature = new List<Dish>(s.Counter.Dishes);
            signature.Add(g.HeldDish);
            if (SameDishes(signature, shownDishes)) return;
            var previous = new List<Dish>(shownDishes);
            shownDishes.Clear();
            shownDishes.AddRange(signature);
            foreach (GameObject d in shelfDishes) Object.Destroy(d);
            shelfDishes.Clear();
            for (int i = 0; i < s.Counter.Dishes.Count && i < shelfSlots.Length; i++)
            {
                Dish dish = s.Counter.Dishes[i];
                if (ReferenceEquals(dish, g.HeldDish)) continue;
                GameObject model = DishModel.Create(shelfSlots[i], dish, s.Recipes);
                Dish captured = dish;
                Interactable.Add(model, "Dish_" + i, new Vector3(0f, 0.08f, 0f), new Vector3(0.42f, 0.22f, 0.42f),
                    () => "Cầm " + captured.name + " · " + UiKit.DishTierName[captured.tier], () => g.TakeDish(captured));
                Canvas tag = UiKit.WorldCanvas(model.transform, "TenMon", new Vector3(0f, -0.2f, -0.3f), Quaternion.identity, new Vector2(470f, 120f));
                Image tb = UiKit.Panel(tag.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(470f, 120f), UiKit.Cream, false);
                UiKit.Text(tb.transform, captured.name, 50f, TextAlignmentOptions.Center, 6f);
                if (!previous.Contains(dish)) Pop.Play(model, 0.6f); // mon ra lo 0,6 giay (Art Bible muc 11)
                shelfDishes.Add(model);
            }
        }

        // =====================================================================
        // Goc 1 - bien mo quan, chuong, khach
        // =====================================================================

        void BuildServe()
        {
            Transform signBody = goc1.Find("Decor/StandSignBody");
            Vector3 signPos = new Vector3(3.25f, 1.05f, 3.35f);
            GameObject signHit = Toon.Empty(goc1, "BienMoQuan", signPos);
            Register(Interactable.Add(signHit, "ShopSign", Vector3.zero, new Vector3(1.0f, 2.1f, 0.6f),
                () => g.Session.Phase == GamePhase.Prep ? "Mở quán" : g.Session.Phase == GamePhase.Open ? "Đóng quán sớm" : "Quán đã đóng",
                () => g.ToggleShop()));
            Canvas sign = UiKit.WorldCanvas(goc1, "ChuBien", new Vector3(3.25f, 1.25f, 3.07f), Quaternion.identity, new Vector2(700f, 420f));
            Image sb = UiKit.Panel(sign.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 420f), UiKit.Cream);
            signText = UiKit.Text(sb.transform, "", 110f);
            signText.lineSpacing = -30f;
            if (signBody != null) signBody.gameObject.SetActive(true);

            GameObject bell = Toon.Empty(goc1, "Chuong", new Vector3(1.3f, 1.35f, 2.15f));
            Register(Interactable.Add(bell, "Bell", Vector3.zero, new Vector3(0.55f, 0.45f, 0.55f),
                BellPrompt,
                () => g.RingBell()));

            // Bong bong canh dau khach: so thu tu, do de tinh, mon khach goi (constitution muc 6.7, Dieu 9).
            Canvas bubbleCanvas = UiKit.WorldCanvas(goc1, "BongThoai", new Vector3(-1.4f, 2.8f, 3.0f), Quaternion.identity, new Vector2(1300f, 520f));
            Image bb = UiKit.Panel(bubbleCanvas.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 520f), UiKit.Cream);
            bubbleText = UiKit.Text(bb.transform, "", 100f, TextAlignmentOptions.Center, 30f);
            bubbleText.lineSpacing = -20f;
            bubble = bubbleCanvas.gameObject;

            servedSpot = Toon.Empty(goc1, "DiaVuaPhucVu", new Vector3(-0.2f, 1.22f, 2.25f)).transform;
        }

        string BellPrompt()
        {
            Customer c = g.Session.CurrentCustomer;
            if (g.Hand != HandKind.Dish) return c != null ? "Chuông · mang " + c.orderName + " từ quầy chờ tới đây" : "Chuông · cầm đĩa từ quầy chờ tới đây";
            if (c == null) return "Chưa có khách";
            if (g.HeldDish.recipeId != c.orderId) return "Sai món: khách gọi " + c.orderName + " (bấm vẫn đưa, khách sẽ giận)";
            return "Bấm chuông: phục vụ " + g.HeldDish.name;
        }

        void RefreshServe()
        {
            GameSession s = g.Session;
            switch (s.Phase)
            {
                case GamePhase.Prep: signText.text = "MỞ QUÁN\n<size=60>bấm để đón khách</size>"; break;
                case GamePhase.Open:
                    // 21:00: bien tu lat sang DA DONG, khach dang ngoi van cho mon (constitution 1.4 muc 6.2)
                    signText.text = s.IsClosedForNewCustomers
                        ? "ĐÃ ĐÓNG\n<size=60>phục vụ nốt khách</size>"
                        : "ĐANG MỞ\n<size=60>bấm để đóng sớm</size>";
                    break;
                default: signText.text = "ĐÃ ĐÓNG"; break;
            }

            bool waiting = s.Phase == GamePhase.Open && s.CurrentCustomer != null;
            bubble.SetActive(waiting);
            if (waiting)
                bubbleText.text = "<size=78>Khách " + s.CustomersToday + " · Dễ tính " + s.CurrentCustomer.easiness + "%</size>\n"
                    + "Gọi: " + s.CurrentCustomer.orderName;

            if (servedDish != null) Object.Destroy(servedDish);
            servedDish = null;
            if (g.ServedShowLeft > 0f && g.LastServed != null) servedDish = DishModel.Create(servedSpot, g.LastServed, s.Recipes);
        }

        // =====================================================================
        // Goc 3 - sach nau an, cua hang relic (sprint 2)
        // =====================================================================

        void BuildBookAndRelics()
        {
            // Trang sach nam tren bia sach, nghieng theo gia sach (giong anh BookPages cu: 1,78 x 1,087 m, nghieng 47 do).
            Quaternion tilt = Quaternion.Euler(47.03f, 0f, 0f);
            Vector3 center = new Vector3(-2.2f, 1.479f, 2.973f) - tilt * Vector3.forward * 0.006f;
            Canvas book = UiKit.WorldCanvas(goc3, "TrangSach", center, tilt, new Vector2(1780f, 1087f));
            var paper = UiKit.Box(book.transform, "Giay", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1780f, 1087f)).gameObject.AddComponent<Image>();
            paper.color = UiKit.Hex("#FFF3DC");
            paper.raycastTarget = false;
            var spine = UiKit.Box(book.transform, "Gay", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 1087f)).gameObject.AddComponent<Image>();
            spine.color = UiKit.Ink;

            // Trang trai: ten mon, dung cu, de mon, thoi gian, hinh dia mon (nhu ban phac).
            RectTransform left = UiKit.Box(book.transform, "TrangTrai", new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(890f, 1087f));
            pageTitle = UiKit.Label(left, "TenMon", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(820f, 170f), "", 92f);
            pageInfo = UiKit.Label(left, "ThongTin", new Vector2(0.5f, 1f), new Vector2(0f, -215f), new Vector2(820f, 90f), "", 48f);
            // Hinh dia mon nhu ban phac: dia trang hinh bau duc, mon u len o giua.
            dishBase = UiKit.Box(left, "Dia", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(480f, 150f)).gameObject.AddComponent<Image>();
            dishBase.sprite = UiKit.Circle;
            dishBase.color = PlateWhite;
            dishFood = UiKit.Box(left, "Mon", new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(280f, 150f)).gameObject.AddComponent<Image>();
            dishFood.sprite = UiKit.Circle;

            // Gia ban theo bac mon (tin nhan nguoi dung 14/11; constitution muc 5.2, 5.8): moi o to mau bac mon.
            priceHeader = UiKit.Label(left, "TieuDeGia", new Vector2(0.5f, 0f), new Vector2(0f, 260f), new Vector2(820f, 70f), "", 48f);
            RectTransform prices = UiKit.Box(left, "GiaBan", new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(820f, 130f));
            float cell = 820f / priceCells.Length;
            for (int tier = 0; tier < priceCells.Length; tier++)
            {
                Image c = UiKit.Panel(prices, "Gia_" + tier, new Vector2(0f, 0.5f), new Vector2(cell * tier + 4f, 0f), new Vector2(cell - 8f, 126f),
                    UiKit.DishTier[tier], false);
                c.raycastTarget = false;
                // Ten bac va gia la hai dong chu rieng: ten dai ("Binh thuong") tu thu nho, khong lam nho so gia.
                UiKit.Label(c.transform, "Ten", new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(cell - 16f, 44f), UiKit.DishTierName[tier], 28f);
                priceCells[tier] = UiKit.Label(c.transform, "Gia", new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(cell - 16f, 66f), "", 50f);
            }
            pageFooter = UiKit.Label(left, "Trang", new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(820f, 70f), "", 44f);

            // Trang phai: thu tu cho vao, moi buoc mot so tron.
            RectTransform right = UiKit.Box(book.transform, "TrangPhai", new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(890f, 1087f));
            rightTitle = UiKit.Label(right, "TieuDe", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(820f, 130f), "", 74f, TextAlignmentOptions.Left);
            stepList = UiKit.Box(right, "CacBuoc", new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(820f, 820f));
            var layout = stepList.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Bam nua trang de lat (constitution muc 5.8).
            GameObject prev = Toon.Empty(book.transform, "LatLui", new Vector3(-445f, 0f, 0f));
            prev.transform.localRotation = Quaternion.identity;
            Register(Interactable.Add(prev, "BookPrev", Vector3.zero, new Vector3(880f, 1080f, 40f), () => "Lật trang trước", () => TurnPage(-1)));
            GameObject next = Toon.Empty(book.transform, "LatToi", new Vector3(445f, 0f, 0f));
            Register(Interactable.Add(next, "BookNext", Vector3.zero, new Vector3(880f, 1080f, 40f), () => "Lật trang sau", () => TurnPage(1)));

            // Cua hang relic: lam o sprint 2. Gia relic de trong (constitution muc 7.1).
            Canvas shop = UiKit.WorldCanvas(goc3, "BienRelic", new Vector3(1.15f, 2.95f, 3.95f), Quaternion.identity, new Vector2(2200f, 420f));
            Image shb = UiKit.Panel(shop.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2200f, 420f), UiKit.Lilac);
            UiKit.Text(shb.transform, "Cửa hàng relic\n<size=80>sắp mở</size>", 130f).lineSpacing = -30f;
            Transform rack = goc3.Find("OwnedRack");
            if (rack != null)
                foreach (Transform t in rack)
                    if (t.name.StartsWith("Slot")) Toon.Recolor(t.gameObject, UiKit.Hex("#E3CFA6"));
        }

        List<Recipe> BookRecipes()
        {
            var list = new List<Recipe>();
            int round = g.Session.Round;
            foreach (Recipe r in g.Session.Recipes.recipes)
                if (r.group == StandardGroup && r.unlockRound >= 1 && r.unlockRound <= round) list.Add(r);
            return list;
        }

        void TurnPage(int delta)
        {
            int count = BookRecipes().Count;
            if (count == 0) return;
            page = ((page + delta) % count + count) % count;
            RefreshBook();
        }

        public int Page { get { return page; } }

        void RefreshBook()
        {
            List<Recipe> recipes = BookRecipes();
            if (recipes.Count == 0) return;
            page = Mathf.Clamp(page, 0, recipes.Count - 1);
            Recipe r = recipes[page];
            pageTitle.text = r.name;
            pageInfo.text = r.tool + " · đế " + r.baseName + " · nấu " + r.cookSeconds.ToString("0.#") + " giây";
            pageFooter.text = "Trang " + (page + 1) + " / " + recipes.Count + " công thức";
            dishFood.color = IngredientArt.ColorOf(r.mainId);
            GameSession s = g.Session;
            priceHeader.text = "Giá bán · vòng " + s.Round + " (khách trả thêm tip)";
            for (int tier = 0; tier < priceCells.Length; tier++)
                priceCells[tier].text = Payment.Price(s.Balance, s.Round, tier) + "G";
            rightTitle.text = "Thứ tự cho vào " + r.tool.ToLowerInvariant();

            UiKit.Clear(stepList);
            for (int i = 0; i < r.steps.Length; i++)
            {
                RectTransform row = UiKit.Box(stepList, "Buoc_" + (i + 1), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(820f, 150f));
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 150f;
                Color c = StepColor(r, r.steps[i]);
                Image circle = UiKit.Panel(row, "So", new Vector2(0f, 0.5f), new Vector2(75f, 0f), new Vector2(150f, 150f), c, false);
                circle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                UiKit.Text(circle.transform, (i + 1).ToString(), 84f);
                UiKit.Label(row, "Ten", new Vector2(0f, 0.5f), new Vector2(190f, 0f), new Vector2(620f, 150f), r.steps[i], 70f, TextAlignmentOptions.Left)
                    .rectTransform.pivot = new Vector2(0f, 0.5f);
            }
        }

        Color StepColor(Recipe r, string step)
        {
            Color c = UiKit.Hex("#F3DFB8");
            foreach (SideIngredient s in g.Session.Recipes.sides) if (s.name == step) c = IngredientArt.ColorOf(s.id);
            foreach (MainIngredient m in g.Session.Recipes.mains) if (m.name == step) c = IngredientArt.ColorOf(m.id);
            // Mau qua nhat (trung, dau phu) se lan vao giay sach: dung mau go nhat thay the.
            float luminance = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            return luminance > 0.9f ? UiKit.Hex("#E9CE9E") : c;
        }

        // =====================================================================
        // Bong tron duoi nhan vat va may (Art Bible muc 10)
        // =====================================================================

        void BuildShadows()
        {
            Transform customer = goc1.Find("Customer");
            if (customer != null) Toon.BlobShadow(customer, "Bong", new Vector3(0f, 0.01f, 3.25f), 1.4f);
            Toon.BlobShadow(goc1, "Bong_Bien", new Vector3(3.25f, 0.01f, 3.35f), 1.0f);
            Toon.BlobShadow(goc2, "Bong_MayNhetVe", new Vector3(-0.1f, 0.01f, 2.8f), 0.7f);
            Toon.BlobShadow(goc2, "Bong_MayGacha", new Vector3(1.6f, 0.01f, 3.2f), 1.8f);
            Toon.BlobShadow(goc3, "Bong_GiaSach", new Vector3(-2.2f, 0.01f, 3f), 1.0f);
            for (int i = 0; i < 3; i++) Toon.BlobShadow(goc3, "Bong_Be" + i, new Vector3(-0.1f + 1.25f * i, 0.01f, 3.2f), 1.1f);
            Toon.BlobShadow(goc4, "Bong_ThungRac", new Vector3(-1.52f, 0.01f, 3.35f), 0.55f);
        }

        // =====================================================================
        // Ve lai
        // =====================================================================

        public void Refresh()
        {
            RefreshOdds();
            RefreshCrates();
            RefreshKitchen();
            RefreshServe();
            RefreshBook();
            RefreshHand();
        }

        void RefreshHand()
        {
            string key;
            switch (g.Hand)
            {
                case HandKind.Ingredient: key = "I" + g.HeldIngredient.GetHashCode() + (g.HeldIngredient.prepared ? "p" : ""); break;
                case HandKind.Plate: key = "P"; break;
                case HandKind.Dish: key = "D" + g.HeldDish.GetHashCode(); break;
                default: key = null; break;
            }
            if (key == handKey) return;
            handKey = key;
            if (handModel != null) Object.Destroy(handModel);
            handModel = null;
            switch (g.Hand)
            {
                case HandKind.Ingredient:
                    ItemView v = ItemView.Create(handAnchor, g.HeldIngredient, 2.2f, false);
                    v.SetPrepared(g.HeldIngredient.prepared);
                    handModel = v.gameObject;
                    break;
                case HandKind.Plate:
                    handModel = DishModel.EmptyPlate(handAnchor);
                    break;
                case HandKind.Dish:
                    handModel = DishModel.Create(handAnchor, g.HeldDish, g.Session.Recipes, false);
                    break;
            }
            if (handModel != null)
            {
                foreach (Collider c in handModel.GetComponentsInChildren<Collider>()) Object.Destroy(c);
                foreach (Interactable i in handModel.GetComponentsInChildren<Interactable>()) Object.Destroy(i);
            }
        }

        /// <summary>Moi khung hinh: thanh tien do, num bep, hieu ung xuat hien.</summary>
        public void Tick(float dt)
        {
            GameSession s = g.Session;
            boardBar.Set((float)s.Board.Progress + (g.OnBoard != null && g.OnBoard.prepared ? 1f : 0f));
            Stove stove = s.Stove;
            if (stove.State == StoveState.Cooking && stove.CookSeconds > 0f) cookBar.Set(stove.Elapsed / stove.CookSeconds);
            else cookBar.Set(stove.State == StoveState.Ready ? 1f : 0f);
            if (knob != null) knob.localEulerAngles = new Vector3(-90f, stove.State == StoveState.Cooking ? 90f : 0f, 0f);
            bool cooking = stove.State == StoveState.Cooking;
            foreach (PanItem p in panItems) if (p != null) p.Sizzle = cooking;
            if (panView != null) panView.Tick(stove, stove.Recipe, dt);
            foreach (CrateView c in crates.Values) c.Tick(dt);
            foreach (ItemView i in items.Values) if (i != null) i.Tick(dt);
        }
    }

    /// <summary>Mau chinh cua tung loai nguyen lieu (sach nau an, giot trong bep, mon tren dia). Hinh 3D: IngredientModels.</summary>
    public static class IngredientArt
    {
        static readonly Dictionary<string, string> Colors = new Dictionary<string, string>
        {
            { "ca_chua", "#E8604F" }, { "hanh_tay", "#D98FB0" }, { "rau_muong", "#7DB87A" }, { "khoai_tay", "#D69B68" },
            { "nam_rom", "#C9C1B6" }, { "bong_cai", "#7DB87A" }, { "ot_chuong", "#D9382C" }, { "bi_do", "#F08A6A" },
            { "mang_tay", "#7DB87A" }, { "nam_dong_co", "#A5644A" },
            { "trung", "#F6C445" }, { "heo", "#F2A0A6" }, { "dau_phu", "#EFE7DA" }, { "bo", "#A5644A" }, { "tom", "#F08A6A" },
            { "ca_hoi", "#F08A6A" }, { "muc", "#EFE7DA" }, { "ga", "#F6D06A" },
            { "tom_hum", "#D9382C" }, { "wagyu", "#E9A0A0" }, { "ca_ngu", "#C94A3B" }, { "cua", "#E8604F" },
        };

        public static Color ColorOf(string id)
        {
            string hex;
            return Colors.TryGetValue(id, out hex) ? UiKit.Hex(hex) : UiKit.Hex("#E9CE9E");
        }
    }

    /// <summary>
    /// Mot mon nguyen lieu 3D: hinh theo loai (IngredientModels) + de bac to mau bac (Art Bible muc 7: "mau khung";
    /// constitution Dieu 9) + mat theo bac. Da so che thi doi sang hinh theo kieu so che (F-012 AC-2).
    /// </summary>
    public class ItemView : MonoBehaviour
    {
        const float DiscDiameter = 0.092f;
        const float DiscHeight = 0.012f;
        static readonly Color SpoiledBody = UiKit.Hex("#8F7F5A");

        Transform shape;
        float appear = 1f;
        float baseScale = 1f;
        string typeId;
        bool prepared;

        public int Tier { get; private set; }
        public Color DiscColor { get; private set; }
        public GameObject Disc { get; private set; }

        /// <summary>Mat cua mon (anh theo bac).</summary>
        public FaceBillboard Face { get; private set; }

        /// <summary>Dang hien hinh da so che.</summary>
        public bool ShowsPrepared { get { return prepared; } }

        /// <summary>So khoi ghep thanh hinh hien tai (F-012 AC-1, AC-3).</summary>
        public int PartCount { get { return shape == null ? 0 : shape.GetComponentsInChildren<MeshRenderer>().Length - CountOutlines(); } }

        int CountOutlines()
        {
            int n = 0;
            foreach (MeshRenderer r in shape.GetComponentsInChildren<MeshRenderer>()) if (r.name == "Outline") n++;
            return n;
        }

        public static ItemView Create(Transform parent, Ingredient ing, float scale, bool animate)
        {
            var go = new GameObject("NguyenLieu_" + ing.typeId);
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<ItemView>();
            v.baseScale = scale;
            v.appear = animate ? 0f : 1f;
            go.transform.localScale = Vector3.one * (animate ? 0.01f : scale);
            v.Tier = ing.tier;
            v.typeId = ing.typeId;
            v.DiscColor = UiKit.IngredientTier[Mathf.Clamp(ing.tier, 0, UiKit.IngredientTier.Length - 1)];
            v.Disc = Toon.Prim(PrimitiveType.Cylinder, go.transform, "DeBac", new Vector3(0f, DiscHeight * 0.5f, 0f), new Vector3(DiscDiameter, DiscHeight, DiscDiameter),
                v.DiscColor, 0.006f);
            v.BuildShape(false);
            return v;
        }

        void BuildShape(bool asPrepared)
        {
            if (shape != null) { shape.gameObject.SetActive(false); Destroy(shape.gameObject); }
            if (Face != null) { Face.gameObject.SetActive(false); Destroy(Face.gameObject); }
            prepared = asPrepared;

            var holder = Toon.Empty(transform, "GiuHinh", new Vector3(0f, DiscHeight, 0f)).transform;
            float radius;
            // anh 08: bac 0 than xin mau
            Transform root = IngredientModels.Build(holder, typeId, asPrepared, Tier == 0 ? SpoiledBody : (Color?)null, 0.6f, out radius);
            shape = holder;
            Transform center = root.Find("TamHinh");

            // Mat theo bac (Art Bible muc 7: bac doc bang mau khung + bieu cam + phu kien), luon quay ve nguoi choi.
            // Day mat ra bang ban kinh bao cua ca hinh de mat khong lot vao khoi nao khi nhin xeo.
            float faceSize = Mathf.Clamp(radius * 2.2f, 0.09f, 0.14f);
            Face = FaceBillboard.Create(transform, center, FaceArt.Ingredient(Tier), faceSize, radius);
            Face.SetSizeFactor(asPrepared ? 0.8f : 1f);
        }

        public void SetPrepared(bool isPrepared)
        {
            if (isPrepared == prepared && shape != null) return;
            BuildShape(isPrepared);
        }

        /// <summary>Lo bac: hien dan trong 0,4 giay (Art Bible muc 11); bac 4-5 lau hon.</summary>
        public void Tick(float dt)
        {
            if (appear >= 1f) return;
            appear = Mathf.Min(1f, appear + dt / (Tier >= 4 ? 1.5f : 0.4f));
            float sc = appear < 1f ? Mathf.Sin(appear * Mathf.PI * 0.5f) * (1f + 0.12f * Mathf.Sin(appear * Mathf.PI)) : 1f;
            transform.localScale = Vector3.one * Mathf.Max(0.01f, sc) * baseScale;
        }
    }

    /// <summary>Thung go mo nap chua mot loai nguyen lieu; roi tu cong dich chuyen xuong (Art Bible muc 11: 1,2 giay).</summary>
    public class CrateView : MonoBehaviour
    {
        const float DropHeight = 1.4f;
        const float DropSeconds = 1.2f;
        static readonly Vector2 Size = new Vector2(0.55f, 0.46f);

        Vector3 target;
        float drop = 1f;
        public Transform ItemRoot { get; private set; }
        public string TypeId { get; private set; }
        public TextMeshProUGUI Label { get; private set; }

        public static CrateView Create(Transform parent, Ingredient first, Color wood)
        {
            var go = new GameObject("Thung_" + first.typeId);
            go.transform.SetParent(parent, false);
            var c = go.AddComponent<CrateView>();
            c.TypeId = first.typeId;
            Toon.Prim(PrimitiveType.Cube, go.transform, "Day", new Vector3(0f, 0.02f, 0f), new Vector3(Size.x, 0.04f, Size.y), wood, Toon.OutlineSmall);
            // Thanh thung thap de luon thay mau de bac cua tung mon (constitution Dieu 9).
            Toon.Prim(PrimitiveType.Cube, go.transform, "ThanhTruoc", new Vector3(0f, 0.035f, -Size.y * 0.5f), new Vector3(Size.x, 0.07f, 0.03f), wood, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, go.transform, "ThanhSau", new Vector3(0f, 0.06f, Size.y * 0.5f), new Vector3(Size.x, 0.12f, 0.03f), wood, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, go.transform, "ThanhTrai", new Vector3(-Size.x * 0.5f, 0.045f, 0f), new Vector3(0.03f, 0.09f, Size.y), wood, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, go.transform, "ThanhPhai", new Vector3(Size.x * 0.5f, 0.045f, 0f), new Vector3(0.03f, 0.09f, Size.y), wood, Toon.OutlineSmall);
            c.ItemRoot = Toon.Empty(go.transform, "Mon", new Vector3(0f, 0.045f, 0f)).transform;

            Canvas label = UiKit.WorldCanvas(go.transform, "Nhan", new Vector3(0f, 0.06f, -Size.y * 0.5f - 0.02f), Quaternion.identity, new Vector2(500f, 100f));
            Image lb = UiKit.Panel(label.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 100f), UiKit.Cream, false);
            c.Label = UiKit.Text(lb.transform, first.name, 64f, TextAlignmentOptions.Center, 4f);
            return c;
        }

        /// <summary>
        /// Vi tri mon thu n trong thung: moi lop 4 cot x 3 hang, nhieu hon thi chat len lop tren.
        /// Cot dau o phia giua phong (gan nguoi choi nhat) de mon dau tien khong bi mon khac che.
        /// </summary>
        public static Vector3 ItemPosition(int n)
        {
            const int cols = 4, rows = 3;
            int layer = n / (cols * rows);
            int k = n % (cols * rows);
            int col = k % cols, row = k / cols;
            return new Vector3(0.195f - 0.13f * col, 0.1f * layer, -0.13f + 0.13f * row);
        }

        /// <summary>Nguyen lieu trong thung to hon kich thuoc goc de nhin ro mau de bac tu xa.</summary>
        public const float ItemScale = 1.4f;

        public void MoveTo(Vector3 localSlot)
        {
            if (target == localSlot) return;
            bool first = drop >= 1f && transform.localPosition == Vector3.zero;
            target = localSlot;
            if (first)
            {
                drop = 0f;
                transform.localPosition = target + Vector3.up * DropHeight;
            }
            else transform.localPosition = target;
        }

        public void Tick(float dt)
        {
            if (drop >= 1f) return;
            drop = Mathf.Min(1f, drop + dt / DropSeconds);
            // roi xuong roi nay 2 lan (Art Bible muc 11)
            float fall = Mathf.Clamp01(drop / 0.6f);
            float y = (1f - fall * fall) * DropHeight;
            if (drop > 0.6f) y = Mathf.Abs(Mathf.Sin((drop - 0.6f) / 0.4f * Mathf.PI * 2f)) * 0.08f * (1f - drop);
            transform.localPosition = target + Vector3.up * y;
        }
    }

    /// <summary>Dia mon an: vien dia mau bac (Art Bible muc 7), de mon va mon theo nguyen lieu chinh.</summary>
    public static class DishModel
    {
        /// <summary>De mon (constitution muc 4.5): com, mi Y, bun, banh mi o nua sau dia.</summary>
        static void BuildBase(Transform plate, string baseName)
        {
            Vector3 at = new Vector3(-0.075f, 0.03f, 0.06f);
            switch (baseName)
            {
                case "Mì Ý":
                    Toon.Prim(PrimitiveType.Cylinder, plate, "DeMon", at + new Vector3(0f, 0.012f, 0f), new Vector3(0.13f, 0.03f, 0.11f), UiKit.Hex("#F2D27A"), 0.006f);
                    for (int i = 0; i < 3; i++)
                        Toon.Prim(PrimitiveType.Cylinder, plate, "SoiMi_" + i, at + new Vector3(-0.03f + 0.03f * i, 0.03f, 0f), new Vector3(0.006f, 0.1f, 0.006f), UiKit.Hex("#E9BE5C"), 0f, new Vector3(90f, 20f * i, 0f));
                    break;
                case "Bún":
                    Toon.Prim(PrimitiveType.Cylinder, plate, "DeMon", at + new Vector3(0f, 0.012f, 0f), new Vector3(0.13f, 0.03f, 0.11f), UiKit.Hex("#FBF7EE"), 0.006f);
                    for (int i = 0; i < 3; i++)
                        Toon.Prim(PrimitiveType.Cylinder, plate, "SoiBun_" + i, at + new Vector3(-0.03f + 0.03f * i, 0.03f, 0f), new Vector3(0.005f, 0.1f, 0.005f), UiKit.Hex("#EDE6D8"), 0f, new Vector3(90f, -15f * i, 0f));
                    break;
                case "Bánh mì":
                    Toon.Prim(PrimitiveType.Capsule, plate, "DeMon", at + new Vector3(0f, 0.025f, 0f), new Vector3(0.05f, 0.07f, 0.05f), UiKit.Hex("#D9963A"), 0.006f, new Vector3(0f, 30f, 90f));
                    break;
                default: // Com: bat com up
                    Toon.Prim(PrimitiveType.Sphere, plate, "DeMon", at + new Vector3(0f, 0.02f, 0f), new Vector3(0.13f, 0.07f, 0.11f), UiKit.Hex("#FFFDF6"), 0.006f);
                    Toon.Prim(PrimitiveType.Sphere, plate, "Hat", at + new Vector3(0.02f, 0.052f, -0.01f), new Vector3(0.012f, 0.006f, 0.008f), UiKit.Hex("#F3EEDF"), 0f);
                    break;
            }
        }

        public static GameObject EmptyPlate(Transform parent)
        {
            GameObject go = Toon.Empty(parent, "DiaTrong", Vector3.zero);
            Toon.Prim(PrimitiveType.Cylinder, go.transform, "Dia", new Vector3(0f, 0.015f, 0f), new Vector3(0.36f, 0.03f, 0.36f), UiKit.Hex("#FFFFFF"), Toon.OutlineSmall);
            return go;
        }

        /// <summary>
        /// Dia mon: vien dia mau bac, mon co mat theo bac mon va hoi bay len theo bac (Art Bible muc 7).
        /// fx = false khi cam tren tay (hoi khong che tam nhin).
        /// </summary>
        public static GameObject Create(Transform parent, Dish dish, RecipeBook book, bool fx = true)
        {
            int tier = Mathf.Clamp(dish.tier, 0, UiKit.DishTier.Length - 1);
            GameObject go = Toon.Empty(parent, "DiaMon", Vector3.zero);
            Color rim = UiKit.DishTier[tier];
            Toon.Prim(PrimitiveType.Cylinder, go.transform, "VienDia", new Vector3(0f, 0.012f, 0f), new Vector3(0.38f, 0.024f, 0.38f), rim, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cylinder, go.transform, "LongDia", new Vector3(0f, 0.02f, 0f), new Vector3(0.28f, 0.024f, 0.28f), UiKit.Hex("#FFFFFF"), 0f);
            string mainId = dish.recipeId != null && dish.recipeId.Contains("__") ? dish.recipeId.Substring(0, dish.recipeId.IndexOf("__")) : "";
            string sideId = dish.recipeId != null && dish.recipeId.Contains("__") ? dish.recipeId.Substring(dish.recipeId.IndexOf("__") + 2) : "";
            BuildBase(go.transform, dish.baseName);
            // F-013 AC-5: dia co dung hinh mon vua thay trong dung cu (cung DishFood) + de mon; Lac vi = chay xem (AC-6)
            string method = null;
            if (book != null) foreach (MainIngredient m in book.mains) if (m.id == mainId) method = m.method;
            GameObject food = DishFood.Build(go.transform, mainId, sideId, method, tier == 0, 0.1f, 1.1f, new Vector3(0.04f, 0.03f, -0.035f));
            Transform center = food.transform.Find("TamMon");
            FaceBillboard.Create(go.transform, center != null ? center : food.transform, FaceArt.Dish(tier), 0.17f, 0.1f, "MatMon");
            if (fx) DishFx.Create(go.transform, tier, null);
            return go;
        }
    }
}
