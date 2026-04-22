using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Coordinates map lifecycle and links map generation with battle scene setup.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MapGenerator mapGenerator;

        [Header("Runtime")]
        [SerializeField] private BattleMapType selectedMapType = BattleMapType.Default;

        public BattleMapType SelectedMapType => selectedMapType;

        private void Start()
        {
            GenerateMap();
        }

        public void SetMapType(BattleMapType mapType)
        {
            selectedMapType = mapType;
        }

        public void GenerateMap()
        {
            if (mapGenerator == null)
            {
                Debug.LogWarning("[MapManager] MapGenerator is not assigned.");
                return;
            }

            mapGenerator.Generate(selectedMapType);
        }

        public void ClearMap()
        {
            if (mapGenerator == null)
            {
                return;
            }

            mapGenerator.Clear();
        }
    }
}
