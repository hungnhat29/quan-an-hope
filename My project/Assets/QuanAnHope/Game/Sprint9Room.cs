using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Sprint 9 F-082 AC-2 (Q-66): Hope va con gai chu no la model tinh (SM_Hope, SM_ConGai). Tranh mo dau / ket thuc tu sprint 6 la
    /// trang truyen ve san nen hai nhan vat xuat hien trong phong: Hope dung sau quay o menu chinh (can phong nhin tu cua), man
    /// thang dung canh Hope + chu no (SK_ChuNo, dong tac Cho) + con gai o quay Goc 1 phia sau the ket qua.
    /// Mat 2D: chom cau mat cua khach (cung mesh voi canh) phong theo dau, anh FaceArt (Hope vui, con gai mat to mi cong, chu no ngac nhien).
    /// </summary>
    public partial class RoomView
    {
        GameObject menuHope, finale;

        /// <summary>
        /// Ti le nhan vat dung trong phong: Hope la nguoi choi (mat 1,75 m, Art Bible muc 9) -> cao ~1,9 m = 0,67 model 2,85 m.
        /// Khach o ghe dung CrowdScale rieng.
        /// </summary>
        public const float FigureScale = 0.67f;

        /// <summary>Hope o menu chinh (test).</summary>
        public GameObject MenuHope { get { return menuHope; } }
        /// <summary>Canh man thang (test).</summary>
        public GameObject Finale { get { return finale; } }

        /// <summary>Goi moi khung hinh ca khi o menu / truyen (GameController.Update).</summary>
        public void TickAlways() { TickSprint9(); }

        readonly System.Collections.Generic.HashSet<string> warmedFaces = new System.Collections.Generic.HashSet<string>();

        /// <summary>
        /// Ve truoc anh mat cua kieu mat khach dang ngoi, moi khung mot anh (anh 512 x 512 ve lan dau ton vai chuc ms: ve dung luc
        /// khach phan ung se lam mot khung cham, B-06). Khach di vao 4-6 giay du ve xong 14 anh.
        /// </summary>
        void PrewarmFaces()
        {
            foreach (SeatView v in seatViews)
            {
                if (v == null || v.Model == null || !v.Model.activeInHierarchy) continue;
                foreach (string mood in FaceArt.CustomerMoods)
                    for (int b = 0; b < 2; b++)
                        if (warmedFaces.Add(v.FaceStyle + "|" + mood + "|" + b))
                        {
                            FaceArt.Customer(mood, b == 1, v.FaceStyle);
                            return;
                        }
            }
        }

        void TickSprint9()
        {
            PrewarmFaces();
            bool menu = g.Mode == GameMode.Menu;
            if (menu && menuHope == null) menuHope = BuildMenuHope();
            // roi menu thi huy han (dung cho di cua nguoi choi ngay sau quay)
            if (!menu && menuHope != null) { Object.Destroy(menuHope); menuHope = null; }
        }

        GameObject BuildMenuHope()
        {
            GameObject root = Toon.Empty(goc1, "HopeMenu", Vector3.zero);
            // phia trong quay (quay ~ z 2,25-2,8 khung Goc 1), quay mat ra cua (+z) nhin camera menu
            if (Figure(root.transform, "SM_Hope", new Vector3(0.35f, 0f, 1.0f), FaceArt.Customer(FaceArt.MoodHappy, false, 0), 1.0f) == null)
            {
                Object.Destroy(root);
                return null;
            }
            return root;
        }

        /// <summary>Bat / tat canh man thang (Hud goi khi hien the thang).</summary>
        public void ShowFinale(bool on)
        {
            if (on && finale == null) finale = BuildFinale();
            if (finale != null && finale.activeSelf != on) finale.SetActive(on);
        }

        /// <summary>Camera menu nhin -z (phai man hinh = -x): dat ba nhan vat o +x (nua trai khung), the ket qua o nua phai (Hud).</summary>
        GameObject BuildFinale()
        {
            GameObject root = Toon.Empty(goc1, "CanhThang", Vector3.zero);
            Figure(root.transform, "SM_Hope", new Vector3(1.75f, 0f, 0.85f), FaceArt.Customer(FaceArt.MoodHappy, false, 0), 1.0f);
            Figure(root.transform, "SM_ConGai", new Vector3(0.35f, 0f, 0.85f), FaceArt.Customer(FaceArt.MoodHappy, false, 1), 0.92f);
            GameObject bossHost = Toon.Empty(root.transform, "ChuNo", new Vector3(1.05f, 0f, 0.5f), new Vector3(0f, 180f, 0f));
            bossHost.transform.localScale = Vector3.one * FigureScale;
            CharacterRig boss = CharacterRig.Create(bossHost.transform, CharacterRig.Boss);
            if (boss != null && boss.FacePoint != null) FaceQuad(boss.FacePoint, boss.transform, FaceArt.Boss(true), 0.84f);
            return root;
        }

        /// <summary>Model tinh + mat 2D dan len dau (chom cau cua canh neu co, khong thi tam phang).</summary>
        /// <summary>Nhan vat tinh khong co va cham (nguoi choi di xuyen qua duoc, khong chan duong).</summary>
        static void NoColliders(GameObject go)
        {
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        }

        GameObject Figure(Transform parent, string model, Vector3 localPos, Texture2D face, float headWidth)
        {
            GameObject host = Toon.Empty(parent, model, localPos, new Vector3(0f, 180f, 0f));
            host.transform.localScale = Vector3.one * FigureScale;
            GameObject m = ModelLibrary.TryInstantiate(CharacterRig.Group, model, host.transform, Toon.OutlineBig);
            if (m == null) { Object.Destroy(host); return null; }
            CharacterRig.Orient(m);
            Transform fp = ModelLibrary.FacePoint(m);
            if (fp != null)
            {
                Mesh cap = FaceCapMesh();
                if (cap != null)
                {
                    var go = new GameObject("Mat");
                    go.transform.SetParent(fp, false);
                    go.transform.rotation = host.transform.rotation;
                    // chom cau cua canh: hinh cau duong kinh 1 ve phia -z; tam chom = tam dau (sau diem Face mot ban kinh)
                    go.transform.localScale = new Vector3(headWidth * 1.02f, headWidth * 0.98f, headWidth * 0.98f);
                    go.transform.position = fp.position + host.transform.TransformDirection(Vector3.forward) * headWidth * 0.47f * host.transform.lossyScale.x;
                    go.AddComponent<MeshFilter>().sharedMesh = cap;
                    var r = go.AddComponent<MeshRenderer>();
                    r.sharedMaterial = FaceArt.Mat(face);
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                else FaceQuad(fp, host.transform, face, headWidth * 0.82f);
            }
            NoColliders(host);
            return host;
        }

        static void FaceQuad(Transform at, Transform facing, Texture2D face, float width)
        {
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Mat";
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(at, false);
            q.transform.rotation = facing.rotation;
            q.transform.localScale = new Vector3(width, width / 1.6f, 1f);
            q.transform.position = at.position + facing.TransformDirection(Vector3.back) * 0.012f;
            var r = q.GetComponent<MeshRenderer>();
            r.sharedMaterial = FaceArt.Mat(face);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>Mesh chom mat cua khach trong canh (BuildNhaHang.Cap), null neu khong co.</summary>
        Mesh FaceCapMesh()
        {
            foreach (SeatView v in seatViews)
                if (v != null && v.FaceCap != null)
                {
                    var mf = v.FaceCap.GetComponent<MeshFilter>();
                    if (mf != null) return mf.sharedMesh;
                }
            return null;
        }
    }
}
