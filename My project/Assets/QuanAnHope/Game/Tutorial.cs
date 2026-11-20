using System;
using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Huong dan ngay 1 (F-053, constitution 1.5 muc 2.1): mot dong chi dan tren HUD + mui ten noi tren do vat ke tiep can bam.
    /// Moi buoc xong khi trang thai game cho thay nguoi choi da lam (khong chan viec khac, khong goi tat vao luat);
    /// buoc sau da xong san (nguoi choi lam truoc) thi bo qua luon.
    /// </summary>
    public class Tutorial
    {
        public class Step
        {
            public string text;
            public Func<Interactable> target;
            public Func<bool> done;
        }

        readonly GameController g;
        readonly List<Step> steps = new List<Step>();
        GameObject arrow;
        bool lookedAtBook;

        public bool Active { get; private set; }
        public int Index { get; private set; }
        public int Count { get { return steps.Count; } }
        public bool Finished { get { return Index >= steps.Count; } }
        public Step Current { get { return Active && !Finished ? steps[Index] : null; } }
        public GameObject Arrow { get { return arrow; } }

        /// <summary>Dong chu tren HUD; null khi khong co huong dan.</summary>
        public string Line
        {
            get
            {
                if (!Active) return null;
                if (Finished) return finishedLeft > 0f ? "Hướng dẫn xong! Phục vụ thêm khách và gom tiền trả nợ ngày 4." : null;
                return "Hướng dẫn " + (Index + 1) + "/" + steps.Count + " · " + steps[Index].text;
            }
        }

        float finishedLeft;

        public Tutorial(GameController controller)
        {
            g = controller;
            GameSession S() { return g.Session; }
            Add("Đi tới máy nhét vé ở Góc 2 (bên phải quầy) và bấm để quay nguyên liệu.",
                () => Station("Kiosk"),
                () => S().Stock.Count > 0 || S().Stove.State != StoveState.Empty || S().Counter.Dishes.Count > 0);
            Add("Lấy một nguyên liệu trong thùng vừa rơi xuống.",
                FirstCrateItem,
                () => g.Hand == HandKind.Ingredient || g.OnBoard != null || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0);
            Add("Đặt lên thớt ở Góc 4 rồi giữ chuột trái tới khi thanh đầy (sơ chế).",
                () => Station("Board"),
                () => (g.OnBoard != null && g.OnBoard.prepared) || (g.HeldIngredient != null && g.HeldIngredient.prepared)
                    || S().Stove.Added.Count > 0 || S().Counter.Dishes.Count > 0);
            Add("Chọn dụng cụ trên giá ở Góc 4 (chảo, nồi hoặc vỉ nướng: xem món trong sách).",
                ToolForStock,
                () => S().Stove.Tool != null || S().Counter.Dishes.Count > 0);
            Add("Nhìn quyển sách ở Góc 3: các bước của món và giá theo bậc.",
                () => Station("BookNext"),
                () => lookedAtBook || S().Stove.Added.Count > 0 || S().Stove.State == StoveState.Cooking || S().Counter.Dishes.Count > 0);
            Add("Cho nguyên liệu đã sơ chế và gia vị vào bếp đúng thứ tự trong sách.",
                () => Station("Stove"),
                () => S().Stove.Added.Count > 0 || S().Stove.State == StoveState.Cooking || S().Stove.State == StoveState.Ready || S().Counter.Dishes.Count > 0);
            Add("Vặn núm bếp để bắt đầu nấu.",
                () => Station("Knob"),
                () => S().Stove.State == StoveState.Cooking || S().Stove.State == StoveState.Ready || S().Counter.Dishes.Count > 0 || g.Hand == HandKind.Dish);
            Add("Món chín: lấy đĩa trống ở chồng đĩa rồi bấm vào bếp để múc.",
                () => S().Stove.State == StoveState.Ready && g.Hand == HandKind.Plate ? Station("Stove") : Station("PlateStack"),
                () => S().Counter.Dishes.Count > 0 || g.Hand == HandKind.Dish || S().ServedToday > 0);
            Add("Bấm biển Mở quán ở Góc 1 để đón khách.",
                () => Station("ShopSign"),
                () => S().Phase == GamePhase.Open || S().ServedToday > 0);
            Add("Cầm đĩa đúng món khách gọi tới khách (hoặc bấm chuông) để phục vụ.",
                () => g.Hand == HandKind.Dish ? Station("Customer_0") : Station("Bell"),
                () => S().ServedToday > 0);
        }

        void Add(string text, Func<Interactable> target, Func<bool> done)
        {
            steps.Add(new Step { text = text, target = target, done = done });
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

        /// <summary>Bat dau (van dau, ngay 1) hoac tat huong dan.</summary>
        public void Begin(bool on)
        {
            Active = on;
            Index = 0;
            lookedAtBook = false;
            finishedLeft = 0f;
            if (!on) HideArrow();
        }

        public void Stop()
        {
            Active = false;
            HideArrow();
        }

        /// <summary>Moi khung hinh khi dang choi.</summary>
        public void Tick(float dt)
        {
            if (!Active) return;
            if (g.Session.Day != 1 || g.Session.Phase == GamePhase.DayEnd)
            {
                // Huong dan chi cho ngay 1 (F-053 AC-1).
                Stop();
                g.OnTutorialChanged();
                return;
            }
            if (g.Player != null && g.Player.Focus != null && (g.Player.Focus.Id == "BookNext" || g.Player.Focus.Id == "BookPrev")) lookedAtBook = true;

            int before = Index;
            while (!Finished && steps[Index].done()) Index++;
            if (Finished)
            {
                if (before != Index) finishedLeft = 6f;
                finishedLeft -= dt;
                HideArrow();
                if (finishedLeft <= 0f) Active = false;
                if (before != Index || !Active) g.OnTutorialChanged();
                return;
            }
            if (before != Index) g.OnTutorialChanged();
            PlaceArrow(steps[Index].target(), dt);
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
