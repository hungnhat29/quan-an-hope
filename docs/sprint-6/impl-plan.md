# Impl plan — Sprint 6: Khung game

Thời gian: 13/12 → 19/12/2026 (7 ngày)
Nguồn luật: `specs/constitution.md` v1.4 Điều 5 (màn ngoài phòng: menu, kết thúc ngày, cốt truyện), Điều 6 (chữ dùng TextMeshPro, không dùng ảnh chữ), §2 (cốt truyện). Hình: Art Bible §3, §13, §14.
Điều kiện vào sprint: Alpha (sprint 5) chơi được đủ 22 ngày.

**Lưu ý nguồn:** lưu game, hướng dẫn và cài đặt **chưa có trong constitution**. Đây là đề xuất; ngày 1 phải được bạn duyệt và ghi vào constitution (bản 1.5) rồi mới làm (Điều 8).

## 1. Mục tiêu

Cuối ngày 19/12, người chưa từng thấy game mở bản build và tự chơi được:

1. Menu chính: Chơi mới, Chơi tiếp, Cài đặt, Thoát.
2. Chơi mới → 4–6 khung tranh cốt truyện mở đầu (Hope bị đuổi việc, gặp chủ nợ, nhận quán) rồi vào ngày 1.
3. Ngày 1 có hướng dẫn từng bước ngay trong phòng.
4. Thoát giữa chừng, mở lại, Chơi tiếp từ đầu ngày đang chơi.
5. Cài đặt: âm lượng, độ nhạy chuột, toàn màn hình / cửa sổ, độ phân giải.
6. Thắng → tranh kết (chủ nợ gả con gái); thua → màn thua có nút chơi lại.
7. Trong cảnh không còn ảnh chữ PNG nào.

### Ngoài phạm vi

| Không làm | Sprint |
| --- | --- |
| Âm thanh thật (chỉ có thanh trượt âm lượng) | 7 |
| Tranh cốt truyện vẽ đẹp: sprint này dùng tranh phác đơn giản dựng bằng hình của game | 9–10 |
| Đổi phím, tay cầm | sau phát hành thử |

## 2. Tính năng và tiêu chí chấp nhận

### F-050 Menu và luồng màn — Điều 5

| AC | Then |
| --- | --- |
| 1 | Mở game vào menu; con trỏ chuột hiện; "Chơi tiếp" mờ khi chưa có bản lưu |
| 2 | Esc trong phòng mở menu tạm dừng (Tiếp tục, Cài đặt, Về menu chính); đồng hồ ngày dừng khi tạm dừng |
| 3 | Mọi nút dùng khung và màu của Art Bible §13 |

### F-051 Cốt truyện mở đầu và kết thúc — §2

| AC | Then |
| --- | --- |
| 1 | Mở đầu kể đủ 3 ý của §2: bị đuổi việc, gặp chủ nợ, nhận quán và lịch đòi nợ |
| 2 | Kết thắng: trả hết nợ ngày 22, chủ nợ gả con gái; kết thua: không đủ tiền ở lần trả nợ nào thì ghi rõ thiếu bao nhiêu |
| 3 | Bấm để qua từng khung; giữ để bỏ qua |
| 4 | Lời thoại là chữ TextMeshPro tiếng Việt đủ dấu |

### F-052 Lưu / tải theo ngày — *đề xuất, cần duyệt*

| AC | Then |
| --- | --- |
| 1 | Tự lưu vào đầu mỗi ngày (sau khi thu nợ, phát vé): ngày, tiền, vé trắng, vé đen, relic, tủ lạnh, nước sốt, máy gacha, số lần đã mua, hạt giống ngẫu nhiên |
| 2 | Chơi tiếp → về đúng đầu ngày đó; chơi lại ngày đó cho kết quả gacha khác (không cho "lưu rồi quay lại" để chọn kết quả tốt: hạt giống đổi mỗi lần tải) — *chọn một trong hai, hỏi bạn* |
| 3 | Thắng hoặc thua thì xoá bản lưu |
| 4 | Test: lưu rồi tải cho trạng thái giống hệt từng trường |

### F-053 Hướng dẫn ngày 1 — *đề xuất, cần duyệt*

| AC | Then |
| --- | --- |
| 1 | Ngày 1 của ván đầu: một dòng chỉ dẫn trên HUD + mũi tên nổi trên đồ vật kế tiếp cần bấm (máy nhét vé → thùng → thớt → giá dụng cụ → sách → bếp → núm → chồng đĩa → biển Mở quán → chuông) |
| 2 | Làm đúng bước thì sang bước sau; không chặn người chơi làm việc khác |
| 3 | Tắt được trong Cài đặt; ván thứ hai trở đi mặc định tắt |

### F-054 Cài đặt — *đề xuất, cần duyệt* (liên quan Q-17)

| AC | Then |
| --- | --- |
| 1 | Âm lượng tổng, nhạc, hiệu ứng (0–100) |
| 2 | Độ nhạy chuột (mặc định 0,1°/điểm ảnh theo Art Bible §9), đảo trục dọc |
| 3 | Toàn màn hình (mặc định) / cửa sổ; độ phân giải; lưu qua các lần mở |

### F-055 Xoá ảnh chữ — Điều 6

| AC | Then |
| --- | --- |
| 1 | Biển "QUÁN NHÂN PHẨM", nhãn relic cũ, nhãn máy nâng phẩm và mọi texture có chữ vẽ sẵn được thay bằng TextMeshPro trên đồ vật |
| 2 | Test quét cảnh: không còn vật liệu nào dùng các ảnh trong danh sách ảnh chữ |

**Kịch bản 10 giây (duyệt ngày 1):** màn menu là chính căn phòng nhìn từ cửa, đèn lồng đung đưa; bốn nút xếp dọc bên trái. Bạn bấm "Chơi mới": màn tối dần, khung tranh đầu hiện Hope ôm thùng đồ ra khỏi công ty, chữ chạy bên dưới.

## 3. Lịch 7 ngày

| Ngày | Việc |
| --- | --- |
| 1 | Gửi bạn 3 đề xuất (lưu, hướng dẫn, cài đặt) + kịch bản; phần được duyệt ghi vào constitution 1.5. Làm F-050 |
| 2 | F-052 Core: tuần tự hoá `GameSession` (test đỏ → xanh) |
| 3 | F-052 Game: Chơi tiếp, tự lưu; F-054 |
| 4 | F-051 mở đầu và kết thúc |
| 5 | F-053 hướng dẫn |
| 6 | F-055; giả lập PlayMode: menu → truyện → ngày 1 theo hướng dẫn → thoát → chơi tiếp |
| 7 | Build, nghiệm thu; nhờ một người chưa biết game chơi 15 phút, ghi chỗ họ kẹt |

## 4. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Bạn không duyệt lưu game | Bỏ F-052, thêm lệnh "bắt đầu từ vòng N" vào menu gỡ lỗi để vẫn chơi thử được |
| Tranh cốt truyện tốn công | Sprint này chỉ cần đúng nội dung; dùng nhân vật khối + phông màu |

## 5. Câu hỏi

- Duyệt lưu theo ngày? Tải lại có cho quay gacha lại không?
- Lời cốt truyện: mình viết nháp theo §2, bạn sửa.
- Tên chính thức của game (đang là "Quán ăn của Hope (tên tạm)", biển trong cảnh ghi "QUÁN NHÂN PHẨM").
