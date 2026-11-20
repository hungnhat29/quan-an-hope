using System;
using UnityEngine;

namespace BepNhanPham
{
    /// <summary>
    /// Khung canh ngoai cua quan (Goc 1): o cua vom khoet that vao tuong va con pho 3D nhieu lop phia sau
    /// de nhin qua cua thay chieu sau (thay anh phang door_street.png). Toa do theo khung Goc 1:
    /// x doc tuong, y len, z huong ra ngoai (tuong o z 4,0 - 4,1). Mau Art Bible muc 3; lop cang xa cang nhat (khi quyen).
    /// Cac khoi duoc tao bang ham cua BuildNhaHang truyen vao (Box / Sphere / Cyl / Tube / Mesh).
    /// </summary>
    public static class BuildStreet
    {
        /// <summary>O cua: rong 3 m, canh thang cao 2,4 m, vom ban nguyet ban kinh 1,5 m (dinh 3,9 m).</summary>
        public const float DoorHalfWidth = 1.5f;
        public const float DoorSpring = 2.4f;
        public const float WallInner = 4.0f;
        public const float WallOuter = 4.1f;

        public delegate void BoxFn(string name, Vector3 size, Vector3 pos, Vector3 euler, string hex, string tex, float tx, float ty, float outline, string group);
        public delegate void TubeFn(string name, Vector3[] points, float radius, string hex, float outline, string group);

        /// <summary>Nua be rong lo cua o do cao y (0 neu tren dinh vom).</summary>
        public static float HalfWidthAt(float y)
        {
            if (y < DoorSpring) return DoorHalfWidth;
            float d = y - DoorSpring;
            if (d >= DoorHalfWidth) return 0f;
            return Mathf.Sqrt(DoorHalfWidth * DoorHalfWidth - d * d);
        }

        /// <summary>
        /// Mat trong cua tuong Goc 1 co lo cua vom: chia thanh cac dai ngang 4 cm, moi dai hai mieng hai ben lo.
        /// UV theo vi tri tren ca buc tuong (0..1) de hoa van khop voi ba tuong con lai (cung vat lieu, cung ti le lap).
        /// </summary>
        public static void WallMesh(float width, float height, out Vector3[] verts, out Vector3[] norms, out Vector2[] uvs, out int[] tris)
        {
            const float Row = 0.04f;
            var v = new System.Collections.Generic.List<Vector3>();
            var t = new System.Collections.Generic.List<int>();
            float half = width * 0.5f;
            int rows = Mathf.CeilToInt(height / Row);
            for (int r = 0; r < rows; r++)
            {
                float y0 = r * Row, y1 = Mathf.Min(height, y0 + Row);
                float hw = HalfWidthAt((y0 + y1) * 0.5f);
                if (hw <= 0f) AddQuad(v, t, -half, half, y0, y1);
                else
                {
                    AddQuad(v, t, -half, -hw, y0, y1);
                    AddQuad(v, t, hw, half, y0, y1);
                }
            }
            verts = v.ToArray();
            norms = new Vector3[verts.Length];
            uvs = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                norms[i] = Vector3.back;
                uvs[i] = new Vector2((half - verts[i].x) / width, verts[i].y / height);
            }
            tris = t.ToArray();
        }

        static void AddQuad(System.Collections.Generic.List<Vector3> v, System.Collections.Generic.List<int> t, float x0, float x1, float y0, float y1)
        {
            int i = v.Count;
            v.Add(new Vector3(x0, y0, WallInner));
            v.Add(new Vector3(x1, y0, WallInner));
            v.Add(new Vector3(x0, y1, WallInner));
            v.Add(new Vector3(x1, y1, WallInner));
            // mat huong -z (vao phong)
            t.Add(i); t.Add(i + 2); t.Add(i + 1);
            t.Add(i + 1); t.Add(i + 2); t.Add(i + 3);
        }

