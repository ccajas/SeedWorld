using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld.ScreenElements
{
    /// <summary>
    /// Screen that updates and renders the procedural world
    /// </summary>
    class WorldViewScreen : InteractiveScreenElement
    {
        /// ContentManager reference
        private ContentManager content;

        /// Rendering resources
        private Effect voxelEffect, skydomeEffect;
        private BoundingBoxRenderer boxRenderer;
        private PlayerCamera playerCamera, skydomeCamera;

        private int vertexCount;

        /// Debug stat usage
        public class DebugStats
        {
            public int vertexCount;
            public int chunksAdded, chunksLoaded;
            public Vector3 cameraPos;
            public Vector3 playerPos;
            public float timeOfDay;
        };

        DebugStats debugStats;
        private bool toggleColors = true;
        private bool consoleEnabled = false;

        /// Game data manager
        private GameData gameData;

        /// Voxel world data       
        private ChunkManager chunkManager;
        private Skydome skydome;

        /// Player data
        private Player player;
        private VoxelSprite playerSprite;

        /// <summary>
        /// Constructor for the WorldViewScreen
        /// </summary>
        public WorldViewScreen(Game game, ScreenElement previousScreenElement) :
            base(previousScreenElement, game.GraphicsDevice)
        {
            this.content = game.Content;
            this.game = game;

            // Set seed to build world with
            int mapSeed = 123;
            Noise.Simplex.Seed(mapSeed);

            // Load graphics resources/assets
            boxRenderer = new BoundingBoxRenderer(graphicsDevice);

            voxelEffect = content.Load<Effect>("Effects/voxel");
            skydomeEffect = content.Load<Effect>("Effects/skydome");

            // Load initial data for the voxel sprite
            VoxelCache voxelCache = new VoxelCache(mapSeed, 32, 128);

            playerSprite = new VoxelSprite(graphicsDevice);
            playerSprite.Load(voxelCache, "chr_knight");

            // Attempt to load player save data
            gameData = new GameData();

            if (gameData.LoadGame(mapSeed))
            {
                player = new Player(playerSprite.Mesh, gameData.Player);
            }
            else
            {
                Player.PlayerData data = new Player.PlayerData()
                {
                    position = new Vector3(200, 50, -20),
                    orientation = 0
                };
                player = new Player(playerSprite.Mesh, data);
            }

            // Set up voxel chunks and skydome
            chunkManager = new ChunkManager(graphicsDevice, 123);
            chunkManager.Initialize(graphicsDevice, player.playerData.position);
            skydome = new Skydome(graphicsDevice, content);

            // Set up cameras
            playerCamera = new PlayerCamera(Vector3.Zero, Vector2.Zero);
            skydomeCamera = new PlayerCamera(Vector3.Zero, Vector2.Zero);

            InitializeGraphicsResources();

            // Set debug info
            debugStats = new DebugStats
            {
                vertexCount = this.vertexCount,
                cameraPos = this.playerCamera.position,
                chunksAdded = this.chunkManager.ChunksAdded
            };

            // Testing console commands
            GUI.EntitySpawner spawner = new GUI.EntitySpawner();
            console.Command("test");

            // Set some non-standard effect parameters here
            Color skyColor = new Color(0.091f, 0.622f, 0.976f);
            voxelEffect.Parameters["skyColor"].SetValue(skyColor.ToVector3());
            voxelEffect.Parameters["skyTexture"].SetValue(skydome.Skymap);
            voxelEffect.Parameters["toggleColors"].SetValue(toggleColors);

            // Manage resources that need to be reset when graphics device is lost
            game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(OnDeviceReset);

            // Add children screen with these stats
            children.Add(new ScreenElements.DebugViewScreen(this, content, graphicsDevice, debugStats, game));
        }

        /// <summary>
        /// Device reset function
        /// </summary>
        protected override void OnDeviceReset(Object sender, EventArgs args)
        {
            InitializeGraphicsResources();
        }

        /// <summary>
        /// Set up graphics related resources
        /// </summary>
        private void InitializeGraphicsResources()
        {
            // Set up cameras
            playerCamera.Initialize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            skydomeCamera.Initialize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
        }

        /// <summary>
        /// Unload World and Player content
        /// </summary>
        public override void UnloadContent()
        {
            // Save player content with map seed as ID
            gameData.SaveGame(chunkManager.mapSeed);

            base.UnloadContent();
        }

        /// <summary>
        /// Update camera input and other world dependent inputs
        /// </summary>
        public override void HandleInput(TimeSpan frameStepTime)
        {
            if (inputState.gamePadState.Buttons.Start == ButtonState.Pressed || NewKeyPressed(Keys.Enter))
            {
                playerCamera.mouseDisabled = !playerCamera.mouseDisabled;
                skydomeCamera.mouseDisabled = !skydomeCamera.mouseDisabled;
                children.Add(new ScreenElements.SettingsScreen(game, this));
            }

            if (inputState.gamePadState.Buttons.Back == ButtonState.Pressed ||
                NewKeyPressed(Keys.Escape))
            {
                if (!consoleEnabled)
                    this.Exit();
                else
                    consoleEnabled = false;
            }

            // First take input from the camera, 
            // then the player to move based on camera orientation
            playerCamera.GetInput(inputState, false);
            skydomeCamera.GetInput(inputState);

            // Player actions
            if (!consoleEnabled)
            {
                player.GetInput(inputState);

                // Reposition the player
                if (NewKeyPressed(Keys.G))
                    player.Reset(playerCamera.position);

                if (inputState.keyboardState.IsKeyDown(Keys.H))
                    skydome.UpdateTimeOfDay(frameStepTime.TotalSeconds * 200f, skydomeEffect);

                // Toggle camera movement
                if (NewKeyPressed(Keys.X))
                {
                    playerCamera.mouseDisabled = !playerCamera.mouseDisabled;
                    skydomeCamera.mouseDisabled = !skydomeCamera.mouseDisabled;
                    game.IsMouseVisible = playerCamera.mouseDisabled;
                }

                // Color debug
                if (NewKeyPressed(Keys.C))
                {
                    toggleColors = !toggleColors;
                    voxelEffect.Parameters["toggleColors"].SetValue(toggleColors);
                }

                // Toggle fullscreen
                if (NewKeyPressed(Keys.V))
                    game.ToggleFullScreen();
            }
        }

        /// <summary>
        /// Update the world
        /// </summary>
        /// <param name="frameStepTime">Provides a snapshot of timing values.</param>
        public override ScreenElement Update(TimeSpan frameStepTime)
        {
            // All chunks and skydome are updated
            chunkManager.CheckForChunkUpdates(frameStepTime, player.playerData.position);
            skydome.UpdateTimeOfDay(frameStepTime.TotalSeconds, skydomeEffect);

            // Camera collision check
            Vector2 playerMoveVector = new Vector2(
                playerCamera.worldMatrix.Forward.X,
                playerCamera.worldMatrix.Forward.Z
            );

            // First move the player
            player.Update(frameStepTime, playerMoveVector, chunkManager);

            // Keep the sky camera position at the origin
            playerCamera.SetChaseTarget(player.playerData.position + new Vector3(0, 1.5f, 0));
            skydomeCamera.SetChaseTarget(Vector3.Zero);

            // Update camera movement
            playerCamera.Update(frameStepTime);
            skydomeCamera.Update(frameStepTime);

            // Update debug info
            debugStats.vertexCount = vertexCount;
            debugStats.chunksAdded = chunkManager.ChunksAdded;
            debugStats.chunksLoaded = chunkManager.voxelCache.voxels.Count;// ChunksLoaded;
            debugStats.cameraPos = playerCamera.position;
            debugStats.playerPos = player.playerData.position;
            debugStats.timeOfDay = skydome.RelativeTimeOfDay;

            return base.Update(frameStepTime);
        }

        /// <summary>
        /// Draw the world
        /// </summary>
        public override void Draw(TimeSpan frameStepTime)
        {
            Color skyColor = new Color(0.091f, 0.622f, 0.976f);
            game.GraphicsDevice.Clear(skyColor);

            // Draw the skydome (twice for a sphere)
            modelBatch.BeginRenderTarget(skydomeCamera, skydomeEffect);
            modelBatch.Draw(skydome.Mesh, Matrix.CreateScale(900f) *
                Matrix.CreateTranslation(0, -player.playerData.position.Y + 50f, 0));
            modelBatch.End();
            modelBatch.DrawRenderTarget(spriteBatch);

            // Set non-standard effect parameters here
            voxelEffect.Parameters["timeOfDay"].SetValue(skydome.RelativeTimeOfDay);

            // Draw regular voxel objects
            modelBatch.Begin(playerCamera, voxelEffect);
            modelBatch.Draw(player.Mesh, player.worldTransform);

            foreach (Chunk chunk in chunkManager.Chunks)
                modelBatch.Draw(chunk.Mesh, chunk.transformMatrix);

            modelBatch.End(ref vertexCount);
            
            // Draw bounding box for player and collisions            
            foreach (BoundingBox box in player.collidedBlocks)
                boxRenderer.Draw(game.GraphicsDevice, playerCamera, Matrix.Identity, box);

            Matrix rotationMatrix =
                Matrix.CreateRotationY(player.playerData.orientation) *
                Matrix.CreateTranslation(player.playerData.position);
            //boxRenderer.Draw(game.GraphicsDevice, playerCamera, player.LocalBounds, rotationMatrix, Color.Blue);
            
            base.Draw(frameStepTime);
        }
    }
}
