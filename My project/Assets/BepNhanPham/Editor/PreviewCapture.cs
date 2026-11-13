using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Chup anh 4 goc phong o 1280x720 de duyet theo Art Bible muc 15 (lan duyet 3).
    /// Anh luu vao docs/preview/ o goc repo. Khong chup lop UI (Screen Space Overlay).
    /// </summary>
    public static class PreviewCapture
    {
        const string ScenePath = "Assets/BepNhanPham/Scenes/NhaHang.unity";
        const int Width = 1280;
        const int Height = 720;

        [MenuItem("Bep Nhan Pham/Chup anh 4 goc (docs/preview)")]
        public static void Capture()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var rig = Object.FindFirstObjectByType<CameraRig>();
            Camera cam = rig != null ? rig.GetComponentInChildren<Camera>() : null;
            if (cam == null)
            {
                Debug.LogError("Bep Nhan Pham: khong tim thay CameraRig trong canh " + ScenePath);
                return;
            }

            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", "preview"));
            Directory.CreateDirectory(outDir);

            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            Quaternion savedRotation = rig.transform.localRotation;
            RenderTexture savedTarget = cam.targetTexture;
            RenderTexture savedActive = RenderTexture.active;

            for (int i = 0; i < 4; i++)
            {
                rig.transform.localRotation = Quaternion.Euler(0f, i * 90f, 0f);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(outDir, "Goc" + (i + 1) + ".png"), tex.EncodeToPNG());
            }

            cam.targetTexture = savedTarget;
            RenderTexture.active = savedActive;
            rig.transform.localRotation = savedRotation;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Debug.Log("Bep Nhan Pham: da chup 4 goc vao " + outDir);
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
