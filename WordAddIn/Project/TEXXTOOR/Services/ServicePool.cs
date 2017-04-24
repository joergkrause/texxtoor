using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TEXXTOOR.Services {


	[DebuggerStepThrough]
	public class ServicePool : IServicePool {

		private static readonly IDictionary<Type, object> _services;

		static ServicePool() {
			_services = new Dictionary<Type, object>();
		}

		private ServicePool() {
			
		}

		private static IServicePool _instance = null;
		private static object _lock = new object();

		public static IServicePool Instance {
			get {
				if (_instance == null)
				{
					lock (_lock) {
						if (_instance == null) {
							_instance = new ServicePool();
						}
					}
				}
				return _instance;
			}
		}

		public void AddService(object instance) {
			var name = instance.GetType();
			if (_services.ContainsKey(name)) {
				_services[name] = instance;
			}
			else {
				_services.Add(name, instance);
			}
		}

		public T GetService<T>() where T : class {
			return (!_services.ContainsKey(typeof(T)) ? null : _services[typeof(T)]) as T;
		}

	}
}
