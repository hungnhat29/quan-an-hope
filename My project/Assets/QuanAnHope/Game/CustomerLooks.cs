using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Sprint 8 F-080 (yeu cau 21/11 "ve them khach hang theo art bible"): 6 kieu khach dung lai khoi khach cua canh
    /// (Art Bible muc 6: dau to 45-50%, tay ong cao su, gang trang) nhung khac hinh bong (toc, mu, phu kien - muc 5) va mau ao.
    /// Ao chi dung mau nhan vat cua bang mau (Art Bible muc 4), khong dung 6 mau bac (tru kieu 0 la khoi goc cua canh).
    /// Tay cam dua (EatingFx) lay vat lieu cua ong tay nen tu cung mau ao.
    /// </summary>
    public static class CustomerLooks
    {
        public const int Count = 6;
        public static readonly string[] Names = { "Bà cụ", "Cậu học sinh", "Cô văn phòng", "Chú nông dân", "Ông họa sĩ", "Em bé" };

        static readonly Color Leaf = UiKit.Hex("#7DB87A"), Curtain = UiKit.Hex("#E9A0A0"), Cream = UiKit.Hex("#FFF3DC");
        static readonly Color WoodDark = UiKit.Hex("#B97F52"), Blush = UiKit.Hex("#F2A0A6"), Straw = UiKit.Hex("#D69B68");
        static readonly Color HairDark = UiKit.Hex("#5A4036"), HairSilver = UiKit.Hex("#EFE7DA"), Coral = UiKit.Hex("#E8604F");

        static readonly Color[] Shirts = { UiKit.Hex("#8CC9AE"), Leaf, Curtain, Cream, WoodDark, Blush };

        /// <summary>Kieu khach theo so thu tu khach trong ngay (tron deu, hai khach lien nhau khac kieu).</summary>
        public static int LookFor(int customerNumber, int day)
        {
            int k = (customerNumber * 5 + day * 3) % Count;
            return k < 0 ? k + Count : k;
        }

        class State
        {
            public readonly Dictionary<string, Material> original = new Dictionary<string, Material>();
            public GameObject extras;
            public int look = -1;
        }

        static readonly Dictionary<GameObject, State> states = new Dictionary<GameObject, State>();

        public static int Current(GameObject model)
        {
            State st;
            return model != null && states.TryGetValue(model, out st) ? st.look : -1;
        }

        /// <summary>Mau ao dang mac (test: tay cam dua cung mau ao).</summary>
        public static Color ShirtOf(int look) { return Shirts[Mathf.Clamp(look, 0, Count - 1)]; }

        /// <summary>Mac kieu <paramref name="look"/> cho khoi khach (-1 = khoi goc: chu no).</summary>
        public static void Apply(GameObject model, int look)
        {
            if (model == null) return;
            State st;
            if (!states.TryGetValue(model, out st))
            {
                st = new State();
                foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                    if (r.name != "Outline" && !st.original.ContainsKey(r.name)) st.original[r.name] = r.sharedMaterial;
                states[model] = st;
            }
            if (st.look == look) return;
            st.look = look;
            if (st.extras != null) Object.Destroy(st.extras);
            st.extras = null;
            // tra khoi ve dang goc
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
            {
                Material m;
                if (r.name != "Outline" && st.original.TryGetValue(r.name, out m)) r.sharedMaterial = m;
            }
            Show(model, "Bun", true); Show(model, "Hairpin", true); Show(model, "HairpinBead", true); Show(model, "Hair", true); Show(model, "Brooch", true);
            if (look < 0) return;

            Color shirt = Shirts[look % Count];
            foreach (string part in new[] { "Body", "ArmWave", "ArmRest" }) Recolor(model, part, shirt);
            Transform head = model.transform.Find("Head");
            if (head == null) return;
            Vector3 top = head.localPosition + new Vector3(0f, head.localScale.y * 0.5f, 0f);
            float w = head.localScale.x;
            st.extras = new GameObject("PhuKien");
            Transform x = st.extras.transform;
            x.SetParent(model.transform, false);

            switch (look)
            {
                case 1: // cau hoc sinh: toc den ngan, mu luoi trai san ho
                    Show(model, "Bun", false); Show(model, "Hairpin", false); Show(model, "HairpinBead", false);
                    Recolor(model, "Hair", HairDark);
                    Toon.Prim(PrimitiveType.Sphere, x, "Mu", top + new Vector3(0f, -0.06f, 0.02f), new Vector3(w * 0.98f, w * 0.42f, w * 0.92f), Coral, Toon.OutlineBig);
                    Toon.Prim(PrimitiveType.Cylinder, x, "LuoiTrai", top + new Vector3(0f, -0.14f, -w * 0.5f), new Vector3(w * 0.6f, 0.035f, w * 0.42f), Coral, Toon.OutlineSmall, new Vector3(-8f, 0f, 0f));
                    break;
                case 2: // co van phong: toc nau sam, hai bui toc tron hai ben
                    Show(model, "Bun", false); Show(model, "Hairpin", false); Show(model, "HairpinBead", false); Show(model, "Brooch", false);
                    Recolor(model, "Hair", HairDark);
                    for (int s = -1; s <= 1; s += 2)
                        Toon.Prim(PrimitiveType.Sphere, x, "BuiToc", head.localPosition + new Vector3(s * w * 0.55f, w * 0.2f, 0.08f), Vector3.one * w * 0.34f, HairDark, Toon.OutlineBig);
                    Toon.Prim(PrimitiveType.Sphere, x, "No", top + new Vector3(w * 0.25f, -0.05f, -w * 0.1f), new Vector3(0.2f, 0.12f, 0.08f), Coral, Toon.OutlineSmall);
                    break;
                case 3: // chu nong dan: non rom vanh rong tron (Art Bible muc 5: tron, khong tam giac)
                    Show(model, "Bun", false); Show(model, "Hairpin", false); Show(model, "HairpinBead", false);
                    Recolor(model, "Hair", HairDark);
                    Toon.Prim(PrimitiveType.Cylinder, x, "VanhNon", top + new Vector3(0f, -0.1f, 0.02f), new Vector3(w * 1.75f, 0.05f, w * 1.75f), Straw, Toon.OutlineBig);
                    Toon.Prim(PrimitiveType.Sphere, x, "DinhNon", top + new Vector3(0f, -0.02f, 0.02f), new Vector3(w * 0.8f, w * 0.38f, w * 0.8f), Straw, Toon.OutlineBig);
                    Toon.Prim(PrimitiveType.Cylinder, x, "DayNon", top + new Vector3(0f, -0.07f, 0.02f), new Vector3(w * 0.82f, 0.04f, w * 0.82f), Coral, 0f);
                    break;
                case 4: // ong hoa si: toc bac hai ben, mu noi det san ho
                    Show(model, "Bun", false); Show(model, "Hairpin", false); Show(model, "HairpinBead", false);
                    Recolor(model, "Hair", HairSilver);
                    Toon.Prim(PrimitiveType.Cylinder, x, "MuNoi", top + new Vector3(-0.08f, -0.04f, 0.04f), new Vector3(w * 1.02f, 0.12f, w * 0.96f), Coral, Toon.OutlineBig, new Vector3(0f, 0f, 9f));
                    Toon.Prim(PrimitiveType.Sphere, x, "NumMu", top + new Vector3(-0.08f, 0.04f, 0.04f), Vector3.one * 0.1f, Coral, Toon.OutlineSmall);
                    break;
                case 5: // em be: nguoi nho, mot chiec la mam tren dinh dau, no san ho
                    Show(model, "Bun", false); Show(model, "Hairpin", false); Show(model, "HairpinBead", false);
                    Recolor(model, "Hair", HairDark);
                    Toon.Prim(PrimitiveType.Cylinder, x, "CuongMam", top + new Vector3(0f, 0.1f, 0.05f), new Vector3(0.05f, 0.22f, 0.05f), Leaf, Toon.OutlineSmall);
                    Toon.Prim(PrimitiveType.Sphere, x, "LaMam", top + new Vector3(0.12f, 0.24f, 0.05f), new Vector3(0.26f, 0.06f, 0.14f), Leaf, Toon.OutlineSmall, new Vector3(0f, 0f, 20f));
                    Toon.Prim(PrimitiveType.Sphere, x, "No", head.localPosition + new Vector3(w * 0.45f, w * 0.32f, 0f), new Vector3(0.22f, 0.13f, 0.1f), Coral, Toon.OutlineSmall);
                    break;
                default: // ba cu: khoi goc cua canh (toc bac bui, tram cai)
                    break;
            }
        }

        static void Show(GameObject model, string part, bool on)
        {
            Transform t = model.transform.Find(part);
            if (t != null) t.gameObject.SetActive(on);
        }

        static void Recolor(GameObject model, string part, Color c)
        {
            Transform t = model.transform.Find(part);
            if (t != null) Toon.Recolor(t.gameObject, c);
        }
    }
}
