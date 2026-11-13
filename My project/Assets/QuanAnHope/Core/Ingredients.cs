using System;
using System.Collections.Generic;

namespace QuanAnHope.Core
{
    public enum IngredientKind
    {
        /// <summary>Do phu (rau, cu, qua).</summary>
        Side,

        /// <summary>Nguyen lieu chinh, ke ca dot pha.</summary>
        Main,
    }

    /// <summary>Mot mon nguyen lieu trong thung: loai, bac, da so che hay chua.</summary>
    public class Ingredient
    {
        public string typeId;
        public string name;
        public IngredientKind kind;
        public int tier;
        public float prepSeconds;
        public bool prepared;

        public static Ingredient FromSide(SideIngredient data, int tier)
        {
            return new Ingredient
            {
                typeId = data.id,
                name = data.name,
                kind = IngredientKind.Side,
                tier = tier,
                prepSeconds = data.prepSeconds,
            };
        }

        public static Ingredient FromMain(MainIngredient data, int tier)
        {
            return new Ingredient
            {
                typeId = data.id,
                name = data.name,
                kind = IngredientKind.Main,
                tier = tier,
                prepSeconds = data.prepSeconds,
            };
        }
    }

    /// <summary>Mot thung: chi mot loai nguyen lieu, moi mon co bac rieng (constitution muc 4.2).</summary>
    public class Crate
    {
        public IngredientKind kind;
        public string typeId;
        public string name;
        public List<Ingredient> items = new List<Ingredient>();
    }

    public class PullResult
    {
        public Crate side;
        public Crate main;
    }

    /// <summary>So ve trang cua nguoi choi (constitution muc 3).</summary>
    public class TicketWallet
    {
        readonly Balance balance;

        public int Count { get; private set; }

        public TicketWallet(Balance balance)
        {
            this.balance = balance;
        }

        /// <summary>Goi dau moi ngay: ngay 1 nhan ve ngay + ve thuong; cac ngay sau cong don.</summary>
        public void BeginDay(int day)
        {
            if (day == 1) Count = balance.ticketsPerDay + balance.start.bonusTickets;
            else Count += balance.ticketsPerDay;
        }

        public bool TrySpend()
        {
            if (Count <= 0) return false;
            Count--;
            return true;
        }
    }

    /// <summary>May tha thung nguyen lieu (constitution muc 4.2, 4.3, 4.5).</summary>
    public class IngredientGacha
    {
        const string MainGroup = "chinh";

        readonly Balance balance;
        readonly RecipeBook recipes;
        readonly IRandom rng;

        public IngredientGacha(Balance balance, RecipeBook recipes, IRandom rng)
        {
            this.balance = balance;
            this.recipes = recipes;
            this.rng = rng;
        }

        /// <summary>Tieu 1 ve va tha thung. Het ve thi tra ve false, khong tha gi.</summary>
        public bool TryPull(TicketWallet wallet, int day, out PullResult result)
        {
            result = null;
            if (!wallet.TrySpend()) return false;
            result = Pull(day);
            return true;
        }

        /// <summary>Tha 1 thung do phu + 1 thung nguyen lieu chinh theo bang cua ngay.</summary>
        public PullResult Pull(int day)
        {
            int round = balance.RoundOfDay(day);
            int[] odds = balance.OddsForDay(day).percents;

            var sides = new List<SideIngredient>();
            foreach (SideIngredient s in recipes.sides)
                if (IsUnlocked(s.unlockRound, round)) sides.Add(s);

            var mains = new List<MainIngredient>();
            foreach (MainIngredient m in recipes.mains)
                if (m.group == MainGroup && IsUnlocked(m.unlockRound, round)) mains.Add(m);

            if (sides.Count == 0 || mains.Count == 0)
                throw new InvalidOperationException("Vong " + round + " chua mo khoa nguyen lieu nao");

            SideIngredient sideType = sides[rng.NextInt(0, sides.Count - 1)];
            var sideCrate = new Crate { kind = IngredientKind.Side, typeId = sideType.id, name = sideType.name };
            int sideCount = balance.SideCrateSize(day);
            for (int i = 0; i < sideCount; i++)
                sideCrate.items.Add(Ingredient.FromSide(sideType, TierRoller.Roll(odds, rng)));

            MainIngredient mainType = mains[rng.NextInt(0, mains.Count - 1)];
            var mainCrate = new Crate { kind = IngredientKind.Main, typeId = mainType.id, name = mainType.name };
            int mainCount = balance.MainCrateSize(day);
            for (int i = 0; i < mainCount; i++)
                mainCrate.items.Add(Ingredient.FromMain(mainType, TierRoller.Roll(odds, rng)));

            return new PullResult { side = sideCrate, main = mainCrate };
        }

        static bool IsUnlocked(int unlockRound, int round)
        {
            return unlockRound >= 1 && unlockRound <= round;
        }
    }

    /// <summary>Nguyen lieu dang co trong ngay; cuoi ngay bo het (constitution muc 4.2).</summary>
    public class IngredientStock
    {
        readonly List<Ingredient> items = new List<Ingredient>();

        public IReadOnlyList<Ingredient> Items { get { return items; } }
        public int Count { get { return items.Count; } }

        public void AddCrate(Crate crate)
        {
            items.AddRange(crate.items);
        }

        public bool Remove(Ingredient ingredient)
        {
            return items.Remove(ingredient);
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}
