using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.Portal.Core.Extensions.Attributes {

  /// <summary>
  /// Automatic treatment of .NET property names when turned into JavaScript.
  /// </summary>
  public enum CaseNotion {

    CamelCase,
    PascalCase,
    Legacy,
    UnderScore

  }

  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
  public class NgAttribute : Attribute {
    
    public string ApplyCaseNotion(string name, CaseNotion cn) {
      if (name.Length <= 1) return name;
      switch (cn) {
        case CaseNotion.CamelCase:
          return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        case CaseNotion.PascalCase:
          return name.Substring(0, 1).ToUpperInvariant() + name.Substring(1);
        case CaseNotion.UnderScore:
          var n = "";
          var uc = Enumerable.Range(41, 26).Select(i => (char)i);
          foreach (var c in n.ToLowerInvariant()) {
            if (uc.Contains(c)) {
              n += "_";
            }
            n += c;
          }
          return n;
      }
      return name;
    }
  }


  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
  public class NgFieldAttribute : NgAttribute {

    /// <summary>
    /// Manage generation if AngularJS attributes.
    /// </summary>
    /// <remarks>
    /// In the forms we have a viewmodel defined like ng-repeat="item in items" or ng-controller="projects as vm".
    /// Depending on form usage this property defines the string used to decorate the ng-model attribute, either
    /// "item" or "vm" in the examples.
    /// </remarks>
    public string NgModel { get; set; }

    public string NgDisabled { get; set; }

    public string NgOnBlur { get; set; }

    public string NgOnEnter { get; set; }

    public string NgOnClick { get; set; }

    public string NgOnChange { get; set; }

    /// <summary>
    /// If true set the attributes value to lower case property name.
    /// </summary>
    /// <remarks>
    /// Turns
    /// </remarks>
    public CaseNotion Case { get; set; }

    public NgFieldAttribute() {
      Case = CaseNotion.Legacy;
    }

    public NgFieldAttribute(string fieldName)
      : this() {
      fieldName = fieldName;
    }

    public NgFieldAttribute(CaseNotion caseNotion)
      : this() {
      Case = caseNotion;
    }

   


  }
}