# Lỗi / điểm còn biết — bản 0.10.1 (F-091 AC-4; cập nhật 24/11/2026)

Cập nhật 23/11/2026. Đo trên máy của bạn (Intel Iris Xe, 1920×1080, DirectX 11), lệnh `QuanAnHope.exe -benchmark`.

| # | Mô tả | Mức | Ghi chú / hướng sửa |
| --- | --- | --- | --- |
| K-1 | Lần **nhét vé đầu tiên** sau khi vào ván có một khung 74–98 ms (các lần sau < 20 ms). Đã làm nóng icon, font, shader, âm thanh, một thùng mẫu lúc ở menu: giảm từ 9 khung > 50 ms xuống 1–2 nhưng chưa hết. | Thấp | F-092 AC-2 chưa đạt trọn. Cần Profiler (Unity Editor, bản Development) để biết phần còn lại (có thể là tải lưới / vật liệu lần đầu khi vẽ bằng camera chính). |
| K-2 | Thỉnh thoảng một chuỗi 3–6 khung ~55 ms khi bộ gom rác (GC) chạy giữa ván (1 lần / 30 giây đo). | Thấp | Đã gom rác lúc chuyển ngày / vào ván. Còn: giảm cấp phát chuỗi mỗi khung (HUD, bong bóng). |
| K-3 | Lần mở Tab đầu có thể 50–57 ms (đã dựng sẵn thực đơn lúc ở menu: 18–24 ms đồng bộ). | Thấp | Như K-1. |
| K-4 | Bot không mô hình thớt thứ 2 / thả nguyên liệu / đi lại song song, nên Điều 1 đo bằng bot chưa tính lợi thế của thớt mua thêm. | Cân bằng | Đo bằng buổi chơi thử thật (F-094). |
| K-5 | Đổi tên sản phẩm (0.10.0) làm chỗ lưu chuyển sang `LocalLow\hungnhat29\Quán ăn của Hope`: bản lưu / cài đặt của các bản trước không được đọc. | Thấp | Chỉ ảnh hưởng máy đã chơi bản cũ. |
| K-6 | Chưa chạy thử trên một máy khác máy của bạn (F-093 AC-2). | Chờ | Gửi `QuanAnHope-0.10.0.zip` cho một người chơi thử. |
| K-7 | Mô phỏng Python (`sim_balance.py`) không mô hình giá niêm yết, 84 món, khách bỏ đi — số Python lệch bot. | Thông tin | Bot C# là chuẩn từ 1.7. |
| K-8 | Thực đơn (Tab): tên món dài (vd "Tôm hùm nướng bơ tỏi hành tây") chạm cột giá; thẻ tranh nhỏ: nhãn bậc "Bình thường", "Tuyệt hảo" xuống hai dòng. | Thấp (hình) | Nới cột tên / thu chữ nhãn bậc ở sprint 11. Ảnh: `docs/sprint-10/ui/b11_thuc_don_dot_pha.png`, `b08_tab_cach_choi_2.png`. |
| K-9 | Bot không dùng bếp thứ hai, lọ sốt cầm tay, "món ngon nhất" cho chủ nợ: Điều 1 (90% / 89% / 31%) chưa tính lợi thế bếp 2 (500G). | Cân bằng | Đo bằng buổi chơi thử thật (F-094). |
| K-10 | Giọng 3 khách đặc biệt là file bạn đưa (làm bằng Voicemod), chỉ phát 7 giây đầu; âm lượng chưa cân với tiếng game. | Thấp | Nghe thử trong bản build, chỉnh `GameController.VoiceSeconds` / âm lượng. |
| K-11 | Bếp 2 đặt sát góc phòng (x = 4,66 trong khung Góc 4); bảng tỉ lệ bếp 2 nhìn chéo từ giữa phòng. Đồ trang trí giao với thân bếp 2 tự ẩn khi bếp hiện. | Thấp (hình) | Ảnh `docs/sprint-10b/ui/c06_bep_thu_hai.png`. |
| K-12 | Benchmark 0.10.1 (24/11, cùng máy): 252 FPS, menu 3,6 giây, **5 khung > 50 ms** (0.10.0: 1–2): lần nhét vé đầu 84 ms, vài lần GC 51–57 ms. Có thể do giải nén 3 file mp3 giọng khách / hiệu ứng mới. | Thấp | F-092 AC-2 vẫn chưa đạt trọn; đặt mp3 "Decompress On Load" hoặc chuyển sang wav ngắn 7 giây ở sprint 11. |
