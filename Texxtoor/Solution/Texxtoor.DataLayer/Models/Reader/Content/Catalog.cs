using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Content {

  [Table("Catalog", Schema = "Reader")]
  public class Catalog : LocalizedHierarchyBase<Catalog> {

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Catalog_Description_Description", Description="Catalog_Description_Description_Helptext")]
    public string Description { get; set; }

    /// <summary>
    /// All published work in this category
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Catalog_Published_Published_Work", Description="Catalog_Published_Published_Work_Helptext")]
    public virtual List<Published> Published { get; set; }

    /// <summary>
    /// The application is required to support multi tenant environments. May be null for texxtoor catalog.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Catalog_Tenant_Assigned_Tenant", Description="Catalog_Tenant_Assigned_Tenant_Helptext")]
    public Tenant Tenant { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Catalog_Application_Assigned_Application", Description="Catalog_Application_Assigned_Application_Helptext")]
    public Application Application { get; set; }

    /// <summary>
    /// Helper for forms, dynamically set in BLL.
    /// </summary>
    [NotMapped]
    public bool Selected { get; set; }

    [NotMapped]
    public int PublishedCount {
      get {
        Func<Catalog, int> countChildren = null;
        countChildren = catalog => {
          var cnt = 0;
          if (catalog.HasChildren()) {
            cnt += catalog.Children.Sum(c => countChildren(c));
          }
          return catalog.Published.Count(p => p.FrozenFragments.Any()) + cnt;
        };
        return countChildren(this);
    }
    }

  }

}