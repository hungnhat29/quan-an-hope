# Roadmap 10 tuần — Quán ăn của Hope

Lập ngày 14/11/2026 · Nguồn: `specs/constitution.md` v1.1, `docs/art-bible/` v1.0, project Unity hiện có (`My project/Assets/BepNhanPham`), `balance/`.
Khung thời gian: 15/11/2026 → 23/01/2027. Tuần 1–8 là phần bắt buộc; tuần 9–10 là hoàn thiện và đệm.

## Nguyên tắc xếp lịch

1. **Luật chơi trước, hình ảnh sau.** Căn phòng 4 góc đã dựng bằng khối cơ bản (`BuildNhaHang.cs`), đủ để chơi thử. Toàn bộ việc Blender dời về tuần 7–8; game vẫn chơi được trọn vẹn nếu Blender trễ.
2. **Mỗi sprint kết thúc bằng một bản chơi thử được**, dài hơn bản trước.
3. **Mỗi tính năng đi theo Điều 8:** spec → plan → tasks → test đỏ → code → chơi thử.
4. **Logic là C# thuần** (không phụ thuộc `UnityEngine`), đọc số từ dữ liệu (Điều 2), có test EditMode (Điều 3). Nhờ vậy đổi model Blender về sau không đụng tới luật chơi.

## Tổng quan

| Sprint | Ngày | Chủ đề | Bản chơi thử cuối sprint | Mục constitution |
| --- | --- | --- | --- | --- |
| 1 | 15–21/11 | Lát cắt dọc vòng 1 | Chơi ngày 1–3, trả nợ 400G đầu ngày 4 | §3, §4.1–4.3, §5.1–5.3, §6.1–6.5 |
| 2 | 22–28/11 | Relic | Vòng 1 có cửa hàng relic, gom bộ được | §7, §4.4 |
| 3 | 29/11–5/12 | Vòng 2 | Chơi tới ngày 9 (nợ 2.200G), có ngày mất điện | §5.5–5.7, §6.6, §8 |
| 4 | 6–12/12 | Vòng 3–4 và boss | **Alpha:** chơi đủ 22 ngày, thắng/thua thật | §2 lịch 22 ngày, §5.4, §6.2–6.3, §8 boss |
| 5 | 13–19/12 | Khung game | Menu, kết thúc ngày, cốt truyện, chữ TextMeshPro toàn bộ | Điều 5, Điều 6, §2 |
| 6 | 20–26/12 | Cân bằng và cảm giác | **Beta nội dung:** đạt 3 mốc Điều 1 trong game thật | Điều 1, §9 |
| 7 | 27/12–2/1 | Blender đợt 1: đồ vật | Đồ vật chính là model `.fbx` | Điều 6, Điều 7 |
| 8 | 3–9/1 | Blender đợt 2: nhân vật | Hope, khách, chủ nợ có model và animation | Điều 7 |
| 9 | 10–16/1 | Hoàn thiện | Bản build Windows cho người ngoài chơi | — |
| 10 | 17–23/1 | Đệm và phát hành thử | Release candidate | — |

## Chi tiết từng sprint

### Sprint 1 — Lát cắt dọc vòng 1 (15–21/11)
Kế hoạch chi tiết: [sprint-1/impl-plan.md](sprint-1/impl-plan.md).
Nền dự án (Git, LFS, test framework, nhập `balance.json` thành ScriptableObject), gacha nguyên liệu, nấu ăn, khách và trả tiền, vòng ngày, trả nợ ngày 4.

### Sprint 1b — Chuyển sang 3D (làm ngày 14/11, theo nghiệm thu sprint 1)
Constitution v1.2 (Điều 5, Điều 9), Art Bible v1.1. Người chơi tự đi lại góc nhìn thứ nhất, tương tác bằng tâm ngắm; thông tin nằm trên đồ vật; HUD chỉ số liệu. Spec: `specs/sprint-1/F-007-choi-3d/spec.md`.

**Áp dụng cho mọi sprint sau:** mỗi tính năng mới phải có đồ vật 3D để bấm và chỗ đặt thông tin trên đồ vật (Art Bible §8, §13); mỗi giả lập PlayMode đi bằng `PlayerController` và bấm qua tâm ngắm, không gọi tắt vào luật.

### Sprint 2 — Relic (22–28/11)
- Cửa hàng relic ở Góc 3: 3 relic là vật 3D trên 3 bệ, giá và hiệu ứng trên thẻ dựng trước bệ; bấm để mua bằng vé trắng; relic đã có đặt lên giá 8 ô (tối đa 8, đầy thì bỏ một cái).
- Hiệu ứng của 12 relic bộ + 10 relic lẻ có tác dụng trong vòng 1–2; thưởng bộ 2/3/4 món.
- Thùng đột phá (0% + relic), 2 món, 70/30.
- Relic chỉ có tác dụng với hệ thống chưa làm (Tủ lạnh 2 cửa, Hũ sốt cũ, Đồng hồ cát, Máy pha cà phê) được mua và giữ được, hiệu ứng nối ở sprint 3.
- Test: mỗi hiệu ứng một test; test thống kê cho thùng đột phá.

