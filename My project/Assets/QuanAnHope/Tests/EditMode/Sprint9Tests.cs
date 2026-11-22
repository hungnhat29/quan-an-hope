using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Data;
using QuanAnHope.EditorTools;
using QuanAnHope.Game;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.Tests
{
    /// <summary>
    /// Sprint 9 (specs/sprint-9/spec.md): B-03 vien, B-04 mon an Blender, B-05 duong di (luat chon ben), B-06 kieu mat, B-07 giai thich,
    /// F-080 than khach co xuong, F-081 dong tac, F-082 chu no / Hope / con gai. Phan nhin thay chay o PlayMode (Sprint9PlayTests).
    /// </summary>
    public class Sprint9Tests
    {
        static BalanceData data;
        static BalanceData Data { get { return data != null ? data : (data = AssetDatabase.LoadAssetAtPath<BalanceData>(BalanceImporter.AssetPath)); } }

        static List<ModelChecker.Result> all;
        static List<ModelChecker.Result> All { get { return all ?? (all = ModelChecker.CheckAll()); } }
        static ModelChecker.Result Get(string group, string name) { return All.FirstOrDefault(r => r.group == group && r.name == name); }

        const string Chars = "Assets/QuanAnHope/Resources/Models/Characters/";

        // ---------------- B-03 vien ----------------

        [Test]
        public void B03_AC1_VienLamMuot_DinhTrungViTriCungHuong_GocKhoiVuongCheo()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                Mesh src = cube.GetComponent<MeshFilter>().sharedMesh;
                Mesh m = BepNhanPham.OutlineMesh.Build(src);
                Assert.AreEqual(src.vertexCount, m.vertexCount);
                Assert.AreEqual(1, m.subMeshCount, "gop submesh: vat lieu vien ve het");
                Vector3[] v = m.vertices, n = m.normals;
                for (int i = 0; i < v.Length; i++)
                {
                    for (int j = 0; j < v.Length; j++)
                        if ((v[i] - v[j]).sqrMagnitude < 1e-8f) Assert.Less((n[i] - n[j]).magnitude, 1e-4f, "dinh trung vi tri dung chung phap tuyen (goc khong ho)");
                    // goc khoi vuong: huong cheo (1,1,1)/sqrt3
                    Vector3 d = new Vector3(Mathf.Sign(v[i].x), Mathf.Sign(v[i].y), Mathf.Sign(v[i].z)).normalized;
                    Assert.Greater(Vector3.Dot(d, n[i]), 0.99f, "goc khoi day ra theo duong cheo");
                }
            }
            finally { Object.DestroyImmediate(cube); }
        }

        [Test]
        public void B03_AC1_DoDayTheoDiemAnh_5pxNhanVat_3pxDaoCu_BoQuaVatNho()
        {
            Assert.AreEqual(5f, BepNhanPham.OutlineMesh.Px(Toon.OutlineBig), 0.05f);
            Assert.AreEqual(3.2f, BepNhanPham.OutlineMesh.Px(Toon.OutlineSmall), 0.1f);
            Assert.IsFalse(BepNhanPham.OutlineMesh.Wants(new Vector3(0.02f, 0.01f, 0.02f), Toon.OutlineSmall), "duoi nguong chi tiet: khong vien");
            Assert.IsTrue(BepNhanPham.OutlineMesh.Wants(new Vector3(0.8f, 0.02f, 0.4f), Toon.OutlineSmall), "tam mong to van co vien");
            Assert.IsNotNull(Shader.Find(BepNhanPham.OutlineMesh.ShaderName), "shader OutlinePx");
            Material mat = BepNhanPham.OutlineMesh.NewMaterial(Toon.OutlineBig);
            Assert.AreEqual(5f, mat.GetFloat("_WidthPx"), 0.05f);
            Assert.AreEqual(Toon.OutlineBig, mat.GetFloat("_WidthWorld"), 1e-4f, "khong day hon do day the gioi khi o xa");
        }

        // ---------------- B-04 mon an ----------------

        [Test]
        public void B04_AC1_DuModelMon_12Chinh_10Phu_4De_CoFace()
        {
            Assert.IsNotNull(Data, "BalanceData");
            foreach (MainIngredient m in Data.recipes.mains)
            {
                var r = Get("Dishes", "SM_Mon_" + m.id);
                Assert.IsNotNull(r, "mon chinh " + m.id);
                Assert.IsTrue(r.hasFace, m.id + ": empty Face (mat mon)");
                Assert.LessOrEqual(r.triangles, 1500, m.id);
            }
            foreach (SideIngredient s in Data.recipes.sides) Assert.IsNotNull(Get("Dishes", "SM_Kem_" + s.id), "do phu " + s.id);
            foreach (string b in new[] { "Com", "MiY", "Bun", "BanhMi" }) Assert.IsNotNull(Get("Dishes", "SM_De_" + b), "de mon " + b);
            Assert.AreEqual(12, Data.recipes.mains.Length);
            Assert.AreEqual(10, Data.recipes.sides.Length);
        }

        [Test]
        public void B04_AC2_MonChinh_ChiemItNhat45PhanTramLongDia()
        {
            foreach (MainIngredient m in Data.recipes.mains)
            {
                var r = Get("Dishes", "SM_Mon_" + m.id);
                float w = Mathf.Max(r.bounds.size.x, r.bounds.size.z);
                Assert.GreaterOrEqual(w / 0.28f, 0.45f, m.id + ": mon chinh rong " + w + " m tren long dia 0,28 m");
            }
        }

        // ---------------- B-05 duong di: ben via he theo hat giong ----------------

        [Test]
        public void B05_AC1_BenViaHe_TheoHatGiong_CoCaHaiBen()
        {
            var sides = Enumerable.Range(1, 40).Select(n => CustomerLooks.SideFor(n, 5, false)).ToList();
            Assert.IsTrue(sides.Contains(-1) && sides.Contains(1), "co khach den tu ben trai va ben phai");
            Assert.IsTrue(Enumerable.Range(1, 40).All(n => CustomerLooks.SideFor(n, 5, false) == CustomerLooks.SideFor(n, 5, false)), "tai lai ra nhu cu");
            Assert.GreaterOrEqual(RoomView.StreetX, 3.4f, "diem dau khuat sau tuong (|x| >= 3,4 m)");
        }

        // ---------------- B-06 kieu mat ----------------

        [Test]
        public void B06_AC1_TamKieuMat_KhacNhau_MoiKieuDu7BieuCam()
        {
            Assert.GreaterOrEqual(FaceArt.CustomerFaces, 8);
            foreach (string mood in FaceArt.CustomerMoods)
            {
                var hashes = new HashSet<string>();
                for (int f = 0; f < FaceArt.CustomerFaces; f++)
                {
                    Texture2D t = FaceArt.Customer(mood, false, f);
                    Assert.IsNotNull(t, mood + " kieu " + f);
                    hashes.Add(Hash(t));
                    Assert.IsNotNull(FaceArt.Customer(mood, true, f), "ban chop mat");
                }
                Assert.AreEqual(FaceArt.CustomerFaces, hashes.Count, "bieu cam " + mood + ": 8 kieu mat khac nhau");
            }
        }

        [Test]
        public void B06_AC1_MatVongTronRong_GiuaMatTrang_KheHoPhiaTren()
        {
            // Art Bible muc 6: mat vong tron rong (giua trang, khong trong den o bieu cam Vui), vien dam, ho mot khe phia tren
            for (int f = 0; f < FaceArt.CustomerFaces; f++)
            {
                Texture2D t = Readable(FaceArt.Customer(FaceArt.MoodHappy, false, f));
                FaceArt.FaceStyle st = FaceArt.Styles[f];
                foreach (float cx in new[] { 0.30f - st.spread, 0.70f + st.spread })
                {
                    Color mid = t.GetPixelBilinear(cx, 0.57f);
                    Assert.Greater(mid.r + mid.g + mid.b, 2.5f, "kieu " + f + ": giua mat trang (rong)");
                    float ry = 0.21f * st.eye * st.tall;
                    Color bottom = t.GetPixelBilinear(cx, 0.57f - ry);
                    Assert.Less(bottom.r + bottom.g + bottom.b, 1.2f, "kieu " + f + ": vien mat dam o day");
                }
            }
        }

        [Test]
        public void B06_AC3_KieuMatTheoHatGiong_HaiKhachLienNhauKhacKieu()
        {
            for (int day = 1; day <= 22; day++)
                for (int n = 2; n < 30; n++)
                    Assert.AreNotEqual(CustomerLooks.FaceFor(n - 1, day), CustomerLooks.FaceFor(n, day));
            Assert.AreEqual(8, Enumerable.Range(1, 16).Select(n => CustomerLooks.FaceFor(n, 4)).Distinct().Count(), "du 8 kieu mat");
        }

        /// <summary>Anh mat FaceArt khong doc duoc tu script (Apply khong giu du lieu): chep qua RenderTexture.</summary>
        static Texture2D Readable(Texture2D src)
        {
            RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            var t = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            t.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            t.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return t;
        }

        static string Hash(Texture2D t)
        {
            Color32[] px = Readable(t).GetPixels32();
            long h = 17;
            for (int i = 0; i < px.Length; i += 7) h = h * 31 + px[i].r * 3 + px[i].g * 5 + px[i].b * 7 + px[i].a;
            return h.ToString();
        }

        // ---------------- B-07 giai thich ----------------

        [Test]
        public void B07_AC1_GiaiThich_TiLe_Bac_DoDeTinh_SoTuDuLieu()
        {
            Balance b = Data.balance;
            string odds = GuideText.Odds(b, 1);
            int[] p = b.OddsForDay(1).percents;
            Assert.That(odds, Does.Contain("Ôi thiu " + p[0] + "%"));
            Assert.That(odds, Does.Contain("Bình thường " + p[1] + "%"));
            Assert.That(GuideText.IngredientTiersNote(), Does.Contain("Hiếm"));
            Assert.That(GuideText.DishNote(b), Does.Contain("0,6").And.Contain("0,4").And.Contain("0,8"));
            string req = GuideText.RequiredTable(b);
            foreach (RequiredTier r in b.customers.requiredTier) Assert.That(req, Does.Contain(UiKit.DishTierName[r.tier]));
            Assert.That(req, Does.StartWith("≥ 80%: Dở"));
            Assert.That(GuideText.EasinessNote(b), Does.Contain("Dễ tính").And.Contain("càng cao càng dễ"));
            string pay = GuideText.PaymentNote(b);
            Assert.That(pay, Does.Contain("50%").And.Contain("30%").And.Contain("sai món"));
            Assert.That(GuideText.PatienceNote(b, 4), Does.Contain(b.patience.dropEverySeconds.ToString()).And.Contain("120"));
        }

        [Test]
        public void B07_AC3_TheCachChoi_DuCacBang()
        {
            string sheet = GuideText.Sheet(Data.balance, 5);
            foreach (string part in new[] { "Tỉ lệ bậc nguyên liệu", "Bậc nguyên liệu", "Bậc món", "Ra bậc món", "Độ dễ tính", "tip", "khoảng an toàn" })
                Assert.That(sheet, Does.Contain(part));
            Assert.That(sheet, Does.Contain(GuideText.Odds(Data.balance, 5)), "ti le cua ngay hien tai");
        }

        // ---------------- F-080 / F-081 / F-082 nhan vat ----------------

        static float HeightOf(GameObject go, string part, out float partHeight)
        {
            Bounds all = new Bounds(), p = new Bounds();
            bool first = true, pf = true;
            foreach (SkinnedMeshRenderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                Enc(r.transform, r.sharedMesh, ref all, ref first, ref p, ref pf, r.name == part, !IsHair(r.name));
            foreach (MeshFilter f in go.GetComponentsInChildren<MeshFilter>(true))
                Enc(f.transform, f.sharedMesh, ref all, ref first, ref p, ref pf, f.name == part, true);
            partHeight = p.size.y;
            return all.max.y;
        }

        /// <summary>Toc / mu doi theo kieu khach (khong tinh vao chieu cao than, Art Bible muc 6); mu chu no ten "Mu" van tinh.</summary>
        static bool IsHair(string n) { return n.StartsWith("Toc_") || n.StartsWith("Mu_") || n == "Mam"; }

        static void Enc(Transform t, Mesh m, ref Bounds all, ref bool first, ref Bounds p, ref bool pf, bool isPart, bool count)
        {
            if (m == null) return;
            Bounds mb = m.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 w = t.TransformPoint(c);
                if (count) { if (first) { all = new Bounds(w, Vector3.zero); first = false; } else all.Encapsulate(w); }
                if (isPart) { if (pf) { p = new Bounds(w, Vector3.zero); pf = false; } else p.Encapsulate(w); }
            }
        }

        [Test]
        public void F080_AC1_MotRigKhach_12Den16Xuong_TiLeTheoDau()
        {
            var r = Get("Characters", "SK_Khach");
            Assert.IsNotNull(r, "SK_Khach.fbx");
            Assert.That(r.bones, Is.InRange(12, 16), "Art Bible muc 14");
            Assert.LessOrEqual(r.triangles, 6000);
            Assert.IsTrue(r.hasFace, "empty Face");
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Chars + "SK_Khach.fbx");
            float head;
            float h = HeightOf(go, "Dau", out head);
            Assert.That(h / head, Is.InRange(2f, 2.4f), "khach cao 2-2,4 dau (h " + h + ", dau " + head + ")");
            Assert.That(head / h, Is.InRange(0.43f, 0.5f), "dau ~45-50% chieu cao");
        }

        [Test]
        public void F080_F082_HuongModel_MatQuayVePhiaTruoc_TayPhaiODuongAm()
        {
            // khach / chu no nhin ve -z (nhu khoi khach cua canh, mat 2D cua canh dan o -z); TayPhai (tay cam dua) o Unity -x
            foreach (string n in new[] { "SK_Khach", "SK_ChuNo", "SM_Hope", "SM_ConGai" })
            {
                var holder = new GameObject("Giu");
                var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Chars + n + ".fbx"), holder.transform);
                CharacterRig.Orient(go);   // cach game dat model (CharacterRig.Create, RoomView.Figure)
                try
                {
                    Transform face = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "Face");
                    Vector3 f = holder.transform.InverseTransformPoint(face.position);
                    Assert.Less(f.z, -0.3f, n + ": Face o phia truoc (-z), dang o " + f);
                    if (!n.StartsWith("SK_")) continue;
                    // clip khong duoc xoay nguoc lai node goc (chi khoa xuong)
                    foreach (AnimationClip clip in AssetDatabase.LoadAllAssetsAtPath(Chars + n + ".fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")))
                    {
                        clip.SampleAnimation(go, 0f);
                        Assert.Less(holder.transform.InverseTransformPoint(face.position).z, -0.3f, n + " / " + clip.name + ": van nhin -z khi chay clip");
                    }
                    Transform arm = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "Arm_R");
                    Assert.Less(holder.transform.InverseTransformPoint(arm.position).x, -0.2f, n + ": vai phai o -x");
                    Assert.Greater(holder.transform.InverseTransformPoint(face.position).y, 1.8f, n + ": mat o tren cao");
                }
                finally { Object.DestroyImmediate(holder); }
            }
        }

        [Test]
        public void F080_AC2_BayKieuToc_Mu_CoTrongFBX_AoMauKhoa()
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Chars + "SK_Khach.fbx");
            var names = new HashSet<string>(go.GetComponentsInChildren<Transform>(true).Select(t => t.name));
            foreach (string hstyle in CharacterRig.HairStyles) Assert.IsTrue(names.Contains(hstyle), "kieu toc / mu " + hstyle);
            foreach (string part in new[] { "Than", "Dau", "TayTrai", "TayPhai", "Chan_L", "Chan_R", "Face", "Mieng" })
                Assert.IsTrue(names.Contains(part), part);
            Assert.GreaterOrEqual(CharacterRig.HairStyles.Length * CustomerLooks.Shirts.Length, 6, ">= 6 bien the");
            foreach (Color c in CustomerLooks.Shirts)
                foreach (Color tier in UiKit.IngredientTier.Concat(UiKit.DishTier))
                    Assert.IsFalse(Mathf.Abs(c.r - tier.r) < 0.01f && Mathf.Abs(c.g - tier.g) < 0.01f && Mathf.Abs(c.b - tier.b) < 0.01f,
                        "ao khong dung mau bac (Art Bible muc 4)");
        }

        [Test]
        public void F081_AC1_AC2_DongTac_ThoiLuong_NhunBaPhanTram_GiuDinh()
        {
            var r = Get("Characters", "SK_Khach");
            foreach (string c in new[] { "Cho", "Vui", "Gian", "VayChao", "Di" }) Assert.Contains(c, r.clips);
            var clips = AssetDatabase.LoadAllAssetsAtPath(Chars + "SK_Khach.fbx").OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToDictionary(c => c.name.Substring(c.name.LastIndexOf('|') + 1));
            Assert.AreEqual(2f, clips["Cho"].length, 0.05f, "dung cho lap 2 giay");
            Assert.That(clips["Vui"].length, Is.InRange(0.6f, 1.2f));
            Assert.That(clips["Gian"].length, Is.InRange(0.6f, 1.2f));

            var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Chars + "SK_Khach.fbx"));
            try
            {
                Transform head = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "Head");
                Transform hips = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "Hips");
                float lo = float.MaxValue, hi = float.MinValue;
                for (int k = 0; k <= 48; k++)
                {
                    clips["Cho"].SampleAnimation(go, k / 24f);
                    float y = head.position.y;
                    lo = Mathf.Min(lo, y); hi = Mathf.Max(hi, y);
                }
                float pct = (hi - lo) / 2f / 2.85f;
                Assert.That(pct, Is.InRange(0.015f, 0.045f), "nhun +-3% chieu cao (do " + pct + ")");
                // Vui: giu dinh khung 8-12 (2-4 khung, Art Bible muc 12)
                clips["Vui"].SampleAnimation(go, 8f / 24f);
                Vector3 a = hips.position;
                clips["Vui"].SampleAnimation(go, 12f / 24f);
                Assert.Less((hips.position - a).magnitude, 1e-3f, "giu tu the dinh");
                clips["Vui"].SampleAnimation(go, 0f);
                Assert.Greater(a.y - hips.position.y, 0.1f, "dinh nhay cao hon luc dau");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void F082_AC1_ChuNo_36Dau_DongTacRieng()
        {
            var r = Get("Characters", "SK_ChuNo");
            Assert.IsNotNull(r, "SK_ChuNo.fbx");
            Assert.LessOrEqual(r.triangles, 8000);
            Assert.That(r.bones, Is.InRange(12, 16));
            foreach (string c in new[] { "Cho", "Gian", "Di", "NgacNhien" }) Assert.Contains(c, r.clips);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Chars + "SK_ChuNo.fbx");
            float head;
            float h = HeightOf(go, "Dau", out head);
            Assert.AreEqual(3.6f, h / head, 0.2f, "chu no cao 3,6 dau ke ca mu");
        }

        [Test]
        public void F082_AC2_Hope24Dau_ConGai_TuTheTinh()
        {
            foreach (string n in new[] { "SM_Hope", "SM_ConGai" })
            {
                var r = Get("Characters", n);
                Assert.IsNotNull(r, n);
                Assert.IsTrue(r.Ok, string.Join("; ", r.errors));
                Assert.AreEqual(0, r.bones, n + " tinh");
            }
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Chars + "SM_Hope.fbx");
            float head;
            float h = HeightOf(go, "Dau", out head);
            Assert.AreEqual(2.4f, h / head, 0.2f, "Hope 2,4 dau");
        }
    }
}
