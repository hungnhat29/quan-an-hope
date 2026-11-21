using System;
using System.Collections.Generic;

namespace QuanAnHope.Core
{
    public enum GamePhase
    {
        /// <summary>Dau ngay: quay gacha, nau truoc; chua co khach.</summary>
        Prep,

        /// <summary>Da bam Mo quan: khach den lan luot.</summary>
        Open,

        /// <summary>Qua 21:00 va het khach dang ngoi (hoac dong quan som): cho sang ngay moi.</summary>
        DayEnd,

        /// <summary>Da tra no va di het phan choi duoc cua ban nay.</summary>
        Cleared,

        /// <summary>Khong du tien tra no dung han.</summary>
        Lost,
    }

    /// <summary>
    /// Mot van choi: ngay, tien, ve, kho, bep, quay cho, khach (constitution muc 2, 3, 4.2, 5.1, 6).
    /// Lop nay khong biet gi ve Unity; thoi gian di vao qua Tick (dong ho, khach) va HoldPrep / Stove.Tick (bep).
    /// </summary>
    public partial class GameSession
    {
        readonly IRandom rng;
        readonly IngredientGacha gacha;
        readonly CustomerGenerator customers;

        public Balance Balance { get; private set; }
        public RecipeBook Recipes { get; private set; }
        public RecipeLookup Lookup { get; private set; }
        public DishFormula Formula { get; private set; }

        public TicketWallet Wallet { get; private set; }
        public IngredientStock Stock { get; private set; }
        public PrepStation Board { get; private set; }
        public Stove Stove { get; private set; }
        public Counter Counter { get; private set; }

        /// <summary>Gia relic 8 o va cua hang 3 be (constitution muc 7).</summary>
        public RelicInventory Relics { get; private set; }
        public RelicShop Shop { get; private set; }

        /// <summary>Thung dot pha cua lan quay gan nhat (null neu khong trung).</summary>
        public Crate LastBreakthrough { get; private set; }

        public int Day { get; private set; }
        public int Money { get; private set; }
        public GamePhase Phase { get; private set; }

        /// <summary>Ngay cuoi cung choi duoc trong ban nay; qua ngay do (sau khi tra no) la Cleared.</summary>
        public int LastPlayableDay { get; private set; }

        /// <summary>Tu lanh (constitution muc 5.7) va do mua bang tien (muc 8).</summary>
        public Fridge Fridge { get; private set; }
        public ItemShop Items { get; private set; }

        /// <summary>Ghe khach hom nay: vong 1 mot ghe, tu vong 2 ba ghe (constitution muc 6.1). Phan tu null = ghe trong.</summary>
        public IReadOnlyList<Customer> Seats { get { return seats; } }
        public int SeatCount { get { return seats.Length; } }
        Customer[] seats = new Customer[0];
        float[] seatTimers = new float[0];

        // Khach da duoc phuc vu, dang an (constitution 1.5 muc 6.2): ghe chua trong cho toi khi an xong.
        Customer[] eaters = new Customer[0];
        Dish[] eatingDishes = new Dish[0];
        float[] eatTimers = new float[0];

        /// <summary>Khach dang an o tung ghe (null = khong ai an). Khach dang an khong goi mon, khong nhan dia.</summary>
        public IReadOnlyList<Customer> Eaters { get { return eaters; } }

        /// <summary>Dia khach o ghe nay dang an (null neu khong ai an).</summary>
        public Dish EatingDish(int seat) { return seat >= 0 && seat < eatingDishes.Length ? eatingDishes[seat] : null; }

        /// <summary>Phan mon da an o ghe nay, 0..1 (0 neu khong ai an).</summary>
        public float EatProgress(int seat)
        {
            if (seat < 0 || seat >= eaters.Length || eaters[seat] == null || Balance.day.eatSeconds <= 0f) return 0f;
            return Math.Max(0f, Math.Min(1f, 1f - eatTimers[seat] / Balance.day.eatSeconds));
        }

        /// <summary>Con khach nao dang an khong.</summary>
        public bool AnyoneEating
        {
            get
            {
                foreach (Customer c in eaters) if (c != null) return true;
                return false;
            }
        }

        /// <summary>Khach den som nhat trong so khach dang ngoi (vong 1: khach duy nhat); null neu khong co ai.</summary>
        public Customer CurrentCustomer
        {
            get
            {
                Customer first = null;
                foreach (Customer c in seats)
                    if (c != null && (first == null || c.number < first.number)) first = c;
                return first;
            }
        }

        /// <summary>Ghe cua khach nay; -1 neu khach khong ngoi.</summary>
        public int SeatOf(Customer c)
        {
            for (int i = 0; i < seats.Length; i++) if (c != null && ReferenceEquals(seats[i], c)) return i;
            return -1;
        }

        /// <summary>Ngay mat dien (constitution muc 2: ngay 8).</summary>
        public bool IsBlackout { get { return Balance.IsBlackoutDay(Day); } }

        /// <summary>Do de tinh cua khach luc nay, da tru gio cho (constitution muc 6.6).</summary>
        public int EasinessNow(Customer c)
        {
            return Patience.EasinessNow(Balance, Day, c, Relics.Effects);
        }

        /// <summary>Thanh kien nhan 0..1 tren bong bong.</summary>
        public float PatienceLeft(Customer c)
        {
            return Patience.Left(Balance, Day, c, Relics.Effects);
        }

        public bool IsImpatient(Customer c)
        {
            return Patience.IsImpatient(Balance, Day, c, Relics.Effects);
        }

        /// <summary>Khoang an toan hom nay (giay; vo cung o vong 1).</summary>
        public float SafeSeconds { get { return Patience.SafeSeconds(Balance, Day, Relics.Effects); } }

