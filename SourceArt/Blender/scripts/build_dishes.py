# -*- coding: utf-8 -*-
"""
Sprint 9 B-04 (yeu cau 22/11/2026 "mon an khi nau xong cung dang xau, toi muon cac mon an ro rang hon nua"):
model mon da nau cho 12 nguyen lieu chinh (SM_Mon_<id>), 10 do phu da nau (SM_Kem_<id>) va 4 de mon (SM_De_<ten>).

Chay:  "D:\\Blender\\blender.exe" -b --factory-startup -P SourceArt/Blender/scripts/build_dishes.py -- [ten ...]
Dung chung lop Model cua build_models.py (toa do viet theo Unity, goc day giua, vat lieu M_Toon_RRGGBB, <= 3 vat lieu).
Kich thuoc theo dia (long dia 0,28 m): mon chinh rong ~0,14-0,17 m, do phu ~0,06-0,08 m, de mon ~0,11-0,13 m.
Ngan sach Art Bible muc 14: mon an <= 1.500 tam giac. Mau mon la mau tu nhien, khong dung 6 mau bac (Art Bible muc 4).
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import build_models as bm  # noqa: E402
from build_models import Model, ring  # noqa: E402

G = "Dishes"


def arc_pts(n, r, a0, a1, y=0.0):
    return [ring(r, a0 + (a1 - a0) * i / max(1, n - 1), y) for i in range(n)]


# ================================================================ mon chinh da nau


def mon_trung():
    """Trung op la: long trang loang vien vang gion, long do vun cao o giua."""
    m = Model("SM_Mon_trung", G)
    m.cyl("#FFF8EC", (0.17, 0.012, 0.155), (0, 0.006, 0), seg=16, bevel=0.004)
    m.torus("#E3A23B", 0.078, 0.009, (0, 0.008, 0), seg=18, rseg=5, scale_y=0.6)
    m.sphere("#F6B93B", (0.07, 0.042, 0.068), (0.012, 0.022, 0.008), seg=12, rings=7)
    m.set_face(0.012, 0.035, -0.02)
    return m


def _slices(m, color, edge, n, w, d, t, r, y0, tilt, start=-50, span=100):
    for i in range(n):
        a = start + span * i / max(1, n - 1)
        x, _, z = ring(r, a - 90, 0)
        m.box(color, (w, t, d), (x, y0 + 0.006 * i, z), (tilt, a, 0), round_=0.3)
        if edge:
            ex, _, ez = ring(r + d * 0.45, a - 90, 0)
            m.box(edge, (w * 0.95, t * 0.9, d * 0.18), (ex, y0 + 0.006 * i + 0.001, ez), (tilt, a, 0), round_=0.3)


def mon_heo():
    """Heo xao: 5 lat thit to, mong, xep quat chong len nhau (de nhan la lat thit), tren lop sot nau."""
    m = Model("SM_Mon_heo", G)
    m.cyl("#9A4E2C", (0.17, 0.006, 0.15), (0, 0.003, 0), seg=14)
    for i in range(5):
        a = -50 + 25 * i
        x, _, z = ring(0.04, a - 90, 0)
        m.sphere("#C9825A", (0.085, 0.014, 0.06), (x, 0.012 + 0.006 * i, z * 0.8), (22, a, 0), seg=10, rings=5)
        m.sphere("#E7A57E", (0.055, 0.005, 0.036), (x, 0.019 + 0.006 * i, z * 0.8 - 0.004), (22, a, 0), seg=8, rings=4)
    m.set_face(0, 0.045, -0.02)
    return m


def mon_dau_phu():
    """Dau phu sot: 4 mieng dau vuong vang, mat chien vang, ngam trong vung sot."""
    m = Model("SM_Mon_dau_phu", G)
    m.cyl("#B0703F", (0.17, 0.01, 0.16), (0, 0.005, 0), seg=16, bevel=0.003)
    for i, (x, z, a) in enumerate([(-0.032, 0.025, 8), (0.035, 0.022, -12), (-0.028, -0.03, -5), (0.034, -0.028, 15)]):
        m.box("#F5E3B8", (0.052, 0.044, 0.052), (x, 0.03, z), (0, a, 0), round_=0.22)
        m.box("#E3A23B", (0.054, 0.012, 0.054), (x, 0.012, z), (0, a, 0), round_=0.3)
    m.set_face(0, 0.06, -0.03)
    return m


def mon_bo():
    """Bo xao: soi thit bo nau sam cuon nhe, xep chong."""
    m = Model("SM_Mon_bo", G)
    m.cyl("#7A3E24", (0.15, 0.006, 0.13), (0, 0.003, 0), seg=14)
    for i in range(6):
        a = i * 60 + 15
        x, _, z = ring(0.03 + 0.008 * (i % 2), a, 0)
        m.sphere("#8E4A33", (0.07, 0.016, 0.034), (x, 0.012 + 0.005 * (i % 3), z), (8, a, 12 * (1 if i % 2 else -1)), seg=10, rings=5)
    m.sphere("#A85A3E", (0.05, 0.018, 0.03), (0, 0.03, 0), (0, 40, 0), seg=10, rings=5)
    m.set_face(0, 0.045, -0.02)
    return m


def _shrimp(m, pos, yaw, s=1.0):
    """Mot con tom nam: than cong chu C nam ngang (ong tron), dau tron, duoi xoe."""
    tx, ty, tz = pos
    m.torus("#F08A5D", 0.026 * s, 0.012 * s, (tx, ty + 0.012 * s, tz), (0, yaw, 0), seg=10, rseg=6, arc=230)
    a = math.radians(-yaw)
    m.sphere("#F08A5D", (0.03 * s, 0.026 * s, 0.03 * s), (tx + math.cos(a) * 0.026 * s, ty + 0.013 * s, tz + math.sin(a) * 0.026 * s), seg=8, rings=5)
    b = math.radians(-yaw - 230)
    m.sphere("#E8604F", (0.03 * s, 0.008 * s, 0.022 * s), (tx + math.cos(b) * 0.03 * s, ty + 0.006, tz + math.sin(b) * 0.03 * s), (0, yaw + 230, 0), seg=8, rings=4)


def mon_tom():
    """Tom xao: 3 con tom cong cam do nam tren lop sot."""
    m = Model("SM_Mon_tom", G)
    m.cyl("#C8703F", (0.16, 0.006, 0.14), (0, 0.003, 0), seg=14)
    _shrimp(m, (-0.035, 0.006, 0.022), 20, 1.15)
    _shrimp(m, (0.038, 0.006, 0.018), 150, 1.15)
    _shrimp(m, (0.0, 0.006, -0.032), 260, 1.15)
    m.set_face(0, 0.05, -0.03)
    return m


def mon_ca_hoi():
    """Ca hoi ap chao: khuc phi le cam, mat ap vang, 3 van mo trang."""
    m = Model("SM_Mon_ca_hoi", G)
    m.box("#F49A6B", (0.15, 0.045, 0.08), (0, 0.0225, 0), (0, 8, 0), round_=0.35)
    m.box("#D9773F", (0.152, 0.01, 0.082), (0, 0.044, 0), (0, 8, 0), round_=0.4)
    for k in range(3):
        m.box("#FFE3D0", (0.008, 0.004, 0.07), (-0.04 + 0.04 * k, 0.05, 0), (0, 8 + 30, 0), round_=0.4)
    m.set_face(0, 0.05, -0.04)
    return m


def mon_muc():
    """Muc xao: vong muc trang nga + chum rau muc."""
    m = Model("SM_Mon_muc", G)
    m.cyl("#C98E5A", (0.15, 0.006, 0.13), (0, 0.003, 0), seg=14)
    for i, (x, z) in enumerate([(-0.04, 0.02), (0.0, 0.035), (0.04, 0.015), (-0.02, -0.025), (0.03, -0.03)]):
        m.torus("#F3E9DC", 0.022, 0.009, (x, 0.014 + 0.004 * (i % 2), z), (12 * (i % 3 - 1), 0, 10), seg=12, rseg=5)
    for k in range(5):
        a = -60 + 30 * k
        x, _, z = ring(0.025, a - 90, 0)
        m.cyl("#E9B98F", (0.009, 0.05, 0.009), (x * 0.6, 0.03, z * 0.6 - 0.005), seg=6)
    m.set_face(0, 0.045, -0.03)
    return m


def mon_ga():
    """Ga nuong mat ong: dui ga to vang nau, xuong tron, 3 vet nuong."""
    m = Model("SM_Mon_ga", G)
    m.sphere("#C97A3A", (0.12, 0.07, 0.09), (-0.015, 0.035, 0), (0, 0, 0), seg=14, rings=8)
    m.cyl("#FFF3DC", (0.022, 0.07, 0.022), (0.06, 0.03, 0), seg=8, rot=(0, 0, 75))
    m.sphere("#FFF3DC", (0.034, 0.03, 0.034), (0.095, 0.03, 0), seg=8, rings=5)
    for k in range(3):
        m.box("#6B3A20", (0.006, 0.006, 0.07), (-0.045 + 0.03 * k, 0.068, 0), (0, 35, 0), round_=0.4)
    m.set_face(-0.015, 0.06, -0.04)
    return m


def mon_tom_hum():
    """Tom hum nuong bo toi: than 5 dot do lien nhau, hai cang lon gan sat dau, duoi xoe, bo toi vang tren lung."""
    m = Model("SM_Mon_tom_hum", G)
    for k in range(5):
        m.sphere("#D9453A", (0.05 - 0.004 * k, 0.042 - 0.004 * k, 0.058 - 0.005 * k), (-0.05 + 0.026 * k, 0.021, 0), seg=10, rings=6)
    m.sphere("#E8604F", (0.035, 0.012, 0.06), (0.085, 0.01, 0), seg=8, rings=4)
    for s_ in (-1, 1):
        m.sphere("#D9453A", (0.03, 0.022, 0.03), (-0.075, 0.02, s_ * 0.032), seg=8, rings=4)
        m.sphere("#D9453A", (0.065, 0.034, 0.04), (-0.105, 0.022, s_ * 0.055), (0, s_ * 25, 0), seg=10, rings=6)
    m.sphere("#F6D25A", (0.08, 0.012, 0.03), (-0.01, 0.043, 0), seg=10, rings=4)
    m.set_face(-0.03, 0.05, -0.04)
    return m


def mon_wagyu():
    """Bo Wagyu ap chao: 3 lat cat lo long hong, vien nau, van mo."""
    m = Model("SM_Mon_wagyu", G)
    for k in range(3):
        x = -0.045 + 0.045 * k
        m.box("#7A3B2A", (0.04, 0.05, 0.1), (x, 0.025, 0), (0, 0, 10), round_=0.3)
        m.box("#E58A86", (0.032, 0.04, 0.102), (x + 0.004, 0.026, 0), (0, 0, 10), round_=0.3)
        m.box("#F3D6C8", (0.004, 0.03, 0.104), (x + 0.004, 0.028, 0.01), (0, 0, 10), round_=0.4)
    m.set_face(0, 0.06, -0.05)
    return m


def mon_ca_ngu():
    """Ca ngu tataki: 5 lat do tham xep nghieng (sashimi), vien ap chao nau tren dinh."""
    m = Model("SM_Mon_ca_ngu", G)
    for k in range(5):
        z = -0.05 + 0.024 * k
        m.box("#B23A48", (0.1, 0.05, 0.012), (0, 0.022, z), (-50, 0, 0), round_=0.3)
        m.box("#6B4A3A", (0.102, 0.008, 0.014), (0, 0.04, z + 0.014), (-50, 0, 0), round_=0.3)
    m.set_face(0, 0.05, -0.06)
    return m


def mon_cua():
    """Cua hoang de hap: mai tron do cam + 6 cang nhieu dot."""
    m = Model("SM_Mon_cua", G)
    m.sphere("#E0603F", (0.09, 0.04, 0.075), (0, 0.02, 0), seg=12, rings=6)
    for s in (-1, 1):
        for k in range(3):
            a = -40 + 40 * k
            x0, _, z0 = ring(0.045, a if s > 0 else 180 - a, 0)
            x1, _, z1 = ring(0.085, (a if s > 0 else 180 - a) + 10 * s, 0)
            m.cyl("#E0603F", (0.014, 0.045, 0.014), ((x0 + x1) / 2, 0.012, (z0 + z1) / 2), seg=6,
                  rot=(90, -(a if s > 0 else 180 - a) + 90, 0))
            m.sphere("#FFF3DC", (0.014, 0.012, 0.014), (x1, 0.012, z1), seg=6, rings=4)
    m.set_face(0, 0.04, -0.03)
    return m


# ================================================================ do phu da nau (mieng nho, nhan ra duoc)


def kem_ca_chua():
    m = Model("SM_Kem_ca_chua", G)
    for i, a in enumerate((0, 120, 240)):
        x, _, z = ring(0.018, a, 0)
        m.sphere("#E2493B", (0.035, 0.026, 0.028), (x, 0.013, z), (0, a, 0), seg=10, rings=5,
                 deform=lambda v: v if v.z > -0.05 else v)
        m.sphere("#F6C23A", (0.02, 0.008, 0.012), (x, 0.024, z), (0, a, 0), seg=6, rings=3)
    m.sphere("#6FA855", (0.016, 0.008, 0.016), (0, 0.03, 0), seg=6, rings=3)
    return m


def kem_hanh_tay():
    m = Model("SM_Kem_hanh_tay", G)
    for i, (x, z) in enumerate([(-0.014, 0.0), (0.014, 0.008), (0.0, -0.016)]):
        m.torus("#F2E3C6", 0.018, 0.0055, (x, 0.006 + 0.005 * i, z), (10 * (i - 1), 0, 8), seg=12, rseg=5)
        m.torus("#C99AC2", 0.011, 0.004, (x, 0.007 + 0.005 * i, z), (10 * (i - 1), 0, 8), seg=10, rseg=4)
    return m


def kem_rau_muong():
    m = Model("SM_Kem_rau_muong", G)
    for k in range(4):
        a = 20 + 45 * k
        m.cyl("#9CCB7A", (0.008, 0.07, 0.008), (0, 0.008, 0), seg=6, rot=(90, a, 0))
        x, _, z = ring(0.03, -a, 0)
        m.sphere("#4F8A4C", (0.034, 0.008, 0.016), (x, 0.012, z), (0, a, 0), seg=8, rings=4)
    return m


def kem_khoai_tay():
    m = Model("SM_Kem_khoai_tay", G)
    for k in range(4):
        m.box("#F2C46B", (0.06, 0.014, 0.014), (-0.01 + 0.006 * k, 0.007 + 0.012 * (k % 2), -0.02 + 0.013 * k), (0, 20 * k - 30, 0), round_=0.3)
    return m


def kem_nam_rom():
    m = Model("SM_Kem_nam_rom", G)
    for i, (x, z) in enumerate([(-0.018, 0.005), (0.018, 0.0), (0.0, -0.02)]):
        m.cyl("#F3EEDF", (0.016, 0.022, 0.016), (x, 0.011, z), seg=8)
        m.sphere("#8C7A6B", (0.034, 0.024, 0.034), (x, 0.026, z), seg=10, rings=5)
    return m


def kem_bong_cai():
    m = Model("SM_Kem_bong_cai", G)
    for i, (x, z) in enumerate([(-0.018, 0.006), (0.018, 0.004), (0.0, -0.018)]):
        m.cyl("#9CCB7A", (0.014, 0.026, 0.014), (x, 0.013, z), seg=6)
        for k in range(3):
            dx, _, dz = ring(0.008, k * 120, 0)
            m.sphere("#4F8A4C", (0.022, 0.018, 0.022), (x + dx, 0.03, z + dz), seg=8, rings=4)
    return m


def kem_ot_chuong():
    m = Model("SM_Kem_ot_chuong", G)
    for i, (col, x, z, a) in enumerate((("#E2493B", -0.018, 0.01, 10), ("#F6C23A", 0.018, 0.006, 70), ("#6FA855", 0.0, -0.018, 140))):
        m.torus(col, 0.016, 0.006, (x, 0.006, z), (0, a, 0), seg=8, rseg=4, arc=150, scale_y=1.6)
    return m


def kem_bi_do():
    m = Model("SM_Kem_bi_do", G)
    for i, (x, z) in enumerate([(-0.016, 0.01), (0.016, 0.012), (0.0, -0.014), (0.02, -0.02)]):
        m.box("#F0A040", (0.026, 0.024, 0.026), (x, 0.012 + 0.006 * (i == 3), z), (0, 20 * i, 0), round_=0.28)
        m.box("#6FA855", (0.027, 0.006, 0.006), (x, 0.022 + 0.006 * (i == 3), z + 0.011), (0, 20 * i, 0), round_=0.3)
    return m


def kem_mang_tay():
    m = Model("SM_Kem_mang_tay", G)
    for k in range(4):
        z = -0.018 + 0.012 * k
        m.cyl("#6FA855", (0.01, 0.075, 0.01), (0, 0.006, z), seg=6, rot=(0, 0, 90))
        m.sphere("#4F8A4C", (0.018, 0.013, 0.013), (0.04, 0.006, z), seg=6, rings=4)
    return m


def kem_nam_dong_co():
    m = Model("SM_Kem_nam_dong_co", G)
    for i, (x, z) in enumerate([(-0.018, 0.005), (0.018, 0.002), (0.0, -0.02)]):
        m.sphere("#6B4A35", (0.036, 0.02, 0.036), (x, 0.012, z), seg=10, rings=5)
        m.box("#F3EEDF", (0.022, 0.003, 0.004), (x, 0.022, z), (0, 45, 0), round_=0.3)
        m.box("#F3EEDF", (0.022, 0.003, 0.004), (x, 0.022, z), (0, -45, 0), round_=0.3)
    return m


# ================================================================ de mon


def de_com():
    m = Model("SM_De_Com", G)
    m.sphere("#FFFDF6", (0.12, 0.07, 0.1), (0, 0.0, 0), seg=14, rings=8, deform=lambda v: v)
    for k in range(7):
        x, _, z = ring(0.035 + 0.012 * (k % 2), k * 51, 0)
        m.sphere("#F3EEDF", (0.012, 0.006, 0.008), (x, 0.028 - 0.004 * (k % 2), z * 0.85), (0, k * 40, 0), seg=6, rings=3)
    return m


def de_mi_y():
    m = Model("SM_De_MiY", G)
    for k in range(4):
        m.torus("#F2D27A", 0.04 - 0.008 * k, 0.006, (0, 0.006 + 0.008 * k, 0), (0, 25 * k, 0), seg=14, rseg=4)
    m.torus("#E9BE5C", 0.05, 0.006, (0, 0.006, 0), seg=16, rseg=4)
    return m


def de_bun():
    m = Model("SM_De_Bun", G)
    for k in range(4):
        m.torus("#FBF7EE", 0.042 - 0.008 * k, 0.005, (0, 0.005 + 0.007 * k, 0), (0, 30 * k, 0), seg=14, rseg=4)
    m.torus("#EDE6D8", 0.052, 0.005, (0, 0.005, 0), seg=16, rseg=4)
    return m


def de_banh_mi():
    m = Model("SM_De_BanhMi", G)
    m.sphere("#D9963A", (0.13, 0.05, 0.055), (0, 0.025, 0), (0, 20, 0), seg=14, rings=6)
    for k in range(3):
        m.box("#F7E3B5", (0.008, 0.012, 0.03), (-0.035 + 0.035 * k, 0.048, 0), (0, 20 + 35, 0), round_=0.4)
    return m


ALL = [mon_trung, mon_heo, mon_dau_phu, mon_bo, mon_tom, mon_ca_hoi, mon_muc, mon_ga, mon_tom_hum, mon_wagyu, mon_ca_ngu, mon_cua,
       kem_ca_chua, kem_hanh_tay, kem_rau_muong, kem_khoai_tay, kem_nam_rom, kem_bong_cai, kem_ot_chuong, kem_bi_do, kem_mang_tay,
       kem_nam_dong_co, de_com, de_mi_y, de_bun, de_banh_mi]


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    report = []
    for fn in ALL:
        m = fn()
        if argv and m.name not in argv:
            m.bm.free()
            continue
        tris = m.build()
        report.append((m.name, m.group, tris, len(m.mats)))
    path = os.path.join(bm.ROOT, "SourceArt", "Blender", "models", "_report_dishes.txt")
    with open(path, "w", encoding="utf-8") as f:
        for r in report:
            f.write("%s\t%s\t%d\t%d\n" % r)
    print("XONG %d model mon" % len(report))


if __name__ == "__main__":
    main()
