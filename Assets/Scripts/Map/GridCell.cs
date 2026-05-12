using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Pure data container for one logical grid cell.
    /// Gameplay uses this data while visual presentation stays 3D.
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        public Vector2Int GridPosition { get; private set; }

        public bool IsHardWall { get; set; }
        public bool IsSoftWall { get; set; }
        public bool HasBomb { get; set; }
        public bool HasCharacter { get; set; }
        public bool HasItem { get; set; }

        /// <summary>
        /// Purpose: Creates a new logical grid cell at the given coordinate.
        /// Inputs: x is the grid column and y is the grid row.
        /// Output: returns a GridCell instance with all blocking and occupancy flags cleared.
        /// </summary>
        /// <param name="x">Grid column index.</param>
        /// <param name="y">Grid row index.</param>
        public GridCell(int x, int y)
        {
            GridPosition = new Vector2Int(x, y);
            IsHardWall = false;
            IsSoftWall = false;
            HasBomb = false;
            HasCharacter = false;
            HasItem = false;
        }

        /// <summary>
        /// Purpose: Clears dynamic flags.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void ClearDynamicFlags()
        {
            HasBomb = false;
            HasCharacter = false;
            HasItem = false;
        }
    }
}
