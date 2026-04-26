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
    CharacterArtSetup.cs
    ExplosionPrefabSetup.cs
    ItemDropSetup.cs
    MapArtSetup.cs
    UIFlowSceneSetup.cs
  Materials/
    BombPlaceholder.mat
    ExplosionCenterPlaceholder.mat
    ItemBombCountUpPlaceholder.mat
    ItemExplosionRangeUpPlaceholder.mat
    ItemMoveSpeedUpPlaceholder.mat
    Mat_Item_BombCount_Body_Cyan.mat
    Mat_Item_BombCount_Icon_Cream.mat
    Mat_Item_BombCount_MiniBomb_Navy.mat
    Mat_Item_Common_Glow_Cream.mat
    Mat_Item_Range_Body_Orange.mat
    Mat_Item_Range_Icon_Yellow.mat
    Mat_Item_Range_Spark_Pink.mat
    Mat_Item_Speed_Body_Lime.mat
    Mat_Item_Speed_Icon_White.mat
    Mat_Item_Speed_Wing_Cyan.mat
    Mat_Bomb_Body_BubbleNavy.mat
    Mat_Bomb_Fuse_Cocoa.mat
    Mat_Bomb_Highlight_Cyan.mat
    Mat_Bomb_Spark_Yellow.mat
    Mat_Bomb_TopCap_Cream.mat
    Mat_Character_AI_Accent.mat
    Mat_Character_AI_Body.mat
    Mat_Character_Face_Dark.mat
    Mat_Character_Player1_Accent.mat
    Mat_Character_Player1_Body.mat
    Mat_Character_Player2_Accent.mat
    Mat_Character_Player2_Body.mat
    Mat_Character_Skin_Peach.mat
    Mat_Explosion_Arm_Orange.mat
    Mat_Explosion_Bubble_Cyan.mat
    Mat_Explosion_Core_Cream.mat
    Mat_Explosion_Spark_Pink.mat
    Mat_Tile_CandyBlue.mat
    Mat_Tile_CheckerAccent.mat
    Mat_Tile_GrassPastel.mat
    Mat_Wall_Hard_Cream.mat
    Mat_Wall_Hard_Highlight.mat
    Mat_Wall_Hard_Shadow.mat
    Mat_Wall_Soft_JellyBlue.mat
  Prefabs/
    Characters/
      Character_AI_Chibi.prefab
      Character_Player1_Chibi.prefab
      Character_Player2_Chibi.prefab
    Environment/
    Gameplay/
      Bomb.prefab
      ExplosionCenter.prefab
      ExplosionHorizontal.prefab
      ExplosionVertical.prefab
      Items/
        Item_BombCountUp.prefab
        Item_ExplosionRangeUp.prefab
        Item_MoveSpeedUp.prefab
    Map/
      Tile_Ground_CandyPark.prefab
      Wall_Hard_RoundedBlock.prefab
      Wall_Soft_JellyCrate.prefab
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
      CharacterPickupFeedback.cs
      CharacterVisualAnimator.cs
    Core/
    Gameplay/
    Items/
      ItemPickupFeedback.cs
      ItemVisualAnimator.cs
    Managers/
    Map/
      WallFeedback.cs
    UI/
      SimpleUIFactory.cs
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

## 4.1 Phase 2 Visual Direction

- Phase 2 focuses on turning the playable MVP into a more presentable chibi-style 3D grid battle game
- Art direction and low-cost asset planning live in `Docs/Phase2_ArtDirection.md`
- The project should use original cute, rounded, toy-like visuals and avoid official copyrighted characters, logos, UI, music, or exact asset recreations

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

- `MainMenu`: colorful chibi-style placeholder start screen with Start Game and Quit actions
- `ModeSelect`: colorful card-based mode screen for `SinglePlayer`, `AIBattle`, and `LocalVS`
- `MapSelect`: card-based map screen with placeholder previews for `Default`, `OpenField`, and `Maze`
- `Battle`: active gameplay scene with a colorful Phase 2 map art pass, players, camera, bombs, explosions, items, AI, and a small HUD
- `Result`: MVP result screen with win/loss text, Retry, and Main Menu actions

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
- `ItemType`: `BombCountUp`, `ExplosionRangeUp`, `MoveSpeedUp`
- `GameConstants`: shared defaults for map size, grid cell size, bomb fuse, bomb range, movement speed, explosion lifetime, and scene names

### Scene Skeleton

