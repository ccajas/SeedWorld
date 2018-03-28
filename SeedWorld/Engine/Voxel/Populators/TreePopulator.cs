using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld.Engine
{
    public class TreePopulator
    {
        /// Objects to pass state to the populator
        public Biome biome;
        private byte[, ,] voxels;
        private Random rnd;

        /// <summary>
        /// Set biome and random generator to the populator
        /// </summary>
        public TreePopulator(Biome biome, byte[, ,] voxels, Random rnd)
        {
            this.voxels = voxels;
            this.biome = biome;
            this.rnd = rnd;
        }

        /// <summary>
        /// Add the tree
        /// </summary>
        public void Populate(int localX, int localZ, int groundLevel, int size)
        {
            int trunkHeight = rnd.Next(8) + 8;

            // Add random leaf type
            int leafType = rnd.Next(4);
            leafType = (int)BlockType.Foliage_1 + leafType;

            // Don't add a tree if elevation too high
            if (groundLevel + trunkHeight > size)
                return;

            for (int y = groundLevel; y < trunkHeight + groundLevel; y++)
            {
                int width = Math.Max(3, 5 - (y - groundLevel));

                for (int x = -width; x < width; x++)
                    for (int z = -width; z < width; z++)
                    {
                        if (localZ + z >= 0 && localX + x >= 0 &&
                            localZ + z <= size + 1 && localX + x <= size + 1)
                        {
                            if ((x >= 1 || x < -1) && (z >= 1 || z < -1)) continue;

                            voxels[localX + x, localZ + z, y] = (byte)BlockType.Wood;
                        }
                    }
            }

            // Finish adding trunk, now add leaves
            Vector3 baseRadius = new Vector3();
            baseRadius.Y = rnd.Next(4) + 5;
            baseRadius.X = baseRadius.Y + rnd.Next(4) + 2;
            baseRadius.Z = baseRadius.Y + rnd.Next(4) + 2;

            float angle = 0;

            Vector3 root = new Vector3(localX, groundLevel, localZ);
            AddLeaves(voxels, baseRadius, trunkHeight, size, root, leafType);

            // Add more leaves
            int steps = rnd.Next(3) + 5;

            for (int i = 0; i < steps; i++)
            {
                // Set random values for tree size
                Vector3 radius = baseRadius - new Vector3(rnd.Next(3) + 1, rnd.Next(2) + 1, rnd.Next(3) + 1);
                Vector3 offset = new Vector3((float)Math.Sin(angle) * 7, rnd.Next(5) - 2, (float)Math.Cos(angle) * 7);
                root = offset + new Vector3(localX, groundLevel, localZ);

                AddLeaves(voxels, radius, trunkHeight, size, root, leafType);
                angle += ((float)Math.PI / steps) * 2;
            }

            // Finish adding leaves
        }

        /// <summary>
        /// Helper function to add a group of leaves
        /// </summary>
        private static void AddLeaves(byte[, ,] voxels, Vector3 radius, int size,
            int trunkHeight, Vector3 root, int leafType)
        {
            root.X = (int)Math.Floor(root.X);
            root.Y = (int)Math.Floor(root.Y);
            root.Z = (int)Math.Floor(root.Z);

            // Finish adding trunk, now add leaves
            for (int i = (int)-radius.X; i <= radius.X; i++)
            {
                for (int j = (int)-radius.Z; j <= radius.Z; j++)
                {
                    for (int k = 0; k < radius.Y * 2; k++)
                    {
                        int trunkTop = k + trunkHeight;
                        if (ShapeTest.InsideChunkBounds(size, root.X + i, root.Z + j, root.Y + trunkTop))
                        {
                            BlockType block = (BlockType)voxels[(int)root.X + i, (int)root.Z + j, (int)root.Y + trunkTop];
                            if (block != BlockType.Empty)
                                continue;

                            bool insideSphere = ShapeTest.InsideEllipsoid(
                                radius,
                                new Vector3(root.X, root.Y + trunkHeight + radius.Y, root.Z),
                                new Vector3(root.X + i + 0.5f, root.Y + trunkTop + 0.5f, root.Z + j + 0.5f));

                            if (insideSphere)
                                voxels[(int)root.X + i, (int)root.Z + j, (int)root.Y + trunkTop] = 
                                    (byte)leafType;
                        }
                    }
                }
            }
            // Finish adding leaves
        }
    }
}
