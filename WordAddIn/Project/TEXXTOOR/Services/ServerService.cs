using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TEXXTOOR.Dialogs;
using TEXXTOOR.Properties;
using TEXXTOOR.Services.Exceptions;
using TEXXTOOR.TexxtoorAddInService;
using System.Drawing;
using System.IO;


namespace TEXXTOOR.Services {
	public class ServerService : IService {

		private readonly UploadServiceClient service = null;
		private int? _documentId;
		private string _ssid;
		private ProgressBarDlg progressBar;

		public ServerService() {
			service = new UploadServiceClient();
			service.PublishDocumentCompleted += new EventHandler<PublishDocumentCompletedEventArgs>(service_PublishDocumentCompleted);
			progressBar = new ProgressBarDlg();
			progressBar.SetMax(100);
		}

		public UploadServiceClient Client {
			get {
				return service;
			}
		}

		/// <summary>
		/// last successfully logged username
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// last password successfully used
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Session id for further communication
		/// </summary>
		public string Ssid {
			get {
				if (String.IsNullOrEmpty(_ssid)) {
					LogOn();
				}
				return _ssid;
			}
			set { _ssid = value; }
		}

		public bool HasError { get; set; }

		/// <summary>
		/// Project on Server
		/// </summary>
		public int ProjectId { get; set; }

		/// <summary>
		/// Opus on server
		/// </summary>
		public int GetDocumentId(bool ask = true) {
			if (ask) {
				if (!_documentId.HasValue || _documentId.GetValueOrDefault() == 0) {
					var getDocumentDlg = new GetDocument();
					if (getDocumentDlg.ShowDialog() == DialogResult.OK) {
						if (getDocumentDlg.NewDocument && getDocumentDlg.ProjectId.HasValue) {
							if (getDocumentDlg.DocumentId.HasValue) {
								SetDocumentId(getDocumentDlg.DocumentId.Value);
								// after we have the id we read the properties and store in the document service
								GetDocumentSettings(_documentId.Value, (e) => {
									ServicePool.Instance.GetService<DocumentService>().ListingSnippetDefault = e.ListingSnippetDefault;
									ServicePool.Instance.GetService<DocumentService>().ChapterTextDefault = e.ChapterDefault;
									ServicePool.Instance.GetService<DocumentService>().SectionTextDefault = e.SectionDefault;
								});
							}
						}
					}
				}
			}
			return _documentId.GetValueOrDefault();
		}

		public void SetDocumentId(int id) {
			_documentId = id;
		}

		# region Logon

