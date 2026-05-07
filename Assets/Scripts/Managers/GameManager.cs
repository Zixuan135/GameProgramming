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

        [Header("Local VS Match")]
        [SerializeField] private bool enableLocalVsBestOf3 = true;
        [SerializeField, Min(1)] private int localVsTargetScore = 2;
        [SerializeField, Min(1)] private int localVsRoundNumber = 1;
        [SerializeField, Min(0)] private int localVsPlayer1Score;
        [SerializeField, Min(0)] private int localVsPlayer2Score;
        [SerializeField] private string lastLocalVsRoundWinner = "None";
        [SerializeField] private bool localVsMatchInProgress;

        [Header("Single Player Objective")]
        [SerializeField] private bool enableSinglePlayerSoftWallObjective = true;
        [SerializeField, Min(1)] private int singlePlayerSoftWallTarget = GameConstants.DefaultSinglePlayerSoftWallTarget;
        [SerializeField, Min(0)] private int activeSinglePlayerSoftWallTarget;
        [SerializeField, Min(0)] private int singlePlayerSoftWallsCleared;
        [SerializeField] private Vector2Int singlePlayerGoalGrid = new Vector2Int(1, 1);
        [SerializeField, Min(0)] private int singlePlayerStartGoalDistance;
        [SerializeField, Min(0)] private int singlePlayerCurrentGoalDistance;
        [SerializeField] private bool singlePlayerObjectiveComplete;

        [Header("Character Selection")]
        [SerializeField] private string selectedPlayer1CharacterId = "bubble_ranger";
        [SerializeField] private string selectedPlayer2CharacterId = "candy_sprout";
        [SerializeField] private string selectedAICharacterId = "robo_pop";
        [SerializeField] private CharacterData selectedPlayer1Character;
        [SerializeField] private CharacterData selectedPlayer2Character;
        [SerializeField] private CharacterData selectedAICharacter;
        [SerializeField] private bool randomizeAICharacterOnBattleStart = true;

        [Header("Runtime References")]
        [SerializeField] private MapManager activeMapManager;
        [SerializeField] private PlayerController player1;
        [SerializeField] private PlayerController player2;
        [SerializeField] private AIController aiPlayer;

        private bool hasBattleSetupSnapshot;
        private int lastBattleSetupSceneHandle;
        private GameMode lastBattleSetupMode;
        private BattleMapType lastBattleSetupMapType;
        private MapManager subscribedSinglePlayerObjectiveMapManager;

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
        public bool EnableLocalVsBestOf3 => enableLocalVsBestOf3;
        public int LocalVsTargetScore => Mathf.Max(1, localVsTargetScore);
        public int LocalVsRoundNumber => Mathf.Max(1, localVsRoundNumber);
        public int LocalVsPlayer1Score => localVsPlayer1Score;
        public int LocalVsPlayer2Score => localVsPlayer2Score;
        public string LastLocalVsRoundWinner => lastLocalVsRoundWinner;
        public bool IsLocalVsMatchInProgress => localVsMatchInProgress;
        public string LocalVsScoreLabel => $"P1 {localVsPlayer1Score} - {localVsPlayer2Score} P2";
        public bool IsSinglePlayerObjectiveEnabled => currentGameMode == GameMode.SinglePlayer && enableSinglePlayerSoftWallObjective;
        public int SinglePlayerSoftWallTarget => Mathf.Max(0, activeSinglePlayerSoftWallTarget);
        public int SinglePlayerSoftWallsCleared => singlePlayerSoftWallsCleared;
        public Vector2Int SinglePlayerGoalGrid => singlePlayerGoalGrid;
        public int SinglePlayerGoalDistance => Mathf.Max(0, singlePlayerCurrentGoalDistance);
        public float SinglePlayerRouteProgress => singlePlayerStartGoalDistance > 0
            ? 1f - Mathf.Clamp01((float)singlePlayerCurrentGoalDistance / singlePlayerStartGoalDistance)
            : singlePlayerObjectiveComplete ? 1f : 0f;
        public bool IsSinglePlayerObjectiveComplete => IsSinglePlayerObjectiveEnabled && singlePlayerObjectiveComplete;
        public string SinglePlayerObjectiveLabel => "Reach Exit";
        public string SinglePlayerObjectiveProgressLabel => singlePlayerObjectiveComplete
            ? "EXIT REACHED"
            : $"{SinglePlayerGoalDistance} tiles";
        public CharacterData SelectedPlayer1Character
        {
            get
            {
                EnsureCharacterSelections();
                return selectedPlayer1Character;
            }
        }
        public CharacterData SelectedPlayer2Character
        {
            get
            {
                EnsureCharacterSelections();
                return selectedPlayer2Character;
            }
        }
        public CharacterData SelectedAICharacter
        {
            get
            {
                EnsureCharacterSelections();
                return selectedAICharacter;
            }
        }

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
            UnsubscribeSinglePlayerObjectiveMap();
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        public void SetGameMode(GameMode mode)
        {
            ResetSinglePlayerObjective();
            ResetLocalVsMatch();
            currentGameMode = mode;
        }

        public void SetMapType(BattleMapType mapType)
        {
            currentMapType = mapType;
        }

        public void SetPlayer1Character(CharacterData characterData)
        {
            selectedPlayer1Character = ResolveCharacterOrDefault(characterData, 0);
            selectedPlayer1CharacterId = selectedPlayer1Character != null ? selectedPlayer1Character.CharacterId : string.Empty;
            EnsureDifferentLocalVsCharacters();
        }

        public void SetPlayer2Character(CharacterData characterData)
        {
            selectedPlayer2Character = ResolveCharacterOrDefault(characterData, 1);
            selectedPlayer2CharacterId = selectedPlayer2Character != null ? selectedPlayer2Character.CharacterId : string.Empty;
            EnsureDifferentLocalVsCharacters();
        }

        public void SetAICharacter(CharacterData characterData)
        {
            selectedAICharacter = ResolveCharacterOrDefault(characterData, 2);
            selectedAICharacterId = selectedAICharacter != null ? selectedAICharacter.CharacterId : string.Empty;
        }

        public void RandomizeAICharacter()
        {
            EnsureCharacterSelections();
            CharacterData randomCharacter = CharacterRoster.GetRandomDifferent(selectedPlayer1Character);
            SetAICharacter(randomCharacter);
        }

        public void SetGameState(GameState gameState)
        {
            currentGameState = gameState;
        }

        public void BeginBattle()
        {
            ClearBattleResult();
            currentGameState = GameState.BattlePreparing;
            if (currentGameMode == GameMode.LocalVS)
            {
                EnsureLocalVsMatchStarted();
            }
            else
            {
                ResetLocalVsMatch();
            }

            if (SceneManager.GetActiveScene().name == GameConstants.SceneBattle)
            {
                hasBattleSetupSnapshot = false;
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
            ResetSinglePlayerObjective();
            ResetLocalVsMatch();
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

        public void StartBattleRound(float spawnProtectionSeconds)
        {
            if (currentGameState == GameState.BattleFinished)
            {
                return;
            }

            ApplySpawnProtectionToActiveCharacters(spawnProtectionSeconds);
            currentGameState = GameState.BattleRunning;

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle round started. Opening protection: {Mathf.Max(0f, spawnProtectionSeconds):0.00}s");
            }
        }

        public void ClearBattleResult()
        {
            hasBattleResult = false;
            lastResultTitle = "No Result Yet";
            lastResultDetail = "Start a battle to create a result.";
            lastResultWinner = "None";
        }

        public void ResetLocalVsMatch()
        {
            localVsPlayer1Score = 0;
            localVsPlayer2Score = 0;
            localVsRoundNumber = 1;
            lastLocalVsRoundWinner = "None";
            localVsMatchInProgress = false;
        }

        public void EnsureLocalVsMatchStarted()
        {
            if (currentGameMode != GameMode.LocalVS || localVsMatchInProgress)
            {
                return;
            }

            localVsPlayer1Score = 0;
            localVsPlayer2Score = 0;
            localVsRoundNumber = 1;
            lastLocalVsRoundWinner = "None";
            localVsMatchInProgress = true;
        }

        public bool RegisterLocalVsRoundResult(string roundWinner)
        {
            EnsureLocalVsMatchStarted();

            lastLocalVsRoundWinner = string.IsNullOrEmpty(roundWinner) ? "None" : roundWinner;
            if (lastLocalVsRoundWinner == "Player1")
            {
                localVsPlayer1Score++;
            }
            else if (lastLocalVsRoundWinner == "Player2")
            {
                localVsPlayer2Score++;
            }

            bool matchComplete = IsLocalVsMatchComplete();
            if (matchComplete)
            {
                localVsMatchInProgress = false;
            }
            else
            {
                localVsRoundNumber++;
            }

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Local VS round result. Winner: {lastLocalVsRoundWinner}, Score: {LocalVsScoreLabel}, Round: {localVsRoundNumber}, MatchComplete: {matchComplete}");
            }

            return matchComplete;
        }

        public bool IsLocalVsMatchComplete()
        {
            if (!enableLocalVsBestOf3)
            {
                return lastLocalVsRoundWinner == "Player1" || lastLocalVsRoundWinner == "Player2";
            }

            int targetScore = LocalVsTargetScore;
            return localVsPlayer1Score >= targetScore || localVsPlayer2Score >= targetScore;
        }

        public string ResolveLocalVsMatchWinner()
        {
            if (localVsPlayer1Score > localVsPlayer2Score)
            {
                return "Player1";
            }

            if (localVsPlayer2Score > localVsPlayer1Score)
            {
                return "Player2";
            }

            return "None";
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
            ConfigureSinglePlayerObjective();
            EnsureCharacterSelectionsForBattle();

            Transform bombSpawnRoot = ResolveBombSpawnRoot();
            BombController bombPrefab = player1.BombPrefab;

            SetupPlayer1(bombSpawnRoot, bombPrefab);
            SetupPlayer2(bombSpawnRoot, bombPrefab);
            SetupAIPlayer(bombSpawnRoot, bombPrefab);

            currentGameState = GameState.BattlePreparing;
            StoreBattleSetupSnapshot();
            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle prepared. Mode: {currentGameMode}, Map: {currentMapType}");
            }
        }

        private void ConfigureSinglePlayerObjective()
        {
            ResetSinglePlayerObjective();

            if (!IsSinglePlayerObjectiveEnabled || activeMapManager == null)
            {
                return;
            }

            int availableSoftWalls = activeMapManager.CountSoftWalls();
            activeSinglePlayerSoftWallTarget = availableSoftWalls > 0
                ? Mathf.Clamp(singlePlayerSoftWallTarget, 1, availableSoftWalls)
                : 0;
            singlePlayerGoalGrid = activeMapManager.GetSinglePlayerGoalGrid();
            singlePlayerStartGoalDistance = CalculateGridDistance(activeMapManager.GetPlayer1SpawnGrid(), singlePlayerGoalGrid);
            singlePlayerCurrentGoalDistance = singlePlayerStartGoalDistance;

            subscribedSinglePlayerObjectiveMapManager = activeMapManager;
            subscribedSinglePlayerObjectiveMapManager.SoftWallDestroyed += HandleSinglePlayerSoftWallDestroyed;

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] SinglePlayer route objective prepared. Goal: {singlePlayerGoalGrid}, start distance: {singlePlayerStartGoalDistance}.");
            }
        }

        private void ResetSinglePlayerObjective()
        {
            UnsubscribeSinglePlayerObjectiveMap();
            activeSinglePlayerSoftWallTarget = 0;
            singlePlayerSoftWallsCleared = 0;
            singlePlayerGoalGrid = new Vector2Int(1, 1);
            singlePlayerStartGoalDistance = 0;
            singlePlayerCurrentGoalDistance = 0;
            singlePlayerObjectiveComplete = false;
        }

        private void HandleSinglePlayerSoftWallDestroyed(Vector2Int gridPosition)
        {
            if (!IsSinglePlayerObjectiveEnabled || singlePlayerObjectiveComplete || currentGameState == GameState.BattleFinished)
            {
                return;
            }

            singlePlayerSoftWallsCleared = Mathf.Min(SinglePlayerSoftWallTarget, singlePlayerSoftWallsCleared + 1);
            RefreshSinglePlayerRouteObjective();

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] SinglePlayer objective progress: {SinglePlayerObjectiveProgressLabel} at {gridPosition}");
            }
        }

        public void RefreshSinglePlayerRouteObjective()
        {
            if (!IsSinglePlayerObjectiveEnabled || singlePlayerObjectiveComplete || activeMapManager == null || player1 == null)
            {
                return;
            }

            singlePlayerCurrentGoalDistance = CalculateGridDistance(player1.CurrentGridPosition, singlePlayerGoalGrid);
            if (activeMapManager.IsSinglePlayerGoal(player1.CurrentGridPosition))
            {
                singlePlayerObjectiveComplete = true;
                singlePlayerCurrentGoalDistance = 0;
            }
        }

        private int CalculateGridDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        private void UnsubscribeSinglePlayerObjectiveMap()
        {
            if (subscribedSinglePlayerObjectiveMapManager == null)
            {
                return;
            }

            subscribedSinglePlayerObjectiveMapManager.SoftWallDestroyed -= HandleSinglePlayerSoftWallDestroyed;
            subscribedSinglePlayerObjectiveMapManager = null;
        }

        private bool IsCurrentBattleSetupAlreadyApplied()
        {
            bool isBattleSetupState = currentGameState == GameState.BattlePreparing ||
                                      currentGameState == GameState.BattleRunning;
            return isBattleSetupState &&
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
            if (sceneName != GameConstants.SceneBattle)
            {
                ResetSinglePlayerObjective();
            }

            switch (sceneName)
            {
                case GameConstants.SceneMainMenu:
                    currentGameState = GameState.MainMenu;
                    break;
                case GameConstants.SceneModeSelect:
                    currentGameState = GameState.ModeSelect;
                    break;
                case GameConstants.SceneCharacterSelect:
                    currentGameState = GameState.CharacterSelect;
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
            ApplyCharacterSelection(player1, selectedPlayer1Character, Player1ObjectName);
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

            ApplyCharacterSelection(player2, selectedPlayer2Character, Player2ObjectName);
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

            ApplyCharacterSelection(aiPlayer, selectedAICharacter, AIObjectName);
            aiPlayer.ConfigureForBattle(
                activeMapManager,
                activeMapManager.GetAISpawnGrid(),
                bombSpawnRoot,
                bombPrefab);
        }

        private void EnsureCharacterSelectionsForBattle()
        {
            EnsureCharacterSelections();
            if (currentGameMode == GameMode.AIBattle && randomizeAICharacterOnBattleStart)
            {
                RandomizeAICharacter();
            }
        }

        private void EnsureCharacterSelections()
        {
            selectedPlayer1Character = ResolveCharacterByIdOrDefault(
                selectedPlayer1Character,
                selectedPlayer1CharacterId,
                0);
            selectedPlayer1CharacterId = selectedPlayer1Character != null ? selectedPlayer1Character.CharacterId : string.Empty;

            selectedPlayer2Character = ResolveCharacterByIdOrDefault(
                selectedPlayer2Character,
                selectedPlayer2CharacterId,
                1);
            selectedPlayer2CharacterId = selectedPlayer2Character != null ? selectedPlayer2Character.CharacterId : string.Empty;

            selectedAICharacter = ResolveCharacterByIdOrDefault(
                selectedAICharacter,
                selectedAICharacterId,
                2);
            selectedAICharacterId = selectedAICharacter != null ? selectedAICharacter.CharacterId : string.Empty;

            EnsureDifferentLocalVsCharacters();
        }

        private CharacterData ResolveCharacterOrDefault(CharacterData characterData, int preferredIndex)
        {
            return characterData != null ? characterData : CharacterRoster.GetDefaultCharacter(preferredIndex);
        }

        private CharacterData ResolveCharacterByIdOrDefault(
            CharacterData currentCharacter,
            string characterId,
            int preferredIndex)
        {
            if (currentCharacter != null)
            {
                return currentCharacter;
            }

            CharacterData characterFromId = CharacterRoster.FindById(characterId);
            return characterFromId != null ? characterFromId : CharacterRoster.GetDefaultCharacter(preferredIndex);
        }

        private void EnsureDifferentLocalVsCharacters()
        {
            if (currentGameMode != GameMode.LocalVS ||
                selectedPlayer1Character == null ||
                selectedPlayer2Character == null ||
                selectedPlayer1Character != selectedPlayer2Character)
            {
                return;
            }

            selectedPlayer2Character = CharacterRoster.GetNextDifferent(selectedPlayer1Character);
            selectedPlayer2CharacterId = selectedPlayer2Character != null ? selectedPlayer2Character.CharacterId : string.Empty;
        }

        private void ApplyCharacterSelection(CharacterBase character, CharacterData characterData, string roleObjectName)
        {
            if (character == null || characterData == null)
            {
                return;
            }

            character.gameObject.name = roleObjectName;
            character.ReplaceVisual(characterData.Prefab);
            character.ApplyCharacterData(characterData);

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Applied character '{characterData.DisplayName}' to {roleObjectName}.");
            }
        }

        private void ApplySpawnProtectionToActiveCharacters(float protectionSeconds)
        {
            float resolvedProtectionSeconds = Mathf.Max(0f, protectionSeconds);
            ApplySpawnProtection(player1, resolvedProtectionSeconds);
            ApplySpawnProtection(player2, resolvedProtectionSeconds);
            ApplySpawnProtection(aiPlayer, resolvedProtectionSeconds);
        }

        private void ApplySpawnProtection(CharacterBase character, float protectionSeconds)
        {
            if (character == null || !character.gameObject.activeInHierarchy)
            {
                return;
            }

            character.SetInvincible(protectionSeconds);
        }

        private AIController EnsureAIPlayer()
        {
            if (aiPlayer != null || !createAIIfMissing)
            {
                return aiPlayer;
            }

            Transform charactersRoot = ResolveCharactersRoot();
            GameObject aiObject = new GameObject(AIObjectName);
            aiObject.name = AIObjectName;
            aiObject.transform.SetParent(charactersRoot);
            aiObject.transform.localScale = Vector3.one;

            BoxCollider collider = aiObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.72f, 1.1f, 0.72f);
            collider.center = new Vector3(0f, 0.05f, 0f);

            aiPlayer = aiObject.AddComponent<AIController>();
            return aiPlayer;
        }
    }
}
