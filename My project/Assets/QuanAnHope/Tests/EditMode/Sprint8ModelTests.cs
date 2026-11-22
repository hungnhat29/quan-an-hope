using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.EditorTools;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>Sprint 8 F-070 AC-3, F-071 AC-1, F-072 AC-1: quy cach model Blender (Art Bible muc 14).</summary>
    public class Sprint8ModelTests
    {
        static readonly string[] Round1 = { "ca_chua", "hanh_tay", "rau_muong", "trung", "heo", "dau_phu" };
        static readonly string[] Kitchen = { "SM_Chao", "SM_Noi", "SM_ViNuong", "SM_Thot", "SM_Dao", "SM_Dia", "SM_ThungPhu", "SM_ThungChinh", "SM_RuongDotPha" };

        static List<ModelChecker.Result> all;
        static List<ModelChecker.Result> All { get { return all ?? (all = ModelChecker.CheckAll()); } }

        static ModelChecker.Result Get(string group, string name)
        {
            return All.FirstOrDefault(r => r.group == group && r.name == name);
        }

        [Test]
        public void F070_AC3_MoiModel_DungQuyCach()
        {
            Assert.Greater(All.Count, 0, "chua co model trong " + ModelChecker.Root);
            var bad = All.Where(r => !r.Ok).Select(r => r.group + "/" + r.name + ": " + string.Join("; ", r.errors)).ToList();
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }

        [Test]
        public void F071_AC1_SauNguyenLieuVong1_NguyenVaSoChe()
        {
            foreach (string id in Round1)
            {
                Assert.IsNotNull(Get("Ingredients", "SM_NL_" + id), id + " nguyen");
                Assert.IsNotNull(Get("Ingredients", "SM_NL_" + id + "_SoChe"), id + " so che");
                Assert.IsTrue(Get("Ingredients", "SM_NL_" + id).hasFace, id + ": empty Face (F-070 AC-5)");
            }
        }

        [Test]
        public void F072_AC1_DoBep_DuModel_CungKichThuocKhoiCu()
        {
            foreach (string k in Kitchen) Assert.IsNotNull(Get("Kitchen", k), k);
            // kich thuoc khop hinh ghep khoi cu (RoomView.BuildTool / CrateView / BuildNhaHang) de va cham + do trong dung cu khong doi
            Bounds pan = Get("Kitchen", "SM_Chao").bounds;
            Assert.AreEqual(0.0f, pan.center.z, 0.01f);
            Assert.Greater(pan.max.x, 0.38f, "can chao o phia +x nhu khoi cu (huong xuat FBX dung)");
            Assert.AreEqual(0.4f, pan.size.z, 0.06f, "long chao ~0,36-0,4 m");
            Bounds knife = Get("Kitchen", "SM_Dao").bounds;
            Assert.Less(knife.min.x, -0.3f, "luoi dao chay theo -x (PrepFx)");
            Bounds crate = Get("Kitchen", "SM_ThungPhu").bounds;
            Assert.AreEqual(0.58f, crate.size.x, 0.04f);
            Assert.AreEqual(0.49f, crate.size.z, 0.04f);
            Bounds board = Get("Kitchen", "SM_Thot").bounds;
            Assert.AreEqual(1.0f, board.size.x, 0.02f);
            Assert.AreEqual(0.6f, board.size.z, 0.02f);
        }

        [Test]
        public void F072_ThungTruocThapSauCao_HuongTruocLa_TruAm()
        {
            // Art Bible muc 7: thanh truoc 7 cm (phia nguoi choi, -z), thanh sau 12 cm
            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ModelChecker.Root + "/Kitchen/SM_ThungPhu.fbx");
            Mesh m = go.GetComponentInChildren<MeshFilter>().sharedMesh;
            Transform t = go.GetComponentInChildren<MeshFilter>().transform;
            float front = 0f, back = 0f;
            foreach (Vector3 v in m.vertices)
            {
                Vector3 p = go.transform.InverseTransformPoint(t.TransformPoint(v));
                if (p.z < -0.2f) front = Mathf.Max(front, p.y);
                if (p.z > 0.2f) back = Mathf.Max(back, p.y);
            }
            Assert.Less(front, back, "thanh truoc (-z) thap hon thanh sau");
        }
    }
}
