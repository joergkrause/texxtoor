using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Security;
using System.Web.Mvc;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Globalization;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Cms {

  [Table("Media", Schema = "Cms")]
  public class CmsMedia : LocalizedEntityBase {

    public string Name { get; set; }

    public byte[] Content { get; set; }
  }

}

