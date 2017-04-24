using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEXXTOOR.Services.Exceptions {
	public class NoConnectionException : Exception {


		public NoConnectionException(string message) : base(message) {
			
		}

	}
}
