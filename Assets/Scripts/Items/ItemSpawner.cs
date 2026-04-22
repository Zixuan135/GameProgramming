using UnityEngine;

namespace BubbleTown.Items
{
    /// <summary>
    /// Spawns random item prefabs after breakable wall destruction.
    /// Current version provides only simple chance-based spawning.
    /// </summary>
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private ItemBase[] itemPrefabs;
        [SerializeField] [Range(0f, 1f)] private float dropChance = 0.3f;

        public void TrySpawnItem(Vector3 worldPosition)
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0)
            {
                return;
            }

            if (Random.value > dropChance)
            {
                return;
            }

            int randomIndex = Random.Range(0, itemPrefabs.Length);
            Instantiate(itemPrefabs[randomIndex], worldPosition, Quaternion.identity);
        }
    }
}
