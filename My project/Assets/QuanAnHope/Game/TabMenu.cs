using System.Collections.Generic;
using System.Text;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Bang Tab (sprint 8 F-078, yeu cau 21/11): the Relic (moi relic dang co, bo relic khi day) va the Cua hang (may gacha do an,
    /// ve den, nuoc sot than thanh). Man ngoai phong nhu menu tam dung (constitution 1.8 Dieu 9: duoc dung nut), dong ho dung.
    /// Quy cach Art Bible muc 13: the kem giua man, nen toi 72%, nut cao toi thieu 52 px, mau nut theo vai tro.
    /// </summary>
    public class TabMenu
    {
        public enum Page { Relic, Shop }

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f), Top = new Vector2(0.5f, 1f), TopLeft = new Vector2(0f, 1f);
        const int Rows = 4;

        readonly GameController g;
        readonly GameObject root;
        readonly RectTransform relicList, shopList;
        readonly TextMeshProUGUI header, note;
        readonly Button relicTab, shopTab;
        string confirmId;

        public Page Current { get; private set; }
        /// <summary>Canvas cua bang (test chup anh).</summary>
        public Canvas Canvas { get { return root.GetComponent<Canvas>(); } }
        public bool Shown { get { return root.activeSelf; } }
        public string HeaderText { get { return header.text; } }
        public string NoteText { get { return note.text; } }

        /// <summary>Nut "Bo" cua tung relic dang hien (test), theo id.</summary>
        public readonly Dictionary<string, Button> DiscardButtons = new Dictionary<string, Button>();
        /// <summary>Nut "Mua" theo GameController.ItemSauce / ItemBlack / ItemMachine (test).</summary>
        public readonly Dictionary<string, Button> BuyButtons = new Dictionary<string, Button>();
        /// <summary>Chu tung dong cua hang (test: gia, so dang co, ti le).</summary>
        public readonly Dictionary<string, TextMeshProUGUI> ShopLines = new Dictionary<string, TextMeshProUGUI>();

        public TabMenu(GameController controller)
        {
            g = controller;
            var go = new GameObject("BangTab", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 28;   // tren HUD, duoi menu chinh
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            root = go;

            RectTransform dim = UiKit.Stretch(go.transform, "NenToi");
            var di = dim.gameObject.AddComponent<Image>();
            di.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.72f);
            Image card = UiKit.Panel(dim, "The", Center, Vector2.zero, new Vector2(1020f, 620f), UiKit.Cream);
            relicTab = UiKit.Button(card.transform, "Tab_Relic", TopLeft, new Vector2(28f, -20f), new Vector2(220f, 52f), "Relic", UiKit.Lilac, () => Show(Page.Relic), 22f);
            shopTab = UiKit.Button(card.transform, "Tab_CuaHang", TopLeft, new Vector2(262f, -20f), new Vector2(220f, 52f), "Cửa hàng", UiKit.Mustard, () => Show(Page.Shop), 22f);
            ((RectTransform)relicTab.transform).pivot = TopLeft;
            ((RectTransform)shopTab.transform).pivot = TopLeft;
            UiKit.Button(card.transform, "Btn_Dong", new Vector2(1f, 1f), new Vector2(-28f, -20f), new Vector2(170f, 52f), "Đóng (Tab)", UiKit.Mint, () => g.CloseTab(), 20f)
                .transform.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            header = UiKit.Label(card.transform, "TieuDe", Top, new Vector2(0f, -88f), new Vector2(960f, 40f), "", 24f);
            relicList = UiKit.Box(card.transform, "DanhSachRelic", Top, new Vector2(0f, -136f), new Vector2(960f, 420f));
            shopList = UiKit.Box(card.transform, "DanhSachCuaHang", Top, new Vector2(0f, -136f), new Vector2(960f, 420f));
            note = UiKit.Label(card.transform, "GhiChu", new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(960f, 40f), "", 16f);
            root.SetActive(false);
        }

        public void Show(Page page)
        {
            Current = page;
            confirmId = null;
            if (page == Page.Shop) g.NoteShopView();
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
            relicTab.interactable = Current != Page.Relic;
            shopTab.interactable = Current != Page.Shop;
            if (Current == Page.Relic) BuildRelics();
            else BuildShop();
        }

        // ---------------- the Relic ----------------

        void BuildRelics()
        {
            GameSession s = g.Session;
            RelicInventory inv = s.Relics;
            UiKit.Clear(relicList);
            DiscardButtons.Clear();
            bool pending = g.PendingRelicSlot >= 0;
            RelicDef want = !pending ? null : g.PendingIsBlack
                ? (g.PendingRelicSlot < s.BlackRelicOffers.Count ? s.BlackRelicOffers[g.PendingRelicSlot] : null)
                : (g.PendingRelicSlot < s.Shop.Offers.Count ? s.Shop.Offers[g.PendingRelicSlot] : null);
            header.text = "Relic đang có · " + inv.Owned.Count + "/" + inv.Capacity
                + (pending && want != null ? " · <color=#E8604F>giá đầy: chọn 1 relic để bỏ, lấy " + want.name + "</color>" : "");
            for (int i = 0; i < inv.Owned.Count; i++)
            {
                RelicDef r = inv.Owned[i];
                int col = i / Rows, row = i % Rows;
                Image box = UiKit.Panel(relicList, "Relic_" + r.id, TopLeft, new Vector2(col * 485f, -row * 104f), new Vector2(470f, 96f), UiKit.Hex("#FBF0DA"), false);
                ((RectTransform)box.transform).pivot = TopLeft;
                Image rim = UiKit.Panel(box.transform, "DoHiem", new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(14f, 80f), RelicArt.RarityColor(r.rarity), false);
                ((RectTransform)rim.transform).pivot = new Vector2(0f, 0.5f);
                TextMeshProUGUI t = UiKit.Label(box.transform, "Chu", new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(330f, 90f),
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
                UiKit.Label(relicList, "Trong", Center, Vector2.zero, new Vector2(900f, 60f), "Chưa có relic. Cửa hàng relic ở Góc 3 (mở từ ngày " + s.Balance.start.relicShopFromDay + ").", 20f);
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
            header.text = "Cửa hàng · ví " + s.Money + "G" + (it.IsOpen ? "" : " · mở từ ngày " + b.itemShopFromDay + " (sau lần trả nợ đầu)");

            double[] so = ItemRolls.SauceOdds(b, s.Day, s.Relics.Effects);
            var sauce = new StringBuilder();
            sauce.Append("<b>Nước sốt thần thánh</b> · ").Append(it.SaucePrice).Append("G · có ").Append(it.Sauces).Append('/').Append(b.sauce.max)
                .Append("\n<size=15>Rưới lên món đã nấu (cầm đĩa, bấm lọ sốt ở Góc 2): hỏng ").Append(Pct(so[0])).Append(" · giữ ").Append(Pct(so[1]))
                .Append(" · +1 bậc ").Append(Pct(so[2])).Append(" · +2 bậc ").Append(Pct(so[3]));
            if (so[4] > 0) sauce.Append(" · Huyền thoại ").Append(Pct(so[4]));
            sauce.Append("</size>");
            Row(0, GameController.ItemSauce, sauce.ToString(), it.IsOpen && it.Sauces < b.sauce.max);

            var black = new StringBuilder();
            black.Append("<b>Vé đen</b> · ").Append(it.BlackPrice).Append("G · có ").Append(it.BlackTickets).Append('/').Append(b.blackTicket.max)
                .Append("\n<size=15>Nhét vào khe máy Góc 2 (thùng hàng tốt: ");
            int[] bo = b.BlackOdds().percents;
            bool first = true;
            for (int tier = 0; tier < bo.Length; tier++)
                if (bo[tier] > 0) { black.Append(first ? "" : " · ").Append(UiKit.IngredientTierName[tier]).Append(' ').Append(bo[tier]).Append('%'); first = false; }
            black.Append(") hoặc khe ở Góc 3 (chọn relic Hiếm)</size>");
            Row(1, GameController.ItemBlack, black.ToString(), it.IsOpen && it.BlackTickets < b.blackTicket.max);

            // Dieu 4: may gacha do an giau ti le
            string machine = "<b>Máy gacha đồ ăn</b> · " + (it.HasMachine ? "đã mua · còn " + it.MachineUsesLeft + "/" + b.machine.usesPerDay + " lượt hôm nay" : b.machine.price + "G · mua 1 lần")
                + "\n<size=15>Đặt ở Góc 2. Đổi bậc một món đã nấu (lên hoặc xuống, tỉ lệ ẩn), mỗi món 1 lần.</size>";
            Row(2, GameController.ItemMachine, machine, it.IsOpen && !it.HasMachine);
            note.text = "Đồ mua xong hiện ở Góc 2. Đồng hồ đứng trong lúc mở bảng này.";
        }

        void Row(int index, string item, string text, bool canBuy)
        {
            Image box = UiKit.Panel(shopList, "Hang_" + item, Top, new Vector2(0f, -index * 136f), new Vector2(960f, 126f), UiKit.Hex("#FBF0DA"), false);
            ((RectTransform)box.transform).pivot = Top;
            TextMeshProUGUI t = UiKit.Label(box.transform, "Chu", new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(760f, 118f), text, 20f, TextAlignmentOptions.Left);
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
    }
}
