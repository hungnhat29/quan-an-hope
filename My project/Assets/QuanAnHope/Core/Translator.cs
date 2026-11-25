using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QuanAnHope.Core
{
    /// <summary>
    /// F-110 (specs/sprint-11/spec.md AC-2, yeu cau 25/11/2026): dich chu hien thi tieng Viet -> tieng Anh.
    /// Ban dich la du lieu (Resources/Lang/en.json sinh tu tools/i18n, Dieu 2); code va du lieu game van dung tieng Viet.
    ///
    /// Thu tu tra: (1) nguyen van (ca ban bo khoang trang dau / cuoi, chu dau viet hoa, chu IN HOA); (2) mau co lo "{0}"
    /// (cau ghep trong code) — chu trong lo dich tiep; (3) tach dong, tach " · "; (4) thay manh con lai dung ranh gioi tu;
    /// cuoi cung doi so kieu Viet sang kieu Anh (32.000 -> 32,000; 0,3 -> 0.3).
    /// </summary>
    public sealed class Translator
    {
        sealed class Template
        {
            public string[] lits;    // phan chu: lits[0] {ids[0]} lits[1] {ids[1]} ... lits[n]
            public int[] ids;
            public string en;
            public string anchor;    // manh chu dai nhat cua mau: loc nhanh truoc khi thu khop
            public int literal;      // tong do dai phan chu (mau cu the hon thu truoc)
        }

        readonly Dictionary<string, string> exact = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly Dictionary<string, string> upper = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly List<Template> templates = new List<Template>();
        readonly List<KeyValuePair<string, string>> fragments = new List<KeyValuePair<string, string>>();
        readonly HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
        readonly Dictionary<string, string> memo = new Dictionary<string, string>(StringComparer.Ordinal);

        const int MemoCap = 8000, LeftoverCap = 2000, MaxDepth = 6;
        static readonly string[] Separators = { " · ", " — ", "  ›  ", " | " };
        static readonly Regex Hole = new Regex(@"\{(\d+)\}", RegexOptions.CultureInvariant);

        /// <summary>Chu hien thi con chu Viet co dau sau khi dich (de tim cho sot, AC-4).</summary>
        public readonly HashSet<string> Leftovers = new HashSet<string>(StringComparer.Ordinal);

        public Translator(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            foreach (KeyValuePair<string, string> p in pairs)
            {
                if (string.IsNullOrEmpty(p.Key) || p.Value == null) continue;
                keys.Add(p.Key);
                if (Hole.IsMatch(p.Key)) AddTemplate(p.Key, p.Value);
                else AddExact(p.Key, p.Value);
            }
            templates.Sort((a, b) => b.literal.CompareTo(a.literal));
            fragments.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
        }

        /// <summary>Bo khoang trang va dau ngan (· — › |) o hai dau: manh " · giữ " sau khi tach doan la "giữ".</summary>
        static string Bare(string s)
        {
            return s.Trim().Trim('·', '—', '›', '|', ':').Trim();
        }

        void AddExact(string vi, string en)
        {
            exact[vi] = en;
            string bv = Bare(vi), be = Bare(en);
            if (bv.Length > 0 && bv != vi.Trim() && !exact.ContainsKey(bv)) AddExact(bv, be);
            string tv = vi.Trim(), te = en.Trim();
            if (tv.Length == 0) return;
            if (!exact.ContainsKey(tv)) exact[tv] = te;
            string up = tv.ToUpperInvariant();
            if (!upper.ContainsKey(up)) upper[up] = te.ToUpperInvariant();
            if (HasVietnamese(tv) && tv.Length >= 2)
            {
                fragments.Add(new KeyValuePair<string, string>(tv, te));
                // "khách trái" con duoc dung viet hoa chu dau ("Khách trái" — char.ToUpper o RoomViewRound2)
                if (char.IsLower(tv[0]) && te.Length > 0)
                    fragments.Add(new KeyValuePair<string, string>(char.ToUpperInvariant(tv[0]) + tv.Substring(1), char.ToUpperInvariant(te[0]) + te.Substring(1)));
            }
        }

        void AddTemplate(string vi, string en)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            string[] vs = { vi, vi.Trim(), Bare(vi) }, es = { en, en.Trim(), Bare(en) };
            for (int k = 0; k < vs.Length; k++)
            {
                string v = vs[k], e = es[k];
                if (v.Length == 0 || !seen.Add(v)) continue;
                var lits = new List<string>();
                var ids = new List<int>();
                int last = 0, literal = 0;
                string anchor = "";
                foreach (Match h in Hole.Matches(v))
                {
                    lits.Add(v.Substring(last, h.Index - last));
                    ids.Add(int.Parse(h.Groups[1].Value));
                    last = h.Index + h.Length;
                }
                lits.Add(v.Substring(last));
                foreach (string l in lits) { literal += l.Length; if (l.Length > anchor.Length) anchor = l; }
                if (literal == 0) continue;
                templates.Add(new Template { lits = lits.ToArray(), ids = ids.ToArray(), en = e, anchor = anchor, literal = literal });
            }
        }

        const int MaxSplits = 64, MaxTemplates = 4;

        /// <summary>
        /// Moi cach cat chuoi theo mau (lo khong rong). Can thu het vi hai lo canh nhau ("{0} {1} → {2}" voi ten mon nhieu tu
        /// va ten bac hai tu) khong co cach cat duy nhat; chon cach dich it sot chu Viet nhat.
        /// </summary>
        static void Cuts(string s, Template t, int hole, int start, string[] vals, List<string[]> outp)
        {
            if (outp.Count >= MaxSplits) return;
            int n = t.ids.Length;
            if (hole == n - 1)
            {
                int end = s.Length - t.lits[n].Length;
                if (end > start) { vals[hole] = s.Substring(start, end - start); outp.Add((string[])vals.Clone()); }
                return;
            }
            string next = t.lits[hole + 1];
            int limit = s.Length - t.lits[n].Length;
            for (int i = start + 1; i < limit && outp.Count < MaxSplits; i++)
            {
                if (next.Length > 0) { i = s.IndexOf(next, i, StringComparison.Ordinal); if (i < 0 || i >= limit) break; }
                vals[hole] = s.Substring(start, i - start);
                Cuts(s, t, hole + 1, i + next.Length, vals, outp);
            }
        }

        static List<string[]> Match_(string s, Template t)
        {
            var outp = new List<string[]>();
            if (!s.StartsWith(t.lits[0], StringComparison.Ordinal) || !s.EndsWith(t.lits[t.lits.Length - 1], StringComparison.Ordinal)) return outp;
            if (s.Length < t.literal + t.ids.Length) return outp;
            Cuts(s, t, 0, t.lits[0].Length, new string[t.ids.Length], outp);
            return outp;
        }

        public int Count { get { return keys.Count; } }

        /// <summary>Bang co chuoi nay (nguyen van hoac mau) — AC-3.</summary>
        public bool Has(string vi) { return keys.Contains(vi); }

        public string Translate(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            string r;
            if (memo.TryGetValue(s, out r)) return r;
            r = EnglishNumbers(Core(s, 0));
            if (memo.Count > MemoCap) memo.Clear();
            memo[s] = r;
            if (HasVietnamese(r) && Leftovers.Count < LeftoverCap) Leftovers.Add(r);
            return r;
        }

        readonly Dictionary<string, string> coreMemo = new Dictionary<string, string>(StringComparer.Ordinal);

        string Core(string s, int depth)
        {
            string r;
            // chu Viet khong dau tren nut ("Xong", "Mua", "Sau ›") chi co the tra nguyen van
            if (!HasVietnamese(s)) return exact.TryGetValue(s, out r) || (s.Trim().Length > 0 && s.Trim().Length < s.Length && Exact(s, out r)) ? r : s;
            if (coreMemo.TryGetValue(s, out r)) return r;
            r = CoreUncached(s, depth);
            if (coreMemo.Count > MemoCap) coreMemo.Clear();
            coreMemo[s] = r;
            return r;
        }

        static readonly Regex Tags = new Regex(@"^((?:<[^>]+>)+)(.*?)((?:<[^>]+>)*)$|^(.*?)((?:<[^>]+>)+)$", RegexOptions.Singleline | RegexOptions.CultureInvariant);

        string CoreUncached(string s, int depth)
        {
            string r;
            if (Exact(s, out r)) return r;
            // the dinh dang o dau / cuoi ("<size=64>Khách trái"): dich phan giua
            Match tg = Tags.Match(s);
            if (tg.Success)
            {
                string pre = tg.Groups[1].Value, mid = tg.Groups[1].Success && tg.Groups[1].Length > 0 ? tg.Groups[2].Value : tg.Groups[4].Value;
                string post = tg.Groups[1].Length > 0 ? tg.Groups[3].Value : tg.Groups[5].Value;
                if (mid.Length > 0 && mid.Length < s.Length && mid.IndexOf('<') < 0 && Exact(mid, out r)) return pre + r + post;
            }
            if (depth >= MaxDepth) return Fragments(s);

            // Mau cau ghep; neu lo nuot ca dau ngan " · " / xuong dong (vd "Tủ lạnh +{0}" an "2 ô · món không tụt bậc")
            // thi so voi cach tach doan, chon cach khong con sot chu Viet.
            string viaTemplate = null;
            bool wideHole = false;
            int tried = 0;
            foreach (Template t in templates)
            {
                if (t.anchor.Length > 0 && s.IndexOf(t.anchor, StringComparison.Ordinal) < 0) continue;
                List<string[]> cuts = Match_(s, t);
                if (cuts.Count == 0) continue;
                foreach (string[] vals in cuts)
                {
                    bool wide = false;
                    var map = new Dictionary<int, string>();
                    for (int i = 0; i < vals.Length; i++)
                    {
                        if (Splits(vals[i])) wide = true;
                        map[t.ids[i]] = Core(vals[i], depth + 1);
                    }
                    string cand = Hole.Replace(t.en, h => { string v; return map.TryGetValue(int.Parse(h.Groups[1].Value), out v) ? v : h.Value; });
                    // mau dich sach thi lay ca cau (vd "Chuột trái · Nhét vé · còn 5 vé" -> "... · 5 tickets left"), ke ca lo chua dau ngan
                    if (!HasVietnamese(cand)) return cand;
                    if (viaTemplate == null || VietCount(cand) < VietCount(viaTemplate) || (wideHole && !wide && VietCount(cand) == VietCount(viaTemplate)))
                    {
                        viaTemplate = cand;
                        wideHole = wide;
                    }
                }
                if (++tried >= MaxTemplates) break;
            }
            if (viaTemplate != null && !wideHole && !HasVietnamese(viaTemplate)) return viaTemplate;

            string viaSplit = null;
            if (s.IndexOf('\n') >= 0) viaSplit = Join(s, "\n", depth);
            else
                foreach (string sep in Separators)
                    if (s.IndexOf(sep, StringComparison.Ordinal) >= 0) { viaSplit = Join(s, sep, depth); break; }

            string best = viaTemplate != null && viaSplit != null ? (VietCount(viaSplit) <= VietCount(viaTemplate) && (wideHole || !HasVietnamese(viaSplit)) ? viaSplit : viaTemplate) : viaTemplate ?? viaSplit;
            if (best != null && !HasVietnamese(best)) return best;
            // manh vat qua dau ngan (vd " khách · đóng quán lúc "): thay manh tren ca cau, lay ban sot it chu Viet hon
            string whole = Fragments(s);
            return best == null || VietCount(whole) < VietCount(best) ? whole : best;
        }

        static bool Splits(string s)
        {
            if (s.IndexOf('\n') >= 0) return true;
            foreach (string sep in Separators) if (s.IndexOf(sep, StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        string Join(string s, string sep, int depth)
        {
            string[] parts = s.Split(new[] { sep }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++) parts[i] = Core(parts[i], depth + 1);
            return string.Join(sep, parts);
        }

        bool Exact(string s, out string r)
        {
            if (exact.TryGetValue(s, out r)) return true;
            int a = 0, b = s.Length;
            while (a < b && char.IsWhiteSpace(s[a])) a++;
            while (b > a && char.IsWhiteSpace(s[b - 1])) b--;
            string core = s.Substring(a, b - a);
            if (core.Length == 0) return false;
            string hit;
            if ((core.Length != s.Length && exact.TryGetValue(core, out hit)) || LooseHit(core, out hit))
            {
                r = s.Substring(0, a) + hit + s.Substring(b);
                return true;
            }
            return false;
        }

        /// <summary>Chu dau viet hoa ("Khách trái" tu "khách trái") hoac IN HOA ("HẾT VÉ").</summary>
        bool LooseHit(string core, out string hit)
        {
            if (char.IsUpper(core[0]))
            {
                string low = char.ToLowerInvariant(core[0]) + core.Substring(1);
                if (exact.TryGetValue(low, out hit) && hit.Length > 0) { hit = char.ToUpperInvariant(hit[0]) + hit.Substring(1); return true; }
            }
            else
            {
                string cap = char.ToUpperInvariant(core[0]) + core.Substring(1);
                if (exact.TryGetValue(cap, out hit) && hit.Length > 0) { hit = char.ToLowerInvariant(hit[0]) + hit.Substring(1); return true; }
            }
            return upper.TryGetValue(core, out hit) && core == core.ToUpperInvariant();
        }

        string Fragments(string s)
        {
            foreach (KeyValuePair<string, string> f in fragments)
            {
                if (s.Length < f.Key.Length) continue;
                int i = s.IndexOf(f.Key, StringComparison.Ordinal);
                while (i >= 0)
                {
                    int end = i + f.Key.Length;
                    bool startOk = i == 0 || !char.IsLetter(s[i - 1]);
                    bool endOk = end >= s.Length || !char.IsLetter(s[end]);
                    if (startOk && endOk)
                    {
                        s = s.Substring(0, i) + f.Value + s.Substring(end);
                        i = s.IndexOf(f.Key, i + f.Value.Length, StringComparison.Ordinal);
                    }
                    else i = s.IndexOf(f.Key, i + 1, StringComparison.Ordinal);
                }
                if (!HasVietnamese(s)) break;
            }
            return s;
        }

        static int VietCount(string s)
        {
            int n = 0;
            foreach (char c in s) if (IsVietLetter(c)) n++;
            return n;
        }

        static bool IsVietLetter(char c)
        {
            if (c < 'À') return false;
            if (c == 'đ' || c == 'Đ') return true;
            if (c <= 'ư' && char.IsLetter(c)) return true;
            return c >= 'Ạ' && c <= 'ỹ';
        }

        /// <summary>Con chu Viet co dau (a-z co dau, d gach) khong.</summary>
        public static bool HasVietnamese(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) if (IsVietLetter(c)) return true;
            return false;
        }

        static readonly Regex Number = new Regex(@"(?<![\d.,])\d+(?:[.,]\d+)+(?![\d])", RegexOptions.CultureInvariant);
        static readonly Regex ViNumber = new Regex(@"^\d{1,3}(?:\.\d{3})*(?:,\d+)?$", RegexOptions.CultureInvariant);

        /// <summary>So kieu Viet (cham ngan cach hang nghin, phay thap phan) -> kieu Anh. So phien ban, gio giu nguyen.</summary>
        public static string EnglishNumbers(string s)
        {
            if (string.IsNullOrEmpty(s) || (s.IndexOf('.') < 0 && s.IndexOf(',') < 0)) return s;
            return Number.Replace(s, m =>
            {
                string v = m.Value;
                if (!ViNumber.IsMatch(v)) return v;
                var sb = new StringBuilder(v.Length);
                foreach (char c in v) sb.Append(c == '.' ? ',' : c == ',' ? '.' : c);
                return sb.ToString();
            });
        }
    }
}
