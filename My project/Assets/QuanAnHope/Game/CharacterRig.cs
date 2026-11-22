using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Nhan vat Blender co xuong (sprint 9 F-080, F-081; Art Bible muc 6, 12): SK_Khach (mot than cho moi khach) va SK_ChuNo.
    /// - Clip (Animation legacy, ten "Rig|Cho"...): Cho (lap 2 giay), Di (lap, khi Lively dang di), mot lan: Vui, Gian, VayChao,
    ///   NgacNhien. Thoi gian clip do script dieu khien: mac dinh noi suy muot (yeu cau 20/11 "muot nhu cot truyen"),
    ///   <see cref="Stepped"/> = true thi lay mau theo buoc 1/12 giay ("tren 2", Art Bible muc 12; Q-67).
    /// - Toc / mu: cac mesh ten Toc_*, Mu_*, Mam bat tat theo ten. Ao: vat lieu mau Hong rem (#E9A0A0) trong FBX doi mau theo khach.
    /// - Diem: Face, Mieng (empty tren xuong dau), xuong Head, tay cam dua TayPhai (Unity -x, EatingFx an no khi an).
    /// Thieu FBX: Create tra ve null, noi goi giu khoi khach cu (impl plan sprint 9 "phuong an khi tre").
    /// </summary>
    public class CharacterRig : MonoBehaviour
    {
        public const string Customer = "SK_Khach", Boss = "SK_ChuNo", Group = "Characters";
        public const string ClipWait = "Cho", ClipWalk = "Di", ClipHappy = "Vui", ClipAngry = "Gian", ClipWave = "VayChao", ClipSurprised = "NgacNhien";
        public static readonly string[] HairStyles = { "Toc_BuiBac", "Mu_LuoiTrai", "Toc_HaiBui", "Mu_Non", "Mu_Noi", "Mam", "Toc_Ngan" };
        public static readonly Color ShirtKey = UiKit.Hex("#E9A0A0");
        public const float StepFps = 12f;

        /// <summary>
        /// FBX nhan vat (khong bake transform vi co xuong) qua "Bake Axis Conversion" cua Unity nhin ve +z; khoi khach cua canh va mat 2D
        /// nhin -z. Model dat duoi mot node chua va xoay 180 do (clip khoa ca node Rig nen khong xoay luc nhap duoc).
        /// </summary>
        public const float ImportYaw = 180f;

        /// <summary>Dat model Blender nhan vat (da Instantiate) dung huong game: xoay quanh truc dung trong node cha.</summary>
        public static void Orient(GameObject model)
        {
            if (model != null) model.transform.localRotation = Quaternion.Euler(0f, ImportYaw, 0f);
        }

        /// <summary>Lay mau clip theo buoc 1/12 giay (Art Bible muc 12). Mac dinh tat: muot theo yeu cau 20/11 (Q-48, Q-67).</summary>
        public static bool Stepped;

        /// <summary>Ten model (giu qua Instantiate: ban sao khach di ra tu khoi lai rig, sprint 9 B-05).</summary>
        [SerializeField] string modelName;

        Animation anim;
        readonly Dictionary<string, string> clipNames = new Dictionary<string, string>();
        readonly List<KeyValuePair<Renderer, int>> shirtSlots = new List<KeyValuePair<Renderer, int>>();
        readonly Dictionary<string, GameObject> hair = new Dictionary<string, GameObject>();
        Lively motion;
        string oneShot;
        float oneT, loopT;

        public string ModelName { get; private set; }
        public Transform Head { get; private set; }
        public Transform FacePoint { get; private set; }
        public Transform MouthPoint { get; private set; }
        /// <summary>Tay gan dia (Unity -x): an khi ong tay cam dua hien (F-080 AC-6).</summary>
        public GameObject EatingArm { get; private set; }
        public string CurrentClip { get; private set; }
        public float CurrentTime { get; private set; }
        public string Hair { get; private set; }
        public Color Shirt { get; private set; }
        public readonly Dictionary<string, int> Played = new Dictionary<string, int>();

        /// <summary>Dung nhan vat <paramref name="model"/> vao <paramref name="parent"/> (goc model = goc parent). Null neu chua co FBX.</summary>
        public static CharacterRig Create(Transform parent, string model, float outline = Toon.OutlineBig)
        {
            if (!ModelLibrary.Has(Group, model)) return null;
            var host = new GameObject(model);
            host.transform.SetParent(parent, false);
            GameObject go = ModelLibrary.TryInstantiate(Group, model, host.transform, outline);
            Orient(go);
            var rig = host.AddComponent<CharacterRig>();
            rig.Init(model);
            return rig;
        }

        void Awake()
        {
            // ban sao (Instantiate) khong mang theo du lieu khoi tao: khoi lai theo ten da luu
            if (ModelName == null && !string.IsNullOrEmpty(modelName)) Init(modelName);
        }

        void Init(string model)
        {
            ModelName = model;
            modelName = model;
            anim = GetComponentInChildren<Animation>(true);
            if (anim != null)
            {
                anim.playAutomatically = false;
                anim.Stop();
                foreach (AnimationState st in anim)
                {
                    string n = st.name;
                    int bar = n.LastIndexOf('|');
                    string shortName = bar >= 0 ? n.Substring(bar + 1) : n;
                    clipNames[shortName] = n;
                    st.wrapMode = WrapMode.ClampForever;
                    st.speed = 0f;
                    st.weight = 1f;
                    st.enabled = false;
                }
            }
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Head" && Head == null) Head = t;
                else if (t.name == ModelLibrary.FaceNode && FacePoint == null) FacePoint = t;
                else if (t.name == "Mieng" && MouthPoint == null) MouthPoint = t;
                else if (t.name == "TayPhai") EatingArm = t.gameObject;
                foreach (string h in HairStyles) if (t.name == h) hair[h] = t.gameObject;
            }
            // Face / Mieng phai di theo xuong dau (FBX co the dat empty o goc): gan vao xuong Head, giu vi tri the gioi
            if (Head != null)
            {
                if (FacePoint != null && FacePoint.parent != Head) FacePoint.SetParent(Head, true);
                if (MouthPoint != null && MouthPoint.parent != Head) MouthPoint.SetParent(Head, true);
            }
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                if (r.name == BepNhanPham.OutlineMesh.ChildName) continue;
                Material[] ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                    if (ms[i] != null && ms[i].HasProperty("_Color") && Close(ms[i].GetColor("_Color"), ShirtKey))
                        shirtSlots.Add(new KeyValuePair<Renderer, int>(r, i));
            }
            Shirt = ShirtKey;
            motion = GetComponentInParent<Lively>(true);
            if (motion != null)
            {
                motion.Rigged = true;
                motion.Reacted += OnReacted;
            }
            Sample(ClipWait, 0f);
        }

        void OnReacted(bool happy)
        {
            if (!isActiveAndEnabled) return;
            if (happy) Play(HasClip(ClipSurprised) && ModelName == Boss ? ClipSurprised : ClipHappy);
            else Play(ClipAngry);
        }

        void OnDestroy()
        {
            if (motion != null) motion.Reacted -= OnReacted;
        }

        static bool Close(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.02f && Mathf.Abs(a.g - b.g) < 0.02f && Mathf.Abs(a.b - b.b) < 0.02f;
        }

        public bool HasClip(string name) { return clipNames.ContainsKey(name); }
        public IEnumerable<string> Clips { get { return clipNames.Keys; } }
        public bool HasHair(string name) { return hair.ContainsKey(name); }

        public float ClipLength(string name)
        {
            string full;
            if (anim == null || !clipNames.TryGetValue(name, out full)) return 0f;
            AnimationState st = anim[full];
            return st != null ? st.length : 0f;
        }

        /// <summary>Kieu toc / mu (ten trong <see cref="HairStyles"/>; null = tat het) va mau ao.</summary>
        public void SetLook(string hairName, Color shirt)
        {
            Hair = hairName;
            foreach (var kv in hair) kv.Value.SetActive(kv.Key == hairName);
            SetShirt(shirt);
        }

        public void SetShirt(Color shirt)
        {
            Shirt = shirt;
            Material m = Toon.Mat(shirt);
            foreach (var kv in shirtSlots)
            {
                Material[] ms = kv.Key.sharedMaterials;
                ms[kv.Value] = m;
                kv.Key.sharedMaterials = ms;
            }
        }

        /// <summary>Vat lieu ao (ong tay cam dua lay cung mau, F-080 AC-6).</summary>
        public Material ShirtMaterial
        {
            get
            {
                if (EatingArm != null)
                {
                    Renderer r = EatingArm.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterials.Length > 0) return r.sharedMaterials[0];
                }
                return Toon.Mat(Shirt);
            }
        }

        /// <summary>Choi mot lan (Vui, Gian, VayChao, NgacNhien) roi ve vong lap (Cho / Di).</summary>
        public void Play(string clip)
        {
            if (!HasClip(clip)) return;
            oneShot = clip;
            oneT = 0f;
            int n;
            Played.TryGetValue(clip, out n);
            Played[clip] = n + 1;
            Sample(clip, 0f);
        }

        public bool Walking { get { return motion != null && (motion.WalkingIn || motion.WalkingOut) && motion.Moving; } }

        void Update()
        {
            float dt = Time.deltaTime;
            bool walking = Walking;
            if (walking) oneShot = null;
            if (oneShot != null)
            {
                oneT += dt;
                if (oneT >= ClipLength(oneShot)) oneShot = null;
            }
            loopT += dt;
            string clip = oneShot ?? (walking ? ClipWalk : ClipWait);
            float len = Mathf.Max(0.01f, ClipLength(clip));
            float t = oneShot != null ? Mathf.Min(oneT, len) : Mathf.Repeat(loopT, len);
            Sample(clip, t);
        }

        void Sample(string clip, float time)
        {
            if (Stepped) time = Mathf.Floor(time * StepFps + 1e-4f) / StepFps;
            CurrentClip = clip;
            CurrentTime = time;
            if (anim == null) return;
            string full;
            if (!clipNames.TryGetValue(clip, out full)) return;
            foreach (AnimationState st in anim)
            {
                bool on = st.name == full;
                st.enabled = on;
                if (on)
                {
                    st.weight = 1f;
                    st.speed = 0f;
                    st.time = time;
                }
            }
            anim.Sample();
        }

        /// <summary>Hop bao the gioi theo dinh da bien dang (BakeMesh) cua cac mesh dang hien - chinh xac hon Renderer.bounds.</summary>
        public Bounds BakedBounds()
        {
            var b = new Bounds();
            bool first = true;
            var tmp = new Mesh();
            foreach (SkinnedMeshRenderer r in GetComponentsInChildren<SkinnedMeshRenderer>(false))
            {
                if (r.name == BepNhanPham.OutlineMesh.ChildName || r.sharedMesh == null) continue;
                r.BakeMesh(tmp);   // khong scale: TransformPoint tu ap ti le ca cac node cha (ghe thu 0,72)
                foreach (Vector3 v in tmp.vertices)
                {
                    Vector3 w = r.transform.TransformPoint(v);
                    if (first) { b = new Bounds(w, Vector3.zero); first = false; }
                    else b.Encapsulate(w);
                }
            }
            Destroy(tmp);
            return b;
        }

        /// <summary>Hop bao the gioi cua cac mesh dang hien (test: ba khach khong long nhau).</summary>
        public Bounds WorldBounds()
        {
            var b = new Bounds();
            bool first = true;
            foreach (Renderer r in GetComponentsInChildren<Renderer>(false))
            {
                if (r.name == BepNhanPham.OutlineMesh.ChildName) continue;
                if (first) { b = r.bounds; first = false; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }
    }
}
