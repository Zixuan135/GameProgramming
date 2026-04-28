# BubbleTown

BubbleTown is a colorful chibi-style 3D Bomberman-inspired Unity game prototype.

The project is developed through small daily iterations: keep the architecture stable, commit frequently, and grow from a playable MVP into a more polished local battle game.

## Current Status

- Engine: Unity 2022.3 LTS
- Language: C#
- View: 3D angled overhead / light third-person camera
- Core style: cute, colorful, low-cost placeholder art first
- Main flow: `MainMenu -> ModeSelect -> MapSelect -> Battle -> Result`
- Current branch workflow: develop on `local`, merge stable work into `main`

## Playable Modes

- `SinglePlayer`: Player1 test mode
- `AIBattle`: Player1 vs simple AI
- `LocalVS`: Player1 vs Player2 on one keyboard
- `LocalVS` currently supports a basic Best of 3 structure
- LocalVS score is shown in Battle and final score is shown on Result

## Core Features

- Grid-based movement on the XZ plane
- Player1 movement with `WASD`
- Player2 movement with arrow keys
- Bomb placement with per-character bomb count limits
- Bomb countdown, cross-shaped explosion propagation, and chain reactions
- Hard walls block explosions
- Soft walls break, stop explosion propagation, and can drop items
- Items can increase bomb count, explosion range, or movement speed
- Characters can die from explosions
- Battle opening flow with `READY -> GO!`
- Short spawn protection at round start
- Basic AI movement, danger detection, and bomb placement

## Current Presentation

- Runtime map themes:
  - `Candy Park`
  - `Jelly Maze`
- MapSelect uses themed cards and simple preview patterns
- Battle scene includes visual-only edge/background decorations
- Chibi placeholder characters for Player1, Player2, and AI
- Bubble-style bomb placeholder with countdown flash
- Center/horizontal/vertical explosion placeholder prefabs
- Recognizable 3D item placeholders with floating/glow animation
- Battle HUD shows mode, map, timer, round state, player stats, pickup toasts, and LocalVS score
- Result screen shows outcome, mode, map, winner, score placeholder, reward placeholder, stars, Retry, and Main Menu
- AudioManager supports BGM/SFX hooks; final audio clips are still placeholder-ready

## Controls

Player1:

- Move: `W`, `A`, `S`, `D`
- Place bomb: `Space`

Player2:

- Move: arrow keys
- Place bomb: `Enter` or `RightControl`

## How To Test

1. Open the project in Unity.
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press Play.
4. Click `Start Game`.
5. Choose a mode.
6. Choose a map.
7. Click `START SELECTED MAP`.
8. Watch the `READY -> GO!` opening flow.
9. Play Battle and verify Result / Retry / Main Menu.

LocalVS test path:

1. Choose `Local VS`.
2. Defeat either player with bombs.
3. Confirm the round winner prompt appears.
4. Confirm the score updates as `P1 X - Y P2`.
5. Confirm a new round starts automatically until one player reaches 2 points.
6. Confirm Result shows the final match winner and VS score.

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
  Prefabs/
    Characters/
    Environment/
    Gameplay/
    Map/
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
  UI/

Docs/
  CandyPark_MapTheme.md
  EnvironmentDecorationGuide.md
  JellyMaze_MapTheme.md
  Phase2_ArtDirection.md
```

## Key Runtime Systems

- `GameManager`: mode/map/session state, battle setup, LocalVS scoring
- `SceneFlowManager`: scene transitions
- `MapManager`: grid data, occupancy, wall/item/bomb state
- `MapGenerator`: runtime map visuals and theme decoration generation
- `CharacterBase`: shared movement, stats, bombs, life state, spawn protection
- `PlayerController`: keyboard input for Player1/Player2
- `AIController`: simple AI movement, danger checks, and bomb behavior
- `BombController`: bomb countdown, explosion spawn, chain reaction
- `ExplosionController`: explosion cell lifetime, hit detection, visual pulse
- `ItemBase` / `ItemSpawner`: item pickup and soft-wall item drops
- `BattleUI`: HUD, opening flow, result detection, LocalVS round flow
- `ResultUI`: final result display and retry/main menu actions
- `AudioManager`: BGM/SFX entry point

## Documentation

- `Docs/Phase2_ArtDirection.md`: visual direction and placeholder-art plan
- `Docs/CandyPark_MapTheme.md`: Candy Park theme notes
- `Docs/JellyMaze_MapTheme.md`: Jelly Maze theme notes
- `Docs/EnvironmentDecorationGuide.md`: decoration placement and readability rules

## Development Notes

- Keep gameplay modular and avoid large manager-only logic.
- Prefer small daily commits that are easy to test.
- Use placeholder geometry first, then replace with proper prefabs/art later.
- Keep gameplay roots stable so art can be replaced without breaking scripts.
- Avoid official copyrighted characters, logos, UI, music, or exact asset recreations.

## Near-Term Roadmap

- Replace IMGUI placeholder screens with polished Canvas or UI Toolkit UI
- Improve LocalVS match flow presentation and final scoring details
- Add stronger AI pathfinding and battle decisions
- Replace runtime primitive decorations with reusable environment prefabs
- Add real BGM/SFX clips and volume tuning
- Add more polished Q-style VFX and animation
- Continue adding original map themes beyond Candy Park and Jelly Maze
