# Impl plan — Sprint 7: Cân bằng và cảm giác → Beta nội dung

Thời gian: 20/12 → 26/12/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` v1.4 Điều 1 (ba mốc độ khó), Điều 2, Điều 3, §9 (mô phỏng và các giả định cần đo), §6.6 (giờ chờ "cần kiểm lại khi playtest"). Hình và âm: Art Bible §6 (7 biểu cảm), §11 (hiệu ứng), §12 (animation trong Unity).
Điều kiện vào sprint: có bot C# (sprint 5) và bản lưu theo ngày hoặc lệnh nhảy vòng (sprint 6).

## 1. Mục tiêu

Cuối ngày 26/12:

1. **Ba mốc Điều 1 đạt trên chính game**, không chỉ trên mô phỏng Python: người không đọc chỉ số đi ≥ 60% chặng đường; người gom ≥ 3 món một bộ thắng ≥ 70%; người không gom bộ thấp hơn rõ.
2. Mọi giả định của mô phỏng được thay bằng số đo (thời gian mỗi món, mức giảm kiên nhẫn).
3. Game có tiếng: bước chân, thái, xèo, chuông, tiền, thùng rơi, nhạc nền.
4. Đủ các hiệu ứng trong bảng Art Bible §11 và 7 biểu cảm khách §6.
5. Bạn đã cài Blender và làm xong bài tập đầu (chưa đưa gì vào game).

### Ngoài phạm vi

Model Blender (sprint 8–9). Nội dung mới.

## 2. Tính năng và tiêu chí chấp nhận

### F-060 Đo và chỉnh cân bằng — Điều 1, §9

| AC | Then |
| --- | --- |
| 1 | Từ `playtest-log.csv` của ít nhất 3 buổi bạn chơi (sprint 2 trở đi): thời gian đi lại và thao tác mỗi món, theo vòng. Số này thay hằng số `OVERHEAD` trong `sim_balance.py` và tham số thời gian của bot |
| 2 | Bot 3 kiểu × 4.000 ván trên luật thật; bảng kết quả ghi vào constitution §1 thành cột "đo trên game" |
| 3 | Nếu trượt mốc nào: chỉnh trong dữ liệu (`balance.json`), theo thứ tự ưu tiên nợ → giá gốc → vé mỗi ngày → độ dài ngày; chạy lại tới khi đạt cả ba. Mỗi lần chỉnh ghi vào §9 |
| 4 | Không chỉnh bằng cách sửa code (Điều 2); test thống kê vẫn xanh (Điều 3) |
| 5 | Kiểm riêng từng bộ relic: không bộ nào thắng dưới 70% hoặc trên 97% khi đủ 3 món |
| 6 | Giờ chờ §6.6: đo số khách bị giảm dễ tính mỗi ngày ở vòng 2–4 với bot; mục tiêu ghi trong spec sau khi bạn chọn (đề xuất: người chơi tốt bị 1–2 khách mỗi ngày) |

### F-061 Âm thanh — Art Bible §11; constitution Điều 6 (`.wav` qua Git LFS)

| AC | Then |
| --- | --- |
| 1 | Mỗi thao tác chính có tiếng: nhét vé, thùng rơi, cầm, đặt, thái (lặp khi giữ), cho vào bếp, xèo / sôi theo dụng cụ, chín, múc, chuông, tiền, khách vui / giận, mua relic, thu nợ |
| 2 | Bậc 4–5 lộ ra có tiếng riêng (Art Bible §11: "bậc cao … có âm thanh riêng") |
| 3 | Nhạc nền phòng; nhạc khác cho ngày mất điện và ngày 21 |
| 4 | Âm lượng theo Cài đặt; tiếng trong phòng là âm 3D (gần to, xa nhỏ) |
| 5 | File âm thanh có giấy phép dùng được; ghi nguồn vào `docs/CREDITS.md` |

### F-062 Hiệu ứng và biểu cảm còn thiếu — Art Bible §6, §11, §12

| AC | Then |
| --- | --- |
| 1 | Bảng §11 đủ 6 dòng: thùng rơi qua cổng dịch chuyển (xoắn ốc), lộ bậc (bậc 4–5 dừng 1,5 giây), món ra lò, khách vui / tip (tim + đồng xu bay về ô Ví), cháy / Lạc vị, nước sốt (vòng xoáy vàng / khói xám) |
| 2 | Khách có đủ 6 biểu cảm (Vui, Chờ, Giận, Ngạc nhiên, Buồn, Đang ăn) và chủ nợ lạnh lùng, dùng đúng lúc theo bảng §6 |
| 3 | Chớp mắt 0,12 giây mỗi 3–5 giây cho khách và nguyên liệu (§12) |
| 4 | Màu phủ theo giờ trong ngày và mưa ngày 2 (Art Bible §4) chạy theo đồng hồ ngày |

### F-063 Học Blender (việc của bạn, 30–45 phút mỗi ngày)

| Ngày | Bài |
| --- | --- |
| 1 | Cài Blender bản LTS; di chuyển, xoay, phóng trong khung nhìn |
| 2 | Dựng khối: cầu, trụ, hộp; kéo, bo góc |
| 3 | Tô màu phẳng theo bảng màu Art Bible §4 |
| 4 | Dựng một quả cà chua theo ảnh 08; shade smooth |
| 5 | Xuất `.fbx`, kéo vào Unity, gắn shader `ToonFlat` + `Outline` |
| 6–7 | Làm lại cà chua trong 20 phút; thử hành tây |

Mình chuẩn bị: file hướng dẫn từng bước có ảnh (`docs/blender/01-ca-chua.md`), thông số xuất `.fbx` (Art Bible §14), cảnh Unity thử model.

## 3. Lịch 7 ngày

| Ngày | Việc |
| --- | --- |
| 1 | F-060: gom số đo, chạy bot, bảng lệch; đề xuất chỉnh cho bạn duyệt |
| 2 | F-060: chỉnh dữ liệu, chạy lại tới khi đạt; cập nhật constitution |
| 3 | F-061: khung âm thanh + 15 tiếng thao tác |
| 4 | F-061: nhạc, âm 3D; F-062 hiệu ứng §11 |
| 5 | F-062 biểu cảm, chớp mắt, màu theo giờ |
| 6 | Chơi thử trọn ván (có lưu): bạn và bot; sửa lỗi |
| 7 | Build Beta nội dung, nghiệm thu |

## 4. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Đạt mốc trên mô phỏng nhưng trượt trên game | Game thắng: constitution ghi số đo trên game làm chuẩn, mô phỏng sửa theo |
| Không tìm được âm thanh có giấy phép phù hợp | Tự tạo tiếng đơn giản bằng công cụ sinh âm; nhạc nền để sau |
| Chỉnh nợ nhiều lần làm bạn mất dấu | Mỗi lần chỉnh một dòng trong §9, kèm bảng trước / sau |

## 5. Câu hỏi

- Mức "khách bị chờ quá giờ" bạn thấy vừa là bao nhiêu mỗi ngày?
- Một ván 22 ngày nên dài bao lâu (liên quan Q-22)?