        /// <summary>Khung go vom quanh lo cua (che bac thang cua cac dai tuong) + nguong cua.</summary>
        public static void Frame(BoxFn box, TubeFn tube, string group)
        {
            float z = (WallInner + WallOuter) * 0.5f;
            float r = DoorHalfWidth + 0.08f;
            var pts = new System.Collections.Generic.List<Vector3>();
            pts.Add(new Vector3(-r, -0.05f, z));
            pts.Add(new Vector3(-r, DoorSpring * 0.5f, z));
            for (int i = 0; i <= 12; i++)
            {
                float a = Mathf.PI - Mathf.PI * i / 12f;
                pts.Add(new Vector3(Mathf.Cos(a) * r, DoorSpring + Mathf.Sin(a) * r, z));
            }
            pts.Add(new Vector3(r, DoorSpring * 0.5f, z));
            pts.Add(new Vector3(r, -0.05f, z));
            tube("KhungCua", pts.ToArray(), 0.12f, "#B97F52", 0.028f, group);
            box("NguongCua", new Vector3(DoorHalfWidth * 2f + 0.3f, 0.05f, 0.4f), new Vector3(0f, 0.025f, z), Vector3.zero, "#D69B68", "", 1f, 1f, 0.018f, group);
        }

        /// <summary>Con pho: via he + do vat gan, mat duong, day nha ben kia duong, hang cay, nha xa, troi, may.</summary>
        public static void Street(BoxFn box, BoxFn sphere, BoxFn cyl, TubeFn tube, string root)
        {
            string near = root + "/Gan", mid = root + "/Giua", far = root + "/Xa", sky = root + "/Troi";

            // ---- mat dat ----
            box("ViaHe", new Vector3(44f, 0.1f, 3.4f), new Vector3(0f, -0.06f, 5.7f), Vector3.zero, "#E3CFA6", "", 1f, 1f, 0f, near);
            for (int i = -10; i <= 10; i++)
                box("ViaHe_Ron_" + (i + 10), new Vector3(0.03f, 0.012f, 3.4f), new Vector3(i * 1.1f, 0f, 5.7f), Vector3.zero, "#C9B38A", "", 1f, 1f, 0f, near);
            box("BoVia", new Vector3(44f, 0.18f, 0.3f), new Vector3(0f, -0.02f, 7.45f), Vector3.zero, "#C9C1B6", "", 1f, 1f, 0.018f, near);
            box("MatDuong", new Vector3(60f, 0.1f, 8f), new Vector3(0f, -0.14f, 11.6f), Vector3.zero, "#8C8279", "", 1f, 1f, 0f, mid);
            for (int i = -6; i <= 6; i++)
                box("VachDuong_" + (i + 6), new Vector3(1.6f, 0.012f, 0.16f), new Vector3(i * 3.2f, -0.085f, 11.6f), Vector3.zero, "#F2C14E", "", 1f, 1f, 0f, mid);
            box("ViaHeBenKia", new Vector3(60f, 0.1f, 2.6f), new Vector3(0f, -0.06f, 16.9f), Vector3.zero, "#E3CFA6", "", 1f, 1f, 0f, mid);
            box("DatXa", new Vector3(90f, 0.1f, 22f), new Vector3(0f, -0.1f, 29f), Vector3.zero, "#D9C7A2", "", 1f, 1f, 0f, far);

            // ---- do vat gan (ngay truoc cua: cam giac buoc ra la pho) ----
            // bang phan chu A dung canh cua
            box("BangPhan_Mat", new Vector3(0.62f, 0.9f, 0.05f), new Vector3(-1.35f, 0.5f, 4.75f), new Vector3(-12f, 0f, 0f), "#4A3F3A", "", 1f, 1f, 0.018f, near);
            box("BangPhan_Khung", new Vector3(0.7f, 0.98f, 0.03f), new Vector3(-1.35f, 0.5f, 4.78f), new Vector3(-12f, 0f, 0f), "#B97F52", "", 1f, 1f, 0.018f, near);
            box("BangPhan_Chan", new Vector3(0.62f, 0.9f, 0.05f), new Vector3(-1.35f, 0.5f, 5.05f), new Vector3(12f, 0f, 0f), "#B97F52", "", 1f, 1f, 0.018f, near);
            for (int i = 0; i < 3; i++)
                box("BangPhan_Chu_" + i, new Vector3(0.4f - i * 0.08f, 0.035f, 0.01f), new Vector3(-1.35f, 0.72f - i * 0.15f, 4.72f), new Vector3(-12f, 0f, 0f), "#FFF3DC", "", 1f, 1f, 0f, near);
            // chau cay hai ben cua
            for (int s = -1; s <= 1; s += 2)
            {
                string n = s < 0 ? "Trai" : "Phai";
                cyl("ChauCay_" + n, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(s * 2.25f, 0.25f, 4.6f), Vector3.zero, "#E8604F", "", 1f, 1f, 0.028f, near);
                for (int k = 0; k < 3; k++)
                    sphere("LaCay_" + n + "_" + k, new Vector3(0.24f, 0.85f, 0.14f), new Vector3(s * 2.25f + (k - 1) * 0.16f, 0.8f, 4.6f), new Vector3(0f, 0f, (k - 1) * -25f), "#7DB87A", "", 1f, 1f, 0.018f, near);
            }
            // cot den duong ben trai, ghe go ben phai
            cyl("CotDen", new Vector3(0.12f, 3.8f, 0.12f), new Vector3(-3.4f, 1.9f, 6.9f), Vector3.zero, "#4A3F3A", "", 1f, 1f, 0.018f, near);
            tube("CanDen", new[] { new Vector3(-3.4f, 3.7f, 6.9f), new Vector3(-3.05f, 3.95f, 6.9f), new Vector3(-2.7f, 3.85f, 6.9f) }, 0.04f, "#4A3F3A", 0.012f, near);
            sphere("BongDenDuong", new Vector3(0.36f, 0.3f, 0.36f), new Vector3(-2.7f, 3.65f, 6.9f), Vector3.zero, "#FFF3DC", "", 1f, 1f, 0.018f, near);
            box("GheGo_Mat", new Vector3(1.6f, 0.08f, 0.45f), new Vector3(3.3f, 0.48f, 6.6f), Vector3.zero, "#D69B68", "", 1f, 1f, 0.018f, near);
            box("GheGo_Tua", new Vector3(1.6f, 0.4f, 0.06f), new Vector3(3.3f, 0.78f, 6.85f), new Vector3(-8f, 0f, 0f), "#D69B68", "", 1f, 1f, 0.018f, near);
            for (int s = -1; s <= 1; s += 2)
                box("GheGo_Chan_" + (s < 0 ? "Trai" : "Phai"), new Vector3(0.08f, 0.46f, 0.4f), new Vector3(3.3f + s * 0.68f, 0.23f, 6.6f), Vector3.zero, "#B97F52", "", 1f, 1f, 0.018f, near);

            // ---- day den long bang qua duong (Sway luc chay: RoomView) ----
            var cord = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i <= 10; i++)
            {
                float x = -12f + 2.4f * i;
                cord.Add(new Vector3(x, 5.2f - 0.5f * (1f - Mathf.Pow((x / 12f), 2f)), 9.6f));
            }
            tube("DayDenPho", cord.ToArray(), 0.02f, "#2B2321", 0f, mid);
            string[] lantern = { "#E8604F", "#F2C14E", "#E8604F", "#FFF3DC" };
            for (int i = 1; i < 10; i++)
            {
                Vector3 c = cord[i];
                cyl("DayLongPho_" + i, new Vector3(0.02f, 0.3f, 0.02f), c + new Vector3(0f, -0.15f, 0f), Vector3.zero, "#2B2321", "", 1f, 1f, 0f, mid);
                sphere("LongPho_" + i, new Vector3(0.42f, 0.54f, 0.42f), c + new Vector3(0f, -0.55f, 0f), Vector3.zero, lantern[i % lantern.Length], "", 1f, 1f, 0.028f, mid);
            }

