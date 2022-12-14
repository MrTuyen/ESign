using System;

namespace WebSockets.Events
{
    public class TextFrameEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public TextFrameEventArgs(string text)
        {
            Text = text;
        }
    }
}
