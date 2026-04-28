# Phase 2 Art Direction

This document describes BubbleTown's current visual direction. It is not a task checklist; it is a reference for keeping future assets consistent.

## Visual Pillars

- Cute chibi proportions
- Rounded, toy-like silhouettes
- Bright but readable colors
- Low-cost geometry first, replaceable prefabs later
- Original designs only, no official game assets or near-copies

## Object Style

Characters:

- Large head, small body, clear face direction
- Player1, Player2, and AI need distinct palettes
- Gameplay scripts stay on the character root
- Replaceable art stays under `CharacterVisual`

Bombs:

- Bubble-like rounded body
- Clear fuse/top detail
- Countdown flash should be readable but not harsh
- Root prefab: `Assets/Prefabs/Gameplay/Bombs/Bomb_Basic.prefab`

Explosions:

- Short-lived bubble/pop style
- Center, horizontal, and vertical cells can use different silhouettes
- Prefabs live under `Assets/Prefabs/Gameplay/Explosions`

Items:

- Small, readable, icon-like 3D shapes
- Floating/rotating/glow feedback is preferred
- Root prefabs live under `Assets/Prefabs/Gameplay/Items`

Walls:

- Hard walls should look permanent and heavier
- Soft walls should look breakable and lighter
- Wall visuals should fit one grid cell and preserve neighboring tile readability

UI:

- Rounded panels and buttons
- High-contrast labels
- Colorful but not noisy
- Current UI is IMGUI placeholder; Canvas or UI Toolkit can replace it later

## Themes

Candy Park:

- Pastel outdoor toy-board mood
- Cream hard walls, jelly soft walls, soft green/blue floor
- Props: fences, lollipop trees, toy barrels, candy lamps, clouds

Jelly Maze:

- Darker neon toy-lab mood
- Violet floor, cyan glow lanes, glassy hard walls, magenta soft walls
- Props: neon gates, crystals, glow tubes, beacons, holograms, data towers

## Asset Organization

```text
Assets/Materials/
  Characters/
  Gameplay/Bombs/
  Gameplay/Explosions/
  Gameplay/Items/
  Map/CandyPark/
  Map/JellyMaze/
  Map/Shared/
  UI/

Assets/Prefabs/
  Characters/
  Gameplay/Bombs/
  Gameplay/Explosions/
  Gameplay/Items/
  Map/CandyPark/
  Map/JellyMaze/
  Map/Shared/
  Environment/CandyPark/
  Environment/JellyMaze/
  UI/
```

## Naming Rules

Characters:

- `Character_Player1_Chibi`
- `Character_Player2_Chibi`
- `Character_AI_Chibi`

Gameplay:

- `Bomb_Basic`
- `Explosion_Center`
- `Explosion_Horizontal`
- `Explosion_Vertical`
- `Item_BombCountUp`
- `Item_ExplosionRangeUp`
- `Item_MoveSpeedUp`
- `Item_Shield`
- `Item_TemporaryInvincible`

Map:

- `Tile_Ground_<Theme>`
- `Wall_Hard_<ThemeOrStyle>`
- `Wall_Soft_<ThemeOrStyle>`
- Runtime instances: `Tile_xx_yy`, `Wall_Hard_xx_yy`, `Wall_Soft_xx_yy`

Environment:

- `Prop_CandyPark_*`
- `Prop_JellyMaze_*`

Materials:

- Prefix all authored materials with `Mat_`
- Include category or theme in the name when useful, for example `Mat_Item_Shield_Body_Sky` or `Mat_Tile_GrassPastel`

## Replacement Rules

When replacing primitive placeholder art:

- Preserve root prefab names unless there is a clear reason to rename.
- Keep gameplay scripts on root objects.
- Keep replaceable meshes and primitives under `VisualRoot` or another visual child.
- Keep colliders simple and aligned to the one-unit grid.
- Move assets with their `.meta` files to preserve references.
