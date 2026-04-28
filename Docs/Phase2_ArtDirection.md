# Phase 2 Art Direction And Presentation Guide

BubbleTown Phase 2 focuses on turning the current playable prototype into a more presentable, colorful, chibi-style 3D grid battle game.

This guide is intentionally practical. It should help each daily task make small visual upgrades without breaking the existing gameplay architecture.

## 1. Visual Pillars

- Cute, rounded, toy-like shapes
- Bright candy colors with clear team/readability contrast
- Low-poly or simple stylized 3D models instead of realistic assets
- Strong grid readability from an angled overhead camera
- Clear feedback for bombs, explosions, item pickups, damage, and victory/defeat
- Original visual identity only: do not copy official Bomberman / Crazy Arcade / Bubble Bobble characters, logos, UI, music, or exact asset designs

## 2. Color Direction

Recommended base palette:

- Sky blue: `#67C7FF`
- Bubble cyan: `#2FE7D6`
- Candy yellow: `#FFD34D`
- Coral red: `#FF6B5E`
- Lime green: `#83E66B`
- Soft violet accent: `#9B8CFF`
- Cream highlight: `#FFF2CC`
- Warm shadow: `#5A4B42`

Usage notes:

- Player1 should stay visually distinct, preferably blue/cyan.
- Player2 should use orange/red or yellow/red.
- AI should use a clearly different accent, such as coral or purple.
- Hard walls should feel solid and permanent.
- Soft walls should feel breakable and lighter.
- Items should use high-saturation colors and simple silhouettes.

## 3. Character Style

Target style:

- Chibi proportions: large head, small body, short limbs
- Rounded helmet/hood/hair-like silhouette
- Simple face: two dot eyes, tiny mouth, optional cheek circles
- Toy-like body with strong color accents
- Minimal animation first: idle bob, movement squash, defeat pop/hide

Suggested proportions:

- Total height: about `0.9` to `1.2` Unity units
- Head: about `55%` to `65%` of total height
- Body: short capsule or rounded cube
- Feet: two small rounded capsules or spheres
- Collider: simple capsule or box, aligned to grid center

Low-cost implementation options:

- Unity primitives:
  - sphere for head
  - capsule or rounded-looking cylinder for body
  - small spheres/capsules for hands and feet
- Blender / Blockbench:
  - make one simple low-poly chibi character mesh
  - duplicate materials for Player1, Player2, and AI
  - export as FBX or glTF
- Later free generic assets:
  - use only assets with clear license terms
  - prefer generic stylized low-poly characters
  - avoid anything resembling official game mascots

Recommended prefab names:

- `Character_Player1_Chibi.prefab`
- `Character_Player2_Chibi.prefab`
- `Character_AI_Chibi.prefab`

Recommended material names:

- `Mat_Character_Player1_Body`
- `Mat_Character_Player2_Body`
- `Mat_Character_AI_Body`
- `Mat_Character_Face`
- `Mat_Character_Shadow`

## 3.1 Current Phase 2 Chibi Character Placeholder Set

The first Phase 2 character art pass uses Unity primitives instead of custom models.

Scene organization:

- `CharactersRoot`
  - `Player1`
    - `CharacterVisual`
      - `VisualRoot`
  - `Player2`
    - `CharacterVisual`
      - `VisualRoot`
  - `AIPlayer`
    - `CharacterVisual`
      - `VisualRoot`

Current prefab set:

- `Assets/Prefabs/Characters/Character_Player1_Chibi.prefab`
  - Large round head, capsule body, small feet, two dot eyes
  - Blue/cyan body and yellow front marker
  - Extra cap bubble to make Player1 easy to spot
- `Assets/Prefabs/Characters/Character_Player2_Chibi.prefab`
  - Same readable chibi proportions as Player1
  - Coral/orange body and yellow bow accents
  - Side puffs make Player2 distinct in `LocalVS`
- `Assets/Prefabs/Characters/Character_AI_Chibi.prefab`
  - Purple body and head
  - Red visor and antenna to read as a simple toy robot opponent

Current material set:

