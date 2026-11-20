# Tools for procedural cartoon story panels in uGUI (Unity 6000.6.9f1, URP 17.6, uGUI 2.6)

Researched 2026-11-19. Versions were checked directly against the registries: packages.unity.com, package.openupm.com, registry.npmjs.org.

## Project facts that matter (checked in repo)
- `ProjectSettings.asset`: `m_ActiveColorSpace: 1` means **Linear**. This affects UI vertex colors, see section 2.
- `Assets/Settings/PC_RPAsset.asset` and `Mobile_RPAsset.asset`: `m_MSAA: 1` means **MSAA off**. Even with MSAA on, a Screen Space Overlay canvas does not get camera MSAA.
- The project already has an SDF uGUI shader at `Assets/QuanAnHope/Resources/Shaders/InkShape.shader`. It supports rounded box, ellipse and arc, 2-tone shading with a sharp boundary, fwidth AA, `UNITY_UI_CLIP_RECT`, `UNITY_UI_ALPHACLIP`, stencil, and `_UIVertexColorAlwaysGammaSpace` handling. Extend this shader rather than replace it.

---

## 1. Vector Graphics (built-in module + package)
- In Unity 6.3+, `com.unity.modules.vectorgraphics` 1.0.0 is a **built-in engine module**. Assembly: `UnityEngine.VectorGraphicsModule`. Namespace: `Unity.VectorGraphics`.
  - It contains the full programmatic API: `Scene`, `SceneNode`, `Shape`, `Path`, `BezierContour`, `BezierPathSegment`, `Stroke`, `SolidFill`, `GradientFill`, `PathProperties`, `SVGParser`.
  - Static methods include `VectorUtils.TessellateScene`, `BuildSprite`, `BuildVectorImage`, `FillMesh`, `GenerateAtlas(AndFillUVs)`, `FillUVs`, `MakeCircleShape`, `MakeEllipseShape`, `MakeRectangleShape`, `MakeArc`, `TessellatePath`, `RenderSprite`, `RenderSpriteToTexture2D(sprite, w, h, mat, samples)`. That is 46 methods in all, per the [6000.6 reference](https://docs.unity.com/en-us/engine/6000.6/script-reference/unity/vectorgraphics/vectorutils).
  - The built-in module also has `SVGParser.ImportSVG(TextReader, ...)`, which returns a `SceneInfo`. That lets you parse an SVG string at runtime: `SVGParser.ImportSVG(new StringReader(svg))`. See the [SVGParser 6000.6 reference](https://docs.unity.com/en-us/engine/6000.6/script-reference/unity/vectorgraphics/svgparser).
  - Typical flow:
    `var geoms = VectorUtils.TessellateScene(scene, new VectorUtils.TessellationOptions{ StepDistance=10, MaxCordDeviation=0.1f, MaxTanAngleDeviation=0.1f, SamplingStepSize=0.01f });`
    then `VectorUtils.FillMesh(mesh, geoms, 100f, false)` or `VectorUtils.BuildSprite(geoms, 100f, VectorUtils.Alignment.Center, Vector2.zero, 16, false)`.
    Source: [Unity manual, work with vector graphics](https://docs.unity.com/en-us/engine/current/manual/uitoolkits/uielements/uie-ui-renderer/work-with-vector-graphics).
- **uGUI and Sprite import types** require the separate package **`com.unity.vectorgraphics` `3.0.0-preview.7`**. Its registry metadata says `unity: 6000.3`, and it depends on `com.unity.modules.vectorgraphics` and `com.unity.ugui 2.0.0`. That package provides the `SVGImage` uGUI Graphic.
  - Install: `"com.unity.vectorgraphics": "3.0.0-preview.7"`. This is a preview package. Source: [package manual 3.0](https://docs.unity3d.com/Packages/com.unity.vectorgraphics@3.0/manual/index.html).
  - A plain `Image` ignores the per-vertex colors that a vector sprite stores, which is why `SVGImage` exists.
  - Alternative: write your own `MaskableGraphic.OnPopulateMesh` that copies the `VectorUtils.Geometry` vertices, indices and colors into a `VertexHelper`. This needs only the built-in module.
- **Anti-aliasing:** basic tessellation produces hard triangle edges and relies on MSAA.
  - Unity staff have said they "rely on MSAA for edge antialiasing" ([forum](https://discussions.unity.com/t/new-svg-importer-pixelation-vectorgraphics-package/706286)).
  - The editor `TessellationMode` docs say the triangulation mode needs AA "achieved using other methods, such as MSAA" ([6000.4 ref](https://docs.unity3d.com/ScriptReference/Unity.VectorGraphics.Editor.TessellationMode.html)).
  - The AA-capable "antialiased arc encoding" mode targets **UI Toolkit VectorImage**, not uGUI.
  - Conclusion: in this project (MSAA off, Overlay canvas), the edges will be **jagged**. The only workarounds are rendering to a supersampled RenderTexture (`RenderSpriteToTexture2D` with samples > 1) or adding an alpha-feather fringe yourself.
- **Per-frame performance:** `TessellateScene` runs on the CPU in managed code and allocates new lists and arrays on every call. A handful of small curves per frame works. Re-tessellating every limb of several characters at 60fps adds GC pressure and costs milliseconds. Use it for static backgrounds or one-off bakes, not for animated rubber-hose limbs.

## 2. SDF / procedural anti-aliased shapes in uGUI
| Option | License / price | Install | Unity 6 | Notes |
|---|---|---|---|---|
| Unity UI Extensions `com.unity.uiextensions` **3.0.0** (2026-06-19, `unity: 6000.0`) | BSD-3-Clause | OpenUPM (below) | yes | `UILineRenderer`, `UICircle`, `UIPolygon`, Bezier. These are geometry only and **not AA** (jagged in this project). Useful for prototyping, not for the final look. [site](https://unity-ui-extensions.github.io/), [OpenUPM](https://openupm.com/packages/com.unity.uiextensions/) |
| MPUIKit (Modern Procedural UI Kit) 1.2.3 | Asset Store EULA (free, but needs Asset Store login) | Asset Store | yes | SDF shapes (circle, rectangle, N-gon, star) with AA. UI widgets only; no curved limbs. [store](https://assetstore.unity.com/packages/tools/gui/modern-procedural-ui-kit-163041) |
| Shapes by Freya Holmér | paid (about $100), Asset Store | Asset Store | yes (listing shows 6000.0) | Excellent AA lines, discs and polylines with immediate-mode `Draw`. Built for world/camera space, not native uGUI Graphics (no RectMask2D or canvas batching). Overkill and paid. [store](https://assetstore.unity.com/packages/tools/particles-effects/shapes-173167) |
| `com.gilzoide.rounded-corners` 1.1.0 | Unlicense | OpenUPM | probably | Rounded rectangles only. |
| `com.coffee.ui-effect` 5.12.0, `com.coffee.softmask-for-ugui` 3.7.0 | MIT | OpenUPM | yes | Effects and soft masks (shadow, outline, transition, dissolve), not shape drawing. Panel pop-in transitions could use UIEffect, but this is optional. |

**Recommended approach: a custom SDF Graphic (already started with InkShape.shader).**
- C#: subclass `MaskableGraphic`. In `OnPopulateMesh(VertexHelper vh)`, emit one quad per primitive and pack the shape parameters into `uv0..uv3` (`UIVertex.uv0..uv3` are `Vector4`). For a rubber-hose limb, use a quadratic or cubic Bezier "sausage" SDF (distance to Bezier minus thickness), with the outline handled as `d - outlineWidth`.
- Enable the extra channels on the Canvas, or `uv1..uv3` get stripped:
  `canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | TexCoord2 | TexCoord3;`
  (Normal and Tangent are also available.)
- Shader (CGPROGRAM works under URP for UI):
  - `#include "UnityCG.cginc"` and `#include "UnityUI.cginc"`
  - `#pragma multi_compile_local _ UNITY_UI_CLIP_RECT`
  - `#pragma multi_compile_local _ UNITY_UI_ALPHACLIP`
  - Declare `float4 _ClipRect;` and `float _UIMaskSoftnessX/Y` if you want soft RectMask2D. In frag: `col.a *= UnityGet2DClipping(worldPos.xy, _ClipRect);`, and under ALPHACLIP: `clip(col.a - 0.001);`.
  - Keep the Stencil block and properties (`_StencilComp`, `_Stencil`, `_StencilOp`, `_StencilWriteMask`, `_StencilReadMask`, `_ColorMask`) so `Mask` works.
  - Keep `ZTest [unity_GUIZTestMode]`, `Blend SrcAlpha OneMinusSrcAlpha`, `Cull Off`, `ZWrite Off`.
- Gotchas:
  - **fwidth AA:** `a = saturate(0.5 - d / fwidth(d))`. Use `max(fwidth(d), 1e-4)`. Compute d in pixel units for a uniform 1px edge at any scale. `fwidth` is computed per 2x2 quad, so very thin features (under 1px) shimmer; clamp the minimum thickness. Expand the quad by a few pixels of padding so the AA fringe and outline are not cut off.
  - **Sharp 2-tone shading boundary:** use the same trick on a second SDF (the light-offset shape), `saturate(0.5 + ds/fwidth(ds))`. This gives a crisp but non-aliased terminator.
  - **Linear color space:** Canvas vertex colors are converted to linear on the CPU by default. If `Canvas.vertexColorAlwaysGammaSpace` is true, the shader must convert: `if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace()) v.color.rgb = UIGammaToLinear(v.color.rgb);`. InkShape already does this. Colors set in shader *properties* (like `_ShadeMul`) are converted automatically when tagged as Color. Raw floats packed into UVs are **not** converted, so do not pack colors into UVs, or convert them yourself. Alpha blending of AA edges happens in linear space and makes dark outlines look slightly thinner or lighter than in gamma tools; this is acceptable. [Canvas.vertexColorAlwaysGammaSpace](https://docs.unity3d.com/ScriptReference/Canvas-vertexColorAlwaysGammaSpace.html), [forum on custom shaders](https://discussions.unity.com/t/published-default-ui-shader-differs-from-actual-default-ui-shader/1556710)
  - Add the shader to `Resources` or Always Included Shaders so `Shader.Find` works in builds. InkShape lives in Resources already.
  - In batchmode screenshots, the Overlay canvas is not in camera captures. Use a Screen Space - Camera canvas, or `ScreenCapture`, after a frame.

## 3. Tweening
| Lib | Version | License | Install (no Asset Store login) | Unity 6 |
|---|---|---|---|---|
| **LitMotion** (AnnulusGames) | 2.0.2 (OpenUPM, 2026-05-10) | MIT | OpenUPM scoped registry, or git `https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion` | yes (DOTS-style, zero-alloc; needs `com.unity.burst` and `com.unity.collections`, pulled automatically) |
| **PrimeTween** (Kyrylo Kuzyk) | 1.5.3 on npm (2026-11-17) | custom license: free for commercial use, no reselling; derivative projects only if installed via npm | npm scoped registry (below). The OpenUPM `com.kyrylokuzyk.primetween` 1.3.3 is a stale third-party mirror; do not use it. | yes |
| DOTween (Demigiant) | 1.2.x | free with a custom license (Pro is paid) | No official UPM or OpenUPM package. The Asset Store or a zip from dotween.demigiant.com, plus a setup panel. | yes, but legacy and allocates |

Sources: [LitMotion](https://github.com/annulusgames/LitMotion), [LitMotion OpenUPM](https://openupm.com/packages/com.annulusgames.lit-motion/), [PrimeTween GitHub](https://github.com/KyryloKuzyk/PrimeTween), [DOTween download](https://dotween.demigiant.com/download).

PrimeTween manifest:
```json
"scopedRegistries": [{ "name": "npm", "url": "https://registry.npmjs.org/", "scopes": ["com.kyrylokuzyk"] }],
"dependencies": { "com.kyrylokuzyk.primetween": "1.5.3" }
```

**Verdict:** idle bob, blink, breathing, squash and stretch are continuous procedural motion. Write them as `Mathf.Sin` plus a few easing functions (EaseOutBack, EaseOutElastic) in `Update`; that is about 50 lines and needs no dependency. A library pays off for **sequencing** the comic strip: panel N pops in (scale 0 to 1.1 to 1 with EaseOutBack), then a delay, then the next panel, with callbacks and cancellation. PrimeTween has the nicest API (`Sequence.Create().Chain(Tween.Scale(...))`, `Tween.UIAnchoredPosition`) and works with plain objects. LitMotion is MIT, so licensing is simplest. Both are fine; pick **PrimeTween** for its ergonomics, or LitMotion if MIT matters. Skip DOTween.

## 4. 2D Animation (`com.unity.2d.animation`)
- For 6000.6, the matching version is **16.0.1** (`unity: 6000.6`). Version 17.x needs 6000.7. It pulls in `2d.common`, `2d.sprite` and collections. IK lives in the same package (`UnityEngine.U2D.IK`, the old `2d.ik` package was merged into it).
- It works through `SpriteSkin`, which deforms **SpriteRenderer** meshes from a PSB or sprite rigged with bones. It does **not** drive uGUI `Image` or a custom `Graphic`, and it needs authored sprite art and bone weights, which contradicts the code-generated, no-art requirement.
- For rubber hose, the limb *is* a Bezier: shoulder, control point, hand. Animating 2-3 control points per limb in code is simpler and more on-style than IK. **Skip.**

## 5. AI-agent tooling
| Tool | Version | License | Install | Needs open Editor? | Value for a batchmode agent |
|---|---|---|---|---|---|
| **CoplayDev/unity-mcp** ("MCP for Unity") | 10.3.0 (OpenUPM `com.coplaydev.unity-mcp`, 2026-10-04) | MIT | git `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`, then Window > MCP for Unity > Configure All Detected Clients. Needs Python 3.10+ and `uv`. | Yes: a bridge inside a running Editor (about 50 tools for scenes, GameObjects, scripts, assets, console, tests, screenshots) | Gives live inspection and screenshots without a 30-60s batchmode relaunch per iteration. Requires the user to keep the Editor open. [repo](https://github.com/CoplayDev/unity-mcp) |
| **IvanMurzak/Unity-MCP** ("AI Game Developer") | 1.13.0 (OpenUPM `com.ivanmurzak.unity.mcp`) | Apache-2.0; bundled RQL/Code-Exec under PolyForm Shield | `openupm add com.ivanmurzak.unity.mcp`, or `npm i -g unity-mcp-cli` then `unity-mcp-cli install-plugin <proj>` and `unity-mcp-cli setup-mcp claude-code <proj> --transport stdio` | Tools run in the Editor. The CLI can start a **headless editor** for `compile`, `test` and `rql`. | Closest to the current batchmode workflow. It offers `script-execute` (run C# in the Editor), `tests-run` and `screenshot-isolated`. The default transport is cloud (login required); use `--transport stdio` for local. [repo](https://github.com/IvanMurzak/Unity-MCP) |
| **Unity official MCP** (inside `com.unity.ai.assistant` 2.22.0-pre.1) | pre-release | Unity terms; **requires a Unity AI subscription** | Package Manager, then Project Settings > AI > Unity MCP. A relay binary in `~/.unity/relay/`. Each client must be approved. | Yes | Official, but needs a paid subscription and is pre-release. [docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.6/manual/integration/unity-mcp-get-started.html), [blog](https://unity.com/blog/unity-ai-mcp-how-to-get-started) |
| **Unity's plugin for Claude Code** (official skills + Unity CLI) | 2026 | Unity | `/plugin marketplace add Unity-Technologies/unity-agent-plugin` then `/plugin install unity@unity-agent-plugin`. Editor-control skills also need the Unity CLI plus `unity pipeline install` and an open Editor. | Partly | Unity's own docs say: "For simple uGUI work, the plugin makes little difference." Mostly API-correctness hints. [docs](https://docs.unity.com/en-us/ai/unity-plugin/claude-code) |
| Community skills (newtro/UnityPlugin, ArkTarusov/unity-cli-plugin, skills.sh entries) | various | mostly MIT | `/plugin marketplace add ...` or `npx skills add ...` | n/a | Generic Unity advice, nothing for procedural art. Low value; review them before trusting. |

No skill or plugin was found that specifically helps with procedural or SDF cartoon art in Unity.

**For this agent:** the current loop (batchmode tests plus screenshots) already works. An MCP server only helps if the user keeps the Editor open. Then it saves the relaunch cost and allows live tweaking of curve parameters and instant screenshots. That is useful for art iteration, but it is not required, and it adds a dependency (Python/uv or Node) plus an Editor-side package to the project.

---

## Final recommendation
**Install:**
1. **Nothing for drawing.** Extend the existing `InkShape.shader` and SDF `MaskableGraphic` with a Bezier-capsule (limb), circle (glove, eye), rounded-box and polygon (background layer) SDF. Enable `TexCoord1-3` on the Canvas. This is the only path that gives AA edges with MSAA off on an Overlay canvas, at 60fps, with RectMask2D clipping for the scrolling strip. It costs nothing and adds no dependency.
2. **PrimeTween 1.5.3** via the npm scoped registry, for panel pop-in sequencing and gesture tweens. LitMotion 2.0.2 (MIT, OpenUPM) is the alternative if a permissive license matters. Keep idle bob, blink and squash as hand-written sine and easing code.
3. *(Optional, only if the user keeps the Editor open during art iteration)* **IvanMurzak Unity-MCP 1.13.0** with `--transport stdio`, which fits a headless CLI workflow, or **CoplayDev unity-mcp 10.3.0** (MIT).

**Skip:**
- Vector Graphics, both the built-in module and the 3.0 preview package: no AA without MSAA, and per-frame CPU re-tessellation allocates. Use it only for static SVG bakes, if ever.
- Unity UI Extensions (no AA).
- MPUIKit and Shapes (Asset Store login, paid or closed, wrong fit for limbs).
- DOTween.
- 2D Animation/IK (SpriteRenderer-only, needs art).
- Unity official MCP (subscription).
- Generic Claude Code Unity skills.
