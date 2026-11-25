using System.IO;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.EditorTools
{
    /// <summary>Viec thiet lap project lam mot lan: tai nguyen TextMeshPro, danh sach canh de build.</summary>
    public static class ProjectSetup
    {
        const string GameScene = "Assets/BepNhanPham/Scenes/NhaHang.unity";
        const string TmpSettings = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        const string TmpPackage = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

        [MenuItem("Quán Ăn Hope/Thiết lập project (TMP + cảnh build)")]
        public static void Run()
        {
            SetBuildScenes();
            ImportTmpEssentials(false);
        }

        /// <summary>Dung cho dong lenh (khong kem -quit): tu thoat khi nhap xong goi TMP.</summary>
        public static void RunBatch()
        {
            SetBuildScenes();
            ImportTmpEssentials(true);
        }

        public static void SetBuildScenes()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(GameScene, true) };
            Debug.Log("Quán Ăn Hope: cảnh build = " + GameScene);
        }

        /// <summary>Build ban Windows 64-bit vao "My project/Builds/Windows/" (thu muc nay khong dua len Git).</summary>
        [MenuItem("Quán Ăn Hope/Build Windows")]
        public static void BuildWindows()
        {
            const string output = "Builds/Windows/QuanAnHope.exe";
            // F-093 AC-1, AC-4: ten san pham, cong ty, phien ban, bieu tuong (Branding)
            Branding.Apply();
            // Toan man hinh, do phan giai goc cua man hinh (nguoi dung yeu cau 14/11).
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.resizableWindow = false;
            var options = new BuildPlayerOptions
            {
                scenes = new[] { GameScene },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;
            string line = "Quán Ăn Hope: build Windows " + summary.result + ", " + (summary.totalSize / (1024 * 1024)) + " MB, "
                + summary.totalErrors + " lỗi, " + summary.totalWarnings + " cảnh báo → " + output;
            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded) Debug.Log(line);
            else Debug.LogError(line);
            if (Application.isBatchMode) EditorApplication.Exit(summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }

        static void ImportTmpEssentials(bool exitWhenDone)
        {
            if (File.Exists(TmpSettings))
            {
                Debug.Log("Quán Ăn Hope: TMP Essential Resources đã có.");
                if (exitWhenDone) EditorApplication.Exit(0);
                return;
            }

            string package = Path.GetFullPath(TmpPackage);
            if (!File.Exists(package))
            {
                Debug.LogError("Quán Ăn Hope: không tìm thấy " + TmpPackage);
                if (exitWhenDone) EditorApplication.Exit(1);
                return;
            }

            if (exitWhenDone)
            {
                AssetDatabase.importPackageCompleted += name =>
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log("Quán Ăn Hope: đã nhập " + name);
                    EditorApplication.Exit(0);
                };
                AssetDatabase.importPackageFailed += (name, error) =>
                {
                    Debug.LogError("Quán Ăn Hope: nhập TMP thất bại: " + error);
                    EditorApplication.Exit(1);
                };
            }
            AssetDatabase.ImportPackage(package, false);
        }
    }
}
