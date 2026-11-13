# PROJECT MAP
Cập nhật: 2026-11-14
## Stack
- Unity 6000.6.9f1 (D:\Unity\6000.6.9f1), URP 17.6.0, Input System 1.20.1, uGUI 2.6.0 [nguồn: My project/Packages/manifest.json]
- Chưa có: com.unity.test-framework, git repo, Blender
## Lệnh
- test: xem STATE.md | mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py
## Cấu trúc
- constitution.md — nguồn sự thật cao nhất (v1.0)
- balance/ — balance.json, relics.json, sim_balance.py (giải nén từ QuanAnHope_constitution.zip ngày 14/11)
- docs/ROADMAP.md, docs/sprint-1/impl-plan.md — kế hoạch
- My project/ — project Unity
  - Assets/BepNhanPham/ — cảnh 4 góc dựng bằng khối cơ bản: Editor/BuildNhaHang.cs (menu "Bep Nhan Pham > Dung nha hang"), Scripts/CameraRig.cs, FaceSwap.cs, Bob.cs, Shaders/ToonFlat|Outline|UnlitAlpha, Scenes/NhaHang.unity, Generated/ (bị tạo lại mỗi lần dựng)
  - Assets/QuanAnHope/ — (chưa tạo) Core / Data / Game / Editor / Tests theo impl plan mục 3
## Quy ước
- Namespace cũ: BepNhanPham. Code mới: QuanAnHope.*; Core không tham chiếu UnityEngine
- Số liệu chỉ ở balance/*.json → ScriptableObject (Điều 2)
## Điểm nhạy cảm
- Cảnh hiện tại trái constitution ở: bảng tỉ lệ máy gacha đồ ăn công khai, "3 vé/thùng", "relic tối đa 10", công thức 3 nguyên liệu, chữ UI là PNG
- Không sửa trực tiếp trong cảnh sinh ra; chạy lại menu dựng sẽ ghi đè
