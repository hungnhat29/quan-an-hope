# Bếp Nhân Phẩm · Mô tả animation cốt truyện (6 trang truyện)

> Đi kèm: `Trang_1.png` … `Trang_6.png` (1920×1080) và file `.svg` cùng tên, trong đó mỗi lớp giấy là một nhóm `<g filter="url(#ppN)">` (N = độ sâu 0–5).
> Cơ sở: báo cáo *Paper craft diorama và truyện tranh.md* và Art Bible v1.
> **Lưu ý về 소맥거핀:** chưa nguồn nào ghi lại kỹ thuật hoạt hình của kênh (frame rate, rig, camera). Các nhịp "kiểu 소맥거핀" dưới đây là **giả thuyết theo quy ước hoạt hình đời thường Hàn Quốc (일상툰)**: giữ dáng lâu, đổi mặt bằng cách hoán đổi, bung chuyển động ở câu chốt, zoom giật, đứng hình im lặng. Trước khi khóa số khung, cần xem trực tiếp 3–5 tập và đếm khung. Chỉ vay **công thức hài**, không vay nhân vật hay thiết kế.

---

## 1. Quy tắc chung

### 1.1 Sân khấu giấy (layered diorama)

Mỗi khung truyện là một "hộp sân khấu" riêng, gồm các tấm phẳng đặt trên trục Z thật trước camera phối cảnh (URP Universal Renderer, **không** dùng 2D Renderer, FOV 25–30°).

| Lớp SVG | Tên | Khoảng cách D (D₀ = mặt phẳng nhân vật) | Parallax f = D₀/D | Ghi chú |
|---|---|---|---|---|
| `pp0` | BG – trời/tường | 5 D₀ | 0,20 | giảm bão hòa 28%, ám kem 12%, không viền trắng |
| `pp1` | Far – phố xa, kệ | 2,5 D₀ | 0,40 | giảm bão hòa 15% |
| `pp2` | Mid – quầy, mặt tiền | 1,5 D₀ | 0,67 | |
| `pp3` | Nhân vật | D₀ | 1,00 | **tương phản cao nhất, màu ấm nhất**, lấy nét DOF tại đây |
| `pp4` | Near – quầy trước, đạo cụ | 0,75 D₀ | 1,33 | |
| `pp5` | FG – bụi cây, đèn lồng | 0,5 D₀ | 2,00 | được phép nhòe DOF, không bao giờ che mặt nhân vật |

- **Bóng giữa các lớp:** tăng theo độ sâu (`dx = 1,5 + 1,6·N`, `dy = 3 + 2,6·N`, blur `1,5 + 1,5·N` px, độ mờ 22–47%), màu nâu ấm `#2B1A10`, không dùng đen. Trong Unity: một đèn directional đổ bóng mềm, cùng một hướng cho mọi lớp.
- **Viền cắt giấy:** viền kem `#FFFDF6` dày 3 px bên ngoài nét mực `#2B2321` (Art Bible). Lớp BG không có viền.
- **Hạt giấy:** một lớp grain chung toàn màn hình, cộng thêm noise riêng mỗi lớp (độ mờ 7–10%).
- **Giữ tỉ lệ khi đẩy lớp ra sau:** scale = D/D₀ để kích thước trên màn hình không đổi.

### 1.2 Thời gian (stop-motion có chọn lọc)

| Thành phần | Nhịp |
|---|---|
| Thân nhân vật, đạo cụ cầm tay | **on twos: 12 hình/s** (mỗi pose giữ 2 frame ở nền 24) |
| Biểu cảm mặt (mắt vòng, miệng "w") | **hoán đổi sticker**, đổi đúng nhịp beat, không nội suy |
| Mưa, hạt giấy vụn, confetti, khói | on threes (8 hình/s) |
| Boil viền (rung nét): `feDisplacementMap` scale 2–3 px, đổi seed | mỗi 2 frame (12/s) |
| Camera, UI, nút "Tiếp", chữ hiện dần | **mượt 60 fps**, không bao giờ step |
| Motion blur | **tắt**; khi chuyển động nhanh dùng speed lines vẽ tay |

Cài đặt: *Hiệu ứng stop-motion: Đầy đủ / Giảm / Tắt*. Mức "Giảm" tắt boil và chuyển nhân vật sang on ones.

### 1.3 Ngữ pháp khung truyện

