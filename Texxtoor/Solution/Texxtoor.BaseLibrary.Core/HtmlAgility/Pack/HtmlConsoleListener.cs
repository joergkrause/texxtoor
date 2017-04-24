// Texxtoor.BaseLibrary.HtmlAgility.Pack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Diagnostics;

namespace Texxtoor.BaseLibrary.Core.HtmlAgility.Pack
{
    internal class HtmlConsoleListener : TraceListener
    {
        #region Public Methods

        public override void Write(string Message)
        {
            Write(Message, "");
        }

        public override void Write(string Message, string Category)
        {
            Console.Write("T:" + Category + ": " + Message);
        }

        public override void WriteLine(string Message)
        {
            Write(Message + "\n");
        }

        public override void WriteLine(string Message, string Category)
        {
            Write(Message + "\n", Category);
        }

        #endregion
    }
}