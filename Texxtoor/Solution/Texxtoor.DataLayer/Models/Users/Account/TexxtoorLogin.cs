using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Texxtoor.DataModels.Models.Users {
  
  [Table("UserLogins", Schema = "Common")]
  public class TexxtoorLogin : IdentityUserLogin<int> {
  }
}
