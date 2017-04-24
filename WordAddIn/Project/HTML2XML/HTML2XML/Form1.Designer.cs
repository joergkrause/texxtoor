namespace HTML2XML
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.txtHtml = new System.Windows.Forms.TextBox();
      this.txtXML = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.btnConvert = new System.Windows.Forms.Button();
      this.chkInlineBase64 = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // txtHtml
      // 
      this.txtHtml.Location = new System.Drawing.Point(100, 27);
      this.txtHtml.Name = "txtHtml";
      this.txtHtml.Size = new System.Drawing.Size(204, 20);
      this.txtHtml.TabIndex = 0;
      // 
      // txtXML
      // 
      this.txtXML.Location = new System.Drawing.Point(100, 60);
      this.txtXML.Name = "txtXML";
      this.txtXML.Size = new System.Drawing.Size(204, 20);
      this.txtXML.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(47, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Html File";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 67);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(73, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "XML File Path";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(310, 25);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 4;
      this.button1.Text = "Browse...";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(310, 62);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 5;
      this.button2.Text = "Browse...";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // btnConvert
      // 
      this.btnConvert.Location = new System.Drawing.Point(100, 110);
      this.btnConvert.Name = "btnConvert";
      this.btnConvert.Size = new System.Drawing.Size(75, 23);
      this.btnConvert.TabIndex = 6;
      this.btnConvert.Text = "Convert";
      this.btnConvert.UseVisualStyleBackColor = true;
      this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
      // 
      // chkInlineBase64
      // 
      this.chkInlineBase64.AutoSize = true;
      this.chkInlineBase64.Location = new System.Drawing.Point(100, 87);
      this.chkInlineBase64.Name = "chkInlineBase64";
      this.chkInlineBase64.Size = new System.Drawing.Size(153, 17);
      this.chkInlineBase64.TabIndex = 7;
      this.chkInlineBase64.Text = "Add Images inline (base64)";
      this.chkInlineBase64.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(405, 262);
      this.Controls.Add(this.chkInlineBase64);
      this.Controls.Add(this.btnConvert);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtXML);
      this.Controls.Add(this.txtHtml);
      this.Name = "Form1";
      this.Text = "Form1";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox txtHtml;
        private System.Windows.Forms.TextBox txtXML;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.CheckBox chkInlineBase64;
    }
}

