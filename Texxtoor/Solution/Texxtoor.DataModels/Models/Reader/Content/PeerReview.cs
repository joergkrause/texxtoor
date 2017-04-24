using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Content {


  [Table("PeerReviews", Schema = "Reader")]
  public abstract class Review : EntityBase {
    [Required]
    [ScaffoldColumn(false)]
    public User Reviewer { get; set; }

    [Range(1, 6)]
    [Display(ResourceType = typeof(ModelResources), Name = "PeerReview_Level", Description = "PeerReview_Level_Level_Helptext", Order = 1)]
    [UIHint("Stars")]
    public int Level { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "PeerReview_Comment", Description = "PeerReview_Comment_Comment_Helptext", Order = 2)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    public string Comment { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "PeerReview_Approved", Description = "PeerReview_Approved_Is_Approved_Helptext", Order = 3)]
    public virtual bool Approved { get; set; }

    /// <summary>
    /// A review is always related to a published work.
    /// </summary>
    public Published PublishedWork { get; set; }    
  }

  /// <summary>
  /// Contains the finally published opuses, regardless the usage of a reader. Reader have readonly access here.
  /// </summary>
  [Table("PeerReviews", Schema = "Reader")]
  public class PeerReview : Review {

    /// <summary>
    /// Override to avoid that a reviewer can approve the value for itself.
    /// </summary>
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "PeerReview_Approved_Approve_Review", Description = "PeerReview_Approved_Approve_Review_Helptext", Order = 10)]
    public override bool Approved { get; set; }

  }

  /// <summary>
  /// An additional class that extends the reviews to those provided by users. Currently it's no different but the approved column is not disclosed to the reader.
  /// </summary>
  [Table("PeerReviews", Schema = "Reader")]
  public class ReaderReview : Review {


    [ScaffoldColumn(false)]
    public override bool Approved { get; set; }

    /// <summary>
    /// A counter for helpfulness
    /// </summary>
    [ScaffoldColumn(false)]
    public int WasHelpful { get; set; }




  }

}