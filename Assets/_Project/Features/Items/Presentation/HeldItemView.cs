using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Passive view hosting the visual of the item currently held by the player.
    /// Owns transform bookkeeping (attach/detach) and drives the equip motion;
    /// presenters only issue commands and never touch transforms directly.
    /// </summary>
    /// <remarks>
    /// The view is enabled only while an equip motion is running, so the idle
    /// state costs no per-frame script calls; the camera parenting itself moves
    /// the held item natively.
    /// </remarks>
    public sealed class HeldItemView : MonoBehaviour
    {
        private HotbarHudSettings _settings;
        private HeldItemConfig _visualConfig;
        private GameObject _visual;
        private float _equipProgress;

        /// <summary>Visual handed over by the presenter/factory, or null when hidden.</summary>
        public GameObject Visual => _visual;

        public void Construct(HotbarHudSettings settings)
        {
            _settings = settings != null ? settings : throw new System.ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Attaches the given factory-owned visual under the view and starts the
        /// equip motion. <paramref name="config"/> supplies the held pose; null
        /// uses the global defaults. Null <paramref name="visual"/> hides the item.
        /// </summary>
        public void Show(GameObject visual, HeldItemConfig config)
        {
            DetachVisual();
            if (visual == null)
            {
                Hide();
                return;
            }

            _visual = visual;
            _visualConfig = config;
            Transform visualTransform = visual.transform;
            visualTransform.SetParent(transform, false);
            visualTransform.localPosition = _settings.StowedItemPosition;
            visualTransform.localScale = ResolveScale(config);
            visual.SetActive(true);

            _equipProgress = 0f;
            enabled = true;
        }

        private Vector3 ResolveScale(HeldItemConfig config)
        {
            return config != null ? config.HeldScale : _settings.HeldItemScale;
        }

        /// <summary>Detaches the current visual back to its factory and stops animating.</summary>
        public void Hide()
        {
            DetachVisual();
            enabled = false;
        }

        private void Update()
        {
            _equipProgress = HeldItemEquipMotion.AdvanceProgress(
                _equipProgress, Time.deltaTime, _settings.EquipDuration);
            float easedProgress = HeldItemEquipMotion.Ease(_equipProgress);

            Transform visualTransform = _visual != null ? _visual.transform : null;
            if (visualTransform == null)
            {
                enabled = false;
                return;
            }

            visualTransform.localPosition = HeldItemEquipMotion.EvaluatePosition(
                _settings.StowedItemPosition, ResolvePosition(), easedProgress);
            visualTransform.localRotation = HeldItemEquipMotion.EvaluateRotation(
                ResolveRotation(), easedProgress);

            if (_equipProgress >= 1f)
                enabled = false;
        }

        private Vector3 ResolvePosition()
        {
            return _visualConfig != null ? _visualConfig.HeldPosition : _settings.HeldItemPosition;
        }

        private Vector3 ResolveRotation()
        {
            return _visualConfig != null ? _visualConfig.HeldRotation : _settings.HeldItemRotation;
        }

        private void DetachVisual()
        {
            if (_visual == null)
                return;

            _visual.transform.SetParent(null, false);
            _visual.SetActive(false);
            _visual = null;
        }
    }
}
