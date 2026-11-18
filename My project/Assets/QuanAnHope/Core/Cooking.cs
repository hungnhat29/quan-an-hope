using System;
using System.Collections.Generic;

namespace QuanAnHope.Core
{
    /// <summary>Mot o tren thanh ti le bac mon: bac va xac suat (0..1).</summary>
    public struct TierChance
    {
        public int tier;
        public double chance;
    }

    /// <summary>
    /// Cong thuc ra bac mon (constitution muc 5.3):
    /// b = wChinh x bac chinh + wPhu x bac phu + offset; mon ra floor(b) hoac floor(b)+1,
    /// xac suat len = phan le cua b; kep trong [0, maxTier].
    /// </summary>
    public class DishFormula
    {
        const int ScoreDecimals = 4; // lam tron de 4,0 khong thanh 3,9999998 do sai so so thuc

        readonly DishRules rules;

        public DishFormula(Balance balance)
        {
            rules = balance.dish;
        }

        /// <summary>Gia relic cua van choi (Muoi hong, Ot ma, Chao gang; constitution muc 7.3). Null = khong co relic.</summary>
        public RelicInventory Relics { get; set; }

        RelicFx Fx { get { return Relics != null ? Relics.Effects : RelicFx.None; } }

        /// <summary>Ngay mat dien: diem mon tru blackoutPenalty (constitution muc 5.3).</summary>
        public bool Blackout { get; set; }

        public double Score(int mainTier, int sideTier)
        {
            double b = (double)rules.weightChinh * mainTier + (double)rules.weightPhu * sideTier + rules.offset
                + Fx.Get(RelicKeys.DishCenter) - (Blackout ? (double)rules.blackoutPenalty : 0.0);
            return Math.Round(b, ScoreDecimals);
        }

        /// <summary>Bac thap nhat mon co the ra (Chao gang ba ngoai: khong duoi Do).</summary>
        public int FloorTier { get { return Math.Min(rules.maxTier, (int)Fx.Get(RelicKeys.DishFloor)); } }

        /// <summary>Bac sau san cua relic (ap ca cho mon sai cong thuc, Q-29).</summary>
        public int ApplyFloor(int tier)
        {
            return Math.Max(tier, FloorTier);
        }

        /// <summary>Thanh ti le tu cong thuc muc 5.3 (truoc Ot ma va Chao gang): 1 o hoac 2 o.</summary>
        TierChance[] BaseBar(int mainTier, int sideTier)
        {
            double b = Math.Max(0.0, Math.Min(rules.maxTier, Score(mainTier, sideTier)));
            int low = (int)Math.Floor(b);
            double up = Math.Round(b - low, ScoreDecimals);

            if (low >= rules.maxTier || up <= 0.0)
                return new[] { new TierChance { tier = low, chance = 1.0 } };

            return new[]
            {
                new TierChance { tier = low, chance = 1.0 - up },
                new TierChance { tier = low + 1, chance = up },
            };
        }

        /// <summary>
        /// Thanh ti le hien cho nguoi choi (constitution Dieu 4): 1 o (chac chan) hoac 2 o (bac duoi, bac tren);
        /// co Ot ma thi them o Lac vi, co Chao gang thi bac duoi Do gop vao Do.
        /// </summary>
        public TierChance[] Bar(int mainTier, int sideTier)
        {
            TierChance[] bar = BaseBar(mainTier, sideTier);
            double ruin = Fx.Get(RelicKeys.DishRuin);
            int floor = FloorTier;
            if (ruin <= 0.0 && floor <= 0) return bar;

            var pct = new SortedDictionary<int, double>();
            foreach (TierChance c in bar) AddTo(pct, ApplyFloor(c.tier), c.chance * (1.0 - ruin));
            if (ruin > 0.0) AddTo(pct, ApplyFloor(0), ruin);
            var list = new List<TierChance>();
            foreach (KeyValuePair<int, double> kv in pct)
                list.Add(new TierChance { tier = kv.Key, chance = Math.Round(kv.Value, ScoreDecimals) });
            return list.ToArray();
        }

