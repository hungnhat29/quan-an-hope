using System;
using System.Collections.Generic;

namespace QuanAnHope.Core
{
    /// <summary>
    /// Ban luu dau ngay (F-052, constitution 1.5 muc 2.1). Chi luu o dau ngay (pha Prep, chua mo quan) nen khong
    /// can luu kho, bep, quay cho, khach. Ngau nhien luu bang seed + so lan da rut: tai lai thi gacha, khach,
    /// cua hang ra y nhu lan truoc (khong "luu roi quay lai" de chon ket qua tot).
    /// Lop thuan du lieu (truong public) de JsonUtility / Newtonsoft deu doc ghi duoc.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Doi khi doi cau truc ban luu; ban cu khac so thi bo qua.</summary>
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int day;
        public int money;
        public int lastPlayableDay;
        public int tickets;
        public int seed;
        public long draws;

        public string[] relics = new string[0];
        public string[] shopOffers = new string[0];

        public int sauces;
        public int saucesBought;
        public int blackTickets;
        public int blackBought;
        public bool hasMachine;

        public SaveDish[] fridge = new SaveDish[0];

        /// <summary>Gio luu (ISO 8601, chi de hien "Choi tiep: ngay 5 · 18/11 21:30").</summary>
        public string savedAt;
    }

    [Serializable]
    public class SaveDish
    {
        public string recipeId;
        public string name;
        public string baseName;
        public int tier;
        public bool machined;
        public int daysInFridge;
        public bool decayed;

        public static SaveDish From(Dish d)
        {
            return new SaveDish
            {
                recipeId = d.recipeId, name = d.name, baseName = d.baseName, tier = d.tier,
                machined = d.machined, daysInFridge = d.daysInFridge, decayed = d.decayed,
            };
        }

        public Dish ToDish()
        {
            return new Dish
            {
                recipeId = recipeId, name = name, baseName = baseName, tier = tier,
                machined = machined, daysInFridge = daysInFridge, decayed = decayed,
            };
        }
    }

    public partial class GameSession
    {
        /// <summary>Luu duoc khi dang o dau ngay (chua mo quan, chua quay gacha / mua gi trong ngay) va ngau nhien co seed.</summary>
        public bool CanSnapshot
        {
            get { return Phase == GamePhase.Prep && rng is SeededRandom && Stock.Count == 0 && Counter.Dishes.Count == 0; }
        }

        /// <summary>Chup trang thai dau ngay (F-052 AC-1). Null neu khong luu duoc luc nay.</summary>
        public SaveData Snapshot()
        {
            var seeded = rng as SeededRandom;
            if (Phase != GamePhase.Prep || seeded == null) return null;
            var data = new SaveData
            {
                day = Day,
                money = Money,
                lastPlayableDay = LastPlayableDay,
                tickets = Wallet.Count,
                seed = seeded.Seed,
                draws = seeded.Draws,
                sauces = Items.Sauces,
                saucesBought = Items.SaucesBought,
                blackTickets = Items.BlackTickets,
                blackBought = Items.BlackBought,
                hasMachine = Items.HasMachine,
                savedAt = DateTime.Now.ToString("s"),
            };
            var relics = new List<string>();
            foreach (RelicDef r in Relics.Owned) relics.Add(r.id);
            data.relics = relics.ToArray();
            var offers = new List<string>();
            foreach (RelicDef r in Shop.Offers) offers.Add(r != null ? r.id : "");
            data.shopOffers = offers.ToArray();
            var fridge = new List<SaveDish>();
            foreach (Dish d in Fridge.Dishes) fridge.Add(SaveDish.From(d));
            data.fridge = fridge.ToArray();
            return data;
        }

        /// <summary>
        /// Dung lai van choi tu ban luu (F-052 AC-2): ve dung dau ngay da luu. Ngau nhien chay tiep tu dung cho da dung
        /// nen ngay do ra y nhu lan choi truoc. Ban luu khong hop le thi nem ArgumentException.
        /// </summary>
        public static GameSession FromSave(Balance balance, RecipeBook recipes, SaveData data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.version != SaveData.CurrentVersion) throw new ArgumentException("Ban luu khac phien ban (" + data.version + ")");
            if (data.day < 1) throw new ArgumentException("Ban luu khong co ngay hop le");

            var rng = new SeededRandom(data.seed);
            var s = new GameSession(balance, recipes, rng, data.lastPlayableDay);
            rng.AdvanceTo(Math.Max(rng.Draws, data.draws));

            s.Day = data.day;
            s.Money = data.money;
            s.ResetDayState();
            s.Wallet.Restore(data.tickets);
            foreach (string id in data.relics ?? new string[0])
            {
                RelicDef r = FindRelic(balance, id);
                if (r != null) s.Relics.Add(r);
            }
            var shown = new List<RelicDef>();
            foreach (string id in data.shopOffers ?? new string[0]) shown.Add(FindRelic(balance, id));
            s.Shop.Restore(s.Day, shown);
            s.Items.Restore(s.Day, data.sauces, data.saucesBought, data.blackTickets, data.blackBought, data.hasMachine);
            var dishes = new List<Dish>();
            foreach (SaveDish d in data.fridge ?? new SaveDish[0]) dishes.Add(d.ToDish());
            s.Fridge.Restore(dishes);
            s.BeginDayRules();
            s.Phase = GamePhase.Prep;
            return s;
        }

        static RelicDef FindRelic(Balance balance, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (RelicDef r in balance.relics) if (r.id == id) return r;
            return null;
        }
    }
}
