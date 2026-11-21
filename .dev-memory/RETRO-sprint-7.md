# Tổng kết Sprint 7 (đóng ngày 21/11/2026)

Bạn đóng sprint ngày 21/11: "end sprint 7, commit và push lên github". Không có accept / reject riêng từng task; câu hỏi Q-53..Q-57 và các mục "CẦN NGƯỜI KIỂM" chuyển sang sprint 8.

## Đã giao

| Hạng mục | Nguồn | Kết quả cuối |
|---|---|---|
| Tay khách cầm đũa khi ăn (ống cao su cong) | tin nhắn 21/11 | xong |
| Ẩn relic bộ / lẻ trên màn hình | tin nhắn 21/11 | xong (độ hiếm vẫn hiện) |
| Ngày 1 có 5 vé; đồng hồ chạy từ lúc mở quán | tin nhắn 21/11, constitution 1.7 | xong, đóng Q-21 |
| F-060 Cân bằng đo trên game | impl plan | 7 vé / ngày, nợ 22 36.000G: 75% / 86% / 37%; AC-1 **cần buổi chơi thật** |
| F-061 Âm thanh | impl plan | 24 tiếng + 3 nhạc tự tạo, âm 3D, theo Cài đặt; **cần bạn nghe thử** |
| F-062 Hiệu ứng, biểu cảm, chớp mắt, màu theo giờ, mưa | impl plan | xong |
| F-063 Blender | impl plan | tài liệu + công cụ Unity; bài tập là của bạn |

Bằng chứng cuối: EditMode 296/296 (+2 Explicit), PlayMode 37/37, bot C# 4.000 ván (`balance/bot_report.md`), quét phương án (`balance/bot_sweep.md`), build Windows chạy 15 giây không lỗi. Ảnh: `docs/sprint-7/ui/`.

## Lỗi của sprint này

| # | Lỗi | Tốn | Nguyên nhân gốc | Quy tắc |
|---|---|---|---|---|
| 1 | Tay 2 khớp trái Art Bible §6/§12 (ống cao su, không khuỷu) | làm lại 1 lần | không đọc Art Bible trước khi dựng hoạt ảnh | L-033 |
| 2 | Ống tay màu đen dù test vị trí xanh; đoán sai nguyên nhân (khung lật gương) | 2 lượt PlayMode | sửa theo giả thuyết chưa kiểm | L-005 count 8, L-037 |
| 3 | Bot nấu sẵn đĩa trước khi mở quán: gom bộ 52% → 27% | 1 lượt bot | chiến thuật bot không hợp luật mới | L-020 count 3 |
| 4 | 3 lỗi biên dịch trùng tên (`bt`, `mood`, `Drop`) | 3 lượt Unity | không grep tên trước khi đặt | L-036 |
| 5 | Heredoc lỗi lần 7 | nhỏ | như L-002 | dùng `sub.py` (spec @@@) |
| 6 | Log playtest lẫn dòng của test PlayMode | thiếu số đo thật | công cụ đo dùng chung thư mục với test | L-034 |
| 7 | Test khách ăn chạy riêng thì hỏng (khung giật × tua 25) | 1 lượt | L-022 | maximumDeltaTime trong test |
| 8 | Kịch bản 10 giây không được duyệt riêng (L-014) | nợ quy trình | coi "triển khai impl plan" là duyệt (lần 5) | sprint 8: hỏi duyệt trước |

## Chuyển sang sprint 8

- Trả lời Q-53 (mục tiêu giờ chờ), Q-54 (7 vé + nợ 36.000G), Q-55 (mặt khách vẽ code hay PNG), Q-56 (3 buổi chơi thật cho F-060 AC-1), Q-57 (âm thanh CC0).
- Bạn: chơi thử trọn ván bản build mới (impl plan sprint 7 ngày 6), nghe âm thanh, làm bài Blender `docs/blender/01-ca-chua.md`.
- Mô phỏng Python vẫn lạc quan hơn game (78 / 98 / 83%): game là chuẩn từ 1.7.
