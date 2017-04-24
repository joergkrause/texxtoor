using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Validation.Attributes;

namespace Texxtoor.DataModels.Models.Users {


  [DebuggerDisplay("Addresses [{Name} {User.Name]")]
  [Table("AddressBook", Schema="Common")]
  public class AddressBook : EntityBase {

    public AddressBook() {
      _canDelete = true;
    }

    [StringLength(100)]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Name_Name", Description="AddressBook_Name_Name_Helptext", Order = 1)]
    [Required]
    public string Name { get; set; }

    [StringLength(115)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_StreetNumber_Street", Description = "AddressBook_StreetNumber_Street_Helptext", Order = 2)]
    public string StreetNumber { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Zip_ZipCode", Description = "AddressBook_Zip_ZipCode_Helptext", Order = 3)]
    [ValidateZipCode(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "AddressBook_Zip_Invalid_Zip_Code")]
    public string Zip { get; set; }

    [StringLength(150)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_City_City", Description = "AddressBook_City_City_Helptext", Order = 4)]
    public string City { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Country_Country", Description = "AddressBook_Country_Country_Helptext", Order = 5)]
    [UIHint("CountryForCountryAndRegion")]
    public string Country { get; set; }

    // pulled from country --> city table
    [StringLength(100)]
    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Region_Region", Description = "AddressBook_Region_Region_Helptext", Order = 6)]
    [UIHint("RegionForCountryAndRegion")]
    [AdditionalMetadata("CountryProperty", "Country")]
    public string Region { get; set; }

    public UserProfile Profile { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Default_Default_Address", Description = "AddressBook_Default_Default_Address_Helptext", Order = 10)]
    [UIHint("Boolean_NotNull")]
    public bool Default { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "AddressBook_Invoice_Address_used_for_invoice", Description = "AddressBook_Invoice_Address_used_for_invoice_Helptext", Order = 11)]
    [UIHint("Boolean_NotNull")]
    public bool Invoice { get; set; }

    private bool _canDelete;
    
    public bool GetCanDelete() {
      return _canDelete;
    }

    public void SetCanDelete(bool val) {
      _canDelete = val;
    }

  }
}
