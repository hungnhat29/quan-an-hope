using System.Collections.Generic;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Yeu cau 24/11/2026 (constitution 2.2): may gacha do an ve lai kieu lo vi song (cua kinh, mam xoay, dia nam trong may, can gat
    /// ben canh, den nhap nhay khi quay, bang ket qua) va mo hinh do o tay thu hai (tay van nang).
    /// </summary>
    public partial class RoomView
    {
        // ---------------- may gacha do an ----------------

        const float MX = 1.6f, MZ = 3.2f;   // tam may theo khung Goc 2 (cho may cu cua canh)
        /// <summary>Day mesh SM_MayGacha_Long (thanh duoi khung cua kinh 0,77 - 0,04).</summary>
        const float InnerModelBottom = 0.73f;
        static readonly Color MachineBlue = UiKit.Hex("#9CC7D8"), MachineInside = UiKit.Hex("#FBE7B0"), MachineDark = UiKit.Hex("#5A7DA0");

        Transform machineRoot, machineHolder, leverPivot;
        GameObject machineDishModel, machineBannerRoot;
        TextMeshProUGUI machineBanner, machineCounter;
        Image machineBannerBg;
        Renderer[] machineBulbs;
        string machineDishKey;
        float leverT = -1f, bannerLeft, bodyShake;
        MachineResult? bannerShownFor;

        /// <summary>Goc may (hieu ung).</summary>
        public Transform MachineRoot { get { return machineRoot; } }

        /// <summary>Vi tri dia trong may (hieu ung ket qua).</summary>
        public Vector3 MachineDishPosition { get { return machineHolder != null ? machineHolder.position + Vector3.up * 0.12f : Vector3.zero; } }

        /// <summary>Dia dang hien trong may (test).</summary>
        public bool MachineDishShown { get { return machineDishModel != null && machineDishModel.activeInHierarchy; } }

        /// <summary>Chu bang ket qua tren may (null neu dang an).</summary>
        public string MachineBannerText { get { return machineBannerRoot != null && machineBannerRoot.activeSelf ? machineBanner.text : null; } }

        void BuildMachine2()
        {
            // may cu cua canh an han; may moi la machineGroup (bat khi da mua)
            Transform old = goc2.Find("UpgradeMachine");
            if (old != null) old.gameObject.SetActive(false);
            machineRoot = Toon.Empty(goc2, "MayGachaDoAn_LoViSong", Vector3.zero).transform;
            Transform t = machineRoot;
            Color cream = UiKit.Cream;
            var shell = new List<GameObject>();
            var inner = new List<GameObject>();
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "ChanDe", new Vector3(MX, 0.275f, MZ + 0.05f), new Vector3(1.3f, 0.55f, 0.8f), WoodDark, Toon.OutlineBig));
            // vo may: lung, noc, day, hong trai, cot dieu khien phai (de trong o cua kinh)
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "Lung", new Vector3(MX, 1.275f, MZ + 0.42f), new Vector3(1.5f, 1.45f, 0.1f), MachineBlue, Toon.OutlineBig));
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "Noc", new Vector3(MX, 1.89f, MZ), new Vector3(1.5f, 0.22f, 0.95f), MachineBlue, Toon.OutlineBig));
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "Day", new Vector3(MX, 0.66f, MZ), new Vector3(1.5f, 0.22f, 0.95f), MachineBlue, Toon.OutlineBig));
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "HongTrai", new Vector3(MX - 0.69f, 1.275f, MZ), new Vector3(0.12f, 1.45f, 0.95f), MachineBlue, Toon.OutlineBig));
            shell.Add(Toon.Prim(PrimitiveType.Cube, t, "CotDieuKhien", new Vector3(MX + 0.54f, 1.275f, MZ), new Vector3(0.42f, 1.45f, 0.95f), MachineBlue, Toon.OutlineBig));
            // long may sang am (nhin ro dia) + mam xoay
            inner.Add(Toon.Prim(PrimitiveType.Cube, t, "LongMay", new Vector3(MX - 0.18f, 1.28f, MZ + 0.35f), new Vector3(1.0f, 1.0f, 0.04f), MachineInside, 0f));
            inner.Add(Toon.Prim(PrimitiveType.Cube, t, "SanLong", new Vector3(MX - 0.18f, 0.78f, MZ), new Vector3(1.0f, 0.04f, 0.75f), MachineInside, 0f));
            inner.Add(Toon.Prim(PrimitiveType.Cylinder, t, "MamXoay", new Vector3(MX - 0.18f, 0.81f, MZ - 0.05f), new Vector3(0.62f, 0.03f, 0.62f), UiKit.Hex("#E3DCD2"), Toon.OutlineSmall));
            machineHolder = Toon.Empty(t, "ChoDia", new Vector3(MX - 0.18f, 0.83f, MZ - 0.05f)).transform;
            // khung cua kinh + tay nam + vet sang kinh
            float fz = MZ - 0.47f;
            inner.Add(Toon.Prim(PrimitiveType.Cube, t, "KhungTren", new Vector3(MX - 0.18f, 1.79f, fz), new Vector3(1.06f, 0.08f, 0.06f), cream, Toon.OutlineSmall));
            inner.Add(Toon.Prim(PrimitiveType.Cube, t, "KhungDuoi", new Vector3(MX - 0.18f, 0.77f, fz), new Vector3(1.06f, 0.08f, 0.06f), cream, Toon.OutlineSmall));
            inner.Add(Toon.Prim(PrimitiveType.Cube, t, "KhungTrai", new Vector3(MX - 0.67f, 1.28f, fz), new Vector3(0.08f, 1.06f, 0.06f), cream, Toon.OutlineSmall));
            inner.Add(Toon.Prim(PrimitiveType.Cube, t, "KhungPhai", new Vector3(MX + 0.31f, 1.28f, fz), new Vector3(0.08f, 1.06f, 0.06f), cream, Toon.OutlineSmall));
            // 24/11/2026 "trau chuot vien den": vo + long may dung lai bang Blender (bo canh, vien om hinh); thieu file thi giu khoi
            Transform modelHost = Toon.Empty(t, "MayModel", new Vector3(MX, 0f, MZ)).transform;
            ModelLibrary.Swap(modelHost, shell, "Machines", "SM_MayGacha_Vo", Toon.OutlineBig);
            // Model.build dat diem thap nhat cua mesh ve y = 0: long may bat dau o y = 0,73 m (thanh duoi khung cua) -> dat goc len lai
            Transform innerHost = Toon.Empty(t, "LongMayModel", new Vector3(MX, InnerModelBottom, MZ)).transform;
            ModelLibrary.Swap(innerHost, inner, "Machines", "SM_MayGacha_Long", Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Cube, t, "TayNam", new Vector3(MX + 0.25f, 1.28f, fz - 0.06f), new Vector3(0.05f, 0.5f, 0.05f), Steel, Toon.OutlineSmall);
            for (int i = 0; i < 2; i++)
                Toon.Prim(PrimitiveType.Cube, t, "VetKinh" + i, new Vector3(MX - 0.5f + 0.12f * i, 1.6f - 0.05f * i, fz + 0.01f), new Vector3(0.025f, 0.26f - 0.08f * i, 0.005f),
                    Color.white, 0f, new Vector3(0f, 0f, -35f));
            // bang dieu khien: man hinh luot, 3 nut
            Canvas lcd = UiKit.WorldCanvas(t, "ManHinh", new Vector3(MX + 0.54f, 1.66f, MZ - 0.48f), Quaternion.identity, new Vector2(300f, 140f));
            Image lb = UiKit.Panel(lcd.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 140f), MachineDark, false);
            lb.raycastTarget = false;
            machineCounter = UiKit.Text(lb.transform, "", 64f, TextAlignmentOptions.Center, 6f);
            machineCounter.color = UiKit.Cream;
            Color[] btn = { UiKit.Coral, UiKit.Mustard, UiKit.Mint };
            for (int i = 0; i < 3; i++)
                Toon.Prim(PrimitiveType.Sphere, t, "Nut" + i, new Vector3(MX + 0.54f, 1.38f - 0.17f * i, MZ - 0.49f), new Vector3(0.11f, 0.11f, 0.05f), btn[i], Toon.OutlineSmall);
            // den tren noc
            machineBulbs = new Renderer[3];
            for (int i = 0; i < 3; i++)
                machineBulbs[i] = Toon.Prim(PrimitiveType.Sphere, t, "Den" + i, new Vector3(MX - 0.4f + 0.4f * i, 2.07f, MZ - 0.1f), Vector3.one * 0.16f, btn[i], Toon.OutlineSmall).GetComponent<Renderer>();
            // bien ten
            Canvas sign = UiKit.WorldCanvas(t, "BienMay", new Vector3(MX, 1.89f, MZ - 0.48f), Quaternion.identity, new Vector2(900f, 150f));
            Image sb = UiKit.Panel(sign.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 150f), UiKit.Cream, false);
            sb.raycastTarget = false;
            UiKit.Text(sb.transform, "MÁY GACHA ĐỒ ĂN", 80f, TextAlignmentOptions.Center, 6f);
            // can gat ben phai
            leverPivot = Toon.Empty(t, "CanGat", new Vector3(MX + 0.82f, 1.3f, MZ)).transform;
            Toon.Prim(PrimitiveType.Cylinder, t, "TrucCan", new Vector3(MX + 0.8f, 1.3f, MZ), new Vector3(0.2f, 0.1f, 0.2f), Steel, Toon.OutlineSmall, new Vector3(0f, 0f, 90f));
            Toon.Prim(PrimitiveType.Cylinder, leverPivot, "ThanCan", new Vector3(0.04f, 0.3f, 0f), new Vector3(0.06f, 0.6f, 0.06f), SteelLight, Toon.OutlineSmall);
            Toon.Prim(PrimitiveType.Sphere, leverPivot, "NumCan", new Vector3(0.04f, 0.62f, 0f), Vector3.one * 0.18f, UiKit.Coral, Toon.OutlineSmall);
            GameObject leverHit = Toon.Empty(t, "GatCan", new Vector3(MX + 0.86f, 1.6f, MZ));
            Register(Interactable.Add(leverHit, "FoodMachineLever", Vector3.zero, new Vector3(0.3f, 0.8f, 0.4f), LeverPrompt, () => { if (g.PullMachineLever()) leverT = 0f; }));
            var col = Toon.Empty(t, "VaChamMay", new Vector3(MX, 0.275f, MZ + 0.05f)).AddComponent<BoxCollider>();
            col.size = new Vector3(1.5f, 2.2f, 0.95f);
            col.center = new Vector3(0f, 0.825f, 0f);
            // bang ket qua
            Canvas banner = UiKit.WorldCanvas(t, "KetQua", new Vector3(MX - 0.1f, 2.32f, MZ - 0.62f), Quaternion.identity, new Vector2(1000f, 200f));
            machineBannerBg = UiKit.Panel(banner.transform, "Nen", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 200f), UiKit.Mint);
            machineBannerBg.raycastTarget = false;
            machineBanner = UiKit.Text(machineBannerBg.transform, "", 96f, TextAlignmentOptions.Center, 8f);
            machineBannerRoot = banner.gameObject;
            machineBannerRoot.SetActive(false);

            // bien chu cu "May gacha do an · con x/3 luot" de len chan may moi: may moi co man hinh luot rieng
            if (machineSign != null) { machineSign.SetActive(false); machineSign = null; }
            machineGroup = machineRoot.gameObject;   // RefreshOwnedItems bat / tat theo HasMachine
            machineGroup.SetActive(g.Session.Items.HasMachine);
        }

        string LeverPrompt()
        {
            if (!g.Session.Items.HasMachine) return "Máy gacha đồ ăn · chưa mua";
            if (g.MachineSpinning) return "Đang quay…";
            if (g.MachineDish == null) return "Cần gạt · cho đĩa vào máy trước";
            if (g.MachineDish.machined) return "Đã quay · bấm máy lấy " + g.MachineDish.name + " ra";
            return "Gạt cần · quay " + g.MachineDish.name + " (còn " + g.Session.Items.MachineUsesLeft + " lượt)";
        }

        string MachineBodyPrompt()
        {
            ItemShop it = g.Session.Items;
            if (!it.IsOpen) return "Máy gacha đồ ăn · mở từ ngày " + g.Session.Balance.itemShopFromDay;
            if (!it.HasMachine) return "Mua máy gacha đồ ăn · " + g.Session.Balance.machine.price + "G";
            if (g.MachineSpinning) return "Đang quay…";
            if (g.MachineDish != null) return "Lấy " + g.MachineDish.name + " ra";
            if (g.Hand == HandKind.Dish) return g.HeldDish.machined ? g.HeldDish.name + " đã vào máy rồi" : "Cho " + g.HeldDish.name + " vào máy · còn " + it.MachineUsesLeft + " lượt";
            return "Máy gacha đồ ăn · cầm đĩa tới để cho vào · còn " + it.MachineUsesLeft + " lượt";
        }

        void RefreshMachine2()
        {
            if (machineRoot == null) return;
            ItemShop it = g.Session.Items;
            machineCounter.text = it.MachineUsesLeft + "/" + g.Session.Balance.machine.usesPerDay;
            Dish d = g.MachineDish;
            string key = d != null ? d.GetHashCode() + ":" + g.MachineShownTier : null;
            if (key != machineDishKey)
            {
                machineDishKey = key;
                if (machineDishModel != null) Object.Destroy(machineDishModel);
                machineDishModel = null;
                if (d != null)
                {
                    var shown = new Dish { recipeId = d.recipeId, name = d.name, baseName = d.baseName, tier = g.MachineShownTier };
                    machineDishModel = DishModel.Create(machineHolder, shown, g.Session.Recipes, false);
                    foreach (Collider c in machineDishModel.GetComponentsInChildren<Collider>()) Object.Destroy(c);
                    foreach (Interactable i in machineDishModel.GetComponentsInChildren<Interactable>()) Object.Destroy(i);
                    if (!g.MachineSpinning) Pop.Play(machineDishModel, 0.4f);
                }
            }
            if (g.MachineLastResult.HasValue && !g.MachineSpinning && bannerShownFor != g.MachineLastResult)
            {
                bannerShownFor = g.MachineLastResult;
                MachineResult r = g.MachineLastResult.Value;
                bool up = r == MachineResult.Up1 || r == MachineResult.Up2 || r == MachineResult.Up3;
                bool bad = r == MachineResult.Trash || r == MachineResult.Down1;
                machineBanner.text = up ? "THÀNH CÔNG! +" + (r == MachineResult.Up3 ? 3 : r == MachineResult.Up2 ? 2 : 1) + " bậc"
                    : r == MachineResult.Trash ? "XỊT! hỏng mất món" : bad ? "XỊT! −1 bậc" : "Giữ nguyên";
                machineBannerBg.color = up ? UiKit.Mint : bad ? UiKit.Coral : UiKit.Cream;
                machineBannerRoot.SetActive(true);
                Pop.Play(machineBannerRoot, 0.4f);
                bannerLeft = 5f;
            }
            if (!g.MachineLastResult.HasValue) bannerShownFor = null;
        }

        void TickMachine2(float dt)
        {
            if (machineRoot == null || !machineRoot.gameObject.activeInHierarchy) return;
            bool spin = g.MachineSpinning;
            if (machineHolder != null) machineHolder.localRotation = spin ? Quaternion.Euler(0f, machineHolder.localEulerAngles.y + 300f * dt, 0f) : machineHolder.localRotation;
            // than may rung nhe khi quay
            bodyShake = spin ? bodyShake + dt : 0f;
            machineRoot.localPosition = spin ? new Vector3(Mathf.Sin(bodyShake * 60f) * 0.006f, 0f, 0f) : Vector3.zero;
            for (int i = 0; i < machineBulbs.Length; i++)
            {
                bool on = !spin || ((int)(bodyShake * 8f) + i) % 3 == 0;
                machineBulbs[i].transform.localScale = Vector3.one * (on ? 0.16f : 0.1f);
            }
            if (leverT >= 0f)
            {
                leverT += dt;
                float k = leverT < 0.25f ? leverT / 0.25f : Mathf.Max(0f, 1f - (leverT - 0.25f) / 0.5f);
                leverPivot.localRotation = Quaternion.Euler(-70f * k, 0f, 0f);
                if (leverT > 0.75f) { leverT = -1f; leverPivot.localRotation = Quaternion.identity; }
            }
            if (bannerLeft > 0f)
            {
                bannerLeft -= dt;
                if (bannerLeft <= 0f) machineBannerRoot.SetActive(false);
            }
        }

        // ---------------- tay thu hai (tay van nang) ----------------

        Transform secondAnchor;
        GameObject secondModel;
        string secondKey;

        /// <summary>Mo hinh do o o khong dung (tay trai) - test.</summary>
        public GameObject SecondHandModel { get { return secondModel; } }

        void RefreshSecondHand()
        {
            if (secondAnchor == null) secondAnchor = Toon.Empty(g.Player.Cam.transform, "TayTrai", new Vector3(-0.34f, -0.3f, 0.6f)).transform;
            GameController.HandSlot s = g.HasGlove ? g.Slot(1 - g.ActiveSlot) : null;
            string key = s == null || s.kind == HandKind.Empty ? null
                : s.kind + ":" + (s.ingredient != null ? s.ingredient.GetHashCode() + (s.ingredient.prepared ? "p" : "") : "") + (s.dish != null ? s.dish.GetHashCode() + ":" + s.dish.tier : "");
            if (key == secondKey) return;
            secondKey = key;
            if (secondModel != null) Object.Destroy(secondModel);
            secondModel = null;
            if (key == null) return;
            switch (s.kind)
            {
                case HandKind.Ingredient:
                    ItemView v = ItemView.Create(secondAnchor, s.ingredient, 2.2f, false);
                    v.SetPrepared(s.ingredient.prepared);
                    secondModel = v.gameObject;
                    break;
                case HandKind.Plate: secondModel = DishModel.EmptyPlate(secondAnchor); break;
                case HandKind.Dish: secondModel = DishModel.Create(secondAnchor, s.dish, g.Session.Recipes, false); break;
                case HandKind.Sauce: secondModel = SauceHandModel(secondAnchor); break;
            }
            if (secondModel != null)
            {
                secondModel.transform.localScale *= 0.85f;
                foreach (Collider c in secondModel.GetComponentsInChildren<Collider>()) Object.Destroy(c);
                foreach (Interactable i in secondModel.GetComponentsInChildren<Interactable>()) Object.Destroy(i);
            }
        }

        // =====================================================================

        void BuildSprint12()
        {
            BuildMachine2();
            Interactable body = Station("FoodMachine");
            if (body != null)
            {
                // than may moi: hop bam bao ca cua kinh (bam = cho dia vao / lay ra)
                body.Box.center = Vector3.zero;
                body.Box.size = new Vector3(1.4f, 1.5f, 0.6f);
                body.transform.localPosition = new Vector3(MX - 0.08f, 1.28f, MZ - 0.25f);
                body.Prompt = MachineBodyPrompt;
            }
        }

        void RefreshSprint12()
        {
            RefreshMachine2();
            RefreshSecondHand();
        }

        void TickSprint12(float dt)
        {
            TickMachine2(dt);
        }
    }
}
