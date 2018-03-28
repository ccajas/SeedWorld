using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    /// <summary>
    /// Creates and updates all the voxel chunks of the world, using a collection of Chunks and ChunkStacks
    /// </summary>
    class ChunkManager
    {
        /// Constants
        private readonly int chunkSize = 32;
        private readonly int chunksPerStack = 8;
        private readonly int visibleRadius = 9;
        private readonly int maxFrameLoadTime = 8;

        /// Chunk data
        private SpeedyDictionary chunks;
        private Dictionary<Point, ChunkStack> chunkStacks;
        private List<ChunkStack> orderedChunkStacks;

        /// Mapping of chunks by location
        private List<Chunk> orderedChunks;
        public List<Chunk> Chunks
        {
            get { return orderedChunks; }
        }

        /// Chunk info
        public VoxelCache voxelCache;
        public int mapSeed { get; private set; }

        /// Chunk loading counter
        private int chunksAdded = 0;
        public int ChunksAdded 
        { 
            get { return chunksAdded; } 
        }

        private int chunksLoaded = 0;
        public int ChunksLoaded
        {
            get { return chunksLoaded; }
        }

        /// Chunk object populator
        private Engine.TreePopulator populator;

        /// <summary>
        /// Load chunks for the first time
        /// </summary>
        public ChunkManager(GraphicsDevice graphicsDevice, int seed)
        {
            // Set the initial seed
            this.mapSeed = seed;

            // Get the bounds of the visible world and set up chunk structures
            int totalChunks = (int)Math.Pow((visibleRadius * 2 + 1), 2);
            chunkStacks = new Dictionary<Point, ChunkStack>(totalChunks);
            orderedChunkStacks = new List<ChunkStack>(totalChunks);

            // Initialize lists for chunk distance ordering
            chunks = new SpeedyDictionary();// Dictionary<Vector3, Chunk>(totalChunks * chunksPerStack);
            orderedChunks = new List<Chunk>(totalChunks * chunksPerStack);

            // Setup biome and cache
            voxelCache = new VoxelCache(chunkSize, chunkSize, chunkSize);

            // Populator to add additional objects to chunks
            //populator = new Engine.TreePopulator(biome, voxelCache, new Random(seed));
        }

        /// <summary>
        /// Set initial location for chunks (usually around the player)
        /// </summary>
        public void Initialize(GraphicsDevice graphicsDevice, Vector3 centerLocation)
        {
            // First chunk location
            Vector3 chunkOffset = Vector3.Zero;
            chunkOffset.X = (int)Math.Round(centerLocation.X / chunkSize) * chunkSize;
            chunkOffset.Z = (int)Math.Round(centerLocation.Z / chunkSize) * chunkSize;   
            chunkOffset.Y = 0;
            
            // Set boundaries
            int nextChunk = 0;
            int minX = (int)(chunkOffset.X - (chunkSize * visibleRadius));
            int minZ = (int)(chunkOffset.Z - (chunkSize * visibleRadius));

            // Initialize all chunks in the map
            for (int z = minZ; z < minZ + (chunkSize * (visibleRadius * 2 + 1)); z += chunkSize)
            {
                for (int x = minX; x < minX + (chunkSize * (visibleRadius * 2 + 1)); x += chunkSize)
                {
                    Point location = new Point(x, z);
                    ChunkStack stack = new ChunkStack(Point.Zero, chunkSize, chunksPerStack);

                    // Add chunks for this stack
                    for (int y = 0; y < chunksPerStack; ++y)
                    {
                        Chunk chunk = new Chunk(mapSeed, graphicsDevice);

                        chunkOffset.X = x;
                        chunkOffset.Y = y * chunkSize;
                        chunkOffset.Z = z;

                        // Set up actual chunk
                        chunk.SetLocation(chunkOffset);

                        // Add to ordered list and to stack
                        orderedChunks.Add(chunk);
                        stack.chunks[y] = chunk;
                        nextChunk++;
                    }

                    // Add the stack
                    stack.OffsetLocation(location, chunks);
                    stack.distance = Vector2.Distance(
                        new Vector2(centerLocation.X, centerLocation.Z),
                        new Vector2(x, z)
                    );
                    orderedChunkStacks.Add(stack);
                }
            }

            // Order chunks by distance so they load starting from center outwards
            SortChunks();
        }

        /// <summary>
        /// Used when new chunks are made or need to be re-sorted again
        /// </summary>
        private void SortChunks()
        {
            orderedChunkStacks.Sort((a, b) => a.distance.CompareTo(b.distance));
        }

        /// <summary>
        /// Get voxel collision data for a specific world coordinate. 
        /// If voxel location does not exist in list of chunks, return 0
        /// </summary>
        public uint GetVoxelAt(int x, int y, int z)
        {
            // Find chunk offset
            Vector3 chunkOffset = Vector3.Zero;
            chunkOffset.X = (x >> 5) << 5;
            chunkOffset.Y = (y >> 5) << 5;
            chunkOffset.Z = (z >> 5) << 5;

            // Get the chunk that contains this block
            Chunk chunk = (Chunk)chunks.Get(chunkOffset);

            // Handle out of bounds Y values
            if (y < 0) return 1;  
            if (y > chunkSize * chunksPerStack - 1) return 0;

            if (chunk != null)
            {
                x &= (chunkSize - 1);
                y &= (chunkSize - 1);
                z &= (chunkSize - 1);

                return chunk.GetVoxel(x, y, z);
            }

            return 1;
        }

        /// <summary>
        /// See if any chunks need updating to reload new parts of the world
        /// </summary>
        public void CheckForChunkUpdates(TimeSpan frameStepTime, Vector3 centerLocation)
        {
            int maxDistance = (int)(visibleRadius + 1) * chunkSize;

            // Check if any chunks need to be replaced. 
            // Any replaceable chunks need to be "popped" from the Dictionary
            foreach (ChunkStack chunkStack in orderedChunkStacks)
            {
                if ((centerLocation.X - chunkStack.location.X) > maxDistance)
                {
                    chunkStacks.Remove(chunkStack.location);
                    chunkStack.OffsetLocation(new Point(chunkSize * (visibleRadius * 2 + 1), 0), chunks);
                }

                if ((centerLocation.X - chunkStack.location.X) < -maxDistance)
                {
                    chunkStacks.Remove(chunkStack.location);
                    chunkStack.OffsetLocation(new Point(-chunkSize * (visibleRadius * 2 + 1), 0), chunks);                   
                }

                if ((centerLocation.Z - chunkStack.location.Y) > maxDistance)
                {
                    chunkStacks.Remove(chunkStack.location);
                    chunkStack.OffsetLocation(new Point(0, chunkSize * (visibleRadius * 2 + 1)), chunks);                    
                }

                if ((centerLocation.Z - chunkStack.location.Y) < -maxDistance)
                {
                    chunkStacks.Remove(chunkStack.location);
                    chunkStack.OffsetLocation(new Point(0, -chunkSize * (visibleRadius * 2 + 1)), chunks);  
                }
            }

            // Check if new chunks need to be updated
            int updatedChunks = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (ChunkStack stack in orderedChunkStacks)
            {
                if (watch.Elapsed.TotalMilliseconds > maxFrameLoadTime)
                    break;

                // If some chunks need updating here
                if (stack.State == ChunkState.Dirty)
                {
                    // First re-add chunk to the Dictionary
                    ChunkStack tempStack;
                    if (!chunkStacks.TryGetValue(stack.location, out tempStack))
                    {
                        chunkStacks.Add(stack.location, stack);

                        // Update the actual stack's location
                        stack.distance = Vector2.Distance(
                            new Vector2(centerLocation.X, centerLocation.Z),
                            new Vector2(stack.location.X, stack.location.Y)
                        );
                    }
                    
                    // Now loop within each stack to update chunks
                    for (int i = chunksPerStack - 1; i >= 0; --i)
                    {
                        if (watch.Elapsed.TotalMilliseconds > maxFrameLoadTime)
                            break;

                        // Only add dirty chunks
                        if (stack.chunks[i].State == ChunkState.Dirty)
                        {
                            Chunk chunk = stack.chunks[i];
                            chunk.Setup(voxelCache, stack.Biome);
                            chunks.Add(chunk.Offset, chunk);

                            updatedChunks++;
                            chunksAdded++;
                        }
                        
                        // Pass along Chunk collection to connect neighbors
                        if (stack.chunks[i].State == ChunkState.VoxelsLoaded)
                        {
                            Chunk chunk = stack.chunks[i];

                            // Attempt to build mesh
                            chunk.ConnectNeighbors(chunks, voxelCache.SizeX);
                            chunk.Build(voxelCache, stack.Biome);

                            if (chunk.State == ChunkState.NeighborsLoaded)
                                chunksLoaded++;
                        }
                    }
                    // Finish updating chunks in this group
                }
            }

            watch.Stop();
            //SortChunks();
        }
    }
}
