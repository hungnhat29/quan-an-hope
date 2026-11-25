using System;
using System.Collections.Generic;

namespace QuanAnHope.Core
{
    /// <summary>Ten cac khoa hieu ung trong relics.json / set_bonus (constitution muc 7). Khong phai con so can bang.</summary>
    public static class RelicKeys
    {
        public const string Up = "_up";
        public const string Reroll0 = "_reroll0";
        public const string Jump4 = "_jump4";
        public const string Min = "_min";
        public const string Plus = "_plus";
        public const string Size = "_size";
        public const string SidePrefix = "phu";
        public const string MainPrefix = "chinh";

        public const string BtChance = "bt_chance";
        public const string BtItems = "bt_items";
        public const string BtT5 = "bt_t5";

        public const string DishCenter = "dish_center";
        public const string DishRuin = "dish_ruin";
        public const string DishFloor = "dish_floor";
        /// <summary>constitution 2.1 muc 5.3 / 7.3: cong them vao ti le Huyen thoai khi nau (tong toi da legend_cap).</summary>
        public const string DishLegend = "dish_legend";
        public const string Tip = "tip";
        public const string Income = "income";
        public const string TicketEvery3 = "ticket_every3";

        // Gan voi he thong co tu sprint 4 (tu lanh, gio cho, nuoc sot).
        public const string Fridge = "fridge";
        public const string NoDecay = "no_decay";
        public const string Patience = "patience";
        public const string SauceSafe = "sauce_safe";

        public static readonly string[] NotYetActive = new string[0];

        /// <summary>Khoa cua thuong bo lay gia tri lon nhat thay vi cong (sim_balance.py P.fx).</summary>
        public static bool IsMaxKey(string key)
        {
            return key == SidePrefix + Min || key == MainPrefix + Min || key == BtT5;
        }

        public static string Group(IngredientKind kind)
        {
            return kind == IngredientKind.Side ? SidePrefix : MainPrefix;
        }

        public const string SetVuon = "vuon";
        public const string SetCho = "cho";
        public const string SetSonHao = "sonhao";

        /// <summary>Do hiem "Thuong" trong relics.json (ve den chi ban tu Hiem tro len, muc 7.1).</summary>
        public const string RarityCommon = "thuong";

        public static string SetName(string set)
        {
            switch (set)
            {
                case SetVuon: return "Vườn Nhà";
                case SetCho: return "Chợ Sớm";
                case SetSonHao: return "Sơn Hào";
                default: return "";
            }
        }

        public static string RarityName(string rarity)
        {
            switch (rarity)
            {
                case "thuong": return "Thường";
                case "hiem": return "Hiếm";
                case "huyenthoai": return "Huyền thoại";
                default: return rarity;
            }
        }
    }

    /// <summary>Hieu ung gop cua moi relic tren gia + thuong bo (sim_balance.py P.fx).</summary>
    public class RelicFx
    {
        readonly Dictionary<string, float> values = new Dictionary<string, float>();

        public static readonly RelicFx None = new RelicFx();

        public float Get(string key)
        {
            float v;
            return values.TryGetValue(key, out v) ? v : 0f;
        }

        public float Get(IngredientKind kind, string suffix)
        {
            return Get(RelicKeys.Group(kind) + suffix);
        }

        public IEnumerable<KeyValuePair<string, float>> All { get { return values; } }

        public bool IsEmpty { get { return values.Count == 0; } }

        internal void Add(string key, float value)
        {
            values[key] = Get(key) + value;
        }

        internal void Max(string key, float value)
        {
            float cur;
            values[key] = values.TryGetValue(key, out cur) ? Math.Max(cur, value) : value;
        }

        /// <summary>Gop hieu ung tu danh sach relic theo luat cua mo phong.</summary>
        public static RelicFx Combine(IEnumerable<RelicDef> relics, SetBonus[] bonuses)
        {
            var fx = new RelicFx();
            var count = new Dictionary<string, int>();
            foreach (RelicDef r in relics)
            {
                foreach (RelicEffect e in r.effects) fx.Add(e.key, e.value);
                if (!string.IsNullOrEmpty(r.set))
                {
                    int n;
                    count.TryGetValue(r.set, out n);
                    count[r.set] = n + 1;
                }
            }
            if (bonuses != null)
                foreach (SetBonus b in bonuses)
                {
                    int n;
                    if (!count.TryGetValue(b.set, out n) || n < b.count) continue;
                    foreach (RelicEffect e in b.effects)
                    {
                        if (RelicKeys.IsMaxKey(e.key)) fx.Max(e.key, e.value);
                        else fx.Add(e.key, e.value);
                    }
                }
            return fx;
        }
    }

