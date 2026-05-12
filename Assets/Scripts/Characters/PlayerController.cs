using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Handles local keyboard input and forwards actions to CharacterBase.
    /// </summary>
    public class PlayerController : CharacterBase
    {
        [Header("Mode")]
        [SerializeField] private bool localVsOnly = false;

        [Header("Input")]
        [SerializeField] private KeyCode moveUpKey = KeyCode.W;
        [SerializeField] private KeyCode moveDownKey = KeyCode.S;
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode primaryBombKey = KeyCode.Space;
        [SerializeField] private KeyCode secondaryBombKey = KeyCode.None;

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected override void Start()
        {
            if (!ShouldActivateForCurrentMode())
            {
                gameObject.SetActive(false);
                return;
            }

            base.Start();
        }

        /// <summary>
        /// Purpose: Runs this component's per-frame logic.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (!IsAlive)
            {
                return;
            }

            if (!CanAcceptBattleInput())
            {
                return;
            }

            HandleMovementInput();
            HandleBombInput();
        }

        /// <summary>
        /// Purpose: Handles movement input.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void HandleMovementInput()
        {
            if (IsMoving)
            {
                return;
            }

            Vector2Int gridDirection = ReadMoveInput();
            if (gridDirection != Vector2Int.zero)
            {
                TryMoveGridDirection(gridDirection);
            }
        }

        /// <summary>
        /// Purpose: Returns whether this object should activate for current mode.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool ShouldActivateForCurrentMode()
        {
            if (!localVsOnly)
            {
                return true;
            }

            if (GameManager.Instance == null)
            {
                return true;
            }

            return GameManager.Instance.CurrentGameMode == GameMode.LocalVS;
        }

        /// <summary>
        /// Purpose: Returns whether this object can accept battle input.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool CanAcceptBattleInput()
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager == null ||
                   (gameManager.CurrentGameState == GameState.BattleRunning && !gameManager.IsBattlePaused);
        }

        /// <summary>
        /// Purpose: Returns read move input for the current state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <returns>a `Vector2Int` value.</returns>
        private Vector2Int ReadMoveInput()
        {
            if (Input.GetKey(moveUpKey)) return Vector2Int.up;
            if (Input.GetKey(moveDownKey)) return Vector2Int.down;
            if (Input.GetKey(moveLeftKey)) return Vector2Int.left;
            if (Input.GetKey(moveRightKey)) return Vector2Int.right;

            return Vector2Int.zero;
        }

        /// <summary>
        /// Purpose: Handles bomb input.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void HandleBombInput()
        {
            if (IsBombInputPressed())
            {
                OnBombInputPressed();
            }
        }

        /// <summary>
        /// Purpose: Returns whether this object is bomb input pressed.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool IsBombInputPressed()
        {
            if (Input.GetKeyDown(primaryBombKey))
            {
                return true;
            }

            return secondaryBombKey != KeyCode.None && Input.GetKeyDown(secondaryBombKey);
        }

        /// <summary>
        /// Purpose: Handles the bomb input pressed event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void OnBombInputPressed()
        {
            bool placedBomb = TryPlaceBomb();
            Debug.Log($"[PlayerController] {name} bomb input pressed. Placed: {placedBomb}. Bombs: {ActiveBombCount}/{MaxBombCount}");
        }
    }
}
