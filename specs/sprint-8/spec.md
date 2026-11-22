# Spec — Sprint 8: Blender đợt 1 + sửa trước sprint

Ngày: 21/11/2026 · Nguồn: `docs/sprint-8/impl-plan.md`, tin nhắn của bạn 21/11/2026 (ghi tắt **[YC]**), câu trả lời 4 câu hỏi 21/11 (**[TL]**), constitution 1.7 → **1.8**, Art Bible 1.3.

Thứ tự ưu tiên: 2 bug → luật lõi (vé, relic, khách) → hình trong phòng → Blender → hướng dẫn tổng hợp → kiểm cân bằng.

## Số mới (vào `balance/make_next.py` → `balance.json`, Điều 2)

| Khoá | Giá trị | Nguồn |
| --- | --- | --- |
| `relic_reroll.base`, `.step` | 50G, +25G mỗi lần sau trong ngày (về 50G đầu ngày) | [TL] |
| `relic_free_day.day`, `.rarity`, `.picks` | ngày 2, chỉ relic Thường, lấy 1 | [TL] "cả 3 đều free và cả khi reroll nhưng chỉ được lấy 1 và không được xuất hiện relic quá mạnh" |
| `day.arrival.from_round`, `.gap_k_s`, `.min_gap_s` | từ vòng 2: khách sau vào cách khách trước ít nhất `max(min, k / ngày)` giây | [YC] "tốc độ xuất hiện tỉ lệ thuận với ngày"; số chọn bằng bot [TL] |
| `patience.safe_s` | nới thêm (mức chọn bằng bot) | [YC] "nới thêm thời gian chờ"; [TL] |

## Bug

