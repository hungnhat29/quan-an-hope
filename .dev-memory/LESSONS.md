# LESSONS

## Quy tac dang ap dung
- [L-014] Tinh nang co hien thi: viet "kich ban 10 giay" (dung dau, nhin gi, bam gi, thay gi) cho nguoi dung duyet TRUOC khi code; cho constitution im lang ve vong choi cot loi thi hoi, khong tu mac dinh (P2) [sprint 1: lam lai UI 2D->3D, them goi mon]
- [L-013] Spec co hinh phai co bang "muc Art Bible -> lam o dau" va moi thao tac co AC "nguoi choi thay gi" (hinh + chuyen dong) (P2/P6) [sprint 1: thieu mat, thieu animation]
- [L-015] Test moi phai chay thay DO mot lan truoc khi viet code, ke ca do vi chua bien dich (P4) [count 6: sprint 1, 3, 6, 8, 9; 24/11 viet het code C-01..C-11 roi moi viet Sprint11Tests - test chay lan dau sau code]
- [L-011] Khong chay thu ban build voi -screen-fullscreen 0 / -screen-width: Unity luu vao registry, lan sau nguoi dung mo len thanh cua so (nguoi dung bao 14/11). Chay thu khong co co man hinh (P6)
- [L-012] Doi luat ghep mon / khach phai dua vao sim_balance.py va chay lai Dieu 1 truoc khi code; luat goi mon lam nguoi choi gioi tu 83% xuong 59% neu mo phong thieu 3 khach cung ngoi + ngoai le chu no (P2)
- [L-010] Doc ket qua test Unity: grep result="[A-Za-z()]*" vi that bai ghi "Failed(Child)"; khong in dong ket qua = coi la DO (P6)
- [L-007] Nguoi dung muon game 3D that: moi thong tin dat tren do vat, HUD chi so lieu, khong nut chuyen goc (constitution v1.2 Dieu 5, 9). Tinh nang moi phai co do vat 3D de bam (P2/P3)
- [L-008] Do vat tao luc chay (BoxCollider) phai goi Physics.SyncTransforms; san canh khong co collider; test PlayMode tua nhanh can chia buoc di; doi ten nut / tram thi sua PlaythroughTests cung luc (L-006) (P4)
- [L-005] UI / hinh moi (ca bat/sua hau ky URP, L-003) phai chup anh tu PlayMode (docs/sprint-N/ui) va tu xem truoc khi bao xong: loi hien thi chi lo ra tren anh (P6); UiKit.Box dat pivot = anchor (L-045), font chi co UiKit.Glyphs (L-046) [count 8: sprint 2-7; 21/11 ong tay khach mau den du test xanh]
- [L-002] Trong Bash tool cua may nay, heredoc nhieu dong co dau nhay don bi loi cu phap; ghi script .py bang Write vao scratchpad roi chay; python doc stdin can PYTHONUTF8=1; KHONG dat `cat > file` truoc lenh co heredoc (cat doc stdin -> treo 120 s) (moi pha) [count 9: sprint 3-6, 9, 10, 24/11]
- [L-004] Chay Unity batchmode: dong Editor truoc; -testResults va -logFile dung duong dan tuyet doi kieu Windows (P4)
- [L-001] Chạy sim_balance.py phải đặt PYTHONUTF8=1 (Windows mặc định cp1258, lỗi đọc JSON tiếng Việt) (P4/P6)
- [L-036] Them bien / ham vao file C# lon (FaceArt, RoomView, BalanceImporter, Tutorial): grep ten (ca thuoc tinh lan bien cuc bo cua ham dang sua) truoc khi dat (P4) [count 5: sprint 7 x3; sprint 9 'Note' trung thuoc tinh Tutorial.Note, 'want' trung bien trong RefreshSeats]
- [L-016] (gop L-009) Hinh dat trong/tren do vat 3D, tam anh quay ve camera (day ra bang ban kinh bao): lay kich thuoc tu hinh that (Renderer.bounds), khong tu so uoc luong; test hinh hoc phai do theo hinh that (P4) [count 2: sprint 2 PanSpec; sprint 5 nap noi bi vanh che, mu chu no qua to]
- [L-020] Truoc khi ket luan "can bang lech": kiem bot co lam dieu nguoi that se lam voi luat moi khong; dem ly do that bai (dong quan som, khong phuc vu chu no) trong BotRun (P6) [count 3: sprint 4, 5, 7 (dong ho dung truoc khi mo: nau san 2 dia lam bot tut 52->27%, so che san moi dung)]
- [L-039] Doi tuong se bat / tat (SetActive) thi luu tham chieu luc dung; GetComponentInParent/Find bo qua doi tuong da an -> null (P4) [sprint 8: NRE o SetUp lam do ca 49 test PlayMode]
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


