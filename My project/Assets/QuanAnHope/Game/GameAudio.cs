using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// F-061 (sprint 7): am thanh cua game. Tieng tu sinh bang tools/am-thanh/make_audio.py (Resources/Audio, ghi nguon o
    /// docs/CREDITS.md). Tieng trong phong la am 3D (gan to, xa nho: tat tuyen tinh 1,2 -> 16 m, phong 9,6 m); nhac nen 2D.
    /// Am luong: AudioListener.volume = am luong tong (GameSettings.Apply), nguon hieu ung theo sfxVolume, nhac theo musicVolume.
    /// Nhac: phong (ngay thuong), mat dien (ngay blackout), ngay chu no (constitution 1.4 muc 2) - F-061 AC-3.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        public const string MusicRoom = "nhac_phong";
        public const string MusicBlackout = "nhac_mat_dien";
        public const string MusicBoss = "nhac_ngay21";

        public const float MinDistance = 1.2f;
        public const float MaxDistance = 16f;
        const int PoolSize = 16;
        const float MusicFade = 1.2f;

        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly List<AudioSource> pool = new List<AudioSource>();
        readonly Dictionary<string, AudioSource> loops = new Dictionary<string, AudioSource>();
        readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        AudioSource music, musicOld;
        float musicT = 1f;
        int next;

        float sfx = 0.8f, musicLevel = 0.7f;

        /// <summary>Ten nhac dang phat (test).</summary>
        public string MusicName { get; private set; }
        /// <summary>Tieng phat gan nhat va noi phat (test: "tieng trong phong la am 3D").</summary>
        public string LastPlayed { get; private set; }
        public AudioSource LastSource { get; private set; }

        /// <summary>Bo am thanh dang dung (do vat tu phat tieng, vd mon lo bac 4-5).</summary>
        public static GameAudio Current { get; private set; }

        public static GameAudio Create(Transform parent)
        {
            var go = new GameObject("AmThanh");
            go.transform.SetParent(parent, false);
            var a = go.AddComponent<GameAudio>();
            a.Load();
            Current = a;
            return a;
        }

        readonly Dictionary<string, float> lastAt = new Dictionary<string, float>();

        /// <summary>Phat neu tieng nay chua phat trong <paramref name="minGap"/> giay (nhieu mon lo cung luc chi mot tieng).</summary>
        public AudioSource PlayThrottled(string clip, Vector3 at, float minGap)
        {
            float last;
            if (lastAt.TryGetValue(clip, out last) && Time.unscaledTime - last < minGap) return null;
            lastAt[clip] = Time.unscaledTime;
            return Play(clip, at);
        }

        /// <summary>F-092 AC-2: nap san du lieu mọi tieng (lan phat dau khong phai giai nen trong khung hinh dang choi).</summary>
        public void Preload()
        {
            foreach (AudioClip c in clips.Values) if (c != null && c.loadState != AudioDataLoadState.Loaded) c.LoadAudioData();
        }

        void Load()
        {
            foreach (AudioClip c in Resources.LoadAll<AudioClip>("Audio")) clips[c.name] = c;
            for (int i = 0; i < PoolSize; i++) pool.Add(NewSource("Nguon_" + i, true));
            music = NewSource("Nhac", false);
            musicOld = NewSource("NhacCu", false);
            music.loop = musicOld.loop = true;
        }

        AudioSource NewSource(string name, bool spatial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = spatial ? 1f : 0f;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.minDistance = MinDistance;
            s.maxDistance = MaxDistance;
            s.dopplerLevel = 0f;
            return s;
        }

        public bool Has(string clip) { return clips.ContainsKey(clip); }
        public int ClipCount { get { return clips.Count; } }

        /// <summary>So lan da phat mot tieng (test).</summary>
        public int Count(string clip)
        {
            int n;
            return counts.TryGetValue(clip, out n) ? n : 0;
        }

        /// <summary>Am luong theo Cai dat (0..1). Am luong tong nam o AudioListener.volume.</summary>
        public void SetLevels(float sfxLevel, float musicLevel01)
        {
            sfx = Mathf.Clamp01(sfxLevel);
            musicLevel = Mathf.Clamp01(musicLevel01);
            foreach (AudioSource s in loops.Values) s.volume = sfx * LoopGain(s);
            ApplyMusicVolume();
        }

        public float SfxLevel { get { return sfx; } }
        public float MusicLevel { get { return musicLevel; } }

        /// <summary>Phat mot tieng 3D tai diem trong phong; pitch lech nhe de khong nham chan.</summary>
        public AudioSource Play(string clip, Vector3 at, float volume = 1f, float pitchJitter = 0.04f)
        {
            AudioClip c;
            if (!clips.TryGetValue(clip, out c)) return null;
            AudioSource s = pool[next];
            next = (next + 1) % pool.Count;
            s.Stop();
            s.transform.position = at;
            s.clip = c;
            s.volume = sfx * Mathf.Clamp01(volume);
            s.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            s.loop = false;
            s.Play();
            int n;
            counts.TryGetValue(clip, out n);
            counts[clip] = n + 1;
            LastPlayed = clip;
            LastSource = s;
            return s;
        }

        /// <summary>Bat / tat mot tieng lap (xeo, soi) tai mot diem. Goi moi khung hinh duoc.</summary>
        public void Loop(string key, string clip, Vector3 at, bool on, float volume = 1f)
        {
            AudioSource s;
            loops.TryGetValue(key, out s);
            if (!on)
            {
                if (s != null && s.isPlaying) s.Stop();
                return;
            }
            AudioClip c;
            if (!clips.TryGetValue(clip, out c)) return;
            if (s == null)
            {
                s = NewSource("Lap_" + key, true);
                s.loop = true;
                loops[key] = s;
            }
            s.transform.position = at;
            gains[s] = volume;
            s.volume = sfx * volume;
            if (s.clip != c || !s.isPlaying)
            {
                s.clip = c;
                s.Play();
                int n;
                counts.TryGetValue(clip, out n);
                counts[clip] = n + 1;
            }
        }

        readonly Dictionary<AudioSource, float> gains = new Dictionary<AudioSource, float>();
        float LoopGain(AudioSource s) { float g; return gains.TryGetValue(s, out g) ? g : 1f; }

        public bool IsLooping(string key)
        {
            AudioSource s;
            return loops.TryGetValue(key, out s) && s.isPlaying;
        }

        public void StopLoops()
        {
            foreach (AudioSource s in loops.Values) s.Stop();
        }

        /// <summary>Doi nhac nen (cross-fade 1,2 giay); null = tat nhac.</summary>
        public void SetMusic(string clip)
        {
            if (clip == MusicName) return;
            MusicName = clip;
            AudioClip c = null;
            if (clip != null) clips.TryGetValue(clip, out c);
            AudioSource tmp = musicOld; musicOld = music; music = tmp;
            music.Stop();
            music.clip = c;
            if (c != null) music.Play();
            musicT = 0f;
            ApplyMusicVolume();
        }

        public bool MusicPlaying { get { return music != null && music.isPlaying; } }
        public float MusicVolumeNow { get { return music != null ? music.volume : 0f; } }

        void ApplyMusicVolume()
        {
            if (music == null) return;
            float k = Mathf.SmoothStep(0f, 1f, musicT);
            music.volume = musicLevel * k;
            musicOld.volume = musicLevel * (1f - k);
            if (k >= 1f && musicOld.isPlaying) musicOld.Stop();
        }

        void Update()
        {
            if (musicT < 1f)
            {
                musicT = Mathf.Min(1f, musicT + Time.unscaledDeltaTime / MusicFade);
                ApplyMusicVolume();
            }
        }

        /// <summary>Tam dung / tiep tuc (menu tam dung): dung ca tieng lap.</summary>
        public void PauseAll(bool paused)
        {
            foreach (AudioSource s in loops.Values) { if (paused) s.Pause(); else s.UnPause(); }
            foreach (AudioSource s in pool) { if (paused) s.Pause(); else s.UnPause(); }
            if (voice != null) { if (paused) voice.Pause(); else voice.UnPause(); }
        }

        // ---------------- giong khach dac biet (sau sprint 10) ----------------

        AudioSource voice;
        float voiceLeft, voiceFade;

        /// <summary>
        /// Giong khach dac biet khi vao quan (file nguoi dung dua 23/11/2026): am 2D (nghe ro o moi cho trong phong), chi phat
        /// <paramref name="maxSeconds"/> giay dau roi nho dan (clip goc dai 5-42 giay). Giong moi thay giong cu.
        /// </summary>
        public AudioSource PlayVoice(string clip, float maxSeconds, float fadeSeconds = 0.8f, float volume = 1f)
        {
            AudioClip c;
            if (!clips.TryGetValue(clip, out c)) return null;
            if (voice == null) voice = NewSource("GiongKhach", false);
            voice.Stop();
            voice.clip = c;
            voice.loop = false;
            voice.pitch = 1f;
            voice.volume = sfx * Mathf.Clamp01(volume);
            voice.Play();
            voiceLeft = Mathf.Min(maxSeconds, c.length);
            voiceFade = fadeSeconds;
            int n;
            counts.TryGetValue(clip, out n);
            counts[clip] = n + 1;
            LastPlayed = clip;
            LastSource = voice;
            return voice;
        }

        public bool VoicePlaying { get { return voice != null && voice.isPlaying; } }

        void LateUpdate()
        {
            if (voice == null || !voice.isPlaying) return;
            voiceLeft -= Time.unscaledDeltaTime;
            if (voiceLeft <= 0f) { voice.Stop(); return; }
            if (voiceLeft < voiceFade) voice.volume = sfx * Mathf.Clamp01(voiceLeft / Mathf.Max(0.01f, voiceFade));
        }
    }
}
