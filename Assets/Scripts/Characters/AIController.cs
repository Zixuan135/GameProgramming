using UnityEngine;

namespace BubbleTown
{
    [RequireComponent(typeof(CharacterBase))]
    public class AIController : MonoBehaviour
    {
        [Header("Simple AI")]
        [SerializeField] private float changeDirectionInterval = 1.2f;
        [SerializeField] private float bombDecisionInterval = 1f;
        [SerializeField] private float bombChancePerCheck = 0.25f;

        private CharacterBase character;
        private float directionTimer;
        private float bombTimer;
        private Vector2 currentDirection;

        private void Awake()
        {
            character = GetComponent<CharacterBase>();
            PickRandomDirection();
        }

        private void Update()
        {
            directionTimer -= Time.deltaTime;
            bombTimer -= Time.deltaTime;

            if (directionTimer <= 0f)
            {
                PickRandomDirection();
                directionTimer = changeDirectionInterval;
            }

            character.SetMoveInput(currentDirection);

            if (bombTimer <= 0f)
            {
                bombTimer = bombDecisionInterval;
                if (Random.value < bombChancePerCheck)
                {
                    character.TryPlaceBomb();
                }
            }
        }

        private void PickRandomDirection()
        {
            Vector2Int random = GameConstants.CardinalDirections[Random.Range(0, GameConstants.CardinalDirections.Length)];
            currentDirection = random;
        }
    }
}
