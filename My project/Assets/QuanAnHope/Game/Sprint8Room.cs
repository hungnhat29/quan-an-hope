using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Sprint 8 trong phong (specs/sprint-8/spec.md): den tran + cong tac (F-076, sua B-01), nut doi hang relic (F-077),
    /// do da mua moi hien o Goc 2 (F-078 AC-4), dao va dong tac so che tren thot (F-075).
    /// </summary>
    public partial class RoomView
    {
        // =====================================================================
        // F-076: den va cong tac. Mau theo gio di vao den chinh (Art Bible muc 10), khong phu anh len man hinh (B-01).
        // =====================================================================

        public const float DuskFrom = 17f, NightAt = 19f;
        /// <summary>Den chinh con lai khi troi toi ma den tat (spec F-076 AC-2: ~40%).</summary>
        public const float DarkLevel = 0.4f;
        static readonly Color Afternoon = UiKit.Hex("#FFD9B0"), LateNight = UiKit.Hex("#C9B3F0");
        static readonly Color NightBlue = UiKit.Hex("#9EA4D6"), LampWarm = UiKit.Hex("#FFE6C4");
        static readonly Color BulbOff = UiKit.Hex("#D9CFC0"), BulbOn = UiKit.Hex("#FFF3DC");

        readonly List<GameObject> bulbs = new List<GameObject>();
        readonly List<GameObject> lampGlows = new List<GameObject>();
        GameObject lightSwitch;
        Transform switchLever;
        Color mainColor = Color.white;
        float shownLevel = 1f;
        Color shownColor = Color.white;
        bool lampsShown;

        /// <summary>Muc sang cua den chinh so voi ban ngay (test F-076).</summary>
        public float RoomLightLevel { get { return shownLevel; } }
        public Color RoomLightColor { get { return shownColor; } }
        public bool LampsLit { get { return lampsShown; } }
        public GameObject LightSwitch { get { return lightSwitch; } }

        /// <summary>Do toi cua troi 0..1 theo gio (17:00 -> 19:00).</summary>
        public static float NightFactor(float hour)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(DuskFrom, NightAt, hour));
        }

        /// <summary>Muc sang va mau den chinh (nhan vao mau vat, Art Bible muc 4 + 10) o mot gio, den bat / tat.</summary>
        public static void LightAt(float hour, bool lampsOn, bool blackout, float blackoutDim, out float level, out Color tint)
        {
            float night = NightFactor(hour);
            // Art Bible muc 4: 15:00 -> 21:00 chieu am #FFD9B0 tang toi 20%; sau 21:00 #C9B3F0 20%. NHAN vao den, khong pha trang.
            tint = Color.Lerp(Color.white, Afternoon, 0.2f * Mathf.InverseLerp(15f, 21f, hour));
            if (hour >= 21f) tint = Color.Lerp(tint, LateNight, 0.2f * Mathf.InverseLerp(21f, 21.25f, hour));
            if (blackout)
            {
                level = 1f - blackoutDim;
                return;
            }
            if (lampsOn)
            {
                level = Mathf.Lerp(1f, 0.95f, night);
                tint = Color.Lerp(tint, LampWarm, 0.55f * night);
            }
            else
            {
                level = Mathf.Lerp(1f, DarkLevel, night);
                tint = Color.Lerp(tint, NightBlue, 0.5f * night);
            }
        }

        void BuildLights()
        {
            if (mainLight != null) mainColor = mainLight.color;
            // Bon den treo tran tren loi di (Art Bible muc 8: khong dat do dung tren san vao loi di; den treo tren cao)
            Vector3[] at = { new Vector3(-1.7f, 0f, -1.3f), new Vector3(1.7f, 0f, -1.3f), new Vector3(-1.7f, 0f, 1.3f), new Vector3(1.7f, 0f, 1.3f) };
            Color shade = UiKit.Hex("#D69B68");
            for (int i = 0; i < at.Length; i++)
            {
                GameObject lamp = Toon.Empty(root, "DenTran_" + i, at[i]);
                Toon.Prim(PrimitiveType.Cylinder, lamp.transform, "Day", new Vector3(0f, 4.55f, 0f), new Vector3(0.025f, 0.9f, 0.025f), UiKit.Ink, 0f);
                Toon.Prim(PrimitiveType.Sphere, lamp.transform, "Chao", new Vector3(0f, 4.05f, 0f), new Vector3(0.6f, 0.34f, 0.6f), shade, Toon.OutlineBig);
                Toon.Prim(PrimitiveType.Cylinder, lamp.transform, "Vanh", new Vector3(0f, 3.92f, 0f), new Vector3(0.58f, 0.03f, 0.58f), UiKit.Hex("#B97F52"), Toon.OutlineSmall);
                bulbs.Add(Toon.Prim(PrimitiveType.Sphere, lamp.transform, "BongDen", new Vector3(0f, 3.86f, 0f), new Vector3(0.18f, 0.18f, 0.18f), BulbOff, Toon.OutlineSmall));
                lampGlows.Add(Glow(lamp.transform, new Vector3(0f, 3.8f, 0f), 1.1f));
            }
            // bong den treo co san o Goc 1 cung bat / tat theo cong tac
            Transform decor = goc1.Find("Decor");
            for (int i = 0; decor != null && i < 2; i++)
            {
                Transform b = decor.Find("LampBulb" + i);
                if (b == null) continue;
                bulbs.Add(b.gameObject);
                lampGlows.Add(Glow(b, Vector3.zero, 3.5f));
            }

            // Cong tac tren tuong Goc 1, ben trai cua (giua cua va chau cay lon), cao 1,45 m: trong tam voi tu loi di.
            lightSwitch = Toon.Empty(goc1, "CongTacDen", new Vector3(-2.45f, 1.45f, 3.95f));
            Toon.Prim(PrimitiveType.Cube, lightSwitch.transform, "De", Vector3.zero, new Vector3(0.2f, 0.28f, 0.04f), UiKit.Cream, Toon.OutlineSmall);
            switchLever = Toon.Prim(PrimitiveType.Cube, lightSwitch.transform, "Cần", new Vector3(0f, 0f, -0.03f), new Vector3(0.06f, 0.12f, 0.05f), UiKit.Coral, Toon.OutlineSmall).transform;
            Canvas tag = UiKit.WorldCanvas(lightSwitch.transform, "NhanCongTac", new Vector3(0f, 0.26f, -0.03f), Quaternion.identity, new Vector2(300f, 90f));
            Image tb = UiKit.Panel(tag.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 90f), UiKit.Cream, false);
            tb.raycastTarget = false;
            UiKit.Text(tb.transform, "Đèn", 56f);
            Register(Interactable.Add(lightSwitch, "LightSwitch", Vector3.zero, new Vector3(0.5f, 0.6f, 0.3f), LightSwitchPrompt, () => g.ToggleLights()));
            ApplyLampVisual(false);
        }

        static GameObject Glow(Transform parent, Vector3 pos, float size)
        {
            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "QuangDen";
            Object.Destroy(glow.GetComponent<Collider>());
            glow.transform.SetParent(parent, false);
            glow.transform.localPosition = pos;
            glow.transform.localScale = Vector3.one * size;
            glow.GetComponent<MeshRenderer>().sharedMaterial = CandleGlow.Material;
            glow.AddComponent<Billboard>();
            glow.SetActive(false);
            return glow;
        }

        string LightSwitchPrompt()
        {
            GameSession s = g.Session;
            if (s.IsBlackout) return "Công tắc đèn · mất điện, đèn không sáng";
            return g.LightsOn ? "Tắt đèn" : "Bật đèn" + (NightFactor(s.Clock.HourOfDay) > 0.05f ? " · trời đang tối" : "");
        }

        void ApplyLampVisual(bool on)
        {
            lampsShown = on;
            foreach (GameObject b in bulbs) if (b != null) Toon.Recolor(b, on ? BulbOn : BulbOff);
            foreach (GameObject gl in lampGlows) if (gl != null) gl.SetActive(on);
            if (switchLever != null) switchLever.localPosition = new Vector3(0f, on ? 0.035f : -0.035f, -0.03f);
        }

        /// <summary>Moi khung hinh: muc sang den chinh theo gio + den (doi muot ~0,25 giay khi bat / tat).</summary>
        void TickLights(float dt)
        {
            GameSession s = g.Session;
            if (s == null) return;
            bool running = s.Phase == GamePhase.Prep || s.Phase == GamePhase.Open;
            bool blackout = s.IsBlackout && running;
            bool lit = g.LightsOn && !blackout;
            if (lit != lampsShown) ApplyLampVisual(lit);
            float level;
            Color tint;
            LightAt(s.Clock.HourOfDay, lit, blackout, s.Balance.blackout.dim, out level, out tint);
            float k = dt <= 0f ? 1f : 1f - Mathf.Exp(-dt * 12f);
            shownLevel = Mathf.Lerp(shownLevel, level, k);
            shownColor = Color.Lerp(shownColor, tint, k);
            if (mainLight == null || mainIntensity < 0f) return;
            mainLight.intensity = mainIntensity * shownLevel;
            mainLight.color = mainColor * shownColor;
        }

        /// <summary>Test: dat ngay muc sang dich (khong doi muot).</summary>
        public void SnapLights()
        {
            shownLevel = -1f;
            TickLights(0f);
        }

        // =====================================================================
        // F-077: nut doi hang o Goc 3, canh be relic thu ba
        // =====================================================================

        TextMeshProUGUI rerollText;
        public string RerollSignText { get { return rerollText != null ? rerollText.text : ""; } }

        void BuildRerollButton()
        {
            GameObject stand = Toon.Empty(goc3, "NutDoiHang", new Vector3(2.45f, 0f, 3.2f));
            Toon.Prim(PrimitiveType.Cylinder, stand.transform, "Cot", new Vector3(0f, 0.55f, 0f), new Vector3(0.16f, 1.1f, 0.16f), UiKit.Hex("#B97F52"), Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cylinder, stand.transform, "De", new Vector3(0f, 1.13f, 0f), new Vector3(0.42f, 0.06f, 0.42f), UiKit.Cream, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Sphere, stand.transform, "Nut", new Vector3(0f, 1.2f, 0f), new Vector3(0.28f, 0.16f, 0.28f), UiKit.Coral, Toon.OutlineSmall);
            Toon.BlobShadow(stand.transform, "Bong", new Vector3(0f, 0.01f, 0f), 0.5f);
            Canvas c = UiKit.WorldCanvas(stand.transform, "BienDoiHang", new Vector3(0f, 0.85f, -0.12f), Quaternion.identity, new Vector2(520f, 220f));
            Image bg = UiKit.Panel(c.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 220f), UiKit.Lilac, false);
            bg.raycastTarget = false;
            rerollText = UiKit.Text(bg.transform, "", 60f);
            rerollText.lineSpacing = -20f;
            Register(Interactable.Add(stand, "RelicReroll", new Vector3(0f, 1.05f, 0f), new Vector3(0.6f, 0.5f, 0.6f), RerollPrompt, () => g.RerollRelics()));
        }

        string RerollPrompt()
        {
            GameSession s = g.Session;
            if (!s.Shop.OpensOn(s.Day)) return "Đổi hàng relic · mở từ ngày " + s.Balance.start.relicShopFromDay;

            return "Đổi 3 relic khác · " + s.Shop.RerollCost + "G (ví " + s.Money + "G)";
        }

        void RefreshReroll()
        {
            if (rerollText == null) return;
            GameSession s = g.Session;
            rerollText.text = !s.Shop.OpensOn(s.Day) ? "Đổi hàng\n<size=44>mở từ ngày " + s.Balance.start.relicShopFromDay + "</size>"
                : "Đổi hàng\n<size=50>" + s.Shop.RerollCost + "G</size>";
        }

        // =====================================================================
        // F-078 AC-4: bo quay vat pham co san; do da mua moi hien o Goc 2
        // =====================================================================

        GameObject itemTable, machineGroup, machineSign, machineStation, itemBoard, sauceTagCanvas, blackTagCanvas;

        void HideItemCounter()
        {
            itemTable = goc2.Find("QuayVatPham") != null ? goc2.Find("QuayVatPham").gameObject : null;
            Transform up = goc2.Find("UpgradeMachine");
            machineGroup = up != null ? up.gameObject : null;
            machineSign = machineSignText != null ? machineSignText.GetComponentInParent<Canvas>(true).gameObject : null;
            Transform hit = goc2.Find("MayGachaDoAn");
            machineStation = hit != null ? hit.gameObject : null;
            // luu truoc: GetComponentInParent bo qua doi tuong da an
            sauceTagCanvas = sauceTag != null ? sauceTag.GetComponentInParent<Canvas>(true).gameObject : null;
            blackTagCanvas = blackTag != null ? blackTag.GetComponentInParent<Canvas>(true).gameObject : null;
            Transform board = goc2.Find("BangVatPham");
            itemBoard = board != null ? board.gameObject : null;
            if (itemBoard != null) itemBoard.SetActive(false);   // gia + ti le nay o bang Tab (F-078 AC-3)
        }

        /// <summary>Hien do da mua: may gacha do an sau khi mua; lo nuoc sot khi con sot; xap ve den + khe ve den khi con ve den.</summary>
        void RefreshOwnedItems()
        {
            ItemShop it = g.Session.Items;
            bool machine = it.HasMachine;
            if (machineGroup != null && machineGroup.activeSelf != machine) machineGroup.SetActive(machine);
            if (machineSign != null) machineSign.SetActive(machine);
            if (machineStation != null) machineStation.SetActive(machine);
            // sau sprint 10: lo sot o ben phai bep (Sprint11Room), an khi dang cam tren tay; quay Goc 2 chi con ve den
            bool sauce = it.Sauces > 0 && g.Hand != HandKind.Sauce, black = it.BlackTickets > 0;
            if (itemTable != null) itemTable.SetActive(black);
            if (sauceBottle != null) sauceBottle.SetActive(sauce);
            if (blackStack != null) blackStack.SetActive(black);
            if (sauceTagCanvas != null) sauceTagCanvas.SetActive(sauce);
            if (blackTagCanvas != null) blackTagCanvas.SetActive(black);
            if (blackSlot != null) blackSlot.SetActive(black);
        }

        /// <summary>Do da mua dang hien trong phong (test F-078 AC-4).</summary>
        public bool MachineShown { get { return machineGroup != null && machineGroup.activeInHierarchy; } }
        public bool SauceShown { get { return sauceBottle != null && sauceBottle.activeInHierarchy; } }
        public bool BlackSlotShown { get { return blackSlot != null && blackSlot.activeInHierarchy; } }
        public bool ItemTableShown { get { return itemTable != null && itemTable.activeInHierarchy; } }

        // =====================================================================
        // F-075: dao va dong tac so che
        // =====================================================================

        PrepFx prepFx;
        public PrepFx Prep { get { return prepFx; } }

        void BuildPrepFx()
        {
            prepFx = PrepFx.Create(boardSpot);
        }

        void TickPrep(float dt)
        {
            // sprint 10: moi thot mot bo dao (Sprint10Room.TickBoards)
        }

        // =====================================================================

        void BuildSprint8()
        {
            BuildLights();
            BuildRerollButton();
            HideItemCounter();
            BuildPrepFx();
        }

        void RefreshSprint8()
        {
            RefreshReroll();
            RefreshOwnedItems();
        }

        void TickSprint8(float dt)
        {
            TickLights(dt);
            TickPrep(dt);
            TickWalkers(dt);
        }
    }

    /// <summary>Quad luon quay ve nguoi choi (quang den).</summary>
    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            if (FaceBillboard.View != null) transform.rotation = FaceBillboard.View.transform.rotation;
        }
    }
}
