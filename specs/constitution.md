# Constitution — Quán ăn của Hope (tên tạm)

Phiên bản 1.3 · 14/11/2026 · Trạng thái: khách gọi một món cụ thể, sách ghi giá bán; game 3D góc nhìn thứ nhất; số cân bằng giữ nguyên từ 1.0

Đây là nguồn sự thật cao nhất của dự án. Mọi spec, plan, task và dòng code phải tuân theo file này. Khi một spec mâu thuẫn với constitution, constitution thắng; muốn đổi thì sửa constitution trước (tăng số phiên bản, ghi lý do ở mục 9).

Các con số cân bằng nằm ở hai nơi phải luôn khớp nhau: bảng trong file này và `balance/balance.json` + `balance/relics.json` + `balance/recipes.json`. Code chỉ đọc số từ dữ liệu, không viết số trong code.

---

## 1. Nguyên tắc bất biến

### Điều 1 — Độ khó mục tiêu
Game nằm trong khoảng **trung bình-dễ đến trung bình-khó**. Ba mốc dưới đây là cổng chấp nhận; mọi thay đổi số liệu phải chạy lại mô phỏng và vẫn đạt cả ba:

| Kiểu người chơi | Cách chơi | Mục tiêu | Kết quả mô phỏng v1.0 | Mô phỏng v1.3 (khách gọi món) |
| --- | --- | --- | --- | --- |
| Không đọc chỉ số | Chọn món có giá to, tỉ lệ to; phục vụ theo thứ tự khách đến | Đi qua **≥ 60%** chặng đường | Đi được trung bình 66% (qua lần trả nợ 1: 100%, lần 2: 86%) | 67% (100%, 87%) |
| Chơi tốt, có **≥ 3 món cùng một bộ relic** | Gom bộ, giữ món ngon cho khách khó tính và boss | Thắng **≥ 70%** (phần còn lại do may) | Bộ Chợ Sớm 82%, Sơn Hào 84%, Vườn Nhà 71%; đủ 4 món: 87–97% | 82% (gộp mọi bộ; chưa tách theo bộ) |
| Chơi tốt nhưng không gom bộ | Như trên, chỉ mua relic lẻ | Thấp hơn rõ so với người gom bộ | 64% | 56% |

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
- **Di chuyển:** tự đi bằng bàn phím (W A S D hoặc phím mũi tên), nhìn bằng chuột. **Không có nút chuyển góc.**
- **Tương tác:** nhìn vào đồ vật (tâm ngắm giữa màn hình) và bấm chuột trái; việc cần giữ (sơ chế) thì giữ chuột trái. Chuột phải: đặt lại thứ đang cầm.
- **Cầm đồ:** mỗi lúc cầm tối đa **một** thứ: một nguyên liệu, một đĩa trống hoặc một đĩa món.
- Không có màn hình chơi nào nằm ngoài căn phòng (trừ menu, kết thúc ngày, cốt truyện).

### Điều 6 — Công nghệ
Unity 6 LTS, URP, C#. Unity Test Framework cho test. Git + Git LFS (`.fbx`, `.png`, `.blend`, `.wav`). Model và animation nhân vật làm bằng Blender, xuất `.fbx`. Chữ trong UI dùng TextMeshPro, không dùng ảnh chữ.

### Điều 7 — Phong cách hình ảnh
Màu ấm kiểu hoạt hình cổ (Cuphead) cho nền; nhân vật và đồ vật đơn giản, tròn, viền đậm, màu phẳng. Mắt là **vòng tròn rỗng viền đậm, hở một đoạn phía trên**, miệng chữ "w" nhỏ. Mặt là ảnh 2D dán lên đầu 3D, đổi biểu cảm bằng đổi ảnh. Bốn tường dùng chung giấy dán tường, sàn gỗ, nẹp gỗ.

### Điều 8 — Quy trình SDD
Mỗi tính năng: `spec.md` (luật + tiêu chí chấp nhận Given/When/Then) → `plan.md` → `tasks.md` → code + test → chơi thử. Sai thì sửa spec trước, code sau. Spec không được đặt con số mới; con số mới phải vào constitution/dữ liệu trước.

### Điều 9 — Thông tin nằm trên đồ vật
Thông tin của một thứ hiện **trên chính thứ đó trong phòng**, không hiện thành bảng phẳng trên màn hình:
- Nguyên liệu nằm trong thùng; **bậc đọc bằng màu** (đế bậc dưới mỗi món, Art Bible §7), không cần chữ.
- Tỉ lệ gacha: bảng treo trên tường Góc 2. Khoảng bậc món, thứ tự đã cho vào, tiến độ nấu: bảng nhỏ ngay trên bếp. Tiến độ sơ chế: thanh ngay trên thớt.
- Công thức và giá bán theo bậc: trang của quyển sách ở Góc 3. Độ dễ tính và món khách gọi: bong bóng trên đầu khách. Đĩa món: nằm trên quầy chờ, viền đĩa theo màu bậc.
- **HUD chỉ hiện con số và thông tin ngắn:** ngày, tiền, nợ, vé, tâm ngắm, gợi ý tương tác của đồ đang nhìn, thứ đang cầm, thông báo một dòng. HUD không có nút bấm.
- Màn kết thúc ngày, qua vòng, thua là màn ngoài phòng (Điều 5) nên được dùng nút.

