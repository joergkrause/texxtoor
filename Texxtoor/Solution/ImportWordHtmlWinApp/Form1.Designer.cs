namespace ImportFromWord {
  partial class Form1 {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.dateiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.öffnenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.backupWiederherstellenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.bilderHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.verarbeitungToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.prüfenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.konvertierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.fehlerBehandelnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.statusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.plattformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.anmeldenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.hochladenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
      this.lokalExportierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.openHtmlFile = new System.Windows.Forms.OpenFileDialog();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.label4 = new System.Windows.Forms.Label();
      this.lblProtocol = new System.Windows.Forms.TextBox();
      this.openImages = new System.Windows.Forms.OpenFileDialog();
      this.saveZipFile = new System.Windows.Forms.SaveFileDialog();
      this.menuStrip1.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dateiToolStripMenuItem,
            this.verarbeitungToolStripMenuItem,
            this.plattformToolStripMenuItem,
            this.aboutToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(925, 24);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // dateiToolStripMenuItem
      // 
      this.dateiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.öffnenToolStripMenuItem,
            this.backupWiederherstellenToolStripMenuItem,
            this.bilderHinzufügenToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
      this.dateiToolStripMenuItem.Name = "dateiToolStripMenuItem";
      this.dateiToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
      this.dateiToolStripMenuItem.Text = "&Datei";
      // 
      // öffnenToolStripMenuItem
      // 
      this.öffnenToolStripMenuItem.Name = "öffnenToolStripMenuItem";
      this.öffnenToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.öffnenToolStripMenuItem.Text = "&Öffnen";
      this.öffnenToolStripMenuItem.Click += new System.EventHandler(this.öffnenToolStripMenuItem_Click);
      // 
      // backupWiederherstellenToolStripMenuItem
      // 
      this.backupWiederherstellenToolStripMenuItem.Enabled = false;
      this.backupWiederherstellenToolStripMenuItem.Name = "backupWiederherstellenToolStripMenuItem";
      this.backupWiederherstellenToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.backupWiederherstellenToolStripMenuItem.Text = "&Backup wiederherstellen";
      // 
      // bilderHinzufügenToolStripMenuItem
      // 
      this.bilderHinzufügenToolStripMenuItem.Name = "bilderHinzufügenToolStripMenuItem";
      this.bilderHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.bilderHinzufügenToolStripMenuItem.Text = "&Bilder hinzufügen";
      this.bilderHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.bilderHinzufügenToolStripMenuItem_Click);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(199, 6);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.exitToolStripMenuItem.Text = "E&xit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // verarbeitungToolStripMenuItem
      // 
      this.verarbeitungToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.prüfenToolStripMenuItem,
            this.konvertierenToolStripMenuItem,
            this.fehlerBehandelnToolStripMenuItem,
            this.toolStripMenuItem1,
            this.statusToolStripMenuItem});
      this.verarbeitungToolStripMenuItem.Name = "verarbeitungToolStripMenuItem";
      this.verarbeitungToolStripMenuItem.Size = new System.Drawing.Size(87, 20);
      this.verarbeitungToolStripMenuItem.Text = "&Verarbeitung";
      // 
      // prüfenToolStripMenuItem
      // 
      this.prüfenToolStripMenuItem.Name = "prüfenToolStripMenuItem";
      this.prüfenToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
      this.prüfenToolStripMenuItem.Text = "1. &Prüfen";
      this.prüfenToolStripMenuItem.Click += new System.EventHandler(this.prüfenToolStripMenuItem_Click);
      // 
      // konvertierenToolStripMenuItem
      // 
      this.konvertierenToolStripMenuItem.Name = "konvertierenToolStripMenuItem";
      this.konvertierenToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
      this.konvertierenToolStripMenuItem.Text = "2. &Konvertieren";
      this.konvertierenToolStripMenuItem.Click += new System.EventHandler(this.konvertierenToolStripMenuItem_Click);
      // 
      // fehlerBehandelnToolStripMenuItem
      // 
      this.fehlerBehandelnToolStripMenuItem.Name = "fehlerBehandelnToolStripMenuItem";
      this.fehlerBehandelnToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
      this.fehlerBehandelnToolStripMenuItem.Text = "3. &XML erzeugen";
      this.fehlerBehandelnToolStripMenuItem.Click += new System.EventHandler(this.fehlerBehandelnToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(158, 6);
      // 
      // statusToolStripMenuItem
      // 
      this.statusToolStripMenuItem.Name = "statusToolStripMenuItem";
      this.statusToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
      this.statusToolStripMenuItem.Text = "&Status";
      this.statusToolStripMenuItem.Click += new System.EventHandler(this.statusToolStripMenuItem_Click);
      // 
      // plattformToolStripMenuItem
      // 
      this.plattformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.anmeldenToolStripMenuItem,
            this.hochladenToolStripMenuItem,
            this.toolStripMenuItem3,
            this.lokalExportierenToolStripMenuItem});
      this.plattformToolStripMenuItem.Name = "plattformToolStripMenuItem";
      this.plattformToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
      this.plattformToolStripMenuItem.Text = "&Plattform";
      // 
      // anmeldenToolStripMenuItem
      // 
      this.anmeldenToolStripMenuItem.Name = "anmeldenToolStripMenuItem";
      this.anmeldenToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.anmeldenToolStripMenuItem.Text = "&Anmelden";
      this.anmeldenToolStripMenuItem.Click += new System.EventHandler(this.anmeldenToolStripMenuItem_Click);
      // 
      // hochladenToolStripMenuItem
      // 
      this.hochladenToolStripMenuItem.Name = "hochladenToolStripMenuItem";
      this.hochladenToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.hochladenToolStripMenuItem.Text = "&Hochladen";
      this.hochladenToolStripMenuItem.Click += new System.EventHandler(this.hochladenToolStripMenuItem_Click);
      // 
      // toolStripMenuItem3
      // 
      this.toolStripMenuItem3.Name = "toolStripMenuItem3";
      this.toolStripMenuItem3.Size = new System.Drawing.Size(161, 6);
      // 
      // lokalExportierenToolStripMenuItem
      // 
      this.lokalExportierenToolStripMenuItem.Name = "lokalExportierenToolStripMenuItem";
      this.lokalExportierenToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.lokalExportierenToolStripMenuItem.Text = "&Lokal exportieren";
      this.lokalExportierenToolStripMenuItem.Click += new System.EventHandler(this.lokalExportierenToolStripMenuItem_Click);
      // 
      // aboutToolStripMenuItem
      // 
      this.aboutToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
      this.aboutToolStripMenuItem.Text = "&About";
      this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(13, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(146, 20);
      this.label1.TabIndex = 1;
      this.label1.Text = "Vorgehensweise:";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(13, 57);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(900, 90);
      this.label2.TabIndex = 2;
      this.label2.Text = resources.GetString("label2.Text");
      // 
      // openHtmlFile
      // 
      this.openHtmlFile.Filter = "HTML|*.html;*.htm|Alle Dateien|*.*";
      this.openHtmlFile.RestoreDirectory = true;
      this.openHtmlFile.Title = "Word HTML";
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
      this.statusStrip1.Location = new System.Drawing.Point(0, 530);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(925, 22);
      this.statusStrip1.TabIndex = 3;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(74, 17);
      this.toolStripStatusLabel1.Text = "<Dateipfad>";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(13, 147);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(67, 20);
      this.label4.TabIndex = 5;
      this.label4.Text = "Status:";
      // 
      // lblProtocol
      // 
      this.lblProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblProtocol.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.lblProtocol.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblProtocol.Location = new System.Drawing.Point(17, 171);
      this.lblProtocol.Multiline = true;
      this.lblProtocol.Name = "lblProtocol";
      this.lblProtocol.ReadOnly = true;
      this.lblProtocol.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.lblProtocol.Size = new System.Drawing.Size(896, 345);
      this.lblProtocol.TabIndex = 6;
      // 
      // openImages
      // 
      this.openImages.Filter = "Bilder|*.png;*.jpg;*.gif|Alle Dateien|*.*";
      this.openImages.Multiselect = true;
      this.openImages.RestoreDirectory = true;
      this.openImages.Title = "Assoziierte Bilder";
      // 
      // saveZipFile
      // 
      this.saveZipFile.DefaultExt = "*.zip";
      this.saveZipFile.Filter = "ZIP-Archive|*.zip|Alle Dateien|*.*";
      this.saveZipFile.RestoreDirectory = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(925, 552);
      this.Controls.Add(this.lblProtocol);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.menuStrip1);
      this.MainMenuStrip = this.menuStrip1;
      this.MinimumSize = new System.Drawing.Size(941, 590);
      this.Name = "Form1";
      this.Text = "texxtoor Word HTML Import";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem dateiToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem öffnenToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem bilderHinzufügenToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem verarbeitungToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem konvertierenToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem fehlerBehandelnToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem statusToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem plattformToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem anmeldenToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem hochladenToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
    private System.Windows.Forms.ToolStripMenuItem lokalExportierenToolStripMenuItem;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ToolStripMenuItem prüfenToolStripMenuItem;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.OpenFileDialog openHtmlFile;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox lblProtocol;
    private System.Windows.Forms.OpenFileDialog openImages;
    private System.Windows.Forms.SaveFileDialog saveZipFile;
    private System.Windows.Forms.ToolStripMenuItem backupWiederherstellenToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
  }
}

