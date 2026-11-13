using System;
using BepNhanPham;
using QuanAnHope.Core;
using QuanAnHope.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Noi luat choi (Core.GameSession) voi canh 4 goc va UI. Moi thao tac cua nguoi choi di qua cac ham
    /// public o day; UI chi goi ham va ve lai. Tu gan vao canh nao co CameraRig, khong can sua canh.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        const string BalanceResource = "BalanceData";
        const float ReactionSeconds = 0.8f; // Art Bible muc 11: khach vui / tip 0,8 giay

        public GameSession Session { get; private set; }
        public GameView View { get; private set; }
        public PullResult LastPull { get; private set; }
        public int SelectedDish { get; private set; }
        public string Toast { get; private set; }
        public bool Holding { get; set; }

        /// <summary>Goc dang xem: 0 Giao mon, 1 Gacha, 2 Sach va relic, 3 Bep.</summary>
        public int Corner { get { return rig != null ? rig.index : 0; } }

        BalanceData data;
        CameraRig rig;
        FaceSwap face;
        GameObject customerModel;
        float reactionLeft;
        bool dirty = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (FindAnyObjectByType<GameController>() != null) return;
            if (FindAnyObjectByType<CameraRig>() == null) return;
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

            rig = FindAnyObjectByType<CameraRig>();
            face = FindAnyObjectByType<FaceSwap>();
            if (face != null) customerModel = face.gameObject;

            NewGame(Environment.TickCount);
            View = new GameView(this);
            View.Refresh();
            SyncCustomerModel();
        }

        /// <summary>Bat dau van moi. Phan choi duoc cua ban nay la vong dau tien trong du lieu.</summary>
        public void NewGame(int seed)
        {
            int lastDay = data.balance.rounds[0].lastDay;
            Session = new GameSession(data.balance, data.recipes, new SeededRandom(seed), lastDay);
            LastPull = null;
            SelectedDish = 0;
            Toast = null;
            Holding = false;
            reactionLeft = 0f;
            dirty = true;
            SyncCustomerModel();
        }

        void Update()
        {
            if (Session == null) return;
            float dt = Time.deltaTime;

            if (Session.Stove.State == StoveState.Cooking)
            {
                Session.Stove.Tick(dt);
                if (Session.Stove.State == StoveState.Ready) Say("Món đã chín. Bấm Múc ra đĩa.");
            }

            if (Holding && Session.Board.Current != null)
            {
                string name = Session.Board.Current.name;
                if (Session.HoldPrep(dt)) Say("Đã sơ chế xong " + name + ".");
            }

            if (reactionLeft > 0f)
            {
                reactionLeft -= dt;
                if (reactionLeft <= 0f) SyncCustomerModel();
            }

            if (View != null)
            {
                if (dirty) { dirty = false; View.Refresh(); }
                View.Tick();
            }
        }

        void Say(string message)
        {
            Toast = message;
            dirty = true;
        }

        // ---------- camera ----------

        public void GoToCorner(int index)
        {
            if (rig != null) rig.GoTo(index);
            dirty = true;
        }

        // ---------- Goc 2 ----------

        public bool Pull()
        {
            PullResult pull;
            if (!Session.PullCrates(out pull))
            {
                Say(Session.Wallet.Count <= 0 ? "Hết vé rồi." : "Giờ này không nhận thùng được.");
                return false;
            }
            LastPull = pull;
            Say("Thùng " + pull.side.name + " và thùng " + pull.main.name + " đã rơi xuống.");
            return true;
        }

        // ---------- Goc 4 ----------

        /// <summary>Bam mot nguyen lieu trong kho: chua so che thi len thot, da so che thi vao bep.</summary>
        public bool UseIngredient(Ingredient ingredient)
        {
            if (ingredient == null) return false;
            if (!ingredient.prepared)
            {
                if (Session.Board.Current != null) { Say("Thớt đang bận, giữ nút để sơ chế xong đã."); return false; }
                if (!Session.BeginPrep(ingredient)) return false;
                Say(ingredient.name + " đang trên thớt. Giữ nút Sơ chế.");
                return true;
            }

            if (Session.Stove.Tool == null) { Say("Chọn dụng cụ trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
            if (!Session.AddToStove(ingredient)) { Say("Bếp không nhận thêm " + ingredient.name + " lúc này."); return false; }
            Say("Đã cho " + ingredient.name + " vào " + Session.Stove.Tool.ToLowerInvariant() + ".");
            return true;
        }

        public bool SelectTool(string tool)
        {
            if (!Session.Stove.SelectTool(tool)) { Say("Bếp đang nấu, chưa đổi dụng cụ được."); return false; }
            Say("Dùng " + tool.ToLowerInvariant() + ".");
            return true;
        }

        public bool AddPantry(string item)
        {
            if (Session.Stove.Tool == null) { Say("Chọn dụng cụ trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
            if (!Session.Stove.AddPantry(item)) { Say("Bếp đang nấu, chưa cho thêm được."); return false; }
            Say("Đã cho " + item + ".");
            return true;
        }

        public bool StartCooking()
        {
            if (!Session.Stove.Start()) { Say("Cần 1 nguyên liệu chính và 1 đồ phụ đã sơ chế trong bếp."); return false; }
            Say("Đang nấu " + Session.Stove.Recipe.name + "…");
            return true;
        }

        public bool Plate()
        {
            Dish dish;
            if (!Session.PlateDish(out dish))
            {
                Say(Session.Counter.IsFull ? "Quầy chờ đầy (" + Session.Counter.Capacity + " đĩa). Phục vụ bớt rồi múc." : "Món chưa chín.");
                return false;
            }
            SelectedDish = Session.Counter.Dishes.Count - 1;
            Say(dish.name + ": " + UiKit.DishTierName[dish.tier] + ".");
            return true;
        }

        public void DiscardStove()
        {
            Session.Stove.Reset();
            Say("Đã đổ bỏ đồ trên bếp.");
        }

        // ---------- Goc 1 ----------

        public bool OpenShop()
        {
            if (!Session.OpenShop()) return false;
            Say("Mở quán! Khách đầu tiên đã tới.");
            SyncCustomerModel();
            return true;
        }

        public void SelectDish(int index)
        {
            SelectedDish = index;
            dirty = true;
        }

        public bool RingBell()
        {
            if (Session.Counter.Dishes.Count == 0) { Say("Chưa có đĩa nào trên quầy chờ. Sang Góc 4 nấu đã."); return false; }
            int index = Mathf.Clamp(SelectedDish, 0, Session.Counter.Dishes.Count - 1);
            PaymentResult paid = Session.Serve(index);
            if (paid == null) { Say("Chưa có khách. Bấm Mở quán trước."); return false; }

            SelectedDish = 0;
            if (paid.outcome == PaymentOutcome.Satisfied)
                Say("Khách vui: +" + paid.amount + "G (món " + paid.price + "G, tip " + paid.tip + "G).");
            else if (paid.outcome == PaymentOutcome.Disappointed)
                Say("Món thấp hơn 1 bậc: khách chỉ trả " + paid.amount + "G, không tip.");
            else
                Say("Khách giận: quán đền " + (-paid.amount) + "G.");

            if (face != null)
            {
                if (paid.outcome == PaymentOutcome.Angry) face.ShowAngry();
                else if (paid.outcome == PaymentOutcome.Satisfied) face.ShowHappy();
                else face.ShowWaiting();
                if (customerModel != null) customerModel.SetActive(true);
                reactionLeft = ReactionSeconds;
            }
            else SyncCustomerModel();
            return true;
        }

        public bool CloseEarly()
        {
            if (!Session.CloseEarly()) return false;
            Say("Đã đóng quán. Khách còn lại ra về.");
            SyncCustomerModel();
            return true;
        }

        // ---------- sang ngay, choi lai ----------

        public bool NextDay()
        {
            if (!Session.StartNextDay()) return false;
            LastPull = null;
            SelectedDish = 0;
            Toast = null;
            dirty = true;
            SyncCustomerModel();
            return true;
        }

        public void Restart()
        {
            NewGame(Environment.TickCount);
            GoToCorner(0);
        }

        /// <summary>Hinh khach chi hien khi co khach dang doi; mat ve "cho".</summary>
        void SyncCustomerModel()
        {
            reactionLeft = 0f;
            if (customerModel == null || Session == null) return;
            bool waiting = Session.Phase == GamePhase.Open && Session.CurrentCustomer != null;
            customerModel.SetActive(waiting);
            if (waiting && face != null) face.ShowWaiting();
        }
    }
}
