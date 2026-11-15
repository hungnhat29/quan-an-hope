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
    /// Khong goi tat vao luat choi. Anh chup luu vao docs/sprint-2/ui/ (sprint 2: ngay theo gio, constitution 1.4).
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
            if (g != null) g.ClockScale = 1f;
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

        /// <summary>Nhu Approach, nhung do vat bi vat khac che thi dich cho dung (nguoi that nghieng dau nhin khac).</summary>
        IEnumerator ApproachAround(Interactable target)
        {
            Vector3 c = target.WorldCenter;
            yield return WalkTo(StandFor(c));
            // Ngam vao phan cao / phan xa cua hop cham (0,42 x 0,22 x 0,42 m) de nhin qua dia dung truoc.
            Vector3 eye = P.Cam.transform.position;
            Vector3 away = c - eye;
            away.y = 0f;
            away = away.normalized * 0.15f;
            foreach (Vector3 aim in new[] { c, c + new Vector3(0f, 0.1f, 0f), c + away + new Vector3(0f, 0.1f, 0f), c + away + new Vector3(0f, 0.16f, 0f) })
            {
                P.LookAt(aim);
                yield return null;
                P.LookAt(aim);
                if (P.Focus == target) yield break;
            }
            yield return Approach(target);
        }

        IEnumerator UseShelfDish(int index)
        {
            Interactable target = g.Room.ShelfDishes[index].GetComponent<Interactable>();
            yield return ApproachAround(target);
            P.TapPrimary();
            yield return null;
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
                yield return null;
                AssertFaceOutside(g.Room.HandModel);
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
            AssertFaceOutside(g.Room.BoardItem);
            yield return HoldOn("Board", () => ing.prepared, ing.prepSeconds + 5f);
            yield return Use("Board");
            Assert.AreSame(ing, g.HeldIngredient, "cam nguyen lieu da so che len");
            yield return Use("Stove");
            Assert.AreEqual(HandKind.Empty, g.Hand);
        }

        /// <summary>
        /// Nau mot mon bang tay theo dung cong thuc (hoac dao 2 buoc dau de ra Lac vi).
        /// recipeId = mon khach goi; null thi lay cap chinh + phu dau tien trong kho.
        /// </summary>
        IEnumerator CookOne(bool correctOrder = true, string recipeId = null)
        {
            Recipe want = recipeId != null ? S.Lookup.ById(recipeId) : null;
            Ingredient main = S.Stock.Items.FirstOrDefault(i => i.kind == IngredientKind.Main && (want == null || i.typeId == want.mainId));
            Ingredient side = S.Stock.Items.FirstOrDefault(i => i.kind == IngredientKind.Side && (want == null || i.typeId == want.sideId));
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
            int shot = 0;
            foreach (string step in steps)
            {
                int inPan = g.Room.PanItems.Count;
                if (step == main.name) yield return PrepareAndAdd(main);
                else if (step == side.name) yield return PrepareAndAdd(side);
                else
                {
                    yield return Use("Jar_" + step);
                    Assert.IsNotNull(g.Room.Jar(step).GetComponent<Wobble>(), "lo gia vi nghieng khi rot");
                }
                // Cho vao co hinh anh: mot vat moi bay vao dung cu va nam lai trong do (tin nhan 14/11).
                Assert.AreEqual(inPan + 1, g.Room.PanItems.Count, "cho " + step + " phai co vat bay vao bep");
                yield return CheckPanSettled();
                if (cookShots != null) yield return PanShot(cookShots + "_" + (++shot) + "_" + Slug(step) + ".png");
            }

            Assert.IsNotNull(S.Stove.Bar(), "thanh ti le phai co khi du chinh + phu");
            CheckTierStrip();
            yield return Use("Knob");
            Assert.AreEqual(StoveState.Cooking, S.Stove.State);
            Assert.IsTrue(g.Room.PanItems.All(p => p.Sizzle), "dang nau thi do trong bep rung");
            Assert.IsTrue(g.Room.Pan.FlameOn, "F-013 AC-3: vat num thi co lua duoi dung cu");
            if (cookShots != null)
            {
                yield return WaitUntil(() => g.Room.Pan.Doneness > 0.5f, r.cookSeconds, "nau toi nua chung");
                yield return PanShot(cookShots + "_" + (++shot) + "_dang_nau.png");
                Assert.IsNull(g.Room.Pan.Finished);
            }
            if (shotCooking != null)
            {
                yield return WalkTo(StandFor(g.Room.PanRoot.position));
                yield return Shot(shotCooking, g.Room.PanRoot.position + new Vector3(0f, 0.3f, 0f));
                shotCooking = null;
            }
            yield return WaitUntil(() => S.Stove.State == StoveState.Ready, r.cookSeconds + 5f, "nau " + r.name);
            yield return null;
            // F-013 AC-4, AC-6: chin thi trong dung cu la mon hoan chinh; sai thu tu thi chay xem
            Assert.IsNotNull(g.Room.Pan.Finished, "chin thi trong dung cu la mon hoan chinh");
            Assert.IsFalse(g.Room.Pan.FlameOn, "chin thi tat lua");
            Assert.AreEqual(!correctOrder, g.Room.Pan.ShowsBurnt, "sai thu tu thi mon chay xem");
            if (cookShots != null) yield return PanShot(cookShots + "_" + (++shot) + "_chin.png");
            int before = S.Counter.Dishes.Count;
            yield return Use("PlateStack");
            Assert.AreEqual(HandKind.Plate, g.Hand);
            yield return Use("Stove");
            Assert.AreEqual(before + 1, S.Counter.Dishes.Count, "dia phai len quay cho");
            Assert.AreEqual(HandKind.Empty, g.Hand);
            Assert.AreEqual(0, g.Room.PanItems.Count, "muc ra dia thi bep trong");
            yield return null;
            Assert.IsNull(g.Room.Pan.Finished, "muc ra dia thi trong dung cu khong con mon");
            // F-013 AC-5: dia co hinh mon giong trong dung cu (cung bo dung DishFood) + de mon
            GameObject plate = g.Room.ShelfDishes[S.Counter.Dishes.Count - 1];
            Assert.IsNotNull(plate.transform.Find("MonAn"), "dia co mon hoan chinh");
            Assert.IsNotNull(plate.transform.Find("DeMon"), "dia co de mon");
            CheckDishFaces();
            if (cookShots != null)
            {
                yield return Approach(g.Room.Station("PlateStack"));
                yield return Shot(cookShots + "_" + (++shot) + "_dia.png", plate.transform.position);
                cookShots = null;
            }
        }

        string shotCooking;
        string cookShots;

        static string Slug(string vi)
        {
            var sb = new StringBuilder();
            foreach (char ch in vi.ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD))
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
                sb.Append(ch == ' ' ? '_' : ch);
            }
            return sb.ToString();
        }

        /// <summary>Chup can dung cu tren bep (khong di lai, chi nhin).</summary>
        IEnumerator PanShot(string file)
        {
            yield return WalkTo(StandFor(g.Room.PanRoot.position));
            yield return Shot(file, g.Room.PanRoot.position);
        }

        /// <summary>Moi vat da cho vao deu da roi xuong, nam trong long dung cu, co mat theo bac (nguyen lieu).</summary>
        IEnumerator CheckPanSettled()
        {
            yield return WaitUntil(() => g.Room.PanItems.All(p => p.Landed), 3f, "do bay vao bep");
            Transform pan = g.Room.PanRoot;
            float radius = S.Stove.Tool == "Nồi" ? 0.55f : 0.29f;
            foreach (PanItem p in g.Room.PanItems)
            {
                foreach (Renderer r in p.GetComponentsInChildren<Renderer>())
                {
                    Vector3 d = r.bounds.center - pan.position;
                    Assert.Less(new Vector2(d.x, d.z).magnitude, radius, r.name + " phai nam trong " + S.Stove.Tool);
                    Assert.Less(Mathf.Abs(d.y), 0.06f, r.name + " nam tren mat dung cu");
                }
            }
            // F-013 AC-1: dau / bo / nuoc dung loang thanh lop phu day dung cu (khong con la giot tron canh do an)
            yield return new WaitForSeconds(0.7f);
            foreach (PanItem p in g.Room.PanItems)
            {
                if (!p.name.StartsWith("GiaVi_")) continue;
                string item = p.name.Substring("GiaVi_".Length);
                if (item != "Dầu ăn" && item != "Bơ" && item != "Nước dùng") continue;
                Renderer layer = p.GetComponentsInChildren<Renderer>().First(r => r.name == "Lop");
                float width = layer.bounds.size.x;
                Assert.Greater(width, radius * 0.8f * (S.Stove.Tool == "Nồi" ? 0.45f : 1f), item + " phai loang thanh lop day dung cu");
                Assert.Less(layer.bounds.size.y, 0.03f, item + " la lop mong");
            }
            // F-013 AC-2: nguyen lieu thanh 4-6 mieng rai deu, khong chong len nhau
            foreach (PanItem p in g.Room.PanItems)
            {
                if (!p.name.StartsWith("NguyenLieu_")) continue;
                var cut = p.transform.Cast<Transform>().Where(t => t.name.StartsWith("Mieng_")).ToList();
                if (p.name == "NguyenLieu_trung") { Assert.AreEqual(0, cut.Count, "trung danh la lop long"); continue; }
                Assert.That(cut.Count, Is.InRange(4, 6), p.name + " roi vao thanh 4-6 mieng");
                for (int i = 0; i < cut.Count; i++)
                    for (int j = i + 1; j < cut.Count; j++)
                    {
                        Vector3 a = cut[i].position, b = cut[j].position;
                        Assert.Greater(new Vector2(a.x - b.x, a.z - b.z).magnitude, 0.035f, p.name + ": mieng " + i + " va " + j + " chong nhau");
                    }
            }
        }

        /// <summary>F-012 AC-1, AC-2: nguyen lieu vong 1 ghep tu >= 3 khoi; da so che thi doi hinh (khong chi ep det).</summary>
        static readonly string[] Round1Ingredients = { "ca_chua", "hanh_tay", "rau_muong", "trung", "heo", "dau_phu" };

        static string PartNames(List<IngredientModels.Part> parts)
        {
            return string.Join(",", parts.Select(x => x.type + ":" + x.name.Split('_')[0]).ToArray());
        }

        /// <summary>Tam mat nam ngoai khoi bao cua than mon, nen khong bi chinh than mon che (khoi vuong nhin xeo).</summary>
        static void AssertFaceOutside(GameObject item)
        {
            ItemView v = item.GetComponent<ItemView>();
            Assert.IsNotNull(v, item.name + " khong phai nguyen lieu");
            Renderer body = item.GetComponentsInChildren<Renderer>().First(r => r.name == "Than");
            // Ban kinh mat cau bao than (khong dung hop AABB: khoi bi xoay thi AABB phinh to hon khoi that).
            float radius = Vector3.Scale(body.localBounds.extents, body.transform.lossyScale).magnitude;
            float d = Vector3.Distance(v.Face.transform.position, body.bounds.center);
            Assert.GreaterOrEqual(d, radius * 0.98f, "mat cua " + item.name + " lot vao trong than");
        }

        /// <summary>Thanh ti le bac mon: mot dai dai, thap, du 6 bac ke ca 0% (tin nhan 14/11).</summary>
        void CheckTierStrip()
        {
            Transform strip = FindAny("ThanhTiLe");
            Assert.IsNotNull(strip);
            Assert.IsTrue(strip.gameObject.activeInHierarchy);
            var rt = (RectTransform)strip;
            Assert.LessOrEqual(rt.sizeDelta.y, 110f, "dai ti le thap (mm tren bang bep)");
            Assert.Greater(rt.sizeDelta.x / rt.sizeDelta.y, 10f, "dai ti le dai va mong");
            TierChance[] bar = S.Stove.Bar();
            int total = 0;
            for (int tier = 0; tier < 6; tier++)
            {
                Transform cell = strip.Find("Bar_" + tier);
                Assert.IsNotNull(cell, "thieu bac " + tier);
                string text = cell.GetComponentInChildren<TextMeshProUGUI>().text;
                StringAssert.Contains(UiKit.DishTierName[tier], text);
                int expected = bar.Where(c => c.tier == tier).Select(c => Mathf.RoundToInt((float)(c.chance * 100.0))).DefaultIfEmpty(0).First();
                StringAssert.EndsWith("\n" + expected + "%", text, "bac " + tier);
                total += expected;
            }
            Assert.AreEqual(100f, total, 1f);
            Assert.IsTrue(bar.Length < 6, "bo du lieu nay phai co it nhat 1 bac 0% de kiem");
        }

        /// <summary>Moi dia tren quay cho: mat theo bac mon, hoi bay len theo bac (Art Bible muc 7).</summary>
        void CheckDishFaces()
        {
            for (int i = 0; i < g.Room.ShelfDishes.Count; i++)
            {
                Dish dish = S.Counter.Dishes[i];
                GameObject model = g.Room.ShelfDishes[i];
                FaceBillboard face = model.GetComponentsInChildren<FaceBillboard>().First(f => f.name == "MatMon");
                Assert.AreEqual("Mat_Mon_" + dish.tier, face.Texture.name, dish.name);
                Assert.Greater(Vector3.Dot(face.transform.forward, P.Cam.transform.forward), 0.99f, "mat mon quay ve nguoi choi");
                DishFx fx = model.GetComponent<DishFx>();
                Assert.IsNotNull(fx);
                Assert.AreEqual(DishFx.KindFor(dish.tier), fx.Kind);
                Assert.AreEqual(dish.tier == 1 ? 0 : 1, System.Math.Sign(fx.Count), "bac " + dish.tier);
            }
        }

        int OrderedOnShelf()
        {
            for (int i = 0; i < S.Counter.Dishes.Count; i++)
                if (S.Counter.Dishes[i].recipeId == S.CurrentCustomer.orderId) return i;
            return -1;
        }

        // F-011: thong ke cua van gia lap
        int maxServedInDay;
        bool sawClosedSign, sawLateClock, shot21h = true;

        /// <summary>
        /// Ghe trong thi doi khach ke tiep (3 giay, constitution 1.4 muc 6.2). Kiem bien 21:00 va gio cam tu 20:00.
        /// Tra ve false neu het ngay trong luc doi.
        /// </summary>
        IEnumerator WaitForCustomer()
        {
            yield return WaitUntil(() => S.CurrentCustomer != null || S.Phase != GamePhase.Open,
                S.Balance.day.nextCustomerSeconds + 1f, "khach ke tiep vao sau 3 giay");
            CheckClock();
        }

        void CheckClock()
        {
            string hud = HudText("HUD_Trai");
            StringAssert.IsMatch("Ngày " + S.Day + " · .*[0-9]+:[0-9][0-9]", hud, "HUD hien ngay va gio");
            if (S.Clock.Hour >= S.Balance.day.closeHour - 1 && S.Phase != GamePhase.DayEnd)
            {
                StringAssert.Contains("<color=", hud, "tu 20:00 gio doi mau");
                sawLateClock = true;
            }
            if (S.Phase == GamePhase.Open && S.IsClosedForNewCustomers && S.CurrentCustomer != null)
            {
                StringAssert.StartsWith("ĐÃ ĐÓNG", TextOf("ChuBien").Trim(), "21:00 bien tu lat sang DA DONG");
                sawClosedSign = true;
            }
        }

        /// <summary>Doc mon khach goi tren bong bong, nau dung mon (neu quay cho chua co), mang ra, bam chuong.</summary>
        IEnumerator ServeOne()
        {
            yield return WaitForCustomer();
            if (S.Phase != GamePhase.Open) yield break;
            // Kho khong con cap nguyen lieu cho mon goi: quay them thung (neu con ve), khong thi dong quan som.
            while (OrderedOnShelf() < 0 && !CanCook(S.CurrentCustomer.orderId))
            {
                if (S.Wallet.Count == 0)
                {
                    yield return Use("ShopSign");
                    yield break;
                }
                yield return Use("Kiosk");
                yield return new WaitForSeconds(1.5f);
            }
            string order = S.CurrentCustomer.orderName;
            StringAssert.Contains(order, TextOf("BongThoai"), "mon khach goi hien tren bong bong (Dieu 9)");
            if (OrderedOnShelf() < 0) yield return CookOne(true, S.CurrentCustomer.orderId);
            yield return null;
            int index = OrderedOnShelf();
            Assert.GreaterOrEqual(index, 0, "quay cho phai co " + order);
            yield return UseShelfDish(index);
            Assert.AreEqual(HandKind.Dish, g.Hand);
            Assert.AreEqual(S.CurrentCustomer.orderId, g.HeldDish.recipeId);
            StringAssert.StartsWith("Bấm chuông: phục vụ", BellPrompt());
            int money = S.Money;
            int expected = Payment.Settle(S.Balance, S.Round, S.CurrentCustomer.easiness, g.HeldDish.tier).amount;
            int servedBefore = S.ServedToday;
            CheckClock();
            if (S.IsClosedForNewCustomers && shot21h)
            {
                shot21h = false;
                yield return Shot("3d_21h_da_dong.png", g.Room.Station("ShopSign").WorldCenter);
            }
            yield return Use("Bell");
            Assert.AreEqual(money + expected, S.Money, "tien doi dung bang tinh tien");
            Assert.AreEqual(servedBefore + 1, S.ServedToday);
            maxServedInDay = Mathf.Max(maxServedInDay, S.ServedToday);
        }

        bool CanCook(string recipeId)
        {
            Recipe r = S.Lookup.ById(recipeId);
            return S.Stock.Items.Any(i => i.kind == IngredientKind.Main && i.typeId == r.mainId)
                && S.Stock.Items.Any(i => i.kind == IngredientKind.Side && i.typeId == r.sideId);
        }

        string BellPrompt()
        {
            return g.Room.Station("Bell").Prompt();
        }

        /// <summary>Dua mot dia khac mon khach goi: goi y o chuong canh bao, khach nhan nhung gian (constitution muc 6.7).</summary>
        IEnumerator ServeWrongDish()
        {
            int index = -1;
            for (int i = 0; i < S.Counter.Dishes.Count; i++)
                if (S.Counter.Dishes[i].recipeId != S.CurrentCustomer.orderId) index = i;
            Assert.GreaterOrEqual(index, 0, "can mot dia khac mon de thu");
            yield return UseShelfDish(index);
            StringAssert.StartsWith("Sai món: khách gọi " + S.CurrentCustomer.orderName, BellPrompt());
            int money = S.Money;
            int served = S.ServedToday;
            yield return Use("Bell");
            Assert.AreEqual(money - 18, S.Money, "sai mon: quan den 30% x 60G");
            Assert.AreEqual(served + 1, S.ServedToday);
            StringAssert.StartsWith("Sai món", g.Toast);
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
            Debug.Log("Gia lap 3D: ngay " + S.Day + " phuc vu " + S.ServedToday + " khach, dong quan luc " + S.ClosedAt);
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
        IEnumerator Shot(string fileName, Vector3? lookAt = null, string sprint = "sprint-2")
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

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", sprint, "ui"));
            string path = Path.Combine(dir, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
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
            StringAssert.Contains("750G", HudText("HUD_Trai"));
            StringAssert.IsMatch("Ngày 1 · 9:[0-9][0-9]", HudText("HUD_Trai"), "dong ho chay tu 9:00 (F-011 AC-1)");
            StringAssert.Contains("8 vé", HudText("HUD_Phai"));

            // Nhin vao may nhet ve: goi y hien o tam ngam, bam thi thung roi xuong ban (nguyen lieu la vat 3D)
            yield return Use("Kiosk");
            StringAssert.Contains("7 vé", HudText("HUD_Phai"));
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
                // Mat theo bac (Art Bible muc 7, anh 08), quay ve nguoi choi
                Assert.IsNotNull(v.Face, ing.name + " phai co mat");
                Assert.AreEqual("Mat_NguyenLieu_" + ing.tier, v.Face.Texture.name);
                Assert.Greater(Vector3.Dot(v.Face.transform.forward, P.Cam.transform.forward), 0.99f);
                Assert.IsNull(v.Face.GetComponent<Collider>(), "mat khong chan tia nhin");
                itemCount++;
            }
            Assert.AreEqual(5, itemCount, "1 ve = 3 do phu + 2 chinh (constitution 1.4 muc 4.2)");

            // Bang ti le tren tuong Goc 2 (Dieu 4, Dieu 9)
            string odds = TextOf("BangTiLe");
            foreach (string pct in new[] { "25%", "57%", "15%", "3%" }) StringAssert.Contains(pct, odds);

            // Sach nau an: trang trai ten mon, trang phai thu tu cho vao co so tron (constitution muc 5.8)
            yield return Approach(g.Room.Station("BookNext"));
            string page1 = TextOf("TrangSach");
            StringAssert.Contains("Trang 1 / 9", page1);
            StringAssert.Contains("Thứ tự cho vào", page1);
            // Gia ban theo bac mon tren trang sach (vong 1: gia goc 60G x he so bac, constitution muc 5.2)
            StringAssert.Contains("Giá bán", page1);
            foreach (string price in new[] { "12G", "30G", "60G", "108G", "192G" }) StringAssert.Contains(price, page1);
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

            cookShots = "nau_sai_thu_tu";
            yield return CookOne(correctOrder: false);
            Assert.AreEqual(0, S.Counter.Dishes[0].tier, "sai thu tu phai ra Lac vi");

            // Mon dung cong thuc
            shotCooking = "3d_bep_dang_nau.png";
            cookShots = "nau";
            yield return CookOne();
            yield return WalkTo(new Vector3(-1.2f, 0f, 0.2f));
            yield return Shot("3d_bep.png", new Vector3(-3.3f, 1.75f, 0f));
            yield return Approach(g.Room.Station("PlateStack"));
            yield return Shot("3d_quay_cho.png", g.Room.ShelfDishes[0].transform.position);

            // Mo quan bang bien o Goc 1, phuc vu toi 21:00 (khach khong gioi han, constitution 1.4 muc 6.2)
            Assert.IsNull(S.CurrentCustomer);
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.Open, S.Phase);
            StringAssert.Contains("Dễ tính " + S.CurrentCustomer.easiness + "%", TextOf("BongThoai"));
            StringAssert.Contains(S.CurrentCustomer.orderName, TextOf("BongThoai"), "khach goi mot mon cu the");
            yield return WalkTo(new Vector3(0.3f, 0f, 0.9f));
            yield return Shot("3d_goc1_khach.png", new Vector3(-0.6f, 2.1f, 3.0f));
            bool wrongTried = false;
            while (S.Phase == GamePhase.Open)
            {
                yield return ServeOne();
                if (S.ServedToday == 1 && S.Phase == GamePhase.Open)
                {
                    // Khach 2: nau dung mon goi, chup luc cam dia toi chuong.
                    yield return WaitForCustomer();
                    if (S.Phase != GamePhase.Open) break;
                    if (OrderedOnShelf() < 0) yield return CookOne(true, S.CurrentCustomer.orderId);
                    yield return UseShelfDish(OrderedOnShelf());
                    yield return Approach(g.Room.Station("Bell"));
                    yield return Shot("3d_cam_dia_toi_chuong.png");
                    yield return Use("Bell");
                }
                if (S.ServedToday == 2 && S.Phase == GamePhase.Open && !wrongTried)
                {
                    // Khach 3: thu dua sai mon (constitution muc 6.7).
                    yield return WaitForCustomer();
                    if (S.Phase != GamePhase.Open) break;
                    bool hasOther = S.Counter.Dishes.Any(d => d.recipeId != S.CurrentCustomer.orderId);
                    // Thung 1.4 nho (3 + 2): kho co the chi lam duoc mot mon, quay them thung cho toi khi co mon khac.
                    string other = null;
                    while (!hasOther && S.Wallet.Count > 0
                        && (other = S.MakeableRecipes().Select(r => r.id).FirstOrDefault(id => id != S.CurrentCustomer.orderId && CanCook(id))) == null)
                    {
                        yield return Use("Kiosk");
                        yield return new WaitForSeconds(1.5f);
                    }
                    if (!hasOther && other != null) yield return CookOne(true, other);
                    Assert.IsTrue(S.Counter.Dishes.Any(d => d.recipeId != S.CurrentCustomer.orderId),
                        "can mot dia khac mon de thu (" + S.Clock.Text + ", " + S.Phase + ", ve " + S.Wallet.Count + ")");
                    yield return ServeWrongDish();
                    wrongTried = true;
                }
            }
            Assert.IsTrue(wrongTried, "van gia lap nay phai thu dua sai mon mot lan");
            Debug.Log("Gia lap 3D: ngay 1 phuc vu " + S.ServedToday + " khach, dong quan luc " + S.ClosedAt);
            Assert.IsTrue(g.ShowsOverlay);
            StringAssert.Contains("Đã phục vụ " + S.ServedToday + " khách", HudText("ManPhu"), "F-011 AC-7");
            StringAssert.Contains("đóng quán lúc " + S.ClosedAt, HudText("ManPhu"), "F-011 AC-7");
            yield return Shot("3d_het_ngay_1.png");
            int ticketsLeft = S.Wallet.Count;
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(2, S.Day);
            Assert.AreEqual(ticketsLeft + 6, S.Wallet.Count, "6 ve moi ngay (constitution 1.4 muc 3)");
            StringAssert.Contains("Ngày 2 · 9:0", HudText("HUD_Trai"), "sang ngay moi dong ho ve 9:00");
            Assert.AreEqual(0, g.Room.Crates.Count(), "nguyen lieu thua bi bo cuoi ngay");

            // Ngay 2
            yield return PlayDay(4, "3d_ngay2");
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(3, S.Day);

            // Ngay 3 sau mua: bang ti le doi, thung nho
            StringAssert.Contains("50%", TextOf("BangTiLe"));
            StringAssert.Contains("sau mưa", TextOf("BangTiLe"));
            yield return Use("Kiosk");
            Assert.AreEqual(2, S.Stock.Items.Count(i => i.kind == IngredientKind.Side), "ngay 3: 2 do phu (constitution 1.4 muc 4.3)");
            Assert.AreEqual(1, S.Stock.Items.Count(i => i.kind == IngredientKind.Main), "ngay 3: 1 chinh");
            yield return PlayDay(4, "3d_ngay3");
            StringAssert.Contains("Sáng mai chủ nợ thu 750G", HudText("ManPhu"));

            // F-011: khach thu 7 tro di van vao; 21:00 bien tu dong; tu 20:00 gio doi mau
            Assert.GreaterOrEqual(maxServedInDay, 7, "khach khong gioi han: co ngay phuc vu tu 7 khach tro len");
            Assert.IsTrue(sawLateClock, "phai thay gio doi mau sau 20:00");

            // F-014: moi mon phuc vu co mot dong thoi gian trong playtest-log.csv
            Assert.GreaterOrEqual(g.Log.RowsWritten, 15, "moi mon da phuc vu ghi mot dong");
            string[] lines = File.ReadAllLines(g.Log.Path);
            Assert.AreEqual(PlaytestLog.Header, lines[0]);
            string last = lines[lines.Length - 1];
            Assert.AreEqual(10, last.Split(',').Length, "du 10 cot: " + last);
            Debug.Log("Gia lap 3D: playtest-log " + g.Log.Path + " +" + g.Log.RowsWritten + " dong, dong cuoi: " + last);

            int before = S.Money;
            Assert.GreaterOrEqual(before, 750, "van gia lap nay phai kiem du 750G");
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(GamePhase.Cleared, S.Phase);
            Assert.AreEqual(before - 750, S.Money);
            StringAssert.Contains("Qua vòng 1", HudText("ManPhu"));
            yield return Shot("3d_qua_vong_1.png");

            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, S.Day);
            Assert.AreEqual(GamePhase.Prep, S.Phase);
            Debug.Log("Gia lap 3D: qua vong 1 voi " + before + "G truoc khi tra no.");
        }

        /// <summary>
        /// F-012: moi nguyen lieu co hinh rieng (vong 1 >= 3 khoi, con lai >= 2 khoi), da so che doi hinh,
        /// van giu de bac dung mau va mat ngoai than. Chup anh can tung loai (nguyen trai, so che phai) vao docs/sprint-2/ui/nguyen_lieu/.
        /// </summary>
        [UnityTest]
        public IEnumerator F012_HinhNguyenLieu_AnhCan()
        {
            RecipeBook book = S.Recipes;
            var all = book.sides.Select(x => new { x.id, x.name, side = true }).Concat(book.mains.Select(x => new { x.id, x.name, side = false })).ToList();
            Assert.AreEqual(22, all.Count);
            Time.timeScale = 1f;
            Vector3 spot = new Vector3(0f, 1.2f, 0.4f);
            P.Teleport(new Vector3(0f, 0f, -0.25f), 0f);
            yield return null;
            foreach (var x in all)
            {
                List<IngredientModels.Part> raw = IngredientModels.Parts(x.id, false);
                List<IngredientModels.Part> cut = IngredientModels.Parts(x.id, true);
                bool round1 = Round1Ingredients.Contains(x.id);
                Assert.GreaterOrEqual(raw.Count, round1 ? 3 : 2, x.name + ": so khoi");
                Assert.GreaterOrEqual(cut.Count, 2, x.name + " da so che: so khoi");
                Assert.AreNotEqual(PartNames(raw), PartNames(cut), x.name + ": da so che phai doi hinh");

                var holder = new GameObject("AnhCan_" + x.id).transform;
                holder.position = spot;
                var views = new List<ItemView>();
                for (int k = 0; k < 2; k++)
                {
                    Ingredient ing = x.side
                        ? Ingredient.FromSide(book.sides.Single(o => o.id == x.id), 2)
                        : Ingredient.FromMain(book.mains.Single(o => o.id == x.id), 2);
                    ItemView v = ItemView.Create(holder, ing, 2.2f, false);
                    v.transform.localPosition = new Vector3(k == 0 ? -0.13f : 0.13f, 0f, 0f);
                    v.SetPrepared(k == 1);
                    views.Add(v);
                }
                yield return null;
                foreach (ItemView v in views)
                {
                    Assert.AreEqual(ColorUtility.ToHtmlStringRGB(UiKit.IngredientTier[2]), ColorUtility.ToHtmlStringRGB(v.DiscColor), "de bac dung mau");
                    Assert.AreEqual("Mat_NguyenLieu_2", v.Face.Texture.name);
                    Assert.GreaterOrEqual(v.PartCount, round1 ? 3 : 2, x.name);
                    AssertFaceOutside(v.gameObject);
                }
                Assert.IsTrue(views[1].ShowsPrepared);
                yield return Shot("nguyen_lieu/" + x.id + ".png", spot + new Vector3(0f, 0.07f, 0f));
                Object.Destroy(holder.gameObject);
                yield return null;
            }
        }

        /// <summary>F-011 AC-4, AC-7 tren canh that: 21:00 bien tu lat, khach dang ngoi van cho, phuc vu xong thi het ngay.</summary>
        [UnityTest]
        public IEnumerator Gio21_BienTuDong_KhachDangNgoiVanCho_PhucVuXongHetNgay()
        {
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.5f);
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.Open, S.Phase);
            Customer seated = S.CurrentCustomer;
            Assert.IsNotNull(seated);
            StringAssert.StartsWith("ĐANG MỞ", TextOf("ChuBien").Trim());

            // Tua dong ho toi qua 21:00 (khach dang ngoi khong roi di)
            g.ClockScale = 40f;
            yield return WaitUntil(() => S.IsClosedForNewCustomers, S.Clock.DaySeconds / 40f + 5f, "dong ho toi 21:00");
            g.ClockScale = 1f;
            yield return null;

            Assert.AreEqual(GamePhase.Open, S.Phase, "con khach ngoi thi chua het ngay");
            Assert.AreSame(seated, S.CurrentCustomer);
            StringAssert.StartsWith("ĐÃ ĐÓNG", TextOf("ChuBien").Trim(), "21:00 bien tu lat sang DA DONG");
            StringAssert.Contains("<color=", HudText("HUD_Trai"), "sau 20:00 gio doi mau");
            StringAssert.Contains("21:", HudText("HUD_Trai"));
            StringAssert.Contains("phục vụ nốt", g.Toast);
            yield return Shot("3d_21h_da_dong.png", g.Room.Station("ShopSign").WorldCenter + new Vector3(-1.2f, 0.4f, 0f));

            yield return ServeOne();
            Assert.AreEqual(GamePhase.DayEnd, S.Phase, "phuc vu xong khach cuoi sau 21:00 thi het ngay");
            StringAssert.Contains("Đã phục vụ 1 khách", HudText("ManPhu"));
            StringAssert.Contains("đóng quán lúc " + S.ClosedAt, HudText("ManPhu"));
            yield return Shot("3d_het_ngay_21h.png");
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
    
        // =====================================================================
        // Sprint 3: cua hang relic Goc 3 (F-020, F-021 AC-4, AC-5) va ruong dot pha (F-022 AC-3)
        // =====================================================================

        const string Sprint3 = "sprint-3";

        /// <summary>Dung truoc be / gia relic o Goc 3 (truoc hang be, cach tuong 1,6 m), nhin vao, bam.</summary>
        IEnumerator UseGoc3(string stationId)
        {
            Interactable target = g.Room.Station(stationId);
            Assert.IsNotNull(target, "Khong co tram " + stationId);
            Transform goc3 = GameObject.Find("NhaHang").transform.Find("Goc3");
            Vector3 local = goc3.InverseTransformPoint(target.WorldCenter);
            Vector3 stand = goc3.TransformPoint(new Vector3(Mathf.Clamp(local.x, -1.6f, 2.4f), 0f, 2.35f));
            stand.y = 0f;
            yield return WalkTo(stand);
            P.LookAt(target.WorldCenter);
            yield return null;
            P.LookAt(target.WorldCenter);
            Assert.AreSame(target, P.Focus, "Nhin vao " + target.Id + " nhung tam ngam trung " + (P.Focus != null ? P.Focus.Id : "khong gi"));
            P.TapPrimary();
            yield return null;
        }

        IEnumerator EndDayEarly()
        {
            yield return Use("ShopSign");
            yield return Use("ShopSign");
            Assert.AreEqual(GamePhase.DayEnd, S.Phase);
            g.Hud.OverlayButton.onClick.Invoke();
            yield return null;
        }

        Vector3 Goc3Point(float x, float z)
        {
            Vector3 w = GameObject.Find("NhaHang").transform.Find("Goc3").TransformPoint(new Vector3(x, 0f, z));
            w.y = 0f;
            return w;
        }

        Vector3 PedestalsView()
        {
            return g.Room.Station("Relic_1").WorldCenter + new Vector3(0f, -0.35f, 0f);
        }

        [UnityTest]
        public IEnumerator F020_Goc3_MuaRelic_TheVaGia_DayThiChonRelicDeBo()
        {
            // AC-1: ngay 1 ba be trong, bien "mo tu ngay 2"
            StringAssert.Contains("mở từ ngày 2", g.Room.ShopSignText);
            Assert.IsTrue(g.Room.PedestalRelics.All(r => r == null));
            yield return UseGoc3("Relic_0");
            StringAssert.Contains("mở từ ngày 2", g.Room.Station("Relic_0").CurrentPrompt);
            Assert.AreEqual(8, S.Wallet.Count, "ngay 1 khong mua duoc");
            yield return Shot("goc3_ngay1_be_trong.png", PedestalsView(), Sprint3);

            yield return EndDayEarly();
            Assert.AreEqual(2, S.Day);

            // AC-2, AC-6: 3 relic khac nhau tren be, the ghi ten / gia / do hiem / hieu ung
            Assert.AreEqual(3, S.Shop.Offers.Select(o => o.id).Distinct().Count());
            for (int i = 0; i < 3; i++)
            {
                RelicDef o = S.Shop.Offers[i];
                Assert.IsNotNull(g.Room.PedestalRelics[i], "be " + i + " co relic 3D");
                RelicCard card = g.Room.Cards[i];
                Assert.AreEqual(o.name, card.Title);
                StringAssert.Contains(o.price + " vé", card.Meta);
                StringAssert.Contains(RelicKeys.RarityName(o.rarity), card.Meta);
                StringAssert.Contains(RelicText.Effect(o), card.EffectText);
                Assert.AreEqual(RelicArt.RarityColor(o.rarity), card.FrameColor, "vien the theo do hiem");
            }
            yield return WalkTo(Goc3Point(1.15f, 1.3f));
            yield return Shot("goc3_ngay2_ba_be.png", PedestalsView(), Sprint3);

            // AC-3: bam mua relic re nhat
            int slot = Enumerable.Range(0, 3).OrderBy(i => S.Shop.Offers[i].price).First();
            RelicDef buy = S.Shop.Offers[slot];
            int tickets = S.Wallet.Count;
            Interactable be = g.Room.Station("Relic_" + slot);
            StringAssert.Contains("Mua · " + buy.price + " vé (còn " + tickets + ")", be.CurrentPrompt);
            yield return UseGoc3("Relic_" + slot);
            Assert.AreEqual(tickets - buy.price, S.Wallet.Count, "tru dung so ve");
            Assert.IsTrue(S.Relics.Has(buy.id));
            Assert.IsNull(g.Room.PedestalRelics[slot], "be trong toi het ngay");
            StringAssert.Contains("đã bán", be.CurrentPrompt);
            Assert.IsNotNull(g.Room.ShelfRelics[0], "relic len o dau cua gia");
            for (float t = 0f; t < 1.2f; t += Time.deltaTime) yield return null; // bay tu be len gia 0,7 giay
            Assert.IsNull(g.Room.ShelfRelics[0].GetComponent<RelicFly>(), "bay xong");
            Assert.Less(g.Room.ShelfRelics[0].transform.localPosition.magnitude, 1e-4f, "nam dung o dau cua gia");
            StringAssert.Contains("Giá relic 1/8", g.Room.ShelfHeaderText);
            if (!string.IsNullOrEmpty(buy.set)) StringAssert.Contains(RelicKeys.SetName(buy.set) + " 1/4", g.Room.ShelfHeaderText);
            yield return null;
            yield return Shot("goc3_vua_mua.png", PedestalsView(), Sprint3);

            // AC-4 (F-021): gia ghi tien do bo
            foreach (string id in new[] { "C1", "C2" }) if (!S.Relics.Has(id)) g.DebugGiveRelic(id);
            StringAssert.Contains("Chợ Sớm " + S.Relics.CountInSet("cho") + "/4", g.Room.ShelfHeaderText);
            // F-021 AC-5: bang ti le Goc 2 hien tac dung relic (Dao phay + Thung da + thuong bo 2)
            StringAssert.Contains("Có relic", g.Room.OddsHeaderText);
            StringAssert.Contains("Chính", g.Room.OddsCellText(1));
            yield return Use("Kiosk");
            yield return Shot("goc2_bang_ti_le_co_relic.png", GameObject.Find("BangTiLe").transform.position, Sprint3);

            // AC-4: thieu ve
            while (S.Wallet.Count > 0) S.Wallet.TrySpend();
            int other = Enumerable.Range(0, 3).First(i => S.Shop.Offers[i] != null);
            yield return UseGoc3("Relic_" + other);
            StringAssert.Contains("thiếu " + S.Shop.Offers[other].price + " vé", g.Room.Station("Relic_" + other).CurrentPrompt);
            StringAssert.Contains("Thiếu", g.Toast);
            Assert.IsFalse(S.Relics.Has(S.Shop.Offers[other].id));

            // AC-5: day 8 -> bam be thi phai chon relic tren gia de bo; huy thi khong mua
            foreach (RelicDef r in S.Balance.relics)
            {
                if (S.Relics.IsFull) break;
                if (!S.Relics.Has(r.id) && !S.Shop.Offers.Contains(r)) g.DebugGiveRelic(r.id);
            }
            Assert.IsTrue(S.Relics.IsFull);
            for (int i = 0; i < 20; i++) S.Wallet.BeginDay(2);
            g.Room.Refresh();
            tickets = S.Wallet.Count;
            RelicDef want = S.Shop.Offers[other];
            yield return UseGoc3("Relic_" + other);
            Assert.AreEqual(other, g.PendingRelicSlot);
            Assert.AreEqual(tickets, S.Wallet.Count, "chua mua khi chua chon relic bo");
            yield return Shot("goc3_day_chon_bo.png", g.Room.Station("Shelf_3").WorldCenter + new Vector3(0f, -0.6f, 0f), Sprint3);
            P.TapSecondary(); // huy
            yield return null;
            Assert.AreEqual(-1, g.PendingRelicSlot);
            Assert.IsFalse(S.Relics.Has(want.id));
            Assert.AreEqual(tickets, S.Wallet.Count);

            yield return UseGoc3("Relic_" + other);
            RelicDef dropped = S.Relics.Owned[2];
            StringAssert.Contains("Bỏ " + dropped.name + " để lấy " + want.name, g.Room.Station("Shelf_2").CurrentPrompt);
            yield return UseGoc3("Shelf_2");
            Assert.IsTrue(S.Relics.Has(want.id));
            Assert.IsFalse(S.Relics.Has(dropped.id));
            Assert.AreEqual(8, S.Relics.Owned.Count);
            Assert.AreEqual(tickets - want.price, S.Wallet.Count);
            yield return null;
            yield return Shot("goc3_gia_8_o.png", g.Room.Station("Shelf_3").WorldCenter + new Vector3(0f, -0.6f, 0f), Sprint3);
        }

        [UnityTest]
        public IEnumerator F022_RuongDotPha_NapVomTim_NauRaMonDotPha()
        {
            foreach (string id in new[] { "S1", "S2", "S3", "S4" }) Assert.IsTrue(g.DebugGiveRelic(id));
            for (int i = 0; i < 8 && S.Stock.Items.All(it => !IsBreakthrough(it)); i++) yield return Use("Kiosk");
            Ingredient bt = S.Stock.Items.FirstOrDefault(IsBreakthrough);
            Assert.IsNotNull(bt, "8 luot voi 80%/luot ma khong ra ruong dot pha");
            yield return null;
            CrateView chest = g.Room.Crates.Single(c => c.TypeId == bt.typeId);
            Assert.IsTrue(chest.IsBreakthrough);
            Assert.AreEqual(5, bt.tier, "thuong bo Son Hao 4: moi mon dot pha bac 5");
            Assert.IsNotNull(chest.transform.Find("NapVom"), "ruong co nap vom");
            for (float t = 0f; t < 1.5f; t += Time.deltaTime) yield return null; // thung roi xong (1,2 giay)
            yield return Shot("goc2_ruong_dot_pha.png", chest.transform.position + new Vector3(0f, 0.15f, 0f), Sprint3);

            Ingredient side = S.Stock.Items.First(i => i.kind == IngredientKind.Side);
            Recipe r = S.Lookup.Find(bt.typeId, side.typeId);
            Assert.AreEqual("dotpha", r.group);
            yield return CookOne(true, r.id);
            Dish dish = S.Counter.Dishes.Last();
            Assert.AreEqual(r.id, dish.recipeId, "nau ra mon dot pha dung cong thuc 5.8");
            yield return Approach(g.Room.Station("PlateStack"));
            yield return Shot("goc4_mon_dot_pha.png", g.Room.ShelfDishes[S.Counter.Dishes.Count - 1].transform.position, Sprint3);
        }

        bool IsBreakthrough(Ingredient i)
        {
            return S.Recipes.mains.Any(m => m.id == i.typeId && m.group == "dotpha");
        }
    }
}
