using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Truoc sprint 10 (yeu cau 22/11/2026, constitution 2.0): nhieu thot + ban so che dai ra (B-15), nguyen lieu tha roi tai cho
    /// (B-16), dat dia len ban truoc mat khach de phuc vu (B-17), khach het thanh cho gian bo di (B-19).
    /// </summary>
    public partial class RoomView
    {
        // =====================================================================
        // B-15: thot (Goc 4, ban trai). Thot 0 la thot co san cua canh; thot 1, 2 hien khi mua, ban dai them theo.
        // =====================================================================

        /// <summary>Tam thot thu i theo khung Goc 4 (thot co san o x = -2,75).</summary>
        public static readonly float[] BoardX = { -2.75f, -3.85f, -4.95f };
        const float BoardY = 1.17f, BoardZ = 3.1f;

        /// <summary>Doan ban noi them khi co thot thu i (x trai, x phai) - ban goc trong canh tu -3,95 toi -1,75.</summary>
        static readonly Vector2[] CounterExtension = { Vector2.zero, new Vector2(-4.45f, -3.95f), new Vector2(-5.5f, -4.45f) };

        public class BoardView
        {
            public Transform Spot;
            public GameObject Item;
            public Ingredient ItemFor;
            public ProgressBar Bar;
            public GameObject BarRoot;
            public PrepFx Fx;
            public GameObject Group;      // thot + doan ban noi them (an khi chua mua)
            public Interactable Hit;
        }

        readonly List<BoardView> boards = new List<BoardView>();
        public IReadOnlyList<BoardView> Boards { get { return boards; } }

        /// <summary>So thot dang hien trong phong.</summary>
        public int BoardsShown
        {
            get { int n = 0; foreach (BoardView b in boards) if (b.Group == null || b.Group.activeSelf) n++; return n; }
        }

        void BuildBoards()
        {
            // thot 0: do vat cua BuildKitchen
            boards.Add(new BoardView { Spot = boardSpot, Bar = boardBar, BarRoot = boardBarRoot, Fx = prepFx, Hit = Station("Board") });
            for (int i = 1; i < BoardX.Length; i++)
            {
                int index = i;
                var group = Toon.Empty(goc4, "ThotThem_" + i, Vector3.zero);
                Vector2 ext = CounterExtension[i];
                float w = ext.y - ext.x, cx = (ext.x + ext.y) * 0.5f;
                GameObject body = Toon.Prim(PrimitiveType.Cube, group.transform, "BanNoi", new Vector3(cx, 0.5f, 3.3f), new Vector3(w, 1f, 1f),
                    UiKit.Hex("#B97F52"), Toon.OutlineBig);
                Toon.Prim(PrimitiveType.Cube, group.transform, "MatBanNoi", new Vector3(cx, 1.05f, 3.3f), new Vector3(w + 0.02f, 0.1f, 1.08f),
                    UiKit.Hex("#D69B68"), Toon.OutlineSmall);
                // va cham cho doan ban (BuildColliders chi chay mot lan luc dung phong)
                var col = body.AddComponent<BoxCollider>();
                col.size = new Vector3(1f, 2f, 1f);
                col.center = new Vector3(0f, 0.5f, 0f);
                Transform spot = Toon.Empty(group.transform, "ViTriThot_" + i, new Vector3(BoardX[i], BoardY, BoardZ)).transform;
                GameObject model = Toon.Empty(spot, "ThotModel", new Vector3(0f, -0.07f, 0f));
                if (ModelLibrary.TryInstantiate("Kitchen", "SM_Thot", model.transform, Toon.OutlineSmall) == null)
                    Toon.Prim(PrimitiveType.Cube, model.transform, "Thot", new Vector3(0f, 0.03f, 0f), new Vector3(1f, 0.06f, 0.6f), UiKit.Hex("#F2D7B6"), Toon.OutlineSmall);
                Interactable hit = Register(Interactable.Add(spot.gameObject, GameController.BoardId(i), new Vector3(0f, 0.1f, 0f), new Vector3(1f, 0.3f, 0.6f),
                    () => BoardPrompt(index), () => g.UseBoard(index), dt => g.HoldBoard(dt, index)));
                Canvas bar = UiKit.WorldCanvas(group.transform, "TienDoSoChe_" + i, new Vector3(BoardX[i], 1.62f, 3.0f), Quaternion.Euler(20f, 0f, 0f), new Vector2(700f, 110f));
                ProgressBar pb = ProgressBar.Create(bar.transform, "Thanh", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 110f), UiKit.Mint);
                boards.Add(new BoardView { Spot = spot, Bar = pb, BarRoot = bar.gameObject, Fx = PrepFx.Create(spot), Group = group, Hit = hit });
                group.SetActive(false);
            }
        }

        string BoardPrompt(int board)
        {
            Ingredient on = g.OnBoardAt(board);
            if (on == null)
                return g.Hand == HandKind.Ingredient && !g.HeldIngredient.prepared ? "Đặt " + g.HeldIngredient.name + " lên thớt" : "Thớt";
            return on.prepared ? "Cầm " + on.name : "Giữ chuột trái: sơ chế " + on.name;
        }

        void RefreshBoards()
        {
            int owned = g.Session.Boards.Count;
            bool grew = false;
            for (int i = 0; i < boards.Count; i++)
            {
                BoardView b = boards[i];
                if (b.Group != null && b.Group.activeSelf != (i < owned))
                {
                    b.Group.SetActive(i < owned);
                    if (i < owned) { Pop.Play(b.Spot.gameObject, 0.6f); grew = true; }
                }
                Ingredient on = g.OnBoardAt(i);
                if (!ReferenceEquals(b.ItemFor, on) || (b.Item != null && on != null))
                {
                    if (b.Item != null) Object.Destroy(b.Item);
                    b.Item = null;
                    b.ItemFor = on;
                    if (on != null)
                    {
                        ItemView v = ItemView.Create(b.Spot, on, 2.2f, false);
                        v.SetPrepared(on.prepared);
                        b.Item = v.gameObject;
                    }
                }
                if (b.BarRoot != null) b.BarRoot.SetActive(on != null);
            }
            // boardItem / boardItemFor cu (test sprint 2-9, F-075) tro vao thot 0
            boardItem = boards[0].Item;
            boardItemFor = boards[0].ItemFor;
            if (grew) Physics.SyncTransforms();
        }

        void TickBoards(float dt)
        {
            for (int i = 0; i < boards.Count; i++)
            {
                BoardView b = boards[i];
                Ingredient on = g.OnBoardAt(i);
                float progress = on == null ? 0f : on.prepared ? 1f : i < g.Session.Boards.Count ? (float)g.Session.Boards[i].Progress : 0f;
                if (b.Bar != null) b.Bar.Set(progress + (on != null && on.prepared ? 1f : 0f));
                if (b.Fx != null)
                {
                    ItemView v = b.Item != null ? b.Item.GetComponent<ItemView>() : null;
                    b.Fx.Tick(on, v, progress, g.PrepHeldRecently && g.LastPrepBoard == i, dt);
                }
            }
        }

        // =====================================================================
        // B-16: nguyen lieu tha bang chuot phai roi tai cho
        // =====================================================================

        readonly Dictionary<Ingredient, ItemView> droppedViews = new Dictionary<Ingredient, ItemView>();
        readonly Dictionary<Ingredient, Vector3> dropFrom = new Dictionary<Ingredient, Vector3>();
        Transform droppedRoot;

        /// <summary>Do vat 3D cua nguyen lieu da tha (null neu khong co).</summary>
        public ItemView DroppedView(Ingredient ing)
        {
            ItemView v;
            return droppedViews.TryGetValue(ing, out v) ? v : null;
        }

        /// <summary>Goi truoc Refresh: nguyen lieu bay tu tay ra roi xuong diem tha.</summary>
        public void DropFrom(Ingredient ing, Vector3 worldFrom)
        {
            dropFrom[ing] = worldFrom;
        }

        /// <summary>
        /// Mat do vat cao nhat duoi diem tam ngam (mat ban, mat quay, san): vat chan cua phong cao 2 m nen khong dung diem trung
        /// cua tia nhin; lay hinh that (Renderer.bounds, L-016) chua diem (x, z) va thap hon tam mat.
        /// </summary>
        public Vector3 SurfaceAt(Vector3 hit)
        {
            float top = 0f;
            float limit = Mathf.Min(hit.y + 0.05f, 1.6f);
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>())
            {
                if (!r.enabled || r.gameObject.name == "Outline" || r.GetComponentInParent<ItemView>() != null) continue;
                Bounds b = r.bounds;
                if (b.max.y > limit || b.max.y <= top) continue;
                if (hit.x < b.min.x - 0.02f || hit.x > b.max.x + 0.02f || hit.z < b.min.z - 0.02f || hit.z > b.max.z + 0.02f) continue;
                if (b.size.x < 0.15f && b.size.z < 0.15f) continue;   // vat nho (nhan, vet) khong do duoc
                top = b.max.y;
            }
            return new Vector3(hit.x, top, hit.z);
        }

        void RefreshDropped()
        {
            if (droppedRoot == null) droppedRoot = new GameObject("NguyenLieuDaTha").transform;
            var gone = new List<Ingredient>();
            foreach (Ingredient i in droppedViews.Keys) if (!g.Dropped.ContainsKey(i) || !g.InStock(i)) gone.Add(i);
            foreach (Ingredient i in gone)
            {
                if (droppedViews[i] != null) Object.Destroy(droppedViews[i].gameObject);
                droppedViews.Remove(i);
            }
            foreach (KeyValuePair<Ingredient, Vector3> kv in g.Dropped)
            {
                if (droppedViews.ContainsKey(kv.Key) || !g.InStock(kv.Key)) continue;
                Ingredient ing = kv.Key;
                ItemView v = ItemView.Create(droppedRoot, ing, 2.2f, false);
                v.SetPrepared(ing.prepared);
                v.transform.position = kv.Value;
                Interactable.Add(v.gameObject, "Dropped_" + ing.typeId, new Vector3(0f, 0.05f, 0f), new Vector3(0.22f, 0.18f, 0.22f),
                    () => "Nhặt " + ing.name + (ing.prepared ? " đã sơ chế" : ""), () => g.TakeIngredient(ing));
                Vector3 from;
                if (dropFrom.TryGetValue(ing, out from)) { DropFall.Play(v.gameObject, from, kv.Value); dropFrom.Remove(ing); }
                droppedViews[ing] = v;
            }
            Physics.SyncTransforms();
        }

        // =====================================================================
        // B-17: dat dia tren ban truoc mat khach (bo chuong)
        // =====================================================================

        readonly List<Transform> tableSpots = new List<Transform>();

        void BuildTables()
        {
            for (int i = 0; i < seatViews.Length; i++)
            {
                SeatView v = seatViews[i];
                if (v == null || v.Pivot == null) continue;
                int seat = i;
                // Tam lot dia tren mat quay Goc 1 ngay truoc ghe (di theo ghe khi vong 1 chi co mot ghe giua: RefreshTables).
                GameObject spot = Toon.Empty(goc1, "ChoDatDia_" + i, new Vector3(SeatX[i], 1.2f, 2.2f));
                Toon.Prim(PrimitiveType.Cylinder, spot.transform, "LotDia", Vector3.zero, new Vector3(0.5f, 0.012f, 0.5f), UiKit.Hex("#E9CE9E"), 0f);
                Register(Interactable.Add(spot, "Table_" + i, new Vector3(0f, 0.12f, 0f), new Vector3(0.7f, 0.3f, 0.6f),
                    () => TablePrompt(seat), () => g.ServeSeat(seat)));
                tableSpots.Add(spot.transform);
            }
        }

        void RefreshTables()
        {
            int n = g.Session.SeatCount;
            for (int i = 0; i < tableSpots.Count; i++)
            {
                SeatView v = seatViews[i];
                tableSpots[i].localPosition = new Vector3(v.Pivot.localPosition.x, 1.2f, 2.2f);
                tableSpots[i].gameObject.SetActive(i < n);
            }
        }

        string TablePrompt(int seat)
        {
            GameSession s = g.Session;
            Customer c = seat < s.SeatCount ? s.Seats[seat] : null;
            if (s.Phase != GamePhase.Open || c == null) return "Bàn trống";
            if (g.Hand != HandKind.Dish) return "Khách gọi " + c.orderName;
            if (c.isBoss) return "Đặt " + g.HeldDish.Title + " cho chủ nợ";
            if (g.HeldDish.recipeId != c.orderId) return "Sai món! Khách gọi " + c.orderName;
            return "Đặt " + g.HeldDish.Title + " trước mặt khách";
        }

        // =====================================================================
        // B-19: khach het thanh cho thi gian bo di
        // =====================================================================

        /// <summary>Goi truoc Refresh khi khach o ghe nay vua bo di: mat gian, rung, roi di ra cua.</summary>
        public void CustomerLeft(int seat)
        {
            if (seat < 0 || seat >= seatViews.Length) return;
            SeatView v = seatViews[seat];
            if (v == null || v.Model == null || !v.Model.activeInHierarchy) return;
            SetFace(v, FaceArt.MoodAngry);
            Lively l = v.Model.GetComponent<Lively>();
            if (l != null) l.Shake();
            if (WalkCustomers && v.Motion != null && v.Arrived != null)
            {
                float[] speeds;
                Vector3[] path = DoorPath(seat, true, CustomerLooks.SideFor(v.Arrived.number, g.Session.Day, true), out speeds);
                SpawnLeaver(v, path, speeds);
            }
            v.Arrived = null;
            v.Bubble.gameObject.SetActive(false);
        }

        // =====================================================================

        /// <summary>Mau lo gia vi tren ke (icon thuc don, tranh huong dan).</summary>
        public bool JarColor(string item, out Color c)
        {
            return jarColors.TryGetValue(item, out c);
        }

        void BuildSprint10()
        {
            BuildBoards();
            BuildTables();
            BuildSprint11();
        }

        void RefreshSprint10()
        {
            RefreshBoards();
            RefreshDropped();
            RefreshTables();
            RefreshSprint11();
        }

        void TickSprint10(float dt)
        {
            TickBoards(dt);
            TickSprint11(dt);
        }
    }

    /// <summary>Nguyen lieu tha: bay tu tay theo duong cong roi nay nhe khi cham mat (0,35 giay).</summary>
    public class DropFall : MonoBehaviour
    {
        Vector3 from, to;
        float t;
        const float Seconds = 0.35f;

        public static DropFall Play(GameObject go, Vector3 from, Vector3 to)
        {
            var d = go.AddComponent<DropFall>();
            d.from = from;
            d.to = to;
            go.transform.position = from;
            return d;
        }

        public bool Done { get { return t >= 1f; } }

        void Update()
        {
            t = Mathf.Min(1f, t + Time.deltaTime / Seconds);
            float y = Mathf.Lerp(from.y, to.y, t * t) + Mathf.Sin(t * Mathf.PI) * 0.15f;
            Vector3 p = Vector3.Lerp(from, to, t);
            transform.position = new Vector3(p.x, y, p.z);
            if (t >= 1f) { transform.position = to; Destroy(this); }
        }
    }
}
