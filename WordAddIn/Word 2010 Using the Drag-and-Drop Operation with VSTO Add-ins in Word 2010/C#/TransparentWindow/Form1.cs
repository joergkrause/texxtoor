using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TransparentWindow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Parent =(UserControl)dragdrop.UserControl1();
            //this.Opacity = 0d;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {

        }
    }
}
