# Spec — Sau sprint 10: sửa theo chơi thử (đợt 23/11/2026)

Ngày: 23–24/11/2026 · Nguồn: tin nhắn của bạn 23/11/2026 (ghi tắt **[YC]**), 2 ảnh đính kèm (ảnh 1: 5 khách; ảnh 2: logo xe lăn có cánh), 3 file tiếng trong `tools/am-thanh/`, đoạn "5.3 / 5.4 / 5.5" bạn dán. Constitution 2.0 → **2.1**.

Phiên này không hỏi lại được giữa chừng: chỗ nguồn chưa rõ đánh dấu **[TỰ QUYẾT]**, ghi `.dev-memory/QUESTIONS.md` (Q-78..Q-84).

## Bảng Art Bible → làm ở đâu (L-013)

| Quy định | Làm ở |
| --- | --- |
| §4 màu nhân vật, không dùng 6 màu bậc cho áo | 5 khách đặc biệt: áo #33406E, #B9A3E3, #F7F3EC, #D9382C, #F2A65A (gần màu ảnh 1, lệch màu bậc; Q-82) |
| §5 hình bóng tròn; §6 đầu to, găng trắng | `build_characters.py khach_dac_biet()` (tóc / mũ / kính trên xương Head, đồ trên người trên Spine / Hand_L) |
| §11 phản hồi 0,6–1,2 giây; nguy hiểm = tam giác | Cầu vồng 1,8 s, bom xịt 1,7 s (`Sprint11Fx.cs`); bom là khối tròn, không gai |
| §13 chữ ≥ 14 px, nút ≥ 52 px | Cột Điều khiển trong Cài đặt (18 px), tranh to (34 px dòng bước) |
| §14 khách ≤ 6.000 tam giác | Đo theo khách đang hiện (thân + kiểu nặng nhất), Q-83 |
| [HÌNH] khung kem bo góc, ✓ tròn / ✗ bát giác, bàn tay ma | Tranh mới `BuyRelic`, `Sauce`, `SauceLuck`, `BossBest` (`GuidePics.cs`) |

## Lỗi

### C-01 Thả đồ lên thớt thì không nhặt lại được — [YC] "chuột phải khi thả đồ xuống khi mà thả xuống thớt thì không nhặt lên lại được"
Nguyên nhân: hộp bấm của thớt (1 × 0,3 × 0,6 m) bọc lên nguyên liệu vừa thả nên tia nhìn trúng thớt.
- AC-1: nhìn vào nguyên liệu đã thả (kể cả nằm trên thớt / mặt bàn) thì tâm ngắm trúng nguyên liệu; bấm là nhặt lại. *Test:* `C01_ThaLenThot_NhatLaiDuoc`.

### C-02 Nước sốt — [YC] "nhấn mua … chuột trái vào thì nó mua thêm 1 lần nữa … khi nhấn mua, nước sốt sẽ hiển thị ở bên phải bếp. Khi nấu xong thì nhấn chuột trái lên để cầm nước sốt rồi nhỏ giọt xuống món ăn mới nấu xong"
Kịch bản 10 giây: Tab → Cửa hàng → Mua nước sốt → đóng Tab: lọ đỏ nắp vàng hiện trên mặt bếp bên phải, nhãn "Nước sốt · 1". Nấu xong một đĩa ra quầy → bấm lọ: lọ lên tay, gợi ý "Nhỏ nước sốt lên Trứng chiên…" khi nhìn đĩa → bấm đĩa: hiệu ứng (C-07), thông báo "Nước sốt: … Bình thường → Ngon · còn 0 chai", lọ biến mất (hết sốt).
- AC-1: lọ hiện bên phải bếp khi có sốt; quầy Góc 2 chỉ còn vé đen. *Test:* `C02_*`.
- AC-2: bấm lọ = cầm lọ (không mua); chuột phải đặt lại; cầm đĩa mà bấm lọ thì nhỏ lên đĩa đang cầm. *Test:* `C02_*`, `F034…` (PlaythroughTests).
- AC-3: cầm lọ, bấm đĩa trên quầy / trong tủ: dùng 1 chai lên đĩa đó. *Test:* `C02_*`.

