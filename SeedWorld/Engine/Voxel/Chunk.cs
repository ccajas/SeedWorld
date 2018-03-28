using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    /// State of chunk build
    public enum ChunkState : byte
    {
        New,
        Dirty,
        VoxelsLoaded,
        NeighborsLoaded,
        Meshed
    }

    public class Chunk
    {
        /// Graphics resources
        private MeshData mesh;
        public MeshData Mesh { get { return mesh; } }
        private GraphicsDevice graphicsDevice;

        /// Upper left corner of chunk position in world
        private Vector3 offset, previousOffset;
        public Vector3 Offset 
        { 
            get { return offset; } 
        }

        /// Distance and empty status
        public float distance;
        private bool isEmpty;
        private bool isCompletelySolid;

        /// Load-in effects
        public Matrix transformMatrix;

        /// Check when chunk needs to be updated
        ChunkState state;
        public ChunkState State
        {
            get { return state; }
        }

        /// Vertex constants. Vertex max size is bound by indexMaxSize, 
        /// since there will always be more indices than vertices
        private readonly int vertexMaxSize = 65535;
        private readonly int indexMaxSize = 65535;

        private readonly int noiseRes = 120;
        private readonly static int totalNeighbors = 8;
        private readonly Vector3[][] points;

        /// List of surrounding neighbor chunks
        private Chunk[] sideNeighbors;
        public Chunk[] SideNeighbors
        {
            get { return sideNeighbors; }
        }

        /// Seed that this chunk belongs to
        private int mapSeed;

        /// Locally stored lightweight voxel data for this chunk. 2 bits/voxel
        private uint[, ,] voxels;
        public uint[, ,] Voxels
        {
            get { return voxels; }
        }

        /// Bounding 
        /// Box extents of chunk
        private Vector3[] extents;

        /// <summary>
        /// Constructor
        /// </summary>
        public Chunk(int seed, GraphicsDevice device)
        {
            offset = Vector3.Zero;
            mesh = new MeshData();

            // Points for ray marching
            points = new Vector3[6][];
            for (int i = 0; i < points.Length; i++)
                points[i] = ShapeTest.GoldenSpiralPoints(40, i);

            // Seed and neighbor list setup
            sideNeighbors = new Chunk[totalNeighbors];
            extents = new Vector3[2];
            mapSeed = seed;
            state = ChunkState.New;

            // Graphics resources
            graphicsDevice = device;
        }

        /// <summary>
        /// Offset the corner (usually to rebuild)
        /// </summary>
        /// <param name="offset"></param>
        public void SetLocation(Vector3 offset)
        {
            // Clear mesh data
            mesh = new MeshData();

            this.previousOffset = this.offset;
            this.offset = offset;

            // Reset status and neighbors
            state = ChunkState.Dirty;
            sideNeighbors = new Chunk[totalNeighbors];

            // At least one of these is false after chunk generation
            isEmpty = true;
            isCompletelySolid = true;
        }

        /// <summary>
        /// Attempt to connect neighboring chunks which are already loaded.
        /// </summary>
        public void ConnectNeighbors(SpeedyDictionary chunks, int size)
        {
            // Loop through possible neighbors in all directions
            for (int i = 0; i < 9; i++)
            {
                int x = 0;
                int z = 0;
                
                if (i != 4)
                {
                    if (i % 3 == 0) x -= 1;
                    if (i % 3 == 2) x += 1;
                    if (i < 3) z -= 1;
                    if (i > 5) z += 1;

                    Vector3 location = offset + new Vector3(x * size, 0, z * size);
                    Chunk neighborChunk = (Chunk)chunks.Get(location);
                    int j = (i > 4) ? i - 1 : i;

                    if (neighborChunk != null)
                    {
                        if (neighborChunk.state != ChunkState.Dirty)
                            sideNeighbors[j] = neighborChunk;
                    }
                    // Finish looking up this chunk
                }
            }
            // Finish checking all neighbors
        }

        /// <summary>
        /// Find a neighbor chunk by offset location
        /// </summary>
        public bool FindNeighbor(Vector3 location)
        {
            bool found = false;
            Parallel.ForEach(sideNeighbors, neighborChunk =>
            {
                if (neighborChunk != null && neighborChunk.Offset == location)
                    found = true;
            });
            return found;
        }

        /// <summary>
        /// Check if all neighbor voxel chunks are loaded
        /// </summary>
        public bool NeighborsLoaded()
        {
            bool loaded = true;
            Parallel.ForEach(sideNeighbors, neighborChunk =>
            {
                if (neighborChunk == null || neighborChunk.state == ChunkState.Dirty)
                    loaded = false;
            });
            return loaded;
        }

        /// <summary>
        /// First step in voxel chunk generation
        /// This could be used to get voxel data only (when mesh is not needed)
        /// </summary>
        private void BuildVoxels(VoxelCache cache, Biome biome)
        {
            // Generate the dense voxel data
            Parallel.For(0, cache.SizeX + 2, x =>
            {
                for (int z = 0; z < cache.SizeX + 2; ++z)
                {
                    //float slope = heightMap.CalculateSlope(x, z);
                    cache.heightValues[x, z] = biome.SourceHeight[x, z];
                    int yHeight = (int)cache.heightValues[x, z];
                
                    // Add some rivers
                    bool riverbed = false;
                    
                    // Create an alternate height for river banks. 
                    // Higher altitudes will have narrower streams
                    float maxRiverHeight = 120f;

                    // Create noise for the river depths
                    float noise = Noise.Simplex.Generate(
                        (float)(((offset.X + x) / (float)(noiseRes * 24f)) + 200f),
                        (float)(((offset.Z + z) / (float)(noiseRes * 24f)) + 200f));
                    noise += Noise.Simplex.Generate(
                        (float)(((offset.X + x) / (noiseRes * 4f)) + 200f),
                        (float)(((offset.Z + z) / (noiseRes * 4f)) + 200f)) / 5f;
                    noise += Noise.Simplex.Generate(
                        (float)(((offset.X + x) / (noiseRes / 2f)) + 200f),
                        (float)(((offset.Z + z) / (noiseRes / 2f)) + 200f)) / 50f;

                    noise *= MathHelper.Lerp(0, 1.5f, yHeight / maxRiverHeight);
                    noise = (float)Math.Pow(Math.Abs(noise), 0.7f);

                    // Increase humidity around river areas
                    float riverDepth = MathHelper.Lerp(0.8f, 0.98f, 
                        (yHeight > maxRiverHeight) ? 1 : yHeight / maxRiverHeight);
                    float surfaceHeight = cache.heightValues[x, z];

                    // Set depth of river beds
                    float maxRiverBankNoise = 0.10f;
                    if (noise >= 0.05f && noise < maxRiverBankNoise && yHeight < maxRiverHeight)
                    {
                        surfaceHeight -= (0.03f - (noise - 0.05f)) * 50f;
                        surfaceHeight--;
                    }

                    // Set the water level
                    float riverWaterLevel = surfaceHeight - (maxRiverBankNoise - 0.05f) * 100f;
                    riverWaterLevel++;

                    if (noise < 0.05f && yHeight < maxRiverHeight)
                    {
                        riverbed = true;
                        surfaceHeight = (int)(yHeight * riverDepth);
                        surfaceHeight--;
                    }

                    // Add plateau height, first by setting a maximum height
                    float noise2 = Noise.Simplex.Generate(
                         (float)(((offset.X + x) / (float)(noiseRes * 5f)) + 200f),
                         (float)(((offset.Z + z) / (float)(noiseRes * 3.2f)) + 200f));
                    noise2 += Noise.Simplex.Generate(
                        (float)(((offset.X + x) / (float)(noiseRes * 1.6f)) + 200f),
                        (float)(((offset.Z + z) / (float)(noiseRes * 2)) + 200f)) / 5f;

                    float plateauRange = 50f;
                    int plateauHeight = (int)((noise2 / 2f + 0.5f) * plateauRange);
                    plateauHeight += 70;

                    for (int y = 0; y < cache.SizeY + 2; ++y)
                    {
                        // Get voxels from height indexes
                        BlockType block = BlockType.Empty;

                        // Add block type depending on relation to surface
                        if (y + offset.Y < (int)surfaceHeight)
                            block = BlockType.Surface;

                        if (riverbed && y + offset.Y < riverWaterLevel)
                            block = BlockType.Water;

                        if (y + offset.Y == (int)surfaceHeight && y + offset.Y < yHeight)
                            block = BlockType.Surface;

                        // Add actual plateau blocks here
                        if (y + offset.Y >= yHeight && y + offset.Y <= plateauHeight)
                        {
                            noise2 = Noise.Simplex.Generate(
                                (float)(((offset.X + x) / (noiseRes * 1.5f)) + 200f),
                                (float)(((offset.Y + y) / (noiseRes * 3f)) + 200f),
                                (float)(((offset.Z + z) / (noiseRes * 1.25f)) + 200f));
                            noise2 += Noise.Simplex.Generate(
                                (float)(((offset.X + x) / (noiseRes / 1.5f)) + 200f),
                                (float)(((offset.Y + y) / (noiseRes)) + 200f),
                                (float)(((offset.Z + z) / (noiseRes / 1.25f)) + 200f)) / 5f;
                            noise2 += Noise.Simplex.Generate(
                                (float)(((offset.X + x) / (noiseRes / 5f)) + 200f),
                                (float)(((offset.Y + y) / (noiseRes / 2f)) + 200f),
                                (float)(((offset.Z + z) / (noiseRes / 5f)) + 200f)) / 10f;

                            // Adjust plateau height around rivers
                            float threshold = 0.3f;
                            if (noise < maxRiverBankNoise * 3f)
                                noise2 *= (float)Math.Pow((noise / (maxRiverBankNoise * 3f)), 0.5);

                            if (noise2 > threshold)
                            {
                                // Inner blocks
                                block = BlockType.Dirt;

                                if (y + offset.Y == plateauHeight)
                                    block = BlockType.Surface;
                            }
                        }

                        // Set upper and lower bounds
                        if (block != BlockType.Empty)
                        {
                            cache.voxels[offset][x, z, y] = (byte)block;
                            isEmpty = false;
                        }
                        else if (isCompletelySolid)
                        {
                            isCompletelySolid = false;
                        }
                    }
                    // Finished updating this voxel
                }
            });

            // Add extra objects to the surface such as trees
            for (int i = 0; i < 9; i++)
            {
                int x = i / 3 - 1;
                int z = i % 3 - 1;
                GenerateExtraObjects(biome, cache, x, z);
            }

            // Set lightweight voxel data for collisions/physics
            Parallel.For(1, cache.SizeX + 1, x =>
            {
                for (int z = 1; z < cache.SizeZ + 1; ++z)
                {
                    for (int y = 0; y < cache.SizeY; ++y)
                    {
                        int voxelY = y / 16;
                        int voxelBits = (y % 16) * 2;

                        uint voxel = voxels[x - 1, z - 1, voxelY];

                        // Add 1 to voxel data
                        BlockType block = (BlockType)cache.voxels[offset][x, z, y];

                        if (block != BlockType.Empty)
                            voxel = voxel | ((uint)1 << voxelBits);

                        voxels[x - 1, z - 1, voxelY] = voxel;
                    }
                }
            });

            // Set bounding box extents
            extents[0] = new Vector3(offset.X, offset.Y, offset.Z);
            extents[1] = new Vector3(offset.X + cache.SizeX, offset.Y + cache.SizeY, offset.Z + cache.SizeX);

            // Create temp mesh and bounding box
            mesh = new MeshData();
            mesh.bBox = new BoundingBox(extents[0], extents[1]);
        }

        /// <summary>
        /// Generate additional 3D objects using the heightmap and surrounding blocks
        /// </summary>
        private void GenerateExtraObjects(Biome biome, VoxelCache cache, int x, int z)
        {
            int newOffsetX = (int)(offset.X + (x * cache.SizeX));
            int newOffsetZ = (int)(offset.Z + (z * cache.SizeZ));
            int seed = cache.GetHashCode() ^ ((newOffsetX << 13) + (newOffsetZ >> 5));// ^ cache.MapSeed;

            Random rnd = new Random(seed);
            int localX = rnd.Next(cache.SizeX) ^ (mapSeed & 0x1f);
            int localZ = rnd.Next(cache.SizeZ) ^ (mapSeed & 0x1f);

            localX++;
            localZ++;

            float objectDist = Noise.Simplex.Generate(
                (float)(((float)newOffsetX / 800f) + 120f),
                (float)(((float)newOffsetZ / 800f) + 100f));

            objectDist = (float)Math.Pow((objectDist * 0.5f) + 0.5f, 1.2f);

            int objectChance = rnd.Next(100);
            int chance = (int)(objectDist * 100f) / 2;

            // We can grab the height value instantly from the noise pattern
            float height = biome.GetBaseHeight(localX + newOffsetX, localZ + newOffsetZ, false);

            if (height < 80 && objectChance < chance)
            {
                Engine.TreePopulator treePopulator = new Engine.TreePopulator(biome, cache.voxels[offset], rnd);

                treePopulator.Populate( 
                    localX + (x * cache.SizeX), 
                    localZ + (z * cache.SizeZ),
                    (int)height, cache.SizeX);
            }
        }

        /// <summary>
        /// Get a voxel from the lightweight voxel array
        /// </summary>
        public byte GetVoxel(int x, int y, int z)
        {
            int voxelY = y >> 4;
            int voxelBits = (y % 16) << 1;

            // Get voxel data at this location
            uint voxel = voxels[x, z, voxelY];
            voxel = (voxel >> voxelBits) & 0x3;

            return (byte)voxel;
        }

        /// <summary>
        /// Second step in chunk generation
        /// Add visible voxels to the list.
        /// </summary>
        private void FindVisible(VoxelCache cache, Biome biome)
        {
            // Assign temporary arrays for visible voxels
            byte[, ,] visibleVoxels = new byte[cache.SizeX + 2, cache.SizeX + 2, cache.SizeY + 2];
            byte[, ,] cacheVoxels = cache.voxels[offset];

            Parallel.For(1, cache.SizeX + 1, x =>
            {
                for (int z = 0; z <= cache.SizeZ + 1; ++z)
                {
                    // Get last shade for x, z location
                    byte shade = biome.lightMask[x, z];
                    bool shaded = (shade == 15);

                    for (int y = cache.SizeY + 1; y >= 0; --y)
                    {
                        byte voxel = cacheVoxels[x, z, y];

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
                                neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z, y] > 0) << NeighborBlock.Middle_W;
                                neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z, y] > 0) << NeighborBlock.Middle_E;
                                neighbors |= Convert.ToInt32(cacheVoxels[x, z - 1, y] > 0) << NeighborBlock.Middle_N;
                                neighbors |= Convert.ToInt32(cacheVoxels[x, z + 1, y] > 0) << NeighborBlock.Middle_S;

                                // Check top and bottom
                                neighbors |= Convert.ToInt32(cacheVoxels[x, z, y - 1] > 0) << NeighborBlock.Bottom_Center;
                                neighbors |= Convert.ToInt32(cacheVoxels[x, z, y + 1] > 0) << NeighborBlock.Top_Center;

                                // Add to the visible list if not occluded on all sides
                                if (neighbors != 0x3f)
                                {
                                    visibleVoxels[x, z, y] = voxel;
                                    cache.visibleNeighbors[x - 1, z - 1, y - 1] = neighbors;
                                }

                                cache.lightVoxels[x, z, y] = shade;
                                shade = 8;
                            }
                        }

                        // Finish checking this voxel
                        if (y == 1)
                            biome.lightMask[x, z] = shade;
                    }
                }
            });

            // Final voxel list is truncated
            

            cache.voxels[offset] = visibleVoxels;
        }

        /// <summary>
        /// Third step in chunk generation
        /// Turn visible voxels into meshes
        /// </summary>
        private void BuildMesh(VoxelCache cache, Biome biome)
        {
            int nextVertex = 0;
            int nextIndex = 0;

            Object lockObject = new object();

            // Initialize vertex and index data
            VertexPositionColorNormal[] chunkVertices = new VertexPositionColorNormal[vertexMaxSize];
            ushort[] chunkIndices = new ushort[indexMaxSize];
          
            // Assign temp array
            byte[, ,] cacheVoxels = cache.voxels[offset];

            // Create the mesh cubes
            Parallel.For(1, cache.SizeX + 1, x =>
            {
                for (int z = 1; z < cache.SizeZ + 1; ++z)
                {
                    for (int y = 1; y < cache.SizeY + 1; ++y)
                    {
                        byte voxel = cacheVoxels[x, z, y];

                        if (nextIndex + 72 > indexMaxSize)
                        {
                            // Skip
                            break;
                        }
                        else if (voxel > 0)
                        {
                            int neighbors = cache.visibleNeighbors[x - 1, z - 1, y - 1];

                            // Get the corner and edge neighbors
                            // Top corners (bits 7-10)
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z - 1, y + 1] > 0) << NeighborBlock.Top_NW;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z - 1, y + 1] > 0) << NeighborBlock.Top_NE;
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z + 1, y + 1] > 0) << NeighborBlock.Top_SW;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z + 1, y + 1] > 0) << NeighborBlock.Top_SE;

                            // Top edges (bits 11-14)
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z, y + 1] > 0) << NeighborBlock.Top_W;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z, y + 1] > 0) << NeighborBlock.Top_E;
                            neighbors |= Convert.ToInt32(cacheVoxels[x, z - 1, y + 1] > 0) << NeighborBlock.Top_N;
                            neighbors |= Convert.ToInt32(cacheVoxels[x, z + 1, y + 1] > 0) << NeighborBlock.Top_S;

                            // Bottom corners (bits 15-18)
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z - 1, y - 1] > 0) << NeighborBlock.Bottom_NW;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z - 1, y - 1] > 0) << NeighborBlock.Bottom_NE;
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z + 1, y - 1] > 0) << NeighborBlock.Bottom_SW;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z + 1, y - 1] > 0) << NeighborBlock.Bottom_SE;

                            // Bottom edges (bits 19-22)
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z, y - 1] > 0) << NeighborBlock.Bottom_W;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z, y - 1] > 0) << NeighborBlock.Bottom_E;
                            neighbors |= Convert.ToInt32(cacheVoxels[x, z - 1, y - 1] > 0) << NeighborBlock.Bottom_N;
                            neighbors |= Convert.ToInt32(cacheVoxels[x, z + 1, y - 1] > 0) << NeighborBlock.Bottom_S;

                            // Middle side corners (bits 23-26)
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z - 1, y] > 0) << NeighborBlock.Middle_NW;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z - 1, y] > 0) << NeighborBlock.Middle_NE;
                            neighbors |= Convert.ToInt32(cacheVoxels[x - 1, z + 1, y] > 0) << NeighborBlock.Middle_SW;
                            neighbors |= Convert.ToInt32(cacheVoxels[x + 1, z + 1, y] > 0) << NeighborBlock.Middle_SE;

                            // Set the cube color and shade
                            uint color = biome.GetBlockColor((BlockType)voxel, (int)(x + Offset.X), y, (int)(z + Offset.Z));
                            float[] shade = ComputeShade(cache, biome, neighbors, x, y, z);

                            // Add the cube
                            Cube cube = new Cube(new Vector3(x, y, z), color, shade, neighbors);

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

        /// <summary>
        /// Generate a world chunk
        /// </summary>
        public void Setup(VoxelCache cache, Biome biome)
        {
            // Create lightweight voxel array. At 2 bits/voxel, Y axis shares 16 voxels per int
            voxels = new uint[cache.SizeX, cache.SizeZ, cache.SizeY / 16];

            cache.voxels.Remove(previousOffset);
            cache.voxels.Add(offset, 
                new byte[cache.SizeX + 2, cache.SizeZ + 2, cache.SizeY + 2]);

            // First generate the height map and dense voxel data
            BuildVoxels(cache, biome);
            state = ChunkState.VoxelsLoaded;

            // Remove voxels from cache if chunk doesn't need a mesh
            if (isEmpty || isCompletelySolid)
                cache.voxels.Remove(offset);

            // Reset cache
            cache.ResetData();
        }

        public void Build(VoxelCache cache, Biome biome)
        {
            // Check if neighbors are loaded for next time
            if (state == ChunkState.VoxelsLoaded && NeighborsLoaded())
                state = ChunkState.NeighborsLoaded;

            // Next, build the mesh
            if (state == ChunkState.NeighborsLoaded)
            {
                if (!isEmpty && !isCompletelySolid)
                {
                    // Second pass: Add visible voxels
                    FindVisible(cache, biome);

                    // Third pass: Build mesh out of visible voxels
                    BuildMesh(cache, biome);
                }

                state = ChunkState.Meshed;

                // Remove data from cache and reset
                cache.voxels.Remove(offset);
                cache.ResetData();

                // Set matrix transformation
                transformMatrix = Matrix.CreateTranslation(offset.X, offset.Y, offset.Z);
            }
        }

        /// <summary>
        /// Calculate shading for all sides of a cube
        /// </summary>
        private float[] ComputeShade(VoxelCache cache, Biome biome, int neighbors, int x, int y, int z)
        {
            // Get shade contributions using a ray cast
            float altShade = (float)(cache.lightVoxels[x, z, y] / 20f);
            altShade = 1 - (float)Math.Pow(altShade, 1.25f);

            // Get shade contributions using a ray cast
            float[] shade = new float[6];

            // Shade all the cube sides individually
            for (int s = 0; s < shade.Length; s++)
            {
                // Start at full light
                shade[s] = 1f;
                float step = 3f / points[s].Length;

                // Skipped sides that have neighbors
                if ((neighbors & 1 << s) == 1) continue;

                for (int j = 0; j < points[s].Length; j++)
                {
                    if (shade[s] <= 0.1f)
                        break;

                    bool found = false;

                    for (float i = 2.5f; i <= 10.5f; i += 1f)
                    {
                        if (found) continue;

                        Vector3 newBlock;
                        int chunkIndex = 4;

                        newBlock.X = i * points[s][j].X + x;
                        newBlock.Y = i * points[s][j].Y + y;
                        newBlock.Z = i * points[s][j].Z + z;

                        int nx = (int)Math.Round(newBlock.X) - 1;
                        int ny = (int)Math.Round(newBlock.Y);
                        int nz = (int)Math.Round(newBlock.Z) - 1;

                        // Set the new voxel offset if ray enters another chunk
                        if (nx < 0)            chunkIndex--;
                        if (nz < 0)            chunkIndex -= 3;
                        if (nx >= cache.SizeX) chunkIndex++;
                        if (nz >= cache.SizeZ) chunkIndex += 3;

                        if (nx < 0)            nx += cache.SizeX;
                        if (nz < 0)            nz += cache.SizeZ;
                        if (nx >= cache.SizeX) nx -= cache.SizeX;
                        if (nz >= cache.SizeZ) nz -= cache.SizeZ;
                            
                        // Continue if Y goes out of bounds
                        if (ny < 0 || ny >= cache.SizeY) continue;

                        byte voxel = GetVoxel(nx, ny, nz);
                        if (chunkIndex != 4)
                        {
                            if (chunkIndex > 4) chunkIndex--;
                            voxel = sideNeighbors[chunkIndex].GetVoxel(nx, ny, nz);
                        }

                        float falloff = 1f / i;
                        if (voxel != (byte)BlockType.Empty)
                        {
                            shade[s] -= (step * falloff);
                            found = true;
                        }
                    }
                }

                shade[s] = (altShade > shade[s]) ? shade[s] : altShade;
                shade[s] = (shade[s] < 0.15f) ? 0.15f : shade[s];
            }

            return shade;
        }
    }
}
