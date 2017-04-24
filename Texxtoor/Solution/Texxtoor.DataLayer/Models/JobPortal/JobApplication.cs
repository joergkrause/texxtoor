using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.JobPortal {

  /// <summary>
  /// An applicant applies to a job ad.
  /// </summary>
  /// <remarks>
  /// </remarks> 
  [Table("JobApplication", Schema = "JobPortal")]
  public class JobApplication : EntityBase {

    public User Applicant { get; set; }

    public string Letter { get; set; }

    public bool Cancelled { get; set; }

    public bool Rejected { get; set; }

    public string Answer { get; set; }


  }

}