---

## 2. Cốt truyện và bối cảnh

- **Nhân vật chính:** Hope. Cùng một ngày, Hope bị đuổi việc và về nhà gặp chủ nợ. Hope van xin; chủ nợ giao cho Hope toàn quyền một quán ăn sắp đóng cửa của ông để bán và phát triển, và sẽ quay lại đòi nợ theo lịch.
- **Không gian:** một quán ăn trong một căn phòng 4 tường; người chơi chỉ tương tác trong phòng.
- **Giờ mở cửa:** 9:00 – 21:00 (trong game). Sau 21:00, khách còn ngồi vẫn được phục vụ đến người cuối cùng. Độ dài đề xuất: 1 giờ trong game = 30 giây thực, một ngày khoảng 6 phút.
- **Kết thúc:** trả hết nợ ngày 22; chủ nợ gả con gái cho Hope.

### Lịch 22 ngày

| Vòng | Ngày | Sự kiện / "boss" | Trả nợ |
| --- | --- | --- | --- |
| 1 | 1–3 | Ngày 2 mưa lớn → ngày 3 nguyên liệu ít hơn và tệ hơn | Đầu ngày 4: **400G** |
| 2 | 4–8 | Ngày 8 sét đánh cột điện → mất điện, nấu bằng nến, tầm nhìn giảm | Đầu ngày 9: **2.200G** |
| 3 | 9–14 | Khách ít và khó tính hơn hẳn; ngày 14 mới biết có kẻ tung tin xấu về quán; ngày 14 là ngày "all-in" | Đầu ngày 15: **8.200G** |
| 4 | 15–21 | Uy tín phục hồi, mọi thứ bình thường. Ngày 21, khách cuối cùng là **chủ nợ** (độ dễ tính 5%) | Đầu ngày 22: **26.200G** |

Nợ thu vào **đầu** ngày đến hạn. Không đủ tiền = thua (kết thúc game).

---

## 3. Tiền tệ

| Loại | Dùng để | Nguồn |
| --- | --- | --- |
| Tiền (G) | Mua vật phẩm ở cửa hàng, trả nợ | Bán món ăn, tip, thưởng boss |
| Vé trắng | Gacha nguyên liệu, mua relic | **3 vé mỗi ngày**, không dùng thì cộng dồn |
| Vé đen | Gacha nguyên liệu cao cấp, hoặc mua relic tốt chắc chắn | Mua ở cửa hàng |

Khởi đầu: **100G** và **2 vé thưởng** (ngày 1 có 5 vé). Cửa hàng relic mở từ **ngày 2** (ngày 1 là ngày làm quen).

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
Bấm máy nhét vé ở Góc 2, máy thả xuống **1 thùng đồ phụ (5 món) + 1 thùng nguyên liệu chính (3 món)** lên bàn nhận hàng; nếu trúng thì thêm **1 thùng đột phá (2 món)**. **Mỗi thùng chỉ có một loại nguyên liệu** (ví dụ thùng 5 quả cà chua, thùng 3 miếng thịt bò); loại được chọn ngẫu nhiên đều trong các loại đã mở khóa ở vòng hiện tại (mục 4.5). Mỗi món tự quay bậc độc lập theo bảng của vòng hiện tại. Nguyên liệu không dùng hết bị bỏ cuối ngày (tủ lạnh chỉ giữ món đã nấu). Trên bàn, nguyên liệu cùng loại nằm chung một thùng (ví dụ mọi quả cà chua của ngày nằm trong thùng cà chua); người chơi lấy từng món ra bằng tay.

### 4.3 Tỉ lệ bậc theo vòng (%)

| Bảng | Bậc 0 | Bậc 1 | Bậc 2 | Bậc 3 | Bậc 4 | Bậc 5 |
| --- | --- | --- | --- | --- | --- | --- |
| Vòng 1 (ngày 1–2) | 25 | 57 | 15 | 3 | 0 | 0 |
| Ngày 3 (sau mưa) | 50 | 42 | 8 | 0 | 0 | 0 |
| Vòng 2 (ngày 4–8) | 15 | 40 | 33 | 10 | 2 | 0 |
| Vòng 3 (ngày 9–14) | 10 | 28 | 35 | 20 | 6 | 1 |
| Vòng 4 (ngày 15–21) | 6 | 18 | 33 | 28 | 12 | 3 |
| Vé đen (mọi vòng) | 0 | 0 | 0 | 62 | 30 | 8 |

