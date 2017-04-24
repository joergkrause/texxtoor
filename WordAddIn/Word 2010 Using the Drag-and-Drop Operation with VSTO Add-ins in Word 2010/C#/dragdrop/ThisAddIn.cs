using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;


namespace dragdrop
{
    public partial class ThisAddIn
    {
        
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Globals.ThisAddIn.CustomTaskPanes.Add(new OrdersListUserControl(), "Drag and Drop List Items on Word Doc").Visible = true;
            Globals.ThisAddIn.Application.WindowActivate += new Word.ApplicationEvents4_WindowActivateEventHandler(Application_WindowActivate);
        }

        void Application_WindowActivate(Word.Document Doc, Word.Window Wn)
        {
            if (OverlayForm != null && OverlayForm.Visible)
             
                OverlayForm.Opacity = 100;     
            

        }

       
        public OverlayForm OverlayForm { get; set; }
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }
        /// <summary>
        /// Called when [drop occurred].
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="data">The data.</param>
        public void OnDropOccurred(int x, int y, object data)
        {
            //Get the Word range from the form's point location 
            Microsoft.Office.Interop.Word.Range range = (Microsoft.Office.Interop.Word.Range)Globals.ThisAddIn.Application.ActiveWindow.RangeFromPoint(x, y);
            //Insert a dummy details table for the selected order
            Word.Table table=this.Application.ActiveDocument.Tables.Add(range,4,4);
            table.Range.Font.Size = 8;
            table.set_Style("Table Grid 8");
            table.Rows[1].Cells[1].Range.Text = "Order Details";
            table.Rows[1].Cells[2].Range.Text = "Order Details";
            table.Rows[1].Cells[3].Range.Text = "Order Details";
            table.Rows[1].Cells[4].Range.Text = "Order Details";
            for (int i = 2; i < 5; i++)
			{
                for (int j = 1; j < 5; j++)
                {
                    table.Rows[i].Cells[j].Range.Text = data.ToString();    
                }
                
			}
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);

        }

        
        #endregion
    }
}
