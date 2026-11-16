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

        /// <summary>Ban sprint 4 choi het vong 2: dau ngay 9 thu no 2.900G roi dung (impl plan sprint 4 muc 1).</summary>
        public const int PlayableRounds = 2;

        /// <summary>Ghe vua phuc vu (de hien dia vua dua), -1 neu chua.</summary>
        public int LastServedSeat { get; private set; }

        BalanceData data;
        // Mat vui / gian cua khach vua phuc vu o tung ghe (Art Bible muc 11: 0,8 giay).
        readonly float[] reactionLeft = new float[3];
        readonly PaymentOutcome[] reactionOutcome = new PaymentOutcome[3];

        /// <summary>Ghe nay dang hien phan ung cua khach vua phuc vu.</summary>
        public bool IsReacting(int seat, out PaymentOutcome outcome)
        {
            outcome = seat >= 0 && seat < reactionOutcome.Length ? reactionOutcome[seat] : PaymentOutcome.Satisfied;
            return seat >= 0 && seat < reactionLeft.Length && reactionLeft[seat] > 0f;
        }

        bool AnyReaction
        {
            get { foreach (float r in reactionLeft) if (r > 0f) return true; return false; }
        }

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
            int lastDay = GameSession.LastPlayableDayFor(data.balance, PlayableRounds);
            Session = new GameSession(data.balance, data.recipes, new SeededRandom(seed), lastDay);
            DishFood.Book = data.recipes;
            ClearHands();
            LastPull = null;
            LastServed = null;
            ServedShowLeft = 0f;
            Toast = null;
            ClearReactions();
            LastServedSeat = -1;
            PendingRelicSlot = -1;
            LastBoughtRelic = null;
            if (Player != null) Player.Teleport(Vector3.zero, 0f);
            Changed();
        }

        void ClearHands()
        {
            Hand = HandKind.Empty;
            HeldIngredient = null;
            HeldDish = null;
            OnBoard = null;
        }

        // F-011: theo doi thay doi do dong ho gay ra (khach moi vao, 21:00, het ngay).
        string seenSeats;
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
            if (Session.Phase != seenPhase || SeatSignature() != seenSeats || Session.IsClosedForNewCustomers != seenClosed)
                OnClockEvent();

            Stove stove = Session.Stove;
            if (stove.State == StoveState.Cooking || stove.State == StoveState.Ready)
            {
                bool wasCooking = stove.State == StoveState.Cooking;
                bool wasWarn = stove.IsOverheating, wasBurnt = stove.IsBurnt;
                stove.Tick(dt);
                if (wasCooking && stove.State == StoveState.Ready)
                {
                    Log.OnReady(Time.time);
                    Say("Món đã chín. Lấy đĩa trống rồi bấm vào bếp để múc.");
                }
                else if (!wasBurnt && stove.IsBurnt) Say("Quá lửa! " + stove.Recipe.name + " bị trừ 1 bậc khi múc.");
                else if (!wasWarn && stove.IsOverheating) Say("Bếp bốc khói: múc " + stove.Recipe.name + " ra ngay kẻo quá lửa.");
            }

            for (int i = 0; i < reactionLeft.Length; i++)
            {
                if (reactionLeft[i] <= 0f) continue;
                reactionLeft[i] -= dt;
                if (reactionLeft[i] <= 0f) Changed();
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
            seenSeats = SeatSignature();
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


        /// <summary>Cam mot dia tu quay cho hoac tu tu lanh (constitution muc 5.1, 5.7).</summary>
        public bool TakeDish(Dish dish)
        {
            if (!DayRunning || dish == null || !HandsFree("lấy đĩa")) return false;
            if (!Session.HasDish(dish)) return false;
            Hand = HandKind.Dish;
            HeldDish = dish;
            Say("Đang cầm " + dish.name + " · " + UiKit.DishTierName[dish.tier]
                + (Session.SeatCount > 1 ? ". Nhìn vào khách gọi món này rồi bấm." : ". Mang ra quầy Góc 1, bấm chuông."));
            return true;
        }

        int IndexOnCounter(Dish dish)
        {
            for (int i = 0; i < Session.Counter.Dishes.Count; i++)
                if (ReferenceEquals(Session.Counter.Dishes[i], dish)) return i;
            return -1;
        }

        // =====================================================================
        // Goc 4: tu lanh (F-032)
        // =====================================================================

        /// <summary>Bam tu lanh khi dang cam dia tu quay cho: cat vao tu.</summary>
        public bool UseFridge()
        {
            if (!DayRunning) return false;
            if (Hand != HandKind.Dish) { Say("Tủ lạnh: cầm một đĩa từ quầy chờ tới đây để cất, hoặc bấm một ô để lấy."); return false; }
            if (Session.Fridge.Contains(HeldDish)) { Say(HeldDish.name + " đã ở trong tủ. Chuột phải để đặt lại."); return false; }
            if (Session.Fridge.IsFull) { Say("Tủ lạnh đầy (" + Session.Fridge.Capacity + " ô)."); return false; }
            Dish d = HeldDish;
            if (!Session.StoreInFridge(d)) return false;
            Hand = HandKind.Empty;
            HeldDish = null;
            Say("Đã cất " + d.name + " vào tủ lạnh.");
            return true;
        }

        // =====================================================================
        // Goc 2: cua hang vat pham (F-034)
        // =====================================================================

        public bool BuyItem(string item)
        {
            ItemBuyResult r;
            string name;
            int price;
            switch (item)
            {
                case ItemSauce: name = "nước sốt thần thánh"; price = Session.Items.SaucePrice; r = Session.BuySauce(); break;
                case ItemMachine: name = "máy gacha đồ ăn"; price = Session.Balance.machine.price; r = Session.BuyMachine(); break;
                default: name = "vé đen"; price = Session.Items.BlackPrice; r = Session.BuyBlackTicket(); break;
            }
            switch (r)
            {
                case ItemBuyResult.Bought:
                    Say("Đã mua " + name + " (" + price + "G) · ví còn " + Session.Money + "G.");
                    return true;
                case ItemBuyResult.Closed:
                    Say("Quầy vật phẩm mở từ ngày " + Session.Balance.itemShopFromDay + ".");
                    return false;
                case ItemBuyResult.NotEnoughMoney:
                    Say("Thiếu " + (price - Session.Money) + "G để mua " + name + ".");
                    return false;
                default:
                    Say(item == ItemMachine ? "Đã có máy gacha đồ ăn." : "Đã giữ tối đa " + name + ".");
                    return false;
            }
        }

        public const string ItemSauce = "sauce";
        public const string ItemMachine = "machine";
        public const string ItemBlack = "black";

        /// <summary>Ket qua lan dung nuoc sot / may gacha gan nhat (de hien hieu ung).</summary>
        public string LastItemEffect { get; private set; }

        /// <summary>Bam lo nuoc sot khi dang cam dia: dung mot chai len dia (constitution muc 5.5).</summary>
        public bool UseSauceOnHeld()
        {
            if (!DayRunning) return false;
            if (Session.Items.Sauces <= 0) { Say(Session.Items.IsOpen ? "Hết nước sốt. Mua ở quầy vật phẩm." : "Quầy vật phẩm mở từ ngày " + Session.Balance.itemShopFromDay + "."); return false; }
            if (Hand != HandKind.Dish) { Say("Cầm một đĩa món rồi bấm lọ nước sốt."); return false; }
            Dish d = HeldDish;
            int before = d.tier;
            SauceResult r;
            if (!Session.UseSauce(d, out r)) return false;
            LastItemEffect = "sauce:" + r;
            if (r == SauceResult.Fail) Player.Shake(0.15f);
            Say("Nước sốt: " + d.name + " " + UiKit.DishTierName[before] + " → " + UiKit.DishTierName[d.tier] + " · còn " + Session.Items.Sauces + " chai.");
            return true;
        }

        /// <summary>Bam may gacha do an khi dang cam dia; chua co may thi mua (constitution muc 5.6, 8). Ti le an (Dieu 4).</summary>
        public bool UseMachineOnHeld()
        {
            if (!DayRunning) return false;
            if (!Session.Items.HasMachine) return BuyItem(ItemMachine);
            if (Hand != HandKind.Dish) { Say("Máy gacha đồ ăn: cầm một đĩa món tới rồi bấm. Hôm nay còn " + Session.Items.MachineUsesLeft + " lượt."); return false; }
            Dish d = HeldDish;
            if (d.machined) { Say(d.name + " đã vào máy một lần rồi."); return false; }
            if (Session.Items.MachineUsesLeft <= 0) { Say("Hôm nay máy đã chạy đủ " + Session.Balance.machine.usesPerDay + " lần."); return false; }
            int before = d.tier;
            MachineResult r;
            if (!Session.UseMachine(d, out r)) return false;
            LastItemEffect = "machine:" + r;
            if (r == MachineResult.Trash)
            {
                Hand = HandKind.Empty;
                HeldDish = null;
                Player.Shake(0.15f);
                Say("Máy kêu rắc rắc… " + d.name + " hỏng mất rồi!");
                return true;
            }
            Say("Máy gacha: " + d.name + " " + UiKit.DishTierName[before] + " → " + UiKit.DishTierName[d.tier] + ".");
            return true;
        }

        /// <summary>Nhet ve den vao khe may nhet ve (constitution muc 4.3).</summary>
        public bool PullBlack()
        {
            if (Session.Items.BlackTickets <= 0) { Say("Chưa có vé đen. Mua ở quầy vật phẩm (Góc 2)."); return false; }
            PullResult pull;
            if (!Session.PullBlack(out pull)) { Say("Giờ này không nhận thùng được."); return false; }
            LastPull = pull;
            Say("Vé đen: thùng " + pull.side.name + " và thùng " + pull.main.name + " hàng tốt đã rơi xuống bàn.");
            return true;
        }

        // =====================================================================
        // Goc 1: bien mo quan, khach, chuong
        // =====================================================================

        /// <summary>Bien Mo quan: dau ngay thi mo quan, dang mo thi dong quan som.</summary>
        public bool ToggleShop()
        {
            if (Session.Phase == GamePhase.Prep)
            {
                if (!Session.OpenShop()) return false;
                Toast = "Mở quán! Khách đầu tiên đã tới.";
                ClearReactions();
                Changed();
                return true;
            }
            if (Session.Phase == GamePhase.Open)
            {
                if (!Session.CloseEarly()) return false;
                ClearHands();
                Toast = "Đã đóng quán. Khách còn lại ra về.";
                ClearReactions();
                Changed();
                return true;
            }
            return false;
        }

        /// <summary>Ten ghe cho gioi y ("khach trai / giua / phai"); vong 1 chi mot ghe.</summary>
        public string SeatName(int seat)
        {
            if (Session.SeatCount <= 1) return "khách";
            switch (seat)
            {
                case 0: return "khách trái";
                case 1: return "khách giữa";
                default: return "khách phải";
            }
        }

        /// <summary>Chuong: phuc vu khach den som nhat dang ngoi.</summary>
        public bool RingBell()
        {
            if (Session.Phase != GamePhase.Open || Session.CurrentCustomer == null) { Say("Chưa có khách. Bấm biển Mở quán trước."); return false; }
            return ServeSeat(Session.SeatOf(Session.CurrentCustomer));
        }

        /// <summary>Nhin vao mot khach va bam: dua dia dang cam cho dung khach do (F-030 AC-3).</summary>
        public bool ServeSeat(int seat)
        {
            if (Session.Phase != GamePhase.Open) { Say("Chưa có khách. Bấm biển Mở quán trước."); return false; }
            if (seat < 0 || seat >= Session.SeatCount || Session.Seats[seat] == null) { Say("Ghế này đang trống."); return false; }
            Customer c = Session.Seats[seat];
            if (Hand != HandKind.Dish) { Say("Cầm " + c.orderName + " từ quầy chờ (hoặc tủ lạnh) tới đây."); return false; }

            Dish dish = HeldDish;
            if (!Session.HasDish(dish)) return false;
            string ordered = c.orderName;
            PaymentResult paid = Session.ServeSeat(seat, dish);
            if (paid == null) return false;
            Log.OnServed(Time.time, Session, dish);

            Hand = HandKind.Empty;
            HeldDish = null;
            LastServed = dish;
            LastServedSeat = seat;
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

            if (Session.Phase == GamePhase.Open)
            {
                reactionLeft[seat] = ReactionSeconds;
                reactionOutcome[seat] = paid.outcome;
            }
            else ClearReactions();
            Changed();
            return true;
        }

        // =====================================================================
        // sang ngay, choi lai
        // =====================================================================

        /// <summary>Chuoi so thu tu khach o tung ghe: doi khi co khach moi vao.</summary>
        string SeatSignature()
        {
            var sb = new System.Text.StringBuilder();
            foreach (Customer c in Session.Seats) sb.Append(c == null ? 0 : c.number).Append(',');
            return sb.ToString();
        }

        /// <summary>Dong ho vua lam doi trang thai: khach moi vao, 21:00 dong bien, hoac het ngay.</summary>
        void OnClockEvent()
        {
            Customer arrived = null;
            int maxSeen = 0;
            foreach (string part in (seenSeats ?? "").Split(','))
            {
                int n;
                if (int.TryParse(part, out n) && n > maxSeen) maxSeen = n;
            }
            foreach (Customer c in Session.Seats) if (c != null && c.number > maxSeen) arrived = c;
            bool justClosed = Session.IsClosedForNewCustomers && !seenClosed;
            bool dayEnded = seenPhase != Session.Phase && !DayRunning;
            seenPhase = Session.Phase;
            seenSeats = SeatSignature();
            seenClosed = Session.IsClosedForNewCustomers;

            if (dayEnded) { ClearHands(); ClearReactions(); }
            if (justClosed && Session.Phase == GamePhase.Open) Toast = "21:00 · Quán đóng cửa, phục vụ nốt khách đang ngồi.";
            else if (arrived != null && !AnyReaction) Toast = "Khách mới vào: gọi " + arrived.orderName + ".";
            Changed();
        }

        public bool NextDay()
        {
            if (!Session.StartNextDay()) return false;
            ClearHands();
            PendingRelicSlot = -1;
            LastBoughtRelic = null;
            LastPull = null;
            LastServed = null;
            LastServedSeat = -1;
            ServedShowLeft = 0f;
            if (Session.IsBlackout) Toast = "Ngày " + Session.Day + ": sét đánh cột điện, mất điện! Nấu bằng nến, khó nhìn bậc món.";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep)
                Toast = "Chủ nợ đã thu " + Session.LastDebtPaid + "G. Vòng " + Session.Round + ": tối đa " + Session.SeatCount
                    + " khách cùng ngồi, khách chờ lâu sẽ khó tính hơn; quầy vật phẩm mở ở Góc 2.";
            else Toast = null;
            ClearReactions();
            Changed();
            return true;
        }

        public void Restart()
        {
            NewGame(Environment.TickCount);
            Changed();
        }

        void ClearReactions()
        {
            for (int i = 0; i < reactionLeft.Length; i++) reactionLeft[i] = 0f;
        }

        /// <summary>Lenh go loi: nhay toi dau ngay va cho tien (chi Editor).</summary>
        public bool DebugJumpToDay(int day, int addMoney)
        {
            if (!Application.isEditor) return false;
            Session.DebugJumpToDay(day);
            Session.DebugAddMoney(addMoney);
            ClearHands();
            ClearReactions();
            LastPull = null;
            Say("[Gỡ lỗi] Sang đầu ngày " + day + ".");
            return true;
        }
    }
}
