# Bot C# tren luat that — 4000 van moi kieu (61 giay)

```
naive  win=0.01 progress=0.80 pass(d4,d9,d15,d22)=[1.0, 0.99, 0.39, 0.01] combo3+=0.12 win|combo=0.02
        khach/ngay=8.1 toi ngay 21=1578 thuong chu no=1253 (79%) | chu no: khong duoc phuc vu=224, bac mon dua [0..4]=[2,4,10,85,1253], dau ngay 21 co dia Tuyet hao trong tu=715 (trong do duoc thuong 584) | diem de tinh mat vi cho=3.0 tien phat sai mon / van=1 mon Hy Vong=0 dong quan som=46190 dua dia de trong ghe cho chu no=181 | khach bi giam de tinh / ngay (vong 2-4)=2.6 nau san / ngay=1.0 so che san / ngay=0.0
        tien truoc thu no 4: [1010, 1076, 1151, 1303, 1460, n=4000] · 9: [3356, 3558, 3835, 4292, 4760, n=3996] · 15: [7258, 7831, 8524, 10000, 11634, n=3963] · 22: [16884, 18240, 23006, 29582, 32064, n=1578]
        dau ngay (trung vi) ngay 9: 1397G [F7,F5,F1] (n=3963) · ngay 15: 1138G [F4,F5,S4,F7,F6,F1,F8] (n=1578) · ngay 21: 15895G [S1,S4,F5,F6,S2,F7,V3] (n=1578)
nocombo win=0.34 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.34] combo3+=0.01 win|combo=0.52
        khach/ngay=10.5 toi ngay 21=3999 thuong chu no=3918 (98%) | chu no: khong duoc phuc vu=55, bac mon dua [0..4]=[1,1,2,22,3918], dau ngay 21 co dia Tuyet hao trong tu=3915 (trong do duoc thuong 3899) | diem de tinh mat vi cho=1.2 tien phat sai mon / van=306 mon Hy Vong=818 dong quan som=78984 dua dia de trong ghe cho chu no=45215 | khach bi giam de tinh / ngay (vong 2-4)=0.9 nau san / ngay=0.0 so che san / ngay=27.3
        tien truoc thu no 4: [1426, 1498, 1587, 1752, 1929, n=4000] · 9: [4279, 4417, 4589, 4956, 5440, n=4000] · 15: [12759, 13245, 13809, 14593, 15296, n=4000] · 22: [31306, 32320, 33320, 35153, 36982, n=3999]
        dau ngay (trung vi) ngay 9: 2056G [F4,V1,C1,F2,F10,S2,F8] (n=4000) · ngay 15: 4093G [F3,F1,F2,F4,V2,C3,C2,F9] (n=3999) · ngay 21: 20463G [F2,F4,F1,F3,F9,C3,V1,V4] (n=3999)
smart  win=0.87 progress=1.00 pass(d4,d9,d15,d22)=[1.0, 1.0, 1.0, 0.87] combo3+=0.96 win|combo=0.88
        khach/ngay=10.7 toi ngay 21=4000 thuong chu no=3936 (98%) | chu no: khong duoc phuc vu=36, bac mon dua [0..4]=[0,0,5,23,3936], dau ngay 21 co dia Tuyet hao trong tu=3873 (trong do duoc thuong 3868) | diem de tinh mat vi cho=1.7 tien phat sai mon / van=303 mon Hy Vong=1088 dong quan som=78704 dua dia de trong ghe cho chu no=44501 | khach bi giam de tinh / ngay (vong 2-4)=1.3 nau san / ngay=0.0 so che san / ngay=27.9
        tien truoc thu no 4: [1469, 1541, 1625, 1795, 1975, n=4000] · 9: [4354, 4483, 4655, 5047, 5582, n=4000] · 15: [13724, 14120, 14518, 15253, 16012, n=4000] · 22: [34274, 35536, 36912, 39154, 41382, n=4000]
        dau ngay (trung vi) ngay 9: 2147G [C1,F10,V1,C3,F3,F9,S4] (n=4000) · ngay 15: 4753G [S1,S2,F2,F9,F4,F1,C2,C3] (n=4000) · ngay 21: 23994G [C2,F1,F4,C3,F2,C1,C4,F10] (n=4000)
```

## So voi sim_balance.py (diem %, bot - mo phong)

| Kieu | Moc | Bot | Mo phong | Chenh |
| --- | --- | --- | --- | --- |
| naive | thang | 0.5 | 0 | +0.5 |
| naive | qua d4 | 99.9 | 100 | -0.1 |
| naive | qua d9 | 99.1 | 99 | +0.1 |
| naive | qua d15 | 39.5 | 33 | +6.5 **> 5** |
| naive | qua d22 | 0.5 | 0 | +0.5 |
| nocombo | thang | 34.1 | 83 | -48.9 **> 5** |
| nocombo | qua d4 | 100.0 | 100 | 0 |
| nocombo | qua d9 | 100.0 | 100 | 0 |
| nocombo | qua d15 | 100.0 | 100 | 0 |
| nocombo | qua d22 | 34.1 | 83 | -48.9 **> 5** |
| smart | thang | 87.4 | 98 | -10.6 **> 5** |
| smart | qua d4 | 100.0 | 100 | 0 |
| smart | qua d9 | 100.0 | 100 | 0 |
| smart | qua d15 | 100.0 | 100 | 0 |
| smart | qua d22 | 87.4 | 98 | -10.6 **> 5** |

