using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.EasyAuthor.Models {
  public class ImageDto {
    public int Width { get; set; }

    public int Height { get; set; }

    public float ResH { get; set; }

    public float ResV { get; set; }

    public string Warn { get; set; }

    public string Px { get; set; }

    public string Src { get; set; }
  }
}
