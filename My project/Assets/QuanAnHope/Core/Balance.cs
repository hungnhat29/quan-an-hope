using System;

namespace QuanAnHope.Core
{
    /// <summary>
    /// So lieu can bang cua game, doc tu balance/balance.json (constitution Dieu 2).
    /// Lop nay chi chua du lieu; khong co con so nao duoc viet san trong code.
    /// Sprint 1 chi nhap cac truong vong 1 dung toi; relic, nuoc sot, may gacha do an,
    /// ve den, tu lanh se them o sprint 2-3.
    /// </summary>
    [Serializable]
    public class Balance
    {
        public int days;
        public int ticketsPerDay;
        public int bossBonus;

        /// <summary>Ngay "sau mua": bang ti le va kich thuoc thung rieng (constitution muc 4.3).</summary>
        public int stormDay;

        /// <summary>So dia quay cho giu duoc (constitution muc 5.1).</summary>
        public int counterSlots;
        public PaymentRules payment = new PaymentRules();
        public DebtEntry[] debts = new DebtEntry[0];
        public RoundRange[] rounds = new RoundRange[0];
        public TierTable[] ingredientOdds = new TierTable[0];
        public CrateSize crateSize = new CrateSize();
        public DishRules dish = new DishRules();
        public CustomerRules customers = new CustomerRules();
        public StartRules start = new StartRules();
        public DayRules day = new DayRules();
    }

    /// <summary>Gio mo cua va nhip khach (constitution 1.4 muc 2, 6.2).</summary>
    [Serializable]
    public class DayRules
    {
        public int openHour;
        public int closeHour;

        /// <summary>So giay thuc cua mot gio trong game.</summary>
        public float secondsPerHour;

        /// <summary>Gio chu no den ngay 21 (sprint 4).</summary>
        public int bossHour;

        /// <summary>Vong 1: khach ke tiep vao sau bay nhieu giay tu luc khach truoc duoc phuc vu.</summary>
        public float nextCustomerSeconds;
    }

    /// <summary>No phai tra vao dau ngay <see cref="day"/>.</summary>
    [Serializable]
    public class DebtEntry
    {
        public int day;
        public int amount;
    }

    [Serializable]
    public class RoundRange
    {
        public int round;
        public int firstDay;
        public int lastDay;
    }

    /// <summary>Bang ti le bac (phan tram), chi so mang = bac 0..5.</summary>
    [Serializable]
    public class TierTable
    {
        public string id;
        public int[] percents = new int[0];
    }

    [Serializable]
    public class CrateSize
    {
        public int phu;
        public int chinh;
        public int phuStorm;
        public int chinhStorm;
    }

    [Serializable]
    public class DishRules
    {
        public float weightChinh;
        public float weightPhu;
        public float offset;
        public float blackoutPenalty;

        /// <summary>Bac cao nhat nau ra duoc (Tuyet hao); Huyen thoai chi ra qua nuoc sot.</summary>
        public int maxTier;

        /// <summary>He so gia theo bac mon 0..5.</summary>
        public float[] priceMult = new float[0];

        /// <summary>Gia goc theo vong; chi so 0 = vong 1.</summary>
        public int[] basePriceByRound = new int[0];
    }

    /// <summary>Tu 1.4 khong con so khach moi ngay: khach vao lien tuc toi gio dong cua (muc 6.2).</summary>
    [Serializable]
    public class CustomerRules
    {
        public RoundBands[] easyBands = new RoundBands[0];
        public RequiredTier[] requiredTier = new RequiredTier[0];
        public int bossEasy;
    }

    [Serializable]
    public class RoundBands
    {
        public int round;
        public EasyBand[] bands = new EasyBand[0];
    }

    /// <summary>Khoang do de tinh [min, max] va ti le khach roi vao khoang do (0..1).</summary>
    [Serializable]
    public class EasyBand
    {
        public int min;
        public int max;
        public float share;
    }

    /// <summary>Do de tinh tu <see cref="minEasy"/> tro len thi chap nhan mon tu bac <see cref="tier"/>.</summary>
    [Serializable]
    public class RequiredTier
    {
        public int minEasy;
        public int tier;
    }

    /// <summary>He so tra tien va tip (constitution muc 6.5).</summary>
    [Serializable]
    public class PaymentRules
    {
        public float tipBase;
        public float tipPerTier;
        public float underPayShare;
        public float penaltyBaseShare;
    }

    [Serializable]
    public class StartRules
    {
        public int money;
        public int bonusTickets;
        public int relicShopFromDay;
    }
}
