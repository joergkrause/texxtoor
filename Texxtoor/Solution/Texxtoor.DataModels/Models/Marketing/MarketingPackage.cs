using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Texxtoor.DataModels.Models;
using System.ComponentModel.DataAnnotations;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.BaseLibrary.Core;
using System.Collections.ObjectModel;

namespace Texxtoor.DataModels.Models.Marketing {

  [Table("Package", Schema = "Marketing")]
  public class MarketingPackage : EntityBase {

    public MarketingPackage() {
      BasePrice = 0M;
      CreateRssFeed = true;
    }

    /// <summary>
    /// Package Name
    /// </summary>
    [Required]
    [StringLength(255)]
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_Name_Package_Name", Description = "MarketingPackage_Name_Package_Name_Helptext", Order = 1)]
    [FilterUIHint("StringFilter", "MVC")]
    [Watermark("Package's Name")]
    public string Name { get; set; }

    /// <summary>
    /// Let's the package describe itself
    /// </summary>
    [StringLength(1024)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_Description_Package_Description", Description = "MarketingPackage_Description_Package_Description_Helptext", Order = 2)]
    public string Description { get; set; }

    /// <summary>
    /// Percentage value, internally used to adjust pricing.
    /// </summary>
    [Range(0D, 1000D)]
    [DataType(DataType.Currency)]
    [AdditionalMetadata("Length", 6)]
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_BasePrice_Base_Price", Description = "MarketingPackage_BasePrice_Base_Price_Helptext", Order = 3)]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Assign a ISBN on each published work using this package.
    /// </summary>
    /// <remarks>
    /// This is "per package" and "per publish". If a reader pulls content from such a published work and re-creates a private book the ISBN
    /// is not longer associated with this new work. As far as the reader decides to create a copy for different media with all and legacy
    /// content in it, the ISBN remains, but each media will, if the distribution channel supports this, get it's very own ISBN.
    /// </remarks>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_AssignIsbn_Assign_ISBN", Description = "MarketingPackage_AssignIsbn_Assign_ISBN_Helptext", Order = 8)]
    public bool AssignIsbn { get; set; }

    /// <summary>
    /// Assign a ISBN-A (DOI based)
    /// </summary>
    /// <remarks>See sample at http://www.german-isbn.de/cgi-bin/isbn_2010.exe/showresolution?isbn13=978-3-7657-1538-9.
    /// This includes the DOI, used to manage cites for scientific works. http://www.medra.org/
    /// </remarks>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_AssignIsbnADOI_Assign_ADOI_ISBN", Description = "MarketingPackage_AssignIsbnADOI_Assign_ADOI_ISBN_Helptext", Order = 9)]
    public bool AssignIsbnADOI { get; set; }

    /// <summary>
    /// Register for libraries if possible and on a "per country" base.
    /// </summary>
    /// <remarks>
    /// Depends on countries, such as "VLB" in Germany, Books-In-Print in the US and so on.
    /// </remarks>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_RegisterForLibraries_Register_for_Library", Description = "MarketingPackage_RegisterForLibraries_Register_for_Library_Helptext", Order = 7)]
    public bool RegisterForLibraries { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool HasLimitCountries {
      get {
        return LimitCountries != null && LimitCountries.Any();
      }
    }

    /// <summary>
    /// Once published a RSS feed becomes available. Default is true.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_CreateRssFeed_Create_Feed", Description = "MarketingPackage_CreateRssFeed_Create_Feed_Helptext", Order = 5)]
    public bool CreateRssFeed { get; set; }

    /// <summary>
    /// Create a page for this book on all social platforms the lead author has a relation to. Behavior depends on user's profile.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_CreateSocialPlatformInstances_Create_Platform_Support", Description = "MarketingPackage_CreateSocialPlatformInstances_Create_Platform_Support_Helptext", Order = 6)]
    public bool CreateSocialPlatformInstances { get; set; }

    /// <summary>
    /// Some packages may have price. The price is assigned through business logic and the book can be published only once the lead author has paid.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_PackageBasePrice_Package_Price", Description = "MarketingPackage_PackageBasePrice_Package_Price_Helptext", Order = 4)]
    [DataType(DataType.Currency)]
    [Range(0D, 1000D)]
    [ScaffoldColumn(false)]
    public decimal PackageBasePrice { get; set; }

    # region -== Manage Social Networks ==-

    /// <summary>
    /// Manages the publishing channels.
    /// </summary>
    [UIHint("MarketingTypes")]
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_MarketingType_Marketing_Package_Type", Description = "MarketingPackage_MarketingType_Marketing_Package_Type_Helptext", Order = 100)]
    public MarketingPackageType MarketingType { get; set; }

    public string GetLocalizedMarketingType() {
       return typeof(MarketingPackageType)
         .GetField(MarketingType.ToString())
         .GetCustomAttributes(typeof(DisplayAttribute), true)
         .Cast<DisplayAttribute>()
         .Single()
         .GetName();
    }

    /// <summary>
    /// Allow others to use the content. Requires to become a team member and negotiate a share. 
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_ShareContent_Content_Sharing_Permitted", Description = "MarketingPackage_ShareContent_Content_Sharing_Permitted_Helptext", Order = 10)]
    public bool ShareContent { get; set; }

    # endregion -== Manage Social Networks ==-

    /// <summary>
    /// The user who has created and own this package. A list of previously used packages is shown for convenience.
    /// </summary>
    public User Owner { get; set; }

    private ObservableCollection<string> _limitCountries;

    /// <summary>
    /// By default each book is available in all countries. Use this to limit to specific countries.
    /// </summary>
    [UIHint("Country")]
    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "MarketingPackage_LimitCountries_Limit_Countries", Description = "MarketingPackage_LimitCountries_Limit_Countries_Helptext", Order = 15)]
    public ObservableCollection<string> LimitCountries {
      get {
        if (String.IsNullOrEmpty(LimitCountriesContainer)) {
          LimitCountriesContainer = String.Empty;
        }
        _limitCountries = new ObservableCollection<string>(LimitCountriesContainer.Split(new [] { '|' }, StringSplitOptions.RemoveEmptyEntries));
        _limitCountries.CollectionChanged += (o, e) => {
          LimitCountriesContainer = _limitCountries == null ? String.Empty : String.Join("|", _limitCountries);
        };
        return _limitCountries;
      }
      set {
        _limitCountries = value;
        LimitCountriesContainer = _limitCountries == null ? String.Empty : String.Join("|", value);
      }
    }

    [ScaffoldColumn(false)]
    public string LimitCountriesContainer { get; set; }

    /// <summary>
    /// Manually mapped if needed.
    /// </summary>
    [ScaffoldColumn(false)]
    [NotMapped]
    public IList<Project> AssignedProjects { get; set; }

  }
}