        static void AddTo(SortedDictionary<int, double> pct, int tier, double chance)
        {
            double cur;
            pct.TryGetValue(tier, out cur);
            pct[tier] = cur + chance;
        }

        /// <summary>Quay bac mon theo thu tu cua sim_balance.py cook(): cong thuc -> Ot ma -> Chao gang.</summary>
        public int Roll(int mainTier, int sideTier, IRandom rng)
        {
            TierChance[] bar = BaseBar(mainTier, sideTier);
            int t = bar.Length == 1 ? bar[0].tier : (rng.NextDouble() < bar[1].chance ? bar[1].tier : bar[0].tier);
            double ruin = Fx.Get(RelicKeys.DishRuin);
            if (ruin > 0.0 && rng.NextDouble() < ruin) t = 0;
            return ApplyFloor(t);
        }
    }

    /// <summary>Tra cong thuc tu cap nguyen lieu chinh + do phu (constitution muc 5.8).</summary>
    public class RecipeLookup
    {
        const string IdSeparator = "__";

        readonly Dictionary<string, Recipe> byId = new Dictionary<string, Recipe>();

        public RecipeLookup(RecipeBook book)
        {
            foreach (Recipe r in book.recipes) byId[r.id] = r;
        }

        /// <summary>Tra ve null neu cap nay khong co cong thuc.</summary>
        public Recipe Find(string mainId, string sideId)
        {
            Recipe r;
            return byId.TryGetValue(mainId + IdSeparator + sideId, out r) ? r : null;
        }

        /// <summary>Cong thuc theo id ("chinh__phu"); null neu khong co.</summary>
        public Recipe ById(string id)
        {
            Recipe r;
            return id != null && byId.TryGetValue(id, out r) ? r : null;
        }
    }

    /// <summary>Thot: giu du thoi gian so che cua nguyen lieu thi xong (constitution muc 4.5, 5.1).</summary>
    public class PrepStation
    {
        float held;

        public Ingredient Current { get; private set; }

        /// <summary>Tien do 0..1 cua nguyen lieu dang so che.</summary>
        public double Progress
        {
            get
            {
                if (Current == null) return 0.0;
                if (Current.prepSeconds <= 0f) return 1.0;
                return Math.Min(1.0, held / Current.prepSeconds);
            }
        }

        public bool Begin(Ingredient ingredient)
        {
            if (Current != null || ingredient == null || ingredient.prepared) return false;
            Current = ingredient;
            held = 0f;
            return true;
        }

        /// <summary>Cong them thoi gian giu. Tra ve true dung luc so che xong.</summary>
        public bool Hold(float seconds)
        {
            if (Current == null) return false;
            held += seconds;
            if (held + 1e-4f < Current.prepSeconds) return false;
            Current.prepared = true;
            Current = null;
            held = 0f;
            return true;
        }

        public void Cancel()
        {
            Current = null;
            held = 0f;
        }
    }

    /// <summary>Mot dia mon da nau.</summary>
    public class Dish
    {
        public string recipeId;
        public string name;
        public string baseName;
        public int tier;

        /// <summary>Da cho vao may gacha do an (moi mon 1 lan, constitution muc 5.6).</summary>
        public bool machined;

        /// <summary>So ngay nam trong tu lanh va da bi tut bac chua (constitution muc 5.7).</summary>
        public int daysInFridge;
        public bool decayed;

        /// <summary>Ten tam cua mon Huyen thoai duy nhat (constitution muc 5.4).</summary>
        public const string LegendName = "Món Hy Vọng";

        /// <summary>Bac Huyen thoai (bac cao nhat cua bang he so gia, constitution muc 5.2).</summary>
        public const int LegendTier = 5;

