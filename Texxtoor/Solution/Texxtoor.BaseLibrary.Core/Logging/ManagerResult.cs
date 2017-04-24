using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.BaseLibrary.Core.Logging {


  public enum ResultKind {

    Unknown = 0,
    Information,
    Warning,
    Error

  }

  /// <summary>
  /// A return class with an additonal return object, usually the viewmodel exposed by BusinessLayer.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ManagerResult<T> : ManagerResult
    where T : IViewModel {

    public ManagerResult(string module) {
      Module = module;
    }


    public T ViewModel { get; set; }

  }

  public class ManagerResults<T> : ManagerResult
    where T : IEnumerable<IViewModel> {

    public ManagerResults(string module) {
      Module = module;
    }


    public T ViewModel { get; set; }

  }


  /// <summary>
  /// A return class for manager functions provided by business layer. Texts might be loalized.
  /// </summary>
  public class ManagerResult {

    public ManagerResult() {
    }

    public ManagerResult(string module, [CallerMemberName] string method = "Not Provided") {
      Module = module;
      Method = method;
    }

    public void SetInformation(string text, bool loggable = false) {
      Kind = ResultKind.Information;
      Text = text;
      if (loggable) {
        Logger.Info(text, Module);
      }
    }

    public void SetWarning(string text, bool loggable = false) {
      Kind = ResultKind.Warning;
      Text = text;
      if (loggable) {
        Logger.Warning(text, Module);
      }
    }

    public void SetError(string text, bool loggable = false) {
      Kind = ResultKind.Error;
      Text = text;
      if (loggable) {
        Logger.Error(text, Module);
      }
    }

    public ResultKind Kind { get; set; }

    public string Text { get; set; }

    public bool Logable { get; set; }

    public bool RaiseException { get; set; }

    public bool RoseException {
      get { return InnerException != null; }
    }

    public Exception InnerException { get; set; }

    public int Code { get; set; }

    public string Module { get; set; }

    public string Method { get; set; }

  }
}
