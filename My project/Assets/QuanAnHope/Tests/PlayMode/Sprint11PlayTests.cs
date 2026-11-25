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
    /// Gia lap nguoi choi sau sprint 10 (yeu cau 23/11/2026, specs/sprint-10b/spec.md, constitution 2.1): tha len thot nhat lai duoc (C-01),
    /// nuoc sot cam tu canh bep (C-02), bai relic dau ngay 3 (C-04), thanh ti le 6 bac (C-05), bep thu hai (C-06), cau vong / bom xit (C-07),
    /// 5 khach dac biet + giong (C-08), tranh goi y mon cuoi (C-09), huong dan toan man hinh + phim T (C-10), dieu khien trong Cai dat (C-11).
    /// Anh: docs/sprint-10b/ui.
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint10b = "sprint-10b";

        // ---------------- C-01 tha len thot roi nhat lai ----------------

        [UnityTest]
        public IEnumerator C01_ThaLenThot_NhatLaiDuoc()
        {
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            Ingredient ing = S.Stock.Items.First();
            yield return TakeFromCrate(ing);
            Interactable board = g.Room.Station("Board");
            yield return WalkTo(StandFor(board.WorldCenter));
            Vector3 top = board.WorldCenter + new Vector3(0.15f, -0.1f, 0f);
            P.LookAt(top);
            yield return null;
            P.TapSecondary();
            yield return Frames(3);
            Assert.IsTrue(g.Dropped.ContainsKey(ing), "tha xuong thot");
            Assert.Greater(g.Dropped[ing].y, 1.0f, "nam tren mat thot / ban");
            Assert.IsNull(g.OnBoardAt(0), "chi tha, khong dat len de so che");
            ItemView v = g.Room.DroppedView(ing);
            P.LookAt(v.GetComponent<Interactable>().WorldCenter);
            yield return null;
            P.LookAt(v.GetComponent<Interactable>().WorldCenter);
            Assert.IsNotNull(P.Focus);
            StringAssert.StartsWith("Dropped_", P.Focus.Id, "tam ngam uu tien nguyen lieu da tha, khong phai thot");
            P.TapPrimary();
            yield return null;
            Assert.AreSame(ing, g.HeldIngredient, "nhat lai duoc");
        }

        // ---------------- C-02 nuoc sot canh bep ----------------

        [UnityTest]
        public IEnumerator C02_NuocSot_MuaOTab_LoCanhBep_CamLenNhoLenDia()
        {
            yield return StartAt(4, 2000, new string[0]);
            Assert.IsFalse(g.Room.SauceAtStove, "chua mua thi khong co lo");
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            yield return null;
            g.Tab.BuyButtons[GameController.ItemSauce].onClick.Invoke();
            g.CloseTab();
            yield return Frames(2);
            Assert.AreEqual(1550, S.Money);
            Assert.IsTrue(g.Room.SauceAtStove, "lo hien ben phai bep");
            // ben phai bep nhin tu giua phong = +x trong khung Goc 4 (thot ben trai -x, chong dia ben phai +x)
            Transform goc4 = GameObject.Find("NhaHang").transform.Find("Goc4");
            Assert.Greater(goc4.InverseTransformPoint(g.Room.Station("Sauce").WorldCenter).x, goc4.InverseTransformPoint(g.Room.Station("Stove").WorldCenter).x + 0.8f, "ben phai bep");

            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return CookOne();
            Assert.AreEqual(1, S.Counter.Dishes.Count);
            Dish d = S.Counter.Dishes[0];

            yield return Use("Sauce");
            Assert.AreEqual(HandKind.Sauce, g.Hand, "bam lo = cam lo");
            Assert.AreEqual(1550, S.Money, "bam lo khong mua them");
            Assert.AreEqual(1, S.Items.Sauces);
            Assert.IsFalse(g.Room.SauceAtStove, "lo dang tren tay");
            Time.timeScale = 1f;
            yield return Frames(5);
            yield return Shot("c02_cam_nuoc_sot.png", g.Room.Station("Stove").WorldCenter, Sprint10b);
            Time.timeScale = FastForward;
            StringAssert.Contains("Nhỏ nước sốt", g.Room.ShelfDishes[0].GetComponent<Interactable>().CurrentPrompt);
            yield return UseShelfDish(0);
            Assert.AreEqual(0, S.Items.Sauces, "nho mot chai");
            Assert.AreEqual(HandKind.Empty, g.Hand);
            StringAssert.StartsWith("sauce:", g.LastItemEffect);
            Assert.IsTrue(S.Counter.Dishes.Contains(d) || d.tier >= 0);
        }

        // ---------------- C-04 bai relic dau ngay 3 ----------------

        [UnityTest]
        public IEnumerator C04_BaiRelic_ChieuDauNgay3_KhongPhaiNgay2()
        {
            yield return StartAt(2, 0, new string[0]);
            g.Tutorial.Begin(true);
            yield return Frames(4);
            Assert.AreNotEqual(Tutorial.Relic, g.Tutorial.LessonId, "ngay 2 khong chieu");
            yield return StartAt(3, 0, new string[0]);
            g.Tutorial.Begin(true);
            yield return Frames(4);
            Assert.AreEqual(Tutorial.Relic, g.Tutorial.LessonId, "dau ngay 3");
            Assert.AreEqual(GuidePics.BuyRelic, g.Hud.ShownPic);
            g.Tutorial.Stop();
        }

        // ---------------- C-05 thanh ti le 6 bac tren bep ----------------

        [UnityTest]
        public IEnumerator C05_ThanhTiLe6Bac_DinhDangPhanTram()
        {
            Recipe r = S.Lookup.ById("trung__ca_chua");
            Ingredient main = Ingredient.FromMain(S.Recipes.mains.First(x => x.id == r.mainId), 1);
            Ingredient side = Ingredient.FromSide(S.Recipes.sides.First(x => x.id == r.sideId), 1);
            main.prepared = side.prepared = true;
            Assert.IsTrue(S.Stove.SelectTool(r.tool));
            Assert.IsTrue(S.Stove.AddIngredient(main));
            Assert.IsTrue(S.Stove.AddIngredient(side));
            g.Room.Refresh();
            yield return Frames(3);
            StringAssert.Contains("62,61%", g.Room.BarCellText(2), "Binh thuong 62,61%");
            StringAssert.Contains("4,17%", g.Room.BarCellText(0));
            StringAssert.Contains("0,005%", g.Room.BarCellText(5), "Huyen thoai < 1%: du chu so");
            for (int t = 0; t < 6; t++) StringAssert.DoesNotContain(" 0%", " " + g.Room.BarCellText(t).Split('\n').Last());
            Time.timeScale = 1f;
            yield return WalkTo(StandFor(g.Room.Station("Stove").WorldCenter));
            yield return Shot("c05_thanh_ti_le_6_bac.png", g.Room.Station("Stove").WorldCenter + new Vector3(0f, 0.75f, -0.3f), Sprint10b);
        }

        // ---------------- C-06 bep thu hai ----------------

        [UnityTest]
        public IEnumerator C06_MuaBep_BepHaiHienNgay_NauDocLap()
        {
            yield return StartAt(4, 2000, new string[0]);
            Assert.IsFalse(g.Room.Stove2Shown);
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            yield return null;
            StringAssert.Contains("500G", g.Tab.ShopLines[GameController.ItemStove].text);
            g.Tab.BuyButtons[GameController.ItemStove].onClick.Invoke();
            yield return null;
            Assert.AreEqual(1500, S.Money);
            Assert.IsFalse(g.Tab.BuyButtons[GameController.ItemStove].interactable, "du 2 bep");
            g.CloseTab();
            yield return Frames(2);
            Assert.IsTrue(g.Room.Stove2Shown);
            Assert.IsTrue(g.Room.Station("Stove_1").isActiveAndEnabled);

            Recipe r = S.Lookup.ById("trung__ca_chua");
            Ingredient main = Ingredient.FromMain(S.Recipes.mains.First(x => x.id == r.mainId), 2);
            Ingredient side = Ingredient.FromSide(S.Recipes.sides.First(x => x.id == r.sideId), 2);
            main.prepared = side.prepared = true;
            Stove two = S.Stoves[1];
            Assert.IsTrue(two.SelectTool(r.tool));
            foreach (string step in r.steps)
            {
                if (step == main.name) Assert.IsTrue(two.AddIngredient(main));
                else if (step == side.name) Assert.IsTrue(two.AddIngredient(side));
                else Assert.IsTrue(two.AddPantry(step));
            }
            Assert.IsTrue(g.StartCooking(1));
            Assert.AreEqual(StoveState.Cooking, two.State);
            Assert.AreEqual(StoveState.Empty, S.Stove.State, "bep 1 van trong");
            yield return Frames(3);
            Time.timeScale = 1f;
            Vector3 c = g.Room.Station("Stove_1").WorldCenter;
            yield return WalkTo(StandFor(c + new Vector3(-1.2f, 0f, 0f)));
            yield return Shot("c06_bep_thu_hai.png", c + new Vector3(-0.6f, 0.2f, 0f), Sprint10b);
            Time.timeScale = FastForward;
            float t = 0f;
            while (two.State != StoveState.Ready && t < 60f) { t += Time.deltaTime; yield return null; }
            Assert.AreEqual(StoveState.Ready, two.State);
            Assert.IsTrue(g.TakePlate());
            Assert.IsTrue(g.UseStove(1));
            Assert.AreEqual(1, S.Counter.Dishes.Count, "muc tu bep 2 ra quay");
        }

        // ---------------- C-07 cau vong / bom xit ----------------

        [UnityTest]
        public IEnumerator C07_HieuUngNuocSot_CauVong_BomXit()
        {
            Time.timeScale = 1f;
            // dung giua phong nhin ve quay dia (khoang cach nguoi choi that khi nho sot)
            Vector3 at = g.Room.Station("PlateStack").WorldCenter;
            yield return WalkTo(StandFor(at) * 0.55f);
            RainbowBurst rb = SauceFx.Rainbow(at);
            BombFizzle bomb = SauceFx.Bomb(g.Room.Station("Stove").WorldCenter + Vector3.down * 0.3f);
            Assert.IsNotNull(rb);
            Assert.AreEqual(7, rb.GetComponentsInChildren<Transform>().Count(x => x.name.StartsWith("Vong_")), "7 vong 7 mau");
            yield return new WaitForSeconds(0.35f);
            yield return Shot("c07_cau_vong.png", at + new Vector3(0f, 0.5f, 0f), Sprint10b);
            yield return new WaitForSeconds(0.9f);
            yield return Shot("c07_bom_xit.png", g.Room.Station("Stove").WorldCenter, Sprint10b);
            yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(rb == null && bomb == null, "hieu ung tu huy");
        }

        // ---------------- C-08 5 khach dac biet ----------------

        [UnityTest, Timeout(300000)]
        public IEnumerator C08_NamKhachDacBiet_PhuKien_GiongVaoQuan()
        {
            // ~1/4 khach la khach dac biet, hai khach lien nhau khac kieu
            int special = 0, n = 0;
            for (int day = 1; day <= 22; day++)
                for (int k = 1; k <= 40; k++)
                {
                    n++;
                    if (CustomerLooks.IsSpecial(CustomerLooks.LookFor(k, day))) special++;
                    Assert.AreNotEqual(CustomerLooks.LookFor(k, day), CustomerLooks.LookFor(k + 1, day));
                }
            Assert.AreEqual(25.0, special * 100.0 / n, 5.0);
            foreach (string v in new[] { "khach_vip", "khach_streamer", "khach_fan_quy_do" }) Assert.IsTrue(g.Audio.Has(v), "co tieng " + v);

            // giong khi khach dac biet vao quan (chi doan dau)
            int num = Enumerable.Range(1, 400).First(k => CustomerLooks.LookFor(k, S.Day) == CustomerLooks.RegularCount + 1);
            g.OnCustomerArrived(new Customer { number = num });
            Assert.AreEqual("khach_streamer", g.LastVoice);
            Assert.IsTrue(g.Audio.VoicePlaying);

            yield return StartAt(17, 0, new string[0]);
            g.ToggleShop();
            float t = 0f;
            while (S.Seats.Count(c => c != null) < 3 && t < 90f) { t += Time.deltaTime; yield return null; }
            Assert.AreEqual(3, S.Seats.Count(c => c != null));
            Time.timeScale = 1f;
            yield return new WaitForSeconds(7f);
            g.ClockScale = 0f;     // dung dong ho: khach khong bo di / an xong trong luc chup
            Vector3 mid = g.Room.SeatViews[1].Pivot.position;
            yield return WalkTo(StandFor(mid));
            for (int batch = 0; batch < 2; batch++)
            {
                for (int i = 0; i < 3; i++)
                {
                    int look = CustomerLooks.RegularCount + batch * 3 + i;
                    if (look >= CustomerLooks.Count) look = CustomerLooks.RegularCount + i;
                    GameObject m = g.Room.SeatViews[i].Model;
                    CustomerLooks.Apply(m, look, CustomerLooks.ShirtOf(look));
                    CharacterRig rig = CustomerLooks.RigOf(m);
                    Assert.IsNotNull(rig);
                    Assert.AreEqual(CharacterRig.HairStyles[look], rig.Hair);
                    string key = CharacterRig.HairStyles[look].Substring(4);
                    if (rig.HasExtras(key)) Assert.IsTrue(rig.ShownExtras.Any(x => x.StartsWith("Do_" + key)), "do tren nguoi " + key);
                }
                yield return Frames(3);
                yield return Shot("c08_khach_dac_biet_" + (batch + 1) + ".png", mid + new Vector3(0f, 1.1f, 0f), Sprint10b);
            }
            g.ClockScale = 1f;
        }

        // ---------------- C-09 goi y mon cuoi ----------------

        [UnityTest]
        public IEnumerator C09_GoiYMonCuoi_TrungChienCaChuaHuyenThoai()
        {
            yield return StartAt(15, 0, new string[0]);
            g.Tutorial.Begin(true);
            yield return Frames(4);
            Assert.AreEqual(Tutorial.BossHint, g.Tutorial.LessonId);
            Assert.AreEqual(GuidePics.BossBest, g.Hud.ShownPic);
            Assert.IsTrue(g.Hud.TutorialBigShown);
            Assert.IsTrue(g.Tutorial.Lessons.First(l => l.id == Tutorial.Boss).steps.Any(s => s.concept == GuidePics.BossBest), "ngay 21 nhac lai");
            Time.timeScale = 1f;
            yield return Shot("c09_goi_y_mon_cuoi.png", null, Sprint10b);
            g.Tutorial.Stop();
        }

        // ---------------- C-10 huong dan toan man hinh, phim T ----------------

        [UnityTest]
        public IEnumerator C10_HuongDanToanManHinh_PhimT()
        {
            g.Tutorial.Begin(true);
            yield return Frames(3);
            Assert.IsTrue(g.Hud.TutorialBigShown, "buoc moi: tranh to toan man hinh");
            Rect r = g.Hud.TutorialBigCards.rect;
            Assert.Greater(r.width, 1000f, "rong gan het man hinh 1280");
            Time.timeScale = 1f;
            yield return Shot("c10_huong_dan_toan_man_hinh.png", null, Sprint10b);
            P.TapTutorialKey();
            yield return Frames(2);
            Assert.IsFalse(g.Hud.TutorialBigShown, "T: an");
            Assert.AreEqual(GuidePics.Kiosk, g.Hud.ShownPic, "the nho van o goc");
            P.TapTutorialKey();
            yield return Frames(2);
            Assert.IsTrue(g.Hud.TutorialBigShown, "T: hien lai");
            P.TapTutorialKey();
            Time.timeScale = FastForward;
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Frames(2);
            Assert.AreEqual(GuidePics.Take, g.Hud.ShownPic);
            Assert.IsFalse(g.Hud.TutorialBigShown, "24/11: da tat bang T thi buoc sau khong tu bat lai");
            P.TapTutorialKey();
            yield return Frames(2);
            Assert.IsTrue(g.Hud.TutorialBigShown, "chi T moi bat lai");
            g.Tutorial.Stop();
        }

        // ---------------- C-11 bo dong dieu khien o HUD, dua vao Cai dat ----------------

        [UnityTest]
        public IEnumerator C11_DieuKhien_KhongOHud_CoTrongCaiDat()
        {
            Assert.IsNull(g.Hud.Canvas.Find("DieuKhien"), "bo dong chu trang goc duoi");
            g.Pause();
            g.Menu.ShowSettings();
            yield return Frames(3);
            Assert.IsTrue(g.Menu.ControlLines.Any(l => l.StartsWith("Shift")));
            Assert.IsTrue(g.Menu.ControlLines.Any(l => l.StartsWith("T:")), "phim T trong Cai dat");
            Assert.AreEqual(MenuView.Controls.Length, g.Menu.ControlLines.Count);
            Time.timeScale = 1f;
            yield return ShotAllTo("c11_cai_dat_dieu_khien.png", Sprint10b);
            g.Menu.CloseSettings();
            g.Resume();
        }
    }
}
