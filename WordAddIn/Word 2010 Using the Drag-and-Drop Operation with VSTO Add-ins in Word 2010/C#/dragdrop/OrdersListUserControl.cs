using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace dragdrop
{
    /// <summary>
    /// User control 
    /// </summary>
    public partial class OrdersListUserControl : UserControl
    {
      
 
        public OrdersListUserControl()
        {
            InitializeComponent();
        }
        static OverlayForm form;
        /// <summary>
        /// Overlays the form on top of the Word document.
        /// </summary>
        private void OverlayForm()
        {
            if (form == null)
            {
                form = new OverlayForm();
                form.AllowDrop = true;
                IntPtr parent = GetCurrentWindowHandle();
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                WinAPI.GetWindowRect(parent, ref rect);
                uint uFlags = WinAPI.SWP_NOACTIVATE | WinAPI.SWP_NOZORDER;
                WinAPI.SetWindowPos((uint)form.Handle.ToInt32(), parent.ToInt32(), 0, 0, rect.Right, rect.Bottom, uFlags);
                WinAPI.SetParent(form.Handle, parent);
                form.AllowTransparency = true;

                Globals.ThisAddIn.OverlayForm = form;
                form.Show();
            }
            else
            {
                if (form.Visible == false)
                {
                    form.Show();
                }

            }
        }
        /// <summary>
        /// Gets the current window handle.
        /// </summary>
        /// <returns></returns>
        private IntPtr GetCurrentWindowHandle()
        {
            IntPtr windowHandle = IntPtr.Zero;

            windowHandle = WinAPI.FindWindow("OpusApp", null);

            windowHandle = WinAPI.FindWindowEx(windowHandle, IntPtr.Zero, "_WwF", null);
            windowHandle = WinAPI.FindWindowEx(windowHandle, IntPtr.Zero, "_WwB", null);
            windowHandle = WinAPI.FindWindowEx(windowHandle, IntPtr.Zero, "_WwG", null);

            return windowHandle;
        }
        private int lastMouseUpItemIndex = -1;
        private int lastLine = -1;
        private void DrawVisualQue(Color color, ref Rectangle rectangle)
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            g.DrawLine(new Pen(color), rectangle.X, lastLine, rectangle.Width, lastLine);
        }
        /// <summary>
        /// Called when [drag leave].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnDragLeave(object sender, EventArgs e)
        {
            OverlayForm();
        }
        /// <summary>
        /// Called when [drag drop].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        private void OnDragDrop(object sender, DragEventArgs e)
        {
            HideOverlayForm();
        }
        
        public static void HideOverlayForm()
        {
            if (form != null)
            {
                if (form.Visible)
                {
                    form.Hide();
                }
            }
        }
        /// <summary>
        /// Called when [mouse down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left
                && this.listBox1.SelectedIndex == lastMouseUpItemIndex
                && this.listBox1.SelectedItem != null)
            {
                
                DoDragDrop(this.listBox1.SelectedItem, DragDropEffects.Copy);
                
                lastMouseUpItemIndex = this.listBox1.SelectedIndex;
            }
        }
        /// <summary>
        /// Called when [selected index changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSelectedIndexChanged(object sender, System.EventArgs e)
        {
            lastMouseUpItemIndex = this.listBox1.SelectedIndex;
        }
    }
}