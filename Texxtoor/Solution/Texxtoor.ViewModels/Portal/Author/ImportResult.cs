using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.ViewModels.Author {

  public class ImportResult {

    public List<string> ItemsMissed { get; set; }
    public List<string> ItemsProcessed { get; set; }
    public int OpusId { get; set; }
  }

}