    /// <summary>Gia relic (constitution muc 7.1): toi da relicCap mon, hieu ung tinh lai moi lan doi.</summary>
    public class RelicInventory
    {
        readonly Balance balance;
        readonly List<RelicDef> owned = new List<RelicDef>();

        public RelicFx Effects { get; private set; }

        public RelicInventory(Balance balance)
        {
            this.balance = balance;
            Effects = RelicFx.None;
        }

        public IReadOnlyList<RelicDef> Owned { get { return owned; } }
        public int Capacity { get { return balance.relicCap; } }
        public bool IsFull { get { return owned.Count >= Capacity; } }

        public bool Has(string id)
        {
            foreach (RelicDef r in owned) if (r.id == id) return true;
            return false;
        }

        public int CountInSet(string set)
        {
            int n = 0;
            if (string.IsNullOrEmpty(set)) return 0;
            foreach (RelicDef r in owned) if (r.set == set) n++;
            return n;
        }

        /// <summary>So relic co trong bo (de ghi "Cho Som 2/4").</summary>
        public int SetSize(string set)
        {
            int n = 0;
            foreach (RelicDef r in balance.relics) if (r.set == set) n++;
            return n;
        }

        /// <summary>Them relic; day thi khong them (phai bo mot cai truoc).</summary>
        public bool Add(RelicDef relic)
        {
            if (relic == null || IsFull || Has(relic.id)) return false;
            owned.Add(relic);
            Recompute();
            return true;
        }

        public bool Remove(string id)
        {
            for (int i = 0; i < owned.Count; i++)
                if (owned[i].id == id)
                {
                    owned.RemoveAt(i);
                    Recompute();
                    return true;
                }
            return false;
        }

        void Recompute()
        {
            Effects = RelicFx.Combine(owned, balance.setBonuses);
        }
    }

    public enum RerollResult
    {
        Rerolled,
        Closed,
        NotEnoughMoney,
        NoneLeft,
    }

    public enum RelicBuyResult
    {
        Bought,

        /// <summary>Cua hang chua mo (ngay 1) hoac het ngay.</summary>
        Closed,

        /// <summary>Be trong (da mua hom nay).</summary>
        Empty,
        NotEnoughTickets,

        /// <summary>Gia day: phai chon mot relic de bo.</summary>
        NeedDiscard,
    }

    /// <summary>Ba be relic cua ngay (constitution muc 7.1): rut deu trong cac relic chua co, khong trung nhau.</summary>
    public class RelicShop
    {
        readonly Balance balance;
        readonly IRandom rng;
        RelicDef[] offers = new RelicDef[0];

        public RelicShop(Balance balance, IRandom rng)
        {
            this.balance = balance;
            this.rng = rng;
        }

        /// <summary>Be thu i; null neu be trong.</summary>
        public IReadOnlyList<RelicDef> Offers { get { return offers; } }
        public bool IsOpen { get; private set; }
        public int Slots { get { return balance.relicShopOffers; } }

        public bool OpensOn(int day)
        {
            return day >= balance.start.relicShopFromDay;
        }

        /// <summary>Ngay dang bay (de biet ngay mien phi).</summary>
        public int Day { get; private set; }

        /// <summary>Hom nay la ngay relic mien phi (constitution 1.8 muc 7.1: ngay 2, chi relic Thuong, lay 1).</summary>
        public bool IsFreeDay { get { return balance.freeRelic != null && balance.freeRelic.day > 0 && Day == balance.freeRelic.day; } }

        /// <summary>So relic da lay mien phi hom nay.</summary>
        public int FreePicked { get; private set; }

        /// <summary>So lan doi hang hom nay (gia lan sau tang them step).</summary>
        public int RerollsToday { get; private set; }

        /// <summary>Gia doi hang lan ke tiep (tien G).</summary>
        public int RerollCost { get { return balance.relicReroll.baseCost + balance.relicReroll.step * RerollsToday; } }

        /// <summary>Gia bang ve cua be <paramref name="slot"/> hom nay (0 vao ngay mien phi); 0 neu be trong.</summary>
        public int Price(int slot)
        {
            if (slot < 0 || slot >= offers.Length || offers[slot] == null) return 0;
            // 2.2 (yeu cau 24/11/2026): ngay mien phi, lay xong relic mien phi thi hai be con lai hien lai gia ve, mua tiep duoc
            return IsFreeDay && !FreeDone ? 0 : offers[slot].price;
        }

        bool Allowed(RelicDef r)
        {
            return !IsFreeDay || string.IsNullOrEmpty(balance.freeRelic.rarity) || r.rarity == balance.freeRelic.rarity;
        }

        /// <summary>Dau ngay: bay hang moi (ngay chua mo thi ba be trong).</summary>
        public void Restock(int day, RelicInventory inventory)
        {
            Day = day;
            FreePicked = 0;
            RerollsToday = 0;
            IsOpen = OpensOn(day);
            offers = new RelicDef[balance.relicShopOffers];
            if (!IsOpen) return;
            Fill(inventory, null);
        }

