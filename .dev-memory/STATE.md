# STATE
Cập nhật: 2026-11-21 (sprint 6 đóng, đã commit + push)
Ngày làm việc: 2026-11-21
Pha: SPRINT 6 ĐÓNG (người dùng yêu cầu kết thúc 21/11), đã commit + push main
Task: sửa trước sprint (phòng 9,6 m; khách ăn 5 giây) + F-050..F-055 DONE; ngày 7 "người mới chơi 15 phút" chưa làm (cần người thật)
Thêm 19/11: trời đổi màu theo giờ (SkyCycle); cốt truyện vẽ lại: dải truyện ngang, bối cảnh nhiều lớp có thị sai, nhân vật SDF (InkShape/InkCanvas/Puppet/StoryStage/StoryComic), cài PrimeTween 1.5.3
Thêm 20/11: cốt truyện theo CotTruyen_BepNhanPham.zip (ComicBook, tools/cot-truyen, 6 chương, constitution 1.6)
Thêm 20/11: khách + nguyên liệu chuyển động mượt (CharacterMotion.cs)
Kiểm: EditMode 286/286 (+1 Explicit), PlayMode 29/29, mô phỏng 63/89/46% không đổi, bot C# 4.000 ván: naive 0% (chặng 63%) / nocombo 9% / smart 52%, build Windows chạy 15 giây không lỗi (vào menu)
Báo cáo: .dev-memory/acceptance/2026-11-18-sprint-6.md · Spec: specs/sprint-6/spec.md · Constitution 1.6 · Ảnh: docs/sprint-6/ui/
Bước tiếp theo: sprint 7 (docs/sprint-7/impl-plan.md); người dùng trả lời Q-45 (luật ăn + cân bằng), Q-46 (tên game); Q-48 đã đóng, Art Bible §12 còn ghi "trên 2"
Chờ người dùng: Q-20, Q-39..Q-47, Q-49..Q-52
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
