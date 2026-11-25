# Kế hoạch đưa "Quán ăn của Hope / Hope's Diner" lên Steam

Ngày lập: 25/11/2026 · Bản game: 0.11.0 (rc-1) · Người làm: một người (bạn) + Claude Code

Nguyên tắc: **không trả phí và không bật trang cửa hàng khi chưa chốt 3 việc chặn** ở mục 1. Mọi mốc dưới đây tính từ ngày bạn bắt đầu (gọi là **T0**), không gắn ngày lịch cứng vì phụ thuộc kết quả chơi thử.

---

## 1. Việc chặn phải xong trước khi trả phí

| # | Việc | Vì sao chặn | Ai làm | Nguồn trong repo |
| --- | --- | --- | --- | --- |
| B1 | **Chốt tên game** (VN + EN) và kiểm trùng tên trên Steam, Google | Tên nằm trong app, thư mục lưu, trang cửa hàng; đổi sau khi bật trang rất tốn công | Bạn | Q-46, Q-70 (`.dev-memory/QUESTIONS.md`); hiện: menu "Quán ăn của Hope", biển trong phòng "QUÁN NHÂN PHẨM", file truyện "Bếp Nhân Phẩm" |
| B2 | **Giọng 3 khách đặc biệt** (file Voicemod): xác nhận quyền dùng thương mại hoặc thay bằng giọng tự thu / CC0 | Valve gỡ game nếu có tài sản không rõ giấy phép; `CREDITS.md` đã đánh dấu "kiểm lại trước khi phát hành công khai" | Bạn | `docs/CREDITS.md` mục Âm thanh |
| B3 | **Tên nhà phát triển / nhà phát hành** (đang là "hungnhat29") và thông tin thuế / ngân hàng | Steamworks bắt buộc; tên hiện trên trang cửa hàng | Bạn | `Editor/Branding.cs` (CompanyName) |
| B4 | Ít nhất **5–10 người ngoài chơi thử** bằng khảo sát (`docs/release/KHAO-SAT.md`) và log `playtest-log.csv` | Chưa có số đo người thật (impl plan sprint 11 nhóm B, C trống); Điều 1 mới chỉ đo bằng bot | Bạn đi khảo sát, Claude tổng hợp (`tools/playtest_tong_hop.py`) | `docs/KNOWN-ISSUES.md` K-4, K-6, K-9 |

Đổi tên (B1) trong code: `MenuView.GameTitle`, `Editor/Branding.cs`, `RoomView.ShopName`, bảng dịch `tools/i18n/en.tsv`. Lưu ý K-5: đổi tên sản phẩm làm đổi thư mục lưu (bản lưu cũ không đọc được) — chấp nhận được vì chưa có người chơi thật.

---

## 2. Lộ trình (8–12 tuần từ T0)

| Mốc | Tuần | Việc | Xong khi |
| --- | --- | --- | --- |
| M0 | T0 → T0+2 | Xong B1–B4; sửa lỗi nhóm A/B từ khảo sát (quy trình sprint 11: A → B → C → D) | Không còn lỗi chặn; ≥ 60% người chơi thử qua lần trả nợ 1 |
| M1 | T0+1 | **Đăng ký Steamworks**: tài khoản Steam có chi tiêu, ký Steam Distribution Agreement, khai thuế (W-8BEN cho cá nhân ngoài Mỹ), tài khoản ngân hàng, **trả phí Steam Direct (100 USD / game)** — phí thu hồi lại khi game đạt 1.000 USD doanh thu điều chỉnh | Có App ID |
| M2 | T0+2 → T0+3 | Làm **trang cửa hàng** (mục 3) + ảnh, trailer; gửi duyệt trang | Valve duyệt trang |
| M3 | T0+3 | **Bật trang "Coming Soon"** công khai. Valve yêu cầu trang công khai **ít nhất 2 tuần** trước khi phát hành; thực tế nên để **≥ 3 tháng** để gom wishlist | Trang công khai, bắt đầu đếm wishlist |
| M4 | T0+3 → T0+6 | **Bản demo** (vòng 1–2, ngày 1–8) làm app Demo riêng; đăng ký **Steam Next Fest** gần nhất (lịch và hạn đăng ký xem trong Steamworks → Upcoming Events) | Demo lên Steam, có trong Next Fest |
| M5 | T0+4 → T0+8 | Tích hợp Steam (mục 4), build qua SteamPipe vào nhánh beta, chơi thử nội bộ bằng key | Bản build trên Steam chạy được từ thư viện |
| M6 | ≥ 2 tuần trước ngày phát hành | Gửi **build review** (Valve chạy game, kiểm cấu hình; thường vài ngày làm việc — để dư 1 tuần) | Valve duyệt build |
| M7 | T0+8 → T0+12 | **Phát hành** (Early Access hoặc 1.0, xem mục 5). Lưu ý: với vài game đầu, Valve có thêm yêu cầu thời gian chờ giữa lúc trả phí và lúc phát hành — **kiểm trong Steamworks** (nhiều hướng dẫn ghi 30 ngày) | Nút Release bấm được |
| M8 | Sau phát hành 0–30 ngày | Sửa lỗi nóng, trả lời review, bản cập nhật 1 tuần sau phát hành | Không còn lỗi chặn trong review |

