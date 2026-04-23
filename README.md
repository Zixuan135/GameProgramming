# BubbleTown (Unity 3D)

A colorful chibi-style 3D Bomberman-inspired game.  
Development approach: small daily iterations, starting from a playable MVP and expanding features and visuals step by step.

## 1. Project Goals

- Grid-based classic Bomberman-style gameplay (movement, bomb placement, blast propagation, breakable/unbreakable walls, items)
- Support for `SinglePlayer` / `AIBattle` / `LocalVS`
- Scene flow: `MainMenu -> ModeSelect -> MapSelect -> Battle -> Result`
- Build one small feature at a time for easy testing and continuous GitHub commits

## 2. Tech Stack

- Unity 3D (LTS recommended)
- C#
- Git + GitHub

## 3. Current Folder Structure (Initialization)

```text
Assets/
  Scenes/
  Scripts/
    Core/
    Managers/
    Map/
    Characters/
    Gameplay/
    AI/
    Items/
    Camera/
    UI/
    Utils/
  Prefabs/
    Characters/
    Map/
    Gameplay/
    UI/
    Environment/
  Materials/
  Art/
    Models/
    Textures/
    VFX/
    Sprites/
  UI/
    Sprites/
    Fonts/
  Audio/
    BGM/
    SFX/
  ScriptableObjects/

Docs/
Packages/
ProjectSettings/
```

## 4. Development Conventions (Initial Phase)

- Keep code modular; avoid putting all logic into one script
- Use placeholder assets first (basic geometry + simple materials + basic UI)
- Prioritize gameplay correctness before VFX and advanced animation
- Each iteration should include:
  - A clear small goal
  - A runnable and testable result
  - A matching Git commit

## 5. Recommended Branching & Commit Strategy

- Main branch: `main`
- Daily dev branch: `local` (or feature branches like `feature/day02-grid-move`)
- Commit examples:
  - `chore(init): setup unity folder structure`
  - `docs(readme): add project roadmap and module layout`
  - `feat(core): add game mode enums`

## 6. Phase 1 Milestones (MVP)

1. Initialize project and folder structure
2. Build scene flow skeleton (switchable empty scenes)
3. Implement grid map and 4-direction character movement
4. Implement bomb placement and delayed explosion
5. Implement wall blocking rules for blast propagation
6. Implement damage/death and battle result
7. Implement item drops and basic power-ups (bomb count/range/speed)

## 7. Setup & Collaboration

1. Open this folder with Unity Hub
2. On first open, Unity will auto-generate content in `ProjectSettings` and `Packages`
3. Commit only source/assets; ignore caches and build outputs (see `.gitignore`)

## 8. Current Progress (as of 2026-04-23)

- Completed project bootstrap (folder structure, README, Unity-generated settings/packages)
- Completed scene skeleton setup with 5 scenes:
  - `MainMenu`
  - `ModeSelect`
  - `MapSelect`
  - `Battle`
  - `Result`
- Build Settings scene order configured:
  - `MainMenu -> ModeSelect -> MapSelect -> Battle -> Result`
- Completed core script skeleton (structure-first, no complex gameplay yet):
  - Core: `GameConstants`, `Enums`
  - Managers: `GameManager`, `SceneFlowManager`
  - Map: `MapManager`, `MapGenerator`
  - Characters: `CharacterBase`, `PlayerController`, `AIController`
  - Gameplay: `BombController`, `ExplosionController`
  - Items: `ItemBase`, `ItemSpawner`
  - Camera: `CameraController`
  - UI: `MainMenuUI`, `ModeSelectUI`, `MapSelectUI`, `BattleUI`, `ResultUI`
- Completed enum/state/constants foundation update:
  - added `GameState` enum for global runtime flow
  - confirmed `GameMode`, `CellType`, and `ItemType` as shared base enums
  - refined constants with `DefaultExplosionRange` (kept `DefaultBombRange` compatibility alias)
- Completed grid data-layer APIs for map logic:
  - added `GridCell` data model (`GridPosition`, hard/soft wall, bomb, character, item flags)
  - expanded `MapManager` with:
    - `GetCell`
    - `IsInsideBounds`
    - `IsWalkable`
    - `SetCharacter`
    - `ClearCharacter`
    - `PlaceBomb`
    - `RemoveBomb`
  - added `WorldToGrid` / `GridToWorld` helpers for XZ-plane movement logic
  - configured `Battle` scene `MapRoot` with `MapGenerator` + `MapManager` references
- Completed map rule data-layer update for Bomberman-style opening fairness:
  - hard-wall border is enforced on map edges
  - spawn-adjacent cells are reserved as walkable space
  - `LocalVS` uses distant spawn points for Player1/Player2
  - `AIBattle` uses a reasonable AI spawn opposite to Player1
- Notes:
  - Current scenes are structure-first placeholders
  - Current scripts are skeleton-level with basic fields/methods only
  - Current map rule update is data-layer focused (no complex visual generation yet)
  - Full UI and complete gameplay logic are intentionally not implemented yet
