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
    /// Vertex structure for cubes
    /// </summary>
    public struct VertexPositionColorNormal : IVertexType
    {
        public uint Position;
        public uint ColorNormal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.Position, 0),
            new VertexElement(4, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>
    /// Generate and store vertex data for a chunk cube/voxel
    /// </summary>

    public class Cube
    {
        /// Buffer data
        private VertexPositionColorNormal[] vertices;
        private ushort[] indices;

        /// Coloring
        private float[] ao;
        private float[] shades;
        private uint color;
        private readonly int neighbors;

        // Constants
        private readonly int left;
        private readonly int right;
        private readonly int back;
        private readonly int front;
        private readonly int top;
        private readonly int bottom;

        public VertexPositionColorNormal[] Vertices
        {
            get { return vertices; }
        }

        public ushort[] Indices
        {
            get { return indices; }
        }

        public Cube(Vector3 offset, uint color, float[] shades, int neighbors, int scale = 1)
        {
            int totalVertices = 0;
            int totalIndices = 0;
            int compare = 1;

            this.color = color;
            this.neighbors = neighbors;
            this.shades = shades;

            // Set constants
            left = 0;
            right = 1;
            front = 2;
            back = 3;
            top = 4;
            bottom = 5;

            // Find out how many faces to render for this cube
            // Each face will have six vertices

            for (int i = NeighborBlock.Middle_W; i <= NeighborBlock.Bottom_Center; i++)
            {
                if ((neighbors & compare) != compare)
                {
                    totalVertices += 4;
                    totalIndices += 6;
                }
                compare <<= 1;
            }

            vertices = new VertexPositionColorNormal[totalVertices];
            indices = new ushort[totalIndices];
            ao = new float[totalVertices];

            SetUpVertices(offset, scale);
        }

        /// <summary>
        /// Lookup table for neighbor voxels 
        /// IDs are used for properly shading the vertices on each side.
        /// 
        /// Neighbors are as follows:
        /// 
        /// Top          Bottom       Middle
        ///  6 12  7     14 20 15     22 -- 23
        /// 10    11     18    19     --    --
        ///  8 13  9     16 21 17     24 -- 25
        ///    v            v
        ///    
        /// </summary>

        /// <summary>
        /// Shade the vertex color with an AO term according to the neighbor info
        /// </summary>
        private float GetAmbientOcclusion(int corner, int side1, int side2)
        {
            side1  = 1 << side1;
            side2  = 1 << side2;
            corner = 1 << corner;

            float ao = 3;
            ao -= ((side1 & neighbors) == side1 && (side2 & neighbors) == side2) ?
                3 : (Convert.ToInt32((side1 & neighbors) == side1) +
                     Convert.ToInt32((side2 & neighbors) == side2) +
                     Convert.ToInt32((corner & neighbors) == corner));

            return ao;
        }

        /// <summary>
        /// Add the AO term data to the vertex color
        /// </summary>
        private uint ApplyAmbientOcclusion(uint colorData, int index)
        {
            return colorData + (uint)((int)ao[index] << 3);
        }

        /// <summary>
        /// Flip the quad drawing where needed for better AO shading
        /// </summary>
        private bool FixAnisotropy(int index, uint normal)
        {
            int idx0 = index - 4;
            int idx1 = index - 3;
            int idx2 = index - 2;
            int idx3 = index - 1;

            bool flipped = false;

            VertexPositionColorNormal[] tempVerts = 
            { 
                vertices[idx0], 
                vertices[idx1], 
                vertices[idx2], 
                vertices[idx3] 
            };

            float[] tempShade = 
            { 
                ao[idx0], 
                ao[idx1], 
                ao[idx2], 
                ao[idx3] 
            };

            if (ao[idx0] + ao[idx3] > ao[idx1] + ao[idx2])
            {
                // generate flipped quad
                vertices[idx0] = tempVerts[1];
                vertices[idx1] = tempVerts[0];
                vertices[idx2] = tempVerts[3];
                vertices[idx3] = tempVerts[2];

                ao[idx0] = tempShade[1];
                ao[idx1] = tempShade[0];
                ao[idx2] = tempShade[3];
                ao[idx3] = tempShade[2];

                flipped = true;
            }

            // Apply color and normal information
            vertices[idx0].ColorNormal = ApplyAmbientOcclusion(color, idx0) + normal;
            vertices[idx1].ColorNormal = ApplyAmbientOcclusion(color, idx1) + normal;
            vertices[idx2].ColorNormal = ApplyAmbientOcclusion(color, idx2) + normal;
            vertices[idx3].ColorNormal = ApplyAmbientOcclusion(color, idx3) + normal;

            return flipped;
        }

        /// <summary>
        /// Create the left face
        /// </summary>
        private bool CreateFaceLeft(uint[] corners, ref int v)
        {
            // Left
            ao[v] = GetAmbientOcclusion(NeighborBlock.Top_NW, NeighborBlock.Top_W, NeighborBlock.Middle_NW); //  + 2
            vertices[v].Position = corners[0] + ((uint)(shades[left] * 255) << 24);
            ao[v + 1] = GetAmbientOcclusion(NeighborBlock.Top_SW, NeighborBlock.Top_W, NeighborBlock.Middle_SW);
            vertices[v + 1].Position = corners[4] + ((uint)(shades[left] * 255) << 24);
            ao[v + 2] = GetAmbientOcclusion(NeighborBlock.Bottom_NW, NeighborBlock.Bottom_W, NeighborBlock.Middle_NW);
            vertices[v + 2].Position = corners[2] + ((uint)(shades[left] * 255) << 24);
            ao[v + 3] = GetAmbientOcclusion(NeighborBlock.Bottom_SW, NeighborBlock.Bottom_W, NeighborBlock.Middle_SW);
            vertices[v + 3].Position = corners[6] + ((uint)(shades[left] * 255) << 24);

            v = v + 4;

            return FixAnisotropy(v, 2);
        }

        /// <summary>
        /// Create the right face
        /// </summary>
        private bool CreateFaceRight(uint[] corners, ref int v)
        {
            // Right
            ao[v] = GetAmbientOcclusion(NeighborBlock.Top_SE, NeighborBlock.Top_E, NeighborBlock.Middle_SE); // + 3
            vertices[v].Position = corners[5] + ((uint)(shades[right] * 255) << 24);
            ao[v + 1] = GetAmbientOcclusion(NeighborBlock.Top_NE, NeighborBlock.Top_E, NeighborBlock.Middle_NE);
            vertices[v + 1].Position = corners[1] + ((uint)(shades[right] * 255) << 24);
            ao[v + 2] = GetAmbientOcclusion(NeighborBlock.Bottom_SE, NeighborBlock.Bottom_E, NeighborBlock.Middle_SE);
            vertices[v + 2].Position = corners[7] + ((uint)(shades[right] * 255) << 24);
            ao[v + 3] = GetAmbientOcclusion(NeighborBlock.Bottom_NE, NeighborBlock.Bottom_E, NeighborBlock.Middle_NE);
            vertices[v + 3].Position = corners[3] + ((uint)(shades[right] * 255) << 24);

            v = v + 4;

            return FixAnisotropy(v, 3);
        }

        /// <summary>
        /// Create the top face
        /// </summary>
        private bool CreateFaceTop(uint[] corners, ref int v)
        {
            // Top
            ao[v] = GetAmbientOcclusion(NeighborBlock.Top_NW, NeighborBlock.Top_W, NeighborBlock.Top_N);
            vertices[v].Position = corners[0] + ((uint)(shades[top] * 255) << 24);
            ao[v + 1] = GetAmbientOcclusion(NeighborBlock.Top_NE, NeighborBlock.Top_E, NeighborBlock.Top_N);
            vertices[v + 1].Position = corners[1] + ((uint)(shades[top] * 255) << 24);
            ao[v + 2] = GetAmbientOcclusion(NeighborBlock.Top_SW, NeighborBlock.Top_W, NeighborBlock.Top_S);
            vertices[v + 2].Position = corners[4] + ((uint)(shades[top] * 255) << 24);
            ao[v + 3] = GetAmbientOcclusion(NeighborBlock.Top_SE, NeighborBlock.Top_E, NeighborBlock.Top_S);
            vertices[v + 3].Position = corners[5] + ((uint)(shades[top] * 255) << 24);

            v = v + 4;

            return FixAnisotropy(v, 0);
        }

        /// <summary>
        /// Create the bottom face
        /// </summary>
        private bool CreateFaceBottom(uint[] corners, ref int v)
        {
            ao[v] = GetAmbientOcclusion(NeighborBlock.Bottom_SW, NeighborBlock.Bottom_W, NeighborBlock.Bottom_S); // + 1
            vertices[v].Position = corners[6] + ((uint)(shades[bottom] * 255) << 24);
            ao[v + 1] = GetAmbientOcclusion(NeighborBlock.Bottom_SE, NeighborBlock.Bottom_E, NeighborBlock.Bottom_S);
            vertices[v + 1].Position = corners[7] + ((uint)(shades[bottom] * 255) << 24);
            ao[v + 2] = GetAmbientOcclusion(NeighborBlock.Bottom_NW, NeighborBlock.Bottom_W, NeighborBlock.Bottom_N);
            vertices[v + 2].Position = corners[2] + ((uint)(shades[bottom] * 255) << 24);
            ao[v + 3] = GetAmbientOcclusion(NeighborBlock.Bottom_NE, NeighborBlock.Bottom_E, NeighborBlock.Bottom_N);
            vertices[v + 3].Position = corners[3] + ((uint)(shades[bottom] * 255) << 24);

            v = v + 4;

            return FixAnisotropy(v, 1);
        }

        /// <summary>
        /// Create the front face
        /// </summary>
        private bool CreateFaceFront(uint[] corners, ref int v)
        {
            ao[v] = GetAmbientOcclusion(NeighborBlock.Top_SW, NeighborBlock.Top_S, NeighborBlock.Middle_SW);// + 4
            vertices[v].Position = corners[4] + ((uint)(shades[front] * 255) << 24);
            ao[v + 1] = GetAmbientOcclusion(NeighborBlock.Top_SE, NeighborBlock.Top_S, NeighborBlock.Middle_SE);
            vertices[v + 1].Position = corners[5] + ((uint)(shades[front] * 255) << 24);
            ao[v + 2] = GetAmbientOcclusion(NeighborBlock.Bottom_SW, NeighborBlock.Bottom_S, NeighborBlock.Middle_SW);
            vertices[v + 2].Position = corners[6] + ((uint)(shades[front] * 255) << 24);
            ao[v + 3] = GetAmbientOcclusion(NeighborBlock.Bottom_SE, NeighborBlock.Bottom_S, NeighborBlock.Middle_SE);
            vertices[v + 3].Position = corners[7] + ((uint)(shades[front] * 255) << 24);

            v = v + 4;

            return FixAnisotropy(v, 4);
        }

        /// <summary>
        /// Create the back face
        /// </summary>
        private bool CreateFaceBack(uint[] corners, ref int v)
        {
            ao[v] = GetAmbientOcclusion(NeighborBlock.Top_NE, NeighborBlock.Top_N, NeighborBlock.Middle_NE);//+ 5
            vertices[v].Position = corners[1] + ((uint)(shades[back] * 255) << 24);
            ao[v + 1] = GetAmbientOcclusion(NeighborBlock.Top_NW, NeighborBlock.Top_N, NeighborBlock.Middle_NW);
            vertices[v + 1].Position = corners[0] + ((uint)(shades[back] * 255) << 24);
            ao[v + 2] = GetAmbientOcclusion(NeighborBlock.Bottom_NE, NeighborBlock.Bottom_N, NeighborBlock.Middle_NE);
            vertices[v + 2].Position = corners[3] + ((uint)(shades[back] * 255) << 24);
            ao[v + 3] = GetAmbientOcclusion(NeighborBlock.Bottom_NW, NeighborBlock.Bottom_N, NeighborBlock.Middle_NW);
            vertices[v + 3].Position = corners[2] + ((uint)(shades[back] * 255) << 24);

            v = v + 4;

            return FixAnisotropy(v, 5);
        }

        /// <summary>
        /// Add the vertex data for this cube
        /// </summary>
        private void SetUpVertices(Vector3 offset, int scale = 1)
        {
            uint s = (uint)scale;
            uint[] corners = 
            {
                ((uint)offset.X) + ((uint)offset.Y + s << 8) + ((uint)offset.Z << 16),         // 0 -- 1
                ((uint)offset.X + s) + ((uint)offset.Y + s << 8) + ((uint)offset.Z << 16),     // | B  |
                ((uint)offset.X) + ((uint)offset.Y << 8) + ((uint)offset.Z << 16),             // 2 -- 3
                ((uint)offset.X + s) + ((uint)offset.Y << 8) + ((uint)offset.Z << 16),

                ((uint)offset.X) + ((uint)offset.Y + s << 8) + ((uint)offset.Z + s << 16),     // 4 -- 5
                ((uint)offset.X + s) + ((uint)offset.Y + s << 8) + ((uint)offset.Z + s << 16), // | F  |
                ((uint)offset.X) + ((uint)offset.Y << 8) + ((uint)offset.Z + s << 16),         // 6 -- 7
                ((uint)offset.X + s) + ((uint)offset.Y << 8) + ((uint)offset.Z + s << 16),
            };

            // Set vertices according to the filter
            int v = 0;
            int b = 0;
            bool[] flipped = new bool[6];

            // Create the cube faces
            if ((neighbors & 1 << NeighborBlock.Middle_W) != 1 << NeighborBlock.Middle_W)
                flipped[b++] = CreateFaceLeft(corners, ref v);

            if ((neighbors & 1 << NeighborBlock.Middle_E) != 1 << NeighborBlock.Middle_E)
                flipped[b++] = CreateFaceRight(corners, ref v);

            if ((neighbors & 1 << NeighborBlock.Middle_N) != 1 << NeighborBlock.Middle_N)
                flipped[b++] = CreateFaceBack(corners, ref v);

            if ((neighbors & 1 << NeighborBlock.Middle_S) != 1 << NeighborBlock.Middle_S)
                flipped[b++] = CreateFaceFront(corners, ref v);

            if ((neighbors & 1 << NeighborBlock.Top_Center) != 1 << NeighborBlock.Top_Center)
                flipped[b++] = CreateFaceTop(corners, ref v);

            if ((neighbors & 1 << NeighborBlock.Bottom_Center) != 1 << NeighborBlock.Bottom_Center)
                flipped[b++] = CreateFaceBottom(corners, ref v);

            // Add the indices for the cube
            int[] faceVertices = new int[6] { 0, 1, 2, 1, 3, 2 };
            int[] faceVertices2 = new int[6] { 1, 0, 2, 1, 2, 3 };

            // Add vertex shading info (unrelated to AO)
            //for (int i = 0; i < vertices.Length; i++)
            //    vertices[i].Position += (uint)(this.shades * 255) << 24;

            for (int i = 0; i < indices.Length; i++)
            {
                int vOffset = (i / 6) * 4;
                if (flipped[i / 6])
                    indices[i] = (ushort)(vOffset + faceVertices2[i % 6]);
                else
                    indices[i] = (ushort)(vOffset + faceVertices[i % 6]);
            }
            // End updating vertex data
        }
    }
}
