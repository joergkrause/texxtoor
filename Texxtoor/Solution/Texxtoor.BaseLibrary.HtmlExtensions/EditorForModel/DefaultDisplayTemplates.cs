using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI.WebControls;

namespace Texxtoor.Portal.Core.Extensions.EditorForModel {
  internal static class DefaultDisplayTemplates {
    internal static string BooleanTemplate(HtmlHelper html) {
      var nullable1 = new bool?();
      if (html.ViewContext.ViewData.Model != null)
        nullable1 = new bool?(Convert.ToBoolean(html.ViewContext.ViewData.Model, (IFormatProvider)CultureInfo.InvariantCulture));
      if (html.ViewContext.ViewData.ModelMetadata.IsNullableValueType)
        return BooleanTemplateDropDownList(nullable1);
      var nullable2 = nullable1;
      return BooleanTemplateCheckbox(nullable2.HasValue && nullable2.GetValueOrDefault());
    }

    private static string BooleanTemplateCheckbox(bool value) {
      var tagBuilder = new TagBuilder("input");
      tagBuilder.AddCssClass("check-box");
      tagBuilder.Attributes["disabled"] = "disabled";
      tagBuilder.Attributes["type"] = "checkbox";
      if (value)
        tagBuilder.Attributes["checked"] = "checked";
      return tagBuilder.ToString(TagRenderMode.SelfClosing);
    }

    private static string BooleanTemplateDropDownList(bool? value) {
      var stringBuilder = new StringBuilder();
      var tagBuilder = new TagBuilder("select");
      tagBuilder.AddCssClass("list-box");
      tagBuilder.AddCssClass("tri-state");
      tagBuilder.Attributes["disabled"] = "disabled";
      stringBuilder.Append(tagBuilder.ToString(TagRenderMode.StartTag));
      foreach (var selectListItem in DefaultEditorTemplates.TriStateValues(value))
        stringBuilder.Append(ListItemToOption(selectListItem));
      stringBuilder.Append(tagBuilder.ToString(TagRenderMode.EndTag));
      return ((object)stringBuilder).ToString();
    }

    private static string ListItemToOption(SelectListItem item) {
      var tagBuilder = new TagBuilder("option") {
        InnerHtml = HttpUtility.HtmlEncode(item.Text)
      };
      if (item.Value != null)
        tagBuilder.Attributes["value"] = item.Value;
      if (item.Selected)
        tagBuilder.Attributes["selected"] = "selected";
      return tagBuilder.ToString(TagRenderMode.Normal);
    }

    internal static string CollectionTemplate(HtmlHelper html) {
      return CollectionTemplate(html, new AngularTemplateHelpers.TemplateHelperDelegate(AngularTemplateHelpers.TemplateHelper));
    }

    internal static string CollectionTemplate(HtmlHelper html, AngularTemplateHelpers.TemplateHelperDelegate templateHelper) {
      var model = html.ViewContext.ViewData.ModelMetadata.Model;
      if (model == null)
        return String.Empty;
      var enumerable = model as IEnumerable;
      if (enumerable == null) {
        throw new InvalidOperationException(String.Format((IFormatProvider)CultureInfo.CurrentCulture, "Templates_TypeMustImplementIEnumerable {0}", model.GetType().FullName));
      } else {
        var type = typeof(string);
        var genericInterface = TypeHelpers.ExtractGenericInterface(enumerable.GetType(), typeof(IEnumerable<>));
        if (genericInterface != (Type)null)
          type = genericInterface.GetGenericArguments()[0];
        var flag = TypeHelpers.IsNullableValueType(type);
        var htmlFieldPrefix = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        try {
          html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = String.Empty;
          var str1 = htmlFieldPrefix;
          var stringBuilder = new StringBuilder();
          var num = 0;          
          try {
            foreach (var item in enumerable) {
              var modelType = type;
              if (item != null && !flag)
                modelType = item.GetType();
              var metadataForType = ModelMetadataProviders.Current.GetMetadataForType((Func<object>)(() => item), modelType);
              var htmlFieldName = String.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}[{1}]", str1, num++);
              var str2 = templateHelper(html, metadataForType, htmlFieldName, null, DataBoundControlMode.ReadOnly, null);
              stringBuilder.Append(str2);
            }
          } finally {
          }
          return stringBuilder.ToString();
        } finally {
          html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
        }
      }
    }

