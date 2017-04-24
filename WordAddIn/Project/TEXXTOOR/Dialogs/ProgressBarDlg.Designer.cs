namespace TEXXTOOR.Dialogs {
	partial class ProgressBarDlg {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.lblCurrent = new System.Windows.Forms.Label();
			this.lblMax = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 45);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(448, 29);
			this.progressBar1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(448, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Some background process is running. This window closes when done.";
			// 
			// lblCurrent
			// 
			this.lblCurrent.AutoSize = true;
			this.lblCurrent.BackColor = System.Drawing.Color.Transparent;
			this.lblCurrent.Location = new System.Drawing.Point(13, 77);
			this.lblCurrent.Name = "lblCurrent";
			this.lblCurrent.Size = new System.Drawing.Size(0, 16);
			this.lblCurrent.TabIndex = 2;
			// 
			// lblMax
			// 
			this.lblMax.AutoSize = true;
			this.lblMax.BackColor = System.Drawing.Color.Transparent;
			this.lblMax.Location = new System.Drawing.Point(413, 77);
			this.lblMax.Name = "lblMax";
			this.lblMax.Size = new System.Drawing.Size(0, 16);
			this.lblMax.TabIndex = 3;
			this.lblMax.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// ProgressBarDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(474, 97);
			this.ControlBox = false;
			this.Controls.Add(this.lblMax);
			this.Controls.Add(this.lblCurrent);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.progressBar1);
			this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressBarDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ProgressBar";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProgressBarDlg_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblCurrent;
		private System.Windows.Forms.Label lblMax;
	}
}