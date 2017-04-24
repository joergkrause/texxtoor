using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Web.Mvc;
using Texxtoor.DataModels;

namespace Texxtoor.BaseLibrary.Core.BaseEntities {


  public abstract class LocalizedHierarchyBase<T> : HierarchyBase<T> where T : class {

    [StringLength(7)]
    [Display(ResourceType = typeof(ModelResources), Name = "LocalizedHierarchyBase_LocaleId_Culture", Description = "LocalizedHierarchyBase_LocaleId_Culture_Helptext", Order = 2)]
    [RegularExpression("[a-zA-Z]{2,3}(-[a-zA-Z]{2,3})?")]
    [ScaffoldColumn(false)]
    public string LocaleId { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool IsFallback {
      get {
        return String.IsNullOrEmpty(LocaleId);
      }
    }

    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "LocalizedHierarchyBase_LocaleId_Culture", Description = "LocalizedHierarchyBase_LocaleId_Culture_Helptext", Order = 2)]
    [UIHint("CultureSelection")]
    [AdditionalMetadata("Name", "Culture")]
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

