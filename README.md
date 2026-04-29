# BubbleTown

BubbleTown is a colorful chibi-style 3D Bomberman-inspired Unity prototype.
It is built through small daily iterations: keep systems modular, keep commits reviewable, and improve the playable version step by step.

## Current Status

- Engine: Unity 2022.3 LTS
- Language: C#
- View: 3D angled overhead / light third-person camera
- Style: cute, colorful, low-cost chibi prototype art
- Flow: `MainMenu -> ModeSelect -> MapSelect -> Battle -> Result`
- Branch workflow: develop on `local`, merge stable work into `main`

## Modes

- `SinglePlayer`: Player1 clears the solo objective in Battle
- `AIBattle`: Player1 fights a basic grid-aware AI
- `LocalVS`: Player1 and Player2 share one keyboard, with a basic Best of 3 score flow

## Core Gameplay

- Grid-based movement on the XZ plane
- Player1 movement with `WASD`; bomb with `Space`
- Player2 movement with arrow keys; bomb with `Enter` or `RightControl`
- Bomb placement with per-character bomb count limits
- Bomb countdown, cross-shaped explosions, hard-wall blocking, soft-wall destruction, and chain reactions
- Soft walls can drop items
- Items currently support bomb count, explosion range, movement speed, shield, and temporary invincibility
- Characters can be defeated by explosions, with basic protection/shield rules
- Battle opening flow uses `READY -> GO!` with short spawn protection
- Basic AI can move, detect danger, place bombs, and escape after bombing

## Presentation

- MainMenu now uses a playful candy-castle layout with `Start Game`, `Guide`, `Settings`, and `Quit`
- Guide uses a compact player-facing popup for controls, bombs, goals, and upgrades
- ModeSelect uses livelier compact cards with clearer `Back` button spacing
- Non-interactive menu labels keep stable colors and avoid button-like hover feedback
- Runtime map themes: `Candy Park` and `Jelly Maze`
- MapSelect uses themed cards and preview patterns
- Battle scene uses chibi characters, bubble bomb visuals, bubble explosion cells, floating items, and edge decorations
- Battle HUD shows mode, map, timer, round state, player stats, objective/score, pickups, and result prompts
- Battle HUD removes developer-only controls and uses player-facing prompts
- Result screen shows outcome, mode, map, winner, score placeholder, reward placeholder, animated stars, Retry, and Main Menu
- Camera shake, pickup toasts, button hover/click animation, and result pop feedback are connected as lightweight game-feel polish
- `AudioManager` supports BGM/SFX hooks; final clips can be added later

## How To Test

1. Open `Assets/Scenes/MainMenu.unity` in Unity.
2. Press Play.
3. Check the main menu buttons: `Start Game`, `Guide`, `Settings`, and `Quit`.
4. Open `Guide` and confirm the compact control cards are readable.
5. Click `Start Game`.
6. Choose a mode and map.
7. Click `START SELECTED MAP`.
8. Watch the `READY -> GO!` opening flow.
9. Play Battle, then verify Result / Retry / Main Menu.

LocalVS quick test:

1. Choose `Local VS`.
2. Defeat either player with bombs.
3. Confirm the round winner prompt and score update.
4. Continue until one player reaches 2 points.
5. Confirm Result shows the final match winner and VS score.

## Scene Order

Build Settings order:

1. `MainMenu`
2. `ModeSelect`
3. `MapSelect`
4. `Battle`
5. `Result`

## Project Structure

```text
Assets/
  Art/
  Audio/
  Editor/
  Materials/
    Characters/
    Gameplay/
      Bombs/
      Explosions/
      Items/
    Map/
      CandyPark/
      JellyMaze/
      Shared/
    UI/
  Prefabs/
    Characters/
    Environment/
      CandyPark/
      JellyMaze/
    Gameplay/
      Bombs/
      Explosions/
      Items/
    Map/
      CandyPark/
      JellyMaze/
      Shared/
    UI/
  Scenes/
  Scripts/
    AI/
    Camera/
    Characters/
    Core/
    Gameplay/
    Items/
    Managers/
    Map/
    UI/
    Visuals/
  UI/

Docs/
  CandyPark_MapTheme.md
  EnvironmentDecorationGuide.md
  JellyMaze_MapTheme.md
  Phase2_ArtDirection.md
  Phase2_VisualStyleGuide.md
```

## Key Systems

- `GameManager`: mode/map/session state, battle setup, solo objective, LocalVS scoring
- `SceneFlowManager`: scene transitions
- `MapManager`: grid data, occupancy, wall/item/bomb state
- `MapGenerator`: runtime map visuals and theme decoration generation
- `CharacterBase`: shared movement, stats, bombs, life state, spawn protection, item effects
- `PlayerController`: keyboard input for Player1/Player2
- `AIController`: AI movement, danger checks, bomb behavior, escape behavior
- `BombController`: bomb countdown, explosion spawn, chain reaction
- `ExplosionController`: explosion cell lifetime, hit detection, visual pulse
- `ItemBase` / `ItemSpawner`: item pickup and soft-wall item drops
- `CameraController`: angled battle camera, shared LocalVS framing, and shake feedback
- `AudioManager`: BGM/SFX entry point
- `SimpleUIFactory`, `BattleUI`, `ResultUI`: current IMGUI UI flow and feedback
- `ProjectStructureSetup`: editor helper for asset folders and legacy scene visual grouping

## Documentation

- `Docs/Phase2_ArtDirection.md`: current visual direction and asset naming rules
- `Docs/Phase2_VisualStyleGuide.md`: palette, material, lighting, and readability rules
- `Docs/CandyPark_MapTheme.md`: Candy Park theme reference
- `Docs/JellyMaze_MapTheme.md`: Jelly Maze theme reference
- `Docs/EnvironmentDecorationGuide.md`: decoration placement and readability rules

## Development Notes

- Keep gameplay modular and avoid large manager-only logic.
- Prefer small commits that are easy to test.
- Move Unity assets with their `.meta` files to preserve references.
- Keep gameplay scripts on roots and replaceable art under visual children such as `VisualRoot`.
- Avoid official copyrighted characters, logos, UI, music, or exact asset recreations.

## Active Next Steps

- Migrate the current IMGUI menus to Canvas or UI Toolkit if the visual direction needs production-ready UI.
- Convert the best runtime primitive decorations into reusable environment prefabs.
- Add final BGM/SFX clips and volume controls.
- Continue AI tuning and presentation polish.
