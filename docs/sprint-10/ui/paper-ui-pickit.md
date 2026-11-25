# UI hướng dẫn bằng tranh: phong cách diorama giấy (B-08 v2)

Ngày: 24/11/2026. Ảnh mẫu nằm trong cùng thư mục:
- `b08v2_tab_cach_choi_1.png`, `b08v2_tab_cach_choi_2.png`: Tab Cách chơi, 2 trang.
- `b08v2_buoc_1.png`, `b08v2_buoc_2.png`: thẻ bước trong game.
- `b08v2_thong_so.png`: thông số PicKit.

Mục tiêu: giữ nguyên nội dung và luật của B-08 (AC-1…AC-5), chỉ đổi cách vẽ sang kiểu hộp diorama giấy, đồng thời sửa các lỗi căn chữ và căn hình.

## 1. Lỗi căn chỉnh ở bản hiện tại (ảnh b08_*.png) → cách sửa

| Chỗ | Lỗi | Sửa |
| --- | --- | --- |
| Thẻ 6 "Màu đế = bậc" | 6 cà chua một hàng, nhãn chia 2 hàng so le, không thẳng với cà chua | Lưới 3 × 2. Mỗi cà chua có nhãn ngay dưới, căn giữa theo tâm. Thứ tự đọc từ trái sang phải, từ trên xuống dưới: Ôi thiu, Bình thường, Tốt, Tươi, Xuất sắc, Hiếm |
| Thẻ 8, 9, 10 | "Bình thường", "Tuyệt hảo", "Sai món: −tiền" tràn hoặc xuống dòng trong viên thuốc | Viên thuốc rộng cố định theo cột (96–108 px), `enableWordWrapping = false`, chữ 14 px. "Sai món: −tiền" rút còn "−tiền" kèm đồng xu |
| Thẻ 3 | Số bước nằm xa hình, hình quá nhỏ | Số bước đặt thẳng trên tâm từng hình, giữa các hình có dấu ›, dùng một mũi tên duy nhất vào chảo |
| Thẻ 7 | Hình nhỏ, viên "Ngon" lơ lửng | Nguyên liệu lấy đế vàng (Hiếm), đĩa phát sáng, "Ngon" đặt ngay dưới đĩa |
| Thẻ 11 | Hình nhỏ, "−20G" tách rời | 3 mặt khách và 3 thanh chờ cùng cột, nhãn "−20G" dán cạnh mặt khách cuối |
| Thẻ 12 | Dấu cảnh báo bay riêng, không gắn với chảo | Dấu cảnh báo đặt sát cột khói của chảo, dấu ✓ cạnh đĩa |
| Dải chữ | Viên thuốc to lặp lại khung thẻ | Thay bằng dải giấy xé dán băng dính, chữ 20 px, căn giữa |
| Chân trang | "Trang 1/2 · số liệu theo ngày hiện tại" dài | Đổi thành 2 chấm trang, nút ‹ Trước bị mờ ở trang 1 |
| Thẻ bước trong game | Thẻ to nhưng hình vẫn nhỏ, nửa dưới trống | Hình phóng theo cửa sổ thẻ (`artScale = windowWidth / 340`). Thẻ trong game bỏ huy hiệu số vì băng rôn đã ghi "1/10" |
| Băng rôn bước | "1/10 · Nhét vé" là chữ trần trên nền game | Dải giấy xé có huy hiệu "1/10" bên trái, tiêu đề 25 px |

## 2. Cấu trúc một thẻ (PicKit.Frame), từ sau ra trước

| Lớp | Thành phần | Giá trị (ở 1280 × 720) |
| --- | --- | --- |
| 0 | Nền cửa sổ: màu nhạt theo thẻ + chấm tường + dải sàn | Giảm bão hòa khoảng 25%. Không viền trắng |
| 1 | Bìa carton phía sau | `#C9A47E`, lệch (4, 6) px, bo 18 |
| 2 | Đồ vật (sprite cắt giấy) | Viền trắng `#FFFDF6` 3 px bên ngoài nét mực, bóng `#2B1A10` lệch (3…8) px, mờ 4–8 px, độ đậm 22–40%. Lớp càng gần thì bóng càng xa |
| 3 | Khung kem khoét cửa sổ | `#FFF3DC`, viền mực 3,5 px, cửa sổ lùi vào 10 px, có bóng trong cửa sổ (viền tối 10 px, mờ) |
| 4 | Dải chữ giấy xé + 2 miếng băng dính mù tạt 55% | Rộng 76% thẻ, cao 34 px, chữ Baloo 2 20 px |
| 4 | Huy hiệu số | Đĩa mù tạt bán kính 17, có đinh ghim xám ở trên |
| 5 | Bàn tay ma | Găng kem, độ mờ 92%. Chạm = vòng nét đứt; giữ = vòng mù tạt chạy dần |

