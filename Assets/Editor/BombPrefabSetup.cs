using BubbleTown.Characters;
using BubbleTown.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Creates the placeholder bomb prefab and wires it into the Battle scene.
    /// </summary>
    public static class BombPrefabSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string BombPrefabPath = "Assets/Prefabs/Gameplay/Bombs/Bomb_Basic.prefab";
        private const string ExplosionPrefabPath = "Assets/Prefabs/Gameplay/Explosions/Explosion_Center.prefab";
        private const string ExplosionHorizontalPrefabPath = "Assets/Prefabs/Gameplay/Explosions/Explosion_Horizontal.prefab";
        private const string ExplosionVerticalPrefabPath = "Assets/Prefabs/Gameplay/Explosions/Explosion_Vertical.prefab";

        private const string BombBodyMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Body_BubbleNavy.mat";
        private const string BombHighlightMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Highlight_Cyan.mat";
        private const string BombCapMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_TopCap_Cream.mat";
        private const string BombFuseMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Fuse_Cocoa.mat";
        private const string BombSparkMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Spark_Yellow.mat";
        private const string BombOutlineMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Outline_Ink.mat";
        private const string BombStickerMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Sticker_Cream.mat";
        private const string BombBaseMaterialPath = "Assets/Materials/Gameplay/Bombs/Mat_Bomb_Base_Shadow.mat";

        [MenuItem("BubbleTown/Setup/Ensure Bomb Prefab")]
        public static void EnsureBombPrefab()
        {
            Material bodyMaterial = EnsureMaterial(
                BombBodyMaterialPath,
                "Mat_Bomb_Body_BubbleNavy",
                new Color(0.06f, 0.16f, 0.40f),
                new Color(0.00f, 0.025f, 0.07f),
                0.24f);
            Material highlightMaterial = EnsureMaterial(
                BombHighlightMaterialPath,
                "Mat_Bomb_Highlight_Cyan",
                new Color(0.58f, 0.92f, 1f),
                new Color(0.02f, 0.16f, 0.20f),
                0.18f);
            Material capMaterial = EnsureMaterial(
                BombCapMaterialPath,
                "Mat_Bomb_TopCap_Cream",
                new Color(1f, 0.88f, 0.58f),
                new Color(0.08f, 0.04f, 0.00f),
                0.18f);
            Material fuseMaterial = EnsureMaterial(
                BombFuseMaterialPath,
                "Mat_Bomb_Fuse_Cocoa",
                new Color(0.38f, 0.22f, 0.13f),
                Color.black,
                0.12f);
            Material sparkMaterial = EnsureMaterial(
                BombSparkMaterialPath,
                "Mat_Bomb_Spark_Yellow",
                new Color(1f, 0.73f, 0.16f),
                new Color(0.65f, 0.30f, 0.00f),
                0.18f);
            Material outlineMaterial = EnsureMaterial(
                BombOutlineMaterialPath,
                "Mat_Bomb_Outline_Ink",
                new Color(0.025f, 0.045f, 0.13f),
                Color.black,
                0.10f);
            Material stickerMaterial = EnsureMaterial(
                BombStickerMaterialPath,
                "Mat_Bomb_Sticker_Cream",
                new Color(1f, 0.96f, 0.82f),
                new Color(0.04f, 0.025f, 0.00f),
                0.14f);
            Material baseMaterial = EnsureMaterial(
                BombBaseMaterialPath,
                "Mat_Bomb_Base_Shadow",
                new Color(0.06f, 0.22f, 0.34f),
                Color.black,
                0.12f);

            BombController bombPrefab = EnsureBubbleStyleBombPrefab(
                bodyMaterial,
                highlightMaterial,
                capMaterial,
                fuseMaterial,
                sparkMaterial,
                outlineMaterial,
                stickerMaterial,
                baseMaterial);
            ConfigureBattleCharacters(bombPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[BombPrefabSetup] Bubble-style Bomb prefab is ready.");
        }

        public static void EnsureBombPrefabFromBatchmode()
        {
            EnsureBombPrefab();
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

        private static BombController EnsureBubbleStyleBombPrefab(
            Material bodyMaterial,
            Material highlightMaterial,
            Material capMaterial,
            Material fuseMaterial,
            Material sparkMaterial,
            Material outlineMaterial,
            Material stickerMaterial,
            Material baseMaterial)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BombPrefabPath);
            BombController existingController = existingPrefab != null ? existingPrefab.GetComponent<BombController>() : null;
            float fuseSeconds = ReadFloat(existingController, "fuseSeconds", 2f);
            ExplosionController explosionPrefab = ReadExplosionPrefab(existingController);

            GameObject bombObject = new GameObject("Bomb");
            SphereCollider collider = bombObject.AddComponent<SphereCollider>();
            collider.radius = 0.42f;
            collider.center = new Vector3(0f, 0.28f, 0f);

            Transform visualRoot = CreateVisualRoot(bombObject.transform);
            CreatePrimitiveVisual(
                visualRoot,
                "Ground_CandyShadow",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.035f, 0f),
                new Vector3(0.43f, 0.020f, 0.43f),
                Quaternion.identity,
                baseMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Ground_CreamBase",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.065f, 0f),
                new Vector3(0.34f, 0.025f, 0.34f),
                Quaternion.identity,
                capMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_OuterInkShell",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.32f, 0f),
                new Vector3(0.84f, 0.74f, 0.84f),
                Quaternion.identity,
                outlineMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_BubbleSphere",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.34f, 0f),
                new Vector3(0.76f, 0.66f, 0.76f),
                Quaternion.identity,
                bodyMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_MainHighlight",
                PrimitiveType.Sphere,
                new Vector3(-0.20f, 0.50f, 0.31f),
                new Vector3(0.17f, 0.095f, 0.055f),
                Quaternion.Euler(-20f, 0f, -26f),
                highlightMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_SideGlint",
                PrimitiveType.Sphere,
                new Vector3(0.20f, 0.40f, 0.34f),
                new Vector3(0.075f, 0.045f, 0.030f),
                Quaternion.Euler(-8f, 0f, 18f),
                highlightMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_CreamSticker",
                PrimitiveType.Cube,
                new Vector3(-0.02f, 0.24f, 0.39f),
                new Vector3(0.24f, 0.030f, 0.050f),
                Quaternion.Euler(0f, 0f, -7f),
                stickerMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "TopCap_OutlineRing",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.72f, 0f),
                new Vector3(0.255f, 0.055f, 0.255f),
                Quaternion.identity,
                outlineMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "TopCap_CreamButton",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.75f, 0f),
                new Vector3(0.215f, 0.055f, 0.215f),
                Quaternion.identity,
                capMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "TopCap_CyanInset",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.79f, 0f),
                new Vector3(0.120f, 0.018f, 0.120f),
                Quaternion.identity,
                highlightMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Fuse_BaseCollar",
                PrimitiveType.Cylinder,
                new Vector3(0.04f, 0.83f, 0.04f),
                new Vector3(0.080f, 0.035f, 0.080f),
                Quaternion.Euler(0f, 0f, -8f),
                outlineMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Fuse_CocoaStemA",
                PrimitiveType.Cylinder,
                new Vector3(0.08f, 0.90f, 0.10f),
                new Vector3(0.035f, 0.135f, 0.035f),
                Quaternion.Euler(42f, 0f, -26f),
                fuseMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Fuse_CocoaStemB",
                PrimitiveType.Cylinder,
                new Vector3(0.20f, 1.00f, 0.15f),
                new Vector3(0.030f, 0.115f, 0.030f),
                Quaternion.Euler(60f, 0f, -55f),
                fuseMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Fuse_CreamTip",
                PrimitiveType.Sphere,
                new Vector3(0.29f, 1.04f, 0.16f),
                new Vector3(0.070f, 0.052f, 0.052f),
                Quaternion.identity,
                stickerMaterial);
            CreateSparkStar(visualRoot, "Fuse_Spark", new Vector3(0.36f, 1.08f, 0.17f), sparkMaterial, stickerMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Warning_YellowButton",
                PrimitiveType.Sphere,
                new Vector3(0.28f, 0.31f, 0.34f),
                new Vector3(0.085f, 0.070f, 0.035f),
                Quaternion.identity,
                sparkMaterial);

            BombController controller = bombObject.AddComponent<BombController>();
            ConfigureBombController(controller, visualRoot, fuseSeconds, explosionPrefab);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bombObject, BombPrefabPath);
            Object.DestroyImmediate(bombObject);
            return prefab != null ? prefab.GetComponent<BombController>() : controller;
        }

        private static void CreateSparkStar(
            Transform parent,
            string prefix,
            Vector3 localPosition,
            Material sparkMaterial,
            Material coreMaterial)
        {
            CreatePrimitiveVisual(parent, prefix + "_Glow", PrimitiveType.Sphere, localPosition, new Vector3(0.15f, 0.15f, 0.15f), Quaternion.identity, sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_Core", PrimitiveType.Sphere, localPosition + new Vector3(0.010f, 0.010f, 0.012f), new Vector3(0.060f, 0.060f, 0.060f), Quaternion.identity, coreMaterial);
            CreatePrimitiveVisual(parent, prefix + "_Vertical", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, 0.025f), new Vector3(0.050f, 0.23f, 0.030f), Quaternion.identity, sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_Horizontal", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, 0.030f), new Vector3(0.23f, 0.050f, 0.030f), Quaternion.identity, sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_SlashA", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, 0.035f), new Vector3(0.17f, 0.035f, 0.030f), Quaternion.Euler(0f, 0f, 45f), sparkMaterial);
            CreatePrimitiveVisual(parent, prefix + "_SlashB", PrimitiveType.Cube, localPosition + new Vector3(0f, 0f, 0.040f), new Vector3(0.17f, 0.035f, 0.030f), Quaternion.Euler(0f, 0f, -45f), sparkMaterial);
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

        private static void ConfigureBombController(
            BombController controller,
            Transform visualRoot,
            float fuseSeconds,
            ExplosionController explosionPrefab)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("fuseSeconds").floatValue = fuseSeconds;
            serializedController.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            SetOptionalObjectReference(serializedController, "explosionCenterPrefab", explosionPrefab);
            SetOptionalObjectReference(serializedController, "explosionHorizontalPrefab", LoadExplosionPrefab(ExplosionHorizontalPrefabPath));
            SetOptionalObjectReference(serializedController, "explosionVerticalPrefab", LoadExplosionPrefab(ExplosionVerticalPrefabPath));
            serializedController.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serializedController.FindProperty("flashEmissionColor").colorValue = new Color(1f, 0.58f, 0.12f);
            serializedController.FindProperty("slowFlashInterval").floatValue = 0.55f;
            serializedController.FindProperty("fastFlashInterval").floatValue = 0.08f;
            serializedController.FindProperty("flashOnRatio").floatValue = 0.42f;
            serializedController.FindProperty("flashScalePulse").floatValue = 0.1f;

            Renderer[] renderers = controller.GetComponentsInChildren<Renderer>();
            SerializedProperty flashRenderers = serializedController.FindProperty("flashRenderers");
            flashRenderers.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                flashRenderers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static float ReadFloat(BombController controller, string propertyName, float fallback)
        {
            if (controller == null)
            {
                return fallback;
            }

            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty property = serializedController.FindProperty(propertyName);
            return property != null ? property.floatValue : fallback;
        }

        private static ExplosionController ReadExplosionPrefab(BombController controller)
        {
            ExplosionController explosionPrefab = null;
            if (controller != null)
            {
                SerializedObject serializedController = new SerializedObject(controller);
                SerializedProperty explosionProperty = serializedController.FindProperty("explosionPrefab");
                explosionPrefab = explosionProperty != null
                    ? explosionProperty.objectReferenceValue as ExplosionController
                    : null;
            }

            if (explosionPrefab != null)
            {
                return explosionPrefab;
            }

            GameObject explosionObject = AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionPrefabPath);
            return explosionObject != null ? explosionObject.GetComponent<ExplosionController>() : null;
        }

        private static ExplosionController LoadExplosionPrefab(string prefabPath)
        {
            GameObject explosionObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            return explosionObject != null ? explosionObject.GetComponent<ExplosionController>() : null;
        }

        private static void ConfigureBattleCharacters(BombController bombPrefab)
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            Transform bombSpawnRoot = ResolveBombSpawnRoot();
            CharacterBase[] characters = Object.FindObjectsOfType<CharacterBase>(true);
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterBase character = characters[i];
                if (character == null)
                {
                    continue;
                }

                ConfigureCharacterBombPrefab(character, bombPrefab, bombSpawnRoot);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform ResolveBombSpawnRoot()
        {
            GameObject existingRoot = GameObject.Find("BombsRoot");
            if (existingRoot != null)
            {
                return existingRoot.transform;
            }

            GameObject root = new GameObject("BombsRoot");
            return root.transform;
        }

        private static void ConfigureCharacterBombPrefab(CharacterBase character, BombController bombPrefab, Transform bombSpawnRoot)
        {
            SerializedObject serializedObject = new SerializedObject(character);
            serializedObject.FindProperty("bombPrefab").objectReferenceValue = bombPrefab;
            serializedObject.FindProperty("bombSpawnRoot").objectReferenceValue = bombSpawnRoot;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(character);
        }

        private static void SetOptionalObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
