using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TEXXTOOR.Properties;
using TEXXTOOR.Services;
using TEXXTOOR.Services.Exceptions;
using TEXXTOOR.TexxtoorAddInService;

namespace TEXXTOOR {
	public partial class CommentsTaskPane : UserControl {

		private ServerService _service;

		public CommentsTaskPane() {
			InitializeComponent();
		}

		private void btnAddComment_Click(object sender, EventArgs e) {
			var strbtnName = ((Button)sender).Name;
			string strCommmentSubject = null;
			string strCommentBody = null;
			string strTarget = null;
			//string UserName = "TeamLead";
			var closed = false;
			switch (strbtnName) {
				case "btnAddPrivateComment":
					if (txtPrivateCommentSubject.Text == String.Empty && (txtPrivateCommentSubject.Text == String.Empty || txtPrivateCommentBody.Text == String.Empty)) {
						MessageBox.Show(Resources.CommentsTaskPane_btnAddComment_Click_Please_Enter_Subject);
						break;
					}
					strCommmentSubject = txtPrivateCommentSubject.Text;
					strCommentBody = txtPrivateCommentBody.Text;
					strTarget = "me";
					//txtPrivateCommentsList.Text = String.Format("{0}({1} user {2})\n{3}\n{4}", strCommemntSubject, DateTime.Now, UserName, strCommentBody, txtPrivateCommentsList.Text);
					closed = cbxPrivateComments.Checked;
					txtPrivateCommentSubject.Text = null;
					txtPrivateCommentBody.Text = null;
					break;
				case "btnAddTeamComments":
					if (txtPrivateCommentSubject.Text == String.Empty && (txtPrivateCommentSubject.Text == String.Empty || txtPrivateCommentBody.Text == String.Empty)) {
						MessageBox.Show(Resources.CommentsTaskPane_btnAddComment_Click_Please_Enter_Subject);
						break;
					}
					strCommmentSubject = txtTeamCommentSubject.Text;
					strCommentBody = txtTeamCommentBody.Text;
					strTarget = "team";
					//txtTeamCommentsList.Text = String.Format("{0}({1} user {2})\n{3}\n{4}", strCommemntSubject, DateTime.Now, UserName, strCommentBody, txtTeamCommentsList.Text);
					closed = cbxTeamComments.Checked;
					txtTeamCommentSubject.Text = null;
					txtTeamCommentBody.Text = null;
					break;
				case "btnAddReadersComment":
					if (txtPrivateCommentSubject.Text == String.Empty && (txtPrivateCommentSubject.Text == String.Empty || txtPrivateCommentBody.Text == String.Empty)) {
						MessageBox.Show(Resources.CommentsTaskPane_btnAddComment_Click_Please_Enter_Subject);
						break;
					}
					strCommmentSubject = txtReaderCommentSubject.Text;
					strCommentBody = txtReaderCommentBody.Text;
					strTarget = "reader";
					//txtReaderCommentsList.Text = String.Format("{0}({1} user {2})\n{3}\n{4}", strCommemntSubject, DateTime.Now, UserName, strCommentBody, txtReaderCommentsList.Text);
					closed = cbxReaderComments.Checked;
					txtReaderCommentSubject.Text = null;
					txtReaderCommentBody.Text = null;
					break;
				default:
					break;
			}
			int snippetId = 0; // TODO: locate current style and get Id from related container
			_service.SaveComment(snippetId, strTarget, strCommmentSubject, strCommentBody, closed, args => ShowComments(args, strTarget));
		}

		private void LoadComments() {
			const int snippetId = 0;
      start: 
			foreach (var target in new[] {"me", "reader", "team"}) {
				try {
					_service.LoadComments(snippetId, target, args => ShowComments(args, target));
				} catch (NoConnectionException ex) {
					var dr = MessageBox.Show(ex.Message, Resources.CommentsTaskPane_LoadComments_No_Connection, MessageBoxButtons.RetryCancel);
				  if (dr == DialogResult.Cancel) break;
          if (dr == DialogResult.Retry) goto start;
				}
			}
		}

		private void ShowComments(IEnumerable<Comment> listItems, string target) {
			foreach (var comment in listItems) {
				var strComment = String.Format("{0}({1} user {2})\n{3}", comment.Subject, comment.Date, comment.UserName, comment.Text);
				switch (target) {
					case "me":
						txtPrivateCommentsList.Text = strComment;
						break;
					case "team":
						txtTeamCommentsList.Text = strComment;
						break;
					case "reader":
						txtReaderCommentsList.Text = strComment;
						break;
				}
			}
		}


		internal void PopulateComments() {
			_service = ServicePool.Instance.GetService<ServerService>();
			txtPrivateCommentBody.Text = txtPrivateCommentSubject.Text = txtReaderCommentBody.Text = txtReaderCommentSubject.Text = txtTeamCommentBody.Text = txtTeamCommentSubject.Text = null;
			LoadComments();
		}
	}
}
