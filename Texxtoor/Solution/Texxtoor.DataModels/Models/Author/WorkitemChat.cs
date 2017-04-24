using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Functions;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// Save private comments for snippets. Lead author can see contributors comments if they made the comments public.
  /// </summary>
  [Table("WorkitemChat", Schema = "Content")]
  public class WorkitemChat : Discussion<WorkitemChat> {

    [Required]
    public Element Snippet { get; set; }
  }

}