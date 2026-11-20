# Tools for an animated layered paper-craft comic cutscene system (research notes, 2026-11-19)

Project facts checked locally: Unity **6000.6.9f1**, URP **17.6.0**, `com.unity.ugui` **2.6.0** (TextMeshPro is inside it), built-in module `com.unity.modules.vectorgraphics`, **PrimeTween 1.5.3 already installed** (npm scoped registry `com.kyrylokuzyk`). Not installed: Timeline, Cinemachine, 2D Sprite, SoftMask.

npm registry (checked with `npm view` on 2026-11-19): playwright-core 1.65.1, playwright 1.65.1, @resvg/resvg-js 2.6.2 (no new release since 2024), sharp 0.35.5, @playwright/mcp 0.0.86.

## 1. SVG -> PNG layer export

| Option | Filter fidelity | Notes |
|---|---|---|
| **playwright-core + `channel: 'msedge'`** | Highest: Chromium/Blink is the engine the SVGs are previewed in, so output matches what the author sees. Full SVG 1.1 filter set (feTurbulence, feMorphology, feDisplacementMap, feComposite, lighting). | `playwright-core` never downloads browsers. `chromium.launch({ channel: 'msedge', headless: true })` uses the installed Edge. `page.screenshot({ omitBackground: true, clip })` or `locator.screenshot({ omitBackground: true })` gives transparent PNG. The same page can run `getBBox()` / `getScreenCTM()` to export text positions to JSON. |
| @resvg/resvg-js 2.6.2 | Good. resvg's unsupported list has no filter primitives (only fonts, glyphs, kerning, `color-profile`, external `use`). feTurbulence and similar render but can differ slightly from Chromium at the pixel level (noise, filter region, blur). | Fast, no browser. Wrapper is old (2.6.2, 2024) and bundles an older resvg. Text/fonts must be loaded explicitly, which does not matter here because text is removed. |
| sharp 0.35.5 (libvips + librsvg) | Medium/good: librsvg implements the filters, but the prebuilt librsvg version and font handling vary. | Best for post-processing (trim, resize, WebP, premultiply), not as the reference renderer. |

**Verdict:** use playwright-core + Edge as the reference renderer. Optionally use resvg-js for quick CI previews or diffs. Use sharp only to trim and pack the PNGs afterwards.

Pipeline tips:
- For each layer N: hide every `<g filter="url(#ppK)">` except N (`style.display='none'`), hide `<text>` and balloon text, then take the screenshot. Use `deviceScaleFactor: 2` for crisp output.
- A filter region (`x/y/width/height` on `<filter>`) can clip displacement or turbulence. Use a generous region such as `-20%/140%`.
- Export the layer's bbox together with its PNG so Unity can place it at the correct offset after trimming.
- Text JSON: `{panel, id, text, x, y, w, h, fontSize, anchor, rotation, fill}` from `getBBox()` mapped through `getScreenCTM()`.

Install: `npm i -D playwright-core` (optional `npm i -D @resvg/resvg-js sharp`).

Sources:
- https://playwright.dev/docs/browsers (msedge channel; Edge not installed by default; `npx playwright install msedge` would REPLACE the system Edge, so do not run it)
- https://playwright.dev/docs/api/class-page#page-screenshot (omitBackground)
- https://github.com/linebender/resvg and https://raw.githubusercontent.com/linebender/resvg/main/docs/unsupported.md
- https://github.com/thx/resvg-js
- https://sharp.pixelplumbing.com/

## 2. Unity packages

- **Vector Graphics / SVG importer:** Unity 6.3+ has a built-in Vector Graphics module (already in the manifest). Package `com.unity.vectorgraphics` 3.0.x-preview is needed only for the Sprite/uGUI import types. The SVG importer tessellates shapes and does **not** support SVG filters or text, so it is unsuitable for the paper look. Import PNGs instead. Known bug UUM-133203: importer settings are lost when upgrading to 6.3+.
  https://docs.unity3d.com/Packages/com.unity.vectorgraphics@3.0/manual/index.html ; https://issuetracker.unity3d.com/issues/svg-importer-settings-are-lost-when-upgrading-to-unity-6-dot-3
