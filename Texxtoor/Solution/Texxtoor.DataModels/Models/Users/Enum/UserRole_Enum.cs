using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models {

  /// <summary>
  /// The global role of the user - used to control the UI.
  /// </summary>
  public enum UserRole {
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_Guest_Guest", Description="UserRole_Guest_Guest_Helptext")]
    Guest = 0,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_Reader_Reader", Description="UserRole_Reader_Reader_Helptext")]
    Reader = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_Contributor_Contributor", Description="UserRole_Contributor_Contributor_Helptext")]
    Contributor = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_Author_Author", Description="UserRole_Author_Author_Helptext")]
    Author = 3,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_TeamLead_Team_Lead", Description="UserRole_TeamLead_Team_Lead_Helptext")]
    TeamLead = 4,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_Admin_Admin", Description="UserRole_Admin_Admin_Helptext")]
    Admin = 9,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_CmsAdmin_Cms_Admin", Description="UserRole_CmsAdmin_Cms_Admin_Helptext")]
    CmsAdmin = 10,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_TenantAdmin_Tenant_s_Admin", Description="UserRole_TenantAdmin_Tenant_s_Admin_Helptext")]
    TenantAdmin = 12,
    [Display(ResourceType = typeof(ModelResources), Name = "UserRole_Unknown_Unknown", Description="UserRole_Unknown_Unknown_Helptext")]
    Unknown = 99
  }

}