using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.EditorTools
{
    /// <summary>
    /// Kiem quy cach model Blender (sprint 8 F-070 AC-3; Art Bible muc 14): moi FBX trong Resources/Models/&lt;Nhom&gt;/ phai
    /// dung ti le (1 don vi = 1 m: kich thuoc trong khoang hop ly cua nhom), goc o day (day cham y = 0) va nam trong hinh,
    /// tam giac duoi ngan sach, &lt;= 3 vat lieu, ten bat dau SM_, nguyen lieu co empty "Face" (AC-5).
    /// Menu "Quán Ăn Hope > Kiểm model Blender" in bao cao; test EditMode goi Check().
    /// </summary>
    public static class ModelChecker
    {
        public const string Root = "Assets/QuanAnHope/Resources/Models";

        public class Result
        {
            public string path, name, group;
            public int triangles, materials;
            public Bounds bounds;
            public bool hasFace;
            public readonly List<string> errors = new List<string>();
            public bool Ok { get { return errors.Count == 0; } }
        }

        /// <summary>Ngan sach tam giac theo nhom (Art Bible muc 14): nguyen lieu 600, do vat nho 800.</summary>
        public static int Budget(string group) { return group == "Ingredients" ? 600 : 800; }

        /// <summary>Kich thuoc lon nhat hop ly (m): nguyen lieu &lt; 0,25 m; do bep &lt; 1,2 m (thot 1 m). Nho hon 1 cm = sai ti le.</summary>
        static float MaxSize(string group) { return group == "Ingredients" ? 0.25f : 1.2f; }

        public static List<Result> CheckAll()
        {
            var list = new List<Result>();
            if (!AssetDatabase.IsValidFolder(Root)) return list;
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { Root }))
                list.Add(Check(AssetDatabase.GUIDToAssetPath(guid)));
            return list;
        }

        public static Result Check(string path)
        {
            var r = new Result { path = path, name = Path.GetFileNameWithoutExtension(path) };
            r.group = Path.GetFileName(Path.GetDirectoryName(path));
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) { r.errors.Add("không nạp được"); return r; }
            if (!r.name.StartsWith("SM_")) r.errors.Add("tên phải bắt đầu SM_ (Art Bible §14)");

            bool first = true;
            var mats = new HashSet<string>();
            foreach (MeshFilter f in go.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh m = f.sharedMesh;
                if (m == null) continue;
                r.triangles += m.triangles.Length / 3;
                var mr = f.GetComponent<MeshRenderer>();
                int matCount = mr != null ? mr.sharedMaterials.Length : 0;
                if (matCount > 3) r.errors.Add(f.name + ": " + matCount + " vật liệu (tối đa 3)");
                if (mr != null) foreach (Material mm in mr.sharedMaterials) if (mm != null) mats.Add(mm.name);
                Bounds mb = m.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 p = go.transform.InverseTransformPoint(f.transform.TransformPoint(c));
                    if (first) { r.bounds = new Bounds(p, Vector3.zero); first = false; }
                    else r.bounds.Encapsulate(p);
                }
            }
            r.materials = mats.Count;
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                if (t.name == "Face") r.hasFace = true;

            if (r.triangles > Budget(r.group)) r.errors.Add(r.triangles + " tam giác > " + Budget(r.group));
            float size = Mathf.Max(r.bounds.size.x, Mathf.Max(r.bounds.size.y, r.bounds.size.z));
            if (size < 0.01f || size > MaxSize(r.group)) r.errors.Add("kích thước " + size.ToString("0.000") + " m: sai tỉ lệ (1 đơn vị = 1 m)");
            if (Mathf.Abs(r.bounds.min.y) > 0.002f) r.errors.Add("đáy ở y = " + r.bounds.min.y.ToString("0.000") + " (gốc phải ở đáy)");
            if (r.bounds.min.x > 0.001f || r.bounds.max.x < -0.001f || r.bounds.min.z > 0.001f || r.bounds.max.z < -0.001f)
                r.errors.Add("gốc nằm ngoài hình");
            if (r.group == "Ingredients" && !r.hasFace) r.errors.Add("thiếu empty Face");
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                if (t != go.transform && (Mathf.Abs(t.localScale.x - 1f) > 1e-3f || Quaternion.Angle(t.localRotation, Quaternion.identity) > 0.5f))
                    r.errors.Add(t.name + " còn scale / xoay chưa áp (Ctrl+A trước khi xuất)");
            return r;
        }

        [MenuItem("Quán Ăn Hope/Kiểm model Blender")]
        public static void Report()
        {
            var sb = new StringBuilder("Kiểm model Blender (Art Bible §14)\n");
            int bad = 0;
            foreach (Result r in CheckAll())
            {
                sb.Append(r.Ok ? "  ✔ " : "  ✘ ").Append(r.group).Append('/').Append(r.name)
                  .Append(" · ").Append(r.triangles).Append(" tam giác · ").Append(r.materials).Append(" vật liệu · ")
                  .Append(r.bounds.size.ToString("0.000")).Append(" m");
                if (!r.Ok) { bad++; sb.Append(" · ").Append(string.Join("; ", r.errors)); }
                sb.Append('\n');
            }
            if (bad > 0) Debug.LogWarning(sb.ToString()); else Debug.Log(sb.ToString());
        }
    }
}
