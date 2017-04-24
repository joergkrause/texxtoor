using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Texxtoor.BaseLibrary.Core.Extensions {

  public static class Helper {

    public static string CreatePath(string path, string file) {
      return String.Format("{0}{1}{2}", path, (String.IsNullOrEmpty(path) ? "" : "/"), file);
    }

    public static string NullSaveString(XAttribute a) {
      if (a == null) return String.Empty;
      return a.Value;
    }
    public static string NullSaveString(XAttribute a, bool urlDecoded) {
      if (a == null) return String.Empty;
      return (urlDecoded) ? HttpUtility.UrlDecode(a.Value) : a.Value;
    }
    public static int NullSaveInt32(this string a) {
      if (String.IsNullOrEmpty(a)) return 0;
      int outValue;
      if (Int32.TryParse(a, out outValue)) {
        return outValue;
      }
      return 0;
    }
    public static int NullSaveInt32(XAttribute a) {
      if (a == null) return 0;
      int outValue;
      if (Int32.TryParse(a.Value, out outValue)) {
        return outValue;
      }
      return 0;
    }
    public static bool? NullSaveBool(XAttribute a) {
      if (a == null || a.Value == null) return null;
      bool outValue;
      if (Boolean.TryParse(a.Value, out outValue)) {
        return outValue;
      }
      if (a.Value.ToLower().Equals("no")) return false;
      if (a.Value.ToLower().Equals("yes")) return true;
      return null;
    }

    /// <summary>
    /// Get the attribute name of an element's class using the specified custom attribute.
    /// </summary>
    /// <typeparam name="T">The type we're looking for</typeparam>
    /// <typeparam name="A">The decorator attribute that determines the name</typeparam>
    /// <param name="func">A resolver function for the property.</param>
    /// <returns>The XML attributes name</returns>
    public static string GetAttributeName<T, A>(Expression<Func<T, object>> func) where T : class {
      dynamic att = null;
      if (func.Body is MemberExpression) {
        att = ((MemberExpression)func.Body).Member.GetCustomAttributes(typeof(A), false).First() as Attribute;
      }
      if (func.Body is UnaryExpression) {
        att = ((MemberExpression)(func.Body as UnaryExpression).Operand).Member.GetCustomAttributes(typeof(A), false).First() as Attribute;
      }
      if (att != null && att is A) {
        return att.Name;
      }
      return null;
    }

    public static string NullSaveString(XNamespace dc, XElement e, string childName) {
      XElement o = ((XElement)e).Element((dc == null ? ((XElement)e).GetNamespaceOfPrefix("dc") : dc) + childName);
      if (o == null) return String.Empty;
      return o.Value;
    }

    /// <summary>
    /// Searches elements for an specific attribute and returns another attribute's value on first hit.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="selectAttribute"></param>
    /// <param name="dataAttribute"></param>
    /// <returns></returns>
    public static string ReadAttributeFromElement(IEnumerable<XElement> element, string selectAttribute, string dataAttribute) {
      var result = element.FirstOrDefault(e => e.Attribute("name").Value == selectAttribute);
      if (result != null) {
        var data = result.Attribute(dataAttribute).Value;
        return data;
      }
      return String.Empty;
    }

    public static void CopyProperties<T>(T source, object target, params Expression<Func<T, object>>[] expressions) { // ,params string[] properties) {            
      foreach (var expression in expressions) {
        Expression exp = expression.Body;
        string name = null;
        if (exp.NodeType == ExpressionType.MemberAccess) {
          name = ((MemberExpression)exp).Member.Name;
        }
        if (exp.NodeType == ExpressionType.Convert) {
          name = ((MemberExpression)((UnaryExpression)exp).Operand).Member.Name;
        }
        if (!String.IsNullOrEmpty(name)) {
          PropertyInfo piSource = source.GetType().GetProperty(name);
          PropertyInfo piTarget = target.GetType().GetProperty(name);
          if (piTarget != null) {
            object value = piSource.GetValue(source, null);
            piTarget.SetValue(target, value, null);
          }
        }
      }
    }

  }
}