### 2026-11-16 Sprint 5 (F-040..F-045)
- L-002 | trigger: heredoc python co chuoi C# chua dau nhay don -> "unexpected EOF" (lan 5) | sua: ghi .py bang Write roi chay | count 5
- L-016 | trigger: nap noi hap dat theo so uoc luong (0,1 m tren tam) bi vanh noi che, test LidOn xanh nhung anh khong thay nap; mu chu no tinh tu khung bao ca khoi khach (tay, ang-ten) nen to gap doi dau | sua: lay dinh vanh tu Renderer.bounds cua noi that, mu theo kich thuoc tam mat; them test hinh hoc "vom nap cao hon vanh" | count 2
- L-022 | trigger: anh "chu no ngac nhien" khong co chu no | goc: phan ung chi 0,8 giay, test tua Time.timeScale 25 roi di bo truoc khi chup | quy tac: hieu ung ngan (mat phan ung, pop) chup ngay sau thao tac voi Time.timeScale = 1 | count 1
- L-023 | trigger: PlayMode bat "Mo quan! Khach dau tien da toi" khi chu no dang doi ngoai cua | goc: thong bao chung ghi de thong bao rieng cua su kien moi | quy tac: them su kien moi (chu no, ngay dac biet) thi ra soat moi cho dat Toast cung luc do (ToggleShop, OnClockEvent, NextDay) | count 1
- L-020 | trigger: bot gom bo chi 77% duoc thuong chu no | goc: luat moi (chu no can ghe trong) + bot tieu het ve tu sang | sua: ghi ly do "khong phuc vu chu no" vao BotRun, bot giu 1 ve toi 20:00 -> 100% | count 2


### 2026-11-18 Truoc sprint 6 + Sprint 6 (F-050..F-055)
- L-002 | trigger: heredoc python sua RoomViewRound2 co chuoi C# chua dau nhay don -> "unexpected EOF" (lan 6) | sua: ghi .py bang Write | count 6
- L-015 | trigger: viet GameSession.Eaters va SaveGame truoc EatingTests / Sprint6Tests | sai: test chay xanh ngay lan dau, khong thay do | count 3
- L-005 | trigger: anh menu_chinh cho thay camera menu nam sau rem cua (noren), nut "Xong" de dong cuoi Cai dat, dong huong dan cham o HUD trai; test deu xanh | count 5
- L-022 | trigger: PlayMode khach an 5 giay ket thuc truoc khi kiem / chup vi tua Time.timeScale 25 | quy tac: ve toc do that ngay sau thao tac tao hieu ung co thoi han | count 2
- L-024 | trigger: doi luat lam khach / ghe ban lau hon (khach an) | goc: thang cua bot rat nhay so khach / ngay (-2% khach -> -8 diem thang) | quy tac: luat moi dong vao ghe / thoi gian phai chay bot C# truoc khi chot so, thu 2 muc (vd 5 va 8 giay) de thay do nhay | count 1
- L-025 | trigger: Write voi duong dan tuong doi "..\..\Users\..." tao file o D:\Users | quy tac: duong dan scratchpad luon viet tuyet doi bat dau tu C:\Users | count 1
- PowerShell Start-Process -ArgumentList tach duong dan co dau cach ("dev game") -> Unity bao "Couldn't set project path"; goi Unity bang & voi tham so rieng

### 2026-11-19 Cot truyen nhieu lop + nhan vat SDF
- L-026 | trigger: Graphic tu viet (InkCanvas) tao bang AddComponent -> MissingComponentException CanvasRenderer trong RectMask2D.PerformClipping | goc: uGUI 2.x (Unity 6) Graphic khong con RequireComponent(CanvasRenderer) | quy tac: them CanvasRenderer truoc khi AddComponent Graphic tu viet | count 2 (20/11 PolygonGraphic lam mat na khung)
- L-027 | trigger: chu tuong thanh bien mat | goc: UiKit.Label tra ve TMP la con stretch cua hop; doi pivot/anchoredPosition cua TMP thay vi cua hop | quy tac: muon dat vi tri nhan thi sua RectTransform cha (hop) | count 1
- L-005 | trigger: anh khung truyen cho thay chan bat cheo khi nhan vat quay trai (hong khong nhan facing), cua vom thanh hinh tron, chu de len mat; test deu xanh | count 7

