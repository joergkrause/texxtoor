using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Texxtoor.Models.BaseEntities;

namespace Texxtoor.Models {

  #region -= Resources =-

  [Table("Resources")]
  public class Resource : EntityBase {

    public byte[] Content { get; set; }

    [Required]
    public virtual Document OwnerDocument { get; set; }

    /// <summary>
    /// Organize the resources in various categories, the resource manager treats this as "volumes".
    /// </summary>
    public TypeOfResource TypesOfResource { get; set; }

    public string Name { get; set; }

    [Required]
    public string MimeType { get; set; }

    /// <summary>
    /// The size, pulled from blob storage and cached.
    /// </summary>
    [ScaffoldColumn(false)]
    [NotMapped]
    public long FileSize { get; set; }

    public bool IsImage() {
      return this.MimeType.StartsWith("image");
    }

  }

  #endregion

}
