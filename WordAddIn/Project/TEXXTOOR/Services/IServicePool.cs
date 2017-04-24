using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEXXTOOR.Services {
	public interface IServicePool {

		T GetService<T>() where T : class;

		void AddService(object instance);

	}
}
