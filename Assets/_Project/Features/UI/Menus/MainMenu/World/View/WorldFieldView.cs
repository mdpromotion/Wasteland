using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.UI.Menus.MainMenu.World.View
{
    [RequireComponent(typeof(InputField))]
    public class WorldFieldView : MonoBehaviour
    {
        private InputField _worldField;
        
        private void Awake()
        {
            _worldField = GetComponent<InputField>();
        }

        public void SetName(string value)
            => _worldField.text = value;

        public bool TryGetName(out string value)
        {
            value = string.Empty;
            
            if (string.IsNullOrEmpty(_worldField.text))
                return false;
            
            value = _worldField.text;
            return true;
        }
    }
}