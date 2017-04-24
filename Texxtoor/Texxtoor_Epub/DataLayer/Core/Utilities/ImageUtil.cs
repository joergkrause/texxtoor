using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge.Imaging.Filters;
using Texxtoor.Editor.ViewModels;

namespace Texxtoor.Editor.Utilities {
  public static class ImageUtil {

    public static Image ApplyImageProperties(byte[] blobContent, ImageProperties properties) {
      Bitmap image = null;
      try {
        using (var ms = new MemoryStream(blobContent)) {
          image = (Bitmap)System.Drawing.Image.FromStream(ms, false, false);
          image = AForge.Imaging.Image.Clone(image, PixelFormat.Format24bppRgb);
          if (properties.Crop != null) {
            AForge.Imaging.Filters.Crop filter = new AForge.Imaging.Filters.Crop(new Rectangle(properties.Crop.XOffset, properties.Crop.YOffset, properties.Crop.CropWidth, properties.Crop.CropHeight));
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
              HueModifier filter = new HueModifier(hue);
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
      } catch (Exception) {
        // TODO: Handle Exceptions here
      }
      return image;
    }

  }
}
