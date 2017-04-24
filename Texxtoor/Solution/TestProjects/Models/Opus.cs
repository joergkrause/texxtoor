using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace LinqDemo.Models {

  [Table("Elements")]
  public class Opus : Element {

    public Opus() {
    }

    [Required]
    public override string Name {
      get {
        {
          return base.Name;
        }
      }
      set { base.Name = value; }
    }


    public VariationType Variation { get; set; }

    # region Project Management

    [Required]
    public virtual Project Project { get; set; }

    [ScaffoldColumn(false)]
    public bool Active { get; set; }


    # endregion

    /// <summary>
    /// The version is just a counter. The version hierarchy is build using a parent relation to another opus
    /// </summary>
    [Range(0, 100)]
    [DefaultValue(1)]
    public int Version { get; set; }

    /// <summary>
    /// The relation of an element's content to its predecessors. 
    /// </summary>
    /// <remarks>
    /// On opus level it's used to organize documents. 
    /// </remarks>
    public virtual Opus PreviousVersion { get; set; }

  }

  public class OpusConfiguration : EntityTypeConfiguration<Opus> {
    public OpusConfiguration() {
      Property(x => x.Variation).IsRequired();
    }
  }

}
