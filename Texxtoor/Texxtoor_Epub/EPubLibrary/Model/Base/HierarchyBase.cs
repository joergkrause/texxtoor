using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Texxtoor.Models.BaseEntities.Epub
{

  public abstract class HierarchyBase<T> : EntityBase, IHierarchyBase<T> where T: class {

    [StringLength(150)]
    public string Name { get; set; }

    public int OrderNr { get; set; }
    public virtual T Parent { get; set; }
    public virtual List<T> Children { get; set; }

    [NotMapped]
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

