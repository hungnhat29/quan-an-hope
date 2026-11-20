using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using QuanAnHope.Core;
using QuanAnHope.Data;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.EditorTools
{
    /// <summary>Nhap balance/balance.json + balance/recipes.json thanh BalanceData.asset.</summary>
    public static class BalanceImporter
    {
        /// <summary>Nam trong Resources de game tai duoc luc chay (Resources.Load("BalanceData")).</summary>
        public const string AssetPath = "Assets/QuanAnHope/Resources/BalanceData.asset";

        /// <summary>Thu muc balance/ o goc repo (ngoai project Unity).</summary>
        public static string BalanceFolder
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "balance")); }
        }

        public static string BalanceJsonPath { get { return Path.Combine(BalanceFolder, "balance.json"); } }
        public static string RecipesJsonPath { get { return Path.Combine(BalanceFolder, "recipes.json"); } }
        public static string RelicsJsonPath { get { return Path.Combine(BalanceFolder, "relics.json"); } }

        [MenuItem("Quán Ăn Hope/Nhập balance.json")]
        public static void Import()
        {
            ImportTo(AssetPath);
            Debug.Log("Quán Ăn Hope: đã nhập balance.json + recipes.json + relics.json vào " + AssetPath);
        }

        /// <summary>Doc hai file JSON va ghi (hoac ghi de) asset tai <paramref name="assetPath"/>.</summary>
        public static BalanceData ImportTo(string assetPath)
        {
            Balance balance = ParseBalance(File.ReadAllText(BalanceJsonPath), File.ReadAllText(RelicsJsonPath));
            RecipeBook recipes = ParseRecipes(File.ReadAllText(RecipesJsonPath));

            var asset = AssetDatabase.LoadAssetAtPath<BalanceData>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BalanceData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            asset.balance = balance;
            asset.recipes = recipes;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        /// <summary>balance.json + relics.json (constitution muc 7). Khong co relics.json thi khong co relic nao.</summary>
        public static Balance ParseBalance(string json, string relicsJson)
        {
            Balance b = ParseBalance(json);
            b.relics = JArray.Parse(relicsJson).Cast<JObject>()
                .Select(o => new RelicDef
                {
                    id = Str(o, "id"),
                    name = Str(o, "name"),
                    set = Str(o, "set"),
                    price = Int(o, "price"),
                    rarity = Str(o, "rarity"),
                    effects = Effects(Obj(o, "fx")),
                })
                .ToArray();
            return b;
        }

        static RelicEffect[] Effects(JObject fx)
        {
            return fx.Properties().Select(p => new RelicEffect { key = p.Name, value = (float)p.Value }).ToArray();
        }

        public static Balance ParseBalance(string json)
        {
            JObject root = JObject.Parse(json);
            var b = new Balance();

            b.days = Int(root, "days");
            b.ticketsPerDay = Int(root, "tickets_per_day");
            b.bossBonus = Int(root, "boss_bonus");
            b.stormDay = Int(root, "storm_day");
            b.counterSlots = Int(root, "counter_slots");

            JObject payment = Obj(root, "payment");
            b.payment = new PaymentRules
            {
                tipBase = Float(payment, "tip_base"),
                tipPerTier = Float(payment, "tip_per_tier"),
                underPayShare = Float(payment, "under_pay_share"),
                penaltyBaseShare = Float(payment, "penalty_base_share"),
            };

            b.debts = NumberedKeys(Obj(root, "debt_days"))
                .Select(p => new DebtEntry { day = p.Key, amount = (int)p.Value })
                .ToArray();

            b.rounds = NumberedKeys(Obj(root, "rounds"))
                .Select(p => new RoundRange { round = p.Key, firstDay = (int)p.Value[0], lastDay = (int)p.Value[1] })
                .ToArray();

            b.ingredientOdds = Obj(root, "ingredient_odds").Properties()
                .Select(p => new TierTable { id = p.Name, percents = p.Value.Select(v => (int)v).ToArray() })
                .ToArray();

            JObject crate = Obj(root, "crate_size");
            b.crateSize = new CrateSize
            {
                phu = Int(crate, "phu"),
                chinh = Int(crate, "chinh"),
                phuStorm = Int(crate, "phu_storm"),
                chinhStorm = Int(crate, "chinh_storm"),
            };

            JObject dish = Obj(root, "dish");
            b.dish = new DishRules
            {
                weightChinh = Float(dish, "w_chinh"),
                weightPhu = Float(dish, "w_phu"),
                offset = Float(dish, "offset"),
                blackoutPenalty = Float(dish, "blackout_penalty"),
                maxTier = Int(dish, "max_tier"),
                priceMult = Arr(dish, "price_mult").Select(v => (float)v).ToArray(),
                basePriceByRound = ByRound(Obj(dish, "base_price"), b.rounds),
            };

            JObject customers = Obj(root, "customers");
            b.customers = new CustomerRules
            {
                easyBands = NumberedKeys(Obj(customers, "easy_bands"))
                    .Select(p => new RoundBands
                    {
                        round = p.Key,
                        bands = p.Value
                            .Select(band => new EasyBand { min = (int)band[0], max = (int)band[1], share = (float)band[2] })
                            .ToArray(),
                    })
                    .ToArray(),
                requiredTier = Arr(customers, "required_tier")
                    .Select(pair => new RequiredTier { minEasy = (int)pair[0], tier = (int)pair[1] })
                    .ToArray(),
                bossEasy = Int(customers, "boss_easy"),
            };

            JObject start = Obj(root, "start");
            b.start = new StartRules
            {
                money = Int(start, "money"),
                bonusTickets = Int(start, "bonus_tickets"),
                relicShopFromDay = Int(start, "relic_shop_from_day"),
            };

            JObject day = Obj(root, "day");
            b.day = new DayRules
            {
                openHour = Int(day, "open_hour"),
                closeHour = Int(day, "close_hour"),
                secondsPerHour = Float(day, "seconds_per_hour"),
                bossHour = Int(day, "boss_hour"),
                bossDay = Int(day, "boss_day"),
                nextCustomerSeconds = Float(day, "next_customer_s"),
                eatSeconds = Float(day, "eat_s"),
            };

            b.relicCap = Int(root, "relic_cap");
            b.relicShopOffers = Int(root, "relic_shop_offers");
            JObject bt = Obj(root, "breakthrough");
            b.breakthrough = new BreakthroughRules
            {
                baseChance = Float(bt, "base_chance"),
                items = Int(bt, "items"),
                tier5Share = Float(bt, "tier5_share"),
            };
            var bonuses = new List<SetBonus>();
            foreach (JProperty set in Obj(root, "set_bonus").Properties())
                foreach (KeyValuePair<int, JToken> th in NumberedKeys((JObject)set.Value))
                    bonuses.Add(new SetBonus { set = set.Name, count = th.Key, effects = Effects((JObject)th.Value) });
            b.setBonuses = bonuses.ToArray();

            // ---------- sprint 4 ----------
            JObject seats = Obj(root, "seats");
            b.seatsByRound = ByRound(seats, b.rounds);
            b.itemShopFromDay = Int(root, "item_shop_from_day");
            JObject pat = Obj(root, "patience");
            JObject safe = Obj(pat, "safe_s");
            b.patience = new PatienceRules
            {
                fromRound = Int(pat, "from_round"),
                safeSecondsByRound = b.rounds.Select(r => safe[r.round.ToString()] != null ? (float)safe[r.round.ToString()] : 0f).ToArray(),
                dropEverySeconds = Float(pat, "drop_every_s"),
                maxDrop = Int(pat, "max_drop"),
                blackoutExtraSeconds = Float(pat, "blackout_extra_s"),
            };
            JObject bo = Obj(root, "blackout");
            b.blackout = new BlackoutRules { day = Int(bo, "day"), cookMultiplier = Float(bo, "cook_mult"), dim = Float(bo, "dim") };
            JObject fr = Obj(root, "fridge");
            b.fridge = new FridgeRules { slots = Int(fr, "slots"), decayDays = Int(fr, "decay_days") };
            JObject sauce = Obj(root, "sauce");
            b.sauce = new SauceRules
            {
                price = Int(sauce, "price"),
                step = Int(sauce, "step"),
                max = Int(sauce, "max"),
                unlockDay = Int(sauce, "unlock_day"),
                lateFromDay = Int(sauce, "late_from_day"),
                oddsEarly = Sauce(Obj(sauce, "odds_early")),
                oddsLate = Sauce(Obj(sauce, "odds_late")),
            };
            JObject mc = Obj(root, "machine");
            JObject mo = Obj(mc, "odds");
            b.machine = new MachineRules
            {
                price = Int(mc, "price"),
                usesPerDay = Int(mc, "uses_per_day"),
                trash = Float(mo, "trash"),
                down1 = Float(mo, "down1"),
                same = Float(mo, "same"),
                up1 = Float(mo, "up1"),
                up2 = Float(mo, "up2"),
                up3 = Float(mo, "up3"),
            };
            JObject bk = Obj(root, "black_ticket");
            b.blackTicket = new PricedItemRules { price = Int(bk, "price"), step = Int(bk, "step"), max = Int(bk, "max") };

            return b;
        }

        static SauceOdds Sauce(JObject o)
        {
            return new SauceOdds
            {
                fail = Float(o, "fail"),
                same = Float(o, "same"),
                up1 = Float(o, "up1"),
                up2 = Float(o, "up2"),
                legend = Float(o, "legend"),
            };
        }

        public static RecipeBook ParseRecipes(string json)
        {
            JObject root = JObject.Parse(json);
            var book = new RecipeBook();

            JObject timing = Obj(root, "timing");
            book.timing = new CookTiming
            {
                stepSeconds = Float(timing, "step_s"),
                plateSeconds = Float(timing, "plate_s"),
                serveSeconds = Float(timing, "serve_s"),
                warnFactor = Float(timing, "warn_factor"),
                burnFactor = Float(timing, "burn_factor"),
                burnFromRound = Int(timing, "burn_from_round"),
            };

            book.pantry = Arr(root, "pantry").Select(v => (string)v).ToArray();
            book.bases = Arr(root, "bases").Select(v => (string)v).ToArray();

            book.sides = Arr(root, "phu").Cast<JObject>()
                .Select(o => new SideIngredient
                {
                    id = Str(o, "id"),
                    name = Str(o, "name"),
                    prep = Str(o, "prep"),
                    prepSeconds = Float(o, "prep_s"),
                    unlockRound = Int(o, "unlock_round"),
                })
                .ToArray();

            book.mains = Arr(root, "chinh").Cast<JObject>()
                .Select(o => new MainIngredient
                {
                    id = Str(o, "id"),
                    name = Str(o, "name"),
                    group = Str(o, "group"),
                    prep = Str(o, "prep"),
                    prepSeconds = Float(o, "prep_s"),
                    tool = Str(o, "tool"),
                    baseName = Str(o, "base"),
                    cookSeconds = Float(o, "cook_s"),
                    unlockRound = Int(o, "unlock_round"),
                    method = Str(o, "method"),
                })
                .ToArray();

            book.recipes = Arr(root, "recipes").Cast<JObject>().Select(ParseRecipe).ToArray();
            return book;
        }

        static Recipe ParseRecipe(JObject o)
        {
            string id = Str(o, "id");
            string[] parts = id.Split(new[] { "__" }, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new FormatException("recipes.json: id cong thuc phai co dang <chinh>__<phu>, gap: " + id);

            return new Recipe
            {
                id = id,
                name = Str(o, "name"),
                group = Str(o, "group"),
                mainId = parts[0],
                sideId = parts[1],
                tool = Str(o, "tool"),
                baseName = Str(o, "base"),
                steps = Arr(o, "steps").Select(v => (string)v).ToArray(),
                cookSeconds = Float(o, "cook_s"),
                warnSeconds = Float(o, "warn_s"),
                burnSeconds = Float(o, "burn_s"),
                unlockRound = Int(o, "unlock_round"),
            };
        }

        // ---------- doc JSON: thieu truong thi bao loi ro rang, khong am tham dung 0 ----------

        static JToken Need(JObject o, string key)
        {
            JToken t = o[key];
            if (t == null || t.Type == JTokenType.Null)
                throw new FormatException("Thieu truong \"" + key + "\" trong " + (string.IsNullOrEmpty(o.Path) ? "goc JSON" : o.Path));
            return t;
        }

        static int Int(JObject o, string key) { return (int)Need(o, key); }
        static float Float(JObject o, string key) { return (float)Need(o, key); }
        static string Str(JObject o, string key) { return (string)Need(o, key); }
        static JObject Obj(JObject o, string key) { return (JObject)Need(o, key); }
        static JArray Arr(JObject o, string key) { return (JArray)Need(o, key); }

        /// <summary>Cac khoa la so ("1", "4", ...) theo thu tu tang dan; bo qua khoa khong phai so (vd "d14").</summary>
        static IEnumerable<KeyValuePair<int, JToken>> NumberedKeys(JObject o)
        {
            var list = new List<KeyValuePair<int, JToken>>();
            foreach (JProperty p in o.Properties())
            {
                int n;
                if (int.TryParse(p.Name, out n)) list.Add(new KeyValuePair<int, JToken>(n, p.Value));
            }
            list.Sort((a, c) => a.Key.CompareTo(c.Key));
            return list;
        }

        /// <summary>Gia tri theo vong, chi so 0 = vong 1; moi vong trong <paramref name="rounds"/> phai co.</summary>
        static int[] ByRound(JObject o, RoundRange[] rounds)
        {
            return rounds.Select(r => Int(o, r.round.ToString())).ToArray();
        }
    }
}
