using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions.EditorForModel {
  public static class ReflectionExtension {

    public static T GetPrivatePropertyValue<T>(this object obj, string propName) {
      if (obj == null) throw new ArgumentNullException("obj");
      PropertyInfo pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (pi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
      return (T)pi.GetValue(obj, null);
    }

    public static void SetPrivatePropertyValue<T>(this object obj, string propName, object value) {
      if (obj == null) throw new ArgumentNullException("obj");
      PropertyInfo pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (pi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
      pi.SetValue(obj, value);
    }

  }
}