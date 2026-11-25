using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Yeu cau 24/11/2026 (constitution 2.2): tay van nang (hai o cam do, phim 1 / 2, mac dinh dung / tha theo thu tu nhat) va may
    /// gacha do an kieu lo vi song (cho dia vao thay dia trong may, gat can de quay, hieu ung thanh cong / xit).
    /// </summary>
    public partial class GameController
    {
        // =====================================================================
        // Hai o cam do
        // =====================================================================

        public class HandSlot
        {
            public HandKind kind;
            public Ingredient ingredient;
            public Dish dish;
            /// <summary>Thu tu nhat (o nhat truoc dung / tha truoc).</summary>
            public int order;
        }

        readonly HandSlot[] slots = { new HandSlot(), new HandSlot() };
        int activeSlot, pickups;
        bool slotChosen;

        /// <summary>O cam do dang dung (0 / 1).</summary>
        public int ActiveSlot { get { return activeSlot; } }
        public HandSlot Slot(int i) { return slots[i]; }

        /// <summary>Co tay van nang (hai o) khong.</summary>
        public bool HasGlove { get { return Session != null && Session.Items.HasGlove; } }

        /// <summary>O kia con trong (chi khi co tay van nang).</summary>
        public bool OtherSlotFree { get { return HasGlove && slots[1 - activeSlot].kind == HandKind.Empty; } }

        void SetSlotKind(HandKind k)
        {
            HandSlot s = slots[activeSlot];
            if (s.kind == HandKind.Empty && k != HandKind.Empty) s.order = ++pickups;
            s.kind = k;
            if (k == HandKind.Empty) { s.ingredient = null; s.dish = null; }
        }

        void ClearSlots()
        {
            foreach (HandSlot s in slots) { s.kind = HandKind.Empty; s.ingredient = null; s.dish = null; }
            activeSlot = 0;
            slotChosen = false;
        }

        /// <summary>Dang cam do o o hien tai: co tay van nang va o kia trong thi chuyen sang o kia de cam them.</summary>
        bool UseOtherSlotIfFree()
        {
            if (!OtherSlotFree) return false;
            activeSlot = 1 - activeSlot;
            return true;
        }

        /// <summary>
        /// Moi khung hinh: khong chon o bang phim thi o dang dung la o nhat truoc (dung / tha theo thu tu); o da chon ma het do thi
        /// tro ve tu dong. Khong co tay van nang thi chi o 0.
        /// </summary>
        void NormalizeSlots()
        {
            int before = activeSlot;
            NormalizeSlotsCore();
            // o dang dung doi (vd vua nhat do vao o kia roi quay ve o nhat truoc): ve lai tay + o do
            if (activeSlot != before) Changed();
        }

        void NormalizeSlotsCore()
        {
            if (!HasGlove)
            {
                if (activeSlot != 0 && slots[0].kind == HandKind.Empty)
                {
                    // ban luu cu / mat tay van nang: don do o 1 ve o 0
                    HandSlot a = slots[0], b = slots[1];
                    a.kind = b.kind; a.ingredient = b.ingredient; a.dish = b.dish; a.order = b.order;
                    b.kind = HandKind.Empty; b.ingredient = null; b.dish = null;
                }
                activeSlot = 0;
                slotChosen = false;
                return;
            }
            if (slotChosen && slots[activeSlot].kind == HandKind.Empty) slotChosen = false;
            if (slotChosen) return;
            int best = -1;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].kind != HandKind.Empty && (best < 0 || slots[i].order < slots[best].order)) best = i;
            activeSlot = best < 0 ? 0 : best;
        }

        /// <summary>Phim 1 / 2: chon o cam do (chi khi co tay van nang).</summary>
        public void SelectSlot(int i)
        {
            if (!HasGlove || i < 0 || i > 1 || Mode != GameMode.Playing) return;
            activeSlot = i;
            slotChosen = true;
            HandSlot s = slots[i];
            Say("Ô " + (i + 1) + ": " + (s.kind == HandKind.Empty ? "trống" : HeldName) + ".");
            Changed();
        }

        /// <summary>Dia dang tren tay (o nao cung duoc) hoac trong may: quay / tu lanh khong ve dia nay.</summary>
        public bool DishTaken(Dish d)
        {
            return d != null && (ReferenceEquals(slots[0].dish, d) || ReferenceEquals(slots[1].dish, d) || ReferenceEquals(MachineDish, d));
        }

        /// <summary>Khoa thay doi cua dia tren tay / trong may (ve lai tu lanh khi doi).</summary>
        public string TakenKey
        {
            get
            {
                return (slots[0].dish != null ? slots[0].dish.GetHashCode() : 0) + ":" + (slots[1].dish != null ? slots[1].dish.GetHashCode() : 0)
                    + ":" + (MachineDish != null ? MachineDish.GetHashCode() : 0);
            }
        }

        /// <summary>Ten do trong o <paramref name="i"/> (HUD hotbar).</summary>
        public string SlotName(int i)
        {
            HandSlot s = slots[i];
            switch (s.kind)
            {
                case HandKind.Ingredient: return s.ingredient.name + (s.ingredient.prepared ? " (sơ chế)" : "");
                case HandKind.Plate: return "đĩa trống";
                case HandKind.Dish: return s.dish.Title;
                case HandKind.Sauce: return "nước sốt";
                default: return null;
            }
        }

        // =====================================================================
        // May gacha do an kieu lo vi song
        // =====================================================================

        /// <summary>Dia dang nam trong may (null neu trong).</summary>
        public Dish MachineDish { get; private set; }

        /// <summary>Con bao nhieu giay quay (0 = khong quay).</summary>
        public float MachineSpinLeft { get { return machineSpin; } }
        public bool MachineSpinning { get { return machineSpin > 0f; } }

        /// <summary>Ket qua lan quay gan nhat (hien sau khi quay xong).</summary>
        public MachineResult? MachineLastResult { get; private set; }

        /// <summary>Bac dia trong may dang hien (truoc khi quay xong van la bac cu).</summary>
        public int MachineShownTier { get; private set; }

        public const float MachineSpinSeconds = 1.8f;

        float machineSpin;
        MachineResult pendingResult;
        int pendingBefore;
        Dish pendingDish;

        /// <summary>Bam than may: chua mua thi mua; cam dia thi cho vao; may co dia (khong quay) thi lay ra.</summary>
        public bool UseMachineBody()
        {
            if (!DayRunning) return false;
            if (!Session.Items.HasMachine) return BuyItem(ItemMachine);
            if (MachineSpinning) { Say("Máy đang quay…"); return false; }
            if (MachineDish != null)
            {
                if (Hand == HandKind.Dish && !OtherSlotFree) { Say("Tay đang cầm đĩa. Lấy " + MachineDish.name + " ra sau."); return false; }
                if (!HandsFree("lấy đĩa ra khỏi máy")) return false;
                Dish d = MachineDish;
                MachineDish = null;
                Hand = HandKind.Dish;
                HeldDish = d;
                SfxHand("cam");
                Say("Lấy " + d.name + " · " + UiKit.DishTierName[d.tier] + " ra khỏi máy.");
                Changed();
                return true;
            }
            if (Hand != HandKind.Dish) { Say("Máy gacha đồ ăn: cầm một đĩa món tới, bấm máy để cho vào. Hôm nay còn " + Session.Items.MachineUsesLeft + " lượt."); return false; }
            Dish dish = HeldDish;
            if (dish.machined) { Say(dish.name + " đã vào máy một lần rồi."); return false; }
            if (Session.Items.MachineUsesLeft <= 0) { Say("Hôm nay máy đã chạy đủ " + Session.Balance.machine.usesPerDay + " lần."); return false; }
            MachineDish = dish;
            MachineShownTier = dish.tier;
            MachineLastResult = null;
            Hand = HandKind.Empty;
            HeldDish = null;
            Sfx("dat", "FoodMachine");
            Say("Đã cho " + dish.name + " vào máy. Gạt cần bên cạnh để quay.");
            Changed();
            return true;
        }

        /// <summary>Gat can: quay dia trong may (ket qua lo ra sau <see cref="MachineSpinSeconds"/> giay).</summary>
        public bool PullMachineLever()
        {
            if (!DayRunning) return false;
            if (!Session.Items.HasMachine) { Say("Chưa có máy gacha đồ ăn. Mua trong Tab → Cửa hàng."); return false; }
            if (MachineSpinning) { Say("Máy đang quay…"); return false; }
            if (MachineDish == null) { Say("Cho một đĩa món vào máy trước (cầm đĩa, bấm máy)."); return false; }
            if (MachineDish.machined) { Say(MachineDish.name + " đã quay rồi. Bấm máy để lấy ra."); return false; }
            Dish d = MachineDish;
            int before = d.tier;
            MachineResult r;
            if (!Session.UseMachine(d, out r)) { Say("Hôm nay máy đã chạy đủ lượt."); return false; }
            pendingResult = r;
            pendingBefore = before;
            pendingDish = d;
            machineSpin = MachineSpinSeconds;
            MachineShownTier = before;
            Sfx("bam", "FoodMachineLever");
            Say("Máy đang quay…");
            Changed();
            return true;
        }

        void TickMachine(float dt)
        {
            if (machineSpin <= 0f) return;
            if (Mode != GameMode.Playing) return;
            machineSpin -= dt;
            if (machineSpin > 0f) return;
            machineSpin = 0f;
            RevealMachine();
        }

        void RevealMachine()
        {
            Dish d = pendingDish;
            MachineResult r = pendingResult;
            MachineLastResult = r;
            LastItemEffect = "machine:" + r;
            bool trashed = r == MachineResult.Trash;
            bool up = !trashed && d.tier > pendingBefore;
            bool down = trashed || d.tier < pendingBefore;
            Vector3 at = Room != null ? Room.MachineDishPosition : HandPosition;
            if (up)
            {
                if (d.tier >= Dish.LegendTier || r == MachineResult.Up3) { SauceFx.Rainbow(at); if (Audio != null) Audio.Play("cau_vong", at, 1f, 0f); LastFx = "cau_vong"; }
                else { RiseFx.Play(Room != null ? Room.MachineRoot : null, Room != null ? Room.MachineRoot.InverseTransformPoint(at) : at, "xoay_vang", 6, 1.1f, 0.25f, 0.08f, 0.16f, 360f); Sfx("bac_4", "FoodMachine"); LastFx = "xoay_vang"; }
            }
            else if (down)
            {
                SauceFx.Bomb(at);
                if (Audio != null) Audio.Play("bom_xit", at, 1f, 0f);
                Player.Shake(0.15f);
                LastFx = "bom_xit";
            }
            else { Sfx("sot_hong", "FoodMachine", 0.6f); LastFx = "giu_nguyen"; }
            MachineShownTier = trashed ? 0 : d.tier;
            if (trashed)
            {
                MachineDish = null;
                Say("Xịt! Máy kêu rắc rắc… " + d.name + " hỏng mất rồi.");
            }
            else if (up) Say("Thành công! " + d.name + " " + UiKit.DishTierName[pendingBefore] + " → " + UiKit.DishTierName[d.tier] + ". Bấm máy để lấy ra.");
            else if (down) Say("Xịt! " + d.name + " " + UiKit.DishTierName[pendingBefore] + " → " + UiKit.DishTierName[d.tier] + ". Bấm máy để lấy ra.");
            else Say("Giữ nguyên " + UiKit.DishTierName[d.tier] + ". Bấm máy để lấy ra.");
            pendingDish = null;
            Changed();
        }
    }
}
