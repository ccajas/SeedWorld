using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using NuclearUI = NuclearWinter.UI;

namespace SeedWorld
{
    //--------------------------------------------------------------------------
    internal class MainMenuManager: NuclearUI.MenuManager<Game>
    {
        NuclearUI.Splitter      mSplitter;
        NuclearUI.Panel         mDemoPanel;

        //----------------------------------------------------------------------
        public MainMenuManager(Game _game, NuclearUI.Style UIstyle, ContentManager _content )
        : base( _game, UIstyle, _content )
        {
            NuclearUI.Button button = new NuclearUI.Button(MenuScreen, "TestButton");

            // Splitter
            mSplitter = new NuclearUI.Splitter( MenuScreen, NuclearUI.Direction.Up );
            mSplitter.AnchoredRect = NuclearUI.AnchoredRect.CreateCentered(500, 400);// CreateFull(100, 100, 100, 100);
            MenuScreen.Root.AddChild( mSplitter );
            mSplitter.Collapsable = true;

            mSplitter.FirstPaneMinSize = 100;

            // Demo list
            var demosBoxGroup = new NuclearUI.BoxGroup( MenuScreen, NuclearUI.Orientation.Horizontal, 0, NuclearUI.Anchor.Start );
            mSplitter.FirstPane = demosBoxGroup;

            mDemoPanel = new NuclearUI.Panel( 
                MenuScreen, Content.Load<Texture2D>( "Textures/UI/Panel04" ), 
                MenuScreen.Style.PanelCornerSize );

            var basicDemoPane = new DemoMenus.BasicDemoPane( this );
            mSplitter.SecondPane = mDemoPanel;

            mDemoPanel.AddChild( basicDemoPane );

            // Test button
            var testButton = new NuclearUI.Button(MenuScreen, "Test");
            testButton.ClickHandler = delegate
            {
                mDemoPanel.Clear();
            };

            demosBoxGroup.AddChild( CreateDemoButton( "Basic", basicDemoPane ), true );
            demosBoxGroup.AddChild( testButton, true);

            //demosBoxGroup.AddChild( CreateDemoButton( "Notebook", new Demos.NotebookPane( this ) ), true );
            //demosBoxGroup.AddChild( CreateDemoButton( "Text Area", new Demos.TextAreaPane( this ) ), true );
            //demosBoxGroup.AddChild( CreateDemoButton( "Custom Viewport", new Demos.CustomViewportPane( this ) ), true );
        }

        //----------------------------------------------------------------------
        NuclearUI.Button CreateDemoButton( string _strDemoName, NuclearUI.ManagerPane<MainMenuManager> _demoPane )
        {
            var demoPaneButton = new NuclearUI.Button( MenuScreen, _strDemoName );
            demoPaneButton.ClickHandler = delegate {
                mDemoPanel.Clear();
                mDemoPanel.AddChild( _demoPane );
            };

            return demoPaneButton;
        }
    }
}
