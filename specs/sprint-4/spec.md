# Spec — Sprint 4: Vòng 2 (F-030 … F-036)

Ngày: 16/11/2026 · Nguồn duyệt: `docs/sprint-4/impl-plan.md` (người dùng yêu cầu triển khai 16/11/2026) · Luật: `specs/constitution.md` v1.4.
Spec này không đặt con số mới (Điều 8). Số đã có trong constitution được chép vào dữ liệu (Điều 2) qua `balance/make_next.py`:
`seats` (§6.1), `patience` (§6.6), `blackout` (§2, §5.9, Art Bible §4), `item_shop_from_day` (§8).

## Mục tiêu (impl plan §1)

Chơi liền ngày 1 → đầu ngày 9: qua vòng 1 thì sang ngày 4; vòng 2 có 3 khách cùng ngồi, giờ chờ, tủ lạnh, quá lửa,
quầy vật phẩm; ngày 8 mất điện; đầu ngày 9 thu 2.900G rồi dừng ("Qua vòng 2").

## Tiêu chí chấp nhận → test

| AC | Nguồn | Test |
| --- | --- | --- |
| F-030 AC-1 tối đa 3 ghế, ghế trống 3 giây sau có khách, tới 21:00 | §6.1, §6.2 | `SeatsAndPatienceTests.F030_AC1_*` (4), PlayMode `F030_F031_*` |
| F-030 AC-2 mỗi khách một bong bóng: dễ tính, món gọi, thanh kiên nhẫn | §6.7, Điều 9 | `F030_AC2_*`, PlayMode `F030_F031_*` (ảnh `goc1_ba_khach.png`) |
| F-030 AC-3 nhìn vào khách, bấm: đúng khách nhận đĩa; đúng / sai món như §6.5 | §6.5, §6.7 | `F030_AC3_*` (3), PlayMode (ảnh `goc1_phuc_vu_khach_giua.png`) |
| F-030 AC-4 21:00 không thêm khách, phục vụ hết thì hết ngày | §6.2 | `F030_AC4_*` |
| F-031 AC-1 an toàn 90 giây, −1 điểm / 3 giây, tối đa −20 | §6.6 | `F031_AC1_*` (2), PlayMode (ảnh `goc1_khach_cho_lau.png`) |
| F-031 AC-2 vòng 1 không tính giờ; ngày 8 an toàn 110 giây | §6.6 | `F031_AC2_*` |
| F-031 AC-3 tiền và bậc tối thiểu theo độ dễ tính lúc phục vụ | §6.5, §6.6 | `F031_AC3_*`, PlayMode |
| F-031 AC-4 Đồng hồ cát +40%, Máy pha cà phê +60% | §7.3 | `F031_AC4_*` (2) |
| F-032 AC-1 tủ 2 ô ở Góc 4; cầm đĩa bấm tủ để cất, bấm ô để lấy | §5.7 | `FridgeAndOvercookTests.F032_AC1_*` (2), PlayMode `F032_F033_*` (ảnh `goc4_tu_lanh.png`) |
| F-032 AC-2 cuối ngày đĩa trên quầy vào tủ nếu còn ô, không thì bỏ | §5.1 | `F032_AC2_*`, PlayMode |
| F-032 AC-3 quá 3 ngày tụt 1 bậc; Tủ lạnh 2 cửa +2 ô, không tụt | §5.7, §7.3 | `F032_AC3_*` (2) |
| F-032 AC-4 món trong tủ là "làm được" khi khách gọi | §6.7 | `F032_AC4_*` |
| F-033 AC-1 từ vòng 2, quá 1,5× thời gian nấu: khói, bảng bếp đổi màu | §5.9 | `F033_AC1_*`, PlayMode (ảnh `goc4_qua_lua_canh_bao.png`) |
| F-033 AC-2 quá 2× thì −1 bậc; vòng 1 không có | §5.9 | `F033_AC2_*` (3), PlayMode |
| F-034 AC-1 giá / giới hạn giữ 450+45 (20), 800 (1), 700+120 (5); mở từ ngày 4 | §8 | `ItemShopTests.AC1_*` (5), PlayMode `F034_*` (ảnh `goc2_quay_vat_pham.png`) |
| F-034 AC-2 nước sốt đúng bảng ngày 4–8, hiện tỉ lệ; test thống kê | §5.5, Điều 3, 4 | `AC2_*` (3; 100.000 lần ±1 điểm) |
| F-034 AC-3 máy gacha: mỗi món 1 lần, 3 lần / ngày, tỉ lệ §5.6, không hiện | §5.6, Điều 3, 4 | `AC3_*` (3; 100.000 lần), PlayMode (biển máy không có "%") |
| F-034 AC-4 vé đen nhét vào máy: thùng theo 0/0/0/62/30/8 | §4.3 | `AC4_*` (2; 100.000 món), PlayMode (ảnh `goc2_thung_ve_den.png`) |
| F-035 AC-1 phòng tối 60%, ánh nến quanh bếp và quầy | §2, Art Bible §4 | PlayMode `F035_*` (ảnh `ngay8_*.png`) |
| F-035 AC-2 giấu khoảng bậc món, màu bậc nguyên liệu; điểm −0,3; nấu +25% | §5.3, §5.9, Điều 4 | `Round2Tests.F035_*` (3), PlayMode |
| F-036 bảng vòng 2, 4 nguyên liệu mới (hình chi tiết), Mì Ý / Bún, 16 món mới, giá gốc 70G | §4.3, §4.5, §5.2, §5.8 | `Round2Tests.F036_*` (2), PlayMode `F012_*` |
| Mục tiêu 1, 7: qua vòng 1 sang ngày 4; đầu ngày 9 thu 2.900G | §2 | `Round2Tests.MucTieu_*` (3), PlayMode `ChoiTronVong1_*`, `F035_*` (ảnh `qua_vong_2.png`) |
| Kiểm (impl plan §4): bot tới ngày 9 so với mô phỏng 79% | §9 | `GameSessionTests.Sprint4_BotKhongDocChiSo_*` |

