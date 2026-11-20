# Spec — Sprint 6: Khung game (F-050 … F-055) + hai sửa trước sprint

Ngày: 18/11/2026 · Nguồn duyệt: `docs/sprint-6/impl-plan.md` (người dùng yêu cầu triển khai 18/11/2026) · Luật: `specs/constitution.md` **v1.5** (mục 2.1, 6.2, Điều 5).
Spec này không đặt con số mới (Điều 8). Số duy nhất thêm vào dữ liệu: `day.eat_s = 5` (constitution 1.5 mục 6.2), qua `balance/make_next.py`.

## Hai sửa trước sprint (yêu cầu 18/11/2026)

| Yêu cầu | Luật | Làm ở đâu | Test |
| --- | --- | --- | --- |
| "dựng không gian 3D phòng rộng hơn 1 chút" | Điều 5 (1.5): 9,6 × 9,6 m | `BepNhanPham/Scripts/RoomSize.cs`; `BuildNhaHang` đẩy mỗi góc ra 0,8 m, tường / sàn / trần / nẹp dài theo; `RoomView` sàn va chạm | PlayMode `Phong_RongHon_TuongLuiRa` (ảnh `phong_rong.png`) |
| "thêm độ sâu cho background ở ngoài quán ăn, ở chỗ cửa" (18/11) | Điều 6 (ảnh phẳng `door_street.png` bỏ), Art Bible §3 màu | `BepNhanPham/Editor/BuildStreet.cs`: tường Góc 1 là lưới có lỗ cửa vòm thật (rộng 3 m, đỉnh 3,9 m) + khung gỗ; phố 4 lớp: gần (vỉa hè, bảng phấn, chậu cây, ghế, cột đèn), giữa (đường, dây lồng đèn đung đưa, 8 nhà thấp có hẻm giữa cửa), xa (hàng cây, nhà nhạt màu), trời (2 dải màu, mây trôi `Drift`) | PlayMode `CuaRaPho_KhoetThat_PhoNhieuLopXaDan` (ảnh `cua_ra_pho.png`, `cua_ra_pho_cheo.png`) |
| "làm trời đổi màu theo giờ trong game" (19/11) | Art Bible §3 màu; giờ từ `DayClock` (9:00–21:00, chạy qua 21:00) | `Game/SkyCycle.cs` trên `Goc1/Street`: 7 mốc màu (sáng, trưa, chiều, chiều vàng 17:30, hoàng hôn 19:00, chạng vạng 20:30, tối 22:00); phố nhuộm theo ánh trời, lớp xa pha màu chân trời; cửa sổ, lồng đèn, bóng đèn đường sáng dần 18:30–20:00; `MaterialPropertyBlock` nên vật liệu dùng chung trong phòng không đổi. Core `DayClock.HourOfDay` | EditMode `GioThuc_ChoTroiDoiMau_KhongLamTron`; PlayMode `TroiDoiMauTheoGio_ToiThiDenSang` (ảnh `troi_12h.png`, `troi_17h30.png`, `troi_19h.png`, `troi_21h.png`) |
| "dùng research skill … paper craft / stop motion / layered diorama … cho vào các khung tranh như truyện tranh để sửa lại phần cốt truyện" (19/11) | §2.1 cốt truyện; Điều 6 (chữ TMP); báo cáo `reports/Phong cách giấy cắt lớp diorama.md` | `Game/StoryComic.cs`: trang giấy trên mặt bàn gỗ, 1–2 khung / trang (mở đầu 3 trang × 2 khung, thắng 1 trang × 2, thua 1 khung lớn); khung là bìa giấy (viền trắng, bóng đổ một hướng sáng, vân giấy Perlin, băng dính); mỗi hình trong tranh thành mảnh giấy cắt (viền trắng + bóng); trang trượt vào, khung rơi xuống, chữ tượng thanh bật ra theo nhịp giật 12 hình/giây; nhân vật "rung" (boil) 6 lần/giây; camera trong khung đẩy chậm, lớp cảnh trôi chậm hơn nhân vật (thị sai). Core `StoryPanel.page`, `StoryPanel.sfx` | EditMode `F051_TruyenTranh_TrangTangDan_ToiDa2Khung`; PlayMode `F050_F053_*` (trang / số khung từng bước; ảnh `truyen_mo_dau_1..6.png`), `F051_*` (ảnh `ket_thang_1..2.png`, `ket_thua_tranh.png`) |
| "sử dụng @CotTruyen_BepNhanPham.zip để làm lại phần cốt truyện" + "research thêm skill hoặc các công cụ cần thiết cài đặt vào prj" (20/11) | constitution 1.6 §2.1; Điều 2, 6; `docs/cot-truyen/Animation_CotTruyen.md`; báo cáo `reports/Công cụ cho cốt truyện tranh.md` | `tools/cot-truyen/` (mã vẽ gốc + `export.js`: playwright-core qua Edge, tách 6 lớp / khung thành PNG không chữ + JSON chữ); `Resources/CotTruyen/Trang_1..6` (28 khung, 128 lớp, 12 MB); `Game/ComicBook.cs` (mặt nạ đa giác, khung trượt vào 8 hình, lớp bung theo độ sâu kiểu pop-up, thị sai 0,2–2, thở, boil 12/s, zoom giật 115%, rung 6 px, chớp trắng, chớp tắt đèn; nhịp từng khung `ComicCue`; chữ TMP đặt đúng chỗ, số ngày theo dữ liệu); `GameController.PlayChapter` (chương 2–4 đầu ngày); Cài đặt "Hiệu ứng truyện"; `Editor/ComicTextureImporter.cs` | EditMode `F051_TruyenTranh_ChuongVaThuTuKhung`, `F051_TruyenTranh_SoNgayTheoDuLieu`; PlayMode `CotTruyen_ChuongGiuaVan_VaDuSauTrang` (ảnh `cot_truyen_trang_1..6.png`), `F050_F053_*` (5 khung trang 1), `F051_*` (ảnh `ket_thang_1..8.png`) |
| "chỉnh sửa animation của khách hàng, nguyên liệu cũng mượt như cốt truyện" (20/11) | Art Bible §6 (khối tròn nảy mềm, nén 90/110), §11, §12 (key "trên 2" → mượt, Q-48) | `Game/CharacterMotion.cs`: `Lively` trên khối khách (nhảy vào ghế có nén / giãn, thở 2,5%, đung đưa, nhún thay `Bob`, nhảy cẫng khi vui, lắc đầu khi giận, nhai mượt, rời ghế bật lên rồi thu nhỏ); `IngredientIdle` trên khối nguyên liệu (nhún, thở 4%, nghiêng, lệch pha từng món, nảy "boing" khi sơ chế); mặt nguyên liệu chớp mắt 2,5–6 giây (`FaceBillboard`); đũa gắp ease in-out; món trong chảo nảy mượt; nguyên liệu hiện ra easeOutBack | PlayMode `KhachVaNguyenLieu_ChuyenDongMuot` (thở, đung đưa, mỗi 1/60 giây đổi < 1%, nhảy cẫng / lắc đầu, ảnh `khach_nguyen_lieu_muot.png`) |
| "hoạt ảnh ăn cho khách hàng và mất 1 khoảng thời gian mới ăn xong" | §6.2 (1.5): ăn 5 giây tại ghế, trả tiền lúc nhận món | Core `GameSession` (Eaters, EatProgress, FreeSeat); Game `EatingFx` (đũa gắp, món vơi theo 4 miếng, khách nhai), `LeaveFx` (rời ghế), gợi ý "đang ăn · còn N giây" | EditMode `EatingTests` (8), PlayMode `KhachAn_*` (ảnh `khach_an_1.png`, `khach_an_2.png`) |

