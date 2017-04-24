using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using TEXXTOOR.Properties;
using TEXXTOOR.Services;
using TEXXTOOR.TexxtoorAddInService;

namespace TEXXTOOR.Dialogs {
	public partial class LogOn : Form {


		public string UserName {
			get {
				return txtName.Text;
			}
			set {
				txtName.Text = value;
			}
		}

		public string Password {
			get {
				return txtPassword.Text;
			}
			set {
				txtPassword.Text = value;
			}
		}

		public string Ssid { get; set; }

		private readonly UploadServiceClient _client;

		public LogOn() {
			InitializeComponent();
			progressBarLogon.Visible = false;
			timerProgress.Tick += timerProgress_Tick;
			_client = ServicePool.Instance.GetService<ServerService>().Client;
			_client.SignInCompleted += client_SignInCompleted;			
		}

		void client_SignInCompleted(object sender, SignInCompletedEventArgs e) {
			var ssid = e.Result;
			if (!String.IsNullOrEmpty(ssid)) {
				Guid id;
				if (Guid.TryParse(ssid, out id)) {
					DialogResult = DialogResult.OK;
					Ssid = ssid;
					progressBarLogon.Visible = false;
					Close();
					return;
				}
				MessageBox.Show(Resources.LogOn_btnLogon_Click_Response_from_Server__ + ssid,
					Resources.LogOn_btnLogon_Click_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} else {
				MessageBox.Show(Resources.LogOn_btnLogon_Click_No_response_from_Server__Please_try_again_,
					Resources.LogOn_btnLogon_Click_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			btnLogon.Enabled = true;

		}

		private int timeOutCount = 0;

		void timerProgress_Tick(object sender, EventArgs e) {
			if (progressBarLogon.Value >= progressBarLogon.Maximum) {
				timeOutCount++;
				progressBarLogon.Value = 0;				
				if (timeOutCount == 2) {
					progressBarLogon.Value = 0;
					progressBarLogon.Visible = false;
					timerProgress.Stop();
					timeOutCount = 0;
					btnLogon.Enabled = true;
				}
			}
			else {
				progressBarLogon.Value = progressBarLogon.Value + 10;
			}
		}

		private void linkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			var target = "http://www.texxtoor.de";
			try {
				Process.Start(target);
			} catch (Win32Exception noBrowser) {
				if (noBrowser.ErrorCode == -2147467259)
					MessageBox.Show(noBrowser.Message);
			} catch (Exception other) {
				MessageBox.Show(other.Message);
			}
		}

		private void txtName_TextChanged(object sender, EventArgs e) {
			btnLogon.Enabled = UserName.Length > 1 && UserName.Length > 1;
		}

		private void txtPassword_TextChanged(object sender, EventArgs e) {
			btnLogon.Enabled = Password.Length > 1 && Password.Length > 1;
		}

		private void btnCancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnLogon_Click(object sender, EventArgs e) {
			timeOutCount = 0;
			btnLogon.Enabled = false;
			progressBarLogon.Visible = true;
			timerProgress.Start();
			try {
				_client.SignInAsync(UserName, Password);
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Resources.LogOn_btnLogon_Click_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
