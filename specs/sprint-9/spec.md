# Spec — Sprint 9: sửa trước sprint + Blender đợt 2 (nhân vật)

Ngày: 22/11/2026 · Nguồn: `docs/sprint-9/impl-plan.md`, tin nhắn của bạn 22/11/2026 (ghi tắt **[YC]**), tin nhắn bổ sung 22/11 "phần tutorial thiếu cả phần hướng dẫn về độ khó tính của khách hàng" (**[YC2]**), constitution 1.8 → **1.9**, Art Bible 1.3 (chưa sửa văn bản Art Bible: các chỗ lệch §6, §10, §12, §14 chờ bạn xác nhận ở Q-67..Q-69).

Phiên này chạy không hỏi lại được giữa chừng, nên các chỗ nguồn chưa nói rõ được đánh dấu **[TỰ QUYẾT]** và ghi vào `.dev-memory/QUESTIONS.md` (Q-65..Q-69) để bạn xác nhận lúc nghiệm thu.

Thứ tự: viền → món ăn → mặt khách → hướng dẫn → nhân vật có xương (F-080) → động tác + đi bộ (F-081, gồm yêu cầu đi từ vỉa hè) → chủ nợ, Hope, con gái (F-082) → kiểm cân bằng, build.

## Bảng Art Bible → làm ở đâu (L-013)

| Mục Art Bible | Quy định | Làm ở |
| --- | --- | --- |
| §10 Viền | #2B2321; nhân vật / máy lớn ≈ 5 px ở 1280×720, đạo cụ nhỏ ≈ 3 px; tường, sàn không viền; inverted hull trong shader | Shader `OutlinePx` + `OutlineMesh` (B-03) |
| §7 Món ăn theo bậc | viền đĩa màu bậc, hơi, mặt món | Giữ; thêm model món (B-04) |
| §6 Mắt và khuôn mặt | mắt vòng tròn rỗng hở 40° phía trên, miệng "w", ảnh 2D đổi theo biểu cảm | 8 kiểu mặt (B-06) |
| §6 Tỉ lệ | khách 2–2,4 đầu, đầu 45–50%; chủ nợ 3,6 đầu, vai vuông, vest #4A3F3A, dây vàng #F2C14E; Hope 2,4 đầu, tạp dề #E8604F; con gái: màu chủ nợ, khối tròn | `SK_Khach`, `SK_ChuNo`, `SK_Hope`, `SK_ConGai` (F-080, F-082) |
| §12 Animation | key "trên 2" (12 khung / giây, Stepped), squash ±15%, đứng chờ 2 giây nhún ±3%, phản ứng 0,6–1,2 giây giữ đỉnh 2–4 khung; chung một rig | Action Blender + `CharacterRig` (F-081) |
| §14 | ≤ 6.000 tam giác khách, ≤ 8.000 chủ nợ, ≤ 1.500 món ăn; 12–16 xương; tên `SK_`, `SM_` | `build_characters.py`, `build_dishes.py`, `ModelChecker` |

## Sửa trước sprint

### B-03 Viền đen lệch — [YC] "các đường viền đen của các đồ vật đang bị lệch, có thể bỏ đi hoặc những cái nào cần thì phải làm lại cho chuẩn"
Nguyên nhân: viền cũ là bản sao phóng to theo **mét** (0,028 m) quanh tâm khối. Nhìn gần (đồ trên tay, bếp, quầy) viền dày hàng chục điểm ảnh; tấm mỏng (mặt lò, nhãn) có mặt sau của bản sao lòi ra một bên như bóng đổ; góc khối vuông hở.
Kịch bản 10 giây: đứng trước bếp → mọi cạnh ngoài của bếp, quầy, chảo có một nét mực đều ≈ 3–5 điểm ảnh, ôm sát hình, không có mảng đen lệch sang một bên; cầm đĩa sát mặt nét vẫn mảnh.
- AC-1: Viền mới tính theo **điểm ảnh màn hình** (≈ 5 px nhân vật / máy lớn, ≈ 3 px đạo cụ ở 1280×720, theo §10), không bao giờ dày hơn độ dày cũ theo mét khi ở xa; đẩy theo pháp tuyến **đã làm mượt** (đỉnh trùng vị trí dùng chung một hướng) nên góc khối vuông không hở, tấm mỏng chỉ có nét quanh viền.
- AC-2: Dùng chung cho khối dựng cảnh (`BuildNhaHang`), khối lúc chạy (`Toon`), model Blender (`ModelLibrary`) và ống (tay, khung cửa).
- AC-3: Bỏ viền ở vật dưới ngưỡng chi tiết (§8: cạnh nhỏ nhất < 2 cm — nhãn, vệt, chữ trên bảng) và ở tường / sàn / trần như §10.
- AC-4: Ảnh trước / sau ở `docs/sprint-9/ui/` (bếp, quầy, thùng, đĩa trên tay).

