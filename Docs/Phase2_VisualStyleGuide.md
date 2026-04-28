# Phase 2 Visual Style Guide

This guide keeps BubbleTown's Phase 2 art upgrades visually consistent while the project still uses low-cost Unity primitives, placeholder prefabs, and simple materials.

The goal is not realism. The target look is a cute, readable, colorful 3D toy-board battle game.

## 1. Visual Pillars

- Cute and rounded: avoid sharp realistic shapes unless they are softened by color or scale.
- Readable first: characters, bombs, explosions, items, hard walls, and soft walls must be distinguishable from the angled camera.
- Bright but controlled: use candy colors as accents, not on every surface equally.
- Soft toy lighting: warm highlights, gentle shadows, no harsh contrast.
- Original identity: do not copy official Bomberman, Crazy Arcade, Bubble Bobble, or other copyrighted character/logo/UI/music designs.

## 2. Core Palette

Use this as the shared project palette for new materials.

| Role | Hex | Usage |
| --- | --- | --- |
| Cream highlight | `#FFF2CC` | UI cards, hard wall highlights, item icon details |
| Warm shadow | `#5A4B42` | readable dark text, grounding shadows, small face details |
| Bubble cyan | `#2FE7D6` | Player1 accents, glow lanes, bomb highlights |
| Sky blue | `#67C7FF` | Player1 body, shield item, friendly UI |
| Candy yellow | `#FFD34D` | rewards, star accents, ready/go highlights |
| Coral red | `#FF6B5E` | Player2 accents, danger, defeat UI |
| Lime green | `#83E66B` | speed item, positive UI, park grass accents |
| Soft violet | `#9B8CFF` | AI body/accent, Jelly Maze theme |
| Jelly magenta | `#FF63B7` | breakable jelly blocks, special energy props |
| Navy dark | `#24324A` | bomb body, deep contrast, not as full black |

## 3. Color Rules

Use a simple `60 / 30 / 10` rule:

- 60% soft background color: floor, large UI panels, map base.
- 30% secondary theme color: walls, props, character body.
- 10% high-saturation accent: item icons, glows, buttons, feedback.

Readability rules:

- Gameplay objects should be more saturated than floor tiles.
- Explosions and item pickups can be brightest, but only for short moments.
- Do not place saturated cyan, yellow, magenta, and lime all in the same small area unless one is clearly dominant.
- Avoid pure black and pure white. Use navy/dark brown for shadows and cream for highlights.
- Keep team colors stable: Player1 stays blue/cyan, Player2 stays coral/orange, AI stays violet/purple.

## 4. Material Rules

Current project materials use Unity Standard-style simple materials. Keep that approach for now.

Recommended baseline material settings:

| Material Type | Albedo | Smoothness | Metallic | Emission |
| --- | --- | --- | --- | --- |
| Character body | saturated team color | `0.35 - 0.55` | `0` | off or very low |
| Character face | dark brown/navy | `0.2 - 0.35` | `0` | off |
| Floor tile | lower saturation theme color | `0.25 - 0.45` | `0` | off or tiny accent only |
| Hard wall | cream/violet stable base | `0.35 - 0.55` | `0` | low highlight only |
| Soft wall | brighter jelly color | `0.45 - 0.65` | `0` | subtle glow allowed |
| Bomb body | navy/dark candy color | `0.45 - 0.65` | `0` | countdown flash only |
| Explosion | orange/cyan/cream | `0.4 - 0.7` | `0` | active pulse |
| Items | strong type color | `0.5 - 0.75` | `0` | pulsing glow |
| UI sprites/panels | cream/candy colors | n/a | n/a | n/a |

Naming rules:

- Characters: `Mat_Character_<Role>_<Part>`
- Map tiles: `Mat_Tile_<Theme>_<Part>`
- Walls: `Mat_Wall_<Theme>_<HardOrSoft>_<Part>`
- Items: `Mat_Item_<ItemType>_<Part>`
- Bombs: `Mat_Bomb_<Part>_<ColorName>`
- Explosions: `Mat_Explosion_<Part>_<ColorName>`

## 5. Character Materials

Player1:

- Body: sky blue or bubble cyan.
- Accent: candy yellow or cream.
- Face: dark navy/brown.
- Effect glow: cyan, used briefly for pickup or bomb placement feedback.

Player2:

- Body: coral/orange.
- Accent: cream/yellow.
- Face: same dark face material as Player1.
- Effect glow: warm coral or yellow.

AI:

- Body: soft violet or deeper purple.
- Accent: coral/red visor or cyan tech detail.
- Face/visor: dark navy plus red/pink accent.
- Effect glow: violet/cyan.

Rules:

- Keep all character bodies slightly brighter than the floor.
- Use one accent marker on the front so facing direction is clear.
- Do not give characters complex texture noise yet; simple shapes read better from the current camera.