- Five required scenes exist in `Assets/Scenes`
- Build Settings order follows the intended game flow
- Each scene has basic Unity objects such as a camera and light for early testing
- Each scene has a `UI_Root_*` object with its matching MVP UI controller
- The current UI uses Unity's built-in IMGUI through `OnGUI` so the MVP flow works without extra UI package dependencies
- Final polished Canvas / UI Toolkit screens are intentionally deferred

### MVP UI Flow

- `MainMenuUI` starts a new session or quits the application
- `MainMenuUI` uses a candy-gradient background, bubble decorations, rounded panel, and layered buttons
- `ModeSelectUI` stores the selected `GameMode`
- `ModeSelectUI` presents mode choices as colorful card buttons
- `MapSelectUI` stores the selected `BattleMapType`, shows map cards with placeholder previews, and starts battle from a dedicated Start button
- `BattleUI` displays current mode, map, character states, and quick test buttons
- `BattleUI` displays short pickup toast messages when items are collected
- `ResultUI` displays the last battle result and supports Retry or returning to the main menu
- `SceneFlowManager` owns the fixed scene flow:
  - `MainMenu -> ModeSelect -> MapSelect -> Battle -> Result`
- `UIFlowSceneSetup` can rewire all MVP UI scene controllers from the Unity editor or batchmode

### Game Mode Setup

- `GameManager` stores the current game mode, map type, and high-level game state
- `GameManager` stores the most recent battle result for the `Result` scene
- `GameManager` prepares the `Battle` scene based on the selected mode:
  - `SinglePlayer`: enables Player1 only
  - `AIBattle`: enables Player1 and AI
  - `LocalVS`: enables Player1 and Player2
- Player and AI spawn positions are resolved through `MapManager`
- Runtime character setup reuses the same map, bomb root, and bomb prefab references

### Result Flow

- `BattleUI` provides the first minimal result detection layer
- `SinglePlayer`: Player1 defeat leads to `Game Over`
- `AIBattle`: Player1 defeat is a loss, AI defeat is a victory, simultaneous defeat is a draw
- `LocalVS`: Player1 or Player2 defeat awards the round to the other player, simultaneous defeat is a draw
- The `Battle` HUD includes a `Force Result` button for quickly testing the result scene
- `ResultUI` supports Retry and Main Menu actions

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
- The current `Battle` scene uses Phase 2 map visual prefabs under `MapRoot`
- Logical map blocker data remains owned by `MapManager`

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
- Character death clears map occupancy, disables colliders immediately, plays a short defeat visual, then hides renderers
- `CharacterBase` can rotate a configured `visualRoot` toward the latest grid movement direction
- This keeps gameplay roots grid-stable while allowing replaceable character art to show facing
- `CharacterBase` emits a lightweight `BombPlaced` event so visual-only scripts can react without owning gameplay logic
- `CharacterBase` also emits `ExplosionHit` and `DeathFeedbackStarted` events for visual-only hit/defeat feedback

### Player Input

- Player1:
  - `WASD` for four-direction movement
  - `Space` to place a bomb
- Player2:
  - Arrow keys for four-direction movement
  - `Enter` or `RightControl` to place a bomb
- Player2 is prepared for `LocalVS` without duplicating movement code
- Both players reuse `CharacterBase` movement and bomb placement logic

### Phase 2 Character Placeholder Art

- The `Battle` scene now uses low-cost chibi-style geometry characters
- Gameplay roots stay named `Player1`, `Player2`, and `AIPlayer`
- Each character root owns movement, bombs, collision, and map occupancy
- Each character root has a child visual named `CharacterVisual`
- `CharacterVisual` is replaceable art and points to one of these prefabs:
  - `Character_Player1_Chibi.prefab`
  - `Character_Player2_Chibi.prefab`
  - `Character_AI_Chibi.prefab`
- Current visual hierarchy:
  - character root
  - `CharacterVisual`
  - `VisualRoot`
  - primitive parts such as `Head_BigRound`, `Body_RoundSuit`, `Foot_*`, `Eye_*`, and `FrontBadge_FacingMarker`
- Player1 uses a blue/cyan body with a yellow front accent
- Player2 uses coral/orange with yellow bow accents
- AI uses purple with a red visor and antenna
- `CharacterVisualAnimator` adds code-driven placeholder animation:
  - idle bob
  - movement bounce/sway
  - stronger squash, hop, tilt, glow, and shake when a bomb is placed
  - explosion hit shake, punch scale, and flash
  - defeat rise, spin, shrink, glow, and small puff placeholders
