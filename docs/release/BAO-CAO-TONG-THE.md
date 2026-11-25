# Báo cáo tổng thể — Quán ăn của Hope (Hope's Diner)

Ngày: 25/11/2026 · Bản: **0.11.0 (release candidate rc-1)** · Người đọc: bạn (chủ dự án)

## 1. Tóm tắt

- Kế hoạch gốc là **10 tuần** (15/11/2026 → 23/01/2027, `docs/ROADMAP.md`). Thực tế **11 sprint xong trong 12 ngày** (14/11 → 25/11/2026), nhờ làm liên tục cùng Claude Code.
- Kết quả: một game 3D góc nhìn thứ nhất **chơi trọn 22 ngày**, có thắng / thua, menu, lưu game, truyện tranh 6 chương, hướng dẫn bằng tranh, âm thanh, model Blender, **hai ngôn ngữ (Tiếng Việt / English)**, bản build Windows.
- Độ khó đạt cả ba mốc Điều 1 khi đo bằng bot chạy trên luật thật (mục 5). **Chưa có số đo của người chơi ngoài**: đây là lỗ hổng lớn nhất trước khi phát hành, và là việc của khảo sát (`docs/release/KHAO-SAT.md`).
- Dự đoán sau khi lên Steam (mục 6): kịch bản cơ sở **khoảng 1.000–3.000 bản năm đầu** (doanh thu gộp khoảng 5.000–15.000 USD ở giá 4,99–7,99 USD). Con số phụ thuộc gần như hoàn toàn vào số wishlist gom được trước ngày ra mắt.

## 2. Game là gì

Hope bị đuổi việc, nhận lại quán ăn cũ kèm giấy nợ. 22 ngày, 4 lần trả nợ (750 · 2.900 · 10.500 · 36.000G, đầu ngày 4 / 9 / 15 / 22). Mỗi ngày:

1. Nhét vé (8 vé / ngày) vào máy → thùng nguyên liệu ngẫu nhiên, **bậc** theo màu đế (Ôi thiu → Hiếm).
2. Sơ chế trên thớt, chọn dụng cụ, cho vào bếp đúng thứ tự, vặn núm → món có bậc theo bảng tỉ lệ công khai.
3. Mở quán: khách gọi món cụ thể, độ dễ tính khác nhau, chờ lâu thì bỏ đi; từ vòng 2 ba khách cùng lúc.
4. Mua **relic** (24 cái, 3 bộ) bằng vé; cửa hàng vật phẩm (nước sốt thần thánh, vé đen, máy gacha đồ ăn, thớt, bếp 2, tay vạn năng).
5. Ngày 21 chủ nợ đến ăn: món Tuyệt hảo trở lên được thưởng lớn.

Điểm riêng: bối cảnh quán ăn Việt, món Việt + món "sơn hào hải vị", gacha công khai tỉ lệ (Điều 4 "minh bạch có chủ đích"), mọi thông tin nằm trên đồ vật trong phòng (Điều 9).

## 3. Đã làm gì

