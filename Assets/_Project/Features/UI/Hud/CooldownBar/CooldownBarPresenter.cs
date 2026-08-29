using _Project.Features.Interaction.Infrastructure;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Project.Features.UI.Hud.CooldownBar
{
    public class CooldownBarPresenter : MonoBehaviour
    {
        [SerializeField] private CooldownBarView cooldownBarView;

        private IPlayerInteractionController _playerInteractionController;
        private Tween _fillTween;

        [Inject]
        public void Construct(IPlayerInteractionController playerInteractionController)
        {
            _playerInteractionController = playerInteractionController;
            _playerInteractionController.PlayerInteracted += OnPlayerInteracted;
        }

        private void Start()
        {
            cooldownBarView.Toggle(false);
        }

        private void OnPlayerInteracted(float interval)
        {
            _fillTween?.Kill();

            cooldownBarView.Toggle(true);
            cooldownBarView.SetFill(1f);

            _fillTween = DOTween.To(
                    () => 1f,
                    value => cooldownBarView.SetFill(value),
                    0f,
                    interval)
                .SetEase(Ease.Linear)
                .OnComplete(() => cooldownBarView.Toggle(false));
        }

        private void OnDestroy()
        {
            _playerInteractionController.PlayerInteracted -= OnPlayerInteracted;
            _fillTween?.Kill();
        }
    }
}