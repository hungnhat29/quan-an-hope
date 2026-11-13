using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Xoay camera giua 4 goc cua nha hang va bat/tat UI cua tung goc.
    /// Goi Next() / Prev() tu nut bam, hoac GoTo(0..3) tu code.
    /// Art Bible muc 9: xoay 90 do trong 0,35 giay, ease-in-out.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        public GameObject[] cornerUI;
        public float turnDuration = 0.35f;
        public int index = 0;

        float fromYaw, toYaw, currentYaw;
        float progress = 1f;

        void Start()
        {
            currentYaw = fromYaw = toYaw = index * 90f;
            progress = 1f;
            transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
            RefreshUI();
        }

        public void Next() { GoTo(index + 1); }
        public void Prev() { GoTo(index - 1); }

        public void GoTo(int i)
        {
            int delta = i - index;
            index = ((i % 4) + 4) % 4;
            fromYaw = currentYaw;
            toYaw += delta * 90f;
            progress = 0f;
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
            if (progress >= 1f) return;
            progress = turnDuration <= 0f ? 1f : Mathf.Min(1f, progress + Time.deltaTime / turnDuration);
            float eased = progress * progress * (3f - 2f * progress);
            currentYaw = Mathf.Lerp(fromYaw, toYaw, eased);
            transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
        }
    }
}