        /// <summary>Ngay cuoi cua vong <paramref name="round"/> (ban sprint 4 choi het vong 2, ngay 9 thu no roi dung).</summary>
        public static int LastPlayableDayFor(Balance b, int round)
        {
            foreach (RoundRange r in b.rounds) if (r.round == round) return r.lastDay;
            throw new ArgumentOutOfRangeException("round", "Khong co vong " + round);
        }

        /// <summary>Ngay cuoi cung choi duoc cua ca game (ngay 21; dau ngay 22 thu no cuoi, constitution muc 2).</summary>
        public static int LastDayOfGame(Balance b)
        {
            int last = 0;
            foreach (RoundRange r in b.rounds) last = Math.Max(last, r.lastDay);
            return last;
        }

        /// <summary>Thang: tra du ca bon khoan no, qua dau ngay 22 (constitution muc 2: chu no ga con gai cho Hope).</summary>
        public bool IsWon { get { return Phase == GamePhase.Cleared && LastPlayableDay >= LastDayOfGame(Balance); } }

        // ---------- chu no ngay 21 (constitution muc 2, 6.2, 6.3, 6.7, 8) ----------

        /// <summary>Hom nay chu no den (ngay 21).</summary>
        public bool IsBossDay { get { return Day == Balance.day.bossDay; } }

        /// <summary>Da qua 20:00 cua ngay chu no: ong da toi (ngoi trong quan, dang doi ghe, hoac doi ngoai cua neu quan chua mo).</summary>
        public bool BossArrived { get; private set; }

        /// <summary>Chu no da toi nhung chua co ghe (quan chua mo, hoac ca ba ghe dang co khach).</summary>
        public bool BossWaiting { get { return BossArrived && !BossServed && !bossSeated && DayIsRunning; } }

        /// <summary>Chu no dang ngoi (null neu chua vao hoac da duoc phuc vu).</summary>
        public Customer Boss
        {
            get
            {
                foreach (Customer c in seats) if (c != null && c.isBoss) return c;
                return null;
            }
        }

        /// <summary>Da phuc vu chu no hom nay.</summary>
        public bool BossServed { get; private set; }
        bool bossSeated;

        /// <summary>Giay thuc tu dau ngay toi luc chu no den (20:00).</summary>
        float BossArrivesAt { get { return (Balance.day.bossHour - Balance.day.openHour) * Balance.day.secondsPerHour; } }

        void CheckBossArrival()
        {
            if (IsBossDay && !BossArrived && Clock.Elapsed >= BossArrivesAt - 1e-4f) BossArrived = true;
            if (Phase != GamePhase.Open || !BossWaiting) return;
            int seat = BossSeat();
            if (seat >= 0) SeatBoss(seat);
        }

        /// <summary>Ghe trong cho chu no: uu tien ghe giua (kich ban 10 giay sprint 5), khong thi ghe trong dau tien; -1 neu het ghe.</summary>
        int BossSeat()
        {
            int mid = seats.Length / 2;
            if (SeatFree(mid)) return mid;
            for (int i = 0; i < seats.Length; i++) if (SeatFree(i)) return i;
            return -1;
        }

        /// <summary>Ghe khong co khach ngoi doi va khong co khach dang an.</summary>
        bool SeatFree(int i)
        {
            return seats[i] == null && eaters[i] == null;
        }

        void SeatBoss(int seat)
        {
            seatTimers[seat] = -1f;
            CustomersToday++;
            seats[seat] = new Customer
            {
                easiness = Balance.customers.bossEasy,
                isBoss = true,
                orderId = null,
                orderName = Customer.BossOrderName,
                number = CustomersToday,
            };
            bossSeated = true;
        }

        // ---------- relic bang ve den (constitution muc 7.1) ----------

        RelicDef[] blackOffers = new RelicDef[0];

        /// <summary>Ba lua chon dang cho nguoi choi chon sau khi dung ve den; rong neu khong co.</summary>
        public IReadOnlyList<RelicDef> BlackRelicOffers { get { return blackOffers; } }

        /// <summary>Dung 1 ve den o Goc 3: hien 3 relic Hiem tro len. False neu khong co ve den hoac dang cho chon.</summary>
        public bool UseBlackForRelic()
        {
            if (!DayIsRunning || blackOffers.Length > 0 || Items.BlackTickets <= 0) return false;
            RelicDef[] offers = Shop.RollBlackOffers(Relics);
            if (offers.Length == 0) return false;
            Items.SpendBlack();
            blackOffers = offers;
            return true;
        }

        /// <summary>Chon relic thu <paramref name="index"/> trong 3 lua chon ve den; gia day thi phai chi ra relic bo.</summary>
        public RelicBuyResult ChooseBlackRelic(int index, string discardId = null)
        {
            if (!DayIsRunning) return RelicBuyResult.Closed;
            if (index < 0 || index >= blackOffers.Length) return RelicBuyResult.Empty;
            if (Relics.IsFull)
            {
                if (discardId == null || !Relics.Has(discardId)) return RelicBuyResult.NeedDiscard;
                Relics.Remove(discardId);
            }
            Relics.Add(blackOffers[index]);
            blackOffers = new RelicDef[0];
            return RelicBuyResult.Bought;
        }

        /// <summary>So khach da vao quan trong ngay (khong gioi han, constitution 1.4 muc 6.2).</summary>
        public int CustomersToday { get; private set; }
        public int ServedToday { get; private set; }
        public int IncomeToday { get; private set; }
        public PaymentResult LastPayment { get; private set; }

