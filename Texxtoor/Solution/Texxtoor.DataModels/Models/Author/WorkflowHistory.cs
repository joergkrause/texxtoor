using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Author {

  [Table("WorkflowHistory", Schema = "Content")]
  public class WorkflowHistory : EntityBase {

    /// <summary>
    /// Workflow that contains this item
    /// </summary>
    [Required]
    public Workflow Workflow { get; set; }

    /// <summary>
    /// Member that is assigned to this item
    /// </summary>
    [Required]
    public TeamMember ResponsibleMember { get; set; }



  }


}
