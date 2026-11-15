# Impl plan — Sprint 9: Blender đợt 2 — nhân vật

Thời gian: 3/1 → 9/1/2027 (7 ngày)
Nguồn: `specs/constitution.md` v1.4 Điều 7 (mắt vòng tròn rỗng, mặt là ảnh 2D dán lên đầu 3D), §2 (Hope, chủ nợ, con gái chủ nợ); Art Bible §6 (tỉ lệ theo "đầu", bộ biểu cảm), §12 (animation: key trên 2, squash ±15%, danh sách động tác), §14.
Điều kiện vào sprint: đường ống model của sprint 8 chạy được.

## 1. Mục tiêu

Cuối ngày 9/1:

1. Khách là model có xương, dùng chung một thân, đổi màu áo và kiểu tóc / mũ; mặt vẫn là ảnh 2D đổi theo biểu cảm.
2. Khách có 3 động tác: đứng chờ (nhún), vui, giận. Thêm vẫy chào và bỏ đi nếu kịp.
3. Chủ nợ có model riêng (cao 3,6 đầu, vai vuông, vest, dây vàng).
4. Hope và con gái chủ nợ chỉ cần cho tranh cốt truyện: dựng tư thế tĩnh.

### Ngoài phạm vi

Tay của Hope trong góc nhìn thứ nhất. Khách ăn tại chỗ. Nhiều thân khách khác nhau.

## 2. Tính năng và tiêu chí chấp nhận

### F-080 Thân khách dùng chung — Art Bible §6, §12

| AC | Then |
| --- | --- |
| 1 | Một rig cho mọi khách (Art Bible §12); tỉ lệ theo "đầu" đúng §6 |
| 2 | Ít nhất 6 biến thể nhìn khác nhau từ một thân: màu áo × 3, tóc / mũ × 2; chọn ngẫu nhiên theo hạt giống của khách |
| 3 | Mặt 2D dán đúng vùng mặt, đổi 6 biểu cảm như sprint 7; chớp mắt còn chạy |
| 4 | Bong bóng món gọi và thanh kiên nhẫn vẫn nằm cạnh đầu |
| 5 | Ba khách cùng ngồi không lồng vào nhau |

### F-081 Động tác — Art Bible §12

| AC | Then |
| --- | --- |
| 1 | Đứng chờ: vòng lặp 2 giây, nhún ±3% chiều cao |
| 2 | Vui và giận: 0,6–1,2 giây, giữ tư thế đỉnh 2–4 khung; chạy đúng lúc nhận món theo kết quả trả tiền |
| 3 | Key "trên 2" (12 khung / giây, Stepped) |
| 4 | Khách vào và ra có chuyển động (trượt vào + nảy nếu chưa có animation đi) |

### F-082 Chủ nợ, Hope, con gái chủ nợ — §2, Art Bible §6

| AC | Then |
| --- | --- |
| 1 | Chủ nợ: model riêng, là nhân vật duy nhất có vai vuông; mặt lạnh lùng, đổi Ngạc nhiên khi nhận món Tuyệt hảo ngày 21 |
| 2 | Hope (2,4 đầu, tạp dề san hô) và con gái chủ nợ (bộ màu chủ nợ, hình khối tròn): tư thế tĩnh, đưa vào tranh mở đầu và kết thúc thay hình khối |

### Phương án khi trễ (constitution cho phép)

Giữ nhân vật khối cầu hiện tại, chỉ thay mặt và thêm nhún bằng code. Game vẫn phát hành được; ghi rõ trong báo cáo nghiệm thu phần nào là khối.

## 3. Việc mỗi ngày

| Ngày | Bạn (Blender) | Mình |
| --- | --- | --- |
| 1 | Thân khách: dựng khối theo §6 | Hướng dẫn rig đơn giản có ảnh; cảnh thử nhân vật |
| 2 | Thân khách: xương + gắn da | Nối vào game, kiểm vùng mặt |
| 3 | Đứng chờ, vui | Bộ điều khiển animation, nối với kết quả trả tiền |
| 4 | Giận; tóc / mũ | Biến thể màu, chọn theo hạt giống |
| 5 | Chủ nợ | Nối chủ nợ ngày 21 |
| 6 | Hope, con gái chủ nợ (tĩnh) | Tranh cốt truyện dùng model |
| 7 | Sửa | Build, nghiệm thu |

## 4. Kiểm

Công cụ kiểm quy cách của sprint 8 cho model nhân vật (thêm: số xương, tên clip). PlayMode: một ngày vòng 2 đủ 3 khách, chụp mỗi biểu cảm và mỗi động tác. Khung hình không tụt dưới 60 khi có 3 khách.

## 5. Rủi ro và phương án cắt

| Rủi ro | Phương án |
| --- | --- |
| Rig và animation khó hơn dựng khối nhiều | Thứ tự cắt: Hope / con gái → chủ nợ dùng thân khách + mũ → chỉ làm đứng chờ → phương án khối cầu |
| Mặt 2D lệch trên đầu mới | Model khai báo empty `Face` như nguyên liệu (sprint 8) |

## 6. Câu hỏi

- Khách có cần đi từ cửa vào không, hay hiện ra sau quầy như bây giờ?
