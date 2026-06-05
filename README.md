# BubbleTown

BubbleTown is a colorful chibi-style 3D grid battle game inspired by classic Bomberman-style party games. Players move tile by tile, place bubble bombs, break soft walls, collect power-ups, and survive compact toy-board arenas.

The current build is a playable Unity prototype with a complete menu-to-result flow, a guided Tutorial mode, AI Battle, Local VS, six selectable heroes, themed 3D runtime maps, illustrated UI screens, battle HUD overlays, placeholder audio, and persistent player-facing settings.

## Latest Snapshot

- Built with Unity `2022.3.62f3c1`.
- Full scene flow: Main Menu, Mode Select, Character Select, Map Select, AI Difficulty Select, Battle, and Result.
- Three playable modes: Tutorial, AI Battle, and Local VS. Tutorial is still backed by the internal `GameMode.SinglePlayer` enum for compatibility.
- Six original card-inspired chibi hero looks backed by `CharacterData` assets and clearer battle-ready character prefabs.
- Three selectable map themes: Candy Park, Snowfield, and Jelly Maze.
- Tutorial mode now guides the player through movement, bomb placement, escaping a blast, breaking route gates, picking up an item, and reaching the exit.
- Runtime-generated 3D map visuals now have stronger theme identity, material contrast, wall readability, and decorative borders.
- Power-up drops use icon-inspired 3D pickup models that match the illustrated item guide.
- Battle character prefabs use stronger silhouettes, per-character outline materials, larger facial features, and role-specific props that better match the character card artwork.
- Battle uses an illustrated full-screen background behind the arena, a Canvas-based left HUD, and a tutorial prompt panel when Tutorial is active.
- Menu, guide, settings, mode select, character select, map select, difficulty select, battle overlays, item guide, pause menu, and result page use image-driven UI assets when available.
- Mode Select now uses a transparent illustrated Tutorial card instead of the older Single Player card treatment.
- The Result screen uses the latest illustrated `ResultUI` artwork with aspect-correct drawing, masked dynamic fields, and aligned retry/main-menu image buttons.
- Built-in placeholder BGM/SFX cover menu, battle, result, movement, bombs, explosions, pickups, victory, defeat, and button feedback.

## How To Run

1. Open the project with Unity `2022.3.62f3c1` or a compatible Unity 2022.3 LTS editor.
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press Play.
4. Choose a mode, character, and map.
5. In AI Battle, choose Easy, Normal, or Hard before entering the battle.
6. In Tutorial, follow the on-screen prompt through movement, bombs, item pickup, and the exit route.

## Game Modes

| Mode | Goal |
| --- | --- |
| `Tutorial` | Learn movement, bombs, item pickup, route gates, and the exit objective step by step. Internally this is still `GameMode.SinglePlayer`. |
| `AI Battle` | Fight a grid-aware AI opponent with selectable difficulty. |
| `Local VS` | Two players share one keyboard and play a Best of 3 match. |

## Controls

| Player | Move | Place Bomb |
| --- | --- | --- |
| Player 1 | `WASD` | `Space` |
| Player 2 | Arrow Keys | `Enter` or `RightControl` |

Pause during battle with the on-screen `Pause` button, `Esc`, or `P`. From pause, players can resume, retry, return to the main menu, or open settings.

## Characters

Players can choose from six original chibi heroes. The current roster focuses on clear silhouettes, readable battle-scale details, and visual variety while keeping gameplay stats balanced:

- `Bubble Ranger`: blue bubble-helmet hero with a glass dome, side pads, tank pack, and bubble cannon.
- `Bear Blaster`: red bear-suit hero with rounded ears, cream muzzle, paw badges, pouch, and oversized blaster.
- `Frog Hopper`: green frog-hood hero with large eye bulbs, cheeky face details, jump tank, and pogo prop.
- `Gear Kid`: yellow engineer-style hero with a safety helmet, gear badges, tool pack, pouch, and chunky wrench.
- `Bunny Pop`: pink bunny-ear hero with long ears, bow, bunny muzzle, pouch, and candy-like blaster.
- `Star Mage`: purple magic-hat hero with a broad hat brim, star badges, cape, book, staff orb, and star charm.

The battle character prefabs are still built from lightweight Unity primitives, but they now use custom fixed materials, darker per-character outline ink, larger face windows, and bolder role props so they read closer to the supplied illustrated role cards. This keeps iteration fast while leaving a clear path to replace the generated forms with authored models later.

## Maps

| Display Name | Internal Type | Current Visual Direction |
| --- | --- | --- |
| `Candy Park` | `BattleMapType.Default` | Bright candy toy-board with cream wafer hard walls, blue jelly soft walls, candy lamps, rails, planets, signs, and pastel props. |
| `Snowfield` | `BattleMapType.OpenField` | Snow-and-ice playground with packed snow hard walls, warm gift-crate soft walls, icy tile insets, snowflake marks, fences, snowmen, pines, and winter props. |
| `Jelly Maze` | `BattleMapType.Maze` | Purple/cyan/pink jelly-lab board with violet hard walls, magenta soft walls, cyan star floor tiles, cream highlights, neon rails, crystals, beacons, and holo props. |

All three maps are generated at runtime by `MapGenerator`, while `MapManager` owns gameplay grid state, wall blocking, soft-wall destruction, item spawning, and objective state. In Tutorial, `MapManager` reshapes the selected theme into a guided route with a movement target, a teaching soft wall, multiple normal soft-wall gates, and a final exit objective so the player cannot finish by breaking only one wall.

