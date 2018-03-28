using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld.ScreenElements
{
    class TextConsoleScreen : InteractiveScreenElement
    {
        /// Console input string
        String consoleString;

        /// Console sprite font
        SpriteFont consoleFont;

        /// <summary>
        /// Setup text console
        /// </summary>
        public TextConsoleScreen(Game game, ScreenElement previousScreenElement, 
            GraphicsDevice graphicsDevice)
            : base(previousScreenElement, graphicsDevice)
        {
            // Initialize sprite resources
            consoleFont = game.Content.Load<SpriteFont>("Fonts/debug");
            consoleString = "";
        }

        public override void HandleInput(TimeSpan frameStepTime)
        {
            //if (NewKeyPressed(Keys.Escape))
            //    this.Exit();
        }

        public override void OnKeyUp(Keys key)
        {
            string toadd = key.ToString();
            if (!(key == Keys.None) && key != Keys.Space && key != Keys.Back && key != Keys.Enter)
            {
                consoleString += toadd;
            }
            else if (key == Keys.Space)
            {
                consoleString += " ";
            }
            else if (key == Keys.Back)
            {
                consoleString = consoleString.Remove(consoleString.Length - 1);
            }
        }

        /// <summary>
        /// Draw the console screen
        /// </summary>
        public override void Draw(TimeSpan frameStepTime)
        {
            spriteBatch.Begin();
            ColorRectangle(new Color(0, 0, 0, 0.7f), new Rectangle(0, graphicsDevice.Viewport.Height - 24, 
                graphicsDevice.Viewport.Width, 24), graphicsDevice, spriteBatch);

            spriteBatch.DrawString(consoleFont, consoleString, new Vector2(0, graphicsDevice.Viewport.Height - 24), 
                Color.White);

            spriteBatch.End();
            
            base.Draw(frameStepTime);
        }
    }
}
