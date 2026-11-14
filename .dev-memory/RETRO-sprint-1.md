# Tổng kết Sprint 1 (đóng ngày 14/11/2026)

Người dùng đóng sprint ngày 14/11: "đóng sprint 1 lưu vào những lỗi cần sửa vào .dev-memory để rút kinh nghiệm cho các sprint sau và đẩy lên github sprint 1".

## Đã giao

| Hạng mục | Spec | Kết quả cuối |
|---|---|---|
| Nền dự án, dữ liệu cân bằng | F-001 | xong |
| Gacha nguyên liệu, nấu ăn, khách và trả tiền, vòng ngày, trả nợ | F-002 … F-005 | xong, 128 test EditMode |
| Giao diện 2D 4 góc | F-006 | **bị thay thế** sau nghiệm thu |
| Chơi 3D góc nhìn thứ nhất, thông tin trên đồ vật | F-007 | xong, làm lại 1 lần |
| Mặt nguyên liệu / món, đồ bay vào bếp, dải tỉ lệ 6 bậc | F-008 | xong, phát sinh sau chơi thử |
| Khách gọi món, giá trên sách, toàn màn hình | F-009 | xong, phát sinh sau chơi thử |

Bằng chứng cuối: EditMode 128/128, PlayMode 3/3, build Windows 119 MB, mô phỏng đạt Điều 1 (67% / 82% / 56%).

## Lỗi của sprint này (xếp theo mức tốn công)

| # | Lỗi | Tốn | Nguyên nhân gốc | Quy tắc cho sprint sau |
|---|---|---|---|---|
| 1 | Dựng UI 2D (nút chuyển góc, bảng phẳng) cho game 3D; người dùng từ chối, làm lại toàn bộ phần hiển thị | ~1 ngày công | Spec F-006 mô tả luật, không mô tả **cách người chơi nhìn và thao tác**; không cho người dùng xem bản phác trước khi code | L-014: tính năng có hiển thị phải có "kịch bản 10 giây" (đứng ở đâu, nhìn gì, bấm gì, thấy gì) để người dùng duyệt trước khi code |
| 2 | Nguyên liệu và món không có mặt, không có hơi theo bậc dù Art Bible §6, §7 đã quy định | 1 vòng làm lại | Chỉ đối chiếu AC với constitution, không đối chiếu với Art Bible | L-013: mỗi tính năng có hình phải có bảng "mục Art Bible → đã làm ở đâu" trong spec |
| 3 | Cho đồ vào bếp không có hình, không có chuyển động | 1 vòng làm lại | AC chỉ kiểm trạng thái luật, không kiểm phản hồi nhìn thấy | L-013 (như trên): mỗi thao tác phải có AC "thấy gì" |
| 4 | Khách không gọi món cụ thể; phải thêm luật §6.7 và mô phỏng lại | 1 vòng làm lại + rủi ro cân bằng | Constitution 1.0 không nói; mình mặc định "món nào cũng được" mà không hỏi | Luật 1 của dev-pilot: chỗ constitution im lặng về **vòng chơi cốt lõi** thì hỏi, không tự mặc định |
| 5 | Game mở dạng cửa sổ | người dùng phải báo | Lệnh chạy thử build có `-screen-fullscreen 0`, Unity lưu vào registry | L-011 |
| 6 | Luật gọi món làm người chơi giỏi tụt 83% → 59% ở lần mô phỏng đầu | phát hiện kịp | Mô phỏng thiếu 3 khách cùng ngồi và ngoại lệ chủ nợ | L-012: đổi luật ghép món / khách thì mô phỏng trước, code sau |
| 7 | Mặt đậu phụ lọt vào trong khối; đồ trong nồi bị mép nồi che | bắt được nhờ xem ảnh | Không nghĩ tới hình khối vuông nhìn xéo, miệng nồi ngang tầm mắt | L-005, L-009: luôn chụp ảnh từ giả lập và tự xem |
| 8 | Người chơi rơi xuyên sàn, kẹt khi tua nhanh, quầy chờ dựng lại liên tục | bắt được nhờ giả lập | Cảnh sinh từ script không có collider; dựng lại UI mỗi lần có thông báo | L-008 |
| 9 | 8 test gọi món không được thấy đỏ trước khi code | nợ quy trình | Viết test và code liền nhau, bỏ bước chạy đỏ | Luật 3: chạy test mới một lần trước khi viết code, kể cả khi chỉ đỏ vì chưa biên dịch |
| 10 | Đọc sót kết quả test thất bại (`Failed(Child)`) | 1 lần | grep chỉ khớp chữ cái | L-010 |

## Điều người dùng chưa hài lòng khi đóng sprint (mang sang các sprint sau)

Nguyên văn ngày 14/11, đã ghi vào `INBOX.md`:

1. Hình nguyên liệu xấu, chưa chân thực.
2. Hình sau khi cho nguyên liệu vào chỗ nấu: xấu, lộn xộn, không giống quá trình nấu ăn.
3. Khách không giới hạn: vào lần lượt tới hết 21h; vòng sau có nhiều khách cùng lúc cũng tới hết 21h.
4. Tăng thời gian chuẩn bị (sơ chế) và thời gian nấu.
5. Tăng số tiền nợ.
6. Giảm cỡ thùng: đồ phụ 3, nguyên liệu chính 2, đột phá 1; chỉnh theo tỉ lệ ngày 3, các vòng sau và các relic liên quan.

Xử lý: constitution 1.4 + mô phỏng lại (xem `STATE.md`), đưa vào ROADMAP và impl plan sprint 2 trở đi.

## Câu hỏi còn mở khi đóng sprint

Q-9, Q-13, Q-17 (tốc độ đi, độ nhạy chuột, tầm với), Q-18 (thùng gộp theo loại), Q-19 (cỡ dải tỉ lệ), Q-20 (chủ nợ gọi món). Xem `QUESTIONS.md`.

## Điều làm tốt, giữ lại

- Luật chơi là C# thuần, có test: ba lần làm lại phần hiển thị không đụng tới luật, 120 test cũ vẫn xanh.
- Giả lập PlayMode đi lại và bấm như người thật, chụp ảnh: bắt được 7 lỗi trước khi người dùng thấy.
- Mọi con số ở `balance/*.json`, mô phỏng 4.000 ván mỗi kiểu chạy trong dưới một phút.
