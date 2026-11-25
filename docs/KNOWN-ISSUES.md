# Lỗi / điểm còn biết — bản 0.11.0 rc-1 (cập nhật 25/11/2026, sprint 11)

Không có lỗi nhóm A (chặn: kẹt, mất bản lưu, thoát game) đã biết — tiêu chí 1 của `docs/sprint-11/impl-plan.md`.

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
| K-8 | **Đã sửa 0.11.0 (F-111):** cột giá tách khỏi ô tên (trước chồng 36 px), tên dài xuống tối đa 2 dòng, test `F111_*` đo cả tiếng Việt và tiếng Anh; nhãn bậc một dòng đã sửa ở 2.2. ~~Thực đơn (Tab): tên món dài chạm cột giá.~~ | Đã sửa | Nới cột tên / thu chữ nhãn bậc ở sprint 11. Ảnh: `docs/sprint-10/ui/b11_thuc_don_dot_pha.png`, `b08_tab_cach_choi_2.png`. |
| K-9 | Bot không dùng bếp thứ hai, lọ sốt cầm tay, "món ngon nhất" cho chủ nợ: Điều 1 (90% / 89% / 31%) chưa tính lợi thế bếp 2 (500G). | Cân bằng | Đo bằng buổi chơi thử thật (F-094). |
| K-10 | Giọng 3 khách đặc biệt là file bạn đưa (làm bằng Voicemod), chỉ phát 7 giây đầu; âm lượng chưa cân với tiếng game. | Thấp | Nghe thử trong bản build, chỉnh `GameController.VoiceSeconds` / âm lượng. |
| K-11 | Bếp 2 đặt sát góc phòng (x = 4,66 trong khung Góc 4); bảng tỉ lệ bếp 2 nhìn chéo từ giữa phòng. Đồ trang trí giao với thân bếp 2 tự ẩn khi bếp hiện. | Thấp (hình) | Ảnh `docs/sprint-10b/ui/c06_bep_thu_hai.png`. |
| K-12 | Benchmark 0.10.1 (24/11, cùng máy): 252 FPS, menu 3,6 giây, **5 khung > 50 ms** (0.10.0: 1–2): lần nhét vé đầu 84 ms, vài lần GC 51–57 ms. Có thể do giải nén 3 file mp3 giọng khách / hiệu ứng mới. | Thấp | F-092 AC-2 vẫn chưa đạt trọn; đặt mp3 "Decompress On Load" hoặc chuyển sang wav ngắn 7 giây ở sprint 11. |
| K-13 | Bản tiếng Anh (0.11.0) do Claude dịch, **chưa có người bản ngữ đọc lại**; vài câu ghép còn cứng ("+1 items"). Chữ trong bong bóng truyện tranh không tự xuống dòng (NoWrap như bản Việt) nên câu Anh dài có thể vượt khung. | Trung bình | Người đọc lại `tools/i18n/en.tsv`; xem `docs/sprint-11/ui/en_truyen.png`. |
| K-14 | Ảnh chụp tự động của Tab Thực đơn / Cách chơi cho thấy ô tranh còn trống vì chụp ngay lúc thẻ giấy đang "dán vào lần lượt" (stop-motion). Trong game người chơi thấy đủ tranh sau ~1 giây. | Thông tin | Chỉ ảnh hưởng ảnh test. |
| K-15 | Model Blender của 16 nguyên liệu vòng 2–4 (F-113) bám theo hình dựng bằng code cũ; măng tây dài hơn bản code (10 cm). Chưa có người duyệt hình. | Thấp (hình) | Ảnh `SourceArt/Blender/preview/_tong_hop_nguyen_lieu_vong2_4.png`. |
| K-16 | Trước khi lên Steam: tên game (Q-46), giọng 3 khách làm bằng Voicemod (`CREDITS.md`), tên nhà phát triển "hungnhat29" (Q-70). | Chặn phát hành Steam | `docs/release/STEAM-DEPLOY-PLAN.md` mục 1. |
| K-12b | **Benchmark 0.11.0** (25/11, cùng máy): 255 FPS, p99 5,3 ms, menu 3,6 giây, **4 khung > 50 ms** (82 / 61 / 54 / 51 ms, đều trùng lúc GC chạy sau nhét vé / mở Tab). Đã bật nạp nền 3 file giọng khách (F-112), nhưng nguồn khựng là GC chứ không phải giải nén mp3. | Thấp | Cần Unity Profiler (Development build) để giảm cấp phát mỗi khung; F-092 AC-2 vẫn chưa đạt trọn. |
