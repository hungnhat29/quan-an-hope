using System;

namespace QuanAnHope.Core
{
    /// <summary>
    /// Dong ho trong ngay (constitution 1.4 muc 2, 6.2): bat dau luc mo cua (9:00), moi gio game dai
    /// <see cref="DayRules.secondsPerHour"/> giay thuc. Dong ho van chay qua gio dong cua de ghi gio
    /// phuc vu xong khach cuoi.
    /// </summary>
    public class DayClock
    {
        readonly DayRules rules;

        public DayClock(DayRules rules)
        {
            if (rules == null) throw new ArgumentNullException("rules");
            if (rules.secondsPerHour <= 0) throw new ArgumentException("secondsPerHour phai > 0");
            this.rules = rules;
        }

        /// <summary>So giay thuc da troi tu luc mo cua.</summary>
        public float Elapsed { get; private set; }

        /// <summary>Do dai ngay mo cua bang giay thuc.</summary>
        public float DaySeconds { get { return (rules.closeHour - rules.openHour) * rules.secondsPerHour; } }

        /// <summary>Da toi (hoac qua) gio dong cua: khong nhan khach moi.</summary>
        public bool IsPastClose { get { return Elapsed >= DaySeconds; } }

        /// <summary>So phut trong game ke tu 0:00, lam tron xuong.</summary>
        int MinuteOfDay
        {
            get
            {
                // cong mot chut de 40 giay x 4,333 khong ra 13:19 vi sai so float
                double minutes = rules.openHour * 60.0 + Elapsed / rules.secondsPerHour * 60.0;
                return (int)Math.Floor(minutes + 1e-4);
            }
        }

        /// <summary>Gio trong game dang so thuc (9,5 = 9:30), khong lam tron: cho troi doi mau muot.</summary>
        public float HourOfDay { get { return rules.openHour + Elapsed / rules.secondsPerHour; } }

        public int Hour { get { return MinuteOfDay / 60; } }
        public int Minute { get { return MinuteOfDay % 60; } }

        /// <summary>Gio dang "14:30" (gio khong co so 0 o dau nhu "9:00").</summary>
        public string Text { get { return Format(MinuteOfDay); } }

        public void Advance(float seconds)
        {
            if (seconds < 0) throw new ArgumentOutOfRangeException("seconds");
            Elapsed += seconds;
        }

        public void Reset()
        {
            Elapsed = 0f;
        }

        static string Format(int minuteOfDay)
        {
            return (minuteOfDay / 60) + ":" + (minuteOfDay % 60).ToString("00");
        }
    }
}
