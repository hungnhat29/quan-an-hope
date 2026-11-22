using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Hoat anh khach an (constitution 1.5 muc 6.2: khach an xong mon moi roi ghe).
    /// Doi dua gap tu dia len mieng theo nhip, mon tren dia voi dan theo tung mieng, khach nhai (than nhun),
    /// an xong dia trong. Tien do lay tu Core (GameSession.EatProgress) nen hinh luon khop luat.
    /// Tay khach cam dua (yeu cau 21/11/2026 "tay cua khach khong di chuyen"): tay vay (ArmWave) an di, thay bang tay
    /// ong cao su cong (Art Bible muc 6, 12: khong khuyu, cong theo duong cung): vai co dinh tren than, ban tay bam
    /// diem cam cua dua; gan thi ong cong nhieu, xa thi gian thang. Chay sau Lively (LateUpdate, thu tu 100) de vai theo than tho.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class EatingFx : MonoBehaviour
    {
        /// <summary>So mieng gap trong mot lan an (mon voi theo bac thang de nhin ro).</summary>
        public const int Bites = 4;

        /// <summary>Mot nhip gap: dua di len mieng va ve dia (giay).</summary>
        public const float BitePeriod = 1.1f;

        static readonly Color Chopstick = UiKit.Hex("#B97F52");
        static readonly HashSet<string> PlateParts = new HashSet<string> { "VienDia", "LongDia", "Outline" };

        Func<float> progress;
        Transform model, dish, mouth;
        readonly List<Transform> food = new List<Transform>();
        readonly List<Vector3> foodScale = new List<Vector3>();
        Transform sticks;
        Vector3 modelScale;
        float t;

        // ---- tay cam dua (ong cao su) ----
        /// <summary>Diem cam tren dua: tu tam dua len phia dau tren (dua dai 0,34 m).</summary>
        public const float GripUp = 0.14f;
        const int HoseRings = 14;
        const int HoseSides = 12;
        GameObject arm, armHand;            // tay vay cua canh (an khi dang an)
        Vector3 shoulderLocal;               // vai trong khung khoi khach (vong dau cua ong tay)
        float armLocalRadius = 0.09f, outlineLocal = 0.028f, handLocalSize = 0.36f, armLocalLength = 0.8f;
        Transform eatArm, hand;
        Mesh hose, hoseOutline;
        Vector3[] hoseVerts, outlineVerts;
        Vector3 bendPoint;

        /// <summary>Ban tay dang cam dua (null neu khoi khach khong co tay hoac da an xong).</summary>
        public Transform Hand { get { return hand != null && hand.gameObject.activeInHierarchy ? hand : null; } }
        /// <summary>Diem cam tren dua (the gioi).</summary>
        public Vector3 Grip { get { return sticks.position + sticks.up * GripUp; } }
        /// <summary>Vai (the gioi).</summary>
        public Vector3 Shoulder { get { return model.TransformPoint(shoulderLocal); } }
        /// <summary>Do cong cua ong tay: khoang cach diem giua duong cong toi duong thang vai - tay (m).</summary>
        public float Bend { get; private set; }
        /// <summary>
        /// Mat ngoai ong tay huong ra ngoai (test): phap tuyen tam giac dau (cach Unity tinh, Cross(b - a, c - a)) cung chieu
        /// voi huong tu truc ong ra dinh. Nguoc chieu = thay mat trong mau toi va vien phu len.
        /// </summary>
        public bool HoseFacesOutward
        {
            get
            {
                if (hose == null) return false;
                Vector3[] v = hose.vertices; int[] t = hose.triangles;
                Vector3 c = Vector3.zero;
                for (int i = 0; i < HoseSides; i++) c += v[i];
                c /= HoseSides;
                Vector3 n = Vector3.Cross(v[t[1]] - v[t[0]], v[t[2]] - v[t[0]]);
                return Vector3.Dot(n, v[t[0]] - c) > 0f;
            }
        }

        /// <summary>Tay vay goc cua khoi khach (an khi dang an, hien lai khi an xong).</summary>
        public GameObject OriginalArm { get { return arm; } }

        /// <summary>Phan mon con lai tren dia luc nay (1 = nguyen, 0 = da an het).</summary>
        public float FoodLeft { get; private set; }

        /// <summary>Dua dang o gan mieng (0..1, 1 = sat mieng).</summary>
        public float StickLift { get; private set; }

        public Transform Sticks { get { return sticks; } }

        /// <summary>
        /// Gan vao dia mon dang dat truoc khach. <paramref name="mouthPoint"/>: diem truoc mieng khach (the gioi).
        /// </summary>
        public static EatingFx Attach(GameObject dishModel, Transform customerModel, Transform mouthPoint, Func<float> progress)
        {
            var fx = dishModel.AddComponent<EatingFx>();
            fx.progress = progress;
            fx.model = customerModel;
            fx.dish = dishModel.transform;
            fx.mouth = mouthPoint;
            fx.modelScale = customerModel != null ? customerModel.localScale : Vector3.one;
            foreach (Transform child in dishModel.transform)
            {
                if (PlateParts.Contains(child.name)) continue;
                fx.food.Add(child);
                fx.foodScale.Add(child.localScale);
            }
            fx.BuildSticks();
            fx.BuildArm();
            fx.Apply(0f);
            return fx;
        }

        /// <summary>
        /// Tay vay cua khoi khach (ArmWave + HandWave) la tay gan dia; vai = tam vong dau cua ong tay (hinh that, L-016),
        /// do dai ong = do dai ong goc. Ong moi dung cung vat lieu va vien muc cua ong goc.
        /// </summary>
        void BuildArm()
        {
            if (model == null) return;
            Transform a = model.Find("ArmWave"), h = model.Find("HandWave");
            if (a == null || h == null) return;
            arm = a.gameObject;
            armHand = h.gameObject;
            var mf = a.GetComponent<MeshFilter>();
            Mesh m = mf != null ? mf.sharedMesh : null;
            if (m != null && m.isReadable && m.vertexCount >= 2 * HoseSides)
            {
                // BuildNhaHang.BuildTube: moi vong 12 dinh, vong dau quanh diem dau (vai), vong cuoi quanh co tay
                Vector3[] v = m.vertices;
                Vector3 c = Vector3.zero, end = Vector3.zero;
                for (int i = 0; i < HoseSides; i++) c += v[i];
                for (int i = v.Length - HoseSides; i < v.Length; i++) end += v[i];
                c /= HoseSides;
                end /= HoseSides;
                armLocalRadius = Mathf.Max(0.02f, (v[0] - c).magnitude);
                shoulderLocal = model.InverseTransformPoint(a.TransformPoint(c));
                // do dai theo duong ong (cong cac doan giua tam vong)
                float len = 0f;
                Vector3 prev = c;
                for (int r = 1; r < v.Length / HoseSides; r++)
                {
                    Vector3 cr = Vector3.zero;
                    for (int k = 0; k < HoseSides; k++) cr += v[r * HoseSides + k];
                    cr /= HoseSides;
                    len += (cr - prev).magnitude;
                    prev = cr;
                }
                armLocalLength = Mathf.Max(0.3f, len);
            }
            else shoulderLocal = model.InverseTransformPoint(a.GetComponent<Renderer>().bounds.center);
            Transform ol = a.Find("Outline");
            Material outlineMat = ol != null ? ol.GetComponent<Renderer>().sharedMaterial : null;
            Mesh om = ol != null ? ol.GetComponent<MeshFilter>().sharedMesh : null;
            if (om != null && om.isReadable && om.vertexCount >= HoseSides && m != null && m.isReadable)
                outlineLocal = Mathf.Max(0f, (om.vertices[0] - m.vertices[0]).magnitude);
            handLocalSize = h.localScale.x;
            Material hoseMat = a.GetComponent<Renderer>().sharedMaterial;
            Renderer hr = h.GetComponent<Renderer>();
            Material handMat = hr != null ? hr.sharedMaterial : null;

            // Sprint 9 F-080 AC-6: nhan vat Blender -> an tay TayPhai cua rig (khong phai khoi cu da tat), vai = xuong Arm_R,
            // do dai = vai -> ban tay cua rig, ong cung mau ao rig (chu no: vest).
            CharacterRig rig = null;
            foreach (CharacterRig cr in model.GetComponentsInChildren<CharacterRig>(false)) { rig = cr; break; }
            if (rig != null && rig.EatingArm != null)
            {
                arm = rig.EatingArm;
                armHand = rig.EatingArm;
                Transform sh = null, wr = null;
                foreach (Transform t in rig.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Arm_R") sh = t;
                    else if (t.name == "Hand_R") wr = t;
                }
                if (sh != null) shoulderLocal = model.InverseTransformPoint(sh.position);
                // do dai ong giu theo ong tay cu (~0,85 m): ong cao su dai hon xuong de cong thanh cung khi dua len mieng
                if (sh != null && wr != null) armLocalLength = Mathf.Max(armLocalLength, model.InverseTransformVector(wr.position - sh.position).magnitude * 1.3f);
                bool boss = rig.ModelName == CharacterRig.Boss;
                armLocalRadius = boss ? 0.1f : 0.09f;
                handLocalSize = boss ? 0.3f : 0.28f;
                hoseMat = rig.ShirtMaterial;
                handMat = Toon.Mat(Color.white);
            }

            // dat o goc canh: dinh tinh bang toa do the gioi, khong phu thuoc ti le / goc cua khoi khach
            eatArm = new GameObject("TayCamDua").transform;
            eatArm.SetParent(null, false);
            hose = NewHose("OngTay", hoseMat, out hoseVerts);
            if (outlineMat != null) hoseOutline = NewHose("Outline", outlineMat, out outlineVerts);
            hand = Toon.Prim(PrimitiveType.Sphere, eatArm, "BanTay", Vector3.zero, Vector3.one, Color.white, Toon.OutlineSmall).transform;
            if (handMat != null) hand.GetComponent<Renderer>().sharedMaterial = handMat;
        }

        Mesh NewHose(string name, Material mat, out Vector3[] verts)
        {
            var go = new GameObject(name);
            go.transform.SetParent(eatArm, false);
            var mesh = new Mesh { name = name };
            mesh.MarkDynamic();
            verts = new Vector3[HoseRings * HoseSides];
            var tris = new int[(HoseRings - 1) * HoseSides * 6];
            int t = 0;
            for (int i = 0; i < HoseRings - 1; i++)
                for (int k = 0; k < HoseSides; k++)
                {
                    int a0 = i * HoseSides + k, a1 = i * HoseSides + (k + 1) % HoseSides;
                    int b0 = a0 + HoseSides, b1 = a1 + HoseSides;
                    // mat ngoai quay ra ngoai: phap tuyen Cross(b - a, c - a) huong tu truc ong ra (HoseFacesOutward, test KhachAn_*).
                    // Thu tu nguoc (nhu BuildTube trong khung khoi khach) cho ong mau den: thay mat trong + vien phu len (anh 21/11).
                    tris[t++] = a0; tris[t++] = a1; tris[t++] = b0;
                    tris[t++] = a1; tris[t++] = b1; tris[t++] = b0;
                }
            mesh.vertices = verts;
            mesh.triangles = tris;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return mesh;
        }

        /// <summary>
        /// Ong tay Bezier bac hai vai -> ban tay; diem dieu khien lech ra ngoai than va xuong de ong dai bang ong goc
        /// (gan thi cong nhieu, xa qua thi thang va gian). Khong khuyu (Art Bible muc 6, 12).
        /// </summary>
        void PoseArm(bool holding)
        {
            if (eatArm == null) return;
            eatArm.gameObject.SetActive(holding);
            arm.SetActive(!holding);
            armHand.SetActive(!holding);
            if (!holding) return;

            float k = model.lossyScale.x;
            float len = armLocalLength * k;
            Vector3 s = Shoulder;
            Vector3 target = Grip;
            Vector3 d = target - s;
            float dist = d.magnitude;
            Vector3 dir = dist > 1e-5f ? d / dist : Vector3.down;
            Vector3 pole = model.TransformDirection(new Vector3(Mathf.Sign(shoulderLocal.x), -0.7f, 0.15f));
            pole -= dir * Vector3.Dot(pole, dir);
            pole = pole.sqrMagnitude > 1e-6f ? pole.normalized : Vector3.down;
            // cung parabol: do dai ~ dist + 8/3 * h^2 / dist  =>  h = sqrt(3/8 * dist * (len - dist))
            float h = dist < len ? Mathf.Sqrt(0.375f * dist * (len - dist)) : 0f;
            Bend = h;
            bendPoint = (s + target) * 0.5f + pole * h;
            Vector3 ctrl = (s + target) * 0.5f + pole * (2f * h);   // diem dieu khien Bezier: dinh cung = 1/2 do lech

            Matrix4x4 toLocal = eatArm.worldToLocalMatrix;
            float r = armLocalRadius * k, ro = r + outlineLocal * k;
            FillHose(hose, hoseVerts, s, ctrl, target, r, toLocal);
            if (hoseOutline != null) FillHose(hoseOutline, outlineVerts, s, ctrl, target, ro, toLocal);

            float pk = 1f / Mathf.Max(1e-4f, eatArm.lossyScale.x);
            hand.position = target;
            hand.localScale = Vector3.one * handLocalSize * 0.6f * k * pk;
        }

        static void FillHose(Mesh mesh, Vector3[] verts, Vector3 a, Vector3 c, Vector3 b, float radius, Matrix4x4 toLocal)
        {
            Vector3 normal = Vector3.zero;
            for (int i = 0; i < HoseRings; i++)
            {
                float u = i / (float)(HoseRings - 1);
                Vector3 p = (1 - u) * (1 - u) * a + 2 * (1 - u) * u * c + u * u * b;
                Vector3 tan = (2 * (1 - u) * (c - a) + 2 * u * (b - c));
                tan = tan.sqrMagnitude > 1e-10f ? tan.normalized : Vector3.up;
                if (i == 0)
                {
                    normal = Vector3.Cross(tan, Vector3.up);
                    if (normal.sqrMagnitude < 1e-4f) normal = Vector3.Cross(tan, Vector3.right);
                }
                normal = (normal - tan * Vector3.Dot(normal, tan)).normalized;   // khung di theo ong, khong xoan
                Vector3 bin = Vector3.Cross(tan, normal);
                for (int k = 0; k < HoseSides; k++)
                {
                    float ang = k / (float)HoseSides * Mathf.PI * 2f;
                    Vector3 w = p + (normal * Mathf.Cos(ang) + bin * Mathf.Sin(ang)) * radius;
                    verts[i * HoseSides + k] = toLocal.MultiplyPoint3x4(w);
                }
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        void BuildSticks()
        {
            var go = new GameObject("DoiDua");
            go.transform.SetParent(transform.parent, false);
            sticks = go.transform;
            for (int i = 0; i < 2; i++)
                Toon.Prim(PrimitiveType.Cylinder, sticks, "Dua_" + i, new Vector3(i == 0 ? -0.012f : 0.012f, 0f, 0f),
                    new Vector3(0.014f, 0.34f, 0.014f), Chopstick, 0.006f, new Vector3(0f, 0f, i == 0 ? 4f : -4f));
        }

        void LateUpdate()
        {
            t += Time.deltaTime;
            Apply(t);
        }

        void Apply(float time)
        {
            float p = progress != null ? Mathf.Clamp01(progress()) : 0f;
            // Mon voi theo tung mieng: con lai = so mieng chua an / Bites
            int eaten = Mathf.Min(Bites, Mathf.FloorToInt(p * Bites + 1e-4f));
            FoodLeft = 1f - eaten / (float)Bites;
            float k = FoodLeft <= 0f ? 0f : Mathf.Lerp(0.35f, 1f, FoodLeft);
            for (int i = 0; i < food.Count; i++)
            {
                if (food[i] == null) continue;
                food[i].gameObject.SetActive(FoodLeft > 0f);
                food[i].localScale = foodScale[i] * k;
            }

            // Dua: nhip len xuong; an het thi dat nam ngang canh dia.
            // dua di len: em vao em ra (ease in-out), dung lai o mieng mot chut
            float phase = (time % BitePeriod) / BitePeriod;
            float arc = Mathf.Sin(phase * Mathf.PI);
            StickLift = FoodLeft > 0f ? arc * arc * (3f - 2f * arc) : 0f;
            Vector3 low = dish.position + new Vector3(0f, 0.16f, 0f);
            Vector3 high = mouth != null ? mouth.position : low + Vector3.up * 0.6f;
            if (FoodLeft > 0f)
            {
                Vector3 mid = Vector3.Lerp(low, high, 0.5f) + Vector3.up * 0.12f;
                float s = StickLift;
                Vector3 pos = (1 - s) * (1 - s) * low + 2 * (1 - s) * s * mid + s * s * high;
                sticks.position = pos;
                Vector3 dir = (high - low).normalized;
                // dua nghieng tu dia huong len mieng, cang gan mieng cang nghieng
                sticks.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.Lerp(Vector3.up, -dir, 0.3f + 0.5f * s));
            }
            else
            {
                sticks.position = dish.position + new Vector3(0.2f, 0.03f, 0f);
                sticks.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            PoseArm(FoodLeft > 0f);

            // Khach nhai: than nhun nhe khi dua toi mieng (Lively nhun muot; khong co Lively thi nhun truc tiep).
            if (model != null)
            {
                float chew = FoodLeft > 0f ? Mathf.SmoothStep(0f, 1f, Mathf.Max(0f, StickLift - 0.4f) / 0.6f) : 0f;
                Lively l = model.GetComponent<Lively>();
                if (l != null) l.Chew = chew;
                else
                {
                    float bob = chew * (0.5f + 0.5f * Mathf.Sin(time * Mathf.PI * 4.4f)) * 0.045f;
                    model.localScale = new Vector3(modelScale.x * (1f + bob * 0.5f), modelScale.y * (1f - bob), modelScale.z * (1f + bob * 0.5f));
                }
            }
        }

        void OnDestroy()
        {
            Lively l = model != null ? model.GetComponent<Lively>() : null;
            if (l != null) l.Chew = 0f;
            else if (model != null) model.localScale = modelScale;
            if (sticks != null) Destroy(sticks.gameObject);
            if (eatArm != null) Destroy(eatArm.gameObject);
            if (hose != null) Destroy(hose);
            if (hoseOutline != null) Destroy(hoseOutline);
            // khong bat lai tay goc o day: khi ca canh dang huy, SetActive bao loi; RoomView goi Release() truoc khi xoa dia
        }

        /// <summary>Tra tay vay goc cho khoi khach (goi truoc khi xoa dia an).</summary>
        public void Release()
        {
            if (arm != null) arm.SetActive(true);
            if (armHand != null) armHand.SetActive(true);
            if (eatArm != null) eatArm.gameObject.SetActive(false);
        }
    }

    /// <summary>Khach an xong roi ghe: thu nho va bay len (0,5 giay). RoomView an khoi khach khi xong.</summary>
    public class LeaveFx : MonoBehaviour
    {
        public const float Seconds = 0.5f;
        float left = Seconds;
        Vector3 scale, pos;

        public static void Play(GameObject model)
        {
            if (model == null) return;
            Stop(model);
            Lively l = model.GetComponent<Lively>();
            if (l != null) { l.Leave(Seconds); return; }
            var fx = model.AddComponent<LeaveFx>();
            fx.scale = model.transform.localScale;
            fx.pos = model.transform.localPosition;
        }

        /// <summary>Dung giua chung (vd chu no vao ngay ghe vua trong): tra khoi khach ve dang cu.</summary>
        public static void Stop(GameObject model)
        {
            if (model == null) return;
            Lively l = model.GetComponent<Lively>();
            if (l != null && l.Leaving) l.CancelLeave();
            LeaveFx old = model.GetComponent<LeaveFx>();
            if (old == null) return;
            old.Restore();
            DestroyImmediate(old);
        }

        void Restore()
        {
            transform.localScale = scale;
            transform.localPosition = pos;
        }

        void Update()
        {
            left -= Time.deltaTime;
            float k = Mathf.Clamp01(left / Seconds);
            transform.localScale = scale * Mathf.Max(0.01f, k);
            transform.localPosition = pos + Vector3.up * (1f - k) * 0.4f;
        }

        void OnDisable()
        {
            Restore();
            Destroy(this);
        }
    }

    /// <summary>Dung dua quanh truc z cua diem treo (den long, den treo).</summary>
    public class Sway : MonoBehaviour
    {
        float degrees, period, phase;
        Quaternion rest;

        public void Init(float maxDegrees, float seconds, float startPhase)
        {
            degrees = maxDegrees;
            period = Mathf.Max(0.1f, seconds);
            phase = startPhase;
            rest = transform.localRotation;
        }

        public float Angle { get; private set; }

        void Update()
        {
            Angle = Mathf.Sin((Time.time + phase) * Mathf.PI * 2f / period) * degrees;
            transform.localRotation = rest * Quaternion.Euler(Angle * 0.4f, 0f, Angle);
        }
    }

    /// <summary>Troi qua lai cham theo truc x cua cha (may ngoai cua).</summary>
    public class Drift : MonoBehaviour
    {
        float speed, range, phase;
        Vector3 rest;

        public void Init(float metersPerSecond, float halfRange, float startPhase)
        {
            speed = metersPerSecond;
            range = Mathf.Max(0.1f, halfRange);
            phase = startPhase;
            rest = transform.localPosition;
        }

        void Update()
        {
            // di qua lai mem (sin), bien do halfRange, toc do toi da ~ speed
            float w = speed / range;
            transform.localPosition = rest + Vector3.right * Mathf.Sin((Time.time + phase) * w) * range;
        }
    }
}