- **Thứ tự đọc:** trái → phải, tầng trên → tầng dưới. Đường chia khung chéo dùng để báo nhịp nhanh hoặc căng.
- **Cách hiện:** mỗi khung trượt vào từ hướng của đường chéo (8 frame, ease-out), sau đó các lớp **bung ra theo Z**, từ BG đến FG, mỗi lớp trễ 2 frame. Hiệu ứng như mở một cuốn sách pop-up.
- **Thời lượng mỗi khung** = 1,2 s + thời gian đọc chữ (12–15 ký tự/s cho tiếng Việt, đây là ước lượng cần test) + thời lượng gag. Người chơi chạm để qua sớm. Có nút đọc lại trang.
- **Chữ tượng thanh (SFX)** là một lớp giấy riêng (`pp4`). Chữ bật vào khung bằng squash 120% → 95% → 100% trong 6 frame.

### 1.4 Nhịp hài (giả thuyết "kiểu 소맥거핀")

```
DỰNG (giữ dáng, 0,8–1,2 s) → LEO THANG (2–3 bước, mỗi bước nhanh hơn) → CÂU CHỐT (bung 4–8 frame vẽ tay/smear + zoom giật 115%)
→ ĐỨNG HÌNH (1–1,5 s, nhạc tắt hẳn, chỉ còn tiếng ồn phòng) → nhạc vào lại
```

- **Zoom giật (punch-in):** camera nhảy 100% → 115% trong 0 frame (cắt cứng), giữ, rồi trả về trong 10 frame.
- **Rung màn hình:** biên độ 6 px, 4 lần đổi vị trí ở nhịp 12/s. Chỉ dùng ở SFX lớn (ĐÙNG, LỘ RỒI, XOẸT).
- **Biểu tượng cảm xúc (manpu):** giọt mồ hôi, gân giận, mây u ám, tim, lấp lánh. Mỗi biểu tượng "pop" 3 frame rồi đung đưa on threes.

---

## 2. Bộ hành động dùng chung (thư viện pose)

| Mã | Mô tả | Khung (on twos) |
|---|---|---|
| `idle_breath` | Thân co 98% ↔ 100% | 4 pose × 2 frame, lặp |
| `shock_pop` | Mắt vòng phóng 118%, người bật lên 12 px, có giọt mồ hôi | anticipation 2 → pop 1 → giữ |
| `walk_sad` | Đầu gục, bước nảy thấp, khung đi 6 pose | 12 frame/bước |
| `cheer` | Hai tay vung (ARMS.cheer), squash ở đáy | 3 pose |
| `point` | Chỉ tay: anticipation lùi 6 frame, đâm tới 2 frame, giữ | |
| `tear_paper` | Kéo căng (2 pose), xé (1 frame smear), mảnh bay on threes | |
| `cry_flow` | Dòng lệ xanh `#9CC7D8` nhấp nhô on threes | lặp |

Nguyên tắc hình khối (Art Bible): nhân vật **tròn** thì nảy mềm (squash 90/110). **Chủ nợ** là hình chữ nhật nên cứng: không squash, chỉ trượt và dừng gắt. **Kẻ phá đám** là tam giác nên chuyển động lén lút, nghiêng 8–12°.

---

## 3. Phân cảnh theo trang

Ký hiệu: ⏱ thời lượng · 🎥 camera · 🎭 diễn xuất · 🔊 âm thanh · 🧱 các lớp nổi bật.

### Trang 1 · Bị đuổi việc (khoảng 22 s)

**1.1 Văn phòng (khung trên trái)** ⏱ 4 s
- 🎥 Bắt đầu tĩnh. Khi sếp hét thì zoom giật 115% vào bóng thoại.
- 🎭 Sếp: anticipation ngả người sau 4 frame, rồi đâm tay chỉ (`point`). Gân giận pop. Hope làm `shock_pop`, hai giọt mồ hôi bắn ra on threes.
- 🔊 Tiếng máy lạnh rè rè, nhạc văn phòng nhẹ. Đến chữ "ĐUỔI!!" thì nhạc tắt.
- 🧱 Cây cảnh ở FG `pp5` trôi nhanh nhất khi camera trượt nhẹ 20 px sang phải.

**1.2 Phố hoàng hôn** ⏱ 4 s
- 🎥 Camera trượt ngang 120 px theo Hope. Các lớp parallax 0,2 / 0,4 / 0,67 / 1 / 2, đây là khung trình diễn chiều sâu chính của trang.
- 🎭 `walk_sad`. Mây u ám trên đầu rung on threes. Củ cà rốt trong hộp nảy theo bước.
- 🔊 Tiếng quạ, gió. Caption "…và thế là hết." hiện chữ từng ký tự.

