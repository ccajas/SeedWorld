using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeedWorld
{
    public class Terrain
    {
        /// <summary>
        /// Pre-determined set of steps to combine 2D noise patterns to form a terrain
        /// </summary>
        public static float GenerateHeightMap(int x, int y, int startFrequency, int octaves)
        {       
            float value = 0;
            float persistence = 0.1f;
            float j = startFrequency * 2f;

            // Add smooth ground
            for (float i = 0; i <= octaves; i++)
            {
                float noise = Noise.Simplex.Generate(
                    (float)((x / j) + 200f),
                    (float)((y / j) + 100f));

                value = value / 2f + 0.5f;
                value += noise * persistence;

                persistence *= 2;
                j *= 2;
            }

            float value2 = 0f;
            persistence = 0.2f;
            j = startFrequency / 2f;

            // Add rolling hills
            for (float i = 0; i <= octaves * 1; i++)
            {
                float noise2 = Math.Abs(Noise.Simplex.Generate(
                    (float)((x / j) + 100f),
                    (float)((y / j) + 300f)));

                j *= 2;
                value2 += noise2 * persistence;
                persistence *= 2f;
            }

            // Add some roughness to the hills
            value2 += Noise.Simplex.Generate(
                    (float)((x / 18f) + 100f),
                    (float)((y / 12f) + 300f)) / 25f;

            j /= 2;

            // Height variation on hills
            float blend = Noise.Simplex.Generate(
                    (float)((x / j) + 310f),
                    (float)((y / j) + 470f));
            blend += 1;

            value2 /= (octaves * 2);

            // Blend the two values
            blend = (1 - blend) + 1;
            blend *= 0.5f;
            blend = (float)Math.Pow(blend, 2);

            value2 *= blend;

            // Scale appropriately
            value = (value * 70f) + value2 * 400f;

            // Clamp to specific heights
            if (value > 511) value = 511;
            if (value < 0) value = 0;

            return value;
        }

        /// <summary>
        /// Pre-determined set of steps to combine 3D noise patterns
        /// </summary>
        public static float GenerateFromNoise(int x, int y, int z, int start, int octaves)
        {
            float value = 0;
            float persistence = 0.1f;
            float j = start * 2f;

            // Add smooth ground
            for (float i = 0; i <= octaves; i++)
            {
                float noise = Noise.Simplex.Generate(
                    (float)((x / j) + 200f),
                    (float)((y / (j * 2)) + 100f),
                    (float)((x / j) + 200f));

                value = value / 2f + 0.5f;
                value += noise * persistence;

                j *= 2;
                persistence *= 2f;
            }

            for (int i = 0; i < octaves; i++)
                value /= 2f;

            return value;
        }

        /// <summary>
        /// Get the slope by finding its immediate neighbor cells
        /// </summary>
        public float CalculateSlope(float[,] heightData, int x, int y)
        {
            // Compute steepness from height data
            double h = (double)heightData[x, y];

            // Compute the differentials by stepping over 1 in both directions.
            double dx = heightData[x + 1, y] - h;
            double dy = heightData[x, y + 1] - h;

            // The "steepness" is the magnitude of the gradient vector
            // For a faster but not as accurate computation, you can just use abs(dx) + abs(dy)
            float slope = (float)Math.Sqrt(dx * dx + dy * dy) / 2f;
            //slope = MathHelper.Clamp(slope, 0f, 1f);

            return slope;
        }
    }
}
