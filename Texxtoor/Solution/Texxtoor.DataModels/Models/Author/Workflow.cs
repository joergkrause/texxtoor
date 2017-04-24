using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Models.Author {

  [Table("Workflow", Schema = "Content")]
  public class Workflow : EntityBase {

    /// <summary>
    /// Project this thread is associated with
    /// </summary>
    public Opus Opus { get; set; }

    public WorkflowStepDefinition CurrentStep { get; set; }
  }


}
