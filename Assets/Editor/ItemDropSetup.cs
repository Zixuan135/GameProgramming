using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Items;
using BubbleTown.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Creates placeholder item prefabs and wires the Battle scene item drop setup.
    /// </summary>
    public static class ItemDropSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string ItemPrefabFolder = "Assets/Prefabs/Gameplay/Items";
        private const string MaterialFolder = "Assets/Materials";

        private const string BombCountPrefabPath = ItemPrefabFolder + "/Item_BombCountUp.prefab";
        private const string ExplosionRangePrefabPath = ItemPrefabFolder + "/Item_ExplosionRangeUp.prefab";
        private const string MoveSpeedPrefabPath = ItemPrefabFolder + "/Item_MoveSpeedUp.prefab";

        private const string BombCountMaterialPath = MaterialFolder + "/ItemBombCountUpPlaceholder.mat";
        private const string ExplosionRangeMaterialPath = MaterialFolder + "/ItemExplosionRangeUpPlaceholder.mat";
        private const string MoveSpeedMaterialPath = MaterialFolder + "/ItemMoveSpeedUpPlaceholder.mat";

        [MenuItem("BubbleTown/Setup/Ensure Item Drops")]
        public static void EnsureItemDrops()
        {
            EnsureFolders();

            ItemBase bombCountPrefab = EnsureItemPrefab(
                BombCountPrefabPath,
                BombCountMaterialPath,
                "Item_BombCountUp",
                ItemType.BombCountUp,
                new Color(0.2f, 0.75f, 1f),
                PrimitiveType.Sphere);

            ItemBase explosionRangePrefab = EnsureItemPrefab(
                ExplosionRangePrefabPath,
                ExplosionRangeMaterialPath,
                "Item_ExplosionRangeUp",
                ItemType.ExplosionRangeUp,
                new Color(1f, 0.55f, 0.08f),
                PrimitiveType.Sphere);

            ItemBase moveSpeedPrefab = EnsureItemPrefab(
                MoveSpeedPrefabPath,
                MoveSpeedMaterialPath,
                "Item_MoveSpeedUp",
                ItemType.MoveSpeedUp,
                new Color(0.25f, 1f, 0.35f),
                PrimitiveType.Capsule);

            ConfigureBattleSceneItemSpawner(bombCountPrefab, explosionRangePrefab, moveSpeedPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[ItemDropSetup] Item drop prefabs and Battle scene setup are ready.");
        }

        public static void EnsureItemDropsFromBatchmode()
        {
            EnsureItemDrops();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Prefabs", "Gameplay");
            EnsureFolder("Assets/Prefabs/Gameplay", "Items");
        }

        private static void EnsureFolder(string parentFolder, string childFolder)
        {
            string fullPath = parentFolder + "/" + childFolder;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentFolder, childFolder);
            }
        }

        private static Material EnsureMaterial(string materialPath, string materialName, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
            {
                material.color = color;
                EditorUtility.SetDirty(material);
                return material;
            }

            material = new Material(Shader.Find("Standard"));
            material.name = materialName;
            material.color = color;
            AssetDatabase.CreateAsset(material, materialPath);
            return material;
        }

        private static ItemBase EnsureItemPrefab(
            string prefabPath,
            string materialPath,
            string itemName,
            ItemType itemType,
            Color color,
            PrimitiveType primitiveType)
        {
            Material material = EnsureMaterial(materialPath, itemName + "Material", color);
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                GameObject itemObject = CreateItemObject(itemName, material, primitiveType);
                ConfigureItem(itemObject.GetComponent<ItemBase>(), itemType);
                prefabAsset = PrefabUtility.SaveAsPrefabAsset(itemObject, prefabPath);
                Object.DestroyImmediate(itemObject);
            }
            else
            {
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
                EnsureItemVisual(prefabContents, material);
                EnsureItemPhysics(prefabContents);
                ItemBase itemBase = prefabContents.GetComponent<ItemBase>();
                if (itemBase == null)
                {
                    itemBase = prefabContents.AddComponent<ItemBase>();
                }

                ConfigureItem(itemBase, itemType);
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabContents);
                prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            return prefabAsset != null ? prefabAsset.GetComponent<ItemBase>() : null;
        }

        private static GameObject CreateItemObject(string itemName, Material material, PrimitiveType primitiveType)
        {
            GameObject itemObject = GameObject.CreatePrimitive(primitiveType);
            itemObject.name = itemName;
            itemObject.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            EnsureItemVisual(itemObject, material);
            EnsureItemPhysics(itemObject);

            itemObject.AddComponent<ItemBase>();
            return itemObject;
        }

        private static void EnsureItemVisual(GameObject itemObject, Material material)
        {
            MeshRenderer renderer = itemObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = itemObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private static void EnsureItemPhysics(GameObject itemObject)
        {
            Collider collider = itemObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            Rigidbody rigidbody = itemObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = itemObject.AddComponent<Rigidbody>();
            }

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        private static void ConfigureItem(ItemBase itemBase, ItemType itemType)
        {
            if (itemBase == null)
            {
                return;
            }

            SerializedObject serializedItem = new SerializedObject(itemBase);
            serializedItem.FindProperty("itemType").intValue = (int)itemType;
            serializedItem.FindProperty("pickupOnTrigger").boolValue = true;
            serializedItem.FindProperty("destroyAfterPickup").boolValue = true;
            serializedItem.FindProperty("bombCountDelta").intValue = GameConstants.DefaultItemBombCountDelta;
            serializedItem.FindProperty("explosionRangeDelta").intValue = GameConstants.DefaultItemExplosionRangeDelta;
            serializedItem.FindProperty("moveSpeedDelta").floatValue = GameConstants.DefaultItemMoveSpeedDelta;
            serializedItem.FindProperty("clearMapItemOnDestroy").boolValue = true;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemBase);
        }

        private static void ConfigureBattleSceneItemSpawner(
            ItemBase bombCountPrefab,
            ItemBase explosionRangePrefab,
            ItemBase moveSpeedPrefab)
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);

            Transform itemsRoot = GameObject.Find("ItemsRoot")?.transform;
            if (itemsRoot == null)
            {
                GameObject itemsRootObject = new GameObject("ItemsRoot");
                itemsRoot = itemsRootObject.transform;
            }

            MapManager mapManager = Object.FindObjectOfType<MapManager>();
            ItemSpawner itemSpawner = itemsRoot.GetComponent<ItemSpawner>();
            if (itemSpawner == null)
            {
                itemSpawner = itemsRoot.gameObject.AddComponent<ItemSpawner>();
            }

            SerializedObject serializedSpawner = new SerializedObject(itemSpawner);
            serializedSpawner.FindProperty("mapManager").objectReferenceValue = mapManager;
            serializedSpawner.FindProperty("itemRoot").objectReferenceValue = itemsRoot;
            serializedSpawner.FindProperty("dropChance").floatValue = GameConstants.DefaultItemDropChance;
            serializedSpawner.FindProperty("registerMapItemState").boolValue = true;
            serializedSpawner.FindProperty("preventSpawnOnBlockedCell").boolValue = true;
            serializedSpawner.FindProperty("spawnHeight").floatValue = GameConstants.DefaultItemSpawnHeight;
            serializedSpawner.FindProperty("spawnOnSoftWallDestroyed").boolValue = true;
            serializedSpawner.FindProperty("logDropResults").boolValue = true;

            SerializedProperty itemDefinitions = serializedSpawner.FindProperty("itemDefinitions");
            itemDefinitions.arraySize = 3;
            ConfigureSpawnDefinition(itemDefinitions.GetArrayElementAtIndex(0), ItemType.BombCountUp, bombCountPrefab, 1f);
            ConfigureSpawnDefinition(itemDefinitions.GetArrayElementAtIndex(1), ItemType.ExplosionRangeUp, explosionRangePrefab, 1f);
            ConfigureSpawnDefinition(itemDefinitions.GetArrayElementAtIndex(2), ItemType.MoveSpeedUp, moveSpeedPrefab, 1f);

            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemSpawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureSpawnDefinition(
            SerializedProperty definition,
            ItemType itemType,
            ItemBase prefab,
            float weight)
        {
            definition.FindPropertyRelative("itemType").intValue = (int)itemType;
            definition.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            definition.FindPropertyRelative("weight").floatValue = weight;
        }
    }
}
