"""F-061 (sprint 7): sinh toan bo tieng hieu ung va nhac nen cua game bang code (numpy), khong dung file ngoai.
Ban quyen: am thanh do script nay tao ra thuoc du an (xem docs/CREDITS.md). Chay lai ra y het (seed co dinh).
Chay:  PYTHONUTF8=1 python tools/am-thanh/make_audio.py
Ra:    My project/Assets/QuanAnHope/Resources/Audio/*.wav (mono 16-bit; tieng 44,1 kHz, nhac 22,05 kHz)
Ten file = ten Unity goi (GameAudio.Play("chuong")). Tieng "_loop" lap lien mach (xeo, soi)."""
import os, wave
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, '..', '..', 'My project', 'Assets', 'QuanAnHope', 'Resources', 'Audio'))
SR = 44100
rng = np.random.default_rng(20261121)


def t_(sec, sr=SR):
    return np.arange(int(sec * sr)) / sr


def env(n, a=0.005, d=0.1, sr=SR, curve=4.0):
    """Len nhanh a giay roi tat dan theo mu trong d giay."""
    t = np.arange(n) / sr
    e = np.minimum(1.0, t / max(a, 1e-4)) * np.exp(-np.maximum(0.0, t - a) / max(d, 1e-4) * curve / 4.0)
    return e


def noise(sec, sr=SR):
    return rng.uniform(-1, 1, int(sec * sr))


def lp(x, cut, sr=SR):
    """Loc thong thap mot cuc (don gian, du cho tieng hoat hinh)."""
    a = np.exp(-2 * np.pi * cut / sr)
    y = np.empty_like(x); s = 0.0
    for i, v in enumerate(x):
        s = (1 - a) * v + a * s; y[i] = s
    return y


def hp(x, cut, sr=SR):
    return x - lp(x, cut, sr)


def bp(x, lo, hi, sr=SR):
    return lp(hp(x, lo, sr), hi, sr)


def sine(f, sec, sr=SR, phase=0.0):
    f = np.broadcast_to(np.asarray(f, dtype=float), (int(sec * sr),))
    return np.sin(2 * np.pi * np.cumsum(f) / sr + phase)


def tri(f, sec, sr=SR):
    f = np.broadcast_to(np.asarray(f, dtype=float), (int(sec * sr),))
    ph = (np.cumsum(f) / sr) % 1.0
    return 4 * np.abs(ph - 0.5) - 1


def saw(f, sec, sr=SR):
    f = np.broadcast_to(np.asarray(f, dtype=float), (int(sec * sr),))
    ph = (np.cumsum(f) / sr) % 1.0
    return 2 * ph - 1


def sweep(f0, f1, sec, sr=SR, curve=1.0):
    k = np.linspace(0, 1, int(sec * sr)) ** curve
    return f0 + (f1 - f0) * k


def mix(*parts, at=None, sec=None, sr=SR):
    """Cong cac doan (x, giay bat dau)."""
    end = max(int(s * sr) + len(x) for x, s in parts)
    if sec: end = max(end, int(sec * sr))
    y = np.zeros(end)
    for x, s in parts:
        i = int(s * sr); y[i:i + len(x)] += x
    return y


def fade(x, a=0.003, b=0.02, sr=SR):
    x = x.copy(); na, nb = int(a * sr), int(b * sr)
    if na: x[:na] *= np.linspace(0, 1, na)
    if nb: x[-nb:] *= np.linspace(1, 0, nb)
    return x


def norm(x, peak=0.85):
    m = np.max(np.abs(x)) or 1.0
    return x / m * peak


def save(name, x, sr=SR, peak=0.85):
    x = norm(x, peak)
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, name + '.wav')
    with wave.open(p, 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype('<i2').tobytes())
    print('%-16s %5.2f s' % (name, len(x) / sr))


def loop_seamless(x, xf=0.15, sr=SR):
    """Noi duoi vao dau bang cross-fade de lap khong vap: out = x[:L-n], n mau dau tron tu duoi (x[L-n:]) sang dau (x[:n]).
    Mau cuoi out[-1] = x[L-n-1] noi tiep out[0] = x[L-n]."""
    n = int(xf * sr)
    k = np.linspace(0, 1, n)
    out = x[:len(x) - n].copy()
    out[:n] = x[len(x) - n:] * (1 - k) + x[:n] * k
    return out


def bell(f, sec, partials=((1, 1), (2.76, 0.4), (5.4, 0.2), (8.9, 0.08)), decay=0.6, sr=SR):
    y = np.zeros(int(sec * sr)); t = t_(sec, sr)
    for m, a in partials:
        y += a * np.sin(2 * np.pi * f * m * t) * np.exp(-t / (decay / m ** 0.5))
    return y * np.minimum(1, t / 0.002)


# ------------------------------------------------------------------ tieng hieu ung (Art Bible muc 11, impl plan F-061 AC-1)

def footstep(i):
    sec = 0.16
    n = noise(sec)
    thump = sine(sweep(110 + 8 * i, 70, sec), sec) * env(len(n), 0.002, 0.05)
    # go tram: nhieu loc hep 200-700 Hz, nho (truoc day 250-1800 Hz nghe lao xao)
    knock = lp(lp(bp(n, 200, 700 + 50 * i), 900), 900) * env(len(n), 0.001, 0.03) * 0.5
    return fade(thump + knock)


def ticket():
    sec = 0.32
    n = noise(sec)
    slide = bp(n, 1500, 6000) * np.interp(t_(sec), [0, 0.05, 0.22, 0.32], [0, 0.6, 0.4, 0])
    click = mix((sine(2600, 0.03) * env(int(0.03 * SR), 0.001, 0.01), 0.24), sec=sec)
    return fade(slide + click * 0.8)


def crate_drop():
    def thud(gain):
        sec = 0.22
        return (sine(sweep(140, 60, sec), sec) * env(int(sec * SR), 0.002, 0.08)
                + bp(noise(sec), 200, 2500) * env(int(sec * SR), 0.001, 0.03) * 0.7) * gain
    whoosh = bp(noise(0.25), 400, 3000) * np.interp(t_(0.25), [0, 0.2, 0.25], [0, 0.25, 0])
    return fade(mix((whoosh, 0), (thud(1.0), 0.22), (thud(0.45), 0.42), (thud(0.18), 0.53)))


def pickup():
    sec = 0.1
    return fade(sine(sweep(320, 720, sec, curve=0.6), sec) * env(int(sec * SR), 0.003, 0.05))


def putdown():
    sec = 0.12
    return fade(sine(sweep(260, 180, sec), sec) * env(int(sec * SR), 0.002, 0.04)
                + bp(noise(sec), 500, 3000) * env(int(sec * SR), 0.001, 0.015) * 0.5)


def chop():
    sec = 0.11
    n = noise(sec)
    return fade(hp(n, 3000) * env(len(n), 0.0005, 0.008) * 0.9
                + sine(sweep(520, 380, sec), sec) * env(len(n), 0.001, 0.035) * 0.8
                + bp(n, 300, 1200) * env(len(n), 0.001, 0.02) * 0.5)


def plop():
    sec = 0.45
    blob = sine(sweep(180, 520, 0.12, curve=0.5), 0.12) * env(int(0.12 * SR), 0.002, 0.05)
    hiss = hp(noise(0.4), 2500) * np.interp(t_(0.4), [0, 0.03, 0.4], [0, 0.5, 0])
    return fade(mix((blob, 0), (hiss, 0.05), sec=sec))


def sizzle_loop():
    sec = 2.6
    n = noise(sec)
    base = hp(n, 3500) * 0.35 + bp(n, 1500, 7000) * 0.25
    crackle = np.zeros(len(n))
    for _ in range(90):
        i = rng.integers(0, len(n) - 800)
        crackle[i:i + 800] += hp(noise(800 / SR), 2000) * env(800, 0.0003, 0.006) * rng.uniform(0.3, 1.0)
    wob = 1 + 0.15 * np.sin(2 * np.pi * 0.7 * t_(sec))
    return loop_seamless((base * wob + crackle * 0.6))


def boil_loop():
    sec = 2.6
    y = noise(sec) * 0.0
    y = lp(noise(sec), 400) * 0.6
    for _ in range(70):
        f0 = rng.uniform(180, 420); d = rng.uniform(0.03, 0.07)
        b = sine(sweep(f0, f0 * 1.9, d), d) * env(int(d * SR), 0.002, d * 0.6)
        i = rng.integers(0, len(y) - len(b)); y[i:i + len(b)] += b * rng.uniform(0.3, 0.8)
    return loop_seamless(y)


def ding():
    return fade(bell(1320, 1.1, decay=0.9) + 0.5 * bell(1980, 1.1, decay=0.6), b=0.1)


def scoop():
    sec = 0.3
    scrape = bp(noise(sec), 800, 4000) * np.interp(t_(sec), [0, 0.04, 0.2, 0.3], [0, 0.5, 0.3, 0])
    clink = mix((bell(2600, 0.25, decay=0.2) * 0.4, 0.18), sec=sec)
    return fade(mix((scrape, 0), (clink, 0)))


def service_bell():
    return fade(bell(2100, 1.6, partials=((1, 1), (2.67, 0.5), (4.1, 0.3), (6.3, 0.12)), decay=1.4), b=0.15)


def coin():
    a = bell(1760, 0.25, decay=0.25) * 0.8
    b = bell(2350, 0.45, decay=0.4)
    return fade(mix((a, 0), (b, 0.07)), b=0.05)


def happy():
    notes = [523.25, 659.25, 783.99]
    parts = []
    for i, f in enumerate(notes):
        d = 0.12
        parts.append((tri(f * (1 + 0.02 * np.sin(2 * np.pi * 9 * t_(d))), d) * env(int(d * SR), 0.004, 0.08), i * 0.075))
    return fade(mix(*parts), b=0.03)


def angry():
    sec = 0.38
    f = sweep(190, 120, sec)
    y = lp(saw(f, sec), 900) * np.interp(t_(sec), [0, 0.03, 0.25, 0.38], [0, 1, 0.8, 0])
    return fade(y * (1 + 0.3 * np.sin(2 * np.pi * 22 * t_(sec))))


def relic_buy():
    notes = [784, 988, 1175, 1568]
    parts = [(bell(f, 0.6, decay=0.5) * 0.7, i * 0.06) for i, f in enumerate(notes)]
    shimmer = hp(noise(0.7), 6000) * np.interp(t_(0.7), [0, 0.1, 0.7], [0, 0.2, 0])
    parts.append((shimmer, 0.05))
    return fade(mix(*parts), b=0.1)


def debt():
    thud = sine(sweep(120, 55, 0.35), 0.35) * env(int(0.35 * SR), 0.002, 0.15)
    clank = (bell(820, 0.5, decay=0.25) + bell(1230, 0.5, decay=0.2)) * 0.35
    drawer = bp(noise(0.25), 300, 2000) * np.interp(t_(0.25), [0, 0.05, 0.25], [0, 0.4, 0])
    return fade(mix((drawer, 0), (thud, 0.18), (clank, 0.2)), b=0.1)


def tier_reveal(level):
    # Art Bible muc 11: bac 4-5 lo ra dung 1,5 giay, co am thanh rieng. Bac 4: hop am sang; bac 5: them chuong + lap lanh.
    root = 523.25 if level == 4 else 587.33
    chord = [root, root * 1.25, root * 1.5, root * 2]
    sec = 1.5
    y = np.zeros(int(sec * SR))
    for i, f in enumerate(chord):
        s = mix((tri(f, sec - i * 0.07) * np.interp(t_(sec - i * 0.07), [0, 0.05, 1.0, sec - i * 0.07], [0, 0.5, 0.3, 0]), i * 0.07), sec=sec)
        y += s[:len(y)]
    if level == 5:
        for i, f in enumerate([2093, 2637, 3136, 4186]):
            y += mix((bell(f, 0.6, decay=0.5) * 0.35, 0.25 + i * 0.09), sec=sec)[:len(y)]
        y += hp(noise(sec), 7000) * np.interp(t_(sec), [0, 0.3, 1.5], [0, 0.18, 0])
    return fade(y, b=0.2)


def sauce(good):
    sec = 0.9
    if good:
        swirl = sine(sweep(500, 1400, sec, curve=0.7), sec) * (0.5 + 0.5 * np.sin(2 * np.pi * 14 * t_(sec)))
        y = swirl * np.interp(t_(sec), [0, 0.1, 0.7, 0.9], [0, 0.5, 0.4, 0]) + mix((bell(1568, 0.5, decay=0.4) * 0.6, 0.4), sec=sec)[:int(sec * SR)]
    else:
        y = lp(noise(sec), 700) * np.interp(t_(sec), [0, 0.1, 0.9], [0, 1, 0]) + sine(sweep(300, 120, sec), sec) * 0.3 * np.interp(t_(sec), [0, 0.5, 0.9], [0.6, 0.3, 0])
    return fade(y, b=0.05)


def burnt():
    sec = 0.8
    puff = lp(noise(sec), 1200) * np.interp(t_(sec), [0, 0.05, 0.8], [0, 1, 0])
    low = sine(sweep(160, 90, sec), sec) * np.interp(t_(sec), [0, 0.05, 0.6], [0, 0.6, 0])
    return fade(puff + low)


def eat():
    sec = 0.18
    return fade(lp(noise(sec), 1500) * env(int(sec * SR), 0.01, 0.06) * 0.6 + sine(220, sec) * env(int(sec * SR), 0.005, 0.04) * 0.3)


def ui_click():
    return fade(sine(sweep(900, 600, 0.05), 0.05) * env(int(0.05 * SR), 0.001, 0.02))


# ------------------------------------------------------------------ nhac nen (F-061 AC-3)
MSR = 22050


def note_f(n):
    return 440.0 * 2 ** ((n - 69) / 12)


def pluck(f, sec, bright=0.6, sr=MSR):
    t = t_(sec, sr)
    y = (np.sin(2 * np.pi * f * t) + bright * 0.5 * np.sin(2 * np.pi * 2 * f * t) + bright * 0.25 * np.sin(2 * np.pi * 3 * f * t))
    return y * np.exp(-t * 5) * np.minimum(1, t / 0.004)


def pad(f, sec, sr=MSR):
    t = t_(sec, sr)
    y = sum(np.sin(2 * np.pi * f * d * t) for d in (1.0, 1.003, 0.997)) / 3
    return y * np.interp(t, [0, 0.3, sec - 0.3, sec], [0, 1, 1, 0])


def render(events, total, sr=MSR):
    """events: (giay bat dau, mang mau). Duoi tran ra sau total cuon ve dau (lap lien mach)."""
    y = np.zeros(int(total * sr))
    for s, x in events:
        i = int(s * sr) % len(y)
        for k in range(0, len(x), len(y)):
            seg = x[k:k + len(y)]
            j = i
            n1 = min(len(seg), len(y) - j)
            y[j:j + n1] += seg[:n1]
            if n1 < len(seg): y[:len(seg) - n1] += seg[n1:]
    return y


def kick(sr=MSR):
    sec = 0.25
    return sine(sweep(110, 45, sec, sr), sec, sr) * np.exp(-t_(sec, sr) * 14)


def brush(sr=MSR):
    sec = 0.15
    return hp(rng.uniform(-1, 1, int(sec * sr)), 3000, sr) * np.exp(-t_(sec, sr) * 30) * 0.4


def song(chords, bpm, bars_per_chord, melody, style):
    beat = 60.0 / bpm
    bar = 4 * beat
    total = bar * bars_per_chord * len(chords)
    ev = []
    for ci, ch in enumerate(chords):
        t0 = ci * bars_per_chord * bar
        root = ch[0]
        for b in range(bars_per_chord):
            tb = t0 + b * bar
            if style == 'phong':
                ev.append((tb, pluck(note_f(root - 12), beat * 1.5, 0.2) * 0.55))
                ev.append((tb + 2 * beat, pluck(note_f(root - 12 + 7), beat * 1.5, 0.2) * 0.45))
                for k, n in enumerate(ch):
                    ev.append((tb + beat * (1 + (k % 2) * 2) + 0.01 * k, pluck(note_f(n), beat, 0.5) * 0.18))
                for q in range(4):
                    ev.append((tb + q * beat, kick() * (0.35 if q % 2 == 0 else 0.0)))
                    ev.append((tb + q * beat + beat / 2, brush()))
            elif style == 'matdien':
                if b == 0:
                    for n in ch: ev.append((tb, pad(note_f(n - 12), bar * bars_per_chord) * 0.12))
                for q in (0, 1.5, 3):
                    n = ch[int(q * 2) % len(ch)] + 12
                    ev.append((tb + q * beat, pluck(note_f(n), beat * 2, 0.9) * 0.16))
            else:  # ngay 21: cang thang, pizzicato lien tuc + trong
                for q in range(8):
                    n = ch[q % len(ch)] - (12 if q % 4 == 0 else 0)
                    ev.append((tb + q * beat / 2, pluck(note_f(n), beat * 0.4, 0.8) * 0.22))
                ev.append((tb, kick() * 0.6)); ev.append((tb + 2 * beat, kick() * 0.5))
                ev.append((tb + 3.5 * beat, kick() * 0.3))
                if b == 0: ev.append((tb, pad(note_f(root - 24), bar * bars_per_chord) * 0.18))
    for s, n, d in melody:
        ev.append((s * beat, pluck(note_f(n), d * beat + 0.3, 0.7) * (0.3 if style != 'matdien' else 0.22)))
    return render(ev, total)


def music():
    # phong: Fmaj7 - Dm7 - Gm7 - C7, 92 bpm, giai dieu am ap
    C = [[65, 69, 72, 76], [62, 65, 69, 72], [67, 70, 74, 77], [60, 64, 67, 70]]
    mel = []
    line = [72, 74, 76, 74, 72, 69, 67, 69, 70, 72, 74, 72, 70, 69, 67, 65]
    for i, n in enumerate(line * 2): mel.append((i * 2 + (0.5 if i % 3 == 1 else 0), n, 1.5))
    save('nhac_phong', song(C, 92, 2, mel, 'phong'), MSR, 0.6)
    # mat dien: Am - F - C - E, 60 bpm, hop nhac thua thot
    C2 = [[57, 60, 64], [53, 57, 60], [48, 52, 55], [52, 56, 59]]
    mel2 = [(i * 3, n, 2.5) for i, n in enumerate([76, 72, 71, 69, 72, 69, 68, 64])]
    save('nhac_mat_dien', song(C2, 60, 2, mel2, 'matdien'), MSR, 0.55)
    # ngay 21: Dm - Bb - Gm - A, 116 bpm, ostinato
    C3 = [[62, 65, 69], [58, 62, 65], [55, 58, 62], [57, 61, 64]]
    mel3 = [(i * 4 + 2, n, 1.5) for i, n in enumerate([74, 77, 74, 73, 74, 70, 69, 73])]
    save('nhac_ngay21', song(C3, 116, 2, mel3, 'ngay21'), MSR, 0.6)


def main():
    for i in range(4): save('buoc_chan_%d' % i, footstep(i), peak=0.5)
    save('nhet_ve', ticket())
    save('thung_roi', crate_drop())
    save('cam', pickup(), peak=0.6)
    save('dat', putdown(), peak=0.6)
    save('thai', chop())
    save('cho_vao_bep', plop())
    save('xeo_loop', sizzle_loop(), peak=0.5)
    save('soi_loop', boil_loop(), peak=0.5)
    save('chin', ding())
    save('muc', scoop())
    save('chuong', service_bell())
    save('tien', coin())
    save('khach_vui', happy())
    save('khach_gian', angry())
    save('mua_relic', relic_buy())
    save('thu_no', debt())
    save('bac_4', tier_reveal(4))
    save('bac_5', tier_reveal(5))
    save('sot_tot', sauce(True))
    save('sot_hong', sauce(False))
    save('chay_khet', burnt())
    save('an', eat(), peak=0.4)
    save('bam', ui_click(), peak=0.5)
    music()


if __name__ == '__main__':
    main()
