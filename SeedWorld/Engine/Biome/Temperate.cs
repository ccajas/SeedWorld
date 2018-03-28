using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld.Engine
{
    public class TemperateBiome
    {
        public TemperateBiome()
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(-10, 10);
            Vector2 p3 = new Vector2(-10, -10);
            Vector2 p4 = new Vector2(10, -10);
            Vector2 p5 = new Vector2(10, 10);

            List<Vector2> pts = new List<Vector2>() { p1, p2, p3, p4, p5 };
            Random rnd = new Random(234);

            // Simplest Voronoi diagram generator!
            for (int i = 0; i < 5000; i++)
            {
                Vector2 point = new Vector2((float)rnd.NextDouble() * 10f, (float)rnd.NextDouble() * 10f);
                float d = 100000f;

                Vector2 ptClose = new Vector2();
                foreach (Vector2 pt in pts)
                {
                    float dist2 = Vector2.DistanceSquared(pt, point);
                    if (dist2 < d)
                        ptClose = pt;
                }
                bool inCell = (ptClose == point);
            }
        }

        /// <summary>
        /// Compute the block color
        /// </summary>
        /// <param name="blockType"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void GetBlockColor(BlockType blockType, int x, int z)
        {
            // Get noise values that serve as basis of color gradients
            float value = Noise.Simplex.Generate(
                (float)(x / 60f) + 1100f,
                (float)(z / 60f));

            float value2 = Math.Abs(Noise.Simplex.Generate(
                (float)(x / 140f) + 1100f,
                (float)(z / 140f)));

            value = value / 2f + 0.5f;

            switch (blockType)
            {
                case BlockType.Surface:

                    break;
            }
            byte red =   (byte)((1 - value) * 0x7f + value * 0x2f);
            byte green = (byte)((1 - value) * 0xff + value * 0x7f);
            byte blue =  (byte)((1 - value) * 0x00 + value2 * 0x3f);

            // Apply shadow mask

            uint color;

            if (blockType == BlockType.Foliage_1)
            {
                red /= 3;
                green /= 2;
            }

            color = (uint)(blue << 24) + (uint)(green << 16) + (uint)(red << 8);

            if (blockType == BlockType.Wood)
                color = 0x22475200;
        }

    }
}