Ngày 3 (sau mưa) thùng cũng nhỏ lại: **3 đồ phụ + 2 nguyên liệu chính**.

**Cách dựng bảng (theo gợi ý TFT):** mỗi vòng giống một cấp trong TFT — khối xác suất trượt dần lên bậc cao, bậc hiếm nhất chỉ xuất hiện muộn. Vòng 2 có 2% bậc 4 giống TFT cấp 5 có 2% tướng 4 vàng; vòng 3 có 1% bậc 5 giống TFT cấp 7 có 1% tướng 5 vàng; vòng 4 có 3% bậc 5 giống TFT cấp 8 có 3% tướng 5 vàng (số liệu TFT ở mục 10).

**Vé đen:** bạn đề xuất 60/30/10; mình chỉnh thành **62/30/8** vì một vé đen cho 8 món, với 10% bậc 5 thì 57% số vé đen có ít nhất một món hiếm — quá dễ. Với 8% là khoảng 49%.

### 4.4 Thùng đột phá
- Tỉ lệ xuất hiện mỗi lượt: **0% + tổng từ relic** (xem mục 7).
- Mỗi thùng: **2 món**, **70% bậc 4 / 30% bậc 5** (relic có thể nâng tỉ lệ bậc 5).

### 4.5 Danh sách nguyên liệu

Nguyên liệu mở dần theo vòng để người chơi học từng ít một: vòng 1 có 3 loại đồ phụ × 3 loại chính (9 món ăn), đến vòng 4 có 10 × 8 (80 món ăn).

**Đồ phụ** (rau, củ, quả)

| Nguyên liệu | Sơ chế (thớt) | Thời gian giữ | Mở khóa |
| --- | --- | --- | --- |
| Cà chua | Thái múi | 1,5 giây | Vòng 1 |
| Hành tây | Thái lát | 1,5 giây | Vòng 1 |
| Rau muống | Nhặt, rửa | 1,0 giây | Vòng 1 |
| Khoai tây | Gọt, thái | 2,0 giây | Vòng 2 |
| Nấm rơm | Rửa, bổ đôi | 1,0 giây | Vòng 2 |
| Bông cải xanh | Tách bông | 1,5 giây | Vòng 3 |
| Ớt chuông | Bỏ hạt, thái | 1,5 giây | Vòng 3 |
| Bí đỏ | Gọt, thái | 2,0 giây | Vòng 4 |
| Măng tây | Cắt khúc | 1,0 giây | Vòng 4 |
| Nấm đông cô | Thái lát | 1,0 giây | Vòng 4 |

**Nguyên liệu chính** (thịt, cá, tôm, trứng, đậu)

| Nguyên liệu | Sơ chế (thớt) | Thời gian giữ | Mở khóa |
| --- | --- | --- | --- |
| Trứng gà | Đập, đánh | 0,5 giây | Vòng 1 |
| Thịt heo | Thái mỏng | 1,5 giây | Vòng 1 |
| Đậu phụ | Cắt miếng | 1,0 giây | Vòng 1 |
| Thịt bò | Thái mỏng | 1,5 giây | Vòng 2 |
| Tôm | Bóc vỏ | 2,0 giây | Vòng 2 |
| Cá hồi | Lọc xương | 2,5 giây | Vòng 3 |
| Mực | Làm sạch, khứa | 2,5 giây | Vòng 3 |
| Gà ta | Chặt miếng | 2,0 giây | Vòng 4 |

**Nguyên liệu đột phá** (sơn hào hải vị — chỉ ra qua relic, luôn bậc 4–5, dùng thay nguyên liệu chính)

| Nguyên liệu | Sơ chế (thớt) | Thời gian giữ |
| --- | --- | --- |
| Tôm hùm | Bổ đôi | 3,0 giây |
| Bò Wagyu | Cắt miếng dày | 2,0 giây |
| Cá ngừ vây xanh | Lọc thăn | 2,5 giây |
| Cua hoàng đế | Tách mai | 3,0 giây |

Bạn đề xuất "thịt cá mập"; mình thay bằng **cá ngừ vây xanh** vì sản phẩm từ cá mập (nhất là vi cá) bị cấm hoặc hạn chế buôn bán ở nhiều nơi như Mỹ và Canada, dễ gây phản ứng tiêu cực từ người chơi quốc tế. Đổi lại nếu bạn muốn giữ.

**Đồ kho** (vô hạn, không có bậc, không tốn vé): Dầu ăn, Bơ, Tỏi, Nước mắm, Nước tương, Muối, Mật ong, Nước dùng, Hành lá. **Đế món** (lấy khi múc ra đĩa): Cơm, Mì Ý, Bún, Bánh mì.

---

## 5. Nấu ăn và món ăn

