using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Managers
{
    /// <summary>
    /// Stores runtime game session data and shared references.
    /// This is intentionally lightweight in the skeleton phase.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Session")]
        [SerializeField] private GameMode currentGameMode = GameMode.SinglePlayer;
        [SerializeField] private BattleMapType currentMapType = BattleMapType.Default;

        public GameMode CurrentGameMode => currentGameMode;
        public BattleMapType CurrentMapType => currentMapType;

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

        public void SetGameMode(GameMode mode)
        {
            currentGameMode = mode;
        }

        public void SetMapType(BattleMapType mapType)
        {
            currentMapType = mapType;
        }

        public void ResetSessionData()
        {
            currentGameMode = GameMode.SinglePlayer;
            currentMapType = BattleMapType.Default;
        }
    }
}