            // ---- day nha ben kia duong: nha thap, co hem ngay giua cua va khe giua cac nha de nhin xuyen ra lop xa ----
            string[] walls = { "#E9A0A0", "#9CC7D8", "#F2D7B6", "#8CC9AE", "#C9B3F0", "#F2C14E", "#E9A0A0", "#9CC7D8" };
            float[] heights = { 4.6f, 3.9f, 5.0f, 4.2f, 4.8f, 4.0f, 4.5f, 3.8f };
            const float HouseW = 3.8f, Gap = 1.7f, Front = 18.5f, Depth = 4f;
            for (int i = 0; i < walls.Length; i++)
            {
                int k = i / 2;                       // 0..3 ra xa dan hai ben hem giua
                float side = i % 2 == 0 ? -1f : 1f;
                float x = side * (Gap * 0.5f + HouseW * 0.5f + k * (HouseW + Gap));
                float h = heights[i];
                string n = "Nha_" + i;
                box(n, new Vector3(HouseW, h, Depth), new Vector3(x, h * 0.5f, Front + Depth * 0.5f), Vector3.zero, walls[i], "", 1f, 1f, 0.028f, mid);
                box(n + "_Mai", new Vector3(HouseW + 0.5f, 0.35f, Depth + 0.4f), new Vector3(x, h + 0.17f, Front + Depth * 0.5f), Vector3.zero, "#B97F52", "", 1f, 1f, 0.028f, mid);
                if (h >= 4.4f)
                    for (int w = -1; w <= 1; w += 2)
                    {
                        float y = h - 1.1f;
                        box(n + "_CuaSo_" + (w < 0 ? "T" : "P"), new Vector3(0.8f, 0.9f, 0.08f), new Vector3(x + w * 0.95f, y, Front - 0.03f), Vector3.zero, "#FFF3DC", "", 1f, 1f, 0.018f, mid);
                        box(n + "_RemCua_" + (w < 0 ? "T" : "P"), new Vector3(0.8f, 0.25f, 0.09f), new Vector3(x + w * 0.95f, y + 0.33f, Front - 0.05f), Vector3.zero, walls[(i + 2) % walls.Length], "", 1f, 1f, 0f, mid);
                    }
                box(n + "_CuaDi", new Vector3(1.3f, 2.1f, 0.08f), new Vector3(x, 1.05f, Front - 0.03f), Vector3.zero, "#B97F52", "", 1f, 1f, 0.018f, mid);
                for (int st = 0; st < 5; st++)
                    box(n + "_MaiHien_" + st, new Vector3(HouseW / 5f, 0.06f, 1.1f), new Vector3(x - HouseW * 0.4f + st * HouseW / 5f, 2.5f, Front - 0.5f), new Vector3(18f, 0f, 0f),
                        st % 2 == 0 ? "#E8604F" : "#FFF3DC", "", 1f, 1f, 0f, mid);
            }

