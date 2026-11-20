using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuanAnHope.Core;
using QuanAnHope.EditorTools;

namespace QuanAnHope.Tests
{
    /// <summary>Du lieu that tu balance/*.json, doc mot lan cho ca bo test.</summary>
    public static class TestData
    {
        static Balance balance;
        static RecipeBook recipes;

        /// <summary>Khach an xong roi ghe sau bay nhieu giay (constitution 1.5 muc 6.2).</summary>
        public static float Eat { get { return Balance.day.eatSeconds; } }

        /// <summary>Tu luc phuc vu toi luc khach ke tiep ngoi vao ghe do: an xong + 3 giay.</summary>
        public static float NextSeat { get { return Balance.day.eatSeconds + Balance.day.nextCustomerSeconds; } }

        public static Balance Balance
        {
            get { return balance ?? (balance = BalanceImporter.ParseBalance(File.ReadAllText(BalanceImporter.BalanceJsonPath), File.ReadAllText(BalanceImporter.RelicsJsonPath))); }
        }

        public static RecipeBook Recipes
        {
            get { return recipes ?? (recipes = BalanceImporter.ParseRecipes(File.ReadAllText(BalanceImporter.RecipesJsonPath))); }
        }

        public static Ingredient Side(string id, int tier, bool prepared = true)
        {
            Ingredient i = Ingredient.FromSide(Recipes.sides.Single(s => s.id == id), tier);
            i.prepared = prepared;
            return i;
        }

        /// <summary>Van choi nhay thang toi dau ngay <paramref name="day"/> (pha Prep) voi so tien cho truoc (sprint 4).</summary>
        public static GameSession SessionAt(int day, int money = 0, int seed = 1, int lastDay = 8)
        {
            var s = new GameSession(Balance, Recipes, new SeededRandom(seed), lastDay);
            s.DebugJumpToDay(day);
            s.DebugAddMoney(money - s.Money);
            return s;
        }

        /// <summary>Mot dia mon dung cong thuc <paramref name="recipeId"/> o bac cho truoc.</summary>
        public static Dish DishOf(string recipeId, int tier)
        {
            Recipe r = Recipes.recipes.Single(x => x.id == recipeId);
            return new Dish { recipeId = r.id, name = r.name, baseName = r.baseName, tier = tier };
        }

        public static Ingredient Main(string id, int tier, bool prepared = true)
        {
            Ingredient i = Ingredient.FromMain(Recipes.mains.Single(m => m.id == id), tier);
            i.prepared = prepared;
            return i;
        }
    }

    /// <summary>Tra ve lan luot cac gia tri cho truoc; het thi bao loi de test khong am tham dung so khac.</summary>
    public class FixedRandom : IRandom
    {
        readonly Queue<double> values;

        public FixedRandom(params double[] values)
        {
            this.values = new Queue<double>(values);
        }

        public double NextDouble()
        {
            if (values.Count == 0) throw new InvalidOperationException("FixedRandom: da dung het gia tri cho truoc");
            return values.Dequeue();
        }
    }
}
