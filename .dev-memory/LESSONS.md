# LESSONS

## Quy tac dang ap dung
- [L-014] Tinh nang co hien thi: viet "kich ban 10 giay" (dung dau, nhin gi, bam gi, thay gi) cho nguoi dung duyet TRUOC khi code; cho constitution im lang ve vong choi cot loi thi hoi, khong tu mac dinh (P2) [sprint 1: lam lai UI 2D->3D, them goi mon]
- [L-013] Spec co hinh phai co bang "muc Art Bible -> lam o dau" va moi thao tac co AC "nguoi choi thay gi" (hinh + chuyen dong) (P2/P6) [sprint 1: thieu mat, thieu animation]
- [L-015] Test moi phai chay thay DO mot lan truoc khi viet code, ke ca do vi chua bien dich (P4) [count 2: sprint 1 bo 8 test goi mon; sprint 3 viet Core relic truoc test, chi do nho importer chua nhap]
- [L-011] Khong chay thu ban build voi -screen-fullscreen 0 / -screen-width: Unity luu vao registry, lan sau nguoi dung mo len thanh cua so (nguoi dung bao 14/11). Chay thu khong co co man hinh (P6)
- [L-012] Doi luat ghep mon / khach phai dua vao sim_balance.py va chay lai Dieu 1 truoc khi code; luat goi mon lam nguoi choi gioi tu 83% xuong 59% neu mo phong thieu 3 khach cung ngoi + ngoai le chu no (P2)
- [L-009] Tam anh quay ve camera dat truoc vat 3D: day ra bang ban kinh bao cua than (khong phai nua be day), test bang ban kinh tu localBounds (AABB cua khoi xoay phinh to) (P4)
- [L-010] Doc ket qua test Unity: grep result="[A-Za-z()]*" vi that bai ghi "Failed(Child)"; khong in dong ket qua = coi la DO (P6)
- [L-007] Nguoi dung muon game 3D that: moi thong tin dat tren do vat, HUD chi so lieu, khong nut chuyen goc (constitution v1.2 Dieu 5, 9). Tinh nang moi phai co do vat 3D de bam (P2/P3)
- [L-008] Do vat tao luc chay (BoxCollider) phai goi Physics.SyncTransforms; san canh khong co collider; test PlayMode tua nhanh can chia buoc di (P4)
- [L-005] UI moi phai chup anh tu PlayMode (docs/sprint-N/ui) va tu xem truoc khi bao xong: loi hien thi chi lo ra tren anh (P6) [count 2: sprint 2 do trong chao nho, lua khuat duoi day chao du test xanh]
- [L-006] Test PlayMode bam nut qua ten GameObject; doi ten nut trong GameView thi sua PlaythroughTests cung luc (P4)
- [L-002] Trong Bash tool cua may nay, heredoc nhieu dong co dau nhay don bi loi cu phap; ghi script .py bang Write vao scratchpad roi chay (moi pha) [count 3: sprint 3 heredoc chua ''' long nhau; them: khong de "cat > file" khong co heredoc, lenh treo cho stdin]
- [L-003] Bat/sua hau ky URP phai chup anh so voi ban khong hau ky (PreviewCapture) truoc khi bao xong (P6)
- [L-004] Chay Unity batchmode: dong Editor truoc; -testResults va -logFile dung duong dan tuyet doi kieu Windows (P4)
- [L-001] Chạy sim_balance.py phải đặt PYTHONUTF8=1 (Windows mặc định cp1258, lỗi đọc JSON tiếng Việt) (P4/P6)
- [L-016] Hinh dat trong/tren do vat 3D: lay kich thuoc tu hinh that (BuildTool x scale), khong tu so cu; test hinh hoc phai do theo hinh that (P4) [sprint 2: PanSpec 0,17 m trong chao 0,29 m]
<!-- max 15 lines: [L-id] rule (phase) -->

## Nhat ky

### 2026-11-14 F-008
- L-009 | trigger: anh chup 3d_cam_nguyen_lieu cho thay dau phu tren tay khong co mat | sai: mat dat cach tam 0,52 x be day, khoi vuong nhin xeo che mat | goc: tia tu tam toi camera di cheo, ban kinh bao lon hon nua be day | quy tac: day mat bang ban kinh bao | kiem: AssertFaceOutside (thay do truoc khi sua) | count 1
- L-010 | trigger: lan chay PlayMode thu 2 khong in dong ket qua | sai: grep chi khop chu cai, bo qua Failed(Child) | quy tac: grep ca ngoac, khong co dong = doc lai xml | count 1

### 2026-11-14 F-009
- L-011 | trigger: nguoi dung bao game dang o che do cua so | sai: lenh chay thu build co -screen-fullscreen 0 -screen-width 1280 | goc: Unity luu Screenmanager* vao HKCU\Software\DefaultCompany\My project | sua: xoa khoa, GameController ep FullScreenWindow khi chay build, ProjectSetup dat PlayerSettings | count 1 (nguoi dung chi ra)
- L-012 | trigger: mo phong sau luat goi mon tut duoi Dieu 1 | goc: mo phong cu cho nguoi choi tu ghep mon voi khach | quy tac: luat moi anh huong ghep mon phai mo phong truoc | count 1

### 2026-11-14 Dong sprint 1
- Tong ket day du: .dev-memory/RETRO-sprint-1.md (10 loi, 6 dieu nguoi dung chua hai long, cau hoi con mo)
- L-013, L-014, L-015 dua len quy tac dang ap dung (nguon: nguoi dung tu choi nghiem thu 2 lan + yeu cau dong sprint)

### 2026-11-15 Sprint 2 (F-010..F-014)
- L-002 | trigger: heredoc python sua GameSessionTests loi "unexpected EOF" | sua: ghi script vao scratchpad bang Write roi chay | count 2
- L-005 | trigger: PlayMode F-013 xanh nhung anh nau_4/nau_5 cho thay do don giua chao, khong thay lua | goc: test chi kiem "nam trong ban kinh", khong kiem "phu du long chao"; lua o ban kinh 0,1 m duoi day chao | sua: PanSpec theo hinh that, vong lua 1,2 R, test lop dau >= 80% long chao | count 2
- L-016 | trigger: nhu tren | goc: PanSpec (sprint 1) dat ban kinh 0,17 m va noi 1,83 m tu uoc luong, khong lay tu BuildTool | quy tac: suy kich thuoc tu hinh that | count 1
- L-017 | trigger: gia lap PlayMode fail "Dish_2 bi Dish_1 che" va "khong thu duoc sai mon" | goc: luat moi (thung 3+2, khach vao sau 3 giay) doi trang thai ma kich ban gia lap gia dinh | quy tac: doi so lieu thi chay lai PlayMode ngay, kich ban gia lap tu xoay xo (quay them thung, ngam cho khac) nhu nguoi that | count 1

### 2026-11-16 Sprint 3 (F-020..F-022)
- L-002 | trigger: (a) "cat > ../scratch_edit2.py" thua truoc heredoc lam lenh treo 120 giay, tao file rong o d:/duanlinhtinh (da xoa); (b) heredoc chua chuoi ''' long nhau loi "unexpected EOF" | sua: ghi script bang Write | count 3
- L-015 | trigger: viet Relics.cs, gacha, DishFormula truoc khi viet RelicShopTests/RelicEffectTests | sai: chi thay do 35/36 vi importer chua nhap relics.json, khong phai do tung hieu ung | quy tac: viet test truoc, chay do, roi moi viet Core | count 2
- L-018 | trigger: git status sau PlayMode co 57 anh docs/sprint-2/ui bi ghi de | goc: test PlayMode cu chup anh vao thu muc sprint cu; anh do dang cho nguoi dung duyet | sua: git checkout -- docs/sprint-2/ui | quy tac: sau moi lan chay PlayMode, kiem git status docs/sprint-*/ui cua sprint cu va khoi phuc neu khong co chu y doi | count 1
- L-019 | trigger: AC5_TiLeHienTrenBang do vi 0,2125 vs 0,21249999851 | goc: so tu JSON la float (0,15f), test so double voi sai so 1e-9 | quy tac: so lieu tu Balance la float: test dung sai so 1e-6 | count 1
- Anh chup (L-005) bat duoc 3 loi du test xanh: tieu de bang ti le bi thu chu khi them dong relic, vien vang ruong che nhan, chu the relic nho


### 2026-11-16 Sprint 4 (F-030..F-036)
- L-020 | trigger: bot C# toi ngay 9 chi qua no 2 47% (mo phong 79%) | sai: bot "phuc vu theo thu tu" dong quan ngay khi mon khach dau khong nau duoc; voi 3 khach goi cung luc tu mot kho, viec nay xay ra hang ngay | goc: luat moi (nhieu khach) lam doi hanh vi hop ly cua nguoi choi ma bot cu khong mo hinh | sua: mon khach dau het thi phuc vu khach ke tiep lam duoc -> 84% | quy tac: truoc khi ket luan "can bang lech", kiem bot co lam dieu nguoi that se lam voi luat moi khong (dem so lan dong quan som / bo qua) | count 1
- L-021 | trigger: PlayMode "nhin Sauce trung BookPrev" | goc: dat quay vat pham o goc phong ma khong tinh do vat cua goc ben canh (Goc 3 xoay 180 do) | quy tac: dat do vat moi gan goc phong: doi toa do sang khung the gioi (yaw 90/180/270) va kiem do vat cua goc ke ben truoc khi chot | count 1
- L-005 | trigger: anh goc2_quay_vat_pham: chu bang qua nho doc khong duoc; anh goc1 dung qua sat thi bong bong bi cat | count 3
- L-002 | trigger: python doc heredoc co tieng Viet qua stdin (cp1258) -> chuoi khong khop | sua: ghi .py bang Write, chay voi PYTHONUTF8=1 | count 4
