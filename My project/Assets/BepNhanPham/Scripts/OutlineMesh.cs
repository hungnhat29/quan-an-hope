using System.Collections.Generic;
using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Vien muc dung chung (sprint 9 B-03, Art Bible muc 10; yeu cau 22/11 "vien den dang bi lech ... lam lai cho chuan").
    /// Vien cu = ban sao phong to theo MET quanh tam khoi: nhin gan day hang chuc diem anh, tam mong lo mat sau sang mot ben.
    /// Vien moi: ban sao cung hinh, phap tuyen lam muot (dinh trung vi tri dung chung huong) de shader OutlinePx day ra mot khoang
    /// tinh bang diem anh. Do day ghi bang met nhu cu (0,028 m nhan vat / may lon ~ 5 px, 0,018 m dao cu ~ 3 px o 1280x720).
    /// </summary>
    public static class OutlineMesh
    {
        public const string ShaderName = "BepNhanPham/OutlinePx";
        public const string ChildName = "Outline";
        /// <summary>Met -> diem anh o 720p (0,028 m ~ 5 px, Art Bible muc 10).</summary>
        public const float MetersPerPx = 0.0056f;
        public const float MinPx = 2.5f, MaxPx = 5f;
        /// <summary>Vat nho hon ngay nay (canh lon nhat, met) khong ve vien (Art Bible muc 8: duoi nguong chi tiet).</summary>
        public const float SkipBelow = 0.03f;

        static readonly Dictionary<Mesh, Mesh> smoothed = new Dictionary<Mesh, Mesh>();
        static readonly Dictionary<float, Material> mats = new Dictionary<float, Material>();
        static Shader shader;

        public static Color Ink = new Color(0x2B / 255f, 0x23 / 255f, 0x21 / 255f, 1f);

        public static float Px(float meters) { return Mathf.Clamp(meters / MetersPerPx, MinPx, MaxPx); }

        /// <summary>Co ve vien cho vat co kich thuoc that <paramref name="size"/> (met) khong.</summary>
        public static bool Wants(Vector3 size, float meters)
        {
            return meters > 0f && Mathf.Max(size.x, Mathf.Max(size.y, size.z)) >= SkipBelow;
        }

        public static Shader Shader
        {
            get
            {
                if (shader == null) shader = Shader.Find(ShaderName);
                return shader;
            }
        }

        /// <summary>Vat lieu vien cho do day <paramref name="meters"/> (dung chung, cache).</summary>
        public static Material Material(float meters)
        {
            Material m;
            if (mats.TryGetValue(meters, out m) && m != null) return m;
            m = NewMaterial(meters);
            mats[meters] = m;
            return m;
        }

        public static Material NewMaterial(float meters)
        {
            var m = new Material(Shader) { name = "M_OutlinePx_" + Mathf.RoundToInt(meters * 1000f) };
            m.SetColor("_Color", Ink);
            m.SetFloat("_WidthPx", Px(meters));
            m.SetFloat("_WidthWorld", Mathf.Max(meters, 0.008f));
            return m;
        }

        /// <summary>
        /// Ban sao cua <paramref name="src"/>: cung dinh, gop moi submesh thanh mot (vat lieu vien chi ve submesh 0), phap tuyen
        /// = trung binh phap tuyen cua cac dinh trung vi tri. Giu boneWeights / bindposes cho mesh co xuong. Cache theo mesh goc.
        /// </summary>
        public static Mesh Smoothed(Mesh src)
        {
            if (src == null) return null;
            Mesh m;
            if (smoothed.TryGetValue(src, out m) && m != null) return m;
            if (!src.isReadable) return src;   // khong doc duoc (FBX chua bat Read/Write): dung nguyen ban
            m = Build(src);
            smoothed[src] = m;
            return m;
        }

        public static Mesh Build(Mesh src)
        {
            Vector3[] v = src.vertices;
            Vector3[] n = src.normals;
            if (n == null || n.Length != v.Length)
            {
                var tmp = Object.Instantiate(src);
                tmp.RecalculateNormals();
                n = tmp.normals;
                Object.DestroyImmediate(tmp);
            }
            var sum = new Dictionary<Vector3Int, Vector3>();
            var key = new Vector3Int[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                key[i] = new Vector3Int(Mathf.RoundToInt(v[i].x * 1e4f), Mathf.RoundToInt(v[i].y * 1e4f), Mathf.RoundToInt(v[i].z * 1e4f));
                Vector3 s;
                sum.TryGetValue(key[i], out s);
                sum[key[i]] = s + n[i];
            }
            var outN = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 s = sum[key[i]];
                outN[i] = s.sqrMagnitude > 1e-12f ? s.normalized : n[i];
            }
            var tris = new List<int>();
            for (int s = 0; s < src.subMeshCount; s++) tris.AddRange(src.GetTriangles(s));
            var m = new Mesh { name = src.name + "_Vien" };
            if (v.Length > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = v;
            m.normals = outN;
            m.SetTriangles(tris, 0);
            if (src.bindposes != null && src.bindposes.Length > 0)
            {
                m.boneWeights = src.boneWeights;
                m.bindposes = src.bindposes;
            }
            m.bounds = src.bounds;
            return m;
        }

        /// <summary>Gan vien cho mot MeshRenderer / SkinnedMeshRenderer (con ten "Outline", cung bo xuong). Tra ve vien (null neu bo qua).</summary>
        public static Renderer Attach(Renderer r, float meters)
        {
            if (r == null || meters <= 0f) return null;
            var mf = r.GetComponent<MeshFilter>();
            var sk = r as SkinnedMeshRenderer;
            Mesh src = sk != null ? sk.sharedMesh : mf != null ? mf.sharedMesh : null;
            if (src == null) return null;
            var o = new GameObject(ChildName);
            o.transform.SetParent(r.transform, false);
            Renderer hull;
            if (sk != null)
            {
                var hs = o.AddComponent<SkinnedMeshRenderer>();
                hs.sharedMesh = Smoothed(src);
                hs.bones = sk.bones;
                hs.rootBone = sk.rootBone;
                hs.localBounds = sk.localBounds;
                hs.updateWhenOffscreen = sk.updateWhenOffscreen;
                hull = hs;
            }
            else
            {
                o.AddComponent<MeshFilter>().sharedMesh = Smoothed(src);
                hull = o.AddComponent<MeshRenderer>();
            }
            hull.sharedMaterial = Material(meters);
            hull.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            hull.receiveShadows = false;
            return hull;
        }
    }
}
