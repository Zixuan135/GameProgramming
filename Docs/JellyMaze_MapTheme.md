# Jelly Maze Map Theme

Jelly Maze is the second Battle scene theme for BubbleTown.
It uses the existing `BattleMapType.Maze` map rules and gives that layout a much stronger visual identity than Candy Park.

## Theme Goal

Jelly Maze should feel like a glowing toy-lab maze:

- Clearly different from the pastel outdoor Candy Park theme
- Darker violet floor base with cyan glow lanes
- Sturdy glassy hard walls that feel permanent
- Magenta jelly soft walls that read as breakable
- Low-cost neon props around the edge of the board
- Strong grid readability from the angled Battle camera

## Current Implementation Status

Jelly Maze is generated at runtime in the `Battle` scene when `BattleMapType.Maze` is selected.

Implemented behavior:

- `MapManager` still owns all gameplay grid data.
- `MapGenerator` selects `JellyMaze` visuals for `BattleMapType.Maze`.
- Ground, hard wall, soft wall, and decoration visuals are generated with runtime primitive objects.
- Candy Park prefabs are still used for `Default` and `OpenField`.
- Soft wall visuals are registered back into `MapManager` so destruction, item drops, and feedback remain synchronized.
- Older generated map theme roots are cleared when switching themes.

Runtime root:

```text
MapRoot
  GeneratedMap_JellyMaze
    GroundRoot
    HardWallRoot
    SoftWallRoot
    DecorationRoot
```

## Floor Style

Target look:

- Dark violet jelly-lab floor
- Cyan glowing lane inset per tile
- Small center light dot to make the grid feel active
- Softer background contrast than bombs, explosions, items, and characters

Current low-cost construction:

- Root object per tile: `Tile_xx_yy`
- Child objects:
  - `TileBase_DarkJelly`
  - `TileInset_GlowLane`
  - `TileCenterDot`

Recommended future material names:

- `Mat_Tile_JellyMaze_FloorViolet`
- `Mat_Tile_JellyMaze_GlowLaneCyan`
- `Mat_Tile_JellyMaze_DotCream`

## Hard Wall Style

Target look:

- Permanent glowing lab block
- More technological and stable than soft walls
- Violet glass body with cyan/cream glow cap
- Dark base shadow to keep it grounded

Current low-cost construction:

- Root object per hard wall: `Wall_Hard_xx_yy`
- Components:
  - `BoxCollider`
- Child objects:
  - `BottomShadow`
  - `GlassBlock`
  - `GlowCap`
  - `CornerLight`

Recommended future prefab name:

- `Wall_Hard_JellyMaze_GlassBlock.prefab`

Recommended future material names:

- `Mat_Wall_JellyMaze_Hard_VioletGlass`
- `Mat_Wall_JellyMaze_Hard_GlowCap`
- `Mat_Wall_JellyMaze_DarkFrame`

## Soft Wall Style

Target look:

- Breakable jelly energy cube
- Brighter and less stable than hard walls
- Magenta body with cyan/cream glowing details
- Should feel like it can pop when hit by an explosion

Current low-cost construction:

- Root object per soft wall: `Wall_Soft_xx_yy`
- Components:
  - `BoxCollider`
- Child objects:
  - `JellyCore`
  - `GlowStripe_X`
  - `GlowStripe_Z`
  - `BubbleShine`

Recommended future prefab name:

- `Wall_Soft_JellyMaze_EnergyCube.prefab`

Recommended future material names:

- `Mat_Wall_JellyMaze_Soft_MagentaJelly`
- `Mat_Wall_JellyMaze_Soft_CyanStripe`
- `Mat_Wall_JellyMaze_Soft_BubbleShine`

## Decoration List

Current generated primitive decorations:

- `NeonGate_South`
- `NeonGate_North`
- `NeonGate_West`
- `NeonGate_East`
- `CrystalCluster_SouthWest`
- `CrystalCluster_NorthEast`
- `GlowTube_SouthEast`
- `GlowTube_NorthWest`
- `SignalBeacon_North`
- `SignalBeacon_South`
- `EnergyBarrel_WestSouth`
- `EnergyBarrel_EastNorth`
- `FloatingOrb_NorthWest`
- `FloatingOrb_SouthEast`
- `HoloSign_JellyMaze`
- `DataTower_East`
- `DataTower_West`

Prefab suggestions for later:

- `Prop_JellyMaze_NeonGateLine.prefab`
- `Prop_JellyMaze_CrystalCluster.prefab`
- `Prop_JellyMaze_GlowTube.prefab`
- `Prop_JellyMaze_SignalBeacon.prefab`
- `Prop_JellyMaze_EnergyBarrel.prefab`
- `Prop_JellyMaze_FloatingGlowOrb.prefab`
- `Prop_JellyMaze_HoloSign.prefab`
- `Prop_JellyMaze_DataTower.prefab`

Placement rules:

- Keep decorative colliders out of the playable grid.
- Prefer placing props just outside the hard wall border.
- Use glow props sparingly so explosions and items remain readable.
- Reuse a few repeated props instead of making many one-off decorations.
- Use `EnvironmentDecorationAnimator` only on visual ambience props such as orbs, beacons, holograms, and antenna lights.

## Color Palette

Recommended colors:

- Floor violet: `#3D2E6B`
- Dark frame: `#1F1A38`
- Glow cyan: `#2EEAFF`
- Soft wall magenta: `#FF54D1`
- Hard wall violet: `#6D56E6`
- Cream glow: `#C7FFF5`
- Accent pink: `#FF6BE6`

Readability notes:

- Use cyan and cream for small highlights only.
- Keep the floor darker than gameplay objects.
- Keep hard walls cooler and heavier than soft walls.
- Let soft walls carry the strongest magenta so they are easy to identify as destructible.

## Prefab And Material Organization

Recommended future folder structure:

```text
Assets/Prefabs/Environment/JellyMaze
  Prop_JellyMaze_NeonGateLine.prefab
  Prop_JellyMaze_CrystalCluster.prefab
  Prop_JellyMaze_GlowTube.prefab
  Prop_JellyMaze_SignalBeacon.prefab

Assets/Prefabs/Map
  Tile_Ground_JellyMaze.prefab
  Wall_Hard_JellyMaze_GlassBlock.prefab
  Wall_Soft_JellyMaze_EnergyCube.prefab
```

Recommended naming rules:

- Runtime-generated root: `GeneratedMap_JellyMaze`
- Pure decorations: `Prop_JellyMaze_*`
- Map gameplay pieces: `Tile_Ground_JellyMaze`, `Wall_Hard_JellyMaze_*`, `Wall_Soft_JellyMaze_*`
- Materials: `Mat_JellyMaze_*`

## MapSelect Preview Direction

The `Jelly Maze` card should preview:

- Dark violet ground
- Violet hard walls
- Cyan route line
- Tighter maze-like blocker pattern

This helps the player understand that `Maze` is not just a rules variant; it is now a distinct map theme.

## Low-Cost Next Upgrades

- Convert generated Jelly Maze primitives into reusable prefabs.
- Add 2 to 3 tile variants to reduce repetition.
- Add a simple pulse animation to `GlowLane` or `SignalBeacon`.
- Add a subtle blue/purple area light if performance remains fine.
- Add one background skybox or gradient plane that matches the neon-lab mood.