### 2026-11-20 Cot truyen tu zip (SVG -> lop PNG -> Unity)
- L-028 | trigger: anh lop xuat tu Edge co nhieu hat mo khap trang, 46 MB / trang | goc: Chromium van ve bo loc feTurbulence cua phan tu visibility:hidden | quy tac: an phan tu SVG khi chup bang display:none; bo nhieu khoi anh PNG (phu trong engine) | count 1
- L-029 | trigger: khung truyen trong tron (chi thay goc toi) | goc: doi pivot RectTransform (giu vi tri) roi gan anchoredPosition = offset, xoa mat phan bu pivot | quy tac: luu vi tri goc sau khi doi pivot, moi khung gan goc + offset | count 1
- L-030 | trigger: chu tuong thanh TMP khong co vien du dat outlineWidth | goc: shader SDF can tu khoa OUTLINE_ON khi dat bang code | quy tac: fontMaterial.EnableKeyword("OUTLINE_ON") truoc khi dat outlineWidth / outlineColor | count 1
- L-022 | trigger: F050_F053 kiem khach dang an sau Frames(2) o toc do 25x -> khach an xong truoc khi kiem (chap chon theo may) | count 3
- L-031 | trigger: test do vi tri khach sau "yield return null" khong thay cu nhay cua Lively | goc: coroutine chay sau Update, truoc LateUpdate; khoi khach con Bob (canh goc) ghi vi tri trong Update | quy tac: thanh phan chuyen dong moi phai tat / thay thanh phan cu cung ghi transform (grep localPosition tren prefab), test doc gia tri cua khung truoc | count 1
- L-032 | trigger: Random.Range trong khoi tao truong MonoBehaviour -> UnityException moi PlayMode | quy tac: ngau nhien trong Awake / lan dau Update | count 1

### 2026-11-21 Truoc sprint 7 + Sprint 7 (F-060..F-063)
- L-002 | trigger: heredoc python chua chuoi C# co dau nhay -> "unexpected EOF" (lan 7) | sua: viet sub.py (spec @@@/<<</===/>>>) bang Write, dung lai moi lan sua | count 7
- L-020 | trigger: bot gom bo 52% -> 27% sau luat "dong ho chay tu luc mo quan" | sai: bot nau san 2 dia truoc khi mo; khach goi mon lam duoc ngau nhien nen dia nam chiem quay cho, bi dem "dua sai mon de trong ghe" | goc: chien thuat bot khong theo hanh vi hop ly voi luat moi | sua: nguoi tot so che san moi nguyen lieu (khong nau san), naive nau 1 mon nhu huong dan -> 73-78% | count 3
- L-033 | trigger: tay khach cam dua lam 2 khop (vai-khuyu-tay) | sai: Art Bible muc 6/12 "ong cao su, khong khuyu, cong theo duong cung" | sua: ong Bezier dung lai moi khung, vien muc nhu ong goc | count 1
- L-034 | trigger: playtest-log.csv 627 dong, ~90% la test PlayMode ghi lan | goc: test PlayMode chay o Editor, persistentDataPath trung voi ban nguoi choi | sua: GameController.PlaytestFolder (test ghi thu muc tam), cot "phien"; playtest_overhead.py loc buoi lap / nhay ngay / deu tang tap | quy tac: cong cu do cua nguoi that phai tach khoi du lieu do test sinh ra ngay tu dau | count 1
- L-036 | trigger: 3 lan Unity bao loi bien dich: 'bt' (BalanceImporter), 'mood' (RoomViewRound2), 'Drop' (FaceArt) da ton tai | sua: doi ten | count 3
- Quet nhieu phuong an trong mot lan mo Unity (Sprint7BotTests.F060_QuetPhuongAn + balance/run_sweep.ps1) nhanh hon chay run_bots.ps1 tung lan: 12 phuong an x 3 kieu x 1.000 van ~ 3,5 phut
- L-005 | trigger: anh khach_an_2 cho thay ong tay mau den (test vi tri tay xanh) | goc: chieu tam giac nguoc -> thay mat trong + vo vien phu len | count 8
- L-037 | trigger: doan "cha lat guong" lam nguyen nhan, sua theo doan, chay lai van den | sai: sua theo gia thuyet chua kiem | quy tac: loi hinh: viet phep kiem do duoc (HoseFacesOutward) truoc, de no DO, roi moi sua | count 1
- L-022 | trigger: KhachAn_* chay rieng thi khach an xong ngay khung dau (khung giat x tua 25 > 5 giay an) | sua: Time.maximumDeltaTime 0,02 trong test, dat lai 1/3 o SetUp | count 4

