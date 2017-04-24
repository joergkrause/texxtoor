namespace TEXXTOOR.TaskPanes {
	partial class IndexPreviewTaskPane {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.listBoxEntries = new System.Windows.Forms.ListBox();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.IndexTextBox = new System.Windows.Forms.TextBox();
			this.buttonChange = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.label1.Font = new System.Drawing.Font("Arial", 10F);
			this.label1.Location = new System.Drawing.Point(4, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(381, 43);
			this.label1.TabIndex = 0;
			this.label1.Text = "The currently added Index entries for this document. More index entries help read" +
    "ers to navigate in the text.";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 14F);
			this.label2.Location = new System.Drawing.Point(3, 51);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(121, 22);
			this.label2.TabIndex = 1;
			this.label2.Text = "Index Entries";
			// 
			// listBoxEntries
			// 
			this.listBoxEntries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxEntries.Font = new System.Drawing.Font("Arial", 9F);
			this.listBoxEntries.FormattingEnabled = true;
			this.listBoxEntries.ItemHeight = 15;
			this.listBoxEntries.Location = new System.Drawing.Point(7, 105);
			this.listBoxEntries.Name = "listBoxEntries";
			this.listBoxEntries.Size = new System.Drawing.Size(360, 304);
			this.listBoxEntries.TabIndex = 2;
			this.listBoxEntries.SelectedIndexChanged += new System.EventHandler(this.listBoxEntries_SelectedIndexChanged);
			// 
			// buttonRemove
			// 
			this.buttonRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonRemove.Font = new System.Drawing.Font("Arial", 8F);
			this.buttonRemove.Location = new System.Drawing.Point(307, 69);
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.Size = new System.Drawing.Size(60, 27);
			this.buttonRemove.TabIndex = 3;
			this.buttonRemove.Text = "&Remove";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
			// 
			// IndexTextBox
			// 
			this.IndexTextBox.Location = new System.Drawing.Point(7, 76);
			this.IndexTextBox.Name = "IndexTextBox";
			this.IndexTextBox.Size = new System.Drawing.Size(214, 20);
			this.IndexTextBox.TabIndex = 4;
			// 
			// buttonChange
			// 
			this.buttonChange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonChange.Font = new System.Drawing.Font("Arial", 8F);
			this.buttonChange.Location = new System.Drawing.Point(240, 69);
			this.buttonChange.Name = "buttonChange";
			this.buttonChange.Size = new System.Drawing.Size(60, 27);
			this.buttonChange.TabIndex = 5;
			this.buttonChange.Text = "&Change";
			this.buttonChange.UseVisualStyleBackColor = true;
			// 
			// IndexPreviewTaskPane
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.buttonChange);
			this.Controls.Add(this.IndexTextBox);
			this.Controls.Add(this.buttonRemove);
			this.Controls.Add(this.listBoxEntries);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "IndexPreviewTaskPane";
			this.Size = new System.Drawing.Size(388, 478);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox listBoxEntries;
		private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.TextBox IndexTextBox;
        private System.Windows.Forms.Button buttonChange;
	}
}
