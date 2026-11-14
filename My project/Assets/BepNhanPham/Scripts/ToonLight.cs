using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Gan vao den Directional cua canh. Moi khung hinh dua huong va mau den vao shader ToonFlat
    /// (Art Bible muc 10: sang toi 2 nac theo mot den chinh co dinh trong khong gian phong).
    /// Doi goc den = xoay doi tuong nay; doi mau theo gio / ngay mat dien = doi mau Light.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class ToonLight : MonoBehaviour
    {
        static readonly int DirId = Shader.PropertyToID("_BNP_LightDir");
        static readonly int ColorId = Shader.PropertyToID("_BNP_LightColor");

        Light lamp;

        void OnEnable()
        {
            lamp = GetComponent<Light>();
            Push();
        }

        void Update()
        {
            Push();
        }

        void OnDisable()
        {
            Shader.SetGlobalVector(DirId, Vector4.zero);
            Shader.SetGlobalVector(ColorId, Vector4.zero);
        }

        void Push()
        {
            if (lamp == null) lamp = GetComponent<Light>();
            Vector3 toLight = -transform.forward;
            Shader.SetGlobalVector(DirId, new Vector4(toLight.x, toLight.y, toLight.z, 1f));
            Color c = lamp.color * Mathf.Clamp(lamp.intensity, 0f, 1.5f);
            Shader.SetGlobalVector(ColorId, new Vector4(c.r, c.g, c.b, 1f));
        }
    }
}
