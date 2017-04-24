using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImportFromWord.ServiceClient;
using Texxtoor.BaseLibrary.Core.Logging;

namespace ImportFromWord {
  public partial class UploadDocument : Form
  {

    private int projectId;
    private int documentId;

    public UploadDocument() {
      InitializeComponent();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
      RefreshProjectList();
    }

    private async void RefreshProjectList()
    {
      var logon = new Logon();
      while (String.IsNullOrEmpty(Client.SessionId)) {
        var dr = logon.ShowDialog(this);
        if (dr == DialogResult.Cancel) {
          Close();
        }
      }
      if (!String.IsNullOrEmpty(Client.SessionId))
      {
        var projects = await Client.Upload.GetAllProjectsAsync(Client.SessionId);
        var data = projects.Select(p => new
        {
          Name = String.Format("{0} [{1}]; {2} Texte", p.Name, p.Id, p.Children.Count),
          Value = p.Id
        }).ToList();
        lbProjects.DataSource = data;
        lbProjects.DisplayMember = "Name";
        lbProjects.ValueMember = "Value";
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private  async void btnSelect_Click(object sender, EventArgs e)
    {
      var dlg = new OpenFileDialog();
      dlg.CheckFileExists = true;
      dlg.DefaultExt = "*.zip";
      dlg.Multiselect = false;
      dlg.ShowReadOnly = false;
      dlg.SupportMultiDottedExtensions = false;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        var html = File.ReadAllText(dlg.FileName);
        var name = Path.GetFileNameWithoutExtension(dlg.FileName);
        Logger.Info("Start Hochladen", "Upload");
        var awaiter = Client.Upload.PublishNewDocumentAsync(Client.SessionId, projectId, name, html).GetAwaiter();
        awaiter.OnCompleted(() =>
        {
          Logger.Info("Hochladen fertig", "Upload");
          var newId = awaiter.GetResult();
          Logger.Info("Neuer Text hat ID " + newId, "Upload");
        });
      }
    }

    private void UploadDocument_Load(object sender, EventArgs e)
    {
      RefreshProjectList();
    }

    private void lbProjects_SelectedIndexChanged(object sender, EventArgs e) {
      dynamic val = lbProjects.SelectedItem;
      projectId = Convert.ToInt32(val.Value);
      lblProject.Text = val.Name;
    }
  }
}