### B-04 Món ăn rõ ràng hơn — [YC] "món ăn khi nấu xong cũng đang xấu, tôi muốn các món ăn rõ ràng hơn nữa"
Kịch bản 10 giây: múc món ra đĩa → giữa đĩa là một khối món to, dễ gọi tên (quả trứng ốp lòng đỏ, lát thịt heo xếp quạt, miếng đậu trong sốt, con tôm cong, khúc cá hồi có vân…), đồ phụ là vài miếng nhận ra được đặt cạnh, đế món (cơm / mì Ý / bún / bánh mì) nằm phía sau; nhìn 1 giây biết món gì.
- AC-1: Model Blender cho **12 nguyên liệu chính đã nấu** (`SM_Mon_<id>`), **10 đồ phụ đã nấu** (`SM_Kem_<id>`) và **4 đế món** (`SM_De_<Com|MiY|Bun|BanhMi>`), mỗi cái ≤ 1.500 tam giác, ≤ 3 vật liệu, gốc đáy giữa; món chính có empty `Face`.
- AC-2: Món trong chảo / nồi / vỉ và trên đĩa dùng chung model (F-013 AC-5 cũ): món chính chiếm ≥ 45% đường kính lòng đĩa, đồ phụ cạnh bên, đế phía sau; Lạc vị (bậc 0) vẫn tối xỉn (F-013 AC-6).
- AC-3: Thiếu model → hình ghép khối cũ, không lỗi.
- AC-4: Ảnh 9 món vòng 1 + 3 món vòng 2–4 trên đĩa (`docs/sprint-9/ui/b04_*`).

### B-05 Khách đi bộ từ vỉa hè — [YC] "đi bộ từ chỗ vỉa hè rẽ trái hoặc rẽ phải vào quán, và đi ra cũng phải rẽ trái hoặc rẽ phải khỏi cửa hoàn toàn mới biến mất"; trả lời câu hỏi impl plan §6 ("khách có cần đi từ cửa vào không").
Kịch bản 10 giây: nhìn ra cửa Góc 1 → một khách đi dọc vỉa hè từ bên trái (hoặc phải) có bước chân, tới trước cửa rẽ vào, bước qua ngưỡng, tới ghế quay mặt vào quầy; bong bóng hiện khi tới ghế. Ăn xong → quay lưng đi ra cửa, rẽ trái hoặc phải đi dọc vỉa hè tới khuất sau tường rồi mới biến mất.
- AC-1: Đường vào: điểm đầu trên vỉa hè ngoài tầm nhìn qua cửa (|x| ≥ 3,4 m so với tâm cửa, z ≈ 5,5 m khung Góc 1) → trước cửa → ngưỡng cửa → ghế. Bên trái / phải chọn theo hạt giống của khách (xác định, tải lại ra như cũ).
- AC-2: Đường ra ngược lại, rẽ trái hoặc phải (có thể khác lúc vào), khách chỉ ẩn khi đã tới điểm cuối ngoài tầm nhìn.
- AC-3: Khi đi có động tác bước (clip `Di` của rig, F-081; chưa có rig thì nhún theo bước như sprint 8). Tốc độ vỉa hè 1,3 m/giây, trong phòng 0,9 m/giây; tổng ≤ 6 giây.
- AC-4: Đi bộ chỉ là hình: luật (giờ chờ, nhịp khách) không đổi. **[TỰ QUYẾT]** Giờ chờ vẫn tính từ lúc khách xuất hiện như sprint 8; thêm ≈ 2–3 giây đi bộ so với 120 / 100 / 105 giây an toàn (≤ 3%), nên không đổi số (Q-65).

### B-06 Mặt khách khác nhau — [YC] "thay đổi khách hàng có nhiều khuôn mặt khác nhau, dựa vào art bible"
- AC-1: ≥ 8 kiểu mặt; mọi kiểu giữ quy cách §6 (mắt vòng tròn rỗng viền đậm hở khe ≈ 40° phía trên hướng vào giữa, không tròng đen / đốm sáng trừ "Chờ"; miệng chữ "w" nhỏ khi vui). Khác nhau ở: cỡ và khoảng cách mắt, lông mày, má hồng (tròn / dẹt / tàn nhang), lông mi, râu mép, nốt ruồi, độ rộng miệng.
- AC-2: Mỗi kiểu mặt có đủ 7 biểu cảm + bản chớp mắt (F-062 AC-2 cũ); chủ nợ giữ mặt riêng.
- AC-3: Kiểu mặt chọn theo hạt giống của khách, hai khách liền nhau khác kiểu mặt.

