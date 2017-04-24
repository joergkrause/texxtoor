using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEXXTOOR.Services {
	
	public interface ICommand {

		string CommandId { get; set; }

		bool CanExecute(object parameter);

		void Execute(object parameter);

		event EventHandler CanExecuteChanged;

	}
}
