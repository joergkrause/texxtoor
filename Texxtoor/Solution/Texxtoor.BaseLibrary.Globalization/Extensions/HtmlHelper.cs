using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Compilation;
using System.Globalization;
using System.Web.Handlers;
using System.Web.UI;
using Texxtoor.BaseLibrary.Globalization;
using System.Web.Script.Serialization;
using System.Web.Security;
using Texxtoor.BaseLibrary.Globalization.Provider;

namespace Texxtoor.BaseLibrary.Globalization.Extensions {

  public static class ResourceExtensions {

    # region handle model based views on MvcString level

    public static IHtmlString LocTag(this System.Web.Mvc.HtmlHelper htmlhelper, MvcHtmlString fallback) {
      var dh = new HtmlHelper<dynamic>(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, false, null, fallback.ToString(), null);
    }

    // to localize a tag, where only one part is replacable, we keep the tag as is and localize just the argument
    public static IHtmlString LocTag(this System.Web.Mvc.HtmlHelper htmlhelper, string expression, string tag, string arg) {
      var dh = new HtmlHelper<dynamic>(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternalForTag(dh, tag, expression, arg);
    }

    # endregion handle model based views on string level

    # region handle model based views on string level

    public static String LocStr(this System.Web.Mvc.HtmlHelper htmlhelper, string fallback) {
      var dh = new HtmlHelper<dynamic>(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, false, null, fallback, null).ToHtmlString();
    }

    public static String LocStr(this System.Web.Mvc.HtmlHelper htmlhelper, string expression, string fallback) {
      var dh = new HtmlHelper<dynamic>(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, false, expression, fallback, null).ToHtmlString();
    }

    # endregion handle model based views on string level

    # region handle model based views

    public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper htmlhelper, string fallback) {
      var dh = new HtmlHelper(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, true, null, fallback, null);
    }

    public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper htmlhelper, string expression, string fallback) {
      var dh = new HtmlHelper(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, true, expression, fallback, null);
    }

