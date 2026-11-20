// Thu vien ve: lop giay (diorama), nhan vat, dao cu, bong thoai.
const INK = '#2B2321';
const C = { ink: INK, cream: '#FFF3DC', paper: '#FFFDF6', coral: '#E8604F', must: '#F2C14E', mint: '#8CC9AE', blue: '#9CC7D8', lilac: '#C9B3F0', pink: '#E9A0A0', skin: '#FBE0C3', hair: '#EFE7DA', green: '#7DB87A', blush: '#F2A0A6', wall: '#F3DFB8', walldot: '#E9CE9E', floor: '#E3BE93', wood: '#B97F52', woodl: '#D69B68', vest: '#4A3F3A', brown: '#6B4A3A', sky: '#E9B98C', night: '#4B4466' };
const P = (pts) => pts.map(p => p.join(',')).join(' ');
// ---------- bo loc giay theo do sau (0 xa .. 4 gan)
function defs() {
  let s = '';
  for (let d = 0; d <= 5; d++) {
    const dx = 1.5 + d * 1.6, dy = 3 + d * 2.6, blur = 1.5 + d * 1.5, op = (0.22 + d * 0.05).toFixed(2);
    const sat = [0.72, 0.85, 0.95, 1, 1, 1][d];
    const edge = d === 0 ? 0 : 3;
    s += `<filter id="pp${d}" x="-15%" y="-15%" width="130%" height="140%" color-interpolation-filters="sRGB">
      <feColorMatrix in="SourceGraphic" type="saturate" values="${sat}" result="g"/>
      ${edge ? `<feMorphology in="SourceAlpha" operator="dilate" radius="${edge}" result="dil"/>` : `<feMorphology in="SourceAlpha" operator="dilate" radius="0.1" result="dil"/>`}
      <feGaussianBlur in="dil" stdDeviation="${blur}" result="b"/><feOffset in="b" dx="${dx}" dy="${dy}" result="o"/>
      <feFlood flood-color="#2B1A10" flood-opacity="${op}"/><feComposite in2="o" operator="in" result="shadow"/>
      <feFlood flood-color="#FFFDF6"/><feComposite in2="dil" operator="in" result="edge"/>
      <feTurbulence type="fractalNoise" baseFrequency="0.85" numOctaves="2" seed="${3 + d}" result="n"/>
      <feColorMatrix in="n" type="matrix" values="0 0 0 0 0.35  0 0 0 0 0.25  0 0 0 0 0.15  0 0 0 ${d === 0 ? 0.10 : 0.07} 0" result="n2"/>
      <feComposite in="n2" in2="dil" operator="in" result="grain"/>
      ${d === 0 ? '<feFlood flood-color="#F3DFB8" flood-opacity="0.12"/><feComposite in2="SourceAlpha" operator="in" result="haze"/>' : ''}
      <feMerge><feMergeNode in="shadow"/><feMergeNode in="edge"/><feMergeNode in="g"/>${d === 0 ? '<feMergeNode in="haze"/>' : ''}${process.env.NO_GRAIN ? '' : '<feMergeNode in="grain"/>'}</feMerge></filter>`;
  }
  s += `<filter id="wob" x="-5%" y="-5%" width="110%" height="110%"><feTurbulence type="fractalNoise" baseFrequency="0.025" numOctaves="2" seed="5" result="n"/><feDisplacementMap in="SourceGraphic" in2="n" scale="3"/></filter>
  <radialGradient id="glowY"><stop offset="0" stop-color="#FFE27A" stop-opacity="0.95"/><stop offset="1" stop-color="#FFE27A" stop-opacity="0"/></radialGradient>
  <radialGradient id="candle"><stop offset="0" stop-color="#FFC46B" stop-opacity="0.85"/><stop offset="0.5" stop-color="#FFC46B" stop-opacity="0.25"/><stop offset="1" stop-color="#FFC46B" stop-opacity="0"/></radialGradient>
  <radialGradient id="vigPanel" cx="50%" cy="50%" r="75%"><stop offset="0.6" stop-color="#2B1A10" stop-opacity="0"/><stop offset="1" stop-color="#2B1A10" stop-opacity="0.35"/></radialGradient>`;
  return s;
}
const layer = (d, content, extra = '') => `<g filter="url(#pp${d})" ${extra}>${content}</g>`;
// ---------- mat (dau ban kinh R, tam 0,0)
function ringEye(cx, cy, rx, ry, left, sw) {
  const a0 = left ? -78 : -142, a1 = left ? -38 : -102;
  const p = a => [cx + rx * Math.cos(a * Math.PI / 180), cy + ry * Math.sin(a * Math.PI / 180)];
  const s = p(a1), e = p(a0);
  return `<ellipse cx="${cx}" cy="${cy}" rx="${rx}" ry="${ry}" fill="#FFFDF6"/><path d="M${s[0].toFixed(1)} ${s[1].toFixed(1)} A${rx} ${ry} 0 1 1 ${e[0].toFixed(1)} ${e[1].toFixed(1)}" fill="none" stroke="${INK}" stroke-width="${sw}" stroke-linecap="round"/>`;
}
function face(R, expr = 'neutral', o = {}) {
  const ex = R * 0.37, ey = o.eyeY ?? R * 0.02, rx = R * 0.22, ry = R * 0.25, sw = Math.max(2.5, R * 0.07);
  const k = R / 60; let s = '';
  const blush = `<ellipse cx="${-R * 0.62}" cy="${R * 0.32}" rx="${R * 0.16}" ry="${R * 0.09}" fill="${C.blush}" opacity="0.85"/><ellipse cx="${R * 0.62}" cy="${R * 0.32}" rx="${R * 0.16}" ry="${R * 0.09}" fill="${C.blush}" opacity="0.85"/>`;
  const eyes = (sc = 1, dy = 0) => ringEye(-ex, ey + dy, rx * sc, ry * sc, true, sw) + ringEye(ex, ey + dy, rx * sc, ry * sc, false, sw);
  const w = (y, up = true, wd = 1) => `<path d="M${-R * 0.2 * wd} ${y} q${R * 0.1 * wd} ${up ? R * 0.15 : -R * 0.15} ${R * 0.2 * wd} 0 q${R * 0.1 * wd} ${up ? R * 0.15 : -R * 0.15} ${R * 0.2 * wd} 0" fill="none" stroke="${INK}" stroke-width="${sw * 0.8}" stroke-linecap="round" stroke-linejoin="round"/>`;
  const L = (d) => `<path d="${d}" fill="none" stroke="${INK}" stroke-width="${sw * 0.9}" stroke-linecap="round"/>`;
  switch (expr) {
    case 'happy': s = eyes() + blush + w(R * 0.36); break;
    case 'neutral': s = eyes() + L(`M${-R * 0.12} ${R * 0.42} h${R * 0.24}`); break;
    case 'sad': s = eyes(1, R * 0.05) + L(`M${-R * 0.6} ${-R * 0.32} L${-R * 0.18} ${-R * 0.42} M${R * 0.6} ${-R * 0.32} L${R * 0.18} ${-R * 0.42}`) + w(R * 0.5, false) + `<path d="M${-R * 0.42} ${R * 0.3} q${-R * 0.08} ${R * 0.16} 0 ${R * 0.2} q${R * 0.08} ${-R * 0.04} 0 ${-R * 0.2}z" fill="${C.blue}" stroke="${INK}" stroke-width="${sw * 0.5}"/>`; break;
    case 'shock': s = eyes(1.18, -R * 0.03) + `<ellipse cx="0" cy="${R * 0.48}" rx="${R * 0.1}" ry="${R * 0.13}" fill="#B8463C" stroke="${INK}" stroke-width="${sw * 0.7}"/>`; break;
    case 'cry': s = L(`M${-ex - rx} ${ey} q${rx} ${ry * 0.9} ${rx * 2} 0 M${ex - rx} ${ey} q${rx} ${ry * 0.9} ${rx * 2} 0`) + w(R * 0.45, false, 1.2) + `<path d="M${-ex} ${ey + ry * 0.5} v${R * 0.75} M${ex} ${ey + ry * 0.5} v${R * 0.75}" stroke="${C.blue}" stroke-width="${R * 0.12}" stroke-linecap="round"/>`; break;
    case 'beg': s = eyes(1.1) + L(`M${-R * 0.62} ${-R * 0.28} L${-R * 0.2} ${-R * 0.42} M${R * 0.62} ${-R * 0.28} L${R * 0.2} ${-R * 0.42}`) + `<ellipse cx="0" cy="${R * 0.45}" rx="${R * 0.12}" ry="${R * 0.08}" fill="#B8463C" stroke="${INK}" stroke-width="${sw * 0.6}"/>` + `<circle cx="${-ex + rx * 0.3}" cy="${ey + ry * 0.4}" r="${R * 0.05}" fill="${C.blue}"/><circle cx="${ex - rx * 0.3}" cy="${ey + ry * 0.4}" r="${R * 0.05}" fill="${C.blue}"/>`; break;
    case 'determined': s = eyes(0.95) + L(`M${-R * 0.62} ${-R * 0.42} L${-R * 0.18} ${-R * 0.28} M${R * 0.62} ${-R * 0.42} L${R * 0.18} ${-R * 0.28}`) + L(`M${-R * 0.15} ${R * 0.42} q${R * 0.15} ${-R * 0.08} ${R * 0.3} 0`); break;
    case 'cold': s = ringEye(-ex, ey, rx, ry * 0.9, true, sw) + ringEye(ex, ey, rx, ry * 0.9, false, sw) + `<rect x="${-ex - rx - 2}" y="${ey - ry}" width="${rx * 2 + 4}" height="${ry * 0.95}" fill="${o.skin || C.skin}"/><rect x="${ex - rx - 2}" y="${ey - ry}" width="${rx * 2 + 4}" height="${ry * 0.95}" fill="${o.skin || C.skin}"/>` + L(`M${-ex - rx} ${ey - ry * 0.05} H${-ex + rx} M${ex - rx} ${ey - ry * 0.05} H${ex + rx}`) + `<circle cx="${-ex}" cy="${ey + ry * 0.35}" r="${R * 0.06}" fill="${INK}"/><circle cx="${ex}" cy="${ey + ry * 0.35}" r="${R * 0.06}" fill="${INK}"/>`; break;
    case 'smirk': s = ringEye(-ex, ey, rx, ry * 0.9, true, sw) + ringEye(ex, ey, rx, ry * 0.9, false, sw) + `<rect x="${-ex - rx - 2}" y="${ey - ry}" width="${rx * 2 + 4}" height="${ry * 0.9}" fill="${o.skin || C.skin}"/><rect x="${ex - rx - 2}" y="${ey - ry}" width="${rx * 2 + 4}" height="${ry * 0.9}" fill="${o.skin || C.skin}"/>` + L(`M${-ex - rx} ${ey - ry * 0.1} H${-ex + rx} M${ex - rx} ${ey - ry * 0.1} H${ex + rx}`) + L(`M${-R * 0.2} ${R * 0.45} q${R * 0.25} ${R * 0.06} ${R * 0.4} ${-R * 0.12}`); break;
    case 'angry': s = eyes(1, R * 0.04) + L(`M${-R * 0.66} ${-R * 0.44} L${-R * 0.16} ${-R * 0.26} M${R * 0.66} ${-R * 0.44} L${R * 0.16} ${-R * 0.26}`) + w(R * 0.52, false); break;
    case 'love': s = `<path d="M${-ex} ${ey + 6 * k} c${-14 * k} ${-12 * k} ${-14 * k} ${-26 * k} 0 ${-18 * k} c${14 * k} ${-8 * k} ${14 * k} ${6 * k} 0 ${18 * k}z M${ex} ${ey + 6 * k} c${-14 * k} ${-12 * k} ${-14 * k} ${-26 * k} 0 ${-18 * k} c${14 * k} ${-8 * k} ${14 * k} ${6 * k} 0 ${18 * k}z" fill="${C.coral}" stroke="${INK}" stroke-width="${sw * 0.6}"/>` + blush + w(R * 0.38); break;
  }
  return s;
}
// ---------- chi tay ong cao su
const hose = (d, col, r, sw = 3) => `<path d="${d}" fill="none" stroke="${INK}" stroke-width="${r * 2 + sw * 2}" stroke-linecap="round"/><path d="${d}" fill="none" stroke="${col}" stroke-width="${r * 2}" stroke-linecap="round"/>`;
const glove = (x, y, r) => `<circle cx="${x}" cy="${y}" r="${r}" fill="#FFFFFF" stroke="${INK}" stroke-width="3"/>`;
const ARMS = {
  down: (L, R2) => [`M${L[0]} ${L[1]} q-14 30 -6 64`, `M${R2[0]} ${R2[1]} q14 30 6 64`],
  up: (L, R2) => [`M${L[0]} ${L[1]} q-34 -20 -36 -70`, `M${R2[0]} ${R2[1]} q34 -20 36 -70`],
  wave: (L, R2) => [`M${L[0]} ${L[1]} q-14 30 -6 64`, `M${R2[0]} ${R2[1]} q30 -10 34 -66`],
  beg: (L, R2) => [`M${L[0]} ${L[1]} q30 10 44 -14`, `M${R2[0]} ${R2[1]} q-30 10 -44 -14`],
  hold: (L, R2) => [`M${L[0]} ${L[1]} q4 30 34 36`, `M${R2[0]} ${R2[1]} q-4 30 -34 36`],
  point: (L, R2) => [`M${L[0]} ${L[1]} q-14 30 -6 64`, `M${R2[0]} ${R2[1]} q40 -4 78 -10`],
  give: (L, R2) => [`M${L[0]} ${L[1]} q-14 30 -6 64`, `M${R2[0]} ${R2[1]} q30 20 70 14`],
  cheer: (L, R2) => [`M${L[0]} ${L[1]} q-30 -26 -30 -74`, `M${R2[0]} ${R2[1]} q30 -26 30 -74`],
};
function armEnds(d) { const m = d.match(/M([-\d.]+) ([-\d.]+) q([-\d.]+) ([-\d.]+) ([-\d.]+) ([-\d.]+)/); return [+m[1] + +m[5], +m[2] + +m[6]]; }
// ---------- nhan vat (goc toa do = giua chan)
function hope(o = {}) {
  const s = o.s || 1, flip = o.flip ? -1 : 1, expr = o.expr || 'neutral', outfit = o.outfit || 'apron', arms = o.arms || 'down';
  const shirt = outfit === 'office' ? '#FFFDF6' : C.mint;
  let g = hose('M-18 -42 v38', '#5A6B8C', 8) + hose('M18 -42 v38', '#5A6B8C', 8) + `<ellipse cx="-22" cy="-2" rx="18" ry="9" fill="${INK}"/><ellipse cx="22" cy="-2" rx="18" ry="9" fill="${INK}"/>`;
  const [a1, a2] = ARMS[arms]([-44, -140], [44, -140]);
  const armsSvg = hose(a1, shirt, 9) + hose(a2, shirt, 9) + glove(...armEnds(a1), 13) + glove(...armEnds(a2), 13);
  g += `<path d="M-50 -40 Q-54 -150 0 -158 Q54 -150 50 -40 Z" fill="${shirt}" stroke="${INK}" stroke-width="5"/>`;
  if (outfit === 'office') g += `<path d="M-8 -152 L0 -120 L8 -152 Z" fill="#5A7FB8" stroke="${INK}" stroke-width="3"/><path d="M-6 -122 L0 -78 L6 -122 Z" fill="#5A7FB8" stroke="${INK}" stroke-width="3"/>`;
  else g += `<path d="M-34 -128 H34 V-46 Q0 -36 -34 -46 Z" fill="${C.coral}" stroke="${INK}" stroke-width="4"/><path d="M-12 -98 c-6 -8 -18 -2 -12 8 l12 10 l12 -10 c6 -10 -6 -16 -12 -8z" fill="${C.cream}" stroke="${INK}" stroke-width="2"/>`;
  g += (arms === 'beg' || arms === 'hold' || arms === 'cheer' || arms === 'up') ? '' : armsSvg;
  g += `<g transform="translate(0,-218)"><circle r="60" fill="${C.skin}" stroke="${INK}" stroke-width="5"/>
    <path d="M-58 -10 Q-56 -66 0 -64 Q56 -66 58 -10 Q40 -40 0 -40 Q-40 -40 -58 -10 Z" fill="${C.brown}" stroke="${INK}" stroke-width="4"/>
    <path d="M6 -62 q14 -30 34 -18 q-16 2 -18 16" fill="${C.brown}" stroke="${INK}" stroke-width="4" stroke-linejoin="round"/>${face(60, expr)}</g>`;
  if (arms === 'beg' || arms === 'hold' || arms === 'cheer' || arms === 'up') g += armsSvg;
  if (o.item) g += `<g transform="translate(0,-110)">${o.item}</g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
function creditor(o = {}) {
  const s = o.s || 1, flip = o.flip ? -1 : 1, expr = o.expr || 'cold', arms = o.arms || 'down';
  let g = hose('M-26 -60 v56', '#3A3230', 10) + hose('M26 -60 v56', '#3A3230', 10) + `<rect x="-52" y="-12" width="44" height="14" rx="6" fill="${INK}"/><rect x="8" y="-12" width="44" height="14" rx="6" fill="${INK}"/>`;
  const [a1, a2] = ARMS[arms]([-66, -230], [66, -230]);
  const armsSvg = hose(a1, C.vest, 11) + hose(a2, C.vest, 11) + glove(...armEnds(a1), 15) + glove(...armEnds(a2), 15);
  g += `<rect x="-70" y="-250" width="140" height="200" rx="12" fill="${C.vest}" stroke="${INK}" stroke-width="5"/>
    <path d="M-18 -250 L0 -196 L18 -250 Z" fill="${C.cream}" stroke="${INK}" stroke-width="3"/><path d="M-6 -214 L0 -160 L6 -214 Z" fill="#B8463C" stroke="${INK}" stroke-width="3"/>
    <path d="M-50 -170 q50 34 100 0" fill="none" stroke="${C.must}" stroke-width="7"/><circle cx="0" cy="-152" r="7" fill="${C.must}" stroke="${INK}" stroke-width="2"/>${armsSvg}
    <g transform="translate(0,-310)"><rect x="-56" y="-62" width="112" height="124" rx="22" fill="${C.skin}" stroke="${INK}" stroke-width="5"/>${face(56, expr, { skin: C.skin })}
    <path d="M-26 22 q13 -10 26 0 q13 -10 26 0" fill="${C.brown}" stroke="${INK}" stroke-width="3" opacity="${expr === 'happy' || expr === 'shock' ? 0 : 1}"/>
    <rect x="-74" y="-72" width="148" height="20" rx="5" fill="${INK}"/><rect x="-46" y="-120" width="92" height="52" rx="8" fill="${INK}"/><rect x="-46" y="-84" width="92" height="10" fill="${C.must}"/></g>`;
  if (o.item) g += `<g transform="translate(${o.itemX ?? 90},${o.itemY ?? -200})">${o.item}</g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
function daughter(o = {}) {
  const s = o.s || 1, flip = o.flip ? -1 : 1;
  let g = hose('M-14 -36 v32', C.skin, 7) + hose('M14 -36 v32', C.skin, 7) + `<ellipse cx="-16" cy="-2" rx="14" ry="8" fill="${C.coral}" stroke="${INK}" stroke-width="3"/><ellipse cx="16" cy="-2" rx="14" ry="8" fill="${C.coral}" stroke="${INK}" stroke-width="3"/>`;
  const [a1, a2] = ARMS[o.arms || 'hold']([-36, -120], [36, -120]);
  g += `<path d="M-60 -34 Q-40 -140 0 -142 Q40 -140 60 -34 Q0 -18 -60 -34 Z" fill="${C.vest}" stroke="${INK}" stroke-width="5"/><path d="M-56 -40 Q0 -24 56 -40" fill="none" stroke="${C.must}" stroke-width="6"/>`;
  g += hose(a1, C.vest, 8) + hose(a2, C.vest, 8) + glove(...armEnds(a1), 12) + glove(...armEnds(a2), 12);
  g += `<g transform="translate(0,-196)"><circle cx="-46" cy="-40" r="22" fill="${C.brown}" stroke="${INK}" stroke-width="4"/><circle cx="46" cy="-40" r="22" fill="${C.brown}" stroke="${INK}" stroke-width="4"/>
    <circle r="56" fill="${C.skin}" stroke="${INK}" stroke-width="5"/><path d="M-54 -6 Q-52 -60 0 -60 Q52 -60 54 -6 Q30 -34 0 -26 Q-30 -34 -54 -6 Z" fill="${C.brown}" stroke="${INK}" stroke-width="4"/>
    <path d="M30 -58 l18 -14 l4 22 z M30 -58 l-4 -22 l22 10 z" fill="${C.must}" stroke="${INK}" stroke-width="3"/>${face(56, o.expr || 'happy')}</g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
function boss(o = {}) { // sep o cong ty
  const s = o.s || 1, flip = o.flip ? -1 : 1;
  const [a1, a2] = ARMS[o.arms || 'point']([-50, -150], [50, -150]);
  let g = hose('M-20 -40 v36', '#55505A', 9) + hose('M20 -40 v36', '#55505A', 9) + `<ellipse cx="-24" cy="-2" rx="18" ry="9" fill="${INK}"/><ellipse cx="24" cy="-2" rx="18" ry="9" fill="${INK}"/>`;
  g += `<ellipse cx="0" cy="-100" rx="66" ry="72" fill="#8F8A99" stroke="${INK}" stroke-width="5"/><path d="M-12 -168 L0 -138 L12 -168Z" fill="${C.cream}" stroke="${INK}" stroke-width="3"/>`;
  g += hose(a1, '#8F8A99', 10) + hose(a2, '#8F8A99', 10) + glove(...armEnds(a1), 14) + glove(...armEnds(a2), 14);
  g += `<g transform="translate(0,-226)"><ellipse rx="62" ry="58" fill="${C.skin}" stroke="${INK}" stroke-width="5"/><path d="M-62 -4 q-10 -20 6 -30 M62 -4 q10 -20 -6 -30" fill="none" stroke="${INK}" stroke-width="4"/>
    ${face(58, o.expr || 'angry')}<circle cx="-21" cy="1" r="17" fill="none" stroke="${INK}" stroke-width="4"/><circle cx="21" cy="1" r="17" fill="none" stroke="${INK}" stroke-width="4"/><path d="M-4 0 h8" stroke="${INK}" stroke-width="4"/></g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
function rival(o = {}) { // ke choi xau: hinh tam giac
  const s = o.s || 1, flip = o.flip ? -1 : 1;
  const [a1, a2] = ARMS[o.arms || 'hold']([-34, -170], [34, -170]);
  let g = hose('M-14 -50 v46', '#3A3550', 7) + hose('M14 -50 v46', '#3A3550', 7) + `<path d="M-30 0 l-6 -10 h26 z M30 0 l6 -10 h-26z" fill="${INK}"/>`;
  g += `<path d="M-46 -50 L0 -190 L46 -50 Z" fill="#7A6A9A" stroke="${INK}" stroke-width="5" stroke-linejoin="round"/><path d="M-30 -90 L0 -170 L30 -90 Z" fill="#B7A9D6" stroke="${INK}" stroke-width="3" stroke-linejoin="round"/>`;
  g += hose(a1, '#7A6A9A', 7) + hose(a2, '#7A6A9A', 7) + glove(...armEnds(a1), 11) + glove(...armEnds(a2), 11);
  g += `<g transform="translate(0,-246)"><path d="M-48 -30 Q0 -70 48 -30 L30 50 Q0 66 -30 50 Z" fill="${C.skin}" stroke="${INK}" stroke-width="5" stroke-linejoin="round"/>
    <path d="M-50 -26 Q-10 -80 52 -34 L40 -50 Q0 -78 -40 -50 Z" fill="${INK}"/>${face(46, o.expr || 'smirk', { eyeY: -6 })}
    <path d="M-26 22 q13 -12 26 0 q13 -12 26 0 l10 -10 M-26 22 l-10 -10" fill="none" stroke="${INK}" stroke-width="4" stroke-linecap="round"/></g>`;
  if (o.item) g += `<g transform="translate(0,-150)">${o.item}</g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
function granny(o = {}) {
  const s = o.s || 1, flip = o.flip ? -1 : 1;
  const [a1, a2] = ARMS[o.arms || 'wave']([-48, -130], [48, -130]);
  let g = `<path d="M-56 -10 Q-60 -150 0 -158 Q60 -150 56 -10 Z" fill="${C.mint}" stroke="${INK}" stroke-width="5"/>` + hose(a1, C.mint, 9) + hose(a2, C.mint, 9) + glove(...armEnds(a1), 13) + glove(...armEnds(a2), 13);
  g += `<g transform="translate(0,-214)"><circle cx="0" cy="-66" r="24" fill="${C.hair}" stroke="${INK}" stroke-width="4"/><circle r="58" fill="${C.skin}" stroke="${INK}" stroke-width="5"/>
    <path d="M-56 -8 Q-54 -62 0 -60 Q54 -62 56 -8 Q36 -36 0 -34 Q-36 -36 -56 -8 Z" fill="${C.hair}" stroke="${INK}" stroke-width="4"/>${face(58, o.expr || 'happy')}</g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
function guest(o = {}) { // khach chung chung
  const s = o.s || 1, flip = o.flip ? -1 : 1, col = o.col || C.blue, hair = o.hair || C.brown;
  let g = `<path d="M-46 -10 Q-50 -130 0 -136 Q50 -130 46 -10 Z" fill="${col}" stroke="${INK}" stroke-width="5"/>`;
  g += `<g transform="translate(0,-186)"><circle r="52" fill="${C.skin}" stroke="${INK}" stroke-width="5"/><path d="M-50 -8 Q-48 -56 0 -54 Q48 -56 50 -8 Q30 -30 0 -28 Q-30 -30 -50 -8 Z" fill="${hair}" stroke="${INK}" stroke-width="4"/>${face(52, o.expr || 'neutral')}</g>`;
  return `<g transform="translate(${o.x || 0},${o.y || 0}) scale(${s * flip},${s})">${g}</g>`;
}
// ---------- bieu tuong cam xuc (manpu)
const sweat = (x, y, s = 1) => `<path transform="translate(${x},${y}) scale(${s})" d="M0 0 q-14 22 0 30 q14 -8 0 -30z" fill="${C.blue}" stroke="${INK}" stroke-width="3"/>`;
const vein = (x, y, s = 1) => `<g transform="translate(${x},${y}) scale(${s})" fill="none" stroke="#D9503F" stroke-width="6" stroke-linecap="round"><path d="M-14 -4 q8 0 10 -10 M4 -14 q0 8 10 10 M14 4 q-8 0 -10 10 M-4 14 q0 -8 -10 -10"/></g>`;
const sparkle = (x, y, s = 1, col = C.must) => `<path transform="translate(${x},${y}) scale(${s})" d="M0 -18 l5 13 l13 5 l-13 5 l-5 13 l-5 -13 l-13 -5 l13 -5z" fill="${col}" stroke="${INK}" stroke-width="2.5" stroke-linejoin="round"/>`;
const heart = (x, y, s = 1, col = C.coral) => `<path transform="translate(${x},${y}) scale(${s})" d="M0 18 C-26 0 -30 -14 -22 -22 C-14 -30 -4 -26 0 -16 C4 -26 14 -30 22 -22 C30 -14 26 0 0 18Z" fill="${col}" stroke="${INK}" stroke-width="3" stroke-linejoin="round"/>`;
const gloom = (x, y, s = 1) => `<g transform="translate(${x},${y}) scale(${s})" stroke="#7A6A9A" stroke-width="4" stroke-linecap="round"><path d="M-30 0 v40 M-10 -6 v50 M10 -6 v50 M30 0 v40"/></g>`;
const speed = (x, y, w, n = 5, col = INK) => `<g stroke="${col}" stroke-width="4" stroke-linecap="round">${Array.from({ length: n }, (_, i) => `<path d="M${x} ${y + i * 18} h${w * (0.6 + 0.4 * ((i * 37) % 10) / 10)}"/>`).join('')}</g>`;
// ---------- bong thoai, khung loi dan, chu hieu ung
function balloon(x, y, w, h, text, o = {}) {
  const tx = o.tail ? o.tail[0] : x + w * 0.3, ty = o.tail ? o.tail[1] : y + h + 40, fs = o.fs || 30, shape = o.shape || 'round';
  const lines = text.split('\n');
  let body;
  if (shape === 'shout') {
    const pts = []; const n = 22; for (let i = 0; i < n; i++) { const a = i / n * Math.PI * 2, r = i % 2 ? 1 : 1.16; pts.push([x + w / 2 + Math.cos(a) * w / 2 * r, y + h / 2 + Math.sin(a) * h / 2 * r]); }
    body = `<polygon points="${P(pts)}" fill="#FFFFFF" stroke="${INK}" stroke-width="5" stroke-linejoin="round"/>`;
  } else if (shape === 'think') {
    body = `<ellipse cx="${x + w / 2}" cy="${y + h / 2}" rx="${w / 2}" ry="${h / 2}" fill="#FFFFFF" stroke="${INK}" stroke-width="5" stroke-dasharray="${shape === 'think' ? '0' : '0'}"/>` + `<circle cx="${tx + (x + w * 0.3 - tx) * 0.4}" cy="${ty + (y + h - ty) * 0.4}" r="10" fill="#fff" stroke="${INK}" stroke-width="4"/><circle cx="${tx}" cy="${ty}" r="6" fill="#fff" stroke="${INK}" stroke-width="4"/>`;
  } else {
    const bx = x + w * 0.42;
    body = `<path d="M${bx - 18} ${y + h - 6} L${tx} ${ty} L${bx + 22} ${y + h - 10}" fill="#FFFFFF" stroke="${INK}" stroke-width="5" stroke-linejoin="round"/><rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${Math.min(h / 2, 46)}" fill="#FFFFFF" stroke="${INK}" stroke-width="5"/><path d="M${bx - 14} ${y + h - 3} L${bx + 18} ${y + h - 3}" stroke="#FFFFFF" stroke-width="8"/>`;
  }
  const t = lines.map((l, i) => `<text x="${x + w / 2}" y="${y + h / 2 + (i - (lines.length - 1) / 2) * fs * 1.12 + fs * 0.34}" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="${fs}" fill="${INK}">${l}</text>`).join('');
  return layer(4, body + t);
}
const caption = (x, y, w, h, text, fs = 26, bg = C.must) => layer(4, `<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="8" fill="${bg}" stroke="${INK}" stroke-width="4" transform="rotate(-1.5 ${x + w / 2} ${y + h / 2})"/><text x="${x + w / 2}" y="${y + h / 2 + fs * 0.34}" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="${fs}" fill="${INK}" transform="rotate(-1.5 ${x + w / 2} ${y + h / 2})">${text}</text>`);
const sfx = (x, y, text, o = {}) => layer(4, `<text x="${x}" y="${y}" transform="rotate(${o.rot || -8} ${x} ${y})" text-anchor="middle" font-family="'Baloo 2', sans-serif" font-weight="800" font-size="${o.fs || 90}" fill="${o.col || C.coral}" stroke="${INK}" stroke-width="${o.sw || 8}" paint-order="stroke" stroke-linejoin="round" letter-spacing="2">${text}</text>`);
module.exports = { C, INK, P, defs, layer, face, hope, creditor, daughter, boss, rival, granny, guest, sweat, vein, sparkle, heart, gloom, speed, balloon, caption, sfx, hose, glove };
