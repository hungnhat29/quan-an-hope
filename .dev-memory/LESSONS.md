# LESSONS

## Quy tac dang ap dung
- [L-014] Tinh nang co hien thi: viet "kich ban 10 giay" (dung dau, nhin gi, bam gi, thay gi) cho nguoi dung duyet TRUOC khi code; cho constitution im lang ve vong choi cot loi thi hoi, khong tu mac dinh (P2) [sprint 1: lam lai UI 2D->3D, them goi mon]
- [L-013] Spec co hinh phai co bang "muc Art Bible -> lam o dau" va moi thao tac co AC "nguoi choi thay gi" (hinh + chuyen dong) (P2/P6) [sprint 1: thieu mat, thieu animation]
- [L-015] Test moi phai chay thay DO mot lan truoc khi viet code, ke ca do vi chua bien dich (P4) [sprint 1: 8 test goi mon bo buoc nay]
- [L-011] Khong chay thu ban build voi -screen-fullscreen 0 / -screen-width: Unity luu vao registry, lan sau nguoi dung mo len thanh cua so (nguoi dung bao 14/11). Chay thu khong co co man hinh (P6)
- [L-012] Doi luat ghep mon / khach phai dua vao sim_balance.py va chay lai Dieu 1 truoc khi code; luat goi mon lam nguoi choi gioi tu 83% xuong 59% neu mo phong thieu 3 khach cung ngoi + ngoai le chu no (P2)
- [L-009] Tam anh quay ve camera dat truoc vat 3D: day ra bang ban kinh bao cua than (khong phai nua be day), test bang ban kinh tu localBounds (AABB cua khoi xoay phinh to) (P4)
- [L-010] Doc ket qua test Unity: grep result="[A-Za-z()]*" vi that bai ghi "Failed(Child)"; khong in dong ket qua = coi la DO (P6)
- [L-007] Nguoi dung muon game 3D that: moi thong tin dat tren do vat, HUD chi so lieu, khong nut chuyen goc (constitution v1.2 Dieu 5, 9). Tinh nang moi phai co do vat 3D de bam (P2/P3)
- [L-008] Do vat tao luc chay (BoxCollider) phai goi Physics.SyncTransforms; san canh khong co collider; test PlayMode tua nhanh can chia buoc di (P4)
- [L-005] UI moi phai chup anh tu PlayMode (docs/sprint-N/ui) va tu xem truoc khi bao xong: loi hien thi chi lo ra tren anh (P6) [count 2: sprint 2 do trong chao nho, lua khuat duoi day chao du test xanh]
- [L-006] Test PlayMode bam nut qua ten GameObject; doi ten nut trong GameView thi sua PlaythroughTests cung luc (P4)
- [L-002] Trong Bash tool cua may nay, heredoc nhieu dong co dau nhay don bi loi cu phap; ghi script .py bang Write vao scratchpad roi chay (moi pha) [count 2]
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
