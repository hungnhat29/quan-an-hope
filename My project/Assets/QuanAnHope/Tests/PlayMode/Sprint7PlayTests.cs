using System.Collections;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Gia lap nguoi choi sprint 7 (docs/sprint-7/impl-plan.md + sua truoc sprint 21/11/2026): ngay 1 co 5 ve, dong ho dung
    /// toi khi mo quan; am thanh (F-061); hieu ung, bieu cam, chop mat, mau theo gio, mua ngay 2 (F-062). Anh: docs/sprint-7/ui.
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint7 = "sprint-7";

        // ---------- sua truoc sprint 7 ----------

        [UnityTest]
        public IEnumerator Truoc7_Ngay1_5Ve_DongHoDungToiKhiMoQuan()
        {
            Assert.AreEqual(5, S.Wallet.Count, "ngay 1 chi co 5 ve");
            StringAssert.Contains("5 vé", HudText("HUD_Phai"));
            yield return Use("Kiosk");
            yield return new WaitForSeconds(2f);                 // 50 giay game o toc do tua
            Assert.AreEqual("9:00", S.Clock.Text, "chua mo quan: dong ho dung o 9:00");
            StringAssert.Contains("chưa mở quán", HudText("HUD_Trai"));
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.Open, S.Phase);
            yield return new WaitForSeconds(2f);
            Assert.AreNotEqual("9:00", S.Clock.Text, "mo quan roi dong ho moi chay");
            StringAssert.DoesNotContain("chưa mở quán", HudText("HUD_Trai"));
            yield return Shot("dong_ho_sau_khi_mo.png", g.Room.Station("ShopSign").WorldCenter, Sprint7);
        }

        // ---------- F-061 am thanh ----------

        static readonly string[] RequiredClips =
        {
            "nhet_ve", "thung_roi", "cam", "dat", "thai", "cho_vao_bep", "xeo_loop", "soi_loop", "chin", "muc", "chuong", "tien",
            "khach_vui", "khach_gian", "mua_relic", "thu_no", "bac_4", "bac_5", "buoc_chan_0",
            GameAudio.MusicRoom, GameAudio.MusicBlackout, GameAudio.MusicBoss,
        };

        [UnityTest]
        public IEnumerator F061_AmThanh_ThaoTacChinh_Am3D_TheoCaiDat()
        {
            GameAudio a = g.Audio;
            Assert.IsNotNull(a, "co bo am thanh");
            foreach (string clip in RequiredClips) Assert.IsTrue(a.Has(clip), "thieu tieng " + clip);

            // AC-1, AC-4: nhet ve -> tieng tai may, am 3D (gan to xa nho)
            yield return Use("Kiosk");
            Assert.Greater(a.Count("nhet_ve"), 0, "nhet ve co tieng");
            Assert.AreEqual(1f, a.LastSource.spatialBlend, 1e-4, "tieng trong phong la am 3D");
            Assert.AreEqual(AudioRolloffMode.Linear, a.LastSource.rolloffMode);
            Assert.Less(Vector3.Distance(a.LastSource.transform.position, g.Room.Station("Kiosk").WorldCenter), 0.01f, "phat tai may nhet ve");
            yield return WaitUntil(() => a.Count("thung_roi") > 0, 2f, "thung roi co tieng");

            // cam, dat len thot, thai lap khi giu, cho vao bep, xeo / soi theo dung cu, chin, muc
            Ingredient side = S.Stock.Items.First(i => i.kind == IngredientKind.Side);
            yield return TakeFromCrate(side);
            Assert.Greater(a.Count("cam"), 0, "cam co tieng");
            int dat = a.Count("dat");
            yield return Use("Board");
            Assert.Greater(a.Count("dat"), dat, "dat len thot co tieng");
            int chops = a.Count("thai");
            Time.timeScale = 1f;
            yield return HoldOn("Board", () => g.OnBoard == null || g.OnBoard.prepared, 15f);
            Time.timeScale = FastForward;
            Assert.GreaterOrEqual(a.Count("thai") - chops, 2, "thai lap khi giu chuot");
            // cam mon da so che len roi dat lai vao thung (chuot phai) de nau bang kich ban chung
            yield return Use("Board");
            Assert.AreEqual(HandKind.Ingredient, g.Hand);
            LookAtFloorAway();
            P.TapSecondary();
            yield return null;

            // xeo / soi va chin: nau mot mon tron ven
            bool sawLoop = false;
            yield return CookOne(true, null, null, () => Watch(() => sawLoop |= a.IsLooping("bep_xeo") || a.IsLooping("bep_soi")));
            Assert.IsTrue(sawLoop, "bep dang nau co tieng xeo / soi");
            Assert.Greater(a.Count("cho_vao_bep"), 0);
            Assert.Greater(a.Count("chin"), 0, "mon chin co tieng");
            Assert.Greater(a.Count("muc"), 0, "muc ra dia co tieng");

            // chuong + khach vui + tien
            yield return Use("ShopSign");
            yield return ServeOne();
            Assert.Greater(a.Count("chuong"), 0, "chuong co tieng");
            Assert.Greater(a.Count("khach_vui") + a.Count("khach_gian") + a.Count("tien"), 0, "khach phan ung co tieng");

            // AC-4: am luong theo Cai dat
            int sfx0 = g.Settings.sfxVolume, music0 = g.Settings.musicVolume;
            g.Settings.sfxVolume = 0; g.Settings.musicVolume = 50;
            yield return null;
            Assert.AreEqual(0f, a.SfxLevel, 1e-4, "hieu ung 0");
            Assert.AreEqual(0.5f, a.MusicLevel, 1e-4, "nhac 50");
            g.Settings.sfxVolume = sfx0; g.Settings.musicVolume = music0;

            // AC-3: nhac nen phong; ngay mat dien va ngay chu no nhac khac
            Assert.AreEqual(GameAudio.MusicRoom, a.MusicName);
            Assert.IsTrue(a.MusicPlaying, "nhac nen dang chay");
        }

        IEnumerator Watch(System.Action each)
        {
            each();
            yield return null;
            each();
        }

        [UnityTest]
        public IEnumerator F061_NhacRieng_NgayMatDien_VaNgayChuNo()
        {
            yield return ToDay(S.Balance.blackout.day, 0);
            yield return null;
            Assert.AreEqual(GameAudio.MusicBlackout, g.Audio.MusicName, "ngay mat dien");
            yield return ToDay(S.Balance.day.bossDay, 0);
            yield return null;
            Assert.AreEqual(GameAudio.MusicBoss, g.Audio.MusicName, "ngay chu no");
        }

        [UnityTest]
        public IEnumerator F061_Bac4_5_CoTiengRieng()
        {
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            int before = g.Audio.Count("bac_4") + g.Audio.Count("bac_5");
            // thung that co mon bac 5: qua ve den / relic hiem khi ra; dung lenh go loi them thung roi phat lai luong tieng
            Assert.IsTrue(g.DebugAddCrate(DebugCrate(5)), "lenh go loi them thung");
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(g.Audio.Count("bac_4") + g.Audio.Count("bac_5"), before, "lo bac 5 co tieng rieng");
        }

        Crate DebugCrate(int tier)
        {
            Ingredient any = S.Stock.Items.First(i => i.kind == IngredientKind.Main);
            var c = new Crate { kind = IngredientKind.Main, typeId = any.typeId, name = any.name };
            Ingredient copy = Ingredient.FromMain(S.Recipes.mains.First(m => m.id == any.typeId), tier);
            c.items.Add(copy);
            return c;
        }

        // ---------- F-062 hieu ung, bieu cam, chop mat, mau theo gio, mua ----------

        [UnityTest]
        public IEnumerator F062_ThungRoiQuaCongXoanOc_LapLanh()
        {
            yield return Use("Kiosk");
            yield return null;
            CrateView crate = Object.FindObjectsByType<CrateView>(FindObjectsInactive.Exclude).FirstOrDefault(c => c.Portal != null);
            Assert.IsNotNull(crate, "thung moi roi co cong xoan oc");
            Time.timeScale = 1f;
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(crate.Portal.Openness, 0.5f, "xoan oc dang mo");
            yield return Shot("cong_xoan_oc.png", crate.Portal.transform.position + Vector3.down * 0.5f, Sprint7);
            yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(crate.Sparkled, "thung cham ban thi lap lanh");
            Assert.IsTrue(crate.Portal == null, "xoan oc dong lai sau 1,2 giay");
            Time.timeScale = FastForward;
        }

        [UnityTest]
        public IEnumerator F062_KhachVui_TimVaDongXuBayVeVi_DuBieuCam()
        {
            // AC-2: bang Art Bible muc 6
            Assert.AreEqual(FaceArt.MoodHappy, FaceArt.CustomerMood(true, PaymentOutcome.Satisfied, 2, true, false, false));
            Assert.AreEqual(FaceArt.MoodSurprised, FaceArt.CustomerMood(true, PaymentOutcome.Satisfied, 4, true, false, false), "Tuyet hao -> ngac nhien");
            Assert.AreEqual(FaceArt.MoodSad, FaceArt.CustomerMood(true, PaymentOutcome.Disappointed, 1, true, false, false), "thap 1 bac -> buon");
            Assert.AreEqual(FaceArt.MoodAngry, FaceArt.CustomerMood(true, PaymentOutcome.Angry, 0, true, false, false));
            Assert.AreEqual(FaceArt.MoodEating, FaceArt.CustomerMood(false, PaymentOutcome.Satisfied, 2, true, false, false), "dang an");
            Assert.AreEqual(FaceArt.MoodWait, FaceArt.CustomerMood(false, PaymentOutcome.Satisfied, 0, false, false, false));
            Assert.AreEqual(FaceArt.MoodSweat, FaceArt.CustomerMood(false, PaymentOutcome.Satisfied, 0, false, true, false), "qua khoang an toan: do mo hoi");
            Assert.AreEqual(FaceArt.MoodAngry, FaceArt.CustomerMood(false, PaymentOutcome.Satisfied, 0, false, true, true), "cho qua lau: gian");
            var seen = new System.Collections.Generic.HashSet<Texture2D>();
            foreach (string m in FaceArt.CustomerMoods)
            {
                Assert.IsTrue(seen.Add(FaceArt.Customer(m)), "moi bieu cam mot anh: " + m);
                if (m != FaceArt.MoodEating) Assert.AreNotSame(FaceArt.Customer(m), FaceArt.Customer(m, true), "ban chop mat: " + m);
            }

            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Use("ShopSign");
            yield return WaitForCustomer();
            RoomView.SeatView v = g.Room.SeatViews[0];
            Assert.AreEqual(FaceArt.MoodWait, v.FaceMood, "khach dang cho");
            // AC-3: chop mat 0,12 giay moi 3-5 giay
            Time.timeScale = 1f;
            int blinks = v.Blinks;
            yield return new WaitForSeconds(5.5f);
            Assert.Greater(v.Blinks, blinks, "khach chop mat trong 5,5 giay");
            Time.timeScale = FastForward;
            yield return ServeOne();
            Time.timeScale = 1f;
            if (S.LastPayment != null && S.LastPayment.outcome == PaymentOutcome.Satisfied)
            {
                Assert.AreEqual("tim+xu", g.LastFx, "khach vui: trai tim + dong xu");
                Assert.IsTrue(g.Hud.LastCoin != null, "dong xu bay ve o Vi");
                StringAssert.StartsWith(S.EatingDish(0) != null && S.EatingDish(0).tier >= 4 ? FaceArt.MoodSurprised : FaceArt.MoodHappy, v.FaceMood);
                // hieu ung 0,8 giay: chup ngay (L-022), nhin len dau khach de thay trai tim va dong xu dang bay
                yield return new WaitForSeconds(0.2f);
                yield return Shot("khach_vui_tim_xu.png", v.Mouth.position + new Vector3(0f, 0.45f, 0f), Sprint7);
            }
            yield return new WaitForSeconds(1f);
            if (S.Eaters[0] != null) Assert.AreEqual(FaceArt.MoodEating, v.FaceMood, "het phan ung: mat dang an");
            yield return Shot("khach_dang_an.png", v.ServedSpot.position + new Vector3(0f, 0.6f, 0.3f), Sprint7);
            Time.timeScale = FastForward;
        }

        [UnityTest]
        public IEnumerator F062_NguyenLieuChopMat_3Den5Giay()
        {
            yield return Use("Kiosk");
            Time.timeScale = 1f;
            yield return new WaitForSeconds(1.5f);
            FaceBillboard[] faces = Object.FindObjectsByType<FaceBillboard>(FindObjectsInactive.Exclude).Where(f => f.name == "Mat").ToArray();
            Assert.Greater(faces.Length, 0);
            int before = faces.Sum(f => f.Blinks);
            yield return new WaitForSeconds(5.5f);
            Assert.Greater(faces.Sum(f => f.Blinks), before, "nguyen lieu chop mat");
            Assert.AreEqual(0.12f, FaceBillboard.BlinkSeconds, 1e-4);
            Assert.AreEqual(3f, FaceBillboard.BlinkMin, 1e-4);
            Assert.AreEqual(5f, FaceBillboard.BlinkMax, 1e-4);
            Time.timeScale = FastForward;
        }

        [UnityTest]
        public IEnumerator F062_MauPhuTheoGio_VaMuaNgay2()
        {
            // Art Bible muc 4: 9-15h 0%, 15-21h #FFD9B0 tang toi 20%, sau 21h #C9B3F0 20%
            Assert.AreEqual(0f, ScreenTint.TintFor(10f).a, 1e-4);
            Assert.AreEqual(0.1f, ScreenTint.TintFor(18f).a, 1e-3);
            Assert.AreEqual(0.2f, ScreenTint.TintFor(20.99f).a, 1e-2);
            Color night = ScreenTint.TintFor(21.5f);
            Assert.AreEqual(0.2f, night.a, 1e-4);
            Assert.AreEqual(UiKit.Hex("#C9B3F0").b, night.b, 1e-3, "sau 21h tim nhat");
            Assert.IsNull(g.Tint, "sprint 8 B-01: bo lop phu anh toan man hinh, mau theo gio nhan vao den chinh");

            Assert.IsFalse(g.Room.Rain.Raining, "ngay 1 khong mua");
            Color dry = g.Room.Sky.ColorOf("MatDuong");
            yield return ToDay(S.Balance.stormDay - 1, 0);
            yield return null;
            Assert.IsTrue(g.Room.Rain.Raining, "ngay 2 mua");
            Assert.AreEqual(1f, g.Room.Sky.Rain, 1e-4);
            Color wet = g.Room.Sky.ColorOf("MatDuong");
            Assert.Greater(Mathf.Abs(wet.b - dry.b) + Mathf.Abs(wet.r - dry.r), 0.01f, "pho nhuom mau mua");
            Time.timeScale = 1f;
            yield return Use("ShopSign");
            P.LookAt(g.Room.Station("ShopSign").WorldCenter + new Vector3(0f, 0.4f, 3f));
            yield return Shot("mua_ngay_2.png", null, Sprint7);
            Time.timeScale = FastForward;
            yield return ToDay(S.Balance.stormDay, 0);
            yield return null;
            Assert.IsFalse(g.Room.Rain.Raining, "ngay 3 tanh mua");
        }
    }
}
