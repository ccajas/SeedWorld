using System;
using System.Collections.Generic;
using System.Management;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld.ScreenElements
{
    /// <summary>
    /// Display debug statistics for the world
    /// </summary>
    class DebugViewScreen : DrawableScreenElement
    {
        private TextItem textItem;
        private WorldViewScreen.DebugStats debugStats;

        /// Storage for hardware info text
        List<string> CPUItems;

        /// <summary>
        /// Constructor for the DebugViewScreen
        /// </summary>
        public DebugViewScreen(ScreenElement previousScreen, ContentManager content,
            GraphicsDevice graphicsDevice, WorldViewScreen.DebugStats stats, Game game)
            : base(previousScreen, graphicsDevice)
        {
            this.previous = previousScreen;

            // Initialize debug resources
            SpriteFont debugFont = content.Load<SpriteFont>("Fonts/debug");
            SpriteFont alphBeta = content.Load<SpriteFont>("Fonts/alphbeta");

            textItem = new TextItem(Vector2.Zero, debugFont, spriteBatch);
            debugStats = stats;

            // Hardware stats
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            ManagementObjectCollection objCollection = searcher.Get();

            CPUItems = new List<string>();
            foreach (ManagementObject mObject in objCollection)
            {
                foreach (PropertyData property in mObject.Properties)
                {
                    if (property.Name == "Name")
                        CPUItems.Add(string.Format("{0}", property.Value));
                }
            }
        }

        /// <summary>
        /// Draw debug text info
        /// </summary>
        public override void Draw(TimeSpan frameStepTime)
        {
            // Draw debug text
            long totalMemory = GC.GetTotalMemory(false);
            spriteBatch.Begin(
                SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.Default,
                RasterizerState.CullCounterClockwise
            );

            textItem.Begin();
            textItem.DrawText(CPUItems[0]);

            textItem.SetDecimal(0).DrawText("Vertices: ", null, debugStats.vertexCount);
            textItem.DrawText("Memory: ", "KB", (int)(totalMemory / 1024));
            textItem.DrawText("Chunks added, loaded: ", null, debugStats.chunksAdded, debugStats.chunksLoaded);
            
            textItem.SetDecimal(4).DrawText("Camera pos: ", null, 
                debugStats.cameraPos.X, debugStats.cameraPos.Y, debugStats.cameraPos.Z);
            textItem.DrawText("Player pos: ", null,
                debugStats.playerPos.X, debugStats.playerPos.Y, debugStats.playerPos.Z);
            textItem.DrawText("Time of Day: ", null, debugStats.timeOfDay * 24f);

            spriteBatch.End();

            base.Draw(frameStepTime);
        }

        /// <summary>
        /// Helper to draw outlined text
        /// </summary>
        /*
        private void DrawText(SpriteBatch sb, StringBuilder text, Color backColor, Color frontColor, 
            float scale, float rotation, Vector2 position)
        {
            Vector2 origin = new Vector2(
                alphBeta.MeasureString(text).X / 2, 
                alphBeta.MeasureString(text).Y / 2
            );

            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 || y != 0)
                        sb.DrawString(alphBeta, text, position + new Vector2(x * scale, y * scale),
                            backColor, rotation, origin, scale, SpriteEffects.None, 0);
                }

            sb.DrawString(alphBeta, text, position, frontColor, rotation, origin, scale, SpriteEffects.None, 0);
        } */
    }
}
