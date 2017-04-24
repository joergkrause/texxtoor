using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  [Table("ContributorMatrix", Schema = "Author")]
  public class ContributorMatrix : LocalizedEntityBase {

    [Required]
    [StringLength(64)]
    public string Name { get; set; }

    [StringLength(512)]
    public string Description { get; set; }

    /// <summary>
    /// This is a unique id that matches contributor types of different languages.
    /// </summary>
    /// <remarks>
    /// E.g., if we have a type "Author" with language "de" and a type "Author" with language "en" than both share the same TypeId.
    /// The TypeId value itself has no particular meaning and does not act as a key by itself.
    /// </remarks>
    [Required]
    public ContributorRole ContributorRole { get; set; }

    /// <summary>
    /// Active per language
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// This type can only be applied to a member if this member has this role (or better) in the particular project
    /// </summary>
    [Required]
    public UserRole MinimumRole { get; set; }

    [Required]
    public UserProfile Profile { get; set; }

  }

  


}
