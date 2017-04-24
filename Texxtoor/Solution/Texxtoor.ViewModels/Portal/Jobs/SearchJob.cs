using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.JobPortal;

namespace Texxtoor.ViewModels.Jobs {
  public class SearchJob {

    [StringLength(50)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "SearchJob_SearchTerm_Search_Terms", Description="SearchJob_SearchTerm_Search_Terms_Helptext")]
    public string SearchTerm { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "SearchJob_ContractTypes_Contract_Type", Description="SearchJob_ContractTypes_Contract_Type_Helptext")]
    [UIHint("JobContractType")]
    public JobContractType ContractTypes { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "SearchJob_WorkTypes_Work_Type", Description="SearchJob_WorkTypes_Work_Type_Helptext")]
    [UIHint("JobStatuteType")]
    public JobStatuteType WorkTypes { get; set; }
  }
}