- `Mat_Character_Skin_Peach`
- `Mat_Character_Face_Dark`
- `Mat_Character_Player1_Body`
- `Mat_Character_Player1_Accent`
- `Mat_Character_Player2_Body`
- `Mat_Character_Player2_Accent`
- `Mat_Character_AI_Body`
- `Mat_Character_AI_Accent`

Implementation notes:

- The gameplay root should stay responsible for movement, collisions, bombs, and map occupancy.
- `CharacterVisual` should stay as a child so art can be replaced without changing gameplay scripts.
- `CharacterBase.visualRoot` points to `CharacterVisual`, and `CharacterBase.faceMoveDirection` rotates only the visual art toward the latest grid move direction.
- The current facing marker is `FrontBadge_FacingMarker`, plus eyes/visor on the forward side.
- `CharacterArtSetup` can regenerate these prefabs and rewire `Battle` from the Unity editor menu or batchmode.

## 3.2 Current Phase 2 Character Animation Placeholder

The current animation pass is code-driven instead of Animator-driven.

Why code first:

- Current characters are primitive placeholder objects, not rigged meshes.
- There are no reusable authored animation clips yet.
- The goal is quick Q-style liveliness without disturbing grid movement or collision.
- The visual script can be removed later when proper Animator clips exist.

Current implementation:

- `CharacterVisualAnimator` lives on `CharacterVisual`.
- It animates the child `VisualRoot`, not the gameplay root.
- `CharacterBase` exposes a `BombPlaced` event for visual-only reactions.
- `CharacterBase` exposes `ExplosionHit` and `DeathFeedbackStarted` events for visual-only combat feedback.
- Idle state uses a subtle vertical bob.
- Moving state uses bounce, sway, and squash/stretch.
- Bomb placement uses a more obvious hop, squash, forward tilt, small shake, and cyan glow.
- Explosion hit uses quick shake, punch scale, and orange flash.
- Defeat uses rise, spin, shrink, warm glow, and tiny primitive puff placeholders before renderers are hidden.
- Death collision is disabled immediately, while renderer hiding is delayed briefly so the defeat feedback remains visible.

Animator recommendation:

- Do not require Animator for the current primitive placeholder phase.
- Add Animator later when characters become Blender/Blockbench/FBX meshes or when we need authored clips.
- Suggested future Animator parameters:
  - `IsMoving` bool
  - `MoveSpeed` float
  - `PlaceBomb` trigger
  - `IsAlive` bool

Low-cost next upgrades:

- Tune per-character animation values for personality.
- Add item pickup pop feedback.
- Replace primitive defeat puffs with a proper VFX prefab.
- Add short hit and defeat sound effects.
- Replace primitive heads/bodies with one simple Blender or Blockbench mesh while keeping the same prefab names.

## 4. Map Style

Target style:

- Colorful toy-board arena
- Clear square grid cells on the XZ plane
- Slightly rounded or beveled tiles
- Small decorative elements outside the playable area
- Readable from angled overhead camera

First theme suggestion:

- Candy park / toy garden
- Grass or pastel floor tiles
- Cream-colored hard borders
- Soft blocks as colorful crates or jelly cubes
- Decorative background props outside the grid: balloons, tiny trees, toy fences

Low-cost implementation options:

- Unity primitives:
  - plane or cube tiles for floor
  - simple cubes for walls
  - slightly raised border cubes
- Blender / Blockbench:
  - bevelled tile mesh
  - rounded cube wall mesh
  - simple decorative props
- Later free generic assets:
  - stylized low-poly environment props
  - tile textures, grass textures, simple fences, trees
  - always verify license and avoid recognizable copyrighted themes

Recommended dimensions:

- Grid cell size remains `1.0` Unity unit
- Floor tile top should stay flat and readable
- Wall blocks should fit inside one grid cell
- Wall height should be around `0.9` to `1.2` Unity units

Recommended prefab names:

- `Tile_Ground_CandyPark.prefab`
- `Wall_Hard_RoundedBlock.prefab`
- `Wall_Soft_JellyCrate.prefab`
- `Prop_Background_ToyFence.prefab`

Recommended material names:

- `Mat_Tile_GrassPastel`
- `Mat_Tile_CheckerAccent`
- `Mat_Wall_Hard_Cream`
- `Mat_Wall_Soft_JellyBlue`
- `Mat_Wall_Soft_JellyOrange`