**1.3 Cộp… cộp…** ⏱ 3 s
- 🎥 Bóng chủ nợ trườn vào từ phải trước (tấm bóng ở `pp2`), sau đó thân ông ta mới trượt vào ở FG `pp5`, rất gần camera nên to và mờ DOF.
- 🎭 Mỗi tiếng "CỘP" làm khung rung 3 px. Chủ nợ không nảy, chỉ trượt và dừng gắt.
- 🔊 Hai tiếng giày nặng, cách nhau 0,5 s.

**1.4 Trao chìa khóa** ⏱ 4 s
- 🎭 Chủ nợ đưa tay (`give`) trong 6 pose. Chìa khóa lóe sáng (tia lấp lánh pop). Mắt Hope chuyển từ shock sang mắt vòng nhìn theo chìa (hoán đổi 3 lần).
- 🎥 Sau câu thoại, giữ im 1 s trước khi qua khung.

**1.5 Giấy nợ** ⏱ 5 s, **gag chốt trang**
- 🎥 Tờ giấy nợ bay vào từ dưới lên ở FG, xoay −8°. Camera dolly in 8% trong 2 s (mượt).
- 🎭 Bốn dòng hạn hiện lần lượt, mỗi dòng 10 frame. Đến "Ngày 22 ☠" thì dấu "ĐÓNG" đập xuống (squash), Hope ở góc vã ba giọt mồ hôi.
- 🔊 Mỗi dòng một tiếng "tách". Dấu đóng kêu "BỘP", rồi đứng hình 1,2 s.

### Trang 2 · Cơn bão (khoảng 24 s)

**2.1 Mặt tiền trong bão** ⏱ 4 s
- 🧱 Mưa nằm ở hai lớp: một lớp sau mặt tiền (`pp1`, mảnh, chậm) và một lớp trước (`pp4`, dày, nhanh), cả hai on threes. Bụi cây FG ngả theo gió (3 pose lặp).
- 🎭 Sét: trước tiên cả khung nháy trắng 2 frame, sau đó khối sét giấy vàng hiện ra, rồi chữ "ĐÙNG!!" bật vào, kèm rung 6 px.
- 🔊 Mưa đều. Tiếng sấm đến **sau** tia chớp 6 frame.

**2.2 Dột** ⏱ 5 s, **gag chính**
- 🎭 Dựng: một giọt rơi xuống, "tách". Leo thang: giọt thứ hai, thứ ba, nhịp nhanh dần. Câu chốt: Hope đội nồi lên đầu, giọt rơi vào nồi kêu "teng". Đứng hình: mặt khóc `cry_flow`, nhạc tắt.
- 🎥 Zoom giật vào mặt Hope ngay tiếng "teng".

**2.3 Kho gần trống** ⏱ 3 s
- 🎥 Dolly lùi chậm để lộ hai thùng rỗng. Mạng nhện trên kệ đung đưa.
- 🎭 Gió thổi qua, một chiếc lá giấy bay ngang (gag "trống trơn").

**2.4 "Ít thì nấu khéo!"** ⏱ 3 s
- 🎭 Hope chuyển từ buồn sang quyết tâm bằng một smear 1 frame, rồi `cheer`. Củ cà rốt giơ cao lấp lánh. Nền tia sáng xoay 10°/s (mượt).
- 🔊 Nhạc chủ đề vào lại mạnh.

**2.5 Bà cụ khen** ⏱ 4 s
- 🎭 Bà cụ húp, mắt chuyển sang mắt tim (hoán đổi), tim pop lần lượt. Hope ở FG ngượng, má đỏ đậm dần trong 3 bước.
- 🧱 Ở cửa sổ `pp0` thấy trời đã tạnh. Đây là cú chuyển cảm xúc của trang.

### Trang 3 · Đêm mất điện (khoảng 24 s)

**3.1 PHỤT!** ⏱ 2,5 s
- 🎭 Bóng đèn chớp 3 lần (on ones, 2 frame sáng, 2 frame tối), rồi tắt hẳn. Lớp tối `#2A2445` độ mờ 55% phủ lên trong 1 frame.
- 🔊 "Rè… rè… PHỤT", sau đó im lặng hoàn toàn 1 s.

**3.2 "…ơ?"** ⏱ 3 s, **gag kiểu 소맥거핀**
- 🎭 Khung tối hoàn toàn, chỉ còn mắt vòng trắng. Mắt Hope chớp hai lần, sau đó **một đôi mắt nhỏ của khách** chớp ở góc. Đứng hình. Mắt Hope liếc sang, kèm một giọt mồ hôi.
- 🔊 Chỉ có tiếng dế. Giữ cảnh 1,5 s rồi mới qua khung.

