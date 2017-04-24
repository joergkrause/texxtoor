using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Common {

  [Table("Language", Schema = "Common")]
  public class Language : EntityBase {

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public bool IsOfficial { get; set; }

    public float Percentage { get; set; }

  }
}
