# F-001 — Plan

## Thiết kế

```
balance/balance.json ─┐
balance/recipes.json ─┴─> BalanceImporter (Editor, Newtonsoft.Json)
                              ├─ ParseBalance(json)  -> Core.Balance
                              ├─ ParseRecipes(json)  -> Core.RecipeBook
                              └─ ImportTo(assetPath) -> Data.BalanceData.asset
```

| File | Vai trò |
| --- | --- |
| `Core/Balance.cs` | Lớp dữ liệu thuần (`[Serializable]`), không tham chiếu UnityEngine |
| `Core/RecipeBook.cs` | Nguyên liệu, công thức, thời gian nấu |
| `Data/BalanceData.cs` | ScriptableObject bọc `Balance` + `RecipeBook` |
| `Editor/BalanceImporter.cs` | Đọc JSON, ghi asset, menu `Quán Ăn Hope > Nhập balance.json` |
| `Tests/EditMode/BalanceDataTests.cs` | 12 test cho AC 1–3 |

## Quyết định

- **Core dùng mảng thay cho từ điển** (`debts[]`, `rounds[]`, `basePriceByRound[]`) vì Unity không serialize được `Dictionary` và mảng lồng nhau. Chỉ số 0 = vòng 1.
- **Thiếu trường trong JSON thì báo lỗi kèm tên trường**, không âm thầm dùng 0: một con số cân bằng bằng 0 do gõ sai tên khóa sẽ rất khó phát hiện.
- **AC-2 so asset đã commit với JSON** bằng cách serialize cả hai rồi so chuỗi, nên thêm trường mới vào `Balance` không cần sửa test.
- **Test AC-1 ghi vào asset tạm** (`__BalanceData_Test.asset`) rồi xóa, không đụng asset thật.
- `mainId` và `sideId` của công thức tách từ `id` dạng `<chính>__<phụ>`; id sai dạng thì báo lỗi.

## Rủi ro

| Rủi ro | Xử lý |
| --- | --- |
| Sửa JSON mà quên nhập lại | AC-2 đỏ, thông báo nêu rõ cách sửa |
| Đổi tên khóa trong JSON | Importer báo lỗi tên trường bị thiếu |
