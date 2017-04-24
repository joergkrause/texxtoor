using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Common {

  /// <summary>
  /// This table stores running workflows. However, each workflow is just a simple decision engine and terminates immediately after falling through 
  /// to a final decision about the wizards current step. After the final step has been passed, the workflow removes itself from this table. The
  /// workflow even adds itself here.
  /// </summary>
  [Table("WizardWorkflows", Schema = "Common")]
  public class WizardWorkflow : LocalizedEntityBase {

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public User Owner { get; set; }

    /// <summary>
    /// serialized .NET object used to restart the workflow
    /// </summary>
    public string StartUpObject { get; set; }

  }
}
