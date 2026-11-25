# Spec — Sau sprint 10, đợt 2 (24/11/2026)

Nguồn: tin nhắn của bạn 24/11/2026 (**[YC]**). Constitution 2.1 → **2.2**. Chỗ chưa rõ: **[TỰ QUYẾT]** → `.dev-memory/QUESTIONS.md` Q-85..Q-87.

## Bảng Art Bible → làm ở đâu (L-013)
| Quy định | Làm ở |
| --- | --- |
| §5 hình tròn, viền mực; §11 phản hồi 0,6–1,2 giây | Máy gacha mới `Sprint12Room.BuildMachine2` (khối bo, viền), cần gạt 0,75 s, bảng kết quả 2,8 s |
| §13 chữ ≥ 14 px | Ô đồ (tên 14 px), gợi ý cốt truyện 15 px |
| Phong cách hướng dẫn: chữ chú thích một dòng | Nhãn bậc trong tranh "Màu đế = bậc" không xuống dòng |

## Lỗi
### D-01 Máy gacha đồ ăn — [YC] "bỏ đồ ăn vào mà đồ ăn trên tay không mất mà cũng không biết món ăn đã được gacha chưa … như lò vi sóng … gạt cần gạt … hiệu ứng thành công hay là xịt"
Kịch bản 10 giây: cầm đĩa, bấm máy → đĩa rời tay, nằm trên mâm sau cửa kính, màn hình "3/3" → gạt cần bên phải: cần gập xuống, mâm xoay, 3 đèn nhấp nháy, máy rung 1,8 s → bảng "THÀNH CÔNG! +1 bậc" (xanh) / "XỊT! −1 bậc" (san hô) + hiệu ứng → bấm máy lấy đĩa ra.
- AC-1: cho vào thì đĩa rời tay, hiện trong máy, không còn trên quầy. *Test:* `D01_*`, `F034_*`.
- AC-2: gạt cần quay; kết quả hiện sau 1,8 s, có bảng + hiệu ứng; quay rồi không quay lại được; lấy ra bằng cách bấm máy. *Test:* `D01_*`, `F034_*` (ảnh `d01_may_co_dia.png`, `d01_ket_qua.png`).
- AC-3: vẽ lại máy (lò vi sóng) — máy cũ của cảnh ẩn hẳn.

### D-02 Chữ trắng đè lời kể cốt truyện — [YC]
- AC-1: gợi ý "Bấm để tiếp · Giữ để bỏ qua" ở góc phải trên trên nền mực. *Test:* `D02_*` (ảnh `d02_cot_truyen.png`).

### D-04 Nhãn bậc + phím T — [YC]
- AC-1: nhãn bậc trong tranh "Màu đế = bậc" một dòng (so le trên / dưới). *Test:* `D04_*`.
- AC-2: T tắt rồi thì bước sau không tự bật lại; chỉ T bật. *Test:* `C10_*`.

### D-05 Ngày 2 relic — [YC] "2 relic còn lại vẫn ở trên kệ nhưng không còn miễn phí và hiển thị lại giá"
- AC-1: lấy 1 relic miễn phí xong, 2 bệ còn lại hiện giá vé, mua bằng vé; đổi hàng được. *Test:* `F077_AC3_Ngay2_LayMotCai_ConLaiHienGiaVe_MuaTiepDuoc`, `D05_*`, `F077_Ngay2_*` (PlayMode).

## Chỉnh sửa
### D-03 Quầy phục vụ rộng — [YC] "chậu hoa … chuông và cái cốc … che mất đĩa … kéo dài bàn … khách hàng bị nhỏ đi"
- AC-1: quầy 6,6 m, ghế cách 2 m, khách 3 người tỉ lệ 0,88; bỏ chuông; chậu hoa / hũ tip ở hai đầu. **[TỰ QUYẾT] Q-87**. *Test:* `D03_*` (ảnh `d03_quay_rong.png`), `F080_*` (khách không lồng nhau).

### D-06 Tay vạn năng — [YC] "cầm được 2 đồ … chọn số 1 2 … mặc định là sử dụng và thả theo lần lượt … ô đồ ở dưới màn hình như minecraft … tự định giá, chỉ mua 1 lần"
- AC-1: **[TỰ QUYẾT] Q-85** 600G, mở ngày 4, mua 1 lần, lưu trong bản lưu. *Test:* `D06_TayVanNang_600G_*`.
- AC-2: có tay vạn năng thì nhặt được thứ hai vào ô trống; mặc định dùng / thả ô nhặt trước; phím 1 / 2 chọn ô; đồ ô kia hiện ở tay trái; ô đồ ở đáy màn hình (ô đang dùng màu mù tạt). *Test:* `D06_TayVanNang_Cam2Do_*` (ảnh `d06_tay_van_nang.png`).

### E-01 Tranh hướng dẫn kiểu diorama giấy — [YC] "thay đổi design của phần art tutorial theo docs/sprint-10/ui/paper-ui-pickit.md"
- AC-1: thẻ theo cấu trúc lớp mục 2 (carton, khung kem khoét cửa sổ, nền theo nhóm mục 5, sticker viền trắng + bóng, dải giấy xé + băng dính, huy hiệu đinh ghim, hạt giấy). *Test / ảnh:* `B08_*` (`b08_tab_cach_choi_1/2.png`, `b08_huong_dan_bac.png` so với `b08v2_*`).
- AC-2: căn chỉnh mục 3: không viên thuốc nào xuống dòng; nhãn lệch tâm hình ≤ 2 px; lưới 3 × 2 cho thẻ 6; thẻ 3 / 7 / 8 / 9 / 10 / 11 / 12 sửa theo bảng mục 1. *Test:* `B08_*` (AssertPaperAlignment), `D04_*`.
- AC-3: stop-motion mục 4 (tay ma 12 hình/giây, rung viền, thẻ bật, Tab dán lần lượt) + Cài đặt "Hiệu ứng giấy". *Kiểm tay* (chuyển động), `C11_*` (Cài đặt).
- AC-4: thẻ trong game bỏ huy hiệu số, băng rôn giấy xé "1/10", chip "T ẩn / hiện"; Tab: chấm trang, nút hết trang mờ, không vòng trang. *Test:* `B08_*`, `C10_*`.

### F (đợt 3, 24/11/2026)
- F-01 lỗi chữ đè ở Tab → Cửa hàng (6 dòng × 92 px > khung): dòng 82 px, ghi chú nằm dưới. *Test:* `F01_*` (ảnh `f01_cua_hang.png`).
- F-02 khối bo tròn mà ruột vuông (cửa sổ tranh, thanh sơ chế / nấu / chờ): mặt nạ bo tròn + ruột dùng sprite bo. *Test:* `F02_*`.
- F-03 bỏ hộp hướng dẫn vàng. *Test:* `F03_*`, `F050_F053_*` (dòng hướng dẫn vẫn chạy, chỉ không hiện).
- F-04 viền đen: tách chi tiết khách thành mesh `_VienMong` / `_KhongVien`, mép tròn nhiều đoạn hơn; lọ sốt, máy gacha (vỏ + lòng), bếp 2 dựng bằng Blender. *Test:* `F04_*`, `F070_AC3_*` (quy cách model; nhóm Machines ≤ 3.000 tam giác, ≤ 2,5 m), ảnh `f04_*.png`.

## Ngoài phạm vi
Bot dùng tay vạn năng / máy theo luồng mới (bot gọi thẳng luật, không đi lại).