| Sprint | Ngày xong | Nội dung chính |
| --- | --- | --- |
| 1 | 14/11 | Lát cắt dọc vòng 1; làm lại từ 2D sang 3D góc nhìn thứ nhất theo nghiệm thu của bạn; khách gọi món |
| 2 | 16/11 | Ngày theo giờ 9:00–21:00, bộ số 1.4, hình nguyên liệu và quá trình nấu |
| 3 | 16/11 | 22 relic, cửa hàng relic, thùng đột phá |
| 4 | 16/11 | Vòng 2: 3 khách, giờ chờ, tủ lạnh, quá lửa, cửa hàng vật phẩm, mất điện |
| 5 | 18/11 | Vòng 3–4, Món Hy Vọng, chủ nợ ngày 21, thắng / thua; bot C# đo cân bằng |
| 6 | 21/11 | Menu, lưu theo ngày, hướng dẫn ngày 1, truyện tranh 6 chương, trời đổi màu |
| 7 | 21/11 | Cân bằng đo trên game, âm thanh tự tạo, hiệu ứng, biểu cảm |
| 8 | 22/11 | Model Blender đợt 1 (nguyên liệu vòng 1, đồ bếp), đèn, relic ngày 2, bảng Tab |
| 9 | 22/11 | Viền theo điểm ảnh, món ăn Blender, nhân vật có xương, khách đi bộ từ vỉa hè |
| 10 (+2 đợt sửa) | 25/11 | Hướng dẫn bằng tranh kiểu giấy, giá niêm yết 84 món, bếp 2, nước sốt, máy gacha, tay vạn năng, 5 khách đặc biệt có giọng, build 0.10.4 |
| 11 | 25/11 | **Tiếng Anh** (bộ dịch + ~1.170 chuỗi), 16 nguyên liệu vòng 2–4 bằng Blender, sửa cột giá Thực đơn (K-8), nạp nền giọng khách (K-12), bản 0.11.0, tài liệu phát hành |

Quy trình mỗi sprint: spec có AC → test đỏ → code → giả lập người chơi bằng PlayMode có chụp ảnh → tự xem ảnh → nghiệm thu. Bộ nhớ dự án ở `.dev-memory/` (17 báo cáo nghiệm thu, 87 câu hỏi đã ghi, bài học tự rút).

## 4. Số liệu dự án

| Hạng mục | Số |
| --- | --- |
| Commit trên GitHub (`hungnhat29/quan-an-hope`, riêng tư) | 16 trước sprint 11 (+1 của sprint 11) |
| Code C# | ~5.500 dòng luật (Core, không phụ thuộc Unity) · ~17.800 dòng game · ~1.100 dòng Editor · ~1.200 dòng dựng cảnh |
| Test | ~11.300 dòng; **EditMode 387** (+2 Explicit) · **PlayMode 88/88** |
| Công cụ Python | ~4.000 dòng (mô phỏng cân bằng, bot, tạo âm thanh, dựng model Blender, kiểm constitution, dịch) |
| Nội dung | 22 nguyên liệu · 84 món · 24 relic (3 bộ) · 22 ngày / 4 vòng · 6 chương truyện · 12 kiểu khách (5 đặc biệt) |
| Tài sản | 87 model FBX (nguyên liệu, món, đồ bếp, máy, nhân vật có xương) · 35 âm thanh / nhạc · 1 phông chữ (OFL) |
| Ngôn ngữ | Tiếng Việt, English (~1.170 chuỗi, 0 thiếu theo `tools/i18n/build.py`) |

## 5. Chất lượng đã kiểm

| Kiểm | Kết quả | Nguồn |
| --- | --- | --- |
| Test tự động | EditMode **388/388** xanh (+2 Explicit) · PlayMode **88/88** xanh (giả lập người chơi trên cảnh thật, có chụp ảnh) | `.dev-memory/acceptance/2026-11-25-sprint-11.md` |
| Điều 1 — không đọc chỉ số đi ≥ 60% chặng | **90%** chặng (qua lần trả nợ 1: 100%, 2: 100%, 3: 67%) | `balance/bot_report.md` (4.000 ván / kiểu) |
| Điều 1 — chơi tốt, gom bộ thắng ≥ 70% | **91%** thắng (99% ván gom đủ 3 món một bộ) | như trên |
| Điều 1 — chơi tốt không gom bộ thấp hơn rõ | **32%** thắng (thấp hơn 59 điểm) | như trên |
| Constitution ↔ dữ liệu | 115 dòng số, 0 lệch | `tools/check_constitution.py` |
| Bản build chạy bot | 1.800 ván (600 / kiểu) trong bản build 0.11.0: 0 ngoại lệ, 0 lỗi log; gom bộ thắng 545/600 | `QuanAnHope.exe -botrun` |
| Hiệu năng | 255 FPS trung bình, p99 5,3 ms, mở tới menu 3,6 giây; 4 khung > 50 ms (đều do GC, mục tiêu ≤ 2 chưa đạt — K-12) | `QuanAnHope.exe -benchmark`, máy Intel Iris Xe 1080p |
| Tiếng Anh không sót trên màn hình | Toàn bộ 88 kịch bản PlayMode chạy ép tiếng Anh: chữ sót còn 2 dạng, đã sửa; harness 0/181 câu sót | `tools/i18n/README.md` |

