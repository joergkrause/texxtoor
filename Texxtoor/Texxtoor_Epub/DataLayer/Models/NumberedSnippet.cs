using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using Texxtoor.Models;

namespace Texxtoor.Editor.Models {

  /// <summary>
  /// This is just a marker interface with nothing to store, just support the builder
  /// </summary>
  [Table("Elements")]
  public abstract class NumberedSnippet : Snippet, INumberingSchema {

    [NotMapped]
    public string Label { get; set; }
    [NotMapped]
    public int Major { get; set; }
    [NotMapped]
    public string MajorString {
      get { return Major == 0 ? "" : Major.ToString(CultureInfo.InvariantCulture); }
    }
    [NotMapped]
    public int Minor { get; set; }

    [NotMapped]
    public string MinorString {
      get { return Minor == 0 ? "" : Minor.ToString(CultureInfo.InvariantCulture); }
    }

    [NotMapped]
    public char Separator { get; set; }
    [NotMapped]
    public string Divider { get; set; }
    [NotMapped]
    public bool IncludeParent { get; set; }
  }

  public sealed class NumberingSchema : INumberingSchema {

    public string Label { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public char Separator { get; set; }
    public string Divider { get; set; }
    public bool IncludeParent { get; set; }
  }

  public interface INumberingSchema {

    string Label { get; set; }
    int Major { get; set; }
    int Minor { get; set; }
    char Separator { get; set; }
    string Divider { get; set; }
    bool IncludeParent { get; set; }
  }


}