**3.3 Thắp nến** ⏱ 3 s
- 🎭 Que diêm xòe (smear 1 frame), ngọn lửa nến nhấp nháy on threes theo 3 hình ngọn lửa.
- 🧱 Vùng sáng là một radial mask ở bán kính 260 px, **rung ±4% theo nhịp 8/s** để ánh sáng chập chờn kiểu giấy.

**3.4 Nấu dưới ánh nến (khung rộng)** ⏱ 7 s, **khung trình diễn**
- 🎥 Truck ngang 160 px từ trái sang phải: từ khách qua hàng nến tới bếp lửa. Bốn vùng sáng lần lượt rõ dần.
- 🎭 Khách gật gù `idle_breath`. Lửa bếp nhảy 4 pose. Hope đảo nồi. Chữ "XÈO XÈO" có màu `#FFC46B`.
- 🧱 Đèn lồng FG `pp5` không thắp, bị nhòe DOF. Đó là khung tiền cảnh tạo chiều sâu.
- 🔊 Xèo xèo, tiếng thìa gõ nồi, nhạc acoustic nhẹ.

**3.5 "Lãng mạn ghê!"** ⏱ 4 s
- 🎭 Cặp khách nghiêng đầu vào nhau từng nấc (3 bước). Tim bay lên on threes và lắc lư.

### Trang 4 · Tin đồn (khoảng 26 s)

**4.1 Dán tờ rơi (đêm)** ⏱ 4 s
- 🎭 Kẻ phá đám bước rón rén, nghiêng 10°. Mỗi tờ rơi bị "dán" bằng một squash. Cười "hí hí hí" với chữ rung on twos.
- 🧱 Trăng ở `pp0`, phố ở `pp1`. Bụi cây FG che một phần, như đang nhìn trộm.

**4.2 Vắng tanh (ngày 10–13)** ⏱ 4 s
- 🎭 Hope sau quầy, mây u ám. Một tờ rơi trượt trên sàn theo gió. Ghế đẩu trống.
- 🎥 Giữ tĩnh lâu (1,5 s) với tiếng đồng hồ tích tắc. Đây là cú đứng hình buồn.

**4.3 "Bắt được rồi nhé!"** ⏱ 4 s
- 🎭 Màn hình camera hiện "REC" nhấp nháy, phát lại cảnh kẻ phá đám dán tờ rơi. Hope đổi sang mắt quyết tâm, lấp lánh pop.
- 🔊 Tiếng "bíp" của máy quay, hiệu ứng tua băng.

**4.4 LỘ RỒI!! (ngày 14)** ⏱ 6 s, **cao trào trang**
- 🎥 Khi Hope chỉ tay thì zoom giật, chữ "LỘ RỒI!!" đập vào và màn hình rung 6 px.
- 🎭 Kẻ phá đám nhảy dựng (`shock_pop`), tờ rơi trong tay tung lên rồi rơi on threes. Khách nổi gân giận lần lượt (trái, rồi phải).
- 🔊 Tiếng ồ của đám đông, sau đó nhạc thắng thế.

**4.5 Danh tiếng trở lại (ngày 15)** ⏱ 4 s
- 🎭 Ba ngôi sao lần lượt rơi vào khung, mỗi sao nảy 2 lần. Hàng khách ở FG nhấp nhô vui vẻ.
- 🎥 Crane lên nhẹ 40 px để lộ biển hiệu.

### Trang 5 · Món cuối cùng (khoảng 30 s)

**5.1 Chủ nợ ở cửa (ngày 21)** ⏱ 5 s
- 🧱 Chủ nợ bị **ngược sáng thành bóng đen**: cánh cửa ở `pp1` sáng kem, bóng hắt dài trên sàn. Hai khách ở FG `pp5` quay đầu lại sợ hãi (shock).
- 🎥 Dolly in chậm 10% trong 3 s. Đây là cú căng thẳng duy nhất không có gag.
- 🔊 Nhạc tắt, gió rít qua cửa, tiếng giày "CỘP" như ở trang 1 (callback).

**5.2 Nấu bằng cả nhân phẩm** ⏱ 5 s
- 🎭 Hope đảo nồi on twos với 6 pose. Lửa bếp 4 pose. Hai chữ "XÈO!" và "XÈO!!" bật luân phiên. Speed lines nền trượt (mượt).
- 🎥 Rung nhẹ 2 px liên tục, tạo cảm giác bếp đang "nóng".

**5.3 HUYỀN THOẠI** ⏱ 5 s, **khoảnh khắc gacha**
- 🎭 Nắp chuông nhấc lên (3 pose, smear), ánh vàng bung ra, nhãn "★ HUYỀN THOẠI ★" màu tím lilac (màu độ hiếm cao nhất) trượt vào và nảy.
- 🔊 Dùng **cùng SFX với lần quay gacha ra món Huyền thoại** trong gameplay, để người chơi nhận ra.

