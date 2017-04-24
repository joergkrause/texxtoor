using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.ServiceModel;
using System.Threading;
using System.Web;

namespace Texxtoor.DataModels.Context {
  /// <summary>
  /// This class provides several static methods for loading DataContext objects 
  /// in a variety of ways. You can load the data context as normal one new instance
  /// at a time, or you can choose to use one of the scoped factory methods that
  /// can scope the DataContext to a WebRequest or a Thread context (in a WinForm app
  /// for example).
  /// 
  /// Using scoped variants can be more efficient in some scenarios and allows passing
  /// a DataContext across multiple otherwise unrelated components so that the change
  /// context can be shared. 
  /// </summary>
  public class UnitOfWorkFactory {

    /// <summary>
    /// This stores the context for immediate reference if used from console app. 
    /// </summary>
    private static readonly Dictionary<string, object> ConsoleCache = new Dictionary<string, object>();

    /// <summary>
    /// Creates a ASP.NET Context scoped instance of a DataContext. This static
    /// method creates a single instance and reuses it whenever this method is
    /// called.
    /// 
    /// This version creates an internal request specific key shared key that is
    /// shared by each caller of this method from the current Web request.
    /// </summary>
    public static TDataContext GetIUnitOfWorkContext<TDataContext>()
            where TDataContext : DbContext, new() {
      // *** Create a request specific unique key 
              return (TDataContext)GetIUnitOfWorkContextInternal(typeof(TDataContext), null, null);
    }

    /// <summary>
    /// Creates a ASP.NET Context scoped instance of a DataContext. This static
    /// method creates a single instance and reuses it whenever this method is
    /// called.
    /// 
    /// This version lets you specify a specific key so you can create multiple 'shared'
    /// DataContexts explicitly.
    /// </summary>
    /// <typeparam name="TDataContext"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public static TDataContext GetIUnitOfWorkContext<TDataContext>(string key)
                               where TDataContext : DbContext, new() {
                                 return (TDataContext)GetIUnitOfWorkContextInternal(typeof(TDataContext), key, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDataContext"></typeparam>
    /// <param name="key"></param>
    /// <param name="connectionString"> </param>
    /// <returns></returns>
    public static TDataContext GetIUnitOfWorkContext<TDataContext>(string key, string connectionString)
                               where TDataContext : DbContext, new() {
                                 return (TDataContext)GetIUnitOfWorkContextInternal(typeof(TDataContext), key, connectionString);
    }

    /// <summary>
    /// Internal method that handles creating a context that is scoped to the HttpContext Items collection
    /// by creating and holding the DataContext there.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="key"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    static object GetIUnitOfWorkContextInternal(Type type, string key, string connectionString) {
      object context;

      if (HttpContext.Current == null) {
        return new PortalContext();
      }

      // *** Create a unique Key for the Web Request/Context 
      if (key == null)
        key = "__WRSCDC_" + HttpContext.Current.GetHashCode().ToString("x") + Thread.CurrentContext.ContextID;

      context = HttpContext.Current.Items[key];
      if (context == null || ((DbContext) context).Database.Connection == null) {
        if (String.IsNullOrEmpty(connectionString)) {
          connectionString = HttpContext.Current.Application["ConnectionStringName"] as string;
        }
        context = connectionString == null ? Activator.CreateInstance(type) : Activator.CreateInstance(type, connectionString);
        HttpContext.Current.Items[key] = context;
      }
      return context;
    }



  }
}
