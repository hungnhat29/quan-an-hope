using UnityEngine;

namespace BepNhanPham
{
    /// <summary>Doi bieu cam bang cach doi anh 2D tren mat nhan vat.</summary>
    public class FaceSwap : MonoBehaviour
    {
        public Renderer faceRenderer;
        public Texture2D happy;
        public Texture2D waiting;
        public Texture2D angry;
        MaterialPropertyBlock block;

        public void Show(Texture2D face)
        {
            if (faceRenderer == null || face == null) return;
            if (block == null) block = new MaterialPropertyBlock();
            faceRenderer.GetPropertyBlock(block);
            block.SetTexture("_MainTex", face);
            faceRenderer.SetPropertyBlock(block);
        }

        [ContextMenu("Mat vui")] public void ShowHappy() { Show(happy); }
        [ContextMenu("Mat cho")] public void ShowWaiting() { Show(waiting); }
        [ContextMenu("Mat gian")] public void ShowAngry() { Show(angry); }
    }
}
