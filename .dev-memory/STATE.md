# STATE
Cập nhật: 2026-11-16 (sprint 4)
Ngày làm việc: 2026-11-16
Pha: SPRINT 4 ACCEPTED (16/11/2026), đã commit + push.
Task: F-030..F-036 ACCEPTED
Kiểm: EditMode 227/227, PlayMode 11/11, mô phỏng 63/90/46% không đổi, bot C# qua nợ 2 = 84,15% (mô phỏng 79%), build Windows chạy 15 giây không lỗi
Báo cáo: .dev-memory/acceptance/2026-11-16-sprint-4.md · Spec: specs/sprint-4/spec.md · Ảnh: docs/sprint-4/ui/
Bước tiếp theo: trả lời Q-32..Q-39; rồi sprint 5 (docs/sprint-5/impl-plan.md)
Chờ người dùng: trả lời Q-32..Q-39 (không chặn sprint 5)
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml
Nhập dữ liệu: ... -executeMethod QuanAnHope.EditorTools.BalanceImporter.Import -quit
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py (số game đang chạy = 1.4) · sinh lại số: python balance/make_next.py rồi chép balance/next/*.json đè balance/ · thử nhanh: DEBTS="a,b,c,d" TICKETS=n HOUR_S=n OVER_GOOD=n OVER_NAIVE=n N=1500 MORE=1
Giả lập người chơi: ... -runTests -testPlatform PlayMode (ảnh ra docs/sprint-2/ui/ và docs/sprint-3/ui/; xong thì git checkout -- docs/sprint-2/ui nếu không chủ ý đổi ảnh cũ, L-018)
Gỡ lỗi relic / nhảy ngày 4, 8: Play trong Editor → menu Quán Ăn Hope > Gỡ lỗi: cho relic
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe
Log chơi thử: %USERPROFILE%\AppData\LocalLow\DefaultCompany\My project\playtest-log.csv
