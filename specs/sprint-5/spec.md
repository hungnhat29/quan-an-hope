# Spec — Sprint 5: Vòng 3–4 và chủ nợ → Alpha (F-040 … F-045)

Ngày: 16/11/2026 · Nguồn duyệt: `docs/sprint-5/impl-plan.md` (người dùng yêu cầu triển khai 16/11/2026) · Luật: `specs/constitution.md` v1.4.
Spec này không đặt con số mới (Điều 8). Số duy nhất thêm vào dữ liệu: `day.boss_day = 21` (constitution §2 "Ngày 21, đúng 20:00 chủ nợ đến"), qua `balance/make_next.py`.
Điều kiện vào sprint "Q-20 đã có câu trả lời" **chưa đạt**: làm theo mặc định đã ghi ở constitution §6.7 (chủ nợ gọi "món ngon nhất của quán", nhận mọi món).

## Mục tiêu (impl plan §1)

Một ván đi từ ngày 1 tới màn thắng hoặc thua ở ngày 22: vòng 3 (ngày 9–14), vòng 4 (ngày 15–21), Món Hy Vọng từ ngày 9, relic bằng vé đen, chủ nợ ngày 21 lúc 20:00, thu nợ 10.500G (đầu ngày 15) và 32.000G (đầu ngày 22), bot C# 3 kiểu × 4.000 ván.

## Tiêu chí chấp nhận → test

| AC | Nguồn | Test |
| --- | --- | --- |
| F-040 AC-1 bảng bậc, khoảng dễ tính đúng vòng 3, 4; thống kê ±1 điểm | §4.3, §6.3 | `Sprint5Tests.F040_AC1_*` (4; gacha thật 100.000 món ngày 9 và 15), `TierTableStatTests`, `CustomerTests` |
| F-040 AC-2 nguyên liệu, công thức, đế món mở đúng vòng; sách 49 / 80 trang; giá gốc 85G / 100G | §4.5, §5.2, §5.8 | `F040_AC2_*` (2) |
| F-040 AC-3 8 nguyên liệu mới hình riêng (ghép khối chi tiết) + hình sơ chế | Art Bible §7 | PlayMode `F012_HinhNguyenLieu_AnhCan` (ảnh `docs/sprint-5/ui/nguyen_lieu/`), `F040_*` (ảnh `goc2_thung_vong4.png`) |
| F-040 AC-4 vỉ nướng: lật giữa chừng, vệt nướng; hấp: nồi có nắp, hơi | §5.1, §5.9 | `F040_AC4_GaNuong_*`, PlayMode `F040_Vong4_ViNuong_*` (ảnh `goc4_vi_nuong_lat.png`, `goc4_ga_nuong_dia.png`, `goc4_hap_nap_noi.png`) |
| F-041 AC-1 trước ngày 9: 100.000 lần nước sốt không ra Huyền thoại | §5.4, §5.5 | `F041_AC1_*` |
| F-041 AC-2 từ ngày 9: 12/16/48/20/4, hiện cho người chơi | §5.5, Điều 3, 4 | `F041_AC2_*`, `F041_DungNuocSotThat_*`, PlayMode `F041_*` (biển quầy "Huyền thoại 4%") |
| F-041 AC-3 Món Hy Vọng: viền vàng, hào quang, kính đen + vương miện, tim vàng; giá ×6,0 | §5.2, Art Bible §7 | `F041_AC3_*`, PlayMode `F041_*` (ảnh `mon_hy_vong.png`) |
| F-042 AC-1 1 vé đen → 3 relic Hiếm trở lên, ≥ 1 thuộc bộ nhiều nhất; chọn 1 | §7.1 | `F042_AC1_*` (4), PlayMode `F042_*` (ảnh `goc3_ve_den_3_relic.png`) |
| F-042 AC-2 chưa có relic bộ: 3 cái bất kỳ Hiếm trở lên | §7.1 | `F042_AC2_*` |
| F-043 AC-1 ngày 21, 20:00 chủ nợ vào, chiếm một ghế, 5%, ngồi qua 21:00 | §2, §6.2, §6.3 | `F043_AC1_*` (2), PlayMode `F043_F044_*` |
| F-043 AC-2 bong bóng "món ngon nhất của quán" (Q-20 mặc định) | §6.7 | `F043_AC2_*`, PlayMode (ảnh `ngay21_chu_no_toi.png`) |
| F-043 AC-3 Tuyệt hảo / Huyền thoại → tiền món + 11.500G; mặt Ngạc nhiên | §8, Art Bible §6 | `F043_AC3_*` (2), PlayMode (ảnh `ngay21_chu_no_ngac_nhien.png`) |
| F-043 AC-4 món thấp hơn: tính như khách khó tính, không thưởng | §6.4, §6.5 | `F043_AC4_*` (2) |
| F-043 AC-5 quán chưa mở 20:00: đợi ngoài cửa, vào ngay khi mở; đóng quán chưa phục vụ thì mất thưởng | impl plan | `F043_AC5_*` (2), PlayMode `F043_F044_*` |
| F-044 AC-1 đầu ngày 15 thu 10.500G, ngày 22 thu 32.000G; thiếu thì thua | §2 | `F044_AC1_*` (2), PlayMode `F044_Ngay22_ThieuTien_Thua` (ảnh `ngay22_thua.png`) |
| F-044 AC-2 qua ngày 22: màn thắng (chữ) | §2 | `F044_AC1_AC2_*`, PlayMode `F043_F044_*` (ảnh `ngay22_thang.png`) |
| F-045 AC-1 3 kiểu bot × 4.000 ván, một lệnh, < 10 phút | Điều 1, §9 | `Sprint5BotTests.F045_*` (5); lệnh `balance/run_bots.ps1` (43 giây) |
| F-045 AC-2 cùng dạng bảng `sim_balance.py`, chênh từng mốc, > 5 điểm có nguyên nhân | §9 | `F045_AC2_*`; `docs/sprint-5/bot-report.md` |
| Kịch bản 10 giây (ngày 21) | impl plan §2 | PlayMode `F043_F044_Ngay21_*` (trừ "chuông cửa kêu": dự án chưa có âm thanh) |

