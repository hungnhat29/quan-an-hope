# F-020 — Cửa hàng relic (Góc 3)

Trạng thái: DONE (16/11/2026), chờ nghiệm thu · Nguồn: constitution 1.4 §3, §7.1, Điều 4, Điều 9 · Duyệt: AC + kịch bản 10 giây trong `docs/sprint-3/impl-plan.md`; người dùng yêu cầu triển khai 16/11/2026

## Luật

1. Cửa hàng mở từ ngày `start.relic_shop_from_day` (= 2). Ngày 1: ba bệ trống, biển "mở từ ngày 2". *(§3, §7.1)*
2. Đầu mỗi ngày mở cửa hàng: rút **3 relic khác nhau**, đều nhau, trong các relic **chưa có trên giá**. Relic đã bỏ quay lại rổ rút như relic chưa có (mặc định impl plan §6). Số 3 nằm ở `balance.json` → `relic_shop_offers` (§7.1, Điều 2). Cách rút giống `sim_balance.py` `offers()`.
3. Mua: đủ vé → trừ đúng giá (vé trắng), relic lên giá, bệ đó trống tới hết ngày. Thiếu vé → không mua, gợi ý ghi "thiếu n vé". *(§7.1)*
4. Giá tối đa `relic_cap` (= 8). Đầy mà mua tiếp → phải bấm một relic trên giá để bỏ; huỷ (bấm lại bệ / chuột phải) thì không mua, không mất vé. *(§7.1; mặc định impl plan §6: bấm relic trên giá)*
5. Thẻ dựng trước mỗi bệ: tên · giá · độ hiếm · hiệu ứng · bộ. Viền thẻ theo độ hiếm. *(Điều 4, Điều 9)*
6. Lệnh gỡ lỗi "cho tôi relic X": chỉ trong Editor (impl plan §5).

## Thiết kế

- Core `Relics.cs`: `RelicDef` (id, tên, bộ, giá, độ hiếm, `RelicEffect[]`), `RelicInventory` (giá 8 ô, tính hiệu ứng gộp), `RelicShop` (3 bệ của ngày), `GameSession.BuyRelic(bệ, bỏ)`.
- Dữ liệu: `relics.json` → `Balance.relics`; `relic_cap`, `relic_shop_offers`, `set_bonus`, `breakthrough` → `Balance`.
- Game: `RoomView.BuildRelicShop` — ba bệ (vật 3D ghép khối trên bệ có sẵn), thẻ dựng, giá 8 ô (relic nhỏ + nhãn tiến độ bộ), lấp lánh khi mua (`KitchenFx.Sparkle`).
- Màu viền theo độ hiếm (Art Bible không có bảng màu độ hiếm relic; dùng thang màu bậc §4): Thường bạc hà #8CC9AE, Hiếm tím #C9B3F0, Huyền thoại vàng #F2C14E. *(Q-30)*

## AC

| AC | Given | When | Then |
| --- | --- | --- | --- |
| 1 | Ngày 1 | Tới Góc 3 | Ba bệ trống, biển "mở từ ngày 2" |
| 2 | Đầu ngày ≥ 2 | — | 3 relic khác nhau, không trùng relic đang có |
| 3 | Đủ vé | Nhìn relic, bấm | Trừ đúng số vé, relic lên giá, bệ đó trống tới hết ngày |
| 4 | Thiếu vé | Bấm | Không mua được, gợi ý ghi thiếu mấy vé |
| 5 | Đang có 8 relic | Mua cái thứ 9 | Phải bấm chọn một relic trên giá để bỏ; huỷ thì không mua |
| 6 | Mỗi relic | Nhìn thẻ | Thẻ ghi tên, giá, độ hiếm, hiệu ứng |

## Ma trận AC → test (16/11/2026)

| AC | Test | Kết quả |
| --- | --- | --- |
| 1 | `RelicShopTests.AC1_Ngay1_BaBeTrong_KhongMuaDuoc`; giả lập `F020_Goc3_*` (gợi ý + biển "mở từ ngày 2", ảnh `docs/sprint-3/ui/goc3_ngay1_be_trong.png`) | xanh |
| 2 | `RelicShopTests.AC2_DauNgay2_3RelicKhacNhau_KhongTrungRelicDangCo` (300 ván), `AC2_MoiRelicChuaCo_DeuCoTheRa_RelicDaBoQuayLai` | xanh |
| 3 | `RelicShopTests.AC3_*`; giả lập: nhìn bệ, gợi ý "Mua · n vé (còn m)", bấm → vé trừ đúng, relic bay lên ô đầu giá, bệ trống (`goc3_vua_mua.png`) | xanh |
| 4 | `RelicShopTests.AC4_ThieuVe_KhongMua_BaoThieuMayVe`; giả lập: gợi ý "thiếu n vé" | xanh |
| 5 | `RelicShopTests.AC5_Day8_PhaiChonRelicDeBo_HuyThiKhongMua`; giả lập: bấm bệ → chờ chọn, chuột phải huỷ, bấm lại → bấm relic trên giá để bỏ (`goc3_day_chon_bo.png`, `goc3_gia_8_o.png`) | xanh |
| 6 | `RelicShopTests.AC6_MoiRelic_TheCoTenGiaDoHiemHieuUng` (22 relic); giả lập kiểm chữ 3 thẻ + màu viền (`goc3_ngay2_ba_be.png`) | xanh |
| luật 6 | Menu Editor "Quán Ăn Hope > Gỡ lỗi: cho relic" (`RelicDebugWindow`), `GameSession.DebugGiveRelic` | CẦN NGƯỜI KIỂM (bấm Play, mở menu) |

## Ngoài phạm vi

Mua bằng vé đen (sprint 5); model relic đẹp (sprint 8).
