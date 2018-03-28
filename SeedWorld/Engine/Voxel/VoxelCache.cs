using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    /// <summary>
    /// Temporary chunk data container for loading and updating chunks
    /// </summary>
    public class VoxelCache
    {
        /// Temp storage lists
        public float[,] heightValues;
        public float[,] slopeValues;

        /// 3D voxel data
        public Dictionary<Vector3, byte[, ,]> voxels;
        public byte[, ,] lightVoxels;
        public int[, ,] visibleNeighbors;

        /// Chunk info accessors
        public readonly int SizeX;
        public readonly int SizeY;
        public readonly int SizeZ;

        /// <summary>
        /// Initialize the lists
        /// </summary>
        /// <param name="chunkSize"></param>
        public VoxelCache(int sizeX, int sizeY, int sizeZ)
        {
            this.SizeX = sizeX;
            this.SizeY = sizeY;
            this.SizeZ = sizeZ;

            // These require overdraw areas to place certain objects. 
            // HeightData requires twice the chunk size, but allBlocks only requires 1 unit extra on each side.
            heightValues = new float[sizeX + 2, sizeZ + 2];
            slopeValues = new float[sizeX + 2, sizeZ + 2];
            voxels = new Dictionary<Vector3, byte[, ,]>();// new byte[sizeX + 2, sizeZ + 2, sizeY + 2];

            // Filtered blocks. These either fit or pad the chunk size
            visibleNeighbors = new int[sizeX, sizeZ, sizeY];
            lightVoxels = new byte[sizeX + 2, sizeZ + 2, sizeY + 2];
        }

        /// <summary>
        /// Clears all the array data from the cache
        /// </summary>
        public void ResetData()
        {
            Array.Clear(heightValues, 0, heightValues.Length);
            Array.Clear(slopeValues, 0, slopeValues.Length);

            Array.Clear(visibleNeighbors, 0, visibleNeighbors.Length);
            Array.Clear(lightVoxels, 0, lightVoxels.Length);
        }
    }
}