        /// <summary>So tien vua bi thu no khi sang ngay hien tai (0 neu khong den han).</summary>
        public int LastDebtPaid { get; private set; }

        /// <summary>Dong ho 9:00-21:00, chay tu dau ngay ke ca khi quan chua mo (muc 6.2).</summary>
        public DayClock Clock { get; private set; }

        /// <summary>Gio quan dong trong ngay (vd "21:04"); null khi ngay chua het.</summary>
        public string ClosedAt { get; private set; }

        /// <summary>Da qua 21:00: khong nhan khach moi, khach dang ngoi van duoc phuc vu.</summary>
        public bool IsClosedForNewCustomers { get { return Clock.IsPastClose; } }

        public int Round { get { return Balance.RoundOfDay(Day); } }

        /// <summary>Nguon ngau nhien cua van (bot dung de quyet dinh giong mo phong).</summary>
        public IRandom Rng { get { return rng; } }

        public GameSession(Balance balance, RecipeBook recipes, IRandom rng, int lastPlayableDay)
        {
            if (balance == null) throw new ArgumentNullException("balance");
            if (recipes == null) throw new ArgumentNullException("recipes");
            if (rng == null) throw new ArgumentNullException("rng");

            Balance = balance;
            Recipes = recipes;
            this.rng = rng;
            LastPlayableDay = lastPlayableDay;

            Lookup = new RecipeLookup(recipes);
            Formula = new DishFormula(balance);
            Relics = new RelicInventory(balance);
            Formula.Relics = Relics;
            Shop = new RelicShop(balance, rng);
            gacha = new IngredientGacha(balance, recipes, rng);
            customers = new CustomerGenerator(balance, rng);

            Wallet = new TicketWallet(balance);
            Stock = new IngredientStock();
            Board = new PrepStation();
            Stove = new Stove(Lookup, Formula);
            Counter = new Counter(balance);
            Clock = new DayClock(balance.day);
            Fridge = new Fridge(balance, Relics);
            Items = new ItemShop(balance);

            Day = 1;
            Money = balance.start.money;
            Wallet.BeginDay(Day, Relics.Effects);
            Shop.Restock(Day, Relics);
            BeginDayRules();
            Phase = GamePhase.Prep;
        }

        /// <summary>Luat theo ngay: so ghe, qua lua (tu vong 2), mat dien (ngay 8), luot may gacha do an.</summary>
        void BeginDayRules()
        {
            int n = Balance.SeatsForDay(Day);
            seats = new Customer[n];
            seatTimers = new float[n];
            eaters = new Customer[n];
            eatingDishes = new Dish[n];
            eatTimers = new float[n];
            for (int i = 0; i < n; i++) seatTimers[i] = -1f;
            Stove.BurnEnabled = Round >= Recipes.timing.burnFromRound;
            Stove.CookMultiplier = IsBlackout ? Balance.blackout.cookMultiplier : 1f;
            Formula.Blackout = IsBlackout;
            Items.BeginDay(Day);
        }

        // ---------- go loi / test (chi Editor va test goi) ----------

        /// <summary>Nhay toi dau ngay <paramref name="day"/> (pha Prep), khong thu no. Dung cho test va cua so go loi.</summary>
        public void DebugJumpToDay(int day)
        {
            if (day < 1) throw new ArgumentOutOfRangeException("day");
            ResetDayState();
            Stock.Clear();
            Counter.Clear();
            Stove.Reset();
            Board.Cancel();
            Day = day;
            Wallet.BeginDay(Day, Relics.Effects);
            Shop.Restock(Day, Relics);
            BeginDayRules();
            Phase = GamePhase.Prep;
        }

        /// <summary>Them ve trang (test, cua so go loi).</summary>
        public void DebugAddTickets(int count)
        {
            Wallet.Restore(Wallet.Count + count);
        }

        public void DebugAddMoney(int amount)
        {
            Money += amount;
        }

        bool DayIsRunning { get { return Phase == GamePhase.Prep || Phase == GamePhase.Open; } }

        /// <summary>Khoan no sap toi (ngay den han sau ngay hien tai); null neu khong con.</summary>
        public DebtEntry NextDebt()
        {
            return Balance.NextDebtAfter(Day);
        }

        // ---------- Goc 2: gacha ----------

        public bool PullCrates(out PullResult result)
        {
            result = null;
            if (!DayIsRunning) return false;
            if (!gacha.TryPull(Wallet, Day, out result, Relics.Effects)) return false;
            Stock.AddCrate(result.side);
            Stock.AddCrate(result.main);
            LastBreakthrough = result.breakthrough;
            if (result.breakthrough != null) Stock.AddCrate(result.breakthrough);
            return true;
        }

        /// <summary>Nhet ve den vao may: thung theo bang ve den (constitution muc 4.3), khong ton ve trang.</summary>
        public bool PullBlack(out PullResult result)
        {
            result = null;
            if (!DayIsRunning || !Items.SpendBlack()) return false;
            result = gacha.Pull(Day, Relics.Effects, true);
            Stock.AddCrate(result.side);
            Stock.AddCrate(result.main);
            LastBreakthrough = result.breakthrough;
            if (result.breakthrough != null) Stock.AddCrate(result.breakthrough);
            return true;
        }

        // ---------- cua hang vat pham (constitution muc 8) ----------

        public ItemBuyResult BuySauce()
        {
            int cost;
            ItemBuyResult r = DayIsRunning ? Items.CheckSauce(Money, out cost) : Closed(out cost);
            if (r == ItemBuyResult.Bought) { Money -= cost; Items.AddSauce(); }
            return r;
        }

