using UnityEngine;

namespace _Project.Features.Interaction.Infrastructure
{
    public interface IHitHandler
    {
        bool CanHandle(RaycastHit hit);
        
        void Handle(RaycastHit hit, float damage);
    }
}