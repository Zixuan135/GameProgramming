# Phase 2 Visual Style Guide

This guide keeps BubbleTown's placeholder art visually consistent while the project moves toward a more polished chibi battle game.

## Core Palette

Shared neutrals:

- Cream: `#FFF2CC`
- Dark navy: `#24324A`
- Cocoa shadow: `#5A4B42`
- Soft white: `#FFF9E8`

Gameplay accents:

- Player1 blue: `#67C7FF`
- Player2 coral: `#FF6B5E`
- AI violet: `#9B8CFF`
- Pickup cyan: `#2FE7D6`
- Pickup gold: `#FFD34D`
- Explosion orange: `#FF8A3D`
- Alert pink: `#FF63B7`

Theme accents:

- Candy Park grass: `#91E8A6`
- Candy Park sky/tile blue: `#7EDBFF`
- Jelly Maze violet: `#3D2E6B`
- Jelly Maze cyan glow: `#2EEAFF`
- Jelly Maze magenta jelly: `#FF54D1`

## Color Rules

- Gameplay-critical objects should be brighter than the floor.
- Explosions should be the brightest short-lived elements.
- Items should glow, but with smaller highlights than explosions.
- Hard walls should read heavier than soft walls.
- Decorations should be softer or farther from the center grid.
- Avoid using the same hue for floor, wall, item, and character at the same time.

## Material Rules

- Use simple Standard/URP-compatible materials.
- Prefer flat color plus emission over detailed textures.
- Keep metallic values near zero.
- Use smoothness for toy-like shine, not realism.
- Use emission only for gameplay feedback, neon props, items, explosions, or small highlights.

Recommended organization:

```text
Assets/Materials/Characters
Assets/Materials/Gameplay/Bombs
Assets/Materials/Gameplay/Explosions
Assets/Materials/Gameplay/Items
Assets/Materials/Map/CandyPark
Assets/Materials/Map/JellyMaze
Assets/Materials/Map/Shared
Assets/Materials/UI
```

## Character Materials

- Skin: warm peach
- Face: dark navy/cocoa
- Player1: blue body with gold accent
- Player2: coral body with gold accent
- AI: violet body with pink accent

Characters should remain readable against both Candy Park and Jelly Maze.

## Map Materials

Candy Park:

- Light pastel base
- Cream hard walls
- Blue jelly soft walls
- Soft shadows, not realistic grime

Jelly Maze:

- Darker floor base
- Cyan glow lanes
- Violet hard walls
- Magenta soft walls

## UI Direction

- Rounded card/panel shapes
- Cream panels with blue, orange, or green accents
- Large readable text
- Buttons should have hover/click feedback
- Keep HUD compact so the Battle grid remains visible

## Lighting

Current Battle lighting is managed by `BattleVisualStyleController`.

Preferred setup:

- Warm directional key light
- Soft shadows
- Slightly saturated ambient colors per theme
- Camera clear color should support the selected map theme

Avoid:

- Overly gray ambient light
- Excessive shadow strength
- Strong bloom-like glow before gameplay readability is stable

## Readability Checks

Before committing a visual change, verify:

- Player1, Player2, and AI are easy to distinguish.
- Bombs and explosion lines are clear from the Battle camera.
- Soft walls still read as destructible.
- Items are visible without looking like explosions.
- Decorations do not look interactive unless they are intentionally interactive.
- The change works in both Candy Park and Jelly Maze lighting.
