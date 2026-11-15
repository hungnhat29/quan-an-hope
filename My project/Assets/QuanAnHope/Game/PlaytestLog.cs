using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// F-014 Do thoi gian that (constitution 1.4 muc 9: gia dinh OVERHEAD cua mo phong can do).
    /// Moi mon mot dong trong playtest-log.csv: luc cam nguyen lieu dau, luc vat num, luc chin, luc bam chuong (giay game
    /// tu luc mo game), va phan "thao tac + di lai" = tong - (so che + cho vao + nau + muc) theo du lieu.
    /// Phan tich: python balance/playtest_overhead.py &lt;duong dan csv&gt;.
    /// </summary>
    public class PlaytestLog
    {
        public const string Header = "ngay,gio_game,mon,bac,t_cam_dau,t_vat_num,t_chin,t_bam_chuong,che_bien_s,thao_tac_di_lai_s";

        class Record
        {
            public float firstTake = -1f, knob = -1f, ready = -1f;
            public float work;
        }

        Record cooking;
        readonly Dictionary<Dish, Record> plated = new Dictionary<Dish, Record>();

        public string Path { get; private set; }
        public int RowsWritten { get; private set; }

        public PlaytestLog(string folder)
        {
            Path = System.IO.Path.Combine(folder, "playtest-log.csv");
            try
            {
                if (!File.Exists(Path)) File.WriteAllText(Path, Header + "\n");
            }
            catch (IOException e)
            {
                Debug.LogWarning("Khong ghi duoc " + Path + ": " + e.Message);
            }
        }

        /// <summary>Cam mot nguyen lieu tu thung: neu bep chua co mon dang lam thi day la luc bat dau mon moi.</summary>
        public void OnTakeIngredient(float now)
        {
            if (cooking == null) cooking = new Record();
            if (cooking.firstTake < 0f) cooking.firstTake = now;
        }

        public void OnKnob(float now, Recipe recipe, RecipeBook book, Stove stove)
        {
            if (cooking == null) cooking = new Record { firstTake = now };
            cooking.knob = now;
            cooking.work = WorkSeconds(recipe, book);
        }

        public void OnReady(float now)
        {
            if (cooking != null) cooking.ready = now;
        }

        public void OnPlated(Dish dish)
        {
            if (cooking == null || dish == null) return;
            plated[dish] = cooking;
            cooking = null;
        }

        /// <summary>Do bo do tren bep: mon dang lam khong tinh.</summary>
        public void OnDiscard()
        {
            cooking = null;
        }

        public void OnServed(float now, GameSession s, Dish dish)
        {
            Record r;
            if (dish == null || !plated.TryGetValue(dish, out r)) return;
            plated.Remove(dish);
            float total = now - r.firstTake;
            string row = string.Join(",", new[]
            {
                s.Day.ToString(), s.Clock.Text, "\"" + dish.name + "\"", dish.tier.ToString(),
                F(r.firstTake), F(r.knob), F(r.ready), F(now), F(r.work), F(total - r.work),
            });
            try
            {
                File.AppendAllText(Path, row + "\n");
                RowsWritten++;
            }
            catch (IOException e)
            {
                Debug.LogWarning("Khong ghi duoc " + Path + ": " + e.Message);
            }
        }

        /// <summary>Phan thoi gian co trong du lieu (giong sim_balance.dish_seconds, chua tinh di lai).</summary>
        public static float WorkSeconds(Recipe recipe, RecipeBook book)
        {
            if (recipe == null) return 0f;
            float prep = 0f;
            foreach (SideIngredient x in book.sides) if (x.id == recipe.sideId) prep += x.prepSeconds;
            foreach (MainIngredient x in book.mains) if (x.id == recipe.mainId) prep += x.prepSeconds;
            return prep + book.timing.stepSeconds * recipe.steps.Length + recipe.cookSeconds + book.timing.plateSeconds;
        }

        static string F(float v)
        {
            return v < 0f ? "" : v.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }
}
