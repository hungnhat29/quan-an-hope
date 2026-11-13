using System;

namespace QuanAnHope.Core
{
    /// <summary>Nguon ngau nhien cua moi luat choi; test thay bang ban co dinh.</summary>
    public interface IRandom
    {
        /// <summary>So thuc trong [0, 1).</summary>
        double NextDouble();
    }

    public static class RandomExtensions
    {
        /// <summary>So nguyen trai deu trong [minInclusive, maxInclusive].</summary>
        public static int NextInt(this IRandom rng, int minInclusive, int maxInclusive)
        {
            if (maxInclusive < minInclusive) throw new ArgumentException("maxInclusive < minInclusive");
            int span = maxInclusive - minInclusive + 1;
            int offset = (int)(rng.NextDouble() * span);
            if (offset >= span) offset = span - 1;
            return minInclusive + offset;
        }
    }

    /// <summary>Ngau nhien co seed, dung cho game that va cho test thong ke.</summary>
    public sealed class SeededRandom : IRandom
    {
        readonly Random random;

        public SeededRandom(int seed)
        {
            random = new Random(seed);
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }
    }

    public static class TierRoller
    {
        /// <summary>Quay mot bac theo bang phan tram (chi so = bac). Bac co ti le 0 khong bao gio ra.</summary>
        public static int Roll(int[] percents, IRandom rng)
        {
            if (percents == null || percents.Length == 0) throw new ArgumentException("Bang ti le rong");

            int total = 0;
            for (int i = 0; i < percents.Length; i++) total += percents[i];
            if (total <= 0) throw new ArgumentException("Bang ti le co tong bang 0");

            double x = rng.NextDouble() * total;
            int cumulative = 0;
            int last = 0;
            for (int tier = 0; tier < percents.Length; tier++)
            {
                if (percents[tier] <= 0) continue;
                cumulative += percents[tier];
                last = tier;
                if (x < cumulative) return tier;
            }
            return last;
        }
    }
}
