using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Content {

  [Table("Milestones", Schema = "Content")]
  public class Milestone : EntityBase {

    [Required]
    [ScaffoldColumn(false)]
    public virtual Opus Opus { get; set; }

    [Column(TypeName="datetime2")]
    [UIHint("FutureDate")]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_DateDue_Due_Date__Deadline_", Order = 4)]
    public DateTime DateDue { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_DateAssigned_Date_Assigned", Description="Milestone_DateAssigned_Date_Assigned_Helptext")]
    [DataType(DataType.Date)]
    [ScaffoldColumn(false)]
    public DateTime DateAssigned { get; set; }

    [Required(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "Milestone_Name_Validator_Required")]
    [StringLength(150)]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_Name_Name", Description = "Milestone_Name_Name", Order = 1)]
    [Watermark(typeof(ModelResources), "Milestone_Watermark_Name")]
    public string Name { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_Description_Description", Description = "Milestone_Description_Description_Helptext", Order = 2)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 5)]
    [AdditionalMetadata("Cols", 55)]
    [Watermark(typeof(ModelResources), "Milestone_Watermark_Description")]
    public string Description { get; set; }

    /// <summary>
    /// Responsible team members. Member may delegate this item to another one. If not assigned teamlead becomes the owner.
    /// </summary>
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_Owner_Responsible_Team_Member", Description = "Milestone_Owner_Responsible_Team_Member_Helptext", Order = 3)]
    [UIHint("TeamMembers")]
    public virtual TeamMember Owner { get; set; }

    /// <summary>
    /// Owner may add a comment for his/her work progress.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_Comment_Comments", Description = "Milestone_Comment_Comments_Helptext")]
    [ScaffoldColumn(false)]
    public string Comment { get; set; }

    /// <summary>
    /// Current working progress. The result is 0 = red, 1...99 = yellow, 100 = green
    /// </summary>
    [Range(0D, 100D)]
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_Progress_Progress", Description="Milestone_Progress_Progress_Helptext")]
    public int Progress { get; set; }

    /// <summary>
    /// Advice of an order (not mandatory). BLL may stop progress if previous milestone is not done.
    /// </summary>
    [UIHint("Milestones")]
    [Display(ResourceType = typeof(ModelResources), Name = "Milestone_NextMilestone_Next_Milestone__for_chain_", Description = "Milestone_NextMilestone_Next_Milestone_Helptext", Order = 5)]
    public Milestone NextMilestone { get; set; }

    /// <summary>
    /// Task is done
    /// </summary>
    [NotMapped]
    [ScaffoldColumn(false)]
    public bool Done { get { return Progress == 100; } set { Progress = value ? 100 : 0; } }

    /// <summary>
    /// Milestone passed
    /// </summary>
    [NotMapped]
    [ScaffoldColumn(false)]
    public bool Overdue { get { return DateTime.Now > DateDue; } }

  }

}
