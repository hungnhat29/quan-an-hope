using System;
using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// F-110 (specs/sprint-11/spec.md, yeu cau 25/11/2026): ngon ngu hien thi. Moi chu TMP tao o UiKit.Text / ComicBook gan
    /// <see cref="Preprocessor"/>: chu HIEN THI duoc dich, con .text (code, du lieu, test) giu tieng Viet nen luat choi khong doi.
    /// Ban dich: Resources/Lang/en.json (sinh bang tools/i18n/build.py).
    /// </summary>
    public static class Lang
    {
        public const int Vietnamese = 0, English = 1;
        public static readonly string[] Names = { "Tiếng Việt", "English" };
        const string TableResource = "Lang/en";

        [Serializable] class Pair { public string vi; public string en; }
        [Serializable] class Table_ { public Pair[] pairs; }

        sealed class Pre : ITextPreprocessor
        {
            public string PreprocessText(string text)
            {
                if (current != English || text == null) return text;
                string r = Table.Translate(text);
                if (ForceLog != null && Translator.HasVietnamese(r) && logged.Add(r))
                    System.IO.File.AppendAllText(ForceLog, text.Replace("\n", "\\n") + "\t" + r.Replace("\n", "\\n") + "\n");
                return r;
            }
        }

        /// <summary>
        /// Chan doan F-110 AC-4 (chi khi chay thu): bien moi truong QAH_FORCE_EN_LOG = duong dan file -> luon tieng Anh, moi chu hien thi
        /// con chu Viet ghi vao file. Dung de chay ca bo PlayMode cu bang tieng Anh. Mac dinh null (tat).
        /// </summary>
        public static readonly string ForceLog = Environment.GetEnvironmentVariable("QAH_FORCE_EN_LOG");
        static readonly HashSet<string> logged = new HashSet<string>();

        static int current = ForceLog != null ? English : Vietnamese;
        static Translator table;

        public static readonly ITextPreprocessor Preprocessor = new Pre();

        public static int Current { get { return current; } }

        /// <summary>Bang dich tieng Anh (nap mot lan tu Resources).</summary>
        public static Translator Table
        {
            get
            {
                if (table != null) return table;
                var list = new List<KeyValuePair<string, string>>();
                TextAsset a = Resources.Load<TextAsset>(TableResource);
                if (a != null)
                {
                    Table_ t = JsonUtility.FromJson<Table_>(a.text);
                    if (t != null && t.pairs != null)
                        foreach (Pair p in t.pairs) list.Add(new KeyValuePair<string, string>(p.vi, p.en));
                }
                else Debug.LogWarning("Quán Ăn Hope: thiếu Resources/Lang/en.json (chạy tools/i18n/build.py).");
                table = new Translator(list);
                return table;
            }
        }

        /// <summary>Doi ngon ngu; chu dang co tren man hinh ve lai ngay (AC-1).</summary>
        public static void Set(int language)
        {
            language = Mathf.Clamp(language, Vietnamese, English);
            if (ForceLog != null) language = English;
            if (language == current) return;
            current = language;
            Refresh();
        }

        /// <summary>Gan bo dich vao mot chu TMP (UiKit.Text, ComicBook).</summary>
        public static void Attach(TMP_Text t)
        {
            if (t != null) t.textPreprocessor = Preprocessor;
        }

        /// <summary>Chu hien thi cua mot chuoi tieng Viet (cho cho khong qua TMP).</summary>
        public static string T(string vi)
        {
            return current == English && vi != null ? Table.Translate(vi) : vi;
        }

        static void Refresh()
        {
            foreach (TMP_Text t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.textPreprocessor == null) t.textPreprocessor = Preprocessor;
                if (t.gameObject.activeInHierarchy) t.ForceMeshUpdate(false, true);
                else t.SetAllDirty();
                MarkReparse(t);
            }
        }

        /// <summary>Chu dang an: lan bat len phai phan tich lai (TMP chi phan tich lai khi .text doi).</summary>
        static void MarkReparse(TMP_Text t)
        {
            if (t.gameObject.activeInHierarchy) return;
            string s = t.text;
            t.text = string.Empty;
            t.text = s;
        }
    }
}
