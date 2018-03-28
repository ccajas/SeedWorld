using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld
{
    public class InputState
    {
        // Previous input states
        public KeyboardState lastKeyboardState;
        public MouseState lastMouseState;
        public GamePadState lastGamePadState;
        public Keys[] lastPressedKeys = new Keys[0];

        // Current input states
        public KeyboardState keyboardState;
        public MouseState mouseState;
        public GamePadState gamePadState;

        /// <summary>
        /// Take the current keyboard input status
        /// </summary>
        public void PollKeyboardInput(PlayerIndex activePlayer)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(activePlayer);
        }

        /// <summary>
        /// Take the current mouse input status
        /// </summary>
        public void PollMouseInput()
        {
            mouseState = Mouse.GetState();
        }

        /// <summary>
        /// Store last input status
        /// </summary>
        public void StoreLastInput()
        {
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;
            lastGamePadState = gamePadState;
        }

        /// <summary>
        /// Get the current mouse state
        /// </summary>
        public MouseState GetMouseState
        {
            get { return Mouse.GetState(); }
        }
    }
}