Chưa kiểm được (cần người): người chưa từng chơi đi hết ngày 1 mà không hỏi (tiêu chí 5); chạy trên máy thứ hai (tiêu chí 6, K-6); người bản ngữ đọc bản tiếng Anh.

## 6. Dự đoán kết quả sau khi public

### 6.1 Bối cảnh thị trường (số có nguồn)

- Steam nhận khoảng **19.000–20.000 game mới năm 2025**; gần **một nửa có dưới 10 review**, chỉ khoảng 600 game đạt 1.000 review ([GamesRadar / VG Insights](https://www.gamesradar.com/games/a-terrifying-20-282-games-were-released-on-steam-in-2025-and-just-608-managed-to-get-1-000-reviews-expert-finds-we-might-be-in-a-bit-of-an-indie-golden-age), [PC Gamer / SteamDB](https://www.pcgamer.com/gaming-industry/more-than-19-000-games-launched-on-steam-this-year-but-almost-half-have-fewer-than-10-reviews)).
- Doanh thu gộp **trung vị** một game Steam năm 2025 chỉ vài trăm USD; khoảng 40% game không đủ 100 USD (tức chưa thu hồi phí Steam Direct) ([OpenCritic](https://opencritic.com/news/22399/only-8-of-steam-games-gross-over-100-thousand-40-make-less-than-steam-listing-fees), [tổng hợp](https://www.steampageanalyzer.com/blog/median-steam-game-2026)). Các nguồn này đo khác nhau nên chỉ dùng làm khoảng.
- Wishlist → bán tuần đầu: trung vị **0,15×** (game trên 10 USD: 0,10×), đo trên game có hơn 25.000 wishlist ([GameDiscoverCo](https://newsletter.gamediscover.co/p/the-state-of-steam-wishlist-conversions)). Game nhỏ thường thấp hơn.
- Ước bán từ review: khoảng **20–60 bản / review**, trung tâm ~30 ([Game Developer](https://www.gamedeveloper.com/business/using-steam-reviews-to-estimate-sales), [Steam Page Analyzer](https://www.steampageanalyzer.com/blog/boxleiter-method-explained)).

### 6.2 Điểm mạnh và điểm yếu của game khi lên kệ

| Mạnh | Yếu |
| --- | --- |
| Vòng lặp rõ: gacha nguyên liệu → nấu → phục vụ → trả nợ; hai thể loại được ưa chuộng (nấu ăn, roguelite có relic) | Hình dựng từ khối và script: dễ thương nhưng không nổi bật trên ảnh cửa hàng |
| Bối cảnh Việt Nam hiếm trên Steam; có truyện tranh 6 chương | Nội dung một ván 2–4 giờ, chưa có chế độ chơi lại (mức khó, seed ngày) |
| Hai ngôn ngữ ngay từ đầu; độ khó đã đo bằng bot 4.000 ván | Chưa có trailer, chưa có cộng đồng, chưa có số đo người thật |
| Một người làm, chi phí thấp: hoà vốn dễ | Tên game, giọng 3 khách, khai báo AI chưa chốt (STEAM-DEPLOY-PLAN mục 1) |

### 6.3 Ba kịch bản (ước tính của mình, không phải dữ liệu)

Giả định: giá 4,99–7,99 USD, phát hành sau một kỳ Next Fest, trang Coming Soon ≥ 3 tháng, quảng bá chủ yếu ở cộng đồng game Việt và mạng xã hội. Doanh thu **gộp** chưa trừ 30% của Steam, thuế, hoàn tiền, giảm giá theo vùng (thực nhận thường còn khoảng 50–60% gộp).

| Kịch bản | Khả năng (ước) | Wishlist lúc ra mắt | Bán tuần đầu | Bán năm đầu | Gộp năm đầu | Review (năm đầu) |
| --- | --- | --- | --- | --- | --- | --- |
| Bi quan | ~50% | < 1.000 | 30–150 | 100–500 | 0,5–3 nghìn USD | < 10 |
| **Cơ sở** | ~35% | 3.000–7.000 | 300–1.000 | **1.000–3.000** | **5–15 nghìn USD** | 30–80 |
| Lạc quan | ~15% | ≥ 20.000 | 2.000–3.000 | 6.000–15.000 | 30–90 nghìn USD | 200–400 |

Cách tính: bán tuần đầu = wishlist × 0,10–0,15; năm đầu ≈ 2,5–4 × tuần đầu (đuôi bán + các đợt giảm giá); review ≈ bán / 30. Mình xếp bi quan nặng nhất vì đa số game Steam rơi vào nhóm dưới 10 review, và game chưa có cộng đồng trước khi ra mắt.

### 6.4 Điều gì đẩy lên kịch bản cơ sở / lạc quan

1. **Khảo sát** cho "Ý định" (Q12–Q14) ≥ 3,5 / 5 và "Điểm độ hay" ≥ 65 trước khi bật trang Steam; dưới mức đó thì sửa vòng lặp trước, quảng bá sau.
2. **Ảnh capsule + trailer** do người vẽ / dựng chuyên (đây là chỗ đáng chi tiền nhất).
3. **Demo trong Next Fest** + gửi key cho streamer Việt (game có giọng khách đặc biệt và chủ nợ — hợp xem trực tiếp).
4. **Chế độ chơi lại** (mức khó, thử thách hằng ngày) để kéo dài đuôi bán và review.

## 7. Còn nợ

| Việc | Mức | Ở đâu |
| --- | --- | --- |
| Chưa có người ngoài chơi thử (nhóm B, C của sprint 11 trống) | Cao | `KNOWN-ISSUES.md` K-4, K-6, K-9; khảo sát |
| Tên game chưa chốt; giọng 3 khách (Voicemod) chưa rõ giấy phép | Cao trước Steam | Q-46, Q-70; `CREDITS.md` |
| 67 câu hỏi diễn giải còn mở (đa số nhỏ: màu, vị trí, chữ Art Bible) | Trung bình | `.dev-memory/QUESTIONS.md` |
| Khựng khung lần nhét vé đầu, GC (K-1..K-3, K-12) | Thấp | cần Unity Profiler |
| Bản tiếng Anh chưa có người bản ngữ đọc; chữ trong bong bóng truyện có thể dài hơn khung | Trung bình | `docs/sprint-11/ui/en_*.png` |
| Tay cầm / Steam Deck, thân khách đa dạng, tranh truyện vẽ tay | Sau phát hành | impl plan sprint 11 §6 |

## 8. Đề xuất bước tiếp theo (theo thứ tự)

1. **Tuần này:** gửi `QuanAnHope-0.11.0.zip` + link khảo sát cho 5–10 người; tự ngồi xem 2 người chơi ngày 1.
2. Chốt tên game, tên nhà phát triển, giọng khách (B1–B3 của kế hoạch Steam).
3. Gom khảo sát → một sprint sửa nhóm A/B/C theo đúng quy trình sprint 11.
4. Đăng ký Steamworks, làm trang Coming Soon, đặt mục tiêu ≥ 3.000 wishlist trước ngày ra mắt (kế hoạch: `docs/release/STEAM-DEPLOY-PLAN.md`).
5. Demo cho Next Fest gần nhất; phát hành 1.0 nếu Ý định ≥ 3,5, ngược lại Early Access.
