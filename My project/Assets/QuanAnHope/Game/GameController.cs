using System;
using System.Collections.Generic;
using BepNhanPham;
using QuanAnHope.Core;
using QuanAnHope.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuanAnHope.Game
{
    /// <summary>Man dang hien (sprint 6, F-050): menu chinh, cot truyen, dang choi trong phong, tam dung.</summary>
    public enum GameMode
    {
        Menu,
        Story,
        Playing,
        Paused,
        /// <summary>Bang Tab: relic + cua hang (sprint 8 F-078), dong ho dung nhu tam dung.</summary>
        Tab,
    }

    public enum HandKind
    {
        Empty,
        Ingredient,
        Plate,
        Dish,
        /// <summary>Lo nuoc sot than thanh (sau sprint 10): cam tu canh bep, nho len dia vua nau.</summary>
        Sauce,
    }

    /// <summary>
    /// Noi luat choi (Core.GameSession) voi can phong 3D. Moi thao tac cua nguoi choi la mot ham public o day,
    /// do vat trong phong (RoomView) goi vao khi duoc bam. Tu gan vao canh co doi tuong "NhaHang".
    /// </summary>
    public partial class GameController : MonoBehaviour
    {
        const string BalanceResource = "BalanceData";
        const string RoomRootName = "NhaHang";
        const float ReactionSeconds = 0.8f; // Art Bible muc 11: khach vui / tip 0,8 giay

        public GameSession Session { get; private set; }
        public PlayerController Player { get; private set; }
        public RoomView Room { get; private set; }
        public Hud Hud { get; private set; }

        // ---- sprint 6: khung game ----
        public GameMode Mode { get; private set; }
        public MenuView Menu { get; private set; }
        public GameSettings Settings { get; private set; }
        public Tutorial Tutorial { get; private set; }

        /// <summary>Tu luu dau moi ngay (F-052). Tat voi van do test / go loi tao (NewGame truc tiep).</summary>
        public bool AutoSave { get; private set; }

        /// <summary>
        /// Menu chinh nhin tu cua vao phong (kich ban 10 giay sprint 6): dung ngay trong rem cua Goc 1 (rem o z = 3,72 + Grow,
        /// mep duoi 2,65 m), cao hon quay, nhin ve phia Goc 4; thay den treo dung dua phia tren.
        /// </summary>
        public static readonly Vector3 MenuCameraPosition = new Vector3(0f, 2.55f, 3.45f + BepNhanPham.RoomSize.Grow);
        public static readonly Quaternion MenuCameraRotation = Quaternion.Euler(10f, 180f, 0f);

        // ---- tay nguoi choi (constitution Dieu 5: cam toi da mot thu) ----
        // 2.2 (tay van nang): hai o cam do; Hand / HeldIngredient / HeldDish la o dang dung (GameControllerSprint12)
        public HandKind Hand { get { return slots[activeSlot].kind; } private set { SetSlotKind(value); } }
        public Ingredient HeldIngredient { get { return slots[activeSlot].ingredient; } private set { slots[activeSlot].ingredient = value; } }
        public Dish HeldDish { get { return slots[activeSlot].dish; } private set { slots[activeSlot].dish = value; } }

        /// <summary>Nguyen lieu dang nam tren thot dau tien (ke ca da so che xong ma chua cam len).</summary>
        public Ingredient OnBoard { get { return onBoards[0]; } }

        /// <summary>Nguyen lieu tren thot thu <paramref name="board"/> (constitution 2.0 muc 8: toi da 3 thot); null neu trong.</summary>
        public Ingredient OnBoardAt(int board) { return board >= 0 && board < onBoards.Length ? onBoards[board] : null; }

        /// <summary>Nguyen lieu nay dang nam tren mot thot nao do.</summary>
        public bool IsOnBoard(Ingredient ingredient)
        {
            foreach (Ingredient i in onBoards) if (i != null && ReferenceEquals(i, ingredient)) return true;
            return false;
        }

        readonly Ingredient[] onBoards = new Ingredient[3];

        public PullResult LastPull { get; private set; }
        public Dish LastServed { get; private set; }
        public PaymentResult LastServedPayment { get; private set; }
        public float ServedShowLeft { get; private set; }
        public string Toast { get; private set; }

        /// <summary>Be relic dang cho chon relic de bo (gia day 8/8); -1 neu khong (F-020 AC-5).</summary>
        public int PendingRelicSlot { get; private set; }

        /// <summary>Relic vua mua (de hien lap lanh tren gia, Art Bible muc 11).</summary>
        public RelicDef LastBoughtRelic { get; private set; }

        /// <summary>Ban Alpha (sprint 5) choi du 4 vong: dau ngay 22 thu no (du lieu) roi ra man thang (impl plan sprint 5 muc 1).</summary>
        public const int PlayableRounds = 4;

        /// <summary>Be relic dang cho chon de bo la lua chon ve den (F-042), khong phai be ve trang.</summary>
        public bool PendingIsBlack { get; private set; }

        // Ghe vua phuc vu chu no: mat ngac nhien neu duoc thuong (Art Bible muc 6), khong thi mat gian.
        int bossReactSeat = -1;
        bool bossReactHappy;

        /// <summary>Ghe nay dang hien phan ung cua chu no vua duoc phuc vu; <paramref name="bonus"/> = co thuong.</summary>
        public bool IsBossReaction(int seat, out bool bonus)
        {
            bonus = bossReactHappy;
            return seat == bossReactSeat && seat >= 0 && seat < reactionLeft.Length && reactionLeft[seat] > 0f;
        }

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
            // Man hinh theo Cai dat (F-054): mac dinh toan man hinh do phan giai goc (L-011: khong de che do cua so
            // con sot lai tu lan chay thu). Editor va batchmode giu nguyen.
            Settings = GameSettings.Load();

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
            Player.EscapePressed += OnEscape;
            FaceBillboard.View = Player.Cam;
            Settings.Apply(Player, true);
            InitAudio();
            InitAmbient();

            NewGame(Environment.TickCount);
            Room = new RoomView(this, GameObject.Find(RoomRootName).transform);
            Hud = new Hud(this);
            Menu = new MenuView(this);
            InitTab();
            Tutorial = new Tutorial(this);
            InitSprint11();
            ShowMainMenu();
            StartCoroutine(PrewarmIcons());
        }

        // =====================================================================
        // Khung game (sprint 6): menu, cot truyen, tam dung, luu / tai
        // =====================================================================

        /// <summary>Menu chinh (F-050 AC-1): camera nhin tu cua, con tro chuot hien, dong ho dung.</summary>
        public void ShowMainMenu()
        {
            Mode = GameMode.Menu;
            if (Tab != null) Tab.Hide();
            if (Tutorial != null) Tutorial.Stop();
            ClearHands();
            Player.HoldCamera(MenuCameraPosition, MenuCameraRotation);
            PlayerController.SetCursorLocked(false);
            if (Menu != null) Menu.ShowMain();
            Changed();
        }

        /// <summary>"Choi moi": van moi, cot truyen mo dau roi vao ngay 1 (co huong dan neu la van dau).</summary>
        public void StartNewGame()
        {
            int before = GameSettings.GamesStarted;
            GameSettings.GamesStarted = before + 1;
            NewGame(Environment.TickCount);
            AutoSave = true;
            SaveStore.Save(Session.Snapshot());
            if (Tutorial != null) Tutorial.Begin(Settings.TutorialForNewGame(before));
            Mode = GameMode.Story;
            Player.HoldCamera(MenuCameraPosition, MenuCameraRotation);
            Changed();
            Menu.PlayStory(StoryScript.Intro(Session.Balance), () =>
            {
                EnterPlaying();
                Say("Ngày 1 · Quay nguyên liệu, nấu sẵn, rồi mở quán.");
            });
        }

        /// <summary>"Choi tiep" (F-052 AC-2): ve dung dau ngay da luu. False neu khong co ban luu.</summary>
        public bool ContinueGame()
        {
            SaveData save = SaveStore.Load();
            if (save == null) { Menu.ShowMain(); return false; }
            GameSession loaded;
            try { loaded = GameSession.FromSave(data.balance, data.recipes, save); }
            catch (ArgumentException e)
            {
                Debug.LogWarning("Quán Ăn Hope: bản lưu hỏng (" + e.Message + ").");
                SaveStore.Delete();
                Menu.ShowMain();
                return false;
            }
            UseSession(loaded);
            AutoSave = true;
            if (Tutorial != null) Tutorial.Begin(Settings.TutorialForNewGame(Mathf.Max(0, GameSettings.GamesStarted - 1)));
            Toast = "Chơi tiếp từ đầu ngày " + Session.Day + ".";
            EnterPlaying();
            return true;
        }

        /// <summary>Vao phong choi: tra camera ve mat Hope, an menu, khoa chuot.</summary>
        void EnterPlaying()
        {
            Mode = GameMode.Playing;
            Player.ReleaseCamera();
            if (Menu != null) Menu.HideAll();
            PlayerController.SetCursorLocked(!ShowsOverlay);
            Changed();
        }

        /// <summary>Esc: dang choi thi tam dung; dang tam dung / cai dat thi quay lai (F-050 AC-2).</summary>
        void OnEscape()
        {
            if (Mode == GameMode.Playing) Pause();
            else if (Mode == GameMode.Tab) CloseTab();
            else if (Mode == GameMode.Paused)
            {
                if (Menu.SettingsShown) Menu.CloseSettings();
                else Resume();
            }
            else if (Mode == GameMode.Menu && Menu.SettingsShown) Menu.CloseSettings();
        }

        /// <summary>Tam dung: dong ho ngay, bep, khach deu dung (Update khong goi Session.Tick).</summary>
        public void Pause()
        {
            if (Mode != GameMode.Playing) return;
            Mode = GameMode.Paused;
            PlayerController.SetCursorLocked(false);
            Menu.ShowPause();
            Changed();
        }

        public void Resume()
        {
            if (Mode != GameMode.Paused) return;
            EnterPlaying();
        }

        public void Quit()
        {
            Application.Quit();
        }

        /// <summary>Cai dat vua doi (MenuView da luu va ap); huong dan bat / tat ngay.</summary>
        public void OnSettingsChanged()
        {
            if (Tutorial != null && Tutorial.Active && Settings.tutorialChosen && !Settings.tutorial) Tutorial.Stop();
            Changed();
        }

        public void OnTutorialChanged()
        {
            // bo hop chu vang (24/11/2026): loi "Xong! ..." cuoi bai hien thanh thong bao mot dong
            string line = Tutorial != null ? Tutorial.Line : null;
            if (line != null && line.StartsWith("Xong")) Toast = line;
            if (Hud != null) Hud.Refresh();
        }

        /// <summary>Doc lai cai dat (test doi GameSettings.Prefix de khong dung cai dat that).</summary>
        public void ReloadSettings()
        {
            Settings = GameSettings.Load();
            Settings.Apply(Player, false);
        }

        /// <summary>Ve menu tu man thang / thua.</summary>
        public void BackToMenuAfterEnd()
        {
            SaveStore.Delete();
            ShowMainMenu();
        }

        /// <summary>Bat dau van moi. Phan choi duoc cua ban nay la vong dau tien trong du lieu.</summary>
        /// <summary>F-014: ghi thoi gian moi mon vao playtest-log.csv (Application.persistentDataPath).</summary>
        public PlaytestLog Log { get; private set; }

        /// <summary>Thu muc ghi playtest-log.csv; null = Application.persistentDataPath. Test PlayMode dat thu muc tam (F-060 AC-1).</summary>
        public static string PlaytestFolder;

        public void NewGame(int seed)
        {
            int lastDay = GameSession.LastPlayableDayFor(data.balance, PlayableRounds);
            UseSession(new GameSession(data.balance, data.recipes, new SeededRandom(seed), lastDay));
            System.GC.Collect();   // F-092: don rac luc chuyen man (khong de GC roi vao luc dang choi)
            // Van tao truc tiep (test, go loi) vao thang phong, khong tu luu, khong huong dan.
            AutoSave = false;
            if (Tutorial != null) Tutorial.Stop();
            if (Menu != null) EnterPlaying();
        }

        /// <summary>Dung mot van choi (moi hoac vua tai): xoa tay, phan ung, thong bao.</summary>
        void UseSession(GameSession session)
        {
            if (Log == null) Log = new PlaytestLog(PlaytestFolder ?? Application.persistentDataPath);
            Session = session;
            DishFood.Book = data.recipes;
            ClearHands();
            LastPull = null;
            LastServed = null;
            ServedShowLeft = 0f;
            Toast = null;
            ClearReactions();
            LastServedSeat = -1;
            PendingRelicSlot = -1;
            PendingIsBlack = false;
            bossReactSeat = -1;
            LastBoughtRelic = null;
            ResetSprint8();
            ticketsAtDayEnd = -1;
            if (Player != null) Player.Teleport(Vector3.zero, 0f);
            Changed();
        }

        void ClearHands()
        {
            ClearSlots();
            MachineDish = null;
            machineSpin = 0f;
            for (int i = 0; i < onBoards.Length; i++) onBoards[i] = null;
            Dropped.Clear();
        }

        // F-011: theo doi thay doi do dong ho gay ra (khach moi vao, 21:00, het ngay).
        string seenSeats;
        GamePhase seenPhase;
        bool seenClosed;
        bool seenBoss;
        int seenLeft;
        bool seenReady;

        /// <summary>Tua nhanh dong ho ngay (chi cho test PlayMode, constitution 1.4 muc 2: 1 gio = 40 giay).</summary>
        public float ClockScale = 1f;

        void Update()
        {
            if (Session == null) return;
            float dt = Time.deltaTime;
            NormalizeSlots();
            TickMachine(dt);
            if (Menu != null) Menu.Tick(Time.unscaledDeltaTime);
            if (Room != null) Room.TickAlways();
            TickAudio(dt);
            if (Mode != GameMode.Playing)
            {
                // Menu, cot truyen, tam dung: dong ho ngay, bep, khach dung (F-050 AC-2).
                Player.InputEnabled = false;
                return;
            }

            // Dong ho ngay chi chay tu luc Mo quan (constitution 1.7 muc 6.2); Session.Tick bo qua pha Prep.
            Session.Tick(dt * ClockScale);
            if (Session.Phase != seenPhase || SeatSignature() != seenSeats || Session.IsClosedForNewCustomers != seenClosed
                || Session.BossArrived != seenBoss || Session.LeftToday != seenLeft || Session.ReadyToClose != seenReady)
                OnClockEvent();

            for (int si = 0; si < Session.Stoves.Count; si++)
            {
                Stove stove = Session.Stoves[si];
                if (stove.State != StoveState.Cooking && stove.State != StoveState.Ready) continue;
                string at = StoveId(si);
                bool wasCooking = stove.State == StoveState.Cooking;
                bool wasWarn = stove.IsOverheating, wasBurnt = stove.IsBurnt;
                stove.Tick(dt);
                if (wasCooking && stove.State == StoveState.Ready)
                {
                    Log.OnReady(Time.time);
                    Sfx("chin", at);
                    Say(StoveSay(si, "Chín rồi! Lấy đĩa trống, bấm vào bếp."));
                }
                else if (!wasBurnt && stove.IsBurnt) { Sfx("chay_khet", at); FxSmoke(at); Say(StoveSay(si, "Cháy rồi! Món tụt 1 bậc.")); }
                else if (!wasWarn && stove.IsOverheating) Say(StoveSay(si, "Bếp bốc khói! Múc ra ngay."));
            }

            for (int i = 0; i < reactionLeft.Length; i++)
            {
                if (reactionLeft[i] <= 0f) continue;
                // sprint 9: mot khung cham (ve anh mat lan dau) khong duoc nuot het 0,8 giay phan ung
                reactionLeft[i] -= Mathf.Min(dt, 0.25f);
                if (reactionLeft[i] <= 0f) Changed();
            }
            if (ServedShowLeft > 0f)
            {
                ServedShowLeft -= dt;
                if (ServedShowLeft <= 0f) Changed();
            }

            Player.InputEnabled = !ShowsOverlay;
            if (Room != null) Room.Tick(dt);
            if (Tutorial != null) Tutorial.Tick(dt);
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
            seenBoss = Session.BossArrived;
            seenLeft = Session.LeftToday;
            seenReady = Session.ReadyToClose;
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
                    case HandKind.Dish: return HeldDish.Title;
                    case HandKind.Sauce: return "nước sốt thần thánh";
                    default: return null;
                }
            }
        }

        bool HandsFree(string action)
        {
            if (Hand == HandKind.Empty) return true;
            // 2.2: co tay van nang va o kia trong thi cam vao o kia (dung theo thu tu nhat)
            if (UseOtherSlotIfFree()) return true;
            Say("Tay đang bận. Chuột phải để " + (Hand == HandKind.Ingredient ? "thả " : "đặt lại ") + HeldName + ".");
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
            SfxPull(pull, false);
            Say("+ " + pull.side.name + " · + " + pull.main.name
                + (pull.breakthrough != null ? " · Rương đột phá: " + pull.breakthrough.name + "!" : ""));
            return true;
        }

        // =====================================================================
        // Goc 3: cua hang relic (constitution muc 7.1)
        // =====================================================================

        /// <summary>Bam relic tren be: mua bang ve trang. Gia day thi chuyen sang chon relic de bo; bam lai be de huy.</summary>
        public bool BuyRelic(int slot)
        {
            if (Session.BlackRelicOffers.Count > 0) return ChooseBlackRelic(slot);
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
                    Sfx("mua_relic", "Relic_" + slot);
                    Say("Đã mua " + offer.name + " · còn " + Session.Wallet.Count + " vé.");
                    return true;
                case RelicBuyResult.NeedDiscard:
                    PendingRelicSlot = slot;
                    Say("Đã đủ " + Session.Relics.Capacity + " relic: nhấn Tab, chọn 1 relic để bỏ. Chuột phải: huỷ.");
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

        /// <summary>Khe ve den o Goc 3 (F-042, constitution muc 7.1): dung 1 ve den, ba be hien 3 relic Hiem tro len.</summary>
        public bool UseBlackForRelic()
        {
            if (!DayRunning) return false;
            if (Session.BlackRelicOffers.Count > 0) { Say("Đang chờ bạn chọn 1 trong 3 relic trên bệ."); return false; }
            if (Session.Items.BlackTickets <= 0)
            {
                Say(Session.Items.IsOpen ? "Chưa có vé đen. Mua trong Tab → Cửa hàng." : "Vé đen bán từ ngày " + Session.Balance.itemShopFromDay + ".");
                return false;
            }
            if (!HandsFree("dùng vé đen")) return false;
            if (!Session.UseBlackForRelic()) { Say("Không còn relic Hiếm nào để chọn."); return false; }
            PendingRelicSlot = -1;
            PendingIsBlack = false;
            Say("Vé đen: chọn 1 trong 3 relic Hiếm trở lên trên bệ.");
            return true;
        }

        bool ChooseBlackRelic(int slot)
        {
            IReadOnlyList<RelicDef> offers = Session.BlackRelicOffers;
            if (slot < 0 || slot >= offers.Count) return false;
            RelicDef pick = offers[slot];
            RelicBuyResult r = Session.ChooseBlackRelic(slot);
            if (r == RelicBuyResult.NeedDiscard)
            {
                PendingRelicSlot = slot;
                PendingIsBlack = true;
                Say("Đã đủ " + Session.Relics.Capacity + " relic: nhấn Tab, chọn 1 relic để bỏ, lấy " + pick.name + ".");
                return false;
            }
            if (r != RelicBuyResult.Bought) return false;
            PendingRelicSlot = -1;
            PendingIsBlack = false;
            LastBoughtRelic = pick;
            Sfx("mua_relic", "Relic_" + slot);
            Say("Vé đen: đã lấy " + pick.name + " (" + RelicKeys.RarityName(pick.rarity) + ").");
            return true;
        }

        /// <summary>Bam relic tren gia: khi dang chon de bo thi bo no va mua relic dang cho.</summary>
        public bool UseShelfRelic(string id)
        {
            if (PendingRelicSlot < 0) return false;
            int slot = PendingRelicSlot;
            PendingRelicSlot = -1;
            if (PendingIsBlack)
            {
                PendingIsBlack = false;
                RelicDef pick = slot < Session.BlackRelicOffers.Count ? Session.BlackRelicOffers[slot] : null;
                if (pick == null || Session.ChooseBlackRelic(slot, id) != RelicBuyResult.Bought) { Say("Không đổi được relic."); return false; }
                LastBoughtRelic = pick;
                Sfx("mua_relic", "Relic_" + slot);
                Say("Đã bỏ " + id + ", lấy " + pick.name + " bằng vé đen.");
                return true;
            }
            RelicDef offer = Session.Shop.Offers[slot];
            RelicDef old = null;
            foreach (RelicDef d in Session.Relics.Owned) if (d.id == id) old = d;
            if (Session.BuyRelic(slot, id) != RelicBuyResult.Bought)
            {
                Say("Không đổi được relic.");
                return false;
            }
            LastBoughtRelic = offer;
            Sfx("mua_relic", "Relic_" + slot);
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

        /// <summary>Cam mot mon tu thung (hoac nhat len tu cho da tha).</summary>
        public bool TakeIngredient(Ingredient ingredient)
        {
            if (!DayRunning || ingredient == null || !HandsFree("lấy " + ingredient.name)) return false;
            if (!InStock(ingredient) || IsOnBoard(ingredient)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = ingredient;
            Dropped.Remove(ingredient);
            Log.OnTakeIngredient(Time.time);
            SfxHand("cam");
            Say("Đang cầm " + HeldName + ".");
            return true;
        }

        /// <summary>
        /// Nguyen lieu da tha bang chuot phai (yeu cau 22/11/2026): nam o diem trong phong (toa do the gioi), nhat lai duoc.
        /// Het ngay thi mat nhu nguyen lieu trong thung (constitution muc 4.2).
        /// </summary>
        public readonly Dictionary<Ingredient, Vector3> Dropped = new Dictionary<Ingredient, Vector3>();

        /// <summary>
        /// Chuot phai: nguyen lieu roi ngay xuong cho dang nhin (mat ban, san...); dia / dia trong thi dat lai cho cu
        /// (quay cho, tu lanh, chong dia).
        /// </summary>
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
            if (Hand == HandKind.Ingredient && HeldIngredient != null && InStock(HeldIngredient))
            {
                Ingredient ing = HeldIngredient;
                Vector3 normal;
                Vector3 hit = Player.DropPoint(out normal);
                Vector3 at = Room != null ? Room.SurfaceAt(hit) : hit;
                Vector3 from = Room != null && Room.HandModel != null ? Room.HandModel.transform.position : Player.Cam.transform.position;
                Dropped[ing] = at;
                Hand = HandKind.Empty;
                HeldIngredient = null;
                if (Room != null) Room.DropFrom(ing, from);
                SfxHand("dat");
                Say("Đã thả " + name + ".");
                return;
            }
            SfxHand("dat");
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

        /// <summary>Ten tram cua thot thu <paramref name="board"/>: "Board", "Board_1", "Board_2".</summary>
        public static string BoardId(int board) { return board == 0 ? "Board" : "Board_" + board; }

        /// <summary>Bam vao thot: dat nguyen lieu len, hoac cam nguyen lieu da so che len.</summary>
        public bool UseBoard(int board = 0)
        {
            if (!DayRunning || board < 0 || board >= Session.Boards.Count) return false;
            Ingredient on = onBoards[board];

            if (on == null)
            {
                if (Hand != HandKind.Ingredient) { Say("Cầm một nguyên liệu tới đây."); return false; }
                if (HeldIngredient.prepared) { Say(HeldIngredient.name + " đã sơ chế, cho thẳng vào bếp."); return false; }
                if (!Session.BeginPrep(HeldIngredient, board)) return false;
                onBoards[board] = HeldIngredient;
                Hand = HandKind.Empty;
                HeldIngredient = null;
                Sfx("dat", BoardId(board));
                chopTimer = 0f;
                Say("Giữ chuột trái: sơ chế " + onBoards[board].name + ".");
                return true;
            }

            if (!on.prepared) { Say("Giữ chuột trái: sơ chế " + on.name + "."); return false; }
            if (!HandsFree("cầm " + on.name)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = on;
            onBoards[board] = null;
            Sfx("cam", BoardId(board));
            Say("Đang cầm " + HeldName + ". Mang tới bếp.");
            return true;
        }

        /// <summary>Giu chuot tren thot.</summary>
        public void HoldBoard(float seconds, int board = 0)
        {
            Ingredient on = OnBoardAt(board);
            if (!DayRunning || on == null || on.prepared) return;
            NotePrepHold(board);
            SfxChop(seconds);
            if (Session.HoldPrep(seconds, board)) Say("Xong! Bấm thớt để cầm " + on.name + ".");
        }

        // =====================================================================
        // Goc 4: bep
        // =====================================================================

        public bool SelectTool(string tool)
        {
            if (!DayRunning) return false;
            int si = StoveForTool();
            if (!Session.Stoves[si].SelectTool(tool)) { Say("Bếp đang nấu, chưa đổi dụng cụ được."); return false; }
            ActiveStove = si;
            Sfx("dat", StoveId(si));
            Say("Đặt " + tool.ToLowerInvariant() + " lên " + (Session.Stoves.Count > 1 ? "bếp " + (si + 1) : "bếp") + ".");
            return true;
        }

        public bool AddPantry(string item)
        {
            if (!DayRunning) return false;
            int si = StoveForPantry();
            Stove stove = Session.Stoves[si];
            if (stove.Tool == null) { Say("Chọn dụng cụ trên giá trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
            if (!stove.AddPantry(item)) { Say("Bếp đang nấu, chưa cho thêm được."); return false; }
            ActiveStove = si;
            if (Room != null) Room.OnAddedToStove(null, item, si);
            Sfx("cho_vao_bep", StoveId(si), 0.5f);
            Say("Đã cho " + item + ".");
            return true;
        }

        /// <summary>Bam vao bep: cho nguyen lieu dang cam vao, hoac muc mon ra dia trong dang cam.</summary>
        public bool UseStove(int si = 0)
        {
            if (!DayRunning || si < 0 || si >= Session.Stoves.Count) return false;
            ActiveStove = si;
            Stove stove = Session.Stoves[si];
            if (Hand == HandKind.Sauce) { Say("Múc món ra đĩa trước, rồi nhỏ nước sốt lên đĩa."); return false; }

            if (Hand == HandKind.Ingredient)
            {
                if (!HeldIngredient.prepared) { Say("Sơ chế " + HeldIngredient.name + " trên thớt trước."); return false; }
                if (stove.Tool == null) { Say("Chọn dụng cụ trên giá trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
                string name = HeldIngredient.name;
                if (!stove.Accepts(HeldIngredient))
                {
                    // constitution 2.0 muc 5.8: cap khong co mon
                    Ingredient other = HeldIngredient.kind == IngredientKind.Main ? stove.SideIn : stove.MainIn;
                    Say(name + " không nấu chung với " + (other != null ? other.name : "món trong bếp") + ". Xem Tab → Thực đơn.");
                    return false;
                }
                if (!Session.AddToStove(HeldIngredient, si)) { Say("Bếp không nhận thêm " + name + " lúc này."); return false; }
                if (Room != null) Room.OnAddedToStove(HeldIngredient, null, si);
                Sfx("cho_vao_bep", StoveId(si));
                Hand = HandKind.Empty;
                HeldIngredient = null;
                Say("Đã cho " + name + " vào " + stove.Tool.ToLowerInvariant() + ".");
                return true;
            }

            if (stove.State == StoveState.Ready)
            {
                if (Hand != HandKind.Plate) { Say("Lấy một đĩa trống ở chồng đĩa rồi bấm vào bếp để múc."); return false; }
                Dish dish;
                if (!Session.PlateDish(out dish, si))
                {
                    Say(Session.Counter.IsFull ? "Quầy đã đủ " + Session.Counter.Capacity + " đĩa. Phục vụ bớt rồi múc." : "Chưa múc được.");
                    return false;
                }
                Hand = HandKind.Empty;
                Log.OnPlated(dish);
                Sfx("muc", StoveId(si));
                if (dish.tier == 0) { Player.Shake(0.15f); Sfx("chay_khet", StoveId(si), 0.8f); FxSmoke(StoveId(si)); } // Lac vi: rung camera 0,15 giay (Art Bible muc 11)
                else if (dish.IsLegend) OnCookedLegend(dish);
                Say(dish.name + ": " + UiKit.DishTierName[dish.tier] + (dish.IsLegend ? "!" : ".")
                    + (Session.Items.Sauces > 0 && !dish.IsLegend ? " Có thể nhỏ nước sốt lên đĩa." : ""));
                return true;
            }

            if (stove.State == StoveState.Cooking) { Say("Đang nấu, chờ chín đã."); return false; }
            Say(stove.Tool == null ? "Chọn dụng cụ trên giá trước." : "Cầm nguyên liệu đã sơ chế tới, hoặc bấm lọ gia vị.");
            return false;
        }

        /// <summary>Vang num bep: bat dau nau.</summary>
        public bool StartCooking(int si = 0)
        {
            if (!DayRunning || si < 0 || si >= Session.Stoves.Count) return false;
            ActiveStove = si;
            Stove stove = Session.Stoves[si];
            if (!stove.Start()) { Say("Cần 1 nguyên liệu chính và 1 đồ phụ đã sơ chế trong bếp."); return false; }
            Log.OnKnob(Time.time, stove.Recipe, Session.Recipes, stove);
            Sfx("bam", KnobId(si));
            Say("Đang nấu " + stove.Recipe.name + "…");
            return true;
        }

        public bool TakePlate()
        {
            if (!DayRunning || !HandsFree("lấy đĩa")) return false;
            Hand = HandKind.Plate;
            Sfx("cam", "PlateStack");
            Say("Đang cầm đĩa trống.");
            return true;
        }

        public void DiscardStove()
        {
            if (!DayRunning) return;
            int si = StoveToDiscard();
            Stove stove = Session.Stoves[si];
            if (stove.State == StoveState.Empty && stove.Tool == null) { Say("Bếp đang trống."); return; }
            stove.Reset();
            Log.OnDiscard();
            Sfx("dat", StoveId(si));
            Say("Đã đổ bỏ đồ trên " + (Session.Stoves.Count > 1 ? "bếp " + (si + 1) : "bếp") + ".");
        }


        /// <summary>Cam mot dia tu quay cho hoac tu tu lanh (constitution muc 5.1, 5.7).</summary>
        public bool TakeDish(Dish dish)
        {
            // sau sprint 10: dang cam lo nuoc sot thi bam dia = nho sot len dia
            if (Hand == HandKind.Sauce) return DripSauce(dish);
            if (!DayRunning || dish == null || !HandsFree("lấy đĩa")) return false;
            if (!Session.HasDish(dish)) return false;
            Hand = HandKind.Dish;
            HeldDish = dish;
            SfxHand("cam");
            Say("Đang cầm " + dish.name + " · " + UiKit.DishTierName[dish.tier] + ". Đặt trước mặt khách gọi món này.");
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
            if (Hand != HandKind.Dish) { Say("Tủ lạnh: cầm đĩa tới để cất, hoặc bấm một ô để lấy."); return false; }
            if (Session.Fridge.Contains(HeldDish)) { Say(HeldDish.name + " đã ở trong tủ. Chuột phải để đặt lại."); return false; }
            if (Session.Fridge.IsFull) { Say("Tủ lạnh đầy (" + Session.Fridge.Capacity + " ô)."); return false; }
            Dish d = HeldDish;
            if (!Session.StoreInFridge(d)) return false;
            Hand = HandKind.Empty;
            HeldDish = null;
            Sfx("dat", "Fridge");
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
                case ItemBoard: name = "thớt"; price = Session.Items.BoardPrice; r = Session.BuyBoard(); break;
                case ItemStove: name = "bếp"; price = Session.Items.StovePrice; r = Session.BuyStove(); break;
                case ItemGlove: name = "tay vạn năng"; price = Session.Balance.glovePrice; r = Session.BuyGlove(); break;
                default: name = "vé đen"; price = Session.Items.BlackPrice; r = Session.BuyBlackTicket(); break;
            }
            switch (r)
            {
                case ItemBuyResult.Bought:
                    Sfx("tien", item == ItemSauce ? "Sauce" : item == ItemMachine ? "FoodMachine" : item == ItemBoard ? BoardId(Session.Boards.Count - 1)
                        : item == ItemStove ? StoveId(Session.Stoves.Count - 1) : "BlackTicket");
                    Say("Đã mua " + name + " (" + price + "G) · ví còn " + Session.Money + "G."
                        + (item == ItemBoard ? " Bàn sơ chế dài thêm, có thêm thớt." : item == ItemStove ? " Bếp mới ở bên phải quầy đĩa."
                        : item == ItemGlove ? " Cầm được 2 đồ: phím 1 / 2 chọn ô."
                        : item == ItemSauce ? " Lọ sốt ở bên phải bếp." : ""));
                    return true;
                case ItemBuyResult.Closed:
                    Say("Cửa hàng mở từ ngày " + Session.Balance.itemShopFromDay + ".");
                    return false;
                case ItemBuyResult.NotEnoughMoney:
                    Say("Thiếu " + (price - Session.Money) + "G để mua " + name + ".");
                    return false;
                default:
                    Say(item == ItemMachine ? "Đã có máy gacha đồ ăn." : item == ItemGlove ? "Đã có tay vạn năng." : item == ItemBoard ? "Đã đủ " + Session.Balance.board.max + " thớt."
                        : item == ItemStove ? "Đã đủ " + Session.Balance.stove.max + " bếp." : "Đã giữ tối đa " + name + ".");
                    return false;
            }
        }

        public const string ItemSauce = "sauce";
        public const string ItemMachine = "machine";
        public const string ItemBlack = "black";

        /// <summary>Thot mua them (constitution 2.0 muc 8).</summary>
        public const string ItemBoard = "board";

        /// <summary>Bep mua them (constitution 2.1 muc 8).</summary>
        public const string ItemStove = "stove";

        /// <summary>Tay van nang (constitution 2.2 muc 8).</summary>
        public const string ItemGlove = "glove";

        /// <summary>Ket qua lan dung nuoc sot / may gacha gan nhat (de hien hieu ung).</summary>
        public string LastItemEffect { get; private set; }

        /// <summary>Bam lo nuoc sot khi dang cam dia: dung mot chai len dia (constitution muc 5.5).</summary>
        public bool UseSauceOnHeld()
        {
            if (!DayRunning) return false;
            if (Session.Items.Sauces <= 0) { Say(Session.Items.IsOpen ? "Hết nước sốt. Mua trong Tab → Cửa hàng." : "Cửa hàng mở từ ngày " + Session.Balance.itemShopFromDay + "."); return false; }
            if (Hand != HandKind.Dish) { Say("Cầm một đĩa món rồi bấm lọ nước sốt."); return false; }
            Dish d = HeldDish;
            int before = d.tier;
            SauceResult r;
            if (!Session.UseSauce(d, out r)) return false;
            ShowSauceResult(d, before, r, Room != null && Room.HandModel != null ? Room.HandModel.transform.position : HandPosition);
            return true;
        }

        /// <summary>Bam may gacha do an khi dang cam dia; chua co may thi mua (constitution muc 5.6, 8). Ti le an (Dieu 4).</summary>
        public bool UseMachineOnHeld()
        {
            // 24/11/2026: may gacha kieu lo vi song - bam may = cho dia vao / lay dia ra; gat can ben canh de quay (GameControllerSprint12)
            return UseMachineBody();
        }

        /// <summary>Nhet ve den vao khe may nhet ve (constitution muc 4.3).</summary>
        public bool PullBlack()
        {
            if (Session.Items.BlackTickets <= 0) { Say("Chưa có vé đen. Mua trong Tab → Cửa hàng."); return false; }
            PullResult pull;
            if (!Session.PullBlack(out pull)) { Say("Giờ này không nhận thùng được."); return false; }
            LastPull = pull;
            SfxPull(pull, true);
            Say("Vé đen: + " + pull.side.name + " · + " + pull.main.name + " hàng tốt.");
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
                Sfx("chuong", "ShopSign", 0.7f);
                Toast = Session.Boss != null ? BossArrivedText(Session.Boss) : "Mở quán! Khách đầu tiên đã tới.";
                ClearReactions();
                Changed();
                return true;
            }
            if (Session.Phase == GamePhase.Open)
            {
                // constitution 2.0 muc 6.2: chi bam bien Dong quan moi het ngay
                bool early = !Session.ReadyToClose;
                if (!Session.CloseEarly()) return false;
                Sfx("bam", "ShopSign");
                ClearHands();
                Toast = early ? "Đóng quán sớm: khách còn lại ra về." : "Đóng quán. Hết ngày " + Session.Day + ".";
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

        /// <summary>
        /// Dat dia dang cam len ban truoc mat khach o ghe nay (hoac bam vao khach): phuc vu dung khach do, khong theo thu tu
        /// (yeu cau 22/11/2026: bo chuong, F-030 AC-3).
        /// </summary>
        public bool ServeSeat(int seat)
        {
            if (Session.Phase != GamePhase.Open) { Say("Chưa có khách. Bấm biển Mở quán trước."); return false; }
            if (seat >= 0 && seat < Session.SeatCount && Session.Seats[seat] == null && Session.Eaters[seat] != null)
            { Say("Khách đang ăn."); return false; }
            if (seat < 0 || seat >= Session.SeatCount || Session.Seats[seat] == null) { Say("Ghế này đang trống."); return false; }
            Customer c = Session.Seats[seat];
            if (Hand != HandKind.Dish) { Say("Mang " + c.orderName + " tới đây."); return false; }

            Dish dish = HeldDish;
            if (!Session.HasDish(dish)) return false;
            string ordered = c.orderName;
            PaymentResult paid = Session.ServeSeat(seat, dish);
            if (paid == null) return false;
            Log.OnServed(Time.time, Session, dish);
            SfxServe(seat, paid, c.isBoss);
            FxServe(seat, paid, c.isBoss);

            Hand = HandKind.Empty;
            HeldDish = null;
            LastServed = dish;
            LastServedSeat = seat;
            LastServedPayment = paid;
            ServedShowLeft = ReactionSeconds * 2f;
            if (Session.Phase != GamePhase.Open) ClearHands();

            if (c.isBoss)
                Toast = paid.bossBonus > 0
                    ? "Chủ nợ ngạc nhiên! +" + paid.amount + "G (món " + paid.price + "G, tip " + paid.tip + "G, thưởng " + paid.bossBonus + "G)."
                    : "Chủ nợ nhận " + dish.Title + " (" + UiKit.DishTierName[Mathf.Min(dish.tier, UiKit.DishTierName.Length - 1)] + "): "
                        + (paid.amount >= 0 ? "+" : "") + paid.amount + "G, không có thưởng (cần Tuyệt hảo trở lên).";
            else if (paid.wrongOrder)
                Toast = "Sai món! Khách gọi " + ordered + ": −" + (-paid.amount) + "G.";
            else if (paid.outcome == PaymentOutcome.Satisfied)
                Toast = "Khách vui: +" + paid.amount + "G" + (paid.tip > 0 ? " (tip " + paid.tip + "G)" : "") + ".";
            else if (paid.outcome == PaymentOutcome.Disappointed)
                Toast = "Hơi tệ: khách trả nửa giá, +" + paid.amount + "G.";
            else
                Toast = "Món quá tệ, khách giận: −" + (-paid.amount) + "G.";

            bossReactSeat = c.isBoss ? seat : (bossReactSeat == seat ? -1 : bossReactSeat);
            if (c.isBoss) bossReactHappy = paid.bossBonus > 0;
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
            // khach an xong roi ghe cung la mot thay doi can hien (constitution 1.5 muc 6.2)
            foreach (Customer c in Session.Eaters) sb.Append(c == null ? 0 : -c.number).Append(',');
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
            bool bossCame = Session.BossArrived && !seenBoss;
            bool left = Session.LeftToday > seenLeft;
            bool ready = Session.ReadyToClose && !seenReady;
            seenPhase = Session.Phase;
            seenSeats = SeatSignature();
            seenClosed = Session.IsClosedForNewCustomers;
            seenBoss = Session.BossArrived;
            seenLeft = Session.LeftToday;
            seenReady = Session.ReadyToClose;

            if (dayEnded) { ClearHands(); ClearReactions(); }
            if (left && Session.LastLeave != null) OnCustomerLeft(Session.LastLeave);
            if (arrived != null && !arrived.isBoss) OnCustomerArrived(arrived);
            if (arrived != null && arrived.isBoss) Toast = BossArrivedText(arrived);
            else if (left && Session.LastLeave != null) Toast = "Khách chờ lâu quá, bỏ đi: −" + Session.LastLeave.penalty + "G.";
            else if (bossCame && Session.Phase == GamePhase.Prep) Toast = "Chủ nợ đã tới, đợi ngoài cửa: mở quán để ông vào.";
            else if (bossCame && Session.BossWaiting) Toast = "Chủ nợ đã tới, đợi ghế trống.";
            else if (ready) Toast = "21:00 · Hết khách. Bấm biển để đóng quán.";
            else if (justClosed && Session.Phase == GamePhase.Open) Toast = "21:00 · Không nhận khách mới. Phục vụ nốt rồi đóng quán.";
            else if (arrived != null && !AnyReaction) Toast = "Khách mới: " + arrived.orderName + ".";
            Changed();
        }

        public bool NextDay()
        {
            System.GC.Collect();   // F-092: man ket thuc ngay la luc don rac
            NoteDayEndTickets();
            if (!Session.StartNextDay()) return false;
            ResetSprint8();
            if (Session.LastDebtPaid > 0 && Audio != null) Audio.Play("thu_no", HandPosition);
            if (AutoSave)
            {
                // F-052 AC-1 / AC-3: tu luu dau ngay; thang hoac thua thi xoa ban luu.
                if (Session.Phase == GamePhase.Prep) SaveStore.Save(Session.Snapshot());
                else if (Session.Phase == GamePhase.Lost || Session.Phase == GamePhase.Cleared) SaveStore.Delete();
            }
            ClearHands();
            PendingRelicSlot = -1;
            LastBoughtRelic = null;
            LastPull = null;
            LastServed = null;
            LastServedSeat = -1;
            ServedShowLeft = 0f;
            if (Session.IsBlackout) Toast = "Ngày " + Session.Day + ": mất điện! Nấu bằng nến, khó nhìn bậc món.";
            else if (Session.Phase == GamePhase.Prep && Session.IsBossDay)
                Toast = "Ngày " + Session.Day + ": " + Session.Balance.day.bossHour + ":00 chủ nợ tới. Để dành món Tuyệt hảo!";
            else if (Session.Phase == GamePhase.Prep && Session.Day == LastDayBeforeDebt(Session) && Session.Round == 3)
                Toast = "Ngày " + Session.Day + ": tin đồn xấu về quán. Mai trả nợ " + Session.Balance.DebtDueOn(Session.Day + 1)
                    + "G: dốc hết món tủ lạnh, nước sốt, vé đen!";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep && Session.Round == 3)
                Toast = "Đã trả " + Session.LastDebtPaid + "G. Vòng 3: khách khó tính hơn.";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep && Session.Round == 4)
                Toast = "Đã trả " + Session.LastDebtPaid + "G. Vòng 4: thêm gà ta, bí đỏ, măng tây, nấm đông cô.";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep)
                Toast = "Đã trả " + Session.LastDebtPaid + "G. Vòng " + Session.Round + ": " + Session.SeatCount + " khách cùng lúc. Tab → Cửa hàng đã mở.";
            else if (Session.Phase == GamePhase.Prep && Session.Shop.IsFreeDay)
                Toast = "Ngày " + Session.Day + ": chọn 1 relic miễn phí · " + TicketLine() + ".";
            else Toast = Session.Phase == GamePhase.Prep ? TicketLine() : null;
            ClearReactions();
            Changed();
            if (Menu != null && (Session.Phase == GamePhase.Lost || Session.IsWon)) PlayEnding();
            else if (Menu != null && PlayChapters && Session.Phase == GamePhase.Prep) PlayChapter(Session.Day);
            return true;
        }

        /// <summary>
        /// Trang chuong giua van (CotTruyen Chuong 2-4) chieu dau ngay 2, ngay mat dien, ngay chu no thu lan 3.
        /// Test choi qua nhieu ngay tat di de khong vuong man truyen.
        /// </summary>
        public bool PlayChapters = ChaptersByDefault;

        /// <summary>Mac dinh cho moi GameController (test dat false de ca khi nap lai canh van tat).</summary>
        public static bool ChaptersByDefault = true;

        /// <summary>Chieu trang chuong cua ngay <paramref name="day"/> neu co; xong thi ve phong choi. False neu ngay khong co chuong.</summary>
        public bool PlayChapter(int day)
        {
            List<StoryPanel> chapter = StoryScript.Chapter(Session.Balance, day);
            if (chapter.Count == 0 || Menu == null) return false;
            Mode = GameMode.Story;
            PlayerController.SetCursorLocked(false);
            Changed();
            Menu.PlayStory(chapter, () => EnterPlaying());
            return true;
        }

        /// <summary>Tranh ket (F-051 AC-2): thang thi chu no ga con gai; thua thi ghi ro thieu bao nhieu. Xong thi hien man ket thuc.</summary>
        void PlayEnding()
        {
            int due = Session.Balance.DebtDueOn(Session.Day);
            var panels = Session.IsWon ? StoryScript.Win(Session.Balance, Session.Money) : StoryScript.Lose(Session.Day, due, Session.Money);
            Mode = GameMode.Story;
            PlayerController.SetCursorLocked(false);
            Changed();
            Menu.PlayStory(panels, () =>
            {
                Mode = GameMode.Playing;
                PlayerController.SetCursorLocked(false);
                Changed();
            });
        }

        string BossArrivedText(Customer boss)
        {
            return "Chủ nợ đã tới · dễ tính " + boss.easiness + "% · gọi " + boss.orderName
                + ". Món Tuyệt hảo trở lên: thưởng " + Session.Balance.bossBonus + "G.";
        }

        static int LastDayBeforeDebt(GameSession s)
        {
            DebtEntry next = s.NextDebt();
            return next != null ? next.day - 1 : -1;
        }

        /// <summary>"Choi lai" o man thang / thua: van moi vao thang ngay 1 (khong chieu lai mo dau), tu luu nhu van thuong.</summary>
        public void Restart()
        {
            bool save = AutoSave;
            NewGame(Environment.TickCount);
            if (save)
            {
                AutoSave = true;
                GameSettings.GamesStarted = GameSettings.GamesStarted + 1;
                SaveStore.Save(Session.Snapshot());
            }
            Changed();
        }

        void ClearReactions()
        {
            for (int i = 0; i < reactionLeft.Length; i++) reactionLeft[i] = 0f;
        }

        /// <summary>
        /// Lenh go loi "bat dau tu vong N voi tien va relic trung binh cua bot" (impl plan sprint 5 muc 5): dat tien dung bang
        /// <paramref name="money"/>, thay gia relic bang <paramref name="relicIds"/>. Chi Editor.
        /// </summary>
        public bool DebugStartAt(int day, int money, string[] relicIds, string topDishRecipe = null)
        {
            if (!DebugJumpToDay(day, 0)) return false;
            Recipe top = topDishRecipe != null ? Session.Lookup.ById(topDishRecipe) : null;
            // bot gom bo de danh mot dia Tuyet hao trong tu lanh cho chu no (97% van, bot_report.md)
            if (top != null)
                Session.Fridge.TryStore(new Dish { recipeId = top.id, name = top.name, baseName = top.baseName, tier = Session.Balance.dish.maxTier });
            Session.DebugAddMoney(money - Session.Money);
            var owned = new List<string>();
            foreach (RelicDef r in Session.Relics.Owned) owned.Add(r.id);
            foreach (string id in owned) Session.Relics.Remove(id);
            foreach (string id in relicIds) Session.DebugGiveRelic(id);
            Say("[Gỡ lỗi] Đầu ngày " + day + " · " + money + "G · " + Session.Relics.Owned.Count + " relic.");
            return true;
        }

        /// <summary>Lenh go loi cho test PlayMode: dat mot thung nguyen lieu cho truoc len ban (chi Editor; vd thung ga ta de thu vi nuong).</summary>
        public bool DebugAddCrate(Crate crate)
        {
            if (!Application.isEditor || crate == null || !DayRunning) return false;
            Session.Stock.AddCrate(crate);
            Changed();
            return true;
        }

        /// <summary>Lenh go loi: nhay toi dau ngay va cho tien (chi Editor).</summary>
        public bool DebugJumpToDay(int day, int addMoney)
        {
            if (!Application.isEditor) return false;
            Session.DebugJumpToDay(day);
            Session.DebugAddMoney(addMoney);
            PendingRelicSlot = -1;
            PendingIsBlack = false;
            ClearHands();
            ClearReactions();
            LastPull = null;
            Say("[Gỡ lỗi] Sang đầu ngày " + day + ".");
            return true;
        }
    }
}
