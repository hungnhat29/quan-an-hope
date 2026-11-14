# F-008 — Mặt nguyên liệu, mặt món, đồ bay vào bếp, dải tỉ lệ bậc món

Trạng thái: giả lập đạt, chờ người chơi thử (14/11/2026) · Bổ sung cho F-007
Nguồn: `docs/art-bible/` v1.1 §6 (mắt), §7 (nguyên liệu và món theo bậc, ảnh 08), §11 (hiệu ứng), §12 (animation); `specs/constitution.md` v1.2 Điều 9
Duyệt: tin nhắn của người dùng ngày 14/11/2026, sau khi chơi bản 3D:
> "vẽ nguyên liệu có khuôn mặt và biểu cảm như docs/art-bible/ và hiệu ứng món ăn lạc vị, bình thường,... cũng phải có biểu cảm
> - cho nguyên liệu vào cũng phải có animation và hình ảnh hiển thị
> - HUD xác xuất món ăn thành bậc gì hiển thị nhỏ lại: nằm trên 1 hình chữ nhật cao khoảng 2-3cm và dài, hiển thị đủ cả những bậc còn lại dù là 0%"

## Luật

1. Mỗi nguyên liệu có mặt theo bậc, giống ảnh 08: 0 Ôi thiu mắt X + ruồi (thân xỉn màu), 1 Bình thường mặt phẳng, 2 Tốt cười + giọt nước, 3 Tươi lấp lánh, 4 Xuất sắc vương miện, 5 Hiếm kính đen + hào quang. Mắt là vòng tròn rỗng, hở khe 40° phía trên (§6). *(AB §6, §7)*
2. Mỗi món có mặt theo bậc món: Lạc vị mắt X, Dở buồn, Bình thường mặt phẳng, Ngon vui, Tuyệt hảo ngạc nhiên, Huyền thoại kính đen + vương miện. Hơi bốc lên theo bậc: khói xám gấp khúc, không có, 3 nét hơi trắng, trái tim trắng, trái tim + lấp lánh, trái tim vàng. *(AB §7)*
3. Mặt luôn quay về phía người chơi, không chắn tia nhìn.
4. Cho đồ vào bếp có hình và chuyển động: nguyên liệu bay theo đường cong từ tay vào chảo / nồi / vỉ, chạm thì nảy (bẹp 15%); bấm lọ gia vị thì lọ nghiêng và một giọt màu của lọ bay vào. Đồ nằm lại trong dụng cụ; đang nấu thì rung nhẹ; múc ra đĩa hoặc đổ bỏ thì hết. *(tin nhắn 14/11; AB §12)*
5. Món ra lò bật lên trên quầy chờ trong 0,6 giây; Lạc vị rung camera 0,15 giây. *(AB §11)*
6. Tỉ lệ bậc món trên bảng của bếp là một dải chữ nhật dài, thấp, đủ 6 bậc theo thứ tự, mỗi ô ghi tên bậc và %, kể cả 0% (chữ mờ hơn). *(tin nhắn 14/11; Điều 9)*

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Thùng vừa rơi xuống | Nhìn vào thùng | Mỗi món có mặt đúng bậc (ảnh `Mat_NguyenLieu_<bậc>`), mặt quay về người chơi, không có va chạm | Luật 1, 3 |
| 2 | Đang cầm nguyên liệu đã sơ chế, bếp có dụng cụ | Bấm bếp | Một vật bay vào dụng cụ, đáp xuống trong lòng dụng cụ, có mặt theo bậc | Luật 4 |
| 3 | Bếp có dụng cụ | Bấm lọ gia vị | Lọ nghiêng; một giọt bay vào và nằm lại trong dụng cụ | Luật 4 |
| 4 | Đồ đã cho vào | Vặn núm | Đồ trong bếp rung nhẹ khi đang nấu | Luật 4 |
| 5 | Món chín | Lấy đĩa trống, bấm bếp | Bếp trống; đĩa trên quầy chờ có mặt `Mat_Mon_<bậc>` và hơi đúng bậc (Dở: không có hơi) | Luật 2, 4, 5 |
| 6 | Đủ nguyên liệu chính + phụ | Nhìn bảng bếp | Dải dài, cao ≤ 11 cm trên bảng, dài gấp > 10 lần cao, đủ 6 ô Lạc vị → Huyền thoại, % đúng công thức, tổng 100% | Luật 6 |

## Mặc định cần bạn xác nhận khi chơi thử

- **Q-19 Cỡ dải tỉ lệ:** "cao 2–3 cm" hiểu là cỡ nhìn thấy trên màn hình. Dải thật trên bảng bếp cao 10,4 cm × dài 1,42 m (bảng là đồ vật trong phòng), đứng ở mép lối đi trước bếp thì nhìn khoảng 2 cm trên màn 24 inch. Cần nhỏ hơn hoặc to hơn thì báo một con số.
- Mặt vẽ bằng code lúc chạy (`FaceArt`), không phải file PNG; khi có Blender (sprint 7–8) có thể thay bằng ảnh vẽ tay cùng quy cách.
- Ô bậc 0% vẫn tô màu bậc nhưng chữ mờ 45% để thấy ngay bậc có thể ra.
- Đĩa đang cầm trên tay có mặt nhưng không có hơi bay lên (để hơi không che tâm ngắm).

## Ngoài phạm vi

- Chớp mắt, âm thanh, hạt bay tung tóe khi rơi vào chảo.
- Mặt khách thêm Buồn / Ngạc nhiên / Đang ăn (vẫn 3 ảnh cũ).
- Hiệu ứng thùng lộ bậc từng món với âm thanh riêng.

## Ma trận AC → test

`Tests/PlayMode/PlaythroughTests.cs` (người chơi giả đi lại, bấm qua tâm ngắm):

| AC | Kiểm ở | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `La3D_DiLaiTuDo_VaChamThat_KhongNutChuyenGoc` (8 món trong thùng) | đạt |
| 2, 3 | `CookOne` → `CheckPanSettled` sau mỗi bước, mọi món nấu trong 2 test | đạt |
| 4 | `CookOne`: mọi `PanItem.Sizzle` khi đang nấu | đạt |
| 5 | `CookOne` → `CheckDishFaces` (gồm 1 đĩa Lạc vị) | đạt |
| 6 | `CookOne` → `CheckTierStrip` | đạt |

Ảnh: `docs/sprint-1/ui/3d_goc2_thung.png`, `3d_bep_dang_nau.png`, `3d_quay_cho.png`.

## Cách làm

| File | Việc |
| --- | --- |
| `Game/FaceArt.cs` (mới) | Vẽ mặt nguyên liệu, mặt món, hạt hiệu ứng bằng hàm khoảng cách lúc chạy; `FaceBillboard` quay về camera |
| `Game/KitchenFx.cs` (mới) | `PanItem` (bay + nảy + rung), `Wobble` (lọ nghiêng), `Pop` (món ra lò), `DishFx` (hơi theo bậc) |
| `Game/RoomView.cs` | Gắn mặt vào `ItemView` và `DishModel`; đồ trong bếp; dải tỉ lệ 6 ô |
| `Game/GameController.cs` | Báo `RoomView.OnAddedToStove` khi bếp nhận đồ; rung camera khi ra Lạc vị |
| `Game/PlayerController.cs` | `Shake` |
