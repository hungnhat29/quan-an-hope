# F-014 — Đo thời gian thật

Trạng thái: AC 1 DONE; AC 2 chờ buổi chơi thử của bạn · Nguồn: constitution 1.4 §9 (giả định OVERHEAD 20 / 30 giây cần đo)

## Thiết kế

- `Game/PlaytestLog.cs`: mỗi món phục vụ ghi một dòng vào `playtest-log.csv` trong `Application.persistentDataPath` (Windows: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\My project\` cho bản build hiện tại).
- Cột: `ngay, gio_game, mon, bac, t_cam_dau, t_vat_num, t_chin, t_bam_chuong, che_bien_s, thao_tac_di_lai_s`. `che_bien_s` = sơ chế + cho vào + nấu + múc theo dữ liệu (giống `sim_balance.dish_seconds`); `thao_tac_di_lai_s` = tổng − `che_bien_s` (đúng đại lượng OVERHEAD của mô phỏng).
- Đổ bỏ đồ trên bếp thì món đang làm không tính.
- `balance/playtest_overhead.py <csv>`: trung bình, trung vị, so với 20 / 30 giây, ngưỡng 5 giây, in lệnh chạy lại mô phỏng.

## Ma trận AC → test

| AC | Test | Kết quả 15/11 |
| --- | --- | --- |
| 1 | Giả lập `ChoiTronVong1_Bang3D_QuaVong1`: ≥ 15 dòng, đúng tiêu đề, đủ 10 cột (22 dòng) | xanh |
| 2 | Script chạy được trên log của người chơi giả (trung vị 14,3 giây). Số này là của người chơi giả, **không** dùng để sửa mô phỏng | **CẦN NGƯỜI KIỂM** (buổi chơi thật) |

Cách làm AC 2: xoá `playtest-log.csv` cũ, chơi bản build, rồi chạy `PYTHONUTF8=1 python balance/playtest_overhead.py "<đường dẫn>"`.
