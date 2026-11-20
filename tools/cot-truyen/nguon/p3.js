const L = require('./lib.js'), S = require('./scenes.js'), K = require('./kit.js'); const { C, INK, layer } = L; const { pr, st } = S;
const TOP = [36, 520], BOT = [548, 1036];
const bulb = (x, y, s, on) => `<g transform="translate(${x},${y}) scale(${s})"><path d="M0 -400 V-80" stroke="${INK}" stroke-width="6"/>${on ? '<circle cy="0" r="140" fill="url(#glowY)"/>' : ''}<rect x="-26" y="-90" width="52" height="40" rx="6" fill="#8F8A99" ${st}/><circle cy="0" r="60" fill="${on ? '#FFE27A' : '#B6B0C2'}" ${st}/><path d="M-16 -10 q16 30 32 0" fill="none" stroke="${INK}" stroke-width="5"/></g>`;
module.exports = { chapter: 'Chương 3 · Đêm mất điện', num: 3, tiers: [
  { y0: TOP[0], y1: TOP[1], cuts: [[620, 560], [1240, 1300]], scenes: [
    (w, h) => { const k = K.kitchen(w, h, { window: false, ctr: h * 0.8 });
      return k.back + k.mid + layer(3, bulb(w * 0.5, h * 0.5, 1.2, false)) + `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#2A2445" opacity=".55"/>` +
        L.sfx(w * 0.55, h * 0.82, 'PHỤT!', { fs: 120, col: '#B6B0C2', rot: -8 }) + L.caption(20, 20, 300, 50, 'Ngày 8 · 19 giờ 02', 24); },
    (w, h) => `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#1E1A30"/>` +
      `<g transform="translate(${w * 0.5},${h * 0.52}) scale(1.6)">${L.face(60, 'shock').replace(/fill="#B8463C"/, 'fill="#1E1A30"')}</g>` + `<g transform="translate(${w * 0.82},${h * 0.7}) scale(0.9)">${L.face(60, 'neutral').replace(/<path[^>]*>$/, '')}</g>` +
      L.sweat(w * 0.7, h * 0.3, 1.3) + L.balloon(w * 0.12, 30, 200, 90, '…ơ?', { fs: 44, tail: [w * 0.36, 190] }) +
      `<text x="${w * 0.82}" y="${h * 0.9}" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="22" fill="#8A84A6">(khách)</text>`,
    (w, h) => { const k = K.kitchen(w, h, { window: false, ctr: h * 0.74 });
      return k.back + k.mid + layer(3, L.hope({ x: w * 0.5, y: h - 10, s: 1.0, expr: 'determined', arms: 'hold', item: pr.candle(0, 70, 1.1) })) +
        S.darkness(w, h, [[w * 0.5, h * 0.66, 260]], 0.7) + L.sparkle(w * 0.75, 110, 1.1, '#FFC46B') + L.balloon(w * 0.06, 24, 300, 96, 'Bếp vẫn\nphải đỏ lửa!', { fs: 30, tail: [w * 0.36, 170] }); },
  ] },
  { y0: BOT[0], y1: BOT[1], cuts: [[1120, 1040]], scenes: [
    (w, h) => { const k = K.kitchen(w, h, { ctr: h * 0.62, sky: '#2E2A4A', windowInner: `<circle cx="160" cy="70" r="30" fill="#FFF3DC"/>${[0, 1, 2, 3, 4].map(i => `<circle cx="${30 + i * 50}" cy="${30 + (i % 2) * 40}" r="4" fill="#FFF3DC"/>`).join('')}` });
      const guests = L.guest({ x: w * 0.2, y: h * 0.66, s: 0.85, col: C.blue, expr: 'happy' }) + L.guest({ x: w * 0.36, y: h * 0.66, s: 0.8, col: C.lilac, hair: '#3A3230', expr: 'love' }) + L.granny({ x: w * 0.52, y: h * 0.66, s: 0.8, expr: 'happy', arms: 'down' });
      return k.back + layer(2, guests) + k.mid + layer(3, pr.candle(w * 0.28, h * 0.62, 0.9) + pr.candle(w * 0.45, h * 0.62, 0.9) + pr.candle(w * 0.62, h * 0.62, 0.9) + K.stove(w * 0.82, h * 0.62, 1, true)) +
        layer(4, L.hope({ x: w * 0.8, y: h + 90, s: 1.1, flip: true, expr: 'determined', arms: 'hold', item: `<g transform="translate(0,70)">${pr.pot(0, 0, 0.7, false)}</g>` })) +
        S.darkness(w, h, [[w * 0.28, h * 0.5, 230], [w * 0.45, h * 0.5, 230], [w * 0.62, h * 0.5, 230], [w * 0.82, h * 0.55, 330]], 0.66) +
        layer(5, pr.lantern(w * 0.06, 60, 1, false) + `<path d="M-20 ${h + 20} q30 -120 120 -100 q30 -60 100 -20 q20 80 -20 140z" fill="#3E5A4A" ${st}/>`) +
        L.sfx(w * 0.7, h * 0.24, 'XÈO XÈO', { fs: 64, col: '#FFC46B', rot: -6, sw: 7 }) + L.caption(150, 20, 340, 50, 'Không điện, vẫn đông khách', 24); },
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#4B4466"/>` + `<circle cx="${w * 0.5}" cy="${h * 0.6}" r="330" fill="url(#candle)"/>`) +
      layer(3, L.guest({ x: w * 0.36, y: h + 30, s: 1.25, col: C.lilac, hair: '#3A3230', expr: 'love' }) + L.guest({ x: w * 0.72, y: h + 60, s: 1.2, col: C.pink, expr: 'happy', flip: true })) +
      layer(4, pr.candle(w * 0.55, h - 20, 1.4)) + L.heart(w * 0.2, 120, 1.2) + L.heart(w * 0.82, 90, 1) + L.heart(w * 0.55, 60, 0.8, C.pink) +
      L.balloon(w * 0.1, h * 0.36, 300, 90, 'Lãng mạn ghê!', { fs: 32, tail: [w * 0.3, h * 0.5] }),
  ] },
] };
