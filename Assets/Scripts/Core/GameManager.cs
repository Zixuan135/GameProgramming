using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown
{
    public class GameManager : MonoBehaviour
    {
        [Serializable]
        private struct SpawnRequest
        {
            public SpawnSlot slot;
            public CharacterType characterType;
            public ControlScheme controlScheme;
        }

        [Header("Mode Runtime")]
        [SerializeField] private GameMode currentMode = GameMode.SinglePlayer;
        [SerializeField] private int selectedMapId;

        [Header("Character Prefabs")]
        [SerializeField] private CharacterBase playerCharacterPrefab;
        [SerializeField] private CharacterBase aiCharacterPrefab;

        [Header("Runtime Parent (Optional)")]
        [SerializeField] private Transform runtimeActorRoot;

        public static GameManager Instance { get; private set; }

        public GameMode CurrentMode => currentMode;
        public int SelectedMapId => selectedMapId;
        public MatchOutcome LastMatchOutcome { get; private set; } = MatchOutcome.None;

        public event Action<GameMode> OnModeChanged;
        public event Action<int> OnMapChanged;
        public event Action<List<CharacterBase>> OnBattleActorsSpawned;
        public event Action<MatchOutcome> OnBattleEnded;

        private readonly List<CharacterBase> aliveCharacters = new List<CharacterBase>();

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
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void SetGameMode(GameMode mode)
        {
            currentMode = mode;
            OnModeChanged?.Invoke(currentMode);
        }

        public void SetMapId(int mapId)
        {
            selectedMapId = Mathf.Max(0, mapId);
            OnMapChanged?.Invoke(selectedMapId);
        }

        public void StartBattleFlow()
        {
            LastMatchOutcome = MatchOutcome.None;
            SceneFlowManager.Instance.LoadBattle();
        }

        public void RegisterCharacter(CharacterBase character)
        {
            if (character == null || aliveCharacters.Contains(character))
            {
                return;
            }

            aliveCharacters.Add(character);
            character.OnCharacterDied += HandleCharacterDied;
        }

        public void UnregisterCharacter(CharacterBase character)
        {
            if (character == null)
            {
                return;
            }

            character.OnCharacterDied -= HandleCharacterDied;
            aliveCharacters.Remove(character);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == SceneFlowManager.GetSceneName(GameScene.Battle))
            {
                InitializeBattleScene();
            }
        }

        private void InitializeBattleScene()
        {
            var mapManager = FindObjectOfType<MapManager>();
            if (mapManager == null)
            {
                Debug.LogError("MapManager is required in Battle scene.");
                return;
            }

            mapManager.BuildMap(selectedMapId);

            aliveCharacters.Clear();
            List<CharacterBase> spawnedActors = SpawnActorsForCurrentMode(mapManager);
            OnBattleActorsSpawned?.Invoke(spawnedActors);
        }

        private List<CharacterBase> SpawnActorsForCurrentMode(MapManager mapManager)
        {
            List<CharacterBase> spawned = new List<CharacterBase>();

            foreach (SpawnRequest request in BuildSpawnRequests())
            {
                CharacterBase prefab = request.characterType == CharacterType.Player ? playerCharacterPrefab : aiCharacterPrefab;
                if (prefab == null)
                {
                    Debug.LogWarning($"Missing prefab for {request.characterType}, skip spawn.");
                    continue;
                }

                Vector3 spawnPos = mapManager.GetSpawnWorldPosition(request.slot);
                CharacterBase actor = Instantiate(prefab, spawnPos, Quaternion.identity, runtimeActorRoot);
                actor.InitializeAs(request.characterType);
                actor.CharacterLabel = request.slot.ToString();

                EnsureController(actor, request);
                RegisterCharacter(actor);
                spawned.Add(actor);
            }

            return spawned;
        }

        private IEnumerable<SpawnRequest> BuildSpawnRequests()
        {
            switch (currentMode)
            {
                case GameMode.SinglePlayer:
                    yield return new SpawnRequest
                    {
                        slot = SpawnSlot.Player1,
                        characterType = CharacterType.Player,
                        controlScheme = ControlScheme.PlayerOne
                    };
                    break;

                case GameMode.AIBattle:
                    yield return new SpawnRequest
                    {
                        slot = SpawnSlot.Player1,
                        characterType = CharacterType.Player,
                        controlScheme = ControlScheme.PlayerOne
                    };
                    yield return new SpawnRequest
                    {
                        slot = SpawnSlot.AI1,
                        characterType = CharacterType.AI,
                        controlScheme = ControlScheme.PlayerOne
                    };
                    break;

                case GameMode.LocalVS:
                    yield return new SpawnRequest
                    {
                        slot = SpawnSlot.Player1,
                        characterType = CharacterType.Player,
                        controlScheme = ControlScheme.PlayerOne
                    };
                    yield return new SpawnRequest
                    {
                        slot = SpawnSlot.Player2,
                        characterType = CharacterType.Player,
                        controlScheme = ControlScheme.PlayerTwo
                    };
                    break;
            }
        }

        private void EnsureController(CharacterBase actor, SpawnRequest request)
        {
            PlayerController playerController = actor.GetComponent<PlayerController>();
            AIController aiController = actor.GetComponent<AIController>();

            if (request.characterType == CharacterType.Player)
            {
                if (aiController != null)
                {
                    Destroy(aiController);
                }

                if (playerController == null)
                {
                    playerController = actor.gameObject.AddComponent<PlayerController>();
                }

                playerController.AssignControlScheme(request.controlScheme);
            }
            else
            {
                if (playerController != null)
                {
                    Destroy(playerController);
                }

                if (aiController == null)
                {
                    actor.gameObject.AddComponent<AIController>();
                }
            }
        }

        private void HandleCharacterDied(CharacterBase deadCharacter)
        {
            UnregisterCharacter(deadCharacter);

            if (aliveCharacters.Count > 1)
            {
                return;
            }

            LastMatchOutcome = ResolveOutcome();
            OnBattleEnded?.Invoke(LastMatchOutcome);
            SceneFlowManager.Instance.LoadResult();
        }

        private MatchOutcome ResolveOutcome()
        {
            if (aliveCharacters.Count == 0)
            {
                return MatchOutcome.Draw;
            }

            CharacterBase winner = aliveCharacters.First();
            if (winner.CharacterType == CharacterType.AI)
            {
                return MatchOutcome.AIWin;
            }

            PlayerController winnerPlayer = winner.GetComponent<PlayerController>();
            if (winnerPlayer != null && winnerPlayer.ControlScheme == ControlScheme.PlayerTwo)
            {
                return MatchOutcome.Player2Win;
            }

            return MatchOutcome.Player1Win;
        }
    }
}
