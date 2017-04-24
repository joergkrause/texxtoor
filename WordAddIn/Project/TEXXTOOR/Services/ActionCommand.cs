using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEXXTOOR.Services {
	public class ActionCommand : ICommand {

		private readonly Action _simpleCallback;
		private readonly Action<string, object> _callback;

		public ActionCommand(string commandId, Action<string, object> callback) {
			CommandId = commandId;
			_callback = callback;
		}

		public ActionCommand(string commandId, Action callback) {
			CommandId = commandId;
			_simpleCallback = callback;
		}

		public string CommandId { get; set; }

		public string MsoName { get; set; }

		public bool CanExecute(object parameter) {
			throw new NotImplementedException();
		}

		public void Execute(object parameter) {
			if (_simpleCallback != null) {
				_simpleCallback();
			}
			if (_callback != null) {
				_callback(CommandId, parameter);
			}
		}

		public event EventHandler CanExecuteChanged;
	}
}