- **Timeline (`com.unity.timeline`):** worth installing. It provides authoring for beats/panels, Signal tracks to trigger shake/zoom, and custom PlayableTracks. Its "timelinefoundation" module is already present, but the package itself still has to be added.
- **Cinemachine 3 (`com.unity.cinemachine` 3.1.x):** optional. It is useful for Impulse (screen shake via Impulse Source plus Listener) and for blending between per-panel virtual cameras with world-space layers. Overkill if the cutscene is pure UI. Hand-written shake/punch-in with PrimeTween is simpler and easier to make "stepped".
  https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulseSource.html
- **URP post-processing:** Volume overrides cover DoF (Gaussian/Bokeh), Film Grain, Vignette and Chromatic Aberration. Caveats:
  - Screen Space **Overlay** canvases are not post-processed. Use Screen Space **Camera** or world space, or render to a RenderTexture.
  - URP DoF reads the **depth buffer**. Transparent sprites and UI do not write depth, so DoF will not separate the 6 layers. Fake DoF instead: blur per layer (pre-blurred PNG variants from the browser via `filter: blur()` or a Kawase blur material per layer) whose radius depends on distance from the focus layer. Use the URP Volume only for grain and vignette.
  - Apply post-processing only on the last camera of a stack.
  https://docs.unity3d.com/Manual/urp/depth-of-field-volume-override.html ; https://docs.unity3d.com/Manual/urp/cameras/camera-stacking-concepts.html ; https://discussions.unity.com/t/urp-render-after-taa/947670
- **2D Sprite (`com.unity.2d.sprite`) / Sprite Shape:** Sprite gives the Sprite Editor with a custom outline and a tight mesh, which reduces overdraw for large transparent PNGs. Sprite Shape is not needed. Panel polygons are better as a simple mesh mask or SpriteMask.
- **TextMeshPro:** merged into `com.unity.ugui` since 2023.2. In this project it is ugui 2.6.0. Import via Window > TextMeshPro > Import TMP Essential Resources. Vietnamese needs a font asset with a Vietnamese character set (dynamic SDF from e.g. Be Vietnam Pro / Nunito).
  https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html ; https://discussions.unity.com/t/textmesh-pro-in-unity-6/1580163
- **Tweens:** **PrimeTween is already installed**. It allocates nothing, has Sequences and Shake (`Tween.ShakeLocalPosition`, `Tween.PunchScale`), and coexists with DOTween. For "on twos" stepping, quantise time with `Mathf.Floor(t*12)/12` in a custom `Tween.Custom` or drive values from a 12 fps ticker. DOTween (free) is not needed.
  https://github.com/KyryloKuzyk/PrimeTween ; https://dotween.demigiant.com/
- **Polygon masks:**
  - Built-in `Mask` (stencil, hard edge) with a polygon sprite, or a custom `Graphic` that emits the panel polygon. Free, and good for hard comic panel borders.
  - `SpriteMask` for world-space SpriteRenderers.
  - **SoftMaskForUGUI 3.7.0** (mob-sakai) for soft or anti-aliased edges, rotation, 4 nesting levels, and TMP support via its `textmeshpro` sub-package (use the 2023.3+ variant for Unity 6). OpenUPM: `openupm add com.coffee.softmask-for-ugui`. Git: `https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.7.0`.
  https://github.com/mob-sakai/SoftMaskForUGUI

## 3. Claude Code side

