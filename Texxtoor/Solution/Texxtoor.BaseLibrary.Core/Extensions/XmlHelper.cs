using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Texxtoor.BaseLibrary.Core.Extensions {

  public static class XmlHelper {

    public static XElement SetNamespace(this XElement src, XNamespace ns) {
      var name = src.IsEmptyNamespace() ? ns + src.Name.LocalName : src.Name;
      var element = new XElement(name, src.Attributes(),
            from e in src.Elements() select e.SetNamespace(ns));
      if (!src.HasElements) element.Value = src.Value;
      return element;
    }

    public static bool IsEmptyNamespace(this XElement src) {
      return (string.IsNullOrEmpty(src.Name.NamespaceName));
    }

    public static T GetEnumAttribute<T>(this XElement e, XName attribute) where T : struct, IConvertible {
      var val = e.Attribute(attribute).NullSafeString();
      return (T)Enum.Parse(typeof(T), val, true);
    }

    public static T GetEnumValue<T>(this XElement e, XName childElement) where T : struct, IConvertible {
      var val = e.Element(childElement).NullSafeString();
      return (T)Enum.Parse(typeof(T), val, true);
    }

    public static string NullSafeString(this XElement e) {
      return e != null ? e.Value : String.Empty;
    }

    public static string NullSafeString(this XAttribute a){
      return a == null ? String.Empty : a.Value;
    }
    public static string NullSafeString(this XAttribute a, string @default) {
      return a == null ? @default : a.Value;
    }

    public static decimal NullSafeDecimal(this XElement a) {
      if (a == null) return 0M;
      decimal outValue;
      return Decimal.TryParse(a.Value, NumberStyles.Number, new CultureInfo("en-us"),  out outValue) ? outValue : 0;
    }
    public static bool? NullSafeBool(this XElement a) {
      if (a == null) return null;
      bool outValue;
      if (Boolean.TryParse(a.Value, out outValue)) {
        return outValue;
      }
      if (a.Value.ToLower().Equals("no")) return false;
      if (a.Value.ToLower().Equals("yes")) return true;
      return null;
    }
    public static string NullSafeString(this XAttribute a, bool urlDecoded) {
      if (a == null) return String.Empty;
      return (urlDecoded) ? HttpUtility.UrlDecode(a.Value) : a.Value;
    }
    public static int NullSafeInt32(this string a) {
      if (String.IsNullOrEmpty(a)) return 0;
      int outValue;
      return Int32.TryParse(a, out outValue) ? outValue : 0;
    }
    public static int NullSafeInt32(this XAttribute a) {
      if (a == null) return 0;
      int outValue;
      return Int32.TryParse(a.Value, out outValue) ? outValue : 0;
    }
    public static decimal NullSafeDecimal(this XAttribute a) {
      if (a == null) return 0M;
      decimal outValue;
      return Decimal.TryParse(a.Value, out outValue) ? outValue : 0;
    }
    public static bool? NullSafeBool(this XAttribute a) {
      if (a == null) return null;
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
    /// <typeparam name="TA">The decorator attribute that determines the name</typeparam>
    /// <param name="func">A resolver function for the property.</param>
    /// <returns>The XML attributes name</returns>
    public static string GetAttributeName<T, TA>(Expression<Func<T, object>> func) 
      where T : class 
      where TA : Attribute {
      dynamic att = null;
      if (func.Body is MemberExpression) {
        att = ((MemberExpression)func.Body).Member.GetCustomAttributes(typeof(TA), false).First() as Attribute;
      }
      if (func.Body is UnaryExpression) {
        att = ((MemberExpression)(func.Body as UnaryExpression).Operand).Member.GetCustomAttributes(typeof(TA), false).First() as Attribute;
      }
      if (att != null && att is TA) {
        return att.Name;
      }
      return null;
    }

    public static string NullSafeString(this XElement e, XNamespace dc, string childName) {
      var o = e.Element((dc ?? e.GetNamespaceOfPrefix("dc")) + childName);
      return o == null ? String.Empty : o.Value;
    }

    /// <summary>
    /// Searches elements for an specific attribute and returns another attribute's value on first hit.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="selectAttribute"></param>
    /// <param name="dataAttribute"></param>
    /// <returns></returns>
    public static string ReadAttributeFromElement(this IEnumerable<XElement> element, string selectAttribute, string dataAttribute) {
      var result = element.FirstOrDefault(e => e.Attribute("name").Value == selectAttribute);
      if (result != null) {
        var data = result.Attribute(dataAttribute).Value;
        return data;
      }
      return String.Empty;
    }

  }
}