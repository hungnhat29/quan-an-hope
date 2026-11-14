using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Gia lap nguoi choi 3D tren canh that (constitution v1.2 Dieu 5, Dieu 9):
    /// di bang phim (CharacterController that), quay dau nhin vao do vat, bam / giu chuot trai qua tia nhin o tam ngam.
    /// Khong goi tat vao luat choi. Anh chup luu vao docs/sprint-1/ui/.
    /// </summary>
    public class PlaythroughTests
    {
        const string SceneName = "NhaHang";
        const float FastForward = 25f;
        const int Seed = 20261121;

        // Vung di lai trong phong (giua cac quay), Art Bible muc 9.
        const float MinX = -2.2f, MaxX = 2.0f, MinZ = -2.1f, MaxZ = 1.35f;

        GameController g;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene(SceneName);
            yield return null;
            yield return null;
            g = Object.FindAnyObjectByType<GameController>();
            Assert.IsNotNull(g, "GameController phai tu gan vao canh " + SceneName);
            g.NewGame(Seed);
            P.SimulateInput = true;
            Time.timeScale = FastForward;
            yield return null;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        PlayerController P { get { return g.Player; } }
        GameSession S { get { return g.Session; } }

        // =====================================================================
        // Di, nhin, bam
        // =====================================================================

        static Vector3 StandFor(Vector3 target)
        {
            return new Vector3(Mathf.Clamp(target.x, MinX, MaxX), 0f, Mathf.Clamp(target.z, MinZ, MaxZ));
        }

        IEnumerator WalkTo(Vector3 dest)
        {
            float t = 0f, sinceProgress = 0f;
            float best = float.MaxValue;
            while (true)
            {
                Vector3 d = dest - P.transform.position;
                d.y = 0f;
                float dist = d.magnitude;
                if (dist < 0.06f) break;
                if (dist < best - 0.01f) { best = dist; sinceProgress = 0f; }
                Vector3 local = Quaternion.Euler(0f, -P.Yaw, 0f) * d;
                float step = Mathf.Max(0.0001f, P.MoveSpeed * Mathf.Max(Time.deltaTime, 0.001f));
                P.SimulatedMove = new Vector2(local.x, local.z).normalized * Mathf.Min(1f, dist / step);
                yield return null;
                t += Time.deltaTime;
                sinceProgress += Time.deltaTime;
                Assert.Less(sinceProgress, 4f, "Bi ket khi di toi " + dest + ", dang o " + P.transform.position);
                Assert.Less(t, 60f, "Di qua lau toi " + dest);
            }
            P.SimulatedMove = Vector2.zero;
            yield return null;
        }

        /// <summary>Di toi truoc do vat, nhin vao, kiem tra tam ngam trung dung no.</summary>
        IEnumerator Approach(Interactable target)
        {
            Assert.IsNotNull(target);
            yield return WalkTo(StandFor(target.WorldCenter));
            P.LookAt(target.WorldCenter);
            yield return null;
            P.LookAt(target.WorldCenter);
            Assert.AreSame(target, P.Focus, "Nhin vao " + target.Id + " nhung tam ngam trung " + (P.Focus != null ? P.Focus.Id : "khong gi")
                + " (dung o " + P.transform.position + ")");
        }

        IEnumerator Use(Interactable target)
        {
            yield return Approach(target);
            P.TapPrimary();
            yield return null;
        }

        IEnumerator Use(string stationId)
        {
            Interactable i = g.Room.Station(stationId);
            Assert.IsNotNull(i, "Khong co tram " + stationId);
            yield return Use(i);
        }

        IEnumerator HoldOn(string stationId, System.Func<bool> until, float maxSeconds)
        {
            Interactable i = g.Room.Station(stationId);
            yield return Approach(i);
            P.SimulatedHold = true;
            float t = 0f;
            while (!until() && t < maxSeconds)
            {
                yield return null;
                t += Time.deltaTime;
            }
            P.SimulatedHold = false;
            yield return null;
            Assert.IsTrue(until(), "Giu chuot tren " + stationId + " khong xong");
        }

        IEnumerator WaitUntil(System.Func<bool> done, float gameSeconds, string what)
        {
            float t = 0f;
            while (!done() && t < gameSeconds)
            {
                yield return null;
                t += Time.deltaTime;
            }
            Assert.IsTrue(done(), "Het thoi gian cho: " + what);
        }

        /// <summary>
        /// Lay bang tay mot mon cung loai voi ing tu thung. Neu mon nay bi mon khac che khi nhin tu cho dung
        /// thi lay mon cung loai ke tiep (nguoi that se nghieng dau nhin khac). Ket qua o g.HeldIngredient.
        /// </summary>
        IEnumerator TakeFromCrate(Ingredient ing)
        {
            var candidates = S.Stock.Items.Where(i => i.typeId == ing.typeId && i.prepared == ing.prepared).ToList();
            candidates.Remove(ing);
            candidates.Insert(0, ing);
            foreach (Ingredient c in candidates)
            {
                ItemView view = g.Room.ItemOf(c);
                if (view == null || !view.gameObject.activeInHierarchy) continue;
                Interactable target = view.GetComponent<Interactable>();
                yield return WalkTo(StandFor(target.WorldCenter));
                P.LookAt(target.WorldCenter);
                yield return null;
                P.LookAt(target.WorldCenter);
                if (P.Focus != target) continue;
                P.TapPrimary();
                yield return null;
                Assert.AreEqual(HandKind.Ingredient, g.Hand);
                Assert.AreSame(c, g.HeldIngredient);
                Assert.IsNotNull(g.Room.HandModel, "do dang cam phai hien truoc mat");
                yield break;
            }
            Assert.Fail("Khong nhin trung mon " + ing.name + " nao trong thung");
        }

        IEnumerator PrepareAndAdd(Ingredient wanted)
        {
            yield return TakeFromCrate(wanted);
            Ingredient ing = g.HeldIngredient;
            if (ing.prepared)
            {
                // Da so che tu truoc (vd dat lai vao thung): cho thang vao bep.
                yield return Use("Stove");
                Assert.AreEqual(HandKind.Empty, g.Hand);
                yield break;
            }
            yield return Use("Board");
            Assert.AreSame(ing, g.OnBoard, "nguyen lieu phai nam tren thot");
            Assert.IsNotNull(g.Room.BoardItem);
            yield return HoldOn("Board", () => ing.prepared, ing.prepSeconds + 5f);
            yield return Use("Board");
            Assert.AreSame(ing, g.HeldIngredient, "cam nguyen lieu da so che len");
            yield return Use("Stove");
            Assert.AreEqual(HandKind.Empty, g.Hand);
        }

        /// <summary>Nau mot mon bang tay theo dung cong thuc (hoac dao 2 buoc dau de ra Lac vi).</summary>
        IEnumerator CookOne(bool correctOrder = true)
        {
            Ingredient main = S.Stock.Items.FirstOrDefault(i => i.kind == IngredientKind.Main);
            Ingredient side = S.Stock.Items.FirstOrDefault(i => i.kind == IngredientKind.Side);
            Assert.IsNotNull(main, "Kho het nguyen lieu chinh");
            Assert.IsNotNull(side, "Kho het do phu");
            Recipe r = S.Lookup.Find(main.typeId, side.typeId);

            yield return Use("Tool_" + r.tool);
            Assert.AreEqual(r.tool, S.Stove.Tool);

            var steps = new List<string>(r.steps);
            if (!correctOrder)
            {
                string first = steps[0];
                steps[0] = steps[1];
                steps[1] = first;
            }
            foreach (string step in steps)
            {
                if (step == main.name) yield return PrepareAndAdd(main);
                else if (step == side.name) yield return PrepareAndAdd(side);
                else yield return Use("Jar_" + step);
            }

            Assert.IsNotNull(S.Stove.Bar(), "thanh ti le phai co khi du chinh + phu");
            StringAssert.Contains("%", TextOf("BangBep"), "khoang bac mon hien tren bang cua bep (Dieu 9)");
            yield return Use("Knob");
            Assert.AreEqual(StoveState.Cooking, S.Stove.State);
            yield return WaitUntil(() => S.Stove.State == StoveState.Ready, r.cookSeconds + 5f, "nau " + r.name);
            int before = S.Counter.Dishes.Count;
            yield return Use("PlateStack");
            Assert.AreEqual(HandKind.Plate, g.Hand);
            yield return Use("Stove");
            Assert.AreEqual(before + 1, S.Counter.Dishes.Count, "dia phai len quay cho");
            Assert.AreEqual(HandKind.Empty, g.Hand);
        }

        IEnumerator ServeOne()
        {
            if (S.Counter.Dishes.Count == 0) yield return CookOne();
            yield return null;
            GameObject dish = g.Room.ShelfDishes[0];
            yield return Use(dish.GetComponent<Interactable>());
            Assert.AreEqual(HandKind.Dish, g.Hand);
            int money = S.Money;
            int expected = Payment.Settle(S.Balance, S.Round, S.CurrentCustomer.easiness, g.HeldDish.tier).amount;
            yield return Use("Bell");
            Assert.AreEqual(money + expected, S.Money, "tien doi dung bang tinh tien");
        }

        IEnumerator PlayDay(int pulls, string shotPrefix)
        {
            for (int i = 0; i < pulls; i++) yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            if (shotPrefix != null) yield return Shot(shotPrefix + "_thung.png", g.Room.Station("Kiosk").WorldCenter + new Vector3(0.6f, -0.3f, 1.2f));

            yield return CookOne();
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.Open, S.Phase);
            if (shotPrefix != null)
            {
                yield return WalkTo(new Vector3(0.3f, 0f, 0.9f));
                yield return Shot(shotPrefix + "_khach.png", new Vector3(-0.6f, 2.1f, 3.0f));
            }
            while (S.Phase == GamePhase.Open) yield return ServeOne();
            Assert.AreEqual(GamePhase.DayEnd, S.Phase);
        }

        // =====================================================================
        // Doc UI
        // =====================================================================

        static Transform FindAny(string name)
        {
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
                if (t.name == name) return t;
            return null;
        }

        static string TextOf(string name)
        {
            Transform t = FindAny(name);
            Assert.IsNotNull(t, "Khong thay \"" + name + "\"");
            var sb = new StringBuilder();
            foreach (TextMeshProUGUI text in t.GetComponentsInChildren<TextMeshProUGUI>(true)) sb.Append(text.text).Append(' ');
            return sb.ToString();
        }

        string HudText(string name)
        {
            Transform t = g.Hud.Canvas.Find(name);
            if (t == null) foreach (Transform c in g.Hud.Canvas.GetComponentsInChildren<Transform>(true)) if (c.name == name) t = c;
            Assert.IsNotNull(t, "HUD thieu " + name);
            var sb = new StringBuilder();
            foreach (TextMeshProUGUI text in t.GetComponentsInChildren<TextMeshProUGUI>(true)) sb.Append(text.text).Append(' ');
            return sb.ToString();
        }

        /// <summary>Chup man hinh nhu nguoi choi thay (ca HUD). lookAt = null thi giu huong dang nhin.</summary>
        IEnumerator Shot(string fileName, Vector3? lookAt = null)
        {
            if (lookAt.HasValue) P.LookAt(lookAt.Value);
            yield return null;
            Camera cam = P.Cam;
            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            var c = g.Hud.Canvas.GetComponent<Canvas>();
            cam.targetTexture = rt;
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = cam;
            c.planeDistance = 0.12f;
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", "sprint-1", "ui"));
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, fileName), tex.EncodeToPNG());
            Object.Destroy(rt);
            Object.Destroy(tex);
            yield return null;
        }

        // =====================================================================
        // Kiem tra "da la game 3D chua"
        // =====================================================================

        [UnityTest]
        public IEnumerator La3D_DiLaiTuDo_VaChamThat_KhongNutChuyenGoc()
        {
            // Khong con camera xoay goc va nut chuyen goc (constitution v1.2 Dieu 5).
            Assert.IsNull(FindAny("CameraRig"));
            Assert.IsNull(FindAny("Nav_Trai"));
            Assert.IsNull(FindAny("Nav_Phai"));
            Assert.IsNull(FindAny("UI_NhaHang"), "con canvas UI cu");

            // Mat nguoi choi cao 1,75 m (Art Bible muc 9)
            Assert.AreEqual(1.75f, P.Cam.transform.position.y, 0.02f);

            // Di bang phim: tien 1,2 m
            Vector3 start = P.transform.position;
            yield return WalkTo(new Vector3(0f, 0f, 1.2f));
            Assert.AreEqual(1.2f, P.transform.position.z, 0.1f);

            // Va cham: di thang vao quay Goc 1 thi bi chan truoc quay (mat quay o z = 1,8)
            P.SimulatedMove = new Vector2(0f, 1f);
            P.SetLook(0f, 0f);
            for (int i = 0; i < 30; i++) yield return null;
            P.SimulatedMove = Vector2.zero;
            Assert.Less(P.transform.position.z, 1.6f, "phai bi quay chan lai");
            Assert.Greater(P.transform.position.z, 1.3f);

            // Nhin quanh bang chuot: quay 360 do, tam ngam doi theo
            P.SetLook(90f, 10f);
            Assert.AreEqual(90f, P.Cam.transform.eulerAngles.y, 0.5f);

            // Den chinh 3D: shader nhan huong sang tu den Directional cua canh
            Vector4 light = Shader.GetGlobalVector("_BNP_LightDir");
            Assert.AreEqual(1f, light.w, 0.001f, "ToonLight phai dua huong den vao shader");
            Assert.Greater(light.y, 0.5f, "den chieu tu tren xuong");

            // HUD: chi so lieu va thong tin, khong co nut khi dang choi (Dieu 9)
            Assert.AreEqual(0, g.Hud.Canvas.GetComponentsInChildren<Button>(false).Length, "HUD khong duoc co nut khi dang choi");
            StringAssert.Contains("Ngày 1", HudText("HUD_Trai"));
            StringAssert.Contains("100G", HudText("HUD_Trai"));
            StringAssert.Contains("400G", HudText("HUD_Trai"));
            StringAssert.Contains("5 vé", HudText("HUD_Phai"));

            // Nhin vao may nhet ve: goi y hien o tam ngam, bam thi thung roi xuong ban (nguyen lieu la vat 3D)
            yield return Use("Kiosk");
            StringAssert.Contains("4 vé", HudText("HUD_Phai"));
            yield return new WaitForSeconds(1.5f);
            var crates = g.Room.Crates.ToList();
            Assert.AreEqual(2, crates.Count, "1 thung do phu + 1 thung chinh");
            int itemCount = 0;
            foreach (Ingredient ing in S.Stock.Items)
            {
                ItemView v = g.Room.ItemOf(ing);
                Assert.IsNotNull(v, ing.name + " phai la vat trong thung");
                Assert.IsNull(v.GetComponentInParent<Canvas>(), "nguyen lieu la vat 3D, khong phai UI");
                Assert.IsTrue(v.gameObject.activeInHierarchy);
                string expected = ColorUtility.ToHtmlStringRGB(UiKit.IngredientTier[ing.tier]);
                Assert.AreEqual(expected, ColorUtility.ToHtmlStringRGB(v.DiscColor), "bac doc bang mau de");
                Assert.AreEqual(expected, ColorUtility.ToHtmlStringRGB(v.Disc.GetComponent<MeshRenderer>().sharedMaterial.GetColor("_Color")));
                itemCount++;
            }
            Assert.AreEqual(8, itemCount);

            // Bang ti le tren tuong Goc 2 (Dieu 4, Dieu 9)
            string odds = TextOf("BangTiLe");
            foreach (string pct in new[] { "25%", "57%", "15%", "3%" }) StringAssert.Contains(pct, odds);

            // Sach nau an: trang trai ten mon, trang phai thu tu cho vao co so tron (constitution muc 5.8)
            yield return Approach(g.Room.Station("BookNext"));
            string page1 = TextOf("TrangSach");
            StringAssert.Contains("Trang 1 / 9", page1);
            StringAssert.Contains("Thứ tự cho vào", page1);
            yield return Shot("3d_sach.png", g.Room.Station("BookNext").WorldCenter + new Vector3(0.45f, 0f, 0f));
            P.TapPrimary();
            yield return null;
            string page2 = TextOf("TrangSach");
            StringAssert.Contains("Trang 2 / 9", page2);
            Assert.AreNotEqual(page1, page2);
            StringAssert.DoesNotContain("Tôm hùm", page2);

            // Anh toan canh tu goc phong
            yield return WalkTo(new Vector3(-2.1f, 0f, -2.0f));
            yield return Shot("3d_toan_canh_goc3-4.png", new Vector3(2.5f, 1.2f, 2.5f));
            yield return WalkTo(new Vector3(1.9f, 0f, -2.0f));
            yield return Shot("3d_toan_canh_goc2-3.png", new Vector3(-2.5f, 1.2f, 2.5f));
            yield return WalkTo(new Vector3(1.9f, 0f, 1.3f));
            yield return Shot("3d_toan_canh_goc1-2.png", new Vector3(-3f, 1.1f, -2.5f));
        }

        // =====================================================================
        // Danh sach choi thu (impl-plan muc 7) bang di lai va bam vao do vat
        // =====================================================================

        [UnityTest]
        public IEnumerator ChoiTronVong1_Bang3D_QuaVong1()
        {
            yield return Shot("3d_bat_dau.png");

            // Lay nham thu tu de kiem Lac vi (impl-plan muc 7 buoc 4)
            yield return Use("Kiosk");
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Shot("3d_goc2_thung.png", g.Room.Station("Kiosk").WorldCenter + new Vector3(0.6f, -0.35f, 1.2f));

            yield return TakeFromCrate(S.Stock.Items.First(i => i.kind == IngredientKind.Main));
            Ingredient firstMain = g.HeldIngredient;
            yield return Shot("3d_cam_nguyen_lieu.png");
            yield return Use("Board");
            P.SimulatedHold = true;
            yield return null;
            P.SimulatedHold = false;
            yield return Shot("3d_thot.png");
            yield return HoldOn("Board", () => firstMain.prepared, 10f);
            yield return Use("Board");
            P.TapSecondary();   // dat lai vao thung (da so che)
            yield return null;
            Assert.AreEqual(HandKind.Empty, g.Hand);
            Assert.IsTrue(g.Room.ItemOf(firstMain).gameObject.activeInHierarchy, "dat lai thi nguyen lieu ve thung");

            yield return CookOne(correctOrder: false);
            Assert.AreEqual(0, S.Counter.Dishes[0].tier, "sai thu tu phai ra Lac vi");

            // Mon dung cong thuc
            yield return CookOne();
            yield return WalkTo(new Vector3(-1.2f, 0f, 0.2f));
            yield return Shot("3d_bep.png", new Vector3(-3.3f, 1.75f, 0f));
            yield return Approach(g.Room.Station("PlateStack"));
            yield return Shot("3d_quay_cho.png", g.Room.ShelfDishes[0].transform.position);

            // Mo quan bang bien o Goc 1, phuc vu het 6 khach
            Assert.IsNull(S.CurrentCustomer);
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.Open, S.Phase);
            StringAssert.Contains("Dễ tính " + S.CurrentCustomer.easiness + "%", TextOf("BongThoai"));
            yield return WalkTo(new Vector3(0.3f, 0f, 0.9f));
            yield return Shot("3d_goc1_khach.png", new Vector3(-0.6f, 2.1f, 3.0f));
            while (S.Phase == GamePhase.Open)
            {
                yield return ServeOne();
                if (S.ServedToday == 1 && S.Phase == GamePhase.Open)
                {
                    yield return CookOne();
                    yield return Use(g.Room.ShelfDishes[0].GetComponent<Interactable>());
                    yield return Approach(g.Room.Station("Bell"));
                    yield return Shot("3d_cam_dia_toi_chuong.png");
                    yield return Use("Bell");
                }
            }
            Assert.AreEqual(6, S.ServedToday);
            Assert.IsTrue(g.ShowsOverlay);
            yield return Shot("3d_het_ngay_1.png");
            int ticketsLeft = S.Wallet.Count;
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(2, S.Day);
            Assert.AreEqual(ticketsLeft + 3, S.Wallet.Count);
            Assert.AreEqual(0, g.Room.Crates.Count(), "nguyen lieu thua bi bo cuoi ngay");

            // Ngay 2
            yield return PlayDay(2, null);
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(3, S.Day);

            // Ngay 3 sau mua: bang ti le doi, thung nho
            StringAssert.Contains("50%", TextOf("BangTiLe"));
            StringAssert.Contains("sau mưa", TextOf("BangTiLe"));
            yield return Use("Kiosk");
            Assert.AreEqual(3, S.Stock.Items.Count(i => i.kind == IngredientKind.Side));
            Assert.AreEqual(2, S.Stock.Items.Count(i => i.kind == IngredientKind.Main));
            yield return PlayDay(2, "3d_ngay3");
            StringAssert.Contains("Sáng mai chủ nợ thu 400G", HudText("ManPhu"));

            int before = S.Money;
            Assert.GreaterOrEqual(before, 400, "van gia lap nay phai kiem du 400G");
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GamePhase.Cleared, S.Phase);
            Assert.AreEqual(before - 400, S.Money);
            StringAssert.Contains("Qua vòng 1", HudText("ManPhu"));
            yield return Shot("3d_qua_vong_1.png");

            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, S.Day);
            Assert.AreEqual(GamePhase.Prep, S.Phase);
            Debug.Log("Gia lap 3D: qua vong 1 voi " + before + "G truoc khi tra no.");
        }

        [UnityTest]
        public IEnumerator KhongBanGi_DauNgay4_ManThua()
        {
            for (int day = 1; day <= 3; day++)
            {
                yield return Use("ShopSign");      // mo quan
                Assert.AreEqual(GamePhase.Open, S.Phase);
                yield return Use("Bell");          // tay khong: khong phuc vu duoc
                Assert.AreEqual(0, S.ServedToday);
                yield return Use("ShopSign");      // dong quan som
                Assert.AreEqual(GamePhase.DayEnd, S.Phase);
                g.Hud.OverlayButton.onClick.Invoke();
                yield return null;
            }
            Assert.AreEqual(GamePhase.Lost, S.Phase);
            StringAssert.Contains("Thua rồi", HudText("ManPhu"));
            StringAssert.Contains("100G", HudText("ManPhu"));
        }
    }
}
