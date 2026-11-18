# Bot C# tren luat that — 4000 van moi kieu (45 giay)

```
naive  win=0.01 progress=0.67 pass(d4,d9,d15,d22)=[0.96, 0.84, 0.11, 0.01] combo3+=0.09 win|combo=0.02
        khach/ngay=7.2 toi ngay 21=446 thuong chu no=282 (63%) | chu no: khong duoc phuc vu=116, bac mon dua [0..4]=[7,5,10,26,282], dau ngay 21 co dia Tuyet hao trong tu=0 (trong do duoc thuong 0) | diem de tinh mat vi cho=10.9 tien phat sai mon / van=0 mon Hy Vong=0 dong quan som=42442 dua dia de trong ghe cho chu no=0
        tien truoc thu no 4: [779, 847, 933, 1087, 1259, n=4000] · 9: [2580, 2805, 3087, 3654, 4224, n=3859] · 15: [5639, 6123, 6795, 8222, 9920, n=3360] · 22: [14595, 15335, 16908, 27367, 30322, n=446]
        dau ngay (trung vi) ngay 9: 878G [F7,F6,F2] (n=3360) · ngay 15: 788G [F9,F2,C4,F8,S3,F1] (n=446) · ngay 21: 14801G [S2,F7,S3,F9,C4,V4,F6] (n=446)
nocombo win=0.12 progress=0.99 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.95, 0.12] combo3+=0.01 win|combo=0.32
        khach/ngay=8.7 toi ngay 21=3819 thuong chu no=3781 (99%) | chu no: khong duoc phuc vu=7, bac mon dua [0..4]=[1,0,5,25,3781], dau ngay 21 co dia Tuyet hao trong tu=3749 (trong do duoc thuong 3742) | diem de tinh mat vi cho=3.7 tien phat sai mon / van=237 mon Hy Vong=430 dong quan som=75887 dua dia de trong ghe cho chu no=35485
        tien truoc thu no 4: [1272, 1354, 1446, 1648, 1883, n=4000] · 9: [4096, 4220, 4384, 4732, 5162, n=4000] · 15: [10577, 11094, 11704, 13003, 14107, n=4000] · 22: [25910, 26661, 27662, 29467, 31364, n=3819]
        dau ngay (trung vi) ngay 9: 1832G [F2,S3,F10,F8,S2,V1,F1,F3] (n=4000) · ngay 15: 2588G [F3,F2,F1,F10,C1,C3,S4,V1] (n=3819) · ngay 21: 15338G [F3,F10,F2,F1,F9,C2,F8,C1] (n=3819)
smart  win=0.60 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.99, 0.6] combo3+=0.96 win|combo=0.61
        khach/ngay=8.8 toi ngay 21=3950 thuong chu no=3937 (100%) | chu no: khong duoc phuc vu=0, bac mon dua [0..4]=[0,0,0,13,3937], dau ngay 21 co dia Tuyet hao trong tu=3845 (trong do duoc thuong 3845) | diem de tinh mat vi cho=5.9 tien phat sai mon / van=232 mon Hy Vong=550 dong quan som=76181 dua dia de trong ghe cho chu no=34790
        tien truoc thu no 4: [1312, 1385, 1473, 1678, 1900, n=4000] · 9: [4141, 4260, 4421, 4785, 5207, n=4000] · 15: [11315, 11950, 12726, 14160, 15093, n=4000] · 22: [27725, 28819, 30216, 32821, 35220, n=3950]
        dau ngay (trung vi) ngay 9: 1885G [V3,V4,C2,S1,F4,F3,F2,F1] (n=4000) · ngay 15: 3685G [S4,S1,F10,F9,F2,S3,S2,C3] (n=3950) · ngay 21: 18322G [S3,F2,F4,S2,S1,F10,S4,F1] (n=3950)
```

## So voi sim_balance.py (diem %, bot - mo phong)

| Kieu | Moc | Bot | Mo phong | Chenh |
| --- | --- | --- | --- | --- |
| naive | thang | 0.9 | 0 | +0.9 |
| naive | qua d4 | 96.5 | 96 | +0.5 |
| naive | qua d9 | 84.0 | 79 | +5.0 |
| naive | qua d15 | 11.2 | 4 | +7.2 **> 5** |
| naive | qua d22 | 0.9 | 0 | +0.9 |
| nocombo | thang | 12.2 | 46 | -33.9 **> 5** |
| nocombo | qua d4 | 100.0 | 100 | 0 |
| nocombo | qua d9 | 100.0 | 100 | 0 |
| nocombo | qua d15 | 95.5 | 99 | -3.5 |
| nocombo | qua d22 | 12.2 | 46 | -33.9 **> 5** |
| smart | thang | 59.6 | 89 | -29.4 **> 5** |
| smart | qua d4 | 100.0 | 100 | 0 |
| smart | qua d9 | 100.0 | 100 | 0 |
| smart | qua d15 | 98.8 | 100 | -1.3 |
| smart | qua d22 | 59.6 | 89 | -29.4 **> 5** |