## 4.1 Current Battle Theme: Candy Park

The first complete Battle scene theme is `Candy Park`.

Reference document:

- `Docs/CandyPark_MapTheme.md`

Current reusable map assets:

- `Tile_Ground_CandyPark.prefab`
- `Wall_Hard_RoundedBlock.prefab`
- `Wall_Soft_JellyCrate.prefab`

Theme direction:

- Pastel candy grass floor tiles
- Cream rounded hard walls
- Blue jelly soft walls
- Optional toy fence, lollipop tree, balloon cluster, sign board, and round bush props outside the playable grid

## 4.2 Current Battle Theme: Jelly Maze

The second Battle scene theme is `Jelly Maze`.

Reference document:

- `Docs/JellyMaze_MapTheme.md`

Current runtime map selection:

- `BattleMapType.Maze` uses the Jelly Maze visual direction.
- `BattleMapType.Default` and `BattleMapType.OpenField` continue using the Candy Park visual direction.

Theme direction:

- Dark violet jelly-lab floor tiles
- Cyan glowing route insets
- Violet glass hard walls
- Magenta jelly soft walls
- Neon gate lines, crystal clusters, glow tubes, and signal beacons outside the playable grid

Low-cost asset direction:

- Current version is generated from Unity primitives at runtime.
- Recommended future prefabs live under `Assets/Prefabs/Environment/JellyMaze`.
- Later map-piece prefabs can be named `Tile_Ground_JellyMaze`, `Wall_Hard_JellyMaze_GlassBlock`, and `Wall_Soft_JellyMaze_EnergyCube`.

## 4.3 Current Environment Decoration Direction

Reference document:

- `Docs/EnvironmentDecorationGuide.md`

Decoration principles:

- Keep decoration outside playable grid cells.
- Prefer clear corner and edge props over visual noise near the center of the board.
- Use static props for stable theme identity.
- Use small code-driven motion only for ambience props such as lamps, balloons, glow orbs, and beacons.

Current Candy Park additions:

- Small trees
- Toy barrels
- Candy lamps
- Background clouds

Current Jelly Maze additions:

- Energy barrels
- Floating glow orbs
- Hologram sign
- Data towers

Implementation note:

- `EnvironmentDecorationAnimator` owns bob, spin, and scale-pulse motion for visual-only props.
- Decoration objects are generated under `DecorationRoot` and are not registered with `MapManager`.

## 5. Hard Wall And Soft Wall Style

Hard wall direction:

- Should look permanent, heavy, and safe as a boundary/blocker
- Rounded cream stone, toy brick, or candy block
- Slight bevels and darker base shadow
- Low saturation compared with active gameplay objects

Soft wall direction:

- Should look fragile, playful, and breakable
- Jelly cube, wooden toy crate, gift box, or candy crate
- Brighter colors than hard walls
- Can have cracks, seams, or a soft wobble effect later

Low-cost implementation options:

- Unity primitives:
  - cubes with colorful materials
  - small top cap cube for hard wall
  - simple crossed strips on soft wall using thin cubes
- Blender / Blockbench:
  - rounded cube with bevels
  - crate mesh with simple grooves
  - jelly cube with slightly inflated shape
- Later free generic assets:
  - stylized crates, blocks, low-poly rocks
  - recolor materials to match the BubbleTown palette

## 5.1 Current Phase 2 Battle Map Placeholder Set

The first Phase 2 map art pass uses simple Unity geometry and reusable prefabs.

Scene organization:

- `MapRoot`
  - `Ground_CandyParkBoard`
  - `WallVisualsRoot`

Current prefab set:

- `Assets/Prefabs/Map/Tile_Ground_CandyPark.prefab`
  - Root: `Tile_Ground_CandyPark`
  - Child: `TileBase`
  - Child: `TileInset`
  - Purpose: one readable candy-park floor cell
- `Assets/Prefabs/Map/Wall_Hard_RoundedBlock.prefab`
  - Root: `Wall_Hard_RoundedBlock`
  - Component: `BoxCollider`
  - Component: `WallFeedback`
  - Child: `VisualRoot`
  - Under `VisualRoot`: `BaseBlock`, `TopCap`, `BottomShadow`, four small `CornerDot_*` highlights
  - Purpose: sturdy permanent blocker
