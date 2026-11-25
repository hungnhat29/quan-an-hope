using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Sau sprint 10 (yeu cau 23/11/2026, constitution 2.1): lo nuoc sot ben phai bep (cam len, nho len dia), bep thu hai mua o
    /// Tab (ben phai quay dia, phong 10,8 m con cho tu x = 4,0 toi tuong), thanh ti le du 6 bac theo luoi ve.
    /// </summary>
    public partial class RoomView
    {
        // =====================================================================
        // Thanh ti le 6 bac (constitution 2.1 muc 5.3): >= 1% hai chu so, < 1% du chu so, khong hien 0% cho bac con ra duoc
        // =====================================================================

        static void FillBar(TextMeshProUGUI[] cells, TierChance[] bar)
        {
            var chance = new double[cells.Length];
            var has = new bool[cells.Length];
            foreach (TierChance c in bar)
                if (c.tier >= 0 && c.tier < cells.Length) { chance[c.tier] += c.chance; has[c.tier] = c.chance > 0.0; }
            for (int tier = 0; tier < cells.Length; tier++)
            {
                string pct = has[tier] ? DishFormula.FormatPercent(chance[tier]) : "0%";
                cells[tier].text = "<size=28>" + UiKit.DishTierName[tier] + "</size>\n" + pct;
                cells[tier].alpha = has[tier] && chance[tier] >= 0.01 ? 1f : has[tier] ? 0.8f : 0.4f;
            }
        }

        /// <summary>Chu o thanh ti le bep 0 (test: du 6 bac, dinh dang phan tram).</summary>
        public string BarCellText(int tier) { return tier >= 0 && tier < barCells.Length ? barCells[tier].text : null; }

        // =====================================================================
        // Lo nuoc sot ben phai bep (bug 23/11/2026)
        // =====================================================================

        /// <summary>Cho lo nuoc sot tren mat bep, goc phai sau (khung Goc 4).</summary>
        public static readonly Vector3 SauceSpot = new Vector3(1.12f, 1.1f, 3.62f);

        void MoveSauceToStove()
        {
            if (sauceBottle == null) return;
            sauceBottle.transform.SetParent(goc4, false);
            sauceBottle.transform.localPosition = SauceSpot;
            sauceBottle.transform.localScale = Vector3.one * 1.25f;
            // 24/11/2026: lo dung lai bang Blender (bo canh, vien muc om hinh); thieu file thi giu khoi
            var prims = new List<GameObject>();
            foreach (Transform c in sauceBottle.transform) prims.Add(c.gameObject);
            ModelLibrary.Swap(sauceBottle.transform, prims, "Kitchen", "SM_LoNuocSot", Toon.OutlineSmall);
            if (sauceTagCanvas != null)
            {
                sauceTagCanvas.transform.SetParent(goc4, false);
                sauceTagCanvas.transform.localPosition = SauceSpot + new Vector3(0f, 0.62f, -0.05f);
                sauceTagCanvas.transform.localRotation = Quaternion.Euler(-10f, 0f, 0f);
            }
        }

        string SauceStovePrompt()
        {
            ItemShop it = g.Session.Items;
            if (g.Hand == HandKind.Dish) return "Nhỏ nước sốt lên " + g.HeldDish.name + " (còn " + it.Sauces + " chai)";
            if (g.Hand == HandKind.Sauce) return "Đặt lại nước sốt";
            return "Cầm nước sốt (còn " + it.Sauces + " chai)";
        }

        /// <summary>Lo sot dang hien canh bep (an khi dang cam tren tay).</summary>
        public bool SauceAtStove { get { return sauceBottle != null && sauceBottle.activeInHierarchy && sauceBottle.transform.parent == goc4; } }

        static GameObject SauceHandModel(Transform parent)
        {
            GameObject b = Toon.Empty(parent, "LoNuocSot_Tay", new Vector3(0f, -0.02f, 0f));
            b.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            if (ModelLibrary.TryInstantiate("Kitchen", "SM_LoNuocSot", b.transform, Toon.OutlineSmall) != null) { b.transform.localScale = Vector3.one * 0.72f; return b; }
            Toon.Prim(PrimitiveType.Cylinder, b.transform, "Than", new Vector3(0f, 0.08f, 0f), new Vector3(0.11f, 0.16f, 0.11f), UiKit.Hex("#D9382C"), Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cylinder, b.transform, "Co", new Vector3(0f, 0.18f, 0f), new Vector3(0.05f, 0.05f, 0.05f), UiKit.Hex("#D9382C"), Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Sphere, b.transform, "Nap", new Vector3(0f, 0.22f, 0f), new Vector3(0.06f, 0.04f, 0.06f), UiKit.Mustard, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, b.transform, "Nhan", new Vector3(0f, 0.08f, -0.056f), new Vector3(0.075f, 0.06f, 0.004f), UiKit.Cream, 0f);
            return b;
        }

        /// <summary>Vi tri the gioi cua mot dia (tren quay / trong tu); khong thay thi vi tri tay.</summary>
        public Vector3 DishPosition(Dish d)
        {
            GameSession s = g.Session;
            for (int i = 0; i < s.Counter.Dishes.Count && i < shelfSlots.Length; i++)
                if (ReferenceEquals(s.Counter.Dishes[i], d)) return shelfSlots[i].position + Vector3.up * 0.08f;
            if (s.Fridge.Contains(d) && fridgeRoot != null) return fridgeRoot.position + Vector3.down * 0.2f + fridgeRoot.forward * -0.3f;
            return handModel != null ? handModel.transform.position : handAnchor.position;
        }

        // =====================================================================
        // Bep thu hai (constitution 2.1 muc 8): mua thi hien ngay o ben phai quay dia
        // =====================================================================

        /// <summary>Tam bep thu hai theo khung Goc 4 (quay phai ket thuc o x = 3,95; tuong o x = 5,32).</summary>
        public const float Stove2X = 4.66f;
        const float Stove2W = 1.28f;

        class StoveView
        {
            public GameObject Group;
            public Transform Knob;
            public readonly Dictionary<string, GameObject> Tools = new Dictionary<string, GameObject>();
            public TextMeshProUGUI Steps, Status;
            public readonly TextMeshProUGUI[] Cells = new TextMeshProUGUI[6];
            public GameObject BarRoot;
            public ProgressBar CookBar;
            public Image PanelBg;
            public Transform PanRoot;
            public PanView Pan;
            public readonly List<PanItem> Items = new List<PanItem>();
            public float FxTimer;
        }

        StoveView stove2;

        /// <summary>Bep thu hai dang hien trong phong.</summary>
        public bool Stove2Shown { get { return stove2 != null && stove2.Group.activeSelf; } }

        void BuildStove2()
        {
            var v = new StoveView();
            stove2 = v;
            v.Group = Toon.Empty(goc4, "Bep2", Vector3.zero);
            Transform t = v.Group.transform;
            float x = Stove2X;
            GameObject body = Toon.Prim(PrimitiveType.Cube, t, "ThanBep", new Vector3(x, 0.5f, 3.3f), new Vector3(Stove2W, 1f, 1f), UiKit.Cream, Toon.OutlineBig);
            GameObject bodyCol = Toon.Empty(t, "VaChamBep2", new Vector3(x, 0.5f, 3.3f));
            var col = bodyCol.AddComponent<BoxCollider>();
            col.size = new Vector3(Stove2W, 2f, 1f);
            col.center = new Vector3(0f, 0.5f, 0f);
            var shell = new List<GameObject> { body };
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "MatBep", new Vector3(x, 1.05f, 3.3f), new Vector3(Stove2W + 0.08f, 0.1f, 1.05f), UiKit.Hex("#C9C1B6"), Toon.OutlineBig));
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "CuaLo", new Vector3(x, 0.45f, 2.79f), new Vector3(Stove2W * 0.72f, 0.55f, 0.04f), UiKit.Hex("#E8DFD3"), Toon.OutlineSmall));
            shell.Add(Toon.Prim(PrimitiveType.Cylinder, t, "MiengBep", new Vector3(x, 1.11f, 3.3f), new Vector3(0.95f, 0.02f, 0.95f), UiKit.Ink, 0f));
            // 24/11/2026: than bep dung lai bang Blender (bo canh, vien om hinh)
            ModelLibrary.Swap(Toon.Empty(t, "Bep2Model", new Vector3(x, 0f, 3.3f)).transform, shell, "Machines", "SM_Bep2", Toon.OutlineBig);
            GameObject knob = Toon.Prim(PrimitiveType.Cylinder, t, "NumBep2", new Vector3(x - 0.45f, 0.86f, 2.77f), new Vector3(0.15f, 0.06f, 0.15f), UiKit.Coral,
                Toon.OutlineSmall, new Vector3(-90f, 0f, 0f));
            v.Knob = knob.transform;
            // so "2" tren cua lo
            Canvas num = UiKit.WorldCanvas(t, "SoBep2", new Vector3(x + 0.35f, 0.86f, 2.765f), Quaternion.identity, new Vector2(160f, 160f));
            Image nb = UiKit.Panel(num.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, 150f), UiKit.Mustard, false);
            nb.sprite = UiKit.Circle;
            nb.type = Image.Type.Simple;
            UiKit.Text(nb.transform, "2", 110f, TextAlignmentOptions.Center, 0f);

            foreach (string tool in Tools) v.Tools[tool] = BuildTool(t, tool, new Vector3(x, 1.12f, 3.3f), "Bep2_", 1.6f);

            GameObject hit = Toon.Empty(t, "BepNau2", new Vector3(x, 1.45f, 3.3f));
            Register(Interactable.Add(hit, GameController.StoveId(1), Vector3.zero, new Vector3(1.2f, 0.8f, 1.0f), () => StovePrompt(1), () => g.UseStove(1)));
            GameObject knobHit = Toon.Empty(t, "NumBep2Bam", new Vector3(x - 0.45f, 0.86f, 2.74f));
            Register(Interactable.Add(knobHit, GameController.KnobId(1), Vector3.zero, new Vector3(0.3f, 0.3f, 0.2f),
                () => g.Session.Stoves.Count > 1 && g.Session.Stoves[1].State == StoveState.Cooking ? "Đang nấu" : "Vặn núm bếp 2", () => g.StartCooking(1)));

            // bang nho tren bep 2 (nho hon bang bep 1)
            Vector2 size = new Vector2(1180f, 470f);
            Canvas panel = UiKit.WorldCanvas(t, "BangBep2", new Vector3(x, 2.18f, 2.98f), Quaternion.Euler(-15f, 0f, 0f), size);
            Image pb = UiKit.Panel(panel.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, size, UiKit.Cream);
            pb.raycastTarget = false;
            v.PanelBg = pb;
            v.Steps = UiKit.Label(pb.transform, "ThuTu", new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(size.x - 60f, 170f), "", 56f);
            Vector2 bar = new Vector2(size.x - 60f, 100f);
            Image strip = UiKit.Panel(pb.transform, "ThanhTiLe_Bep2", new Vector2(0.5f, 1f), new Vector2(0f, -196f), bar, UiKit.Ink, false);
            strip.raycastTarget = false;
            v.BarRoot = strip.gameObject;
            float seg = (bar.x - 8f) / v.Cells.Length;
            for (int tier = 0; tier < v.Cells.Length; tier++)
            {
                RectTransform cell = UiKit.Box(strip.transform, "Bar_" + tier, new Vector2(0f, 0.5f), new Vector2(4f + seg * tier + 2f, 0f), new Vector2(seg - 4f, bar.y - 8f));
                var img = cell.gameObject.AddComponent<Image>();
                img.color = UiKit.DishTier[tier];
                img.raycastTarget = false;
                v.Cells[tier] = UiKit.Text(cell, "", 34f, TextAlignmentOptions.Center, 4f);
                v.Cells[tier].lineSpacing = -28f;
            }
            v.Status = UiKit.Label(pb.transform, "TrangThai", new Vector2(0.5f, 0f), new Vector2(0f, 84f), new Vector2(size.x - 60f, 80f), "", 48f);
            v.CookBar = ProgressBar.Create(pb.transform, "DangNau", new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(size.x - 60f, 54f), UiKit.Coral);

            v.PanRoot = Toon.Empty(t, "DoTrongBep2", new Vector3(x, 1.215f, 3.3f)).transform;
            v.Pan = v.PanRoot.gameObject.AddComponent<PanView>();
            v.Group.SetActive(false);
        }

        /// <summary>Tam / ban kinh / ti le do trong dung cu tren bep 2 (dung cu Blender x 1,6 nhu bep 1; noi nho hon noi cua canh).</summary>
        static void PanSpec2(string tool, out Vector3 center, out float radius, out float scale)
        {
            PanSpec(tool, out center, out radius, out scale);
            if (tool == "Nồi") { center = new Vector3(0f, 1.36f, 3.3f); radius = 0.2f; scale = 2f; }
            center.x += Stove2X;
        }

        void PlacePan2(string tool)
        {
            Vector3 center;
            float radius, scale;
            PanSpec2(tool, out center, out radius, out scale);
            stove2.PanRoot.localPosition = center;
            stove2.Pan.Setup(tool, radius, scale, BurnerY - center.y);
            foreach (PanItem p in stove2.Items) if (p != null) p.Retarget(Vector3.zero);
        }

        /// <summary>Bep 2 vua nhan mot thu: do bay tu tay / lo vao dung cu.</summary>
        void OnAddedToStove2(Ingredient ingredient, string pantryItem)
        {
            Stove stove = g.Session.Stoves[1];
            PlacePan2(stove.Tool);
            GameObject go;
            Vector3 from;
            if (ingredient != null)
            {
                go = stove2.Pan.AddIngredient(ingredient);
                from = handModel != null ? handModel.transform.position : handAnchor.position;
            }
            else
            {
                Color c;
                if (!jarColors.TryGetValue(pantryItem, out c)) c = UiKit.Cream;
                go = stove2.Pan.AddPantry(pantryItem, c);
                GameObject jar = Jar(pantryItem);
                from = jar != null ? jar.transform.position + Vector3.down * 0.05f : stove2.PanRoot.position + Vector3.up * 0.5f;
                if (jar != null) jar.GetComponent<Wobble>().Play();
            }
            stove2.Items.Add(PanItem.Launch(go, stove2.PanRoot, from, Vector3.zero));
        }

        string StovePrompt(int si)
        {
            if (si == 0) return StovePrompt();
            if (si >= g.Session.Stoves.Count) return "Bếp 2";
            Stove stove = g.Session.Stoves[si];
            if (g.Hand == HandKind.Ingredient)
                return g.HeldIngredient.prepared ? "Cho " + g.HeldIngredient.name + " vào bếp 2" : "Sơ chế " + g.HeldIngredient.name + " trước";
            if (stove.State == StoveState.Ready) return g.Hand == HandKind.Plate ? "Múc " + stove.Recipe.name + " ra đĩa" : "Lấy đĩa trống để múc";
            if (stove.State == StoveState.Cooking) return "Đang nấu " + stove.Recipe.name;
            return stove.Tool == null ? "Bếp 2 · chọn dụng cụ trên giá" : "Bếp 2 · " + stove.Tool;
        }

        void RefreshStove2()
        {
            GameSession s = g.Session;
            bool owned = s.Stoves.Count > 1;
            if (stove2.Group.activeSelf != owned)
            {
                stove2.Group.SetActive(owned);
                if (owned) { Pop.Play(stove2.Group, 0.6f); Physics.SyncTransforms(); }
                HideDecorUnderStove2(owned);
            }
            if (!owned) return;
            Stove stove = s.Stoves[1];
            foreach (var kv in stove2.Tools) kv.Value.SetActive(stove.Tool == kv.Key);
            // gia dung cu chung: dung cu dang o bep nao thi an tren gia
            foreach (string tool in Tools)
            {
                bool used = false;
                foreach (Stove st in s.Stoves) if (st.Tool == tool) used = true;
                rackTools[tool].SetActive(!used);
            }
            if (stove.Tool == null) stove2.Steps.text = "1 Chọn dụng cụ  ›  2 Cho đúng thứ tự  ›  3 Vặn núm";
            else if (stove.Added.Count == 0) stove2.Steps.text = stove.Tool + " · trống";
            else stove2.Steps.text = stove.Tool + ": " + string.Join(" → ", stove.Added);
            TierChance[] bar = s.IsBlackout ? null : stove.Bar();
            stove2.BarRoot.SetActive(bar != null);
            if (bar != null) FillBar(stove2.Cells, bar);
            if (stove.Added.Count == 0 || stove.Added.Count < stove2.Items.Count)
            {
                foreach (PanItem p in stove2.Items) if (p != null) Object.Destroy(p.gameObject);
                stove2.Items.Clear();
                stove2.Pan.Clear();
            }
            if (stove.Tool != null) PlacePan2(stove.Tool);
            switch (stove.State)
            {
                case StoveState.Cooking: stove2.Status.text = "Đang nấu " + stove.Recipe.name; break;
                case StoveState.Ready: stove2.Status.text = stove.Recipe.name + " đã chín"; break;
                default: stove2.Status.text = bar != null ? "Món sẽ ra bậc nào (nếu đúng thứ tự)" : s.IsBlackout && stove.Bar() != null ? "Mất điện: không nhìn rõ bậc món" : ""; break;
            }
        }

        readonly List<GameObject> stove2Hidden = new List<GameObject>();

        /// <summary>Do trang tri cua canh (chau cay goc phong...) giao voi than bep 2 thi an khi bep 2 hien.</summary>
        void HideDecorUnderStove2(bool owned)
        {
            if (owned && stove2Hidden.Count == 0)
            {
                Bounds body = new Bounds(goc4.TransformPoint(new Vector3(Stove2X, 1.2f, 3.3f)), Vector3.zero);
                body.Encapsulate(goc4.TransformPoint(new Vector3(Stove2X - Stove2W * 0.5f, 0f, 2.75f)));
                body.Encapsulate(goc4.TransformPoint(new Vector3(Stove2X + Stove2W * 0.5f, 2.4f, 3.85f)));
                foreach (Transform corner in new[] { goc1, goc3 })
                {
                    Transform decor = corner != null ? corner.Find("Decor") : null;
                    if (decor == null) continue;
                    foreach (Transform t in decor)
                    {
                        Renderer r = t.GetComponent<Renderer>();
                        if (r != null && t.gameObject.activeSelf && r.bounds.Intersects(body)) stove2Hidden.Add(t.gameObject);
                    }
                }
            }
            foreach (GameObject go in stove2Hidden) if (go != null) go.SetActive(!owned);
        }

        /// <summary>Do trang tri dang an vi bep 2 (test).</summary>
        public int DecorHiddenByStove2 { get { int n = 0; foreach (GameObject go in stove2Hidden) if (go != null && !go.activeSelf) n++; return n; } }

        void TickStove2(float dt)
        {
            GameSession s = g.Session;
            if (s.Stoves.Count < 2 || !stove2.Group.activeSelf) return;
            Stove stove = s.Stoves[1];
            if (stove.State == StoveState.Cooking && stove.CookSeconds > 0f) stove2.CookBar.Set(stove.Elapsed / stove.CookSeconds);
            else stove2.CookBar.Set(stove.State == StoveState.Ready ? 1f : 0f);
            stove2.Knob.localEulerAngles = new Vector3(-90f, stove.State == StoveState.Cooking ? 90f : 0f, 0f);
            bool cooking = stove.State == StoveState.Cooking;
            foreach (PanItem p in stove2.Items) if (p != null) p.Sizzle = cooking;
            stove2.Pan.Tick(stove, stove.Recipe, dt);
            stove2.PanelBg.color = stove.IsBurnt ? UiKit.Coral : stove.IsOverheating ? UiKit.Mustard : UiKit.Cream;
            // hoi nuoc khi nau, khoi khi qua lua (bep 1 dung hinh cua canh)
            stove2.FxTimer -= dt;
            if ((cooking || stove.IsOverheating) && stove2.FxTimer <= 0f)
            {
                stove2.FxTimer = stove.IsOverheating ? 0.7f : 1.4f;
                RiseFx.Play(stove2.Group.transform, new Vector3(Stove2X, 1.55f, 3.25f), stove.IsOverheating ? "khoi" : "tim", 2, 1.2f, 0.5f, 0.12f);
            }
        }

        // =====================================================================

        void BuildSprint11()
        {
            MoveSauceToStove();
            BuildStove2();
            BuildSprint12();
        }

        void RefreshSprint11()
        {
            RefreshStove2();
            RefreshSprint12();
        }

        void TickSprint11(float dt)
        {
            TickStove2(dt);
            TickSprint12(dt);
        }
    }
}
