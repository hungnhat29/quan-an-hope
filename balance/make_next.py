"""Sinh bo so constitution 1.4 vao balance/next/ tu bo so 1.3 (balance/*.json).
Chay: PYTHONUTF8=1 python balance/make_next.py
Nguon: bo so 1.3 o balance/v1.3/ (giu nguyen de chay lai khong nhan he so hai lan).
Tu sprint 2 (15/11/2026) game doc balance/*.json = ban chep cua balance/next/*: sua o day, chay lai, roi chep next/* de len balance/.
Moi thay doi 1.4 nam o day de doc mot cho (yeu cau nguoi dung 14/11/2026)."""
import json, os, copy
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, 'next')
os.makedirs(OUT, exist_ok=True)
SRC = os.path.join(HERE, 'v1.3')
load = lambda n: json.load(open(os.path.join(SRC, n), encoding='utf-8'))
def save(n, o):
    json.dump(o, open(os.path.join(OUT, n), 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=1)

# ---- he so thoi gian: "tang thoi gian chuan bi va thoi gian nau"
PREP_X = 3.0     # so che: 0,5-3 giay -> 1,5-9 giay
COOK_X = 1.5     # nau: 8-20 giay -> 12-30 giay

# ---- no: "tang so tien no" (chon bang mo phong, xem constitution 1.4 muc 2 va muc 9)
DEBT = {"4": 750, "9": 2900, "15": 10500, "22": 32000}
BOSS_BONUS = 11500

B = load('balance.json')
B['_note'] = 'Constitution 1.4. Sinh tu make_next.py; sua o make_next.py roi chay lai, sau do: V=next python sim_balance.py'
B['debt_days'] = DEBT
B['boss_bonus'] = BOSS_BONUS
# "nguyen lieu phu 3, nguyen lieu chinh 2, dot pha 1"; ngay 3 (sau mua) giam theo: 2 phu + 1 chinh
B['crate_size'] = {"phu": 3, "chinh": 2, "phu_storm": 2, "chinh_storm": 1}
B['breakthrough']['items'] = 1
# "khong gioi han khach ... den het 21h": bo so khach moi ngay, thay bang do dai ngay
del B['customers']['per_day']
# Ngay dai hon de kip phuc vu nhieu khach hon khi so che / nau lau hon: 1 gio game = 40 giay (truoc 30) -> 8 phut / ngay
# next_customer_s: vong 1 khach ke tiep vao 3 giay sau khi khach truoc duoc phuc vu (constitution 1.4 muc 6.2)
B['day'] = {"open_hour": 9, "close_hour": 21, "seconds_per_hour": 40, "boss_hour": 20, "boss_day": 21, "next_customer_s": 3, "eat_s": 5}
# eat_s (constitution 1.5 muc 6.2): khach an xong mon moi roi ghe; ghe trong tu luc do, khach moi vao sau next_customer_s
# boss_day (sprint 5): constitution muc 2 "Ngay 21, dung 20:00 chu no den"
B['day_seconds'] = (21 - 9) * 40
# Thung nho lai (2 chinh) nen can nhieu ve hon de du nguyen lieu cho ca ngay va con ve mua relic: 3 -> 6 ve / ngay
B["tickets_per_day"] = 6
B["relic_shop_offers"] = 3  # constitution 7.1: moi ngay 3 relic khac nhau (sprint 3)
# ---- sprint 4 (vong 2): so co san o constitution 1.4, dua vao du lieu theo Dieu 2
# 6.1 / 6.2: vong 1 mot ghe (khach lan luot), tu vong 2 toi da 3 khach cung ngoi
B["seats"] = {"1": 1, "2": 3, "3": 3, "4": 3}
# 6.6: tu vong 2, sau khoang an toan do de tinh giam 1 diem moi 3 giay, toi da -20; ngay 8 an toan +20 giay
B["patience"] = {"from_round": 2, "safe_s": {"2": 90, "3": 70, "4": 75}, "drop_every_s": 3, "max_drop": 20, "blackout_extra_s": 20}
# 2, 5.3, 5.9, Dieu 4: ngay 8 mat dien - diem mon -0,3 (dish.blackout_penalty), nau +25%, phong toi 60% (Art Bible 4)
B["blackout"] = {"day": 8, "cook_mult": 1.25, "dim": 0.6}
# 8: cua hang vat pham (may gacha do an, nuoc sot, ve den) mo tu ngay 4
B["item_shop_from_day"] = 4
save('balance.json', B)

R = load('recipes.json')
T = R['timing']
r1 = lambda x: round(x * 2) / 2            # lam tron 0,5 giay
for g in ('phu', 'chinh'):
    for x in R[g]:
        x['prep_s'] = r1(x['prep_s'] * PREP_X)
        if 'cook_s' in x: x['cook_s'] = int(round(x['cook_s'] * COOK_X))
prep = {x['name']: x['prep_s'] for g in ('phu', 'chinh') for x in R[g]}
cook = {x['id']: x['cook_s'] for x in R['chinh']}
for r in R['recipes']:
    mid = r['id'].split('__')[0]
    for k in r['prep']: r['prep'][k][1] = prep[k]
    r['cook_s'] = cook[mid]
    r['burn_s'] = r['cook_s'] * T['burn_factor']
    r['warn_s'] = r['cook_s'] * T['warn_factor']
    r['total_s'] = round(sum(v[1] for v in r['prep'].values()) + T['step_s'] * len(r['steps']) + r['cook_s'] + T['plate_s'] + T['serve_s'], 1)
R['_note'] = 'Constitution 1.4: so che x%s, nau x%s so voi 1.3. Sinh tu make_next.py.' % (PREP_X, COOK_X)
save('recipes.json', R)

RL = load('relics.json')
for x in RL:
    # Ro tre ba ngoai: thung do phu tu 5 xuong 3 nen +2 mon thanh +1 mon (giu ti le ~ +35%)
    if x['id'] == 'V2': x['fx'] = {'phu_size': 1}
save('relics.json', RL)
print('ok ->', OUT)
