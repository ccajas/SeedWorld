using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    public class Biome
    {
        /// List of base colors for various biomes
        /// Surface colors
        private Color temperateColor1 = new Color(0x23, 0x7f, 0);
        private Color temperateColor2 = new Color(0x9f, 0xd5, 0x17);
        private Color desertColor1 = new Color(0xf7, 0xe8, 0x75);
        private Color desertColor2 = new Color(0xf7, 0xb3, 0x58);

        /// Rock colors
        private Color desertRockColor1 = new Color(0xd7, 0xc8, 0x85);
        private Color desertRockColor2 = new Color(0xc7, 0x93, 0x48);

        /// Noise frequency coefficients for different maps
        private float humidityMapNoise1 = 1500f;
        private float humidityMapNoise2 = 400f;
        private readonly int heightMapNoise = 120;

        /// Location in the world
        private Point worldOffset;
        private int areaSize;

        /// Terrain building object
        private Terrain terrain = new Terrain();
        public Terrain Terrain
        {
            get { return terrain; }
        }

        /// Humidity map for sources of water
        private float[,] sourceHumidity;
        public float[,] SourceHumidity
        {
            get { return sourceHumidity; }
        }

        /// <summary>
        /// Height modifier
        /// </summary>
        private float[,] sourceHeight;
        public float[,] SourceHeight
        {
            get { return sourceHeight; }
        }

        /// Mask for light/shadowing
        public byte[,] lightMask;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public Biome(Point offset, int size) 
        {
            sourceHumidity = new float[size + 2, size + 2];
            sourceHeight = new float[size + 2, size + 2];
            lightMask = new byte[size + 2, size + 2];

            worldOffset = offset;
            areaSize = size;
        }

        /// <summary>
        /// Reset the location for biome data
        /// </summary>
        public void SetLocation(Point offset)
        {
            int areaSize = lightMask.GetLength(0);
            lightMask = new byte[areaSize, areaSize];
            worldOffset = offset;
        }

        /// <summary>
        /// Compute base surface humidity
        /// </summary>
        public float BaseHumidity(int x, int y)
        {
            float persistence = 0.4f;

            // Combine two noise patterns for a detailed humidity map
            float humidity = Noise.Simplex.Generate(
                x / humidityMapNoise1, y / humidityMapNoise1);
            humidity += Noise.Simplex.Generate(
                x / humidityMapNoise2, y / humidityMapNoise2) * persistence;

            // Clamp noise value to (-1, 1)
            humidity = MathHelper.Clamp(humidity + 0.2f, -1, 1);

            // Normalize to (0, 1)
            return (humidity / 2f) + 0.5f;
        }

        /// <summary>
        /// Base height from a 2D point in the world
        /// </summary>
        public float GetBaseHeight(int x, int z, bool saveToMap)
        {
            float height = Terrain.GenerateHeightMap(
                (int)(worldOffset.X + x), (int)(worldOffset.Y + z), heightMapNoise, 3);

            // Generate the height from this area
            if (saveToMap)
                sourceHeight[x, z] = height;

            return height;
        }

        /// <summary>
        /// Compute additive humidity around a water source
        /// </summary>
        public float GetLocalHumidity(int x, int z, float minThreshold, bool saveToMap)
        {
            if (!saveToMap)
                return sourceHumidity[x, z];

            // Create an alternate height for river banks. 
            // Higher altitudes will have narrower streams
            float maxRiverHeight = 120f;

            // Create noise for the river depths
            float noise = Noise.Simplex.Generate(
                (float)(((worldOffset.X + x) / (float)(heightMapNoise * 24f)) + 200f),
                (float)(((worldOffset.Y + z) / (float)(heightMapNoise * 24f)) + 200f));
            noise += Noise.Simplex.Generate(
                (float)(((worldOffset.X + x) / (heightMapNoise * 4f)) + 200f),
                (float)(((worldOffset.Y + z) / (heightMapNoise * 4f)) + 200f)) / 5f;
            noise += Noise.Simplex.Generate(
                (float)(((worldOffset.X + x) / (heightMapNoise / 2f)) + 200f),
                (float)(((worldOffset.Y + z) / (heightMapNoise / 2f)) + 200f)) / 50f;

            int yHeight = (int)sourceHeight[x, z];

            // Increase humidity around river areas
            noise *= MathHelper.Lerp(0, 1.5f, yHeight / maxRiverHeight);
            noise = (float)Math.Pow(Math.Abs(noise), 0.7f);

            // Group noise values together
            float humidity = 1 - (noise - minThreshold);
            sourceHumidity[x, z] = (float)Math.Pow(humidity, 12f) * 1.5f;

            return sourceHumidity[x, z];
        }

        /// <summary>
        /// Get default Surface block color using 2D noise
        /// </summary>
        public uint GetSurfaceColor(int x, int z)
        {
            // Noise pattern for colors
            float noise = Noise.Simplex.Generate(
                (float)(x / 100f) + 3100f,
                (float)(z / 100f) + 4700f);
            noise = (noise / 2f) + 0.5f;
            noise = MathHelper.Clamp(noise, 0, 1);

            // Mix color values base on noise value

            // First measure humidity and get color intensity for each humidity range
            int nx = Math.Abs((x - 1) % areaSize);
            int nz = Math.Abs((z - 1) % areaSize);
            float humidity = 0f + sourceHumidity[nx, nz];// 0 //BaseHumidity(x, z);

            Color desertColor = Color.Lerp(desertColor1, desertColor2, noise);
            Color temperateColor = Color.Lerp(temperateColor1, temperateColor2, noise);

            // Blend biome colors to get final color
            Color surfaceColor = Color.Lerp(desertColor, temperateColor, humidity);

            // Mixed color for certain block types
            uint color = (uint)(surfaceColor.B << 24) |
                    (uint)(surfaceColor.G << 16) |
                    (uint)(surfaceColor.R << 8);

            return color;
        }

        /// <summary>
        /// Get default Rock block color using 3D noise
        /// </summary>
        public uint GetRockColor(int x, int y, int z)
        {
            // Noise pattern for colors
            float noise = Noise.Simplex.Generate(
                (float)(x / 100f) + 3100f,
                (float)(y / 6f) + 4700f,
                (float)(z / 100f) + 2200f);

            noise = (noise / 2f) + 0.5f;
            noise = MathHelper.Clamp(noise, 0, 1);

            // Mix color values base on noise value
            // Blend biome colors to get final color
            Color rockColor = Color.Lerp(desertRockColor1, desertRockColor2, noise);

            // Mixed color for certain block types
            uint color = (uint)(rockColor.B << 24) |
                    (uint)(rockColor.G << 16) |
                    (uint)(rockColor.R << 8);

            return color;
        }

        /// <summary>
        /// Get color of the block depending on its type
        /// </summary>
        public uint GetBlockColor(BlockType block, int x, int y, int z)
        {
            // Default color
            uint color = 0xffffff00;

            BlockType blockType = (BlockType)((int)block & 0xf);

            if (block == BlockType.Surface)
                color = GetSurfaceColor(x, z);
            if (blockType == BlockType.Dirt)
                color = GetRockColor(x, y, z); //0x00448800;
            if (blockType == BlockType.Wood)
                color = 0x00447700;
            if (blockType == BlockType.Water)
                color = 0xcc004480; // A special flag is added for water in the 7th bit
            if (blockType == BlockType.Foliage_1)
                color = 0x00880000;
            if (blockType == BlockType.Foliage_2)
                color = 0x0077ff00;
            if (blockType == BlockType.Foliage_3)
                color = 0x00ffcc00;
            if (blockType == BlockType.Foliage_4)
                color = 0x00995500;

            return color;
        }
    }
}
