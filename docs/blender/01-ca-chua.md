# Blender bài 1: quả cà chua (F-063, sprint 7)

Dành cho: bạn, lần đầu dùng Blender. Mỗi ngày 30–45 phút, 7 ngày. Chưa đưa gì vào game ở sprint này (sprint 8–9 mới thay model).

Hình mẫu: Art Bible ảnh 08 (`docs/art-bible/images/08_nguyen_lieu_theo_bac.png`), quả cà chua bậc 1 "Bình thường": thân tròn hơi dẹt màu San hô `#E8604F`, cuống lá 4 cánh màu Lá `#7DB87A`, viền mực `#2B2321` (viền do shader Unity vẽ, **không** dựng viền trong Blender). Mặt là ảnh 2D dán sau (Điều 7), không nặn mặt.

> Ảnh chụp màn hình: hướng dẫn này chỉ có chữ vì máy làm hướng dẫn không chạy được Blender. Bạn làm tới đâu chụp màn hình tới đó, lưu vào `docs/blender/anh/` theo số bước (ví dụ `01-3.png`), để lần sau mình chú thích lên ảnh của chính bạn.

## Ngày 1 · Cài đặt và di chuyển trong khung nhìn

1. Tải Blender bản **LTS** mới nhất ở blender.org/download/lts (cài cho mọi người dùng, không cần quyền gì thêm).
2. Mở Blender, chọn "General". Bấm vào khối lập phương có sẵn rồi nhấn `X` → Delete: ta bắt đầu từ cảnh trống.
3. Học 3 thao tác nhìn (chuột phải không dùng tới):
   - Giữ **nút giữa chuột** rồi kéo: xoay quanh.
   - Lăn chuột: phóng to / thu nhỏ.
   - `Shift` + giữ nút giữa: trượt khung nhìn.
   - Phím số `1`, `3`, `7` (bàn phím số) hoặc thanh trục màu ở góc phải trên: nhìn trước / bên / trên. Laptop không có bàn phím số: Edit → Preferences → Input → bật "Emulate Numpad".
4. `Ctrl + S`: lưu thành `SourceArt/Blender/ca_chua.blend` (thư mục `SourceArt/` ngoài `My project/`, Art Bible §14).

Đạt khi: xoay, phóng, nhìn thẳng từ trước / trên mà không phải nghĩ.

## Ngày 2 · Dựng khối: cầu, trụ, hộp

1. `Shift + A` → Mesh → **UV Sphere**. Ô nhỏ hiện ở góc trái dưới ("Add UV Sphere"): Segments 24, Rings 12, Radius 0,05 m (quả cà chua thật trong game cao khoảng 10 cm).
2. Ba phím biến đổi (gõ phím rồi di chuột, bấm trái để chốt, phải để huỷ):
   - `G` di chuyển, `R` xoay, `S` co giãn. Thêm trục: `S` rồi `Z` = chỉ co theo chiều cao. Gõ số ngay sau: `S Z 0.85` Enter.
3. Thử thêm **Cylinder** (`Shift + A`) và **Cube**; kéo, xoay, co từng khối; xoá đi khi xong (`X`).
4. **Bo góc** hộp: chọn Cube, `Tab` vào Edit Mode, `Ctrl + B` (Bevel) rồi lăn chuột tăng số đoạn lên 3. `Tab` để ra.

Đạt khi: làm được một hộp bo góc và một quả cầu dẹt mà không tra lại phím.

## Ngày 3 · Tô màu phẳng theo bảng màu

1. Chọn quả cầu → khung bên phải, thẻ **Material** (quả bóng đỏ) → New.
2. Base Color: bấm ô màu → Hex → `E8604F`. Roughness 1, Specular 0 (màu phẳng; ánh sáng thật do shader `ToonFlat` của Unity làm).
3. Thêm material thứ hai `7DB87A` cho cuống lá (tối đa 3 material mỗi mesh, Art Bible §14).
4. Đặt tên theo quy ước: `M_Toon_SanHo`, `M_Toon_La`.
5. Bấm `Z` → Material Preview để thấy màu.

Mã màu lấy ở `docs/art-bible/palette/palette.json` (hoặc nạp `QuanAnHope.gpl` vào bảng màu của Blender / Krita).

