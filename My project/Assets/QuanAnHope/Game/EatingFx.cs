using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Hoat anh khach an (constitution 1.5 muc 6.2: khach an xong mon moi roi ghe).
    /// Doi dua gap tu dia len mieng theo nhip, mon tren dia voi dan theo tung mieng, khach nhai (than nhun),
    /// an xong dia trong. Tien do lay tu Core (GameSession.EatProgress) nen hinh luon khop luat.
    /// </summary>
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
            fx.Apply(0f);
            return fx;
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

        void Update()
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
