# Spec — Sprint 10: sửa trước sprint + hoàn thiện, bản chơi thử

Ngày: 22–23/11/2026 · Nguồn: tin nhắn của bạn 22/11/2026 (ghi tắt **[YC]**), `Phong cách hướng dẫn bằng hình.md` (bạn đưa, ghi tắt **[HÌNH]**), `docs/sprint-10/impl-plan.md`, constitution 1.9 → **2.0**.

Phiên này không hỏi lại được giữa chừng: chỗ nguồn chưa nói rõ đánh dấu **[TỰ QUYẾT]** và ghi `.dev-memory/QUESTIONS.md` (Q-70..Q-79).

## Bảng Art Bible / [HÌNH] → làm ở đâu (L-013)

| Quy định | Làm ở |
| --- | --- |
| [HÌNH] khung kem #FFF3DC viền mực #2B2321, bo góc giống nhau; chữ ≤ 4 từ trong dải riêng dưới tranh | `PicKit.Frame` |
| [HÌNH] huy hiệu số tròn mù tạt góc trái trên; mũi tên dày đầu đặc viền mực; chevron "›" mờ giữa khung; bóng mờ nét đứt ở đích | `PicKit.Badge / Arrow / Chevron / Ghost` |
| [HÌNH] ĐÚNG = huy hiệu tròn bạc hà + ✓; SAI = bát giác san hô + ✗; CẢNH BÁO = tam giác mù tạt + "!" (khác cả hình, đọc được ở thang xám) | `PicKit.Mark / Warn` (vẽ bằng hình: font Baloo 2 không có glyph ✓ ✗) |
| [HÌNH] bàn tay ma găng kem viền mực, mỗi cử chỉ một tín hiệu đầu ngón; nhịp Material 225 / 200 / cử chỉ / 400 / 195 / 450 ms; vòng < 3 giây | `GhostHand` |
| [HÌNH] một ngôn ngữ hình: cùng icon ở thẻ hướng dẫn, thực đơn, Tab | `IconBook` (chụp từ chính model trong phòng) |
| [HÌNH] khuôn A (một hành động + kết quả), khuôn B (thực đơn: thành phẩm → CẦN icon × số → các bước), khuôn C (đúng / sai / thưởng) | `GuidePics`, `TabMenu` thẻ Thực đơn |
| Art Bible §13: chữ ≥ 14 px ở 1280×720, nút ≥ 52 px | Tab, HUD |

## Sửa trước sprint

### B-08 Hướng dẫn bằng hình — [YC] "chỉnh sửa lại tất cả phần hướng dẫn, sử dụng [HÌNH] … bỏ góc 1, góc 2, bỏ những con số dài dòng khó hiểu, món ăn thấp hơn 1 bậc"
Kịch bản 10 giây: vào ngày 1 → góc trái có thẻ "1 · Nhét vé": vé → máy, bàn tay ma chạm máy, › hai thùng; dòng vàng trên HUD "1/10 · Nhét vé"; mũi tên 3D trên máy. Nhét vé → thẻ đổi sang "Lấy 1 món" + thẻ khái niệm "Màu đế = bậc" (6 quả cà chua 6 màu đế).
- AC-1: mỗi bước một hành động; chữ bước ≤ 6 từ, không "Góc N"; chữ chú thích tranh ≤ 4 từ. *Test:* `B08_…` (PlayMode).
- AC-2: bước ngày 1 nào cũng có tranh; bước có khái niệm thì thêm thẻ khái niệm (bậc nguyên liệu, giá, bậc món, độ dễ tính, đúng / sai món, chờ lâu, khói). *Test:* `B08_…`, `F050_F053_…`.
- AC-3: thẻ có bàn tay ma lặp cử chỉ (vòng < 3 giây). *Test:* `B08_…`.
- AC-4: Tab → Cách chơi là các thẻ tranh (2 trang × 6). *Test:* `B08_…`, ảnh `b08_tab_cach_choi_1/2.png`.
- AC-5: lời nhắn trong game viết lại ngắn (bỏ "Góc N", bỏ "món thấp hơn 1 bậc…"): ví dụ "Hơi tệ: khách trả nửa giá", "Sai món! Khách gọi …".
- Ngoài phạm vi: test 5 giây với 20–30 người chơi Việt ([HÌNH] mục test) → F-094.