    internal static string DecimalTemplate(HtmlHelper html) {
      if (html.ViewContext.ViewData.TemplateInfo.FormattedModelValue == html.ViewContext.ViewData.ModelMetadata.Model)
        html.ViewContext.ViewData.TemplateInfo.FormattedModelValue = (object)string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0:0.00}", new object[1]
        {
          html.ViewContext.ViewData.ModelMetadata.Model
        });
      return StringTemplate(html);
    }

    internal static string EmailAddressTemplate(HtmlHelper html) {
      return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "<a href=\"mailto:{0}\">{1}</a>", 
        html.AttributeEncode(html.ViewContext.ViewData.Model),
        html.Encode(html.ViewContext.ViewData.TemplateInfo.FormattedModelValue)
      );
    }

    internal static string HiddenInputTemplate(HtmlHelper html) {
      if (html.ViewContext.ViewData.ModelMetadata.HideSurroundingHtml)
        return string.Empty;
      else
        return StringTemplate(html);
    }

    internal static string HtmlTemplate(HtmlHelper html) {
      return html.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString();
    }

    internal static string ObjectTemplate(HtmlHelper html) {
      return ObjectTemplate(html, new AngularTemplateHelpers.TemplateHelperDelegate(AngularTemplateHelpers.TemplateHelper));
    }

    internal static string ObjectTemplate(HtmlHelper html, AngularTemplateHelpers.TemplateHelperDelegate templateHelper) {
      var viewData = html.ViewContext.ViewData;
      var templateInfo = viewData.TemplateInfo;
      var modelMetadata = viewData.ModelMetadata;
      var stringBuilder = new StringBuilder();
      if (modelMetadata.Model == null)
        return modelMetadata.NullDisplayText;
      if (templateInfo.TemplateDepth > 1)
        return modelMetadata.SimpleDisplayText;
      foreach (var metadata in modelMetadata.Properties.Where((Func<ModelMetadata, bool>)(pm => ShouldShow(pm, templateInfo)))) {
        if (!metadata.HideSurroundingHtml) {
          // follow the bootstrap CSS
          stringBuilder.Append("<div class=\"form-group\">");
          var displayName = metadata.GetDisplayName();
          if (!string.IsNullOrEmpty(displayName)) {
            // follow the bootstrap CSS
            stringBuilder.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture, "<label class=\"form-group\">{0}</label>", new object[1]
            {
              (object) displayName
            });
            stringBuilder.AppendLine();
          }          
        }
        stringBuilder.Append(templateHelper(html, metadata, metadata.PropertyName, (string)null, DataBoundControlMode.ReadOnly, (object)null));
        if (!metadata.HideSurroundingHtml)
          stringBuilder.AppendLine("</div>");
      }
      return stringBuilder.ToString();
    }

    private static bool ShouldShow(ModelMetadata metadata, TemplateInfo templateInfo) {
      if (metadata.ShowForDisplay /*&& metadata.ModelType != typeof(EntityState)*/ && !metadata.IsComplexType)
        return !templateInfo.Visited(metadata);
      else
        return false;
    }

    internal static string StringTemplate(HtmlHelper html) {
      return html.Encode(html.ViewContext.ViewData.TemplateInfo.FormattedModelValue);
    }

    internal static string UrlTemplate(HtmlHelper html) {
      return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "<a href=\"{0}\">{1}</a>", new object[2]
      {
        (object) html.AttributeEncode(html.ViewContext.ViewData.Model),
        (object) html.Encode(html.ViewContext.ViewData.TemplateInfo.FormattedModelValue)
      });
    }
  }

}