# STATE
Cập nhật: 2026-11-14
Ngày làm việc: 2026-11-14
Pha: P8 — Sprint 1 (ngày 1–7) xong, chờ người dùng chơi thử và nghiệm thu
Task hôm nay: T-001 DONE (git/GitHub), T-002 DONE (F-001), T-003 DONE (cảnh theo Art Bible), T-004 DONE (F-002 logic), T-005 DONE (F-003 logic), T-006 DONE (F-004 AC1-7), T-007 DONE (F-005 + nút Mở quán), T-008..T-010 DONE (UI 4 góc, màn kết thúc, build Windows). 120 test EditMode + 3 PlayMode xanh
Bước tiếp theo: xử lý phản hồi chơi thử của người dùng; sau đó Sprint 2 = Relic (docs/ROADMAP.md). Chưa có impl plan sprint 2.
Kế hoạch: docs/ROADMAP.md, docs/sprint-1/impl-plan.md
Nguồn luật: specs/constitution.md v1.1; số: balance/balance.json + recipes.json; hình: docs/art-bible/
Chờ người dùng: chơi thử + nghiệm thu sprint 1 (.dev-memory/acceptance/2026-11-14-sprint-1.md); Q-9..Q-16
Repo: https://github.com/hungnhat29/quan-an-hope (private), nhánh main
Lệnh test (đóng Unity Editor trước): "D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults "<abs>\TestResults.xml" -logFile "<abs>\test.log"  → đọc TestResults.xml
Dựng lại cảnh + nhập dữ liệu + chụp ảnh: ... -executeMethod BepNhanPham.PreviewCapture.RebuildAll -quit
Mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py
Giả lập người chơi: ... -runTests -testPlatform PlayMode -testResults "<abs>\PlayResults.xml" (ảnh chụp ra docs/sprint-1/ui/)
Build Windows: ... -executeMethod QuanAnHope.EditorTools.ProjectSetup.BuildWindows → My project/Builds/Windows/QuanAnHope.exe
