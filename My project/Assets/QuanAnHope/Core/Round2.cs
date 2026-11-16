using System;
using System.Collections.Generic;

namespace QuanAnHope.Core
{
    /// <summary>Thoi gian cho cua khach (constitution muc 6.6, relic Dong ho cat / May pha ca phe muc 7.3).</summary>
    public static class Patience
    {
        /// <summary>Ngay nay co tinh gio cho khong (tu vong 2).</summary>
        public static bool Applies(Balance b, int day)
        {
            return b.RoundOfDay(day) >= b.patience.fromRound;
        }

        /// <summary>Khoang an toan (giay thuc) cua ngay, da cong relic; vo cung neu ngay nay khong tinh gio.</summary>
        public static float SafeSeconds(Balance b, int day, RelicFx fx)
        {
            if (!Applies(b, day)) return float.PositiveInfinity;
            int round = b.RoundOfDay(day);
            float[] byRound = b.patience.safeSecondsByRound;
            float safe = round >= 1 && round <= byRound.Length ? byRound[round - 1] : 0f;
            if (b.IsBlackoutDay(day)) safe += b.patience.blackoutExtraSeconds;
            return safe * (1f + (fx ?? RelicFx.None).Get(RelicKeys.Patience));
        }

        /// <summary>So diem do de tinh bi tru sau khi khach cho <paramref name="waited"/> giay.</summary>
        public static int Drop(Balance b, int day, float waited, RelicFx fx)
        {
            float safe = SafeSeconds(b, day, fx);
            if (float.IsInfinity(safe) || waited <= safe) return 0;
            int drop = (int)Math.Floor((waited - safe) / b.patience.dropEverySeconds + 1e-4);
            return Math.Min(b.patience.maxDrop, drop);
        }

        /// <summary>Do de tinh luc nay (khong duoi 1, nhu sim_balance.py pay(): max(1, E - impatience)).</summary>
        public static int EasinessNow(Balance b, int day, Customer c, RelicFx fx)
        {
            int drop = Drop(b, day, c.waitedSeconds, fx);
            return drop == 0 ? c.easiness : Math.Max(1, c.easiness - drop);
        }

        /// <summary>
        /// Thanh kien nhan tren bong bong, 1 = day, 0 = da bi tru toi da: phan dau la khoang an toan,
        /// phan sau la 20 diem co the bi tru (Art Bible muc 13: thanh tien do).
        /// </summary>
        public static float Left(Balance b, int day, Customer c, RelicFx fx)
        {
            float safe = SafeSeconds(b, day, fx);
            if (float.IsInfinity(safe)) return 1f;
            float total = safe + b.patience.maxDrop * b.patience.dropEverySeconds;
            if (total <= 0f) return 0f;
            return Math.Max(0f, Math.Min(1f, 1f - c.waitedSeconds / total));
        }

        /// <summary>Khach da qua khoang an toan (thanh doi mau).</summary>
        public static bool IsImpatient(Balance b, int day, Customer c, RelicFx fx)
        {
            return c.waitedSeconds > SafeSeconds(b, day, fx);
        }
    }

    /// <summary>Tu lanh: giu dia mon qua ngay (constitution muc 5.1, 5.7; relic Tu lanh 2 cua muc 7.3).</summary>
    public class Fridge
    {
        readonly Balance balance;
        readonly RelicInventory relics;
        readonly List<Dish> dishes = new List<Dish>();

        public Fridge(Balance balance, RelicInventory relics)
        {
            this.balance = balance;
            this.relics = relics;
        }

        public IReadOnlyList<Dish> Dishes { get { return dishes; } }
        public int Capacity { get { return balance.fridge.slots + (int)Fx.Get(RelicKeys.Fridge); } }
        public bool IsFull { get { return dishes.Count >= Capacity; } }
        public bool NoDecay { get { return Fx.Get(RelicKeys.NoDecay) > 0f; } }

        RelicFx Fx { get { return relics != null ? relics.Effects : RelicFx.None; } }

        public bool Contains(Dish d)
        {
            return d != null && dishes.Contains(d);
        }

        public bool TryStore(Dish d)
        {
            if (d == null || IsFull || dishes.Contains(d)) return false;
            d.daysInFridge = 0;
            dishes.Add(d);
            return true;
        }

        public bool Remove(Dish d)
        {
            return dishes.Remove(d);
        }

        /// <summary>Sang ngay moi: moi dia them 1 ngay; qua <see cref="FridgeRules.decayDays"/> ngay thi tut 1 bac (mot lan).</summary>
        public void AgeOneDay()
        {
            foreach (Dish d in dishes)
            {
                d.daysInFridge++;
                if (!NoDecay && !d.decayed && d.daysInFridge > balance.fridge.decayDays)
                {
                    d.tier = Math.Max(0, d.tier - 1);
                    d.decayed = true;
                }
            }
        }
    }

    public enum ItemBuyResult
    {
        Bought,
        Closed,
        NotEnoughMoney,
        AtLimit,
    }

    /// <summary>Ket qua mot lan dung nuoc sot (thu tu = cot bang constitution muc 5.5).</summary>
    public enum SauceResult
    {
        Fail,
        Same,
        Up1,
        Up2,
        Legend,
    }

    /// <summary>Ket qua mot lan cho dia vao may gacha do an (constitution muc 5.6).</summary>
    public enum MachineResult
    {
        Trash,
        Down1,
        Same,
        Up1,
        Up2,
        Up3,
    }

