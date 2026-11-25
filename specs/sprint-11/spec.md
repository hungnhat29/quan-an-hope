# Spec — Sprint 11: Đệm, tiếng Anh, release candidate

Ngày: 25/11/2026 · Constitution 2.3 · Kế hoạch: `docs/sprint-11/impl-plan.md`

Nguồn yêu cầu (tin nhắn người dùng 25/11/2026, nguyên văn):
> "chuẩn bị thêm 1 ngôn ngữ Tiếng Anh" · "sử dụng @specs/ và @.dev-memory/ mới nhất để thực hiện @docs/sprint-11/impl-plan.md" · "sau khi chạy xong sprint 11 là sprint cuối thì hãy: Viết cho tôi 1 báo cáo tổng thể về dự án này và dự đoán kết quả sau khi public; Viết deploy plan lên Steam; Tạo 1 survey để tôi đi khảo sát những người chơi thử để đánh giá độ hay của game"

Ghi chú lịch: impl plan ghi 17–23/1/2027; người dùng yêu cầu làm ngay 25/11/2026. Dịch tiếng Anh nằm trong mục 6 "Sau sprint 11" của impl plan, nhưng tin nhắn của người dùng thắng (yêu cầu trực tiếp, mới hơn).

## 1. Xếp việc theo impl plan §2 (ngày 1)

| Nhóm | Nguồn đã rà | Kết quả |
| --- | --- | --- |
| A — Chặn | `docs/KNOWN-ISSUES.md` K-1..K-12; acceptance 23–24/11 | **Không có** lỗi chặn đã biết. Kiểm lại bằng test đầy đủ + bot 600 ván trong bản build (tiêu chí 2, 3) |
| B — Hiểu sai | `docs/sprint-10/playtest-summary.md` | **Chưa có file**: chưa có buổi chơi thử người ngoài (F-094, Q-56). Không có việc; khảo sát (F-115) để thu dữ liệu này |
| C — Cân bằng | Điều 1 (bot 2.1: 90% / 89% / 31%), STATE 24/11 (90% / 91% / 32%) | Bot đạt ba mốc; **không có số đo người thật** → không chỉnh số (impl plan §5: chỉ chỉnh khi có số đo) |
| D — Việc trễ | KNOWN-ISSUES K-8, K-12 ("ở sprint 11"); specs/sprint-9/spec.md:82 "16 nguyên liệu còn lại bằng Blender (sprint 11)"; acceptance 23/11 mục "đề xuất sprint 11" | F-111 (K-8), F-112 (K-12), F-113 (16 nguyên liệu, nếu còn thời gian) |
| Yêu cầu mới | Tin nhắn 25/11/2026 | F-110 tiếng Anh, F-115 báo cáo / Steam / khảo sát |
| Phát hành | impl plan §3 (8 tiêu chí), §4 ngày 7 | F-114 |

## 2. Tính năng

### F-110 — Tiếng Anh (nguồn: tin nhắn 25/11/2026)

Cách làm: dịch ở lớp hiển thị. Mọi chữ trong game đi qua TextMeshPro tạo ở `UiKit.Text` hoặc `ComicBook` (Điều 6); gắn `textPreprocessor` để chữ **hiển thị** được dịch, còn `.text` (code, dữ liệu, test) giữ tiếng Việt. Không đổi dữ liệu cân bằng, không đổi luật (Điều 2).

Kịch bản 10 giây (L-014): mở game → menu chính → Cài đặt → dòng **"Ngôn ngữ / Language"** → bấm nút "Tiếng Việt" → nút thành "English", mọi chữ trên màn hình (tiêu đề, nhãn, nút, cột Điều khiển) đổi sang tiếng Anh ngay → Xong → Chơi mới: truyện, thẻ hướng dẫn, chữ trên đồ vật, HUD, Tab đều tiếng Anh.

- AC-1 (cài đặt): Cài đặt có dòng "Ngôn ngữ / Language", nút vòng qua Tiếng Việt → English; lưu trong `GameSettings` (PlayerPrefs) qua các lần mở. *Test:* `LanguageTests.AC1_*` (EditMode), `LanguagePlayTests.AC1_*`.
- AC-2 (bộ dịch): chuỗi có trong bảng → bản dịch; mẫu có lỗ (`"Đã mua {0} · còn {1} vé."`) → dịch cả câu, chữ trong lỗ dịch tiếp (tên món, tên relic); câu nhiều dòng dịch từng dòng; phần còn sót thay theo mảnh (đúng ranh giới từ); số kiểu Anh (`32.000G` → `32,000G`, `0,3%` → `0.3%`). Tiếng Việt: trả lại nguyên văn. *Test:* `LanguageTests.AC2_*`.
- AC-3 (phủ bảng): mọi chuỗi `tools/i18n/extract.py` gom được (code Core + Game, chữ trên 6 trang truyện, tên trong `balance/*.json`) có bản dịch; tên 84 món sinh từ mẫu món chính + tên đồ phụ. `tools/i18n/build.py` báo **0 thiếu**. *Test:* `LanguageTests.AC3_*` (đọc bảng trong Resources) + lệnh build.py.
- AC-4 (không sót trên màn hình): ở tiếng Anh, giả lập người chơi qua menu, Cài đặt, truyện chương 1, ngày 1 (quay, sơ chế, nấu, mở quán, phục vụ, đóng quán), Tab 4 thẻ: **mọi chữ TMP đang hiện không còn chữ Việt có dấu**. *Test:* `LanguagePlayTests.AC4_*` (ghi các chuỗi sót ra file, phải rỗng).
- AC-5 (hình, L-005): ảnh `docs/sprint-11/ui/en_*.png` (menu, cài đặt, truyện, HUD, thẻ tranh, Tab thực đơn / cửa hàng, cuối ngày); tự xem: chữ không tràn khung. *Kiểm:* ảnh + **CẦN NGƯỜI KIỂM** (người đọc tiếng Anh).
- AC-6 (mặc định): lần đầu mở (chưa lưu cài đặt): ngôn ngữ hệ điều hành là tiếng Việt → Tiếng Việt, khác → English; Unity Editor, batchmode, test → Tiếng Việt (người làm game dùng tiếng Việt). *Test:* `LanguageTests.AC6_*`.
- AC-7 (không đổi luật): EditMode + PlayMode cũ xanh; bot không đổi (không đổi dữ liệu). *Test:* toàn bộ suite.

