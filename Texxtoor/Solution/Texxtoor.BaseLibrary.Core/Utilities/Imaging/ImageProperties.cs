namespace Texxtoor.BaseLibrary.Core.Utilities.Imaging {

  /// <summary>
  /// Handle the dynamic properties an author can apply to a figure.
  /// </summary>
  public class ImageProperties {

    public ImageProperties() {
      KeepSize = true;
      Colors = new ColorProperties { Brightness = 0, Contrast = 0, Saturation = 0, Hue = 0 };
      _crop = new ResizeProperties();
    }

    private ResizeProperties _crop;

    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public bool KeepSize { get; set; }
    public string Effects { get; set; }
    public ColorProperties Colors { get; set; }

    public ResizeProperties Crop {
      get {
        if (_crop == null) {
          _crop = new ResizeProperties {
            XOffset = ImageWidth,
            YOffset = ImageHeight,            
            CropWidth = 0,
            CropHeight = 0
          };
        }
        return _crop;
      }
      set {
        _crop = value;
      }
    }

    public class ColorProperties {
      public string TransparentColor { get; set; }
      public int Brightness { get; set; }
      public int Contrast { get; set; }
      public int Hue { get; set; }
      public int Saturation { get; set; }
    }
    public class ResizeProperties {
      public int XOffset { get; set; }
      public int YOffset { get; set; }
      public int CropWidth { get; set; }
      public int CropHeight { get; set; }
    }


  }
}
