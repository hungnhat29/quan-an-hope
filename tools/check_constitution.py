"""F-090 AC-3 (docs/sprint-10/impl-plan.md): so cac bang so trong specs/constitution.md voi balance/*.json.
Chay: PYTHONUTF8=1 python tools/check_constitution.py   -> in tung dong OK / LECH, thoat ma 1 neu co dong lech.
Constitution la nguon su that cao nhat; hai noi phai khop nhau (Dieu 2)."""
import json, os, re, sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
C = open(os.path.join(ROOT, 'specs', 'constitution.md'), encoding='utf-8').read()
B = json.load(open(os.path.join(ROOT, 'balance', 'balance.json'), encoding='utf-8'))
R = json.load(open(os.path.join(ROOT, 'balance', 'recipes.json'), encoding='utf-8'))
RL = json.load(open(os.path.join(ROOT, 'balance', 'relics.json'), encoding='utf-8'))

results = []


def num(s):
    return float(s.replace('.', '').replace(',', '.')) if re.search(r'\d', s) else None


def check(name, const_value, data_value):
    ok = const_value == data_value
    results.append((ok, name, const_value, data_value))


def section(title):
    i = C.index(title)
    j = C.find('\n### ', i + len(title))
    k = C.find('\n## ', i + len(title))
    ends = [x for x in ((k,) if title.startswith('## ') else (j, k)) if x > 0]
    return C[i:min(ends) if ends else len(C)]


def table_rows(text):
    rows = []
    for ln in text.splitlines():
        if ln.startswith('|') and not re.match(r'^\|\s*-', ln):
            rows.append([c.strip() for c in ln.strip('|').split('|')])
    return rows


# ---- muc 2: lich no
for day, amount in re.findall(r'Đầu ngày (\d+): \*\*([\d.]+)G\*\*', C):
    check('No dau ngay ' + day, int(amount.replace('.', '')), B['debt_days'].get(day))
check('Thuong chu no', int(re.search(r'nhận thưởng \*\*([\d.]+)G\*\*', C).group(1).replace('.', '')), B['boss_bonus'])

# ---- muc 3: tien te
check('Ve moi ngay', int(re.search(r'\*\*(\d+) vé mỗi ngày\*\*', C).group(1)), B['tickets_per_day'])
check('Tien khoi dau', int(re.search(r'Khởi đầu: \*\*(\d+)G\*\*', C).group(1)), B['start']['money'])
check('Ve ngay 1', int(re.search(r'\*\*ngày 1 có (\d+) vé\*\*', C).group(1)), B['start']['first_day_tickets'])

# ---- muc 4.2 / 4.3: thung va ti le
m = re.search(r'1 thùng đồ phụ \((\d+) món\) \+ 1 thùng nguyên liệu chính \((\d+) món\)', C)
check('Thung do phu', int(m.group(1)), B['crate_size']['phu'])
check('Thung chinh', int(m.group(2)), B['crate_size']['chinh'])
odds_key = {'Vòng 1 (ngày 1–2)': 'r1', 'Ngày 3 (sau mưa)': 'storm', 'Vòng 2 (ngày 4–8)': 'r2', 'Vòng 3 (ngày 9–14)': 'r3',
            'Vòng 4 (ngày 15–21)': 'r4', 'Vé đen (mọi vòng)': 'black'}
for row in table_rows(section('### 4.3 Tỉ lệ bậc theo vòng')):
    if row[0] in odds_key:
        check('Ti le ' + row[0], [int(x) for x in row[1:7]], B['ingredient_odds'][odds_key[row[0]]])

# ---- muc 4.5: thoi gian so che
prep = {x['name']: x['prep_s'] for g in ('phu', 'chinh') for x in R[g]}
for row in table_rows(section('### 4.5 Danh sách nguyên liệu')):
    if len(row) >= 3 and row[0] in prep and 'giây' in row[2]:
        check('So che ' + row[0], num(row[2].split()[0]), prep[row[0]])

