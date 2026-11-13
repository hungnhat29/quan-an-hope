using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Data
{
    /// <summary>
    /// ScriptableObject sinh tu balance/balance.json va balance/recipes.json
    /// bang menu "Quán Ăn Hope > Nhập balance.json". Khong sua tay asset nay.
    /// </summary>
    public class BalanceData : ScriptableObject
    {
        public Balance balance = new Balance();
        public RecipeBook recipes = new RecipeBook();
    }
}
