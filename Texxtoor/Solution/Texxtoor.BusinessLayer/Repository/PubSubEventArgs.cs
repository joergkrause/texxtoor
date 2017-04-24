using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.BaseLibrary.Repository {
  public class PubSubEventArgs : EventArgs {

    public string Target { get; set; }

    public int Value { get; set; }

  }
}
