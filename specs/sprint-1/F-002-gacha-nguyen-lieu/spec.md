# F-002 — Vé và gacha nguyên liệu

Trạng thái: phần logic DONE (14/11/2026); AC-7 (hiển thị) làm ngày 5 · Nguồn: `specs/constitution.md` v1.1 · Duyệt: bảng AC trong `docs/sprint-1/impl-plan.md`, người dùng yêu cầu thực hiện ngày 2 (14/11/2026)

## Luật

1. Mỗi ngày nhận 3 vé trắng, không dùng thì cộng dồn; ngày 1 có thêm 2 vé thưởng. *(§3)*
2. Một lượt gacha tốn 1 vé, thả 1 thùng đồ phụ (5 món) và 1 thùng nguyên liệu chính (3 món). *(§4.2)*
3. Mỗi thùng chỉ có một loại nguyên liệu, chọn ngẫu nhiên đều trong các loại đã mở khóa ở vòng hiện tại. *(§4.2, §4.5)*
4. Mỗi món tự quay bậc độc lập theo bảng của vòng. *(§4.2, §4.3)*
5. Ngày 3 (sau mưa) dùng bảng riêng và thùng nhỏ lại: 3 đồ phụ + 2 chính. *(§4.3)*
6. Nguyên liệu không dùng hết bị bỏ cuối ngày. *(§4.2)*
7. Tỉ lệ gacha nguyên liệu theo vòng hiện cho người chơi. *(Điều 4)*

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Ván mới | Bắt đầu ngày 1; rồi ngày 2 sau khi tiêu 2 vé | Ngày 1 có 5 vé; ngày 2 có 6 vé (3 thừa + 3 mới) | §3 |
| 2 | Còn ít nhất 1 vé | Quay một lượt | Mất 1 vé, nhận 5 đồ phụ + 3 chính. Hết vé thì không quay được và không nhận gì | §4.2 |
| 3 | Ngày 3 | Quay một lượt | Nhận 3 đồ phụ + 2 chính | §4.3 |
| 4 | Ngày 1, 2 hoặc 3 | Quay | Ngày 1–2 dùng bảng vòng 1, ngày 3 dùng bảng sau mưa; mỗi món một lần quay bậc riêng | §4.3 |
| 5 | Một bảng tỉ lệ bậc | Quay 100.000 lần | Mỗi bậc lệch không quá ±1 điểm phần trăm; bậc 0% không bao giờ ra | Điều 3 |
| 6 | Kho còn nguyên liệu | Kết thúc ngày | Kho trống | §4.2 |
| 7 | Đang ở Góc 2 | Nhìn bảng tỉ lệ | Thấy tỉ lệ 6 bậc của vòng hiện tại | Điều 4 |
| 8 | Vòng 1 | Quay nhiều lượt | Mỗi thùng một loại; chỉ ra cà chua, hành tây, rau muống và trứng gà, thịt heo, đậu phụ; mỗi loại xấp xỉ 1/3 (±1 điểm trên 30.000 lượt) | §4.2, §4.5, Điều 3 |

## Ngoài phạm vi

- Thùng đột phá và mọi hiệu ứng relic lên gacha (sprint 2).
- Gacha vé đen (sprint 3).
- Hiệu ứng thùng rơi, lộ bậc (ngày 5 trở đi).

## Bổ sung dữ liệu

`balance.json` thêm `storm_day: 3`. Con số này đã có trong constitution §4.3 nhưng trước đây chỉ viết sẵn trong `sim_balance.py`; code game không được viết số (Điều 2). Mô phỏng chạy lại cho kết quả không đổi.

## Ma trận AC → test

| AC | Test (`Tests/EditMode/`) | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `TicketWalletTests.AC1_*` (2) | xanh |
| 2 | `TicketWalletTests.AC2_*` (1), `IngredientGachaTests.AC2_*` (2) | xanh |
| 3 | `IngredientGachaTests.AC3_*` (2) | xanh |
| 4 | `IngredientGachaTests.AC4_*` (2) | xanh |
| 5 | `TierTableStatTests.AC5_*` (6 bảng + gacha ngày 1 và 3) | xanh |
| 6 | `IngredientGachaTests.AC6_*` (1) | xanh |
| 7 | — | CẦN NGƯỜI KIỂM, ngày 5 |
| 8 | `IngredientGachaTests.AC8_*` (4) | xanh |
