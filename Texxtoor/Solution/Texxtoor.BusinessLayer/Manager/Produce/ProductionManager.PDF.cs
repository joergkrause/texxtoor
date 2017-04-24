using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Gma.QrCodeNet.Encoding;
using Texxtoor.BaseLibrary.Pdf;
using Texxtoor.BaseLibrary.Pdf.Prince;

namespace Texxtoor.BusinessLayer {


  /// <summary>
  /// This class contains all methods to create actual content, such as PDF, EPub, iBook, and more.
  /// </summary>
  /// <remarks>
  /// We have these input elements:
  /// - Opus, let authors see their product at any phase during creation
  /// - Published, used by authors to see their final product immediately
  /// - OrderProduct, created by readers when purchasing a book
  /// 
  /// Opus is internally only and will never go outside
  /// Published is also what we use to distribute in major book shops, such as Amazon
  /// OrderProduct is a personalized copy we send directly
  /// 
  /// All three channels go through an abstract printer layer that creates a "printable" and that's what the PDF
  /// engine gets to do theirs. 
  /// 
  /// The <see cref="Printable"/> object collects all relevant data. The subsequent steps create the output from these data.
  /// </remarks>
  public partial class ProductionManager {


    public byte[] CreatePdfCover(Printable printable) {
      var coverTpl = Encoding.UTF8.GetString(printable.Templates.Single(t => t.InternalName == Printable.TemplatePartial.BookCover).Content);
      var tempFiles = new List<string>();
      // create and write cover background
      var coverImg = Path.Combine(printable.TempStorePath, "bg" + Guid.NewGuid() + ".png");
      using (var coverFile = File.Create(coverImg)) {
        coverFile.Write(printable.CoverImage, 0, printable.CoverImage.Length);
      }
      tempFiles.Add(coverImg);
      // create and write QR code
      // TODO: Make this dynamic with additional data
      // create and write barcode, if needed
      byte[] barcodeImg = null;
      if (printable.HasIsbn) {
        barcodeImg = GetBarCode(printable.Isbn, 158, 77, printable.Title);
      }
      string bio = null;
      if (printable.AdditionalAuthorInfo.Any()) {
        bio = String.Join(" ", printable.AdditionalAuthorBiographies.ToArray());
      }
      var publishedAuthors = "";
      if (printable.AdditionalAuthorInfo.Any()) {
        publishedAuthors = String.Join(", ", printable.AdditionalAuthorInfo.ToArray());
      }
      if (printable.PermaLink != null) {
        printable.Updatelink = String.Format(WebConfigurationManager.AppSettings["production:CoverPermaLink"], printable.PermaLink);
        printable.QRImg = CreateQRCode(printable.Updatelink);
      }
      printable.BarCodeImg = barcodeImg;
      printable.CoverImg = coverImg;
      printable.HasToc = false;
      printable.HasIndex = false;
      printable.HasImprint = false;
      var bc = new BoomConverter();
      var html = bc.GenerateHtml(printable, Printable.TemplatePartial.BookCover);
      return MakePdf(html);
    }

    private byte[] CreateQRCode(string data) {
      // generating a barcode here. Code is taken from QrCode.Net library
      QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
      QrCode qrCode = new QrCode();
      qrEncoder.TryEncode(data, out qrCode);
      //GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Four), Brushes.Black, Brushes.White);

      var memoryStream = new MemoryStream();
      //renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, memoryStream);

      // very important to reset memory stream to a starting position, otherwise you would get 0 bytes returned
      memoryStream.Position = 0;
      return memoryStream.ToArray();
    }

    public byte[] CreatePdfContent(Printable printable) {
      _issueReport = new Dictionary<string, string>();
      // the prince converter optimized for boom templates
      var converter = new BoomConverter();
      converter.IssueReport += converter_IssueReport;
      // TODO: Add dynamic modifications to final document here
      var html = converter.GenerateHtml(printable, Printable.TemplatePartial.DocumentXml);
      return MakePdf(html);
    }


    private byte[] MakePdf(string html) {
      try {
        var prince = new Prince(HttpContext.Current.Server.MapPath("~/CommandLine/PrinceEngine/bin/prince.exe"));
        prince.SetLicenseFile(HttpContext.Current.Server.MapPath("~/CommandLine/PrinceEngine/license/license.dat").Replace("\\", "/"));
        //prince.SetLicenseFile("../license/license.dat");
        prince.SetHtml(true);
        //prince.SetFileRoot(tempPath.EndsWith("\\") ? tempPath.Substring(0, tempPath.Length - 1) : tempPath);
        prince.SetLog(HttpContext.Current.Server.MapPath("~/App_Data/Blobs/prince.log"));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(html)) { Position = 0 }) {
          using (var pdf = new MemoryStream()) {
            prince.ConvertMemoryStream(ms, pdf);
            return pdf.ToArray();
          }
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        return null;
      } finally {
      }
    }


  }
}