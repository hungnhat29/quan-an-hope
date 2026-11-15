# Tổng kết Sprint 3 (đóng ngày 16/11/2026)

Người dùng đóng sprint ngày 16/11: "đóng sprint 3 và push lên github". Không có phản hồi accept/reject riêng cho từng tính năng; câu hỏi Q-28..Q-31 và mục "CẦN NGƯỜI KIỂM" chuyển sang sprint 4.

## Đã giao

| Hạng mục | Spec | Kết quả cuối |
|---|---|---|
| Cửa hàng relic Góc 3: 3 bệ, thẻ dựng, mua bằng vé, giá 8 ô, bỏ relic khi đầy, relic bay lên giá | F-020 | xong; lệnh gỡ lỗi "cho relic" **cần người kiểm** |
| Hiệu ứng 22 relic + thưởng bộ 2/3/4; tiến độ bộ trên giá; bảng tỉ lệ Góc 2 tính relic | F-021 | xong; 4 relic hệ thống sprint 4 ghi "có tác dụng từ ngày 4" |
| Thùng đột phá: rương tím nắp vòm, 70/30, món đột phá nấu được và khách gọi được | F-022 | xong |

Bằng chứng cuối: EditMode 179/179, PlayMode 7/7, bot C# có mua relic 96,75% qua nợ 1 (mô phỏng 96%; trước sprint 3 là 99,85%), mô phỏng 63% / 90% / 46% không đổi, build Windows chạy không lỗi. Ảnh: `docs/sprint-3/ui/`.

## Lỗi của sprint này

| # | Lỗi | Tốn | Nguyên nhân gốc | Quy tắc |
|---|---|---|---|---|
| 1 | Viết Core relic trước test; test chỉ đỏ vì chưa nhập dữ liệu | nợ quy trình | Nóng vội khi nhiều hiệu ứng | L-015 count 2 |
| 2 | Lệnh `cat >` thừa treo 120 giây, tạo file rỗng ngoài repo; heredoc lồng `'''` lỗi | ~3 phút | như L-002 | L-002 count 3, lên G-002 |
| 3 | Chạy PlayMode ghi đè 57 ảnh sprint 2 đang chờ duyệt | nhỏ (đã khôi phục) | test cũ chụp vào thư mục sprint cũ | L-018 mới |
| 4 | Test so double 1e-9 với số float từ JSON | 1 lần chạy | 0,15f ≠ 0,15 | L-019 mới |
| 5 | Ảnh bắt 3 lỗi hiển thị khi test xanh (tiêu đề bảng co chữ, viền vàng che nhãn, chữ thẻ nhỏ) | 1 lần chạy | test kiểm chữ, không kiểm cỡ/che | L-005 đúng việc |
| 6 | Thiếu đường bay relic trong kịch bản 10 giây, chỉ thấy khi double-check | nhỏ | đọc kịch bản lúc cuối thay vì lúc plan | đối chiếu kịch bản ở P3 |
| 7 | Kịch bản 10 giây không được duyệt riêng trước khi code (L-014) | nợ quy trình | coi "triển khai impl plan" là duyệt (lần 2) | sprint 4: hỏi duyệt kịch bản trước |

## Chuyển sang sprint 4

- Trả lời Q-28 (thưởng bộ cộng dồn), Q-29 (Chảo gang với món sai công thức), Q-30 (màu độ hiếm), Q-31 (`relic_shop_offers`).
- Tác dụng Tủ lạnh 2 cửa, Đồng hồ cát, Máy pha cà phê, Hũ sốt cũ khi có tủ lạnh / giờ chờ / nước sốt.
- Còn từ sprint 2: duyệt ảnh `docs/sprint-2/ui/`, chơi thử → `balance/playtest_overhead.py` (Q-24).
