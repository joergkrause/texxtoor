using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Texxtoor.Models.BaseEntities;

namespace Texxtoor.Models {

  #region -= Elements =-

  [Table("Elements")]
  public abstract class Element : LocalizedHierarchyBase<Element> {

    /// <summary>
    /// The actual data in the Element
    /// </summary>
    public byte[] Content { get; set; }

    [NotMapped]
    public abstract string WidgetName { get; }

    /// <summary>
    /// Supports the process of building frozen fragments. Hard coded.
    /// </summary>
    [NotMapped]
    public abstract FragmentType ProposedFragmentType { get; }

    [NotMapped]
    public virtual string RawContent {
      get {
        return (Content != null) ? Encoding.UTF8.GetString(Content) : String.Empty;
      }
    }
  }

  #endregion

}
