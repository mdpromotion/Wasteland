using _Project.Features.ProceduralWorld.Domain.Chunks;
using _Project.Features.ProceduralWorld.Infrastructure.Interfaces;
using UnityEngine;

namespace _Project.Features.ProceduralWorld.Application.Chunks
{
    /// <summary>
    /// Maintains Unity Terrain neighbour links for a chunk and its adjacent chunks.
    /// </summary>
    /// <remarks>
    /// When a chunk is connected or disconnected, the neighbouring terrains are also
    /// updated so that every affected Terrain has a consistent four-direction neighbour
    /// configuration.
    /// </remarks>
    public sealed class ChunkNeighborConnector
    {
        private readonly ILandscapeFactory _factory;
        
        public ChunkNeighborConnector(ILandscapeFactory factory)
        {
            _factory = factory;
        }
        
        /// <summary>
        /// Connects the specified chunk to all currently loaded cardinal neighbours and
        /// refreshes the neighbour links of those adjacent terrains.
        /// </summary>
        /// <param name="chunks">Lookup containing currently loaded chunks.</param>
        /// <param name="coordinate">Coordinate of the chunk being connected.</param>
        public void Connect(IChunkLookup chunks, ChunkCoordinate coordinate)
        {
            Terrain self = Get(chunks, coordinate);

            Terrain left = Get(chunks, coordinate, -1, 0);
            Terrain top = Get(chunks, coordinate, 0, 1);
            Terrain right = Get(chunks, coordinate, 1, 0);
            Terrain bottom = Get(chunks, coordinate, 0, -1);


            _factory.Connect(
                self,
                left,
                top,
                right,
                bottom);


            if (left != null)
            {
                _factory.Connect(
                    left,
                    Get(chunks, coordinate, -2, 0),
                    Get(chunks, coordinate, -1, 1),
                    self,
                    Get(chunks, coordinate, -1, -1));
            }


            if (top != null)
            {
                _factory.Connect(
                    top,
                    Get(chunks, coordinate, -1, 1),
                    Get(chunks, coordinate, 0, 2),
                    Get(chunks, coordinate, 1, 1),
                    self);
            }


            if (right != null)
            {
                _factory.Connect(
                    right,
                    self,
                    Get(chunks, coordinate, 1, 1),
                    Get(chunks, coordinate, 2, 0),
                    Get(chunks, coordinate, 1, -1));
            }


            if (bottom != null)
            {
                _factory.Connect(
                    bottom,
                    Get(chunks, coordinate, -1, -1),
                    self,
                    Get(chunks, coordinate, 1, -1),
                    Get(chunks, coordinate, 0, -2));
            }
        }

        /// <summary>
        /// Removes the specified chunk from the neighbor configuration of its adjacent
        /// loaded terrains.
        /// </summary>
        public void Disconnect(IChunkLookup chunks, ChunkCoordinate coordinate)
        {
            Terrain left = Get(chunks, coordinate, -1, 0);
            Terrain top = Get(chunks, coordinate, 0, 1);
            Terrain right = Get(chunks, coordinate, 1, 0);
            Terrain bottom = Get(chunks, coordinate, 0, -1);


            if (left != null)
            {
                _factory.Connect(
                    left,
                    Get(chunks, coordinate, -2, 0),
                    Get(chunks, coordinate, -1, 1),
                    null,
                    Get(chunks, coordinate, -1, -1));
            }


            if (top != null)
            {
                _factory.Connect(
                    top,
                    Get(chunks, coordinate, -1, 1),
                    Get(chunks, coordinate, 0, 2),
                    Get(chunks, coordinate, 1, 1),
                    null);
            }


            if (right != null)
            {
                _factory.Connect(
                    right,
                    null,
                    Get(chunks, coordinate, 1, 1),
                    Get(chunks, coordinate, 2, 0),
                    Get(chunks, coordinate, 1, -1));
            }


            if (bottom != null)
            {
                _factory.Connect(
                    bottom,
                    Get(chunks, coordinate, -1, -1),
                    null,
                    Get(chunks, coordinate, 1, -1),
                    Get(chunks, coordinate, 0, -2));
            }
        }

        private static Terrain Get(IChunkLookup chunks, ChunkCoordinate coordinate)
            => chunks.Get(coordinate)?.Terrain;
        
        private static Terrain Get(IChunkLookup chunks, ChunkCoordinate origin, int dx, int dy)
            => chunks.Get(new ChunkCoordinate(origin.X + dx, origin.Y + dy))?.Terrain;
    }
}