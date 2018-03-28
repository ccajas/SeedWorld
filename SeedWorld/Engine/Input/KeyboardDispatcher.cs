using System;
using System.Threading;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeedWorld
{
    public interface IKeyboardSubscriber
    {
        void RecieveTextInput(char inputChar);
        void RecieveTextInput(string text);
        void RecieveCommandInput(char command);
        void RecieveSpecialInput(Keys key);

        bool Selected { get; set; } //or Focused
    }

    public class KeyboardDispatcher
    {
        public KeyboardDispatcher(GameWindow window)
        {
            EventInput.EventInput.Initialize(window);
            EventInput.EventInput.CharEntered += new EventInput.CharEnteredHandler(EventInput_CharEntered);
            EventInput.EventInput.KeyDown += new EventInput.KeyEventHandler(EventInput_KeyDown);
        }

        void EventInput_KeyDown(object sender, EventInput.KeyEventArgs e)
        {
            if (_subscriber == null)
                return;

            _subscriber.RecieveSpecialInput(e.KeyCode);
        }

        void EventInput_CharEntered(object sender, EventInput.CharacterEventArgs e)
        {
            if (_subscriber == null)
                return;
            if (char.IsControl(e.Character))
            {
                //ctrl-v
                if (e.Character == 0x16)
                {
                    //XNA runs in Multiple Thread Apartment state, which cannot recieve clipboard
                    Thread thread = new Thread(PasteThread);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    _subscriber.RecieveTextInput(_pasteResult);
                }
                else
                {
                    _subscriber.RecieveCommandInput(e.Character);
                }
            }
            else
            {
                _subscriber.RecieveTextInput(e.Character);
            }
        }

        IKeyboardSubscriber _subscriber;
        public IKeyboardSubscriber Subscriber
        {
            get { return _subscriber; }
            set
            {
                if (_subscriber != null)
                    _subscriber.Selected = false;
                _subscriber = value;
                if (value != null)
                    value.Selected = true;
            }
        }

        //Thread has to be in Single Thread Apartment state in order to receive clipboard
        string _pasteResult = "";
        [STAThread]
        void PasteThread()
        {
            if (Clipboard.ContainsText())
            {
                _pasteResult = Clipboard.GetText();
            }
            else
            {
                _pasteResult = "";
            }
        }
    }
}