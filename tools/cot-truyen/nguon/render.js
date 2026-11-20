const { chromium } = require('playwright'); const fs = require('fs'); const { pageSVG, html } = require('./page.js');
(async () => {
  const b = await chromium.launch(); const pg = await b.newPage({ viewport: { width: 1920, height: 1080 } });
  for (const name of process.argv.slice(2)) {
    delete require.cache[require.resolve('./' + name)];
    const def = require('./' + name); const svg = pageSVG(def);
    fs.writeFileSync(`out/${name.replace('.js', '')}.svg`, svg);
    await pg.setContent(html(svg)); const ok = await pg.evaluate(async () => { await document.fonts.load('800 30px "Baloo 2"', 'Aaệ'); await document.fonts.ready; return document.fonts.check('800 30px "Baloo 2"', 'ệ'); }); await pg.waitForTimeout(200); console.log('font', ok);
    await pg.screenshot({ path: `out/${name.replace('.js', '')}.png` });
    console.log('rendered', name);
  }
  await b.close();
})();
