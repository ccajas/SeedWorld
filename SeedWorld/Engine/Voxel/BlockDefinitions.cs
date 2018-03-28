using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeedWorld
{
    /// <summary>
    /// Designate different block types
    /// </summary>
    public enum BlockType : byte
    {
        Empty = 0,
        Water,
        Surface,
        Dirt,
        Stone,
        Wood,
        Foliage_1,
        Foliage_2,
        Foliage_3,
        Foliage_4,
        Custom,

        // Special modifiers
        Mod_1 = 0x10,
        Mod_2 = 0x20,
        Mod_3 = 0x40,
        Mod_4 = 0x80
    }

    /// <summary>
    /// Neighbor block locations
    /// </summary>
    public static class NeighborBlock
    {
        // Immediate neighbors
        public const int Middle_W = 0;
        public const int Middle_E = 1;
        public const int Middle_N = 2;
        public const int Middle_S = 3;
        public const int Top_Center = 4;
        public const int Bottom_Center = 5;

        // Edge and corner neighbors
        public const int Top_NW = 6;
        public const int Top_NE = 7;
        public const int Top_SW = 8;
        public const int Top_SE = 9;
        public const int Top_W = 10;
        public const int Top_E = 11;
        public const int Top_N = 12;
        public const int Top_S = 13;

        public const int Bottom_NW = 14;
        public const int Bottom_NE = 15;
        public const int Bottom_SW = 16;
        public const int Bottom_SE = 17;
        public const int Bottom_W = 18;
        public const int Bottom_E = 19;
        public const int Bottom_N = 20;
        public const int Bottom_S = 21;

        public const int Middle_NW = 22;
        public const int Middle_NE = 23;
        public const int Middle_SW = 24;
        public const int Middle_SE = 25;
    }
}

