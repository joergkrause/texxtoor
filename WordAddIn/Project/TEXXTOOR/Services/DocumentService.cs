using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using TEXXTOOR.Dialogs;
using TEXXTOOR.Properties;
using TEXXTOOR.Publish;
using TEXXTOOR.TaskPanes;
using TEXXTOOR.TexxtoorAddInService;
using System.Drawing;

namespace TEXXTOOR.Services {
  // ReSharper disable UseIndexedProperty
  public class DocumentService : IService {

    readonly Regex _regexHeading = new Regex(@"Heading(?<hx>\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    enum Snippet { Listing, Sidebar, BulletList }

    # region ServerProperties

    public string ListingSnippetDefault {
      get {
        if (String.IsNullOrEmpty(_listingSnippetDefault)) {
          _listingSnippetDefault = Resources.Default_ListingContent;
        }
        return _listingSnippetDefault.Replace(@"\n", Environment.NewLine);
      }
      set { _listingSnippetDefault = value; }
    }

    public string ChapterTextDefault {
      get {
        if (String.IsNullOrEmpty(_chapterTextDefault)) {
          _chapterTextDefault = Resources.Default_ChapterText;
        }
        return _chapterTextDefault;
      }
      set { _chapterTextDefault = value; }
    }

    public string SectionTextDefault {
      get {
        if (String.IsNullOrEmpty(_sectionTextDefault)) {
          _sectionTextDefault = Resources.Default_SectionText;
        }
        return _sectionTextDefault;
      }
      set { _chapterTextDefault = value; }
    }

    # endregion

    #region PageProperties

    private string _listingSnippetDefault;
    private string _chapterTextDefault;
    private string _sectionTextDefault;

    public Document CurrentDocument {
      get {
        try {
          return Globals.ThisAddIn.Application.ActiveDocument;
        } catch (Exception) {
          return null;
        }
      }
    }

    //Current Selection Cursor
    public Selection CurrentSelection {
      get {
        return Globals.ThisAddIn.Application.Selection;
      }
    }

    //Current Selected Text
    public string CurrentText {
      get {
        return Globals.ThisAddIn.Application.Selection.Text;
      }
    }

    //Current Range Of Cursor
    public Range CurrentRange {
      get {
        return Globals.ThisAddIn.Application.Selection.Range;
      }
    }

    //Current Horizontal Position of cursor
    public float Left {
      get {
        return Globals.ThisAddIn.Application.Selection.get_Information(WdInformation.wdHorizontalPositionRelativeToPage);
      }
    }

    //Current Vertical Position of Cursor
    public float Top {
      get {
        return Globals.ThisAddIn.Application.Selection.get_Information(WdInformation.wdVerticalPositionRelativeToPage);
      }
    }

    #endregion


    internal void DocumentChange() {
      if (CurrentDocument == null || JustChecking) return;
      try {
        // if editing the template itself we do not activate the add-in
        if (CurrentDocument.Name == Settings.Default.TemplateName) {
          ServicePool.Instance.GetService<AddInService>().AddInEnabled = false;
          LocalizedNames();
          return;
        }
        ConsistencyCheckOkay = false;
        AssureStylesCheckOkay = false;
        Template currentTemplate = CurrentDocument.get_AttachedTemplate();
        if (currentTemplate.Name == Settings.Default.TemplateName || currentTemplate.Name == Settings.Default.TemplateNameDocx) {
          if (MessageBox.Show(Resources.DocumentService_DocumentChange_texxtoor_template_recognized__Activate_full_service_, "texxtoor", MessageBoxButtons.YesNo) == DialogResult.Yes) {
            ServicePool.Instance.GetService<AddInService>().AddInEnabled = true;
            ProvideNewName();
          }
          CurrentDocument.ActiveWindow.View.ReadingLayout = false;
          CurrentDocument.ActiveWindow.View.Type = WdViewType.wdPrintView;
          try {
            if (CurrentDocument.ProtectionType != WdProtectionType.wdNoProtection) {
              CurrentDocument.Unprotect();
            }
          } catch (Exception ex) {
            Trace.TraceError(ex.Message, "DocumentChange:Inner");
          }
          LocalizedNames();
        } else {
          CurrentDocument.ActiveWindow.View.ReadingLayout = false;
          CurrentDocument.ActiveWindow.View.Type = WdViewType.wdPrintView;
          try {
            if (CurrentDocument.ProtectionType != WdProtectionType.wdNoProtection) {
              CurrentDocument.Unprotect();
            }
          } catch (Exception ex) {
            Trace.TraceError(ex.Message, "DocumentChange:Inner");
          }
          LocalizedNames();
          ServicePool.Instance.GetService<AddInService>().AddInEnabled = false;
        }
      } catch (Exception exe) {
        Trace.TraceError(exe.Message, "DocumentChange:Outer");
      }
    }

    private readonly IDictionary<WdBuiltinStyle, string> _localStyleNames = new Dictionary<WdBuiltinStyle, string>();
    private readonly IDictionary<Style, string> _defaultStyleNames = new Dictionary<Style, string>();

    private string GetLocalizedName(WdBuiltinStyle wd) {
      return _localStyleNames[wd];
    }

    // use this for asking for custom texxtoor styles, such as FigureCaption
    private bool CheckInternalName(string localStylename) {
      return _defaultStyleNames.Any(n => n.Value.Contains(localStylename));
    }

    internal bool CheckLocalizedStyleName(Style style, WdBuiltinStyle wd) {
      if (style == null) return false;
      var styleName = style.NameLocal;
      return GetLocalizedName(wd).Contains(styleName);
    }

    private void LocalizedNames() {
      Globals.ThisAddIn.Application.ScreenUpdating = false;
      if (!_localStyleNames.Any()) {
        // create a test object
        // Move this to start up routine to get localized names
        foreach (var wd in Enum.GetValues(typeof(WdBuiltinStyle))) {
          object s = (WdBuiltinStyle)wd;
          Paragraph testPar = null;
          try {
            testPar = CurrentDocument.Paragraphs.Add();
            testPar.set_Style(ref s);
            var headingStyle = (Style)testPar.get_Style();
            CurrentDocument.Paragraphs[CurrentDocument.Paragraphs.Count].Range.Delete();
            var headingStyleName = headingStyle.NameLocal;
            _localStyleNames.Add((WdBuiltinStyle)s, headingStyleName);
          } catch (Exception ex) {
            Range testRange = null;
            // not a para style, trying range style
            try {
              testRange = testPar.Range;
              testRange.set_Style(ref s);
              var rangeStyle = (Style)testRange.get_Style();
              var rangeStyleName = rangeStyle.NameLocal;
              _localStyleNames.Add((WdBuiltinStyle)s, rangeStyleName);
              CurrentDocument.Paragraphs[CurrentDocument.Paragraphs.Count].Range.Delete();
            } catch (Exception) {
              // even not range, try table
              if (s.ToString().Contains("Table")) {
                try {
                  var t = CurrentDocument.Tables.Add(testRange, 1, 1, null, null);
                  t.set_Style(ref s);
                  var tStyle = (Style)t.get_Style();
                  var tStyleName = tStyle.NameLocal;
                  _localStyleNames.Add((WdBuiltinStyle)s, tStyleName);
                  t.Delete();
                } catch (Exception) {
                  // ignore even this
                }
              }
            }
          } finally {
            if (testPar != null) {
              testPar.Range.Delete();
            }
          }
        }
      }
      if (!_defaultStyleNames.Any()) {
        // after this, all built in names are present. However, word has some more styles "embedded" and those we catch here
        foreach (Style wd in CurrentDocument.Styles) {
          _defaultStyleNames.Add(wd, wd.NameLocal);
        }
      }
      Globals.ThisAddIn.Application.ScreenUpdating = true;
    }

    /// <summary>
    /// Need to have at least one chapter heading at the beginning
    /// </summary>
    private void AssureChapterHeading() {
      var needChapter = false;
      needChapter = CurrentDocument.Paragraphs.Count == 0;
      needChapter = !needChapter && CurrentDocument.Paragraphs.Count >= 1 && !CheckLocalizedStyleName(CurrentDocument.Paragraphs.First.get_Style(), WdBuiltinStyle.wdStyleHeading1);
      if (needChapter) {
        var pSection = CurrentSelection.Paragraphs.Add(Type.Missing);

        pSection.set_Style(WdBuiltinStyle.wdStyleHeading1);
        pSection.Format.PageBreakBefore = 0;

        pSection.Range.InsertParagraphBefore();

        if (String.IsNullOrEmpty(pSection.Range.Text.Trim())) {
          CurrentSelection.Collapse(WdCollapseDirection.wdCollapseStart);
          CurrentSelection.Range.Text += ChapterTextDefault;
          CurrentSelection.Range.NoProofing = 1;
          CurrentSelection.Move(WdUnits.wdParagraph, 1);
          CurrentSelection.Paragraphs.First.Range.Delete();
          pSection.Range.Select();
        }

      }
    }


    internal void InsertTable() {
      //MoveOutsideCaption();
      var dialogBox = Globals.ThisAddIn.Application.Dialogs[WdWordDialog.wdDialogTableInsertTable];
      object missing = 0;

      MoveOutsideCaption();
      MoveOutsideSnippet();
      if (dialogBox.Show(ref missing) != 0 && CurrentSelection.Tables[1].NestingLevel == 1) {
        SetCaption(CurrentRange, Resources.DocumentService_InsertTable, "TableCaption", false);
      }
    }

    internal void InsertVideo() {
      MoveOutsideCaption();
      MoveOutsideSnippet();
      CurrentDocument.CommandBars.ExecuteMso("OleObjectctInsert");
    }


    internal void CheckDocument() {
      try {
        Globals.ThisAddIn.Application.ScreenUpdating = false;
        if (ConsistencyChecker()) {
          if (AssureStyles()) {
            var name = CurrentDocument.Name;
            try {
              string cleanUpPath;
              var html = GetDocumentContent(out cleanUpPath);
              Convertor.Export(html, name, cleanUpPath);
              MessageBox.Show(Resources.DocumentService_CheckDocument_The_document_has_been_checked_successfully_,
              Resources.DocumentService_CheckDocument_Error_Checking, MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
              MessageBox.Show(ex.Message, Resources.DocumentService_CheckDocument_Error_Checking, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          }
        }
        ServicePool.Instance.GetService<AddInService>().Invalidate("lblOrderCheck");
        ServicePool.Instance.GetService<AddInService>().Invalidate("lblStyleCheck");

      } finally {
        Globals.ThisAddIn.Application.ScreenUpdating = true;
      }
    }

    /// <summary>
    /// Get content as HTML. We save document into temp file as filtered HTML, retrieve it, and delete the temp files.
    /// </summary>
    /// <returns></returns>
    internal string GetDocumentContent(out string cleanUpPath) {
      var html = String.Empty;
      cleanUpPath = String.Empty;
      try {
        ProvideNewName();
        Globals.ThisAddIn.Application.ScreenUpdating = false;
        var fileName = CurrentDocument.FullName;
        var oFileName = Path.GetTempFileName();
        Globals.ThisAddIn.Application.DefaultWebOptions().AllowPNG = true;       
        CurrentDocument.WebOptions.RelyOnCSS = false;
        CurrentDocument.WebOptions.OptimizeForBrowser = false;
        CurrentDocument.WebOptions.OrganizeInFolder = true;
        CurrentDocument.WebOptions.UseLongFileNames = false;
        CurrentDocument.WebOptions.AllowPNG = true;
        CurrentDocument.WebOptions.TargetBrowser = MsoTargetBrowser.msoTargetBrowserIE6; // highest possible
        CurrentDocument.WebOptions.ScreenSize = MsoScreenSize.msoScreenSize1920x1200;
        CurrentDocument.WebOptions.PixelsPerInch = 120;
        CurrentDocument.WebOptions.RelyOnVML = false;
        CurrentDocument.WebOptions.Encoding = MsoEncoding.msoEncodingUTF8;
        // save as HTML to disc
        CurrentDocument.SaveAs2(oFileName, WdSaveFormat.wdFormatFilteredHTML, true, "", false, "", false, false, false, false);
        // handle images to keep high res even with HTML
        // we assume the images are in the right order, as they appear in the document
        var fullResImgs = GetEmbeddedImages(fileName);
        // save regular to keep previous state
        CurrentDocument.SaveAs2(fileName, WdSaveFormat.wdFormatXMLDocument);
        html = File.ReadAllText(oFileName);
        // extract high res bitmaps and overwrite exports
        var xhtml = Convertor.CleanUp(html);
        var xDoc = XDocument.Parse(xhtml);
        var imgs = xDoc.Descendants("img").ToList();
        for (int i = 0; i < imgs.Count(); i++) {                  
          var src = imgs.ElementAt(i).Attribute("src").Value;
          // src is of form E:\temp\/tmp1234/image001.jpg
          var png = String.Format("{0}\\{1}.png", Path.GetDirectoryName(src), Path.GetFileNameWithoutExtension(src));
          // we now write the high res image there, forcing it to use png
          var resultBytes = ImageToByte2(fullResImgs[Path.GetFileNameWithoutExtension(src)]);          
          if (resultBytes != null) {
            File.WriteAllBytes(Path.Combine(Path.GetTempPath(), png), resultBytes);
            imgs.ElementAt(i).Attribute("src").Value = png;
          }
        }
        // keep this to cleanup after processing
        cleanUpPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(oFileName));
        html = xDoc.ToString();
        try {
          if (File.Exists(oFileName)) {
            File.Delete(oFileName);
          }
        } catch (Exception ex) {
          Trace.TraceError(ex.Message, "GetDocumentContent:Inner");
        } finally {
          // and switch back to print layout 
          CurrentDocument.ActiveWindow.View.ReadingLayout = false;
          CurrentDocument.ActiveWindow.View.Type = WdViewType.wdPrintView;
          CurrentDocument.Unprotect();
        }
        return html;
      } catch (Exception exe) {
        Trace.TraceError(exe.Message, "GetDocumentContent:Outer");
      } finally {
        Globals.ThisAddIn.Application.ScreenUpdating = true;
      }
      return html;
    }

    private static byte[] ImageToByte2(Image img) {
      byte[] byteArray;
      using (var stream = new MemoryStream()) {
        img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Close();

        byteArray = stream.ToArray();
      }
      return byteArray;
    }

    public Dictionary<string, Image> GetEmbeddedImages(string fileName) {
      var images = new Dictionary<string, Image>();
      try {
        using (var package = Package.Open(fileName, FileMode.Open)) {
          var rx = new Regex(@"image(?<digits>\d+)");
          var packageParts = package.GetParts();
          var imageParts = packageParts.Where(p => p.Uri.OriginalString.StartsWith("/word/media"));
          foreach (var imagePart in imageParts) {
            var img = Image.FromStream(imagePart.GetStream());
            // translate into HTML export name, imageN ==> imgNNN, e.g. image4 becomes img004
            var match = Int32.Parse(rx.Match(imagePart.Uri.OriginalString).Groups["digits"].Value);
            var exportname = String.Format("img{0:000}", match);
            images.Add(exportname, img);
          }
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "GetEmbeddedImages");
      }
      return images;
    }



    /// <summary>
    /// To assure that the user cannot overwrite the template we force him to save the document as regular word somewhere.
    /// </summary>
    private void ProvideNewName() {
      int tries = 3;
      var dlg = new SaveFileDialog {
        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Title = Resources.DocumentService_ProvideNewName_Save_File,
        Filter = Resources.DocumentService_ProvideNewName_Word_files
      };
      while (CurrentDocument.Name.Contains(Settings.Default.TemplateName)) {
        tries--;
        if (dlg.ShowDialog() == DialogResult.OK) {
          CurrentDocument.SaveAs2(dlg.FileName, WdSaveFormat.wdFormatXMLDocument, true, "", false, "", false, false, false, false);
          continue;
        }
        if (tries == 0) {
          MessageBox.Show(
            Resources.DocumentService_GetDocumentContent_You_must_provide_a_new_document_name,
            Resources.DocumentService_GetDocumentContent_New_File_Name);
          tries = 2;
        }
      }
    }

    # region Insert Snippets

    internal void InsertText() {
      MoveOutsideCaption();
      var pSection = MoveOutsideSnippet();
      var range = pSection.Range;

      pSection.Range.InsertParagraphAfter();

      pSection.Range.Text = "Type here...";

      pSection.Range.Select();
      pSection.set_Style(WdBuiltinStyle.wdStyleNormal);
    }

    private Paragraph MoveOutsideSnippet() {
      string[] specialStyles = { "Listing", "List Paragraph", "SideBarHeader", "SideBarContent", "Table Grid" };
      int lastPara = CurrentDocument.Range(0, CurrentSelection.Paragraphs[CurrentSelection.Paragraphs.Count].Range.End).Paragraphs.Count;
      string snippet = CurrentDocument.Paragraphs[lastPara].Range.get_Style().NameLocal;

      if (Array.IndexOf(specialStyles, snippet) == -1)
        return CurrentDocument.Paragraphs[lastPara].Range.Paragraphs.Add(Type.Missing);

      bool finish = false;
      Range r = null;

      while (!finish && lastPara <= CurrentDocument.Paragraphs.Count) {
        r = CurrentDocument.Paragraphs[lastPara].Range;
        if (r.get_Style().NameLocal != snippet) {
          finish = true;
        } else {
          lastPara++;
        }
      }

      Paragraph p = null;
      if (lastPara > CurrentDocument.Paragraphs.Count)
        p = CurrentDocument.Paragraphs.Add(Type.Missing);
      else
        p = CurrentDocument.Paragraphs.Add(CurrentDocument.Paragraphs[lastPara].Range);
      return p;
      //return CurrentDocument.Paragraphs[lastPara - 1].Range.Paragraphs.Add(Type.Missing);
    }

    internal void InsertListing() {
      try {
        MoveOutsideCaption();
        Range listingRange = null;
        //if (!string.IsNullOrEmpty(CurrentSelection.Range.Text) ||
        //    CurrentSelection.Range.Paragraphs.First.Range.Text.Trim() != string.Empty) {
        //    if (CurrentSelection.Range.Paragraphs.First.Range.Text.Contains(CurrentSelection.Text))
        //        listingRange = CurrentSelection.Range.Paragraphs.First.Range;
        //    else
        //        listingRange = CurrentSelection.Range;
        //} else {
        //    listingRange = CurrentSelection.Paragraphs.Add().Range;
        //    listingRange.Text = ServicePool.Instance.GetService<DocumentService>().ListingSnippetDefault;
        //    listingRange.InsertParagraphAfter();
        //}
        listingRange = MoveOutsideSnippet().Range;
        listingRange.Text = ServicePool.Instance.GetService<DocumentService>().ListingSnippetDefault;
        listingRange.InsertParagraphAfter();

        listingRange.set_Style("Listing");

        SetCaption(listingRange, Resources.DocumentService_InsertListing_Listing, "ListingCaption", false);
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "InsertListing");
      }
    }

    // Assure we don't take a caption as entry point
    private void MoveOutsideCaption() {
      try {
        if (CheckLocalizedStyleName(CurrentSelection.Range.get_Style(), WdBuiltinStyle.wdStyleCaption)
          ||
          CheckInternalName("Caption")
          ) {
          CurrentRange.Collapse(WdCollapseDirection.wdCollapseEnd);
          CurrentRange.GoToNext(WdGoToItem.wdGoToLine);
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
      }
    }

    private void SetCaption(Range range, string label, string style, bool after) {
      var caption = Globals.ThisAddIn.Application.CaptionLabels.Add(label);
      caption.Separator = WdSeparatorType.wdSeparatorEnDash; // TODO: Make . or - depending on server document culture
      caption.IncludeChapterNumber = true;
      if (after) {
        range.InsertCaption(label, ": ", Type.Missing, WdCaptionPosition.wdCaptionPositionBelow, false);
        range.Collapse();
        var next = range.Next(WdUnits.wdParagraph);
        if (next != null) {
          next.set_Style(style);
        }
      } else {
        range.InsertCaption(label, ": ", Type.Missing, WdCaptionPosition.wdCaptionPositionAbove, false);
        range.Collapse();
        range.set_Style(style);
      }
    }

    private void SetCaptionStyle(string styleName) {
      Style captionStyle = null;
      try { captionStyle = CurrentDocument.Styles[styleName]; } catch (System.Runtime.InteropServices.COMException) { Trace.TraceError("Style '" + styleName + "' for caption could not be found", "SetCaptionStyle"); }

      if (captionStyle != null) {
        CurrentDocument.Styles[WdBuiltinStyle.wdStyleCaption].Font = captionStyle.Font;
      }
    }


    internal void ConvertSection(int tag) {
      try {
        var range = CurrentSelection.Paragraphs.First.Range;
        range.Collapse(WdCollapseDirection.wdCollapseEnd);
        range.Select();
        switch (tag) {
          case 0:
              range.Paragraphs.First.set_Style(WdBuiltinStyle.wdStyleNormal);
            break;
          case 1:
            range.set_Style(WdBuiltinStyle.wdStyleHeading1);
            if (CurrentSelection.Paragraphs.First != null && CurrentSelection.Paragraphs.First.Previous(1) != null) {
              range.Paragraphs.First.Format.PageBreakBefore = -1;
            } else {
              range.Paragraphs.First.Format.PageBreakBefore = 0;
            }
            break;
          case 2:
            range.set_Style(WdBuiltinStyle.wdStyleHeading2);
            break;
          case 3:
            range.set_Style(WdBuiltinStyle.wdStyleHeading3);
            break;
          case 4:
            range.set_Style(WdBuiltinStyle.wdStyleHeading4);
            break;
          case 5:
            range.set_Style(WdBuiltinStyle.wdStyleHeading5);
            break;
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "ConvertSection");
      }
    }

    internal void InsertSection(int tag) {
      try {
        var range = CurrentSelection.Paragraphs.First.Range;
        range.Collapse(WdCollapseDirection.wdCollapseEnd);
        range.Select();
        int pCount = CurrentSelection.Paragraphs.Count;
        var pSection = CurrentSelection.Paragraphs.Add(Type.Missing);
        if (CurrentDocument.Paragraphs.Count == pCount) {
          pSection = CurrentSelection.Paragraphs.Add(Type.Missing);
          CurrentSelection.Paragraphs[CurrentSelection.Paragraphs.Count].Range.Delete();
        }
        switch (tag) {
          case 1:
            if (CurrentDocument.Paragraphs.Count == 1 && CurrentDocument.Paragraphs.First.get_Style().NameLocal.EndsWith("Heading1")) {
              CurrentDocument.Paragraphs[1].set_Style(WdBuiltinStyle.wdStyleHeading1);
            }
            pSection.set_Style(WdBuiltinStyle.wdStyleHeading1);
            if (CurrentSelection.Paragraphs.First != null && CurrentSelection.Paragraphs.First.Previous(1) != null) {
              pSection.Format.PageBreakBefore = -1;
            } else {
              pSection.Format.PageBreakBefore = 0;
            }
            break;
          case 2:
            pSection.set_Style(WdBuiltinStyle.wdStyleHeading2);
            break;
          case 3:
            pSection.set_Style(WdBuiltinStyle.wdStyleHeading3);
            break;
          case 4:
            pSection.set_Style(WdBuiltinStyle.wdStyleHeading4);
            break;
          case 5:
            pSection.set_Style(WdBuiltinStyle.wdStyleHeading5);
            break;
        }
        pSection.Range.InsertParagraphAfter();
        if (String.IsNullOrEmpty(pSection.Range.Text.Trim())) {
          CurrentSelection.Collapse(WdCollapseDirection.wdCollapseStart);
          CurrentSelection.Range.Text += tag == 1 ? ChapterTextDefault : SectionTextDefault;
          CurrentSelection.Range.NoProofing = 1;
          CurrentSelection.Move(WdUnits.wdParagraph, 1);
          CurrentSelection.Paragraphs.First.Range.Delete();
          pSection.Range.Select();
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "InsertSection");
      }
    }

    internal void InsertFigure(Image serverImageData, int serverId) {
      MoveOutsideCaption();
      Bitmap bm = new Bitmap(serverImageData);
      Clipboard.SetImage(bm);
      var pict = CurrentDocument.Paragraphs.Add();
      pict.Range.Paste();
      Clipboard.Clear();
      var para = CurrentDocument.Paragraphs.Add();
      para.Range.ID = String.Format("ResourceId:{0}", serverId);
      SetCaption(para.Range, Resources.DocumentService_InsertFigure_Figure, "FigureCaption", true);
    }

    internal void InsertFigure() {
      try {
        var dlg = new OpenFileDialog();
        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        dlg.Title = Resources.CommandService_InsertLocalPicture_Open_Image;
        dlg.Filter = Resources.DocumentService_InsertFigure_Image_files;
        InlineShape picture = null;
        if (dlg.ShowDialog() == DialogResult.OK) {
          //var buffer = new DocBuffer();
          //buffer.Start();
          MoveOutsideCaption();
          MoveOutsideSnippet();
          object oLinkToFile = false;  //default
          object oSaveWithDocument = true;//default
          var para = CurrentRange.Paragraphs.First;
          picture = CurrentDocument.InlineShapes.AddPicture(dlg.FileName, oLinkToFile, oSaveWithDocument, para.Range);
          //picture.Range.set_Style("Figure");
          SetCaption(picture.Range, Resources.DocumentService_InsertFigure_Figure, "FigureCaption", true);
          //buffer.End();
        }
        dlg.Dispose();

      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "InsertFigure");
      }
    }

    internal void InsertSidebar() {
      try {
        Range boxRange = null;
        MoveOutsideCaption();
        //if (!string.IsNullOrEmpty(CurrentSelection.Range.Text))
        //{
        //    var range = CurrentSelection.Range;
        //    if (range.Paragraphs.Last.Range.Text.Contains(range.Text) &&
        //        range.Paragraphs.Last.Range.Text.Length != range.Text.Length)
        //    {
        //        range.InsertParagraphBefore();
        //        range.InsertParagraphAfter();
        //        range.Start = range.Start + 1;
        //    }
        //    boxRange = range;
        //}
        //else
        //{
        //    boxRange = CurrentSelection.Range;


        //    if (boxRange.Paragraphs.Last.Range.Text.Trim() != string.Empty)
        //    {
        //        boxRange.InsertAfter("\r\n");
        //        boxRange.Start = boxRange.End;
        //    }

        //    boxRange.Text = Resources.DocumentService_InsertSidebar_Content_goes_here___;
        //    boxRange.InsertParagraphAfter();
        //}

        boxRange = MoveOutsideSnippet().Range;
        boxRange.Text = Resources.DocumentService_InsertSidebar_Content_goes_here___;
        boxRange.InsertParagraphAfter();

        boxRange.set_Style("SideBarContent");
        boxRange.InsertParagraphBefore();
        var header = boxRange.Paragraphs.First;
        header.Range.InsertParagraphAfter();
        header.Range.Text = Resources.SidebarTipDefault;
        header.set_Style("SideBarHeader");
        boxRange.Start = header.Range.End;
        boxRange.InsertAfter("\r\n");
        var lastLine = boxRange.Paragraphs.Last;
        lastLine.set_Style(WdBuiltinStyle.wdStyleNormal);
        boxRange.End = lastLine.Range.Start;

        header.Range.Select();

        ContentControl cc = header.Range.ContentControls.Add(WdContentControlType.wdContentControlGroup);
        cc.LockContents = true;

        boxRange.Select();
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "InsertSidebar");
      }
    }

    internal void SetSidebarType(string strOfficeId) {
      Style currentStyle = CurrentSelection.Range.get_Style();
      if (currentStyle.NameLocal.Equals("SideBarContent") ||
          currentStyle.NameLocal.Equals("SideBarHeader")) {
        Selection selection = CurrentSelection;
        bool findResult = false;
        if (!currentStyle.NameLocal.Equals("SideBarHeader")) {
          var StyleToFind = CurrentDocument.Styles["SideBarHeader"];
          if (StyleToFind != null) {
            selection = CurrentSelection;
            selection.Find.Forward = false;
            selection.Find.Format = true;
            selection.Find.Text = "";
            selection.Find.Wrap = WdFindWrap.wdFindContinue;
            selection.Find.set_Style(StyleToFind);
            findResult = selection.Find.Execute();
          }
        } else {
          findResult = true;
        }

        if (selection != null && findResult) {
          Paragraph p = selection.Paragraphs.First;
          var oldFormat = p.Range.ParagraphFormat.Duplicate;
          bool lockHeadline = true;
          switch (strOfficeId) {
            case "btnSidebarNote":
              p.Range.Text = Resources.SidebarNoteDefault + "\r";
              break;
            case "btnSidebarWarning":
              p.Range.Text = Resources.SidebarWarningDefault + "\r";
              break;
            case "btnSidebarTip":
              p.Range.Text = Resources.SidebarTipDefault + "\r";
              break;
            case "btnSidebarCustom":
              p.Range.Text = Resources.SidebarCustomDefault + "\r";
              lockHeadline = false;
              break;
          }
          p = p.Previous();
          p.Range.ParagraphFormat = oldFormat;
          p.Range.Select();
          if (lockHeadline) {
            ContentControl cc = p.Range.ContentControls.Add(WdContentControlType.wdContentControlGroup);
            cc.LockContents = true;
          }
        }
      }
    }

    # endregion

    internal bool GetInsertEnabled(string tag) {
      // at least one element must exists and we assure this is the chapter
      var paras = CurrentSelection.Paragraphs;
      Paragraph prev = paras.First;
      if (prev == null) return false; // doc is empty
      if (CurrentRange.get_Style() == null) return false; // multiple paragraphs selected
      var currentStyle = CurrentRange.get_Style().NameLocal.ToString();
      //var snippetStyles = new[] {"SidebarHeader", "SidebarContent", "Listing", "ListingCaption", "MsoTableGrid", "Figure", "FigureCaption"};
      var snippetStyles = new[] { "List Paragraph", "Table Grid", "SideBarHeader", "SideBarContent", "Listing", "ListingCaption", "MsoTableGrid", "Figure", "FigureCaption" };

      //enable nesting a table into another
      if (tag == "Table" && currentStyle == "Table Grid")
        return true;

      switch (tag) {
        case "Table":
        case "Listing":
        case "Sidebar":
        case "Video":
        case "Figure":
          if (Enumerable.Contains(snippetStyles, currentStyle)) return false;
          break;
        case "SidebarType":
          if (currentStyle == "Sidebar")
            return true;
          return false;
        case "1":
        case "2":
        case "3":
        case "4":
        case "5":
          do {
            if (CheckLocalizedStyleName(prev.get_Style(), WdBuiltinStyle.wdStyleHeading1)) {
              return true;
            }
            prev = prev.Previous(1);
          } while (prev != null);
          break;
      }

      return true;
    }

    internal bool AssureStylesCheckOkay { get; private set; }

    private bool AssureStyles() {
      bool stylesCheckOkay = true;
      Document tempDoc = null;
      // progress
      var progress = new ProgressBarDlg { Title = "Check Document", TopMost = true };
      progress.Show();
      try {
        // get allowed styles through temp doc
        tempDoc = Globals.ThisAddIn.Application.Documents.Add(Type.Missing, false, WdNewDocumentType.wdNewBlankDocument, false);
        var templatePath = String.Format("{0}\\Microsoft\\Templates", RibbonTexxtoor.LOCAL_APPN_DATA_PATH);
        var template = String.Format("{0}\\{1}", templatePath, Settings.Default.TemplateName);
        tempDoc.set_AttachedTemplate(template);
        tempDoc.UpdateStyles();
        var allowedStyles = tempDoc.Styles.Cast<Style>().ToList().Select(s => s.NameLocal);
        // remove all custom comments
        foreach (Microsoft.Office.Interop.Word.Comment comment in CurrentDocument.Comments) {
          if (comment.Initial.Contains(Settings.Default.Initials)) {
            comment.Delete();
          }
        }
        // check existing and add comments
        var allPara = CurrentDocument.Paragraphs.Cast<Paragraph>().ToList();
        progress.SetMax(allPara.Count());
        for (int p = 0; p < allPara.Count(); p++) {
          progress.Progress = p;
          var para = allPara[p];
          var currentStyle = para.get_Style().NameLocal as string;
          if (allowedStyles.Contains(currentStyle)) {
            // if the current style is already texxtoor we skip this
            continue;
          }
          para.set_Style("Standard");
          var userInitials = Globals.ThisAddIn.Application.UserInitials;
          Globals.ThisAddIn.Application.UserInitials = Settings.Default.Initials;
          CurrentDocument.Comments.Add(para.Range,
            String.Format(Resources.DocumentService_AssureStyles_The_style___0___is_not_allowed__Assign_valid_style_from_toolbar, currentStyle));
          Globals.ThisAddIn.Application.UserInitials = userInitials;
          stylesCheckOkay = false;
        }
      } finally {
        if (tempDoc != null) {
          tempDoc.Close(WdSaveOptions.wdDoNotSaveChanges);
        }
        progress.Close();
      }
      AssureStylesCheckOkay = stylesCheckOkay;
      return stylesCheckOkay;
    }

    internal bool GetHeadingEnabled(int tag) {
      try {
        if (tag == 1) return true; // chapter is always allowed
        var currentPara = CurrentSelection.Range.Paragraphs.First;
        while (currentPara != null) {
          var hx = 0;
          var s = currentPara.get_Style();
          do {
            if (CheckLocalizedStyleName(s, WdBuiltinStyle.wdStyleHeading1)) {
              hx = 1;
              break;
            }
            if (CheckLocalizedStyleName(s, WdBuiltinStyle.wdStyleHeading2)) {
              hx = 2;
              break;
            }
            if (CheckLocalizedStyleName(s, WdBuiltinStyle.wdStyleHeading3)) {
              hx = 3;
              break;
            }
            if (CheckLocalizedStyleName(s, WdBuiltinStyle.wdStyleHeading4)) {
              hx = 4;
              break;
            }
            if (CheckLocalizedStyleName(s, WdBuiltinStyle.wdStyleHeading5)) {
              hx = 5;
            }
            break;
          } while (true);
          if (hx > 0) {
            return (tag <= hx + 1);
          }
          currentPara = currentPara.Previous(1);
        }

      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "GetHeadingEnabled");
      }
      // in code we just check for true, if anything fails, we assume it's not allowed
      return false;
    }

