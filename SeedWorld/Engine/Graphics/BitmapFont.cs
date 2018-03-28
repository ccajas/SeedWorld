using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SeedWorld
{
    class BitmapFont
    {
        // Bit array tiles for font
        private readonly uint[] fontTiles =
        {
            0x00000000, 0x00000000, 0x18181818, 0x00180018, 0x00003636, 0x00000000, 0x367F3636, 0x0036367F, 
            0x3C067C18, 0x00183E60, 0x1B356600, 0x0033566C, 0x6E16361C, 0x00DE733B, 0x000C1818, 0x00000000, 
            0x0C0C1830, 0x0030180C, 0x3030180C, 0x000C1830, 0xFF3C6600, 0x0000663C, 0x7E181800, 0x00001818, 
            0x00000000, 0x0C181800, 0x7E000000, 0x00000000, 0x00000000, 0x00181800, 0x183060C0, 0x0003060C, 
            0x7E76663C, 0x003C666E, 0x181E1C18, 0x00181818, 0x3060663C, 0x007E0C18, 0x3860663C, 0x003C6660, 
            0x33363C38, 0x0030307F, 0x603E067E, 0x003C6660, 0x3E060C38, 0x003C6666, 0x3060607E, 0x00181818,
            0x3C66663C, 0x003C6666, 0x7C66663C, 0x001C3060, 0x00181800, 0x00181800, 0x00181800, 0x0C181800, 
            0x06186000, 0x00006018, 0x007E0000, 0x0000007E, 0x60180600, 0x00000618, 0x3060663C, 0x00180018, 

            0x5A5A663C, 0x003C067A, 0x7E66663C, 0x00666666, 0x3E66663E, 0x003E6666, 0x06060C78, 0x00780C06, 
            0x6666361E, 0x001E3666, 0x1E06067E, 0x007E0606, 0x1E06067E, 0x00060606, 0x7606663C, 0x007C6666, 
            0x7E666666, 0x00666666, 0x1818183C, 0x003C1818, 0x60606060, 0x003C6660, 0x0F1B3363, 0x0063331B, 
            0x06060606, 0x007E0606, 0x6B7F7763, 0x00636363, 0x7B6F6763, 0x00636373, 0x6666663C, 0x003C6666, 
            0x3E66663E, 0x00060606, 0x3333331E, 0x007E3B33, 0x3E66663E, 0x00666636, 0x3C0E663C, 0x003C6670, 
            0x1818187E, 0x00181818, 0x66666666, 0x003C6666, 0x66666666, 0x00183C3C, 0x6B636363, 0x0063777F, 
            0x183C66C3, 0x00C3663C, 0x183C66C3, 0x00181818, 0x0C18307F, 0x007F0306, 0x0C0C0C3C, 0x003C0C0C, 
            0x180C0603, 0x00C06030, 0x3030303C, 0x003C3030, 0x00663C18, 0x00000000, 0x00000000, 0x003F0000, 

            0x00301818, 0x00000000, 0x603C0000, 0x007C667C, 0x663E0606, 0x003E6666, 0x063C0000, 0x003C0606, 
            0x667C6060, 0x007C6666, 0x663C0000, 0x003C067E, 0x0C3E0C38, 0x000C0C0C, 0x667C0000, 0x3C607C66, 
            0x663E0606, 0x00666666, 0x18180018, 0x00301818, 0x30300030, 0x1E303030, 0x36660606, 0x0066361E,
            0x18181818, 0x00301818, 0x7F370000, 0x0063636B, 0x663E0000, 0x00666666, 0x663C0000, 0x003C6666, 
            0x663E0000, 0x06063E66, 0x667C0000, 0x60607C66, 0x663E0000, 0x00060606, 0x063C0000, 0x003E603C, 
            0x0C3E0C0C, 0x00380C0C, 0x66660000, 0x007C6666, 0x66660000, 0x00183C66, 0x63630000, 0x00367F6B, 
            0x36630000, 0x0063361C, 0x66660000, 0x0C183C66, 0x307E0000, 0x007E0C18, 0x0C181830, 0x00301818, 
            0x18181818, 0x00181818, 0x3018180C, 0x000C1818, 0x003B6E00, 0x00000000, 0x00000000, 0x00000000, 
        };

        /// Mapping to character values
        private int[] textChars = new int[256];

        /// Texture atlas for font
        public Texture2D fontMap { get; private set; }

        /// <summary>
        /// Set up font structures
        /// </summary>
        public BitmapFont(GraphicsDevice graphicsDevice)
        {
            // Setup character mappings
            for (int i = 0; i < fontTiles.Length / 2; i++)
                textChars[i + 32] = i;

            // Create font map and write to texture
            fontMap = new Texture2D(graphicsDevice, 128, fontTiles.Length / 4);
            Color[] textureData = new Color[fontTiles.Length * 32];

            // Loop through bit array to get colors
            for (int i = 0; i < fontTiles.Length; i++)
            {
                int x = (i % 32) / 2;
                int y = (i / 32) * 2;
                y += (i % 2 == 0) ? 0 : 1;

                x <<= 3;
                y <<= 2;

                int bytePos = 0;
                while (bytePos < 4)
                {
                    uint nextBits = (fontTiles[i] >> (bytePos * 8)) & 0xff;
                    int bitPos = 0;

                    int nx = x;

                    while (bitPos < 8)
                    {
                        int offset = (y * 128) + nx;
                        textureData[offset] = ((nextBits & 1) == 1) ? Color.White : Color.Transparent;
                        nextBits = nextBits >> 1;

                        bitPos++;
                        nx++;
                    }

                    y++;

                    bytePos++;
                }
            }

            // Add colors to the texture
            fontMap.SetData(textureData);
        }


    }
}
