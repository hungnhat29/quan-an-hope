using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace QuanAnHope.Core
{
    /// <summary>Ba kieu nguoi choi cua constitution Dieu 1 / balance/sim_balance.py: naive, nocombo, smart.</summary>
    public enum BotStyle
    {
        /// <summary>Khong doc chi so: dung het ve, lay nguyen lieu dau tien, phuc vu theo thu tu khach den.</summary>
        Naive,

        /// <summary>Choi tot, khong gom bo relic.</summary>
        NoCombo,

        /// <summary>Choi tot va gom mot bo relic.</summary>
        Smart,
    }

    /// <summary>Ket qua mot van bot (de in bao cao F-045).</summary>
    public class BotRun
    {
        public BotStyle style;
        public bool won;
        public int lastDay;
        public bool reachedBossDay;
        public bool bossServed;
        public int bossTier = -1;
        public int bossBonus;
        public int served;
        public int daysPlayed;
        public int earlyCloses;
        public int legendDishes;
        public int maxSetCount;
        /// <summary>Bo co nhieu mon nhat luc het van (F-060 AC-5: thang theo tung bo).</summary>
        public string topSet;
        public int wrongForSeat;

        /// <summary>Ngay 21: dau ngay co dia Tuyet hao trong tu lanh khong; gio phuc vu chu no (phut tu 9:00, -1 = khong phuc vu).</summary>
        public bool bossDayHadReserve;
        public int bossServedMinute = -1;

        /// <summary>Tien bi tru khi dua dia sai mon (de trong ghe), so khach co tinh gio va tong diem de tinh mat vi cho.</summary>
        public int wrongPenalty;
        public int timedServes;
        public int patienceDrop;

        /// <summary>F-060 AC-6 (sprint 7): so khach bi giam de tinh vi cho (giam &gt; 0) va so ngay co tinh gio (vong 2-4).</summary>
        public int impatientServes;
        public int timedDays;

        /// <summary>Mon nau san / nguyen lieu so che san truoc khi mo quan (constitution 1.7 muc 6.2).</summary>
        public int preCooked;
        public int prePrepped;

        /// <summary>Ngay 21 khong phuc vu duoc chu no: vi sao (de ghi bao cao).</summary>
        public string bossMissReason;

        /// <summary>Dau ngay 9, 15, 21 (sau khi tra no): tien va relic dang co (lenh go loi "bat dau tu vong N", impl plan muc 5).</summary>
        public readonly Dictionary<int, KeyValuePair<int, string>> startOf = new Dictionary<int, KeyValuePair<int, string>>();
        public static readonly int[] StartDays = { 9, 15, 21 };

        /// <summary>Tien dau ngay thu no, truoc khi tra (ngay -> tien), nhu sim_balance.py p.hist.</summary>
        public readonly Dictionary<int, int> moneyBeforeDebt = new Dictionary<int, int>();
    }

    /// <summary>
    /// Bot F-045 (docs/sprint-5/impl-plan.md): choi tron 22 ngay tren GameSession that (dong ho, 3 ghe, gio cho, goi mon,
    /// tu lanh, nuoc sot, may gacha, ve den, chu no). Chien thuat chep tu balance/sim_balance.py run(); nhung cho
    /// mo phong phai don gian hoa (thoi gian cho, goi mon) thi o day dung luat that. Day la GIA DINH ve nguoi choi, khong phai luat game.
    /// </summary>
    public static class StrategyBot
    {
        /// <summary>sim_balance.py: OVERHEAD naive 30 giay, OVER_GOOD 20 giay, PULL_S 4, FRIDGE_S 6.</summary>
        public static readonly BotTiming Good = new BotTiming { pullSeconds = 4f, overheadSeconds = 20f };
        public const float FridgeSeconds = 6f;

        /// <summary>sim_balance.py score(): diem relic le cho nguoi choi tot.</summary>
        static readonly Dictionary<string, int> LoneScore = new Dictionary<string, int>
        {
            { "F1", 8 }, { "F4", 5 }, { "F2", 4 }, { "F3", 3 }, { "F9", 3 }, { "F5", 2 }, { "F10", 4 },
        };

        // Nguong mua cua sim_balance.py run() (GIA DINH cua mo phong).
        const int MachineFromDay = 5;
        const double SauceReserve = 1.15;
        const int SauceHoldMax = 4;
        const int GoodBlackFromDay = 17;
        const int GoodBlackPullFromDay = 20;
        const double BlackReserve = 1.1;
        const int KeepTopFromDay = 17;
        const int NeedTopFromDay = 19;
        const int SauceTierGood = 3;
        const int SauceRoundGood = 3;
        const int MachineMaxTier = 1;
        const int KeepTicketsForBoss = 1;

        public static BotRun LastRun { get; private set; }

        class Ctx
        {
            public GameSession s;
            public BotStyle style;
            public BotTiming timing;
            public string target; // bo smart dang gom
            public BotRun run;
            public bool Good { get { return style != BotStyle.Naive; } }
        }

        /// <summary>Choi toi dau ngay <paramref name="day"/> (pha Prep) hoac het van. Dung cho test luu / tai (F-052).</summary>
        public static void PlayUntil(GameSession s, BotStyle style, int day)
        {
            var c = new Ctx { s = s, style = style, timing = BotTiming.From(s.Balance, style == BotStyle.Naive), run = new BotRun { style = style } };
            while (s.Phase == GamePhase.Prep && s.Day < day)
            {
                PlayDay(c);
                s.StartNextDay();
            }
        }

        public static GamePhase PlayToEnd(GameSession s, BotStyle style)
        {
            var c = new Ctx
            {
                s = s,
                style = style,
                timing = BotTiming.From(s.Balance, style == BotStyle.Naive),
                run = new BotRun { style = style },
            };
            LastRun = c.run;
            while (s.Phase != GamePhase.Cleared && s.Phase != GamePhase.Lost)
            {
                if (s.Phase != GamePhase.Prep) break;
                if (Array.IndexOf(BotRun.StartDays, s.Day) >= 0)
                {
                    var ids = new List<string>();
                    foreach (RelicDef r in s.Relics.Owned) ids.Add(r.id);
                    c.run.startOf[s.Day] = new KeyValuePair<int, string>(s.Money, string.Join(",", ids.ToArray()));
                }
                PlayDay(c);
                c.run.served += s.ServedToday;
                c.run.daysPlayed++;
                if (Patience.Applies(s.Balance, s.Day)) c.run.timedDays++;
                int due = s.Balance.DebtDueOn(s.Day + 1);
                if (due > 0) c.run.moneyBeforeDebt[s.Day + 1] = s.Money;
                s.StartNextDay();
            }
            c.run.won = s.IsWon;
            c.run.lastDay = s.Day;
            foreach (RelicDef r in s.Relics.Owned)
                if (!string.IsNullOrEmpty(r.set) && s.Relics.CountInSet(r.set) > c.run.maxSetCount)
                {
                    c.run.maxSetCount = s.Relics.CountInSet(r.set);
                    c.run.topSet = r.set;
                }
            return s.Phase;
        }

        // ---------------- mot ngay ----------------

        static void PlayDay(Ctx c)
        {
            GameSession s = c.s;
            if (s.IsBossDay)
            {
                c.run.reachedBossDay = true;
                foreach (Dish d in s.Fridge.Dishes) if (d.tier >= s.Balance.dish.maxTier) c.run.bossDayHadReserve = true;
            }
            int needPulls = NeedPulls(c);

            BuyRelics(c, needPulls);
            BuyItems(c);

            int pulls = c.Good ? Math.Min(s.Wallet.Count, needPulls + (s.Day >= 20 && s.Day <= 21 ? 1 : 0)) : s.Wallet.Count;
            // Ngay chu no, nguoi choi tot giu 1 ve toi 20:00: ba ghe co the kin khach goi mon da het (Q-39),
            // phai co mon dua cho mot khach thi chu no moi co ghe (khong co trong sim_balance.py).
            if (c.Good && s.IsBossDay) pulls = Math.Min(pulls, Math.Max(0, s.Wallet.Count - KeepTicketsForBoss));
            PullResult pull;
            for (int i = 0; i < pulls && s.PullCrates(out pull); i++) Spend(c, c.timing.pullSeconds);
            if (!c.Good || s.Day >= GoodBlackPullFromDay)
                while (s.PullBlack(out pull)) Spend(c, c.timing.pullSeconds);

            PreCook(c);
            s.OpenShop();
            int guard = 0;
            while (s.Phase == GamePhase.Open && guard++ < 2000)
            {
                if (s.CurrentCustomer == null)
                {
                    s.Tick(s.Balance.day.nextCustomerSeconds);
                    continue;
                }
                if (ServeSomeone(c)) continue;
                if (WaitForBoss(c)) continue;
                if (c.Good && FreeSeat(c)) continue;
                c.run.earlyCloses++;
                NoteMiss(c, "dong quan");
                s.CloseEarly();
            }
            if (s.Phase == GamePhase.Open) { NoteMiss(c, "het luot"); s.CloseEarly(); }
            if (s.IsBossDay && !c.run.bossServed && c.run.bossMissReason == null) NoteMiss(c, "het ngay");
            if (s.Phase == GamePhase.Prep && s.OpenShop()) s.CloseEarly();
        }

        static void NoteMiss(Ctx c, string what)
        {
            GameSession s = c.s;
            if (!s.IsBossDay || s.BossServed || c.run.bossMissReason != null) return;
            int mains = 0, sides = 0;
            foreach (Ingredient i in s.Stock.Items) { if (i.kind == IngredientKind.Main) mains++; else sides++; }
            c.run.bossMissReason = what + " " + s.Clock.Text + (s.BossArrived ? " da toi" : " chua toi") + (s.BossWaiting ? " doi ghe" : "")
                + (s.Boss != null ? " dang ngoi" : "") + " khach=" + Queue(s).Count + " kho=" + mains + "/" + sides + " ve=" + s.Wallet.Count
                + " quay=" + s.Counter.Dishes.Count + " tu=" + s.Fridge.Dishes.Count + " pha=" + s.Phase;
        }

        /// <summary>sim_balance.py: so luot quay du cho so mon kip nau trong ngay.</summary>
        static int NeedPulls(Ctx c)
        {
            GameSession s = c.s;
            Balance b = s.Balance;
            int mainBase = b.MainCrateSize(s.Day) + (int)s.Relics.Effects.Get(RelicKeys.MainPrefix + RelicKeys.Size);
            int cap = (int)(b.DaySeconds() / AvgDishSeconds(c));
            return (cap + Math.Max(1, mainBase) - 1) / Math.Max(1, mainBase);
        }

        static readonly Dictionary<string, double> avgCache = new Dictionary<string, double>();

        static double AvgDishSeconds(Ctx c)
        {
            int round = c.s.Round;
            string key = c.style + "_" + round + "_" + c.s.Recipes.GetHashCode();
            double v;
            if (avgCache.TryGetValue(key, out v)) return v;
            double sum = 0; int n = 0;
            foreach (Recipe r in c.s.Recipes.recipes)
            {
                if (r.group != "chinh" || r.unlockRound < 1 || r.unlockRound > round) continue;
                // nguoi tot so che san truoc khi mo quan (constitution 1.7): mot mon khong con ton gio so che
                sum += (c.Good ? WorkSeconds(c.s, r) - PrepSeconds(c.s, r) : WorkSeconds(c.s, r)) + c.timing.overheadSeconds;
                n++;
            }
            v = n > 0 ? sum / n : 60.0;
            avgCache[key] = v;
            return v;
        }

        static float PrepSeconds(GameSession s, Recipe r)
        {
            float prep = 0f;
            foreach (MainIngredient m in s.Recipes.mains) if (m.id == r.mainId) prep += m.prepSeconds;
            foreach (SideIngredient p in s.Recipes.sides) if (p.id == r.sideId) prep += p.prepSeconds;
            return prep;
        }

        static float WorkSeconds(GameSession s, Recipe r)
        {
            CookTiming t = s.Recipes.timing;
            float prep = 0f;
            foreach (MainIngredient m in s.Recipes.mains) if (m.id == r.mainId) prep += m.prepSeconds;
            foreach (SideIngredient p in s.Recipes.sides) if (p.id == r.sideId) prep += p.prepSeconds;
            return prep + t.stepSeconds * r.steps.Length + r.cookSeconds + t.plateSeconds;
        }

        static void Spend(Ctx c, float seconds)
        {
            if (seconds > 0f) c.s.Tick(seconds);
        }

        // ---------------- relic (sim_balance.py run() muc relics, score, add_relic) ----------------

        static int Score(Ctx c, RelicDef o, bool owned)
        {
            if (c.style == BotStyle.Naive) return o.price;
            int lone;
            if (c.style == BotStyle.NoCombo) return LoneScore.TryGetValue(o.id, out lone) ? lone : 2;
            if (!string.IsNullOrEmpty(o.set))
            {
                if (c.target == null && !owned) c.target = o.set;
                return o.set == c.target ? 10 + c.s.Relics.CountInSet(o.set) * 3 : 1;
            }
            return LoneScore.TryGetValue(o.id, out lone) ? lone : 0;
        }

        static void BuyRelics(Ctx c, int needPulls)
        {
            GameSession s = c.s;
            if (!s.Shop.IsOpen) return;
            if (c.style == BotStyle.Naive)
            {
                NaiveBot.BuyRelic(s);
                return;
            }
            int budget = s.Day > 2 ? s.Wallet.Count - Math.Max(0, needPulls - 1) : s.Wallet.Count - needPulls + 1;
            if (s.IsBossDay) budget -= KeepTicketsForBoss;
            var slots = new List<int>();
            var scores = new Dictionary<int, int>();
            for (int i = 0; i < s.Shop.Offers.Count; i++)
            {
                if (s.Shop.Offers[i] == null) continue;
                slots.Add(i);
                scores[i] = Score(c, s.Shop.Offers[i], false);
            }
            slots.Sort((a, b2) => scores[b2].CompareTo(scores[a]));
            foreach (int i in slots)
            {
                RelicDef o = s.Shop.Offers[i];
                if (o == null) continue;
                int sc = Score(c, o, false);
                int price = s.Shop.Price(i);
                if (sc <= 0 || price > budget || price > s.Wallet.Count) continue;
                string discard = null;
                if (s.Relics.IsFull)
                {
                    RelicDef worst = null; int ws = int.MaxValue;
                    foreach (RelicDef r in s.Relics.Owned)
                    {
                        int rs = Score(c, r, true);
                        if (rs < ws) { ws = rs; worst = r; }
                    }
                    discard = worst.id;
                }
                if (s.BuyRelic(i, discard) == RelicBuyResult.Bought) budget -= price;
            }
            // ngay relic mien phi (constitution 1.8 muc 7.1): nguoi that luon lay mot cai, ke ca khi khong hop bo
            if (s.Shop.IsFreeDay && !s.Shop.FreeDone && slots.Count > 0 && !s.Relics.IsFull) s.BuyRelic(slots[0]);
        }

        // ---------------- cua hang vat pham (sim_balance.py run() muc shop) ----------------

        static int Reserve(GameSession s)
        {
            DebtEntry next = null;
            foreach (DebtEntry d in s.Balance.debts)
                if (d.day >= s.Day && (next == null || d.day < next.day)) next = d;
            return next != null ? next.amount : 0;
        }

        static void BuyItems(Ctx c)
        {
            GameSession s = c.s;
            if (!s.Items.IsOpen) return;
            int reserve = Reserve(s);
            if (c.style == BotStyle.Naive)
            {
                if (s.Day >= NaiveBot.BlackFromDay && s.Items.BlackBought < s.Balance.blackTicket.max
                    && s.Money - s.Items.BlackPrice > reserve * BlackReserve)
                    s.BuyBlackTicket();
                return;
            }
            if (!s.Items.HasMachine && s.Day >= MachineFromDay && s.Money - s.Balance.machine.price > reserve) s.BuyMachine();
            while (s.Day >= s.Balance.sauce.unlockDay && s.Items.SaucesBought < s.Balance.sauce.max)
            {
                if (s.Money - s.Items.SaucePrice > reserve * SauceReserve && s.Items.Sauces < SauceHoldMax)
                {
                    if (s.BuySauce() != ItemBuyResult.Bought) break;
                }
                else break;
            }
            if (s.Day >= GoodBlackFromDay)
                while (s.Items.BlackBought < s.Balance.blackTicket.max && s.Money - s.Items.BlackPrice > reserve * BlackReserve)
                    if (s.BuyBlackTicket() != ItemBuyResult.Bought) break;
        }

        // ---------------- phuc vu ----------------

        /// <summary>Dia de danh cho chu no (nguoi choi tot, tu ngay 17): dia trong tu lanh bac Tuyet hao tro len.</summary>
        static Dish Reserved(Ctx c)
        {
            GameSession s = c.s;
            if (!c.Good || s.Day < KeepTopFromDay) return null;
            Dish best = null;
            foreach (Dish d in s.Fridge.Dishes)
                if (d.tier >= s.Balance.dish.maxTier && (best == null || d.tier > best.tier)) best = d;
            return best;
        }

        static IEnumerable<Dish> ReadyDishes(GameSession s)
        {
            foreach (Dish d in s.Counter.Dishes) yield return d;
            foreach (Dish d in s.Fridge.Dishes) yield return d;
        }

        static int Required(GameSession s, Customer cu)
        {
            return s.Balance.RequiredTierFor(s.EasinessNow(cu));
        }

        static int Capped(GameSession s, int tier)
        {
            return Math.Min(tier, s.Balance.dish.maxTier);
        }

        /// <summary>sim pick(): dia dung mon; nguoi choi tot lay dia thap nhat con dat yeu cau, khong co thi dia cao nhat.</summary>
        static Dish PickDish(Ctx c, Customer cu)
        {
            GameSession s = c.s;
            Dish reserved = Reserved(c);
            var same = new List<Dish>();
            foreach (Dish d in ReadyDishes(s))
                if (d.recipeId == cu.orderId && !ReferenceEquals(d, reserved)) same.Add(d);
            if (same.Count == 0) return null;
            if (!c.Good) return same[0];
            int rq = Required(s, cu);
            Dish ok = null, top = null;
            foreach (Dish d in same)
            {
                if (Capped(s, d.tier) >= rq && (ok == null || d.tier < ok.tier)) ok = d;
                if (top == null || d.tier > top.tier) top = d;
            }
            return ok ?? top;
        }

        static List<Customer> Queue(GameSession s)
        {
            var q = new List<Customer>();
            foreach (Customer cu in s.Seats) if (cu != null) q.Add(cu);
            q.Sort((a, b) => a.number.CompareTo(b.number));
            return q;
        }

        static bool ServeSomeone(Ctx c)
        {
            GameSession s = c.s;
            Customer boss = s.Boss;
            if (boss != null && ServeBoss(c, boss)) return true;

            List<Customer> queue = Queue(s);
            queue.RemoveAll(x => x.isBoss);
            if (queue.Count == 0) return false;

            if (!c.Good)
            {
                // Giong NaiveBot: theo thu tu khach den; mon khach dau khong co thi khach ke tiep.
                foreach (Customer cu in queue)
                {
                    Dish d = PickDish(c, cu);
                    if (d == null && CookOrder(c, cu.orderId)) d = PickDish(c, cu);
                    if (d != null) { Serve(c, cu, d); return true; }
                }
                return false;
            }

            // sim key(): uu tien khach co mon dat yeu cau (khach kho nhat truoc), khong thi khach de tinh nhat.
            Customer pick = null; int bestGroup = int.MaxValue, bestE = 0; bool pickReady = false;
            foreach (Customer cu in queue)
            {
                Dish d = PickDish(c, cu);
                int tier;
                bool ready = d != null;
                if (ready) tier = d.tier;
                else if (!PredictCook(c, cu.orderId, out tier)) continue;
                int e = s.EasinessNow(cu);
                bool good = Capped(s, tier) >= Required(s, cu);
                int group = good ? 0 : 1;
                int key = good ? e : -e;
                if (group < bestGroup || (group == bestGroup && key < bestE))
                {
                    pick = cu; bestGroup = group; bestE = key; pickReady = ready;
                }
            }
            if (pick == null) return false;
            Dish dish = pickReady ? PickDish(c, pick) : null;
            if (dish == null)
            {
                if (!CookOrder(c, pick.orderId)) return false;
                dish = PickDish(c, pick);
                if (dish == null) return false;
            }
            Serve(c, pick, dish);
            return true;
        }

        static void Serve(Ctx c, Customer cu, Dish d)
        {
            GameSession s = c.s;
            if (s.Fridge.Contains(d)) Spend(c, c.timing.fridgeSeconds);
            if (!cu.isBoss && Patience.Applies(s.Balance, s.Day))
            {
                c.run.timedServes++;
                int drop = cu.easiness - s.EasinessNow(cu);
                c.run.patienceDrop += drop;
                if (drop > 0) c.run.impatientServes++;
            }
            PaymentResult p = s.ServeSeat(s.SeatOf(cu), d);
            if (p != null && p.wrongOrder) c.run.wrongPenalty -= p.amount;
            if (p != null && cu.isBoss)
            {
                c.run.bossServed = true;
                c.run.bossServedMinute = (int)(s.Clock.Elapsed / s.Balance.day.secondsPerHour * 60f);
                c.run.bossTier = Math.Min(d.tier, s.Balance.dish.maxTier);
                c.run.bossBonus = p.bossBonus;
            }
        }

        /// <summary>Chu no ngoi: dua dia tot nhat (dia de danh trong tu lanh); khong co thi nau mon tot nhat lam duoc roi dua.</summary>
        static bool ServeBoss(Ctx c, Customer boss)
        {
            GameSession s = c.s;
            Dish best = BestDish(s);
            if (best == null || best.tier < s.Balance.dish.maxTier)
            {
                Dish cooked = CookBest(c);
                PullResult pull;
                // kho het ma con ve: quay them de co mon cho chu no
                while (cooked == null && best == null && CanCook(s) && s.PullCrates(out pull))
                {
                    Spend(c, c.timing.pullSeconds);
                    cooked = CookBest(c);
                }
                if (cooked != null && (best == null || cooked.tier > best.tier)) best = cooked;
            }
            if (best == null) return false;
            Serve(c, boss, best);
            return true;
        }

        static Dish BestDish(GameSession s)
        {
            Dish best = null;
            foreach (Dish d in ReadyDishes(s)) if (best == null || d.tier > best.tier) best = d;
            return best;
        }

        /// <summary>
        /// Khong phuc vu duoc ai. Ngay chu no: chua toi 20:00 thi cho; chu no dang doi ghe ma ba ghe kin thi dua dia bat ky
        /// cho khach den som nhat de trong ghe (khach gian, -30% gia goc). True = da lam gi do, vong lap tiep tuc.
        /// </summary>
        static bool WaitForBoss(Ctx c)
        {
            GameSession s = c.s;
            if (!s.IsBossDay || s.BossServed) return false;
            if (!s.BossArrived)
            {
                float left = (s.Balance.day.bossHour - s.Balance.day.openHour) * s.Balance.day.secondsPerHour - s.Clock.Elapsed;
                s.Tick(Math.Max(0.5f, Math.Min(10f, left)));
                return true;
            }
            if (s.BossWaiting)
            {
                List<Customer> q = Queue(s);
                Dish reserved = Reserved(c);
                Dish any = null;
                foreach (Dish d in ReadyDishes(s))
                    if (!ReferenceEquals(d, reserved) && (any == null || d.tier < any.tier)) any = d;
                if (any == null) any = CookAny(c);
                PullResult pull;
                // het do: con ve thi quay them de co dia lam trong ghe
                while (any == null && CanCook(s) && s.PullCrates(out pull))
                {
                    Spend(c, c.timing.pullSeconds);
                    any = CookAny(c);
                }
                if (any == null || q.Count == 0) return false;
                c.run.wrongForSeat++;
                Serve(c, q[0], any);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Nguoi choi tot, ca ba ghe ngoi khach co mon da het (Q-39), truoc 21:00 va kho con du it nhat 2 mon: dua mot dia
        /// re nhat cho khach den som nhat (khach gian, -30% gia goc) de trong ghe cho khach moi goi mon lam duoc.
        /// </summary>
        static bool FreeSeat(Ctx c)
        {
            GameSession s = c.s;
            if (s.IsClosedForNewCustomers) return false;
            int mains = 0, sides = 0;
            foreach (Ingredient i in s.Stock.Items) { if (i.kind == IngredientKind.Main) mains++; else sides++; }
            Dish reserved = Reserved(c);
            Dish cheap = null; int ready = 0;
            foreach (Dish d in ReadyDishes(s))
            {
                if (ReferenceEquals(d, reserved)) continue;
                ready++;
                if (cheap == null || d.tier < cheap.tier) cheap = d;
            }
            if (Math.Min(mains, sides) + ready < 2) return false;
            List<Customer> q = Queue(s);
            q.RemoveAll(x => x.isBoss);
            if (q.Count == 0) return false;
            if (cheap == null) cheap = CookAny(c);
            if (cheap == null || !s.HasDish(cheap) || s.Phase != GamePhase.Open) return false;
            c.run.wrongForSeat++;
            Serve(c, q[0], cheap);
            return true;
        }

        // ---------------- nau ----------------

        /// <summary>
        /// Constitution 1.7 muc 6.2: dong ho chi chay tu luc Mo quan, nen viec lam truoc khi mo khong mat gio (sim_balance.py
        /// PREOPEN). Nguoi tot so che san moi nguyen lieu trong kho (chua biet khach goi gi nen khong nau san: dia khong ai goi
        /// se chiem quay cho - do o sprint 7, nau san 2 dia lam bot gom bo tut 52% -> 27%); naive nau 1 mon nhu huong dan ngay 1.
        /// </summary>
        static void PreCook(Ctx c)
        {
            GameSession s = c.s;
            if (!c.Good)
            {
                if (s.Phase == GamePhase.Prep && CanCook(s) && CookAny(c) != null) c.run.preCooked++;
                return;
            }
            var all = new List<Ingredient>(s.Stock.Items);
            foreach (Ingredient i in all)
                if (s.Phase == GamePhase.Prep && !i.prepared && Prepare(s, i)) c.run.prePrepped++;
        }

        /// <summary>Nguyen lieu cho mon: nguoi tot lay cai bac cao nhat, naive lay cai dau tien trong kho.</summary>
        static bool FindPair(Ctx c, Recipe want, out Ingredient main, out Ingredient side)
        {
            main = null; side = null;
            foreach (Ingredient i in c.s.Stock.Items)
            {
                if (i.kind == IngredientKind.Main && i.typeId == want.mainId && (main == null || (c.Good && i.tier > main.tier))) main = i;
                if (i.kind == IngredientKind.Side && i.typeId == want.sideId && (side == null || (c.Good && i.tier > side.tier))) side = i;
            }
            return main != null && side != null;
        }

        static bool CanCook(GameSession s)
        {
            return !s.Counter.IsFull && s.Stove.State == StoveState.Empty;
        }

        /// <summary>Bac du doan neu nau mon nay tu kho luc nay (phan nguyen cua b, khong quay them thung).</summary>
        static bool PredictCook(Ctx c, string recipeId, out int tier)
        {
            tier = 0;
            Recipe want = recipeId == null ? null : c.s.Lookup.ById(recipeId);
            if (want == null || !CanCook(c.s)) return false;
            Ingredient m, p;
            // nguoi choi tot khong dot ve quay may cho mot mon (mot cap loai trung chi ~1/80 o vong 4)
            if (!FindPair(c, want, out m, out p)) return false;
            tier = Capped(c.s, (int)Math.Floor(c.s.Formula.Score(m.tier, p.tier)));
            return true;
        }

        /// <summary>Nau dung mon khach goi; thieu nguyen lieu thi quay them thung (nhu NaiveBot.CookFor).</summary>
        static bool CookOrder(Ctx c, string recipeId)
        {
            GameSession s = c.s;
            Recipe want = recipeId == null ? null : s.Lookup.ById(recipeId);
            if (want == null || !CanCook(s)) return false;
            while (true)
            {
                Ingredient main, side;
                if (FindPair(c, want, out main, out side)) return Cook(c, want, main, side) != null;
                PullResult pull;
                if (!s.PullCrates(out pull)) return false;
                Spend(c, c.timing.pullSeconds);
            }
        }

        /// <summary>Nau mon co bac du doan cao nhat tu kho (cho chu no).</summary>
        static Dish CookBest(Ctx c)
        {
            GameSession s = c.s;
            if (!CanCook(s)) return null;
            Recipe bestR = null; Ingredient bm = null, bp = null; double bestScore = double.MinValue;
            foreach (Recipe r in s.MakeableRecipes())
            {
                Ingredient m, p;
                if (!FindPair(c, r, out m, out p)) continue;
                double sc = s.Formula.Score(m.tier, p.tier);
                if (sc > bestScore) { bestScore = sc; bestR = r; bm = m; bp = p; }
            }
            if (bestR == null) return null;
            Dish d = Cook(c, bestR, bm, bp);
            // chu no can Tuyet hao: dung nuoc sot len mon con thieu (nguoi tot)
            SauceResult sr;
            while (d != null && c.Good && d.tier < s.Balance.dish.maxTier && d.tier >= SauceTierGood && s.Items.Sauces > 0
                   && s.HasDish(d) && s.UseSauce(d, out sr))
                if (sr == SauceResult.Legend) c.run.legendDishes++;
            return d != null && s.HasDish(d) ? d : null;
        }

        static Dish CookAny(Ctx c)
        {
            GameSession s = c.s;
            if (!CanCook(s)) return null;
            foreach (Recipe r in s.MakeableRecipes())
            {
                Ingredient m, p;
                if (FindPair(c, r, out m, out p)) return Cook(c, r, m, p);
            }
            return null;
        }

        static Dish Cook(Ctx c, Recipe recipe, Ingredient main, Ingredient side)
        {
            GameSession s = c.s;
            // so che san (truoc khi mo quan) thi khong ton gio so che nua
            float prepTime = (main.prepared ? 0f : main.prepSeconds) + (side.prepared ? 0f : side.prepSeconds);
            if (!Prepare(s, main) || !Prepare(s, side)) return null;
            if (!s.Stove.SelectTool(recipe.tool)) return null;
            foreach (string step in recipe.steps)
            {
                bool ok;
                if (step == main.name) ok = s.AddToStove(main);
                else if (step == side.name) ok = s.AddToStove(side);
                else ok = s.Stove.AddPantry(step);
                if (!ok) return null;
            }
            if (!s.Stove.Start()) return null;
            s.Stove.Tick(s.Stove.CookSeconds);
            Dish dish;
            if (!s.PlateDish(out dish)) return null;

            CookTiming t = s.Recipes.timing;
            s.Tick(prepTime + t.stepSeconds * recipe.steps.Length
                + recipe.cookSeconds + t.plateSeconds + c.timing.overheadSeconds);
            if (s.Phase != GamePhase.Open && s.Phase != GamePhase.Prep) return null;
            AfterPlate(c, dish);
            return s.HasDish(dish) ? dish : null;
        }

        /// <summary>sim_balance.py: nuoc sot (nguoi tot: mon Ngon tu vong 3 hoac can mon top cho chu no), may gacha (mon bac thap), de danh mon top.</summary>
        static void AfterPlate(Ctx c, Dish dish)
        {
            GameSession s = c.s;
            if (!c.Good) return;
            SauceResult sr;
            bool needTop = s.Day >= NeedTopFromDay && Reserved(c) == null;
            if (dish.tier == SauceTierGood && s.Items.Sauces > 0 && (needTop || s.Round >= SauceRoundGood) && s.UseSauce(dish, out sr)
                && sr == SauceResult.Legend) c.run.legendDishes++;
            MachineResult mr;
            if (s.HasDish(dish) && dish.tier <= MachineMaxTier && s.Items.HasMachine && s.Items.MachineUsesLeft > 0)
                s.UseMachine(dish, out mr);
            // tu ngay 17, giu mot dia Tuyet hao trong tu lanh cho chu no ngay 21
            if (s.HasDish(dish) && s.Day >= KeepTopFromDay && s.Day < s.Balance.day.bossDay
                && dish.tier >= s.Balance.dish.maxTier && Reserved(c) == null && !s.Fridge.IsFull)
                s.StoreInFridge(dish);
        }

        static bool Prepare(GameSession s, Ingredient ingredient)
        {
            if (ingredient.prepared) return true;
            if (!s.BeginPrep(ingredient)) return false;
            s.HoldPrep(ingredient.prepSeconds);
            return ingredient.prepared;
        }
    }

    /// <summary>Thong ke cung dang bang voi balance/sim_balance.py stats() (F-045 AC-2).</summary>
    public class BotStats
    {
        static readonly int[] Marks = { 4, 9, 15, 22 };

        public BotStyle Style { get; private set; }
        public int Games { get; private set; }
        int wins, combo3, comboWins, reachedBoss, bossBonus, legend, earlyCloses, wrongForSeat, bossMissed, hadReserve, reserveBonus;
        readonly int[] bossTiers = new int[5];
        long wrongPenalty, timedServes, patienceDrop, impatient, timedDays, preCooked, prePrepped;
        long days, served, progressSum;
        readonly int[] pass = new int[Marks.Length];
        readonly Dictionary<int, List<int>> money = new Dictionary<int, List<int>>();
        readonly Dictionary<int, List<KeyValuePair<int, string>>> starts = new Dictionary<int, List<KeyValuePair<int, string>>>();

        public BotStats(BotStyle style)
        {
            Style = style;
        }

        public static string Name(BotStyle style)
        {
            switch (style)
            {
                case BotStyle.Naive: return "naive";
                case BotStyle.NoCombo: return "nocombo";
                default: return "smart";
            }
        }

        public void Add(GameSession s, BotRun r)
        {
            Games++;
            if (r.won) wins++;
            int day = Math.Min(r.lastDay, 22);
            progressSum += day;
            for (int i = 0; i < Marks.Length; i++) if (r.won || day > Marks[i]) pass[i]++;
            if (r.maxSetCount >= 3) { combo3++; if (r.won) comboWins++; }
            if (r.reachedBossDay) reachedBoss++;
            if (r.bossBonus > 0) bossBonus++;
            wrongPenalty += r.wrongPenalty;
            timedServes += r.timedServes;
            patienceDrop += r.patienceDrop;
            impatient += r.impatientServes;
            timedDays += r.timedDays;
            preCooked += r.preCooked;
            prePrepped += r.prePrepped;
            if (r.reachedBossDay && !r.bossServed) bossMissed++;
            if (r.bossServed) bossTiers[r.bossTier]++;
            if (r.bossDayHadReserve) { hadReserve++; if (r.bossBonus > 0) reserveBonus++; }
            legend += r.legendDishes;
            earlyCloses += r.earlyCloses;
            wrongForSeat += r.wrongForSeat;
            days += r.daysPlayed;
            served += r.served;
            foreach (KeyValuePair<int, KeyValuePair<int, string>> kv in r.startOf)
            {
                List<KeyValuePair<int, string>> list;
                if (!starts.TryGetValue(kv.Key, out list)) starts[kv.Key] = list = new List<KeyValuePair<int, string>>();
                list.Add(kv.Value);
            }
            foreach (KeyValuePair<int, int> kv in r.moneyBeforeDebt)
            {
                List<int> list;
                if (!money.TryGetValue(kv.Key, out list)) money[kv.Key] = list = new List<int>();
                list.Add(kv.Value);
            }
        }

        public double Win { get { return Games == 0 ? 0 : (double)wins / Games; } }
        public double Progress { get { return Games == 0 ? 0 : progressSum / 22.0 / Games; } }
        public double Pass(int i) { return Games == 0 ? 0 : (double)pass[i] / Games; }
        public double Combo { get { return Games == 0 ? 0 : (double)combo3 / Games; } }
        public double WinGivenCombo { get { return (double)comboWins / Math.Max(1, combo3); } }

        static string F(double v) { return v.ToString("0.00", CultureInfo.InvariantCulture); }

        /// <summary>Mot dong giong het sim_balance.py: "naive  win=0.63 progress=0.85 pass(d4,d9,d15,d22)=[...] combo3+=0.00 win|combo=0.00".</summary>
        public string Line()
        {
            var p = new List<string>();
            for (int i = 0; i < Marks.Length; i++) p.Add(Pass(i).ToString("0.0#", CultureInfo.InvariantCulture));
            return Name(Style).PadRight(6) + " win=" + F(Win) + " progress=" + F(Progress) + " pass(d4,d9,d15,d22)=[" + string.Join(", ", p.ToArray())
                + "] combo3+=" + F(Combo) + " win|combo=" + F(WinGivenCombo);
        }

        /// <summary>Tien dau ngay thu no: p5, p10, p20, p50, p80, so van (nhu sim_balance.py EXTRA).</summary>
        public string MoneyLine()
        {
            var parts = new List<string>();
            foreach (int d in Marks)
            {
                List<int> v;
                if (!money.TryGetValue(d, out v) || v.Count == 0) { parts.Add(d + ": -"); continue; }
                v.Sort();
                var q = new List<string>();
                foreach (double f in new[] { 0.05, 0.1, 0.2, 0.5, 0.8 }) q.Add(v[(int)(v.Count * f)].ToString(CultureInfo.InvariantCulture));
                parts.Add(d + ": [" + string.Join(", ", q.ToArray()) + ", n=" + v.Count + "]");
            }
            return string.Join(" · ", parts.ToArray());
        }

        /// <summary>Dau ngay 9 / 15 / 21: tien trung vi va relic cua van trung vi (de dung trang thai go loi).</summary>
        public string StartLine()
        {
            var parts = new List<string>();
            foreach (int d in BotRun.StartDays)
            {
                List<KeyValuePair<int, string>> v;
                if (!starts.TryGetValue(d, out v) || v.Count == 0) continue;
                v.Sort((a, b) => a.Key.CompareTo(b.Key));
                KeyValuePair<int, string> mid = v[v.Count / 2];
                parts.Add("ngay " + d + ": " + mid.Key + "G [" + mid.Value + "] (n=" + v.Count + ")");
            }
            return string.Join(" · ", parts.ToArray());
        }

        public string Extra()
        {
            return "khach/ngay=" + (days > 0 ? (double)served / days : 0).ToString("0.0", CultureInfo.InvariantCulture)
                + " toi ngay 21=" + reachedBoss + " thuong chu no=" + bossBonus + " (" + (reachedBoss > 0 ? 100.0 * bossBonus / reachedBoss : 0).ToString("0", CultureInfo.InvariantCulture) + "%)"
                + " | chu no: khong duoc phuc vu=" + bossMissed + ", bac mon dua [0..4]=[" + string.Join(",", Array.ConvertAll(bossTiers, x => x.ToString(CultureInfo.InvariantCulture))) + "]"
                + ", dau ngay 21 co dia Tuyet hao trong tu=" + hadReserve + " (trong do duoc thuong " + reserveBonus + ") |"
                + " diem de tinh mat vi cho=" + (timedServes > 0 ? (double)patienceDrop / timedServes : 0).ToString("0.0", CultureInfo.InvariantCulture)
                + " tien phat sai mon / van=" + (Games > 0 ? wrongPenalty / Games : 0)
                + " mon Hy Vong=" + legend + " dong quan som=" + earlyCloses + " dua dia de trong ghe cho chu no=" + wrongForSeat
                + " | khach bi giam de tinh / ngay (vong 2-4)=" + ImpatientPerDay.ToString("0.0", CultureInfo.InvariantCulture)
                + " nau san / ngay=" + (days > 0 ? (double)preCooked / days : 0).ToString("0.0", CultureInfo.InvariantCulture)
                + " so che san / ngay=" + (days > 0 ? (double)prePrepped / days : 0).ToString("0.0", CultureInfo.InvariantCulture);
        }

        /// <summary>F-060 AC-6: so khach bi giam de tinh vi cho moi ngay o vong 2-4.</summary>
        public double ImpatientPerDay { get { return timedDays > 0 ? (double)impatient / timedDays : 0; } }

        /// <summary>Doc dong ket qua cua sim_balance.py (win, pass d4..d22); null neu khong co.</summary>
        public static double[] ParseSim(string simOutput, BotStyle style)
        {
            if (string.IsNullOrEmpty(simOutput)) return null;
            string name = Name(style) + " ";
            foreach (string raw in simOutput.Split('\n'))
            {
                string line = raw.Trim();
                if (!line.StartsWith(name, StringComparison.Ordinal) || !line.Contains("win=")) continue;
                try
                {
                    var r = new double[5];
                    r[0] = double.Parse(Between(line, "win=", " "), CultureInfo.InvariantCulture);
                    string[] p = Between(line, "pass(d4,d9,d15,d22)=[", "]").Split(',');
                    for (int i = 0; i < 4; i++) r[i + 1] = double.Parse(p[i].Trim(), CultureInfo.InvariantCulture);
                    return r;
                }
                catch (FormatException) { return null; }
            }
            return null;
        }

        static string Between(string s, string a, string b)
        {
            int i = s.IndexOf(a, StringComparison.Ordinal) + a.Length;
            int j = s.IndexOf(b, i, StringComparison.Ordinal);
            return j < 0 ? s.Substring(i) : s.Substring(i, j - i);
        }

        /// <summary>Bao cao markdown: bang bot, so voi sim (neu co file sim), chenh lech tung moc.</summary>
        public static string Report(BotStats[] all, int games, double seconds, string simOutput = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Bot C# tren luat that — " + games + " van moi kieu (" + seconds.ToString("0", CultureInfo.InvariantCulture) + " giay)");
            sb.AppendLine();
            sb.AppendLine("```");
            foreach (BotStats st in all) { sb.AppendLine(st.Line()); sb.AppendLine("        " + st.Extra()); sb.AppendLine("        tien truoc thu no " + st.MoneyLine()); sb.AppendLine("        dau ngay (trung vi) " + st.StartLine()); }
            sb.AppendLine("```");
            sb.AppendLine();
            if (simOutput != null)
            {
                sb.AppendLine("## So voi sim_balance.py (diem %, bot - mo phong)");
                sb.AppendLine();
                sb.AppendLine("| Kieu | Moc | Bot | Mo phong | Chenh |");
                sb.AppendLine("| --- | --- | --- | --- | --- |");
                string[] labels = { "thang", "qua d4", "qua d9", "qua d15", "qua d22" };
                foreach (BotStats st in all)
                {
                    double[] sim = ParseSim(simOutput, st.Style);
                    if (sim == null) continue;
                    double[] bot = { st.Win, st.Pass(0), st.Pass(1), st.Pass(2), st.Pass(3) };
                    for (int i = 0; i < 5; i++)
                    {
                        double diff = 100 * (bot[i] - sim[i]);
                        sb.AppendLine("| " + Name(st.Style) + " | " + labels[i] + " | " + (100 * bot[i]).ToString("0.0", CultureInfo.InvariantCulture)
                            + " | " + (100 * sim[i]).ToString("0", CultureInfo.InvariantCulture) + " | " + diff.ToString("+0.0;-0.0;0", CultureInfo.InvariantCulture)
                            + (Math.Abs(diff) > 5 ? " **> 5**" : "") + " |");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
