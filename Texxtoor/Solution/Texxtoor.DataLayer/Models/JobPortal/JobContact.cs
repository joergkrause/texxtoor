using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.JobPortal {

  /// <summary>
  /// The contact of an employeer the application can send its application to.
  /// </summary>
  [Table("JobContact", Schema = "JobPortal")]
  public class JobContact : EntityBase {

    public JobContact() {
      ShowBusinessData = true;
    }

    /// <summary>
    /// The person who is responsible for an ad
    /// </summary>
    [Required]
    public User ContactPerson { get; set; }

    /// <summary>
    /// Whether or not allow people to contact outside the platform.
    /// </summary>
    public bool ShowBusinessData { get; set; }


  }

}