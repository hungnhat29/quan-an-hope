using System;
using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Huong dan (F-053, constitution 1.5 muc 2.1; sprint 8 F-081 "tong hop lai tat ca de bo sung them phan tutorial"):
    /// mot dong chi dan tren HUD + mui ten noi tren do vat ke tiep can bam. Gom nhieu bai hoc, moi bai hien mot lan moi van khi
    /// huong dan dang bat: ngay 1 (10 buoc), troi toi (bat den), ngay 2 (relic mien phi, doi hang, Tab), ngay 4 (Tab cua hang,
    /// nhieu khach + kien nhan, qua lua, tu lanh), ngay mat dien, ngay chu no. Moi buoc xong khi trang thai game cho thay nguoi
    /// choi da lam (khong chan viec khac); buoc chi de doc thi tu sang sau vai giay.
    /// </summary>
    public class Tutorial
    {
        public class Step
        {
            public string text;
            public Func<Interactable> target;
            public Func<bool> done;
            /// <summary>&gt; 0: buoc chi de doc, tu sang sau bay nhieu giay.</summary>
            public float readSeconds;
            /// <summary>Sprint 9 B-07: dong giai thich di kem (ti le, bac, do de tinh...), so lay tu du lieu luc hien.</summary>
            public Func<string> note;
        }

        public class Lesson
        {
            public string id;
            public Func<bool> when;
            public readonly List<Step> steps = new List<Step>();
            /// <summary>Bai cua mot ngay: het ngay thi dung.</summary>
            public int day;
        }

        public const string Day1 = "ngay1", Night = "troi_toi", Relic = "ngay2_relic", Round2 = "vong2", Blackout = "mat_dien", Boss = "chu_no";

        readonly GameController g;
        readonly List<Lesson> lessons = new List<Lesson>();
        readonly HashSet<string> finished = new HashSet<string>();
        Lesson lesson;
        GameObject arrow;
        bool lookedAtBook;
        float stepShown;
        int tabOpensAtStart, shopViewsAtStart;

        /// <summary>Huong dan bat cho van nay (Cai dat / van dau).</summary>
        public bool Enabled { get; private set; }
        /// <summary>Dang hien mot bai hoc.</summary>
        public bool Active { get { return Enabled && (lesson != null || finishedLeft > 0f); } }
        public int Index { get; private set; }
        public int Count { get { return lesson != null ? lesson.steps.Count : 0; } }
        public bool Finished { get { return lesson == null || Index >= lesson.steps.Count; } }
        public Step Current { get { return lesson != null && !Finished ? lesson.steps[Index] : null; } }
        public GameObject Arrow { get { return arrow; } }
        public string LessonId { get { return lesson != null ? lesson.id : null; } }

        /// <summary>Dong giai thich cua buoc dang hien (B-07); null neu buoc khong co.</summary>
        public string Note
        {
            get
            {
                if (!Enabled || lesson == null || Finished) return null;
                Step st = lesson.steps[Index];
                return st.note != null ? st.note() : null;
            }
        }
        public bool HasFinished(string id) { return finished.Contains(id); }
        public IEnumerable<Lesson> Lessons { get { return lessons; } }

        /// <summary>Dong chu tren HUD; null khi khong co huong dan.</summary>
        public string Line
        {
            get
            {
                if (!Enabled) return null;
                if (lesson == null) return finishedLeft > 0f ? finishedText : null;
                if (Finished) return null;
                return "Hướng dẫn " + (Index + 1) + "/" + lesson.steps.Count + " · " + lesson.steps[Index].text;
            }
        }

        float finishedLeft;
        string finishedText;

        public Tutorial(GameController controller)
        {
            g = controller;
            GameSession S() { return g.Session; }

            // ---------- ngay 1 (F-053, giu nguyen 10 buoc) ----------
            Lesson d1 = NewLesson(Day1, () => S().Day == 1, 1);
            Add(d1, "Đi tới máy nhét vé ở Góc 2 (bên phải quầy) và bấm để quay nguyên liệu.",
                () => Station("Kiosk"),
                () => S().Stock.Count > 0 || S().Stove.State != StoveState.Empty || S().Counter.Dishes.Count > 0);
            Explain(d1, () => GuideText.RatesNote(S().Balance, S().Day));
            Add(d1, "Lấy một nguyên liệu trong thùng vừa rơi xuống.",
                FirstCrateItem,
                () => g.Hand == HandKind.Ingredient || g.OnBoard != null || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0);
            Explain(d1, GuideText.IngredientTiersNote);
            Add(d1, "Đặt lên thớt ở Góc 4 rồi giữ chuột trái tới khi thanh đầy (sơ chế: dao thái, trứng gõ thớt…).",
                () => Station("Board"),
                () => (g.OnBoard != null && g.OnBoard.prepared) || (g.HeldIngredient != null && g.HeldIngredient.prepared)
                    || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0);
            Add(d1, "Chọn dụng cụ trên giá ở Góc 4 (chảo, nồi hoặc vỉ nướng: xem món trong sách).",
                ToolForStock,
                () => S().Stove.Tool != null || S().Counter.Dishes.Count > 0);
            Add(d1, "Nhìn quyển sách ở Góc 3: các bước của món và giá theo bậc.",
                () => Station("BookNext"),
                () => lookedAtBook || S().Stove.Added.Count > 0 || S().Stove.State == StoveState.Cooking || S().Counter.Dishes.Count > 0);
            Explain(d1, GuideText.BookNote);
            Add(d1, "Cho nguyên liệu đã sơ chế và gia vị vào bếp đúng thứ tự trong sách.",
                () => Station("Stove"),
                () => S().Stove.Added.Count > 0 || S().Stove.State == StoveState.Cooking || S().Stove.State == StoveState.Ready || S().Counter.Dishes.Count > 0);
            Explain(d1, () => GuideText.DishNote(S().Balance));
            Add(d1, "Vặn núm bếp để bắt đầu nấu.",
                () => Station("Knob"),
                () => S().Stove.State == StoveState.Cooking || S().Stove.State == StoveState.Ready || S().Counter.Dishes.Count > 0 || g.Hand == HandKind.Dish);
            Explain(d1, () => GuideText.DishNote(S().Balance));
            Add(d1, "Món chín: lấy đĩa trống ở chồng đĩa rồi bấm vào bếp để múc.",
                () => S().Stove.State == StoveState.Ready && g.Hand == HandKind.Plate ? Station("Stove") : Station("PlateStack"),
                () => S().Counter.Dishes.Count > 0 || g.Hand == HandKind.Dish || S().ServedToday > 0);
            Add(d1, "Bấm biển Mở quán ở Góc 1 để đón khách.",
                () => Station("ShopSign"),
                () => S().Phase == GamePhase.Open || S().ServedToday > 0);
            Explain(d1, () => GuideText.EasinessNote(S().Balance));
            Add(d1, "Cầm đĩa đúng món khách gọi tới khách (hoặc bấm chuông) để phục vụ.",
                () => g.Hand == HandKind.Dish ? Station("Customer_0") : Station("Bell"),
                () => S().ServedToday > 0);
            Explain(d1, () => GuideText.EasinessNote(S().Balance) + " " + GuideText.PaymentNote(S().Balance));

            // ---------- troi toi: bat den (F-076) ----------
            Lesson night = NewLesson(Night, () => S().Phase == GamePhase.Open && !S().IsBlackout && !g.LightsOn
                && RoomView.NightFactor(S().Clock.HourOfDay) > 0.3f, 0);
            Add(night, "Trời tối rồi: bấm công tắc đèn trên tường cạnh cửa Góc 1 (đầu mỗi ngày đèn tắt).",
                () => Station("LightSwitch"), () => g.LightsOn);

            // ---------- ngay 2: relic (F-077, F-078) ----------
            Lesson relic = NewLesson(Relic, () => S().Shop.IsFreeDay && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(relic, "Hôm nay relic miễn phí: tới Góc 3, chọn 1 trong 3 relic (chỉ lấy được 1). Không ưng thì bấm nút Đổi hàng ("
                + "tốn tiền) để ra 3 relic khác.",
                () => Station("Relic_1"), () => S().Shop.FreeDone || S().Relics.Owned.Count > 0);
            Add(relic, "Nhấn Tab để xem relic đang có và hiệu ứng; giá đầy thì bỏ bớt ở đây.",
                null, () => g.TabOpens > tabOpensAtStart);
            Add(relic, "Từ ngày 3 relic trả bằng vé trắng; vé không dùng được cộng dồn sang hôm sau.", null, () => false, 8f);

            // ---------- ngay 4: vong 2 (F-078, F-079) ----------
            Lesson r2 = NewLesson(Round2, () => S().Day >= S().Balance.itemShopFromDay && S().Round >= 2
                && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(r2, "Cửa hàng mở: nhấn Tab rồi chọn Cửa hàng để mua nước sốt thần thánh, vé đen, máy gacha đồ ăn.",
                null, () => g.Tab != null && g.TabShopViews > shopViewsAtStart);
            Add(r2, "Vòng 2: tối đa 3 khách, vào cách nhau vài chục giây. Thanh dưới bong bóng là kiên nhẫn: chờ quá lâu khách khó tính hơn.",
                () => S().Phase == GamePhase.Open ? Station("Customer_1") : Station("ShopSign"), () => S().ServedToday >= 2, 30f);
            Explain(r2, () => GuideText.PatienceNote(S().Balance, S().Day) + " Nhấn Tab → Cách chơi để xem lại mọi bảng.");
            Add(r2, "Bếp bốc khói là sắp quá lửa: múc ngay, để quá lâu món bị trừ 1 bậc.", () => Station("Stove"), () => false, 9f);
            Add(r2, "Tủ lạnh trên tường Góc 4 giữ món qua đêm: cầm đĩa rồi bấm tủ (để dành món ngon cho khách khó tính).",
                () => Station("Fridge"), () => S().Fridge.Dishes.Count > 0, 12f);

            // ---------- ngay mat dien ----------
            Lesson dark = NewLesson(Blackout, () => S().IsBlackout && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(dark, "Mất điện: nến sáng quanh bếp và quầy, công tắc đèn không dùng được, không thấy bậc món và nguyên liệu.", null, () => false, 10f);

            // ---------- ngay chu no ----------
            Lesson boss = NewLesson(Boss, () => S().IsBossDay && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(boss, "Đúng " + g.Session.Balance.day.bossHour + ":00 chủ nợ tới và chiếm một ghế: phục vụ món Tuyệt hảo trở lên (để sẵn trong tủ lạnh) để nhận thưởng.",
                () => Station("Fridge"), () => S().BossServed, 14f);
        }

        Lesson NewLesson(string id, Func<bool> when, int day)
        {
            var l = new Lesson { id = id, when = when, day = day };
            lessons.Add(l);
            return l;
        }

        /// <summary>Gan dong giai thich cho buoc vua them (B-07 AC-1).</summary>
        static void Explain(Lesson l, Func<string> note)
        {
            if (l.steps.Count > 0) l.steps[l.steps.Count - 1].note = note;
        }

        static void Add(Lesson l, string text, Func<Interactable> target, Func<bool> done, float readSeconds = 0f)
        {
            l.steps.Add(new Step { text = text, target = target, done = done, readSeconds = readSeconds });
        }

        Interactable Station(string id)
        {
            return g.Room != null ? g.Room.Station(id) : null;
        }

        Interactable FirstCrateItem()
        {
            if (g.Room == null) return null;
            foreach (Ingredient i in g.Session.Stock.Items)
            {
                ItemView v = g.Room.ItemOf(i);
                if (v != null && v.gameObject.activeInHierarchy) return v.GetComponent<Interactable>();
            }
            return Station("Kiosk");
        }

        /// <summary>Dung cu cua mon dau tien lam duoc tu kho (khach chua goi thi lay cap chinh + phu dau tien).</summary>
        Interactable ToolForStock()
        {
            List<Recipe> can = g.Session.MakeableRecipes();
            string tool = can.Count > 0 ? can[0].tool : RoomView.Tools[0];
            return Station("Tool_" + tool);
        }

        /// <summary>Bat (van dau, hoac Cai dat bat) hoac tat huong dan cho ca van.</summary>
        public void Begin(bool on)
        {
            Enabled = on;
            finished.Clear();
            lesson = null;
            Index = 0;
            lookedAtBook = false;
            finishedLeft = 0f;
            if (!on) HideArrow();
        }

        public void Stop()
        {
            Enabled = false;
            lesson = null;
            finishedLeft = 0f;
            HideArrow();
        }

        void StartLesson(Lesson l)
        {
            lesson = l;
            Index = 0;
            stepShown = 0f;
            finishedLeft = 0f;
            tabOpensAtStart = g.TabOpens;
            shopViewsAtStart = g.TabShopViews;
        }

        void EndLesson(bool completed)
        {
            if (lesson == null) return;
            finished.Add(lesson.id);
            if (completed)
            {
                finishedLeft = 6f;
                finishedText = lesson.id == Day1 ? "Hướng dẫn xong! Phục vụ thêm khách và gom tiền trả nợ ngày 4." : "Xong phần hướng dẫn này.";
            }
            lesson = null;
            HideArrow();
        }

        /// <summary>Moi khung hinh khi dang choi.</summary>
        public void Tick(float dt)
        {
            if (!Enabled) return;
            if (g.Player != null && g.Player.Focus != null && (g.Player.Focus.Id == "BookNext" || g.Player.Focus.Id == "BookPrev")) lookedAtBook = true;

            if (lesson != null && lesson.day > 0 && (g.Session.Day != lesson.day || g.Session.Phase == GamePhase.DayEnd))
            {
                // bai cua mot ngay (ngay 1, F-053 AC-1): het ngay thi dung
                EndLesson(false);
                g.OnTutorialChanged();
                return;
            }
            if (lesson == null)
            {
                if (finishedLeft > 0f)
                {
                    finishedLeft -= dt;
                    if (finishedLeft <= 0f) g.OnTutorialChanged();
                    return;
                }
                foreach (Lesson l in lessons)
                    if (!finished.Contains(l.id) && l.when())
                    {
                        StartLesson(l);
                        g.OnTutorialChanged();
                        break;
                    }
                if (lesson == null) return;
            }

            int before = Index;
            stepShown += dt;
            while (!Finished)
            {
                Step st = lesson.steps[Index];
                if (!(st.done() || (st.readSeconds > 0f && stepShown >= st.readSeconds))) break;
                Index++;
                stepShown = 0f;
            }
            if (Finished)
            {
                EndLesson(true);
                g.OnTutorialChanged();
                return;
            }
            if (before != Index) g.OnTutorialChanged();
            Step cur = lesson.steps[Index];
            PlaceArrow(cur.target != null ? cur.target() : null, dt);
        }

        // ---------------- mui ten ----------------

        float bob;

        void PlaceArrow(Interactable target, float dt)
        {
            if (target == null || !target.isActiveAndEnabled) { HideArrow(); return; }
            if (arrow == null) arrow = BuildArrow();
            arrow.SetActive(true);
            bob += dt;
            Bounds b = target.Box != null ? target.Box.bounds : new Bounds(target.WorldCenter, Vector3.one * 0.2f);
            arrow.transform.position = new Vector3(b.center.x, b.max.y + 0.22f + Mathf.Sin(bob * 4f) * 0.05f, b.center.z);
            arrow.transform.rotation = Quaternion.Euler(0f, bob * 90f, 0f);
        }

        void HideArrow()
        {
            if (arrow != null) arrow.SetActive(false);
        }

        /// <summary>Mui ten vang chi xuong (Art Bible muc 3: mustard #F2C14E, vien muc).</summary>
        static GameObject BuildArrow()
        {
            var go = new GameObject("MuiTenHuongDan");
            Toon.Prim(PrimitiveType.Cylinder, go.transform, "Than", new Vector3(0f, 0.15f, 0f), new Vector3(0.05f, 0.18f, 0.05f), UiKit.Mustard, Toon.OutlineSmall);
            // dau mui ten: khoi vuong xoay 45 do, ep det -> hinh thoi chi xuong
            Toon.Prim(PrimitiveType.Cube, go.transform, "Dau", new Vector3(0f, 0.03f, 0f), new Vector3(0.12f, 0.12f, 0.045f), UiKit.Mustard, Toon.OutlineSmall, new Vector3(0f, 0f, 45f));
            Toon.Prim(PrimitiveType.Cube, go.transform, "Dau2", new Vector3(0f, 0.03f, 0f), new Vector3(0.12f, 0.12f, 0.045f), UiKit.Mustard, Toon.OutlineSmall, new Vector3(0f, 90f, 45f));
            return go;
        }
    }
}