| Tool | What | Maturity / notes |
|---|---|---|
| **Unity official plugin for Claude Code** (Unity-Technologies/unity-agent-plugin) | 29 skills (UI/TMP, 2D sprites/atlases, URP post-processing, Shader Graph, render graph...). Drives the Editor through the Unity CLI. Skills only, no bundled MCP. | First-party, released 2026-09-09. Needs Unity CLI + `unity pipeline install` + an open Editor. `/plugin marketplace add Unity-Technologies/unity-agent-plugin` then `/plugin install unity@unity-agent-plugin`. https://unity.com/blog/unity-plugin-for-claude-code ; https://docs.unity.com/en-us/ai/unity-plugin/claude-code |
| Unity MCP (official, in `com.unity.ai.assistant`) | Relay MCP server inside the Editor | Open beta (May 2026). Needs a Unity Cloud project and a Unity AI trial/subscription. https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.6/manual/integration/unity-mcp-get-started.html |
| **CoplayDev unity-mcp ("MCP for Unity")** | ~50 tools: scenes, GameObjects, scripts, assets, tests, VFX/animation/UI groups | Most popular community option, v10.3.0 (2026-10-04), MIT. Unity 2021.3 to 6.x, needs Python 3.10+ via `uv`. Git URL `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` then Window > MCP for Unity > Configure All Detected Clients. https://github.com/CoplayDev/unity-mcp ; https://coplaydev.github.io/unity-mcp/getting-started/install |
| IvanMurzak Unity-MCP ("AI Game Developer") | Editor + runtime tools, custom tools via `[AiTool]`, `screenshot-isolated` | Active. Unity 2022.3+ (CI on 6000.6). The default is a cloud relay (ai-game.dev). Use `--transport stdio` to stay local. `npm i -g unity-mcp-cli`; `unity-mcp-cli install-plugin ./proj`; `unity-mcp-cli setup-mcp claude-code ./proj --transport stdio`. https://github.com/IvanMurzak/Unity-MCP |
| CoderGamester mcp-unity | Scene/GameObject/component/material/menu/tests/console/play mode, batching with rollback | Unity 6+, Node 18+, v1.5.x. Git URL `https://github.com/CoderGamester/mcp-unity.git`, then Tools > MCP Unity > Server Window > Configure Claude Code (Project), which writes `.mcp.json`. The path contains the PackageCache hash, so re-run Configure after each update. https://github.com/CoderGamester/mcp-unity |
| **Playwright MCP** | Claude opens a browser, views or screenshots the SVG to check layers | `claude mcp add playwright -- npx @playwright/mcp@latest --browser msedge --headless` (v0.0.86). https://github.com/microsoft/playwright-mcp |
| Anthropic skills repo | `canvas-design`, `algorithmic-art`, `webapp-testing` (Playwright scripts), `skill-creator`, `mcp-builder`, `frontend-design`... No dedicated SVG skill. | `/plugin marketplace add anthropics/skills` then `/plugin install example-skills@anthropic-agent-skills`. webapp-testing is the most useful (a Playwright pattern for exporting/verifying). A custom skill "svg-layer-export" could be written with skill-creator. https://github.com/anthropics/skills |

Notes:
- The repo already uses `.mcp`-free Editor scripts (BuildNhaHang.cs, batch-mode bot). Batch mode `Unity.exe -batchmode -executeMethod` remains a no-MCP route for importing and building cutscene prefabs.
- Install only one Unity MCP bridge at a time. They overlap and each one opens a port or socket in the Editor.

## 4. Recommendation

Now:
1. `npm i -D playwright-core` (Edge channel). Export script: layers -> PNG, text -> JSON.
2. Unity: `com.unity.timeline` (via Package Manager / manifest) and `com.unity.2d.sprite`. TMP Essential Resources plus a Vietnamese SDF font. Keep PrimeTween.
3. Masks: built-in stencil Mask / custom polygon Graphic first.
4. Claude: `claude mcp add playwright -- npx @playwright/mcp@latest --browser msedge --headless`. Unity official plugin (skills) if the Unity CLI is acceptable.

Later (only if needed):
- SoftMaskForUGUI (soft or anti-aliased panel edges); Cinemachine 3 (if moving to world-space camera moves/Impulse); a Unity MCP bridge (CoplayDev, MIT, local); @resvg/resvg-js for fast previews; sharp for trim/WebP.

Manifest entries (versions to confirm in Package Manager for 6000.6):
```json
"com.unity.timeline": "1.8.x",
"com.unity.2d.sprite": "1.0.0",
"com.unity.cinemachine": "3.1.x",
"com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.7.0"
```
OpenUPM alternative: add the scope `com.coffee` to a scoped registry `https://package.openupm.com`.
