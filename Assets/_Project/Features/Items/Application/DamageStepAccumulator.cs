using UnityEngine;

namespace _Project.Features.Items.Application
{
    public sealed class DamageStepAccumulator
    {
        private float _remainder;

        public int Add(float damage, float step)
        {
            if (damage <= 0f || step <= 0f) return 0;
            float accumulated = _remainder + damage;
            int earnedSteps = Mathf.FloorToInt(accumulated / step);
            _remainder = accumulated - earnedSteps * step;
            return earnedSteps;
        }
    }
}