### B-09 Cài đặt: nút kéo tròn — [YC] "HUD của cài đặt các phần hình tròn bị biến dạng thành hình ovan"
Nguyên nhân: Slider kéo nút theo chiều cao thanh, sizeDelta.y là phần cộng thêm → nút 30 × 64 px.
- AC-1: nút kéo 38 × 38 px, tròn. *Test:* `B09_CaiDat_NutKeoTron`.

### B-10 Tab Relic: hình relic thật — [YC]
- AC-1: mỗi dòng relic có hình chụp từ model relic trên đĩa màu độ hiếm (vuông, không kéo). *Test:* `B10_…`, ảnh `b10_tab_relic.png`.

### B-11 Phòng: bàn nhận hàng dài, bỏ sách + giá relic, relic ra giữa, thực đơn trong Tab — [YC]
- AC-1: bàn nhận hàng 3,9 m, mỗi hàng 6 thùng (vòng 4 tối đa 2 tầng). *Test:* `B11_BanNhanHangDai_…`.
- AC-2: không còn sách và giá relic trên tường ở Góc 3; ba bệ relic ở x = −1,25 / 0 / 1,25 (giữa tường). *Test:* `B11_ThucDonTrongTab_…`.
- AC-3: Tab có thẻ **Thực đơn** (khuôn B): danh sách món đã mở + giá; trang món: tên, giá niêm yết, thành phẩm, hàng CẦN (3 icon), khung bước = số bước + 2. *Test:* `B11_…`, `ChoiTronVong1_…`.

### B-12 Giá niêm yết — [YC] "bỏ phần giá theo bậc món, có giá niêm yết cho từng món ở bậc bình thường … tự tính toán và cân bằng lại giá"
- AC-1: mỗi món một `price`; tiền bán = price × hệ số bậc; đền bù = 30% price (món khách gọi khi sai món). *Test:* `B12_…` (EditMode).
- AC-2: trung bình giá mỗi vòng = giá gốc cũ × 1,05 (±2G), hệ số chọn bằng bot (mục 9 constitution). *Test:* `B12_MoiMonCoGia_…`.

### B-13 Bỏ chữ "Quầy chờ", "Nấu" — [YC]. *Test:* `B13_…`.

### B-14 Bớt 30% công thức + món đột phá trong thực đơn — [YC]
- AC-1: 84 món (56 + 28); mỗi vòng ~70% cặp; mỗi nguyên liệu có món ngay ở vòng mở. *Test:* `B14_…`.
- AC-2: bếp từ chối cặp không có món (nhắn "X không nấu chung với Y"). *Test:* `B14_BepKhongNhan…`.
- AC-3: **[TỰ QUYẾT] Q-71** gacha chọn loại chính nấu được với loại phụ vừa ra ("tính toán với loại nguyên liệu tương ứng") → không có lượt quay "chết". *Test:* `B14_MoiLuotQuay_…`, `AC8_Vong1_…` (Điều 3: tỉ lệ loại chính theo bảng cặp, lệch ≤ 1 điểm).
- AC-4: món đột phá hiện trong thực đơn khi có rương đột phá. *Test:* `B14_ThucDonCoMonDotPha`, `B11_…`.

