using BubbleTown.AI;
using BubbleTown.Characters;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Gameplay;
using BubbleTown.Items;
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
        /// <summary>
        /// Ordered milestones used by single-player tutorial mode to select the current HUD guidance.
        /// </summary>
        private enum TutorialStep
        {
            Move = 0,
            PlaceBomb = 1,
            RunAway = 2,
            BreakSoftWall = 3,
            PickUpItem = 4,
            ReachExit = 5,
            Complete = 6
        }

        private const string RuntimeObjectName = "GameManager";
        private const string Player1ObjectName = "Player1";
        private const string Player2ObjectName = "Player2";
        private const string AIObjectName = "AIPlayer";
        private const string CharactersRootName = "CharactersRoot";
        private const string BombsRootName = "BombsRoot";
        private const int TutorialPlayableStepCount = 6;

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
        [SerializeField] private AIDifficulty currentAIDifficulty = AIDifficulty.Normal;
        [SerializeField] private GameState currentGameState = GameState.None;
        [SerializeField] private bool isBattlePaused;

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

        [Header("Tutorial Mode")]
        [SerializeField] private ItemType tutorialGuaranteedItemType = ItemType.MoveSpeedUp;
        [SerializeField] private TutorialStep tutorialStep = TutorialStep.Move;
        [SerializeField] private Vector2Int tutorialMoveTargetGrid = new Vector2Int(2, 1);
        [SerializeField] private Vector2Int tutorialSoftWallGrid = new Vector2Int(3, 1);
        [SerializeField] private Vector2Int tutorialLastBombGrid = new Vector2Int(-1, -1);
        [SerializeField] private bool tutorialBombPlaced;
        [SerializeField] private bool tutorialSoftWallCleared;
        [SerializeField] private bool tutorialItemStepSkipped;

        [Header("Character Selection")]
        [SerializeField] private string selectedPlayer1CharacterId = "bubble_ranger";
        [SerializeField] private string selectedPlayer2CharacterId = "bear_blaster";
        [SerializeField] private string selectedAICharacterId = "frog_hopper";
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
        private AIDifficulty lastBattleSetupAIDifficulty;
        private MapManager subscribedSinglePlayerObjectiveMapManager;
        private CharacterBase subscribedTutorialPlayer;
        private bool tutorialItemPickupSubscribed;

        public GameMode CurrentGameMode => currentGameMode;
        public BattleMapType CurrentMapType => currentMapType;
        public AIDifficulty CurrentAIDifficulty => currentAIDifficulty;
        public GameState CurrentGameState => currentGameState;
        public bool IsBattlePaused => isBattlePaused;
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
        public bool IsTutorialMode => currentGameMode == GameMode.SinglePlayer;
        public ItemType TutorialGuaranteedItemType => tutorialGuaranteedItemType;
        public Vector2Int TutorialMoveTargetGrid => tutorialMoveTargetGrid;
        public Vector2Int TutorialSoftWallGrid => tutorialSoftWallGrid;
        public float SinglePlayerRouteProgress => IsTutorialMode
            ? ResolveTutorialProgress()
            : singlePlayerStartGoalDistance > 0
            ? 1f - Mathf.Clamp01((float)singlePlayerCurrentGoalDistance / singlePlayerStartGoalDistance)
            : singlePlayerObjectiveComplete ? 1f : 0f;
        public bool IsSinglePlayerObjectiveComplete => IsSinglePlayerObjectiveEnabled && singlePlayerObjectiveComplete;
        public string SinglePlayerObjectiveLabel => IsTutorialMode ? ResolveTutorialObjectiveLabel() : "Reach Exit";
        public string SinglePlayerObjectiveProgressLabel => IsTutorialMode
            ? ResolveTutorialProgressLabel()
            : singlePlayerObjectiveComplete
            ? "EXIT REACHED"
            : $"{SinglePlayerGoalDistance} tiles";
        public string SinglePlayerObjectiveHintLabel => IsTutorialMode
            ? ResolveTutorialHintLabel()
            : "Break soft walls, open the route, and reach the glowing exit.";
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

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            if (autoSetupBattleOnSceneLoaded && SceneManager.GetActiveScene().name == GameConstants.SceneBattle)
            {
                SetupBattleForCurrentMode();
            }
        }

        /// <summary>
        /// Purpose: Advances lightweight tutorial objectives while the battle runs.
        /// Inputs: no direct parameters; reads current player, map, and tutorial state.
        /// Output: no return value; moves the tutorial objective to the next step when conditions are met.
        /// </summary>
        private void Update()
        {
            TickTutorialObjective();
        }

        /// <summary>
        /// Purpose: Cleans up subscriptions or runtime state when this component becomes inactive.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeSinglePlayerObjectiveMap();
            UnsubscribeTutorialObjectiveEvents();
        }

        /// <summary>
        /// Purpose: Handles application shutdown cleanup.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// Purpose: Sets game mode.
        /// Inputs: `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mode">Input value used by this method.</param>
        public void SetGameMode(GameMode mode)
        {
            ResetSinglePlayerObjective();
            ResetLocalVsMatch();
            currentGameMode = mode;
        }

        /// <summary>
        /// Purpose: Sets map type.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        public void SetMapType(BattleMapType mapType)
        {
            currentMapType = mapType;
        }

        /// <summary>
        /// Purpose: Stores the AI difficulty selected before entering AI Battle.
        /// Inputs: difficulty is the chosen AI preset from MapSelect or another menu.
        /// Output: no return value; the selected difficulty is kept in the session and applied during AI setup.
        /// </summary>
        /// <param name="difficulty">AI difficulty preset to use for the next AI Battle.</param>
        public void SetAIDifficulty(AIDifficulty difficulty)
        {
            currentAIDifficulty = difficulty;
        }

        /// <summary>
        /// Purpose: Sets player1 character.
        /// Inputs: `characterData`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="characterData">Input value used by this method.</param>
        public void SetPlayer1Character(CharacterData characterData)
        {
            selectedPlayer1Character = ResolveCharacterOrDefault(characterData, 0);
            selectedPlayer1CharacterId = selectedPlayer1Character != null ? selectedPlayer1Character.CharacterId : string.Empty;
            EnsureDifferentLocalVsCharacters();
        }

        /// <summary>
        /// Purpose: Sets player2 character.
        /// Inputs: `characterData`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="characterData">Input value used by this method.</param>
        public void SetPlayer2Character(CharacterData characterData)
        {
            selectedPlayer2Character = ResolveCharacterOrDefault(characterData, 1);
            selectedPlayer2CharacterId = selectedPlayer2Character != null ? selectedPlayer2Character.CharacterId : string.Empty;
            EnsureDifferentLocalVsCharacters();
        }

        /// <summary>
        /// Purpose: Sets aicharacter.
        /// Inputs: `characterData`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="characterData">Input value used by this method.</param>
        public void SetAICharacter(CharacterData characterData)
        {
            selectedAICharacter = ResolveCharacterOrDefault(characterData, 2);
            selectedAICharacterId = selectedAICharacter != null ? selectedAICharacter.CharacterId : string.Empty;
        }

        /// <summary>
        /// Purpose: Performs randomize aicharacter for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void RandomizeAICharacter()
        {
            EnsureCharacterSelections();
            CharacterData randomCharacter = CharacterRoster.GetRandomDifferent(selectedPlayer1Character);
            SetAICharacter(randomCharacter);
        }

        /// <summary>
        /// Purpose: Sets game state.
        /// Inputs: `gameState`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gameState">Input value used by this method.</param>
        public void SetGameState(GameState gameState)
        {
            currentGameState = gameState;
            if (currentGameState != GameState.BattlePreparing && currentGameState != GameState.BattleRunning)
            {
                isBattlePaused = false;
            }
        }

        /// <summary>
        /// Purpose: Stores whether the battle should be paused while keeping non-battle scenes unpaused.
        /// Inputs: paused is the requested pause state from UI or keyboard shortcuts.
        /// Output: no return value; updates IsBattlePaused only when the current game state can be paused.
        /// </summary>
        /// <param name="paused">True to pause a preparing/running battle; false to resume.</param>
        public void SetBattlePaused(bool paused)
        {
            bool canPause = currentGameState == GameState.BattlePreparing ||
                            currentGameState == GameState.BattleRunning;
            isBattlePaused = paused && canPause;
        }

        /// <summary>
        /// Purpose: Begins battle.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void BeginBattle()
        {
            ClearBattleResult();
            isBattlePaused = false;
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

        /// <summary>
        /// Purpose: Begins battle.
        /// Inputs: `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mode">Input value used by this method.</param>
        public void BeginBattle(GameMode mode)
        {
            SetGameMode(mode);
            BeginBattle();
        }

        /// <summary>
        /// Purpose: Resets session data to a safe default state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void ResetSessionData()
        {
            currentGameMode = GameMode.SinglePlayer;
            currentMapType = BattleMapType.Default;
            currentAIDifficulty = AIDifficulty.Normal;
            currentGameState = GameState.None;
            isBattlePaused = false;
            activeMapManager = null;
            player1 = null;
            player2 = null;
            aiPlayer = null;
            hasBattleSetupSnapshot = false;
            ResetSinglePlayerObjective();
            ResetLocalVsMatch();
            ClearBattleResult();
        }

        /// <summary>
        /// Purpose: Finishes battle.
        /// Inputs: `resultTitle`, `resultDetail`, `winnerName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="resultTitle">Input value used by this method.</param>
        /// <param name="resultDetail">Input value used by this method.</param>
        /// <param name="winnerName">Input value used by this method.</param>
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
            isBattlePaused = false;
            currentGameState = GameState.BattleFinished;

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle finished. Winner: {lastResultWinner}. Result: {lastResultTitle}");
            }
        }

        /// <summary>
        /// Purpose: Performs start battle round for this component.
        /// Inputs: `spawnProtectionSeconds`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="spawnProtectionSeconds">Input value used by this method.</param>
        public void StartBattleRound(float spawnProtectionSeconds)
        {
            if (currentGameState == GameState.BattleFinished)
            {
                return;
            }

            ApplySpawnProtectionToActiveCharacters(spawnProtectionSeconds);
            isBattlePaused = false;
            currentGameState = GameState.BattleRunning;

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle round started. Opening protection: {Mathf.Max(0f, spawnProtectionSeconds):0.00}s");
            }
        }

        /// <summary>
        /// Purpose: Clears battle result.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void ClearBattleResult()
        {
            hasBattleResult = false;
            lastResultTitle = "No Result Yet";
            lastResultDetail = "Start a battle to create a result.";
            lastResultWinner = "None";
        }

        /// <summary>
        /// Purpose: Resets local vs match to a safe default state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void ResetLocalVsMatch()
        {
            localVsPlayer1Score = 0;
            localVsPlayer2Score = 0;
            localVsRoundNumber = 1;
            lastLocalVsRoundWinner = "None";
            localVsMatchInProgress = false;
        }

        /// <summary>
        /// Purpose: Ensures local vs match started exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Registers local vs round result in the relevant runtime system.
        /// Inputs: `roundWinner`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="roundWinner">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Returns whether this object is local vs match complete.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        public bool IsLocalVsMatchComplete()
        {
            if (!enableLocalVsBestOf3)
            {
                return lastLocalVsRoundWinner == "Player1" || lastLocalVsRoundWinner == "Player2";
            }

            int targetScore = LocalVsTargetScore;
            return localVsPlayer1Score >= targetScore || localVsPlayer2Score >= targetScore;
        }

        /// <summary>
        /// Purpose: Resolves local vs match winner from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <returns>a `string` value.</returns>
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

        /// <summary>
        /// Purpose: Rebuilds the battle scene for the selected mode, map, and character choices.
        /// Inputs: no direct parameters; reads current mode, map, selected CharacterData, scene references, and prefabs.
        /// Output: no return value; configures the map, player slots, AI slot, bomb roots, and battle state.
        /// </summary>
        public void SetupBattleForCurrentMode()
        {
            if (IsCurrentBattleSetupAlreadyApplied())
            {
                return;
            }

            isBattlePaused = false;
            currentGameState = GameState.BattlePreparing;
            ClearBattleResult();
            ResolveBattleReferences();

            if (activeMapManager == null || player1 == null)
            {
                Debug.LogWarning("[GameManager] Battle setup skipped because MapManager or Player1 is missing.");
                return;
            }

            activeMapManager.SetMapType(currentMapType);
            // Rebuild data first, then visuals, so spawned characters use the newest grid rules.
            activeMapManager.InitializeGridData();
            activeMapManager.GenerateMap();
            ConfigureSinglePlayerObjective();
            EnsureCharacterSelectionsForBattle();

            Transform bombSpawnRoot = ResolveBombSpawnRoot();
            BombController bombPrefab = player1.BombPrefab;

            SetupPlayer1(bombSpawnRoot, bombPrefab);
            SetupPlayer2(bombSpawnRoot, bombPrefab);
            SetupAIPlayer(bombSpawnRoot, bombPrefab);
            ConfigureTutorialObjectiveRuntime();

            currentGameState = GameState.BattlePreparing;
            StoreBattleSetupSnapshot();
            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Battle prepared. Mode: {currentGameMode}, Map: {currentMapType}");
            }
        }

        /// <summary>
        /// Purpose: Prepares the Tutorial objective that teaches movement, bombs, pickups, and the exit route.
        /// Inputs: no direct parameters; reads the active map, spawn grid, goal grid, and configured soft-wall target.
        /// Output: no return value; stores objective counters and subscribes to soft-wall destruction events.
        /// </summary>
        private void ConfigureSinglePlayerObjective()
        {
            ResetSinglePlayerObjective();

            if (!IsSinglePlayerObjectiveEnabled || activeMapManager == null)
            {
                return;
            }

            int availableSoftWalls = activeMapManager.CountSoftWalls();
            activeSinglePlayerSoftWallTarget = IsTutorialMode && availableSoftWalls > 0
                ? Mathf.Clamp(activeMapManager.GetSinglePlayerTutorialRouteGateCount(), 1, availableSoftWalls)
                : availableSoftWalls > 0
                ? Mathf.Clamp(singlePlayerSoftWallTarget, 1, availableSoftWalls)
                : 0;
            singlePlayerGoalGrid = activeMapManager.GetSinglePlayerGoalGrid();
            tutorialMoveTargetGrid = activeMapManager.GetSinglePlayerTutorialMoveGrid();
            tutorialSoftWallGrid = activeMapManager.GetSinglePlayerTutorialSoftWallGrid();
            // Manhattan distance is enough here because movement is four-directional on a grid.
            singlePlayerStartGoalDistance = CalculateGridDistance(activeMapManager.GetPlayer1SpawnGrid(), singlePlayerGoalGrid);
            singlePlayerCurrentGoalDistance = singlePlayerStartGoalDistance;
            tutorialStep = IsTutorialMode ? TutorialStep.Move : TutorialStep.ReachExit;

            subscribedSinglePlayerObjectiveMapManager = activeMapManager;
            subscribedSinglePlayerObjectiveMapManager.SoftWallDestroyed += HandleSinglePlayerSoftWallDestroyed;

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] SinglePlayer route objective prepared. Goal: {singlePlayerGoalGrid}, start distance: {singlePlayerStartGoalDistance}.");
            }
        }

        /// <summary>
        /// Purpose: Resets single player objective to a safe default state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResetSinglePlayerObjective()
        {
            UnsubscribeSinglePlayerObjectiveMap();
            UnsubscribeTutorialObjectiveEvents();
            activeSinglePlayerSoftWallTarget = 0;
            singlePlayerSoftWallsCleared = 0;
            singlePlayerGoalGrid = new Vector2Int(1, 1);
            singlePlayerStartGoalDistance = 0;
            singlePlayerCurrentGoalDistance = 0;
            singlePlayerObjectiveComplete = false;
            tutorialStep = TutorialStep.Move;
            tutorialMoveTargetGrid = new Vector2Int(2, 1);
            tutorialSoftWallGrid = new Vector2Int(3, 1);
            tutorialLastBombGrid = new Vector2Int(-1, -1);
            tutorialBombPlaced = false;
            tutorialSoftWallCleared = false;
            tutorialItemStepSkipped = false;
        }

        /// <summary>
        /// Purpose: Handles single player soft wall destroyed.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        private void HandleSinglePlayerSoftWallDestroyed(Vector2Int gridPosition)
        {
            if (!IsSinglePlayerObjectiveEnabled || singlePlayerObjectiveComplete || currentGameState == GameState.BattleFinished)
            {
                return;
            }

            bool countsTowardObjective = !IsTutorialMode ||
                activeMapManager == null ||
                activeMapManager.IsSinglePlayerTutorialRouteGate(gridPosition);
            if (countsTowardObjective)
            {
                singlePlayerSoftWallsCleared = Mathf.Min(SinglePlayerSoftWallTarget, singlePlayerSoftWallsCleared + 1);
            }

            if (IsTutorialMode)
            {
                tutorialSoftWallCleared = countsTowardObjective || tutorialSoftWallCleared;
                if (countsTowardObjective && (tutorialStep == TutorialStep.BreakSoftWall || tutorialStep == TutorialStep.RunAway))
                {
                    AdvanceTutorialStep(ShouldSkipTutorialItemStep(gridPosition)
                        ? TutorialStep.ReachExit
                        : TutorialStep.PickUpItem);
                }
            }

            RefreshSinglePlayerRouteObjective();

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] SinglePlayer objective progress: {SinglePlayerObjectiveProgressLabel} at {gridPosition}");
            }
        }

        /// <summary>
        /// Purpose: Refreshes single player route objective from the latest runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void RefreshSinglePlayerRouteObjective()
        {
            if (!IsSinglePlayerObjectiveEnabled || singlePlayerObjectiveComplete || activeMapManager == null || player1 == null)
            {
                return;
            }

            singlePlayerCurrentGoalDistance = CalculateGridDistance(player1.CurrentGridPosition, singlePlayerGoalGrid);
            if (IsTutorialMode && tutorialStep != TutorialStep.ReachExit && tutorialStep != TutorialStep.Complete)
            {
                return;
            }

            if (activeMapManager.IsSinglePlayerGoal(player1.CurrentGridPosition))
            {
                singlePlayerObjectiveComplete = true;
                singlePlayerCurrentGoalDistance = 0;
                if (IsTutorialMode)
                {
                    AdvanceTutorialStep(TutorialStep.Complete);
                }
            }
        }

        /// <summary>
        /// Purpose: Connects runtime tutorial events after player objects are configured for battle.
        /// Inputs: no direct parameters; reads current player and item pickup event state.
        /// Output: no return value; subscribes to player/item callbacks when tutorial mode is active.
        /// </summary>
        private void ConfigureTutorialObjectiveRuntime()
        {
            if (!IsTutorialMode || player1 == null)
            {
                return;
            }

            SubscribeTutorialObjectiveEvents();
        }

        /// <summary>
        /// Purpose: Advances the tutorial from polling-friendly state such as movement and safe distance.
        /// Inputs: no direct parameters; reads player position, current step, and map item state.
        /// Output: no return value; updates the current tutorial step when its goal is satisfied.
        /// </summary>
        private void TickTutorialObjective()
        {
            if (!IsTutorialMode ||
                currentGameState == GameState.BattleFinished ||
                player1 == null ||
                activeMapManager == null ||
                singlePlayerObjectiveComplete)
            {
                return;
            }

            singlePlayerCurrentGoalDistance = CalculateGridDistance(player1.CurrentGridPosition, singlePlayerGoalGrid);
            switch (tutorialStep)
            {
                case TutorialStep.Move:
                    if (player1.CurrentGridPosition == tutorialMoveTargetGrid ||
                        player1.CurrentGridPosition != activeMapManager.GetPlayer1SpawnGrid())
                    {
                        AdvanceTutorialStep(TutorialStep.PlaceBomb);
                    }
                    break;
                case TutorialStep.RunAway:
                    if (tutorialBombPlaced && IsPlayerSafeFromTutorialBomb())
                    {
                        AdvanceTutorialStep(TutorialStep.BreakSoftWall);
                    }
                    break;
                case TutorialStep.PickUpItem:
                    if (tutorialSoftWallCleared && ShouldSkipTutorialItemStep(tutorialSoftWallGrid))
                    {
                        AdvanceTutorialStep(TutorialStep.ReachExit);
                    }
                    break;
                case TutorialStep.ReachExit:
                    RefreshSinglePlayerRouteObjective();
                    break;
            }
        }

        /// <summary>
        /// Purpose: Subscribes to callbacks used by the tutorial step tracker.
        /// Inputs: no direct parameters; reads player1 and static item pickup event.
        /// Output: no return value; updates subscription state.
        /// </summary>
        private void SubscribeTutorialObjectiveEvents()
        {
            if (subscribedTutorialPlayer == player1 && tutorialItemPickupSubscribed)
            {
                return;
            }

            UnsubscribeTutorialObjectiveEvents();
            subscribedTutorialPlayer = player1;
            subscribedTutorialPlayer.BombPlaced += HandleTutorialBombPlaced;
            ItemBase.ItemPickedUp += HandleTutorialItemPickedUp;
            tutorialItemPickupSubscribed = true;
        }

        /// <summary>
        /// Purpose: Unsubscribes tutorial callbacks to avoid stale scene references.
        /// Inputs: no direct parameters; reads cached subscription fields.
        /// Output: no return value; updates subscription state.
        /// </summary>
        private void UnsubscribeTutorialObjectiveEvents()
        {
            if (subscribedTutorialPlayer != null)
            {
                subscribedTutorialPlayer.BombPlaced -= HandleTutorialBombPlaced;
                subscribedTutorialPlayer = null;
            }

            if (tutorialItemPickupSubscribed)
            {
                ItemBase.ItemPickedUp -= HandleTutorialItemPickedUp;
                tutorialItemPickupSubscribed = false;
            }
        }

        /// <summary>
        /// Purpose: Handles the first player bomb placement for tutorial progression.
        /// Inputs: sourceCharacter is the character that placed a bomb.
        /// Output: no return value; updates the tutorial step and cached bomb grid.
        /// </summary>
        /// <param name="sourceCharacter">Character that placed the bomb.</param>
        private void HandleTutorialBombPlaced(CharacterBase sourceCharacter)
        {
            if (!IsTutorialMode || sourceCharacter != player1)
            {
                return;
            }

            tutorialBombPlaced = true;
            tutorialLastBombGrid = sourceCharacter.CurrentGridPosition;
            if (tutorialStep == TutorialStep.Move || tutorialStep == TutorialStep.PlaceBomb)
            {
                AdvanceTutorialStep(TutorialStep.RunAway);
            }
        }

        /// <summary>
        /// Purpose: Handles tutorial item pickup progression.
        /// Inputs: character and item come from the shared item system pickup callback.
        /// Output: no return value; advances to the exit step after the tutorial item is collected.
        /// </summary>
        /// <param name="character">Character that picked up the item.</param>
        /// <param name="item">Picked item.</param>
        private void HandleTutorialItemPickedUp(CharacterBase character, ItemBase item)
        {
            if (!IsTutorialMode || character != player1 || tutorialStep != TutorialStep.PickUpItem)
            {
                return;
            }

            AdvanceTutorialStep(TutorialStep.ReachExit);
        }

        /// <summary>
        /// Purpose: Updates the current tutorial step without moving backwards.
        /// Inputs: nextStep is the requested new state.
        /// Output: no return value; updates serialized tutorial state.
        /// </summary>
        /// <param name="nextStep">Requested tutorial step.</param>
        private void AdvanceTutorialStep(TutorialStep nextStep)
        {
            if (nextStep <= tutorialStep)
            {
                return;
            }

            tutorialStep = nextStep;
            if (tutorialStep == TutorialStep.Complete)
            {
                singlePlayerObjectiveComplete = true;
                singlePlayerCurrentGoalDistance = 0;
            }

            if (logBattleSetup)
            {
                Debug.Log($"[GameManager] Tutorial step: {tutorialStep}");
            }
        }

        /// <summary>
        /// Purpose: Determines if the player is outside the simple cross-blast lesson line.
        /// Inputs: no direct parameters; reads cached bomb grid, player position, and bomb range.
        /// Output: true when the player has left the cached bomb's row/column danger line.
        /// </summary>
        /// <returns>True when the player is safe from the tutorial bomb line.</returns>
        private bool IsPlayerSafeFromTutorialBomb()
        {
            if (tutorialLastBombGrid.x < 0 || player1 == null)
            {
                return false;
            }

            Vector2Int playerGrid = player1.CurrentGridPosition;
            int dx = Mathf.Abs(playerGrid.x - tutorialLastBombGrid.x);
            int dy = Mathf.Abs(playerGrid.y - tutorialLastBombGrid.y);
            if (dx + dy == 0)
            {
                return false;
            }

            int range = Mathf.Max(1, player1.BombRange);
            bool onHorizontalBlastLine = dy == 0 && dx <= range;
            bool onVerticalBlastLine = dx == 0 && dy <= range;
            return !onHorizontalBlastLine && !onVerticalBlastLine;
        }

        /// <summary>
        /// Purpose: Returns whether the pickup tutorial step should be skipped because no item exists.
        /// Inputs: gridPosition identifies the soft wall cell that should contain the guaranteed item.
        /// Output: true when no item is registered on that grid cell.
        /// </summary>
        /// <param name="gridPosition">Grid position to inspect.</param>
        /// <returns>True if the item step should be skipped.</returns>
        private bool ShouldSkipTutorialItemStep(Vector2Int gridPosition)
        {
            if (activeMapManager == null)
            {
                tutorialItemStepSkipped = true;
                return true;
            }

            GridCell cell = activeMapManager.GetCell(gridPosition);
            bool shouldSkip = cell == null || !cell.HasItem;
            tutorialItemStepSkipped = shouldSkip;
            return shouldSkip;
        }

        /// <summary>
        /// Purpose: Resolves the compact tutorial objective title for HUD.
        /// Inputs: no direct parameters; reads current tutorial step.
        /// Output: short player-facing objective label.
        /// </summary>
        /// <returns>Current tutorial objective label.</returns>
        private string ResolveTutorialObjectiveLabel()
        {
            switch (tutorialStep)
            {
                case TutorialStep.Move:
                    return "Move";
                case TutorialStep.PlaceBomb:
                    return "Place Bomb";
                case TutorialStep.RunAway:
                    return "Run Away";
                case TutorialStep.BreakSoftWall:
                    return "Break Wall";
                case TutorialStep.PickUpItem:
                    return "Pick Up";
                case TutorialStep.ReachExit:
                    return "Clear Route";
                default:
                    return "Tutorial Clear";
            }
        }

        /// <summary>
        /// Purpose: Resolves the compact tutorial hint for HUD.
        /// Inputs: no direct parameters; reads current tutorial step and route distance.
        /// Output: short player-facing objective detail.
        /// </summary>
        /// <returns>Current tutorial progress text.</returns>
        private string ResolveTutorialProgressLabel()
        {
            switch (tutorialStep)
            {
                case TutorialStep.Move:
                    return "WASD";
                case TutorialStep.PlaceBomb:
                    return "SPACE";
                case TutorialStep.RunAway:
                    return "SAFE TILE";
                case TutorialStep.BreakSoftWall:
                    return "WAIT";
                case TutorialStep.PickUpItem:
                    return tutorialItemStepSkipped ? "SKIPPED" : "GRAB IT";
                case TutorialStep.ReachExit:
                    return singlePlayerObjectiveComplete ? "DONE" : "KEEP GOING";
                default:
                    return "DONE";
            }
        }

        /// <summary>
        /// Purpose: Resolves the full tutorial instruction shown in the battle overlay.
        /// Inputs: no direct parameters; reads current tutorial step.
        /// Output: one clear player-facing sentence for the current step.
        /// </summary>
        /// <returns>Current tutorial instruction text.</returns>
        private string ResolveTutorialHintLabel()
        {
            switch (tutorialStep)
            {
                case TutorialStep.Move:
                    return "Use WASD to step onto the glowing arrow tile.";
                case TutorialStep.PlaceBomb:
                    return "Stand beside the nearby soft wall and press SPACE.";
                case TutorialStep.RunAway:
                    return "Move off the bomb row or column before it pops.";
                case TutorialStep.BreakSoftWall:
                    return "Wait for the blast to break the soft wall.";
                case TutorialStep.PickUpItem:
                    return tutorialItemStepSkipped
                        ? "No item dropped this time. Follow the path to the exit."
                        : "Walk onto the dropped power-up to collect it.";
                case TutorialStep.ReachExit:
                    return "Keep bombing soft walls on the route and reach the glowing exit.";
                default:
                    return "Tutorial complete. Nice and tidy.";
            }
        }

        /// <summary>
        /// Purpose: Resolves tutorial progress as a normalized HUD fill amount.
        /// Inputs: no direct parameters; reads current tutorial step and exit route progress.
        /// Output: value from 0 to 1.
        /// </summary>
        /// <returns>Normalized tutorial progress.</returns>
        private float ResolveTutorialProgress()
        {
            if (tutorialStep == TutorialStep.Complete || singlePlayerObjectiveComplete)
            {
                return 1f;
            }

            float stepIndex = Mathf.Clamp((int)tutorialStep, 0, TutorialPlayableStepCount - 1);
            float baseProgress = stepIndex / TutorialPlayableStepCount;
            if (tutorialStep != TutorialStep.ReachExit || singlePlayerStartGoalDistance <= 0)
            {
                return Mathf.Clamp01(baseProgress + 0.05f);
            }

            float routeProgress = 1f - Mathf.Clamp01((float)singlePlayerCurrentGoalDistance / singlePlayerStartGoalDistance);
            return Mathf.Clamp01(Mathf.Lerp(baseProgress, 1f, routeProgress));
        }

        /// <summary>
        /// Purpose: Checks whether a grid cell is the tutorial soft wall used for the guaranteed pickup.
        /// Inputs: gridPosition is the soft wall cell being queried.
        /// Output: true when the current mode is tutorial and the grid matches the tutorial wall.
        /// </summary>
        /// <param name="gridPosition">Grid position to test.</param>
        /// <returns>True for the tutorial soft wall cell.</returns>
        public bool IsTutorialSoftWallGrid(Vector2Int gridPosition)
        {
            return IsTutorialMode && gridPosition == tutorialSoftWallGrid;
        }

        /// <summary>
        /// Purpose: Calculates grid distance.
        /// Inputs: `from`, `to`; may also read serialized fields and current runtime state.
        /// Output: a `int` value.
        /// </summary>
        /// <param name="from">Input value used by this method.</param>
        /// <param name="to">Input value used by this method.</param>
        /// <returns>a `int` value.</returns>
        private int CalculateGridDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// Purpose: Performs unsubscribe single player objective map for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void UnsubscribeSinglePlayerObjectiveMap()
        {
            if (subscribedSinglePlayerObjectiveMapManager == null)
            {
                return;
            }

            subscribedSinglePlayerObjectiveMapManager.SoftWallDestroyed -= HandleSinglePlayerSoftWallDestroyed;
            subscribedSinglePlayerObjectiveMapManager = null;
        }

        /// <summary>
        /// Purpose: Returns whether this object is current battle setup already applied.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool IsCurrentBattleSetupAlreadyApplied()
        {
            bool isBattleSetupState = currentGameState == GameState.BattlePreparing ||
                                      currentGameState == GameState.BattleRunning;
            // Prevent duplicate setup when Unity reloads or UI requests setup more than once for the same scene.
            return isBattleSetupState &&
                   hasBattleSetupSnapshot &&
                   lastBattleSetupSceneHandle == SceneManager.GetActiveScene().handle &&
                   lastBattleSetupMode == currentGameMode &&
                   lastBattleSetupMapType == currentMapType &&
                   lastBattleSetupAIDifficulty == currentAIDifficulty;
        }

        /// <summary>
        /// Purpose: Performs store battle setup snapshot for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void StoreBattleSetupSnapshot()
        {
            hasBattleSetupSnapshot = true;
            lastBattleSetupSceneHandle = SceneManager.GetActiveScene().handle;
            lastBattleSetupMode = currentGameMode;
            lastBattleSetupMapType = currentMapType;
            lastBattleSetupAIDifficulty = currentAIDifficulty;
        }

        /// <summary>
        /// Purpose: Handles the scene loaded event or callback.
        /// Inputs: `scene`, `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="scene">Input value used by this method.</param>
        /// <param name="mode">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Updates game state for scene.
        /// Inputs: `sceneName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="sceneName">Input value used by this method.</param>
        private void UpdateGameStateForScene(string sceneName)
        {
            if (sceneName != GameConstants.SceneBattle)
            {
                isBattlePaused = false;
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

        /// <summary>
        /// Purpose: Resolves battle references from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResolveBattleReferences()
        {
            activeMapManager = FindObjectOfType<MapManager>();
            player1 = FindPlayerControllerByName(Player1ObjectName);
            player2 = FindPlayerControllerByName(Player2ObjectName);
            aiPlayer = FindAIControllerByName(AIObjectName);
        }

        /// <summary>
        /// Purpose: Finds player controller by name from scene objects or cached data.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `PlayerController` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `PlayerController` value.</returns>
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

        /// <summary>
        /// Purpose: Finds aicontroller by name from scene objects or cached data.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `AIController` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `AIController` value.</returns>
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

        /// <summary>
        /// Purpose: Resolves characters root from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Transform` value.
        /// </summary>
        /// <returns>a `Transform` value.</returns>
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

        /// <summary>
        /// Purpose: Resolves bomb spawn root from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Transform` value.
        /// </summary>
        /// <returns>a `Transform` value.</returns>
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

        /// <summary>
        /// Purpose: Sets up player1.
        /// Inputs: `bombSpawnRoot`, `bombPrefab`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="bombSpawnRoot">Input value used by this method.</param>
        /// <param name="bombPrefab">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Sets up player2.
        /// Inputs: `bombSpawnRoot`, `bombPrefab`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="bombSpawnRoot">Input value used by this method.</param>
        /// <param name="bombPrefab">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Sets up aiplayer.
        /// Inputs: `bombSpawnRoot`, `bombPrefab`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="bombSpawnRoot">Input value used by this method.</param>
        /// <param name="bombPrefab">Input value used by this method.</param>
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
            aiPlayer.ConfigureDifficulty(currentAIDifficulty);
            aiPlayer.ConfigureForBattle(
                activeMapManager,
                activeMapManager.GetAISpawnGrid(),
                bombSpawnRoot,
                bombPrefab);
        }

        /// <summary>
        /// Purpose: Ensures character selections for battle exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void EnsureCharacterSelectionsForBattle()
        {
            EnsureCharacterSelections();
            if (currentGameMode == GameMode.AIBattle && randomizeAICharacterOnBattleStart)
            {
                RandomizeAICharacter();
            }
        }

        /// <summary>
        /// Purpose: Ensures character selections exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Resolves character or default from the current runtime state.
        /// Inputs: `characterData`, `preferredIndex`; may also read serialized fields and current runtime state.
        /// Output: a `CharacterData` value.
        /// </summary>
        /// <param name="characterData">Input value used by this method.</param>
        /// <param name="preferredIndex">Input value used by this method.</param>
        /// <returns>a `CharacterData` value.</returns>
        private CharacterData ResolveCharacterOrDefault(CharacterData characterData, int preferredIndex)
        {
            return characterData != null ? characterData : CharacterRoster.GetDefaultCharacter(preferredIndex);
        }

        /// <summary>
        /// Purpose: Resolves character by id or default from the current runtime state.
        /// Inputs: `currentCharacter`, `characterId`, `preferredIndex`; may also read serialized fields and current runtime state.
        /// Output: a `CharacterData` value.
        /// </summary>
        /// <param name="currentCharacter">Input value used by this method.</param>
        /// <param name="characterId">Input value used by this method.</param>
        /// <param name="preferredIndex">Input value used by this method.</param>
        /// <returns>a `CharacterData` value.</returns>
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

        /// <summary>
        /// Purpose: Ensures different local vs characters exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Applies character selection to the current object or scene.
        /// Inputs: `character`, `characterData`, `roleObjectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="characterData">Input value used by this method.</param>
        /// <param name="roleObjectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Applies spawn protection to active characters to the current object or scene.
        /// Inputs: `protectionSeconds`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="protectionSeconds">Input value used by this method.</param>
        private void ApplySpawnProtectionToActiveCharacters(float protectionSeconds)
        {
            float resolvedProtectionSeconds = Mathf.Max(0f, protectionSeconds);
            ApplySpawnProtection(player1, resolvedProtectionSeconds);
            ApplySpawnProtection(player2, resolvedProtectionSeconds);
            ApplySpawnProtection(aiPlayer, resolvedProtectionSeconds);
        }

        /// <summary>
        /// Purpose: Applies spawn protection to the current object or scene.
        /// Inputs: `character`, `protectionSeconds`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="protectionSeconds">Input value used by this method.</param>
        private void ApplySpawnProtection(CharacterBase character, float protectionSeconds)
        {
            if (character == null || !character.gameObject.activeInHierarchy)
            {
                return;
            }

            character.SetInvincible(protectionSeconds);
        }

        /// <summary>
        /// Purpose: Ensures aiplayer exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `AIController` value.
        /// </summary>
        /// <returns>a `AIController` value.</returns>
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