## Power-Ups

- `Bomb Slot`: place one more bomb at a time.
- `Blast Range`: extend bomb explosion range.
- `Speed Boots`: increase movement speed.
- `Shield`: block one explosion hit.
- `Invincible`: gain short temporary safety after pickup.

Power-up drops are represented in battle by stylized 3D models inspired by their item-guide icons: bomb slot, blast range, speed boots, shield, and invincible star.

Tutorial mode guarantees a readable pickup from the teaching soft wall when possible, then keeps the prompt focused on breaking route gates and reaching the exit.

## UI And Flow

BubbleTown now mixes runtime UI construction with imported image assets stored under `Assets/Resources/UI`.

- Main menu, mode select, character select, map select, AI difficulty select, guide, settings, result, and battle overlays use illustrated textures when the full asset set is present.
- `SimpleUIFactory` still provides fallback drawing for screens that do not have every image asset loaded.
- `ModeSelectUI` renders image-backed Tutorial, AI Battle, Local VS, and Back controls, with the Tutorial card supplied as a transparent PNG so it matches the neighboring mode cards.
- `BattleCanvasHUD` owns the in-battle left HUD, item guide, pause panel, settings overlay, opening prompt, tutorial prompt, result prompt, and pickup toast.
- `BattleVisualStyleController` applies map-aware lighting and places the shared illustrated battle background behind the arena.
- The Result screen intentionally uses its own result artwork instead of the battle background, then redraws the dynamic outcome area and summary fields with masks so runtime result text does not overlap baked art.

## Audio And Settings

The prototype includes generated placeholder audio so it has immediate game feel:

- Menu, battle, and result BGM loops.
- Button click, movement, bomb placement, explosion, pickup, character death, victory, and defeat SFX.
- Settings popup with master volume, BGM volume, SFX volume, BGM/SFX mute toggles, preview playback, screen shake toggle, and reset defaults.
- Settings persist through `PlayerPrefs` via `GameSettings`.

These sounds are placeholders and can be replaced later with final original or licensed audio.

## Project Structure

```text
Assets/
  Audio/                 Source folders for generated placeholder audio
  Materials/             Character, gameplay, map, environment, and UI materials
  Prefabs/               Character, gameplay, map, environment, and UI prefabs
  Resources/
    Audio/               Runtime-loaded BGM and SFX clips
    Characters/          CharacterData assets used by character selection
    UI/                  Runtime-loaded illustrated UI textures
  Scenes/                MainMenu, ModeSelect, CharacterSelect, MapSelect, DifficultySelect, Battle, Result
  Scripts/
    AI/                  Grid-aware AI movement, danger avoidance, and bomb decisions
    Camera/              Battle camera framing and shake feedback
    Characters/          Player/character data, control, visuals, and pickup feedback
    Core/                Constants, settings, enums, and shared state definitions
    Gameplay/            Bomb and explosion behavior
    Items/               Power-up spawning, visuals, and pickup feedback
    Managers/            Game session, scene flow, and audio systems
    Map/                 Grid, map generation, visual themes, wall feedback, decorations
    UI/                  Menu screens, Canvas HUD, settings, guide, result UI
    Visuals/             Battle lighting and illustrated background controller

Docs/                    Art direction, map theme, and environment decoration notes
```

## Documentation

- `Docs/Phase2_ArtDirection.md`: overall visual direction and asset guidelines.
- `Docs/Phase2_VisualStyleGuide.md`: palette, material, lighting, and readability notes.
- `Docs/CandyPark_MapTheme.md`: Candy Park theme reference.
- `Docs/SnowfieldPlayground_MapTheme.md`: Snowfield theme reference.
- `Docs/JellyMaze_MapTheme.md`: Jelly Maze theme reference.
- `Docs/EnvironmentDecorationGuide.md`: decoration placement and readability notes.

## Current Prototype Status

BubbleTown is a playable vertical-slice prototype rather than a finished game. The main loop is implemented: choose a mode, pick a hero, select a map, enter battle, move on the grid, place bombs, trigger explosions, collect items, resolve the round, and retry or return to the menu. The former single-player route has been reframed as Tutorial, with guided step text, route gates, guaranteed pickup support, and exit completion.

The visual direction is now much closer to a soft toy-board style: maps have distinct themed materials and props, item drops read closer to their illustrated icons, battle characters have clearer card-inspired silhouettes, the battle screen has a dedicated illustrated background and HUD, the mode select screen has a matching Tutorial card, the result page uses the latest illustrated layout, and the menu flow uses custom image assets. Many 3D assets are still runtime-generated primitives, so future art work can focus on replacing those generated forms with reusable polished models while preserving the current gameplay layout.

## Roadmap

- Replace runtime primitive map pieces with authored reusable prefabs while keeping the current Candy Park, Snowfield, and Jelly Maze readability.
- Replace generated primitive character forms with authored reusable models while preserving the current card-inspired silhouettes, props, and battle readability.
- Replace placeholder audio with final original or licensed sound assets.
- Continue improving AI behavior, difficulty tuning, and local-versus balance.
- Continue tuning Tutorial prompt copy, route layout, and visual markers after more playtesting.
- Convert remaining fallback IMGUI pieces into reusable Canvas or prefab-based UI where useful.
- Add automated validation or play-mode checks for map generation, scene flow, and battle-state regressions.