Gợi ý triển khai trong Unity:
- Mỗi lớp là một `Image` riêng trong cùng `RectTransform`.
- Bóng dùng `UI Shadow`, hoặc nướng sẵn vào sprite (rẻ hơn).
- Viền trắng nướng sẵn vào sprite đồ vật, tức "sticker".
- Mép giấy xé là một sprite 9-slice có mép răng cưa.
- Một lớp hạt giấy (`grain`, tile 256 px, độ đậm 6–8%) phủ lên toàn bộ panel.

## 3. Quy tắc căn chỉnh

1. Cửa sổ hình chia 3 cột đều, tâm cột ở 18% / 50% / 82%. Nếu có 2 hàng thì tâm hàng ở 28% và 76%.
2. Nhãn luôn nằm dưới hình, căn giữa theo tâm hình, cách 16 px. Không để nhãn lệch sang cột khác.
3. Viên thuốc có độ rộng cố định theo cột, không co theo chữ, không xuống dòng. Nếu chữ không vừa thì rút chữ, không thu nhỏ font.
4. Chữ ≥ 14 px; dải chữ ≤ 4 từ; tiêu đề bước ≤ 6 từ (AC-1).
5. Hình chiếm ≥ 60% diện tích cửa sổ; mỗi thẻ một hành động.
6. Đúng / sai / cảnh báo phân biệt bằng hình dạng, không chỉ bằng màu:
   - đúng = tròn bạc hà có dấu ✓;
   - sai = bát giác san hô có dấu ✗;
   - cảnh báo = tam giác mù tạt có dấu !
7. Mũi tên vàng viền mực luôn nằm ngang, giữa hai cột. Dấu › mờ dùng cho chuỗi bước.

## 4. Chuyển động kiểu stop-motion

| Cái gì | Nhịp |
| --- | --- |
| Bàn tay ma, đồ vật nhún | 12 hình/giây, giữ mỗi tư thế 2 khung (on twos). Vòng cử chỉ vẫn < 3 giây (AC-3) |
| Rung viền giấy | Lệch ±1 px, đổi 8 lần/giây (luân phiên 3 bộ offset) |
| Thẻ mở ra | Bật 120% → 96% → 100% trong 6 hình. Các lớp hiện lần lượt từ sau ra trước, mỗi lớp trễ 2 hình |
| Lật trang Tab | Thẻ cũ trượt ra, thẻ mới "dán" vào, mỗi thẻ trễ 2 hình |
| Camera, chữ, nút bấm, thanh trượt | Mượt 60 fps, không bao giờ giật theo hình |
| Cài đặt | Hiệu ứng giấy: Đầy đủ / Giảm (tắt rung viền, tay ma mượt) / Tắt |

Thời lượng chuyển cảnh vẫn theo nhịp Material đã có trong B-08: 225 / 200 / cử chỉ / 400 / 195 / 450 ms. Chỉ phần chuyển động bên trong mỗi khoảng là chạy theo bước 12 hình/giây.

## 5. Màu nền cửa sổ theo nhóm thẻ

| Nhóm | Nền / chấm | Thẻ |
| --- | --- | --- |
| Thao tác chung | `#F3DFB8` / `#E9CE9E` | 1, 3, 6, 8, 10 |
| Nguyên liệu, sơ chế | `#E6EFD9` / `#D3E3C1` | 2, 7 |
| Khách | `#F7E3D6` / `#EECBB8` | 4, 11 |
| Ngày, độ khó | `#EAE2F5` / `#D8CBEE` | 5, 9 |
| Bếp, khói | `#E3ECF0` / `#CCDDE5` | 12 |

## 6. Test cần cập nhật

- `B08_…`: thêm kiểm tra không viên thuốc nào có `textInfo.lineCount > 1`.
- `B08_…`: kiểm tra mỗi nhãn có tâm x lệch so với hình bên trên ≤ 2 px.
- Chụp lại `b08_tab_cach_choi_1/2.png` và so với ảnh mẫu `b08v2_*`.
