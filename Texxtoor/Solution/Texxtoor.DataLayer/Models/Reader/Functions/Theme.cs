using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  /// <summary>
  /// Anybody can create a theme and add groups to it. Themes are public in any way.
  /// </summary>
  [Table("Themes", Schema = "Reader")]
  public class Theme : LocalizedEntityBase {

    /// <summary>
    /// The themes internal name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Description on website
    /// </summary>
    [Required]
    public string Description { get; set; }


    /// <summary>
    /// Groups this theme is assigned to.
    /// </summary>
    public IList<ReaderGroup> ReaderGroups { get; set; }

  }


}