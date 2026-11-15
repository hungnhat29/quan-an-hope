"""F-014 (sprint 2): tinh "thoi gian thao tac va di lai moi mon" tu playtest-log.csv cua game,
so voi gia dinh cua mo phong (constitution 1.4 muc 9: 20 giay nguoi choi tot, 30 giay nguoi khong doc chi so).
Chay: PYTHONUTF8=1 python balance/playtest_overhead.py <duong dan playtest-log.csv>
Mac dinh Windows: %USERPROFILE%/AppData/LocalLow/<cong ty>/<san pham>/playtest-log.csv (Application.persistentDataPath).
Lech qua 5 giay thi chay lai mo phong voi so do: OVER_GOOD=<so> OVER_NAIVE=<so> V= python balance/sim_balance.py"""
import csv, statistics, sys

ASSUMED_GOOD, ASSUMED_NAIVE, TOLERANCE = 20.0, 30.0, 5.0

def main(path):
    rows = list(csv.DictReader(open(path, encoding='utf-8')))
    over = [float(r['thao_tac_di_lai_s']) for r in rows if r.get('thao_tac_di_lai_s')]
    if not over:
        print('Chua co dong nao trong', path); return 1
    mean, med = statistics.mean(over), statistics.median(over)
    print('So mon:', len(over))
    print('Thao tac + di lai moi mon: trung binh %.1f giay, trung vi %.1f giay (min %.1f, max %.1f)' % (mean, med, min(over), max(over)))
    print('Luu y: mon nau san roi de lau tren quay cho lam so lon hon; trung vi it bi anh huong hon.')
    for name, assumed in (('nguoi choi tot', ASSUMED_GOOD), ('nguoi khong doc chi so', ASSUMED_NAIVE)):
        d = med - assumed
        print('  so voi gia dinh %s %.0f giay: lech %+.1f giay -> %s' % (name, assumed, d, 'can sua OVERHEAD va chay lai mo phong' if abs(d) > TOLERANCE else 'trong nguong'))
    print('Chay lai: OVER_GOOD=%.0f OVER_NAIVE=%.0f PYTHONUTF8=1 python balance/sim_balance.py' % (med, max(med, ASSUMED_NAIVE)))
    return 0

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print(__doc__); sys.exit(1)
    sys.exit(main(sys.argv[1]))
