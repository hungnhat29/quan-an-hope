# Spec — Sprint 7: Cân bằng và cảm giác (F-060 … F-063) + ba sửa trước sprint

Ngày: 21/11/2026 · Nguồn duyệt: `docs/sprint-7/impl-plan.md` và yêu cầu của bạn 21/11/2026 · Luật: `specs/constitution.md` **v1.7**.
Số mới (Điều 8: vào dữ liệu trước, qua `balance/make_next.py`): `start.first_day_tickets = 5`, `tickets_per_day = 7`, nợ ngày 22 `36000`, `bot_timing` (giả định thời gian của bot, đo từ `playtest-log.csv`).

## Ba sửa trước sprint (yêu cầu 21/11/2026)

| Yêu cầu | Luật | Làm ở đâu | Test |
| --- | --- | --- | --- |
| "chỉnh sửa lại animation ăn của khách, vì tay của khách không di chuyển" | Art Bible §6, §12: tay ống cao su, không khuỷu, cong theo đường cung | `Game/EatingFx.cs`: lúc ăn tay vẫy của khối khách (`ArmWave`, `HandWave`) ẩn đi, thay bằng ống cao su Bezier dựng lại mỗi khung (vai lấy từ vòng đầu của ống gốc, độ dài bằng ống gốc, cùng vật liệu + viền mực); bàn tay bám điểm cầm của đũa; gần miệng ống cong, xa đĩa thì giãn thẳng. Ăn xong hiện lại tay vẫy | PlayMode `KhachAn_DuaGapMonVoiDan_AnXongRoiGhe` (bàn tay luôn ở điểm cầm < 1 cm, tay đi > 20 cm theo nhịp gắp, ống cong > 3 cm, tay gốc ẩn / hiện lại; ảnh `docs/sprint-6/ui/khach_an_1..2.png`) |
| "không hiển thị relic nào là relic ở trong combo, relic nào là relic thường lên màn hình" | constitution 1.7 §7.1 | `RelicCard.Show`: bỏ dòng "Bộ … · đang có n/4" / "Relic lẻ"; `RoomView.ShelfSummary`: chỉ "Giá relic n/8". Tác dụng bộ vẫn tính (bảng tỉ lệ Góc 2 vẫn ghi "đã tính tác dụng relic") | PlayMode `F020_*` (giá không ghi tên bộ, thẻ không có "Bộ" / "lẻ") |
| "ngày 1 chỉ có 5 vé" | constitution 1.7 §3 | `start.first_day_tickets = 5` (bỏ `bonus_tickets`); `TicketWallet.BeginDay` | EditMode `Truoc7_Ngay1_Co5Ve`, `Truoc7_Ngay2_CongVeMoiNgay_VeConLaiCongDon`, `TicketWalletTests.*`; PlayMode `Truoc7_Ngay1_5Ve_DongHoDungToiKhiMoQuan` |
| "khi nào bắt đầu mở quán thì đồng hồ mới bắt đầu chạy từ 9h" | constitution 1.7 §6.2 (đóng Q-21) | `GameSession.Tick` bỏ qua pha Prep; HUD ghi "chưa mở quán"; chủ nợ 20:00 tính từ lúc mở; bot và mô phỏng: việc trước khi mở không tốn giờ | EditMode `Truoc7_DongHo_*` (3), `Truoc7_SoCheTruocKhiMo_KhongMatGio`, `Truoc7_ChuNo_DenLuc20h_TinhTuLucMoQuan`, `DayClockTests.*` (sửa theo 1.7); PlayMode `Truoc7_Ngay1_5Ve_DongHoDungToiKhiMoQuan` (ảnh `dong_ho_sau_khi_mo.png`) |

## F-060 Đo và chỉnh cân bằng

| AC | Làm | Test / bằng chứng |
| --- | --- | --- |
| 1 | `PlaytestLog` thêm cột `phien`, test PlayMode ghi vào thư mục tạm (`GameController.PlaytestFolder`); file cũ đổi tên `playtest-log-sprint2-6.csv`. `balance/playtest_overhead.py` tách buổi, loại buổi do test ghi, in theo vòng. Số đo vào `balance.json bot_timing`, mô phỏng và bot đọc chung | EditMode `F060_AC2_BotDungGioDo_TuDuLieu`; PlayMode F-014 (11 cột, thư mục tạm). **CẦN NGƯỜI KIỂM:** chỉ có ≤ 6 buổi vòng 1 (47 món) có thể là người thật; cần 3 buổi bạn chơi qua vòng 2 |
| 2 | Bot 3 kiểu × 4.000 ván trên luật thật; cột "đo trên game" ở constitution Điều 1 | `balance/run_bots.ps1` → `balance/bot_report.md` |
| 3 | Chỉnh trong dữ liệu theo thứ tự nợ → giá → vé → độ dài ngày; quét phương án `balance/run_sweep.ps1` → `balance/bot_sweep.md`; mỗi lần chỉnh một dòng ở constitution §9 | EditMode `F060_PhuongAn_ChiDoiSo_KhongDoiDuLieuGoc` |
| 4 | Không sửa code để chỉnh; test thống kê vẫn xanh | Toàn bộ EditMode (Điều 3: `TierTableStatTests`) |
| 5 | Từng bộ đủ 3 món: 70–97% | `bot_sweep.md` cột "Từng bộ" |
| 6 | Đếm khách bị giảm dễ tính mỗi ngày vòng 2–4 (`BotStats.ImpatientPerDay`) | EditMode `F060_AC6_DemKhachBiGiamDeTinh_VongCoTinhGio`; mục tiêu chờ bạn chọn (Q-53) |

