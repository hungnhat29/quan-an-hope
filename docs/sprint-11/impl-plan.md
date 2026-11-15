# Impl plan — Sprint 11: Đệm và phát hành thử

Thời gian: 17/1 → 23/1/2027 (7 ngày) · tuần cuối của khung 10 tuần
Nguồn: phản hồi chơi thử của sprint 10 (`docs/sprint-10/playtest-summary.md`), `docs/KNOWN-ISSUES.md`, việc trễ của các sprint trước ghi trong `.dev-memory/STATE.md`, constitution v1.4 Điều 1.

Sprint này **không có tính năng định trước**. Nội dung được chốt vào ngày 1 từ ba nguồn trên; file này quy định cách chọn và điều kiện phát hành.

## 1. Mục tiêu

Cuối ngày 23/1: bản **release candidate** — bản mà nếu không ai tìm ra lỗi chặn trong 3 ngày thì thành bản phát hành thử.

## 2. Cách chọn việc (ngày 1)

Xếp mọi việc vào 4 nhóm, làm theo thứ tự, hết giờ thì dừng:

| Nhóm | Là gì | Ví dụ |
| --- | --- | --- |
| A — Chặn | Người chơi không đi tiếp được, mất bản lưu, game thoát | Kẹt trong đồ vật, không bấm được chuông |
| B — Hiểu sai | ≥ 2 người chơi thử không hiểu một luật cốt lõi | Không biết khách gọi món, không biết màu là bậc |
| C — Cân bằng | Số đo người thật trượt mốc Điều 1 | Người mới không qua nổi lần trả nợ 1 |
| D — Việc trễ | Việc đã hứa ở sprint trước chưa xong | Model nguyên liệu vòng 2–4, tranh cốt truyện |

Ý tưởng mới không vào sprint này; ghi vào `INBOX.md`.

## 3. Tiêu chí phát hành (tất cả phải đạt)

| # | Tiêu chí | Kiểm bằng |
| --- | --- | --- |
| 1 | Không còn lỗi nhóm A | `KNOWN-ISSUES.md` |
| 2 | EditMode và PlayMode xanh toàn bộ | Lệnh test trong `STATE.md` |
| 3 | Bot 3 kiểu đạt ba mốc Điều 1 trên bản build cuối | Bảng bot |
| 4 | Bảng số constitution khớp dữ liệu | Script so của sprint 10 |
| 5 | Một người chưa từng chơi đi được hết ngày 1 mà không hỏi | Quan sát hoặc log |
| 6 | Bản build mở toàn màn hình trên 2 máy khác nhau | Thử tay |
| 7 | `CREDITS`, `HUONG-DAN`, số phiên bản đúng | Đọc lại |
| 8 | Mã nguồn đã đẩy lên GitHub, gắn thẻ `rc-1` | `git tag` |

## 4. Lịch 7 ngày

| Ngày | Việc |
| --- | --- |
| 1 | Gom phản hồi, xếp nhóm A–D, bạn duyệt danh sách |
| 2–3 | Nhóm A, rồi B |
| 4 | Nhóm C: chỉnh dữ liệu theo quy trình sprint 7 (đo → mô phỏng → bot → ghi constitution) |
| 5 | Nhóm D trong thời gian còn lại |
| 6 | Đóng băng: chỉ sửa lỗi; chạy đủ 8 tiêu chí |
| 7 | Build `rc-1`, gắn thẻ, báo cáo cuối: đã làm gì trong 10 tuần, còn nợ gì, đề xuất bước tiếp |

## 5. Rủi ro

| Rủi ro | Phương án |
| --- | --- |
| Nhóm A + B đã hết tuần | Phát hành thử với nhóm C, D ghi rõ trong `KNOWN-ISSUES.md` |
| Chỉnh cân bằng phút cuối gây lỗi mới | Chỉ chỉnh dữ liệu, không sửa code; chạy lại toàn bộ test thống kê |

## 6. Sau sprint 11 (không thuộc kế hoạch này)

Các việc đã biết là sẽ còn: đổi phím và tay cầm, thêm thân khách, tranh cốt truyện vẽ tay, dịch tiếng Anh, đưa lên cửa hàng.
