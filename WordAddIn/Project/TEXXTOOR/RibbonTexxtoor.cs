using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using TEXXTOOR.Properties;
using TEXXTOOR.Services;
using TEXXTOOR.TaskPanes;
using TEXXTOOR.TexxtoorAddInService;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new RibbonTexxtoor();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace TEXXTOOR {
	[ComVisible(true)]
	public class RibbonTexxtoor : IRibbonExtensibility {


		# region Server Data

		public static string LOCAL_APPN_DATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		# endregion


		public RibbonTexxtoor() {
			// First we add the services we want to use globally

			// MSO and other commands from ribbon
			ServicePool.Instance.AddService(new CommandService());
			// Server Services
			ServicePool.Instance.AddService(new ServerService());
			// document parameters and actions
			ServicePool.Instance.AddService(new DocumentService());
		}

		# region KeyHandler

		public void OnKeyPressed(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Enter:
					//var pInsertText = CurrentDocument.Paragraphs.Add();					
					break;
				default:
					if (e.Control) {
						switch (e.KeyCode) {
							case Keys.V:
								ServicePool.Instance.GetService<CommandService>().PlainPaste("", null);
								break;
						}
					}
					break;
			}			
		}

		# endregion

		#region IRibbonExtensibility Members

		public string GetCustomUI(string ribbonID) {
			return GetResourceText("TEXXTOOR.RibbonTexxtoor.xml");
		}

		#endregion

		#region Ribbon Callbacks
		//Create callback methods here. For more information about adding callback methods, select the Ribbon XML item in Solution Explorer and then press F1
		#region RibbonLoad

		public void OnRibbonLoad(IRibbonUI ribbonUi) {
			// template folder
			var strTemplatePath = String.Format("{0}\\Microsoft\\Templates", LOCAL_APPN_DATA_PATH);
			// check add-in folder
			if (!Directory.Exists(strTemplatePath)) {
				Directory.CreateDirectory(strTemplatePath);
			}
			// copy template into folder if it isn't present
			try {
				strTemplatePath = String.Format("{0}\\{1}", strTemplatePath, Settings.Default.TemplateName);
# if !DEBUG
				if (!File.Exists(strTemplatePath)) {
# endif
				File.WriteAllBytes(strTemplatePath, GetResourceBinary("TEXXTOOR.Resources." + Settings.Default.TemplateName));
# if !DEBUG
				}
# endif
			} catch (Exception exe) {
				Trace.TraceError(exe.Message);
			}
			// AddIn's ribbon service
			ServicePool.Instance.AddService(new AddInService(ribbonUi));

			Globals.ThisAddIn.Application.WindowSelectionChange += Application_WindowSelectionChange;
			Globals.ThisAddIn.Application.DocumentChange += Application_DocumentChange;
		}

		public bool GetControlVisible(IRibbonControl control) {
			//Debug.WriteLine(control.Id, "Check Visbility");
			var enabled = ServicePool.Instance.GetService<AddInService>().AddInEnabled;
			// in case enabled we show these three tabs:
			var lstTexxtoorTabs = new List<string> { "tabInsert", "tabSemantics", "tabTEXXTOOR", "tabTexxtoorInfo" };
			// in case disabled we show this tab:
			var lstTexxtoorActivationTab = new List<string> { "tabActivateTEXXTOOR" };
			if (enabled) {
				if (lstTexxtoorTabs.Contains(control.Id)) return true; // do show add-in tabs
				if (lstTexxtoorActivationTab.Contains(control.Id)) return false; // do not show add-in's activation tabs
				return false; // suppress all regular tabs
			}
			if (lstTexxtoorTabs.Contains(control.Id)) return false; // do not show add-in tabs
			if (lstTexxtoorActivationTab.Contains(control.Id)) return true; // do show add-in's activation tabs
			return true; // and show all regular tabs
		}


		void Application_DocumentChange() {
			ServicePool.Instance.GetService<DocumentService>().DocumentChange();
		}

		#endregion

		#region WindowSelection Change
		public void Application_WindowSelectionChange(Selection sel) {
			ServicePool.Instance.GetService<AddInService>().Invalidate();
		}
		#endregion

		# region Localization

		public string GetDynamicLabel(IRibbonControl control) {
			switch (control.Id) {
				case "lblUserName":
					var user = ServicePool.Instance.GetService<ServerService>().UserName;
					if (String.IsNullOrEmpty(user)) {
						return "Not Logged In";
					}
					return String.Format("User '{0}'", user);
				case "lblDocumentId":
					var id = ServicePool.Instance.GetService<ServerService>().GetDocumentId(false);
					if (id == 0) {
						return "";
					}
					return String.Format("Document ID '{0}'", id);
				case "lblOrderCheck":
					return ServicePool.Instance.GetService<DocumentService>().ConsistencyCheckOkay ? "Order Check OK" : "Order Check Fail";
				case "lblStyleCheck":
					return ServicePool.Instance.GetService<DocumentService>().AssureStylesCheckOkay ? "Style Check OK" : "Style Check Fail";
			}
			return "n/a";
		}

		public string GetLabel(IRibbonControl control) {
			return Resources.ResourceManager.GetString(String.Format("lb_{0}", control.Id));
		}

		public string GetSupertip(IRibbonControl control) {
			return Resources.ResourceManager.GetString(String.Format("st_{0}", control.Id));
		}

		public Bitmap GetImages(IRibbonControl control) {
			return ServicePool.Instance.GetService<AddInService>().GetIcon(control.Id);
		}

		# endregion

		# region Common Command Action

		/// <summary>
		/// All regular commands go here and get a callback in the command service.
		/// </summary>
		/// <param name="control"></param>
		public void OnAction(IRibbonControl control) {
			ServicePool.Instance.GetService<CommandService>().Execute(control.Id);
		}

		/// <summary>
		/// All action we forward directly to Word go here
		/// </summary>
		/// <param name="control"></param>
		public void OnMsoAction(IRibbonControl control) {
			ServicePool.Instance.GetService<CommandService>().ExecuteMso(control.Id);
		}

		/// <summary>
		/// Buttons that need a state use this.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="pressed"></param>
		/// <returns></returns>
		public bool OnToggleAction(IRibbonControl control, ref bool pressed) {
			pressed = ServicePool.Instance.GetService<CommandService>().ExecuteToggleMso(control.Id, pressed);
			return pressed;
		}

		/// <summary>
		/// Dynamically added buttons of server driven menues
		/// </summary>
		/// <param name="control"></param>
		public void OnSemanticAction(IRibbonControl control) {
			var type = (TermType)Enum.Parse(typeof(TermType), control.Tag, true);
			var parts = control.Id.Split('_');
			ServicePool.Instance.GetService<DocumentService>().InsertSemanticList(type, parts[0], parts[1], parts[2]);
		}

		public string GetSemanticList(IRibbonControl control) {
			return ServicePool.Instance.GetService<AddInService>().GetSemanticList(control);
		}

		/// <summary>
		/// Buttons for sections that need ask there availability go here.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public bool OnHeadingEnabled(IRibbonControl control) {
			return ServicePool.Instance.GetService<DocumentService>().GetHeadingEnabled(Convert.ToInt32(control.Tag));
		}

		/// <summary>
		/// Buttons for widgets that need ask there availability go here.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public bool OnInsertEnabled(IRibbonControl control) {
			if (control == null || control.Tag == null) return true;
			return ServicePool.Instance.GetService<DocumentService>().GetInsertEnabled(control.Tag);
		}

		# endregion

		# region Index

        public void ShowIndex(IRibbonControl control)
        {
            Microsoft.Office.Tools.CustomTaskPane _taskpane = null;
            IndexPreviewTaskPane cmntsTaskPane = null;
            if (Globals.ThisAddIn.CustomTaskPanes.Any(x => x.Control.GetType().Equals(typeof(IndexPreviewTaskPane))))
            {
                _taskpane = Globals.ThisAddIn.CustomTaskPanes.FirstOrDefault(x => x.Control.GetType().Equals(typeof(IndexPreviewTaskPane)));
                cmntsTaskPane = _taskpane.Control as IndexPreviewTaskPane;
            }
            else
            {
                cmntsTaskPane = new IndexPreviewTaskPane();
                _taskpane = Globals.ThisAddIn.CustomTaskPanes.Add(cmntsTaskPane, Resources.RibbonTexxtoor_GetComments_Comments___Meta_Data);
                _taskpane.Width = 500;
                _taskpane.VisibleChanged += IndexTaskPane_VisibleChanged;
            }
            cmntsTaskPane.PopulateIndex();
            _taskpane.Visible = true;
        }

        void IndexTaskPane_VisibleChanged(object sender, EventArgs e)
        {
        }

		# endregion

		# region Server Image Gallery

		public void OnGetGalleryRefresh(IRibbonControl control) {
			ServicePool.Instance.GetService<AddInService>().Invalidate();
		}

		public int OnGetGalleryItemCount(IRibbonControl control) {
			DialogResult dr = DialogResult.Retry;
			while (dr == DialogResult.Retry) {
				int count = ServicePool.Instance.GetService<ServerService>().GetServerImagesCount();
				if (count == 0) {
					dr = MessageBox.Show(Resources.RibbonTexxtoor_OnGetGalleryItemCount_NoCount, Resources.RibbonTexxtoor_OnGetGalleryItemCount_No_Content, MessageBoxButtons.RetryCancel);
					continue;
				}
				return count;
			}
			return 0;
		}

		public int OnGetGalleryItemId(IRibbonControl control, int itemIndex) {
			return ServicePool.Instance.GetService<ServerService>().GetServerImageId(itemIndex);
		}

		public Image OnGetGalleryItemImage(IRibbonControl control, int itemIndex) {
			return ServicePool.Instance.GetService<ServerService>().GetServerThumbnailImage(itemIndex);
		}

		public void OnGetGalleryImage(IRibbonControl control, string id, int index) {
			var serverId = Int32.Parse(id);
			var image = ServicePool.Instance.GetService<ServerService>().GetServerImage(serverId);
			ServicePool.Instance.GetService<DocumentService>().InsertFigure(image, serverId);
		}

		# endregion

		#region GetComments

		public void ShowComments(IRibbonControl control) {
			Microsoft.Office.Tools.CustomTaskPane _taskpane = null;
			CommentsTaskPane cmntsTaskPane = null;
			if (Globals.ThisAddIn.CustomTaskPanes.Any(x => x.Control.GetType().Equals(typeof(CommentsTaskPane)))) {
				_taskpane = Globals.ThisAddIn.CustomTaskPanes.FirstOrDefault(x => x.Control.GetType().Equals(typeof(CommentsTaskPane)));
				cmntsTaskPane = _taskpane.Control as CommentsTaskPane;
			} else {
				cmntsTaskPane = new CommentsTaskPane();
				_taskpane = Globals.ThisAddIn.CustomTaskPanes.Add(cmntsTaskPane, Resources.RibbonTexxtoor_GetComments_Comments___Meta_Data);
				_taskpane.Width = 500;
				object obj = _taskpane.Window;
				_taskpane.Visible = true;
				_taskpane.VisibleChanged += CommentsTaskPane_VisibleChanged;				
			}
			cmntsTaskPane.PopulateComments();
			_taskpane.Visible = true;
		}

		void CommentsTaskPane_VisibleChanged(object sender, EventArgs e) {
		}

		#endregion

		#region InternalLinks
		public void ShowInternalLinks(IRibbonControl control) {

			Microsoft.Office.Tools.CustomTaskPane _taskpane = null;
			InternalLinkTaskPane cmntsTaskPane = null;
			if (Globals.ThisAddIn.CustomTaskPanes.Any(x => x.Control.GetType().Equals(typeof(InternalLinkTaskPane)))) {
				_taskpane = Globals.ThisAddIn.CustomTaskPanes.FirstOrDefault(x => x.Control.GetType().Equals(typeof(InternalLinkTaskPane)));
				cmntsTaskPane = _taskpane.Control as InternalLinkTaskPane;
			} else {
				cmntsTaskPane = new InternalLinkTaskPane();
				_taskpane = Globals.ThisAddIn.CustomTaskPanes.Add(cmntsTaskPane, Resources.RibbonTexxtoor_InternalLinks_Internal_Links);
				_taskpane.Width = 500;
				_taskpane.Visible = true;
				_taskpane.VisibleChanged += InternalTaskPane_VisibleChanged;
			}
			cmntsTaskPane.PopulateLinks();
			_taskpane.Visible = true;
		}

		void InternalTaskPane_VisibleChanged(object sender, EventArgs e) {
		}

		#endregion

		#region KeyState

		public bool GetPressed(IRibbonControl control) {
			return ServicePool.Instance.GetService<CommandService>().GetButtonPressed(control.Id);
		}

		#endregion

		#endregion

		#region Helpers

		private static string GetResourceText(string resourceName) {
			var asm = Assembly.GetExecutingAssembly();
			var resourceNames = asm.GetManifestResourceNames();
			for (var i = 0; i < resourceNames.Length; ++i) {
				if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0) {
					using (var resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i]))) {
						if (resourceReader != null) {
							return resourceReader.ReadToEnd();
						}
					}
				}
			}
			return null;
		}

		private static byte[] GetResourceBinary(string resourceName) {
			var asm = Assembly.GetExecutingAssembly();
			var resourceNames = asm.GetManifestResourceNames();
			for (var i = 0; i < resourceNames.Length; ++i) {
				if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0) {
					var s = asm.GetManifestResourceStream(resourceNames[i]);
					var buffer = new byte[s.Length];
					s.Read(buffer, 0, buffer.Length);
					return buffer;
				}
			}
			return null;
		}

		#endregion


	}
}
