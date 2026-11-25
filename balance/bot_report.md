# Bot C# tren luat that — 4000 van moi kieu (95 giay)

```
naive  win=0.02 progress=0.90 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.67, 0.02] combo3+=0.08 win|combo=0.03
        khach/ngay=8.3 toi ngay 21=2695 thuong chu no=2270 (84%) | chu no: khong duoc phuc vu=184, bac mon dua [0..4]=[3,11,36,191,2270], dau ngay 21 co dia Tuyet hao trong tu=1197 (trong do duoc thuong 1049) | diem de tinh mat vi cho=2.8 tien phat sai mon / van=1 mon Hy Vong=0 dong quan som=40501 dua dia de trong ghe cho chu no=183 | khach bi giam de tinh / ngay (vong 2-4)=2.8 nau san / ngay=1.0 so che san / ngay=0.0
        tien truoc thu no 4: [1263, 1361, 1482, 1744, 2017, n=4000] · 9: [4179, 4394, 4673, 5287, 5946, n=3999] · 15: [7897, 8671, 9612, 11528, 13415, n=3999] · 22: [17114, 19922, 24609, 30137, 32823, n=2695]
        dau ngay (trung vi) ngay 9: 2387G [C2,V3,F7,F5,F9,S3] (n=3999) · ngay 15: 1989G [F6,C2,C4,V4,F7,S3] (n=2695) · ngay 21: 16364G [F6,S3,V4,C4,F9,F12,F7,F5] (n=2695)
nocombo win=0.32 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.32] combo3+=0.00 win|combo=0.27
        khach/ngay=10.9 toi ngay 21=4000 thuong chu no=3927 (98%) | chu no: khong duoc phuc vu=33, bac mon dua [0..4]=[2,1,8,29,3927], dau ngay 21 co dia Tuyet hao trong tu=3884 (trong do duoc thuong 3865) | diem de tinh mat vi cho=0.8 tien phat sai mon / van=291 mon Hy Vong=1533 dong quan som=74567 dua dia de trong ghe cho chu no=51705 | khach bi giam de tinh / ngay (vong 2-4)=0.8 nau san / ngay=0.0 so che san / ngay=29.0
        tien truoc thu no 4: [1815, 1934, 2096, 2404, 2755, n=4000] · 9: [5973, 6334, 6823, 7840, 8931, n=4000] · 15: [13779, 14059, 14394, 15030, 15775, n=4000] · 22: [31145, 32137, 33129, 34979, 36798, n=4000]
        dau ngay (trung vi) ngay 9: 4940G [F10,F1,F8,V3,C1,S2] (n=4000) · ngay 15: 4530G [F10,F4,F9,F3,F1,V4,S3,V3] (n=4000) · ngay 21: 20271G [F4,F10,F1,F3,F2,F9,V1,S1] (n=4000)
smart  win=0.91 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.91] combo3+=0.99 win|combo=0.91
        khach/ngay=11.1 toi ngay 21=4000 thuong chu no=3966 (99%) | chu no: khong duoc phuc vu=13, bac mon dua [0..4]=[0,0,4,17,3966], dau ngay 21 co dia Tuyet hao trong tu=3849 (trong do duoc thuong 3847) | diem de tinh mat vi cho=1.3 tien phat sai mon / van=274 mon Hy Vong=1736 dong quan som=72012 dua dia de trong ghe cho chu no=50123 | khach bi giam de tinh / ngay (vong 2-4)=1.4 nau san / ngay=0.0 so che san / ngay=30.2
        tien truoc thu no 4: [1830, 1944, 2108, 2424, 2770, n=4000] · 9: [6231, 6605, 7070, 8069, 9111, n=4000] · 15: [14318, 14648, 15039, 15881, 17217, n=4000] · 22: [34953, 36095, 37348, 39633, 41906, n=4000]
        dau ngay (trung vi) ngay 9: 5169G [V1,F4,F3,S2,F1,V3,V2,F2] (n=4000) · ngay 15: 5381G [V2,F10,F4,V3,V1,V4,F1,C1] (n=4000) · ngay 21: 24317G [V1,F1,V3,F4,V4,V2,F10,C1] (n=4000)
```

## So voi sim_balance.py (diem %, bot - mo phong)

| Kieu | Moc | Bot | Mo phong | Chenh |
| --- | --- | --- | --- | --- |
| naive | thang | 1.9 | 0 | +1.9 |
| naive | qua d4 | 100.0 | 99 | +1.0 |
| naive | qua d9 | 100.0 | 96 | +4.0 |
| naive | qua d15 | 67.4 | 12 | +55.4 **> 5** |
| naive | qua d22 | 1.9 | 0 | +1.9 |
| nocombo | thang | 31.6 | 35 | -3.5 |
| nocombo | qua d4 | 100.0 | 100 | 0 |
| nocombo | qua d9 | 100.0 | 100 | 0 |
| nocombo | qua d15 | 100.0 | 100 | 0 |
| nocombo | qua d22 | 31.6 | 35 | -3.5 |
| smart | thang | 90.7 | 96 | -5.3 **> 5** |
| smart | qua d4 | 100.0 | 100 | 0 |
| smart | qua d9 | 100.0 | 100 | 0 |
| smart | qua d15 | 100.0 | 100 | 0 |
| smart | qua d22 | 90.7 | 96 | -5.3 **> 5** |

