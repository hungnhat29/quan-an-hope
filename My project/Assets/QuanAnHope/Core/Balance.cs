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

        /// <summary>So relic toi da tren gia (constitution muc 7.1).</summary>
        public int relicCap;

        /// <summary>So relic cua hang bay moi ngay (constitution muc 7.1).</summary>
        public int relicShopOffers;

        /// <summary>22 relic tu balance/relics.json (constitution muc 7.2, 7.3).</summary>
        public RelicDef[] relics = new RelicDef[0];

        /// <summary>Thuong bo theo nguong so mon cung bo (constitution muc 7.2).</summary>
        public SetBonus[] setBonuses = new SetBonus[0];

        public BreakthroughRules breakthrough = new BreakthroughRules();

        // ---------- sprint 4: vong 2 ----------

        /// <summary>So ghe theo vong; chi so 0 = vong 1 (constitution muc 6.1, 6.2).</summary>
        public int[] seatsByRound = new int[0];

        /// <summary>Cua hang vat pham mo tu ngay nay (constitution muc 8).</summary>
        public int itemShopFromDay;
        public PatienceRules patience = new PatienceRules();
        public BlackoutRules blackout = new BlackoutRules();
        public FridgeRules fridge = new FridgeRules();
        public SauceRules sauce = new SauceRules();
        public MachineRules machine = new MachineRules();
        public PricedItemRules blackTicket = new PricedItemRules();
    }

    /// <summary>Thoi gian cho (constitution muc 6.6).</summary>
    [Serializable]
    public class PatienceRules
    {
        public int fromRound;

        /// <summary>Khoang an toan (giay thuc) theo vong; chi so 0 = vong 1 (0 neu vong do khong tinh gio).</summary>
        public float[] safeSecondsByRound = new float[0];
        public float dropEverySeconds;
        public int maxDrop;
        public float blackoutExtraSeconds;
    }

    /// <summary>Ngay mat dien (constitution muc 2, 5.3, 5.9; Art Bible muc 4).</summary>
    [Serializable]
    public class BlackoutRules
    {
        public int day;
        public float cookMultiplier;

        /// <summary>Phong toi bao nhieu phan (0..1).</summary>
        public float dim;
    }

    /// <summary>Tu lanh (constitution muc 5.7).</summary>
    [Serializable]
    public class FridgeRules
    {
        public int slots;
        public int decayDays;
    }

    /// <summary>Vat pham mua bang tien: gia dau, moi lan mua sau + step, giu toi da max (constitution muc 8).</summary>
    [Serializable]
    public class PricedItemRules
    {
        public int price;
        public int step;
        public int max;
    }

    /// <summary>Ti le mot lan dung nuoc sot (phan tram).</summary>
    [Serializable]
    public class SauceOdds
    {
        public float fail;
        public float same;
        public float up1;
        public float up2;
        public float legend;
    }

    /// <summary>Nuoc sot than thanh (constitution muc 5.5, 8).</summary>
    [Serializable]
    public class SauceRules : PricedItemRules
    {
        public int unlockDay;
        public int lateFromDay;
        public SauceOdds oddsEarly = new SauceOdds();
        public SauceOdds oddsLate = new SauceOdds();
    }

    /// <summary>May gacha do an (constitution muc 5.6, 8). Ti le an voi nguoi choi (Dieu 4).</summary>
    [Serializable]
    public class MachineRules
    {
        public int price;
        public int usesPerDay;
        public float trash;
        public float down1;
        public float same;
        public float up1;
        public float up2;
        public float up3;
    }

    /// <summary>Mot hieu ung relic: khoa (vd "phu_up") va gia tri, nhu trong relics.json.</summary>
    [Serializable]
    public class RelicEffect
    {
        public string key;
        public float value;
    }

    [Serializable]
    public class RelicDef
    {
        public string id;
        public string name;

        /// <summary>"vuon", "cho", "sonhao" hoac rong (relic le).</summary>
        public string set;
        public int price;

        /// <summary>"thuong", "hiem", "huyenthoai".</summary>
        public string rarity;
        public RelicEffect[] effects = new RelicEffect[0];
    }

    /// <summary>Co du <see cref="count"/> relic cua bo <see cref="set"/> thi duoc them cac hieu ung nay.</summary>
    [Serializable]
    public class SetBonus
    {
        public string set;
        public int count;
        public RelicEffect[] effects = new RelicEffect[0];
    }

    /// <summary>Thung dot pha (constitution muc 4.4).</summary>
    [Serializable]
    public class BreakthroughRules
    {
        public float baseChance;
        public int items;
        public float tier5Share;
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

        /// <summary>Ngay chu no den (constitution muc 2: ngay 21, sprint 5).</summary>
        public int bossDay;

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
