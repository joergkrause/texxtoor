namespace TEXXTOOR
{
    partial class CommentsTaskPane
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
			this.CommentsTabControl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.txtPrivateCommentsList = new System.Windows.Forms.RichTextBox();
			this.btnAddPrivateComment = new System.Windows.Forms.Button();
			this.cbxPrivateComments = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtPrivateCommentBody = new System.Windows.Forms.RichTextBox();
			this.txtPrivateCommentSubject = new System.Windows.Forms.RichTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.txtTeamCommentsList = new System.Windows.Forms.RichTextBox();
			this.btnAddTeamComments = new System.Windows.Forms.Button();
			this.cbxTeamComments = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtTeamCommentBody = new System.Windows.Forms.RichTextBox();
			this.txtTeamCommentSubject = new System.Windows.Forms.RichTextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.txtReaderCommentsList = new System.Windows.Forms.RichTextBox();
			this.btnAddReadersComment = new System.Windows.Forms.Button();
			this.cbxReaderComments = new System.Windows.Forms.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.txtReaderCommentBody = new System.Windows.Forms.RichTextBox();
			this.txtReaderCommentSubject = new System.Windows.Forms.RichTextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.CommentsTabControl.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// CommentsTabControl
			// 
			this.CommentsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommentsTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.CommentsTabControl.Controls.Add(this.tabPage1);
			this.CommentsTabControl.Controls.Add(this.tabPage2);
			this.CommentsTabControl.Controls.Add(this.tabPage3);
			this.CommentsTabControl.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CommentsTabControl.Location = new System.Drawing.Point(7, 36);
			this.CommentsTabControl.Name = "CommentsTabControl";
			this.CommentsTabControl.SelectedIndex = 0;
			this.CommentsTabControl.Size = new System.Drawing.Size(445, 501);
			this.CommentsTabControl.TabIndex = 10;
			// 
			// tabPage1
			// 
			this.tabPage1.AutoScroll = true;
			this.tabPage1.Controls.Add(this.txtPrivateCommentsList);
			this.tabPage1.Controls.Add(this.btnAddPrivateComment);
			this.tabPage1.Controls.Add(this.cbxPrivateComments);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.txtPrivateCommentBody);
			this.tabPage1.Controls.Add(this.txtPrivateCommentSubject);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Location = new System.Drawing.Point(4, 28);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(437, 469);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Private";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// txtPrivateCommentsList
			// 
			this.txtPrivateCommentsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPrivateCommentsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPrivateCommentsList.Location = new System.Drawing.Point(8, 265);
			this.txtPrivateCommentsList.Name = "txtPrivateCommentsList";
			this.txtPrivateCommentsList.ReadOnly = true;
			this.txtPrivateCommentsList.Size = new System.Drawing.Size(418, 188);
			this.txtPrivateCommentsList.TabIndex = 7;
			this.txtPrivateCommentsList.Text = "";
			// 
			// btnAddPrivateComment
			// 
			this.btnAddPrivateComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddPrivateComment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddPrivateComment.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnAddPrivateComment.Location = new System.Drawing.Point(286, 230);
			this.btnAddPrivateComment.Name = "btnAddPrivateComment";
			this.btnAddPrivateComment.Size = new System.Drawing.Size(140, 24);
			this.btnAddPrivateComment.TabIndex = 6;
			this.btnAddPrivateComment.Text = "Add Comment";
			this.btnAddPrivateComment.UseVisualStyleBackColor = true;
			this.btnAddPrivateComment.Click += new System.EventHandler(this.btnAddComment_Click);
			// 
			// cbxPrivateComments
			// 
			this.cbxPrivateComments.AutoSize = true;
			this.cbxPrivateComments.Location = new System.Drawing.Point(114, 230);
			this.cbxPrivateComments.Name = "cbxPrivateComments";
			this.cbxPrivateComments.Size = new System.Drawing.Size(15, 14);
			this.cbxPrivateComments.TabIndex = 5;
			this.cbxPrivateComments.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(10, 230);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(98, 16);
			this.label4.TabIndex = 4;
			this.label4.Text = "Close &Thread:";
			// 
			// txtPrivateCommentBody
			// 
			this.txtPrivateCommentBody.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPrivateCommentBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPrivateCommentBody.Location = new System.Drawing.Point(8, 111);
			this.txtPrivateCommentBody.Name = "txtPrivateCommentBody";
			this.txtPrivateCommentBody.Size = new System.Drawing.Size(416, 112);
			this.txtPrivateCommentBody.TabIndex = 3;
			this.txtPrivateCommentBody.Text = "";
			// 
			// txtPrivateCommentSubject
			// 
			this.txtPrivateCommentSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPrivateCommentSubject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPrivateCommentSubject.Location = new System.Drawing.Point(8, 73);
			this.txtPrivateCommentSubject.Name = "txtPrivateCommentSubject";
			this.txtPrivateCommentSubject.Size = new System.Drawing.Size(418, 24);
			this.txtPrivateCommentSubject.TabIndex = 2;
			this.txtPrivateCommentSubject.Text = "";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Arial", 10F);
			this.label3.Location = new System.Drawing.Point(8, 44);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(227, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = "Add a new comment here. Subject:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(4, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(166, 22);
			this.label2.TabIndex = 0;
			this.label2.Text = "Private Comments";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.txtTeamCommentsList);
			this.tabPage2.Controls.Add(this.btnAddTeamComments);
			this.tabPage2.Controls.Add(this.cbxTeamComments);
			this.tabPage2.Controls.Add(this.label5);
			this.tabPage2.Controls.Add(this.txtTeamCommentBody);
			this.tabPage2.Controls.Add(this.txtTeamCommentSubject);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Controls.Add(this.label7);
			this.tabPage2.Font = new System.Drawing.Font("Arial", 8.25F);
			this.tabPage2.Location = new System.Drawing.Point(4, 28);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(437, 469);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Team";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// txtTeamCommentsList
			// 
			this.txtTeamCommentsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTeamCommentsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtTeamCommentsList.Location = new System.Drawing.Point(8, 265);
			this.txtTeamCommentsList.Name = "txtTeamCommentsList";
			this.txtTeamCommentsList.ReadOnly = true;
			this.txtTeamCommentsList.Size = new System.Drawing.Size(418, 188);
			this.txtTeamCommentsList.TabIndex = 15;
			this.txtTeamCommentsList.Text = "";
			// 
			// btnAddTeamComments
			// 
			this.btnAddTeamComments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddTeamComments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddTeamComments.Font = new System.Drawing.Font("Arial", 10F);
			this.btnAddTeamComments.Location = new System.Drawing.Point(286, 230);
			this.btnAddTeamComments.Name = "btnAddTeamComments";
			this.btnAddTeamComments.Size = new System.Drawing.Size(140, 24);
			this.btnAddTeamComments.TabIndex = 14;
			this.btnAddTeamComments.Text = "Add Comment";
			this.btnAddTeamComments.UseVisualStyleBackColor = true;
			this.btnAddTeamComments.Click += new System.EventHandler(this.btnAddComment_Click);
			// 
			// cbxTeamComments
			// 
			this.cbxTeamComments.AutoSize = true;
			this.cbxTeamComments.Location = new System.Drawing.Point(114, 230);
			this.cbxTeamComments.Name = "cbxTeamComments";
			this.cbxTeamComments.Size = new System.Drawing.Size(15, 14);
			this.cbxTeamComments.TabIndex = 13;
			this.cbxTeamComments.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Arial", 10F);
			this.label5.Location = new System.Drawing.Point(10, 230);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(98, 16);
			this.label5.TabIndex = 12;
			this.label5.Text = "Close &Thread:";
			// 
			// txtTeamCommentBody
			// 
			this.txtTeamCommentBody.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTeamCommentBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtTeamCommentBody.Location = new System.Drawing.Point(8, 111);
			this.txtTeamCommentBody.Name = "txtTeamCommentBody";
			this.txtTeamCommentBody.Size = new System.Drawing.Size(418, 112);
			this.txtTeamCommentBody.TabIndex = 11;
			this.txtTeamCommentBody.Text = "";
			// 
			// txtTeamCommentSubject
			// 
			this.txtTeamCommentSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTeamCommentSubject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtTeamCommentSubject.Location = new System.Drawing.Point(8, 73);
			this.txtTeamCommentSubject.Name = "txtTeamCommentSubject";
			this.txtTeamCommentSubject.Size = new System.Drawing.Size(418, 24);
			this.txtTeamCommentSubject.TabIndex = 10;
			this.txtTeamCommentSubject.Text = "";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Arial", 10F);
			this.label6.Location = new System.Drawing.Point(8, 44);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(244, 16);
			this.label6.TabIndex = 9;
			this.label6.Text = "Write your contributors here. Subject:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Arial", 14F);
			this.label7.Location = new System.Drawing.Point(4, 9);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(244, 22);
			this.label7.TabIndex = 8;
			this.label7.Text = "Team Member\'s Comments";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.txtReaderCommentsList);
			this.tabPage3.Controls.Add(this.btnAddReadersComment);
			this.tabPage3.Controls.Add(this.cbxReaderComments);
			this.tabPage3.Controls.Add(this.label8);
			this.tabPage3.Controls.Add(this.txtReaderCommentBody);
			this.tabPage3.Controls.Add(this.txtReaderCommentSubject);
			this.tabPage3.Controls.Add(this.label9);
			this.tabPage3.Controls.Add(this.label10);
			this.tabPage3.Location = new System.Drawing.Point(4, 28);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(437, 469);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Reader";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// txtReaderCommentsList
			// 
			this.txtReaderCommentsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtReaderCommentsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtReaderCommentsList.Location = new System.Drawing.Point(8, 265);
			this.txtReaderCommentsList.Name = "txtReaderCommentsList";
			this.txtReaderCommentsList.ReadOnly = true;
			this.txtReaderCommentsList.Size = new System.Drawing.Size(418, 188);
			this.txtReaderCommentsList.TabIndex = 15;
			this.txtReaderCommentsList.Text = "";
			// 
			// btnAddReadersComment
			// 
			this.btnAddReadersComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddReadersComment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddReadersComment.Font = new System.Drawing.Font("Arial", 10F);
			this.btnAddReadersComment.Location = new System.Drawing.Point(286, 230);
			this.btnAddReadersComment.Name = "btnAddReadersComment";
			this.btnAddReadersComment.Size = new System.Drawing.Size(140, 24);
			this.btnAddReadersComment.TabIndex = 14;
			this.btnAddReadersComment.Text = "&Add Comment";
			this.btnAddReadersComment.UseVisualStyleBackColor = true;
			this.btnAddReadersComment.Click += new System.EventHandler(this.btnAddComment_Click);
			// 
			// cbxReaderComments
			// 
			this.cbxReaderComments.AccessibleDescription = "";
			this.cbxReaderComments.AutoSize = true;
			this.cbxReaderComments.Location = new System.Drawing.Point(114, 230);
			this.cbxReaderComments.Name = "cbxReaderComments";
			this.cbxReaderComments.Size = new System.Drawing.Size(15, 14);
			this.cbxReaderComments.TabIndex = 13;
			this.cbxReaderComments.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Arial", 10F);
			this.label8.Location = new System.Drawing.Point(10, 230);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(98, 16);
			this.label8.TabIndex = 12;
			this.label8.Text = "Close &Thread:";
			// 
			// txtReaderCommentBody
			// 
			this.txtReaderCommentBody.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtReaderCommentBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtReaderCommentBody.Location = new System.Drawing.Point(8, 111);
			this.txtReaderCommentBody.Name = "txtReaderCommentBody";
			this.txtReaderCommentBody.Size = new System.Drawing.Size(418, 112);
			this.txtReaderCommentBody.TabIndex = 11;
			this.txtReaderCommentBody.Text = "";
			// 
			// txtReaderCommentSubject
			// 
			this.txtReaderCommentSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtReaderCommentSubject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtReaderCommentSubject.Location = new System.Drawing.Point(8, 73);
			this.txtReaderCommentSubject.Name = "txtReaderCommentSubject";
			this.txtReaderCommentSubject.Size = new System.Drawing.Size(418, 24);
			this.txtReaderCommentSubject.TabIndex = 10;
			this.txtReaderCommentSubject.Text = "";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Arial", 10F);
			this.label9.Location = new System.Drawing.Point(8, 44);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(231, 16);
			this.label9.TabIndex = 9;
			this.label9.Text = "Answer your readers here. Subject:";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("Arial", 14F);
			this.label10.Location = new System.Drawing.Point(4, 9);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(195, 22);
			this.label10.TabIndex = 8;
			this.label10.Text = "Readers\'s Comments";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(446, 25);
			this.label1.TabIndex = 9;
			this.label1.Text = "Comments provide a way to interact with your peers and readers.\r\n";
			// 
			// CommentsTaskPane
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.CommentsTabControl);
			this.Controls.Add(this.label1);
			this.Name = "CommentsTaskPane";
			this.Size = new System.Drawing.Size(455, 559);
			this.CommentsTabControl.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			this.tabPage3.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl CommentsTabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox txtPrivateCommentsList;
        private System.Windows.Forms.Button btnAddPrivateComment;
        private System.Windows.Forms.CheckBox cbxPrivateComments;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox txtPrivateCommentBody;
        private System.Windows.Forms.RichTextBox txtPrivateCommentSubject;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox txtTeamCommentsList;
        private System.Windows.Forms.Button btnAddTeamComments;
        private System.Windows.Forms.CheckBox cbxTeamComments;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RichTextBox txtTeamCommentBody;
        private System.Windows.Forms.RichTextBox txtTeamCommentSubject;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RichTextBox txtReaderCommentsList;
        private System.Windows.Forms.Button btnAddReadersComment;
        private System.Windows.Forms.CheckBox cbxReaderComments;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RichTextBox txtReaderCommentBody;
        private System.Windows.Forms.RichTextBox txtReaderCommentSubject;
        private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
    }
}
