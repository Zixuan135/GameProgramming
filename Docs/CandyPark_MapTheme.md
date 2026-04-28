# Candy Park Map Theme

Candy Park is the first complete Battle scene theme for BubbleTown.
It is designed to stay low-cost, readable from the angled 3D camera, and easy to replace with stronger art later.

## Theme Goal

Candy Park should feel like a colorful toy-board arena:

- Bright, cute, and rounded
- Clear grid readability on the XZ plane
- Hard walls read as permanent blockers
- Soft walls read as breakable objects
- Decorative props stay outside or near the edge of the playable grid
- All art remains original and avoids copying official Bomberman or Crazy Arcade assets

## Current Reusable Assets

Existing map prefab set:

- `Assets/Prefabs/Map/Tile_Ground_CandyPark.prefab`
- `Assets/Prefabs/Map/Wall_Hard_RoundedBlock.prefab`
- `Assets/Prefabs/Map/Wall_Soft_JellyCrate.prefab`

Existing map material set:

- `Assets/Materials/Mat_Tile_GrassPastel.mat`
- `Assets/Materials/Mat_Tile_CandyBlue.mat`
- `Assets/Materials/Mat_Tile_CheckerAccent.mat`
- `Assets/Materials/Mat_Wall_Hard_Cream.mat`
- `Assets/Materials/Mat_Wall_Hard_Highlight.mat`
- `Assets/Materials/Mat_Wall_Hard_Shadow.mat`
- `Assets/Materials/Mat_Wall_Soft_JellyBlue.mat`

These are enough for the first themed pass. No new gameplay logic is required before the map theme is visually testable.

## Current Implementation Status

Candy Park is now generated at runtime in the `Battle` scene.

Implemented behavior:

- `MapManager` still owns the logical grid, spawn reservations, hard walls, soft walls, bomb occupancy, character occupancy, and item flags.
- `MapGenerator` reads the current `MapManager` grid and builds the visual layer.
- Ground tiles are generated for every grid cell.
- Hard wall visuals are generated for cells where `GridCell.IsHardWall` is true.
- Soft wall visuals are generated for cells where `GridCell.IsSoftWall` is true.
- Soft wall objects are registered back into `MapManager` so destruction feedback and item drops still work.
- Decorative Candy Park props are generated outside the playable grid.
- Existing scene-authored visual roots are hidden at runtime to avoid duplicate tiles and walls.

Runtime root:

```text
MapRoot
  GeneratedMap_CandyPark
    GroundRoot
    HardWallRoot
    SoftWallRoot
    DecorationRoot
```

Scene-authored fallback roots:

- `Ground_CandyParkBoard`
- `WallVisualsRoot`

These remain in the scene as edit-time reference/fallback visuals, but the runtime generator now owns the active Battle view.

## Floor Style

Target look:

- Pastel grass and candy-tile board
- Slightly raised square tiles
- Clear tile boundaries without high-detail textures
- Soft contrast so bombs, explosions, characters, and items remain readable

Low-cost construction:

- Use cube primitives scaled to one grid cell
- Root prefab: `Tile_Ground_CandyPark`
- Child objects:
  - `TileBase`: wide shallow cube, pastel green
  - `TileInset`: smaller shallow cube, candy blue or checker accent

Recommended colors:

- Base green: `#91E8A6`
- Candy blue inset: `#7EDBFF`
- Cream highlight: `#FFF2CC`
- Soft shadow: `#5A4B42`

Optional upgrade later:

- Add 2 to 3 tile color variants, such as `Tile_Ground_CandyPark_A`, `Tile_Ground_CandyPark_B`, and `Tile_Ground_CandyPark_C`.
- Randomly rotate or alternate tile accents in `MapGenerator`.

## Hard Wall Style

Target look:

- Permanent candy-stone blocks
- Sturdy, stable, and slightly heavier than soft walls
- Cream body with warm shadow and highlight pieces
- Shape should fit inside one grid cell and not hide adjacent tile readability

Low-cost construction:

- Root prefab: `Wall_Hard_RoundedBlock`
- Components:
  - `BoxCollider` on the root
  - `WallFeedback` on the root
- Child objects:
  - `VisualRoot`
  - `BottomShadow`
  - `BaseBlock`
  - `TopHighlight`
  - optional corner candy dots

Recommended colors:

- Main cream: `#FFF2CC`
- Highlight: `#FFFFFF`
- Shadow caramel: `#B8864D`
- Corner candy accent: `#7EDBFF` or `#FF8AC7`

Behavior notes:

- Hard walls must never be destroyed.
- When hit by an explosion, use the existing `WallFeedback.PlayHardWallBlockedFeedback` behavior.
- The visual should shake or punch subtly, then return to normal.

## Soft Wall Style

Target look:

- Breakable jelly crate or candy gel block
- Lighter and more playful than hard walls
- Clearly destructible from a distance
- Should produce a stronger feedback moment when destroyed

Low-cost construction:

- Root prefab: `Wall_Soft_JellyCrate`
- Components:
  - `BoxCollider` on the root
  - `WallFeedback` on the root
- Child objects:
  - `VisualRoot`
  - `JellyBody`
  - optional `RibbonX`
  - optional `RibbonZ`
  - optional small candy dot or shine cap

Recommended colors:

- Jelly blue: `#5DDCFF`
- Jelly pink variant: `#FF87C8`
- Jelly orange variant: `#FFB04A`
- Cream shine: `#FFF8D6`

Behavior notes:

