# Impl plan — Sprint 2: Nhịp ngày mới + hình bếp

Thời gian: Chủ nhật 15/11 → Thứ bảy 21/11/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` **v1.4**. Nguồn số: `balance/next/*.json` (sinh từ `balance/make_next.py`). Hình ảnh: `docs/art-bible/` v1.3. Không có Blender.
Nguồn yêu cầu: 6 ghi chú của bạn ngày 14/11 (`.dev-memory/INBOX.md`), bài học sprint 1 (`.dev-memory/RETRO-sprint-1.md`).

## 1. Mục tiêu

Cuối ngày 21/11, bạn mở bản build (toàn màn hình) và chơi vòng 1 theo luật mới:

1. Ngày 1 có 8 vé (6 vé mỗi ngày + 2 vé thưởng). Một vé cho 1 thùng 3 đồ phụ + 1 thùng 2 nguyên liệu chính; ngày 3 là 2 + 1.
2. HUD có đồng hồ chạy từ 9:00 tới 21:00 (1 giờ = 40 giây, một ngày 8 phút).
3. Bấm biển Mở quán: khách vào liên tục, người sau vào khi người trước được phục vụ, không giới hạn số khách. Tới 21:00 không nhận khách mới; phục vụ xong khách đang ngồi thì hết ngày.
4. Sơ chế và nấu lâu hơn (cà chua 4,5 giây, trứng chiên 12 giây…).
5. Nguyên liệu nhìn ra là cà chua, hành tây, rau muống, trứng, thịt heo, đậu phụ chứ không còn là quả cầu và khối hộp.
6. Trong chảo / nồi thấy món đang được nấu: có dầu, đồ thái lát xếp gọn, chín dần đổi màu, được đảo; múc ra đĩa giống món trong chảo.
7. Đầu ngày 4 bị thu **750G**.

### Ngoài phạm vi sprint 2

| Không làm | Lý do | Sprint |
| --- | --- | --- |
| Relic, thùng đột phá | Hệ thống riêng | 3 |
| 3 khách cùng lúc, giờ chờ, tủ lạnh, quá lửa | Luật của vòng 2 | 4 |
| Hình chi tiết cho 13 nguyên liệu của vòng 2–4 và đột phá | Chỉ làm mức cơ bản; vòng 1 chỉ dùng 6 loại | 4, 5, 8 |
| Model Blender | Chưa học | 8 |
| Âm thanh | Dồn về một chỗ | 7 |

## 2. Hiện trạng (sau sprint 1)

| Có sẵn | Ghi chú |
| --- | --- |
| Core: `GameSession`, `Stove`, `PrepStation`, `Payment`, gọi món (`ChooseOrder`), `NaiveBot` | 128 test EditMode |
| Game 3D: `PlayerController`, `RoomView`, `Hud`, `FaceArt`, `KitchenFx` | 3 giả lập PlayMode, chụp ảnh vào `docs/sprint-1/ui/` |
| Bộ số 1.3 ở `balance/*.json`; bộ số 1.4 ở `balance/next/*.json` | Game và test đang đọc 1.3 |

| Phải đổi | Ở đâu |
| --- | --- |
| Số khách mỗi ngày cố định (`customers.per_day`) | `Balance`, `BalanceRules.CustomersPerDay`, `GameSession.OpenShop / Serve` |
| Ngày kết thúc khi đủ số khách | `GameSession`: kết thúc theo đồng hồ |
| Test viết cứng 400G, 5 vé, thùng 5 + 3, 6 khách | `GameSessionTests`, `IngredientGachaTests`, `PlaythroughTests`… |
| Nguyên liệu là một khối cơ bản (`IngredientArt.Shape`) | `RoomView.cs` → tách `IngredientModels.cs` |
| Đồ trong bếp là các món xếp vòng tròn, không đổi theo lúc nấu | `RoomView.OnAddedToStove`, `KitchenFx.PanItem` |

## 3. Tính năng và tiêu chí chấp nhận

Spec đầy đủ viết ở `specs/sprint-2/F-0xx-*/spec.md` vào ngày làm tính năng đó. Dưới đây là AC đã chốt được từ constitution 1.4.

### F-010 Bộ số 1.4 vào game — nguồn: constitution §2, §3, §4.2–4.5, §5.8–5.9

| AC | Given / When / Then |
| --- | --- |
| 1 | `balance/balance.json`, `recipes.json`, `relics.json` là bản 1.4 (chép từ `balance/next/`); `BalanceData.asset` nhập lại; test so asset với JSON xanh |
| 2 | Ngày 1 có 8 vé; mỗi ngày sau +6; vé thừa cộng dồn |
| 3 | 1 vé → 3 đồ phụ + 2 chính (ngày 3: 2 + 1), mỗi thùng một loại |
| 4 | Thời gian sơ chế và nấu đúng bảng §4.5, §5.9 (trứng 1,5 giây / 12 giây; đậu phụ 3 giây / 18 giây) |
| 5 | Đầu ngày 4 thu 750G; thiếu thì thua |
| 6 | Test thống kê bảng bậc vẫn trong ±1 điểm (Điều 3) |

### F-011 Ngày theo giờ, khách không giới hạn — nguồn: constitution §2, §6.1, §6.2

| AC | Given | When | Then |
| --- | --- | --- | --- |
| 1 | Đầu ngày | Thời gian trôi | Đồng hồ đi từ 9:00, 40 giây thực = 1 giờ; HUD hiện giờ dạng "14:30" |
| 2 | Quán chưa mở | Đồng hồ chạy | Không có khách; giờ vẫn trôi |
| 3 | Quán mở, trước 21:00, vòng 1 | Phục vụ xong một khách | Khách kế tiếp vào sau 3 giây và gọi món (§6.7); không có giới hạn số khách |
| 4 | 21:00, đang có khách ngồi | — | Không nhận khách mới; khách đang ngồi vẫn phục vụ được; phục vụ xong thì hết ngày |
| 5 | 21:00, không có khách ngồi | — | Hết ngày ngay |
| 6 | Quán mở | Bấm biển lần nữa | Đóng quán sớm như cũ (§6.1) |
| 7 | Hết ngày | Xem màn kết thúc ngày | Ghi số khách đã phục vụ, tiền trong ngày, giờ đóng quán |
| 8 | Bot `NaiveBot` có tính thời gian mỗi món | Chạy 2.000 ván vòng 1 | Tỉ lệ qua lần trả nợ 1 nằm trong ±5 điểm so với mô phỏng Python (96%) |

**Kịch bản 10 giây (cần bạn duyệt ngày 1):** bạn đứng ở Góc 1, góc trên trái HUD ghi "Ngày 1 · 13:20". Khách thứ 7 vừa nhận đĩa, mặt vui, tiền bay về ví. Ba giây sau khách thứ 8 bước vào, bong bóng ghi "Khách 8 · Dễ tính 71% · Gọi: Heo xào hành tây". Lúc 20:00 đồng hồ đổi màu cam. Lúc 21:00 biển tự lật sang "ĐÃ ĐÓNG", khách đang ngồi vẫn chờ món.

### F-012 Hình nguyên liệu — nguồn: ghi chú 14/11 "đỡ xấu và nhìn chân thực hơn"; Art Bible §5, §7

| AC | Then |
| --- | --- |
| 1 | 6 nguyên liệu vòng 1 mỗi loại ghép từ ≥ 3 khối, nhận ra được khi không có nhãn: cà chua (thân dẹt + cuống + 5 lá đài), hành tây (củ + chóp + rễ, vân dọc), rau muống (bó 3 cọng + lá), trứng (hình trứng, không phải cầu), thịt heo (miếng dày có viền mỡ), đậu phụ (khối bo góc) |
| 2 | Đã sơ chế thì đổi hình theo kiểu sơ chế của §4.5: cà chua thái múi, hành thái lát, trứng thành bát trứng đánh, thịt thái mỏng… (không chỉ ép dẹt) |
| 3 | 16 nguyên liệu còn lại có hình riêng mức cơ bản (≥ 2 khối, đúng màu) |
| 4 | Giữ nguyên: đế bậc đúng màu, mặt theo bậc quay về người chơi, mặt không lọt vào thân (test `AssertFaceOutside`) |
| 5 | Mỗi loại có ảnh chụp cận trong `docs/sprint-2/ui/`; bạn duyệt bằng mắt |

Bảng Art Bible → làm ở đâu: §5 hình khối tròn, viền 0,018 m → `IngredientModels`; §7 một hình dáng cho mọi bậc, đế bậc, mặt → `ItemView`; §7 "đã sơ chế: thân dẹt xuống" → sửa thành "đổi hình theo kiểu sơ chế" (Art Bible 1.4).

### F-013 Quá trình nấu nhìn ra nấu ăn — nguồn: ghi chú 14/11 "xấu và lộn xộn, không giống quá trình nấu ăn"; constitution §5.1, §5.8

| AC | Given | When | Then |
| --- | --- | --- | --- |
| 1 | Dụng cụ trên bếp | Cho Dầu ăn / Bơ / Nước dùng | Một lớp dầu / bơ tan / nước loang trên đáy dụng cụ (không còn là một giọt tròn nằm cạnh đồ ăn) |
| 2 | Có lớp nền | Cho nguyên liệu đã sơ chế | Nguyên liệu rơi vào thành các miếng đã cắt (4–6 miếng) rải đều trong lòng dụng cụ, không chồng lên nhau |
| 3 | Đủ đồ | Vặn núm | Lửa dưới dụng cụ; đồ được đảo (chảo), sôi lăn tăn (nồi), xèo (vỉ); màu đậm dần theo tiến độ nấu; hơi bốc lên |
| 4 | Món chín | — | Trong dụng cụ là món hoàn chỉnh theo cách nấu của công thức (chiên: một miếng trứng tròn có đồ phụ; xào: các miếng trộn; sốt: miếng nổi trong nước sốt) |
| 5 | Món chín | Múc ra đĩa | Đĩa có đúng hình món vừa thấy trong dụng cụ + đế món (cơm / mì / bún / bánh mì) |
| 6 | Sai thứ tự | Nấu xong | Món cháy xém, khói xám (Lạc vị) |

**Kịch bản 10 giây (cần bạn duyệt ngày 4):** bạn bấm lọ Dầu ăn, lọ nghiêng, dầu vàng loang ra đáy chảo. Bạn bấm bếp, 5 múi cà chua rơi vào rải quanh chảo. Thêm trứng: trứng lỏng vàng chảy phủ đáy. Vặn núm: lửa xanh, trứng đông dần từ viền vào, cà chua sẫm màu, hơi trắng bốc lên. 12 giây sau trong chảo là một miếng trứng chiên tròn có cà chua.

### F-014 Đo thời gian thật — nguồn: constitution §9 (giả định của mô phỏng cần đo)

| AC | Then |
| --- | --- |
| 1 | Game ghi vào `playtest-log.csv` mỗi món: lúc cầm nguyên liệu đầu, lúc vặn núm, lúc chín, lúc bấm chuông |
| 2 | Sau buổi chơi thử của bạn: tính "thời gian đi lại và thao tác mỗi món" trung bình, so với giả định 20–30 giây; lệch quá 5 giây thì sửa `OVERHEAD` trong mô phỏng và chạy lại |

## 4. Lịch 7 ngày

| Ngày | Việc | Xong khi |
| --- | --- | --- |
| 1 (CN 15/11) | F-010: chép bộ số 1.4, sửa importer (`day`, bỏ `per_day`), sửa test theo số mới. Viết spec F-011, gửi bạn kịch bản 10 giây | EditMode xanh với số 1.4 |
| 2 (T2) | F-011 Core: `DayClock`, `GameSession.Tick`, khách liên tục, hết giờ; test đỏ trước | Test F-011 AC 1–6 xanh |
| 3 (T3) | F-011 Game: đồng hồ HUD, biển tự đóng, màn kết thúc ngày; bot có thời gian; giả lập PlayMode chơi 3 ngày theo giờ | PlayMode xanh, ảnh chụp |
| 4 (T4) | F-012: 6 nguyên liệu vòng 1 (nguyên + đã sơ chế); gửi ảnh và kịch bản F-013 cho bạn | Ảnh cận 12 hình |
| 5 (T5) | F-012: 16 nguyên liệu còn lại. F-013: lớp nền, miếng cắt rải đều | AC F-013 1–2 |
| 6 (T6) | F-013: lửa, đảo, đổi màu, món hoàn chỉnh, đĩa món | AC F-013 3–6, ảnh chụp chuỗi nấu |
| 7 (T7) | F-014, build Windows, mô phỏng lại, báo cáo nghiệm thu; bạn chơi thử | Bạn "accept" hoặc nêu lỗi |

## 5. Kiểm

- EditMode: test mới cho đồng hồ và khách liên tục; sửa mọi test viết cứng số cũ. Không được xoá test để cho xanh.
- PlayMode: người chơi giả chơi 3 ngày theo giờ; kiểm khách thứ 7 trở đi vẫn vào; kiểm 21:00; chụp chuỗi ảnh nấu một món.
- Mô phỏng: `python balance/sim_balance.py` sau khi chép số phải ra đúng kết quả của `V=next` hôm nay.
- Build Windows chạy thử **không** truyền cờ màn hình (L-011).

## 6. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Hình ghép khối vẫn bị chê | Ngày 4 gửi ảnh sớm; nếu không đạt, giữ 6 loại vòng 1 ở mức tốt nhất làm được và đẩy Blender nguyên liệu lên sớm hơn |
| F-013 quá lớn | Cắt AC 4–5 xuống còn 3 kiểu (chiên, xào, sốt) đủ cho vòng 1; nướng và hấp làm ở sprint 4–5 |
| Ngày 8 phút quá dài khi chơi thử | Đổi `seconds_per_hour` trong `make_next.py`, chạy lại mô phỏng (Q-22) |

## 7. Câu hỏi cần bạn trả lời trong sprint

Q-17 (tốc độ đi, độ nhạy), Q-18 (thùng gộp theo loại), Q-19 (cỡ dải tỉ lệ), Q-20 (chủ nợ gọi món), Q-21 (đồng hồ chạy từ đầu ngày hay từ lúc mở quán), Q-22 (6 vé mỗi ngày, ngày dài 8 phút), Q-23 (giữ nguyên bảng tỉ lệ bậc). Xem `.dev-memory/QUESTIONS.md`.