- `CharacterArtSetup` can regenerate these prefabs and rewire the `Battle` scene from the editor menu or batchmode

Animator recommendation:

- Current phase: use `CharacterVisualAnimator` instead of Animator because characters are primitive placeholder objects with no animation clips or skeletons yet
- Later phase: switch to Animator when the character art becomes proper mesh/rig assets
- Suggested future Animator parameters:
  - `IsMoving` bool
  - `MoveSpeed` float
  - `PlaceBomb` trigger
  - `IsAlive` bool

### AI

- `AIController` reuses `CharacterBase` movement and bomb placement
- The first AI pass supports stable random grid movement
- AI avoids moving into known dangerous cells when possible
- AI detects danger from:
  - active bomb blast lines
  - active explosion cells
- When standing in danger, AI searches for a nearby safe cell and moves toward it
- AI can attempt to place bombs when:
  - a soft wall is nearby
  - a living player is in the same unobstructed bomb line
- AI checks for an escape route before placing a bomb by default
- This is intentionally MVP behavior, not advanced pathfinding or tactical combat

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
- `Bomb.prefab` now uses a Phase 2 bubble-bomb placeholder structure:
  - root `Bomb` with `SphereCollider` and `BombController`
  - `VisualRoot`
  - `Body_BubbleSphere`
  - `Body_CartoonHighlight`
  - `TopCap_CreamButton`
  - `Fuse_CurvedStem`
  - `Fuse_Spark`
- `BombController` drives countdown flash feedback through the prefab renderers
- Bomb flashing becomes faster as the fuse gets closer to explosion
- `ExplosionCenter.prefab` is available under `Assets/Prefabs/Gameplay`
- `ExplosionHorizontal.prefab` and `ExplosionVertical.prefab` provide directional arm visuals
- Explosion prefab structure uses a gameplay root with trigger collider, kinematic rigidbody, `ExplosionController`, and `VisualRoot`
- Center explosion uses round pop bubbles and pink spark accents
- Horizontal explosion uses an orange splash bar with side bubbles
- Vertical explosion uses the same visual language rotated for the Z axis
- `ExplosionController` drives short scale, rotation, and emission pulses with `MaterialPropertyBlock`
- Explosions spawn grid-based placeholder cells
- Explosion propagation is cross-shaped from the bomb center
- Explosion range uses the character/bomb range setting
- Hard walls stop explosion propagation immediately in that direction
- Soft walls are destroyed when hit and stop further propagation in that direction
- Explosion cells can kill characters by calling `CharacterBase.OnHitByExplosion`
- Explosion cells can trigger nearby bombs early, enabling chain reactions
- Chain reactions are guarded against duplicate explosions and duplicate occupancy cleanup

### Items And Power-Ups

- `ItemBase` handles simple pickup and stat application
- `ItemBase` stays focused on pickup rules, stat application, map cleanup, and pickup event dispatch
- `ItemSpawner` listens for soft wall destruction and can spawn random items
- Soft wall item drops use a configurable probability
- Item spawn state syncs with `MapManager.SetItem`
- `ItemVisualAnimator` adds visual-only floating, rotation, scale pulse, and emission pulse
- `ItemPickupFeedback` adds a low-cost pickup disappear animation and an `AudioClip` hook
- `CharacterPickupFeedback` gives the collecting character a short color-matched glow flash
- `BattleUI` listens for `ItemBase.ItemPickedUp` and shows a temporary text/icon-style pickup toast
- Current Phase 2 item placeholder prefabs:
  - `Item_BombCountUp`
    - cyan bubble token with a mini-bomb and plus icon
  - `Item_ExplosionRangeUp`
    - orange burst token with a yellow cross-range icon and pink sparks
  - `Item_MoveSpeedUp`
    - lime capsule token with a forward arrow and cyan speed wings
- Current item prefab structure:
  - root item object with trigger collider, kinematic rigidbody, `ItemBase`, `ItemPickupFeedback`, and `ItemVisualAnimator`
  - `VisualRoot`
  - primitive token/icon/glow children
- Pickup feedback flow:
  - item clears map item state
  - item dispatches a pickup event for UI
  - character flashes briefly
  - item disables its trigger, rises, spins, shrinks, and then destroys itself
  - optional pickup audio can be assigned through `ItemPickupFeedback.pickupClip`
- Current item effects:
  - increase max bomb count
  - increase explosion range
  - increase movement speed

