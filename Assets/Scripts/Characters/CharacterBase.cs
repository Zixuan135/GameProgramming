using System;
using UnityEngine;

namespace BubbleTown
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterBase : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private CharacterType characterType = CharacterType.Player;
        [SerializeField] private string characterLabel = "Character";

        [Header("Stats")]
        [SerializeField] private float moveSpeed = GameConstants.DefaultMoveSpeed;
        [SerializeField] private int maxBombCount = GameConstants.DefaultMaxBombCount;
        [SerializeField] private int bombRange = GameConstants.DefaultExplosionRange;

        [Header("Bomb")]
        [SerializeField] private BombController bombPrefab;
        [SerializeField] private Transform bombSpawnPivot;

        public CharacterType CharacterType => characterType;
        public string CharacterLabel
        {
            get => characterLabel;
            set => characterLabel = value;
        }

        public float MoveSpeed => moveSpeed;
        public int MaxBombCount => maxBombCount;
        public int BombRange => bombRange;

        public event Action<CharacterBase> OnCharacterDied;

        private Rigidbody2D rb2D;
        private Vector2 moveInput;
        private int activeBombCount;
        private bool isDead;

        protected virtual void Awake()
        {
            rb2D = GetComponent<Rigidbody2D>();
        }

        protected virtual void Update()
        {
            if (isDead)
            {
                return;
            }

            HandleUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (isDead)
            {
                rb2D.velocity = Vector2.zero;
                return;
            }

            rb2D.velocity = moveInput * moveSpeed;
        }

        public void InitializeAs(CharacterType type)
        {
            characterType = type;
        }

        public void SetMoveInput(Vector2 input)
        {
            moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public bool TryPlaceBomb()
        {
            if (bombPrefab == null || activeBombCount >= maxBombCount)
            {
                return false;
            }

            MapManager mapManager = FindObjectOfType<MapManager>();
            Vector3 basePosition = bombSpawnPivot != null ? bombSpawnPivot.position : transform.position;
            Vector3 spawnPos = mapManager != null ? mapManager.SnapToGrid(basePosition) : basePosition;

            BombController bomb = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
            bomb.Initialize(this, bombRange);
            bomb.OnBombExploded += HandleOwnedBombExploded;

            activeBombCount++;
            return true;
        }

        public void TakeDamage()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            rb2D.velocity = Vector2.zero;
            OnCharacterDied?.Invoke(this);
            Destroy(gameObject, 0.05f);
        }

        public void AddMoveSpeed(float value)
        {
            moveSpeed = Mathf.Max(1f, moveSpeed + value);
        }

        public void AddBombCount(int value)
        {
            maxBombCount = Mathf.Max(1, maxBombCount + value);
        }

        public void AddBombRange(int value)
        {
            bombRange = Mathf.Max(1, bombRange + value);
        }

        protected virtual void HandleUpdate()
        {
        }

        private void HandleOwnedBombExploded(BombController bomb)
        {
            bomb.OnBombExploded -= HandleOwnedBombExploded;
            activeBombCount = Mathf.Max(0, activeBombCount - 1);
        }
    }
}
