using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Content {

  #region -= Resources =-
  [Table("Resources", Schema = "Content")]
  public abstract class Resource : HierarchyBase<Resource> {

    [Required]
    public Guid ResourceId { get; set; }

    [ForeignKey("ProjectId")]
    [Required]
    public virtual Project Project { get; set; }

    [Column("Project_Id")]
    public int ProjectId { get; set; }

    /// <summary>
    /// This methods sets the project for this and all children. Used for bulk store/import and copy operations.
    /// </summary>
    /// <param name="prj"></param>
    public void SetProjectDeep(Project prj) {
      if (HasChildren()) {
        SetProject(Children, prj);
      }
      Project = prj;
    }

    private static void SetProject(IEnumerable<Resource> children, Project prj) {
      foreach (var item in children) {
        item.Project = prj;
        if (item.HasChildren()) {
          SetProject(item.Children, prj);
        }
      }
    }

    /// <summary>
    /// Once deleted the resource appear in the Trash section
    /// </summary> 
    public bool Deleted { get; set; }

    /// <summary>
    /// If False it's shared among others in the project. Default is FALSE.
    /// </summary>
    public bool Private { get; set; }

    /// <summary>
    /// Owner of the file. Folder and file operations, such as copy, inherit this value.
    /// If empty it becomes a public object that is related to the system.
    /// </summary>
    [ForeignKey("OwnerId")]
    public virtual User Owner { get; set; }

    [Column("Owner_Id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// This methods sets the owner for this and all children.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="force">If the owner in a deeper level is already set it gets overwritten.</param>
    public void SetOwnerDeep(User owner, bool force = true) {
      if (HasChildren()) {
        SetOwner(Children, owner, force);
      }
      Owner = owner;
    }

    private static void SetOwner(IEnumerable<Resource> children, User owner, bool force) {
      foreach (var item in children) {
        if (item.Owner == null || force) {
          item.Owner = owner;
        }
        if (item.HasChildren()) {
          SetOwner(item.Children, owner, force);
        }
      }
    }

    /// <summary>
    /// Organize the resources in various categories, the resource manager treats this as "volumes".
    /// </summary>
    public TypeOfResource TypesOfResource { get; set; }

    public string GetLocalizedTypeOfResource(){
      return typeof(TypeOfResource).GetField(TypesOfResource.ToString()).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().Single().GetName();
    }

    public virtual bool CanRead() {
      return true;
    }

    public virtual bool CanWrite() {
      return true;
    }

    public virtual bool CanDelete() {
      return true;
    }

    public bool HasParent() {
      return (Parent != null);
    }

    public ResourceFolder GetParent() {
      return Parent as ResourceFolder;
    }

    /// <summary>
    /// Summarize all childrens file sizes.
    /// </summary>
    /// <returns></returns>
    public long Size() {
      if (HasChildren()) {
        return GetChildSize(Children);
      }
      return 0;
    }

    private long GetChildSize(IEnumerable<Resource> children) {
      var enumerable = children as IList<Resource> ?? children.ToList();
      if (children == null || !enumerable.Any()) return 0;
      var size = enumerable.OfType<ResourceFile>().Sum(c => c.FileSize);
      var folders = enumerable.OfType<ResourceFolder>();
      var resourceFolders = folders as IList<ResourceFolder> ?? folders.ToList();
      if (resourceFolders.Any()) {
        size += resourceFolders.Sum(item => GetChildSize(item.Children));
      }
      return size;
    }

    /// <summary>
    /// Creates the full path in the hierarchy to get a unique name apart from id.
    /// </summary>
    [NotMapped]
    public string FullName {
      get {
        var path = new List<string> {Name};
        var next = this;
        while (next.Parent != null) {
          next = (ResourceFolder)next.Parent;
          path.Add(next.Name);
        }
        path.Reverse();
        return String.Join("/", path.ToArray());
      }
    }

  }

  [Table("Resources", Schema = "Content")]
  public class ResourceFolder : Resource {
  }

  [Table("Resources", Schema = "Content")]
  public class ResourceFile : Resource {

    public ResourceFile() {
      Published = new List<Published>();
      Metadata = new Dictionary<string, object>();
    }

    [Required]
    [StringLength(80)]
    public string MimeType { get; set; }

    /// <summary>
    /// The size, pulled from blob storage and cached.
    /// </summary>
    [ScaffoldColumn(false)]
    [NotMapped]
    public long FileSize { get; set; }

    [ScaffoldColumn(false)]
    [NotMapped]
    public string FileSizeString {
      get {
        if (FileSize > Math.Pow(2, 20)) {
          return String.Format("{0:0.00} MB", FileSize/Math.Pow(2, 20));
        }
        if (FileSize > Math.Pow(2, 10)) {
          return String.Format("{0:0.00} KB", FileSize / Math.Pow(2, 10));
        }
        return String.Format("{0} Bytes", FileSize);
      }
    }

    public bool IsImage() {
      return this.MimeType.StartsWith("image");
    }

    /// <summary>
    /// Resource might be associated with published work for downloads.
    /// </summary>
    public IList<Published> Published { get; set; }

    [NotMapped]
    public bool IsAssociatedWithPublished {
      get {
        return Published.Any();
      }
    }

    /// <summary>
    /// If the blob storage has meta data we can expose these data here to support views.
    /// </summary>
    [NotMapped]
    public IDictionary<string, object> Metadata { get; set; }

  }
  #endregion

}
