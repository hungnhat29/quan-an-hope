using System;

namespace QuanAnHope.Core
{
    /// <summary>Cac cau hoi tra cuu tren <see cref="Balance"/>. Khong chua con so nao.</summary>
    public static class BalanceRules
    {
        const string StormTableId = "storm";
        const string RoundTablePrefix = "r";

        /// <summary>Vong cua mot ngay. Ngay sau vong cuoi (ngay tra no cuoi) tinh vao vong cuoi.</summary>
        public static int RoundOfDay(this Balance b, int day)
        {
            if (day < 1) throw new ArgumentOutOfRangeException("day", "Ngay bat dau tu 1");
            RoundRange last = null;
            foreach (RoundRange r in b.rounds)
            {
                if (day >= r.firstDay && day <= r.lastDay) return r.round;
                if (last == null || r.lastDay > last.lastDay) last = r;
            }
            if (last == null) throw new InvalidOperationException("Balance khong co vong nao");
            return last.round;
        }

        public static bool IsStormDay(this Balance b, int day)
        {
            return day == b.stormDay;
        }

        /// <summary>Bang ti le bac nguyen lieu cua ngay: bang sau mua, hoac bang cua vong.</summary>
        public static TierTable OddsForDay(this Balance b, int day)
        {
            string id = b.IsStormDay(day) ? StormTableId : RoundTablePrefix + b.RoundOfDay(day);
            foreach (TierTable t in b.ingredientOdds)
                if (t.id == id) return t;
            throw new InvalidOperationException("Khong co bang ti le \"" + id + "\" trong balance.json");
        }

        public static int SideCrateSize(this Balance b, int day)
        {
            return b.IsStormDay(day) ? b.crateSize.phuStorm : b.crateSize.phu;
        }

        public static int MainCrateSize(this Balance b, int day)
        {
            return b.IsStormDay(day) ? b.crateSize.chinhStorm : b.crateSize.chinh;
        }

        public static int BasePrice(this Balance b, int round)
        {
            return ByRound(b.dish.basePriceByRound, round, "gia goc");
        }

        /// <summary>So khach cua ngay theo vong. Ngoai le ngay 14 se them khi lam vong 3 (sprint 4).</summary>
        public static int CustomersPerDay(this Balance b, int day)
        {
            return ByRound(b.customers.perDayByRound, b.RoundOfDay(day), "so khach");
        }

        /// <summary>Bac mon toi thieu ma khach co do de tinh nay chap nhan (constitution muc 6.4).</summary>
        public static int RequiredTierFor(this Balance b, int easiness)
        {
            RequiredTier best = null;
            RequiredTier lowest = null;
            foreach (RequiredTier r in b.customers.requiredTier)
            {
                if (lowest == null || r.minEasy < lowest.minEasy) lowest = r;
                if (easiness >= r.minEasy && (best == null || r.minEasy > best.minEasy)) best = r;
            }
            if (best != null) return best.tier;
            if (lowest != null) return lowest.tier;
            throw new InvalidOperationException("Balance khong co bang mon toi thieu");
        }

        static int ByRound(int[] values, int round, string what)
        {
            if (round < 1 || round > values.Length)
                throw new ArgumentOutOfRangeException("round", "Khong co " + what + " cho vong " + round);
            return values[round - 1];
        }
    }
}
