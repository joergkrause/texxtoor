using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BaseLibrary.EPub.Model {

  public enum ProgressionDirection {
    Ltr,
    Rtl,
    Default
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class EPubElement : Attribute {

    public string Name { get; set; }

    public EPubElement(string name) {
      Name = name;
    }
  }

  [AttributeUsage(AttributeTargets.Property)]
  public class EPubAttribute : Attribute {

    public string Name { get; set; }

    public EPubAttribute(string name) {
      Name = name;
    }
  }


}
