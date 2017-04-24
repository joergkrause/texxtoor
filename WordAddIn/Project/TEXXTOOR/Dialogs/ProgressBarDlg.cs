using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TEXXTOOR.Dialogs {
	public partial class ProgressBarDlg : Form {

		Timer timer = new Timer();

		public ProgressBarDlg() {
			InitializeComponent();
			timer.Tick += new EventHandler(timer_Tick);
		}

		public string Title { get; set; }

		public int Progress {
			set {
				progressBar1.Value = Math.Min(value, progressBar1.Maximum);
				lblCurrent.Text = progressBar1.Value.ToString();
				progressBar1.Invalidate();
			}
		}

		private int value = 0;

		public void AutoProgressShow() {			
			timer.Interval = 200;
			timer.Start();
			Show();
		}

		public void AutoProgressHide() {
			timer.Stop();
			if (InvokeRequired) {
				Invoke(new MethodInvoker(Hide));
			}
			else {
				Hide();	
			}			
		}

		void timer_Tick(object sender, EventArgs e) {
			Progress = value;
			value++;
			if (value == progressBar1.Maximum) value = 1;
		}

		public void SetMax(int max) {
			progressBar1.Maximum = max;
			lblMax.Text = max.ToString();
		}

		private void ProgressBarDlg_FormClosing(object sender, FormClosingEventArgs e) {
			timer.Stop();
		}

	}
}