---

## 3. Trang cửa hàng

| Hạng mục | Nội dung | Trạng thái |
| --- | --- | --- |
| Ngôn ngữ trang | Tiếng Anh + Tiếng Việt | Game đã có cả hai (từ 0.11.0); bản dịch Anh cần người bản ngữ đọc lại |
| Mô tả ngắn (EN) | "Cook your way out of debt. Pull ingredient crates, prep and cook in a cozy 3D diner, and pay the Creditor in 22 days." | Nháp — bạn sửa |
| Thể loại / tag | Cooking, Simulation, Roguelite, Management, Cozy, First-Person, Casual, Singleplayer | Chọn lại theo phản hồi khảo sát |
| Ảnh capsule | Header, small, main, vertical, library capsule / hero / logo — kích thước theo trang "Graphical Assets" của Steamworks (đọc lại bản mới nhất trước khi vẽ) | Chưa có. Dùng phong cách Art Bible + tranh truyện |
| Ảnh chụp | ≥ 5 ảnh 1920×1080 **trong game** (không UI gỡ lỗi): bếp đang nấu, khách đông vòng 2, Tab Thực đơn, chủ nợ ngày 21, trang truyện | Có thể chụp từ `docs/sprint-11/ui/` ở 1080p |
| Trailer | 30–60 giây: 5 giây đầu là cảnh nấu + khách vui, sau đó vòng lặp gacha → nấu → phục vụ → trả nợ | Chưa có |
| Khai báo AI | Steam có mục khai nội dung tạo bằng AI: code viết cùng Claude Code; hình / âm thanh dựng bằng script; ghi trung thực | Bạn điền |
| Phân loại tuổi | Bảng hỏi nội dung của Steam (không bạo lực, không cờ bạc tiền thật — gacha chỉ dùng vé trong game) | Điền khi tạo app |
| Yêu cầu hệ thống | Windows 10/11 64-bit, DirectX 11; tối thiểu ghi theo máy đã thử (Intel Iris Xe chạy ~250 FPS ở 1080p — `KNOWN-ISSUES.md` K-12) | Cần thử thêm máy thứ 2 (K-6) |

Lưu ý mô tả: gacha trong game là **cơ chế chơi**, không bán bằng tiền thật. Ghi rõ "no microtransactions" để tránh hiểu lầm.

---

## 4. Việc trong repo / bản build

