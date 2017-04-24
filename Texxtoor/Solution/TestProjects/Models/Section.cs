using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinqDemo.Models {

  #region -= Elements =-

  [Table("Elements")]
  public class Section : Snippet {

    public Section() {
      Counter = 1;
    }

    [NotMapped]
    public bool IsBoilerplate
    {
      get
      {
        var parent = Parent;
        while (!(parent is Opus)) {
          parent = parent.Parent;
        }
        return ((Opus) parent).IsBoilerplate;
      }
    }

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

    public int Counter { get; set; }

  }

  #endregion

}
