using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Do trong dung cu tren bep nhin ra qua trinh nau an (F-013; ghi chu nguoi dung 14/11 "xau va lon xon,
    /// khong giong qua trinh nau an"; constitution muc 5.1, 5.8):
    /// dau / bo / nuoc dung loang thanh lop o day, nguyen lieu roi vao thanh cac mieng da cat rai deu,
    /// vat num thi co lua, chao duoc dao / noi soi / vi xeo, mau dam dan theo tien do, hoi boc len;
    /// chin thi trong dung cu la mon hoan chinh theo cach nau; sai thu tu thi chay xem, khoi xam.
    /// Gan vao goc "DoTrongBep"; RoomView goi Add* khi bep nhan them do va Tick moi khung hinh.
    /// </summary>
    public class PanView : MonoBehaviour
    {
        // Lop day: (mau, ban kinh so voi long dung cu)
        static readonly Dictionary<string, string> LayerColors = new Dictionary<string, string>
        {
            { "Dầu ăn", "#F5CF5B" }, { "Bơ", "#FBE7A1" }, { "Nước dùng", "#E3B979" },
            { "Nước mắm", "#B5793F" }, { "Nước tương", "#6B4430" }, { "Mật ong", "#E8A33D" },
        };
        static readonly HashSet<string> FullLayers = new HashSet<string> { "Dầu ăn", "Bơ", "Nước dùng" };

        const string RawEgg = "#F6C445", CookedEgg = "#F9DD7A", Burnt = "#3A2E28", Smoke = "#8C8580", SteamColor = "#FFFFFF";

        float radius, scale;
        string tool;
        bool deep;
        float flameY;
        int ringIndex;

        readonly List<Renderer> tinted = new List<Renderer>();
        readonly List<Color> baseColors = new List<Color>();
        readonly List<Transform> pieces = new List<Transform>();
        readonly List<Vector3> homes = new List<Vector3>();
        readonly List<Transform> spreading = new List<Transform>();
        Transform eggLiquid;
        Transform flames;
        Transform steam;
        Transform bubbles;
        GameObject finished;
        MaterialPropertyBlock block;
        float clock;

        /// <summary>Mon hoan chinh dang nam trong dung cu (F-013 AC-4); null khi chua chin.</summary>
        public GameObject Finished { get { return finished; } }

        /// <summary>Cac mieng nguyen lieu dang nam trong dung cu.</summary>
        public IReadOnlyList<Transform> Pieces { get { return pieces; } }

        /// <summary>Lua duoi dung cu dang chay.</summary>
        public bool FlameOn { get { return flames != null && flames.gameObject.activeSelf; } }

        /// <summary>Do dam mau hien tai (0 = song, 1 = chin han).</summary>
        public float Doneness { get; private set; }

        public bool ShowsBurnt { get; private set; }

        public void Setup(string toolName, float innerRadius, float itemScale, float burnerLocalY)
        {
            if (tool == toolName && Mathf.Approximately(radius, innerRadius)) return;
            tool = toolName;
            radius = innerRadius;
            scale = itemScale;
            deep = toolName == "Nồi";
            flameY = burnerLocalY;
            if (flames != null) Destroy(flames.gameObject);
            flames = null;
        }

        /// <summary>Do sach moi thu (muc ra dia, do bo, het ngay).</summary>
        public void Clear()
        {
            tinted.Clear();
            baseColors.Clear();
            pieces.Clear();
            homes.Clear();
            spreading.Clear();
            eggLiquid = null;
            ringIndex = 0;
            Doneness = 0f;
            ShowsBurnt = false;
            if (finished != null) Destroy(finished);
            finished = null;
            if (flames != null) flames.gameObject.SetActive(false);
            if (steam != null) steam.gameObject.SetActive(false);
            if (bubbles != null) bubbles.gameObject.SetActive(false);
        }

        // =====================================================================
        // Cho vao
        // =====================================================================

        /// <summary>Do kho: lop dau / bo / nuoc dung phu day, nuoc mam / tuong / mat ong thanh vet sot, toi / muoi / hanh la rac hat.</summary>
        public GameObject AddPantry(string item, Color jarColor)
        {
            GameObject go = Toon.Empty(transform, "GiaVi_" + item, Vector3.zero);
            string hex;
            if (LayerColors.TryGetValue(item, out hex))
            {
                float r = (FullLayers.Contains(item) ? 0.86f : 0.5f) * radius;
                float y = FullLayers.Contains(item) ? 0.002f : 0.004f;
                GameObject disc = Toon.Prim(PrimitiveType.Cylinder, go.transform, "Lop", new Vector3(0f, y, 0f), new Vector3(2f * r, 0.004f, 2f * r), UiKit.Hex(hex), 0f);
                Register(disc.GetComponent<Renderer>());
                disc.transform.localScale = Vector3.Scale(disc.transform.localScale, new Vector3(0.25f, 1f, 0.25f));
                spreading.Add(disc.transform);
                return go;
            }
            // Hat nho: toi bam, muoi, hanh la
            string bit = item == "Hành lá" ? "#6FA855" : item == "Muối" ? "#FFFFFF" : "#F3E9CF";
            for (int i = 0; i < 7; i++)
            {
                Vector3 p = Spot(i, 7, 0.5f * radius, 0.25f * radius, 20f);
                GameObject g = Toon.Prim(item == "Hành lá" ? PrimitiveType.Cylinder : PrimitiveType.Cube, go.transform, "Hat_" + i, p + new Vector3(0f, 0.008f, 0f),
                    item == "Hành lá" ? new Vector3(0.008f, 0.014f, 0.008f) : Vector3.one * 0.009f, UiKit.Hex(bit), 0f,
                    new Vector3(item == "Hành lá" ? 90f : 0f, 40f * i, 0f));
                Register(g.GetComponent<Renderer>());
                pieces.Add(g.transform);
                homes.Add(g.transform.localPosition);
            }
            return go;
        }

        /// <summary>Nguyen lieu da so che: roi vao thanh 4-6 mieng rai deu tren mot vong, khong chong nhau; trung danh thanh lop long.</summary>
        public GameObject AddIngredient(Ingredient ing)
        {
            GameObject go = Toon.Empty(transform, "NguyenLieu_" + ing.typeId, Vector3.zero);
            int count;
            List<IngredientModels.Part> parts = IngredientModels.Piece(ing.typeId, out count);
            if (count == 0)
            {
                // Trung danh: lop trung long vang phu day, chin thi dong dan tu vien vao (F-013 kich ban).
                GameObject cooked = Toon.Prim(PrimitiveType.Cylinder, go.transform, "TrungChin", new Vector3(0f, 0.006f, 0f),
                    new Vector3(1.6f * radius, 0.006f, 1.6f * radius), UiKit.Hex(CookedEgg), 0f);
                GameObject liquid = Toon.Prim(PrimitiveType.Cylinder, go.transform, "TrungLong", new Vector3(0f, 0.0075f, 0f),
                    new Vector3(1.6f * radius, 0.006f, 1.6f * radius), UiKit.Hex(RawEgg), 0f);
                Register(cooked.GetComponent<Renderer>());
                eggLiquid = liquid.transform;
                spreading.Add(cooked.transform);
                spreading.Add(liquid.transform);
                cooked.transform.localScale = Vector3.Scale(cooked.transform.localScale, new Vector3(0.25f, 1f, 0.25f));
                liquid.transform.localScale = Vector3.Scale(liquid.transform.localScale, new Vector3(0.25f, 1f, 0.25f));
                return go;
            }

            int ring = ringIndex++;
            float rr = (deep ? (ring == 0 ? 0.42f : 0.2f) : (ring == 0 ? 0.6f : 0.3f)) * radius;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = Spot(i, count, rr, 0f, ring * 23f);
                float a = Mathf.Atan2(p.z - Center.z, p.x - Center.x) * Mathf.Rad2Deg;
                GameObject piece = Toon.Empty(go.transform, "Mieng_" + i, p + new Vector3(0f, 0.008f + 0.003f * ring, 0f), new Vector3(0f, -a + 90f, 0f));
                piece.transform.localScale = Vector3.one * scale;
                IngredientModels.BuildParts(piece.transform, parts, 1f);
                foreach (Renderer r in piece.GetComponentsInChildren<Renderer>()) if (r.name != "Outline") Register(r);
                pieces.Add(piece.transform);
                homes.Add(piece.transform.localPosition);
            }
            return go;
        }

        /// <summary>Tam vung dat do: chao / vi o giua; noi o nua sau de mep noi phia truoc khong che.</summary>
        Vector3 Center { get { return deep ? new Vector3(0f, 0f, 0.35f * radius) : Vector3.zero; } }

        Vector3 Spot(int i, int n, float r, float jitter, float offsetDeg)
        {
            float a = (offsetDeg + 360f * i / n) * Mathf.Deg2Rad;
            float rad = r + (i % 2 == 0 ? jitter : -jitter) * 0.5f;
            return Center + new Vector3(Mathf.Cos(a) * rad, 0f, Mathf.Sin(a) * rad);
        }

        void Register(Renderer r)
        {
            tinted.Add(r);
            baseColors.Add(r.sharedMaterial.GetColor("_Color"));
        }

        // =====================================================================
        // Nau
        // =====================================================================

        public void Tick(Stove stove, Recipe recipe, float dt)
        {
            clock += dt;
            foreach (Transform t in spreading)
            {
                if (t == null) continue;
                Vector3 s = t.localScale;
                float full = t.parent != null && t.parent.GetComponent<PanItem>() != null && !t.parent.GetComponent<PanItem>().Landed ? 0.25f : 1f;
                float target = full >= 1f ? FullScale(t) : s.x;
                if (full >= 1f && s.x < target) t.localScale = new Vector3(Mathf.Min(target, s.x + target * dt / 0.6f), s.y, Mathf.Min(target, s.z + target * dt / 0.6f));
            }

            bool cooking = stove.State == StoveState.Cooking;
            bool ready = stove.State == StoveState.Ready;
            float progress = cooking && stove.CookSeconds > 0f ? Mathf.Clamp01(stove.Elapsed / stove.CookSeconds) : ready ? 1f : 0f;
            bool burnt = (cooking || ready) && !stove.FollowsRecipe;
            Doneness = progress;
            ShowsBurnt = burnt && progress > 0.5f;

            Flames(cooking);
            Steam(cooking || ready, burnt);
            Bubbles(cooking && deep);

            if (ready && finished == null && recipe != null) ShowFinished(recipe, burnt);
            if (finished != null) return;

            // Mau dam dan theo tien do (sai thu tu: chay xem dan)
            if (block == null) block = new MaterialPropertyBlock();
            for (int i = 0; i < tinted.Count; i++)
            {
                if (tinted[i] == null) continue;
                Color c = Cooked(baseColors[i], progress, burnt);
                tinted[i].GetPropertyBlock(block);
                block.SetColor("_Color", c);
                tinted[i].SetPropertyBlock(block);
            }
            // Trung dong dan tu vien vao: lop long thu nho lai
            if (eggLiquid != null && cooking)
            {
                float k = Mathf.Max(0.02f, 1f - progress);
                float full = FullScale(eggLiquid);
                eggLiquid.localScale = new Vector3(full * k, eggLiquid.localScale.y, full * k);
            }

            // Dao (chao), soi lan tan (noi), xeo (vi)
            for (int i = 0; i < pieces.Count; i++)
            {
                Transform p = pieces[i];
                if (p == null) continue;
                Vector3 home = homes[i];
                if (!cooking) { p.localPosition = home; continue; }
                if (tool == "Chảo")
                {
                    float toss = Mathf.Repeat(clock / 1.4f, 1f);
                    float angle = toss < 0.35f ? Mathf.Sin(toss / 0.35f * Mathf.PI) * 28f : 0f;
                    float hop = toss < 0.35f ? Mathf.Sin(toss / 0.35f * Mathf.PI) * 0.03f * (1f + 0.3f * (i % 3)) : 0f;
                    p.localPosition = Quaternion.Euler(0f, angle, 0f) * home + Vector3.up * hop;
                }
                else if (deep)
                {
                    p.localPosition = home + Vector3.up * Mathf.Sin(clock * 3f + i) * 0.006f;
                }
                else
                {
                    p.localPosition = home + new Vector3(Mathf.Sin(clock * 40f + i) * 0.002f, Mathf.Abs(Mathf.Sin(clock * 20f + i)) * 0.004f, 0f);
                }
            }
        }

        float FullScale(Transform t)
        {
            // Cylinder do Toon.Prim tao: scale x = duong kinh that; ghi lai lan dau.
            Spread s = t.GetComponent<Spread>();
            if (s == null)
            {
                s = t.gameObject.AddComponent<Spread>();
                s.full = t.localScale.x * 4f;   // luc tao da thu nho 0,25
            }
            return s.full;
        }

        static Color Cooked(Color c, float progress, bool burnt)
        {
            Color brown = UiKit.Hex("#9A5A2C");
            Color done = Color.Lerp(c, brown, 0.32f) * 0.95f;
            done.a = 1f;
            Color k = Color.Lerp(c, done, progress);
            if (burnt) k = Color.Lerp(k, UiKit.Hex(Burnt), 0.65f * progress);
            return k;
        }

        void Flames(bool on)
        {
            if (!on)
            {
                if (flames != null) flames.gameObject.SetActive(false);
                return;
            }
            if (flames == null)
            {
                flames = Toon.Empty(transform, "Lua", new Vector3(0f, flameY, 0f)).transform;
                // Vong lua lo ra ngoai mep day dung cu de nguoi choi nhin thay
                float ring = radius * 1.2f;
                for (int i = 0; i < 14; i++)
                {
                    float a = 360f / 14f * i * Mathf.Deg2Rad;
                    Vector3 p = new Vector3(Mathf.Cos(a) * ring, 0f, Mathf.Sin(a) * ring);
                    Transform f = Toon.Empty(flames, "NgonLua_" + i, p).transform;
                    Toon.Prim(PrimitiveType.Capsule, f, "Xanh", new Vector3(0f, 0.02f, 0f), new Vector3(0.026f, 0.02f, 0.026f), UiKit.Hex("#5B8DEF"), 0f);
                    Toon.Prim(PrimitiveType.Capsule, f, "Cam", new Vector3(0f, 0.048f, 0f), new Vector3(0.016f, 0.016f, 0.016f), UiKit.Hex("#F2A65A"), 0f);
                }
            }
            flames.gameObject.SetActive(true);
            for (int i = 0; i < flames.childCount; i++)
            {
                Transform f = flames.GetChild(i);
                f.localScale = new Vector3(1f, 0.8f + 0.35f * Mathf.Abs(Mathf.Sin(clock * 11f + i * 1.7f)), 1f);
            }
        }

        void Steam(bool on, bool burnt)
        {
            if (!on)
            {
                if (steam != null) steam.gameObject.SetActive(false);
                return;
            }
            if (steam == null)
            {
                steam = Toon.Empty(transform, "Hoi", Vector3.zero).transform;
                for (int i = 0; i < 4; i++)
                    Toon.Prim(PrimitiveType.Sphere, steam, "Lan_" + i, Vector3.zero, Vector3.one * 0.05f, UiKit.Hex(SteamColor), 0f);
            }
            steam.gameObject.SetActive(true);
            Color c = UiKit.Hex(burnt ? Smoke : SteamColor);
            for (int i = 0; i < steam.childCount; i++)
            {
                Transform puff = steam.GetChild(i);
                Toon.Recolor(puff.gameObject, c);
                float u = Mathf.Repeat(clock / 2.2f + i / 4f, 1f);
                Vector3 basePos = Center + new Vector3(Mathf.Cos(i * 1.9f) * 0.3f * radius, 0f, Mathf.Sin(i * 1.9f) * 0.3f * radius);
                puff.localPosition = basePos + new Vector3(Mathf.Sin(u * 6f + i) * 0.02f, 0.05f + 0.35f * u, 0f);
                puff.localScale = Vector3.one * (burnt ? 0.08f : 0.055f) * Mathf.Sin(u * Mathf.PI);
            }
        }

        void Bubbles(bool on)
        {
            if (!on)
            {
                if (bubbles != null) bubbles.gameObject.SetActive(false);
                return;
            }
            if (bubbles == null)
            {
                bubbles = Toon.Empty(transform, "BotSoi", Vector3.zero).transform;
                for (int i = 0; i < 6; i++)
                    Toon.Prim(PrimitiveType.Sphere, bubbles, "Bot_" + i, Vector3.zero, Vector3.one * 0.02f, UiKit.Hex("#FFF8EE"), 0f);
            }
            bubbles.gameObject.SetActive(true);
            for (int i = 0; i < bubbles.childCount; i++)
            {
                float u = Mathf.Repeat(clock * 1.3f + i / 6f, 1f);
                Transform b = bubbles.GetChild(i);
                b.localPosition = Spot(i, 6, 0.55f * radius, 0.2f * radius, 31f * i) + Vector3.up * (0.006f + 0.01f * u);
                b.localScale = Vector3.one * 0.022f * Mathf.Sin(u * Mathf.PI);
            }
        }

        void ShowFinished(Recipe recipe, bool burnt)
        {
            foreach (Transform child in transform)
                if (child.GetComponent<PanItem>() != null) child.gameObject.SetActive(false);
            MainIngredient main = null;
            foreach (MainIngredient m in DishFood.Book.mains) if (m.id == recipe.mainId) main = m;
            finished = DishFood.Build(transform, recipe.mainId, recipe.sideId, main != null ? main.method : null, burnt, radius, scale, Center);
            finished.name = "MonChin";
            Pop.Play(finished, 0.6f);
        }
    }

    /// <summary>Ghi kich thuoc day du cua mot lop dang loang ra.</summary>
    public class Spread : MonoBehaviour
    {
        public float full;
    }

    /// <summary>
    /// Hinh mon an hoan chinh theo cach nau cua nguyen lieu chinh (F-013 AC-4, AC-5): dung chung cho trong dung cu va tren dia.
    /// chien: mot mieng trung tron co do phu; xao: cac mieng tron lan; sot: mieng noi trong nuoc sot;
    /// ap chao / nuong / hap: mot mieng lon giua + do phu (muc co ban, lam ky o sprint 4-5).
    /// </summary>
    public static class DishFood
    {
        /// <summary>So lieu cong thuc (de biet cach nau). GameController gan luc tao ban.</summary>
        public static RecipeBook Book;

        public static GameObject Build(Transform parent, string mainId, string sideId, string method, bool burnt, float radius, float pieceScale, Vector3 center)
        {
            GameObject root = Toon.Empty(parent, "MonAn", center);
            System.Func<Color, Color> fix = c =>
            {
                Color k = Color.Lerp(c, UiKit.Hex("#9A5A2C"), 0.3f) * 0.96f;
                if (burnt) k = Color.Lerp(k, UiKit.Hex("#3A2E28"), 0.6f);
                k.a = 1f;
                return k;
            };
            int sideCount, mainCount;
            List<IngredientModels.Part> side = IngredientModels.Piece(sideId, out sideCount);
            List<IngredientModels.Part> main = IngredientModels.Piece(mainId, out mainCount);
            var rng = new System.Random((mainId + sideId).GetHashCode());

            switch (method)
            {
                case "chiên":
                    {
                        // Mot mieng trung chien tron, vien vang sam, do phu nam tren
                        Toon.Prim(PrimitiveType.Cylinder, root.transform, "VienTrung", new Vector3(0f, 0.005f, 0f), new Vector3(1.62f * radius, 0.01f, 1.55f * radius), fix(UiKit.Hex("#E3A23B")), 0.004f);
                        Toon.Prim(PrimitiveType.Cylinder, root.transform, "Trung", new Vector3(0f, 0.008f, 0f), new Vector3(1.42f * radius, 0.012f, 1.36f * radius), fix(UiKit.Hex("#F6D25A")), 0f);
                        for (int i = 0; i < 3; i++)
                            Toon.Prim(PrimitiveType.Sphere, root.transform, "Chay_" + i, Ring(0.35f * radius, 110f * i + 20f, 0.0145f), new Vector3(0.05f, 0.002f, 0.03f) * radius / 0.17f,
                                fix(UiKit.Hex("#D9963A")), 0f, new Vector3(0f, 50f * i, 0f));
                        Pieces(root.transform, side, Mathf.Max(3, sideCount - 1), 0.4f * radius, 0.016f, pieceScale * 0.85f, fix, rng);
                        break;
                    }
                case "sốt":
                case "hấp":
                    {
                        string sauce = burnt ? "#3A2E28" : sideId == "ca_chua" ? "#C8553D" : method == "hấp" ? "#E3B979" : "#B07A4A";
                        Toon.Prim(PrimitiveType.Cylinder, root.transform, "NuocSot", new Vector3(0f, 0.004f, 0f), new Vector3(1.7f * radius, 0.008f, 1.7f * radius), UiKit.Hex(sauce), 0f);
                        Pieces(root.transform, main, Mathf.Max(3, mainCount), 0.5f * radius, 0.002f, pieceScale, c => fix(Color.Lerp(c, UiKit.Hex("#E8C27A"), 0.5f)), rng);
                        Pieces(root.transform, side, 3, 0.2f * radius, 0.006f, pieceScale * 0.9f, fix, rng);
                        for (int i = 0; i < 5; i++)
                            Toon.Prim(PrimitiveType.Cylinder, root.transform, "HanhLa_" + i, Ring(0.3f * radius + 0.012f * i, 70f * i, 0.012f), new Vector3(0.006f, 0.01f, 0.006f) * pieceScale,
                                UiKit.Hex(burnt ? "#3A2E28" : "#6FA855"), 0f, new Vector3(90f, 30f * i, 0f));
                        break;
                    }
                case "xào":
                    {
                        Toon.Prim(PrimitiveType.Cylinder, root.transform, "Bong", new Vector3(0f, 0.003f, 0f), new Vector3(1.4f * radius, 0.004f, 1.4f * radius), fix(UiKit.Hex("#C98E5A")), 0f);
                        // Tron lan: chinh va phu xen ke tren hai vong
                        Pieces(root.transform, main, Mathf.Max(4, mainCount), 0.55f * radius, 0.006f, pieceScale, fix, rng, 0f);
                        Pieces(root.transform, side, Mathf.Max(4, sideCount - 1), 0.28f * radius, 0.012f, pieceScale, fix, rng, 30f);
                        Pieces(root.transform, main, 2, 0.1f * radius, 0.018f, pieceScale, fix, rng, 70f);
                        break;
                    }
                default:
                    {
                        // ap chao / nuong: mieng lon giua + do phu quanh
                        Pieces(root.transform, main, 1, 0f, 0.004f, pieceScale * 1.8f, fix, rng);
                        Pieces(root.transform, side, Mathf.Max(4, sideCount - 1), 0.62f * radius, 0.006f, pieceScale * 0.9f, fix, rng);
                        break;
                    }
            }
            // Tam mon (mat mon, hoi bay len)
            Toon.Empty(root.transform, "TamMon", new Vector3(0f, 0.03f * pieceScale, 0f));
            return root;
        }

        static Vector3 Ring(float r, float deg, float y)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
        }

        static void Pieces(Transform root, List<IngredientModels.Part> parts, int n, float r, float y, float scale, System.Func<Color, Color> fix,
            System.Random rng, float offsetDeg = 0f)
        {
            if (parts.Count == 0 || n <= 0) return;
            for (int i = 0; i < n; i++)
            {
                float deg = offsetDeg + 360f * i / n + (float)rng.NextDouble() * 14f;
                Vector3 p = n == 1 ? new Vector3(0f, y, 0f) : Ring(r * (0.9f + 0.2f * (float)rng.NextDouble()), deg, y);
                GameObject piece = Toon.Empty(root, "Mieng", p, new Vector3(0f, -deg + 90f + (float)rng.NextDouble() * 40f, 0f));
                piece.transform.localScale = Vector3.one * scale;
                IngredientModels.BuildParts(piece.transform, parts, 1f, fix);
            }
        }
    }
}
