using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    public abstract class DrawableScreenElement : ScreenElement
    {
        /// Viewport and graphics system
        protected Viewport viewport;
        protected GraphicsDevice graphicsDevice;

        /// Spritebatch reference
        protected SpriteBatch spriteBatch;
        
        /// ModelBatch reference
        protected ModelBatch modelBatch;

        /// Resource managers
        protected VoxelMeshManager voxelContent;

        // Background pixel texture 
        private static Texture2D pixel;
        private static Color[] colorData = { Color.White };  

        /// <summary>
        /// Creates the ScreenElement
        /// </summary>
        public DrawableScreenElement(ScreenElement previousScreenElement, GraphicsDevice graphicsDevice) :
            base(previousScreenElement)
        {
            // Add graphics systems
            this.graphicsDevice = graphicsDevice;
            this.viewport = graphicsDevice.Viewport;

            // Create a SpriteBatch and ModelBatch
            spriteBatch = new SpriteBatch(graphicsDevice);
            modelBatch = new ModelBatch(graphicsDevice);

            // Create resource managers
            voxelContent = new VoxelMeshManager(graphicsDevice);

            // Handle device reset
            graphicsDevice.DeviceReset += OnDeviceReset;
        }

        /// <summary>
        /// Make a solid color rectangle
        /// </summary>
        public void ColorRectangle(Color color, Rectangle rect,
            GraphicsDevice device, SpriteBatch spriteBatch)
        {
            // Make a 1x1 texture named pixel and set the color data.
            pixel = new Texture2D(device, 1, 1);
            pixel.SetData<Color>(colorData);

            // Draw a fancy rectangle.  
            spriteBatch.Draw(pixel, rect, color);
        }

        /// <summary>
        /// Called when the graphics device is reset
        /// </summary>
        protected virtual void OnDeviceReset(Object sender, EventArgs args) { }

        /// <summary>
        /// Unload and remove attached events
        /// </summary>
        public override void UnloadContent()
        {
            // Remove device reset handler
            graphicsDevice.DeviceReset -= OnDeviceReset;

            base.UnloadContent();
        }
    }
}
