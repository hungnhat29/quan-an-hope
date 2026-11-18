# Báo cáo bot C# trên luật thật — sprint 5 (16/11/2026)

Lệnh (đóng Unity Editor trước): `powershell -NoProfile -ExecutionPolicy Bypass -File balance/run_bots.ps1` (tuỳ chọn `-Games 1000`).
Lệnh chạy `sim_balance.py` rồi bot C# trên `GameSession` thật, 4.000 ván mỗi kiểu, và ghi `balance/sim_report.txt`, `balance/bot_report.md`.
Thời gian: bot 43 giây cho 12.000 ván (cả lệnh ~100 giây). Không đổi con số nào (impl plan: sprint 7 mới chỉnh).

## Bảng (cùng dạng `sim_balance.py`)

```
mô phỏng  naive   win=0.00 progress=0.63 pass(d4,d9,d15,d22)=[0.96, 0.79, 0.04, 0.0]  combo3+=0.10
mô phỏng  nocombo win=0.46 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.99, 0.46]  combo3+=0.00
mô phỏng  smart   win=0.89 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.89]   combo3+=0.97
bot C#    naive   win=0.01 progress=0.67 pass(d4,d9,d15,d22)=[0.96, 0.84, 0.11, 0.01] combo3+=0.09
bot C#    nocombo win=0.12 progress=0.99 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.95, 0.12]  combo3+=0.01
bot C#    smart   win=0.60 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.99, 0.6]   combo3+=0.96
```

| Kiểu | Mốc | Bot | Mô phỏng | Chênh (điểm) |
| --- | --- | --- | --- | --- |
| naive | qua d4 / d9 / d15 / d22 | 96,5 / 84,0 / 11,2 / 0,9 | 96 / 79 / 4 / 0 | +0,5 / +5,0 / **+7,2** / +0,9 |
| nocombo | qua d4 / d9 / d15 / d22 | 100 / 100 / 95,5 / 12,2 | 100 / 100 / 99 / 46 | 0 / 0 / −3,5 / **−33,9** |
| smart | qua d4 / d9 / d15 / d22 | 100 / 100 / 98,8 / 59,6 | 100 / 100 / 100 / 89 | 0 / 0 / −1,3 / **−29,4** |

Tiền đầu ngày 22 (trước khi trả 32.000G), trung vị: smart bot **32.821G** / mô phỏng 34.900G; nocombo 29.467G / 31.749G.
Ngưỡng 32.000G nằm ngay sát trung vị của bot nên hụt ~2.000G ở vòng 4 làm tỉ lệ thắng rơi 30 điểm.

## Nguyên nhân các chênh > 5 điểm

**1. Người chơi tốt hụt mốc cuối (−29 / −34 điểm) — do luật game, không phải lỗi bot:**

- **Khách không bao giờ bỏ về (Q-39).** Ba khách gọi món từ cùng một kho; món của một khách có thể hết, ghế bị giữ. Bot phải đưa đĩa khác để trống ghế: trung bình **8,7 lần / ván**, mất 232G tiền đền + chính đĩa đó (~1.400G giá trị). `sim_balance.py` cho khách gọi lại món còn làm được nên không mất khoản này.
- **Chờ lâu hơn giả định:** bot tốt mất **5,9 điểm** dễ tính lúc được phục vụ (mô phỏng giả định 3; vòng 3 an toàn chỉ 70 giây). Bot không đọc chỉ số mất 10,9 (giả định 8).
- **Chủ nợ cần ghế trống (mới phát hiện):** lúc 20:00 ba ghế thường đã kín khách gọi món hết. Khi bot tiêu hết vé từ sáng thì không còn gì đưa để trống ghế → 23% số ván không phục vụ được chủ nợ, mất 11.500G. Bot tốt giờ giữ lại 1 vé tới 20:00 → 99–100% được thưởng. Người chơi thật cũng sẽ gặp bẫy này (Q-40). Mô phỏng coi chủ nợ là khách cuối, luôn được phục vụ.

**2. Người không đọc chỉ số qua d15 (+7 điểm) — do giả định mô phỏng:** mô phỏng cắt số món mỗi ngày theo ngân sách thời gian (6,7 món / ngày); trên luật thật bot phục vụ 7,2 khách / ngày vì nấu theo món khách gọi và kho còn thừa. Thêm tiền → qua d15 nhiều hơn. Vẫn dưới ngưỡng thắng (0,9%).

## Gợi ý cho sprint 7 (chưa làm)

- Quyết Q-39 (khách đổi món / bỏ về) và Q-40 (chủ nợ luôn có chỗ): hai luật này chiếm phần lớn khoảng hụt.
- Sửa giả định thời gian chờ trong `sim_balance.py` (3 → ~6 điểm với người chơi tốt) rồi mới chỉnh nợ.
- Dùng `balance/run_bots.ps1` làm phép kiểm Điều 1 thay cho mô phỏng Python khi hai bên lệch.

## Trạng thái bot dùng cho lệnh gỡ lỗi (trung vị bot gom bộ)

Đầu ngày 9: 1.885G, relic V3 V4 C2 S1 F4 F3 F2 F1 · ngày 15: 3.685G, S4 S1 F10 F9 F2 S3 S2 C3 · ngày 21: 18.322G, S3 F2 F4 S2 S1 F10 S4 F1 (+ 1 đĩa Tuyệt hảo trong tủ lạnh, 97% ván có).
Trong Editor: Play → menu **Quán Ăn Hope > Gỡ lỗi: cho relic** → nút "Ngày 9 / 15 / 21".
