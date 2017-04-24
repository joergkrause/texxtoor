using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.Portal.Core.Extensions {

  public static class BtHtmlHelperExtensions {

    # region Fieldset

    public static ClosingFieldsetElement BtFieldset(this HtmlHelper html, IHtmlString legend) {
      return new ClosingFieldsetElement(html, legend.ToString());
    }

    public static ClosingFieldsetElement BtFieldset(this HtmlHelper html, string legend) {
      return new ClosingFieldsetElement(html, legend);
    }
    public class ClosingFieldsetElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingFieldsetElement(HtmlHelper htmlHelper, string legend) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<fieldset>");
        writer.Write(@"<legend>{0}</legend>", legend);
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</fieldset>");
      }
    }

    # endregion

    # region Panel

    public static ClosingPanelElement BtPanel(this HtmlHelper html, IHtmlString title, string colorClass) {
      return new ClosingPanelElement(html, title.ToString(), colorClass);
    }

    public static ClosingPanelElement BtPanel(this HtmlHelper html, string title, string colorClass) {
      return new ClosingPanelElement(html, title, colorClass);
    }
    /*

     *  <div class="panel panel-default">
  <div class="panel-heading green-background">@ViewResources.ItemZone_Header</div>
  <div class="panel-body">


     * */
    public class ClosingPanelElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingPanelElement(HtmlHelper htmlHelper, string title, string colorClass) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<div class=""panel panel-default"">");
        writer.Write(@"<div class=""panel-heading {1}-background"">{0}</div>", title, colorClass);
        writer.Write(@"<div class=""panel-body"">");
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</div></div>");
      }
    }

    # endregion

    # region Box

    public static ClosingBoxElement BtBox(this HtmlHelper html, IHtmlString title, Color colorClass, BtIcon icon, params BtBoxOption[] options) {
      return new ClosingBoxElement(html, title.ToString(), colorClass, icon, options);
    }

    public static ClosingBoxElement BtBox(this HtmlHelper html, string title, Color colorClass, BtIcon icon, params BtBoxOption[] options) {
      return new ClosingBoxElement(html, title, colorClass, icon, options);
    }
    /*

     *  <div class="panel panel-default">
  <div class="panel-heading green-background">@ViewResources.ItemZone_Header</div>
  <div class="panel-body">


     * */
    public class ClosingBoxElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingBoxElement(HtmlHelper htmlHelper, string title, Color colorClass, BtIcon icon, params BtBoxOption[] options) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer; 
        writer.Write(@"<div class=""box {0}"">", options == null ? "" : String.Join(" ", options.Where(o => o.IsOption == false).Select(o => o.Value).ToArray()));
        writer.Write(@"<div class=""box-header {1}-background""><div class=""title""><div class=""{2}""></div>{0}</div>", title, colorClass.Name.ToLowerInvariant(), icon);
        writer.Write(@"<div class=""actions"">");
        if (options != null) {
          foreach (var btBoxOption in options.Where(o => o.IsOption)) {
            writer.Write(@"<a class=""btn {0} btn-xs btn-link"" data-toggle=""event"" href=""#""><i class=""{1}""></i></a>", btBoxOption.Value, btBoxOption == BtBoxOption.Remove ? BtIcon.Remove : String.Empty);
          }
        }
        writer.Write(@"</div></div>");
        writer.Write(@"<div class=""box-content"">");
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</div></div>");
      }
    }

    # endregion

    # region Default Form
    /// <summary>
    /// Creates a panel in Razor.
    /// </summary>
    /// <remarks>
    /// Use inside using:
    /// <code>
    /// @using(Html.BtPanel("colorClass")) {
    ///   using (Html.BtContainer()) {
    ///     Html.BtButton(BtSize.Wide, BtColor.Green) 
    ///   }
    /// }
    /// </code>
    /// </remarks>
    /// <param name="html"></param>
    /// <param name="colorClass"></param>
    /// <returns></returns>
    public static ClosingPanelFormElement BtFormPanel(this HtmlHelper html, IHtmlString title, string id, string colorClass) {
      return new ClosingPanelFormElement(html, title.ToString(), id, colorClass);
    }

    public static ClosingPanelFormElement BtFormPanel(this HtmlHelper html, string title, string id, string colorClass) {
      return new ClosingPanelFormElement(html, title, id, colorClass);
    }
    /*

     *   <div id="@id" class="hidden-to-show">
    <div class="box">
      <div class="box-header green-background">
        <div class="title">
          @name
        </div>
      </div>
      <div class="box-content">
        @html
      </div>
    </div>
  </div>

     * */
    public class ClosingPanelFormElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingPanelFormElement(HtmlHelper htmlHelper, string title, string id, string colorClass) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<div id=""{0}"" class=""hidden-to-show"">", id);
        writer.Write(@"<div class=""box"">");
        writer.Write(@"<div class=""box-header {0}-background"">", colorClass);
        writer.Write(@"<div class=""title"">{0}</div></div>", title);
        writer.Write(@"<div class=""box-content"">");
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</div></div></div>");
      }
    }

    # endregion

    # region Delete Template

    /*
     * <div id="delTemplate" class="hidden-to-show">
  <div class="alert alert-danger">
    @Html.Loc("txtDel", "You're about to deactivate a project permanently. Please confirm.")
  </div>
  <a id="@id" onclick="@click" href="#" class="btn btn-sm @cssClass">@text</a>

  @Create.FormButtonOnClick("delProjectSendButton", "", ViewResources.Action_Project_Deactivate, cssClass: "btn-warning")
  @Create.FormButtonOnClick("delProjectCancelButton", "", ViewResources.Button_Cancel)
</div>
     * */
    public static ClosingDeleteTemplateElement BtDeleteTemplate(this HtmlHelper html, IHtmlString warningText) {
      return new ClosingDeleteTemplateElement(html, warningText.ToString());
    }

    public class ClosingDeleteTemplateElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingDeleteTemplateElement(HtmlHelper htmlHelper, string warningText) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<div id=""delTemplate"" class=""hidden-to-show"">");
        writer.Write(@"<div class=""alert alert-danger"">{0}</div>", warningText);
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</div>");
      }
    }


    # endregion

    # region Simple Container

    public static ClosingDivElement BtDiv(this HtmlHelper html) {
      return new ClosingDivElement(html, String.Empty);
    }

    public static ClosingDivElement BtDiv(this HtmlHelper html, Bt cssClass) {
      return new ClosingDivElement(html, cssClass.Value);
    }

    public class ClosingDivElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingDivElement(HtmlHelper htmlHelper, string cssClass) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<div class=""{0}"">", cssClass);
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</div>");
      }
    }

    # endregion

    # region Simple Span

    public static ClosingSpanElement BtSpan(this HtmlHelper html, BtStyle cssClass) {
      return new ClosingSpanElement(html, cssClass.Value);
    }

    public class ClosingSpanElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingSpanElement(HtmlHelper htmlHelper, string cssClass) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<span class=""{0}"">", cssClass);
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</span>");
      }
    }

    # endregion

    # region Simple Heading

    public static MvcHtmlString Heading(this HtmlHelper html, HtmlHeading heading, IHtmlString content, string cssClass = null){
      return Heading(html, heading, content.ToHtmlString(), cssClass);
    }

    public static MvcHtmlString Heading(this HtmlHelper html, HtmlHeading heading, string content, string cssClass = null) {
      if (String.IsNullOrEmpty(cssClass)){
        return new MvcHtmlString(String.Format(@"<{0}>{1}</{0}>", heading, content));
      }
      else{
        return new MvcHtmlString(String.Format(@"<{0} class=""{2}"">{1}</{0}>", heading, content, cssClass));
      }
    }

    # endregion

    # region Nav Container

    public static ClosingNavElement BtNav(this HtmlHelper html) {
      return new ClosingNavElement(html, String.Empty);
    }

    public static ClosingNavElement BtNav(this HtmlHelper html, BtStyle cssClass = null) {
      return new ClosingNavElement(html, cssClass ?? cssClass.Value);
    }

    public class ClosingNavElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingNavElement(HtmlHelper htmlHelper, string cssClass) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<ul class=""nav {0}"">", cssClass);
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</ul>");
      }
    }

    public static ClosingItemElement BtItem(this HtmlHelper html, BtStyle cssClass = null) {
      return new ClosingItemElement(html, cssClass == null ? String.Empty : cssClass.Value);
    }

    public class ClosingItemElement : IDisposable {
      private readonly HtmlHelper _helper;
      public ClosingItemElement(HtmlHelper htmlHelper, string cssClass) {
        _helper = htmlHelper;
        var writer = _helper.ViewContext.Writer;
        writer.Write(@"<li class=""nav {0}"">", cssClass);
      }

      public void Dispose() {
        _helper.ViewContext.Writer.Write("</li>");
      }
    }

    # endregion

  }
}