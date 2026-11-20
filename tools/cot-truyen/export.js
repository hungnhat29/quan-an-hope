// Xuat 6 trang truyen (nguon/p1..p6.js) thanh lop PNG cho Unity (constitution Dieu 6: chu khong nam trong anh).
// Moi khung: lop pp0..pp5 (trong suot, khong cat theo khung, chua le 90 px de thi sai), lop toi goc (vig);
// moi trang: nen giay + bong khung (bg), vien khung (border), phan tran khung (pop), toan trang (top).
// Chu (bong thoai, chu thich, tuong thanh, bien hieu) -> JSON (toa do trang 1920x1080), Unity ve bang TextMeshPro.
// Chay: node export.js [p1 p2 ...]   (mac dinh ca 6 trang). Can Microsoft Edge (playwright-core, channel msedge).
const { chromium } = require('playwright-core');
const fs = require('fs');
const path = require('path');
const { pageSVG, pagePanels, html, W, H } = require('./nguon/page.js');

const OUT = path.join(__dirname, '..', '..', 'My project', 'Assets', 'QuanAnHope', 'Resources', 'CotTruyen');
const PREVIEW = path.join(__dirname, '..', '..', 'docs', 'cot-truyen', 'xuat');
const B = 90; // le quanh trang / khung (px)
process.env.NO_GRAIN = '1'; // hat giay: Unity phu mot lop chung (nen nhe anh PNG)

