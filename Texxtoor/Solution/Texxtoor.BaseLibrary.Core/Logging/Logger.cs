using System;
using System.Diagnostics;
using System.Text;

namespace Texxtoor.BaseLibrary.Core.Logging {
  public static class Logger {

    public static void Error(string message, string module) {
      Error(message, module, null);
    }

    public static void Error(string message, string module, params object[] param) {
      WriteEntry(message, "error", module, param);
    }

    public static void Error(Exception ex, string module) {
      Error(ex, module, null);
    }

    public static void Error(Exception ex, string module, params object[] param) {
      WriteEntry(ex.Message, "error", module, param);
    }

    public static void Warning(string message, string module) {
      Warning(message, module, null);
    }

    public static void Warning(string message, string module, params object[] param) {
      WriteEntry(message, "warning", module, param);
    }

    public static void Info(string message, string module) {
      Info(message, module, null);
    }

    public static void Info(string message, string module, params object[] param) {
      WriteEntry(message, "info", module, param);
    }

    private static void WriteEntry(string message, string type, string module, params object[] param) {
      if (param == null) {
        Trace.WriteLine(
                string.Format("{0},{1},{2},{3}",
                              DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                              type,
                              module,
                              message));
      } else {
        StringBuilder sb = new StringBuilder();
        foreach (var item in param) {
          if (item == null) {
            sb.AppendFormat("NULL");
          } else {
            sb.AppendFormat("{0}={1}", item.GetType().Name, item);
          }
        }
        Trace.WriteLine(
        string.Format("{0},{1},{2},{3},{4}",
                      DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                      type,
                      module,
                      message,
                      sb.ToString()
                      ));

      }
    }
  }
}