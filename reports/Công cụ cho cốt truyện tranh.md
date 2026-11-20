# Công cụ cho cốt truyện tranh (cutscene giấy cắt lớp)

Ngày: 2026-11-19. Ghi chú gốc kèm nguồn: `research_notes/Cong cu cot truyen tranh/tools.md`. Chưa cài gì.

Dự án hiện tại: Unity 6000.6.9f1, URP 17.6, ugui 2.6.0 (đã có TextMeshPro), đã có module Vector Graphics và **PrimeTween 1.5.3**. Chưa có Timeline, Cinemachine, 2D Sprite, SoftMask.

## Tóm tắt khuyến nghị

- **Xuất lớp:** dùng **playwright-core + Edge sẵn có** (`channel: 'msedge'`), vì nó render filter SVG giống hệt lúc xem trong trình duyệt và lấy được toạ độ chữ (`getBBox`) ngay trong cùng một trang. resvg-js chỉ làm phương án xem trước nhanh. sharp dùng để cắt và nén ảnh.
- **Unity, cài ngay:** Timeline và 2D Sprite. Import TMP Essential Resources kèm font SDF tiếng Việt. Giữ PrimeTween, không cần DOTween. Làm mask khung tranh bằng Mask stencil có sẵn.
- **Unity, để sau:** SoftMaskForUGUI (khi cần mép mềm), Cinemachine 3 (khi chuyển sang camera world-space).
- **DoF:** **không dựa vào DoF của URP**, vì sprite/UI trong suốt không ghi depth. Làm DoF giả bằng cách làm mờ theo từng lớp. Volume URP chỉ dùng cho grain và vignette.
- **Claude Code:** cài Playwright MCP (chạy bằng Edge, headless) ngay. Plugin Unity chính chủ là tuỳ chọn. Một cầu MCP Unity (CoplayDev) để sau, và chỉ cài một cái.

## Xuất lớp SVG

| Công cụ | Độ trung thực filter | Ghi chú |
|---|---|---|
| **playwright-core 1.65 + Edge** | Cao nhất. Chromium hỗ trợ đầy đủ feTurbulence, feMorphology, feDisplacementMap, feComposite. | Không tải trình duyệt về. `omitBackground: true` cho ra PNG trong suốt. Đo được bbox chữ để xuất JSON. |
| @resvg/resvg-js 2.6.2 | Tốt. resvg không liệt kê filter nào trong danh sách "không hỗ trợ". Kết quả có thể lệch vài pixel so với Chromium (nhiễu, vùng filter). | Nhanh, không cần trình duyệt. Bản wrapper cũ (2024). |
| sharp 0.35.5 (librsvg) | Trung bình–khá | Dùng để hậu xử lý: trim, resize, WebP. |

Quy trình cho mỗi lớp N:
1. Ẩn các `<g filter>` khác và ẩn toàn bộ `<text>`.
2. Chụp với `deviceScaleFactor: 2`.
3. Ghi bbox của lớp ra JSON để Unity đặt lớp đúng vị trí sau khi trim.

Nên đặt vùng filter rộng (`x=-20% width=140%`) để displacement không bị cắt.

Không chạy `npx playwright install msedge`. Lệnh này sẽ ghi đè lên Edge của hệ thống.

## Gói Unity

- **Vector Graphics / SVG importer:** importer chỉ tessellate hình, **không hỗ trợ filter và text**, nên không giữ được chất giấy. Hãy import PNG. Lỗi UUM-133203: mất cài đặt importer khi nâng cấp lên 6.3+.
- **Timeline:** dùng để dựng nhịp panel. Dùng Signal để kích hoạt rung màn hình và punch-in.
- **Cinemachine 3.1:** cung cấp Impulse (rung) và blend camera theo panel. Chỉ đáng cài khi các lớp nằm trong world-space.
- **URP post:**
  - Canvas Screen Space Overlay không bị post-process. Hãy dùng Screen Space Camera, world-space hoặc RenderTexture.
  - Chỉ bật post ở camera cuối của stack.
  - DoF đọc depth, nên không tách được 6 lớp. Làm mờ theo lớp bằng PNG mờ sẵn hoặc material blur, với bán kính tuỳ theo khoảng cách đến lớp đang lấy nét.
