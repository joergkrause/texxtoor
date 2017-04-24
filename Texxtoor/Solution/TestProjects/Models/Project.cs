using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinqDemo.Models {

  [Table("Projects")]
  public class Project : EntityBase {

    public Project() {
      //Opuses = new List<Opus>();
    }

    /// <summary>
    /// Project Name
    /// </summary>
    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    /// <summary>
    /// Short description for overviews and lists. Can contain HTML.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Short { get; set; }

    /// <summary>
    /// Deactivate projects are not visible. Deleting will just deactivatin the project.
    /// </summary>
    [Display(Name = "Is active")]
    public bool Active { get; set; }

    /// <summary>
    /// Opus' of the project
    /// </summary>
    public virtual IList<Opus> Opuses { get; set; }


  }

}
