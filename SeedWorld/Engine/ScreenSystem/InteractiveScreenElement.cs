using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld
{
    /// <summary>
    /// A screen element that can receive user input.
    /// </summary>
    public abstract class InteractiveScreenElement : DrawableScreenElement
    {
        /// Keyboard and mouse input
        protected InputState inputState;

        /// Dispatcher to subscribe to input controls
        protected KeyboardDispatcher kbDispatcher;

        /// Active player
        protected PlayerIndex activePlayer;

        /// Console for all interactive screens
        protected GUI.ConsoleManager console;

        /// <summary>
        /// Constructor with default state setup
        /// </summary>
        public InteractiveScreenElement(ScreenElement previousScreenElement, 
            GraphicsDevice graphicsDevice, PlayerIndex playerIndex = PlayerIndex.One) :
            base(previousScreenElement, graphicsDevice)
        {
            activePlayer = playerIndex;
            inputState = new InputState();
            //kbDispatcher = new KeyboardDispatcher(window);
            console = new GUI.ConsoleManager();

            // Set default last states for first frame
            inputState.lastKeyboardState = Keyboard.GetState();
            inputState.lastMouseState = Mouse.GetState();
            inputState.lastGamePadState = GamePad.GetState(activePlayer);
        }

        /// <summary>
        /// Check if a new key has been pressed in this update
        /// </summary>
        public bool NewKeyPressed(Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key) && !inputState.lastKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Used for all the input logic
        /// </summary>
        public abstract void HandleInput(TimeSpan frameStepTime);

        public virtual void OnKeyUp(Keys key) { }
        public virtual void OnKeyDown(Keys key) { }

        /// <summary>
        /// Input logic gets called before the Update function
        /// </summary>
        public override ScreenElement Update(TimeSpan frameStepTime)
        {
            inputState.PollKeyboardInput(activePlayer);
            this.HandleInput(frameStepTime);

            Keys[] pressedKeys = inputState.keyboardState.GetPressedKeys();

            //check if any of the previous update's keys are no longer pressed
            foreach (Keys key in inputState.lastPressedKeys)
            {
                if (!pressedKeys.Contains(key))
                    OnKeyUp(key);
            }

            //check if the currently pressed keys were already pressed
            foreach (Keys key in pressedKeys)
            {
                if (!inputState.lastPressedKeys.Contains(key))
                    OnKeyDown(key);
            }

            inputState.lastPressedKeys = pressedKeys;

            inputState.PollMouseInput();
            inputState.StoreLastInput();

            return base.Update(frameStepTime);
        }
    }
}
