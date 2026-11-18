using System.Text;
using QuanAnHope.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace QuanAnHope.Game
{
    /// <summary>
    /// HUD chi hien con so va thong tin ngan (constitution Dieu 9): ngay, vi, no, ve, tam ngam, goi y cua do dang nhin,
    /// thu dang cam, thong bao mot dong. HUD khong co nut. Man ket thuc ngay / qua vong / thua la man ngoai phong nen co nut.
    /// </summary>
    public class Hud
    {
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        static readonly Vector2 Top = new Vector2(0.5f, 1f);
        static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        static readonly Vector2 TopRight = new Vector2(1f, 1f);
        static readonly Vector2 Bottom = new Vector2(0.5f, 0f);
        static readonly Vector2 BottomLeft = new Vector2(0f, 0f);

        const string ControlsHint = "W A S D: đi · Chuột: nhìn · Chuột trái: bấm / giữ · Chuột phải: đặt lại đồ đang cầm · Esc: nhả chuột";

        readonly GameController g;
        public Transform Canvas { get; private set; }

        TextMeshProUGUI day, money, tickets, toast, prompt, held, hint, clickToPlay;
        Image crosshair;
        GameObject toastBox, promptBox, heldBox;

        GameObject overlay;
        TextMeshProUGUI overlayTitle, overlayBody;
        Button overlayButton;
        bool overlayShown;

        public Hud(GameController controller)
        {
            g = controller;
            var go = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            Canvas = go.transform;
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            Build();
        }

        void Build()
        {
            // So lieu: goc tren trai va tren phai.
            Image left = UiKit.Panel(Canvas, "HUD_Trai", TopLeft, new Vector2(24f, -14f), new Vector2(400f, 72f), UiKit.Cream);
            left.raycastTarget = false;
            // "Ngay 1 · 13:20" (F-011 AC-1): rong hon de co gio, o tien hep lai.
            day = UiKit.Label(left.transform, "Ngay", TopLeft, new Vector2(12f, -4f), new Vector2(160f, 64f), "", 24f, TextAlignmentOptions.Left);
            money = UiKit.Label(left.transform, "Tien", TopRight, new Vector2(-12f, -4f), new Vector2(222f, 64f), "", 16f, TextAlignmentOptions.Right);
            money.lineSpacing = -38f;

            Image right = UiKit.Panel(Canvas, "HUD_Phai", TopRight, new Vector2(-24f, -14f), new Vector2(150f, 60f), UiKit.Mustard);
            right.raycastTarget = false;
            tickets = UiKit.Text(right.transform, "", UiKit.NumberFontSize);

            // Thong bao mot dong
            // Hep lai de khong che o tien ben trai (HUD_Trai rong 400 px tu mep 24 px).
            Image t = UiKit.Panel(Canvas, "Toast", Top, new Vector2(0f, -14f), new Vector2(410f, 52f), UiKit.Cream, false);
            t.raycastTarget = false;
            toastBox = t.gameObject;
            toast = UiKit.Text(t.transform, "", 16f);

            // Tam ngam + goi y cua do dang nhin
            var ch = UiKit.Box(Canvas, "TamNgam", Center, Vector2.zero, new Vector2(14f, 14f)).gameObject.AddComponent<Image>();
            ch.sprite = UiKit.Rounded;
            ch.type = Image.Type.Sliced;
            ch.color = UiKit.Cream;
            ch.raycastTarget = false;
            crosshair = ch;

            Image p = UiKit.Panel(Canvas, "GoiY", Center, new Vector2(0f, -46f), new Vector2(420f, 40f), UiKit.Cream, false);
            p.raycastTarget = false;
            promptBox = p.gameObject;
            prompt = UiKit.Text(p.transform, "", 18f);

            // Dang cam
            Image h = UiKit.Panel(Canvas, "DangCam", Bottom, new Vector2(0f, 20f), new Vector2(360f, 40f), UiKit.Mint, false);
            h.raycastTarget = false;
            heldBox = h.gameObject;
            held = UiKit.Text(h.transform, "", 17f);

            hint = UiKit.Label(Canvas, "DieuKhien", BottomLeft, new Vector2(16f, 8f), new Vector2(760f, 26f), ControlsHint, 14f, TextAlignmentOptions.Left);
            hint.rectTransform.pivot = new Vector2(0f, 0f);
            hint.color = UiKit.Cream;

            clickToPlay = UiKit.Label(Canvas, "BamDeChoi", Center, new Vector2(0f, 60f), new Vector2(500f, 40f), "Bấm chuột vào màn hình để điều khiển", 20f);
            clickToPlay.color = UiKit.Cream;

            BuildOverlay();
        }

        void BuildOverlay()
        {
            RectTransform dim = UiKit.Stretch(Canvas, "ManPhu");
            var img = dim.gameObject.AddComponent<Image>();
            img.color = new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.72f);
            overlay = dim.gameObject;

            Image card = UiKit.Panel(dim, "The", Center, Vector2.zero, new Vector2(600f, 360f), UiKit.Cream);
            overlayTitle = UiKit.Label(card.transform, "TieuDe", Top, new Vector2(0f, -22f), new Vector2(560f, 56f), "", 36f);
            overlayBody = UiKit.Label(card.transform, "NoiDung", Center, new Vector2(0f, 6f), new Vector2(560f, 176f), "", UiKit.NumberFontSize);
            overlayBody.lineSpacing = -22f;
            overlayButton = UiKit.Button(card.transform, "Btn_ManPhu", Bottom, new Vector2(0f, 24f), new Vector2(280f, 64f), "", UiKit.Coral, OnOverlayButton, 24f);
            overlay.SetActive(false);
        }

        void OnOverlayButton()
        {
            if (g.Session.Phase == GamePhase.DayEnd) g.NextDay();
            else g.Restart();
        }

        public Button OverlayButton { get { return overlayButton; } }

        public void Refresh()
        {
            GameSession s = g.Session;
            DebtEntry debt = s.NextDebt();
            RefreshClock();
            money.text = "Ví <size=26>" + s.Money + "G</size>\n"
                + (debt != null ? "Nợ <size=26>" + debt.amount + "G</size> đầu ngày " + debt.day : "Hết nợ");
            bool black = s.Items.BlackTickets > 0;
            tickets.text = s.Wallet.Count + " vé" + (black ? " · " + s.Items.BlackTickets + " vé đen" : "");
            var ticketBox = (RectTransform)tickets.transform.parent;
            ticketBox.sizeDelta = new Vector2(black ? 310f : 150f, ticketBox.sizeDelta.y);

            bool hasToast = !string.IsNullOrEmpty(g.Toast);
            toastBox.SetActive(hasToast);
            if (hasToast) toast.text = g.Toast;

            string name = g.HeldName;
            heldBox.SetActive(name != null);
            if (name != null) held.text = "Đang cầm: " + name;

            RefreshOverlay();
        }

        string shownClock;

        /// <summary>Ngay va gio; tu 20:00 (mot gio truoc dong cua) gio doi mau cam (kich ban F-011).</summary>
        void RefreshClock()
        {
            GameSession s = g.Session;
            string text = s.Clock.Text;
            bool late = s.Clock.Hour >= s.Balance.day.closeHour - 1;
            string shown = s.Day + text + late;
            if (shown == shownClock) return;
            shownClock = shown;
            day.text = "Ngày " + s.Day + " · " + (late ? "<color=#" + ColorUtility.ToHtmlStringRGB(UiKit.Coral) + ">" + text + "</color>" : text);
        }

        /// <summary>Moi khung hinh: dong ho, goi y cua do dang nhin, con tro chuot.</summary>
        public void Tick()
        {
            RefreshClock();
            Interactable focus = g.Player.Focus;
            bool show = focus != null && !overlayShown;
            promptBox.SetActive(show);
            if (show) prompt.text = "Chuột trái · " + focus.CurrentPrompt;
            crosshair.color = focus != null ? UiKit.Mustard : UiKit.Cream;

            bool locked = g.Player.CursorLocked || Application.isBatchMode || g.Player.SimulateInput;
            clickToPlay.gameObject.SetActive(!locked && !overlayShown);
        }

        void RefreshOverlay()
        {
            GameSession s = g.Session;
            bool show = g.ShowsOverlay;
            if (show != overlayShown) PlayerController.SetCursorLocked(!show);
            overlayShown = show;
            overlay.SetActive(show);
            if (!show) return;
            overlay.transform.SetAsLastSibling();

            if (s.Phase == GamePhase.DayEnd)
            {
                DebtEntry debt = s.NextDebt();
                overlayTitle.text = "Hết ngày " + s.Day;
                var sb = new StringBuilder();
                sb.Append("Đã phục vụ ").Append(s.ServedToday).Append(" khách · đóng quán lúc ").Append(s.ClosedAt).Append('\n');
                sb.Append("Thu trong ngày: ").Append(s.IncomeToday >= 0 ? "+" : "").Append(s.IncomeToday).Append("G\n");
                sb.Append("Tiền hiện có: ").Append(s.Money).Append("G\n");
                if (s.Fridge.Dishes.Count > 0) sb.Append("Tủ lạnh giữ ").Append(s.Fridge.Dishes.Count).Append("/").Append(s.Fridge.Capacity).Append(" món\n");
                if (debt != null)
                {
                    if (debt.day == s.Day + 1) sb.Append("<b>Sáng mai chủ nợ thu ").Append(debt.amount).Append("G</b>");
                    else sb.Append("Nợ sắp tới: ").Append(debt.amount).Append("G vào đầu ngày ").Append(debt.day);
                }
                overlayBody.text = sb.ToString();
                UiKit.SetLabel(overlayButton, "Sang ngày " + (s.Day + 1));
            }
            else if (s.IsWon)
            {
                // F-044 AC-2: man thang bang chu (tranh lam o sprint 6)
                overlayTitle.text = "Hope trả hết nợ!";
                overlayBody.text = "Đầu ngày " + s.Day + ", chủ nợ thu khoản cuối " + s.LastDebtPaid + "G.\nHope còn " + s.Money
                    + "G và quán ăn đã đứng vững.\n\nChủ nợ bật cười lần đầu tiên, rồi gả con gái cho Hope.";
                UiKit.SetLabel(overlayButton, "Chơi lại");
            }
            else if (s.Phase == GamePhase.Cleared)
            {
                overlayTitle.text = "Qua vòng " + s.Balance.RoundOfDay(s.LastPlayableDay) + "!";
                overlayBody.text = "Đầu ngày " + s.Day + ", chủ nợ đã thu " + s.LastDebtPaid + "G.\nBạn còn " + s.Money
                    + "G.\n\nBản chơi thử này dừng ở đây.";
                UiKit.SetLabel(overlayButton, "Chơi lại");
            }
            else
            {
                int due = s.Balance.DebtDueOn(s.Day);
                overlayTitle.text = "Thua rồi";
                overlayBody.text = "Đầu ngày " + s.Day + ", chủ nợ đòi " + due + "G\nnhưng bạn chỉ có " + s.Money + "G."
                    + (s.Day == s.Balance.day.bossDay + 1 ? "\n\nCần phục vụ chủ nợ món Tuyệt hảo ngày " + s.Balance.day.bossDay + " (thưởng " + s.Balance.bossBonus + "G)." : "");
                UiKit.SetLabel(overlayButton, "Chơi lại");
            }
        }
    }
}
