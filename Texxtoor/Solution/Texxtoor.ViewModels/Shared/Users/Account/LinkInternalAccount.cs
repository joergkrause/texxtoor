using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;

namespace Texxtoor.ViewModels.Users
{
    public class LinkInternalAccount
    {
        [Required]
        [Display(ResourceType = typeof(ModelResources), Name = "LinkInternalAccount_UserName_User_name")]
        public string UserName { get; set; }

        [ScaffoldColumn(false)]
        public string ExternalLoginData { get; set; }

    }
}
