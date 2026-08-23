using UnityEngine;

namespace _Project.Features.Core.Infrastructure
{
    [CreateAssetMenu(fileName = "FrameBudgetConfig", menuName = "Project/Frame Budget Config")]
    public sealed class FrameBudgetConfig : ScriptableObject
    {
        [Min(1f)]
        [SerializeField] private float lowFpsThreshold = 30f;
        
        [Min(1f)]
        [SerializeField] private float highFpsThreshold = 60f;
        
        [Min(0)]
        [SerializeField] private int minOperationsPerFrame = 0;
        
        [Min(0)]
        [SerializeField] private int maxOperationsPerFrame = 4;
        
        [Range(0.001f, 1f)]
        [SerializeField] private float fpsSmoothingDown = 0.5f;
        
        [Range(0.001f, 1f)]
        [SerializeField] private float fpsSmoothingUp = 0.05f;
        
        [Range(0.1f, 0.95f)]
        [SerializeField] private float lagDropRatio = 0.6f;


        public float LowFpsThreshold =>
            lowFpsThreshold;

        public float HighFpsThreshold =>
            highFpsThreshold;

        public int MinOperationsPerFrame =>
            minOperationsPerFrame;

        public int MaxOperationsPerFrame =>
            maxOperationsPerFrame;

        public float FpsSmoothingDown =>
            fpsSmoothingDown;

        public float FpsSmoothingUp =>
            fpsSmoothingUp;

        public float LagDropRatio =>
            lagDropRatio;
    }
}