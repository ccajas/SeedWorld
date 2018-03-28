using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NuclearUI = NuclearWinter.UI;

namespace SeedWorld.DemoMenus
{
    class BasicDemoPane : NuclearUI.ManagerPane<MainMenuManager>
    {
        enum Flavor
        {
            Chocolate,
            Vanilla,
            Cheese
        }

        //----------------------------------------------------------------------
        public BasicDemoPane(MainMenuManager _manager)
            : base(_manager)
        {
            int iRows = 3;

            var gridGroup = new NuclearUI.GridGroup(Manager.MenuScreen, 2, iRows, false, 0);
            gridGroup.AnchoredRect = NuclearUI.AnchoredRect.CreateTopLeftAnchored(0, 0, 400, iRows * 50);

            var gridGroup2 = new NuclearUI.GridGroup(Manager.MenuScreen, 1, 1, false, 0);
            gridGroup2.AnchoredRect = NuclearUI.AnchoredRect.CreateBottomAnchored(20, 0, 20, 50);

            AddChild(gridGroup);
            AddChild(gridGroup2);

            int iRowIndex = 0;

            //------------------------------------------------------------------
            gridGroup.AddChildAt(new NuclearUI.Label(Manager.MenuScreen, "Select Flavor", NuclearUI.Anchor.Start), 0, iRowIndex);

            {

                var lItems = new List<NuclearUI.DropDownItem>();
                lItems.Add(new NuclearUI.DropDownItem(Manager.MenuScreen, "Chocolate", Flavor.Chocolate));
                lItems.Add(new NuclearUI.DropDownItem(Manager.MenuScreen, "Vanilla", Flavor.Vanilla));
                lItems.Add(new NuclearUI.DropDownItem(Manager.MenuScreen, "Cheese", Flavor.Cheese));

                List<NuclearWinter.ScreenMode> modes = NuclearWinter.Resolution.SortedScreenModes;

                foreach (NuclearWinter.ScreenMode mode in modes)
                {
                    lItems.Add(new NuclearUI.DropDownItem(Manager.MenuScreen, mode.ToString(), Flavor.Vanilla));
                }

                NuclearUI.DropDownBox dropDownBox = new NuclearUI.DropDownBox(Manager.MenuScreen, lItems, 0);
                gridGroup.AddChildAt(dropDownBox, 1, iRowIndex);
            }

            iRowIndex++;

            //------------------------------------------------------------------
            gridGroup.AddChildAt(new NuclearUI.Label(Manager.MenuScreen, "Draw Distance", NuclearUI.Anchor.Start), 0, iRowIndex);

            {
                var sizeSlider = new NuclearUI.Slider(Manager.MenuScreen, 1, 5, 1, 1);
                gridGroup.AddChildAt(sizeSlider, 1, iRowIndex);
            }

            iRowIndex++;

            //------------------------------------------------------------------
            gridGroup.AddChildAt(new NuclearUI.Label(Manager.MenuScreen, "Clicky Clicky", NuclearUI.Anchor.Start), 0, iRowIndex);

            {
                var button = new NuclearUI.Button(Manager.MenuScreen, "Go!");
                button.ClickHandler = delegate
                {
                    Manager.MessagePopup.Setup("Oh noes!", "It melted already. Sorry.", NuclearWinter.i18n.Common.Close, false);
                    Manager.MessagePopup.Open(600, 250);
                };
                gridGroup.AddChildAt(button, 1, iRowIndex);
            }

            iRowIndex++;

            //------------------------------------------------------------------

            // Add button to second GridGroup
            {
                var button = new NuclearUI.Button(Manager.MenuScreen, "Apply Changes");
                button.ClickHandler = delegate
                {
                    Manager.MessagePopup.Setup("Oh noes!", "It melted already. Sorry.", NuclearWinter.i18n.Common.Close, false);
                    Manager.MessagePopup.Open(600, 250);
                };
                gridGroup2.AddChildAt(button, 0, 0);
            }
        }

        //----------------------------------------------------------------------
    }
}
