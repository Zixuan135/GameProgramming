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

        protected override void Start()
        {
            if (!ShouldActivateForCurrentMode())
            {
                gameObject.SetActive(false);
                return;
            }

            base.Start();
        }

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

        private bool CanAcceptBattleInput()
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager == null ||
                   (gameManager.CurrentGameState == GameState.BattleRunning && !gameManager.IsBattlePaused);
        }

        private Vector2Int ReadMoveInput()
        {
            if (Input.GetKey(moveUpKey)) return Vector2Int.up;
            if (Input.GetKey(moveDownKey)) return Vector2Int.down;
            if (Input.GetKey(moveLeftKey)) return Vector2Int.left;
            if (Input.GetKey(moveRightKey)) return Vector2Int.right;

            return Vector2Int.zero;
        }

        private void HandleBombInput()
        {
            if (IsBombInputPressed())
            {
                OnBombInputPressed();
            }
        }

        private bool IsBombInputPressed()
        {
            if (Input.GetKeyDown(primaryBombKey))
            {
                return true;
            }

            return secondaryBombKey != KeyCode.None && Input.GetKeyDown(secondaryBombKey);
        }

        protected virtual void OnBombInputPressed()
        {
            bool placedBomb = TryPlaceBomb();
            Debug.Log($"[PlayerController] {name} bomb input pressed. Placed: {placedBomb}. Bombs: {ActiveBombCount}/{MaxBombCount}");
        }
    }
}
