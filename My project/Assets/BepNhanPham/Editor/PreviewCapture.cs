using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Chup anh can phong trong Editor (chua bam Play) o 1280x720 de duyet theo Art Bible muc 15 (lan duyet 3).
    /// Dung o mat nguoi choi (giua phong, cao 1,75 m) nhin ve 4 tuong va 4 goc cheo. Anh luu vao docs/preview/.
    /// Do vat choi duoc (thung, sach, bang) chi co khi bam Play; anh choi thu do test PlayMode chup vao docs/sprint-1/ui/.
    /// </summary>
    public static class PreviewCapture
    {
        const string ScenePath = "Assets/BepNhanPham/Scenes/NhaHang.unity";
        const int Width = 1280;
        const int Height = 720;
        const float EyeHeight = 1.75f;

        [MenuItem("Bep Nhan Pham/Chup anh can phong (docs/preview)")]
        public static void Capture()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Bep Nhan Pham: khong tim thay Main Camera trong canh " + ScenePath);
                return;
            }

            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", "preview"));
            Directory.CreateDirectory(outDir);
            foreach (string old in Directory.GetFiles(outDir, "*.png")) File.Delete(old);

            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            Vector3 savedPos = cam.transform.position;
            Quaternion savedRot = cam.transform.rotation;
            RenderTexture savedTarget = cam.targetTexture;
            RenderTexture savedActive = RenderTexture.active;

            for (int i = 0; i < 8; i++)
            {
                float yaw = i * 45f;
                bool diagonal = i % 2 == 1;
                // Goc cheo: lui ve goc doi dien de thay hai buc tuong cung luc.
                Vector3 back = diagonal ? Quaternion.Euler(0f, yaw, 0f) * Vector3.back * 1.6f : Vector3.zero;
                cam.transform.position = new Vector3(back.x, EyeHeight, back.z);
                cam.transform.rotation = Quaternion.Euler(diagonal ? 14f : 8f, yaw, 0f);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                tex.Apply();
                string name = diagonal ? "Cheo_" + (i / 2 + 1) + "-" + ((i / 2 + 1) % 4 + 1) : "Goc" + (i / 2 + 1);
                File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG());
            }

            cam.targetTexture = savedTarget;
            RenderTexture.active = savedActive;
            cam.transform.SetPositionAndRotation(savedPos, savedRot);
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Debug.Log("Bep Nhan Pham: da chup can phong vao " + outDir);
        }

        /// <summary>
        /// Dung cho dong lenh: nhap du lieu can bang, dung lai canh, chup anh.
        /// Unity.exe -batchmode -projectPath "My project" -executeMethod BepNhanPham.PreviewCapture.RebuildAll -quit
        /// </summary>
        public static void RebuildAll()
        {
            QuanAnHope.EditorTools.BalanceImporter.Import();
            BuildNhaHang.Build();
            Capture();
        }
    }
}