### B-15 Thớt — [YC] "mua thêm thớt max 3, tăng giá mỗi lượt mua … làm rộng phòng hơn … bàn sơ chế sẽ dài hơn và thêm thớt"
- AC-1: **[TỰ QUYẾT] Q-72** tổng tối đa 3 thớt (có sẵn 1), 300G rồi +200G, mở cùng cửa hàng (ngày 4). *Test:* `B15_…` (EditMode + PlayMode).
- AC-2: phòng 10,8 m (Grow 1,4); mua thớt thì bàn trái bếp dài thêm + thớt mới hiện ngay; hai thớt sơ chế độc lập; số thớt lưu trong bản lưu. *Test:* `B15_HaiThot_…`, `B15_LuuTai_…`, ảnh `b15_ba_thot.png`.

### B-16 Shift chạy, Space nhảy, chuột phải thả — [YC]
- AC-1: Shift × 1,7 tốc độ; Space nhảy (rơi về sàn). *Test:* `B16_…`.
- AC-2: chuột phải khi cầm nguyên liệu: rơi xuống đúng chỗ đang nhìn (mặt bàn / sàn, theo hình thật L-016), nhặt lại được; **[TỰ QUYẾT] Q-73** đĩa / đĩa trống vẫn đặt lại chỗ cũ. *Test:* `B16_…`, `ChoiTronVong1_…`.

### B-17 Đặt đĩa trước mặt khách, bỏ chuông — [YC]
- AC-1: mỗi ghế có chỗ đặt đĩa trên quầy (`Table_i`), đặt đĩa = phục vụ khách đó, không theo thứ tự; không còn chuông. *Test:* `B17_…`, `B13_…`.

### B-18 Chỉ đóng quán mới qua ngày — [YC]. *Test:* `B18_…` (EditMode), `Gio21_…` (PlayMode).

### B-19 Khách hết thanh chờ bỏ đi, trừ tiền — [YC]
- AC-1: **[TỰ QUYẾT] Q-74** thanh hết = khoảng an toàn + 60 giây; trừ 20 / 25 / 30G (vòng 2 / 3 / 4); vòng 1 và chủ nợ không bỏ đi. *Test:* `B19_…` (EditMode + PlayMode, ảnh `b19_khach_bo_di.png`).

## Sprint 10 (impl plan)

| Việc | AC | Kiểm |
| --- | --- | --- |
| F-090 | 1: bảng rà constitution | `docs/sprint-10/constitution-audit.md` |
| | 2: câu hỏi OPEN đóng hoặc "để sau phát hành" | **CẦN BẠN** — đề xuất từng câu trong QUESTIONS.md |
| | 3: script so số | `tools/check_constitution.py` (98 dòng, 0 lệch) |
| F-091 | 1: bot 200 ván / kiểu trong build, 0 ngoại lệ | `QuanAnHope.exe -batchmode -nographics -botrun 200` → `bot-build.txt` |
| | 2: PlayMode + EditMode xanh | lệnh test (STATE.md) |
| | 3: lưu / tải 50 lần | `F091_AC3_…` (EditMode) |
| | 4: lỗi còn lại | `docs/KNOWN-ISSUES.md` |
| F-092 | 1: ≥ 60 FPS ở 1920×1080, ngày đông nhất | `QuanAnHope.exe -benchmark` → `benchmark.txt` |
| | 2: không khựng > 50 ms khi nhét vé / khách vào / lật thực đơn | như trên (xem KNOWN-ISSUES: còn lần nhét vé đầu) |
| | 3: mở game tới menu < 10 giây | như trên |
| F-093 | 1–4 | `Branding.cs`, `HUONG-DAN.txt`, `CREDITS`, nhãn phiên bản menu; chạy thử máy khác: **CẦN BẠN** |
| F-094 | 1–3 | **CẦN NGƯỜI CHƠI**: bộ câu hỏi trong `HUONG-DAN.txt`, tổng hợp `tools/playtest_tong_hop.py` |

## Ngoài phạm vi
Tính năng mới ngoài danh sách trên; bot dùng thớt thứ 2 / thả nguyên liệu (bot không mô hình đi lại song song); test 5 giây với người chơi Việt (F-094).
