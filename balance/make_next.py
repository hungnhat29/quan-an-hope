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
DEBT = {"4": 750, "9": 2900, "15": 10500, "22": 36000}
# sprint 7 F-060 (21/11/2026): no 22 32.000 -> 36.000G cung ve 6 -> 7 / ngay (chon bang bot C# tren luat that,
# balance/bot_sweep.md; constitution 1.7 muc 9)
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
B["tickets_per_day"] = 8  # sprint 7 F-060: 6 -> 7; yeu cau 23/11/2026 "mac dinh moi ngay la 8 ve, tru ngay 1 van 5 ve": 7 -> 8
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
# ---- truoc sprint 7 (yeu cau 21/11/2026, constitution 1.7 muc 3): "ngay 1 chi co 5 ve" (cu: 6 ve ngay + 2 ve thuong = 8)
B["start"] = {"money": 100, "first_day_tickets": 5, "relic_shop_from_day": 2}
# ---- sprint 7 F-060 AC-1: thoi gian cua bot / mo phong (GIA DINH ve nguoi choi, khong phai luat game) lay tu so do
# playtest-log.csv (balance/playtest_overhead.py): nguoi choi tot 19 giay / mon (do, 47 mon vong 1), nguoi khong doc
# chi so 30 giay (chua co so do rieng, giu gia dinh), di quay may 4 giay, lay mon tu tu lanh 6 giay.
B["bot_timing"] = {"good_overhead_s": 19, "naive_overhead_s": 30, "pull_s": 4, "fridge_s": 6}
# ---- sprint 8 (yeu cau 21/11/2026, constitution 1.8) ----
SPRINT8_GAP_K, SPRINT8_MIN_GAP = 100, 6
SPRINT8_SAFE = {"2": 120, "3": 100, "4": 105}
# 6.2 "ngay 4 vong 2 ma 1 giay da 3 nguoi vao cung luc": tu vong 2 hai khach lien nhau vao cach nhau it nhat
# max(min_gap_s, gap_k_s / ngay) giay (toc do khach den ti le thuan voi ngay). So chon bang bot C# (balance/bot_sweep.md).
B['day']['arrival'] = {"from_round": 2, "gap_k_s": SPRINT8_GAP_K, "min_gap_s": SPRINT8_MIN_GAP}
# 6.6 "noi them thoi gian cho": khoang an toan moi vong (chon bang bot C#)
B["patience"]["safe_s"] = SPRINT8_SAFE
# 7.1 doi hang cua hang relic bang tien: 50G, moi lan sau trong ngay +25G (ban tra loi 21/11)
B["relic_reroll"] = {"base": 50, "step": 25}
# 7.1 ngay 2 relic mien phi: ca 3 be (ke ca sau doi hang) gia 0, chi relic Thuong, lay 1 (ban tra loi 21/11)
B["relic_free_day"] = {"day": 2, "rarity": "thuong", "picks": 1}
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

# ---- truoc sprint 10 (yeu cau 22/11/2026, constitution 2.0) ----
# 5.8 "giam bot 30% so cong thuc mon an lai va tinh toan voi loai nguyen lieu tuong ung": moi nguyen lieu chinh / dot pha chi
# ghep voi nhung do phu duoi day (120 -> 84 mon). Chon de: (a) moi do phu co >= 1 mon ngay o vong no mo khoa; (b) moi vong
# giu ~70% so cap; (c) cap nghe hop mieng (bo xao rau muong, dau phu sot ca chua...). Cap khong co o day = khong co mon.
PAIRS = {
    'trung':   ['ca_chua', 'hanh_tay', 'khoai_tay', 'nam_rom', 'ot_chuong', 'mang_tay', 'nam_dong_co'],
    'heo':     ['hanh_tay', 'rau_muong', 'khoai_tay', 'nam_rom', 'bong_cai', 'ot_chuong', 'bi_do', 'nam_dong_co'],
    'dau_phu': ['ca_chua', 'hanh_tay', 'nam_rom', 'bong_cai', 'bi_do', 'nam_dong_co'],
    'bo':      ['ca_chua', 'hanh_tay', 'rau_muong', 'khoai_tay', 'bong_cai', 'ot_chuong', 'mang_tay', 'nam_dong_co'],
    'tom':     ['hanh_tay', 'rau_muong', 'nam_rom', 'bong_cai', 'bi_do', 'mang_tay'],
    'ca_hoi':  ['ca_chua', 'khoai_tay', 'nam_rom', 'bong_cai', 'ot_chuong', 'mang_tay', 'nam_dong_co'],
    'muc':     ['hanh_tay', 'nam_rom', 'bong_cai', 'ot_chuong', 'mang_tay', 'nam_dong_co'],
    'ga':      ['ca_chua', 'hanh_tay', 'khoai_tay', 'bong_cai', 'ot_chuong', 'bi_do', 'mang_tay', 'nam_dong_co'],
    'tom_hum': ['ca_chua', 'hanh_tay', 'khoai_tay', 'bong_cai', 'ot_chuong', 'mang_tay', 'nam_dong_co'],
    'wagyu':   ['hanh_tay', 'khoai_tay', 'nam_rom', 'ot_chuong', 'bi_do', 'mang_tay', 'nam_dong_co'],
    'ca_ngu':  ['ca_chua', 'hanh_tay', 'rau_muong', 'nam_rom', 'bong_cai', 'ot_chuong', 'mang_tay'],
    'cua':     ['ca_chua', 'hanh_tay', 'rau_muong', 'nam_rom', 'bi_do', 'mang_tay', 'nam_dong_co'],
}
R['recipes'] = [r for r in R['recipes'] if r['id'].split('__')[1] in PAIRS[r['id'].split('__')[0]]]

