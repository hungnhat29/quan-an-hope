# F-007 — Chơi 3D góc nhìn thứ nhất, thông tin trên đồ vật

Trạng thái: giả lập đạt, chờ người chơi thử (14/11/2026) · Thay thế phần hiển thị của F-006
Nguồn: `specs/constitution.md` **v1.2** (Điều 5, Điều 9, §4.2, §5.1, §5.8, §6.1), `docs/art-bible/` **v1.1** (§7, §8, §9, §10, §13)
Duyệt: nghiệm thu sprint 1 của người dùng ngày 14/11/2026:
> "vì game này là 3D nhưng hiện tại thì chơi như game 2D: bỏ đi nút bấm chuyển góc, tôi muốn tự di chuyển; […] chỉ những con số hoặc thông tin thì hiển thị ở HUD thôi còn ví dụ như hiển thị các nguyên liệu trong 1 thùng thì cho người dùng thấy ở thùng […] chỉ cần hiển thị màu để biết được bậc của nguyên liệu […] công thức nấu ăn hiển thị ở quyển sách như bản phác thảo đã vẽ […] hãy dựng game là 1 tựa game 3D và các góc nhìn, góc ánh sáng cx vậy"

## Luật

1. Người chơi tự đi lại trong phòng bằng W A S D / mũi tên, nhìn bằng chuột; không có nút chuyển góc. *(Điều 5)*
2. Tương tác bằng tâm ngắm + chuột trái (bấm / giữ); chuột phải đặt lại đồ đang cầm. Cầm tối đa một thứ. *(Điều 5)*
3. Thông tin nằm trên đồ vật; HUD chỉ có con số và thông tin ngắn, không có nút. *(Điều 9)*
4. Bậc nguyên liệu đọc bằng màu đế dưới mỗi món trong thùng. *(Điều 9; Art Bible §7)*
5. Công thức nằm trên trang sách 3D như bản phác. *(§5.8)*
6. Mỗi món: lấy từ thùng → thớt (giữ) → chọn dụng cụ trên giá → cho vào bếp / bấm lọ gia vị theo thứ tự → vặn núm → lấy đĩa trống → bấm bếp để múc → đĩa lên quầy chờ → cầm ra Góc 1 → bấm chuông. *(§5.1)*
7. Ánh sáng theo một đèn chính cố định trong phòng; va chạm thật với tường và đồ đứng trên sàn. *(Art Bible §8, §10)*

Luật chơi (tiền, bậc, khách, nợ) không đổi: Core giữ nguyên, 120 test EditMode vẫn xanh.

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Vào cảnh | — | Không còn camera xoay góc, nút chuyển góc, canvas UI cũ; mắt người chơi cao 1,75 m | Điều 5, AB §9 |
| 2 | Đứng giữa phòng | Giữ phím tiến | Người chơi đi tới; đi thẳng vào quầy Góc 1 thì bị chặn trước quầy | Điều 5, AB §8 |
| 3 | Đang chơi | Nhìn HUD | Chỉ có ngày, ví, nợ, vé, tâm ngắm, gợi ý, đồ đang cầm, thông báo; không có nút nào | Điều 9, AB §13 |
| 4 | Đứng trước máy nhét vé | Nhìn vào máy, bấm | Mất 1 vé; thùng rơi xuống bàn; mỗi món là vật 3D trong thùng, đế bậc đúng màu bậc | §4.2, Điều 9 |
| 5 | Góc 2 | Nhìn bảng trên tường | Thấy tỉ lệ 6 bậc của ngày; ngày 3 đổi sang bảng sau mưa | Điều 4, Điều 9 |
| 6 | Góc 3 | Nhìn sách, bấm trang phải | Trang trái tên món + dụng cụ + đế + thời gian, trang phải thứ tự cho vào có số; lật được; chỉ có món đã mở khóa | §5.8 |
| 7 | Có nguyên liệu trong thùng | Làm đủ chuỗi ở luật 6 | Món lên quầy chờ đúng tên; thanh khoảng bậc hiện trên bảng của bếp; cho sai thứ tự ra Lạc vị | §5.1, §5.3 |
| 8 | Đang cầm nguyên liệu đã sơ chế | Chuột phải | Nguyên liệu về lại thùng, vẫn là đã sơ chế | Điều 5 |
| 9 | Đầu ngày | Bấm biển Mở quán ở Góc 1 | Khách vào, bong bóng cạnh đầu khách ghi "Khách n/6" và "Dễ tính N%"; bấm biển lần nữa thì đóng quán sớm | §6.1 |
| 10 | Cầm đĩa từ quầy chờ | Bấm chuông | Tiền đổi đúng bảng tính tiền; đủ 6 khách thì hết ngày | §5.1, §6.5 |
| 11 | Chơi 3 ngày | Sang ngày 4 | Đủ 400G: qua vòng 1; không bán gì: thua | §2 |
| 12 | Cảnh có đèn chính | — | Shader nhận hướng sáng từ đèn (chiếu từ trên xuống) | AB §10 |

