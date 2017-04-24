namespace TEXXTOOR.Dialogs {
	partial class LogOn {
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
			this.components = new System.ComponentModel.Container();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnLogon = new System.Windows.Forms.Button();
			this.txtName = new System.Windows.Forms.TextBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.lblUsername = new System.Windows.Forms.Label();
			this.linkWebsite = new System.Windows.Forms.LinkLabel();
			this.progressBarLogon = new System.Windows.Forms.ProgressBar();
			this.timerProgress = new System.Windows.Forms.Timer(this.components);
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCancel.Location = new System.Drawing.Point(179, 156);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnLogon
			// 
			this.btnLogon.Enabled = false;
			this.btnLogon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLogon.Location = new System.Drawing.Point(260, 156);
			this.btnLogon.Name = "btnLogon";
			this.btnLogon.Size = new System.Drawing.Size(75, 23);
			this.btnLogon.TabIndex = 3;
			this.btnLogon.Text = "&Log On";
			this.btnLogon.UseVisualStyleBackColor = true;
			this.btnLogon.Click += new System.EventHandler(this.btnLogon_Click);
			// 
			// txtName
			// 
			this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtName.Location = new System.Drawing.Point(81, 24);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(225, 20);
			this.txtName.TabIndex = 1;
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			// 
			// txtPassword
			// 
			this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPassword.Location = new System.Drawing.Point(81, 62);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(225, 20);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lblPassword);
			this.groupBox1.Controls.Add(this.lblUsername);
			this.groupBox1.Controls.Add(this.txtName);
			this.groupBox1.Controls.Add(this.txtPassword);
			this.groupBox1.Location = new System.Drawing.Point(13, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(322, 100);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Provide your Credentials";
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point(8, 64);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(53, 13);
			this.lblPassword.TabIndex = 5;
			this.lblPassword.Text = "&Password";
			// 
			// lblUsername
			// 
			this.lblUsername.AutoSize = true;
			this.lblUsername.Location = new System.Drawing.Point(7, 30);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(60, 13);
			this.lblUsername.TabIndex = 4;
			this.lblUsername.Text = "&User Name";
			// 
			// linkWebsite
			// 
			this.linkWebsite.AutoSize = true;
			this.linkWebsite.Location = new System.Drawing.Point(17, 161);
			this.linkWebsite.Name = "linkWebsite";
			this.linkWebsite.Size = new System.Drawing.Size(156, 13);
			this.linkWebsite.TabIndex = 5;
			this.linkWebsite.TabStop = true;
			this.linkWebsite.Text = "Click here to create an account";
			this.linkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkWebsite_LinkClicked);
			// 
			// progressBarLogon
			// 
			this.progressBarLogon.Location = new System.Drawing.Point(20, 120);
			this.progressBarLogon.Maximum = 1000;
			this.progressBarLogon.Name = "progressBarLogon";
			this.progressBarLogon.Size = new System.Drawing.Size(315, 23);
			this.progressBarLogon.TabIndex = 6;
			this.progressBarLogon.Visible = false;
			// 
			// LogOn
			// 
			this.AcceptButton = this.btnLogon;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(347, 191);
			this.Controls.Add(this.progressBarLogon);
			this.Controls.Add(this.linkWebsite);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnLogon);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LogOn";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Log On";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnLogon;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.LinkLabel linkWebsite;
		private System.Windows.Forms.ProgressBar progressBarLogon;
		private System.Windows.Forms.Timer timerProgress;
	}
}