        /// <summary>Rut ba relic chua co (khong lap, giong random.sample cua mo phong), bo qua cac relic trong <paramref name="exclude"/>.</summary>
        void Fill(RelicInventory inventory, ICollection<RelicDef> exclude)
        {
            var pool = new List<RelicDef>();
            foreach (RelicDef r in balance.relics)
                if (!inventory.Has(r.id) && Allowed(r) && (exclude == null || !exclude.Contains(r))) pool.Add(r);
            offers = new RelicDef[balance.relicShopOffers];
            for (int i = 0; i < offers.Length && pool.Count > 0; i++)
            {
                int k = rng.NextInt(0, pool.Count - 1);
                offers[i] = pool[k];
                pool.RemoveAt(k);
            }
        }

        /// <summary>So relic co the bay khi doi hang (khac ba relic dang bay, chua co tren gia).</summary>
        public int RerollPoolSize(RelicInventory inventory)
        {
            int n = 0;
            foreach (RelicDef r in balance.relics)
                if (!inventory.Has(r.id) && Allowed(r) && Array.IndexOf(offers, r) < 0) n++;
            return n;
        }

        /// <summary>
        /// Doi hang (constitution 1.8 muc 7.1): ba be bay ba relic khac han ba relic dang bay. Tien do GameSession tru.
        /// False neu cua hang dong, da lay relic mien phi, hoac khong con relic khac de bay.
        /// </summary>
        public bool Reroll(RelicInventory inventory)
        {
            if (!IsOpen || RerollPoolSize(inventory) == 0) return false;
            var shown = new List<RelicDef>();
            foreach (RelicDef r in offers) if (r != null) shown.Add(r);
            Fill(inventory, shown);
            RerollsToday++;
            return true;
        }

        /// <summary>Ngay mien phi va da lay du so relic duoc lay: cac be con lai dong.</summary>
        public bool FreeDone { get { return IsFreeDay && FreePicked >= Math.Max(1, balance.freeRelic.picks); } }

        public void Close()
        {
            IsOpen = false;
        }

        /// <summary>Tai game (F-052): bay lai dung ba relic da bay dau ngay (khong rut lai).</summary>
        internal void Restore(int day, IList<RelicDef> shown)
        {
            Day = day;
            FreePicked = 0;
            RerollsToday = 0;
            IsOpen = OpensOn(day);
            offers = new RelicDef[balance.relicShopOffers];
            if (!IsOpen) return;
            for (int i = 0; i < offers.Length && i < shown.Count; i++) offers[i] = shown[i];
        }

        /// <summary>
        /// Mua relic o be <paramref name="slot"/>. Gia day thi phai truyen <paramref name="discardId"/>
        /// (relic tren gia se bo); khong truyen thi tra NeedDiscard va khong doi gi.
        /// </summary>
        public RelicBuyResult Buy(int slot, TicketWallet wallet, RelicInventory inventory, string discardId = null)
        {
            if (!IsOpen) return RelicBuyResult.Closed;
            if (slot < 0 || slot >= offers.Length || offers[slot] == null) return RelicBuyResult.Empty;
            RelicDef relic = offers[slot];
            int price = Price(slot);
            if (wallet.Count < price) return RelicBuyResult.NotEnoughTickets;
            if (inventory.IsFull)
            {
                if (discardId == null || !inventory.Has(discardId)) return RelicBuyResult.NeedDiscard;
                inventory.Remove(discardId);
            }
            if (price > 0) wallet.TrySpend(price);
            inventory.Add(relic);
            offers[slot] = null;
            // ngay mien phi: picks cai dau tien mien phi; sau do cac be con lai ban bang ve nhu ngay thuong (2.2)
            if (IsFreeDay && price == 0) FreePicked++;
            return RelicBuyResult.Bought;
        }

        /// <summary>So ve con thieu de mua be nay (0 neu du).</summary>
        public int Missing(int slot, TicketWallet wallet)
        {
            if (slot < 0 || slot >= offers.Length || offers[slot] == null) return 0;
            return Math.Max(0, Price(slot) - wallet.Count);
        }

        /// <summary>Do hiem duoc ban bang ve den (constitution muc 7.1: "chi gom relic Hiem tro len").</summary>
        public static bool IsRareOrBetter(RelicDef r)
        {
            return r != null && r.rarity != RelicKeys.RarityCommon;
        }

