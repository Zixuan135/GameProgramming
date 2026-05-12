namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Purpose: Lists the playable battle map themes/patterns that can be selected before a match.
    /// Inputs: no runtime input; values are chosen by UI or stored in GameManager.
    /// Output: enum values used by map generation, camera framing, and result display.
    /// </summary>
    public enum BattleMapType
    {
        /// <summary>Balanced candy park map used as the default arena.</summary>
        Default = 0,

        /// <summary>More open arena with wider movement lanes.</summary>
        OpenField = 1,

        /// <summary>Tighter maze-like arena with more route planning.</summary>
        Maze = 2
    }
}