### 2026-11-21 sprint 8
- L-039 | trigger: 49/49 PlayMode do o SetUp (NullReference RefreshOwnedItems) | sai: sauceTag.GetComponentInParent<Canvas>() sau khi canvas da SetActive(false) | goc: GetComponentInParent mac dinh bo qua doi tuong khong active | quy tac: luu tham chieu luc dung, hoac GetComponentInParent(true) | kiem: chay PlayMode 1 test truoc khi chay ca bo | count 1
- L-040 | trigger: F073 do thoi gian di vao 0,15 giay | sai: do thoi gian sau Shot() (Shot ton vai khung = vai giay game o toc do tua 25x) | goc: dong ho bat dau sai cho | quy tac: test do thoi luong ghi Time.time ngay sau thao tac, chup anh khong chen giua | count 1
- L-015 count 4: viet code loi (RelicShop, ArrivalGap) truoc test Sprint8Tests
- L-002 count 7: heredoc python dai trong Bash loi "unexpected EOF" -> ghi script bang Write
- L-036 count 5 (22/11 sprint 9): ham tinh Note() trung thuoc tinh Note cua Tutorial; bien 'want' trung bien da co trong RefreshSeats -> 2 luot Unity
- L-015 count 5 (22/11 sprint 9): Blender + CharacterRig + OutlineMesh viet truoc Sprint9Tests; 11/15 test xanh ngay lan dau (khong co bang chung do)
- L-041 | trigger: test doc anh mat FaceArt bang GetPixels -> 'not readable' | nguyen nhan: Texture2D.Apply(false, true) bo du lieu CPU | quy tac: test doc anh sinh luc chay qua RenderTexture (Graphics.Blit + ReadPixels) | count 1
- L-042 | trigger: shader dùng UNITY_MATRIX_P / _ScreenParams trong URP: viền không hiện ở ảnh test | nguyên nhân: ma trận chiếu built-in bị lật / không đúng khi vẽ ra render texture | quy tắc: shader CG trong URP chỉ dùng UNITY_MATRIX_VP, unity_MatrixV, _WorldSpaceCameraPos; luôn có test đo điểm ảnh thật (L-005) | count 1
- L-043 | trigger: sửa file .cs bằng PowerShell Get-Content -Raw / Set-Content | sai: tiếng Việt thành mã rác (đọc cp1258), test so chuỗi đỏ | quy tắc: sửa file bằng Python io.open(encoding='utf-8') hoặc Edit tool, không dùng Get-Content/Set-Content cho file UTF-8 | count 1
- L-044 | trigger: FBX nhân vật có xương (không bake transform) + Unity Bake Axis Conversion | sai: model quay ngược 180°, clip khoá node gốc nên xoay lúc nhập bị ghi đè | quy tắc: test hướng (Face ở -z) kể cả sau SampleAnimation; sửa bằng node chứa xoay ở runtime | count 1


