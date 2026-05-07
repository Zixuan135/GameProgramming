using BubbleTown.Core;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Player-facing character definition used by the character select flow.
    /// The prefab is currently treated as a replaceable visual prefab that sits under
    /// the role controller in Battle, so input and AI logic stay role-specific.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData_NewCharacter", menuName = "BubbleTown/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string characterId = "new_character";
        [SerializeField] private string displayName = "New Character";

        [Header("Presentation")]
        [SerializeField] private GameObject prefab = null;
        [SerializeField] private Sprite icon = null;
        [SerializeField] private Color themeColor = new Color(0.1f, 0.72f, 1f, 1f);

        [Header("Base Stats")]
        [SerializeField, Min(1f)] private float moveSpeed = GameConstants.DefaultMoveSpeed;
        [SerializeField, Min(1)] private int maxBombCount = GameConstants.DefaultBombCount;
        [SerializeField, Min(1)] private int explosionRange = GameConstants.DefaultExplosionRange;

        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public float MoveSpeed => Mathf.Max(1f, moveSpeed);
        public int MaxBombCount => Mathf.Max(1, maxBombCount);
        public int ExplosionRange => Mathf.Max(1, explosionRange);
        public Color ThemeColor => themeColor;

        private void OnValidate()
        {
            characterId = string.IsNullOrWhiteSpace(characterId)
                ? name.ToLowerInvariant().Replace(" ", "_")
                : characterId.Trim();

            displayName = string.IsNullOrWhiteSpace(displayName)
                ? NicifyId(characterId)
                : displayName.Trim();

            moveSpeed = Mathf.Max(1f, moveSpeed);
            maxBombCount = Mathf.Max(1, maxBombCount);
            explosionRange = Mathf.Max(1, explosionRange);
        }

        private static string NicifyId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "New Character";
            }

            string[] words = id.Replace('-', '_').Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                if (string.IsNullOrEmpty(words[i]))
                {
                    continue;
                }

                words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
            }

            return string.Join(" ", words);
        }
    }
}