        public ItemBuyResult BuyMachine()
        {
            int cost;
            ItemBuyResult r = DayIsRunning ? Items.CheckMachine(Money, out cost) : Closed(out cost);
            if (r == ItemBuyResult.Bought) { Money -= cost; Items.AddMachine(); }
            return r;
        }

        public ItemBuyResult BuyBlackTicket()
        {
            int cost;
            ItemBuyResult r = DayIsRunning ? Items.CheckBlack(Money, out cost) : Closed(out cost);
            if (r == ItemBuyResult.Bought) { Money -= cost; Items.AddBlack(); }
            return r;
        }

        static ItemBuyResult Closed(out int cost)
        {
            cost = 0;
            return ItemBuyResult.Closed;
        }

        /// <summary>Dia nay dang nam tren quay cho hoac trong tu lanh.</summary>
        public bool HasDish(Dish d)
        {
            if (d == null) return false;
            foreach (Dish x in Counter.Dishes) if (ReferenceEquals(x, d)) return true;
            return Fridge.Contains(d);
        }

        bool RemoveDish(Dish d)
        {
            for (int i = 0; i < Counter.Dishes.Count; i++)
                if (ReferenceEquals(Counter.Dishes[i], d)) { Counter.TakeAt(i); return true; }
            return Fridge.Remove(d);
        }

        /// <summary>Dung mot chai nuoc sot len dia (constitution muc 5.5). False neu het sot hoac dia khong o quay / tu.</summary>
        public bool UseSauce(Dish dish, out SauceResult result)
        {
            result = SauceResult.Same;
            if (!DayIsRunning || !HasDish(dish) || Items.Sauces <= 0) return false;
            Items.SpendSauce();
            result = ItemRolls.Sauce(Balance, Day, Relics.Effects, rng);
            dish.tier = ItemRolls.ApplySauce(result, dish.tier, Balance);
            return true;
        }

        /// <summary>Cho dia vao may gacha do an (constitution muc 5.6): moi mon 1 lan, 3 lan moi ngay; hong thi mat mon.</summary>
        public bool UseMachine(Dish dish, out MachineResult result)
        {
            result = MachineResult.Same;
            if (!DayIsRunning || !HasDish(dish) || dish.machined || !Items.HasMachine || Items.MachineUsesLeft <= 0) return false;
            Items.SpendMachineUse();
            result = ItemRolls.Machine(Balance, rng);
            int tier = ItemRolls.ApplyMachine(result, dish.tier, Balance);
            dish.machined = true;
            if (tier < 0) RemoveDish(dish);
            else dish.tier = tier;
            return true;
        }

        // ---------- Goc 4: tu lanh (constitution muc 5.7) ----------

        /// <summary>Cat dia tu quay cho vao tu lanh. False neu tu day hoac dia khong o quay.</summary>
        public bool StoreInFridge(Dish dish)
        {
            if (!DayIsRunning || dish == null || Fridge.IsFull) return false;
            for (int i = 0; i < Counter.Dishes.Count; i++)
            {
                if (!ReferenceEquals(Counter.Dishes[i], dish)) continue;
                Counter.TakeAt(i);
                return Fridge.TryStore(dish);
            }
            return false;
        }

        /// <summary>Ti le ra thung dot pha moi luot voi relic dang co (bang ti le Goc 2, Dieu 4).</summary>
        public double BreakthroughChance { get { return gacha.BreakthroughChance(Relics.Effects); } }
        public double BreakthroughTier5 { get { return gacha.BreakthroughTier5(Relics.Effects); } }
        public int BreakthroughItems { get { return gacha.BreakthroughItems(Relics.Effects); } }

        // ---------- Goc 3: cua hang relic (constitution muc 7.1) ----------

        /// <summary>Mua relic o be <paramref name="slot"/> bang ve trang; gia day thi phai chi ra relic bo.</summary>
        public RelicBuyResult BuyRelic(int slot, string discardId = null)
        {
            if (!DayIsRunning) return RelicBuyResult.Closed;
            return Shop.Buy(slot, Wallet, Relics, discardId);
        }

        /// <summary>Lenh go loi "cho toi relic X" (impl plan sprint 3 muc 5; chi goi tu Editor).</summary>
        public bool DebugGiveRelic(string id)
        {
            foreach (RelicDef r in Balance.relics)
                if (r.id == id) return Relics.Add(r);
            return false;
        }

        // ---------- Goc 4: bep ----------

        public bool BeginPrep(Ingredient ingredient)
        {
            if (!DayIsRunning || !Contains(ingredient)) return false;
            return Board.Begin(ingredient);
        }

        /// <summary>Giu thot them mot khoang thoi gian; true dung luc so che xong.</summary>
        public bool HoldPrep(float seconds)
        {
            return DayIsRunning && Board.Hold(seconds);
        }

        /// <summary>Cho nguyen lieu da so che tu kho vao bep.</summary>
        public bool AddToStove(Ingredient ingredient)
        {
            if (!DayIsRunning || !Contains(ingredient)) return false;
            if (!Stove.AddIngredient(ingredient)) return false;
            Stock.Remove(ingredient);
            return true;
        }

        public bool PlateDish(out Dish dish)
        {
            dish = null;
            return DayIsRunning && Stove.TryPlate(Counter, rng, out dish);
        }

        // ---------- Goc 1: khach ----------

