using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Management;
using System.Management.Instrumentation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using NuclearWinter;
using NuclearUI = NuclearWinter.UI;

namespace SeedWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : NuclearGame //Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Rendering resources and services
        /// </summary>
        SpriteFont debugFont;
        StringBuilder debugString;

        /// Diagnostic tool
        Stopwatch stopWatch = new Stopwatch();
        Stopwatch stopWatch2 = new Stopwatch(); 

        /// Current screen to update
        ScreenElement currentScreen, nextScreen;

        public Game()
        {
            Content.RootDirectory = "Content";
            debugString = new StringBuilder(64, 64);

            // Setup starting graphics properties
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
            Graphics.PreferMultiSampling = true;
            //graphics.IsFullScreen = true;
            //graphics.SynchronizeWithVerticalRetrace = false;
            //this.IsFixedTimeStep = false;

            Graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Allow resizable window
            this.Window.AllowUserResizing = true;

            // Add window handlers
            Form.Resize += delegate { EnsureProperPresentationParams(); };
            Exiting += delegate(Object o, EventArgs e)
            {
                if (currentScreen != null)
                    currentScreen.UnloadContent();
            };

            base.Initialize();
        }

        /// <summary>
        /// Keep presentation parameters consistent
        /// </summary>
        void EnsureProperPresentationParams()
        {
            if (Form.ClientSize.IsEmpty) return;

            if (Form.ClientSize.Width != GraphicsDevice.Viewport.Width
            || Form.ClientSize.Height != GraphicsDevice.Viewport.Height)
            {
                var updatedPresentationParams = GraphicsDevice.PresentationParameters.Clone();
                updatedPresentationParams.BackBufferWidth = Form.ClientSize.Width;
                updatedPresentationParams.BackBufferHeight = Form.ClientSize.Height;
                GraphicsDevice.Reset(updatedPresentationParams);
            }
        }

        /// <summary>
        /// Handle window resizing
        /// </summary>
        private void WindowClientSizeChanged(object sender, EventArgs e)
        {
            // Remove this event handler, so we don't call it when we change the window size in here
            Window.ClientSizeChanged -= new EventHandler<EventArgs>(WindowClientSizeChanged);

            int newWidth = Graphics.GraphicsDevice.Viewport.Width;
            int newHeight = Graphics.GraphicsDevice.Viewport.Height;
             
            // Update the GD parameters
            Graphics.PreferredBackBufferWidth = newWidth;
            Graphics.PreferredBackBufferHeight = newHeight;
            Graphics.ApplyChanges();

            this.GraphicsDevice.PresentationParameters.BackBufferWidth = newWidth;
            this.GraphicsDevice.PresentationParameters.BackBufferHeight = newHeight;

            // Add the event handler again
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(WindowClientSizeChanged);        
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            debugFont = Content.Load<SpriteFont>("Fonts/debug");

            // Launch the first screen.
            currentScreen = new ScreenElements.SettingsScreen(this, null);
            currentScreen = new ScreenElements.WorldViewScreen(this, null);
        }

        /// <summary>
        /// UnloadContent will be called once per game state
        /// </summary>
        protected override void UnloadContent()
        {
            // Check if game data needs to be saved
        }

        /// <summary>
        /// A way to toggle fullscreen while rebuilding necessary resources
        /// </summary>
        public void ToggleFullScreen()
        {
            Graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            Graphics.ToggleFullScreen();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            stopWatch.Restart();

            // Allows the game to exit
            if (currentScreen == null)
            {
                this.Exit();
            }
            else
            {
                // Update screens
                nextScreen = currentScreen.Update(gameTime.ElapsedGameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Start drawing the ScreenElements
            base.Draw(gameTime);

            // Draw the current screen
            currentScreen.Draw(gameTime.ElapsedGameTime);

            SpriteBatch.Begin();
            debugString.Append("Elapsed update time: ").Concat(stopWatch.ElapsedMilliseconds, 0);
            SpriteBatch.DrawString(debugFont, debugString,
                new Vector2(GraphicsDevice.Viewport.Width - 160f, 0), Color.White);
            debugString.Clear();
            SpriteBatch.End();

            // Swap screen for the next frame
            if (nextScreen != currentScreen)
                currentScreen = nextScreen;

            stopWatch.Stop();
        }
    }
}
