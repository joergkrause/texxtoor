using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Serialization;

namespace LinqDemo.Models {

  /// <summary>
  /// Used for static localization, each entry has a direct extension with localeid info.
  /// </summary>
  public abstract class LocalizedEntityBase : EntityBase {

    [StringLength(7)]
    [RegularExpression("[a-zA-Z]{2,3}(-[a-zA-Z]{2,3})?")]
    [ScaffoldColumn(false)]
    public string LocaleId { get; set; }

  }

}

