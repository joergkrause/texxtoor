using System;
using System.Linq;
using System.Web.Optimization;

namespace Texxtoor.Portal {
  public class BundleConfig {
    public static void RegisterBundles(BundleCollection bundles) {

      bundles.IgnoreList.Clear();
      AddDefaultIgnorePatterns(bundles.IgnoreList);

      bundles.UseCdn = true;

      /********************** NOT AUTHORIZED *********************/
      bundles.Add(new StyleBundle("~/Content/Landing/texxtoor").Include(
          "~/Content/css/bootstrap/bootstrap.css",               // Bootstrap
          "~/Content/css/light-theme-texxtoor.css",              // Flatty
          "~/Content/css/theme-colors.css",             // Flatty Colors
          "~/Content/css/style-light-texxtoor.css"
          ));
      bundles.Add(new StyleBundle("~/Content/Landing/business").Include(
          "~/Content/css/bootstrap/bootstrap.css",               // Bootstrap
          "~/Content/css/light-theme-business.css",              // Flatty
          "~/Content/css/theme-colors.css",             // Flatty Colors
          "~/Content/css/style-light-business.css"
          ));
      bundles.Add(new StyleBundle("~/Content/Landing/mymanuals").Include(
          "~/Content/css/bootstrap/bootstrap.css",                // Bootstrap
          "~/Content/css/light-theme-mymanuals.css",              // Flatty
          "~/Content/css/theme-colors.css",             // Flatty Colors
          "~/Content/css/style-light-mymanuals.css"
          ));

      bundles.Add(new ScriptBundle("~/Scripts/Landing").Include(
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/jquery/jguide/jguide-0.0.3.js",
        "~/Scripts/jquery-ui-1.10.3.js",
        "~/Scripts/bootstrap/bootstrap.js",
        "~/Scripts/bootstrap/bootstrap-maxlength.js",  // need this to make home.js and login forms working
        "~/Scripts/jquery/jcarousellite/jcarousellite-1.0.1.js",
        "~/Scripts/jquery/jglobalize/globalize.js",
        "~/Scripts/home.js"));

      /************************ AUTHORIZED *********************/
      /********************** LayoutCommon.cshtml **********************/

      bundles.Add(new StyleBundle("~/Content/Base/texxtoor").Include(
        "~/Content/css/bootstrap/bootstrap.css", // Bootstrap
        "~/Content/css/light-theme-texxtoor.css", // Flatty
        "~/Content/css/theme-colors.css", // Flatty Colors
        "~/Content/css/style-texxtoor.css",
        "~/Content/css/plugins/dynatree/ui.dynatree.css",
        "~/Content/css/jquery_ui.css",
        "~/Scripts/jquery/jguide/jguide.css",
        "~/Scripts/toastr/toastr.css",
        "~/Scripts/jquery/tokenizer/styles/token-input-texxtoor.css",
        "~/Content/css/plugins/slimscroll/slimscroll.css",
        "~/Content/css/bootstrap-datetimepicker.min.css",
        "~/Content/css/bootstrap-TouchSpin.css"
        //, "~/Scripts/bootstrap/bootstrap-daterangepickerFlatty.css"
        , "~/Scripts/jquery/jcrop/jquery.Jcrop.css"
        , "~/Scripts/jquery/Imageselect/imgareaselect-default.css"
        ));

      bundles.Add(new StyleBundle("~/Content/Base/mymanuals").Include(
        "~/Content/css/bootstrap/bootstrap.css", // Bootstrap
        "~/Content/css/light-theme-mymanuals.css", // Flatty
        "~/Content/css/theme-colors.css", // Flatty Colors
        "~/Content/css/style-mymanuals.css",
        "~/Content/css/plugins/dynatree/ui.dynatree.css",
        "~/Content/css/jquery_ui.css",
        "~/Scripts/jquery/jguide/jguide.css",
        "~/Scripts/toastr/toastr.css",
        "~/Scripts/jquery/tokenizer/styles/token-input-mymanuals.css",
        "~/Content/css/plugins/slimscroll/slimscroll.css",
        "~/Content/css/bootstrap-datetimepicker.min.css",
        "~/Content/css/bootstrap-TouchSpin.css"
              //, "~/Scripts/bootstrap/bootstrap-daterangepickerFlatty.css"
        , "~/Scripts/jquery/jcrop/jquery.Jcrop.css"
        , "~/Scripts/jquery/Imageselect/imgareaselect-default.css"
        ));

      bundles.Add(new StyleBundle("~/Content/Base/business").Include(
        "~/Content/css/bootstrap/bootstrap.css", // Bootstrap
        "~/Content/css/light-theme-business.css", // Flatty
        "~/Content/css/theme-colors.css", // Flatty Colors
        "~/Content/css/style-business.css",
        "~/Content/css/plugins/dynatree/ui.dynatree.css",
        "~/Content/css/jquery_ui.css",
        "~/Scripts/jquery/jguide/jguide.css",
        "~/Scripts/toastr/toastr.css",
        "~/Scripts/jquery/tokenizer/styles/token-input-business.css",
        "~/Content/css/plugins/slimscroll/slimscroll.css",
        "~/Content/css/bootstrap-datetimepicker.min.css",
        "~/Content/css/bootstrap-TouchSpin.css"
              //, "~/Scripts/bootstrap/bootstrap-daterangepickerFlatty.css"
        , "~/Scripts/jquery/jcrop/jquery.Jcrop.css"
        , "~/Scripts/jquery/Imageselect/imgareaselect-default.css"
        ));


      bundles.Add(new ScriptBundle("~/Scripts/Base").Include(
        "~/Scripts/jquery/jglobalize/globalize.js",
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/theme.js",
        "~/Scripts/modernizr-2.6.2.js",
        "~/Scripts/jquery-migrate-1.2.1.js",
        "~/Scripts/jquery-ui-1.10.3.js",
        "~/Scripts/jquery/jquery.spinner.js",
        "~/Scripts/jquery/json/jquery.json-2.3.js",
        "~/Scripts/jquery/jquery.validate.js",
        "~/Scripts/jquery/jquery.unobtrusive-ajax.js",
        "~/Scripts/jquery/jquery.validate.unobtrusive.js",
        "~/Scripts/jquery/jquery.resetvalidate.js",
        "~/Scripts/jquery/cookie.js",
        "~/Scripts/jquery/jquery.dynatree.js",
        "~/Scripts/jquery/jquery.colorpicker.js",
        "~/Scripts/jquery/jquery.fontSelector.js",
        "~/Scripts/jquery/additional-methods.js",
        "~/Scripts/jquery/jguide/jguide-0.0.3.js",
        "~/Scripts/toastr/toastr.js",
        "~/Scripts/jquery/tokenizer/jquery.tokeninput.js",
        "~/Scripts/jquery/imgpreview.0.22.jquery.js",
        //"~/Scripts/jquery/jquery.blockUI.js",
        "~/Scripts/jquery/jquery.simplemodal-{version}.js",
        "~/Scripts/jquery/jquery.form.js",
        "~/Scripts/jquery.signalR-2.0.0.js",
        "~/Scripts/bootstrap/bootstrap.js",
        "~/Scripts/bootstrap/bootstrap-maxlength.js",
        "~/Scripts/jquery/jquery.slimscroll.min.js",
        "~/Scripts/bootstrap/bootstrap-datetimepicker.js",
        "~/Scripts/moment.js",
        "~/Scripts/bootstrap/bootstrap.touchspin.js",
        "~/Scripts/jquery/jcrop/jquery.Jcrop.js",
        "~/Scripts/jquery/Imageselect/jquery.imgareaselect.pack.js",
        "~/Scripts/home.js",
        "~/Scripts/ui.js"
        ));

      /**********************  LayoutAuthorEditor.cshtml **********************/

      bundles.Add(new StyleBundle("~/Content/Base/AuthorRoom").Include(
        // base
        "~/Scripts/jquery/ui/css/jquery-ui.css",
        "~/Scripts/jquery/jqueryscroll/jquery.scroll.css",
        "~/Scripts/jquery/jcontextmenu/jquery.contextMenu.css",
        "~/Scripts/jquery/jstree/jquery.treeview.css",
        // ribbon
        "~/Scripts/ribbon/style.css",
        // images
        "~/Scripts/jquery/jcrop/jquery.Jcrop.css",
        "~/Scripts/jquery/jcolorpicker/css/colorpicker.css",
        // editor
        "~/Content/css/author.css",
        // listing
        "~/Scripts/author/codemirror/codemirror.css",
        // equations
        "~/Scripts/author/jqmath/jqmath-{version}.css"));

      bundles.Add(new ScriptBundle("~/Scripts/Base/AuthorRoom").Include(
        // base
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/jquery-migrate-1.2.1.js",
        "~/Scripts/jquery-ui-1.10.3.js",
        "~/Scripts/jquery/jquery.disable.text.select.pack.js",
        "~/Scripts/jquery/jquery.highlight-3.js",
        "~/Scripts/jquery/jquery.mousewheel.js",
        "~/Scripts/jquery/jquery.shortcuts.js",
        "~/Scripts/jquery/jquery.spinner.js",
        "~/Scripts/jquery/jstree/jquery.jstree.js",
        "~/Scripts/jquery/jcolorpicker/js/colorpicker.js",
        "~/Scripts/jquery/jcookies/jcookies.js",
        "~/Scripts/jquery/jcontextmenu/jquery.contextMenu.js",
        // figure upload
        "~/Scripts/jquery/jupload/js/jquery.tmpl.js",
        "~/Scripts/jquery/jupload/js/cors/jquery.postmessage-transport.js",
        "~/Scripts/jquery/jupload/js/cors/jquery.xdr-transport.js",
        "~/Scripts/jquery/jupload/js/jquery.iframe-transport.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-process.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-image.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-audio.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-video.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-validate.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-ui.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-jquery-ui.js",
        "~/Scripts/jquery/jupload/js/main.js",
        // ribbon
        "~/Scripts/ribbon/jquery.ribbon.js",
        // editor
        "~/Scripts/author/v2/author-base.js",
        "~/Scripts/author/v2/author.js",
        "~/Scripts/author/v2/author-ui.js",
        "~/Scripts/author/v2/author-init.js",
        "~/Scripts/author/v2/author-dialog-find.js",
        "~/Scripts/author/v2/author-dialog-link.js",
        "~/Scripts/author/v2/author-dialog-metadata.js",
        "~/Scripts/author/v2/author-dialog-prop.js",
        "~/Scripts/author/v2/author-dialog-reorganize.js",
        "~/Scripts/author/v2/author-dialog-comments.js",
        "~/Scripts/author/v2/author-equations.js",
        "~/Scripts/author/v2/author-imaging.js",
        "~/Scripts/author/v2/author-panemgmt.js",
        "~/Scripts/author/v2/author-undoredo.js",
        "~/Scripts/author/jquery.editorextentions.js",
        "~/Scripts/author/_SingleWidget.js",
        "~/Scripts/author/textwidget.js",
        "~/Scripts/author/imagecrop.js",
        "~/Scripts/author/colorpicker/js/colorpicker.js",
        "~/Scripts/author/Widgets/_SectionWidget.js",
        "~/Scripts/author/Widgets/_TextWidget.js",
        "~/Scripts/author/Widgets/_ListingWidget.js",
        "~/Scripts/author/Widgets/_SidebarWidget.js",
        "~/Scripts/author/Widgets/_TableWidget.js",
        "~/Scripts/author/Widgets/_ImageWidget.js",
        "~/Scripts/author/Menus/_TextSnippetMenu.js",
        // dialogs and ribbon dialogs
        "~/Scripts/author/Tools/BaseDlg.js",
        "~/Scripts/author/Tools/_ImageColors.js",
        "~/Scripts/author/Tools/_ImageCrop.js",
        "~/Scripts/author/Tools/_ImageUpload.js",
        "~/Scripts/author/Tools/_InternalLink.js",
        "~/Scripts/author/Tools/_Properties.js",
        "~/Scripts/author/Tools/_Comments.js",
        "~/Scripts/author/Tools/_RibbonImages.js",
        "~/Scripts/author/Tools/_ImagePicker.js",
        "~/Scripts/author/Tools/_Reorganize.js",
        // image
        "~/Scripts/jquery/jcrop/jquery.color.js",
        "~/Scripts/jquery/jcrop/jquery.Jcrop.js",
        // listings
        "~/Scripts/author/codemirror/codemirror.js",
        "~/Scripts/author/codemirror/mode/clike/clike.js",
        "~/Scripts/author/codemirror/mode/css/css.js",
        "~/Scripts/author/codemirror/mode/javascript/javascript.js",
        "~/Scripts/author/codemirror/mode/vbscript/vbscript.js",
        "~/Scripts/author/codemirror/mode/xml/xml.js",
        // equations
        "~/Scripts/author/jqmath/jscurry-0.3.0.js",
        "~/Scripts/author/jqmath/jqmath-0.4.0.js"));

      bundles.Add(new StyleBundle("~/Content/Base/TranslatorRoom").Include(
        // base
        "~/Scripts/jquery/ui/css/jquery-ui.css",
        "~/Scripts/jquery/jqueryscroll/jquery.scroll.css",
        "~/Scripts/jquery/jcontextmenu/jquery.contextMenu.css",
        "~/Scripts/jquery/jstree/jquery.treeview.css",
        // ribbon
        "~/Scripts/ribbon/style.css",
        // editor
        "~/Content/css/author.css"));

      bundles.Add(new ScriptBundle("~/Scripts/Base/TranslatorRoom").Include(
        // base
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/jquery-migrate-1.2.1.js",
        "~/Scripts/jquery-ui-1.10.3.js",
        "~/Scripts/jquery/jquery.disable.text.select.pack.js",
        "~/Scripts/jquery/jquery.highlight-3.js",
        "~/Scripts/jquery/jquery.mousewheel.js",
        "~/Scripts/jquery/jquery.shortcuts.js",
        "~/Scripts/jquery/jquery.spinner.js",
        "~/Scripts/jquery/jstree/jquery.jstree.js",
        "~/Scripts/jquery/jcolorpicker/js/colorpicker.js",
        "~/Scripts/jquery/jcookies/jcookies.js",
        "~/Scripts/jquery/jcontextmenu/jquery.contextMenu.js",
        // figure upload
        "~/Scripts/jquery/jupload/js/jquery.tmpl.js",
        "~/Scripts/jquery/jupload/js/cors/jquery.postmessage-transport.js",
        "~/Scripts/jquery/jupload/js/cors/jquery.xdr-transport.js",
        "~/Scripts/jquery/jupload/js/jquery.iframe-transport.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-process.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-image.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-audio.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-video.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-validate.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-ui.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-jquery-ui.js",
        "~/Scripts/jquery/jupload/js/main.js",
        // ribbon
        "~/Scripts/ribbon/jquery.ribbon.js"));

      bundles.Add(new StyleBundle("~/Styles/Base/DesignerRoom").Include(
        "~/Scripts/ribbon/font-awesome.4.css",
        "~/Scripts/svgedit/jgraduate/css/jPicker.css",
        "~/Scripts/svgedit/jgraduate/css/jgraduate.css",
        "~/Scripts/svgedit/svg-editor.css",
        "~/Scripts/svgedit/spinbtn/JQuerySpinBtn.css",
        "~/Scripts/designer/texxtoor.css"));

      bundles.Add(new ScriptBundle("~/Scripts/Base/DesignerRoom").Include(
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/jquery-migrate-1.2.1.js",
        //<!-- jquery Support -->
        "~/Scripts/svgedit/js-hotkeys/jquery.hotkeys.min.js",
        "~/Scripts/svgedit/jquerybbq/jquery.bbq.min.js",
        "~/Scripts/svgedit/svgicons/jquery.svgicons.js",
        "~/Scripts/svgedit/jgraduate/jquery.jgraduate.min.js",
        "~/Scripts/svgedit/spinbtn/JQuerySpinBtn.min.js",
        "~/Scripts/svgedit/touch.js",
        //<!-- SVG Edit -->
        "~/Scripts/svgedit/svgedit.js",
        "~/Scripts/svgedit/jquery-svg.js",
        "~/Scripts/svgedit/contextmenu/jquery.contextMenu.js",
        "~/Scripts/svgedit/browser.js",
        "~/Scripts/svgedit/svgtransformlist.js",
        "~/Scripts/svgedit/math.js",
        "~/Scripts/svgedit/units.js",
        "~/Scripts/svgedit/svgutils.js",
        "~/Scripts/svgedit/sanitize.js",
        "~/Scripts/svgedit/history.js",
        "~/Scripts/svgedit/coords.js",
        "~/Scripts/svgedit/recalculate.js",
        "~/Scripts/svgedit/select.js",
        "~/Scripts/svgedit/draw.js",
        "~/Scripts/svgedit/path.js",
        "~/Scripts/svgedit/svgcanvas.js",
        "~/Scripts/svgedit/svg-editor.js",
        "~/Scripts/svgedit/locale/locale.js",
        "~/Scripts/svgedit/contextmenu.js"));


      # region !!CURRENTLY NOT IN USE!!
      // !!CURRENTLY NOT IN USE!!
      bundles.Add(new ScriptBundle("~/Scripts/Base/EditorApp").Include(
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/jquery-migrate-1.2.1.js",
        "~/Scripts/jquery-ui-1.10.3.js",
        "~/Scripts/jquery/jquery.disable.text.select.pack.js",
        "~/Scripts/jquery/jquery.highlight-3.js",
        "~/Scripts/jquery/jquery.mousewheel.js",
        "~/Scripts/jquery/jquery.shortcuts.js",
        "~/Scripts/jquery/jquery.spinner.js",
        "~/Scripts/jquery/jstree/jquery.jstree.js",
        "~/Scripts/jquery/jcolorpicker/js/colorpicker.js",
        "~/Scripts/jquery/jcookies/jcookies.js",
        "~/Scripts/jquery/jcontextmenu/jquery.contextMenu.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-ui.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-jui.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-jquery-ui.js",
        "~/Scripts/jquery/jupload/js/jquery.tmpl.js",
        "~/Scripts/jquery/jupload/js/vendor/jquery.ui.widget.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-process.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-validate.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-image.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-audio.js",
        "~/Scripts/jquery/jupload/js/jquery.fileupload-video.js",
        "~/Scripts/jquery/jupload/js/jquery.iframe-transport.js",
        "~/Scripts/jquery/jcontextmenu/jquery.contextMenu.js",

        "~/Scripts/jquery/jquery.blockUI.js",
        "~/Scripts/jquery/jquery.simplemodal-1.4.1.js",
        "~/Scripts/jquery/jquery.ajaxfileupload.js",
        "~/Scripts/jquery/jquery.form.js",
        "~/Scripts/author/jquery.editorextentions.js",
        "~/Scripts/author/rangy/rangy-core.js",
        "~/Scripts/author/undo/undo.js",
        //<!-- UI of Editor -->
        "~/Scripts/author/Widgets/_ImageWidget.js",
        "~/Scripts/author/Widgets/_ListingWidget.js",
        "~/Scripts/author/Widgets/_SectionWidget.js",
        "~/Scripts/author/Widgets/_SidebarWidget.js",
        "~/Scripts/author/Widgets/_TableWidget.js",
        "~/Scripts/author/Widgets/_TextWidget.js",
        "~/Scripts/author/_SingleWidget.js",
        "~/Scripts/author/v3/author-base.js",
        "~/Scripts/author/v3/author-dialog-comments.js",
        "~/Scripts/author/v3/author-dialog-find.js",
        "~/Scripts/author/v3/author-dialog-link.js",
        "~/Scripts/author/v3/author-dialog-metadata.js",
        "~/Scripts/author/v3/author-dialog-prop.js",
        "~/Scripts/author/v3/author-dialog-reorganize.js",
        "~/Scripts/author/v3/author-equations.js",
        "~/Scripts/author/v3/author-imaging.js",
        "~/Scripts/author/v3/author-init.js",
        "~/Scripts/author/v3/author-panemgmt.js",
        "~/Scripts/author/v3/author-ui.js",
        "~/Scripts/author/v3/author-undoredo.js",
        "~/Scripts/author/v3/author.js",
        "~/Scripts/author/imagecrop.js",
        "~/Scripts/author/ribbon/jquery.ribbon.js",
        "~/Scripts/author/Menus/_TextSnippetMenu.js",
        //<!-- TOOLS -->
        "~/Scripts/author/Tools/BaseDlg.js",
        "~/Scripts/author/Tools/_Comments.js",
        "~/Scripts/author/Tools/_ImageColors.js",
        "~/Scripts/author/Tools/_ImageCrop.js",
        "~/Scripts/author/Tools/_InternalLink.js",
        "~/Scripts/author/Tools/_ImagePicker.js",
        "~/Scripts/author/Tools/_ImageUpload.js",
        "~/Scripts/author/Tools/_Properties.js",
        "~/Scripts/author/Tools/_Reorganize.js",
        "~/Scripts/author/Tools/_RibbonImages.js",
        "~/Scripts/author/Tools/_SemanticList.js",
        //<!-- Additions -->
        // image
        "~/Scripts/jquery/jcrop/jquery.color.js",
        "~/Scripts/jquery/jcrop/jquery.Jcrop.js",
        // listings
        "~/Scripts/author/codemirror/codemirror.js",
        "~/Scripts/author/codemirror/mode/clike/clike.js",
        "~/Scripts/author/codemirror/mode/css/css.js",
        "~/Scripts/author/codemirror/mode/javascript/javascript.js",
        "~/Scripts/author/codemirror/mode/vbscript/vbscript.js",
        "~/Scripts/author/codemirror/mode/xml/xml.js",
        // equations
        "~/Scripts/author/jqmath/jscurry-0.3.0.js",
        "~/Scripts/author/jqmath/jqmath-0.4.0.js",
        //<!-- ALOHA UTILS -->        
        "~/Scripts/author/core/util/class.js",
        "~/Scripts/author/core/util/arrays.js",
        "~/Scripts/author/core/util/browser.js",
        "~/Scripts/author/core/util/dom.js",
        "~/Scripts/author/core/util/dom2.js",
        "~/Scripts/author/core/util/functions.js",
        "~/Scripts/author/core/util/maps.js",
        "~/Scripts/author/core/util/strings.js",
        "~/Scripts/author/core/util/utils.js",
        "~/Scripts/author/core/util/pubsub.js",
        "~/Scripts/author/core/util/contenthandlerutils.js",
        //<!-- ALOHA HANDLER -->        
        "~/Scripts/author/core/html.js",
        "~/Scripts/author/core/handler/formathandler.js",
        "~/Scripts/author/core/handler/formatlesshandler.js",
        "~/Scripts/author/core/handler/wordcontenthandler.js",
        //<!-- ALOHA -->
        "~/Scripts/author/core/aloha.js",
        "~/Scripts/author/core/registry.js",
        "~/Scripts/author/core/util/scopes.js",
        "~/Scripts/author/core/ecma5shims.js",
        "~/Scripts/author/core/engine.js",
        "~/Scripts/author/core/core.js",
        "~/Scripts/author/core/editable.js",
        "~/Scripts/author/core/block-jump.js",
        "~/Scripts/author/core/blockmanager.js",
        "~/Scripts/author/core/command.js",
        "~/Scripts/author/core/ephemera.js",
        "~/Scripts/author/core/markup.js",
        "~/Scripts/author/core/misc.js",
        "~/Scripts/author/core/range.js",
        "~/Scripts/author/core/selection.js",
        "~/Scripts/author/core/state-override.js",
        "~/Scripts/author/core/trees.js",
        "~/Scripts/author/core/wordcontenthandler.js",
        //<!-- ALOHA FUNCTIONS -->
        "~/Scripts/author/core/functions/copypaste.js",
        "~/Scripts/author/core/functions/format.js",
        "~/Scripts/author/core/functions/inserthtml.js",
        "~/Scripts/author/core/functions/listenforcer.js",
        "~/Scripts/author/core/functions/lists.js",
        "~/Scripts/author/core/functions/paste.js",
        "~/Scripts/author/core/functions/formatlesspaste.js",
        "~/Scripts/author/core/functions/undo.js"));

      /**********************  LayoutReaderApp.cshtml **********************/
      bundles.Add(new StyleBundle("~/Content/ReaderApp").Include(
        "~/Scripts/reader/css/reader.css",
        "~/Scripts/jquery/ui/css/jquery-ui.css",
        "~/Scripts/jquery/jstree/jquery.treeview.css"));

      bundles.Add(new ScriptBundle("~/Scripts/ReaderApp").Include(
        "~/Scripts/jquery/json/jquery.json-2.3.js",
        "~/Scripts/jquery-2.0.3.js",
        "~/Scripts/jquery-ui-1.10.3.js",
        "~/Scripts/jquery-migrate-1.2.1.js",
        "~/Scripts/jquery/jquery.context.menu.js",
        "~/Scripts/jquery/jquery.mousewheel.js",
        "~/Scripts/jquery/jquery.disable.text.select.pack.js",
        "~/Scripts/jquery/jstree/jquery.jstree.js",
        "~/Scripts/reader/js/tools.js",
        "~/Scripts/reader/js/loader.js",
        "~/Scripts/reader/js/readerui.js",
        "~/Scripts/reader/js/reader.js",
        "~/Scripts/reader/js/navigationitem.js",
        "~/Scripts/reader/js/navigation.js",
        "~/Scripts/reader/js/comment.js",
        "~/Scripts/reader/js/bookmark.js",
        "~/Scripts/reader/js/book.js"
        ));
      # endregion

      # region !!CURRENTLY NOT IN USE!!
      /**********************  AuthorPortal/Resources/Index.cshtml **********************/
      bundles.Add(new StyleBundle("~/Content/ElFinder").Include(
        "~/Scripts/finder/css/commands.css",
        "~/Scripts/finder/css/common.css",
        "~/Scripts/finder/css/contextmenu.css",
        "~/Scripts/finder/css/cwd.css",
        "~/Scripts/finder/css/dialog.css",
        "~/Scripts/finder/css/fonts.css",
        "~/Scripts/finder/css/navbar.css",
        "~/Scripts/finder/css/places.css",
        "~/Scripts/finder/css/quicklook.css",
        "~/Scripts/finder/css/statusbar.css",
        "~/Scripts/finder/css/theme.css",
        "~/Scripts/finder/css/toolbar.css"));

      bundles.Add(new ScriptBundle("~/Scripts/ElFinder").Include(
        "~/Scripts/finder/elFinder.js",
        "~/Scripts/finder/elFinder.version.js",
        "~/Scripts/finder/jquery.elfinder.js",
        "~/Scripts/finder/elFinder.options.js",
        "~/Scripts/finder/elFinder.history.js",
        "~/Scripts/finder/elFinder.command.js",
        "~/Scripts/finder/elFinder.resources.js",
        "~/Scripts/finder/jquery.dialogelfinder.js",
        "~/Scripts/finder/ui/button.js",
        "~/Scripts/finder/ui/cwd.js",
        "~/Scripts/finder/ui/contextmenu.js",
        "~/Scripts/finder/ui/dialog.js",
        "~/Scripts/finder/ui/navbar.js",
        "~/Scripts/finder/ui/overlay.js",
        "~/Scripts/finder/ui/panel.js",
        "~/Scripts/finder/ui/path.js",
        "~/Scripts/finder/ui/places.js",
        "~/Scripts/finder/ui/searchbutton.js",
        "~/Scripts/finder/ui/sortbutton.js",
        "~/Scripts/finder/ui/stat.js",
        "~/Scripts/finder/ui/toolbar.js",
        "~/Scripts/finder/ui/tree.js",
        "~/Scripts/finder/ui/uploadButton.js",
        "~/Scripts/finder/ui/viewbutton.js",
        "~/Scripts/finder/ui/workzone.js",
        "~/Scripts/finder/commands/archive.js",
        "~/Scripts/finder/commands/back.js",
        "~/Scripts/finder/commands/copy.js",
        "~/Scripts/finder/commands/cut.js",
        "~/Scripts/finder/commands/download.js",
        "~/Scripts/finder/commands/duplicate.js",
        "~/Scripts/finder/commands/edit.js",
        "~/Scripts/finder/commands/extract.js",
        "~/Scripts/finder/commands/forward.js",
        "~/Scripts/finder/commands/getfile.js",
        "~/Scripts/finder/commands/help.js",
        "~/Scripts/finder/commands/home.js",
        "~/Scripts/finder/commands/info.js",
        "~/Scripts/finder/commands/mkdir.js",
        "~/Scripts/finder/commands/mkfile.js",
        "~/Scripts/finder/commands/netmount.js",
        "~/Scripts/finder/commands/paste.js",
        "~/Scripts/finder/commands/open.js",
        "~/Scripts/finder/commands/quicklook.js",
        "~/Scripts/finder/commands/quicklook.plugins.js",
        "~/Scripts/finder/commands/reload.js",
        "~/Scripts/finder/commands/rename.js",
        "~/Scripts/finder/commands/resize.js",
        "~/Scripts/finder/commands/rm.js",
        "~/Scripts/finder/commands/search.js",
        "~/Scripts/finder/commands/sort.js",
        "~/Scripts/finder/commands/up.js",
        "~/Scripts/finder/commands/upload.js",
        "~/Scripts/finder/commands/view.js"));
      #endregion

      bundles.Add(new ScriptBundle("~/Scripts/EditProfile").Include(
         "~/sass/plugins/common/ckeditor.js",
         "~/sass/plugins/common/ckeditor/templates.js",
         "~/sass/plugins/common/config.js"
        ));

      bundles.Add(new StyleBundle("~/Styles/EditProfile").Include(
        "~/sass/plugins/common/ckeditor/editor.css",
        "~/sass/plugins/common/dialog.css",
        "~/sass/plugins/common/templates.css"
        ));
    }

    public static void AddDefaultIgnorePatterns(IgnoreList ignoreList) {
      if (ignoreList == null) {
        throw new ArgumentNullException("ignoreList");
      }

      ignoreList.Ignore("*.intellisense.js");
      ignoreList.Ignore("*-vsdoc.js");

      //ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
      //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
      //ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
    }

  }
}
