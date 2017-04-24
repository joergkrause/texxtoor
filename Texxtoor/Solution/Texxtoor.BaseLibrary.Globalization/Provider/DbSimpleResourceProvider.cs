using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.Collections;
using System.Resources;
using System.Globalization;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using Texxtoor.DataModels.Models;

namespace Texxtoor.BaseLibrary.Globalization.Provider {

  /// <summary>
  /// Implementation of a very simple database Resource Provider that supports MVC 3 partially.
  /// </summary>
  public class DbSimpleResourceProvider : IResourceProvider {

    private object _lockObject = new object();

    /// <summary>
    /// Keep track of the 'className' passed by ASP.NET
    /// which is the ResourceSetId in the database.
    /// </summary>
    private readonly string _resourceSetName;

    public Dictionary<CultureInfo, Dictionary<string, object>> Resources = new Dictionary<CultureInfo, Dictionary<string, object>>();
    public static bool ProviderLoaded = false;

    /// <summary>
    /// Critical section for loading Resource Cache safely
    /// </summary>
    private static object _syncLock = new object();

    public DbSimpleResourceProvider(string virtualPath, string className) {
      if (!ProviderLoaded) {
        ProviderLoaded = true;
      }
      DbResourceDataManager.SetDbResourceDataManager(this);
      _resourceSetName = className;
    }

    /// <summary>
    /// The main worker method that retrieves a resource key for a given culture
    /// from a ResourceSet.
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    object IResourceProvider.GetObject(string resourceKey, CultureInfo culture) {
      if (culture == null || culture.TwoLetterISOLanguageName.Equals("iv")) {
        culture = CultureInfo.CurrentUICulture;
      }
      // currently we support neutral cultures only to keep things handy
      if (!culture.IsNeutralCulture) {
        culture = culture.Parent;
      }
      return GetObjectInternal(resourceKey, culture);
    }

    /// <summary>
    /// Internal lookup method that handles retrieving a resource
    /// by its resource id and culture. Realistically this method
    /// is always called with the culture being null or empty
    /// but the routine handles resource fallback in case the
    /// code is manually called.
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    object GetObjectInternal(string resourceKey, CultureInfo culture) {

      object value = null;
      if (HttpContext.Current.User.IsInRole(UserRole.CmsAdmin.ToString())) {
        if (Resources.ContainsKey(culture)) {
          Resources.Remove(culture);
        }
      }
      // first time build the complete fallback path and cache
      if (!Resources.ContainsKey(culture)) {
        // First get the current level
        lock (_lockObject) {
          Resources.Add(culture, new Dictionary<string, object>());
          if (!Resources.ContainsKey(culture)) {
            Resources.Add(culture, new Dictionary<string, object>());
          }
        }
      }
      if (!Resources[culture].Any()){
        lock (_lockObject) {
          if (!Resources[culture].Any()) {
            Resources[culture] = DbResourceDataManager.GetResourceSet(culture, this._resourceSetName);
            // if just called for invariant don't force lookup
            if (culture != CultureInfo.InvariantCulture) {
              // then decide on current level, not neutral means something like de-de
              if (!culture.IsNeutralCulture) {
                // because it's specfic trying to get the neutral too
                Resources[culture.Parent] = DbResourceDataManager.GetResourceSet(culture.Parent, this._resourceSetName);
              }
              // get it "as is", either neutral or specific
              Resources[CultureInfo.InvariantCulture] = DbResourceDataManager.GetResourceSet(CultureInfo.InvariantCulture, this._resourceSetName);
            }
          }
        }
      }
      // step down the fallback hierarchy on read level (2 level fix)
      if (!Resources.ContainsKey(culture)) {
        culture = culture.Parent;
        if (!Resources.ContainsKey(culture)) {
          culture = CultureInfo.InvariantCulture;
        }
      }
      // now we try to get the value, from most specific to fallback
      value = Resources[culture].ContainsKey(resourceKey) ? Resources[culture][resourceKey] : null;
      value = value ?? (Resources[culture.Parent].ContainsKey(resourceKey) ? Resources[culture.Parent][resourceKey] : null);
      value = value ?? (Resources[CultureInfo.InvariantCulture].ContainsKey(resourceKey) ? Resources[CultureInfo.InvariantCulture][resourceKey] : null);

      return value;
    }

    /// <summary>
    /// The Resource Reader is used parse over the resource collection
    /// that the ResourceSet contains. It's basically an IEnumarable interface
    /// implementation and it's what's used to retrieve the actual keys
    /// </summary>
    public IResourceReader ResourceReader  // IResourceProvider.ResourceReader
    {
      get {
        if (this._ResourceReader != null)
          return this._ResourceReader as IResourceReader;

        this._ResourceReader = new DbSimpleResourceReader(Resources);
        return this._ResourceReader as IResourceReader;
      }
    }
    private DbSimpleResourceReader _ResourceReader = null;

  }
}