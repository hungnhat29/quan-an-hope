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
            /// <summary>Tong tam giac ca file (nhan vat nhieu kieu: triangles = than + kieu nang nhat).</summary>
            public int trianglesAll;
            public Bounds bounds;
            public bool hasFace;
            /// <summary>Sprint 9: so xuong va ten clip (nhan vat SK_).</summary>
            public int bones;
            public readonly List<string> clips = new List<string>();
            public readonly List<string> errors = new List<string>();
            public bool Ok { get { return errors.Count == 0; } }
        }

        /// <summary>Ngan sach tam giac (Art Bible muc 14): nguyen lieu 600, do vat nho 800, mon an 1.500, khach 6.000, chu no 8.000.</summary>
        public static int Budget(string group) { return group == "Ingredients" ? 600 : group == "Dishes" ? 1500 : group == "Characters" ? 6000 : group == "Machines" ? 3000 : 800; }

        public static int Budget(string group, string name) { return name == "SK_ChuNo" ? 8000 : Budget(group); }

        /// <summary>Khoa kieu cua mot mesh nhan vat: "Toc_Vip", "Toc_Vip_Chu", "Do_Vip_The" -> "Vip"; "Mu_Non" -> "Mu_Non"; than -> null.</summary>
        public static string LookKey(string mesh)
        {
            if (mesh.StartsWith("Do_")) { string rest = mesh.Substring(3); int u = rest.IndexOf('_'); return u < 0 ? rest : rest.Substring(0, u); }
            if (mesh.StartsWith("Toc_")) { string rest = mesh.Substring(4); int u = rest.IndexOf('_'); return u < 0 ? rest : rest.Substring(0, u); }
            if (mesh.StartsWith("Mu_") || mesh == "Mam") return mesh;
            return null;
        }

        /// <summary>Kich thuoc lon nhat hop ly (m): nguyen lieu / mon &lt; 0,25 m; do bep &lt; 1,2 m; nhan vat &lt; 3,6 m. Nho hon 1 cm = sai ti le.</summary>
        static float MaxSize(string group) { return group == "Ingredients" || group == "Dishes" ? 0.25f : group == "Characters" ? 3.6f : group == "Machines" ? 2.5f : 1.2f; }

        /// <summary>Xuong toi thieu / toi da cho nhan vat co xuong (Art Bible muc 14: 12-16).</summary>
        public const int MinBones = 12, MaxBones = 16;

        /// <summary>Clip bat buoc (impl plan sprint 9 F-081): moi nhan vat co xuong + rieng khach / chu no.</summary>
        public static string[] RequiredClips(string name)
        {
            return name == "SK_ChuNo" ? new[] { "Cho", "Gian", "Di", "NgacNhien" } : new[] { "Cho", "Vui", "Gian", "VayChao", "Di" };
        }

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
            bool character = r.group == "Characters";
            bool skinned = r.name.StartsWith("SK_");
            if (!r.name.StartsWith("SM_") && !(character && skinned)) r.errors.Add("tên phải bắt đầu SM_ (nhân vật có xương: SK_) (Art Bible §14)");

            bool first = true;
            var mats = new HashSet<string>();
            var meshes = new List<KeyValuePair<Transform, Mesh>>();
            var rends = new List<Renderer>();
            foreach (MeshFilter f in go.GetComponentsInChildren<MeshFilter>(true))
                if (f.sharedMesh != null) { meshes.Add(new KeyValuePair<Transform, Mesh>(f.transform, f.sharedMesh)); rends.Add(f.GetComponent<MeshRenderer>()); }
            foreach (SkinnedMeshRenderer sk in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (sk.sharedMesh != null) { meshes.Add(new KeyValuePair<Transform, Mesh>(sk.transform, sk.sharedMesh)); rends.Add(sk); }
            var boneSet = new HashSet<Transform>();
            foreach (SkinnedMeshRenderer sk in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                foreach (Transform b in sk.bones) if (b != null) boneSet.Add(b);
            r.bones = boneSet.Count;
            for (int k = 0; k < meshes.Count; k++)
            {
                Transform ft = meshes[k].Key;
                Mesh m = meshes[k].Value;
                r.triangles += m.triangles.Length / 3;
                Renderer mr = rends[k];
                int matCount = mr != null ? mr.sharedMaterials.Length : 0;
                if (matCount > 3) r.errors.Add(ft.name + ": " + matCount + " vật liệu (tối đa 3)");
                if (mr != null) foreach (Material mm in mr.sharedMaterials) if (mm != null) mats.Add(mm.name);
                Bounds mb = m.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 p = go.transform.InverseTransformPoint(ft.TransformPoint(c));
                    if (first) { r.bounds = new Bounds(p, Vector3.zero); first = false; }
                    else r.bounds.Encapsulate(p);
                }
            }
            r.materials = mats.Count;
            // Nhan vat nhieu kieu (Toc_* / Mu_* / Mam / Do_*): moi luc chi bat MOT kieu, ngan sach Art Bible muc 14 tinh cho khach dang
            // hien = than + kieu nang nhat (Q-83, sau sprint 10). Tong ca file ghi rieng de tham khao.
            r.trianglesAll = r.triangles;
            if (character)
            {
                int baseTris = 0;
                var looks = new Dictionary<string, int>();
                for (int k = 0; k < meshes.Count; k++)
                {
                    string key = LookKey(meshes[k].Key.name);
                    int t = meshes[k].Value.triangles.Length / 3;
                    if (key == null) baseTris += t;
                    else { int cur; looks.TryGetValue(key, out cur); looks[key] = cur + t; }
                }
                int worst = 0;
                foreach (int t in looks.Values) worst = System.Math.Max(worst, t);
                r.triangles = baseTris + worst;
            }
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                if (t.name == "Face") r.hasFace = true;

            if (r.triangles > Budget(r.group, r.name)) r.errors.Add(r.triangles + " tam giác > " + Budget(r.group, r.name));
            if (skinned)
            {
                if (r.bones < MinBones || r.bones > MaxBones) r.errors.Add(r.bones + " xương (cần " + MinBones + "–" + MaxBones + ")");
                foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    var clip = o as AnimationClip;
                    if (clip == null || clip.name.StartsWith("__preview__")) continue;
                    int bar = clip.name.LastIndexOf('|');
                    r.clips.Add(bar >= 0 ? clip.name.Substring(bar + 1) : clip.name);
                }
                foreach (string c in RequiredClips(r.name)) if (!r.clips.Contains(c)) r.errors.Add("thiếu động tác " + c);
            }
            float size = Mathf.Max(r.bounds.size.x, Mathf.Max(r.bounds.size.y, r.bounds.size.z));
            if (size < 0.01f || size > MaxSize(r.group)) r.errors.Add("kích thước " + size.ToString("0.000") + " m: sai tỉ lệ (1 đơn vị = 1 m)");
            if (Mathf.Abs(r.bounds.min.y) > 0.002f) r.errors.Add("đáy ở y = " + r.bounds.min.y.ToString("0.000") + " (gốc phải ở đáy)");
            if (r.bounds.min.x > 0.001f || r.bounds.max.x < -0.001f || r.bounds.min.z > 0.001f || r.bounds.max.z < -0.001f)
                r.errors.Add("gốc nằm ngoài hình");
            if ((r.group == "Ingredients" || character || r.name.StartsWith("SM_Mon_")) && !r.hasFace) r.errors.Add("thiếu empty Face");
            if (!character)
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
                if (r.bones > 0) sb.Append(" · ").Append(r.bones).Append(" xương · ").Append(string.Join(",", r.clips));
                if (!r.Ok) { bad++; sb.Append(" · ").Append(string.Join("; ", r.errors)); }
                sb.Append('\n');
            }
            if (bad > 0) Debug.LogWarning(sb.ToString()); else Debug.Log(sb.ToString());
        }
    }
}
