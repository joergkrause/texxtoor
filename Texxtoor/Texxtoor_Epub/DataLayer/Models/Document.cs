using System.ComponentModel.DataAnnotations.Schema;

namespace Texxtoor.Models {

  [Table("Elements")]
  public class Document : Element {

    public Document() {
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Opus"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Meta; }
    }

  }

}
