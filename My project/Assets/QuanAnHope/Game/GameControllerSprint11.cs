using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Sau sprint 10 (yeu cau 23/11/2026, constitution 2.1): bep thu hai, lo nuoc sot canh bep (cam len - nho len dia vua nau),
    /// hieu ung cau vong / bom xit, giong khach dac biet khi vao quan, phim T cho tranh huong dan toan man hinh.
    /// </summary>
    public partial class GameController
    {
        void InitSprint11()
        {
            Player.TutorialKeyPressed += ToggleTutorialPics;
            // bug 23/11/2026: tay trong thi nhin nguyen lieu da tha (ke ca tren thot) la nhat; dang cam do thi van dat len thot / ban
            Player.PreferDropped = () => Hand == HandKind.Empty || OtherSlotFree;
            Player.SlotKeyPressed += SelectSlot;
        }

        // =====================================================================
        // Bep (constitution 2.1 muc 5.1, 8): bep 0 co san, bep 1 mua o Tab -> Cua hang
        // =====================================================================

        int activeStove;

        /// <summary>Bep vua dung (giá dung cu, lo gia vi, thung rac ap vao bep nay neu hop le).</summary>
        public int ActiveStove
        {
            get { return Session != null ? Mathf.Clamp(activeStove, 0, Session.Stoves.Count - 1) : 0; }
            private set { activeStove = value; }
        }

        /// <summary>Ten tram cua bep / num bep thu i: "Stove", "Stove_1" / "Knob", "Knob_1".</summary>
        public static string StoveId(int i) { return i == 0 ? "Stove" : "Stove_" + i; }
        public static string KnobId(int i) { return i == 0 ? "Knob" : "Knob_" + i; }

        /// <summary>Loi nhan cua mot bep: co hai bep thi ghi "Bep 2: ...".</summary>
        string StoveSay(int si, string text)
        {
            return Session.Stoves.Count > 1 ? "Bếp " + (si + 1) + ": " + text : text;
        }

        static bool Editable(Stove s) { return s.State == StoveState.Empty || s.State == StoveState.Filling; }

        /// <summary>Chon dung cu: bep dang dung neu con doi duoc, khong thi bep trong dau tien.</summary>
        int StoveForTool()
        {
            int a = ActiveStove;
            if (Editable(Session.Stoves[a])) return a;
            for (int i = 0; i < Session.Stoves.Count; i++) if (Editable(Session.Stoves[i])) return i;
            return a;
        }

        /// <summary>Cho gia vi: bep dang dung neu da co dung cu va chua nau, khong thi bep dau tien nhu vay.</summary>
        int StoveForPantry()
        {
            int a = ActiveStove;
            Stove s = Session.Stoves[a];
            if (Editable(s) && s.Tool != null) return a;
            for (int i = 0; i < Session.Stoves.Count; i++)
                if (Editable(Session.Stoves[i]) && Session.Stoves[i].Tool != null) return i;
            return a;
        }

        int StoveToDiscard()
        {
            int a = ActiveStove;
            Stove s = Session.Stoves[a];
            if (s.State != StoveState.Empty || s.Tool != null) return a;
            for (int i = 0; i < Session.Stoves.Count; i++)
                if (Session.Stoves[i].State != StoveState.Empty || Session.Stoves[i].Tool != null) return i;
            return a;
        }

        // =====================================================================
        // Nuoc sot than thanh (bug 23/11/2026): lo hien ben phai bep khi da mua; bam de cam lo, bam dia vua nau de nho sot
        // =====================================================================

        /// <summary>Bam lo nuoc sot canh bep.</summary>
        public bool PickSauce()
        {
            if (!DayRunning) return false;
            if (Hand == HandKind.Sauce)
            {
                Hand = HandKind.Empty;
                SfxHand("dat");
                Say("Đã đặt lại nước sốt.");
                return true;
            }
            if (Session.Items.Sauces <= 0) { Say(Session.Items.IsOpen ? "Hết nước sốt. Mua trong Tab → Cửa hàng." : "Cửa hàng mở từ ngày " + Session.Balance.itemShopFromDay + "."); return false; }
            if (Hand == HandKind.Dish) return UseSauceOnHeld();
            if (!HandsFree("cầm nước sốt")) return false;
            Hand = HandKind.Sauce;
            SfxHand("cam");
            Say("Cầm nước sốt (còn " + Session.Items.Sauces + " chai). Bấm vào đĩa vừa nấu để nhỏ sốt.");
            return true;
        }

        /// <summary>Dang cam lo sot, bam mot dia tren quay / trong tu: nho mot chai len dia (constitution muc 5.5).</summary>
        public bool DripSauce(Dish dish)
        {
            if (!DayRunning || Hand != HandKind.Sauce) return false;
            if (dish == null || !Session.HasDish(dish)) { Say("Nhỏ sốt lên đĩa trên quầy hoặc trong tủ lạnh."); return false; }
            int before = dish.tier;
            SauceResult r;
            if (!Session.UseSauce(dish, out r)) { Say("Hết nước sốt."); return false; }
            Hand = HandKind.Empty;
            Vector3 at = Room != null ? Room.DishPosition(dish) : HandPosition;
            ShowSauceResult(dish, before, r, at);
            return true;
        }

        /// <summary>Ket qua mot lan nho sot: tieng, hieu ung (Huyen thoai = cau vong bung no, Lac vi = bom xit), loi nhan.</summary>
        void ShowSauceResult(Dish d, int before, SauceResult r, Vector3 at)
        {
            LastItemEffect = "sauce:" + r;
            bool ruined = d.tier == 0 && before > 0;
            if (d.IsLegend && before < Dish.LegendTier)
            {
                SauceFx.Rainbow(at);
                if (Audio != null) Audio.Play("cau_vong", at, 1f, 0f);
                LastFx = "cau_vong";
            }
            else if (ruined || r == SauceResult.Fail)
            {
                SauceFx.Bomb(at);
                if (Audio != null) Audio.Play("bom_xit", at, 1f, 0f);
                Player.Shake(0.15f);
                LastFx = "bom_xit";
            }
            else
            {
                SfxHand("sot_tot");
                FxSauce(false);
            }
            string left = " · còn " + Session.Items.Sauces + " chai.";
            if (d.IsLegend && before < Dish.LegendTier) Say("HUYỀN THOẠI! " + d.name + " (giá ×" + Session.Balance.dish.priceMult[d.tier].ToString("0.0") + ")" + left);
            else Say("Nước sốt: " + d.name + " " + UiKit.DishTierName[before] + " → " + UiKit.DishTierName[d.tier] + left);
        }

        /// <summary>Mon nau ra bac Huyen thoai (constitution 2.1 muc 5.4): cung hieu ung cau vong.</summary>
        void OnCookedLegend(Dish dish)
        {
            Vector3 at = Room != null ? Room.DishPosition(dish) : HandPosition;
            SauceFx.Rainbow(at);
            if (Audio != null) Audio.Play("cau_vong", at, 1f, 0f);
            LastFx = "cau_vong";
        }

        // =====================================================================
        // Khach dac biet: giong khi vao quan
        // =====================================================================

        /// <summary>Chi phat mot doan dau cua giong (clip goc 5-42 giay, Q-82).</summary>
        public const float VoiceSeconds = 7f;

        /// <summary>Giong khach dac biet gan nhat (test).</summary>
        public string LastVoice { get; private set; }

        public void OnCustomerArrived(Customer c)
        {
            int look = CustomerLooks.LookFor(c.number, Session.Day);
            string voice = CustomerLooks.VoiceOf(look);
            if (voice == null) return;
            LastVoice = voice;
            if (Audio != null) Audio.PlayVoice(voice, VoiceSeconds);
        }

        // =====================================================================
        // Tranh huong dan toan man hinh: T bat / tat
        // =====================================================================

        public void ToggleTutorialPics()
        {
            if (Hud != null && Mode == GameMode.Playing) Hud.ToggleTutorialBig();
        }
    }
}
