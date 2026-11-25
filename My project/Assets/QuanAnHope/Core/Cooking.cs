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
    /// Cong thuc ra bac mon (constitution 2.1 muc 5.3, balance/cookodds.py):
    /// S = wChinh x bac chinh + wPhu x bac phu; b = S + offset (+ relic diem mon) (- mat dien).
    /// Huyen thoai la ngan rieng: L = legendBase x legendGrowth^(S/5) (+ relic, toi da legendCap). Nam bac con lai chia (1 - L)
    /// theo hinh dang mau chuan truot theo b (noi suy log, duoi e^-decay moi bac). Moi bac toi thieu minChance; luu tren luoi
    /// grid ve nguyen (phan du lon nhat). Du lieu cu (khong co odds): cong thuc 2 bac cua 1.0-2.0.
    /// </summary>
    public class DishFormula
    {
        const int ScoreDecimals = 4; // lam tron de 4,0 khong thanh 3,9999998 do sai so so thuc
        const int TierCount = 6;
        const int TailTerms = 40;

        readonly DishRules rules;

        public DishFormula(Balance balance)
        {
            rules = balance.dish;
        }

        /// <summary>Gia relic cua van choi (Muoi hong, Ot ma, Chao gang, Sao bang; constitution muc 7.3). Null = khong co relic.</summary>
        public RelicInventory Relics { get; set; }

        RelicFx Fx { get { return Relics != null ? Relics.Effects : RelicFx.None; } }

        /// <summary>Ngay mat dien: diem mon tru blackoutPenalty (constitution muc 5.3).</summary>
        public bool Blackout { get; set; }

        /// <summary>Du lieu 2.1 (du 6 bac); false = cong thuc 2 bac cu.</summary>
        public bool SixTiers { get { return rules.odds != null && rules.odds.shape != null && rules.odds.shape.Length == 5 && rules.odds.grid > 0; } }

        /// <summary>Diem nguyen lieu S (thang 0-5, chua cong offset).</summary>
        public double Points(int mainTier, int sideTier)
        {
            return Math.Round((double)rules.weightChinh * mainTier + (double)rules.weightPhu * sideTier, ScoreDecimals);
        }

        public double Score(int mainTier, int sideTier)
        {
            double b = (double)rules.weightChinh * mainTier + (double)rules.weightPhu * sideTier + rules.offset
                + Fx.Get(RelicKeys.DishCenter) - (Blackout ? (double)rules.blackoutPenalty : 0.0);
            return Math.Round(b, ScoreDecimals);
        }

        /// <summary>Ti le Huyen thoai khi nau (0..1): ngan rieng theo S, cong relic, toi da legendCap (muc 5.3, 5.4).</summary>
        public double LegendChance(int mainTier, int sideTier)
        {
            if (!SixTiers) return 0.0;
            DishOddsRules o = rules.odds;
            double l = o.legendBase * Math.Pow(o.legendGrowth, Points(mainTier, sideTier) / 5.0);
            return Math.Min(o.legendCap, l + Fx.Get(RelicKeys.DishLegend));
        }

        /// <summary>Bac thap nhat mon co the ra (Chao gang ba ngoai: khong duoi Do).</summary>
        public int FloorTier { get { return Math.Min(rules.maxTier, (int)Fx.Get(RelicKeys.DishFloor)); } }

        /// <summary>Bac sau san cua relic (ap ca cho mon sai cong thuc, Q-29).</summary>
        public int ApplyFloor(int tier)
        {
            return Math.Max(tier, FloorTier);
        }

        // ---------------- 2.1: sau bac ----------------

        double PointMass(int k)
        {
            DishOddsRules o = rules.odds;
            float[] shape = o.shape;
            double r = Math.Exp(-o.tailDecay);
            const int lo = -2, hi = 2;
            if (k > lo && k < hi) return shape[k - lo];
            if (k <= lo) return shape[0] * (1.0 - r) * Math.Pow(r, lo - k);
            return shape[4] * (1.0 - r) * Math.Pow(r, k - hi);
        }

        double ValueAt(double x)
        {
            int k = (int)Math.Floor(x);
            double f = x - k;
            double a = PointMass(k);
            if (f < 1e-12) return a;
            double b = PointMass(k + 1);
            return Math.Exp((1.0 - f) * Math.Log(a) + f * Math.Log(b));
        }

        /// <summary>Sau xac suat (0..1) bac 0..5 truoc khi lam tron luoi, da ap Ot ma (Lac vi) va Chao gang (san).</summary>
        public double[] Chances(int mainTier, int sideTier)
        {
            var p = new double[TierCount];
            DishOddsRules o = rules.odds;
            double b = Score(mainTier, sideTier);
            double center = b - o.shapeB + o.shapeTier;
            double total = 0.0;
            var m = new double[5];
            for (int t = 0; t < 5; t++)
            {
                double x = t - center;
                if (t == 0) for (int j = 0; j < TailTerms; j++) m[t] += ValueAt(x - j);
                else if (t == 4) for (int j = 0; j < TailTerms; j++) m[t] += ValueAt(x + j);
                else m[t] = ValueAt(x);
                total += m[t];
            }
            double legend = LegendChance(mainTier, sideTier);
            double sum = 0.0;
            for (int t = 0; t < 5; t++) { p[t] = Math.Max(m[t] / total * (1.0 - legend), o.minChance); sum += p[t]; }
            p[5] = Math.Max(legend, o.minChance);
            sum += p[5];
            for (int t = 0; t < TierCount; t++) p[t] /= sum;

            double ruin = Fx.Get(RelicKeys.DishRuin);
            if (ruin > 0.0)
            {
                for (int t = 0; t < TierCount; t++) p[t] *= 1.0 - ruin;
                p[0] += ruin;
            }
            int floor = FloorTier;
            for (int t = 0; t < floor && t < TierCount; t++) { p[floor] += p[t]; p[t] = 0.0; }
            return p;
        }

        /// <summary>So ve tren luoi (tong = grid) cua sau bac; bac con ra duoc co it nhat 1 ve (muc 5.3 buoc 3-4).</summary>
        public int[] Tickets(int mainTier, int sideTier)
        {
            double[] p = Chances(mainTier, sideTier);
            int grid = rules.odds.grid;
            var t = new int[TierCount];
            var frac = new double[TierCount];
            int used = 0;
            for (int i = 0; i < TierCount; i++)
            {
                double raw = p[i] * grid;
                if (p[i] > 0.0 && raw < 1.0) { t[i] = 1; frac[i] = -1.0; }
                else { t[i] = (int)Math.Floor(raw); frac[i] = p[i] > 0.0 ? raw - t[i] : -1.0; }
                used += t[i];
            }
            int left = grid - used;
            // phan du lon nhat: moi bac nhan toi da 1 ve them (tong phan du < so bac nen du)
            while (left > 0)
            {
                int best = -1;
                for (int i = 0; i < TierCount; i++) if (frac[i] >= 0.0 && (best < 0 || frac[i] > frac[best])) best = i;
                if (best < 0) { for (int i = 0; i < TierCount; i++) if (p[i] > 0.0 && (best < 0 || t[i] > t[best])) best = i; }
                t[best]++;
                frac[best] = -1.0;
                left--;
            }
            while (left < 0)
            {
                int big = 0;
                for (int i = 1; i < TierCount; i++) if (t[i] > t[big]) big = i;
                t[big]--;
                left++;
            }
            return t;
        }

        // ---------------- 1.0-2.0: hai bac ----------------

        /// <summary>Thanh ti le tu cong thuc 2 bac (truoc Ot ma va Chao gang): 1 o hoac 2 o.</summary>
        TierChance[] TwoTierBar(int mainTier, int sideTier)
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
        /// Thanh ti le hien cho nguoi choi (constitution Dieu 4). 2.1: du 6 bac (ke ca bac rat nho), theo luoi ve; bac bi relic chan
        /// (Chao gang) khong co trong thanh. Du lieu cu: 1-2 o, co Ot ma thi them o Lac vi, Chao gang gop bac duoi vao Do.
        /// </summary>
        public TierChance[] Bar(int mainTier, int sideTier)
        {
            if (SixTiers)
            {
                int[] t = Tickets(mainTier, sideTier);
                var list6 = new List<TierChance>();
                for (int i = 0; i < TierCount; i++)
                    if (t[i] > 0) list6.Add(new TierChance { tier = i, chance = (double)t[i] / rules.odds.grid });
                return list6.ToArray();
            }

            TierChance[] bar = TwoTierBar(mainTier, sideTier);
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

        /// <summary>
        /// Quay bac mon. 2.1: mot lan rut tren luoi ve (Ot ma / Chao gang da nam trong luoi). Du lieu cu: theo thu tu cua
        /// sim_balance.py cook(): cong thuc -> Ot ma -> Chao gang.
        /// </summary>
        public int Roll(int mainTier, int sideTier, IRandom rng)
        {
            if (SixTiers)
            {
                int[] t = Tickets(mainTier, sideTier);
                int x = (int)Math.Floor(rng.NextDouble() * rules.odds.grid);
                int acc = 0;
                for (int i = 0; i < TierCount; i++)
                {
                    acc += t[i];
                    if (x < acc) return i;
                }
                return TierCount - 1;
            }

            TierChance[] bar = TwoTierBar(mainTier, sideTier);
            int tier = bar.Length == 1 ? bar[0].tier : (rng.NextDouble() < bar[1].chance ? bar[1].tier : bar[0].tier);
            double ruin = Fx.Get(RelicKeys.DishRuin);
            if (ruin > 0.0 && rng.NextDouble() < ruin) tier = 0;
            return ApplyFloor(tier);
        }

        /// <summary>
        /// Chu hien phan tram (constitution 2.1 muc 5.3): tu 1% hai chu so thap phan, duoi 1% du chu so (0,005%),
        /// khong bao gio hien "0%" cho bac van ra duoc. Dau phay thap phan kieu Viet.
        /// </summary>
        public static string FormatPercent(double chance)
        {
            double pct = Math.Round(chance * 100.0, 6);
            if (pct <= 0.0) return "0%";
            string s;
            if (pct >= 1.0) s = pct.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            else
            {
                s = pct.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                if (s == "0") s = "0.001";
            }
            return s.Replace('.', ',') + "%";
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

        /// <summary>Cap (chinh, phu) nay co mon khong (constitution 2.0 muc 5.8: chi ~70% cap co cong thuc).</summary>
        public bool Pairs(string mainId, string sideId)
        {
            return mainId != null && sideId != null && byId.ContainsKey(mainId + IdSeparator + sideId);
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

        /// <summary>Bac Huyen thoai (2.1 muc 5.4: mon nao cung dat duoc - khi nau, relic, nuoc sot).</summary>
        public bool IsLegend { get { return tier >= LegendTier; } }

        /// <summary>Ten hien tren dia: ten mon (2.1: Huyen thoai la bac cua moi mon; "Mon Hy Vong" chi la ten goi trong truyen).</summary>
        public string Title { get { return name; } }
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

        /// <summary>Nguyen lieu chinh / do phu dang trong bep (null neu chua co).</summary>
        public Ingredient MainIn { get { return main; } }
        public Ingredient SideIn { get { return side; } }

        /// <summary>Nguyen lieu nay cho vao bep luc nay co thanh cap co mon khong (true neu chua co nguyen lieu kia).</summary>
        public bool Accepts(Ingredient ingredient)
        {
            if (ingredient == null) return false;
            if (ingredient.kind == IngredientKind.Main) return side == null || lookup.Pairs(ingredient.typeId, side.typeId);
            return main == null || lookup.Pairs(main.typeId, ingredient.typeId);
        }

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
                // constitution 2.0 muc 5.8: cap khong co mon thi bep khong nhan
                if (side != null && !lookup.Pairs(ingredient.typeId, side.typeId)) return false;
                main = ingredient;
            }
            else
            {
                if (side != null) return false;
                if (main != null && !lookup.Pairs(main.typeId, ingredient.typeId)) return false;
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
