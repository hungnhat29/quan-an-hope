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
            // nhai: bam theo dua muot (khong nhay bac)
            chew = Mathf.Lerp(chew, chewTarget, 1f - Mathf.Exp(-dt * 14f));
            Apply();
        }

        void Apply()
        {
            float t = clock + phase;
            float up = 0f, sx = 1f, sy = 1f, rz = 0f, ry = 0f;

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
                float c = (0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f * 2.2f)) * chew * 0.045f;
                sy *= 1f - c; sx *= 1f + c * 0.5f;
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

            transform.localPosition = restPos + Vector3.up * up;
            transform.localScale = new Vector3(restScale.x * sx, restScale.y * sy, restScale.z * sx);
            transform.localRotation = restRot * Quaternion.Euler(0f, ry, rz);
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
