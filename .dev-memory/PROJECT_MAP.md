# PROJECT MAP
Cập nhật: 2026-11-18
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

## Sprint 4 (16/11) — vòng 2
- balance.json (make_next.py): seats, patience, blackout, item_shop_from_day; Balance: seatsByRound, patience, blackout, fridge, sauce, machine, blackTicket, itemShopFromDay (importer đọc hết)
- Core/Round2.cs: Patience (SafeSeconds/Drop/EasinessNow/Left), Fridge, ItemShop, ItemRolls (nước sốt, máy gacha), enum ItemBuyResult/SauceResult/MachineResult
- GameSession: Seats/SeatCount/SeatOf, CurrentCustomer = khách đến sớm nhất, ServeSeat(seat, dish), EasinessNow, PatienceLeft, Fridge/StoreInFridge, Items/BuySauce/BuyMachine/BuyBlackTicket/UseSauce/UseMachine/PullBlack, IsBlackout, LastPlayableDayFor, DebugJumpToDay/DebugAddMoney; Stove: CookMultiplier, BurnEnabled, IsOverheating/IsBurnt; DishFormula.Blackout; IngredientGacha.Pull(day, fx, black)
- NaiveBot: mua vé đen như sim naive (ngày ≥ 6), thống kê LastRunTimedServes/LastRunDrop
- Game: RoomViewRound2.cs (partial RoomView): 3 ghế Customer_0..2 (bản sao khách cảnh, BongThoai/BongThoai_1/_2), Fridge + FridgeDish_i, quầy vật phẩm Sauce/BlackTicket/FoodMachine/BlackSlot (giữa máy nhét vé và máy gacha), khói quá lửa + màu bảng bếp, nến + đèn tối ngày 8 (ItemView.HideTier); GameController: PlayableRounds=2, ServeSeat, UseFridge, BuyItem, UseSauceOnHeld, UseMachineOnHeld, PullBlack, DebugJumpToDay; Editor: cửa sổ gỡ lỗi có nút "Tới ngày 4 / 8"
- Tests: EditMode +47 (SeatsAndPatienceTests, FridgeAndOvercookTests, ItemShopTests, Round2Tests, bot sprint 4); PlayMode +4 (F030_F031_*, F032_F033_*, F034_*, F035_*), ảnh docs/sprint-4/ui; spec specs/sprint-4/spec.md

## Sprint 5 (16/11) — vong 3-4, chu no, Alpha
- balance.json `day.boss_day` (make_next.py); Balance.DayRules.bossDay; importer doc
- Core/GameSession: LastDayOfGame, IsWon, IsBossDay, BossArrived/BossWaiting/Boss/BossServed (chu no 20:00 ngay 21, uu tien ghe giua, cho ghe trong, de tinh co dinh), BlackRelicOffers/UseBlackForRelic/ChooseBlackRelic; PaymentResult.bossBonus; Customer.isBoss; Dish.IsLegend/Title/LegendName; RelicShop.RollBlackOffers; Patience bo qua chu no
- Core/Bots.cs (moi): BotStyle (Naive/NoCombo/Smart), StrategyBot (chien thuat chep tu sim_balance.py tren luat that), BotRun, BotStats (bang cung dang sim + so chenh). Lenh balance/run_bots.ps1 -> balance/bot_report.md
- Game: GameController PlayableRounds=4, DebugStartAt/DebugAddCrate, PendingIsBlack, IsBossReaction, toast vong 3/4/ngay 14/ngay 21; RoomViewRound2 BuildBossLook (mu + day vang), BossFrame bong bong vang; RoomView khe ve den "BlackRelic" (cot giua sach va be dau Goc 3), DishModel + LegendHalo; PanView vi nuong lat + vet nuong, noi hap co nap (RimY tu hinh noi that); FaceArt.Boss (lanh lung / ngac nhien); IngredientModels 8 nguyen lieu vong 3-4 chi tiet; Hud man thang / thua ngay 22
- Tests: EditMode Sprint5Tests (31), Sprint5BotTests (5 + 1 Explicit bao cao); PlayMode +5 (F043_F044_*, F044_*, F042_*, F041_*, F040_*), anh docs/sprint-5/ui; F035 doi: ngay 9 choi tiep

