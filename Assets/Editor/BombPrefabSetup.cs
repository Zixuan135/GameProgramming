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
        private const string BombPrefabPath = "Assets/Prefabs/Gameplay/Bomb.prefab";
        private const string ExplosionPrefabPath = "Assets/Prefabs/Gameplay/ExplosionCenter.prefab";
        private const string ExplosionHorizontalPrefabPath = "Assets/Prefabs/Gameplay/ExplosionHorizontal.prefab";
        private const string ExplosionVerticalPrefabPath = "Assets/Prefabs/Gameplay/ExplosionVertical.prefab";

        private const string BombBodyMaterialPath = "Assets/Materials/Mat_Bomb_Body_BubbleNavy.mat";
        private const string BombHighlightMaterialPath = "Assets/Materials/Mat_Bomb_Highlight_Cyan.mat";
        private const string BombCapMaterialPath = "Assets/Materials/Mat_Bomb_TopCap_Cream.mat";
        private const string BombFuseMaterialPath = "Assets/Materials/Mat_Bomb_Fuse_Cocoa.mat";
        private const string BombSparkMaterialPath = "Assets/Materials/Mat_Bomb_Spark_Yellow.mat";

        [MenuItem("BubbleTown/Setup/Ensure Bomb Prefab")]
        public static void EnsureBombPrefab()
        {
            Material bodyMaterial = EnsureMaterial(
                BombBodyMaterialPath,
                "Mat_Bomb_Body_BubbleNavy",
                new Color(0.08f, 0.12f, 0.32f),
                new Color(0.02f, 0.08f, 0.24f),
                0.55f);
            Material highlightMaterial = EnsureMaterial(
                BombHighlightMaterialPath,
                "Mat_Bomb_Highlight_Cyan",
                new Color(0.42f, 0.9f, 1f),
                new Color(0.1f, 0.45f, 0.65f),
                0.4f);
            Material capMaterial = EnsureMaterial(
                BombCapMaterialPath,
                "Mat_Bomb_TopCap_Cream",
                new Color(1f, 0.88f, 0.48f),
                new Color(0.3f, 0.18f, 0.04f),
                0.28f);
            Material fuseMaterial = EnsureMaterial(
                BombFuseMaterialPath,
                "Mat_Bomb_Fuse_Cocoa",
                new Color(0.35f, 0.2f, 0.12f),
                Color.black,
                0.18f);
            Material sparkMaterial = EnsureMaterial(
                BombSparkMaterialPath,
                "Mat_Bomb_Spark_Yellow",
                new Color(1f, 0.65f, 0.12f),
                new Color(1f, 0.45f, 0.02f),
                0.25f);

            BombController bombPrefab = EnsureBubbleStyleBombPrefab(
                bodyMaterial,
                highlightMaterial,
                capMaterial,
                fuseMaterial,
                sparkMaterial);
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
            Material sparkMaterial)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BombPrefabPath);
            BombController existingController = existingPrefab != null ? existingPrefab.GetComponent<BombController>() : null;
            float fuseSeconds = ReadFloat(existingController, "fuseSeconds", 2f);
            ExplosionController explosionPrefab = ReadExplosionPrefab(existingController);

            GameObject bombObject = new GameObject("Bomb");
            SphereCollider collider = bombObject.AddComponent<SphereCollider>();
            collider.radius = 0.43f;
            collider.center = Vector3.zero;

            Transform visualRoot = CreateVisualRoot(bombObject.transform);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_BubbleSphere",
                PrimitiveType.Sphere,
                Vector3.zero,
                new Vector3(0.78f, 0.78f, 0.78f),
                Quaternion.identity,
                bodyMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Body_CartoonHighlight",
                PrimitiveType.Sphere,
                new Vector3(-0.18f, 0.18f, 0.28f),
                new Vector3(0.18f, 0.12f, 0.08f),
                Quaternion.Euler(-20f, 0f, -20f),
                highlightMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "TopCap_CreamButton",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.44f, 0f),
                new Vector3(0.22f, 0.065f, 0.22f),
                Quaternion.identity,
                capMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Fuse_CurvedStem",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.6f, 0.08f),
                new Vector3(0.045f, 0.17f, 0.045f),
                Quaternion.Euler(35f, 0f, 0f),
                fuseMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Fuse_Spark",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.74f, 0.2f),
                new Vector3(0.16f, 0.16f, 0.16f),
                Quaternion.identity,
                sparkMaterial);

            BombController controller = bombObject.AddComponent<BombController>();
            ConfigureBombController(controller, visualRoot, fuseSeconds, explosionPrefab);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bombObject, BombPrefabPath);
            Object.DestroyImmediate(bombObject);
            return prefab != null ? prefab.GetComponent<BombController>() : controller;
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
