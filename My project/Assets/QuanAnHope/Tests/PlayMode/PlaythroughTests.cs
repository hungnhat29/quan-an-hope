using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    /// Gia lap nguoi choi tren canh that: bam dung cac nut tren UI, doi bep nau theo thoi gian game,
    /// di het danh sach choi thu o docs/sprint-1/impl-plan.md muc 7 va chup anh tung goc vao docs/sprint-1/ui/.
    /// </summary>
    public class PlaythroughTests
    {
        const string SceneName = "NhaHang";
        const float FastForward = 20f;
        const float SafeHalfWidth = 0.4f;   // Art Bible muc 9: vat bam duoc nam trong 80% giua chieu ngang
        const float ReferenceWidth = 1280f;
        const int Seed = 20261121;

        GameController g;
        Transform canvas;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene(SceneName);
            yield return null;
            yield return null;
            g = Object.FindAnyObjectByType<GameController>();
            Assert.IsNotNull(g, "GameController phai tu gan vao canh " + SceneName);
            canvas = GameObject.Find("UI_NhaHang").transform;
            g.NewGame(Seed);
            g.GoToCorner(0);
            Time.timeScale = FastForward;
            yield return null;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        // =====================================================================
        // Tro giup
        // =====================================================================

        GameSession S { get { return g.Session; } }

        Transform Find(string name)
        {
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        string TextOf(string name)
        {
            Transform t = Find(name);
            Assert.IsNotNull(t, "Khong thay UI \"" + name + "\"");
            var sb = new StringBuilder();
            foreach (TextMeshProUGUI text in t.GetComponentsInChildren<TextMeshProUGUI>(true)) sb.Append(text.text).Append(' ');
            return sb.ToString();
        }

        bool IsShown(string name)
        {
            Transform t = Find(name);
            return t != null && t.gameObject.activeInHierarchy;
        }

        /// <summary>Bam nut nhu nguoi choi: nut phai dang hien va bam duoc.</summary>
        void Click(string name)
        {
            Transform t = Find(name);
            Assert.IsNotNull(t, "Khong thay nut \"" + name + "\"");
            Assert.IsTrue(t.gameObject.activeInHierarchy, "Nut \"" + name + "\" dang bi an");
            var b = t.GetComponent<Button>();
            Assert.IsNotNull(b, "\"" + name + "\" khong phai nut");
            Assert.IsTrue(b.interactable, "Nut \"" + name + "\" dang bi khoa");
            b.onClick.Invoke();
        }

        /// <summary>Bam o nguyen lieu trong luoi (ChuaSoChe / DaSoChe) co ten va bac nay.</summary>
        void ClickIngredientChip(string gridName, Ingredient ing)
        {
            Transform grid = Find(gridName);
            Assert.IsNotNull(grid);
            string tier = UiKit.IngredientTierName[ing.tier];
            foreach (Button b in grid.GetComponentsInChildren<Button>())
            {
                string label = b.GetComponentInChildren<TextMeshProUGUI>().text;
                if (label.StartsWith(ing.name + " ×") && label.Contains(">" + tier + "<"))
                {
                    b.onClick.Invoke();
                    return;
                }
            }
            Assert.Fail("Khong thay o \"" + ing.name + " / " + tier + "\" trong " + gridName);
        }

        IEnumerator GoTo(int corner)
        {
            // Bam mui ten phai cho den khi toi goc can den (camera xoay 0,35 giay moi lan).
            int guard = 0;
            while (g.Corner != corner && guard++ < 4)
            {
                Click("Nav_Phai");
                yield return null;
            }
            Assert.AreEqual(corner, g.Corner);
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator WaitUntil(System.Func<bool> done, float gameSeconds, string what)
        {
            float t = 0f;
            while (!done() && t < gameSeconds)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Assert.IsTrue(done(), "Het thoi gian cho: " + what);
        }

        /// <summary>Nau mot mon bang nut bam. correctOrder = false thi dao hai buoc dau de ra Lac vi.</summary>
        IEnumerator CookViaUi(bool correctOrder = true)
        {
            Ingredient main = null, side = null;
            foreach (Ingredient i in S.Stock.Items)
            {
                if (main == null && i.kind == IngredientKind.Main) main = i;
                if (side == null && i.kind == IngredientKind.Side) side = i;
            }
            Assert.IsNotNull(main, "Kho het nguyen lieu chinh");
            Assert.IsNotNull(side, "Kho het do phu");
            Recipe recipe = S.Lookup.Find(main.typeId, side.typeId);

            foreach (Ingredient ing in new[] { main, side })
            {
                if (ing.prepared) continue;
                yield return null;
                ClickIngredientChip("ChuaSoChe", ing);
                yield return null;
                Assert.AreSame(ing, S.Board.Current, "nguyen lieu phai len thot");
                g.View.HoldButton.Press();
                yield return WaitUntil(() => ing.prepared, ing.prepSeconds + 3f, "so che " + ing.name);
                g.View.HoldButton.Release();
                yield return null;
            }

            Click("Tool_" + recipe.tool);
            yield return null;

            var steps = new List<string>(recipe.steps);
            if (!correctOrder)
            {
                string first = steps[0];
                steps[0] = steps[1];
                steps[1] = first;
            }
            foreach (string step in steps)
            {
                if (step == main.name) ClickIngredientChip("DaSoChe", main);
                else if (step == side.name) ClickIngredientChip("DaSoChe", side);
                else Click("Pantry_" + step);
                yield return null;
            }

            Assert.IsTrue(IsShown("Bar_0"), "thanh ti le bac mon phai hien khi du nguyen lieu (F-003 AC-5)");
            Click("Btn_Nau");
            yield return null;
            Assert.AreEqual(StoveState.Cooking, S.Stove.State);
            Assert.IsFalse(Find("Btn_Muc").GetComponent<Button>().interactable, "chua chin thi chua muc duoc");
            yield return WaitUntil(() => S.Stove.State == StoveState.Ready, recipe.cookSeconds + 5f, "nau " + recipe.name);
            yield return null;
            Click("Btn_Muc");
            yield return null;
        }

        IEnumerator Capture(string fileName)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            Camera cam = Camera.main;
            Assert.IsNotNull(cam);
            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            cam.targetTexture = rt;

            // Canvas Overlay khong vao duoc anh chup camera: tam chuyen sang Screen Space Camera.
            // (Vi vay trong anh chup UI cung bi phu hat phim / vignette; khi choi that thi khong.)
            var c = canvas.GetComponent<Canvas>();
            RenderMode mode = c.renderMode;
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = cam;
            c.planeDistance = 0.5f;
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", "sprint-1", "ui"));
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, fileName), tex.EncodeToPNG());
            Object.Destroy(rt);
            Object.Destroy(tex);

            c.renderMode = mode;
            yield return null;
        }

        /// <summary>Moi nut dang hien (tru mui ten xoay) phai nam trong 80% giua chieu ngang va trong man hinh.</summary>
        void AssertButtonsInSafeArea(string where)
        {
            var canvasRect = (RectTransform)canvas;
            var corners = new Vector3[4];
            foreach (Button b in canvas.GetComponentsInChildren<Button>(false))
            {
                if (b.name.StartsWith("Nav_")) continue;
                ((RectTransform)b.transform).GetWorldCorners(corners);
                for (int i = 0; i < 4; i++)
                {
                    // Tinh theo khung tham chieu 16:9 (1280 don vi ngang) de khong phu thuoc co cua so khi chay test.
                    Vector3 local = canvasRect.InverseTransformPoint(corners[i]);
                    Assert.LessOrEqual(Mathf.Abs(local.x), SafeHalfWidth * ReferenceWidth + 0.5f, where + ": nut " + b.name + " ra ngoai 80% giua");
                    Assert.LessOrEqual(Mathf.Abs(local.y), canvasRect.rect.height * 0.5f + 0.5f, where + ": nut " + b.name + " ra ngoai man hinh");
                }
            }
        }

        /// <summary>Choi tron mot ngay bang nut bam: nhet ve, nau, mo quan, phuc vu het khach.</summary>
        IEnumerator PlayDayViaUi(int pulls, string shotPrefix)
        {
            yield return GoTo(1);
            for (int i = 0; i < pulls; i++) { Click("Btn_NhetVe"); yield return null; }
            if (shotPrefix != null) yield return Capture(shotPrefix + "_goc2.png");
            AssertButtonsInSafeArea("Goc 2");

            yield return GoTo(3);
            yield return CookViaUi();
            if (shotPrefix != null) yield return Capture(shotPrefix + "_goc4.png");
            AssertButtonsInSafeArea("Goc 4");

            yield return GoTo(0);
            Assert.IsNull(S.CurrentCustomer, "chua mo quan thi chua co khach");
            Assert.IsFalse(IsShown("BongThoai"));
            Click("Btn_MoQuan");
            yield return null;
            Assert.AreEqual(GamePhase.Open, S.Phase);
            Assert.IsTrue(IsShown("BongThoai"));
            StringAssert.Contains("Khách 1/" + S.CustomersToday, TextOf("BongThoai"));
            StringAssert.Contains("Dễ tính " + S.CurrentCustomer.easiness + "%", TextOf("BongThoai"));
            if (shotPrefix != null) yield return Capture(shotPrefix + "_goc1.png");
            AssertButtonsInSafeArea("Goc 1");

            while (S.Phase == GamePhase.Open)
            {
                if (S.Counter.Dishes.Count == 0)
                {
                    yield return GoTo(3);
                    yield return CookViaUi();
                    yield return GoTo(0);
                }
                int before = S.Money;
                // Chuong phuc vu dia dang duoc chon tren quay cho (mac dinh la dia vua muc).
                int chosen = Mathf.Clamp(g.SelectedDish, 0, S.Counter.Dishes.Count - 1);
                int expected = Payment.Settle(S.Balance, S.Round, S.CurrentCustomer.easiness, S.Counter.Dishes[chosen].tier).amount;
                Click("Btn_Chuong");
                yield return null;
                Assert.AreEqual(before + expected, S.Money, "tien phai doi dung theo bang tinh tien");
            }
            Assert.AreEqual(GamePhase.DayEnd, S.Phase);
            yield return null;
        }

        // =====================================================================
        // Danh sach choi thu (impl-plan muc 7)
        // =====================================================================

        [UnityTest]
        public IEnumerator ChoiTronVong1_BangNutBam_QuaVong1()
        {
            // 1. HUD: Ngay 1, 100G, no 400G, 5 ve
            StringAssert.Contains("Ngày 1", TextOf("HUD_Trai"));
            StringAssert.Contains("100G", TextOf("HUD_Trai"));
            StringAssert.Contains("400G", TextOf("HUD_Trai"));
            StringAssert.Contains("5 vé", TextOf("HUD_Phai"));
            StringAssert.Contains("Góc 1", TextOf("HUD_TieuDe"));

            // 2. Goc 2: bang ti le vong 1 va mot luot gacha
            yield return GoTo(1);
            StringAssert.Contains("Góc 2", TextOf("HUD_TieuDe"));
            string[] round1 = { "25%", "57%", "15%", "3%", "0%", "0%" };
            for (int tier = 0; tier < 6; tier++) StringAssert.StartsWith(round1[tier], TextOf("Bac_" + tier));
            Click("Btn_NhetVe");
            yield return null;
            StringAssert.Contains("4 vé", TextOf("HUD_Phai"));
            Assert.AreEqual(5, Find("ThungPhu").Find("Mon").childCount, "thung do phu 5 mon");
            Assert.AreEqual(3, Find("ThungChinh").Find("Mon").childCount, "thung chinh 3 mon");
            StringAssert.Contains(g.LastPull.side.name, TextOf("ThungPhu"));
            StringAssert.Contains(g.LastPull.main.name, TextOf("ThungChinh"));

            // 4. Goc 4: co y cho sai thu tu -> Lac vi
            yield return GoTo(3);
            yield return CookViaUi(correctOrder: false);
            Assert.AreEqual(1, S.Counter.Dishes.Count);
            Assert.AreEqual(0, S.Counter.Dishes[0].tier, "sai thu tu phai ra Lac vi");
            StringAssert.Contains("Lạc vị", TextOf("Plate_0"));

            // 3. Nau dung cong thuc: ten mon dung, bac khong phai Lac vi do sai thu tu
            yield return CookViaUi();
            Assert.AreEqual(2, S.Counter.Dishes.Count);
            Dish good = S.Counter.Dishes[1];
            Assert.IsNotNull(S.Lookup.Find(good.recipeId.Split(new[] { "__" }, System.StringSplitOptions.None)[0],
                good.recipeId.Split(new[] { "__" }, System.StringSplitOptions.None)[1]));
            StringAssert.Contains(good.name, TextOf("Plate_1"));

            // Goc 3: sach nau an liet ke 9 mon vong 1
            yield return GoTo(2);
            string book = TextOf("SachNauAn");
            StringAssert.Contains("Trứng chiên cà chua", book);
            StringAssert.Contains("Đậu phụ sốt rau muống", book);
            StringAssert.Contains("Dầu ăn → Tỏi → Thịt heo → Hành tây", book);
            StringAssert.DoesNotContain("Tôm hùm", book, "mon dot pha chua mo khoa thi khong hien");
            StringAssert.DoesNotContain("Mì Ý", book, "mon vong 2 chua mo khoa thi khong hien");
            yield return Capture("ngay1_goc3.png");

            // 5-7. Ngay 1: mo quan, phuc vu du 6 khach
            yield return PlayDayViaUi(1, "ngay1");
            Assert.AreEqual(6, S.ServedToday);
            Assert.IsTrue(IsShown("ManPhu"));
            StringAssert.Contains("Hết ngày 1", TextOf("ManPhu"));
            StringAssert.Contains("6/6", TextOf("ManPhu"));
            yield return Capture("ngay1_ket_thuc.png");
            int ticketsLeft = S.Wallet.Count;
            Assert.AreEqual(0, S.Stock.Count, "nguyen lieu thua bi bo cuoi ngay");

            Click("Btn_ManPhu");
            yield return null;
            Assert.AreEqual(2, S.Day);
            Assert.AreEqual(ticketsLeft + 3, S.Wallet.Count, "ve thua cong don + 3 ve moi");
            Assert.IsFalse(IsShown("ManPhu"));
            StringAssert.Contains("Ngày 2", TextOf("HUD_Trai"));

            // Ngay 2
            yield return PlayDayViaUi(2, null);
            Click("Btn_ManPhu");
            yield return null;
            Assert.AreEqual(3, S.Day);

            // 8. Ngay 3: bang sau mua, thung nho
            yield return GoTo(1);
            string[] storm = { "50%", "42%", "8%", "0%", "0%", "0%" };
            for (int tier = 0; tier < 6; tier++) StringAssert.StartsWith(storm[tier], TextOf("Bac_" + tier));
            Click("Btn_NhetVe");
            yield return null;
            Assert.AreEqual(3, Find("ThungPhu").Find("Mon").childCount, "ngay 3: thung do phu 3 mon");
            Assert.AreEqual(2, Find("ThungChinh").Find("Mon").childCount, "ngay 3: thung chinh 2 mon");
            yield return PlayDayViaUi(2, "ngay3");
            StringAssert.Contains("Sáng mai chủ nợ thu 400G", TextOf("ManPhu"));

            // 9. Dau ngay 4: tru 400G, qua vong 1
            int before = S.Money;
            Assert.GreaterOrEqual(before, 400, "van gia lap nay phai kiem du 400G");
            Click("Btn_ManPhu");
            yield return null;
            Assert.AreEqual(GamePhase.Cleared, S.Phase);
            Assert.AreEqual(before - 400, S.Money);
            StringAssert.Contains("Qua vòng 1", TextOf("ManPhu"));
            StringAssert.Contains("400G", TextOf("ManPhu"));
            yield return Capture("qua_vong_1.png");

            // Choi lai
            Click("Btn_ManPhu");
            yield return null;
            Assert.AreEqual(1, S.Day);
            Assert.AreEqual(100, S.Money);
            Assert.AreEqual(GamePhase.Prep, S.Phase);
            Assert.IsFalse(IsShown("ManPhu"));

            UnityEngine.Debug.Log("Gia lap UI: qua vong 1 voi " + before + "G truoc khi tra no.");
        }

        [UnityTest]
        public IEnumerator KhongBanGi_DauNgay4_ManThua()
        {
            // 10. Con duoi 400G -> man thua
            for (int day = 1; day <= 3; day++)
            {
                yield return GoTo(0);
                Click("Btn_MoQuan");
                yield return null;
                Assert.IsFalse(Find("Btn_Chuong").GetComponent<Button>().interactable, "khong co dia thi chuong bi khoa");
                Click("Btn_DongQuan");
                yield return null;
                Assert.AreEqual(GamePhase.DayEnd, S.Phase);
                Click("Btn_ManPhu");
                yield return null;
            }

            Assert.AreEqual(GamePhase.Lost, S.Phase);
            StringAssert.Contains("Thua rồi", TextOf("ManPhu"));
            StringAssert.Contains("400G", TextOf("ManPhu"));
            StringAssert.Contains("100G", TextOf("ManPhu"));
            yield return Capture("thua.png");
        }

        [UnityTest]
        public IEnumerator XoayDu4Goc_MoiGocChiHienUiCuaNo_ChuDuDau()
        {
            // 11. Xoay du 4 goc
            string[] titles = { "Góc 1", "Góc 2", "Góc 3", "Góc 4" };
            for (int corner = 0; corner < 4; corner++)
            {
                yield return GoTo(corner);
                StringAssert.Contains(titles[corner], TextOf("HUD_TieuDe"));
                for (int other = 0; other < 4; other++)
                    Assert.AreEqual(other == corner, IsShown("Game_Goc" + (other + 1)), "Goc " + (corner + 1) + ": UI goc " + (other + 1));
                AssertButtonsInSafeArea(titles[corner]);
            }

            // F-006 AC-3: khong con bang ti le may gacha do an trong canh
            Assert.IsNull(GameObject.Find("OddsBoard"), "bang ti le may gacha do an phai an (Dieu 4)");

            // Chu tieng Viet: font Baloo 2 phai co du dau, khong roi ve font du phong
            uint[] missing;
            const string vietnamese = "àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđĐƯƠ";
            Assert.IsTrue(UiKit.Font.HasCharacters(vietnamese, out missing, false, true),
                "Baloo 2 thieu " + (missing != null ? missing.Length : 0) + " ky tu tieng Viet");
            Assert.IsTrue(UiKit.Font.HasCharacters("×→·…%", out missing, true, true), "thieu ky hieu ×→·…");
            StringAssert.Contains("Baloo2", UiKit.Font.name);

            // Art Bible muc 13: chu toi thieu 14 px
            foreach (TextMeshProUGUI t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                Assert.GreaterOrEqual(t.fontSizeMin, UiKit.MinFontSize - 0.01f, "chu qua nho o " + t.transform.parent.name);
        }
    }
}
