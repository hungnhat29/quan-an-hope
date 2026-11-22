using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Chuyen dong "song" muot cho khach (yeu cau 20/11: "animation cua khach hang, nguyen lieu cung muot nhu cot truyen").
    /// Moi khung hinh (60 hinh/giay, khong giat) ghi lai vi tri / ti le / goc tu tu the nghi + cac hieu ung:
    /// nhay vao ghe (nen - gian), tho, dung dua, nhay cang khi vui, lac dau khi gian, nhai, roi ghe (bat len roi thu nho).
    /// Moi hieu ung dung duong cong em (ease / lo xo tat dan) thay cho dao dong |sin| gay giat.
    /// Khoi tron nen mem (Art Bible muc 6: nhan vat tron nay mem, nen 90/110).
    /// </summary>
    public class Lively : MonoBehaviour
    {
        public const float ArriveSeconds = 0.5f;
        public const float HopSeconds = 0.5f;
        public const float ShakeSeconds = 0.45f;

        Vector3 restPos, restScale;
        Quaternion restRot;
        bool hasRest;
        float phase, clock;
        float arrive = 1f, hop = 1f, shake = 1f;
        float leave = -1f, leaveSeconds = 0.5f;
        float chew, chewTarget;

        // ---------- sprint 8 F-073: di vao / di ra (yeu cau 21/11 "khong can qua nhanh") ----------
        /// <summary>Toc do di (m/giay the gioi).</summary>
        public const float WalkSpeed = 0.9f;
        public const float TurnSeconds = 0.35f;

        /// <summary>Diem chan cua khoi (toa do cua chinh khoi): xoay / co gian quanh diem nay. Khoi khach cua canh: (0, 0, SeatZ).</summary>
        public Vector3 Pivot;

        enum Walk { None, In, Out }
        Walk walk;
        Vector3[] path;            // toa do cha, tinh tu cho ngoi (offset cua diem chan)
        float walkT, walkLength, walkSeconds, turnFrom;
        Vector3 walkOffset;
        float yaw;

        /// <summary>Dang di vao ghe (chua ngoi).</summary>
        public bool WalkingIn { get { return walk == Walk.In; } }
        /// <summary>Dang di ra cua.</summary>
        public bool WalkingOut { get { return walk == Walk.Out; } }
        /// <summary>Da ra khoi cua (an khoi duoc).</summary>
        public bool GoneOut { get; private set; }
        public int WalkIns { get; private set; }

        /// <summary>Thoi gian di het duong (giay) voi do dai the gioi <paramref name="worldLength"/>.</summary>
        public static float WalkTime(float worldLength) { return Mathf.Max(0.5f, worldLength / WalkSpeed); }

        /// <summary>Bat dau di tu cua vao ghe: <paramref name="worldPath"/> la cac diem the gioi (ngoai cua -> cua), diem cuoi la cho ngoi.</summary>
        public void WalkIn(Vector3[] worldPath)
        {
            BeginWalk(worldPath, false);
            walk = Walk.In;
            WalkIns++;
            arrive = 1f;
        }

        /// <summary>Dung day, quay lung, di ra cua theo <paramref name="worldPath"/> (cua -> ngoai cua).</summary>
        public void WalkOut(Vector3[] worldPath)
        {
            BeginWalk(worldPath, true);
            walk = Walk.Out;
            leave = -1f;
        }

        /// <summary>Tong thoi gian cua lan di ra (de RoomView giu khoi tren man hinh).</summary>
        public float WalkOutSeconds { get { return TurnSeconds + walkSeconds; } }
        /// <summary>Thoi gian cua lan di vao.</summary>
        public float WalkInSeconds { get { return walkSeconds; } }

        void BeginWalk(Vector3[] worldPath, bool outward)
        {
            if (!hasRest) CaptureRest();
            Transform parent = transform.parent;
            Vector3 seat = SeatParent();
            var pts = new System.Collections.Generic.List<Vector3>();
            if (outward) pts.Add(Vector3.zero);
            foreach (Vector3 w in worldPath) pts.Add((parent != null ? parent.InverseTransformPoint(w) : w) - seat);
            if (!outward) pts.Add(Vector3.zero);
            for (int i = 0; i < pts.Count; i++) pts[i] = new Vector3(pts[i].x, 0f, pts[i].z);
            path = pts.ToArray();
            walkLength = 0f;
            float world = 0f;
            for (int i = 1; i < path.Length; i++)
            {
                walkLength += (path[i] - path[i - 1]).magnitude;
                world += (parent != null ? parent.TransformVector(path[i] - path[i - 1]) : path[i] - path[i - 1]).magnitude;
            }
            walkSeconds = WalkTime(world);
            walkT = 0f;
            turnFrom = yaw;
            GoneOut = false;
            walkOffset = outward ? Vector3.zero : path[0];
            if (!outward) yaw = YawAlong(path[0], path.Length > 1 ? path[1] : Vector3.zero, yaw);
            Apply();
        }

        /// <summary>Diem chan cua khoi luc ngoi (toa do cha).</summary>
        Vector3 SeatParent()
        {
            return restPos + restRot * Vector3.Scale(restScale, Pivot);
        }

        static float YawAlong(Vector3 from, Vector3 to, float fallback)
        {
            Vector3 d = to - from;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-6f) return fallback;
            // mat khoi khach huong -z (nhin Hope): yaw = 0 khi di theo -z
            return Mathf.Atan2(-d.x, -d.z) * Mathf.Rad2Deg;
        }

        Vector3 PointAt(float dist, out Vector3 dir)
        {
            dir = Vector3.zero;
            for (int i = 1; i < path.Length; i++)
            {
                Vector3 seg = path[i] - path[i - 1];
                float l = seg.magnitude;
                if (dist <= l || i == path.Length - 1)
                {
                    dir = seg;
                    return path[i - 1] + seg * (l > 1e-5f ? Mathf.Clamp01(dist / l) : 1f);
                }
                dist -= l;
            }
            return path.Length > 0 ? path[path.Length - 1] : Vector3.zero;
        }

        float stepPhase;

        /// <summary>Buoc di: offset vi tri, nhun buoc, quay mat theo huong di (cuoi duong vao: quay ve Hope).</summary>
        void TickWalk(float dt, ref float up, ref float sx, ref float sy, ref float rz)
        {
            if (walk == Walk.None || path == null || path.Length < 2)
            {
                if (GoneOut) { sx = sy = 0.01f; }
                return;
            }
            walkT += dt;
            float turn = walk == Walk.Out ? TurnSeconds : 0f;
            float moveT = walkT - turn;
            if (walk == Walk.Out && moveT < 0f)
            {
                // dung day + quay lung truoc khi di
                float k = Mathf.SmoothStep(0f, 1f, walkT / TurnSeconds);
                Vector3 d0;
                PointAt(0.001f, out d0);
                yaw = Mathf.LerpAngle(turnFrom, YawAlong(Vector3.zero, d0, turnFrom), k);
                up += Mathf.Sin(k * Mathf.PI) * 0.06f;
                return;
            }
            float frac = walkSeconds > 0f ? Mathf.Clamp01(moveT / walkSeconds) : 1f;
            Vector3 dir;
            walkOffset = PointAt(frac * walkLength, out dir);
            float targetYaw = YawAlong(Vector3.zero, dir, yaw);
            yaw = Mathf.LerpAngle(yaw, targetYaw, 1f - Mathf.Exp(-dt * 10f));
            // nhun buoc: 2,2 buoc / giay, bat len khi chan roi dat (|sin|), nghieng nhe trai phai
            stepPhase += dt * 2.2f * Mathf.PI;
            float bounce = Mathf.Abs(Mathf.Sin(stepPhase));
            up += bounce * 0.07f;
            sy *= 1f + (bounce - 0.5f) * 0.06f;
            sx *= 1f - (bounce - 0.5f) * 0.03f;
            rz += Mathf.Sin(stepPhase) * 4f;
            if (walk == Walk.Out && frac > 0.85f)
            {
                // khuat ngoai cua: thu nho dan
                float s = Mathf.Lerp(1f, 0.01f, (frac - 0.85f) / 0.15f);
                sx *= s; sy *= s;
            }
            if (frac >= 1f)
            {
                if (walk == Walk.In)
                {
                    walk = Walk.None;
                    walkOffset = Vector3.zero;
                    yaw = 0f;
                    // ngoi xuong: nen roi bat lai (lo xo tat dan)
                    arrive = 0.45f;
                }
                else
                {
                    walk = Walk.None;
                    GoneOut = true;
                    sx = sy = 0.01f;
                }
            }
        }

        /// <summary>Dung moi chuyen dong di (khach moi thay khach dang ra cung ghe).</summary>
        public void StopWalk()
        {
            walk = Walk.None;
            walkOffset = Vector3.zero;
            yaw = 0f;
            GoneOut = false;
            Apply();
        }

        // ---------- sprint 8 F-074: an theo ket qua ----------
        public enum EatMood { Happy, Delighted, Sad, Angry }
        /// <summary>Kieu an: vui (mac dinh), rat vui (Tuyet hao: thinh thoang nhay), buon (cui dau, nhai cham), gian (nhai cham, lac dau ngat quang).</summary>
        public EatMood Eating = EatMood.Happy;
        float moodClock;

        /// <summary>Do nhun khi nhai (0..1), EatingFx dat theo do gan mieng cua dua.</summary>
        public float Chew { set { chewTarget = Mathf.Clamp01(value); } }
        public bool Leaving { get { return leave >= 0f; } }
        /// <summary>Ti le doc hien tai so voi tu the nghi (test: dang tho / nhun).</summary>
        public float ScaleY { get { return hasRest ? transform.localScale.y / Mathf.Max(1e-5f, restScale.y) : 1f; } }

        public static Lively On(GameObject go)
        {
            if (go == null) return null;
            Lively l = go.GetComponent<Lively>();
            if (l == null) l = go.AddComponent<Lively>();
            return l;
        }

        void Awake()
        {
            // Bob (nhun sin cua canh goc) ghi de vi tri trong Update: Lively thay the bang nhun em hon
            var bob = GetComponent<BepNhanPham.Bob>();
            if (bob != null)
            {
                bobAmp = bob.amplitude;
                bob.enabled = false;
            }
            CaptureRest();
            phase = Random.value * 10f;
        }

        float bobAmp = 0.02f;
        float walkDt;

        void CaptureRest()
        {
            restPos = transform.localPosition;
            restScale = transform.localScale;
            restRot = transform.localRotation;
            hasRest = true;
        }

        /// <summary>Khoi khach hien lai (khach moi ngoi vao): nhay vao ghe.</summary>
        void OnEnable()
        {
            if (!hasRest) CaptureRest();
            leave = -1f;
            arrive = 0f;
            walk = Walk.None;
            walkOffset = Vector3.zero;
            yaw = 0f;
            GoneOut = false;
            Eating = EatMood.Happy;
            chew = chewTarget = 0f;
            Apply();
        }

        void OnDisable()
        {
            if (!hasRest) return;
            transform.localPosition = restPos;
            transform.localScale = restScale;
            transform.localRotation = restRot;
        }

        /// <summary>So lan nhay cang / lac dau da dien (test).</summary>
        public int HopCount { get; private set; }
        public int ShakeCount { get; private set; }

        public void Hop() { hop = 0f; HopCount++; }
        public void Shake() { shake = 0f; ShakeCount++; }
        public void Leave(float seconds) { leaveSeconds = Mathf.Max(0.05f, seconds); leave = 0f; }
        public void CancelLeave() { leave = -1f; Apply(); }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            clock += dt;
            arrive = Mathf.Min(1f, arrive + dt / ArriveSeconds);
            hop = Mathf.Min(1f, hop + dt / HopSeconds);
            shake = Mathf.Min(1f, shake + dt / ShakeSeconds);
            if (leave >= 0f) leave = Mathf.Min(1f, leave + dt / leaveSeconds);
            // an theo ket qua (F-074): gian lac dau moi 1,6 giay; Tuyet hao nhay cang moi 1,8 giay
            moodClock += dt;
            if (chewTarget > 0.01f && Eating == EatMood.Angry && moodClock > 1.6f) { moodClock = 0f; Shake(); }
            else if (chewTarget > 0.01f && Eating == EatMood.Delighted && moodClock > 1.8f) { moodClock = 0f; Hop(); }
            // nhai: bam theo dua muot (khong nhay bac)
            chew = Mathf.Lerp(chew, chewTarget, 1f - Mathf.Exp(-dt * 14f));
            walkDt = dt;
            Apply();
        }

        void Apply()
        {
            float t = clock + phase;
            float up = 0f, sx = 1f, sy = 1f, rz = 0f, ry = 0f, rx = 0f;

            // tho: than co gian 2,5% chu ky 2,4 giay (ngang gian nguoc chieu de giu the tich)
            float breath = Mathf.Sin(t * Mathf.PI * 2f / 2.4f) * 0.025f;
            sy += breath; sx -= breath * 0.5f;
            // dung dua nhe trai phai, nhun len xuong theo nhip tho (thay Bob)
            rz += Mathf.Sin(t * Mathf.PI * 2f / 3.3f) * 2.2f;
            up += (Mathf.Sin(t * Mathf.PI * 2f / 2.4f - 0.8f) * 0.5f + 0.5f) * Mathf.Min(bobAmp, 0.04f);

            // nhay vao ghe: roi tu tren xuong (ease-in), cham ghe thi nen 12% roi bat lai (lo xo tat dan)
            if (arrive < 1f)
            {
                const float fall = 0.45f;
                if (arrive < fall)
                {
                    float k = arrive / fall;
                    up += (1f - k * k) * 0.45f;
                    sy *= 1f + 0.08f * (1f - k);           // gian doc khi roi
                    sx *= 1f - 0.04f * (1f - k);
                    float grow = Mathf.SmoothStep(0.55f, 1f, k);
                    sx *= grow; sy *= grow;
                }
                else
                {
                    float k = (arrive - fall) / (1f - fall);
                    float squash = Mathf.Exp(-k * 5f) * Mathf.Cos(k * Mathf.PI * 3f) * 0.12f;
                    sy *= 1f - squash; sx *= 1f + squash * 0.6f;
                }
            }

            // nhay cang khi vui: nen chuan bi -> bat len (gian) -> roi -> nen cham dat
            if (hop < 1f)
            {
                float k = hop;
                float air = k > 0.15f && k < 0.75f ? Mathf.Sin((k - 0.15f) / 0.6f * Mathf.PI) : 0f;
                up += air * 0.22f;
                float squash = k < 0.15f ? Mathf.Sin(k / 0.15f * Mathf.PI) * 0.1f
                    : k > 0.75f ? Mathf.Sin((k - 0.75f) / 0.25f * Mathf.PI) * 0.08f : -air * 0.06f;
                sy *= 1f - squash; sx *= 1f + squash * 0.6f;
            }

            // lac dau khi gian: quay trai phai tat dan
            if (shake < 1f)
                ry += Mathf.Sin(shake * Mathf.PI * 5f) * 12f * (1f - shake);

            // nhai: nhun theo nhip 2,2 lan/giay, muot
            if (chew > 0.001f)
            {
                bool slow = Eating == EatMood.Sad || Eating == EatMood.Angry;
                float c = (0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f * (slow ? 1.1f : 2.2f))) * chew * 0.045f;
                sy *= 1f - c; sx *= 1f + c * 0.5f;
                if (Eating == EatMood.Sad) rx += 9f * chew;    // cui dau
            }

            // roi ghe: bat len (anticipation), roi thu nho bay len (ease-in-back)
            if (leave >= 0f)
            {
                float k = leave;
                float lift = k < 0.25f ? -Mathf.Sin(k / 0.25f * Mathf.PI) * 0.05f : Mathf.Pow((k - 0.25f) / 0.75f, 2f) * 0.5f;
                up += lift;
                float s = k < 0.25f ? 1f + 0.06f * Mathf.Sin(k / 0.25f * Mathf.PI) : Mathf.Lerp(1f, 0.01f, Mathf.SmoothStep(0f, 1f, (k - 0.25f) / 0.75f));
                sx *= s; sy *= s;
            }

            TickWalk(walkDt, ref up, ref sx, ref sy, ref rz);
            walkDt = 0f;

            // xoay / co gian quanh diem chan (Pivot) de khoi khach dung tai cho khi quay mat (sprint 8)
            Vector3 scale = new Vector3(restScale.x * sx, restScale.y * sy, restScale.z * sx);
            Quaternion rot = restRot * Quaternion.Euler(rx, yaw + ry, rz);
            transform.localScale = scale;
            transform.localRotation = rot;
            transform.localPosition = SeatParent() + walkOffset + Vector3.up * up - rot * Vector3.Scale(scale, Pivot);
        }
    }

    /// <summary>
    /// Nguyen lieu "song" trong thung / tren thot (yeu cau 20/11): nhun, tho, nghieng nhe, lech pha tung mon de ca thung khong nhun cung luc.
    /// Gan vao khoi hinh (GiuHinh) nen mat (FaceBillboard bam tam hinh) di theo.
    /// </summary>
    [DefaultExecutionOrder(-50)]   // truoc FaceBillboard (LateUpdate) de mat dat theo hinh da nhun trong cung khung
    public class IngredientIdle : MonoBehaviour
    {
        Vector3 restPos, restScale;
        Quaternion restRot;
        float phase, speed;
        float boing = 1f;

        void Awake()
        {
            restPos = transform.localPosition;
            restScale = transform.localScale;
            restRot = transform.localRotation;
            phase = Random.value * 20f;
            speed = Random.Range(0.85f, 1.15f);
        }

        /// <summary>Nay "boing" (vua duoc dat xuong / vua so che xong).</summary>
        public void Boing() { boing = 0f; }

        void LateUpdate()
        {
            float t = (Time.time + phase) * speed;
            boing = Mathf.Min(1f, boing + Time.deltaTime / 0.6f);
            float breath = Mathf.Sin(t * Mathf.PI * 2f / 1.9f);
            float bounce = boing < 1f ? Mathf.Exp(-boing * 6f) * Mathf.Cos(boing * Mathf.PI * 4f) * 0.18f : 0f;
            float sy = 1f + breath * 0.04f - bounce, sx = 1f - breath * 0.02f + bounce * 0.6f;
            float lift = (0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f / 1.9f + 0.6f)) * 0.006f;
            transform.localPosition = restPos + Vector3.up * lift;
            transform.localScale = new Vector3(restScale.x * sx, restScale.y * sy, restScale.z * sx);
            transform.localRotation = restRot * Quaternion.Euler(Mathf.Sin(t * 1.3f) * 3f, Mathf.Sin(t * 0.7f) * 6f, Mathf.Sin(t * 1.7f) * 3f);
        }
    }
}
