# -*- coding: utf-8 -*-
"""
F-113 (specs/sprint-11/spec.md; specs/sprint-9/spec.md:82 "16 nguyen lieu con lai bang Blender (sprint 11)"):
16 nguyen lieu vong 2-4 + dot pha, moi loai 2 model (nguyen + da so che). Hinh, kich thuoc, mau lay tu ban dung bang code
(My project/Assets/QuanAnHope/Game/IngredientModels.cs, sprint 4-5, Art Bible muc 5) de nguoi choi thay cung mot do vat;
mau gop toi da 3 vat lieu / mesh, ngan sach <= 600 tam giac (Art Bible muc 14).

Kich thuoc nhu IngredientModels: sphere / box = kich thuoc ngoai; cyl = duong kinh x, CAO y; Cap (vien thuoc) = ellipsoid dai 2 * y.
Nap boi build_models.py (ALL += R2_R4).
"""
import math


def V(x, y, z):
    return (x, y, z)


def add(a, b):
    return (a[0] + b[0], a[1] + b[1], a[2] + b[2])


def ring(r, deg, y):
    a = math.radians(deg)
    return (math.cos(a) * r, y, math.sin(a) * r)


LEAF_DARK, CREAM, TAN = "#4F8A4C", "#FFF3DC", "#D69B68"
INK = "#2B2321"


