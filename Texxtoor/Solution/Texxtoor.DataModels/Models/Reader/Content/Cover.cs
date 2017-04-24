using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Models.Reader.Content {
  
  [ComplexType]
  public class Cover {
    private float _baseFontSize;

    public Cover() {
      BaseFontSize = 64F;
    }

    public byte[] CoverImage { get; set; }

    private bool _useCoverBackgroundTemplate;

    public bool UseCoverBackgroundTemplate {
      get { return _useCoverBackgroundTemplate; }
      set {
        _useCoverBackgroundTemplate = value;
        if (_useCoverBackgroundTemplate && String.IsNullOrEmpty(CoverBackgroundTemplate)) {
          CoverBackgroundTemplate = "cover.png";
        }
      }
    }

    [StringLength(200)]
    public string CoverBackgroundTemplate { get; set; }

    [StringLength(10)]
    public string ForeColor { get; set; }

    [StringLength(10)]
    public string BackColor { get; set; }

    public float BaseFontSize {
      get { return _baseFontSize == 0 ? 64F : _baseFontSize; }
      set { if (value > 0) {_baseFontSize = value;} }
    }

    [StringLength(200)]
    public string FontFamily { get; set; }

    public byte[] GetFinalCoverBytes(Published publ) {
      byte[] finalCover;
      using (var ms = new MemoryStream()) {
        publ.CoverImage.GetFinalCover(publ).Save(ms, ImageFormat.Png);
        ms.Position = 0;
        finalCover = ms.ToArray();
      }
      return finalCover;
    }

    /// <summary>
    /// Create the final cover on the fly
    /// </summary>
    /// <returns></returns>
    public Image GetFinalCover(Published publ) {
      var fontFamily = FontFamily ?? "Arial";
      var fColor = ForeColor == null ? Color.White : ColorTranslator.FromHtml(ForeColor);
      var bColor = BackColor == null ? Color.Teal : ColorTranslator.FromHtml(BackColor);      
      // A5 300 dpi
      var w = 1748F;
      var h = 2482F;
      var font = new Font(fontFamily, BaseFontSize);
      var font1 = new Font(fontFamily, BaseFontSize / 2);
      var font2 = new Font(fontFamily, BaseFontSize / 3 * 2);
      Image img = new Bitmap((int)w, (int)h, PixelFormat.Format24bppRgb);
      using (var g = Graphics.FromImage(img)) {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        byte[] fileCover = null;
        if (CoverImage == null) {
          if (UseCoverBackgroundTemplate) {
            var template = HttpContext.Current.Server.MapPath(String.Format("~/App_Data/Templates/cover/{0}", CoverBackgroundTemplate));
            fileCover = File.ReadAllBytes(template);
          }
        }
        else {
          fileCover = CoverImage;
        }
        if (fileCover == null) {
          using (var b = new SolidBrush(bColor)) {
            g.FillRectangle(b, 0, 0, w, h); // use background color
          }
        }
        else {
          using (var b = new SolidBrush(Color.White)) {
            g.FillRectangle(b, 0, 0, w, h); // all white
          }
          using (var templateImageStream = new MemoryStream(fileCover, 0, fileCover.Length)) {
            var templateImage = Image.FromStream(templateImageStream);
            g.DrawImage(templateImage, 0, 0, templateImage.Width, templateImage.Height);
          }
        }
        Action<KeyValuePair<string, float>, float, float, Font> drawFunc = (d, sh, dh, ft) => {
          var xz = 200; // to center use this: w / 2 - d.Value / 2;
          var yz = sh;  // to arrange use this: h / 3 - sh / 2 + dh;
          using (var b = new SolidBrush(fColor)) {
            g.DrawString(d.Key, ft, b, new PointF(xz, yz));
          }
        };
        var sf = g.MeasureString(publ.Title, font);
        // title does not fit, break in two lines
        var titleWords = publ.Title.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var measures = titleWords.ToDictionary(t => t, t => g.MeasureString(t + " ", font).Width);
        if (measures.Sum(t => t.Value) > w) {
          // does not fit in; strategy: take first ones until width exceeded, place, treat remainder
          var startWidth = 0F;
          var firstLine = measures.TakeWhile(t => {
            startWidth += t.Value;
            return startWidth < w;
          }).ToDictionary(t => t.Key, t => t.Value);
          var lastLine = measures.Except(firstLine).ToList();
          drawFunc(new KeyValuePair<string, float>(
            String.Join(" ", firstLine.Select(t => t.Key).ToArray()), firstLine.Sum(t => t.Value)),
            sf.Height, 0F, font);
          drawFunc(new KeyValuePair<string, float>(
            String.Join(" ", lastLine.Select(t => t.Key).ToArray()), lastLine.Sum(t => t.Value)),
            sf.Height, sf.Height + 10, font); // 10 is an additional gap between lines
        } else {
          drawFunc(new KeyValuePair<string, float>(publ.Title, sf.Width), 600F, 0F, font);
        }
        var titleHeight = sf.Height;
        if (!String.IsNullOrEmpty(publ.SubTitle)) {
          sf = g.MeasureString(publ.SubTitle, font2);
          drawFunc(new KeyValuePair<string, float>(publ.SubTitle, sf.Width), 700F + titleHeight, 0F, font2);
        }
        var publAuthors = "";
        if (publ.Authors.Any()) {
          // get authors only
          publAuthors = String.Join(", ", publ.Authors
            .Select(u => String.Format("{0} {1}", u.Profile.FirstName, u.Profile.LastName)).ToArray());
        }
        if (!String.IsNullOrEmpty(publAuthors)) {
          sf = g.MeasureString(publAuthors, font1);
          drawFunc(new KeyValuePair<string, float>(publAuthors, sf.Width), 1100F, 0F, font1);
        }
      }
      return img;
    }

  }
}
