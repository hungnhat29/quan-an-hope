using NUnit.Framework;
using QuanAnHope.Core;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// F-002 AC-5. Constitution Dieu 3: quay 100.000 lan, moi ket qua lech khong qua
    /// +-1 diem phan tram so voi bang.
    /// </summary>
    public class TierTableStatTests
    {
        const int Rolls = 100000;
        const double TolerancePoints = 1.0;

        [TestCase("r1")]
        [TestCase("storm")]
        [TestCase("r2")]
        [TestCase("r3")]
        [TestCase("r4")]
        [TestCase("black")]
        public void AC5_BangTiLeBac_Quay100000Lan(string tableId)
        {
            TierTable table = null;
            foreach (TierTable t in TestData.Balance.ingredientOdds)
                if (t.id == tableId) table = t;
            Assert.IsNotNull(table);

            var rng = new SeededRandom(20261114);
            var counts = new int[table.percents.Length];
            for (int i = 0; i < Rolls; i++) counts[TierRoller.Roll(table.percents, rng)]++;

            for (int tier = 0; tier < counts.Length; tier++)
            {
                double actual = 100.0 * counts[tier] / Rolls;
                Assert.AreEqual(table.percents[tier], actual, TolerancePoints, "Bang " + tableId + ", bac " + tier);
                if (table.percents[tier] == 0) Assert.AreEqual(0, counts[tier], "Bac ti le 0% khong duoc ra");
            }
        }

        [Test]
        public void AC5_GachaNgay1_VaNgay3_KhopBang()
        {
            AssertGachaMatches(1, "r1");
            AssertGachaMatches(3, "storm");
        }

        static void AssertGachaMatches(int day, string tableId)
        {
            Balance b = TestData.Balance;
            var gacha = new IngredientGacha(b, TestData.Recipes, new SeededRandom(day * 1000 + 7));
            int[] expected = b.OddsForDay(day).percents;
            Assert.AreEqual(tableId, b.OddsForDay(day).id);

            var counts = new int[expected.Length];
            int total = 0;
            while (total < Rolls)
            {
                PullResult pull = gacha.Pull(day);
                foreach (Ingredient i in pull.side.items) { counts[i.tier]++; total++; }
                foreach (Ingredient i in pull.main.items) { counts[i.tier]++; total++; }
            }

            for (int tier = 0; tier < counts.Length; tier++)
                Assert.AreEqual(expected[tier], 100.0 * counts[tier] / total, TolerancePoints, "Ngay " + day + ", bac " + tier);
        }
    }
}
