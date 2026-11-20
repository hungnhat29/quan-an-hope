using System.Collections.Generic;
using BepNhanPham;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Do vat vong 2 (sprint 4): 3 ghe khach + bong bong + thanh kien nhan (F-030, F-031), tu lanh (F-032),
    /// khoi qua lua (F-033), quay vat pham / lo nuoc sot / may gacha do an / khe ve den (F-034), ngay mat dien (F-035).
    /// </summary>
    public partial class RoomView
    {
        // ---------------- Goc 1: ghe khach ----------------

        /// <summary>Mot ghe: khach 3D (ban sao khach cua canh), bong bong, dia vua phuc vu.</summary>
        public class SeatView
        {
            public Transform Pivot;
            public GameObject Model;
            public FaceSwap Face;
            public Canvas Bubble;
            public TextMeshProUGUI Text;
            public ProgressBar Patience;
            public Image PatienceFill;
            public Transform ServedSpot;
            public GameObject ServedDish;
            public Interactable Hit;
            public string ShownText;

            /// <summary>Do cua chu no (mu, day vang) va vien vang bong bong (sprint 5, Art Bible muc 5, 6).</summary>
            public GameObject BossLook;
            public Image BossFrame;

            /// <summary>Khach dang an (constitution 1.5 muc 6.2): diem truoc mieng, dia dang an, hoat anh.</summary>
            public Transform Mouth;
            public Dish ServedFor;
            public EatingFx Eating;
            public Customer EaterShown;
            /// <summary>Tam trang da dien (0 cho, 1 vui, 2 gian): doi thi nhay cang / lac dau.</summary>
            public int Mood;
            public float LeaveLeft;
        }

        // Ba ghe doc quay (x), ti le khach khi ngoi 3 nguoi; vong 1 mot khach giua, co nhu canh cu.
        static readonly float[] SeatX = { -1.45f, 0f, 1.45f };
        const float SeatZ = 3.25f;
        const float CrowdScale = 0.72f;
        static readonly Vector2 BubbleSize = new Vector2(1250f, 560f);

        readonly SeatView[] seatViews = new SeatView[3];
        int seatLayout = -1;

        public IReadOnlyList<SeatView> SeatViews { get { return seatViews; } }

        void BuildSeats()
        {
            Transform src = goc1.Find("Customer");
            for (int i = 0; i < seatViews.Length; i++)
            {
                var v = new SeatView();
                v.Pivot = Toon.Empty(goc1, "Ghe_" + i, new Vector3(SeatX[i], 0f, SeatZ)).transform;
                if (src != null)
                {
                    v.Model = Object.Instantiate(src.gameObject, v.Pivot);
                    v.Model.name = "Khach_" + i;
                    v.Model.transform.localPosition = new Vector3(0f, 0f, -SeatZ);
                    v.Model.transform.localRotation = Quaternion.identity;
                    v.Model.transform.localScale = Vector3.one;
                    v.Face = v.Model.GetComponent<FaceSwap>();
                    Lively.On(v.Model);   // tho, dung dua, nhay vao ghe, nhai, roi ghe muot (20/11)
                    Toon.BlobShadow(v.Model.transform, "Bong", new Vector3(0f, 0.01f, SeatZ), 1.4f);
                    v.BossLook = BuildBossLook(v.Model.transform);
                    // dau khach cua canh: tam (0; 2,2; SeatZ), ban kinh 0,6 m -> mieng o mat truoc, thap hon tam
                    v.Mouth = Toon.Empty(v.Model.transform, "Mieng", new Vector3(0f, 1.98f, SeatZ - 0.62f)).transform;
                }
                int seat = i;
                v.Hit = Register(Interactable.Add(v.Pivot.gameObject, "Customer_" + i, new Vector3(0f, 1.9f, 0f), new Vector3(1.4f, 2.4f, 1.0f),
                    () => CustomerPrompt(seat), () => g.ServeSeat(seat)));

                // Bong bong tren dau khach (constitution muc 6.7, Dieu 9): do de tinh, mon goi, thanh kien nhan (muc 6.6).
                v.Bubble = UiKit.WorldCanvas(goc1, i == 0 ? "BongThoai" : "BongThoai_" + i, Vector3.zero, Quaternion.identity, BubbleSize);
                // vien vang cho bong bong chu no (kich ban 10 giay sprint 5), nam duoi nen kem
                v.BossFrame = UiKit.Panel(v.Bubble.transform, "VienChuNo", new Vector2(0.5f, 0.5f), Vector2.zero, BubbleSize + Vector2.one * 60f, BossGold, false);
                v.BossFrame.raycastTarget = false;
                v.BossFrame.gameObject.SetActive(false);
                Image bb = UiKit.Panel(v.Bubble.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, BubbleSize, UiKit.Cream);
                bb.raycastTarget = false;
                v.Text = UiKit.Label(bb.transform, "Chu", new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(BubbleSize.x - 40f, 390f), "", 76f);
                v.Text.lineSpacing = -18f;
                v.Patience = ProgressBar.Create(bb.transform, "KienNhan", new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(BubbleSize.x - 80f, 84f), UiKit.Mint);
                v.PatienceFill = v.Patience.transform.Find("Fill").GetComponent<Image>();
                v.ServedSpot = Toon.Empty(goc1, "DiaVuaPhucVu_" + i, new Vector3(SeatX[i] - 0.2f, 1.22f, 2.25f)).transform;
                seatViews[i] = v;
            }
            if (src != null) src.gameObject.SetActive(false);
        }

        static readonly Color BossGold = UiKit.Hex("#F2C14E");
        static readonly Color BossVest = UiKit.Hex("#4A3F3A");

        /// <summary>
        /// Chu no dung lai khoi khach cua canh (impl plan sprint 5 ngay 5: "model khoi hien co + mat lanh lung") them mu
        /// chu nhat bo nho mau vest #4A3F3A, bang mu va day vang #F2C14E (Art Bible muc 5: quyen luc = chu nhat bo nho).
        /// </summary>
        GameObject BuildBossLook(Transform model)
        {
            // Mu dat theo tam mat (khung bao ca khoi khach gom tay va ang-ten nen qua to): rong ~ mat, ngoi tren dinh dau.
            FaceSwap fs = model.GetComponentInChildren<FaceSwap>();
            Renderer face = fs != null ? fs.faceRenderer : null;
            GameObject look = Toon.Empty(model, "ChuNo", Vector3.zero);
            if (face == null) { look.SetActive(false); return look; }
            Bounds fb = face.bounds;
            float fw = Mathf.Max(fb.size.x, fb.size.z);
            Vector3 back = model.position - fb.center;
            back.y = 0f;
            back = back.sqrMagnitude > 1e-6f ? back.normalized : Vector3.forward;
            Vector3 headTop = fb.center + back * fw * 0.35f + Vector3.up * (fb.extents.y + fw * 0.32f);
            Vector3 top = model.InverseTransformPoint(headTop);
            Vector3 chin = model.InverseTransformPoint(new Vector3(fb.center.x, fb.min.y - fw * 0.06f, fb.center.z) - back * fw * 0.05f);
            float k = 1f / Mathf.Max(1e-4f, model.lossyScale.x);
            float w = fw * k;
            Toon.Prim(PrimitiveType.Cube, look.transform, "VanhMu", top + new Vector3(0f, 0.015f * w, 0f), new Vector3(w * 1.05f, 0.04f * w, w * 0.9f), BossVest, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, look.transform, "Mu", top + new Vector3(0f, 0.2f * w, 0f), new Vector3(w * 0.66f, 0.36f * w, w * 0.58f), BossVest, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, look.transform, "BangMu", top + new Vector3(0f, 0.07f * w, 0f), new Vector3(w * 0.67f, 0.07f * w, w * 0.59f), BossGold, 0f);
            // day vang ngay duoi cam (Art Bible muc 5: vest #4A3F3A, day vang #F2C14E)
            for (int n = 0; n < 5; n++)
            {
                float t = (n - 2) / 2f;
                Toon.Prim(PrimitiveType.Sphere, look.transform, "DayVang_" + n, chin + new Vector3(t * w * 0.3f, -0.05f * w * (1f - t * t), 0f),
                    Vector3.one * 0.07f * w, BossGold, 0f);
            }
            look.SetActive(false);
            return look;
        }

        /// <summary>Dat ghe theo so ghe hom nay: 1 ghe (vong 1) o giua co lon, 3 ghe (tu vong 2) nho lai cho vua quay.</summary>
        void LayoutSeats(int count)
        {
            if (count == seatLayout) return;
            seatLayout = count;
            for (int i = 0; i < seatViews.Length; i++)
            {
                SeatView v = seatViews[i];
                bool single = count <= 1;
                float x = single ? 0f : SeatX[i];
                float scale = single ? 1f : CrowdScale;
                v.Pivot.localPosition = new Vector3(x, 0f, SeatZ);
                v.Pivot.localScale = Vector3.one * scale;
                // vong 1 bong bong canh dau nhu cu; 3 ghe thi ngay tren dau tung khach
                v.Bubble.transform.localPosition = single ? new Vector3(-1.4f, 2.8f, 3.0f) : new Vector3(x, 2.72f, 2.95f);
                v.ServedSpot.localPosition = new Vector3(x - 0.2f, 1.22f, 2.25f);
            }
        }

        string CustomerPrompt(int seat)
        {
            GameSession s = g.Session;
            Customer c = seat < s.SeatCount ? s.Seats[seat] : null;
            Customer eater = seat < s.SeatCount && s.Phase == GamePhase.Open ? s.Eaters[seat] : null;
            if (c == null && eater != null)
                return (eater.isBoss ? "Chủ nợ" : char.ToUpper(g.SeatName(seat)[0]) + g.SeatName(seat).Substring(1))
                    + " đang ăn · còn " + Mathf.CeilToInt((1f - s.EatProgress(seat)) * s.Balance.day.eatSeconds) + " giây";
            if (s.Phase != GamePhase.Open || c == null) return "Ghế trống";
            if (c.isBoss)
            {
                if (g.Hand != HandKind.Dish) return "Chủ nợ · gọi " + c.orderName + " · Tuyệt hảo trở lên: thưởng " + s.Balance.bossBonus + "G";
                return "Phục vụ chủ nợ: " + g.HeldDish.Title + (g.HeldDish.tier >= s.Balance.dish.maxTier ? " · được thưởng " + s.Balance.bossBonus + "G" : " · chưa đủ Tuyệt hảo, không có thưởng");
            }
            string who = g.SeatName(seat);
            string whoCap = char.ToUpper(who[0]) + who.Substring(1);
            if (g.Hand != HandKind.Dish) return whoCap + " · gọi " + c.orderName;
            if (g.HeldDish.recipeId != c.orderId) return "Sai món: " + who + " gọi " + c.orderName + " (bấm vẫn đưa, khách sẽ giận)";
            return "Phục vụ " + who + ": " + g.HeldDish.Title;
        }

        void RefreshSeats()
        {
            GameSession s = g.Session;
            LayoutSeats(s.SeatCount);
            for (int i = 0; i < seatViews.Length; i++)
            {
                SeatView v = seatViews[i];
                Customer c = i < s.SeatCount && s.Phase == GamePhase.Open ? s.Seats[i] : null;
                Customer eater = i < s.SeatCount && s.Phase == GamePhase.Open ? s.Eaters[i] : null;
                PaymentOutcome outcome;
                bool reacting = g.IsReacting(i, out outcome);
                bool eatingFace = eater != null && c == null;
                // An xong roi ghe: khoi khach thu nho bay len roi moi an (LeaveFx).
                if (v.EaterShown != null && eater != v.EaterShown && c == null && s.Phase == GamePhase.Open)
                {
                    v.LeaveLeft = LeaveFx.Seconds;
                    LeaveFx.Play(v.Model);
                }
                v.EaterShown = eater;
                if (c != null || eater != null || s.Phase != GamePhase.Open) { v.LeaveLeft = 0f; LeaveFx.Stop(v.Model); }
                bool leaving = v.LeaveLeft > 0f;
                bool show = i < s.SeatCount && (c != null || reacting || eater != null || leaving);
                if (v.Model != null) v.Model.SetActive(show);
                v.Hit.enabled = c != null || eater != null;
                bool bonus;
                bool bossReact = g.IsBossReaction(i, out bonus);
                bool boss = (c != null && c.isBoss) || (reacting && bossReact) || (eatingFace && eater.isBoss);
                if (v.BossLook != null) v.BossLook.SetActive(show && boss);
                v.BossFrame.gameObject.SetActive(c != null && c.isBoss);
                if (v.Face != null && show && boss)
                {
                    // chu no: lanh lung; chi ngac nhien khi duoc mon Tuyet hao / Huyen thoai (Art Bible muc 6)
                    if ((reacting || eatingFace) && bossReact && bonus) v.Face.Show(FaceArt.Boss(true));
                    else if (reacting && bossReact) v.Face.ShowAngry();
                    else v.Face.Show(FaceArt.Boss(false));
                }
                else if (v.Face != null && show)
                {
                    // dang an: giu mat theo ket qua phuc vu (vui / gian / cho)
                    if ((reacting || eatingFace) && outcome == PaymentOutcome.Angry) v.Face.ShowAngry();
                    else if ((reacting || eatingFace) && outcome == PaymentOutcome.Satisfied) v.Face.ShowHappy();
                    else v.Face.ShowWaiting();
                }
                int mood = !show ? 0 : (reacting || eatingFace) && outcome == PaymentOutcome.Angry ? 2
                    : (reacting || eatingFace) && (outcome == PaymentOutcome.Satisfied || (boss && bossReact && bonus)) ? 1 : 0;
                if (mood != v.Mood && v.Model != null)
                {
                    Lively l = v.Model.GetComponent<Lively>();
                    if (l != null && mood == 1) l.Hop();
                    else if (l != null && mood == 2) l.Shake();
                }
                v.Mood = mood;
                v.Bubble.gameObject.SetActive(c != null && !reacting);
                v.ShownText = null;
                UpdateBubble(i);

                // Dia truoc khach: dang an thi dia do (mon voi dan, EatingFx); khong an (eat_s = 0) thi hien dia vua dua mot luc.
                Dish want = eater != null ? s.EatingDish(i)
                    : (g.ServedShowLeft > 0f && g.LastServed != null && g.LastServedSeat == i && s.Balance.day.eatSeconds <= 0f ? g.LastServed : null);
                if (want != v.ServedFor)
                {
                    if (v.ServedDish != null) Object.Destroy(v.ServedDish);
                    v.ServedDish = null;
                    v.Eating = null;
                    v.ServedFor = want;
                    if (want != null)
                    {
                        v.ServedDish = DishModel.Create(v.ServedSpot, want, s.Recipes);
                        if (eater != null && v.Model != null)
                        {
                            int seat = i;
                            v.Eating = EatingFx.Attach(v.ServedDish, v.Model.transform, v.Mouth, () => g.Session.EatProgress(seat));
                        }
                    }
                }
            }
        }

        /// <summary>Dem nguoc hoat anh roi ghe; xong thi an khoi khach.</summary>
        void TickSeats(float dt)
        {
            for (int i = 0; i < seatViews.Length; i++)
            {
                SeatView v = seatViews[i];
                if (v.LeaveLeft <= 0f) continue;
                v.LeaveLeft -= dt;
                if (v.LeaveLeft <= 0f) RefreshSeats();
            }
        }

        /// <summary>Chu va thanh kien nhan cua mot bong bong (moi khung hinh: do de tinh giam theo gio cho).</summary>
        void UpdateBubble(int i)
        {
            GameSession s = g.Session;
            SeatView v = seatViews[i];
            if (!v.Bubble.gameObject.activeSelf || i >= s.SeatCount) return;
            Customer c = s.Seats[i];
            if (c == null) return;
            bool timed = Patience.Applies(s.Balance, s.Day) && !c.isBoss;
            int easy = s.EasinessNow(c);
            string text = c.isBoss
                ? "<size=64>Chủ nợ · Dễ tính " + easy + "%</size>\nGọi: " + c.orderName
                : "<size=64>" + (timed ? char.ToUpper(g.SeatName(i)[0]) + g.SeatName(i).Substring(1) : "Khách " + c.number)
                + " · Dễ tính " + easy + "%</size>\nGọi: " + c.orderName;
            if (text != v.ShownText)
            {
                v.Text.text = text;
                v.ShownText = text;
            }
            v.Patience.gameObject.SetActive(timed);
            if (timed)
            {
                v.Patience.Set(s.PatienceLeft(c));
                // xanh trong khoang an toan; qua khoang an toan doi cam (dang bi tru diem)
                v.PatienceFill.color = s.IsImpatient(c) ? UiKit.Coral : UiKit.Mint;
            }
        }

        public string BubbleText(int seat) { return seatViews[seat].Text.text; }
        public float BubblePatience(int seat) { return seatViews[seat].Patience.Value; }
        public Color BubblePatienceColor(int seat) { return seatViews[seat].PatienceFill.color; }

        // ---------------- Goc 4: tu lanh ----------------

        // Tu treo tuong phia tren gia dung cu, ben phai quay cho (impl plan sprint 4 muc 6: "canh quay cho ben phai").
        static readonly Vector3 FridgeCenter = new Vector3(2.75f, 2.78f, 3.62f);
        static readonly Vector2 FridgeSize = new Vector2(1.5f, 0.8f);
        readonly List<Transform> fridgeSlots = new List<Transform>();
        readonly List<GameObject> fridgeDishes = new List<GameObject>();
        readonly List<GameObject> fridgeSlotHits = new List<GameObject>();
        string fridgeKey;
        TextMeshProUGUI fridgeLabel;
        Transform fridgeRoot;

        public IReadOnlyList<GameObject> FridgeDishes { get { return fridgeDishes; } }

        void BuildFridge()
        {
            fridgeRoot = Toon.Empty(goc4, "TuLanh", FridgeCenter).transform;
            Color body = UiKit.Hex("#9CC7D8");
            Toon.Prim(PrimitiveType.Cube, fridgeRoot, "Than", new Vector3(0f, 0f, 0.08f), new Vector3(FridgeSize.x, FridgeSize.y, 0.36f), body, Toon.OutlineBig);
            Toon.Prim(PrimitiveType.Cube, fridgeRoot, "LongTu", new Vector3(0f, 0f, -0.06f), new Vector3(FridgeSize.x - 0.12f, FridgeSize.y - 0.12f, 0.1f), UiKit.Hex("#EAF4F7"), 0f);
            Toon.Prim(PrimitiveType.Cube, fridgeRoot, "Ke", new Vector3(0f, -0.04f, -0.08f), new Vector3(FridgeSize.x - 0.14f, 0.02f, 0.2f), UiKit.Hex("#C9C1B6"), 0f);
            Toon.Prim(PrimitiveType.Sphere, fridgeRoot, "DenXanh", new Vector3(FridgeSize.x * 0.5f - 0.1f, FridgeSize.y * 0.5f - 0.08f, -0.11f), new Vector3(0.06f, 0.06f, 0.03f), UiKit.Mint, Toon.OutlineSmall);
            Register(Interactable.Add(fridgeRoot.gameObject, "Fridge", new Vector3(0f, 0f, 0.12f), new Vector3(FridgeSize.x, FridgeSize.y, 0.2f), FridgePrompt, () => g.UseFridge()));

            Canvas sign = UiKit.WorldCanvas(goc4, "NhanTuLanh", FridgeCenter + new Vector3(0f, FridgeSize.y * 0.5f + 0.1f, -0.12f), Quaternion.identity, new Vector2(1100f, 120f));
            Image sb = UiKit.Panel(sign.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100f, 120f), UiKit.Cream, false);
            fridgeLabel = UiKit.Text(sb.transform, "Tủ lạnh", 66f);
        }

        string FridgePrompt()
        {
            Fridge f = g.Session.Fridge;
            if (g.Hand == HandKind.Dish && !f.Contains(g.HeldDish))
                return f.IsFull ? "Tủ lạnh đầy" : "Cất " + g.HeldDish.name + " vào tủ lạnh";
            return "Tủ lạnh · " + f.Dishes.Count + "/" + f.Capacity + " ô";
        }

        /// <summary>Ve lai o tu theo suc chua (2, hoac 4 voi Tu lanh 2 cua) va dia dang cat.</summary>
        void RefreshFridge()
        {
            GameSession s = g.Session;
            Fridge f = s.Fridge;
            fridgeLabel.text = "Tủ lạnh · " + f.Dishes.Count + "/" + f.Capacity + (f.NoDecay ? " · không tụt bậc" : " · quá " + s.Balance.fridge.decayDays + " ngày tụt 1 bậc");

            // Chi ve lai khi suc chua, dia, bac hoac dia dang cam doi.
            var key = new System.Text.StringBuilder().Append(f.Capacity).Append('|').Append(g.HeldDish == null ? 0 : g.HeldDish.GetHashCode());
            foreach (Dish d in f.Dishes) key.Append('|').Append(d.GetHashCode()).Append(':').Append(d.tier);
            if (key.ToString() == fridgeKey) return;
            fridgeKey = key.ToString();

            foreach (GameObject d in fridgeDishes) Object.Destroy(d);
            fridgeDishes.Clear();
            foreach (GameObject h in fridgeSlotHits) Object.Destroy(h);
            fridgeSlotHits.Clear();
            fridgeSlots.Clear();

            int cap = f.Capacity;
            float w = (FridgeSize.x - 0.2f) / cap;
            for (int i = 0; i < cap; i++)
            {
                Transform slot = Toon.Empty(fridgeRoot, "O_" + i, new Vector3(-FridgeSize.x * 0.5f + 0.1f + w * (i + 0.5f), -0.03f, -0.1f)).transform;
                fridgeSlots.Add(slot);
                fridgeSlotHits.Add(slot.gameObject);
                if (i >= f.Dishes.Count) continue;
                Dish dish = f.Dishes[i];
                if (ReferenceEquals(dish, g.HeldDish)) continue;
                GameObject model = DishModel.Create(slot, dish, s.Recipes);
                model.transform.localScale = Vector3.one * Mathf.Min(1f, w / 0.42f);
                Dish captured = dish;
                Interactable.Add(model, "FridgeDish_" + i, new Vector3(0f, 0.08f, 0f), new Vector3(0.42f, 0.25f, 0.42f),
                    () => "Lấy " + captured.Title + " · " + UiKit.DishTierName[captured.tier] + " (" + captured.daysInFridge + " ngày trong tủ)",
                    () => g.TakeDish(captured));
                fridgeDishes.Add(model);
            }
        }

        // ---------------- Goc 2: quay vat pham, nuoc sot, may gacha do an, ve den ----------------

        // Ke nho giua may nhet ve va may gacha do an (impl plan sprint 4 muc 6: "canh may gacha do an o Goc 2").
        // Goc phai Goc 2 sat quyen sach Goc 3 nen khong dat o do (tia nhin trung trang sach).
        static readonly Vector3 ShopTable = new Vector3(0.56f, 0f, 2.75f);
        TextMeshProUGUI itemSignText, machineSignText, sauceTag, blackTag;
        GameObject sauceBottle, blackStack, blackSlot;

        public string ItemSignText { get { return itemSignText != null ? itemSignText.text : ""; } }
        public string MachineSignText { get { return machineSignText != null ? machineSignText.text : ""; } }

        void BuildItemShop()
        {
            Transform t = Toon.Empty(goc2, "QuayVatPham", ShopTable).transform;
            Toon.Prim(PrimitiveType.Cube, t, "Than", new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.9f, 0.42f), WoodDark, Toon.OutlineBig);
            Toon.Prim(PrimitiveType.Cube, t, "MatBan", new Vector3(0f, 0.93f, 0f), new Vector3(0.56f, 0.06f, 0.48f), SideCrateColor, Toon.OutlineSmall);

            // Lo nuoc sot than thanh: than tron hong + nap vang + nhan tim
            sauceBottle = Toon.Empty(t, "LoNuocSot", new Vector3(-0.13f, 0.96f, -0.03f));
            Toon.Prim(PrimitiveType.Cylinder, sauceBottle.transform, "Than", new Vector3(0f, 0.12f, 0f), new Vector3(0.16f, 0.12f, 0.16f), UiKit.Hex("#D9382C"), Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cylinder, sauceBottle.transform, "Co", new Vector3(0f, 0.27f, 0f), new Vector3(0.07f, 0.05f, 0.07f), UiKit.Hex("#D9382C"), Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Sphere, sauceBottle.transform, "Nap", new Vector3(0f, 0.33f, 0f), new Vector3(0.09f, 0.06f, 0.09f), UiKit.Mustard, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, sauceBottle.transform, "Nhan", new Vector3(0f, 0.12f, -0.081f), new Vector3(0.11f, 0.08f, 0.005f), UiKit.Cream, 0f);
            Register(Interactable.Add(sauceBottle, "Sauce", new Vector3(0f, 0.17f, 0f), new Vector3(0.22f, 0.4f, 0.24f), SaucePrompt, () => OnSauce()));

            // Xap ve den
            blackStack = Toon.Empty(t, "XapVeDen", new Vector3(0.13f, 0.96f, -0.03f));
            for (int k = 0; k < 3; k++)
                Toon.Prim(PrimitiveType.Cube, blackStack.transform, "Ve" + k, new Vector3(0.005f * k, 0.012f + 0.022f * k, 0f), new Vector3(0.18f, 0.018f, 0.11f), UiKit.Ink, Toon.OutlineSmall, new Vector3(0f, 8f * k, 0f));
            Toon.Prim(PrimitiveType.Cube, blackStack.transform, "Vien", new Vector3(0.01f, 0.068f, 0f), new Vector3(0.14f, 0.004f, 0.06f), UiKit.Mustard, 0f, new Vector3(0f, 16f, 0f));
            Register(Interactable.Add(blackStack, "BlackTicket", new Vector3(0f, 0.06f, 0f), new Vector3(0.22f, 0.2f, 0.24f), BlackPrompt, () => g.BuyItem(GameController.ItemBlack)));

            // Bang gia + ti le nuoc sot va ve den (Dieu 4: hien ti le; may gacha do an giau ti le).
            // Co chu nhu bien "Nhet ve" / bang ti le (Art Bible muc 13): doc duoc tu giua phong.
            Canvas sign = UiKit.WorldCanvas(goc2, "BangVatPham", new Vector3(0.2f, 2.43f, 3.92f), Quaternion.identity, new Vector2(1300f, 960f));
            Image sb = UiKit.Panel(sign.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 960f), UiKit.Cream);
            sb.raycastTarget = false;
            itemSignText = UiKit.Label(sb.transform, "Chu", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240f, 920f), "", 66f);
            itemSignText.lineSpacing = -14f;

            Canvas st = UiKit.WorldCanvas(goc2, "NhanNuocSot", ShopTable + new Vector3(-0.13f, 0.74f, -0.225f), Quaternion.identity, new Vector2(250f, 90f));
            sauceTag = UiKit.Text(UiKit.Panel(st.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250f, 90f), UiKit.Cream, false).transform, "", 32f);
            Canvas bt = UiKit.WorldCanvas(goc2, "NhanVeDen", ShopTable + new Vector3(0.13f, 0.74f, -0.225f), Quaternion.identity, new Vector2(250f, 90f));
            blackTag = UiKit.Text(UiKit.Panel(bt.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250f, 90f), UiKit.Cream, false).transform, "", 32f);

            // May gacha do an (khoi co san trong canh): bam de mua, cam dia bam de quay.
            GameObject machineHit = Toon.Empty(goc2, "MayGachaDoAn", new Vector3(1.6f, 1.25f, 2.95f));
            Register(Interactable.Add(machineHit, "FoodMachine", Vector3.zero, new Vector3(1.5f, 1.6f, 0.6f), MachinePrompt, () => g.UseMachineOnHeld()));

            // Khe ve den tren may nhet ve
            blackSlot = Toon.Prim(PrimitiveType.Cube, goc2, "KheVeDen", new Vector3(-0.1f, 0.75f, 2.62f), new Vector3(0.32f, 0.12f, 0.06f), UiKit.Ink, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, blackSlot.transform, "Khe", new Vector3(0f, 0f, -0.55f), new Vector3(0.7f, 0.15f, 0.2f), UiKit.Mustard, 0f);
            Register(Interactable.Add(blackSlot, "BlackSlot", Vector3.zero, new Vector3(1.4f, 2.4f, 3f), BlackSlotPrompt, () => g.PullBlack()));
        }

        void OnSauce()
        {
            if (g.Hand == HandKind.Dish && g.Session.Items.Sauces > 0) g.UseSauceOnHeld();
            else g.BuyItem(GameController.ItemSauce);
        }

        string SaucePrompt()
        {
            ItemShop it = g.Session.Items;
            if (!it.IsOpen) return "Nước sốt thần thánh · mở từ ngày " + g.Session.Balance.itemShopFromDay;
            if (g.Hand == HandKind.Dish && it.Sauces > 0) return "Rưới nước sốt lên " + g.HeldDish.name + " (còn " + it.Sauces + " chai)";
            return it.Sauces >= g.Session.Balance.sauce.max ? "Đã giữ tối đa nước sốt" : "Mua nước sốt · " + it.SaucePrice + "G";
        }

        string BlackPrompt()
        {
            ItemShop it = g.Session.Items;
            if (!it.IsOpen) return "Vé đen · mở từ ngày " + g.Session.Balance.itemShopFromDay;
            return it.BlackTickets >= g.Session.Balance.blackTicket.max ? "Đã giữ tối đa vé đen" : "Mua vé đen · " + it.BlackPrice + "G";
        }

        string BlackSlotPrompt()
        {
            int n = g.Session.Items.BlackTickets;
            return n > 0 ? "Nhét vé đen · còn " + n + " vé đen" : "Khe vé đen · chưa có vé đen";
        }

        string MachinePrompt()
        {
            GameSession s = g.Session;
            ItemShop it = s.Items;
            if (!it.IsOpen) return "Máy gacha đồ ăn · mở từ ngày " + s.Balance.itemShopFromDay;
            if (!it.HasMachine) return "Mua máy gacha đồ ăn · " + s.Balance.machine.price + "G";
            if (g.Hand == HandKind.Dish) return g.HeldDish.machined ? g.HeldDish.name + " đã vào máy rồi" : "Cho " + g.HeldDish.name + " vào máy · còn " + it.MachineUsesLeft + " lượt";
            return "Máy gacha đồ ăn · còn " + it.MachineUsesLeft + " lượt hôm nay";
        }

        void RefreshItemShop()
        {
            GameSession s = g.Session;
            ItemShop it = s.Items;
            Balance b = s.Balance;
            var sb = new System.Text.StringBuilder();
            if (!it.IsOpen)
            {
                sb.Append("<size=86>Quầy vật phẩm</size>\n\nMở từ ngày ").Append(b.itemShopFromDay).Append(" (sau lần trả nợ đầu)");
            }
            else
            {
                double[] o = ItemRolls.SauceOdds(b, s.Day, s.Relics.Effects);
                sb.Append("<size=86>Quầy vật phẩm</size>\n");
                sb.Append("<b>Nước sốt thần thánh</b> ").Append(it.SaucePrice).Append("G · có ").Append(it.Sauces).Append("/").Append(b.sauce.max).Append('\n');
                sb.Append("<size=52>Hỏng ").Append(SPct(o[0])).Append(" · Giữ ").Append(SPct(o[1])).Append('\n')
                  .Append("+1 bậc ").Append(SPct(o[2])).Append(" · +2 bậc ").Append(SPct(o[3]));
                if (o[4] > 0) sb.Append(" · Huyền thoại ").Append(SPct(o[4]));
                sb.Append("</size>\n");
                sb.Append("<b>Vé đen</b> ").Append(it.BlackPrice).Append("G · có ").Append(it.BlackTickets).Append("/").Append(b.blackTicket.max).Append('\n');
                int[] bo = b.BlackOdds().percents;
                sb.Append("<size=52>Thùng: ");
                for (int tier = 0; tier < bo.Length; tier++)
                    if (bo[tier] > 0) sb.Append(UiKit.IngredientTierName[tier]).Append(' ').Append(bo[tier]).Append("% · ");
                sb.Length -= 3;
                sb.Append("</size>\n");
                sb.Append("<b>Máy gacha đồ ăn</b> ").Append(it.HasMachine ? "đã mua" : b.machine.price + "G · mua 1 lần");
            }
            itemSignText.text = sb.ToString();
            sauceTag.text = it.IsOpen ? "Nước sốt · " + it.Sauces : "Nước sốt";
            blackTag.text = it.IsOpen ? "Vé đen · " + it.BlackTickets : "Vé đen";
            if (machineSignText != null)
                machineSignText.text = !it.IsOpen ? "Máy gacha đồ ăn\n<size=60>mở từ ngày " + b.itemShopFromDay + "</size>"
                    : !it.HasMachine ? "Máy gacha đồ ăn\n<size=60>" + b.machine.price + "G · bấm để mua</size>"
                    : "Máy gacha đồ ăn\n<size=60>còn " + it.MachineUsesLeft + "/" + b.machine.usesPerDay + " lượt hôm nay</size>";
            blackSlot.SetActive(it.IsOpen);
        }

        static string SPct(double v)
        {
            return System.Math.Round(v).ToString("0") + "%";
        }

        // ---------------- Goc 4: qua lua ----------------

        GameObject smoke;
        Image stovePanelBg;

        void BuildOverheat()
        {
            smoke = Toon.Empty(goc4, "KhoiQuaLua", new Vector3(0f, 1.5f, 3.2f));
            Material m = FaceArt.Mat(FaceArt.Fx("khoi"));
            for (int i = 0; i < 4; i++)
            {
                GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                q.name = "Khoi" + i;
                Object.Destroy(q.GetComponent<Collider>());
                q.transform.SetParent(smoke.transform, false);
                q.GetComponent<MeshRenderer>().sharedMaterial = m;
                q.AddComponent<SmokePuff>().Init(i * 0.3f, new Vector3(-0.2f + 0.13f * i, 0f, 0f));
            }
            smoke.SetActive(false);
        }

        public bool SmokeVisible { get { return smoke != null && smoke.activeSelf; } }
        public Color StovePanelColor { get { return stovePanelBg != null ? stovePanelBg.color : Color.clear; } }

        void TickOverheat()
        {
            Stove st = g.Session.Stove;
            bool warn = st.IsOverheating;
            if (smoke.activeSelf != warn) smoke.SetActive(warn);
            if (stovePanelBg != null)
                stovePanelBg.color = st.IsBurnt ? UiKit.Coral : warn ? UiKit.Mustard : UiKit.Cream;
        }

        // ---------------- Ngay mat dien (F-035) ----------------

        readonly List<GameObject> candles = new List<GameObject>();
        Light mainLight;
        float mainIntensity = -1f;
        bool blackoutShown;

        public bool BlackoutShown { get { return blackoutShown; } }
        public float MainLightIntensity { get { return mainLight != null ? mainLight.intensity : 0f; } }
        public IReadOnlyList<GameObject> Candles { get { return candles; } }

        void BuildCandles()
        {
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { mainLight = l; break; }
            if (mainLight != null) mainIntensity = mainLight.intensity;

            // Nen quanh bep (Goc 4) va quay (Goc 1) - Art Bible muc 4: ngay mat dien, anh nen.
            AddCandle(goc4, new Vector3(-0.7f, 1.1f, 2.85f));
            AddCandle(goc4, new Vector3(0.7f, 1.1f, 2.85f));
            AddCandle(goc4, new Vector3(-2.2f, 1.1f, 3.5f));
            AddCandle(goc4, new Vector3(2.2f, 1.1f, 3.55f));
            AddCandle(goc1, new Vector3(-1.95f, 1.22f, 2.05f));
            AddCandle(goc1, new Vector3(0.75f, 1.22f, 2.05f));
            AddCandle(goc2, new Vector3(0.56f, 0.96f, 2.9f)); // giua lo nuoc sot va xap ve den
            AddCandle(goc3, new Vector3(-2.2f, 1.2f, 2.6f));
            foreach (GameObject c in candles) c.SetActive(false);
        }

        void AddCandle(Transform parent, Vector3 pos)
        {
            GameObject c = Toon.Empty(parent, "Nen_" + candles.Count, pos);
            Toon.Prim(PrimitiveType.Cylinder, c.transform, "Than", new Vector3(0f, 0.07f, 0f), new Vector3(0.06f, 0.07f, 0.06f), UiKit.Cream, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cylinder, c.transform, "Dia", new Vector3(0f, 0.005f, 0f), new Vector3(0.12f, 0.005f, 0.12f), UiKit.Mustard, Toon.OutlineSmall);
            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "Quang";
            Object.Destroy(glow.GetComponent<Collider>());
            glow.transform.SetParent(c.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            glow.GetComponent<MeshRenderer>().sharedMaterial = CandleGlow.Material;
            glow.AddComponent<CandleGlow>();
            candles.Add(c);
        }

        /// <summary>Phong toi 60% (Art Bible muc 4), nen sang quanh bep va quay; giau mau bac (Dieu 4).</summary>
        void RefreshBlackout()
        {
            GameSession s = g.Session;
            bool dark = s.IsBlackout && (s.Phase == GamePhase.Prep || s.Phase == GamePhase.Open);
            ItemView.HideTier = s.IsBlackout;
            if (dark == blackoutShown) return;
            blackoutShown = dark;
            if (mainLight != null && mainIntensity >= 0f) mainLight.intensity = mainIntensity * (dark ? 1f - s.Balance.blackout.dim : 1f);
            foreach (GameObject c in candles) c.SetActive(dark);
            // Ve lai nguyen lieu de an / hien mau bac.
            foreach (CrateView cv in crates.Values) if (cv != null) Object.Destroy(cv.gameObject);
            crates.Clear();
            items.Clear();
        }
    }

    /// <summary>Mot cum khoi bay len, mo dan, lap lai (qua lua; Art Bible muc 11 dung anh hat "khoi").</summary>
    public class SmokePuff : MonoBehaviour
    {
        float t;
        Vector3 basePos;

        public void Init(float phase, Vector3 offset)
        {
            t = phase;
            basePos = offset;
        }

        void Update()
        {
            t += Time.deltaTime * 0.8f;
            float k = t % 1f;
            transform.localPosition = basePos + new Vector3(0.05f * Mathf.Sin(t * 5f), 0.6f * k, 0f);
            transform.localScale = Vector3.one * (0.12f + 0.2f * k);
            if (FaceBillboard.View != null) transform.rotation = FaceBillboard.View.transform.rotation;
        }
    }

    /// <summary>Quang nen: anh tron am khong chiu anh sang, nhap nhay nhe, luon quay ve nguoi choi.</summary>
    public class CandleGlow : MonoBehaviour
    {
        static Material material;
        float seed;

        public static Material Material
        {
            get
            {
                if (material != null) return material;
                const int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "T_QuangNen", wrapMode = TextureWrapMode.Clamp };
                var px = new Color32[size * size];
                float half = size * 0.5f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float d = new Vector2(x + 0.5f - half, (y + 0.5f - half) * 0.8f).magnitude / half;
                        float core = Mathf.Clamp01(1f - d * 4f);
                        float halo = Mathf.Clamp01(1f - d) * 0.55f;
                        Color c = Color.Lerp(new Color(1f, 0.78f, 0.35f), new Color(1f, 0.97f, 0.85f), core);
                        c.a = Mathf.Clamp01(halo + core);
                        px[y * size + x] = c;
                    }
                tex.SetPixels32(px);
                tex.Apply();
                material = new Material(Shader.Find("BepNhanPham/UnlitAlpha")) { name = "M_QuangNen" };
                material.SetTexture("_MainTex", tex);
                material.SetColor("_Color", Color.white);
                return material;
            }
        }

        void Start()
        {
            seed = Random.value * 10f;
        }

        void Update()
        {
            float s = 0.32f * (1f + 0.08f * Mathf.Sin(Time.time * 9f + seed) + 0.05f * Mathf.Sin(Time.time * 23f + seed));
            transform.localScale = new Vector3(s, s * 1.2f, 1f);
            if (FaceBillboard.View != null) transform.rotation = FaceBillboard.View.transform.rotation;
        }
    }
}
