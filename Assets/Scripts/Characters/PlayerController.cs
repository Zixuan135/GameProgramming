using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Handles local keyboard input and forwards actions to CharacterBase.
    /// </summary>
    public class PlayerController : CharacterBase
    {
        [Header("Input")]
        [SerializeField] private bool isPlayerOne = true;

        protected override void Update()
        {
            base.Update();
            HandleMovementInput();
            HandleBombInput();
        }

        private void HandleMovementInput()
        {
            if (IsMoving)
            {
                return;
            }

            Vector2Int gridDirection = Vector2Int.zero;

            if (isPlayerOne)
            {
                gridDirection = ReadPlayerOneMoveInput();
            }
            else
            {
                gridDirection = ReadPlayerTwoMoveInput();
            }

            if (gridDirection != Vector2Int.zero)
            {
                TryMoveGridDirection(gridDirection);
            }
        }

        private Vector2Int ReadPlayerOneMoveInput()
        {
            if (Input.GetKey(KeyCode.W)) return Vector2Int.up;
            if (Input.GetKey(KeyCode.S)) return Vector2Int.down;
            if (Input.GetKey(KeyCode.A)) return Vector2Int.left;
            if (Input.GetKey(KeyCode.D)) return Vector2Int.right;

            return Vector2Int.zero;
        }

        private Vector2Int ReadPlayerTwoMoveInput()
        {
            if (Input.GetKey(KeyCode.UpArrow)) return Vector2Int.up;
            if (Input.GetKey(KeyCode.DownArrow)) return Vector2Int.down;
            if (Input.GetKey(KeyCode.LeftArrow)) return Vector2Int.left;
            if (Input.GetKey(KeyCode.RightArrow)) return Vector2Int.right;

            return Vector2Int.zero;
        }

        private void HandleBombInput()
        {
            bool pressed = isPlayerOne
                ? Input.GetKeyDown(KeyCode.Space)
                : Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightControl);

            if (pressed)
            {
                OnBombInputPressed();
            }
        }

        protected virtual void OnBombInputPressed()
        {
            Debug.Log("[PlayerController] Bomb input pressed. Bomb placement will be implemented later.");
        }
    }
}
