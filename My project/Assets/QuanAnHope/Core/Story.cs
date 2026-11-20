using System.Collections.Generic;

namespace QuanAnHope.Core
{
    /// <summary>Canh trong mot khung tranh ve bang khoi (StoryComic). Chi con dung cho ket thua (bo tranh CotTruyen chua co).</summary>
    public enum StoryScene
    {
        Office,      // Hope om thung do ra khoi cong ty
        Home,        // ve nha, chu no dung truoc cua
        Begging,     // Hope van xin
        Keys,        // chu no dua chia khoa quan
        Schedule,    // lich doi no 4 moc
        Shop,        // Hope dung truoc quan, mo cua
        Wedding,     // ket thang: chu no ga con gai
        Closed,      // ket thua: chu no lay lai quan
    }

    /// <summary>
    /// Mot khung truyen + loi ke (tieng Viet du dau, TextMeshPro, constitution Dieu 6).
    /// <see cref="comicPage"/> &gt; 0: khung thu <see cref="comicPanel"/> cua trang truyen tranh ve san (Resources/CotTruyen/Trang_N,
    /// nguon CotTruyen_BepNhanPham.zip). Bang 0: tranh khoi <see cref="scene"/> ve bang code (trang <see cref="page"/>, chu tuong thanh <see cref="sfx"/>).
    /// </summary>
    public class StoryPanel
    {
        public StoryScene scene;
        public string text;
        public int page;
        public string sfx;
        public int comicPage;
        public int comicPanel;

        public StoryPanel(StoryScene scene, string text, int page = 0, string sfx = "")
        {
            this.scene = scene;
            this.text = text;
            this.page = page;
            this.sfx = sfx ?? "";
        }

        /// <summary>Khung cua trang truyen ve san (1..6), khung 0..n-1 theo thu tu doc.</summary>
        public static StoryPanel Comic(int comicPage, int comicPanel, string text)
        {
            return new StoryPanel(StoryScene.Shop, text, comicPage - 1) { comicPage = comicPage, comicPanel = comicPanel };
        }
    }

    /// <summary>
    /// Loi cot truyen (constitution muc 2, 2.1; F-051). Tranh: 6 trang CotTruyen_BepNhanPham (Chuong 1-6).
    /// Loi ke duoi trang tom tat khung dang doc; so ngay / so tien lay tu du lieu (Dieu 2).
    /// </summary>
    public static class StoryScript
    {
        /// <summary>Trang chuong chen vao dau ngay (Chuong 2 bao, 3 mat dien, 4 tin don); 0 = khong co.</summary>
        public static int ChapterPageFor(Balance b, int day)
        {
            if (day == 2) return 2;
            if (b.blackout != null && day == b.blackout.day) return 3;
            if (b.debts.Length >= 3 && day == b.debts[2].day) return 4;
            return 0;
        }

        /// <summary>Mo dau = Chuong 1 (5 khung): bi duoi viec, gap chu no, nhan quan va lich doi no (F-051 AC-1).</summary>
        public static List<StoryPanel> Intro(Balance b)
        {
            var debts = new List<string>();
            foreach (DebtEntry d in b.debts) debts.Add("đầu ngày " + d.day + ": " + Money(d.amount));
            return new List<StoryPanel>
            {
                StoryPanel.Comic(1, 0, "Sáng thứ Hai, Hope bị đuổi việc ngay giữa văn phòng."),
                StoryPanel.Comic(1, 1, "Cả sự nghiệp gói gọn trong một thùng các-tông. …và thế là hết."),
                StoryPanel.Comic(1, 2, "Về tới quán ăn cũ, Hope thấy chủ nợ đã đứng chờ trước cửa."),
                StoryPanel.Comic(1, 3, "Thay vì siết nợ, chủ nợ đưa Hope chùm chìa khóa: \"Quán này giờ là của cậu. Nấu đi.\""),
                StoryPanel.Comic(1, 4, "Giấy nợ: " + string.Join(" · ", debts.ToArray()) + ". Thiếu một đồng là tôi lấy lại quán."),
            };
        }

