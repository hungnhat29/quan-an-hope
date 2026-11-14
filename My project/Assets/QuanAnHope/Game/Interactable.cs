using System;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Do vat bam duoc trong phong (constitution Dieu 5): nguoi choi nhin vao (tam ngam) va bam / giu chuot trai.
    /// Collider la trigger nen khong chan duong di, chi de tia nhin trung.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        /// <summary>Ten tram, dung cho test va log (vd "Kiosk", "Board", "Jar_Dầu ăn").</summary>
        public string Id;

        /// <summary>Goi y hien duoi tam ngam khi dang nhin vao.</summary>
        public Func<string> Prompt;

        /// <summary>Bam chuot trai mot lan.</summary>
        public Action Press;

        /// <summary>Dang giu chuot trai (moi khung hinh, tham so la so giay).</summary>
        public Action<float> Hold;

        public BoxCollider Box { get; private set; }

        public static Interactable Add(GameObject go, string id, Vector3 center, Vector3 size, Func<string> prompt, Action press,
            Action<float> hold = null)
        {
            var i = go.AddComponent<Interactable>();
            i.Id = id;
            i.Prompt = prompt;
            i.Press = press;
            i.Hold = hold;
            i.Box = go.AddComponent<BoxCollider>();
            i.Box.isTrigger = true;
            i.Box.center = center;
            i.Box.size = size;
            return i;
        }

        /// <summary>Tam vung bam theo toa do the gioi.</summary>
        public Vector3 WorldCenter { get { return Box != null ? Box.bounds.center : transform.position; } }

        public string CurrentPrompt { get { return Prompt != null ? Prompt() : Id; } }
    }
}