def make(Model):
    """Tra ve danh sach ham dung model (Model la lop trong build_models.py)."""

    def cap(m, hexcode, size, pos, rot=(0, 0, 0), seg=10, rings=6):
        m.sphere(hexcode, (size[0], size[1] * 2, size[2]), pos, rot=rot, seg=seg, rings=rings)

    # ------------------------------------------------------------ vong 2
    def khoai_tay():
        m = Model("SM_NL_khoai_tay", "Ingredients")
        m.sphere(TAN, (0.074, 0.05, 0.056), (0, 0.026, 0), seg=12, rings=7)
        m.sphere(TAN, (0.036, 0.034, 0.034), (0.026, 0.03, 0.012), rot=(0, 20, 0), seg=8, rings=5)
        for p, s in (((0.02, 0.046, -0.018), 0.008), ((-0.018, 0.042, -0.02), 0.007), ((-0.026, 0.03, 0.014), 0.006)):
            m.sphere("#8C5A3A", (s, 0.004, s), p, seg=6, rings=3)
        for i in range(4):
            m.sphere("#B8834F", (0.005, 0.003, 0.004), ring(0.03, 40 + 85 * i, 0.022), rot=(0, 85 * i, 0), seg=6, rings=3)
        m.set_face(0, 0.026, 0)
        return m

    def khoai_tay_so_che():
        m = Model("SM_NL_khoai_tay_SoChe", "Ingredients")
        for i in range(5):
            m.box("#F3D58A", (0.05, 0.011, 0.011), ring(0.012, 72 * i, 0.007 + 0.004 * (i % 2)), rot=(0, 72 * i + 20, 0), round_=0.25, segs=1)
        m.set_face(0, 0.012, 0)
        return m

    def nam_rom():
        m = Model("SM_NL_nam_rom", "Ingredients")
        m.sphere("#8F8578", (0.05, 0.042, 0.05), (0, 0.046, 0), seg=12, rings=7)
        m.sphere("#6E655A", (0.03, 0.016, 0.03), (0, 0.062, 0), seg=8, rings=4)
        m.cyl("#EDE6D8", (0.034, 0.03, 0.034), (0, 0.016, 0), seg=10)
        m.sphere("#EDE6D8", (0.04, 0.016, 0.04), (0, 0.008, 0), seg=10, rings=5)
        m.cyl("#EDE6D8", (0.046, 0.004, 0.046), (0, 0.028, 0), seg=12)
        m.set_face(0, 0.04, 0)
        return m

    def nam_rom_so_che():
        m = Model("SM_NL_nam_rom_SoChe", "Ingredients")
        for x in (-0.018, 0.018):
            m.sphere("#8F8578", (0.022, 0.044, 0.048), (x, 0.026, 0), seg=10, rings=6)
            m.box("#F4EFE6", (0.002, 0.04, 0.04), (x * 0.55, 0.026, 0), round_=0)
        m.set_face(0, 0.026, 0)
        return m

    # ------------------------------------------------------------ vong 3
    def bong_cai():
        m = Model("SM_NL_bong_cai", "Ingredients")
        m.cyl("#A9CF8C", (0.026, 0.032, 0.026), (0, 0.016, 0), seg=8)
        m.cyl("#A9CF8C", (0.01, 0.026, 0.01), (-0.012, 0.03, 0.006), rot=(0, 0, 30), seg=6)
        m.cyl("#A9CF8C", (0.01, 0.026, 0.01), (0.012, 0.03, -0.006), rot=(0, 0, -30), seg=6)
        m.sphere("#5E9E5B", (0.04, 0.034, 0.04), (0, 0.056, 0), seg=8, rings=5)
        for i in range(6):
            m.sphere("#6FA855" if i % 2 == 0 else "#5E9E5B", (0.032, 0.028, 0.032), ring(0.026, 60 * i, 0.048 - 0.004 * (i % 2)), seg=7, rings=4)
        for i in range(3):
            m.sphere("#6FA855", (0.008, 0.004, 0.008), ring(0.012, 120 * i + 20, 0.07), seg=5, rings=3)
        m.sphere("#5E9E5B", (0.03, 0.006, 0.016), (0, 0.02, 0.024), rot=(-25, 0, 0), seg=6, rings=3)
        m.set_face(0, 0.05, 0)
        return m

    def bong_cai_so_che():
        m = Model("SM_NL_bong_cai_SoChe", "Ingredients")
        for i in range(3):
            m.sphere("#5E9E5B", (0.03, 0.024, 0.03), ring(0.02, 120 * i, 0.024), seg=8, rings=5)
            m.cyl("#A9CF8C", (0.012, 0.016, 0.012), ring(0.02, 120 * i, 0.008), seg=6)
        m.set_face(0, 0.02, 0)
        return m

    def ot_chuong():
        m = Model("SM_NL_ot_chuong", "Ingredients")
        for i in range(3):
            m.sphere("#D9382C", (0.04, 0.064, 0.04), ring(0.014, 120 * i, 0.036), seg=10, rings=6)
            m.sphere("#D9382C", (0.022, 0.016, 0.022), ring(0.016, 120 * i + 60, 0.008), seg=6, rings=4)
        m.sphere("#F07A6E", (0.01, 0.022, 0.006), (-0.018, 0.05, -0.022), seg=6, rings=3)
        m.cyl("#5E8F3E", (0.03, 0.006, 0.03), (0, 0.068, 0), seg=8)
        m.cyl("#5E8F3E", (0.009, 0.012, 0.009), (0.002, 0.078, 0), rot=(0, 0, -10), seg=6)
        m.cyl("#5E8F3E", (0.008, 0.01, 0.008), (0.008, 0.088, 0), rot=(0, 0, -45), seg=6)
        m.set_face(0, 0.036, 0)
        return m

    def ot_chuong_so_che():
        m = Model("SM_NL_ot_chuong_SoChe", "Ingredients")
        for i in range(5):
            m.box("#D9382C", (0.052, 0.008, 0.01), ring(0.012, 72 * i, 0.006), rot=(0, 72 * i + 15, 0), round_=0.3, segs=1)
        m.set_face(0, 0.01, 0)
        return m

    def ca_hoi():
        m = Model("SM_NL_ca_hoi", "Ingredients")
        m.box("#F08A6A", (0.064, 0.03, 0.05), (-0.008, 0.018, 0), round_=0.3)
        m.box("#F08A6A", (0.03, 0.022, 0.04), (0.035, 0.014, 0), rot=(0, 0, -6), round_=0.3)
        m.box("#9AA4A8", (0.088, 0.004, 0.05), (0, 0.002, 0), round_=0)
        m.box("#9AA4A8", (0.084, 0.026, 0.002), (0, 0.016, -0.0255), round_=0)
        for i in range(5):
            m.box("#FFE3D3", (0.004, 0.002, 0.048), (-0.03 + 0.016 * i, 0.0335 - (0.006 if i > 3 else 0), 0), rot=(0, 25, 0), round_=0)
        m.set_face("center")
        return m

    def ca_hoi_so_che():
        m = Model("SM_NL_ca_hoi_SoChe", "Ingredients")
        m.box("#F59A72", (0.066, 0.026, 0.04), (0, 0.014, 0), round_=0.3)
        for i in range(3):
            m.box("#FFE3D3", (0.004, 0.002, 0.04), (-0.02 + 0.02 * i, 0.0275, 0), rot=(0, 25, 0), round_=0)
        m.set_face("center")
        return m

    def muc():
        m = Model("SM_NL_muc", "Ingredients")
        cap(m, "#EFE7DA", (0.032, 0.036, 0.03), (0.012, 0.02, 0), rot=(0, 0, 90))
        m.box("#E3D6C6", (0.026, 0.004, 0.05), (0.042, 0.02, 0), rot=(0, 45, 0), round_=0.3, segs=1)
        m.sphere("#EFE7DA", (0.024, 0.022, 0.024), (-0.022, 0.018, 0), seg=10, rings=6)
        for z in (-0.012, 0.012):
            m.sphere(INK, (0.007, 0.007, 0.004), (-0.024, 0.024, z), seg=6, rings=3)
        m.sphere("#E3D6C6", (0.012, 0.002, 0.008), (0.012, 0.037, 0.004), seg=6, rings=3)
        for i in range(8):
            m.cyl("#E3D6C6", (0.004, 0.014, 0.004), (-0.044, 0.01, -0.014 + 0.004 * i), rot=(0, -12 + 3 * i, 90), seg=4)
        m.cyl("#E3D6C6", (0.003, 0.024, 0.003), (-0.054, 0.008, -0.008), rot=(0, 15, 90), seg=4)
        m.cyl("#E3D6C6", (0.003, 0.024, 0.003), (-0.054, 0.008, 0.008), rot=(0, -15, 90), seg=4)
        m.set_face("center")
        return m

    def muc_so_che():
        m = Model("SM_NL_muc_SoChe", "Ingredients")
        for i in range(3):
            p = ring(0.018, 120 * i, 0.005)
            m.cyl("#EFE7DA", (0.03, 0.01, 0.03), p, seg=12)
            m.cyl("#C9BCA8", (0.017, 0.011, 0.017), add(p, (0, 0.0005, 0)), seg=10)
        m.set_face("center")
        return m

    # ------------------------------------------------------------ vong 4
    def bi_do():
        m = Model("SM_NL_bi_do", "Ingredients")
        for i in range(6):
            m.sphere("#F08A6A" if i % 2 == 0 else "#E87C58", (0.046, 0.054, 0.04), ring(0.022, 60 * i, 0.03), rot=(0, -60 * i, 0), seg=8, rings=5)
        m.sphere("#F08A6A", (0.06, 0.05, 0.06), (0, 0.032, 0), seg=8, rings=5)
        m.cyl("#E87C58", (0.024, 0.004, 0.024), (0, 0.057, 0), seg=8)
        m.cyl(LEAF_DARK, (0.012, 0.018, 0.012), (0, 0.066, 0), rot=(0, 0, 8), seg=6)
        m.sphere(LEAF_DARK, (0.03, 0.005, 0.022), (0.02, 0.064, 0.008), rot=(0, 30, 12), seg=6, rings=3)
        m.cyl(LEAF_DARK, (0.003, 0.016, 0.003), (-0.014, 0.064, -0.004), rot=(0, 0, 60), seg=4)
        m.set_face(0, 0.032, 0)
        return m

    def bi_do_so_che():
        m = Model("SM_NL_bi_do_SoChe", "Ingredients")
        for i in range(4):
            m.box("#F2A65A", (0.025, 0.024, 0.025), (-0.016 if i < 2 else 0.016, 0.012, -0.016 if i % 2 == 0 else 0.016), rot=(0, 10 * i, 0), round_=0.3, segs=1)
        m.set_face(0, 0.012, 0)
        return m

    def mang_tay():
        m = Model("SM_NL_mang_tay", "Ingredients")
        for i in range(5):
            z = -0.016 + 0.008 * i
            y = 0.008 + (i % 2) * 0.006
            m.cyl("#7DB87A", (0.009, 0.1, 0.009), (-0.004, y, z), rot=(0, 0, 90), seg=6)  # than dai 10 cm: noi goc (-5,4 cm) toi ngon (5 cm)
            m.sphere("#5E8F3E", (0.02, 0.011, 0.011), (0.05, y, z), seg=6, rings=4)
            m.sphere("#5E8F3E", (0.008, 0.004, 0.006), (0.026, y + 0.004, z), seg=5, rings=3)
            m.cyl("#7DB87A", (0.0092, 0.004, 0.0092), (-0.054, y, z), rot=(0, 0, 90), seg=6)
        m.cyl("#C9574D", (0.02, 0.005, 0.046), (-0.014, 0.012, 0), rot=(0, 0, 90), seg=8)
        m.set_face(0, 0.012, 0)
        return m

    def mang_tay_so_che():
        m = Model("SM_NL_mang_tay_SoChe", "Ingredients")
        for i in range(6):
            m.cyl("#7DB87A", (0.01, 0.026, 0.01), ring(0.018, 60 * i, 0.006), rot=(0, 60 * i, 90), seg=6)
        m.set_face(0, 0.008, 0)
        return m

    def nam_dong_co():
        m = Model("SM_NL_nam_dong_co", "Ingredients")
        m.sphere("#8A4F3A", (0.066, 0.03, 0.066), (0, 0.036, 0), seg=12, rings=6)
        m.cyl("#8A4F3A", (0.062, 0.006, 0.062), (0, 0.03, 0), seg=12)
        m.cyl("#E8DCC3", (0.056, 0.003, 0.056), (0, 0.026, 0), seg=12)
        m.cyl("#D9C9A8", (0.018, 0.026, 0.018), (0, 0.012, 0), seg=8)
        m.sphere("#D9C9A8", (0.022, 0.006, 0.022), (0, 0.002, 0), seg=8, rings=4)
        for i in range(3):
            m.box("#D9C9A8", (0.034, 0.002, 0.004), (0, 0.0505, 0), rot=(0, 60 * i + 15, 0), round_=0)
        m.set_face(0, 0.034, 0)
        return m

    def nam_dong_co_so_che():
        m = Model("SM_NL_nam_dong_co_SoChe", "Ingredients")
        for i in range(4):
            p = (-0.018 + 0.012 * i, 0.004, -0.008 + 0.005 * i)
            m.sphere("#8A4F3A", (0.05, 0.006, 0.02), p, rot=(0, 15 * i, 0), seg=8, rings=4)
            m.sphere("#D9C9A8", (0.04, 0.006, 0.01), add(p, (0, 0.001, 0.002)), rot=(0, 15 * i, 0), seg=8, rings=4)
        m.set_face(0, 0.006, 0)
        return m

    def ga():
        m = Model("SM_NL_ga", "Ingredients")
        m.sphere("#F3CFA0", (0.062, 0.048, 0.052), (0.012, 0.026, 0), seg=12, rings=7)
        m.sphere("#F6DDB4", (0.04, 0.022, 0.036), (0.02, 0.038, -0.004), seg=10, rings=5)
        m.sphere("#FFF8EE", (0.014, 0.006, 0.01), (0.024, 0.046, -0.016), seg=6, rings=3)
        m.cyl("#FFF8EE", (0.012, 0.026, 0.012), (-0.032, 0.022, 0), rot=(0, 0, 90), seg=8)
        for z in (-0.006, 0.006):
            m.sphere("#FFF8EE", (0.016, 0.016, 0.016), (-0.054, 0.022, z), seg=8, rings=5)
        m.set_face("center")
        return m

    def ga_so_che():
        m = Model("SM_NL_ga_SoChe", "Ingredients")
        for i in range(3):
            m.sphere("#F3CFA0", (0.032, 0.026, 0.03), ring(0.018, 120 * i, 0.014), rot=(0, 30 * i, 0), seg=10, rings=6)
        m.set_face("center")
        return m

    def bo():
        m = Model("SM_NL_bo", "Ingredients")
        m.box("#B8443C", (0.078, 0.032, 0.056), (0, 0.017, 0), round_=0.25)
        m.box("#C9574D", (0.072, 0.002, 0.05), (0, 0.0335, 0), round_=0)
        for p, s, yaw in (((0.01, 0.035, 0.01), (0.05, 0.002, 0.004), 25), ((-0.012, 0.035, -0.01), (0.04, 0.002, 0.003), -15),
                          ((0.018, 0.035, -0.014), (0.022, 0.002, 0.003), 60)):
            m.box("#F3E2D5", s, p, rot=(0, yaw, 0), round_=0)
        m.box("#F3E2D5", (0.078, 0.03, 0.006), (0, 0.017, -0.03), round_=0.3, segs=1)
        m.box("#F3E2D5", (0.006, 0.022, 0.014), (0.041, 0.017, 0.012), round_=0.3, segs=1)
        m.set_face("center")
        return m

    def bo_so_che():
        m = Model("SM_NL_bo_SoChe", "Ingredients")
        for i in range(4):
            m.box("#B8443C", (0.062, 0.005, 0.022), (-0.012 + 0.008 * i, 0.004 + 0.0045 * i, -0.024 + 0.016 * i), rot=(10, 6 * i, 0), round_=0.4, segs=1)
        m.set_face("center")
        return m

    def tom():
        m = Model("SM_NL_tom", "Ingredients")
        for i in range(4):
            m.sphere("#F08A6A", (0.028, 0.026, 0.026), ring(0.026, 200 + 40 * i, 0.016), seg=8, rings=5)
        for i in range(2):
            m.cyl("#E8604F", (0.002, 0.03, 0.002), add(ring(0.026, 160, 0.03), (0.004, 0, -0.006 + 0.012 * i)), rot=(0, 0, 55 + 15 * i), seg=4)
        m.sphere(INK, (0.006, 0.006, 0.006), add(ring(0.026, 160, 0.03), (0, 0, -0.012)), seg=6, rings=3)
        m.sphere("#E8604F", (0.03, 0.028, 0.028), ring(0.026, 160, 0.018), seg=8, rings=5)
        m.sphere("#E8604F", (0.024, 0.006, 0.026), add(ring(0.026, 345, 0.014), (0.008, 0, 0.006)), rot=(0, 30, 0), seg=6, rings=3)
        m.cyl("#E8604F", (0.002, 0.04, 0.002), ring(0.04, 150, 0.02), rot=(0, 30, 80), seg=4)
        m.set_face("center")  # rau dai lam lech hop bao (PlayMode F012 AssertFaceOutside)
        return m

    def tom_so_che():
        m = Model("SM_NL_tom_SoChe", "Ingredients")
        for i in range(4):
            m.sphere("#F6B39A", (0.026, 0.022, 0.024), ring(0.024, 200 + 40 * i, 0.014), seg=8, rings=5)
        m.sphere("#E8604F", (0.02, 0.005, 0.022), add(ring(0.024, 345, 0.012), (0.008, 0, 0.006)), rot=(0, 30, 0), seg=6, rings=3)
        m.set_face("center")
        return m

    # ------------------------------------------------------------ dot pha
    def tom_hum():
        m = Model("SM_NL_tom_hum", "Ingredients")
        cap(m, "#D9382C", (0.04, 0.04, 0.036), (0, 0.022, 0), rot=(0, 0, 90), seg=12, rings=7)
        m.sphere("#C42E24", (0.032, 0.02, 0.022), (-0.05, 0.02, 0.03), rot=(0, 30, 0), seg=10, rings=6)
        m.sphere("#C42E24", (0.032, 0.02, 0.022), (-0.05, 0.02, -0.03), rot=(0, -30, 0), seg=10, rings=6)
        m.sphere("#C42E24", (0.02, 0.008, 0.034), (0.055, 0.016, 0), seg=8, rings=4)
        m.set_face("center")
        return m

    def tom_hum_so_che():
        m = Model("SM_NL_tom_hum_SoChe", "Ingredients")
        m.sphere("#D9382C", (0.08, 0.02, 0.036), (0, 0.012, 0), seg=12, rings=6)
        m.sphere(CREAM, (0.068, 0.014, 0.026), (0, 0.02, 0), seg=10, rings=5)
        m.set_face("center")
        return m

    def wagyu():
        m = Model("SM_NL_wagyu", "Ingredients")
        m.box("#E28B8B", (0.076, 0.032, 0.056), (0, 0.017, 0), round_=0.25)
        for i in range(4):
            m.box("#FFF3EC", (0.03, 0.002, 0.004), (-0.024 + 0.016 * i, 0.0335, 0.004 * (1 if i % 2 == 0 else -1)), rot=(0, 40 * i, 0), round_=0)
        m.set_face("center")
        return m

    def wagyu_so_che():
        m = Model("SM_NL_wagyu_SoChe", "Ingredients")
        for x in (-0.02, 0.02):
            m.box("#E28B8B", (0.034, 0.03, 0.05), (x, 0.016, 0), round_=0.25)
            m.box("#FFF3EC", (0.026, 0.002, 0.004), (x, 0.0315, 0.004), rot=(0, 30, 0), round_=0)
        m.set_face("center")
        return m

    def ca_ngu():
        m = Model("SM_NL_ca_ngu", "Ingredients")
        m.box("#C94A3B", (0.082, 0.034, 0.046), (0, 0.018, 0), round_=0.25)
        m.box("#9E3329", (0.082, 0.002, 0.012), (0, 0.0355, 0), round_=0)
        m.box("#E57A6C", (0.004, 0.002, 0.02), (0.012, 0.0355, 0.014), rot=(0, 30, 0), round_=0)
        m.set_face("center")
        return m

    def ca_ngu_so_che():
        m = Model("SM_NL_ca_ngu_SoChe", "Ingredients")
        for i in range(3):
            m.box("#C94A3B", (0.03, 0.012, 0.042), (-0.014 + 0.014 * i, 0.007 + 0.004 * i, 0), rot=(0, 0, -12), round_=0.25)
        m.set_face("center")
        return m

    def cua():
        m = Model("SM_NL_cua", "Ingredients")
        m.sphere("#E8604F", (0.072, 0.03, 0.056), (0, 0.022, 0), seg=12, rings=7)
        m.sphere("#D9382C", (0.03, 0.02, 0.02), (-0.03, 0.018, -0.036), rot=(0, -30, 0), seg=10, rings=6)
        m.sphere("#D9382C", (0.03, 0.02, 0.02), (0.03, 0.018, -0.036), rot=(0, 30, 0), seg=10, rings=6)
        for i in range(3):
            m.cyl("#D9382C", (0.004, 0.03, 0.004), (-0.04, 0.01, -0.008 + 0.014 * i), rot=(0, -15 + 15 * i, 70), seg=4)
            m.cyl("#D9382C", (0.004, 0.03, 0.004), (0.04, 0.01, -0.008 + 0.014 * i), rot=(0, 15 - 15 * i, -70), seg=4)
        m.set_face("center")
        return m

    def cua_so_che():
        m = Model("SM_NL_cua_SoChe", "Ingredients")
        m.sphere("#E8604F", (0.062, 0.026, 0.05), (0, 0.016, 0), seg=12, rings=6)
        m.sphere(CREAM, (0.046, 0.01, 0.036), (0, 0.027, 0), seg=10, rings=5)
        m.sphere("#D9382C", (0.05, 0.012, 0.04), (0.05, 0.012, 0.02), rot=(0, 0, 25), seg=10, rings=5)
        m.set_face("center")
        return m

    return [khoai_tay, khoai_tay_so_che, nam_rom, nam_rom_so_che, bong_cai, bong_cai_so_che, ot_chuong, ot_chuong_so_che,
            ca_hoi, ca_hoi_so_che, muc, muc_so_che, bi_do, bi_do_so_che, mang_tay, mang_tay_so_che, nam_dong_co, nam_dong_co_so_che,
            ga, ga_so_che, bo, bo_so_che, tom, tom_so_che, tom_hum, tom_hum_so_che, wagyu, wagyu_so_che, ca_ngu, ca_ngu_so_che,
            cua, cua_so_che]
