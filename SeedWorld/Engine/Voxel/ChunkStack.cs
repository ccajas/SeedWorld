using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    /// <summary>
    /// A group of chunks stacked vertically, used for easier organization and ordering of chunks
    /// </summary>
    class ChunkStack
    {
        /// Group of chunks
        public Chunk[] chunks;

        /// Spatial location of chunk stack
        public Point location;

        /// Distance to focal point
        public float distance;

        /// ChunkStack constants
        private readonly int numberOfChunks;
        private readonly int chunkSize;
        public readonly int stackHeight;

        /// Biome to build the chunks with
        private Biome biome;
        public Biome Biome
        {
            get { return biome; }
        }

        /// Check when chunks need to be updated
        ChunkState state;
        public ChunkState State
        {
            get { return state; }
        }

        /// <summary>
        /// Set up a empty array of chunks according to the chunk sizes
        /// </summary>
        public ChunkStack(Point location, int chunkSize, int numberOfChunks)
        {
            state = ChunkState.New;
            chunks = new Chunk[numberOfChunks];
            biome = new Biome(location, chunkSize);

            this.numberOfChunks = numberOfChunks;
            this.location = location;
            this.chunkSize = chunkSize;
            this.stackHeight = chunkSize * numberOfChunks;
        }

        /// <summary>
        /// Offset the location (usually to rebuild)
        /// </summary>
        /// <param name="offset"></param>
        public void OffsetLocation(Point offset, SpeedyDictionary chunkCollection)
        {
            // Remove old chunks and update location
            for (int i = 0; i < chunks.Length; ++i)
                chunkCollection.Remove(new Vector3(location.X, i * chunkSize, location.Y));

            this.location.X += offset.X;
            this.location.Y += offset.Y;

            // Update the chunk locations as well
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].SetLocation(new Vector3(location.X, i * chunkSize, location.Y));
            }

            // Reset biome location and generate 2D biome map data
            biome.SetLocation(location);
            Parallel.For(0, chunkSize + 2, x =>
            {
                for (int z = 0; z < chunkSize + 2; ++z)
                {
                    biome.GetBaseHeight(x, z, true);
                    biome.GetLocalHumidity(x, z, 0.05f, true);
                }
            });

            // Set dirty flag
            state = ChunkState.Dirty;
        }
    }
}