### 5.1 Khu bếp (Góc 4)
Một món đi qua các bước sau; mọi bước là đồ vật thật trong phòng.
1. **Lấy nguyên liệu:** cầm một món từ thùng ở Góc 2.
2. **Trái — thớt:** đặt nguyên liệu lên thớt, giữ chuột để sơ chế (thời gian theo mục 4.5); thanh tiến độ nằm ngay trên thớt. Xong thì cầm lên.
3. **Giữa — bếp:** chọn dụng cụ trên giá treo (**Chảo**, **Nồi** hoặc **Vỉ nướng**); dụng cụ hiện lên bếp. Cho từng thứ vào **đúng thứ tự**: nguyên liệu đang cầm thì bấm vào bếp, đồ kho thì bấm lọ trên kệ gia vị. Vặn **núm bếp** để nấu, rồi chờ.
4. **Phải — đĩa:** lấy một đĩa trống ở chồng đĩa (đĩa có sẵn đế món: cơm, mì Ý, bún hoặc bánh mì theo công thức), bấm vào bếp để múc món ra; đĩa đặt lên quầy chờ.
5. **Thùng rác:** đổ bỏ những gì đang ở trên bếp.
- **Quầy chờ:** giữ tối đa **3 đĩa** đã nấu trong ngày, không giảm bậc. Cuối ngày đĩa còn lại vào tủ lạnh (nếu còn ô) hoặc bị bỏ.
- **Phục vụ:** cầm đĩa từ quầy chờ ra quầy Góc 1, **bấm chuông**.

Mỗi món = **1 nguyên liệu chính (hoặc đột phá) + 1 đồ phụ + đồ kho**. Cho sai thứ tự → món ra **Lạc vị**. Đồ kho không ảnh hưởng bậc món; chúng tạo ra thứ tự cần nhớ.

### 5.2 Bậc món ăn

| Bậc | Tên | Hệ số giá |
| --- | --- | --- |
| 0 | Lạc vị | ×0,2 |
| 1 | Dở | ×0,5 |
| 2 | Bình thường | ×1,0 |
| 3 | Ngon | ×1,8 |
| 4 | Tuyệt hảo (= "hoàn hảo") | ×3,2 |
| 5 | Huyền thoại (chỉ một món duy nhất trong game, xem 5.4) | ×6,0 |

Giá gốc một món theo vòng: **60G / 70G / 85G / 100G** (vòng 1 → 4).

### 5.3 Công thức ra bậc món (hiển thị cho người chơi khi đang nấu)

```
Điểm = 0,6 × bậc nguyên liệu chính + 0,4 × bậc đồ phụ
b    = Điểm + 0,8 (+ hiệu ứng relic) (− 0,3 vào ngày mất điện)
Món ra bậc floor(b) hoặc floor(b)+1; xác suất lên bậc trên = phần lẻ của b
Giới hạn trong [Lạc vị, Tuyệt hảo]
```

Ví dụ hiển thị trên nồi:

| Nguyên liệu | b | Thanh tỉ lệ |
| --- | --- | --- |
| Chính bậc 1 + phụ bậc 1 | 1,8 | 20% Dở · 80% Bình thường |
| Chính bậc 0 + phụ bậc 1 | 1,2 | 80% Dở · 20% Bình thường |
| Chính bậc 2 + phụ bậc 2 | 2,8 | 20% Bình thường · 80% Ngon |
| Chính bậc 3 + phụ bậc 3 | 3,8 | 20% Ngon · 80% Tuyệt hảo |
| Chính bậc 4 + phụ bậc 2 | 4,0 | 100% Tuyệt hảo |

### 5.4 Món Huyền thoại
Chỉ có **một** món Huyền thoại trong game, đặt tên tạm "Món Hy Vọng". Chỉ ra được qua nước sốt thần thánh, và chỉ từ **ngày 9** (sau 2 lần trả nợ).

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

### 5.6 Máy gacha đồ ăn (tỉ lệ ẩn)
Đổi bậc một món đã nấu, lên hoặc xuống. Mỗi món chỉ được đưa vào máy **1 lần**; tối đa **3 lần mỗi ngày**; không tốn vé.

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

Tên và công thức của món được ghép từ **nguyên liệu chính** (quyết định cách nấu, dụng cụ, đế món) và **đồ phụ** (điền vào chỗ ‹đồ phụ›). Nhờ vậy mọi thùng chính đều nấu được với mọi thùng phụ — không có thùng nào bị "chết". Đầy đủ 120 món (80 thường + 40 đột phá) nằm trong `balance/recipes.json`.

