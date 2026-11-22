# Roadmap — Quán ăn của Hope

Cập nhật 15/11/2026 (bản 2) · Nguồn: `specs/constitution.md` **v1.4**, `docs/art-bible/` v1.3, tổng kết sprint 1 (`.dev-memory/RETRO-sprint-1.md`), ghi chú của bạn ngày 14/11 (`.dev-memory/INBOX.md`).
Khung thời gian không đổi: 15/11/2026 → 23/01/2027 (10 tuần). Sprint 1 xong sớm ngày 14/11, nên 10 tuần này chứa sprint 2 → 11.

## Thay đổi so với bản 1

- **Sprint 1 đã đóng** (14/11), gồm cả phần làm lại 3D, mặt theo bậc, khách gọi món.
- **Thêm sprint 2 "Nhịp ngày mới"** vào tuần trống 15–21/11: đưa constitution 1.4 vào game (ngày theo giờ, khách không giới hạn, thùng nhỏ, thời gian sơ chế và nấu dài hơn, nợ mới) và sửa hai điểm bạn chê về hình (nguyên liệu, quá trình nấu).
- Các sprint Relic, Vòng 2, Vòng 3–4, Khung game, Cân bằng, Blender, Hoàn thiện lùi một số thứ tự nhưng **giữ nguyên tuần lịch** như bản 1.
- Blender đợt 1 ưu tiên **nguyên liệu** trước đồ vật (bạn chê nguyên liệu xấu).

## Nguyên tắc xếp lịch

1. **Luật chơi trước, hình ảnh sau**, nhưng mỗi tính năng phải nhìn ra được (bài học sprint 1): có đồ vật 3D để bấm, thông tin nằm trên đồ vật, mỗi thao tác có hình và chuyển động.
2. **Mỗi sprint kết thúc bằng một bản chơi thử được**, dài hơn bản trước, kèm bản build Windows toàn màn hình.
3. **Mỗi tính năng đi theo Điều 8:** spec → "kịch bản 10 giây" cho bạn duyệt → test đỏ → code → giả lập PlayMode có chụp ảnh → bạn chơi thử.
4. **Đổi luật ảnh hưởng tiền hoặc khách thì mô phỏng trước, code sau** (`balance/sim_balance.py`, Điều 1).
5. **Logic là C# thuần**, đọc số từ dữ liệu, có test EditMode. Đổi model Blender không đụng luật.

## Tổng quan

| Sprint | Ngày | Chủ đề | Bản chơi thử cuối sprint | Mục constitution | Impl plan |
| --- | --- | --- | --- | --- | --- |
| 1 | xong 14/11 | Lát cắt dọc vòng 1, 3D | Chơi ngày 1–3, trả nợ đầu ngày 4 | §3–§6 | [sprint-1](sprint-1/impl-plan.md) |
| 2 | 15–21/11 | Nhịp ngày mới + hình bếp | Vòng 1 theo số 1.4: đồng hồ 9:00–21:00, khách liên tục, nợ 750G; nguyên liệu và nồi chảo nhìn ra món ăn | §2, §3, §4.2–4.5, §5.8–5.9, §6.1–6.2 | [sprint-2](sprint-2/impl-plan.md) |
| 3 | 22–28/11 | Relic | Vòng 1 có cửa hàng relic, gom bộ được, thùng đột phá | §7, §4.4 | [sprint-3](sprint-3/impl-plan.md) |
| 4 | 29/11–5/12 | Vòng 2 | Chơi tới ngày 9 (nợ 2.900G): 3 khách cùng lúc, giờ chờ, tủ lạnh, cửa hàng, mất điện | §5.5–5.7, §5.9, §6.1, §6.6, §8 | [sprint-4](sprint-4/impl-plan.md) |
| 5 | 6–12/12 | Vòng 3–4 và chủ nợ | **Alpha:** chơi đủ 22 ngày, thắng / thua thật | §2, §5.4, §6.3, §6.7, §7.1, §8 | [sprint-5](sprint-5/impl-plan.md) |
| 6 | 13–19/12 | Khung game | Menu, cốt truyện, lưu theo ngày, hướng dẫn ngày 1, cài đặt | Điều 5, Điều 6, §2 | [sprint-6](sprint-6/impl-plan.md) |
| 7 | 20–26/12 | Cân bằng và cảm giác | **Beta nội dung:** đạt 3 mốc Điều 1 bằng bot chạy trên game thật; âm thanh, hiệu ứng | Điều 1, §9 | [sprint-7](sprint-7/impl-plan.md) |
| 8 | 27/12–2/1 | Blender đợt 1: nguyên liệu, đồ vật | Nguyên liệu và đồ bếp là model `.fbx` | Điều 6, Điều 7 | [sprint-8](sprint-8/impl-plan.md) |
| 9 | 3–9/1 | Blender đợt 2: nhân vật | Khách, Hope, chủ nợ có model và animation | Điều 7 | [sprint-9](sprint-9/impl-plan.md) |
| 10 | 10–16/1 | Hoàn thiện | Bản build Windows cho người ngoài chơi | — | [sprint-10](sprint-10/impl-plan.md) |
| 11 | 17–23/1 | Đệm và phát hành thử | Release candidate | — | [sprint-11](sprint-11/impl-plan.md) |

