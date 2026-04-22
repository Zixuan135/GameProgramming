using BubbleTown.Characters;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Items
{
    /// <summary>
    /// Base item pickup behavior.
    /// Applies simple stat modifiers to character on trigger.
    /// </summary>
    public class ItemBase : MonoBehaviour
    {
        [SerializeField] protected ItemType itemType = ItemType.None;
        [SerializeField] protected float moveSpeedDelta = 0.5f;
        [SerializeField] protected int bombCountDelta = 1;
        [SerializeField] protected int bombRangeDelta = 1;

        public ItemType ItemType => itemType;

        private void OnTriggerEnter(Collider other)
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (character == null)
            {
                return;
            }

            ApplyTo(character);
            Destroy(gameObject);
        }

        public virtual void ApplyTo(CharacterBase character)
        {
            switch (itemType)
            {
                case ItemType.BombCountUp:
                    character.ApplyBombCountModifier(bombCountDelta);
                    break;
                case ItemType.BombRangeUp:
                    character.ApplyBombRangeModifier(bombRangeDelta);
                    break;
                case ItemType.MoveSpeedUp:
                    character.ApplyMoveSpeedModifier(moveSpeedDelta);
                    break;
            }
        }
    }
}