# ---- muc 5.2: he so gia
mult = []
for row in table_rows(section('### 5.2 Bậc món ăn')):
    if len(row) == 3 and row[2].startswith('×'):
        mult.append(num(row[2][1:]))
check('He so gia theo bac', mult, B['dish']['price_mult'])
check('Mon dot pha gia', int(re.search(r'món đột phá \*\*(\d+)G\*\*', C).group(1)), sorted({r['price'] for r in R['recipes'] if r['group'] != 'chinh'})[0])

# ---- muc 5.8: so mon, bang cap
check('So mon', 84 if '**84 món**' in C else None, len(R['recipes']))
names = {x['name']: x['id'] for g in ('phu', 'chinh') for x in R[g]}
short = {x['in_name']: x['id'] for x in R['phu']}
short.update({'bông cải': 'bong_cai'})
pairs = {}
for r in R['recipes']:
    pairs.setdefault(r['main'], set()).add(r['id'].split('__')[1])
for row in table_rows(section('### 5.8 Công thức và danh sách món')):
    if len(row) == 2 and row[0] in pairs:
        want = {short.get(x.strip()) for x in row[1].split(',')}
        check('Cap ' + row[0], sorted(want), sorted(pairs[row[0]]))

# ---- muc 5.5 / 5.6 / 5.7: nuoc sot, may gacha, tu lanh
sauce = table_rows(section('### 5.5 Nước sốt thần thánh của Hope'))
keys = ['fail', 'same', 'up1', 'up2', 'legend']
early = [int(r[1].rstrip('%')) for r in sauce[1:6]]
late = [int(r[2].rstrip('%')) for r in sauce[1:6]]
check('Sot ngay 4-8', early, [B['sauce']['odds_early'][k] for k in keys])
check('Sot tu ngay 9', late, [B['sauce']['odds_late'][k] for k in keys])
mach = [int(r[1].rstrip('%')) for r in table_rows(section('### 5.6 Máy gacha đồ ăn'))[1:7]]
check('May gacha do an', mach, [B['machine']['odds'][k] for k in ['trash', 'down1', 'same', 'up1', 'up2', 'up3']])
check('Tu lanh o', int(re.search(r'Mặc định \*\*(\d+) ô\*\*', C).group(1)), B['fridge']['slots'])
check('Tu lanh ngay', int(re.search(r'quá \*\*(\d+) ngày\*\*', C).group(1)), B['fridge']['decay_days'])

# ---- muc 6.3 / 6.4 / 6.6: khach
for row in table_rows(section('### 6.3 Độ dễ tính (%)')):
    if row[0].isdigit():
        share = [B['customers']['easy_bands'][row[0]][i][2] for i in range(3)]
        check('De tinh vong ' + row[0], [int(x.rstrip('%')) for x in row[1:4]], [round(s * 100) for s in share])
check('Chu no de tinh', int(re.search(r'cố định \*\*(\d+)%\*\*', C).group(1)), B['customers']['boss_easy'])
safe = {}
for row in table_rows(section('### 6.6 Thời gian chờ (từ vòng 2)')):
    m = re.match(r'\*\*(\d+) giây\*\*', row[1]) if len(row) > 1 else None
    if row[0].isdigit() and m: safe[row[0]] = int(m.group(1))
    m2 = re.match(r'\*\*(\d+)G\*\*', row[1]) if len(row) > 1 else None
    if row[0].isdigit() and m2: check('Khach bo di vong ' + row[0], int(m2.group(1)), B['patience']['leave_penalty'][row[0]])
for k, v in safe.items(): check('Khoang an toan vong ' + k, v, B['patience']['safe_s'][k])

# ---- muc 7: relic
prices = {}
for row in table_rows(section('## 7. Relic')):
    if len(row) >= 3 and re.match(r'\d+ vé', row[1]):
        prices[row[0]] = int(row[1].split()[0])
