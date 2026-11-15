# F-012 — Hình nguyên liệu

Trạng thái: DONE, chờ bạn duyệt bằng mắt (AC 5) · Nguồn: ghi chú 14/11 "đỡ xấu và nhìn chân thực hơn"; Art Bible §5, §7; constitution 1.4 §4.5 (kiểu sơ chế)

## Art Bible → làm ở đâu

| Mục | Làm ở |
| --- | --- |
| §5 hình khối tròn, viền đạo cụ nhỏ 0,018 m | `Game/IngredientModels.cs` (viền 0,008 ở tỉ lệ 2,2 ≈ 0,018 m; chi tiết nhỏ 0,004) |
| §7 một hình dáng cho mọi bậc, đế bậc tô màu bậc, mặt theo bậc | `ItemView` trong `Game/RoomView.cs` |
| §7 "đã sơ chế: thân dẹt xuống" | đổi thành "đổi hình theo kiểu sơ chế" trong code; **văn bản Art Bible chưa sửa** (cần bạn duyệt rồi mới ghi Art Bible 1.4) |

## Hình

- Vòng 1 (≥ 3 khối): cà chua (quả + đốm sáng + cuống + 5 lá đài) → 5 múi; hành tây (củ + chóp + 3 vân dọc + rễ) → 3 khoanh có vòng trắng; rau muống (3 cọng + 5 lá + dây buộc) → khúc cọng + lá; trứng (hình trứng từ 2 khối + đốm) → bát trứng đánh có đũa; thịt heo (nạc + viền mỡ + bì + vân mỡ) → 4 lát xếp lợp; đậu phụ (khối bo góc) → 4 miếng.
- 16 loại còn lại: ≥ 2 khối, đúng màu, có hình sơ chế riêng.
- Mặt đặt ngoài bán kính bao của cả hình (không lọt vào khối nào).

## Ma trận AC → test

| AC | Test | Kết quả 15/11 |
| --- | --- | --- |
| 1 | `PlaythroughTests.F012_HinhNguyenLieu_AnhCan`: 6 loại vòng 1 ≥ 3 khối | xanh |
| 2 | như trên: tập khối khi sơ chế khác khi còn nguyên (cả 22 loại); `ShowsPrepared` | xanh |
| 3 | như trên: 16 loại còn lại ≥ 2 khối | xanh |
| 4 | như trên: màu đế bậc, `Mat_NguyenLieu_<bậc>`, `AssertFaceOutside`; giả lập chơi 3 ngày (cầm, thớt, thùng) | xanh |
| 5 | 22 ảnh cận `docs/sprint-2/ui/nguyen_lieu/<id>.png` (trái: nguyên, phải: đã sơ chế) | **CẦN NGƯỜI KIỂM** |
