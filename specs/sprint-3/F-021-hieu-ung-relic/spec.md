# F-021 — Hiệu ứng 22 relic và thưởng bộ

Trạng thái: DONE (16/11/2026), chờ nghiệm thu · Nguồn: constitution 1.4 §7.1–7.3, §4.2, §5.3, §6.5, Điều 3, Điều 4 · dữ liệu `balance/relics.json`, `balance/balance.json` → `set_bonus` · cách gộp và thứ tự áp: `balance/sim_balance.py` (`P.fx`, `pull`, `cook`, `pay`, vé ngày) · Duyệt: impl plan sprint 3

## Luật (khoá hiệu ứng trong dữ liệu → nơi áp)

Hiệu ứng gộp = cộng mọi `fx` của relic trên giá + thưởng bộ của mọi ngưỡng đã đạt (2, 3, 4 món cùng bộ **cộng dồn**, như `sim_balance.py`); riêng `phu_min`, `chinh_min`, `bt_t5` của thưởng bộ lấy giá trị lớn nhất. *(Q-28: constitution §7.2 không nói rõ cộng dồn; theo mô phỏng)*

| Khoá | Relic / thưởng | Áp ở |
| --- | --- | --- |
| `phu_up`, `chinh_up` | Bình tưới, Dao phay, thưởng 2/3 | mỗi món trong thùng: x% +1 bậc, tối đa 5 (§7.1) |
| `phu_reroll0`, `chinh_reroll0` | Phân bón, Thùng đá | bậc 0 quay lại 1 lần |
| `phu_jump4`, `chinh_jump4` | Hạt giống vàng, Cân vàng | x% nhảy thẳng lên bậc 4 (không hạ bậc 5) |
| `phu_min`, `chinh_min` | thưởng 3 | bậc tối thiểu |
| `phu_plus`, `chinh_plus` | thưởng 4 | +1 bậc, tối đa 5 |
| `phu_size`, `chinh_size` | Rổ tre +1, Thuyền thuê −1, Mối quen +1 | cỡ thùng, tối thiểu 1 món |
| `bt_*` | bộ Sơn Hào | xem F-022 |
| `dish_center` | Muối hồng +0,15, Ớt ma +0,6 | cộng vào điểm b (§5.3) — thanh tỉ lệ bếp hiện đúng |
| `dish_ruin` | Ớt ma 10% | món ra Lạc vị |
| `dish_floor` | Chảo gang | món không dưới Dở (cả khi sai công thức, Q-29) |
| `tip` | Chuông vàng | tip × (1 + 0,15) |
| `income` | Tượng mèo | tiền món + tip × (1 + 0,08) khi khách hài lòng (như mô phỏng) |
| `ticket_every3` | Vé số cào | +1 vé đầu ngày chia hết cho 3 |
| `fridge`, `no_decay`, `patience`, `sauce_safe` | Tủ lạnh 2 cửa, Đồng hồ cát, Máy pha cà phê, Hũ sốt cũ | chưa có hệ thống: mua và giữ được, thẻ ghi "có tác dụng từ ngày 4" (impl plan, sprint 4) |

Thứ tự áp cho một món nguyên liệu (như `pull`): quay bậc → quay lại nếu 0 → nâng bậc → nhảy bậc 4 → tối thiểu → cộng bậc. Món: điểm b (+center) → quay → Lạc vị (ruin) → sàn Dở (floor).

## AC

| AC | Then |
| --- | --- |
| 1 | Mỗi relic có ít nhất một test: có relic thì luật đổi đúng như bảng |
| 2 | "x% nâng bậc" áp cho từng món trong thùng, tối đa bậc 5; test thống kê ±1 điểm |
| 3 | Thưởng bộ bật đúng ngưỡng 2 / 3 / 4 món, đúng số trong `set_bonus` |
| 4 | Giá relic 8 ô ghi tiến độ bộ ("Chợ Sớm 2/4") ngay trên giá |
| 5 | Bảng tỉ lệ ở Góc 2 hiện thêm tác dụng relic đang có lên tỉ lệ |

## Ma trận AC → test (16/11/2026)

| AC | Test | Kết quả |
| --- | --- | --- |
| 1 | `RelicEffectTests`: V1, V2+S3, V3, V4, C1, C2, C3, C4 (gacha), S1–S4 (F-022), F2+F6 (tiền), F4, F8, F5 (bếp), F10 (vé, cả trong ván), `TrongVanChoi_RelicApVaoGacha_BepVaTien`; F1/F3/F7/F9: `RelicShopTests.F021_RelicChuaCoHeThong_*` (nhãn "có tác dụng từ ngày 4") | xanh |
| 2 | Thống kê 100.000 món, ±1 điểm: V1, C1, V3, C2, V4, C4, thưởng 2 Chợ Sớm, Ớt ma 10%; `AC2_NangBac_ToiDaBac5`, `ThuongBo4_*` | xanh |
| 3 | `AC3_VuonNha_*`, `AC3_ChoSom_*`, `AC3_SonHao_*` (ngưỡng 2/3/4, số từ `set_bonus`) | xanh |
| 4 | `RoomView.ShelfSummary`; giả lập kiểm "Giá relic 1/8", "Chợ Sớm n/4" trên giá (`goc3_gia_8_o.png`) | xanh |
| 5 | `AC5_TiLeHienTrenBang_KhopTinhTay_VaKhopQuayThat` (bảng tính = quay thật ±1); giả lập: bảng Góc 2 ghi "Có relic", ô "Phụ x% · Chính y%" (`goc2_bang_ti_le_co_relic.png`) | xanh |

## Ngoài phạm vi

Tác dụng của 4 relic gắn với hệ thống chưa có (sprint 4).
