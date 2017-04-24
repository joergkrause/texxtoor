using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Web.Mvc;
using System.Xml.Serialization;
using Texxtoor.DataModels;

namespace Texxtoor.BaseLibrary.Core.BaseEntities {

  /// <summary>
  /// Used for static localization, each entry has a direct extension with localeid info.
  /// </summary>
  public abstract class LocalizedEntityBase : EntityBase {

    [StringLength(7)]
    [RegularExpression("[a-zA-Z]{2,3}(-[a-zA-Z]{2,3})?")]
    [ScaffoldColumn(false)]
    [XmlAttribute("Culture")]
    public string LocaleId { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string LocaleIdLocalName {
      get {
        return System.Globalization.CultureInfo.GetCultureInfo(LocaleId).Name;
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    [XmlIgnore]
    public bool IsFallback {
      get {
        return String.IsNullOrEmpty(LocaleId);
      }
    }

    [UIHint("CultureSelection")]
    [AdditionalMetadata("Name", "Locale")]
    [Display(ResourceType = typeof(ModelResources), Name = "LocalizedEntityBase_Culture_Culture", Description = "LocalizedEntityBase_Culture_Culture_Helptext")]
    [NotMapped]
    [XmlIgnore]
    public virtual CultureInfo Culture {
      get {
        return String.IsNullOrEmpty(LocaleId) ? CultureInfo.InvariantCulture : new CultureInfo(LocaleId);
      }
      set {
        LocaleId = value.Name;
      }
    }
  }

}

