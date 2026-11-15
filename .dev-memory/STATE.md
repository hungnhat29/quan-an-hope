# STATE
Cập nhật: 2026-11-16
Ngày làm việc: 2026-11-15
Pha: SPRINT 2 ĐÃ ĐÓNG (16/11/2026), đẩy GitHub với thẻ sprint-2. Tổng kết: .dev-memory/RETRO-sprint-2.md
Task: F-010 DONE, F-011 DONE, F-012 DONE (chờ duyệt ảnh), F-013 DONE (chờ duyệt ảnh), F-014 AC1 DONE / AC2 chờ buổi chơi thật
Kiểm: EditMode 142/142, PlayMode 5/5, mô phỏng balance/ = V=next (63/46/89%, naive qua nợ 1 96%), bot C# 99,85%, build Windows chạy 15 giây không lỗi
Báo cáo: .dev-memory/acceptance/2026-11-15-sprint-2.md · Spec: specs/sprint-2/F-01x-*/spec.md
Bước tiếp theo: người dùng duyệt ảnh docs/sprint-2/ui/ + chơi thử bản build → chạy balance/playtest_overhead.py; trả lời Q-21, Q-22 (đã làm theo mặc định), Q-25, Q-26, Q-27; rồi sprint 3 (docs/sprint-3/impl-plan.md)
Chờ người dùng: duyệt ảnh + chơi thử (chuyển sang sprint 3)
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml
Nhập dữ liệu: ... -executeMethod QuanAnHope.EditorTools.BalanceImporter.Import -quit
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py (số game đang chạy = 1.4) · sinh lại số: python balance/make_next.py rồi chép balance/next/*.json đè balance/ · thử nhanh: DEBTS="a,b,c,d" TICKETS=n HOUR_S=n OVER_GOOD=n OVER_NAIVE=n N=1500 MORE=1
Giả lập người chơi: ... -runTests -testPlatform PlayMode (ảnh chụp ra docs/sprint-2/ui/)
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe
Log chơi thử: %USERPROFILE%\AppData\LocalLow\DefaultCompany\My project\playtest-log.csv
