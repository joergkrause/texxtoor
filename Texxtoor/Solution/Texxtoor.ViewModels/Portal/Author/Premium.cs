using System.ComponentModel.DataAnnotations;

namespace Texxtoor.ViewModels.Author {

  public class Premium {

    [Required]
    [StringLength(7)]
    public string Code { get; set; }
    
    [Required]
    [StringLength(128)]
    public string Name { get; set; }
    
    [Required]
    [StringLength(128)]
    [EmailAddress]
    public string Email { get; set; }


  }
}
