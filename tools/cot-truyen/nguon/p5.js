const L = require('./lib.js'), S = require('./scenes.js'), K = require('./kit.js'); const { C, INK, layer } = L; const { pr, st } = S;
const TOP = [36, 520], BOT = [548, 1036];
module.exports = { chapter: 'Chương 5 · Món cuối cùng', num: 5, tiers: [
  { y0: TOP[0], y1: TOP[1], cuts: [[820, 740]], scenes: [
    (w, h) => { const k = K.kitchen(w, h, { ctr: h * 0.78, sky: '#E8604F' });
      return k.back + `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#2A1E1A" opacity=".45"/>` +
        layer(1, `<path d="M${w * 0.32} 20 h${w * 0.36} v${h} h-${w * 0.36}z" fill="#FFE2A8" ${st}/>`) +
        `<path d="M${w * 0.32} ${h} L${w * 0.68} ${h} L${w + 40} ${h + 40} L-40 ${h + 40}Z" fill="#FFE2A8" opacity=".6"/>` +
        layer(3, L.creditor({ x: w * 0.5, y: h + 20, s: 1.15, expr: 'cold' }).replace(/fill="#FBE0C3"/g, 'fill="#3A2E2B"').replace(/fill="#4A3F3A"/g, 'fill="#2B2321"')) +
        `<g fill="#FFF3DC"><ellipse cx="${w * 0.5 - 24}" cy="${h + 20 - 1.15 * 310}" rx="14" ry="6"/><ellipse cx="${w * 0.5 + 24}" cy="${h + 20 - 1.15 * 310}" rx="14" ry="6"/></g>` +
        layer(5, L.guest({ x: 60, y: h + 200, s: 1.3, col: C.blue, expr: 'shock' }) + L.guest({ x: w - 50, y: h + 220, s: 1.3, col: C.lilac, expr: 'shock', flip: true })) +
        L.caption(20, 20, 260, 50, 'Ngày 21 · Hạn chót', 24, C.coral) +
        L.balloon(w * 0.64, 70, 250, 120, 'Một món.\nTuyệt hảo.', { fs: 30, tail: [w * 0.56, 200] }); },
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#E8604F"/>` + L.speed(-20, 30, w * 0.5, 22, '#F4A08F').replace(/<g /, '<g opacity=".8" ')) +
      layer(2, K.stove(w * 0.46, h * 0.86, 1.6, true) + `<rect x="-20" y="${h * 0.86}" width="${w + 40}" height="200" fill="${C.wood}" ${st}/>`) +
      layer(3, L.hope({ x: w * 0.46, y: h * 0.86 + 6, s: 1.0, expr: 'determined', arms: 'hold', item: `<g transform="translate(0,80)">${pr.pot(0, 0, 0.9, true)}</g>` })) +
      layer(5, `<g transform="translate(${w * 0.86},${h * 0.5}) rotate(-20)"><rect x="-14" y="-160" width="28" height="200" rx="10" fill="${C.wood}" ${st}/><ellipse cx="0" cy="-170" rx="44" ry="60" fill="#D8DEE6" ${st}/></g>`) +
      L.sfx(w * 0.22, 130, 'XÈO!', { fs: 100, col: C.must, rot: -12 }) + L.sfx(w * 0.8, h * 0.88, 'XÈO!!', { fs: 80, col: C.must, rot: 8 }) +
      L.balloon(w * 0.58, 20, 330, 92, 'Nấu bằng cả\nnhân phẩm!', { shape: 'shout', fs: 30, tail: [w * 0.5, 160] }),
  ] },
  { y0: BOT[0], y1: BOT[1], cuts: [[640, 700], [1300, 1240]], scenes: [
    (w, h) => layer(0, K.burst(w, h, w * 0.5, h * 0.55, '#FFF3DC', '#F7D88A', 26)) +
      layer(2, K.cloche(w * 0.5 - 180, h * 0.5, 0.6) ) +
      layer(3, pr.dish(w * 0.5, h * 0.78, 1.5, true) + L.sparkle(w * 0.2, h * 0.4, 1.6) + L.sparkle(w * 0.84, h * 0.3, 1.3) + L.sparkle(w * 0.7, h * 0.6, 0.9)) +
      layer(4, `<g transform="translate(${w * 0.5},70) rotate(-4)"><rect x="-200" y="-36" width="400" height="72" rx="36" fill="${C.lilac}" ${st}/><text y="16" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="44" fill="${INK}">★ HUYỀN THOẠI ★</text></g>`),
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#2B2321"/>${Array.from({ length: 30 }, (_, i) => `<path d="M${w / 2} ${h / 2} L${w / 2 + Math.cos(i / 30 * 6.283) * 900} ${h / 2 + Math.sin(i / 30 * 6.283) * 900}" stroke="#FFF3DC" stroke-width="${3 + (i % 3) * 3}"/>`).join('')}<ellipse cx="${w / 2}" cy="${h / 2}" rx="${w * 0.36}" ry="${h * 0.42}" fill="#2B2321"/>`) +
      layer(3, L.creditor({ x: w * 0.5, y: h + 270, s: 1.35, expr: 'shock', arms: 'hold', item: `<path d="M0 0 l-40 -80" stroke="${INK}" stroke-width="10" stroke-linecap="round"/><ellipse cx="-44" cy="-86" rx="18" ry="12" fill="#D8DEE6" ${st}/>`, itemX: 30, itemY: -160 })) +
      L.sfx(w * 0.5, 90, 'Ự…!!', { fs: 90, col: C.cream, rot: -4 }),
    (w, h) => layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#F2B6B6"/>` + Array.from({ length: 10 }, (_, i) => `<circle cx="${(i * 131) % w}" cy="${(i * 83) % h}" r="${20 + (i * 7) % 30}" fill="#F7CACA"/>`).join('')) +
      layer(1, `<ellipse cx="${w * 0.74}" cy="${h * 0.36}" rx="120" ry="115" fill="#FFF3DC" ${st}/><circle cx="${w * 0.6}" cy="${h * 0.6}" r="14" fill="#FFF3DC" ${st}/><circle cx="${w * 0.55}" cy="${h * 0.68}" r="8" fill="#FFF3DC" ${st}/><clipPath id="mem"><ellipse cx="${w * 0.74}" cy="${h * 0.36}" rx="112" ry="107"/></clipPath><g clip-path="url(#mem)" opacity=".85">${L.granny({ x: w * 0.74, y: h * 0.36 + 150, s: 0.75, expr: 'happy', arms: 'down' })}</g>`) +
      layer(3, L.creditor({ x: w * 0.38, y: h + 230, s: 1.2, expr: 'cry' })) +
      L.balloon(w * 0.02, 20, 330, 100, 'Vị này… giống hệt\nmón mẹ ta nấu…', { fs: 26, tail: [w * 0.3, 160] }) + `<text x="${w * 0.5}" y="${h * 0.12}" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="20" fill="#8A6F55"></text>`,
  ] },
] };
