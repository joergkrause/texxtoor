namespace ImportFromWord {
  partial class UploadDocument {
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
      this.lblProjects = new System.Windows.Forms.Label();
      this.lbProjects = new System.Windows.Forms.ListBox();
      this.btnSelect = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnRefresh = new System.Windows.Forms.Button();
      this.lblProject = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblProjects
      // 
      this.lblProjects.AutoSize = true;
      this.lblProjects.Location = new System.Drawing.Point(13, 13);
      this.lblProjects.Name = "lblProjects";
      this.lblProjects.Size = new System.Drawing.Size(49, 13);
      this.lblProjects.TabIndex = 0;
      this.lblProjects.Text = "Projekte:";
      // 
      // lbProjects
      // 
      this.lbProjects.FormattingEnabled = true;
      this.lbProjects.Location = new System.Drawing.Point(71, 12);
      this.lbProjects.Name = "lbProjects";
      this.lbProjects.Size = new System.Drawing.Size(423, 199);
      this.lbProjects.TabIndex = 1;
      this.lbProjects.SelectedIndexChanged += new System.EventHandler(this.lbProjects_SelectedIndexChanged);
      // 
      // btnSelect
      // 
      this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSelect.Location = new System.Drawing.Point(419, 259);
      this.btnSelect.Name = "btnSelect";
      this.btnSelect.Size = new System.Drawing.Size(75, 23);
      this.btnSelect.TabIndex = 2;
      this.btnSelect.Text = "&Auswahl";
      this.btnSelect.UseVisualStyleBackColor = true;
      this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(338, 259);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Abbre&chen";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnRefresh
      // 
      this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRefresh.Location = new System.Drawing.Point(71, 259);
      this.btnRefresh.Name = "btnRefresh";
      this.btnRefresh.Size = new System.Drawing.Size(75, 23);
      this.btnRefresh.TabIndex = 4;
      this.btnRefresh.Text = "A&ktualisieren";
      this.btnRefresh.UseVisualStyleBackColor = true;
      this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
      // 
      // lblProject
      // 
      this.lblProject.AutoSize = true;
      this.lblProject.Location = new System.Drawing.Point(71, 218);
      this.lblProject.Name = "lblProject";
      this.lblProject.Size = new System.Drawing.Size(76, 13);
      this.lblProject.TabIndex = 5;
      this.lblProject.Text = "<Kein Projekt>";
      // 
      // UploadDocument
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(506, 294);
      this.Controls.Add(this.lblProject);
      this.Controls.Add(this.btnRefresh);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnSelect);
      this.Controls.Add(this.lbProjects);
      this.Controls.Add(this.lblProjects);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "UploadDocument";
      this.Text = "Upload a Document";
      this.Load += new System.EventHandler(this.UploadDocument_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblProjects;
    private System.Windows.Forms.ListBox lbProjects;
    private System.Windows.Forms.Button btnSelect;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Label lblProject;
  }
}