## Ghi chú của bạn ngày 14/11 nằm ở đâu

| Ghi chú | Xử lý | Sprint |
| --- | --- | --- |
| Hình nguyên liệu đỡ xấu, chân thực hơn | Bước 1: dựng lại bằng nhiều khối ghép + chi tiết (cuống, lá, vân, lát cắt) trong code. Bước 2: model Blender | 2, rồi 8 |
| Sau khi cho vào chỗ nấu: xấu, lộn xộn, không giống nấu ăn | Đồ vào chảo / nồi xếp theo lớp, đổi màu khi chín, có dầu / nước sốt, đảo, hơi; múc ra đúng hình món | 2 |
| Khách không giới hạn, tới hết 21h | Constitution 1.4 §6.2: đồng hồ ngày, khách vào liên tục khi quán mở | 2 (lần lượt), 4 (3 khách cùng lúc) |
| Tăng thời gian chuẩn bị và nấu | Constitution 1.4 §4.5, §5.9: sơ chế ×3, nấu ×1,5 | 2 |
| Tăng nợ | Constitution 1.4 §2: 750 / 2.900 / 10.500 / 32.000G | 2 (vòng 1), 4, 5 |
| Thùng 3 phụ / 2 chính / 1 đột phá, ngày 3 và relic chỉnh theo | Constitution 1.4 §4.2–4.4, §7.2 | 2 (thùng), 3 (đột phá, relic) |

## Chi tiết từng sprint

Mỗi sprint có impl plan riêng (cột cuối bảng trên). Tóm tắt:

### Sprint 2 — Nhịp ngày mới + hình bếp (15–21/11)
Chép `balance/next/*` thành bộ số chính, nhập lại vào game, sửa test theo số mới. Đồng hồ ngày trên HUD, khách vào liên tục tới 21:00, hết giờ thì phục vụ nốt khách đang ngồi. Nguyên liệu dựng lại bằng khối ghép. Trong chảo / nồi / vỉ: đồ xếp gọn, chín dần, có chuyển động nấu.

### Sprint 3 — Relic (22–28/11)
Cửa hàng relic ở Góc 3: 3 relic là vật 3D trên 3 bệ, thẻ giá và hiệu ứng trước bệ, mua bằng vé trắng, giá 8 ô. 12 relic bộ + 10 relic lẻ, thưởng bộ 2/3/4 món. Thùng đột phá 1 món.

