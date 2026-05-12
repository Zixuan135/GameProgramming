namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Purpose: Describes the logical type of one grid cell in the Bomberman-style map.
    /// Inputs: assigned by map generation or map rules.
    /// Output: enum values used for movement, bomb placement, and explosion blocking.
    /// </summary>
    public enum CellType
    {
        /// <summary>Walkable cell with no wall by default.</summary>
        Empty = 0,

        /// <summary>Permanent wall cell that blocks movement and explosions.</summary>
        HardWall = 1,

        /// <summary>Breakable wall cell that blocks movement and stops explosions when destroyed.</summary>
        SoftWall = 2,

        /// <summary>Reserved spawn cell for a player or AI character.</summary>
        SpawnPoint = 3
    }
}
