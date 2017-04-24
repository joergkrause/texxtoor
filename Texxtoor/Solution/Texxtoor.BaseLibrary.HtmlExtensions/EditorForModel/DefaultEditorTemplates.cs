using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI.WebControls;
using Texxtoor.Portal.Core.Extensions.Attributes;

namespace Texxtoor.Portal.Core.Extensions.EditorForModel {
  internal static class DefaultEditorTemplates {
    internal static string BooleanTemplate(HtmlHelper html) {
      var nullable1 = new bool?();
      if (html.ViewContext.ViewData.Model != null)
        nullable1 = new bool?(Convert.ToBoolean(html.ViewContext.ViewData.Model, CultureInfo.InvariantCulture));
      if (html.ViewContext.ViewData.ModelMetadata.IsNullableValueType)
        return BooleanTemplateDropDownList(html, nullable1);
      var html1 = html;
      var nullable2 = nullable1;
      var num = nullable2.HasValue ? (nullable2.GetValueOrDefault() ? 1 : 0) : 0;
      return BooleanTemplateCheckbox(html1, num != 0);
    }

    private static string BooleanTemplateCheckbox(HtmlHelper html, bool value) {
      return html.CheckBox(string.Empty, value, CreateHtmlAttributes("check-box", (string)null)).ToHtmlString();
    }

    private static string BooleanTemplateDropDownList(HtmlHelper html, bool? value) {
      return html.DropDownList(string.Empty, TriStateValues(value), CreateHtmlAttributes("list-box tri-state", (string)null)).ToHtmlString();
    }

    internal static string CollectionTemplate(HtmlHelper html) {
      return CollectionTemplate(html, AngularTemplateHelpers.TemplateHelper);
    }

