using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Cms;

namespace Texxtoor.DataModels.Models.Users {

  /// <summary>
  /// Generic roles in the system, such as Author, Contributor, Reader
  /// </summary>  
  [Table("Roles", Schema = "Common")]
  public class Role : IdentityRole<int, TexxtoorUserRole> {

    public IList<CmsMenu> Menus { get; set; }

    public IList<CmsMenuItem> MenuItems { get; set; }


    # region EntityBase

    [ScaffoldColumn(false)]
    [Column(TypeName = "datetime2")]
    [SuppressPropertyCopy]
    [Display(ResourceType = typeof(ModelResources), Name = "EntityBase_CreatedAt_Created_At")]
    public DateTime CreatedAt { get; set; }

    [ScaffoldColumn(false)]
    [Column(TypeName = "datetime2")]
    [SuppressPropertyCopy]
    [Display(ResourceType = typeof(ModelResources), Name = "EntityBase_ModifiedAt_Modified_At")]
    public DateTime ModifiedAt { get; set; }

    # endregion

    /// <summary>
    /// Hard coded mapping from <see cref="UserRole"/> to Name.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Role_UserRole_Generic_User_Role", Description = "Role_UserRole_Generic_User_Role_Helptext")]
    [NotMapped]
    public UserRole UserRole {
      get {
        // minimum role
        var role = UserRole.Guest;
        if (!String.IsNullOrEmpty(Name) && Enum.TryParse(Name, out role)) {
          return role;
        }
        return UserRole.Unknown;
      }
      set { Name = value.ToString(); }
    }


  }

}