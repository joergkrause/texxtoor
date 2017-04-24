using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.Editor.Core {
  public class CreateImageArguments : EventArgs {

    public string FileName { get; set; }

    public byte[] Content { get; set; }
  }
}
