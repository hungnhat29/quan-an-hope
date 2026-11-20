using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Do vat bay tu tay / lo gia vi vao dung cu tren bep theo duong cong, cham thi nay bep (squash 15%, Art Bible muc 12),
    /// roi nam lai trong chao / noi / vi. Dang nau thi rung nhe (xeo xeo).
    /// </summary>
    public class PanItem : MonoBehaviour
    {
        public const float FlySeconds = 0.45f;
        const float LandSeconds = 0.18f;
        const float ArcHeight = 0.28f;

        Vector3 fromWorld;
        Vector3 target;
        Vector3 scale;
        float t;
        float phase;

        /// <summary>Da nam yen trong dung cu.</summary>
        public bool Landed { get { return t >= 1f + LandSeconds / FlySeconds; } }
        public bool Sizzle;
        public Vector3 Target { get { return target; } }

        public static PanItem Launch(GameObject item, Transform pan, Vector3 fromWorld, Vector3 localTarget)
        {
            var p = item.AddComponent<PanItem>();
            item.transform.SetParent(pan, true);
            p.scale = item.transform.localScale;
            p.fromWorld = fromWorld;
            p.target = localTarget;
            p.phase = Random.value * 10f;
            item.transform.position = fromWorld;
            return p;
        }

        public void Retarget(Vector3 localTarget)
        {
            target = localTarget;
            if (Landed) transform.localPosition = target;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (!Landed)
            {
                t += dt / FlySeconds;
                if (t < 1f)
                {
                    float e = t * t * (3f - 2f * t);
                    Vector3 to = transform.parent.TransformPoint(target);
                    transform.position = Vector3.Lerp(fromWorld, to, e) + Vector3.up * ArcHeight * 4f * t * (1f - t);
                    transform.localRotation = Quaternion.Euler(0f, 0f, 360f * e);
                    return;
                }
                transform.localPosition = target;
                transform.localRotation = Quaternion.identity;
                float land = Mathf.Clamp01((t - 1f) * FlySeconds / LandSeconds);
                float squash = Mathf.Sin(land * Mathf.PI) * 0.15f;
                transform.localScale = new Vector3(scale.x * (1f + squash), scale.y * (1f - squash), scale.z * (1f + squash));
                if (Landed) transform.localScale = scale;
                return;
            }
            if (Sizzle)
            {
                // xeo xeo: nay muot (khong |sin| gay goc), nghieng nhe
                float bob = (0.5f + 0.5f * Mathf.Sin(Time.time * 11f + phase)) * 0.012f;
                transform.localPosition = target + Vector3.up * bob;
                transform.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 7f + phase) * 4f, 0f, Mathf.Sin(Time.time * 9f + phase * 1.3f) * 4f);
            }
            else transform.localPosition = target;
        }
    }

    /// <summary>Lac lu do vat (lo gia vi nghieng khi rot) trong 0,45 giay.</summary>
    public class Wobble : MonoBehaviour
    {
        const float Seconds = 0.45f;
        Quaternion rest;
        float t = 1f;
        bool hasRest;

        public bool Playing { get { return t < 1f; } }

        public void Play()
        {
            if (!hasRest)
            {
                rest = transform.localRotation;
                hasRest = true;
            }
            t = 0f;
        }

        void Update()
        {
            if (t >= 1f) return;
            t = Mathf.Min(1f, t + Time.deltaTime / Seconds);
            float angle = Mathf.Sin(t * Mathf.PI) * 35f + Mathf.Sin(t * Mathf.PI * 4f) * 6f * (1f - t);
            transform.localRotation = rest * Quaternion.Euler(-angle, 0f, angle * 0.4f);
            if (t >= 1f) transform.localRotation = rest;
        }
    }

    /// <summary>Hien ra bang cach phong to co nay qua 12% (Art Bible muc 12: nay 10-15%). Mon ra lo 0,6 giay (muc 11).</summary>
    public class Pop : MonoBehaviour
    {
        Vector3 full;
        float seconds = 0.6f;
        float t;

        public static Pop Play(GameObject go, float seconds)
        {
            var p = go.AddComponent<Pop>();
            p.full = go.transform.localScale;
            p.seconds = seconds;
            go.transform.localScale = p.full * 0.01f;
            return p;
        }

        void Update()
        {
            t = Mathf.Min(1f, t + Time.deltaTime / seconds);
            float s = Mathf.Sin(t * Mathf.PI * 0.5f) * (1f + 0.12f * Mathf.Sin(t * Mathf.PI));
            transform.localScale = full * Mathf.Max(0.01f, s);
            if (t >= 1f)
            {
                transform.localScale = full;
                Destroy(this);
            }
        }
    }

    /// <summary>
    /// Hoi bay len tu dia mon theo bac (Art Bible muc 7): Lac vi khoi xam gap khuc, Do khong co, Binh thuong 3 net hoi trang,
    /// Ngon trai tim trang, Tuyet hao trai tim trang + lap lanh, Huyen thoai trai tim vang.
    /// </summary>
    public class DishFx : MonoBehaviour
    {
        const float Period = 1.6f;
        const float Rise = 0.26f;

        FaceBillboard[] parts;
        Vector3[] starts;
        float[] offsets;
        MaterialPropertyBlock block;

        public int Count { get { return parts != null ? parts.Length : 0; } }
        public string Kind { get; private set; }

        public static string KindFor(int dishTier)
        {
            switch (dishTier)
            {
                case 0: return "khoi";
                case 1: return null;
                case 2: return "hoi";
                case 3: return "tim";
                case 4: return "tim+lap_lanh";
                default: return "tim_vang";
            }
        }

        public static DishFx Create(Transform dish, int tier, Transform anchor)
        {
            string kind = KindFor(tier);
            var fx = dish.gameObject.AddComponent<DishFx>();
            fx.Kind = kind;
            if (kind == null)
            {
                fx.parts = new FaceBillboard[0];
                return fx;
            }

            string[] textures;
            Vector3[] spots;
            float size = 0.09f;
            switch (kind)
            {
                case "khoi":
                    textures = new[] { "khoi", "khoi" };
                    spots = new[] { new Vector3(-0.04f, 0.1f, 0f), new Vector3(0.05f, 0.1f, 0.02f) };
                    break;
                case "hoi":
                    textures = new[] { "hoi", "hoi", "hoi" };
                    spots = new[] { new Vector3(-0.06f, 0.1f, 0f), new Vector3(0f, 0.12f, 0.01f), new Vector3(0.06f, 0.1f, 0f) };
                    size = 0.08f;
                    break;
                case "tim":
                    textures = new[] { "tim", "tim" };
                    spots = new[] { new Vector3(-0.03f, 0.12f, 0f), new Vector3(0.05f, 0.1f, 0.02f) };
                    break;
                case "tim+lap_lanh":
                    textures = new[] { "tim", "lap_lanh", "lap_lanh" };
                    spots = new[] { new Vector3(0f, 0.12f, 0f), new Vector3(-0.08f, 0.1f, 0f), new Vector3(0.08f, 0.14f, 0f) };
                    break;
                default:
                    textures = new[] { "tim_vang", "tim_vang", "lap_lanh" };
                    spots = new[] { new Vector3(-0.04f, 0.12f, 0f), new Vector3(0.05f, 0.1f, 0.02f), new Vector3(0.09f, 0.16f, 0f) };
                    break;
            }

            fx.parts = new FaceBillboard[textures.Length];
            fx.starts = spots;
            fx.offsets = new float[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                Transform spot = Toon.Empty(dish, "HoiGoc_" + i, spots[i]).transform;
                fx.parts[i] = FaceBillboard.Create(dish, spot, FaceArt.Fx(textures[i]), size, 0f, "Hoi_" + textures[i]);
                fx.offsets[i] = i / (float)textures.Length;
            }
            return fx;
        }

        void Update()
        {
            if (parts == null || parts.Length == 0) return;
            if (block == null) block = new MaterialPropertyBlock();
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null) continue;
                float u = Mathf.Repeat(Time.time / Period + offsets[i], 1f);
                Transform spot = parts[i].Anchor;
                spot.localPosition = starts[i] + Vector3.up * Rise * u + Vector3.right * Mathf.Sin(u * Mathf.PI * 2f + i) * 0.015f;
                MeshRenderer r = parts[i].Renderer;
                r.GetPropertyBlock(block);
                block.SetColor("_Color", new Color(1f, 1f, 1f, Mathf.Sin(u * Mathf.PI)));
                r.SetPropertyBlock(block);
            }
        }
    }

    /// <summary>
    /// Hao quang vang cua Mon Hy Vong (Art Bible muc 4, 7: "Bac 5 / Huyen thoai luon co them hao quang vang"):
    /// vong 12 hat vang quay cham phia tren dia, nhap nho len xuong.
    /// </summary>
    public class LegendHalo : MonoBehaviour
    {
        public const int Beads = 12;
        Transform ring;

        public static LegendHalo Create(Transform dish)
        {
            var h = dish.gameObject.AddComponent<LegendHalo>();
            h.ring = Toon.Empty(dish, "HaoQuang", new Vector3(0f, 0.2f, 0f)).transform;
            for (int i = 0; i < Beads; i++)
            {
                float a = i * Mathf.PI * 2f / Beads;
                Toon.Prim(PrimitiveType.Sphere, h.ring, "Hat_" + i, new Vector3(Mathf.Cos(a) * 0.2f, 0f, Mathf.Sin(a) * 0.2f),
                    Vector3.one * 0.03f, UiKit.Hex("#F2C14E"), 0f);
            }
            return h;
        }

        void Update()
        {
            if (ring == null) return;
            ring.localRotation = Quaternion.Euler(0f, Time.time * 40f, 0f);
            ring.localPosition = new Vector3(0f, 0.2f + Mathf.Sin(Time.time * 2f) * 0.012f, 0f);
        }
    }
}
