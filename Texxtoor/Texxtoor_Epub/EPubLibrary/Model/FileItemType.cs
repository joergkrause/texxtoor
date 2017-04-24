using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.BaseLibrary.EPub.Model {
  public enum FileItemType {
    JPEG,
    GIF,
    PNG,
    XHTML,
    CSS,
    /// <summary>
    /// Font file
    /// </summary>
    OTF,
    /// <summary>
    /// Font file
    /// </summary>
    TTF,
    /// <summary>
    /// http://wiki.mobileread.com/wiki/XPGT
    /// </summary>
    XPGT
  }
}
