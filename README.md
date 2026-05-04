# BubbleTown

BubbleTown is a colorful chibi-style 3D grid battle game inspired by classic Bomberman-style party games. Players move tile by tile, place bubble bombs, break soft blocks, collect power-ups, and try to survive the blast-filled arena.

The project currently uses low-cost placeholder art and procedural Unity primitives, but the goal is to shape it into a playful, readable, candy-colored prototype that can keep improving over time.

## Highlights

- 3D angled overhead battle view
- Cute chibi-style placeholder characters and candy-themed arenas
- Grid-based movement on the XZ plane
- Bubble bombs with countdowns, cross-shaped explosions, hard-wall blocking, soft-block destruction, and chain reactions
- Power-ups for bomb count, blast range, movement speed, shield, and temporary invincibility
- SinglePlayer route objective: blast through soft blocks and reach the exit marker
- AI Battle mode with a basic grid-aware opponent
- Local VS mode with shared-keyboard controls and Best of 3 scoring
- Player-facing menu flow: Main Menu, Mode Select, Map Select, Battle, Result
- Battle HUD with mode, map, timer, objective, player stats, item guide, retry, and main menu actions
- Built-in placeholder BGM/SFX with settings for volume, mute, and screen shake

## Game Modes

### SinglePlayer
Break soft blocks, open a route through the arena, and reach the highlighted exit marker.

### AI Battle
Fight a simple AI opponent that can move around the grid, avoid danger, and place bombs.

### Local VS
Two players share one keyboard and battle for round wins. The first player to reach the Best of 3 target wins the match.

## Controls

| Player | Move | Place Bomb |
| --- | --- | --- |
| Player 1 | `WASD` | `Space` |
| Player 2 | Arrow Keys | `Enter` or `RightControl` |

## Maps

- `Candy Park`: balanced candy-themed paths and colorful soft blocks
- `Open Field`: wider lanes with a few hard-wall islands for blast blocking
- `Jelly Maze`: tighter routes with a more maze-like feel

## Power-Ups

- `Bomb Slot`: place one more bomb at a time
- `Blast Range`: bombs reach farther
- `Speed Boots`: move faster on the grid
- `Shield`: block one explosion hit
- `Invincible`: gain short temporary safety after pickup

## Audio And Settings

BubbleTown includes generated placeholder audio so the prototype has immediate game feel:

- Menu, battle, and result BGM loops
- Button click, movement, bomb placement, explosion, pickup, defeat, victory, and character death SFX
- Settings popup with master volume, BGM volume, SFX volume, mute toggles, screen shake toggle, and reset defaults

These sounds are placeholders and can be replaced later with final original or licensed audio.

## How To Run

1. Open the project with Unity 2022.3 LTS.
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press Play.
4. Use `Start Game` to choose a mode and map.
5. Use `Guide` for controls and `Settings` for audio/game-feel options.

## Current Prototype Status

BubbleTown is currently a playable prototype rather than a finished game. The core loop is in place: choose a mode, enter a map, move on the grid, place bombs, trigger explosions, collect items, finish a round, and return to the menu or retry.

The current visuals are intentionally lightweight. Most assets are placeholder primitives or generated UI shapes, which keeps the project easy to iterate while the gameplay and presentation direction are still evolving.

## Project Structure

```text
Assets/
  Audio/                 Placeholder source folders
  Materials/             Character, gameplay, map, and UI materials
  Prefabs/               Gameplay, character, map, environment, and UI prefabs
  Resources/Audio/       Auto-loaded placeholder BGM and SFX
  Scenes/                MainMenu, ModeSelect, MapSelect, Battle, Result
  Scripts/               Gameplay, map, UI, camera, AI, managers, visuals
  UI/                    UI-related assets

Docs/                    Art direction, map themes, and visual style notes
```

## Documentation

- `Docs/Phase2_ArtDirection.md`: visual direction and asset guidelines
- `Docs/Phase2_VisualStyleGuide.md`: palette, material, lighting, and readability notes
- `Docs/CandyPark_MapTheme.md`: Candy Park theme reference
- `Docs/JellyMaze_MapTheme.md`: Jelly Maze theme reference
- `Docs/EnvironmentDecorationGuide.md`: decoration placement and readability notes

## Roadmap

- Replace placeholder audio with final original or licensed sound assets
- Replace primitive placeholder art with stronger reusable prefabs
- Improve AI behavior and game balance
- Polish menus, HUD, result screen, and feedback animations
- Continue refining maps so each mode feels distinct and readable
