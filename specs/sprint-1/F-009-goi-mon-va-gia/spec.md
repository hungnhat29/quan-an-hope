# F-009 — Khách gọi món cụ thể, giá bán trên sách công thức, toàn màn hình

Trạng thái: giả lập đạt, chờ người chơi thử (14/11/2026)
Nguồn: `specs/constitution.md` **v1.3** (§5.8 giá trên trang sách, §6.5 dòng "Sai món", §6.7 gọi món, Điều 9)
Duyệt: tin nhắn của người dùng ngày 14/11/2026:
> "chỉnh sửa specs là khách hàng phải order 1 món ăn cụ thể và từng món phải hiển thị giá ở trên quyển sách công thức nấu ăn; sau đó chỉnh sửa lại sản phẩm và game hiện tại đang ở chế độ cửa sổ hãy trở về lại chế độ fullscreen"

và ba câu trả lời cùng ngày: khách gọi **món nấu được**; mang sai món thì khách **nhận nhưng giận**; giá **chung theo vòng, ghi theo bậc**.

## Luật

1. Khách vào thì gọi một món: chọn đều trong các món làm được lúc đó (đĩa trên quầy chờ, món trên bếp, cặp loại chính + phụ trong kho); không làm được món nào thì chọn đều trong các món đã mở khóa của sách. *(§6.7)*
2. Món gọi hiện trên bong bóng cạnh đầu khách, cùng số thứ tự khách và độ dễ tính. *(Điều 9)*
3. Đúng món: trả tiền + tip theo bậc như cũ. Sai món: khách nhận đĩa, không trả, quán bị trừ 30% giá gốc (vòng 1: −18G). *(§6.5, §6.7)*
4. Khi cầm đĩa sai món nhìn vào chuông, gợi ý cảnh báo "Sai món: khách gọi …"; bấm vẫn đưa. *(Điều 9: gợi ý ở tâm ngắm)*
5. Trang trái của sách ghi giá bán của vòng theo 5 bậc Lạc vị → Tuyệt hảo (vòng 1: 12G · 30G · 60G · 108G · 192G), mỗi ô tô màu bậc món. *(§5.8, §5.2)*
6. Bản build mở toàn màn hình ở độ phân giải gốc của màn hình, kể cả khi lần chạy trước để chế độ cửa sổ. *(tin nhắn 14/11)*

## Tiêu chí chấp nhận

| AC | Given | When | Then | Nguồn |
| --- | --- | --- | --- | --- |
| 1 | Kho có 1 thùng chính + 1 thùng phụ | Mở quán | Khách gọi đúng món ghép từ hai loại đó | Luật 1 |
| 2 | Kho trống, quầy chờ có 1 đĩa | Mở quán | Khách gọi món của đĩa đó | Luật 1 |
| 3 | Không làm được món nào | Mở quán | Khách gọi một món đã mở khóa trong sách | Luật 1 |
| 4 | Nhiều món làm được | Chọn món 30.000 lần | Mỗi món được chọn đều (lệch ≤ 6%) | Luật 1; Điều 3 |
| 5 | Khách đang chờ | Bấm chuông với đĩa đúng / sai món | Đúng: tiền theo bảng; sai: −18G, đĩa mất, khách kế tiếp vào | Luật 3 |
| 6 | Mỗi khách trong ngày | Khách vào | Có món gọi, món đó làm được | Luật 1 |
| 7 | Góc 1, có khách | Nhìn bong bóng | Thấy "Khách n/6 · Dễ tính N%" và "Gọi: <tên món>" | Luật 2 |
| 8 | Cầm đĩa sai món | Nhìn chuông, bấm | Gợi ý "Sai món: khách gọi …"; ví −18G; thông báo bắt đầu "Sai món" | Luật 3, 4 |
| 9 | Góc 3 | Nhìn trang sách | Có "Giá bán", 12G, 30G, 60G, 108G, 192G | Luật 5 |
| 10 | Bản build | Mở `QuanAnHope.exe` | Toàn màn hình | Luật 6 |

## Mặc định cần bạn xác nhận

- **Q-20 Chủ nợ ngày 21:** không gọi món cụ thể, gọi "món ngon nhất của quán". Mô phỏng: nếu chủ nợ cũng gọi ngẫu nhiên thì người gom bộ relic chỉ thắng 73% (mốc Điều 1 là 70%); để ngoại lệ thì 82%.
- Bảng giá không ghi Huyền thoại (chỉ có một món riêng, §5.4).
- Khách gọi món ngay lúc vào; nếu sau đó bạn dùng hết nguyên liệu của món đó thì khách vẫn chờ món đó (có thể nhét vé quay thêm, hoặc đưa món khác và chịu −30% giá gốc).

## Ngoài phạm vi

- Tủ lạnh, 3 khách cùng lúc (vòng 2 trở đi), thời gian chờ — làm khi mở vòng 2.
- Hình món ăn trên bong bóng (hiện chỉ có chữ).

## Ma trận AC → test

| AC | Test | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `OrderTests.AC1_MotThungChinhMotThungPhu_KhachGoiDungMonGhepDuoc`, `AC1_NhieuThung_MonGoiLuonGhepDuocTuKho` (200 seed) | đạt |
| 2 | `OrderTests.AC2_KhoTrong_ConDiaTrenQuayCho_KhachGoiMonTrenQuay` | đạt |
| 3 | `OrderTests.AC3_KhongCoGiLamDuoc_GoiMonDaMoKhoaTrongSach` | đạt |
| 4 | `OrderTests.AC4_ChonDeuGiuaCacMonLamDuoc` | đạt |
| 5 | `OrderTests.AC5_DungMon_TraTheoBangTinhTien`, `AC5_SaiMon_KhachNhanNhungGian_QuanDen30PhanTramGiaGoc` | đạt |
| 6 | `OrderTests.AC6_MoiKhachMoiGoiMon`; bot `NaiveBot` nấu đúng món gọi, 2.000/2.000 ván qua lần trả nợ 1 | đạt |
| 7, 8 | PlayMode `ChoiTronVong1_Bang3D_QuaVong1`: `ServeOne` đọc món trên bong bóng, nấu đúng món; `ServeWrongDish` ở khách thứ 3 | đạt |
| 9 | PlayMode `La3D_DiLaiTuDo_VaChamThat_KhongNutChuyenGoc` | đạt |
| 10 | Không tự động được (batchmode không có màn hình) | CẦN NGƯỜI KIỂM |

## Cách làm

| File | Việc |
| --- | --- |
| `Core/Customers.cs` | `Customer.orderId/orderName`; `PaymentResult.wrongOrder`; `Payment.WrongOrder` |
| `Core/Cooking.cs` | `RecipeLookup.ById`; `Stove.PendingRecipe` |
| `Core/GameSession.cs` | `MakeableRecipes`, `BookRecipes`, `ChooseOrder`; khách mới gọi món; `Serve` xét đúng / sai món; `NaiveBot` nấu đúng món gọi (`CookFor`) |
| `Game/RoomView.cs` | Bong bóng ghi món gọi; gợi ý chuông; ô giá theo bậc trên trang sách |
| `Game/GameController.cs` | Thông báo sai món; ép toàn màn hình khi chạy bản build |
| `Editor/ProjectSetup.cs` | Đặt toàn màn hình + độ phân giải gốc trước khi build |
| `balance/sim_balance.py` | Mô phỏng gọi món (xem constitution §9, bản 1.3) |
