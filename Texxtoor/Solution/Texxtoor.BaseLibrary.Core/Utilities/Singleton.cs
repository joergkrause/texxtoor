using System;
using System.Diagnostics;

namespace Texxtoor.BaseLibrary.Core {

  [DebuggerStepThrough]
  public class Singleton<T> where T: new() {
  
    private static volatile object instance;
    private static object syncRoot = new Object();    

    public static T Instance {
      get {
        if (instance == null) {
          lock (syncRoot) {
            if (instance == null)
              instance = new T();
          }
        }
        return (T)instance;
      }
    }
  }
}
