using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Dao tren thot va dong tac so che theo cot "So che" cua constitution muc 4.5 (spec sprint 8 F-075):
    /// thai / cat / chat = dao chat len xuong (chat manh hon); dap, danh (trung) = trung go canh thot 2 lan roi nut;
    /// nhat, rua / rua = giot nuoc ban + nguyen lieu lac; boc vo / tach mai / tach bong = manh bat ra; loc = dao luot ngang.
    /// Chi chay khi dang giu chuot tren thot; tha chuot thi dao ve cho nghi canh thot.
    /// </summary>
    public class PrepFx : MonoBehaviour
    {
        public enum Motion { Chop, HeavyChop, Crack, Wash, Peel, Fillet }

        static readonly Dictionary<string, Motion> Kinds = new Dictionary<string, Motion>
        {
            { "ca_chua", Motion.Chop }, { "hanh_tay", Motion.Chop }, { "khoai_tay", Motion.Chop }, { "ot_chuong", Motion.Chop },
            { "bi_do", Motion.Chop }, { "mang_tay", Motion.Chop }, { "nam_dong_co", Motion.Chop }, { "dau_phu", Motion.Chop },
            { "heo", Motion.Chop }, { "bo", Motion.Chop }, { "wagyu", Motion.Chop }, { "muc", Motion.Chop },
            { "ga", Motion.HeavyChop }, { "tom_hum", Motion.HeavyChop },
            { "trung", Motion.Crack },
            { "rau_muong", Motion.Wash }, { "nam_rom", Motion.Wash },
            { "tom", Motion.Peel }, { "cua", Motion.Peel }, { "bong_cai", Motion.Peel },
            { "ca_hoi", Motion.Fillet }, { "ca_ngu", Motion.Fillet },
        };

        public static Motion MotionFor(string typeId)
        {
            Motion m;
            return typeId != null && Kinds.TryGetValue(typeId, out m) ? m : Motion.Chop;
        }

        static readonly Color Water = UiKit.Hex("#8FC3D6");
        static readonly Vector3 KnifeRest = new Vector3(0.36f, 0.012f, 0.16f);
        const float StrikesPerSecond = 2.4f;

        Transform knife;           // goc = dau chuoi dao, luoi chay theo -x
        Ingredient shownFor;
        Transform item;            // ItemView dang nam tren thot
        Vector3 itemRest;
        float phase;
        int lastStrike = -1;
        bool active;
        readonly List<Bit> bits = new List<Bit>();

        /// <summary>So nhat da chat / go / ban (test F-075 AC-2, AC-3).</summary>
        public int Strikes { get; private set; }
        public Motion Current { get; private set; }
        public bool Active { get { return active; } }
        public Transform Knife { get { return knife; } }
        public bool KnifeAtRest { get { return knife != null && (knife.localPosition - KnifeRest).sqrMagnitude < 1e-4f; } }

        class Bit
        {
            public Transform t;
            public Vector3 v;
            public float life;
        }

        public static PrepFx Create(Transform board)
        {
            var go = new GameObject("DaoVaSoChe");
            go.transform.SetParent(board, false);
            var fx = go.AddComponent<PrepFx>();
            fx.knife = BuildKnife(go.transform);
            fx.knife.localPosition = KnifeRest;
            fx.knife.localRotation = Quaternion.Euler(0f, 18f, 0f);
            return fx;
        }

        /// <summary>Dao: model SM_Dao (Blender, F-072) neu co, khong thi ghep khoi (luoi thep bo tron, chuoi go).</summary>
        static Transform BuildKnife(Transform parent)
        {
            var root = new GameObject("Dao").transform;
            root.SetParent(parent, false);
            if (ModelLibrary.TryInstantiate("Kitchen", "SM_Dao", root, Toon.OutlineSmall * 0.5f) != null)
                return root;
            Toon.Prim(PrimitiveType.Capsule, root, "Chuoi", new Vector3(-0.06f, 0.012f, 0f), new Vector3(0.026f, 0.06f, 0.026f), UiKit.Hex("#8C5A3A"), 0.006f, new Vector3(0f, 0f, 90f));
            Toon.Prim(PrimitiveType.Cylinder, root, "Khau", new Vector3(-0.125f, 0.014f, 0f), new Vector3(0.03f, 0.008f, 0.03f), UiKit.Hex("#B8B2A8"), 0.004f, new Vector3(0f, 0f, 90f));
            Toon.Prim(PrimitiveType.Cube, root, "Luoi", new Vector3(-0.25f, 0.026f, 0f), new Vector3(0.24f, 0.06f, 0.006f), UiKit.Hex("#D8D6D0"), 0.006f);
            Toon.Prim(PrimitiveType.Sphere, root, "MuiDao", new Vector3(-0.37f, 0.026f, 0f), new Vector3(0.05f, 0.06f, 0.006f), UiKit.Hex("#D8D6D0"), 0.006f);
            return root;
        }

        /// <summary>Moi khung hinh: <paramref name="holding"/> = nguoi choi dang giu chuot tren thot.</summary>
        public void Tick(Ingredient ing, ItemView view, float progress, bool holding, float dt)
        {
            Transform v = view != null ? view.transform : null;
            if (!ReferenceEquals(ing, shownFor) || v != item)
            {
                RestoreItem();
                shownFor = ing;
                item = v;
                itemRest = v != null ? v.localPosition : Vector3.zero;
                lastStrike = -1;
                phase = 0f;
                Current = MotionFor(ing != null ? ing.typeId : null);
            }
            active = holding && ing != null && !ing.prepared && item != null;
            TickBits(dt);
            if (!active)
            {
                RestKnife(dt);
                RestoreItem();
                return;
            }
            phase += dt * StrikesPerSecond * (Current == Motion.HeavyChop ? 0.8f : 1f);
            float k = phase % 1f;
            int n = Mathf.FloorToInt(phase);
            switch (Current)
            {
                case Motion.Crack: Crack(progress); break;
                case Motion.Wash: Wash(k, n); break;
                case Motion.Fillet: Fillet(k, n); break;
                case Motion.Peel: Chop(k, n, 0.6f, true); break;
                case Motion.HeavyChop: Chop(k, n, 1.5f, false); break;
                default: Chop(k, n, 1f, false); break;
            }
        }

        void RestoreItem()
        {
            if (item != null)
            {
                item.localPosition = itemRest;
                item.localRotation = Quaternion.identity;
            }
        }

        void RestKnife(float dt)
        {
            if (knife == null) return;
            float a = 1f - Mathf.Exp(-dt * 10f);
            knife.localPosition = Vector3.Lerp(knife.localPosition, KnifeRest, a);
            knife.localRotation = Quaternion.Slerp(knife.localRotation, Quaternion.Euler(0f, 18f, 0f), a);
            if ((knife.localPosition - KnifeRest).sqrMagnitude < 2e-5f) knife.localPosition = KnifeRest;
        }

        /// <summary>Dao chat: dau chuoi o ben phai mon, luoi gio len 28 do roi bo xuong (ease-in), cham thi mon nay.</summary>
        void Chop(float k, int n, float strength, bool peel)
        {
            float lift = k < 0.55f ? Mathf.SmoothStep(0f, 1f, k / 0.55f) : 1f - Mathf.Pow((k - 0.55f) / 0.45f, 2.2f);
            float angle = Mathf.Lerp(0f, 26f + 8f * strength, lift);
            knife.localPosition = itemRest + new Vector3(0.24f, 0.05f, -0.015f * Mathf.Sin(n * 1.7f));
            knife.localRotation = Quaternion.Euler(0f, 0f, -angle);
            if (n != lastStrike && k < 0.1f && n > 0)
            {
                lastStrike = n;
                Strike(peel ? 2 : Mathf.RoundToInt(2 * strength), peel ? 0.9f : 0.5f * strength, null);
            }
        }

        /// <summary>Trung: dua sang mep thot, go 2 lan (0,15-0,35 va 0,4-0,6 tien do), roi nut ra (vo bat ra) khi qua 0,65.</summary>
        void Crack(float progress)
        {
            RestKnife(Time.deltaTime);
            float side = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.12f, progress)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.75f, 0.95f, progress)));
            float tap = TapCurve(progress, 0.15f, 0.35f) + TapCurve(progress, 0.4f, 0.6f);
            item.localPosition = itemRest + new Vector3(0.12f * side, 0.06f * side - 0.05f * tap, 0f);
            item.localRotation = Quaternion.Euler(0f, 0f, -35f * side);
            // cham thot o dinh moi lan go (0,27 va 0,52), nut ra o 0,65
            int hit = progress >= 0.65f ? 3 : progress >= 0.52f ? 2 : progress >= 0.27f ? 1 : 0;
            if (lastStrike < 0) lastStrike = 0;
            while (lastStrike < hit)
            {
                lastStrike++;
                // go 1, go 2, roi nut: manh vo trung bat ra
                Strike(lastStrike == 3 ? 4 : 1, lastStrike == 3 ? 1f : 0.3f, UiKit.Hex("#F4DDB8"));
            }
        }

        static float TapCurve(float p, float a, float b)
        {
            if (p < a || p > b) return 0f;
            float k = (p - a) / (b - a);
            return k < 0.6f ? Mathf.SmoothStep(0f, 1f, k / 0.6f) : 1f - Mathf.SmoothStep(0f, 1f, (k - 0.6f) / 0.4f);
        }

        /// <summary>Rua: mon lac trai phai, giot nuoc ban tu tren xuong; dao nam yen.</summary>
        void Wash(float k, int n)
        {
            RestKnife(Time.deltaTime);
            item.localRotation = Quaternion.Euler(0f, Mathf.Sin(k * Mathf.PI * 2f) * 8f, Mathf.Sin(k * Mathf.PI * 2f) * 12f);
            item.localPosition = itemRest + new Vector3(0f, 0.01f * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)), 0f);
            if (n != lastStrike)
            {
                lastStrike = n;
                Strike(4, 0.8f, Water);
            }
        }

        /// <summary>Loc: dao nam ngang, luot toi lui doc than mon.</summary>
        void Fillet(float k, int n)
        {
            float x = Mathf.Sin(k * Mathf.PI * 2f) * 0.07f;
            knife.localPosition = itemRest + new Vector3(0.3f + x, 0.05f, 0f);
            knife.localRotation = Quaternion.Euler(0f, 0f, -4f);
            if (n != lastStrike)
            {
                lastStrike = n;
                Strike(1, 0.35f, null);
            }
        }

        /// <summary>Mot nhat: mon nay 10-15% (Art Bible muc 11), vang vun / manh / giot.</summary>
        void Strike(int count, float speed, Color? color)
        {
            Strikes++;
            ItemView v = item != null ? item.GetComponent<ItemView>() : null;
            if (v != null && v.Idle != null) v.Idle.Boing();
            Color c = color ?? IngredientArt.ColorOf(shownFor != null ? shownFor.typeId : null);
            for (int i = 0; i < count; i++)
            {
                Transform t = Toon.Prim(PrimitiveType.Sphere, transform, "Vun", itemRest + new Vector3(0f, 0.08f, 0f), Vector3.one * Random.Range(0.018f, 0.03f), c, 0.004f).transform;
                Vector2 d = Random.insideUnitCircle.normalized;
                bits.Add(new Bit { t = t, v = new Vector3(d.x * 0.5f, 1.1f + Random.value * 0.5f, d.y * 0.4f) * speed, life = 0.55f });
            }
        }

        void TickBits(float dt)
        {
            for (int i = bits.Count - 1; i >= 0; i--)
            {
                Bit b = bits[i];
                b.life -= dt;
                if (b.t == null || b.life <= 0f)
                {
                    if (b.t != null) Destroy(b.t.gameObject);
                    bits.RemoveAt(i);
                    continue;
                }
                b.v += Vector3.down * 4.5f * dt;
                Vector3 p = b.t.localPosition + b.v * dt;
                if (p.y < 0.01f) { p.y = 0.01f; b.v = new Vector3(b.v.x * 0.4f, -b.v.y * 0.3f, b.v.z * 0.4f); }
                b.t.localPosition = p;
                b.t.localScale = Vector3.one * Mathf.Max(0.001f, b.t.localScale.x * (b.life < 0.2f ? 0.9f : 1f));
            }
        }

        public int BitsAlive { get { return bits.Count; } }
    }
}
