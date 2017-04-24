using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.DataModels.Models;
using System.Xml.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.DataModels.Models.Content;
using System.Diagnostics;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;

namespace Texxtoor.BaseLibrary.WordInterop {

  /// <summary>
  /// This class converts a word document using a given mapping into fragments (snippets).
  /// </summary>
  /// <remarks>
  /// The mapping is bound to a specific project. The processing is a multi-step procedure.
  /// 
  /// The class can be called multiple-times and the styles from all processed documents get aggregated!
  /// 
  /// 1. Call ExtractStyles to get a map of styles actually used in the Word document. 
  ///    This step also extracts all images in the document and invokes the 'StoreItemInBlobStore' event.
  ///    The caller is responsible to store the image for further access in the author's writer application.
  /// 2. Fill the mapping dictionaries externally.
  /// 3. Call CreateHtmlFragments for each DOCX. The fragments appears in the 'Fragments' list.
  /// 4. Attach events to get information about blob storage calls, errors, and processing progress information.
  /// 
  /// The import object can finally be XML serialized and saved to keep mappings and results save through multiple calls.
  /// </remarks>
  [Serializable]
  public sealed class Import {

    private const string NS = "Texxtoor.DataModels.Models.Content.";
    private const string DefaultPara = "paragraph";

    public Import() {
      ParagraphStyles = new Dictionary<string, string>();
      NumberingStyles = new Dictionary<string, string>();
      CharacterStyles = new Dictionary<string, string>();

      ParagraphStylesMap = new Dictionary<string, IMapObject>();
      NumberingStylesMap = new Dictionary<string, IMapObject>();
      CharacterStylesMap = new Dictionary<string, IMapObject>();

      Fragments = new List<Element>();
    }

    # region Serializer - supports Blob Store

    public static Import Deserialize(byte[] data) {
      Import i = StorageSerializer.Deserialize<Import>(data);
      return i;
    }

    public static byte[] Serialize(Import i) {
      return StorageSerializer.Serialize(i);
    }

    public byte[] Serialize() {
      return Import.Serialize(this);
    }

    # endregion

    public int ProjectId { get; set; }

    public string ImportName { get; set; }

    public List<Element> Fragments { get; set; }

    # region ----==== Extract Content and Images by mapping styles to HTML ====----

    public Dictionary<string, IMapObject> ParagraphStylesMap { get; set; }
    public Dictionary<string, IMapObject> NumberingStylesMap { get; set; }
    public Dictionary<string, IMapObject> CharacterStylesMap { get; set; }

    public void CreateHtmlFragments(string fileName) {
      var d1 = WordprocessingDocument.Open(fileName, false);
      CreateHtmlFragments(d1);
    }

    public void CreateHtmlFragments(byte[] fileData) {
      var ms = new MemoryStream(fileData);
      var d1 = WordprocessingDocument.Open(ms, false);
      CreateHtmlFragments(d1);
    }

    public void CreateHtmlFragments(WordprocessingDocument d1) {
      if (d1 == null) throw new ArgumentNullException("d1");
      // being prepared for unassigned paras
      if (!ParagraphStylesMap.ContainsKey(DefaultPara)) {
        ParagraphStylesMap.Add(DefaultPara, new MapObject { FragmentSplit = false, ControlData = "<p>{0}</p>", FragmentTypeName = typeof(TextSnippet).Name });
      }
      /* we're going through paragraphs one by one
       * The mapping has a definition for "fragment" level. 
       * If such an style appears, a new fragment is born
       * */
      var paras = d1.MainDocumentPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().ToList();
      var c = new StringBuilder();
      bool inSplit = false;
      MapObject map = null, lastmap = null;
      // run through all paragraphs
      foreach (var item in paras) {
        // to have a look ahead we need the next item (if any) as well; this is used to match connected element, such as figure+figurecaption, tablecaption+table and so on
        var currentIdx = paras.ToList().IndexOf(item);
        var aheadItem = (currentIdx + 1 < paras.Count() ? paras.ElementAt(currentIdx + 1) : null);
        // retrieve styles
        var t = String.Empty;
        var s = GetParaStyle(item);
        if (s == null)
        {
          Debug.WriteLine("ParaStyle not found");
          // default style, just write para
          c.AppendFormat("<p>{0}</p>", GetParaContent(d1, item, null));
          s = DefaultPara;
        }
        else
        {
          // ignore unkown
          if (!ParagraphStylesMap.ContainsKey(s))
          {
            Debug.WriteLine("ParaStyle not mapped ({0})", s);
            continue;
          }
          // ignore what's explicitly not mapped
          if (ParagraphStylesMap[s] is NoMapObject)
          {
            Debug.WriteLine("ParaStyle mapped to nomap ({0})", s);
            continue;
          }
        }
        // write everything else depending on MapObject control, rember the previous step
        lastmap = map;
        map = ParagraphStylesMap[s] as MapObject;
        // if this fragment is a split fragment we start a new fragment, we start a new fragment if the current is different from former as well
        if (map.FragmentSplit || (lastmap != null && lastmap.FragmentTypeName != map.FragmentTypeName)) {
          // write last fragment if current one is a section
          if (map.FragmentSplit && lastmap != null && c.Length > 0) {
            WriteContentToFragment(lastmap, c.ToString());
            c.Clear();
          }
          // further collect content
          c.AppendFormat(map.ControlData, GetParaContent(d1, item, aheadItem));
          if (map.FragmentTypeName == "ListingSnippet") {
            c.AppendLine();
          }
          // let's finish this fragment immediately if split
          if (map.FragmentSplit) {            
            // ignore empty paras
            if (c.Length > 0) {
              WriteContentToFragment(map, c.ToString());
              c.Clear();
            }
          }
          continue;
        } else {
          c.AppendFormat(map.ControlData, GetParaContent(d1, item, aheadItem));
          if (map.FragmentTypeName == "ListingSnippet") {
            c.AppendLine();
          }
          continue;
        }
      }
      // write last para 
      if (map != null) {
        WriteContentToFragment(map, c.ToString());
      }
      c.Clear();
    }

