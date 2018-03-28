using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    public class Interpolate
    {
        /// <summary>
        /// Interpolation functions
        /// </summary>
        public static float Linear(float target, params float[] values)
        {
            return target * values[0] + (1.0f - target) * values[1];
        }

        public static float Bilinear(float[] target, params float[] values)
        {
            var prime = new[]
        {
            Linear(target[1], values),
            Linear(target[1], values.Skip(2).Take(2).ToArray())
        };

            return Linear(target[0], prime);
        }

        public static float Trilinear(float[] target, params float[] values)
        {
            var prime = new[]
            {
                Bilinear(target, values),
                Bilinear(target.Skip(1).ToArray(), values.Skip(4).ToArray())
            };

            return Linear(target[2], prime);
        }
    }
}
