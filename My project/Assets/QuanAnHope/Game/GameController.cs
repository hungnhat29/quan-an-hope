using System;
using BepNhanPham;
using QuanAnHope.Core;
using QuanAnHope.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuanAnHope.Game
{
    public enum HandKind
    {
        Empty,
        Ingredient,
        Plate,
        Dish,
    }

    /// <summary>
    /// Noi luat choi (Core.GameSession) voi can phong 3D. Moi thao tac cua nguoi choi la mot ham public o day,
    /// do vat trong phong (RoomView) goi vao khi duoc bam. Tu gan vao canh co doi tuong "NhaHang".
    /// </summary>
    public class GameController : MonoBehaviour
    {
        const string BalanceResource = "BalanceData";
        const string RoomRootName = "NhaHang";
        const float ReactionSeconds = 0.8f; // Art Bible muc 11: khach vui / tip 0,8 giay

        public GameSession Session { get; private set; }
        public PlayerController Player { get; private set; }
        public RoomView Room { get; private set; }
        public Hud Hud { get; private set; }

        // ---- tay nguoi choi (constitution Dieu 5: cam toi da mot thu) ----
        public HandKind Hand { get; private set; }
        public Ingredient HeldIngredient { get; private set; }
        public Dish HeldDish { get; private set; }

        /// <summary>Nguyen lieu dang nam tren thot (ke ca da so che xong ma chua cam len).</summary>
        public Ingredient OnBoard { get; private set; }

        public PullResult LastPull { get; private set; }
        public Dish LastServed { get; private set; }
        public PaymentResult LastServedPayment { get; private set; }
        public float ServedShowLeft { get; private set; }
        public string Toast { get; private set; }

        /// <summary>Be relic dang cho chon relic de bo (gia day 8/8); -1 neu khong (F-020 AC-5).</summary>
        public int PendingRelicSlot { get; private set; }

        /// <summary>Relic vua mua (de hien lap lanh tren gia, Art Bible muc 11).</summary>
        public RelicDef LastBoughtRelic { get; private set; }

        BalanceData data;
        FaceSwap face;
        GameObject customerModel;
        float reactionLeft;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (FindAnyObjectByType<GameController>() != null) return;
            if (GameObject.Find(RoomRootName) == null) return;
            new GameObject("QuanAnHope_Game").AddComponent<GameController>();
        }

        void Awake()
        {
            // Ban build luon mo toan man hinh o do phan giai goc, ke ca khi lan chay truoc da luu che do cua so
            // (Unity nho che do man hinh trong registry). Editor va batchmode giu nguyen.
            if (!Application.isEditor && !Application.isBatchMode)
            {
                Resolution native = Screen.currentResolution;
                Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
            }

            data = Resources.Load<BalanceData>(BalanceResource);
            if (data == null)
            {
                Debug.LogError("Quán Ăn Hope: thiếu Resources/BalanceData. Chạy menu Quán Ăn Hope > Nhập balance.json.");
                enabled = false;
                return;
            }

            face = FindAnyObjectByType<FaceSwap>();
            if (face != null) customerModel = face.gameObject;

            GameObject playerGo = GameObject.Find("Player");
            if (playerGo == null) playerGo = new GameObject("Player");
            Player = PlayerController.Attach(playerGo);
            Player.SecondaryPressed += PutDown;
            FaceBillboard.View = Player.Cam;

            NewGame(Environment.TickCount);
            Room = new RoomView(this, GameObject.Find(RoomRootName).transform);
            Hud = new Hud(this);
            Changed();
        }

        /// <summary>Bat dau van moi. Phan choi duoc cua ban nay la vong dau tien trong du lieu.</summary>
        /// <summary>F-014: ghi thoi gian moi mon vao playtest-log.csv (Application.persistentDataPath).</summary>
        public PlaytestLog Log { get; private set; }

        public void NewGame(int seed)
        {
            if (Log == null) Log = new PlaytestLog(Application.persistentDataPath);
            int lastDay = data.balance.rounds[0].lastDay;
            Session = new GameSession(data.balance, data.recipes, new SeededRandom(seed), lastDay);
            DishFood.Book = data.recipes;
            ClearHands();
            LastPull = null;
            LastServed = null;
            ServedShowLeft = 0f;
            Toast = null;
            reactionLeft = 0f;
            PendingRelicSlot = -1;
            LastBoughtRelic = null;
            if (Player != null) Player.Teleport(Vector3.zero, 0f);
            SyncCustomerModel();
        }

        void ClearHands()
        {
            Hand = HandKind.Empty;
            HeldIngredient = null;
            HeldDish = null;
            OnBoard = null;
        }

        // F-011: theo doi thay doi do dong ho gay ra (khach moi vao, 21:00, het ngay).
        Customer seenCustomer;
        GamePhase seenPhase;
        bool seenClosed;

        /// <summary>Tua nhanh dong ho ngay (chi cho test PlayMode, constitution 1.4 muc 2: 1 gio = 40 giay).</summary>
        public float ClockScale = 1f;

        void Update()
        {
            if (Session == null) return;
            float dt = Time.deltaTime;

            // Dong ho ngay chay tu dau ngay ke ca khi quan chua mo (constitution 1.4 muc 2, 6.2).
            Session.Tick(dt * ClockScale);
            if (Session.Phase != seenPhase || Session.CurrentCustomer != seenCustomer || Session.IsClosedForNewCustomers != seenClosed)
                OnClockEvent();

            if (Session.Stove.State == StoveState.Cooking)
            {
                Session.Stove.Tick(dt);
                if (Session.Stove.State == StoveState.Ready)
                {
                    Log.OnReady(Time.time);
                    Say("Món đã chín. Lấy đĩa trống rồi bấm vào bếp để múc.");
                }
            }

            if (reactionLeft > 0f)
            {
                reactionLeft -= dt;
                if (reactionLeft <= 0f) SyncCustomerModel();
            }
            if (ServedShowLeft > 0f)
            {
                ServedShowLeft -= dt;
                if (ServedShowLeft <= 0f) Changed();
            }

            Player.InputEnabled = !ShowsOverlay;
            if (Room != null) Room.Tick(dt);
            if (Hud != null) Hud.Tick();
        }

        /// <summary>Dang hien man ket thuc ngay / qua vong / thua (man ngoai phong, Dieu 5).</summary>
        public bool ShowsOverlay
        {
            get
            {
                return Session.Phase == GamePhase.DayEnd || Session.Phase == GamePhase.Cleared || Session.Phase == GamePhase.Lost;
            }
        }

        void Changed()
        {
            // Moi thay doi do nguoi choi gay ra da duoc hien; Update chi bat thay doi do dong ho.
            seenPhase = Session.Phase;
            seenCustomer = Session.CurrentCustomer;
            seenClosed = Session.IsClosedForNewCustomers;
            if (Room != null) Room.Refresh();
            if (Hud != null) Hud.Refresh();
        }

        void Say(string message)
        {
            Toast = message;
            Changed();
        }

        bool DayRunning { get { return Session.Phase == GamePhase.Prep || Session.Phase == GamePhase.Open; } }

        public string HeldName
        {
            get
            {
                switch (Hand)
                {
                    case HandKind.Ingredient: return HeldIngredient.name + (HeldIngredient.prepared ? " đã sơ chế" : "");
                    case HandKind.Plate: return "đĩa trống";
                    case HandKind.Dish: return HeldDish.name;
                    default: return null;
                }
            }
        }

        bool HandsFree(string action)
        {
            if (Hand == HandKind.Empty) return true;
            Say("Tay đang cầm " + HeldName + ". Chuột phải để đặt lại rồi " + action + ".");
            return false;
        }

        // =====================================================================
        // Goc 2: may nhet ve, thung nguyen lieu
        // =====================================================================

        public bool Pull()
        {
            PullResult pull;
            if (!Session.PullCrates(out pull))
            {
                Say(Session.Wallet.Count <= 0 ? "Hết vé rồi." : "Giờ này không nhận thùng được.");
                return false;
            }
            LastPull = pull;
            Say("Thùng " + pull.side.name + " và thùng " + pull.main.name + " đã rơi xuống bàn."
                + (pull.breakthrough != null ? " Trúng rương đột phá: " + pull.breakthrough.name + "!" : ""));
            return true;
        }

        // =====================================================================
        // Goc 3: cua hang relic (constitution muc 7.1)
        // =====================================================================

        /// <summary>Bam relic tren be: mua bang ve trang. Gia day thi chuyen sang chon relic de bo; bam lai be de huy.</summary>
        public bool BuyRelic(int slot)
        {
            if (PendingRelicSlot == slot)
            {
                PendingRelicSlot = -1;
                Say("Đã huỷ, không mua relic.");
                return false;
            }
            PendingRelicSlot = -1;
            RelicDef offer = slot >= 0 && slot < Session.Shop.Offers.Count ? Session.Shop.Offers[slot] : null;
            RelicBuyResult r = Session.BuyRelic(slot);
            switch (r)
            {
                case RelicBuyResult.Bought:
                    LastBoughtRelic = offer;
                    Say("Đã mua " + offer.name + " · còn " + Session.Wallet.Count + " vé.");
                    return true;
                case RelicBuyResult.NeedDiscard:
                    PendingRelicSlot = slot;
                    Say("Giá relic đầy " + Session.Relics.Capacity + "/" + Session.Relics.Capacity
                        + ": bấm một relic trên giá để bỏ, hoặc bấm lại bệ / chuột phải để huỷ.");
                    return false;
                case RelicBuyResult.NotEnoughTickets:
                    Say("Thiếu " + Session.Shop.Missing(slot, Session.Wallet) + " vé để mua " + offer.name + ".");
                    return false;
                case RelicBuyResult.Closed:
                    Say(Session.Shop.OpensOn(Session.Day) ? "Cửa hàng đã đóng." : "Cửa hàng relic mở từ ngày " + Session.Balance.start.relicShopFromDay + ".");
                    return false;
                default:
                    Say("Bệ này đã bán hôm nay.");
                    return false;
            }
        }

        /// <summary>Bam relic tren gia: khi dang chon de bo thi bo no va mua relic dang cho.</summary>
        public bool UseShelfRelic(string id)
        {
            if (PendingRelicSlot < 0) return false;
            int slot = PendingRelicSlot;
            PendingRelicSlot = -1;
            RelicDef offer = Session.Shop.Offers[slot];
            RelicDef old = null;
            foreach (RelicDef d in Session.Relics.Owned) if (d.id == id) old = d;
            if (Session.BuyRelic(slot, id) != RelicBuyResult.Bought)
            {
                Say("Không đổi được relic.");
                return false;
            }
            LastBoughtRelic = offer;
            Say("Đã bỏ " + (old != null ? old.name : id) + ", mua " + offer.name + " · còn " + Session.Wallet.Count + " vé.");
            return true;
        }

        /// <summary>Lenh go loi "cho toi relic X" (impl plan sprint 3 muc 5): chi Editor goi.</summary>
        public bool DebugGiveRelic(string id)
        {
            if (!Application.isEditor || !Session.DebugGiveRelic(id)) return false;
            foreach (RelicDef d in Session.Relics.Owned) if (d.id == id) LastBoughtRelic = d;
            Say("[Gỡ lỗi] Đã thêm relic " + id + ".");
            return true;
        }

        /// <summary>Cam mot mon tu thung.</summary>
        public bool TakeIngredient(Ingredient ingredient)
        {
            if (!DayRunning || ingredient == null || !HandsFree("lấy " + ingredient.name)) return false;
            if (!InStock(ingredient) || ReferenceEquals(ingredient, OnBoard)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = ingredient;
            Log.OnTakeIngredient(Time.time);
            Say("Đang cầm " + HeldName + ".");
            return true;
        }

        /// <summary>Chuot phai: dat lai thu dang cam ve cho cu (thung, chong dia, quay cho).</summary>
        public void PutDown()
        {
            if (PendingRelicSlot >= 0)
            {
                PendingRelicSlot = -1;
                Say("Đã huỷ, không mua relic.");
                return;
            }
            if (Hand == HandKind.Empty) return;
            string name = HeldName;
            Hand = HandKind.Empty;
            HeldIngredient = null;
            HeldDish = null;
            Say("Đã đặt lại " + name + ".");
        }

        public bool InStock(Ingredient ingredient)
        {
            foreach (Ingredient i in Session.Stock.Items)
                if (ReferenceEquals(i, ingredient)) return true;
            return false;
        }

        // =====================================================================
        // Goc 4: thot
        // =====================================================================

        /// <summary>Bam vao thot: dat nguyen lieu len, hoac cam nguyen lieu da so che len.</summary>
        public bool UseBoard()
        {
            if (!DayRunning) return false;

            if (OnBoard == null)
            {
                if (Hand != HandKind.Ingredient) { Say("Cầm một nguyên liệu từ thùng ở Góc 2 tới đây."); return false; }
                if (HeldIngredient.prepared) { Say(HeldIngredient.name + " đã sơ chế rồi, cho thẳng vào bếp."); return false; }
                if (!Session.BeginPrep(HeldIngredient)) return false;
                OnBoard = HeldIngredient;
                Hand = HandKind.Empty;
                HeldIngredient = null;
                Say(OnBoard.name + " đã lên thớt. Giữ chuột trái để sơ chế.");
                return true;
            }

            if (!OnBoard.prepared) { Say("Giữ chuột trái để sơ chế " + OnBoard.name + "."); return false; }
            if (!HandsFree("cầm " + OnBoard.name)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = OnBoard;
            OnBoard = null;
            Say("Đang cầm " + HeldName + ". Mang tới bếp.");
            return true;
        }

        /// <summary>Giu chuot tren thot.</summary>
        public void HoldBoard(float seconds)
        {
            if (!DayRunning || OnBoard == null || OnBoard.prepared) return;
            if (Session.HoldPrep(seconds)) Say("Đã sơ chế xong " + OnBoard.name + ". Bấm thớt để cầm lên.");
        }

        // =====================================================================
        // Goc 4: bep
        // =====================================================================

        public bool SelectTool(string tool)
        {
            if (!DayRunning) return false;
            if (!Session.Stove.SelectTool(tool)) { Say("Bếp đang nấu, chưa đổi dụng cụ được."); return false; }
            Say("Đặt " + tool.ToLowerInvariant() + " lên bếp.");
            return true;
        }

        public bool AddPantry(string item)
        {
            if (!DayRunning) return false;
            if (Session.Stove.Tool == null) { Say("Chọn dụng cụ trên giá trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
            if (!Session.Stove.AddPantry(item)) { Say("Bếp đang nấu, chưa cho thêm được."); return false; }
            if (Room != null) Room.OnAddedToStove(null, item);
            Say("Đã cho " + item + ".");
            return true;
        }

        /// <summary>Bam vao bep: cho nguyen lieu dang cam vao, hoac muc mon ra dia trong dang cam.</summary>
        public bool UseStove()
        {
            if (!DayRunning) return false;
            Stove stove = Session.Stove;

            if (Hand == HandKind.Ingredient)
            {
                if (!HeldIngredient.prepared) { Say("Sơ chế " + HeldIngredient.name + " trên thớt trước."); return false; }
                if (stove.Tool == null) { Say("Chọn dụng cụ trên giá trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
                string name = HeldIngredient.name;
                if (!Session.AddToStove(HeldIngredient)) { Say("Bếp không nhận thêm " + name + " lúc này."); return false; }
                if (Room != null) Room.OnAddedToStove(HeldIngredient, null);
                Hand = HandKind.Empty;
                HeldIngredient = null;
                Say("Đã cho " + name + " vào " + stove.Tool.ToLowerInvariant() + ".");
                return true;
            }

            if (stove.State == StoveState.Ready)
            {
                if (Hand != HandKind.Plate) { Say("Lấy một đĩa trống ở chồng đĩa rồi bấm vào bếp để múc."); return false; }
                Dish dish;
                if (!Session.PlateDish(out dish))
                {
                    Say(Session.Counter.IsFull ? "Quầy chờ đầy (" + Session.Counter.Capacity + " đĩa). Phục vụ bớt rồi múc." : "Chưa múc được.");
                    return false;
                }
                Hand = HandKind.Empty;
                Log.OnPlated(dish);
                if (dish.tier == 0) Player.Shake(0.15f); // Lac vi: rung camera 0,15 giay (Art Bible muc 11)
                Say(dish.name + ": " + UiKit.DishTierName[dish.tier] + ". Đĩa đã lên quầy chờ.");
                return true;
            }

            if (stove.State == StoveState.Cooking) { Say("Đang nấu, chờ chín đã."); return false; }
            Say(stove.Tool == null ? "Chọn dụng cụ trên giá trước." : "Cầm nguyên liệu đã sơ chế tới, hoặc bấm lọ gia vị.");
            return false;
        }

        /// <summary>Vang num bep: bat dau nau.</summary>
        public bool StartCooking()
        {
            if (!DayRunning) return false;
            if (!Session.Stove.Start()) { Say("Cần 1 nguyên liệu chính và 1 đồ phụ đã sơ chế trong bếp."); return false; }
            Log.OnKnob(Time.time, Session.Stove.Recipe, Session.Recipes, Session.Stove);
            Say("Đang nấu " + Session.Stove.Recipe.name + "…");
            return true;
        }

        public bool TakePlate()
        {
            if (!DayRunning || !HandsFree("lấy đĩa")) return false;
            Hand = HandKind.Plate;
            Say("Đang cầm đĩa trống.");
            return true;
        }

        public void DiscardStove()
        {
            if (!DayRunning) return;
            if (Session.Stove.State == StoveState.Empty && Session.Stove.Tool == null) { Say("Bếp đang trống."); return; }
            Session.Stove.Reset();
            Log.OnDiscard();
            Say("Đã đổ bỏ đồ trên bếp.");
        }

        /// <summary>Cam mot dia tu quay cho.</summary>
        public bool TakeDish(Dish dish)
        {
            if (!DayRunning || dish == null || !HandsFree("lấy đĩa")) return false;
            if (IndexOnCounter(dish) < 0) return false;
            Hand = HandKind.Dish;
            HeldDish = dish;
            Say("Đang cầm " + dish.name + ". Mang ra quầy Góc 1, bấm chuông.");
            return true;
        }

        int IndexOnCounter(Dish dish)
        {
            for (int i = 0; i < Session.Counter.Dishes.Count; i++)
                if (ReferenceEquals(Session.Counter.Dishes[i], dish)) return i;
            return -1;
        }

        // =====================================================================
        // Goc 1: bien mo quan, chuong
        // =====================================================================

        /// <summary>Bien Mo quan: dau ngay thi mo quan, dang mo thi dong quan som.</summary>
        public bool ToggleShop()
        {
            if (Session.Phase == GamePhase.Prep)
            {
                if (!Session.OpenShop()) return false;
                Toast = "Mở quán! Khách đầu tiên đã tới.";
                SyncCustomerModel();
                return true;
            }
            if (Session.Phase == GamePhase.Open)
            {
                if (!Session.CloseEarly()) return false;
                ClearHands();
                Toast = "Đã đóng quán. Khách còn lại ra về.";
                SyncCustomerModel();
                return true;
            }
            return false;
        }

        public bool RingBell()
        {
            if (Session.Phase != GamePhase.Open || Session.CurrentCustomer == null) { Say("Chưa có khách. Bấm biển Mở quán trước."); return false; }
            if (Hand != HandKind.Dish) { Say("Cầm một đĩa từ quầy chờ tới đây rồi bấm chuông."); return false; }

            Dish dish = HeldDish;
            int index = IndexOnCounter(dish);
            if (index < 0) return false;
            string ordered = Session.CurrentCustomer.orderName;
            PaymentResult paid = Session.Serve(index);
            if (paid == null) return false;
            Log.OnServed(Time.time, Session, dish);

            Hand = HandKind.Empty;
            HeldDish = null;
            LastServed = dish;
            LastServedPayment = paid;
            ServedShowLeft = ReactionSeconds * 2f;
            if (Session.Phase != GamePhase.Open) ClearHands();

            if (paid.wrongOrder)
                Toast = "Sai món: khách gọi " + ordered + ". Khách giận, quán đền " + (-paid.amount) + "G.";
            else if (paid.outcome == PaymentOutcome.Satisfied)
                Toast = "Khách vui: +" + paid.amount + "G (món " + paid.price + "G, tip " + paid.tip + "G).";
            else if (paid.outcome == PaymentOutcome.Disappointed)
                Toast = "Món thấp hơn 1 bậc: khách chỉ trả " + paid.amount + "G, không tip.";
            else
                Toast = "Khách giận: quán đền " + (-paid.amount) + "G.";

            if (face != null && Session.Phase == GamePhase.Open)
            {
                if (paid.outcome == PaymentOutcome.Angry) face.ShowAngry();
                else if (paid.outcome == PaymentOutcome.Satisfied) face.ShowHappy();
                else face.ShowWaiting();
                if (customerModel != null) customerModel.SetActive(true);
                reactionLeft = ReactionSeconds;
                Changed();
            }
            else SyncCustomerModel();
            return true;
        }

        // =====================================================================
        // sang ngay, choi lai
        // =====================================================================

        /// <summary>Dong ho vua lam doi trang thai: khach moi vao, 21:00 dong bien, hoac het ngay.</summary>
        void OnClockEvent()
        {
            bool newCustomer = Session.CurrentCustomer != null && Session.CurrentCustomer != seenCustomer;
            bool justClosed = Session.IsClosedForNewCustomers && !seenClosed;
            bool dayEnded = seenPhase != Session.Phase && !DayRunning;
            seenPhase = Session.Phase;
            seenCustomer = Session.CurrentCustomer;
            seenClosed = Session.IsClosedForNewCustomers;

            if (dayEnded) ClearHands();
            if (justClosed && Session.Phase == GamePhase.Open) Toast = "21:00 · Quán đóng cửa, phục vụ nốt khách đang ngồi.";
            else if (newCustomer && reactionLeft <= 0f) Toast = "Khách mới vào: gọi " + Session.CurrentCustomer.orderName + ".";
            // Dang hien mat vui / gian cua khach vua phuc vu thi doi het roi moi doi hinh khach.
            if (reactionLeft <= 0f) SyncCustomerModel();
            else Changed();
        }

        public bool NextDay()
        {
            if (!Session.StartNextDay()) return false;
            ClearHands();
            PendingRelicSlot = -1;
            LastBoughtRelic = null;
            LastPull = null;
            LastServed = null;
            ServedShowLeft = 0f;
            Toast = null;
            SyncCustomerModel();
            return true;
        }

        public void Restart()
        {
            NewGame(Environment.TickCount);
        }

        /// <summary>Hinh khach chi hien khi co khach dang doi; mat ve "cho".</summary>
        void SyncCustomerModel()
        {
            reactionLeft = 0f;
            if (customerModel != null && Session != null)
            {
                bool waiting = Session.Phase == GamePhase.Open && Session.CurrentCustomer != null;
                customerModel.SetActive(waiting);
                if (waiting && face != null) face.ShowWaiting();
            }
            Changed();
        }
    }
}
