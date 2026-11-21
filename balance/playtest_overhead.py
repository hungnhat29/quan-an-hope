"""F-014 (sprint 2), F-060 AC-1 (sprint 7): tinh "thoi gian thao tac va di lai moi mon" tu playtest-log.csv cua game,
so voi gia dinh cua mo phong (constitution muc 9: OVER_GOOD nguoi choi tot, OVER_NAIVE nguoi khong doc chi so).
Chay: PYTHONUTF8=1 python balance/playtest_overhead.py <duong dan playtest-log.csv> [them file ...]
Mac dinh Windows: %USERPROFILE%/AppData/LocalLow/<cong ty>/<san pham>/playtest-log.csv (Application.persistentDataPath).

- File moi (tu sprint 7) co cot "phien": moi lan mo game mot ma -> tach tung buoi choi.
- File cu (10 cot, playtest-log-sprint2-6.csv) khong co phien: tach buoi khi t_cam_dau tut ve; test PlayMode cu ghi lan vao file
  nay va lap lai y het (cung chuoi ngay / mon) -> buoi nao lap tu 2 lan tro len bi coi la test va bo ra.
- Dong co thao_tac_di_lai_s > 120 giay la mon nau san de lau tren quay (khong phai thoi gian thao tac): bo ra.
In: trung vi theo vong va tong; dong lenh chay lai mo phong / bot voi so do."""
import csv, statistics, sys, collections

ASSUMED_GOOD, ASSUMED_NAIVE, TOLERANCE = 20.0, 30.0, 5.0
PARKED_S = 120.0     # mon de lau tren quay cho
ROUND_OF = lambda d: 1 if d <= 3 else 2 if d <= 8 else 3 if d <= 14 else 4


def sessions(path):
    rows = list(csv.DictReader(open(path, encoding='utf-8')))
    if rows and 'phien' in rows[0]:
        out = collections.OrderedDict()
        for r in rows: out.setdefault(r['phien'], []).append(r)
        return list(out.values()), 0
    out, prev = [], 1e18
    for r in rows:
        t = float(r['t_cam_dau'] or 0)
        if t < prev - 1 or not out: out.append([])
        prev = t
        out[-1].append(r)
    sig = collections.Counter(tuple((r['ngay'], r['mon']) for r in s) for s in out)

    def looks_human(s):
        days = [int(r['ngay']) for r in s]
        if len(s) < 3 or sig[tuple((r['ngay'], r['mon']) for r in s)] > 1: return False
        if days[0] != 1 or any(b < a for a, b in zip(days, days[1:])): return False   # test nhay ngay (DebugJumpToDay)
        ov = sorted(float(r['thao_tac_di_lai_s']) for r in s if r.get('thao_tac_di_lai_s'))
        q1, q3 = ov[len(ov) // 4], ov[(3 * len(ov)) // 4]
        return q3 - q1 >= 2.0                                                          # bot: deu tang tap
    human = [s for s in out if looks_human(s)]
    return human, len(out) - len(human)


def main(paths):
    all_s, dropped = [], 0
    for p in paths:
        s, d = sessions(p)
        all_s += s; dropped += d
    by_round = collections.defaultdict(list)
    print('So buoi choi:', len(all_s), '(bo', dropped, 'buoi do test PlayMode cu ghi lan)')
    for i, s in enumerate(all_s):
        ov = [float(r['thao_tac_di_lai_s']) for r in s if r.get('thao_tac_di_lai_s')]
        keep = [x for x in ov if x <= PARKED_S]
        for r in s:
            x = float(r['thao_tac_di_lai_s'] or -1)
            if 0 <= x <= PARKED_S: by_round[ROUND_OF(int(r['ngay']))].append(x)
        if keep:
            print('  buoi %d: %d mon, ngay %s-%s, trung vi %.1f giay' % (i + 1, len(keep), s[0]['ngay'], s[-1]['ngay'], statistics.median(keep)))
    every = [x for v in by_round.values() for x in v]
    if not every:
        print('Chua co dong nao'); return 1
    for rd in sorted(by_round):
        v = by_round[rd]
        print('Vong %d: %d mon, trung vi %.1f giay, trung binh %.1f' % (rd, len(v), statistics.median(v), statistics.mean(v)))
    med = statistics.median(every)
    print('Tong: %d mon, trung vi %.1f giay (gia dinh: tot %.0f, khong doc chi so %.0f)' % (len(every), med, ASSUMED_GOOD, ASSUMED_NAIVE))
    if len(all_s) < 3:
        print('CANH BAO: duoi 3 buoi choi (F-060 AC-1 can >= 3 buoi): so do chi la chi dau.')
    good = round(med)
    naive = max(ASSUMED_NAIVE, good + (ASSUMED_NAIVE - ASSUMED_GOOD))
    print('Chay lai: OVER_GOOD=%d OVER_NAIVE=%d PYTHONUTF8=1 python balance/sim_balance.py' % (good, naive))
    print('Bot C#: BotTiming.Good.overheadSeconds = %d, BotTiming.Naive.overheadSeconds = %d (balance.json bot_timing)' % (good, naive))
    return 0


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print(__doc__); sys.exit(1)
    sys.exit(main(sys.argv[1:]))