        /// <summary>Mon Hy Vong: chi ra qua nuoc sot tu ngay 9 (muc 5.4, 5.5). Van giu cong thuc goc de khach goi mon do nhan.</summary>
        public bool IsLegend { get { return tier >= LegendTier; } }

        /// <summary>Ten hien tren dia: Mon Hy Vong neu la Huyen thoai, khong thi ten mon.</summary>
        public string Title { get { return IsLegend ? LegendName : name; } }
    }

    /// <summary>Quay cho: giu mot so dia da nau trong ngay, khong giam bac (constitution muc 5.1).</summary>
    public class Counter
    {
        readonly List<Dish> dishes = new List<Dish>();

        public int Capacity { get; private set; }
        public IReadOnlyList<Dish> Dishes { get { return dishes; } }
        public bool IsFull { get { return dishes.Count >= Capacity; } }

        public Counter(Balance balance)
        {
            Capacity = balance.counterSlots;
        }

        public bool TryPlace(Dish dish)
        {
            if (dish == null || IsFull) return false;
            dishes.Add(dish);
            return true;
        }

        public Dish TakeAt(int index)
        {
            Dish d = dishes[index];
            dishes.RemoveAt(index);
            return d;
        }

        public void Clear()
        {
            dishes.Clear();
        }
    }

    public enum StoveState
    {
        Empty,
        Filling,
        Cooking,
        Ready,
    }

    /// <summary>
    /// Bep: chon dung cu, cho tung thu vao theo thu tu, cho nau, muc ra dia (constitution muc 5.1, 5.8).
    /// Sai thu tu (ke ca thieu, thua do kho) hoac sai dung cu thi mon ra Lac vi.
    /// Vong 1 khong co qua lua; qua lua them o sprint 3.
    /// </summary>
    public class Stove
    {
        const int SpoiledTier = 0;

        readonly RecipeLookup lookup;
        readonly DishFormula formula;
        readonly List<string> added = new List<string>();

        Ingredient main;
        Ingredient side;
        bool followsRecipe;

        public StoveState State { get; private set; }
        public string Tool { get; private set; }
        public Recipe Recipe { get; private set; }
        public float Elapsed { get; private set; }

        /// <summary>He so thoi gian nau cua ngay (ngay mat dien x1,25; constitution muc 5.9). Mac dinh 1.</summary>
        public float CookMultiplier { get; set; }

        /// <summary>Tu vong 2 co qua lua (constitution muc 5.9).</summary>
        public bool BurnEnabled { get; set; }

        public float CookSeconds { get { return Recipe != null ? Recipe.cookSeconds * CookMultiplier : 0f; } }
        public float WarnSeconds { get { return Recipe != null ? Recipe.warnSeconds * CookMultiplier : 0f; } }
        public float BurnSeconds { get { return Recipe != null ? Recipe.burnSeconds * CookMultiplier : 0f; } }

        /// <summary>Mon chin de tren bep qua 1,5 lan thoi gian nau: hien canh bao (khoi, bang bep doi mau).</summary>
        public bool IsOverheating { get { return BurnEnabled && State == StoveState.Ready && Elapsed + 1e-4f >= WarnSeconds; } }

        /// <summary>Qua 2 lan thoi gian nau: mon muc ra bi tru 1 bac.</summary>
        public bool IsBurnt { get { return BurnEnabled && State == StoveState.Ready && Elapsed + 1e-4f >= BurnSeconds; } }

        /// <summary>Ten nhung thu da cho vao, theo thu tu.</summary>
        public IReadOnlyList<string> Added { get { return added; } }

        /// <summary>Tu luc vat num: dung dung cu va dung thu tu cong thuc (sai thi mon ra Lac vi, constitution muc 5.1).</summary>
        public bool FollowsRecipe { get { return followsRecipe; } }

        public Stove(RecipeLookup lookup, DishFormula formula)
        {
            this.lookup = lookup;
            this.formula = formula;
            CookMultiplier = 1f;
        }

        bool CanEdit { get { return State == StoveState.Empty || State == StoveState.Filling; } }

