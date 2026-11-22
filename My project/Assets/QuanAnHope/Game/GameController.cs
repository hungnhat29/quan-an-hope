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
            ShowMainMenu();
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
                Say("Ngày 1 · Quán còn đóng: quay nguyên liệu, nấu sẵn rồi bấm biển Mở quán.");
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
            Hand = HandKind.Empty;
            HeldIngredient = null;
            HeldDish = null;
            OnBoard = null;
        }

        // F-011: theo doi thay doi do dong ho gay ra (khach moi vao, 21:00, het ngay).
        string seenSeats;
        GamePhase seenPhase;
        bool seenClosed;
        bool seenBoss;

        /// <summary>Tua nhanh dong ho ngay (chi cho test PlayMode, constitution 1.4 muc 2: 1 gio = 40 giay).</summary>
        public float ClockScale = 1f;

        void Update()
        {
            if (Session == null) return;
            float dt = Time.deltaTime;
            if (Menu != null) Menu.Tick(Time.unscaledDeltaTime);
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
                || Session.BossArrived != seenBoss)
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
                    Sfx("chin", "Stove");
                    Say("Món đã chín. Lấy đĩa trống rồi bấm vào bếp để múc.");
                }
                else if (!wasBurnt && stove.IsBurnt) { Sfx("chay_khet", "Stove"); FxSmoke("Stove"); Say("Quá lửa! " + stove.Recipe.name + " bị trừ 1 bậc khi múc."); }
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
            SfxPull(pull, false);
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
                    Say("Giá relic đầy " + Session.Relics.Capacity + "/" + Session.Relics.Capacity
                        + ": nhấn Tab (hoặc bấm relic trên giá) để chọn relic bỏ; bấm lại bệ / chuột phải để huỷ.");
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
                Say(Session.Items.IsOpen ? "Chưa có vé đen. Mua ở quầy vật phẩm (Góc 2)." : "Vé đen bán ở quầy vật phẩm từ ngày " + Session.Balance.itemShopFromDay + ".");
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
                Say("Giá relic đầy " + Session.Relics.Capacity + "/" + Session.Relics.Capacity + ": nhấn Tab (hoặc bấm relic trên giá) để chọn relic bỏ, lấy " + pick.name + ".");
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

        /// <summary>Cam mot mon tu thung.</summary>
        public bool TakeIngredient(Ingredient ingredient)
        {
            if (!DayRunning || ingredient == null || !HandsFree("lấy " + ingredient.name)) return false;
            if (!InStock(ingredient) || ReferenceEquals(ingredient, OnBoard)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = ingredient;
            Log.OnTakeIngredient(Time.time);
            SfxHand("cam");
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
                Sfx("dat", "Board");
                chopTimer = 0f;
                Say(OnBoard.name + " đã lên thớt. Giữ chuột trái để sơ chế.");
                return true;
            }

            if (!OnBoard.prepared) { Say("Giữ chuột trái để sơ chế " + OnBoard.name + "."); return false; }
            if (!HandsFree("cầm " + OnBoard.name)) return false;
            Hand = HandKind.Ingredient;
            HeldIngredient = OnBoard;
            OnBoard = null;
            Sfx("cam", "Board");
            Say("Đang cầm " + HeldName + ". Mang tới bếp.");
            return true;
        }

        /// <summary>Giu chuot tren thot.</summary>
        public void HoldBoard(float seconds)
        {
            if (!DayRunning || OnBoard == null || OnBoard.prepared) return;
            NotePrepHold();
            SfxChop(seconds);
            if (Session.HoldPrep(seconds)) Say("Đã sơ chế xong " + OnBoard.name + ". Bấm thớt để cầm lên.");
        }

        // =====================================================================
        // Goc 4: bep
        // =====================================================================

        public bool SelectTool(string tool)
        {
            if (!DayRunning) return false;
            if (!Session.Stove.SelectTool(tool)) { Say("Bếp đang nấu, chưa đổi dụng cụ được."); return false; }
            Sfx("dat", "Stove");
            Say("Đặt " + tool.ToLowerInvariant() + " lên bếp.");
            return true;
        }

        public bool AddPantry(string item)
        {
            if (!DayRunning) return false;
            if (Session.Stove.Tool == null) { Say("Chọn dụng cụ trên giá trước: Chảo, Nồi hoặc Vỉ nướng."); return false; }
            if (!Session.Stove.AddPantry(item)) { Say("Bếp đang nấu, chưa cho thêm được."); return false; }
            if (Room != null) Room.OnAddedToStove(null, item);
            Sfx("cho_vao_bep", "Stove", 0.5f);
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
                Sfx("cho_vao_bep", "Stove");
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
                Sfx("muc", "Stove");
                if (dish.tier == 0) { Player.Shake(0.15f); Sfx("chay_khet", "Stove", 0.8f); FxSmoke("Stove"); } // Lac vi: rung camera 0,15 giay (Art Bible muc 11)
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
            Sfx("bam", "Knob");
            Say("Đang nấu " + Session.Stove.Recipe.name + "…");
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
            if (Session.Stove.State == StoveState.Empty && Session.Stove.Tool == null) { Say("Bếp đang trống."); return; }
            Session.Stove.Reset();
            Log.OnDiscard();
            Sfx("dat", "Stove");
            Say("Đã đổ bỏ đồ trên bếp.");
        }


        /// <summary>Cam mot dia tu quay cho hoac tu tu lanh (constitution muc 5.1, 5.7).</summary>
        public bool TakeDish(Dish dish)
        {
            if (!DayRunning || dish == null || !HandsFree("lấy đĩa")) return false;
            if (!Session.HasDish(dish)) return false;
            Hand = HandKind.Dish;
            HeldDish = dish;
            SfxHand("cam");
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
                default: name = "vé đen"; price = Session.Items.BlackPrice; r = Session.BuyBlackTicket(); break;
            }
            switch (r)
            {
                case ItemBuyResult.Bought:
                    Sfx("tien", item == ItemSauce ? "Sauce" : item == ItemMachine ? "FoodMachine" : "BlackTicket");
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
            SfxHand(r == SauceResult.Fail ? "sot_hong" : "sot_tot");
            FxSauce(r == SauceResult.Fail);
            if (r == SauceResult.Legend)
            {
                Say("Nước sốt thần thánh: " + d.name + " thành " + Dish.LegendName + " (Huyền thoại, giá ×"
                    + Session.Balance.dish.priceMult[d.tier].ToString("0.0") + ")! · còn " + Session.Items.Sauces + " chai.");
                return true;
            }
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
            Sfx(r == MachineResult.Trash ? "chay_khet" : "bam", "FoodMachine");
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
            SfxPull(pull, true);
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
                Sfx("chuong", "ShopSign", 0.7f);
                Toast = Session.Boss != null ? BossArrivedText(Session.Boss) : "Mở quán! Khách đầu tiên đã tới.";
                ClearReactions();
                Changed();
                return true;
            }
            if (Session.Phase == GamePhase.Open)
            {
                if (!Session.CloseEarly()) return false;
                Sfx("bam", "ShopSign");
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
            Sfx("chuong", "Bell");
            return ServeSeat(Session.SeatOf(Session.CurrentCustomer));
        }

        /// <summary>Nhin vao mot khach va bam: dua dia dang cam cho dung khach do (F-030 AC-3).</summary>
        public bool ServeSeat(int seat)
        {
            if (Session.Phase != GamePhase.Open) { Say("Chưa có khách. Bấm biển Mở quán trước."); return false; }
            if (seat >= 0 && seat < Session.SeatCount && Session.Seats[seat] == null && Session.Eaters[seat] != null)
            { Say("Khách đang ăn. Ăn xong khách rời ghế, khách mới vào sau " + Mathf.RoundToInt(Session.Balance.day.nextCustomerSeconds) + " giây."); return false; }
            if (seat < 0 || seat >= Session.SeatCount || Session.Seats[seat] == null) { Say("Ghế này đang trống."); return false; }
            Customer c = Session.Seats[seat];
            if (Hand != HandKind.Dish) { Say("Cầm " + c.orderName + " từ quầy chờ (hoặc tủ lạnh) tới đây."); return false; }

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
                Toast = "Sai món: khách gọi " + ordered + ". Khách giận, quán đền " + (-paid.amount) + "G.";
            else if (paid.outcome == PaymentOutcome.Satisfied)
                Toast = "Khách vui: +" + paid.amount + "G (món " + paid.price + "G, tip " + paid.tip + "G).";
            else if (paid.outcome == PaymentOutcome.Disappointed)
                Toast = "Món thấp hơn 1 bậc: khách chỉ trả " + paid.amount + "G, không tip.";
            else
                Toast = "Khách giận: quán đền " + (-paid.amount) + "G.";

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
            seenPhase = Session.Phase;
            seenSeats = SeatSignature();
            seenClosed = Session.IsClosedForNewCustomers;
            seenBoss = Session.BossArrived;

            if (dayEnded) { ClearHands(); ClearReactions(); }
            if (arrived != null && arrived.isBoss) Toast = BossArrivedText(arrived);
            else if (bossCame && Session.Phase == GamePhase.Prep) Toast = "Chủ nợ đã tới, đang đợi ngoài cửa: mở quán để ông vào.";
            else if (bossCame && Session.BossWaiting) Toast = "Chủ nợ đã tới, đang đợi ghế trống: phục vụ một khách để ông ngồi.";
            else if (justClosed && Session.Phase == GamePhase.Open) Toast = "21:00 · Quán đóng cửa, phục vụ nốt khách đang ngồi.";
            else if (arrived != null && !AnyReaction) Toast = "Khách mới vào: gọi " + arrived.orderName + ".";
            Changed();
        }

        public bool NextDay()
        {
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
            if (Session.IsBlackout) Toast = "Ngày " + Session.Day + ": sét đánh cột điện, mất điện! Nấu bằng nến, khó nhìn bậc món.";
            else if (Session.Phase == GamePhase.Prep && Session.IsBossDay)
                Toast = "Ngày " + Session.Day + ": đúng " + Session.Balance.day.bossHour + ":00 chủ nợ tới. Để dành món Tuyệt hảo: thưởng "
                    + Session.Balance.bossBonus + "G.";
            else if (Session.Phase == GamePhase.Prep && Session.Day == LastDayBeforeDebt(Session) && Session.Round == 3)
                Toast = "Ngày " + Session.Day + ": có kẻ tung tin xấu về quán. Mai chủ nợ thu " + Session.Balance.DebtDueOn(Session.Day + 1)
                    + "G: dốc hết món trong tủ, nước sốt, vé đen!";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep && Session.Round == 3)
                Toast = "Chủ nợ đã thu " + Session.LastDebtPaid + "G. Vòng 3: khách khó tính hơn hẳn; từ hôm nay nước sốt có "
                    + Session.Balance.sauce.oddsLate.legend + "% ra " + Dish.LegendName + ".";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep && Session.Round == 4)
                Toast = "Chủ nợ đã thu " + Session.LastDebtPaid + "G. Vòng 4: uy tín phục hồi; có thêm gà ta (vỉ nướng), bí đỏ, măng tây, nấm đông cô.";
            else if (Session.LastDebtPaid > 0 && Session.Phase == GamePhase.Prep)
                Toast = "Chủ nợ đã thu " + Session.LastDebtPaid + "G. Vòng " + Session.Round + ": tối đa " + Session.SeatCount
                    + " khách cùng ngồi, khách chờ lâu sẽ khó tính hơn; quầy vật phẩm mở ở Góc 2.";
            else if (Session.Phase == GamePhase.Prep && Session.Shop.IsFreeDay)
                Toast = "Ngày " + Session.Day + ": cửa hàng relic ở Góc 3 cho chọn 1 relic miễn phí · " + TicketLine() + ".";
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
            Say("[Gỡ lỗi] Đầu ngày " + day + " · " + money + "G · " + Session.Relics.Owned.Count + " relic (trung vị của bot gom bộ).");
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
