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

        /// <summary>Ruong dot pha (Art Bible muc 7: nap vom tim vien vang).</summary>
        public bool isBreakthrough;
    }

    public class PullResult
    {
        public Crate side;
        public Crate main;

        /// <summary>Thung dot pha (constitution muc 4.4); null neu luot nay khong trung.</summary>
        public Crate breakthrough;
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
        public void BeginDay(int day, RelicFx fx = null)
        {
            if (day == 1) Count = balance.ticketsPerDay + balance.start.bonusTickets;
            else Count += balance.ticketsPerDay;
            // Ve so cao: +1 ve vao ngay chia het cho 3 (constitution muc 7.3; sim_balance.py: day % 3 == 0).
            if (fx != null && day % 3 == 0) Count += (int)fx.Get(RelicKeys.TicketEvery3);
        }

        public bool TrySpend(int count = 1)
        {
            if (count <= 0 || Count < count) return false;
            Count -= count;
            return true;
        }
    }

    /// <summary>May tha thung nguyen lieu (constitution muc 4.2, 4.3, 4.5).</summary>
    public class IngredientGacha
    {
        const string MainGroup = "chinh";
        const string BreakthroughGroup = "dotpha";

        // Bac cua thang bac nguyen lieu (constitution muc 4.1): khong phai so can bang.
        const int TopTier = 5;
        const int JumpTier = 4;
        const int BreakthroughLowTier = 4;

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
        public bool TryPull(TicketWallet wallet, int day, out PullResult result, RelicFx fx = null)
        {
            result = null;
            if (!wallet.TrySpend()) return false;
            result = Pull(day, fx);
            return true;
        }

        /// <summary>Tha 1 thung do phu + 1 thung nguyen lieu chinh theo bang cua ngay.</summary>
        public PullResult Pull(int day, RelicFx fx = null)
        {
            if (fx == null) fx = RelicFx.None;
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
            int sideCount = CrateCount(balance.SideCrateSize(day), fx, IngredientKind.Side);
            for (int i = 0; i < sideCount; i++)
                sideCrate.items.Add(Ingredient.FromSide(sideType, RollTier(odds, fx, IngredientKind.Side)));

            MainIngredient mainType = mains[rng.NextInt(0, mains.Count - 1)];
            var mainCrate = new Crate { kind = IngredientKind.Main, typeId = mainType.id, name = mainType.name };
            int mainCount = CrateCount(balance.MainCrateSize(day), fx, IngredientKind.Main);
            for (int i = 0; i < mainCount; i++)
                mainCrate.items.Add(Ingredient.FromMain(mainType, RollTier(odds, fx, IngredientKind.Main)));

            return new PullResult { side = sideCrate, main = mainCrate, breakthrough = RollBreakthrough(fx) };
        }

        /// <summary>Ti le ra thung dot pha moi luot (constitution muc 4.4): co ban + tong bt_chance.</summary>
        public double BreakthroughChance(RelicFx fx)
        {
            return balance.breakthrough.baseChance + (fx ?? RelicFx.None).Get(RelicKeys.BtChance);
        }

        /// <summary>Ti le mot mon dot pha ra bac 5 (constitution muc 4.4, 7.2).</summary>
        public double BreakthroughTier5(RelicFx fx)
        {
            return Math.Min(1.0, balance.breakthrough.tier5Share + (double)(fx ?? RelicFx.None).Get(RelicKeys.BtT5));
        }

        public int BreakthroughItems(RelicFx fx)
        {
            return balance.breakthrough.items + (int)(fx ?? RelicFx.None).Get(RelicKeys.BtItems);
        }

        Crate RollBreakthrough(RelicFx fx)
        {
            double chance = BreakthroughChance(fx);
            // Khong co relic: khong quay so, chuoi ngau nhien giong het ban truoc (F-022 AC-1).
            if (chance <= 0.0 || rng.NextDouble() >= chance) return null;

            var types = new List<MainIngredient>();
            foreach (MainIngredient m in recipes.mains) if (m.group == BreakthroughGroup) types.Add(m);
            if (types.Count == 0) return null;
            MainIngredient type = types[rng.NextInt(0, types.Count - 1)];
            var crate = new Crate { kind = IngredientKind.Main, typeId = type.id, name = type.name, isBreakthrough = true };
            double t5 = BreakthroughTier5(fx);
            int n = BreakthroughItems(fx);
            for (int i = 0; i < n; i++)
                crate.items.Add(Ingredient.FromMain(type, t5 >= 1.0 || rng.NextDouble() < t5 ? TopTier : BreakthroughLowTier));
            return crate;
        }

        /// <summary>Co thung sau relic (Ro tre, Thuyen thue, Moi quen cho ca), toi thieu 1 mon (sim_balance.py).</summary>
        public static int CrateCount(int baseCount, RelicFx fx, IngredientKind kind)
        {
            if (fx == null) fx = RelicFx.None;
            return Math.Max(1, baseCount + (int)Math.Round(fx.Get(kind, RelicKeys.Size)));
        }

        /// <summary>
        /// Ti le bac (0..1) cua mot mon sau relic, tinh dung theo thu tu cua RollTier
        /// (bang ti le Goc 2 hien tac dung relic, constitution Dieu 4; F-021 AC-5).
        /// </summary>
        public static double[] EffectiveOdds(int[] percents, RelicFx fx, IngredientKind kind)
        {
            if (fx == null) fx = RelicFx.None;
            int n = TopTier + 1;
            double total = 0;
            foreach (int x in percents) total += x;
            var p = new double[n];
            for (int t = 0; t < percents.Length && t < n; t++) p[t] = percents[t] / total;

            if (fx.Get(kind, RelicKeys.Reroll0) > 0f)
            {
                double p0 = p[0];
                var q = new double[n];
                for (int t = 0; t < n; t++) q[t] = t == 0 ? p0 * p0 : p[t] * (1 + p0);
                p = q;
            }
            double up = fx.Get(kind, RelicKeys.Up);
            if (up > 0)
            {
                var q = new double[n];
                for (int t = 0; t < n; t++) { q[t] += p[t] * (1 - up); q[Math.Min(TopTier, t + 1)] += p[t] * up; }
                p = q;
            }
            double jump = fx.Get(kind, RelicKeys.Jump4);
            if (jump > 0)
            {
                var q = new double[n];
                for (int t = 0; t < n; t++)
                    if (t < JumpTier) { q[t] += p[t] * (1 - jump); q[JumpTier] += p[t] * jump; }
                    else q[t] += p[t];
                p = q;
            }
            int min = (int)fx.Get(kind, RelicKeys.Min);
            int plus = (int)fx.Get(kind, RelicKeys.Plus);
            var r = new double[n];
            for (int t = 0; t < n; t++) r[Math.Min(TopTier, Math.Max(t, min) + plus)] += p[t];
            return r;
        }

        /// <summary>
        /// Bac mot mon trong thung, theo thu tu cua sim_balance.py pull(): quay -> quay lai neu bac 0 -> x% +1 bac
        /// -> x% nhay bac 4 -> bac toi thieu -> cong bac; toi da bac 5. Hieu ung bang 0 thi khong quay them so.
        /// </summary>
        int RollTier(int[] odds, RelicFx fx, IngredientKind kind)
        {
            int t = TierRoller.Roll(odds, rng);
            if (t == 0 && fx.Get(kind, RelicKeys.Reroll0) > 0f) t = TierRoller.Roll(odds, rng);
            float up = fx.Get(kind, RelicKeys.Up);
            if (up > 0f && rng.NextDouble() < up) t = Math.Min(TopTier, t + 1);
            float jump = fx.Get(kind, RelicKeys.Jump4);
            if (jump > 0f && rng.NextDouble() < jump) t = Math.Max(t, JumpTier);
            t = Math.Max(t, (int)fx.Get(kind, RelicKeys.Min));
            t = Math.Min(TopTier, t + (int)fx.Get(kind, RelicKeys.Plus));
            return t;
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
