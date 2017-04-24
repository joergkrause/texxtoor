namespace ImportFromWord {
  partial class Logon {
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
      this.btnLogon = new System.Windows.Forms.Button();
      this.btnClose = new System.Windows.Forms.Button();
      this.txtUsername = new System.Windows.Forms.TextBox();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnLogon
      // 
      this.btnLogon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLogon.Location = new System.Drawing.Point(444, 118);
      this.btnLogon.Name = "btnLogon";
      this.btnLogon.Size = new System.Drawing.Size(75, 23);
      this.btnLogon.TabIndex = 0;
      this.btnLogon.Text = "&Logon";
      this.btnLogon.UseVisualStyleBackColor = true;
      this.btnLogon.Click += new System.EventHandler(this.btnLogon_Click);
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.Location = new System.Drawing.Point(363, 118);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(75, 23);
      this.btnClose.TabIndex = 1;
      this.btnClose.Text = "&Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // txtUsername
      // 
      this.txtUsername.Location = new System.Drawing.Point(167, 30);
      this.txtUsername.Name = "txtUsername";
      this.txtUsername.Size = new System.Drawing.Size(352, 20);
      this.txtUsername.TabIndex = 2;
      // 
      // txtPassword
      // 
      this.txtPassword.Location = new System.Drawing.Point(167, 65);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.Size = new System.Drawing.Size(352, 20);
      this.txtPassword.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 33);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(75, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "Benutzername";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 68);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(52, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Kennwort";
      // 
      // Logon
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(531, 153);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtPassword);
      this.Controls.Add(this.txtUsername);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.btnLogon);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "Logon";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Logon to texxtoor";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnLogon;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
  }
}