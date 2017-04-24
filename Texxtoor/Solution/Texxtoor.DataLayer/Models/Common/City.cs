using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Common {

  [Table("City", Schema = "Common")]
  public class City : EntityBase {

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(200)]
    public string District { get; set; }

    public long Population { get; set; }
  }
}
