# STATE
Cập nhật: 2026-11-14
Ngày làm việc: 2026-11-14
Pha: SPRINT 1 ĐÃ ĐÓNG (14/11/2026, người dùng yêu cầu đóng). Tổng kết: .dev-memory/RETRO-sprint-1.md
Task hôm nay: T-001 DONE (git/GitHub), T-002 DONE (F-001), T-003 DONE (cảnh theo Art Bible), T-004 DONE (F-002 logic), T-005 DONE (F-003 logic), T-006 DONE (F-004 AC1-7), T-007 DONE (F-005 + nút Mở quán), T-008..T-010 DONE (UI 4 góc, màn kết thúc, build Windows). 120 test EditMode + 3 PlayMode xanh
Bước tiếp theo: xử lý phản hồi chơi thử bản 3D; sau đó Sprint 2 = Relic (docs/ROADMAP.md), làm theo constitution v1.2 + Art Bible v1.3 (đồ vật 3D, thông tin trên đồ vật, mặt theo bậc). Chưa có impl plan sprint 2.
Kế hoạch: docs/ROADMAP.md, docs/sprint-1/impl-plan.md
Nguồn luật: specs/constitution.md v1.3 (§6.7 gọi món); số: balance/balance.json + recipes.json; hình: docs/art-bible/ v1.3
Chờ người dùng: Q-9, Q-13, Q-17, Q-18, Q-19, Q-20 (mang sang sprint 2). F-007, F-008, F-009 đóng cùng sprint 1; điều chưa hài lòng nằm ở INBOX.md
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml
Dựng lại cảnh + nhập dữ liệu + chụp ảnh: ... -executeMethod BepNhanPham.PreviewCapture.RebuildAll -quit
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py (ORDERS=0: luật cũ không gọi món; BOSS_ANY=0: chủ nợ cũng gọi món)
Giả lập người chơi: ... -runTests -testPlatform PlayMode -testResults "<abs>\PlayResults.xml" (ảnh chụp ra docs/sprint-1/ui/)
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe
