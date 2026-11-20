const L = require('./lib.js'), S = require('./scenes.js'), K = require('./kit.js'); const { C, INK, layer } = L; const { pr, st } = S;
const TOP = [36, 470], BOT = [498, 1036];
module.exports = { chapter: 'Chương 6 · Hết nợ, có vợ', num: 6, tiers: [
  { y0: TOP[0], y1: TOP[1], cuts: [[920, 1000]], scenes: [
    (w, h) => { const k = K.kitchen(w, h, { ctr: h * 0.8 });
      return k.back + k.mid + layer(3, L.creditor({ x: w * 0.34, y: h + 120, s: 0.95, expr: 'happy', arms: 'up' }) + L.hope({ x: w * 0.8, y: h + 40, s: 0.95, flip: true, expr: 'shock' })) +
        layer(4, K.pieces(w * 0.34, 70, 12)) + L.sfx(w * 0.5, 110, 'XOẸT!!', { fs: 100, col: C.coral, rot: -8 }) + L.caption(20, h - 70, 320, 50, 'Ngày 22 · Nợ đã trả đủ', 24, C.mint); },
    (w, h) => { const k = K.kitchen(w, h, { ctr: h * 0.8 });
      return k.back + layer(2, L.daughter({ x: w * 0.68, y: h * 0.84, s: 0.95, expr: 'love', arms: 'hold' }) + L.heart(w * 0.84, 90, 1) + L.heart(w * 0.92, 160, 0.7, C.pink)) + k.mid +
        layer(4, L.creditor({ x: w * 0.3, y: h + 90, s: 1.0, expr: 'smirk', arms: 'point' }) + L.hope({ x: w * 0.92, y: h + 160, s: 1.0, flip: true, expr: 'love' })) +
        L.balloon(w * 0.04, 16, 360, 100, 'Còn một chuyện nữa…\ncon gái ta.', { fs: 28, tail: [w * 0.24, 140] }); },
  ] },
  { y0: BOT[0], y1: BOT[1], cuts: [], scenes: [
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#F7D88A"/>` + S.cloud(260, 90, 1.2) + S.cloud(1500, 70, 1) + S.cloud(980, 50, 0.7)) +
      layer(1, S.cityFar(w, h * 0.66, '#EFC59A', 8)) +
      layer(2, S.facade(w * 0.5, h * 0.82, 0.95, { lit: true }) + `<path d="M${w * 0.2} 14 Q${w * 0.5} 74 ${w * 0.8} 14" fill="none" stroke="${INK}" stroke-width="4"/>` + [0.28, 0.39, 0.5, 0.61, 0.72].map((t, i) => pr.lantern(w * t, 40 + Math.sin(t * Math.PI) * 30, 0.5, true)).join('') + `<rect x="-20" y="${h * 0.82}" width="${w + 40}" height="200" fill="#C9A47E" ${st}/>`) +
      layer(3, L.guest({ x: w * 0.1, y: h * 0.96, s: 0.85, col: C.blue, expr: 'happy' }) + L.granny({ x: w * 0.22, y: h * 0.96, s: 0.85, expr: 'love', arms: 'wave' }) + L.guest({ x: w * 0.78, y: h * 0.96, s: 0.85, col: C.lilac, expr: 'happy', flip: true }) + L.guest({ x: w * 0.9, y: h * 0.96, s: 0.85, col: C.pink, hair: '#3A3230', expr: 'love', flip: true })) +
      layer(4, L.hope({ x: w * 0.44, y: h - 10, s: 1.15, expr: 'love', arms: 'down' }) + L.daughter({ x: w * 0.56, y: h - 10, s: 1.15, expr: 'love', arms: 'down', flip: true }) + `<path d="M${w * 0.44 - 110} ${h - 40} q-60 -40 -30 -110" fill="none"/>` +
        L.creditor({ x: w * 0.68, y: h + 40, s: 0.95, expr: 'cry', flip: true }) + pr.ring(w * 0.5, h * 0.4, 0.8) + L.sparkle(w * 0.46, h * 0.33, 0.8) + L.sparkle(w * 0.55, h * 0.36, 0.6)) +
      layer(5, S.confetti(w, h * 0.6, 70) + `<path d="M-20 ${h + 20} q20 -160 140 -130 q40 -80 130 -20 q40 60 0 150z" fill="${C.green}" ${st}/><path d="M${w + 20} ${h + 20} q-20 -160 -140 -130 q-40 -80 -130 -20 q-40 60 0 150z" fill="${C.green}" ${st}/>`) +
      `<g transform="translate(${w - 210},${h - 120}) rotate(-12)" filter="url(#pp4)"><circle r="78" fill="${C.coral}" stroke="${INK}" stroke-width="6"/><circle r="64" fill="none" stroke="#FFF3DC" stroke-width="4"/><text y="16" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="48" fill="#FFF3DC">HẾT</text></g>` +
      L.caption(30, 30, 380, 54, 'Quán của Hope · Khai trương lại', 24),
  ] },
] };
