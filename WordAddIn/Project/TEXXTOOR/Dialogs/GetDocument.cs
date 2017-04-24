using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TEXXTOOR.TexxtoorAddInService;
using TEXXTOOR.Services;

namespace TEXXTOOR.Dialogs {
	public partial class GetDocument : Form {

		public IList<ServiceElement> Projects { get; set; }

		public int? DocumentId { get; set; }

		public int? ProjectId { get; set; }

		public bool NewDocument { get; set; }

		public GetDocument() {
			InitializeComponent();
		}

		private void GetDocument_Load(object sender, EventArgs e) {
			var client = ServicePool.Instance.GetService<ServerService>();
			client.GetAllProjects(args => {
				Projects = args;
				if (Projects.Any()) {
					lbProjects.DataSource = Projects;
					lbProjects.DisplayMember = "Name";
					lbProjects.ValueMember = "Id";
					lbProjects.Enabled = true;
				}
			});
		}

		private void lbProjects_SelectedIndexChanged(object sender, EventArgs e) {
			var idx = ((ServiceElement)lbProjects.SelectedItem).Id;
			SetDocumentListbox(idx);
		}

		private void SetDocumentListbox(int idx) {
			var project = Projects.SingleOrDefault(p => p.Id == idx);
			DocumentId = null;
			if (project != null) {
				ProjectId = project.Id;
				var documents = project.Children;
				lbDocuments.DataSource = documents;
				lbDocuments.DisplayMember = "Name";
				lbDocuments.ValueMember = "Id";
				lbDocuments.Enabled = true;
			} else {
				ProjectId = null;
				lbDocuments.DataSource = null;
				lbDocuments.Items.Clear();
				lbDocuments.Enabled = false;
			}
		}

		private void lbDocuments_SelectedIndexChanged(object sender, EventArgs e) {
			var idx = ((ServiceElement)lbDocuments.SelectedItem).Id;			
			if (idx > -1) {
				DocumentId = idx;
				btnSelect.Enabled = true;
				labelDocId.Text = idx.ToString();
			}
			else {
				DocumentId = null;
				btnSelect.Enabled = false;
				labelDocId.Text = "n/a";
			}
		}


		private void chkNewDocument_CheckedChanged(object sender, EventArgs e) {
			NewDocument = chkNewDocument.Checked;
			if (chkNewDocument.Checked) {
				DocumentId = null;
				lbDocuments.Enabled = false;
				lbDocuments.Items.Clear();
				lbDocuments.DataSource = null;
				btnSelect.Enabled = true;
			}
			else {
				var idx = ((ServiceElement)lbProjects.SelectedItem).Id;
				if (idx > -1) {
					SetDocumentListbox(idx);
				}				
			}
		}

		private void btnSelect_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Ignore;
			if (DocumentId.HasValue) {
				DialogResult = DialogResult.OK;
				ServicePool.Instance.GetService<ServerService>().SetDocumentId(DocumentId.Value);
				ServicePool.Instance.GetService<AddInService>().ResetSemanticList();
			}
			Close();
		}

		private void btnClose_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}

}