        /// <summary>Trang chuong giua van (2, 3, 4) cho ngay <paramref name="day"/>; rong neu ngay do khong co chuong.</summary>
        public static List<StoryPanel> Chapter(Balance b, int day)
        {
            int page = ChapterPageFor(b, day);
            int d1 = b.debts.Length > 1 ? b.debts[1].day : 9, d2 = b.debts.Length > 2 ? b.debts[2].day : 15;
            switch (page)
            {
                case 2:
                    return new List<StoryPanel>
                    {
                        StoryPanel.Comic(2, 0, "Ngày 2, bão đổ bộ. Sấm nổ ngay trên mái quán."),
                        StoryPanel.Comic(2, 1, "Mái dột tứ tung, Hope đội nồi hứng nước: \"Quán ơi, đừng sập nha…\""),
                        StoryPanel.Comic(2, 2, "Ngày 3, kho gần như trống trơn."),
                        StoryPanel.Comic(2, 3, "Ít nguyên liệu thì nấu thật khéo!"),
                        StoryPanel.Comic(2, 4, "Bà cụ khách quen húp một thìa: \"Ngon quá cháu ơi!\" Ngoài cửa sổ, trời đã tạnh."),
                    };
                case 3:
                    return new List<StoryPanel>
                    {
                        StoryPanel.Comic(3, 0, "Ngày " + b.blackout.day + ", 19 giờ 02: sét đánh cột điện. PHỤT!"),
                        StoryPanel.Comic(3, 1, "Cả quán tối om… chỉ còn mấy đôi mắt nhìn nhau."),
                        StoryPanel.Comic(3, 2, "Hope thắp nến: bếp vẫn phải đỏ lửa!"),
                        StoryPanel.Comic(3, 3, "Không điện, quán vẫn đông khách."),
                        StoryPanel.Comic(3, 4, "Khách còn khen: \"Lãng mạn ghê!\""),
                    };
                case 4:
                    return new List<StoryPanel>
                    {
                        StoryPanel.Comic(4, 0, "Ngày " + d1 + ", có kẻ dán tờ rơi khắp phố: \"Quán bẩn!!\""),
                        StoryPanel.Comic(4, 1, "Mấy ngày liền, quán vắng tanh."),
                        StoryPanel.Comic(4, 2, "Nhưng máy quay của quán đã ghi lại tất cả. \"Bắt được rồi nhé!\""),
                        StoryPanel.Comic(4, 3, "Ngày " + (d2 - 1) + ", kẻ phá đám bị lật tẩy trước mặt mọi người."),
                        StoryPanel.Comic(4, 4, "Ngày " + d2 + ", danh tiếng của quán trở lại!"),
                    };
                default:
                    return new List<StoryPanel>();
            }
        }

        /// <summary>Ket thang = Chuong 5 + 6: mon cuoi cho chu no, tra het no, chu no ga con gai (constitution muc 2; F-051 AC-2).</summary>
        public static List<StoryPanel> Win(Balance b, int money)
        {
            DebtEntry last = b.debts.Length > 0 ? b.debts[b.debts.Length - 1] : null;
            int bossDay = b.day.bossDay > 0 ? b.day.bossDay : 21;
            return new List<StoryPanel>
            {
                StoryPanel.Comic(5, 0, "Ngày " + bossDay + ", hạn chót. Chủ nợ đứng ở cửa: \"Một món. Tuyệt hảo.\""),
                StoryPanel.Comic(5, 1, "Hope nấu bằng cả nhân phẩm."),
                StoryPanel.Comic(5, 2, "Nắp chuông nhấc lên: món Huyền thoại!"),
                StoryPanel.Comic(5, 3, "Chủ nợ nếm thử một thìa… và sững người."),
                StoryPanel.Comic(5, 4, "\"Vị này… giống hệt món mẹ ta nấu…\""),
                StoryPanel.Comic(6, 0, "Đầu ngày " + (last != null ? last.day : 22) + ", Hope trả đủ khoản cuối "
                    + (last != null ? Money(last.amount) : "") + ", còn dư " + Money(money) + ". Chủ nợ xé giấy nợ. XOẸT!!"),
                StoryPanel.Comic(6, 1, "\"Còn một chuyện nữa… con gái ta.\" Rồi ông gả con gái cho Hope."),
                StoryPanel.Comic(6, 2, "Quán ăn của Hope khai trương lại. Hết nợ, có vợ."),
            };
        }

        /// <summary>Ket thua: thieu tien o lan tra no nao, thieu bao nhieu (F-051 AC-2). Mot khung lon ve bang khoi.</summary>
        public static List<StoryPanel> Lose(int day, int due, int money)
        {
            return new List<StoryPanel>
            {
                new StoryPanel(StoryScene.Closed, "Đầu ngày " + day + ", chủ nợ đến thu " + Money(due) + ". Hope chỉ có " + Money(money)
                    + ", thiếu " + Money(due - money) + ". Chủ nợ lấy lại chìa khóa quán.", 0, "RẦM!"),
            };
        }

        /// <summary>
        /// Doi so ngay in tren tranh ("Ngày 15", "Ngày 10–13") theo du lieu: tranh ve voi lich no 4 / 9 / 15 / 22, chu no ngay 21, mat dien ngay 8.
        /// </summary>
        public static Dictionary<int, int> ComicDayMap(Balance b)
        {
            var m = new Dictionary<int, int>();
            int[] drawn = { 4, 9, 15, 22 };
            for (int i = 0; i < drawn.Length && i < b.debts.Length; i++) m[drawn[i]] = b.debts[i].day;
            if (b.debts.Length >= 3)
            {
                m[10] = b.debts[1].day + 1;
                m[13] = b.debts[2].day - 2;
                m[14] = b.debts[2].day - 1;
            }
            if (b.day.bossDay > 0) m[21] = b.day.bossDay;
            if (b.blackout != null && b.blackout.day > 0) m[8] = b.blackout.day;
            return m;
        }

        /// <summary>So tien kieu Viet: 32.000G.</summary>
        public static string Money(int amount)
        {
            string digits = System.Math.Abs(amount).ToString();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < digits.Length; i++)
            {
                if (i > 0 && (digits.Length - i) % 3 == 0) sb.Append('.');
                sb.Append(digits[i]);
            }
            return (amount < 0 ? "−" : "") + sb + "G";
        }
    }
}
