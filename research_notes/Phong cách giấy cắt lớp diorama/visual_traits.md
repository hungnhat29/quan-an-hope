# Visual traits of paper craft, stop motion and layered diorama styles (for a cozy 3D cooking game)

Scope note: about 20 search/fetch calls. Several primary pages (AWN Spider-Verse, VGC Paper Mario interview, Fast Company and NPR/WJCT Harold Halibut, GDC Vault Tearaway talk) returned 403 or a login wall, so some claims come from search-result summaries of those pages rather than full reads. Those are marked "(snippet)". Anything without a source is in Inferences or Gaps.

## 1. Paper craft / cut paper: what makes an image read as "paper"

### Takeaway
Shipped games such as Tearaway, Paper Mario and Lumino City do not get the look from a paper texture laid on everything. It comes from paper *behaviour* (crease, bend, tear), studying or photographing real paper, and clearly separated cut-out layers. The cheapest 2D recipe found is base shape + light edge offset toward the light + dark edge offset away from it + a blurred Multiply shadow layer at the bottom + a subtle clipped grain texture.

### Cited Findings
- Media Molecule (Tearaway, GDC 2013 talk "Building the Touchy-Feely World of Tearaway", Rex Crowle) aimed for a "visually pleasing recreation of paper". They recreated how paper creases, bends when characters walk on it, and tears apart. — [Game Informer](https://www.gameinformer.com/b/news/archive/2013/03/28/media-molecule-talks-about-crafting-the-world-of-tearaway.aspx); talk listing: [GDC Vault](https://www.gdcvault.com/play/1017845/Building-the-Touchy-Feely-World)
- Tearaway's early look was low-poly. The team later "removed all these textures" and let the paper itself "unroll and unravel". They built in-game objects as if from real paper ("everything is paper"). (snippet) — [Pocket Gamer: Unfolding story, the making of Tearaway](https://www.pocketgamer.com/articles/057875/r/); [Gameranx: building Tearaway with real paper too](https://gameranx.com/updates/id/9306/article/holy-crap-media-molecule-is-actually-building-tearaway-with-real-paper-too/)
- Paper Mario team (Intelligent Systems): the series puts "great emphasis on conveying texture visually" and they "prepare actual materials for our reference". (snippet) — [VGC interview, Origami King](https://www.videogameschronicle.com/features/interviews/paper-mario-origami-king/)
- For Paper Mario: Color Splash, the art team bought and studied the texture of various kinds of paper so the game would read as paper. (snippet) — [Game Informer Afterwords: Color Splash](https://gameinformer.com/b/features/archive/2016/12/28/afterwords-paper-mario-color-splash)
- Origami King uses two clearly different material reads: flat "paper" characters/world versus folded 3D "origami" enemies. A reviewer noted how different the paper and origami textures look. (snippet) — [Shacknews review](https://www.shacknews.com/article/119427/paper-mario-the-origami-king-review-creased-lightning)
- Lumino City (State of Play): every level was a real physical model of paper, cardboard and glue. Together they form a ~10 ft (3 m) city with tiny lights and motors. It was shot with the Talos motion-control film camera rig (£2,500/day), which can get "within millimetres" of the models. The team needed ~150 shots in the time a film crew would take one per day. The character Lumi was built from photos of cut coloured paper, animated in Flash from separate paper parts. Code adds film grain and blur to the character so it sits in the photographed plate, and she has a cast shadow. — [MCV/Develop: Crafting the Lumino City](https://mcvuk.com/development-news/crafting-the-lumino-city); [Engadget](https://www.engadget.com/2014-11-15-lumino-city-is-a-handmade-puzzler-launching-this-month.html); SIGGRAPH 2015 talk "Making it Real: Crafting a World with Lumino City" — [SIGGRAPH history](https://history.siggraph.org/?p=71730)
- Lumino City pipeline: a rough virtual mock-up of each scene to test the puzzle first, then the physical set. — [MCV/Develop](https://mcvuk.com/development-news/crafting-the-lumino-city)
- 2D layered paper-cut recipe (Procreate tutorial). Light from top right, shadow toward bottom left. Per shape, bottom to top:
  1. Shadow: a copy of the dark edge, Gaussian blur ~2–3%, blend mode Multiply, opacity to taste.
  2. Light edge: base copy, brightness raised "a bit", optional desaturation, nudged 2–3 taps toward the light.
  3. Dark edge: base copy, darkened and desaturated "not too extreme", nudged 2–3 taps away from the light. The author calls this edge optional.
  4. Base shape.
  5. Clipped texture layer: sampled base colour slightly darkened (optionally more saturated), painted with a paper-grain brush in ~3 passes.

  The light edge "mimics the lighter edge often seen on real paper where light hits". The blurred shadow "grounds the paper cutout". — [Bokettori: Paper cut effect](https://bokettori.substack.com/p/paper-cut-effect-with-paper-tales)
- Torn or rough paper outline in a toon shader (Shader Graph devlog): displace vertices along the normal by thresholded noise (noise scale 4–8 for hardness). The noise is time-quantised with Sine → clamp 0–1 → Floor, so the edge jumps in steps rather than sliding ("jitter every few seconds"). Split UV islands make each face move separately, which produces the torn-edge look. — [Prasin Shrestha devlog](https://prasinshrestha.itch.io/some-toon-shader-demo-with-papery-thing/devlog/945680/making-time-based-jitter-for-papery-outlines)
- Ready-made shader building block: the "Lyuma 2D Paper Shader" includes a flat drop shadow drawn behind a 2D element. — [Booth listing](https://booth.pm/en/items/3194844)
- A stylized Shader Graph project put paper grain only into shadowed areas, plus a darker edge on the shadow. It notes this works best on meshes with soft normals and rounded edges. (snippet) — [Diana Ouyang stylized shader project](https://diana-ouyang-graphics.notion.site/Stylized-Shader-Project-22926a60090a485dba6e6063bd162119)

### Inferences
- Rule set for story scenes (now flat UI shapes). Convert each shape into a "card":
  - one flat fill colour from a small palette;
  - a 1–3 px lighter rim on the light-facing side;
  - an optional darker rim on the far side;
  - a soft drop shadow offset away from the light onto the layer below, kept the same direction for the whole scene;
  - a low-contrast grain texture clipped to the fill.

  This is the tutorial recipe translated to UI or sprites. Pixel values are my estimate; the source gives only "2–3 taps" and "2–3% blur".
- Drop-shadow size should grow with the depth gap between two layers. A consistent single light direction is what sells "one physical object". Mixed shadow directions break the read.
- Grain in shadows only (Ouyang) and "subtle texture" are both ways to avoid noise. Texture works best as a low-amplitude modulation, not as a visible pattern on lit faces.
- In 3D, Paper Mario and Tearaway show that motion sells paper more than surface does. Flipping or turning a flat card to reveal a thin edge, bending, crumpling and folding transitions all read as paper even with a plain texture.
- The common "white core" or paper-thickness edge (a thin pale band where paper is cut) matches the tutorial's "light edge". In 3D it can be done by giving cards real thickness (0.5–2% of the card width) with a near-white side material. This is my synthesis; no source gives a value.

### Gaps
- No primary source gave numeric drop-shadow offsets or blur radii for any shipped game (Paper Mario, Tearaway).
- No primary interview text on Eric Carle, Charley Harper or kirigami influences in games was found. These are commonly cited references, but unverified here.
- Paper Beast (Pixel Reef) was not researched. Its creatures are sometimes described as papercraft, but nothing was confirmed.
- Paper Mario: TTYD remake (2024) lighting and paper-shader changes were not researched.

## 2. Stop motion: frame rate, holds, boil, replacement, imperfection

### Takeaway
"Stop motion" mostly reads from timing: 24 fps base, characters on twos (12 unique poses/s), camera moves on ones. Add small pose-to-pose imperfection and no motion blur. Harold Halibut shows a game can fake it with smooth mocap animation reduced to a lower frame rate and stiffer joints. The key pitfall is a character on twos over a camera on ones without care, which makes the character appear to vibrate.

### Cited Findings
- Aardman: many projects are effectively 12 unique character poses per second, "but the camera moves are always on 24". They also use an in-between rate they call "one-and-a-half". — [Post Magazine, Wallace & Gromit: Vengeance Most Fowl](https://postmagazine.com/Publications/Post-Magazine/2024/November-December-2024/Animation-I-Wallace-Gromit-Vengeance-Most-Fowl-I.aspx)
- One claymation guide lists ParaNorman as 24 fps on ones and Shaun the Sheep as 24 fps on twos. If the camera pans or tilts, shoot on ones; otherwise a puppet on twos can appear to vibrate against the background. (secondary source) — [Scott Daros: claymation frame rates](https://www.scottdaros.com/claymation-frame-rates.html); definitions — [iD Tech: ones, twos and threes](https://www.idtech.com/blog/what-does-animating-on-ones-twos-and-threes-mean); [Stop Motion Studio manual: shooting on twos](https://help.stopmotionstudio.com/en/manual/Shooting_on_Twos)
- Laika: animators often work on twos, swapping face parts about 12 times per second ("Animating on twos gives us more pop"). Another Laika-related source says up to 24 printed faces per second, probably shots on ones. (snippet; numbers conflict) — [Hollywood Reporter: Coraline makers](https://hollywoodreporter.com/movies/movie-news/coraline-makers-reveal-how-they-863155); [Studio Daily: ParaNorman](https://studiodaily.com/2012/08/laika-expands-the-stop-motion-state-of-the-art-for-paranorman)
- Coraline was the first feature to use 3D-printed replacement faces. The faces attach to the skull with magnets and are hand-painted one by one. Reported counts conflict: 6,333 faces allowing ~207,000 expressions per Hollywood Reporter, versus ~20,000 in other sources (possibly counting face parts). (snippet) — [Hollywood Reporter](https://hollywoodreporter.com/movies/movie-news/coraline-makers-reveal-how-they-863155); [Post Magazine Coraline cover story](https://www.postmagazine.com/Publications/Post-Magazine/2009/February-1-2009/COVER-STORY-CORALINE-ANIMATED-VIA-STOP-MOTION.aspx)
- Spider-Verse (CG reference for stepped animation):
  - The film was "done primarily on twos, so we would hold every second frame." Animator Panko animated on every frame, baked, then deleted every other key.
  - A custom playblast tool output only every second frame for review.
  - Hair and cloth simulation had to run on ones or it "jumps all over the place". Mixing stepped and spline keys blocked publishing.
  - "There was no motion blur". A stepped echo or shifted-image treatment stood in for blur.
  - Smears and ink lines were limited to 1–2 frames.

  — [CG Spectrum](https://www.cgspectrum.com/blog/spider-man-into-the-spider-verse-how-they-got-that-mind-blowing-look)
- Spider-Verse directors: stepped timing "more often than not, it's 12 frames per second, but it's a fluid scale". Character performance was often on twos, camera moves often on ones. Miles is on twos more than experienced Spider-People, to make him feel clumsier. (snippet) — [Slashfilm](https://www.slashfilm.com/562748/into-the-spider-verse-technique); [AWN](https://www.awn.com/animationworld/creating-stylized-universe-sonys-spider-man-spider-verse)
- Harold Halibut (Slow Bros):
  - All sets, puppets and props were handmade, then photogrammetry-scanned into the game.
  - Movement is motion capture, which art director Ole Tillmann "roughs up" with lower frame rates and stiffer articulation to feel like imperfect stop motion.
  - Giving up real stop motion was "a pretty big decision" and caused internal argument.

  (snippet) — [Skwigly interview](https://skwigly.co.uk/harold-halibut-slow-bros-ole-tillmann-tactile-gaming); [RealityCapture case study](https://capturingreality.com/Article-RealityCapture-and-HaroldHalibut); [Fast Company](https://www.fastcompany.com/90132134/making-the-worlds-first-stop-motion-video-game); [Game Informer preview](https://www.gameinformer.com/preview/2024/02/07/the-stop-motion-adventure-11-years-in-the-making)
- Stepped-noise "boil" on edges can be made in-shader by flooring time so the noise pattern changes in discrete jumps. — [Prasin Shrestha devlog](https://prasinshrestha.itch.io/some-toon-shader-demo-with-papery-thing/devlog/945680/making-time-based-jitter-for-papery-outlines)

### Inferences
- Concrete rules for a game:
  - Sample character animation at 12 Hz (hold each pose ~83 ms). Use 8 Hz (on threes) for slow or idle holds.
  - Keep the camera, UI and screen-space effects on full frame rate, as Aardman does.
  - Quantise per-object, not globally, so gameplay input stays responsive.
  - Turn motion blur off.
  - For boil, re-randomise a tiny offset (sub-pixel to ~1 px, or <1° rotation) or a noise seed at the step rate, never continuously.
- Consistency matters more than the exact rate. All story characters should share one step clock (e.g. a global 12 Hz tick), so different elements do not jitter out of phase. Spider-Verse used a deliberate per-character difference only as storytelling (Miles).
- Physics or secondary motion (cloth, steam, liquid in a cooking game) may need to stay smooth or be stepped after simulation. Stepping it naively causes popping, as Spider-Verse found with hair and cloth.
- Replacement animation translates to swapping discrete face or mouth sprites/meshes (a limited expression set), not blendshape tweens. This is cheap and fits cozy story scenes.

### Gaps
- No primary source found on The Neverhood, Kirby's Epic Yarn or Yoshi's Crafted World animation frame rates.
- No numeric guidance on boil amplitude in stop motion (how far fingerprints or cloth "crawl"). Fantastic Mr Fox's fur crawl was not researched.
- No sourced information on practical stop-motion lighting setups (key/fill ratios) was gathered.

## 3. Layered diorama / shadow box / pop-up: depth planes, box lighting, miniature look

### Takeaway
A diorama reads as small and handmade through:
- a limited number of discrete depth planes;
- a sharp focus band with progressive blur (tilt-shift);
- a higher, look-down camera;
- slightly boosted saturation and contrast ("painted model" colour);
- real materials and small lights in the set, as in Lumino City and Yoshi's Crafted World.

Over-strong blur reads as a digital filter and loses the effect.

### Cited Findings
- Miniature faking or tilt-shift makes a real scene look like a scale model. Shallow depth of field signals "small", and this cue usually overrides conflicting scale cues, at least initially. — [Wikipedia: Miniature faking](https://en.wikipedia.com/wiki/Diorama_effect)
- Companion cues: more colour saturation and contrast to imitate bright model paint, and a high camera angle looking down. — [Wikipedia: Miniature faking](https://en.wikipedia.com/wiki/Diorama_effect); [Zoner tilt-shift guide](https://learn.zoner.com/the-tilt-shift-effect-make-it-mini/)
- Restraint: an extremely heavy blur reads as an obvious digital effect rather than believable shallow focus. — [Phlearn tilt-shift tutorial](https://phlearn.com/tutorial/tilt-shift-effect-photoshop/)
- Yoshi's Crafted World (Good-Feel, Unreal Engine 4): levels follow a "theme of things crafted in kindergarten", built from cardboard and craft materials. A flip-side mechanic shows the back of the set (tape, unpainted cardboard), as on a real stage set. — [Super Mario Wiki](https://www.mariowiki.com/Yoshi%27s_Crafted_World); developer interview — [Nintendo Everything](http://nintendoeverything.com/yoshis-crafted-world-devs-on-the-art-style-kindergarten-inspiration-flip-side-gimmick-more/). The Super Mario Wiki page was only partly read; the flip-side detail is general knowledge, consistent with the interview title, but not verified in full text.
- Lumino City's built diorama included tiny practical lights and motors. Characters were composited with film grain and blur to match the plate. — [Engadget](https://www.engadget.com/2014-11-15-lumino-city-is-a-handmade-puzzler-launching-this-month.html); [MCV/Develop](https://mcvuk.com/development-news/crafting-the-lumino-city)
- Kirby and the Forgotten Land is described by a reviewer as "soft-edged" and like "a diorama you're looking down into". — [Inverse review](https://www.inverse.com/gaming/kirby-the-forgotten-land-star-crossed-world-dlc-switch-2-review)

### Inferences
- Story-scene diorama rules:
  - Use 3–5 depth planes: back wall/sky, far set, mid action plane, foreground frame/props, optional "box lip" or proscenium.
  - Use parallax between planes on camera drift (small, slow drift sells it).
  - Cast soft shadows from each plane onto the next, larger for bigger gaps.
  - Add a warm interior key light from the top or top-front, a subtle vignette or darker box edges, and keep the focus band on the mid plane.
  - Blur foreground and background slightly rather than heavily.
  - The frame edge (box lip, book page, stage curtain) tells the viewer "this is an object".
- Pop-up book transitions (planes rotating up from flat on hinges) are a natural replacement for the current UI fades in story scenes. They also convey depth layering explicitly.
- On small screens (mobile), the depth-of-field and grain cues are the first to vanish or turn into mush. Plane separation (shadow plus value contrast between layers) and silhouette survive best.

### Gaps
- Moncage, Lost in Play, Paper Trail, Unravel, Gris: no developer sources on their layering found within budget.
- No concrete diorama lighting values (colour temperature, key angle, vignette strength) from a primary source.

## 4. Readability at small sizes and common pitfalls

### Takeaway
The sources support three points:
- Real-material behaviour and silhouettes do the heavy lifting; textures stay subtle.
- Timing must be consistent: characters stepped, camera smooth, no mixing that causes vibration.
- Effects like blur must be restrained.

### Cited Findings
- Tearaway's team removed textures in favour of honest paper behaviour. (snippet) — [Pocket Gamer](https://www.pocketgamer.com/articles/057875/r/)
- Mixing stepped characters with a smooth camera without care causes visible "vibration". Studios keep the camera on ones. — [Scott Daros](https://www.scottdaros.com/claymation-frame-rates.html); [Post Magazine](https://postmagazine.com/Publications/Post-Magazine/2024/November-December-2024/Animation-I-Wallace-Gromit-Vengeance-Most-Fowl-I.aspx)
- Stepped animation can look stiff ("you can tell it was posed on every second frame"). Simulated secondary motion breaks if stepped naively. — [CG Spectrum](https://www.cgspectrum.com/blog/spider-man-into-the-spider-verse-how-they-got-that-mind-blowing-look)
- Smear or effect frames over 1–2 frames "would overtake the animation". — [CG Spectrum](https://www.cgspectrum.com/blog/spider-man-into-the-spider-verse-how-they-got-that-mind-blowing-look)
- Heavy tilt-shift blur reads as fake. — [Phlearn](https://phlearn.com/tutorial/tilt-shift-effect-photoshop/)
- Paper-cut tutorial: keep the dark edge "not too extreme". Texture is a darker tint of the base colour, not a contrasting pattern. — [Bokettori](https://bokettori.substack.com/p/paper-cut-effect-with-paper-tales)

### Inferences
- Priority order for small screens:
  1. Clear layer separation by value and shadow.
  2. A strong flat-colour silhouette.
  3. Consistent light direction.
  4. Stepped timing.
  5. Edge rim.
  6. Grain or texture, which is the first thing to drop or scale down at low resolution.
- Use a world-space or screen-scaled grain so it does not alias into moiré when cards are small. Keep grain contrast low (roughly a few % luminance) so it does not compete with food and UI icons. These values are estimates, not sourced.
- Inconsistent jitter (different step clocks, continuous noise mixed with stepped noise, boil on the UI) reads as a bug, not a style. Use one global step clock and exempt interactive UI.

### Gaps
- No user-testing or published data on how paper or diorama styles read on mobile screen sizes was found.
