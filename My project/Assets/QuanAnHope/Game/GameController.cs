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

            NewGame(Environment.TickCount);
            Room = new RoomView(this, GameObject.Find(RoomRootName).transform);
            Hud = new Hud(this);
            Changed();
        }

        /// <summary>Bat dau van moi. Phan choi duoc cua ban nay la vong dau tien trong du lieu.</summary>
        public void NewGame(int seed)
        {
            int lastDay = data.balance.rounds[0].lastDay;
            Session = new GameSession(data.balance, data.recipes, new SeededRandom(seed), lastDay);
            ClearHands();
            LastPull = null;
            LastServed = null;
            ServedShowLeft = 0f;
            Toast = null;
            reactionLeft = 0f;
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

        void Update()
        {
            if (Session == null) return;
            float dt = Time.deltaTime;

            if (Session.Stove.State == StoveState.Cooking)
            {
                Session.Stove.Tick(dt);
                if (Session.Stove.State == StoveState.Ready) Say("Món đã chín. Lấy đĩa trống rồi bấm vào bếp để múc.");
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
            Say("Thùng " + pull.side.name + " và thùng " + pull.main.name + " đã rơi xuống bàn.");
            return true;
        }

        /// <summary>Cam mot mon tu thung.</summary>
        public bool TakeIngredient(Ingredient ingredient)
        {
            if (!DayRunning || ingredient == null || !HandsFree("lấy " + ingredient.name)) return false;
            if (!InStock(ingredient) || ReferenceEquals(ingredient, OnBoard)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = ingredient;
            Say("Đang cầm " + HeldName + ".");
            return true;
        }

        /// <summary>Chuot phai: dat lai thu dang cam ve cho cu (thung, chong dia, quay cho).</summary>
        public void PutDown()
        {
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
            PaymentResult paid = Session.Serve(index);
            if (paid == null) return false;

            Hand = HandKind.Empty;
            HeldDish = null;
            LastServed = dish;
            LastServedPayment = paid;
            ServedShowLeft = ReactionSeconds * 2f;
            if (Session.Phase != GamePhase.Open) ClearHands();

            if (paid.outcome == PaymentOutcome.Satisfied)
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

        public bool NextDay()
        {
            if (!Session.StartNextDay()) return false;
            ClearHands();
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
