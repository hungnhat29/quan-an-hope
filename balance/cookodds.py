# -*- coding: utf-8 -*-
"""
Cong thuc ra bac mon (constitution 2.1 muc 5.3, yeu cau 23/11/2026): moi to hop nguyen lieu ra du 6 bac.

  S = 0,6 x bac chinh + 0,4 x bac phu                      (thang 0-5)
  b = S + 0,8 (+ relic "diem mon") (- 0,3 ngay mat dien)
  Huyen thoai (ngan rieng): L = 0,001% x 2000^(S/5)  -> toi da 2% o S = 5; L = min(5%, L + relic Huyen thoai)
  Nam bac con lai chia (100% - L) theo hinh dang mau chuan truot theo b:
    mau (chinh 1 + phu 1, b = 1,8, tam = bac 2): Lac vi 4,95 . Do 17,05 . Binh thuong 74,35 . Ngon 13,25 . Tuyet hao 9,15
    o mep: moi bac xa hon giam e^-3; b le: noi suy log giua hai bac lien ke; phan tran khoi thang don vao Lac vi / Tuyet hao.
  Moi bac toi thieu 0,001% (tru khi relic chan han bac do); luu tren luoi 100.000 ve, lam tron theo phan du lon nhat.

Chay:  PYTHONUTF8=1 python balance/cookodds.py      (in bang + kiem tra khop bang thiet ke + kiem tra "nguyen lieu tot hon
       khong bao gio lam giam xac suat ra bac k tro len")
So lieu doc tu balance/balance.json (dish.odds) - Dieu 2. Ban C# o Core/Cooking.cs (DishOdds) lam y het.
"""
import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
NAMES = ["Lạc vị", "Dở", "Bình thường", "Ngon", "Tuyệt hảo", "Huyền thoại"]


def load_rules(path=None):
    with open(path or os.path.join(HERE, "balance.json"), encoding="utf-8") as f:
        return json.load(f)["dish"]


