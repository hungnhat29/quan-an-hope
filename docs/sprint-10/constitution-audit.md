# Rà constitution 2.0 → game → test (F-090)

Ngày 23/11/2026. Số liệu: `tools/check_constitution.py` so 98 dòng số giữa constitution và `balance/*.json`: **0 lệch**.

| Điều / mục | Có trong game? | Test / cách kiểm |
| --- | --- | --- |
| Điều 1 độ khó | Có — bot C# 4.000 ván: 82% / 85% / 50% | `balance/run_bots.ps1` → `bot_report.md`; build: `-botrun 200` |
| Điều 2 dữ liệu tách code | Có — số trong `balance/*.json` → `BalanceData.asset` | `BalanceDataTests`, `check_constitution.py` |
| Điều 3 xác suất có test | Có (tỉ lệ bậc, sốt, máy gacha, vé đen, loại nguyên liệu) | `TierTableStatTests`, `IngredientGachaTests.AC8_*`, `ItemShopTests` |
| Điều 4 minh bạch | Có — bảng tỉ lệ Góc 2, dải bậc trên bếp, sốt / vé đen trong Tab; máy gacha giấu | `F021_*`, `F078_*`, `F034_*` |
| Điều 5 phòng 3D, tự đi | Có — 10,8 m, WASD + Shift + Space, chuột phải thả | `La3D_*`, `B16_*` |
| Điều 6 công nghệ | Có — Unity 6 URP, TMP, Blender FBX, Git LFS | build OK |
| Điều 7 phong cách | Có — viền điểm ảnh, mặt 2D, 8 kiểu mặt | `B03_*`, `B06_*` |
| Điều 8 SDD | Có — spec mỗi sprint, test trước | `specs/sprint-10/spec.md` |
| Điều 9 thông tin trên đồ vật + Tab | Có — Tab 4 thẻ, HUD chỉ số liệu + thẻ tranh hướng dẫn | `B08_*`, `B10_*`, `B11_*`, `F078_*` |
| 2 lịch 22 ngày, nợ | Có | `F035_*`, `F043_F044_*`, `check_constitution.py` |
| 2.1 menu, truyện, lưu, hướng dẫn bằng hình, cài đặt | Có | `F050_*`…`F055_*`, `B08_*`, `B09_*`, `F091_AC3_*` |
| 3 tiền tệ | Có | `TicketWalletTests`, `Sprint7Tests` |
| 4.1–4.4 bậc, gacha, đột phá, ghép cặp | Có | `IngredientGachaTests`, `B14_MoiLuotQuay_*`, `F022_*` |
| 4.5 nguyên liệu mở theo vòng | Có — 6 / 18 / 34 / 56 món | `B14_*`, `Round2Tests`, `Sprint5Tests` |
| 5.1 bếp, thớt (tối đa 3), phục vụ trên bàn | Có | `KitchenTests`, `B15_*`, `B17_*` |
| 5.2 bậc món, giá niêm yết | Có | `PaymentTests`, `B12_*` |
| 5.3 công thức bậc món | Có | `DishFormulaTests` |
| 5.4–5.7 Món Hy Vọng, sốt, máy gacha, tủ lạnh | Có | `Sprint5Tests.F041_*`, `ItemShopTests`, `FridgeAndOvercookTests` |
| 5.8 84 món, thực đơn Tab, món đột phá | Có | `B14_*`, `B11_*` |
| 5.9 thời gian nấu, quá lửa, mất điện | Có | `StoveTests`, `F033_*`, `F035_*` |
| 6.1–6.2 mở / đóng quán, khách tới 21:00, ăn tại ghế, chỉ đóng quán mới hết ngày | Có | `DayClockTests`, `EatingTests`, `B18_*`, `Gio21_*` |
| 6.3–6.5 độ dễ tính, món tối thiểu, trả tiền | Có | `CustomerTests`, `PaymentTests`, `OrderTests` |
| 6.6 giờ chờ, khách bỏ đi | Có | `SeatsAndPatienceTests`, `B19_*` |
| 6.7 gọi món | Có | `OrderTests` |
| 7 relic, ngày 2 miễn phí, đổi hàng, vé đen → relic | Có | `RelicShopTests`, `RelicEffectTests`, `Sprint8Tests`, `F042_*` |
| 8 cửa hàng vật phẩm + thớt | Có | `ItemShopTests`, `B15_*`, `F034_*` |
| 9 kiểm tra | Có — bot C# là chuẩn, mô phỏng Python chỉ dò hướng | `run_bots.ps1` |

Dòng "chưa": không có. Điểm còn chờ bạn: các câu hỏi OPEN trong `.dev-memory/QUESTIONS.md` (F-090 AC-2 — mỗi câu đã ghi đề xuất "đóng" hoặc "để sau phát hành").
