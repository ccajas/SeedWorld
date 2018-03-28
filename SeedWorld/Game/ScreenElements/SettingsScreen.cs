using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using NuclearUI = NuclearWinter.UI;

namespace SeedWorld.ScreenElements
{
    class SettingsScreen : InteractiveScreenElement
    {
        // Test settings menu manager
        public MainMenuManager MainMenuManager { get; private set; }

        // Style for the menu
        NuclearUI.Style UIstyle;

        public SettingsScreen(Game game, ScreenElement previousScreenElement) : 
            base(previousScreenElement, game.GraphicsDevice)
        {
            LoadUIStyle(game.Content);

            MainMenuManager = new MainMenuManager(game, UIstyle, game.Content);
            game.IsMouseVisible = true;
        }

        /// <summary>
        /// Load the UI style
        /// </summary>
        public void LoadUIStyle(ContentManager content)
        {
            UIstyle = new NuclearUI.Style();

            UIstyle.SmallFont = new NuclearUI.UIFont(content.Load<SpriteFont>("Fonts/SmallFont"), 14, 0);
            UIstyle.MediumFont = new NuclearUI.UIFont(content.Load<SpriteFont>("Fonts/MediumFont"), 18, -2);
            UIstyle.LargeFont = new NuclearUI.UIFont(content.Load<SpriteFont>("Fonts/LargeFont"), 24, 0);
            UIstyle.ExtraLargeFont = new NuclearUI.UIFont(content.Load<SpriteFont>("Fonts/LargeFont"), 24, 0);

            UIstyle.SpinningWheel = content.Load<Texture2D>("Textures/UI/SpinningWheel");

            UIstyle.DefaultTextColor = new Color(224, 224, 224);
            UIstyle.DefaultButtonHeight = 60;

            UIstyle.ButtonFrame = content.Load<Texture2D>("Textures/UI/ButtonFrame");
            UIstyle.ButtonDownFrame = content.Load<Texture2D>("Textures/UI/ButtonFrameDown");
            UIstyle.ButtonHoverOverlay = content.Load<Texture2D>("Textures/UI/ButtonHover");
            UIstyle.ButtonFocusOverlay = content.Load<Texture2D>("Textures/UI/ButtonFocus");
            UIstyle.ButtonDownOverlay = content.Load<Texture2D>("Textures/UI/ButtonPress");

            UIstyle.TooltipFrame = content.Load<Texture2D>("Textures/UI/TooltipFrame");

            UIstyle.ButtonCornerSize = 20;
            UIstyle.ButtonVerticalPadding = 10;
            UIstyle.ButtonHorizontalPadding = 15;

            UIstyle.RadioButtonCornerSize = UIstyle.ButtonCornerSize;
            UIstyle.RadioButtonFrameOffset = 7;
            UIstyle.ButtonFrameLeft = content.Load<Texture2D>("Textures/UI/ButtonFrameLeft");
            UIstyle.ButtonDownFrameLeft = content.Load<Texture2D>("Textures/UI/ButtonFrameLeftDown");

            UIstyle.ButtonFrameMiddle = content.Load<Texture2D>("Textures/UI/ButtonFrameMiddle");
            UIstyle.ButtonDownFrameMiddle = content.Load<Texture2D>("Textures/UI/ButtonFrameMiddleDown");

            UIstyle.ButtonFrameRight = content.Load<Texture2D>("Textures/UI/ButtonFrameRight");
            UIstyle.ButtonDownFrameRight = content.Load<Texture2D>("Textures/UI/ButtonFrameRightDown");

            UIstyle.EditBoxFrame = content.Load<Texture2D>("Textures/UI/EditBoxFrame");
            UIstyle.EditBoxCornerSize = 20;

            UIstyle.Panel = content.Load<Texture2D>("Textures/UI/Panel01");
            UIstyle.PanelCornerSize = 15;

            UIstyle.NotebookStyle.TabCornerSize = 15;
            UIstyle.NotebookStyle.Tab = content.Load<Texture2D>("Textures/UI/Tab");
            UIstyle.NotebookStyle.TabFocus = content.Load<Texture2D>("Textures/UI/ButtonFocus");
            UIstyle.NotebookStyle.ActiveTab = content.Load<Texture2D>("Textures/UI/ActiveTab");
            UIstyle.NotebookStyle.ActiveTabFocus = content.Load<Texture2D>("Textures/UI/ActiveTabFocused");
            UIstyle.NotebookStyle.TabClose = content.Load<Texture2D>("Textures/UI/TabClose");
            UIstyle.NotebookStyle.TabCloseHover = content.Load<Texture2D>("Textures/UI/TabCloseHover");
            UIstyle.NotebookStyle.TabCloseDown = content.Load<Texture2D>("Textures/UI/TabCloseDown");
            UIstyle.NotebookStyle.UnreadTabMarker = content.Load<Texture2D>("Textures/UI/UnreadTabMarker");

            UIstyle.ListViewStyle.ListViewFrame = content.Load<Texture2D>("Textures/UI/ListFrame");
            UIstyle.ListViewStyle.ListViewFrameCornerSize = 10;
            UIstyle.ListRowInsertMarker = content.Load<Texture2D>("Textures/UI/ListRowInsertMarker");

            UIstyle.ListViewStyle.CellFrame = content.Load<Texture2D>("Textures/UI/ListRowFrame");
            UIstyle.ListViewStyle.CellCornerSize = 10;
            UIstyle.ListViewStyle.SelectedCellFrame = content.Load<Texture2D>("Textures/UI/ListRowFrameSelected");
            UIstyle.ListViewStyle.CellFocusOverlay = content.Load<Texture2D>("Textures/UI/ListRowFrameFocused");
            UIstyle.ListViewStyle.CellHoverOverlay = content.Load<Texture2D>("Textures/UI/ListRowFrameHover");
            UIstyle.ListViewStyle.ColumnHeaderFrame = content.Load<Texture2D>("Textures/UI/ButtonFrame"); // FIXME

            UIstyle.PopupFrame = content.Load<Texture2D>("Textures/UI/PopupFrame");
            UIstyle.PopupFrameCornerSize = 30;

            UIstyle.CheckBoxFrameHover = content.Load<Texture2D>("Textures/UI/CheckBoxFrameHover");
            UIstyle.CheckBoxChecked = content.Load<Texture2D>("Textures/UI/Checked");
            UIstyle.CheckBoxUnchecked = content.Load<Texture2D>("Textures/UI/Unchecked");

            UIstyle.SliderFrame = content.Load<Texture2D>("Textures/UI/ListFrame");

            UIstyle.VerticalScrollbar = content.Load<Texture2D>("Textures/UI/VerticalScrollbar");
            UIstyle.VerticalScrollbarCornerSize = 5;

            UIstyle.DropDownBoxEntryHoverOverlay = content.Load<Texture2D>("Textures/UI/ListRowFrameFocused");
            UIstyle.DropDownArrow = content.Load<Texture2D>("Textures/UI/DropDownArrow");

            UIstyle.SplitterFrame = content.Load<Texture2D>("Textures/UI/SplitterFrame");
            UIstyle.SplitterDragHandle = content.Load<Texture2D>("Textures/UI/SplitterDragHandle");
            UIstyle.SplitterCollapseArrow = content.Load<Texture2D>("Textures/UI/SplitterCollapseArrow");

            UIstyle.ProgressBarFrame = content.Load<Texture2D>("Textures/UI/EditBoxFrame");
            UIstyle.ProgressBarFrameCornerSize = 15;
            UIstyle.ProgressBar = content.Load<Texture2D>("Textures/UI/ProgressBar");
            UIstyle.ProgressBarCornerSize = 15;

            UIstyle.TextAreaFrame = content.Load<Texture2D>("Textures/UI/ListFrame");
            UIstyle.TextAreaFrameCornerSize = 15;
            UIstyle.TextAreaGutterFrame = content.Load<Texture2D>("Textures/UI/TextAreaGutterFrame");
            UIstyle.TextAreaGutterCornerSize = 15;
        }

        /// <summary>
        /// Receive input from menu
        /// </summary>
        public override void HandleInput(TimeSpan frameStepTime)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Update the Settings screen
        /// </summary>
        public override ScreenElement Update(TimeSpan frameStepTime)
        {
            float elapsedTime = (float)frameStepTime.TotalSeconds;
            MainMenuManager.Update(elapsedTime);

            return base.Update(frameStepTime);
        }

        /// <summary>
        /// Called for resizing forms when window is resized
        /// </summary>
        protected override void OnDeviceReset(object sender, EventArgs e)
        {
            MainMenuManager.MenuScreen.Resize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            MainMenuManager.PopupScreen.Resize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
        }

        /// <summary>
        /// Draw the Settings menu on the screen
        /// </summary>
        public override void Draw(TimeSpan frameStepTime)
        {
            //graphicsDevice.Clear(new Color(111, 125, 120));
            MainMenuManager.Draw();
            
            base.Draw(frameStepTime);
        }
    }
}
