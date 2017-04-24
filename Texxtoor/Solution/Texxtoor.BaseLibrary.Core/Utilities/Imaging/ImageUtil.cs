using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using AForge.Imaging.Filters;
using System.Drawing.Drawing2D;

namespace Texxtoor.BaseLibrary.Core.Utilities.Imaging {
  public static class ImageUtil {

    public static Image ApplyImageProperties(byte[] blobContent, ImageProperties properties) {
      Bitmap image = null;
      try {
        if (blobContent.Length <= 16) {
          image =
            (Bitmap)
            Image.FromStream(File.Open(HttpContext.Current.Server.MapPath("~/App_Data/Templates/no_image.png"), FileMode.Open),
                             false, false);
        } else {
          using (var ms = new MemoryStream(blobContent)) {
            image = (Bitmap)Image.FromStream(ms, false, false);
            image = AForge.Imaging.Image.Clone(image, PixelFormat.Format24bppRgb); // "A" preserves transparency
            if (properties.Crop != null && 0 < (properties.Crop.CropWidth + properties.Crop.CropHeight)) {
              var filter =
                new AForge.Imaging.Filters.Crop(new Rectangle(properties.Crop.XOffset, properties.Crop.YOffset,
                                                              properties.Crop.CropWidth, properties.Crop.CropHeight));
              image = filter.Apply(image);
            }
            if (properties.ImageWidth != properties.OriginalWidth || properties.ImageHeight != properties.OriginalHeight) {
              var filter = new ResizeBicubic(properties.ImageWidth, properties.ImageHeight);
              image = filter.Apply(image);
            }
            if (properties.Colors != null) {
              if (properties.Colors.TransparentColor != null)
                image.MakeTransparent(ColorTranslator.FromHtml("#" + properties.Colors.TransparentColor));
              var brightness = properties.Colors.Brightness;
              var bfilter = new BrightnessCorrection(brightness);
              bfilter.ApplyInPlace(image);
              var contrast = properties.Colors.Contrast;
              var cfilter = new ContrastCorrection(contrast);
              cfilter.ApplyInPlace(image);
              if (properties.Colors.Hue != 0) {
                var hue = properties.Colors.Hue;
                var filter = new HueModifier(hue);
                filter.ApplyInPlace(image);
              }
              var saturation = properties.Colors.Saturation;
              var sfilter = new SaturationCorrection(saturation * 0.01f);
              sfilter.ApplyInPlace(image);
            }

            # region Effects

            if (!String.IsNullOrEmpty(properties.Effects)) {
              var effects = properties.Effects.Split(';');
              foreach (var item in effects) {
                switch (item) {
                  case "Grayscale":
                    var g = new Grayscale(0.2125, 0.7154, 0.0721);
                    image = g.Apply(image);
                    break;
                  case "Sepia":
                    var s = new Sepia();
                    image = AForge.Imaging.Image.Clone(image, PixelFormat.Format24bppRgb);
                    s.ApplyInPlace(image);
                    break;
                  case "Rotate Channels":
                    image = AForge.Imaging.Image.Clone(image, PixelFormat.Format24bppRgb);
                    var r = new RotateChannels();
                    r.ApplyInPlace(image);
                    break;
                  case "Invert":
                    var i = new Invert();
                    i.ApplyInPlace(image);
                    break;
                  case "Blur":
                    var b = new Blur();
                    b.ApplyInPlace(image);
                    break;
                  case "Gaussian Blur":
                    var gb = new GaussianBlur(4, 11);
                    gb.ApplyInPlace(image);
                    break;
                  case "Convolution":
                    int[,] kernel = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
                    var c = new Convolution(kernel);
                    c.ApplyInPlace(image);
                    break;
                  case "Edges":
                    var e = new Edges();
                    e.ApplyInPlace(image);
                    break;
                }
              }
            }

            # endregion
          }
        }
      } catch
        (Exception) {
        // TODO: Handle Exceptions here
      }
      return image;
    }

    #region -= Images Helper =-

    public static byte[] GetThumbnailImage(Stream image, int scaleToW, int scaleToH) {
      if (image == null) return null;
      try {
        using (var img = Image.FromStream(image)) {
          float w = img.Width;
          float h = img.Height;
          var scaleW = (w > scaleToW);
          var scaleH = (h > scaleToH);
          float nPercentW = scaleW ? (float)scaleToW / w : 1F;
          float nPercentH = scaleH ? (float)scaleToH / h : 1F;
          var nPercent = (scaleH && scaleW)
                              ? Math.Min(nPercentH, nPercentW)
                              : (scaleW ? nPercentW : nPercentH);
          var thmb = img.GetThumbnailImage(Convert.ToInt32(w * nPercent), Convert.ToInt32(h * nPercent), null,
                                           IntPtr.Zero);
          int destWidth = (int)(img.Width * nPercentW);
          int destHeight = (int)(img.Height * nPercent);
          int destX = Convert.ToInt32((scaleToW - destWidth) / 2);
          int destY = Convert.ToInt32((scaleToH - destHeight) / 2);
          // this is scaled thumbnail, now we project onto a surface
          var bmp = new Bitmap(scaleToW, scaleToH, PixelFormat.Format24bppRgb);
          bmp.SetResolution(thmb.HorizontalResolution, thmb.VerticalResolution);
          var photo = Graphics.FromImage(bmp);
          photo.Clear(Color.White);
          photo.InterpolationMode = InterpolationMode.HighQualityBicubic;
          photo.DrawImage(thmb,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(0, 0, thmb.Width, thmb.Height),
            GraphicsUnit.Pixel);
          using (var newMs = new MemoryStream()) {
            bmp.Save(newMs, ImageFormat.Png);
            return newMs.ToArray();
          }
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message, "Convert Image from Work or EPub Failed");
        return null;
      }
    }


    public static byte[] GetThumbnailImage(byte[] image, int scaleToW, int scaleToH) {
      if (image == null) return null;
      var ms = new MemoryStream(image);
      return GetThumbnailImage(ms, scaleToW, scaleToH);
    }

    public static byte[] GetStaticImage(string fullPath) {
      var ms = new MemoryStream();
      var img = Image.FromFile(fullPath);
      img.Save(ms, ImageFormat.Png);
      return ms.ToArray();
    }

    #endregion

  }
}