### C-03 Vé mỗi ngày — [YC] "tại sao ngày thì 7 vé, ngày thì 8 vé … để mặc định mỗi ngày là 8 vé, trừ ngày 1 vẫn 5 vé"
Nguyên nhân: mặc định 7 vé; relic Vé số cào +1 vé vào ngày chia hết cho 3.
- AC-1: ngày 2–22 mỗi ngày +8 vé, ngày 1 có 5 (relic Vé số cào vẫn +1 mỗi 3 ngày). *Test:* `C03_MoiNgay8Ve_Ngay1Van5Ve`.

## Chỉnh sửa

### C-04 Bài hướng dẫn ngày 2 chiếu đầu ngày 3 — [YC]
- AC-1: ngày 2 không có bài relic; đầu ngày 3 có bài "Trả vé: mua relic" → "Tab: xem relic". **[TỰ QUYẾT] Q-78:** ngày 2 vẫn là ngày relic miễn phí (dữ liệu không đổi), chỉ dời tranh. *Test:* `C04_*`, `F081_*`.

### C-05 Tỉ lệ ra bậc món — [YC] đoạn 5.3–5.5 bạn dán, "kết hợp với spec hiện tại"
- AC-1: thanh tỉ lệ đủ 6 bậc, khớp bảng 5.3 (lưới 100.000 vé, mỗi bậc ≥ 0,001%, relic chặn thì 0). *Test:* `C05_ThanhTiLe_KhopBangConstitution` (10 hàng), `C05_MoiBacToiThieu1Ve_LuoiNguyen`, `balance/cookodds.py`, `tools/check_constitution.py`.
- AC-2: nguyên liệu tốt hơn không làm giảm P(≥ bậc k). *Test:* `C05_NguyenLieuTotHon_*`.
- AC-3: Huyền thoại khi nấu tối đa 2%; relic Sao băng trong nồi +0,3%, Bí kíp gia truyền +1,0% (**[TỰ QUYẾT] Q-81** tên + giá), tổng ≤ 5%; mất điện / Chảo gang / Ớt ma theo bảng. *Test:* `C05_HuyenThoai_*`, `C05_NgayMatDien_ChaoGang_*`, `F8_OtMa_*`.
- AC-4 (Điều 3): quay 200.000 lần khớp thanh. *Test:* `C05_QuayBacMon_*`, `DishFormulaTests.AC3_*`.
- AC-5: hiển thị: ≥ 1% hai chữ số, < 1% đủ chữ số, không "0%" cho bậc còn ra được. *Test:* `C05_DinhDangPhanTram`, `C05_ThanhTiLe6Bac_DinhDangPhanTram` (PlayMode, ảnh `c05_thanh_ti_le_6_bac.png`).
- AC-6: món Huyền thoại giữ tên món; sốt / máy +1 +2 không kéo Huyền thoại xuống. *Test:* `C05_MonHuyenThoai_*`.

### C-06 Bếp trong cửa hàng — [YC] "max 2 bếp, nới rộng phần bếp như thớt. Mua thêm bếp giá 500g, mua lần 2 với giá 800g"
- AC-1: **[TỰ QUYẾT] Q-84** tổng tối đa 2 bếp (có sẵn 1), 500G; giá lần sau 800G nằm trong dữ liệu (`stove.price + step`). Mở ngày 4. Lưu trong bản lưu. *Test:* `C06_MuaBep_*` (EditMode).
- AC-2: mua xong bếp 2 hiện ngay bên phải quầy đĩa (thân bếp, núm, số "2", bảng tỉ lệ 6 bậc, đồ trong nồi); nấu độc lập bếp 1. *Test:* `C06_MuaBep_BepHaiHienNgay_NauDocLap` (ảnh `c06_bep_thu_hai.png`), `C06_HaiBep_NauDocLap`.

