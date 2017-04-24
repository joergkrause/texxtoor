using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace Texxtoor.Editor.Core {


  /// <summary>
  /// This class contains all methods to create actual content, such as PDF, EPub, iBook, and more.
  /// </summary>
  public class PdfManager : Manager<PdfManager> {

    public Stream ConvertHtmltoPdf(string htmlCode, List<Bitmap> images) {

      //Render PlaceHolder to temporary stream 
      var stringWrite = new StringWriter();
      var htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);
      var template = new TexxtoorTemplate();
      var resultStream = new MemoryStream();
      var reader = new StringReader(htmlCode);
      //Create PDF document 
      //using (
      var doc = new Document(PageSize.A4, PdfSettings.MarginLeft, PdfSettings.MarginRight, PdfSettings.MarginTop,
                             PdfSettings.MarginBottom);
      //var parser = new HTMLWorker(doc);
      var writer = PdfWriter.GetInstance(doc, resultStream);
      doc.Open();
      var css = GenerateStyleSheet();
      ParseHtml(doc, reader, css, images);
      writer.Flush();
      //CreateTableOfContents(doc, writer);
      //doc.Close();
      //parser.Close();
      return resultStream;
    }

    private static void AddCoverToParameters(IDictionary<string, string> dictionary, string workingDirectory) {
      string[] filenames = Directory.GetFiles(workingDirectory, "*-1_1.*");
      if (filenames.Length == 1) {
        string imageTag = string.Format("<img src=\"{0}\" alt=\"cover\" style=\"height: 100%\"/>", Path.GetFileName(filenames[0]));
        dictionary.Add("##Cover##", imageTag);
      } else {
        string msg = "            <div id=\"titlepage\">\n" +
                    "               <h1 class=\"part-title\">{0}</h1>\n" +
                    "               <h3 class=\"title-break\">***</h3>\n" +
                    "               <h3 class=\"author\">{1}</h3>\n" +
                    "            </div>";
        string cover = string.Format(msg, dictionary["##Title##"], dictionary["##Author##"]);
        dictionary.Add("##Cover##", cover);
      }
    }

    //private static IDictionary<string, string> Meta2Parameters(Texxtoor.BaseLibrary.Pdf2Epub.MetaData data) {
    //  IDictionary<string, string> dictionary = new Dictionary<string, string>();
    //  dictionary.Add("##Author##", data.Author);
    //  dictionary.Add("##Title##", data.Title);
    //  dictionary.Add("##Date##", data.Date);
    //  dictionary.Add("##UUID##", Guid.NewGuid().ToString());
    //  return dictionary;
    //}

    private static void ParseHtml(Document doc, StringReader html, StyleSheet css, List<Bitmap> images) {
      iTextSharp.text.Rectangle mySize = null;
      var htmlWorker = new HTMLWorker(doc);
      htmlWorker.SetStyleSheet(css);
      //htmlWorker.Parse(html);
      var objects = HTMLWorker.ParseToList(html, css);
      foreach (var element in objects) {
        doc.Add(element);
      }
      mySize = doc.PageSize;
      if (images.Count > 0) {
        for (int i = 0; i <= images.Count - 1; i++) {
          System.Drawing.Image mySource = (Bitmap)images[i];
          iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(mySource, System.Drawing.Imaging.ImageFormat.Png);
          myImage.ScaleAbsolute(400, 300);
          doc.Add(myImage);
        }
      }

    }

    private static StyleSheet GenerateStyleSheet() {

      FontFactory.Register(@"c:\windows\fonts\verdana.ttf", "Verdana");

      StyleSheet css = new StyleSheet();

      css.LoadTagStyle("body", "face", "Verdana");
      css.LoadTagStyle("body", "size", "9pt");
      css.LoadTagStyle("p", "face", "Verdana");
      css.LoadTagStyle("p", "size", "8pt");

      css.LoadTagStyle("h1", "face", "Verdana");
      css.LoadTagStyle("h1", "size", "10pt");
      //css.LoadTagStyle(HtmlTags.H4, "align", "left");
      //css.LoadTagStyle(HtmlTags.H3, "face", "Verdana");
      //css.LoadTagStyle(HtmlTags.H3, "size", "13pt");
      //css.LoadTagStyle(HtmlTags.H3, "align", "left");
      //css.LoadTagStyle(HtmlTags.TD, "width", "10%");

      return css;
    }

    public struct TableOfContentsEntry {
      private string _title;
      private string _page;

      public TableOfContentsEntry(string title, string page) {
        _title = title;
        _page = page;
      }

      public string Title {
        get { return _title; }
        set { _title = value; }
      }

      public string Page {
        get { return _page; }
        set { _page = value; }
      }
    }



    public void CreateTableOfContents(Document doc, PdfWriter writer) {

      int _pageCount = 0;
      doc.NewPage();
      _pageCount++;
      PdfPTable _pdfContentsTable;
      doc.Add(new Paragraph("Table of Contents", FontFactory.GetFont("verdana", 18, iTextSharp.text.Font.BOLD)));
      doc.Add(new Chunk(Environment.NewLine));

      _pdfContentsTable = new PdfPTable(2);


      var _contentsTable = new List<TableOfContentsEntry>();

      _contentsTable.Add(new TableOfContentsEntry("Execution Page1", writer.CurrentPageNumber.ToString()));
      _contentsTable.Add(new TableOfContentsEntry("Execution Page2", writer.CurrentPageNumber.ToString()));
      _contentsTable.Add(new TableOfContentsEntry("Execution Page3", writer.CurrentPageNumber.ToString()));


      foreach (TableOfContentsEntry content in _contentsTable) {
        var nameCell = new PdfPCell(_pdfContentsTable);
        nameCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        nameCell.Padding = 6f;
        nameCell.Phrase = new Phrase(content.Title);
        _pdfContentsTable.AddCell(nameCell);

        var pageCell = new PdfPCell(_pdfContentsTable);
        pageCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        pageCell.Padding = 6f;
        pageCell.Phrase = new Phrase(content.Page);
        _pdfContentsTable.AddCell(pageCell);
      }

      doc.Add(_pdfContentsTable);
      doc.Add(new Chunk(Environment.NewLine));

      //** Reorder pages so that TOC will will be the second page in the doc
      //* right after the title page**/
      int toc = writer.PageNumber - 1;
      int total = writer.ReorderPages(null);
      int[] order = new int[total];

      for (int i = 0; i < total; i++) {
        if (i == 0) {
          order[i] = 1;
        } else if (i == 1) {
          order[i] = toc;
        } else {
          order[i] = i;
        }
      }

      writer.ReorderPages(order);
    }

    //#region GenerateQRCode
    ////http://www.texxtoor.com/qr/{guid}.
    //public System.Drawing.Image QRCodeSettings(string strURL) {

    //  QRCodeEncoder qrCodeEncoder = new QRCodeEncoder {
    //    QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
    //    QRCodeScale = 6,
    //    QRCodeVersion = 9,
    //    QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M
    //  };

    //  System.Drawing.Image image;
    //  String data = strURL;
    //  image = qrCodeEncoder.Encode(data);

    //  return image;
    //}



    //#endregion


  }


  //handle Image relative and absolute URL's
}