## Mặc định cho chỗ constitution chưa nói (cần bạn xác nhận khi chơi thử)

- **Q-17 Tốc độ và độ nhạy:** đi 2,6 m/giây, chuột 0,1° mỗi điểm ảnh, tầm với 3 m (ghi ở Art Bible §9). Chỉnh sau khi bạn chơi.
- **Q-18 Thùng gộp theo loại:** mọi món cùng loại trong ngày nằm chung một thùng (constitution §4.2 đã ghi theo cách này ở bản 1.2), thay vì mỗi lượt gacha một cặp thùng riêng.
- Nhãn tên loại nguyên liệu trên thùng, tên đồ kho dưới lọ, tên món trước đĩa: là chữ trên đồ vật, không phải HUD.
- Món mất thêm thời gian đi lại giữa Góc 2 và Góc 4 (đường chéo phòng). Vòng 1 không tính giờ chờ nên chưa ảnh hưởng.

## Ngoài phạm vi

- Model Blender (vẫn là khối cơ bản), animation khách, âm thanh.
- Cửa hàng relic ở Góc 3 (sprint 2) — hiện biển "sắp mở".
- Máy gacha đồ ăn (sprint 3) — hiện biển "sắp mở".
- Hỗ trợ tay cầm, đổi phím, chỉnh độ nhạy trong game.

## Ma trận AC → test

Giả lập: `Tests/PlayMode/PlaythroughTests.cs`. Người chơi giả đi bằng `CharacterController` thật (nhập phím giả), quay đầu bằng góc nhìn, bấm qua tia nhìn ở tâm ngắm. Mỗi lần bấm kiểm tâm ngắm trúng đúng đồ vật từ chỗ đứng ở mép lối đi.

| AC | Test | Kết quả 14/11 |
| --- | --- | --- |
| 1, 2, 3, 4, 5, 6, 12 | `La3D_DiLaiTuDo_VaChamThat_KhongNutChuyenGoc` | đạt |
| 4, 5, 7, 8, 9, 10, 11 | `ChoiTronVong1_Bang3D_QuaVong1` (3 ngày, 18 khách, qua vòng 1 với 1.190G) | đạt |
| 9, 11 | `KhongBanGi_DauNgay4_ManThua` | đạt |

Ảnh chụp từ giả lập: `docs/sprint-1/ui/3d_*.png`.

## Cách làm (plan + tasks)

| File | Việc |
| --- | --- |
| `Game/PlayerController.cs` | Đi, nhìn, tia nhìn, bấm / giữ; hàm cho test (`SimulateInput`, `LookAt`, `TapPrimary`) |
| `Game/Interactable.cs` | Đồ vật bấm được: id, gợi ý, bấm, giữ |
| `Game/RoomView.cs` | Dựng mọi đồ tương tác, bảng chữ trên đồ vật, thùng và nguyên liệu, đĩa, sách, va chạm, bóng tròn |
| `Game/Hud.cs` | HUD số liệu + màn kết thúc ngày |
| `Game/GameController.cs` | Thao tác người chơi + tay cầm; luật vẫn ở Core |
| `Game/Toon.cs` | Dựng khối toon + viền lúc chạy |
| `BepNhanPham/Scripts/ToonLight.cs`, `Shaders/ToonFlat.shader` | Đèn chính → shader |
| `BepNhanPham/Editor/BuildNhaHang.cs` | Bỏ camera xoay góc và đồ trang trí giả; thêm Player, đèn chính; nồi thành nhóm riêng |

- [x] Constitution v1.2, Art Bible v1.1
- [x] Đèn chính + shader
- [x] Người chơi, tương tác, tay cầm
- [x] Đồ vật 4 góc, HUD mới; xoá `GameView.cs`, `CameraRig.cs`
- [x] Giả lập PlayMode viết lại; sửa 4 lỗi giả lập bắt được (rơi qua sàn, khung hình dài khi tua nhanh, quầy chờ dựng lại liên tục, thùng che màu đế)
- [x] Build Windows
