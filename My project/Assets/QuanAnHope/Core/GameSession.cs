using System;

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
            CurrentCustomer = customers.Next(Round);
            return true;
        }

        /// <summary>Bam chuong: dua dia o vi tri nay tren quay cho cho khach dang doi. Null neu khong phuc vu duoc.</summary>
        public PaymentResult Serve(int counterIndex)
        {
            if (Phase != GamePhase.Open || CurrentCustomer == null) return null;
            if (counterIndex < 0 || counterIndex >= Counter.Dishes.Count) return null;

            Dish dish = Counter.TakeAt(counterIndex);
            PaymentResult payment = Payment.Settle(Balance, Round, CurrentCustomer.easiness, dish.tier);
            Money += payment.amount;
            IncomeToday += payment.amount;
            LastPayment = payment;
            ServedToday++;

            if (ServedToday >= CustomersToday) EndDay();
            else CurrentCustomer = customers.Next(Round);
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
                if (s.Counter.Dishes.Count == 0 && !CookOne(s))
                {
                    s.CloseEarly();
                    break;
                }
                s.Serve(0);
            }
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
