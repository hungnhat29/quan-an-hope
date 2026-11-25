# Constitution — Quán ăn của Hope (tên tạm)

Phiên bản 2.2 · 24/11/2026 · Trạng thái: **sau sprint 10 — đợt sửa thứ hai** (yêu cầu 24/11/2026: máy gacha đồ ăn kiểu lò vi sóng có cần gạt, gợi ý cốt truyện không đè lời kể, quầy phục vụ rộng + khách to hơn, nhãn bậc một dòng, phím T giữ trạng thái, ngày 2 relic còn lại bán bằng vé, **tay vạn năng**. Điều 5, 9; mục 2.1, 5.6, 6.2, 7.1, 8, 9). Trước đó 2.1: **sau sprint 10 — sửa theo chơi thử của bạn** (yêu cầu 23/11/2026: tỉ lệ ra bậc món mới, mọi món có thể ra Huyền thoại khi nấu, 2 relic Huyền thoại, 8 vé mỗi ngày, bếp thứ hai trong cửa hàng, lọ nước sốt cạnh bếp + hiệu ứng, 5 khách đặc biệt có tiếng, món ngon nhất cho chủ nợ + tranh gợi ý, hướng dẫn toàn màn hình phím T, phím điều khiển trong Cài đặt. Điều 1, 5, 7, 9; mục 2.1, 3, 5.1–5.5, 6.7, 7.3, 8, 9). Trước đó 2.0: **sprint 10 — bản chơi thử cho người ngoài** (yêu cầu của bạn 22/11/2026 trước sprint 10: giá niêm yết từng món, bỏ 30% công thức + công thức món đột phá trong thực đơn, thực đơn trong bảng Tab, thớt mua thêm, phòng 10,8 m, chạy / nhảy / thả nguyên liệu, đặt đĩa trước mặt khách, chỉ đóng quán mới hết ngày, khách hết kiên nhẫn bỏ đi; hướng dẫn bằng hình. Điều 1, 5, 9; mục 2.1, 4.2, 4.5, 5.1, 5.2, 5.8, 6.1, 6.2, 6.5, 6.6, 7.1, 8, 9). Trước đó 1.9: **sprint 9** (nhân vật Blender có xương, khách đi bộ từ vỉa hè, 8 kiểu mặt, viền theo điểm ảnh, món ăn model Blender, hướng dẫn giải thích tỉ lệ / bậc / độ dễ tính + thẻ "Cách chơi"; Điều 7, 9, mục 2.1, 6.2 — không đổi số cân bằng). Trước đó 1.8: **sprint 8** (relic ngày 2 miễn phí + đổi hàng bằng tiền, bảng Tab relic / cửa hàng, nhịp khách tăng theo ngày từ vòng 2, nới giờ chờ; mục 2.1, 6.2, 6.6, 7.1, 8, Điều 9). Trước đó 1.7: **cân bằng đo trên game** (Điều 1 cột "đo trên game"; ngày 1 có 5 vé, 7 vé mỗi ngày, nợ ngày 22 là 36.000G; đồng hồ chạy từ lúc mở quán, mục 3, 6.2, 9). Trước đó 1.6: **cốt truyện 6 chương** (mục 2.1, tranh `CotTruyen_BepNhanPham.zip`). Trước đó 1.5: **khung game** — phòng rộng hơn, khách ăn tại ghế, lưu game theo ngày, hướng dẫn ngày 1, cài đặt (mục 2.1, 6.2, Điều 5). Số cân bằng 1.4 giữ nguyên, thêm một số: thời gian ăn 5 giây (mục 6.2)

Đây là nguồn sự thật cao nhất của dự án. Mọi spec, plan, task và dòng code phải tuân theo file này. Khi một spec mâu thuẫn với constitution, constitution thắng; muốn đổi thì sửa constitution trước (tăng số phiên bản, ghi lý do ở mục 9).

Các con số cân bằng nằm ở hai nơi phải luôn khớp nhau: bảng trong file này và dữ liệu JSON. Code chỉ đọc số từ dữ liệu, không viết số trong code.

**Chuyển tiếp 1.3 → 1.4:** bảng trong file này là số **1.4**, khớp với `balance/next/*.json` (sinh từ `balance/make_next.py`). `balance/*.json` còn là số 1.3 mà bản game sprint 1 và test đang đọc. Việc đầu tiên của sprint 2 là chép `balance/next/*` đè lên `balance/*`, nhập lại vào game và sửa test; từ đó chỉ còn một bộ số.

---

## 1. Nguyên tắc bất biến

### Điều 1 — Độ khó mục tiêu
Game nằm trong khoảng **trung bình-dễ đến trung bình-khó**. Ba mốc dưới đây là cổng chấp nhận; mọi thay đổi số liệu phải chạy lại mô phỏng và vẫn đạt cả ba:

