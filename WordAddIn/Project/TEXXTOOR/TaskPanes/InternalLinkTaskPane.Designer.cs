namespace TEXXTOOR
{
    partial class InternalLinkTaskPane
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InternalLinkTaskPane));
			this.btnSave = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnToText = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.lbHyperLinks = new System.Windows.Forms.CheckedListBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lblNotAvailable = new System.Windows.Forms.Label();
			this.treeViewChapter = new System.Windows.Forms.TreeView();
			this.iconList = new System.Windows.Forms.ImageList(this.components);
			this.txtSearch = new System.Windows.Forms.RichTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtCaption = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSave.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSave.Location = new System.Drawing.Point(251, 23);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(122, 29);
			this.btnSave.TabIndex = 4;
			this.btnSave.Text = "&Insert";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoScroll = true;
			this.panel1.Controls.Add(this.btnToText);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.lbHyperLinks);
			this.panel1.Controls.Add(this.btnClear);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.label5);
			this.panel1.Controls.Add(this.lblNotAvailable);
			this.panel1.Controls.Add(this.treeViewChapter);
			this.panel1.Controls.Add(this.txtSearch);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.txtCaption);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Location = new System.Drawing.Point(5, 59);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(388, 666);
			this.panel1.TabIndex = 3;
			// 
			// btnToText
			// 
			this.btnToText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnToText.Enabled = false;
			this.btnToText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnToText.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnToText.Location = new System.Drawing.Point(246, 393);
			this.btnToText.Name = "btnToText";
			this.btnToText.Size = new System.Drawing.Size(122, 29);
			this.btnToText.TabIndex = 16;
			this.btnToText.Text = "Convert to Te&xt";
			this.btnToText.UseVisualStyleBackColor = true;
			this.btnToText.Click += new System.EventHandler(this.btnToText_Click);
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(8, 399);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(202, 16);
			this.label6.TabIndex = 15;
			this.label6.Text = "Active &Hyperlinks in Document:";
			// 
			// lbHyperLinks
			// 
			this.lbHyperLinks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbHyperLinks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lbHyperLinks.FormattingEnabled = true;
			this.lbHyperLinks.Location = new System.Drawing.Point(11, 432);
			this.lbHyperLinks.Name = "lbHyperLinks";
			this.lbHyperLinks.Size = new System.Drawing.Size(357, 212);
			this.lbHyperLinks.TabIndex = 14;
			this.lbHyperLinks.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbHyperLinks_ItemCheck);
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClear.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnClear.Location = new System.Drawing.Point(243, 98);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(122, 29);
			this.btnClear.TabIndex = 6;
			this.btnClear.Text = "Cl&ear Search";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 151);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(113, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "&Available targets";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(180, 104);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(18, 17);
			this.label5.TabIndex = 12;
			this.label5.Text = "(?)";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label5.UseCompatibleTextRendering = true;
			// 
			// lblNotAvailable
			// 
			this.lblNotAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblNotAvailable.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblNotAvailable.Location = new System.Drawing.Point(332, 6);
			this.lblNotAvailable.Name = "lblNotAvailable";
			this.lblNotAvailable.Size = new System.Drawing.Size(33, 22);
			this.lblNotAvailable.TabIndex = 11;
			this.lblNotAvailable.Text = "(n/a)";
			this.lblNotAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblNotAvailable.UseCompatibleTextRendering = true;
			// 
			// treeViewChapter
			// 
			this.treeViewChapter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeViewChapter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewChapter.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeViewChapter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.treeViewChapter.ImageIndex = 0;
			this.treeViewChapter.ImageList = this.iconList;
			this.treeViewChapter.Location = new System.Drawing.Point(10, 173);
			this.treeViewChapter.Name = "treeViewChapter";
			this.treeViewChapter.SelectedImageIndex = 0;
			this.treeViewChapter.Size = new System.Drawing.Size(358, 194);
			this.treeViewChapter.StateImageList = this.iconList;
			this.treeViewChapter.TabIndex = 10;
			this.treeViewChapter.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChapter_AfterSelect);
			// 
			// iconList
			// 
			this.iconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconList.ImageStream")));
			this.iconList.TransparentColor = System.Drawing.Color.Transparent;
			this.iconList.Images.SetKeyName(0, "text_16.png");
			this.iconList.Images.SetKeyName(1, "code_16.png");
			this.iconList.Images.SetKeyName(2, "photo_landscape_16.png");
			this.iconList.Images.SetKeyName(3, "table2_16.png");
			this.iconList.Images.SetKeyName(4, "icon_doc.png");
			// 
			// txtSearch
			// 
			this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSearch.Location = new System.Drawing.Point(11, 103);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(163, 24);
			this.txtSearch.TabIndex = 9;
			this.txtSearch.Text = "";
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(7, 81);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "&Search";
			// 
			// txtCaption
			// 
			this.txtCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtCaption.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtCaption.Location = new System.Drawing.Point(11, 34);
			this.txtCaption.Name = "txtCaption";
			this.txtCaption.Size = new System.Drawing.Size(354, 24);
			this.txtCaption.TabIndex = 7;
			this.txtCaption.Text = "";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(7, 11);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(156, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "&Caption (shown on link)";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(5, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(260, 40);
			this.label1.TabIndex = 5;
			this.label1.Text = "Add an element that hyperlinks to a place inside this text.";
			// 
			// InternalLinkTaskPane
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label1);
			this.Name = "InternalLinkTaskPane";
			this.Size = new System.Drawing.Size(404, 744);
			this.Load += new System.EventHandler(this.InternalLinkTaskPane_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblNotAvailable;
        private System.Windows.Forms.TreeView treeViewChapter;
        private System.Windows.Forms.RichTextBox txtSearch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox txtCaption;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ImageList iconList;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckedListBox lbHyperLinks;
		private System.Windows.Forms.Button btnToText;
    }
}