| Món | Nguyên liệu chính | Dụng cụ | Thứ tự cho vào | Đế món | Thời gian nấu | Mở khóa |
| --- | --- | --- | --- | --- | --- | --- |
| Trứng chiên *‹đồ phụ›* | Trứng gà | Chảo | Dầu ăn → *(đồ phụ)* → Trứng gà → Nước mắm | Cơm | 8 giây | Vòng 1 |
| Heo xào *‹đồ phụ›* | Thịt heo | Chảo | Dầu ăn → Tỏi → Thịt heo → *(đồ phụ)* | Cơm | 10 giây | Vòng 1 |
| Đậu phụ sốt *‹đồ phụ›* | Đậu phụ | Nồi | Dầu ăn → *(đồ phụ)* → Đậu phụ → Nước mắm | Cơm | 12 giây | Vòng 1 |
| Mì Ý bò xào *‹đồ phụ›* | Thịt bò | Chảo | Bơ → Tỏi → Thịt bò → *(đồ phụ)* | Mì Ý | 10 giây | Vòng 2 |
| Bún xào tôm *‹đồ phụ›* | Tôm | Chảo | Dầu ăn → Tôm → *(đồ phụ)* → Nước mắm | Bún | 10 giây | Vòng 2 |
| Cá hồi áp chảo *‹đồ phụ›* | Cá hồi | Chảo | Bơ → Cá hồi → *(đồ phụ)* → Muối | Cơm | 14 giây | Vòng 3 |
| Mực xào *‹đồ phụ›* | Mực | Chảo | Dầu ăn → Tỏi → Mực → *(đồ phụ)* | Cơm | 8 giây | Vòng 3 |
| Gà nướng mật ong *‹đồ phụ›* | Gà ta | Vỉ nướng | Mật ong → Gà ta → *(đồ phụ)* | Bánh mì | 18 giây | Vòng 4 |
| Tôm hùm nướng bơ tỏi *‹đồ phụ›* | Tôm hùm | Vỉ nướng | Bơ → Tỏi → Tôm hùm → *(đồ phụ)* | Bánh mì | 20 giây | Relic |
| Bò Wagyu áp chảo *‹đồ phụ›* | Bò Wagyu | Chảo | Muối → Bò Wagyu → Bơ → *(đồ phụ)* | Cơm | 14 giây | Relic |
| Cá ngừ vây xanh tataki *‹đồ phụ›* | Cá ngừ vây xanh | Chảo | Dầu ăn → Cá ngừ vây xanh → *(đồ phụ)* → Nước tương | Cơm | 8 giây | Relic |
| Cua hoàng đế hấp *‹đồ phụ›* | Cua hoàng đế | Nồi | Nước dùng → Cua hoàng đế → *(đồ phụ)* → Hành lá | Bún | 20 giây | Relic |

Ngoại lệ tên: Thịt bò + Cà chua = **Mì Ý bò sốt cà chua** (món xuất hiện trong bản phác).

**Chín món vòng 1:**

| Món | Thứ tự cho vào | Dụng cụ | Nấu | Tổng thời gian một món* |
| --- | --- | --- | --- | --- |
| Trứng chiên cà chua | Dầu ăn → Cà chua → Trứng gà → Nước mắm | Chảo | 8 giây | 14,5 giây |
| Trứng chiên hành tây | Dầu ăn → Hành tây → Trứng gà → Nước mắm | Chảo | 8 giây | 14,5 giây |
| Trứng chiên rau muống | Dầu ăn → Rau muống → Trứng gà → Nước mắm | Chảo | 8 giây | 14,0 giây |
| Heo xào cà chua | Dầu ăn → Tỏi → Thịt heo → Cà chua | Chảo | 10 giây | 17,5 giây |
| Heo xào hành tây | Dầu ăn → Tỏi → Thịt heo → Hành tây | Chảo | 10 giây | 17,5 giây |
| Heo xào rau muống | Dầu ăn → Tỏi → Thịt heo → Rau muống | Chảo | 10 giây | 17,0 giây |
| Đậu phụ sốt cà chua | Dầu ăn → Cà chua → Đậu phụ → Nước mắm | Nồi | 12 giây | 19,0 giây |
| Đậu phụ sốt hành tây | Dầu ăn → Hành tây → Đậu phụ → Nước mắm | Nồi | 12 giây | 19,0 giây |
| Đậu phụ sốt rau muống | Dầu ăn → Rau muống → Đậu phụ → Nước mắm | Nồi | 12 giây | 18,5 giây |

\*Tổng = sơ chế 2 nguyên liệu + 0,5 giây mỗi lần cho vào + thời gian nấu + 1 giây múc + 1,5 giây mang ra và bấm chuông.

Sách nấu ăn (Góc 3) là quyển sách đặt trên giá: mỗi trang đôi là một món như bản phác — trang trái tên món, dụng cụ, đế món, thời gian nấu; trang phải thứ tự cho vào, mỗi bước một số tròn. Bấm trang phải để lật tới, trang trái để lật lui. Sách tự mở trang khi nguyên liệu tương ứng mở khóa, ghi kỷ lục bậc cao nhất từng nấu cho mỗi món. Mọi món dùng chung giá gốc của vòng (mục 5.2), nên công thức chỉ thay đổi trải nghiệm, không làm lệch cân bằng.

