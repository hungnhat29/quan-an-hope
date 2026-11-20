const L = require('./lib.js'), S = require('./scenes.js'), K = require('./kit.js'); const { C, INK, layer } = L; const { pr, st } = S;
const TOP = [36, 520], BOT = [548, 1036];
module.exports = { chapter: 'Chương 4 · Tin đồn', num: 4, tiers: [
  { y0: TOP[0], y1: TOP[1], cuts: [[660, 720], [1300, 1240]], scenes: [
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="${S.skyFill('night')}"/><circle cx="${w * 0.8}" cy="80" r="44" fill="#FFF3DC" ${st}/>`) +
      layer(1, S.cityFar(w, h * 0.6, '#5E5680', 5)) +
      layer(2, `<rect x="-20" y="${h * 0.2}" width="${w * 0.75}" height="${h}" fill="#D9C6A2" ${st}/>` + pr.flyer(80, h * 0.4, 0.9, -8) + pr.flyer(200, h * 0.32, 0.9, 6) + pr.flyer(140, h * 0.62, 0.9, 3) + `<rect x="-20" y="${h - 40}" width="${w + 40}" height="60" fill="#6B6384" ${st}/>`) +
      layer(3, L.rival({ x: w * 0.6, y: h - 20, s: 1.05, expr: 'smirk', arms: 'hold', item: pr.flyer(0, 0, 0.8, -10) })) +
      layer(5, `<path d="M${w + 20} ${h + 20} q-40 -160 -120 -150 q-30 -50 -80 -10 q-40 40 -20 180z" fill="#3E5A4A" ${st}/>`) +
      L.sfx(w * 0.4, 90, 'hí hí hí', { fs: 52, col: C.lilac, rot: -6, sw: 6 }) + L.caption(20, h - 74, 300, 50, 'Ngày 9 · Tin đồn lan', 24, C.lilac),
    (w, h) => { const k = K.kitchen(w, h, { ctr: h * 0.72, sky: '#C9C2D6' });
      return k.back + layer(2, L.hope({ x: w * 0.5, y: h * 0.98, s: 0.9, expr: 'sad', arms: 'down' }) + L.gloom(w * 0.5, 20, 1.2)) + k.mid + layer(3, pr.stool(w * 0.25, h * 0.98, 1.2) + pr.stool(w * 0.55, h * 0.98, 1.2) + pr.stool(w * 0.82, h * 0.98, 1.2)) +
        layer(4, pr.flyer(w * 0.2, h * 0.86, 0.8, 70) + pr.flyer(w * 0.8, h * 0.84, 0.8, -60)) +
        `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#6B6384" opacity=".25"/>` +
        L.sfx(w * 0.78, 120, '…vắng tanh', { fs: 46, col: '#C9C2D6', rot: 4, sw: 6 }) + L.caption(20, 20, 260, 50, 'Ngày 10–13', 24); },
    (w, h) => layer(0, K.burst(w, h, w * 0.5, h * 0.5, '#E9A0A0', '#F2B6B6')) +
      layer(3, L.hope({ x: w * 0.36, y: h + 60, s: 1.15, expr: 'determined', arms: 'hold', item: pr.camera(0, -10, 1.3) }) + L.sparkle(w * 0.78, 300, 1.3)) +
      layer(4, `<g transform="translate(${w * 0.74},${h * 0.62}) rotate(6)"><rect x="-120" y="-90" width="240" height="170" rx="10" fill="${INK}" ${st}/><rect x="-108" y="-78" width="216" height="146" fill="#6B6384"/>${L.rival({ x: 0, y: 64, s: 0.45, expr: 'smirk', arms: 'hold', item: pr.flyer(0, 0, 0.8) })}<circle cx="-90" cy="-60" r="8" fill="${C.coral}"/><text x="-74" y="-52" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="20" fill="#fff">REC</text></g>`) +
      L.balloon(w * 0.06, 20, 290, 96, 'Bắt được\nrồi nhé!', { shape: 'shout', fs: 32, tail: [w * 0.3, 170] }),
  ] },
  { y0: BOT[0], y1: BOT[1], cuts: [[1060, 1000]], scenes: [
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="${S.skyFill('day')}"/>` + S.cloud(160, 70, 1, '#FFFDF6') + S.cloud(720, 60, 0.8)) +
      layer(1, S.cityFar(w, h * 0.62, '#A9C4D6', 6)) +
      layer(2, L.guest({ x: w * 0.12, y: h * 0.86, s: 0.8, col: C.mint, expr: 'angry' }) + L.guest({ x: w * 0.26, y: h * 0.84, s: 0.78, col: C.must, hair: '#3A3230', expr: 'angry' }) + L.granny({ x: w * 0.88, y: h * 0.86, s: 0.8, expr: 'angry', arms: 'down' }) + `<rect x="-20" y="${h * 0.84}" width="${w + 40}" height="200" fill="${C.floor}" ${st}/>`) +
      layer(3, L.rival({ x: w * 0.6, y: h - 30, s: 1.2, expr: 'shock', arms: 'up' }) + L.sweat(w * 0.68, 140, 1.4) + L.sweat(w * 0.52, 180, 1)) +
      layer(4, L.hope({ x: w * 0.3, y: h + 30, s: 1.1, expr: 'determined', arms: 'point' }) + pr.flyer(w * 0.48, h * 0.3, 0.8, 30) + pr.flyer(w * 0.74, h * 0.24, 0.8, -40) + pr.flyer(w * 0.8, h * 0.6, 0.8, 80)) +
      L.vein(w * 0.12, h * 0.36, 1) + L.vein(w * 0.88, h * 0.38, 1) +
      L.sfx(w * 0.62, 100, 'LỘ RỒI!!', { fs: 100, col: C.coral, rot: -6 }) + L.caption(20, 20, 220, 50, 'Ngày 14', 26),
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#F7D88A"/>` + S.cloud(160, 70, 0.8)) +
      layer(1, S.facade(w * 0.5, h * 0.78, 0.68, { lit: true }) + `<rect x="-20" y="${h * 0.78}" width="${w + 40}" height="200" fill="#C9A47E" ${st}/>`) +
      layer(2, pr.star(w * 0.24, 70, 1.2) + pr.star(w * 0.5, 40, 1.4) + pr.star(w * 0.76, 70, 1.2)) +
      layer(4, L.guest({ x: w * 0.15, y: h + 70, s: 0.95, col: C.blue, expr: 'happy' }) + L.guest({ x: w * 0.45, y: h + 90, s: 0.95, col: C.pink, hair: '#3A3230', expr: 'love' }) + L.guest({ x: w * 0.8, y: h + 70, s: 0.95, col: C.lilac, expr: 'happy', flip: true })) +
      L.caption(w * 0.2, h * 0.6, 420, 54, 'Ngày 15 · Danh tiếng trở lại!', 26, C.mint),
  ] },
] };
