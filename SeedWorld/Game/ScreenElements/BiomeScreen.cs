using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld.ScreenElements
{
    class BiomeScreen : InteractiveScreenElement
    {
        /// Resources
        Texture2D biomeTexture;
        Vector2[] locations;
        Color[] locationColors;
        Color[] mapColors;
        Random rnd;

        /// Map information
        int mapSize = 480;
        int biomeCount = 1600;
        int seed = 123;

        /// Check progress of map
        bool mapDone;
        int nextPoint;

        /// <summary>
        /// Constructor for the BiomeScreen
        /// </summary>
        public BiomeScreen(ScreenElement previousScreenElement, ContentManager content,
            GraphicsDevice graphicsDevice, Game game) :
            base(previousScreenElement, graphicsDevice)
        {
            this.game = game;

            biomeTexture = new Texture2D(graphicsDevice, mapSize, mapSize);
            mapDone = false;

            // Initialize list of locations and their color
            locations = new Vector2[biomeCount];
            locationColors = new Color[biomeCount];

            rnd = new Random(seed);
            Noise.Simplex.Seed(seed);
            mapColors = new Color[mapSize * mapSize];
        }

        /// <summary>
        /// Add the biomes to the map
        /// </summary>
        private void PlotBiomes(Color[] mapColors)
        {
            if (nextPoint == 0)
            {
                // Add biome centers
                for(int i = 0; i < biomeCount; i++)
                {
                    locations[i] = new Vector2(rnd.Next(mapSize), rnd.Next(mapSize));

                    // Combine two noise patterns for a detailed terrain
                    float avgHeight = ComputeTerrainHeight(locations[i]);

                    // Give the biome cells land or water colors
                    if (avgHeight > 0)
                        locationColors[i] = new Color(0, (avgHeight * 0.7f) + 0.3f, 0f);
                    else
                        locationColors[i] = new Color(0.7f + avgHeight, 0.7f + avgHeight, 1f + (avgHeight / 4f));
                }
            }

            // Plot pixels defining their position to the closest biome
            // A simple Voronoi diagram generator!
            Parallel.For(nextPoint, nextPoint + 3200, i =>
            {
                // Get x and y locations for texture
                int x = i % mapSize;
                int y = i / mapSize;

                Vector2 point = new Vector2(x, y);
                
                float d = 1000000f;
                int closest = 0;

                for (int j = 0; j < locations.Length; ++j)
                {
                    float dist = Vector2.DistanceSquared(locations[j], point);
                    if (dist < d)
                    {
                        d = dist;
                        closest = j;
                    }
                }

                mapColors[i] = locationColors[closest];
            });

            nextPoint += 3200;

            if (nextPoint >= mapColors.Length)
                mapDone = true;
        }

        /// <summary>
        /// Get the terrain height for a point in the map
        /// </summary>
        private float ComputeTerrainHeight(Vector2 location)
        {
            // Combine two noise patterns for a detailed terrain
            float height = Noise.Simplex.Generate(location.X / 150f, location.Y / 150f);
            height += Noise.Simplex.Generate(location.X / 40f, location.Y / 40f) * 0.4f;

            // Clamp noise value to (-1, 1)
            height = MathHelper.Clamp(height / 2f + 0.2f, -1, 1);

            // Radial mask for making edges water areas
            //float distToCenter = Vector2.Distance(location, new Vector2(mapSize / 2, mapSize / 2));
            //float heightMask = (distToCenter / (mapSize / 10f)) - 3.5f;

            // A soft square mask for making edges water areas
            float heightMask1 = MathHelper.Clamp(((location.X * 4f) / mapSize) - 3f, 0, 1);
            float heightMask2 = MathHelper.Clamp((1 - ((location.X * 4f) / mapSize)), 0, 1);
            float heightMask3 = MathHelper.Clamp(((location.Y * 4f) / mapSize) - 3f, 0, 1);
            float heightMask4 = MathHelper.Clamp((1 - ((location.Y * 4f) / mapSize)), 0, 1);

            float heightMask = heightMask1 + heightMask2 + heightMask3 + heightMask4;

            heightMask = MathHelper.Clamp(heightMask, 0, 1);
            height -= heightMask;

            return height;
        }

        /// <summary>
        /// Reads user input to load a new map or exit the program
        /// </summary>
        public override void HandleInput(TimeSpan frameStepTime)
        {
            //if (Keyboard.GetState().IsKeyDown(Keys.A))
            if (NewKeyPressed(Keys.A))
            {
                seed = rnd.Next(1000000);
                rnd = new Random(seed);
                Noise.Simplex.Seed(seed);

                mapColors = new Color[mapSize * mapSize];
                nextPoint = 0;
                mapDone = false;
            }

            // Exit program
            if (NewKeyPressed(Keys.Escape))
                this.Exit();
        }

        /// <summary>
        /// Updates the program and reads input
        /// </summary>
        public override ScreenElement Update(TimeSpan frameStepTime)
        {
            // Start or continue plotting the biome map
            if (!mapDone)
            {
                PlotBiomes(mapColors);
                biomeTexture.SetData(mapColors);
            }

            return base.Update(frameStepTime);
        }

        /// <summary>
        /// Draws the map
        /// </summary>
        public override void Draw(TimeSpan frameStepTime)
        {
            //graphicsDevice.Clear(Color.CornflowerBlue * 0.5f);

            spriteBatch.Begin();
            spriteBatch.Draw(biomeTexture, new Vector2(0, 100), Color.White);
            spriteBatch.End();

            base.Draw(frameStepTime);
        }
    }
}
