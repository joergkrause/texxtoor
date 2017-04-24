using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;

namespace Texxtoor.ViewModels.Users
{
    public class LinkExistingAccount
    {
        [ScaffoldColumn(false)]
        public string UserName { get; set; }

        [ScaffoldColumn(false)]
        public string ExternalLoginData { get; set; }
    }
}
