# PROJECT MAP
Cập nhật: 2026-11-16
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
  - Assets/QuanAnHope/ — Core (không UnityEngine): Balance.cs, RecipeBook.cs (dữ liệu); BalanceRules.cs (tra cứu theo ngày/vòng); Randomness.cs (IRandom, SeededRandom, TierRoller); Ingredients.cs (TicketWallet, IngredientGacha, IngredientStock); Cooking.cs (DishFormula, RecipeLookup, PrepStation, Stove, Counter); Customers.cs (CustomerGenerator, Payment), Data (BalanceData SO + .asset), Editor (BalanceImporter: menu "Quán Ăn Hope > Nhập balance.json"), Tests/EditMode (120 test), Tests/PlayMode (PlaythroughTests: giả lập người chơi bấm nút trên cảnh NhaHang). Core/GameSession.cs (vòng ngày, trả nợ, gọi món: MakeableRecipes/ChooseOrder, NaiveBot nấu đúng món gọi). Game (3D, từ 14/11): GameController (tự gắn vào cảnh có "NhaHang"; thao tác + tay cầm), PlayerController (FPS, tia nhìn), Interactable, RoomView (mọi đồ tương tác + bảng chữ trên đồ vật + va chạm), Hud (chỉ số liệu), Toon (khối toon lúc chạy), FaceArt (vẽ mặt nguyên liệu/món + hạt hiệu ứng lúc chạy, FaceBillboard quay về camera), KitchenFx (PanItem bay vào bếp, Wobble lọ gia vị, Pop món ra lò, DishFx hơi theo bậc), UiKit (quy cách Art Bible §13, WorldCanvas). Resources: BalanceData.asset, Fonts/Baloo2-Bold.ttf. Editor/ProjectSetup.cs (TMP, cảnh build, Build Windows)
  - Assets/BepNhanPham/Editor/PreviewCapture.cs — chụp 4 góc; RebuildAll cho batchmode
## Quy ước
- Namespace cũ: BepNhanPham. Code mới: QuanAnHope.*; Core không tham chiếu UnityEngine
- Số liệu chỉ ở balance/*.json → ScriptableObject (Điều 2)
## Điểm nhạy cảm
- Cảnh hiện tại trái constitution ở: bảng tỉ lệ máy gacha đồ ăn công khai, "3 vé/thùng", "relic tối đa 10", công thức 3 nguyên liệu, chữ UI là PNG
- Không sửa trực tiếp trong cảnh sinh ra; chạy lại menu dựng sẽ ghi đè
- balance/make_next.py (nguồn balance/v1.3/) → balance/next/*.json → chép đè balance/*.json (game đọc, từ 15/11). balance/playtest_overhead.py: phân tích playtest-log.csv (F-014). sim_balance.py: V=next đọc bộ này; có mô hình ngày theo giờ (day_seconds), gọi món, 3 ghế từ vòng 2.
- docs/ROADMAP.md (bản 2), docs/sprint-2..11/impl-plan.md: kế hoạch các sprint còn lại.
## Sprint 2 (15/11)
- Core/DayClock.cs; GameSession.Tick (đồng hồ, khách sau 3 giây, 21:00), ClosedAt; NaiveBot + BotTiming (giả định của mô phỏng); Stove.FollowsRecipe; MainIngredient.method
- Game/IngredientModels.cs (hình 22 nguyên liệu nguyên + sơ chế, Piece cho chảo); Game/PanView.cs (PanView: lớp dầu, miếng, lửa, đảo, màu, hơi, món chín; DishFood: món hoàn chỉnh dùng chung chảo + đĩa); Game/PlaytestLog.cs (playtest-log.csv)
- GameController.ClockScale (chỉ test tua đồng hồ); ảnh chụp: docs/sprint-2/ui/ (nguyen_lieu/, nau_*, 3d_*)
- Tests: EditMode 142 (DayClockTests mới); PlayMode 5 (thêm F012_HinhNguyenLieu_AnhCan, Gio21_BienTuDong_*)
## Sprint 3 (16/11) — relic
- Core/Relics.cs: RelicKeys (khoá fx), RelicFx.Combine (gộp như sim_balance P.fx, thưởng bộ cộng dồn), RelicInventory (giá 8 ô), RelicShop (3 bệ/ngày, Buy có discardId), RelicText (chữ hiệu ứng, IsActiveNow)
- Balance: relics[], setBonuses[], breakthrough, relicCap, relicShopOffers (balance.json `relic_shop_offers`). Importer: ParseBalance(json, relicsJson)
- IngredientGacha.Pull(day, fx): RollTier theo thứ tự sim, CrateCount, rương đột phá (PullResult.breakthrough, Crate.isBreakthrough), EffectiveOdds (bảng Góc 2)
- DishFormula.Relics (Muối hồng, Ớt ma, Chảo gang); Payment.Settle(..., fx) (Chuông vàng, Tượng mèo); TicketWallet.BeginDay(day, fx) (Vé số cào)
- GameSession: Relics, Shop, BuyRelic, DebugGiveRelic, Breakthrough*; NaiveBot.BuyRelic (50% như sim naive)
- Game: GameController.BuyRelic/UseShelfRelic/PendingRelicSlot; RoomView.BuildRelicShop/RefreshRelics/ShelfSummary, trạm Relic_0..2, Shelf_0..7; RelicArt.cs (22 hình relic, RelicCard, RelicFly, Sparkle); CrateView.IsBreakthrough
- Editor/RelicDebugWindow.cs (menu "Quán Ăn Hope/Gỡ lỗi: cho relic"); Editor asmdef tham chiếu Game
- Tests: EditMode 179 (RelicShopTests, RelicEffectTests); PlayMode 7 (F020_*, F022_*), ảnh docs/sprint-3/ui

