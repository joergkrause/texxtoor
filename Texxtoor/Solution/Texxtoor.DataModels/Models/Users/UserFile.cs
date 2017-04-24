using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Content {

  #region -= Resources =-
  [Table("UserFile", Schema = "Common")]
  public class UserFile : EntityBase {

    public UserFile() {
      Folder = "Archive";
    }

    [Required]
    public Guid ResourceId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserFile_Name_File_Name", Description="UserFile_Name_File_Name_Helptext")]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserFile_Folder_Folder_Name", Description="UserFile_Folder_Folder_Name_Helptext")]
    public string Folder { get; set; }

    [StringLength(29)]
    public string MimeType { get; set; }

    /// <summary>
    /// Once deleted the resource appear in the Trash section
    /// </summary> 
    [Display(ResourceType = typeof(ModelResources), Name = "UserFile_Deleted_Is_Deleted", Description="UserFile_Deleted_Is_Deleted_Helptext")]
    public bool Deleted { get; set; }

    /// <summary>
    /// If False it's shared among others in the project. Default is FALSE.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "UserFile_Private_Is_Private", Description="UserFile_Private_Is_Private_Helptext")]
    public bool Private { get; set; }

    /// <summary>
    /// Owner of the file. Folder and file operations, such as copy, inherit this value.
    /// If empty it becomes a public object that is related to the system.
    /// </summary>
    [Required]
    public virtual User Owner { get; set; }

    public virtual bool CanRead() {
      return true;
    }

    public virtual bool CanWrite() {
      return false;
    }

    public virtual bool CanDelete() {
      return true;
    }


  }

  #endregion

}
