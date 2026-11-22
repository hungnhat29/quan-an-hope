using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Dung do vat 3D luc choi bang cung phong cach voi canh (Art Bible muc 5 va 10):
    /// khoi tron / bo goc, mau phang 2 nac (shader ToonFlat), vien muc #2B2321 kieu inverted hull.
    /// </summary>
    public static class Toon
    {
        public const float OutlineBig = 0.028f;   // nhan vat, may lon
        public const float OutlineSmall = 0.018f; // dao cu nho

        static Shader toonShader, alphaShader;
        static readonly Dictionary<Color, Material> toonMats = new Dictionary<Color, Material>();
        static Material shadowMat;

        static Shader ToonShader { get { return toonShader != null ? toonShader : (toonShader = Shader.Find("BepNhanPham/ToonFlat")); } }

        public static Material Mat(Color c)
        {
            Material m;
            if (toonMats.TryGetValue(c, out m) && m != null) return m;
            m = new Material(ToonShader) { name = "M_Toon_" + ColorUtility.ToHtmlStringRGB(c) };
            m.SetColor("_Color", c);
            toonMats[c] = m;
            return m;
        }

        /// <summary>
        /// Khoi co ban. size la kich thuoc that (met); voi Cylinder, size.y la chieu cao that.
        /// Khong giu collider (va cham va tuong tac do code goi tu them).
        /// </summary>
        public static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPos, Vector3 size, Color color,
            float outline = OutlineSmall, Vector3 euler = default(Vector3))
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = euler;
            Vector3 scale = type == PrimitiveType.Cylinder ? new Vector3(size.x, size.y * 0.5f, size.z) : size;
            go.transform.localScale = scale;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = Mat(color);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            if (outline > 0f) AddOutline(go, size, outline);
            return go;
        }

        /// <summary>
        /// Vien muc (sprint 9 B-03): ban sao phap tuyen lam muot + shader OutlinePx, do day theo diem anh man hinh
        /// (thay ban sao phong to theo met bi lech / qua day khi nhin gan). Vat nho hon nguong chi tiet khong co vien.
        /// </summary>
        static void AddOutline(GameObject go, Vector3 size, float outline)
        {
            if (!BepNhanPham.OutlineMesh.Wants(size, outline)) return;
            BepNhanPham.OutlineMesh.Attach(go.GetComponent<MeshRenderer>(), outline);
        }

        /// <summary>Doi mau than cua mot khoi da tao (khong dong vao vien).</summary>
        public static void Recolor(GameObject go, Color color)
        {
            var r = go.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = Mat(color);
        }

        public static GameObject Empty(Transform parent, string name, Vector3 localPos, Vector3 euler = default(Vector3))
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = euler;
            return go;
        }

        /// <summary>Bong tron mau muc do duc 20% duoi nhan vat va may (Art Bible muc 10).</summary>
        public static GameObject BlobShadow(Transform parent, string name, Vector3 localPos, float diameter)
        {
            if (shadowMat == null)
            {
                if (alphaShader == null) alphaShader = Shader.Find("BepNhanPham/UnlitAlpha");
                shadowMat = new Material(alphaShader) { name = "M_BlobShadow" };
                shadowMat.SetTexture("_MainTex", BlobTexture());
                Color c = UiKit.Ink;
                c.a = 0.2f;
                shadowMat.SetColor("_Color", c);
            }
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            go.transform.localScale = new Vector3(diameter, diameter, 1f);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = shadowMat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        static Texture2D BlobTexture()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "T_BlobShadow", wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / half;
                    byte a = (byte)(255f * Mathf.Clamp01((1f - d) * 3f));
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
