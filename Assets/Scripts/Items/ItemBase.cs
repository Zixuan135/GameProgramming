using UnityEngine;

namespace BubbleTown
{
    public class ItemBase : MonoBehaviour
    {
        [SerializeField] private ItemType itemType = ItemType.BombCount;
        [SerializeField] private float moveSpeedBonus = 0.5f;
        [SerializeField] private int bombCountBonus = 1;
        [SerializeField] private int bombRangeBonus = 1;

        private void OnTriggerEnter2D(Collider2D other)
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (character == null)
            {
                return;
            }

            ApplyTo(character);
            Destroy(gameObject);
        }

        public void ApplyTo(CharacterBase character)
        {
            switch (itemType)
            {
                case ItemType.BombCount:
                    character.AddBombCount(bombCountBonus);
                    break;
                case ItemType.BombRange:
                    character.AddBombRange(bombRangeBonus);
                    break;
                case ItemType.MoveSpeed:
                    character.AddMoveSpeed(moveSpeedBonus);
                    break;
            }
        }
    }
}
