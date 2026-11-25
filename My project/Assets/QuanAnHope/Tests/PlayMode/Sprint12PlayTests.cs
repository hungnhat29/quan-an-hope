using System.Collections;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Gia lap nguoi choi theo yeu cau 24/11/2026 (specs/sprint-10c/spec.md, constitution 2.2): may gacha kieu lo vi song (D-01), goi y
    /// cot truyen khong de len loi ke (D-02), quay phuc vu rong + khach to (D-03), nhan bac khong xuong dong (D-04), relic ngay 2 (D-05),
    /// tay van nang + o do (D-06). Anh: docs/sprint-10c/ui.
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint10c = "sprint-10c";

        // ---------------- D-01 may gacha do an kieu lo vi song ----------------

        [UnityTest]
        public IEnumerator D01_MayGacha_ChoDiaVao_GatCan_HieuUng()
        {
            yield return StartAt(4, 3000, new string[0]);
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            g.Tab.BuyButtons[GameController.ItemMachine].onClick.Invoke();
            g.CloseTab();
            yield return null;
            Assert.IsTrue(g.Room.MachineShown);
            Assert.IsNull(GameObject.Find("NhaHang").transform.Find("Goc2/UpgradeMachine").gameObject.activeInHierarchy ? "cu" : null, "may cu an");
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return CookOne();
            yield return UseShelfDish(0);
            Dish d = g.HeldDish;
            yield return Use("FoodMachine");
            Assert.AreEqual(HandKind.Empty, g.Hand, "dia roi khoi tay");
            Assert.AreSame(d, g.MachineDish);
            yield return null;
            Assert.IsTrue(g.Room.MachineDishShown);
            Assert.IsFalse(g.Room.ShelfDishes.Any(x => x != null && x.activeInHierarchy && g.DishTaken(d) && x.name.Contains(d.name)), "quay khong ve dia dang trong may");
            Time.timeScale = 1f;
            Vector3 mc = g.Room.Station("FoodMachine").WorldCenter;
            yield return WalkTo(StandFor(mc) * 0.55f);
            yield return Shot("d01_may_co_dia.png", mc + Vector3.up * 0.3f, Sprint10c);
            yield return Approach(g.Room.Station("FoodMachineLever"));
            P.TapPrimary();
            yield return null;
            Assert.IsTrue(g.MachineSpinning);
            yield return WalkTo(StandFor(mc) * 0.55f);
            yield return new WaitForSeconds(GameController.MachineSpinSeconds + 0.3f);
            Assert.IsFalse(g.MachineSpinning);
            Assert.IsNotNull(g.Room.MachineBannerText);
            StringAssert.IsMatch("THÀNH CÔNG|XỊT|Giữ nguyên", g.Room.MachineBannerText);
            yield return Shot("d01_ket_qua.png", mc + Vector3.up * 0.6f, Sprint10c);
            Time.timeScale = FastForward;
        }

        // ---------------- D-02 goi y cot truyen o goc tren ----------------

        [UnityTest]
        public IEnumerator D02_CotTruyen_GoiYKhongDeLenLoiKe()
        {
            g.Menu.PlayStory(StoryScript.Intro(S.Balance), () => { });
            yield return Frames(3);
            Transform hint = g.Menu.Canvas.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "GoiYNen");
            var rt = (RectTransform)hint;
            Assert.AreEqual(new Vector2(1f, 1f), rt.anchorMin, "goi y o goc phai tren (khong o day de len loi ke)");
            Time.timeScale = 1f;
            yield return new WaitForSeconds(1.5f);
            yield return ShotAllTo("d02_cot_truyen.png", Sprint10c);
            g.Menu.SkipStory();
            yield return Frames(2);
        }

        // ---------------- D-03 quay phuc vu rong, khach to ----------------

        [UnityTest]
        public IEnumerator D03_QuayRong_BaGheCachXa_KhachTo()
        {
            Assert.IsNull(GameObject.Find("BellDome"), "bo chuong tren quay");
            yield return StartAt(17, 0, new string[0]);
            g.ToggleShop();
            float t = 0f;
            while (S.Seats.Count(c => c != null) < 3 && t < 90f) { t += Time.deltaTime; yield return null; }
            Time.timeScale = 1f;
            yield return new WaitForSeconds(7f);
            g.ClockScale = 0f;
            var seats = g.Room.SeatViews;
            Assert.GreaterOrEqual(seats[1].Pivot.localPosition.x - seats[0].Pivot.localPosition.x, 1.9f, "ghe cach xa hon (1,45 -> 2,0 m)");
            Assert.GreaterOrEqual(seats[0].Pivot.localScale.x, 0.85f, "khach to hon (0,72 -> 0,88)");
            yield return WalkTo(StandFor(seats[1].Pivot.position) * 0.2f);
            yield return Shot("d03_quay_rong.png", seats[1].Pivot.position + new Vector3(0f, 1.0f, 0f), Sprint10c);
            g.ClockScale = 1f;
        }

        // ---------------- D-04 nhan bac nguyen lieu khong xuong dong ----------------

        [UnityTest]
        public IEnumerator D04_TranhBacNguyenLieu_NhanMotDong()
        {
            var probe = new GameObject("Thu", typeof(RectTransform), typeof(Canvas));
            GuidePics.Card(g, probe.transform, GuidePics.Tiers, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 210f));
            yield return Frames(2);
            foreach (TextMeshProUGUI t in probe.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (!UiKit.IngredientTierName.Contains(t.text)) continue;
                t.ForceMeshUpdate();
                Assert.AreEqual(1, t.textInfo.lineCount, "\"" + t.text + "\" mot dong");
            }
            Object.Destroy(probe);
        }

        // ---------------- D-06 tay van nang ----------------

        /// <summary>Nhat nguyen lieu trong thung vao o trong (tay van nang: khong doi o dang dung).</summary>
        IEnumerator PickIntoSlot(Ingredient ing)
        {
            Interactable target = g.Room.ItemOf(ing).GetComponent<Interactable>();
            yield return WalkTo(StandFor(target.WorldCenter));
            P.LookAt(target.WorldCenter);
            yield return null;
            P.LookAt(target.WorldCenter);
            Assert.AreSame(target, P.Focus, "nhin trung " + ing.name);
            P.TapPrimary();
            yield return null;
            Assert.IsTrue(ReferenceEquals(g.Slot(0).ingredient, ing) || ReferenceEquals(g.Slot(1).ingredient, ing), "da cam " + ing.name);
        }

        [UnityTest]
        public IEnumerator D06_TayVanNang_Cam2Do_DungTheoThuTu_Phim12()
        {
            yield return StartAt(4, 2000, new string[0]);
            Assert.IsFalse(g.Hud.HotbarShown, "chua mua: khong co o do");
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            yield return null;
            StringAssert.Contains("600G", g.Tab.ShopLines[GameController.ItemGlove].text);
            g.Tab.BuyButtons[GameController.ItemGlove].onClick.Invoke();
            yield return null;
            Assert.IsFalse(g.Tab.BuyButtons[GameController.ItemGlove].interactable, "mua 1 lan");
            g.CloseTab();
            yield return Frames(2);
            Assert.IsTrue(g.Hud.HotbarShown, "o do duoi man hinh");

            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            Ingredient a = S.Stock.Items.First(i => i.kind == IngredientKind.Side);
            Ingredient b = S.Stock.Items.First(i => i.kind == IngredientKind.Main);
            yield return PickIntoSlot(a);
            yield return PickIntoSlot(b);
            yield return Frames(2);
            Assert.AreSame(a, g.Slot(0).ingredient, "o 1: do nhat truoc");
            Assert.AreSame(b, g.Slot(1).ingredient, "o 2: do nhat sau");
            Assert.AreSame(a, g.HeldIngredient, "mac dinh dung do nhat truoc");
            Assert.IsNotNull(g.Room.SecondHandModel, "tay trai cam do thu hai");
            Time.timeScale = 1f;
            yield return Frames(3);
            yield return Shot("d06_tay_van_nang.png", g.Room.Station("Kiosk").WorldCenter, Sprint10c);
            Time.timeScale = FastForward;
            Assert.IsTrue(g.Hud.HotbarSlotActive(0));

            // dat len thot: do nhat truoc len thot, tay con do thu hai
            yield return Use("Board");
            Assert.AreSame(a, g.OnBoardAt(0));
            yield return Frames(2);
            Assert.AreSame(b, g.HeldIngredient, "con lai do thu hai");

            // phim 2 / 1 chon o; tha (chuot phai) theo o dang chon
            yield return PickIntoSlot(S.Stock.Items.First(i => i.kind == IngredientKind.Side && !ReferenceEquals(i, a) && !g.Dropped.ContainsKey(i)));
            Ingredient c = g.Slot(0).kind == HandKind.Ingredient && !ReferenceEquals(g.Slot(0).ingredient, b) ? g.Slot(0).ingredient : g.Slot(1).ingredient;
            int slotOfC = ReferenceEquals(g.Slot(0).ingredient, c) ? 0 : 1;
            P.TapSlotKey(slotOfC);
            yield return null;
            Assert.AreSame(c, g.HeldIngredient, "phim chon o");
            LookAtFloorAway();
            P.TapSecondary();
            yield return Frames(2);
            Assert.IsTrue(g.Dropped.ContainsKey(c), "tha do o da chon");
            Assert.AreSame(b, g.HeldIngredient, "con lai do kia");
        }
    
        // ---------------- F (24/11/2026 dot 3): chu de len o Cua hang, goc vuong trong khung bo tron, bo hop vang, vien muc ----------------

        [UnityTest]
        public IEnumerator F01_CuaHang_SauDong_KhongDeLenGhiChu()
        {
            yield return StartAt(4, 3000, new string[0]);
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            yield return Frames(2);
            RectTransform last = g.Tab.ShopLines[GameController.ItemGlove].transform.parent.parent as RectTransform;
            RectTransform note = g.Tab.Root.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "GhiChu");
            var a = new Vector3[4]; var b = new Vector3[4];
            last.GetWorldCorners(a);
            note.GetWorldCorners(b);
            Assert.Greater(a[0].y, b[1].y, "dong cuoi (Tay van nang) nam tren ghi chu");
            Time.timeScale = 1f;
            yield return Shot("f01_cua_hang.png", null, Sprint10c);
            g.CloseTab();
        }

        [UnityTest]
        public IEnumerator F02_KhungBoTron_RuotKhongLoGocVuong()
        {
            g.Tutorial.Begin(true);
            yield return Frames(3);
            foreach (RectTransform w in g.Hud.TutorialBigCards.GetComponentsInChildren<RectTransform>(true).Where(r => r.name == "CuaSo"))
                Assert.IsNotNull(w.GetComponent<UnityEngine.UI.Mask>(), "cua so tranh co mat na bo tron");
            ProgressBar bar = Object.FindObjectsByType<ProgressBar>(FindObjectsSortMode.None).First();
            Assert.IsNotNull(bar.GetComponent<UnityEngine.UI.Mask>(), "thanh tien do: vo bo tron cat ruot");
            Assert.AreEqual(UiKit.Rounded, bar.transform.Find("Fill").GetComponent<UnityEngine.UI.Image>().sprite, "ruot bo tron");
            g.Tutorial.Stop();
        }

        [UnityTest]
        public IEnumerator F03_BoHopHuongDanVang()
        {
            g.Tutorial.Begin(true);
            yield return Frames(3);
            Assert.IsNotNull(g.Hud.TutorialText, "huong dan van chay");
            Transform box = g.Hud.Canvas.Find("HuongDan");
            Assert.IsTrue(box == null || !box.gameObject.activeSelf, "khong con hop chu vang");
            P.TapTutorialKey();   // an tranh to: van khong hien hop vang
            yield return Frames(2);
            Assert.IsTrue(box == null || !box.gameObject.activeSelf);
            P.TapTutorialKey();
            g.Tutorial.Stop();
        }

        [UnityTest]
        public IEnumerator F04_VienMuc_ChiTietNhoVienMong_ChuInKhongVien()
        {
            Assert.IsTrue(ModelLibrary.Has("Kitchen", "SM_LoNuocSot"));
            Assert.IsTrue(ModelLibrary.Has("Machines", "SM_MayGacha_Vo") && ModelLibrary.Has("Machines", "SM_MayGacha_Long") && ModelLibrary.Has("Machines", "SM_Bep2"));
            var host = new GameObject("ThuKhach");
            CharacterRig rig = CharacterRig.Create(host.transform, CharacterRig.Customer);
            Assert.IsNotNull(rig);
            int none = 0, thin = 0;
            foreach (Renderer r in rig.GetComponentsInChildren<Renderer>(true))
            {
                if (r.name == BepNhanPham.OutlineMesh.ChildName) continue;
                Transform hull = r.transform.Find(BepNhanPham.OutlineMesh.ChildName);
                if (r.name.Contains(ModelLibrary.NoOutlineTag)) { Assert.IsNull(hull, r.name + ": chu in / not nho khong vien"); none++; }
                else if (r.name.Contains(ModelLibrary.ThinOutlineTag))
                {
                    Assert.IsNotNull(hull, r.name);
                    Assert.Less(hull.GetComponent<Renderer>().sharedMaterial.GetFloat("_WidthPx"), BepNhanPham.OutlineMesh.MaxPx, r.name + ": vien mong");
                    thin++;
                }
            }
            Assert.GreaterOrEqual(none, 6, "chu VO DICH, GG, 66, 7, #1, VIP, not ruoi, son ma");
            Assert.GreaterOrEqual(thin, 5, "kinh, tai nghe, bang do, logo, the, tien");
            Object.Destroy(host);
            yield return StartAt(4, 3000, new string[0]);
            S.DebugAddMoney(0);
            Assert.IsTrue(g.OpenTab(TabMenu.Page.Shop));
            g.Tab.BuyButtons[GameController.ItemSauce].onClick.Invoke();
            g.Tab.BuyButtons[GameController.ItemMachine].onClick.Invoke();
            g.Tab.BuyButtons[GameController.ItemStove].onClick.Invoke();
            g.CloseTab();
            yield return Frames(3);
            Assert.IsNotNull(GameObject.Find("SM_MayGacha_Vo"), "may gacha dung model Blender");
            Assert.IsNotNull(GameObject.Find("SM_Bep2"), "bep 2 dung model Blender");
            Assert.IsNotNull(GameObject.Find("SM_LoNuocSot"), "lo sot dung model Blender");
            Time.timeScale = 1f;
            Vector3 mc = g.Room.Station("FoodMachine").WorldCenter;
            yield return WalkTo(StandFor(mc) * 0.55f);
            yield return Shot("f04_may_blender.png", mc + Vector3.up * 0.3f, Sprint10c);
            Vector3 st = g.Room.Station("Stove").WorldCenter;
            yield return WalkTo(StandFor(st));
            yield return Shot("f04_bep_lo_sot.png", st + new Vector3(0f, 0f, 0f), Sprint10c);
        }
    }
}
