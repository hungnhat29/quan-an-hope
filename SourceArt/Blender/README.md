# SourceArt/Blender — model Blender của Quán ăn của Hope

Sprint 8 (impl plan `docs/sprint-8/impl-plan.md`, spec `specs/sprint-8/spec.md`). Bạn yêu cầu 21/11/2026 "tạo prj blender vào thư mục dự án này và làm các task blender của sprint", nên đợt 1 được dựng **bằng script Python của Blender** (đọc được, chạy lại được, sửa số là ra model mới), không nặn tay.

## Thư mục

| Đường dẫn | Nội dung |
| --- | --- |
| `scripts/build_models.py` | Dựng 21 model (6 nguyên liệu vòng 1 × nguyên + sơ chế, 9 đồ bếp), lưu `.blend`, xuất `.fbx` vào Unity |
| `scripts/render_previews.py` | Ảnh xem trước (Workbench) từng model + bảng tổng hợp |
| `models/*.blend` | File gốc (Git LFS). Mở bằng Blender 5.3 để sửa tay nếu muốn |
| `models/_report.txt` | Tên, nhóm, số tam giác, số vật liệu của lần dựng gần nhất |
| `preview/*.png` | Ảnh xem trước; `_tong_hop.png` là cả bộ |

FBX ra ở `My project/Assets/QuanAnHope/Resources/Models/Ingredients/` và `.../Kitchen/`. Game tự nạp theo tên (`Game/ModelLibrary.cs`); thiếu file thì dùng lại hình ghép khối cũ.

## Chạy lại

```
"D:\Blender\blender.exe" -b --factory-startup -P SourceArt/Blender/scripts/build_models.py
"D:\Blender\blender.exe" -b --factory-startup -P SourceArt/Blender/scripts/render_previews.py
```

Chỉ dựng vài model: thêm `-- SM_NL_trung SM_Chao` sau tên script. Xong mở Unity (hoặc chạy test EditMode) để Unity nhập FBX; menu **Quán Ăn Hope > Kiểm model Blender** in bảng kiểm quy cách.

## Quy cách (Art Bible §5, §10, §14)

- 1 đơn vị = 1 m; gốc ở đáy, giữa (dao: gốc ở đầu chuôi để xoay khi chặt). Kích thước khớp hình khối cũ nên va chạm và đồ trong chảo / nồi không đổi.
- Khối tròn, bo góc (bevel 2 đoạn), shade smooth. Không dựng viền: shader `OutlineNormal` của Unity đẩy mặt sau theo pháp tuyến.
- Vật liệu tên `M_Toon_RRGGBB` (mã màu bảng màu Art Bible §4); game đọc mã trong tên, không phụ thuộc không gian màu. Tối đa 3 vật liệu / mesh.
- Tam giác: nguyên liệu ≤ 600, đồ vật nhỏ ≤ 800 (lần dựng 21/11: 108–480 và 176–668).
- Nguyên liệu có empty `Face`: tâm đặt mặt theo bậc (ảnh 2D luôn quay về người chơi).
- Xuất FBX: Selected, Mesh + Empty, −Z Forward, Y Up, Apply Scalings FBX All, Apply Transform (bake). Script viết toạ độ theo Unity (hàm `U()` đổi sang Blender).

## Muốn tự sửa tay

Mở `models/SM_NL_ca_chua.blend`, sửa, rồi xuất theo đúng bảng trên vào thư mục FBX (ghi đè cùng tên). Lần chạy script sau sẽ **ghi đè** file `.blend` và `.fbx` của model đó: sửa tay thì bỏ tên model khỏi danh sách `ALL` cuối `build_models.py`. Bài học cho người mới: `docs/blender/01-ca-chua.md`.