### 2026-11-22/23 sprint 10
- L-002 count 8: heredoc bash chua dau nhay don + script python dai -> "unexpected EOF"; va `python - <<EOF` doc stdin bang cp1258 -> chuoi tieng Viet khong khop (assert sai). Quy tac: ghi script / patch bang Write roi chay; neu buoc dung stdin thi dat PYTHONUTF8=1
- L-043 count 2: PowerShell Set-Content -Encoding UTF8 (5.1) them BOM vao make_next.py -> phai go BOM bang Python
- L-045 | trigger: the hinh lech nua kich thuoc, tran khung | sai: dat vi tri theo tam trong khi UiKit.Box dat pivot = anchor | goc: khong doc ham tien ich truoc khi dung | quy tac: UiKit.Box / Panel / Label: vi tri la DIEM ANCHOR cua khung (TopLeft -> goc trai tren); kiem anh chup truoc khi bao xong (L-005) | count 1
- L-046 | trigger: log "character \u2460 / \u2713 was not found" -> o vuong tren UI | goc: font Baloo 2 khong co ①②③ ✓ ✗ | quy tac: chi dung ky tu co trong UiKit.Glyphs; ky hieu ve bang hinh (PicKit.Mark); sau PlayMode grep "was not found in the" = 0 | count 1
- L-047 | trigger: test PlayMode "Bi ket khi di toi" ngau nhien | goc: tua 25x, khung cham 50 ms = 1,3 giay game; nguong 4 giay = 3 khung | quy tac: kiem ket theo ca giay game lan so khung (>= 20 khung) | count 1
- L-048 | trigger: bot gom bo tu 87% xuong 71% sau khi bo 30% cong thuc + gia theo mon | goc: gacha ghep cap lam loai chinh cu ra nhieu, khach goi lech ve mon re; chi giu "trung binh deu" chua du | quy tac: doi luat ghep / gia thi chay bot C# truoc khi chot so, chinh mot he so (PRICE_X) va ghi tung lan quet vao make_next.py | count 1
- L-015 count 6 (sprint 10): Sprint10Tests viet truoc va do dung (bien dich); nhung Sprint10PlayTests viet sau code game (khong do truoc) -> lap lai
- L-049 | trigger: kiem bot trong build -nographics ra 1749 loi RenderTexture | goc: lam nong icon chup bang camera khi khong co GPU | quy tac: code ve / chup anh phai bo qua khi SystemInfo.graphicsDeviceType == Null | count 1
- [L-050] 2026-11-24 | trigger: test model F070/F080 do | sai: them 5 kieu khach (chu 3D extrude, 4 vat lieu) lam SK_Khach 32.198 tam giac, ngan sach Art Bible 6.000 | goc: khong tinh ngan sach truoc khi dung; chu Blender extrude + do phan giai cao rat nang | quy tac: chu tren model dung extrude 0, resolution_u 2; model nhieu kieu do theo kieu dang hien (ModelChecker.LookKey); dem vat lieu <= 3 moi mesh truoc khi xuat | kiem: Sprint8ModelTests.F070_AC3 | count 1
- [L-051] 2026-11-24 | trigger: 18 test EditMode do sau doi luat | sai: test cu khoa so luat cu (7 ve, thanh 2 o, 22 relic, "Mon Hy Vong") | goc: doi constitution ma chua grep test theo hang so / chuoi | quy tac: truoc khi doi mot so trong du lieu, grep Tests/ theo gia tri cu va ten truong, sua test cung luc | kiem: grep -rn "<gia tri cu>" Tests/ | count 1
- [L-052] 2026-11-24 | trigger: loi bien dich TabMenu (51 loi) lam hong ca lan dung canh + test | sai: chuoi C# co "\n" viet qua heredoc python bi doi thanh xuong dong that | goc: escape hai lop (bash + python) | quy tac: chuoi C# co \n thi sua bang file .py (Write) hoac Edit tool, khong qua heredoc; sau khi va file C# lon, grep '"$' tim chuoi bi cat | kiem: grep -n '"$' Game/*.cs | count 1
- [L-053] 2026-11-24 | trigger: D06 do (o dang dung khong ve lai) | sai: trang thai doi trong Update (NormalizeSlots) nhung khong goi Changed() nen HUD / tay khong ve lai | goc: view chi lam moi khi Changed | quy tac: moi trang thai nguoi choi nhin thay ma doi ngoai thao tac (Update, hen gio) phai goi Changed() khi doi | kiem: test PlayMode doc HUD sau 1-2 khung | count 1
- [L-054] 2026-11-24 | trigger: anh lòng máy gacha tụt | sai: model Blender keep_origin van keo diem thap nhat ve y = 0 | goc: Model.build luon tru min z | quy tac: model phu khong cham san thi dat goc host o day mesh (hoac chup anh kiem ngay sau khi thay khoi bang model) | kiem: anh PlayMode sau Swap | count 1
