# Plan và tasks — F-002, F-003, F-004 (phần logic)

Một file chung cho ba tính năng vì chúng dùng chung thiết kế Core. Spec riêng ở từng thư mục `F-00x-*/spec.md`.

## Thiết kế

Tất cả nằm trong `My project/Assets/QuanAnHope/Core/` (không tham chiếu UnityEngine).

| File | Lớp | Việc |
| --- | --- | --- |
| `Randomness.cs` | `IRandom`, `SeededRandom`, `TierRoller` | Nguồn ngẫu nhiên thay được; quay bậc theo bảng phần trăm |
| `BalanceRules.cs` | `BalanceRules` | Tra cứu trên `Balance`: vòng của ngày, bảng tỉ lệ của ngày, kích thước thùng, giá gốc, số khách, món tối thiểu |
| `Ingredients.cs` | `Ingredient`, `Crate`, `TicketWallet`, `IngredientGacha`, `IngredientStock` | Vé, thả thùng, kho trong ngày |
| `Cooking.cs` | `DishFormula`, `RecipeLookup`, `PrepStation`, `Stove`, `Counter`, `Dish` | Công thức bậc món, thớt, bếp, quầy chờ |
| `Customers.cs` | `CustomerGenerator`, `Payment` | Sinh khách, tính tiền |

## Quyết định

- **Ngẫu nhiên đi qua `IRandom` chỉ có `NextDouble()`.** Test thống kê dùng `SeededRandom` (seed cố định, chạy lại ra cùng kết quả); test từng trường hợp dùng `FixedRandom` trả đúng các số cho trước.
- **Thời gian đi vào bằng `Tick(giây)` / `Hold(giây)`.** Core không đọc đồng hồ, nên test không phải chờ.
- **Bếp so tên.** Thứ tự trong `recipes.json` là tên tiếng Việt (đồ kho lẫn nguyên liệu); bếp ghi lại tên từng thứ được cho vào rồi so nguyên dãy với công thức. Thiếu hoặc thừa một thứ cũng là sai.
- **Điểm món làm tròn 4 chữ số** trước khi tách phần nguyên và phần lẻ, để `0,6 × 4 + 0,4 × 2 + 0,8` ra đúng 4,0 chứ không phải 3,9999998.
- **Không có con số cân bằng trong code:** bốn số còn thiếu được thêm vào `balance.json` (xem mục "Bổ sung dữ liệu" trong từng spec).

## Tasks

- [x] Thêm `storm_day`, `counter_slots`, `dish.max_tier`, `payment` vào `balance.json`; chạy lại mô phỏng (không đổi)
- [x] Mở rộng `Balance`, `BalanceImporter`, test F-001 cho các trường mới; nhập lại asset
- [x] Viết 95 test mới cho F-002, F-003, F-004; chạy thấy đỏ (không biên dịch được vì chưa có lớp)
- [x] Viết 5 file Core; chạy lại: 108/108 xanh
- [x] Kiểm test có bắt lỗi: cố ý làm sai 3 chỗ (bỏ kiểm thứ tự, bỏ phần tip theo chênh, bỏ thùng nhỏ ngày mưa) → 8 test đỏ; khôi phục → xanh
- [x] Commit và đẩy lên GitHub

## Việc để lại

- `sim_balance.py` vẫn viết sẵn ngày 3, hệ số tip, hệ số đền bù thay vì đọc các khóa mới trong `balance.json`.
- `GameSession`, nút Mở quán (F-004 AC-9): ngày 4.
