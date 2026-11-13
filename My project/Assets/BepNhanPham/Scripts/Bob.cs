using UnityEngine;

namespace BepNhanPham
{
    /// <summary>Nhun nhe len xuong (trai tim, hoi nuoc, nhan vat dang cho).</summary>
    public class Bob : MonoBehaviour
    {
        public float amplitude = 0.06f;
        public float speed = 2.2f;
        public float phase = 0f;
        Vector3 basePos;

        void OnEnable() { basePos = transform.localPosition; }

        void Update()
        {
            transform.localPosition = basePos + Vector3.up * (Mathf.Sin(Time.time * speed + phase) * amplitude);
        }
    }
}
