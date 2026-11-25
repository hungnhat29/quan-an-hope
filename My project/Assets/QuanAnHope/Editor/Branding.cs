using System.IO;
using QuanAnHope.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace QuanAnHope.EditorTools
{
    /// <summary>
    /// F-093 AC-1, AC-4 (docs/sprint-10/impl-plan.md): ten san pham, ten cong ty, so phien ban, bieu tuong game trong cai dat project.
    /// Goi tu ProjectSetup.BuildWindows truoc khi build. Ten chinh thuc con cho ban chot (Q-46): dung ten tren menu chinh.
    /// </summary>
    public static class Branding
    {
        /// <summary>Ten hien tren thanh cua so / Task Manager (menu chinh: MenuView.GameTitle).</summary>
        public const string ProductName = MenuView.GameTitle;

        /// <summary>[TU QUYET] Q-70: ten cong ty = ten tai khoan GitHub cua ban (khong co ten cong ty nao trong tai lieu).</summary>
        public const string CompanyName = "hungnhat29";

        /// <summary>0.&lt;sprint&gt;.&lt;lan build&gt;: sprint 11, release candidate rc-1 (F-114).</summary>
        public const string Version = "0.11.0";

        public const string IconPath = "Assets/QuanAnHope/Art/AppIcon.png";

        [MenuItem("Quán Ăn Hope/Gắn tên, phiên bản, biểu tượng")]
        public static void Apply()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.bundleVersion = Version;
            Texture2D icon = MakeIcon();
            if (icon != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
                PlayerSettings.SetIcons(NamedBuildTarget.Standalone, new[] { icon, icon, icon, icon, icon, icon, icon }, IconKind.Any);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Quán Ăn Hope: " + ProductName + " · " + CompanyName + " · " + Version + " · biểu tượng " + (icon != null ? IconPath : "không có"));
        }

        /// <summary>
        /// Bieu tuong 512 px theo Art Bible: dia tron mu tat vien muc #2B2321, ben trong noi do (#E8604F) co mat "w" cua nguyen lieu
        /// (FaceArt, mat tron rong ho tren). Ve bang code de khong can phan mem ve.
        /// </summary>
        public static Texture2D MakeIcon()
        {
            const int S = 512;
            var px = new Color[S * S];
            Color ink = UiKit.Ink, mustard = UiKit.Mustard, coral = UiKit.Coral, cream = UiKit.Cream;
            float c = S * 0.5f;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c, d = Mathf.Sqrt(dx * dx + dy * dy);
                    Color col = new Color(0f, 0f, 0f, 0f);
                    if (d < 248f) col = ink;
                    if (d < 232f) col = mustard;
                    // noi: than bo goc + vanh
                    bool body = Mathf.Abs(dx) < 150f && dy > -150f && dy < 70f;
                    bool rim = Mathf.Abs(dx) < 172f && dy >= 70f && dy < 104f;
                    bool handle = (Mathf.Abs(dx) > 168f && Mathf.Abs(dx) < 214f && dy > 10f && dy < 46f);
                    if (body || rim || handle) col = d < 232f ? ink : col;
                    bool bodyIn = Mathf.Abs(dx) < 140f && dy > -140f && dy < 64f;
                    bool rimIn = Mathf.Abs(dx) < 162f && dy >= 78f && dy < 96f;
                    if (bodyIn || rimIn) col = coral;
                    // hoi nuoc: ba trai tim nho mau kem
                    for (int k = 0; k < 3; k++)
                    {
                        float hx = dx - (-70f + 70f * k), hy = dy - (150f + (k == 1 ? 26f : 0f));
                        float hd = Mathf.Sqrt(hx * hx + hy * hy);
                        if (hd < 22f) col = cream;
                    }
                    px[y * S + x] = col;
                }
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            // mat cua noi: anh FaceArt (khong doc truc tiep duoc - chep qua RenderTexture)
            Texture2D face = FaceArt.Ingredient(4);
            if (face != null)
            {
                RenderTexture rt = RenderTexture.GetTemporary(256, 160, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                Graphics.Blit(face, rt);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                var f = new Texture2D(256, 160, TextureFormat.RGBA32, false);
                f.ReadPixels(new Rect(0, 0, 256, 160), 0, 0);
                f.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                for (int y = 0; y < 160; y++)
                    for (int x = 0; x < 256; x++)
                    {
                        Color fc = f.GetPixel(x, y);
                        if (fc.a < 0.05f) continue;
                        int ix = (int)c - 128 + x, iy = (int)c - 110 + y;
                        tex.SetPixel(ix, iy, Color.Lerp(tex.GetPixel(ix, iy), new Color(fc.r, fc.g, fc.b, 1f), fc.a));
                    }
                Object.DestroyImmediate(f);
            }
            tex.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
            File.WriteAllBytes(IconPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Default;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        }
    }
}
