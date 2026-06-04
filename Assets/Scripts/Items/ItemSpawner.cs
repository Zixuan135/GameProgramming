using System;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
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

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            EnsureMapManager();
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            SubscribeToMapManager();
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            SubscribeToMapManager();
        }

        /// <summary>
        /// Purpose: Cleans up subscriptions or runtime state when this component becomes inactive.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnDisable()
        {
            UnsubscribeFromMapManager();
        }

        /// <summary>
        /// Purpose: Handles soft wall destroyed.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        private void HandleSoftWallDestroyed(Vector2Int gridPosition)
        {
            if (!spawnOnSoftWallDestroyed)
            {
                return;
            }

            bool spawned = TrySpawnTutorialItemAtGrid(gridPosition, out ItemBase spawnedItem) ||
                TrySpawnRandomItemAtGrid(gridPosition, out spawnedItem);
            if (!logDropResults)
            {
                return;
            }

            string itemName = spawnedItem != null ? spawnedItem.ItemType.ToString() : "None";
            Debug.Log($"[ItemSpawner] Soft wall destroyed at {gridPosition}. Spawned: {spawned}. Item: {itemName}");
        }

        /// <summary>
        /// Purpose: Guarantees the tutorial teaching wall drops a readable power-up.
        /// Inputs: gridPosition identifies the soft wall that was destroyed.
        /// Output: true when a tutorial item was spawned.
        /// </summary>
        /// <param name="gridPosition">Destroyed soft wall grid position.</param>
        /// <param name="spawnedItem">Spawned tutorial item, or null.</param>
        /// <returns>True if a tutorial item was spawned.</returns>
        private bool TrySpawnTutorialItemAtGrid(Vector2Int gridPosition, out ItemBase spawnedItem)
        {
            spawnedItem = null;
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.IsTutorialMode || !gameManager.IsTutorialSoftWallGrid(gridPosition))
            {
                return false;
            }

            ItemSpawnDefinition definition = FindDefinition(gameManager.TutorialGuaranteedItemType) ?? FindFirstValidDefinition();
            if (definition == null)
            {
                return false;
            }

            return TrySpawnItemAtGrid(definition.ItemType, gridPosition, out spawnedItem);
        }

        /// <summary>
        /// Purpose: Attempts to spawn random item at grid.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public bool TrySpawnRandomItemAtGrid(Vector2Int gridPosition)
        {
            return TrySpawnRandomItemAtGrid(gridPosition, out _);
        }

        /// <summary>
        /// Purpose: Attempts to spawn random item at grid.
        /// Inputs: `gridPosition`, `spawnedItem`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <param name="spawnedItem">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Attempts to spawn item at grid.
        /// Inputs: `itemType`, `gridPosition`, `spawnedItem`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="itemType">Input value used by this method.</param>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <param name="spawnedItem">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Attempts to spawn item.
        /// Inputs: `worldPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="worldPosition">Input value used by this method.</param>
        public void TrySpawnItem(Vector3 worldPosition)
        {
            TrySpawnRandomItem(worldPosition, out _);
        }

        /// <summary>
        /// Purpose: Attempts to spawn random item.
        /// Inputs: `worldPosition`, `spawnedItem`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="worldPosition">Input value used by this method.</param>
        /// <param name="spawnedItem">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Returns whether this object can spawn at grid.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Resolves world position from the current runtime state.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
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

        /// <summary>
        /// Purpose: Finds definition from scene objects or cached data.
        /// Inputs: `itemType`; may also read serialized fields and current runtime state.
        /// Output: a `ItemSpawnDefinition` value.
        /// </summary>
        /// <param name="itemType">Input value used by this method.</param>
        /// <returns>a `ItemSpawnDefinition` value.</returns>
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

        /// <summary>
        /// Purpose: Returns pick random definition for the current state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `ItemSpawnDefinition` value.
        /// </summary>
        /// <returns>a `ItemSpawnDefinition` value.</returns>
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

        /// <summary>
        /// Purpose: Finds the first configured item as a deterministic tutorial fallback.
        /// Inputs: no direct parameters; reads serialized item definitions.
        /// Output: a valid item definition, or null when none exists.
        /// </summary>
        /// <returns>First valid item definition.</returns>
        private ItemSpawnDefinition FindFirstValidDefinition()
        {
            if (itemDefinitions == null)
            {
                return null;
            }

            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                ItemSpawnDefinition definition = itemDefinitions[i];
                if (definition != null && definition.IsValid)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Purpose: Ensures map manager exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void EnsureMapManager()
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }
        }

        /// <summary>
        /// Purpose: Performs subscribe to map manager for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Performs unsubscribe from map manager for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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
