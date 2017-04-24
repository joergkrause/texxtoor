using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models.Attributes;

namespace Texxtoor.Models {

  # region Media

  [Table("Elements")]
  [SnippetElementAttribute(@"<figure><img src=""{0}"" alt=""{1}"" width=""{3}"" height=""{4}"" /><figcaption>{5}{6}{7}{8}{9}{2}</figcaption></figure>", "ItemHref", "Name", "Title", "Width", "Height", "Label", "Major", "Separator", "Minor", "Divider", CreateResource = true)]
  public class ImageSnippet : TitledSnippet {

    private string _properties;
    private string _itemHref;

    public ImageSnippet() {
      _itemHref = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".png";
    }

    public string Properties {
      get {
        if (String.IsNullOrEmpty(_properties)) {
          int w = 200, h = 200;
          if (Content != null) {
            try {
              var ms = new MemoryStream(Content);
              var img = System.Drawing.Image.FromStream(ms);
              w = img.Width;
              h = img.Height;
            } catch (Exception) {
            }
          }
          _properties =
            String.Format(
              "{{\"OriginalWidth\":{0},\"OriginalHeight\":{1},\"ImageWidth\":{0},\"ImageHeight\":{1},\"KeepSize\":true,\"Effects\":null,\"Colors\":{{\"TransparentColor\":null,\"Brightness\":0,\"Contrast\":0,\"Hue\":0,\"Saturation\":0}},\"Crop\":null}}",
              w, h);
        }

        return _properties;
      }
      set { _properties = value; }
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
    public string ItemHref {
      get { return _itemHref; }
      set { _itemHref = value; }
    }

    [NotMapped]
    public int Width {
      get {
        var properties = System.Web.Helpers.Json.Decode<ImageProperties>(Properties);
        return properties.ImageWidth;
      }
    }
    [NotMapped]
    public int Height {
      get {
        var properties = System.Web.Helpers.Json.Decode<ImageProperties>(Properties);
        return properties.ImageHeight;
      }
    }
    # endregion
  }


  # endregion

  [Table("Elements")]
  [SnippetElementAttribute("{0}", "RawContent")]
  public class TextSnippet : Snippet {

    [NotMapped]
    public override string WidgetName {
      get { return "Text"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

  }

  [Table("Elements")]
  [SnippetElementAttribute("{0}", "RawContent")]
  public class SidebarSnippet : TextSnippet {

    /// <summary>
    /// An optional field that defines the render behavior, such as "Note", "Tip", "Advice", "Important!", ...
    /// </summary>
    public SidebarType SidebarType { get; set; }

    [NotMapped]
    public override string WidgetName {
      get { return "Sidebar"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

  }

  [Table("Elements")]
  [SnippetElementAttribute(@"<div class=""caption"">{2}{3}{4}{5}{6}{1}</div><pre>{0}</pre>", "RawContent", "Title", "Label", "Major", "Separator", "Minor", "Divider")]
  public class ListingSnippet : TitledSnippet {
    [Column("ListingLanguage")]
    public string Language { get; set; }
    // use the highlight modul to format and beatify according the Language
    [Column("SyntaxHighlight")]
    public bool SyntaxHighlight { get; set; }
    // add line numbers for reference in text
    [Column("LineNumbers")]
    public bool LineNumbers { get; set; }

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
  }


  [Table("Elements")]
  [SnippetElementAttribute(@"<table><caption>{2}{3}{4}{5}{6}{1}</caption>{0}</table>", "InnerTableContent", "Title", "Label", "Major", "Separator", "Minor", "Divider")]
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

    public bool RepeatHeadRow { get; set; }

    [NotMapped]
    public uint Rows { get; set; }
    [NotMapped]
    public uint Cols { get; set; }
    [NotMapped]
    public Style TableStyle { get; set; }
    [NotMapped]
    public Style HeadRowStyle { get; set; }
    [NotMapped]
    public Style RowStyle { get; set; }
    [NotMapped]
    public Style CellStyle { get; set; }
    [NotMapped]
    public Style HeadCellStyle { get; set; }
    [NotMapped]
    public Style LeadCellStyle { get; set; }
    [NotMapped]
    public string DefaultContent { get; set; }
    [NotMapped]
    public string TableType { get; set; }

    public void GenerateTable() {
      if (Rows == 0 || Cols == 0) return;
      var sw = new StringWriter();
      using (var tw = new HtmlTextWriter(sw)) {
        TableStyle.AddAttributesToRender(tw);
        tw.AddAttribute(HtmlTextWriterAttribute.Class, "editableTableInner " + TableType); // manage private styles, as tables are used from other panes (equations) as well
        tw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
        tw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
        tw.RenderBeginTag(HtmlTextWriterTag.Table);
        tw.RenderBeginTag(HtmlTextWriterTag.Thead);
        HeadRowStyle.AddAttributesToRender(tw);
        tw.RenderBeginTag(HtmlTextWriterTag.Tr);
        for (int i = 0; i < Cols; i++) {
          ((TableType == "leadcol") ? LeadCellStyle : HeadCellStyle).AddAttributesToRender(tw);
          HeadCellStyle.AddAttributesToRender(tw);
          tw.RenderBeginTag(HtmlTextWriterTag.Th);
          tw.Write(DefaultContent);
          tw.RenderEndTag(); //th
        }
        tw.RenderEndTag(); // tr
        tw.RenderEndTag(); //thead
        tw.RenderBeginTag(HtmlTextWriterTag.Tbody);
        for (var i = 0; i < Rows; i++) {
          RowStyle.AddAttributesToRender(tw);
          tw.RenderBeginTag(HtmlTextWriterTag.Tr);
          for (int j = 0; j < Cols; j++) {
            if (TableType == "leadcol") {
              ((j == 0) ? LeadCellStyle : CellStyle).AddAttributesToRender(tw);
            } else {
              CellStyle.AddAttributesToRender(tw);
            }
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

    private static readonly Regex R = new Regex(@"^<table>(?<inner>(.|\s)*)</table>$");

    [NotMapped]
    public string InnerTableContent {
      get { return String.IsNullOrEmpty(RawContent) ? String.Empty : R.Match(RawContent).Groups["inner"].Value; }
    }

    # endregion
    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

  }


}
