# Nguồn và giấy phép

Mọi tài sản đưa vào bản build phải có dòng ở đây (impl plan sprint 7 F-061 AC-5; constitution Điều 6).

## Âm thanh (sprint 7)

| Tài sản | Nguồn | Giấy phép |
| --- | --- | --- |
| 24 tiếng hiệu ứng: `buoc_chan_0..3`, `nhet_ve`, `thung_roi`, `cam`, `dat`, `thai`, `cho_vao_bep`, `xeo_loop`, `soi_loop`, `chin`, `muc`, `chuong`, `tien`, `khach_vui`, `khach_gian`, `mua_relic`, `thu_no`, `bac_4`, `bac_5`, `sot_tot`, `sot_hong`, `chay_khet`, `an`, `bam` | Tự tạo bằng code: `tools/am-thanh/make_audio.py` (numpy, tổng hợp sóng + lọc nhiễu, seed cố định, không dùng mẫu âm thanh ngoài) | Thuộc dự án |
| Sau sprint 10: `cau_vong` (sốt ra Huyền thoại), `bom_xit` (sốt hỏng) | Tự tạo bằng code: `tools/am-thanh/make_audio.py --new` | Thuộc dự án |
| Giọng 3 khách đặc biệt: `khach_vip`, `khach_streamer`, `khach_fan_quy_do` (Resources/Audio, phát 7 giây đầu) | **Bạn đưa 23/11/2026** (`tools/am-thanh/*-made-with-Voicemod.mp3`, làm bằng Voicemod) | Do bạn cung cấp; kiểm lại điều khoản Voicemod / nội dung gốc trước khi phát hành công khai |
| 3 bản nhạc nền: `nhac_phong` (92 bpm, Fmaj7–Dm7–Gm7–C7), `nhac_mat_dien` (60 bpm, hộp nhạc thưa), `nhac_ngay21` (116 bpm, ostinato căng) | Tự tạo bằng code: `tools/am-thanh/make_audio.py` | Thuộc dự án |

File nằm ở `My project/Assets/QuanAnHope/Resources/Audio/*.wav` (Git LFS). Chạy lại script ra y hệt. Muốn thay bằng tiếng thu âm hoặc tải về (ví dụ CC0 từ freesound.org, Kenney.nl): giữ nguyên tên file, ghi nguồn và giấy phép từng file vào bảng trên.

## Phông chữ

| Tài sản | Nguồn | Giấy phép |
| --- | --- | --- |
| Baloo 2 Bold (`Resources/Fonts/Baloo2-Bold.ttf`) | The Baloo 2 Project Authors, github.com/EkType/Baloo2 | SIL Open Font License 1.1 (`OFL_Baloo2.txt` cạnh file phông) |
| Be Vietnam Pro (chỉ trong Art Bible / tranh cốt truyện) | `docs/art-bible/fonts/` | SIL Open Font License 1.1 (`OFL_BeVietnamPro.txt`) |

## Thư viện

| Thư viện | Dùng cho | Giấy phép |
| --- | --- | --- |
| PrimeTween 1.5.3 (Kyrylo Kuzyk), cài qua npm scoped registry | Chuyển động tranh cốt truyện | Giấy phép riêng của PrimeTween: được dùng trong game thương mại, không được phân phối lại mã nguồn / tarball (xem `license.md` trong gói) |
| Newtonsoft Json (Unity) | Nhập `balance.json` trong Editor | MIT |
| Unity Test Framework, URP, Input System, uGUI, TextMeshPro | Engine | Unity Companion License |

## Hình ảnh

Toàn bộ hình trong game dựng bằng code hoặc vẽ theo Art Bible của dự án (`docs/art-bible/`). Tranh cốt truyện từ `CotTruyen_BepNhanPham.zip` của bạn (`tools/cot-truyen/`).

## Công cụ (sprint 10, F-093 AC-3)

| Công cụ | Dùng cho |
| --- | --- |
| Unity 6000.6.9f1 (URP 17) | Engine, build Windows |
| Blender 5.3 | Model nguyên liệu, dụng cụ, món ăn, nhân vật có xương — dựng bằng script `SourceArt/Blender/scripts/*.py` |
| Python 3 (numpy) | Mô phỏng cân bằng `balance/`, tạo âm thanh `tools/am-thanh/` |
| Node.js + Microsoft Edge (playwright-core) | Xuất tranh cốt truyện `tools/cot-truyen/` |
| Claude Code (Anthropic) | Trợ lý lập trình |
