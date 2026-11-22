using NUnit.Framework;
using QuanAnHope.EditorTools;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>F-063 (sprint 7): cong cu thu model Blender trong Unity (docs/blender/01-ca-chua.md ngay 5).</summary>
    public class Sprint7EditorTests
    {
        [Test]
        public void F063_GanToonFlat_GiuMau_ThemVienQuanhKhungBao()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            try
            {
                go.transform.localScale = Vector3.one * 0.1f;          // qua ca chua ~10 cm
                Material src = new Material(go.GetComponent<MeshRenderer>().sharedMaterial);
                Color tomato = new Color(0.91f, 0.376f, 0.31f);           // #E8604F
                if (src.HasProperty("_BaseColor")) src.SetColor("_BaseColor", tomato); else src.color = tomato;
                go.GetComponent<MeshRenderer>().sharedMaterial = src;

                Assert.AreEqual(1, ModelTestTools.ApplyToon(go, false));
                Material m = go.GetComponent<MeshRenderer>().sharedMaterial;
                Assert.AreEqual("BepNhanPham/ToonFlat", m.shader.name);
                Assert.AreEqual(tomato.r, m.GetColor("_Color").r, 1e-3, "giu mau Base Color cua FBX");
                Transform outline = go.transform.Find("Outline");
                Assert.IsNotNull(outline, "co lop vien");
                // sprint 9 B-03: vien theo diem anh (shader OutlinePx), ban sao phap tuyen lam muot, khong phong to
                Assert.AreEqual("BepNhanPham/OutlinePx", outline.GetComponent<MeshRenderer>().sharedMaterial.shader.name);
                Assert.AreEqual(1f, outline.localScale.x, 1e-3);
                Assert.AreEqual(1, ModelTestTools.ApplyToon(go, false), "gan lan hai khong nhan doi vien");
                int outlines = 0;
                foreach (Transform t in go.transform) if (t.name == "Outline") outlines++;
                Assert.AreEqual(1, outlines);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void F063_CanhThuModel_CoSan_Be_Den_ChoDatModel()
        {
            GameObject root = ModelTestTools.BuildTestScene(false);
            try
            {
                Assert.IsNotNull(root.transform.Find("San"));
                Assert.IsNotNull(root.transform.Find("Be"));
                Assert.AreEqual(LightType.Directional, root.transform.Find("DenChinh").GetComponent<Light>().type);
                Assert.IsNotNull(root.transform.Find("DatModelOday"));
                Assert.AreEqual("BepNhanPham/ToonFlat", root.transform.Find("San").GetComponent<MeshRenderer>().sharedMaterial.shader.name);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