**Giá bán trên trang sách (từ 1.3):** trang trái mỗi món ghi giá bán của vòng hiện tại theo từng bậc, Lạc vị → Tuyệt hảo (= giá gốc × hệ số bậc, mục 5.2; vòng 1: 12G · 30G · 60G · 108G · 192G), kèm ghi chú "khách trả thêm tip" (mục 6.5). Huyền thoại không ghi vì chỉ có một món riêng (mục 5.4). Mọi món chung bảng giá theo vòng (bạn chọn ngày 14/11).

### 5.9 Thời gian nấu và quá lửa

| Cách nấu | Dụng cụ | Thời gian nấu |
| --- | --- | --- |
| Chiên (trứng), xào mực, tataki (cá ngừ) | Chảo | 8 giây |
| Xào (heo, bò, tôm) | Chảo | 10 giây |
| Sốt (đậu phụ) | Nồi | 12 giây |
| Áp chảo (cá hồi, bò Wagyu) | Chảo | 14 giây |
| Nướng (gà) | Vỉ nướng | 18 giây |
| Nướng (tôm hùm), hấp (cua) | Vỉ nướng, Nồi | 20 giây |

- **Một món mất trung bình 17–19 giây** từ lúc sơ chế đến lúc bấm chuông (nhanh nhất 14 giây, chậm nhất 26 giây; món đột phá 22–30 giây). Phục vụ 3 khách liền nhau mất khoảng 55 giây.
- **Quá lửa (từ vòng 2):** món để trên bếp quá **1,5 lần** thời gian nấu thì hiện cảnh báo; quá **2 lần** thì **−1 bậc**. Vòng 1 không có quá lửa.
- **Ngày 8 (mất điện):** nấu bằng nến, mọi thời gian nấu **+25%**.

Về tra cứu: các wiki của Overcooked và PlateUp mô tả cấu trúc công thức (sơ chế → nấu trong chảo/nồi → múc ra đĩa, ghép đế với sốt) nhưng không công bố số giây. Số liệu có giây rõ ràng mình tìm được là Sea of Thieves (cá chín ở 40 giây và cháy ở 80 giây; thịt chín ở 60 giây và cháy ở 120 giây) — đó là nguồn của hệ số quá lửa ×2. Thời gian nấu của game này ngắn hơn nhiều vì một ngày chỉ khoảng 6 phút.

---

## 6. Khách hàng

### 6.1 Chỗ ngồi và mở quán
Phục vụ tối đa **3 khách cùng lúc**; khách có thể đến lần lượt hoặc 3 người liền nhau. **Vòng 1: khách đến lần lượt**, người sau vào khi người trước đã được phục vụ.

**Mở quán:** đầu ngày quán đóng, người chơi gacha và nấu trước. Bấm **biển Mở quán** ở Góc 1 thì khách mới bắt đầu đến. Khi quán đang mở, bấm biển lần nữa để **đóng quán sớm**: khách còn lại ra về, không trả tiền, không bị phạt, và hết ngày.

### 6.2 Số khách mỗi ngày

| Vòng | Khách/ngày |
| --- | --- |
| 1 | 6 |
| 2 | 8 |
| 3 (ngày 9–13) | 5 |
| Ngày 14 | 8 |
| 4 | 9 (ngày 21 khách cuối là chủ nợ) |

Ngày 14 (cách hiểu của mình về "all-in"): khách quay lại đủ số lượng nhưng độ khó tính vẫn như vòng 3, để người chơi dốc hết món trong tủ lạnh, nước sốt và vé đen.

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
| chênh ≤ −2 | Không trả, quán **bị trừ 30% giá gốc** (đền bù) |
| **Sai món** (đĩa khác món khách gọi, mục 6.7) | Khách nhận đĩa nhưng giận: không trả, quán **bị trừ 30% giá gốc** — bất kể bậc món |

Hệ quả đúng ý bạn: khách khó tính được món bậc cao → tip nhiều; khách dễ tính được món bậc cao → tip ít; món dưới yêu cầu xa → bị trừ tiền.

### 6.6 Thời gian chờ (từ vòng 2)
Vòng 1 không tính giờ. Từ vòng 2, sau một **khoảng an toàn**, độ dễ tính của khách giảm **1 điểm mỗi 3 giây thực**, tối đa −20 điểm:

| Vòng | Khoảng an toàn |
| --- | --- |
| 2 | 60 giây |
| 3 | 45 giây |
| 4 | 50 giây |
| Ngày 8 (mất điện) | +15 giây so với vòng 2 |