for r in RL:
    if r['name'] in prices: check('Gia relic ' + r['name'], prices[r['name']], r['price'])
check('So relic nhan', len(prices), len(RL))
check('Toi da relic', int(re.search(r'Tối đa \*\*(\d+) relic\*\*', C).group(1)), B['relic_cap'])

# ---- muc 8: cua hang
shop = table_rows(section('## 8. Cửa hàng vật phẩm'))
for row in shop:
    if row[0].startswith('Máy gacha'): check('Gia may gacha', int(re.search(r'(\d+)G', row[2]).group(1)), B['machine']['price'])
    if row[0].startswith('Nước sốt'):
        a, b = re.findall(r'\+?(\d+)G', row[2])[:2]
        check('Gia sot', [int(a), int(b)], [B['sauce']['price'], B['sauce']['step']])
    if row[0].startswith('Vé đen'):
        a, b = re.findall(r'\+?(\d+)G', row[2])[:2]
        check('Gia ve den', [int(a), int(b)], [B['black_ticket']['price'], B['black_ticket']['step']])
    if row[0].startswith('**Thớt**'):
        a, b = re.findall(r'\+?(\d+)G', row[2])[:2]
        check('Gia thot', [int(a), int(b)], [B['board']['price'], B['board']['step']])
        check('Toi da thot', int(re.search(r'tối đa \*\*(\d+) thớt', row[3]).group(1)), B['board']['max'])
    if row[0].startswith('**Tay vạn năng**'):
        check('Gia tay van nang', int(re.search(r'(\d+)G', row[2]).group(1)), B['glove']['price'])
    if row[0].startswith('**Bếp**'):
        a, b = re.findall(r'\+?(\d+)G', row[2])[:2]
        check('Gia bep', [int(a), int(b)], [B['stove']['price'], B['stove']['step']])
        check('Toi da bep', int(re.search(r'tối đa \*\*(\d+) bếp', row[3]).group(1)), B['stove']['max'])

# ---- 2.1: thuong mon ngon nhat, ti le 6 bac (bang muc 5.3 so voi balance/cookodds.py), relic Huyen thoai
check('Thuong mon ngon nhat x', float(re.search(r'thưởng ×\*\*([\d,]+)\*\*', C).group(1).replace(',', '.')), B['boss_best']['bonus_mult'])
sys.path.insert(0, os.path.join(ROOT, 'balance'))
import cookodds  # noqa: E402
table = table_rows(section('### 5.3 Công thức ra bậc món'))
for row in table:
    m = re.match(r'Chính (\d) \+ phụ (\d)', row[0])
    if not m or len(row) < 8:
        continue
    want = [num(x) for x in row[2:8]]
    p = cookodds.chances(B['dish'], int(m.group(1)), int(m.group(2)))
    tk = cookodds.tickets(B['dish'], p)
    got = [round(t / B['dish']['odds']['grid'] * 100, 3) for t in tk]
    close = all(abs(g - w) <= max(0.011, w * 0.01) for g, w in zip(got, want))
    results.append((close, 'Ti le ' + row[0], want, got))
for rid, val in (('F11', 0.003), ('F12', 0.01)):
    rel = next(x for x in RL if x['id'] == rid)
    pct = float(re.search(re.escape(rel['name']) + r'\*\* \(từ 2\.1\) \| (\d) vé \| [^|]+\| Huyền thoại khi nấu \*\*\+([\d,]+)%', C).group(2).replace(',', '.'))
    check('Relic ' + rel['name'], pct / 100, rel['fx']['dish_legend'])

bad = [r for r in results if not r[0]]
for ok, name, c, d in results:
    print(('OK   ' if ok else 'LECH ') + name + ': constitution ' + str(c) + ' · du lieu ' + str(d))
print('\n%d dong so, %d lech' % (len(results), len(bad)))
sys.exit(1 if bad else 0)
