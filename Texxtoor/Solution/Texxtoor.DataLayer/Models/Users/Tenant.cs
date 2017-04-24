using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.ViewModels.Users;

namespace Texxtoor.DataModels.Models.Users {
  
  /// <summary>
  /// Make the whole applicatio multi-tenant aware. Tenants can get administrative rights. 
  /// </summary>
  [Table("Tenants", Schema = "Common")]
  public class Tenant : EntityBase {

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    public IList<User> Users { get; set; }

    public IList<TemplateGroup> TemplateGroups { get; set; }

    public byte[] LogoImage { get; set; }

    private string _properties;
    public string Properties {
      get {
        if (String.IsNullOrEmpty(_properties)) {
          _properties =
            String.Format(
              "{{\"CssFile\":\"{0}\"}}",
              "");
        }

        return _properties;
      }
      set { _properties = value; }
    }

    # region Properties

    [NotMapped]
    public string CssFile
    {
      get { return System.Web.Helpers.Json.Decode<TenantProperties>(Properties).CssFile; }
      set {
        var p = System.Web.Helpers.Json.Decode<TenantProperties>(Properties);
        p.CssFile = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    # endregion

  }
}
