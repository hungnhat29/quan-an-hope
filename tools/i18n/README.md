# Bản dịch tiếng Anh (F-110, sprint 11)

Game hiển thị chữ qua TextMeshPro; mỗi chữ có bộ dịch (`Game/Lang.cs` → `Core/Translator.cs`). Code và dữ liệu vẫn viết tiếng Việt; chỉ chữ **hiển thị** đổi theo Cài đặt → Ngôn ngữ / Language.

## Khi thêm hoặc sửa chữ trong game

```
PYTHONUTF8=1 python tools/i18n/extract.py   # gom moi chuoi tieng Viet -> strings_vi.json
# them dong vao en.tsv cho chuoi moi:  tieng Viet <TAB> English  (kieu chuoi JSON: \n, \")
PYTHONUTF8=1 python tools/i18n/build.py     # ghep -> My project/Assets/QuanAnHope/Resources/Lang/en.json, bao thieu
```

`build.py` thoát lỗi nếu còn chuỗi chưa dịch, bản dịch còn dấu tiếng Việt, hoặc số lỗ `{0}` khác nhau. Test `LanguageTests.AC3_*` kiểm lại trong Unity.

## Cách bộ dịch tìm câu

1. Nguyên văn (kể cả bỏ khoảng trắng / dấu ngăn ở hai đầu, chữ đầu viết hoa, chữ IN HOA, thẻ `<size>` ở đầu / cuối).
2. Mẫu có lỗ: câu ghép trong code (`"Đã mua " + x + " · còn " + n + " vé."` → `Đã mua {0} · còn {1} vé.`), thử mọi cách cắt, chữ trong lỗ dịch tiếp.
3. Tách dòng / tách theo ` · `.
4. Thay mảnh còn sót theo ranh giới từ.
5. Đổi số kiểu Việt sang kiểu Anh (`32.000G` → `32,000G`, `0,3%` → `0.3%`).

Tên 84 món sinh tự động từ mẫu món chính (`Trứng chiên {p}` → `Fried egg with {p}`) và tên đồ phụ trong món.

## Tìm chữ còn sót trên màn hình

Chạy cả bộ PlayMode với biến môi trường `QAH_FORCE_EN_LOG=<đường dẫn file>`: game luôn ở tiếng Anh và ghi mọi chữ hiển thị còn dấu tiếng Việt vào file (cột 1 gốc, cột 2 sau dịch). Sau đó khôi phục ảnh của sprint cũ (`git checkout -- docs/sprint-*/ui`, L-018).
