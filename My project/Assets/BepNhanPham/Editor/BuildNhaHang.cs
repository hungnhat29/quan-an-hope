using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace BepNhanPham
{
    /// <summary>
    /// Menu: Bep Nhan Pham > Dung nha hang (4 goc).
    /// Dung ca can phong 4 goc tu khoi co ban + anh trong thu muc Textures,
    /// luu thanh scene va prefab. Chay lai bat cu luc nao de dung lai tu dau.
    /// </summary>
    public static class BuildNhaHang
    {
        const string RootPath = "Assets/BepNhanPham";
        const string GenPath = RootPath + "/Generated";

        static Transform sceneRoot;
        static readonly Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static Material outlineMat;
        static Shader toonShader, alphaShader, outlineShader;
        static GameObject faceObject;
        static int meshCount;

        [MenuItem("Bep Nhan Pham/Dung nha hang (4 goc)")]
        public static void Build()
        {
            toonShader = Shader.Find("BepNhanPham/ToonFlat");
            alphaShader = Shader.Find("BepNhanPham/UnlitAlpha");
            outlineShader = Shader.Find("BepNhanPham/Outline");
            if (toonShader == null || alphaShader == null || outlineShader == null)
            {
                EditorUtility.DisplayDialog("Bep Nhan Pham", "Khong tim thay shader trong Assets/BepNhanPham/Shaders. Hay import lai goi.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (AssetDatabase.IsValidFolder(GenPath)) AssetDatabase.DeleteAsset(GenPath);
            EnsureFolder(GenPath);
            EnsureFolder(RootPath + "/Scenes");
            EnsureFolder(RootPath + "/Prefabs");
            groups.Clear();
            mats.Clear();
            outlineMeshes.Clear();
            outlineMats.Clear();
            faceObject = null;
            meshCount = 0;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            sceneRoot = new GameObject("NhaHang").transform;

            outlineMat = new Material(outlineShader);
            outlineMat.SetColor("_Color", Hex("#2B2321"));
            AssetDatabase.CreateAsset(outlineMat, GenPath + "/M_Outline.mat");

            // Phong rong hon 8 x 8 m (RoomSize, constitution 1.5): moi goc lui ra phia tuong cua no.
            for (int c = 0; c < 4; c++)
            {
                Transform gc = Group("Goc" + (c + 1));
                gc.localEulerAngles = new Vector3(0f, c * 90f, 0f);
                gc.localPosition = Quaternion.Euler(0f, c * 90f, 0f) * new Vector3(0f, 0f, RoomSize.Grow);
            }
            const float W = RoomSize.Width;
            const float Tile = 4f * W / 8f;

            Box("Floor", new Vector3(W, 0.1f, W), new Vector3(0f, -0.05f, 0f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "floor_wood", Tile, Tile, 0f, "Room");
            Box("Ceiling", new Vector3(W, 0.1f, W), new Vector3(0f, 5.05f, 0f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0f, "Room");
            // Tuong Goc 1 co o cua vom khoet that (BuildStreet): nhin qua cua thay con pho 3D phia sau.
            WallWithDoor(W, Tile);
            Box("Baseboard", new Vector3(W - 0.1f, 0.22f, 0.08f), new Vector3(0f, 0.11f, 3.96f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc1/Wall");
            Box("Rail", new Vector3(W - 0.1f, 0.1f, 0.06f), new Vector3(0f, 4.85f, 3.97f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.018f, "Goc1/Wall");
            Box("CornerPost", new Vector3(0.16f, 5f, 0.16f), new Vector3(-3.92f - RoomSize.Grow, 2.5f, 3.92f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc1/Wall");
            Box("WallBack", new Vector3(W, 5f, 0.1f), new Vector3(0f, 2.5f, 4.05f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "wall_pattern", Tile, 2.5f, 0f, "Goc2/Wall");
            Box("Baseboard", new Vector3(W - 0.1f, 0.22f, 0.08f), new Vector3(0f, 0.11f, 3.96f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc2/Wall");
            Box("Rail", new Vector3(W - 0.1f, 0.1f, 0.06f), new Vector3(0f, 4.85f, 3.97f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.018f, "Goc2/Wall");
            Box("CornerPost", new Vector3(0.16f, 5f, 0.16f), new Vector3(-3.92f - RoomSize.Grow, 2.5f, 3.92f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc2/Wall");
            Box("WallBack", new Vector3(W, 5f, 0.1f), new Vector3(0f, 2.5f, 4.05f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "wall_pattern", Tile, 2.5f, 0f, "Goc3/Wall");
            Box("Baseboard", new Vector3(W - 0.1f, 0.22f, 0.08f), new Vector3(0f, 0.11f, 3.96f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc3/Wall");
            Box("Rail", new Vector3(W - 0.1f, 0.1f, 0.06f), new Vector3(0f, 4.85f, 3.97f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.018f, "Goc3/Wall");
            Box("CornerPost", new Vector3(0.16f, 5f, 0.16f), new Vector3(-3.92f - RoomSize.Grow, 2.5f, 3.92f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc3/Wall");
            Box("WallBack", new Vector3(W, 5f, 0.1f), new Vector3(0f, 2.5f, 4.05f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "wall_pattern", Tile, 2.5f, 0f, "Goc4/Wall");
            Box("Baseboard", new Vector3(W - 0.1f, 0.22f, 0.08f), new Vector3(0f, 0.11f, 3.96f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc4/Wall");
            Box("Rail", new Vector3(W - 0.1f, 0.1f, 0.06f), new Vector3(0f, 4.85f, 3.97f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.018f, "Goc4/Wall");
            Box("CornerPost", new Vector3(0.16f, 5f, 0.16f), new Vector3(-3.92f - RoomSize.Grow, 2.5f, 3.92f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc4/Wall");
            // Cua: khung go vom + con pho nhieu lop thay anh phang door_street.png (yeu cau 18/11: them do sau ngoai cua).
            BuildStreet.Frame(Box, Tube, "Goc1/Entrance");
            BuildStreet.Street(Box, Sphere, Cyl, Tube, "Goc1/Street");
            Quad("Noren", 3.1f, 1.137f, new Vector3(0f, 3.22f, 3.72f), new Vector3(-0f, -0f, 0f), "noren", "Goc1/Entrance", true);
            // Bien ten quan: chu TextMeshProUGUI dung luc chay (RoomView.BuildShopName), khong dung anh chu (constitution Dieu 6, F-055).
            Sphere("Bulb0", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(-2.4f, 4.02f, 3.85f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb1", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(-1.8f, 4.08f, 3.85f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb2", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(-1.2f, 4.02f, 3.85f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb3", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(-0.6f, 4.08f, 3.85f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb4", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(0f, 4.02f, 3.85f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb5", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(0.6f, 4.08f, 3.85f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb6", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(1.2f, 4.02f, 3.85f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb7", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(1.8f, 4.08f, 3.85f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Sphere("Bulb8", new Vector3(0.17f, 0.17f, 0.17f), new Vector3(2.4f, 4.02f, 3.85f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc1/Entrance");
            Cyl("LanternCord0", new Vector3(0.03f, 0.5f, 0.03f), new Vector3(-2.1f, 3.75f, 3.7f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Entrance");
            Sphere("Lantern0", new Vector3(0.52f, 0.66f, 0.52f), new Vector3(-2.1f, 3.2f, 3.7f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc1/Entrance");
            Cyl("LanternTop0", new Vector3(0.26f, 0.08f, 0.26f), new Vector3(-2.1f, 3.53f, 3.7f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Entrance");
            Cyl("LanternBottom0", new Vector3(0.22f, 0.08f, 0.22f), new Vector3(-2.1f, 2.87f, 3.7f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Entrance");
            Cyl("LanternCord1", new Vector3(0.03f, 0.5f, 0.03f), new Vector3(2.1f, 3.75f, 3.7f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Entrance");
            Sphere("Lantern1", new Vector3(0.52f, 0.66f, 0.52f), new Vector3(2.1f, 3.2f, 3.7f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc1/Entrance");
            Cyl("LanternTop1", new Vector3(0.26f, 0.08f, 0.26f), new Vector3(2.1f, 3.53f, 3.7f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Entrance");
            Cyl("LanternBottom1", new Vector3(0.22f, 0.08f, 0.22f), new Vector3(2.1f, 2.87f, 3.7f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Entrance");
            Sphere("StandSignBody", new Vector3(0.95f, 2.1f, 0.5f), new Vector3(3.25f, 1.05f, 3.35f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.028f, "Goc1/Decor");
            Sphere("StandSignBulb", new Vector3(0.2f, 0.2f, 0.2f), new Vector3(3.25f, 2.2f, 3.35f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.018f, "Goc1/Decor");
            Cyl("RoundFrame", new Vector3(0.92f, 0.06f, 0.92f), new Vector3(-3.3f, 2.95f, 3.96f), new Vector3(-90f, -0f, 0f), "#B97F52", "", 1f, 1f, 0.028f, "Goc1/Decor");
            Cyl("RoundMat", new Vector3(0.72f, 0.07f, 0.72f), new Vector3(-3.3f, 2.95f, 3.95f), new Vector3(-90f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0f, "Goc1/Decor");
            Quad("FrameHeart", 0.36f, 0.36f, new Vector3(-3.3f, 2.95f, 3.9f), new Vector3(-0f, -0f, 0f), "heart_red", "Goc1/Decor", false);
            Cyl("BigPot", new Vector3(0.6f, 0.6f, 0.6f), new Vector3(-3.3f, 0.3f, 3.4f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc1/Decor");
            Sphere("BigLeaf0", new Vector3(0.26f, 1f, 0.14f), new Vector3(-3.3f, 1.05f, 3.4f), new Vector3(-0f, -0f, 0f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc1/Decor");
            Sphere("BigLeaf1", new Vector3(0.26f, 1f, 0.14f), new Vector3(-3.52f, 0.962f, 3.4f), new Vector3(-0f, -0f, 28.65f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc1/Decor");
            Sphere("BigLeaf2", new Vector3(0.26f, 1f, 0.14f), new Vector3(-3.08f, 0.962f, 3.4f), new Vector3(-0f, -0f, -28.65f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc1/Decor");
            Cyl("LampCord0", new Vector3(0.03f, 1f, 0.03f), new Vector3(-2.9f, 4.5f, 2.6f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Decor");
            Sphere("LampShade0", new Vector3(0.62f, 0.4f, 0.62f), new Vector3(-2.9f, 3.92f, 2.6f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.028f, "Goc1/Decor");
            Sphere("LampBulb0", new Vector3(0.2f, 0.2f, 0.2f), new Vector3(-2.9f, 3.74f, 2.6f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc1/Decor");
            Cyl("LampCord1", new Vector3(0.03f, 1f, 0.03f), new Vector3(2.9f, 4.5f, 2.6f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc1/Decor");
            Sphere("LampShade1", new Vector3(0.62f, 0.4f, 0.62f), new Vector3(2.9f, 3.92f, 2.6f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.028f, "Goc1/Decor");
            Sphere("LampBulb1", new Vector3(0.2f, 0.2f, 0.2f), new Vector3(2.9f, 3.74f, 2.6f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc1/Decor");
            Sphere("Body", new Vector3(1.15f, 1.3f, 1f), new Vector3(0f, 1.2f, 3.25f), new Vector3(-0f, -0f, 0f), "#8CC9AE", "", 1f, 1f, 0.028f, "Goc1/Customer");
            Sphere("Brooch", new Vector3(0.16f, 0.16f, 0.1f), new Vector3(0f, 1.5f, 2.79f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.018f, "Goc1/Customer");
            Tube("ArmWave", new Vector3[] { new Vector3(-0.5f, 1.55f, 3.2f), new Vector3(-0.9f, 1.7f, 3.1f), new Vector3(-0.98f, 2.2f, 3f) }, 0.09f, "#8CC9AE", 0.028f, "Goc1/Customer");
            Sphere("HandWave", new Vector3(0.36f, 0.36f, 0.36f), new Vector3(-0.98f, 2.24f, 3f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "", 1f, 1f, 0.028f, "Goc1/Customer");
            Tube("ArmRest", new Vector3[] { new Vector3(0.5f, 1.5f, 3.2f), new Vector3(0.82f, 1.42f, 2.85f), new Vector3(0.74f, 1.38f, 2.5f) }, 0.09f, "#8CC9AE", 0.028f, "Goc1/Customer");
            Sphere("HandRest", new Vector3(0.36f, 0.3f, 0.36f), new Vector3(0.74f, 1.38f, 2.46f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "", 1f, 1f, 0.028f, "Goc1/Customer");
            Sphere("Bun", new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 2.98f, 3.35f), new Vector3(-0f, -0f, 0f), "#EFE7DA", "", 1f, 1f, 0.028f, "Goc1/Customer");
            Cyl("Hairpin", new Vector3(0.05f, 0.62f, 0.05f), new Vector3(0.2f, 3.1f, 3.35f), new Vector3(-0f, -0f, -51.57f), "#B97F52", "", 1f, 1f, 0.018f, "Goc1/Customer");
            Sphere("HairpinBead", new Vector3(0.13f, 0.13f, 0.13f), new Vector3(0.45f, 3.3f, 3.35f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.018f, "Goc1/Customer");
            Sphere("Head", new Vector3(1.3f, 1.2f, 1.2f), new Vector3(0f, 2.2f, 3.25f), new Vector3(-0f, -0f, 0f), "#FBE0C3", "", 1f, 1f, 0.028f, "Goc1/Customer");
            Sphere("Hair", new Vector3(1.36f, 0.8f, 1.26f), new Vector3(0f, 2.42f, 3.3f), new Vector3(-0f, -0f, 0f), "#EFE7DA", "", 1f, 1f, 0.028f, "Goc1/Customer");
            Cap("Face", new Vector3(1.33f, 1.23f, 1.23f), new Vector3(0f, 2.2f, 3.25f), "face_happy", "Goc1/Customer");
            Quad("HeartBig", 0.5f, 0.5f, new Vector3(1f, 3.05f, 3.1f), new Vector3(-0f, -0f, 0f), "heart_red", "Goc1/FX", true);
            Quad("HeartSmall", 0.3f, 0.3f, new Vector3(1.36f, 3.42f, 3.1f), new Vector3(-0f, -0f, 0f), "heart_red", "Goc1/FX", true);
            Quad("Steam0", 0.16f, 0.16f, new Vector3(-0.98f, 1.8f, 2.2f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc1/FX", true);
            Quad("Steam1", 0.12f, 0.12f, new Vector3(-0.8f, 2.02f, 2.2f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc1/FX", true);
            Box("CounterBody", new Vector3(6.4f, 1.1f, 0.8f), new Vector3(0f, 0.55f, 2.3f), new Vector3(-0f, -0f, 0f), "#B97F52", "", 1f, 1f, 0.028f, "Goc1/Counter");
            Box("CounterTop", new Vector3(6.6f, 0.12f, 1f), new Vector3(0f, 1.16f, 2.3f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc1/Counter");
            Box("Panel0", new Vector3(1f, 0.5f, 0.05f), new Vector3(-2.0f, 0.5f, 1.88f), new Vector3(-0f, -0f, 0f), "#E9A0A0", "", 1f, 1f, 0.028f, "Goc1/Counter");
            Quad("PanelHeart0", 0.28f, 0.28f, new Vector3(-2.0f, 0.5f, 1.84f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc1/Counter", false);
            Box("Panel1", new Vector3(1f, 0.5f, 0.05f), new Vector3(2.0f, 0.5f, 1.88f), new Vector3(-0f, -0f, 0f), "#E9A0A0", "", 1f, 1f, 0.028f, "Goc1/Counter");
            Quad("PanelHeart1", 0.28f, 0.28f, new Vector3(2.0f, 0.5f, 1.84f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc1/Counter", false);
            Cyl("Pot", new Vector3(0.3f, 0.28f, 0.3f), new Vector3(-3.05f, 1.36f, 2.3f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc1/Props");
            Sphere("Leaf0", new Vector3(0.14f, 0.5f, 0.08f), new Vector3(-3.05f, 1.74f, 2.3f), new Vector3(-0f, -0f, 0f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc1/Props");
            Sphere("Leaf1", new Vector3(0.14f, 0.5f, 0.08f), new Vector3(-3.17f, 1.704f, 2.3f), new Vector3(-0f, -0f, 28.65f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc1/Props");
            Sphere("Leaf2", new Vector3(0.14f, 0.5f, 0.08f), new Vector3(-2.93f, 1.704f, 2.3f), new Vector3(-0f, -0f, -28.65f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc1/Props");
            Cyl("TipJar", new Vector3(0.26f, 0.34f, 0.26f), new Vector3(3.05f, 1.39f, 2.3f), new Vector3(-0f, -0f, 0f), "#9CC7D8", "", 1f, 1f, 0.028f, "Goc1/Props");
            Sphere("Coin", new Vector3(0.14f, 0.04f, 0.14f), new Vector3(3.05f, 1.57f, 2.3f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc1/Props");
            Tube("PortalRing", new Vector3[] { new Vector3(-1.73f, 2.95f, 3.15f), new Vector3(-1.777f, 2.95f, 2.913f), new Vector3(-1.912f, 2.95f, 2.712f), new Vector3(-2.113f, 2.95f, 2.577f), new Vector3(-2.35f, 2.95f, 2.53f), new Vector3(-2.587f, 2.95f, 2.577f), new Vector3(-2.788f, 2.95f, 2.712f), new Vector3(-2.923f, 2.95f, 2.913f), new Vector3(-2.97f, 2.95f, 3.15f), new Vector3(-2.923f, 2.95f, 3.387f), new Vector3(-2.788f, 2.95f, 3.588f), new Vector3(-2.587f, 2.95f, 3.723f), new Vector3(-2.35f, 2.95f, 3.77f), new Vector3(-2.113f, 2.95f, 3.723f), new Vector3(-1.912f, 2.95f, 3.588f), new Vector3(-1.777f, 2.95f, 3.387f), new Vector3(-1.73f, 2.95f, 3.15f) }, 0.07f, "#C9B3F0", 0.028f, "Goc2/DropMachine");
            Quad("PortalSwirl", 1.3f, 1.3f, new Vector3(-2.35f, 2.95f, 3.15f), new Vector3(-90f, -0f, 0f), "portal", "Goc2/DropMachine", false);
            Quad("PortalGlow", 1.5f, 1.5f, new Vector3(-2.35f, 2.95f, 3.65f), new Vector3(-0f, -0f, 0f), "glow", "Goc2/DropMachine", false);
            Box("Table", new Vector3(3.9f, 0.8f, 1.5f), new Vector3(-2.35f, 0.4f, 3.15f), new Vector3(-0f, -0f, 0f), "#B97F52", "", 1f, 1f, 0.028f, "Goc2/DropMachine");
            Box("Pad", new Vector3(3.8f, 0.1f, 1.3f), new Vector3(-2.35f, 0.85f, 3.15f), new Vector3(-0f, -0f, 0f), "#9CC7D8", "", 1f, 1f, 0.028f, "Goc2/DropMachine");
            Box("PadInner", new Vector3(3.5f, 0.11f, 1f), new Vector3(-2.35f, 0.86f, 3.15f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0f, "Goc2/DropMachine");
            Cyl("KioskPost", new Vector3(0.16f, 1.2f, 0.16f), new Vector3(-0.1f, 0.6f, 2.8f), new Vector3(-0f, -0f, 0f), "#B97F52", "", 1f, 1f, 0.028f, "Goc2/DropMachine");
            Sphere("KioskHead", new Vector3(0.62f, 0.56f, 0.5f), new Vector3(-0.1f, 1.4f, 2.8f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc2/DropMachine");
            Quad("KioskFace", 0.4f, 0.25f, new Vector3(-0.1f, 1.5f, 2.54f), new Vector3(-0f, -0f, 0f), "face_happy", "Goc2/DropMachine", false);
            Quad("KioskSlot", 0.34f, 0.1962f, new Vector3(-0.1f, 1.26f, 2.55f), new Vector3(-0f, -0f, 0f), "ticket_slot", "Goc2/DropMachine", false);
            Box("Body", new Vector3(1.5f, 2.3f, 0.95f), new Vector3(1.6f, 1.15f, 3.2f), new Vector3(-0f, -0f, 0f), "#9CC7D8", "", 1f, 1f, 0.028f, "Goc2/UpgradeMachine");
            Box("FacePlate", new Vector3(1.1f, 0.3f, 0.05f), new Vector3(1.6f, 2.02f, 2.71f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.018f, "Goc2/UpgradeMachine");
            Quad("MachineFace", 0.44f, 0.275f, new Vector3(1.6f, 2.02f, 2.67f), new Vector3(-0f, -0f, 0f), "face_happy", "Goc2/UpgradeMachine", false);
            Cyl("Window", new Vector3(1f, 0.08f, 1f), new Vector3(1.6f, 1.3f, 2.72f), new Vector3(-90f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.028f, "Goc2/UpgradeMachine");
            Cyl("WindowInner", new Vector3(0.84f, 0.09f, 0.84f), new Vector3(1.6f, 1.3f, 2.71f), new Vector3(-90f, -0f, 0f), "#EAF4F7", "", 1f, 1f, 0f, "Goc2/UpgradeMachine");
            Quad("DishInside", 0.66f, 0.66f, new Vector3(1.6f, 1.3f, 2.65f), new Vector3(-0f, -0f, 0f), "dish_icon", "Goc2/UpgradeMachine", true);
            Cyl("Lever", new Vector3(0.1f, 0.8f, 0.1f), new Vector3(2.48f, 1.7f, 3.2f), new Vector3(-0f, -0f, 0f), "#C9C1B6", "", 1f, 1f, 0.028f, "Goc2/UpgradeMachine");
            Sphere("LeverBall", new Vector3(0.26f, 0.26f, 0.26f), new Vector3(2.48f, 2.15f, 3.2f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc2/UpgradeMachine");
            Sphere("TopLight", new Vector3(0.3f, 0.3f, 0.3f), new Vector3(1.6f, 2.4f, 3.2f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.028f, "Goc2/UpgradeMachine");
            // Nhan may: RoomView.BienMayGacha (TextMeshPro, F-055).
            Cyl("Pedestal0", new Vector3(0.95f, 1.5f, 0.8f), new Vector3(-1.25f, 0.75f, 3.2f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("PedestalTop0", new Vector3(1.08f, 0.1f, 0.95f), new Vector3(-1.25f, 1.55f, 3.2f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("Pedestal1", new Vector3(0.95f, 1.5f, 0.8f), new Vector3(0f, 0.75f, 3.2f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("PedestalTop1", new Vector3(1.08f, 0.1f, 0.95f), new Vector3(0f, 1.55f, 3.2f), new Vector3(-0f, -0f, 0f), "#9CC7D8", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("Pedestal2", new Vector3(0.95f, 1.5f, 0.8f), new Vector3(1.25f, 0.75f, 3.2f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("PedestalTop2", new Vector3(1.08f, 0.1f, 0.95f), new Vector3(1.25f, 1.55f, 3.2f), new Vector3(-0f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("SaltJar", new Vector3(0.3f, 0.36f, 0.3f), new Vector3(-1.25f, 1.78f, 3.2f), new Vector3(-0f, -0f, 0f), "#E9A0A0", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("SaltLid", new Vector3(0.33f, 0.09f, 0.33f), new Vector3(-1.25f, 2f, 3.2f), new Vector3(-0f, -0f, 0f), "#FFFFFF", "", 1f, 1f, 0.018f, "Goc3/RelicShop");
            Quad("SaltFace", 0.2f, 0.125f, new Vector3(-1.25f, 1.78f, 3.04f), new Vector3(-0f, -0f, 0f), "face_happy", "Goc3/RelicShop", false);
            Cyl("Pan", new Vector3(0.62f, 0.12f, 0.62f), new Vector3(0f, 1.84f, 3.2f), new Vector3(-51.57f, -0f, 0f), "#5A5250", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("PanInner", new Vector3(0.5f, 0.13f, 0.5f), new Vector3(0f, 1.85f, 3.19f), new Vector3(-51.57f, -0f, 0f), "#6E6462", "", 1f, 1f, 0f, "Goc3/RelicShop");
            Cyl("PanHandle", new Vector3(0.09f, 0.5f, 0.09f), new Vector3(0.44f, 1.84f, 3.2f), new Vector3(-0f, -0f, 90f), "#5A5250", "", 1f, 1f, 0.018f, "Goc3/RelicShop");
            Sphere("Chili", new Vector3(0.26f, 0.62f, 0.26f), new Vector3(1.25f, 1.96f, 3.2f), new Vector3(-0f, -0f, 28.65f), "#D9382C", "", 1f, 1f, 0.028f, "Goc3/RelicShop");
            Cyl("ChiliStem", new Vector3(0.06f, 0.16f, 0.06f), new Vector3(1.07f, 2.26f, 3.2f), new Vector3(-0f, -0f, 28.65f), "#7DB87A", "", 1f, 1f, 0.018f, "Goc3/RelicShop");
            Quad("ChiliFace", 0.2f, 0.125f, new Vector3(1.27f, 1.94f, 3.06f), new Vector3(-0f, -0f, 0f), "face_angry", "Goc3/RelicShop", false);
            Sphere("Hood", new Vector3(2.7f, 1.1f, 1.5f), new Vector3(0f, 4.25f, 3.45f), new Vector3(-0f, -0f, 0f), "#9CC7D8", "", 1f, 1f, 0.028f, "Goc4/Stove");
            Cyl("HoodPipe", new Vector3(0.5f, 0.9f, 0.5f), new Vector3(0f, 4.75f, 3.6f), new Vector3(-0f, -0f, 0f), "#9CC7D8", "", 1f, 1f, 0.028f, "Goc4/Stove");
            Box("StoveBody", new Vector3(2.6f, 1f, 1f), new Vector3(0f, 0.5f, 3.3f), new Vector3(-0f, -0f, 0f), "#FFF3DC", "", 1f, 1f, 0.028f, "Goc4/Stove");
            Box("StoveTop", new Vector3(2.7f, 0.1f, 1.05f), new Vector3(0f, 1.05f, 3.3f), new Vector3(-0f, -0f, 0f), "#C9C1B6", "", 1f, 1f, 0.028f, "Goc4/Stove");
            Box("OvenDoor", new Vector3(2f, 0.55f, 0.04f), new Vector3(0f, 0.45f, 2.79f), new Vector3(-0f, -0f, 0f), "#E8DFD3", "", 1f, 1f, 0.018f, "Goc4/Stove");
            Cyl("Knob0", new Vector3(0.15f, 0.06f, 0.15f), new Vector3(-1f, 0.86f, 2.77f), new Vector3(-90f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.018f, "Goc4/Stove");
            Cyl("Knob1", new Vector3(0.15f, 0.06f, 0.15f), new Vector3(1f, 0.86f, 2.77f), new Vector3(-90f, -0f, 0f), "#F2C14E", "", 1f, 1f, 0.018f, "Goc4/Stove");
            Cyl("PotBody", new Vector3(1.2f, 0.62f, 1.2f), new Vector3(0f, 1.42f, 3.3f), new Vector3(-0f, -0f, 0f), "#E8604F", "", 1f, 1f, 0.028f, "Goc4/Pot");
            Tube("PotRim", new Vector3[] { new Vector3(0.6f, 1.74f, 3.3f), new Vector3(0.554f, 1.74f, 3.07f), new Vector3(0.424f, 1.74f, 2.876f), new Vector3(0.23f, 1.74f, 2.746f), new Vector3(0f, 1.74f, 2.7f), new Vector3(-0.23f, 1.74f, 2.746f), new Vector3(-0.424f, 1.74f, 2.876f), new Vector3(-0.554f, 1.74f, 3.07f), new Vector3(-0.6f, 1.74f, 3.3f), new Vector3(-0.554f, 1.74f, 3.53f), new Vector3(-0.424f, 1.74f, 3.724f), new Vector3(-0.23f, 1.74f, 3.854f), new Vector3(-0f, 1.74f, 3.9f), new Vector3(0.23f, 1.74f, 3.854f), new Vector3(0.424f, 1.74f, 3.724f), new Vector3(0.554f, 1.74f, 3.53f), new Vector3(0.6f, 1.74f, 3.3f) }, 0.05f, "#C94A3B", 0.018f, "Goc4/Pot");
            Cyl("Soup", new Vector3(1.1f, 0.03f, 1.1f), new Vector3(0f, 1.73f, 3.3f), new Vector3(-0f, -0f, 0f), "#F08A6A", "", 1f, 1f, 0f, "Goc4/Pot");
            Sphere("PotHandle0", new Vector3(0.3f, 0.12f, 0.16f), new Vector3(-0.74f, 1.6f, 3.3f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc4/Pot");
            Sphere("PotHandle1", new Vector3(0.3f, 0.12f, 0.16f), new Vector3(0.74f, 1.6f, 3.3f), new Vector3(-0f, -0f, 0f), "#2B2321", "", 1f, 1f, 0f, "Goc4/Pot");
            Quad("PotFace", 0.62f, 0.3875f, new Vector3(0f, 1.4f, 2.69f), new Vector3(-0f, -0f, 0f), "face_happy", "Goc4/Pot", false);
            Quad("SteamHeart0", 0.22f, 0.22f, new Vector3(-0.3f, 2.1f, 3.2f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc4/Pot", true);
            Quad("SteamHeart1", 0.28f, 0.28f, new Vector3(0.1f, 2.5f, 3.2f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc4/Pot", true);
            Quad("SteamHeart2", 0.2f, 0.2f, new Vector3(0.35f, 2.95f, 3.2f), new Vector3(-0f, -0f, 0f), "heart_white", "Goc4/Pot", true);
            Box("CounterL", new Vector3(2.2f, 1f, 1f), new Vector3(-2.85f, 0.5f, 3.3f), new Vector3(-0f, -0f, 0f), "#B97F52", "", 1f, 1f, 0.028f, "Goc4/PrepLeft");
            Box("CounterLTop", new Vector3(2.3f, 0.1f, 1.08f), new Vector3(-2.85f, 1.05f, 3.3f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc4/PrepLeft");
            Box("CuttingBoard", new Vector3(1f, 0.06f, 0.6f), new Vector3(-2.75f, 1.13f, 3.1f), new Vector3(-0f, -0f, 0f), "#F2D7B6", "", 1f, 1f, 0.018f, "Goc4/PrepLeft");
            Box("CounterR", new Vector3(2.2f, 1f, 1f), new Vector3(2.85f, 0.5f, 3.3f), new Vector3(-0f, -0f, 0f), "#B97F52", "", 1f, 1f, 0.028f, "Goc4/PrepRight");
            Box("CounterRTop", new Vector3(2.3f, 0.1f, 1.08f), new Vector3(2.85f, 1.05f, 3.3f), new Vector3(-0f, -0f, 0f), "#D69B68", "", 1f, 1f, 0.028f, "Goc4/PrepRight");

            SetupCustomer();
            SetupPlayer();
            SetupLight();
            SetupPostFx();

            PrefabUtility.SaveAsPrefabAssetAndConnect(sceneRoot.gameObject, RootPath + "/Prefabs/NhaHang.prefab", InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(scene, RootPath + "/Scenes/NhaHang.unity");
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = sceneRoot.gameObject;
            Debug.Log("Bep Nhan Pham: da dung xong nha hang 4 goc. Scene: " + RootPath + "/Scenes/NhaHang.unity");
        }

        // ---------- cac khoi ----------

        static void Box(string name, Vector3 size, Vector3 pos, Vector3 euler, string hex, string tex, float tx, float ty, float outline, string group)
        {
            Prim(PrimitiveType.Cube, name, size, size, pos, euler, hex, tex, tx, ty, outline, group);
        }

        static void Sphere(string name, Vector3 size, Vector3 pos, Vector3 euler, string hex, string tex, float tx, float ty, float outline, string group)
        {
            Prim(PrimitiveType.Sphere, name, size, size, pos, euler, hex, tex, tx, ty, outline, group);
        }

        // size = (duong kinh X, chieu cao, duong kinh Z). Cylinder cua Unity cao 2 don vi nen scale Y chia doi.
        static void Cyl(string name, Vector3 size, Vector3 pos, Vector3 euler, string hex, string tex, float tx, float ty, float outline, string group)
        {
            Prim(PrimitiveType.Cylinder, name, size, new Vector3(size.x, size.y * 0.5f, size.z), pos, euler, hex, tex, tx, ty, outline, group);
        }

        static GameObject Prim(PrimitiveType type, string name, Vector3 size, Vector3 scale, Vector3 pos, Vector3 euler, string hex, string tex, float tx, float ty, float outline, string group)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            StripCollider(go);
            go.transform.SetParent(Group(group), false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = scale;
            SetRenderer(go.GetComponent<MeshRenderer>(), GetMat(false, hex, tex, tx, ty));
            if (outline > 0f) AddOutline(go, go.GetComponent<MeshFilter>().sharedMesh, size, outline);
            return go;
        }

        static void Quad(string name, float w, float h, Vector3 pos, Vector3 euler, string tex, string group, bool bob)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            StripCollider(go);
            go.transform.SetParent(Group(group), false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = new Vector3(w, h, 1f);
            SetRenderer(go.GetComponent<MeshRenderer>(), GetMat(true, "#FFFFFF", tex, 1f, 1f));
            if (bob)
            {
                var b = go.AddComponent<Bob>();
                b.phase = pos.x * 3f + pos.y;
            }
        }

        static void WallWithDoor(float width, float tile)
        {
            Vector3[] v, n;
            Vector2[] uv;
            int[] t;
            BuildStreet.WallMesh(width, 5f, out v, out n, out uv, out t);
            var go = new GameObject("WallBack");
            go.transform.SetParent(Group("Goc1/Wall"), false);
            go.AddComponent<MeshFilter>().sharedMesh = SaveMesh(v, n, uv, t, "TuongCua");
            SetRenderer(go.AddComponent<MeshRenderer>(), GetMat(false, "#FFFFFF", "wall_pattern", tile, 2.5f));
        }

        static void Tube(string name, Vector3[] points, float radius, string hex, float outline, string group)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Group(group), false);
            go.AddComponent<MeshFilter>().sharedMesh = BuildTube(points, radius, name);
            SetRenderer(go.AddComponent<MeshRenderer>(), GetMat(false, hex, "", 1f, 1f));
            if (outline > 0f)
            {
                Mesh tm = go.GetComponent<MeshFilter>().sharedMesh;
                AddOutline(go, tm, tm.bounds.size, outline);
            }
        }

        // Vien muc (sprint 9 B-03): ban sao phap tuyen lam muot (luu thanh asset) + shader OutlinePx, do day theo diem anh.
        static readonly Dictionary<Mesh, Mesh> outlineMeshes = new Dictionary<Mesh, Mesh>();
        static readonly Dictionary<float, Material> outlineMats = new Dictionary<float, Material>();

        static void AddOutline(GameObject go, Mesh src, Vector3 size, float outline)
        {
            if (src == null || !OutlineMesh.Wants(size, outline)) return;
            Mesh sm;
            if (!outlineMeshes.TryGetValue(src, out sm))
            {
                sm = OutlineMesh.Build(src);
                AssetDatabase.CreateAsset(sm, GenPath + "/Vien_" + (meshCount++) + "_" + src.name + ".asset");
                outlineMeshes[src] = sm;
            }
            Material m;
            if (!outlineMats.TryGetValue(outline, out m))
            {
                m = OutlineMesh.NewMaterial(outline);
                AssetDatabase.CreateAsset(m, GenPath + "/" + m.name + ".mat");
                outlineMats[outline] = m;
            }
            var o = new GameObject("Outline");
            o.transform.SetParent(go.transform, false);
            o.AddComponent<MeshFilter>().sharedMesh = sm;
            SetRenderer(o.AddComponent<MeshRenderer>(), m);
        }

        // Mieng chom cau om lay mat truoc cua dau, de dan anh khuon mat 2D len.
        static void Cap(string name, Vector3 size, Vector3 pos, string tex, string group)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Group(group), false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.AddComponent<MeshFilter>().sharedMesh = BuildCap(name);
            SetRenderer(go.AddComponent<MeshRenderer>(), GetMat(true, "#FFFFFF", tex, 1f, 1f));
            faceObject = go;
        }

        // ---------- mesh tu sinh ----------

        static Mesh BuildTube(Vector3[] p, float r, string assetName)
        {
            const int steps = 8;
            const int sides = 12;
            var centers = new List<Vector3>();
            for (int i = 0; i < p.Length - 1; i++)
            {
                Vector3 p0 = p[Mathf.Max(i - 1, 0)];
                Vector3 p1 = p[i];
                Vector3 p2 = p[i + 1];
                Vector3 p3 = p[Mathf.Min(i + 2, p.Length - 1)];
                for (int s = 0; s < steps; s++) centers.Add(CatmullRom(p0, p1, p2, p3, s / (float)steps));
            }
            centers.Add(p[p.Length - 1]);

            int n = centers.Count;
            var verts = new Vector3[n * sides];
            var norms = new Vector3[n * sides];
            var uvs = new Vector2[n * sides];
            Vector3 normal = Vector3.up;
            for (int i = 0; i < n; i++)
            {
                Vector3 tan = (centers[Mathf.Min(i + 1, n - 1)] - centers[Mathf.Max(i - 1, 0)]).normalized;
                if (i == 0)
                {
                    normal = Vector3.Cross(tan, Vector3.up);
                    if (normal.sqrMagnitude < 0.0001f) normal = Vector3.Cross(tan, Vector3.right);
                }
                normal = normal - tan * Vector3.Dot(normal, tan);
                if (normal.sqrMagnitude < 0.000001f) normal = Vector3.Cross(tan, Vector3.right);
                normal.Normalize();
                Vector3 bin = Vector3.Cross(tan, normal);
                for (int k = 0; k < sides; k++)
                {
                    float a = k / (float)sides * Mathf.PI * 2f;
                    Vector3 d = normal * Mathf.Cos(a) + bin * Mathf.Sin(a);
                    verts[i * sides + k] = centers[i] + d * r;
                    norms[i * sides + k] = d;
                    uvs[i * sides + k] = new Vector2(k / (float)sides, i / (float)(n - 1));
                }
            }
            var tris = new int[(n - 1) * sides * 6];
            int t = 0;
            for (int i = 0; i < n - 1; i++)
            {
                for (int k = 0; k < sides; k++)
                {
                    int a = i * sides + k;
                    int b = i * sides + (k + 1) % sides;
                    int c = a + sides;
                    int d = b + sides;
                    tris[t++] = a; tris[t++] = c; tris[t++] = b;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }
            }
            return SaveMesh(verts, norms, uvs, tris, assetName);
        }

        static Mesh BuildCap(string assetName)
        {
            const int N = 16;
            float az = 50f * Mathf.Deg2Rad;
            float el0 = -48f * Mathf.Deg2Rad;
            float el1 = 24f * Mathf.Deg2Rad;
            var verts = new Vector3[(N + 1) * (N + 1)];
            var norms = new Vector3[verts.Length];
            var uvs = new Vector2[verts.Length];
            for (int j = 0; j <= N; j++)
            {
                for (int i = 0; i <= N; i++)
                {
                    float a = -az + 2f * az * i / N;
                    float e = el0 + (el1 - el0) * j / N;
                    // mat truoc huong ve -Z (phia camera)
                    var d = new Vector3(Mathf.Sin(a) * Mathf.Cos(e), Mathf.Sin(e), -Mathf.Cos(a) * Mathf.Cos(e));
                    int idx = j * (N + 1) + i;
                    verts[idx] = d * 0.5f;
                    norms[idx] = d;
                    uvs[idx] = new Vector2(i / (float)N, j / (float)N);
                }
            }
            var tris = new int[N * N * 6];
            int t = 0;
            for (int j = 0; j < N; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    int a = j * (N + 1) + i;
                    int b = a + 1;
                    int c = a + N + 1;
                    int d = c + 1;
                    tris[t++] = a; tris[t++] = b; tris[t++] = d;
                    tris[t++] = a; tris[t++] = d; tris[t++] = c;
                }
            }
            return SaveMesh(verts, norms, uvs, tris, assetName);
        }

        static Mesh SaveMesh(Vector3[] verts, Vector3[] norms, Vector2[] uvs, int[] tris, string assetName)
        {
            // Dam bao mat ngoai la mat truoc (Unity: tam giac theo chieu kim dong ho).
            Vector3 faceNormal = Vector3.Cross(verts[tris[1]] - verts[tris[0]], verts[tris[2]] - verts[tris[0]]);
            if (Vector3.Dot(faceNormal, norms[tris[0]]) < 0f)
            {
                for (int i = 0; i < tris.Length; i += 3)
                {
                    int tmp = tris[i + 1];
                    tris[i + 1] = tris[i + 2];
                    tris[i + 2] = tmp;
                }
            }
            var mesh = new Mesh();
            mesh.name = assetName;
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, GenPath + "/Mesh_" + (meshCount++) + "_" + assetName + ".asset");
            return mesh;
        }

        static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2f * p1) + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }

        // ---------- nhan vat, camera, UI ----------

        static void SetupCustomer()
        {
            Transform customer = Group("Goc1/Customer");
            var bob = customer.gameObject.AddComponent<Bob>();
            bob.amplitude = 0.015f;
            bob.speed = 1.6f;
            var swap = customer.gameObject.AddComponent<FaceSwap>();
            if (faceObject != null) swap.faceRenderer = faceObject.GetComponent<Renderer>();
            swap.happy = LoadTex("face_happy", false);
            swap.waiting = LoadTex("face_wait", false);
            swap.angry = LoadTex("face_angry", false);
        }

        // Art Bible muc 9: goc nhin thu nhat, mat cao 1,75 m, dung giua phong nhin ve Goc 1.
        // Dieu khien (di lai, nhin, tuong tac) do QuanAnHope.Game.PlayerController gan luc chay.
        static void SetupPlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.SetParent(player.transform, false);
            var cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            go.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            go.transform.localEulerAngles = new Vector3(8f, 0f, 0f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#F3DFB8");
            // Art Bible muc 10: hau ky chay tren camera, UI (Screen Space Overlay) nam tren lop hau ky.
            var camData = go.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
        }

        // Art Bible muc 10: mot den chinh am, chieu tu phia cua ra vao (Goc 1) va tu tren xuong.
        static void SetupLight()
        {
            var go = new GameObject("Den_Chinh");
            var lamp = go.AddComponent<Light>();
            lamp.type = LightType.Directional;
            lamp.color = Hex("#FFF6E6");
            lamp.intensity = 1f;
            lamp.shadows = LightShadows.None;
            go.transform.rotation = Quaternion.LookRotation(-new Vector3(0.35f, 1f, 0.55f).normalized);
            go.AddComponent<ToonLight>();
        }

        // Art Bible muc 10 - Hau ky (URP Volume): Vignette 0,35 (#2B1A10); Film Grain 0,25 (Medium);
        // Color Adjustments: Saturation -5, Color Filter #FFF1DE. Khong Bloom.
        static void SetupPostFx()
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, GenPath + "/PP_NhaHang.asset");

            var vignette = AddOverride<Vignette>(profile);
            vignette.intensity.Override(0.35f);
            vignette.color.Override(Hex("#2B1A10"));

            var grain = AddOverride<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Medium1);
            grain.intensity.Override(0.25f);

            var color = AddOverride<ColorAdjustments>(profile);
            color.saturation.Override(-5f);
            color.colorFilter.Override(Hex("#FFF1DE"));

            var bloom = AddOverride<Bloom>(profile);
            bloom.intensity.Override(0f);

            // Profile mac dinh cua URP bat tonemapping, lam mau phang bi xin; Art Bible khong dung tonemapping.
            var tonemapping = AddOverride<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.None);

            EditorUtility.SetDirty(profile);

            var go = new GameObject("PostFx_Volume");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
        }

        static T AddOverride<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T c = profile.Add<T>(true);
            c.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(c, profile);
            return c;
        }

        // ---------- tien ich ----------

        // "Goc2/DropMachine" -> nhom con DropMachine nam trong nhom Goc2.
        static Transform Group(string path)
        {
            Transform t;
            if (groups.TryGetValue(path, out t)) return t;
            int slash = path.LastIndexOf('/');
            Transform parent = slash < 0 ? sceneRoot : Group(path.Substring(0, slash));
            t = new GameObject(slash < 0 ? path : path.Substring(slash + 1)).transform;
            t.SetParent(parent, false);
            groups[path] = t;
            return t;
        }

        static Material GetMat(bool alpha, string hex, string tex, float tx, float ty)
        {
            string key = (alpha ? "A" : "T") + hex + "|" + tex + "|" + tx + "|" + ty;
            Material m;
            if (mats.TryGetValue(key, out m)) return m;
            m = new Material(alpha ? alphaShader : toonShader);
            m.SetColor("_Color", Hex(hex));
            bool hasTex = !string.IsNullOrEmpty(tex);
            if (hasTex)
            {
                bool repeat = tx != 1f || ty != 1f;
                m.SetTexture("_MainTex", LoadTex(tex, repeat));
                m.SetTextureScale("_MainTex", new Vector2(tx, ty));
            }
            string label = hasTex ? tex : hex.Replace("#", "");
            AssetDatabase.CreateAsset(m, GenPath + "/M_" + mats.Count + "_" + label + ".mat");
            mats[key] = m;
            return m;
        }

        static Texture2D LoadTex(string name, bool repeat)
        {
            string path = RootPath + "/Textures/" + name + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                TextureWrapMode wrap = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                if (importer.wrapMode != wrap || !importer.alphaIsTransparency || importer.npotScale != TextureImporterNPOTScale.None)
                {
                    importer.wrapMode = wrap;
                    importer.alphaIsTransparency = true;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.SaveAndReimport();
                }
            }
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) Debug.LogWarning("Bep Nhan Pham: thieu anh " + path);
            return tex;
        }

        static void SetRenderer(MeshRenderer r, Material m)
        {
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        static void StripCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }

        static Color Hex(string hex)
        {
            Color c;
            return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
