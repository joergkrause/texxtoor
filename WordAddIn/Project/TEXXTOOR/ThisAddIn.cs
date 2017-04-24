using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using TEXXTOOR.Services;

namespace TEXXTOOR {

  public partial class ThisAddIn {

    private RibbonTexxtoor ribbonHandler;
    private KeyboardHook hooker;
    private const byte TAB = 9;

    protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject() {
      ribbonHandler = new RibbonTexxtoor();
      return ribbonHandler;
    }
    private void ThisAddIn_Startup(object sender, EventArgs e) {
      hooker = new KeyboardHook();
      hooker.OnKeyboardEvent += new KeyboardHook.OnKeyboardEventDelegate(KeyHooked);
      hooker.OnMouseEvent += new KeyboardHook.OnMouseEventDelegate(MouseHooked);
      //Application.WindowSelectionChange += 
      //    new ApplicationEvents4_WindowSelectionChangeEventHandler(Application_WindowSelectionChange);
    }

    //void Application_WindowSelectionChange(Selection Sel)
    //{
    //    //Debug.Print(Sel.Range.Text);
    //    Debug.Print("sel change");
    //}

    void KeyHooked(byte key, byte state, ref bool callNextHook) {
      //if (ServicePool.Instance.GetService<DocumentService>().te == null)
      //    return;

      if (KeyboardHook.CTRL > 0 && (char)key == 'V') {
        if (state == (byte)KeyState.KEY_DOWN) {
          ServicePool.Instance.GetService<CommandService>().PlainPaste(null, null);
        }
        callNextHook = false;
      } else if (key == TAB) {
        /*********** THIS PART OF CODE IS COPIED AS IT FROM *****************/
        /*********** CommandService internal void IndentParagraph(string strOfficeId) ***/
        if (state == (byte)KeyState.KEY_DOWN) {
          var currentSelection = ServicePool.Instance.GetService<DocumentService>().CurrentSelection;
          var lst = currentSelection.Range.ListFormat.List;
          if (lst != null) {
            ServicePool.Instance.GetService<CommandService>().IndentParagraph();
          }
        }
        callNextHook = false;
      }
    }

    void MouseHooked(long x, long y, byte key, ref bool callNextHook) {
      Debug.WriteLine("{0} x {1}", x, y);
    }

    private void ThisAddIn_Shutdown(object sender, EventArgs e) {
      hooker.Dispose();
    }

    enum KeyState { KEY_DOWN = 0, KEY_UP = 1 }

    #region VSTO generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup() {
      Startup += new EventHandler(ThisAddIn_Startup);
      Shutdown += new EventHandler(ThisAddIn_Shutdown);
    }

    #endregion


  }
}
