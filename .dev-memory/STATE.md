# STATE
Cập nhật: 2026-11-25 (SPRINT 11 xong — release candidate rc-1, sprint cuối của khung 10 tuần)
Ngày làm việc: 2026-11-25
Pha: P8 — chờ nghiệm thu `.dev-memory/acceptance/2026-11-25-sprint-11.md`
Bước tiếp theo: bạn trả lời "accept" / "reject"; đi khảo sát người chơi thử (link khảo sát trong acceptance); chốt B1–B4 của docs/release/STEAM-DEPLOY-PLAN.md; Q-88..Q-91 mở
Constitution 2.3 · Spec: specs/sprint-11/spec.md · Ảnh: docs/sprint-11/ui/ (en_*.png)
Kiểm: EditMode 388/388 (+2 Explicit), PlayMode 88/88, bot 90% / 91% / 32%, check_constitution 115/0, build 0.11.0, bot trong build 1.800 ván 0 ngoại lệ, benchmark 255 FPS / 4 khung > 50 ms
Gói phát: My project/Builds/QuanAnHope-0.11.0.zip · Tài liệu phát hành: docs/release/ (BAO-CAO-TONG-THE.md, STEAM-DEPLOY-PLAN.md, KHAO-SAT.md, khao-sat.html)
Tiếng Anh (F-110): PYTHONUTF8=1 python tools/i18n/extract.py && PYTHONUTF8=1 python tools/i18n/build.py (bản dịch tools/i18n/en.tsv; README trong tools/i18n) · quét chữ sót: PlayMode với QAH_FORCE_EN_LOG=<file>
Tỉ lệ bậc món: PYTHONUTF8=1 python balance/cookodds.py [balance/next/balance.json] · quét hệ số giá: PRICE_X=0.9 python balance/make_next.py (mặc định 0.95)
Tiếng mới: PYTHONUTF8=1 python tools/am-thanh/make_audio.py --new · Khách Blender: blender -b --factory-startup -P SourceArt/Blender/scripts/build_characters.py -- SK_Khach
Build checks: "My project/Builds/Windows/QuanAnHope.exe" -batchmode -nographics -botrun 200 -out <dir>  ·  QuanAnHope.exe -benchmark -out <dir> (mở toàn màn hình ~40 s, KHÔNG thêm cờ -screen-*, L-011)
Constitution ↔ dữ liệu: PYTHONUTF8=1 python tools/check_constitution.py · Tổng hợp chơi thử: python tools/playtest_tong_hop.py <thư mục csv>
Bản lưu / log người chơi (từ 0.10.0): %USERPROFILE%\AppData\LocalLow\hungnhat29\Quán ăn của Hope\ (save.json, playtest-log.csv)
Blender: "D:\Blender\blender.exe" -b --factory-startup -P SourceArt/Blender/scripts/<build_models|build_dishes|build_characters>.py (rồi render_previews.py -- tiền_tố)
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
Xuất lại tranh truyện: cd tools/cot-truyen && npm install && node export.js (cần Edge; xem README)