### C-07 Hiệu ứng nhỏ sốt — [YC] "lên được bậc huyền thoại thì hiệu ứng 7 sắc cầu vồng siêu bùng nổ, còn khi xuống lạc vị thì như quả bom xịt"
- AC-1: Huyền thoại: 7 vòng 7 màu nở + hạt bắn + chớp sáng + tiếng `cau_vong`; Lạc vị: bom ngòi cháy → xịt + khói + tiếng `bom_xit` + rung. *Test:* `C07_*` (ảnh `c07_cau_vong.png`, `c07_bom_xit.png`).

### C-08 5 khách đặc biệt + tiếng — [YC] ảnh 1, "người thứ 2: thêm 1 nốt ruồi dưới miệng; người thứ 3: logo thành ảnh 2", 3 file tiếng
- AC-1: 12 kiểu khách (7 thường + 5 đặc biệt), **[TỰ QUYẾT] Q-82** khoảng 1/4 khách là đặc biệt, từ ngày 1; hai khách liền nhau khác kiểu. *Test:* `C08_*`, `F080_*`.
- AC-2: phụ kiện đúng ảnh: VIP (tóc xám, kính râm trên đầu, nơ vàng, khuy vàng, thẻ VIP); streamer (tai nghe cam tai mèo, mic, "GG", nốt ruồi dưới miệng); vô địch (băng đô "VÔ ĐỊCH", số 66, logo xe lăn có cánh); fan Quỷ Đỏ (tóc dựng, sơn má, khăn, số 7, ngón tay xốp "#1"); đại gia (kính tròn, xấp tiền). *Test:* `C08_*` (ảnh `c08_khach_dac_biet_1/2.png`), preview Blender `SourceArt/Blender/preview/SK_Khach_*`.
- AC-3: khách 1, 2, 4 khi vào quán phát tiếng (âm 2D, **[TỰ QUYẾT]** chỉ 7 giây đầu rồi nhỏ dần vì clip dài 5–42 giây). *Test:* `C08_*`.

### C-09 Món cuối cho chủ nợ + tranh gợi ý — [YC] "đòi món ngon nhất … art tutorial … hint: bậc huyền thoại, món trứng chiên cà chua"
- AC-1: tranh "Món ngon nhất?": mặt chủ nợ → Trứng chiên cà chua Huyền thoại trong vòng 7 màu; hiện đầu vòng 4 và ngày 21. *Test:* `C09_*` (ảnh `c09_goi_y_mon_cuoi.png`).
- AC-2: **[TỰ QUYẾT] Q-80** đúng món + đúng bậc → thưởng ×1,5; Tuyệt hảo bất kỳ vẫn thưởng thường. *Test:* `C09_ChuNo_*`.

### C-10 Hướng dẫn toàn màn hình, phím T — [YC]
- AC-1: bước mới có tranh: tranh to giữa màn hình (rộng > 1000 px ở 1280×720), T ẩn / hiện; bước sau lại hiện to. *Test:* `C10_*` (ảnh `c10_huong_dan_toan_man_hinh.png`).
- AC-2: hình tròn trịa hơn: sprite tròn / tam giác / bát giác 256 px, khung bo góc lớn 160 px.

### C-11 Bỏ dòng điều khiển ở HUD, đưa vào Cài đặt — [YC]
- AC-1: HUD không còn "DieuKhien"; Cài đặt có cột Điều khiển (9 phím, có T). *Test:* `C11_*` (ảnh `c11_cai_dat_dieu_khien.png`).

## Ngoài phạm vi
Bot dùng bếp thứ hai (như thớt, K-4); mô phỏng Python không mô hình tỉ lệ 6 bậc (bot C# là chuẩn); giọng khách ở bản build cần bạn nghe thử âm lượng.
