using System;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Texxtoor.Editor.Core {
  internal class TexxtoorTemplate : PdfPageEventHelper {
    float TEXTSIZE = 13;
    public PdfTemplate total;
    //I create a font object to use within my footer
    protected iTextSharp.text.Font FooterFont {
      get {
        return FontFactory.GetFont("Verdana", 7);
      }
    }
    protected iTextSharp.text.Font HeaderFont {
      get {
        return FontFactory.GetFont("Verdana", TEXTSIZE);
      }
    }

    //I create a font object to use within my footer
    protected BaseFont BaseFnt {
      get {
        return BaseFont.CreateFont(@"c:\windows\fonts\verdana.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
      }
    }

    public override void OnOpenDocument(PdfWriter writer, Document document) {
      total = writer.DirectContent.CreateTemplate(100, 100);
      total.BoundingBox = new iTextSharp.text.Rectangle(-20, -20, 100, 100);
    }

    public override void OnCloseDocument(PdfWriter writer, Document document) {
      total.BeginText();
      total.SetFontAndSize(BaseFnt, TEXTSIZE);
      total.SetTextMatrix(0, 0);
      int pageNumber = writer.PageNumber - 1;
      total.ShowText("(" + pageNumber + ")");
      total.EndText();
    }

    //override the OnStartPage event handler to add our header
    public override void OnStartPage(PdfWriter writer, Document doc) {
      PdfContentByte cb = writer.DirectContent;
      cb.SaveState();
      string text = writer.PageNumber.ToString();
      float textBase = doc.Top;

      //Sidnummrering
      cb.BeginText();
      cb.SetFontAndSize(BaseFnt, TEXTSIZE);
      float adjust = BaseFnt.GetWidthPoint("0", TEXTSIZE);
      cb.SetTextMatrix(doc.Right - TEXTSIZE - adjust, textBase + 40);
      cb.ShowText(text);
      cb.EndText();

      //Datum
      cb.BeginText();
      cb.SetFontAndSize(BaseFnt, TEXTSIZE);
      cb.SetTextMatrix(doc.Right - 90, textBase + 20);
      // cb.NewlineText();
      cb.ShowText("Datum: " + DateTime.Now.ToShortDateString());
      cb.EndText();

      cb.AddTemplate(total, doc.Right - adjust, textBase + 40);
      cb.RestoreState();

      PdfPTable headerTbl = new PdfPTable(1);
      headerTbl.TotalWidth = doc.PageSize.Width - (PdfSettings.MarginRight + PdfSettings.MarginLeft);

      headerTbl.WriteSelectedRows(0, -1, 0, (doc.PageSize.Height + PdfSettings.MarginBottom), writer.DirectContent);

    }

    public override void OnEndPage(PdfWriter writer, Document doc) {

      PdfPTable footerTbl = new PdfPTable(6);
      //set the width of the table to be the same as the document
      footerTbl.TotalWidth = doc.PageSize.Width - (PdfSettings.MarginRight + PdfSettings.MarginLeft);

      Paragraph para = new Paragraph("Company name", FooterFont);
      para.Add(Environment.NewLine);
      para.Add("ORG.NR.");
      para.Add(Environment.NewLine);
      para.Add("12200-2335");
      PdfPCell cell = new PdfPCell(para);
      cell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
      cell.BorderWidthTop = .5f;
      // cell.PaddingLeft = 20;
      footerTbl.AddCell(cell);

      // 2nd cell 
      para = new Paragraph("POSTADRESS POSTAL ADDRESS", FooterFont);
      para.Add(Environment.NewLine);
      para.Add("SE-113 24 Stockholm");
      para.Add(Environment.NewLine);
      para.Add("Sweden");
      cell = new PdfPCell(para);
      cell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
      cell.BorderWidthTop = .5f;
      footerTbl.AddCell(cell);

      // 3d cell 
      para = new Paragraph("BESÖK VISITORS", FooterFont);
      para.Add(Environment.NewLine);
      para.Add("Snallagatan 5");
      cell = new PdfPCell(para);
      cell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
      cell.BorderWidthTop = .5f;
      footerTbl.AddCell(cell);

      // 4th cell 
      para = new Paragraph("TELEFON TELEPHONE", FooterFont);
      para.Add(Environment.NewLine);
      para.Add("NAT 08 123 45 00");
      para.Add(Environment.NewLine);
      para.Add("INT +46 8 123 45 00");
      cell = new PdfPCell(para);
      cell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
      cell.BorderWidthTop = .5f;
      footerTbl.AddCell(cell);

      // 5th cell 
      para = new Paragraph("TELEFAX", FooterFont);
      para.Add(Environment.NewLine);
      para.Add("NAT 08 12 23 45");
      para.Add(Environment.NewLine);
      para.Add("INT +46 8 12 31 45");
      cell = new PdfPCell(para);
      cell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
      cell.BorderWidthTop = .5f;
      footerTbl.AddCell(cell);

      // 6th cell 
      para = new Paragraph("INTERNET", FooterFont);
      para.Add(Environment.NewLine);
      para.Add("info@info.se");
      para.Add(Environment.NewLine);
      para.Add("www.website.se");
      cell = new PdfPCell(para);
      //cell.HorizontalAlignment = Element.ALIGN_RIGHT;
      cell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
      cell.BorderWidthTop = .5f;
      footerTbl.AddCell(cell);

      //write the rows out to the PDF output stream.
      footerTbl.WriteSelectedRows(0, -1, PdfSettings.MarginLeft, (PdfSettings.MarginBottom), writer.DirectContent);
    }



  }
}