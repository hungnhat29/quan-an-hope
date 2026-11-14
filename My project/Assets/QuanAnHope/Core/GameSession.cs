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

        /// <summary>Het khach (hoac dong quan som): cho sang ngay moi.</summary>
        DayEnd,

        /// <summary>Da tra no va di het phan choi duoc cua ban nay.</summary>
        Cleared,

        /// <summary>Khong du tien tra no dung han.</summary>
        Lost,
    }

    /// <summary>
    /// Mot van choi: ngay, tien, ve, kho, bep, quay cho, khach (constitution muc 2, 3, 4.2, 5.1, 6).
    /// Lop nay khong biet gi ve Unity; thoi gian di vao qua HoldPrep / Stove.Tick.
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

        public int Day { get; private set; }
        public int Money { get; private set; }
        public GamePhase Phase { get; private set; }

        /// <summary>Ngay cuoi cung choi duoc trong ban nay; qua ngay do (sau khi tra no) la Cleared.</summary>
        public int LastPlayableDay { get; private set; }

        public Customer CurrentCustomer { get; private set; }
        public int CustomersToday { get; private set; }
        public int ServedToday { get; private set; }
        public int IncomeToday { get; private set; }
        public PaymentResult LastPayment { get; private set; }

        /// <summary>So tien vua bi thu no khi sang ngay hien tai (0 neu khong den han).</summary>
        public int LastDebtPaid { get; private set; }

        public int Round { get { return Balance.RoundOfDay(Day); } }

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
            gacha = new IngredientGacha(balance, recipes, rng);
            customers = new CustomerGenerator(balance, rng);

            Wallet = new TicketWallet(balance);
            Stock = new IngredientStock();
            Board = new PrepStation();
            Stove = new Stove(Lookup, Formula);
            Counter = new Counter(balance);

            Day = 1;
            Money = balance.start.money;
            Wallet.BeginDay(Day);
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
            if (!gacha.TryPull(Wallet, Day, out result)) return false;
            Stock.AddCrate(result.side);
            Stock.AddCrate(result.main);
            return true;
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
            if (Phase != GamePhase.Prep) return false;
            CustomersToday = Balance.CustomersPerDay(Day);
            ServedToday = 0;
            Phase = GamePhase.Open;
            CurrentCustomer = NextCustomer();
            return true;
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
                ? Payment.Settle(Balance, Round, CurrentCustomer.easiness, dish.tier)
                : Payment.WrongOrder(Balance, Round);
            Money += payment.amount;
            IncomeToday += payment.amount;
            LastPayment = payment;
            ServedToday++;

            if (ServedToday >= CustomersToday) EndDay();
            else CurrentCustomer = NextCustomer();
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
            CurrentCustomer = null;
            Stock.Clear();
            Counter.Clear();
            Stove.Reset();
            Board.Cancel();
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

            Wallet.BeginDay(Day);
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
    /// Nguoi choi "khong doc chi so" (constitution Dieu 1): dung het ve, ghep nguyen lieu theo thu tu
    /// trong kho, phuc vu theo thu tu khach den. Nau dung cong thuc. Choi thang tren GameSession.
    /// </summary>
    public static class NaiveBot
    {
        /// <summary>Choi den khi qua phan choi duoc hoac thua.</summary>
        public static GamePhase PlayToEnd(GameSession s)
        {
            while (s.Phase != GamePhase.Cleared && s.Phase != GamePhase.Lost)
            {
                PlayDay(s);
                s.StartNextDay();
            }
            return s.Phase;
        }

        /// <summary>Choi tron mot ngay, dung lai o pha DayEnd.</summary>
        public static void PlayDay(GameSession s)
        {
            if (s.Phase != GamePhase.Prep) return;

            PullResult pull;
            while (s.PullCrates(out pull)) { }

            s.OpenShop();
            while (s.Phase == GamePhase.Open)
            {
                // Phuc vu dung mon khach goi (constitution muc 6.7); khong nau duoc thi dong quan.
                string want = s.CurrentCustomer.orderId;
                int index = IndexOnCounter(s, want);
                if (index < 0 && CookFor(s, want)) index = IndexOnCounter(s, want);
                if (index < 0)
                {
                    s.CloseEarly();
                    break;
                }
                s.Serve(index);
            }
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
        public static bool CookFor(GameSession s, string recipeId)
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
                if (main != null && side != null) return Cook(s, main, side);
                PullResult pull;
                if (!s.PullCrates(out pull)) return false;
            }
        }

        static bool Cook(GameSession s, Ingredient main, Ingredient side)
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
            return s.PlateDish(out dish);
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
