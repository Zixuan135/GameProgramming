using BubbleTown.Characters;
using BubbleTown.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Keeps the Battle scene's local multiplayer test objects aligned with the current script setup.
    /// </summary>
    public static class BattleScenePlayerSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private static readonly Vector2Int Player1Grid = new Vector2Int(1, 1);

        [MenuItem("BubbleTown/Setup/Ensure Battle Player2")]
        public static void EnsureBattlePlayer2()
        {
            SceneSetupResult setup = OpenBattleScene();
            if (!setup.IsValid)
            {
                Debug.LogError("[BattleScenePlayerSetup] Battle scene setup failed.");
                return;
            }

            ConfigurePlayer(setup.Player1Controller, setup.MapManager, Player1Grid, false,
                KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.None);

            PlayerController player2Controller = EnsurePlayer2(setup.CharactersRoot, setup.Player1Controller.gameObject);
            Vector2Int player2Grid = ResolvePlayer2Grid(setup.MapManager);
            ConfigurePlayer(player2Controller, setup.MapManager, player2Grid, true,
                KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Return, KeyCode.RightControl);

            EditorSceneManager.MarkSceneDirty(setup.Scene);
            EditorSceneManager.SaveScene(setup.Scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[BattleScenePlayerSetup] Battle scene Player2 is ready.");
        }

        public static void EnsureBattlePlayer2FromBatchmode()
        {
            EnsureBattlePlayer2();
        }

        private static SceneSetupResult OpenBattleScene()
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            Transform charactersRoot = GameObject.Find("CharactersRoot")?.transform;
            MapManager mapManager = Object.FindObjectOfType<MapManager>();
            PlayerController player1Controller = GameObject.Find("Player1")?.GetComponent<PlayerController>();

            return new SceneSetupResult(scene, charactersRoot, mapManager, player1Controller);
        }

        private static PlayerController EnsurePlayer2(Transform charactersRoot, GameObject player1Object)
        {
            GameObject player2Object = GameObject.Find("Player2");
            if (player2Object == null)
            {
                player2Object = Object.Instantiate(player1Object, charactersRoot);
                player2Object.name = "Player2";
            }
            else if (player2Object.transform.parent != charactersRoot)
            {
                player2Object.transform.SetParent(charactersRoot);
            }

            return player2Object.GetComponent<PlayerController>();
        }

        private static Vector2Int ResolvePlayer2Grid(MapManager mapManager)
        {
            if (mapManager == null)
            {
                return new Vector2Int(11, 9);
            }

            return new Vector2Int(
                Mathf.Max(1, mapManager.MapWidth - 2),
                Mathf.Max(1, mapManager.MapHeight - 2));
        }

        private static void ConfigurePlayer(
            PlayerController controller,
            MapManager mapManager,
            Vector2Int gridPosition,
            bool localVsOnly,
            KeyCode upKey,
            KeyCode downKey,
            KeyCode leftKey,
            KeyCode rightKey,
            KeyCode primaryBombKey,
            KeyCode secondaryBombKey)
        {
            if (controller == null)
            {
                return;
            }

            float y = Mathf.Approximately(controller.transform.position.y, 0f)
                ? 0.5f
                : controller.transform.position.y;

            Vector3 worldPosition = new Vector3(gridPosition.x, y, gridPosition.y);
            if (mapManager != null)
            {
                worldPosition = mapManager.GridToWorld(gridPosition, y);
            }

            controller.transform.position = worldPosition;
            controller.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("currentGridPosition").vector2IntValue = gridPosition;
            serializedObject.FindProperty("currentWorldPosition").vector3Value = worldPosition;
            serializedObject.FindProperty("isMoving").boolValue = false;
            serializedObject.FindProperty("localVsOnly").boolValue = localVsOnly;
            serializedObject.FindProperty("moveUpKey").intValue = (int)upKey;
            serializedObject.FindProperty("moveDownKey").intValue = (int)downKey;
            serializedObject.FindProperty("moveLeftKey").intValue = (int)leftKey;
            serializedObject.FindProperty("moveRightKey").intValue = (int)rightKey;
            serializedObject.FindProperty("primaryBombKey").intValue = (int)primaryBombKey;
            serializedObject.FindProperty("secondaryBombKey").intValue = (int)secondaryBombKey;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(controller.gameObject);
            EditorUtility.SetDirty(controller);
        }

        private readonly struct SceneSetupResult
        {
            public SceneSetupResult(
                UnityEngine.SceneManagement.Scene scene,
                Transform charactersRoot,
                MapManager mapManager,
                PlayerController player1Controller)
            {
                Scene = scene;
                CharactersRoot = charactersRoot;
                MapManager = mapManager;
                Player1Controller = player1Controller;
            }

            public UnityEngine.SceneManagement.Scene Scene { get; }
            public Transform CharactersRoot { get; }
            public MapManager MapManager { get; }
            public PlayerController Player1Controller { get; }

            public bool IsValid => CharactersRoot != null && Player1Controller != null;
        }
    }
}