- Soft walls block explosion propagation.
- Soft walls are destroyed when hit by an explosion.
- Existing destruction feedback can use shrink, shake, and small primitive shards.
- Item drops should spawn at the same grid cell after destruction.

## Scene Decoration Suggestions

Keep decorations out of walkable gameplay cells unless they are purely visual and non-blocking.

Recommended low-cost props:

- `Prop_CandyPark_FenceSegment`
  - Thin cubes outside the grid
  - Cream posts and pastel rails
- `Prop_CandyPark_LollipopTree`
  - Cylinder stick plus sphere candy top
  - Place outside the battle border
- `Prop_CandyPark_BalloonCluster`
  - Small spheres on thin cylinders
  - Good for menu-like background charm
- `Prop_CandyPark_SignBoard`
  - Cube board with text later
  - Can sit near a corner outside playable area
- `Prop_CandyPark_RoundBush`
  - Squashed sphere with green material
  - Useful for background edge decoration
- `Prop_CandyPark_SmallTree`
  - Cylinder trunk plus rounded leaf puffs
  - Best as static corner/edge decoration
- `Prop_CandyPark_ToyBarrel`
  - Cylinder body with colored bands
  - Best as static edge decoration
- `Prop_CandyPark_CandyLamp`
  - Pole plus glowing sphere head
  - Good for subtle bob or scale-pulse ambience
- `Prop_CandyPark_BackgroundCloud`
  - Clustered spheres placed higher and farther out
  - Good for slow bobbing background motion

Decoration rules:

- Do not place decorative colliders inside the playable grid.
- If a prop needs a collider, put it outside the map boundary.
- Prefer 6 to 12 repeated props rather than many unique meshes.
- Keep colors softer than bombs, explosions, and items.
- Use `EnvironmentDecorationAnimator` only for non-gameplay ambience props.

## Material List

Current minimum set:

- `Mat_Tile_GrassPastel`
- `Mat_Tile_CandyBlue`
- `Mat_Tile_CheckerAccent`
- `Mat_Wall_Hard_Cream`
- `Mat_Wall_Hard_Highlight`
- `Mat_Wall_Hard_Shadow`
- `Mat_Wall_Soft_JellyBlue`

Recommended next materials:

- `Mat_Wall_Soft_JellyPink`
- `Mat_Wall_Soft_JellyOrange`
- `Mat_Prop_CandyFence_Cream`
- `Mat_Prop_Lollipop_Red`
- `Mat_Prop_Lollipop_Cyan`
- `Mat_Prop_Bush_Mint`
- `Mat_Prop_Sign_WoodToy`

## Prefab Organization

Recommended folder structure:

```text
Assets/Prefabs/Map
  Tile_Ground_CandyPark.prefab
  Wall_Hard_RoundedBlock.prefab
  Wall_Soft_JellyCrate.prefab

Assets/Prefabs/Environment/CandyPark
  Prop_CandyPark_FenceSegment.prefab
  Prop_CandyPark_LollipopTree.prefab
  Prop_CandyPark_BalloonCluster.prefab
  Prop_CandyPark_SignBoard.prefab
  Prop_CandyPark_RoundBush.prefab
```

Recommended scene hierarchy:

```text
Battle
  MapRoot
    GroundRoot
    HardWallRoot
    SoftWallRoot
    DecorationRoot
  CharactersRoot
  BombsRoot
  ItemsRoot
  RuntimeSystems
```

Prefab naming rule:

- Gameplay map pieces start with `Tile_` or `Wall_`.
- Pure decorations start with `Prop_CandyPark_`.
- Materials start with `Mat_`.
- Keep visual child names stable: `VisualRoot`, `Body`, `Highlight`, `Shadow`.

## MapGenerator Integration Notes

The first runtime generator pass is implemented.

Current serialized prefab references on `MapGenerator`:

- `groundTilePrefab`
- `hardWallPrefab`
- `softWallPrefab`

Current generation responsibilities:

- `MapManager.InitializeGridData()` remains the source of logical grid truth.
- `MapGenerator.Generate(mapType, mapManager)` instantiates visuals based on `MapManager` grid data.
- `MapGenerator` registers soft wall instances with `MapManager.RegisterSoftWallObject(gridPos, instance)`.
- Decorations are generated outside the logical map bounds so they do not affect movement or explosions.
- Small trees, toy barrels, candy lamps, and background clouds are generated under `DecorationRoot`.
- Candy lamps and clouds use subtle code-driven motion without touching gameplay data.

Recommended next step later:

- Replace primitive generated decoration objects with real `Prop_CandyPark_*` prefabs.
- Add optional `decorationPrefabs` to `MapGenerator`.
- Add tile material variants or prefab variants for less repetition.

Suggested generated hierarchy:

```text
GeneratedMapRoot
  GroundRoot
  HardWallRoot
  SoftWallRoot
  DecorationRoot
```

Suggested generation rules:

- Spawn one ground tile at every grid coordinate.
- Spawn hard wall prefab wherever `GridCell.IsHardWall` is true.
- Spawn soft wall prefab wherever `GridCell.IsSoftWall` is true.
- Do not spawn decorations inside playable cells.
- For border decoration, place props one or two cells outside the map bounds.

## First Pass Acceptance Checklist

- The grid remains easy to read from the Battle camera.
- Player, bomb, explosion, and item colors stand out from the floor.
- Hard walls feel solid and permanent.
- Soft walls feel breakable and playful.
- Spawn areas stay visually clear.
- Decoration props do not block movement or explosion logic.
- All assets are low-cost primitives or original placeholder art.