		public bool LogOn() {
			if (String.IsNullOrEmpty(_ssid)) {
				if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password)) {
					var logonDlg = new LogOn();
					if (!String.IsNullOrEmpty(UserName)) {
						logonDlg.UserName = UserName;
					}
					if (!String.IsNullOrEmpty(Password)) {
						logonDlg.Password = Password;
					}
					var exceptionCount = 2;
					do {
						try {
							if (logonDlg.ShowDialog() == DialogResult.OK) {
								Ssid = logonDlg.Ssid;
								UserName = logonDlg.UserName;
								Password = logonDlg.Password;
								ServicePool.Instance.GetService<AddInService>().Invalidate(); // Force labels to reload
								return true;
							}
							break;
						} catch (Exception ex) {
							exceptionCount--;
							if (exceptionCount == 0) {
								var abortDlg = MessageBox.Show(ex.Message, Resources.ServerService_LogOn_Error_accessing_Server, MessageBoxButtons.RetryCancel);
								if (abortDlg == DialogResult.Retry) {
									exceptionCount = 2;
								}
							} else {
								logonDlg.UserName = UserName;
								logonDlg.Password = Password;
							}
						}
					} while (exceptionCount > 0);
				}
			} else {
				return true;
			}
			return false;
		}

		# endregion

		# region Operations


		public object UploadImage(int chapterId, int? currentSnippetId) {
			return service.UploadImage(GetDocumentId(), chapterId, currentSnippetId);
		}

		public void PublishDocument(string html) {
			if (LogOn()) {
				var id = GetDocumentId();
				if (id > 0) {
					progressBar.AutoProgressShow();
					try {
						service.PublishDocumentAsync(Ssid, id, html);
					} catch (Exception ex) {
						MessageBox.Show(ex.Message, Resources.ServerService_PublishDocument_Error_Publishing, MessageBoxButtons.OK);
					}
				} else {
					MessageBox.Show("No target document selected. Please try again.", Resources.ServerService_PublishDocument_Error_Publishing, MessageBoxButtons.OK);
				}
			}
		}

		void service_PublishDocumentCompleted(object sender, PublishDocumentCompletedEventArgs e) {
			progressBar.AutoProgressHide();
			MessageBox.Show(Resources.ServerService_PublishDocument_The_document_has_been_published_successfully_, Resources.ServerService_PublishDocument_Publishing_Done, MessageBoxButtons.OK);			
		}

		public void PublishNewDocument(int projectId, string name, string html) {
			if (LogOn()) {
				try {
					service.PublishNewDocumentAsync(Ssid, projectId, name, html);
					MessageBox.Show(Resources.ServerService_PublishDocument_The_document_has_been_published_successfully_, Resources.ServerService_PublishDocument_Publishing_Done, MessageBoxButtons.OK);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Resources.ServerService_PublishDocument_Error_Publishing, MessageBoxButtons.OK);
				}
			}
		}

		public void GetAllProjects(Action<List<ServiceElement>> callback) {
			if (LogOn()) {
				service.GetAllProjectsCompleted += (o, e) => callback(e.Result);
				service.GetAllProjectsAsync(Ssid);
			}
		}

		public void SignOut() {
			if (LogOn()) {
				service.SignOutAsync(Ssid);
			}
		}

		public void SignIn(string uname, string password) {
			service.SignInCompleted += (o, e) => Ssid = e.Result;
			service.SignInAsync(uname, password);
		}

		public void GetDocumentSettings(int id, Action<DocumentProperties> callback) {
			if (LogOn()) {
				service.GetDocumentSettingsCompleted += (o, e) => callback(e.Result);
				service.GetDocumentSettingsAsync(Ssid, id);
				return;
			}
			throw new NoConnectionException(Resources.ServerService_GetDocumentSettings_Login_failed_on_texxtoor_server);
		}

		public void SaveComment(int snippetId, string target, string subject, string comment, bool closed, Action<List<Comment>> callback) {
			if (LogOn()) {
				service.SaveCommentCompleted += (o, e) => callback(e.Result);
				service.SaveCommentAsync(Ssid, GetDocumentId(), snippetId, target, subject, comment, closed);
				return;
			}
			throw new NoConnectionException(Resources.ServerService_GetDocumentSettings_Login_failed_on_texxtoor_server);
		}

		public void LoadComments(int snippetId, string target, Action<List<Comment>> callback) {
			if (LogOn()) {
				service.LoadCommentsCompleted += (o, e) => callback(e.Result);
				service.LoadCommentsAsync(Ssid, GetDocumentId(), snippetId, target);
				return;
			}
			throw new NoConnectionException(Resources.ServerService_GetDocumentSettings_Login_failed_on_texxtoor_server);
		}

		public void SemanticLists(TermType type, Action<List<KeyValuePair<int, string>>> callback) {
			if (LogOn()) {
				service.SemanticListsCompleted += (o, e) => callback(e.Result);
				service.SemanticListsAsync(Ssid, GetDocumentId(), type);
				return;
			}
			throw new NoConnectionException(Resources.ServerService_GetDocumentSettings_Login_failed_on_texxtoor_server);
		}

		# endregion

		# region Server Images

		private int _imagesCount;
		private IList<int> _images;

		internal int GetServerImagesCount() {
			var docId = GetDocumentId();
			if (docId == 0) return 0;
			_images = service.GetServerImages(Ssid, docId);
			_imagesCount = _images.Count;
			return _imagesCount;
		}

		internal int GetServerImageId(int itemIndex) {
			return _images[itemIndex];
		}

		internal Image GetServerThumbnailImage(int itemIndex) {
			var bytes = service.GetServerImage(Ssid, _images[itemIndex], true, "100x100");
			using (var ms = new MemoryStream(bytes)) {
				return Image.FromStream(ms);
			}
		}

		internal Image GetServerImage(int id) {
			var bytes = service.GetServerImage(Ssid, id, false, "");
			using (var ms = new MemoryStream(bytes)) {
				return Image.FromStream(ms);
			}
		}

		# endregion

		# region

		private Dictionary<TermType, List<KeyValuePair<int, string>>> listCache = new Dictionary<TermType, List<KeyValuePair<int, string>>>();

		internal Dictionary<TermType, List<KeyValuePair<int, string>>> GetSemanticLists() {
			var docId = GetDocumentId();
			if (docId == 0) return null;
			if (!listCache.Any()) {
				listCache = Enum.GetValues(typeof(TermType))
					.Cast<TermType>()
					.ToDictionary(type => type, type => service.SemanticLists(Ssid, docId, type));
			}
			return listCache;
		}

		# endregion


	}
}
