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

        private void Update()
        {
            HandleMovementInput();
            HandleBombInput();
        }

        private void HandleMovementInput()
        {
            Vector3 move = Vector3.zero;

            if (isPlayerOne)
            {
                if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
                if (Input.GetKey(KeyCode.S)) move += Vector3.back;
                if (Input.GetKey(KeyCode.A)) move += Vector3.left;
                if (Input.GetKey(KeyCode.D)) move += Vector3.right;
            }
            else
            {
                if (Input.GetKey(KeyCode.UpArrow)) move += Vector3.forward;
                if (Input.GetKey(KeyCode.DownArrow)) move += Vector3.back;
                if (Input.GetKey(KeyCode.LeftArrow)) move += Vector3.left;
                if (Input.GetKey(KeyCode.RightArrow)) move += Vector3.right;
            }

            if (move != Vector3.zero)
            {
                Move(move);
            }
        }

        private void HandleBombInput()
        {
            bool pressed = isPlayerOne
                ? Input.GetKeyDown(KeyCode.Space)
                : Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightControl);

            if (pressed)
            {
                TryPlaceBomb();
            }
        }
    }
}
