using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Xoay camera giua 4 goc cua nha hang va bat/tat UI cua tung goc.
    /// Goi Next() / Prev() tu nut bam, hoac GoTo(0..3) tu code.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        public GameObject[] cornerUI;
        public float turnSpeed = 6f;
        public int index = 0;
        float yaw;

        void Start()
        {
            yaw = index * 90f;
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            RefreshUI();
        }

        public void Next() { GoTo(index + 1); }
        public void Prev() { GoTo(index - 1); }

        public void GoTo(int i)
        {
            int delta = i - index;
            index = ((i % 4) + 4) % 4;
            yaw += delta * 90f;
            RefreshUI();
        }

        void RefreshUI()
        {
            if (cornerUI == null) return;
            for (int i = 0; i < cornerUI.Length; i++)
            {
                if (cornerUI[i] != null) cornerUI[i].SetActive(i == index);
            }
        }

        void Update()
        {
            Quaternion target = Quaternion.Euler(0f, yaw, 0f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Mathf.Clamp01(Time.deltaTime * turnSpeed));
        }
    }
}