## 6. Map Materials

Candy Park:

- Floor: pastel grass green plus candy blue inset.
- Hard wall: cream body, white/cream top highlight, caramel shadow.
- Soft wall: jelly blue, pink, or orange variants.
- Props: trees, barrels, signs, and fences should stay outside the playable cells and use lower saturation than items.

Jelly Maze:

- Floor: dark violet base with cyan lane details.
- Hard wall: violet glass block with cyan or cream cap.
- Soft wall: magenta jelly cube with cyan stripes.
- Props: neon gates, glow tubes, crystals, and orbs should be visually secondary to bombs/explosions.

Map readability rules:

- Floor should never compete with bombs or explosions.
- Hard wall silhouettes should look heavier and more stable than soft walls.
- Soft walls can glow or shine, but should still read as destructible.
- Use decoration density sparingly: more props at the border, fewer near spawn corners.

## 7. Item Materials

Current item color ownership:

- Bomb count up: cyan body, cream plus icon, navy mini-bomb detail.
- Explosion range up: orange body, yellow cross/burst, pink spark detail.
- Move speed up: lime body, white arrow, cyan wing detail.
- Shield: sky blue body, white shield icon, cyan spark detail.
- Temporary invincible: gold body, cream star icon, pink aura detail.

Rules:

- Every item should have a distinct silhouette and color family.
- Use pulsing emission on item visuals, not on colliders/root objects.
- Keep item glows small enough that they do not hide the grid cell below.

## 8. UI Color Direction

UI should feel like a bright casual party game.

Recommended UI palette:

- Main panel fill: `#FFF2CC` with high opacity.
- Main panel border: cyan, orange, or lime depending on context.
- Primary button: sky blue/cyan.
- Secondary button: cream/yellow.
- Danger/defeat button or label: coral red.
- Success/victory label: lime green or candy yellow.
- Text: dark navy/brown, not pure black.

Rules:

- Use rounded panels and soft shadows.
- Keep button labels short and high contrast.
- Use one accent color per screen section.
- MainMenu and ModeSelect can be more decorative; BattleUI must stay clean and readable.

## 9. Lighting Setup

Recommended Battle scene baseline:

- Main Directional Light:
  - Rotation: about `X 50`, `Y -35`, `Z 0`
  - Color: warm white, around `#FFF4DD`
  - Intensity: `1.0 - 1.25`
  - Shadows: soft, medium strength
- Ambient Light:
  - Use sky/gradient ambient if available.
  - Sky color: pale blue, around `#BFEFFF`
  - Equator color: soft cream, around `#FFEBC2`
  - Ground color: muted violet/gray, around `#7C7198`
- Camera background:
  - Candy Park: light sky blue.
  - Jelly Maze: slightly deeper blue-violet, but not black.

Rules:

- Avoid flat gray ambient light.
- Avoid extremely dark shadows; this is a readable casual game.
- If props look muddy, raise ambient brightness before increasing material saturation.

## 10. Lightweight Post-Processing

Post-processing is optional. Do not add a new render pipeline package only for this task.

If the current project already supports post-processing or later moves to URP, use a very light stack:

- Bloom: low intensity, only to catch item/explosion/neon emission.
- Color Adjustments: slightly higher saturation and contrast.
- Vignette: very subtle, or off for Battle.
- Ambient Occlusion: subtle, only if it does not make the grid muddy.

Suggested starting values if URP is used later:

- Bloom intensity: `0.15 - 0.35`
- Bloom threshold: `1.0 - 1.3`
- Saturation: `+5` to `+10`
- Contrast: `+3` to `+8`
- Vignette intensity: `0 - 0.12`

## 11. Avoiding A Gray Or Messy Image

If the scene feels gray:

- Increase ambient sky brightness slightly.
- Use warmer directional light.
- Check that floor materials are not too dark or desaturated.
- Add cream highlights to walls and UI panels.

If the scene feels messy:

- Reduce decoration saturation before changing gameplay objects.
- Keep border props outside the playable grid.
- Use fewer hue families in one theme.
- Make item glow smaller and more type-specific.
- Keep explosions short-lived and brighter than everything else.

If gameplay readability is poor:

- Darken floor slightly or reduce floor saturation.
- Increase character/body contrast.
- Make hard walls taller/heavier than soft walls.
- Make soft walls brighter than hard walls but less bright than items.

## 12. Daily Implementation Checklist

Before committing a new visual upgrade, check:

- Does it use the project palette or a deliberate theme extension?
- Does it keep Player1, Player2, and AI readable?
- Does it preserve grid readability from the Battle camera?
- Does it avoid copying official copyrighted designs?
- Does it use existing naming rules?
- Does it keep gameplay-critical objects more readable than decorations?
- Does it work in both Candy Park and Jelly Maze lighting?
