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
    public class GameSession
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

        public Customer CurrentCustomer { get; private set; }

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

        /// <summary>Giay con lai truoc khi khach ke tiep vao; am khi khong co ai sap vao.</summary>
        float nextCustomerIn = -1f;

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

            Day = 1;
            Money = balance.start.money;
            Wallet.BeginDay(Day, Relics.Effects);
            Shop.Restock(Day, Relics);
            Phase = GamePhase.Prep;
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
            SeatNextCustomer();
            return true;
        }

        /// <summary>
        /// Thoi gian troi (constitution 1.4 muc 2, 6.2): dong ho chay tu dau ngay; khi quan mo va ghe trong,
        /// khach ke tiep vao sau <see cref="DayRules.nextCustomerSeconds"/> giay; toi 21:00 khong nhan khach moi,
        /// khong con ai ngoi thi het ngay.
        /// </summary>
        public void Tick(float seconds)
        {
            if (!DayIsRunning || seconds <= 0f) return;

            float before = Clock.Elapsed;
            Clock.Advance(seconds);

            if (Phase == GamePhase.Open && CurrentCustomer == null && nextCustomerIn >= 0f)
            {
                float arrivesAt = before + nextCustomerIn;
                nextCustomerIn -= seconds;
                if (nextCustomerIn <= 0f && arrivesAt < Clock.DaySeconds) SeatNextCustomer();
            }

            if (IsClosedForNewCustomers && CurrentCustomer == null) EndDay();
        }

        void SeatNextCustomer()
        {
            nextCustomerIn = -1f;
            CurrentCustomer = NextCustomer();
            CustomersToday++;
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

        /// <summary>Bam chuong: dua dia o vi tri nay tren quay cho cho khach dang doi. Null neu khong phuc vu duoc.</summary>
        public PaymentResult Serve(int counterIndex)
        {
            if (Phase != GamePhase.Open || CurrentCustomer == null) return null;
            if (counterIndex < 0 || counterIndex >= Counter.Dishes.Count) return null;

            Dish dish = Counter.TakeAt(counterIndex);
            // Sai mon: khach nhan dia nhung gian (constitution muc 6.7).
            PaymentResult payment = dish.recipeId == CurrentCustomer.orderId
                ? Payment.Settle(Balance, Round, CurrentCustomer.easiness, dish.tier, Relics.Effects)
                : Payment.WrongOrder(Balance, Round);
            Money += payment.amount;
            IncomeToday += payment.amount;
            LastPayment = payment;
            ServedToday++;

            // Khach ke tiep vao sau vai giay; qua 21:00 thi khach vua roi la nguoi cuoi (muc 6.2).
            CurrentCustomer = null;
            if (IsClosedForNewCustomers) EndDay();
            else nextCustomerIn = Balance.day.nextCustomerSeconds;
            return payment;
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
            nextCustomerIn = -1f;
            CurrentCustomer = null;
            Stock.Clear();
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
            LastDebtPaid = 0;
            LastPayment = null;
            ServedToday = 0;
            CustomersToday = 0;
            IncomeToday = 0;
            ClosedAt = null;
            nextCustomerIn = -1f;
            Clock.Reset();

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
            Phase = GamePhase.Prep;
            return true;
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
        public static GamePhase PlayToEnd(GameSession s, BotTiming timing = null)
        {
            LastRunServed = 0;
            while (s.Phase != GamePhase.Cleared && s.Phase != GamePhase.Lost)
            {
                PlayDay(s, timing);
                LastRunServed += s.ServedToday;
                s.StartNextDay();
            }
            return s.Phase;
        }

        /// <summary>Choi tron mot ngay, dung lai o pha DayEnd.</summary>
        public static void PlayDay(GameSession s, BotTiming timing = null)
        {
            if (s.Phase != GamePhase.Prep) return;

            BuyRelic(s);
            PullResult pull;
            while (s.PullCrates(out pull)) Spend(s, timing, timing == null ? 0f : timing.pullSeconds);

            s.OpenShop();
            while (s.Phase == GamePhase.Open)
            {
                if (s.CurrentCustomer == null)
                {
                    // ghe trong: cho khach ke tiep (hoac toi 21:00 thi het ngay)
                    s.Tick(s.Balance.day.nextCustomerSeconds);
                    continue;
                }
                // Phuc vu dung mon khach goi (constitution muc 6.7); khong nau duoc thi dong quan.
                string want = s.CurrentCustomer.orderId;
                int index = IndexOnCounter(s, want);
                if (index < 0 && CookFor(s, want, timing)) index = IndexOnCounter(s, want);
                if (index < 0)
                {
                    s.CloseEarly();
                    break;
                }
                s.Serve(index);
            }
            // Khong mo duoc quan (vd da qua 21:00): cho het gio.
            if (s.Phase == GamePhase.Prep) s.Tick(s.Clock.DaySeconds);
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
