using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld.ScreenElements
{
    class VoxelEditorScreen : InteractiveScreenElement
    {
        /// Camera that rotates around the voxel object
        PlayerCamera editorCamera = new PlayerCamera(20, 40);

        /// Displays grid for the editor
        GridRenderer gridRenderer;

        /// <summary>
        /// Editor Screen constructor
        /// </summary>
        public VoxelEditorScreen(Game game, ScreenElement previousScreenElement, ContentManager content,
            GraphicsDevice graphicsDevice) :
            base(game.Window, previousScreenElement, graphicsDevice)
        {
            editorCamera.Initialize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            editorCamera.SetChaseTarget(Vector3.Zero);

            gridRenderer = new GridRenderer(graphicsDevice);
            gridRenderer.SetGridSize(10);
        }

        /// <summary>
        /// Reads user input to use the editor
        /// </summary>
        public override void HandleInput(TimeSpan frameStepTime)
        {
            editorCamera.GetInput(inputState);
        }

        /// <summary>
        /// Update the voxel editor
        /// </summary>
        public override ScreenElement Update(TimeSpan frameStepTime)
        {
            if (NewKeyPressed(Keys.Escape))
                this.Exit();

            editorCamera.Update(frameStepTime);

            return base.Update(frameStepTime);
        }

        /// <summary>
        /// Draw the voxel editor and interface
        /// </summary>
        public override void Draw(TimeSpan frameStepTime)
        {
            game.GraphicsDevice.Clear(new Color(64, 64, 64));
            gridRenderer.Draw(spriteBatch.GraphicsDevice, editorCamera);

            base.Draw(frameStepTime);
        }
    }
}
