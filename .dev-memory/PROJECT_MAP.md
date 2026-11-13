# PROJECT MAP
Cập nhật: 2026-11-14
## Stack
- Unity 6000.6.9f1 (D:\Unity\6000.6.9f1), URP 17.6.0, Input System 1.20.1, uGUI 2.6.0 [nguồn: My project/Packages/manifest.json]
- Test: com.unity.test-framework 1.8.0 (builtin), Newtonsoft 3.2.1. Git + LFS, remote github.com/hungnhat29/quan-an-hope. Chưa có: Blender, gh CLI
## Lệnh
- test: xem STATE.md | mô phỏng: PYTHONUTF8=1 python balance/sim_balance.py
## Cấu trúc
- specs/constitution.md — nguồn sự thật cao nhất (v1.1); specs/sprint-1/F-00x-*/ — spec, plan, tasks
- docs/art-bible/ — Art Bible v1.0 (html, pdf, palette.json, asset_list.csv); docs/preview/ — ảnh chụp 4 góc
- balance/ — balance.json, relics.json, sim_balance.py (giải nén từ QuanAnHope_constitution.zip ngày 14/11)
- docs/ROADMAP.md, docs/sprint-1/impl-plan.md — kế hoạch
- My project/ — project Unity
  - Assets/BepNhanPham/ — cảnh 4 góc dựng bằng khối cơ bản: Editor/BuildNhaHang.cs (menu "Bep Nhan Pham > Dung nha hang"), Scripts/CameraRig.cs, FaceSwap.cs, Bob.cs, Shaders/ToonFlat|Outline|UnlitAlpha, Scenes/NhaHang.unity, Generated/ (bị tạo lại mỗi lần dựng)
  - Assets/QuanAnHope/ — Core (không UnityEngine): Balance.cs, RecipeBook.cs (dữ liệu); BalanceRules.cs (tra cứu theo ngày/vòng); Randomness.cs (IRandom, SeededRandom, TierRoller); Ingredients.cs (TicketWallet, IngredientGacha, IngredientStock); Cooking.cs (DishFormula, RecipeLookup, PrepStation, Stove, Counter); Customers.cs (CustomerGenerator, Payment), Data (BalanceData SO + .asset), Editor (BalanceImporter: menu "Quán Ăn Hope > Nhập balance.json"), Tests/EditMode (120 test), Tests/PlayMode (PlaythroughTests: giả lập người chơi bấm nút trên cảnh NhaHang). Core/GameSession.cs (vòng ngày, trả nợ, NaiveBot). Game: GameController (tự gắn vào cảnh có CameraRig), GameView (dựng UI lúc chạy), UiKit (quy cách Art Bible §13). Resources: BalanceData.asset, Fonts/Baloo2-Bold.ttf. Editor/ProjectSetup.cs (TMP, cảnh build, Build Windows)
  - Assets/BepNhanPham/Editor/PreviewCapture.cs — chụp 4 góc; RebuildAll cho batchmode
## Quy ước
- Namespace cũ: BepNhanPham. Code mới: QuanAnHope.*; Core không tham chiếu UnityEngine
- Số liệu chỉ ở balance/*.json → ScriptableObject (Điều 2)
## Điểm nhạy cảm
- Cảnh hiện tại trái constitution ở: bảng tỉ lệ máy gacha đồ ăn công khai, "3 vé/thùng", "relic tối đa 10", công thức 3 nguyên liệu, chữ UI là PNG
- Không sửa trực tiếp trong cảnh sinh ra; chạy lại menu dựng sẽ ghi đè
