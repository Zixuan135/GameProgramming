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

        public static void ClearCache()
        {
            cachedCharacters = null;
        }

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
