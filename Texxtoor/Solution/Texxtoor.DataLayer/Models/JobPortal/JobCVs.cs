using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.JobPortal {

  // Curriculum vitae of applicants
  [Table("JobCV", Schema = "JobPortal")]
  public class JobCV : EntityBase {

    [Required]
    public UserProfile UserProfile { get; set; }

    [Required]
    public DateTime VisibleFrom { get; set; }

    [Required]    
    public DateTime VisibleTo { get; set; }

    [Required]
    [StringLength(120)]
    public string CvTitle { get; set; }

    [StringLength(500)]
    public string CvShortDescription { get; set; }

    [Required]
    [StringLength(5000)]
    public string CvLongDescription { get; set; }

    public Guid Attachment { get; set; }


  }

}