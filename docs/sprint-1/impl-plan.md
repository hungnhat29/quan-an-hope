# Impl plan — Sprint 1: Lát cắt dọc vòng 1

Thời gian: Chủ nhật 15/11 → Thứ bảy 21/11/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` v1.2. Nguồn số: `balance/balance.json`, `balance/recipes.json`. Hình ảnh: `docs/art-bible/` v1.1. Không có Blender trong sprint này.

Cập nhật 14/11: theo constitution v1.1 (nguyên liệu, công thức, thời gian nấu, quầy chờ) và câu trả lời của bạn (nút Mở quán). **Cả 7 ngày đã xong ngày 14/11.** Bạn nghiệm thu chức năng và yêu cầu chuyển sang 3D góc nhìn thứ nhất; phần làm lại là [F-007](../../specs/sprint-1/F-007-choi-3d/spec.md). Chờ bạn chơi thử bản 3D — xem `.dev-memory/acceptance/2026-11-14-sprint-1-3d.md`.

## 1. Mục tiêu

Cuối ngày 21/11, bạn mở Unity, bấm Play và chơi được trọn vòng 1:

1. Ngày 1 bắt đầu với 100G và 5 vé trắng.
2. Sang Góc 2, nhét vé, nhận thùng đồ phụ và thùng nguyên liệu chính, thấy bậc từng món và bảng tỉ lệ của vòng.
3. Sang Góc 4, sơ chế trên thớt, chọn chảo hoặc nồi, cho nguyên liệu và đồ kho vào đúng thứ tự công thức, thấy thanh tỉ lệ bậc món, múc ra đĩa; quầy chờ giữ được 3 đĩa.
4. Sang Góc 1, bấm **Mở quán** thì khách mới bắt đầu đến, lần lượt từng người; thấy độ dễ tính (%), bấm chuông phục vụ, nhận tiền và tip (hoặc bị trừ tiền).
5. Hết khách thì kết thúc ngày; chơi tiếp ngày 2 và ngày 3 (ngày 3 nguyên liệu ít và tệ hơn).
6. Đầu ngày 4 bị thu 400G: đủ tiền thì thấy màn "Qua vòng 1", thiếu thì thấy màn thua.

### Ngoài phạm vi sprint 1

| Không làm | Lý do | Sprint |
| --- | --- | --- |
| Relic và cửa hàng Góc 3, thùng đột phá | Hệ thống lớn nhất; vòng 1 qua được mà không cần (mô phỏng: 100% qua lần trả nợ 1) | 2 |
| Thời gian chờ của khách | Vòng 1 không tính giờ (§6.6) | 3 |
| Tủ lạnh, nước sốt, máy gacha đồ ăn, vé đen | Mở khóa từ ngày 4 (§8); tủ lạnh dời cùng nhóm | 3 |
| Model và animation Blender | Chưa cài, chưa học | 7–8 |
| Menu, cốt truyện, lưu game, âm thanh | Không cần để chơi thử lát cắt | 5–6 |

Hệ quả cần biết: trong sprint 1, ngày 2–3 chưa có cửa hàng relic dù constitution mở từ ngày 2, và đĩa còn trên quầy chờ bị bỏ cuối ngày vì chưa có tủ lạnh. Vòng 1 chỉ có 3 đồ phụ × 3 nguyên liệu chính (9 món), không có quá lửa (§5.9).

## 2. Hiện trạng

| Có sẵn | Ghi chú |
| --- | --- |
| Unity 6000.6.9f1, URP 17.6, Input System, uGUI 2.6 (đã gồm TextMeshPro) | `My project/Packages/manifest.json` |
| Cảnh `NhaHang.unity` 4 góc dựng bằng khối cơ bản, camera xoay 90° (`CameraRig`), đổi mặt (`FaceSwap`) | Sinh từ `Editor/BuildNhaHang.cs` |
| `balance/balance.json`, `relics.json`, `sim_balance.py` | Đã giải nén từ file zip; mô phỏng chạy lại ra đúng số của constitution |

| Còn thiếu | Xử lý |
| --- | --- |
| Git và Git LFS | Ngày 1 |
| Gói `com.unity.test-framework`, gói đọc JSON | Ngày 1 |
| Mọi logic game | Ngày 1–4 |
| Chữ UI đang là ảnh PNG (trái Điều 6) | Chữ mới trong sprint dùng TextMeshPro; ảnh cũ thay dần |
| Cảnh có nội dung trái constitution: bảng "tỉ lệ nâng phẩm" công khai (Điều 4 yêu cầu ẩn), "3 vé/thùng" (§4.2: 1 vé), công thức 3 nguyên liệu (§5.1: 1 chính + 1 phụ) | Sprint 1 sửa phần Góc 2 và Góc 4 có dùng tới; bảng tỉ lệ máy gacha đồ ăn ẩn đi |

## 3. Kiến trúc

```
My project/Assets/QuanAnHope/
  Core/      QuanAnHope.Core.asmdef   C# thuần, không tham chiếu UnityEngine
  Data/      QuanAnHope.Data.asmdef   ScriptableObject BalanceData + asset sinh ra
  Game/      QuanAnHope.Game.asmdef   MonoBehaviour: nối Core với cảnh và UI
  Editor/    nhập balance.json → BalanceData.asset
  Tests/EditMode/                     test của Core và Data
