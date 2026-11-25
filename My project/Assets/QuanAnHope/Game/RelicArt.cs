using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Hinh relic ghep khoi (F-020; Art Bible muc 5 "hinh khoi tron", vien dam). Model dep lam o sprint 8.
    /// Kich thuoc o scale 1: cao khoang 0,3 m, dat tren mat be. Goc toa do = day vat.
    /// </summary>
    public static class RelicArt
    {
        const float O = Toon.OutlineSmall;

        /// <summary>Mau vien the / de theo do hiem (Q-30: theo thang mau bac, Art Bible muc 4).</summary>
        public static Color RarityColor(string rarity)
        {
            switch (rarity)
            {
                case "hiem": return UiKit.Lilac;
                case "huyenthoai": return UiKit.Mustard;
                default: return UiKit.Mint;
            }
        }

        static Color C(string hex) { return UiKit.Hex(hex); }

        static GameObject P(PrimitiveType t, Transform p, string n, float x, float y, float z, float sx, float sy, float sz, string hex,
            float ex = 0f, float ey = 0f, float ez = 0f)
        {
            return Toon.Prim(t, p, n, new Vector3(x, y, z), new Vector3(sx, sy, sz), C(hex), O, new Vector3(ex, ey, ez));
        }

        const PrimitiveType Box = PrimitiveType.Cube;
        const PrimitiveType Cyl = PrimitiveType.Cylinder;
        const PrimitiveType Sph = PrimitiveType.Sphere;
        const PrimitiveType Cap = PrimitiveType.Capsule;

        public static GameObject Create(Transform parent, RelicDef r, float scale)
        {
            var go = new GameObject("Relic_" + r.id);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * scale;
            Transform t = go.transform;
            switch (r.id)
            {
                case "V1": // binh tuoi gi
                    P(Cyl, t, "Than", 0f, 0.1f, 0f, 0.2f, 0.2f, 0.2f, "#7DB87A");
                    P(Sph, t, "Gi", 0.05f, 0.12f, -0.09f, 0.06f, 0.05f, 0.02f, "#A5644A");
                    P(Cyl, t, "Voi", 0.15f, 0.17f, 0f, 0.035f, 0.2f, 0.035f, "#7DB87A", 0f, 0f, -55f);
                    P(Cyl, t, "Sen", 0.24f, 0.24f, 0f, 0.07f, 0.03f, 0.07f, "#5E8F3E", 0f, 0f, -55f);
                    P(Box, t, "Quai", -0.1f, 0.17f, 0f, 0.03f, 0.12f, 0.04f, "#5E8F3E");
                    break;
                case "V2": // ro tre ba ngoai
                    P(Cyl, t, "Ro", 0f, 0.07f, 0f, 0.3f, 0.14f, 0.3f, "#D69B68");
                    P(Cyl, t, "Nan", 0f, 0.1f, 0f, 0.31f, 0.02f, 0.31f, "#B97F52");
                    P(Box, t, "Quai", 0f, 0.2f, 0f, 0.26f, 0.025f, 0.03f, "#B97F52");
                    P(Sph, t, "Rau", 0.05f, 0.15f, 0f, 0.1f, 0.06f, 0.1f, "#7DB87A");
                    break;
                case "V3": // phan bon bi truyen
                    P(Box, t, "Bao", 0f, 0.12f, 0f, 0.2f, 0.24f, 0.12f, "#B97F52");
                    P(Box, t, "Mieng", 0f, 0.25f, 0f, 0.16f, 0.03f, 0.1f, "#8C6A3A", 0f, 0f, 8f);
                    P(Sph, t, "La", 0f, 0.13f, -0.065f, 0.08f, 0.08f, 0.01f, "#7DB87A");
                    break;
                case "V4": // hat giong vang
                    P(Box, t, "Goi", 0f, 0.12f, 0f, 0.18f, 0.24f, 0.03f, "#FFF3DC");
                    P(Sph, t, "Hat", 0f, 0.12f, -0.03f, 0.1f, 0.12f, 0.06f, "#F2C14E");
                    P(Sph, t, "Mam", 0f, 0.2f, -0.03f, 0.05f, 0.04f, 0.02f, "#7DB87A");
                    break;
                case "C1": // dao phay cu
                    P(Box, t, "Luoi", 0f, 0.15f, 0f, 0.24f, 0.13f, 0.015f, "#C9C1B6", 0f, 0f, 10f);
                    P(Box, t, "Lung", 0f, 0.22f, 0f, 0.24f, 0.015f, 0.02f, "#5A5250", 0f, 0f, 10f);
                    P(Cyl, t, "Can", -0.18f, 0.12f, 0f, 0.04f, 0.14f, 0.04f, "#B97F52", 0f, 0f, 100f);
                    P(Box, t, "De", 0f, 0.025f, 0f, 0.3f, 0.05f, 0.14f, "#D69B68");
                    break;
                case "C2": // thung da
                    P(Box, t, "Thung", 0f, 0.1f, 0f, 0.26f, 0.2f, 0.18f, "#9CC7D8");
                    P(Box, t, "Nap", 0f, 0.21f, 0f, 0.28f, 0.03f, 0.2f, "#FFFFFF");
                    P(Box, t, "Da1", -0.05f, 0.25f, 0f, 0.06f, 0.06f, 0.06f, "#EFF7FA", 15f, 30f, 0f);
                    P(Box, t, "Da2", 0.05f, 0.25f, 0.02f, 0.05f, 0.05f, 0.05f, "#EFF7FA", 0f, 20f, 25f);
                    break;
                case "C3": // moi quen cho ca: ca tren khay giay
                    P(Box, t, "Khay", 0f, 0.02f, 0f, 0.3f, 0.04f, 0.16f, "#FFF3DC");
                    P(Sph, t, "Ca", 0f, 0.08f, 0f, 0.24f, 0.08f, 0.1f, "#9CC7D8");
                    P(Sph, t, "Duoi", 0.14f, 0.08f, 0f, 0.06f, 0.08f, 0.02f, "#6F9FB2");
                    P(Sph, t, "Mat", -0.08f, 0.1f, -0.045f, 0.025f, 0.025f, 0.01f, "#2B2321");
                    break;
                case "C4": // can vang
                    P(Cyl, t, "De", 0f, 0.02f, 0f, 0.18f, 0.04f, 0.18f, "#F2C14E");
                    P(Cyl, t, "Cot", 0f, 0.14f, 0f, 0.03f, 0.22f, 0.03f, "#F2C14E");
                    P(Box, t, "Don", 0f, 0.25f, 0f, 0.3f, 0.02f, 0.02f, "#F2C14E");
                    P(Cyl, t, "DiaTrai", -0.14f, 0.18f, 0f, 0.1f, 0.015f, 0.1f, "#E0A93A");
                    P(Cyl, t, "DiaPhai", 0.14f, 0.18f, 0f, 0.1f, 0.015f, 0.1f, "#E0A93A");
                    break;
                case "S1": // ban do hai dao
                    P(Box, t, "Giay", 0f, 0.012f, 0f, 0.28f, 0.012f, 0.2f, "#F3DFB8", 30f, 0f, 0f);
                    P(Cyl, t, "CuonTren", 0f, 0.12f, 0.08f, 0.04f, 0.3f, 0.04f, "#E9CE9E", 0f, 0f, 90f);
                    P(Sph, t, "Dao", 0.03f, 0.06f, -0.04f, 0.08f, 0.01f, 0.05f, "#7DB87A", 30f, 0f, 0f);
                    P(Box, t, "X", -0.06f, 0.07f, -0.02f, 0.04f, 0.008f, 0.008f, "#E8604F", 30f, 45f, 0f);
                    break;
                case "S2": // luoi thep
                    P(Cyl, t, "Cuon", 0f, 0.09f, 0f, 0.24f, 0.18f, 0.24f, "#5A5250");
                    P(Cyl, t, "Mat", 0f, 0.181f, 0f, 0.2f, 0.004f, 0.2f, "#9AA4A8");
                    P(Sph, t, "Phao", 0.12f, 0.16f, -0.08f, 0.06f, 0.06f, 0.06f, "#E8604F");
                    break;
                case "S3": // thuyen thue
                    P(Sph, t, "Than", 0f, 0.06f, 0f, 0.3f, 0.1f, 0.12f, "#B97F52");
                    P(Box, t, "San", 0f, 0.08f, 0f, 0.24f, 0.02f, 0.09f, "#D69B68");
                    P(Cyl, t, "Cot", 0f, 0.18f, 0f, 0.015f, 0.22f, 0.015f, "#8C6A3A");
                    P(Box, t, "Buom", 0.05f, 0.2f, 0f, 0.09f, 0.14f, 0.008f, "#FFF3DC");
                    break;
                case "S4": // la ban rong
                    P(Cyl, t, "Vo", 0f, 0.03f, 0f, 0.24f, 0.06f, 0.24f, "#F2C14E");
                    P(Cyl, t, "Mat", 0f, 0.061f, 0f, 0.2f, 0.004f, 0.2f, "#FFF3DC");
                    P(Box, t, "Kim", 0f, 0.068f, 0f, 0.16f, 0.006f, 0.025f, "#D9382C", 0f, 40f, 0f);
                    P(Sph, t, "Ngoc", 0f, 0.1f, 0.13f, 0.06f, 0.06f, 0.06f, "#C9B3F0");
                    break;
                case "F1": // tu lanh 2 cua
                    P(Box, t, "Than", 0f, 0.15f, 0f, 0.2f, 0.3f, 0.16f, "#FFFFFF");
                    P(Box, t, "Khe", 0f, 0.2f, -0.081f, 0.18f, 0.008f, 0.004f, "#9CC7D8");
                    P(Box, t, "Tay1", 0.07f, 0.25f, -0.085f, 0.015f, 0.06f, 0.01f, "#C9C1B6");
                    P(Box, t, "Tay2", 0.07f, 0.12f, -0.085f, 0.015f, 0.08f, 0.01f, "#C9C1B6");
                    break;
                case "F2": // chuong vang
                    P(Cyl, t, "De", 0f, 0.015f, 0f, 0.24f, 0.03f, 0.24f, "#B97F52");
                    P(Sph, t, "Chuong", 0f, 0.06f, 0f, 0.2f, 0.16f, 0.2f, "#F2C14E");
                    P(Sph, t, "Num", 0f, 0.15f, 0f, 0.04f, 0.04f, 0.04f, "#E0A93A");
                    break;
                case "F3": // dong ho cat
                    P(Cyl, t, "Day", 0f, 0.015f, 0f, 0.18f, 0.03f, 0.18f, "#B97F52");
                    P(Cyl, t, "Nap", 0f, 0.285f, 0f, 0.18f, 0.03f, 0.18f, "#B97F52");
                    P(Sph, t, "BauDuoi", 0f, 0.09f, 0f, 0.13f, 0.12f, 0.13f, "#F2C14E");
                    P(Sph, t, "BauTren", 0f, 0.21f, 0f, 0.13f, 0.12f, 0.13f, "#FFF3DC");
                    break;
                case "F4": // muoi hong
                    P(Cyl, t, "Lo", 0f, 0.09f, 0f, 0.15f, 0.18f, 0.15f, "#E9A0A0");
                    P(Cyl, t, "Nap", 0f, 0.195f, 0f, 0.16f, 0.04f, 0.16f, "#FFFFFF");
                    break;
                case "F5": // chao gang ba ngoai
                    P(Cyl, t, "Chao", 0f, 0.03f, 0f, 0.26f, 0.06f, 0.26f, "#5A5250");
                    P(Cyl, t, "Long", 0f, 0.055f, 0f, 0.21f, 0.012f, 0.21f, "#6E6462");
                    P(Cyl, t, "Can", 0.2f, 0.05f, 0f, 0.035f, 0.16f, 0.035f, "#5A5250", 0f, 0f, 90f);
                    break;
                case "F6": // tuong meo vang
                    P(Sph, t, "Than", 0f, 0.09f, 0f, 0.18f, 0.18f, 0.15f, "#F2C14E");
                    P(Sph, t, "Dau", 0f, 0.22f, 0f, 0.14f, 0.12f, 0.12f, "#F2C14E");
                    P(Sph, t, "Tai1", -0.045f, 0.29f, 0f, 0.04f, 0.04f, 0.02f, "#E0A93A");
                    P(Sph, t, "Tai2", 0.045f, 0.29f, 0f, 0.04f, 0.04f, 0.02f, "#E0A93A");
                    P(Cap, t, "Tay", 0.09f, 0.22f, 0f, 0.04f, 0.08f, 0.04f, "#F2C14E", 0f, 0f, -15f);
                    break;
                case "F7": // may pha ca phe xin
                    P(Box, t, "Than", 0f, 0.13f, 0.02f, 0.2f, 0.26f, 0.12f, "#5A5250");
                    P(Box, t, "Dau", 0f, 0.2f, -0.06f, 0.14f, 0.04f, 0.08f, "#C9C1B6");
                    P(Cyl, t, "Coc", 0f, 0.04f, -0.07f, 0.06f, 0.07f, 0.06f, "#FFF3DC");
                    break;
                case "F8": // ot ma
                    P(Sph, t, "Qua", 0f, 0.1f, 0f, 0.1f, 0.24f, 0.1f, "#D9382C", 0f, 0f, 20f);
                    P(Cyl, t, "Cuong", -0.04f, 0.23f, 0f, 0.025f, 0.06f, 0.025f, "#5E8F3E", 0f, 0f, 20f);
                    break;
                case "F9": // hu sot cu
                    P(Sph, t, "Hu", 0f, 0.1f, 0f, 0.2f, 0.2f, 0.2f, "#A5644A");
                    P(Cyl, t, "Co", 0f, 0.2f, 0f, 0.1f, 0.04f, 0.1f, "#8C5A3A");
                    P(Cyl, t, "Nap", 0f, 0.225f, 0f, 0.12f, 0.02f, 0.12f, "#FFF3DC");
                    break;
                case "F10": // ve so cao
                    P(Box, t, "Ve", 0f, 0.12f, 0f, 0.2f, 0.24f, 0.012f, "#F2C14E", 0f, 0f, -8f);
                    P(Box, t, "Cao", 0f, 0.1f, -0.007f, 0.14f, 0.08f, 0.004f, "#C9C1B6", 0f, 0f, -8f);
                    break;
                case "F11": // sao bang trong noi (2.1 muc 7.3): noi nho, ngoi sao vang bay ra co duoi cau vong
                    P(Cyl, t, "Noi", 0f, 0.07f, 0f, 0.22f, 0.14f, 0.22f, "#E8604F");
                    P(Cyl, t, "Vanh", 0f, 0.145f, 0f, 0.24f, 0.02f, 0.24f, "#C94A3B");
                    P(Box, t, "Sao1", 0.02f, 0.27f, 0f, 0.09f, 0.09f, 0.03f, "#F2C14E", 0f, 0f, 45f);
                    P(Box, t, "Sao2", 0.02f, 0.27f, 0f, 0.09f, 0.09f, 0.03f, "#F2C14E");
                    P(Cyl, t, "Duoi", -0.05f, 0.2f, 0f, 0.025f, 0.12f, 0.025f, "#A77BD6", 0f, 0f, 40f);
                    break;
                case "F12": // bi kip gia truyen: cuon sach bia do day vang, dau trang lap lanh
                    P(Box, t, "Bia", 0f, 0.04f, 0f, 0.24f, 0.08f, 0.18f, "#B8463C");
                    P(Box, t, "Giay", 0.005f, 0.04f, 0f, 0.22f, 0.065f, 0.17f, "#FFF3DC");
                    P(Box, t, "Day", 0f, 0.04f, 0f, 0.03f, 0.085f, 0.185f, "#F2C14E");
                    P(Sph, t, "LapLanh", 0.06f, 0.13f, 0f, 0.06f, 0.06f, 0.06f, "#F2C14E");
                    break;
                default:
                    P(Sph, t, "Hop", 0f, 0.12f, 0f, 0.2f, 0.2f, 0.2f, "#C9B3F0");
                    break;
            }
            return go;
        }
    }

    /// <summary>
    /// The dung truoc be relic (F-020 AC-6; constitution Dieu 4, Dieu 9; Art Bible muc 13): ten · gia · do hiem · hieu ung.
    /// Khong ghi bo combo / relic le (yeu cau 21/11/2026, constitution 1.7 muc 7.1).
    /// Vien the theo mau do hiem.
    /// </summary>
    public class RelicCard : MonoBehaviour
    {
        static readonly Vector2 Size = new Vector2(1100f, 660f);
        const float Border = 26f;

        Image frame;
        TextMeshProUGUI title, meta, effect, setLine;

        public string Title { get { return title.text; } }
        public string Meta { get { return meta.text; } }
        public string EffectText { get { return effect.text; } }
        public string SetText { get { return setLine.text; } }
        public Color FrameColor { get { return frame.color; } }

        public static RelicCard Create(Transform parent, string name, Vector3 localPos)
        {
            // Nghieng 12 do ve phia sau nhu the dung tren ban.
            Canvas c = UiKit.WorldCanvas(parent, name, localPos, Quaternion.Euler(12f, 0f, 0f), Size);
            var card = c.gameObject.AddComponent<RelicCard>();
            card.frame = UiKit.Panel(c.transform, "Vien", new Vector2(0.5f, 0.5f), Vector2.zero, Size, UiKit.Mint);
            Image inner = UiKit.Panel(card.frame.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero,
                Size - Vector2.one * (2f * Border), UiKit.Cream, false);
            float w = Size.x - 2f * Border - 50f;
            card.title = UiKit.Label(inner.transform, "Ten", new Vector2(0.5f, 1f), new Vector2(0f, -15f), new Vector2(w, 130f), "", 100f);
            card.meta = UiKit.Label(inner.transform, "GiaDoHiem", new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(w, 95f), "", 78f);
            card.effect = UiKit.Label(inner.transform, "HieuUng", new Vector2(0.5f, 1f), new Vector2(0f, -245f), new Vector2(w, 200f), "", 76f);
            card.setLine = UiKit.Label(inner.transform, "Bo", new Vector2(0.5f, 0f), new Vector2(0f, 15f), new Vector2(w, 85f), "", 64f);
            return card;
        }

        public void Show(RelicDef r, RelicInventory inv, bool pending, bool viaBlack = false, bool free = false)
        {
            gameObject.SetActive(true);
            frame.color = RelicArt.RarityColor(r.rarity);
            title.text = r.name;
            meta.text = (viaBlack ? "Vé đen" : free ? "<color=#E8604F>Miễn phí</color>" : r.price + " vé") + " · " + RelicKeys.RarityName(r.rarity);
            effect.text = RelicText.Effect(r)
                + (RelicText.IsActiveNow(r) ? "" : "\n<color=#E8604F><size=46>có tác dụng từ ngày 4</size></color>");
            // Yeu cau 21/11/2026: khong hien relic nao thuoc bo combo, relic nao la relic le (nguoi choi tu kham pha).
            setLine.text = "";
            if (pending) setLine.text = "<color=#E8604F>Bấm relic trên giá để bỏ</color>";
        }

        public void ShowEmpty(string text)
        {
            gameObject.SetActive(true);
            frame.color = UiKit.Disabled;
            title.text = text;
            meta.text = "";
            effect.text = "";
            setLine.text = "";
        }
    }

    /// <summary>Relic vua mua bay theo duong cong tu be len o tren gia (0,7 giay) roi lap lanh (Art Bible muc 11).</summary>
    public class RelicFly : MonoBehaviour
    {
        const float Seconds = 0.7f;
        const float Arc = 0.6f;
        Vector3 from;
        Transform slot;
        Vector3 fullScale;
        float t;
        float startScale = 1f;

        public bool Done { get { return t >= 1f; } }

        public static RelicFly Play(GameObject relic, Vector3 fromWorld, Transform slot, float startScale)
        {
            var f = relic.AddComponent<RelicFly>();
            f.from = fromWorld;
            f.slot = slot;
            f.fullScale = relic.transform.localScale;
            f.startScale = startScale;
            relic.transform.position = fromWorld;
            return f;
        }

        void Update()
        {
            t = Mathf.Min(1f, t + Time.deltaTime / Seconds);
            float e = t * t * (3f - 2f * t);
            Vector3 to = slot.position;
            transform.position = Vector3.Lerp(from, to, e) + Vector3.up * Arc * Mathf.Sin(e * Mathf.PI);
            transform.localRotation = Quaternion.Euler(0f, 360f * e, 0f);
            // co tren be -> co tren gia: giu cam giac cung mot vat
            transform.localScale = fullScale * Mathf.Lerp(startScale, 1f, e);
            if (t >= 1f)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = fullScale;
                Sparkle.Play(slot);
                Destroy(this);
            }
        }
    }

    /// <summary>Lap lanh khi mua relic (Art Bible muc 11): vai ngoi sao 4 canh bay len roi mo, tu huy.</summary>
    public class Sparkle : MonoBehaviour
    {
        const float Seconds = 0.9f;
        FaceBillboard[] parts;
        Vector3[] starts;
        float t;
        MaterialPropertyBlock block;

        public static Sparkle Play(Transform at)
        {
            var go = new GameObject("LapLanh");
            go.transform.SetParent(at, false);
            var s = go.AddComponent<Sparkle>();
            s.parts = new FaceBillboard[4];
            s.starts = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                s.starts[i] = new Vector3(Mathf.Cos(a) * 0.14f, 0.12f + 0.04f * i, Mathf.Sin(a) * 0.05f);
                Transform spot = Toon.Empty(go.transform, "Goc_" + i, s.starts[i]).transform;
                s.parts[i] = FaceBillboard.Create(go.transform, spot, FaceArt.Fx("lap_lanh"), 0.1f, 0f, "Sao_" + i);
            }
            return s;
        }

        void Update()
        {
            t += Time.deltaTime / Seconds;
            if (block == null) block = new MaterialPropertyBlock();
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null) continue;
                parts[i].Anchor.localPosition = starts[i] + Vector3.up * 0.2f * t;
                MeshRenderer r = parts[i].Renderer;
                r.GetPropertyBlock(block);
                block.SetColor("_Color", new Color(1f, 1f, 1f, Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI)));
                r.SetPropertyBlock(block);
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
