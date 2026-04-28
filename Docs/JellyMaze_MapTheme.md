# Jelly Maze Map Theme

Jelly Maze is BubbleTown's darker neon toy-lab theme. It gives `BattleMapType.Maze` a distinct identity from Candy Park while preserving the same grid gameplay rules.

## Current Status

Jelly Maze is generated at runtime in `Battle` when `BattleMapType.Maze` is selected.

Runtime hierarchy:

```text
MapRoot
  GeneratedMap_JellyMaze
    GroundRoot
    HardWallRoot
    SoftWallRoot
    DecorationRoot
```

`MapManager` still owns all gameplay state. `MapGenerator` only builds visuals and registers soft wall visual objects back into `MapManager`.

## Visual Rules

Floor:

- Dark violet base
- Cyan glow lanes or dots to show the grid
- Lower brightness than gameplay objects

Hard walls:

- Permanent glassy lab blocks
- Violet/cyan/cream accents
- Heavier and cooler than soft walls

Soft walls:

- Breakable magenta jelly energy cubes
- Brighter and less stable than hard walls
- Should visually pop when destroyed

Decorations:

- Neon lab props outside the playable border
- Glow should support the theme without overpowering bombs, items, or explosions

## Current Generated Props

- `NeonGate_*`
- `CrystalCluster_*`
- `GlowTube_*`
- `SignalBeacon_*`
- `EnergyBarrel_*`
- `FloatingOrb_*`
- `HoloSign_JellyMaze`
- `DataTower_*`

Animated ambience can use `EnvironmentDecorationAnimator` on props such as glow orbs, signal beacons, and holograms.

## Palette

- Floor violet: `#3D2E6B`
- Dark frame: `#1F1A38`
- Glow cyan: `#2EEAFF`
- Soft wall magenta: `#FF54D1`
- Hard wall violet: `#6D56E6`
- Cream glow: `#C7FFF5`
- Accent pink: `#FF6BE6`

## Naming Rules

- Runtime root: `GeneratedMap_JellyMaze`
- Generated instances: `Tile_xx_yy`, `Wall_Hard_xx_yy`, `Wall_Soft_xx_yy`
- Future map prefabs: `Tile_Ground_JellyMaze`, `Wall_Hard_JellyMaze_*`, `Wall_Soft_JellyMaze_*`
- Future props: `Prop_JellyMaze_*`
- Materials: `Mat_JellyMaze_*` or `Mat_Wall_JellyMaze_*`

## Future Replacements

When replacing generated primitives with reusable assets, place them under:

```text
Assets/Prefabs/Environment/JellyMaze
Assets/Prefabs/Map/JellyMaze
Assets/Materials/Map/JellyMaze
```
