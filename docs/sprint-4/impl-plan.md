# Impl plan — Sprint 4: Vòng 2

Thời gian: 29/11 → 5/12/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` v1.4 §2 (ngày 4–8, nợ 2.900G), §4.3 (bảng vòng 2), §4.5 (khoai tây, nấm rơm, thịt bò, tôm), §5.5–5.7, §5.9 (quá lửa), §6.1–6.2 (3 khách cùng ngồi), §6.6 (giờ chờ), §6.7, §8. Hình: Art Bible §4 (ngày mất điện), §8, §13.
Điều kiện vào sprint: đã đo thời gian thật một món ở sprint 2 (F-014) và chỉnh lại nợ nếu lệch.

## 1. Mục tiêu

Cuối ngày 5/12, bạn chơi liền từ ngày 1 tới đầu ngày 9:

1. Qua vòng 1 (trả 750G) thì sang ngày 4 chứ không dừng.
2. Vòng 2 có 3 khách cùng ngồi ở quầy, mỗi người một bong bóng món gọi; bạn chọn phục vụ ai trước.
3. Khách chờ lâu thì kém dễ tính dần; có thanh kiên nhẫn trên bong bóng.
4. Có tủ lạnh 2 ô để dành món; để quên trên bếp thì món quá lửa.
5. Có quầy cửa hàng bán nước sốt thần thánh, máy gacha đồ ăn, vé đen.
6. Ngày 8 mất điện: phòng tối, nấu bằng nến, không thấy bậc.
7. Đầu ngày 9 bị thu **2.900G**.

### Ngoài phạm vi

| Không làm | Sprint |
| --- | --- |
| Vòng 3–4, Món Hy Vọng (4% từ ngày 9), mua relic bằng vé đen, chủ nợ | 5 |
| Lưu game giữa chừng | 6 |

## 2. Tính năng và tiêu chí chấp nhận

### F-030 Ba khách cùng ngồi — §6.1, §6.2, §6.7

| AC | Given | When | Then |
| --- | --- | --- | --- |
| 1 | Vòng 2, quán mở | — | Tối đa 3 khách ngồi 3 ghế; ghế trống thì 3 giây sau có khách mới, tới 21:00 |
| 2 | 3 khách đang ngồi | Nhìn từng khách | Mỗi người một bong bóng: dễ tính, món gọi, thanh kiên nhẫn |
| 3 | Cầm đĩa | Nhìn vào một khách (hoặc chuông trước mặt khách đó), bấm | Đúng khách đó nhận đĩa; đúng / sai món tính như §6.5 |
| 4 | 21:00 | — | Không thêm khách; phục vụ hết người đang ngồi thì hết ngày |

### F-031 Giờ chờ — §6.6

| AC | Then |
| --- | --- |
| 1 | Vòng 2: sau 90 giây an toàn, độ dễ tính giảm 1 điểm mỗi 3 giây, tối đa −20 |
| 2 | Vòng 1 không tính giờ; ngày 8 an toàn 110 giây |
| 3 | Tiền và bậc tối thiểu tính theo độ dễ tính **lúc phục vụ** |
| 4 | Relic Đồng hồ cát (+40%), Máy pha cà phê (+60%) kéo dài khoảng an toàn |

### F-032 Tủ lạnh — §5.7

| AC | Then |
| --- | --- |
| 1 | Tủ 2 ô ở Góc 4; cầm đĩa bấm tủ để cất, bấm ô để lấy |
| 2 | Cuối ngày đĩa trên quầy chờ tự vào tủ nếu còn ô, không thì bị bỏ (§5.1) |
| 3 | Món để quá 3 ngày tụt 1 bậc; relic Tủ lạnh 2 cửa: +2 ô, không tụt |
| 4 | Món trong tủ tính là "làm được" khi khách gọi món (§6.7) |

### F-033 Quá lửa — §5.9

| AC | Then |
| --- | --- |
| 1 | Từ vòng 2: để trên bếp quá 1,5× thời gian nấu thì bếp cảnh báo (khói, bảng bếp đổi màu) |
| 2 | Quá 2× thì món −1 bậc; vòng 1 không có |

### F-034 Cửa hàng vật phẩm — §8, §5.5, §5.6, §4.3 (vé đen)

| AC | Then |
| --- | --- |
| 1 | Từ ngày 4 có quầy bán: nước sốt 450G (+45G mỗi lần), máy gacha đồ ăn 800G (1 lần), vé đen 700G (+120G mỗi lần); giới hạn giữ 20 / 1 / 5 |
| 2 | Nước sốt dùng lên một đĩa: tỉ lệ ngày 4–8 đúng bảng §5.5, hiện cho người chơi; test thống kê |
| 3 | Máy gacha đồ ăn: mỗi món 1 lần, 3 lần mỗi ngày, tỉ lệ đúng §5.6 và **không hiện** (Điều 4) |
| 4 | Vé đen nhét vào máy: thùng theo bảng 0/0/0/62/30/8 |

### F-035 Ngày 8 mất điện — §2, §5.3, §5.9, Điều 4

| AC | Then |
| --- | --- |
| 1 | Ngày 8: phòng tối 60%, ánh nến quanh bếp và quầy (Art Bible §4) |
| 2 | Giấu khoảng bậc món và màu bậc nguyên liệu; điểm món −0,3; thời gian nấu +25% |

### F-036 Nội dung vòng 2 — §4.3, §4.5, §5.8

Bảng bậc vòng 2; thêm khoai tây, nấm rơm, thịt bò, tôm (hình ghép khối chi tiết như 6 loại vòng 1); đế món Mì Ý, Bún; 16 món mới trong sách; giá gốc 70G.

**Kịch bản 10 giây (duyệt ngày 1):** ngày 5, 15:40. Ba khách ngồi ở quầy. Bong bóng người giữa có thanh kiên nhẫn sắp chuyển cam, ghi "Dễ tính 52% · Gọi: Bún xào tôm khoai tây". Bạn cầm đĩa bún từ quầy chờ ra, nhìn vào người giữa, gợi ý "Phục vụ khách giữa: Bún xào tôm khoai tây", bấm. Người đó vui, tiền bay về ví; ba giây sau ghế đó có khách mới.

## 3. Lịch 7 ngày

| Ngày | Việc |
| --- | --- |
| 1 | Spec + kịch bản; Core: nhiều ghế, giờ chờ (test đỏ → xanh); nối vòng 1 → vòng 2 |
| 2 | Core: tủ lạnh, quá lửa; nội dung vòng 2 (dữ liệu đã có) |
| 3 | Core: cửa hàng vật phẩm, nước sốt, máy gacha đồ ăn, vé đen + test thống kê |
| 4 | Game: 3 ghế, 3 bong bóng, thanh kiên nhẫn, phục vụ theo khách |
| 5 | Game: tủ lạnh, quầy cửa hàng, lọ nước sốt, máy gacha đồ ăn (đã có khối ở Góc 2) |
| 6 | Ngày mất điện; 4 nguyên liệu mới; giả lập PlayMode ngày 4–8 |
| 7 | Mô phỏng đối chiếu bot tới ngày 9, build, nghiệm thu |

## 4. Kiểm

EditMode cho từng luật, test thống kê cho nước sốt, máy gacha, vé đen. Bot chơi tới ngày 9: tỉ lệ qua lần trả nợ 2 so với mô phỏng (79% với người không đọc chỉ số). PlayMode: một ngày vòng 2 đủ 3 khách, một lần để khách chờ quá giờ, một lần quá lửa.

## 5. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Sprint nặng nhất (6 tính năng) | Thứ tự cắt: máy gacha đồ ăn → sprint 5; ngày mất điện chỉ làm luật, ánh nến → sprint 7 |
| Khoảng an toàn 90 giây chưa mô phỏng riêng | Ngày 7 đo bằng bot có thời gian; ghi kết quả vào constitution |
| Chọn khách để phục vụ khó ngắm trong 3D | Kịch bản ngày 1 cho bạn chọn: nhìn vào khách, hay 3 chuông trước 3 ghế |

## 6. Câu hỏi

- Phục vụ: nhìn vào khách rồi bấm, hay mỗi ghế một chuông?
- Cửa hàng vật phẩm đặt ở đâu? Constitution chưa nói. Đề xuất: một quầy nhỏ cạnh máy gacha đồ ăn ở Góc 2.
- Tủ lạnh đặt ở đâu trong Góc 4? Đề xuất: cạnh quầy chờ bên phải.