## Ngày 4 · Dựng quả cà chua theo ảnh 08, shade smooth

1. Thân: UV Sphere ở ngày 2. `S Z 0.85` cho hơi dẹt. Trong Edit Mode chọn đỉnh trên cùng (`1` = chế độ chọn đỉnh), `G Z -0.008`: tạo lõm nhẹ chỗ cuống.
2. Cuống lá: `Shift + A` → Cylinder, Vertices **8**, Radius 0,02, Depth 0,004 → đặt lên đỉnh. Edit Mode, chọn xen kẽ 4 đỉnh vòng ngoài, `S` kéo ra 1,6 lần: thành ngôi sao 4 cánh như ảnh. Thêm một trụ nhỏ (Radius 0,004, Depth 0,012) làm cọng.
3. Chọn thân + lá + cọng (`Shift` + bấm), `Ctrl + J` gộp thành một mesh; đặt tên `SM_CaChua`.
4. Chuột phải → **Shade Smooth** (hoặc Shade Auto Smooth): mặt tròn, không thấy cạnh.
5. **Gốc toạ độ ở chân vật, giữa đáy** (Art Bible §14): `Shift + S` → Cursor to World Origin; đặt quả sao cho đáy chạm mặt đất (`G Z` cho tới khi đáy chạm lưới đỏ/xanh); Object → Set Origin → Origin to 3D Cursor.
6. Kiểm ngân sách: Overlays (góc phải trên) → bật **Statistics**: số Triangles phải **≤ 600** (nguyên liệu, Art Bible §14). Quả này khoảng 500.

## Ngày 5 · Xuất `.fbx`, kéo vào Unity

Thông số xuất (Art Bible §14, ghi lại ở đây để khỏi tra):

| Mục | Đặt |
| --- | --- |
| Áp transform trước | Chọn mesh → `Ctrl + A` → All Transforms |
| File → Export → FBX | Selected Objects ✔ · Object Types: Mesh |
| Scale | 1,00 · Apply Scalings: **FBX All** |
| Forward / Up | **−Z Forward**, **Y Up** |
| Apply Transform | ✔ (thử nghiệm của Blender, đúng cho mesh tĩnh) |
| Geometry | Smoothing: **Face** · Apply Modifiers ✔ |
| Tên file | `SM_CaChua.fbx` |
| Nơi lưu | `My project/Assets/Art/Models/` (Git LFS đã bật cho `.fbx`) |

Trong Unity:

1. Mở cảnh **ThuModel**: menu `Quán Ăn Hope → Cảnh thử model Blender`. Cảnh có sàn gỗ, đèn chính như phòng, một bệ ở giữa.
2. Kéo `SM_CaChua.fbx` từ thư mục Models lên bệ. Ở Inspector của file fbx: Scale Factor 1, "Convert Units" ✔.
3. Chọn model trong cảnh → menu `Quán Ăn Hope → Gắn ToonFlat + Outline cho model đang chọn`: lệnh này thay material bằng `ToonFlat` (giữ màu Base Color) và thêm lớp viền `Outline` dày 0,018 m (đạo cụ nhỏ, Art Bible §10).
4. So với ảnh 08: màu San hô, viền mực đều, không thấy cạnh đa giác.

## Ngày 6–7 · Làm lại nhanh, thử hành tây

1. Mở file mới, làm lại cà chua **trong 20 phút** (bấm giờ). Ghi lại bước nào chậm vào cuối file này.
2. Hành tây (vòng 1): thân cầu hơi nhọn đầu (`S Z 1.15`, kéo đỉnh trên lên). Màu lấy đúng màu hành tây đang có trong game (hình đang dựng bằng code ở `My project/Assets/QuanAnHope/Game/IngredientModels.cs`) để model Blender khớp hình cũ.
3. Lỗi thường gặp (Art Bible §15): viền đứt (pháp tuyến ngược: Edit Mode → `Shift + N`), model nằm nghiêng 90° trong Unity (quên −Z Forward / Y Up), model to gấp 100 lần (quên Apply Scalings: FBX All).

## Ghi chép của bạn

- Ngày làm / thời gian / chỗ vướng:
