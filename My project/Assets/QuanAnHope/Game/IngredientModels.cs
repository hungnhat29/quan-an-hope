using System.Collections.Generic;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Hinh 3D cua tung nguyen lieu, ghep tu khoi co ban (F-012; Art Bible muc 5 "hinh khoi tron", muc 7
    /// "mot hinh dang cho moi bac"). Moi loai co hai hinh: nguyen (trong thung) va da so che theo kieu so che
    /// cua constitution 1.4 muc 4.5 (ca chua thai mui, hanh thai lat, trung danh trong bat, thit thai mong...).
    /// Kich thuoc that (met), goc toa do o mat tren de bac; khoi dau tien ten "Than".
    /// Sprint 8 thay bang model Blender.
    /// </summary>
    public static class IngredientModels
    {
        /// <summary>Vien dao cu nho (Art Bible muc 5: 0,018 m o ty le tay cam 2,2) va vien chi tiet nho.</summary>
        const float Big = 0.008f, Small = 0.004f, None = 0f;

        public struct Part
        {
            public PrimitiveType type;
            public string name;
            public Vector3 pos, size, euler;
            public Color color;
            public float outline;
        }

        class Shape
        {
            public readonly List<Part> parts = new List<Part>();

            Shape Add(PrimitiveType t, string name, Vector3 p, Vector3 s, string hex, Vector3 e, float o)
            {
                parts.Add(new Part { type = t, name = parts.Count == 0 ? "Than" : name + "_" + parts.Count, pos = p, size = s, euler = e, color = UiKit.Hex(hex), outline = o });
                return this;
            }

            public Shape Sph(string n, Vector3 p, Vector3 s, string hex, Vector3 e = default(Vector3), float o = Big) { return Add(PrimitiveType.Sphere, n, p, s, hex, e, o); }
            public Shape Box(string n, Vector3 p, Vector3 s, string hex, Vector3 e = default(Vector3), float o = Big) { return Add(PrimitiveType.Cube, n, p, s, hex, e, o); }
            public Shape Cyl(string n, Vector3 p, Vector3 s, string hex, Vector3 e = default(Vector3), float o = Big) { return Add(PrimitiveType.Cylinder, n, p, s, hex, e, o); }
            public Shape Cap(string n, Vector3 p, Vector3 s, string hex, Vector3 e = default(Vector3), float o = Big) { return Add(PrimitiveType.Capsule, n, p, s, hex, e, o); }
        }

        static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }

        /// <summary>Diem tren vong tron ban kinh r, goc a do (0 = truc x), do cao y.</summary>
        static Vector3 Ring(float r, float a, float y) { return new Vector3(Mathf.Cos(a * Mathf.Deg2Rad) * r, y, Mathf.Sin(a * Mathf.Deg2Rad) * r); }

        // Mau phu (mau chinh lay tu IngredientArt de sach / bep dung chung).
        const string LeafDark = "#4F8A4C", Leaf = "#6FA855", Stem = "#9CCB7A", Cream = "#FFF3DC", Wood = "#C98E5A", Tan = "#D69B68";

        public static List<Part> Parts(string id, bool prepared)
        {
            Shape s = prepared ? Prepared(id) : Raw(id);
            return s.parts;
        }

        // =====================================================================
        // Nguyen (trong thung)
        // =====================================================================

        static Shape Raw(string id)
        {
            var s = new Shape();
            switch (id)
            {
                // ---------- vong 1: chi tiet (>= 3 khoi) ----------
                case "ca_chua":
                    s.Sph("Qua", V(0f, 0.029f, 0f), V(0.074f, 0.056f, 0.074f), "#E8604F")
                     .Sph("Bong", V(-0.018f, 0.045f, -0.022f), V(0.018f, 0.01f, 0.012f), "#F59A8C", V(0f, 30f, 0f), None)
                     .Cyl("Cuong", V(0f, 0.062f, 0f), V(0.007f, 0.014f, 0.007f), "#5E8F3E", default(Vector3), Small);
                    for (int i = 0; i < 5; i++)
                        s.Sph("LaDai", Ring(0.012f, 72f * i, 0.0575f), V(0.026f, 0.005f, 0.01f), Leaf, V(0f, -72f * i, -14f), Small);
                    return s;
                case "hanh_tay":
                    s.Sph("Cu", V(0f, 0.03f, 0f), V(0.066f, 0.06f, 0.066f), "#D98FB0")
                     .Cap("Chop", V(0f, 0.066f, 0f), V(0.014f, 0.012f, 0.014f), Tan, default(Vector3), Small);
                    for (int i = 0; i < 3; i++)
                        s.Sph("Van", V(0f, 0.03f, 0f), V(0.004f, 0.061f, 0.067f), "#B96F92", V(0f, 60f * i, 0f), None);
                    for (int i = 0; i < 3; i++)
                        s.Cyl("Re", V(-0.006f + 0.006f * i, 0.002f, 0.002f * i), V(0.002f, 0.008f, 0.002f), "#E9CE9E", V(0f, 0f, -20f + 20f * i), None);
                    return s;
                case "rau_muong":
                    s.Cyl("Cong", V(0f, 0.012f, 0f), V(0.012f, 0.11f, 0.012f), Stem, V(0f, 0f, 90f));
                    s.Cyl("Cong", V(0f, 0.012f, 0.013f), V(0.011f, 0.105f, 0.011f), Stem, V(0f, 12f, 90f));
                    s.Cyl("Cong", V(0f, 0.012f, -0.013f), V(0.011f, 0.105f, 0.011f), Stem, V(0f, -12f, 90f));
                    for (int i = 0; i < 5; i++)
                        s.Sph("La", V(0.045f + 0.008f * (i % 2), 0.02f, -0.024f + 0.012f * i), V(0.036f, 0.006f, 0.015f), LeafDark, V(0f, -30f + 15f * i, 0f), Small);
                    s.Cyl("DayBuoc", V(-0.012f, 0.012f, 0f), V(0.034f, 0.008f, 0.034f), Tan, V(0f, 0f, 90f), Small);
                    return s;
                case "trung":
                    s.Sph("VoDuoi", V(0f, 0.026f, 0f), V(0.054f, 0.052f, 0.054f), "#F4DDB8")
                     .Sph("VoTren", V(0f, 0.047f, 0f), V(0.043f, 0.046f, 0.043f), "#F4DDB8")
                     .Sph("Dom", V(0.02f, 0.035f, -0.017f), V(0.006f, 0.005f, 0.004f), "#DDBF94", V(0f, 40f, 0f), None)
                     .Sph("Dom", V(-0.012f, 0.052f, -0.017f), V(0.005f, 0.004f, 0.004f), "#DDBF94", V(0f, -30f, 0f), None);
                    return s;
                case "heo":
                    s.Box("Nac", V(0f, 0.017f, 0.006f), V(0.078f, 0.032f, 0.046f), "#F2A0A6")
                     .Box("Mo", V(0f, 0.017f, -0.024f), V(0.078f, 0.032f, 0.014f), Cream)
                     .Box("Bi", V(0f, 0.017f, -0.0325f), V(0.078f, 0.03f, 0.004f), "#E9B28F", default(Vector3), Small)
                     .Box("VanMo", V(0.008f, 0.0335f, 0.008f), V(0.05f, 0.002f, 0.004f), Cream, V(0f, 15f, 0f), None)
                     .Box("VanMo", V(-0.012f, 0.0335f, 0.016f), V(0.03f, 0.002f, 0.003f), Cream, V(0f, -20f, 0f), None);
                    return s;
                case "dau_phu":
                    s.Box("Khoi", V(0f, 0.024f, 0f), V(0.06f, 0.046f, 0.046f), "#F1EAD8")
                     .Box("Khoi", V(0f, 0.024f, 0f), V(0.046f, 0.046f, 0.06f), "#F1EAD8");
                    for (int i = 0; i < 4; i++)
                        s.Cyl("Goc", V(i < 2 ? -0.023f : 0.023f, 0.024f, i % 2 == 0 ? -0.023f : 0.023f), V(0.014f, 0.046f, 0.014f), "#F1EAD8", default(Vector3), Small);
                    s.Box("MatTren", V(0f, 0.0475f, 0f), V(0.05f, 0.002f, 0.05f), "#FBF6EA", default(Vector3), None);
                    return s;

                // ---------- vong 2-4 va dot pha: muc co ban (>= 2 khoi) ----------
                case "khoai_tay":
                    return s.Sph("Cu", V(0f, 0.026f, 0f), V(0.074f, 0.05f, 0.056f), Tan)
                        .Sph("Mat", V(0.02f, 0.044f, -0.018f), V(0.007f, 0.004f, 0.007f), "#8C5A3A", default(Vector3), None)
                        .Sph("Mat", V(-0.018f, 0.04f, -0.02f), V(0.006f, 0.004f, 0.006f), "#8C5A3A", default(Vector3), None);
                case "nam_rom":
                    return s.Sph("Mu", V(0f, 0.042f, 0f), V(0.05f, 0.046f, 0.05f), "#8F8578")
                        .Cyl("Chan", V(0f, 0.014f, 0f), V(0.032f, 0.028f, 0.032f), "#EDE6D8");
                case "bong_cai":
                    s.Sph("Bong", V(0f, 0.048f, 0f), V(0.058f, 0.04f, 0.058f), "#5E9E5B");
                    for (int i = 0; i < 3; i++) s.Sph("Bong", Ring(0.022f, 120f * i, 0.042f), V(0.034f, 0.03f, 0.034f), "#6FA855", default(Vector3), Small);
                    return s.Cyl("Cuong", V(0f, 0.014f, 0f), V(0.024f, 0.028f, 0.024f), "#A9CF8C");
                case "ot_chuong":
                    return s.Sph("Qua", V(0f, 0.033f, 0f), V(0.064f, 0.066f, 0.064f), "#D9382C")
                        .Sph("Dai", V(0f, 0.065f, 0f), V(0.024f, 0.006f, 0.024f), LeafDark, default(Vector3), Small)
                        .Cyl("Cuong", V(0f, 0.074f, 0f), V(0.009f, 0.016f, 0.009f), "#5E8F3E", default(Vector3), Small);
                case "bi_do":
                    s.Sph("Qua", V(0f, 0.028f, 0f), V(0.082f, 0.056f, 0.082f), "#F08A6A");
                    for (int i = 0; i < 3; i++) s.Sph("Mui", V(0f, 0.028f, 0f), V(0.005f, 0.055f, 0.083f), "#D9704F", V(0f, 60f * i, 0f), None);
                    return s.Cyl("Cuong", V(0f, 0.06f, 0f), V(0.01f, 0.014f, 0.01f), "#8C6A3A", default(Vector3), Small);
                case "mang_tay":
                    for (int i = 0; i < 3; i++)
                    {
                        s.Cyl("Than", V(0f, 0.008f, -0.012f + 0.012f * i), V(0.01f, 0.1f, 0.01f), "#7DB87A", V(0f, -6f + 6f * i, 90f));
                        s.Sph("Ngon", V(0.052f, 0.008f, -0.012f + 0.012f * i - 0.005f * (i - 1)), V(0.018f, 0.012f, 0.012f), "#5E8F3E", default(Vector3), Small);
                    }
                    return s;
                case "nam_dong_co":
                    return s.Sph("Mu", V(0f, 0.034f, 0f), V(0.064f, 0.03f, 0.064f), "#8A4F3A")
                        .Cyl("Chan", V(0f, 0.012f, 0f), V(0.018f, 0.024f, 0.018f), "#D9C9A8")
                        .Box("Nut", V(0f, 0.049f, 0f), V(0.03f, 0.002f, 0.004f), "#D9C9A8", V(0f, 35f, 0f), None);
                case "bo":
                    return s.Box("Nac", V(0f, 0.017f, 0f), V(0.078f, 0.032f, 0.056f), "#B8443C")
                        .Box("VanMo", V(0.01f, 0.0335f, 0.01f), V(0.05f, 0.002f, 0.004f), "#F3E2D5", V(0f, 25f, 0f), None)
                        .Box("VanMo", V(-0.012f, 0.0335f, -0.01f), V(0.04f, 0.002f, 0.003f), "#F3E2D5", V(0f, -15f, 0f), None)
                        .Box("Mo", V(0f, 0.017f, -0.03f), V(0.078f, 0.03f, 0.006f), Cream, default(Vector3), Small);
                case "tom":
                    for (int i = 0; i < 4; i++) s.Sph("Dot", Ring(0.026f, 200f + 40f * i, 0.016f), V(0.028f, 0.026f, 0.026f), "#F08A6A");
                    return s.Sph("Dau", Ring(0.026f, 160f, 0.018f), V(0.03f, 0.028f, 0.028f), "#E8604F")
                        .Sph("Duoi", Ring(0.026f, 345f, 0.014f) + V(0.008f, 0f, 0.006f), V(0.024f, 0.006f, 0.026f), "#D9704F", V(0f, 30f, 0f), Small)
                        .Cyl("Rau", Ring(0.04f, 150f, 0.02f), V(0.002f, 0.04f, 0.002f), "#D9704F", V(0f, 30f, 80f), None);
                case "ca_hoi":
                    s.Box("Thit", V(0f, 0.018f, 0f), V(0.082f, 0.028f, 0.05f), "#F08A6A")
                     .Box("Da", V(0f, 0.002f, 0f), V(0.082f, 0.004f, 0.05f), "#9AA4A8", default(Vector3), Small);
                    for (int i = 0; i < 3; i++) s.Box("Van", V(-0.024f + 0.024f * i, 0.0325f, 0f), V(0.004f, 0.002f, 0.05f), "#FFE3D3", V(0f, 25f, 0f), None);
                    return s;
                case "muc":
                    s.Cap("Than", V(0f, 0.04f, 0f), V(0.034f, 0.024f, 0.034f), "#EFE7DA")
                     .Sph("Vay", V(0f, 0.062f, 0f), V(0.05f, 0.006f, 0.018f), "#E3D6C6", default(Vector3), Small);
                    for (int i = 0; i < 4; i++) s.Cyl("Rau", Ring(0.008f, 90f * i + 45f, 0.008f), V(0.004f, 0.026f, 0.004f), "#E3D6C6", V(0f, -90f * i, 25f), None);
                    return s;
                case "ga":
                    return s.Sph("Dui", V(0.008f, 0.025f, 0f), V(0.06f, 0.046f, 0.05f), "#F3CFA0")
                        .Cyl("Xuong", V(-0.034f, 0.022f, 0f), V(0.012f, 0.03f, 0.012f), "#FFF8EE", V(0f, 0f, 90f), Small)
                        .Sph("DauXuong", V(-0.052f, 0.022f, 0f), V(0.02f, 0.018f, 0.02f), "#FFF8EE", default(Vector3), Small);
                case "tom_hum":
                    return s.Cap("Than", V(0f, 0.022f, 0f), V(0.04f, 0.04f, 0.036f), "#D9382C", V(0f, 0f, 90f))
                        .Sph("Cang", V(-0.05f, 0.02f, 0.03f), V(0.032f, 0.02f, 0.022f), "#C42E24", V(0f, 30f, 0f))
                        .Sph("Cang", V(-0.05f, 0.02f, -0.03f), V(0.032f, 0.02f, 0.022f), "#C42E24", V(0f, -30f, 0f))
                        .Sph("Duoi", V(0.055f, 0.016f, 0f), V(0.02f, 0.008f, 0.034f), "#C42E24", default(Vector3), Small);
                case "wagyu":
                    s.Box("Nac", V(0f, 0.017f, 0f), V(0.076f, 0.032f, 0.056f), "#E28B8B");
                    for (int i = 0; i < 4; i++)
                        s.Box("VanMo", V(-0.024f + 0.016f * i, 0.0335f, 0.004f * (i % 2 == 0 ? 1 : -1)), V(0.03f, 0.002f, 0.004f), "#FFF3EC", V(0f, 40f * i, 0f), None);
                    return s;
                case "ca_ngu":
                    return s.Box("Than", V(0f, 0.018f, 0f), V(0.082f, 0.034f, 0.046f), "#C94A3B")
                        .Box("Soc", V(0f, 0.0355f, 0f), V(0.082f, 0.002f, 0.012f), "#9E3329", default(Vector3), None)
                        .Box("Van", V(0.012f, 0.0355f, 0.014f), V(0.004f, 0.002f, 0.02f), "#E57A6C", V(0f, 30f, 0f), None);
                case "cua":
                    s.Sph("Mai", V(0f, 0.022f, 0f), V(0.072f, 0.03f, 0.056f), "#E8604F")
                     .Sph("Cang", V(-0.03f, 0.018f, -0.036f), V(0.03f, 0.02f, 0.02f), "#D9382C", V(0f, -30f, 0f))
                     .Sph("Cang", V(0.03f, 0.018f, -0.036f), V(0.03f, 0.02f, 0.02f), "#D9382C", V(0f, 30f, 0f));
                    for (int i = 0; i < 3; i++)
                    {
                        s.Cyl("Chan", V(-0.04f, 0.01f, -0.008f + 0.014f * i), V(0.004f, 0.03f, 0.004f), "#D9382C", V(0f, -15f + 15f * i, 70f), None);
                        s.Cyl("Chan", V(0.04f, 0.01f, -0.008f + 0.014f * i), V(0.004f, 0.03f, 0.004f), "#D9382C", V(0f, 15f - 15f * i, -70f), None);
                    }
                    return s;
                default:
                    return s.Sph("Qua", V(0f, 0.032f, 0f), V(0.064f, 0.064f, 0.064f), "#E9CE9E")
                        .Sph("Bong", V(-0.016f, 0.046f, -0.02f), V(0.014f, 0.01f, 0.01f), Cream, default(Vector3), None);
            }
        }

        // =====================================================================
        // Da so che (constitution 1.4 muc 4.5: cot "So che")
        // =====================================================================

        static Shape Prepared(string id)
        {
            var s = new Shape();
            switch (id)
            {
                case "ca_chua": // thai mui: 5 mui cam nam quanh, ruot hong nhat
                    for (int i = 0; i < 5; i++)
                    {
                        float a = 72f * i;
                        s.Sph("Mui", Ring(0.022f, a, 0.012f), V(0.048f, 0.024f, 0.02f), "#E8604F", V(0f, -a, 0f));
                        s.Sph("Ruot", Ring(0.022f, a, 0.02f), V(0.038f, 0.012f, 0.011f), "#F59A8C", V(0f, -a, 0f), None);
                    }
                    return s;
                case "hanh_tay": // thai lat: 3 khoanh hanh, vong ngoai tim hong, vong trong trang
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 p = V(-0.012f + 0.012f * i, 0.004f + 0.006f * i, -0.006f + 0.006f * i);
                        s.Cyl("Khoanh", p, V(0.056f, 0.006f, 0.056f), "#D98FB0", V(6f, 0f, 0f));
                        s.Cyl("VongTrang", p + V(0f, 0.0004f, 0f), V(0.042f, 0.0062f, 0.042f), "#FBEFF4", V(6f, 0f, 0f), None);
                        s.Cyl("Loi", p + V(0f, 0.0008f, 0f), V(0.024f, 0.0064f, 0.024f), "#D98FB0", V(6f, 0f, 0f), None);
                    }
                    return s;
                case "rau_muong": // nhat, rua: khuc cong ngan + la roi
                    for (int i = 0; i < 6; i++)
                        s.Cyl("Khuc", Ring(0.018f + 0.006f * (i % 2), 60f * i, 0.006f), V(0.009f, 0.032f, 0.009f), Stem, V(0f, 25f + 60f * i, 90f));
                    for (int i = 0; i < 3; i++)
                        s.Sph("La", Ring(0.01f, 120f * i + 30f, 0.012f), V(0.034f, 0.005f, 0.014f), LeafDark, V(0f, -120f * i, 0f), Small);
                    return s;
                case "trung": // dap, danh: bat trung danh co dua
                    return s.Cyl("Bat", V(0f, 0.016f, 0f), V(0.076f, 0.032f, 0.076f), "#F7F3EA")
                        .Cyl("TrungDanh", V(0f, 0.0315f, 0f), V(0.064f, 0.003f, 0.064f), "#F6C445", default(Vector3), None)
                        .Sph("Bot", V(0.01f, 0.033f, 0.006f), V(0.03f, 0.003f, 0.016f), "#FBE08A", V(0f, 30f, 0f), None)
                        .Cyl("Dua", V(0.006f, 0.05f, 0f), V(0.004f, 0.09f, 0.004f), Wood, V(0f, 30f, 65f), Small);
                case "heo": // thai mong: 4 lat xep loi, moi lat co vien mo
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 p = V(-0.012f + 0.008f * i, 0.004f + 0.0045f * i, -0.024f + 0.016f * i);
                        s.Box("Lat", p, V(0.062f, 0.005f, 0.022f), "#F2A0A6", V(10f, 6f * i, 0f));
                        s.Box("VienMo", p + V(0f, 0f, -0.012f), V(0.062f, 0.0052f, 0.005f), Cream, V(10f, 6f * i, 0f), None);
                    }
                    return s;
                case "dau_phu": // cat mieng: 4 mieng vuong
                    for (int i = 0; i < 4; i++)
                        s.Box("Mieng", V(i < 2 ? -0.016f : 0.016f, 0.012f, i % 2 == 0 ? -0.016f : 0.016f), V(0.026f, 0.024f, 0.026f), "#F1EAD8", V(0f, 8f * i, 0f));
                    return s;

                case "khoai_tay":
                    for (int i = 0; i < 5; i++) s.Box("Thanh", Ring(0.012f, 72f * i, 0.007f + 0.004f * (i % 2)), V(0.05f, 0.011f, 0.011f), "#F3D58A", V(0f, 72f * i + 20f, 0f));
                    return s;
                case "nam_rom":
                    for (int i = 0; i < 2; i++)
                    {
                        float x = i == 0 ? -0.018f : 0.018f;
                        s.Sph("NuaMu", V(x, 0.026f, 0f), V(0.022f, 0.044f, 0.048f), "#8F8578");
                        s.Box("MatCat", V(x * 0.55f, 0.026f, 0f), V(0.002f, 0.04f, 0.04f), "#F4EFE6", default(Vector3), None);
                    }
                    return s;
                case "bong_cai":
                    for (int i = 0; i < 3; i++)
                    {
                        s.Sph("Bong", Ring(0.02f, 120f * i, 0.024f), V(0.03f, 0.024f, 0.03f), "#5E9E5B");
                        s.Cyl("Cuong", Ring(0.02f, 120f * i, 0.008f), V(0.012f, 0.016f, 0.012f), "#A9CF8C", default(Vector3), Small);
                    }
                    return s;
                case "ot_chuong":
                    for (int i = 0; i < 5; i++) s.Box("Soi", Ring(0.012f, 72f * i, 0.006f), V(0.052f, 0.008f, 0.01f), "#D9382C", V(0f, 72f * i + 15f, 0f));
                    return s;
                case "bi_do":
                    for (int i = 0; i < 4; i++) s.Box("Mieng", V(i < 2 ? -0.016f : 0.016f, 0.012f, i % 2 == 0 ? -0.016f : 0.016f), V(0.025f, 0.024f, 0.025f), "#F2A65A", V(0f, 10f * i, 0f));
                    return s;
                case "mang_tay":
                    for (int i = 0; i < 6; i++) s.Cyl("Khuc", Ring(0.018f, 60f * i, 0.006f), V(0.01f, 0.026f, 0.01f), "#7DB87A", V(0f, 60f * i, 90f));
                    return s;
                case "nam_dong_co":
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 p = V(-0.018f + 0.012f * i, 0.004f, -0.008f + 0.005f * i);
                        s.Sph("Lat", p, V(0.05f, 0.006f, 0.02f), "#8A4F3A", V(0f, 15f * i, 0f));
                        s.Sph("Ruot", p + V(0f, 0.001f, 0.002f), V(0.04f, 0.006f, 0.01f), "#D9C9A8", V(0f, 15f * i, 0f), None);
                    }
                    return s;
                case "bo":
                    for (int i = 0; i < 4; i++) s.Box("Lat", V(-0.012f + 0.008f * i, 0.004f + 0.0045f * i, -0.024f + 0.016f * i), V(0.062f, 0.005f, 0.022f), "#B8443C", V(10f, 6f * i, 0f));
                    return s;
                case "tom":
                    for (int i = 0; i < 4; i++) s.Sph("Dot", Ring(0.024f, 200f + 40f * i, 0.014f), V(0.026f, 0.022f, 0.024f), "#F6B39A");
                    return s.Sph("Duoi", Ring(0.024f, 345f, 0.012f) + V(0.008f, 0f, 0.006f), V(0.02f, 0.005f, 0.022f), "#E8604F", V(0f, 30f, 0f), Small);
                case "ca_hoi":
                    s.Box("Thit", V(0f, 0.014f, 0f), V(0.066f, 0.026f, 0.04f), "#F59A72");
                    for (int i = 0; i < 3; i++) s.Box("Van", V(-0.02f + 0.02f * i, 0.0275f, 0f), V(0.004f, 0.002f, 0.04f), "#FFE3D3", V(0f, 25f, 0f), None);
                    return s;
                case "muc":
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 p = Ring(0.018f, 120f * i, 0.005f);
                        s.Cyl("Khoanh", p, V(0.03f, 0.01f, 0.03f), "#EFE7DA");
                        s.Cyl("Lo", p + V(0f, 0.0005f, 0f), V(0.017f, 0.011f, 0.017f), "#C9BCA8", default(Vector3), None);
                    }
                    return s;
                case "ga":
                    for (int i = 0; i < 3; i++) s.Sph("Mieng", Ring(0.018f, 120f * i, 0.014f), V(0.032f, 0.026f, 0.03f), "#F3CFA0", V(0f, 30f * i, 0f));
                    return s;
                case "tom_hum":
                    return s.Sph("NuaVo", V(0f, 0.012f, 0f), V(0.08f, 0.02f, 0.036f), "#D9382C")
                        .Sph("Thit", V(0f, 0.02f, 0f), V(0.068f, 0.014f, 0.026f), Cream, default(Vector3), Small);
                case "wagyu":
                    for (int i = 0; i < 2; i++)
                    {
                        float x = i == 0 ? -0.02f : 0.02f;
                        s.Box("Mieng", V(x, 0.016f, 0f), V(0.034f, 0.03f, 0.05f), "#E28B8B");
                        s.Box("VanMo", V(x, 0.0315f, 0.004f), V(0.026f, 0.002f, 0.004f), "#FFF3EC", V(0f, 30f, 0f), None);
                    }
                    return s;
                case "ca_ngu":
                    for (int i = 0; i < 3; i++) s.Box("Lat", V(-0.014f + 0.014f * i, 0.007f + 0.004f * i, 0f), V(0.03f, 0.012f, 0.042f), "#C94A3B", V(0f, 0f, -12f));
                    return s;
                case "cua":
                    return s.Sph("Than", V(0f, 0.016f, 0f), V(0.062f, 0.026f, 0.05f), "#E8604F")
                        .Sph("Thit", V(0f, 0.027f, 0f), V(0.046f, 0.01f, 0.036f), Cream, default(Vector3), Small)
                        .Sph("Mai", V(0.05f, 0.012f, 0.02f), V(0.05f, 0.012f, 0.04f), "#D9382C", V(0f, 0f, 25f));
                default:
                    for (int i = 0; i < 3; i++) s.Box("Mieng", Ring(0.016f, 120f * i, 0.01f), V(0.024f, 0.02f, 0.024f), "#E9CE9E");
                    return s;
            }
        }

        /// <summary>
        /// Mot mieng da cat de rai trong chao / noi (F-013 AC-2), tam o goc toa do, nam doc truc x.
        /// count = so mieng cua mot nguyen lieu (4-6). Trung danh la lop long (count = 0, ve rieng).
        /// </summary>
        public static List<Part> Piece(string id, out int count)
        {
            var s = new Shape();
            switch (id)
            {
                case "ca_chua":
                    count = 5;
                    s.Sph("Mui", V(0f, 0.012f, 0f), V(0.048f, 0.024f, 0.02f), "#E8604F")
                     .Sph("Ruot", V(0f, 0.02f, 0f), V(0.038f, 0.012f, 0.011f), "#F59A8C", default(Vector3), None);
                    return s.parts;
                case "hanh_tay":
                    count = 4;
                    s.Cyl("Khoanh", V(0f, 0.003f, 0f), V(0.05f, 0.006f, 0.05f), "#D98FB0")
                     .Cyl("VongTrang", V(0f, 0.0034f, 0f), V(0.038f, 0.0062f, 0.038f), "#FBEFF4", default(Vector3), None)
                     .Cyl("Loi", V(0f, 0.0038f, 0f), V(0.022f, 0.0064f, 0.022f), "#D98FB0", default(Vector3), None);
                    return s.parts;
                case "rau_muong":
                    count = 6;
                    s.Cyl("Khuc", V(0f, 0.005f, 0f), V(0.009f, 0.032f, 0.009f), Stem, V(0f, 0f, 90f))
                     .Sph("La", V(0.004f, 0.01f, 0.008f), V(0.028f, 0.004f, 0.012f), LeafDark, V(0f, 25f, 0f), Small);
                    return s.parts;
                case "trung":
                    count = 0;
                    return s.parts;
                case "heo":
                    count = 5;
                    s.Box("Lat", V(0f, 0.003f, 0f), V(0.056f, 0.005f, 0.02f), "#F2A0A6")
                     .Box("VienMo", V(0f, 0.003f, -0.011f), V(0.056f, 0.0052f, 0.005f), Cream, default(Vector3), None);
                    return s.parts;
                case "dau_phu":
                    count = 4;
                    s.Box("Mieng", V(0f, 0.012f, 0f), V(0.028f, 0.024f, 0.028f), "#F1EAD8");
                    return s.parts;
                default:
                    {
                        // Vong 2-4: lay khoi dau cua hinh da so che lam mot mieng (muc co ban).
                        count = 5;
                        Part p = Prepared(id).parts[0];
                        p.pos = new Vector3(0f, p.size.y * 0.5f, 0f);
                        p.euler = new Vector3(0f, 0f, p.type == PrimitiveType.Cylinder ? p.euler.z : 0f);
                        s.parts.Add(p);
                        return s.parts;
                    }
            }
        }

        /// <summary>Dung danh sach khoi vao parent, khong tinh ban kinh.</summary>
        public static void BuildParts(Transform parent, List<Part> parts, float scale, System.Func<Color, Color> recolor = null)
        {
            foreach (Part p in parts)
            {
                Color c = recolor != null ? recolor(p.color) : p.color;
                Toon.Prim(p.type, parent, p.name, p.pos * scale, p.size * scale, c, p.outline * scale, p.euler);
            }
        }

        /// <summary>
        /// Dung cac khoi vao parent (goc = mat tren de bac). tint != null thi pha mau (bac 0: xin mau).
        /// Tra ve ban kinh bao cua ca hinh tinh tu tam hop bao (de dat mat ngoai moi khoi).
        /// </summary>
        public static Transform Build(Transform parent, string id, bool prepared, Color? tint, float tintAmount, out float boundRadius)
        {
            List<Part> parts = Parts(id, prepared);
            var root = new GameObject(prepared ? "Hinh_SoChe" : "Hinh").transform;
            root.SetParent(parent, false);

            Bounds b = new Bounds();
            bool first = true;
            foreach (Part p in parts)
            {
                Color c = tint.HasValue ? Color.Lerp(p.color, tint.Value, tintAmount) : p.color;
                Toon.Prim(p.type, root, p.name, p.pos, p.size, c, p.outline, p.euler);
                Vector3 half = HalfExtent(p);
                var pb = new Bounds(p.pos, half * 2f);
                if (first) { b = pb; first = false; }
                else b.Encapsulate(pb);
            }
            // Tam hinh: dat mat tu day; ban kinh = khoang cach xa nhat toi goc hop bao cua tung khoi.
            var center = new GameObject("TamHinh").transform;
            center.SetParent(root, false);
            center.localPosition = b.center;
            float r = 0f;
            foreach (Part p in parts)
                r = Mathf.Max(r, Vector3.Distance(p.pos, b.center) + HalfExtent(p).magnitude);
            boundRadius = r;
            return root;
        }

        /// <summary>Nua hop bao cuc bo (truoc khi xoay) cua mot khoi theo kich thuoc that.</summary>
        static Vector3 HalfExtent(Part p)
        {
            Vector3 s = p.size;
            if (p.type == PrimitiveType.Capsule) s = new Vector3(s.x, s.y * 2f, s.z);
            return s * 0.5f;
        }
    }
}
