using UnityEngine;

namespace BubbleTown
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private ItemBase[] itemPrefabs;
        [SerializeField] private float spawnChance = 0.35f;

        public bool TrySpawnItem(Vector3 position)
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0)
            {
                return false;
            }

            if (Random.value > spawnChance)
            {
                return false;
            }

            ItemBase prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            Instantiate(prefab, position, Quaternion.identity);
            return true;
        }
    }
}
