using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using ImportFromWord.ServiceClient;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml;
using HAP = HtmlAgilityPack;

namespace ImportFromWord {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }


    private string imgFolder;
    private string htmlFileName;
    private string[] imgFileArrayToAdd;
    private Dictionary<string, string> imgFileArrayToChange;
    private string genericImgPathPart;

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      this.Close();
    }

    private void öffnenToolStripMenuItem_Click(object sender, EventArgs e) {
      toolStripStatusLabel1.Text = "";
      lblProtocol.Clear();
      openHtmlFile.CheckFileExists = true;
      if (openHtmlFile.ShowDialog() == DialogResult.OK) {
        // get path
        var filePath = openHtmlFile.FileName;
        toolStripStatusLabel1.Text = filePath;
        AddTextToProtocol("Datei {0} geladen", Path.GetFileName(filePath));
        // get file
        if (File.Exists(filePath)) {
          // exit if we want to restore backup
          if (CheckBackup(filePath) == DialogResult.Cancel) {
            var backupFileName = GetBackupFileName(filePath);
            if (File.Exists(filePath)) {
              File.Delete(filePath);
              AddTextToProtocol("HTML gelöscht");
              File.Copy(backupFileName, filePath);
              AddTextToProtocol("HTML aus Backup wiederhergestellt");
              File.Delete(backupFileName);
              AddTextToProtocol("Backup entfernt");
            }
            if (MessageBox.Show("HTML aus Backup wiederhergestellt. Weiter mit Konvertierung?", "Frage",
              MessageBoxButtons.YesNo) == DialogResult.No) {
              AddTextToProtocol("Backup hergestellt, Konvertierung abgebrochen");
              return;
            }
          }
          // otherwise proceed
          var file = File.ReadAllText(filePath);
          AddTextToProtocol("Datei gelesen, {0} Zeichen", file.Length);
          var folder = Path.GetDirectoryName(filePath);
          if (folder != null && Directory.Exists(folder)) {
            // store after we're sure it's valid
            genericImgPathPart = String.Format("{0}_files", Path.GetFileNameWithoutExtension(filePath));
            imgFolder = Path.Combine(folder, genericImgPathPart);
            htmlFileName = filePath;
            if (Directory.Exists(imgFolder)) {
              AddTextToProtocol("Bilderordner {0} gefunden", imgFolder);
              var imgCnt = Directory.GetFiles(imgFolder, "*.png")
                .Union(Directory.GetFiles(imgFolder, "*.jpg"))
                .Union(Directory.GetFiles(imgFolder, "*.gif"))
                .Union(Directory.GetFiles(imgFolder, "*.jpeg"));
              AddTextToProtocol("{0} Bilder gefunden", imgCnt.Count());
              // look for images folder
            } else {
              AddTextToProtocol("! Kein Bildordner gefunden (Standardname: <Dateiname>_files)");
            }
          } else {
            AddTextToProtocol("! Stammordner nicht vorhanden");

          }
        }
      }
    }

    private string GetBackupFileName(string fileName) {
      if (fileName == null) return null;
      var backupPath = Path.GetDirectoryName(fileName);
      var stemName = Path.GetFileNameWithoutExtension(fileName);
      var backupFileName = Path.Combine(backupPath, String.Format("{0}.backup", stemName));
      return backupFileName;
    }

    private DialogResult CheckBackup(string fileName) {
      AddTextToProtocol("Prüfe auf Backup");
      var backupFileName = GetBackupFileName(fileName);
      if (File.Exists(backupFileName)) {
        AddTextToProtocol("Backup gefunden");
        return MessageBox.Show("Ein Backup existiert. Überschreiben [OK] oder Wiederherstellen [Cancel]", "Frage", MessageBoxButtons.OKCancel);
      }
      AddTextToProtocol("Kein Backup gefunden");
      return DialogResult.Ignore;
    }

    private void AddTextToProtocol(string text, params object[] values) {
      lblProtocol.AppendText(String.Format(text, values) + Environment.NewLine);

    }

    private void bilderHinzufügenToolStripMenuItem_Click(object sender, EventArgs e) {
      if (openImages.ShowDialog() == DialogResult.OK) {
        if (openImages.FileNames.Length == 0) {
          AddTextToProtocol("Keine Auswahl von Bildern");
        } else {
          AddTextToProtocol("Es werden {0} kopiert", openImages.FileNames.Length);
          imgFileArrayToAdd = openImages.FileNames;
        }
      }
    }

    private void prüfenToolStripMenuItem_Click(object sender, EventArgs e) {
      AddTextToProtocol("Prüfe");
      if (String.IsNullOrEmpty(htmlFileName)) {
        MessageBox.Show("Es wurde keine Datei geladen. Bitte erst HTML laden.", "Keine Datei", MessageBoxButtons.OK,
          MessageBoxIcon.Error);
        return;
      }
      var doc = new HAP.HtmlDocument();
      try {
        doc.Load(htmlFileName);
        var nav = doc.CreateNavigator();
        AddTextToProtocol("Datei okay");
        // <img>
        var imgTags = nav.Select("//img");
        AddTextToProtocol("{0} verlinkte Bilder", imgTags.Count);
        var imgFolderCnt = Directory.GetFiles(imgFolder).Length;
        var err = false;
        if (imgFolderCnt < imgTags.Count) {
          AddTextToProtocol(" --- Ergebnis: {0} Links {1} Bilder (Bilder fehlen, bitte hochladen)", imgTags.Count,
            imgFolderCnt);
          err = true;
        }
        if (imgFolderCnt > imgTags.Count) {
          AddTextToProtocol(" --- Ergebnis: {0} Links {1} Bilder (Zuviele Bilder, ist aber okay)", imgTags.Count,
            imgFolderCnt);
        }
        if (imgFolderCnt == imgTags.Count) {
          AddTextToProtocol(" --- Ergebnis: {0} Links {1} Bilder (Perfekt)", imgTags.Count,
            imgFolderCnt);
        }
        if (err) {
        }
        imgFileArrayToChange = new Dictionary<string, string>();
        var successCnt = 0;
        var missingCnt = 0;
        // Check all tags
        foreach (var imgTag in imgTags) {
          var node = ((HAP.HtmlNodeNavigator)imgTag).CurrentNode;
          if (node != null) {
            var foundImg = false;
            var src = node.Attributes["src"].Value;
            if (string.IsNullOrEmpty(src)) continue;
            // lookup in Img Folder  
            var attemptToFolder = Directory.Exists(imgFolder) ? imgFolder : String.Empty;
            var attemptToFile = Path.GetFileName(src);
            var lookupFullPathFiles = Directory.GetFiles(attemptToFolder, attemptToFile);
            if (lookupFullPathFiles.Length == 0) {
              // correct HTML encoding
              attemptToFile = HttpUtility.UrlDecode(attemptToFile);
              lookupFullPathFiles = Directory.GetFiles(attemptToFolder, attemptToFile);
              if (lookupFullPathFiles.Length == 0) {
                attemptToFile = System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(attemptToFile));
                lookupFullPathFiles = Directory.GetFiles(attemptToFolder, attemptToFile);
                if (lookupFullPathFiles.Length == 0) {
                  AddTextToProtocol(" --- Bild nicht gefunden: {0}", attemptToFile);
                  missingCnt++;
                } else {
                  foundImg = true;
                }
              } else {
                foundImg = true;
              }
            } else {
              foundImg = true;
            }
            if (foundImg) {
              successCnt++;
              AddTextToProtocol(@"Bild gefunden, Pfad falsch ({0}\{1})", attemptToFolder, attemptToFile);
              // add with proper name to import array
              if (!imgFileArrayToChange.ContainsKey(src)) {
                imgFileArrayToChange.Add(src, attemptToFile);
              }
            }
          }
        }
        AddTextToProtocol(" --- Gesamt: {0} korrekt verlinkt, {1} korrigierbar, {2} fehlen", successCnt,
          imgFileArrayToChange.Count(), missingCnt);
      } catch (Exception ex) {
        AddTextToProtocol("! Fehler: " + ex.Message);
      } finally {
      }
    }

    private void konvertierenToolStripMenuItem_Click(object sender, EventArgs e) {
      if (imgFileArrayToChange.Any()) {
        AddTextToProtocol("{0} Bilder zu korrigieren", imgFileArrayToChange.Count());
        var doc = new HAP.HtmlDocument();
        doc.Load(htmlFileName);
        if (doc.DocumentNode != null) {
          foreach (var imgFileName in imgFileArrayToChange) {
            AddTextToProtocol("Konvertiere Datei {0}", imgFileName);
            // <img>
            var imgTag = doc.DocumentNode.SelectNodes(String.Format(@"//img[@src='{0}']", imgFileName.Key));
            if (imgTag.Count == 1) {
              var imgFolderRelative = Path.GetFileName(imgFolder);
              imgTag.Single().SetAttributeValue("src", String.Format("{0}/{1}", imgFolderRelative, imgFileName.Value));
            }
          }
          var backupCopy = String.Format("{0}.backup", Path.GetFileNameWithoutExtension(htmlFileName));
          var backupCopyPath = Path.Combine(Path.GetDirectoryName(htmlFileName), backupCopy);
          if (File.Exists(backupCopyPath)) {
            var r =
              MessageBox.Show(
                "Sicherungsdatei existiert. Backup überschreiben und weiter [Ja], Belassen und weiter [Nein] oder [Abbrechen]?\nDie HTMl-Datei wird beim fortsetzen verändert.",
                "Frage", MessageBoxButtons.YesNoCancel);
            switch (r) {
              case DialogResult.Yes:
                File.Copy(htmlFileName, backupCopyPath, true);
                AddTextToProtocol("Backup erstellt.");
                doc.Save(htmlFileName);
                AddTextToProtocol("HTML gespeichert.");
                break;
              case DialogResult.No:
                doc.Save(htmlFileName);
                AddTextToProtocol("HTML gespeichert.");
                break;
              case DialogResult.Cancel:
                AddTextToProtocol("Funktion beendet.");
                break;
            }
          } else {
            File.Copy(htmlFileName, backupCopyPath);
            AddTextToProtocol("Erstes Backup erstellt.");
            doc.Save(htmlFileName);
            AddTextToProtocol("HTML gespeichert.");
          }
        }
      } else {
        AddTextToProtocol("Nichts zu korrigieren, Funktion beendet.");
      }
    }

    private void fehlerBehandelnToolStripMenuItem_Click(object sender, EventArgs e) {
      // convert into opus XML and manage conversion errors
      var mapping = GetDefaultMapping();
      var html = File.ReadAllText(htmlFileName, Encoding.Default);
      try {
        // Make XHTML
        var xhtmlDocument = new Texxtoor.BaseLibrary.Core.HtmlAgility.Pack.HtmlDocument();
        xhtmlDocument.OptionOutputAsXhtml = true;
        xhtmlDocument.OptionFixNestedTags = true;
        xhtmlDocument.OptionOutputAsXml = true;
        xhtmlDocument.LoadHtml(html);
        var ms = new MemoryStream();
        var tw = new XmlTextWriter(ms, Encoding.Default);
        xhtmlDocument.Save(tw);
        // change encoding, as our xslt module needs utf-8
        var enc = Encoding.GetEncoding("iso-8859-1");
        var text = enc.GetString(ms.ToArray());
        var xhtml = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
        // transform
        var xml = Html2XmlUtil.HtmlToOpusXsltParser(xhtml, mapping);
        // TODO: check XML here

        // TODO: if XML is okay store on disc for further processing
        var xmlFileName = Path.Combine(Path.GetDirectoryName(htmlFileName) ?? String.Empty, String.Format("{0}.xml", Path.GetFileNameWithoutExtension(htmlFileName)));
        if (File.Exists(xmlFileName)) {
          AddTextToProtocol("XML exists, deleting ");
          File.Delete(xmlFileName);
          AddTextToProtocol("XML deleted ");
        }
        AddTextToProtocol("Attempt to write XML");
        File.WriteAllText(xmlFileName, xml, Encoding.UTF8);
        AddTextToProtocol("XML written to disc at {0}", xmlFileName);
      } catch (Exception ex) {
        AddTextToProtocol("** Fehler beim Konvertieren in Texxtoor-XML: {0}", ex.Message);
      }
    }

    private NameValueCollection GetDefaultMapping(string name = "Default Mapping")
    {
      var d = new NameValueCollection();
      switch (name)
      {
        case "Default Mapping":
          //<xsl:param name="codeCharacter">ListingTextZchn</xsl:param>
          //<xsl:param name="codePara">ListingText</xsl:param>
          //<xsl:param name="listingCaption">Listingunterschrift</xsl:param>
          //<xsl:param name="imageCaption">Bildunterschrift</xsl:param>
          //<xsl:param name="tableCaption">Tabellenberschrift</xsl:param>
          //<xsl:param name="sidebarHint">IconHinweisText</xsl:param>
          //<xsl:param name="sidebarWarning">IconWarnungText</xsl:param>
          //<xsl:param name="textPara">StandardAbsatz|AufzhlungEinrckung</xsl:param>
          //<xsl:param name="bulletPara">Aufzhl1</xsl:param>
          //<xsl:param name="numberPara">AufzhlNumber</xsl:param>

          d.Add("codeCharacter", "hervorhebungZchn");
          d.Add("codePara", "Listing");
          d.Add("listingCaption", "ListingCaption");
          d.Add("imageCaption", "berschriftung");
          d.Add("tableCaption", "berschriftung");
          d.Add("sidebarHint", "SideBarContent");
          d.Add("sidebarWarning", "SideBarContent");
          d.Add("textPara", "MsoNormal");
          d.Add("bulletPara", "AbsatzBullet");
          d.Add("numberPara", "AbsatzNumber");
          break;
      }
      return d;
    }

    private void statusToolStripMenuItem_Click(object sender, EventArgs e) {

    }

    private void anmeldenToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var dlg = new Logon();
      dlg.ShowDialog();
    }

    private void hochladenToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var dlg = new UploadDocument();
      dlg.ShowDialog();

    }

    private void lokalExportierenToolStripMenuItem_Click(object sender, EventArgs e) {
      saveZipFile.FileName = String.Format("{0}.zip", Path.GetFileNameWithoutExtension(htmlFileName));
      if (saveZipFile.ShowDialog() == DialogResult.OK) {
        var targetFileName = String.Format("{0}.html", Path.GetFileNameWithoutExtension(htmlFileName));
        using (var zipFileToOpen = new FileStream(saveZipFile.FileName, FileMode.Create)) {
          using (var archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Update)) {
            // create root file
            var rootEntry = archive.CreateEntry(targetFileName);
            using (var writer = new BinaryWriter(rootEntry.Open())) {
              writer.Write(File.ReadAllBytes(htmlFileName));
            }
            // images with folder
            foreach (var imgFilePath in Directory.GetFiles(imgFolder)) {
              var normalizedPath = Path.Combine(Path.GetFileName(Path.GetDirectoryName(imgFilePath)),
                Path.GetFileName(imgFilePath));
              var imgEntry = archive.CreateEntry(normalizedPath);
              using (var writer = new BinaryWriter(imgEntry.Open())) {
                writer.Write(File.ReadAllBytes(imgFilePath));
              }
            }
          }
        }
        AddTextToProtocol("Zip geschrieben nach {0}", saveZipFile.FileName);
      }
    }

    private void Form1_Load(object sender, EventArgs e) {
      backupWiederherstellenToolStripMenuItem.Enabled = true;
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
      var ab = new AboutBox1();
      ab.ShowDialog();
    }

  }
}
