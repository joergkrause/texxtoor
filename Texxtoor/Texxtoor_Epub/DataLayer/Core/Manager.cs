using System;
using System.Globalization;
using System.Web;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.Editor.Context;

namespace Texxtoor.Editor.Core {
  
  /// <summary>
  /// Base class for all BLL classes
  /// </summary>
  public abstract class Manager<T> : Singleton<T> where T : new() {

    private EditorContext _ctx;

    protected EditorContext Ctx {
      get {
        if (_ctx == null) {
          return DataContextFactory.GetWebRequestScopedDataContext<EditorContext>();
        }
        return _ctx;
      }
    }

    public EditorContext Context {
      get { return Ctx; }
      set { _ctx = value; }
    }

    public int SaveChanges() {
      return Ctx.SaveChanges();
    }

    protected void ClearNavCache(string username) {
      var key = String.Format("NavCache-{0}", username);
      var c = HttpContext.Current.Cache;
      c.Remove(key);
    }

    public string CurrentCulture {
      get {
        var cult = System.Threading.Thread.CurrentThread.CurrentUICulture;
        return cult.Parent == CultureInfo.InvariantCulture ? cult.TwoLetterISOLanguageName : cult.Parent.TwoLetterISOLanguageName;
      }
    }

  }
}
