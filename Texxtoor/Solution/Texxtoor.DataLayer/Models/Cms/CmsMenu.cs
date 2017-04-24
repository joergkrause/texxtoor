using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Security;
using System.Web.Mvc;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Globalization;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Cms {

  /// <summary>
  /// Menu elements
  /// </summary>
  [Table("Menus", Schema = "Cms")]
  public class CmsMenu : LocalizedEntityBase {
    public CmsMenu() {
      this.MenuItems = new List<CmsMenuItem>();
    }

    [Display(Name = "Applications", Description = "Application this menu is available in.")]
    public virtual Application Application { get; set; }

    [Display(Name = "Menu Items")]
    public IList<CmsMenuItem> MenuItems { get; set; }

    [Display(Name = "Navigate URL", Description = "The URL or Action string the item navigates to.")]
    [StringLength(500)]
    public string NavigateUrl { get; set; }

    [NotMapped]
    public bool IsExternalUrl {
      get { return !String.IsNullOrEmpty(NavigateUrl); }
    }

    [Required]
    [Display(Name = "Menu Title")]
    [StringLength(256)]
    public string Title { get; set; }

    [Display(Name = "Order", Description = "Order of entries")]
    [Required]
    public int Order { get; set; }

    [Display(Name = "Page", Description = "The page that's connected to this item. Leave empty for parent menus that don't show own content. Same page might be available for one or more menus.")]
    public CmsPage Page { get; set; }

    [Required]
    [Display(Name = "Menu Type", Description = "The Type names a section where the menu is being materialized (must match PartialView name)")]
    [StringLength(128)]
    public string Type { get; set; }

    [Display(Name = "Roles", Description = "Roles that this entry is shown to.")]
    public IList<Role> Roles { get; set; }

    [NotMapped]
    public string TypeLowered {
      get {
        return Type.ToLowerInvariant();
      }
    }

    [Display(Name = "Description")]
    public string Description { get; set; }

    [Display(Name = "Dynamic Data Field")]
    public string DynamicData { get; set; }

    [NotMapped]
    public string DynamicDataResolved { get; set; }

    [Display(Name = "Feature Set")]
    public string FeatureSet { get; set; }
    
    /// <summary>
    /// A menu specific CSS information added to static CSS
    /// </summary>
    [StringLength(256)]
    public string Style { get; set; }

  }

}

