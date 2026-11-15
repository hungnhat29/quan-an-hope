# Tổng kết Sprint 2 (đóng ngày 16/11/2026)

Người dùng đóng sprint ngày 16/11: "đóng sprint 2 và push lên github". Chưa có phản hồi accept/reject riêng cho từng tính năng; các mục "CẦN NGƯỜI KIỂM" chuyển sang sprint 3.

## Đã giao

| Hạng mục | Spec | Kết quả cuối |
|---|---|---|
| Bộ số constitution 1.4 vào game (8 vé ngày 1, thùng 3+2, sơ chế ×3, nấu ×1,5, nợ 750G…) | F-010 | xong |
| Ngày theo giờ 9:00–21:00, khách không giới hạn, biển tự đóng 21:00 | F-011 | xong |
| Hình 22 nguyên liệu (nguyên + đã sơ chế) | F-012 | xong, **chờ duyệt ảnh** |
| Quá trình nấu: lớp dầu, miếng cắt, lửa, đảo, màu, món chín, đĩa, cháy xém | F-013 | xong, **chờ duyệt ảnh** |
| Ghi thời gian thật mỗi món (`playtest-log.csv`) + script phân tích | F-014 | AC1 xong; **AC2 chờ buổi chơi thật** |

Bằng chứng cuối: EditMode 142/142, PlayMode 5/5, bot có giờ 99,85% qua nợ 1 (mô phỏng 96%), mô phỏng `balance/` khớp `V=next` (63% / 46% / 89%), build Windows chạy không lỗi.

## Lỗi của sprint này

| # | Lỗi | Tốn | Nguyên nhân gốc | Quy tắc |
|---|---|---|---|---|
| 1 | Test PlayMode F-013 xanh nhưng ảnh cho thấy đồ dồn giữa chảo, lửa khuất dưới đáy | 1 vòng sửa | `PanSpec` từ sprint 1 tự ước lượng bán kính 0,17 m (chảo thật 0,29 m), nồi đặt đồ ở 1,83 m trên vành 1,51 m; test chỉ kiểm "nằm trong bán kính" | L-016 mới; L-005 count 2 |
| 2 | Giả lập PlayMode fail 3 lần sau khi đổi số (đĩa bị che, kho chỉ một món, kiểm 21:00 phụ thuộc nhịp khung hình) | 3 lần chạy (~10 phút mỗi lần) | Kịch bản giả lập gắn với trạng thái của bộ số cũ | L-017 mới |
| 3 | Heredoc Python trong Bash lỗi cú pháp | nhỏ | như L-002 | L-002 count 2 |
| 4 | Test F-011 viết sai kịch bản (một lần Tick lớn vẫn cho khách vào) | nhỏ | Viết test trước khi nghĩ hết luật "khách vào trước 21:00" | đã sửa test, không cần quy tắc mới |
| 5 | Kịch bản 10 giây không được duyệt riêng trước khi code (L-014) | nợ quy trình | Coi "triển khai impl plan" là duyệt | Sprint 3: gửi kịch bản cửa hàng relic để duyệt trước |

## Chuyển sang sprint 3

- Duyệt ảnh `docs/sprint-2/ui/nguyen_lieu/`, `nau_*.png`; chưa có ảnh cận chuỗi trứng chiên.
- Chơi thử bản build → `balance/playtest_overhead.py` → nếu lệch quá 5 giây thì sửa OVERHEAD và chạy lại mô phỏng (Q-24).
- Câu hỏi: Q-21, Q-22 (đã làm theo mặc định), Q-25, Q-26, Q-27 và các câu cũ.
- Bot C# chưa có relic nên cao hơn mô phỏng 3,85 điểm; thêm relic vào bot ở sprint 3.
