using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Sprint 8 (specs/sprint-8/spec.md): cong tac den (F-076), doi hang relic (F-077), bang Tab relic + cua hang (F-078),
    /// dao tren thot (F-075), dong ve dau ngay (B-02).
    /// </summary>
    public partial class GameController
    {
        // ---------- F-076: den ----------

        /// <summary>Den tran dang bat (dau moi ngay tat; F-076 AC-1).</summary>
        public bool LightsOn { get; private set; }

        public bool ToggleLights()
        {
            if (!DayRunning) return false;
            if (Session.IsBlackout)
            {
                Sfx("bam", "LightSwitch");
                Say("Mất điện: công tắc không có tác dụng, nấu bằng nến.");
                return false;
            }
            LightsOn = !LightsOn;
            Sfx("bam", "LightSwitch");
            Say(LightsOn ? "Đã bật đèn." : "Đã tắt đèn.");
            return true;
        }

        // ---------- F-077: doi hang ----------

        public bool RerollRelics()
        {
            if (Session.BlackRelicOffers.Count > 0) { Say("Đang chọn relic bằng vé đen: chọn xong rồi mới đổi hàng."); return false; }
            int cost = Session.Shop.RerollCost;
            RerollResult r = Session.RerollShop();
            switch (r)
            {
                case RerollResult.Rerolled:
                    PendingRelicSlot = -1;
                    PendingIsBlack = false;
                    Sfx("tien", "RelicReroll");
                    Say("Đổi hàng (" + cost + "G): 3 relic mới · ví còn " + Session.Money + "G · lần sau " + Session.Shop.RerollCost + "G.");
                    return true;
                case RerollResult.NotEnoughMoney:
                    Say("Thiếu " + (cost - Session.Money) + "G để đổi hàng relic.");
                    return false;
                case RerollResult.NoneLeft:
                    Say("Không còn relic nào khác để bày.");
                    return false;
                default:
                    Say(!Session.Shop.OpensOn(Session.Day) ? "Cửa hàng relic mở từ ngày " + Session.Balance.start.relicShopFromDay + "."
                        : "Giờ này không đổi hàng được.");
                    return false;
            }
        }

        // ---------- F-075: dang giu chuot tren thot ----------

        float lastPrepHold = -10f;
        /// <summary>Vua giu chuot tren thot trong 0,15 giay qua (dao chat theo nhip).</summary>
        public bool PrepHeldRecently { get { return Time.time - lastPrepHold < 0.15f; } }

        void NotePrepHold(int board = 0) { lastPrepHold = Time.time; LastPrepBoard = board; }

        /// <summary>Thot vua duoc giu chuot gan nhat (dao chi chat tren thot do).</summary>
        public int LastPrepBoard { get; private set; }

        // ---------- F-078: bang Tab ----------

        public TabMenu Tab { get; private set; }

        void InitTab()
        {
            Tab = new TabMenu(this);
            Player.TabPressed += OnTabKey;
        }

        void OnTabKey()
        {
            // sprint 10: Tab mo Thuc don (dang cho bo relic thi mo the Relic); lan sau mo lai the vua xem
            if (Mode == GameMode.Playing && !ShowsOverlay)
                OpenTab(PendingRelicSlot >= 0 ? TabMenu.Page.Relic : Tab != null && Tab.Opened ? Tab.Current : TabMenu.Page.Menu);
            else if (Mode == GameMode.Tab) CloseTab();
        }

        /// <summary>Mo bang Tab (F-078 AC-1): dong ho, bep, khach dung nhu menu tam dung.</summary>
        public bool OpenTab(TabMenu.Page page)
        {
            if (Mode != GameMode.Playing || Tab == null) return false;
            Mode = GameMode.Tab;
            PlayerController.SetCursorLocked(false);
            Tab.Show(page);
            TabOpens++;
            Changed();
            return true;
        }

        public int TabOpens { get; private set; }

        /// <summary>So lan xem the Cua hang (huong dan ngay 4, F-081).</summary>
        public int TabShopViews { get; private set; }
        internal void NoteShopView() { TabShopViews++; }

        /// <summary>So lan xem the Thuc don (huong dan ngay 1, sprint 10 B-11).</summary>
        public int TabMenuViews { get; private set; }
        internal void NoteMenuView() { TabMenuViews++; }

        public void CloseTab()
        {
            if (Mode != GameMode.Tab) return;
            Tab.Hide();
            EnterPlaying();
        }

        /// <summary>
        /// Bo relic tu bang Tab (F-078 AC-2). Dang cho mua ma gia day (PendingRelicSlot) thi bo relic nay va lay relic dang cho.
        /// </summary>
        public bool DiscardRelic(string id)
        {
            if (!DayRunning || !Session.Relics.Has(id)) return false;
            if (PendingRelicSlot >= 0) return UseShelfRelic(id);
            RelicDef old = null;
            foreach (RelicDef d in Session.Relics.Owned) if (d.id == id) old = d;
            Session.Relics.Remove(id);
            SfxHand("bam");
            Say("Đã bỏ " + (old != null ? old.name : id) + " · còn " + Session.Relics.Owned.Count + "/" + Session.Relics.Capacity + " relic.");
            return true;
        }

        // ---------- B-02: ve cong don hien ro dau ngay ----------

        /// <summary>Ve con lai cuoi ngay truoc (de bao "+7 ve, cong don x").</summary>
        int ticketsAtDayEnd = -1;

        void NoteDayEndTickets()
        {
            if (Session != null && Session.Phase == GamePhase.DayEnd) ticketsAtDayEnd = Session.Wallet.Count;
        }

        /// <summary>Dong ve dau ngay: "Ngày n: +7 vé (còn x từ hôm qua) → y vé".</summary>
        public string TicketLine()
        {
            if (ticketsAtDayEnd < 0 || Session.Day <= 1) return null;
            int added = Session.Wallet.Count - ticketsAtDayEnd;
            return "+" + added + " vé" + (ticketsAtDayEnd > 0 ? " (cộng " + ticketsAtDayEnd + " vé còn từ hôm qua)" : "") + " → " + Session.Wallet.Count + " vé";
        }

        void ResetSprint8()
        {
            LightsOn = false;
            lastPrepHold = -10f;
            if (Tab != null) Tab.Hide();
        }
    }
}
