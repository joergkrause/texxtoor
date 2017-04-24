namespace Texxtoor.Editor.ViewModels {
  public class ImageProperties {
    public ImageProperties() {
      KeepSize = true;
      Colors = new Colors { Brightness = 0, Contrast = 0, Saturation = 0, Hue = 0 }; ;
    }
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public bool KeepSize { get; set; }
    public string Effects { get; set; }
    public Colors Colors { get; set; }
    public Crop Crop { get; set; }
  }
  public class Colors {
    public string TransparentColor { get; set; }
    public int Brightness { get; set; }
    public int Contrast { get; set; }
    public int Hue { get; set; }
    public int Saturation { get; set; }
  }
  public class Crop {
    public int XOffset { get; set; }
    public int YOffset { get; set; }
    public int CropWidth { get; set; }
    public int CropHeight { get; set; }
  }
}