    internal static string CollectionTemplate(HtmlHelper html, AngularTemplateHelpers.TemplateHelperDelegate templateHelper) {
      var model = html.ViewContext.ViewData.ModelMetadata.Model;
      if (model == null)
        return string.Empty;
      var enumerable = model as IEnumerable;
      if (enumerable == null) {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Templates_TypeMustImplementIEnumerable", new object[1]
        {
          model.GetType().FullName
        }));
      } else {
        var type = typeof(string);
        var genericInterface = TypeHelpers.ExtractGenericInterface(enumerable.GetType(), typeof(IEnumerable<>));
        if (genericInterface != null)
          type = genericInterface.GetGenericArguments()[0];
        var flag = TypeHelpers.IsNullableValueType(type);
        var htmlFieldPrefix = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        try {
          html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;
          var str1 = htmlFieldPrefix;
          var stringBuilder = new StringBuilder();
          var num = 0;
          var enumerator = enumerable.GetEnumerator();
          try {
            while (enumerator.MoveNext()) {
              var item = enumerator.Current;
              var modelType = type;
              if (item != null && !flag)
                modelType = item.GetType();
              var metadataForType = ModelMetadataProviders.Current.GetMetadataForType(() => item, modelType);
              var htmlFieldName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", new object[2]
              {
                str1,
                num++
              });
              var str2 = templateHelper(html, metadataForType, htmlFieldName, null, DataBoundControlMode.Edit, null);
              stringBuilder.Append(str2);
            }
          } finally {
            var disposable = enumerator as IDisposable;
            if (disposable != null)
              disposable.Dispose();
          }
          return stringBuilder.ToString();
        } finally {
          html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
        }
      }
    }

    internal static string DecimalTemplate(HtmlHelper html) {
      if (html.ViewContext.ViewData.TemplateInfo.FormattedModelValue == html.ViewContext.ViewData.ModelMetadata.Model)
        html.ViewContext.ViewData.TemplateInfo.FormattedModelValue = string.Format(CultureInfo.CurrentCulture, "{0:0.00}", new object[1]
        {
          html.ViewContext.ViewData.ModelMetadata.Model
        });
      return StringTemplate(html);
    }

    internal static string HiddenInputTemplate(HtmlHelper html) {
      var str = !html.ViewContext.ViewData.ModelMetadata.HideSurroundingHtml ? DefaultDisplayTemplates.StringTemplate(html) : string.Empty;
      var obj = html.ViewContext.ViewData.Model;
      var binary = obj as Binary;
      if (binary != null) {
        obj = Convert.ToBase64String(binary.ToArray());
      } else {
        var inArray = obj as byte[];
        if (inArray != null)
          obj = Convert.ToBase64String(inArray);
      }
      return str + html.Hidden(string.Empty, obj).ToHtmlString();
    }

    internal static string MultilineTextTemplate(HtmlHelper html) {
      var placeholder = GetPlaceHolder(html.ViewContext.ViewData.ModelMetadata);
      return html.TextArea(string.Empty, html.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString(), 0, 0, CreateHtmlAttributes("", placeHolder: placeholder)).ToHtmlString();
    }

    private static IDictionary<string, object> CreateHtmlAttributes(string className, string placeHolder = null, string inputType = null) {
      var dictionary = new Dictionary<string, object>() {{ "class", className }};
      if (inputType != null)
        dictionary.Add("type", inputType);
      if (placeHolder != null)
        dictionary.Add("placeholder", placeHolder);
      return dictionary;
    }

    private static void CreateAngularAttributes(IDictionary<string, object> attributes, ModelMetadata metaData) {
      var prefix = "vm";
      var modelTypeName = "";
      var propertyName = "";
      GetMetadataNames(metaData, ref prefix, ref modelTypeName, ref propertyName);
      var angularPropAttribute = metaData.ContainerType.GetProperty(propertyName).GetCustomAttributes(typeof(NgFieldAttribute), true).Cast<NgFieldAttribute>().SingleOrDefault();
      if (angularPropAttribute != null) {
        propertyName = angularPropAttribute.ApplyCaseNotion(propertyName, angularPropAttribute.Case);
        if (angularPropAttribute.NgModel != null) {
          propertyName = angularPropAttribute.NgModel;
        }
        if (angularPropAttribute.NgOnBlur != null) {
          attributes.Add("data-on-blur", angularPropAttribute.NgOnBlur);
        } 
        if (angularPropAttribute.NgOnEnter != null) {
          attributes.Add("data-on-enter", angularPropAttribute.NgOnBlur);
        }
        if (angularPropAttribute.NgOnChange != null) {
          attributes.Add("data-ng-change", angularPropAttribute.NgOnChange);
        }
        if (angularPropAttribute.NgOnClick != null) {
          attributes.Add("data-ng-click", angularPropAttribute.NgOnClick);
        }
        if (angularPropAttribute.NgDisabled != null) {
          attributes.Add("data-ng-disabled", angularPropAttribute.NgDisabled);
        }
      }
      // NgModel 
      attributes.Add("data-ng-model", String.Format("{0}{3}{1}.{2}", prefix, modelTypeName, propertyName, prefix != null ? "." : ""));
    }

    private static void GetMetadataNames(ModelMetadata metaData, ref string prefix, ref string modelTypeName, ref string propertyName) {
      prefix = "vm";
      modelTypeName = metaData.ContainerType.Name;
      propertyName = metaData.PropertyName;
      // check for specific Angular settings
      var angularTypeAttribute = metaData.ContainerType.GetCustomAttributes(typeof(AngularAttribute), true).Cast<AngularAttribute>().SingleOrDefault();
      if (angularTypeAttribute != null) {
        if (angularTypeAttribute.ViewModelPrefix != null) {
          prefix = angularTypeAttribute.ViewModelPrefix;
        }
        if (angularTypeAttribute.ContainerName != null) {
          modelTypeName = angularTypeAttribute.ContainerName;
        }
      }
    }

    internal static string GetPlaceHolder(ModelMetadata metaData) {
      return metaData.Watermark;
    }

    internal static string ObjectTemplate(HtmlHelper html) {
      return ObjectTemplate(html, AngularTemplateHelpers.TemplateHelper);
    }

    internal static string ObjectTemplate(HtmlHelper html, AngularTemplateHelpers.TemplateHelperDelegate templateHelper) {
      var viewData = html.ViewContext.ViewData;
      var templateInfo = viewData.TemplateInfo;
      var modelMetadata = viewData.ModelMetadata;
      var stringBuilder = new StringBuilder();
      if (templateInfo.TemplateDepth > 1) {
        if (modelMetadata.Model != null)
          return modelMetadata.SimpleDisplayText;
        else
          return modelMetadata.NullDisplayText;
      }
      // basic template for a form field that is aware of Bootstrap styles and angular form settings 
      foreach (var metadata in modelMetadata.Properties.Where(pm => ShouldShow(pm, templateInfo))) {
        if (!metadata.HideSurroundingHtml) {
          // bootstrap form-group
          stringBuilder.Append("<div class=\"form-group\">");
          // Label text
          var labeltext = LabelHelperInternal(html, metadata, metadata.PropertyName, null, new Dictionary<string, object>() {{ "class", "col-md-2 control-label" }}).ToHtmlString();
          if (!string.IsNullOrEmpty(labeltext))
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", labeltext);
          stringBuilder.Append("<div class=\"col-md-10\">");
        }
        // Core element
        stringBuilder.Append(templateHelper(html, metadata, metadata.PropertyName, null, DataBoundControlMode.Edit, null));
        // End surrounding
        if (!metadata.HideSurroundingHtml) {
          stringBuilder.Append("</div>\r\n");
          // Bootstrap style help block with private extension
          stringBuilder.AppendFormat("<span class=\"help-block-dynamic\">{0}</span>", metadata.Description);
          stringBuilder.Append(" ");
          stringBuilder.Append("<div class=\"col-md-offset-2\">");
          stringBuilder.Append(html.ValidationMessage(metadata.PropertyName));
          stringBuilder.Append("</div>\r\n");
          stringBuilder.Append("</div>\r\n");
        }
      }
      return stringBuilder.ToString();
    }

    private static MvcHtmlString LabelHelperInternal(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string labelText = null, IDictionary<string, object> htmlAttributes = null)
    {
      var str = labelText;
      if (str == null)
      {
        var displayName = metadata.DisplayName;
        if (displayName == null)
        {
          var propertyName = metadata.PropertyName;
          if (propertyName == null)
            str = htmlFieldName.Split('.').Last();
          else
            str = propertyName;
        }
        else
          str = displayName;
      }
      var innerText = str;
      if (string.IsNullOrEmpty(innerText))
        return MvcHtmlString.Empty;
      var tagBuilder1 = new TagBuilder("label");
      tagBuilder1.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
      tagBuilder1.SetInnerText(innerText);
      var tagBuilder2 = tagBuilder1;
      var flag = true;
      var attributes = htmlAttributes;
      var num = flag ? 1 : 0;
      tagBuilder2.MergeAttributes<string, object>(attributes, num != 0);
      return new MvcHtmlString(tagBuilder1.ToString(TagRenderMode.Normal));
    }  

    internal static string PasswordTemplate(HtmlHelper html) {
      var placeholder = GetPlaceHolder(html.ViewContext.ViewData.ModelMetadata);
      return html.Password(string.Empty, html.ViewContext.ViewData.TemplateInfo.FormattedModelValue, CreateHtmlAttributes("", placeHolder: placeholder, inputType: "password")).ToHtmlString();
    }

    private static bool ShouldShow(ModelMetadata metadata, TemplateInfo templateInfo) {
      if (metadata.ShowForEdit /*&& metadata.ModelType != typeof(EntityState)*/ && !metadata.IsComplexType)
        return !templateInfo.Visited(metadata);
      else
        return false;
    }

    internal static string StringTemplate(HtmlHelper html) {
      return HtmlInputTemplateHelper(html);
    }

    internal static string PhoneNumberInputTemplate(HtmlHelper html) {
      var inputType = "tel";
      return HtmlInputTemplateHelper(html, inputType);
    }

    internal static string UrlInputTemplate(HtmlHelper html) {
      var inputType = "url";
      return HtmlInputTemplateHelper(html, inputType);
    }

    internal static string EmailAddressInputTemplate(HtmlHelper html) {
      var inputType = "email";
      return HtmlInputTemplateHelper(html, inputType);
    }

    internal static string DateTimeInputTemplate(HtmlHelper html) {
      var inputType = "datetime";
      return HtmlInputTemplateHelper(html, inputType);
    }

    internal static string DateInputTemplate(HtmlHelper html) {
      var inputType = "date";
      return HtmlInputTemplateHelper(html, inputType);
    }

    internal static string TimeInputTemplate(HtmlHelper html) {
      var inputType = "time";
      return HtmlInputTemplateHelper(html, inputType);
    }

    internal static string NumberInputTemplate(HtmlHelper html) {
      var inputType = "number";
      return HtmlInputTemplateHelper(html, inputType);
    }

    private static string HtmlInputTemplateHelper(HtmlHelper html, string inputType = null) {
      var name = string.Empty;
      var modelMetadata = html.ViewContext.ViewData.ModelMetadata;
      var placeholder = GetPlaceHolder(modelMetadata);
      var formattedModelValue = html.ViewContext.ViewData.TemplateInfo.FormattedModelValue;
      // default CSS is bootstrap aware
      var htmlAttributes = CreateHtmlAttributes("form-control", placeHolder: placeholder, inputType: inputType);
      CreateAngularAttributes(htmlAttributes, modelMetadata);
      return html.TextBox(name, formattedModelValue, htmlAttributes).ToHtmlString();
    }

    internal static List<SelectListItem> TriStateValues(bool? value) {
      return new List<SelectListItem>()
      {
        new SelectListItem()
        {
          Text = "Common_TriState_NotSet",
          Value = string.Empty,
          Selected = !value.HasValue
        },
        new SelectListItem()
        {
          Text = "Common_TriState_True",
          Value = "true",
          Selected = value.HasValue && value.Value
        },
        new SelectListItem()
        {
          Text = "Common_TriState_False",
          Value = "false",
          Selected = value.HasValue && !value.Value
        }
      };
    }
  }
}
