# Candy Park Map Theme

Candy Park is BubbleTown's bright outdoor toy-board theme. It should read as friendly, colorful, and simple from the angled Battle camera while keeping gameplay objects easy to identify.

## Current Status

Candy Park is generated at runtime in `Battle` for `BattleMapType.Default` and `BattleMapType.OpenField`.

Runtime hierarchy:

```text
MapRoot
  GeneratedMap_CandyPark
    GroundRoot
    HardWallRoot
    SoftWallRoot
    DecorationRoot
```

The old scene-authored fallback visuals are grouped under `LegacyVisualsRoot_Disabled` in the Battle scene.

## Reusable Assets

Map prefabs:

- `Assets/Prefabs/Map/CandyPark/Tile_Ground_CandyPark.prefab`
- `Assets/Prefabs/Map/CandyPark/Wall_Hard_RoundedBlock.prefab`
- `Assets/Prefabs/Map/CandyPark/Wall_Soft_JellyCrate.prefab`

Map materials:

- `Assets/Materials/Map/CandyPark/Mat_Tile_GrassPastel.mat`
- `Assets/Materials/Map/CandyPark/Mat_Tile_CandyBlue.mat`
- `Assets/Materials/Map/CandyPark/Mat_Tile_CheckerAccent.mat`
- `Assets/Materials/Map/CandyPark/Mat_Wall_Hard_Cream.mat`
- `Assets/Materials/Map/CandyPark/Mat_Wall_Hard_Highlight.mat`
- `Assets/Materials/Map/CandyPark/Mat_Wall_Hard_Shadow.mat`
- `Assets/Materials/Map/CandyPark/Mat_Wall_Soft_JellyBlue.mat`

## Visual Rules

Floor:

- Pastel grass/candy-board look
- Low contrast compared with characters, bombs, items, and explosions
- Clear grid boundaries without noisy texture detail

Hard walls:

- Cream candy-stone blocks
- Stable, heavier, and more permanent than soft walls
- Should use subtle blocked-hit feedback, not destruction

Soft walls:

- Breakable jelly crate feel
- Lighter and more playful than hard walls
- Should shrink/shake/pop when destroyed

Decorations:

- Keep decorative props outside the playable grid or near the border
- Use fewer larger props rather than many small noisy props
- Keep decoration colors softer than gameplay-critical objects

## Current Decoration Set

Candy Park currently uses runtime primitive props such as:

- `Fence_*`
- `LollipopTree_*`
- `BalloonCluster_*`
- `RoundBush_*`
- `Sign_CandyPark`
- `SmallTree_*`
- `ToyBarrel_*`
- `CandyLamp_*`
- `Cloud_Background_*`

Use `EnvironmentDecorationAnimator` only for non-gameplay ambience pieces, such as balloons, lamps, and background clouds.

## Naming Rules

- Map pieces: `Tile_Ground_CandyPark`, `Wall_Hard_RoundedBlock`, `Wall_Soft_JellyCrate`
- Generated instances: `Tile_xx_yy`, `Wall_Hard_xx_yy`, `Wall_Soft_xx_yy`
- Decoration prefabs: `Prop_CandyPark_*`
- Materials: `Mat_Tile_*`, `Mat_Wall_*`, or `Mat_CandyPark_*` when the asset is theme-specific

## Future Replacements

When moving from runtime primitives to authored prefabs, place them under:

```text
Assets/Prefabs/Environment/CandyPark
Assets/Prefabs/Map/CandyPark
Assets/Materials/Map/CandyPark
```
