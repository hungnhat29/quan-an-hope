using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Duong ong model Blender -> game (sprint 8 F-070; Art Bible muc 14). FBX dat dung ten trong
    /// Assets/QuanAnHope/Resources/Models/&lt;Nhom&gt;/ thi game tu dung, khong code rieng tung model; thieu file thi noi goi
    /// giu hinh ghep khoi cu (AC-2). Vat lieu cua FBX doi sang ToonFlat 2 tong (mau lay tu ten vat lieu "M_Toon_RRGGBB",
    /// khong lo khong gian mau), them vien inverted hull theo phap tuyen (AC-4), khong bong do (Art Bible muc 10).
    /// </summary>
    public static class ModelLibrary
    {
        public const string Folder = "Models";
        public const string FaceNode = "Face";

        static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();
                static readonly Regex HexInName = new Regex("([0-9A-Fa-f]{6})");

        /// <summary>Tat model (test so sanh truoc / sau: chup anh hinh ghep khoi).</summary>
        public static bool Disabled;

        public static string IngredientName(string typeId, bool prepared)
        {
            return "SM_NL_" + typeId + (prepared ? "_SoChe" : "");
        }

        public static GameObject Prefab(string group, string name)
        {
            if (Disabled) return null;
            string key = group + "/" + name;
            GameObject p;
            if (cache.TryGetValue(key, out p)) return p;
            p = Resources.Load<GameObject>(Folder + "/" + key);
            cache[key] = p;
            return p;
        }

        public static bool Has(string group, string name) { return Prefab(group, name) != null; }

        /// <summary>
        /// Dung model vao <paramref name="parent"/> (goc model trung goc parent). Null neu chua co file.
        /// <paramref name="outline"/>: do day vien theo don vi cua model; <paramref name="recolor"/> doi mau tung vat lieu.
        /// </summary>
        public static GameObject TryInstantiate(string group, string name, Transform parent, float outline, System.Func<Color, Color> recolor = null)
        {
            GameObject prefab = Prefab(group, name);
            if (prefab == null) return null;
            GameObject go = Object.Instantiate(prefab, parent, false);
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            foreach (Collider c in go.GetComponentsInChildren<Collider>()) Object.Destroy(c);
            var renderers = new List<Renderer>();
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
                if (r is MeshRenderer || r is SkinnedMeshRenderer) renderers.Add(r);
            foreach (Renderer r in renderers)
            {
                Material[] src = r.sharedMaterials;
                var mats = new Material[src.Length];
                for (int i = 0; i < src.Length; i++)
                {
                    Color c = ColorOf(src[i]);
                    mats[i] = Toon.Mat(recolor != null ? recolor(c) : c);
                }
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                if (outline > 0f) AddHull(r, outline);
            }
            return go;
        }

        /// <summary>Mau cua vat lieu FBX: uu tien ma hex trong ten (M_Toon_E8604F), khong thi _BaseColor / _Color.</summary>
        public static Color ColorOf(Material m)
        {
            if (m == null) return Color.white;
            Match k = HexInName.Match(m.name);
            if (k.Success) return UiKit.Hex("#" + k.Groups[1].Value);
            if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
            return m.HasProperty("_Color") ? m.color : Color.white;
        }

        /// <summary>Vien muc theo diem anh (sprint 9 B-03): ban sao phap tuyen lam muot, shader OutlinePx (ca mesh co xuong).</summary>
        static void AddHull(Renderer r, float width)
        {
            BepNhanPham.OutlineMesh.Attach(r, width);
        }

        public static Material OutlineMat(float width)
        {
            return BepNhanPham.OutlineMesh.Material(width);
        }

        /// <summary>Diem mat cua model (empty "Face", AC-5); null neu khong co.</summary>
        public static Transform FacePoint(GameObject model)
        {
            if (model == null) return null;
            foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
                if (t.name == FaceNode || t.name.StartsWith(FaceNode + ".")) return t;
            return null;
        }

        /// <summary>
        /// Thay hinh ghep khoi bang model: an cac khoi <paramref name="prims"/> (khong dong vao collider / tuong tac), dat model o goc
        /// <paramref name="host"/>. False neu chua co model (giu khoi).
        /// </summary>
        public static bool Swap(Transform host, IList<GameObject> prims, string group, string name, float outline, System.Func<Color, Color> recolor = null)
        {
            if (!Has(group, name)) return false;
            foreach (GameObject p in prims) if (p != null) p.SetActive(false);
            GameObject m = TryInstantiate(group, name, host, outline, recolor);
            if (m != null) m.transform.SetAsFirstSibling();
            return m != null;
        }

        /// <summary>Hop bao cua mesh model theo toa do cua <paramref name="space"/> (L-016: lay tu hinh that).</summary>
        public static Bounds LocalBounds(GameObject model, Transform space)
        {
            var b = new Bounds();
            bool first = true;
            foreach (MeshFilter f in model.GetComponentsInChildren<MeshFilter>())
            {
                if (f.sharedMesh == null || f.name == "Outline") continue;
                Bounds mb = f.sharedMesh.bounds;
                Vector3 c = mb.center, e = mb.extents;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z);
                    Vector3 p = space.InverseTransformPoint(f.transform.TransformPoint(corner));
                    if (first) { b = new Bounds(p, Vector3.zero); first = false; }
                    else b.Encapsulate(p);
                }
            }
            return b;
        }
    }
}