### Sprint 4 — Vòng 2 (29/11–5/12)
3 khách cùng ngồi, mỗi người một món gọi. Giờ chờ và độ dễ tính giảm dần. Tủ lạnh. Cửa hàng vật phẩm: nước sốt, máy gacha đồ ăn, vé đen. Quá lửa. Ngày 8 mất điện. Trả nợ 2.900G đầu ngày 9.

### Sprint 5 — Vòng 3–4 và chủ nợ (6–12/12) → Alpha
Vòng 3 khách khó tính, vòng 4, Món Hy Vọng, mua relic bằng vé đen, chủ nợ ngày 21 lúc 20:00, thưởng 11.500G, nợ 32.000G, màn thắng.

### Sprint 6 — Khung game (13–19/12)
Menu chính, cốt truyện mở đầu và kết thúc, lưu / tải theo ngày, hướng dẫn trong ngày 1, cài đặt (âm lượng, độ nhạy chuột, độ phân giải), thay ảnh chữ PNG còn lại.

### Sprint 7 — Cân bằng và cảm giác (20–26/12) → Beta nội dung
Bot C# chạy 4.000 ván mỗi kiểu trên chính logic game, đối chiếu `sim_balance.py`. Đo thời gian thật một món để thay giả định của mô phỏng. Âm thanh, hiệu ứng hạt, rung. Bắt đầu học Blender 30–45 phút mỗi ngày.

### Sprint 8 — Blender đợt 1: nguyên liệu và đồ vật (27/12–2/1)
22 nguyên liệu (10 phụ, 8 chính, 4 đột phá) rồi nồi, chảo, vỉ, thùng, đĩa, chuông. Thay khối ghép bằng prefab; logic không đổi.

**Làm sớm 21/11/2026** (bạn yêu cầu): dựng bằng script Blender (`SourceArt/Blender/`): 6 nguyên liệu vòng 1 × nguyên + sơ chế, chảo, nồi, vỉ, thớt, dao, đĩa, 2 thùng, rương đột phá. Còn lại sang sprint 9/11: 16 nguyên liệu vòng 2–4 + đột phá, chuông. Cùng đợt: sửa trước sprint (đèn + công tắc, relic ngày 2 miễn phí + đổi hàng, bảng Tab, khách đi vào/ra, 6 kiểu khách, dao + động tác sơ chế, nhịp khách theo ngày, hướng dẫn theo sự kiện) — constitution 1.8, `specs/sprint-8/spec.md`.

### Sprint 9 — Blender đợt 2: nhân vật (3–9/1)
Một thân khách dùng chung + mặt 2D, Hope, chủ nợ; 3 animation tối thiểu. Trễ thì giữ nhân vật khối cầu.

### Sprint 10 — Hoàn thiện (10–16/1)
Sửa lỗi, tối ưu, build Windows, mời 3–5 người ngoài chơi thử.

### Sprint 11 — Đệm (17–23/1)
Xử lý phản hồi, việc trễ, đóng gói release candidate.

## Rủi ro

| Rủi ro | Cách xử lý |
| --- | --- |
| Số 1.4 dựa trên giả định "một món tốn thêm 20–30 giây đi lại" chưa đo | Sprint 2 đo bằng người thật trong buổi chơi thử; lệch thì chỉnh `balance/make_next.py` và chạy lại mô phỏng trước sprint 4 |
| Ngày dài 8 phút × 21 ngày ≈ 2,8 giờ một ván | Lưu theo ngày (sprint 6); Q-22 cho bạn chọn lại độ dài ngày |
| Chưa biết Blender | Học dần từ sprint 7; sprint 2 đã làm hình "tạm đẹp" bằng code nên game phát hành được nếu Blender trễ |
| Một sprint trễ | Sprint 11 là đệm; sprint 6 và 10 co lại được |
| Làm xong mới biết bạn muốn khác (sprint 1 bị 3 lần) | Kịch bản 10 giây + ảnh phác cho bạn duyệt ở ngày 1 mỗi sprint |
