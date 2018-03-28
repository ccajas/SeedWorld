using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    public class BoundingBoxRenderer
    {
        /// Containers for temp data, to avoid calling the GC
        Vector3[] boxCorners;

        /// Effect for drawing shapes
        private BasicEffect basicEffect;

        /// Vertex structure for the colored bounding boxes
        public VertexPositionColor[] bBoxVertices;

        /// Initialize an array of bounding box indices
        public static readonly short[] bBoxIndices = {
			0, 1, 1, 2, 2, 3, 3, 0,
			4, 5, 5, 6, 6, 7, 7, 4,
			0, 4, 1, 5, 2, 6, 3, 7
		};

        /// <summary>
        /// Initialize variables in constructor
        /// </summary>
        public BoundingBoxRenderer(GraphicsDevice graphicsDevice)
        {
            boxCorners = new Vector3[8];
            basicEffect = new BasicEffect(graphicsDevice);

            // Bounding box data
            bBoxVertices = new VertexPositionColor[BoundingBox.CornerCount];

            // Effects for debug shape drawing
            basicEffect.LightingEnabled = false;
            basicEffect.TextureEnabled = false;
            basicEffect.VertexColorEnabled = true;
        }

        /// <summary>
        /// Draw a debug bounding box
        /// </summary>
        [Conditional("DEBUG")]
        public void Draw(GraphicsDevice graphicsDevice, Camera camera, Matrix matrix, BoundingBox box, Color color)
        {
            basicEffect.World = matrix;
            basicEffect.View = camera.view;
            basicEffect.Projection = camera.projection;

            // Assign the box corners
            boxCorners[0] = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
            boxCorners[1] = new Vector3(box.Max.X, box.Max.Y, box.Max.Z); // maximum
            boxCorners[2] = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
            boxCorners[3] = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
            boxCorners[4] = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
            boxCorners[5] = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
            boxCorners[6] = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
            boxCorners[7] = new Vector3(box.Min.X, box.Min.Y, box.Min.Z); // minimum

            for (int i = 0; i < boxCorners.Length; i++)
            {
                bBoxVertices[i].Position = Vector3.Transform(boxCorners[i], Matrix.Identity);
                bBoxVertices[i].Color = color;
            }

            for (int i = 0; i < basicEffect.CurrentTechnique.Passes.Count; i++)
            {
                basicEffect.CurrentTechnique.Passes[i].Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList, bBoxVertices, 0, 8,
                    bBoxIndices, 0, 12);
            } 
        }

        /// <summary>
        /// Draw bounding box overload with default color
        /// </summary>
        [Conditional("DEBUG")]
        public void Draw(GraphicsDevice graphicsDevice, Camera camera, Matrix matrix, BoundingBox box)
        {
            Draw(graphicsDevice, camera, matrix, box, Color.Red);
        }
    }
}
