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

            int count = RoundToInt(damage * ParticlesPerDamagePoint);
            return Clamp(count, MinParticles, MaxParticles);
        }

        private static int RoundToInt(float value) =>
            value < 0f
                ? -(int)(-value + 0.5f)
                : (int)(value + 0.5f);

        private static int Clamp(int value, int min, int max) =>
            value < min ? min : (value > max ? max : value);
    }
}
