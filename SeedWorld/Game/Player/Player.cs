using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld
{
    class Player
    {
        /// <summary>
        /// Serializable player data
        /// </summary>
        [Serializable]
        public class PlayerData
        {
            /// Physics state
            public Vector3 position;
            public float orientation;

            /// Stats data
            public int playerHP;

            /// Level data
            public int level;
            public int playerXP;
        }

        public PlayerData playerData
        {
            get; private set;
        }

        public Matrix worldTransform;

        /// Current user movement input
        private Vector2 movement;
        private float verticalMovement;

        /// Physics state and player dimensions
        private Vector3 velocity;
        private Vector3 bBoxCenter;

        /// Jumping state
        public bool isJumping 
        { 
            get; private set;      
        }

        public bool wasJumping 
        { 
            get; private set; 
        }

        public bool isOnGround 
        { 
            get; private set; 
        }

        // Constants for controling horizontal movement
        private const float moveSpeed = 16f;

        // Constants for controlling vertical movement

        /// Input configuration
        private const float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;

        /// Bounding box for world space
        private BoundingBox playerBounds;
        public BoundingBox PlayerBounds 
        { 
            get { return playerBounds; } 
        }

        /// Bounding box in local space, which can't be changed
        private readonly BoundingBox localBounds;
        public BoundingBox LocalBounds
        {
            get { return localBounds; }
        }

        /// Visual sprite/avatar for player
        private MeshData mesh;
        public MeshData Mesh
        {
            get { return mesh; }
        }

        public List<BoundingBox> collidedBlocks;
        private Object lockObject = new object();

        /// <summary>
        /// Constructor for a new player
        /// </summary>
        public Player(MeshData mesh, PlayerData playerData = null)
        {
            // Load player data
            this.playerData = playerData ?? new PlayerData();
            this.bBoxCenter = (mesh.bBox.Max + mesh.bBox.Min) / 2;

            // Set local bounds (fixed)
            localBounds = new BoundingBox(mesh.bBox.Min - bBoxCenter, mesh.bBox.Max - bBoxCenter);

            float meshScale = 0.125f;
            localBounds.Min *= meshScale;
            localBounds.Max *= meshScale;

            // Add mesh for player
            this.mesh = mesh;

            Reset(playerData.position);
        }

        /// <summary>
        /// Resets the player's status
        /// </summary>
        public void Reset(Vector3 position)
        {
            this.playerData.position = position;
            velocity = Vector3.Zero;
            collidedBlocks = new List<BoundingBox>();

            // Create a world space position bounding box
            playerBounds.Min = position + localBounds.Min;
            playerBounds.Max = position + localBounds.Max;
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        public void Update(TimeSpan frameStepTime, Vector2 moveVector, ChunkManager chunks)
        {
            DoPhysics(frameStepTime, moveVector, chunks);
            playerData.orientation = (float)Math.Atan2(-moveVector.Y, moveVector.X);

            if (isOnGround) // && IsAlive
            {
                // TODO: animations
            }

            // Clear input.
            movement = Vector2.Zero;

            // Update transform matrix and bounding box
            worldTransform =
                Matrix.CreateTranslation(-bBoxCenter) *
                Matrix.CreateScale(0.125f) *
                Matrix.CreateRotationY(playerData.orientation) *
                Matrix.CreateTranslation(playerData.position);
            mesh.bBox = playerBounds;

            isJumping = false;
        }

        /// <summary>
        /// Gets player horizontal movement and update states according to input.
        /// </summary>
        public void GetInput(InputState input)
        {
            // Get analog horizontal movement.
            movement.Y = input.gamePadState.ThumbSticks.Left.Y * MoveStickScale;
            movement.X = input.gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement.X) < 0.5f && Math.Abs(movement.Y) < 0.5f)
                movement = Vector2.Zero;

            // If any digital horizontal movement input is found, override the analog movement.
            if (input.gamePadState.IsButtonDown(Buttons.DPadUp) ||
                input.keyboardState.IsKeyDown(Keys.S))
            {
                movement.Y -= 1.0f;
            }
            if (input.gamePadState.IsButtonDown(Buttons.DPadDown) ||
                input.keyboardState.IsKeyDown(Keys.W))
            {
                movement.Y += 1.0f;
            }
            if (input.gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                input.keyboardState.IsKeyDown(Keys.A))
            {
                movement.X += 1.0f;
            }
            if (input.gamePadState.IsButtonDown(Buttons.DPadRight) ||
                input.keyboardState.IsKeyDown(Keys.D))
            {
                movement.X -= 1.0f;
            }

            // Normalize move vector for diagonal movement
            if (Math.Abs(movement.X) + Math.Abs(movement.Y) >= 2)
                movement.Normalize();

            // Speed up
            if (input.keyboardState.IsKeyDown(Keys.E))
                movement *= 10f;

            // Move down
            if (input.keyboardState.IsKeyDown(Keys.LeftShift))
                verticalMovement -= 1.0f;

            // Check if the player wants to jump.
            if (input.keyboardState.IsKeyDown(Keys.Space) && !input.lastKeyboardState.IsKeyDown(Keys.Space))
            {
                isJumping = true;
            }

            // Mouse button input
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void DoPhysics(TimeSpan frameStepTime, Vector2 directionVector, ChunkManager chunks)
        {
            float elapsed = (float)frameStepTime.TotalSeconds;
            velocity.X = 0;
            velocity.Z = 0;
            directionVector.Normalize();

            // Gravity effect
            float gravityVelocity = -28f;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement.Y * directionVector.X * moveSpeed;
            velocity.Z += movement.Y * directionVector.Y * moveSpeed;

            velocity.X += movement.X * directionVector.Y * moveSpeed;
            velocity.Z += movement.X * -directionVector.X * moveSpeed;

            // Vertical movement is simpler
            if (isJumping)
            {
                verticalMovement = 15.0f;
                velocity.Y = verticalMovement;
            }

            velocity.Y += gravityVelocity * elapsed;

            // Apply velocity.
            playerData.position += velocity * elapsed;

            // If the player is now colliding with the level, separate them.
            HandleCollisions(chunks);
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        
        /*private void DoJump(TimeSpan frameStepTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && isOnGround) || jumpTime > 0.0f)
                {
                    //if (jumpTime == 0.0f)
                    //    jumpSound.Play();

                    jumpTime += (float)frameStepTime.TotalSeconds;
                    //sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocity.Y = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;
        }*/

        /// <summary>
        /// Detects and resolves collisions from neighboring blocks.
        /// </summary>
        private List<BoundingBox> HandleCollisions(ChunkManager chunks)
        {
            playerBounds.Min = playerData.position + localBounds.Min;
            playerBounds.Max = playerData.position + localBounds.Max;

            BoundingBox bounds = playerBounds;

            int leftBlock = (int)Math.Floor(bounds.Min.X);
            int rightBlock = (int)Math.Ceiling(bounds.Max.X);
            int bottomBlock = (int)Math.Floor(bounds.Min.Y);
            int topBlock = (int)Math.Ceiling(bounds.Max.Y);
            int backBlock = (int)Math.Floor(bounds.Min.Z);
            int frontBlock = (int)Math.Ceiling(bounds.Max.Z);

            BoundingBox blockBounds = new BoundingBox();
            collidedBlocks.Clear();

            Vector3 position = playerData.position;
            BlockType block = BlockType.Empty;

            // For each potentially colliding block
            for (int y = bottomBlock; y < topBlock; ++y)
            {
                for (int x = leftBlock; x < rightBlock; ++x)
                {
                    for (int z = backBlock; z < frontBlock; ++z)
                    {
                        block = (BlockType)chunks.GetVoxelAt(x - 1, y, z - 1);

                        // Check only non-empty blocks
                        if (block != BlockType.Empty)
                        {
                            blockBounds.Min = new Vector3(x, y, z);// +localBounds.Min;
                            blockBounds.Max = new Vector3(x + 1, y + 1, z + 1);// +localBounds.Max;

                            //float intDepth = blockBounds.Intersects(position, position);

                            Vector3 depth = blockBounds.GetIntersectionDepth(bounds);
                            // Resolve the collision along the Y axis.
                            if (depth != Vector3.Zero)
                            {
                                collidedBlocks.Add(blockBounds);

                                float absDepthX = Math.Abs(depth.X);
                                float absDepthY = Math.Abs(depth.Y);
                                float absDepthZ = Math.Abs(depth.Z);

                                float smallestDepth = (absDepthX < absDepthY) ? absDepthX : absDepthY;
                                smallestDepth = (absDepthZ < smallestDepth) ? absDepthZ : smallestDepth;

                                // Resolve the collision along the shallow axis.
                                if (smallestDepth != absDepthY)
                                {
                                    // Resolve the collision along the X axis
                                    if (absDepthX < absDepthZ)
                                        position = new Vector3(position.X - depth.X, position.Y, position.Z);

                                    // Resolve the collision along the Z axis
                                    if (absDepthZ < absDepthX)
                                        position = new Vector3(position.X, position.Y, position.Z - depth.Z);

                                    // Keep calculating collision with new bounds
                                    playerBounds.Min = position + localBounds.Min;
                                    playerBounds.Max = position + localBounds.Max;

                                    bounds = playerBounds;
                                }
                                else
                                {
                                    if (velocity.Y != 0f)
                                    {
                                        // Resolve the collision along the Y axis
                                        position = new Vector3(position.X, position.Y - depth.Y, position.Z);
                                        velocity.Y = 0f;

                                        // Keep calculating collision with new bounds
                                        playerBounds.Min = position + localBounds.Min;
                                        playerBounds.Max = position + localBounds.Max;
                                        bounds = playerBounds;
                                    }
                                }
                            }
                        }
                        // Finish checking this block
                    }
                }
            }
            playerData.position = position;

            // Finish checking collisions
            return collidedBlocks;
        }
    }
}
