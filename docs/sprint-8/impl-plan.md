# Impl plan — Sprint 8: Blender đợt 1 — nguyên liệu và đồ vật

Thời gian: 27/12/2026 → 2/1/2027 (7 ngày)
Nguồn: `specs/constitution.md` v1.4 Điều 6 (model Blender xuất `.fbx`, Git LFS), Điều 7 (tròn, viền đậm, màu phẳng); Art Bible §5 (hình khối), §7 (nguyên liệu và món), §10 (viền, tô bóng), §14 (quy cách file); ghi chú của bạn 14/11: "hình ảnh các nguyên liệu để đỡ xấu và nhìn chân thực hơn".
Điều kiện vào sprint: bạn đã làm xong bài cà chua ở sprint 7. Người dựng model là **bạn**; mình viết hướng dẫn, kiểm quy cách, nối vào game.

## 1. Mục tiêu

Cuối ngày 2/1, trong game:

1. 6 nguyên liệu vòng 1 (cà chua, hành tây, rau muống, trứng, thịt heo, đậu phụ) là model Blender, có cả hình đã sơ chế.
2. Các đồ bếp hay nhìn nhất là model: chảo, nồi, vỉ nướng, thớt, đĩa, thùng gỗ.
3. Những gì chưa kịp vẫn là hình ghép khối của sprint 2; game không bị vỡ khi thiếu model.

Thứ tự ưu tiên theo số lần người chơi nhìn thấy: nguyên liệu vòng 1 → dụng cụ nấu → đĩa, thùng → nguyên liệu vòng 2 → phần còn lại.

### Ngoài phạm vi

Nhân vật (sprint 9). Animation có xương. Texture vẽ tay (dùng màu phẳng theo vật liệu).

## 2. Tính năng và tiêu chí chấp nhận

### F-070 Đường ống model → game — Điều 6, Art Bible §14

| AC | Then |
| --- | --- |
| 1 | Thả file `.fbx` đúng tên vào `Assets/QuanAnHope/Models/Ingredients/` là game tự dùng thay hình ghép khối; không sửa code cho từng model |
| 2 | Không có file thì dùng hình ghép khối (không lỗi, không vật màu hồng) |
| 3 | Công cụ kiểm trong Editor báo từng model: đúng tỉ lệ (1 đơn vị = 1 m), gốc toạ độ ở đáy, dưới ngưỡng tam giác của Art Bible §14, số vật liệu, tên đúng quy ước |
| 4 | Model nhận shader `ToonFlat` + viền 0,018 m, tô 2 tông, không bóng đổ thời gian thực (Art Bible §10) |
| 5 | Đế bậc và mặt theo bậc vẫn đúng chỗ: mỗi model khai báo "điểm mặt" (một empty tên `Face`) |

### F-071 Nguyên liệu — Art Bible §5, §7

| AC | Then |
| --- | --- |
| 1 | Mỗi nguyên liệu 2 model: nguyên và đã sơ chế (theo cột "Sơ chế" của constitution §4.5) |
| 2 | Một hình dáng cho mọi bậc; bậc đọc bằng đế, mặt, phụ kiện như cũ |
| 3 | Nhận ra được ở khoảng cách 2 m không cần nhãn: bạn và một người khác nhìn ảnh gọi đúng tên ≥ 5/6 |
| 4 | Ảnh so sánh trước / sau cho từng loại trong `docs/sprint-8/ui/` |

### F-072 Đồ bếp — Art Bible §5, §8

| AC | Then |
| --- | --- |
| 1 | Chảo, nồi, vỉ nướng, thớt, đĩa, thùng gỗ (2 loại), rương đột phá là model; vị trí và kích thước va chạm không đổi |
| 2 | Giả lập PlayMode cũ vẫn xanh (tâm ngắm vẫn trúng đồ vật) |
| 3 | Quá trình nấu của sprint 2 (dầu, miếng cắt, đổi màu) chạy đúng trong lòng model mới |

## 3. Việc mỗi ngày

| Ngày | Bạn (Blender, 2–3 giờ) | Mình |
| --- | --- | --- |
| 1 | Cà chua nguyên + thái múi | F-070: thư mục, quy ước tên, tự nạp, công cụ kiểm |
| 2 | Hành tây, trứng | Hướng dẫn có ảnh cho từng loại; nối 2 model đầu, chụp ảnh |
| 3 | Rau muống, đậu phụ | Sửa điểm mặt, đế bậc; kiểm viền |
| 4 | Thịt heo; chảo | Nối chảo; chỉnh quá trình nấu cho lòng chảo mới |
| 5 | Nồi, vỉ nướng | Nối; giả lập PlayMode |
| 6 | Thớt, đĩa, thùng gỗ | Nối; ảnh trước / sau |
| 7 | Sửa theo góp ý | Build, nghiệm thu |

Mỗi model mới: mình trả lại trong ngày một ảnh chụp trong game + danh sách lỗi quy cách (nếu có).

## 4. Kiểm

Công cụ kiểm quy cách (F-070 AC 3) chạy trong EditMode cho mọi file trong thư mục model. PlayMode: bộ giả lập hiện có + ảnh chụp. Kích thước bản build không tăng quá 30 MB.

## 5. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Một model tốn hơn 1 buổi | Dừng ở 6 nguyên liệu + chảo + nồi; phần còn lại sang sprint 9 hoặc 11 |
| Model đẹp nhưng lệch phong cách (không tròn, nhiều chi tiết nhỏ dưới 10 cm) | Kiểm theo Art Bible §5 và ngưỡng chi tiết §8 trước khi nối |
| Bạn chưa kịp học | Giữ hình ghép khối của sprint 2; sprint này mình làm đường ống và hướng dẫn, model làm dần |

## 6. Câu hỏi

- Bạn muốn tự dựng hết hay dùng thêm model có giấy phép tự do (CC0) rồi sửa cho hợp phong cách?
