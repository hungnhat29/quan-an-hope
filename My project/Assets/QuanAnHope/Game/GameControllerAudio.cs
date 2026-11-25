using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// F-061 (sprint 7, Art Bible muc 11): tieng cho moi thao tac chinh, phat tai do vat trong phong (am 3D); tieng lap cua bep
    /// theo dung cu (chao / vi nuong xeo, noi soi); buoc chan; nhac nen theo ngay; am luong theo Cai dat.
    /// </summary>
    public partial class GameController
    {
        public GameAudio Audio { get; private set; }

        /// <summary>Nhip thai khi giu chuot tren thot (giay).</summary>
        public const float ChopPeriod = 0.22f;
        /// <summary>Moi buoc chan sau bay nhieu met di bo.</summary>
        public const float StepMeters = 0.75f;

        float chopTimer;
        float stepDistance;
        int stepIndex;
        Vector3 lastFeet;
        bool feetKnown;
        readonly List<KeyValuePair<float, System.Action>> later = new List<KeyValuePair<float, System.Action>>();

        void InitAudio()
        {
            Audio = GameAudio.Create(transform);
            ApplyAudioSettings();
        }

        /// <summary>Mau phu theo gio (Art Bible muc 4, F-062 AC-4); chi khi dang choi trong phong.</summary>
        public ScreenTint Tint { get; private set; }

        void InitAmbient()
        {
            // Sprint 8 B-01: bo lop phu anh mau toan man hinh (lam nhat mau, cuoi ngay tim). Mau theo gio nay nhan vao den chinh
            // (RoomView.TickLights, Art Bible muc 10 "doi gio = doi mau / cuong do cua den").
            Tint = null;
        }

        /// <summary>Mua ngay 2 ngoai cua: vet mua + pho nhuom mau mua.</summary>
        void TickAmbient()
        {
            if (Room == null) return;
            bool rain = RoomView.IsRainDay(Session) && Mode != GameMode.Menu;
            if (Room.Rain != null) Room.Rain.SetRaining(rain);
            if (Room.Sky != null) Room.Sky.Rain = rain ? 1f : 0f;
        }

        void ApplyAudioSettings()
        {
            if (Audio == null || Settings == null) return;
            Audio.SetLevels(Settings.sfxVolume / 100f, Settings.musicVolume / 100f);
        }

        /// <summary>Diem giua cua mot tram trong phong; khong co thi vi tri nguoi choi.</summary>
        public Vector3 SoundAt(string station)
        {
            Interactable st = Room != null ? Room.Station(station) : null;
            if (st != null) return st.WorldCenter;
            return Player != null ? Player.transform.position : Vector3.zero;
        }

        /// <summary>Tay nguoi choi: truoc mat, thap hon tam mat (tieng cam / dat).</summary>
        Vector3 HandPosition
        {
            get
            {
                if (Player == null || Player.Cam == null) return Vector3.zero;
                Transform c = Player.Cam.transform;
                return c.position + c.forward * 0.5f - c.up * 0.3f;
            }
        }

        void Sfx(string clip, string station, float volume = 1f)
        {
            if (Audio != null) Audio.Play(clip, SoundAt(station), volume);
        }

        void SfxHand(string clip, float volume = 1f)
        {
            if (Audio != null) Audio.Play(clip, HandPosition, volume);
        }

        void SfxLater(float seconds, string clip, string station, float volume = 1f)
        {
            Vector3 at = SoundAt(station);
            later.Add(new KeyValuePair<float, System.Action>(Time.time + seconds, () => { if (Audio != null) Audio.Play(clip, at, volume); }));
        }

        /// <summary>Tieng khi thung roi: nhet ve ngay, thung cham ban; bac 4-5 lo ra co tieng rieng (Art Bible muc 11).</summary>
        void SfxPull(PullResult pull, bool black)
        {
            Sfx("nhet_ve", "Kiosk");
            SfxLater(0.15f, "thung_roi", "Kiosk");
            // tieng lo bac 4-5 phat luc mon lo ra (ItemView.Tick)
        }

        /// <summary>Tieng phuc vu: dat dia, khach vui (+ tien) / buon / gian; chu no thuong thi them tien.</summary>
        void SfxServe(int seat, PaymentResult paid, bool boss)
        {
            string st = "Customer_" + seat;
            Sfx("dat", st, 0.8f);
            if (paid.wrongOrder || paid.outcome == PaymentOutcome.Angry) { Sfx("khach_gian", st); return; }
            if (paid.outcome == PaymentOutcome.Satisfied || (boss && paid.bossBonus > 0))
            {
                Sfx("khach_vui", st);
                SfxLater(0.25f, "tien", st);
                if (paid.bossBonus > 0) SfxLater(0.5f, "tien", st);
                return;
            }
            SfxLater(0.1f, "tien", st, 0.5f);
        }

        // ---------- hieu ung hinh (sprint 7 F-062 AC-1, Art Bible muc 11) ----------

        /// <summary>Hieu ung cuoi cung (test): "tim+xu", "xoay_vang", "khoi".</summary>
        public string LastFx { get; private set; }

        /// <summary>Khach vui / tip: trai tim bay len tren dau khach + dong xu bay ve o Vi (0,8 giay).</summary>
        void FxServe(int seat, PaymentResult paid, bool boss)
        {
            if (Room == null || seat < 0 || seat >= Room.SeatViews.Count) return;
            bool happy = !paid.wrongOrder && (paid.outcome == PaymentOutcome.Satisfied || (boss && paid.bossBonus > 0));
            if (!happy || paid.amount <= 0) return;
            RoomView.SeatView v = Room.SeatViews[seat];
            Transform head = v.Mouth != null ? v.Mouth : v.Pivot;
            RiseFx.Play(head, new Vector3(0f, 0.75f, 0f), paid.bossBonus > 0 ? "tim_vang" : "tim", 2, 0.8f, 0.35f, 0.16f);
            if (Hud != null) Hud.FlyCoin(head.position + Vector3.up * 0.5f);
            LastFx = "tim+xu";
        }

        /// <summary>Nuoc sot: thanh cong xoay vang quanh dia, hong khoi xam (1,0 giay).</summary>
        void FxSauce(bool failed)
        {
            if (Player == null || Player.Cam == null) return;
            Transform cam = Player.Cam.transform;
            Vector3 local = new Vector3(0f, -0.3f, 0.6f);
            if (failed) RiseFx.Play(cam, local, "khoi", 3, 1f, 0.3f, 0.09f);
            else RiseFx.Play(cam, local, "xoay_vang", 5, 1f, 0.08f, 0.07f, 0.12f, 360f);
            LastFx = failed ? "khoi" : "xoay_vang";
        }

        /// <summary>Chay / Lac vi: khoi gap khuc bay len tu bep (0,8 giay); rung camera do noi goi lam.</summary>
        void FxSmoke(string station)
        {
            if (Room == null) return;
            Interactable st = Room.Station(station);
            if (st == null) return;
            RiseFx.Play(st.transform, st.transform.InverseTransformPoint(st.WorldCenter + Vector3.up * 0.25f), "khoi", 3, 0.8f, 0.4f, 0.1f);
            LastFx = "khoi";
        }

        /// <summary>Moi khung hinh: am luong, nhac, tieng lap cua bep, buoc chan, tieng hen gio.</summary>
        void TickAudio(float dt)
        {
            TickAmbient();
            if (Audio == null) return;
            ApplyAudioSettings();
            Audio.SetMusic(MusicFor());

            for (int i = later.Count - 1; i >= 0; i--)
            {
                if (Time.time < later[i].Key) continue;
                System.Action a = later[i].Value;
                later.RemoveAt(i);
                a();
            }

            bool playing = Mode == GameMode.Playing && Session != null;
            Stove stove = playing ? Session.Stove : null;
            bool cooking = stove != null && (stove.State == StoveState.Cooking || stove.IsOverheating);
            bool boil = stove != null && stove.Tool == "Nồi";
            Vector3 at = SoundAt("Stove");
            Audio.Loop("bep_xeo", "xeo_loop", at, cooking && !boil, stove != null && stove.IsOverheating ? 1f : 0.7f);
            Audio.Loop("bep_soi", "soi_loop", at, cooking && boil, 0.8f);
            // sau sprint 10: bep thu hai cung keu
            Stove st2 = playing && Session.Stoves.Count > 1 ? Session.Stoves[1] : null;
            bool cooking2 = st2 != null && (st2.State == StoveState.Cooking || st2.IsOverheating);
            bool boil2 = st2 != null && st2.Tool == "Nồi";
            Vector3 at2 = st2 != null ? SoundAt(StoveId(1)) : at;
            Audio.Loop("bep_xeo_2", "xeo_loop", at2, cooking2 && !boil2, st2 != null && st2.IsOverheating ? 1f : 0.7f);
            Audio.Loop("bep_soi_2", "soi_loop", at2, cooking2 && boil2, 0.8f);

            // buoc chan: theo quang duong di tren mat san
            if (playing && Player != null)
            {
                Vector3 feet = Player.transform.position;
                feet.y = 0f;
                if (feetKnown && !Player.CameraOverride)
                {
                    stepDistance += (feet - lastFeet).magnitude;
                    if (stepDistance >= StepMeters)
                    {
                        stepDistance = 0f;
                        Audio.Play("buoc_chan_" + (stepIndex++ % 4), Player.transform.position + Vector3.down * 0.2f, 0.55f, 0.08f);
                    }
                }
                lastFeet = feet;
                feetKnown = true;
            }
            else feetKnown = false;
        }

        /// <summary>Nhac nen (F-061 AC-3): ngay mat dien va ngay chu no co nhac rieng; menu / truyen dung nhac phong.</summary>
        public string MusicFor()
        {
            if (Session == null || Mode == GameMode.Menu || Mode == GameMode.Story) return GameAudio.MusicRoom;
            if (Session.Phase == GamePhase.Lost || Session.Phase == GamePhase.Cleared) return GameAudio.MusicRoom;
            if (Session.IsBlackout) return GameAudio.MusicBlackout;
            if (Session.IsBossDay) return GameAudio.MusicBoss;
            return GameAudio.MusicRoom;
        }

        /// <summary>Thai lap khi giu chuot (impl plan F-061 AC-1: "thai (lap khi giu)").</summary>
        void SfxChop(float seconds)
        {
            chopTimer -= seconds;
            if (chopTimer > 0f) return;
            chopTimer = ChopPeriod;
            Sfx("thai", "Board");
        }
    }
}
