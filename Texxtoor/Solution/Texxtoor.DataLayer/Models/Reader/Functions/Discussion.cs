using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  /// <summary>
  /// Discussions. Definition allows just more precious subclasses.
  /// </summary>  
  public abstract class Discussion<T> : HierarchyBase<T> where T : Discussion<T> {

    /// <summary>
    /// The themes internal name
    /// </summary>
    [Required]
    [StringLength(200)]
    [AdditionalMetadata("Length", 35)]
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_Subject_Subject", Description="Discussion_Subject_Subject_Helptext", Order= 1)]
    public string Subject { get; set; }

    /// <summary>
    /// Mark as private, default is public
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_Private_Is_Private", Description="Discussion_Private_Is_Private_Helptext", Order= 20)]
    public virtual bool Private { get; set; }

    /// <summary>
    /// Limit a publicitly visible comment to own groups only.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_GroupOnly_For_Group_Only", Description = "Discussion_GroupOnly_For_Group_Only_Helptext", Order = 21)]
    public virtual bool GroupOnly { get; set; }

    /// <summary>
    /// As the comment's thread owner one can close the thread and avoid further comments.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_Closed_Closed_Audience", Description = "Discussion_Closed_Closed_Audience_Helptext", Order = 22)]
    public virtual bool Closed { get; set; }

    /// <summary>
    /// The text of the comment.
    /// </summary>
    [StringLength(3000)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 5)]
    [AdditionalMetadata("Cols", 55)]
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_Content_Content", Description="Discussion_Content_Content_Helptext", Order=10)]
    public string Content { get; set; }

    /// <summary>
    /// Users are allowed to change own comments, but we save the complete history.
    /// </summary>
    public List<ThreadHistory> ThreadHistory { get; set; }

    /// <summary>
    /// The original owner of the comment. Does not inherit.
    /// </summary>
    public virtual User Owner { get; set; }

    # region Advanced Social Functions

    [StringLength(255)]
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_Tags_Tags", Description = "Discussion_Tags_Tags_Helptext", Order=3)]
    public string Tags { get; set; }

    /// <summary>
    /// The mood of a specific thread.
    /// </summary>
    [Range(0, 5)]
    [UIHint("Mood")]
    [Display(ResourceType = typeof(ModelResources), Name = "Discussion_Mood_Mood", Description="Discussion_Mood_Mood_Helptext", Order = 10)]
    public virtual int Mood { get; set; }

    /// <summary>
    ///  Returns the average mood of the whole thread hierarchy.
    /// </summary>
    /// <returns></returns>
    public decimal GetMoods() {
      var result = (decimal)Mood;
      if (HasChildren()) {
        var flat = Children.Traverse(p => p.Mood > 0).Select(p => p.Mood).Cast<decimal>().ToList();
        flat.Add(result);
        result = flat.Average();
      }
      return result;
    }

    # endregion

  }



}