    public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper htmlhelper, string expression, string fallback, params object[] args) {
      var dh = new HtmlHelper(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, true, expression, fallback, args);
    }

    public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper htmlhelper, bool makeEditable, string expression, string fallback, params object[] args) {
      var dh = new HtmlHelper(htmlhelper.ViewContext, htmlhelper.ViewDataContainer);
      return LocInternal(dh, makeEditable, expression, fallback, args);
    }

    # endregion handle model based views

    # region handle model free views

    //public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper<dynamic> htmlhelper, string fallback) {
    //  return Loc(htmlhelper, true, null, fallback, null);
    //}

    //public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper<dynamic> htmlhelper, string expression, string fallback) {
    //  return Loc(htmlhelper, true, expression, fallback, null);
    //}

    //public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper<dynamic> htmlhelper, string expression, string fallback, params object[] args) {
    //  return LocInternal(htmlhelper, true, expression, fallback, args);
    //}

    //public static IHtmlString Loc(this System.Web.Mvc.HtmlHelper<dynamic> htmlhelper, bool makeEditable, string expression, string fallback, params object[] args) {
    //  return LocInternal(htmlhelper, makeEditable, expression, fallback, args);
    //}

    # endregion handle model free views

    private static IHtmlString LocInternal(System.Web.Mvc.HtmlHelper htmlhelper, bool makeEditable, string expression, string fallback, params object[] args) {
      // if the expression is null we use the fallback text to create a unique ID
      expression = expression ?? FormsAuthentication.HashPasswordForStoringInConfigFile(fallback, "MD5");
      // the path assigns all IDs of a view to one container (used for save here to support partial views)
      var virtualPath = GetVirtualPath(htmlhelper);
      var content = "";
      try {
        content = GetResourceString(htmlhelper.ViewContext.HttpContext, expression, virtualPath, fallback, args);
      } catch (Exception) {
      } 
      List<string> stringArgs = null;
      // cast to string whatever the model delivers
      if (args != null) {
        stringArgs = new List<string>();
        args.ToList().ForEach(arg => stringArgs.Add(arg == null ? "" : arg.ToString()));
      }
      if (HttpContext.Current.User.IsInRole("CmsAdmin")) {
        // make a unique id among all partial views on the same page 
        // okay, the most stupid way to embed a mini icon
        var editImg = ResourceManager.GetMvcResourceUrl(typeof(DbSimpleResourceProvider), "Texxtoor.BaseLibrary.Globalization.Resources.localize.gif");
        var saveImg = ResourceManager.GetMvcResourceUrl(typeof(DbSimpleResourceProvider), "Texxtoor.BaseLibrary.Globalization.Resources.save.gif");
        var uniqueId = expression + virtualPath.Substring(2).Replace("/", "").Replace(".cshtml", "");
        if (makeEditable) {
          // full editing for plain text
          content = String.Format(@"
                    <img src='/ThemeSwitcher/GetImage/{1}' onclick='{4}' title='Key = {3}' style='position:relative;top:-10px;left:20px;cursor:pointer' />
                    <img src='/ThemeSwitcher/GetImage/{2}' onclick='{5}' id='img_{7}' style='position:relative;top:-10px;left:0px;cursor:pointer;visibility:hidden' />
                    <span style='border:2px dotted #FFAAAA; background-color:#fffacd;'>                        
                        <span id='{7}' onclick='{4}' onblur='{5}'>{0}</span>
                    </span>".Trim()
              , content
              , editImg
              , saveImg
              , expression
              , String.Format(@"cmsAdmin.EditRes(""#{0}"", ""{1}"")", uniqueId, args == null ? String.Empty : String.Join("|", stringArgs.ToArray()))
              , String.Format(@"cmsAdmin.SaveRes(""{2}"", ""#{0}"", ""{3}"", ""{1}"")", uniqueId, args == null ? String.Empty : String.Join("|", stringArgs.ToArray()), virtualPath, expression)
              , CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
              , uniqueId
              );
        } else {
          // minimized text editing for action links or button content
          content = String.Format(@"{0}"
              , content
              , expression
              );
        }
      }
      return new MvcHtmlString(content);
    }

    private static IHtmlString LocInternalForTag(System.Web.Mvc.HtmlHelper htmlhelper, string tag, string expression, string fallback, params object[] args) {
      // if the expression is null we use the fallback text to create a unique ID
      expression = expression ?? FormsAuthentication.HashPasswordForStoringInConfigFile(fallback, "MD5");
      // the path assigns all IDs of a view to one container (used for save here to support partial views)
      var virtualPath = GetVirtualPath(htmlhelper);
      var content = GetResourceString(htmlhelper.ViewContext.HttpContext, expression, virtualPath, fallback, args);
      List<string> stringArgs = null;
      // cast to string whatever the model delivers
      if (args != null) {
        stringArgs = new List<string>();
        args.ToList().ForEach(arg => stringArgs.Add(arg.ToString()));
      }
      if (HttpContext.Current.User.IsInRole("CmsAdmin")) {
        // okay, the most stupid way to embed a mini icon
        var editImg = ResourceManager.GetMvcResourceUrl(typeof(DbSimpleResourceProvider), "Texxtoor.BaseLibrary.Globalization.Resources.tag.gif");
        var saveImg = ResourceManager.GetMvcResourceUrl(typeof(DbSimpleResourceProvider), "Texxtoor.BaseLibrary.Globalization.Resources.save.gif");
        // make a unique id among all partial views on the same page 
        var uniqueId = expression + virtualPath.Substring(2).Replace("/", "").Replace(".cshtml", "");
        // full editing for plain text
        content = String.Format(@"
                    <span style='background-color:#ffffff;font-size:6pt;top:-11px;left:7px;position:relative;z-index:1;'>'{3}'</span>
                    <span style='position:relative;left:-40px;'>{8}</span>
                    <img src='/ThemeSwitcher/GetImage/{1}' onclick='{4}' title='Key = {3}' style='position:relative;top:-10px;left:20px;cursor:pointer' />
                    <img src='/ThemeSwitcher/GetImage/{2}' onclick='{5}' id='img_{7}' style='position:relative;top:-10px;left:0px;cursor:pointer;visibility:hidden' />                    
                    <span style='background-color:#ffffff;font-size:6pt;top:-11px;left:7px;position:relative;z-index:1;'>Edit '{3}'</span>
                    <span style='border:2px solid #FFEEEE; background-color:#eeeeee;position:relative;left:-50px;'>
                        <span id='{7}' onclick='{4}' onblur='{5}'>{0}</span>
                    </span>".Trim()
            , content
            , editImg
            , saveImg
            , expression
            , String.Format(@"cmsAdmin.EditRes(""#{0}"", ""{1}"")", uniqueId, args == null ? String.Empty : String.Join("|", stringArgs.ToArray()))
            , String.Format(@"cmsAdmin.SaveRes(""{2}"", ""#{0}"", ""{3}"", ""{1}"")", uniqueId, args == null ? String.Empty : String.Join("|", stringArgs.ToArray()), virtualPath, expression)
            , CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            , uniqueId,
            String.Format(tag, content)
            );
      } else {
        content = String.Format(tag, content);
      }
      return new MvcHtmlString(content);
    }

    private static string GetResourceString(HttpContextBase httpContext, string expression, string virtualPath, string fallback, object[] args) {
      var context = new ExpressionBuilderContext(virtualPath);
      var builder = new ResourceExpressionBuilder();
      ResourceExpressionFields fields = null;
      try {
        fields = (ResourceExpressionFields)builder.ParseExpression(expression, typeof(string), context);
      } catch {
        // assuming the resource isn't in the db already, so putting it in...a rare case
        DbResourceDataManager.UpdateOrAdd(expression, fallback, CultureInfo.InvariantCulture, virtualPath);
      }
      // No such key found, use fallback text
      if (fields == null) {
        if (args == null || args.Length == 0) {
          return fallback;
        }
        return String.Format(fallback, args);
      }
      // found in global table
      if (!String.IsNullOrEmpty(fields.ClassKey))
        return String.Format((string)httpContext.GetGlobalResourceObject(
            fields.ClassKey,
            fields.ResourceKey,
            CultureInfo.CurrentUICulture),
            args);
      // found in local table
      if (args == null || args.Length == 0) {
        return (string)httpContext.GetLocalResourceObject(
          virtualPath,
          fields.ResourceKey,
          CultureInfo.CurrentUICulture);
      }
      return String.Format((string)httpContext.GetLocalResourceObject(
          virtualPath,
          fields.ResourceKey,
          CultureInfo.CurrentUICulture),
          args);
    }

    private static string GetVirtualPath(HtmlHelper htmlhelper) {
      string virtualPath = null;
      var controller = htmlhelper.ViewContext.Controller as Controller;

      if (controller != null) {
        var webFormView = htmlhelper.ViewContext.View as RazorView;

        if (webFormView != null) {
          virtualPath = webFormView.ViewPath;
        }
      }

      return virtualPath;
    }
  }
}
