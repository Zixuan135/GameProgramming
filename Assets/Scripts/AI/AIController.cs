using BubbleTown.Characters;
using UnityEngine;

namespace BubbleTown.AI
{
    /// <summary>
    /// Very simple placeholder AI loop.
    /// Real pathing and tactical bomb behavior will be added later.
    /// </summary>
    public class AIController : CharacterBase
    {
        [Header("AI Placeholder")]
        [SerializeField] private float directionChangeInterval = 1f;
        [SerializeField] private float bombDecisionInterval = 3f;

        private float directionTimer;
        private float bombTimer;
        private Vector3 currentDirection = Vector3.zero;

        private void Update()
        {
            UpdateDirection();
            UpdateBombDecision();

            if (currentDirection != Vector3.zero)
            {
                Move(currentDirection);
            }
        }

        private void UpdateDirection()
        {
            directionTimer -= Time.deltaTime;
            if (directionTimer > 0f)
            {
                return;
            }

            directionTimer = directionChangeInterval;
            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0:
                    currentDirection = Vector3.forward;
                    break;
                case 1:
                    currentDirection = Vector3.back;
                    break;
                case 2:
                    currentDirection = Vector3.left;
                    break;
                default:
                    currentDirection = Vector3.right;
                    break;
            }
        }

        private void UpdateBombDecision()
        {
            bombTimer -= Time.deltaTime;
            if (bombTimer > 0f)
            {
                return;
            }

            bombTimer = bombDecisionInterval;
            TryPlaceBomb();
        }
    }
}
