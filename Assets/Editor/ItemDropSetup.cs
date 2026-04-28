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
    /// Creates recognizable low-cost item prefabs and wires the Battle scene item drop setup.
    /// </summary>
    public static class ItemDropSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string ItemPrefabFolder = "Assets/Prefabs/Gameplay/Items";
        private const string MaterialFolder = "Assets/Materials/Gameplay/Items";

        private const string BombCountPrefabPath = ItemPrefabFolder + "/Item_BombCountUp.prefab";
        private const string ExplosionRangePrefabPath = ItemPrefabFolder + "/Item_ExplosionRangeUp.prefab";
        private const string MoveSpeedPrefabPath = ItemPrefabFolder + "/Item_MoveSpeedUp.prefab";

        private const string BombCountBodyMaterialPath = MaterialFolder + "/Mat_Item_BombCount_Body_Cyan.mat";
        private const string BombCountIconMaterialPath = MaterialFolder + "/Mat_Item_BombCount_Icon_Cream.mat";
        private const string BombCountMiniBombMaterialPath = MaterialFolder + "/Mat_Item_BombCount_MiniBomb_Navy.mat";
        private const string RangeBodyMaterialPath = MaterialFolder + "/Mat_Item_Range_Body_Orange.mat";
        private const string RangeIconMaterialPath = MaterialFolder + "/Mat_Item_Range_Icon_Yellow.mat";
        private const string RangeSparkMaterialPath = MaterialFolder + "/Mat_Item_Range_Spark_Pink.mat";
        private const string SpeedBodyMaterialPath = MaterialFolder + "/Mat_Item_Speed_Body_Lime.mat";
        private const string SpeedIconMaterialPath = MaterialFolder + "/Mat_Item_Speed_Icon_White.mat";
        private const string SpeedWingMaterialPath = MaterialFolder + "/Mat_Item_Speed_Wing_Cyan.mat";
        private const string CommonGlowMaterialPath = MaterialFolder + "/Mat_Item_Common_Glow_Cream.mat";

        private sealed class ItemVisualMaterials
        {
            public Material BombCountBody;
            public Material BombCountIcon;
            public Material BombCountMiniBomb;
            public Material RangeBody;
            public Material RangeIcon;
            public Material RangeSpark;
            public Material SpeedBody;
            public Material SpeedIcon;
            public Material SpeedWing;
            public Material CommonGlow;
        }

        [MenuItem("BubbleTown/Setup/Ensure Item Drops")]
        public static void EnsureItemDrops()
        {
            EnsureFolders();

            ItemVisualMaterials materials = EnsureItemMaterials();
            ItemBase bombCountPrefab = EnsureBombCountItemPrefab(materials);
            ItemBase explosionRangePrefab = EnsureExplosionRangeItemPrefab(materials);
            ItemBase moveSpeedPrefab = EnsureMoveSpeedItemPrefab(materials);

            ConfigureBattleSceneItemSpawner(bombCountPrefab, explosionRangePrefab, moveSpeedPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[ItemDropSetup] Stylized item drop prefabs and Battle scene setup are ready.");
        }

        public static void EnsureItemDropsFromBatchmode()
        {
            EnsureItemDrops();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "Gameplay");
            EnsureFolder("Assets/Materials/Gameplay", "Items");
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

        private static ItemVisualMaterials EnsureItemMaterials()
        {
            return new ItemVisualMaterials
            {
                BombCountBody = EnsureMaterial(
                    BombCountBodyMaterialPath,
                    "Mat_Item_BombCount_Body_Cyan",
                    new Color(0.1f, 0.72f, 1f),
                    new Color(0.02f, 0.32f, 0.55f),
                    0.42f),
                BombCountIcon = EnsureMaterial(
                    BombCountIconMaterialPath,
                    "Mat_Item_BombCount_Icon_Cream",
                    new Color(1f, 0.92f, 0.62f),
                    new Color(0.35f, 0.22f, 0.05f),
                    0.28f),
                BombCountMiniBomb = EnsureMaterial(
                    BombCountMiniBombMaterialPath,
                    "Mat_Item_BombCount_MiniBomb_Navy",
                    new Color(0.08f, 0.12f, 0.32f),
                    new Color(0.02f, 0.08f, 0.22f),
                    0.4f),
                RangeBody = EnsureMaterial(
                    RangeBodyMaterialPath,
                    "Mat_Item_Range_Body_Orange",
                    new Color(1f, 0.48f, 0.14f),
                    new Color(0.75f, 0.18f, 0.02f),
                    0.36f),
                RangeIcon = EnsureMaterial(
                    RangeIconMaterialPath,
                    "Mat_Item_Range_Icon_Yellow",
                    new Color(1f, 0.86f, 0.18f),
                    new Color(1f, 0.5f, 0.04f),
                    0.3f),
                RangeSpark = EnsureMaterial(
                    RangeSparkMaterialPath,
                    "Mat_Item_Range_Spark_Pink",
                    new Color(1f, 0.36f, 0.62f),
                    new Color(1f, 0.1f, 0.28f),
                    0.22f),
                SpeedBody = EnsureMaterial(
                    SpeedBodyMaterialPath,
                    "Mat_Item_Speed_Body_Lime",
                    new Color(0.34f, 1f, 0.32f),
                    new Color(0.08f, 0.5f, 0.08f),
                    0.4f),
                SpeedIcon = EnsureMaterial(
                    SpeedIconMaterialPath,
                    "Mat_Item_Speed_Icon_White",
                    new Color(0.96f, 1f, 0.86f),
                    new Color(0.25f, 0.5f, 0.12f),
                    0.24f),
                SpeedWing = EnsureMaterial(
                    SpeedWingMaterialPath,
                    "Mat_Item_Speed_Wing_Cyan",
                    new Color(0.35f, 0.95f, 1f),
                    new Color(0.05f, 0.42f, 0.55f),
                    0.34f),
                CommonGlow = EnsureMaterial(
                    CommonGlowMaterialPath,
                    "Mat_Item_Common_Glow_Cream",
                    new Color(1f, 0.94f, 0.58f),
                    new Color(0.55f, 0.36f, 0.08f),
                    0.2f)
            };
        }

        private static Material EnsureMaterial(string materialPath, string materialName, Color color, Color emissionColor, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                material.name = materialName;
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", emissionColor);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static ItemBase EnsureBombCountItemPrefab(ItemVisualMaterials materials)
        {
            GameObject itemObject = CreateItemRoot("Item_BombCountUp", ItemType.BombCountUp, out Transform visualRoot);

            CreatePrimitiveVisual(visualRoot, "Token_CyanBubble", PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), new Vector3(0.68f, 0.68f, 0.68f), Quaternion.identity, materials.BombCountBody);
            CreatePrimitiveVisual(visualRoot, "Icon_MiniBomb_Body", PrimitiveType.Sphere, new Vector3(0f, 0.08f, 0.24f), new Vector3(0.24f, 0.24f, 0.24f), Quaternion.identity, materials.BombCountMiniBomb);
            CreatePrimitiveVisual(visualRoot, "Icon_MiniBomb_Top", PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0.24f), new Vector3(0.06f, 0.04f, 0.06f), Quaternion.identity, materials.BombCountIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_Plus_Vertical", PrimitiveType.Cube, new Vector3(0.2f, 0.08f, 0.36f), new Vector3(0.055f, 0.28f, 0.055f), Quaternion.identity, materials.BombCountIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_Plus_Horizontal", PrimitiveType.Cube, new Vector3(0.2f, 0.08f, 0.36f), new Vector3(0.2f, 0.055f, 0.055f), Quaternion.identity, materials.BombCountIcon);
            CreatePrimitiveVisual(visualRoot, "Glow_FloorRing", PrimitiveType.Cylinder, new Vector3(0f, -0.27f, 0f), new Vector3(0.82f, 0.025f, 0.82f), Quaternion.identity, materials.CommonGlow);

            ConfigureItemVisualAnimator(
                itemObject,
                visualRoot,
                new Color(0.22f, 0.95f, 1f),
                new Vector3(0f, 78f, 0f),
                0.075f,
                2.4f,
                0.045f,
                3.1f,
                1.15f);
            ConfigureItemPickupFeedback(
                itemObject,
                visualRoot,
                new Color(0.22f, 0.95f, 1f),
                0.32f,
                0.45f,
                210f);

            return SaveItemPrefab(itemObject, BombCountPrefabPath);
        }

        private static ItemBase EnsureExplosionRangeItemPrefab(ItemVisualMaterials materials)
        {
            GameObject itemObject = CreateItemRoot("Item_ExplosionRangeUp", ItemType.ExplosionRangeUp, out Transform visualRoot);

            CreatePrimitiveVisual(visualRoot, "Token_OrangeBurst", PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), new Vector3(0.68f, 0.68f, 0.68f), Quaternion.identity, materials.RangeBody);
            CreatePrimitiveVisual(visualRoot, "Icon_Range_Core", PrimitiveType.Sphere, new Vector3(0f, 0.1f, 0f), new Vector3(0.22f, 0.22f, 0.22f), Quaternion.identity, materials.RangeIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_RangeArm_EastWest", PrimitiveType.Cylinder, new Vector3(0f, 0.1f, 0f), new Vector3(0.06f, 0.32f, 0.06f), Quaternion.Euler(0f, 0f, 90f), materials.RangeIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_RangeArm_NorthSouth", PrimitiveType.Cylinder, new Vector3(0f, 0.1f, 0f), new Vector3(0.06f, 0.32f, 0.06f), Quaternion.Euler(90f, 0f, 0f), materials.RangeIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_Spark_North", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0.42f), new Vector3(0.13f, 0.13f, 0.13f), Quaternion.identity, materials.RangeSpark);
            CreatePrimitiveVisual(visualRoot, "Icon_Spark_South", PrimitiveType.Sphere, new Vector3(0f, 0.16f, -0.42f), new Vector3(0.13f, 0.13f, 0.13f), Quaternion.identity, materials.RangeSpark);
            CreatePrimitiveVisual(visualRoot, "Icon_Spark_East", PrimitiveType.Sphere, new Vector3(0.42f, 0.16f, 0f), new Vector3(0.13f, 0.13f, 0.13f), Quaternion.identity, materials.RangeSpark);
            CreatePrimitiveVisual(visualRoot, "Icon_Spark_West", PrimitiveType.Sphere, new Vector3(-0.42f, 0.16f, 0f), new Vector3(0.13f, 0.13f, 0.13f), Quaternion.identity, materials.RangeSpark);
            CreatePrimitiveVisual(visualRoot, "Glow_FloorRing", PrimitiveType.Cylinder, new Vector3(0f, -0.27f, 0f), new Vector3(0.82f, 0.025f, 0.82f), Quaternion.identity, materials.CommonGlow);

            ConfigureItemVisualAnimator(
                itemObject,
                visualRoot,
                new Color(1f, 0.58f, 0.1f),
                new Vector3(0f, 92f, 0f),
                0.07f,
                2.75f,
                0.055f,
                3.7f,
                1.25f);
            ConfigureItemPickupFeedback(
                itemObject,
                visualRoot,
                new Color(1f, 0.58f, 0.1f),
                0.34f,
                0.48f,
                240f);

            return SaveItemPrefab(itemObject, ExplosionRangePrefabPath);
        }

        private static ItemBase EnsureMoveSpeedItemPrefab(ItemVisualMaterials materials)
        {
            GameObject itemObject = CreateItemRoot("Item_MoveSpeedUp", ItemType.MoveSpeedUp, out Transform visualRoot);

            CreatePrimitiveVisual(visualRoot, "Token_LimeCapsule", PrimitiveType.Capsule, new Vector3(0f, 0.04f, 0f), new Vector3(0.48f, 0.48f, 0.48f), Quaternion.identity, materials.SpeedBody);
            CreatePrimitiveVisual(visualRoot, "Icon_Arrow_Stem", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.28f), new Vector3(0.11f, 0.075f, 0.34f), Quaternion.identity, materials.SpeedIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_Arrow_HeadLeft", PrimitiveType.Cube, new Vector3(-0.095f, 0.12f, 0.45f), new Vector3(0.1f, 0.075f, 0.22f), Quaternion.Euler(0f, -35f, 0f), materials.SpeedIcon);
            CreatePrimitiveVisual(visualRoot, "Icon_Arrow_HeadRight", PrimitiveType.Cube, new Vector3(0.095f, 0.12f, 0.45f), new Vector3(0.1f, 0.075f, 0.22f), Quaternion.Euler(0f, 35f, 0f), materials.SpeedIcon);
            CreatePrimitiveVisual(visualRoot, "SpeedWing_Left", PrimitiveType.Cube, new Vector3(-0.42f, 0.12f, -0.04f), new Vector3(0.22f, 0.055f, 0.12f), Quaternion.Euler(0f, 0f, -18f), materials.SpeedWing);
            CreatePrimitiveVisual(visualRoot, "SpeedWing_Right", PrimitiveType.Cube, new Vector3(0.42f, 0.12f, -0.04f), new Vector3(0.22f, 0.055f, 0.12f), Quaternion.Euler(0f, 0f, 18f), materials.SpeedWing);
            CreatePrimitiveVisual(visualRoot, "Glow_FloorRing", PrimitiveType.Cylinder, new Vector3(0f, -0.27f, 0f), new Vector3(0.82f, 0.025f, 0.82f), Quaternion.identity, materials.CommonGlow);

            ConfigureItemVisualAnimator(
                itemObject,
                visualRoot,
                new Color(0.45f, 1f, 0.3f),
                new Vector3(0f, 126f, 0f),
                0.09f,
                3f,
                0.06f,
                4.1f,
                1.2f);
            ConfigureItemPickupFeedback(
                itemObject,
                visualRoot,
                new Color(0.45f, 1f, 0.3f),
                0.3f,
                0.52f,
                280f);

            return SaveItemPrefab(itemObject, MoveSpeedPrefabPath);
        }

        private static GameObject CreateItemRoot(string itemName, ItemType itemType, out Transform visualRoot)
        {
            GameObject itemObject = new GameObject(itemName);

            SphereCollider collider = itemObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.55f;
            collider.center = new Vector3(0f, 0.02f, 0f);

            Rigidbody rigidbody = itemObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            visualRoot = CreateVisualRoot(itemObject.transform);
            itemObject.AddComponent<ItemPickupFeedback>();
            ItemBase itemBase = itemObject.AddComponent<ItemBase>();
            ConfigureItem(itemBase, itemType);
            return itemObject;
        }

        private static Transform CreateVisualRoot(Transform parent)
        {
            GameObject visualRootObject = new GameObject("VisualRoot");
            visualRootObject.transform.SetParent(parent, false);
            visualRootObject.transform.localPosition = Vector3.zero;
            visualRootObject.transform.localRotation = Quaternion.identity;
            visualRootObject.transform.localScale = Vector3.one;
            return visualRootObject.transform;
        }

        private static GameObject CreatePrimitiveVisual(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = objectName;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = localScale;

            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return visual;
        }

        private static void ConfigureItemVisualAnimator(
            GameObject itemObject,
            Transform visualRoot,
            Color pulseColor,
            Vector3 rotationDegreesPerSecond,
            float floatAmplitude,
            float floatSpeed,
            float scalePulseAmount,
            float scalePulseSpeed,
            float maxEmissionIntensity)
        {
            ItemVisualAnimator animator = itemObject.GetComponent<ItemVisualAnimator>();
            if (animator == null)
            {
                animator = itemObject.AddComponent<ItemVisualAnimator>();
            }

            Renderer[] renderers = itemObject.GetComponentsInChildren<Renderer>();
            SerializedObject serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serializedAnimator.FindProperty("enableFloat").boolValue = true;
            serializedAnimator.FindProperty("floatAmplitude").floatValue = floatAmplitude;
            serializedAnimator.FindProperty("floatSpeed").floatValue = floatSpeed;
            serializedAnimator.FindProperty("rotationDegreesPerSecond").vector3Value = rotationDegreesPerSecond;
            serializedAnimator.FindProperty("scalePulseAmount").floatValue = scalePulseAmount;
            serializedAnimator.FindProperty("scalePulseSpeed").floatValue = scalePulseSpeed;
            serializedAnimator.FindProperty("pulseEmissionColor").colorValue = pulseColor;
            serializedAnimator.FindProperty("maxEmissionIntensity").floatValue = maxEmissionIntensity;

            SerializedProperty pulseRenderers = serializedAnimator.FindProperty("pulseRenderers");
            pulseRenderers.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                pulseRenderers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }

            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(animator);
        }

        private static void ConfigureItemPickupFeedback(
            GameObject itemObject,
            Transform visualRoot,
            Color pulseColor,
            float pickupDuration,
            float riseHeight,
            float spinDegrees)
        {
            ItemPickupFeedback pickupFeedback = itemObject.GetComponent<ItemPickupFeedback>();
            if (pickupFeedback == null)
            {
                pickupFeedback = itemObject.AddComponent<ItemPickupFeedback>();
            }

            Renderer[] renderers = itemObject.GetComponentsInChildren<Renderer>();
            SerializedObject serializedFeedback = new SerializedObject(pickupFeedback);
            serializedFeedback.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serializedFeedback.FindProperty("pickupDuration").floatValue = pickupDuration;
            serializedFeedback.FindProperty("riseHeight").floatValue = riseHeight;
            serializedFeedback.FindProperty("popScaleAmount").floatValue = 0.28f;
            serializedFeedback.FindProperty("spinDegrees").floatValue = spinDegrees;
            serializedFeedback.FindProperty("disableCollidersOnPickup").boolValue = true;
            serializedFeedback.FindProperty("pickupEmissionColor").colorValue = pulseColor;
            serializedFeedback.FindProperty("maxEmissionIntensity").floatValue = 1.8f;
            serializedFeedback.FindProperty("pickupVolume").floatValue = 0.85f;
            serializedFeedback.FindProperty("playClipAtWorldPosition").boolValue = true;

            SerializedProperty pulseRenderers = serializedFeedback.FindProperty("pulseRenderers");
            pulseRenderers.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                pulseRenderers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }

            serializedFeedback.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pickupFeedback);
        }

        private static ItemBase SaveItemPrefab(GameObject itemObject, string prefabPath)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemObject, prefabPath);
            Object.DestroyImmediate(itemObject);
            return prefab != null ? prefab.GetComponent<ItemBase>() : null;
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
            serializedItem.FindProperty("pickupFeedback").objectReferenceValue = itemBase.GetComponent<ItemPickupFeedback>();
            serializedItem.FindProperty("notifyPickupFeedback").boolValue = true;
            serializedItem.FindProperty("notifyCharacterFeedback").boolValue = true;
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
