# F-010 — Bộ số 1.4 vào game

Trạng thái: DONE (15/11/2026) · Nguồn: `specs/constitution.md` v1.4 §2, §3, §4.2–4.5, §5.8–5.9 · Duyệt: bảng AC trong `docs/sprint-2/impl-plan.md`; người dùng yêu cầu "triển khai impl-plan sprint 2" ngày 15/11/2026

## Luật

1. Game đọc `balance/*.json` = bản chép của `balance/next/*` (sinh từ `balance/make_next.py`). *(Điều 2)*
2. Ngày 1 có 8 vé (6 vé/ngày + 2 vé thưởng), vé thừa cộng dồn. *(§3)*
3. Một vé: thùng 3 đồ phụ + thùng 2 chính; ngày 3: 2 + 1. *(§4.2, §4.3)*
4. Sơ chế ×3, nấu ×1,5 so với 1.3 (trứng 1,5 / 12 giây; đậu phụ 3 / 18 giây). *(§4.5, §5.9)*
5. Nợ 750 / 2.900 / 10.500 / 32.000G. *(§2)*

## Thay đổi dữ liệu

- `balance/v1.3/` giữ bộ số 1.3 làm nguồn cho `make_next.py` (chạy lại không nhân hệ số hai lần). `make_recipes.py` chuyển vào đó.
- `day.next_customer_s = 3` thêm vào `make_next.py` (nguồn: §6.2 "khách kế tiếp vào 3 giây sau…"; trước đây số này chưa có trong dữ liệu).
- Bỏ `customers.per_day` khỏi `Balance` / importer / `BalanceRules.CustomersPerDay` (1.4 không còn số khách cố định).

## Ma trận AC → test

| AC | Test | Kết quả 15/11 |
| --- | --- | --- |
| 1 | `BalanceDataTests.AC1_*`, `F010_NgayTheoGio`, `AC2_AssetDaCommit_KhopVoiJson` (asset nhập lại) | xanh |
| 2 | `TicketWalletTests.AC1_Ngay1_Co8Ve`, `AC1_MoiNgaySau_Cong6Ve_VeThuaCongDon`; `GameSessionTests.F010_AC2_AC5_*` | xanh |
| 3 | `IngredientGachaTests.AC2_MotLuot_Ton1Ve_Cho3DoPhu_2Chinh`, `AC3_Ngay3_Cho2DoPhu_1Chinh`, `AC3_Ngay2_VanLaThungThuong`; giả lập PlayMode ngày 3 | xanh |
| 4 | `BalanceDataTests.AC1_NguyenLieu_Vong1`, `AC1_CongThuc_Vong1`; `KitchenTests.AC6_*`, `AC8_ChinMonVong1`; `StoveTests.AC6_PhaiChoDuThoiGianNau_MoiMucDuoc` | xanh |
| 5 | `GameSessionTests.F010_AC5_DauNgay4_KhongDu750G_Thua`, `F010_AC5_DauNgay4_Du750G_TruNo_QuaVong1` | xanh |
| 6 | `TierTableStatTests` (±1 điểm, Điều 3) | xanh |
| Mô phỏng | `PYTHONUTF8=1 python balance/sim_balance.py` trên `balance/` = kết quả `V=next`: naive tiến độ 0,63 (qua nợ 1: 0,96), nocombo 0,46, smart 0,89 | khớp |
