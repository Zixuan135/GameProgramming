using UnityEngine;

namespace BubbleTown
{
    [RequireComponent(typeof(CharacterBase))]
    public class PlayerController : MonoBehaviour
    {
        public ControlScheme ControlScheme => controlScheme;

        [SerializeField] private ControlScheme controlScheme = ControlScheme.PlayerOne;

        private CharacterBase character;

        private void Awake()
        {
            character = GetComponent<CharacterBase>();
        }

        private void Update()
        {
            character.SetMoveInput(ReadMoveInput());

            if (ReadBombInputDown())
            {
                character.TryPlaceBomb();
            }
        }

        public void AssignControlScheme(ControlScheme scheme)
        {
            controlScheme = scheme;
        }

        private Vector2 ReadMoveInput()
        {
            if (controlScheme == ControlScheme.PlayerOne)
            {
                float x = 0f;
                float y = 0f;

                if (Input.GetKey(KeyCode.A)) x -= 1f;
                if (Input.GetKey(KeyCode.D)) x += 1f;
                if (Input.GetKey(KeyCode.W)) y += 1f;
                if (Input.GetKey(KeyCode.S)) y -= 1f;
                return new Vector2(x, y).normalized;
            }
            else
            {
                float x = 0f;
                float y = 0f;

                if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (Input.GetKey(KeyCode.UpArrow)) y += 1f;
                if (Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                return new Vector2(x, y).normalized;
            }
        }

        private bool ReadBombInputDown()
        {
            if (controlScheme == ControlScheme.PlayerOne)
            {
                return Input.GetKeyDown(KeyCode.Space);
            }

            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightControl);
        }
    }
}
