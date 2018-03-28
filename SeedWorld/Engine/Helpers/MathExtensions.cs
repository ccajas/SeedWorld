using System;

namespace SeedWorld
{
    /// <summary>
    /// A set of extra Math functions
    /// </summary>
    public static class MathExtra
    {
        /// <summary>
        /// True modulus function (always returns positive)
        /// C# '%' is really a remainder operator
        /// </summary>
        public static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        /// <summary>
        /// Return integer value for floor
        /// </summary>
        public static int FastFloor(float x)
        {
            return (x > 0) ? (int)x : ((int)x) - 1;
        }

        /// <summary>
        /// Return integer value for ceiling
        /// </summary>
        public static int FastCeiling(float x)
        {
            return (x > (int)x) ? ((int)x) + 1 : (int)x;
        }
    }   
}
