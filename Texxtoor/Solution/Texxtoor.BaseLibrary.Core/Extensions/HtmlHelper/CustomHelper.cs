using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web;
using System.Resources;
using Texxtoor.DataModels;

namespace Texxtoor.BaseLibrary.Core.Extensions {

  public static class CustomHelper {

    public static MvcHtmlString CheckboxListForEnum<T>(this HtmlHelper html, string name, T modelItems, bool lineBreak = false, int withName = -1, IDictionary<string, object> attr = null) where T : struct {
      var sb = new StringBuilder();
      var values = Enum.GetValues(typeof(T)).Cast<int>().ToList();
      var lastValue = Convert.ToInt64(values.Last());
      foreach (int item in values) {
        var builder = new TagBuilder("input");
        long flagValue = Convert.ToInt64(modelItems);

        if ((item & flagValue) == item)
          builder.MergeAttribute("checked", "checked");

        var id = Guid.NewGuid().ToString();
        builder.MergeAttribute("id", id);
        builder.MergeAttribute("type", "checkbox");
        builder.MergeAttribute("value", item.ToString());
        builder.MergeAttribute("name", name);
        if (attr != null) {
          foreach (var a in attr) {
            builder.MergeAttribute(a.Key, a.Value.ToString());
          }
        }
        var enumName = Enum.GetName(typeof(T), item) ?? "";
        if (withName >= 0) {
          builder.InnerHtml = String.Format(@"<label for=""{0}"">{1}</label>", id, enumName.Substring(0, Math.Min(enumName.Length - withName - 1, withName)));
        } else {
          builder.InnerHtml = String.Format(@"<label for=""{0}"">{1}</label>", id, enumName);
        }
        sb.Append(builder.ToString(TagRenderMode.Normal));
        if (lineBreak && item != lastValue) {
          sb.Append("<br />");
        }
      }

      return new MvcHtmlString(sb.ToString());
    }

    public static MvcHtmlString RadiobuttonListForEnum<T>(this HtmlHelper html, string name, T modelItem, bool lineBreak = false, int withName = -1, IDictionary<string, object> attr = null) where T : struct {
      var sb = new StringBuilder();
      var values = Enum.GetValues(typeof(T)).Cast<int>().ToList();
      var lastValue = Convert.ToInt64(values.Last());
      var itemValue = Convert.ToInt64(modelItem);
      foreach (int item in values) {
        var builder = new TagBuilder("input");

        if (itemValue == item)
          builder.MergeAttribute("checked", "checked");
        var id = Guid.NewGuid().ToString();
        builder.MergeAttribute("id", id);
        builder.MergeAttribute("type", "radio");
        builder.MergeAttribute("value", item.ToString());
        builder.MergeAttribute("name", name);
        if (attr != null) {
          foreach (var a in attr) {
            builder.MergeAttribute(a.Key, a.Value.ToString());
          }
        }
        var enumName = Enum.GetName(typeof (T), item) ?? "";
        if (withName >= 0) {
          builder.InnerHtml = String.Format(@"<label for=""{0}"">{1}</label>", id, enumName.Substring(0, Math.Min(enumName.Length - withName - 1, withName)));
        } else {
          builder.InnerHtml = String.Format(@"<label for=""{0}"">{1}</label>", id, enumName);
        }
        sb.Append(builder.ToString(TagRenderMode.Normal));
        if (lineBreak && item != lastValue) {
          sb.Append("<br />");
        }
      }

      return new MvcHtmlString(sb.ToString());
    }

    public static MvcHtmlString DropdownForEnum<T>(this HtmlHelper html, string name, T modelItem, IDictionary<string, object> attr = null, bool withDefault = false) where T : struct {
      return DropdownForEnum<T>(html, name, name, modelItem, attr, withDefault);
    }

    public static MvcHtmlString DropdownForEnum<T>(this HtmlHelper html, string name, string id, T modelItem, IDictionary<string, object> attr = null, bool withDefault = false) where T : struct {
      var sb = new StringBuilder();
      var values = Enum.GetValues(typeof(T));
      var itemValue = Convert.ToInt64(modelItem);
      var selectBuilder = new TagBuilder("select");
      selectBuilder.MergeAttribute("name", name);
      selectBuilder.MergeAttribute("id", id);
      if (attr != null) {
        foreach (var a in attr) {
          selectBuilder.MergeAttribute(a.Key, a.Value.ToString());
        }
      }
      // default field      
      if (withDefault) {
        var defaultAttr =
          typeof (T).GetCustomAttributes(typeof (DefaultValueAttribute), true)
                    .Cast<DefaultValueAttribute>()
                    .FirstOrDefault();
        if (defaultAttr != null) {
          var defaultField = new TagBuilder("option");
          var rm = new ResourceManager("Texxtoor.DataModels.ModelResources.ModelResources", typeof (ModelResources).Assembly);
          defaultField.InnerHtml = rm.GetString(defaultAttr.Value.ToString());
          if (itemValue < 0) {
            defaultField.MergeAttribute("selected", "selected");
            defaultField.MergeAttribute("value", "");
          }
          selectBuilder.InnerHtml += defaultField.ToString(TagRenderMode.Normal);
        }
      }
      // enum fields
      foreach (int item in values) {
        var builder = new TagBuilder("option");
        if (itemValue == item)
          builder.MergeAttribute("selected", "selected");
        builder.MergeAttribute("value", item.ToString());
        var da = typeof(T).GetField(Enum.GetName(typeof(T), item)).GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().FirstOrDefault();
        var text = da == null ? item.ToString() : da.GetName(); //da.ResourceType.GetProperty(da.Name).GetValue(null).ToString(); 
        builder.InnerHtml = text;
        selectBuilder.InnerHtml += builder.ToString(TagRenderMode.Normal);
      }
      sb.Append(selectBuilder.ToString(TagRenderMode.Normal));
      return new MvcHtmlString(sb.ToString());
    }

    public static MvcHtmlString CultureSelectionDropDown(this HtmlHelper html, string selected, string id = "cc", string name = "cc", string cssClass = "") {
      var sb = new StringBuilder();
      sb.AppendFormat(@"<select id=""{0}"" name=""{1}"" class=""{2}"">", id, name, cssClass);
      foreach (var c in WebConfigurationManager.AppSettings["ui:SupportedCultures"].Split(',').Select(item => new System.Globalization.CultureInfo(item))) {
        sb.AppendFormat(@"<option value=""{0}"" {2})>{1}</option>",
                        c.TwoLetterISOLanguageName,
                        c.NativeName,
                        c.TwoLetterISOLanguageName == selected ? "selected='selected'" : "");
      }
      sb.Append("</select>");
      return new MvcHtmlString(sb.ToString());
    }

  }
}
