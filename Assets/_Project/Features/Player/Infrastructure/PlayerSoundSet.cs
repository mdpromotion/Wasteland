using UnityEngine;
using _Project.Features.Sound.Domain;

namespace _Project.Features.Player.Infrastructure
{
    [CreateAssetMenu(menuName = "Project/Player/Player Sound Set", fileName = "PlayerSoundSet")]
    public sealed class PlayerSoundSet : ScriptableObject
    {
        [SerializeField] private SoundKey[] _walkFootsteps;
        [SerializeField] private SoundKey[] _runFootsteps;
        [SerializeField] private SoundKey[] _jumpStart;
        [SerializeField] private SoundKey[] _jumpLand;
        [SerializeField] private SoundKey _waterEnter;
        [SerializeField] private SoundKey _waterExit;
        [SerializeField] private SoundKey _treeChop;
        [SerializeField] private SoundKey _treeFall;
        [SerializeField, Min(0.01f)] private float _waterSoundMaxVelocity = 10f;
        
        public SoundKey[] WalkFootsteps => _walkFootsteps;
        public SoundKey[] RunFootsteps => _runFootsteps;
        public SoundKey[] JumpStart => _jumpStart;
        public SoundKey[] JumpLand => _jumpLand;
        public SoundKey WaterEnter => _waterEnter;
        public SoundKey WaterExit => _waterExit;
        public SoundKey TreeChop => _treeChop;
        public SoundKey TreeFall => _treeFall;
        public float WaterSoundMaxVelocity => _waterSoundMaxVelocity;
    }
}