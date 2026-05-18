# Snowfield Playground Map Theme

Snowfield Playground gives BubbleTown a bright winter toy-board map that contrasts with Candy Park and Jelly Maze while keeping the same readable grid combat.

## Current Status

Snowfield Playground is generated at runtime in `Battle` when the player selects the Snowfield map. Internally this still uses `BattleMapType.OpenField` so existing save/session flow remains stable.

Runtime hierarchy:

```text
MapRoot
  GeneratedMap_SnowfieldPlayground
    GroundRoot
    HardWallRoot
    SoftWallRoot
    DecorationRoot
    GoalRoot
```

## Visual Language

Floor:

- Pale snow base with icy blue tile insets
- Small sparkle details to make the ground feel colder without hiding the grid

Hard walls:

- Packed snow and ice blocks with wooden braces
- Should look sturdy and capable of blocking explosions

Soft walls:

- Gift-crate blocks with bright ribbon accents
- Should read as lighter, playful, and breakable

Decorations:

- Snow fences, pine trees, snowmen, gift crates, ice lamps, and soft snow clouds
- Keep props outside the playable border so movement and explosion reading stay clean

## Asset Folders

```text
Assets/Materials/Map/SnowfieldPlayground
Assets/Prefabs/Map/SnowfieldPlayground
Assets/Prefabs/Environment/SnowfieldPlayground
```

Runtime primitives currently fill the theme, but these folders are ready for future handmade prefabs or polished materials.