        /// <summary>Nut Mo quan: tu luc nay khach moi bat dau den, lan luot tung nguoi.</summary>
        public bool OpenShop()
        {
            if (Phase != GamePhase.Prep || IsClosedForNewCustomers) return false;
            Phase = GamePhase.Open;
            // Ghe dau co khach ngay; cac ghe con lai (tu vong 2) co khach sau vai giay (muc 6.2).
            // Chu no dang doi ngoai cua thi vao ngay khi mo quan (impl plan sprint 5 F-043 AC-5).
            int first = 0;
            if (BossWaiting) { first = BossSeat(); SeatBoss(first); }
            else SeatCustomer(0);
            for (int i = 0; i < seats.Length; i++) if (i != first) seatTimers[i] = Balance.day.nextCustomerSeconds;
            return true;
        }

        /// <summary>
        /// Thoi gian troi (constitution 1.7 muc 6.2): dong ho dung o 9:00 toi khi bam Mo quan (gacha, so che, nau truoc
        /// khi mo khong mat gio); khi quan mo va ghe trong,
        /// khach ke tiep vao sau <see cref="DayRules.nextCustomerSeconds"/> giay; toi 21:00 khong nhan khach moi,
        /// khong con ai ngoi thi het ngay.
        /// </summary>
        public void Tick(float seconds)
        {
            if (!DayIsRunning || seconds <= 0f) return;
            if (Phase == GamePhase.Prep) return;   // chua mo quan: dong ho chua chay (yeu cau 21/11/2026)

            float before = Clock.Elapsed;
            Clock.Advance(seconds);
            CheckBossArrival();

            if (Phase == GamePhase.Open)
            {
                // Khach dang ngoi cho them (constitution muc 6.6).
                foreach (Customer c in seats) if (c != null) c.waitedSeconds += seconds;

                // Khach an xong thi roi ghe; khach moi vao sau nextCustomerSeconds tinh tu luc do (muc 6.2).
                for (int i = 0; i < eaters.Length; i++)
                {
                    if (eaters[i] == null) continue;
                    float left = eatTimers[i];
                    eatTimers[i] -= seconds;
                    if (eatTimers[i] > 0f) continue;
                    eaters[i] = null;
                    eatingDishes[i] = null;
                    FreeSeat(i, Math.Max(0f, left));
                }

                for (int i = 0; i < seats.Length; i++)
                {
                    if (seats[i] != null || seatTimers[i] < 0f) continue;
                    float arrivesAt = before + seatTimers[i];
                    seatTimers[i] -= seconds;
                    if (seatTimers[i] > 0f) continue;
                    if (arrivesAt < Clock.DaySeconds)
                    {
                        SeatCustomer(i);
                        // vao giua buoc Tick: chi tinh phan thoi gian sau khi ngoi
                        seats[i].waitedSeconds = Math.Max(0f, Clock.Elapsed - arrivesAt);
                    }
                    else seatTimers[i] = -1f;
                }
            }

            if (IsClosedForNewCustomers && CurrentCustomer == null && !AnyoneEating && !(Phase == GamePhase.Open && BossWaiting)) EndDay();
        }

        void SeatCustomer(int seat)
        {
            seatTimers[seat] = -1f;
            CustomersToday++;
            Customer c = NextCustomer();
            c.number = CustomersToday;
            seats[seat] = c;
        }

        // ---------- goi mon (constitution muc 6.7) ----------

        const string StandardGroup = "chinh";

        /// <summary>Khach moi vao: quay do de tinh roi goi mot mon.</summary>
        Customer NextCustomer()
        {
            Customer c = customers.Next(Round);
            Recipe order = ChooseOrder();
            c.orderId = order.id;
            c.orderName = order.name;
            return c;
        }

        /// <summary>
        /// Nhung mon lam duoc ngay luc nay, sap theo id: dia tren quay cho, mon dang tren bep,
        /// va moi cap (loai chinh, loai phu) dang co trong kho.
        /// </summary>
        public List<Recipe> MakeableRecipes()
        {
            var byId = new SortedDictionary<string, Recipe>(StringComparer.Ordinal);
            foreach (Dish d in Counter.Dishes) AddRecipe(byId, Lookup.ById(d.recipeId));
            foreach (Dish d in Fridge.Dishes) AddRecipe(byId, Lookup.ById(d.recipeId));
            AddRecipe(byId, Stove.PendingRecipe);

            var mains = new List<string>();
            var sides = new List<string>();
            foreach (Ingredient i in Stock.Items)
            {
                List<string> list = i.kind == IngredientKind.Main ? mains : sides;
                if (!list.Contains(i.typeId)) list.Add(i.typeId);
            }
            foreach (string m in mains)
                foreach (string p in sides)
                    AddRecipe(byId, Lookup.Find(m, p));
            return new List<Recipe>(byId.Values);
        }

        static void AddRecipe(SortedDictionary<string, Recipe> byId, Recipe r)
        {
            if (r != null && !byId.ContainsKey(r.id)) byId[r.id] = r;
        }

        /// <summary>Mon trong sach da mo khoa o vong hien tai (constitution muc 5.8).</summary>
        public List<Recipe> BookRecipes()
        {
            var list = new List<Recipe>();
            foreach (Recipe r in Recipes.recipes)
                if (r.group == StandardGroup && r.unlockRound >= 1 && r.unlockRound <= Round) list.Add(r);
            return list;
        }

        /// <summary>
        /// Chon mon cho khach: deu giua cac mon lam duoc; khong lam duoc mon nao thi deu giua cac mon trong sach.
        /// </summary>
        public Recipe ChooseOrder()
        {
            List<Recipe> candidates = MakeableRecipes();
            if (candidates.Count == 0) candidates = BookRecipes();
            return candidates[rng.NextInt(0, candidates.Count - 1)];
        }

