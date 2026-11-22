using UnityEditor;
using UnityEngine;

namespace QuanAnHope.EditorTools
{
    /// <summary>
    /// Quy cach nhap FBX Blender (sprint 9):
    /// - Moi model trong Resources/Models: bat Read/Write de OutlineMesh lam muot phap tuyen cho vien (B-03).
    /// - Characters/SK_*: Animation legacy (game tu lay mau thoi gian clip: muot hoac "tren 2", F-081 AC-3), khong nen clip,
    ///   bake truc (FBX nhan vat xuat khong bake transform vi co xuong). Characters/SM_*: tinh, khong animation.
    /// </summary>
    public class ModelImportSettings : AssetPostprocessor
    {
        const string Root = "Assets/QuanAnHope/Resources/Models/";

        public override uint GetVersion() { return 5; }

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Root)) return;
            var mi = assetImporter as ModelImporter;
            if (mi == null) return;
            mi.isReadable = true;
            if (!assetPath.Contains("/Characters/")) return;
            mi.bakeAxisConversion = true;
            mi.importCameras = false;
            mi.importLights = false;
            string file = System.IO.Path.GetFileName(assetPath);
            if (file.StartsWith("SK_"))
            {
                mi.animationType = ModelImporterAnimationType.Legacy;
                mi.importAnimation = true;
                mi.animationCompression = ModelImporterAnimationCompression.Off;
            }
            else
            {
                mi.animationType = ModelImporterAnimationType.None;
                mi.importAnimation = false;
            }
        }
    }
}
