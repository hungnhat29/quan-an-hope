# Bot C# tren luat that — 4000 van moi kieu (39 giay)

```
naive  win=0.00 progress=0.63 pass(d4,d9,d15,d22)=[0.95, 0.78, 0.06, 0.0] combo3+=0.09 win|combo=0.01
        khach/ngay=6.8 toi ngay 21=254 thuong chu no=150 (59%) | chu no: khong duoc phuc vu=84, bac mon dua [0..4]=[3,5,0,12,150], dau ngay 21 co dia Tuyet hao trong tu=0 (trong do duoc thuong 0) | diem de tinh mat vi cho=10.5 tien phat sai mon / van=0 mon Hy Vong=0 dong quan som=37749 dua dia de trong ghe cho chu no=22
        tien truoc thu no 4: [744, 812, 895, 1049, 1214, n=4000] · 9: [2497, 2683, 2948, 3477, 4063, n=3783] · 15: [5312, 5805, 6400, 7794, 9443, n=3102] · 22: [13655, 14496, 15900, 25816, 28962, n=254]
        dau ngay (trung vi) ngay 9: 740G [S4,F2,V4,S3,F7] (n=3102) · ngay 15: 710G [F8,S2,C2,F6] (n=254) · ngay 21: 14004G [F6,S4,F5,S3,F9,V4,C4,F8] (n=254)
nocombo win=0.09 progress=0.98 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.94, 0.09] combo3+=0.01 win|combo=0.15
        khach/ngay=8.5 toi ngay 21=3758 thuong chu no=3703 (99%) | chu no: khong duoc phuc vu=39, bac mon dua [0..4]=[0,0,4,12,3703], dau ngay 21 co dia Tuyet hao trong tu=3699 (trong do duoc thuong 3683) | diem de tinh mat vi cho=3.6 tien phat sai mon / van=254 mon Hy Vong=363 dong quan som=73522 dua dia de trong ghe cho chu no=37819
        tien truoc thu no 4: [1235, 1303, 1397, 1596, 1830, n=4000] · 9: [4081, 4229, 4375, 4714, 5084, n=4000] · 15: [10374, 10877, 11518, 12788, 13964, n=4000] · 22: [25504, 26361, 27311, 29140, 30937, n=3758]
        dau ngay (trung vi) ngay 9: 1814G [F2,V3,C1,V1,C3,F3,S3] (n=4000) · ngay 15: 2401G [F10,F2,F3,F9,F4,F1,V1,F8] (n=3758) · ngay 21: 15057G [F1,F2,F10,F3,F9,C1,V4,F4] (n=3758)
smart  win=0.52 progress=0.99 pass(d4,d9,d15,d22)=[1.0, 1.0, 0.98, 0.52] combo3+=0.96 win|combo=0.53
        khach/ngay=8.6 toi ngay 21=3937 thuong chu no=3880 (99%) | chu no: khong duoc phuc vu=35, bac mon dua [0..4]=[0,0,2,20,3880], dau ngay 21 co dia Tuyet hao trong tu=3807 (trong do duoc thuong 3802) | diem de tinh mat vi cho=5.4 tien phat sai mon / van=253 mon Hy Vong=472 dong quan som=73640 dua dia de trong ghe cho chu no=37471
        tien truoc thu no 4: [1262, 1331, 1423, 1622, 1837, n=4000] · 9: [4130, 4256, 4424, 4749, 5121, n=4000] · 15: [11245, 11851, 12653, 13992, 14970, n=4000] · 22: [27172, 28374, 29793, 32229, 34628, n=3937]
        dau ngay (trung vi) ngay 9: 1849G [S1,F4,V1,C2,S2,F1,S3] (n=4000) · ngay 15: 3518G [V2,F4,F1,V1,V4,F2,V3,F10] (n=3937) · ngay 21: 17855G [S3,F2,S1,F10,S2,F4,F1,F3] (n=3937)
```

## So voi sim_balance.py (diem %, bot - mo phong)

| Kieu | Moc | Bot | Mo phong | Chenh |
| --- | --- | --- | --- | --- |
| naive | thang | 0.2 | 0 | +0.2 |
| naive | qua d4 | 94.6 | 96 | -1.4 |
| naive | qua d9 | 77.6 | 79 | -1.5 |
| naive | qua d15 | 6.4 | 4 | +2.4 |
| naive | qua d22 | 0.2 | 0 | +0.2 |
| nocombo | thang | 8.8 | 46 | -37.2 **> 5** |
| nocombo | qua d4 | 100.0 | 100 | 0 |
| nocombo | qua d9 | 100.0 | 100 | 0 |
| nocombo | qua d15 | 94.0 | 99 | -5.1 **> 5** |
| nocombo | qua d22 | 8.8 | 46 | -37.2 **> 5** |
| smart | thang | 51.8 | 89 | -37.2 **> 5** |
| smart | qua d4 | 100.0 | 100 | 0 |
| smart | qua d9 | 100.0 | 100 | 0 |
| smart | qua d15 | 98.4 | 100 | -1.6 |
| smart | qua d22 | 51.8 | 89 | -37.2 **> 5** |