def point_mass(shape, decay, k):
    """Khoi luong cua mau chuan o vi tri nguyen k (tuong doi bac tam): mep chia duoi hinh hoc ti so e^-decay."""
    r = math.exp(-decay)
    n = len(shape)                # 5 o, tam o giua (chi so 2)
    lo, hi = -(n // 2), n // 2     # -2..2
    if lo < k < hi:
        return shape[k - lo]
    if k <= lo:
        return shape[0] * (1 - r) * r ** (lo - k)
    return shape[-1] * (1 - r) * r ** (k - hi)


def value_at(shape, decay, x):
    k = math.floor(x)
    f = x - k
    a, b = point_mass(shape, decay, k), point_mass(shape, decay, k + 1)
    if f < 1e-12:
        return a
    return math.exp((1 - f) * math.log(a) + f * math.log(b))


def legend_chance(odds, s, legend_bonus=0.0):
    base = odds["legend_base"] * odds["legend_growth"] ** (s / 5.0)
    return min(odds["legend_cap"], base + legend_bonus)


def five_tiers(odds, b, tail=40):
    """Khoi luong (chua chuan hoa) cua 5 bac 0..4 voi diem b."""
    shape, decay = odds["shape"], odds["tail_decay"]
    center = b - odds["shape_b"] + odds["shape_tier"]
    out = []
    for t in range(5):
        x = t - center
        if t == 0:
            m = sum(value_at(shape, decay, x - j) for j in range(tail))
        elif t == 4:
            m = sum(value_at(shape, decay, x + j) for j in range(tail))
        else:
            m = value_at(shape, decay, x)
        out.append(m)
    return out


def chances(rules, main_tier, side_tier, dish_center=0.0, blackout=False, legend_bonus=0.0, ruin=0.0, floor=0):
    """6 xac suat (0..1, chua lam tron luoi) theo thu tu bac 0..5."""
    odds = rules["odds"]
    s = rules["w_chinh"] * main_tier + rules["w_phu"] * side_tier
    b = s + rules["offset"] + dish_center - (rules["blackout_penalty"] if blackout else 0.0)
    L = legend_chance(odds, s, legend_bonus)
    m = five_tiers(odds, b)
    total = sum(m)
    p = [v / total * (1 - L) for v in m] + [L]
    p = [max(v, odds["min_chance"]) for v in p]
    t = sum(p)
    p = [v / t for v in p]
    if ruin > 0:
        p = [v * (1 - ruin) for v in p]
        p[0] += ruin
    if floor > 0:
        for k in range(floor):
            p[floor] += p[k]
            p[k] = 0.0
    return p


def tickets(rules, p):
    """Lam tron len luoi ve nguyen (phan du lon nhat), moi bac con kha nang ra >= 1 ve."""
    grid = rules["odds"]["grid"]
    raw = [v * grid for v in p]
    base = [int(math.floor(v)) for v in raw]
    pinned = [p[i] > 0 and raw[i] < 1 for i in range(len(p))]
    for i in range(len(p)):
        if pinned[i]:
            base[i] = 1
    left = grid - sum(base)
    order = sorted(range(len(p)), key=lambda i: (raw[i] - math.floor(raw[i])) if p[i] > 0 and not pinned[i] else -1, reverse=True)
    i = 0
    while left > 0:
        base[order[i % len(order)]] += 1
        left -= 1
        i += 1
    while left < 0:      # do dat toi thieu 1 ve: tru vao bac lon nhat
        j = max(range(len(base)), key=lambda k: base[k])
        base[j] -= 1
        left += 1
    return base


def fmt(pct):
    """Hien thi (muc 5.3): >= 1% hai chu so thap phan, < 1% du chu so (vd 0,005%), khong bao gio hien 0% cho bac con ra duoc."""
    if pct >= 1.0:
        s = "%.2f" % pct
    else:
        s = ("%.3f" % pct).rstrip("0").rstrip(".")
        if s in ("0", "0.0", ""):
            s = "0.001"
    return s.replace(".", ",") + "%"


# Bang thiet ke (tin nhan 23/11/2026) de kiem tra; sai so cho phep do lam tron cua bang.
DESIGN = [
    ((0, 0), [18.53, 62.61, 11.16, 7.32, 0.38, 0.001]),
    ((0, 1), [12.81, 45.26, 28.98, 11.29, 1.66, 0.002]),
    ((1, 1), [4.17, 14.36, 62.61, 11.16, 7.71, 0.005]),
    ((2, 1), [0.90, 8.68, 33.89, 41.13, 15.38, 0.012]),
    ((2, 2), [0.21, 3.96, 14.36, 62.60, 18.86, 0.021]),
    ((3, 2), [0.045, 0.86, 8.68, 33.88, 56.49, 0.052]),
    ((3, 3), [0.01, 0.20, 3.96, 14.34, 81.40, 0.096]),
    ((4, 2), [0.007, 0.13, 2.57, 13.12, 84.04, 0.13]),
    ((4, 4), [0.001, 0.01, 0.20, 3.94, 95.41, 0.44]),
    ((5, 4), [0.001, 0.002, 0.042, 0.85, 98.02, 1.09]),
    ((5, 5), [0.001, 0.001, 0.01, 0.19, 97.80, 2.00]),
]


def check(rules):
    bad = 0
    for (m, s), want in DESIGN:
        p = chances(rules, m, s)
        got = [v * 100 for v in p]
        for k in range(6):
            tol = max(0.06, want[k] * 0.06)
            if abs(got[k] - want[k]) > tol:
                bad += 1
                print("  LECH chinh %d + phu %d, %s: tinh %.4f, bang %.4f" % (m, s, NAMES[k], got[k], want[k]))
    # Dominance: nguyen lieu tot hon khong lam giam P(bac >= k)
    worse = 0
    pairs = [(m, s) for m in range(6) for s in range(6)]
    for (m1, s1) in pairs:
        for (m2, s2) in pairs:
            if m2 < m1 or s2 < s1 or (m1, s1) == (m2, s2):
                continue
            a = chances(rules, m1, s1)
            b = chances(rules, m2, s2)
            for k in range(1, 6):
                if sum(b[k:]) + 1e-9 < sum(a[k:]):
                    worse += 1
    return bad, worse


def main():
    rules = load_rules(sys.argv[1] if len(sys.argv) > 1 else None)
    print("Nguyen lieu        b     " + " | ".join("%-11s" % n for n in NAMES))
    for m in range(6):
        for s in range(6):
            p = chances(rules, m, s)
            tk = tickets(rules, p)
            b = rules["w_chinh"] * m + rules["w_phu"] * s + rules["offset"]
            print("Chinh %d + phu %d   %.2f  " % (m, s, b) + " | ".join("%-11s" % fmt(t / rules["odds"]["grid"] * 100) for t in tk))
    bad, worse = check(rules)
    print("\nKhop bang thiet ke: %s (%d o lech)" % ("OK" if bad == 0 else "LECH", bad))
    print("Nguyen lieu tot hon khong lam giam P(>= bac k): %s (%d vi pham)" % ("OK" if worse == 0 else "VI PHAM", worse))
    return 0 if bad == 0 and worse == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
