using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Office.Core;
using TEXXTOOR.TexxtoorAddInService;
using TEXXTOOR.Properties;
using System.Threading;
using System.Threading.Tasks;

namespace TEXXTOOR.Services {
	public class AddInService : IService {

		private IRibbonUI _ribbon;
		private bool _addInEnabled;

		private static IDictionary<string, Bitmap> _customIcons = new Dictionary<string, Bitmap>();

		public AddInService(IRibbonUI ribbon) {
			_ribbon = ribbon;
			GetCustomIcons();
		}

		public bool AddInEnabled {
			get { return _addInEnabled; }
			set {
				_addInEnabled = value;
				Invalidate();
			}
		}

		internal void Invalidate(string controlId = null) {
			if (!String.IsNullOrEmpty(controlId)) {
				_ribbon.InvalidateControl(controlId);
			}
			else {
				_ribbon.Invalidate();
			}
		}

		private static void GetCustomIcons() {
			_customIcons.Add("btnHeading1", Resources.h1);
			_customIcons.Add("btnHeading2", Resources.h2);
			_customIcons.Add("btnHeading3", Resources.h3);
			_customIcons.Add("btnHeading4", Resources.h4);
			_customIcons.Add("btnHeading5", Resources.h5);
			_customIcons.Add("btnMakeText", Resources.Text);
		}

		# region Semantic Lists

		private readonly Dictionary<TermType, string> _semanticMenu = new Dictionary<TermType, string>();

		internal void ResetSemanticList() {
			_semanticMenu.Clear();
			foreach (TermType type in Enum.GetValues(typeof(TermType))) {
				Task.Factory.StartNew(() => GetSemanticMenu(type));
			}
			Invalidate();
		}

		internal string GetSemanticList(IRibbonControl control) {
			var controlTag = control.Tag;
			var type = (TermType) Enum.Parse(typeof (TermType), controlTag, true);
			return _semanticMenu.ContainsKey(type) ? 
				_semanticMenu[type] :
				String.Format(@"<menu xmlns=""http://schemas.microsoft.com/office/2009/07/customui"" ><button id=""lblNo"" label=""No entries"" /></menu>");
		}


		/// <summary>
		/// Build all the drop downs dynamically.
		/// </summary>
		/// <param name="type">TermType</param>
		/// <returns></returns>
		private string GetSemanticMenu(TermType type) {
			string buttons = BindSemanticList(type);
			var sbMenu = new StringBuilder(@"<menu xmlns=""http://schemas.microsoft.com/office/2009/07/customui"" >");			
			sbMenu.Append(buttons);
			sbMenu.Append(@"</menu>");
			return sbMenu.ToString();
		}

		private string BindSemanticList(TermType type) {
			var evt = new ManualResetEvent(false);
			var listItems = "";
			ServicePool.Instance.GetService<ServerService>().SemanticLists(type, (e) => {
				var varList = e;
				// create a button per list
				int i = 1;
				foreach (var btn in varList) {
					var strid = String.Format("{0}_{1}_{2}", type, btn.Key, btn.Value);
					listItems += String.Format(@"<button id=""{0}{4}"" label=""{1}"" onAction=""{2}"" tag=""{3}"" /> ", 
						type.ToString().ToLowerInvariant(), 
						btn.Value, 
						"OnSemanticAction", 
						strid,
						i++);
				}
				// wait for callback
				evt.Set();				
			});
			evt.WaitOne(Settings.Default.ServiceTimeout);
			return listItems;
		}

		# endregion

		internal Bitmap GetIcon(string strOfficeId) {
			if (_customIcons.ContainsKey(strOfficeId)) {
				return _customIcons[strOfficeId];
			}
			throw new InvalidOperationException("Missing icon " + strOfficeId);
		}

	}
}
