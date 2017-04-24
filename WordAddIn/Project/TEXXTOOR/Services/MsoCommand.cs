using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEXXTOOR.Services {
	public class MsoCommand : ICommand {

		private readonly Action<string> _callback;

		public MsoCommand(string commandId, string msoName) {
			CommandId = commandId;
			MsoName = msoName;
		}

		public MsoCommand(string commandId, string msoName, Action<string> callback) :this(commandId, msoName) {
			_callback = callback;
		}

		public string CommandId { get; set; }

		public string MsoName { get; set; }

		public bool CanExecute(object parameter) {
			throw new NotImplementedException();
		}

		public void Execute(object parameter) {
			if (_callback != null) {
				_callback(MsoName);
			}
			else {
				ServicePool.Instance.GetService<DocumentService>().CurrentDocument.CommandBars.ExecuteMso(MsoName);
			}
		}

		public event EventHandler CanExecuteChanged;
	}
}