## 8. Current Test Controls

For the full MVP loop, open `MainMenu` and press Play.

Flow:

1. Click `Start Game`
2. Choose `Single Player`, `AI Battle`, or `Local VS`
3. Choose a map card: `Candy Park`, `Open Field`, or `Jelly Maze`
4. Click `START SELECTED MAP`
5. Play the `Battle` scene
6. Defeat a character or click `Force Result`
7. Verify the `Result` screen
8. Use `Retry` or `Main Menu`

You can still open the `Battle` scene directly for gameplay-system testing.

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
- Bomb visuals flash and pulse faster near the end of the fuse
- Explosion center and arms use different colorful placeholder visuals
- Explosion cells form a cross shape
- Hard walls stop explosion spread
- Soft walls disappear when hit by an explosion
- A character hit by an explosion dies and stops responding to input
- A bomb touched by another explosion triggers early as a chain reaction
- Destroyed soft walls can drop placeholder items
- Dropped items float, rotate, and use glowing color pulses
- Picked-up items play a short disappear animation
- The collecting character flashes with a matching item color
- The battle HUD shows a short pickup toast such as `[Bomb Slot +1]`
- Characters automatically pick up items and apply stat changes
- In `AIBattle`, the AI moves around the grid, tries to avoid bomb danger, and can place bombs near soft walls or players
- The UI flow moves from menu screens into battle and then into the result screen
- `MainMenu` and `ModeSelect` use colorful placeholder Q-style menu visuals
- `MapSelect` uses map cards, simple color preview blocks, selected-state feedback, and a separate start button
- Result screen shows the last winner/result and supports Retry/Main Menu
- Player1, Player2, and AI use distinct chibi placeholder visuals with visible facing markers
- Characters have visible idle bob, movement bounce, and a stronger bomb-placement squash/hop/glow
- Characters shake and flash when hit by an explosion
- Defeated characters briefly pop upward, spin/shrink, spawn tiny puffs, then disappear

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
- [x] Phase 2 bubble-bomb placeholder prefab and countdown flash
- [x] Center explosion placeholder
- [x] Cross-shaped explosion propagation
- [x] Phase 2 center/horizontal/vertical explosion placeholder prefabs
- [x] Hard wall explosion blocking
- [x] Soft wall destruction
- [x] Character death from explosions
- [x] Bomb chain reactions
- [x] Soft wall item drops
- [x] Bomb count, range, and speed power-ups
- [x] Phase 2 recognizable 3D item placeholder prefabs
- [x] Low-cost item pickup feedback with toast, character flash, and audio hook
- [x] Basic game mode battle setup
- [x] Basic AI random movement
- [x] AI danger detection and simple bomb placement
- [x] Minimal menu UI flow
- [x] Q-style placeholder MainMenu and ModeSelect visual pass
- [x] Card-based MapSelect placeholder UI with map previews
- [x] Minimal result screen
- [x] Retry and Main Menu result actions
- [x] Phase 2 map placeholder art pass
- [x] Hard/soft wall explosion feedback placeholders
- [x] Phase 2 chibi character placeholder prefabs
- [x] Shared character visual facing support
- [x] Code-driven placeholder character animations
- [x] Character bomb, hit, and defeat feedback polish

Still planned:

- [ ] More complete battle rules, scoring, timers, and win conditions
- [ ] Replace IMGUI placeholder screens with polished Canvas or UI Toolkit menus
- [ ] More robust AI pathfinding and battle decisions
- [ ] More complete map theme decoration outside the playable grid
- [ ] Polished Q-style bomb, explosion, item, UI, and audio assets
- [ ] Optional Blender/Blockbench replacement meshes for current primitive character prefabs
- [ ] Animator-based character animation once real character meshes or rigs exist
- [ ] Audio, VFX, and UI polish

## 10. Notes For Future Iterations

- The current project is playable as a gameplay-system test, not yet a finished game loop
- The `Battle` scene is the main development/test scene for now
- Prefabs and editor setup scripts exist to reduce manual Unity Inspector work
- Current result logic is intentionally simple and lives in `BattleUI`; later it can move into a dedicated battle rules/result service
- Current UI is IMGUI-only for speed and stability; final UI should use proper prefabs, layout, styling, and navigation
- Item pickup feedback and balancing can be improved after the current item visuals are stable
- AI currently favors stability over intelligence; future work can add path scoring, target chasing, and better self-preservation