**5.4 "Ự…!!"** ⏱ 4 s, **câu chốt kiểu 소맥거핀**
- 🎭 Chủ nợ đưa thìa vào miệng. Đứng hình 12 frame. Sau đó **mắt vòng mở bật** (hoán đổi 1 frame), các tia nền nổ ra.
- 🔊 Im lặng hoàn toàn, sau đó một tiếng "đing" duy nhất.

**5.5 "Vị này… giống hệt món mẹ ta nấu…"** ⏱ 6 s
- 🎭 Nước mắt chảy `cry_flow`. Bóng ký ức (bà mẹ trong bong bóng suy nghĩ) hiện dần với độ mờ 0 → 85% trong 1 s, bong bóng nảy nhẹ.
- 🎥 Dolly in chậm 6%, nhạc piano vào.

### Trang 6 · Hết nợ, có vợ (khoảng 26 s)

**6.1 XOẸT!! (ngày 22)** ⏱ 4 s
- 🎭 Chủ nợ xé giấy nợ (`tear_paper`), 12 mảnh giấy bay on threes và xoay. Hope `shock_pop`.
- 🔊 "XOẸT" kèm rung. Nhạc vui vào.

**6.2 "Còn một chuyện nữa… con gái ta."** ⏱ 6 s, **gag đổi giọng**
- 🎭 Chủ nợ chỉ tay đầy uy lực (`point`). Con gái ló ra sau quầy với mắt tim, tim bay lên. Mắt Hope chuyển shock → tim (hoán đổi), má đỏ dần 3 bước.
- 🎥 Whip pan 6 frame từ chủ nợ sang con gái (camera mượt, có speed lines), rồi đứng hình.

**6.3 Khai trương lại · đám cưới (khung toàn tầng)** ⏱ 12 s, **khung trình diễn cuối**
- 🎥 Bắt đầu cận đôi uyên ương, sau đó dolly lùi 25% trong 4 s để lộ toàn cảnh 6 lớp: phố ở `pp1`, mặt tiền và đèn lồng ở `pp2`, khách ở `pp3`, đôi uyên ương và chủ nợ ở `pp4`, bụi cây và confetti ở `pp5`.
- 🎭 Confetti rơi on threes ở hai lớp (sau và trước nhân vật). Đèn lồng đung đưa lệch pha. Bà cụ vẫy. Chủ nợ khóc. Nhẫn xoay lấp lánh.
- 🎭 Câu chốt cuối: con dấu "HẾT" đập xuống góc phải (squash 130% → 100%), rung, rồi đứng hình 2 s.
- 🔊 Nhạc chủ đề phiên bản đầy đủ, tiếng vỗ tay và pháo giấy.

---

## 4. Ghép vào Unity 6 URP

1. **Tách lớp:** mỗi `<g filter="url(#ppN)">` trong SVG xuất thành PNG (kèm viền kem nướng sẵn), đặt trên quad alpha-clip tại Z = D theo bảng 1.1.
2. **Khung:** dùng mask stencil với hình tứ giác của khung (đường chia chéo). Viền khung là một quad mực riêng nằm trên mọi lớp.
3. **Ánh sáng:** một directional light đổ bóng mềm. Riêng trang 3 dùng spot light màu `#FFC46B` cho từng nến, cường độ rung ±4% ở 8 Hz.
4. **Hậu kỳ:** Bokeh DOF lấy nét ở `pp3`, vignette, film grain, **tắt motion blur**.
5. **Bước khung:** `Animator.Update(1/12f)` gọi thủ công cho nhân vật. Camera, Timeline và UI chạy theo deltaTime thường.
6. **Dữ liệu:** mỗi trang là một `ComicPage` ScriptableObject gồm danh sách khung, và mỗi khung có lớp, cue camera, cue âm thanh, thời lượng và text. Có lệnh *Đọc lại trang* và *Bỏ qua chương*.

## 5. Việc cần kiểm chứng trước khi khóa

- [ ] Xem 3–5 tập 소맥거핀 để đếm khung: độ dài đứng hình sau câu chốt, số frame của zoom giật, và gag có dùng on twos hay không.
- [ ] Đo tốc độ đọc tiếng Việt ở người chơi thật (giả định 12–15 ký tự/s).
- [ ] Thử bóng lớp và viền 3 px trên điện thoại 6".
- [ ] Thử mức "Giảm" với người dễ say chuyển động.
