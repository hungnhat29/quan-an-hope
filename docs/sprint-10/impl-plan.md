# Impl plan — Sprint 10: Hoàn thiện

Thời gian: 10/1 → 16/1/2027 (7 ngày)
Nguồn: `specs/constitution.md` v1.4 toàn bộ (rà lại từng điều), Điều 1 (độ khó với người thật), Art Bible §15 (lỗi thường gặp), ROADMAP ("bản build Windows cho người ngoài chơi").
Điều kiện vào sprint: Beta nội dung (sprint 7) + model đã có tới đâu dùng tới đó.

## 1. Mục tiêu

Cuối ngày 16/1: một file nén gửi được cho người ngoài; họ giải nén, mở, chơi trọn một ván mà không cần bạn ngồi cạnh. Có 3–5 người đã chơi và gửi phản hồi.

### Ngoài phạm vi

Tính năng mới. Mọi ý mới ghi vào `.dev-memory/INBOX.md` cho sau phát hành thử.

## 2. Việc và tiêu chí chấp nhận

### F-090 Rà constitution — Điều 8

| AC | Then |
| --- | --- |
| 1 | Bảng "điều / mục constitution → có trong game chưa → test nào" cho toàn bộ 9 điều và mục 2–8; mọi dòng "chưa" đều có quyết định: làm, hoặc sửa constitution |
| 2 | Mọi câu hỏi OPEN trong `QUESTIONS.md` được đóng hoặc bạn ghi "để sau phát hành" |
| 3 | Bảng số trong constitution khớp `balance/*.json` (một script so tự động) |

### F-091 Sửa lỗi và độ ổn định

| AC | Then |
| --- | --- |
| 1 | Bot chơi 200 ván đủ 22 ngày trong bản build không có ngoại lệ nào trong log |
| 2 | Giả lập PlayMode + EditMode xanh; không test nào bị bỏ qua |
| 3 | Lưu / tải 50 lần ngẫu nhiên giữa ván không hỏng bản lưu |
| 4 | Danh sách lỗi đã biết còn lại ghi trong `docs/KNOWN-ISSUES.md` |

### F-092 Tối ưu

| AC | Then |
| --- | --- |
| 1 | ≥ 60 khung hình / giây ở 1920×1080 trên máy bạn; đo cả ngày đông nhất (vòng 4, 3 khách, nhiều thùng) |
| 2 | Không có khựng quá 50 ms khi nhét vé, khi khách vào, khi lật sách (hình và mặt tạo sẵn, không tạo lúc đang chơi) |
| 3 | Thời gian từ mở game tới menu dưới 10 giây |

### F-093 Bản build cho người ngoài

| AC | Then |
| --- | --- |
| 1 | Tên sản phẩm và tên công ty thật trong cài đặt dự án (hiện là "My project" / "DefaultCompany"); biểu tượng game |
| 2 | Mở lần đầu: toàn màn hình, đúng độ phân giải màn; chạy thử trên một máy khác máy bạn |
| 3 | File nén kèm `HUONG-DAN.txt`: cách mở, phím, cách gửi phản hồi; `CREDITS` có font, âm thanh, công cụ |
| 4 | Số phiên bản hiện ở menu |

### F-094 Chơi thử với người ngoài — Điều 1

| AC | Then |
| --- | --- |
| 1 | 3–5 người chơi; mỗi người một file log (`playtest-log.csv`) + 5 câu hỏi ngắn (kẹt ở đâu, ngày nào chán, có hiểu bậc và tỉ lệ không, thua ở đâu, có muốn chơi lại không) |
| 2 | Bảng tổng hợp: mỗi người đi tới ngày nào, thời gian mỗi món, số khách mỗi ngày; so với ba kiểu người chơi của Điều 1 |
| 3 | Danh sách việc cho sprint 11, xếp theo số người gặp |

## 3. Lịch 7 ngày

| Ngày | Việc |
| --- | --- |
| 1 | F-090 bảng rà constitution; chốt câu hỏi mở với bạn |
| 2 | F-091 chạy bot 200 ván trong build, sửa lỗi |
| 3 | F-092 đo và tối ưu |
| 4 | F-093 tên, biểu tượng, hướng dẫn, đóng gói; thử trên máy khác |
| 5 | Gửi bản build cho người chơi thử; trong lúc chờ: sửa lỗi nhỏ, `KNOWN-ISSUES` |
| 6 | Thu log và câu trả lời; tổng hợp |
| 7 | Sửa các lỗi chặn người chơi; nghiệm thu; lập danh sách sprint 11 |

## 4. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Không đủ người chơi thử | Tối thiểu 2 người + bạn; kéo dài thu phản hồi sang sprint 11 |
| Người ngoài chơi chậm hơn nhiều so với giả định cân bằng | Không chỉnh vội: gom số, chỉnh một lần ở sprint 11 theo quy trình sprint 7 |

## 5. Câu hỏi

- Tên game chính thức, tên hiển thị của tác giả.
- Gửi cho ai chơi thử, bằng cách nào (file nén qua mạng, itch.io riêng tư…).
