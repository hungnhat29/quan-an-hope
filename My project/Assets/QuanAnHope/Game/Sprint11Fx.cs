using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Hieu ung nuoc sot (yeu cau 23/11/2026): len Huyen thoai = "7 sac cau vong sieu bung no"; xuong Lac vi = "nhu qua bom xit".
    /// Dung khoi co ban (Toon) va den diem, khong phu thuoc timeScale cua Tab (dung Time.deltaTime nhu cac hieu ung khac).
    /// </summary>
    public static class SauceFx
    {
        /// <summary>7 mau cau vong (do, cam, vang, luc, lam, cham, tim).</summary>
        public static readonly Color[] Rainbow7 =
        {
            UiKit.Hex("#E8604F"), UiKit.Hex("#F28C38"), UiKit.Hex("#F2C14E"), UiKit.Hex("#7DB87A"),
            UiKit.Hex("#5FB3D9"), UiKit.Hex("#4F6CC9"), UiKit.Hex("#A77BD6"),
        };

        public static RainbowBurst Rainbow(Vector3 at) { return RainbowBurst.Play(at); }
        public static BombFizzle Bomb(Vector3 at) { return BombFizzle.Play(at); }
    }

    /// <summary>Cau vong bung no: 7 vong mau no lan ra, ~50 hat 7 mau ban tung toa, chop sang (1,8 giay).</summary>
    public class RainbowBurst : MonoBehaviour
    {
        const float Seconds = 1.8f;
        const int Sparks = 49;
        Transform[] rings, sparks;
        Vector3[] vel;
        Light flash;
        float t;

        public static RainbowBurst Play(Vector3 at)
        {
            var go = new GameObject("HieuUng_CauVong");
            go.transform.position = at;
            var fx = go.AddComponent<RainbowBurst>();
            fx.Build();
            return fx;
        }

        public float Progress { get { return t / Seconds; } }

        void Build()
        {
            rings = new Transform[SauceFx.Rainbow7.Length];
            for (int i = 0; i < rings.Length; i++)
                rings[i] = Toon.Prim(PrimitiveType.Cylinder, transform, "Vong_" + i, new Vector3(0f, 0.05f + 0.035f * i, 0f), new Vector3(0.1f, 0.012f, 0.1f),
                    SauceFx.Rainbow7[i], 0f).transform;
            sparks = new Transform[Sparks];
            vel = new Vector3[Sparks];
            var rnd = new System.Random(7);
            for (int i = 0; i < Sparks; i++)
            {
                Color c = SauceFx.Rainbow7[i % SauceFx.Rainbow7.Length];
                PrimitiveType shape = i % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere;
                sparks[i] = Toon.Prim(shape, transform, "Hat_" + i, new Vector3(0f, 0.08f, 0f), Vector3.one * 0.05f, c, 0f,
                    new Vector3(rnd.Next(360), rnd.Next(360), 0f)).transform;
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f;
                float up = 1.6f + (float)rnd.NextDouble() * 2.2f;
                float out_ = 0.8f + (float)rnd.NextDouble() * 1.6f;
                vel[i] = new Vector3(Mathf.Cos(a) * out_, up, Mathf.Sin(a) * out_);
            }
            var lightGo = new GameObject("ChopSang");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            flash = lightGo.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.range = 4f;
            flash.color = SauceFx.Rainbow7[2];
            flash.intensity = 6f;
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            t += dt;
            float k = Mathf.Clamp01(t / Seconds);
            for (int i = 0; i < rings.Length; i++)
            {
                float start = i * 0.06f;
                float r = Mathf.Clamp01((t - start) / 0.8f);
                // vong no ra toi 0,9 m roi tat (khong che kin tam nhin khi dung gan quay)
                float d = 0.1f + 0.8f * (1f - (1f - r) * (1f - r));
                float thick = Mathf.Lerp(0.03f, 0.004f, r);
                float fade = 1f - Mathf.SmoothStep(0.75f, 1f, r);
                rings[i].localScale = r <= 0f || fade <= 0.01f ? Vector3.zero : new Vector3(d * (0.4f + 0.6f * fade), thick, d * (0.4f + 0.6f * fade));
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                vel[i] += Vector3.down * 5.5f * dt;
                sparks[i].localPosition += vel[i] * dt;
                sparks[i].Rotate(240f * dt, 180f * dt, 0f);
                float s = 0.06f * (1f - Mathf.SmoothStep(0.55f, 1f, k));
                sparks[i].localScale = Vector3.one * s;
            }
            if (flash != null)
            {
                flash.intensity = Mathf.Lerp(6f, 0f, Mathf.Clamp01(t / 0.6f));
                flash.color = SauceFx.Rainbow7[(int)(t * 14f) % SauceFx.Rainbow7.Length];
            }
            if (t >= Seconds) Destroy(gameObject);
        }
    }

    /// <summary>Bom xit: qua bom den ngoi chay xeo xeo, rung, roi xep lai "pfff" va mot cum khoi xam nho (1,7 giay), khong no.</summary>
    public class BombFizzle : MonoBehaviour
    {
        const float Seconds = 1.7f, FuseSeconds = 1.0f;
        Transform bomb, spark, fuse;
        Transform[] puffs;
        float t;

        public static BombFizzle Play(Vector3 at)
        {
            var go = new GameObject("HieuUng_BomXit");
            go.transform.position = at + Vector3.up * 0.18f;
            var fx = go.AddComponent<BombFizzle>();
            fx.Build();
            return fx;
        }

        public float Progress { get { return t / Seconds; } }

        void Build()
        {
            bomb = Toon.Empty(transform, "Bom", Vector3.zero).transform;
            Toon.Prim(PrimitiveType.Sphere, bomb, "Than", Vector3.zero, Vector3.one * 0.2f, UiKit.Ink, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Sphere, bomb, "Bong", new Vector3(-0.045f, 0.05f, -0.07f), new Vector3(0.05f, 0.035f, 0.02f), UiKit.Hex("#8E8580"), 0f);
            Toon.Prim(PrimitiveType.Cylinder, bomb, "Co", new Vector3(0f, 0.105f, 0f), new Vector3(0.07f, 0.04f, 0.07f), UiKit.Hex("#5A5250"), Toon.OutlineSmall);
            fuse = Toon.Prim(PrimitiveType.Cylinder, bomb, "NgoiNo", new Vector3(0.02f, 0.16f, 0f), new Vector3(0.015f, 0.09f, 0.015f), UiKit.Hex("#D69B68"), 0f,
                new Vector3(0f, 0f, -20f)).transform;
            spark = Toon.Prim(PrimitiveType.Sphere, bomb, "TiaLua", new Vector3(0.035f, 0.205f, 0f), Vector3.one * 0.045f, UiKit.Mustard, 0f).transform;
            puffs = new Transform[5];
            for (int i = 0; i < puffs.Length; i++)
            {
                puffs[i] = Toon.Prim(PrimitiveType.Sphere, transform, "Khoi_" + i, new Vector3(0f, 0.05f, 0f), Vector3.zero, UiKit.Hex("#B8B2AD"), 0f).transform;
            }
            bomb.localScale = Vector3.zero;
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            t += dt;
            if (t < FuseSeconds)
            {
                float pop = Mathf.Clamp01(t / 0.15f);
                float wob = Mathf.Sin(t * 40f) * 0.06f * Mathf.Clamp01(t / FuseSeconds);
                bomb.localScale = Vector3.one * (pop * (1.15f - 0.15f * pop)) * (1f + wob);
                bomb.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 30f) * 8f);
                // tia lua chay xuong ngoi, nhap nhay
                float burn = t / FuseSeconds;
                spark.localPosition = Vector3.Lerp(new Vector3(0.035f, 0.205f, 0f), new Vector3(0.012f, 0.13f, 0f), burn);
                spark.localScale = Vector3.one * (0.035f + 0.025f * Mathf.Abs(Mathf.Sin(t * 47f)));
                fuse.localScale = new Vector3(0.015f, 0.045f * (1f - burn * 0.8f), 0.015f);
                return;
            }
            // xit: tia lua tat, bom xep doc rot nghieng, khoi xam bay cham len
            float k = Mathf.Clamp01((t - FuseSeconds) / (Seconds - FuseSeconds));
            spark.localScale = Vector3.zero;
            float squash = Mathf.Lerp(1f, 0.45f, Mathf.SmoothStep(0f, 1f, k));
            bomb.localScale = new Vector3(1.1f + (1f - squash) * 0.3f, squash, 1.1f + (1f - squash) * 0.3f) * (1f - 0.6f * k * k);
            bomb.localRotation = Quaternion.Euler(0f, 0f, 25f * k);
            for (int i = 0; i < puffs.Length; i++)
            {
                float a = i * Mathf.PI * 2f / puffs.Length;
                float s = 0.09f * Mathf.Sin(Mathf.Clamp01(k * 1.2f) * Mathf.PI);
                puffs[i].localScale = Vector3.one * s;
                puffs[i].localPosition = new Vector3(Mathf.Cos(a) * 0.06f * (1f + k), 0.16f + 0.25f * k + 0.02f * i, Mathf.Sin(a) * 0.06f * (1f + k));
            }
            if (t >= Seconds) Destroy(gameObject);
        }
    }
}
