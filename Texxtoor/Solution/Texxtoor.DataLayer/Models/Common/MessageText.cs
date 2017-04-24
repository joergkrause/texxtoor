using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Common {

  /// <summary>
  /// Default message texts used in the internal mail system.
  /// </summary>
  [Table("MessageTexts", Schema = "Common")]
  public class MessageText : LocalizedEntityBase {

    [Required]
    [StringLength(50)]
    public string Identifier { get; set; }

    [Required]
    [StringLength(250)]
    public string Subject { get; set; }

    [StringLength(50)]
    public string SubjectParamHint { get; set; }

    [Required]
    [StringLength(5000)]
    public string Body { get; set; }

    [StringLength(50)]
    public string BodyParamHint { get; set; }


  }
}