    private void WriteContentToFragment(MapObject map, string content) {
      // check for namespace (this is because the mapping in console test app is slightly different)
      var typeName = (map.FragmentTypeName.Contains(".")) ? map.FragmentTypeName : "Texxtoor.DataModels.Models.Content." + map.FragmentTypeName;
      var e = (Element) Activator.CreateInstance(typeof(Element).Assembly.FullName, typeName).Unwrap();
      if (e is Section) {
        // assume mapobject has an attribute 
        int mapLevel;
        if (!Int32.TryParse(map.ControlAttributes, out mapLevel)) {
          mapLevel = 1;
        }
        ((Section)e).SetDesignatedLevelOnImport(mapLevel);
      }      
      e.Name = typeName;
      // artifical replacements outside the regular context using an optional regex
      if (map.ControlExpression != null && map.ControlExpression.Replacement != null) {
        content = Regex.Replace(content, map.ControlExpression.Match, map.ControlExpression.Replacement);
      }      
      // assure that the content of a fragment is HTML compliant
      var doc = new HtmlDocument();
      doc.OptionFixNestedTags = true;
      doc.OptionOutputAsXml = false;
      doc.OptionAutoCloseOnEnd = true;      
      doc.LoadHtml(content);
      e.Content = Encoding.UTF8.GetBytes(doc.DocumentNode.OuterHtml);
      InvokeItemProcessed(new ProcessEventArgs { Name = typeName, TypeName = typeName });
      Fragments.Add(e);      
    }

