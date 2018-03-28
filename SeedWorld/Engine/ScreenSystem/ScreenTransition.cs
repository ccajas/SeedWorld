using System;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    public enum ScreenStatus
    {
        Waiting,
        TransitionOn,
        Active,
        TransitionOff
    }

    /// <summary>
    /// Base class to contain behavioral features and functions
    /// for all GameScreen objects.
    /// </summary>
    public struct ScreenTransition
    {
        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float Position
        {
            get { return transitionPosition; }
        }

        float transitionPosition;

        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 1 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public float Alpha
        {
            get { return 1f - transitionPosition; }
        }

        /// How long to wait before transitioning
        public TimeSpan waitTime;

        /// How long the menu takes to transition on
        public TimeSpan onTime;

        /// How long the menu takes to transition off
        public TimeSpan offTime;

        /// <summary>
        /// Set transition times in constructor
        /// </summary>

        public ScreenTransition(TimeSpan transitionOnTime, TimeSpan transitionOffTime)
        {
            onTime = transitionOnTime;
            offTime = transitionOffTime;
            waitTime = TimeSpan.Zero;

            transitionPosition = 1;
        }

        /// <summary>
        /// Set transition times in constructor, in seconds using floats
        /// </summary>

        public ScreenTransition(float transitionOnTime, float transitionOffTime,
            float transitionWaitTime = 0f)
        {
            onTime = TimeSpan.FromSeconds(transitionOnTime);
            offTime = TimeSpan.FromSeconds(transitionOffTime);
            waitTime = TimeSpan.FromSeconds(transitionWaitTime);

            transitionPosition = 1;
        }

        /// <summary>
        /// Set a transition time for both entering and exiting
        /// </summary>

        public ScreenTransition(TimeSpan transitionTime, float transitionWaitTime = 0)
        {
            onTime = transitionTime;
            offTime = transitionTime;
            waitTime = TimeSpan.FromSeconds(transitionWaitTime);

            transitionPosition = 1;
        }

        /// <summary>
        /// Start transitioning again
        /// </summary>
        public void Reset()
        {
            transitionPosition = 1;
        }

        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>

        public bool Updating(TimeSpan frameStepTime, TimeSpan time, int direction)
        {
            // How much should we move by?
            float transitionDelta = (time == TimeSpan.Zero) ?
                1f : (float)frameStepTime.TotalMilliseconds / (float)time.TotalMilliseconds;

            // Update the transition position.
            transitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            // 1 = entered, 0 = exited
            if (((direction < 0) && (transitionPosition <= 0)) ||
                ((direction > 0) && (transitionPosition >= 1)))
            {
                transitionPosition = MathHelper.Clamp(transitionPosition, 0, 1);
                return false;
            }

            // Otherwise we are still busy transitioning.
            return true;
        }
    }
}
