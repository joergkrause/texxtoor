namespace DragDrop
{
    using System;
    using System.Drawing;
    using System.Collections;
    using System.ComponentModel;
    using System.Windows.Forms;
    using System.Data;
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private DragDrop.Form1.ListBoxDragNDrop listBox1;
        private DragDrop.Form1.ListBoxDragNDrop listBox2;
       
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new DragDrop.Form1.ListBoxDragNDrop();
            this.listBox2 = new DragDrop.Form1.ListBoxDragNDrop();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.Items.AddRange(new object[] {
                                                    "round",
                                                    "square",
                                                    "thin"});

            this.listBox1.Location = new System.Drawing.Point(96, 16);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(200, 225);
            this.listBox1.TabIndex = 0;
            // 
            // listBox2
            // 
            this.listBox2.AllowDrop = true;
            this.listBox2.Items.AddRange(new object[] {
 
                                                                        "red",
                                                                        "blue",
                                                                        "green"});

            this.listBox2.Location = new System.Drawing.Point(384, 16);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(200, 225);
            this.listBox2.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(672, 301);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                            this.listBox2,
                                                                                            this.listBox1});
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
        }
        #endregion
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }
        public class ListBoxDragNDrop : ListBox
        {
            private int lastMouseUpItemIndex = -1;
            private bool isDropSource = false;
            private int lastLine = -1;
            public ListBoxDragNDrop()
            {
                this.AllowDrop = true;
                this.SelectionMode = SelectionMode.One;
                DragDrop += new System.Windows.Forms.DragEventHandler(OnDragDrop);
                DragEnter += new System.Windows.Forms.DragEventHandler(OnDragEnter);
                DragLeave += new System.EventHandler(OnDragLeave);
                MouseDown += new System.Windows.Forms.MouseEventHandler(OnMouseDown);
                DragOver += new System.Windows.Forms.DragEventHandler(OnDragOver);
                SelectedIndexChanged += new System.EventHandler(OnSelectedIndexChanged);
            }
            private void DrawVisualQue(Color color, ref Rectangle rectangle)
            {
                Graphics g = Graphics.FromHwnd(this.Handle);
                g.DrawLine(new Pen(color), rectangle.X, lastLine, rectangle.Width, lastLine);
            }
            private void OnDragLeave(object sender, EventArgs e)
            {
                if (lastLine > -1)
                {
                    Rectangle rect = this.GetItemRectangle(0);
                    DrawVisualQue(Color.White, ref rect);
                }
            }
            /// <summary>
            /// Called when [drag over].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
            private void OnDragOver(object sender, DragEventArgs e)
            {
                if (e.Effect == DragDropEffects.Copy)
                {
                    Point point = this.PointToClient(new Point(e.X, e.Y));
                    int index = this.IndexFromPoint(point);
                    bool belowLastItem = false;
                    if (index < 0 || index >= this.Items.Count)
                    {
                        index = this.Items.Count - 1;
                        belowLastItem = true;
                    }
                    Rectangle rect = this.GetItemRectangle(index);
                    if (lastLine > -1)
                        DrawVisualQue(Color.White, ref rect);
                    lastLine = rect.Y + (belowLastItem ? rect.Height : 0);
                    DrawVisualQue(Color.Red, ref rect);
                }
            }
            /// <summary>
            /// Called when [drag drop].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
            private void OnDragDrop(object sender, DragEventArgs e)
            {
                if (e.Effect == DragDropEffects.Copy)
                {
                    Point point = this.PointToClient(new Point(e.X, e.Y));
                    int index = this.IndexFromPoint(point);
                    if (index > -1 && index < this.Items.Count)
                        Items.Insert(index, e.Data.GetData(DataFormats.Text));
                    else
                        index = Items.Add(e.Data.GetData(DataFormats.Text));
                    this.SelectedIndex = index;
                }
            }
            /// <summary>
            /// Called when [drag enter].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
            private void OnDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
            {
                if (e.Data.GetDataPresent(DataFormats.Text) && !isDropSource)
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
                lastLine = -1;
            }
            /// <summary>
            /// Called when [mouse down].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
            private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                if (MouseButtons == MouseButtons.Left
                    && SelectedIndex == lastMouseUpItemIndex
                    && SelectedItem != null)
                {
                    isDropSource = true;
                    DoDragDrop(SelectedItem, DragDropEffects.Copy);
                    isDropSource = false;
                    lastMouseUpItemIndex = this.SelectedIndex;
                }
            }
            /// <summary>
            /// Called when [selected index changed].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void OnSelectedIndexChanged(object sender, System.EventArgs e)
            {
                lastMouseUpItemIndex = this.SelectedIndex;
            }
        }
    }
}