- `Assets/Prefabs/Map/Wall_Soft_JellyCrate.prefab`
  - Root: `Wall_Soft_JellyCrate`
  - Component: `BoxCollider`
  - Component: `WallFeedback`
  - Child: `VisualRoot`
  - Under `VisualRoot`: `JellyBody`
  - Purpose: lightweight breakable wall that can be safely destroyed as one object

Current material set:

- `Mat_Tile_GrassPastel`
- `Mat_Tile_CandyBlue`
- `Mat_Tile_CheckerAccent`
- `Mat_Wall_Hard_Cream`
- `Mat_Wall_Hard_Highlight`
- `Mat_Wall_Hard_Shadow`
- `Mat_Wall_Soft_JellyBlue`

Implementation note:

- `MapManager.mapVisualRoot` should point to `WallVisualsRoot`, not the full map root.
- This keeps soft wall destruction from accidentally registering floor tiles as soft wall visuals.
- `MapArtSetup` can regenerate these prefabs and rewire the `Battle` scene from the editor menu or batchmode.

Current wall feedback behavior:

- Hard walls:
  - Do not break
  - Play a short visual-only punch/shake when they block an explosion
  - Keep collision and grid logic stable because only `VisualRoot` animates
- Soft walls:
  - Immediately clear logical soft wall state when destroyed
  - Play a short shake and shrink animation before disappearing
  - Spawn a few tiny cube shards as low-cost placeholder debris
  - Disable colliders during the destroy feedback so movement/bomb logic does not get blocked by a dying wall visual

## 6. Bomb And Explosion Style

Bomb direction:

- Cute bubble-bomb rather than realistic explosive
- Round glossy sphere with small fuse or cap
- Clear readable scale: should fit inside one grid cell
- Add a subtle pulse while counting down

Explosion direction:

- Bubble splash / foam burst / colorful cross blast
- Should show clear cross-shaped grid coverage
- Avoid realistic fire/explosive violence
- Use short-lived rounded spheres, rings, or transparent bubbles

Low-cost implementation options:

- Unity primitives:
  - sphere bomb with dark blue or purple material
  - small cylinder or capsule for fuse/cap
  - explosion cells as translucent spheres
  - scale pulse animation in existing scripts
- Blender / Blockbench:
  - bomb mesh with fuse cap
  - bubble burst mesh or ring
- Later free generic assets:
  - stylized bubble particles
  - toon smoke puffs
  - non-realistic cartoon VFX textures

Recommended prefab names:

- `Bomb_BubbleBasic.prefab`
- `Explosion_BubbleCenter.prefab`
- `Explosion_BubbleArm.prefab`
- `VFX_BombFusePulse.prefab`

Recommended material names:

- `Mat_Bomb_Body_BubbleNavy`
- `Mat_Bomb_Highlight_Cyan`
- `Mat_Bomb_TopCap_Cream`
- `Mat_Bomb_Fuse_Cocoa`
- `Mat_Bomb_Spark_Yellow`
- `Mat_Explosion_Core_Cream`
- `Mat_Explosion_Bubble_Cyan`
- `Mat_Explosion_Arm_Orange`
- `Mat_Explosion_Spark_Pink`

## 6.1 Current Phase 2 Bubble-Bomb Placeholder

The current bomb art pass keeps the existing `Assets/Prefabs/Gameplay/Bomb.prefab` name and upgrades the visual hierarchy.

Current prefab hierarchy:

- `Bomb`
  - Component: `SphereCollider`
  - Component: `BombController`
  - `VisualRoot`
    - `Body_BubbleSphere`
    - `Body_CartoonHighlight`
    - `TopCap_CreamButton`
    - `Fuse_CurvedStem`
    - `Fuse_Spark`

Current material set:

- `Mat_Bomb_Body_BubbleNavy`
- `Mat_Bomb_Highlight_Cyan`
- `Mat_Bomb_TopCap_Cream`
- `Mat_Bomb_Fuse_Cocoa`
- `Mat_Bomb_Spark_Yellow`

Current feedback behavior:

- `BombController` owns the fuse timer and visual countdown feedback.
- The bomb renderers receive emission pulses through `MaterialPropertyBlock`.
- Flash interval blends from slow to fast as the fuse approaches zero.
- `VisualRoot` also gets a small scale pulse during the flash.
- This keeps the effect cheap and easy to replace with particles or Animator later.

Low-cost next upgrades:

- Add a tiny looping fuse spark particle.
- Add a stronger last-half-second squash pulse.
- Replace the cylinder fuse with a simple curved mesh from Blender or Blockbench.

## 6.2 Current Phase 2 Bubble Explosion Placeholder

The current explosion art pass keeps the classic cross-shaped gameplay and gives each explosion cell a more playful visual role.

Current prefab set:

- `Assets/Prefabs/Gameplay/ExplosionCenter.prefab`
  - Component: `SphereCollider`
  - Component: `Rigidbody`
  - Component: `ExplosionController`
  - `VisualRoot`
    - `Center_CoreBubble`
    - `Center_CyanPop_North`
    - `Center_CyanPop_South`
    - `Center_PinkSpark_East`
    - `Center_PinkSpark_West`
- `Assets/Prefabs/Gameplay/ExplosionHorizontal.prefab`
  - Component: `BoxCollider`
  - Component: `Rigidbody`
  - Component: `ExplosionController`
  - `VisualRoot`
    - `Horizontal_OrangeSplash`
    - `Horizontal_Bubble_Left`
    - `Horizontal_Bubble_Right`
    - `Horizontal_Spark_Top`
- `Assets/Prefabs/Gameplay/ExplosionVertical.prefab`
  - Component: `BoxCollider`
  - Component: `Rigidbody`
  - Component: `ExplosionController`
  - `VisualRoot`
    - `Vertical_OrangeSplash`
    - `Vertical_Bubble_North`
    - `Vertical_Bubble_South`
    - `Vertical_Spark_Top`

Current material set:

- `Mat_Explosion_Core_Cream`
- `Mat_Explosion_Bubble_Cyan`
- `Mat_Explosion_Arm_Orange`
- `Mat_Explosion_Spark_Pink`

Current feedback behavior:

- `BombController` chooses center, horizontal, or vertical explosion prefab while preserving the existing grid propagation rules.
- `ExplosionController` owns each explosion cell lifetime, hit detection, scale pulse, rotation pulse, and emission pulse.
- The visuals use rounded bubble shapes, warm orange, cyan, cream, and pink rather than realistic fire.
- This keeps the result closer to a Q-style casual game and safer for a cute presentation.

Low-cost next upgrades:

- Add short-lived particle puffs to the center prefab only.
- Add tiny star sprites or simple mesh stars to the arm prefabs.
- Add a soft pop sound when each explosion cell spawns.

## 7. Item Style

Target style:

- Items must be readable at a glance from the battle camera
- Use simple icon-like 3D shapes
- Each item should have a clear color identity
- Add gentle floating/bobbing, rotation, and glow to draw attention

Current item categories:

- Bomb count up
- Explosion range up
- Move speed up

Suggested visual language:

- Bomb count up:
  - small bomb icon or plus bubble
  - blue/cyan color
- Explosion range up:
  - starburst, flame-free burst, or cross symbol
  - orange/yellow color
- Move speed up:
  - wing, boot, arrow, or lightning-like shape
  - green color

Low-cost implementation options:

- Unity primitives:
  - sphere/capsule placeholders with unique colors
  - small plus/cross made from thin cubes
- Blender / Blockbench:
  - simple icon meshes: plus, starburst, boot, arrow
  - keep each item under a few hundred triangles
- Later free generic assets:
  - generic power-up icons
  - simple 3D icon packs with editable materials

Recommended prefab names:

- `Item_BombCountUp.prefab`
- `Item_ExplosionRangeUp.prefab`
- `Item_MoveSpeedUp.prefab`

Recommended material names:

- `Mat_Item_BombCount_Body_Cyan`
- `Mat_Item_BombCount_Icon_Cream`
- `Mat_Item_BombCount_MiniBomb_Navy`
- `Mat_Item_Range_Body_Orange`
- `Mat_Item_Range_Icon_Yellow`
- `Mat_Item_Range_Spark_Pink`
- `Mat_Item_Speed_Body_Lime`
- `Mat_Item_Speed_Icon_White`
- `Mat_Item_Speed_Wing_Cyan`
- `Mat_Item_Common_Glow_Cream`

