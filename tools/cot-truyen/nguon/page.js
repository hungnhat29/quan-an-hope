const fs = require('fs'); const L = require('./lib.js'); const { C, INK, P } = L;
const W = 1920, H = 1080, M = 40, G = 26;
// tier: {y0,y1, cuts:[[xTop,xBottom],...]} -> polygons
function tierPanels(y0, y1, cuts) {
  const xs = [[M, M], ...cuts, [W - M, W - M]]; const out = [];
  for (let i = 0; i < xs.length - 1; i++) {
    const l = xs[i], r = xs[i + 1], gl = i === 0 ? 0 : G / 2, gr = i === xs.length - 2 ? 0 : G / 2;
    const poly = [[l[0] + gl, y0], [r[0] - gr, y0], [r[1] - gr, y1], [l[1] + gl, y1]];
    const x = Math.min(poly[0][0], poly[3][0]), X = Math.max(poly[1][0], poly[2][0]);
    out.push({ poly, x, y: y0, w: X - x, h: y1 - y0 });
  }
  return out;
}
let fontCSS = '';
function fonts() {
  if (fontCSS) return fontCSS;
  const base = process.env.FONTSOURCE || require('path').join(__dirname, '..', 'node_modules', '@fontsource');
  const ranges = { latin: 'U+0000-00FF,U+0131,U+0152-0153,U+02BB-02BC,U+02C6,U+02DA,U+02DC,U+0304,U+0308,U+0329,U+2000-206F,U+20AC,U+2122,U+2191,U+2193,U+2212,U+2215,U+FEFF,U+FFFD', 'latin-ext': 'U+0100-02BA,U+02BD-02C5,U+02C7-02CC,U+02CE-02D7,U+02DD-02FF,U+0304,U+0308,U+0329,U+1D00-1DBF,U+1E00-1E9F,U+1EF2-1EFF,U+2020,U+20A0-20AB,U+20AD-20C0,U+2113,U+2C60-2C7F,U+A720-A7FF', vietnamese: 'U+0102-0103,U+0110-0111,U+0128-0129,U+0168-0169,U+01A0-01A1,U+01AF-01B0,U+0300-0301,U+0303-0304,U+0308-0309,U+0323,U+0329,U+1EA0-1EF9,U+20AB' };
  for (const [fam, dir, w] of [['Baloo 2', 'baloo-2', 800], ['Be Vietnam Pro', 'be-vietnam-pro', 600]]) for (const sub of Object.keys(ranges)) {
    const f = `${base}/${dir}/files/${dir}-${sub}-${w}-normal.woff2`; if (!fs.existsSync(f)) continue;
    fontCSS += `@font-face{font-family:'${fam}';font-weight:${w};src:url(data:font/woff2;base64,${fs.readFileSync(f).toString('base64')}) format('woff2');unicode-range:${ranges[sub]}}`;
  }
  return fontCSS;
}
function pageSVG({ tiers, title, chapter, num }) {
  const panels = tiers.flatMap(t => tierPanels(t.y0, t.y1, t.cuts).map((p, i) => ({ ...p, scene: t.scenes[i] })));
  let body = '';
  panels.forEach((p, i) => {
    const id = `cp${i}`;
    body += `<clipPath id="${id}"><polygon points="${P(p.poly)}"/></clipPath>
    <polygon data-role="shadow" points="${P(p.poly)}" transform="translate(7,9)" fill="#2B1A10" opacity="0.28" filter="url(#soft)"/>
    <g data-role="panel" data-i="${i}" clip-path="url(#${id})"><g data-role="inner" transform="translate(${p.x},${p.y})">${p.scene ? p.scene(p.w, p.h, p) : ''}<rect data-role="vig" width="${p.w}" height="${p.h}" fill="url(#vigPanel)"/></g></g>
    <polygon data-role="border" points="${P(p.poly)}" fill="none" stroke="${INK}" stroke-width="7" stroke-linejoin="round"/>`;
  });
  // phan "pop-out" ngoai khung (ve sau khung)
  panels.forEach((p, i) => { if (p.scene && p.scene.pop) body += `<g data-role="pop" data-i="${i}" transform="translate(${p.x},${p.y})">${p.scene.pop(p.w, p.h)}</g>`; });
  const grain = `<filter id="grain"><feTurbulence type="fractalNoise" baseFrequency="0.9" numOctaves="3" seed="9"/><feColorMatrix values="0 0 0 0 0.3 0 0 0 0 0.2 0 0 0 0 0.1 0 0 0 0.09 0"/></filter>`;
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">
  <defs>${L.defs()}${grain}<filter id="soft"><feGaussianBlur stdDeviation="6"/></filter>
  <radialGradient id="lightMask"><stop offset="0" stop-color="#000"/><stop offset="0.55" stop-color="#000" stop-opacity="0.85"/><stop offset="1" stop-color="#000" stop-opacity="0"/></radialGradient>
  <radialGradient id="pageVig" cx="50%" cy="50%" r="70%"><stop offset="0.7" stop-color="#5A3A20" stop-opacity="0"/><stop offset="1" stop-color="#5A3A20" stop-opacity="0.22"/></radialGradient></defs>
  <rect data-role="bg" width="${W}" height="${H}" fill="#FBF1DD"/>${process.env.NO_GRAIN ? '' : `<rect data-role="bg" width="${W}" height="${H}" filter="url(#grain)"/>`}
  ${body}
  <g data-role="footer" font-family="'Baloo 2', sans-serif" font-weight="800"><text x="${M}" y="${H - 8}" font-size="22" fill="#8A6F55">${chapter}</text><text x="${W - M}" y="${H - 8}" font-size="22" fill="#8A6F55" text-anchor="end">— ${num} —</text></g>
  <rect data-role="top" width="${W}" height="${H}" fill="url(#pageVig)"/>${process.env.NO_GRAIN ? '' : `<rect data-role="top" width="${W}" height="${H}" filter="url(#grain)" opacity="0.6"/>`}
  </svg>`;
}
function html(svg) { return `<!doctype html><html><head><meta charset="utf-8"><style>${fonts()}html,body{margin:0;background:#FBF1DD}</style></head><body>${svg}</body></html>`; }
function pagePanels({ tiers }) { return tiers.flatMap(t => tierPanels(t.y0, t.y1, t.cuts)); }
module.exports = { pageSVG, pagePanels, html, W, H, M, G };
