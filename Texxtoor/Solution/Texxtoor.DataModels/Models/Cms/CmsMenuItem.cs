using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Cms {

  [Table("MenuItems", Schema = "Cms")]
  public class CmsMenuItem : LocalizedEntityBase {

    /// <summary>
    /// All menus the page ist related to
    /// </summary>
    public IList<CmsMenu> Menu {
      get;
      set;
    }

    [Display(Name = "Application", Description = "For what application type.")]
    public Application Application { get; set; }

    [Display(Name = "Menu Item", Description = "The title that appears on the screen.")]
    [StringLength(256)]
    public string Title { get; set; }

    [Display(Name = "Navigate URL", Description = "The URL or Action string the item navigates to.")]
    [StringLength(500)]
    public string NavigateUrl { get; set; }

    [NotMapped]
    public bool IsExternalUrl {
      get { return !String.IsNullOrEmpty(NavigateUrl); }
    }

    [Display(Name = "Icon Class", Description = "The Name of the embedded icon class")]
    [StringLength(50)]
    public string IconClass { get; set; }

    [Display(Name = "Order", Description = "Order of entries")]
    [Required]
    public int Order { get; set; }

    [Display(Name = "Visible", Description = "Make item invisible (not rendered).")]
    public bool Visible { get; set; }

    [Display(Name = "Page", Description = "The page that's connected to this item. Leave empty for external URLs. Same page might be available for one or more menu items.")]
    public virtual CmsPage Page { get; set; }

    [Display(Name = "Roles", Description = "Roles that this entry is shown to.")]
    public IList<Role> Roles { get; set; }

    public string GetUrl() {
      if (this.IsExternalUrl) {
        var abstractContext = new System.Web.HttpContextWrapper(System.Web.HttpContext.Current);
        return UrlHelper.GenerateContentUrl(NavigateUrl, abstractContext);
      }
      return this.NavigateUrl;
    }

    /// <summary>
    /// Menu item is shown only when this set is activated in web.config.
    /// </summary>
    [Display(Name = "Feature Set")]
    [StringLength(32)]
    public string FeatureSet { get; set; }

    /// <summary>
    /// For metro style items an additional text can be added.
    /// </summary>
    [StringLength(512)]
    public string Description { get; set; }

    [NotMapped]
    public string DecodedDescription {
      get { return System.Web.HttpUtility.HtmlDecode(Description); }
    }

    /// <summary>
    /// A comma separated list of items from a propertybag provided by the navigationcontroller
    /// </summary>
    /// <remarks>
    /// Works in conjunction with <see cref="Description"/>. The elements appear in order and replace the placeholders in description.
    /// For example: "You have {0} books" in the description field and "BookCount" in DynamicData looks for PropertyBag("BookCount") and
    /// replaces the value in the string.
    /// </remarks>
    [StringLength(256)]
    public string DynamicData { get; set; }

    [NotMapped]
    public string DynamicDataResolved { get; set; }

    # region Dyna Menu Items

    [StringLength(50)]
    public string DynaTitle { get; set; }
    [StringLength(150)]
    public string DynaDesc { get; set; }
    [StringLength(80)]
    public string DynaData { get; set; }
    [StringLength(120)]
    public string DynaNavi { get; set; }

    /// <summary>
    /// The actual data for display, filled from "Dyna" template in NavigationController.
    /// </summary>
    [NotMapped]
    public List<Tuple<string, string>> DynaMenuItems {
      get;
      set; 
    }


    [NotMapped]
    public bool HasDynaMenu { get { return DynaMenuItems != null && DynaMenuItems.Any() && !String.IsNullOrEmpty(DynaTitle); } }

    # endregion

  }

}

