using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeedWorld
{
    public class Octree
    {
        /// <summary>
        /// Find Morton code for Z-curve position from 3D coordinates
        /// </summary>
        public static uint MortonEncode(int x, int y, int z)
        {
            // pack 3 5-bit indices into a 15-bit Morton code
            x &= 0x0000001f; // 0000 0000 0000 0000 0000 0000 0001 1111
            y &= 0x0000001f;
            z &= 0x0000001f;
            x *= 0x01041041; // 0000 0001 0000 0100 0001 0000 0010 0001
            y *= 0x01041041;
            z *= 0x01041041;
            x &= 0x10204081; // 0001 0000 0010 0000 0100 0000 1000 0001
            y &= 0x10204081;
            z &= 0x10204081;
            x *= 0x00011111; // 0000 0000 0000 1111 1111 1111 1111 1111
            y *= 0x00011111;
            z *= 0x00011111;
            x &= 0x12490000; // 0001 0010 0100 1001 0000 0000 0000 0000
            y &= 0x12490000;
            z &= 0x12490000;
            return (uint)((x >> 16) | (y >> 15) | (z >> 14));
        }

        /// <summary>
        /// Find coordinate value from Z-curve position
        /// </summary>
        public static int MortonDecode(uint morton)
        {
            // unpack 3 5-bit indices from a 15-bit Morton code
            uint value = morton;
            value &= 0x00001249;
            value |= (value >> 2);
            value &= 0x000010c3;
            value |= (value >> 4);
            value &= 0x0000100f;
            value |= (value >> 8);
            value &= 0x0000001f;
            return (int)value;
        }

        /// <summary>
        /// Check if all nodes on this level are filled
        /// </summary>
        private void CheckFilledNodes(uint start, uint end, ref uint[] octree)
        {
            byte nodeData = 0;
            for (uint i = start; i < end; i++)
            {
                int index = (int)(i - 1) % 8;
                byte filled = (octree[i] > 0) ? (byte)1 : (byte)0;

                nodeData |= (byte)(filled << index);
                if (index == 7)
                {
                    // Set parent to completely filled if all nodes are filled
                    octree[(i - 1) >> 3] = Convert.ToUInt32(nodeData == 0xff);

                    nodeData = 0;
                }
            }
        }

        /// <summary>
        /// Create visible voxels from octree data
        /// </summary>

        private List<Cube> AddVisibleVoxels(uint[] octree, ref int[, ,] solidVoxels, 
            int index, uint node, int level = 0)
        {
            // Start at the lowest level
            if (node == 0) level = 0;
            int maxLevel = 5;

            // Node marked empty, don't add a cube mesh
            if (octree[0] == 2)
                return null;

            List<Cube> cubes = new List<Cube>();

            if (octree[node] == 0)
            {
                // Partially filled node, visit children
                uint nextNode = (node << 3) + 1;
                if (node > 1 + 8 + 512 + 4096)
                    return cubes;

                for (uint i = nextNode; i < nextNode + 8; i++)
                {
                    AddVisibleVoxels(octree, ref solidVoxels, index, i, level + 1);
                    //if (tempCubes != null)
                    //    cubes.AddRange(tempCubes);
                }

                return cubes;
            }
            else
            {
                uint topNode = node;
                for (int i = level; i < maxLevel; i++)
                    topNode = (topNode << 3) + 1;

                topNode -= (4096 + 512 + 64 + 8 + 1);
                int lx = MortonDecode(topNode);
                int ly = MortonDecode(topNode >> 1);
                int lz = MortonDecode(topNode >> 2);

                int offsetDivide = maxLevel - level;
                int max = (1 << offsetDivide);

                lx <<= offsetDivide;
                ly <<= offsetDivide;
                lz <<= offsetDivide;

                lx >>= offsetDivide;
                ly >>= offsetDivide;
                lz >>= offsetDivide;

                // Add solid voxels to test
                for (int x = 1; x <= max; x++)
                {
                    for (int y = 0; y < max; y++)
                    {
                        solidVoxels[x, 1, (index * 32) + y] += 0x100;
                        solidVoxels[x, max, (index * 32) + y] += 0x100;
                    }
                }

                for (int z = 2; z <= max - 1; z++)
                {
                    for (int y = 0; y < max; y++)
                    {
                        solidVoxels[1, z, (index * 32) + y] += 0x100;
                        solidVoxels[max, z, (index * 32) + y] += 0x100;
                    }
                }

                // Add the full octree node here
                uint clz = (uint)(node << 8);
                //Cube newCube = new Cube(new Vector3(_offset.X + lx, (index * 32) + ly, _offset.Y + lz),
                //    color + clz, 0, (1 << chunkSizePow) >> level);

                //cubes.Add(newCube);

                return cubes;
            }
        }
    }
}
