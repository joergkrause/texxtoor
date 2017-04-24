using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models {


  [Table("ContactFormRequest", Schema = "Common")]
  public class ContactFormRequest : EntityBase {

    [Required]
    [Display(Name = "User")]
    public User Sender { get; set; }

    [Required]
    [Display(Name = "Your Name")]
    [StringLength(256)]
    public string Name { get; set; }

    [Required]
    [Display(Name = "Your E-Mail reply address")]
    [StringLength(256)]
    public string EMail { get; set; }

    [Required]
    [Display(Name = "Subject")]
    [StringLength(512)]
    public string Subject { get; set; }

    [Required]
    [Display(Name = "Your message")]
    public string Message { get; set; }


  }
}
