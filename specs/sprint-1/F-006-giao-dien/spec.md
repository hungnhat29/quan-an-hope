# F-006 — Giao diện lát cắt

Trạng thái: giả lập đạt, chờ người chơi thử (14/11/2026) · Nguồn: `specs/constitution.md` v1.1, `docs/art-bible/` v1.0 · Duyệt: bảng AC trong `docs/sprint-1/impl-plan.md`, người dùng yêu cầu thực hiện ngày 5–7 (14/11/2026)

## Luật

1. Người chơi chỉ ở trong một căn phòng; mỗi tường một khu chức năng. *(Điều 5)*
2. Chữ trong UI dùng TextMeshPro. *(Điều 6)*
3. Hiện tỉ lệ gacha nguyên liệu và khoảng bậc món; giấu tỉ lệ máy gacha đồ ăn. *(Điều 4)*
4. UI theo Art Bible §9 (vùng an toàn) và §13 (khung, chữ, màu nút, bảng tỉ lệ).

## Tiêu chí chấp nhận

| AC | Then | Nguồn |
| --- | --- | --- |
| 1 | HUD hiện ngày, tiền, nợ sắp tới, số vé bằng TextMeshPro, đủ dấu tiếng Việt | Điều 6 |
| 2 | Chuyển góc bằng nút trái/phải; mỗi góc chỉ hiện UI của góc đó | Điều 5 |
| 3 | Không còn bảng tỉ lệ máy gacha đồ ăn trong cảnh | Điều 4 |
| 4 | Chơi được từ ngày 1 tới màn kết quả mà không cần mở Inspector | mục tiêu sprint |

Kèm theo, từ các tính năng khác: F-002 AC-7 (bảng tỉ lệ vòng ở Góc 2), F-003 AC-5 và AC-6 (thanh tỉ lệ món, trình tự thớt → bếp → đĩa), F-004 AC-8 (độ dễ tính %, chuông).

## Cách làm

- `GameController` tự gắn vào cảnh nào có `CameraRig`; không sửa cảnh sinh ra từ `BuildNhaHang.cs`.
- `GameView` dựng toàn bộ UI lúc chạy bằng UGUI + TextMeshPro; `UiKit` giữ quy cách Art Bible (khung bo góc, viền, bóng đặc, màu vai trò, sáu màu bậc).
- Font Baloo 2 Bold (`Resources/Fonts`, giấy phép OFL) tạo thành font TextMeshPro động lúc chạy.
- `BalanceData.asset` chuyển vào `Resources/` để game tải được.
- Góc 3 hiện sách nấu ăn (§5.8: "Sách nấu ăn (Góc 3)"); cửa hàng relic để sprint 2.

## Mặc định cho chỗ constitution chưa nói (cần bạn xác nhận)

- **Q-16 Múc món:** constitution §5.1 tả hai bước (lấy đĩa, rồi bấm vào bếp). Bản này gộp thành một nút "Múc ra đĩa".
- Khách chỉ hiện độ dễ tính (%), không hiện món tối thiểu họ chấp nhận.
- Chuông phục vụ đĩa đang được chọn trên quầy chờ; đĩa vừa múc được chọn sẵn.
- Kho ở Góc 4 gộp các món cùng loại, cùng bậc thành một ô có số lượng.

## Ngoài phạm vi

- Thay ảnh chữ PNG còn lại trong cảnh 3D (biển hiệu, nhãn relic ở Góc 3, nhãn máy nâng phẩm).
- Hiệu ứng thùng rơi, lộ bậc, hơi món, đồng xu tip (Art Bible §11); âm thanh.
- Biểu cảm Buồn / Ngạc nhiên / Đang ăn (chỉ có 3 ảnh mặt: vui, chờ, giận).
- Màu phủ theo giờ, mưa ngày 2 (Art Bible §4).

## Ma trận AC → kiểm

Giả lập: `Tests/PlayMode/PlaythroughTests.cs` chạy trên cảnh `NhaHang`, bấm đúng các nút trên UI. Ảnh chụp ở `docs/sprint-1/ui/`.

| AC | Giả lập kiểm | Kết quả 14/11 | Cần bạn nhìn |
| --- | --- | --- | --- |
| F-006 AC-1 | Chữ HUD đúng; Baloo 2 có đủ 67 ký tự có dấu; không chữ nào dưới 14 px | đạt | Chữ có đẹp, dễ đọc không |
| F-006 AC-2 | Xoay đủ 4 góc; mỗi góc chỉ UI của nó; mọi nút nằm trong 80% giữa chiều ngang | đạt | Camera xoay có êm không |
| F-006 AC-3 | Không còn đối tượng `OddsBoard` | đạt | — |
| F-006 AC-4 | Chơi trọn 3 ngày bằng nút bấm tới "Qua vòng 1"; ván khác tới "Thua rồi"; "Chơi lại" về ngày 1 | đạt | Chơi thật bằng chuột |
| F-002 AC-7 | Bảng ghi 25/57/15/3/0/0 ở vòng 1 và 50/42/8/0/0/0 ở ngày 3 | đạt | — |
| F-003 AC-5 | Thanh tỉ lệ hiện khi đủ chính + phụ | đạt | — |
| F-003 AC-6 | Sơ chế bằng nút giữ, chọn dụng cụ, cho đồ, chờ nấu theo thời gian game, múc | đạt | Giữ chuột có thuận tay không |
| F-004 AC-8 | Bong bóng ghi "Khách 1/6", "Dễ tính N%"; chuông đổi tiền đúng bảng tính | đạt | — |
