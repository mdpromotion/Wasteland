namespace _Project.Features.ProceduralWorld.Domain.Vegetation
{
    /// <summary>
    /// Maps the damage dealt to a breakable tree by a single hit
    /// onto the number of impact (wood chip) particles to show.
    /// Pure stateless rule — kept in Domain so both gameplay feedback
    /// and tests can rely on it without Unity scene objects.
    /// </summary>
    public static class TreeImpactParticlesPolicy
    {
        public const int MinParticles = 1;
        public const int MaxParticles = 24;
        public const int ParticlesPerDamagePoint = 8;

        public static int GetParticleCount(float damage)
        {
            if (damage <= 0f)
                return MinParticles;

            float scaled = damage * ParticlesPerDamagePoint;

            // Clamp as float BEFORE the int cast: an int cast of a value beyond
            // int range (or of +Infinity) is undefined and can yield int.MinValue.
            if (scaled >= MaxParticles)
                return MaxParticles;

            if (scaled <= MinParticles)
                return MinParticles;

            return (int)(scaled + 0.5f);
        }
    }
}