        /// <summary>
        /// Ba lua chon khi dung ve den (constitution muc 7.1): relic Hiem tro len, chua co tren gia, khac nhau; it nhat mot cai
        /// thuoc bo dang co nhieu mon nhat (bang nhau thi bo nao trong so do cung duoc). Chua co relic bo nao: ba cai bat ky.
        /// </summary>
        public RelicDef[] RollBlackOffers(RelicInventory inventory)
        {
            var pool = new List<RelicDef>();
            foreach (RelicDef r in balance.relics)
                if (IsRareOrBetter(r) && !inventory.Has(r.id)) pool.Add(r);

            int best = 0;
            foreach (RelicDef r in inventory.Owned)
                if (!string.IsNullOrEmpty(r.set)) best = Math.Max(best, inventory.CountInSet(r.set));

            var result = new List<RelicDef>();
            if (best > 0)
            {
                var fromTop = new List<RelicDef>();
                foreach (RelicDef r in pool)
                    if (!string.IsNullOrEmpty(r.set) && inventory.CountInSet(r.set) == best) fromTop.Add(r);
                if (fromTop.Count > 0)
                {
                    RelicDef first = fromTop[rng.NextInt(0, fromTop.Count - 1)];
                    result.Add(first);
                    pool.Remove(first);
                }
            }
            int n = balance.relicShopOffers;
            while (result.Count < n && pool.Count > 0)
            {
                int k = rng.NextInt(0, pool.Count - 1);
                result.Add(pool[k]);
                pool.RemoveAt(k);
            }
            // tron thu tu de cai thuoc bo khong luon nam o be dau
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(0, i);
                RelicDef t = result[i]; result[i] = result[j]; result[j] = t;
            }
            return result.ToArray();
        }
    }

    /// <summary>Chu hieu ung relic cho the dung (constitution Dieu 4: hien gia va hieu ung relic).</summary>
    public static class RelicText
    {
        public static string Effect(RelicDef r)
        {
            var parts = new List<string>();
            foreach (RelicEffect e in r.effects) parts.Add(EffectLine(e.key, e.value));
            return string.Join(" · ", parts);
        }

        public static bool IsActiveNow(RelicDef r)
        {
            foreach (RelicEffect e in r.effects)
                if (Array.IndexOf(RelicKeys.NotYetActive, e.key) >= 0) return false;
            return true;
        }

        static string Pct(float v)
        {
            return Math.Round(v * 100f).ToString("0") + "%";
        }

        public static string EffectLine(string key, float v)
        {
            switch (key)
            {
                case "phu_up": return "Đồ phụ: " + Pct(v) + " nâng bậc";
                case "chinh_up": return "Nguyên liệu chính: " + Pct(v) + " nâng bậc";
                case "phu_reroll0": return "Đồ phụ bậc 0 được quay lại 1 lần";
                case "chinh_reroll0": return "Nguyên liệu chính bậc 0 được quay lại 1 lần";
                case "phu_jump4": return "Đồ phụ: " + Pct(v) + " nhảy lên bậc 4";
                case "chinh_jump4": return "Nguyên liệu chính: " + Pct(v) + " nhảy lên bậc 4";
                case "phu_min": return "Đồ phụ tối thiểu bậc " + v.ToString("0");
                case "chinh_min": return "Nguyên liệu chính tối thiểu bậc " + v.ToString("0");
                case "phu_plus": return "Mọi đồ phụ +" + v.ToString("0") + " bậc";
                case "chinh_plus": return "Mọi nguyên liệu chính +" + v.ToString("0") + " bậc";
                case "phu_size": return "Thùng đồ phụ " + (v >= 0 ? "+" : "−") + Math.Abs(v).ToString("0") + " món";
                case "chinh_size": return "Thùng chính " + (v >= 0 ? "+" : "−") + Math.Abs(v).ToString("0") + " món";
                case "bt_chance": return "Thùng đột phá +" + Pct(v);
                case "bt_t5": return v >= 1f ? "Mọi món đột phá bậc 5" : "Đột phá bậc 5 +" + Pct(v);
                case "bt_items": return "Thùng đột phá +" + v.ToString("0") + " món";
                case "dish_center": return "Điểm món +" + v.ToString("0.0#");
                case "dish_ruin": return Pct(v) + " món thành Lạc vị";
                case "dish_floor": return "Món không bao giờ dưới Dở";
                case "dish_legend": return "Huyền thoại khi nấu +" + Math.Round(v * 100f, 1).ToString("0.0#") + "%";
                case "tip": return "Tip +" + Pct(v);
                case "income": return "Tiền bán món +" + Pct(v);
                case "ticket_every3": return "+" + v.ToString("0") + " vé trắng mỗi 3 ngày";
                case "fridge": return "Tủ lạnh +" + v.ToString("0") + " ô";
                case "no_decay": return "món không tụt bậc";
                case "patience": return "Thời gian chờ an toàn +" + Pct(v);
                case "sauce_safe": return "Tỉ lệ hỏng nước sốt −" + Math.Round(v * 100f).ToString("0") + " điểm";
                default: return key + " " + v;
            }
        }
    }
}
