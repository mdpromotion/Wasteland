using System;
using UnityEngine;

namespace _Project.Features.GameTime.Infrastructure
{
    [Serializable]
    public struct GameTimeTransition
    {
        [Range(0f, 24f)]
        public float Hour;

        public Color FogColor;
    }

    [CreateAssetMenu(menuName = "GameTime/GameTimePresenterSceneConfig")]
    public class GameTimePresenterSceneConfig : ScriptableObject
    {
        [SerializeField]
        private float _transitionDurationHours = 1f;

        [SerializeField]
        private float _sunRotationOffset;

        [SerializeField]
        private GameTimeTransition _dayTransition;

        [SerializeField]
        private GameTimeTransition _nightTransition;

        public float TransitionDurationHours => _transitionDurationHours;
        public float SunRotationOffset => _sunRotationOffset;

        public GameTimeTransition DayTransition => _dayTransition;
        public GameTimeTransition NightTransition => _nightTransition;
        
#if UNITY_INCLUDE_TESTS
        public void SetForTest(float sunRotationOffset, float transitionDurationHours, float dayHour, float nightHour)
        {
            _sunRotationOffset = sunRotationOffset;
            _transitionDurationHours = transitionDurationHours;
            _dayTransition = new GameTimeTransition { Hour = dayHour };
            _nightTransition = new GameTimeTransition { Hour = nightHour };
        }
#endif
    }
}