# STATE
Cập nhật: 2026-11-22 (sprint 8 ACCEPTED, đã commit + push)
Ngày làm việc: 2026-11-21/22
Pha: sprint 8 ACCEPTED 22/11 (người dùng: "end sprint 8") — báo cáo .dev-memory/acceptance/2026-11-21-sprint-8.md
Bước tiếp theo: Q-58..Q-64 còn mở; 5 AC CẦN NGƯỜI KIỂM chưa có phản hồi riêng; sprint 9 (nhân vật Blender, 16 nguyên liệu còn lại)
Constitution 1.8 · Spec: specs/sprint-8/spec.md · Ảnh: docs/sprint-8/ui/ · Blender: SourceArt/Blender (README)
Kiểm: EditMode 317/317 (+2 Explicit), PlayMode 49/49, build Windows 15 giây không lỗi; bot 4.000 ván 80% / 87% / 34%, khách giảm dễ tính / ngày 1,3
Blender: "D:\Blender\blender.exe" -b --factory-startup -P SourceArt/Blender/scripts/build_models.py (rồi render_previews.py)
Ảnh trước/sau: python tools/so_sanh_truoc_sau.py (sau PlayMode, rồi git checkout -- docs/sprint-2..7/ui theo L-018)
Quét bot sprint 8 thêm khoá: gapk, gapmin, safe, s2/s3/s4
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml (PowerShell: gọi bằng & , không Start-Process vì đường dẫn có dấu cách)
Bot 3 kiểu (F-045): powershell -NoProfile -ExecutionPolicy Bypass -File balance/run_bots.ps1 [-Games n] → balance/bot_report.md, balance/sim_report.txt
Nhập dữ liệu: ... -executeMethod QuanAnHope.EditorTools.BalanceImporter.Import -quit
Dựng lại cảnh (sau khi sửa BuildNhaHang): ... -executeMethod BepNhanPham.PreviewCapture.RebuildAll -quit (nhập dữ liệu + dựng cảnh + chụp docs/preview)
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py (số game đang chạy = 1.4 + eat_s 1.5) · sinh lại số: python balance/make_next.py rồi chép balance/next/*.json đè balance/
Giả lập người chơi: ... -runTests -testPlatform PlayMode (ảnh ra docs/sprint-2..6/ui; xong thì git checkout -- docs/sprint-2/ui docs/sprint-3/ui docs/sprint-4/ui docs/sprint-5/ui nếu không chủ ý đổi ảnh cũ, L-018). Test sprint 6 dùng bản lưu ở %TEMP%/QuanAnHope_TestSave và PlayerPrefs khoá "Test."
Gỡ lỗi: Play trong Editor → menu chính → Chơi mới (giữ chuột để bỏ qua truyện) → menu Quán Ăn Hope > Gỡ lỗi: cho relic
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe (vào menu chính; chơi đủ 22 ngày, tự lưu đầu ngày)
Bản lưu người chơi: %USERPROFILE%\AppData\LocalLow\DefaultCompany\My project\save.json · Log chơi thử: ...\playtest-log.csv
Xuất lại tranh truyện: cd tools/cot-truyen && npm install && node export.js (cần Edge; xem README)
