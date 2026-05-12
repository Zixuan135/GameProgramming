using System;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Loads character definitions from Resources/Characters so the first version of
    /// character select works without manual scene references.
    /// </summary>
    public static class CharacterRoster
    {
        private const string ResourcesPath = "Characters";

        private static CharacterData[] cachedCharacters;

        public static CharacterData[] Characters
        {
            get
            {
                if (cachedCharacters == null || cachedCharacters.Length == 0)
                {
                    cachedCharacters = Resources.LoadAll<CharacterData>(ResourcesPath);
                    Array.Sort(cachedCharacters, CompareCharactersByName);
                }

                return cachedCharacters;
            }
        }

        public static bool HasCharacters => Characters.Length > 0;

        /// <summary>
        /// Purpose: Gets default character.
        /// Inputs: `preferredIndex`; may also read serialized fields and current runtime state.
        /// Output: a `CharacterData` value.
        /// </summary>
        /// <param name="preferredIndex">Input value used by this method.</param>
        /// <returns>a `CharacterData` value.</returns>
        public static CharacterData GetDefaultCharacter(int preferredIndex = 0)
        {
            CharacterData[] characters = Characters;
            if (characters.Length == 0)
            {
                return null;
            }

            int safeIndex = Mathf.Clamp(preferredIndex, 0, characters.Length - 1);
            return characters[safeIndex];
        }

        /// <summary>
        /// Purpose: Finds by id from scene objects or cached data.
        /// Inputs: `characterId`; may also read serialized fields and current runtime state.
        /// Output: a `CharacterData` value.
        /// </summary>
        /// <param name="characterId">Input value used by this method.</param>
        /// <returns>a `CharacterData` value.</returns>
        public static CharacterData FindById(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            CharacterData[] characters = Characters;
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterData character = characters[i];
                if (character != null &&
                    string.Equals(character.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return character;
                }
            }

            return null;
        }

        /// <summary>
        /// Purpose: Gets next different.
        /// Inputs: `current`; may also read serialized fields and current runtime state.
        /// Output: a `CharacterData` value.
        /// </summary>
        /// <param name="current">Input value used by this method.</param>
        /// <returns>a `CharacterData` value.</returns>
        public static CharacterData GetNextDifferent(CharacterData current)
        {
            CharacterData[] characters = Characters;
            if (characters.Length == 0)
            {
                return null;
            }

            if (current == null || characters.Length == 1)
            {
                return characters[0];
            }

            int currentIndex = Array.IndexOf(characters, current);
            for (int offset = 1; offset <= characters.Length; offset++)
            {
                CharacterData candidate = characters[(Mathf.Max(0, currentIndex) + offset) % characters.Length];
                if (candidate != null && candidate != current)
                {
                    return candidate;
                }
            }

            return characters[0];
        }

        /// <summary>
        /// Purpose: Gets random different.
        /// Inputs: `excluded`; may also read serialized fields and current runtime state.
        /// Output: a `CharacterData` value.
        /// </summary>
        /// <param name="excluded">Input value used by this method.</param>
        /// <returns>a `CharacterData` value.</returns>
        public static CharacterData GetRandomDifferent(CharacterData excluded)
        {
            CharacterData[] characters = Characters;
            if (characters.Length == 0)
            {
                return null;
            }

            if (characters.Length == 1 || excluded == null)
            {
                return characters[UnityEngine.Random.Range(0, characters.Length)];
            }

            CharacterData[] candidates = new CharacterData[characters.Length - 1];
            int candidateCount = 0;
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null || characters[i] == excluded)
                {
                    continue;
                }

                candidates[candidateCount] = characters[i];
                candidateCount++;
            }

            if (candidateCount == 0)
            {
                return excluded;
            }

            return candidates[UnityEngine.Random.Range(0, candidateCount)];
        }

        /// <summary>
        /// Purpose: Clears cache.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public static void ClearCache()
        {
            cachedCharacters = null;
        }

        /// <summary>
        /// Purpose: Returns compare characters by name for the current state.
        /// Inputs: `left`, `right`; may also read serialized fields and current runtime state.
        /// Output: a `int` value.
        /// </summary>
        /// <param name="left">Input value used by this method.</param>
        /// <param name="right">Input value used by this method.</param>
        /// <returns>a `int` value.</returns>
        private static int CompareCharactersByName(CharacterData left, CharacterData right)
        {
            int leftPriority = GetCharacterPriority(left);
            int rightPriority = GetCharacterPriority(right);
            if (leftPriority != rightPriority)
            {
                return leftPriority.CompareTo(rightPriority);
            }

            string leftName = left != null ? left.DisplayName : string.Empty;
            string rightName = right != null ? right.DisplayName : string.Empty;
            return string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Purpose: Gets character priority.
        /// Inputs: `character`; may also read serialized fields and current runtime state.
        /// Output: a `int` value.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <returns>a `int` value.</returns>
        private static int GetCharacterPriority(CharacterData character)
        {
            switch (character != null ? character.CharacterId : string.Empty)
            {
                case "bubble_ranger":
                    return 0;
                case "bear_blaster":
                    return 1;
                case "frog_hopper":
                    return 2;
                case "gear_kid":
                    return 3;
                case "bunny_pop":
                    return 4;
                case "star_mage":
                    return 5;
                default:
                    return 100;
            }
        }
    }
}