## 7.1 Current Phase 2 Item Placeholder Set

The current item art pass uses Unity primitives, clear color coding, and icon-like silhouettes.

Current prefab set:

- `Assets/Prefabs/Gameplay/Items/Item_BombCountUp.prefab`
  - Root: `Item_BombCountUp`
  - Component: `SphereCollider` trigger
  - Component: kinematic `Rigidbody`
  - Component: `ItemBase`
  - Component: `ItemVisualAnimator`
  - Child: `VisualRoot`
  - Under `VisualRoot`: `Token_CyanBubble`, mini-bomb parts, plus icon cubes, and `Glow_FloorRing`
  - Visual read: cyan bomb-count power-up
- `Assets/Prefabs/Gameplay/Items/Item_ExplosionRangeUp.prefab`
  - Root: `Item_ExplosionRangeUp`
  - Component: `SphereCollider` trigger
  - Component: kinematic `Rigidbody`
  - Component: `ItemBase`
  - Component: `ItemVisualAnimator`
  - Child: `VisualRoot`
  - Under `VisualRoot`: `Token_OrangeBurst`, yellow cross-range arms, pink spark dots, and `Glow_FloorRing`
  - Visual read: orange/yellow explosion-range power-up
- `Assets/Prefabs/Gameplay/Items/Item_MoveSpeedUp.prefab`
  - Root: `Item_MoveSpeedUp`
  - Component: `SphereCollider` trigger
  - Component: kinematic `Rigidbody`
  - Component: `ItemBase`
  - Component: `ItemVisualAnimator`
  - Child: `VisualRoot`
  - Under `VisualRoot`: `Token_LimeCapsule`, forward arrow cubes, cyan speed wings, and `Glow_FloorRing`
  - Visual read: lime speed power-up

Current behavior:

- `ItemBase` remains responsible only for pickup, map item state cleanup, and stat application.
- `ItemVisualAnimator` owns floating, rotation, scale pulse, and emission pulse.
- `ItemPickupFeedback` owns the pickup disappear animation and optional audio hook.
- `CharacterPickupFeedback` owns the short collector highlight flash.
- `BattleUI` listens for pickup events and displays a temporary text/icon-style toast.
- The gameplay root stays stable so trigger pickup and grid state do not move around with the visual bob.
- `ItemDropSetup` can regenerate these prefabs and rewire `Battle` from the Unity editor menu or batchmode.

Pickup feedback structure:

- Item root:
  - `ItemBase`
  - `ItemVisualAnimator`
  - `ItemPickupFeedback`
  - trigger collider and kinematic rigidbody
- Character root or runtime-added component:
  - `CharacterPickupFeedback`
- `BattleUI`:
  - displays text such as `[Bomb Slot +1]`, `[Range +1]`, or `[Speed Up]`

Pickup feedback behavior:

- The item immediately clears its logical map item state.
- The item dispatches a pickup event for the HUD.
- The collecting character briefly flashes with a color matching the item type.
- The item disables its trigger collider to prevent duplicate pickup.
- The item rises, spins, shrinks, pulses emission, and destroys itself.
- `ItemPickupFeedback.pickupClip` is intentionally exposed but empty for now so later audio can be assigned without code changes.

Low-cost next upgrades:

- Add real pickup pop particles.
- Add short pickup sounds per item type.
- Replace cube icons with simple Blender or Blockbench meshes while keeping the same prefab names.
- Add small HUD feedback when an item changes bomb count, explosion range, or move speed.

## 8. UI Style

Target style:

- Rounded, friendly, colorful
- Big readable buttons
- Simple icons and soft panels
- No official logos or copied layouts
- Works on desktop and basic windowed play mode

MainMenu direction:

- Big original `BubbleTown` title
- Bright bubble/candy background
- Start button as primary action

ModeSelect direction:

- Three large cards:
  - Single Player
  - AI Battle
  - Local VS
- Each card can have a simple icon later

MapSelect direction:

- Map cards with small preview thumbnails
- For now, previews can be colored panels or mini tile mockups

Battle HUD direction:

