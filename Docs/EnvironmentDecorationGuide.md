# Environment Decoration Guide

This guide defines how BubbleTown should add Battle scene decoration without hurting grid readability or gameplay stability.

## Goals

- Make the Battle scene feel richer and less like an empty test board.
- Keep decoration outside the playable grid unless it is purely visual and non-blocking.
- Keep gameplay readability stronger than background charm.
- Use cheap primitive props first, then replace them with reusable prefabs later.

## Static Decoration

Static decoration is best for objects that should support the theme without drawing too much attention.

Good examples:

- Small trees
- Bushes
- Barrels
- Crates
- Sign posts
- Fence rails
- Crystal clusters
- Data towers
- Background clouds or distant silhouettes

Rules:

- Place these outside the hard wall border.
- Do not register them with `MapManager`.
- Avoid colliders unless the object is far outside the playable area.
- Keep colors slightly softer than bombs, explosions, players, and items.

## Simple Animated Decoration

Animated decoration is best for small atmosphere details that make the scene feel alive.

Good examples:

- Lamps with subtle scale pulse
- Balloons with slight bobbing
- Floating glow orbs
- Signal beacon lights
- Hologram signs
- Slow-moving background clouds

Current implementation:

- `EnvironmentDecorationAnimator` provides lightweight bob, spin, and scale-pulse motion.
- It is intended for decorative objects only.
- It should not be used on gameplay walls, players, bombs, items, or explosions.

Rules:

- Keep motion small and slow.
- Do not animate the whole map root.
- Avoid fast spinning near the center of the gameplay grid.
- Use stronger animation only at the outer edge or in the background.

## Runtime Hierarchy

Generated decoration should live under the active map theme root:

```text
MapRoot
  GeneratedMap_CandyPark
    DecorationRoot
      Fence_South
      LollipopTree_SouthWest
      SmallTree_WestSouth
      ToyBarrel_West
      CandyLamp_NorthWest
      Cloud_Background_NorthWest

MapRoot
  GeneratedMap_JellyMaze
    DecorationRoot
      NeonGate_South
      CrystalCluster_SouthWest
      EnergyBarrel_WestSouth
      FloatingOrb_NorthWest
      HoloSign_JellyMaze
      DataTower_East
```

## Future Prefab Organization

Recommended folder structure:

```text
Assets/Prefabs/Environment/CandyPark
  Prop_CandyPark_SmallTree.prefab
  Prop_CandyPark_ToyBarrel.prefab
  Prop_CandyPark_CandyLamp.prefab
  Prop_CandyPark_BackgroundCloud.prefab

Assets/Prefabs/Environment/JellyMaze
  Prop_JellyMaze_EnergyBarrel.prefab
  Prop_JellyMaze_FloatingGlowOrb.prefab
  Prop_JellyMaze_HoloSign.prefab
  Prop_JellyMaze_DataTower.prefab
```

Naming rules:

- Pure decoration prefabs should start with `Prop_`.
- Theme-specific prefabs should include the theme name.
- Animated decoration can include a child named `AnimatedRoot`.
- Keep visible child names descriptive, such as `LampHead_Glow`, `OrbCore`, or `BarrelBody`.

## Readability Rules

Use these checks before adding more decoration:

- The player spawn areas must remain visually clear.
- The center gameplay grid must stay less noisy than the outer border.
- Bombs and explosion arms must remain the brightest moving objects.
- Soft walls must remain easier to identify than background props.
- Decoration should never look like a walkable tile, bomb, item, or destructible wall.
- Prefer fewer larger props at corners over many tiny props along every edge.

## Current Theme Additions

Candy Park additions:

- `SmallTree_WestSouth`
- `SmallTree_EastNorth`
- `ToyBarrel_West`
- `ToyBarrel_East`
- `CandyLamp_NorthWest`
- `CandyLamp_SouthEast`
- `Cloud_Background_NorthWest`
- `Cloud_Background_SouthEast`

Jelly Maze additions:

- `EnergyBarrel_WestSouth`
- `EnergyBarrel_EastNorth`
- `FloatingOrb_NorthWest`
- `FloatingOrb_SouthEast`
- `HoloSign_JellyMaze`
- `DataTower_East`
- `DataTower_West`

## Low-Cost Upgrade Path

Suggested next steps:

1. Keep runtime primitives while tuning placement and density.
2. Convert the most successful props into prefabs under `Assets/Prefabs/Environment`.
3. Add simple material variants per theme.
4. Add subtle particle effects only to lamps, orbs, and beacons.
5. Add per-map decoration density settings after the first two themes feel stable.
