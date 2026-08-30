using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.UI.Menus.MainMenu.World.View
{
    [RequireComponent(typeof(InputField))]
    public class SeedFieldView : MonoBehaviour
    {
        private InputField _seedField;

        private void Awake()
        {
            _seedField = GetComponent<InputField>();
        }
        
        public bool TryGetSeed(out int seed)
        {
            string text = _seedField.text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                seed = 0;
                return false;
            }

            seed = GetSeedFromString(text);

            if (seed == 0)
                seed = 1;

            return true;
        }

        private static int GetSeedFromString(string text)
        {
            unchecked
            {
                uint hash = 2166136261;

                foreach (char c in text)
                {
                    hash ^= c;
                    hash *= 16777619;
                }

                return (int)hash;
            }
        }
    }
}