        /// <summary>Dua dia o vi tri nay tren quay cho cho khach den som nhat dang ngoi. Null neu khong phuc vu duoc.</summary>
        public PaymentResult Serve(int counterIndex)
        {
            if (counterIndex < 0 || counterIndex >= Counter.Dishes.Count) return null;
            return ServeSeat(SeatOf(CurrentCustomer), Counter.Dishes[counterIndex]);
        }

        /// <summary>
        /// Dua dia (tren quay cho hoac trong tu lanh) cho khach o ghe <paramref name="seat"/> (constitution muc 6.7: nguoi choi
        /// chon phuc vu ai truoc). Tien va bac toi thieu tinh theo do de tinh luc phuc vu (muc 6.6). Null neu khong phuc vu duoc.
        /// </summary>
        public PaymentResult ServeSeat(int seat, Dish dish)
        {
            if (Phase != GamePhase.Open || seat < 0 || seat >= seats.Length || seats[seat] == null) return null;
            if (!HasDish(dish)) return null;
            Customer c = seats[seat];

            RemoveDish(dish);
            // Sai mon: khach nhan dia nhung gian (constitution muc 6.7). Chu no nhan moi mon (Q-20).
            PaymentResult payment = c.isBoss || dish.recipeId == c.orderId
                ? Payment.Settle(Balance, Round, EasinessNow(c), dish.tier, Relics.Effects)
                : Payment.WrongOrder(Balance, Round);
            if (c.isBoss)
            {
                // Tuyet hao hoac Huyen thoai: thuong ngoai tien mon (constitution muc 8).
                if (dish.tier >= Balance.dish.maxTier) payment.bossBonus = Balance.bossBonus;
                payment.amount += payment.bossBonus;
                BossServed = true;
            }
            Money += payment.amount;
            IncomeToday += payment.amount;
            LastPayment = payment;
            ServedToday++;

            seats[seat] = null;
            if (Balance.day.eatSeconds > 0f)
            {
                // Khach an tai ghe (constitution 1.5 muc 6.2); an xong moi roi ghe (Tick).
                eaters[seat] = c;
                eatingDishes[seat] = dish;
                eatTimers[seat] = Balance.day.eatSeconds;
                seatTimers[seat] = -1f;
            }
            else FreeSeat(seat, 0f);
            return payment;
        }

        /// <summary>
        /// Ghe vua trong (khach an xong): chu no dang doi thi vao ngay; qua 21:00 thi khong ai vao nua, het khach thi het ngay;
        /// con lai khach moi vao sau nextCustomerSeconds. <paramref name="offset"/>: ghe trong tu bay nhieu giay sau dau buoc Tick.
        /// </summary>
        void FreeSeat(int seat, float offset)
        {
            if (BossWaiting) SeatBoss(seat); // chu no dang doi ghe: vao ngay (muc 6.2 "chiem mot ghe")
            else if (IsClosedForNewCustomers)
            {
                seatTimers[seat] = -1f;
                if (CurrentCustomer == null && !AnyoneEating) EndDay();
            }
            else seatTimers[seat] = offset + Balance.day.nextCustomerSeconds;
        }

        /// <summary>Dong quan khi con khach: khach con lai ra ve, khong tra va khong phat.</summary>
        public bool CloseEarly()
        {
            if (Phase != GamePhase.Open) return false;
            EndDay();
            return true;
        }

        void EndDay()
        {
            ClosedAt = Clock.Text;
            for (int i = 0; i < seats.Length; i++) { seats[i] = null; seatTimers[i] = -1f; eaters[i] = null; eatingDishes[i] = null; }
            Stock.Clear();
            // Dia con tren quay cho vao tu lanh neu con o, khong thi bi bo (constitution muc 5.1, 5.7).
            foreach (Dish d in Counter.Dishes) Fridge.TryStore(d);
            Counter.Clear();
            Stove.Reset();
            Board.Cancel();
            LastBreakthrough = null;
            Phase = GamePhase.DayEnd;
        }

        // ---------- sang ngay ----------

        /// <summary>Sang ngay moi: thu no neu den han (dau ngay), roi phat ve.</summary>
        public bool StartNextDay()
        {
            if (Phase != GamePhase.DayEnd) return false;

            Day++;
            ResetDayState();
            Fridge.AgeOneDay();

            int due = Balance.DebtDueOn(Day);
            if (due > 0)
            {
                if (Money < due)
                {
                    Phase = GamePhase.Lost;
                    return true;
                }
                Money -= due;
                LastDebtPaid = due;
            }

            if (Day > LastPlayableDay)
            {
                Phase = GamePhase.Cleared;
                return true;
            }

            Wallet.BeginDay(Day, Relics.Effects);
            Shop.Restock(Day, Relics);
            BeginDayRules();
            Phase = GamePhase.Prep;
            return true;
        }

        void ResetDayState()
        {
            LastDebtPaid = 0;
            LastPayment = null;
            ServedToday = 0;
            CustomersToday = 0;
            IncomeToday = 0;
            ClosedAt = null;
            LastBreakthrough = null;
            BossArrived = false;
            BossServed = false;
            bossSeated = false;
            Clock.Reset();
        }

        bool Contains(Ingredient ingredient)
        {
            if (ingredient == null) return false;
            foreach (Ingredient i in Stock.Items)
                if (ReferenceEquals(i, ingredient)) return true;
            return false;
        }
    }

    /// <summary>
    /// Gio cua bot: thoi gian mot thao tac ngoai so che + nau trong du lieu. Day la GIA DINH cua mo phong
    /// (balance/sim_balance.py: PULL_S = 4, OVERHEAD naive = 30 giay; constitution 1.4 muc 9), khong phai luat game.
    /// </summary>
    public class BotTiming
    {
        /// <summary>Di toi may, nhet ve, thung roi.</summary>
        public float pullSeconds;

