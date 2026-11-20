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

        /// <summary>Bam chuot phai.</summary>
        public event Action SecondaryPressed;

        /// <summary>Bam Esc (F-050 AC-2: mo menu tam dung). Khong ai nghe thi Esc nha chuot nhu cu.</summary>
        public event Action EscapePressed;

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

        void ReadEscapeOnly()
        {
            if (SimulateInput) return;
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) OnEscape();
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
            Vector3 walk = facing * new Vector3(input.x, 0f, input.y) * MoveSpeed;

            // Chia nho buoc di khi khung hinh dai (may cham, tua nhanh) de khong xuyen tuong / san.
            float left = dt;
            while (left > 0f)
            {
                float step = Mathf.Min(left, MaxStepSeconds);
                left -= step;
                fallSpeed = body.isGrounded ? -1f : Mathf.Max(fallSpeed - Gravity * step, -MaxFallSpeed);
                Vector3 v = walk;
                v.y = fallSpeed;
                body.Move(v * step);
            }

            // Luoi an toan: neu rot khoi phong thi dua ve san.
            if (transform.position.y < -1f) Teleport(new Vector3(transform.position.x, 0f, transform.position.z), yaw);
        }

        const float MaxStepSeconds = 0.05f;
        const float MaxFallSpeed = 10f;

        void ApplyLook()
        {
            Cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        void RefreshFocus()
        {
            Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit[] hits = Physics.RaycastAll(ray, Reach, ~0, QueryTriggerInteraction.Collide);
            Interactable best = null;
            float bestDistance = float.MaxValue;
            foreach (RaycastHit h in hits)
            {
                var i = h.collider.GetComponent<Interactable>();
                if (i == null || !i.isActiveAndEnabled || h.distance >= bestDistance) continue;
                best = i;
                bestDistance = h.distance;
            }
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
