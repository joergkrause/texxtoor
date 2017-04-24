using System.Web.Optimization;

namespace Texxtoor.Editor {
  public class BundleConfig {
    // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
    public static void RegisterBundles(BundleCollection bundles) {
      bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                  "~/Scripts/jquery-{version}.js"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                  "~/Scripts/jquery-ui-{version}.js"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.unobtrusive*",
                  "~/Scripts/jquery.validate*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
      bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

      bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

      bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
        "~/Content/themes/base/jquery.ui.core.css",
        "~/Content/themes/base/jquery.ui.resizable.css",
        "~/Content/themes/base/jquery.ui.selectable.css",
        "~/Content/themes/base/jquery.ui.accordion.css",
        "~/Content/themes/base/jquery.ui.autocomplete.css",
        "~/Content/themes/base/jquery.ui.button.css",
        "~/Content/themes/base/jquery.ui.dialog.css",
        "~/Content/themes/base/jquery.ui.slider.css",
        "~/Content/themes/base/jquery.ui.tabs.css",
        "~/Content/themes/base/jquery.ui.datepicker.css",
        "~/Content/themes/base/jquery.ui.progressbar.css",
        "~/Content/themes/base/jquery.ui.theme.css"));

      bundles.Add(new StyleBundle("~/Content/Base/AuthorRoom").Include(
        "~/Scripts/jquery/ui/css/jquery-ui.css",
        "~/Content/author.css",
        "~/Scripts/jquery/jstree/jquery.treeview.css"));

      bundles.Add(new ScriptBundle("~/Scripts/Base/AuthorRoom").Include(
        "~/Scripts/jquery-1.8.0.js",
        "~/Scripts/jquery-ui-1.8.22.js",
        "~/Scripts/jquery/json/jquery.json-2.3.js",
        "~/Scripts/jquery/jcookies/jcookies.js",
        "~/Scripts/jquery/jquery.shortcuts.js",
        "~/Scripts/jquery/jstree/jquery.jstree.js"));

      bundles.Add(new StyleBundle("~/Content/Base/AuthorRoom/Editor").Include(
        "~/Scripts/author/ribbon/style.css",
        "~/Scripts/author/codemirror/codemirror.css",
        "~/Scripts/author/jqmath/jqmath-0.2.0.css",
        "~/Scripts/jquery/ui/css/ui.spinner.css",
        "~/Scripts/jquery/jcrop/jquery.Jcrop.css",
        "~/Scripts/author/colorpicker/css/colorpicker.css",
        "~/Scripts/jquery/jqueryscroll/jquery.scroll.css"));

      bundles.Add(new ScriptBundle("~/Scripts/Base/AuthorRoom/Editor").Include(
        "~/Scripts/author/ribbon/jquery.ribbon.js",
        "~/Scripts/jquery/jquery.disable.text.select.pack.js",
        "~/Scripts/author/widgetmgmt.js",
        "~/Scripts/author/authorui.js",
        "~/Scripts/author/textwidget.js",
        "~/Scripts/author/codemirror/codemirror.js",
        "~/Scripts/author/codemirror/mode/clike/clike.js",
        "~/Scripts/author/codemirror/mode/css/css.js",
        "~/Scripts/author/codemirror/mode/javascript/javascript.js",
        "~/Scripts/author/codemirror/mode/vbscript/vbscript.js",
        "~/Scripts/author/codemirror/mode/xml/xml.js",
        "~/Scripts/author/jqmath/jscurry-0.2.0.js",
        "~/Scripts/author/jqmath/jqmath-0.2.0.js",
        "~/Scripts/jquery/jquery.highlight-3.js",
        "~/Scripts/jquery/ui/ui.spinner.js",
        "~/Scripts/author/authorroom.js",
        "~/Scripts/author/imagecrop.js",
        "~/Scripts/jquery/jcrop/jquery.color.js",
        "~/Scripts/jquery/jcrop/jquery.Jcrop.js",
        "~/Scripts/author/colorpicker/js/colorpicker.js",
        "~/Scripts/jquery/jquery.mousewheel.js",
        "~/Scripts/jquery/jqueryscroll/jquery.scroll.js",
        "~/Scripts/jquery/jqueryscroll/RedrawExecutor.js"));
    }
  }
}