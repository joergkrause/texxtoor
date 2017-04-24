using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImportFromWord.PlatformService;
using ImportFromWord.ServiceClient;

namespace ImportFromWord {
  public partial class Logon : Form {

    public Logon() {
      InitializeComponent();
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private async void btnLogon_Click(object sender, EventArgs e)
    {
      var dlgLogon = System.Windows.Forms.DialogResult.OK;
      try {
        string sid = await Client.Upload.SignInAsync(txtUsername.Text.Trim(), txtPassword.Text.Trim());
        if (!String.IsNullOrEmpty(sid)) {
          Client.SessionId = sid;
          DialogResult = DialogResult.OK;
        } else {
          DialogResult = DialogResult.No;
        }
      } catch (FaultException<SignFault> fault)
      {
        dlgLogon = MessageBox.Show(this, "Anmeldung nicht möglich. Serverfehler:\n\n" + fault.Detail.Description, "Fehler", MessageBoxButtons.AbortRetryIgnore);
      }
      DialogResult = dlgLogon;
      if (dlgLogon != DialogResult.Retry)
      {
        Close();
      }
    }
  }
}