## Truoc sprint 6 + Sprint 6 (18/11) — khung game, constitution 1.5
- Phong 9,6 m: BepNhanPham/Scripts/RoomSize.cs (Grow 0,8, Width, Half); BuildNhaHang day moi Goc ra Grow, tuong/san/tran theo W; test PlayMode dung MinX..MaxZ +- Grow, toa do phia Goc 1 + Grow
- Khach an: balance.json day.eat_s = 5 (DayRules.eatSeconds); GameSession Eaters/EatingDish/EatProgress/AnyoneEating, FreeSeat (ghe trong sau khi an xong; chu no vao ngay); Game/EatingFx.cs (EatingFx dua + mon voi 4 mieng, LeaveFx roi ghe, Sway den dung dua)
- Core/SaveGame.cs: SaveData/SaveDish, GameSession.Snapshot()/FromSave() (partial); SeededRandom.Seed/Draws/AdvanceTo; Restore() noi bo o TicketWallet, ItemShop, Fridge, RelicShop; StrategyBot.PlayUntil
- Core/Story.cs: StoryScene, StoryPanel, StoryScript.Intro/Win/Lose/Money ("32.000G")
- Game: GameMode (Menu/Story/Playing/Paused) trong GameController (ShowMainMenu, StartNewGame, ContinueGame, Pause/Resume, PlayEnding, AutoSave, ReloadSettings, MenuCameraPosition); MenuView.cs (menu chinh, tam dung, cai dat, chieu truyen); StoryArt.cs (tranh phac bang UI); Tutorial.cs (10 buoc + mui ten 3D); GameSettings.cs (PlayerPrefs, Prefix cho test); SaveStore.cs (save.json, Directory cho test); Hud: dong huong dan, nut "Ve menu chinh" o man thang/thua; PlayerController: EscapePressed, HoldCamera/ReleaseCamera, InvertY
- F-055: RoomView.ShopName (TMP "QUAN NHAN PHAM" thay sign.png), RoomView.TextImages (33 anh chu), BuildNhaHang bo quad Sign, UpgradeLabel
- Tests: EditMode EatingTests (8), Sprint6Tests (12); PlayMode Sprint6PlayTests.cs (partial PlaythroughTests, 9 kich ban), anh docs/sprint-6/ui
- Ngoai cua (18/11): BepNhanPham/Editor/BuildStreet.cs (WallMesh tuong Goc 1 co lo cua vom, Frame, Street 4 lop Goc1/Street/Gan|Giua|Xa|Troi); bo quad DoorStreet; RoomView.BuildSway them long den pho + Drift may (EatingFx.cs)
- 19/11: Game/SkyCycle.cs (troi ngoai cua doi mau theo DayClock.HourOfDay, MaterialPropertyBlock; RoomView.Sky); Game/StoryComic.cs (trang truyen tranh ngang: bo cuc + khung chen, khung hien lan luot, camera tung khung); Game/StoryStage.cs (san khau 6 lop, 9 boi canh); Game/StoryChibi.cs (nhan vat chibi: bieu cam, dang, hoat anh muot); Game/StoryShapes.cs (hinh ve bang code); Core StoryShot/ShotFrame, StoryPanel = trang (shots, layout); bo StoryArt; bao cao reports/Phong cách giấy cắt lớp diorama.md + research_notes/
- 19/11 (2): cot truyen ve lai: Resources/Shaders/InkShape.shader (SDF uGUI) + Game/InkCanvas.cs (Begin/End bo phan, Box/Ellipse/Capsule/Hose/Arc/RingEye); Game/Puppet.cs (Hope/Boss/Daughter, Mood, Prop, HandFrontAt); Game/StoryStage.cs (9 canh nhieu lop, SfxAt, LayerFactor/LayerRect); Game/StoryComic.cs (dai ngang, PrimeTween, PanelDim); StoryArt.cs da xoa; Core StoryScene.Payoff; package com.kyrylokuzyk.primetween 1.5.3 (npm scoped registry), asmdef Game tham chieu PrimeTween.Runtime
- 20/11 cot truyen zip: tools/cot-truyen (nguon/*.js tu zip, export.js -> Resources/CotTruyen/Trang_N.json + PNG lop, README); Game/ComicBook.cs (ComicPageData/ComicCue/PolygonGraphic, MenuView dung khi StoryPanel.comicPage > 0); Core StoryScript.Chapter/ChapterPageFor/ComicDayMap, StoryPanel.Comic(); GameController.PlayChapter + PlayChapters/ChaptersByDefault (test tat); GameSettings.comicMotion; Editor/ComicTextureImporter.cs; docs/cot-truyen (mo ta hoat anh, bao cao goc); ket thua van dung StoryComic/StoryStage (phien 19/11)
- 20/11 chuyen dong muot: Game/CharacterMotion.cs (Lively tren khoi khach: tat BepNhanPham.Bob, Hop/Shake/Chew/Leave; IngredientIdle tren GiuHinh cua ItemView, chay truoc FaceBillboard); FaceBillboard chop mat (Blinking); LeaveFx/EatingFx uy quyen cho Lively; SeatView.Mood