    internal bool ConsistencyCheckOkay { get; private set; }

    internal bool ConsistencyChecker() {
      ConsistencyCheckOkay = true;
      // if we come here the heading broke our rule
      var userInitials = Globals.ThisAddIn.Application.UserInitials;
      // remove all comments
      try {
        foreach (Microsoft.Office.Interop.Word.Comment comment in CurrentDocument.Comments) {
          if (comment.Initial.Contains(Settings.Default.Initials)) {
            comment.Delete();
          }
        }
        var allParas = CurrentDocument.Paragraphs;
        // document must start with a chapter
        Globals.ThisAddIn.Application.Options.CommentsColor = WdColorIndex.wdBlack;
        Globals.ThisAddIn.Application.UserInitials = Settings.Default.Initials + "1";
        if (allParas.Count == 0 || !CheckLocalizedStyleName(allParas.First.get_Style(), WdBuiltinStyle.wdStyleHeading1)) {
          var first = allParas.First ?? CurrentDocument.Paragraphs.Add();
          CurrentDocument.Comments.Add(first.Range, Resources.DocumentService_ConsistencyChecker_The_document_must_begin_with_a_chapter_heading);
          ConsistencyCheckOkay = false;
        }
        Globals.ThisAddIn.Application.Options.CommentsColor = WdColorIndex.wdBlue;
        Globals.ThisAddIn.Application.UserInitials = Settings.Default.Initials + "2";
        // we're going through all paras and once we found a heading, we look that the next heading in direction of top of document is same or higher level
        var lastHeadingLevel = 0;
        Paragraph lastPara = null;
        for (int p = allParas.Count - 1; p > 0; p--) {
          var currentStyle = allParas[p].get_Style();
          var match = false;
          var hx = 0;
          do {
            if (CheckLocalizedStyleName(currentStyle, WdBuiltinStyle.wdStyleHeading1)) {
              match = true;
              hx = 1;
              break;
            }
            if (CheckLocalizedStyleName(currentStyle, WdBuiltinStyle.wdStyleHeading2)) {
              match = true;
              hx = 2;
              break;
            }
            if (CheckLocalizedStyleName(currentStyle, WdBuiltinStyle.wdStyleHeading3)) {
              match = true;
              hx = 3;
              break;
            }
            if (CheckLocalizedStyleName(currentStyle, WdBuiltinStyle.wdStyleHeading4)) {
              match = true;
              hx = 4;
              break;
            }
            if (CheckLocalizedStyleName(currentStyle, WdBuiltinStyle.wdStyleHeading5)) {
              match = true;
              hx = 5;
              
              break;
            }
            match = false;
            break;
          } while (true);
          // no heading just ignore
          if (!match) continue;
          if (lastHeadingLevel != 0) {
            // we cought something before and hence need to check
            if (lastHeadingLevel <= hx + 1) {
              lastHeadingLevel = hx;
              lastPara = allParas[p];
              continue;
            }
            CurrentDocument.Comments.Add((lastPara ?? allParas[p]).Range,
              String.Format(Resources.DocumentService_HeadingOrderChecker_This_section_violates,
                String.Join(".", Enumerable.Range(1, hx + 1))));
            ConsistencyCheckOkay = false;
          }
          lastPara = allParas[p];
          lastHeadingLevel = hx;
        }
        // check side bars
        Globals.ThisAddIn.Application.Options.CommentsColor = WdColorIndex.wdRed;
        Globals.ThisAddIn.Application.UserInitials = Settings.Default.Initials + "3";
        for (int p = allParas.Count - 1; p > 0; p--) {
          var currentStyle = allParas[p].get_Style().NameLocal;
          if (currentStyle == "SideBarHeader") {
            var next = allParas[p].Next(1);
            if (next != null) {
              var nextStyle = next.get_Style().NameLocal;
              if (nextStyle != "SideBarContent") {
                CurrentDocument.Comments.Add(next.Range,
                  String.Format(Resources.DocumentService_ConsistencyChecker_There_is_a_sidebar));
                ConsistencyCheckOkay = false;
              }
            } else {
              CurrentDocument.Comments.Add(allParas[p].Range,
                String.Format(Resources.DocumentService_ConsistencyChecker_Document_shall_not_end));
              ConsistencyCheckOkay = false;
            }
          }
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "HeadingOrderChecker");
      } finally {
        Globals.ThisAddIn.Application.UserInitials = userInitials;
      }
      return ConsistencyCheckOkay;
    }

