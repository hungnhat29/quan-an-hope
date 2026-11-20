const L = require('./lib.js'), S = require('./scenes.js'); const { C, INK, layer } = L; const { pr, st } = S;
const TOP = [36, 520], BOT = [548, 1036];
module.exports = {
  title: 'Bị đuổi việc', chapter: 'Chương 1 · Bị đuổi việc', num: 1,
  tiers: [
    { y0: TOP[0], y1: TOP[1], cuts: [[840, 760]], scenes: [
      (w, h) => // van phong
        layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#D5DCE3"/>${Array.from({ length: 14 }, (_, i) => `<path d="M${i * 70} -20 V${h}" stroke="#C3CCD6" stroke-width="18"/>`).join('')}`) +
        layer(1, S.windowFrame(70, 40, 300, 220, `<rect x="0" y="0" width="600" height="400" fill="#BFE0EA"/>${S.cityFar(600, 300, '#9CB5C9', 2)}`) + `<rect x="480" y="60" width="160" height="110" rx="10" fill="#FFFDF6" ${st}/><path d="M500 150 l30 -40 l30 20 l30 -50 l30 30" fill="none" stroke="${C.coral}" stroke-width="7" stroke-linejoin="round"/><path d="M590 90 l30 -30 m-14 0 h14 v14" fill="none" stroke="${C.coral}" stroke-width="6"/>`) +
        layer(2, `<rect x="-20" y="${h - 110}" width="${w + 40}" height="140" fill="#B8C2CC" ${st}/>`) +
        layer(3, L.boss({ x: 520, y: h - 70, s: 1.02, flip: true, arms: 'point', expr: 'angry' }) + L.vein(560, 120, 1.2)) +
        layer(4, `<path d="M330 ${h - 150} H${w + 30} V${h + 20} H330Z" fill="#8E7B6E" ${st}/><rect x="380" y="${h - 190}" width="130" height="40" rx="4" fill="#FFFDF6" ${st}/><rect x="392" y="${h - 214}" width="110" height="26" rx="4" fill="#FFFDF6" ${st}/>`) +
        layer(4, L.hope({ x: 170, y: h + 60, s: 1.05, outfit: 'office', expr: 'shock' }) + L.sweat(250, 150, 1.2) + L.sweat(80, 180, 0.9)) +
        layer(5, pr.plant(w - 40, h + 20, 1.2)) +
        L.caption(24, 20, 270, 50, 'Thứ Hai · 9 giờ sáng', 24) +
        L.balloon(560, 20, 220, 120, 'CẬU BỊ\nĐUỔI!!', { shape: 'shout', fs: 38, tail: [600, 150] }),
      (w, h) => // pho luc hoang hon
        layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="${S.skyFill('dusk')}"/><circle cx="${w * 0.72}" cy="${h * 0.56}" r="90" fill="#F7D88A" ${st}/>` + S.cloud(200, 90, 1.1, '#FBD3B6') + S.cloud(860, 70, 0.9, '#FBD3B6')) +
        layer(1, S.cityFar(w, h * 0.72, '#E3A884', 3)) +
        layer(2, S.cityFar(w, h * 0.85, '#C98B6E', 7) + `<rect x="-20" y="${h * 0.84}" width="${w + 40}" height="200" fill="#B7836A" ${st}/>`) +
        layer(3, L.hope({ x: w * 0.46, y: h - 50, s: 0.95, outfit: 'office', expr: 'sad', arms: 'hold', item: pr.box(0, 46, 0.9) }) + L.gloom(w * 0.46, 30, 1.3)) +
        layer(4, `<path d="M${w - 170} ${h + 20} V60" stroke="${INK}" stroke-width="22"/><path d="M${w - 170} ${h + 20} V60" stroke="#5A4A44" stroke-width="12"/><path d="M${w - 170} 70 q-60 -10 -80 30" fill="none" stroke="${INK}" stroke-width="10"/>` + pr.lantern(w - 250, 130, 0.9, false)) +
        layer(5, `<path d="M-30 ${h + 20} q20 -130 120 -110 q40 -70 120 -20 q70 -10 70 130z" fill="${C.green}" ${st}/>`) +
        `<path d="M${w * 0.46 - 90} ${h - 40} q90 20 200 0" stroke="#000" stroke-opacity=".15" stroke-width="16" fill="none"/>` +
        L.caption(w - 330, h - 80, 290, 50, '…và thế là hết.', 26, C.cream),
    ] },
    { y0: BOT[0], y1: BOT[1], cuts: [[640, 700], [1300, 1240]], scenes: [
      (w, h) => // bong chu no do xuong
        layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="${S.skyFill('dusk')}"/>` + S.cloud(120, 60, 0.8, '#FBD3B6')) +
        layer(1, S.facade(w * 0.5, h - 40, 0.8, { old: true })) +
        layer(2, `<rect x="-20" y="${h - 50}" width="${w + 40}" height="80" fill="#B7836A" ${st}/>`) +
        layer(3, L.hope({ x: 150, y: h - 20, s: 0.95, outfit: 'office', expr: 'shock' }) + L.sweat(230, 130, 1)) +
        `<path d="M${w + 20} ${h} L170 ${h - 30} L170 ${h + 20} Z" fill="#2B1A10" opacity="0.32"/>` +
        layer(5, L.creditor({ x: w - 70, y: h + 150, s: 1.25, flip: true, expr: 'cold' })) +
        L.sfx(w * 0.42, 110, 'CỘP… CỘP…', { fs: 54, col: C.cream, rot: -6, sw: 7 }),
      (w, h) => // trao chia khoa
        layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#E8B48C"/>${Array.from({ length: 10 }, (_, i) => `<path d="M${w / 2} ${h * 0.4} L${i * w / 9 - 40} -40" stroke="#F2C9A3" stroke-width="40"/>`).join('')}`) +
        layer(1, S.facade(w * 0.5, h + 40, 0.75, { old: true })) +
        layer(3, L.creditor({ x: w * 0.68, y: h + 10, s: 1.05, flip: true, arms: 'give', expr: 'smirk', item: pr.key(0, 0, 1.3, 10), itemX: 80, itemY: -228 })) +
        layer(4, L.hope({ x: w * 0.2, y: h + 30, s: 1.0, expr: 'shock', outfit: 'office' }) + L.sparkle(w * 0.47, 190, 1.2)) +
        L.balloon(w * 0.3, 22, 360, 112, 'Quán này giờ là\ncủa cậu. Nấu đi.', { fs: 30, tail: [w * 0.6, 170] }),
      (w, h) => // can canh giay no
        layer(0, `<rect x="-20" y="-20" width="${w + 40}" height="${h + 40}" fill="#F7D88A"/>${Array.from({ length: 20 }, (_, i) => `<path d="M${w / 2} ${h / 2} L${Math.cos(i / 20 * 6.283) * 900 + w / 2} ${Math.sin(i / 20 * 6.283) * 900 + h / 2}" stroke="#F2C14E" stroke-width="30"/>`).join('')}`) +
        layer(2, L.hope({ x: w * 0.78, y: h + 140, s: 1.15, expr: 'beg', outfit: 'office' }) + L.sweat(w * 0.92, 140, 1.3) + L.sweat(w * 0.62, 200, 1)) +
        layer(5, `<g transform="translate(${w * 0.36},${h * 0.52}) rotate(-8)"><path d="M-170 -200 H170 V190 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 l-17 14 l-17 -14 Z" fill="#FFFDF6" ${st}/>
          <text y="-140" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="46" fill="${C.coral}">GIẤY NỢ</text>
          ${[['Ngày 4', '★'], ['Ngày 9', '★★'], ['Ngày 15', '★★★'], ['Ngày 22', '☠']].map(([d, k], i) => `<text x="-130" y="${-70 + i * 58}" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="36" fill="${INK}">${d}</text><text x="130" y="${-70 + i * 58}" text-anchor="end" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="32" fill="${i === 3 ? C.coral : '#B8862A'}">${k}</text><path d="M-130 ${-58 + i * 58} H130" stroke="${INK}" stroke-width="2" stroke-dasharray="6 8" opacity=".5"/>`).join('')}
          <circle cx="90" cy="150" r="34" fill="none" stroke="${C.coral}" stroke-width="7"/><text x="90" y="160" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="22" fill="${C.coral}">ĐÓNG</text></g>`) +
        L.caption(20, h - 70, 300, 50, 'Hạn trả: 4 lần', 26),
    ] },
  ],
};
