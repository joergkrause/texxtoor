using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  [Table("WorkflowStepDefinition", Schema = "Content")]
  public class WorkflowStepDefinition : LocalizedEntityBase {

    [Required]
    [StringLength(32)]
    public string WorkflowSet { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    [StringLength(512)]
    public string Description { get; set; }

    public WorkflowStepDefinition NextStep { get; set; }

    public bool RequiredStep { get; set; }

    public List<Role> Roles { get; set; }

  }


}
