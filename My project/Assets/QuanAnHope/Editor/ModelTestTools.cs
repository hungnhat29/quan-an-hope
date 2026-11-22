using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace QuanAnHope.EditorTools
{
    /// <summary>
    /// F-063 (sprint 7): cho bai tap Blender (docs/blender/01-ca-chua.md ngay 5). Canh thu model co san go, den chinh nhu phong,
    /// mot be o giua; lenh gan shader ToonFlat (giu mau Base Color cua FBX) + lop vien Outline (vo nguoc phong to theo khung bao,
    /// Art Bible muc 10: dao cu nho 0,018 m). Model thay vao game o sprint 8-9.
    /// </summary>
    public static class ModelTestTools
    {
        public const string ScenePath = "Assets/Art/Scenes/ThuModel.unity";
        const string MaterialFolder = "Assets/Art/Materials";
        public const float OutlineSmall = 0.018f;   // Art Bible muc 10: dao cu nho
        static readonly string[] Folders = { "Assets/Art", "Assets/Art/Models", "Assets/Art/Textures", "Assets/Art/Materials", "Assets/Art/Animations", "Assets/Art/VFX", "Assets/Art/UI", "Assets/Art/Scenes" };

        [MenuItem("Quán Ăn Hope/Cảnh thử model Blender")]
        public static void OpenTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolders();
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildTestScene(true);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        /// <summary>Dung canh thu (tach rieng de test goi duoc ma khong luu file).</summary>
        public static GameObject BuildTestScene(bool saveAssets)
        {
            var root = new GameObject("ThuModel");
            Material wood = Toon(Hex("#D69B68"), "M_Toon_GoNhat", saveAssets);
            Material cream = Toon(Hex("#FFF3DC"), "M_Toon_Kem", saveAssets);
            Prim(PrimitiveType.Cube, root.transform, "San", new Vector3(0f, -0.05f, 0f), new Vector3(3f, 0.1f, 3f), wood);
            Prim(PrimitiveType.Cylinder, root.transform, "Be", new Vector3(0f, 0.15f, 0f), new Vector3(0.4f, 0.15f, 0.4f), cream);
            var light = new GameObject("DenChinh").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = Hex("#FFF6E6");
            light.transform.SetParent(root.transform, false);
            var cam = new GameObject("Camera").AddComponent<Camera>();
            cam.transform.SetParent(root.transform, false);
            cam.transform.position = new Vector3(0f, 0.55f, -0.75f);
            cam.transform.LookAt(new Vector3(0f, 0.33f, 0f));
            cam.backgroundColor = Hex("#FFF3DC");
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 40f;
            var spot = new GameObject("DatModelOday").transform;
            spot.SetParent(root.transform, false);
            spot.localPosition = new Vector3(0f, 0.3f, 0f);
            return root;
        }

        [MenuItem("Quán Ăn Hope/Gắn ToonFlat + Outline cho model đang chọn")]
        public static void ApplyToSelection()
        {
            EnsureFolders();
            int n = 0;
            foreach (GameObject go in Selection.gameObjects) n += ApplyToon(go, true);
            Debug.Log("ToonFlat + Outline: " + n + " mesh.");
        }

        /// <summary>Gan ToonFlat (giu mau) va them vien cho moi mesh duoi go. Tra ve so mesh da gan.</summary>
        public static int ApplyToon(GameObject go, bool saveAssets)
        {
            int n = 0;
            // lay danh sach truoc (bo lop vien cu): AddOutline xoa vien cu nen khong duyet tren danh sach co no
            var renderers = new List<MeshRenderer>();
            foreach (MeshRenderer r in go.GetComponentsInChildren<MeshRenderer>(true)) if (r.name != "Outline") renderers.Add(r);
            foreach (MeshRenderer r in renderers)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Color c = mats[i] == null ? Color.white
                        : mats[i].HasProperty("_BaseColor") ? mats[i].GetColor("_BaseColor")
                        : mats[i].HasProperty("_Color") ? mats[i].GetColor("_Color") : Color.white;
                    string name = mats[i] != null ? mats[i].name : "Mau";
                    mats[i] = Toon(c, "M_Toon_" + name, saveAssets);
                }
                r.sharedMaterials = mats;
                r.shadowCastingMode = ShadowCastingMode.Off;
                AddOutline(r.transform, mf.sharedMesh, OutlineSmall, saveAssets);
                n++;
            }
            return n;
        }

        /// <summary>Vien muc theo diem anh (sprint 9 B-03): ban sao phap tuyen lam muot + shader OutlinePx, khong phong to.</summary>
        static void AddOutline(Transform t, Mesh mesh, float width, bool saveAssets)
        {
            Transform old = t.Find("Outline");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var o = new GameObject("Outline");
            o.transform.SetParent(t, false);
            Mesh sm = BepNhanPham.OutlineMesh.Build(mesh);
            if (saveAssets && AssetDatabase.Contains(mesh))
                AssetDatabase.CreateAsset(sm, MaterialFolder + "/Vien_" + mesh.name + ".asset");
            o.AddComponent<MeshFilter>().sharedMesh = sm;
            var r = o.AddComponent<MeshRenderer>();
            r.sharedMaterial = OutlineMaterial(width, saveAssets);
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        static Material outline;

        static Material OutlineMaterial(float width, bool saveAssets)
        {
            string path = MaterialFolder + "/M_OutlinePx.mat";
            if (saveAssets)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m != null) return m;
            }
            if (!saveAssets && outline != null) return outline;
            Material mat = BepNhanPham.OutlineMesh.NewMaterial(width);
            if (saveAssets) AssetDatabase.CreateAsset(mat, path);
            else outline = mat;
            return mat;
        }

        public static Material Toon(Color c, string name, bool saveAssets)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            if (saveAssets)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m != null) { m.SetColor("_Color", c); return m; }
            }
            var mat = new Material(Shader.Find("BepNhanPham/ToonFlat")) { name = name };
            mat.SetColor("_Color", c);
            if (saveAssets) AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static void EnsureFolders()
        {
            foreach (string f in Folders)
            {
                if (AssetDatabase.IsValidFolder(f)) continue;
                string parent = Path.GetDirectoryName(f).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, Path.GetFileName(f));
            }
        }

        static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString(hex, out c);
            return c;
        }
    }
}