    private string GetParaContent(WordprocessingDocument d, DocumentFormat.OpenXml.Wordprocessing.Paragraph item, DocumentFormat.OpenXml.Wordprocessing.Paragraph aheadItem) {
      var content = new StringBuilder();
      foreach (var run in item.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>()) {
        if (run.RunProperties != null) {
          if (!String.IsNullOrEmpty(run.InnerText)) {
            if (run.RunProperties.RunStyle != null) {
              // handle character styles here
              var v = run.RunProperties.RunStyle.Val.Value;
              if (!CharacterStylesMap.ContainsKey(v)) {
                content.Append(run.InnerText);
              } else {
                if (CharacterStylesMap[v] is NoMapObject) continue;
                var t = CharacterStylesMap[v] as MapObject;
                var i = run.InnerText;
                if (!String.IsNullOrEmpty(i)) {
                  content.Append(i);
                }
              }
            } else {
              content.Append(run.InnerText);
            }
          }
          // investigate the Run element further for drawings or images
          //.Inline.OfType<DocumentFormat.OpenXml.Drawing.Graphic>().First().OuterXml
          //"<a:graphic xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\"><a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:pic xmlns:pic=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:nvPicPr><pic:cNvPr id=\"0\" name=\"ASPEXTf0101.tif\" /><pic:cNvPicPr /></pic:nvPicPr><pic:blipFill><a:blip r:link=\"rId11\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" /><a:stretch><a:fillRect /></a:stretch></pic:blipFill><pic:spPr><a:xfrm><a:off x=\"0\" y=\"0\" /><a:ext cx=\"4391025\" cy=\"3543300\" /></a:xfrm><a:prstGeom prst=\"rect\"><a:avLst /></a:prstGeom></pic:spPr></pic:pic></a:graphicData></a:graphic>"
          var drawing = run.OfType<Drawing>().FirstOrDefault();
          if (drawing != null)
          {
            Graphic g = null;
            if (drawing.Inline != null)
            {
              g = drawing.Inline.OfType<Graphic>().FirstOrDefault();
            }
            else if (drawing.Anchor != null)
            {
              g = drawing.Anchor.OfType<Graphic>().FirstOrDefault();
            }
            if (g != null) {
              var pic = g.GraphicData.OfType<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();
              if (pic != null) {
                var nvpicpr = pic.NonVisualPictureProperties;
                var blibFill = pic.BlipFill;
                if (nvpicpr != null && blibFill != null) {
                  var cnvpr = nvpicpr.NonVisualDrawingProperties;
                  if (cnvpr != null) {
                    var name = cnvpr.Name;
                    var blib = blibFill.OfType<Blip>().FirstOrDefault();
                    string id;
                    ResourceFile extImage = null;
                    Guid? successfulConvertedImageId = null; // take an image and ALWAYS convert into JPEG
                    if (blib != null && (id = (blib.Embed != null) ? blib.Embed.Value : null) != null) {
                      // the ID can be pulled from Embed if the image is embedded in the file. Otherwise it's external and must be loaded from designated folder
                      var extImgPart = d.MainDocumentPart.GetPartById(id) as ImagePart;
                      System.Diagnostics.Debug.WriteLine(name, "*** Intern ***");
                      if (extImgPart != null) {
                        // While Processing we create a new resources collection of converted images (e.g. TIFF ==> JPG)
                        successfulConvertedImageId = StoreImageInBlobStore(name, extImgPart.GetStream()); 
                        extImage = GetImageFromStore(name);
                      }
                    } else {
                      System.Diagnostics.Debug.WriteLine(name, "*** Extern ***");
                      // pull image from "Image" folder in Blob Storage and store a pre-processed copy in blob store for further reference
                      extImage = GetImageFromStore(name);
                      if (extImage != null) {
                        using (var blob = BlobFactory.GetBlobStorage(extImage.ResourceId, BlobFactory.Container.Resources)) {
                          using (Stream ms = new MemoryStream(blob.Content)) {
                            // While Processing we create a new resources collection of converted images (e.g. TIFF ==> JPG)
                            successfulConvertedImageId = StoreImageInBlobStore(name, ms);
                          }
                        }
                      }
                    }
                    if (successfulConvertedImageId != null) {
                      if (aheadItem != null) {
                        name = GetParaContent(d, aheadItem, null);
                      }
                      // the image appears within the content stream, but we want to create a new snippet
                      ImageSnippet img = new ImageSnippet {
                        Content = successfulConvertedImageId.Value.ToByteArray(),
                        MimeType = "image/jpg",
                        Title = name
                      };
                      Fragments.Add(img);
                    } else {
                      InvokeItemMissed(new ProcessEventArgs { TypeName = typeof(ImageSnippet).Name, Name = name });
                    }
                  }
                }
              }
            }
          }
        } else {
          content.Append(run.InnerText);
        }
      }
      return content.ToString();
    }

    private string GetParaStyle(DocumentFormat.OpenXml.Wordprocessing.Paragraph item) {
      if (item.ParagraphProperties != null) {
        if (item.ParagraphProperties.ParagraphStyleId != null) {
          var v = item.ParagraphProperties.ParagraphStyleId.Val.Value;
          return ParagraphStyles[v].InnerTrim(); // removes all spaces, this is used as an ID in HTML forms while mappings, hence spaces are a bad idea
        }
      }
      // default with no specific formatting
      return null;
    }

    private Guid? StoreImageInBlobStore(string name, Stream s) {
      try {
        return InvokeStoreItemInBlobStore(new BlobStoreEventArgs { ProjectId = ProjectId, Name = name, RawData = s, TypeName = typeof(ImageSnippet).Name });
      } catch {
        return null;
      }
    }

    # endregion ----==== Extract Content and Map Styles to HTML using Maps ====----

    # region ----==== Prepare and Extract Styles From Document ====----

    public Dictionary<string, string> ParagraphStyles { get; set; }
    public Dictionary<string, string> NumberingStyles { get; set; }
    public Dictionary<string, string> CharacterStyles { get; set; }

    public void ExtractAllStyles(string fileName, bool getStylesWithEffectsPart = true) {
      var d1 = WordprocessingDocument.Open(fileName, false);
      ExtractAllStyles(d1, getStylesWithEffectsPart);
    }