(async () => {
  const names = process.argv.slice(2).length ? process.argv.slice(2) : ['p1', 'p2', 'p3', 'p4', 'p5', 'p6'];
  fs.mkdirSync(OUT, { recursive: true });
  fs.mkdirSync(PREVIEW, { recursive: true });
  const browser = await chromium.launch({ channel: 'msedge' });
  const pg = await browser.newPage({ viewport: { width: W + 2 * B, height: H + 2 * B } });

  for (const name of names) {
    const def = require('./nguon/' + name + '.js');
    const n = def.num;
    const dir = path.join(OUT, 'Trang_' + n);
    fs.rmSync(dir, { recursive: true, force: true });
    fs.mkdirSync(dir, { recursive: true });
    await pg.setContent(html(pageSVG(def)));
    await pg.addStyleTag({ content: `html,body{background:transparent!important;margin:0}svg{display:block;margin:${B}px}` });
    await pg.evaluate(async () => { await document.fonts.load('800 30px "Baloo 2"', 'Aaệ'); await document.fonts.ready; });
    await pg.waitForTimeout(150);
    await pg.screenshot({ path: path.join(PREVIEW, `Trang_${n}_goc.png`), clip: { x: B, y: B, width: W, height: H } });

    // ---- chu -> JSON, roi xoa khoi SVG; gan do sau cho moi phan tu trong khung
    const info = await pg.evaluate(() => {
      const hex = (c) => {
        const m = /rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*([\d.]+))?\)/.exec(c || '');
        if (!m || m[4] === '0') return '';
        return '#' + [m[1], m[2], m[3]].map(v => (+v).toString(16).padStart(2, '0')).join('').toUpperCase();
      };
      const depthOf = (el) => { const g = el.closest('g[filter^="url(#pp"]'); return g ? +/pp(\d)/.exec(g.getAttribute('filter'))[1] : -1; };
      const texts = [];
      // Chu cai / so / dau cau -> TextMeshPro; ky hieu (★ ☠ ♥ ...) font Baloo 2 khong co -> giu trong anh.
      const safe = /[\p{L}\p{N}\s.,!?…:;'"()·—–\-+%\/]/u;
      for (const el of Array.from(document.querySelectorAll('svg text'))) {
        const raw = el.textContent;
        if (!/[\p{L}\p{N}]/u.test(raw)) continue;            // toan ky hieu: de nguyen trong anh
        let s = raw;
        const chars = Array.from(raw);
        if (chars.some(c => !safe.test(c))) {
          // giu ky hieu trong anh, chu cai trong suot; TMP chen <space> dung bang be rong ky hieu
          const k0 = Math.hypot(el.getCTM().a, el.getCTM().b);
          let idx = 0; s = '';
          const runs = [];
          for (const c of chars) {
            const sym = !safe.test(c);
            if (sym) s += `<space=${(el.getSubStringLength(idx, c.length) * k0).toFixed(1)}>`; else s += c;
            if (runs.length && runs[runs.length - 1].sym === sym) runs[runs.length - 1].t += c; else runs.push({ sym, t: c });
            idx += c.length;
          }
          el.textContent = '';
          for (const r of runs) {
            const ts = document.createElementNS('http://www.w3.org/2000/svg', 'tspan');
            ts.textContent = r.t;
            if (!r.sym) { ts.setAttribute('fill-opacity', '0'); ts.setAttribute('stroke-opacity', '0'); }
            el.appendChild(ts);
          }
        }
        const ctm = el.getCTM();
        const cs = getComputedStyle(el);
        const pt = new DOMPoint(+(el.getAttribute('x') || 0), +(el.getAttribute('y') || 0)).matrixTransform(ctm);
        const k = Math.hypot(ctm.a, ctm.b);
        const panel = el.closest('[data-role=panel]'), pop = el.closest('[data-role=pop]');
        if (s.trim().length) texts.push({
          s, x: +pt.x.toFixed(1), y: +pt.y.toFixed(1), size: +(parseFloat(cs.fontSize) * k).toFixed(1),
          rot: +(Math.atan2(ctm.b, ctm.a) * 180 / Math.PI).toFixed(2), anchor: cs.textAnchor || 'start',
          fill: hex(cs.fill), stroke: cs.stroke === 'none' ? '' : hex(cs.stroke), sw: +((parseFloat(cs.strokeWidth) || 0) * k).toFixed(1),
          ls: +((parseFloat(cs.letterSpacing) || 0) * k).toFixed(1), depth: depthOf(el),
          panel: panel ? +panel.dataset.i : pop ? +pop.dataset.i : -1, pop: !!pop && !panel,
        });
        if (s === raw) el.remove();
      }
      // do sau phan tu "loi" (khong nam trong lop pp nao): theo lop dung truoc no trong khung
      for (const inner of document.querySelectorAll('[data-role=inner]')) {
        let cur = 0;
        for (const ch of inner.children) {
          if (ch.dataset.role === 'vig') { ch.dataset.depth = 'vig'; continue; }
          const f = ch.getAttribute('filter');
          if (f && /pp\d/.test(f)) { cur = +/pp(\d)/.exec(f)[1]; ch.dataset.depth = cur; continue; }
          if (ch.querySelector('g[filter^="url(#pp"]')) { ch.dataset.depth = 'mix'; continue; }
          ch.dataset.depth = cur;
        }
      }
      const pops = Array.from(document.querySelectorAll('[data-role=pop]')).map(p => {
        const r = p.getBoundingClientRect(); return { i: +p.dataset.i, x: r.x, y: r.y, w: r.width, h: r.height };
      });
      return { texts, pops };
    });

    // an / hien theo vai tro
    const only = (sel) => pg.evaluate((sel) => {
      // display:none (khong dung visibility: Chromium van ve bo loc feTurbulence cua phan tu an)
      const svg = document.querySelector('svg');
      for (const el of svg.querySelectorAll('[data-role=inner] *')) el.style.display = '';
      for (const el of svg.querySelectorAll('[data-role]')) {
        const r = el.dataset.role;
        if (r === 'inner' || r === 'vig') continue;
        el.style.display = sel.roles.includes(r) && (sel.i === undefined || +el.dataset.i === sel.i) ? '' : 'none';
      }
      if (sel.depth === undefined) return true;
      const p = svg.querySelector(`[data-role=panel][data-i="${sel.i}"]`);
      p.removeAttribute('clip-path');
      const inner = p.querySelector('[data-role=inner]');
      const depthOf = (g) => +/pp(\d)/.exec(g.getAttribute('filter'))[1];
      let any = false;
      for (const ch of inner.children) {
        const d = ch.dataset.depth;
        if (d === 'mix') {
          for (const g of ch.querySelectorAll('g[filter^="url(#pp"]')) { const on = depthOf(g) === sel.depth; g.style.display = on ? '' : 'none'; any = any || on; }
        } else {
          const on = String(d) === String(sel.depth);
          ch.style.display = on ? '' : 'none';
          any = any || on;
        }
      }
      return any;
    }, sel);

    const shot = async (file, x, y, w, h) => {
      const cx = Math.max(0, Math.floor(x + B)), cy = Math.max(0, Math.floor(y + B));
      // be rong / cao chia het cho 4: Unity nen DXT / BC7 duoc (khong thi de nguyen RGBA32, ton bo nho)
      let cw = Math.min(W + 2 * B, Math.ceil(x + w + B)) - cx, ch = Math.min(H + 2 * B, Math.ceil(y + h + B)) - cy;
      cw -= cw % 4; ch -= ch % 4;
      await pg.screenshot({ path: path.join(dir, file), clip: { x: cx, y: cy, width: cw, height: ch }, omitBackground: true });
      return { file: path.basename(file, '.png'), x: cx - B, y: cy - B, w: cw, h: ch };
    };

    const json = { page: n, title: def.title, chapter: def.chapter, width: W, height: H, panels: [], pops: [], texts: [] };
    await only({ roles: ['bg', 'shadow'] }); json.bg = await shot('nen.png', 0, 0, W, H);
    await only({ roles: ['border'] }); json.border = await shot('vien.png', 0, 0, W, H);
    await only({ roles: ['top'] }); json.top = await shot('phu.png', 0, 0, W, H);

    const panels = pagePanels(def);
    for (let i = 0; i < panels.length; i++) {
      const p = panels[i];
      const pj = { i, poly: p.poly.flat(), x: p.x, y: p.y, w: p.w, h: p.h, layers: [], texts: [] };
      for (let d = 0; d <= 5; d++) {
        const any = await only({ roles: ['panel'], i, depth: d });
        if (!any) continue;
        const l = await shot(`k${i + 1}_lop${d}.png`, p.x - B, p.y - B, p.w + 2 * B, p.h + 2 * B);
        l.d = d; pj.layers.push(l);
      }
      await only({ roles: ['panel'], i, depth: 'vig' });
      pj.vig = await shot(`k${i + 1}_toi.png`, p.x, p.y, p.w, p.h);
      pj.texts = info.texts.filter(t => t.panel === i && !t.pop);
      json.panels.push(pj);
    }
    for (const pop of info.pops) {
      await only({ roles: ['pop'], i: pop.i });
      const m = 40;
      const pj = await shot(`k${pop.i + 1}_tran.png`, pop.x - B - m, pop.y - B - m, pop.w + 2 * m, pop.h + 2 * m);
      pj.i = pop.i; pj.texts = info.texts.filter(t => t.panel === pop.i && t.pop);
      json.pops.push(pj);
    }
    json.texts = info.texts.filter(t => t.panel === -1);
    fs.writeFileSync(path.join(OUT, `Trang_${n}.json`), JSON.stringify(json, null, 1));
    console.log(`Trang ${n}: ${json.panels.length} khung, ${json.panels.reduce((a, p) => a + p.layers.length, 0)} lop, ${info.texts.length} chu`);
  }
  await browser.close();
})().catch(e => { console.error(e); process.exit(1); });
