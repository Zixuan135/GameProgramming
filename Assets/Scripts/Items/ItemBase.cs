using System;
using BubbleTown.Characters;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using BubbleTown.Map;
using UnityEngine;

namespace BubbleTown.Items
{
    /// <summary>
    /// Base class for simple stat items.
    /// The first version keeps pickup behavior lightweight and exposes reusable hooks for later visuals/SFX/UI.
    /// </summary>
    public class ItemBase : MonoBehaviour
    {
        public static event Action<CharacterBase, ItemBase> ItemPickedUp;

        [Header("Item")]
        [SerializeField] protected ItemType itemType = ItemType.None;
        [SerializeField] protected bool pickupOnTrigger = true;
        [SerializeField] protected bool destroyAfterPickup = true;

        [Header("Effect Values")]
        [SerializeField, Min(0)] protected int bombCountDelta = GameConstants.DefaultItemBombCountDelta;
        [SerializeField, Min(0)] protected int explosionRangeDelta = GameConstants.DefaultItemExplosionRangeDelta;
        [SerializeField, Min(0f)] protected float moveSpeedDelta = GameConstants.DefaultItemMoveSpeedDelta;
        [SerializeField, Min(0)] protected int shieldChargesDelta = GameConstants.DefaultItemShieldChargesDelta;
        [SerializeField, Min(0f)] protected float invincibleSeconds = GameConstants.DefaultItemInvincibleSeconds;

        [Header("Grid State")]
        [SerializeField] protected MapManager mapManager;
        [SerializeField] protected Vector2Int gridPosition;
        [SerializeField] protected bool hasGridPosition;
        [SerializeField] protected bool clearMapItemOnDestroy = true;

        [Header("Pickup Feedback")]
        [SerializeField] protected ItemPickupFeedback pickupFeedback;
        [SerializeField] protected bool notifyPickupFeedback = true;
        [SerializeField] protected bool notifyCharacterFeedback = true;

        private bool pickedUp;

        public ItemType ItemType => itemType;
        public int BombCountDelta => bombCountDelta;
        public int ExplosionRangeDelta => explosionRangeDelta;
        public float MoveSpeedDelta => moveSpeedDelta;
        public int ShieldChargesDelta => shieldChargesDelta;
        public float InvincibleSeconds => invincibleSeconds;
        public Vector2Int GridPosition => gridPosition;
        public bool HasGridPosition => hasGridPosition;
        public bool IsPickedUp => pickedUp;

        public virtual void Initialize(ItemType newItemType)
        {
            itemType = newItemType;
            ClearGridPositionState();
        }

        public virtual void Initialize(ItemType newItemType, MapManager ownerMapManager, Vector2Int itemGridPosition)
        {
            itemType = newItemType;

            if (ownerMapManager == null)
            {
                ClearGridPositionState();
                return;
            }

            SetGridPosition(ownerMapManager, itemGridPosition);
        }

        public virtual void SetGridPosition(MapManager ownerMapManager, Vector2Int itemGridPosition)
        {
            mapManager = ownerMapManager;
            gridPosition = itemGridPosition;
            hasGridPosition = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryPickupFromCollider(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryPickupFromCollider(other);
        }

        private void TryPickupFromCollider(Collider other)
        {
            if (!pickupOnTrigger)
            {
                return;
            }

            CharacterBase character = other.GetComponentInParent<CharacterBase>();
            if (character == null)
            {
                return;
            }

            TryPickup(character);
        }

        public virtual bool TryPickup(CharacterBase character)
        {
            if (pickedUp || character == null || !character.IsAlive)
            {
                return false;
            }

            if (!ApplyTo(character))
            {
                return false;
            }

            pickedUp = true;
            OnPickedUp(character);
            return true;
        }

        public virtual bool ApplyTo(CharacterBase character)
        {
            if (character == null)
            {
                return false;
            }

            bool applied = character.ApplyItemEffect(
                itemType,
                bombCountDelta,
                explosionRangeDelta,
                moveSpeedDelta,
                shieldChargesDelta,
                invincibleSeconds);

            if (!applied)
            {
                Debug.LogWarning($"[ItemBase] {name} failed to apply item effect: {itemType}.");
            }

            return applied;
        }

        protected virtual void OnPickedUp(CharacterBase character)
        {
            ClearMapItemState();
            Debug.Log($"[ItemBase] {character.name} picked up {itemType}.");
            AudioManager.Instance?.PlayItemPickupSFX();
            ItemPickedUp?.Invoke(character, this);
            PlayCharacterPickupFeedback(character);

            if (destroyAfterPickup)
            {
                PlayItemPickupFeedback(character, DestroySelf);
            }
        }

        protected virtual void PlayItemPickupFeedback(CharacterBase character, Action onComplete)
        {
            if (!notifyPickupFeedback)
            {
                onComplete?.Invoke();
                return;
            }

            if (pickupFeedback == null)
            {
                pickupFeedback = GetComponent<ItemPickupFeedback>();
            }

            if (pickupFeedback == null)
            {
                onComplete?.Invoke();
                return;
            }

            pickupFeedback.PlayPickupFeedback(character, itemType, onComplete);
        }

        protected virtual void PlayCharacterPickupFeedback(CharacterBase character)
        {
            if (!notifyCharacterFeedback || character == null)
            {
                return;
            }

            CharacterPickupFeedback characterFeedback = character.GetComponentInChildren<CharacterPickupFeedback>();
            if (characterFeedback == null)
            {
                characterFeedback = character.gameObject.AddComponent<CharacterPickupFeedback>();
            }

            characterFeedback.PlayPickupFlash(itemType);
        }

        protected virtual void DestroySelf()
        {
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (clearMapItemOnDestroy)
            {
                ClearMapItemState();
            }
        }

        protected virtual void ClearMapItemState()
        {
            if (!hasGridPosition || mapManager == null)
            {
                return;
            }

            mapManager.SetItem(gridPosition, false);
            hasGridPosition = false;
        }

        protected virtual void ClearGridPositionState()
        {
            mapManager = null;
            gridPosition = Vector2Int.zero;
            hasGridPosition = false;
        }
    }
}
