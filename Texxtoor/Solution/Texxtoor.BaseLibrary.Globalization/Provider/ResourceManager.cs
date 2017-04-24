using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;

namespace Texxtoor.BaseLibrary.Globalization.Provider {
  public static class ResourceManager {

    private static readonly Dictionary<int, string> _urlCache = new Dictionary<int, string>();
    private static readonly Dictionary<string, Assembly> _assemblyInfoCache = new Dictionary<string, Assembly>();

    public static Dictionary<string, Assembly> AssemblyInfoCache {
      get { return _assemblyInfoCache; }
    }

    internal static string GetMvcResourceUrl(Type type, string resourceName) {
      return GetMvcResourceUrl(type, resourceName, true);
    }

    internal static string GetMvcResourceUrl(Type type, string resourceName, bool htmlEncoded) {
      // cache Assembly for this Type
      var assembly = GetAssemblyInfo(type);
      var name = assembly.GetName();
      var urlkey = CreateWebResourceUrlCacheKey(assembly, resourceName, htmlEncoded);
      string url = null;
      if (!_urlCache.ContainsKey(urlkey)) {
        url = FormatWebResourceUrl(name.FullName, resourceName);
        _urlCache[urlkey] = url;
      } else {
        url = _urlCache[urlkey];
      }
      return url;
    }

    private static string FormatWebResourceUrl(string assemblyName, string resourceName) {
      return EncryptString(assemblyName + "|" + Path.GetFileName(resourceName));
    }

    private static string EncryptString(string str) {
      return HttpServerUtility.UrlTokenEncode(Encoding.ASCII.GetBytes(str));
    }

    private static Assembly GetAssemblyInfo(Type type) {
      if (!_assemblyInfoCache.ContainsKey(type.Assembly.FullName)) {
        _assemblyInfoCache[type.Assembly.FullName] = AssureAssemblyLoaded(type);
      }
      return _assemblyInfoCache[type.Assembly.FullName];
    }

    private static Assembly AssureAssemblyLoaded(Type assembly) {
      return Assembly.Load(assembly.Assembly.FullName);
    }

    private static int CreateWebResourceUrlCacheKey(Assembly assembly, string resourceName, bool htmlEncoded) {
      return HashCodeCombiner.CombineHashCodes(HashCodeCombiner.CombineHashCodes(assembly.GetHashCode(), resourceName.GetHashCode()), htmlEncoded.GetHashCode());
    }

    internal class HashCodeCombiner {

      internal static int CombineHashCodes(int h1, int h2) {
        return (((h1 << 5) + h1) ^ h2);
      }

      internal static int CombineHashCodes(int h1, int h2, int h3) {
        return CombineHashCodes(CombineHashCodes(h1, h2), h3);
      }

      internal static int CombineHashCodes(int h1, int h2, int h3, int h4) {
        return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
      }

      internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5) {
        return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
      }

    }


  }
}