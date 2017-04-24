using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.JobPortal {

  /// <summary>
  /// Advertisments of employeers looking for people.
  /// </summary>
  /// <remarks>
  /// We register regular seekers, and advertisers through the user management, both get reader and guest roles.
  /// Job Portal is integral part of the contribution module.
  /// </remarks> 
  [Table("JobAdvertisment", Schema = "JobPortal")]
  public class JobAdvertisment : EntityBase {

    /// <summary>
    /// Issuing companies profile
    /// </summary>
    [Required]
    public UserProfile CompanyProfile { get; set; }

    [StringLength(32)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_Reference_Reference_Code", Description="JobAdvertisment_Reference_Reference_Code_Helptext")]
    public string Reference { get; set; }

    [Required]
    [ScaffoldColumn(false)]
    public User Contact { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_Categories_Job_Category", Description="JobAdvertisment_Categories_Job_Category_Helptext")]
    [UIHint("JobCategories")]
    public virtual List<JobCategory> Categories { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    [DataType(DataType.Date)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_VisibleFrom_Visible_From", Description="JobAdvertisment_VisibleFrom_Visible_From_Helptext")]
    [UIHint("FutureDate_NotNull")]
    [AdditionalMetadata("Offset", 1)]
    public DateTime VisibleFrom { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    [DataType(DataType.Date)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_VisibleTo_Visible_Until", Description="JobAdvertisment_VisibleTo_Visible_Until_Helptext")]
    [UIHint("FutureDate_NotNull")]
    [AdditionalMetadata("Offset", 7)]
    public DateTime VisibleTo { get; set; }

    [Required]
    [StringLength(120)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_JobTitle_Job_Title", Description="JobAdvertisment_JobTitle_Job_Title_Helptext")]
    public string JobTitle { get; set; }

    [StringLength(500)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_JobShortDescription_Short_Description", Description="JobAdvertisment_JobShortDescription_Short_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 75)]
    public string JobShortDescription { get; set; }

    [Required]
    [StringLength(5000)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_JobLongDescription_Verbose_Description", Description="JobAdvertisment_JobLongDescription_Verbose_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 10)]
    [AdditionalMetadata("Cols", 75)]
    public string JobLongDescription { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_Attachment_Attachment", Description="JobAdvertisment_Attachment_Attachment_Helptext")]
    [ScaffoldColumn(false)]
    public Guid Attachment { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_Banner_Advertisment_Banner_or_Logo", Description="JobAdvertisment_Banner_Advertisment_Banner_or_Logo_Helptext")]    
    public byte[] Banner { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_Regions_Limit_to_regions", Description="JobAdvertisment_Regions_Limit_to_regions_Helptext")]
    public List<string> Regions { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_ContractTypes_Contract_Types", Description="JobAdvertisment_ContractTypes_Contract_Types_Helptext")]
    public JobContractType ContractTypes { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "JobAdvertisment_WorkTypes_Work_Types", Description="JobAdvertisment_WorkTypes_Work_Types_Helptext")]
    public JobStatuteType WorkTypes { get; set; }

    /// <summary>
    /// Applications made online by applicants
    /// </summary>
    public List<JobApplication> Applications { get; set; }

  }

}