- Keep it small and readable
- Show mode, player state, bomb count, range, maybe timer later
- Show a clear `READY -> GO!` round opening prompt
- Use a short `SAFE` HUD state for spawn protection at the start of combat
- In `LocalVS`, show round number and placeholder Best of 3 score clearly enough for couch play
- Avoid covering the play area

Result direction:

- Large result title
- Winner name
- In `LocalVS`, show final `P1 X - Y P2` score
- Retry and Main Menu buttons
- Later: stats such as time, defeated players, items collected

Low-cost implementation options:

- Current MVP:
  - IMGUI placeholder screens
- Next step:
  - Unity Canvas with TextMeshPro if available
  - rounded panels using simple sprites
  - button color states
- Later free generic assets:
  - generic rounded UI packs
  - icon packs for arrows, stars, boots, bombs
  - verify licenses before committing assets

Recommended UI asset names:

- `UI_Panel_RoundedBubble`
- `UI_Button_Primary`
- `UI_Button_Secondary`
- `UI_Icon_ModeSingle`
- `UI_Icon_ModeAI`
- `UI_Icon_ModeLocalVS`

## 9. Low-Cost Asset Strategy

Use Unity primitives when:

- Testing gameplay readability
- Replacing flat placeholders with simple color-coded forms
- Creating first-pass bombs, blocks, tiles, item pickups
- The exact model silhouette is not important yet

Use Blender / Blockbench when:

- A rounded or chibi silhouette matters
- A primitive cube/sphere no longer communicates the object well
- You need repeatable prefab-ready models
- You want simple bevels, rounded edges, or icon shapes

Use free generic assets later when:

- The system is already stable
- The asset can be recolored to match the project palette
- The license is clear for GitHub/project use
- The asset is generic and not tied to a copyrighted game identity

Avoid:

- Official game characters, UI, logos, sounds, music, or map themes
- Fan recreations of copyrighted characters
- Assets with unclear redistribution licenses
- High-poly realistic assets that clash with the chibi style

## 10. Suggested Technical Specs

Models:

- Low-poly or simple stylized meshes
- Character target: under `2k` triangles per character for now
- Blocks/items: under `500` triangles each if custom-made
- Pivot at grid-cell center or base center depending on object type
- Scale should fit the existing `1.0` unit grid

Textures:

- Prefer flat colors and materials first
- If textures are needed:
  - `512x512` for small props and items
  - `1024x1024` for character atlases or larger environment pieces
- Use PNG for sprites/icons and simple textures

Materials:

- Use simple Standard/URP-compatible stylized materials
- Avoid complex shaders until gameplay is stable
- Keep material names descriptive and reusable

Prefabs:

- One prefab per gameplay object type
- Keep visual children under a `VisualRoot` child when possible
- Keep gameplay scripts on the root object
- Keep colliders simple and aligned to existing grid logic

## 11. Phase 2 Asset Checklist

Priority 1: Readability upgrades

- Ground tile material
- Hard wall prefab
- Soft wall prefab
- Player1 chibi placeholder prefab
- Player2 chibi placeholder prefab
- AI chibi placeholder prefab
- Bubble bomb prefab
- Bubble explosion cell prefab
- Three item visual prefabs

Priority 2: Presentation upgrades

- Main menu background
- Rounded button sprite or simple panel sprite
- Mode select icons
- Map select preview thumbnails
- Battle HUD panel
- Result screen panel
- Basic pickup VFX
- Basic bomb fuse pulse VFX
- Basic explosion pop VFX

Priority 3: Atmosphere upgrades

- Candy park / toy garden background props
- Decorative border props outside the playable grid
- Simple ambient music
- Bomb place sound
- Bomb fuse sound
- Explosion pop sound
- Item pickup sound
- Victory/defeat stinger

## 12. Recommended Phase 2 Daily Order

1. Define materials and palette assets
2. Replace hard/soft wall visuals
3. Replace ground tile visuals
4. Create chibi character placeholder prefabs
5. Improve bomb visual and fuse feedback
6. Improve explosion visual feedback
7. Improve item visuals and pickup feedback
8. Replace IMGUI menus with simple styled Canvas screens
9. Improve battle HUD readability
10. Improve result screen presentation
11. Add basic sound effects
12. Add light polish pass: camera, screen shake, particles, small animations
