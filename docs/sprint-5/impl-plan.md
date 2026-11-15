# Impl plan — Sprint 5: Vòng 3–4 và chủ nợ → Alpha

Thời gian: 6/12 → 12/12/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` v1.4 §2 (lịch 22 ngày, nợ 10.500G và 32.000G), §4.3 (bảng vòng 3, 4), §4.5, §5.4 (Món Hy Vọng), §5.5 (nước sốt từ ngày 9), §6.2 (chủ nợ lúc 20:00), §6.3 (độ dễ tính vòng 3, 4, chủ nợ 5%), §6.7 (chủ nợ gọi món, Q-20), §7.1 (mua relic bằng vé đen), §8 (thưởng 11.500G).
Điều kiện vào sprint: Q-20 đã có câu trả lời.

## 1. Mục tiêu (Alpha)

Cuối ngày 12/12, một ván chơi đi được từ ngày 1 tới màn thắng hoặc thua ở ngày 22:

1. Vòng 3 (ngày 9–14): khách khó tính hẳn (20% dưới 35%); thêm bông cải, ớt chuông, cá hồi, mực.
2. Vòng 4 (ngày 15–21): thêm bí đỏ, măng tây, nấm đông cô, gà ta, vỉ nướng dùng thật.
3. Từ ngày 9 nước sốt có 4% ra **Món Hy Vọng** (Huyền thoại).
4. Vé đen mua được relic: 3 lựa chọn Hiếm trở lên.
5. Ngày 21, 20:00: chủ nợ bước vào. Món Tuyệt hảo trở lên → thưởng 11.500G.
6. Đầu ngày 15 thu 10.500G, đầu ngày 22 thu 32.000G; đủ thì màn thắng (chủ nợ gả con gái), thiếu thì màn thua.
7. Bot C# chạy 4.000 ván mỗi kiểu trên chính luật của game.

### Ngoài phạm vi

| Không làm | Sprint |
| --- | --- |
| Tranh cốt truyện, menu, lưu game | 6 |
| Chỉnh số cân bằng (chỉ đo, chưa chỉnh) | 7 |

## 2. Tính năng và tiêu chí chấp nhận

### F-040 Nội dung vòng 3–4 — §4.3, §4.5, §5.8, §6.3

| AC | Then |
| --- | --- |
| 1 | Bảng bậc và khoảng độ dễ tính đúng từng vòng; test thống kê ±1 điểm |
| 2 | Nguyên liệu, công thức, đế món mở khóa đúng vòng; sách tự thêm trang; giá gốc 85G / 100G |
| 3 | 8 nguyên liệu mới có hình riêng (ghép khối chi tiết) và hình đã sơ chế |
| 4 | Vỉ nướng có quá trình nấu riêng (vệt nướng, lật), hấp trong nồi có nắp và hơi |

### F-041 Món Hy Vọng — §5.4, §5.5

| AC | Then |
| --- | --- |
| 1 | Trước ngày 9: 100.000 lần dùng nước sốt không ra Huyền thoại |
| 2 | Từ ngày 9: tỉ lệ đúng bảng (hỏng 12 / giữ 16 / +1: 48 / +2: 20 / Huyền thoại 4), hiện cho người chơi |
| 3 | Món Hy Vọng: viền đĩa vàng, hào quang, mặt kính đen + vương miện, tim vàng (Art Bible §7); giá ×6,0 |

### F-042 Relic bằng vé đen — §7.1

| AC | Then |
| --- | --- |
| 1 | Dùng 1 vé đen ở Góc 3: hiện 3 relic Hiếm trở lên, ít nhất một cái thuộc bộ đang có nhiều món nhất; chọn 1 |
| 2 | Chưa có relic bộ nào thì 3 cái bất kỳ Hiếm trở lên |

### F-043 Chủ nợ — §2, §6.2, §6.3, §6.7, §8

| AC | Given | When | Then |
| --- | --- | --- | --- |
| 1 | Ngày 21, quán mở | 20:00 | Chủ nợ vào, chiếm một ghế, dễ tính 5%, ngồi tới khi được phục vụ (kể cả sau 21:00) |
| 2 | Chủ nợ đang ngồi | Nhìn bong bóng | Ghi món theo câu trả lời Q-20 (mặc định: "món ngon nhất của quán") |
| 3 | — | Phục vụ Tuyệt hảo hoặc Huyền thoại | Tiền món + 11.500G; mặt chủ nợ đổi sang Ngạc nhiên (Art Bible §6) |
| 4 | — | Phục vụ món thấp hơn | Tính như khách khó tính thường, không thưởng |
| 5 | Quán chưa mở lúc 20:00 | — | Chủ nợ đợi ngoài cửa, vào ngay khi mở quán; đóng quán mà chưa phục vụ thì mất thưởng |

### F-044 Thắng, thua, trả nợ — §2

| AC | Then |
| --- | --- |
| 1 | Đầu ngày 15 thu 10.500G, đầu ngày 22 thu 32.000G; thiếu thì thua ngay |
| 2 | Qua ngày 22: màn thắng (chữ; tranh làm ở sprint 6) |

### F-045 Bot trên luật thật — Điều 1, §9

| AC | Then |
| --- | --- |
| 1 | 3 kiểu bot (không đọc chỉ số, chơi tốt không gom bộ, gom bộ), mỗi kiểu 4.000 ván, chạy bằng một lệnh, dưới 10 phút |
| 2 | In cùng dạng bảng với `sim_balance.py`; ghi chênh lệch từng mốc. Chênh quá 5 điểm thì tìm nguyên nhân (luật game hay giả định mô phỏng) và ghi vào báo cáo |

**Kịch bản 10 giây (duyệt ngày 1):** ngày 21, đồng hồ nhảy sang 20:00. Chuông cửa kêu, HUD báo "Chủ nợ đã tới". Ông ngồi ghế giữa, bong bóng viền vàng: "Chủ nợ · Dễ tính 5% · Gọi: món ngon nhất của quán". Bạn mở tủ lạnh lấy đĩa Tuyệt hảo để dành từ ngày 19, mang ra, bấm. Mặt ông từ lạnh lùng sang ngạc nhiên; "Thưởng 11.500G" hiện cùng tiền món.

## 3. Lịch 7 ngày

| Ngày | Việc |
| --- | --- |
| 1 | Spec + kịch bản; Core: vòng 3–4, nối 22 ngày, thua / thắng |
| 2 | Core: Món Hy Vọng, relic vé đen; test thống kê |
| 3 | Core: chủ nợ; bot 3 kiểu |
| 4 | Game: 8 nguyên liệu mới, vỉ nướng, hấp |
| 5 | Game: chủ nợ (model khối hiện có + mặt lạnh lùng), Món Hy Vọng, màn thắng / thua |
| 6 | Giả lập PlayMode: nhảy tới ngày 21 bằng lệnh gỡ lỗi, chơi trọn ngày 21 và 22; chạy bot 4.000 ván |
| 7 | Build Alpha, báo cáo: bảng bot so với mô phỏng; bạn chơi thử từ đầu |

## 4. Kiểm

EditMode + thống kê. Bot là phép kiểm chính của sprint. PlayMode không chơi đủ 22 ngày (quá lâu): dùng lệnh gỡ lỗi chỉ có trong Editor để dựng trạng thái ngày 20.

## 5. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Bot lệch xa mô phỏng | Không chỉnh số trong sprint này; ghi lại, xử lý ở sprint 7 |
| Một ván 22 ngày ≈ 2,8 giờ, bạn khó chơi thử hết | Lệnh gỡ lỗi "bắt đầu từ vòng N với tiền và relic trung bình của bot" |

## 6. Câu hỏi

- Q-20 (chủ nợ gọi món gì).
- Ngày 14 "all-in" có gì đặc biệt không, khi khách đã không giới hạn (§6.2)?
