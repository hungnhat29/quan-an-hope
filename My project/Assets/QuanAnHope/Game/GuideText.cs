using System.Collections.Generic;
using System.Globalization;
using System.Text;
using QuanAnHope.Core;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Loi giai thich cho huong dan va the "Cach choi" cua bang Tab (sprint 9 B-07; yeu cau 22/11 "tutorial thieu giai thich
    /// nhung ti le, bac" va "thieu ca phan huong dan ve do kho tinh cua khach hang"). Moi con so lay tu du lieu (Dieu 2):
    /// bang ti le bac theo ngay (constitution 4.3), 6 bac nguyen lieu / mon (4.1, 5.2), cong thuc bac mon (5.3), mon toi thieu
    /// theo do de tinh (6.4), tra tien / tip (6.5), gio cho (6.6).
    /// </summary>
    public static class GuideText
    {
        static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        static string Num(float v) { return v.ToString("0.##", Vi); }
        static string Pct(float share) { return (share * 100f).ToString("0.##", Vi) + "%"; }

        /// <summary>Bang ti le bac nguyen lieu cua ngay: "Ôi thiu 25% · Bình thường 57% · ...". Bo cac o 0%.</summary>
        public static string Odds(Balance b, int day)
        {
            TierTable t = b.OddsForDay(day);
            var parts = new List<string>();
            for (int i = 0; i < t.percents.Length && i < UiKit.IngredientTierName.Length; i++)
                if (t.percents[i] > 0) parts.Add(UiKit.IngredientTierName[i] + " " + t.percents[i] + "%");
            return string.Join(" · ", parts.ToArray());
        }

        public static string RatesNote(Balance b, int day)
        {
            return "Bảng trên tường Góc 2 là tỉ lệ bậc: mỗi món trong thùng tự quay bậc theo bảng của vòng. Hôm nay: " + Odds(b, day) + ".";
        }

        public static string IngredientTiersNote()
        {
            return "Bậc nguyên liệu đọc bằng màu đế dưới chân + mặt: " + string.Join(" → ", UiKit.IngredientTierName)
                + " (0 → 5). Ôi thiu có ruồi, Hiếm đội vương miện. Bậc càng cao món càng ngon.";
        }

        public static string BookNote()
        {
            return "Sách ghi các bước của món và giá bán theo bậc món (" + UiKit.DishTierName[0] + " → " + UiKit.DishTierName[4]
                + "). Khách hài lòng còn trả thêm tip.";
        }

        public static string DishFormula(Balance b)
        {
            return "bậc món ≈ " + Num(b.dish.weightChinh) + " × bậc chính + " + Num(b.dish.weightPhu) + " × bậc phụ + " + Num(b.dish.offset);
        }

        public static string DishNote(Balance b)
        {
            return "Dải 6 ô trên bếp là tỉ lệ ra bậc món: " + DishFormula(b) + "; phần lẻ là cơ hội lên bậc trên (vd 1,8 → 80% Bình thường).";
        }

        /// <summary>Bang mon toi thieu theo do de tinh, tu de toi kho: "≥ 80%: Dở · 60–79%: Bình thường · ...".</summary>
        public static string RequiredTable(Balance b)
        {
            var rows = new List<RequiredTier>(b.customers.requiredTier);
            rows.Sort((x, y) => y.minEasy.CompareTo(x.minEasy));
            var parts = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                RequiredTier r = rows[i];
                string range = i == 0 ? "≥ " + r.minEasy + "%"
                    : i == rows.Count - 1 ? "< " + rows[i - 1].minEasy + "%"
                    : r.minEasy + "–" + (rows[i - 1].minEasy - 1) + "%";
                string tier = r.tier >= 0 && r.tier < UiKit.DishTierName.Length ? UiKit.DishTierName[r.tier] : r.tier.ToString();
                parts.Add(range + ": " + tier);
            }
            return string.Join(" · ", parts.ToArray());
        }

        public static string EasinessNote(Balance b)
        {
            return "Bong bóng khách ghi \"Dễ tính N%\": càng cao càng dễ chịu. Món tối thiểu khách nhận: " + RequiredTable(b) + ".";
        }

        public static string PaymentNote(Balance b)
        {
            return "Đạt yêu cầu: trả đủ giá + tip (khách càng khó tính, món càng vượt yêu cầu thì tip càng nhiều). Thiếu 1 bậc: trả "
                + Pct(b.payment.underPayShare) + " giá. Thiếu 2 bậc hoặc sai món: không trả, quán bị trừ " + Pct(b.payment.penaltyBaseShare) + " giá gốc.";
        }

        public static string PatienceNote(Balance b, int day)
        {
            float safe = Patience.SafeSeconds(b, day, RelicFx.None);
            string s = float.IsInfinity(safe) ? "" : " (hôm nay " + Num(safe) + " giây)";
            return "Từ vòng 2, khách chờ quá khoảng an toàn" + s + " thì độ dễ tính giảm 1 điểm mỗi " + Num(b.patience.dropEverySeconds)
                + " giây (tối đa −" + b.patience.maxDrop + "): khách khó tính hơn, đòi món bậc cao hơn.";
        }

        /// <summary>Trang "Cach choi" cua bang Tab (B-07 AC-3): moi giai thich, so cua ngay hien tai.</summary>
        public static string Sheet(Balance b, int day)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Tỉ lệ bậc nguyên liệu</b> (vòng " + b.RoundOfDay(day) + ", ngày " + day + "): " + Odds(b, day));
            sb.AppendLine("Mỗi món trong thùng tự quay bậc theo bảng trên tường Góc 2; relic và vé đen đổi bảng.");
            sb.AppendLine();
            sb.AppendLine("<b>Bậc nguyên liệu</b> (màu đế + mặt): " + string.Join(" → ", UiKit.IngredientTierName));
            sb.AppendLine("<b>Bậc món</b> (màu viền đĩa) và giá: " + DishTiersWithPrice(b, day));
            sb.AppendLine("<b>Ra bậc món</b>: " + DishFormula(b) + "; phần lẻ là cơ hội lên bậc trên.");
            sb.AppendLine();
            sb.AppendLine("<b>Độ dễ tính</b> (bong bóng khách): càng cao càng dễ. Món tối thiểu: " + RequiredTable(b) + ".");
            sb.AppendLine(PaymentNote(b));
            sb.AppendLine(PatienceNote(b, day));
            return sb.ToString().TrimEnd();
        }

        static string DishTiersWithPrice(Balance b, int day)
        {
            int basePrice = b.BasePrice(b.RoundOfDay(day));
            var parts = new List<string>();
            for (int i = 0; i < UiKit.DishTierName.Length && i < b.dish.priceMult.Length; i++)
                parts.Add(UiKit.DishTierName[i] + " ×" + Num(b.dish.priceMult[i]) + (i <= 4 ? " (" + UnityEngine.Mathf.RoundToInt(basePrice * b.dish.priceMult[i]) + "G)" : ""));
            return string.Join(" · ", parts.ToArray());
        }
    }
}