specs/sprint-1/F-00x-*/spec.md, plan.md, tasks.md
```

Quyết định:

- **Core không biết Unity.** Mọi luật (quay bậc, công thức món, tiền, vòng ngày) nằm trong Core, nhận ngẫu nhiên qua `IRandom` để test cố định được seed. Test chạy vài giây, không cần mở cảnh.
- **Một nguồn số.** `balance/balance.json` → công cụ Editor → `BalanceData.asset`. Core nhận một đối tượng `Balance` thuần; không có con số nào trong code (Điều 2). Có một test so asset với JSON để hai bên không lệch.
- **Game chỉ hiển thị và nhận thao tác.** MonoBehaviour gọi Core rồi vẽ kết quả. Sau này thay khối cơ bản bằng model Blender không đụng tới Core.
- **Giữ `BepNhanPham/`** làm lớp cảnh; code mới vào `QuanAnHope/`.

Lệnh test (cần đóng Unity Editor khi chạy):
```
"D:\Unity\6000.6.9f1\Editor\Unity.exe" -batchmode -projectPath "My project" -runTests -testPlatform EditMode -testResults TestResults.xml -logFile test.log
```
Hoặc trong Editor: Window → General → Test Runner → EditMode → Run All.

## 4. Tính năng và tiêu chí chấp nhận

Mỗi tính năng có một `spec.md` viết đầy đủ Given/When/Then vào đúng ngày làm. Bảng dưới là danh sách AC và nguồn; spec không được thêm số mới (Điều 8).

### F-001 Dữ liệu cân bằng
| AC | Nội dung | Nguồn |
| --- | --- | --- |
| 1 | Nhập `balance.json` và `recipes.json` sinh ra `BalanceData.asset` với mọi trường sprint 1 dùng (gồm nguyên liệu, công thức, thời gian nấu) | Điều 2, §4.5, §5.8 |
| 2 | Giá trị trong asset bằng giá trị trong JSON | constitution dòng 7 |
| 3 | Mỗi bảng tỉ lệ bậc có tổng 100 | §4.3 |

### F-002 Vé và gacha nguyên liệu
| AC | Nội dung | Nguồn |
| --- | --- | --- |
| 1 | Ngày 1 có 5 vé (3 + 2 thưởng); mỗi ngày sau +3; vé không dùng cộng dồn | §3 |
| 2 | Một lượt tốn 1 vé, cho 5 đồ phụ + 3 nguyên liệu chính; hết vé thì không quay được | §4.2 |
| 3 | Ngày 3 cho 3 đồ phụ + 2 chính | §4.3 |
| 4 | Mỗi món quay bậc độc lập theo bảng: ngày 1–2 dùng bảng vòng 1, ngày 3 dùng bảng sau mưa | §4.3 |
| 5 | Quay 100.000 lần, mỗi bậc lệch không quá ±1 điểm phần trăm so với bảng (cả hai bảng) | Điều 3 |
| 6 | Nguyên liệu chưa dùng bị bỏ cuối ngày | §4.2 |
| 7 | Bảng tỉ lệ của vòng hiện tại hiện cho người chơi | Điều 4 |
| 8 | Mỗi thùng chỉ có một loại nguyên liệu, chọn ngẫu nhiên đều trong các loại đã mở khóa ở vòng hiện tại (vòng 1: cà chua, hành tây, rau muống; trứng gà, thịt heo, đậu phụ); có test thống kê | §4.2, §4.5, Điều 3 |

### F-003 Nấu ăn
| AC | Nội dung | Nguồn |
| --- | --- | --- |
| 1 | `b = 0,6 × bậc chính + 0,4 × bậc phụ + 0,8`; món ra bậc `floor(b)` hoặc `floor(b)+1`, xác suất lên bằng phần lẻ; kẹp trong [Lạc vị, Tuyệt hảo] | §5.3 |
| 2 | Năm ví dụ trong bảng §5.3 cho đúng thanh tỉ lệ (1+1 → 20% Dở, 80% Bình thường; …; 4+2 → 100% Tuyệt hảo) | §5.3 |
| 3 | Test thống kê 100.000 lần cho một cặp nguyên liệu, lệch không quá ±1 điểm | Điều 3 |
| 4 | Thứ tự cho vào lấy từ công thức, gồm cả đồ kho; cho sai thứ tự thì món ra Lạc vị | §5.1, §5.8 |
| 5 | Thanh tỉ lệ bậc món hiện trên nồi khi đang nấu | Điều 4, §5.3 |
| 6 | Trình tự thao tác: thớt (giữ đủ thời gian sơ chế của nguyên liệu) → chọn dụng cụ (chảo/nồi) → cho từng thứ đúng thứ tự → chờ đủ thời gian nấu của món → đĩa (múc) | §5.1, §4.5, §5.8 |
| 7 | Quầy chờ giữ tối đa 3 đĩa đã nấu, không giảm bậc; đầy thì không múc thêm được | §5.1 |
| 8 | Tên, dụng cụ, đế món và thời gian nấu của món tra được từ cặp nguyên liệu chính + đồ phụ (9 món vòng 1) | §5.8 |

### F-004 Khách và trả tiền
| AC | Nội dung | Nguồn |
| --- | --- | --- |
| 1 | Vòng 1: 6 khách mỗi ngày | §6.2 |
| 2 | Độ dễ tính vòng 1: 90% khách trong 60–95, 10% trong 35–59; có test thống kê | §6.3, Điều 3 |
| 3 | Món tối thiểu: ≥80 → Dở, 60–79 → Bình thường, 35–59 → Ngon, <35 → Tuyệt hảo | §6.4 |
| 4 | `chênh ≥ 0`: trả `giá` + tip `giá × khó × (0,3 + 0,2 × chênh)` | §6.5 |
| 5 | `chênh = −1`: trả 50% giá, không tip | §6.5 |
| 6 | `chênh ≤ −2`: không trả, quán bị trừ 30% giá gốc | §6.5 |
| 7 | Giá = giá gốc vòng 1 (60G) × hệ số bậc món | §5.2 |
| 8 | Độ dễ tính hiện bằng % trên khách; bấm chuông để phục vụ | §6.3, §5.1 |
| 9 | Góc 1 có nút **Mở quán**; chưa bấm thì chưa có khách; sau khi bấm khách đến lần lượt, người sau vào khi người trước đã được phục vụ | Bạn trả lời ngày 14/11 |

### F-005 Vòng ngày và trả nợ
| AC | Nội dung | Nguồn |
| --- | --- | --- |
| 1 | Bắt đầu với 100G | §3 |
| 2 | Ngày kết thúc khi khách cuối đã được phục vụ; sang ngày mới thì cộng vé, bỏ nguyên liệu thừa và đĩa còn trên quầy chờ | §2, §4.2, §5.1 |
| 3 | Đầu ngày 4 thu 400G; không đủ thì thua | §2 |
| 4 | Đủ tiền thì hiện màn "Qua vòng 1" (điểm dừng của lát cắt) | phạm vi sprint |
| 5 | Bot "không đọc chỉ số" (chọn món giá to, phục vụ theo thứ tự) chạy 2.000 ván trên Core, tỉ lệ qua lần trả nợ 1 khớp mô phỏng (100%) | Điều 1 |

### F-006 Giao diện lát cắt
| AC | Nội dung | Nguồn |
| --- | --- | --- |
| 1 | HUD hiện ngày, tiền, nợ sắp tới, số vé, bằng TextMeshPro có dấu tiếng Việt | Điều 6 |
| 2 | Chuyển góc bằng nút trái/phải có sẵn; mỗi góc chỉ hiện UI của góc đó | Điều 5 |
| 3 | Bảng tỉ lệ máy gacha đồ ăn ở Góc 2 bị ẩn | Điều 4 |
| 4 | Chơi được từ ngày 1 tới màn kết quả mà không cần mở Inspector | mục tiêu sprint |

AC 7 của F-002, AC 5–6 của F-003, AC 8 của F-004 và toàn bộ F-006 kiểm bằng tay theo danh sách chơi thử ở mục 7.

## 5. Lịch 7 ngày

Mỗi ngày theo cùng nhịp: **spec → test đỏ → code → test xanh → kiểm bằng mắt**. Ước lượng mỗi ngày 3–4 giờ; nếu bạn có ít hơn, xem mục 8.

### Ngày 1 — Nền dự án và dữ liệu (F-001) — ĐÃ XONG ngày 14/11
- **Thiết lập:** `git init` ở thư mục gốc, `.gitignore` cho Unity (bỏ `Library/`, `Logs/`, `UserSettings/`, `Temp/`), Git LFS cho `.fbx .png .blend .wav`, commit đầu.
- **Gói:** thêm `com.unity.test-framework` và `com.unity.nuget.newtonsoft-json` qua Package Manager.
- **Khung:** tạo thư mục, asmdef Core / Data / Editor + asmdef test. (Asmdef Game dời sang ngày 5, khi có script đầu tiên.)
- **Spec:** `specs/sprint-1/F-001-du-lieu/spec.md`.
- **Test đỏ → code:** lớp `Balance` (Core), `BalanceData` (ScriptableObject), menu `Quán Ăn Hope > Nhập balance.json`.
- **Xong khi:** 3 AC của F-001 xanh; chạy được test bằng dòng lệnh.
- **Kết quả:** 12/12 test xanh; repo `github.com/hungnhat29/quan-an-hope` (riêng tư); cảnh đã chỉnh theo Art Bible (hậu kỳ, camera, viền).

### Ngày 2 — Vé và gacha nguyên liệu (F-002, phần logic) — ĐÃ XONG ngày 14/11
- **Spec:** F-002.
- **Test đỏ → code:** `IRandom` + bản seed cố định; `TierTable.Roll`; `TicketWallet`; `IngredientGacha.Pull(day)` trả về hai thùng, mỗi thùng một loại nguyên liệu đã mở khóa, từng món có bậc riêng; kho nguyên liệu trong ngày.
- **Xong khi:** AC 1–6 xanh, gồm hai test thống kê 100.000 lần.

### Ngày 3 — Nấu ăn và khách (F-003, F-004, phần logic) — ĐÃ XONG ngày 14/11
- **Spec:** F-003, F-004.
- **Test đỏ → code:** `DishFormula` (b, thanh tỉ lệ, quay bậc), `RecipeLookup` (cặp chính + phụ → công thức), `Stove` (dụng cụ, thứ tự gồm đồ kho, sai → Lạc vị), `Counter` (quầy chờ 3 đĩa), `CustomerGenerator` (dải độ dễ tính), `RequiredTier`, `Payment`.
- **Xong khi:** F-003 AC 1–4, 7, 8 và F-004 AC 1–7 xanh.

### Ngày 4 — Vòng ngày, trả nợ, bot (F-005) — ĐÃ XONG ngày 14/11
- **Spec:** F-005.
- **Test đỏ → code:** `GameSession` (trạng thái: chuẩn bị → mở quán → phục vụ → kết thúc ngày → thu nợ → kết quả), `NaiveBot` chơi trên Core.
- **Kiểm chéo:** chạy `sim_balance.py` (đặt `PYTHONUTF8=1`) và so tỉ lệ qua ngày 4 với bot C#.
- **Xong khi:** F-005 AC 1–5 và F-004 AC 9 xanh. Từ đây toàn bộ luật vòng 1 đã chạy được không cần hình.

### Ngày 5 — Thiết kế UI, HUD và Góc 2 (F-006, F-002 AC 7) — ĐÃ XONG ngày 14/11
- **Thiết kế (1 giờ đầu):** phác 3 màn Góc 2, Góc 4, Góc 1 trên giấy hoặc ảnh chụp cảnh hiện có: vị trí nút, chỗ hiện bậc nguyên liệu (màu theo bậc), bảng tỉ lệ vòng, thanh tỉ lệ món. Lưu vào `docs/sprint-1/ui/`.
- **Code:** tải Baloo 2 bản `.ttf` (Art Bible §13; gói hiện chỉ có `.woff2`) và tạo font TextMeshPro; bảng màu bậc từ `docs/art-bible/palette/palette.json`; tạo asmdef Game; `GameController` giữ `GameSession`; HUD; nút "Nhét vé" ở Góc 2, thùng hiện món kèm bậc, bảng tỉ lệ của vòng; ẩn bảng tỉ lệ máy gacha đồ ăn.
- **Xong khi:** bấm Play, nhét vé, thấy nguyên liệu và số vé giảm.

### Ngày 6 — Góc 4 và Góc 1 (F-003 AC 5–6, F-004 AC 8) — ĐÃ XONG ngày 14/11
- **Góc 4:** chọn nguyên liệu, giữ chuột để sơ chế (thanh tiến trình), chọn chảo hoặc nồi, bấm để cho từng thứ vào theo thứ tự (thanh thứ tự nấu), thanh tỉ lệ bậc món, chờ nấu, lấy đĩa và múc ra quầy chờ.
- **Góc 1:** nút Mở quán; khách xuất hiện với % dễ tính, chọn món đã nấu, bấm chuông, hiện tiền và tip, đổi mặt vui/giận bằng `FaceSwap`.
- **Xong khi:** hoàn thành một vòng gacha → nấu → phục vụ → nhận tiền trong lúc Play.

### Ngày 7 — Khép vòng, chơi thử, nghiệm thu — ĐÃ XONG ngày 14/11 (phần chơi thử bằng tay chờ bạn)
- **Code:** màn kết thúc ngày (thu trong ngày, tiền hiện có, nợ sắp tới), chuyển ngày, màn thu nợ đầu ngày 4, màn "Qua vòng 1" và màn thua, nút chơi lại.
- **Chơi thử:** chạy danh sách ở mục 7 ít nhất 3 lượt trọn vẹn; ghi lỗi, sửa lỗi chặn.
- **Đóng sprint:** chạy toàn bộ test, build Windows thử, báo cáo nghiệm thu (ma trận AC → test → kết quả), ghi việc dời sang sprint 2.

## 6. Ma trận AC → test

| Tính năng | File test dự kiến | AC tự động | AC kiểm tay |
| --- | --- | --- | --- |
| F-001 | `BalanceDataTests.cs` | 1, 2, 3 | — |
| F-002 | `TicketWalletTests.cs`, `IngredientGachaTests.cs`, `TierTableStatTests.cs` | 1–6, 8 | 7 |
| F-003 | `DishFormulaTests.cs`, `StoveTests.cs`, `KitchenTests.cs` | 1–4, 7, 8 | 5, 6 |
| F-004 | `CustomerTests.cs`, `PaymentTests.cs`, `GameSessionTests.cs` | 1–7, 9 | 8 |
| F-005 | `GameSessionTests.cs`, `NaiveBotTests.cs` | 1–5 | — |
| F-006 → F-007 | `Tests/PlayMode/PlaythroughTests.cs` (giả lập 3D) | 1–12 (giả lập) | cảm giác điều khiển |

Tổng: 37 AC, trong đó 29 tự động và 8 kiểm tay. Sau ngày 7: 29/29 AC tự động xanh; 8 AC kiểm tay đã qua giả lập PlayMode (bấm nút trên cảnh thật) và chờ bạn chơi thử xác nhận.

## 7. Danh sách chơi thử (ngày 7)

1. Play: HUD ghi Ngày 1, 100G, nợ 400G, 5 vé.
2. Góc 2: nhét 1 vé → một thùng 5 đồ phụ cùng loại + một thùng 3 nguyên liệu chính cùng loại, vé còn 4; bảng tỉ lệ ghi 25/57/15/3/0/0.
3. Góc 4: nấu chính bậc 1 + phụ bậc 1 đúng thứ tự công thức → thanh ghi 20% Dở, 80% Bình thường; tên món đúng (ví dụ Trứng chiên cà chua).
4. Góc 4: cố ý cho sai thứ tự → Lạc vị.
5. Góc 1: chưa bấm Mở quán thì không có khách; bấm rồi khách đầu tiên vào. Phục vụ khách ≥80% món Dở → được trả đủ và có tip.
6. Góc 1: phục vụ khách 35–59% món Dở → không trả, tiền bị trừ 18G.
7. Phục vụ đủ 6 khách → màn kết thúc ngày; sang ngày 2 có thêm 3 vé cộng với vé thừa; nguyên liệu thừa biến mất.
8. Ngày 3: mỗi lượt chỉ 3 đồ phụ + 2 chính; bảng tỉ lệ ghi 50/42/8/0/0/0.
9. Đầu ngày 4 với ≥400G → bị trừ 400G, hiện "Qua vòng 1".
10. Chơi lại, cố ý phục vụ món tệ để còn <400G → màn thua.
11. Xoay đủ 4 góc: không có chữ lỗi dấu, không có UI của góc khác lọt vào.

## 8. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Ngày 5–6 (nối UI) dài hơn dự kiến | Bỏ thao tác giữ chuột sơ chế, thay bằng một lần bấm; bỏ đổi mặt khách. Logic không bị ảnh hưởng |
| Lần đầu dùng asmdef và Test Runner | Ngày 1 có test rỗng để thông đường ống trước khi viết gì khác |
| Cảnh sinh từ `BuildNhaHang.cs` bị ghi đè khi chạy lại menu | UI mới đặt trong prefab riêng `QuanAnHope/Prefabs/GameUI.prefab`, không sửa trực tiếp trong cảnh sinh ra |
| Mỗi ngày chỉ có 1–2 giờ | Giữ ngày 1–4 (toàn bộ logic + test), dồn ngày 5–7 sang đầu sprint 2; mục tiêu "chơi thử được" trễ khoảng 3–4 ngày |
| `sim_balance.py` lỗi mã hoá trên Windows | Chạy với `PYTHONUTF8=1` |

## 9. Câu hỏi

Đã trả lời ngày 14/11:

| # | Câu hỏi | Trả lời |
| --- | --- | --- |
| Q-2 | Mỗi ngày có bao nhiêu giờ? | 3–4 giờ hoặc hơn |
| Q-3 | Các điểm tự diễn giải ở constitution §9 | Đã cập nhật trong constitution v1.1 |
| Q-4 | Tên nguyên liệu và món vòng 1 | Constitution §4.5, §5.8 và `balance/recipes.json` |
| Q-5 | Nhịp khách đến | Vòng 1 khách đến lần lượt; Góc 1 có nút Mở quán, bấm thì mới bắt đầu có khách |
| Q-7 | Thứ tự công thức | Theo cột "Thứ tự cho vào" ở constitution §5.8 |

Dùng mặc định (bạn không trả lời):

| # | Câu hỏi | Mặc định |
| --- | --- | --- |
| Q-5b | Đồng hồ 9:00–21:00 có chạy ở vòng 1 không? | Chưa chạy trong sprint 1 |
| Q-6 | Có được nấu trước nhiều món rồi mới phục vụ không? | Được, tối đa 3 đĩa trên quầy chờ (constitution §5.1) |

Mới phát sinh:

| # | Câu hỏi | Mặc định |
| --- | --- | --- |
| Q-9 | Art Bible ghi "thanh 10 ô relic", constitution §7.1 ghi tối đa 8. Theo bên nào? | Constitution (8); sửa Góc 3 ở sprint 2 |
| Q-10 | Nút Mở quán chưa có trong constitution. Có thêm vào §6 (bản 1.2) không? | Có, khi bạn đồng ý; hiện ghi trong spec F-004 |
| Q-11 | Chọn sai dụng cụ (ví dụ nấu trứng chiên bằng nồi) thì sao? Constitution chỉ nói sai thứ tự → Lạc vị | Coi như sai công thức → Lạc vị |
| Q-12 | Tiền làm tròn thế nào? Constitution không nói; mô phỏng giữ số lẻ | Giá và tip làm tròn riêng tới 1G gần nhất |
| Q-14 | Hết nguyên liệu và vé khi còn khách thì sao? Constitution không nói | Có nút Đóng quán sớm: khách còn lại ra về, không trả, không phạt |
| Q-15 | Tiền đền bù có làm ví âm được không? | Được |
| Q-16 | Constitution §5.1 tả hai bước lấy đĩa rồi múc. Có cần tách không? | Gộp thành một nút Múc ra đĩa |
| Q-13 | `balance.json` được thêm `storm_day`, `counter_slots`, `dish.max_tier`, `payment` (số lấy từ constitution §4.3, §5.1, §5.3, §6.5). Có cần ghi vào lịch sử constitution không? | Không; không có con số mới, chỉ chuyển số từ mô phỏng sang dữ liệu |