### Sprint 3 — Vòng 2 (29/11–5/12)
- Thời gian chờ: khoảng an toàn theo vòng, −1 điểm mỗi 3 giây, tối đa −20.
- Phục vụ 3 khách cùng lúc.
- Tủ lạnh 2 ô, tụt bậc sau 3 ngày.
- Cửa hàng vật phẩm: nước sốt thần thánh, máy gacha đồ ăn (tỉ lệ ẩn), vé đen và gacha vé đen.
- Ngày 8 mất điện: giấu khoảng bậc, −0,3 điểm, +15 giây an toàn, ánh nến.
- Trả nợ 2.200G đầu ngày 9.

### Sprint 4 — Vòng 3–4 và boss (6–12/12) → Alpha
- Vòng 3: 5 khách/ngày, khó tính; ngày 14 có 8 khách.
- Nước sốt từ ngày 9 có 4% ra Món Hy Vọng (Huyền thoại).
- Mua relic bằng vé đen (3 lựa chọn Hiếm trở lên, một cái thuộc bộ đang nhiều nhất).
- Vòng 4 và chủ nợ ngày 21 (dễ tính 5%), thưởng 11.500G, trả nợ 26.200G, màn thắng.
- Bot chơi tự động viết bằng C# trên chính logic game, chạy 4.000 ván mỗi kiểu, đối chiếu với `sim_balance.py`.

### Sprint 5 — Khung game (13–19/12)
- Menu chính, màn kết thúc ngày, màn cốt truyện mở đầu và kết thúc (ảnh tĩnh + chữ).
- Thay toàn bộ ảnh chữ PNG bằng TextMeshPro, font có dấu tiếng Việt.
- *Đề xuất, chưa có trong constitution, cần bạn duyệt:* lưu/tải game theo ngày; hướng dẫn trong ngày 1.

### Sprint 6 — Cân bằng và cảm giác (20–26/12) → Beta nội dung
- Chơi thử trọn 22 ngày nhiều lần; chỉnh `balance.json`, chạy lại mô phỏng, giữ 3 mốc Điều 1.
- Kiểm lại thời gian chờ bằng playtest (constitution §6.6 ghi rõ cần kiểm).
- Hiệu ứng không cần Blender: hạt, rung, nảy, chuyển cảnh, âm thanh `.wav`.
- **Bắt đầu Blender:** cài đặt, mỗi ngày 30–45 phút bài cơ bản (di chuyển, dựng khối, tô màu phẳng, xuất `.fbx`). Chưa đưa gì vào game.

### Sprint 7 — Blender đợt 1: đồ vật (27/12–2/1)
- Bài đầu tiên: một cái nồi, xuất `.fbx`, đưa vào Unity với shader `ToonFlat` + `Outline` có sẵn.
- Lần lượt: thùng nguyên liệu, máy thả thùng, máy gacha đồ ăn, thớt, đĩa, chuông, tủ lạnh.
- Thay khối cơ bản bằng prefab; logic không đổi.

### Sprint 8 — Blender đợt 2: nhân vật (3–9/1)
- Một thân khách dùng chung, đổi màu và mặt 2D (`FaceSwap` có sẵn); Hope; chủ nợ.
- Rig đơn giản, 3 animation: đứng chờ, vui, giận.
- Nếu trễ: giữ nhân vật khối cầu hiện tại, chỉ thay mặt. Game vẫn phát hành được.

### Sprint 9 — Hoàn thiện (10–16/1)
Sửa lỗi, tối ưu, cài đặt âm lượng và độ phân giải, build Windows, mời 3–5 người ngoài chơi thử.

### Sprint 10 — Đệm (17–23/1)
Xử lý phản hồi playtest, hoàn tất việc trễ từ các sprint trước, đóng gói release candidate.

## Rủi ro

| Rủi ro | Cách xử lý |
| --- | --- |
| Chưa biết Blender | Dời về tuần 7–8, học dần từ tuần 6, luôn có phương án giữ khối cơ bản |
| Một sprint trễ | Tuần 10 là đệm; sprint 5 và 9 có thể co lại |
| Số liệu không cho cảm giác như mô phỏng | Bot C# ở sprint 4 chạy trên logic thật; sprint 6 dành riêng cho cân bằng |
| Cảnh hiện tại dựng theo thiết kế cũ (bảng tỉ lệ, "tối đa 10 relic", "3 vé/thùng") | Constitution thắng; sửa dần khi làm tới từng góc |
| Chưa có Git | Việc đầu tiên của sprint 1 |
