using BubbleTown.AI;
using BubbleTown.Characters;
using BubbleTown.Core;
using BubbleTown.Gameplay;
using BubbleTown.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Builds low-cost chibi character placeholder prefabs from Unity primitives and wires them into Battle.
    /// The character root still owns gameplay scripts, while CharacterVisual owns replaceable art.
    /// </summary>
    public static class CharacterArtSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string MaterialFolder = "Assets/Materials/Characters";
        private const string CharacterPrefabFolder = "Assets/Prefabs/Characters";

        private const string Player1PrefabPath = CharacterPrefabFolder + "/Character_Player1_Chibi.prefab";
        private const string Player2PrefabPath = CharacterPrefabFolder + "/Character_Player2_Chibi.prefab";
        private const string AIPrefabPath = CharacterPrefabFolder + "/Character_AI_Chibi.prefab";
        private const string BombPrefabPath = "Assets/Prefabs/Gameplay/Bombs/Bomb_Basic.prefab";

        private const string CharactersRootName = "CharactersRoot";
        private const string BombsRootName = "BombsRoot";
        private const string Player1Name = "Player1";
        private const string Player2Name = "Player2";
        private const string AIName = "AIPlayer";
        private const string CharacterVisualName = "CharacterVisual";

        private static readonly Vector3 CharacterVisualOffset = new Vector3(0f, -0.5f, 0f);

        private enum CharacterVariant
        {
            Player1,
            Player2,
            AI
        }

        [MenuItem("BubbleTown/Setup/Ensure Chibi Character Art")]
        public static void EnsureChibiCharacterArt()
        {
            EnsureFolders();

            Material skin = EnsureMaterial(
                MaterialFolder + "/Mat_Character_Skin_Peach.mat",
                "Mat_Character_Skin_Peach",
                new Color(1f, 0.78f, 0.58f),
                0.22f);
            Material face = EnsureMaterial(
                MaterialFolder + "/Mat_Character_Face_Dark.mat",
                "Mat_Character_Face_Dark",
                new Color(0.12f, 0.08f, 0.06f),
                0.08f);
            Material player1Body = EnsureMaterial(
                MaterialFolder + "/Mat_Character_Player1_Body.mat",
                "Mat_Character_Player1_Body",
                new Color(0.18f, 0.72f, 1f),
                0.34f);
            Material player1Accent = EnsureMaterial(
                MaterialFolder + "/Mat_Character_Player1_Accent.mat",
                "Mat_Character_Player1_Accent",
                new Color(0.96f, 0.98f, 0.42f),
                0.28f);
            Material player2Body = EnsureMaterial(
                MaterialFolder + "/Mat_Character_Player2_Body.mat",
                "Mat_Character_Player2_Body",
                new Color(1f, 0.45f, 0.3f),
                0.32f);
            Material player2Accent = EnsureMaterial(
                MaterialFolder + "/Mat_Character_Player2_Accent.mat",
                "Mat_Character_Player2_Accent",
                new Color(1f, 0.86f, 0.28f),
                0.3f);
            Material aiBody = EnsureMaterial(
                MaterialFolder + "/Mat_Character_AI_Body.mat",
                "Mat_Character_AI_Body",
                new Color(0.52f, 0.36f, 0.92f),
                0.38f);
            Material aiAccent = EnsureMaterial(
                MaterialFolder + "/Mat_Character_AI_Accent.mat",
                "Mat_Character_AI_Accent",
                new Color(1f, 0.28f, 0.42f),
                0.26f);

            GameObject player1Prefab = EnsureCharacterPrefab(
                Player1PrefabPath,
                "Character_Player1_Chibi",
                CharacterVariant.Player1,
                skin,
                face,
                player1Body,
                player1Accent);
            GameObject player2Prefab = EnsureCharacterPrefab(
                Player2PrefabPath,
                "Character_Player2_Chibi",
                CharacterVariant.Player2,
                skin,
                face,
                player2Body,
                player2Accent);
            GameObject aiPrefab = EnsureCharacterPrefab(
                AIPrefabPath,
                "Character_AI_Chibi",
                CharacterVariant.AI,
                aiBody,
                face,
                aiBody,
                aiAccent);

            ConfigureBattleScene(player1Prefab, player2Prefab, aiPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterArtSetup] Chibi character prefabs and Battle scene character visuals are ready.");
        }

        public static void EnsureChibiCharacterArtFromBatchmode()
        {
            EnsureChibiCharacterArt();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "Characters");
            EnsureFolder("Assets/Prefabs", "Characters");
        }

        private static void EnsureFolder(string parentFolder, string childFolder)
        {
            string fullPath = parentFolder + "/" + childFolder;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentFolder, childFolder);
            }
        }

        private static Material EnsureMaterial(string materialPath, string materialName, Color color, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                material.name = materialName;
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
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

        private static GameObject EnsureCharacterPrefab(
            string prefabPath,
            string prefabName,
            CharacterVariant variant,
            Material headMaterial,
            Material faceMaterial,
            Material bodyMaterial,
            Material accentMaterial)
        {
            GameObject root = new GameObject(prefabName);
            Transform visualRoot = CreateVisualRoot(root.transform);

            CreatePrimitiveVisual(
                visualRoot,
                "Body_RoundSuit",
                PrimitiveType.Capsule,
                new Vector3(0f, 0.38f, 0f),
                new Vector3(0.42f, 0.34f, 0.42f),
                bodyMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Head_BigRound",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.88f, 0f),
                new Vector3(0.58f, 0.58f, 0.58f),
                headMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Foot_Left",
                PrimitiveType.Sphere,
                new Vector3(-0.18f, 0.08f, 0.12f),
                new Vector3(0.22f, 0.12f, 0.28f),
                bodyMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Foot_Right",
                PrimitiveType.Sphere,
                new Vector3(0.18f, 0.08f, 0.12f),
                new Vector3(0.22f, 0.12f, 0.28f),
                bodyMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Eye_Left",
                PrimitiveType.Sphere,
                new Vector3(-0.12f, 0.94f, 0.275f),
                new Vector3(0.06f, 0.08f, 0.035f),
                faceMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "Eye_Right",
                PrimitiveType.Sphere,
                new Vector3(0.12f, 0.94f, 0.275f),
                new Vector3(0.06f, 0.08f, 0.035f),
                faceMaterial);
            CreatePrimitiveVisual(
                visualRoot,
                "FrontBadge_FacingMarker",
                PrimitiveType.Cube,
                new Vector3(0f, 0.43f, 0.24f),
                new Vector3(0.2f, 0.12f, 0.055f),
                accentMaterial);

            AddVariantDetails(visualRoot, variant, faceMaterial, bodyMaterial, accentMaterial);
            ConfigureVisualAnimator(root, visualRoot, variant, null);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddVariantDetails(
            Transform visualRoot,
            CharacterVariant variant,
            Material faceMaterial,
            Material bodyMaterial,
            Material accentMaterial)
        {
            switch (variant)
            {
                case CharacterVariant.Player1:
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Cap_TopBubble",
                        PrimitiveType.Sphere,
                        new Vector3(0f, 1.15f, 0.02f),
                        new Vector3(0.36f, 0.16f, 0.32f),
                        bodyMaterial);
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Cap_FrontDot",
                        PrimitiveType.Sphere,
                        new Vector3(0f, 1.06f, 0.26f),
                        new Vector3(0.18f, 0.09f, 0.08f),
                        accentMaterial);
                    break;
                case CharacterVariant.Player2:
                    CreatePrimitiveVisual(
                        visualRoot,
                        "HairPuff_Left",
                        PrimitiveType.Sphere,
                        new Vector3(-0.31f, 0.91f, 0f),
                        new Vector3(0.2f, 0.22f, 0.2f),
                        bodyMaterial);
                    CreatePrimitiveVisual(
                        visualRoot,
                        "HairPuff_Right",
                        PrimitiveType.Sphere,
                        new Vector3(0.31f, 0.91f, 0f),
                        new Vector3(0.2f, 0.22f, 0.2f),
                        bodyMaterial);
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Bow_Left",
                        PrimitiveType.Cube,
                        new Vector3(-0.08f, 1.17f, 0.08f),
                        new Vector3(0.18f, 0.12f, 0.08f),
                        accentMaterial);
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Bow_Right",
                        PrimitiveType.Cube,
                        new Vector3(0.08f, 1.17f, 0.08f),
                        new Vector3(0.18f, 0.12f, 0.08f),
                        accentMaterial);
                    break;
                case CharacterVariant.AI:
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Visor_RedFront",
                        PrimitiveType.Cube,
                        new Vector3(0f, 0.95f, 0.29f),
                        new Vector3(0.28f, 0.08f, 0.05f),
                        accentMaterial);
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Antenna_Stem",
                        PrimitiveType.Cube,
                        new Vector3(0f, 1.2f, 0f),
                        new Vector3(0.04f, 0.22f, 0.04f),
                        faceMaterial);
                    CreatePrimitiveVisual(
                        visualRoot,
                        "Antenna_Dot",
                        PrimitiveType.Sphere,
                        new Vector3(0f, 1.34f, 0f),
                        new Vector3(0.12f, 0.12f, 0.12f),
                        accentMaterial);
                    break;
            }
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
            Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = objectName;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = Quaternion.identity;
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

        private static void ConfigureBattleScene(GameObject player1Prefab, GameObject player2Prefab, GameObject aiPrefab)
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);

            Transform charactersRoot = ResolveRoot(CharactersRootName);
            Transform bombsRoot = ResolveRoot(BombsRootName);
            MapManager mapManager = Object.FindObjectOfType<MapManager>();
            MapGenerator mapGenerator = Object.FindObjectOfType<MapGenerator>();
            BombController bombPrefab = AssetDatabase.LoadAssetAtPath<BombController>(BombPrefabPath);

            int mapWidth = mapGenerator != null ? mapGenerator.MapWidth : GameConstants.DefaultMapWidth;
            int mapHeight = mapGenerator != null ? mapGenerator.MapHeight : GameConstants.DefaultMapHeight;
            Vector2Int player1Grid = new Vector2Int(1, 1);
            Vector2Int player2Grid = new Vector2Int(mapWidth - 2, mapHeight - 2);
            Vector2Int aiGrid = new Vector2Int(mapWidth - 2, mapHeight - 2);

            GameObject player1Object = GameObject.Find(Player1Name);
            GameObject player2Object = GameObject.Find(Player2Name);
            GameObject aiObject = ResolveAIObject(charactersRoot);

            if (player1Object != null)
            {
                ConfigureCharacterRoot(player1Object, player1Prefab, mapManager, player1Grid, bombsRoot, bombPrefab);
            }

            if (player2Object != null)
            {
                ConfigureCharacterRoot(player2Object, player2Prefab, mapManager, player2Grid, bombsRoot, bombPrefab);
            }

            ConfigureCharacterRoot(aiObject, aiPrefab, mapManager, aiGrid, bombsRoot, bombPrefab);
            aiObject.SetActive(false);

            EditorUtility.SetDirty(charactersRoot.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform ResolveRoot(string rootName)
        {
            GameObject existing = GameObject.Find(rootName);
            if (existing != null)
            {
                return existing.transform;
            }

            GameObject rootObject = new GameObject(rootName);
            return rootObject.transform;
        }

        private static GameObject ResolveAIObject(Transform charactersRoot)
        {
            GameObject aiObject = GameObject.Find(AIName);
            if (aiObject == null)
            {
                Transform existingChild = charactersRoot.Find(AIName);
                aiObject = existingChild != null ? existingChild.gameObject : null;
            }

            if (aiObject == null)
            {
                aiObject = new GameObject(AIName);
                aiObject.transform.SetParent(charactersRoot, false);
            }

            if (aiObject.GetComponent<AIController>() == null)
            {
                aiObject.AddComponent<AIController>();
            }

            return aiObject;
        }

        private static void ConfigureCharacterRoot(
            GameObject characterObject,
            GameObject visualPrefab,
            MapManager mapManager,
            Vector2Int gridPosition,
            Transform bombsRoot,
            BombController bombPrefab)
        {
            if (characterObject == null || visualPrefab == null)
            {
                return;
            }

            RemoveRootPrimitiveRenderer(characterObject);
            ConfigureGameplayCollider(characterObject);

            characterObject.transform.localScale = Vector3.one;
            characterObject.transform.position = mapManager != null
                ? mapManager.GridToWorld(gridPosition, 0.5f)
                : new Vector3(gridPosition.x, 0.5f, gridPosition.y);

            Transform oldVisual = characterObject.transform.Find(CharacterVisualName);
            if (oldVisual != null)
            {
                Object.DestroyImmediate(oldVisual.gameObject);
            }

            GameObject visualInstance = PrefabUtility.InstantiatePrefab(visualPrefab, characterObject.transform) as GameObject;
            if (visualInstance == null)
            {
                visualInstance = Object.Instantiate(visualPrefab, characterObject.transform);
            }

            visualInstance.name = CharacterVisualName;
            visualInstance.transform.localPosition = CharacterVisualOffset;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one;

            CharacterBase character = characterObject.GetComponent<CharacterBase>();
            ConfigureVisualAnimator(
                visualInstance,
                ResolveVisualRoot(visualInstance.transform),
                ResolveCharacterVariant(characterObject.name),
                character);

            if (character != null)
            {
                SerializedObject serializedCharacter = new SerializedObject(character);
                SetObjectReference(serializedCharacter, "mapManager", mapManager);
                SetVector2Int(serializedCharacter, "currentGridPosition", gridPosition);
                SetVector3(serializedCharacter, "currentWorldPosition", characterObject.transform.position);
                SetBool(serializedCharacter, "isMoving", false);
                SetBool(serializedCharacter, "isAlive", true);
                SetBool(serializedCharacter, "delayDeathPresentationForFeedback", true);
                SetFloat(serializedCharacter, "deathPresentationDelay", 0.55f);
                SetBool(serializedCharacter, "disableCollidersImmediatelyOnDeath", true);
                SetObjectReference(serializedCharacter, "bombSpawnRoot", bombsRoot);
                SetObjectReference(serializedCharacter, "bombPrefab", bombPrefab);
                SetBool(serializedCharacter, "faceMoveDirection", true);
                SetObjectReference(serializedCharacter, "visualRoot", visualInstance.transform);
                serializedCharacter.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(character);
            }

            EditorUtility.SetDirty(characterObject);
        }

        private static void ConfigureVisualAnimator(
            GameObject visualObject,
            Transform animatedRoot,
            CharacterVariant variant,
            CharacterBase character)
        {
            CharacterVisualAnimator animator = visualObject.GetComponent<CharacterVisualAnimator>();
            if (animator == null)
            {
                animator = visualObject.AddComponent<CharacterVisualAnimator>();
            }

            SerializedObject serializedAnimator = new SerializedObject(animator);
            SetObjectReference(serializedAnimator, "character", character);
            SetObjectReference(serializedAnimator, "animatedRoot", animatedRoot);
            SetFloat(serializedAnimator, "idleBobAmplitude", variant == CharacterVariant.AI ? 0.028f : 0.035f);
            SetFloat(serializedAnimator, "idleBobSpeed", variant == CharacterVariant.AI ? 4f : 3.2f);
            SetFloat(serializedAnimator, "moveBobAmplitude", variant == CharacterVariant.AI ? 0.055f : 0.065f);
            SetFloat(serializedAnimator, "moveBobSpeed", variant == CharacterVariant.AI ? 12f : 11f);
            SetFloat(serializedAnimator, "moveSwayDegrees", variant == CharacterVariant.AI ? 5f : 7f);
            SetFloat(serializedAnimator, "moveScalePulse", variant == CharacterVariant.AI ? 0.055f : 0.07f);
            SetFloat(serializedAnimator, "bombActionDuration", variant == CharacterVariant.AI ? 0.24f : 0.28f);
            SetFloat(serializedAnimator, "bombSquashAmount", variant == CharacterVariant.AI ? 0.16f : 0.22f);
            SetFloat(serializedAnimator, "bombHopHeight", variant == CharacterVariant.AI ? 0.09f : 0.13f);
            SetFloat(serializedAnimator, "bombTiltDegrees", variant == CharacterVariant.AI ? 9f : 13f);
            SetFloat(serializedAnimator, "bombShakeDegrees", variant == CharacterVariant.AI ? 3.5f : 5f);
            SetFloat(serializedAnimator, "hitFeedbackDuration", 0.22f);
            SetFloat(serializedAnimator, "hitShakeAmplitude", variant == CharacterVariant.AI ? 0.065f : 0.075f);
            SetFloat(serializedAnimator, "hitScalePunch", variant == CharacterVariant.AI ? 0.11f : 0.14f);
            SetColor(serializedAnimator, "hitFlashColor", new Color(1f, 0.35f, 0.18f));
            SetFloat(serializedAnimator, "defeatFeedbackDuration", 0.52f);
            SetFloat(serializedAnimator, "defeatRiseHeight", variant == CharacterVariant.AI ? 0.18f : 0.22f);
            SetFloat(serializedAnimator, "defeatShakeAmplitude", variant == CharacterVariant.AI ? 0.075f : 0.09f);
            SetFloat(serializedAnimator, "defeatSpinDegrees", variant == CharacterVariant.AI ? 140f : 180f);
            SetFloat(serializedAnimator, "defeatShrinkStart", 0.28f);
            SetColor(serializedAnimator, "defeatFlashColor", new Color(1f, 0.72f, 0.12f));
            SetBool(serializedAnimator, "spawnDefeatPuffs", true);
            SetInt(serializedAnimator, "defeatPuffCount", variant == CharacterVariant.AI ? 5 : 6);
            SetFloat(serializedAnimator, "defeatPuffDuration", 0.42f);
            SetFloat(serializedAnimator, "defeatPuffDistance", variant == CharacterVariant.AI ? 0.38f : 0.48f);
            SetFloat(serializedAnimator, "defeatPuffScale", variant == CharacterVariant.AI ? 0.1f : 0.12f);
            SetFloat(serializedAnimator, "maxEmissionIntensity", 1.45f);

            Renderer[] renderers = visualObject.GetComponentsInChildren<Renderer>();
            SerializedProperty flashRenderers = serializedAnimator.FindProperty("flashRenderers");
            if (flashRenderers != null)
            {
                flashRenderers.arraySize = renderers.Length;
                for (int i = 0; i < renderers.Length; i++)
                {
                    flashRenderers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
                }
            }

            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(animator);
        }

        private static Transform ResolveVisualRoot(Transform characterVisual)
        {
            Transform visualRoot = characterVisual.Find("VisualRoot");
            return visualRoot != null ? visualRoot : characterVisual;
        }

        private static CharacterVariant ResolveCharacterVariant(string characterObjectName)
        {
            switch (characterObjectName)
            {
                case Player2Name:
                    return CharacterVariant.Player2;
                case AIName:
                    return CharacterVariant.AI;
                default:
                    return CharacterVariant.Player1;
            }
        }

        private static void RemoveRootPrimitiveRenderer(GameObject characterObject)
        {
            MeshRenderer renderer = characterObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Object.DestroyImmediate(renderer);
            }

            MeshFilter meshFilter = characterObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Object.DestroyImmediate(meshFilter);
            }
        }

        private static void ConfigureGameplayCollider(GameObject characterObject)
        {
            BoxCollider collider = characterObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = characterObject.AddComponent<BoxCollider>();
            }

            collider.center = new Vector3(0f, 0.05f, 0f);
            collider.size = new Vector3(0.72f, 1.1f, 0.72f);
            collider.isTrigger = false;
            EditorUtility.SetDirty(collider);
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetVector2Int(SerializedObject serializedObject, string propertyName, Vector2Int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector2IntValue = value;
            }
        }

        private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.colorValue = value;
            }
        }
    }
}
