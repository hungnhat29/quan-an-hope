# -*- coding: utf-8 -*-
"""
F-110 (specs/sprint-11/spec.md): gom moi chuoi tieng Viet nguoi choi co the thay.

Nguon:
  1. Chuoi trong code C# (Assets/QuanAnHope/Core + Game, bo Tests / Editor): "..." va $"..."
     ($"Ngay {day}" -> mau "Ngay {0}"; cac lo {..} danh so lan luot).
  2. Chu tren trang truyen tranh (Resources/CotTruyen/Trang_N.json, truong "s", "title", "chapter").
  3. Ten trong du lieu (balance/recipes.json, relics.json, balance.json): moi chuoi co chu Viet co dau.

Ra: tools/i18n/strings_vi.json  [{"vi": ..., "src": [...]}], sap theo thu tu xuat hien.
Chay: PYTHONUTF8=1 python tools/i18n/extract.py
"""
import json, os, re, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
ASSETS = os.path.join(ROOT, 'My project', 'Assets', 'QuanAnHope')
VI = re.compile(r'[À-ÖØ-öø-ỹđĐ]')  # bo dau nhan / chia (×, ÷)
# chuoi C#: $@"..", @$"..", $"..", @"..", ".."  (khong xu ly chuoi nhieu dong)
LIT = re.compile(r'(\$@|@\$|\$|@)?"((?:[^"\\\n]|\\.)*)"')

# chuoi khong phai chu hien thi (ten doi tuong, khoa, duong dan): bo qua theo ngu canh truoc chuoi
SKIP_BEFORE = re.compile(r'(Debug\.Log\w*|LogWarning|LogError|Exception|Find|\.name\s*==|CompareTag|Resources\.Load\w*(<[^>]*>)?|PlayerPrefs\.\w+|Shader\.Find|nameof)\s*\(?\s*$')


def unescape(s):
    return s.replace('\\n', '\n').replace('\\"', '"').replace('\\t', '\t').replace('\\\\', '\\')


def to_template(body):
    """$"..{a}..{b:N0}.." -> "..{0}..{1}..". Giu {{ }} thanh { }."""
    out, i, n = [], 0, 0
    while i < len(body):
        c = body[i]
        if c == '{' and i + 1 < len(body) and body[i + 1] == '{':
            out.append('{'); i += 2; continue
        if c == '}' and i + 1 < len(body) and body[i + 1] == '}':
            out.append('}'); i += 2; continue
        if c == '{':
            depth, j = 1, i + 1
            while j < len(body) and depth:
                if body[j] == '{': depth += 1
                elif body[j] == '}': depth -= 1
                elif body[j] == '"':
                    j += 1
                    while j < len(body) and body[j] != '"': j += 1
                j += 1
            out.append('{%d}' % n); n += 1; i = j; continue
        out.append(c); i += 1
    return ''.join(out)


def code_strings(found):
    for sub in ('Core', 'Game'):
        for d, _, fs in os.walk(os.path.join(ASSETS, sub)):
            for f in sorted(fs):
                if not f.endswith('.cs'):
                    continue
                p = os.path.join(d, f)
                rel = os.path.relpath(p, ASSETS).replace('\\', '/')
                for ln, line in enumerate(open(p, encoding='utf-8'), 1):
                    s = line.strip()
                    if s.startswith('//') or s.startswith('*') or s.startswith('[Test'):
                        continue
                    code = line.split('//')[0] if '"' not in line.split('//')[0][-1:] else line
                    for t in chains(line):
                        add(found, t, '%s:%d' % (rel, ln))
                    for m in LIT.finditer(line):
                        prefix, body = m.group(1) or '', m.group(2)
                        if m.start() > len(code):
                            continue  # nam trong chu thich cuoi dong
                        if not VI.search(body):
                            continue
                        if SKIP_BEFORE.search(line[:m.start()]):
                            continue
                        text = to_template(body) if '$' in prefix else body
                        text = unescape(text)
                        add(found, text, '%s:%d' % (rel, ln))