    public int ChapterCount {
      get {
        if (CurrentDocument == null) return 0;
        if (CurrentDocument.Paragraphs == null) return 0;
        return CurrentDocument.Paragraphs
          .OfType<Paragraph>()
          .Count(p => p.get_Style().NameLocal == "Heading1");
      }
    }


    public int CodeSnippetCount {
      get {
        if (CurrentDocument == null) return 0;
        if (CurrentDocument.Paragraphs == null) return 0;
        return CurrentDocument.Paragraphs
          .OfType<Paragraph>()
          .Count(p => p.get_Style().NameLocal == "Listing");
      }
    }

    public int SideBarCount {
      get {
        if (CurrentDocument == null) return 0;
        if (CurrentDocument.Paragraphs == null) return 0;
        return CurrentDocument.Paragraphs
          .OfType<Paragraph>()
          .Count(p => p.get_Style().NameLocal == "Sidebar");
      }
    }


    internal void DeleteSnippet() {
      try {
        var paras = CurrentSelection.Range.Paragraphs;
        if (paras.Count == 1) {
          // remove caption before or after as well
          var next = CurrentSelection.Range.Paragraphs.First.Next(1);
          if (next != null && next.get_Style().NameLocal.Contains("Caption")) {
            next.Range.Delete();
          }
          var prev = CurrentSelection.Range.Paragraphs.First.Previous(1);
          if (prev != null && prev.get_Style().NameLocal.Contains("Caption")) {
            prev.Range.Delete();
          }
          if (CurrentDocument.Paragraphs.Count >= 1 && CurrentSelection.Paragraphs.First.get_Style().NameLocal.EndsWith("Heading1")) {
            // if only one para exists and this is the chapter, we cannot delete
            MessageBox.Show(Resources.DocumentService_DeleteSnippet_You_cannot_delete_the_only_chapter_heading, Resources.DocumentService_DeleteSnippet_Error_Deleting);
            return;
          }
          // handle deleting Sidebar
          var para = paras.First;
          //if (para != null && (para.get_Style().NameLocal == "SideBarHeader" || para.get_Style().NameLocal == "SideBarContent")) {
          if (para != null) {
            if ((para.get_Style().NameLocal == "SideBarHeader" || para.get_Style().NameLocal == "SideBarContent")) {
              // if on header, delete all following content paragraphs
              if (para.get_Style().NameLocal == "SideBarHeader") {
                DeleteSideBarContent(para, next);
                para.Range.Delete();
                return;
              }
              // if on content, move to header, then run same procedure
              while (para.get_Style().NameLocal == "SideBarContent") {
                para = para.Previous(1);
              }
              if (para.get_Style().NameLocal == "SideBarHeader") {
                DeleteSideBarContent(para, next);
                para.Range.Delete();
                return;
              }
            } else
              TryDeleteTable();
          }
          // delete single paragraph
          CurrentSelection.Range.Paragraphs.First.Range.Delete();
        } else
          TryDeleteTable();
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "DeleteSnippet");
      }
    }

