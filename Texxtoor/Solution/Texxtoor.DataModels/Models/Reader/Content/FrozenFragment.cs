using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models.Content;
using System.Text;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using System.IO;
using System.Drawing.Imaging;
using System.Web;

namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// Store of each fragment published by authors as part of the publishing process.
  /// </summary>
  /// <remarks>
  /// This is the final store for all related data, such as HTML content, images, videos, CSS from templates and more.
  /// The fragments are hierarchical to determine a distinct order.
  /// </remarks>
  [Table("FrozenFragments", Schema = "Reader")]
  public class FrozenFragment : HierarchyBase<FrozenFragment> {

    public FrozenFragment(){
      SingularEntity = true;
    }

    /// <summary>
    /// The ID of the work (refers to Published table).
    /// </summary>
    /// <remarks>Only top level element with no parent must have this field set.</remarks>
    public virtual Published Published { get; set; }

    /// <summary>
    /// The entity in the work. Assuming the content is finally built using Epub, we assign the future manifest's id here.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string ItemHref { get; set; }

    /// <summary>
    /// Content as raw byte stream. 
    /// </summary>
    /// <remarks>Usually this is a chapter's content as HTML 5.
    /// Children of the fragment provide static resources, such as figures' content.
    /// </remarks>
    public byte[] Content { get; set; }

    /// <summary>
    /// Define whether it's allowed to treat this fragment as a singular entity.
    /// </summary>
    /// <remarks>
    /// Singular entities can be added to work, searched by the matrix, and purchased separately.
    /// </remarks>
    public bool SingularEntity { get; set; }

    /// <summary>
    /// For convenience we map the Name to Title.
    /// </summary>
    [NotMapped]
    [ScaffoldColumn(false)]
    public string Title {
      get { return Name; }
      set { Name = value; }
    }

    # region --== Properties ==--

    /// <summary>
    /// The fragments type determines how to treat in further steps (Content, Resource, MetaData)
    /// </summary>
    /// <remarks>
    /// Some types, such as Resources, usually does not form a hierarchy.
    /// </remarks>
    public FragmentType TypeOfFragment { get; set; }

    # endregion

  }


}