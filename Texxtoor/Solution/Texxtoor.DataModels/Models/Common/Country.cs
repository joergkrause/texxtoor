using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using System.Collections.Generic;

namespace Texxtoor.DataModels.Models.Common {

  [Table("Country", Schema = "Common")]
  public class Country : EntityBase {

    [Required]
    [StringLength(3)]
    public string IsoCode { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(125)]
    public string LocalName { get; set; }
    
    [StringLength(80)]
    public string Continent { get; set; }
    
    [StringLength(80)]
    public string Region { get; set; }
    
    public long GNP { get; set; }

    public long Population { get; set; }
    
    public double SurfaceArea { get; set; }

    public virtual City Capital { get; set; }

    public virtual List<City> Cities { get; set; }

    public List<Language> Languages { get; set; }

    [Required]
    [StringLength(2)]
    public string Iso2Code { get; set; }
  }
}
