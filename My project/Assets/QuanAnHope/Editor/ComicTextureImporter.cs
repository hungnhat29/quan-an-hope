using UnityEditor;

namespace QuanAnHope.EditorTools
{
    /// <summary>
    /// Anh lop truyen tranh (Resources/CotTruyen, xuat bang tools/cot-truyen/export.js): giu dung kich thuoc (khong lam tron 2^n),
    /// khong mipmap (UI), alpha trong suot, nen chat luong cao (anh xuat co canh chia het cho 4).
    /// </summary>
    public class ComicTextureImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/CotTruyen/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            ti.maxTextureSize = 2048;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
        }
    }
}