Về tra cứu: mình không tìm được con số công bố chính thức về thời gian chờ của Overcooked, Cook Serve Delicious hay Papa's Pizzeria (các nguồn chỉ mô tả có thanh kiên nhẫn, không ghi giây). Các con số trên là mình tự đặt theo thời gian nấu của game này (trung bình 17–19 giây một món, mục 5.9): khoảng an toàn 60 giây đủ để nấu liền cho cả 3 khách ở vòng 2; vòng 3 (45 giây) thì khách thứ ba bắt đầu sốt ruột nếu không có món làm sẵn ở quầy chờ hoặc tủ lạnh — dễ hơn các game nấu ăn thông thường như bạn yêu cầu. Cần kiểm lại khi playtest.

### 6.7 Khách gọi món (từ 1.3)
Mỗi khách khi vào **gọi một món cụ thể** (một công thức ở mục 5.8), hiện trên bong bóng cạnh đầu khách cùng độ dễ tính.
- **Món gọi là món nấu được:** khách chọn ngẫu nhiên, đều nhau, một trong các món đang làm được ngay lúc vào — đĩa trên quầy chờ (hoặc tủ lạnh), món đang trên bếp, và mọi cặp (loại chính, loại phụ) đang có trong kho. Nếu không làm được món nào, khách gọi ngẫu nhiên một món đã mở khóa trong sách.
- **Mang sai món:** khách vẫn nhận đĩa nhưng giận (mục 6.5): không trả, quán bị trừ 30% giá gốc. Đĩa đó mất.
- **Đúng món:** trả tiền và tip theo bậc như mục 6.5.
- **Chủ nợ ngày 21** không gọi món cụ thể: ông gọi "món ngon nhất của quán" và nhận mọi món (vẫn cần Tuyệt hảo trở lên để được thưởng, mục 8). *(cần bạn xác nhận, Q-20)*
- Từ vòng 2 có tối đa 3 khách cùng ngồi (mục 6.1), mỗi người một món gọi; người chơi chọn phục vụ ai trước.

Mô phỏng (mục 9): người chơi không đọc chỉ số vẫn đi được 67%; người gom bộ thắng 82%. Nếu chủ nợ cũng gọi món ngẫu nhiên thì người gom bộ còn 73% — sát mốc 70% của Điều 1, nên chủ nợ là ngoại lệ.

---

## 7. Relic

### 7.1 Luật chung
- Mỗi ngày (từ ngày 2) cửa hàng hiện **3 relic khác nhau**; giá trả bằng vé trắng.
- Tối đa **8 relic**. Đầy thì phải bỏ một cái để lấy cái mới. (Mô phỏng cho thấy 8 đủ để gom 1 bộ đầy đủ cộng 4 relic lẻ, mà vẫn phải chọn lọc.)
- Mua bằng **vé đen**: 3 lựa chọn chỉ gồm relic Hiếm trở lên, và ít nhất một cái thuộc bộ bạn đang có nhiều món nhất.
- Hiệu ứng "x% nâng bậc": mỗi món trong thùng tương ứng có x% được +1 bậc (tối đa bậc 5).

### 7.2 Ba bộ combo

**Bộ Vườn Nhà** — đồ phụ

| Relic | Giá | Độ hiếm | Hiệu ứng |
| --- | --- | --- | --- |
| Bình tưới gỉ | 1 vé | Thường | Đồ phụ: 15% nâng bậc |
| Rổ tre bà ngoại | 2 vé | Thường | Thùng đồ phụ +2 món |
| Phân bón bí truyền | 2 vé | Hiếm | Đồ phụ bậc 0 được quay lại 1 lần |
| Hạt giống vàng | 3 vé | Hiếm | Đồ phụ: 6% nhảy thẳng lên bậc 4 |
| **Thưởng bộ** | | | 2 món: +20% nâng bậc · 3 món: đồ phụ tối thiểu bậc 2 và +35% nâng bậc · 4 món: mọi đồ phụ +1 bậc |

**Bộ Chợ Sớm** — nguyên liệu chính

| Relic | Giá | Độ hiếm | Hiệu ứng |
| --- | --- | --- | --- |
| Dao phay cũ | 1 vé | Thường | Nguyên liệu chính: 15% nâng bậc |
| Thùng đá | 2 vé | Thường | Nguyên liệu chính bậc 0 được quay lại 1 lần |
| Mối quen chợ cá | 2 vé | Hiếm | Thùng chính +1 món |
| Cân vàng | 4 vé | Hiếm | Nguyên liệu chính: 8% nhảy thẳng lên bậc 4 |
| **Thưởng bộ** | | | 2 món: +20% nâng bậc · 3 món: chính tối thiểu bậc 2 và +25% nâng bậc · 4 món: mọi nguyên liệu chính +1 bậc |

**Bộ Sơn Hào Hải Vị** — nguyên liệu đột phá

