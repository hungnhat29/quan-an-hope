"""F-094 AC-2 (docs/sprint-10/impl-plan.md): tong hop playtest-log.csv cua 3-5 nguoi choi thu.
Chay: PYTHONUTF8=1 python tools/playtest_tong_hop.py <thu muc chua file> > docs/sprint-10/playtest-tong-hop.md
Moi nguoi mot file, dat ten <ten-nguoi>.csv (hoac thu muc con <ten-nguoi>/playtest-log.csv).
In bang Markdown: moi nguoi di toi ngay nao, so mon / khach moi ngay, thoi gian thao tac + di lai moi mon (trung vi theo vong),
roi so voi ba kieu nguoi choi cua constitution Dieu 1 (gia dinh bot: 19 giay / mon nguoi choi tot, 30 giay nguoi khong doc chi so)."""
import csv, glob, os, statistics, sys

ROUND_OF = lambda d: 1 if d <= 3 else 2 if d <= 8 else 3 if d <= 14 else 4
GOOD_S, NAIVE_S = 19.0, 30.0
PARKED_S = 120.0


def files(folder):
    out = {}
    for f in glob.glob(os.path.join(folder, '*.csv')):
        out[os.path.splitext(os.path.basename(f))[0]] = f
    for f in glob.glob(os.path.join(folder, '*', 'playtest-log.csv')):
        out[os.path.basename(os.path.dirname(f))] = f
    return out


def main(folder):
    people = files(folder)
    if not people:
        print('Khong thay file .csv nao trong ' + folder)
        return 1
    print('# Tong hop choi thu (F-094)\n')
    print('| Nguoi | Toi ngay | Mon / ngay | Trung vi giay / mon (vong 1 · 2 · 3 · 4) | Gan kieu nguoi choi |')
    print('| --- | --- | --- | --- | --- |')
    for name, path in sorted(people.items()):
        rows = list(csv.DictReader(open(path, encoding='utf-8')))
        if not rows:
            print('| %s | — | — | — | file trong |' % name)
            continue
        days = [int(r['ngay']) for r in rows]
        per_day = {}
        by_round = {1: [], 2: [], 3: [], 4: []}
        for r in rows:
            d = int(r['ngay'])
            per_day[d] = per_day.get(d, 0) + 1
            try:
                s = float(r['thao_tac_di_lai_s'])
            except (KeyError, ValueError):
                continue
            if 0 < s < PARKED_S:
                by_round[ROUND_OF(d)].append(s)
        meds = [statistics.median(v) if v else None for v in by_round.values()]
        allv = [x for v in by_round.values() for x in v]
        med = statistics.median(allv) if allv else None
        kind = '—' if med is None else 'nguoi choi tot' if med <= (GOOD_S + NAIVE_S) / 2 else 'khong doc chi so'
        print('| %s | %d | %.1f | %s | %s (%.0f giay) |' % (
            name, max(days), sum(per_day.values()) / max(1, len(per_day)),
            ' · '.join('—' if m is None else '%.0f' % m for m in meds), kind, med or 0))
    print('\nGia dinh cua bot (balance.json bot_timing): %.0f giay / mon nguoi choi tot, %.0f giay nguoi khong doc chi so.' % (GOOD_S, NAIVE_S))
    print('Neu trung vi nguoi that lech > 5 giay: chinh bot_timing roi chay lai balance/run_bots.ps1 (quy trinh sprint 7), khong chinh vội tay.')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1] if len(sys.argv) > 1 else '.'))
