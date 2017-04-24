using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Texxtoor.Portal.Core.Extensions.Attributes;

namespace Texxtoor.Portal.Core.Extensions.EditorForModel {
  internal static class AngularTemplateHelpers {
    private static readonly Dictionary<DataBoundControlMode, string> _modeViewPaths = new Dictionary<DataBoundControlMode, string>()
    {
      {
        DataBoundControlMode.ReadOnly,
        "DisplayTemplates"
      },
      {
        DataBoundControlMode.Edit,
        "EditorTemplates"
      }
    };
    private static readonly Dictionary<string, Func<HtmlHelper, string>> _defaultDisplayActions = new Dictionary<string, Func<HtmlHelper, string>>(StringComparer.OrdinalIgnoreCase)
    {
      {
        "EmailAddress",
        DefaultDisplayTemplates.EmailAddressTemplate
      },
      {
        "HiddenInput",
        DefaultDisplayTemplates.HiddenInputTemplate
      },
      {
        "Html",
        DefaultDisplayTemplates.HtmlTemplate
      },
      {
        "Text",
        DefaultDisplayTemplates.StringTemplate
      },
      {
        "Url",
        DefaultDisplayTemplates.UrlTemplate
      },
      {
        "Collection",
        DefaultDisplayTemplates.CollectionTemplate
      },
      {
        typeof (bool).Name,
        DefaultDisplayTemplates.BooleanTemplate
      },
      {
        typeof (Decimal).Name,
        DefaultDisplayTemplates.DecimalTemplate
      },
      {
        typeof (string).Name,
        DefaultDisplayTemplates.StringTemplate
      },
      {
        typeof (object).Name,
        DefaultDisplayTemplates.ObjectTemplate
      }
    };
    private static readonly Dictionary<string, Func<HtmlHelper, string>> _defaultEditorActions = new Dictionary<string, Func<HtmlHelper, string>>(StringComparer.OrdinalIgnoreCase)
    {
      {
        "HiddenInput",
        DefaultEditorTemplates.HiddenInputTemplate
      },
      {
        "MultilineText",
        DefaultEditorTemplates.MultilineTextTemplate
      },
      {
        "Password",
        DefaultEditorTemplates.PasswordTemplate
      },
      {
        "Text",
        DefaultEditorTemplates.StringTemplate
      },
      {
        "Collection",
        DefaultEditorTemplates.CollectionTemplate
      },
      {
        "PhoneNumber",
        DefaultEditorTemplates.PhoneNumberInputTemplate
      },
      {
        "Url",
        DefaultEditorTemplates.UrlInputTemplate
      },
      {
        "EmailAddress",
        DefaultEditorTemplates.EmailAddressInputTemplate
      },
      {
        "DateTime",
        DefaultEditorTemplates.DateTimeInputTemplate
      },
      {
        "Date",
        DefaultEditorTemplates.DateInputTemplate
      },
      {
        "Time",
        DefaultEditorTemplates.TimeInputTemplate
      },
      {
        typeof (byte).Name,
        DefaultEditorTemplates.NumberInputTemplate
      },
      {
        typeof (sbyte).Name,
        DefaultEditorTemplates.NumberInputTemplate
      },
      {
        typeof (int).Name,
        DefaultEditorTemplates.NumberInputTemplate
      },
      {
        typeof (uint).Name,
        DefaultEditorTemplates.NumberInputTemplate
      },
      {
        typeof (long).Name,
        DefaultEditorTemplates.NumberInputTemplate
      },
      {
        typeof (ulong).Name,
        DefaultEditorTemplates.NumberInputTemplate
      },
      {
        typeof (bool).Name,
        DefaultEditorTemplates.BooleanTemplate
      },
      {
        typeof (Decimal).Name,
        DefaultEditorTemplates.DecimalTemplate
      },
      {
        typeof (string).Name,
        DefaultEditorTemplates.StringTemplate
      },
      {
        typeof (object).Name,
        DefaultEditorTemplates.ObjectTemplate
      }
    };
    internal static string CacheItemId = Guid.NewGuid().ToString();

    static AngularTemplateHelpers() {
    }

