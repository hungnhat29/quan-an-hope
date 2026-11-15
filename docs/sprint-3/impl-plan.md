# Impl plan — Sprint 3: Relic

Thời gian: 22/11 → 28/11/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` v1.4 §7 (relic), §4.4 (thùng đột phá), §3 (vé trắng), Điều 9. Nguồn số: `balance/relics.json`, `balance/balance.json` (`set_bonus`, `breakthrough`, `relic_cap`). Hình: Art Bible §8 (Góc 3: ba bệ relic, giá 8 ô), §13.
Điều kiện vào sprint: sprint 2 đã nghiệm thu; bộ số 1.4 là bộ số duy nhất.

## 1. Mục tiêu

Cuối ngày 28/11, trong vòng 1 (ngày 1–3) bạn:

1. Từ ngày 2, tới Góc 3 thấy 3 relic khác nhau đặt trên 3 bệ, mỗi relic có thẻ ghi tên, giá (vé), độ hiếm, hiệu ứng.
2. Nhìn vào relic, bấm để mua bằng vé trắng; relic bay lên giá 8 ô. Đầy 8 thì phải chọn bỏ một cái.
3. Thấy hiệu ứng thật: ví dụ có Dao phay cũ thì nguyên liệu chính lên bậc nhiều hơn; đủ 2 / 3 / 4 món một bộ thì thưởng bộ bật.
4. Có relic Sơn Hào thì thỉnh thoảng rơi thêm thùng đột phá 1 món (bậc 4–5), nấu được món đột phá.

### Ngoài phạm vi

| Không làm | Sprint |
| --- | --- |
| Mua relic bằng vé đen (3 lựa chọn Hiếm trở lên) | 5 |
| Hiệu ứng của relic gắn với hệ thống chưa có (Tủ lạnh 2 cửa, Đồng hồ cát, Máy pha cà phê, Hũ sốt cũ): mua và giữ được, có nhãn "có tác dụng từ ngày 4" | 4 |
| Model relic đẹp | 8 |

## 2. Tính năng và tiêu chí chấp nhận

### F-020 Cửa hàng relic — §7.1

| AC | Given | When | Then |
| --- | --- | --- | --- |
| 1 | Ngày 1 | Tới Góc 3 | Ba bệ trống, biển "mở từ ngày 2" |
| 2 | Đầu ngày ≥ 2 | — | 3 relic khác nhau, không trùng relic đang có |
| 3 | Đủ vé | Nhìn relic, bấm | Trừ đúng số vé, relic lên giá, bệ đó trống tới hết ngày |
| 4 | Thiếu vé | Bấm | Không mua được, gợi ý ghi thiếu mấy vé |
| 5 | Đang có 8 relic | Mua cái thứ 9 | Phải bấm chọn một relic trên giá để bỏ; huỷ thì không mua |
| 6 | Mỗi relic | Nhìn thẻ | Thẻ ghi tên, giá, độ hiếm, hiệu ứng (Điều 4: hiện giá và hiệu ứng relic) |

### F-021 Hiệu ứng 22 relic và thưởng bộ — §7.2, §7.3

| AC | Then |
| --- | --- |
| 1 | Mỗi relic có ít nhất một test: có relic thì luật đổi đúng như bảng (ví dụ Rổ tre: thùng đồ phụ 4 món; Thuyền thuê: 2 món) |
| 2 | "x% nâng bậc" áp cho từng món trong thùng, tối đa bậc 5; test thống kê ±1 điểm (Điều 3) |
| 3 | Thưởng bộ bật đúng ngưỡng 2 / 3 / 4 món, đúng số trong `set_bonus` |
| 4 | Giá relic 8 ô ghi tiến độ bộ ("Chợ Sớm 2/4") ngay trên giá |
| 5 | Bảng tỉ lệ ở Góc 2 hiện thêm tác dụng relic đang có lên tỉ lệ (Điều 4) |

### F-022 Thùng đột phá — §4.4

| AC | Then |
| --- | --- |
| 1 | Không có relic Sơn Hào: 100.000 lượt gacha không ra thùng đột phá nào |
| 2 | Có relic: tỉ lệ ra thùng = tổng `bt_chance`; mỗi thùng 1 món (2 món khi có thưởng bộ 3), 70% bậc 4 / 30% bậc 5 (±1 điểm) |
| 3 | Thùng đột phá là rương nắp vòm màu tím viền vàng (Art Bible §7), món bên trong dùng thay nguyên liệu chính, nấu ra món đột phá đúng công thức §5.8 |
| 4 | Khách có thể gọi món đột phá khi kho đang có nguyên liệu đó (§6.7) |

**Kịch bản 10 giây (duyệt ngày 1):** sáng ngày 2 bạn quay sang Góc 3. Trên ba bệ là một bình tưới gỉ, một con dao phay, một cuộn bản đồ. Trước mỗi bệ có thẻ dựng: "Dao phay cũ · 1 vé · Thường · Nguyên liệu chính: 15% nâng bậc · Bộ Chợ Sớm". Bạn nhìn con dao, gợi ý ghi "Mua · 1 vé (còn 6)". Bấm: dao bay lên ô đầu của giá, giá ghi "Chợ Sớm 1/4", HUD còn 5 vé.

Bảng Art Bible → làm ở đâu: §8 ba bệ và giá 8 ô (đã có trong cảnh) → gắn tương tác; §13 thẻ trên đồ vật, cùng khung UI → `RoomView.BuildRelicShop`; §4 màu độ hiếm → viền thẻ; §11 lấp lánh khi mua → `KitchenFx`.

## 3. Lịch 7 ngày

| Ngày | Việc | Xong khi |
| --- | --- | --- |
| 1 | Spec F-020–022, kịch bản cho bạn duyệt; Core `RelicInventory`, `RelicShop` (test đỏ → xanh) | AC F-020 2–5 ở Core |
| 2 | Hiệu ứng bộ Vườn Nhà + Chợ Sớm, thưởng bộ | Test từng relic xanh |
| 3 | Bộ Sơn Hào + thùng đột phá + test thống kê | F-022 AC 1–2 |
| 4 | 10 relic lẻ (những cái có hệ thống); bot biết mua relic; đối chiếu mô phỏng vòng 1 | Bot ±5 điểm so với Python |
| 5 | Góc 3: bệ, thẻ, mua, giá 8 ô, bỏ relic | Giả lập PlayMode mua được |
| 6 | Rương đột phá, 4 nguyên liệu đột phá (hình ghép khối), món đột phá; bảng tỉ lệ có relic | Ảnh chụp |
| 7 | Build, báo cáo nghiệm thu, bạn chơi thử | "accept" |

## 4. Kiểm

EditMode: mỗi relic một test + test thống kê. PlayMode: người chơi giả mua 1 relic ngày 2, kiểm vé, giá, thẻ; ép đầy 8 để kiểm bỏ relic. Mô phỏng: không đổi số nên không cần chạy lại, chỉ đối chiếu bot.

## 5. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| 22 hiệu ứng quá nhiều cho 3 ngày | Làm trước 12 relic bộ; relic lẻ chưa kịp thì hiện trong cửa hàng với nhãn "sắp có" và không bán |
| Vòng 1 chỉ 3 ngày nên khó thấy thưởng bộ | Thêm lệnh gỡ lỗi "cho tôi relic X" chỉ có trong Editor để bạn thử |

## 6. Câu hỏi

- Bỏ relic khi đầy: bấm vào relic trên giá để bỏ (mặc định), hay có thùng rác riêng?
- Relic đã bỏ có quay lại cửa hàng không? Mặc định: có, như relic chưa có.