- **2D Sprite:** có Sprite Editor và mesh bám sát hình, giúp giảm overdraw của PNG lớn trong suốt. Không cần Sprite Shape.
- **TextMeshPro:** đã gộp vào ugui từ 2023.2. Vào Window > TextMeshPro > Import TMP Essential Resources. Cần font asset có đủ dấu tiếng Việt.
- **Tween:** PrimeTween (đã cài) có Sequence, Shake và Punch. Muốn hiệu ứng "on twos" ở 12fps thì lượng tử hoá thời gian bằng `Mathf.Floor(t*12)/12`.
- **Mask đa giác:**
  - `Mask` stencil kết hợp sprite đa giác hoặc một Graphic tự sinh polygon: miễn phí, mép cứng, rất hợp với khung truyện.
  - `SpriteMask`: dùng cho world-space.
  - **SoftMaskForUGUI 3.7.0**: cho mép mềm, xoay được, lồng tối đa 4 cấp, hỗ trợ TMP qua gói phụ `textmeshpro`.

## Claude Code skill/MCP

- **Plugin Unity chính chủ** (09/2026): 29 skill (UI/TMP, sprite/atlas, URP post, Shader Graph...), điều khiển Editor qua Unity CLI. Bản thân plugin không kèm MCP.
- **Unity MCP chính chủ** (trong `com.unity.ai.assistant`): đang beta, cần Unity Cloud và gói Unity AI, nên không ưu tiên.
- **CoplayDev unity-mcp v10.3.0:** khoảng 50 tool, MIT, cộng đồng dùng nhiều nhất. Cần Python 3.10+ qua `uv`.
- **IvanMurzak Unity-MCP:** có tool lúc runtime, tool tuỳ biến bằng `[AiTool]` và chụp ảnh. Mặc định dùng relay cloud, nên dùng `--transport stdio`.
- **CoderGamester mcp-unity v1.5:** chạy trên Unity 6+ với Node 18+, có batch rollback. Đường dẫn chứa hash PackageCache nên phải Configure lại sau mỗi lần cập nhật.
- **Playwright MCP 0.0.86:** cho Claude mở hoặc chụp SVG để kiểm tra từng lớp.
- **Skills của Anthropic:** `webapp-testing` (mẫu script Playwright) có ích nhất, ngoài ra có `canvas-design` và `algorithmic-art`. Chưa có skill SVG chuyên dụng. Có thể tự viết skill "svg-layer-export" bằng `skill-creator`.

## Lệnh cài đặt

Cài ngay:
```bash
npm i -D playwright-core
claude mcp add playwright -- npx @playwright/mcp@latest --browser msedge --headless
```
Trong Unity (Package Manager hoặc thêm vào `Packages/manifest.json`; kiểm tra lại số phiên bản trong PM của 6000.6):
```json
"com.unity.timeline": "1.8.x",
"com.unity.2d.sprite": "1.0.0"
```
Sau đó vào Window > TextMeshPro > Import TMP Essential Resources.

Plugin Unity cho Claude Code (tuỳ chọn):
```
/plugin marketplace add Unity-Technologies/unity-agent-plugin
/plugin install unity@unity-agent-plugin
```

Để sau:
```bash
npm i -D @resvg/resvg-js sharp
openupm add com.coffee.softmask-for-ugui            # hoặc git: https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.7.0
openupm add com.coffee.softmask-for-ugui.textmeshpro
# Cinemachine: "com.unity.cinemachine": "3.1.x"
# CoplayDev MCP: Package Manager > git URL https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
#   rồi Window > MCP for Unity > Configure All Detected Clients
/plugin marketplace add anthropics/skills
/plugin install example-skills@anthropic-agent-skills
```

## Rủi ro

- **DoF URP không có tác dụng với lớp trong suốt.** Phải tự làm blur theo lớp. Hãy prototype sớm.
- **Canvas Overlay bỏ qua post-process.** Chọn kiến trúc camera/RenderTexture ngay từ đầu.
- **PNG 6 lớp × nhiều panel × @2x tốn bộ nhớ.** Cần trim, đóng atlas và nén (ASTC/BC7).
- **Filter có thể bị cắt hoặc lệch giữa các renderer.** Chỉ dùng Edge làm chuẩn. Nếu Edge tự cập nhật, kết quả có thể đổi chút ít.
- **Toạ độ chữ:** bbox trong SVG khác cách TMP dàn chữ (font, xuống dòng tiếng Việt). Nên xuất cả khung bóng thoại và để TMP tự fit trong khung đó.
- **MCP Unity của cộng đồng thay đổi nhanh** và các bridge chồng chéo nhau. Chỉ cài một cái. Bản IvanMurzak mặc định gửi qua cloud.
- **SVG importer của Unity** đang có lỗi mất cài đặt khi nâng phiên bản. Không nên dựa vào nó.
- **Bộ đếm "on twos":** nếu dùng `Time.deltaTime` liên tục sẽ mất cảm giác giật hình. Cần ticker 12fps riêng và tôn trọng `timeScale`/pause.
