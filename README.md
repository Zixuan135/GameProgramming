# BubbleTown (Unity 3D)

A colorful chibi-style 3D Bomberman-inspired game built with Unity and C#.

The project is developed through small daily iterations: keep the architecture stable, commit frequently, and grow from a playable MVP into a fuller local battle game.

## 1. Project Goals

- Build a grid-based 3D Bomberman-style game on the XZ plane
- Use a cute, colorful, placeholder-first visual style during MVP development
- Support three game modes: `SinglePlayer`, `AIBattle`, and `LocalVS`
- Keep the scene flow clear: `MainMenu -> ModeSelect -> MapSelect -> Battle -> Result`
- Prioritize small, testable GitHub commits over large all-at-once feature drops

## 2. Tech Stack

- Unity 2022.3 LTS style project layout
- C# gameplay scripts
- Git + GitHub for daily iteration and pull requests
- Placeholder geometry, materials, and prefabs before final art/VFX/audio

## 3. Current Folder Structure

```text
Assets/
  Art/
    Models/
    Sprites/
    Textures/
    VFX/
  Audio/
    BGM/
    SFX/
  Editor/
    BattleSceneCameraSetup.cs
    BattleScenePlayerSetup.cs
    BombPrefabSetup.cs
    ExplosionPrefabSetup.cs
  Materials/
    BombPlaceholder.mat
    ExplosionCenterPlaceholder.mat
  Prefabs/
    Characters/
    Environment/
    Gameplay/
      Bomb.prefab
      ExplosionCenter.prefab
    Map/
    UI/
  Scenes/
    MainMenu.unity
    ModeSelect.unity
    MapSelect.unity
    Battle.unity
    Result.unity
  ScriptableObjects/
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
    Utils/
  UI/
    Fonts/
    Sprites/

Docs/
Packages/
ProjectSettings/
```

## 4. Development Conventions

- Keep gameplay code modular and avoid one giant manager script
- Implement the data/logic layer before polished visuals
- Prefer basic 3D objects and simple materials for early tests
- Keep naming stable so future daily tasks can build on existing modules
- Every iteration should have a small goal, a testable result, and a matching Git commit

## 5. Branching And Commit Strategy

- Stable branch: `main`
- Daily development branch: `local`
- Recommended PR flow: finish daily task on `local`, push, open PR, merge to `main`
- Commit message examples:
  - `chore(init): setup unity folder structure`
  - `docs(readme): update current project status`
  - `feat(map): add grid occupancy data layer`
  - `feat(gameplay): add bomb chain reaction`

## 6. Scene Flow

Build Settings scene order:

1. `MainMenu`
2. `ModeSelect`
3. `MapSelect`
4. `Battle`
5. `Result`

Current scene state:

- `MainMenu`: placeholder scene shell for future title/menu UI
- `ModeSelect`: placeholder scene shell for `SinglePlayer`, `AIBattle`, and `LocalVS`
- `MapSelect`: placeholder scene shell for future map selection
- `Battle`: active gameplay test scene with map, players, camera, bombs, and explosions
- `Result`: placeholder scene shell for future win/lose display

## 7. Core Systems Implemented So Far

### Project Bootstrap

- Unity folder structure is organized for long-term iteration
- Initial README and GitHub workflow are in place
- Unity-generated `Packages` and `ProjectSettings` are tracked where appropriate
- `.gitignore` is configured to ignore Unity caches, builds, and local-only files

### Core Definitions

- `GameMode`: `SinglePlayer`, `AIBattle`, `LocalVS`
- `GameState`: shared runtime state foundation
- `CellType`: map tile classification foundation
- `ItemType`: future item/power-up type foundation
- `GameConstants`: shared defaults for map size, grid cell size, bomb fuse, bomb range, movement speed, explosion lifetime, and scene names

### Scene Skeleton

- Five required scenes exist in `Assets/Scenes`
- Build Settings order follows the intended game flow
- Each scene has basic Unity objects such as a camera and light for early testing
- Complex UI and transitions are intentionally deferred

### Map And Grid Data

- `GridCell` stores logical map state:
  - grid position
  - hard wall flag
  - soft wall flag
  - bomb occupancy
  - character occupancy
  - item occupancy
- `MapManager` provides shared map APIs:
  - `GetCell`
  - `IsInsideBounds`
  - `IsWalkable`
  - `SetCharacter`
  - `ClearCharacter`
  - `PlaceBomb`
  - `RemoveBomb`
  - `DestroySoftWall`
  - `WorldToGrid`
  - `GridToWorld`
- The map is grid-based logically while remaining 3D visually
- Movement and bomb logic operate on the XZ plane

### Map Rules

