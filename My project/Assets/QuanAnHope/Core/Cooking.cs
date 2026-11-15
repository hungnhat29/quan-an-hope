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

        public double Score(int mainTier, int sideTier)
        {
            double b = (double)rules.weightChinh * mainTier + (double)rules.weightPhu * sideTier + rules.offset;
            return Math.Round(b, ScoreDecimals);
        }

        /// <summary>Thanh ti le hien cho nguoi choi: 1 o (chac chan) hoac 2 o (bac duoi, bac tren).</summary>
        public TierChance[] Bar(int mainTier, int sideTier)
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

        public int Roll(int mainTier, int sideTier, IRandom rng)
        {
            TierChance[] bar = Bar(mainTier, sideTier);
            if (bar.Length == 1) return bar[0].tier;
            return rng.NextDouble() < bar[1].chance ? bar[1].tier : bar[0].tier;
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
        public float CookSeconds { get { return Recipe != null ? Recipe.cookSeconds : 0f; } }

        /// <summary>Ten nhung thu da cho vao, theo thu tu.</summary>
        public IReadOnlyList<string> Added { get { return added; } }

        /// <summary>Tu luc vat num: dung dung cu va dung thu tu cong thuc (sai thi mon ra Lac vi, constitution muc 5.1).</summary>
        public bool FollowsRecipe { get { return followsRecipe; } }

        public Stove(RecipeLookup lookup, DishFormula formula)
        {
            this.lookup = lookup;
            this.formula = formula;
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

        public void Tick(float seconds)
        {
            if (State != StoveState.Cooking) return;
            Elapsed += seconds;
            if (Elapsed + 1e-4f >= CookSeconds) State = StoveState.Ready;
        }

        /// <summary>Muc mon ra quay cho. Quay day thi mon o lai tren bep.</summary>
        public bool TryPlate(Counter counter, IRandom rng, out Dish dish)
        {
            dish = null;
            if (State != StoveState.Ready || counter.IsFull) return false;

            dish = new Dish
            {
                recipeId = Recipe.id,
                name = Recipe.name,
                baseName = Recipe.baseName,
                tier = followsRecipe ? formula.Roll(main.tier, side.tier, rng) : SpoiledTier,
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
