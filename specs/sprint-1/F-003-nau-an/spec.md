# F-003 — Nấu ăn

Trạng thái: phần logic DONE (14/11/2026); AC-5, AC-6 phần hiển thị làm ngày 6 · Nguồn: `specs/constitution.md` v1.1 · Duyệt: bảng AC trong `docs/sprint-1/impl-plan.md`, người dùng yêu cầu thực hiện ngày 3 (14/11/2026)

## Luật

1. Mỗi món = 1 nguyên liệu chính + 1 đồ phụ + đồ kho. Đồ kho vô hạn, không có bậc, không ảnh hưởng bậc món. *(§5.1, §4.5)*
2. Thớt: giữ đủ thời gian sơ chế của nguyên liệu. *(§5.1, §4.5)*
3. Bếp: chọn dụng cụ theo công thức, cho từng thứ vào đúng thứ tự, rồi chờ đủ thời gian nấu. *(§5.1, §5.8)*
4. Cho sai thứ tự thì món ra Lạc vị. *(§5.1)*
5. Bậc món: `b = 0,6 × bậc chính + 0,4 × bậc phụ + 0,8`; ra `floor(b)` hoặc `floor(b)+1`, xác suất lên bậc trên bằng phần lẻ của `b`; kẹp trong [Lạc vị, Tuyệt hảo]. *(§5.3)*
6. Khoảng bậc món hiện cho người chơi khi đang nấu. *(Điều 4, §5.3)*
7. Quầy chờ giữ tối đa 3 đĩa đã nấu trong ngày, không giảm bậc. *(§5.1)*
8. Tên, dụng cụ, đế món, thứ tự và thời gian nấu của món do cặp nguyên liệu chính + đồ phụ quyết định. *(§5.8)*
9. Vòng 1 không có quá lửa. *(§5.9)*

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Bậc chính và bậc phụ | Tính và quay bậc món | Đúng công thức ở luật 5; chính 5 + phụ 5 vẫn chỉ Tuyệt hảo; chính 0 + phụ 0 cho 20% Lạc vị, 80% Dở | §5.3 |
| 2 | Năm cặp trong bảng §5.3 | Xem thanh tỉ lệ | 1+1 → 20% Dở, 80% Bình thường · 0+1 → 80% Dở, 20% Bình thường · 2+2 → 20% Bình thường, 80% Ngon · 3+3 → 20% Ngon, 80% Tuyệt hảo · 4+2 → 100% Tuyệt hảo | §5.3 |
| 3 | Chính 1 + phụ 1 | Quay 100.000 lần | 20% Dở và 80% Bình thường, lệch không quá ±1 điểm | Điều 3 |
| 4 | Đủ 1 chính + 1 phụ trên bếp | Nấu với thứ tự khác công thức: đảo nguyên liệu, đảo đồ kho, thiếu hoặc thừa đồ kho | Món ra Lạc vị. Đúng thứ tự thì món ra bậc theo công thức | §5.1, §5.8 |
| 5 | Đã cho đủ chính và phụ | Nhìn bếp | Thấy thanh tỉ lệ bậc món; chưa đủ thì chưa có thanh | Điều 4, §5.3 |
| 6 | Nguyên liệu vừa lấy từ thùng | Nấu một món | Phải sơ chế trên thớt đủ thời gian của nguyên liệu, chọn dụng cụ trước khi cho đồ vào, chờ đủ thời gian nấu mới múc được | §5.1, §4.5, §5.8 |
| 7 | Quầy chờ có 3 đĩa | Múc món thứ tư | Không múc được, món ở lại trên bếp; lấy bớt một đĩa thì múc được. Đĩa trên quầy giữ nguyên bậc | §5.1 |
| 8 | Một cặp chính + phụ của vòng 1 | Tra công thức | Ra đúng tên, dụng cụ, đế món, thời gian nấu của 9 món vòng 1 | §5.8 |

## Mặc định cho chỗ constitution chưa nói (cần bạn xác nhận)

- **Q-11 Sai dụng cụ:** constitution chỉ nói "sai thứ tự → Lạc vị". Code đang coi chọn sai dụng cụ (ví dụ nấu trứng chiên bằng nồi) cũng là sai công thức → Lạc vị.
- **Bắt đầu nấu:** người chơi bấm nấu sau khi cho xong; bếp không tự bắt đầu. Cần đủ 1 chính + 1 phụ mới bấm được.
- **Sơ chế dở dang:** nhả tay giữa chừng không mất tiến độ; có lệnh hủy riêng.

## Ngoài phạm vi

- Quá lửa (từ vòng 2), +25% thời gian nấu ngày mất điện, −0,3 điểm ngày mất điện (sprint 3).
- Hiệu ứng relic lên điểm món (sprint 2).
- Nước sốt, máy gacha đồ ăn, tủ lạnh (sprint 3).
- Vỉ nướng và món vòng 2–4: dữ liệu đã có, chưa kiểm riêng.

## Bổ sung dữ liệu

`balance.json` thêm `dish.max_tier: 4` (§5.3: giới hạn tới Tuyệt hảo) và `counter_slots: 3` (§5.1).

## Ma trận AC → test

| AC | Test (`Tests/EditMode/`) | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `DishFormulaTests.AC1_*` (3) | xanh |
| 2 | `DishFormulaTests.AC2_*` (5) | xanh |
| 3 | `DishFormulaTests.AC3_*` (1) | xanh |
| 4 | `StoveTests.AC4_*` (7) | xanh |
| 5 | `StoveTests.AC5_*` (1) — phần logic | xanh; hiển thị CẦN NGƯỜI KIỂM, ngày 6 |
| 6 | `StoveTests.AC6_*` (6), `KitchenTests.AC6_*` (3) — phần logic | xanh; thao tác CẦN NGƯỜI KIỂM, ngày 6 |
| 7 | `KitchenTests.AC7_*` (3), `StoveTests.AC7_*` (1) | xanh |
| 8 | `KitchenTests.AC8_*` (9 món + 2) | xanh |
