using System.Collections;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Gia lap nguoi choi truoc sprint 10 (specs/sprint-10/spec.md, constitution 2.0): huong dan bang hinh (B-08), nut tron cai dat (B-09),
    /// hinh relic trong Tab (B-10), thuc don trong Tab + ban nhan hang dai + relic giua (B-11), gia niem yet (B-12), bo chu "Quay cho" /
    /// "Nau" (B-13), thot mua them (B-15), chay / nhay / tha (B-16), dat dia truoc mat khach (B-17), dong quan moi het ngay (B-18),
    /// khach het kien nhan bo di (B-19). Anh: docs/sprint-10/ui.
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint10 = "sprint-10";

        static int Words(string s) { return s.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Count(w => w != "·" && w != ":" && w != "→"); }

        // ---------------- B-08 huong dan bang hinh ----------------

        [UnityTest]
        public IEnumerator B08_HuongDanBangHinh_MoiBuocMotTranh_ChuNgan_KhongGoiTenGoc()
        {
            // moi buoc: chu <= 6 tu, khong "Goc N"; chu thich tranh <= 4 tu
            foreach (Tutorial.Lesson l in g.Tutorial.Lessons)
                foreach (Tutorial.Step st in l.steps)
                {
                    Assert.LessOrEqual(Words(st.text), 6, l.id + ": " + st.text);
                    StringAssert.DoesNotContain("Góc", st.text);
                    if (st.pic != null) Assert.LessOrEqual(Words(GuidePics.Caption(st.pic)), 4, st.pic);
                    if (st.concept != null) Assert.LessOrEqual(Words(GuidePics.Caption(st.concept)), 4, st.concept);
                }
            foreach (string id in GuidePics.Sheet) Assert.LessOrEqual(Words(GuidePics.Caption(id)), 4, id);
            Tutorial.Step[] d1 = g.Tutorial.Lessons.First(l => l.id == Tutorial.Day1).steps.ToArray();
            Assert.IsTrue(d1.All(s => s.pic != null), "buoc ngay 1 nao cung co tranh");

            g.Tutorial.Begin(true);
            yield return Frames(3);
            Assert.AreEqual(GuidePics.Kiosk, g.Hud.ShownPic, "tranh buoc 1");
            GhostHand hand = (g.Hud.TutorialBigShown ? g.Hud.TutorialBigCards : g.Hud.TutorialCards).GetComponentInChildren<GhostHand>(true);
            Assert.IsNotNull(hand, "co ban tay ma lam mau");
            Assert.Less(hand.Cycle, 3f, "vong lap < 3 giay");
            Time.timeScale = 1f;
            yield return WalkTo(StandFor(g.Room.Station("Kiosk").WorldCenter));
            yield return Shot("b08_huong_dan_buoc_1.png", g.Room.Station("Kiosk").WorldCenter, Sprint10);
            Time.timeScale = FastForward;
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Frames(2);
            Assert.AreEqual(GuidePics.Take, g.Hud.ShownPic);
            Assert.AreEqual(GuidePics.Tiers, g.Hud.ShownConcept, "kem tranh bac nguyen lieu");
            Assert.IsNotNull(g.Hud.TutorialNote);
            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(1f);   // the bat ra xong (stop-motion)
            yield return Shot("b08_huong_dan_bac.png", null, Sprint10);

            // ✓ / ✗ khac nhau ca hinh (doc duoc o thang xam)
            var probe = new GameObject("Thu", typeof(RectTransform));
            Image ok = PicKit.Mark(probe.transform, Vector2.zero, 40f, true), bad = PicKit.Mark(probe.transform, Vector2.zero, 40f, false);
            Assert.AreNotSame(ok.sprite, bad.sprite, "dung tron, sai bat giac");
            Object.Destroy(probe);

            // Tab -> Cach choi: the tranh, 2 trang
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Guide));
            yield return Frames(2);
            Assert.AreEqual(6, g.Tab.GuideCards.Count);
            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(1.4f);   // the "dan" vao lan luot (stop-motion)
            AssertPaperAlignment();
            yield return Shot("b08_tab_cach_choi_1.png", null, Sprint10);
            g.Tab.TurnGuide(1);
            yield return new WaitForSecondsRealtime(1.4f);
            Assert.Greater(g.Tab.GuideCards.Count, 0);
            AssertPaperAlignment();
            yield return Shot("b08_tab_cach_choi_2.png", null, Sprint10);
            g.Tab.TurnGuide(1);
            Assert.AreEqual(1, g.Tab.GuidePage, "trang cuoi: khong vong lai trang 1");
            g.CloseTab();
        }

        /// <summary>
        /// paper-ui-pickit.md muc 6: khong vien thuoc nao xuong dong (lineCount &lt;= 1); moi nhan co tam x lech so voi hinh ben tren
        /// &lt;= 2 px (nhom "Cot" cua PicKit.Column).
        /// </summary>
        void AssertPaperAlignment()
        {
            Transform box = g.Tab.Root.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "CachChoi");
            int pills = 0, cols = 0;
            foreach (TMPro.TextMeshProUGUI t in box.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
            {
                if (t.transform.parent == null || t.transform.parent.name != "Nhan" || string.IsNullOrEmpty(t.text)) continue;
                t.ForceMeshUpdate();
                Assert.LessOrEqual(t.textInfo.lineCount, 1, "vien thuoc \"" + t.text + "\" xuong dong");
                pills++;
            }
            foreach (RectTransform col in box.GetComponentsInChildren<RectTransform>().Where(r => r.name == "Cot"))
            {
                Transform pic = col.Find("Hinh"), tag = col.Find("Nhan");
                if (pic == null || tag == null) continue;
                Assert.LessOrEqual(Mathf.Abs(pic.position.x - tag.position.x), 2f * col.lossyScale.x + 0.001f, "nhan lech tam hinh");
                Assert.Less(tag.position.y, pic.position.y, "nhan nam duoi hinh");
                cols++;
            }
            Assert.Greater(pills, 0);
        }

        // ---------------- B-09 nut tron trong Cai dat ----------------

        [UnityTest]
        public IEnumerator B09_CaiDat_NutKeoTron()
        {
            g.Pause();
            g.Menu.ShowSettings();
            yield return Frames(3);
            foreach (Slider s in new[] { g.Menu.MasterSlider, g.Menu.SensitivitySlider })
            {
                Rect r = s.handleRect.rect;
                Assert.AreEqual(r.width, r.height, 0.5f, "nut keo tron (khong bau duc)");
                Assert.AreEqual(MenuView.HandleSize, r.width, 0.5f);
            }
            Time.timeScale = 1f;
            yield return ShotAllTo("b09_cai_dat.png", Sprint10);
            g.Menu.CloseSettings();
            g.Resume();
        }

        IEnumerator ShotAllTo(string fileName, string sprint)
        {
            Canvas menu = g.Menu.Canvas.GetComponent<Canvas>();
            menu.renderMode = RenderMode.ScreenSpaceCamera;
            menu.worldCamera = P.Cam;
            menu.planeDistance = 0.1f;
            yield return Shot(fileName, null, sprint);
            menu.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // ---------------- B-10 hinh relic trong Tab ----------------

        [UnityTest]
        public IEnumerator B10_TabRelic_HinhRelicThat()
        {
            foreach (string id in new[] { "V1", "C2", "F2" }) Assert.IsTrue(g.DebugGiveRelic(id));
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Relic));
            yield return Frames(2);
            Assert.AreEqual(3, g.Tab.RelicIcons.Count);
            foreach (RawImage icon in g.Tab.RelicIcons.Values)
            {
                Assert.IsNotNull(icon.texture, "co anh relic chup tu model");
                Rect r = icon.rectTransform.rect;
                Assert.AreEqual(r.width, r.height, 0.5f, "hinh vuong, khong keo bau duc");
            }
            Time.timeScale = 1f;
            yield return Shot("b10_tab_relic.png", null, Sprint10);
            g.CloseTab();
        }

        // ---------------- B-11 thuc don trong Tab, ban nhan hang dai, relic giua ----------------

        [UnityTest]
        public IEnumerator B11_ThucDonTrongTab_KhongConSachVaGiaRelic_BeRelicGiua()
        {
            Assert.IsNull(g.Room.Station("BookNext"), "sach trong phong da bo");
            Assert.IsNull(g.Room.Station("Shelf_0"), "gia relic tren tuong da bo");
            Transform goc3 = GameObject.Find("NhaHang").transform.Find("Goc3");
            Transform book = goc3.Find("Cookbook");
            Assert.IsTrue(book == null || !book.gameObject.activeInHierarchy);
            float mid = goc3.InverseTransformPoint(g.Room.Station("Relic_1").WorldCenter).x;
            Assert.AreEqual(0f, mid, 0.05f, "be giua o giua tuong");

            Assert.IsTrue(g.OpenTab(TabMenu.Page.Menu));
            yield return Frames(2);
            Assert.AreEqual(6, g.Tab.MenuButtons.Count, "vong 1: 6 mon");
            Recipe r = S.Lookup.ById(g.Tab.MenuRecipe);
            Assert.AreEqual(r.steps.Length + 2, g.Tab.MenuStepFrames, "khung: chon dung cu + tung buoc + vat num");
            Assert.AreEqual(3, g.Tab.MenuNeedIcons, "hang CAN: chinh, phu, dung cu");
            Assert.AreEqual(r.price + "G", g.Tab.MenuPrice.text.Trim());
            Assert.IsTrue(g.Tab.MenuBreakthroughLocked);
            Time.timeScale = 1f;
            yield return Shot("b11_thuc_don.png", null, Sprint10);
            g.CloseTab();

            // co ruong dot pha -> thuc don co mon son hao hai vi
            Assert.IsTrue(g.DebugGiveRelic("S1"));
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Menu));
            yield return Frames(2);
            Assert.IsFalse(g.Tab.MenuBreakthroughLocked);
            Assert.AreEqual(6 + 28, g.Tab.MenuButtons.Count);
            g.Tab.OpenRecipe("tom_hum__ca_chua");
            yield return Frames(2);
            StringAssert.Contains("Tôm hùm", g.Tab.MenuTitle.text);
            yield return Shot("b11_thuc_don_dot_pha.png", null, Sprint10);
            g.CloseTab();
        }

        [UnityTest]
        public IEnumerator B11_BanNhanHangDai_VongCuoiNhieuThung()
        {
            yield return StartAt(15, 0, new string[0]);
            S.DebugAddTickets(14);
            for (int i = 0; i < 14; i++) yield return Use("Kiosk");
            yield return new WaitForSeconds(2f);
            Transform table = GameObject.Find("NhaHang").transform.Find("Goc2/DropMachine/Table");
            Bounds tb = table.GetComponent<Renderer>().bounds;
            int sides = 0, mains = 0;
            foreach (CrateView c in g.Room.Crates)
            {
                Bounds cb = c.GetComponentsInChildren<Renderer>().Select(x => x.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
                Assert.GreaterOrEqual(cb.center.x, tb.min.x - 0.05f, "thung tren ban");
                Assert.LessOrEqual(cb.center.x, tb.max.x + 0.05f);
                Assert.GreaterOrEqual(cb.center.z, tb.min.z - 0.05f);
                Assert.LessOrEqual(cb.center.z, tb.max.z + 0.05f);
            }
            foreach (Ingredient i in S.Stock.Items.GroupBy(x => x.typeId).Select(x => x.First())) if (i.kind == IngredientKind.Side) sides++; else mains++;
            Assert.LessOrEqual(Mathf.CeilToInt(sides / (float)RoomView.CratesPerRow), 2, "do phu toi da 2 tang");
            Time.timeScale = 1f;
            yield return WalkTo(StandFor(table.position) + new Vector3(0f, 0f, 0f));
            yield return Shot("b11_ban_nhan_hang.png", tb.center + new Vector3(0f, 0.2f, 0f), Sprint10);
        }

        // ---------------- B-13 bo chu "Quay cho", "Nau" ----------------

        [UnityTest]
        public IEnumerator B13_KhongConChuQuayChoVaNau()
        {
            yield return null;
            Assert.IsNull(FindAny("NhanQuayCho"));
            Assert.IsNull(FindAny("NhanNum"));
            Assert.IsNull(g.Room.Station("Bell"), "chuong da bo");
            Time.timeScale = 1f;
            yield return WalkTo(StandFor(g.Room.Station("Stove").WorldCenter));
            yield return Shot("b13_bep.png", g.Room.Station("Stove").WorldCenter + new Vector3(0f, -0.3f, 0f), Sprint10);
        }

        // ---------------- B-15 thot mua them ----------------

        [UnityTest]
        public IEnumerator B15_MuaThot_BanSoCheDaiRa_SoCheHaiThot()
        {
            yield return StartAt(4, 2000, new string[0]);
            Assert.AreEqual(1, g.Room.BoardsShown);
            Assert.IsFalse(g.Room.Station("Board_1").isActiveAndEnabled, "thot 1 chua mua thi an");
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            yield return null;
            StringAssert.Contains("300G", g.Tab.ShopLines[GameController.ItemBoard].text);
            g.Tab.BuyButtons[GameController.ItemBoard].onClick.Invoke();
            yield return null;
            Assert.AreEqual(2, S.Boards.Count);
            Assert.AreEqual(1700, S.Money);
            StringAssert.Contains("500G", g.Tab.ShopLines[GameController.ItemBoard].text, "gia tang");
            g.Tab.BuyButtons[GameController.ItemBoard].onClick.Invoke();
            yield return null;
            Assert.AreEqual(3, S.Boards.Count);
            Assert.IsFalse(g.Tab.BuyButtons[GameController.ItemBoard].interactable, "du 3 thot");
            g.CloseTab();
            yield return Frames(2);
            Assert.AreEqual(3, g.Room.BoardsShown, "ban dai ra, them thot");

            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            Ingredient a = S.Stock.Items.First(i => i.kind == IngredientKind.Side);
            yield return TakeFromCrate(a);
            yield return Use("Board_2");
            Assert.AreSame(a, g.OnBoardAt(2));
            Ingredient b = S.Stock.Items.First(i => i.kind == IngredientKind.Main);
            yield return TakeFromCrate(b);
            yield return Use("Board_1");
            Assert.AreSame(b, g.OnBoardAt(1));
            yield return HoldOn("Board_2", () => a.prepared, a.prepSeconds + 5f);
            Assert.IsFalse(b.prepared, "chi thot dang giu moi so che");
            Time.timeScale = 1f;
            yield return WalkTo(StandFor(g.Room.Station("Board_1").WorldCenter));
            yield return Shot("b15_ba_thot.png", g.Room.Station("Board_1").WorldCenter + new Vector3(0f, -0.2f, 0f), Sprint10);
        }

        // ---------------- B-16 chay, nhay, tha ----------------

        [UnityTest]
        public IEnumerator B16_ShiftChay_SpaceNhay_ChuotPhaiThaTaiCho()
        {
            Time.timeScale = 1f;
            Vector3 start = P.transform.position;
            P.SimulatedMove = new Vector2(0f, -1f);
            yield return new WaitForSeconds(0.5f);
            float walk = (P.transform.position - start).magnitude;
            P.SimulatedMove = Vector2.zero;
            yield return null;
            start = P.transform.position;
            P.SimulatedRun = true;
            P.SimulatedMove = new Vector2(0f, 1f);
            yield return new WaitForSeconds(0.5f);
            float run = (P.transform.position - start).magnitude;
            P.SimulatedMove = Vector2.zero;
            P.SimulatedRun = false;
            Assert.Greater(run, walk * 1.4f, "Shift: chay nhanh hon");

            yield return Frames(3);
            float ground = P.transform.position.y, top = ground;
            P.Jump();
            for (float t = 0f; t < 1f; t += Time.deltaTime) { top = Mathf.Max(top, P.transform.position.y); yield return null; }
            Assert.Greater(top - ground, 0.3f, "Space: nhay len");
            Assert.AreEqual(ground, P.transform.position.y, 0.05f, "roi xuong lai san");

            Time.timeScale = FastForward;
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            Ingredient ing = S.Stock.Items.First();
            yield return TakeFromCrate(ing);
            // ra giua phong (khong co do vat), nhin xuong san truoc mat roi chuot phai
            yield return WalkTo(Vector3.zero);
            Vector3 floor = new Vector3(0f, 0f, 1.1f);
            P.LookAt(floor);
            yield return null;
            P.TapSecondary();
            yield return Frames(3);
            Assert.AreEqual(HandKind.Empty, g.Hand);
            Assert.IsTrue(g.Dropped.ContainsKey(ing));
            ItemView v = g.Room.DroppedView(ing);
            Assert.IsNotNull(v);
            Assert.AreEqual(0f, g.Dropped[ing].y, 0.06f, "roi tren san");
            Assert.Less(Vector2.Distance(new Vector2(floor.x, floor.z), new Vector2(g.Dropped[ing].x, g.Dropped[ing].z)), 0.4f, "roi dung cho dang nhin");
            Time.timeScale = 1f;
            yield return Frames(20);
            yield return Shot("b16_tha_nguyen_lieu.png", g.Dropped[ing] + Vector3.up * 0.1f, Sprint10);
            Time.timeScale = FastForward;
            // nhat lai, tha len mat ban nhan hang
            yield return Approach(v.GetComponent<Interactable>());
            P.TapPrimary();
            yield return null;
            Assert.AreSame(ing, g.HeldIngredient, "nhat lai");
            Transform table = GameObject.Find("NhaHang").transform.Find("Goc2/DropMachine/Table");
            Bounds tb = table.GetComponent<Renderer>().bounds;
            Vector3 onTable = new Vector3(tb.center.x, tb.max.y, tb.min.z + 0.12f);
            yield return WalkTo(StandFor(onTable));
            P.LookAt(onTable);
            yield return null;
            P.TapSecondary();
            yield return Frames(2);
            Assert.Greater(g.Dropped[ing].y, 0.7f, "roi tren mat ban, khong xuyen xuong san");
        }

        // ---------------- B-17 dat dia truoc mat khach ----------------

        [UnityTest]
        public IEnumerator B17_DatDiaTruocMatKhach_PhucVuKhongTheoThuTu()
        {
            yield return StartAt(6, 0, new string[0]);
            S.DebugAddTickets(4);
            for (int i = 0; i < 4; i++) yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Use("ShopSign");
            g.ClockScale = 10f;
            yield return WaitUntil(() => S.Seats.Count(c => c != null) >= 2, 120f, "hai khach ngoi");
            g.ClockScale = 1f;
            Customer first = S.Seats.Where(c => c != null).OrderBy(c => c.number).First();
            Customer later = S.Seats.Where(c => c != null).OrderBy(c => c.number).Last();
            Assert.AreNotSame(first, later);
            Dish d = new Dish { recipeId = later.orderId, name = later.orderName, baseName = S.Lookup.ById(later.orderId).baseName, tier = 3 };
            Assert.IsTrue(S.Counter.TryPlace(d));
            g.Room.Refresh();
            yield return null;
            Time.timeScale = 3f;   // 3 khach co xuong + di bo: khung hinh dai, tua 25 lan de di bo bi giat (L-022)
            yield return UseShelfDish(S.Counter.Dishes.Count - 1);
            Assert.AreSame(d, g.HeldDish);
            string table = "Table_" + S.SeatOf(later);
            yield return Approach(g.Room.Station(table));
            StringAssert.StartsWith("Đặt " + d.Title, g.Room.Station(table).CurrentPrompt);
            Time.timeScale = 1f;
            yield return Shot("b17_dat_dia.png", g.Room.Station(table).WorldCenter, Sprint10);
            Time.timeScale = FastForward;
            P.TapPrimary();
            yield return null;
            Assert.IsTrue(S.Eaters.Contains(later), "khach den sau duoc phuc vu truoc");
            Assert.GreaterOrEqual(S.SeatOf(first), 0, "khach den truoc van ngoi cho");
        }

        // ---------------- B-19 khach het thanh cho bo di ----------------

        [UnityTest]
        public IEnumerator B19_KhachHetKienNhan_GianBoDi_TruTien()
        {
            RoomView.WalkCustomers = true;
            yield return StartAt(4, 500, new string[0]);
            yield return Use("ShopSign");
            Customer first = S.CurrentCustomer;
            int money = S.Money;
            g.ClockScale = 8f;
            yield return WaitUntil(() => S.LeftToday > 0, 300f, "khach dau het thanh cho");
            g.ClockScale = 1f;
            Assert.AreSame(first, S.LastLeave.customer);
            Assert.AreSame(S.LastLeave, g.LastLeaveShown);
            Assert.AreEqual(money - 20 * S.LeftToday, S.Money, "moi khach bo di tru 20G (vong 2)");
            StringAssert.Contains("bỏ đi", g.Toast);
            Time.timeScale = 1f;
            yield return Frames(10);
            yield return WalkTo(new Vector3(0.3f, 0f, 0.9f + BepNhanPham.RoomSize.Grow));
            yield return Shot("b19_khach_bo_di.png", new Vector3(-0.6f, 1.9f, 3.0f + BepNhanPham.RoomSize.Grow), Sprint10);
        }
    }
}
