using BubbleTown.Core;
using BubbleTown.Gameplay;
using UnityEditor;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Creates the low-cost chibi explosion prefabs and wires them into the bomb prefab.
    /// </summary>
    public static class ExplosionPrefabSetup
    {
        private const string BombPrefabPath = "Assets/Prefabs/Gameplay/Bombs/Bomb_Basic.prefab";
        private const string ExplosionCenterPrefabPath = "Assets/Prefabs/Gameplay/Explosions/Explosion_Center.prefab";
        private const string ExplosionHorizontalPrefabPath = "Assets/Prefabs/Gameplay/Explosions/Explosion_Horizontal.prefab";
        private const string ExplosionVerticalPrefabPath = "Assets/Prefabs/Gameplay/Explosions/Explosion_Vertical.prefab";

        private const string CoreMaterialPath = "Assets/Materials/Gameplay/Explosions/Mat_Explosion_Core_Cream.mat";
        private const string BubbleMaterialPath = "Assets/Materials/Gameplay/Explosions/Mat_Explosion_Bubble_Cyan.mat";
        private const string ArmMaterialPath = "Assets/Materials/Gameplay/Explosions/Mat_Explosion_Arm_Orange.mat";
        private const string SparkMaterialPath = "Assets/Materials/Gameplay/Explosions/Mat_Explosion_Spark_Pink.mat";

        private enum ExplosionVisualKind
        {
            Center,
            Horizontal,
            Vertical
        }

        [MenuItem("BubbleTown/Setup/Ensure Bubble Explosion Prefabs")]
        public static void EnsureBubbleExplosionPrefabs()
        {
            Material coreMaterial = EnsureMaterial(
                CoreMaterialPath,
                "Mat_Explosion_Core_Cream",
                new Color(1f, 0.84f, 0.42f),
                new Color(1f, 0.55f, 0.08f),
                0.25f);
            Material bubbleMaterial = EnsureMaterial(
                BubbleMaterialPath,
                "Mat_Explosion_Bubble_Cyan",
                new Color(0.28f, 0.92f, 1f),
                new Color(0.04f, 0.65f, 0.9f),
                0.45f);
            Material armMaterial = EnsureMaterial(
                ArmMaterialPath,
                "Mat_Explosion_Arm_Orange",
                new Color(1f, 0.48f, 0.16f),
                new Color(1f, 0.26f, 0.04f),
                0.3f);
            Material sparkMaterial = EnsureMaterial(
                SparkMaterialPath,
                "Mat_Explosion_Spark_Pink",
                new Color(1f, 0.36f, 0.62f),
                new Color(1f, 0.12f, 0.32f),
                0.2f);

            ExplosionController centerPrefab = EnsureExplosionPrefab(
                ExplosionCenterPrefabPath,
                "ExplosionCenter",
                ExplosionVisualKind.Center,
                coreMaterial,
                bubbleMaterial,
                armMaterial,
                sparkMaterial);
            ExplosionController horizontalPrefab = EnsureExplosionPrefab(
                ExplosionHorizontalPrefabPath,
                "ExplosionHorizontal",
                ExplosionVisualKind.Horizontal,
                coreMaterial,
                bubbleMaterial,
                armMaterial,
                sparkMaterial);
            ExplosionController verticalPrefab = EnsureExplosionPrefab(
                ExplosionVerticalPrefabPath,
                "ExplosionVertical",
                ExplosionVisualKind.Vertical,
                coreMaterial,
                bubbleMaterial,
                armMaterial,
                sparkMaterial);

            ConfigureBombPrefab(centerPrefab, horizontalPrefab, verticalPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[ExplosionPrefabSetup] Bubble explosion prefabs are ready.");
        }

        public static void EnsureExplosionCenterPrefab()
        {
            EnsureBubbleExplosionPrefabs();
        }

        public static void EnsureExplosionCenterPrefabFromBatchmode()
        {
            EnsureBubbleExplosionPrefabs();
        }

        public static void EnsureBubbleExplosionPrefabsFromBatchmode()
        {
            EnsureBubbleExplosionPrefabs();
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

        private static ExplosionController EnsureExplosionPrefab(
            string prefabPath,
            string prefabName,
            ExplosionVisualKind visualKind,
            Material coreMaterial,
            Material bubbleMaterial,
            Material armMaterial,
            Material sparkMaterial)
        {
            GameObject root = new GameObject(prefabName);
            ConfigureTriggerCollider(root, visualKind);

            Rigidbody rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            Transform visualRoot = CreateVisualRoot(root.transform);
            BuildExplosionVisuals(visualRoot, visualKind, coreMaterial, bubbleMaterial, armMaterial, sparkMaterial);

            ExplosionController controller = root.AddComponent<ExplosionController>();
            ConfigureExplosionController(controller, visualRoot, visualKind);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab != null ? prefab.GetComponent<ExplosionController>() : controller;
        }

        private static void ConfigureTriggerCollider(GameObject root, ExplosionVisualKind visualKind)
        {
            if (visualKind == ExplosionVisualKind.Center)
            {
                SphereCollider collider = root.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.56f;
                collider.center = Vector3.zero;
                return;
            }

            BoxCollider boxCollider = root.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.center = Vector3.zero;
            boxCollider.size = visualKind == ExplosionVisualKind.Horizontal
                ? new Vector3(1.08f, 0.72f, 0.5f)
                : new Vector3(0.5f, 0.72f, 1.08f);
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

        private static void BuildExplosionVisuals(
            Transform visualRoot,
            ExplosionVisualKind visualKind,
            Material coreMaterial,
            Material bubbleMaterial,
            Material armMaterial,
            Material sparkMaterial)
        {
            switch (visualKind)
            {
                case ExplosionVisualKind.Center:
                    CreatePrimitiveVisual(visualRoot, "Center_CoreBubble", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.72f, 0.72f, 0.72f), Quaternion.identity, coreMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_CyanPop_North", PrimitiveType.Sphere, new Vector3(0f, 0f, 0.38f), new Vector3(0.26f, 0.26f, 0.26f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_CyanPop_South", PrimitiveType.Sphere, new Vector3(0f, 0f, -0.38f), new Vector3(0.26f, 0.26f, 0.26f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_PinkSpark_East", PrimitiveType.Sphere, new Vector3(0.38f, 0.08f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Quaternion.identity, sparkMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_PinkSpark_West", PrimitiveType.Sphere, new Vector3(-0.38f, 0.08f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Quaternion.identity, sparkMaterial);
                    break;
                case ExplosionVisualKind.Horizontal:
                    CreatePrimitiveVisual(visualRoot, "Horizontal_OrangeSplash", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.19f, 0.5f, 0.19f), Quaternion.Euler(0f, 0f, 90f), armMaterial);
                    CreatePrimitiveVisual(visualRoot, "Horizontal_Bubble_Left", PrimitiveType.Sphere, new Vector3(-0.5f, 0f, 0f), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Horizontal_Bubble_Right", PrimitiveType.Sphere, new Vector3(0.5f, 0f, 0f), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Horizontal_Spark_Top", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Quaternion.identity, sparkMaterial);
                    break;
                case ExplosionVisualKind.Vertical:
                    CreatePrimitiveVisual(visualRoot, "Vertical_OrangeSplash", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.19f, 0.5f, 0.19f), Quaternion.Euler(90f, 0f, 0f), armMaterial);
                    CreatePrimitiveVisual(visualRoot, "Vertical_Bubble_North", PrimitiveType.Sphere, new Vector3(0f, 0f, 0.5f), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Vertical_Bubble_South", PrimitiveType.Sphere, new Vector3(0f, 0f, -0.5f), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Vertical_Spark_Top", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Quaternion.identity, sparkMaterial);
                    break;
            }
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

        private static void ConfigureExplosionController(
            ExplosionController controller,
            Transform visualRoot,
            ExplosionVisualKind visualKind)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("lifeSeconds").floatValue = GameConstants.DefaultExplosionDuration;
            serializedController.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serializedController.FindProperty("startScale").vector3Value = new Vector3(0.25f, 0.25f, 0.25f);
            serializedController.FindProperty("peakScale").vector3Value = visualKind == ExplosionVisualKind.Center
                ? new Vector3(1.08f, 1.08f, 1.08f)
                : new Vector3(1f, 1f, 1f);
            serializedController.FindProperty("pulseEmissionColor").colorValue = visualKind == ExplosionVisualKind.Center
                ? new Color(1f, 0.72f, 0.18f)
                : new Color(0.45f, 0.95f, 1f);
            serializedController.FindProperty("maxEmissionIntensity").floatValue = visualKind == ExplosionVisualKind.Center ? 1.6f : 1.25f;
            serializedController.FindProperty("rotationAxis").vector3Value = Vector3.up;
            serializedController.FindProperty("rotationDegreesPerSecond").floatValue = visualKind == ExplosionVisualKind.Center ? 120f : 30f;

            Renderer[] renderers = controller.GetComponentsInChildren<Renderer>();
            SerializedProperty pulseRenderers = serializedController.FindProperty("pulseRenderers");
            pulseRenderers.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                pulseRenderers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureBombPrefab(
            ExplosionController centerPrefab,
            ExplosionController horizontalPrefab,
            ExplosionController verticalPrefab)
        {
            GameObject bombPrefabContents = PrefabUtility.LoadPrefabContents(BombPrefabPath);
            BombController bombController = bombPrefabContents.GetComponent<BombController>();
            if (bombController == null)
            {
                PrefabUtility.UnloadPrefabContents(bombPrefabContents);
                return;
            }

            SerializedObject serializedBomb = new SerializedObject(bombController);
            SetObjectReference(serializedBomb, "explosionPrefab", centerPrefab);
            SetObjectReference(serializedBomb, "explosionCenterPrefab", centerPrefab);
            SetObjectReference(serializedBomb, "explosionHorizontalPrefab", horizontalPrefab);
            SetObjectReference(serializedBomb, "explosionVerticalPrefab", verticalPrefab);
            serializedBomb.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(bombPrefabContents, BombPrefabPath);
            PrefabUtility.UnloadPrefabContents(bombPrefabContents);
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