| Kiểu người chơi | Cách chơi | Mục tiêu | Kết quả mô phỏng v1.0 | Mô phỏng v1.3 (khách gọi món) | Mô phỏng v1.4 | Đo trên game 1.7 (bot C#, 4.000 ván) | Đo trên game 1.8 (bot C#, 4.000 ván) | Đo trên game 2.0 (bot C#, 4.000 ván) | **Đo trên game 2.1** (bot C#, 4.000 ván) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Không đọc chỉ số | Chọn món có giá to, tỉ lệ to; phục vụ theo thứ tự khách đến | Đi qua **≥ 60%** chặng đường | Đi được trung bình 66% (qua lần trả nợ 1: 100%, lần 2: 86%) | 67% (100%, 87%) | 63% (qua lần trả nợ 1: 96%, lần 2: 79%) | 75% (qua lần trả nợ 1: 100%, lần 2: 96%, lần 3: 25%) | 80% (100%, 99%, 39%) | 82% (100%, 100%, 44%) | **90%** (100%, 100%, 67%) |
| Chơi tốt, có **≥ 3 món cùng một bộ relic** | Gom bộ, giữ món ngon cho khách khó tính và boss | Thắng **≥ 70%** (phần còn lại do may) | Bộ Chợ Sớm 82%, Sơn Hào 84%, Vườn Nhà 71%; đủ 4 món: 87–97% | 82% (Vườn Nhà 76%, Chợ Sớm 86%, Sơn Hào 85%) | 90% (Vườn Nhà 98%, Chợ Sớm 85%, Sơn Hào 88%) | 86% (2.000 ván / bộ: Vườn Nhà 95%, Chợ Sớm 83%, Sơn Hào 77%) | 87% (đủ 3 món: 88%; quét 1.000 ván / bộ: Vườn Nhà 95%, Chợ Sớm 82%, Sơn Hào 80%) | 85% (đủ 3 món: 85%) | **89%** (đủ 3 món: 89%) |
| Chơi tốt nhưng không gom bộ | Như trên, chỉ mua relic lẻ | Thấp hơn rõ so với người gom bộ | 64% | 56% | 46% | 37% | 34% | 50% | **31%** |

**Từ 1.7 số đo trên game là chuẩn** (impl plan sprint 7 §4: "game thắng, mô phỏng sửa theo"). Bot C# chơi trên luật thật (`balance/run_bots.ps1`), giờ thao tác mỗi món lấy từ số đo `playtest-log.csv` (mục 9). Mô phỏng Python cùng bộ số cho 78% / 98% / 83%: vẫn lạc quan hơn game vì không mô hình khách giữ ghế (Q-39), chờ ghế của chủ nợ (Q-40) và món khách gọi ngẫu nhiên trên 3 ghế; chỉ dùng để dò hướng.

Số v1.4 phụ thuộc mạnh vào một giả định chưa đo: thời gian đi lại và thao tác cho mỗi món (xem mục 9). Sprint 2 phải đo bằng người thật. Sprint 7 đo được 19 giây / món (người chơi tốt, 47 món vòng 1), sát giả định 20 giây; cần thêm buổi chơi của bạn ở vòng 2–4.

### Điều 2 — Dữ liệu tách khỏi code
Mọi tỉ lệ, giá, nợ, hệ số, danh sách relic là dữ liệu (ScriptableObject sinh từ `balance.json` / `relics.json`). Chỉnh cân bằng không được phải sửa code.

### Điều 3 — Mọi xác suất có test
Mỗi bảng xác suất có một test thống kê (EditMode): quay 100.000 lần, mỗi kết quả lệch không quá ±1 điểm phần trăm so với bảng.

### Điều 4 — Minh bạch có chủ đích
- **Hiện tỉ lệ:** gacha nguyên liệu (theo vòng), vé đen, khoảng bậc món ăn đang nấu, nước sốt, giá và hiệu ứng relic.
- **Giấu tỉ lệ:** máy gacha đồ ăn (để người chơi cảm thấy "thuần may rủi").
- Ngày mất điện (ngày 8) tạm giấu khoảng bậc món và bậc nguyên liệu — đây là thử thách, không phải lỗi.

### Điều 5 — Một căn phòng 3D, tự đi lại
Người chơi chỉ ở trong một căn phòng 3D và nhìn bằng mắt Hope (góc nhìn thứ nhất). Mỗi tường là một khu chức năng.
- **Kích thước (từ 2.0):** sàn **10,8 × 10,8 m** (1.5–1.9: 9,6 m; Art Bible §8 ghi 8 × 8 m): mỗi tường lùi ra 1,4 m, đồ vật trong từng góc giữ nguyên vị trí so với tường của góc đó, khoảng trống giữa phòng rộng ra; bàn sơ chế dài ra khi mua thêm thớt (mục 8). Số nằm ở `BepNhanPham.RoomSize` (quy cách hình, không phải số cân bằng).
- **Di chuyển:** tự đi bằng bàn phím (W A S D hoặc phím mũi tên), nhìn bằng chuột; **giữ Shift để chạy, Space để nhảy** (từ 2.0). **Không có nút chuyển góc.**
- **Tương tác:** nhìn vào đồ vật (tâm ngắm giữa màn hình) và bấm chuột trái; việc cần giữ (sơ chế) thì giữ chuột trái. Chuột phải (từ 2.0): **nguyên liệu rơi xuống ngay chỗ đang nhìn** (mặt bàn, sàn) và nằm đó, nhặt lại được, hết ngày thì mất như nguyên liệu trong thùng; đĩa món / đĩa trống thì đặt lại chỗ cũ.
- **Cầm đồ:** mỗi lúc cầm tối đa **một** thứ: một nguyên liệu, một đĩa trống, một đĩa món hoặc (từ 2.1) lọ nước sốt. **Từ 2.2 có tay vạn năng** (mục 8) thì cầm được **2 thứ** (hai ô): mặc định dùng / thả theo thứ tự nhặt (ô nhặt trước dùng trước); phím **1 / 2** chọn ô để dùng hoặc thả ô đó; đồ ở ô kia hiện ở tay trái.
- **Phím (từ 2.1):** danh sách phím nằm trong **Cài đặt** (cột "Điều khiển"); HUD không còn dòng chữ hướng dẫn di chuyển. **T** ẩn / hiện tranh hướng dẫn toàn màn hình.
- Không có màn hình chơi nào nằm ngoài căn phòng (trừ menu, kết thúc ngày, cốt truyện).

### Điều 6 — Công nghệ
Unity 6 LTS, URP, C#. Unity Test Framework cho test. Git + Git LFS (`.fbx`, `.png`, `.blend`, `.wav`). Model và animation nhân vật làm bằng Blender, xuất `.fbx`. Chữ trong UI dùng TextMeshPro, không dùng ảnh chữ.

### Điều 7 — Phong cách hình ảnh
Màu ấm kiểu hoạt hình cổ (Cuphead) cho nền; nhân vật và đồ vật đơn giản, tròn, viền đậm, màu phẳng. Mắt là **vòng tròn rỗng viền đậm, hở một đoạn phía trên**, miệng chữ "w" nhỏ. Mặt là ảnh 2D dán lên đầu 3D, đổi biểu cảm bằng đổi ảnh. **Từ 1.9:** khách có 8 kiểu mặt (cỡ / khoảng cách mắt, lông mày, má, lông mi, râu, nốt ruồi, độ rộng miệng) giữ đúng quy cách mắt và miệng trên; nhân vật là model Blender có xương (một thân cho mọi khách, đổi tóc / mũ / áo; chủ nợ model riêng); **từ 2.1 thêm 5 khách đặc biệt** theo tranh bạn đưa 23/11/2026 (ông trùm VIP vest xanh than + kính râm + thẻ VIP; anh streamer tai nghe tai mèo, áo "GG", nốt ruồi dưới miệng; nhà vô địch băng đô "VÔ ĐỊCH", áo số 66 logo xe lăn có cánh; fan Quỷ Đỏ áo số 7, khăn, ngón tay xốp "#1"; anh đại gia kính tròn, cầm xấp tiền), xuất hiện từ ngày 1 với khoảng **1/4** số khách; ba người có tiếng khi bước vào quán (mục 6.2); viền mực dày theo điểm ảnh màn hình (≈ 5 px nhân vật / máy lớn, ≈ 3 px đạo cụ ở 1280×720), vật dưới ngưỡng chi tiết không viền. **Từ 2.2 (yêu cầu 24/11/2026 "trau chuốt viền đen"):** chi tiết nhỏ của khách (kính, tai nghe, mic, băng đô, logo, thẻ, tiền) là mesh riêng viền mỏng ~½; chữ in, nốt ruồi, sơn má, khuy không viền (tên mesh `_VienMong` / `_KhongVien`); lọ sốt, máy gacha, bếp 2 dựng lại bằng Blender (bo cạnh, viền ôm hình). Bốn tường dùng chung giấy dán tường, sàn gỗ, nẹp gỗ.

### Điều 8 — Quy trình SDD
Mỗi tính năng: `spec.md` (luật + tiêu chí chấp nhận Given/When/Then) → `plan.md` → `tasks.md` → code + test → chơi thử. Sai thì sửa spec trước, code sau. Spec không được đặt con số mới; con số mới phải vào constitution/dữ liệu trước.

### Điều 9 — Thông tin nằm trên đồ vật
Thông tin của một thứ hiện **trên chính thứ đó trong phòng**, không hiện thành bảng phẳng trên màn hình:
- Nguyên liệu nằm trong thùng; **bậc đọc bằng màu** (đế bậc dưới mỗi món, Art Bible §7), không cần chữ.
- Tỉ lệ gacha: bảng treo trên tường Góc 2. Khoảng bậc món, thứ tự đã cho vào, tiến độ nấu: bảng nhỏ ngay trên bếp. Tiến độ sơ chế: thanh ngay trên thớt.
- **Thực đơn (từ 2.0):** công thức từng món và **giá niêm yết** nằm ở bảng Tab → Thực đơn (bỏ quyển sách trong phòng). Độ dễ tính và món khách gọi: bong bóng trên đầu khách. Đĩa món: nằm trên quầy (không còn chữ "Quầy chờ"), viền đĩa theo màu bậc.
- **HUD chỉ hiện con số và thông tin ngắn:** ngày, tiền, nợ, vé, tâm ngắm, gợi ý tương tác của đồ đang nhìn, thứ đang cầm, thông báo một dòng. HUD không có nút bấm.
- Màn kết thúc ngày, qua vòng, thua là màn ngoài phòng (Điều 5) nên được dùng nút.
- **HUD (từ 2.1):** bỏ dòng chữ phím điều khiển màu trắng ở góc dưới (chuyển vào Cài đặt). **Từ 2.2:** có tay vạn năng thì đáy màn hình có **2 ô đồ** (kiểu hotbar Minecraft: số 1 / 2, icon đồ đang cầm, ô đang dùng màu mù tạt).
- **Bảng Tab (từ 1.8, yêu cầu của bạn 21/11/2026; 2.0: bốn thẻ):** nhấn Tab mở bảng ngoài phòng (đồng hồ dừng như menu tạm dừng): **Thực đơn** (danh sách món + giá niêm yết; trang món theo khuôn "thành phẩm → CẦN (icon × số) → các bước đánh số"; món đột phá hiện khi đã có rương đột phá), **Relic** (hình relic thật, mọi relic đang có, bỏ relic — **không còn giá relic trên tường**, relic đang có chỉ xem ở đây), **Cửa hàng** (máy gacha đồ ăn, vé đen, nước sốt, **thớt**, **bếp** (từ 2.1): giá, số đang có, tỉ lệ theo Điều 4), **Cách chơi** (thẻ tranh). Bảng này được dùng nút. Đồ đã mua mới hiện trong phòng (máy gacha, khe vé đen, thớt; **lọ sốt ở bên phải bếp** và **bếp thứ hai bên phải quầy đĩa** từ 2.1). Lọ sốt chỉ để cầm, không mua thêm khi bấm.

---

## 2. Cốt truyện và bối cảnh

- **Nhân vật chính:** Hope. Cùng một ngày, Hope bị đuổi việc và về nhà gặp chủ nợ. Hope van xin; chủ nợ giao cho Hope toàn quyền một quán ăn sắp đóng cửa của ông để bán và phát triển, và sẽ quay lại đòi nợ theo lịch.
- **Không gian:** một quán ăn trong một căn phòng 4 tường; người chơi chỉ tương tác trong phòng.
- **Giờ mở cửa:** 9:00 – 21:00 (trong game). **1 giờ trong game = 40 giây thực, một ngày 8 phút.** Đồng hồ chạy từ đầu ngày. Khách vào liên tục, không giới hạn số lượng, cho tới 21:00 (mục 6.2). Sau 21:00, khách còn ngồi vẫn được phục vụ đến người cuối cùng.
- **Kết thúc:** trả hết nợ ngày 22; chủ nợ gả con gái cho Hope.

### 2.1 Khung game: menu, cốt truyện, lưu game, hướng dẫn, cài đặt (từ 1.5)

Người dùng duyệt ngày 18/11/2026 khi yêu cầu triển khai `docs/sprint-6/impl-plan.md` (Q-8).

- **Menu chính** (màn ngoài phòng, Điều 5): căn phòng nhìn từ cửa, bốn nút dọc bên trái: Chơi mới, Chơi tiếp (mờ khi chưa có bản lưu), Cài đặt, Thoát. **Esc** trong phòng mở menu tạm dừng (Tiếp tục, Cài đặt, Về menu chính); khi tạm dừng đồng hồ ngày, bếp và khách đều dừng.
- **Cốt truyện (từ 1.6, 20/11/2026):** 6 trang truyện tranh vẽ sẵn (`CotTruyen_BepNhanPham.zip`, mỗi khung 6 lớp giấy có thị sai). **Chương 1** khi Chơi mới (bị đuổi việc, gặp chủ nợ, nhận quán, giấy nợ) rồi vào ngày 1. **Chương 2–4** chiếu ở đầu ngày: ngày 2 (bão, kho trống), ngày mất điện (`blackout.day`), ngày chủ nợ thu lần 3 (tin đồn bị lật tẩy, danh tiếng trở lại). Thắng → **Chương 5–6** (món cuối cho chủ nợ, xé giấy nợ, gả con gái). Thua → tranh ghi rõ thiếu bao nhiêu ở lần trả nợ nào (tranh khối, chưa có trang vẽ), rồi màn thua có nút Chơi lại. Khung tự sang sau nhịp khung + thời gian đọc; bấm để qua sớm, giữ để bỏ qua. Lời kể dưới trang và chữ trên tranh đều là TextMeshPro (Điều 6); số ngày, số tiền lấy từ dữ liệu (Điều 2). Cài đặt "Hiệu ứng truyện": Đầy đủ / Giảm / Tắt.
- **Lưu game:** tự lưu **đầu mỗi ngày** (sau khi thu nợ, phát vé): ngày, tiền, vé trắng, vé đen, relic, ba relic đang bày, tủ lạnh, nước sốt, máy gacha, số lần đã mua, và trạng thái ngẫu nhiên (seed + số lần đã rút). Chơi tiếp → về đúng đầu ngày đã lưu. **Tải lại thì ngày đó ra y như lần trước** (gacha, khách, cửa hàng): không "lưu rồi quay lại" để chọn kết quả tốt. Thắng hoặc thua thì xoá bản lưu. Chỉ có một bản lưu.
- **Hướng dẫn ngày 1:** ván đầu tiên, ngày 1: một dòng chỉ dẫn trên HUD + mũi tên nổi trên đồ vật kế tiếp (máy nhét vé → thùng → thớt → giá dụng cụ → sách → bếp → núm → chồng đĩa → biển Mở quán → khách / chuông). Làm đúng bước thì sang bước sau, không chặn việc khác. Tắt / bật trong Cài đặt; chưa chọn thì ván thứ hai trở đi tắt.
- **Hướng dẫn bằng hình (từ 2.0, yêu cầu 22/11/2026 + "Phong cách hướng dẫn bằng hình.md"):** mỗi bước **một hành động**, chữ ngắn (≤ 4–6 từ, không gọi tên "Góc N", không dãy số dài), kèm **thẻ tranh** góc trái màn hình (khuôn A: icon chụp từ chính đồ vật trong phòng + bàn tay ma làm mẫu cử chỉ + mũi tên mù tạt + huy hiệu số) và **thẻ khái niệm** (khuôn C: bậc nguyên liệu, món ngon → đắt hơn, độ dễ tính → món tối thiểu, đúng / sai món, chờ lâu → bỏ đi, khói → múc ngay). Đúng / sai khác nhau cả hình lẫn màu (✓ tròn bạc hà, ✗ bát giác san hô). Mũi tên 3D trên đồ vật giữ nguyên. Thẻ Cách chơi trong Tab là các thẻ tranh này. Thay cho mục "Giải thích chỉ số" của 1.9 (chữ dài, bỏ).
- **Giải thích chỉ số (1.9, thay bằng hướng dẫn bằng hình ở 2.0):** hướng dẫn ngày 1 có ô giải thích đi kèm bước (bảng tỉ lệ bậc của vòng, 6 bậc nguyên liệu, công thức bậc món mục 5.3, giá theo bậc, **độ dễ tính** và món tối thiểu mục 6.4, trả tiền / tip mục 6.5); bài vòng 2 giải thích giờ chờ (mục 6.6). Bảng Tab có thẻ thứ ba **Cách chơi** đọc lại mọi bảng, số lấy từ dữ liệu theo ngày hiện tại.
- **Hướng dẫn theo sự kiện (từ 1.8):** cùng công tắc với hướng dẫn ngày 1, mỗi bài hiện một lần mỗi ván: trời tối lần đầu (bật đèn), **đầu ngày 3** (từ 2.1, yêu cầu 23/11/2026 "chuyển phần art tutorial ngày 2 sang đầu ngày 3": trả vé mua relic, Tab xem relic; 1.8–2.0: ngày 2), ngày mở cửa hàng vật phẩm (Tab → Cửa hàng, nhiều khách + thanh kiên nhẫn, quá lửa, tủ lạnh), **lần đầu có nước sốt** (từ 2.1: cầm lọ cạnh bếp → bấm đĩa vừa nấu; tranh "thường lên bậc / có thể hỏng / Huyền thoại"), **đầu vòng 4** (từ 2.1: tranh gợi ý món cuối cho chủ nợ — Trứng chiên cà chua bậc Huyền thoại), ngày mất điện, ngày chủ nợ (nhắc lại tranh gợi ý). Chơi tiếp từ bản lưu vẫn có bài của ngày đó.
- **Hướng dẫn toàn màn hình (từ 2.1, yêu cầu 23/11/2026):** mỗi bước mới có tranh thì tranh hiện **to giữa màn hình** (nền tối mờ, thẻ bước + thẻ khái niệm cạnh nhau, khung bo góc lớn, hình vẽ ở độ phân giải cao cho tròn trịa); **T** ẩn / hiện lại; khi ẩn vẫn còn thẻ nhỏ ở góc trái và dòng chữ "… · T: xem hình". **Từ 2.2 (yêu cầu 24/11/2026):** đã bấm T tắt thì các bước sau **không tự bật lại**, chỉ bấm T mới bật.
- **Kiểu diorama giấy (từ 2.2, yêu cầu 24/11/2026 + `docs/sprint-10/ui/paper-ui-pickit.md`):** mỗi thẻ tranh là hộp giấy nhiều lớp — bìa carton #C9A47E lệch phía sau, khung kem viền mực khoét cửa sổ (nền nhạt theo nhóm thẻ + chấm tường + dải sàn + bóng trong), đồ vật là sticker viền trắng có bóng, dải chữ giấy xé dán 2 băng dính mù tạt, huy hiệu số có đinh ghim (thẻ khái niệm: "?" tím), hạt giấy 7%. Căn chỉnh: 3 cột 18 / 50 / 82%, nhãn ngay dưới hình căn giữa, viên thuốc rộng cố định không xuống dòng, chữ ≥ 14 px. Chuyển động stop-motion 12 hình/giây (tay ma, rung viền ±1 px 8 lần/giây, thẻ bật 120% → 96% → 100%, các thẻ Tab dán vào lần lượt); camera / chữ / nút vẫn mượt. Thẻ trong game bỏ huy hiệu số (băng rôn giấy xé ghi "1/10"); Tab Cách chơi có chấm trang, nút hết trang mờ. Cài đặt **"Hiệu ứng giấy": Đầy đủ / Giảm / Tắt**.
- **Bỏ dòng hướng dẫn màu vàng (từ 2.2, yêu cầu 24/11/2026):** hướng dẫn chỉ còn tranh (to hoặc thẻ nhỏ góc trái) + băng rôn "1/10"; lời "Xong!" cuối bài hiện thành thông báo một dòng.
- **Cốt truyện (từ 2.2):** gợi ý "Bấm để tiếp · Giữ để bỏ qua" và thanh giữ-để-bỏ-qua nằm ở **góc phải trên** trên nền mực (trước nằm ở đáy, đè lên hộp lời kể).
- **Cài đặt:** âm lượng tổng, nhạc, hiệu ứng (0–100); hiệu ứng truyện, hiệu ứng giấy (từ 2.2); độ nhạy chuột (mặc định 0,1°/điểm ảnh, Art Bible §9) và đảo trục dọc; toàn màn hình (mặc định) / cửa sổ, độ phân giải; lưu qua các lần mở.

### Lịch 22 ngày

| Vòng | Ngày | Sự kiện / "boss" | Trả nợ |
| --- | --- | --- | --- |
| 1 | 1–3 | Ngày 2 mưa lớn → ngày 3 nguyên liệu ít hơn và tệ hơn | Đầu ngày 4: **750G** |
| 2 | 4–8 | Ngày 8 sét đánh cột điện → mất điện, nấu bằng nến, tầm nhìn giảm | Đầu ngày 9: **2.900G** |
| 3 | 9–14 | Khách khó tính hơn hẳn; ngày 14 mới biết có kẻ tung tin xấu về quán; ngày 14 là ngày "all-in" | Đầu ngày 15: **10.500G** |
| 4 | 15–21 | Uy tín phục hồi, mọi thứ bình thường. Ngày 21, đúng **20:00** **chủ nợ** đến (độ dễ tính 5%) và ngồi tới khi được phục vụ | Đầu ngày 22: **36.000G** (1.4–1.6: 32.000G) |

Nợ thu vào **đầu** ngày đến hạn. Không đủ tiền = thua (kết thúc game). Nợ bản 1.3 là 400 / 2.200 / 8.200 / 26.200G; bản 1.4 tăng cả bốn mốc theo yêu cầu của bạn, mức cụ thể chọn bằng mô phỏng (mục 9).

---

## 3. Tiền tệ

| Loại | Dùng để | Nguồn |
| --- | --- | --- |
| Tiền (G) | Mua vật phẩm ở cửa hàng, trả nợ | Bán món ăn, tip, thưởng boss |
| Vé trắng | Gacha nguyên liệu, mua relic | **8 vé mỗi ngày** (từ ngày 2; từ 2.1, yêu cầu 23/11/2026 "mặc định mỗi ngày là 8 vé, trừ ngày 1 vẫn 5 vé"; 1.7–2.0: 7; 1.4–1.6: 6), không dùng thì cộng dồn; relic Vé số cào cộng thêm 1 vé vào ngày chia hết cho 3 |
| Vé đen | Gacha nguyên liệu cao cấp, hoặc mua relic tốt chắc chắn | Mua ở cửa hàng |

Khởi đầu: **100G**; **ngày 1 có 5 vé** (từ 1.7, yêu cầu của bạn 21/11/2026; trước đó 6 vé + 2 vé thưởng = 8). Cửa hàng relic mở từ **ngày 2** (ngày 1 là ngày làm quen). Dữ liệu: `start.first_day_tickets`.

Vé mỗi ngày tăng từ 3 lên 6 ở bản 1.4 vì thùng nhỏ lại (mục 4.2): một ngày kịp nấu khoảng 8 món, cần 4 vé nguyên liệu, còn lại để mua relic. Đây là số mình thêm để bù, cần bạn xác nhận (Q-22).

---

## 4. Nguyên liệu

### 4.1 Nhóm và bậc

| Bậc | Tên |
| --- | --- |
| 0 | Ôi thiu |
| 1 | Bình thường |
| 2 | Tốt |
| 3 | Tươi |
| 4 | Xuất sắc |
| 5 | Hiếm |

- **Đồ phụ** (rau, cà chua, củ quả…) và **nguyên liệu chính** (thịt, cá, tôm…): luôn ra khi gacha.
- **Nguyên liệu đột phá** (sơn hào hải vị: thịt cá mập, tôm hùm…): mặc định **0%**, chỉ relic mới mở; luôn ở bậc 4–5; dùng như nguyên liệu chính.

### 4.2 Một lượt gacha (1 vé trắng)
Bấm máy nhét vé ở Góc 2, máy thả xuống **1 thùng đồ phụ (3 món) + 1 thùng nguyên liệu chính (2 món)** lên bàn nhận hàng; nếu trúng thì thêm **1 thùng đột phá (1 món)**. **Mỗi thùng chỉ có một loại nguyên liệu** (ví dụ thùng 3 quả cà chua, thùng 2 miếng thịt bò); loại đồ phụ chọn ngẫu nhiên đều trong các loại đã mở khóa ở vòng hiện tại (mục 4.5); **từ 2.0 loại nguyên liệu chính (và đột phá) chọn đều trong các loại đã mở khóa nấu được với đồ phụ vừa ra** (mục 5.8: không phải cặp nào cũng có món) — mỗi lượt quay luôn có ít nhất một món, không có thùng "chết". Bàn nhận hàng dài, mỗi hàng 6 thùng. Mỗi món tự quay bậc độc lập theo bảng của vòng hiện tại. Nguyên liệu không dùng hết bị bỏ cuối ngày (tủ lạnh chỉ giữ món đã nấu). Trên bàn, nguyên liệu cùng loại nằm chung một thùng (ví dụ mọi quả cà chua của ngày nằm trong thùng cà chua); người chơi lấy từng món ra bằng tay.

### 4.3 Tỉ lệ bậc theo vòng (%)

| Bảng | Bậc 0 | Bậc 1 | Bậc 2 | Bậc 3 | Bậc 4 | Bậc 5 |
| --- | --- | --- | --- | --- | --- | --- |
| Vòng 1 (ngày 1–2) | 25 | 57 | 15 | 3 | 0 | 0 |
| Ngày 3 (sau mưa) | 50 | 42 | 8 | 0 | 0 | 0 |
| Vòng 2 (ngày 4–8) | 15 | 40 | 33 | 10 | 2 | 0 |
| Vòng 3 (ngày 9–14) | 10 | 28 | 35 | 20 | 6 | 1 |
| Vòng 4 (ngày 15–21) | 6 | 18 | 33 | 28 | 12 | 3 |
| Vé đen (mọi vòng) | 0 | 0 | 0 | 62 | 30 | 8 |

Ngày 3 (sau mưa) thùng cũng nhỏ lại: **2 đồ phụ + 1 nguyên liệu chính**.

Bảng tỉ lệ bậc ở trên **giữ nguyên** ở bản 1.4: tỉ lệ tính cho từng món nên không phụ thuộc cỡ thùng. Một vé giờ cho 5 món thay vì 8, nhưng mỗi ngày có 6 vé thay vì 3 (30 món so với 24). Nếu bạn muốn đổi cả bảng tỉ lệ thì báo (Q-23).

**Cách dựng bảng (theo gợi ý TFT):** mỗi vòng giống một cấp trong TFT — khối xác suất trượt dần lên bậc cao, bậc hiếm nhất chỉ xuất hiện muộn. Vòng 2 có 2% bậc 4 giống TFT cấp 5 có 2% tướng 4 vàng; vòng 3 có 1% bậc 5 giống TFT cấp 7 có 1% tướng 5 vàng; vòng 4 có 3% bậc 5 giống TFT cấp 8 có 3% tướng 5 vàng (số liệu TFT ở mục 10).

**Vé đen:** bạn đề xuất 60/30/10; mình chỉnh thành **62/30/8** vì ở bản 1.0 một vé đen cho 8 món, với 10% bậc 5 thì 57% số vé đen có ít nhất một món hiếm — quá dễ. Ở bản 1.4 một vé đen cho 5 món; với 8% thì 34% số vé đen có ít nhất một món hiếm.

### 4.4 Thùng đột phá
- Tỉ lệ xuất hiện mỗi lượt: **0% + tổng từ relic** (xem mục 7).
- Mỗi thùng: **1 món**, **70% bậc 4 / 30% bậc 5** (relic có thể nâng tỉ lệ bậc 5; thưởng bộ Sơn Hào 3 món thêm 1 món thành 2).

### 4.5 Danh sách nguyên liệu

Thời gian giữ để sơ chế ở bản 1.4 gấp 3 bản 1.3 (yêu cầu "tăng thời gian chuẩn bị").

Nguyên liệu mở dần theo vòng để người chơi học từng ít một: vòng 1 có 3 loại đồ phụ × 3 loại chính (**6 món ăn** từ 2.0; 1.9: 9), vòng 2: 18 món, vòng 3: 34 món, vòng 4 có 10 × 8 loại (**56 món ăn**; 1.9: 80).

**Đồ phụ** (rau, củ, quả)

| Nguyên liệu | Sơ chế (thớt) | Thời gian giữ | Mở khóa |
| --- | --- | --- | --- |
| Cà chua | Thái múi | 4,5 giây | Vòng 1 |
| Hành tây | Thái lát | 4,5 giây | Vòng 1 |
| Rau muống | Nhặt, rửa | 3,0 giây | Vòng 1 |
| Khoai tây | Gọt, thái | 6,0 giây | Vòng 2 |
| Nấm rơm | Rửa, bổ đôi | 3,0 giây | Vòng 2 |
| Bông cải xanh | Tách bông | 4,5 giây | Vòng 3 |
| Ớt chuông | Bỏ hạt, thái | 4,5 giây | Vòng 3 |
| Bí đỏ | Gọt, thái | 6,0 giây | Vòng 4 |
| Măng tây | Cắt khúc | 3,0 giây | Vòng 4 |
| Nấm đông cô | Thái lát | 3,0 giây | Vòng 4 |

**Nguyên liệu chính** (thịt, cá, tôm, trứng, đậu)

| Nguyên liệu | Sơ chế (thớt) | Thời gian giữ | Mở khóa |
| --- | --- | --- | --- |
| Trứng gà | Đập, đánh | 1,5 giây | Vòng 1 |
| Thịt heo | Thái mỏng | 4,5 giây | Vòng 1 |
| Đậu phụ | Cắt miếng | 3,0 giây | Vòng 1 |
| Thịt bò | Thái mỏng | 4,5 giây | Vòng 2 |
| Tôm | Bóc vỏ | 6,0 giây | Vòng 2 |
| Cá hồi | Lọc xương | 7,5 giây | Vòng 3 |
| Mực | Làm sạch, khứa | 7,5 giây | Vòng 3 |
| Gà ta | Chặt miếng | 6,0 giây | Vòng 4 |

**Nguyên liệu đột phá** (sơn hào hải vị — chỉ ra qua relic, luôn bậc 4–5, dùng thay nguyên liệu chính)

| Nguyên liệu | Sơ chế (thớt) | Thời gian giữ |
| --- | --- | --- |
| Tôm hùm | Bổ đôi | 9,0 giây |
| Bò Wagyu | Cắt miếng dày | 6,0 giây |
| Cá ngừ vây xanh | Lọc thăn | 7,5 giây |
| Cua hoàng đế | Tách mai | 9,0 giây |

Bạn đề xuất "thịt cá mập"; mình thay bằng **cá ngừ vây xanh** vì sản phẩm từ cá mập (nhất là vi cá) bị cấm hoặc hạn chế buôn bán ở nhiều nơi như Mỹ và Canada, dễ gây phản ứng tiêu cực từ người chơi quốc tế. Đổi lại nếu bạn muốn giữ.

**Đồ kho** (vô hạn, không có bậc, không tốn vé): Dầu ăn, Bơ, Tỏi, Nước mắm, Nước tương, Muối, Mật ong, Nước dùng, Hành lá. **Đế món** (lấy khi múc ra đĩa): Cơm, Mì Ý, Bún, Bánh mì.

---

## 5. Nấu ăn và món ăn

### 5.1 Khu bếp (Góc 4)
Một món đi qua các bước sau; mọi bước là đồ vật thật trong phòng.
1. **Lấy nguyên liệu:** cầm một món từ thùng ở Góc 2.
2. **Trái — thớt:** đặt nguyên liệu lên thớt, giữ chuột để sơ chế (thời gian theo mục 4.5); thanh tiến độ nằm ngay trên thớt. Xong thì cầm lên. Có sẵn **1 thớt**; mua thêm ở cửa hàng (mục 8) tới **3 thớt**, bàn sơ chế tự dài ra; mỗi thớt sơ chế riêng (giữ chuột trên thớt nào thì thớt đó chạy).
3. **Giữa — bếp:** có sẵn **1 bếp**; mua thêm ở cửa hàng (mục 8) tới **2 bếp** — bếp thứ hai đặt bên phải quầy đĩa, có núm, bảng tỉ lệ và đồ trong nồi riêng, hai bếp nấu độc lập (từ 2.1). Chọn dụng cụ trên giá treo (**Chảo**, **Nồi** hoặc **Vỉ nướng**; giá chung, dụng cụ vào bếp vừa dùng nếu bếp đó còn đổi được, không thì bếp trống đầu tiên); dụng cụ hiện lên bếp. Cho từng thứ vào **đúng thứ tự**: nguyên liệu đang cầm thì bấm vào bếp, đồ kho thì bấm lọ trên kệ gia vị. Vặn **núm bếp** để nấu, rồi chờ.
4. **Phải — đĩa:** lấy một đĩa trống ở chồng đĩa (đĩa có sẵn đế món: cơm, mì Ý, bún hoặc bánh mì theo công thức), bấm vào bếp để múc món ra; đĩa đặt lên quầy chờ.
5. **Thùng rác:** đổ bỏ những gì đang ở trên bếp.
- **Quầy chờ:** giữ tối đa **3 đĩa** đã nấu trong ngày, không giảm bậc. Cuối ngày đĩa còn lại vào tủ lạnh (nếu còn ô) hoặc bị bỏ.
- **Phục vụ (từ 2.0, bỏ chuông):** cầm đĩa **đặt lên bàn trước mặt khách** (hoặc bấm vào khách) — phục vụ khách nào trước cũng được, không theo thứ tự đến.

Mỗi món = **1 nguyên liệu chính (hoặc đột phá) + 1 đồ phụ + đồ kho**, và cặp (chính, phụ) phải **có trong thực đơn** (mục 5.8, từ 2.0: bếp không nhận cặp không có món). Cho sai thứ tự → món ra **Lạc vị**. Đồ kho không ảnh hưởng bậc món; chúng tạo ra thứ tự cần nhớ.

### 5.2 Bậc món ăn

| Bậc | Tên | Hệ số giá |
| --- | --- | --- |
| 0 | Lạc vị | ×0,2 |
| 1 | Dở | ×0,5 |
| 2 | Bình thường | ×1,0 |
| 3 | Ngon | ×1,8 |
| 4 | Tuyệt hảo (= "hoàn hảo") | ×3,2 |
| 5 | Huyền thoại (từ 2.1: mọi món, xem 5.4) | ×6,0 |

**Giá niêm yết (từ 2.0, yêu cầu 22/11/2026):** mỗi món có **một giá ở bậc Bình thường** (ghi trong thực đơn, `recipes.json price`); bậc khác = giá niêm yết × hệ số bậc ở bảng trên (tỉ lệ thuận). Bỏ "giá gốc theo vòng" (1.9: 60 / 70 / 85 / 100G cho mọi món). Cách đặt giá (`make_next.py`): món mở ở vòng r có giá sao cho trung bình mọi món làm được ở vòng r = giá gốc cũ × **0,95** (vòng 1 → 4: 58 / 68 / 82 / 95G; 2.0: ×1,05); trong cùng vòng, món làm lâu hơn đắt hơn (theo căn bậc hai thời gian), làm tròn 5G; món đột phá **100G**. Hệ số chọn bằng bot C# (mục 9): từ 2.1 tỉ lệ 6 bậc + 8 vé / ngày làm game dễ hơn nên hạ từ 1,05 xuống 0,95.

| Mở khóa | Giá niêm yết |
| --- | --- |
| Vòng 1 (6 món) | Trứng chiên 55G · Heo xào, Đậu phụ sốt 60G |
| Vòng 2 (12 món mới) | 60–75G |
| Vòng 3 (16 món mới) | 80–110G |
| Vòng 4 (22 món mới) | 95–135G |
| Đột phá (28 món) | 100G |

### 5.3 Công thức ra bậc món (hiển thị cho người chơi khi đang nấu)

**Từ 2.1 (bảng bạn đưa 23/11/2026, "kết hợp với spec hiện tại"):** mọi tổ hợp nguyên liệu đều có thể ra đủ 6 bậc. Nguyên liệu tốt hơn không đổi hai bậc liền kề như 1.0–2.0, mà đẩy cả phân bố lên trên. Code: `balance/cookodds.py` (kiểm bảng) và `Core/Cooking.cs DishFormula` (game); số ở `balance.json dish.odds`.

```
S = 0,6 × bậc nguyên liệu chính + 0,4 × bậc đồ phụ          (thang 0–5)
b = S + 0,8 (+ relic "điểm món") (− 0,3 vào ngày mất điện)

1) Huyền thoại (ngăn vé riêng):
   L = 0,001% × 2000^(S/5)        → 0,001% ở S = 0, tối đa 2% ở S = 5
   L = min(5%, L + relic Huyền thoại)
2) Năm bậc còn lại chia (100% − L) theo một hình dạng trượt theo b:
   mẫu chuẩn (chính 1 + phụ 1, b = 1,8): Lạc vị 4,95 · Dở 17,05 · Bình thường 74,35 · Ngon 13,25 · Tuyệt hảo 9,15 (Huyền thoại 2,1 tách sang ngăn riêng)
   Ngoài mẫu: mỗi bậc xa hơn giảm e^−3 (≈ 20 lần); b lẻ: nội suy log giữa hai bậc liền kề; phần tràn khỏi thang dồn vào Lạc vị / Tuyệt hảo.
   Relic Ớt ma: 10% thành Lạc vị (phần còn lại × 0,9); Chảo gang: phần dưới Dở dồn vào Dở (bậc Lạc vị bị chặn hẳn).
3) Mỗi bậc tối thiểu 0,001% (trừ khi relic chặn hẳn bậc đó).
4) Lưu trên lưới 100.000 vé nguyên, làm tròn theo phần dư lớn nhất → mỗi hàng cộng đúng 100%; món quay một vé trên lưới.
```

Đã kiểm bằng script (`cookodds.py`) và test (`Sprint11Tests.C05_*`): khớp bảng dưới; với mọi cặp, nguyên liệu tốt hơn không bao giờ làm giảm xác suất ra "bậc k trở lên".

Bảng hiển thị trên nồi (%):

| Nguyên liệu | b | Lạc vị | Dở | Bình thường | Ngon | Tuyệt hảo | Huyền thoại |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Chính 0 + phụ 0 | 0,80 | 18,53 | 62,61 | 11,16 | 7,32 | 0,384 | 0,001 |
| Chính 0 + phụ 1 | 1,20 | 12,81 | 45,26 | 28,98 | 11,29 | 1,66 | 0,002 |
| Chính 1 + phụ 1 (mẫu chuẩn) | 1,80 | 4,17 | 14,36 | 62,61 | 11,16 | 7,70 | 0,005 |
| Chính 2 + phụ 1 | 2,40 | 0,902 | 8,68 | 33,89 | 41,13 | 15,38 | 0,012 |
| Chính 2 + phụ 2 | 2,80 | 0,208 | 3,96 | 14,36 | 62,60 | 18,86 | 0,021 |
| Chính 3 + phụ 2 | 3,40 | 0,045 | 0,857 | 8,68 | 33,88 | 56,49 | 0,052 |
| Chính 3 + phụ 3 | 3,80 | 0,01 | 0,197 | 3,96 | 14,34 | 81,40 | 0,096 |
| Chính 4 + phụ 2 | 4,00 | 0,007 | 0,128 | 2,57 | 13,12 | 84,04 | 0,13 |
| Chính 4 + phụ 4 | 4,80 | 0,001 | 0,01 | 0,196 | 3,94 | 95,41 | 0,437 |
| Chính 5 + phụ 4 (đột phá) | 5,40 | 0,001 | 0,002 | 0,042 | 0,848 | 98,02 | 1,09 |
| Chính 5 + phụ 5 | 5,80 | 0,001 | 0,001 | 0,01 | 0,193 | 97,80 | 2,00 |

Các trường hợp đặc biệt:

| Tình huống | Lạc vị | Dở | Bình thường | Ngon | Tuyệt hảo | Huyền thoại |
| --- | --- | --- | --- | --- | --- | --- |
| Mẫu chuẩn, ngày mất điện (b − 0,3) | 8,03 | 28,28 | 47,26 | 12,45 | 3,97 | 0,005 |
| Mẫu chuẩn + relic Sao băng trong nồi (+0,3%) | 4,16 | 14,31 | 62,42 | 11,12 | 7,68 | 0,30 |
| Chính 4 + phụ 4 + cả 2 relic (+1,3%) | 0,001 | 0,01 | 0,19 | 3,89 | 94,17 | 1,74 |
| Chính 0 + phụ 0 + Chảo gang bà ngoại (không dưới Dở) | 0 | 81,14 | 11,16 | 7,32 | 0,38 | 0,001 |

Số trong bảng do lưới vé làm tròn có thể lệch bảng bạn đưa ở chữ số cuối (vd 7,70 / 7,71; 0,384 / 0,38).

**Cách hiển thị:** thanh 6 màu trên bảng bếp, cập nhật ngay khi đổi nguyên liệu hoặc relic. Bậc từ 1% trở lên hiện 2 chữ số thập phân; bậc dưới 1% hiện đủ chữ số (ví dụ 0,005%). Không bao giờ hiện "0%" cho một bậc vẫn có thể ra (bậc bị relic chặn hiện 0%).

1.0–2.0 (bỏ): món ra bậc floor(b) hoặc floor(b)+1, xác suất lên bậc trên = phần lẻ của b, kẹp trong [Lạc vị, Tuyệt hảo].

### 5.4 Món Huyền thoại
**Từ 2.1:** Huyền thoại là bậc cao nhất, **món nào cũng có thể đạt** (1.0–2.0: chỉ một món, chỉ qua nước sốt từ ngày 9). Ba đường để ra Huyền thoại:
- **Khi nấu, từ ngày 1:** 0,001% với nguyên liệu tệ nhất, tăng theo bậc nguyên liệu, tối đa 2% (mục 5.3).
- **Relic:** chỉ relic mới đẩy tỉ lệ khi nấu vượt 2%, tổng tối đa 5%: Sao băng trong nồi (+0,3%), Bí kíp gia truyền (+1,0%) (mục 7.3).
- **Nước sốt thần thánh:** áp lên món đã nấu, giữ bảng 5.5 (4% từ ngày 9).

Món Huyền thoại giữ tên món (đĩa viền vàng, hào quang, giá ×6,0). Tên "Món Hy Vọng" giữ làm tên gọi chung cho món đạt bậc Huyền thoại trong cốt truyện. Nước sốt / máy gacha "+1, +2" không kéo món Huyền thoại xuống Tuyệt hảo.

### 5.5 Nước sốt thần thánh của Hope
Dùng lên một món đã nấu. Tỉ lệ hiện cho người chơi.

| Kết quả | Ngày 4–8 | Từ ngày 9 |
| --- | --- | --- |
| Hỏng → Lạc vị | 12% | 12% |
| Giữ nguyên | 18% | 16% |
| +1 bậc | 50% | 48% |
| +2 bậc | 20% | 20% |
| Thành món Huyền thoại | 0% | 4% |

Tỉ lệ hỏng giữ dưới 20% như yêu cầu. Từ ngày 9, xác suất có ít nhất một món Huyền thoại sau 10 lần dùng là khoảng 33% — cần "nhân phẩm cao".

**Cách dùng (từ 2.1, sửa lỗi bạn báo 23/11/2026 "bấm vào lọ thì mua thêm, không biết dùng"):** mua ở Tab → Cửa hàng; lọ sốt hiện **ở bên phải bếp** (mặt bếp, góc sau). Bấm lọ để **cầm lọ** (bấm lọ không bao giờ mua thêm), rồi bấm **đĩa vừa nấu** trên quầy (hoặc món trong tủ lạnh) để nhỏ một chai; chuột phải để đặt lại. Đang cầm đĩa mà bấm lọ thì nhỏ lên đĩa đang cầm (như 1.8). Lần đầu có sốt có bài hướng dẫn bằng hình (mục 2.1).

**Hiệu ứng (từ 2.1):** lên **Huyền thoại** → cầu vồng 7 sắc bùng nổ (7 vòng màu nở ra, hạt 7 màu bắn tung, chớp sáng, tiếng nổ + chuỗi 7 nốt); xuống **Lạc vị** → quả bom xịt (bom đen ngòi cháy xèo xèo, rung, rồi "pfff" xẹp xuống với cụm khói xám nhỏ, rung camera). Món nấu ra Huyền thoại cũng có hiệu ứng cầu vồng.

### 5.6 Máy gacha đồ ăn (tỉ lệ ẩn)
Đổi bậc một món đã nấu, lên hoặc xuống. Mỗi món chỉ được đưa vào máy **1 lần**; tối đa **3 lần mỗi ngày**; không tốn vé.

**Cách dùng (từ 2.2, sửa lỗi bạn báo 24/11/2026 "bỏ đồ ăn vào mà đồ ăn trên tay không mất, không biết đã gacha chưa"):** máy vẽ lại kiểu **lò vi sóng** (cửa kính, lòng máy sáng, mâm xoay, màn hình "còn x/3 lượt", 3 đèn trên nóc). Cầm đĩa bấm máy → đĩa **rời tay, nằm trên mâm trong máy** (nhìn thấy qua cửa). **Gạt cần bên phải** → mâm xoay, đèn nhấp nháy, máy rung 1,8 giây → bảng kết quả: **"THÀNH CÔNG! +n bậc"** (xoáy vàng; +3 hoặc ra Huyền thoại là cầu vồng), **"XỊT!"** khi tụt bậc / hỏng (bom xịt, rung camera), "Giữ nguyên". Bấm máy để lấy đĩa ra (hỏng thì đĩa mất).

| Kết quả (ẩn với người chơi) | Tỉ lệ |
| --- | --- |
| Hỏng, mất món | 15% |
| −1 bậc | 20% |
| Giữ nguyên | 25% |
| +1 bậc | 28% |
| +2 bậc | 10% |
| +3 bậc | 2% |

Nếu không hỏng, trung bình món được +0,4 bậc; nhưng 15% mất trắng món, nên chỉ đáng dùng cho món bậc thấp — đủ rủi ro để người chơi cân nhắc.

### 5.7 Tủ lạnh
Giữ món đã nấu để dùng sau (ví dụ để dành món Tuyệt hảo cho khách khó tính hoặc chủ nợ ngày 21). Mặc định **2 ô**. Món để quá **3 ngày** bị tụt 1 bậc. Relic "Tủ lạnh 2 cửa" thêm 2 ô và bỏ việc tụt bậc.

### 5.8 Công thức và danh sách món

Tên và công thức của món được ghép từ **nguyên liệu chính** (quyết định cách nấu, dụng cụ, đế món) và **đồ phụ** (điền vào chỗ ‹đồ phụ›). **Từ 2.0 (yêu cầu 22/11/2026 "giảm bớt 30% số công thức món ăn"):** chỉ ~70% cặp có món — **84 món** (56 thường + 28 đột phá; 1.9: 120). Mỗi đồ phụ có ít nhất một món ngay ở vòng nó mở khóa; gacha chỉ ghép thùng chính nấu được với thùng phụ (mục 4.2) nên vẫn không có thùng "chết". Bảng cặp nằm ở `balance/make_next.py` (`PAIRS`), đầy đủ ở `balance/recipes.json`:

| Nguyên liệu chính | Đồ phụ có món |
| --- | --- |
| Trứng gà | cà chua, hành tây, khoai tây, nấm rơm, ớt chuông, măng tây, nấm đông cô |
| Thịt heo | hành tây, rau muống, khoai tây, nấm rơm, bông cải, ớt chuông, bí đỏ, nấm đông cô |
| Đậu phụ | cà chua, hành tây, nấm rơm, bông cải, bí đỏ, nấm đông cô |
| Thịt bò | cà chua, hành tây, rau muống, khoai tây, bông cải, ớt chuông, măng tây, nấm đông cô |
| Tôm | hành tây, rau muống, nấm rơm, bông cải, bí đỏ, măng tây |
| Cá hồi | cà chua, khoai tây, nấm rơm, bông cải, ớt chuông, măng tây, nấm đông cô |
| Mực | hành tây, nấm rơm, bông cải, ớt chuông, măng tây, nấm đông cô |
| Gà ta | cà chua, hành tây, khoai tây, bông cải, ớt chuông, bí đỏ, măng tây, nấm đông cô |
| Tôm hùm | cà chua, hành tây, khoai tây, bông cải, ớt chuông, măng tây, nấm đông cô |
| Bò Wagyu | hành tây, khoai tây, nấm rơm, ớt chuông, bí đỏ, măng tây, nấm đông cô |
| Cá ngừ vây xanh | cà chua, hành tây, rau muống, nấm rơm, bông cải, ớt chuông, măng tây |
| Cua hoàng đế | cà chua, hành tây, rau muống, nấm rơm, bí đỏ, măng tây, nấm đông cô |

**Công thức món đột phá (từ 2.0):** có trong Thực đơn (Tab) từ khi có rương đột phá (relic Sơn Hào) hoặc nguyên liệu đột phá trong kho; trước đó hiện một dòng khoá "Sơn hào hải vị · cần rương đột phá".

| Món | Nguyên liệu chính | Dụng cụ | Thứ tự cho vào | Đế món | Thời gian nấu | Mở khóa |
| --- | --- | --- | --- | --- | --- | --- |
| Trứng chiên *‹đồ phụ›* | Trứng gà | Chảo | Dầu ăn → *(đồ phụ)* → Trứng gà → Nước mắm | Cơm | 12 giây | Vòng 1 |
| Heo xào *‹đồ phụ›* | Thịt heo | Chảo | Dầu ăn → Tỏi → Thịt heo → *(đồ phụ)* | Cơm | 15 giây | Vòng 1 |
| Đậu phụ sốt *‹đồ phụ›* | Đậu phụ | Nồi | Dầu ăn → *(đồ phụ)* → Đậu phụ → Nước mắm | Cơm | 18 giây | Vòng 1 |
| Mì Ý bò xào *‹đồ phụ›* | Thịt bò | Chảo | Bơ → Tỏi → Thịt bò → *(đồ phụ)* | Mì Ý | 15 giây | Vòng 2 |
| Bún xào tôm *‹đồ phụ›* | Tôm | Chảo | Dầu ăn → Tôm → *(đồ phụ)* → Nước mắm | Bún | 15 giây | Vòng 2 |
| Cá hồi áp chảo *‹đồ phụ›* | Cá hồi | Chảo | Bơ → Cá hồi → *(đồ phụ)* → Muối | Cơm | 21 giây | Vòng 3 |
| Mực xào *‹đồ phụ›* | Mực | Chảo | Dầu ăn → Tỏi → Mực → *(đồ phụ)* | Cơm | 12 giây | Vòng 3 |
| Gà nướng mật ong *‹đồ phụ›* | Gà ta | Vỉ nướng | Mật ong → Gà ta → *(đồ phụ)* | Bánh mì | 27 giây | Vòng 4 |
| Tôm hùm nướng bơ tỏi *‹đồ phụ›* | Tôm hùm | Vỉ nướng | Bơ → Tỏi → Tôm hùm → *(đồ phụ)* | Bánh mì | 30 giây | Relic |
| Bò Wagyu áp chảo *‹đồ phụ›* | Bò Wagyu | Chảo | Muối → Bò Wagyu → Bơ → *(đồ phụ)* | Cơm | 21 giây | Relic |
| Cá ngừ vây xanh tataki *‹đồ phụ›* | Cá ngừ vây xanh | Chảo | Dầu ăn → Cá ngừ vây xanh → *(đồ phụ)* → Nước tương | Cơm | 12 giây | Relic |
| Cua hoàng đế hấp *‹đồ phụ›* | Cua hoàng đế | Nồi | Nước dùng → Cua hoàng đế → *(đồ phụ)* → Hành lá | Bún | 30 giây | Relic |

Ngoại lệ tên: Thịt bò + Cà chua = **Mì Ý bò sốt cà chua** (món xuất hiện trong bản phác).

**Chín món vòng 1 (bảng 1.9; từ 2.0 bỏ Trứng chiên rau muống, Heo xào cà chua, Đậu phụ sốt rau muống — còn 6):**

| Món | Thứ tự cho vào | Dụng cụ | Nấu | Tổng thời gian một món* |
| --- | --- | --- | --- | --- |
| Trứng chiên cà chua | Dầu ăn → Cà chua → Trứng gà → Nước mắm | Chảo | 12 giây | 22,5 giây |
| Trứng chiên hành tây | Dầu ăn → Hành tây → Trứng gà → Nước mắm | Chảo | 12 giây | 22,5 giây |
| Trứng chiên rau muống | Dầu ăn → Rau muống → Trứng gà → Nước mắm | Chảo | 12 giây | 21,0 giây |
| Heo xào cà chua | Dầu ăn → Tỏi → Thịt heo → Cà chua | Chảo | 15 giây | 28,5 giây |
| Heo xào hành tây | Dầu ăn → Tỏi → Thịt heo → Hành tây | Chảo | 15 giây | 28,5 giây |
| Heo xào rau muống | Dầu ăn → Tỏi → Thịt heo → Rau muống | Chảo | 15 giây | 27,0 giây |
| Đậu phụ sốt cà chua | Dầu ăn → Cà chua → Đậu phụ → Nước mắm | Nồi | 18 giây | 30,0 giây |
| Đậu phụ sốt hành tây | Dầu ăn → Hành tây → Đậu phụ → Nước mắm | Nồi | 18 giây | 30,0 giây |
| Đậu phụ sốt rau muống | Dầu ăn → Rau muống → Đậu phụ → Nước mắm | Nồi | 18 giây | 28,5 giây |

\*Tổng = sơ chế 2 nguyên liệu + 0,5 giây mỗi lần cho vào + thời gian nấu + 1 giây múc + 1,5 giây mang ra và bấm chuông. **Chưa tính thời gian đi lại trong phòng 3D** (mô phỏng giả định thêm 20–30 giây mỗi món, mục 9).

**Thực đơn (từ 2.0, thay sách nấu ăn ở Góc 3):** bảng Tab → Thực đơn. Trái: danh sách món đã mở (hình món + tên + giá niêm yết). Phải: trang món — tên, **giá niêm yết** (bậc Bình thường), hình thành phẩm + đế món, hàng **CẦN** (icon nguyên liệu chính ×1, đồ phụ ×1, dụng cụ), thời gian nấu, rồi các khung bước đánh số: ① chọn dụng cụ → ② … cho từng thứ theo thứ tự → bước cuối vặn núm ra món; ghi chú "món ngon hơn → đắt hơn". (1.3–1.9: sách trong phòng ghi giá theo từng bậc — bỏ theo yêu cầu 22/11/2026.)

### 5.9 Thời gian nấu và quá lửa

| Cách nấu | Dụng cụ | Thời gian nấu |
| --- | --- | --- |
| Chiên (trứng), xào mực, tataki (cá ngừ) | Chảo | 12 giây |
| Xào (heo, bò, tôm) | Chảo | 15 giây |
| Sốt (đậu phụ) | Nồi | 18 giây |
| Áp chảo (cá hồi, bò Wagyu) | Chảo | 21 giây |
| Nướng (gà) | Vỉ nướng | 27 giây |
| Nướng (tôm hùm), hấp (cua) | Vỉ nướng, Nồi | 30 giây |

- Thời gian nấu ở bản 1.4 gấp 1,5 bản 1.3 (yêu cầu "tăng thời gian nấu").
- **Một món mất trung bình 27–31 giây thao tác** từ lúc sơ chế đến lúc bấm chuông (nhanh nhất 21 giây, chậm nhất 43 giây; món đột phá 27–50 giây), **cộng thời gian đi lại** trong phòng. Mô phỏng giả định cả đi lại là 47–61 giây một món, tức một ngày 8 phút kịp khoảng 8 món với người nhanh, 6–7 món với người chậm.
- **Quá lửa (từ vòng 2):** món để trên bếp quá **1,5 lần** thời gian nấu thì hiện cảnh báo; quá **2 lần** thì **−1 bậc**. Vòng 1 không có quá lửa.
- **Ngày 8 (mất điện):** nấu bằng nến, mọi thời gian nấu **+25%**.

Về tra cứu: các wiki của Overcooked và PlateUp mô tả cấu trúc công thức (sơ chế → nấu trong chảo/nồi → múc ra đĩa, ghép đế với sốt) nhưng không công bố số giây. Số liệu có giây rõ ràng mình tìm được là Sea of Thieves (cá chín ở 40 giây và cháy ở 80 giây; thịt chín ở 60 giây và cháy ở 120 giây) — đó là nguồn của hệ số quá lửa ×2. Thời gian nấu của game này ngắn hơn nhiều vì một ngày chỉ khoảng 6 phút.

---

## 6. Khách hàng

### 6.1 Chỗ ngồi và mở quán
Phục vụ tối đa **3 khách cùng lúc**; khách có thể đến lần lượt hoặc 3 người liền nhau. **Vòng 1: khách đến lần lượt**, người sau vào khi người trước đã được phục vụ và ăn xong (mục 6.2, từ 1.5). **Quầy phục vụ (từ 2.2, yêu cầu 24/11/2026 "mở rộng bàn … khách hàng bị nhỏ đi"):** quầy dài **6,6 m** (trước 4,3 m), ba ghế cách nhau **2 m** (trước 1,45 m), khách khi 3 người cùng ngồi ở tỉ lệ **0,88** (trước 0,72); bỏ chuông trang trí, chậu hoa và hũ tip dời ra hai đầu quầy để không che đĩa (quy cách hình, Q-87).

**Mở quán:** đầu ngày quán đóng, người chơi gacha và nấu trước. Bấm **biển Mở quán** ở Góc 1 thì khách mới bắt đầu đến. Khi quán đang mở, bấm biển lần nữa để **đóng quán sớm**: khách còn lại ra về, không trả tiền, không bị phạt, và hết ngày.

**Đóng quán (từ 2.0, yêu cầu 22/11/2026 "bỏ cơ chế tự động qua ngày"):** ngày **chỉ hết khi người chơi bấm biển**. Qua 21:00 và không còn khách ngồi / đang ăn, biển đổi thành **"ĐÓNG QUÁN"** và HUD nhắc; quán vẫn mở (không có khách mới) cho tới khi bấm.

### 6.2 Khách không giới hạn, tới 21:00 (từ 1.4)

Không còn số khách cố định mỗi ngày (bản 1.3: 6 / 8 / 5 / 9). Số khách phục vụ được do **thời gian** và **nguyên liệu** quyết định.

- **Đồng hồ (từ 1.7):** mỗi ngày đồng hồ **đứng ở 9:00 tới khi bấm Mở quán**, rồi chạy liên tục tới 21:00 (8 phút thực). Gacha, sơ chế, nấu trước khi mở không mất giờ; HUD hiện "9:00 chưa mở quán". Không mở quán thì ngày chưa bắt đầu (không tự hết ngày). Chủ nợ ngày 21 tới lúc 20:00 tính từ lúc mở. (Q-21 đóng: bạn chọn 21/11/2026.)
- **Khách chỉ đến khi quán mở** (biển Mở quán, mục 6.1). Mở sớm thì có nhiều giờ bán hơn; mở muộn để gacha và nấu sẵn thì mất giờ bán.
- **Vòng 1 — lần lượt:** mỗi lúc một khách; khách kế tiếp vào **3 giây** sau khi khách trước được phục vụ và ăn xong. Cứ thế tới 21:00.
- **Từ vòng 2 — nhiều khách:** tối đa 3 khách cùng ngồi; ghế nào trống thì 3 giây sau có khách mới ngồi vào, **nhưng (từ 1.8) hai khách liền nhau vào cách nhau ít nhất `max(6, 100 / ngày)` giây** (`day.arrival`): ngày 4 là 25 giây, ngày 8 là 12,5 giây, từ ngày 17 trở đi 6 giây — tốc độ khách đến tăng theo ngày (yêu cầu của bạn 21/11/2026: "ngày 4 vòng 2 mà 1 giây đã 3 người vào cùng lúc sẽ bị ngợp"). Mở quán: chỉ khách đầu vào ngay. Cứ thế tới 21:00.
- **Khách đi bộ (từ 1.9, chỉ là hình):** khách mới đi dọc vỉa hè từ bên trái hoặc phải (theo hạt giống của khách), rẽ vào cửa Góc 1 rồi tới ghế; ăn xong đi ra, rẽ trái hoặc phải dọc vỉa hè tới khuất sau tường mới biến mất (≈ 4–6 giây mỗi lượt). Giờ chờ (mục 6.6) vẫn tính từ lúc khách xuất hiện như 1.8; thêm ≤ 3% so với khoảng an toàn nên không đổi số (Q-65).
- **Khách ăn (từ 1.5):** phục vụ xong, khách **ngồi ăn tại ghế 5 giây** (`day.eat_s`) rồi mới rời ghế; tiền và tip trả ngay lúc nhận món. Khách đang ăn không gọi món, không nhận đĩa khác. "Ghế trống" ở các dòng trên và dưới tính từ lúc khách ăn xong (khách mới vào 3 giây sau đó); chủ nợ đang đợi ghế thì vào ngay khi khách ăn xong.
- **21:00:** không nhận khách mới. Khách đang ngồi vẫn được phục vụ; phục vụ xong người cuối và người đó ăn xong (hoặc không còn ai ngồi) thì **bấm biển Đóng quán để hết ngày** (từ 2.0; 1.9: tự hết ngày).
- **Hết nguyên liệu và hết vé** khi còn khách: bấm biển để đóng quán sớm (mục 6.1).
- **Ngày 21:** đúng 20:00 chủ nợ đến, chiếm một ghế và ngồi tới khi được phục vụ.
- **Ngày 14 "all-in":** không còn khác các ngày vòng 3 về số khách; "all-in" nghĩa là ngày cuối trước hạn nợ 10.500G, nên dốc món trong tủ lạnh, nước sốt, vé đen. *(cần bạn xác nhận)*

### 6.3 Độ dễ tính (%)
Hiển thị bằng %, **càng cao càng dễ tính**, mốc trung bình 50%. Mỗi khách quay một giá trị trong khoảng:

| Vòng | 60–95% (dễ) | 35–59% (vừa) | 10–34% (khó) |
| --- | --- | --- | --- |
| 1 | 90% | 10% | 0% |
| 2 | 75% | 22% | 3% |
| 3 | 30% | 50% | 20% |
| 4 | 60% | 32% | 8% |
| Ngày 21, chủ nợ | — | — | cố định **5%** |

Vòng 1: 90% khách trên 60%, khoảng 10% khách dưới 60% đúng như yêu cầu.

### 6.4 Món tối thiểu khách chấp nhận

| Độ dễ tính | Món tối thiểu |
| --- | --- |
| ≥ 80% | Dở |
| 60–79% | Bình thường |
| 35–59% | Ngon |
| < 35% (kể cả chủ nợ) | Tuyệt hảo (Huyền thoại cũng đạt) |

### 6.5 Trả tiền và tip
Gọi `chênh = bậc món − bậc tối thiểu`, `khó = 1 − độ dễ tính`, `giá = giá gốc × hệ số bậc món`.

| Trường hợp | Khách trả |
| --- | --- |
| chênh ≥ 0 | `giá` + tip = `giá × khó × (0,3 + 0,2 × chênh)` |
| chênh = −1 | 50% `giá`, không tip |
| chênh ≤ −2 | Không trả, quán **bị trừ 30% giá niêm yết của món** (đền bù; 1.9: 30% giá gốc của vòng) |
| **Sai món** (đĩa khác món khách gọi, mục 6.7) | Khách nhận đĩa nhưng giận: không trả, quán **bị trừ 30% giá niêm yết của món khách gọi** — bất kể bậc món |

Hệ quả đúng ý bạn: khách khó tính được món bậc cao → tip nhiều; khách dễ tính được món bậc cao → tip ít; món dưới yêu cầu xa → bị trừ tiền.

### 6.6 Thời gian chờ (từ vòng 2)
Vòng 1 không tính giờ. Từ vòng 2, sau một **khoảng an toàn**, độ dễ tính của khách giảm **1 điểm mỗi 3 giây thực**, tối đa −20 điểm:

| Vòng | Khoảng an toàn |
| --- | --- |
| 2 | **120 giây** (1.7: 90) |
| 3 | **100 giây** (1.7: 70) |
| 4 | **105 giây** (1.7: 75) |
| Ngày 8 (mất điện) | +20 giây so với vòng 2 |

Khoảng an toàn ở bản 1.4 gấp 1,5 bản 1.3 (60 / 45 / 50 / +15 giây) vì thời gian nấu cũng gấp 1,5; chưa mô phỏng riêng, cần đo khi làm vòng 2 (sprint 4).

**Khách bỏ đi (từ 2.0, yêu cầu 22/11/2026 "khách hàng khi hết thanh chờ đợi thì sẽ tức giận bỏ đi và tự động trừ 1 số tiền nhất định"):** khi thanh kiên nhẫn hết (khoảng an toàn + 20 điểm × 3 giây = 60 giây sau đó), khách **giận, rời ghế đi ra cửa** và quán **bị trừ tiền cố định** theo vòng; ghế trống, khách mới vào như khi khách ăn xong (mục 6.2). Chủ nợ không bao giờ bỏ đi. Vòng 1 không tính giờ nên không ai bỏ đi. Dữ liệu `patience.leave_penalty`. Thay cho câu hỏi Q-39 (khách giữ ghế mãi).

| Vòng | Tiền bị trừ khi khách bỏ đi |
| --- | --- |
| 2 | **20G** |
| 3 | **25G** |
| 4 | **30G** |

**Từ 1.8 (yêu cầu của bạn 21/11/2026 "nới thêm thời gian chờ", Q-53):** khoảng an toàn chọn bằng bot C# cùng nhịp khách mới (mục 6.2): +30 giây mỗi vòng. Bot C# 4.000 ván (bộ số 1.8): khách bị giảm dễ tính vì chờ mỗi ngày ở vòng 2–4: người chơi tốt gom bộ **1,3** (1.7: 2,6), không gom bộ 0,9, không đọc chỉ số 2,6 (1.7: 5,1) — đạt đề xuất 1–2 (Q-53). Quét 1.000 ván (`balance/bot_sweep.md`): +15 giây cho 1,6; k = 80 / 120 hay tối thiểu 8 giây đều đạt Điều 1, chênh ≤ 2 điểm.

**Đo ở sprint 7 (21/11/2026, bot C# 4.000 ván, bộ số 1.7):** số khách bị giảm dễ tính vì chờ mỗi ngày ở vòng 2–4: người chơi tốt gom bộ **2,6**, chơi tốt không gom bộ **1,8**, không đọc chỉ số **5,1**. Đề xuất của impl plan: người chơi tốt bị 1–2 khách mỗi ngày; số hiện tại nhỉnh hơn một chút. Chưa đổi khoảng an toàn: chờ bạn chọn mức (Q-53).

**Đo ở sprint 4 (16/11/2026, chưa đổi số):** bot "không đọc chỉ số" chơi trên luật thật của game (3 ghế, giờ chờ thật, phục vụ theo thứ tự khách đến, món khách đầu hết thì phục vụ khách kế tiếp), 2.000 ván: khách mất trung bình **9,5 điểm** dễ tính lúc được phục vụ (mô phỏng giả định 8), qua lần trả nợ 2 là **84%** (mô phỏng 79%). Với 90 giây, vòng 2 không khó hơn mô phỏng.

Về tra cứu: mình không tìm được con số công bố chính thức về thời gian chờ của Overcooked, Cook Serve Delicious hay Papa's Pizzeria (các nguồn chỉ mô tả có thanh kiên nhẫn, không ghi giây). Lập luận của bản 1.0 (số cũ, giữ để tra): các con số được đặt theo thời gian nấu khi đó (trung bình 17–19 giây một món): khoảng an toàn 60 giây đủ để nấu liền cho cả 3 khách ở vòng 2; vòng 3 (45 giây) thì khách thứ ba bắt đầu sốt ruột nếu không có món làm sẵn ở quầy chờ hoặc tủ lạnh — dễ hơn các game nấu ăn thông thường như bạn yêu cầu. Cần kiểm lại khi playtest.

### 6.7 Khách gọi món (từ 1.3)
Mỗi khách khi vào **gọi một món cụ thể** (một công thức ở mục 5.8), hiện trên bong bóng cạnh đầu khách cùng độ dễ tính.
- **Món gọi là món nấu được:** khách chọn ngẫu nhiên, đều nhau, một trong các món đang làm được ngay lúc vào — đĩa trên quầy chờ (hoặc tủ lạnh), món đang trên bếp, và mọi cặp (loại chính, loại phụ) đang có trong kho. Nếu không làm được món nào, khách gọi ngẫu nhiên một món đã mở khóa trong sách.
- **Mang sai món:** khách vẫn nhận đĩa nhưng giận (mục 6.5): không trả, quán bị trừ 30% giá gốc. Đĩa đó mất.
- **Đúng món:** trả tiền và tip theo bậc như mục 6.5.
- **Chủ nợ ngày 21** không gọi món cụ thể: ông gọi "món ngon nhất của quán" và nhận mọi món (vẫn cần Tuyệt hảo trở lên để được thưởng, mục 8). *(cần bạn xác nhận, Q-20)* **Từ 2.1** (yêu cầu 23/11/2026): "món ngon nhất" là **Trứng chiên cà chua bậc Huyền thoại** — tranh gợi ý hiện đầu vòng 4 và ngày 21; mang đúng món đó thì thưởng **×1,5** (Q-80; Tuyệt hảo bất kỳ vẫn đủ thưởng thường để không đổi Điều 1). Dữ liệu `boss_best`.
- Từ vòng 2 có tối đa 3 khách cùng ngồi (mục 6.1), mỗi người một món gọi; người chơi chọn phục vụ ai trước.

Mô phỏng (mục 9): người chơi không đọc chỉ số vẫn đi được 67%; người gom bộ thắng 82%. Nếu chủ nợ cũng gọi món ngẫu nhiên thì người gom bộ còn 73% — sát mốc 70% của Điều 1, nên chủ nợ là ngoại lệ. (Số của bản 1.3. Với số 1.4: 90% khi có ngoại lệ, 83% khi không.)

---

## 7. Relic

### 7.1 Luật chung
- Mỗi ngày (từ ngày 2) cửa hàng hiện **3 relic khác nhau**; giá trả bằng vé trắng.
- **Ngày 2 miễn phí (từ 1.8, yêu cầu của bạn 21/11/2026):** cả 3 bệ giá 0 vé (kể cả sau khi đổi hàng) nhưng chỉ lấy **miễn phí 1** relic; ngày này chỉ bày relic **Thường** (không bày relic quá mạnh). **Từ 2.2 (yêu cầu 24/11/2026):** lấy xong relic miễn phí thì 2 bệ còn lại **vẫn trên kệ, hiện lại giá vé**, mua tiếp bằng vé như ngày thường (đổi hàng cũng được); 1.8–2.1: 2 bệ còn lại đóng tới hết ngày. Dữ liệu: `relic_free_day`.
- **Đổi hàng (từ 1.8):** nút Đổi hàng ở Góc 3: trả **50G** (mỗi lần sau trong ngày **+25G**, đầu ngày về 50G) để 3 bệ bày 3 relic **khác hẳn** 3 relic đang bày (và chưa có trên giá). Dữ liệu: `relic_reroll`.
- **Từ 1.7 (yêu cầu của bạn 21/11/2026):** trên màn hình không ghi relic nào thuộc bộ combo, relic nào là relic lẻ (thẻ relic chỉ ghi tên, giá, độ hiếm, hiệu ứng; giá relic chỉ ghi số relic đang có). Thưởng bộ vẫn tính như mục 7.2; người chơi tự khám phá. Điều 4 vẫn giữ: giá và hiệu ứng từng relic hiện đủ.
- Tối đa **8 relic**. Đầy thì phải bỏ một cái để lấy cái mới — **từ 2.0 chọn relic bỏ trong bảng Tab → Relic** (bỏ giá 8 ô trên tường). Ba bệ relic dời ra giữa tường Góc 3 (bỏ quyển sách).
- Mua bằng **vé đen**: 3 lựa chọn chỉ gồm relic Hiếm trở lên, và ít nhất một cái thuộc bộ bạn đang có nhiều món nhất.
- Hiệu ứng "x% nâng bậc": mỗi món trong thùng tương ứng có x% được +1 bậc (tối đa bậc 5).

### 7.2 Ba bộ combo

**Bộ Vườn Nhà** — đồ phụ

| Relic | Giá | Độ hiếm | Hiệu ứng |
| --- | --- | --- | --- |
| Bình tưới gỉ | 1 vé | Thường | Đồ phụ: 15% nâng bậc |
| Rổ tre bà ngoại | 2 vé | Thường | Thùng đồ phụ +1 món (bản 1.3: +2, khi thùng còn 5 món) |
| Phân bón bí truyền | 2 vé | Hiếm | Đồ phụ bậc 0 được quay lại 1 lần |
| Hạt giống vàng | 3 vé | Hiếm | Đồ phụ: 6% nhảy thẳng lên bậc 4 |
| **Thưởng bộ** | | | 2 món: +20% nâng bậc · 3 món: đồ phụ tối thiểu bậc 2 và +35% nâng bậc · 4 món: mọi đồ phụ +1 bậc |

**Bộ Chợ Sớm** — nguyên liệu chính

| Relic | Giá | Độ hiếm | Hiệu ứng |
| --- | --- | --- | --- |
| Dao phay cũ | 1 vé | Thường | Nguyên liệu chính: 15% nâng bậc |
| Thùng đá | 2 vé | Thường | Nguyên liệu chính bậc 0 được quay lại 1 lần |
| Mối quen chợ cá | 2 vé | Hiếm | Thùng chính +1 món (2 → 3) |
| Cân vàng | 4 vé | Hiếm | Nguyên liệu chính: 8% nhảy thẳng lên bậc 4 |
| **Thưởng bộ** | | | 2 món: +20% nâng bậc · 3 món: chính tối thiểu bậc 2 và +25% nâng bậc · 4 món: mọi nguyên liệu chính +1 bậc |

**Bộ Sơn Hào Hải Vị** — nguyên liệu đột phá

| Relic | Giá | Độ hiếm | Hiệu ứng |
| --- | --- | --- | --- |
| Bản đồ hải đảo | 1 vé | Thường | Thùng đột phá +10% |
| Lưới thép | 2 vé | Hiếm | Thùng đột phá +8%; tỉ lệ bậc 5 trong thùng từ 30% lên 50% |
| Thuyền thuê | 3 vé | Hiếm | Thùng đột phá +12%; **đổi lại** thùng đồ phụ −1 món (3 → 2) |
| La bàn rồng | 4 vé | Huyền thoại | Thùng đột phá +15% |
| **Thưởng bộ** | | | 2 món: +15% · 3 món: thùng đột phá có thêm 1 món (1 → 2) · 4 món: +20% và mọi món đột phá đều bậc 5 |

Mỗi bộ có một món **1 vé nhưng là mảnh quan trọng của combo** (Bình tưới gỉ, Dao phay cũ, Bản đồ hải đảo), đúng ý "relic rẻ nhưng giá trị cao trong combo".

### 7.3 Relic lẻ

| Relic | Giá | Độ hiếm | Hiệu ứng | Ghi chú thiết kế |
| --- | --- | --- | --- | --- |
| Tủ lạnh 2 cửa | 2 vé | Hiếm | Tủ lạnh +2 ô, món không tụt bậc | Rất hợp để giữ món cho boss |
| Chuông vàng | 2 vé | Thường | Tip +15% | |
| Đồng hồ cát | 1 vé | Thường | Thời gian chờ an toàn +40% | Vô dụng ở vòng 1 |
| Muối hồng | 1 vé | Thường | Điểm món +0,15 | |
| Chảo gang bà ngoại | 3 vé | Hiếm | Món không bao giờ dưới Dở | Chống thảm họa |
| Tượng mèo vàng | 4 vé | Thường | Tiền bán món +8% | **Bẫy giá cao**: đắt nhưng giá trị bình thường |
| Máy pha cà phê xịn | 4 vé | Thường | Thời gian chờ an toàn +60% | **Bẫy giá cao** |
| Ớt ma | 2 vé | Hiếm | Điểm món +0,6; **đổi lại** 10% món thành Lạc vị | Đánh đổi |
| Hũ sốt cũ | 3 vé | Hiếm | Tỉ lệ hỏng của nước sốt −8 điểm (12% → 4%) | Hợp với người dùng nhiều sốt |
| Vé số cào | 1 vé | Thường | +1 vé trắng mỗi 3 ngày | |
| **Sao băng trong nồi** (từ 2.1) | 2 vé | Hiếm | Huyền thoại khi nấu **+0,3%** (mục 5.3) | Tên theo bảng bạn đưa 23/11/2026 |
| **Bí kíp gia truyền** (từ 2.1) | 4 vé | Hiếm | Huyền thoại khi nấu **+1,0%** | Tên mình đặt (Q-81); cùng Sao băng tổng +1,3%, trần 5% |

---

## 8. Cửa hàng vật phẩm (trả bằng tiền)

| Vật phẩm | Mở khóa | Giá | Giới hạn |
| --- | --- | --- | --- |
| Máy gacha đồ ăn | Ngày 4 | 800G | Mua 1 lần |
| Nước sốt thần thánh | Ngày 4 (qua vòng 1) | 450G, mỗi lần mua sau +45G | Giữ tối đa 20 |
| Vé đen | Ngày 4 | 700G, mỗi lần mua sau +120G | Giữ tối đa 5 |
| **Thớt** (từ 2.0) | Ngày 4 | **300G**, mỗi lần mua sau **+200G** | Tổng tối đa **3 thớt** (có sẵn 1) |
| **Bếp** (từ 2.1) | Ngày 4 | **500G**, mỗi lần mua sau **+300G** | Tổng tối đa **2 bếp** (có sẵn 1) |
| **Tay vạn năng** (từ 2.2) | Ngày 4 | **600G** | Mua 1 lần |

**Tay vạn năng (từ 2.2, yêu cầu 24/11/2026 "mua thì sẽ cầm được 2 đồ … tự định giá mức giá và chỉ được mua 1 lần"):** cầm 2 thứ cùng lúc (Điều 5), ô đồ ở đáy màn hình (Điều 9). Giá 600G mình chọn (Q-85): đắt hơn thớt / bếp vì tiết kiệm nhiều lượt đi lại, rẻ hơn máy gacha. Bot chưa mô hình (như thớt, bếp).

**Bếp (từ 2.1, yêu cầu 23/11/2026 "max 2 bếp, nới rộng phần bếp như thớt. Mua thêm bếp giá 500g, mua lần 2 với giá 800g"):** "max 2 bếp" hiểu là tổng 2 (có sẵn 1, mua 1 lần 500G); giá lần sau 800G nằm trong dữ liệu (`stove.price + stove.step`) để dùng khi nâng giới hạn (Q-84). Bếp mới hiện ngay bên phải quầy đĩa, giữ qua các ngày (bản lưu). Bot chưa mô hình bếp thứ hai (như thớt, K-4).

**Thớt (từ 2.0, yêu cầu 22/11/2026 "mua thêm thớt max 3, tăng giá mỗi lượt mua"):** mua xong bàn sơ chế bên trái bếp dài ra và có thêm thớt ngay; giữ qua các ngày (bản lưu). Bot chưa mô hình thớt thứ hai (bot không tốn giờ đi lại song song), nên không đổi Điều 1. Dữ liệu `board`.

Từ 1.8 mua ở **bảng Tab → Cửa hàng** (Điều 9); đồ đã mua mới hiện ở Góc 2.

Mô phỏng: với giá nước sốt 450G, 10% người chơi may mắn mua được chai đầu từ ngày 4–5, một phần tư mua được trước ngày 7, trung vị là ngày 8 — khớp yêu cầu "mua được tầm ngày 5 nếu nhân phẩm tốt".

**Boss cuối:** phục vụ chủ nợ ngày 21 món **Tuyệt hảo hoặc Huyền thoại** → nhận thưởng **11.500G** (ngoài tiền món); **từ 2.1** đúng "món ngon nhất" (Trứng chiên cà chua Huyền thoại, mục 6.7) → thưởng ×**1,5** = 17.250G. Mô phỏng: nếu bỏ khoản thưởng này thì không ai trả nổi khoản nợ cuối (tỉ lệ thắng 0%) — tức là phải thắng boss cuối mới thắng game. Đây là chủ ý: món Tuyệt hảo giữ trong tủ lạnh cho ngày 21 là mục tiêu chiến thuật của cả vòng 4.

---

## 9. Kiểm tra và thay đổi

- Mô phỏng: `balance/sim_balance.py` (4.000 ván mỗi kiểu người chơi). Mọi thay đổi số liệu phải chạy lại và đạt Điều 1.
- **Đo ở sprint 7 (F-060 AC-1):** `balance/playtest_overhead.py` trên `playtest-log.csv`: phần lớn dòng do test PlayMode ghi lẫn (đã tách: từ 1.7 test ghi vào thư mục tạm, mỗi dòng có cột `phien`). Còn ≤ 6 buổi có thể là người thật, 47 món, toàn vòng 1: trung vị **19 giây** thao tác + đi lại mỗi món (giả định 20). Số này vào `balance.json bot_timing.good_overhead_s = 19`; người không đọc chỉ số chưa có số đo riêng, giữ 30 giây. Cần 3 buổi bạn chơi qua vòng 2 để đo lại.
- **Giả định thời gian của mô phỏng 1.4 (chưa đo):** ngoài thời gian sơ chế + nấu trong dữ liệu, mỗi món tốn thêm thời gian đi lại, ngắm, mang ra quầy: **20 giây** với người chơi tốt (tranh thủ sơ chế lúc bếp đang nấu), **30 giây** với người không đọc chỉ số. Kết quả rất nhạy với số này: nếu là 26 / 38 giây thì người gom bộ chỉ còn thắng 33% và người không đọc chỉ số đi được 54%. Sprint 2 phải đo bằng người thật (F-014) rồi chỉnh nợ nếu lệch.
- Mô phỏng đã đơn giản hóa: không mô hình hóa vé đen dùng để mua relic, việc bỏ relic khi đầy chỉ theo điểm ưu tiên, thời gian chờ được quy đổi thành mức giảm độ dễ tính trung bình (−3 điểm cho người nhanh, −8 cho người chậm). Kết quả là chỉ dấu, cần xác nhận bằng playtest.
- Lịch sử phiên bản:
  - **1.0 (14/11/2026):** bản đầu từ ý tưởng của bạn; mọi con số đánh dấu "claude" đã được tính và kiểm bằng mô phỏng.
  - **1.1 (14/11/2026):** thêm danh sách nguyên liệu (mục 4.5), công thức và 120 món (5.8), thời gian nấu và quá lửa (5.9), quầy chờ 3 đĩa (5.1); mỗi thùng chỉ một loại nguyên liệu (4.2). Không đổi số cân bằng: mô phỏng vẫn tính mỗi món 1 chính + 1 phụ với giá gốc theo vòng. Quá lửa chưa có trong mô phỏng — nó chỉ phạt người chơi bỏ quên bếp.
  - **1.4 (15/11/2026):** theo 6 ghi chú của bạn khi đóng sprint 1. (a) Khách không giới hạn tới 21:00, đồng hồ ngày (mục 2, 6.2). (b) Sơ chế ×3, nấu ×1,5 (mục 4.5, 5.8, 5.9); khoảng chờ an toàn ×1,5 (6.6). (c) Thùng 3 phụ + 2 chính, ngày 3 là 2 + 1, đột phá 1 món; Rổ tre +1 thay vì +2 (mục 4.2–4.4, 7.2). (d) Nợ 750 / 2.900 / 10.500 / 32.000G (mục 2). (e) Hai số mình thêm để hệ thống còn chạy được, cần bạn xác nhận: **6 vé mỗi ngày** (thùng nhỏ nên 3 vé chỉ đủ 6 món, không còn vé mua relic, không ai trả nổi nợ) và **1 giờ = 40 giây** (nấu lâu hơn mà ngày vẫn 6 phút thì số khách giảm chứ không tăng). (f) Bảng tỉ lệ bậc và giá món giữ nguyên. Mô phỏng: 63% / 90% / 46% (Điều 1). Số nằm ở `balance/next/`, chạy `V=next python balance/sim_balance.py`.
  - **1.3 (14/11/2026):** theo yêu cầu của bạn "khách hàng phải order 1 món ăn cụ thể và từng món phải hiển thị giá ở trên quyển sách công thức". Thêm mục 6.7 (gọi món), dòng "Sai món" ở 6.5, giá bán theo bậc trên trang sách (5.8), Điều 9 cập nhật vị trí thông tin. Ba lựa chọn do bạn trả lời 14/11: gọi món nấu được, sai món coi như khách giận, giá chung theo vòng ghi theo bậc. Không đổi số trong dữ liệu. Mô phỏng `sim_balance.py` thêm loại nguyên liệu cho từng thùng, công thức cho từng đĩa, khách gọi món, và 3 khách cùng ngồi từ vòng 2 (trước đây mô phỏng cho người chơi tự ghép món với khách bất kỳ); chạy `ORDERS=0` để ra lại số cũ.
  - **1.2 (14/11/2026):** theo nghiệm thu sprint 1 của bạn ("game này là 3D nhưng hiện tại chơi như game 2D"): Điều 5 đổi từ xoay camera 90° sang tự đi lại góc nhìn thứ nhất; thêm Điều 9 (thông tin nằm trên đồ vật, HUD chỉ số liệu); mục 4.2, 5.1, 5.8 tả lại thao tác bằng đồ vật; mục 6.1 thêm nút Mở quán (bạn trả lời 14/11) và đóng quán sớm. Không đổi số cân bằng; mô phỏng không bị ảnh hưởng. Thời gian đi lại trong phòng chưa có trong ước tính "17–19 giây một món" ở mục 5.9 — cần đo lại khi playtest vòng 2 (có tính giờ chờ).

  - **1.5 (18/11/2026):** theo yêu cầu của bạn trước sprint 6 ("dựng không gian 3D phòng rộng hơn 1 chút", "thêm hoạt ảnh ăn cho khách hàng và mất 1 khoảng thời gian mới ăn xong") và việc triển khai impl plan sprint 6. (a) Phòng 9,6 × 9,6 m (Điều 5). (b) Khách ăn 5 giây tại ghế (mục 6.2): số mới `day.eat_s = 5` trong dữ liệu. Đo bằng bot C# trên luật thật, 4.000 ván mỗi kiểu: 8 giây làm người không đọc chỉ số tụt từ 67% xuống 61% chặng đường (sát mốc 60% của Điều 1) và người gom bộ từ 60% xuống 53% thắng; **5 giây**: 63% / 52% / chơi tốt không gom bộ 9%. Mô phỏng Python không đổi (63 / 89 / 46%) vì mô phỏng tính số món theo thời gian nấu, không theo ghế. Người gom bộ đã dưới mốc 70% từ sprint 5 (Q-44, chỉnh ở sprint 7). (c) Mục 2.1 khung game: menu, cốt truyện, lưu game, hướng dẫn ngày 1, cài đặt (Q-8). Chọn "tải lại ra y như cũ" cho lưu game.

  - **1.7 (21/11/2026):** theo yêu cầu của bạn trước sprint 7 và triển khai `docs/sprint-7/impl-plan.md` (F-060). (a) **Ngày 1 có 5 vé** (mục 3; `start.first_day_tickets`, bỏ `bonus_tickets`). (b) **Đồng hồ đứng ở 9:00 tới khi mở quán** (mục 6.2; đóng Q-21): gacha, sơ chế, nấu trước khi mở không mất giờ. Bot đo: người chơi tốt tận dụng bằng cách sơ chế sẵn mọi nguyên liệu (nấu sẵn đĩa thì hại: khách gọi món ngẫu nhiên nên đĩa nằm chiếm quầy, bot gom bộ tụt 52% → 27%). (c) Thẻ relic và giá relic không ghi bộ / lẻ (mục 7.1). (d) Chỉnh số theo bot C# trên luật thật (thứ tự nợ → giá gốc → vé → độ dài ngày, `balance/bot_sweep.md`):
    - Lần 1 — giữ số 1.6 (vé 6, nợ 22 32.000G) với luật (a)(b): không đọc chỉ số 67% / gom bộ thắng 79% / không gom bộ 75%: **trượt** mốc "thấp hơn rõ" (bot gom bộ chỉ đủ vé gom 3 món ở 35% ván).
    - Lần 2 — chỉ nợ 22 (36.000 / 40.000G): 47% / 12% gom bộ, không gom bộ 49% / 7%: không tách được hai kiểu, gom bộ trượt 70%. Chỉ giá gốc ×0,9: 56% / 52%: không tách. → phải dùng nút thứ ba (vé).
    - Lần 3 — vé 7 (nợ giữ): 98% / 92%: quá dễ, không tách. Vé 7 + nợ 22 34.000 / 35.000 / **36.000G**: gom bộ 95 / 91 / **85%**, không gom bộ 71 / 55 / **37%**; vé 7 + giá ×0,9: 91% / 56% nhưng bộ Vườn Nhà 98% (trượt AC-5 ≤ 97%).
    - **Chốt: vé 7 / ngày, nợ ngày 22 36.000G**, giá gốc và ba mốc nợ đầu giữ nguyên. 4.000 ván: 75% / 86% / 37%; từng bộ 77–95%. Test thống kê (Điều 3) xanh.
  - **1.8 (21/11/2026):** theo yêu cầu của bạn trước sprint 8 và triển khai `docs/sprint-8/impl-plan.md` (spec `specs/sprint-8/spec.md`). (a) **Relic ngày 2 miễn phí** (chỉ relic Thường, lấy 1, kể cả sau đổi hàng) và **đổi hàng 50G +25G / lần** (mục 7.1; bạn trả lời 21/11). (b) **Bảng Tab** relic + cửa hàng; bỏ quầy vật phẩm bày sẵn, đồ mua mới hiện ở Góc 2 (Điều 9, mục 8). (c) **Nhịp khách từ vòng 2:** hai khách liền nhau cách ít nhất max(6, 100 / ngày) giây (mục 6.2). (d) **Khoảng an toàn +30 giây** (120 / 100 / 105, mục 6.6; đóng Q-53). (e) Hướng dẫn theo sự kiện (mục 2.1). Số chọn bằng bot C# (bạn chọn "mình chọn bằng bot"): 80% / 87% / 34%, Điều 1 đạt; ván 1.7 với luật (a)(b) nhưng nhịp + giờ chờ cũ: 78% / 89% / 36%. Mô phỏng Python không mô hình (a)(c) nên không chạy lại. Hình, đèn, khách đi bộ, dao, model Blender không đổi luật.
  - **1.9 (22/11/2026):** theo yêu cầu của bạn trước sprint 9 và triển khai `docs/sprint-9/impl-plan.md` (spec `specs/sprint-9/spec.md`). (a) Viền làm lại theo điểm ảnh (Điều 7). (b) Món ăn model Blender: 12 món chính, 10 đồ phụ, 4 đế. (c) Khách đi bộ từ vỉa hè, rẽ trái / phải (mục 6.2). (d) 8 kiểu mặt khách (Điều 7). (e) Giải thích tỉ lệ, bậc, độ dễ tính trong hướng dẫn và thẻ Cách chơi (mục 2.1). (f) Nhân vật Blender có xương: khách (14 xương, 7 kiểu tóc / mũ × 6 màu áo, động tác Chờ / Vui / Giận / Vẫy chào / Đi), chủ nợ (thêm Ngạc nhiên), Hope và con gái chủ nợ tĩnh (Hope ở menu chính; màn thắng có cả ba, Q-66). Không đổi luật hay số cân bằng; bot C# chạy lại để xác nhận Điều 1.
  - **2.0 (23/11/2026):** theo yêu cầu của bạn 22/11/2026 trước sprint 10 và triển khai `docs/sprint-10/impl-plan.md` (spec `specs/sprint-10/spec.md`). (a) **Giá niêm yết từng món**, bậc khác tỉ lệ thuận; giá hiện ở Thực đơn (mục 5.2). (b) **84 món** (bỏ 30% cặp), gacha ghép thùng chính nấu được với thùng phụ, bếp từ chối cặp không có món, công thức món đột phá trong Thực đơn (mục 4.2, 4.5, 5.1, 5.8). (c) Thực đơn + relic đang có trong bảng Tab, bỏ sách và giá relic trong phòng, ba bệ relic ra giữa, bàn nhận hàng 6 thùng / hàng (Điều 9, mục 7.1). (d) Thớt mua thêm tới 3, phòng 10,8 m (Điều 5, mục 8). (e) Shift chạy, Space nhảy, chuột phải thả nguyên liệu tại chỗ (Điều 5). (f) Đặt đĩa trước mặt khách, bỏ chuông (mục 5.1). (g) Chỉ bấm biển Đóng quán mới hết ngày (mục 6.1, 6.2). (h) Khách hết kiên nhẫn bỏ đi, trừ 20 / 25 / 30G (mục 6.6). (i) Hướng dẫn bằng hình (mục 2.1). Bot C# 4.000 ván: lần 1 giữ trung bình giá = giá gốc cũ → 79% / gom bộ **71%** / 31% (sát mốc); ×1,07 → 83% / 88% / 57%; **chốt ×1,05 → 82% / 85% / 50%** (Điều 1 đạt cả ba). Mô phỏng Python không mô hình (a)(b)(h) nên không chạy lại.
  - **2.2 (24/11/2026):** theo đợt báo lỗi thứ hai của bạn (spec `specs/sprint-10c/spec.md`). Lỗi: máy gacha đồ ăn (đĩa không rời tay, không biết đã quay chưa) → máy kiểu lò vi sóng, đĩa vào máy, gạt cần, bảng thành công / xịt (mục 5.6); gợi ý cốt truyện đè lời kể (mục 2.1); nhãn "Bình thường" tràn khỏi tranh bậc; T tắt rồi không tự bật lại (mục 2.1); ngày 2 hai relic còn lại bán bằng vé (mục 7.1). Thêm: quầy phục vụ rộng, khách to hơn (mục 6.1); **tay vạn năng 600G** (Điều 5, 9, mục 8). Không đổi số cân bằng (bot không dùng máy theo cách mới, không dùng tay vạn năng; quầy chỉ là hình). Bot C# 4.000 ván sau đổi luật relic ngày 2: **90% / 91% / 32%** (2.1: 90 / 89 / 31) — Điều 1 đạt.
  - **2.1 (24/11/2026):** theo chơi thử của bạn 23/11/2026 (spec `specs/sprint-10b/spec.md`). Lỗi: thả nguyên liệu lên thớt nhặt lại được (tay trống thì tâm ngắm ưu tiên nguyên liệu đã thả); lọ nước sốt ở bên phải bếp, bấm là cầm, không mua thêm (mục 5.5); **8 vé mỗi ngày** (mục 3). Sửa: (a) **tỉ lệ ra bậc 6 bậc** theo bảng bạn đưa, Huyền thoại khi nấu tối đa 2%, mọi món đạt được Huyền thoại, 2 relic Huyền thoại (mục 5.3, 5.4, 7.3); (b) **bếp thứ hai** 500G (mục 5.1, 8); (c) hiệu ứng cầu vồng / bom xịt (mục 5.5); (d) 5 khách đặc biệt + tiếng (Điều 7); (e) món ngon nhất cho chủ nợ ×1,5 + tranh gợi ý (mục 6.7, 8); (f) hướng dẫn toàn màn hình phím T, bài relic đầu ngày 3, bài nước sốt, phím điều khiển trong Cài đặt (mục 2.1, Điều 5, 9). Cân bằng: (a) + 8 vé làm game dễ hơn — bot 4.000 ván giữ giá ×1,05: 94% / gom bộ **97%** / 72%; quét giá niêm yết ×0,95 → 90% / 89% / 30%, ×0,9 → 87% / 77% / 12%; **chốt ×0,95 → 90% / 89% / 31%** (Điều 1 đạt cả ba, gần độ khó 2.0). Bot chưa mô hình bếp thứ hai (K-4).
  - **1.6 (20/11/2026):** theo yêu cầu "sử dụng @CotTruyen_BepNhanPham.zip để làm lại phần cốt truyện". Cốt truyện từ 3 ý mở đầu + 2 kết thành 6 chương; thêm 3 chương giữa ván (đầu ngày 2, ngày mất điện, ngày thu nợ lần 3). Nhân vật mới trong tranh: sếp cũ, kẻ phá đám (tờ rơi "Quán bẩn"), bà cụ khách quen, con gái chủ nợ. Không đổi luật chơi hay số cân bằng.

### Các điểm mình đã tự diễn giải (cần bạn xác nhận)
1. "Hoàn hảo" = bậc Tuyệt hảo.
2. Ngày 14 "all-in" = khách quay lại đủ số lượng, nhưng vẫn khó tính như vòng 3.
3. Mỗi món dùng 1 nguyên liệu chính + 1 đồ phụ; các bước còn lại dùng đồ kho vô hạn. Món nhiều nguyên liệu gacha hơn để cho các phiên bản sau (sẽ phải chạy lại mô phỏng).
6. Mỗi thùng một loại nguyên liệu, như ví dụ "thùng 5 quả cà chua" của bạn.
7. Quầy chờ 3 đĩa và quá lửa từ vòng 2 là luật mới mình thêm để thời gian nấu có ý nghĩa; bỏ được nếu bạn thấy thừa.
4. Máy gacha đồ ăn không tốn vé, giới hạn 3 lần/ngày, mỗi món một lần.
5. Thua khi không đủ tiền trả nợ đúng hạn (không có trả góp).

### Mặc định đã qua nghiệm thu chức năng sprint 1 (14/11/2026)
Constitution chưa nói những điểm này; sprint 1 dùng mặc định dưới đây và bạn đã nghiệm thu chức năng "đúng như impl". Muốn đổi thì sửa ở đây trước.
8. Chọn sai dụng cụ (ví dụ nấu trứng chiên bằng nồi) bị coi như sai công thức → món ra Lạc vị.
9. Tiền là số nguyên G; giá và tip làm tròn riêng tới 1G gần nhất (0,5 làm tròn lên).
10. Tiền đền bù có thể làm ví xuống dưới 0.
11. Người chơi tự vặn núm để bắt đầu nấu; bếp không tự nấu khi đủ đồ.

---

## 10. Phụ lục — số liệu TFT đã tham khảo

Tỉ lệ cửa hàng theo cấp của TFT Set 18, nguồn: [Esports Tales — TFT champion pool size, roll odds](https://www.esportstales.com/teamfight-tactics/champion-pool-size-and-draw-chances) (truy cập 14/11/2026):

| Cấp | 1 vàng | 2 vàng | 3 vàng | 4 vàng | 5 vàng |
| --- | --- | --- | --- | --- | --- |
| 3 | 75% | 25% | 0% | 0% | 0% |
| 4 | 55% | 30% | 15% | 0% | 0% |
| 5 | 45% | 33% | 20% | 2% | 0% |
| 6 | 30% | 40% | 25% | 5% | 0% |
| 7 | 16% | 30% | 43% | 10% | 1% |
| 8 | 15% | 20% | 32% | 30% | 3% |
| 9 | 10% | 17% | 25% | 33% | 15% |

Điều mình mượn: mỗi vòng là một "cấp", khối xác suất trượt dần lên bậc cao, và bậc hiếm nhất mở rất muộn với tỉ lệ 1–3%. Điều mình không mượn: TFT không có bậc "hỏng"; game này có bậc 0 (ôi thiu) nên toàn bảng dịch xuống một nấc.