## Mặc định đã chọn (constitution / impl plan chưa nói — ghi ở `.dev-memory/QUESTIONS.md`)

1. **Phục vụ = nhìn vào khách rồi bấm** (impl plan §6 câu 1, AC-3 cho phép cả hai). Chuông vẫn còn: phục vụ khách đến sớm nhất. (Q-32)
2. **Quầy vật phẩm**: kệ nhỏ giữa máy nhét vé và máy gacha đồ ăn ở Góc 2. Đề xuất cũ (góc phải Góc 2) sát quyển sách Góc 3 nên tia nhìn trúng trang sách. (Q-33)
3. **Tủ lạnh**: tủ treo tường phía trên giá dụng cụ, bên phải quầy chờ Góc 4 (mặt quầy phải đã kín chồng đĩa). (Q-34)
4. **Giới hạn 20 / 5 là số đang giữ** (constitution §8 "Giữ tối đa"); `sim_balance.py` đang tính là số lần mua. (Q-35)
5. **Tụt bậc trong tủ chỉ một lần** khi quá 3 ngày. (Q-36)
6. **Độ dễ tính lúc phục vụ không dưới 1%** (như `sim_balance.py pay()`: `max(1, E − impatience)`). (Q-37)
7. **Nước sốt +1 / +2 tối đa Tuyệt hảo**, Huyền thoại chỉ qua ô "Thành món Huyền thoại" (như `sim_balance.py sauce_roll`, §5.4). Máy gacha cũng tối đa Tuyệt hảo.
8. **Mở quán vòng 2**: ghế đầu có khách ngay, hai ghế còn lại 3 giây sau (§6.1 "có thể 3 người liền nhau").
9. **Ngày mất điện giấu cả mặt nguyên liệu** (mặt cũng lộ bậc, Art Bible §7) và chữ bậc trong gợi ý; đĩa món vẫn hiện bậc.
10. **Máy gacha đồ ăn**: chưa mua thì bấm máy để mua; vé đen nhét vào khe đen trên chân máy nhét vé.

## Ngoài phạm vi (impl plan §1)

Vòng 3–4, Món Hy Vọng, mua relic bằng vé đen, chủ nợ (sprint 5); lưu game (sprint 6).
