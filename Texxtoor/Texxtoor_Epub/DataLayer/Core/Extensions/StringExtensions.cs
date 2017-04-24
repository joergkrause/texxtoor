using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.Web.Mvc;

namespace Texxtoor.Editor.Core.Extensions {

  public static class StringExtension {

    [DebuggerStepThrough]
    public static string InnerTrim(this string text) {
      return text.Replace(" ", "");
    }

    [DebuggerStepThrough]
    public static string NullSafe(this string target) {
      return (target ?? string.Empty).Trim();
    }

    [DebuggerStepThrough]
    public static string FormatWith(this string target, params object[] args) {
      return string.Format(CultureInfo.CurrentCulture, target, args);
    }

    [DebuggerStepThrough]
    public static MvcHtmlString Ellipsis(this string value, int maxLength) {
      return Ellipsis(value, maxLength, null);
    }

    /// <summary>
    /// Shorten string, trim, and strip tags
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string CleanUpString(this string value, int maxLength) {
      return Ellipsis(value, maxLength, null).ToHtmlString().Trim().StripTags();
    }

    [DebuggerStepThrough]
    public static byte[] GetBytes(this string value) {
      return System.Text.Encoding.UTF8.GetBytes(value);
    }

    [DebuggerStepThrough]
    public static MvcHtmlString Ellipsis(this string value, int maxLength, string actionLink) {
      const string suffix = " ...";
      const string boundaryChars = " .,;";
      if (value == null)
        return null;
      string s = value.Replace("<![CDATA[", "").Replace("]]>", "");
      if (string.IsNullOrEmpty(s) || s.Length <= maxLength || s.Length < suffix.Length)
        return MvcHtmlString.Create(s);
      // try breaking at word boundary back from last character
      var sub = s.Substring(0, maxLength - suffix.Length);
      var idx = sub.Length;
      do { } while (!boundaryChars.Contains(s[--idx]) && idx > 0);
      return MvcHtmlString.Create(s.Substring(0, ++idx) + (String.IsNullOrEmpty(actionLink) ? suffix : actionLink));
    }

    [DebuggerStepThrough]
    public static void GetIntPair(this string res, out int w, out int h) {
      if (!String.IsNullOrEmpty(res) && res.Contains("x")) {
        w = Int32.Parse(res.Split('x')[0]);
        h = Int32.Parse(res.Split('x')[1]);
      } else {
        w = 50;
        h = 80;
      }
    }

    [DebuggerStepThrough]
    public static string StripTags(this string source) {
      char[] array = new char[source.Length];
      int arrayIndex = 0;
      bool inside = false;

      for (int i = 0; i < source.Length; i++) {
        char let = source[i];
        if (let == '<') {
          inside = true;
          continue;
        }
        if (let == '>') {
          inside = false;
          continue;
        }
        if (!inside) {
          array[arrayIndex] = let;
          arrayIndex++;
        }
      }
      return new string(array, 0, arrayIndex);
    }

  }


}
