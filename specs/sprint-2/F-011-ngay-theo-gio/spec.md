# F-011 — Ngày theo giờ, khách không giới hạn

Trạng thái: DONE (15/11/2026) · Nguồn: constitution 1.4 §2, §6.1, §6.2, §6.7 · Duyệt: AC + kịch bản 10 giây trong `docs/sprint-2/impl-plan.md`; người dùng yêu cầu triển khai 15/11/2026

## Luật

1. Đồng hồ 9:00 → 21:00, 1 giờ = 40 giây thực, chạy từ đầu ngày kể cả khi chưa mở quán. *(§2, §6.2; Q-21 dùng mặc định của constitution)*
2. Bấm biển Mở quán: khách đầu tiên vào ngay; vòng 1 khách kế tiếp vào **3 giây** sau khi khách trước được phục vụ; không giới hạn số khách. *(§6.2)*
3. 21:00: không nhận khách mới; khách đang ngồi vẫn được phục vụ, xong thì hết ngày; không còn ai ngồi thì hết ngày ngay. *(§6.2)*
4. Bấm biển khi đang mở = đóng quán sớm. *(§6.1)*

## Thiết kế

- Core: `DayClock` (giờ "14:30", `IsPastClose`), `GameSession.Tick(giây)`, `ClosedAt`, `IsClosedForNewCustomers`, `CustomersToday` = số khách đã vào.
- Game: `GameController.Update` gọi `Session.Tick`; bắt sự kiện do đồng hồ gây ra (khách mới, 21:00, hết ngày). HUD "Ngày 1 · 13:20", từ 20:00 giờ chuyển màu Coral (#E8604F, bảng màu Art Bible không có màu cam riêng). Biển tự lật "ĐÃ ĐÓNG · phục vụ nốt khách". Bong bóng "Khách n · Dễ tính N%". Màn kết thúc ngày: số khách, tiền trong ngày, giờ đóng quán.
- Bot `NaiveBot` có `BotTiming`: 4 giây mỗi lần quay thùng, 30 giây thao tác + đi lại mỗi món. Đây là giả định của `sim_balance.py` (PULL_S, OVERHEAD naive), không phải luật game.

## Ma trận AC → test

| AC | Test | Kết quả 15/11 |
| --- | --- | --- |
| 1 | `DayClockTests.AC1_*` (4); giả lập: HUD "Ngày 1 · 9:xx" | xanh |
| 2 | `DayClockTests.AC2_ChuaMoQuan_GioVanTroi_KhongCoKhach` | xanh |
| 3 | `DayClockTests.AC3_*` (3), `GameSessionTests.F004_AC9_*`, `OrderTests.AC5/AC6`; giả lập: ngày 1 phục vụ 9 khách (≥ 7) | xanh |
| 4 | `DayClockTests.AC4_*` (2); giả lập `Gio21_BienTuDong_KhachDangNgoiVanCho_PhucVuXongHetNgay` (ảnh `docs/sprint-2/ui/3d_21h_da_dong.png`) | xanh |
| 5 | `DayClockTests.AC5_*` (3) | xanh |
| 6 | `DayClockTests.AC6_*`, `GameSessionTests.F005_AC2_DongQuanSom_*` | xanh |
| 7 | `DayClockTests.AC7_*` (2); giả lập kiểm chữ màn kết thúc ngày (`3d_het_ngay_1.png`) | xanh |
| 8 | `GameSessionTests.F011_AC8_BotKhongDocChiSo_CoTinhGio_2000Van_QuaLanTraNo1`: 99,85% (1.997/2.000), 7,4 khách/ngày; mô phỏng 96%; ngưỡng ±5 điểm | xanh |

Ghi chú AC 8: bot C# cao hơn mô phỏng 3,85 điểm vì chưa có cửa hàng relic (sprint 3); naive trong mô phỏng tiêu vé vào relic từ ngày 2 nên có ít thùng hơn.

## Ngoài phạm vi

3 khách cùng lúc, giờ chờ (vòng 2, sprint 4); chủ nợ 20:00 ngày 21 (`day.boss_hour` đã có trong dữ liệu, chưa dùng).
