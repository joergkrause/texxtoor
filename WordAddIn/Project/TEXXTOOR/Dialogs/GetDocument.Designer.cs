namespace TEXXTOOR.Dialogs {
  partial class GetDocument {
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
			this.btnClose = new System.Windows.Forms.Button();
			this.btnSelect = new System.Windows.Forms.Button();
			this.lbProjects = new System.Windows.Forms.ListBox();
			this.lbDocuments = new System.Windows.Forms.ListBox();
			this.chkNewDocument = new System.Windows.Forms.CheckBox();
			this.lblProjects = new System.Windows.Forms.Label();
			this.lblDocuments = new System.Windows.Forms.Label();
			this.lblHint = new System.Windows.Forms.Label();
			this.labelDocId = new System.Windows.Forms.Label();
			this.labelDocTxt = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Location = new System.Drawing.Point(275, 272);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(75, 23);
			this.btnClose.TabIndex = 5;
			this.btnClose.Text = "&Cancel";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// btnSelect
			// 
			this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSelect.Enabled = false;
			this.btnSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSelect.Location = new System.Drawing.Point(356, 272);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = new System.Drawing.Size(109, 23);
			this.btnSelect.TabIndex = 4;
			this.btnSelect.Text = "&Select";
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
			// 
			// lbProjects
			// 
			this.lbProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbProjects.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lbProjects.Enabled = false;
			this.lbProjects.FormattingEnabled = true;
			this.lbProjects.Location = new System.Drawing.Point(12, 25);
			this.lbProjects.Name = "lbProjects";
			this.lbProjects.Size = new System.Drawing.Size(227, 223);
			this.lbProjects.TabIndex = 1;
			this.lbProjects.SelectedIndexChanged += new System.EventHandler(this.lbProjects_SelectedIndexChanged);
			// 
			// lbDocuments
			// 
			this.lbDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbDocuments.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lbDocuments.Enabled = false;
			this.lbDocuments.FormattingEnabled = true;
			this.lbDocuments.Location = new System.Drawing.Point(250, 25);
			this.lbDocuments.Name = "lbDocuments";
			this.lbDocuments.Size = new System.Drawing.Size(215, 171);
			this.lbDocuments.TabIndex = 2;
			this.lbDocuments.SelectedIndexChanged += new System.EventHandler(this.lbDocuments_SelectedIndexChanged);
			// 
			// chkNewDocument
			// 
			this.chkNewDocument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkNewDocument.AutoSize = true;
			this.chkNewDocument.Enabled = false;
			this.chkNewDocument.Location = new System.Drawing.Point(250, 235);
			this.chkNewDocument.Name = "chkNewDocument";
			this.chkNewDocument.Size = new System.Drawing.Size(179, 17);
			this.chkNewDocument.TabIndex = 3;
			this.chkNewDocument.Text = "Create &new Document in Project";
			this.chkNewDocument.UseVisualStyleBackColor = true;
			this.chkNewDocument.CheckedChanged += new System.EventHandler(this.chkNewDocument_CheckedChanged);
			// 
			// lblProjects
			// 
			this.lblProjects.AutoSize = true;
			this.lblProjects.Location = new System.Drawing.Point(13, 6);
			this.lblProjects.Name = "lblProjects";
			this.lblProjects.Size = new System.Drawing.Size(87, 13);
			this.lblProjects.TabIndex = 5;
			this.lblProjects.Text = "&Existing Projects:";
			// 
			// lblDocuments
			// 
			this.lblDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDocuments.AutoSize = true;
			this.lblDocuments.Location = new System.Drawing.Point(250, 5);
			this.lblDocuments.Name = "lblDocuments";
			this.lblDocuments.Size = new System.Drawing.Size(190, 13);
			this.lblDocuments.TabIndex = 6;
			this.lblDocuments.Text = "&Documents within the selected project:";
			// 
			// lblHint
			// 
			this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblHint.ForeColor = System.Drawing.Color.Red;
			this.lblHint.Location = new System.Drawing.Point(13, 259);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(233, 39);
			this.lblHint.TabIndex = 7;
			this.lblHint.Text = "Selected document will be overwritten through publish (One Way only).";
			// 
			// labelDocId
			// 
			this.labelDocId.AutoSize = true;
			this.labelDocId.Location = new System.Drawing.Point(374, 204);
			this.labelDocId.Name = "labelDocId";
			this.labelDocId.Size = new System.Drawing.Size(13, 13);
			this.labelDocId.TabIndex = 8;
			this.labelDocId.Text = "0";
			// 
			// labelDocTxt
			// 
			this.labelDocTxt.AutoSize = true;
			this.labelDocTxt.Location = new System.Drawing.Point(250, 203);
			this.labelDocTxt.Name = "labelDocTxt";
			this.labelDocTxt.Size = new System.Drawing.Size(117, 13);
			this.labelDocTxt.TabIndex = 9;
			this.labelDocTxt.Text = "Server ID of document:";
			// 
			// GetDocument
			// 
			this.AcceptButton = this.btnSelect;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(477, 309);
			this.Controls.Add(this.labelDocTxt);
			this.Controls.Add(this.labelDocId);
			this.Controls.Add(this.lblHint);
			this.Controls.Add(this.lblDocuments);
			this.Controls.Add(this.lblProjects);
			this.Controls.Add(this.chkNewDocument);
			this.Controls.Add(this.lbDocuments);
			this.Controls.Add(this.lbProjects);
			this.Controls.Add(this.btnSelect);
			this.Controls.Add(this.btnClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GetDocument";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Document for Publish";
			this.Load += new System.EventHandler(this.GetDocument_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Button btnSelect;
    private System.Windows.Forms.ListBox lbProjects;
    private System.Windows.Forms.ListBox lbDocuments;
    private System.Windows.Forms.CheckBox chkNewDocument;
    private System.Windows.Forms.Label lblProjects;
    private System.Windows.Forms.Label lblDocuments;
    private System.Windows.Forms.Label lblHint;
	private System.Windows.Forms.Label labelDocId;
	private System.Windows.Forms.Label labelDocTxt;
  }
}