        /// <summary>Di lai trong phong, ngam, mang ra quay, bam chuong cho moi mon.</summary>
        public float overheadSeconds;

        /// <summary>Nguoi khong doc chi so (sim_balance.py OVERHEAD['naive']).</summary>
        public static readonly BotTiming Naive = new BotTiming { pullSeconds = 4f, overheadSeconds = 30f };

        /// <summary>Lay mon tu tu lanh mang ra.</summary>
        public float fridgeSeconds = 6f;

        /// <summary>Sprint 7 (F-060 AC-1): gio cua bot theo so do trong du lieu (balance.json bot_timing); thieu du lieu thi dung so cu.</summary>
        public static BotTiming From(Balance b, bool naive)
        {
            BotTimingData d = b != null ? b.botTiming : null;
            float over = d == null ? 0f : naive ? d.naiveOverheadSeconds : d.goodOverheadSeconds;
            return new BotTiming
            {
                pullSeconds = d != null && d.pullSeconds > 0f ? d.pullSeconds : 4f,
                overheadSeconds = over > 0f ? over : naive ? 30f : 20f,
                fridgeSeconds = d != null && d.fridgeSeconds > 0f ? d.fridgeSeconds : 6f,
            };
        }
    }

    /// <summary>
    /// Nguoi choi "khong doc chi so" (constitution Dieu 1): dung het ve, ghep nguyen lieu theo thu tu
    /// trong kho, phuc vu theo thu tu khach den. Nau dung cong thuc. Choi thang tren GameSession.
    /// Co <see cref="BotTiming"/> thi moi thao tac lam dong ho troi (F-011 AC-8); null = khong tinh gio.
    /// </summary>
    public static class NaiveBot
    {
        /// <summary>So khach phuc vu trong lan PlayToEnd gan nhat (de in thong ke).</summary>
        public static int LastRunServed { get; private set; }

        /// <summary>Choi den khi qua phan choi duoc hoac thua.</summary>
        /// <summary>So khach phuc vu tu vong 2 (co tinh gio cho) va tong so diem do de tinh bi tru vi cho, trong lan PlayToEnd gan nhat.</summary>
        public static int LastRunTimedServes { get; private set; }
        public static int LastRunDrop { get; private set; }

        /// <summary>So lan dong quan som vi khong khach nao lam duoc mon, va so lan phai bo qua khach den truoc.</summary>
        public static int LastRunEarlyCloses { get; private set; }
        public static int LastRunSkips { get; private set; }

        /// <summary>So khach phuc vu theo ngay trong lan PlayToEnd gan nhat (chi so 0 = ngay 1).</summary>
        public static readonly List<int> LastRunServedByDay = new List<int>();

        public static GamePhase PlayToEnd(GameSession s, BotTiming timing = null)
        {
            LastRunServed = 0;
            LastRunTimedServes = 0;
            LastRunDrop = 0;
            LastRunEarlyCloses = 0;
            LastRunSkips = 0;
            LastRunServedByDay.Clear();
            while (s.Phase != GamePhase.Cleared && s.Phase != GamePhase.Lost)
            {
                PlayDay(s, timing);
                LastRunServed += s.ServedToday;
                LastRunServedByDay.Add(s.ServedToday);
                s.StartNextDay();
            }
            return s.Phase;
        }

        /// <summary>Choi tron mot ngay, dung lai o pha DayEnd.</summary>
        public static void PlayDay(GameSession s, BotTiming timing = null)
        {
            if (s.Phase != GamePhase.Prep) return;

            BuyRelic(s);
            BuyBlackTickets(s);
            PullResult pull;
            while (s.PullCrates(out pull)) Spend(s, timing, timing == null ? 0f : timing.pullSeconds);
            while (s.PullBlack(out pull)) Spend(s, timing, timing == null ? 0f : timing.pullSeconds);

            s.OpenShop();
            while (s.Phase == GamePhase.Open)
            {
                if (s.CurrentCustomer == null)
                {
                    // ghe trong: cho khach ke tiep (hoac toi 21:00 thi het ngay)
                    s.Tick(s.Balance.day.nextCustomerSeconds);
                    continue;
                }
                // Phuc vu dung mon khach goi (constitution muc 6.7) theo thu tu khach den. Tu vong 2 nhieu khach cung goi
                // tu mot kho: mon cua khach dau khong con thi phuc vu khach ke tiep lam duoc; khong ai lam duoc thi dong quan.
                Customer target = null;
                int index = -1;
                var queue = new List<Customer>();
                foreach (Customer c in s.Seats) if (c != null) queue.Add(c);
                queue.Sort((a, b) => a.number.CompareTo(b.number));
                foreach (Customer c in queue)
                {
                    index = IndexOnCounter(s, c.orderId);
                    if (index < 0 && CookFor(s, c.orderId, timing)) index = IndexOnCounter(s, c.orderId);
                    if (index >= 0) { target = c; break; }
                }
                if (target == null)
                {
                    LastRunEarlyCloses++;
                    s.CloseEarly();
                    break;
                }
                if (target != queue[0]) LastRunSkips++;
                if (Patience.Applies(s.Balance, s.Day))
                {
                    // Do khoang an toan 90 giay (impl plan sprint 4 muc 5): khach mat bao nhieu diem luc duoc phuc vu.
                    LastRunTimedServes++;
                    LastRunDrop += target.easiness - s.EasinessNow(target);
                }
                s.ServeSeat(s.SeatOf(target), s.Counter.Dishes[index]);
            }
            // Khong mo duoc quan: dong quan luon (dong ho chi chay sau khi mo, constitution 1.7 muc 6.2).
            if (s.Phase == GamePhase.Prep && s.OpenShop()) s.CloseEarly();
        }