    /// <summary>Do da mua o cua hang vat pham (constitution muc 8). Tien nam o GameSession.</summary>
    public class ItemShop
    {
        readonly Balance balance;

        public ItemShop(Balance balance)
        {
            this.balance = balance;
        }

        public int Day { get; private set; }
        public bool IsOpen { get { return Day >= balance.itemShopFromDay; } }

        public int Sauces { get; private set; }
        public int SaucesBought { get; private set; }
        public int SaucePrice { get { return balance.sauce.price + balance.sauce.step * SaucesBought; } }

        public int BlackTickets { get; private set; }
        public int BlackBought { get; private set; }
        public int BlackPrice { get { return balance.blackTicket.price + balance.blackTicket.step * BlackBought; } }

        public bool HasMachine { get; private set; }
        public int MachineUsesLeft { get; private set; }

        public void BeginDay(int day)
        {
            Day = day;
            MachineUsesLeft = balance.machine.usesPerDay;
        }

        /// <summary>Kiem tra mua duoc khong; tra ve gia qua <paramref name="price"/>.</summary>
        ItemBuyResult Check(int money, int price, bool atLimit, out int cost)
        {
            cost = price;
            if (!IsOpen) return ItemBuyResult.Closed;
            if (atLimit) return ItemBuyResult.AtLimit;
            if (money < price) return ItemBuyResult.NotEnoughMoney;
            return ItemBuyResult.Bought;
        }

        public ItemBuyResult CheckSauce(int money, out int cost) { return Check(money, SaucePrice, Sauces >= balance.sauce.max, out cost); }
        public ItemBuyResult CheckMachine(int money, out int cost) { return Check(money, balance.machine.price, HasMachine, out cost); }
        public ItemBuyResult CheckBlack(int money, out int cost) { return Check(money, BlackPrice, BlackTickets >= balance.blackTicket.max, out cost); }

        internal void AddSauce() { Sauces++; SaucesBought++; }
        internal void AddMachine() { HasMachine = true; }
        internal void AddBlack() { BlackTickets++; BlackBought++; }

        internal bool SpendSauce() { if (Sauces <= 0) return false; Sauces--; return true; }
        internal bool SpendBlack() { if (BlackTickets <= 0) return false; BlackTickets--; return true; }
        internal bool SpendMachineUse() { if (!HasMachine || MachineUsesLeft <= 0) return false; MachineUsesLeft--; return true; }
    }

    /// <summary>Quay nuoc sot va may gacha do an theo bang trong du lieu (Dieu 2, Dieu 3).</summary>
    public static class ItemRolls
    {
        /// <summary>Ti le nuoc sot cua ngay (phan tram, thu tu SauceResult), da tru Hu sot cu: hong giam, giu nguyen tang (sim_balance.py sauce_roll).</summary>
        public static double[] SauceOdds(Balance b, int day, RelicFx fx)
        {
            SauceOdds o = day >= b.sauce.lateFromDay ? b.sauce.oddsLate : b.sauce.oddsEarly;
            double safe = Math.Min(o.fail, (double)(fx ?? RelicFx.None).Get(RelicKeys.SauceSafe) * 100.0);
            safe = Math.Round(safe, 4);
            return new[] { o.fail - safe, o.same + safe, (double)o.up1, o.up2, o.legend };
        }

        public static SauceResult Sauce(Balance b, int day, RelicFx fx, IRandom rng)
        {
            return (SauceResult)Pick(SauceOdds(b, day, fx), rng);
        }

        /// <summary>Bac sau nuoc sot; +1 / +2 toi da Tuyet hao, Huyen thoai la bac 5 (muc 5.4, sim_balance.py).</summary>
        public static int ApplySauce(SauceResult r, int tier, Balance b)
        {
            int max = b.dish.maxTier;
            switch (r)
            {
                case SauceResult.Fail: return 0;
                case SauceResult.Up1: return Math.Min(max, tier + 1);
                case SauceResult.Up2: return Math.Min(max, tier + 2);
                case SauceResult.Legend: return b.dish.priceMult.Length - 1;
                default: return tier;
            }
        }

        public static double[] MachineOdds(Balance b)
        {
            MachineRules m = b.machine;
            return new double[] { m.trash, m.down1, m.same, m.up1, m.up2, m.up3 };
        }

        public static MachineResult Machine(Balance b, IRandom rng)
        {
            return (MachineResult)Pick(MachineOdds(b), rng);
        }

        /// <summary>Bac sau may gacha; -1 = hong, mat mon (sim_balance.py machine_roll).</summary>
        public static int ApplyMachine(MachineResult r, int tier, Balance b)
        {
            int max = b.dish.maxTier;
            switch (r)
            {
                case MachineResult.Trash: return -1;
                case MachineResult.Down1: return Math.Max(0, tier - 1);
                case MachineResult.Up1: return Math.Min(max, tier + 1);
                case MachineResult.Up2: return Math.Min(max, tier + 2);
                case MachineResult.Up3: return Math.Min(max, tier + 3);
                default: return tier;
            }
        }

        static int Pick(double[] percents, IRandom rng)
        {
            double x = rng.NextDouble() * 100.0, acc = 0.0;
            int last = 0;
            for (int i = 0; i < percents.Length; i++)
            {
                if (percents[i] <= 0.0) continue;
                last = i;
                acc += percents[i];
                if (x < acc) return i;
            }
            return last;
        }
    }
}
