using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    public class VoxelMeshManager
    {
        /// Device for creating mesh buffers
        private GraphicsDevice graphicsDevice;

        /// Storage for meshes and mesh totals
        Dictionary<String, MeshData> meshes;
        Dictionary<String, int> meshCounters;

        /// <summary>
        /// Loads and stores a collection of model meshes from coverting voxel arrays.
        /// The arrays can come from a variety of sources and stored in different data formats.
        /// </summary>
        public VoxelMeshManager(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            meshes = new Dictionary<String, MeshData>();
            meshCounters = new Dictionary<String, int>();

            // Load voxel mesh processors

        }
    }
}