- Map borders are sealed with hard walls
- Player spawn areas reserve nearby walkable cells
- `LocalVS` uses distant spawn positions for Player1 and Player2
- `AIBattle` reserves a reasonable AI spawn position opposite Player1
- The current `Battle` scene includes visible blocker test objects:
  - `TestHardWall_3_1`
  - `TestSoftWall_1_3`

### Character Framework

- `CharacterBase` owns shared character data and behavior:
  - current grid position
  - current world position
  - smooth grid movement
  - movement speed
  - bomb range
  - max active bomb count
  - active bomb count
  - alive/dead state
- Characters move smoothly from one grid cell to the next
- New movement input is ignored while already moving
- Characters cannot move through hard walls, soft walls, bombs, or occupied character cells
- Character death currently hides renderers, disables colliders, clears map occupancy, and emits a `Died` event for future result logic

### Player Input

- Player1:
  - `WASD` for four-direction movement
  - `Space` to place a bomb
- Player2:
  - Arrow keys for four-direction movement
  - `Enter` or `RightControl` to place a bomb
- Player2 is prepared for `LocalVS` without duplicating movement code
- Both players reuse `CharacterBase` movement and bomb placement logic

### Camera

- `CameraController` provides an angled 3D overhead / light third-person style view
- The camera follows Player1 by default
- In `LocalVS`, the current design supports a shared camera that can frame both players
- The first camera pass favors grid readability over cinematic movement

### Bombs And Explosions

- `Bomb.prefab` is available under `Assets/Prefabs/Gameplay`
- Characters can place bombs on their current grid cell
- The same grid cell cannot receive duplicate bombs
- Bomb placement syncs with `MapManager` bomb occupancy
- Characters have a max active bomb limit, currently based on `GameConstants.DefaultBombCount`
- Bombs start a countdown after placement and explode when the fuse reaches zero
- Bombs notify their owner when finished so the active bomb slot is released
- `ExplosionCenter.prefab` is available under `Assets/Prefabs/Gameplay`
- Explosions spawn grid-based placeholder cells
- Explosion propagation is cross-shaped from the bomb center
- Explosion range uses the character/bomb range setting
- Hard walls stop explosion propagation immediately in that direction
- Soft walls are destroyed when hit and stop further propagation in that direction
- Explosion cells can kill characters by calling `CharacterBase.OnHitByExplosion`
- Explosion cells can trigger nearby bombs early, enabling chain reactions
- Chain reactions are guarded against duplicate explosions and duplicate occupancy cleanup

## 8. Current Test Controls

Open the `Battle` scene and press Play.

Player1:

- Move: `W`, `A`, `S`, `D`
- Place bomb: `Space`

Player2:

- Move: Arrow keys
- Place bomb: `Enter` or `RightControl`
- Player2 is mainly for `LocalVS` testing

Basic things to verify:

- Player movement stays on the grid
- Movement is smooth between cells
- Hard walls and soft walls block movement
- Bombs block movement after placement
- One character cannot place more bombs than its current limit
- Bombs count down and explode
- Explosion cells form a cross shape
- Hard walls stop explosion spread
- Soft walls disappear when hit by an explosion
- A character hit by an explosion dies and stops responding to input
- A bomb touched by another explosion triggers early as a chain reaction

## 9. MVP Roadmap

Completed or started:

- [x] Project initialization and folder structure
- [x] GitHub-ready README and Unity `.gitignore`
- [x] Scene skeleton and Build Settings order
- [x] Core enums and shared constants
- [x] Grid data layer and map occupancy APIs
- [x] Bomberman-style map boundary and spawn rules
- [x] Player1 movement
- [x] Player2 local keyboard movement
- [x] Movement blocking rules
- [x] Basic angled 3D battle camera
- [x] Bomb placement
- [x] Bomb count limit
- [x] Bomb countdown
- [x] Center explosion placeholder
- [x] Cross-shaped explosion propagation
- [x] Hard wall explosion blocking
- [x] Soft wall destruction
- [x] Character death from explosions
- [x] Bomb chain reactions

Still planned:

- [ ] Full battle win/lose detection and result flow
- [ ] Real menu UI interactions
- [ ] Item drop chance after soft wall destruction
- [ ] Bomb count, range, and speed power-ups
- [ ] Basic AI movement and bomb behavior
- [ ] Better placeholder map generation visuals
- [ ] Polished Q-style character, bomb, wall, and explosion assets
- [ ] Audio, VFX, and UI polish

## 10. Notes For Future Iterations

- The current project is playable as a gameplay-system test, not yet a finished game loop
- The `Battle` scene is the main development/test scene for now
- Prefabs and editor setup scripts exist to reduce manual Unity Inspector work
- Result logic should later subscribe to character death events and decide battle outcome
- Item logic should later connect `ItemSpawner`, `ItemBase`, and `MapManager.SetItem`
- AI should reuse `CharacterBase` movement and bomb placement instead of creating a separate movement system