## Tiêu chí chấp nhận → test

| AC | Nguồn | Test |
| --- | --- | --- |
| F-050 AC-1 mở game vào menu; con trỏ hiện; "Chơi tiếp" mờ khi chưa có bản lưu | §2.1 | PlayMode `F050_F053_*` (ảnh `menu_chinh.png`) |
| F-050 AC-2 Esc → tạm dừng (Tiếp tục, Cài đặt, Về menu chính); đồng hồ dừng | §2.1 | PlayMode `F050_F053_*` (đồng hồ không đổi 0,5 giây; ảnh `tam_dung.png`) |
| F-050 AC-3 nút theo khung, màu Art Bible §13 | Art Bible §13 | dùng `UiKit.Button` (cao ≥ 52 px, viền mực, bóng 5 px); ảnh `menu_chinh.png`, `cai_dat.png` |
| F-051 AC-1 mở đầu kể đủ 3 ý + lịch nợ | §2 | EditMode `Sprint6Tests.F051_AC1_*`; PlayMode (ảnh `truyen_mo_dau_1..6.png`) |
| F-051 AC-2 kết thắng gả con gái; kết thua ghi rõ thiếu bao nhiêu | §2, §2.1 | EditMode `F051_AC2_*` (2); PlayMode `F051_KetThang_*`, `F051_KetThua_*` (ảnh `ket_thang_1..2.png`, `ket_thua_tranh.png`, `ket_thua_man.png`) |
| F-051 AC-3 bấm để qua khung; giữ để bỏ qua | §2.1 | PlayMode `F051_AC3_GiuDeBoQua`, `F050_F053_*` (Advance) |
| F-051 AC-4 lời TextMeshPro tiếng Việt đủ dấu | Điều 6 | EditMode `F051_AC4_*` |
| F-052 AC-1 tự lưu đầu mỗi ngày đủ các trường | §2.1 | EditMode `F052_AC1_*` (2) |
| F-052 AC-2 Chơi tiếp về đúng đầu ngày; tải lại ra y như cũ | §2.1 | EditMode `F052_AC2_*`; PlayMode `F050_F053_*` (về menu giữa ngày 1 → chơi tiếp ngày 1; nạp lại cảnh → chơi tiếp ngày 2, cùng tiền / vé / relic bày) |
| F-052 AC-3 thắng hoặc thua thì xoá bản lưu | §2.1 | PlayMode `F051_KetThang_*`, `F051_KetThua_*` |
| F-052 AC-4 lưu rồi tải giống hệt từng trường | §2.1 | EditMode `F052_AC4_*` (3 seed, qua JsonUtility), `F052_SeededRandom_*`, `F052_TaiBanLuuSaiPhienBan_*` |
| F-053 AC-1 dòng chỉ dẫn HUD + mũi tên trên đồ vật kế tiếp, 10 bước | §2.1 | PlayMode `F050_F053_*` (`FollowTutorial`: mũi tên ngay trên trạm, ảnh `huong_dan_1_may_nhet_ve.png`, `huong_dan_7_vat_num.png`) |
| F-053 AC-2 làm đúng bước thì sang bước sau; không chặn việc khác | §2.1 | PlayMode `F050_F053_*` (bước đã làm trước thì tự qua) |
| F-053 AC-3 tắt được; ván thứ hai mặc định tắt | §2.1 | PlayMode `F053_AC3_*` |
| F-054 AC-1..3 âm lượng 0–100, độ nhạy (mặc định 0,1°), đảo trục, toàn màn hình / cửa sổ, độ phân giải, lưu qua lần mở | §2.1, Art Bible §9 | PlayMode `F054_CaiDat_*` (ảnh `cai_dat.png`) |
| F-055 AC-1 biển quán, nhãn máy và mọi ảnh chữ → TextMeshPro | Điều 6 | `BuildNhaHang` bỏ quad `Sign`, `UpgradeLabel`; `RoomView.BuildShopName` |
| F-055 AC-2 test quét cảnh: không vật liệu nào dùng ảnh chữ | Điều 6 | PlayMode `F055_*` (danh sách `RoomView.TextImages`, 33 ảnh; ảnh `bien_ten_quan.png`) |
| Kịch bản 10 giây | impl plan §2 | menu là phòng nhìn từ cửa, đèn treo / đèn lồng đung đưa (`Sway`), 4 nút trái; Chơi mới → màn tối dần (mờ dần 0,35 giây), Hope ôm thùng ra khỏi công ty, chữ chạy dưới |

