using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    public class MeshData
    {
        public IndexBuffer ib;
        public VertexBuffer vb;
        public Texture3D aoTexture;
        public BoundingBox bBox;
        public Color bBoxColor;
    }

    /// <summary>
    /// Batches a collection of model meshes to draw in a specific order
    /// Each mesh can be drawn once for every transform matrix, and is cached
    /// to increase performance.
    /// </summary>
    public class ModelBatch
    {
        /// <summary>
        /// Storage for matrices ordered by individual matrix transformations,
        /// to handle drawing the same mesh more than once
        /// </summary>
        private Dictionary<MeshData, List<Matrix>> meshInstances;

        /// <summary>
        /// Cached mesh instances can reuse existing meshes if they existed previously.
        /// This reduces memory allocation for the GC to collect.
        /// </summary>
        private Dictionary<MeshData, List<Matrix>> cachedMeshInstances;
        private Queue<MeshData> meshQueue;

        /// Number of vertices rendered in a frame
        private int totalVertexCount;

        /// Initial vertex model batch size and instance count for each mesh
        private int initialBatchSize = 12000;
        //private int initialInstanceCount = 100;

        /// Resources used for rendering a batch
        private GraphicsDevice graphicsDevice;
        private Camera camera;
        private Effect effect;

        /// Render states for batch
        private RasterizerState rasterizerState;
        private BlendState blendState;
        private DepthStencilState depthStencilState;

        /// Render target for on-screen texture drawing
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Initialize the vertex and index lists
        /// </summary>
        public ModelBatch(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            meshInstances = new Dictionary<MeshData, List<Matrix>>(initialBatchSize + 1);
            cachedMeshInstances = new Dictionary<MeshData, List<Matrix>>(initialBatchSize + 1);
            meshQueue = new Queue<MeshData>(initialBatchSize);

            // Set render target for later use
            renderTarget = new RenderTarget2D(
                graphicsDevice,
                graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight);
        }

        /// <summary>
        /// Begin a new vertex batch with the selected camera and effect
        /// </summary>
        public void Begin(
            Camera camera, Effect effect, 
            RasterizerState rasterizerState = null,
            BlendState blendState = null, 
            DepthStencilState depthStencilState = null
            )
        {
            // Set devault render state values
            this.rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            this.blendState = blendState ?? BlendState.NonPremultiplied;
            this.depthStencilState = depthStencilState ?? DepthStencilState.None;

            // Set other parameters
            this.camera = camera;
            this.effect = effect;
        }

        /// <summary>
        /// Begin() overload to draw scene to the render target
        /// </summary>
        public void BeginRenderTarget(
            Camera camera, Effect effect,
            RasterizerState rasterizerState = null,
            BlendState blendState = null,
            DepthStencilState depthStencilState = null
            )
        {
            // Reset render target if needed
            if (renderTarget.Height != graphicsDevice.PresentationParameters.BackBufferHeight ||
                renderTarget.Width != graphicsDevice.PresentationParameters.BackBufferWidth)
            {
                renderTarget = new RenderTarget2D(
                    graphicsDevice,
                    graphicsDevice.PresentationParameters.BackBufferWidth,
                    graphicsDevice.PresentationParameters.BackBufferHeight);
            }

            // Set the device to the render target
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.DeepSkyBlue);

            Begin(camera, effect, rasterizerState, blendState, depthStencilState);
        }

        /// <summary>
        /// Add a mesh instance to be drawn
        /// </summary>
        public void Draw(MeshData mesh, Matrix worldTransform)
        {
            // Ignore if no indices are set
            if (mesh.ib == null)
                return;

            List<Matrix> value = null;

            if (meshInstances.TryGetValue(mesh, out value))
            {
                // Add a transform matrix to the existing list
                meshInstances[mesh].Add(worldTransform);
            }
            else
            {
                // Search if mesh already exists in the cache
                if (cachedMeshInstances.TryGetValue(mesh, out value))
                {
                    // Clear the matrix list first
                    value.Clear();
                    meshInstances.Add(mesh, value);
                }
                else
                {
                    meshInstances.Add(mesh, new List<Matrix>());
                }
                meshInstances[mesh].Add(worldTransform);
            }
        }

        private BlendState alphaBlend = new BlendState()
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            AlphaSourceBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            ColorSourceBlend = Blend.One,
            BlendFactor = Color.White,
            ColorWriteChannels = ColorWriteChannels.All,
            MultiSampleMask = -1
        };

        /// <summary>
        /// Cull and draw visible meshes
        /// </summary>
        public void End(ref int vertexCount, BoundingBoxRenderer boxRenderer = null)
        {
            totalVertexCount = 0;
            graphicsDevice.BlendState = blendState;
            graphicsDevice.RasterizerState = rasterizerState;

            // Set universal values for effects used by meshes
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["camPos"].SetValue(camera.position);

            effect.CurrentTechnique = effect.Techniques["Default"];

            // Loop through each mesh and render it once for each transform
            foreach (KeyValuePair<MeshData, List<Matrix>> meshInstance in meshInstances)
            {
                MeshData mesh = meshInstance.Key;
                List<Matrix> matrices = meshInstance.Value;

                // Set the mesh once before setting the transforms
                graphicsDevice.SetVertexBuffer(mesh.vb);
                graphicsDevice.Indices = mesh.ib;

                // Render each instance of this mesh
                foreach (Matrix worldTransform in matrices)
                {
                    if (mesh.ib == null)
                        continue;

                    effect.Parameters["World"].SetValue(worldTransform);

                    // Set additional parameters
                    if (mesh.aoTexture != null)
                        effect.Parameters["aoTexture"].SetValue(mesh.aoTexture);

                    if (camera.frustum.Contains(mesh.bBox) != ContainmentType.Disjoint ||
                        (mesh.bBox.Max == Vector3.Zero && mesh.bBox.Min == Vector3.Zero))
                    {
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList, 0, 0,
                                mesh.ib.IndexCount, 0, mesh.ib.IndexCount / 3);
                        }

                        // Add total vertices rendered
                        totalVertexCount += mesh.vb.VertexCount;

                        // Draw the bounding box
                        if (boxRenderer != null)
                            boxRenderer.Draw(graphicsDevice, camera, Matrix.Identity, mesh.bBox, mesh.bBoxColor);
                    }
                }

                // Done rendering mesh, release item to the cached list
                if (!cachedMeshInstances.ContainsKey(mesh))
                {
                    // Add to cache if not found already
                    cachedMeshInstances.Add(mesh, new List<Matrix>());

                    // Limit cache to batch size if too large, by removing the oldest mesh
                    if (meshQueue.Count == initialBatchSize)
                        cachedMeshInstances.Remove(meshQueue.Dequeue());

                    meshQueue.Enqueue(mesh);
                }
            }

            // Remove all current instances
            meshInstances.Clear();
            graphicsDevice.SetRenderTarget(null);

            vertexCount = totalVertexCount;
        }

        /// <summary>
        /// End() overload without a ref parameter
        /// </summary>
        public void End(BoundingBoxRenderer boxRenderer = null)
        {
            int vertexCount = 0;

            End(ref vertexCount, boxRenderer);
        }

        /// <summary>
        /// Draw the render target output
        /// </summary>
        public void DrawRenderTarget(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}