# 5.2 "bo phan gia theo bac mon, co gia niem yet cho tung mon o bac binh thuong, bac thap hon va cao hon ti le thuan voi gia":
# moi mon mot gia (bac Binh thuong, he so x1). Gia goc cu theo vong (60 / 70 / 85 / 100) la muc tieu: giai lan luot gia cua
# nhom mon mo o vong r sao cho trung binh cac mon lam duoc o vong r (deu nhau) bang gia goc cu cua vong r. Trong nhom, mon
# lau hon thi dat hon (can bac hai thoi gian). Lam tron 5G. Mon dot pha: mot gia cho ca nhom (BT_PRICE), chon bang bot.
OLD_BASE = {1: 60, 2: 70, 3: 85, 4: 100}
# Lan 1 (bot C# 4.000 van): giu trung binh = gia goc cu -> gom bo thang 71% (1.9: 87%), khach goi lech ve mon re (gacha ghep cap
# lam loai chinh cu ra nhieu hon). Lan 2: x1,07 -> 88% / 57% (khong gom bo) / 83%; lan 3: x1,05 -> 85% / 50% / 82% (chot).
PRICE_X = float(os.environ.get("PRICE_X", "0.95"))  # 2.1 (24/11/2026): 8 ve + ti le 6 bac lam de hon -> quet bot: 1.05 -> 94/97/72%, 0.95 -> 90/89/30%, 0.9 -> 87/77/12%; chon 0.95 (Q-79)
BT_PRICE = 100
r5 = lambda x: int(5 * round(x / 5.0))
std = [r for r in R['recipes'] if r['group'] == 'chinh']
for rd in (1, 2, 3, 4):
    group = [r for r in std if r['unlock_round'] == rd]
    older = [r for r in std if r['unlock_round'] < rd]
    mean_t = sum(r['total_s'] for r in group) / len(group)
    target = OLD_BASE[rd] * PRICE_X * (len(group) + len(older)) - sum(r["price"] for r in older)
    U = target / len(group)
    for _ in range(40):  # chinh U de tong sau khi lam tron khop muc tieu
        for r in group: r['price'] = r5(U * (r['total_s'] / mean_t) ** 0.5)
        U += (target - sum(r['price'] for r in group)) / len(group) * 0.5
for r in R['recipes']:
    if r['group'] != 'chinh': r['price'] = BT_PRICE
save('recipes.json', R)

# 6.6 "khach hang khi het thanh cho doi thi se tuc gian bo di va tu dong tru 1 so tien nhat dinh": vong 2 / 3 / 4 (~30% gia goc cu)
B['patience']['leave_penalty'] = {"2": 20, "3": 25, "4": 30}
# 8 "them vao Shop: mua them thot max 3, tang gia moi luot mua": tong toi da 3 thot (co san 1), 300G roi +200G moi lan
B['board'] = {"start": 1, "max": 3, "price": 300, "step": 200}
# 5.3 / 5.4 (yeu cau 23/11/2026 "chinh sua lai ti le"): moi to hop ra du 6 bac, Huyen thoai ngan rieng (balance/cookodds.py)
B['dish']['odds'] = {
    "shape": [4.95, 17.05, 74.35, 13.25, 9.15],  # mau chuan chinh 1 + phu 1 (b = 1,8): Lac vi..Tuyet hao (Huyen thoai 2,1 tach rieng)
    "shape_b": 1.8, "shape_tier": 2,               # o b = 1,8 tam mau o bac Binh thuong
    "tail_decay": 3.0,                             # moi bac xa hon giam e^-3 (~20 lan)
    "legend_base": 0.00001, "legend_growth": 2000, # L = 0,001% x 2000^(S/5): 0,001% o S = 0, 2% o S = 5
    "legend_cap": 0.05,                            # relic Huyen thoai cong them, tong toi da 5%
    "min_chance": 0.00001,                         # moi bac toi thieu 0,001%
    "grid": 100000,                                # luu tren 100.000 ve nguyen
}
B['dish']['max_tier'] = 4  # bac cao nhat cua 5 bac chia hinh dang (Tuyet hao); Huyen thoai = bac 5 (ngan rieng)
# 8 "them bep vao shop: max 2 bep ... mua them bep gia 500g, mua lan 2 voi gia 800g": co san 1; gia 500G, +300G moi lan
B['stove'] = {"start": 1, "max": 2, "price": 500, "step": 300}
# 6.7 / muc 8 boss: "mon ngon nhat cua tiem" - goi y bang tranh: Trung chien ca chua bac Huyen thoai -> thuong x1,5 (Q-80)
B['boss_best'] = {"recipe": "trung__ca_chua", "tier": 5, "bonus_mult": 1.5}
# 8 (yeu cau 24/11/2026 "tay van nang: mua thi cam duoc 2 do ... tu dinh gia, chi mua 1 lan"): 600G, mo ngay 4 cung cua hang (Q-85)
B['glove'] = {"price": 600}
save('balance.json', B)

RL = load('relics.json')
for x in RL:
    # Ro tre ba ngoai: thung do phu tu 5 xuong 3 nen +2 mon thanh +1 mon (giu ti le ~ +35%)
    if x['id'] == 'V2': x['fx'] = {'phu_size': 1}
# 7.3 (yeu cau 23/11/2026 "co 2 relic moi"): relic Huyen thoai khi nau (+0,3% va +1,0%; tong toi da 5%). Ten relic thu hai: Q-81
RL.append({"id": "F11", "name": "Sao băng trong nồi", "set": "", "price": 2, "rarity": "hiem", "fx": {"dish_legend": 0.003}})
RL.append({"id": "F12", "name": "Bí kíp gia truyền", "set": "", "price": 4, "rarity": "hiem", "fx": {"dish_legend": 0.01}})
save('relics.json', RL)
print('ok ->', OUT)
