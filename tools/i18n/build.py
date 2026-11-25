# -*- coding: utf-8 -*-
"""
F-110 AC-3 (specs/sprint-11/spec.md): ghep ban dich tieng Anh -> My project/Assets/QuanAnHope/Resources/Lang/en.json.

Nguon: tools/i18n/en.tsv (viet tay: tieng Viet <TAB> English, kieu chuoi JSON) + ten 84 mon sinh tu
mau mon chinh ("Trứng chiên {p}" -> "Fried egg with {p}") va ten do phu trong mon ("cà chua" -> "tomato").

Kiem (exit 1 neu co loi): chuoi trong strings_vi.json chua co ban dich; ban dich con chu Viet co dau;
so lo {n} khac nhau; khoa trung khac nghia.
Chay: PYTHONUTF8=1 python tools/i18n/extract.py && PYTHONUTF8=1 python tools/i18n/build.py
"""
import json, os, re, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, 'My project', 'Assets', 'QuanAnHope', 'Resources', 'Lang', 'en.json')
VI = re.compile(r'[À-ÖØ-öø-ỹđĐ]')  # bo dau nhan / chia (×, ÷)
HOLE = re.compile(r'\{\d+\}')


def dec(s):
    return json.loads('"' + s + '"')


def load_tsv():
    pairs, errors = {}, []
    for n, line in enumerate(open(os.path.join(HERE, 'en.tsv'), encoding='utf-8'), 1):
        line = line.rstrip('\r\n')
        if not line or line.startswith('#'):
            continue
        if '\t' not in line:
            errors.append('en.tsv:%d khong co TAB: %s' % (n, line[:60]))
            continue
        vi, en = line.split('\t', 1)
        try:
            vi, en = dec(vi), dec(en)
        except ValueError as e:
            errors.append('en.tsv:%d chuoi JSON hong (%s): %s' % (n, e, line[:60]))
            continue
        if vi in pairs and pairs[vi] != en:
            errors.append('en.tsv:%d khoa trung khac nghia: %r (%r / %r)' % (n, vi, pairs[vi], en))
        pairs[vi] = en
    return pairs, errors


def dish_names(pairs, errors):
    r = json.load(open(os.path.join(ROOT, 'balance', 'recipes.json'), encoding='utf-8'))
    mains = {m['name']: m for m in r['chinh']}
    sides = {s['name']: s for s in r['phu']}
    out = {}
    for x in r['recipes']:
        if x['name'] in pairs:
            continue
        m, s = mains.get(x['main']), sides.get(x['side'])
        if not m or not s or m['template'] not in pairs or s['in_name'] not in pairs:
            errors.append('mon %s: thieu mau / ten do phu de sinh ten' % x['name'])
            continue
        out[x['name']] = pairs[m['template']].replace('{p}', pairs[s['in_name']])
    return out


def main():
    pairs, errors = load_tsv()
    dishes = dish_names(pairs, errors)
    pairs.update(dishes)
    for vi, en in pairs.items():
        if VI.search(en):
            errors.append('ban dich con chu Viet: %r -> %r' % (vi, en))
        if sorted(HOLE.findall(vi)) != sorted(HOLE.findall(en)):
            errors.append('so lo {n} khac: %r -> %r' % (vi, en))
    want = json.load(open(os.path.join(HERE, 'strings_vi.json'), encoding='utf-8'))
    missing = [w for w in want if w['vi'] not in pairs]
    for w in missing:
        errors.append('thieu: %s  (%s)' % (json.dumps(w['vi'], ensure_ascii=False), w['src'][0]))
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    data = {'pairs': [{'vi': k, 'en': v} for k, v in pairs.items()]}
    json.dump(data, open(OUT, 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
    print('ban dich: %d (mon sinh tu mau: %d) · can: %d · thieu: %d -> %s'
          % (len(pairs), len(dishes), len(want), len(missing), os.path.relpath(OUT, ROOT)))
    for e in errors[:80]:
        print('  ' + e)
    if errors:
        print('LOI: %d' % len(errors))
        sys.exit(1)
    print('OK')


if __name__ == '__main__':
    main()
