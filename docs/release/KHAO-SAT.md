# Khảo sát người chơi thử — Quán ăn của Hope 0.11.0

Mục đích: đo **độ hay** của game và tìm chỗ người chơi **hiểu sai** / **bị kẹt**, để làm nhóm B, C của quy trình sprint 11 (impl plan §2) và quyết định Steam (`STEAM-DEPLOY-PLAN.md` mục 5). Thời gian điền: khoảng 5 phút. Điền **ngay sau khi chơi**.

Trang khảo sát dùng được ngay: xem link Claude gửi (trang song ngữ Việt / Anh). Bộ câu hỏi dưới đây giống hệt trang đó, để bạn dựng lại trên Google Forms nếu muốn.

## Cách đi khảo sát

1. Gửi cho người chơi: `QuanAnHope-0.11.0.zip` + link trang khảo sát. Không giải thích luật trước (để đo được hướng dẫn trong game).
2. Nếu ngồi cạnh: chỉ quan sát, ghi lại lúc họ **hỏi** hoặc **đứng im > 20 giây** (tiêu chí 5 của impl plan: "đi được hết ngày 1 mà không hỏi").
3. Sau khi chơi (thắng, thua, hoặc chán thì dừng): mời điền khảo sát, rồi gửi `playtest-log.csv` (đường dẫn trong `HUONG-DAN.txt`).
4. Người chơi bấm **Sao chép câu trả lời** và gửi cho bạn; bạn mở trang, bấm **Kết quả** → dán vào ô nhập. Mục tiêu tối thiểu: **5–10 người**, ít nhất 3 người chưa từng chơi game nấu ăn.

## Câu hỏi

### A. Bạn là ai
| # | Câu hỏi | Kiểu |
| --- | --- | --- |
| Q1 | Bạn chơi game bao nhiêu giờ mỗi tuần? | < 1 · 1–5 · 5–15 · > 15 |
| Q2 | Thể loại bạn hay chơi | Chọn nhiều: nấu ăn / quản lý quán · roguelike / thẻ bài · nhẹ nhàng (cozy) · hành động / bắn súng · chiến thuật · ít chơi game |
| Q3 | Bạn chơi bằng ngôn ngữ nào? | Tiếng Việt · English |

### B. Bạn đã chơi tới đâu
| # | Câu hỏi | Kiểu |
| --- | --- | --- |
| Q4 | Bạn chơi bao lâu? | < 15 phút · 15–30 · 30–60 · 1–2 giờ · > 2 giờ |
| Q5 | Kết quả | Thắng · Thua ở lần trả nợ ngày 4 · ngày 9 · ngày 15 · ngày 22 · Dừng giữa chừng |
| Q6 | Ngày xa nhất bạn tới | Số 1–22 |

### C. Bạn có hiểu luật không (Có · Hơi hơi · Không)
| # | Luật |
| --- | --- |
| Q7a | Khách gọi một món cụ thể; phải đặt đúng món trước mặt khách |
| Q7b | Màu đế nguyên liệu là bậc; đồ tốt hơn → món ngon hơn → bán đắt hơn |
| Q7c | Bảng tỉ lệ trên máy vé và trên bếp |
| Q7d | Relic và việc gom đủ một bộ relic |
| Q7e | Chủ nợ thu tiền đầu ngày 4, 9, 15, 22; thiếu là thua |
| Q8 | Lần đầu bạn bị kẹt (không biết làm gì tiếp) là ở đâu? *(chữ)* |

### D. Cảm nhận (1 = rất không đồng ý … 5 = rất đồng ý)
| # | Câu | Ghi chú |
| --- | --- | --- |
| Q9 | Phần nấu (sơ chế, cho đúng thứ tự, vặn núm) vui | Vòng lặp chính |
| Q10 | Nhét vé quay nguyên liệu thấy hồi hộp | Gacha |
| Q11 | Độ khó (1 quá dễ · 3 vừa · 5 quá khó) | Thang riêng, mục tiêu 3 (Điều 1: trung bình-dễ tới trung bình-khó) |
| Q12 | Tôi muốn chơi lại / chơi tiếp | Ý định |
| Q13 | Tôi sẽ giới thiệu game này cho bạn bè | Ý định |
| Q14 | Nếu game có trên Steam, tôi sẽ thêm vào wishlist hoặc mua | Ý định |
| Q15 | Hình ảnh và không khí quán dễ chịu | |
| Q16 | Âm thanh và nhạc hợp | |
| Q17 | Truyện tranh / cốt truyện thú vị | |
| Q18 | Tranh hướng dẫn dễ hiểu | |

### E. Giá và góp ý
| # | Câu hỏi | Kiểu |
| --- | --- | --- |
| Q19 | Bạn sẵn lòng trả bao nhiêu cho bản đầy đủ? | Không trả · < 50.000đ (~2 USD) · 50–100 nghìn (~2–4 USD) · 100–200 nghìn (~4–8 USD) · > 200 nghìn |
| Q20 | Khoảnh khắc bạn thích nhất | Chữ |
| Q21 | Điều làm bạn khó chịu nhất | Chữ |
| Q22 | Nếu được đổi một điều, bạn đổi gì? | Chữ |
| Q23 | Lỗi bạn gặp (nếu có): đang làm gì, ngày mấy | Chữ |

## Cách chấm

| Chỉ số | Cách tính | Ngưỡng đề xuất | Dùng cho |
| --- | --- | --- | --- |
| **Điểm độ hay** (0–100) | Trung bình Q9, Q10, Q12–Q18, quy ra 0–100: (TB − 1) / 4 × 100 | ≥ 65 tốt · 50–65 cần sửa · < 50 xem lại vòng lặp chính | Báo cáo tổng thể mục 6 |
| **Ý định** (1–5) | Trung bình Q12, Q13, Q14 | ≥ 3,5 → phát hành 1.0; thấp hơn → Early Access / sửa | `STEAM-DEPLOY-PLAN.md` mục 5 |
| **Độ khó** | Phân bố Q11 + Q5 | Đa số 2–4; ≥ 60% người mới qua lần trả nợ 1 | Điều 1, nhóm C |
| **Hiểu luật** | % "Có" mỗi câu Q7a–e | Câu nào ≥ 2 người "Không" → nhóm B (sửa hướng dẫn) | Impl plan §2 |
| **Giá** | Trung vị Q19 | So với đề xuất 4,99–7,99 USD | `STEAM-DEPLOY-PLAN.md` mục 6 |

Câu trả lời chữ (Q8, Q20–Q23): gom theo chủ đề, đếm số người nhắc; mục nào ≥ 2 người nhắc vào `INBOX.md`.