    public void ExtractAllStyles(byte[] fileData, bool getStylesWithEffectsPart = true) {
      var ms = new MemoryStream(fileData);
      var d1 = WordprocessingDocument.Open(ms, false);
      ExtractAllStyles(d1, getStylesWithEffectsPart);
    }

    public void ExtractAllStyles(WordprocessingDocument d1, bool getStylesWithEffectsPart = true) {
      try {
        var stylePart = d1.MainDocumentPart.StyleDefinitionsPart;
        // collect all used (!) styles
        var paras = d1.MainDocumentPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
        var usedParaStyles = new List<string>();
        var usedCharStyles = new List<string>();
        var usedNumbStyles = new List<string>();
        foreach (var item in paras) {
          // Investigate run to get character styles
          ExtractTextStyles(item, usedCharStyles);
          // Investigate paragraph
          ExtractParaStyles(item, usedParaStyles);
        }

        // collect all defined (!) styles
        foreach (var item in stylePart.Styles.OfType<Style>()) {
          var st = new Style(item.OuterXml);
          switch (st.Type.Value) {
            case StyleValues.Paragraph:
              if (usedParaStyles.Contains(st.StyleId) && !ParagraphStyles.ContainsKey(st.StyleId)) {
                ParagraphStyles.Add(st.StyleId, st.StyleName.Val.Value);
              }
              break;
            case StyleValues.Character:
              if (usedCharStyles.Contains(st.StyleId) && !CharacterStyles.ContainsKey(st.StyleId)) {
                CharacterStyles.Add(st.StyleId, st.StyleName.Val.Value);
              }
              break;
            case StyleValues.Numbering:
              if (usedNumbStyles.Contains(st.StyleId) && !NumberingStyles.ContainsKey(st.StyleId)) {
                NumberingStyles.Add(st.StyleId, st.StyleName.Val.Value);
              }
              break;
            case StyleValues.Table:
              //ignore
              break;
          }
        }

      } finally {
        d1.Close();
      }
    }

    private void ExtractParaStyles(DocumentFormat.OpenXml.Wordprocessing.Paragraph item, List<string> usedParaStyles) {
      if (item.ParagraphProperties != null) {
        if (item.ParagraphProperties.ParagraphStyleId != null) {
          var v = item.ParagraphProperties.ParagraphStyleId.Val.Value;
          if (!usedParaStyles.Contains(v)) {
            usedParaStyles.Add(v);
          }
        }
      }
    }

    /// <summary>
    /// Extract the used character styles from run elements.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="usedCharStyles"></param>
    private void ExtractTextStyles(DocumentFormat.OpenXml.Wordprocessing.Paragraph item, IList<string> usedCharStyles) {
      // a para consists of run elements that can contain text elements
      foreach (var run in item.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>()) {
        if (run.RunProperties != null) {
          if (run.RunProperties.RunStyle != null) {
            // assume we found a character style
            var v = run.RunProperties.RunStyle.Val.Value;
            if (usedCharStyles.Contains(v)) continue;
            usedCharStyles.Add(v);
          }
        }
      }
    }

    # endregion ----==== Prepare and Extract Styles From Document ====----

    # region Events

    public event EventHandler<BlobStoreEventArgs> GetItemFromBlobStore;

    public event EventHandler<ProcessEventArgs> ItemProcessed;

    public event EventHandler<ProcessEventArgs> ItemMissed;

    public event BlobStoreEventHandler StoreItemInBlobStore;

    private void InvokeItemProcessed(ProcessEventArgs e) {
      if (ItemProcessed != null) {
        ItemProcessed(this, e);
      }
    }

    private void InvokeItemMissed(ProcessEventArgs e) {
      if (ItemMissed != null) {
        ItemMissed(this, e);
      }
    }

    private Guid? InvokeStoreItemInBlobStore(BlobStoreEventArgs e) {
      if (StoreItemInBlobStore != null) {
        return StoreItemInBlobStore(this, e);
      }
      return null;
    }

    private void InvokeGetItemFromBlobStore(BlobStoreEventArgs e) {
      if (GetItemFromBlobStore != null) {
        GetItemFromBlobStore(this, e);
      }
    }

    private ResourceFile GetImageFromStore(string name) {
      var e = new BlobStoreEventArgs {
        ProjectId = ProjectId,
        Name = name,
        TypeName = typeof(ImageSnippet).Name,
        Item = null
      };
      // ask the caller to retrieve and deliver the image so we can store it as an ImageSnippet
      InvokeGetItemFromBlobStore(e);
      // null is acceptable (image may not exist)
      return e.Item as ResourceFile;
    }

    # endregion

  }
}
