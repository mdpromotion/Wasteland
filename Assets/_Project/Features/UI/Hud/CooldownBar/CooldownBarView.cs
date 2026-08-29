using UnityEngine;
using UnityEngine.UI;

namespace _Project.Features.UI.Hud.CooldownBar
{
    [RequireComponent(typeof(Image))]
    public class CooldownBarView : MonoBehaviour
    {
        private Image _image;
        
        private void Awake() => _image = GetComponent<Image>();
        
        public void Toggle(bool state)
        {
            _image.gameObject.SetActive(state);
        }
        
        public void SetFill(float fillAmount)
        {
            _image.fillAmount = fillAmount;
        }
    }
}
