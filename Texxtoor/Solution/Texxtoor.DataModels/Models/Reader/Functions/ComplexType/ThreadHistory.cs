using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  [ComplexType]
  public class ThreadHistory {
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime Changed { get; set; }

    [Required]
    public string Content { get; set; }
  }

}