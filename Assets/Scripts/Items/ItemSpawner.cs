using System;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Map;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BubbleTown.Items
{
    /// <summary>
    /// Lightweight item spawn entry point.
    /// Later soft-wall destruction can call this class without owning item selection logic itself.
    /// </summary>
    public class ItemSpawner : MonoBehaviour
    {
        [Serializable]
        public class ItemSpawnDefinition
        {
            [SerializeField] private ItemType itemType = ItemType.None;
            [SerializeField] private ItemBase prefab;
            [SerializeField, Min(0f)] private float weight = 1f;

            public ItemType ItemType => itemType;
            public ItemBase Prefab => prefab;
            public float Weight => weight;
            public bool IsValid => itemType != ItemType.None && prefab != null && weight > 0f;
        }

        [Header("References")]
        [SerializeField] private MapManager mapManager;
        [SerializeField] private Transform itemRoot;

        [Header("Spawn Config")]
        [SerializeField] private ItemSpawnDefinition[] itemDefinitions = new ItemSpawnDefinition[0];
        [SerializeField, Range(0f, 1f)] private float dropChance = GameConstants.DefaultItemDropChance;
        [SerializeField] private bool registerMapItemState = true;
        [SerializeField] private bool preventSpawnOnBlockedCell = true;
        [SerializeField] private float spawnHeight = GameConstants.DefaultItemSpawnHeight;

        [Header("Soft Wall Drops")]
        [SerializeField] private bool spawnOnSoftWallDestroyed = true;
        [SerializeField] private bool logDropResults = true;

        private MapManager subscribedMapManager;

        public float DropChance => dropChance;
        public MapManager MapManager => mapManager;
        public Transform ItemRoot => itemRoot;
        public bool SpawnOnSoftWallDestroyed => spawnOnSoftWallDestroyed;

        private void Awake()
        {
            EnsureMapManager();
        }

        private void OnEnable()
        {
            SubscribeToMapManager();
        }

        private void Start()
        {
            SubscribeToMapManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromMapManager();
        }

        private void HandleSoftWallDestroyed(Vector2Int gridPosition)
        {
            if (!spawnOnSoftWallDestroyed)
            {
                return;
            }

            bool spawned = TrySpawnRandomItemAtGrid(gridPosition, out ItemBase spawnedItem);
            if (!logDropResults)
            {
                return;
            }

            string itemName = spawnedItem != null ? spawnedItem.ItemType.ToString() : "None";
            Debug.Log($"[ItemSpawner] Soft wall destroyed at {gridPosition}. Spawned: {spawned}. Item: {itemName}");
        }

        public bool TrySpawnRandomItemAtGrid(Vector2Int gridPosition)
        {
            return TrySpawnRandomItemAtGrid(gridPosition, out _);
        }

        public bool TrySpawnRandomItemAtGrid(Vector2Int gridPosition, out ItemBase spawnedItem)
        {
            spawnedItem = null;

            if (Random.value > dropChance)
            {
                return false;
            }

            ItemSpawnDefinition definition = PickRandomDefinition();
            if (definition == null)
            {
                return false;
            }

            return TrySpawnItemAtGrid(definition.ItemType, gridPosition, out spawnedItem);
        }

        public bool TrySpawnItemAtGrid(ItemType itemType, Vector2Int gridPosition, out ItemBase spawnedItem)
        {
            spawnedItem = null;

            EnsureMapManager();
            if (!CanSpawnAtGrid(gridPosition))
            {
                return false;
            }

            ItemSpawnDefinition definition = FindDefinition(itemType);
            if (definition == null)
            {
                Debug.LogWarning($"[ItemSpawner] No item prefab configured for {itemType}.");
                return false;
            }

            Vector3 worldPosition = ResolveWorldPosition(gridPosition);
            spawnedItem = Instantiate(definition.Prefab, worldPosition, Quaternion.identity, itemRoot);
            spawnedItem.Initialize(definition.ItemType, mapManager, gridPosition);

            if (registerMapItemState && mapManager != null)
            {
                mapManager.SetItem(gridPosition, true);
            }

            return true;
        }

        public void TrySpawnItem(Vector3 worldPosition)
        {
            TrySpawnRandomItem(worldPosition, out _);
        }

        public bool TrySpawnRandomItem(Vector3 worldPosition, out ItemBase spawnedItem)
        {
            spawnedItem = null;

            EnsureMapManager();
            if (mapManager != null)
            {
                return TrySpawnRandomItemAtGrid(mapManager.WorldToGrid(worldPosition), out spawnedItem);
            }

            if (Random.value > dropChance)
            {
                return false;
            }

            ItemSpawnDefinition definition = PickRandomDefinition();
            if (definition == null)
            {
                return false;
            }

            spawnedItem = Instantiate(definition.Prefab, worldPosition, Quaternion.identity, itemRoot);
            spawnedItem.Initialize(definition.ItemType);
            return true;
        }

        public bool CanSpawnAtGrid(Vector2Int gridPosition)
        {
            EnsureMapManager();
            if (mapManager == null)
            {
                return true;
            }

            GridCell cell = mapManager.GetCell(gridPosition);
            if (cell == null)
            {
                return false;
            }

            if (cell.HasItem)
            {
                return false;
            }

            if (!preventSpawnOnBlockedCell)
            {
                return true;
            }

            return !cell.IsHardWall && !cell.IsSoftWall && !cell.HasBomb && !cell.HasCharacter;
        }

        private Vector3 ResolveWorldPosition(Vector2Int gridPosition)
        {
            if (mapManager != null)
            {
                return mapManager.GridToWorld(gridPosition, spawnHeight);
            }

            return new Vector3(
                gridPosition.x * GameConstants.GridCellSize,
                spawnHeight,
                gridPosition.y * GameConstants.GridCellSize);
        }

        private ItemSpawnDefinition FindDefinition(ItemType itemType)
        {
            if (itemDefinitions == null)
            {
                return null;
            }

            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                ItemSpawnDefinition definition = itemDefinitions[i];
                if (definition != null && definition.IsValid && definition.ItemType == itemType)
                {
                    return definition;
                }
            }

            return null;
        }

        private ItemSpawnDefinition PickRandomDefinition()
        {
            if (itemDefinitions == null || itemDefinitions.Length == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                ItemSpawnDefinition definition = itemDefinitions[i];
                if (definition != null && definition.IsValid)
                {
                    totalWeight += definition.Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float roll = Random.Range(0f, totalWeight);
            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                ItemSpawnDefinition definition = itemDefinitions[i];
                if (definition == null || !definition.IsValid)
                {
                    continue;
                }

                roll -= definition.Weight;
                if (roll <= 0f)
                {
                    return definition;
                }
            }

            return null;
        }

        private void EnsureMapManager()
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }
        }

        private void SubscribeToMapManager()
        {
            EnsureMapManager();
            if (mapManager == null || subscribedMapManager == mapManager)
            {
                return;
            }

            UnsubscribeFromMapManager();
            subscribedMapManager = mapManager;
            subscribedMapManager.SoftWallDestroyed += HandleSoftWallDestroyed;
        }

        private void UnsubscribeFromMapManager()
        {
            if (subscribedMapManager == null)
            {
                return;
            }

            subscribedMapManager.SoftWallDestroyed -= HandleSoftWallDestroyed;
            subscribedMapManager = null;
        }
    }
}
