# Bot C# tren luat that — 4000 van moi kieu (55 giay)

```
naive  win=0.00 progress=0.75 pass(d4,d9,d15,d22)=[1.0, 0.96, 0.25, 0.0] combo3+=0.09 win|combo=0.01
        khach/ngay=7.9 toi ngay 21=998 thuong chu no=771 (77%) | chu no: khong duoc phuc vu=156, bac mon dua [0..4]=[0,6,10,55,771], dau ngay 21 co dia Tuyet hao trong tu=460 (trong do duoc thuong 360) | diem de tinh mat vi cho=8.9 tien phat sai mon / van=1 mon Hy Vong=0 dong quan som=44665 dua dia de trong ghe cho chu no=141 | khach bi giam de tinh / ngay (vong 2-4)=5.1 nau san / ngay=1.0 so che san / ngay=0.0
        tien truoc thu no 4: [957, 1019, 1097, 1257, 1423, n=4000] · 9: [3011, 3238, 3546, 4071, 4568, n=3994] · 15: [6453, 6972, 7742, 9187, 10943, n=3847] · 22: [16070, 17474, 21194, 29187, 31606, n=998]
        dau ngay (trung vi) ngay 9: 1193G [F3,F7,F6,S3] (n=3847) · ngay 15: 1032G [F7,F6,S2,F5,S4,C4,F2,V4] (n=998) · ngay 21: 15491G [C4,F6,F5,S3,S4,F7,F9,C1] (n=998)
nocombo win=0.37 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.37] combo3+=0.01 win|combo=0.62
        khach/ngay=10.5 toi ngay 21=3994 thuong chu no=3924 (98%) | chu no: khong duoc phuc vu=50, bac mon dua [0..4]=[0,0,2,18,3924], dau ngay 21 co dia Tuyet hao trong tu=3918 (trong do duoc thuong 3896) | diem de tinh mat vi cho=2.2 tien phat sai mon / van=307 mon Hy Vong=924 dong quan som=81058 dua dia de trong ghe cho chu no=45292 | khach bi giam de tinh / ngay (vong 2-4)=1.8 nau san / ngay=0.0 so che san / ngay=27.0
        tien truoc thu no 4: [1341, 1409, 1497, 1677, 1877, n=4000] · 9: [4287, 4430, 4607, 4998, 5548, n=4000] · 15: [12585, 13158, 13749, 14560, 15283, n=4000] · 22: [31326, 32293, 33370, 35250, 36999, n=3994]
        dau ngay (trung vi) ngay 9: 2098G [F4,C1,C2,F1,F3,C3] (n=4000) · ngay 15: 4062G [F1,F10,F3,F9,F4,C3,F2,V2] (n=3994) · ngay 21: 20497G [F1,F3,F4,F10,F9,F2,V3,C2] (n=3994)
smart  win=0.85 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.85] combo3+=0.94 win|combo=0.86
        khach/ngay=10.6 toi ngay 21=3998 thuong chu no=3942 (99%) | chu no: khong duoc phuc vu=34, bac mon dua [0..4]=[0,0,5,17,3942], dau ngay 21 co dia Tuyet hao trong tu=3889 (trong do duoc thuong 3885) | diem de tinh mat vi cho=2.9 tien phat sai mon / van=302 mon Hy Vong=1012 dong quan som=80642 dua dia de trong ghe cho chu no=44421 | khach bi giam de tinh / ngay (vong 2-4)=2.6 nau san / ngay=0.0 so che san / ngay=27.4
        tien truoc thu no 4: [1355, 1424, 1508, 1688, 1883, n=4000] · 9: [4312, 4439, 4624, 5007, 5510, n=4000] · 15: [13470, 13912, 14384, 15161, 15935, n=4000] · 22: [34045, 35291, 36619, 39006, 41326, n=3998]
        dau ngay (trung vi) ngay 9: 2107G [F1,C3,V2,C2,F3,S4,S3] (n=4000) · ngay 15: 4661G [C2,C1,F4,F2,F1,F10,F3,V1] (n=3998) · ngay 21: 23868G [F4,C2,C3,F2,F1,C1,C4,V3] (n=3998)
```

## So voi sim_balance.py (diem %, bot - mo phong)

| Kieu | Moc | Bot | Mo phong | Chenh |
| --- | --- | --- | --- | --- |
| naive | thang | 0.3 | 0 | +0.3 |
| naive | qua d4 | 99.9 | 100 | -0.1 |
| naive | qua d9 | 96.2 | 99 | -2.8 |
| naive | qua d15 | 25.0 | 33 | -8.1 **> 5** |
| naive | qua d22 | 0.3 | 0 | +0.3 |
| nocombo | thang | 36.5 | 83 | -46.5 **> 5** |
| nocombo | qua d4 | 100.0 | 100 | 0 |
| nocombo | qua d9 | 100.0 | 100 | 0 |
| nocombo | qua d15 | 99.9 | 100 | -0.1 |
| nocombo | qua d22 | 36.5 | 83 | -46.5 **> 5** |
| smart | thang | 85.0 | 98 | -13.0 **> 5** |
| smart | qua d4 | 100.0 | 100 | 0 |
| smart | qua d9 | 100.0 | 100 | 0 |
| smart | qua d15 | 100.0 | 100 | 0 |
| smart | qua d22 | 85.0 | 98 | -13.0 **> 5** |

