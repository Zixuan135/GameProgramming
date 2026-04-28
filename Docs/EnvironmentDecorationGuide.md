# Environment Decoration Guide

This guide defines how BubbleTown adds Battle scene decoration without hurting grid readability or gameplay stability.

## Goals

- Make Battle scenes feel richer than a plain logic test board.
- Keep the playable grid easy to read from the angled camera.
- Keep decorations visual-only unless they are safely outside the map boundary.
- Use runtime primitives now, then migrate the strongest props into prefabs later.

## Placement Rules

- Keep decorative props outside the hard wall border whenever possible.
- Do not register decorations with `MapManager`.
- Avoid colliders unless the prop is far outside the playable grid.
- Do not place decorations where they can look like bombs, items, destructible walls, or walkable tiles.
- Keep spawn areas visually clean.
- Keep the center grid quieter than the outer border.

## Static Decoration

Good static props:

- Trees and bushes
- Barrels and crates
- Fence rails
- Sign boards
- Crystal clusters
- Data towers
- Background clouds or silhouettes

Static props should support the theme without attracting more attention than gameplay objects.

## Animated Decoration

Good animated props:

- Lamps with subtle scale pulse
- Balloons with slight bobbing
- Floating glow orbs
- Signal beacons
- Hologram signs
- Slow background clouds

`EnvironmentDecorationAnimator` provides lightweight bob, spin, and scale-pulse motion. Use it only on ambience props, never on gameplay walls, players, bombs, items, or explosions.

## Runtime Hierarchy

Generated decoration should live under the active map theme root:

```text
MapRoot
  GeneratedMap_CandyPark
    DecorationRoot

MapRoot
  GeneratedMap_JellyMaze
    DecorationRoot
```

## Prefab Organization

When a runtime primitive prop is worth keeping, turn it into a prefab under the matching theme folder:

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

## Naming Rules

- Pure decoration prefabs should start with `Prop_`.
- Theme-specific props should include the theme name.
- Animated props can include a child named `AnimatedRoot`.
- Visual children should use descriptive names such as `LampHead_Glow`, `OrbCore`, or `BarrelBody`.