BETWEEN = re.compile(r'^\s*\+\s*(.*?)\s*\+\s*$')


def balanced(expr):
    d = 0
    for c in expr:
        if c in '([{': d += 1
        elif c in ')]}':
            d -= 1
            if d < 0: return False
    return d == 0 and not re.search(r'[;?]|(?<![=!<>])=(?!=)', expr)


def chains(line):
    """Cau ghep tren mot dong: "a " + x + " b" + y -> "a {0} b{1}" (mau de dich ca cau thay vi tung manh)."""
    ms = [m for m in LIT.finditer(line) if '$' not in (m.group(1) or '')]
    out, i = [], 0
    while i < len(ms):
        j = i
        while j + 1 < len(ms):
            mid = line[ms[j].end():ms[j + 1].start()]
            if re.match(r'^\s*\+\s*$', mid):
                j += 1; continue
            b = BETWEEN.match(mid)
            if b and b.group(1) and balanced(b.group(1)) and '"' not in b.group(1):
                j += 1; continue
            break
        parts, n = [], 0
        before = line[:ms[i].start()].rstrip()
        if before.endswith('+') and not before.endswith('++'):
            parts.append('{0}'); n = 1
        for k in range(i, j + 1):
            if k > i:
                mid = line[ms[k - 1].end():ms[k].start()]
                if BETWEEN.match(mid) and BETWEEN.match(mid).group(1):
                    parts.append('{%d}' % n); n += 1
            parts.append(unescape(ms[k].group(2)))
        after = line[ms[j].end():].lstrip()
        if after.startswith('+') and not after.startswith('++') and not after.startswith('+='):
            parts.append('{%d}' % n); n += 1
        t = ''.join(parts)
        if n > 0 and VI.search(t) and (j > i or n > 0):
            out.append(t)
        i = j + 1
    return out


# khong phai chu hien thi: bo chu nap san cua font, regex, loi cho nguoi lam game
EXCLUDE_IN = ('àáảãạ', '·→›‹', 'Ngày (\\d', 'tools/cot-truyen/export.js')


def add(found, text, src):
    if not VI.search(text) or any(e in text for e in EXCLUDE_IN):
        return
    if text not in found:
        found[text] = []
    if len(found[text]) < 4:
        found[text].append(src)


def comic_strings(found):
    d = os.path.join(ASSETS, 'Resources', 'CotTruyen')
    for f in sorted(os.listdir(d)):
        if not (f.startswith('Trang_') and f.endswith('.json')):
            continue
        j = json.load(open(os.path.join(d, f), encoding='utf-8'))
        for k in ('title', 'chapter', 'narration', 'caption'):
            if isinstance(j.get(k), str):
                add(found, j[k], 'CotTruyen/' + f)
        def walk(o):
            if isinstance(o, dict):
                for k, v in o.items():
                    if isinstance(v, str) and k in ('s', 'text', 'narration', 'caption', 'line'):
                        add(found, v, 'CotTruyen/' + f)
                    else:
                        walk(v)
            elif isinstance(o, list):
                for v in o:
                    walk(v)
        walk(j)


def data_strings(found):
    for name in ('recipes.json', 'relics.json', 'balance.json'):
        j = json.load(open(os.path.join(ROOT, 'balance', name), encoding='utf-8'))
        def walk(o, path):
            if isinstance(o, dict):
                for k, v in o.items():
                    if k.startswith('_'):
                        continue
                    walk(v, path + '.' + k)
            elif isinstance(o, list):
                for v in o:
                    walk(v, path)
            elif isinstance(o, str):
                add(found, o, 'balance/' + name + path)
        walk(j, '')


def main():
    found = {}
    code_strings(found)
    comic_strings(found)
    data_strings(found)
    out = [{'vi': k, 'src': v} for k, v in found.items()]
    p = os.path.join(ROOT, 'tools', 'i18n', 'strings_vi.json')
    json.dump(out, open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
    print('strings:', len(out), 'chars:', sum(len(k) for k in found), '->', os.path.relpath(p, ROOT))


if __name__ == '__main__':
    main()
