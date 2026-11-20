# Công cụ xuất truyện tranh (CotTruyen)

Nguồn tranh: `CotTruyen_BepNhanPham.zip` (20/11/2026). Mã vẽ gốc nằm ở `nguon/`: `p1.js` … `p6.js` là 6 trang, `lib.js` là nhân vật và bóng thoại, `scenes.js` và `kit.js` là bối cảnh và đạo cụ. Bản mô tả hoạt ảnh: `docs/cot-truyen/Animation_CotTruyen.md`.

## Xuất sang Unity

```
cd tools/cot-truyen
npm install            # playwright-core + font Baloo 2 / Be Vietnam Pro (@fontsource)
node export.js         # cả 6 trang; hoặc: node export.js p3
```

Cần Microsoft Edge có sẵn trên máy (playwright-core dùng `channel: 'msedge'`, không tải trình duyệt). **Không** chạy `npx playwright install msedge`, vì lệnh này cài đè Edge của máy.

Kết quả nằm ở `My project/Assets/QuanAnHope/Resources/CotTruyen/`:

| File | Nội dung |
| --- | --- |
| `Trang_N.json` | Đa giác khung, lớp ảnh, chữ (toạ độ trang 1920×1080, y hướng xuống) |
| `Trang_N/k{khung}_lop{0..5}.png` | Lớp giấy pp0 (nền) … pp5 (tiền cảnh). Không cắt theo khung, chừa lề 90 px để chạy thị sai |
| `Trang_N/k{khung}_toi.png` | Góc tối của khung |
| `Trang_N/nen.png`, `vien.png`, `phu.png` | Giấy và bóng khung, viền mực, góc tối toàn trang |

Ảnh xem trước bản gốc (chữ dùng font của trình duyệt) ở `docs/cot-truyen/xuat/`.

## Quy tắc

- **Chữ không nằm trong ảnh** (constitution Điều 6): mọi `<text>` có chữ cái hoặc số được tách sang JSON, Unity vẽ lại bằng TextMeshPro. Ký hiệu như ★ ☠ không có trong font Baloo 2 nên giữ trong ảnh; TMP chừa chỗ bằng `<space=…>`.
- **Hạt giấy không nướng vào ảnh** (`NO_GRAIN`): nhiễu làm PNG nặng gấp khoảng 20 lần. Unity phủ một lớp hạt chung lên trang.
- **Cạnh ảnh chia hết cho 4**, để Unity nén được (BC7, xem `Editor/ComicTextureImporter.cs`).
- **Số ngày trên tranh** ("Ngày 15", "Ngày 10–13"): Unity đổi theo `balance.json` (`StoryScript.ComicDayMap`).
- **Nhịp máy quay và hiệu ứng từng khung** nằm ở `Game/ComicBook.cs` (`ComicCue.For`).

Sửa tranh: chỉnh `nguon/p*.js` rồi chạy lại `node export.js pN`. Phải giữ đủ 6 lớp `layer(N, …)` và các hàm `balloon` / `caption` / `sfx` cho chữ.
