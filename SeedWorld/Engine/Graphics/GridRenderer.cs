using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    class GridRenderer
    {
        /// Vertex structure for the colored grid lines
        public List<VertexPositionColor> gridVertices;
        private int initialVertexCount = 1000;

        /// Effect for drawing shapes
        private BasicEffect basicEffect;

        /// Initialize an array of bounding box indices
        public List<short> gridIndices;

        /// <summary>
        /// Initialize variables in constructor
        /// </summary>
        public GridRenderer(GraphicsDevice graphicsDevice)
        {
            gridVertices = new List<VertexPositionColor>(initialVertexCount);
            gridIndices = new List<short>(initialVertexCount);
            basicEffect = new BasicEffect(graphicsDevice);

            // Default effect render states
            basicEffect.LightingEnabled = false;
            basicEffect.TextureEnabled = false;
            basicEffect.VertexColorEnabled = true;
        }

        /// <summary>
        /// Set up the grid with the given size
        /// </summary>
        public void SetGridSize(int gridSize)
        {
            // Add a 2D grid in the X, Z plane
            int j = 0;

            // Set vertices for lines on X axis, then Z axis
            for (int i = -gridSize; i <= gridSize; i++)
            {
                Color color = (i == 0) ? Color.LightGray : Color.Gray;

                gridVertices.Add(new VertexPositionColor(new Vector3(i, 0, -gridSize), color));
                gridVertices.Add(new VertexPositionColor(new Vector3(i, 0, gridSize), color));
                gridIndices.Add((short)j);
                gridIndices.Add((short)(j + 1));

                j = j + 2;
            }

            for (int i = -gridSize; i <= gridSize; i++)
            {
                Color color = (i == 0) ? Color.LightGray : Color.Gray;

                gridVertices.Add(new VertexPositionColor(new Vector3(-gridSize, 0, i), color));
                gridVertices.Add(new VertexPositionColor(new Vector3(gridSize, 0, i), color));
                gridIndices.Add((short)j);
                gridIndices.Add((short)(j + 1));

                j = j + 2;
            }
        }

        /// <summary>
        /// Draw the grid
        /// </summary>
        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            basicEffect.View = camera.view;
            basicEffect.Projection = camera.projection;

            for (int i = 0; i < basicEffect.CurrentTechnique.Passes.Count; i++)
            {
                basicEffect.CurrentTechnique.Passes[i].Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList, gridVertices.ToArray(), 0, gridVertices.Count,
                    gridIndices.ToArray(), 0, gridIndices.Count / 2);
            }
        }
    }
}