Ngoài phạm vi: tên sản phẩm trên cửa sổ / thư mục lưu (Q-46, Q-70 chưa chốt tên); giọng khách; log gỡ lỗi; menu Editor; tên đồ vật trong Hierarchy. Bản dịch do mình viết: cần người bản ngữ đọc lại (CẦN NGƯỜI KIỂM).

### F-111 — Tên món dài trong Thực đơn (nguồn: KNOWN-ISSUES K-8 "Nới cột tên … ở sprint 11")

- AC-1: thẻ món trong Tab → Thực đơn: tên món dài nhất (tiếng Việt và tiếng Anh) nằm gọn trong ô tên, không chồng lên cột giá. *Test:* `LanguagePlayTests.F111_*` đo khung chữ thật (L-016).
- Nhãn bậc xuống hai dòng: đã sửa ở 2.2 (specs/sprint-10c AC-1) → chỉ kiểm lại bằng ảnh.

### F-112 — Khựng khung do giọng khách (nguồn: KNOWN-ISSUES K-12 "đặt mp3 Decompress On Load … ở sprint 11")

- Rà 25/11: 3 file `Resources/Audio/khach_*.mp3` **đã** là Decompress On Load (meta `loadType: 0`), nhưng `preloadAudioData: 0`, `loadInBackground: 0` (giải nén trên luồng chính khi `GameAudio.Preload` gọi ở menu).
- AC-1: 3 file `khach_*.mp3`: Decompress On Load + Preload Audio Data + Load In Background. *Test:* `LanguageTests.F112_*` (AudioImporter).
- AC-2: benchmark bản build cuối: số khung > 50 ms ghi vào KNOWN-ISSUES (mục tiêu ≤ 2 như 0.10.0; không đạt thì ghi rõ). *Kiểm:* `QuanAnHope.exe -benchmark -out <dir>`.

### F-113 — 16 nguyên liệu vòng 2–4 bằng Blender (nguồn: specs/sprint-9/spec.md:82; impl plan §2 nhóm D ví dụ "Model nguyên liệu vòng 2–4")

Chỉ làm nếu F-110, F-114 xong mà còn thời gian (impl plan §4 ngày 5 "trong thời gian còn lại"). Không làm → ghi trong KNOWN-ISSUES và báo cáo cuối.

### F-114 — Release candidate `rc-1` (nguồn: impl plan §3, §4 ngày 6–7)

- AC-1: số phiên bản **0.11.0** (Branding.Version, menu chính). *Test:* `LanguageTests.F114_*`.
- AC-2: `docs/sprint-11/HUONG-DAN.txt` có phần tiếng Việt + tiếng Anh, đúng phím và số hiện tại; `docs/CREDITS.md` cập nhật; `docs/KNOWN-ISSUES.md` cập nhật cho 0.11.0.
- AC-3: 8 tiêu chí impl plan §3 có bảng kết quả trong acceptance (đạt / chưa đạt / CẦN NGƯỜI KIỂM).
- AC-4: build Windows, gói `QuanAnHope-0.11.0.zip`, bot 600 ván trong build 0 ngoại lệ, benchmark.
- AC-5: commit + thẻ `rc-1`, đẩy lên GitHub (tiêu chí 8 của impl plan).

### F-115 — Tài liệu sau sprint cuối (nguồn: tin nhắn 25/11/2026)

- AC-1: `docs/release/BAO-CAO-TONG-THE.md`: 10 tuần đã làm gì, số liệu thật (test, bot, nội dung), còn nợ gì, dự đoán kết quả sau khi public (có giả định và khoảng, không bịa số thị trường: số nào lấy từ nguồn ngoài thì ghi nguồn, số nào ước thì ghi là ước).
- AC-2: `docs/release/STEAM-DEPLOY-PLAN.md`: các bước từ Steamworks tới phát hành, mốc thời gian, việc trong repo (build, depot, cửa hàng, giá, Next Fest), checklist.
- AC-3: khảo sát người chơi thử: bộ câu hỏi `docs/release/KHAO-SAT.md` + trang khảo sát dùng được ngay (thu câu trả lời).

## 3. Thay đổi constitution (2.2 → 2.3)
- Mục 2.1 Cài đặt: thêm **Ngôn ngữ: Tiếng Việt / English** (mặc định theo ngôn ngữ máy). Điều 6: chữ vẫn là TextMeshPro; bản dịch là dữ liệu (`Resources/Lang/en.json`, sinh từ `tools/i18n`).
- Mục 9: dòng thay đổi 2.3.
