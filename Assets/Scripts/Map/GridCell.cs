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

        public GridCell(int x, int y)
        {
            GridPosition = new Vector2Int(x, y);
            IsHardWall = false;
            IsSoftWall = false;
            HasBomb = false;
            HasCharacter = false;
            HasItem = false;
        }

        public void ClearDynamicFlags()
        {
            HasBomb = false;
            HasCharacter = false;
            HasItem = false;
        }
    }
}