| Relic | Giá | Độ hiếm | Hiệu ứng |
| --- | --- | --- | --- |
| Bản đồ hải đảo | 1 vé | Thường | Thùng đột phá +10% |
| Lưới thép | 2 vé | Hiếm | Thùng đột phá +8%; tỉ lệ bậc 5 trong thùng từ 30% lên 50% |
| Thuyền thuê | 3 vé | Hiếm | Thùng đột phá +12%; **đổi lại** thùng đồ phụ −1 món |
| La bàn rồng | 4 vé | Huyền thoại | Thùng đột phá +15% |
| **Thưởng bộ** | | | 2 món: +15% · 3 món: thùng đột phá có thêm 1 món · 4 món: +20% và mọi món đột phá đều bậc 5 |

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

---

## 8. Cửa hàng vật phẩm (trả bằng tiền)

| Vật phẩm | Mở khóa | Giá | Giới hạn |
| --- | --- | --- | --- |
| Máy gacha đồ ăn | Ngày 4 | 800G | Mua 1 lần |
| Nước sốt thần thánh | Ngày 4 (qua vòng 1) | 450G, mỗi lần mua sau +45G | Giữ tối đa 20 |
| Vé đen | Ngày 4 | 700G, mỗi lần mua sau +120G | Giữ tối đa 5 |

Mô phỏng: với giá nước sốt 450G, 10% người chơi may mắn mua được chai đầu từ ngày 4–5, một phần tư mua được trước ngày 7, trung vị là ngày 8 — khớp yêu cầu "mua được tầm ngày 5 nếu nhân phẩm tốt".

**Boss cuối:** phục vụ chủ nợ ngày 21 món **Tuyệt hảo hoặc Huyền thoại** → nhận thưởng **11.500G** (ngoài tiền món). Mô phỏng: nếu bỏ khoản thưởng này thì không ai trả nổi khoản nợ cuối (tỉ lệ thắng 0%) — tức là phải thắng boss cuối mới thắng game. Đây là chủ ý: món Tuyệt hảo giữ trong tủ lạnh cho ngày 21 là mục tiêu chiến thuật của cả vòng 4.

---

## 9. Kiểm tra và thay đổi

- Mô phỏng: `balance/sim_balance.py` (4.000 ván mỗi kiểu người chơi). Mọi thay đổi số liệu phải chạy lại và đạt Điều 1.
- Mô phỏng đã đơn giản hóa: không mô hình hóa vé đen dùng để mua relic, việc bỏ relic khi đầy chỉ theo điểm ưu tiên, thời gian chờ được quy đổi thành mức giảm độ dễ tính trung bình (−3 điểm cho người nhanh, −8 cho người chậm). Kết quả là chỉ dấu, cần xác nhận bằng playtest.
- Lịch sử phiên bản:
  - **1.0 (14/11/2026):** bản đầu từ ý tưởng của bạn; mọi con số đánh dấu "claude" đã được tính và kiểm bằng mô phỏng.
  - **1.1 (14/11/2026):** thêm danh sách nguyên liệu (mục 4.5), công thức và 120 món (5.8), thời gian nấu và quá lửa (5.9), quầy chờ 3 đĩa (5.1); mỗi thùng chỉ một loại nguyên liệu (4.2). Không đổi số cân bằng: mô phỏng vẫn tính mỗi món 1 chính + 1 phụ với giá gốc theo vòng. Quá lửa chưa có trong mô phỏng — nó chỉ phạt người chơi bỏ quên bếp.
  - **1.3 (14/11/2026):** theo yêu cầu của bạn "khách hàng phải order 1 món ăn cụ thể và từng món phải hiển thị giá ở trên quyển sách công thức". Thêm mục 6.7 (gọi món), dòng "Sai món" ở 6.5, giá bán theo bậc trên trang sách (5.8), Điều 9 cập nhật vị trí thông tin. Ba lựa chọn do bạn trả lời 14/11: gọi món nấu được, sai món coi như khách giận, giá chung theo vòng ghi theo bậc. Không đổi số trong dữ liệu. Mô phỏng `sim_balance.py` thêm loại nguyên liệu cho từng thùng, công thức cho từng đĩa, khách gọi món, và 3 khách cùng ngồi từ vòng 2 (trước đây mô phỏng cho người chơi tự ghép món với khách bất kỳ); chạy `ORDERS=0` để ra lại số cũ.
  - **1.2 (14/11/2026):** theo nghiệm thu sprint 1 của bạn ("game này là 3D nhưng hiện tại chơi như game 2D"): Điều 5 đổi từ xoay camera 90° sang tự đi lại góc nhìn thứ nhất; thêm Điều 9 (thông tin nằm trên đồ vật, HUD chỉ số liệu); mục 4.2, 5.1, 5.8 tả lại thao tác bằng đồ vật; mục 6.1 thêm nút Mở quán (bạn trả lời 14/11) và đóng quán sớm. Không đổi số cân bằng; mô phỏng không bị ảnh hưởng. Thời gian đi lại trong phòng chưa có trong ước tính "17–19 giây một món" ở mục 5.9 — cần đo lại khi playtest vòng 2 (có tính giờ chờ).

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
