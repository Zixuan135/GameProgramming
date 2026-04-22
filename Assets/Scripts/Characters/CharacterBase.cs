using BubbleTown.Core;
using BubbleTown.Gameplay;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Base stats and shared behavior for player and AI characters.
    /// </summary>
    public class CharacterBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected float moveSpeed = GameConstants.DefaultMoveSpeed;
        [SerializeField] protected int maxBombCount = GameConstants.DefaultBombCount;
        [SerializeField] protected int bombRange = GameConstants.DefaultBombRange;

        [Header("Bomb")]
        [SerializeField] protected BombController bombPrefab;
        [SerializeField] protected Transform bombSpawnRoot;

        protected int activeBombCount;

        public float MoveSpeed => moveSpeed;
        public int MaxBombCount => maxBombCount;
        public int BombRange => bombRange;

        protected virtual void Awake()
        {
            activeBombCount = 0;
        }

        public virtual void Move(Vector3 worldDirection)
        {
            Vector3 delta = worldDirection.normalized * moveSpeed * Time.deltaTime;
            transform.position += delta;
        }

        public virtual bool TryPlaceBomb()
        {
            if (bombPrefab == null || activeBombCount >= maxBombCount)
            {
                return false;
            }

            Transform root = bombSpawnRoot == null ? transform : bombSpawnRoot;
            BombController bomb = Instantiate(bombPrefab, root.position, Quaternion.identity);
            bomb.Initialize(this, bombRange);
            activeBombCount++;
            return true;
        }

        public virtual void OnBombExploded(BombController bomb)
        {
            activeBombCount = Mathf.Max(0, activeBombCount - 1);
        }

        public virtual void ApplyMoveSpeedModifier(float delta)
        {
            moveSpeed = Mathf.Max(1f, moveSpeed + delta);
        }

        public virtual void ApplyBombCountModifier(int delta)
        {
            maxBombCount = Mathf.Max(1, maxBombCount + delta);
        }

        public virtual void ApplyBombRangeModifier(int delta)
        {
            bombRange = Mathf.Max(1, bombRange + delta);
        }

        public virtual void OnHitByExplosion()
        {
            Debug.Log($"[CharacterBase] {name} was hit by explosion.");
            // TODO: Hook HP/life and battle result flow.
        }
    }
}