### B-07 Hướng dẫn giải thích tỉ lệ, bậc, độ dễ tính — [YC] "phần tutorial thiếu đi giải thích những tỉ lệ, bậc,…", [YC2]
- AC-1: Hướng dẫn ngày 1 có thêm dòng **giải thích** đi kèm bước (không đổi 10 bước làm): bảng tỉ lệ Góc 2 (mỗi món tự quay bậc theo bảng của vòng); 6 bậc nguyên liệu đọc bằng màu đế + mặt + phụ kiện (§7); dải 6 ô trên bếp (bậc món = 0,6 × chính + 0,4 × phụ + 0,8, phần lẻ là xác suất lên bậc — constitution §5.3); giá theo bậc trên sách; **độ dễ tính** trên bong bóng khách (càng cao càng dễ; món tối thiểu theo §6.4; khó tính được món cao tip nhiều; thiếu 1 bậc trả nửa giá; thiếu 2 bậc hoặc sai món: không trả, quán bị trừ 30% giá gốc — §6.5).
- AC-2: Bài vòng 2 thêm: độ dễ tính giảm 1 điểm mỗi 3 giây sau khoảng an toàn (§6.6), thanh kiên nhẫn.
- AC-3: Bảng Tab có thẻ thứ ba **Cách chơi** đọc lại mọi giải thích bất cứ lúc nào, số lấy từ dữ liệu (Điều 2): bảng tỉ lệ bậc của vòng hiện tại, tên + màu 6 bậc nguyên liệu, 6 bậc món + hệ số giá, công thức bậc món, bảng độ dễ tính → món tối thiểu, trả tiền / tip, giờ chờ của vòng.
- AC-4: Chữ ≥ 14 px ở 1280×720 (§13).

## Sprint 9 (impl plan)

### F-080 Thân khách dùng chung — Art Bible §6, §12
Kịch bản 10 giây: khách đứng sau quầy là một nhân vật tròn có thân, tay ống cao su găng trắng, chân ngắn; đầu to; mặt 2D chớp mắt; áo và tóc / mũ mỗi người một khác; khi chờ thì nhún nhẹ.
- AC-1: Một rig `SK_Khach` cho mọi khách, 12–16 xương; cao 2,3 đầu, đầu ≈ 47% chiều cao. **Lệch Art Bible §14:** tay dùng 2 xương / tay thay Bendy Bones vì FBX không xuất Bendy Bones; trọng số mượt nên tay vẫn cong như ống.
- AC-2: ≥ 6 biến thể nhìn khác nhau: màu áo (6 màu nhân vật, không dùng màu bậc) × tóc / mũ (6 kiểu trong FBX, bật / tắt theo tên); chọn theo hạt giống của khách; hai khách liền nhau khác kiểu.
- AC-3: Mặt 2D dán đúng vùng mặt (empty `Face` trên xương đầu), đổi 7 biểu cảm + 8 kiểu mặt (B-06); chớp mắt còn chạy.
- AC-4: Bong bóng món gọi và thanh kiên nhẫn vẫn cạnh đầu.
- AC-5: Ba khách cùng ngồi không lồng vào nhau (hộp bao không chạm nhau).
- AC-6: Tay cầm đũa lúc ăn (sprint 7) vẫn chạy: tay vẫy của rig ẩn khi ống tay đũa hiện.

### F-081 Động tác — Art Bible §12
- AC-1: `Cho` (đứng chờ): vòng lặp 2 giây, nhún ±3% chiều cao.
- AC-2: `Vui`, `Gian`: 0,6–1,2 giây, giữ tư thế đỉnh 2–4 khung; chạy đúng lúc nhận món theo kết quả trả tiền (vui = đạt; giận = thiếu ≥ 2 bậc / sai món). Thêm `VayChao` (khi vừa ngồi) và `Di` (bước, vòng lặp) cho B-05.
- AC-3: Key "trên 2" (impl plan, Art Bible §12) **mâu thuẫn** yêu cầu của bạn ngày 20/11 "animation của khách hàng, nguyên liệu cũng mượt như cốt truyện" (Q-48 đã đóng theo hướng mượt). Theo quy tắc "mâu thuẫn thì làm theo yêu cầu, ghi câu hỏi": clip key trong Blender theo bước 2 khung (12 / giây), game **mặc định nội suy mượt**; `CharacterRig.Stepped = true` thì lấy mẫu theo bước 1/12 giây (test kiểm cả hai). Q-67.
- AC-4: Vào / ra có chuyển động (B-05).

