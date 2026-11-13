# F-001 — Tasks

- [x] T1 Thêm gói `com.unity.test-framework` 1.8.0 và `com.unity.nuget.newtonsoft-json` 3.2.1
- [x] T2 Tạo asmdef Core (không tham chiếu engine), Data, Editor, Tests.EditMode
- [x] T3 Lớp dữ liệu `Balance`, `RecipeBook`; `BalanceData`; importer rỗng
- [x] T4 Viết 12 test; chạy thấy đỏ 12/12 đúng lý do (importer trả dữ liệu rỗng)
- [x] T5 Viết `ParseBalance`, `ParseRecipes`, `ImportTo`, menu
- [x] T6 Sinh `BalanceData.asset` bằng dòng lệnh; chạy lại test: xanh 12/12
- [x] T7 Commit và đẩy lên GitHub

Lệnh:

```
# test (đóng Unity Editor trước khi chạy)
"D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<đường dẫn tuyệt đối>\TestResults.xml" -logFile "<đường dẫn tuyệt đối>\test.log"

# nhập dữ liệu + dựng lại cảnh + chụp ảnh 4 góc
"D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -executeMethod BepNhanPham.PreviewCapture.RebuildAll -quit -logFile "<đường dẫn tuyệt đối>\build.log"
```
