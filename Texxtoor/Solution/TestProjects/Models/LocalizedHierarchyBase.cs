using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace LinqDemo.Models {


  public abstract class LocalizedHierarchyBase<T> : HierarchyBase<T> where T : class {

    [StringLength(7)]
    [RegularExpression("[a-zA-Z]{2,3}(-[a-zA-Z]{2,3})?")]
    [ScaffoldColumn(false)]
    public string LocaleId { get; set; }

  }

}