### F-082 Chủ nợ, Hope, con gái chủ nợ — §2, Art Bible §6
- AC-1: `SK_ChuNo` model riêng (3,6 đầu kể cả mũ, vai vuông, vest #4A3F3A, dây vàng #F2C14E, khối chữ nhật bo nhỏ), dùng các clip `Cho`, `Gian`, `Di`, `NgacNhien`; mặt lạnh lùng, đổi Ngạc nhiên khi nhận món Tuyệt hảo ngày 21.
- AC-2: `SM_Hope` (2,4 đầu, tạp dề #E8604F) và `SM_ConGai` (màu chủ nợ, khối tròn) tư thế tĩnh. **[TỰ QUYẾT] (Q-66):** từ sprint 6 tranh mở đầu và kết thúc là trang truyện vẽ sẵn (constitution §2.1, không còn tranh khối). Nên: Hope đứng sau quầy ở **menu chính** (căn phòng nhìn từ cửa); màn **thắng** dựng cảnh Hope, chủ nợ và con gái đứng ở quầy Góc 1 phía sau thẻ kết quả.

### Kiểm (impl plan §4)
- Công cụ kiểm model: thêm số xương, tên clip, giới hạn tam giác theo loại (khách 6.000, chủ nợ 8.000, món 1.500).
- PlayMode: một ngày vòng 2 đủ 3 khách, chụp mỗi biểu cảm, mỗi động tác, mỗi kiểu mặt; đo khung hình ≥ 60 / giây với 3 khách (đo trong Editor, tắt vsync).
- Toàn bộ EditMode + PlayMode cũ xanh; bot C# không đổi luật nên chạy lại để xác nhận Điều 1.

## Ngoài phạm vi
Tay Hope góc nhìn thứ nhất, khách ăn tại chỗ khác ghế, nhiều thân khách khác nhau (impl plan §1); 16 nguyên liệu còn lại bằng Blender (sprint 11); đổi số cân bằng; vẽ lại trang truyện.

## Ma trận AC → test (22/11/2026, sau triển khai)

| AC | Test | Kết quả |
| --- | --- | --- |
| B-03 AC-1, AC-3 | EditMode `B03_AC1_VienLamMuot_*`, `B03_AC1_DoDayTheoDiemAnh_*`; PlayMode `B03_VienTheoDiemAnh_*` (đo điểm ảnh thật quanh khối) | xanh |
| B-03 AC-2 | PlayMode B03 (khối cảnh + khối lúc chạy), `F071_F072_*` (model) , Sprint7EditorTests (công cụ thử model) | xanh |
| B-03 AC-4 | ảnh `b03_bep/quay_cho/thung/dia_tren_tay.png` | CẦN NGƯỜI KIỂM (so với ảnh sprint 8) |
| B-04 AC-1, AC-3 | EditMode `B04_AC1_DuModelMon_*`, `Sprint8ModelTests.F070_AC3_*` (quy cách mọi FBX) | xanh |
| B-04 AC-2 | EditMode `B04_AC2_*`; PlayMode `B04_MonAn_*` (12 món, ≥ 45%, Lạc vị xỉn), `F040_*` (vết nướng) | xanh |
| B-04 AC-4 | ảnh `b04_muoi_hai_mon.png`, `b04_mon_gan.png` | CẦN NGƯỜI KIỂM (nhận ra món) |
| B-05 AC-1..4 | EditMode `B05_AC1_*`; PlayMode `B05_KhachDiTuViaHe_*`, `F073_*` (sửa theo B-05) | xanh |
| B-06 AC-1..3 | EditMode `B06_AC1_*` (8 kiểu × 7 biểu cảm khác nhau; mắt rỗng, viền đậm), `B06_AC3_*`; PlayMode F080 (kiểu mặt theo hạt giống) | xanh |
| B-07 AC-1..4 | EditMode `B07_AC1_*`, `B07_AC3_*`; PlayMode `B07_HuongDan_*` | xanh |
| F-080 AC-1..6 | EditMode `F080_AC1_*`, `F080_AC2_*`, `F080_F082_HuongModel_*`; PlayMode `F080_BaKhach_*` (rig, kiểu, mặt trên đầu, bong bóng, không lồng nhau, ≥ 60 khung/giây), `F081_*` (tay rig ẩn khi cầm đũa), `KhachAn_*` | xanh |
| F-081 AC-1..4 | EditMode `F081_AC1_AC2_*`; PlayMode `F081_DongTac_*` (Vui khi đạt, Giận khi sai món, "trên 2" khi bật), `KhachVaNguyenLieu_ChuyenDongMuot`, B05 | xanh |
| F-082 AC-1 | EditMode `F082_AC1_*`; PlayMode `F082_ChuNo_*` | xanh |
| F-082 AC-2 | EditMode `F082_AC2_*`; PlayMode `F082_Hope_MenuChinh_CanhThang_*` | xanh; chỗ đặt = CẦN NGƯỜI KIỂM (Q-66) |

Hoàn thiện: 13/13 nhóm AC có test tự động xanh; 3 AC hình ảnh cần bạn xem ảnh.

