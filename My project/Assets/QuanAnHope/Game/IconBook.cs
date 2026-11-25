using System;
using System.Collections.Generic;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// Anh nho cho UI chup tu chinh model 3D trong phong (yeu cau 22/11/2026 + "Phong cach huong dan bang hinh.md": mot bo icon
    /// dung chung cho bong bong goi mon, thuc don, the huong dan, bang Tab - nguoi choi chi hoc hinh mot lan).
    /// Moi anh chup mot lan bang camera rieng dat xa phong (y = -300), nen trong suot, cat vao bo nho dem.
    /// </summary>
    public static class IconBook
    {
        public const int Size = 256;
        static readonly Vector3 StagePos = new Vector3(0f, -300f, 0f);
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
        static Camera cam;
        static Transform stage;
        static RenderTexture rt;

        /// <summary>So anh da chup (test / do hieu nang).</summary>
        public static int Shots { get; private set; }

        public static Texture2D Ingredient(string typeId, bool prepared = false, int tier = 2)
        {
            return Shoot("nl_" + typeId + (prepared ? "_p" : "") + "_" + tier, t =>
            {
                var ing = new Core.Ingredient { typeId = typeId, tier = tier, prepared = prepared, name = typeId };
                ItemView v = ItemView.Create(t, ing, 4f, false);
                v.SetPrepared(prepared);
                return v.gameObject;
            }, 18f);
        }

        public static Texture2D Tool(string tool)
        {
            return Shoot("dc_" + tool, t =>
            {
                GameObject go = Toon.Empty(t, "DungCu", Vector3.zero);
                if (ModelLibrary.TryInstantiate("Kitchen", RoomView.ToolModel(tool), go.transform, Toon.OutlineSmall) == null)
                    Toon.Prim(PrimitiveType.Cylinder, go.transform, "Than", new Vector3(0f, 0.05f, 0f), new Vector3(0.35f, 0.1f, 0.35f), UiKit.Hex("#5A5250"), Toon.OutlineSmall);
                return go;
            }, 30f);
        }

        /// <summary>Lo do kho (gia vi) theo mau lo tren ke Goc 4.</summary>
        public static Texture2D Jar(string item, Color color)
        {
            return Shoot("lo_" + item, t =>
            {
                GameObject jar = Toon.Prim(PrimitiveType.Cylinder, t, "Lo", new Vector3(0f, 0.15f, 0f), new Vector3(0.2f, 0.3f, 0.2f), color, Toon.OutlineSmall);
                Toon.Prim(PrimitiveType.Cylinder, jar.transform, "Nap", new Vector3(0f, 0.17f / 0.15f, 0f), new Vector3(1.1f, 0.05f / 0.15f, 1.1f),
                    UiKit.Hex("#B97F52"), Toon.OutlineSmall * 3f);
                return jar;
            }, 22f);
        }

        /// <summary>Mon tren dia (bac Binh thuong, viền dia mau bac).</summary>
        public static Texture2D Dish(Recipe r, RecipeBook book, int tier = 2)
        {
            if (r == null) return null;
            return Shoot("mon_" + r.id + "_" + tier, t =>
                DishModel.Create(t, new Core.Dish { recipeId = r.id, name = r.name, baseName = r.baseName, tier = tier }, book, false), 32f);
        }

        public static Texture2D Plate()
        {
            return Shoot("dia_trong", t => DishModel.EmptyPlate(t), 32f);
        }

        public static Texture2D Board()
        {
            return Shoot("thot", t =>
            {
                GameObject go = Toon.Empty(t, "Thot", Vector3.zero);
                if (ModelLibrary.TryInstantiate("Kitchen", "SM_Thot", go.transform, Toon.OutlineSmall) == null)
                    Toon.Prim(PrimitiveType.Cube, go.transform, "Thot", new Vector3(0f, 0.03f, 0f), new Vector3(1f, 0.06f, 0.6f), UiKit.Hex("#F2D7B6"), Toon.OutlineSmall);
                return go;
            }, 40f);
        }

        /// <summary>
        /// F-092 AC-2: ve mot lan thung nguyen lieu day mon + hieu ung cong xoan bang camera rieng (luc o menu) de lan nhet ve dau
        /// khong phai nap luoi / vat lieu / hat trong khung hinh dang choi.
        /// </summary>
        public static void WarmCrate(Core.Ingredient sample)
        {
            if (sample == null) return;
            Shoot("am_thung_" + sample.typeId, t =>
            {
                CrateView c = CrateView.Create(t, sample, UiKit.Hex("#D69B68"), false);
                for (int i = 0; i < 3; i++)
                {
                    ItemView v = ItemView.Create(c.ItemRoot, sample, CrateView.ItemScale, false);
                    v.transform.localPosition = CrateView.ItemPosition(i);
                }
                PortalFx.Play(c.transform, Vector3.up * 0.5f);
                return c.gameObject;
            }, 30f);
        }

        public static Texture2D Relic(RelicDef r)
        {
            if (r == null) return null;
            return Shoot("relic_" + r.id, t => RelicArt.Create(t, r, 1f), 24f);
        }

        /// <summary>Chup mot vat dung tren san khau: camera xien 30 do, khung vua hinh that (Renderer.bounds, L-016).</summary>
        static Texture2D Shoot(string key, Func<Transform, GameObject> build, float pitch)
        {
            Texture2D tex;
            if (cache.TryGetValue(key, out tex) && tex != null) return tex;
            if (!Ensure()) return null;

            GameObject go = null;
            Camera savedView = FaceBillboard.View;
            RenderTexture savedActive = RenderTexture.active;
            try
            {
                go = build(stage);
                if (go == null) return null;
                go.transform.SetParent(stage, false);
                go.transform.localPosition = Vector3.zero;
                foreach (Pop p in go.GetComponentsInChildren<Pop>(true)) UnityEngine.Object.DestroyImmediate(p);
                go.transform.localScale = go.transform.localScale.sqrMagnitude < 1e-6f ? Vector3.one : go.transform.localScale;

                Bounds b = BoundsOf(go);
                float radius = Mathf.Max(0.05f, b.extents.magnitude);
                Quaternion rot = Quaternion.Euler(pitch, 180f + 20f, 0f);
                float dist = radius / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.02f;
                cam.transform.SetPositionAndRotation(b.center - rot * Vector3.forward * dist, rot);
                cam.nearClipPlane = Mathf.Max(0.01f, dist - radius * 2f);
                cam.farClipPlane = dist + radius * 2f;

                // mat 2D quay ve camera chup
                FaceBillboard.View = cam;
                foreach (FaceBillboard f in go.GetComponentsInChildren<FaceBillboard>(true)) f.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);

                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { name = "Icon_" + key, wrapMode = TextureWrapMode.Clamp };
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                tex.Apply(false, false);
                cache[key] = tex;
                Shots++;
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogWarning("IconBook: không chụp được " + key + ": " + e.Message);
                return null;
            }
            finally
            {
                FaceBillboard.View = savedView;
                RenderTexture.active = savedActive;
                if (cam != null) cam.targetTexture = null;
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            }
        }

        static Bounds BoundsOf(GameObject go)
        {
            bool any = false;
            var b = new Bounds(go.transform.position, Vector3.zero);
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.name == "Outline" || r is ParticleSystemRenderer) continue;
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }

        static bool Ensure()
        {
            if (cam != null && stage != null && rt != null) return true;
            // chay khong card do hoa (-nographics: kiem tra bot trong ban build, F-091): khong chup duoc, UI dung o trong
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return false;
            var root = new GameObject("IconBook_SanKhau");
            UnityEngine.Object.DontDestroyOnLoad(root);
            root.transform.position = StagePos;
            stage = new GameObject("Vat").transform;
            stage.SetParent(root.transform, false);
            var cgo = new GameObject("IconCam");
            cgo.transform.SetParent(root.transform, false);
            cam = cgo.AddComponent<Camera>();
            cam.enabled = false;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.fieldOfView = 22f;
            cam.allowHDR = false;
            cam.allowMSAA = true;
            // URP tu them du lieu camera mac dinh: khong hau ky (renderPostProcessing mac dinh tat)
            rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 4, name = "IconBook_RT" };
            return true;
        }

        /// <summary>Xoa bo nho dem (doi phong / test).</summary>
        public static void Clear()
        {
            foreach (Texture2D t in cache.Values) if (t != null) UnityEngine.Object.Destroy(t);
            cache.Clear();
        }
    }
}
