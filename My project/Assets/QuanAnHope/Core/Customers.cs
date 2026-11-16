using System;

namespace QuanAnHope.Core
{
    public class Customer
    {
        /// <summary>Do de tinh, phan tram; cang cao cang de (constitution muc 6.3).</summary>
        public int easiness;

        /// <summary>Mon khach goi (id cong thuc "chinh__phu") va ten mon (constitution muc 6.7).</summary>
        public string orderId;
        public string orderName;

        /// <summary>Khach thu may trong ngay (thu tu den).</summary>
        public int number;

        /// <summary>Giay thuc da ngoi cho (constitution muc 6.6).</summary>
        public float waitedSeconds;
    }

    /// <summary>Sinh khach theo khoang do de tinh cua vong (constitution muc 6.3).</summary>
    public class CustomerGenerator
    {
        readonly Balance balance;
        readonly IRandom rng;

        public CustomerGenerator(Balance balance, IRandom rng)
        {
            this.balance = balance;
            this.rng = rng;
        }

        public Customer Next(int round)
        {
            EasyBand[] bands = null;
            foreach (RoundBands rb in balance.customers.easyBands)
                if (rb.round == round) bands = rb.bands;
            if (bands == null || bands.Length == 0)
                throw new ArgumentOutOfRangeException("round", "Khong co khoang do de tinh cho vong " + round);

            double x = rng.NextDouble();
            double cumulative = 0.0;
            EasyBand chosen = null;
            foreach (EasyBand band in bands)
            {
                if (band.share <= 0f) continue;
                cumulative += band.share;
                chosen = band;
                if (x < cumulative) break;
            }

            return new Customer { easiness = rng.NextInt(chosen.min, chosen.max) };
        }
    }

    public enum PaymentOutcome
    {
        /// <summary>Mon dat hoac vuot yeu cau: tra du + tip.</summary>
        Satisfied,

        /// <summary>Mon thap hon yeu cau 1 bac: tra mot phan, khong tip.</summary>
        Disappointed,

        /// <summary>Mon thap hon yeu cau tu 2 bac: khong tra, quan den bu.</summary>
        Angry,
    }

    public class PaymentResult
    {
        public PaymentOutcome outcome;

        /// <summary>Bac mon tru bac toi thieu.</summary>
        public int diff;

        /// <summary>Gia cua mon (gia goc x he so bac), da lam tron.</summary>
        public int price;
        public int tip;

        /// <summary>So tien vi thay doi; am khi quan phai den bu.</summary>
        public int amount;

        /// <summary>Dia khac mon khach goi: khach nhan nhung gian (constitution muc 6.7).</summary>
        public bool wrongOrder;
    }

    /// <summary>Tra tien va tip (constitution muc 5.2, 6.5). Tien lam tron den 1G.</summary>
    public static class Payment
    {
        public static int Price(Balance b, int round, int dishTier)
        {
            return RoundMoney(RawPrice(b, round, dishTier));
        }

        /// <param name="fx">Relic (Chuong vang: tip x (1 + tip); Tuong meo: tien mon + tip x (1 + income), chi khi khach hai long,
        /// nhu sim_balance.py pay()). Null = khong co relic.</param>
        public static PaymentResult Settle(Balance b, int round, int easiness, int dishTier, RelicFx fx = null)
        {
            if (fx == null) fx = RelicFx.None;
            int required = b.RequiredTierFor(easiness);
            // Huyen thoai tinh nhu Tuyet hao khi so voi yeu cau (muc 6.4: "Huyen thoai cung dat").
            int diff = Math.Min(dishTier, b.dish.maxTier) - required;
            double price = RawPrice(b, round, dishTier);
            var result = new PaymentResult { diff = diff, price = RoundMoney(price) };

            if (diff >= 0)
            {
                double strictness = 1.0 - easiness / 100.0;
                double income = 1.0 + fx.Get(RelicKeys.Income);
                double tip = price * strictness * (b.payment.tipBase + (double)b.payment.tipPerTier * diff)
                    * (1.0 + fx.Get(RelicKeys.Tip));
                result.outcome = PaymentOutcome.Satisfied;
                result.price = RoundMoney(price * income);
                result.tip = RoundMoney(tip * income);
                result.amount = result.price + result.tip;
            }
            else if (diff == -1)
            {
                result.outcome = PaymentOutcome.Disappointed;
                result.amount = RoundMoney(b.payment.underPayShare * price);
            }
            else
            {
                result.outcome = PaymentOutcome.Angry;
                result.amount = -RoundMoney((double)b.payment.penaltyBaseShare * b.BasePrice(round));
            }
            return result;
        }

        /// <summary>Mang sai mon: khach nhan dia nhung khong tra, quan den bu nhu khach gian (constitution muc 6.5, 6.7).</summary>
        public static PaymentResult WrongOrder(Balance b, int round)
        {
            return new PaymentResult
            {
                outcome = PaymentOutcome.Angry,
                wrongOrder = true,
                amount = -RoundMoney((double)b.payment.penaltyBaseShare * b.BasePrice(round)),
            };
        }

        static double RawPrice(Balance b, int round, int dishTier)
        {
            float[] mult = b.dish.priceMult;
            if (dishTier < 0 || dishTier >= mult.Length)
                throw new ArgumentOutOfRangeException("dishTier", "Khong co he so gia cho bac " + dishTier);
            return (double)mult[dishTier] * b.BasePrice(round);
        }

        static int RoundMoney(double value)
        {
            // float 0,3 x 60 = 18,0000007...: lam tron 4 chu so truoc de khong lech 1G vi sai so so thuc.
            return (int)Math.Round(Math.Round(value, 4), MidpointRounding.AwayFromZero);
        }
    }
}
