# F-004 — Khách và trả tiền

Trạng thái: AC 1–7 DONE (14/11/2026); AC-9 làm ngày 4, AC-8 ngày 6 · Nguồn: `specs/constitution.md` v1.1 và tin nhắn người dùng 14/11 · Duyệt: bảng AC trong `docs/sprint-1/impl-plan.md`, người dùng yêu cầu thực hiện ngày 3 (14/11/2026)

## Luật

1. Vòng 1 có 6 khách mỗi ngày. *(§6.2)*
2. Mỗi khách có độ dễ tính (%), quay trong một khoảng; tỉ lệ rơi vào từng khoảng theo vòng. *(§6.3)*
3. Món tối thiểu khách chấp nhận phụ thuộc độ dễ tính. *(§6.4)*
4. `chênh = bậc món − bậc tối thiểu`, `khó = 1 − độ dễ tính`, `giá = giá gốc × hệ số bậc món`. *(§6.5, §5.2)*
5. Vòng 1 khách đến lần lượt; Góc 1 có nút Mở quán, bấm thì mới bắt đầu có khách. *(Người dùng, 14/11/2026)*

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Ngày 1, 2 hoặc 3 | Hỏi số khách trong ngày | 6 | §6.2 |
| 2 | Một vòng | Sinh 100.000 khách | Vòng 1: 90% trong 60–95, 10% trong 35–59, 0% trong 10–34 (±1 điểm); không khách nào nằm ngoài ba khoảng | §6.3, Điều 3 |
| 3 | Độ dễ tính của khách | Hỏi món tối thiểu | ≥80 → Dở · 60–79 → Bình thường · 35–59 → Ngon · <35 → Tuyệt hảo | §6.4 |
| 4 | `chênh ≥ 0` | Tính tiền | Trả `giá` + tip `giá × khó × (0,3 + 0,2 × chênh)` | §6.5 |
| 5 | `chênh = −1` | Tính tiền | Trả 50% `giá`, không tip | §6.5 |
| 6 | `chênh ≤ −2` | Tính tiền | Không trả; quán bị trừ 30% giá gốc của vòng | §6.5 |
| 7 | Vòng 1 | Hỏi giá một món | 60G × hệ số bậc: 12 / 30 / 60 / 108 / 192 | §5.2 |
| 8 | Có khách ở Góc 1 | Nhìn khách, bấm chuông | Thấy độ dễ tính bằng %; chuông phục vụ món | §6.3, §5.1 |
| 9 | Đầu ngày, chưa bấm Mở quán | Bấm Mở quán | Trước khi bấm không có khách; sau khi bấm khách đến lần lượt, người sau vào khi người trước đã được phục vụ | Người dùng, 14/11 |

Ví dụ đã kiểm (vòng 1): khách 85% nhận món Dở → 30 + 1 = 31G · khách 85% nhận món Ngon → 108 + 11 = 119G · khách 50% nhận món Ngon → 108 + 16 = 124G · khách 50% nhận món Tuyệt hảo → 192 + 48 = 240G · khách 70% nhận món Dở → 15G · khách 50% nhận món Dở → −18G.

## Mặc định cho chỗ constitution chưa nói (cần bạn xác nhận)

- **Q-12 Làm tròn tiền:** tiền là số nguyên G. Giá và tip làm tròn riêng từng khoản tới 1G gần nhất (0,5 làm tròn lên). Mô phỏng Python giữ số lẻ, nên game có thể lệch mô phỏng dưới 1G mỗi khách.
- Huyền thoại so với yêu cầu như Tuyệt hảo (§6.4 "Huyền thoại cũng đạt"), nhưng giá dùng hệ số ×6,0.

## Ngoài phạm vi

- Thời gian chờ làm giảm độ dễ tính (từ vòng 2, sprint 3).
- Ngoại lệ số khách ngày 14, chủ nợ ngày 21 (sprint 4).
- Relic tip và thu nhập (sprint 2).
- Phục vụ 3 khách cùng lúc (sprint 3).

## Bổ sung dữ liệu

`balance.json` thêm khối `payment` (`tip_base 0,3`, `tip_per_tier 0,2`, `under_pay_share 0,5`, `penalty_base_share 0,3`). Bốn số này có trong constitution §6.5 nhưng trước đây chỉ viết sẵn trong `sim_balance.py`.

## Ma trận AC → test

| AC | Test (`Tests/EditMode/`) | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `CustomerTests.AC1_*` (4 ngày) | xanh |
| 2 | `CustomerTests.AC2_*` (4 vòng + 2) | xanh |
| 3 | `CustomerTests.AC3_*` (9 mốc) | xanh |
| 4 | `PaymentTests.AC4_*` (3) | xanh |
| 5 | `PaymentTests.AC5_*` (1) | xanh |
| 6 | `PaymentTests.AC6_*` (3) | xanh |
| 7 | `PaymentTests.AC7_*` (6 bậc + 1) | xanh |
| 8 | — | CẦN NGƯỜI KIỂM, ngày 6 |
| 9 | `GameSessionTests` | ngày 4 |