### B-01 Màu nhạt dần, cuối ngày phòng tím — [YC] ảnh 1
Nguyên nhân: `ScreenTint` (sprint 7) phủ một lớp ảnh màu 20% **đè** lên cả màn hình (trộn alpha = làm nhạt mọi màu; sau 21:00 thành tím #C9B3F0). Art Bible §10: "Đổi giờ, mưa, mất điện = đổi màu / cường độ của **đèn** chính".
- AC-1: Không còn lớp phủ toàn màn hình. Màu theo giờ (Art Bible §4) đi vào màu đèn chính (nhân, không pha trắng).
- AC-2: Ảnh 9:00 / 17:00 / 20:30 / 21:30 (đèn bật): độ bão hòa trung bình của khung hình không giảm quá 10% so với 9:00; không có lớp tím đè.

### B-02 Vé không dùng không cộng dồn sang ngày sau — [YC], constitution §3
- AC-1: Ngày 1 dùng 2/5 vé, hết ngày → ngày 2 có 3 + 7 = 10 vé, đi qua đường chơi thật (GameController.NextDay, kể cả lưu / chơi tiếp).
- AC-2: Mọi đường khởi đầu ngày (ngày mới, chơi tiếp, nhảy ngày gỡ lỗi) giữ vé cũ.

## Luật

### F-077 Relic: đổi hàng (reroll) và ngày 2 miễn phí — [YC], [TL]
- AC-1: Góc 3 có nút **Đổi hàng** (đồ vật 3D, Điều 9) ghi giá. Bấm: trừ tiền, 3 bệ bày 3 relic **khác hẳn** 3 relic đang bày (và chưa có trên giá). Giá 50G, mỗi lần sau trong ngày +25G; đầu ngày về 50G.
- AC-2: Không đủ tiền / ngày 1 / hết ngày → không đổi, báo lý do một dòng.
- AC-3: Ngày 2: 3 bệ chỉ bày relic **Thường**, giá 0 vé (kể cả sau khi đổi hàng); lấy 1 cái thì 2 bệ còn lại đóng tới hết ngày.
- AC-4: Lưu đầu ngày không đổi (số lần đổi hàng về 0 mỗi ngày). Bot C# tính được ngày 2 miễn phí.

### F-079 Nhịp khách và giờ chờ — [YC] "ngày 4 vòng 2 mà 1 giây đã 3 người vào cùng lúc"
- AC-1: Từ vòng 2, hai khách liền nhau vào cách nhau ít nhất `gap(ngày) = max(min_gap_s, gap_k_s / ngày)` giây (ghế trống vẫn cần thêm `next_customer_s`). Mở quán: chỉ 1 khách vào ngay.
- AC-2: Vòng 1 giữ nguyên (lần lượt, 3 giây sau khi khách trước ăn xong).
- AC-3: Khoảng an toàn nới thêm; con số chọn bằng bot C# (`balance/run_sweep.ps1`) sao cho Điều 1 vẫn đạt (≥ 60% / ≥ 70% / thấp hơn rõ) và khách bị giảm dễ tính / ngày của người chơi tốt ≤ 2 (Q-53).
- AC-4: Constitution 1.8 ghi số mới + bảng quét.

## Hình và thao tác trong phòng

### F-073 Khách đi vào, đi ra — [YC] "đi vào và đi ra khi ăn xong, không cần quá nhanh"
Kịch bản 10 giây: cửa Góc 1 → khách bước qua rèm, nhún theo bước (≈ 0,9 m/giây), tới ghế quay mặt về Hope, nhún ngồi; bong bóng gọi món hiện khi đã ngồi. Ăn xong → đứng dậy, quay lưng, bước ra cửa, khuất ngoài phố.
- AC-1: Khách mới: đi từ ngoài cửa tới ghế trong 2–3,5 giây; bong bóng ẩn tới khi ngồi.
- AC-2: Ăn xong: quay lưng đi ra cửa trong 2–3 giây rồi ẩn; khách sau (vòng 1: 3 giây sau) không chồng lên khách đang ra.
- AC-3: Chủ nợ cũng đi vào như khách.

### F-074 Mặt khách lúc ăn theo kết quả — [YC] "chỉ biểu hiện đúng 0,5 giây sau khi ăn rồi trong lúc ăn lại về mặt vui"
- AC-1: Sai món / thấp hơn yêu cầu ≥ 2 bậc: **suốt lúc ăn** mặt giận, nhai chậm, lắc đầu ngắt quãng.
- AC-2: Thấp hơn 1 bậc: suốt lúc ăn mặt buồn, đầu cúi.
- AC-3: Đạt: mặt đang ăn (vui) như cũ; Tuyệt hảo trở lên: thỉnh thoảng nhảy cẫng.

### F-075 Dao và động tác sơ chế — [YC]
Kịch bản: đặt nguyên liệu lên thớt, giữ chuột → dao trên thớt chặt theo nhịp, miếng vụn văng; trứng: gõ cạnh thớt 2 lần, nứt, ra bát trứng đánh.
- AC-1: Thớt có con dao (model Blender, thiếu thì khối) nằm cạnh khi không dùng.
- AC-2: Giữ chuột: động tác theo cột "Sơ chế" (constitution §4.5): **thái / cắt / chặt** = dao chặt lên xuống (chặt mạnh hơn); **đập, đánh** (trứng) = trứng gõ 2 lần rồi nứt; **nhặt, rửa / rửa** = giọt nước bắn + nguyên liệu lắc; **bóc vỏ / tách mai / tách bông** = mảnh vỏ / bông bật ra; **lọc** = dao lướt ngang. Thả chuột thì dừng.
- AC-3: Mỗi nhát có nảy nhẹ trên nguyên liệu (Art Bible §11 nảy 10–15%).

### F-076 Đèn và công tắc — [YC], [TL] "chỉ tối hình"
- AC-1: Cạnh cửa Góc 1 có công tắc (đồ vật, bấm bật / tắt); trần có đèn treo ở 4 góc. Đầu ngày đèn tắt.
- AC-2: Trời tối dần từ 17:00, tối hẳn 19:00 (đi theo đồng hồ ngày): đèn tắt thì phòng tối (đèn chính còn ~40%); bật thì sáng ấm, bóng đèn sáng.
- AC-3: Chỉ đổi hình: bậc, chữ, bảng vẫn đọc được. Ngày mất điện: công tắc không có tác dụng (nến như cũ).

### F-078 Bảng Tab: relic và cửa hàng — [YC]
- AC-1: Nhấn **Tab** khi đang chơi: mở bảng (tạm dừng như Esc), 2 thẻ **Relic** / **Cửa hàng**; Tab / Esc đóng.
- AC-2: Thẻ Relic: mọi relic đang có (tên, độ hiếm, hiệu ứng), số đang có / tối đa; nút **Bỏ** mỗi relic (bấm 2 lần để chắc). Đang mua mà giá đầy: lời nhắc bảo nhấn Tab, thẻ Relic ở chế độ chọn relic bỏ (bỏ xong lấy relic đang chờ); bấm relic trên giá Góc 3 vẫn được như cũ.
- AC-3: Thẻ Cửa hàng: máy gacha đồ ăn, vé đen, nước sốt thần thánh — giá, số đang có / tối đa, tỉ lệ (sốt, vé đen; máy giấu tỉ lệ, Điều 4), nút Mua; trước ngày 4 ghi "mở từ ngày 4".
- AC-4: Bỏ quầy vật phẩm trong phòng. Đồ đã mua mới hiện ở Góc 2: máy gacha đồ ăn (sau khi mua), lọ nước sốt (khi còn sốt), khe vé đen (khi còn vé đen).
- Constitution 1.8 Điều 9 thêm ngoại lệ: bảng Tab là màn ngoài phòng (như menu tạm dừng) nên được dùng nút.

### F-080 Thêm khách theo Art Bible §5–6 — [YC]
- AC-1: ≥ 6 kiểu khách khác hình bóng (tóc, mũ, phụ kiện trên đầu), áo dùng màu nhân vật của bảng màu, không dùng 6 màu bậc (Art Bible §4; trừ bà cụ là khối gốc của cảnh, áo bạc hà có từ sprint 1); chủ nợ giữ như cũ. Hai khách liền nhau khác kiểu.
- AC-2: Tay cầm đũa cùng màu áo khách.

### F-081 Hướng dẫn tổng hợp — [YC] "tổng hợp lại tất cả để bổ sung thêm phần tutorial"
Bài học hiện một lần mỗi ván khi hướng dẫn đang bật (cùng công tắc Cài đặt):
- Ngày 1: 10 bước cũ + Tab; trời tối lần đầu → bật đèn.
- Ngày 2: relic miễn phí (Góc 3), đổi hàng, Tab xem relic.
- Ngày 4: nhiều khách + thanh kiên nhẫn, Tab → Cửa hàng, quá lửa, tủ lạnh.
- Ngày mất điện: nến, công tắc không dùng. Ngày 21: để dành món cho chủ nợ.
- AC-1: Chơi tiếp từ bản lưu ngày ≥ 2 vẫn có bài của ngày đó.

## Blender (impl plan sprint 8)

Người dựng: Claude bằng script Python của Blender 5.3 (bạn yêu cầu 21/11 "tạo prj blender vào thư mục dự án này và làm các task blender"). Nguồn `.blend` + script ở `SourceArt/blender/` (Art Bible §14), FBX xuất vào Unity.

### F-070 Đường ống — impl plan F-070
- AC-1: FBX đặt đúng tên trong `Assets/QuanAnHope/Resources/Models/<Nhom>/` thì game tự dùng (không code riêng từng model). **Lệch impl plan:** thêm `Resources/` vì Unity chỉ nạp được file lúc chạy từ thư mục Resources.
- AC-2: Thiếu file → hình ghép khối cũ, không lỗi, không màu hồng.
- AC-3: Test EditMode kiểm mọi FBX: 1 đơn vị = 1 m (kích thước trong khoảng hợp lý), gốc ở đáy giữa, tam giác ≤ ngân sách §14, ≤ 3 vật liệu, tên `SM_`, có empty `Face`.
- AC-4: Model nhận ToonFlat 2 tông + viền inverted hull theo pháp tuyến.
- AC-5: Mặt theo bậc đặt theo empty `Face`.

### F-071 Nguyên liệu vòng 1 — AC như impl plan (6 loại × nguyên + sơ chế; ảnh trước / sau `docs/sprint-8/ui/`). AC-3 (người khác gọi đúng tên) = CẦN NGƯỜI KIỂM.
### F-072 Đồ bếp — chảo, nồi, vỉ nướng, thớt, dao, đĩa, thùng gỗ 2 loại, rương đột phá; va chạm giữ nguyên; giả lập PlayMode cũ xanh.

## Ngoài phạm vi
Nhân vật Blender có xương (sprint 9), texture vẽ tay, nguyên liệu vòng 2–4 bằng Blender (sang sprint 9/11 theo impl plan §1), đổi số nợ.
