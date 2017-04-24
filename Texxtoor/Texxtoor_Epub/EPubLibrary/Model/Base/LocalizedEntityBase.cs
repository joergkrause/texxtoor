using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Texxtoor.Models.BaseEntities.Epub
{

  /// <summary>
  /// Used for static localization, each entry has a direct extension with localeid info.
  /// </summary>
  public class LocalizedEntityBase : EntityBase {

    [Display(Name = "Culture", Description = "The Culture this content is assigned to. Leave empty to make a fallback culture.")]
    [StringLength(7)]
    [RegularExpression("[a-zA-Z]{2,3}(-[a-zA-Z]{2,3})?")]
    public string LocaleId { get; set; }

    [NotMapped]
    public bool IsFallback {
      get {
        return String.IsNullOrEmpty(LocaleId);
      }
    }

    [NotMapped]
    public CultureInfo Culture {
      get {
        return String.IsNullOrEmpty(LocaleId) ? CultureInfo.InvariantCulture : new CultureInfo(LocaleId);
      }
      set {
        LocaleId = value.Name;
      }
    }
  }

}

