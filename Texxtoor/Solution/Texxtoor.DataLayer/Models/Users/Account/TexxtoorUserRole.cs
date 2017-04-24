using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Texxtoor.DataModels.Models.Users {
  
  // table name defined in PortalContext's model builder
  public class TexxtoorUserRole : IdentityUserRole<int> {


  }
}
