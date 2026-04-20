using UnityEngine;

namespace BubbleTown
{
    public class MapManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = GameConstants.DefaultMapWidth;
        [SerializeField] private int height = GameConstants.DefaultMapHeight;
        [SerializeField] private float cellSize = GameConstants.DefaultGridSize;
        [SerializeField] private Vector3 origin = Vector3.zero;

        [Header("Collision Query")]
        [SerializeField] private LayerMask blockedLayerMask;

        [Header("Spawn Anchors")]
        [SerializeField] private Transform player1Spawn;
        [SerializeField] private Transform player2Spawn;
        [SerializeField] private Transform ai1Spawn;
        [SerializeField] private Transform ai2Spawn;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        public void BuildMap(int mapId)
        {
            Debug.Log($"BuildMap called. mapId={mapId}, size={width}x{height}");
            // MVP skeleton: this method is intentionally lightweight.
            // Later stages can generate hard/soft walls by mapId.
        }

        public Vector3 GetSpawnWorldPosition(SpawnSlot slot)
        {
            switch (slot)
            {
                case SpawnSlot.Player1:
                    return player1Spawn != null ? player1Spawn.position : CellToWorld(new Vector2Int(1, 1));
                case SpawnSlot.Player2:
                    return player2Spawn != null ? player2Spawn.position : CellToWorld(new Vector2Int(width - 2, height - 2));
                case SpawnSlot.AI1:
                    return ai1Spawn != null ? ai1Spawn.position : CellToWorld(new Vector2Int(width - 2, 1));
                case SpawnSlot.AI2:
                    return ai2Spawn != null ? ai2Spawn.position : CellToWorld(new Vector2Int(1, height - 2));
                default:
                    return origin;
            }
        }

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt((worldPos.x - origin.x) / cellSize);
            int y = Mathf.RoundToInt((worldPos.y - origin.y) / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 CellToWorld(Vector2Int cellPos)
        {
            return origin + new Vector3(cellPos.x * cellSize, cellPos.y * cellSize, 0f);
        }

        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            return CellToWorld(WorldToCell(worldPos));
        }

        public bool IsInsideMap(Vector2Int cellPos)
        {
            return cellPos.x >= 0 && cellPos.x < width && cellPos.y >= 0 && cellPos.y < height;
        }

        public bool IsCellWalkable(Vector2Int cellPos)
        {
            if (!IsInsideMap(cellPos))
            {
                return false;
            }

            Vector3 worldPos = CellToWorld(cellPos);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, cellSize * 0.35f, blockedLayerMask);
            return hit == null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 center = CellToWorld(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f));
                }
            }
        }
#endif
    }
}