## Mục Art Bible → làm ở đâu (L-013)

| Mục | Làm ở đâu |
| --- | --- |
| §3 bảng màu | `StoryArt` (phông, nhân vật), `MenuView` (kem #FFF3DC, san hô #E8604F, mù tạt #F2C14E, bạc hà #8CC9AE) |
| §5, §6 chủ nợ (vest #4A3F3A, dây vàng, mặt lạnh lùng / ngạc nhiên) | `StoryArt.Boss` dùng chung `FaceArt.Boss` |
| §8 phòng | 9,6 m thay 8 m (constitution 1.5 Điều 5) — Art Bible chưa sửa văn bản |
| §9 điều khiển | độ nhạy mặc định 0,1°/px trong `GameSettings` |
| §11 hiệu ứng | khách ăn: đũa nhịp 1,1 giây, thân nhún khi nhai; rời ghế thu nhỏ bay lên 0,5 giây; trời ngoài cửa đổi màu theo giờ, đèn phố sáng khi tối |
| §13 UI | `UiKit.Button` / `Panel` cho mọi nút menu, tạm dừng, cài đặt, màn thắng / thua |
| §14 cốt truyện | truyện tranh giấy cắt (`StoryComic`) trên tranh khối `StoryArt`; tranh vẽ đẹp vẫn ở sprint 9–10, khung / nhịp giữ nguyên |

## Mặc định đã chọn (`.dev-memory/QUESTIONS.md`)

1. **Thời gian ăn 5 giây** (không phải 8): đo bằng bot, 8 giây kéo người không đọc chỉ số xuống 61% (sát mốc 60%). (Q-45)
2. **Tải lại ra y như cũ** (seed + số lần rút): chọn chống "lưu rồi quay lại". (impl plan §5 câu 1)
3. **Về menu giữa ngày** mất tiến độ trong ngày (chỉ lưu đầu ngày); menu tạm dừng ghi rõ.
4. **"Chơi lại" ở màn thắng / thua** vào thẳng ngày 1, không chiếu lại mở đầu; "Về menu chính" cũng có.
5. **Biển quán vẫn ghi "QUÁN NHÂN PHẨM"** (chữ cũ của ảnh), menu ghi "Quán ăn của Hope": tên chính thức chờ bạn (Q-46).
6. **Âm lượng nhạc / hiệu ứng** lưu và có thanh trượt nhưng chưa có âm thanh (sprint 7); âm lượng tổng đặt `AudioListener.volume`.
7. **Camera menu** đứng trong rèm cửa Góc 1, nhìn về Góc 4 (rèm và đèn lồng ở sau lưng camera; thấy đèn treo đung đưa).

## Ngoài phạm vi (impl plan)

Âm thanh thật (sprint 7), tranh cốt truyện vẽ đẹp (sprint 9–10), đổi phím / tay cầm, buổi chơi thử 15 phút với người mới (ngày 7 của impl plan: cần người thật).
