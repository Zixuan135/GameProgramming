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
        private const string OutlineMaterialPath = "Assets/Materials/Gameplay/Explosions/Mat_Explosion_Outline_Ink.mat";
        private const string ShadowMaterialPath = "Assets/Materials/Gameplay/Explosions/Mat_Explosion_Shadow_Cyan.mat";

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
                new Color(1f, 0.90f, 0.58f),
                new Color(0.28f, 0.12f, 0.02f),
                0.16f);
            Material bubbleMaterial = EnsureMaterial(
                BubbleMaterialPath,
                "Mat_Explosion_Bubble_Cyan",
                new Color(0.45f, 0.93f, 1f),
                new Color(0.02f, 0.20f, 0.26f),
                0.18f);
            Material armMaterial = EnsureMaterial(
                ArmMaterialPath,
                "Mat_Explosion_Arm_Orange",
                new Color(1f, 0.53f, 0.18f),
                new Color(0.35f, 0.12f, 0.00f),
                0.16f);
            Material sparkMaterial = EnsureMaterial(
                SparkMaterialPath,
                "Mat_Explosion_Spark_Pink",
                new Color(1f, 0.47f, 0.72f),
                new Color(0.28f, 0.04f, 0.12f),
                0.14f);
            Material outlineMaterial = EnsureMaterial(
                OutlineMaterialPath,
                "Mat_Explosion_Outline_Ink",
                new Color(0.12f, 0.09f, 0.18f),
                Color.black,
                0.08f);
            Material shadowMaterial = EnsureMaterial(
                ShadowMaterialPath,
                "Mat_Explosion_Shadow_Cyan",
                new Color(0.05f, 0.34f, 0.45f),
                Color.black,
                0.10f);

            ExplosionController centerPrefab = EnsureExplosionPrefab(
                ExplosionCenterPrefabPath,
                "ExplosionCenter",
                ExplosionVisualKind.Center,
                coreMaterial,
                bubbleMaterial,
                armMaterial,
                sparkMaterial,
                outlineMaterial,
                shadowMaterial);
            ExplosionController horizontalPrefab = EnsureExplosionPrefab(
                ExplosionHorizontalPrefabPath,
                "ExplosionHorizontal",
                ExplosionVisualKind.Horizontal,
                coreMaterial,
                bubbleMaterial,
                armMaterial,
                sparkMaterial,
                outlineMaterial,
                shadowMaterial);
            ExplosionController verticalPrefab = EnsureExplosionPrefab(
                ExplosionVerticalPrefabPath,
                "ExplosionVertical",
                ExplosionVisualKind.Vertical,
                coreMaterial,
                bubbleMaterial,
                armMaterial,
                sparkMaterial,
                outlineMaterial,
                shadowMaterial);

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
            Material sparkMaterial,
            Material outlineMaterial,
            Material shadowMaterial)
        {
            GameObject root = new GameObject(prefabName);
            ConfigureTriggerCollider(root, visualKind);

            Rigidbody rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            Transform visualRoot = CreateVisualRoot(root.transform);
            BuildExplosionVisuals(visualRoot, visualKind, coreMaterial, bubbleMaterial, armMaterial, sparkMaterial, outlineMaterial, shadowMaterial);

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
            Material sparkMaterial,
            Material outlineMaterial,
            Material shadowMaterial)
        {
            switch (visualKind)
            {
                case ExplosionVisualKind.Center:
                    CreatePrimitiveVisual(visualRoot, "Center_CandyShadow", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0f), new Vector3(0.58f, 0.018f, 0.58f), Quaternion.identity, shadowMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_OutlineBubble", PrimitiveType.Sphere, new Vector3(0f, 0.24f, 0f), new Vector3(0.62f, 0.42f, 0.62f), Quaternion.identity, outlineMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_CoreCreamPop", PrimitiveType.Sphere, new Vector3(0f, 0.27f, 0f), new Vector3(0.53f, 0.36f, 0.53f), Quaternion.identity, coreMaterial);
                    CreateDirectionalArm(visualRoot, "Center_East", ExplosionVisualKind.Horizontal, new Vector3(0.34f, 0.18f, 0f), 0.42f, coreMaterial, armMaterial, bubbleMaterial, outlineMaterial, shadowMaterial);
                    CreateDirectionalArm(visualRoot, "Center_West", ExplosionVisualKind.Horizontal, new Vector3(-0.34f, 0.18f, 0f), 0.42f, coreMaterial, armMaterial, bubbleMaterial, outlineMaterial, shadowMaterial);
                    CreateDirectionalArm(visualRoot, "Center_North", ExplosionVisualKind.Vertical, new Vector3(0f, 0.18f, 0.34f), 0.42f, coreMaterial, armMaterial, bubbleMaterial, outlineMaterial, shadowMaterial);
                    CreateDirectionalArm(visualRoot, "Center_South", ExplosionVisualKind.Vertical, new Vector3(0f, 0.18f, -0.34f), 0.42f, coreMaterial, armMaterial, bubbleMaterial, outlineMaterial, shadowMaterial);
                    CreateSparkStar(visualRoot, "Center_PinkStar_NE", new Vector3(0.30f, 0.48f, 0.30f), 0.12f, sparkMaterial, coreMaterial);
                    CreateSparkStar(visualRoot, "Center_PinkStar_SW", new Vector3(-0.31f, 0.40f, -0.30f), 0.10f, sparkMaterial, coreMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_CyanBubble_North", PrimitiveType.Sphere, new Vector3(0f, 0.44f, 0.25f), new Vector3(0.15f, 0.12f, 0.15f), Quaternion.identity, bubbleMaterial);
                    CreatePrimitiveVisual(visualRoot, "Center_CyanBubble_West", PrimitiveType.Sphere, new Vector3(-0.26f, 0.37f, 0.03f), new Vector3(0.13f, 0.10f, 0.13f), Quaternion.identity, bubbleMaterial);
                    break;
                case ExplosionVisualKind.Horizontal:
                    CreateLineExplosionVisuals(visualRoot, ExplosionVisualKind.Horizontal, coreMaterial, bubbleMaterial, armMaterial, sparkMaterial, outlineMaterial, shadowMaterial);
                    break;
                case ExplosionVisualKind.Vertical:
                    CreateLineExplosionVisuals(visualRoot, ExplosionVisualKind.Vertical, coreMaterial, bubbleMaterial, armMaterial, sparkMaterial, outlineMaterial, shadowMaterial);
                    break;
            }
        }

        private static void CreateLineExplosionVisuals(
            Transform visualRoot,
            ExplosionVisualKind visualKind,
            Material coreMaterial,
            Material bubbleMaterial,
            Material armMaterial,
            Material sparkMaterial,
            Material outlineMaterial,
            Material shadowMaterial)
        {
            bool horizontal = visualKind == ExplosionVisualKind.Horizontal;
            string prefix = horizontal ? "Horizontal" : "Vertical";
            Quaternion lineRotation = horizontal ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(90f, 0f, 0f);
            Vector3 lineScale = new Vector3(0.18f, 0.58f, 0.18f);
            Vector3 shadowScale = horizontal ? new Vector3(0.58f, 0.016f, 0.22f) : new Vector3(0.22f, 0.016f, 0.58f);
            Vector3 endA = horizontal ? new Vector3(-0.48f, 0.20f, 0f) : new Vector3(0f, 0.20f, -0.48f);
            Vector3 endB = horizontal ? new Vector3(0.48f, 0.20f, 0f) : new Vector3(0f, 0.20f, 0.48f);
            Vector3 sideA = horizontal ? new Vector3(-0.18f, 0.40f, 0.16f) : new Vector3(0.16f, 0.40f, -0.18f);
            Vector3 sideB = horizontal ? new Vector3(0.18f, 0.34f, -0.16f) : new Vector3(-0.16f, 0.34f, 0.18f);

            CreatePrimitiveVisual(visualRoot, prefix + "_CandyShadow", PrimitiveType.Cylinder, new Vector3(0f, 0.030f, 0f), shadowScale, Quaternion.identity, shadowMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_InkRail", PrimitiveType.Cylinder, new Vector3(0f, 0.17f, 0f), new Vector3(lineScale.x * 1.18f, lineScale.y, lineScale.z * 1.18f), lineRotation, outlineMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_CreamRail", PrimitiveType.Cylinder, new Vector3(0f, 0.20f, 0f), lineScale, lineRotation, coreMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_OrangeCore", PrimitiveType.Cylinder, new Vector3(0f, 0.235f, 0f), new Vector3(0.095f, 0.55f, 0.095f), lineRotation, armMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_BubbleEndA_Outline", PrimitiveType.Sphere, endA + new Vector3(0f, -0.015f, 0f), new Vector3(0.32f, 0.25f, 0.32f), Quaternion.identity, outlineMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_BubbleEndB_Outline", PrimitiveType.Sphere, endB + new Vector3(0f, -0.015f, 0f), new Vector3(0.32f, 0.25f, 0.32f), Quaternion.identity, outlineMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_BubbleEndA", PrimitiveType.Sphere, endA + new Vector3(0f, 0.015f, 0f), new Vector3(0.27f, 0.21f, 0.27f), Quaternion.identity, bubbleMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_BubbleEndB", PrimitiveType.Sphere, endB + new Vector3(0f, 0.015f, 0f), new Vector3(0.27f, 0.21f, 0.27f), Quaternion.identity, bubbleMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_FoamDotA", PrimitiveType.Sphere, sideA, new Vector3(0.12f, 0.09f, 0.12f), Quaternion.identity, coreMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_FoamDotB", PrimitiveType.Sphere, sideB, new Vector3(0.10f, 0.08f, 0.10f), Quaternion.identity, coreMaterial);
            CreateSparkStar(visualRoot, prefix + "_PinkStar", horizontal ? new Vector3(0.08f, 0.48f, 0.18f) : new Vector3(0.18f, 0.48f, 0.08f), 0.095f, sparkMaterial, coreMaterial);
        }

        private static void CreateDirectionalArm(
            Transform visualRoot,
            string prefix,
            ExplosionVisualKind visualKind,
            Vector3 localPosition,
            float length,
            Material coreMaterial,
            Material armMaterial,
            Material bubbleMaterial,
            Material outlineMaterial,
            Material shadowMaterial)
        {
            bool horizontal = visualKind == ExplosionVisualKind.Horizontal;
            Quaternion lineRotation = horizontal ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(90f, 0f, 0f);
            Vector3 lineScale = new Vector3(0.10f, length, 0.10f);
            CreatePrimitiveVisual(visualRoot, prefix + "_Shadow", PrimitiveType.Cylinder, new Vector3(localPosition.x, 0.030f, localPosition.z), horizontal ? new Vector3(length, 0.014f, 0.13f) : new Vector3(0.13f, 0.014f, length), Quaternion.identity, shadowMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_InkRail", PrimitiveType.Cylinder, localPosition, new Vector3(lineScale.x * 1.2f, lineScale.y, lineScale.z * 1.2f), lineRotation, outlineMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_CreamRail", PrimitiveType.Cylinder, localPosition + Vector3.up * 0.020f, lineScale, lineRotation, coreMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_OrangePop", PrimitiveType.Sphere, localPosition + Vector3.up * 0.055f, new Vector3(0.17f, 0.13f, 0.17f), Quaternion.identity, armMaterial);
            CreatePrimitiveVisual(visualRoot, prefix + "_CyanBubble", PrimitiveType.Sphere, localPosition + Vector3.up * 0.135f, new Vector3(0.13f, 0.10f, 0.13f), Quaternion.identity, bubbleMaterial);
        }

        private static void CreateSparkStar(
            Transform parent,
            string prefix,
            Vector3 localPosition,
            float size,
            Material sparkMaterial,
            Material coreMaterial)
        {
            CreatePrimitiveVisual(parent, prefix + "_Core", PrimitiveType.Sphere, localPosition, new Vector3(size * 0.50f, size * 0.50f, size * 0.50f), Quaternion.identity, coreMaterial);
            CreatePrimitiveVisual(parent, prefix + "_Vertical", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, size * 0.05f), new Vector3(size * 0.34f, size * 1.55f, size * 0.24f), Quaternion.identity, sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_Horizontal", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, size * 0.07f), new Vector3(size * 1.55f, size * 0.34f, size * 0.24f), Quaternion.identity, sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_SlashA", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, size * 0.09f), new Vector3(size * 1.15f, size * 0.25f, size * 0.24f), Quaternion.Euler(0f, 0f, 45f), sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_SlashB", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, size * 0.11f), new Vector3(size * 1.15f, size * 0.25f, size * 0.24f), Quaternion.Euler(0f, 0f, -45f), sparkMaterial);
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
            serializedController.FindProperty("startScale").vector3Value = new Vector3(0.20f, 0.20f, 0.20f);
            serializedController.FindProperty("peakScale").vector3Value = visualKind == ExplosionVisualKind.Center
                ? new Vector3(1.10f, 1.10f, 1.10f)
                : new Vector3(1.02f, 1.02f, 1.02f);
            serializedController.FindProperty("pulseEmissionColor").colorValue = visualKind == ExplosionVisualKind.Center
                ? new Color(1f, 0.62f, 0.15f)
                : new Color(0.55f, 0.93f, 1f);
            serializedController.FindProperty("maxEmissionIntensity").floatValue = visualKind == ExplosionVisualKind.Center ? 1.05f : 0.90f;
            serializedController.FindProperty("rotationAxis").vector3Value = Vector3.up;
            serializedController.FindProperty("rotationDegreesPerSecond").floatValue = visualKind == ExplosionVisualKind.Center ? 80f : 18f;

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
