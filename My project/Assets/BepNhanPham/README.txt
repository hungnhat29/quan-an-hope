BEP NHAN PHAM - NHA HANG 4 GOC

Cach dung:
1. Import goi nay vao du an Unity (Assets > Import Package > Custom Package).
2. Tren thanh menu chon: Bep Nhan Pham > Dung nha hang (4 goc).
3. Script se tao scene Assets/BepNhanPham/Scenes/NhaHang.unity va prefab Assets/BepNhanPham/Prefabs/NhaHang.prefab.
4. De nut bam nhan duoc click: GameObject > UI > Event System.

Thu muc:
- Textures/  : anh nen pho, hoa van tuong, san, 3 bieu cam mat, trai tim, bien hieu, anh UI.
- Shaders/   : ToonFlat (mau 2 nac), Outline (vien den), UnlitAlpha (anh trong suot).
- Scripts/   : Bob (nhun), FaceSwap (doi bieu cam: chuot phai vao component de thu),
               CameraRig (xoay giua 4 goc: Next / Prev / GoTo, tu bat tat UI tung goc).
- Editor/    : BuildNhaHang (script dung canh; sua toa do/mau trong file roi chay lai menu).
- Generated/ : material va mesh do script tao ra (bi xoa va tao lai moi lan dung).
- Preview/   : anh ket qua mong doi cua 4 goc.

Doi mau: sua ma mau trong BuildNhaHang.cs roi dung lai, hoac sua truc tiep material trong Generated/.
Chu tren UI la anh PNG tam thoi; khi lam that nen thay bang TextMeshPro.

Cac goc: Goc1 giao mon (cua ra vao + rem vai), Goc2 may tha thung nguyen lieu + may nang pham mon + bang ti le,
Goc3 sach nau an + 3 relic moi ngay (toi da 10), Goc4 bep nau.
Neu truoc do da import ban Goc 1 cu: xoa thu muc Assets/BepNhanPham truoc khi import ban nay.
