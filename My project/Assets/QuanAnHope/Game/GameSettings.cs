using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Cai dat (F-054, constitution 1.5 muc 2.1): am luong tong / nhac / hieu ung 0-100, do nhay chuot, dao truc doc,
    /// toan man hinh / cua so, do phan giai, huong dan ngay 1. Luu qua cac lan mo bang PlayerPrefs.
    /// Day la quy cach dieu khien (Art Bible muc 9), khong phai so can bang.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        /// <summary>Mac dinh Art Bible muc 9: 0,1 do moi diem anh.</summary>
        public const float DefaultSensitivity = 0.1f;
        public const float MinSensitivity = 0.02f;
        public const float MaxSensitivity = 0.4f;

        const string Key = "QuanAnHope.Settings";
        const string GamesKey = "QuanAnHope.GamesStarted";

        public int masterVolume = 80;
        public int musicVolume = 70;
        public int sfxVolume = 80;
        public float sensitivity = DefaultSensitivity;
        public bool invertY;
        public bool fullscreen = true;
        public int resolutionWidth;
        public int resolutionHeight;

        /// <summary>Huong dan ngay 1 (F-053 AC-3): van dau bat; tu van thu hai mac dinh tat (xem <see cref="TutorialForNewGame"/>).</summary>
        public bool tutorial = true;

        /// <summary>Nguoi choi tu bat / tat huong dan trong Cai dat (thi khong tu tat o van thu hai).</summary>
        public bool tutorialChosen;

        /// <summary>Hieu ung truyen tranh (Animation_CotTruyen 1.2): 0 day du, 1 giam (tat boil / rung / zoom giat), 2 tat.</summary>
        public int comicMotion;

        /// <summary>Hieu ung giay cua tranh huong dan (paper-ui-pickit.md muc 4): 0 Day du, 1 Giam, 2 Tat.</summary>
        public int paperMotion;
        public static readonly string[] ComicMotionNames = { "Đầy đủ", "Giảm", "Tắt" };

        /// <summary>F-110 AC-1 (yeu cau 25/11/2026): ngon ngu hien thi (Lang.Vietnamese / Lang.English); -1 = chua chon, lay theo may (AC-6).</summary>
        public int language = -1;

        /// <summary>F-110 AC-6: chua chon thi may tieng Viet -> Tieng Viet, may khac -> English; chay batchmode / test -> Tieng Viet.</summary>
        public static int DefaultLanguage(bool systemIsVietnamese, bool batchMode)
        {
            return systemIsVietnamese || batchMode ? Lang.Vietnamese : Lang.English;
        }

        /// <summary>Ten khoa PlayerPrefs (test doi sang khoa rieng de khong dong vao cai dat that).</summary>
        public static string Prefix = "";

        public static GameSettings Load()
        {
            string json = PlayerPrefs.GetString(Prefix + Key, "");
            GameSettings s = null;
            if (!string.IsNullOrEmpty(json))
            {
                try { s = JsonUtility.FromJson<GameSettings>(json); }
                catch (ArgumentException) { s = null; }
            }
            if (s == null) s = new GameSettings();
            if (s.language < 0)
                s.language = DefaultLanguage(Application.systemLanguage == SystemLanguage.Vietnamese, Application.isBatchMode || Application.isEditor);
            s.Clamp();
            return s;
        }

        public void Save()
        {
            Clamp();
            PlayerPrefs.SetString(Prefix + Key, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        public static void ClearSaved()
        {
            PlayerPrefs.DeleteKey(Prefix + Key);
            PlayerPrefs.DeleteKey(Prefix + GamesKey);
        }

        void Clamp()
        {
            masterVolume = Mathf.Clamp(masterVolume, 0, 100);
            musicVolume = Mathf.Clamp(musicVolume, 0, 100);
            sfxVolume = Mathf.Clamp(sfxVolume, 0, 100);
            if (sensitivity <= 0f) sensitivity = DefaultSensitivity;
            sensitivity = Mathf.Clamp(sensitivity, MinSensitivity, MaxSensitivity);
            if (language > Lang.English) language = Lang.Vietnamese;
        }

        /// <summary>So van da bat dau (de tat huong dan tu van thu hai).</summary>
        public static int GamesStarted
        {
            get { return PlayerPrefs.GetInt(Prefix + GamesKey, 0); }
            set { PlayerPrefs.SetInt(Prefix + GamesKey, value); PlayerPrefs.Save(); }
        }

        /// <summary>Van moi co huong dan khong: nguoi choi da chon thi theo lua chon; chua chon thi chi van dau.</summary>
        public bool TutorialForNewGame(int gamesStartedBefore)
        {
            return tutorialChosen ? tutorial : gamesStartedBefore == 0;
        }

        /// <summary>Am luong that cua hieu ung / nhac 0..1 (tong x rieng). Am thanh that o sprint 7.</summary>
        public float SfxLevel { get { return masterVolume / 100f * sfxVolume / 100f; } }
        public float MusicLevel { get { return masterVolume / 100f * musicVolume / 100f; } }

        /// <summary>Ap cai dat vao game: am luong tong, chuot, man hinh.</summary>
        public void Apply(PlayerController player, bool screen)
        {
            AudioListener.volume = masterVolume / 100f;
            Lang.Set(language < 0 ? Lang.Vietnamese : language);
            PaperKit.Motion = Mathf.Clamp(paperMotion, 0, PaperKit.MotionNames.Length - 1);
            if (player != null)
            {
                player.LookDegreesPerPixel = sensitivity;
                player.InvertY = invertY;
            }
            if (!screen || Application.isEditor || Application.isBatchMode) return;
            Resolution r = PickResolution();
            Screen.SetResolution(r.width, r.height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        /// <summary>Do phan giai dang chon; chua chon (0) thi lay do phan giai goc cua man hinh.</summary>
        public Resolution PickResolution()
        {
            Resolution native = Screen.currentResolution;
            if (resolutionWidth <= 0 || resolutionHeight <= 0) return native;
            foreach (Resolution r in Choices())
                if (r.width == resolutionWidth && r.height == resolutionHeight) return r;
            return native;
        }

        /// <summary>Cac do phan giai khac nhau cua man hinh (bo trung tan so quet), tu nho toi lon.</summary>
        public static List<Resolution> Choices()
        {
            var list = new List<Resolution>();
            foreach (Resolution r in Screen.resolutions)
            {
                if (r.width < 1024 || r.height < 576) continue;
                if (list.Exists(x => x.width == r.width && x.height == r.height)) continue;
                list.Add(r);
            }
            if (list.Count == 0) list.Add(Screen.currentResolution);
            list.Sort((a, b) => a.width != b.width ? a.width.CompareTo(b.width) : a.height.CompareTo(b.height));
            return list;
        }

        public string ResolutionText
        {
            get
            {
                if (resolutionWidth <= 0) return "Theo màn hình";
                return resolutionWidth + " × " + resolutionHeight;
            }
        }

        /// <summary>Doi sang do phan giai ke tiep trong danh sach (nut bam vong tron).</summary>
        public void NextResolution()
        {
            List<Resolution> list = Choices();
            int i = list.FindIndex(r => r.width == resolutionWidth && r.height == resolutionHeight);
            if (i + 1 >= list.Count) { resolutionWidth = 0; resolutionHeight = 0; return; }
            resolutionWidth = list[i + 1].width;
            resolutionHeight = list[i + 1].height;
        }
    }
}
