using BubbleTown.AI;
using BubbleTown.Characters;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Gameplay;
using BubbleTown.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown.Managers
{
    /// <summary>
    /// Stores runtime game session data and owns the first pass of battle-mode setup.
    /// It decides which character roles are active for SinglePlayer, AIBattle, and LocalVS.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private const string RuntimeObjectName = "GameManager";
        private const string Player1ObjectName = "Player1";
        private const string Player2ObjectName = "Player2";
        private const string AIObjectName = "AIPlayer";
        private const string CharactersRootName = "CharactersRoot";
        private const string BombsRootName = "BombsRoot";

        private static GameManager instance;
        private static bool isQuitting;

        public static GameManager Instance
        {
            get
            {
                if (instance == null && !isQuitting)
                {
                    instance = FindObjectOfType<GameManager>();
                    if (instance == null)
                    {
                        GameObject gameManagerObject = new GameObject(RuntimeObjectName);
                        instance = gameManagerObject.AddComponent<GameManager>();
                    }
                }

                return instance;
            }
            private set => instance = value;
        }

        [Header("Session")]
        [SerializeField] private GameMode currentGameMode = GameMode.SinglePlayer;
        [SerializeField] private BattleMapType currentMapType = BattleMapType.Default;
        [SerializeField] private GameState currentGameState = GameState.None;

        [Header("Last Battle Result")]
        [SerializeField] private bool hasBattleResult;
        [SerializeField] private string lastResultTitle = "No Result Yet";
        [SerializeField] private string lastResultDetail = "Start a battle to create a result.";
        [SerializeField] private string lastResultWinner = "None";

        [Header("Battle Setup")]
        [SerializeField] private bool autoSetupBattleOnSceneLoaded = true;
        [SerializeField] private bool createAIIfMissing = true;
        [SerializeField] private bool logBattleSetup = true;

        [Header("Runtime References")]
        [SerializeField] private MapManager activeMapManager;
        [SerializeField] private PlayerController player1;
        [SerializeField] private PlayerController player2;
        [SerializeField] private AIController aiPlayer;

        private bool hasBattleSetupSnapshot;
        private int lastBattleSetupSceneHandle;
        private GameMode lastBattleSetupMode;
        private BattleMapType lastBattleSetupMapType;

        public GameMode CurrentGameMode => currentGameMode;
        public BattleMapType CurrentMapType => currentMapType;
        public GameState CurrentGameState => currentGameState;
        public bool HasBattleResult => hasBattleResult;
        public string LastResultTitle => lastResultTitle;
        public string LastResultDetail => lastResultDetail;
        public string LastResultWinner => lastResultWinner;
        public MapManager ActiveMapManager => activeMapManager;
        public PlayerController Player1 => player1;
        public PlayerController Player2 => player2;
        public AIController AIPlayer => aiPlayer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (autoSetupBattleOnSceneLoaded && SceneManager.GetActiveScene().name == GameConstants.SceneBattle)
            {
                SetupBattleForCurrentMode();
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        public void SetGameMode(GameMode mode)
        {
            currentGameMode = mode;
        }

        public void SetMapType(BattleMapType mapType)
        {
            currentMapType = mapType;
        }

        public void SetGameState(GameState gameState)
        {
            currentGameState = gameState;
        }

        public void BeginBattle()
        {
            ClearBattleResult();
            currentGameState = GameState.BattlePreparing;

            if (SceneManager.GetActiveScene().name == GameConstants.SceneBattle)
            {
                SetupBattleForCurrentMode();
                return;
            }

            SceneManager.LoadScene(GameConstants.SceneBattle);
        }

        public void BeginBattle(GameMode mode)
        {
            SetGameMode(mode);
            BeginBattle();
        }

        public void ResetSessionData()
        {
            currentGameMode = GameMode.SinglePlayer;
            currentMapType = BattleMapType.Default;
            currentGameState = GameState.None;
            activeMapManager = null;
            player1 = null;
            player2 = null;
            aiPlayer = null;
            hasBattleSetupSnapshot = false;
            ClearBattleResult();
        }

        public void FinishBattle(string resultTitle, string resultDetail, string winnerName)
        {
            if (currentGameState == GameState.BattleFinished)
            {
                return;
            }

            lastResultTitle = string.IsNullOrEmpty(resultTitle) ? "Battle Finished" : resultTitle;
            lastResultDetail = string.IsNullOrEmpty(resultDetail) ? "The battle has ended." : resultDetail;
            lastResultWinner = string.IsNullOrEmpty(winnerName) ? "None" : winnerName;
            hasBattleResult = true;
            currentGameState = GameState.BattleFinished;

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle finished. Winner: {lastResultWinner}. Result: {lastResultTitle}");
            }
        }

        public void ClearBattleResult()
        {
            hasBattleResult = false;
            lastResultTitle = "No Result Yet";
            lastResultDetail = "Start a battle to create a result.";
            lastResultWinner = "None";
        }

        public void SetupBattleForCurrentMode()
        {
            if (IsCurrentBattleSetupAlreadyApplied())
            {
                return;
            }

            currentGameState = GameState.BattlePreparing;
            ClearBattleResult();
            ResolveBattleReferences();

            if (activeMapManager == null || player1 == null)
            {
                Debug.LogWarning("[GameManager] Battle setup skipped because MapManager or Player1 is missing.");
                return;
            }

            activeMapManager.SetMapType(currentMapType);
            activeMapManager.InitializeGridData();
            activeMapManager.GenerateMap();

            Transform bombSpawnRoot = ResolveBombSpawnRoot();
            BombController bombPrefab = player1.BombPrefab;

            SetupPlayer1(bombSpawnRoot, bombPrefab);
            SetupPlayer2(bombSpawnRoot, bombPrefab);
            SetupAIPlayer(bombSpawnRoot, bombPrefab);

            currentGameState = GameState.BattleRunning;
            StoreBattleSetupSnapshot();
            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle ready. Mode: {currentGameMode}, Map: {currentMapType}");
            }
        }

        private bool IsCurrentBattleSetupAlreadyApplied()
        {
            return currentGameState == GameState.BattleRunning &&
                   hasBattleSetupSnapshot &&
                   lastBattleSetupSceneHandle == SceneManager.GetActiveScene().handle &&
                   lastBattleSetupMode == currentGameMode &&
                   lastBattleSetupMapType == currentMapType;
        }

        private void StoreBattleSetupSnapshot()
        {
            hasBattleSetupSnapshot = true;
            lastBattleSetupSceneHandle = SceneManager.GetActiveScene().handle;
            lastBattleSetupMode = currentGameMode;
            lastBattleSetupMapType = currentMapType;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!autoSetupBattleOnSceneLoaded)
            {
                return;
            }

            if (scene.name == GameConstants.SceneBattle)
            {
                SetupBattleForCurrentMode();
                return;
            }

            UpdateGameStateForScene(scene.name);
        }

        private void UpdateGameStateForScene(string sceneName)
        {
            switch (sceneName)
            {
                case GameConstants.SceneMainMenu:
                    currentGameState = GameState.MainMenu;
                    break;
                case GameConstants.SceneModeSelect:
                    currentGameState = GameState.ModeSelect;
                    break;
                case GameConstants.SceneMapSelect:
                    currentGameState = GameState.MapSelect;
                    break;
                case GameConstants.SceneResult:
                    currentGameState = GameState.Result;
                    break;
            }
        }

        private void ResolveBattleReferences()
        {
            activeMapManager = FindObjectOfType<MapManager>();
            player1 = FindPlayerControllerByName(Player1ObjectName);
            player2 = FindPlayerControllerByName(Player2ObjectName);
            aiPlayer = FindAIControllerByName(AIObjectName);
        }

        private PlayerController FindPlayerControllerByName(string objectName)
        {
            PlayerController[] controllers = FindObjectsOfType<PlayerController>(true);
            for (int i = 0; i < controllers.Length; i++)
            {
                PlayerController controller = controllers[i];
                if (controller != null && controller.gameObject.name == objectName)
                {
                    return controller;
                }
            }

            return null;
        }

        private AIController FindAIControllerByName(string objectName)
        {
            AIController[] controllers = FindObjectsOfType<AIController>(true);
            for (int i = 0; i < controllers.Length; i++)
            {
                AIController controller = controllers[i];
                if (controller != null && controller.gameObject.name == objectName)
                {
                    return controller;
                }
            }

            return null;
        }

        private Transform ResolveCharactersRoot()
        {
            Transform root = GameObject.Find(CharactersRootName)?.transform;
            if (root != null)
            {
                return root;
            }

            GameObject rootObject = new GameObject(CharactersRootName);
            return rootObject.transform;
        }

        private Transform ResolveBombSpawnRoot()
        {
            Transform root = GameObject.Find(BombsRootName)?.transform;
            if (root != null)
            {
                return root;
            }

            GameObject rootObject = new GameObject(BombsRootName);
            return rootObject.transform;
        }

        private void SetupPlayer1(Transform bombSpawnRoot, BombController bombPrefab)
        {
            player1.gameObject.SetActive(true);
            player1.ConfigureForBattle(
                activeMapManager,
                activeMapManager.GetPlayer1SpawnGrid(),
                bombSpawnRoot,
                bombPrefab);
        }

        private void SetupPlayer2(Transform bombSpawnRoot, BombController bombPrefab)
        {
            if (player2 == null)
            {
                return;
            }

            bool shouldUsePlayer2 = currentGameMode == GameMode.LocalVS;
            player2.gameObject.SetActive(shouldUsePlayer2);
            if (!shouldUsePlayer2)
            {
                return;
            }

            player2.ConfigureForBattle(
                activeMapManager,
                activeMapManager.GetPlayer2SpawnGrid(),
                bombSpawnRoot,
                bombPrefab);
        }

        private void SetupAIPlayer(Transform bombSpawnRoot, BombController bombPrefab)
        {
            bool shouldUseAI = currentGameMode == GameMode.AIBattle;
            if (shouldUseAI)
            {
                aiPlayer = EnsureAIPlayer();
            }

            if (aiPlayer == null)
            {
                return;
            }

            aiPlayer.gameObject.SetActive(shouldUseAI);
            if (!shouldUseAI)
            {
                return;
            }

            aiPlayer.ConfigureForBattle(
                activeMapManager,
                activeMapManager.GetAISpawnGrid(),
                bombSpawnRoot,
                bombPrefab);
        }

        private AIController EnsureAIPlayer()
        {
            if (aiPlayer != null || !createAIIfMissing)
            {
                return aiPlayer;
            }

            Transform charactersRoot = ResolveCharactersRoot();
            GameObject aiObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            aiObject.name = AIObjectName;
            aiObject.transform.SetParent(charactersRoot);
            aiObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            MeshRenderer renderer = aiObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material aiMaterial = new Material(Shader.Find("Standard"));
                aiMaterial.color = new Color(1f, 0.42f, 0.35f);
                renderer.sharedMaterial = aiMaterial;
            }

            aiPlayer = aiObject.AddComponent<AIController>();
            return aiPlayer;
        }
    }
}
