using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Common;

namespace Texxtoor.EasyAuthor.Models {
  public class ImprintAddressDto {

    [ScaffoldColumn(false)]
    public int ImprintId { get; set; }

    [ScaffoldColumn(false)]
    public int AddressId { get; set; }

    [ScaffoldColumn(false)]
    public int OwnerId { get; set; }

    [Required]
    [StringLength(64)]
    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Name_Name", Description = "Imprint_Name_Name_Helptext", Order= 1)]
    public string Name { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Description_Description", Description = "Imprint_Description_Description_Helptext", Order = 5)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 2)]
    [AdditionalMetadata("Cols", 55)]
    [StringLength(512)]
    public string Description { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_AboutUs_About_Us", Description = "Imprint_AboutUs_About_Us_Helptext", Order = 6)]
    [StringLength(2048)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 4)]
    [AdditionalMetadata("Cols", 55)]
    [Watermark(typeof(ModelResources), "Imprint_AboutUs_Watermark")]
    public string AboutUs { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Firm_Firm", Description = "Imprint_Firm_Firm_Helptext", Order = 2)]
    [Required]
    [StringLength(128)]
    public string Firm { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Url_Website", Description = "Imprint_Url_Website_Helptext", Order = 3)]
    [StringLength(128)]
    [Url(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ImprintAddress_Url_If_provided_it_must_match_URL_Format", ErrorMessage=null)]
    public string Url { get; set; }

    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_CompanyLogo_Logo", Description = "Imprint_CompanyLogo_Logo_Helptext")]
    public byte[] CompanyLogo { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_StreetNumber_Street", Description = "AddressBook_StreetNumber_Street_Helptext", Order = 20)]
    public string StreetNumber { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Zip_ZipCode", Description = "AddressBook_Zip_ZipCode_Helptext", Order = 30)]
    public string Zip { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_City_City", Description = "AddressBook_City_City_Helptext", Order = 40)]
    public string City { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Country_Country", Description = "AddressBook_Country_Country_Helptext", Order = 50)]
    [UIHint("CountryForCountryAndRegion")]
    public virtual Country Country { get; set; }

    // pulled from country --> city table
    [StringLength(100)]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Region_Region", Description = "AddressBook_Region_Region_Helptext", Order = 60)]
    [UIHint("RegionForCountryAndRegion")]
    [AdditionalMetadata("CountryProperty", "Country")]
    public string Region { get; set; }

    [ScaffoldColumn(false)]
    public int CountryId { get; set; }
  }
}