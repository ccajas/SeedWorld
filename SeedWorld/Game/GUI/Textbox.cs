using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld.GUI
{
    public delegate void TextBoxEvent(TextBox sender);

    public class TextBox : IKeyboardSubscriber
    {
        /// <summary>
        /// Graphics resources
        /// </summary>
        Texture2D _textBoxTexture;
        Texture2D _caretTexture;
        SpriteFont _font;

        bool caretVisible = true;

        public Point position
        {
            get; set;
        }

        public int Width 
        { 
            get; set;         
        }

        public int Height 
        { 
            get; private set;
        }

        public bool Highlighted 
        { 
            get; set; 
        }

        public bool PasswordBox 
        { 
            get; set; 
        }

        public event TextBoxEvent Clicked;
        public event TextBoxEvent OnEnterPressed;
        public event TextBoxEvent OnTabPressed;

        string _text = "";

        public bool Selected
        {
            get; set;
        }

        /// <summary>
        /// Get or set the text in the box
        /// </summary>
        public String Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                if (_text == null)
                    _text = "";

                if (_text != "")
                {
                    //if you attempt to display a character that is not in your font
                    //you will get an exception, so we filter the characters
                    //remove the filtering if you're using a default character in your spritefont
                    String filtered = "";
                    foreach (char c in value)
                    {
                        if (_font.Characters.Contains(c))
                            filtered += c;
                    }

                    _text = filtered;

                    while (_font.MeasureString(_text).X > Width)
                    {
                        //to ensure that text cannot be larger than the box
                        _text = _text.Substring(0, _text.Length - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize TextBox with textures, font and position
        /// </summary>
        public TextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Point position)
        {
            _textBoxTexture = textBoxTexture;
            _caretTexture = caretTexture;
            _font = font;
            this.position = position;
        }

        /// <summary>
        /// Update and take input
        /// </summary>
        public void Update(GameTime gameTime, MouseState previousMouseState)
        {
            MouseState mouse = Mouse.GetState();
            Point mousePoint = new Point(mouse.X, mouse.Y);

            Rectangle rectangle = new Rectangle(position.X, position.Y, Width, Height);
            if (rectangle.Contains(mousePoint))
            {
                Highlighted = true;
                if (previousMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed)
                {
                    if (Clicked != null)
                        Clicked(this);
                }
            }
            else
            {
                Highlighted = false;
            }

            // Make the caret blink

            if ((gameTime.TotalGameTime.TotalMilliseconds % 1000) < 500)
                caretVisible = false;
            else
                caretVisible = true;
        }

        /// <summary>
        /// Display the text input box
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            String toDraw = Text;

            if (PasswordBox)
            {
                toDraw = "";
                for (int i = 0; i < Text.Length; i++)
                    toDraw += (char)0x2022; //bullet character (make sure you include it in the font!!!!)
            }

            // My texture was split vertically in 2 parts, upper was unhighlighted, lower was highlighted version of the box
            spriteBatch.Draw(_textBoxTexture, 
                new Rectangle(position.X, position.Y, Width, Height), 
                new Rectangle(0, Highlighted ? 
                    (_textBoxTexture.Height / 2) : 0, 
                    _textBoxTexture.Width, _textBoxTexture.Height / 2), 
                    Color.White
                );

            Vector2 size = _font.MeasureString(toDraw);

            // My caret texture was a simple vertical line, 4 pixels smaller than font size.Y
            if (caretVisible && Selected)
                spriteBatch.Draw(_caretTexture, new Vector2(position.X + (int)size.X + 2, position.Y + 2), Color.White); 

            // Shadow first, then the actual text
            spriteBatch.DrawString(_font, toDraw, new Vector2(position.X, position.Y) + Vector2.One, Color.Black);
            spriteBatch.DrawString(_font, toDraw, new Vector2(position.X, position.Y), Color.White);
        }

        /// <summary>
        /// Add a character to the input area
        /// </summary>
        public void RecieveTextInput(char inputChar)
        {
            Text = Text + inputChar;
        }

        /// <summary>
        /// Add more text to the input area
        /// </summary>
        public void RecieveTextInput(string text)
        {
            Text = Text + text;
        }

        public void RecieveCommandInput(char command)
        {
            switch (command)
            {
                case '\b': //backspace
                    if (Text.Length > 0)
                        Text = Text.Substring(0, Text.Length - 1);
                    break;
                case '\r': //return
                    if (OnEnterPressed != null)
                        OnEnterPressed(this);
                    break;
                case '\t': //tab
                    if (OnTabPressed != null)
                        OnTabPressed(this);
                    break;
                default:
                    break;
            }
        }
        public void RecieveSpecialInput(Keys key)
        {

        }
    }
}
