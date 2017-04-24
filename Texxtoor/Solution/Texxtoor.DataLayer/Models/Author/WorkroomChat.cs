using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Functions;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// A discussion board within the project, regardless of a specific work.
  /// </summary>
  [Table("WorkroomChat", Schema = "Content")]
  public class WorkroomChat : Discussion<WorkroomChat> {

    [ScaffoldColumn(false)]
    public override string Name {
      get {
        return Subject;
      }
      set {
        base.Subject = value;
      }
    }

    /// <summary>
    /// Project this thread is associated with
    /// </summary>
    [Required]
    public Project Project { get; set; }

    /// <summary>
    /// Mark as private, default is public
    /// </summary>
    [ScaffoldColumn(false)]
    public override bool Private { get; set; }

    /// <summary>
    /// Limit a publicitly visible comment to own groups only.
    /// </summary>
    [ScaffoldColumn(false)]
    public override bool GroupOnly { get; set; }

    /// <summary>
    /// As the comment's thread owner one can close the thread and avoid further comments.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "WorkroomChat_Closed_Thread_Closed", Description = "WorkroomChat_Closed_Thread_Closed_Helptext", Order=100)]
    public override bool Closed { get; set; }

    [ScaffoldColumn(false)]
    public override int Mood  { get; set; }

  }


}
