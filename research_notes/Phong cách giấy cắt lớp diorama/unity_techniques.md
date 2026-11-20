# Unity Techniques for Paper-Craft / Stop-Motion / Layered Diorama Story Panels (Unity 6, URP, uGUI/TMP)

Scope note: the web has very few dedicated "papercraft in Unity" tutorials or open-source repos (see the last section). Most of what follows combines official Unity docs (uGUI 2.0, URP volume overrides, scripting API) with standard engineering patterns. Code sketches under "Inferences" are this researcher's own synthesis from those documented APIs. They are not copied from a source, and they need testing in-engine.

---

## 1. Stepped / "on twos" animation and boil jitter, with gameplay kept at full fps

### Takeaway
Do not lower the real frame rate. Keep a separate "cutscene clock" that is quantized in code (`floor(t*fps)/fps`), drive every panel transform and shader parameter from it, and re-roll boil jitter only when the quantized frame index changes. In Shader Graph the same thing is a Posterize node (or floor) applied to time. For keyframed clips, use Constant ("stepped") tangents.

### Cited Findings
- Shader Graph's Posterize node computes `Out = floor(In / (1 / Steps)) * (1 / Steps)`. Feeding it time gives a quantized clock — [Unity Shader Graph Posterize Node docs](https://docs.unity3d.com/Packages/com.unity.shadergraph@8.2/manual/Posterize-Node.html)
- On the Unity forums, the suggested way to get a stop-motion step in a shader is "a step function on the time variable" so incoming time is held for a fixed interval — [Unity Discussions: Intentionally Lower Framerate for shader?](https://discussions.unity.com/t/intentionally-lower-framerate-for-shader/1554968); see also [Unity forum: rounding time not working in HLSL (solved)](https://forum.unity.com/threads/solved-rounding-time-not-working-in-hlsl.1368564/)
- For imported/skeletal animation, the forum advice is to set all keys to stepped in the DCC tool and uncheck "Resample Curves" in Unity's import settings — [Unity forum: 3D animation on twos in Unity](https://forum.unity.com/threads/3d-animation-on-twos-in-unity.707120/); related: [Making a Stopmotion 3D Animation in Unity](https://discussions.unity.com/t/making-a-stopmotion-3d-animation-in-unity/826330)
- In the curve editor, "Broken – Constant" tangents mean the left key's value holds for the whole segment, i.e. a stepped curve — [Unity Manual: Editing Curves (6000.3)](https://docs.unity3d.com/Manual/EditingCurves.html)
- `AnimationUtility.SetKeyBroken` sets a key's broken-tangent flag and recalculates in/out tangents. It is editor-only (`UnityEditor`) — [ScriptReference: AnimationUtility.SetKeyBroken](https://docs.unity3d.com/ScriptReference/AnimationUtility.SetKeyBroken.html). Unity's own reference source sets left and right tangent modes to Constant and then calls `SetKeyBroken(true)` for discrete keys — [UnityCsReference mirror commit](https://scriptagc.wasmer.app/https_github_com/prototypestd/UnityCsReference/commit/55dd5f4fdaedbc2e98d33d91f7978d6e8547b5a7) (third-party mirror, 2017.3)
- Precision pitfall: time-driven shader animation becomes choppy when computed at half precision on some Android devices — [Unity Issue Tracker: low-precision time](https://issuetracker.unity3d.com/issues/android-animations-based-on-the-time-variable-become-choppy-when-using-low-precision-calculations); [half precision stutter](https://issuetracker.unity3d.com/issues/half-point-precision-causes-stutter-in-shader)
- Animators on UI dirty their elements every frame even when values do not change. Unity recommends code or tweening for UI that changes rarely — [Unity: UI optimization tips](https://unity.com/how-to/unity-ui-optimization-tips)

### Inferences
- **Separate clock, full-fps game.** Gameplay keeps `Time.deltaTime` at the native frame rate. The cutscene controller accumulates its own time (use `Time.unscaledDeltaTime` if the game is paused with `timeScale = 0` during the story panel), then quantizes it:
  ```csharp
  public class SteppedClock {
      public float Fps = 12f;            // 12 = "on twos" at 24fps; 8 = "on threes"
      public float RawTime { get; private set; }
      public int   Frame   { get; private set; }
      public float SteppedTime => Frame / Fps;
      public event System.Action<int> OnFrame;   // fires only when the frame index changes
      public void Tick(float dt) {
          RawTime += dt;
          int f = Mathf.FloorToInt(RawTime * Fps);
          if (f != Frame) { Frame = f; OnFrame?.Invoke(f); }
      }
  }
  ```
  All tweens are evaluated at `SteppedTime`, never at `RawTime`: `pos = Vector2.LerpUnclamped(a, b, ease(SteppedTime / dur))`. Positions only change about 12 times per second while rendering stays at 60+ fps, so input and UI stay responsive.
- **Writing transforms only on OnFrame** also cuts Canvas rebuilds by about 5x compared with per-frame tweening, since an unchanged RectTransform is not dirtied. This follows from the rebuild rules in section 6.
- **Boil (line/shape jitter).** Use a deterministic hash on (layerId, frame / holdFrames) so the jitter is reproducible and does not allocate:
  ```csharp
  static float Hash(int a, int b) { unchecked { uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663);
      h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xFFFFFF) / (float)0xFFFFFF; } }
  // in OnFrame(int f):
  int k = f / boilHold;                     // boilHold = 2..3 stepped frames
  Vector2 off = new Vector2(Hash(id, k) - .5f, Hash(id + 999, k) - .5f) * 2f * boilPx; // 0.5–1.5 px
  float rot   = (Hash(id + 31, k) - .5f) * 2f * boilDeg;                                // 0.3–1°
  rt.anchoredPosition = basePos + off; rt.localEulerAngles = new Vector3(0,0,baseRot + rot);
  ```
  For "real" boil (the outline itself wobbles, not just the whole piece), cycle among 2–3 pre-generated mesh variants. In a custom Graphic, regenerate the polygon with a different noise seed per boil index and cache the 3 VertexHelper results as Meshes, or swap among 3 sprites with different torn edges. Cycling a fixed set is cheaper and reads better than fresh noise each time.
- **Shader-side stepped time:** in HLSL, `float t = floor(_Time.y * _StepFps) / _StepFps;`. Better still, push the stepped time from C# as a global (`Shader.SetGlobalFloat("_CutsceneTime", clock.SteppedTime)`) so shaders and transforms share one clock and pausing works. Use `float`, not `half`, for time (see the precision issue above).
- **Timeline / Animator clips:** if clips are authored in-editor, an editor script can loop over `AnimationUtility.GetCurveBindings`, set every key's left/right tangent mode to `TangentMode.Constant`, and write back with `AnimationUtility.SetEditorCurve`. Or, simpler for a procedural pipeline, skip Animator and drive everything from the SteppedClock (Unity's own tips advise against Animators on UI anyway).
- Easing still works when stepped: the ease shapes the spacing of the 12 poses, as in traditional animation. Linear motion stepped on twos looks mechanical, so prefer ease-in/out.

### Gaps
- No official Unity sample or blog post on "on twos" for UI/cutscenes was found. The solved HLSL forum thread's fix was not visible in the snippet.
- No source compared perceived quality of 12 vs 8 fps holds or recommended boil amplitudes. The px and degree values above are engineering guesses.

---

## 2. Paper look in uGUI (grain, white cut edge, drop shadows, torn edges, custom Graphic)

### Takeaway
Build each paper piece as a custom `Graphic` (`MaskableGraphic` subclass overriding `OnPopulateMesh`) that triangulates a noisy polygon. That gives torn or irregular edges without any art. Fake the white cut edge and the drop shadow by emitting the same polygon two more times inside the same mesh (inflated white rim, offset dark shadow), rather than stacking Shadow/Outline components. Generate paper grain once as a small tiling `Texture2D` with `Mathf.PerlinNoise` (fBm) and multiply it in via `mainTexture` or a UI shader.

### Cited Findings
- `Graphic.OnPopulateMesh(VertexHelper)` is the protected virtual callback subclasses use to fill vertex data. Text, Image and RawImage use it to generate their own vertices. The old `OnPopulateMesh(Mesh)` overload is obsolete — [uGUI 2.0 API: Graphic](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Graphic.html)
- `mainTexture` is "the Texture that gets passed to the CanvasRenderer, Material and then Shader _MainTex" and can be overridden. UI batching means an atlas reduces draw calls — [uGUI 2.0 API: Graphic](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Graphic.html)
- `SetVerticesDirty()` flags vertices for rebuild, `SetMaterialDirty()` flags material, and `SetAllDirty()` flags layout, verts and material. Rebuild happens in the PreRender step of the canvas update — [uGUI 2.0 API: Graphic](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Graphic.html)
- The Shadow component has Effect Color, Effect Distance (offset vector) and Use Graphic Alpha, and must sit on the same GameObject as the Graphic — [uGUI 2.0 manual: Shadow](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Shadow.html)
- `Mathf.PerlinNoise(float x, float y)` returns roughly 0–1 but "can be slightly lower than 0.0f or slightly higher than 1.0f", so clamp it. The same coordinates always return the same value, and sampling a random area avoids visible repetition. The official example fills a `Texture2D` with `SetPixels` + `Apply` — [Unity ScriptReference: Mathf.PerlinNoise](https://docs.unity.com/en-us/engine/current/script-reference/unityengine/mathf/perlinnoise)
- A real game reference: Paper Cut Mansion built paper textures by drawing flat papercraft unfolds ("Reverse UV") — [Game Developer: Deep dive – Paper Cut Mansion](https://gamedeveloper.com/art/deep-dive-how-paper-cut-mansion-was-made). A paper-cut look tutorial focused on art (not Unity code) is at [Bokettori: paper cut effect](https://bokettori.substack.com/p/paper-cut-effect-with-paper-tales)

### Inferences
- **How Shadow/Outline actually work (from the uGUI source, not re-verified this session):** both are `BaseMeshEffect`s that copy the Graphic's vertices in `ModifyMesh`. Shadow adds one offset copy (2x verts). Outline adds four copies at ±x/±y (5x verts). Outline on a big polygon only makes a uniform-offset "fat" edge, which gaps at sharp concave corners, and on TMP text it costs 5x glyph quads. For paper pieces it is better to generate the rim yourself. The uGUI source is on GitHub at Unity-Technologies/uGUI, but the fetch failed here, so treat this as unverified.
- **Custom PaperShape Graphic sketch:**
  ```csharp
  [RequireComponent(typeof(CanvasRenderer))]
  public class PaperShape : MaskableGraphic {
      public Vector2[] outline;            // closed polygon in local px, generated in code
      public float tearAmp = 3f, tearFreq = 0.15f; public int seed;
      public float rimWidth = 4f;  public Color rimColor = Color.white;
      public Vector2 shadowOffset = new(4,-5); public Color shadowColor = new(0,0,0,.35f);
      public Texture paperTex;  public override Texture mainTexture => paperTex ? paperTex : s_WhiteTexture;

      protected override void OnPopulateMesh(VertexHelper vh) {
          vh.Clear();
          var torn = Tear(Resample(outline, 4f), tearAmp, tearFreq, seed); // jitter along normals with Perlin
          var rim  = Offset(torn, rimWidth);                                // polygon offset (miter / round)
          AddFan(vh, rim,  shadowOffset, shadowColor);   // 1) drop shadow (drawn first = behind)
          AddFan(vh, rim,  Vector2.zero, rimColor);      // 2) white cut edge
          AddFan(vh, torn, Vector2.zero, color);         // 3) colored paper face
      }
      // AddFan: centroid fan works for star-shaped polys; for concave shapes use ear-clipping.
      // UV = local position / paperTileSize so the grain tiles in world-consistent scale.
  }
  ```
  Order inside one VertexHelper determines draw order, so shadow, rim and face cost one draw call and batch with siblings that share the material and texture.
- **Torn edge:** resample the polygon at about 3–5 px spacing, then displace each point along its normal by `(PerlinNoise(s*tearFreq, seed) - .5f) * 2 * tearAmp` plus a small high-frequency term. Use a larger amplitude on the rim polygon than on the face, so the white fiber edge shows uneven thickness, which is the signature of torn paper. For clean scissor cuts, set amp to 0 on the face and about 0.5 px on the rim.
- **Concave triangulation:** ear clipping is O(n²) but fine for <300 verts. Do it once at build time and cache. Never retriangulate per frame (boil should cycle cached variants).
- **Paper grain texture** (generate once, about 256², `wrapMode = Repeat`, mipmaps on, `filterMode = Bilinear`):
  ```csharp
  var tex = new Texture2D(256,256,TextureFormat.RGBA32,true){wrapMode=TextureWrapMode.Repeat};
  var px = new Color32[256*256];
  for (int y=0;y<256;y++) for (int x=0;x<256;x++){
      float u=x/256f, v=y/256f, n=0, a=.5f, f=8;
      for(int o=0;o<4;o++){ n+=a*Mathf.PerlinNoise(u*f+17.3f*o, v*f+5.1f*o); a*=.5f; f*=2; } // fBm
      float fiber = Mathf.PerlinNoise(u*64, v*6);           // anisotropic streaks = paper fibers
      float g = Mathf.Clamp01(0.88f + (n-.47f)*0.18f + (fiber-.5f)*0.05f);
      byte b=(byte)(g*255); px[y*256+x]=new Color32(b,b,b,255);
  }
  tex.SetPixels32(px); tex.Apply(true, true); // makeNoLongerReadable frees CPU copy
  ```
  Perlin repeats with period 256 only at integer lattice points, so making the texture truly seamless needs the 4-corner blend trick or periodic noise. Mathf.PerlinNoise is not periodic, so blend: `n(u,v)*(1-u)*(1-v)+n(u-1,v)*u*(1-v)+...`. Keep grain contrast low (±5–10%) or it reads as dirt.
- **Multiplying grain** without a custom shader: the default UI shader computes `tex * vertexColor`, so a grey grain texture times the piece's vertex color already gives tinted paper. That means one shared grain texture and one material for all pieces, which batches.
- **Image + generated Sprite** is an alternative: draw the shape into a Texture2D (alpha mask) and use `Sprite.Create`. This costs texture memory per shape and gives a blurrier edge when scaled up. Custom Graphic meshes stay crisp at any zoom (good for Ken Burns) and use no memory beyond vertices, so OnPopulateMesh is preferred for procedural pieces.
- **SDF option:** for very smooth, resolution-independent rims and soft shadows, write a UI shader that evaluates an SDF (per-piece SDF texture baked at runtime, or analytic for circles/rounded rects). Rim = `smoothstep` band, shadow = SDF sampled at an offset UV, blurred. TextMeshPro already does this for text: its SDF material has Outline (white rim) and Underlay (offset, soft drop shadow) at almost no cost, which is the right way to get "cut-paper lettering". Do not use the Outline component on TMP.
- **Shadows between layers:** a cheap stacked-paper look uses a shadow offset of 3–8 px down-right (consistent light direction across all pieces) at about 25–40% alpha black or a dark warm tint. Scale the offset with the layer's "depth" (front layers cast longer shadows onto the back plate). A soft shadow needs either a blurred SDF or a second, larger, low-alpha fan ring. With vertex color you can feather by building the shadow as an inner polygon (alpha a) plus an outer offset ring (alpha 0), so the GPU interpolates the gradient.

### Gaps
- Could not fetch the uGUI Shadow.cs/Outline.cs source to cite exact vertex-duplication behavior (404s). The claim above comes from prior knowledge.
- No published, tested Unity sample of a torn-paper procedural Graphic was found.

---

## 3. Paper look in 3D URP (planes in depth, parallax camera, Shader Graph, lighting, DoF, vignette)

### Takeaway
A diorama in URP is a set of flat quads (or extruded thin meshes) at different Z, viewed by a perspective camera with a narrow FOV. Real parallax and real cast shadows between planes come for free. Use a Lit or Simple Lit material (paper texture plus a procedural normal from the grain) with one directional or spot light so planes cast shadows onto each other. Add Depth of Field (Bokeh for near and far, Gaussian for cheap far-only) and Vignette via a Volume for the miniature "box" feel.

### Cited Findings
- URP DoF has two modes. Gaussian is "the fastest" with "limited blur radius and only does far-field blurring". Bokeh is slower, "closely imitates the effects of a real-life camera", and does near and far blur. Unity recommends Gaussian for lower-end devices and Bokeh for console/desktop — [Unity 6.3 Manual: Depth of Field volume override](https://docs.unity3d.com/Manual/urp/depth-of-field-volume-override.html); [reference (6000.6)](https://docs.unity3d.com/Manual/urp/depth-of-field-volume-override-reference.html); [Unity 6 (6000.0) DoF in URP](https://docs.unity.cn/Manual/urp/depth-of-field-urp.html)
- Gaussian: Start/End distances for far blur. `gaussianMaxRadius` values above 1 can show under-sampling artifacts, and High Quality Sampling reduces flicker at some cost. Bokeh: Focus Distance, Focal Length, Aperture, Blade Count/Curvature/Rotation — [URP API: DepthOfField (17.0)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/api/UnityEngine.Rendering.Universal.DepthOfField.html)
- Setup: Volume → Add Override → Post-processing → Depth Of Field. It applies to cameras the Volume affects (the camera must have Post Processing enabled) — [Unity 6 Manual: DoF volume override](https://docs-alpha.unity3d.com/Manual/urp/depth-of-field-volume-override.html)
- Unity's URP recipes / URP Cookbook (Unity 6 edition) include a diorama-style sample scene mixing baked and realtime light, with samples on GitHub — [Unity blog: 12 recipes for URP](https://unity.com/blog/engine-platform/12-recipes-for-popular-visual-effects-using-universal-render-pipeline); [URP Cookbook PDF](https://unity3d.jp/wp-content/uploads/2025/07/URP-Cookbook.pdf)
- Two-sided paper shaders that use albedo alpha as the cut-out and different front/back looks exist as paid Asset Store packs, which confirms the alpha-clip approach — [Asset Store: Two-sided shaders URP](https://marketplace.unity.com/packages/vfx/shaders/two-sided-shaders-urp-180885)
- Daniel Ilett publishes URP stylized shader work (sketched shadows from screen-space shadow maps, cel shading) that can be adapted — [daniel-ilett/cel-shading-urp](https://github.com/daniel-ilett/cel-shading-urp)

### Inferences
- **Planes:** each layer is a Quad (or a procedurally generated polygon Mesh, reusing the same torn-polygon code as uGUI with a MeshFilter) at z = 0, 0.5, 1.0... For visible paper thickness, extrude the polygon 1–2 mm with side faces in an off-white color. That edge catches light and reads strongly as "cardboard cut-out".
- **Material choice:** URP/Lit with Alpha Clipping (or opaque polygon meshes, which avoid clipping entirely) and Cast/Receive Shadows on. Opaque meshes are better than alpha-cut quads for overdraw and early-Z. The normal map can be generated from the grain heightmap (Sobel → RGB) at runtime, or in Shader Graph use the Normal From Height node on the grain texture. Unlit is fine for the UI version but loses inter-plane shadows. A middle ground is Unlit plus a baked blob shadow quad per layer.
- **Shader Graph paper node chain:** BaseColor = Color × lerp(1, grainTex, 0.15). Normal = NormalFromHeight(grain, strength 0.02–0.05). Smoothness about 0.1. Optional edge darkening via vertex color stored on outline verts (alpha 0 on perimeter) feeding a `1 - smoothstep` multiply. Stepped time into any animated noise via Posterize.
- **Shadows between planes:** one directional light angled about 30–45° plus a soft-ish shadow (URP Soft Shadows, shadow distance clamped to the diorama size so the cascade resolution is spent on a small area). Planes 0.3–1 units apart at a 50-unit camera distance give readable but non-overlapping shadows. Shadow bias may need lowering or the thin gaps show light leaks (peter-panning).
- **Parallax camera:** in 3D, parallax is real. Move a perspective camera with a small FOV (15–30°) so the dolly shows depth without distortion, or slide the camera on X/Y while looking forward. Step the camera transform with the SteppedClock for stop-motion feel. A stepped camera with smooth DoF is a classic miniature look.
- **Tilt-shift:** URP has no built-in tilt-shift. The usual approximation is Bokeh DoF with a short focus distance, large aperture (low f-number) and long focal length. A true screen-space tilt-shift (blur by screen Y band) needs a custom Full Screen Pass Renderer Feature with a Fullscreen Shader Graph (Unity 2022.2+ / Unity 6).
- **Vignette** (Volume override: Intensity about 0.3–0.45, Smoothness about 0.4, slightly warm color) darkens the box corners. Film Grain at low intensity and a slight Color Adjustments saturation drop help sell the photographed look.
- **UI-on-top option:** the story panel can be a 3D diorama rendered by a dedicated camera into a RenderTexture shown in a uGUI RawImage. This gives real DoF and shadows inside a UI frame, at the cost of a second camera pass (keep the RT small, e.g. panel-sized).

### Gaps
- No Unity-official or well-known open-source papercraft diorama sample was found. The Cookbook diorama is about lighting, not paper.
- The URP Vignette and Film Grain docs were not fetched this session. Parameter suggestions are from experience.

---

## 4. Parallax inside a UI panel (Ken Burns, RectMask2D clipping)

### Takeaway
Put the panel's layers under one RectMask2D (rectangular, stencil-free clipping) and drive a virtual camera (pan offset `p` and zoom `z`). Each layer gets `anchoredPosition = basePos - p * depthFactor` and `localScale = 1 + (z-1) * depthFactor`. Background layers have factors around 0.2–0.5, foreground around 1.2–1.5. Evaluate it on the stepped clock.

### Cited Findings
- RectMask2D clips children to the parent rectangle without the stencil buffer, so it adds no extra draw calls or material changes (unlike Mask). It only works for coplanar 2D elements — [uGUI 2.0 manual: RectMask2D](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-RectMask2D.html)
- RectMask2D exposes `padding` (Vector4 L/B/R/T) and `softness` (Vector2Int) for feathered edges. `PerformClipping()` culls or clips children after layout, before graphics update — [uGUI 2.0 API: RectMask2D](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.RectMask2D.html)
- Mask uses the stencil buffer (contrast with RectMask2D) — [uGUI 2.0 manual: RectMask2D](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-RectMask2D.html)

### Inferences
- **Ken Burns controller:**
  ```csharp
  [System.Serializable] public struct ParallaxLayer { public RectTransform rt; public float depth; public Vector2 basePos; }
  public class PanelCamera : MonoBehaviour {
      public ParallaxLayer[] layers; public SteppedClock clock = new();
      public Vector2 panFrom, panTo; public float zoomFrom = 1f, zoomTo = 1.12f, duration = 4f;
      void Awake(){ clock.OnFrame += _ => Apply(); }
      void Update(){ clock.Tick(Time.unscaledDeltaTime); }
      void Apply(){
          float k = Mathf.SmoothStep(0,1, Mathf.Clamp01(clock.SteppedTime / duration));
          Vector2 p = Vector2.Lerp(panFrom, panTo, k); float z = Mathf.Lerp(zoomFrom, zoomTo, k);
          foreach (var L in layers){
              float s = 1 + (z - 1) * L.depth;                 // nearer layers zoom more
              L.rt.localScale = new Vector3(s, s, 1);
              L.rt.anchoredPosition = (L.basePos - p * L.depth) * s; // zoom about panel centre
          }
      }
  }
  ```
  Keep layer pivots at the panel center (0.5, 0.5) so zoom scales around the center. Oversize background layers by `max pan × depth + zoom margin` so edges never show inside the mask.
- Use RectMask2D softness (a few px) for slightly soft panel edges. For a hard white "photo border", just add a white PaperShape frame on top. For irregular (torn) panel borders, RectMask2D cannot help since it is rectangle only. Use a stencil `Mask` with a PaperShape as the mask graphic (costs extra draw calls and breaks batching), or simply draw a torn frame on top that covers the rectangular clip edge, which is usually cheapest.
- RectMask2D clipping only works if children are coplanar, so do not rotate layers in X/Y (3D tilt) inside it. Z rotation is fine. Boil rotation is Z only.
- For a parallax "depth push" when the panel opens, animate `depth`-weighted pan from a small offset to 0 over about 0.5 s on the stepped clock.

### Gaps
- No source quantified a recommended depth-factor range for UI parallax. The values are conventional.

---

## 5. Performance notes and pitfalls

### Takeaway
The main costs are Canvas rebuilds (any dirtied Graphic forces its Canvas to rebuild its batch), overdraw from many stacked full-screen paper layers, and large runtime textures. Put the animated panel on its own sub-Canvas, change transforms only on stepped frames, share one grain texture and one material, and prefer opaque-looking polygon meshes over big transparent quads.

### Cited Findings
- Objects that change their RectTransform every frame make the canvas rebuild its batch, which causes CPU spikes. Profile `Canvas.BuildBatch` and `Canvas.RenderOverlays`. Each split canvas generates its own batch, so splitting trades rebuild cost for draw calls — [Unity Support: Split canvas for dynamic objects](https://support.unity.com/hc/en-us/articles/115000355466-Split-canvas-for-dynamic-objects)
- "Each Canvas is an island". Keep static and dynamic UI on separate Canvases. Elements on a Canvas should share Z, materials and textures for batching. Disable the Canvas component (not the GameObject) to hide without losing meshes. Turn off Raycast Target on non-interactive graphics. Avoid Layout Groups. Avoid Animators on UI. Stacked layered elements (like cards) cause overdraw, so merge layers at runtime where possible — [Unity: UI optimization tips](https://unity.com/how-to/unity-ui-optimization-tips)
- A dirty child sub-canvas does not force the parent to rebuild, and vice versa. On mobile, heavy UI overdraw can quickly exceed GPU fill-rate — [Unity Learn: Optimizing Unity UI](https://learn.unity.com/tutorial/optimizing-unity-ui)
- Time-based shader animation computed in half precision can stutter on Android — [Unity Issue Tracker](https://issuetracker.unity3d.com/issues/android-animations-based-on-the-time-variable-become-choppy-when-using-low-precision-calculations)

### Inferences
- **Overdraw math:** 6 full-panel layers, each with shadow and rim copies, means about 18x fill in the worst case, which is heavy on low-end mobile. Custom polygon meshes only cover their shape (unlike rectangular sprites with transparent margins), which already cuts overdraw a lot. Back plates and sky can be a single opaque-ish quad.
- **Bake static layers:** once a panel's background layers stop moving relative to each other, render them once to a RenderTexture (or `Texture2D` via ReadPixels) and display a single RawImage. This is the "merge layers at runtime" advice applied.
- **Rebuild control:** moving or scaling a RectTransform only re-batches. It does not re-run `OnPopulateMesh`, but changing color, size or properties that call `SetVerticesDirty` does. Keep boil to transform changes or cached mesh swaps so `OnPopulateMesh` runs only on build.
- **Textures:** a 256² RGBA32 grain texture with mips is about 340 KB. Use a single-channel `TextureFormat.R8` (about 85 KB) if a custom shader reads `.r`. The default UI shader samples RGBA, so R8 gives a red tint there; use Alpha8/RGBA or a custom shader. Call `Apply(true, true)` to drop the CPU copy. Avoid generating textures per piece. Use one shared tiling grain and tint with vertex color.
- **TMP:** use SDF Outline/Underlay on the material preset rather than Outline/Shadow components (5x and 2x vertex cost per glyph). Note that changing material properties per text instance creates material instances that break batching, so create 1–2 shared presets.
- **Unity 6 notes:** uGUI ships as the `com.unity.ugui` 2.0 package (docs above are for 2.0), and TextMeshPro is folded into it in Unity 6 rather than being a separate package (from prior knowledge, not verified this session). `OnPopulateMesh(Mesh)` is obsolete, so use the `VertexHelper` overload. URP in Unity 6 uses Render Graph, so custom fullscreen effects (tilt-shift) should use the Full Screen Pass Renderer Feature or a Render Graph `ScriptableRenderPass` rather than legacy `Execute`-based passes.

### Gaps
- No benchmark numbers for custom Graphic vs Image cost were found.
- Unity 6 Render Graph and TMP-merge details were not verified with a fetched source in this session.

---

## 6. Open-source examples and tutorials (papercraft / cut-out / stop-motion)

### Takeaway
Very little exists that is specific to papercraft in Unity. Searches found no dedicated open-source Unity papercraft or cut-paper diorama repo or tutorial. The closest materials are general stylized-URP resources, paid Asset Store shaders, and game post-mortems. Plan to build from the primitives above.

### Cited Findings
- No GitHub repo for a URP paper-cutout diorama shader turned up. Related: Daniel Ilett's URP cel shading and stylized shadows — [github.com/daniel-ilett/cel-shading-urp](https://github.com/daniel-ilett/cel-shading-urp); a URP shader collection — [TinyPlay/URPShadersCollection](https://github.com/TinyPlay/URPShadersCollection); NiloCat URP toon examples — [ColinLeung-NiloCat/UnityURPToonLitShaderExample](https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample)
- Stop-motion clay look (paid, Shader Graph, URP/HDRP) whose marks can be "moved around to mimic the effect in stop motion movies" — [Asset Store: Realistic Clay Shader](https://marketplace.unity.com/packages/vfx/shaders/realistic-clay-shader-167036)
- Diorama lighting reference: the URP Cookbook / 12 recipes — [Unity blog](https://unity.com/blog/engine-platform/12-recipes-for-popular-visual-effects-using-universal-render-pipeline)
- Game post-mortem: Paper Cut Mansion (papercraft textures via unfolded "Reverse UV") — [Game Developer](https://gamedeveloper.com/art/deep-dive-how-paper-cut-mansion-was-made); Paper Trail's custom Unity shader combining a normal map with vertex deformation for folding paper — [Foro3D (2026)](https://foro3d.com/en/2026/mayo/paper-trail-como-doblar-la-realidad-con-shaders-en-unity.html) (secondary source)
- A non-Unity analog of stepped time: Caddis "Posterize Time" node (step 2 = on twos) — [Caddis docs](https://caddis-docs.pages.dev/nodes/posterize-time); Remotion posterization — [remotion.dev](https://remotion.dev/docs/posterization)

### Inferences
- Ronja Tutorials (polygon/2D SDF, noise), Cyanilux (Shader Graph, UI shaders) and Minions Art (stylized shaders) are well-known general resources for the SDF, noise and Shader Graph pieces, but no paper-specific article from them was confirmed in this session.

### Gaps
- No GDC/Unite talk specific to papercraft rendering in Unity was found within the search budget. Known papercraft-style games (Paper Mario, Tearaway, Paper Beast, Paper Trail) are not Unity-sourced talks with technique detail, except Paper Trail (Unity), for which only a secondary article was found.
