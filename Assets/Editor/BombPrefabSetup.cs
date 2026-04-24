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
        private const string BombMaterialPath = "Assets/Materials/BombPlaceholder.mat";

        [MenuItem("BubbleTown/Setup/Ensure Bomb Prefab")]
        public static void EnsureBombPrefab()
        {
            Material bombMaterial = EnsureBombMaterial();
            BombController bombPrefab = EnsurePlaceholderBombPrefab(bombMaterial);
            ConfigureBattlePlayers(bombPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[BombPrefabSetup] Placeholder Bomb prefab is ready.");
        }

        public static void EnsureBombPrefabFromBatchmode()
        {
            EnsureBombPrefab();
        }

        private static Material EnsureBombMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(BombMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Standard"));
            material.name = "BombPlaceholder";
            material.color = new Color(0.12f, 0.1f, 0.16f);
            AssetDatabase.CreateAsset(material, BombMaterialPath);
            return material;
        }

        private static BombController EnsurePlaceholderBombPrefab(Material bombMaterial)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BombPrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab.GetComponent<BombController>();
            }

            GameObject bombObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bombObject.name = "Bomb";
            bombObject.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

            MeshRenderer renderer = bombObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = bombMaterial;
            }

            BombController controller = bombObject.AddComponent<BombController>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bombObject, BombPrefabPath);
            Object.DestroyImmediate(bombObject);
            return prefab != null ? prefab.GetComponent<BombController>() : controller;
        }

        private static void ConfigureBattlePlayers(BombController bombPrefab)
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            Transform bombSpawnRoot = GameObject.Find("BombsRoot")?.transform;
            ConfigurePlayerBombPrefab("Player1", bombPrefab, bombSpawnRoot);
            ConfigurePlayerBombPrefab("Player2", bombPrefab, bombSpawnRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigurePlayerBombPrefab(string playerName, BombController bombPrefab, Transform bombSpawnRoot)
        {
            PlayerController player = GameObject.Find(playerName)?.GetComponent<PlayerController>();
            if (player == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(player);
            serializedObject.FindProperty("bombPrefab").objectReferenceValue = bombPrefab;
            serializedObject.FindProperty("bombSpawnRoot").objectReferenceValue = bombSpawnRoot;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(player);
        }
    }
}
