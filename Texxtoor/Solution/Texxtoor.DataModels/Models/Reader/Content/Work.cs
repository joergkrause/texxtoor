using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// The thing a reader finally gets is called a "work". It's complete regarding the content, but does not has a specific media type,
  /// apart from being stored internally as EPUB 3.
  /// </summary>
  /// <remarks>
  /// The work is decoupled in any way from former steps. There is no steady connection to published works, collections, and so on.
  /// Works are ALWAYS private, user specific containers for FINALIZED works. The storage format is EPUB and any transformation to
  /// a final product goes from here.
  /// </remarks>
  [Table("Work", Schema = "Reader")]
  public class Work : EntityBase {

    /// <summary>
    /// Reader can name the book internally as he like.
    /// </summary>
    [Required]
    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "Work_Name_Name", Description = "Work_Name_Name_Helptext", Order = 1)]
    [Watermark(typeof(ModelResources), "Work_Watermark_Name")]
    public string Name { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Work_Note_Description", Description = "Work_Note_Description_Helptext", Order = 2)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Watermark(typeof(ModelResources), "Work_Watermark_Note")]
    public string Note { get; set; }

    /// <summary>
    /// The content that belongs to this collection, as a collection of references.
    /// </summary>
    public virtual ICollection<WorkingFragment> Fragments { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; }

    public virtual ICollection<Comment> Comments { get; set; }

    /// <summary>
    /// This collection belongs to this user. Group member might have access to it, but cannot own it. Is NULL for public work's collections.
    /// </summary>
    [Display(Name = "Owner")]
    [ScaffoldColumn(false)]
    public virtual User Owner { get; set; }

    /// <summary>
    /// If true group members can have access to the collection.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "ContentCollection_Public_Collection_is_Public", Description = "ContentCollection_Public_Collection_is_Public_Helptext", Order = 3)]
    public bool Public { get; set; }

    /// <summary>
    /// If true group members can edit and contribute to the collection. Collection must not be private to being shared.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "ContentCollection_Shared_Shared_Collection", Description = "ContentCollection_Shared_Shared_Collection_Helptext", Order = 4)]
    public bool Shared { get; set; }

    ///// <summary>
    ///// Each work has exactly one collection of data, which contains a hierarchy of fragments.
    ///// </summary>
    //[ScaffoldColumn(true)]
    //[Display(ResourceType = typeof(ModelResources), Name = "Work_Collection_Content_Collection", Description = "Work_Collection_Content_Collection_Helptext", Order = 3)]
    //[UIHint("Collection")]
    //public virtual ContentCollection Collection { get; set; }

    /// <summary>
    /// Stores a reference to an external book that has been copied to the db from particular user.
    /// </summary>
    /// <remarks>
    /// While the frozenfragments and working fragments contain selectable content the resources remain in the epub for reference.
    /// </remarks>
    [ScaffoldColumn(false)]
    public EpubBook ExternalBook { get; set; }

    /// <summary>
    /// Is true if this work has no association with anything used internally. User might have been uploaded some epub stuff.
    /// </summary>
    /// <remarks>
    /// The list of fragments is extracted from uploaded epub on-the-fly while uploading. It might be changed later by user interaction.
    /// </remarks>
    [Display(ResourceType = typeof(ModelResources), Name = "Work_Extern_External", Description="Work_Extern_External_Helptext")]
    [ScaffoldColumn(false)]
    [NotMapped]
    public WorkType Extern {
      get {
        if (ExternalBook != null)
          return WorkType.External;
        return Published != null ? WorkType.Published : WorkType.Custom;
      }
    }

    /// <summary>
    /// Products created based on this work
    /// </summary>
    public List<Product> Products { get; set; }

    /// <summary>
    /// The published text that this work based on, can be null
    /// </summary>
    public Published Published { get; set; }

  }

}