    private void DeleteSideBarContent(Paragraph para, Paragraph next) {
      while (next != null && next.get_Style().NameLocal == "SideBarContent") {
        // current is header
        next.Range.Paragraphs.First.Range.Delete();
        next = next.Next(1);
      }
      para.Range.Delete();
    }

    private void TryDeleteTable() {
      if (CurrentSelection.Range.get_Style().NameLocal == "Table Grid" && CurrentSelection.Tables.Count == 1) {
        if (CurrentSelection.Tables[1].Range.Previous().get_Style().NameLocal.Contains("Caption"))
          CurrentSelection.Tables[1].Range.Previous().Delete();

        CurrentSelection.Tables[1].Delete();
      }
    }

    internal bool ApplyTemplate() {
      try {
        Template currentTemplate = CurrentDocument.get_AttachedTemplate();
        if (currentTemplate.Name != Settings.Default.TemplateName && currentTemplate.Name != Settings.Default.TemplateNameDocx) {
          if (MessageBox.Show(Resources.DocumentService_ApplyTemplate_ + currentTemplate.Name, "texxtoor Template",
                     MessageBoxButtons.YesNo) == DialogResult.Yes) {
            var templatePath = String.Format("{0}\\Microsoft\\Templates", RibbonTexxtoor.LOCAL_APPN_DATA_PATH);
            var template = String.Format("{0}\\{1}", templatePath, Settings.Default.TemplateName);
            CurrentDocument.set_AttachedTemplate(template);
            CurrentDocument.UpdateStyles();
            // prepare document
            DocumentChange();
            return true;
          }
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message, "ApplyTemplate");
      }
      return false;
    }

