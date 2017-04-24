using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace TEXXTOOR
{
    /// <summary>
    /// Low-level keyboard intercept class to trap and suppress system keys.
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        /// <summary>
        /// Parameters accepted by the KeyboardHook constructor.
        /// </summary>
        public enum Parameters
        {
            None,
            AllowAltTab,
            AllowWindowsKey,
            AllowAltTabAndWindows,
            PassAllKeysToNextApp
        }

        //Internal parameters
        private bool PassAllKeysToNextApp = false;
        private bool AllowAltTab = false;
        private bool AllowWindowsKey = false;

        //Keyboard API constants
		private const int WH_MOUSE_LL = 14;
	    private const int WM_LBUTTONUP = 0x0202;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        //Mouse API COnstants
        public const int WH_MOUSE = 7;
        //Modifier key constants
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_CAPITAL = 0x14;

        //Variables used in the call to SetWindowsHookEx
        private HandlerDelegate kProc;
		private HandlerDelegate mProc;
        private IntPtr hookKeyID = IntPtr.Zero;
	    private IntPtr hookMouseID = IntPtr.Zero;
        public delegate void OnKeyboardEventDelegate(byte VKey, byte Event, ref bool callNextHook);
        public OnKeyboardEventDelegate OnKeyboardEvent;

		public delegate void OnMouseEventDelegate(long x, long y, byte key, ref bool callNextHook);
		public OnMouseEventDelegate OnMouseEvent;

        internal delegate IntPtr HandlerDelegate(int nCode, IntPtr wParam, ref IntPtr lParam);

        /// <summary>
        /// Event triggered when a keystroke is intercepted by the 
        /// low-level hook.
        /// </summary>
        public event KeyboardHookEventHandler KeyIntercepted;

        // Structure returned by the hook whenever a key is pressed
        internal struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            int scanCode;
            public int flags;
            int time;
            int dwExtraInfo;
        }

	    internal struct POINT {
		    public long x;
		    public long y;
	    }

		internal struct MSLLHOOKSTRUCT {
			public POINT pt;
			int mouseData;
			public int flags;
			int time;
			int dwExtraInfo;
		}

        #region Constructors
        /// <summary>
        /// Sets up a keyboard hook to trap all keystrokes without 
        /// passing any to other applications.
        /// </summary>
        public KeyboardHook()
        {
            kProc = new HandlerDelegate(HookCallbackKey);
	        mProc = new HandlerDelegate(HookCallbackMouse);
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
				hookKeyID = NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, kProc,
					NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
				//hookMouseID = NativeMethods.SetWindowsHookEx(WH_MOUSE_LL, mProc,
				//    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        /// Sets up a keyboard hook with custom parameters.
        /// </summary>
        /// <param name="param">A valid name from the Parameter enum; otherwise, the 
        /// default parameter Parameter.None will be used.</param>
        public KeyboardHook(string param)
            : this()
        {
            if (!String.IsNullOrEmpty(param) && Enum.IsDefined(typeof(Parameters), param))
            {
                SetParameters((Parameters)Enum.Parse(typeof(Parameters), param));
            }
        }

        /// <summary>
        /// Sets up a keyboard hook with custom parameters.
        /// </summary>
        /// <param name="param">A value from the Parameters enum.</param>
        public KeyboardHook(Parameters param)
            : this()
        {
            SetParameters(param);
        }

        private void SetParameters(Parameters param)
        {
            switch (param)
            {
                case Parameters.None:
                    break;
                case Parameters.AllowAltTab:
                    AllowAltTab = true;
                    break;
                case Parameters.AllowWindowsKey:
                    AllowWindowsKey = true;
                    break;
                case Parameters.AllowAltTabAndWindows:
                    AllowAltTab = true;
                    AllowWindowsKey = true;
                    break;
                case Parameters.PassAllKeysToNextApp:
                    PassAllKeysToNextApp = true;
                    break;
            }
        }
        #endregion

        #region Check Modifier keys
        /// <summary>
        /// Checks whether Alt, Shift, Control or CapsLock
        /// is enabled at the same time as another key.
        /// Modify the relevant sections and return type 
        /// depending on what you want to do with modifier keys.
        /// </summary>



        private void CheckModifiers()
        {
            StringBuilder sb = new StringBuilder();

            if ((NativeMethods.GetKeyState(VK_CAPITAL) & 0x0001) != 0)
            {
                //CAPSLOCK is ON
                sb.AppendLine("Capslock is enabled.");
            }

            if ((NativeMethods.GetKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                //SHIFT is pressed
                sb.AppendLine("Shift is pressed.");
            }
            if ((NativeMethods.GetKeyState(VK_CONTROL) & 0x8000) != 0)
            {
                //CONTROL is pressed
                sb.AppendLine("Control is pressed.");
            }
            if ((NativeMethods.GetKeyState(VK_MENU) & 0x8000) != 0)
            {
                //ALT is pressed
                sb.AppendLine("Alt is pressed.");
            }
            Console.WriteLine(sb.ToString());
        }
        #endregion Check Modifier keys

        #region Hook Callback Method
        /// <summary>
        /// Processes the key event captured by the hook.
        /// </summary>
        private IntPtr HookCallbackKey(int nCode, IntPtr wParam, ref IntPtr lParam)
        {
            bool callNextHook = true;
            if (NativeMethods.GetActiveWindow() != IntPtr.Zero) {
	            KBDLLHOOKSTRUCT lPara = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                if (nCode >= 0)
                {
                    byte flag=(byte)((wParam == (IntPtr)WM_KEYDOWN|| wParam == (IntPtr)WM_SYSKEYDOWN)?0:1);
					OnKeyboardEvent((byte)lPara.vkCode, flag, ref callNextHook);
                    if (!callNextHook)
                    {
                        return new IntPtr(-1);
                    }
                }
            }
            return NativeMethods.CallNextHookEx(hookKeyID, nCode, wParam, lParam);
        }

		private IntPtr HookCallbackMouse(int nCode, IntPtr wParam, ref IntPtr lParam) {
			bool callNextHook = true;
			if (NativeMethods.GetActiveWindow() != IntPtr.Zero) {				
				if (nCode >= 0 && lParam != IntPtr.Zero) {
					MSLLHOOKSTRUCT lPara = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
					if (wParam.ToInt32() == WM_LBUTTONUP) {
						OnMouseEvent(lPara.pt.x, lPara.pt.y, 1, ref callNextHook);
					}
					if (!callNextHook) {
						return new IntPtr(-1);
					}
				}
			}
			return NativeMethods.CallNextHookEx(hookMouseID, nCode, wParam, lParam);
		}
        
        #endregion

        #region Event Handling
        /// <summary>
        /// Raises the KeyIntercepted event.
        /// </summary>
        /// <param name="e">An instance of KeyboardHookEventArgs</param>
        public void OnKeyIntercepted(KeyboardHookEventArgs e)
        {
            if (KeyIntercepted != null)
                KeyIntercepted(e);
        }

        /// <summary>
        /// Delegate for KeyboardHook event handling.
        /// </summary>
        /// <param name="e">An instance of InterceptKeysEventArgs.</param>
        public delegate void KeyboardHookEventHandler(KeyboardHookEventArgs e);

        public static int ALT { get { return (NativeMethods.GetKeyState(VK_MENU) & 0x8000) != 0 ? 1 : 0; } }
        public static int CTRL { get { return (NativeMethods.GetKeyState(VK_CONTROL) & 0x8000) != 0 ? 1 : 0; } }
        public static int CAPS { get { return (NativeMethods.GetKeyState(VK_CAPITAL) & 0x0001) != 0 ? 1 : 0; } }
        public static int SHIFT { get { return (NativeMethods.GetKeyState(VK_SHIFT) & 0x8000) != 0 ? 1 : 0; } }

        /// <summary>
        /// Event arguments for the KeyboardHook class's KeyIntercepted event.
        /// </summary>
        public class KeyboardHookEventArgs : System.EventArgs
        {

            private string keyName;
            private int keyCode;
            private bool passThrough;

            //public static int ALT { get { return (NativeMethods.GetKeyState(VK_MENU) & 0x8000) != 0 ? 1 : 0; } }
            //public static int CTRL { get { return (NativeMethods.GetKeyState(VK_CONTROL) & 0x8000) != 0 ? 1 : 0; } }
            //public static int CAPS { get { return (NativeMethods.GetKeyState(VK_CAPITAL) & 0x0001) != 0 ? 1 : 0; } }
            //public static int SHIFT { get { return (NativeMethods.GetKeyState(VK_SHIFT) & 0x8000) != 0 ? 1 : 0; } }

            /// <summary>
            /// The name of the key that was pressed.
            /// </summary>
            public string KeyName
            {
                get { return keyName; }
            }

            /// <summary>
            /// The virtual key code of the key that was pressed.
            /// </summary>
            public int KeyCode
            {
                get { return keyCode; }
            }

            public int SysKey
            {
                get
                {
                    if ((NativeMethods.GetKeyState(VK_CONTROL) & 0x8000) != 0)
                        return 0xA2;
                    else if ((NativeMethods.GetKeyState(VK_MENU) & 0x8000) != 0)
                        return 0xA4;
                    else if ((NativeMethods.GetKeyState(VK_SHIFT) & 0x8000) != 0)
                        return 0xA1;
                    else
                        return 0;

                }
            }

            /// <summary>
            /// True if this key combination was passed to other applications,
            /// false if it was trapped.
            /// </summary>
            public bool PassThrough
            {
                get { return passThrough; }
            }

            public KeyboardHookEventArgs(int evtKeyCode, bool evtPassThrough)
            {
                keyName = ((Keys)evtKeyCode).ToString();
                keyCode = evtKeyCode;
                passThrough = evtPassThrough;
            }

        }

        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases the keyboard hook.
        /// </summary>
        public void Dispose()
        {
            NativeMethods.UnhookWindowsHookEx(hookKeyID);
			NativeMethods.UnhookWindowsHookEx(hookMouseID);
        }
        #endregion

        #region Native methods

        [ComVisibleAttribute(false),
         System.Security.SuppressUnmanagedCodeSecurity()]
        internal class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetActiveWindow();

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook,
                HandlerDelegate lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
            public static extern short GetKeyState(int keyCode);

        }


        #endregion
    }
}