            // ---- hang cay sau day nha + nha xa (nhat dan theo khi quyen) ----
            for (int i = 0; i < 9; i++)
            {
                float x = -20f + i * 5f + (i % 2) * 1.3f;
                cyl("Cay_Than_" + i, new Vector3(0.3f, 3f, 0.3f), new Vector3(x, 1.5f, 25f), Vector3.zero, "#A08260", "", 1f, 1f, 0f, far);
                sphere("Cay_Tan_" + i, new Vector3(3.2f, 3.4f, 3.2f), new Vector3(x, 4.6f, 25f), Vector3.zero, "#A9CBA0", "", 1f, 1f, 0f, far);
            }
            string[] farCol = { "#D8CDEB", "#C8DCE3", "#E3D3E6", "#CFDFD6" };
            for (int i = 0; i < 8; i++)
            {
                float x = -24f + i * 6.8f;
                float h = 6.5f + (i * 37 % 5);
                box("NhaXa_" + i, new Vector3(5.6f, h, 3f), new Vector3(x, h * 0.5f, 30f), Vector3.zero, farCol[i % farCol.Length], "", 1f, 1f, 0f, far);
                for (int w = 0; w < 3; w++)
                    box("NhaXa_" + i + "_CuaSo_" + w, new Vector3(0.7f, 0.9f, 0.05f), new Vector3(x - 1.6f + w * 1.6f, h - 2f, 28.47f), Vector3.zero, "#F3EBDD", "", 1f, 1f, 0f, far);
            }

            // ---- troi: dai xanh nhat phia tren, dai am phia duoi (khoi xa nhat), may ----
            box("TroiTren", new Vector3(90f, 30f, 0.2f), new Vector3(0f, 25f, 36f), Vector3.zero, "#CFE4EA", "", 1f, 1f, 0f, sky);
            box("TroiDuoi", new Vector3(90f, 10f, 0.2f), new Vector3(0f, 5f, 35.9f), Vector3.zero, "#F7E3BE", "", 1f, 1f, 0f, sky);
            float[,] clouds = { { -9f, 13f, 4.5f }, { 3f, 15.5f, 6f }, { 12f, 12f, 4f }, { -2f, 10.5f, 3.2f } };
            for (int i = 0; i < clouds.GetLength(0); i++)
            {
                Vector3 c = new Vector3(clouds[i, 0], clouds[i, 1], 34f);
                float w = clouds[i, 2];
                sphere("May_" + i, new Vector3(w, w * 0.32f, 0.5f), c, Vector3.zero, "#FFF8EC", "", 1f, 1f, 0f, sky);
                sphere("May_" + i + "_Bong", new Vector3(w * 0.55f, w * 0.38f, 0.5f), c + new Vector3(-w * 0.12f, w * 0.12f, -0.1f), Vector3.zero, "#FFF8EC", "", 1f, 1f, 0f, sky);
            }
        }
    }
}
