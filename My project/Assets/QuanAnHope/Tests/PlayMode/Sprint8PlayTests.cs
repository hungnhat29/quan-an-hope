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
    /// Gia lap nguoi choi sprint 8 (specs/sprint-8/spec.md): B-01 mau, B-02 ve, F-073 khach di vao / ra, F-074 mat luc an,
    /// F-075 dao + so che, F-076 den, F-077 relic mien phi + doi hang, F-078 bang Tab, F-080 kieu khach, F-081 huong dan,
    /// F-070..072 model Blender. Anh: docs/sprint-8/ui.
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint8 = "sprint-8";

        /// <summary>Do bao hoa trung binh (HSV) cua mot khung hinh 1280x720 tu mat Hope.</summary>
        float MeanSaturation()
        {
            Camera cam = P.Cam;
            var rt = new RenderTexture(320, 180, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(320, 180, TextureFormat.RGB24, false);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 320, 180), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            float sum = 0f;
            Color[] px = tex.GetPixels();
            foreach (Color c in px)
            {
                float h, s, v;
                Color.RGBToHSV(c, out h, out s, out v);
                sum += s;
            }
            Object.Destroy(rt);
            Object.Destroy(tex);
            return sum / px.Length;
        }

        IEnumerator OpenAndAdvanceTo(float hour)
        {
            if (S.Phase == GamePhase.Prep) yield return Use("ShopSign");
            g.ClockScale = 6f;
            yield return WaitUntil(() => S.Clock.HourOfDay >= hour || S.Phase != GamePhase.Open, 600f, "toi " + hour + "h");
            g.ClockScale = 0f;
            yield return null;
        }

        // ---------------- B-01: mau khong nhat dan, khong phu tim ----------------

        [UnityTest]
        public IEnumerator B01_MauKhongNhatDan_KhongPhuTim_DenNhanVaoMau()
        {
            Assert.IsNull(g.Tint, "bo lop phu anh toan man hinh");
            Assert.IsNull(GameObject.Find("MauPhuTheoGio"));
            yield return WalkTo(new Vector3(0f, 0f, 0.5f));
            P.LookAt(g.Room.Station("Stove").WorldCenter);
            g.Room.SnapLights();
            yield return null;
            float morning = MeanSaturation();
            yield return Shot("b01_9h.png", null, Sprint8);

            yield return Use("LightSwitch");
            Assert.IsTrue(g.LightsOn);
            yield return WalkTo(new Vector3(0f, 0f, 0.5f));
            P.LookAt(g.Room.Station("Stove").WorldCenter);
            yield return OpenAndAdvanceTo(17f);
            g.Room.SnapLights();
            yield return Shot("b01_17h_den_bat.png", g.Room.Station("Stove").WorldCenter, Sprint8);
            float afternoon = MeanSaturation();
            yield return OpenAndAdvanceTo(20.5f);
            g.Room.SnapLights();
            yield return Shot("b01_20h30_den_bat.png", g.Room.Station("Stove").WorldCenter, Sprint8);
            float evening = MeanSaturation();
            // AC-2: khong giam qua 10% so voi 9:00 (truoc day lop phu kem / tim lam nhat dan)
            Assert.GreaterOrEqual(afternoon, morning * 0.9f, "17:00 khong nhat");
            Assert.GreaterOrEqual(evening, morning * 0.9f, "20:30 den bat khong nhat");
            // mau den chinh la phep nhan, khong co thanh phan tim de (b > r qua nhieu)
            Color tint;
            float level;
            RoomView.LightAt(21.5f, true, false, 0.6f, out level, out tint);
            Assert.Less(tint.b - tint.r, 0.08f, "sau 21:00 den bat khong ngả tim");
        }

        // ---------------- B-02: ve cong don ----------------

        [UnityTest]
        public IEnumerator B02_VeKhongDung_CongDonSangNgaySau_CoDongBao()
        {
            yield return Use("Kiosk");
            yield return Use("Kiosk");
            Assert.AreEqual(3, S.Wallet.Count);
            yield return Use("ShopSign");
            yield return Use("ShopSign");    // dong quan som
            Assert.AreEqual(GamePhase.DayEnd, S.Phase);
            Assert.IsTrue(g.NextDay());
            Assert.AreEqual(3 + S.Balance.ticketsPerDay, S.Wallet.Count, "3 ve con lai + 7 ve ngay moi");
            StringAssert.Contains("cộng 3 vé", g.Toast);
            StringAssert.Contains((3 + S.Balance.ticketsPerDay) + " vé", HudText("HUD_Phai"));
        }

        // ---------------- F-073 khach di vao / di ra; F-080 kieu khach ----------------

        [UnityTest]
        public IEnumerator F073_KhachDiVaoTuCua_RoiNgoi_AnXongDiRa()
        {
            RoomView.WalkCustomers = true;
            RoomView.SeatView v = g.Room.SeatViews[0];   // vong 1: mot ghe (dat o giua quay)
            yield return Use("ShopSign");
            float started = Time.time;
            Customer c = S.CurrentCustomer;
            Assert.IsNotNull(c);
            yield return null;
            Lively l = v.Motion;
            Assert.IsTrue(l.WalkingIn, "khach moi di tu cua vao");
            Assert.That(l.WalkInSeconds, Is.InRange(3f, 6.5f), "sprint 9 B-05: di tu via he vao 3-6,5 giay (khong qua nhanh)");
            Vector3 seat = v.Pivot.position;
            float startDist = Vector3.Distance(Flat(v.Model.transform.TransformPoint(l.Pivot)), Flat(seat));
            Assert.Greater(startDist, 1.2f, "bat dau o ngoai cua");
            Assert.IsFalse(v.Bubble.gameObject.activeSelf, "dang di thi chua hien bong bong");
            yield return Shot("f073_khach_dang_vao.png", seat + Vector3.up * 1.6f, Sprint8);
            while (l.WalkingIn && Time.time - started < 10f) yield return null;
            Assert.IsFalse(l.WalkingIn);
            Assert.That(Time.time - started, Is.InRange(2.8f, 7.5f), "ngoi xuong sau khoang thoi gian di");
            yield return null;
            Assert.Less(Vector3.Distance(Flat(v.Model.transform.TransformPoint(l.Pivot)), Flat(seat)), 0.05f, "ngoi dung ghe");
            Assert.IsTrue(v.Bubble.gameObject.activeSelf, "ngoi xong hien bong bong goi mon");
            Assert.AreEqual(CustomerLooks.LookFor(c.number, S.Day), CustomerLooks.Current(v.Model), "F-080: mac kieu khach");

            // phuc vu dung mon tu tu lanh, an xong thi di ra
            Recipe r = S.Lookup.ById(c.orderId);
            var dish = new Dish { recipeId = r.id, name = r.name, baseName = r.baseName, tier = 3 };
            S.Fridge.TryStore(dish);
            Assert.IsNotNull(S.ServeSeat(0, dish));
            // sprint 9 B-05: khach di ra la ban sao rieng (v.Leaver) de khach sau vao ngay ma nguoi cu van di het via he
            yield return WaitUntil(() => v.Leaver != null && v.Leaver.WalkingOut, S.Balance.day.eatSeconds + 3f, "an xong thi di ra");
            Lively leaver = v.Leaver;
            yield return new WaitForSeconds(0.6f);
            yield return Shot("f073_khach_di_ra.png", seat + Vector3.up * 1.6f, Sprint8);
            yield return WaitUntil(() => leaver == null || leaver.GoneOut, 12f, "ra khoi cua");
        }

        static Vector3 Flat(Vector3 p) { return new Vector3(p.x, 0f, p.z); }

        [UnityTest]
        public IEnumerator F080_SauKieuKhach_KhacHinhBong_TayCungMauAo()
        {
            Assert.AreEqual(7, CustomerLooks.RegularCount, "sprint 9: them kieu Anh tho (Toc_Ngan)");
            Assert.AreEqual(12, CustomerLooks.Count, "23/11/2026: them 5 khach dac biet");
            var looks = Enumerable.Range(1, 40).Select(n => CustomerLooks.LookFor(n, 4)).ToList();
            Assert.AreEqual(7, looks.Where(l => !CustomerLooks.IsSpecial(l)).Distinct().Count(), "40 khach dau ngay du 7 kieu thuong");
            for (int i = 1; i < looks.Count; i++) Assert.AreNotEqual(looks[i - 1], looks[i], "hai khach lien nhau khac kieu");
            // bay 6 kieu ra 3 ghe, chup anh
            yield return ToDay(4, 2000);
            RoomView.WalkCustomers = false;
            yield return Use("ShopSign");
            g.ClockScale = 0f;
            for (int k = 0; k < 6; k += 3)
            {
                for (int s = 0; s < 3; s++)
                {
                    GameObject m = g.Room.SeatViews[s].Model;
                    m.SetActive(true);
                    CustomerLooks.Apply(m, k + s);
                    Color arm = m.transform.Find("ArmWave").GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
                    Color want = CustomerLooks.ShirtOf(k + s);
                    Assert.AreEqual(want.r, arm.r, 0.01f, "tay cung mau ao (tay cam dua lay vat lieu nay)");
                    Assert.AreEqual(want.g, arm.g, 0.01f);
                    Assert.AreEqual(want.b, arm.b, 0.01f);
                }
                yield return WalkTo(new Vector3(0f, 0f, 1.2f));
                yield return Shot("f080_kieu_khach_" + (k / 3 + 1) + ".png", g.Room.SeatViews[1].Pivot.position + Vector3.up * 1.5f, Sprint8);
            }
        }

        // ---------------- F-074 mat luc an ----------------

        [UnityTest]
        public IEnumerator F074_SaiMon_SuotLucAn_MatGian_KhongVeMatVui()
        {
            yield return Use("ShopSign");
            Customer c = S.CurrentCustomer;
            Recipe other = S.BookRecipes().First(x => x.id != c.orderId);
            var wrong = new Dish { recipeId = other.id, name = other.name, baseName = other.baseName, tier = 4 };
            S.Fridge.TryStore(wrong);
            Assert.IsNotNull(S.ServeSeat(0, wrong));
            yield return null;
            RoomView.SeatView v = g.Room.SeatViews[0];
            float t = 0f;
            var faces = new System.Collections.Generic.HashSet<string>();
            while (S.Eaters[0] != null && t < S.Balance.day.eatSeconds + 1f)
            {
                PaymentOutcome o;
                if (!g.IsReacting(0, out o) && v.FaceMood != null) faces.Add(v.FaceMood);
                yield return null;
                t += Time.deltaTime;
            }
            Assert.IsFalse(faces.Contains(FaceArt.MoodEating), "sai mon: khong ve mat vui dang an");
            Assert.IsTrue(faces.Contains(FaceArt.MoodAngry), "sai mon: mat gian suot luc an");
            Assert.AreEqual(RoomView.EatingFace(new PaymentResult { outcome = PaymentOutcome.Disappointed }), FaceArt.MoodSad);
        }

        // ---------------- F-075 dao + so che ----------------

        [UnityTest]
        public IEnumerator F075_GiuChuot_DaoChat_TraNo_ThaChuot_DaoVeCho()
        {
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            PrepFx fx = g.Room.Prep;
            Assert.IsNotNull(fx.Knife, "thot co dao");
            Assert.IsTrue(fx.KnifeAtRest);
            Ingredient side = S.Stock.Items.First(i => i.kind == IngredientKind.Side);
            yield return TakeFromCrate(side);
            yield return Use("Board");
            Interactable board = g.Room.Station("Board");
            yield return Approach(board);
            P.SimulatedHold = true;
            Time.timeScale = 1f;
            yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(fx.Active, "dang giu: dong tac so che chay");
            Assert.AreEqual(PrepFx.MotionFor(side.typeId), fx.Current);
            Assert.Greater(fx.Strikes, 0, "co nhat chat / ban nuoc");
            yield return Shot("f075_so_che_" + side.typeId + ".png", board.WorldCenter, Sprint8);
            P.SimulatedHold = false;
            yield return new WaitForSeconds(0.6f);
            Assert.IsFalse(fx.Active);
            Assert.IsTrue(fx.KnifeAtRest, "tha chuot: dao ve cho");
            Time.timeScale = FastForward;
            Assert.AreEqual(PrepFx.Motion.Crack, PrepFx.MotionFor("trung"), "trung: go thot");
            Assert.AreEqual(PrepFx.Motion.Wash, PrepFx.MotionFor("rau_muong"));
            Assert.AreEqual(PrepFx.Motion.Peel, PrepFx.MotionFor("tom"));
            Assert.AreEqual(PrepFx.Motion.Fillet, PrepFx.MotionFor("ca_hoi"));
            Assert.AreEqual(PrepFx.Motion.HeavyChop, PrepFx.MotionFor("ga"));
        }

        // ---------------- F-076 den ----------------

        [UnityTest]
        public IEnumerator F076_TroiToi_DenTat_PhongToi_BatCongTac_SangAm()
        {
            Assert.IsFalse(g.LightsOn, "dau ngay den tat");
            Assert.IsNotNull(g.Room.LightSwitch);
            yield return OpenAndAdvanceTo(19.5f);
            g.Room.SnapLights();
            Assert.AreEqual(RoomView.DarkLevel, g.Room.RoomLightLevel, 0.05f, "toi ma den tat: den chinh ~40%");
            yield return WalkTo(new Vector3(0f, 0f, 0.5f));
            yield return Shot("f076_toi_den_tat.png", g.Room.Station("Stove").WorldCenter, Sprint8);
            g.ClockScale = 0f;
            yield return Use("LightSwitch");
            Assert.IsTrue(g.LightsOn);
            g.Room.SnapLights();
            Assert.Greater(g.Room.RoomLightLevel, 0.9f);
            Assert.IsTrue(g.Room.LampsLit, "bong den sang");
            yield return WalkTo(new Vector3(0f, 0f, 0.5f));
            yield return Shot("f076_toi_den_bat.png", g.Room.Station("Stove").WorldCenter, Sprint8);
            yield return Shot("f076_cong_tac.png", g.Room.Station("LightSwitch").WorldCenter, Sprint8);
        }

        [UnityTest]
        public IEnumerator F076_MatDien_CongTacKhongTacDung()
        {
            yield return ToDay(S.Balance.blackout.day, 3000);
            yield return Use("LightSwitch");
            Assert.IsFalse(g.LightsOn, "mat dien: cong tac khong co tac dung");
            g.Room.SnapLights();
            Assert.AreEqual(1f - S.Balance.blackout.dim, g.Room.RoomLightLevel, 0.05f);
        }

        // ---------------- F-077 relic ngay 2 mien phi + doi hang ----------------

        [UnityTest]
        public IEnumerator F077_Ngay2_MienPhi_LayMot_ConLaiDong_DoiHang()
        {
            yield return ToDay(2, 500);
            Assert.IsTrue(S.Shop.IsFreeDay);
            int tickets = S.Wallet.Count;
            yield return UseGoc3("Relic_0");
            yield return WalkTo(StandFor(g.Room.Station("Relic_1").WorldCenter));
            yield return Shot("f077_ngay2_mien_phi.png", g.Room.Station("Relic_1").WorldCenter, Sprint8);
            Assert.AreEqual(1, S.Relics.Owned.Count);
            Assert.AreEqual(tickets, S.Wallet.Count, "mien phi");
            // 2.2 (yeu cau 24/11/2026): hai be con lai hien lai gia ve
            Assert.AreEqual(2, S.Shop.Offers.Count(o => o != null), "hai relic con lai van tren ke");
            Assert.Greater(S.Shop.Price(1) + S.Shop.Price(2), 0, "het mien phi: hien gia ve");

            // ngay 3: doi hang ton 50G, ba relic khac han
            yield return ToDay(3, 500);
            var before = S.Shop.Offers.ToList();
            int money = S.Money;
            yield return UseGoc3("RelicReroll");
            Assert.AreEqual(money - S.Balance.relicReroll.baseCost, S.Money);
            Assert.IsTrue(S.Shop.Offers.All(o => o != null && !before.Contains(o)), "3 relic khac han");
            StringAssert.Contains((S.Balance.relicReroll.baseCost + S.Balance.relicReroll.step) + "G", g.Room.RerollSignText, "lan sau dat hon");
            yield return Shot("f077_doi_hang.png", g.Room.Station("RelicReroll").WorldCenter, Sprint8);
        }

        // ---------------- F-078 bang Tab ----------------

        [UnityTest]
        public IEnumerator F078_Tab_XemRelic_BoRelic_CuaHang_DoMuaMoiHien()
        {
            yield return ToDay(4, 3000);
            Assert.IsFalse(g.Room.MachineShown, "chua mua may: khong hien may gacha do an");
            Assert.IsFalse(g.Room.ItemTableShown, "khong con quay vat pham co san");
            Assert.IsFalse(g.Room.BlackSlotShown);
            S.DebugGiveRelic("F4");
            S.DebugGiveRelic("V1");

            Assert.IsTrue(g.OpenTab(TabMenu.Page.Relic));
            Assert.AreEqual(GameMode.Tab, g.Mode);
            Assert.IsTrue(g.Tab.Shown);
            StringAssert.Contains("2/" + S.Relics.Capacity, g.Tab.HeaderText);
            float clock = S.Clock.Elapsed;
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(clock, S.Clock.Elapsed, 1e-4, "mo bang Tab: dong ho dung");
            yield return Shot("f078_tab_relic.png", null, Sprint8);
            g.Tab.DiscardButtons["V1"].onClick.Invoke();     // lan 1: hoi lai
            Assert.IsTrue(S.Relics.Has("V1"));
            g.Tab.DiscardButtons["V1"].onClick.Invoke();     // lan 2: bo
            Assert.IsFalse(S.Relics.Has("V1"));

            g.Tab.Show(TabMenu.Page.Shop);
            StringAssert.Contains(S.Items.SaucePrice + "G", g.Tab.ShopLines[GameController.ItemSauce].text);
            StringAssert.DoesNotContain("%", g.Tab.ShopLines[GameController.ItemMachine].text, "Dieu 4: may gacha giau ti le");
            yield return Shot("f078_tab_cua_hang.png", null, Sprint8);
            g.Tab.BuyButtons[GameController.ItemBlack].onClick.Invoke();
            g.Tab.BuyButtons[GameController.ItemSauce].onClick.Invoke();
            g.Tab.BuyButtons[GameController.ItemMachine].onClick.Invoke();
            Assert.AreEqual(1, S.Items.BlackTickets);
            Assert.AreEqual(1, S.Items.Sauces);
            Assert.IsTrue(S.Items.HasMachine);
            g.CloseTab();
            Assert.AreEqual(GameMode.Playing, g.Mode);
            yield return null;
            Assert.IsTrue(g.Room.MachineShown, "mua xong: may hien o Goc 2");
            Assert.IsTrue(g.Room.SauceShown);
            Assert.IsTrue(g.Room.BlackSlotShown);
            yield return Shot("f078_do_da_mua_goc2.png", g.Room.Station("FoodMachine").WorldCenter, Sprint8);
            yield return Use("BlackSlot");
            Assert.AreEqual(0, S.Items.BlackTickets, "nhet ve den o khe may");
        }

        // ---------------- F-081 huong dan ----------------

        [UnityTest]
        public IEnumerator F081_HuongDan_Ngay2_Relic_Ngay4_CuaHang_TroiToi()
        {
            // yeu cau 23/11/2026: bai relic chieu dau ngay 3 (khong con ngay 2)
            yield return ToDay(3, 500);
            g.Tutorial.Begin(true);
            yield return null;
            yield return null;
            Assert.AreEqual(Tutorial.Relic, g.Tutorial.LessonId, "ngay 3: bai relic");
            StringAssert.Contains("relic", g.Tutorial.Line);
            yield return UseGoc3("Relic_0");
            yield return null;
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Relic));
            g.CloseTab();
            yield return null;
            yield return null;
            Assert.IsTrue(g.Tutorial.HasFinished(Tutorial.Relic) || g.Tutorial.Index >= 2);

            yield return ToDay(4, 3000);
            g.Tutorial.Begin(true);
            yield return null;
            yield return null;
            Assert.AreEqual(Tutorial.Round2, g.Tutorial.LessonId);
            StringAssert.Contains("Tab", g.Tutorial.Line);
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            g.CloseTab();
            yield return null;
            yield return null;
            Assert.AreEqual(1, g.Tutorial.Index, "xem cua hang xong thi sang buoc nhieu khach");
            Assert.Greater(g.Tutorial.Lessons.Count(), 4, "bai: ngay 1, troi toi, ngay 2, vong 2, mat dien, chu no");
        }

        // ---------------- F-070..072 model Blender ----------------

        [UnityTest]
        public IEnumerator F071_F072_ModelBlender_TrongGame_AnhTruocSau()
        {
            Assert.IsTrue(ModelLibrary.Has("Ingredients", "SM_NL_ca_chua"));
            Assert.IsTrue(ModelLibrary.Has("Kitchen", "SM_Chao"));
            // F-070 AC-2: thieu file -> null, khong loi
            Assert.IsNull(ModelLibrary.TryInstantiate("Ingredients", "SM_NL_khong_co", null, 0.01f));
            yield return PullAndWait("Kiosk", 3);
            yield return WalkTo(StandFor(g.Room.Station("Kiosk").WorldCenter));
            ItemView any = S.Stock.Items.Select(i => g.Room.ItemOf(i)).First(v => v != null);
            Assert.IsNotNull(any.GetComponentsInChildren<MeshFilter>().FirstOrDefault(f => f.name.StartsWith("SM_NL_")), "nguyen lieu vong 1 dung model Blender");
            Material m = any.GetComponentsInChildren<MeshRenderer>().First(r => r.name.StartsWith("SM_NL_")).sharedMaterial;
            Assert.AreEqual("BepNhanPham/ToonFlat", m.shader.name, "AC-4: ToonFlat");
            Assert.IsTrue(any.GetComponentsInChildren<MeshRenderer>().Any(r => r.sharedMaterial != null && r.sharedMaterial.shader.name == BepNhanPham.OutlineMesh.ShaderName), "AC-4: vien (sprint 9 B-03: OutlinePx)");
            Vector3 crates = g.Room.Station("Kiosk").WorldCenter + new Vector3(0f, -0.3f, 0f);
            yield return Shot("f071_sau_thung_nguyen_lieu.png", any.transform.position, Sprint8);
            yield return WalkTo(new Vector3(0f, 0f, 1.2f));
            yield return Shot("f072_sau_bep.png", g.Room.Station("Stove").WorldCenter + new Vector3(0f, -0.2f, 0f), Sprint8);
            yield return Shot("f072_sau_thot_dao.png", g.Room.Station("Board").WorldCenter, Sprint8);
        }
    }
}
