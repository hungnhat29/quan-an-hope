# STATE
Cập nhật: 2026-11-21 (sprint 7 ĐÓNG, đã commit + push)
Ngày làm việc: 2026-11-21
Pha: SPRINT 7 ĐÓNG (người dùng yêu cầu 21/11), tổng kết .dev-memory/RETRO-sprint-7.md
Bước tiếp theo: sprint 8 (docs/sprint-8/impl-plan.md); người dùng trả lời Q-53..Q-57, chơi thử bản build, bài Blender
Task: 3 sửa trước sprint (tay khách cầm đũa, ẩn bộ/lẻ relic, ngày 1 5 vé + đồng hồ từ lúc mở quán) + F-060..F-063 DONE; F-060 AC-1 và F-063 CẦN NGƯỜI KIỂM
Constitution 1.7 · Spec: specs/sprint-7/spec.md · Ảnh: docs/sprint-7/ui/ · Bot: balance/bot_report.md, balance/bot_sweep.md
Số mới: start.first_day_tickets 5, tickets_per_day 7, nợ 22 36.000G, bot_timing (good 19 s / naive 30 s)
Bot 4.000 ván: naive chặng 75% / gom bộ (đủ 3 món) thắng 86% / không gom bộ 37%; khách bị giảm dễ tính / ngày vòng 2-4: 2,6 (gom bộ)
Kiểm: EditMode 296/296 (+2 Explicit), PlayMode 37/37, build Windows chạy 15 giây không lỗi
Âm thanh: tools/am-thanh/make_audio.py -> Resources/Audio (27 file); GameAudio.cs + GameControllerAudio.cs
Quét phương án cân bằng: powershell -NoProfile -ExecutionPolicy Bypass -File balance/run_sweep.ps1 -Sweep "ten|d22=36000|tickets=7;..." [-Games 1000]
Chờ người dùng: Q-20, Q-22, Q-39..Q-47, Q-49..Q-57
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
