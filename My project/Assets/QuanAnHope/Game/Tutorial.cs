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
            /// <summary>Sprint 10 B-08: tranh cua buoc (GuidePics, khuon A) va tranh khai niem di kem (khuon C); null = khong co.</summary>
            public string pic, concept;
        }

        public class Lesson
        {
            public string id;
            public Func<bool> when;
            public readonly List<Step> steps = new List<Step>();
            /// <summary>Bai cua mot ngay: het ngay thi dung.</summary>
            public int day;
        }

        public const string Day1 = "ngay1", Night = "troi_toi", Relic = "ngay2_relic", Round2 = "vong2", Blackout = "mat_dien", Boss = "chu_no",
            CloseShop = "dong_quan", Sauce = "nuoc_sot", BossHint = "goi_y_mon_cuoi";

        readonly GameController g;
        readonly List<Lesson> lessons = new List<Lesson>();
        readonly HashSet<string> finished = new HashSet<string>();
        Lesson lesson;
        GameObject arrow;
        float stepShown;
        int tabOpensAtStart, shopViewsAtStart, menuViewsAtStart, relicsAtStart, saucesUsedAtStart;

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
                if (st.note != null) return st.note();
                return st.concept != null ? GuidePics.Caption(st.concept) : null;
            }
        }

        /// <summary>Tranh cua buoc dang hien (B-08); null neu khong co.</summary>
        public string Pic { get { return Enabled && lesson != null && !Finished ? lesson.steps[Index].pic : null; } }

        /// <summary>Tranh khai niem di kem buoc dang hien (B-08); null neu khong co.</summary>
        public string Concept { get { return Enabled && lesson != null && !Finished ? lesson.steps[Index].concept : null; } }
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
                return (Index + 1) + "/" + lesson.steps.Count + " · " + lesson.steps[Index].text;
            }
        }

        float finishedLeft;
        string finishedText;

        public Tutorial(GameController controller)
        {
            g = controller;
            GameSession S() { return g.Session; }

            // Sprint 10 B-08 (yeu cau 22/11/2026, "Phong cach huong dan bang hinh.md"): moi buoc MOT hanh dong, chu <= 4 tu, tranh
            // (khuon A) + tranh khai niem (khuon C); khong goi ten goc, khong day so dai - so nam trong tranh, sat vat.

            // ---------- ngay 1 ----------
            Lesson d1 = NewLesson(Day1, () => S().Day == 1, 1);
            Add(d1, "Nhét vé", () => Station("Kiosk"),
                () => S().Stock.Count > 0 || S().Stove.State != StoveState.Empty || S().Counter.Dishes.Count > 0, 0f, GuidePics.Kiosk);
            Add(d1, "Lấy 1 nguyên liệu", FirstCrateItem,
                () => g.Hand == HandKind.Ingredient || g.OnBoard != null || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0, 0f,
                GuidePics.Take, GuidePics.Tiers);
            Add(d1, "Sơ chế trên thớt", () => Station("Board"),
                () => (g.OnBoard != null && g.OnBoard.prepared) || (g.HeldIngredient != null && g.HeldIngredient.prepared)
                    || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0, 0f, GuidePics.Prep);
            Add(d1, "Tab: xem thực đơn", null,
                () => g.TabMenuViews > menuViewsAtStart || S().Stove.Tool != null || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0, 0f,
                GuidePics.Menu, GuidePics.Price);
            Add(d1, "Chọn dụng cụ", ToolForStock,
                () => S().Stove.Tool != null || S().Counter.Dishes.Count > 0, 0f, GuidePics.Tool);
            Add(d1, "Cho vào đúng thứ tự", () => Station("Stove"),
                () => S().Stove.Added.Count > 0 || S().Stove.State == StoveState.Cooking || S().Stove.State == StoveState.Ready || S().Counter.Dishes.Count > 0, 0f,
                GuidePics.Order, GuidePics.DishTier);
            Add(d1, "Vặn núm", () => Station("Knob"),
                () => S().Stove.State == StoveState.Cooking || S().Stove.State == StoveState.Ready || S().Counter.Dishes.Count > 0 || g.Hand == HandKind.Dish, 0f,
                GuidePics.Knob);
            Add(d1, "Múc ra đĩa", () => S().Stove.State == StoveState.Ready && g.Hand == HandKind.Plate ? Station("Stove") : Station("PlateStack"),
                () => S().Counter.Dishes.Count > 0 || g.Hand == HandKind.Dish || S().ServedToday > 0, 0f, GuidePics.Plate);
            Add(d1, "Mở quán", () => Station("ShopSign"),
                () => S().Phase == GamePhase.Open || S().ServedToday > 0, 0f, GuidePics.Open, GuidePics.Easy);
            Add(d1, "Đặt trước mặt khách", () => g.Hand == HandKind.Dish ? Station("Table_0") : FirstCounterDish(),
                () => S().ServedToday > 0, 0f, GuidePics.Serve, GuidePics.WrongRight);

            // ---------- 21:00 het khach: dong quan (constitution 2.0 muc 6.2) ----------
            Lesson close = NewLesson(CloseShop, () => S().ReadyToClose, 0);
            Add(close, "Đóng quán", () => Station("ShopSign"), () => S().Phase != GamePhase.Open, 0f, GuidePics.Close);

            // ---------- troi toi: bat den (F-076) ----------
            Lesson night = NewLesson(Night, () => S().Phase == GamePhase.Open && !S().IsBlackout && !g.LightsOn
                && RoomView.NightFactor(S().Clock.HourOfDay) > 0.3f, 0);
            Add(night, "Trời tối: bật đèn", () => Station("LightSwitch"), () => g.LightsOn, 0f, GuidePics.Light);

            // ---------- relic (F-077, F-078): yeu cau 23/11/2026 "chuyen phan art tutorial ngay 2 sang dau ngay 3 moi chieu" ----------
            Lesson relic = NewLesson(Relic, () => S().Day == RelicLessonDay && S().Shop.IsOpen && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(relic, "Trả vé: mua relic", () => Station("Relic_1"), () => S().Relics.Owned.Count > relicsAtStart, 20f, GuidePics.BuyRelic);
            Add(relic, "Tab: xem relic", null, () => g.TabOpens > tabOpensAtStart, 0f, GuidePics.TabRelic);

            // ---------- goi y mon cuoi (yeu cau 23/11/2026): tu dau vong 4 de kip chuan bi (dat truoc bai vong 2: chi chay tu vong 4) ----------
            Lesson hint = NewLesson(BossHint, () => S().Round >= 4 && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(hint, "Ngày " + g.Session.Balance.day.bossDay + ": món ngon nhất", null, () => false, 14f, GuidePics.BossBest);

            // ---------- ngay 4: vong 2 (F-078, F-079) ----------
            Lesson r2 = NewLesson(Round2, () => S().Day >= S().Balance.itemShopFromDay && S().Round >= 2
                && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(r2, "Tab: cửa hàng", null, () => g.Tab != null && g.TabShopViews > shopViewsAtStart, 0f, GuidePics.TabShop);
            Add(r2, "Phục vụ ai trước cũng được", () => S().Phase == GamePhase.Open ? Station("Table_1") : Station("ShopSign"), () => S().ServedToday >= 2, 30f,
                GuidePics.Serve, GuidePics.Patience);
            Add(r2, "Khói: múc ngay", () => Station("Stove"), () => false, 9f, GuidePics.Overheat);
            Add(r2, "Cất tủ lạnh", () => Station("Fridge"), () => S().Fridge.Dishes.Count > 0, 12f, GuidePics.Fridge);

            // ---------- nuoc sot (bug 23/11/2026 "khong tim duoc cach su dung nuoc sot") ----------
            Lesson sauce = NewLesson(Sauce, () => S().Items.Sauces > 0 && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(sauce, "Cầm lọ nước sốt", () => Station("Sauce"), () => g.Hand == HandKind.Sauce || SaucesUsed > saucesUsedAtStart, 0f, GuidePics.Sauce,
                GuidePics.SauceLuck);
            Add(sauce, "Bấm đĩa vừa nấu", FirstCounterDish, () => SaucesUsed > saucesUsedAtStart, 40f, GuidePics.Sauce);


            // ---------- ngay mat dien ----------
            Lesson dark = NewLesson(Blackout, () => S().IsBlackout && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(dark, "Mất điện: nấu bằng nến", null, () => false, 10f, GuidePics.Dark);

            // ---------- ngay chu no ----------
            Lesson boss = NewLesson(Boss, () => S().IsBossDay && (S().Phase == GamePhase.Prep || S().Phase == GamePhase.Open), 0);
            Add(boss, g.Session.Balance.day.bossHour + ":00 chủ nợ tới", () => Station("Fridge"), () => S().BossServed, 14f, GuidePics.Boss, GuidePics.BossBest);
        }

        /// <summary>Bai relic chieu dau ngay nay (yeu cau 23/11/2026: ngay 3 thay cho ngay 2).</summary>
        public const int RelicLessonDay = 3;

        int SaucesUsed { get { return g.Session.Items.SaucesBought - g.Session.Items.Sauces; } }

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

        static void Add(Lesson l, string text, Func<Interactable> target, Func<bool> done, float readSeconds = 0f, string pic = null, string concept = null)
        {
            l.steps.Add(new Step { text = text, target = target, done = done, readSeconds = readSeconds, pic = pic, concept = concept });
        }

        /// <summary>Dia dau tien tren quay (buoc phuc vu khi tay chua cam dia).</summary>
        Interactable FirstCounterDish()
        {
            return Station("Dish_0") ?? Station("PlateStack");
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
            menuViewsAtStart = g.TabMenuViews;
            relicsAtStart = g.Session.Relics.Owned.Count;
            saucesUsedAtStart = SaucesUsed;
        }

        void EndLesson(bool completed)
        {
            if (lesson == null) return;
            finished.Add(lesson.id);
            if (completed)
            {
                finishedLeft = 6f;
                finishedText = lesson.id == Day1 ? "Xong! Gom tiền trả nợ ngày " + g.Session.Balance.NextDebtAfter(0).day + "." : "Xong!";
            }
            lesson = null;
            HideArrow();
        }

        /// <summary>Moi khung hinh khi dang choi.</summary>
        public void Tick(float dt)
        {
            if (!Enabled) return;

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
