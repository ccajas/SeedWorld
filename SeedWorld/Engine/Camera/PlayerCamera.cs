using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld
{
    /// <summary>
    /// Controllable camera class
    /// </summary>
    public class PlayerCamera : Camera
    {
        /// <summary>
        /// Adjust smoothing to create a more fluid moving camera.
        /// Too much smoothing will cause a disorienting feel.
        /// </summary>
        private readonly float smoothing = 3.8f;
        private readonly float moveSpeed = 0.002f;
        private float minOffset = 0.1f;
        private float maxOffset = 1200f;

        public float TargetArcRotation { get { return targetArcRotation; } }

        /// Default position to keep the mouse pointer centered
        private Vector2 centerMousePos;

        /// Position of target to look at/follow
        private Vector3 targetPosition;
        public float offsetDistance;

        /// Check if camera is moving along the axes (no rotation)
        public bool isMoving { get; private set; }

        /// Incremental move speed per frame
        private float frameMoveSpeed;

        /// Toggle to disable camera movement
        public bool mouseDisabled;

        /// <summary>
        /// Set a default orientation in the constructor
        /// </summary>
        public PlayerCamera(float minOffset = 0.1f, float maxOffset = 12f)
        {
            cameraYawRotation = 0f;
            cameraArcRotation = 0f;

            targetYawRotation = 0f;
            targetArcRotation = 0f;

            offsetDistance = 7f;
            this.minOffset = minOffset;
            this.maxOffset = maxOffset;

            centerMousePos = viewAspect / 2;
        }

        /// <summary>
        /// Create a PlayerCamera with a specific orientation
        /// </summary>
        /// <param name="pos"></param>
        public PlayerCamera(Vector3 target, Vector2 orientation)
        {
            targetPosition = target;
            offsetDistance = 7f;

            cameraYawRotation = orientation.X;
            cameraArcRotation = orientation.Y;

            targetYawRotation = orientation.X;
            targetArcRotation = orientation.Y;
        }

        /// <summary>
        /// Set the camera's matrix transformations
        /// </summary>
        protected override void UpdateMatrices()
        {
            worldMatrix =
                Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(cameraArcRotation)) *
                Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(cameraYawRotation));

            position = targetPosition - worldMatrix.Forward * offsetDistance;
            view = Matrix.CreateLookAt(position, targetPosition, worldMatrix.Up);

            base.UpdateMatrices();
        }

        /// <summary>
        /// Update camera movement and matrices
        /// </summary>
        public override void Update(TimeSpan elapsed)
        {
            float elaspedTime = (float)elapsed.TotalMilliseconds;
            frameMoveSpeed = moveSpeed * elaspedTime;

            // Smooth the camera movement
            cameraArcRotation += targetArcRotation - (cameraArcRotation / smoothing);
            cameraYawRotation += targetYawRotation - (cameraYawRotation / smoothing);
            offsetDistance = MathHelper.Clamp(offsetDistance, minOffset, maxOffset);

            centerMousePos = viewAspect / 2;
            UpdateMatrices();
        }

        /// <summary>
        /// Update the position to be targeted by the camera
        /// </summary>
        public void SetChaseTarget(Vector3 target)
        {
            targetPosition = target;
        }

        /// <summary>
        /// Allows the camera to respond to input.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void GetInput(InputState input, bool resetMouse = true)
        {
            isMoving = false;
            if (mouseDisabled)
                return;

            //if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                targetYawRotation += (float)((input.lastMouseState.X - input.GetMouseState.X) * frameMoveSpeed);
                targetArcRotation += (float)((input.lastMouseState.Y - input.GetMouseState.Y) * frameMoveSpeed);
            }

            // Zoom in/out with mouse wheel
            offsetDistance -= (Mouse.GetState().ScrollWheelValue - input.lastMouseState.ScrollWheelValue) / 100f;

            // Handle rotation with joystick
            cameraArcRotation += input.gamePadState.ThumbSticks.Right.Y * frameMoveSpeed;
            cameraYawRotation += input.gamePadState.ThumbSticks.Right.X * frameMoveSpeed;

            // Limit the arc movement
            if (cameraArcRotation > 89.0f)
                cameraArcRotation = 89.0f;
            else if (cameraArcRotation < -89.0f)
                cameraArcRotation = -89.0f;

            // Reset mouse position
            if (resetMouse && new Vector2(input.GetMouseState.X, input.GetMouseState.Y) != centerMousePos)
                Mouse.SetPosition((int)centerMousePos.X, (int)centerMousePos.Y);
        }
    }
}