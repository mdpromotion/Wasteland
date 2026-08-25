namespace _Project.Features.ProceduralWorld.Domain.World
{
    /// <summary>
    /// Represents an absolute world-space position with double precision on the
    /// horizontal axes.
    /// </summary>
    /// <remarks>
    /// X and Z use double precision to retain accuracy across large world distances,
    /// while Y remains float because vertical coordinates do not require the same
    /// range for world-space rebasing.
    /// </remarks>
    public readonly struct WorldPosition
    {
        /// <summary>
        /// Absolute world-space X coordinate.
        /// </summary>
        public readonly double X;
        
        /// <summary>
        /// World-space Y coordinate.
        /// </summary>
        public readonly float Y;
        
        /// <summary>
        /// Absolute world-space Z coordinate.
        /// </summary>
        public readonly double Z;

        public WorldPosition(double x, float y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}