## F-061 Âm thanh

| AC | Làm | Test |
| --- | --- | --- |
| 1 | 24 tiếng tự tạo (`tools/am-thanh/make_audio.py`) gắn vào thao tác ở `GameControllerAudio.cs`: nhét vé, thùng rơi, cầm, đặt, thái (lặp mỗi 0,22 giây khi giữ), cho vào bếp, xèo (chảo, vỉ) / sôi (nồi) lặp khi nấu, chín, múc, chuông, tiền, khách vui / giận, mua relic, thu nợ; thêm bước chân, nước sốt, cháy | PlayMode `F061_AmThanh_ThaoTacChinh_Am3D_TheoCaiDat` |
| 2 | Bậc 4–5 lộ ra có tiếng riêng (`bac_4`, `bac_5`), phát lúc món lộ (cả thùng vé đen); mất điện giấu bậc thì im | PlayMode `F061_Bac4_5_CoTiengRieng` |
| 3 | Nhạc phòng; nhạc riêng ngày mất điện, ngày chủ nợ; đổi nhạc mờ chéo 1,2 giây | PlayMode `F061_NhacRieng_NgayMatDien_VaNgayChuNo` |
| 4 | Âm lượng theo Cài đặt (tổng = AudioListener, hiệu ứng, nhạc); tiếng trong phòng 3D, tắt tuyến tính 1,2 → 16 m | PlayMode `F061_AmThanh_*` |
| 5 | `docs/CREDITS.md` | xem file |

## F-062 Hiệu ứng và biểu cảm

| AC | Mục Art Bible → làm ở đâu | Test |
| --- | --- | --- |
| 1 | §11 thùng rơi qua cổng: `PortalFx` (xoắn ốc 24 hạt tím / vàng, mở easeOutBack, đóng khi thùng chạm bàn) + `Sparkle` lúc chạm; lộ bậc 4–5 1,5 giây (đã có) + tiếng; món ra lò `Pop` 0,6 giây (đã có); khách vui: `RiseFx` trái tim + `CoinFly` đồng xu bay về ô Ví 0,8 giây; cháy / Lạc vị: khói gấp khúc + rung 0,15 giây; nước sốt: vòng xoáy vàng (`Fx_xoay_vang`) / khói xám | PlayMode `F062_ThungRoiQuaCongXoanOc_LapLanh` (ảnh `cong_xoan_oc.png`), `F062_KhachVui_TimVaDongXuBayVeVi_DuBieuCam` (ảnh `khach_vui_tim_xu.png`) |
| 2 | §6 bảng biểu cảm: `FaceArt.Customer` vẽ 7 mặt theo nét 3 ảnh gốc (vui, chờ, chờ đổ mồ hôi, giận, ngạc nhiên, buồn, đang ăn); `FaceArt.CustomerMood` chọn theo kết quả + món Tuyệt hảo + giờ chờ; chủ nợ giữ mặt lạnh lùng / ngạc nhiên | PlayMode `F062_KhachVui_*` (ảnh `khach_dang_an.png`) |
| 3 | §12 chớp mắt 0,12 giây mỗi 3–5 giây: khách (`SeatView.Blinks`, ảnh mắt nhắm) và nguyên liệu (`FaceBillboard`) | PlayMode `F062_KhachVui_*`, `F062_NguyenLieuChopMat_3Den5Giay` |
| 4 | §4 màu phủ theo giờ (`ScreenTint`, dưới HUD) và mưa ngày 2 (`RainFx` 140 vệt ngoài cửa + phố nhuộm `#9CC7D8` 25%) | PlayMode `F062_MauPhuTheoGio_VaMuaNgay2` (ảnh `mua_ngay_2.png`) |

## F-063 Học Blender (việc của bạn)

| Mình chuẩn bị | File | Test |
| --- | --- | --- |
| Hướng dẫn từng bước 7 ngày (chưa có ảnh chụp Blender, xem ghi chú trong file) | `docs/blender/01-ca-chua.md` | CẦN NGƯỜI KIỂM |
| Thông số xuất `.fbx` (Art Bible §14) | trong file trên, ngày 5 | — |
| Cảnh Unity thử model + lệnh gắn ToonFlat / Outline | `Editor/ModelTestTools.cs` (menu "Quán Ăn Hope/Cảnh thử model Blender", "Gắn ToonFlat + Outline cho model đang chọn") | EditMode `Sprint7EditorTests` (2) |

## Ngoài phạm vi

Model Blender vào game (sprint 8–9). Nội dung mới. Số mục tiêu "khách chờ quá giờ" (chờ bạn chọn). Một ván 22 ngày dài bao lâu (Q-22).
