const L = require('./lib.js'), S = require('./scenes.js'), K = require('./kit.js'); const { C, INK, layer } = L; const { pr, st } = S;
const TOP = [36, 520], BOT = [548, 1036];
module.exports = { chapter: 'Chương 2 · Cơn bão', num: 2, tiers: [
  { y0: TOP[0], y1: TOP[1], cuts: [[760, 840]], scenes: [
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="${S.skyFill('storm')}"/>` + S.cloud(150, 70, 1.4, '#9AA6B8') + S.cloud(560, 50, 1.2, '#9AA6B8')) +
      layer(1, S.bolt(w * 0.72, -10, 1) + S.cityFar(w, h * 0.7, '#6E7C93', 4)) +
      layer(2, S.facade(w * 0.42, h - 30, 0.85, { lit: true }) + `<rect x="-20" y="${h - 34}" width="${w + 40}" height="60" fill="#5F6E80" ${st}/>`) +
      `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#3B4A66" opacity="0.28"/>` + S.rain(w, h, 90, 0.75) +
      layer(4, `<g transform="rotate(-14 ${w - 40} ${h})"><path d="M${w - 160} ${h + 30} q-10 -190 60 -230 q-30 90 40 130 q20 -120 90 -150 q-30 120 -20 250z" fill="#4E8A5A" ${st}/></g>`) +
      L.sfx(w * 0.6, h * 0.9, 'ĐÙNG!!', { fs: 110, col: C.must, rot: -10 }) + L.caption(22, 20, 250, 50, 'Ngày 2 · Bão đổ bộ', 24),
    (w, h) => { const k = K.kitchen(w, h, { sky: '#7E8FA6', windowInner: S.rain(400, 300, 30, 0.9), ctr: h * 0.74 });
      return k.back + `<path d="M${w * 0.38} 0 l20 40 l-14 20 l24 30" fill="none" stroke="${INK}" stroke-width="5"/>` +
        layer(1, `<g fill="${C.blue}" stroke="${INK}" stroke-width="3">${[0, 1, 2].map(i => `<path d="M${w * 0.4 + 6} ${80 + i * 80} q-10 16 0 22 q10 -6 0 -22z"/>`).join('')}</g>`) +
        k.mid + layer(3, L.hope({ x: w * 0.4, y: h - 20, s: 1.0, expr: 'cry', arms: 'up', item: `<g transform="translate(0,-118) scale(0.8)">${pr.pot(0, 0, 1, false)}</g>` }) + L.sweat(w * 0.48, 120, 1)) +
        layer(4, pr.stool(w * 0.8, h + 10, 1.4) + pr.stool(w * 0.1, h + 30, 1.3) + `<ellipse cx="${w * 0.4}" cy="${h - 10}" rx="90" ry="16" fill="${C.blue}" opacity=".8" ${st}/>`) +
        L.sfx(w * 0.68, 120, 'tách… tách…', { fs: 56, col: C.blue, rot: 6, sw: 6 }) +
        L.balloon(w * 0.58, h * 0.36, 300, 96, 'Quán ơi, đừng\nsập nha…', { fs: 28, tail: [w * 0.47, h * 0.4], shape: 'round' }); },
  ] },
  { y0: BOT[0], y1: BOT[1], cuts: [[700, 640], [1300, 1360]], scenes: [
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#CDB89A"/>${Array.from({ length: 9 }, (_, i) => `<path d="M${i * 80} -20 V${h}" stroke="#BCA582" stroke-width="10"/>`).join('')}`) +
      layer(1, `<rect x="30" y="${h * 0.22}" width="${w - 60}" height="16" fill="${C.wood}" ${st}/><rect x="30" y="${h * 0.48}" width="${w - 60}" height="16" fill="${C.wood}" ${st}/>` + `<g opacity=".9">${'<path d="M120 ' + (h * 0.22) + ' q20 -50 60 0" fill="none" stroke="#9A8A6E" stroke-width="4" stroke-dasharray="6 6"/>'}</g>`) +
      layer(2, K.crate(w * 0.25, h * 0.86, 1.3) + K.crate(w * 0.75, h * 0.86, 1.3, K.carrot(0, -96, 0.9, 20)) + `<rect x="-20" y="${h * 0.86}" width="${w + 40}" height="200" fill="${C.floor}" ${st}/>`) +
      layer(3, L.hope({ x: w * 0.48, y: h - 30, s: 0.95, expr: 'sad', arms: 'down' }) + L.gloom(w * 0.48, 40, 1.1)) +
      layer(5, `<path d="M-20 ${h + 20} V${h - 70} q60 -40 120 0 V${h + 20}z" fill="${C.woodl}" ${st}/>`) +
      L.caption(20, 20, 300, 50, 'Ngày 3 · Kho gần trống', 24),
    (w, h) => layer(0, K.burst(w, h, w * 0.5, h * 0.45, '#BFE0EA', '#A9D4E2')) +
      layer(3, L.hope({ x: w * 0.42, y: h + 40, s: 1.25, expr: 'determined', arms: 'cheer' }) + `<g transform="translate(${w * 0.42 + 75},${h - 300})">${K.carrot(0, 0, 1.2, -20)}</g>` + L.sparkle(w * 0.75, 90, 1.4) + L.sparkle(w * 0.18, 150, 1)) +
      L.balloon(w * 0.55, h * 0.48, 270, 100, 'Ít thì\nnấu khéo!', { shape: 'shout', fs: 34, tail: [w * 0.5, h * 0.5] }),
    (w, h) => { const k = K.kitchen(w, h, { ctr: h * 0.68, sky: '#BFE0EA', windowInner: `<circle cx="80" cy="60" r="40" fill="${C.must}"/>` });
      return k.back + layer(2, L.granny({ x: w * 0.35, y: h * 0.72, s: 0.95, expr: 'love', arms: 'wave' }) + L.heart(w * 0.18, 140, 1) + L.heart(w * 0.52, 100, 0.8)) + k.mid +
        layer(4, K.bowl(w * 0.35, h * 0.68 + 6, 1.1) + L.hope({ x: w * 0.82, y: h + 120, s: 1.05, flip: true, expr: 'happy' })) +
        L.balloon(w * 0.45, 16, 280, 90, 'Ngon quá\ncháu ơi!', { fs: 30, tail: [w * 0.38, 150] }); },
  ] },
] };
