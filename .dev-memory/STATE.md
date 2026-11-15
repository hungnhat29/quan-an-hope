# STATE
Cập nhật: 2026-11-16 (sprint 3)
Ngày làm việc: 2026-11-16
Pha: SPRINT 3 ĐÃ ĐÓNG (16/11/2026), đẩy GitHub với thẻ sprint-3. Tổng kết: .dev-memory/RETRO-sprint-3.md
Task: F-020 DONE, F-021 DONE, F-022 DONE (đã commit)
Kiểm: EditMode 179/179, PlayMode 7/7, bot C# có mua relic 96,75% (mô phỏng 96%), mô phỏng 63/90/46% không đổi, build Windows chạy 15 giây không lỗi
Báo cáo: .dev-memory/acceptance/2026-11-16-sprint-3.md · Spec: specs/sprint-3/F-02x-*/spec.md · Ảnh: docs/sprint-3/ui/
Bước tiếp theo: trả lời Q-28..Q-31; duyệt ảnh sprint 2 + chơi thử F-014 (Q-24); rồi sprint 4 (docs/sprint-4/impl-plan.md)
Chờ người dùng: bắt đầu sprint 4
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml
Nhập dữ liệu: ... -executeMethod QuanAnHope.EditorTools.BalanceImporter.Import -quit
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py (số game đang chạy = 1.4) · sinh lại số: python balance/make_next.py rồi chép balance/next/*.json đè balance/ · thử nhanh: DEBTS="a,b,c,d" TICKETS=n HOUR_S=n OVER_GOOD=n OVER_NAIVE=n N=1500 MORE=1
Giả lập người chơi: ... -runTests -testPlatform PlayMode (ảnh ra docs/sprint-2/ui/ và docs/sprint-3/ui/; xong thì git checkout -- docs/sprint-2/ui nếu không chủ ý đổi ảnh cũ, L-018)
Gỡ lỗi relic: Play trong Editor → menu Quán Ăn Hope > Gỡ lỗi: cho relic
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe
Log chơi thử: %USERPROFILE%\AppData\LocalLow\DefaultCompany\My project\playtest-log.csv
