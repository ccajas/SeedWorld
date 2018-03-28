using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    class TextItem
    {
        /// Starting location for text
        private Vector2 basePoint;

        /// SpriteBatch for drawing text
        private SpriteBatch spriteBatch;

        /// Font used for text
        private SpriteFont font;

        /// Text storage
        private StringBuilder text;

        /// Current position and color
        private Vector2 currentPos;
        private int increment;
        private Color currentColor;

        /// Decimal places for numbers
        private uint places;

        /// <summary>
        /// Set location and font for text
        /// </summary>
        public TextItem(Vector2 point, SpriteFont font, SpriteBatch spriteBatch)
        {
            text = new StringBuilder(64, 64);
            basePoint = point;

            this.font = font;
            this.spriteBatch = spriteBatch;
            this.places = 0;
        
            // Set font defaults
            currentPos = basePoint;
            increment = font.LineSpacing;
            currentColor = Color.White;
        }

        /// <summary>
        /// Start drawing the text at the base position
        /// </summary>
        public void Begin()
        {
            currentPos = basePoint;
        }

        /// <summary>
        /// Set new current color
        /// </summary>
        public TextItem SetColor(Color color)
        {
            currentColor = color;
            return this;
        }

        /// <summary>
        /// Set no. of decimal places
        /// </summary>
        public TextItem SetDecimal(uint places)
        {
            this.places = places;
            return this;
        }

        /// <summary>
        /// Align text to bottom, going up per line
        /// </summary>
        public void AlignBottom(Vector2 position)
        {
            basePoint = position;
            currentPos = position;
            currentPos.Y -= font.LineSpacing;
            increment = -font.LineSpacing;
        }

        /// <summary>
        /// Align text to top, going down per line
        /// </summary>
        public void AlignTop(Vector2 position)
        {
            basePoint = position;
            currentPos = position;
            increment = font.LineSpacing;
        }

        /// <summary>
        /// Text with no variables
        /// </summary>
        public void DrawText(String textString)
        {
            text.Append(textString);
            DrawTextFull(null);
        }

        /// <summary>
        /// Text with int variable(s)
        /// </summary>
        public void DrawText(String prefix = null, String suffix = null, params int[] intVals)
        {
            text.Append(prefix);
            foreach (float intVal in intVals)
                text.Concat(intVal, places).Append(" ");

            DrawTextFull(suffix);
        }

        /// <summary>
        /// Text with float variable(s)
        /// </summary>
        public void DrawText(String prefix = null, String suffix = null, params float[] floatVals)
        {
            text.Append(prefix);
            foreach (float floatVal in floatVals)
                text.Concat(floatVal, places).Append(" ");

            DrawTextFull(suffix);
        }

        /// <summary>
        /// Draw a string with the StringBuilder object
        /// </summary>
        private void DrawTextFull(String str)
        {
            text.Append(str);
            spriteBatch.DrawString(font, text, currentPos, currentColor);
            text.Clear();

            currentPos.Y += increment;
        }
    }
}
