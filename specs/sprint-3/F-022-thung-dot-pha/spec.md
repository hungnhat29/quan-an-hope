# F-022 — Thùng đột phá

Trạng thái: DONE (16/11/2026), chờ nghiệm thu · Nguồn: constitution 1.4 §4.1, §4.2, §4.4, §5.8, §6.7, §7.2 (Sơn Hào), Điều 3 · dữ liệu `balance.json` → `breakthrough`, `relics.json` S1–S4 · Art Bible §7 (rương nắp vòm tím #C9B3F0 viền vàng) · Duyệt: impl plan sprint 3

## Luật

1. Mỗi lượt gacha: tỉ lệ ra thùng đột phá = `breakthrough.base_chance` (0) + tổng `bt_chance`. Không có relic → không bao giờ ra. *(§4.4)*
2. Loại đột phá chọn đều trong 4 loại `group = dotpha`. Mỗi thùng `breakthrough.items` (1) món, + `bt_items` (thưởng Sơn Hào 3 → 2 món). *(§4.4)*
3. Bậc mỗi món: bậc 5 với xác suất `tier5_share` (0,3) + `bt_t5` (Lưới thép +0,2; thưởng 4 = 1,0 → luôn bậc 5), còn lại bậc 4. *(§4.4, §7.2)*
4. Món đột phá dùng thay nguyên liệu chính; ghép với đồ phụ ra món đột phá theo `recipes.json` (40 món, §5.8). Khách gọi được món đột phá khi kho có nguyên liệu đó (§6.7: mọi cặp đang có trong kho).
5. Hình: rương nắp vòm tím viền vàng (Art Bible §7); 4 nguyên liệu đột phá có hình ghép khối.


## AC

| AC | Then |
| --- | --- |
| 1 | Không có relic Sơn Hào: 100.000 lượt gacha không ra thùng đột phá nào |
| 2 | Có relic: tỉ lệ ra thùng = tổng `bt_chance`; mỗi thùng 1 món (2 món khi có thưởng bộ 3), 70% bậc 4 / 30% bậc 5 (±1 điểm) |
| 3 | Thùng đột phá là rương nắp vòm màu tím viền vàng, món bên trong dùng thay nguyên liệu chính, nấu ra món đột phá đúng công thức §5.8 |
| 4 | Khách có thể gọi món đột phá khi kho đang có nguyên liệu đó |

## Ma trận AC → test (16/11/2026)

| AC | Test | Kết quả |
| --- | --- | --- |
| 1 | `RelicEffectTests.F022_AC1_KhongCoRelic_100000Luot_KhongRaThungDotPha` | xanh |
| 2 | `F022_AC2_BanDo_TiLeThung10PhanTram_*` (10% ±1, 1 món, 70/30), `F022_AC2_BacMonDotPha_100000Mon_70_30` (thưởng bộ 3: 2 món), `S2_LuoiThep_Bac5Len50PhanTram_ThongKe`, `S3_S4_*` (thưởng bộ 4: mọi món bậc 5) | xanh |
| 3 | Giả lập `F022_RuongDotPha_NapVomTim_NauRaMonDotPha`: rương tím viền vàng nắp vòm (`goc2_ruong_dot_pha.png`), nấu tay đúng công thức ra món đột phá (`goc4_mon_dot_pha.png`) | xanh |
| 4 | `F022_AC3_AC4_MonDotPha_KhachGoiDuoc_NauRaMonDotPha` (món đột phá nằm trong `MakeableRecipes`) | xanh |

