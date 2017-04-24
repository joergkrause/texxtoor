using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Linq;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Attributes;
using Texxtoor.DataModels.Properties;

namespace Texxtoor.DataModels.Models.Content {

  # region Media

  /// <summary>
  /// 
  /// </summary>
  /// <remarks>The Item uses ItemHref to force production internally.</remarks>
  [Table("Elements", Schema = "Content")]
  [EditorServiceWrapper(typeof(ImageJsonBehavior))]
  [SnippetBuilder(GroupKind.Pdf,  @"<figure id='{10}'><img class='builder' src=""{0}"" alt=""{1}"" width=""{3}"" height=""{4}"" /><figcaption>{5}{6}{7}{8}{9}{2}</figcaption></figure>", "Item", "Name", "Title", "Width", "Height", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId", CreateResource = true)]
  [SnippetBuilder(GroupKind.Epub, @"<figure id='{10}'><img class='builder' src=""{0}"" alt=""{1}"" width=""{3}"" height=""{4}"" /><figcaption>{5}{6}{7}{8}{9}{2}</figcaption></figure>", "Item", "Name", "Title", "Width", "Height", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId", CreateResource = true)]
  [SnippetBuilder(GroupKind.Html, @"<figure id='{10}'><img class='builder' src=""{0}"" alt=""{1}"" width=""{3}"" height=""{4}"" /><figcaption>{5}{6}{7}{8}{9}{2}</figcaption></figure>", "Item", "Name", "Title", "Width", "Height", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId", CreateResource = true)]
  public class ImageSnippet : TitledSnippet {

    public override string Properties {
      get {
        var prop = new JavaScriptSerializer().Deserialize<ImageProperties>(properties);
        if (String.IsNullOrEmpty(properties) || prop == null) {
          int w = 200, h = 200;
          if (Content != null) {
            byte[] cnt = null;
            // first assume the image in the blob store
            if (Content.Length == 16) {
              var guid = new Guid(Content);
              using (var blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.Resources)) {
                cnt = blob.Content;
              }
            } else {
              cnt = Content;
            }
            try {
              var ms = new MemoryStream(cnt);
              var img = System.Drawing.Image.FromStream(ms);
              w = img.Width;
              h = img.Height;
            } catch (Exception) {
            }
          }
          properties =
            String.Format(
              "{{\"OriginalWidth\":{0},\"OriginalHeight\":{1},\"ImageWidth\":{0},\"ImageHeight\":{1},\"KeepSize\":true,\"Effects\":null,\"Colors\":{{\"TransparentColor\":null,\"Brightness\":0,\"Contrast\":0,\"Hue\":0,\"Saturation\":0}},\"Crop\":null}}",
              w, h);
        }
        return properties;
      }
      set { properties = value; }
    }

    [StringLength(150)]
    public string MimeType { get; set; }

    [NotMapped]
    public override string WidgetName {
      get { return "Image"; }
    }

    # region Builder Support

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Image; }
    }

    [NotMapped]
    public string ItemHref { get; set; }

    [NotMapped]
    public int Width {
      get {
        var prop = new JavaScriptSerializer().Deserialize<ImageProperties>(Properties);
        return prop.ImageWidth;
      }
      set {
        var p = new JavaScriptSerializer().Deserialize<ImageProperties>(Properties);
        p.ImageWidth = value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }

    [NotMapped]
    public int Height {
      get {
        var prop = new JavaScriptSerializer().Deserialize<ImageProperties>(Properties);
        return prop.ImageHeight;
      }
      set {
        var p = new JavaScriptSerializer().Deserialize<ImageProperties>(Properties);
        p.ImageHeight = value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }

    [NotMapped]
    public ImageProperties ImageProperties {
      get {
        var prop = new JavaScriptSerializer().Deserialize<ImageProperties>(Properties);
        return prop;
      }
    }

    # endregion

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get {
        if (Content == null) {
          return String.Empty;
        }
        // lookup for image handler in the parent opus object
        Element opus = this;
        do {
          opus = opus.Parent;
        } while (!(opus is Opus));
        var scaleImage = ((Opus)opus).ScaleImage;
        var createImage = ((Opus)opus).CreateImage;
        var tempPath = ((Opus)opus).TempPath;
        var prop = System.Web.Helpers.Json.Decode<ImageProperties>(Properties);
        var scaleProps = new ScaleImageEventArgs { Properties = prop };
        if (scaleImage != null) {
          scaleImage(this, scaleProps);
        }
        Properties = System.Web.Helpers.Json.Encode(scaleProps.Properties);
        var image = ImageUtil.ApplyImageProperties(Content, prop);
        if (image != null) {
          using (var ms = new MemoryStream()) {
            image.Save(ms, ImageFormat.Png);
            byte[] bytes = ms.ToArray();
            // Assign temporarily assigned bytes
            Content = bytes;
            if (createImage != null) {
              // caller might write this image to disk for further processing
              ItemHref = createImage(this,
                                     new CreateImageArguments {
                                       SourceSnippet = this,
                                       TargetFragment = targetFragment,
                                       Content = bytes,
                                       Path = tempPath,
                                       FileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".png"
                                     });
            }
          }
        } else {
          // create a placeholder image
          if (HttpContext.Current != null) {
            var fs = File.ReadAllBytes(Path.Combine(HttpContext.Current.Server.MapPath("~/Content/"), "no_image.png"));
            if (createImage != null) {
              ItemHref = createImage(this,
                                     new CreateImageArguments {
                                       SourceSnippet = this,
                                       TargetFragment = targetFragment,
                                       Content = fs,
                                       Path = tempPath,
                                       FileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".png"
                                     });
            }
          }
        }
        return ItemHref;
      }
    }

  }

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<video src='{0}' width='{1}' height='{2}' {3} {4} {5} {6} poster='{7}' />", "GetVideo/{id}", "Width", "Height", "AutoPlay", "Muted", "Loop", "Controls", "PosterUrl")]
  [SnippetBuilder(GroupKind.Epub, "<video src='{0}' width='{1}' height='{2}' {3} {4} {5} {6} poster='{7}' />", "GetVideo/{id}", "Width", "Height", "AutoPlay", "Muted", "Loop", "Controls", "PosterUrl")]
  [SnippetBuilder(GroupKind.Html, "<video src='{0}' width='{1}' height='{2}' {3} {4} {5} {6} poster='{7}' />", "GetVideo/{id}", "Width", "Height", "AutoPlay", "Muted", "Loop", "Controls", "PosterUrl")]
  public class VideoSnippet : ImageSnippet {

    [NotMapped]
    public bool AutoPlay { get; set; }
    [NotMapped]
    public bool Muted { get; set; }
    [NotMapped]
    public bool Loop { get; set; }
    [NotMapped]
    public bool Controls { get; set; }

    /// <summary>
    /// A reference to an image used as a poster URL or static replacement
    /// </summary>
    public ImageSnippet PosterImage { get; set; }

    [NotMapped]
    public override string WidgetName {
      get { return "Video"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Video; }
    }

  }

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<audio src='{0}' width='{1}' height='{2}' {3} {4} {5} {6} poster='{7}' />", "GetAudio/{id}", "Width", "Height", "AutoPlay", "Muted", "Loop", "Controls", "PosterUrl")]
  [SnippetBuilder(GroupKind.Epub, "<audio src='{0}' width='{1}' height='{2}' {3} {4} {5} {6} poster='{7}' />", "GetAudio/{id}", "Width", "Height", "AutoPlay", "Muted", "Loop", "Controls", "PosterUrl")]
  [SnippetBuilder(GroupKind.Html, "<audio src='{0}' width='{1}' height='{2}' {3} {4} {5} {6} poster='{7}' />", "GetAudio/{id}", "Width", "Height", "AutoPlay", "Muted", "Loop", "Controls", "PosterUrl")]
  public class AudioSnippet : VideoSnippet {

    [NotMapped]
    public override string WidgetName {
      get { return "Audio"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Audio; }
    }

  }

  // The canvas' content property contains the JavaScript drawing instructions 
  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<canvas width='{1}' height='{2}' />", "Width", "Height")]
  [SnippetBuilder(GroupKind.Epub, "<canvas width='{1}' height='{2}' />", "Width", "Height")]
  [SnippetBuilder(GroupKind.Html, "<canvas width='{1}' height='{2}' />", "Width", "Height")]
  public class CanvasSnippet : ImageSnippet {

    [NotMapped]
    public override string WidgetName {
      get { return "Canvas"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { throw new NotImplementedException(); }
    }

  }

  # endregion

  [Table("Elements", Schema = "Content")]
  [EditorServiceWrapper(typeof(TextJsonBehavior))]
  [SnippetBuilder(GroupKind.Pdf, @"<div id=""{1}"" data-item=""text"">{0}</div>", "Item", "BuilderId")]
  [SnippetBuilder(GroupKind.Epub, @"<div id=""{1}"" data-item=""text"">{0}</div>", "Item", "BuilderId")]
  [SnippetBuilder(GroupKind.Html, @"<div id=""{1}"" data-item=""text"">{0}</div>", "Item", "BuilderId")]
  public class TextSnippet : Snippet {

    [NotMapped]
    public override string WidgetName {
      get { return "Text"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html;  }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get { return RawContent; }
    }
  }

  [Table("Elements", Schema = "Content")]
  [EditorServiceWrapper(typeof(SidebarJsonBehavior))]
  [SnippetBuilder(GroupKind.Pdf, @"<div id=""{1}"" data-item=""sidebar"" data-type=""{2}"">{0}</div>", "Item", "BuilderId", "SidebarType")]
  [SnippetBuilder(GroupKind.Epub, @"<div id=""{1}"" data-item=""sidebar"" data-type=""{2}"">{0}</div>", "Item", "BuilderId", "SidebarType")]
  [SnippetBuilder(GroupKind.Html, @"<div id=""{1}"" data-item=""sidebar"" data-type=""{2}"">{0}</div>", "Item", "BuilderId", "SidebarType")]
  public class SidebarSnippet : TextSnippet {

    private static readonly Regex Rx = new Regex(@"<header [^>]*>\s*(?<header>.*)\s*</header>\s*<aside [^>]*>\s*(?<aside>.*)\s*</aside>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// An optional field that defines the render behavior, such as "Note", "Tip", "Advice", "Important!", ...
    /// </summary>
    [NotMapped]
    public SidebarType SidebarType {
      get { return (SidebarType)new JavaScriptSerializer().Deserialize<SidebarProperties>(Properties).SidebarType; }
      set {
        var p = new JavaScriptSerializer().Deserialize<SidebarProperties>(Properties);
        p.SidebarType = (int)value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }

    public override string Properties {
      get {
        if (String.IsNullOrEmpty(properties)) {
          properties =
            String.Format(
              "{{\"SidebarType\":{0}}}",
              (int)SidebarType.Custom);
        }

        return properties;
      }
      set { properties = value; }
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Sidebar"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    public override byte[] Content {
      get {
        return base.Content;
      }
      set {
        // analyse value and assure structure
        var val = System.Text.Encoding.UTF8.GetString(value);
        var m = Rx.Match(val);
        var h = m.Groups["header"].Value;
        var a = m.Groups["aside"].Value;
        base.Content = System.Text.Encoding.UTF8.GetBytes(String.Format("<header class='builder'  data-type='{2}'>{0}</header><aside class='builder'>{1}</aside>", h, a, SidebarType));
      }
    }

    [NotMapped]
    public string HeaderContent {
      get { return Rx.Match(base.RawContent).Groups["header"].Value; }
    }

    [NotMapped]
    public string AsideContent {
      get { return Rx.Match(base.RawContent).Groups["aside"].Value; }
    }
  }

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<nav>{0}</nav>", "Item")]
  [SnippetBuilder(GroupKind.Epub, "<nav>{0}</nav>", "Item")]
  [SnippetBuilder(GroupKind.Html, "<nav>{0}</nav>", "Item")]
  public class NavigationSnippet : Snippet {
    [NotMapped]
    public override string WidgetName {
      get { return "Navigation"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get { return RawContent; }
    }

  }

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<article>{0}</article>", "Item")]
  [SnippetBuilder(GroupKind.Epub, "<article>{0}</article>", "Item")]
  [SnippetBuilder(GroupKind.Html, "<article>{0}</article>", "Item")]
  public class ArticleSnippet : Snippet {
    [NotMapped]
    public override string WidgetName {
      get { return "Article"; }
    }
    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get { return RawContent; }
    }
  }

  [Table("Elements", Schema = "Content")]
  [EditorServiceWrapper(typeof(ListingJsonBehavior))]
  [SnippetBuilder(GroupKind.Pdf, @"<div class='builder listing' id='{7}'><p class='caption'>{2}{3}{4}{5}{6}{1}</p><pre class='builder'>{0}</pre></div>", "Item", "Title", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId")]
  [SnippetBuilder(GroupKind.Epub, @"<div class='builder listing' id='{7}'><p class='caption'>{2}{3}{4}{5}{6}{1}</p><pre class='builder'>{0}</pre></div>", "Item", "Title", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId")]
  [SnippetBuilder(GroupKind.Html, @"<div class='builder listing' id='{7}'><p class='caption'>{2}{3}{4}{5}{6}{1}</p><pre class='builder'>{0}</pre></div>", "Item", "Title", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId")]
  public class ListingSnippet : TitledSnippet {

    [NotMapped]
    public string Language {
      get { return new JavaScriptSerializer().Deserialize<ListingProperties>(Properties).Language; }
      set {
        var p = new JavaScriptSerializer().Deserialize<ListingProperties>(Properties);
        p.Language = value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }
    // use the highlight modul to format and beatify according the Language

    [NotMapped]
    public bool SyntaxHighlight {
      get { return new JavaScriptSerializer().Deserialize<ListingProperties>(Properties).SyntaxHighlight; }
      set {
        var p = new JavaScriptSerializer().Deserialize<ListingProperties>(Properties);
        p.SyntaxHighlight = value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }
    // add line numbers for reference in text

    [NotMapped]
    public bool LineNumbers {
      get { return new JavaScriptSerializer().Deserialize<ListingProperties>(Properties).LineNumbers; }
      set {
        var p = new JavaScriptSerializer().Deserialize<ListingProperties>(Properties);
        p.LineNumbers = value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }

    public override string Properties {
      get {
        if (String.IsNullOrEmpty(properties)) {
          properties =
            String.Format(
              "{{\"Language\":\"{0}\",\"SyntaxHighlight\":{1},\"LineNumbers\":{2}}}",
              "C#", "true", "true");
        }

        return properties;
      }
      set { properties = value; }
    }

    public override string RawContent {
      get {
        return HttpUtility.HtmlEncode(base.RawContent);
      }
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Listing"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get {
        // make the pre using no spaces on left, user must enter &160; to indent
        int lineNumbers = 1;
        if (LineNumbers) {
          return String.Join("\n",
                             RawContent.Split('\n')
                                       .Select(s => String.Format("{0,3}: {1}", lineNumbers++, s.TrimEnd())));
        } else {
          return String.Join("\n",
                             RawContent.Split('\n')
                                       .Select(s => String.Format("{0}", s.TrimEnd())));
        }
      }
    }
  }


  [Table("Elements", Schema = "Content")]
  [EditorServiceWrapper(typeof(TableJsonBehavior))]
  [SnippetBuilder(GroupKind.Pdf, @"{9}<table class='builder {8}' id='{7}'><caption>{2}{3}{4}{5}{6}{1}</caption>{0}</table>", "Item", "Title", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId", "TableType", "FrontText")]
  [SnippetBuilder(GroupKind.Epub, @"{9}<table class='builder {8}' id='{7}'><caption>{2}{3}{4}{5}{6}{1}</caption>{0}</table>", "Item", "Title", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId", "TableType", "FrontText")]
  [SnippetBuilder(GroupKind.Html, @"{9}<table class='builder {8}' id='{7}'><caption>{2}{3}{4}{5}{6}{1}</caption>{0}</table>", "Item", "Title", "Label", "MajorString", "Separator", "MinorString", "Divider", "BuilderId", "TableType", "FrontText")]
  public class TableSnippet : TitledSnippet {

    public TableSnippet() {
      Rows = 3;
      Cols = 4;
      DefaultContent = "&nbsp;";
      RepeatHeadRow = true;
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Table"; }
    }

    [NotMapped]
    public bool RepeatHeadRow {
      get { return new JavaScriptSerializer().Deserialize<TableProperties>(Properties).RepeatHeadRow; }
      set {
        var p = new JavaScriptSerializer().Deserialize<TableProperties>(Properties);
        p.RepeatHeadRow = value;
        Properties = new JavaScriptSerializer().Serialize(p);
      }
    }

    public override string Properties {
      get {
        if (String.IsNullOrEmpty(properties)) {
          properties =
            String.Format(
              "{{\"RepeatHeadRow\":{0}}}",
              "true");
        }

        return properties;
      }
      set { properties = value; }
    }

    [NotMapped]
    public uint Rows { get; set; }

    [NotMapped]
    public uint Cols { get; set; }

    [NotMapped]
    public string DefaultContent { get; set; }

    [NotMapped]
    public string TableType { get; set; }

    public void GenerateTable() {
      if (Rows == 0 || Cols == 0) return;
      var sw = new StringWriter();
      using (var tw = new HtmlTextWriter(sw)) {
        tw.AddAttribute(HtmlTextWriterAttribute.Class, "editableTableInner " + TableType);
        // manage private styles, as tables are used from other panes (equations) as well
        tw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
        tw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
        tw.RenderBeginTag(HtmlTextWriterTag.Table);
        tw.RenderBeginTag(HtmlTextWriterTag.Thead);
        tw.RenderBeginTag(HtmlTextWriterTag.Tr);
        for (int i = 0; i < Cols; i++) {
          tw.RenderBeginTag(HtmlTextWriterTag.Th);
          tw.Write(DefaultContent);
          tw.RenderEndTag(); //th
        }
        tw.RenderEndTag(); // tr
        tw.RenderEndTag(); //thead
        tw.RenderBeginTag(HtmlTextWriterTag.Tbody);
        for (var i = 0; i < Rows; i++) {
          tw.RenderBeginTag(HtmlTextWriterTag.Tr);
          for (int j = 0; j < Cols; j++) {
            tw.RenderBeginTag(HtmlTextWriterTag.Td);
            tw.Write(DefaultContent);
            tw.RenderEndTag(); // td
          }
          tw.RenderEndTag(); // tr
        }
        tw.RenderEndTag(); // tbody
        tw.RenderEndTag(); // table
      }
      Content = System.Text.Encoding.ASCII.GetBytes(sw.ToString());
    }

    # region Builder Support

    /// <summary>
    /// The regex assumes that the table's content can consist of leading text (front) and the table itself. 
    /// Using this we assure a clean output, even if the editor adds some parts we cannot process on production.
    /// </summary>
    private static readonly Regex Rx = new Regex(@"(?<front>.*)<table[^>]*>\s*(?<inner>.*)\s*</table>$",
                                                 RegexOptions.Singleline | RegexOptions.IgnoreCase |
                                                 RegexOptions.Compiled);

    # endregion

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get { return String.IsNullOrEmpty(RawContent) ? String.Empty : Rx.Match(RawContent).Groups["inner"].Value; }
    }

    /// <summary>
    /// Forward the front text, text that goes before the table.
    /// </summary>
    [NotMapped]
    public string FrontText {
      get { return String.IsNullOrEmpty(RawContent) ? String.Empty : Rx.Match(RawContent).Groups["front"].Value; }
    }

  }

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<footer>{0}</footer>", "Item")]
  [SnippetBuilder(GroupKind.Epub, "<footer>{0}</footer>", "Item")]
  [SnippetBuilder(GroupKind.Html, "<footer>{0}</footer>", "Item")]
  public class FooterSnippet : Snippet {
    [NotMapped]
    public override string WidgetName {
      get { return "Footer"; }
    }
    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get { return RawContent; }
    }
  }

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, "<header>{0}</header>", "Item")]
  [SnippetBuilder(GroupKind.Epub, "<header>{0}</header>", "Item")]
  [SnippetBuilder(GroupKind.Html, "<header>{0}</header>", "Item")]
  public class HeadSnippet : Snippet {
    [NotMapped]
    public override string WidgetName {
      get { return "Head"; }
    }
    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get { return RawContent; }
    }
  }

}
