# F-005 — Vòng ngày và trả nợ

Trạng thái: DONE (14/11/2026) · Nguồn: `specs/constitution.md` v1.1 · Duyệt: bảng AC trong `docs/sprint-1/impl-plan.md`, người dùng yêu cầu thực hiện ngày 4 (14/11/2026)

## Luật

1. Khởi đầu 100G và 5 vé. *(§3)*
2. Một ngày: chuẩn bị (gacha, nấu trước) → Mở quán → phục vụ lần lượt → hết khách thì hết ngày. *(§2, §6.2; người dùng 14/11)*
3. Cuối ngày bỏ nguyên liệu thừa; đĩa trên quầy chờ bị bỏ vì chưa có tủ lạnh. *(§4.2, §5.1)*
4. Nợ thu vào đầu ngày đến hạn; không đủ tiền là thua. *(§2)*
5. Người chơi "không đọc chỉ số" phải qua lần trả nợ 1. *(Điều 1: mô phỏng 100%)*

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Ván mới | — | Ngày 1, 100G, 5 vé, nợ sắp tới 400G đầu ngày 4 | §3, §2 |
| 2 | Đã mở quán | Phục vụ xong khách cuối, hoặc đóng quán sớm | Hết ngày: kho trống, quầy chờ trống. Sang ngày mới: +3 vé cộng dồn | §2, §4.2, §5.1 |
| 3 | Hết ngày 3 | Sang ngày 4 | Đủ 400G thì bị trừ 400G; thiếu thì thua và không bị trừ | §2 |
| 4 | Đã trả 400G | — | Màn "Qua vòng 1", bản chơi thử dừng | phạm vi sprint |
| 5 | Bot không đọc chỉ số | Chơi 2.000 ván trên luật thật | Tỉ lệ qua lần trả nợ 1 ≥ 99% (mô phỏng: 100%) | Điều 1 |

## Mặc định cho chỗ constitution chưa nói (cần bạn xác nhận)

- **Q-14 Đóng quán sớm:** có nút đóng quán khi đang mở; khách còn lại ra về, không trả và không phạt. Lý do: nếu hết nguyên liệu lẫn vé thì không còn cách nào phục vụ, ngày sẽ kẹt mãi.
- **Q-15 Tiền âm:** tiền đền bù có thể làm ví xuống dưới 0.
- Gacha và nấu được cả trước lẫn sau khi mở quán.

## Ngoài phạm vi

- Ngày 4 trở đi; số khách ngày 14; chủ nợ ngày 21; thưởng boss (sprint 3–4).
- Đồng hồ 9:00–21:00 (Q-5b).
- Lưu và tải ván chơi.

## Ma trận AC → test

| AC | Test | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `GameSessionTests.F005_AC1_*` | xanh |
| 2 | `GameSessionTests.F005_AC2_*` (4) | xanh |
| 3 | `GameSessionTests.F005_AC3_*` (3) | xanh |
| 4 | `GameSessionTests.F005_AC3_AC4_*`; giả lập `ChoiTronVong1_BangNutBam_QuaVong1` | xanh |
| 5 | `GameSessionTests.F005_AC5_*`: 2.000/2.000 ván qua (100%), trung bình 958G trước khi trả nợ | xanh |
| F-004 AC-9 | `GameSessionTests.F004_AC9_*` (2); giả lập | xanh |