    internal static string ExecuteTemplate(HtmlHelper html, ViewDataDictionary viewData, string templateName, DataBoundControlMode mode, GetViewNamesDelegate getViewNames, GetDefaultActionsDelegate getDefaultActions) {
      var actionCache = GetActionCache(html);
      var dictionary = getDefaultActions(mode);
      var str = _modeViewPaths[mode];
      foreach (var key in getViewNames(viewData.ModelMetadata, new string[3]
      {
        templateName,
        viewData.ModelMetadata.TemplateHint,
        viewData.ModelMetadata.DataTypeName
      })) {
        var index = str + "/" + key;
        ActionCacheItem actionCacheItem;
        if (actionCache.TryGetValue(index, out actionCacheItem)) {
          if (actionCacheItem != null)
            return actionCacheItem.Execute(html, viewData);
        } else {
          // we must suppress global templates, because those from other areas may not know Angular
          // so, first we look for angular attribute, but we let them pass if an UIHint was attached
          var isAngular = false;
          if (viewData.ModelMetadata.ContainerType != null) {
            if (viewData.ModelMetadata.PropertyName != null) {
              isAngular = viewData.ModelMetadata.ContainerType.GetProperty(viewData.ModelMetadata.PropertyName).GetCustomAttributes(true).OfType<NgFieldAttribute>().Any();
            }
          }
          if (!isAngular) {
            var partialView = ViewEngines.Engines.FindPartialView(html.ViewContext, index);
            if (partialView.View != null) {
              actionCache[index] = new ActionCacheViewItem() {
                ViewName = index
              };
              using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture)) {
                partialView.View.Render(new ViewContext(html.ViewContext, partialView.View, viewData, html.ViewContext.TempData, stringWriter), stringWriter);
                return stringWriter.ToString();
              }
            }
          }
          Func<HtmlHelper, string> func;
          if (dictionary.TryGetValue(key, out func)) {
            actionCache[index] = new ActionCacheCodeItem() {
              Action = func
            };
            return func(MakeHtmlHelper(html, viewData));
          }
          actionCache[index] = null;
        }
      }
      throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "TemplateHelpers_NoTemplate", new object[1]
      {
        viewData.ModelMetadata.ModelType.FullName
      }));
    }

    internal static Dictionary<string, ActionCacheItem> GetActionCache(HtmlHelper html) {
      var httpContext = html.ViewContext.HttpContext;
      Dictionary<string, ActionCacheItem> dictionary;
      if (!httpContext.Items.Contains(CacheItemId)) {
        dictionary = new Dictionary<string, ActionCacheItem>();
        httpContext.Items[CacheItemId] = dictionary;
      } else
        dictionary = (Dictionary<string, ActionCacheItem>)httpContext.Items[CacheItemId];
      return dictionary;
    }

    internal static Dictionary<string, Func<HtmlHelper, string>> GetDefaultActions(DataBoundControlMode mode) {
      if (mode != DataBoundControlMode.ReadOnly)
        return _defaultEditorActions;
      return _defaultDisplayActions;
    }

    internal static IEnumerable<string> GetViewNames(ModelMetadata metadata, params string[] templateHints) {
      foreach (var str in templateHints.Where(s => !string.IsNullOrEmpty(s)))
        yield return str;
      var fieldType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
      yield return fieldType.Name;
      if (!metadata.IsComplexType)
        yield return "String";
      else if (fieldType.IsInterface) {
        if (typeof(IEnumerable).IsAssignableFrom(fieldType))
          yield return "Collection";
        yield return "Object";
      } else {
        var isEnumerable = typeof(IEnumerable).IsAssignableFrom(fieldType);
        while (true) {
          fieldType = fieldType.BaseType;
          if (!(fieldType == null)) {
            if (isEnumerable && fieldType == typeof(object))
              yield return "Collection";
            yield return fieldType.Name;
          } else
            break;
        }
      }
    }

    internal static MvcHtmlString Template(HtmlHelper html, string expression, string templateName, string htmlFieldName, DataBoundControlMode mode, object additionalViewData) {
      return MvcHtmlString.Create(Template(html, expression, templateName, htmlFieldName, mode, additionalViewData, TemplateHelper));
    }

    internal static string Template(HtmlHelper html, string expression, string templateName, string htmlFieldName, DataBoundControlMode mode, object additionalViewData, TemplateHelperDelegate templateHelper) {
      return templateHelper(html, ModelMetadata.FromStringExpression(expression, html.ViewData), htmlFieldName ?? ExpressionHelper.GetExpressionText(expression), templateName, mode, additionalViewData);
    }

    internal static MvcHtmlString TemplateFor<TContainer, TValue>(this HtmlHelper<TContainer> html, Expression<Func<TContainer, TValue>> expression, string templateName, string htmlFieldName, DataBoundControlMode mode, object additionalViewData) {
      return MvcHtmlString.Create(html.TemplateFor(expression, templateName, htmlFieldName, mode, additionalViewData, TemplateHelper));
    }

    internal static string TemplateFor<TContainer, TValue>(this HtmlHelper<TContainer> html, Expression<Func<TContainer, TValue>> expression, string templateName, string htmlFieldName, DataBoundControlMode mode, object additionalViewData, TemplateHelperDelegate templateHelper) {
      return templateHelper(html, ModelMetadata.FromLambdaExpression<TContainer, TValue>(expression, html.ViewData), htmlFieldName ?? ExpressionHelper.GetExpressionText(expression), templateName, mode, additionalViewData);
    }

    internal static string TemplateHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData) {
      return TemplateHelper(html, metadata, htmlFieldName, templateName, mode, additionalViewData, ExecuteTemplate);
    }

    internal static string TemplateHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData, ExecuteTemplateDelegate executeTemplate) {
      if (metadata.ConvertEmptyStringToNull && string.Empty.Equals(metadata.Model))
        metadata.Model = null;
      var obj1 = metadata.Model;
      if (metadata.Model == null && mode == DataBoundControlMode.ReadOnly)
        obj1 = metadata.NullDisplayText;
      var format = mode == DataBoundControlMode.ReadOnly ? metadata.DisplayFormatString : metadata.EditFormatString;
      if (metadata.Model != null && !string.IsNullOrEmpty(format))
        obj1 = string.Format(CultureInfo.CurrentCulture, format, new object[1]
        {
          metadata.Model
        });
      var obj2 = metadata.Model ?? metadata.ModelType;
      if (html.ViewDataContainer.ViewData.TemplateInfo.GetPrivatePropertyValue<HashSet<object>>("VisitedObjects").Contains(obj2))
        return string.Empty;
      var viewData = new ViewDataDictionary(html.ViewDataContainer.ViewData) {
        Model = metadata.Model,
        ModelMetadata = metadata,
        TemplateInfo = new TemplateInfo() {
          FormattedModelValue = obj1,
          HtmlFieldPrefix = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)          
        }
      };
      viewData.TemplateInfo.SetPrivatePropertyValue<HashSet<object>>("VisitedObjects", new HashSet<object>(html.ViewContext.ViewData.TemplateInfo.GetPrivatePropertyValue<HashSet<object>>("VisitedObjects")));
      if (additionalViewData != null) {
        foreach (var keyValuePair in new RouteValueDictionary(additionalViewData))
          viewData[keyValuePair.Key] = keyValuePair.Value;
      }
      viewData.TemplateInfo.GetPrivatePropertyValue<HashSet<object>>("VisitedObjects").Add(obj2);
      return executeTemplate(html, viewData, templateName, mode, GetViewNames, GetDefaultActions);
    }

    private static HtmlHelper MakeHtmlHelper(HtmlHelper html, ViewDataDictionary viewData) {
      return new HtmlHelper(new ViewContext(html.ViewContext, html.ViewContext.View, viewData, html.ViewContext.TempData, html.ViewContext.Writer), new ViewDataContainer(viewData));
    }

    internal delegate string ExecuteTemplateDelegate(HtmlHelper html, ViewDataDictionary viewData, string templateName, DataBoundControlMode mode, GetViewNamesDelegate getViewNames, GetDefaultActionsDelegate getDefaultActions);

    internal delegate Dictionary<string, Func<HtmlHelper, string>> GetDefaultActionsDelegate(DataBoundControlMode mode);

    internal delegate IEnumerable<string> GetViewNamesDelegate(ModelMetadata metadata, params string[] templateHints);

    internal delegate string TemplateHelperDelegate(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData);

    internal abstract class ActionCacheItem {
      public abstract string Execute(HtmlHelper html, ViewDataDictionary viewData);
    }

    internal class ActionCacheCodeItem : ActionCacheItem {
      public Func<HtmlHelper, string> Action { get; set; }

      public override string Execute(HtmlHelper html, ViewDataDictionary viewData) {
        return Action(MakeHtmlHelper(html, viewData));
      }
    }

    internal class ActionCacheViewItem : ActionCacheItem {
      public string ViewName { get; set; }

      public override string Execute(HtmlHelper html, ViewDataDictionary viewData) {
        var partialView = ViewEngines.Engines.FindPartialView(html.ViewContext, ViewName);
        using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture)) {
          partialView.View.Render(new ViewContext(html.ViewContext, partialView.View, viewData, html.ViewContext.TempData, stringWriter), stringWriter);
          return stringWriter.ToString();
        }
      }
    }

    private class ViewDataContainer : IViewDataContainer {
      public ViewDataDictionary ViewData { get; set; }

      public ViewDataContainer(ViewDataDictionary viewData) {
        ViewData = viewData;
      }
    }
  }

}