    internal void InsertSemanticElement(string strOfficeId) {
      switch (strOfficeId) {
        case "btn_CharacterCode":
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlCode);
          break;
        case "btn_CharacterFile":
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlSamp);
          break;
        case "btn_CharacterKeystroke":
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlKbd);
          break;
      }
    }

    internal void InsertSemanticList(TermType type, string label, string content, string id) {
      if (String.IsNullOrEmpty(CurrentSelection.Text)) return;
      switch (type) {
        case TermType.Abbreviation:
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlDfn);
          break;
        case TermType.Cite:
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlCite);
          break;
        case TermType.Definition:
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlDfn);
          break;
        case TermType.Idiom:
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlTt);
          break;
        case TermType.Link:
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHyperlink);
          break;
        case TermType.Variable:
          CurrentSelection.set_Style(WdBuiltinStyle.wdStyleHtmlVar);
          break;
      }
    }

    internal void InsertIndex() {
      if (String.IsNullOrEmpty(CurrentSelection.Range.Text)) {
        MessageBox.Show(Resources.DocumentService_InsertIndex_Select_the_text, Resources.DocumentService_InsertIndex_Error_Adding_Index, MessageBoxButtons.OK);
        return;
      }

      CurrentDocument.Indexes.MarkEntry(CurrentRange, CurrentRange.Text, CurrentRange.Text);

      if (Globals.ThisAddIn.CustomTaskPanes.Any(x => x.Control.GetType().Equals(typeof(IndexPreviewTaskPane)))) {
        Microsoft.Office.Tools.CustomTaskPane _taskpane = Globals.ThisAddIn.CustomTaskPanes.FirstOrDefault(x => x.Control.GetType().Equals(typeof(IndexPreviewTaskPane)));
        var cmntsTaskPane = _taskpane.Control as IndexPreviewTaskPane;
        cmntsTaskPane.PopulateIndex();
      }
    }

    public bool IsValidDragNDropoperation() {

      return true;
    }

    public bool JustChecking { get; set; }
  }
  // ReSharper restore UseIndexedProperty
}
