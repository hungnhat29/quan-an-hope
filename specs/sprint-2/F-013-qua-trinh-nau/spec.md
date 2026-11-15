# F-013 — Quá trình nấu nhìn ra nấu ăn

Trạng thái: DONE, chờ bạn duyệt bằng mắt · Nguồn: ghi chú 14/11 "xấu và lộn xộn, không giống quá trình nấu ăn"; constitution §5.1, §5.8, §5.9 · Kịch bản 10 giây trong `docs/sprint-2/impl-plan.md`

## Thiết kế

- `Game/PanView.cs` gắn vào "DoTrongBep": Dầu ăn / Bơ / Nước dùng loang thành lớp phủ 86% đáy; Nước mắm / Nước tương / Mật ong thành vệt sốt; Tỏi / Muối / Hành lá thành hạt nhỏ.
- Nguyên liệu → 4–6 miếng đã cắt (`IngredientModels.Piece`) trên một vòng, nguyên liệu thứ hai ở vòng trong, lệch góc. Trứng đánh → lớp trứng lỏng phủ đáy, khi nấu đông dần từ viền vào.
- Vặn núm: vòng lửa xanh đầu cam lộ ra quanh đáy dụng cụ; chảo được đảo theo nhịp 1,4 giây; nồi có bọt sôi; vỉ xèo rung. Màu đậm dần theo tiến độ (MaterialPropertyBlock). Hơi trắng bốc lên.
- Chín: `DishFood` dựng món hoàn chỉnh theo `method` của nguyên liệu chính (dữ liệu `recipes.json`): chiên = miếng trứng tròn viền sẫm có đồ phụ; xào = các miếng trộn trên hai vòng; sốt/hấp = miếng nổi trong nước sốt; áp chảo/nướng = miếng lớn giữa + đồ phụ (mức cơ bản).
- Múc ra đĩa: đĩa dùng cùng `DishFood` + đế món (cơm / mì Ý / bún / bánh mì).
- Sai thứ tự (`Stove.FollowsRecipe` = false): màu sẫm dần thành cháy xém, khói xám, đĩa Lạc vị cũng cháy xém.
- `PanSpec` sửa theo hình dụng cụ thật: chảo bán kính trong 0,25 m (trước 0,17); nồi: mặt canh 1,47 m ngay dưới vành 1,51 m (trước đồ nổi lơ lửng ở 1,83 m).

## Ma trận AC → test

| AC | Test | Kết quả 15/11 |
| --- | --- | --- |
| 1 | `PlaythroughTests.CheckPanSettled`: lớp dầu rộng ≥ 80% lòng chảo, mỏng < 3 cm | xanh |
| 2 | `CheckPanSettled`: mỗi nguyên liệu 4–6 miếng, khoảng cách ≥ 3,5 cm, nằm trong lòng dụng cụ | xanh |
| 3 | `CookOne`: `Pan.FlameOn` khi vặn núm, đồ rung; `Doneness` tăng; ảnh `nau_5_dang_nau.png` | xanh + CẦN NGƯỜI KIỂM |
| 4 | `CookOne`: `Pan.Finished` có khi chín, tắt lửa; ảnh `nau_6_chin.png` | xanh + CẦN NGƯỜI KIỂM |
| 5 | `CookOne`: đĩa có "MonAn" + "DeMon"; ảnh `nau_7_dia.png` | xanh + CẦN NGƯỜI KIỂM |
| 6 | `StoveTests.F013_AC6_BietSaiThuTuTuLucBatDauNau`; `CookOne(correctOrder:false)`: `Pan.ShowsBurnt`; ảnh `nau_sai_thu_tu_*.png` | xanh |

Chuỗi ảnh: `docs/sprint-2/ui/nau_1..7_*.png` (Heo xào cà chua), `nau_sai_thu_tu_1..7_*.png`. Giả lập chưa chụp riêng chuỗi trứng chiên của kịch bản; trứng chiên và đậu phụ sốt có trong ván 3 ngày nhưng không có ảnh cận.

## Ngoài phạm vi

Nướng / hấp / áp chảo chi tiết (sprint 4–5, theo phương án cắt của impl plan); âm thanh xèo (sprint 7).
