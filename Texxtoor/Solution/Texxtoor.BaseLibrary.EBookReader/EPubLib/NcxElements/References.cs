using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Obsolete("Deprecated in EPub 3 Spec")]
  public enum ReferenceType {
    Cover,
    Text
  }

  [Obsolete("Deprecated in EPub 3 Spec")]
  [ComplexType]
  public class References {
    public ReferenceType Type { get; set; }
    public string Title { get; set; }
    public string Href { get; set; }
  }
}
