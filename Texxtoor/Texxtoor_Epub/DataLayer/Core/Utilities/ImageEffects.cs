using System.Collections.Generic;

namespace Texxtoor.Editor.Utilities {
  public class ImageEffects {
    public List<string> Effects { get; set; }
    public ImageEffects() {
      Effects = new List<string>();
      Effects.Add("Grayscale");
      Effects.Add("Sepia");
      Effects.Add("Rotate Channels");
      Effects.Add("Invert");
      Effects.Add("Blur");
      Effects.Add("Gaussian Blur");
      Effects.Add("Convolution");
      Effects.Add("Edges");
    }
  }
}
