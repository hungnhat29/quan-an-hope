# STATE
Cập nhật: 2026-11-18 (sprint 5 ACCEPTED)
Ngày làm việc: 2026-11-16
Pha: SPRINT 5 ACCEPTED (18/11/2026), đã commit + push lên main
Task: F-040..F-045 DONE (F-040 AC-3/AC-4 và kịch bản 10 giây có phần CẦN NGƯỜI KIỂM về cảm giác hình)
Kiểm: EditMode 262/262 (+2 Explicit), PlayMode 16/16, mô phỏng 63/89/46% không đổi, bot C# 4.000 ván × 3 kiểu (43 giây): naive 0,9% / nocombo 12% / smart 60% thắng (mô phỏng 0 / 46 / 89), build Windows chạy 15 giây không lỗi
Báo cáo: .dev-memory/acceptance/2026-11-16-sprint-5.md · Spec: specs/sprint-5/spec.md · Bot: docs/sprint-5/bot-report.md · Ảnh: docs/sprint-5/ui/
Bước tiếp theo: trả lời Q-20, Q-39, Q-40 (ảnh hưởng lớn tới cân bằng vòng 4); rồi sprint 6 (docs/sprint-6/impl-plan.md)
Chờ người dùng: Q-20, Q-39..Q-44
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml
Bot 3 kiểu (F-045): powershell -NoProfile -ExecutionPolicy Bypass -File balance/run_bots.ps1 [-Games n] → balance/bot_report.md, balance/sim_report.txt
Nhập dữ liệu: ... -executeMethod QuanAnHope.EditorTools.BalanceImporter.Import -quit
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py (số game đang chạy = 1.4) · sinh lại số: python balance/make_next.py rồi chép balance/next/*.json đè balance/
Giả lập người chơi: ... -runTests -testPlatform PlayMode (ảnh ra docs/sprint-2..5/ui; xong thì git checkout -- docs/sprint-2/ui docs/sprint-3/ui docs/sprint-4/ui nếu không chủ ý đổi ảnh cũ, L-018)
Gỡ lỗi: Play trong Editor → menu Quán Ăn Hope > Gỡ lỗi: cho relic (tới ngày 4 / 8; "Ngày 9 / 15 / 21" với tiền + relic trung vị bot gom bộ)
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe (bản Alpha: chơi đủ 22 ngày)
Log chơi thử: %USERPROFILE%\AppData\LocalLow\DefaultCompany\My project\playtest-log.csv
