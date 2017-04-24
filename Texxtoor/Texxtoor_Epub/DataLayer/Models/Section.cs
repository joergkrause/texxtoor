using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.Editor.Models;
using Texxtoor.Models.Attributes;

namespace Texxtoor.Models {

  #region -= Elements =-

  [Table("Elements")]
  [SnippetElement("<h{0}>{2}{3}{4}{5}{1}</h{0}>", "Deep", "RawContent", "ParentNumbering", "MinorString", "Divider", "Label")]
  public class Section : NumberedSnippet {

    # region Support for building hierarchy on import 

    private int _designatedLevelOnImport;

    public void SetDesignatedLevelOnImport(int level) {
      _designatedLevelOnImport = level;
    }

    public int GetDesignatedLevelFromImport() {
      return _designatedLevelOnImport;
    }

    # endregion

    [NotMapped]
    public int Deep {
      get {
        int level = 0;
        var parent = Parent;
        while (parent != null) {
          level++;
          parent = parent.Parent;
        }
        return level;
      }
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Section"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Html; }
    }

    [NotMapped]
    public string ParentNumbering { get; set; }

  }

  #endregion

}
