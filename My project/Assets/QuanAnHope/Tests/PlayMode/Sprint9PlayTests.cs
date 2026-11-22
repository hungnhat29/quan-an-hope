using System.Collections;
using System.Linq;
using NUnit.Framework;
using QuanAnHope.Core;
using QuanAnHope.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace QuanAnHope.PlayTests
{
    /// <summary>
    /// Gia lap nguoi choi sprint 9 (specs/sprint-9/spec.md): B-03 vien theo diem anh, B-04 mon an Blender, B-05 khach di bo tu via
    /// he, B-06 kieu mat, B-07 giai thich trong huong dan + the Cach choi, F-080 than khach co xuong, F-081 dong tac, F-082 chu no /
    /// Hope / con gai. Anh: docs/sprint-9/ui.
    /// </summary>
    public partial class PlaythroughTests
    {
        const string Sprint9 = "sprint-9";

        static Vector3 Goc1Local(Transform goc1, Vector3 world) { return goc1.InverseTransformPoint(world); }

        Transform Goc1 { get { return GameObject.Find("NhaHang").transform.Find("Goc1"); } }

        IEnumerator ServeFromFridge(int seat, Dish dish)
        {
            Assert.IsTrue(S.Fridge.TryStore(dish), "cat dia vao tu lanh");
            g.Room.Refresh();
            yield return null;
            Interactable inFridge = g.Room.FridgeDishes[g.Room.FridgeDishes.Count - 1].GetComponent<Interactable>();
            yield return Approach(inFridge);
            P.TapPrimary();
            yield return null;
            Assert.AreEqual(HandKind.Dish, g.Hand, "cam dia");
            yield return Use("Customer_" + seat);
        }

        // ---------------- B-03 vien ----------------

        [UnityTest]
        public IEnumerator B03_VienTheoDiemAnh_KhongPhongTo_AnhBepQuayThungDia()
        {
            // khoi cua canh (BuildNhaHang) va khoi luc chay (Toon) deu dung shader OutlinePx, ban sao cung kich thuoc
            Transform stove = GameObject.Find("NhaHang").transform.Find("Goc4/Stove");
            Assert.IsNotNull(stove);
            Renderer[] hulls = stove.GetComponentsInChildren<Renderer>(true).Where(r => r.name == "Outline").ToArray();
            Assert.Greater(hulls.Length, 0, "bep co vien");
            foreach (Renderer h in hulls)
            {
                Assert.AreEqual(BepNhanPham.OutlineMesh.ShaderName, h.sharedMaterial.shader.name, h.transform.parent.name);
                Assert.AreEqual(Vector3.one, h.transform.localScale, "vien khong con phong to theo met");
            }
            GameObject prim = Toon.Prim(PrimitiveType.Cube, null, "ThuVien", Vector3.zero, new Vector3(0.4f, 0.2f, 0.3f), Color.white, Toon.OutlineBig);
            Transform ol = prim.transform.Find("Outline");
            Assert.IsNotNull(ol);
            Assert.AreEqual(BepNhanPham.OutlineMesh.ShaderName, ol.GetComponent<Renderer>().sharedMaterial.shader.name);
            GameObject tiny = Toon.Prim(PrimitiveType.Sphere, null, "Hat", Vector3.zero, new Vector3(0.012f, 0.006f, 0.008f), Color.white, Toon.OutlineSmall);
            Assert.IsNull(tiny.transform.Find("Outline"), "vat duoi nguong chi tiet khong vien");
            Object.Destroy(prim);
            Object.Destroy(tiny);

            // do diem anh that (L-005): khoi trang truoc camera, ngay ngoai canh phai phai co net muc 2-4 px, ben trong van trang
            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 0f));
            Camera cam = P.Cam;
            GameObject probe = Toon.Prim(PrimitiveType.Cube, null, "DoVien", cam.transform.position + cam.transform.forward * 1.6f,
                new Vector3(0.3f, 0.3f, 0.3f), Color.white, Toon.OutlineBig);
            probe.transform.rotation = cam.transform.rotation;
            yield return null;
            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            cam.targetTexture = rt;
            // toa do diem anh tinh khi camera da gan render texture (cung ti le khung 16:9 voi anh doc)
            // canh truoc ben phai (gan camera nhat) la duong bao ngoai cung theo phoi canh
            Vector3 edge = cam.WorldToScreenPoint(probe.transform.position + probe.transform.right * 0.15f - probe.transform.forward * 0.15f);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            int ex = Mathf.RoundToInt(edge.x), ey = Mathf.RoundToInt(edge.y);
            float darkest = 9f;
            for (int dx = 0; dx <= 5; dx++) { Color c = tex.GetPixel(ex + dx, ey); darkest = Mathf.Min(darkest, c.r + c.g + c.b); }
            Color inside = tex.GetPixel(ex - 8, ey);
            for (int k = -12; k <= 12; k++) { tex.SetPixel(ex + k, ey - 6, Color.magenta); tex.SetPixel(ex, ey - 6 + k / 2, Color.magenta); }
            tex.Apply();
            string dbg = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "..", "docs", Sprint9, "ui", "b03_do_vien_debug.png"));
            System.IO.File.WriteAllBytes(dbg, tex.EncodeToPNG());
            Debug.Log("B03 do vien: canh " + ex + "," + ey + " ngoai " + darkest + " trong " + (inside.r + inside.g + inside.b));
            Object.Destroy(rt);
            Object.Destroy(tex);
            Object.Destroy(probe);
            Assert.Less(darkest, 0.9f, "net muc ngay ngoai canh khoi (vien hien that tren man hinh)");
            Assert.Greater(inside.r + inside.g + inside.b, 1.5f, "ben trong khoi khong bi vien de");
            Time.timeScale = FastForward;

            Time.timeScale = 1f;
            yield return WalkTo(StandFor(g.Room.Station("Stove").WorldCenter));
            yield return Shot("b03_bep.png", g.Room.Station("Stove").WorldCenter + new Vector3(0f, -0.2f, 0f), Sprint9);
            yield return Shot("b03_quay_cho.png", g.Room.Station("PlateStack").WorldCenter, Sprint9);
            yield return Use("Kiosk");
            yield return new WaitForSeconds(1.6f);
            yield return WalkTo(StandFor(g.Room.Station("Kiosk").WorldCenter));
            yield return Shot("b03_thung.png", g.Room.Station("Kiosk").WorldCenter + new Vector3(0.6f, -0.6f, 0f), Sprint9);
            // dia tren tay (gan mat 0,55 m): vien van manh
            yield return Use("PlateStack");
            Assert.AreEqual(HandKind.Plate, g.Hand);
            yield return Shot("b03_dia_tren_tay.png", null, Sprint9);
        }

        // ---------------- B-04 mon an ----------------

        [UnityTest]
        public IEnumerator B04_MonAn_ModelBlender_MonChinhTo_DePhiaSau_AnhMuoiHaiMon()
        {
            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 0f));
            P.LookAt(P.Cam.transform.position + new Vector3(0f, -1.2f, -1f));
            yield return null;
            Transform cam = P.Cam.transform;
            var root = new GameObject("BayMon").transform;
            Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, fwd);
            root.position = cam.position + fwd * 1.05f + Vector3.down * 1.05f;
            root.rotation = Quaternion.LookRotation(fwd);
            MainIngredient[] mains = S.Recipes.mains;
            string[] sides = { "ca_chua", "hanh_tay", "rau_muong", "khoai_tay", "nam_rom", "bong_cai", "ot_chuong", "bi_do", "mang_tay", "nam_dong_co", "ca_chua", "hanh_tay" };
            for (int i = 0; i < mains.Length; i++)
            {
                Recipe r = S.Lookup.Find(mains[i].id, sides[i]);
                Assert.IsNotNull(r, mains[i].id + "__" + sides[i]);
                var dish = new Dish { recipeId = r.id, name = r.name, baseName = r.baseName, tier = i == 0 ? 0 : 2 + i % 3 };
                var slot = new GameObject("O_" + i).transform;
                slot.SetParent(root, false);
                slot.localPosition = new Vector3(((i % 4) - 1.5f) * 0.42f, 0f, (1 - i / 4) * 0.4f);
                slot.localRotation = Quaternion.Euler(0f, 180f, 0f);
                GameObject plate = DishModel.Create(slot, dish, S.Recipes);
                Transform food = plate.transform.Find("MonAn");
                Assert.IsNotNull(food, "mon hoan chinh");
                Assert.IsNotNull(plate.transform.Find("DeMon"), "de mon");
                Transform main = food.Find("MonChinh");
                Assert.IsNotNull(main, mains[i].id + ": model Blender mon chinh");
                Bounds b = ModelLibrary.LocalBounds(main.gameObject, plate.transform);
                Assert.GreaterOrEqual(Mathf.Max(b.size.x, b.size.z) / 0.28f, 0.45f, mains[i].id + ": mon chinh >= 45% long dia");
                if (i == 0)
                {
                    // Lac vi: mau xam nau (F-013 AC-6)
                    Color c = main.GetComponentInChildren<Renderer>().sharedMaterial.GetColor("_Color");
                    Assert.Less(c.r + c.g + c.b, 1.4f, "lac vi xam toi");
                }
            }
            yield return new WaitForSeconds(0.4f);
            yield return Shot("b04_muoi_hai_mon.png", null, Sprint9);
            yield return Shot("b04_mon_gan.png", root.position + right * -0.21f + fwd * 0.2f, Sprint9);
            Object.Destroy(root.gameObject);
        }

        // ---------------- B-05 di bo tu via he ----------------

        [UnityTest]
        public IEnumerator B05_KhachDiTuViaHe_ReVao_AnXong_ReTraiPhai_KhuatMoiAn()
        {
            RoomView.WalkCustomers = true;
            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 0.2f + BepNhanPham.RoomSize.Grow));
            yield return Use("ShopSign");
            yield return null;
            Customer c = S.CurrentCustomer;
            Assert.IsNotNull(c);
            RoomView.SeatView v = g.Room.SeatViews[0];
            Lively l = v.Motion;
            Assert.IsTrue(l.WalkingIn);
            Assert.LessOrEqual(l.WalkInSeconds, 6.5f, "tong <= 6,5 giay");
            Assert.GreaterOrEqual(l.WalkInSeconds, 3f, "khong qua nhanh");
            Vector3 start = Goc1Local(Goc1, v.Model.transform.TransformPoint(l.Pivot));
            Assert.AreEqual(RoomView.StreetZ, start.z, 0.15f, "bat dau tren via he");
            Assert.GreaterOrEqual(Mathf.Abs(start.x), 3.4f, "bat dau khuat sau tuong");
            Assert.AreEqual(CustomerLooks.SideFor(c.number, S.Day, false), (int)Mathf.Sign(start.x), "ben theo hat giong");
            Assert.IsFalse(v.Bubble.gameObject.activeSelf);
            if (v.Rig != null) Assert.AreEqual(CharacterRig.ClipWalk, v.Rig.CurrentClip, "dang di: clip Di");
            // doi toi truoc cua: luc do da re vao (x gan cua, z giua via he va nguong)
            bool turned = false;
            float t0 = Time.time;
            while (l.WalkingIn && Time.time - t0 < 9f)
            {
                Vector3 p = Goc1Local(Goc1, v.Model.transform.TransformPoint(l.Pivot));
                if (!turned && Mathf.Abs(p.x) < 1f && p.z < RoomView.StreetZ - 0.4f && p.z > 4.1f)
                {
                    turned = true;
                    yield return Shot("b05_khach_re_vao_cua.png", v.Model.transform.TransformPoint(l.Pivot) + Vector3.up * 1.4f, Sprint9);
                }
                yield return null;
            }
            Assert.IsTrue(turned, "re tu via he vao cua");
            Assert.IsFalse(l.WalkingIn, "toi ghe");
            yield return null;
            Assert.IsTrue(v.Bubble.gameObject.activeSelf, "ngoi xong hien bong bong");
            if (v.Rig != null) Assert.Greater(v.Rig.Played.ContainsKey(CharacterRig.ClipWave) ? v.Rig.Played[CharacterRig.ClipWave] : 0, 0, "vua ngoi: vay chao");

            Recipe r = S.Lookup.ById(c.orderId);
            var dish = new Dish { recipeId = r.id, name = r.name, baseName = r.baseName, tier = 3 };
            S.Fridge.TryStore(dish);
            Assert.IsNotNull(S.ServeSeat(0, dish));
            yield return WaitUntil(() => v.Leaver != null && v.Leaver.WalkingOut, S.Balance.day.eatSeconds + 3f, "an xong thi di ra");
            l = v.Leaver;
            GameObject leaver = l.gameObject;
            int outSide = CustomerLooks.SideFor(c.number, S.Day, true);
            float sideSeen = 0f;
            Vector3 last = Vector3.zero;
            bool shotOut = false;
            t0 = Time.time;
            while (leaver != null && Time.time - t0 < 12f)
            {
                last = Goc1Local(Goc1, leaver.transform.TransformPoint(l.Pivot));
                if (last.z > RoomView.StreetZ - 0.2f) sideSeen = last.x;
                if (!shotOut && last.z > RoomView.StreetZ - 0.2f && Mathf.Abs(last.x) > 1.2f)
                {
                    shotOut = true;
                    yield return Shot("b05_khach_di_doc_via_he.png", leaver.transform.TransformPoint(l.Pivot) + Vector3.up * 1.4f, Sprint9);
                    if (leaver == null) break;
                }
                if (!l.GoneOut) Assert.Greater(leaver.transform.lossyScale.x, 0.3f, "khong thu nho dan nua (an khi da khuat)");
                yield return null;
            }
            Assert.AreEqual(outSide, (int)Mathf.Sign(sideSeen), "re dung ben theo hat giong");
            Assert.GreaterOrEqual(Mathf.Abs(last.x), 3.3f, "chi an khi da khuat sau tuong");
        }

        // ---------------- F-080 / B-06 / F-081: ba khach co xuong ----------------

        IEnumerator ThreeSeated()
        {
            yield return ToDay(4, 2000);
            RoomView.WalkCustomers = false;
            yield return Use("ShopSign");
            g.ClockScale = 8f;
            yield return WaitUntil(() => S.Seats.Take(3).All(x => x != null), 200f, "ba khach ngoi");
            g.ClockScale = 0f;
            yield return null;
        }

        static string Widest(CharacterRig rig)
        {
            string best = "";
            float w = 0f;
            var tmp = new Mesh();
            foreach (SkinnedMeshRenderer r in rig.GetComponentsInChildren<SkinnedMeshRenderer>(false))
            {
                if (r.name == "Outline") continue;
                r.BakeMesh(tmp);
                float lo = 9f, hi = -9f;
                foreach (Vector3 v in tmp.vertices) { float x = r.transform.TransformPoint(v).x; lo = Mathf.Min(lo, x); hi = Mathf.Max(hi, x); }
                if (hi - lo > w) { w = hi - lo; best = r.name + " " + lo.ToString("0.00") + ".." + hi.ToString("0.00"); }
            }
            Object.Destroy(tmp);
            return best + " goc " + rig.transform.position.x.ToString("0.00");
        }

        [UnityTest]
        public IEnumerator F080_BaKhach_CoXuong_KieuKhacNhau_MatTrenDau_KhongLongNhau_60Hinh()
        {
            yield return ThreeSeated();
            var rigs = new CharacterRig[3];
            for (int i = 0; i < 3; i++)
            {
                RoomView.SeatView v = g.Room.SeatViews[i];
                Customer c = S.Seats[i];
                Assert.IsNotNull(v.Rig, "F-080: nhan vat Blender (khong phai khoi cu)");
                Assert.IsTrue(v.Rig.gameObject.activeInHierarchy);
                rigs[i] = v.Rig;
                int look = CustomerLooks.LookFor(c.number, S.Day);
                Assert.AreEqual(CharacterRig.HairStyles[look], v.Rig.Hair, "AC-2 toc / mu theo kieu");
                Color shirt = CustomerLooks.ShirtFor(c.number, S.Day);
                Assert.AreEqual(shirt, v.Rig.Shirt, "AC-2 ao theo hat giong");
                Assert.AreEqual(CustomerLooks.FaceFor(c.number, S.Day), v.FaceStyle, "B-06 kieu mat theo hat giong");
                Assert.IsTrue(v.FaceCap.transform.IsChildOf(v.Rig.Head), "AC-3 mat dan tren xuong dau");
                Assert.Less(Vector3.Distance(v.FaceCap.bounds.center, v.Rig.Head.position + Vector3.up * 0.6f * v.Pivot.lossyScale.y), 0.6f, "mat dung vung dau");
                Assert.Less(Vector3.Distance(v.Bubble.transform.position, v.Rig.Head.position), 2f, "AC-4 bong bong canh dau");
                foreach (Renderer r in v.Model.GetComponentsInChildren<Renderer>())
                    if (!r.transform.IsChildOf(v.Rig.transform) && r != v.FaceCap && r.name != "Bong")
                        Assert.IsFalse(r.enabled && r.gameObject.activeInHierarchy && r.transform.parent == v.Model.transform, "khoi cu da an: " + r.name);
            }
            Assert.AreNotEqual(rigs[0].Hair + rigs[0].Shirt, rigs[1].Hair + rigs[1].Shirt);
            // AC-5: ba khach khong long nhau (hop bao luc dung cho)
            for (int i = 0; i < 2; i++)
            {
                Bounds a = rigs[i].BakedBounds(), b = rigs[i + 1].BakedBounds();
                Assert.Less(a.max.x, b.min.x + 0.02f, "khach " + i + " va " + (i + 1) + " khong long nhau: " + Widest(rigs[i]) + " | " + Widest(rigs[i + 1])
                    + " | clip " + rigs[i].CurrentClip + " " + rigs[i].Hair + " / " + rigs[i + 1].CurrentClip + " " + rigs[i + 1].Hair);
            }
            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 1.2f));
            yield return Shot("f080_ba_khach.png", g.Room.SeatViews[1].Pivot.position + Vector3.up * 1.5f, Sprint9);
            // khung hinh >= 60 / giay voi 3 khach (impl plan muc 4)
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            float sum = 0f;
            int frames = 0;
            yield return null;
            for (int k = 0; k < 120; k++) { yield return null; sum += Time.unscaledDeltaTime; frames++; }
            float fps = frames / sum;
            Debug.Log("F080 khung hinh / giay voi 3 khach: " + fps.ToString("0"));
            Assert.GreaterOrEqual(fps, 60f, "khung hinh / giay");

            // B-06: 8 kieu mat + 7 bieu cam (chup)
            for (int f = 0; f < FaceArt.CustomerFaces; f += 3)
            {
                for (int s = 0; s < 3; s++)
                    g.Room.SeatViews[s].Face.Show(FaceArt.Customer(FaceArt.MoodHappy, false, Mathf.Min(f + s, FaceArt.CustomerFaces - 1)));
                yield return null;
                yield return Shot("b06_kieu_mat_" + (f / 3 + 1) + ".png", g.Room.SeatViews[1].Pivot.position + Vector3.up * 1.6f, Sprint9);
            }
            foreach (string mood in FaceArt.CustomerMoods)
            {
                for (int s = 0; s < 3; s++) g.Room.SeatViews[s].Face.Show(FaceArt.Customer(mood, false, g.Room.SeatViews[s].FaceStyle));
                yield return null;
                yield return Shot("f080_bieu_cam_" + mood + ".png", g.Room.SeatViews[1].Pivot.position + Vector3.up * 1.6f, Sprint9);
            }
        }

        [UnityTest]
        public IEnumerator F081_DongTac_Cho_Vui_Gian_VayChao_TheoKetQua_TrenHai()
        {
            yield return ThreeSeated();
            RoomView.SeatView v0 = g.Room.SeatViews[0];
            Assert.IsNotNull(v0.Rig);
            Assert.AreEqual(CharacterRig.ClipWait, v0.Rig.CurrentClip, "AC-1 dung cho");
            Assert.AreEqual(2f, v0.Rig.ClipLength(CharacterRig.ClipWait), 0.05f);
            // chup tung dong tac o dinh
            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 1.2f));
            foreach (string clip in new[] { CharacterRig.ClipHappy, CharacterRig.ClipAngry, CharacterRig.ClipWave })
            {
                v0.Rig.Play(clip);
                g.Room.SeatViews[1].Rig.Play(clip);
                g.Room.SeatViews[2].Rig.Play(clip);
                yield return new WaitForSeconds(Mathf.Min(0.45f, v0.Rig.ClipLength(clip) * 0.45f));
                Assert.AreEqual(clip, v0.Rig.CurrentClip);
                yield return Shot("f081_" + clip + ".png", g.Room.SeatViews[1].Pivot.position + Vector3.up * 1.4f, Sprint9);
                yield return new WaitForSeconds(v0.Rig.ClipLength(clip));
                Assert.AreEqual(CharacterRig.ClipWait, v0.Rig.CurrentClip, "xong thi ve dung cho");
            }
            // AC-3 "tren 2": bat Stepped thi thoi gian clip la boi cua 1/12 giay
            CharacterRig.Stepped = true;
            try
            {
                for (int k = 0; k < 10; k++)
                {
                    yield return null;
                    float q = v0.Rig.CurrentTime * CharacterRig.StepFps;
                    Assert.AreEqual(Mathf.Round(q), q, 1e-3f, "lay mau theo buoc 1/12 giay");
                }
            }
            finally { CharacterRig.Stepped = false; }

            // AC-2: nhan mon dung luc tra tien -> Vui; sai mon -> Gian
            g.ClockScale = 1f;
            Time.timeScale = FastForward;
            Customer c0 = S.Seats[0];
            Recipe r0 = S.Lookup.ById(c0.orderId);
            int happy0 = v0.Rig.Played.ContainsKey(CharacterRig.ClipHappy) ? v0.Rig.Played[CharacterRig.ClipHappy] : 0;
            yield return ServeFromFridge(0, new Dish { recipeId = r0.id, name = r0.name, baseName = r0.baseName, tier = 4 });
            yield return Frames(3);
            Assert.Greater(v0.Rig.Played[CharacterRig.ClipHappy], happy0, "dat yeu cau: Vui");
            RoomView.SeatView v1 = g.Room.SeatViews[1];
            Customer c1 = S.Seats[1];
            Recipe other = S.BookRecipes().First(x => x.id != c1.orderId);
            int angry1 = v1.Rig.Played.ContainsKey(CharacterRig.ClipAngry) ? v1.Rig.Played[CharacterRig.ClipAngry] : 0;
            yield return ServeFromFridge(1, new Dish { recipeId = other.id, name = other.name, baseName = other.baseName, tier = 4 });
            yield return Frames(3);
            Assert.Greater(v1.Rig.Played[CharacterRig.ClipAngry], angry1, "sai mon: Gian");
            Assert.IsNotNull(v1.Eating, "dang an");
            Assert.IsFalse(v1.Rig.EatingArm.activeSelf, "F-080 AC-6: tay rig an khi ong tay cam dua hien");
        }

        // ---------------- F-082 chu no ----------------

        [UnityTest]
        public IEnumerator F082_ChuNo_ModelRieng_LanhLung_MonTuyetHao_NgacNhien()
        {
            yield return StartAt(21, 25000, BotRelicsDay21);
            RoomView.WalkCustomers = false;
            yield return Use("ShopSign");
            g.ClockScale = 60f;
            yield return WaitUntil(() => S.BossArrived, 120f, "20:00 chu no toi");
            g.ClockScale = 0f;
            if (S.Boss == null)
            {
                int seatB = 1;
                Customer cb = S.Seats[seatB];
                Recipe rb = S.Lookup.ById(cb.orderId);
                var db = new Dish { recipeId = rb.id, name = rb.name, baseName = rb.baseName, tier = 3 };
                Assert.IsTrue(S.Fridge.TryStore(db));
                Assert.IsNotNull(S.ServeSeat(seatB, db));
                g.ClockScale = 1f;
                yield return WaitUntil(() => S.Boss != null, S.Balance.day.eatSeconds + 3f, "chu no vao ghe");
                g.ClockScale = 0f;
            }
            yield return Frames(2);
            int seat = S.SeatOf(S.Boss);
            RoomView.SeatView v = g.Room.SeatViews[seat];
            Assert.IsNotNull(v.BossRig, "SK_ChuNo");
            Assert.IsTrue(v.BossRig.gameObject.activeInHierarchy, "chu no la model rieng");
            Assert.IsFalse(v.Rig.gameObject.activeInHierarchy, "an than khach");
            Assert.AreSame(v.BossFace, v.Face.faceRenderer, "mat dan tren dau chu no");
            Time.timeScale = 1f;
            yield return WalkTo(new Vector3(0f, 0f, 0.6f + BepNhanPham.RoomSize.Grow));
            yield return Shot("f082_chu_no_lanh_lung.png", v.Pivot.position + Vector3.up * 1.8f, Sprint9);
            Time.timeScale = FastForward;
            int surprised = v.BossRig.Played.ContainsKey(CharacterRig.ClipSurprised) ? v.BossRig.Played[CharacterRig.ClipSurprised] : 0;
            yield return ServeFromFridge(seat, new Dish { recipeId = "ga__bi_do", name = "Gà nướng mật ong bí đỏ", baseName = "Bánh mì", tier = 4 });
            yield return Frames(2);
            bool bonus;
            Assert.IsTrue(g.IsBossReaction(seat, out bonus) && bonus, "Tuyet hao: thuong");
            Assert.Greater(v.BossRig.Played[CharacterRig.ClipSurprised], surprised, "dong tac ngac nhien");
            Time.timeScale = 1f;
            yield return Shot("f082_chu_no_ngac_nhien.png", v.Pivot.position + Vector3.up * 1.8f, Sprint9);
        }

        [UnityTest]
        public IEnumerator F082_Hope_MenuChinh_CanhThang_HopeChuNoConGai()
        {
            g.ShowMainMenu();
            yield return Frames(2);
            Assert.IsNotNull(g.Room.MenuHope, "Hope o menu chinh");
            Assert.IsTrue(g.Room.MenuHope.activeInHierarchy);
            Time.timeScale = 1f;
            yield return ShotAll("f082_menu_hope.png");
            Time.timeScale = FastForward;
            yield return StartAndJump(21, 40000);
            Assert.IsTrue(S.IsWon);
            g.Menu.SkipStory();
            yield return Frames(3);
            Assert.AreEqual(GameMode.Playing, g.Mode);
            Assert.IsTrue(g.Room.MenuHope == null, "het menu thi Hope menu duoc huy");
            Assert.IsNotNull(g.Room.Finale, "canh thang");
            Assert.IsTrue(g.Room.Finale.activeInHierarchy);
            Assert.IsTrue(P.CameraOverride, "camera nhin canh thang");
            foreach (string n in new[] { "SM_Hope", "SM_ConGai", "ChuNo" }) Assert.IsNotNull(g.Room.Finale.transform.Find(n), n);
            Time.timeScale = 1f;
            yield return Shot("f082_man_thang.png", null, Sprint9);
        }

        // ---------------- B-07 giai thich ----------------

        [UnityTest]
        public IEnumerator B07_HuongDan_GiaiThichTiLeBacDoDeTinh_TheCachChoi()
        {
            g.Tutorial.Begin(true);
            yield return Frames(3);
            Assert.AreEqual(Tutorial.Day1, g.Tutorial.LessonId);
            string note = g.Hud.TutorialNote;
            Assert.IsNotNull(note, "AC-1: o giai thich hien cung buoc");
            StringAssert.Contains("tỉ lệ bậc", note);
            StringAssert.Contains(GuideText.Odds(S.Balance, S.Day), note);
            Time.timeScale = 1f;
            yield return Shot("b07_giai_thich_ti_le.png", g.Room.Station("Kiosk").WorldCenter, Sprint9);
            yield return Frames(2);
            Assert.GreaterOrEqual(g.Hud.TutorialNoteLabel.fontSize, 14f, "AC-4 chu >= 14 px");
            Time.timeScale = FastForward;
            // buoc mo quan / phuc vu: giai thich do de tinh + tra tien
            var steps = g.Tutorial.Lessons.First(l => l.id == Tutorial.Day1).steps;
            Assert.AreEqual(10, steps.Count, "van 10 buoc lam");
            string serve = steps[9].note();
            StringAssert.Contains("Dễ tính", serve);
            StringAssert.Contains("trừ", serve);
            StringAssert.Contains("Dải 6 ô", steps[5].note());
            StringAssert.Contains("Đế".ToLower(), steps[1].note().ToLower());

            // AC-3 Tab -> Cach choi
            g.OpenTab(TabMenu.Page.Relic);
            yield return null;
            g.Tab.GuideTab.onClick.Invoke();
            yield return null;
            Assert.AreEqual(TabMenu.Page.Guide, g.Tab.Current);
            string sheet = g.Tab.GuideLabel.text;
            StringAssert.Contains("Độ dễ tính", sheet);
            StringAssert.Contains(GuideText.RequiredTable(S.Balance), sheet);
            Time.timeScale = 1f;
            yield return Shot("b07_tab_cach_choi.png", null, Sprint9);
            Assert.GreaterOrEqual(g.Tab.GuideLabel.fontSize, 14f, "chu the Cach choi >= 14 px");
            g.CloseTab();
        }
    }
}

