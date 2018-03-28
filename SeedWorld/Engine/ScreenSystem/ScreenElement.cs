using System;
using System.Collections.Generic;
//using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    /// <summary>
    /// This empty interface designates which screens are "exclusive"
    /// in their input. Whenever an ExclusiveScreen is loaded, all other
    /// ScreenElements stop updating.
    /// </summary>
    public interface IExclusiveScreen { }

    /// <summary>
    /// Base for all the screen elements that contain separate logic for the game.
    /// </summary>
    public abstract class ScreenElement
    {
        /// Game reference
        protected Game game;

        /// Container of screen elements within this one
        protected List<ScreenElement> children;

        /// Previous and next screens in the list
        protected ScreenElement previous;
        protected ScreenElement nextScreen;

        public ScreenElement Previous
        {
            get { return previous; }
            set { previous = value; }
        }

        /// Transition data for this screen
        protected ScreenTransition transition = new ScreenTransition();

        public ScreenTransition Transition
        {
            get { return transition; }
            set { transition = value; }
        }

        /// <summary>
        /// Gets the current menu transition state.
        /// </summary>
        ScreenStatus screenStatus = ScreenStatus.Waiting;

        public ScreenStatus ScreenStatus
        {
            get { return screenStatus; }
        }

        /// Tell the screen to stop updating and reading input
        public bool disabled;

        /// Name of this screen
        public String screenHandle { protected set; get; }

        public ScreenElement() { }

        /// <summary>
        /// Constructor with default transition
        /// </summary>
        public ScreenElement(ScreenElement previousScreenElement)
        {
            previous = previousScreenElement;
            transition = new ScreenTransition(TimeSpan.Zero);
            children = new List<ScreenElement>();
        }

        /// <summary>
        /// All screens must check and update their transition state.
        /// </summary>
        private ScreenElement UpdateState(TimeSpan frameStepTime)
        {
            // Should only happen upon the first frame.
            if (screenStatus == ScreenStatus.Waiting)
                screenStatus = ScreenStatus.TransitionOn;

            if (screenStatus == ScreenStatus.TransitionOn)
            {
                // The screen should transition on and become active.
                if (transition.Updating(frameStepTime, transition.onTime, -1))
                {
                    // Still busy transitioning.
                    screenStatus = ScreenStatus.TransitionOn;
                }
                else
                {
                    // Transition finished.
                    screenStatus = ScreenStatus.Active;
                }
            }

            if (screenStatus == ScreenStatus.TransitionOff)
            {
                // Exiting screens should transition off.
                if (!transition.Updating(frameStepTime, transition.offTime, 1))
                {
                    // When the transition finishes, unload and return to the previous screen
                    UnloadContent();
                    return previous;
                }
            }

            // If a next screen is loaded, unload this screen and return it
            if (nextScreen != null)
            {
                ScreenElement screen = nextScreen;
                nextScreen = null;
                return screen;
            }

            return this;
        }

        /// <summary>
        /// Tell the screen to transition off
        /// </summary>
        public void Exit()
        {
            screenStatus = ScreenStatus.TransitionOff;
        }

        /// <summary>
        /// Tell the screen to transition on again
        /// </summary>
        public void Reset()
        {
            screenStatus = ScreenStatus.TransitionOn;
            transition.Reset();
        }

        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (screenStatus == ScreenStatus.TransitionOn ||
                        screenStatus == ScreenStatus.Active);
            }
        }

        /// <summary>
        /// Do all cleanup here when removing the screen
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// </summary>
        public virtual ScreenElement Update(TimeSpan frameStepTime)
        {
            foreach (ScreenElement screen in children)
                screen.Update(frameStepTime);

            return UpdateState(frameStepTime);
        }

        /// <summary>
        /// Draw contents of the screen
        /// </summary>
        public virtual void Draw(TimeSpan frameStepTime) 
        {
            foreach (ScreenElement screen in children)
                screen.Draw(frameStepTime);
        }

        /// <summary>
        /// Helper draws a translucent black fullmenu sprite, used for fading
        /// menus in and out, and for darkening the background behind popups.
        /// </summary>
        /*protected void FadeBackBufferToBlack(float alpha, GraphicsDevice graphicsDevice)
        {
            Viewport viewport = graphicsDevice.Viewport;

            /*
            if (alpha != 0f)
            {
                spriteBatch.Begin();
                graphicsHelper.ColorRectangle(Color.Black * alpha,
                    new Rectangle(0, 0, viewport.Width, viewport.Height), spriteBatch);
                spriteBatch.End();
            }
        } */
    }
}