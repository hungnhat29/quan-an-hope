# F-001 — Dữ liệu cân bằng

Trạng thái: DONE (14/11/2026) · Nguồn: `specs/constitution.md` v1.1 · Duyệt: người dùng yêu cầu thực hiện ngày 1 của `docs/sprint-1/impl-plan.md` (14/11/2026)

## Luật

1. Mọi tỉ lệ, giá, nợ, hệ số là dữ liệu sinh từ JSON; code không chứa con số cân bằng. *(Điều 2)*
2. Số liệu nằm ở `balance/balance.json` và `balance/recipes.json`; bảng trong constitution và JSON phải khớp nhau. *(constitution dòng 7)*
3. Game đọc số qua một ScriptableObject `BalanceData.asset` sinh từ hai file JSON bằng công cụ trong Editor. *(Điều 2)*

## Tiêu chí chấp nhận

### AC-1 — Nhập JSON sinh ra asset *(Điều 2; §2, §3, §4.2, §4.3, §4.5, §5.2, §5.3, §5.8, §5.9, §6.2–6.4)*
- **Given** `balance/balance.json` và `balance/recipes.json` ở gốc repo
- **When** chạy menu `Quán Ăn Hope > Nhập balance.json`
- **Then** có `Assets/QuanAnHope/Data/BalanceData.asset` chứa mọi trường sprint 1 dùng:
  lịch nợ, vòng theo ngày, vé mỗi ngày, tiền và vé khởi đầu, 6 bảng tỉ lệ bậc, kích thước thùng, công thức bậc món và giá, số khách mỗi ngày, dải độ dễ tính, món tối thiểu; 10 đồ phụ, 12 nguyên liệu chính và đột phá, 120 công thức, thời gian nấu.

### AC-2 — Asset khớp JSON *(constitution dòng 7)*
- **Given** `BalanceData.asset` đã commit
- **When** đọc lại hai file JSON
- **Then** mọi giá trị trong asset bằng giá trị trong JSON; nếu JSON đã đổi mà chưa nhập lại thì test báo lỗi.

### AC-3 — Bảng tỉ lệ hợp lệ *(§4.3, §6.3)*
- **Given** dữ liệu đã nhập
- **Then** có đủ 6 bảng tỉ lệ bậc (r1, storm, r2, r3, r4, black), mỗi bảng 6 bậc và tổng bằng 100; tỉ lệ ba dải khách của mỗi vòng cộng lại bằng 100%.

## Ngoài phạm vi

- Dữ liệu relic, thưởng bộ, thùng đột phá, nước sốt, máy gacha đồ ăn, vé đen, tủ lạnh (sprint 2–3).
- Tự nhập lại khi JSON đổi; hiện phải chạy menu bằng tay, AC-2 nhắc khi quên.
- Kiểm tra bảng trong constitution khớp JSON bằng máy; test chỉ đối chiếu một số con số chép tay từ constitution.

## Ma trận AC → test

File: `My project/Assets/QuanAnHope/Tests/EditMode/BalanceDataTests.cs`

| AC | Test | Kết quả 14/11 |
| --- | --- | --- |
| 1 | `AC1_Import_TaoRaAssetDocLaiDuoc`, `AC1_TienTe_VaLichNo`, `AC1_Vong_TheoNgay`, `AC1_TiLeBac_VaKichThuocThung`, `AC1_CongThucBacMon_VaGia`, `AC1_Khach`, `AC1_NguyenLieu_Vong1`, `AC1_CongThuc_Vong1`, `AC1_ThoiGianNau` | 9/9 xanh |
| 2 | `AC2_AssetDaCommit_KhopVoiJson` | xanh |
| 3 | `AC3_MoiBangTiLeBac_CoTong100`, `AC3_TiLeKhachMoiVong_CoTong100Phantram` | 2/2 xanh |
