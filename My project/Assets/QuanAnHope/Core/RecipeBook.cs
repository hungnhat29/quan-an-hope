using System;

namespace QuanAnHope.Core
{
    /// <summary>
    /// Nguyen lieu, cong thuc va thoi gian nau, doc tu balance/recipes.json
    /// (constitution muc 4.5, 5.8, 5.9).
    /// </summary>
    [Serializable]
    public class RecipeBook
    {
        public CookTiming timing = new CookTiming();
        public string[] pantry = new string[0];
        public string[] bases = new string[0];
        public SideIngredient[] sides = new SideIngredient[0];
        public MainIngredient[] mains = new MainIngredient[0];
        public Recipe[] recipes = new Recipe[0];
    }

    [Serializable]
    public class CookTiming
    {
        public float stepSeconds;
        public float plateSeconds;
        public float serveSeconds;
        public float warnFactor;
        public float burnFactor;
        public int burnFromRound;
    }

    /// <summary>Do phu (rau, cu, qua).</summary>
    [Serializable]
    public class SideIngredient
    {
        public string id;
        public string name;
        public string prep;
        public float prepSeconds;
        public int unlockRound;
    }

    /// <summary>Nguyen lieu chinh hoac dot pha (group = "chinh" / "dotpha").</summary>
    [Serializable]
    public class MainIngredient
    {
        public string id;
        public string name;
        public string group;
        public string prep;
        public float prepSeconds;
        public string tool;
        public string baseName;
        public float cookSeconds;
        public int unlockRound;

        /// <summary>Cach nau (chien, xao, sot, ap chao, nuong, hap; constitution muc 5.9): quyet dinh hinh mon trong dung cu.</summary>
        public string method;
    }

    [Serializable]
    public class Recipe
    {
        public string id;
        public string name;
        public string group;
        public string mainId;
        public string sideId;
        public string tool;
        public string baseName;

        /// <summary>Thu tu cho vao bep, gom ca do kho.</summary>
        public string[] steps = new string[0];
        public float cookSeconds;
        public float warnSeconds;
        public float burnSeconds;
        public int unlockRound;

        /// <summary>Gia niem yet (G) o bac Binh thuong; bac khac = gia x he so bac (constitution 2.0 muc 5.2).</summary>
        public int price;
    }
}
