using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Texxtoor.Portal.Core.Extensions.EditorForModel {
  public static class AngularEditorExtensions {
    // Methods
    public static MvcHtmlString AngularEditor(this HtmlHelper html, string expression) {
      return AngularTemplateHelpers.Template(html, expression, null, null, DataBoundControlMode.Edit, null);
    }

    public static MvcHtmlString AngularEditor(this HtmlHelper html, string expression, object additionalViewData) {
      return AngularTemplateHelpers.Template(html, expression, null, null, DataBoundControlMode.Edit, additionalViewData);
    }

    public static MvcHtmlString AngularEditor(this HtmlHelper html, string expression, string templateName) {
      return AngularTemplateHelpers.Template(html, expression, templateName, null, DataBoundControlMode.Edit, null);
    }

    public static MvcHtmlString AngularEditor(this HtmlHelper html, string expression, string templateName, object additionalViewData) {
      return AngularTemplateHelpers.Template(html, expression, templateName, null, DataBoundControlMode.Edit, additionalViewData);
    }

    public static MvcHtmlString AngularEditor(this HtmlHelper html, string expression, string templateName, string htmlFieldName) {
      return AngularTemplateHelpers.Template(html, expression, templateName, htmlFieldName, DataBoundControlMode.Edit, null);
    }

    public static MvcHtmlString AngularEditor(this HtmlHelper html, string expression, string templateName, string htmlFieldName, object additionalViewData) {
      return AngularTemplateHelpers.Template(html, expression, templateName, htmlFieldName, DataBoundControlMode.Edit, additionalViewData);
    }

    public static MvcHtmlString AngularEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression) {
      return html.TemplateFor<TModel, TValue>(expression, null, null, DataBoundControlMode.Edit, null);
    }

    public static MvcHtmlString AngularEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object additionalViewData) {
      return html.TemplateFor<TModel, TValue>(expression, null, null, DataBoundControlMode.Edit, additionalViewData);
    }

    public static MvcHtmlString AngularEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName) {
      return html.TemplateFor<TModel, TValue>(expression, templateName, null, DataBoundControlMode.Edit, null);
    }

    public static MvcHtmlString AngularEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, object additionalViewData) {
      return html.TemplateFor<TModel, TValue>(expression, templateName, null, DataBoundControlMode.Edit, additionalViewData);
    }

    public static MvcHtmlString AngularEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName) {
      return html.TemplateFor<TModel, TValue>(expression, templateName, htmlFieldName, DataBoundControlMode.Edit, null);
    }

    public static MvcHtmlString AngularEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName, object additionalViewData) {
      return html.TemplateFor<TModel, TValue>(expression, templateName, htmlFieldName, DataBoundControlMode.Edit, additionalViewData);
    }

    public static MvcHtmlString AngularEditorForModel(this HtmlHelper html) {
      return MvcHtmlString.Create(AngularTemplateHelpers.TemplateHelper(html, html.ViewData.ModelMetadata, string.Empty, null, DataBoundControlMode.Edit, null));
    }

    public static MvcHtmlString AngularEditorForModel(this HtmlHelper html, object additionalViewData) {
      return MvcHtmlString.Create(AngularTemplateHelpers.TemplateHelper(html, html.ViewData.ModelMetadata, string.Empty, null, DataBoundControlMode.Edit, additionalViewData));
    }

    public static MvcHtmlString AngularEditorForModel(this HtmlHelper html, string templateName) {
      return MvcHtmlString.Create(AngularTemplateHelpers.TemplateHelper(html, html.ViewData.ModelMetadata, string.Empty, templateName, DataBoundControlMode.Edit, null));
    }

    public static MvcHtmlString AngularEditorForModel(this HtmlHelper html, string templateName, object additionalViewData) {
      return MvcHtmlString.Create(AngularTemplateHelpers.TemplateHelper(html, html.ViewData.ModelMetadata, string.Empty, templateName, DataBoundControlMode.Edit, additionalViewData));
    }

    public static MvcHtmlString AngularEditorForModel(this HtmlHelper html, string templateName, string htmlFieldName) {
      return MvcHtmlString.Create(AngularTemplateHelpers.TemplateHelper(html, html.ViewData.ModelMetadata, htmlFieldName, templateName, DataBoundControlMode.Edit, null));
    }

    public static MvcHtmlString AngularEditorForModel(this HtmlHelper html, string templateName, string htmlFieldName, object additionalViewData) {
      return MvcHtmlString.Create(AngularTemplateHelpers.TemplateHelper(html, html.ViewData.ModelMetadata, htmlFieldName, templateName, DataBoundControlMode.Edit, additionalViewData));
    }
  }
}