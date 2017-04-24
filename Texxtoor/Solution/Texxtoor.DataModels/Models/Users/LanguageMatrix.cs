using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Users {

  [Table("LanguageMatrix", Schema = "Common")]
  public class LanguageMatrix : EntityBase {

    public LanguageMatrix() {
      LocaleId = String.Empty;
      LanguageLevel = Texxtoor.DataModels.Models.Users.LanguageLevel.Fluent;
    }

    /// <summary>
    /// A locale the related user (via profile) provides.
    /// </summary>
    [Required]
    [StringLength(10)]
    public string LocaleId { get; set; }

    [Required]
    public LanguageLevel LanguageLevel { get; set; }

    [Required]
    public UserProfile Profile { get; set; }

  }

}