        public bool SelectTool(string tool)
        {
            if (!CanEdit || string.IsNullOrEmpty(tool)) return false;
            Tool = tool;
            return true;
        }

        /// <summary>Cho mot mon do kho (dau an, toi, nuoc mam...).</summary>
        public bool AddPantry(string name)
        {
            if (!CanEdit || Tool == null || string.IsNullOrEmpty(name)) return false;
            added.Add(name);
            State = StoveState.Filling;
            return true;
        }

        /// <summary>Cho nguyen lieu da so che. Moi mon chi nhan 1 chinh va 1 phu.</summary>
        public bool AddIngredient(Ingredient ingredient)
        {
            if (!CanEdit || Tool == null || ingredient == null || !ingredient.prepared) return false;

            if (ingredient.kind == IngredientKind.Main)
            {
                if (main != null) return false;
                main = ingredient;
            }
            else
            {
                if (side != null) return false;
                side = ingredient;
            }

            added.Add(ingredient.name);
            State = StoveState.Filling;
            return true;
        }

        /// <summary>Mon dang nam tren bep (da du chinh + phu, ke ca dang nau / da chin); null neu chua du.</summary>
        public Recipe PendingRecipe
        {
            get
            {
                if (Recipe != null) return Recipe;
                return main != null && side != null ? lookup.Find(main.typeId, side.typeId) : null;
            }
        }

        /// <summary>Thanh ti le bac mon; null khi chua du nguyen lieu chinh va do phu.</summary>
        public TierChance[] Bar()
        {
            if (main == null || side == null) return null;
            return formula.Bar(main.tier, side.tier);
        }

        /// <summary>Bat dau nau. Can du 1 chinh + 1 phu.</summary>
        public bool Start()
        {
            if (State != StoveState.Filling || main == null || side == null) return false;

            Recipe recipe = lookup.Find(main.typeId, side.typeId);
            if (recipe == null) return false;

            Recipe = recipe;
            followsRecipe = Tool == recipe.tool && SameOrder(added, recipe.steps);
            Elapsed = 0f;
            State = StoveState.Cooking;
            return true;
        }

        /// <summary>Thoi gian troi tren bep; mon da chin van tiep tuc tinh gio de biet qua lua.</summary>
        public void Tick(float seconds)
        {
            if (State != StoveState.Cooking && State != StoveState.Ready) return;
            Elapsed += seconds;
            if (State == StoveState.Cooking && Elapsed + 1e-4f >= CookSeconds) State = StoveState.Ready;
        }

        /// <summary>Muc mon ra quay cho. Quay day thi mon o lai tren bep.</summary>
        public bool TryPlate(Counter counter, IRandom rng, out Dish dish)
        {
            dish = null;
            if (State != StoveState.Ready || counter.IsFull) return false;

            int tier = followsRecipe ? formula.Roll(main.tier, side.tier, rng) : formula.ApplyFloor(SpoiledTier);
            // Qua lua: -1 bac (constitution muc 5.9); Chao gang van giu san "khong duoi Do".
            if (IsBurnt) tier = formula.ApplyFloor(Math.Max(SpoiledTier, tier - 1));
            dish = new Dish
            {
                recipeId = Recipe.id,
                name = Recipe.name,
                baseName = Recipe.baseName,
                tier = tier,
            };
            counter.TryPlace(dish);
            Reset();
            return true;
        }

        /// <summary>Do bo moi thu tren bep.</summary>
        public void Reset()
        {
            added.Clear();
            main = null;
            side = null;
            Recipe = null;
            Tool = null;
            followsRecipe = false;
            Elapsed = 0f;
            State = StoveState.Empty;
        }

        static bool SameOrder(List<string> actual, string[] expected)
        {
            if (actual.Count != expected.Length) return false;
            for (int i = 0; i < expected.Length; i++)
                if (actual[i] != expected[i]) return false;
            return true;
        }
    }
}
