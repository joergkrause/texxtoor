using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.Models;
using Texxtoor.Models.BaseEntities;

namespace Texxtoor.Editor.Models {
  [Table("Terms")]
  public class Term : LocalizedEntityBase {

    /// <summary>
    /// Short text in editor.
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Text { get; set; }

    /// <summary>
    /// Long text that explains the value (optional)
    /// </summary>
    [StringLength(512)]
    public string Content { get; set; }

    public TermType TermType { get; set; }


  }



}