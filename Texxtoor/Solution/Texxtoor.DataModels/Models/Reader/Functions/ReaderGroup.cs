using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Security;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  [Table("ReaderGroups", Schema = "Reader")]
  public class ReaderGroup : EntityBase {

    /// <summary>
    /// The groups internal name
    /// </summary>
    [Required]
    [StringLength(128)]
    [Display(ResourceType = typeof (ModelResources), Name = "ReaderGroup_Name_Group_s_Name", Order = 1)]
    public string Name { get; set; }

    /// <summary>
    /// Description on website
    /// </summary>
    [Required]
    [StringLength(1024)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Display(ResourceType = typeof (ModelResources), Name = "ReaderGroup_Description_Description", Order = 2)]
    public string Description { get; set; }

    /// <summary>
    /// Group does not accept new members (admin can change this property).
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ReaderGroup_Closed_Closed", Order = 3)]
    public bool Closed { get; set; }

    /// <summary>
    /// Group is visible on the website to all other users.
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ReaderGroup_Public_Public", Order = 4)]
    public bool Public { get; set; }

    /// <summary>
    /// The one who owns the group, can  be delegated to another owner.
    /// </summary>
    [ScaffoldColumn(false)]
    public User Owner { get; set; }

    /// <summary>
    /// The administrator of this group. At least one required.
    /// </summary>
    [ScaffoldColumn(false)]
    public IList<User> Admins { get; set; }

    /// <summary>
    /// All members of the group
    /// </summary>
    [ScaffoldColumn(false)]
    public IList<User> Members { get; set; }

    /// <summary>
    /// This group addresses these particular themes.
    /// </summary>
    [ScaffoldColumn(false)]
    public IList<Theme> Themes { get; set; }


  }

}