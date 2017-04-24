using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Texxtoor.DataModels;

namespace Texxtoor.BaseLibrary.Core.BaseEntities {

  public abstract class HierarchyBase<T> : EntityBase, IHierarchyBase<T> where T : class {

    protected HierarchyBase() {
      Children = new List<T>();
    }

    [StringLength(150)]
    [Display(ResourceType = typeof(ModelResources), Name = "Element_Name_Name", Description = "Element_Name_Name_Helptext", Order = 1)]
    [FilterUIHint("StringFilter", "MVC")]
    public virtual string Name { get; set; }

    [ScaffoldColumn(false)]
    public int OrderNr { get; set; }

    [ScaffoldColumn(false)]
    public virtual T Parent { get; set; }

    [ScaffoldColumn(false)]
    public virtual List<T> Children { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public int Level {
      get {
        int i = 1;
        IHierarchyBase<T> t = this;
        while (t.Parent != null) {
          t = (IHierarchyBase<T>)t.Parent;
          i++;
        }
        return i;
      }
    }

    public override string ToString() {
      return Name ?? base.ToString();
    }

    public bool HasChildren() {
      return (Children != null && Children.Count > 0);
    }

  }

}

