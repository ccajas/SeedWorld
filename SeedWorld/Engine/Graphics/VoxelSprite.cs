using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    class VoxelSprite
    {
        /// Graphics resources
        private MeshData mesh;
        public MeshData Mesh { get { return mesh; } }
        private GraphicsDevice graphicsDevice;

        /// Sprite dimensions
        private int sizeX;
        private int sizeY;
        private int sizeZ;

        /// Vertex constants. Vertex max size is bound by indexMaxSize, 
        /// since there will always be more indices than vertices
        private readonly int vertexMaxSize = 65535;
        private readonly int indexMaxSize = 65535;

        /// Spatial data for voxels
        private byte[] voxelData;
        private uint[] colors;
        private Vector3[] extents;

        /// <summary>
        /// Make a voxelSprite with existing data and dimensions
        /// </summary>
        public VoxelSprite(GraphicsDevice device)
        {
            this.graphicsDevice = device;
            this.colors = new uint[256];

            sizeX = 32;
            sizeY = 32;
            sizeZ = 128;

            extents = new Vector3[2];
        }

        /// <summary>
        /// Load voxel data and create a mesh from it
        /// </summary>
        public void Load(VoxelCache cache, String filename)
        {
            this.voxelData = MagicaVoxImporter.Load(filename);

            // Setup cache
            cache.ResetData();
            cache.voxels.Add(Vector3.One, 
                new byte[cache.SizeX + 2, cache.SizeZ + 2, cache.SizeY + 2]);

            // First generate the height map and dense voxel data
            BuildVoxels(cache);

            // Second pass: Add visible voxels
            FindVisible(cache);

            // Third pass: Build mesh out of visible voxels
            BuildMesh(cache);

            // Reset cache
            cache.voxels.Remove(Vector3.One);
            cache.ResetData();
        }

        /// <summary>
        /// Copy voxel data to 3D array
        /// </summary>
        /// <param name="voxels"></param>
        private void BuildVoxels(VoxelCache cache)
        {
            for (int i = 0; i < 256; i++)
            {
                byte b = voxelData[i * 3];
                byte g = voxelData[i * 3 + 1];
                byte r = voxelData[i * 3 + 2];
                colors[i] = (uint)((r << 24) | (g << 16) | (b << 8));
            }

            // Add non-empty voxels to the array
            for (int i = 768; i < voxelData.Length - 6; i += 4)
            {
                int x = voxelData[i + 1];
                int z = voxelData[i + 2];
                int y = voxelData[i + 3];
                cache.voxels[Vector3.One][x + 1, z + 1, y + 1] = voxelData[i];
            }

            // Add bounding box extents
            int offset = voxelData.Length - 6;
            extents[0] = new Vector3(voxelData[offset], voxelData[offset + 2], voxelData[offset + 1]);
            extents[1] = new Vector3(voxelData[offset + 3] + 1, voxelData[offset + 5] + 1, voxelData[offset + 4] + 1);
        }

        /// <summary>
        /// Copy existing voxel data into an array
        /// </summary>
        private void LoadData(byte[, ,] voxels)
        {

        }

        /// <summary>
        /// Second step in voxel mesh generation
        /// Add visible voxels to the list.
        /// </summary>
        private void FindVisible(VoxelCache cache)
        {
            // Temporary array for visible voxels
            byte[, ,] visibleVoxels = new byte[cache.SizeX + 2, cache.SizeZ + 2, cache.SizeY + 2];

            for (int y = cache.SizeY - 1; y >= 0; --y)
            {
                Parallel.For(0, cache.SizeX, x =>
                {
                    for (int z = 0; z < cache.SizeZ; ++z)
                    {
                        byte voxel = (byte)cache.voxels[Vector3.One][x, z, y];
                        if (voxel != 0)
                        {
                            int neighbors = 0;

                            if (x == 0 || y == 0 || z == 0 ||
                                x == cache.SizeX + 1 || y == cache.SizeY + 1 || z == cache.SizeZ + 1)
                            {
                                visibleVoxels[x, z, y] = voxel;
                            }
                            else
                            {
                                // Immediate side neighbors
                                neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z, y] > 0) << NeighborBlock.Middle_W;
                                neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z, y] > 0) << NeighborBlock.Middle_E;
                                neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z - 1, y] > 0) << NeighborBlock.Middle_N;
                                neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z + 1, y] > 0) << NeighborBlock.Middle_S;

                                // Check top and bottom
                                neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z, y - 1] > 0) << NeighborBlock.Bottom_Center;
                                neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z, y + 1] > 0) << NeighborBlock.Top_Center;

                                // Add to the visible list if not occluded on all sides
                                if (neighbors != 0x3f)
                                {
                                    visibleVoxels[x, z, y] = voxel;
                                    cache.visibleNeighbors[x - 1, z - 1, y - 1] = neighbors;
                                }
                            }
                        }
                        // Finish neighbor search on this voxel
                    }
                });
            }

            // Finish adding all visible voxels. 
            // Copy them back to the original voxel array
            cache.voxels[Vector3.One] = visibleVoxels;
        }

        /// <summary>
        /// Third step in mesh generation
        /// Turn visible voxels into meshes
        /// </summary>
        private void BuildMesh(VoxelCache cache)
        {
            int nextVertex = 0;
            int nextIndex = 0;

            Object lockObject = new object();

            // Initialize vertex and index data
            VertexPositionColorNormal[] chunkVertices = new VertexPositionColorNormal[vertexMaxSize];
            ushort[] chunkIndices = new ushort[indexMaxSize];

            // Create the mesh cubes
            Parallel.For(1, sizeX + 1, x =>
            {
                for (int z = 1; z <= sizeZ; ++z)
                {
                    for (int y = 1; y <= sizeY; ++y)
                    {
                        byte voxel = (byte)cache.voxels[Vector3.One][x, z, y];

                        if (nextIndex + 72 > indexMaxSize)
                        {
                            // Skip
                        }
                        else if (voxel > 0)
                        {
                            int neighbors = cache.visibleNeighbors[x - 1, z - 1, y - 1];

                            // Get the corner and edge neighbors
                            // Top corners (bits 7-10)
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z - 1, y + 1] > 0) << NeighborBlock.Top_NW;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z - 1, y + 1] > 0) << NeighborBlock.Top_NE;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z + 1, y + 1] > 0) << NeighborBlock.Top_SW;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z + 1, y + 1] > 0) << NeighborBlock.Top_SE;

                            // Top edges (bits 11-14)
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z, y + 1] > 0) << NeighborBlock.Top_W;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z, y + 1] > 0) << NeighborBlock.Top_E;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z - 1, y + 1] > 0) << NeighborBlock.Top_N;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z + 1, y + 1] > 0) << NeighborBlock.Top_S;

                            // Bottom corners (bits 15-18)
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z - 1, y - 1] > 0) << NeighborBlock.Bottom_NW;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z - 1, y - 1] > 0) << NeighborBlock.Bottom_NE;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z + 1, y - 1] > 0) << NeighborBlock.Bottom_SW;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z + 1, y - 1] > 0) << NeighborBlock.Bottom_SE;

                            // Bottom edges (bits 19-22)
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z, y - 1] > 0) << NeighborBlock.Bottom_W;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z, y - 1] > 0) << NeighborBlock.Bottom_E;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z - 1, y - 1] > 0) << NeighborBlock.Bottom_N;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x, z + 1, y - 1] > 0) << NeighborBlock.Bottom_S;

                            // Middle side corners (bits 23-26)
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z - 1, y] > 0) << NeighborBlock.Middle_NW;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z - 1, y] > 0) << NeighborBlock.Middle_NE;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x - 1, z + 1, y] > 0) << NeighborBlock.Middle_SW;
                            neighbors |= Convert.ToInt32(cache.voxels[Vector3.One][x + 1, z + 1, y] > 0) << NeighborBlock.Middle_SE;

                            // Set the cube color and shade
                            uint color = colors[voxel];

                            float[] shades = { 1f, 1f, 1f, 1f, 1f, 1f };

                            // Add the cube
                            Cube cube = new Cube(new Vector3(x, y, z), color, shades, neighbors);

                            lock (lockObject)
                            {
                                // Point next index value to number of vertices
                                int indexOffset = nextVertex;

                                for (int v = 0; v < cube.Vertices.Length; v++)
                                    chunkVertices[nextVertex++] = cube.Vertices[v];

                                for (int i = 0; i < cube.Indices.Length; i++)
                                    chunkIndices[nextIndex++] = (ushort)(indexOffset + cube.Indices[i]);
                            }
                        }
                        // Finish adding cube for this voxel
                    }
                }
            });

            // Create a mesh
            mesh = new MeshData();

            // Create bounding box
            mesh.bBox = new BoundingBox(extents[0], extents[1]);

            if (nextIndex > 0)
            {
                // Create vertex and index buffers
                mesh.vb = new VertexBuffer(
                    graphicsDevice,
                    VertexPositionColorNormal.VertexDeclaration,
                    nextVertex,
                    BufferUsage.None
                );

                mesh.ib = new IndexBuffer(
                    graphicsDevice, IndexElementSize.SixteenBits,
                    (ushort)nextIndex,
                    BufferUsage.None
                );

                // Set up vertex and index buffer
                mesh.vb.SetData(chunkVertices, 0, nextVertex);
                mesh.ib.SetData(chunkIndices, 0, nextIndex);
            }
            // Finished building meshes
        }
    }
}