| # | Việc | Ghi chú |
| --- | --- | --- |
| S1 | Steamworks SDK: thêm **Steamworks.NET** (MIT) hoặc Facepunch.Steamworks; gọi `SteamAPI.Init`, thoát an toàn nếu không có Steam (bản itch / chơi thử vẫn chạy) | Bọc trong `#if STEAM` để bản không-Steam vẫn build |
| S2 | **Steam Cloud**: dùng Auto-Cloud (cấu hình trên Steamworks, không cần code) cho thư mục `LocalLow\<công ty>\<tên game>\save.json` | Chốt tên trước (B1, B3) vì đường dẫn phụ thuộc tên |
| S3 | Thành tựu (tuỳ chọn): qua lần trả nợ 1/2/3, thắng, món Huyền thoại đầu tiên, gom đủ 1 bộ relic | 6–10 cái là đủ |
| S4 | **SteamPipe**: `steamcmd` + `app_build_<appid>.vdf`, `depot_build_<depotid>.vdf`; nội dung = `My project/Builds/Windows` **bỏ** thư mục `*_BackUpThisFolder_*` (như gói zip hiện tại) | Viết `tools/steam/upload.ps1` |
| S5 | Tắt cờ / menu gỡ lỗi trong bản phát hành: kiểm `DebugStartAt`, `-botrun`, `-benchmark` chỉ chạy khi có cờ dòng lệnh (hiện vậy) | Thêm test xác nhận |
| S6 | Bỏ dòng "Bản chơi thử này dừng ở đây" nếu phát hành đủ 22 ngày (Hud) | Kiểm `PlayableRounds = 4` |
| S7 | Số phiên bản 1.0.0 khi phát hành (`Branding.Version`), ghi `CHANGELOG` cho trang tin tức Steam | |
| S8 | Tay cầm (Steam Deck): impl plan sprint 11 §6 ghi "đổi phím và tay cầm" là việc sau. Nếu muốn nhãn "Steam Deck Playable" phải làm việc này | Không bắt buộc cho bản đầu |

---

## 5. Early Access hay 1.0?

| | Early Access | 1.0 |
| --- | --- | --- |
| Hợp khi | Muốn thêm nội dung theo phản hồi (thân khách, nguyên liệu, chế độ chơi dài) | Game đã đủ 22 ngày, 4 vòng, kết thắng / thua — **hiện đúng là vậy** |
| Rủi ro | Người chơi chấm theo lời hứa; phải cập nhật đều | Ít nội dung lặp lại sau khi thắng 1 lần |

**Đề xuất:** phát hành **1.0 giá thấp** sau Next Fest, nếu khảo sát cho thấy người chơi muốn chơi lại (câu Q12–Q14 của khảo sát ≥ 3,5/5). Nếu thấp hơn, dùng Early Access và ưu tiên nội dung chơi lại (mức khó, seed hằng ngày).

## 6. Giá

- Đề xuất **4,99–7,99 USD** (game nhỏ, một người làm, 2–4 giờ cho một ván thắng). Dùng bảng giá theo vùng gợi ý của Steam; Việt Nam nên ở mức thấp của bảng.
- Giảm giá ra mắt 10–20% trong tuần đầu.
- Tham chiếu: GameDiscoverCo đo trung vị chuyển wishlist → bán tuần đầu khoảng **0,15×** (game trên 10 USD khoảng 0,10×), trên nhóm game có > 25.000 wishlist — game nhỏ thường thấp hơn. Chi tiết dự đoán: `docs/release/BAO-CAO-TONG-THE.md` mục 6.

## 7. Checklist ngày phát hành

- [ ] B1–B4 xong, `KNOWN-ISSUES.md` không còn lỗi mức A
- [ ] EditMode + PlayMode xanh trên bản build cuối; bot 3 kiểu đạt Điều 1
- [ ] Build trên Steam chạy từ thư viện trên 2 máy khác nhau, có và không có Steam đang mở
- [ ] Cloud save: chơi máy A, tiếp tục máy B
- [ ] Trang cửa hàng: giá, ảnh, trailer, ngôn ngữ, yêu cầu hệ thống, khai AI
- [ ] Bài đăng ra mắt (Steam News) + đăng trên mạng xã hội / nhóm game Việt
- [ ] Theo dõi review 72 giờ đầu, bản sửa nhanh sẵn sàng

## Nguồn tham khảo

- Steam Direct (phí, trang Coming Soon ≥ 2 tuần, review): https://partner.steamgames.com/steamdirect
- Phí ứng dụng Steamworks (thu hồi khi đạt 1.000 USD): https://partner.steamgames.com/doc/gettingstarted/appfee
- Chuyển đổi wishlist: GameDiscoverCo, "The State of Steam Wishlist Conversions (2024–2025)": https://newsletter.gamediscover.co/p/the-state-of-steam-wishlist-conversions
- Thời gian review / chờ (nguồn thứ ba, cần kiểm lại trên Steamworks): https://www.summerengine.com/blog/how-to-publish-game-on-steam
