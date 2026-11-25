using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Nguoi choi goc nhin thu nhat (constitution Dieu 5, Art Bible muc 9): W A S D / mui ten de di,
    /// chuot de nhin, chuot trai de bam / giu vao do vat o tam ngam, chuot phai de dat lai do dang cam,
    /// Esc de nha chuot. Test dieu khien qua SimulateInput / LookAt / TapPrimary.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        // Art Bible muc 9 (quy cach dieu khien, khong phai so can bang).
        public float MoveSpeed = 2.6f;

        /// <summary>Giu Shift: chay nhanh gap bay nhieu lan (yeu cau 22/11/2026, quy cach dieu khien, khong phai so can bang).</summary>
        public float RunMultiplier = 1.7f;

        /// <summary>Space: nhay (van toc dau 4,2 m/giay, do cao do that ~0,4 m vi CharacterController va buoc chia nho).</summary>
        public float JumpSpeed = 4.2f;
        public float LookDegreesPerPixel = 0.1f;
        public float Reach = 3f;
        public float EyeHeight = 1.75f;
        const float PitchLimit = 80f;
        const float Gravity = 9.8f;

        public Camera Cam { get; private set; }
        public Interactable Focus { get; private set; }

        /// <summary>Tat khi dang co man ket thuc ngay / qua vong / thua.</summary>
        public bool InputEnabled = true;

        /// <summary>Bat de test tu dieu khien; khi bat, ban phim va chuot bi bo qua.</summary>
        public bool SimulateInput;
        public Vector2 SimulatedMove;
        public bool SimulatedHold;
        public bool SimulatedRun;

        /// <summary>Dang giu Shift (chay).</summary>
        public bool Running { get; private set; }

        /// <summary>Dang o tren khong (vua nhay).</summary>
        public bool Airborne { get { return body != null && !body.isGrounded; } }
        bool jumpQueued;
        float groundedAgo;

        /// <summary>Bam chuot phai.</summary>
        public event Action SecondaryPressed;

        /// <summary>Bam Esc (F-050 AC-2: mo menu tam dung). Khong ai nghe thi Esc nha chuot nhu cu.</summary>
        public event Action EscapePressed;

        /// <summary>Phim Tab: bang relic + cua hang (sprint 8 F-078).</summary>
        public event Action TabPressed;

        /// <summary>Dao truc doc cua chuot (F-054 AC-2).</summary>
        public bool InvertY;

        /// <summary>Camera dat o vi tri co dinh (menu chinh nhin tu cua, F-050); nguoi choi khong di, khong nhin.</summary>
        public bool CameraOverride { get; private set; }

        CharacterController body;
        float yaw, pitch, fallSpeed;
        Interactable holdTarget;
        bool simulatedHoldStarted;

        public float Yaw { get { return yaw; } }
        public float Pitch { get { return pitch; } }

        public static PlayerController Attach(GameObject player)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc == null) cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.2f;
            cc.skinWidth = 0.02f;
            var p = player.GetComponent<PlayerController>();
            if (p == null) p = player.AddComponent<PlayerController>();
            return p;
        }

        void Awake()
        {
            body = GetComponent<CharacterController>();
            Cam = GetComponentInChildren<Camera>();
            if (Cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                go.transform.SetParent(transform, false);
                Cam = go.AddComponent<Camera>();
            }
            Cam.transform.localPosition = new Vector3(0f, EyeHeight, 0f);
            yaw = transform.eulerAngles.y;
            pitch = NormalizeAngle(Cam.transform.localEulerAngles.x);
            transform.rotation = Quaternion.identity;
            ApplyLook();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector2 move = Vector2.zero;
            bool pressed = false, held = false, secondary = false;

            if (SimulateInput)
            {
                move = SimulatedMove;
                held = SimulatedHold;
                Running = SimulatedRun;
                pressed = SimulatedHold && !simulatedHoldStarted;
                simulatedHoldStarted = SimulatedHold;
            }
            else if (InputEnabled && !CameraOverride)
            {
                ReadDevices(ref move, ref pressed, ref held, ref secondary);
            }
            else ReadEscapeOnly();

            if (CameraOverride)
            {
                Focus = null;
                return;
            }
            Move(move, dt);
            RefreshFocus();

            if (pressed)
            {
                holdTarget = Focus;
                if (Focus != null && Focus.Press != null) Focus.Press();
            }
            if (held && holdTarget != null && holdTarget == Focus && holdTarget.Hold != null) holdTarget.Hold(dt);
            if (!held) holdTarget = null;
            if (secondary && SecondaryPressed != null) SecondaryPressed();

            if (shakeLeft > 0f)
            {
                shakeLeft -= dt;
                Vector3 jitter = shakeLeft > 0f ? (Vector3)(UnityEngine.Random.insideUnitCircle * 0.025f) : Vector3.zero;
                Cam.transform.localPosition = new Vector3(0f, EyeHeight, 0f) + jitter;
            }
        }

        float shakeLeft;

        /// <summary>Rung camera ngan (Art Bible muc 11: chay / Lac vi 0,15 giay).</summary>
        public void Shake(float seconds)
        {
            shakeLeft = Mathf.Max(shakeLeft, seconds);
        }

        public bool Shaking { get { return shakeLeft > 0f; } }

        void ReadDevices(ref Vector2 move, ref bool pressed, ref bool held, ref bool secondary)
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
                if (kb.escapeKey.wasPressedThisFrame) OnEscape();
                if (kb.tabKey.wasPressedThisFrame && TabPressed != null) TabPressed();
                // sau sprint 10 (yeu cau 23/11/2026): T bat / tat tranh huong dan toan man hinh
                if (kb.tKey.wasPressedThisFrame && TutorialKeyPressed != null) TutorialKeyPressed();
                // 2.2 tay van nang: 1 / 2 chon o cam do
                if (SlotKeyPressed != null)
                {
                    if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) SlotKeyPressed(0);
                    if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) SlotKeyPressed(1);
                }
                Running = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
                if (kb.spaceKey.wasPressedThisFrame) jumpQueued = true;
            }
            if (mouse == null) return;

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                // Bam vao cua so de bat dau dieu khien; lan bam nay khong tinh la tuong tac.
                if (mouse.leftButton.wasPressedThisFrame) SetCursorLocked(true);
                return;
            }

            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * LookDegreesPerPixel;
            pitch = Mathf.Clamp(pitch - delta.y * LookDegreesPerPixel * (InvertY ? -1f : 1f), -PitchLimit, PitchLimit);
            ApplyLook();

            pressed = mouse.leftButton.wasPressedThisFrame;
            held = mouse.leftButton.isPressed;
            secondary = mouse.rightButton.wasPressedThisFrame;
        }

        /// <summary>Phim T (tranh huong dan toan man hinh).</summary>
        public event System.Action TutorialKeyPressed;

        /// <summary>Phim 1 / 2 (o cam do cua tay van nang): tham so 0 / 1.</summary>
        public event System.Action<int> SlotKeyPressed;

        /// <summary>Test: nhu bam phim 1 / 2.</summary>
        public void TapSlotKey(int slot)
        {
            if (SlotKeyPressed != null) SlotKeyPressed(slot);
        }

        /// <summary>Test: nhu bam phim T.</summary>
        public void TapTutorialKey()
        {
            if (TutorialKeyPressed != null) TutorialKeyPressed();
        }

        void ReadEscapeOnly()
        {
            if (SimulateInput) return;
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) OnEscape();
            if (kb != null && kb.tabKey.wasPressedThisFrame && TabPressed != null) TabPressed();
        }

        void OnEscape()
        {
            if (EscapePressed != null) EscapePressed();
            else SetCursorLocked(false);
        }

        /// <summary>Dat camera o mot vi tri co dinh trong phong (menu chinh). Goi ReleaseCamera de tra ve mat Hope.</summary>
        public void HoldCamera(Vector3 worldPosition, Quaternion rotation)
        {
            CameraOverride = true;
            Cam.transform.SetPositionAndRotation(worldPosition, rotation);
        }

        public void ReleaseCamera()
        {
            if (!CameraOverride) return;
            CameraOverride = false;
            Cam.transform.localPosition = new Vector3(0f, EyeHeight, 0f);
            ApplyLook();
        }

        public static void SetCursorLocked(bool locked)
        {
            if (Application.isBatchMode) return;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public bool CursorLocked { get { return Cursor.lockState == CursorLockMode.Locked; } }

        void Move(Vector2 input, float dt)
        {
            if (input.sqrMagnitude > 1f) input.Normalize();
            Quaternion facing = Quaternion.Euler(0f, yaw, 0f);
            Vector3 walk = facing * new Vector3(input.x, 0f, input.y) * MoveSpeed * (Running ? RunMultiplier : 1f);
            // isGrounded co the tat o khung dung yen (buoc xuong < do day da): cho nhay neu vua cham dat trong 0,2 giay
            bool jump = jumpQueued && (body.isGrounded || groundedAgo < 0.2f || OnGround());
            jumpQueued = false;
            if (jump) fallSpeed = JumpSpeed;

            // Chia nho buoc di khi khung hinh dai (may cham, tua nhanh) de khong xuyen tuong / san.
            float left = dt;
            while (left > 0f)
            {
                float step = Mathf.Min(left, MaxStepSeconds);
                left -= step;
                if (jump) jump = false;   // buoc dau sau khi nhay: chua cham dat, giu van toc nhay
                else fallSpeed = body.isGrounded && fallSpeed <= 0f ? -1f : Mathf.Max(fallSpeed - Gravity * step, -MaxFallSpeed);
                Vector3 v = walk;
                v.y = fallSpeed;
                body.Move(v * step);
                groundedAgo = body.isGrounded ? 0f : groundedAgo + step;
            }

            // Luoi an toan: neu rot khoi phong thi dua ve san.
            if (transform.position.y < -1f) Teleport(new Vector3(transform.position.x, 0f, transform.position.z), yaw);
        }

        /// <summary>Chan cham mat dat / mat ban (tia ngan tu trong than xuong; vat cham khong phai trigger).</summary>
        bool OnGround()
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f, ~0, QueryTriggerInteraction.Ignore);
        }

        const float MaxStepSeconds = 0.05f;
        const float MaxFallSpeed = 10f;

        void ApplyLook()
        {
            Cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        /// <summary>Id cua nguyen lieu da tha (Sprint10Room) va khoang uu tien sau do vat khac.</summary>
        public const string DroppedPrefix = "Dropped_";

        /// <summary>Luc nay co uu tien nguyen lieu da tha khong (GameController: chi khi tay trong - dang muon nhat).</summary>
        public System.Func<bool> PreferDropped;
        const float DroppedReach = 0.8f;

        void RefreshFocus()
        {
            Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit[] hits = Physics.RaycastAll(ray, Reach, ~0, QueryTriggerInteraction.Collide);
            Interactable best = null, dropped = null;
            float bestDistance = float.MaxValue, droppedDistance = float.MaxValue;
            foreach (RaycastHit h in hits)
            {
                var i = h.collider.GetComponent<Interactable>();
                if (i == null || !i.isActiveAndEnabled) continue;
                if (i.Id != null && i.Id.StartsWith(DroppedPrefix) && h.distance < droppedDistance) { dropped = i; droppedDistance = h.distance; }
                if (h.distance >= bestDistance) continue;
                best = i;
                bestDistance = h.distance;
            }
            // Bug 23/11/2026: nguyen lieu tha len thot / mat ban nam trong hop bam lon cua thot nen tia nhin trung thot truoc
            // -> khong nhat lai duoc. Nguyen lieu da tha nam trong tam ngam thi uu tien (nam sau hop kia toi da DroppedReach).
            if (dropped != null && droppedDistance - bestDistance <= DroppedReach && (PreferDropped == null || PreferDropped())) best = dropped;
            Focus = best;
        }

        // ---------------- dieu khien cho test ----------------

        /// <summary>Quay dau nhin thang vao mot diem.</summary>
        public void LookAt(Vector3 worldPoint)
        {
            Vector3 d = worldPoint - Cam.transform.position;
            yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            float flat = new Vector2(d.x, d.z).magnitude;
            pitch = Mathf.Clamp(-Mathf.Atan2(d.y, flat) * Mathf.Rad2Deg, -PitchLimit, PitchLimit);
            ApplyLook();
            RefreshFocus();
        }

        public void SetLook(float newYaw, float newPitch)
        {
            yaw = newYaw;
            pitch = Mathf.Clamp(newPitch, -PitchLimit, PitchLimit);
            ApplyLook();
            RefreshFocus();
        }

        /// <summary>Bam chuot trai mot lan vao do dang o tam ngam (dung duong di nhu nguoi choi).</summary>
        public void TapPrimary()
        {
            RefreshFocus();
            if (Focus != null && Focus.Press != null) Focus.Press();
        }

        public void TapSecondary()
        {
            if (SecondaryPressed != null) SecondaryPressed();
        }

        /// <summary>Test: nhay mot lan (nhu bam Space).</summary>
        public void Jump()
        {
            jumpQueued = true;
        }

        /// <summary>
        /// Diem roi cua do tha bang chuot phai (yeu cau 22/11/2026): cho tam ngam chiem trung trong tam voi (mat ban, san, do
        /// vat); khong trung gi thi roi xuong san truoc mat. <paramref name="normal"/>: phap tuyen mat roi (len tren neu la san).
        /// </summary>
        public Vector3 DropPoint(out Vector3 normal)
        {
            Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit[] hits = Physics.RaycastAll(ray, Reach, ~0, QueryTriggerInteraction.Ignore);
            RaycastHit? best = null;
            foreach (RaycastHit h in hits)
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (best == null || h.distance < best.Value.distance) best = h;
            }
            if (best != null)
            {
                normal = best.Value.normal;
                return best.Value.point;
            }
            normal = Vector3.up;
            Vector3 ahead = transform.position + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 1.2f;
            return new Vector3(ahead.x, 0f, ahead.z);
        }

        /// <summary>Dat nguoi choi ngay tai mot diem (chi dung khi bat dau van).</summary>
        public void Teleport(Vector3 position, float newYaw)
        {
            body.enabled = false;
            transform.position = position;
            body.enabled = true;
            yaw = newYaw;
            pitch = 8f;
            fallSpeed = 0f;
            ApplyLook();
        }

        static float NormalizeAngle(float a)
        {
            a %= 360f;
            return a > 180f ? a - 360f : a;
        }
    }
}