        /// <summary>
        /// Nguoi khong doc chi so mua relic nhu sim_balance.py run() (style naive): neu co relic mua noi thi
        /// 50% mua cai dat nhat; gia day thi bo cai re nhat (score naive = gia). Ti le 50% la GIA DINH cua mo phong.
        /// </summary>
        public const double BuyChance = 0.5;

        public static bool BuyRelic(GameSession s)
        {
            if (!s.Shop.IsOpen) return false;
            int best = -1;
            for (int i = 0; i < s.Shop.Offers.Count; i++)
            {
                RelicDef o = s.Shop.Offers[i];
                if (o == null || o.price > s.Wallet.Count) continue;
                if (best < 0 || o.price > s.Shop.Offers[best].price) best = i;
            }
            if (best < 0 || s.Rng.NextDouble() >= BuyChance) return false;

            string discard = null;
            if (s.Relics.IsFull)
            {
                RelicDef worst = null;
                foreach (RelicDef r in s.Relics.Owned) if (worst == null || r.price < worst.price) worst = r;
                discard = worst.id;
            }
            return s.BuyRelic(best, discard) == RelicBuyResult.Bought;
        }

        /// <summary>sim_balance.py run() style naive: tu ngay nay mua ve den neu tien con hon no sap toi x1,1 (GIA DINH cua mo phong).</summary>
        public const int BlackFromDay = 6;
        public const double BlackReserve = 1.1;

        /// <summary>Mua toi da mot ve den moi ngay nhu sim (naive dung het ve den trong ngay).</summary>
        public static bool BuyBlackTickets(GameSession s)
        {
            if (s.Day < BlackFromDay || !s.Items.IsOpen) return false;
            DebtEntry next = s.NextDebt();
            int reserve = next != null ? next.amount : 0;
            if (s.Money - s.Items.BlackPrice <= reserve * BlackReserve) return false;
            return s.BuyBlackTicket() == ItemBuyResult.Bought;
        }

        static void Spend(GameSession s, BotTiming timing, float seconds)
        {
            if (timing != null) s.Tick(seconds);
        }

        static int IndexOnCounter(GameSession s, string recipeId)
        {
            for (int i = 0; i < s.Counter.Dishes.Count; i++)
                if (s.Counter.Dishes[i].recipeId == recipeId) return i;
            return -1;
        }

        /// <summary>Nau mot mon tu cap nguyen lieu dau tien trong kho, dung cong thuc. False neu khong du do.</summary>
        public static bool CookOne(GameSession s)
        {
            if (s.Counter.IsFull || s.Stove.State != StoveState.Empty) return false;
            if (s.Stock.Count == 0)
            {
                PullResult pull;
                if (!s.PullCrates(out pull)) return false;
            }

            Ingredient main = null, side = null;
            foreach (Ingredient i in s.Stock.Items)
            {
                if (main == null && i.kind == IngredientKind.Main) main = i;
                if (side == null && i.kind == IngredientKind.Side) side = i;
            }
            if (main == null || side == null) return false;
            return Cook(s, main, side);
        }

        /// <summary>Nau dung mon co id nay tu kho (quay them thung neu thieu). False neu khong du do.</summary>
        public static bool CookFor(GameSession s, string recipeId, BotTiming timing = null)
        {
            Recipe want = s.Lookup.ById(recipeId);
            if (want == null || s.Counter.IsFull || s.Stove.State != StoveState.Empty) return false;
            while (true)
            {
                Ingredient main = null, side = null;
                foreach (Ingredient i in s.Stock.Items)
                {
                    if (main == null && i.kind == IngredientKind.Main && i.typeId == want.mainId) main = i;
                    if (side == null && i.kind == IngredientKind.Side && i.typeId == want.sideId) side = i;
                }
                if (main != null && side != null) return Cook(s, main, side, timing);
                PullResult pull;
                if (!s.PullCrates(out pull)) return false;
                Spend(s, timing, timing == null ? 0f : timing.pullSeconds);
            }
        }

        static bool Cook(GameSession s, Ingredient main, Ingredient side, BotTiming timing = null)
        {
            Recipe recipe = s.Lookup.Find(main.typeId, side.typeId);
            if (recipe == null) return false;

            if (!Prepare(s, main) || !Prepare(s, side)) return false;
            if (!s.Stove.SelectTool(recipe.tool)) return false;

            foreach (string step in recipe.steps)
            {
                bool ok;
                if (step == main.name) ok = s.AddToStove(main);
                else if (step == side.name) ok = s.AddToStove(side);
                else ok = s.Stove.AddPantry(step);
                if (!ok) return false;
            }

            if (!s.Stove.Start()) return false;
            s.Stove.Tick(s.Stove.CookSeconds);
            Dish dish;
            if (!s.PlateDish(out dish)) return false;

            if (timing != null)
            {
                // Giong sim_balance.dish_seconds: so che + cho vao + nau + muc + di lai.
                CookTiming t = s.Recipes.timing;
                s.Tick(main.prepSeconds + side.prepSeconds + t.stepSeconds * recipe.steps.Length
                    + recipe.cookSeconds + t.plateSeconds + timing.overheadSeconds);
            }
            return true;
        }

        static bool Prepare(GameSession s, Ingredient ingredient)
        {
            if (ingredient.prepared) return true;
            if (!s.BeginPrep(ingredient)) return false;
            s.HoldPrep(ingredient.prepSeconds);
            return ingredient.prepared;
        }
    }
}
