using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    /// <summary>
    /// Creates a skydome with vertex color only
    /// </summary>
    class Skydome
    {
        /// Graphics resources
        private GraphicsDevice graphicsDevice;
        private MeshData mesh;
        public MeshData Mesh { get { return mesh; } }
        private Texture2D skymap;
        public Texture2D Skymap { get { return skymap; } }

        /// Skydome settings
        private readonly int DomeN = 32;
        private readonly int vertexCount;
        private readonly int indexCount;

        /// Time settings
        /// # of real seconds that equals one game day
        private readonly float dayLengthTime = 1440f;
        private float timeOfDay;
        public float TimeOfDay { get { return timeOfDay; } }
        public float RelativeTimeOfDay { 
            get { return timeOfDay / dayLengthTime; } }

        /// <summary>
        /// Skydome constructor
        /// </summary>
        public Skydome(GraphicsDevice device, ContentManager content)
        {
            graphicsDevice = device;
            mesh = new MeshData();
            skymap = content.Load<Texture2D>("Textures/skymap1");

            int latitude = DomeN / 2;
            int longitude = DomeN;

            // Start default time of day
            timeOfDay = 0.5f * dayLengthTime;

            // Set buffer sizes
            vertexCount = longitude * latitude;
            indexCount = (longitude - 1) * (latitude - 1) * 2;
            vertexCount *= 2;
            indexCount *= 2;
            
            GenerateDome(latitude, longitude);
        }

        /// <summary>
        /// Generate the skydome, filling in the vertex and index buffers
        /// </summary>
        private void GenerateDome(int latitude, int longitude)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[vertexCount];

            // Fill Vertex Buffer
            int v = 0;
            for (int i = 0; i < longitude; i++)
            {
                double MoveXZ = 100.0f * (i / ((float)longitude - 1.0f)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < latitude; j++)
                {
                    double MoveY = MathHelper.Pi * j / (latitude - 1);

                    vertices[v] = new VertexPositionColor();
                    vertices[v].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    vertices[v].Position.Y = (float)Math.Cos(MoveXZ);
                    vertices[v].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    vertices[v].Color = Color.DeepSkyBlue;

                    v++;
                }
            }

            for (int i = 0; i < longitude; i++)
            {
                double MoveXZ = 100.0 * (i / (float)(longitude - 1)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < latitude; j++)
                {
                    double MoveY = (MathHelper.Pi * 2.0) - (MathHelper.Pi * j / (latitude - 1));

                    vertices[v] = new VertexPositionColor();
                    vertices[v].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    vertices[v].Position.Y = (float)Math.Cos(MoveXZ);
                    vertices[v].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    vertices[v].Color = Color.DeepSkyBlue;

                    v++;
                }
            }

            // Fill index buffer
            short[] indices = new short[indexCount * 3];
            v = 0;

            for (short i = 0; i < longitude - 1; i++)
            {
                for (short j = 0; j < latitude - 1; j++)
                {
                    indices[v++] = (short)(i * latitude + j);
                    indices[v++] = (short)((i + 1) * latitude + j);
                    indices[v++] = (short)((i + 1) * latitude + j + 1);

                    indices[v++] = (short)((i + 1) * latitude + j + 1);
                    indices[v++] = (short)(i * latitude + j + 1);
                    indices[v++] = (short)(i * latitude + j);
                }
            }

            short Offset = (short)(latitude * longitude);
            for (short i = 0; i < longitude - 1; i++)
            {
                for (short j = 0; j < latitude - 1; j++)
                {
                    indices[v++] = (short)(Offset + i * latitude + j);
                    indices[v++] = (short)(Offset + (i + 1) * latitude + j + 1);
                    indices[v++] = (short)(Offset + (i + 1) * latitude + j);

                    indices[v++] = (short)(Offset + i * latitude + j + 1);
                    indices[v++] = (short)(Offset + (i + 1) * latitude + j + 1);
                    indices[v++] = (short)(Offset + i * latitude + j);
                }
            }

            // Create vertex and index buffers
            mesh.vb = new VertexBuffer(
                graphicsDevice,
                VertexPositionColor.VertexDeclaration,
                vertices.Length,
                BufferUsage.None
            );

            mesh.ib = new IndexBuffer(
                graphicsDevice, IndexElementSize.SixteenBits,
                (short)indices.Length,
                BufferUsage.None
            );

            // Set up vertex and index buffer
            mesh.vb.SetData(vertices, 0, vertices.Length);
            mesh.ib.SetData(indices, 0, indices.Length);

            // Finish generating skydome
        }

        /// <summary>
        /// Set time parameters for skydome to draw the appropriate colors
        /// in the shader
        /// </summary>
        public void UpdateTimeOfDay(double timeInSeconds, Effect effect)
        {
            timeOfDay += (float)timeInSeconds;

            if (timeOfDay >= dayLengthTime)
                timeOfDay = 0;

            // Set shader parameters for skydome
            effect.Parameters["timeOfDay"].SetValue(timeOfDay);
            effect.Parameters["dayLengthTime"].SetValue(dayLengthTime);
            effect.Parameters["skyTexture"].SetValue(skymap);
        }
    }
}