## Mặc định đã chọn (constitution / impl plan chưa nói — `.dev-memory/QUESTIONS.md`)

1. **Q-20** chủ nợ gọi "món ngon nhất của quán", nhận mọi món (mặc định constitution §6.7).
2. **Chủ nợ chờ ghế trống**: 3 ghế kín thì ông đợi, phục vụ xong một khách thì ông ngồi ngay ghế đó (không đợi 3 giây); ưu tiên ghế giữa. (Q-40)
3. **Chủ nợ không mất kiên nhẫn**: dễ tính cố định 5%, không có thanh kiên nhẫn. (§6.3 "cố định 5%", "ngồi tới khi được phục vụ")
4. **Món Hy Vọng giữ công thức gốc**: khách gọi đúng món đó vẫn nhận (tên hiện "Món Hy Vọng"). (Q-41)
5. **Vé đen → relic**: dùng vé là phải chọn, không huỷ, không hoàn vé; 3 lựa chọn giữ tới khi chọn. Hai bộ bằng nhau thì bộ nào trong số đó cũng tính là "nhiều nhất". (Q-42)
6. **Ngày 14 "all-in"**: chỉ là lời nhắn đầu ngày (tin xấu về quán, dốc tủ lạnh / nước sốt / vé đen), không đổi luật (constitution §6.2).
7. **Khe vé đen ở Góc 3**: cột nhỏ giữa quyển sách và bệ relic đầu tiên (đặt trên tường thì tia nhìn trúng bệ).

## Ngoài phạm vi (impl plan)

Tranh cốt truyện, menu, lưu game (sprint 6); chỉnh số cân bằng (sprint 7); âm thanh chuông cửa (chưa có hệ thống âm thanh).
